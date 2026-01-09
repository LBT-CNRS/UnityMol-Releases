/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

        Xavier Martinez, 2017-2022
        Hubert Santuz, 2022-2026
        Marc Baaden, 2010-2026
        unitymol@gmail.com
        https://unity.mol3d.tech/

        This file is part of UnityMol, a general framework whose purpose is to provide
        a prototype for developing molecular graphics and scientific
        visualisation applications based on the Unity3D game engine.
        More details about UnityMol are provided at the following URL: https://unity.mol3d.tech/

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

        To help us with UnityMol development, we ask that you cite
        the research papers listed at https://unity.mol3d.tech/cite-us/.
    ================================================================================
*/
//
// New classes to handle PDB format
// Joao Rodrigues: j.p.g.l.m.rodrigues@gmail.com
// Xavier Martinez: xavier.martinez.xm@gmail.com
// Hubert Santuz: hubert.santuz@ibpc.fr
//

// Unity Classes
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Networking;

// C# Classes
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;



namespace UMol {

/// <summary>
/// Create a UnityMolStructure object from a local or remote PDB file
/// </summary>
public class PDBReader: Reader {

	/// <summary>
	/// List of possible PDB extensions
	/// </summary>
    public static readonly string[] PDBextensions = {"pdb", "pdb.gz", "ent", "pqr"};

	/// <summary>
	/// URL address of the PDB Server to fetch the data
	/// </summary>
    private readonly string PDBServer = "https://files.wwpdb.org/pub/pdb/data/structures/all/pdb/pdb";

	/// <summary>
	/// Create a PDBReader with an optional filename and a optional PDBserver URL address
	/// </summary>
    public PDBReader(string fileName = "", string pdbServer = ""): base(fileName)
    {
        if (pdbServer != "") {
            PDBServer = pdbServer;
        }
    }

    /// <summary>
    /// Fetch a PDB file from RCSB servers and parse the data (coroutine version)
    /// </summary>
    public IEnumerator Fetch(string entryCode, Action<UnityMolStructure> result, bool readHet = true, bool readWater = true, int forceType = -1) {

        // string extension = ".pdb.gz";
        string extension = ".ent.gz";
        string entryCodeLow = entryCode.ToLower();


        FileName = entryCode + extension;
        UpdateFileNames();

        string entryURL = PDBServer + entryCodeLow + extension;
        Debug.Log("Fetching " + entryCode);

        UnityWebRequest request = UnityWebRequest.Get(entryURL);
        yield return request.SendWebRequest();

        if (request.isNetworkError) {
            Debug.Log("Error reading remote URL: " + request.error);
        } else {
            yield return Fetch_URL(entryURL, readHet, readWater, false, forceType);
        }
    }

    /// <summary>
    /// Fetch a PDB file from RCSB servers and parse the data (blocking version)
    /// </summary>
    public UnityMolStructure Fetch(string entryCode, bool readHet = true, bool readWater = true, int forceType = -1) {

        const string extension = ".ent.gz";
        string entryURL = PDBServer + entryCode.ToLower() + extension;
        FileName = entryCode + extension;

        UpdateFileNames();

        Debug.Log("Fetching remote file: " + entryURL);

        return Fetch_URL(entryURL, readHet, readWater, false, forceType);
    }


