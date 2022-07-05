/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

        Xavier Martinez, 2017-2021
        Marc Baaden, 2010-2021
        baaden@smplinux.de
        http://www.baaden.ibpc.fr

        This software is a computer program based on the Unity3D game engine.
        It is part of UnityMol, a general framework whose purpose is to provide
        a prototype for developing molecular graphics and scientific
        visualisation applications. More details about UnityMol are provided at
        the following URL: "http://unitymol.sourceforge.net". Parts of this
        source code are heavily inspired from the advice provided on the Unity3D
        forums and the Internet.

        This program is free software: you can redistribute it and/or modify
        it under the terms of the GNU General Public License as published by
        the Free Software Foundation, either version 3 of the License, or
        (at your option) any later version.

        This program is distributed in the hope that it will be useful,
        but WITHOUT ANY WARRANTY; without even the implied warranty of
        MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
        GNU General Public License for more details.

        You should have received a copy of the GNU General Public License
        along with this program. If not, see <https://www.gnu.org/licenses/>.

        References : 
        If you use this code, please cite the following reference :         
        Z. Lv, A. Tek, F. Da Silva, C. Empereur-mot, M. Chavent and M. Baaden:
        "Game on, Science - how video game technology may help biologists tackle
        visualization challenges" (2013), PLoS ONE 8(3):e57990.
        doi:10.1371/journal.pone.0057990
       
        If you use the HyperBalls visualization metaphor, please also cite the
        following reference : M. Chavent, A. Vanel, A. Tek, B. Levy, S. Robert,
        B. Raffin and M. Baaden: "GPU-accelerated atom and dynamic bond visualization
        using HyperBalls, a unified algorithm for balls, sticks and hyperboloids",
        J. Comput. Chem., 2011, 32, 2924

    Please contact unitymol@gmail.com
    ================================================================================
*/


//
// New classes to handle GRO format
// Xavier Martinez
//
// Classes:
//   ReadData: parses a GRO string
//   TODO:
//   WriteGROToString: writes a structure in GRO format to a string

// Unity Classes
using UnityEngine;

// C# Classes
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Net;

namespace UMol {

/// <summary>
/// Creates a UnityMolStructure object from a local GRO file
/// </summary>
public class GROReader: Reader {

    static readonly string[] chainNames = new[] {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N",
            "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI",
            "AJ", "AK", "AL", "AM", "AN", "AO", "AP", "AQ", "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ", "BA", "BB",
            "BC", "BD", "BE", "BF", "BG", "BH"
                                                };

    public static string[] GROextensions = {"gro"};

    public GROReader(string fileName = ""): base(fileName) {}

