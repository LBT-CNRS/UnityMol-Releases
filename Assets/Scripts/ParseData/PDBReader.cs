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
// New classes to handle PDB format
// Joao Rodrigues: j.p.g.l.m.rodrigues@gmail.com
// Xavier Martinez: xavier.martinez.xm@gmail.com
//
// Classes:
//   Fetch: requests/downloads a PDB file from RCSB.org
//   ReadData: parses a PDB string
//   Write writes a structure in PDB format to a string

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
using System.Globalization;

namespace UMol {

/// <summary>
/// Creates a UnityMolStructure object from a local or remote PDB file
/// </summary>
public class PDBReader: Reader {

    public static string[] PDBextensions = {"pdb", "pdb.gz", "ent", "pqr"};

    // private string PDBServer = "http://files.rcsb.org/download/";
    //The pdb files are recorded as .../all/pdb/pdb1kx2.ent.gz
    private string PDBServer = "https://ftp.wwpdb.org/pub/pdb/data/structures/all/pdb/pdb";

    public PDBReader(string fileName = "", string PDBServer = ""): base(fileName)
    {
        if (PDBServer != "") {
            this.PDBServer = PDBServer;
        }
    }

    //Version with a coroutine
    public IEnumerator Fetch(string entryCode, System.Action<UnityMolStructure> result, bool readHet = true, bool readWater = true) {

        // string extension = ".pdb.gz";
        string extension = ".ent.gz";
        string entryCodeLow = entryCode.ToLower();


        this.fileName = entryCode + extension;
        updateFileNames();

        string entryURL = PDBServer + entryCodeLow + extension;
        Debug.Log("Fetching " + entryCode);

        WWW webResponse = new WWW(entryURL);
        yield return webResponse;

        UnityMolStructure structure = null;
        if (!string.IsNullOrEmpty(webResponse.error)) {
            Debug.Log("Error reading remote URL: " + webResponse.error);
        }
        else {
            // PDB returns a gzip'ed binary file
            using(MemoryStream byteStream = new MemoryStream(webResponse.bytes))
            using(GZipStream flatStream = new GZipStream(byteStream, CompressionMode.Decompress))
            using (StreamReader sr = new StreamReader(flatStream)) {
                try {
                    structure = ReadData(sr, readHet, readWater);
                }
                catch (ParsingException err) {
                    Debug.LogError("Something went wrong when parsing the PDB file: " + err);
                }
            }
        }
        result(structure);
    }

    //Blocking version
    public UnityMolStructure Fetch(string EntryCode, bool readHet = true, bool readWater = true) {

        // string extension = ".pdb.gz";
        string extension = ".ent.gz";
        string EntryCodeLow = EntryCode.ToLower();


        string entryURL = PDBServer + EntryCodeLow + extension;
        this.fileName = EntryCode + extension;
        updateFileNames();
        Debug.Log("Fetching remote file: " + entryURL);

        UnityMolStructure structure = null;

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(entryURL);

        using(HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
            using(Stream stream = response.GetResponseStream()) {
                using(GZipStream flatStream = new GZipStream(stream, CompressionMode.Decompress))
                using(StreamReader reader = new StreamReader(flatStream))
                {

                    try {
                        structure = ReadData(reader, readHet, readWater);
                    }
                    catch (ParsingException err) {
                        Debug.LogError("Something went wrong when parsing your PDB file: " + err);
                    }
                    return structure;
                }
            }
        }

    }