    /// <summary>
    /// Parse a PDB file contained in a StreamReader to a UnityMolStructure object
    /// </summary>
    protected override UnityMolStructure ReadData(StreamReader sr, bool readHET = true, bool readWater = true,
            bool simplyParse = false, UnityMolStructure.MolecularType? forceStructureType = null) {

        float start = Time.realtimeSinceStartup;

        // When parsing PDBs it makes sense to filter/parse data line by line
        // as this is the purpose of the format.


        List<UnityMolModel> models = new();
        HashSet<string> residueAtoms = new(); // to check for double definitions of atoms
        List<SecStruct> parsedSSList = new();
        List<int2> chemBonds = new();
        List<int2> bondedAtoms = new();
        List<Vector3[]> frames = new();
        List<bool> ignoreAtom = new();// atoms to ignore during modelAsTraj pass
        List<Matrix4x4> matrices = new();//symetry matrices

        Vector3 periodic = Vector3.one;

        using (sr) { // Don't use garbage collection but free temp memory after reading the pdb file

            string line;
            StringBuilder debug = new();
            StringBuilder sbtrim = new(10);

            List<UnityMolChain> chains = new();
            List<UnityMolResidue> residues = new();
            List<UnityMolAtom> atomsList = new();
            List<UnityMolAtom> allAtoms = new();

            Vector3[] curFrame = null;

            bool tooMuchDebug = false;

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
            int offsetidA = 0;
            int resNum = 0;

            int curSymId = 0;
            Vector3 invertX = new(-1.0f, 1.0f, 1.0f);
            Matrix4x4 curMatrix = Matrix4x4.identity;

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
                            if (QuickStartWith(currentLine, "CRYST1")) {
                                float perx, pery, perz;
                                TryParseFloatFast(currentLine, 7, 7 + 8, out perx);
                                TryParseFloatFast(currentLine, 16, 16 + 8, out pery);
                                TryParseFloatFast(currentLine, 25, 25 + 8, out perz);
                                periodic = new Vector3(perx, pery, perz);
                            }

                            if (isAtomLine || isHetAtm) {

                                if (ModelsAsTraj && models.Count == 1) {
                                    if (idA >= ignoreAtom.Count || idA < 0) {
                                        idA = -1;
                                        continue;
                                    }

                                    if (ignoreAtom[idA]) {
                                        idA++;
                                        offsetidA++;
                                        continue;
                                    }

                                    float px, py, pz;
                                    TryParseFloatFast(currentLine, 30, 30 + 8, out px);
                                    TryParseFloatFast(currentLine, 38, 38 + 8, out py);
                                    TryParseFloatFast(currentLine, 46, 46 + 8, out pz);
                                    //Unity has a left-handed coordinates system while PDBs are right-handed
                                    curFrame[idA - offsetidA] = new Vector3(-px, py, pz);
                                    idA++;
                                    continue;
                                }

                                // Skip Waters?
                                string resName = SubstringWithTrim(sbtrim, currentLine, 17, 4);
                                if (!readWater && WaterSelection.waterResidues.Contains(resName, StringComparer.OrdinalIgnoreCase)) {
                                    continue;
                                }

                                bool atomSerialOnlyDigit = OnlyDigits(currentLine, 6, 6 + 5);

                                int atomSerial;
                                if (atomSerialOnlyDigit) {
                                    atomSerial = ParseInt(currentLine, 6, 6 + 5);
                                } else {
                                    atomSerial = ParseIntH36(currentLine, 5, 6, 6 + 5);
                                }

                                string atomName = SubstringWithTrim(sbtrim, currentLine, 12, 4);
                                //TODO: PDBH36 should be column 21 and 22
                                string atomChain = currentLine.Substring(21, 1);

                                bool residOnlyDigit = OnlyDigits(currentLine, 22, 22 + 4);
                                int resid;
                                if (residOnlyDigit) {
                                    resid = ParseInt(currentLine, 22, 22 + 4);
                                }
                                else {
                                    resid = ParseIntH36(currentLine, 4, 22, 22 + 4);
                                }


                                string insertCode = currentLine.Substring(26, 1);
                                bool hasInsertCode = !String.IsNullOrWhiteSpace(insertCode);
                                string initResName = resName;
                                if (hasInsertCode) {
                                    //     //Change residue number using residue insertion (other method)
                                    //     resid = resid - 1000 + (char.ToUpper(insertCode[0]) - 64);
                                    //Change residue name using residue insertion
                                    resName = resName + "_" + insertCode;
                                }

                                //Unity has a left-handed coordinates system while PDBs are right-handed
                                float sx, sy, sz;
                                TryParseFloatFast(currentLine, 30, 30 + 8, out sx);
                                TryParseFloatFast(currentLine, 38, 38 + 8, out sy);
                                TryParseFloatFast(currentLine, 46, 46 + 8, out sz);
                                Vector3 coord = new(-sx, sy, sz);


                                string altern = SubstringWithTrim(sbtrim, currentLine, 16, 1);
                                if (altern != "" && alternFirst == null) {
                                    alternFirst = altern;
                                }

                                float bfactor = 0.0f;
                                if (currentLine.Length >= 67) {
                                    // float.TryParse(currentLine.Substring(60, 6), NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out bfactor);
                                    TryParseFloatFast(currentLine, 60, 60 + 6, out bfactor);
                                }


                                string atomElement;
                                // // Atom Type is linked to Atom Element
                                try {
                                    atomElement = SubstringWithTrim(sbtrim, currentLine, 76, 2);
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
                                        if (resid == lastResidueId && !hasInsertCode) { // same residue number
                                            if (resName != lastResidue) { // not same name => create a new residue
                                                residueAtoms.Clear();

                                                if (atomsList.Count > 0) {
                                                    residues.Add(new UnityMolResidue(resNum, lastResidueId, atomsList, lastResidue));
                                                    resNum++;
                                                    foreach (UnityMolAtom a in atomsList) {
                                                        a.SetResidue(residues.Last());
                                                    }
                                                    atomsList.Clear();
                                                }

                                                // Do we have a chain break?
                                                if (resid - lastResidueId > 1 && !hasInsertCode && !isHetAtm) {
                                                    debug.AppendFormat("Chain {0} discontinuous at residue {1}\n", atomChain, resid);
                                                }
                                                residueAtoms.Add(atomName);

                                                debug.AppendFormat("Residue number {0} on chain {1} defined multiple times with different names consecutively\n", resid, atomChain);
                                            }
                                            else {

                                                // is atom name already registered? (partial occupancy)
                                                if (residueAtoms.Contains(atomName)) {
                                                    if (altern != "" && altern != alternFirst) {
                                                        if (cptAltern < 20) {
                                                            if (debug.Length < 3000) {
                                                                debug.AppendFormat("Residue {0}{1} already contains atom {2}. Ignoring alternative position\n", resName, resid, atomName);
                                                            }
                                                            else if (!tooMuchDebug) {
                                                                tooMuchDebug = true;
                                                                debug.Append("Too many warnings\n");
                                                            }
                                                        }
                                                        ignoreAtom.Add(true);
                                                        cptAltern++;
                                                        continue;
                                                    }
                                                    else {
                                                        string newAtomName = FindNewAtomName(residueAtoms, atomName);
                                                        if (debug.Length < 3000) {
                                                            debug.AppendFormat("Residue {0}{1} already contains atom {2}. Changing name to {3}\n", resName, resid, atomName, newAtomName);
                                                        }
                                                        else if (!tooMuchDebug) {
                                                            tooMuchDebug = true;
                                                            debug.Append("Too many warnings\n");
                                                        }
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
                                        else { // different residue number

                                            residueAtoms.Clear();

                                            if (atomsList.Count > 0) {
                                                residues.Add(new UnityMolResidue(resNum, lastResidueId, atomsList, lastResidue));
                                                resNum++;
                                                foreach (UnityMolAtom a in atomsList) {
                                                    a.SetResidue(residues.Last());
                                                }
                                                atomsList.Clear();
                                            }

                                            // Do we have a chain break?
                                            if (resid - lastResidueId > 1 && !hasInsertCode && !isHetAtm) {
                                                if (debug.Length < 1000) {
                                                    debug.AppendFormat("Chain {0} discontinuous at residue {1}\n", atomChain, resid);
                                                }
                                                else if (!tooMuchDebug) {
                                                    tooMuchDebug = true;
                                                    debug.Append("Too many warnings\n");
                                                }
                                            }
                                            residueAtoms.Add(atomName);

                                        }
                                    }
                                    else { // different chain identifier (new chain)
                                        residueAtoms.Clear();
                                        if (atomsList.Count > 0) { // New Residue = record the previous one
                                            residues.Add(new UnityMolResidue(resNum, lastResidueId, atomsList, lastResidue));
                                            resNum++;
                                            foreach (UnityMolAtom a in atomsList) {
                                                a.SetResidue(residues.Last());
                                            }
                                            atomsList.Clear();

                                        }
                                        if (residues.Count > 0) { //New Chain = record the previous one
                                            chains.Add(new UnityMolChain(residues, lastChain));
                                            foreach (UnityMolResidue r in residues) {
                                                r.chain = chains.Last();
                                            }
                                            residues.Clear();
                                        }
                                        residueAtoms.Add(atomName);
                                    }
                                }

                                else { // ... but still catch first atom name
                                    residueAtoms.Add(atomName);
                                }

                                UnityMolAtom newAtom = new(atomName, atomElement, coord, bfactor, atomSerial, isHetAtm);
                                newAtom.isLigand = IsLigand(initResName);

                                atomsList.Add(newAtom);
                                allAtoms.Add(newAtom);

                                lastChain = atomChain;
                                lastResidueId = resid;
                                lastResidue = resName;
                                atomCounter++;
                                ignoreAtom.Add(false);
                            }
                            if (isChemBond) {
                                string[] splitedStringTemp = currentLine.Split(' ');
                                if (splitedStringTemp.Length == 3) {
                                    int2 pair;
                                    pair.x = int.Parse(splitedStringTemp[1]);
                                    pair.y = int.Parse(splitedStringTemp[2]);
                                    chemBonds.Add(pair);
                                }
                            }
                        }
                        if (currentLine.Length >= 6 && QuickStartWith(currentLine, "ENDMDL")) { // New Model

                            if (ModelsAsTraj) {
                                if (frames.Count == 0) { //First frame
                                    countAtomsInModels = allAtoms.Count;
                                    curFrame = new Vector3[countAtomsInModels];
                                    //Record the first frame
                                    for (int i = 0; i < countAtomsInModels; i++) {
                                        curFrame[i] = allAtoms[i].position;
                                    }
                                    frames.Add(curFrame);
                                }
                                else if (idA == ignoreAtom.Count) {
                                    frames.Add(curFrame);
                                }
                                else {
                                    Debug.LogWarning("Ignoring model, number of atoms differ from the first model, try to set modelsAsTraj to false");
                                }
                                curFrame = new Vector3[countAtomsInModels];
                            }
                            idA = 0;
                            offsetidA = 0;

                            // Record last residue and last chain
                            if (atomsList.Count > 0) {
                                UnityMolResidue newRes = new(resNum, lastResidueId, atomsList, lastResidue);
                                residues.Add(newRes);
                                resNum++;
                                foreach (UnityMolAtom a in atomsList) {
                                    a.SetResidue(newRes);
                                }
                                atomsList.Clear();
                            }
                            if (residues.Count > 0) {
                                chains.Add(new UnityMolChain(residues, lastChain));
                                foreach (UnityMolResidue r in residues)
                                {
                                    r.chain = chains.Last();
                                }
                                residues.Clear();
                            }
                            if (chains.Count > 0) {
                                // Record the model
                                UnityMolModel model = new(chains, curModel.ToString());
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
                            string chainH = SubstringWithTrim(sbtrim, currentLine, 19, 2);
                            int startRes = ParseInt(currentLine, 21, 21 + 4); //int.Parse(currentLine.Substring(21, 4));
                            int endRes = ParseInt(currentLine, 33, 33 + 4); //int.Parse(currentLine.Substring (33, 4));
                            int classH = ParseInt(currentLine, 38, 38 + 2); //int.Parse(currentLine.Substring(38, 2));

                            SecStruct newHelix; newHelix.Start = startRes; newHelix.End = endRes;
                            newHelix.Chain = chainH; newHelix.Type = (UnityMolResidue.secondaryStructureType)classH;
                            parsedSSList.Add(newHelix);

                        }
                        // SHEET Secondary Structure Records
                        else if (currentLine.Length >= 5 &&  QuickStartWith(currentLine, "SHEET")) {
                            string chainS = SubstringWithTrim(sbtrim, currentLine, 21, 2);
                            int startRes = ParseInt(currentLine, 23, 23 + 3); //int.Parse(currentLine.Substring (23, 3));

                            int endRes = ParseInt(currentLine, 34, 34 + 3); //int.Parse(currentLine.Substring (34, 3));

                            SecStruct newSheet; newSheet.Start = startRes; newSheet.End = endRes;
                            newSheet.Chain = chainS; newSheet.Type = UnityMolResidue.secondaryStructureType.Strand;
                            parsedSSList.Add(newSheet);

                        }
                        else if (QuickStartWith(currentLine, "CONECT") && currentLine.Length >= 16) {

                            int2 pair;
                            int rootAtom = ParseInt(currentLine, 6, 6 + 5); //int.Parse(currentLine.Substring(6, 5));
                            int bondedA = ParseInt(currentLine, 11, 11 + 5); //int.Parse(currentLine.Substring(11, 5));

                            if (rootAtom != -int.MaxValue && bondedA != -int.MaxValue && rootAtom != bondedA) {
                                pair.x = rootAtom;
                                pair.y = bondedA;
                                bondedAtoms.Add(pair);
                            }

                            int2 bond;
                            if (currentLine.Length >= 22) {
                                // Not all atoms are bonded to 1+ others
                                // string bondedB = currentLine.Substring(16, 5).Trim();
                                // if (bondedB != "") {
                                int _bondedB = ParseInt(currentLine, 16, 16 + 5);
                                if (_bondedB != -int.MaxValue && _bondedB != rootAtom) {
                                    bond.x = rootAtom;
                                    bond.y = _bondedB;
                                    bondedAtoms.Add(bond);
                                }
                            }
                            if (currentLine.Length >= 27) {

                                // string bondedC = currentLine.Substring(21, 5).Trim();
                                // if (bondedC != "") {
                                // int _bondedC = int.Parse(bondedC);
                                int _bondedC = ParseInt(currentLine, 21, 21 + 5);
                                if (_bondedC != -int.MaxValue && _bondedC != rootAtom)  {
                                    bond.x = rootAtom;
                                    bond.y = _bondedC;
                                    bondedAtoms.Add(bond);
                                }
                            }
                            if (currentLine.Length >= 32) {
                                // string bondedD = currentLine.Substring(26, 5).Trim();
                                // if (bondedD != "") {
                                //     int _bondedD = int.Parse(bondedD);
                                int _bondedD = ParseInt(currentLine, 26, 26 + 5);
                                if (_bondedD != -int.MaxValue && _bondedD != rootAtom) {
                                    bond.x = rootAtom;
                                    bond.y = _bondedD;
                                    bondedAtoms.Add(bond);
                                }
                            }
                        }
                        else if (currentLine.Length > 10 && QuickStartWith(currentLine, "REMARK 350") &&
                                 (currentLine.Contains("BIOMT1") || currentLine.Contains("BIOMT2") || currentLine.Contains("BIOMT3"))) {
                            string[] tokens = currentLine.Split(new [] { ' ', '\r'}, StringSplitOptions.RemoveEmptyEntries);
                            float x = float.Parse(tokens[4], CultureInfo.InvariantCulture);
                            float y = float.Parse(tokens[5], CultureInfo.InvariantCulture);
                            float z = float.Parse(tokens[6], CultureInfo.InvariantCulture);
                            float tx = float.Parse(tokens[7], CultureInfo.InvariantCulture);
                            curMatrix[curSymId, 0] = x;
                            curMatrix[curSymId, 1] = y;
                            curMatrix[curSymId, 2] = z;
                            curMatrix[curSymId, 3] = tx;
                            curSymId++;
                            if (curSymId == 3) {
                                Quaternion r = CEAlignWrapper.ExtractRotation(curMatrix);
                                Vector3 t = curMatrix.GetColumn(3);
                                t = Vector3.Scale(t, invertX);
                                Vector3 s = Vector3.one;
                                matrices.Add(Matrix4x4.TRS(t, r, s));
                                curMatrix = Matrix4x4.identity;
                                curSymId = 0;
                            }
                        }
                    }
                }
                catch (Exception e) {
                    string message = "Parser failed while reading line " + lineNumber + ": " + e.Message + "";
                    throw new ParsingException(message, e);
                }
            }


            if (debug.Length != 0) {
                Debug.LogWarning(debug.ToString());
            }

            // Record last residue and last chain
            if (atomsList.Count > 0) {
                residues.Add(new UnityMolResidue(resNum, lastResidueId, atomsList, lastResidue));
                resNum++;
                foreach (UnityMolAtom a in atomsList)
                {
                    a.SetResidue(residues.Last());
                }
                atomsList.Clear();
            }
            if (residues.Count > 0) {
                chains.Add(new UnityMolChain(residues, lastChain));
                foreach (UnityMolResidue r in residues)
                {
                    r.chain = chains.Last();
                }
                residues.Clear();
            }
            if (chains.Count > 0) {
                //Record the model
                UnityMolModel model = new(chains, curModel.ToString());
                model.allAtoms.AddRange(allAtoms);
                allAtoms.Clear();
                models.Add(model);
                curModel++;
            }
        }

        if (models.Count == 0) {
            throw new Exception("PDB parsing error");
        }

        UnityMolStructure newStruct;
        if (frames.Count != 0) {
            newStruct = new UnityMolStructure(models, FileNameWithoutExtension, frames);
        }
        else {
            newStruct = new UnityMolStructure(models, FileNameWithoutExtension);
        }
        newStruct.periodic = periodic;
        newStruct.symMatrices = matrices;

        if (forceStructureType.HasValue) {
            newStruct.structureType = forceStructureType.Value;
        }
        else {
            newStruct.SetStructureMolecularType();
        }

        if (newStruct.structureType != UnityMolStructure.MolecularType.standard) {
            newStruct.updateAtomRepValues();
        }

        if (bondedAtoms.Count != 0) {
            newStruct.parsedConnectivity = bondedAtoms;
        }

        for (int i = 0; i < models.Count; i++) {
            newStruct.models[i].structure = newStruct;

            if (!simplyParse) {
                newStruct.models[i].fillIdAtoms();

                // newStruct.models[i].bonds = ComputeUnityMolBonds.ComputeBondsSlidingWindow(models[i].allAtoms);
                newStruct.models[i].bonds = ComputeUnityMolBonds.ComputeBondsByResidue(models[i].allAtoms);

                newStruct.models[i].ComputeCentroid();
                // newStruct.models[i].CenterAtoms();

                if (chemBonds.Count != 0) {
                    newStruct.models[i].customChemBonds.AddRange(chemBonds);
                }
            }
        }

        if (!simplyParse) {

            FillSecondaryStructure(newStruct, parsedSSList);
            newStruct.parsedSSInfo = parsedSSList;
            UnityMolSelection sel = newStruct.ToSelection();

            if (newStruct.models.Count != 1) {
                for (int i = 1; i < newStruct.models.Count; i++) {
                    CreateUnityObjects(newStruct.ToSelectionName(), new UnityMolSelection(newStruct.models[i].allAtoms, newBonds: null, sel.name, newStruct.name));
                }
            }
            CreateUnityObjects(newStruct.ToSelectionName(), sel);
            newStruct.surfThread = StartSurfaceThread(sel);

            UnityMolMain.getStructureManager().AddStructure(newStruct);
            UnityMolMain.getSelectionManager().Add(sel);
        }

#if UNITY_EDITOR
        Debug.Log("Time for parsing: " + (1000.0f * (Time.realtimeSinceStartup - start)).ToString("f3") + " ms");
#endif
        return newStruct;
    }