    /// <summary>
    /// Parses a GRO file file to a Molecule object
    /// </summary>
    protected override UnityMolStructure ReadData(StreamReader sr, bool readHET, bool readWater, bool simplyParse = false) {
        float start = Time.realtimeSinceStartup;

        List<UnityMolModel> models = new List<UnityMolModel>();
        List<UnityMolChain> chains = new List<UnityMolChain>();
        List<UnityMolResidue> residues = new List<UnityMolResidue>();
        List<UnityMolAtom> atomsList = new List<UnityMolAtom>();
        List<UnityMolAtom> allAtoms = new List<UnityMolAtom>();
        List<Vector3[]> frames = new List<Vector3[]>();

        bool normalPosParse = true;
// readWater=false;

        int lineNumber = 0;
        StringBuilder debug = new StringBuilder();


        using (sr) { // Don't use garbage collection but free temp memory after reading the pdb file

            // Read frames as trajectory frames
            if (modelsAsTraj) {
                Vector3[] curFrame = null;

                HashSet<int> doneResidues = new HashSet<int>();

                bool onlyReadPos = false;
                int countAtomsInModels = 0;
                int idA = 0;
                string line;
                int curModel = 0;
                int curChain = 0;
                int lastResidue = -1;
                int prevAtomSerial = -1;
                int atomSerialAdd = 0;
                string lastResidueName = null;

                while ((line = sr.ReadLine()) != null) {

                    if (line.Contains("t=")) {
                        if (onlyReadPos) {
                            if (countAtomsInModels != 0 && idA == countAtomsInModels) {
                                frames.Add(curFrame);
                            }
                            else {
                                debug.Append("Something went wrong when reading frame before line " + lineNumber);
                            }
                            curFrame = new Vector3[countAtomsInModels];
                            idA = 0;
                        }
                        if (atomsList.Count > 0 ) {
                            if (!onlyReadPos) { //First frame end
                                onlyReadPos = true;
                                idA = 0;
                                countAtomsInModels = allAtoms.Count;
                                curFrame = new Vector3[countAtomsInModels];
                                for (int i = 0; i < countAtomsInModels; i++) {
                                    curFrame[i] = allAtoms[i].position;
                                }
                                frames.Add(curFrame);
                                curFrame = new Vector3[countAtomsInModels];

                                residues.Add(new UnityMolResidue(lastResidue, atomsList, lastResidueName));
                                for (int a = 0; a < atomsList.Count; a++) {
                                    atomsList[a].SetResidue(residues.Last());
                                }
                                atomsList.Clear();


                                if (residues.Count > 0) {
                                    string nameChain = "_";
                                    if (curChain < chainNames.Length)
                                        nameChain = chainNames[curChain];

                                    UnityMolChain c = new UnityMolChain(residues, nameChain);
                                    chains.Add(c);
                                    for (int r = 0; r < residues.Count; r++) {
                                        residues[r].chain = c;
                                    }
                                    curChain++;
                                    residues.Clear();
                                }
                                if (chains.Count > 0) {
                                    //Record the model
                                    UnityMolModel model = new UnityMolModel(chains, curModel.ToString());
                                    model.allAtoms.AddRange(allAtoms);
                                    allAtoms.Clear();
                                    chains.Clear();

                                    models.Add(model);
                                    chains.Clear();
                                    curModel++;
                                    prevAtomSerial = -1;
                                }
                            }
                        }
                    }

                    else if (line.Length >= 39) { //Atom line

                        if (onlyReadPos) {
                            float posx, posy, posz;

                            if (normalPosParse) {
                                //Position in Angstrom + Unity has x inverted
                                posx = -float.Parse(line.Substring(20, 8).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                posy = float.Parse(line.Substring(28, 8).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                posz = float.Parse(line.Substring(36, 8).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                            }
                            else {
                                string[] fields = line.Substring(20).Split(new [] { ' '}, System.StringSplitOptions.RemoveEmptyEntries);
                                posx = -float.Parse(fields[0], System.Globalization.CultureInfo.InvariantCulture);
                                posy = float.Parse(fields[1], System.Globalization.CultureInfo.InvariantCulture);
                                posz = float.Parse(fields[2], System.Globalization.CultureInfo.InvariantCulture);
                            }

                            Vector3 coord = new Vector3(posx, posy, posz) * 10.0f;//gro file format unit is nm not Angstrom
                            if (idA >= 0 && idA > countAtomsInModels) {
                                curFrame[idA] = coord;
                            }
                            idA++;

                        }
                        else {
                            try {
                                int resnb = int.Parse(line.Substring(0, 5).Trim());
                                string resName = line.Substring(5, 5).Trim();
                                string atomName = line.Substring(10, 5).Trim();
                                int atomSerial = int.Parse(line.Substring(15, 5).Trim());


                                float posx, posy, posz;

                                if (!readWater && WaterSelection.waterResidues.Contains(resName, StringComparer.OrdinalIgnoreCase)) {
                                    continue;
                                }
                                bool isHet = (resName == "HEC");

                                if (!readHET && isHet) {
                                    continue;
                                }

                                string type = PDBReader.GuessElementFromAtomName(atomName, resName, isHet);

                                if (allAtoms.Count == 0) {
                                    try {
                                        //Position in angstrom + Unity has x inverted
                                        posx = -float.Parse(line.Substring(20, 8).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                        posy = float.Parse(line.Substring(28, 8).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                        posz = float.Parse(line.Substring(36, 8).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                        normalPosParse = true;
                                    }
                                    catch { //Test to parse with a split approach
                                        string[] fields = line.Substring(20).Split(new [] { ' '}, System.StringSplitOptions.RemoveEmptyEntries);
                                        posx = -float.Parse(fields[0], System.Globalization.CultureInfo.InvariantCulture);
                                        posy = float.Parse(fields[1], System.Globalization.CultureInfo.InvariantCulture);
                                        posz = float.Parse(fields[2], System.Globalization.CultureInfo.InvariantCulture);
                                        normalPosParse = false;
                                    }
                                }
                                else {
                                    if (normalPosParse) {
                                        //Position in Angstrom + Unity has x inverted
                                        posx = -float.Parse(line.Substring(20, 8).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                        posy = float.Parse(line.Substring(28, 8).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                        posz = float.Parse(line.Substring(36, 8).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                    else {
                                        string[] fields = line.Substring(20).Split(new [] { ' '}, System.StringSplitOptions.RemoveEmptyEntries);
                                        posx = -float.Parse(fields[0], System.Globalization.CultureInfo.InvariantCulture);
                                        posy = float.Parse(fields[1], System.Globalization.CultureInfo.InvariantCulture);
                                        posz = float.Parse(fields[2], System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                }

                                Vector3 coord = new Vector3(posx, posy, posz) * 10.0f;//gro file format unit is nm not Angstrom


                                if (atomsList.Count != 0 && lastResidue != resnb) { // New residue
                                    if (doneResidues.Contains(lastResidue)) { //Residue number already done = new Chain
                                        if (residues.Count > 0) {
                                            string nameChain = "_";
                                            if (curChain < chainNames.Length)
                                                nameChain = chainNames[curChain];
                                            chains.Add(new UnityMolChain(residues, nameChain));
                                            for (int r = 0; r < residues.Count; r++) {
                                                residues[r].chain = chains.Last();
                                            }
                                            residues.Clear();
                                            curChain++;
                                        }
                                        doneResidues.Clear();
                                    }


                                    residues.Add(new UnityMolResidue(lastResidue, atomsList, lastResidueName));
                                    for (int a = 0; a < atomsList.Count; a++) {
                                        atomsList[a].SetResidue(residues.Last());
                                    }
                                    atomsList.Clear();

                                    doneResidues.Add(lastResidue);
                                }

                                int modifAtomSerial = atomSerial;
                                if (prevAtomSerial == 99999 && atomSerial < 10) {
                                    atomSerialAdd += 100000;
                                }
                                modifAtomSerial = atomSerialAdd + atomSerial;

                                float bfactor = 0.0f;

                                UnityMolAtom newAtom = new UnityMolAtom(atomName, type, coord, bfactor, modifAtomSerial, isHet);
                                atomsList.Add(newAtom);
                                allAtoms.Add(newAtom);

                                lastResidue = resnb;
                                lastResidueName = resName;
                                prevAtomSerial = atomSerial;
                            }
                            catch {
                                debug.Append(String.Format("Ignoring line {0} : {1}\n", (lineNumber + 1), line));
                                // Debug.LogWarning("Ignoring line " + (lineNumber + 1) + " : " + line);
                            }
                        }
                    }
                    lineNumber++;
                }

                if (debug.Length != 0) {
                    Debug.LogWarning(debug.ToString());
                }


                if (!onlyReadPos) {//No frames to record
                    // Record last residue and last chain
                    if (atomsList.Count > 0) {

                        residues.Add(new UnityMolResidue(lastResidue, atomsList, lastResidueName));
                        for (int a = 0; a < atomsList.Count; a++) {
                            atomsList[a].SetResidue(residues.Last());
                        }
                        atomsList.Clear();


                    }

                    if (residues.Count > 0) {
                        string nameChain = "_";
                        if (curChain < chainNames.Length)
                            nameChain = chainNames[curChain];

                        UnityMolChain c = new UnityMolChain(residues, nameChain);
                        chains.Add(c);
                        for (int r = 0; r < residues.Count; r++) {
                            residues[r].chain = c;
                        }
                        curChain++;
                        residues.Clear();
                    }
                    if (chains.Count > 0) {
                        //Record the model
                        UnityMolModel model = new UnityMolModel(chains, curModel.ToString());
                        model.allAtoms.AddRange(allAtoms);
                        allAtoms.Clear();
                        chains.Clear();

                        models.Add(model);
                        chains.Clear();
                        curModel++;
                        prevAtomSerial = -1;
                    }
                }
                else if (idA == countAtomsInModels) { //Record as new frame
                    frames.Add(curFrame);
                    curFrame = new Vector3[countAtomsInModels];
                }

            }
            // Read GRO frames as new models
            else {
                HashSet<int> doneResidues = new HashSet<int>();

                string line;
                int curModel = 0;
                int curChain = 0;
                int lastResidue = -1;
                int prevAtomSerial = -1;
                int atomSerialAdd = 0;
                string lastResidueName = null;

                while ((line = sr.ReadLine()) != null) {

                    if (line.Contains("t=")) {
                        if (atomsList.Count > 0 ) { //New model
                            //Save previously recorded atoms to residue
                            residues.Add(new UnityMolResidue(lastResidue, atomsList, lastResidueName));
                            for (int a = 0; a < atomsList.Count; a++) {
                                atomsList[a].SetResidue(residues.Last());
                            }
                            atomsList.Clear();
                        }


                        if (residues.Count > 0) {
                            string nameChain = "_";
                            if (curChain < chainNames.Length)
                                nameChain = chainNames[curChain];

                            UnityMolChain c = new UnityMolChain(residues, nameChain);
                            chains.Add(c);
                            for (int r = 0; r < residues.Count; r++) {
                                residues[r].chain = c;
                            }
                            residues.Clear();
                            doneResidues.Clear();
                            curChain++;
                        }
                        if (chains.Count > 0) {
                            //Record the model
                            UnityMolModel model = new UnityMolModel(chains, curModel.ToString());
                            model.allAtoms.AddRange(allAtoms);
                            allAtoms.Clear();
                            chains.Clear();
                            models.Add(model);
                            curModel++;
                            curChain = 0;
                        }

                    }

                    else if (line.Length >= 39) { //Atom line
                        try {
                            int resnb = int.Parse(line.Substring(0, 5).Trim());
                            string resName = line.Substring(5, 5).Trim();
                            string atomName = line.Substring(10, 5).Trim();
                            int atomSerial = int.Parse(line.Substring(15, 5).Trim());


                            float posx, posy, posz;

                            if (!readWater && WaterSelection.waterResidues.Contains(resName, StringComparer.OrdinalIgnoreCase)) {
                                continue;
                            }
                            bool isHet = (resName == "HEC");

                            if (!readHET && isHet) {
                                continue;
                            }

                            string type = PDBReader.GuessElementFromAtomName(atomName, resName, isHet);

                            if (allAtoms.Count == 0) {
                                try {
                                    //Position in angstrom + Unity has x inverted
                                    posx = -float.Parse(line.Substring(20, 8).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                    posy = float.Parse(line.Substring(28, 8).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                    posz = float.Parse(line.Substring(36, 8).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                    normalPosParse = true;
                                }
                                catch { //Test to parse with a split approach
                                    string[] fields = line.Substring(20).Split(new [] { ' '}, System.StringSplitOptions.RemoveEmptyEntries);
                                    posx = -float.Parse(fields[0], System.Globalization.CultureInfo.InvariantCulture);
                                    posy = float.Parse(fields[1], System.Globalization.CultureInfo.InvariantCulture);
                                    posz = float.Parse(fields[2], System.Globalization.CultureInfo.InvariantCulture);
                                    normalPosParse = false;
                                }
                            }
                            else {
                                if (normalPosParse) {
                                    //Position in Angstrom + Unity has x inverted
                                    posx = -float.Parse(line.Substring(20, 8).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                    posy = float.Parse(line.Substring(28, 8).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                    posz = float.Parse(line.Substring(36, 8).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                }
                                else {
                                    string[] fields = line.Substring(20).Split(new [] { ' '}, System.StringSplitOptions.RemoveEmptyEntries);
                                    posx = -float.Parse(fields[0], System.Globalization.CultureInfo.InvariantCulture);
                                    posy = float.Parse(fields[1], System.Globalization.CultureInfo.InvariantCulture);
                                    posz = float.Parse(fields[2], System.Globalization.CultureInfo.InvariantCulture);
                                }
                            }

                            Vector3 coord = new Vector3(posx, posy, posz) * 10.0f;//gro file format unit is nm not Angstrom


                            if (atomsList.Count != 0 && lastResidue != resnb) { // New residue
                                if (doneResidues.Contains(lastResidue)) { //Residue number already done = new Chain
                                    if (residues.Count > 0) {
                                        string nameChain = "_";
                                        if (curChain < chainNames.Length)
                                            nameChain = chainNames[curChain];
                                        chains.Add(new UnityMolChain(residues, nameChain));
                                        for (int r = 0; r < residues.Count; r++) {
                                            residues[r].chain = chains.Last();
                                        }
                                        residues.Clear();
                                        curChain++;
                                    }
                                    doneResidues.Clear();
                                }


                                residues.Add(new UnityMolResidue(lastResidue, atomsList, lastResidueName));
                                for (int a = 0; a < atomsList.Count; a++) {
                                    atomsList[a].SetResidue(residues.Last());
                                }
                                atomsList.Clear();

                                doneResidues.Add(lastResidue);
                            }

                            int modifAtomSerial = atomSerial;
                            if (prevAtomSerial == 99999 && atomSerial < 10) {
                                atomSerialAdd += 100000;
                            }
                            modifAtomSerial = atomSerialAdd + atomSerial;

                            float bfactor = 0.0f;

                            UnityMolAtom newAtom = new UnityMolAtom(atomName, type, coord, bfactor, modifAtomSerial, isHet);
                            atomsList.Add(newAtom);
                            allAtoms.Add(newAtom);

                            lastResidue = resnb;
                            lastResidueName = resName;
                            prevAtomSerial = atomSerial;
                        }
                        catch {
                            debug.Append(String.Format("Ignoring line {0} : {1}\n", (lineNumber + 1), line));
                            // Debug.LogWarning("Ignoring line " + (lineNumber + 1) + " : " + line);
                        }
                    }
                    lineNumber++;
                }

                if (debug.Length != 0) {
                    Debug.LogWarning(debug.ToString());
                }


                // Record last residue and last chain
                if (atomsList.Count > 0) {
                    residues.Add(new UnityMolResidue(lastResidue, atomsList, lastResidueName));
                    for (int a = 0; a < atomsList.Count; a++) {
                        atomsList[a].SetResidue(residues.Last());
                    }
                    atomsList.Clear();
                }
                if (residues.Count > 0) {
                    string nameChain = "_";
                    if (curChain < chainNames.Length)
                        nameChain = chainNames[curChain];

                    UnityMolChain c = new UnityMolChain(residues, nameChain);
                    chains.Add(c);
                    for (int r = 0; r < residues.Count; r++) {
                        residues[r].chain = c;
                    }
                    curChain++;
                    residues.Clear();
                }
                if (chains.Count > 0) {
                    //Record the model
                    UnityMolModel model = new UnityMolModel(chains, curModel.ToString());
                    model.allAtoms.AddRange(allAtoms);
                    allAtoms.Clear();
                    chains.Clear();

                    models.Add(model);
                    chains.Clear();
                    curModel++;
                    prevAtomSerial = -1;
                }
            }
        }

        UnityMolStructure newStruct = null;
        if (frames.Count != 0) {
            newStruct = new UnityMolStructure(models, this.fileNameWithoutExtension, frames);
        }
        else {
            newStruct = new UnityMolStructure(models, this.fileNameWithoutExtension);
        }
        identifyStructureMolecularType(newStruct);

        if (newStruct.structureType != UnityMolStructure.MolecularType.standard) {
            newStruct.updateAtomRepValues();
        }

        for (int i = 0; i < models.Count; i++) {
            newStruct.models[i].structure = newStruct;
        }


        if (!simplyParse) {
            for (int i = 0; i < models.Count; i++) {
                // newStruct.models[i].bonds = ComputeUnityMolBonds.ComputeBondsSlidingWindow(models[i].allAtoms);
                newStruct.models[i].bonds = ComputeUnityMolBonds.ComputeBondsByResidue(models[i].allAtoms);
                newStruct.models[i].ComputeCenterOfGravity();
                // newStruct.models[i].CenterAtoms();
                newStruct.models[i].fillIdAtoms();
            }

            UnityMolSelection sel = newStruct.ToSelection();

            if (newStruct.models.Count != 1) {
                for (int i = 1; i < newStruct.models.Count; i++) {
                    CreateColliders(new UnityMolSelection(newStruct.models[i].allAtoms, newBonds: null, sel.name, newStruct.uniqueName));
                }
            }
            CreateColliders(sel);
            newStruct.surfThread = startSurfaceThread(sel);

            UnityMolMain.getStructureManager().AddStructure(newStruct);
            UnityMolMain.getSelectionManager().Add(sel);
        }
        
#if UNITY_EDITOR
        Debug.Log("Time for parsing: " + (1000.0f * (Time.realtimeSinceStartup - start)).ToString("f3") + " ms");
#endif
        return newStruct;
    }
}
}