    /// <summary>
    /// Parses a PDB file to a UnityMolStructure object
    /// ignoreStructureM flag is used to avoid adding the structure into managers and just returns a UnityMolStructure
    /// </summary>
    protected override UnityMolStructure ReadData(StreamReader sr, bool readHET, bool readWater, bool ignoreStructureM = false) {
        float start = Time.realtimeSinceStartup;

        // When parsing PDBs it makes sense to filter/parse data line by line
        // as this is the purpose of the format.

        bool readConnect = true;

        List<UnityMolModel> models = new List<UnityMolModel>();
        HashSet<string> residueAtoms = new HashSet<string>(); // to check for double definitions
        List<secStruct> parsedSSList = new List<secStruct>();
        List<Int2> chemBonds = new List<Int2>();
        List<Int2> bondedAtoms = new List<Int2>();
        List<Vector3[]> frames = new List<Vector3[]>();


        using (sr) { // Don't use garbage collection but free temp memory after reading the pdb file

            string line;
            StringBuilder debug = new StringBuilder();

            List<UnityMolChain> chains = new List<UnityMolChain>();
            List<UnityMolResidue> residues = new List<UnityMolResidue>();
            List<UnityMolAtom> atomsList = new List<UnityMolAtom>();
            List<UnityMolAtom> allAtoms = new List<UnityMolAtom>();

            Vector3[] curFrame = null;

            int curModel = 0;
            int countAtomsInModels = 0;
            int atomCounter = 0;
            string lastChain = null;
            string lastResidue = null;
            int lastResidueId = -1;

            string currentLine = null;
            string alternFirst = null;
            int cptAltern = 0;
            int lineNumber = 0;
            int idA = 0;
            while ((line = sr.ReadLine()) != null) {
                lineNumber++;
                try {
                    if (!string.IsNullOrWhiteSpace(line)) {
                        currentLine = line;
                        if (currentLine.Length > 3) {
                            bool isAtomLine = QuickStartWith(currentLine, "ATOM");
                            bool isHetAtm = QuickStartWith(currentLine, "HETATM");
                            bool isChemBond = QuickStartWith(currentLine, "CHEMBOND");

                            if (!readHET && isHetAtm) {
                                continue;
                            }

                            if (isAtomLine || isHetAtm) {

                                if (modelsAsTraj && models.Count == 1) {
                                    if (idA >= countAtomsInModels || idA < 0) {
                                        idA = -1;
                                        continue;
                                    }
                                    //Unity has a left-handed coordinates system while PDBs are right-handed
                                    float px = -float.Parse(currentLine.Substring(30, 8).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                    float py = float.Parse(currentLine.Substring(38, 8).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                    float pz = float.Parse(currentLine.Substring(46, 8).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                    curFrame[idA] = new Vector3(px, py, pz);
                                    idA++;
                                    continue;
                                }

                                // Skip Waters?
                                string resName = currentLine.Substring(17, 4).Trim();
                                if (!readWater && WaterSelection.waterResidues.Contains(resName, StringComparer.OrdinalIgnoreCase)) {
                                    continue;
                                }

                                int atomSerial = int.Parse(currentLine.Substring(6, 5));
                                string atomName = currentLine.Substring(12, 4).Trim();
                                string atomChain = currentLine.Substring(21, 1);
                                int resNum = int.Parse(currentLine.Substring(22, 4));
                                string insertCode = currentLine.Substring(26, 1);
                                bool hasInsertCode = !String.IsNullOrWhiteSpace(insertCode);
                                string initResName = resName;
                                if (hasInsertCode) {
                                    //     //Change residue number using residue insertion
                                    //     resNum = resNum - 1000 + (char.ToUpper(insertCode[0]) - 64);
                                    //Change residue name using residue insertion
                                    resName = resName + "_" + insertCode;
                                }

                                //Unity has a left-handed coordinates system while PDBs are right-handed
                                float sx = -float.Parse(currentLine.Substring(30, 8).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                float sy = float.Parse(currentLine.Substring(38, 8).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                float sz = float.Parse(currentLine.Substring(46, 8).Trim(), System.Globalization.CultureInfo.InvariantCulture);

                                string altern = currentLine.Substring(16, 1).Trim();
                                if (altern != "" && alternFirst == null) {
                                    alternFirst = altern;
                                }

                                Vector3 coord = new Vector3(sx, sy, sz);
                                float bfactor = 0.0f;
                                if (currentLine.Length >= 67 &&
                                        float.TryParse(currentLine.Substring(60, 6), NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out bfactor)) {
                                }


                                string atomElement = "";
                                // // Atom Type is linked to Atom Element
                                try {
                                    atomElement = currentLine.Substring(76, 2).Trim();
                                    if (String.IsNullOrEmpty(atomElement) || !UnityMolMain.atomColors.isKnownAtom(atomElement)) {
                                        atomElement = GuessElementFromAtomName(atomName, initResName, isHetAtm);
                                    }
                                }
                                catch {
                                    //Use the first letter of the atom name
                                    atomElement = GuessElementFromAtomName(atomName, initResName, isHetAtm);
                                }


                                if (atomChain == " ") {
                                    atomChain = "_";
                                }


                                // Check for continuity of the chain
                                // And for atom/residue heterogeneity (partial occupancies, insertion codes, ...)
                                // If this is the case, ignore the atom and send a warning to the logger.
                                //
                                if (atomCounter > 0) { // skip all tests on first atom ...
                                    if (atomChain == lastChain) { // same chain
                                        if (resNum == lastResidueId && !hasInsertCode) { // same residue number
                                            if (resName != lastResidue) { // is insertion?
                                                debug.Append(String.Format("Residue number {0} on chain {1} defined multiple times\n", resNum, atomChain));
                                                continue;
                                            }
                                            else { // is atom name already registered? (partial occupancy)

                                                if (residueAtoms.Contains(atomName)) {
                                                    if (altern != "" && altern != alternFirst) {

                                                        if (cptAltern < 20) {
                                                            debug.Append(String.Format("Residue {0}{1} already contains atom {2}. Ignoring alternative position\n", resName, resNum, atomName));
                                                        }

                                                        cptAltern++;
                                                        continue;
                                                    }
                                                    else {
                                                        string newAtomName = findNewAtomName(residueAtoms, atomName);
                                                        debug.Append(String.Format("Residue {0}{1} already contains atom {2}. Changing name to {3}\n", resName, resNum, atomName, newAtomName));
                                                        // debug.Append(String.Format("Residue {0}{1} already contains atom {2}\n", resName, resNum, atomName));
                                                        atomName = newAtomName;
                                                        // continue;
                                                        residueAtoms.Add(atomName);
                                                    }
                                                }
                                                else {
                                                    residueAtoms.Add(atomName);
                                                }
                                            }
                                        }
                                        else { // different residue number (new residue)
                                            residueAtoms.Clear();
                                            if (atomsList.Count > 0) {
                                                residues.Add(new UnityMolResidue(lastResidueId, atomsList, lastResidue));
                                                for (int a = 0; a < atomsList.Count; a++) {
                                                    atomsList[a].SetResidue(residues.Last());
                                                }
                                                atomsList.Clear();
                                            }

                                            // Do we have a chain break?
                                            if (resNum - lastResidueId > 1 && !hasInsertCode) {
                                                debug.Append(String.Format("Chain {0} discontinuous at residue {1}\n", atomChain, resNum));
                                            }
                                            residueAtoms.Add(atomName);
                                        }
                                    }
                                    else { // different chain identifier (new chain)
                                        residueAtoms.Clear();
                                        if (atomsList.Count > 0) { // New Residue = record the previous one
                                            residues.Add(new UnityMolResidue(lastResidueId, atomsList, lastResidue));
                                            for (int a = 0; a < atomsList.Count; a++) {
                                                atomsList[a].SetResidue(residues.Last());
                                            }
                                            atomsList.Clear();
                                        }
                                        if (residues.Count > 0) { //New Chain = record the previous one
                                            chains.Add(new UnityMolChain(residues, lastChain));
                                            for (int r = 0; r < residues.Count; r++) {
                                                residues[r].chain = chains.Last();
                                            }
                                            residues.Clear();
                                        }
                                        residueAtoms.Add(atomName);
                                    }
                                }

                                else { // ... but still catch first atom name
                                    residueAtoms.Add(atomName);
                                }

                                UnityMolAtom newAtom = new UnityMolAtom(atomName, atomElement, coord, bfactor, atomSerial, isHetAtm);
                                newAtom.isLigand = isLigand(initResName);

                                atomsList.Add(newAtom);
                                allAtoms.Add(newAtom);

                                lastChain = atomChain;
                                lastResidueId = resNum;
                                lastResidue = resName;
                                atomCounter++;
                            }
                            if (isChemBond) {
                                string[] splitedStringTemp = currentLine.Split(' ');
                                if (splitedStringTemp.Length == 3) {
                                    Int2 pair;
                                    pair.x = int.Parse(splitedStringTemp[1]);
                                    pair.y = int.Parse(splitedStringTemp[2]);
                                    chemBonds.Add(pair);
                                }
                            }
                        }
                        if (currentLine.Length >= 6 && QuickStartWith(currentLine, "ENDMDL")) { // New Model

                            if (modelsAsTraj) {
                                if (frames.Count == 0) { //First frame
                                    countAtomsInModels = allAtoms.Count;
                                    curFrame = new Vector3[countAtomsInModels];
                                    //Record the first frame
                                    for (int i = 0; i < countAtomsInModels; i++) {
                                        curFrame[i] = allAtoms[i].position;
                                    }
                                    frames.Add(curFrame);
                                }
                                else if (idA == countAtomsInModels) {
                                    frames.Add(curFrame);
                                }
                                else {
                                    Debug.LogWarning("Ignoring model, number of atoms differ from the first model, try to set modelsAsTraj to false");
                                }
                                curFrame = new Vector3[countAtomsInModels];
                            }
                            idA = 0;

                            // Record last residue and last chain
                            if (atomsList.Count > 0) {
                                UnityMolResidue newRes = new UnityMolResidue(lastResidueId, atomsList, lastResidue);
                                residues.Add(newRes);
                                for (int a = 0; a < atomsList.Count; a++) {
                                    atomsList[a].SetResidue(newRes);
                                }
                                atomsList.Clear();
                            }
                            if (residues.Count > 0) {
                                chains.Add(new UnityMolChain(residues, lastChain));
                                for (int r = 0; r < residues.Count; r++) {
                                    residues[r].chain = chains.Last();
                                }
                                residues.Clear();
                            }
                            if (chains.Count > 0) {
                                // Record the model
                                UnityMolModel model = new UnityMolModel(chains, curModel.ToString());
                                model.allAtoms.AddRange(allAtoms);
                                allAtoms.Clear();
                                models.Add(model);
                                chains.Clear();
                                curModel++;
                            }

                            lastChain = null;
                            lastResidue = null;
                            lastResidueId = -1;
                        }


                        // HELIX Secondary Structure Records
                        else if (currentLine.Length >= 5 &&  QuickStartWith(currentLine, "HELIX")) {
                            string chainH = currentLine.Substring (19, 2).Trim();
                            int startRes = int.Parse(currentLine.Substring(21, 4));
                            int endRes = int.Parse(currentLine.Substring (33, 4));
                            int classH = int.Parse(currentLine.Substring(38, 2));

                            secStruct newHelix; newHelix.start = startRes; newHelix.end = endRes;
                            newHelix.chain = chainH; newHelix.type = (UnityMolResidue.secondaryStructureType)classH;
                            parsedSSList.Add(newHelix);

                        }
                        // SHEET Secondary Structure Records
                        else if (currentLine.Length >= 5 &&  QuickStartWith(currentLine, "SHEET")) {
                            string chainS = currentLine.Substring (21, 2).Trim();
                            int startRes = int.Parse(currentLine.Substring (23, 3));

                            int endRes = int.Parse(currentLine.Substring (34, 3));

                            secStruct newSheet; newSheet.start = startRes; newSheet.end = endRes;
                            newSheet.chain = chainS; newSheet.type = UnityMolResidue.secondaryStructureType.Strand;
                            parsedSSList.Add(newSheet);

                        }
                        else if (readConnect && QuickStartWith(currentLine, "CONECT")) {

                            Int2 pair;
                            int rootAtom = int.Parse(currentLine.Substring(6, 5));
                            int bondedA = int.Parse(currentLine.Substring(11, 5));

                            pair.x = rootAtom;
                            pair.y = bondedA;
                            bondedAtoms.Add(pair);


                            Int2 bond;
                            if (currentLine.Length >= 22) {
                                // Not all atoms are bonded to 1+ others
                                string bondedB = currentLine.Substring(16, 5).Trim();
                                if (bondedB != "") {
                                    int _bondedB = int.Parse(bondedB);
                                    bond.x = rootAtom;
                                    bond.y = _bondedB;
                                    bondedAtoms.Add(bond);
                                }
                            }
                            if (currentLine.Length >= 27) {
                                string bondedC = currentLine.Substring(21, 5).Trim();
                                if (bondedC != "") {
                                    int _bondedC = int.Parse(bondedC);
                                    bond.x = rootAtom;
                                    bond.y = _bondedC;
                                    bondedAtoms.Add(bond);
                                }
                            }
                            if (currentLine.Length >= 32) {
                                string bondedD = currentLine.Substring(26, 5).Trim();
                                if (bondedD != "") {
                                    int _bondedD = int.Parse(bondedD);
                                    bond.x = rootAtom;
                                    bond.y = _bondedD;
                                    bondedAtoms.Add(bond);
                                }
                            }
                        }
                    }
                }
                catch (Exception e) {
                    string message = "Parser failed while reading line " + lineNumber.ToString() + "=> '" + currentLine + "'";
                    throw new ParsingException(message, e);
                }
            }


            if (debug.Length != 0) {
                Debug.LogWarning(debug.ToString());
            }

            // Record last residue and last chain
            if (atomsList.Count > 0) {
                residues.Add(new UnityMolResidue(lastResidueId, atomsList, lastResidue));
                for (int a = 0; a < atomsList.Count; a++) {
                    atomsList[a].SetResidue(residues.Last());
                }
                atomsList.Clear();
            }
            if (residues.Count > 0) {
                chains.Add(new UnityMolChain(residues, lastChain));
                for (int r = 0; r < residues.Count; r++) {
                    residues[r].chain = chains.Last();
                }
                residues.Clear();
            }
            if (chains.Count > 0) {
                //Record the model
                UnityMolModel model = new UnityMolModel(chains, curModel.ToString());
                model.allAtoms.AddRange(allAtoms);
                allAtoms.Clear();
                models.Add(model);
                curModel++;
            }
        }

        if (models.Count == 0) {
            throw new System.Exception("PDB parsing error");
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

        if (bondedAtoms.Count != 0) {
            newStruct.parsedConnectivity = bondedAtoms;
        }

        for (int i = 0; i < models.Count; i++) {
            newStruct.models[i].structure = newStruct;

            if (!ignoreStructureM) {
                // newStruct.models[i].bonds = ComputeUnityMolBonds.ComputeBondsSlidingWindow(models[i].allAtoms);
                newStruct.models[i].bonds = ComputeUnityMolBonds.ComputeBondsByResidue(models[i].allAtoms);

                newStruct.models[i].ComputeCenterOfGravity();
                // newStruct.models[i].CenterAtoms();
                newStruct.models[i].fillIdAtoms();

                if (chemBonds.Count != 0) {
                    newStruct.models[i].customChemBonds.AddRange(chemBonds);
                }
            }
        }

        if (!ignoreStructureM) {

            FillSecondaryStructure(newStruct, parsedSSList);
            newStruct.parsedSSInfo = parsedSSList;
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


    public static UnityMolStructure ParseFromString(string pdbContent) {
        PDBReader pdbr = new PDBReader();
        UnityMolStructure res = null;
        byte[] byteArray = Encoding.UTF8.GetBytes(pdbContent);
        using (MemoryStream sr = new MemoryStream(byteArray)) {
            StreamReader reader = new StreamReader(sr, System.Text.Encoding.UTF8, true);
            pdbr.ReadData(reader, readHET: true, readWater: true, ignoreStructureM: true);
        }
        return res;
    }

    /// <summary>
    /// PDB writer
    /// Uses a structure and outputs a string containing all the models
    /// Ignores secondary structure information
    /// </summary>
    public static string Write(UnityMolStructure structure) {
        StringBuilder sw = new StringBuilder();

        foreach (UnityMolModel m in structure.models) {
            sw.Append(Write(m.ToSelection(), true));
        }
        sw.Append("END");
        return sw.ToString();
    }

    /// <summary>
    /// PDB writer
    /// Uses a selection
    /// Ignores secondary structure information
    /// </summary>
    public static string Write(UnityMolSelection select, bool writeModel = false, bool writeHET = true, bool forceHetAsAtom = false, Vector3[] overridedPos = null, bool writeSS = false) {

        if (overridedPos != null && select.atoms.Count != overridedPos.Length) {
            Debug.LogError("Size of the overridedPos list does not match the number of atoms in the selections");
            return "";
        }

        // ATOM/HETATM
        string pdbString = "{0,-6}{1, 5} {2, 4}{3, 1}{4, 3} {5, 1}{6, 4}{7, 1}"; // insCode
        pdbString += "   {8,8:N3}{9,8:N3}{10,8:N3}{11,6:N2}{12,6:N2}          {13,2}{14,2}\n";

        // TER
        string terString = "TER   {0, 5}      {1,3} {2,1}{3,4}{4,1}\n";

        StringBuilder sw = new StringBuilder();

        string prevChain = null;
        int atomSerial = 0;

        List<UnityMolAtom> atoms = select.atoms;

        if (writeModel) {
            sw.Append("MODEL\n");
        }

        for (int i = 0; i < atoms.Count; i++) {
            UnityMolAtom atom = atoms[i];
            if (atom.isHET && !writeHET) {
                continue;
            }
            string resName = atom.residue.name;
            resName = resName.Substring(0, Mathf.Min(3, resName.Length));


            bool isWater = WaterSelection.waterResidues.Contains(resName, StringComparer.OrdinalIgnoreCase);
            string atomRecordType = "ATOM  ";
            if (!forceHetAsAtom && (atom.isHET || isWater)) {
                atomRecordType = "HETATM";
            }

            atomSerial++;
            string atomName = atom.name.Substring(0, Mathf.Min(4, atom.name.Length));
            atomName = formatAtomName(atomName);
            string altLoc = " "; // We do not store it yet.

            int resNum = atom.residue.id;

            string chainId = atom.residue.chain.name;
            chainId = chainId.Substring(0, 1);
            string insCode = " "; // We do not store it yet.

            float x = -1 * atom.oriPosition.x; // Revert to right-handed
            float y = atom.oriPosition.y;
            float z = atom.oriPosition.z;

            if (overridedPos != null) {
                x = -1 * overridedPos[i].x;
                y = overridedPos[i].y;
                z = overridedPos[i].z;
            }

            float occupancy = 1.0f;
            float Bfactor = atom.bfactor;
            string element = atom.type;
            string charge = " "; // Nothing for now.

            if (chainId != prevChain) {
                if (prevChain != null) {
                    string prevResName = atoms[i - 1].residue.name;
                    int prevResNum = atoms[i - 1].residue.id;
                    sw.Append(String.Format(terString, atomSerial, prevResName, prevChain, prevResNum, insCode));
                    atomSerial++;
                }
                prevChain = chainId;
            }

            sw.Append(String.Format(CultureInfo.InvariantCulture, pdbString, atomRecordType, atomSerial, atomName.CenterString(4, ' '),
                                    altLoc, resName, chainId, resNum, insCode, x, y, z,
                                    occupancy, Bfactor, element, charge));
            if (atomSerial > 99999) {
                Debug.LogError("Cannot write a file with more than 99999 atoms. Stopping");
                break;
            }
            if (resNum >= 9999) {
                Debug.LogWarning("Cannot write a file with more than 9999 residues. Stopping");
                break;
            }
        }
        if (!writeModel) {
            sw.Append("END\n");
        }
        else {
            sw.Append("ENDMDL\n");
        }
        if (writeSS) {
            writeSecondaryStructure(select, sw);
        }

        return sw.ToString();
    }
    public static void writeSecondaryStructure(UnityMolSelection sel, StringBuilder sw) {
        if (sel.structures.Count == 1) {
            int nbHelix = 0;
            int nbSheet = 0;
            int curResIdStart = -1;
            string curResNameStart = "";
            UnityMolResidue.secondaryStructureType curSSType = UnityMolResidue.secondaryStructureType.Helix;

            bool inHelix = false;

            string pdbStringHelix = "HELIX  {0,3} {1,3} {2,2}{3,2} {4,4}  {5,2} {6,1} {7,4} {8,2}{9,36}\n"; // insCode
            string pdbStringSheet = "SHEET  {0,3} {1,3} 0 {2,3} {3,1}{4,4}  {5,3} {6,1}{7,4} \n"; // insCode

            UnityMolResidue lastR = null;
            UnityMolChain lastC = null;

            foreach (UnityMolChain c in sel.structures[0].currentModel.chains.Values) {
                foreach (UnityMolResidue r in c.residues.Values) {
                    if (r.secondaryStructure == UnityMolResidue.secondaryStructureType.Helix ||
                            r.secondaryStructure == UnityMolResidue.secondaryStructureType.HelixRightOmega ||
                            r.secondaryStructure == UnityMolResidue.secondaryStructureType.HelixRightPi ||
                            r.secondaryStructure == UnityMolResidue.secondaryStructureType.HelixRightGamma ||
                            r.secondaryStructure == UnityMolResidue.secondaryStructureType.Helix310 ||
                            r.secondaryStructure == UnityMolResidue.secondaryStructureType.HelixLeftAlpha ||
                            r.secondaryStructure == UnityMolResidue.secondaryStructureType.HelixLeftOmega ||
                            r.secondaryStructure == UnityMolResidue.secondaryStructureType.HelixLeftGamma ||
                            r.secondaryStructure == UnityMolResidue.secondaryStructureType.Helix27) {

                        if (!inHelix) { //Helix start
                            curResNameStart = r.name;
                            curResIdStart = r.id;
                            curSSType = r.secondaryStructure;
                            nbHelix++;
                            inHelix = true;
                        }
                        else if (r.secondaryStructure != curSSType) {
                            sw.Append(String.Format(pdbStringHelix, nbHelix, nbHelix, curResNameStart, c.name, curResIdStart, r.name, c.name, r.id, (int)curSSType, (r.id - curResIdStart)));

                            curResNameStart = r.name;
                            curResIdStart = r.id;
                            curSSType = r.secondaryStructure;
                            nbHelix++;
                            inHelix = true;
                        }
                    }
                    else if (inHelix) { //End of helix
                        sw.Append(String.Format(pdbStringHelix, nbHelix, nbHelix, curResNameStart, c.name, curResIdStart, r.name, c.name, r.id, (int)curSSType, (r.id - curResIdStart)));
                        inHelix = false;
                    }
                    lastR = r;
                }
                if (inHelix && lastR != null) { //End of chain but still in helix
                    sw.Append(String.Format(pdbStringHelix, nbHelix, nbHelix, curResNameStart, c.name, curResIdStart, lastR.name, c.name, lastR.id, (int)curSSType, (lastR.id - curResIdStart)));
                    inHelix = false;
                }
                lastC = c;
            }
            if (inHelix && lastC != null) {
                sw.Append(String.Format(pdbStringHelix, nbHelix, nbHelix, curResNameStart, lastC.name, curResIdStart, lastR.name, lastC.name, lastR.id, (int)curSSType, (lastR.id - curResIdStart)));
                inHelix = false;
            }

            bool inSheet = false;
            foreach (UnityMolChain c in sel.structures[0].currentModel.chains.Values) {
                foreach (UnityMolResidue r in c.residues.Values) {
                    if (r.secondaryStructure == UnityMolResidue.secondaryStructureType.Strand) {
                        if (!inSheet) { //Start sheet
                            curResNameStart = r.name;
                            curResIdStart = r.id;
                            nbSheet++;
                            inSheet = true;
                        }
                    }
                    else if (inSheet) { //End sheet
                        sw.Append(String.Format(pdbStringSheet, nbSheet, nbSheet, curResNameStart, c.name, curResIdStart, r.name, c.name, r.id));
                        inSheet = false;
                    }

                    lastR = r;
                }
                if (inSheet) {
                    sw.Append(String.Format(pdbStringSheet, nbSheet, nbSheet, curResNameStart, c.name, curResIdStart, lastR.name, c.name, lastR.id));
                    inSheet = false;
                }
                lastC = c;
            }

            if (inSheet) {
                sw.Append(String.Format(pdbStringSheet, nbSheet, nbSheet, curResNameStart, lastC.name, curResIdStart, lastR.name, lastC.name, lastR.id));
                inSheet = false;
            }
        }
    }


    public static bool isLigand(string resName) {
        //TODO: do something smarter here
        return (resName.ToUpper() == "LIG");
    }


    /// <summary>
    /// Guess atom element from atom name
    /// By default, if the element cannot be guessed, returns X.
    /// </summary>
    public static string GuessElementFromAtomName(string atomName, string resName, bool isHET) {

        if (atomName.Length == 1) {
            return atomName;
        }
        if (atomName == "CA" && !isHET) {
            return "C";
        }

        if (UnityMolMain.atomColors.isKnownAtom(atomName.ToUpper())) {
            return atomName;
        }

        string first = atomName[0].ToString();

        if (!isHET && UnityMolMain.atomColors.isKnownAtom(first.ToUpper())) {
            return first;
        }

        if (atomName.Length >= 2 && System.Char.IsDigit(atomName[0]) && atomName[1] == 'H') {
            return "H";
        }

        string firstTwo = atomName.Substring(0, 2);
        bool endsWithDigits = false;
        if (atomName.Length >= 3) {
            int placeholderInt;
            endsWithDigits = int.TryParse(atomName.Substring(2), out placeholderInt);
        }
        if (resName == atomName || (!endsWithDigits && UnityMolMain.atomColors.isKnownAtom(firstTwo))) {
            return firstTwo;
        }
        if (UnityMolMain.atomColors.isKnownAtom(first.ToUpper())) {
            return first;
        }


        return "X";
    }

    //TODO: Improve this
    private static string formatAtomName(string name) {
        if (name.Length == 1)
            return " " + name + "  ";
        if (name.Length == 2)
            return " " + name + " ";
        if (name.Length == 3)
            return " " + name;
        return name;
    }

    /// <summary>
    /// Add atoms to a structure using pdb lines
    /// Atoms are only added to the current model of the structure
    /// </summary>
    public static void AddToStructure(string lines, UnityMolStructure structure) {
        StreamReader reader = new StreamReader(
            new MemoryStream(Encoding.ASCII.GetBytes(lines)));
        PDBReader r = new PDBReader ();

        UnityMolStructure s = r.ReadData(reader, readHET: true, readWater: true, ignoreStructureM: true);

        List<UnityMolAtom> toBeAdded = s.ToSelectionAll().atoms;
        List<UnityMolAtom> prevAtoms = structure.ToSelectionAll().atoms;
        Debug.Log("Adding " + toBeAdded.Count + " atoms to " + structure.uniqueName);

        foreach (UnityMolAtom a in toBeAdded) {
            //Residue of atom "a" has to be set
            structure.AddAtom(a, a.residue.chain.model.name, a.residue.chain.name);
        }

        //Update bonds and offsets
        structure.currentModel.bonds = ComputeUnityMolBonds.ComputeBondsByResidue(structure.currentModel.allAtoms);
        structure.currentModel.ComputeCenterOfGravity();
        structure.currentModel.fillIdAtoms();


        //Need to create colliders for newly added atoms
        CreateColliders(new UnityMolSelection(toBeAdded, newBonds: null, structure.ToSelectionName(), structure.uniqueName));
        //Remove existing pre computed representations
        UnityMolMain.getPrecompRepManager().Clear(structure.uniqueName);

    }

}
}