    /// <summary>
    /// Parse a PDB file contained in a String to a UnityMolStructure object
    /// </summary>
    public static UnityMolStructure ParseFromString(string pdbContent) {
        PDBReader pdbr = new();
        UnityMolStructure res = null;
        byte[] byteArray = Encoding.UTF8.GetBytes(pdbContent);
        using (MemoryStream sr = new(byteArray)) {
            StreamReader reader = new(sr, Encoding.UTF8, true);
            res = pdbr.ReadData(reader, readHET: true, readWater: true, simplyParse: true);
        }
        return res;
    }

    /// <summary>
    /// PDB writer
    /// Uses a structure and outputs a string containing all the models
    /// </summary>
    public static string Write(UnityMolStructure structure) {
        StringBuilder sw = new();

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
    public static string Write(UnityMolSelection select, bool writeModel = false, bool writeHET = true,
                               bool forceHetAsAtom = false, Vector3[] overridedPos = null, bool writeSS = false,
                               bool rewriteChains = false, StringBuilder sw = null) {

        if (overridedPos != null && select.atoms.Count != overridedPos.Length) {
            Debug.LogError("Size of the overridedPos list does not match the number of atoms in the selections");
            return "";
        }
        // if (select.structures.Count > 1) {
        //     Debug.LogError("Only supports selections with one structure");
        //     return "";
        // }
        // ATOM/HETATM
        string pdbString = "{0,-6}{1, 5} {2, 4}{3, 1}{4, 3} {5, 1}{6, 4}{7, 1}"; // insCode
        pdbString += "   {8,8:N3}{9,8:N3}{10,8:N3}{11,6:N2}{12,6:N2}          {13,2}{14,2}\n";

        // TER
        const string terString = "TER   {0, 5}      {1,3} {2,1}{3,4}{4,1}\n";

        if (sw == null) {
            sw = new StringBuilder();
        } else {
            sw.Clear();
        }

        string prevChain = null;
        string prevStoredChainName = null;
        string curChainNamerewrite = null;
        string curSname = null;
        int idChainRewrite = 0;
        int atomSerial = 0;

        List<UnityMolAtom> atoms = select.atoms;

        if (writeModel) {
            sw.Append("MODEL\n");
        }

        int lastI = 0;
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

            int resid = atom.residue.id;

            string chainId = atom.residue.chain.name;
            chainId = chainId.Substring(0, 1);
            string insCode = " "; // We do not store it yet.

            if (rewriteChains) {
                if (prevStoredChainName == null) {
                    prevStoredChainName = chainId;
                    curChainNamerewrite = getChainName(idChainRewrite);
                }
                else if (chainId != prevStoredChainName || curSname != atom.residue.chain.model.structure.name) { //New chain => find a new name
                    idChainRewrite++;
                    curChainNamerewrite = getChainName(idChainRewrite);
                    prevStoredChainName = chainId;
                }
                chainId = curChainNamerewrite;
            }

            float x = -1 * atom.oriPosition.x; // Revert to right-handed
            float y = atom.oriPosition.y;
            float z = atom.oriPosition.z;

            if (overridedPos != null) {
                x = -1 * overridedPos[i].x;
                y = overridedPos[i].y;
                z = overridedPos[i].z;
            }

            float occupancy = 1.0f;
            float bfactor = atom.bfactor;
            string element = atom.type;
            string charge = " "; // Nothing for now.

            int serial = atomSerial;

            if (atomSerial > 99999) {
                serial = atomSerial % 99999;
            }

            if (resid > 9999) {
                resid %= 9999;
            }

            if (chainId != prevChain) {
                if (prevChain != null) {
                    string prevResName = atoms[lastI].residue.name;
                    int prevResid = atoms[lastI].residue.id;
                    sw.AppendFormat(terString, serial, prevResName, prevChain, prevResid, insCode);
                    atomSerial++;
                }
                prevChain = chainId;
            }

            sw.AppendFormat(CultureInfo.InvariantCulture, pdbString, atomRecordType, serial, atomName.CenterString(4, ' '),
                               altLoc, resName, chainId, resid, insCode, x, y, z,
                               occupancy, bfactor, element, charge);
            curSname = atom.residue.chain.model.structure.name;
            lastI = i;
        }
        if (!writeModel) {
            sw.Append("END\n");
        }
        else {
            sw.Append("ENDMDL\n");
        }
        if (writeSS) {
            WriteSecondaryStructure(select, sw);
        }

        return sw.ToString();
    }

    /// <summary>
    /// Append the information of the Secondary structure contained in the selection 'sel' in the sw Stringbuilder object
    /// </summary>
    public static void WriteSecondaryStructure(UnityMolSelection sel, StringBuilder sw) {
        if (sel.structures.Count == 1) {
            int nbHelix = 0;
            int nbSheet = 0;
            int curResIdStart = -1;
            string curResNameStart = "";
            UnityMolResidue.secondaryStructureType curSSType = UnityMolResidue.secondaryStructureType.Helix;

            bool inHelix = false;

            const string pdbStringHelix = "HELIX  {0,3} {1,3} {2,2}{3,2} {4,4}  {5,2} {6,1} {7,4} {8,2}{9,36}\n"; // insCode
            const string pdbStringSheet = "SHEET  {0,3} {1,3} 0 {2,3} {3,1}{4,4}  {5,3} {6,1}{7,4} \n"; // insCode

            UnityMolResidue lastR = null;
            UnityMolChain lastC = null;

            foreach (UnityMolChain c in sel.structures[0].currentModel.chains.Values) {
                foreach (UnityMolResidue r in c.residues) {
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
                            sw.AppendFormat(pdbStringHelix, nbHelix, nbHelix, curResNameStart, c.name, curResIdStart, r.name, c.name, r.id, (int)curSSType, (r.id - curResIdStart));

                            curResNameStart = r.name;
                            curResIdStart = r.id;
                            curSSType = r.secondaryStructure;
                            nbHelix++;
                            inHelix = true;
                        }
                    }
                    else if (inHelix) { //End of helix
                        sw.AppendFormat(pdbStringHelix, nbHelix, nbHelix, curResNameStart, c.name, curResIdStart, r.name, c.name, r.id, (int)curSSType, (r.id - curResIdStart));
                        inHelix = false;
                    }
                    lastR = r;
                }
                if (inHelix && lastR != null) { //End of chain but still in helix
                    sw.AppendFormat(pdbStringHelix, nbHelix, nbHelix, curResNameStart, c.name, curResIdStart, lastR.name, c.name, lastR.id, (int)curSSType, (lastR.id - curResIdStart));
                    inHelix = false;
                }
                lastC = c;
            }
            if (inHelix && lastC != null) {
                sw.AppendFormat(pdbStringHelix, nbHelix, nbHelix, curResNameStart, lastC.name, curResIdStart, lastR.name, lastC.name, lastR.id, (int)curSSType, (lastR.id - curResIdStart));
                inHelix = false;
            }

            bool inSheet = false;
            foreach (UnityMolChain c in sel.structures[0].currentModel.chains.Values) {
                foreach (UnityMolResidue r in c.residues) {
                    if (r.secondaryStructure == UnityMolResidue.secondaryStructureType.Strand) {
                        if (!inSheet) { //Start sheet
                            curResNameStart = r.name;
                            curResIdStart = r.id;
                            nbSheet++;
                            inSheet = true;
                        }
                    }
                    else if (inSheet) { //End sheet
                        sw.AppendFormat(pdbStringSheet, nbSheet, nbSheet, curResNameStart, c.name, curResIdStart, r.name, c.name, r.id);
                        inSheet = false;
                    }

                    lastR = r;
                }
                if (inSheet) {
                    sw.AppendFormat(pdbStringSheet, nbSheet, nbSheet, curResNameStart, c.name, curResIdStart, lastR.name, c.name, lastR.id);
                    inSheet = false;
                }
                lastC = c;
            }

            if (inSheet) {
                sw.AppendFormat(pdbStringSheet, nbSheet, nbSheet, curResNameStart, lastC.name, curResIdStart, lastR.name, lastC.name, lastR.id);
                inSheet = false;
            }
        }
    }

    /// <summary>
    /// Check if the residue name is a ligand
    /// </summary>
    /// <param name="resName">Name of the residue</param>
    /// <returns>True if the residue is as ligand, False otherwise</returns>
    public static bool IsLigand(string resName) {
        // TODO: do something smarter here
        return resName.Length == 3 && resName is "LIG" or "lig";
    }


    /// <summary>
    /// Guess the atom element from the atom name
    /// Return X if the element cannot be guessed
    /// </summary>
    /// <param name="atomName">name of the atome</param>
    /// <param name="resName">residue name of the atom</param>
    /// <param name="isHET">if the atom is defined as heterogen atom in the PDB file</param>
    /// <returns></returns>
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
        string firstUp = first.ToUpper();

        if (!isHET && UnityMolMain.atomColors.isKnownAtom(firstUp)) {
            return first;
        }

        if (atomName.Length >= 2 && Char.IsDigit(atomName[0]) && atomName[1] == 'H') {
            return "H";
        }

        string firstTwo = atomName.Substring(0, 2);
        bool endsWithDigits = false;
        if (atomName.Length >= 3) {
            endsWithDigits = true;
            for (int i = 2; i < atomName.Length; i++) {
                if (!char.IsDigit(atomName[i])) {
                    endsWithDigits = false;
                    break;
                }
            }
        }
        if (resName == atomName || (!endsWithDigits && UnityMolMain.atomColors.isKnownAtom(firstTwo))) {
            return firstTwo;
        }
        if (UnityMolMain.atomColors.isKnownAtom(firstUp)) {
            return first;
        }
        if (UnityMolMain.atomColors.isKnownAtom(atomName.ToUpper())) {
            return atomName;
        }
        //Remove non-alpha characters
        string onlyAlpha = atomName.Trim('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-');
        if (!string.IsNullOrEmpty(onlyAlpha)) {
            return onlyAlpha;
        }

        return "X";
    }

   /// <summary>
   /// Return a formatted string of 4 characters long whatever the name of the atom
   /// </summary>
   /// <param name="name">name of the atom</param>
   /// <returns> the formatted string </returns>
    private static string formatAtomName(string name) {
        if (name.Length == 1) {
            return " " + name + "  ";
        }

        if (name.Length == 2) {
            return " " + name + " ";
        }

        if (name.Length == 3) {
            return " " + name;
        }

        return name;
    }
    /// <summary>
    /// Returns a 1 letter string from A to z based on id
    /// </summary>
    /// <param name="id"> id of the chain</param>
    /// <returns></returns>
    static string getChainName(int id) {
        int v = id + 65;
        if (v <= 122) {
            return Convert.ToChar(v).ToString();
        }

        v %= 122;
        return Convert.ToChar(v).ToString();
    }

    /// <summary>
    /// Add atoms to a structure using PDB lines
    /// Atoms are only added to the current model of the structure
    /// </summary>
    /// <param name="lines">PDB lines as string</param>
    /// <param name="structure">structure which the atoms will be added</param>
    public static void AddToStructure(string lines, UnityMolStructure structure)
    {
        StreamReader reader = new(
            new MemoryStream(Encoding.ASCII.GetBytes(lines)));
        PDBReader r = new();

        UnityMolStructure hydrogensStructure = r.ReadData(reader, readHET: true, readWater: true, simplyParse: true);

        List<UnityMolAtom> atomsToBeAdded = hydrogensStructure.ToSelectionAll().atoms;

        structure.AddAtoms(atomsToBeAdded);
        // Remove temporary structure for hydrogens to be added
        hydrogensStructure.OnDestroy();

    }

}
}
