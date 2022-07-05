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


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System;
using System.IO;
using System.Linq;

/// <summary>
/// Defines all the functions available from the console
/// </summary>
namespace UMol {
namespace API {
public class APIPython : MonoBehaviour {

    public static string path;
    private static APIPython instance;

    private static PythonConsole2 pythonConsole;
    private static CultureInfo culture = CultureInfo.InvariantCulture;

    void Awake() {

        instance = this;

        path = Application.dataPath;
        PythonConsole2[] objs = FindObjectsOfType<PythonConsole2>();
        if (objs.Length == 0) {
            Debug.LogWarning("Couldn't find the python console object");
        }
        else {
            pythonConsole = objs[0];
        }
    }


    /// <summary>
    /// Allow to call python API commands and record them in the history from C#
    /// </summary>
    public static void ExecuteCommand(string command) {

        pythonConsole.ExecuteCommand(command);
    }


    /// <summary>
    /// Load a local molecular file (pdb/mmcif/gro/mol2/sdf/xyz formats)
    /// </summary>
    public static UnityMolStructure load(string filePath, bool readHetm = true, bool forceDSSP = false, bool showDefaultRep = true, bool center = true, bool modelsAsTraj = true) {

        UnityMolStructure newStruct = null;

        Reader r = Reader.GuessReaderFrom(filePath);
        if (r != null) {
            r.modelsAsTraj = modelsAsTraj;
            newStruct = r.Read(readHet: readHetm);

            if (newStruct != null) {
                string fileName = Path.GetFileName(filePath);
                Debug.Log("Loaded PDB " + fileName + " with " + newStruct.models.Count + " models");
                UnityMolSelection sel = newStruct.ToSelection();

                if (forceDSSP || !newStruct.ssInfoFromFile) {
                    DSSP.assignSS_DSSP(newStruct);
                }
                else {
                    Debug.Log("Using secondary structure definition from the file");
                }

                if (showDefaultRep)
                    defaultRep(sel.name);
                if (center)
                    centerOnStructure(newStruct.uniqueName, recordCommand: false);
            }
            else {
                Debug.LogError("Could not load file " + filePath);
            }
        }
        UnityMolMain.recordPythonCommand("load(filePath=\"" + filePath.Replace("\\", "/") + "\", readHetm=" + cBoolToPy(readHetm) + ", forceDSSP=" +
                                         cBoolToPy(forceDSSP) + ", showDefaultRep=" + cBoolToPy(showDefaultRep) + ", modelsAsTraj=" + cBoolToPy(modelsAsTraj) +
                                         ", center=" + cBoolToPy(center) + ")");
        UnityMolMain.recordUndoPythonCommand("delete(\"" + newStruct.uniqueName + "\")");

        return newStruct;
    }

    /// <summary>
    /// Load a molecular file (pdb/mmcif/gro/mol2/sdf/xyz formats) from a string
    /// </summary>
    public static UnityMolStructure loadFromString(string fileName, string fileContent, bool readHetm = true, bool forceDSSP = false, bool showDefaultRep = true, bool center = true, bool modelsAsTraj = true) {

        UnityMolStructure newStruct = null;

        string tempPath = Application.temporaryCachePath + "/" + fileName;
        try {

            using(StreamWriter sw = new StreamWriter(tempPath, false)) {

                sw.WriteLine(fileContent);
                sw.Close();
            }

            Reader r = Reader.GuessReaderFrom(tempPath);

            if (r != null) {
                r.modelsAsTraj = modelsAsTraj;

                newStruct = r.Read(readHet: readHetm);

                if (newStruct != null) {
                    Debug.Log("Loaded PDB " + fileName + " with " + newStruct.models.Count + " models");
                    UnityMolSelection sel = newStruct.ToSelection();

                    if (forceDSSP || !newStruct.ssInfoFromFile) {
                        DSSP.assignSS_DSSP(newStruct);
                    }
                    else {
                        Debug.Log("Using secondary structure definition from the file");
                    }

                    if (showDefaultRep)
                        defaultRep(sel.name);
                    if (center)
                        centerOnStructure(newStruct.uniqueName, recordCommand: false);
                }
                else {
                    Debug.LogError("Could not load file content");
                }
                UnityMolMain.recordPythonCommand("loadFromString(fileName=\"" + fileName + "\", fileContent= \"" +
                                                 fileContent + "\", readHetm=" + cBoolToPy(readHetm) + ", forceDSSP=" +
                                                 cBoolToPy(forceDSSP) + ", showDefaultRep=" + cBoolToPy(showDefaultRep) +
                                                 ", center=" + cBoolToPy(center) + ", modelsAsTraj=" + cBoolToPy(modelsAsTraj) + ")");
                UnityMolMain.recordUndoPythonCommand("delete(\"" + newStruct.uniqueName + "\")");

            }
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }

        return newStruct;
    }


    /// <summary>
    /// Fetch a remote molecular file (pdb or mmcif zipped)
    /// </summary>
    public static UnityMolStructure fetch(string PDBId, bool usemmCIF = true, bool readHetm = true, bool forceDSSP = false, bool showDefaultRep = true, bool center = true, bool modelsAsTraj = true) {

        UnityMolStructure newStruct = null;

        if (usemmCIF) {
            PDBxReader rx = new PDBxReader ();
            rx.modelsAsTraj = modelsAsTraj;

            newStruct = rx.Fetch(PDBId, readHet: readHetm);
        }
        else {

            PDBReader r = new PDBReader ();
            r.modelsAsTraj = modelsAsTraj;

            newStruct = r.Fetch(PDBId, readHet: readHetm);
        }

        UnityMolSelection sel = newStruct.ToSelection();

        if (forceDSSP || !newStruct.ssInfoFromFile) {
            DSSP.assignSS_DSSP(newStruct);
        }
        else {
            Debug.Log("Using secondary structure definition from the file");
        }

        if (showDefaultRep)
            defaultRep(sel.name);
        if (center)
            centerOnStructure(newStruct.uniqueName, recordCommand: false);

        UnityMolMain.recordPythonCommand("fetch(PDBId=\"" + PDBId + "\", usemmCIF=" + cBoolToPy(usemmCIF) + ", readHetm=" + cBoolToPy(readHetm) + ", forceDSSP=" +
                                         cBoolToPy(forceDSSP) + ", showDefaultRep=" + cBoolToPy(showDefaultRep) + ", modelsAsTraj=" + cBoolToPy(modelsAsTraj) +
                                         ", center=" + cBoolToPy(center) + ")");

        UnityMolMain.recordUndoPythonCommand("delete(\"" + newStruct.uniqueName + "\")");

        return newStruct;
    }

    /// Load a XML file containing covalent and noncovalent bonds
    /// modelId = -1 means currentModel
    public static void loadBondsXML(string structureName, string filePath, int modelId = -1) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (s != null) {
            UnityMolModel m = s.currentModel;

            if (modelId >= 0 && modelId < s.models.Count) {
                m = s.models[modelId];
            }
            else if ( modelId != -1) {
                Debug.LogError("Wrong model");
                return;
            }

            //Stores the bonds in the model.covBondOrders
            Dictionary<bondOrderType, List<AtomDuo>> res = BondOrderParser.parseBondOrderFile(m, filePath);

            int id = 0;
            foreach (bondOrderType bot in res.Keys) {

                UnityMolBonds curBonds = new UnityMolBonds();
                foreach (AtomDuo d in res[bot]) {
                    curBonds.Add(d.a1, d.a2);
                }

                UnityMolSelection sel = new UnityMolSelection(m.allAtoms,
                        curBonds, s.uniqueName + "_" + bot.btype + id + "_ExternalBonds");

                sel.canUpdateBonds = false;
                UnityMolMain.getSelectionManager().Add(sel);

                // showSelection(sel.name, "hbondtube");
                showSelection(sel.name, "hbondtube", true);

                id++;
            }
        }
        else {
            Debug.LogError("Structure not found");
        }
        UnityMolMain.recordPythonCommand("loadBondsXML(\"" + structureName + "\", \"" + filePath.Replace("\\", "/") + "\", " + modelId + ")");
        UnityMolMain.recordUndoPythonCommand("unloadCustomBonds(\"" + structureName + "\", " + modelId + ")");
    }

    public static void overrideBondsWithXML(string structureName,  int modelId = -1) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (s != null) {
            UnityMolModel m = s.currentModel;

            if (modelId >= 0 && modelId < s.models.Count) {
                m = s.models[modelId];
            }
            else if ( modelId != -1) {
                Debug.LogError("Wrong model");
                return;
            }

            Dictionary<AtomDuo, bondOrderType> xmlBonds = m.covBondOrders;
            if (xmlBonds != null) {
                m.savedBonds = m.bonds;

                UnityMolBonds newBonds = new UnityMolBonds();
                foreach (AtomDuo d in xmlBonds.Keys) {
                    newBonds.Add(d.a1, d.a2);
                }
                m.bonds = newBonds;
                s.updateRepresentations(trajectory: false);
            }
            else {
                Debug.LogError("No bonds parsed from a XML file in this model");
            }

        }

        UnityMolMain.recordPythonCommand("overrideBondsWithXML(\"" + structureName + "\", " + modelId + ")");
        UnityMolMain.recordUndoPythonCommand("restoreBonds(\"" + structureName + "\", " + modelId + ")");
    }
    public static void restoreBonds(string structureName,  int modelId = -1) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (s != null) {
            UnityMolModel m = s.currentModel;

            if (modelId >= 0 && modelId < s.models.Count) {
                m = s.models[modelId];
            }
            else if ( modelId != -1) {
                Debug.LogError("Wrong model");
                return;
            }

            Dictionary<AtomDuo, bondOrderType> xmlBonds = m.covBondOrders;
            if (xmlBonds != null && m.savedBonds != null) {
                m.bonds = m.savedBonds;
                s.updateRepresentations(trajectory: false);
            }
            else {
                Debug.LogError("No bonds parsed from a XML file in this model");
            }

        }

        UnityMolMain.recordPythonCommand("restoreBonds(\"" + structureName + "\", " + modelId + ")");
        UnityMolMain.recordUndoPythonCommand("overrideBondsWithXML(\"" + structureName + "\", " + modelId + ")");
    }

    public static void unloadCustomBonds(string structureName, int modelId) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (s != null) {
            if (modelId >= 0 && modelId < s.models.Count) {
                s.models[modelId].covBondOrders = null;
            }
        }
        else {
            Debug.LogError("Structure not found");
        }
        UnityMolMain.recordPythonCommand("unloadCustomBonds(\"" + structureName + "\", " + modelId + ")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Delete all the loaded molecules
    /// </summary>
    public static void reset() {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        List<string> toDelete = new List<string>();

        foreach (UnityMolStructure s in sm.loadedStructures) {
            toDelete.Add(s.uniqueName);
        }
        foreach (string s in toDelete) {
            delete(s);
        }

    }

    /// <summary>
    /// Switch between parsed secondary structure information and DSSP computation
    /// </summary>
    public static void switchSSAssignmentMethod(string structureName, bool forceDSSP = false) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (s != null) {

            if (forceDSSP || s.parsedSSInfo == null) {
                DSSP.assignSS_DSSP(s);
                Debug.LogWarning("Setting secondary structure assignment to DSSP");
            }
            else if (s.ssInfoFromFile) {
                DSSP.assignSS_DSSP(s);
                Debug.LogWarning("Setting secondary structure assignment to DSSP");
            }
            else {
                Reader.FillSecondaryStructure(s, s.parsedSSInfo);
                Debug.LogWarning("Setting secondary structure assignment parsed from file");
            }

            UnityMolMain.getPrecompRepManager().Clear(s.uniqueName);

            UnityMolMain.recordPythonCommand("switchSSAssignmentMethod(\"" + structureName + "\"," + cBoolToPy(forceDSSP) + ")");
            UnityMolMain.recordUndoPythonCommand("switchSSAssignmentMethod(\"" + structureName + "\")");
        }
    }

    /// <summary>
    /// Show/Hide hydrogens in representations of the provided selection
    /// This only works for lines, hyperball and sphere representations
    /// </summary>
    public static void showHideHydrogensInSelection(string selName, bool? shouldShow = null) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        bool firstRep = true;

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            foreach (List<UnityMolRepresentation> reps in sel.representations.Values) {
                foreach (UnityMolRepresentation r in reps) {
                    foreach (SubRepresentation sr in r.subReps) {
                        if (sr.atomRepManager != null) {
                            if (firstRep) {
                                if (!shouldShow.HasValue)
                                    shouldShow = !sr.atomRepManager.areHydrogensOn;
                                firstRep = false;
                            }
                            sr.atomRepManager.ShowHydrogens(shouldShow.HasValue ? shouldShow.Value : false);
                        }
                        if (sr.bondRepManager != null) {
                            if (firstRep) {
                                if (!shouldShow.HasValue)
                                    shouldShow = !sr.bondRepManager.areHydrogensOn;
                                firstRep = false;
                            }
                            sr.bondRepManager.ShowHydrogens(shouldShow.HasValue ? shouldShow.Value : false);
                        }
                    }
                }
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }
        UnityMolMain.recordPythonCommand("showHideHydrogensInSelection(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("showHideHydrogensInSelection(\"" + selName + "\")");
    }



    /// <summary>
    /// Show/Hide side chains in representations of the current selection
    /// This only works for lines, hyperball and sphere representations only
    /// </summary>
    public static void showHideSideChainsInSelection(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        bool firstRep = true;
        bool shouldShow = true;

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            foreach (List<UnityMolRepresentation> reps in sel.representations.Values) {
                foreach (UnityMolRepresentation r in reps) {
                    foreach (SubRepresentation sr in r.subReps) {
                        if (sr.atomRepManager != null) {
                            if (firstRep) {
                                shouldShow = !sr.atomRepManager.areSideChainsOn;
                                firstRep = false;
                            }
                            sr.atomRepManager.ShowSideChains(shouldShow);
                        }
                        if (sr.bondRepManager != null) {
                            if (firstRep) {
                                shouldShow = !sr.bondRepManager.areSideChainsOn;
                                firstRep = false;
                            }
                            sr.bondRepManager.ShowSideChains(shouldShow);
                        }
                    }
                }
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }
        UnityMolMain.recordPythonCommand("showHideSideChainsInSelection(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("showHideSideChainsInSelection(\"" + selName + "\")");
    }

    /// <summary>
    /// Show/Hide backbone in representations of the current selection
    /// This only works for lines, hyperball and sphere representations only
    /// </summary>
    public static void showHideBackboneInSelection(string selName) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        bool firstRep = true;
        bool shouldShow = true;

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];
            foreach (List<UnityMolRepresentation> reps in sel.representations.Values) {
                foreach (UnityMolRepresentation r in reps) {
                    foreach (SubRepresentation sr in r.subReps) {
                        if (sr.atomRepManager != null) {
                            if (firstRep) {
                                shouldShow = !sr.atomRepManager.isBackboneOn;
                                firstRep = false;
                            }
                            sr.atomRepManager.ShowBackbone(shouldShow);
                        }
                        if (sr.bondRepManager != null) {
                            if (firstRep) {
                                shouldShow = !sr.bondRepManager.isBackboneOn;
                                firstRep = false;
                            }
                            sr.bondRepManager.ShowBackbone(shouldShow);
                        }
                    }
                }
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }
        UnityMolMain.recordPythonCommand("showHideBackboneInSelection(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("showHideBackboneInSelection(\"" + selName + "\")");
    }

    /// <summary>
    /// Set the current model of the structure
    /// This function is used by ModelPlayers.cs to read the models of a structure like a trajectory
    /// </summary>
    public static void setModel(string structureName, int modelId) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);

        int prev = s.currentModelId;
        if (s.trajectoryMode) {
            prev = s.currentFrameId;
        }

        s.setModel(modelId);

        UnityMolMain.recordPythonCommand("setModel(\"" + structureName + "\", " + modelId + ")");
        UnityMolMain.recordUndoPythonCommand("setModel(\"" + structureName + "\", " + prev + ")");
    }

    /// <summary>
    /// Load a trajectory for a loaded structure
    /// It creates a XDRFileReader in the corresponding UnityMolStructure and a TrajectoryPlayer
    /// </summary>
    public static void loadTraj(string structureName, string path) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);

        try {
            s.readTrajectoryXDR(path);
            s.createTrajectoryPlayer();
        }
        catch (System.Exception e) {
            Debug.LogError("Could not load trajectory file '" + path + "'");
#if UNITY_EDITOR
            Debug.LogError(e);
#endif
            return;
        }

        UnityMolMain.recordPythonCommand("loadTraj(\"" + structureName + "\", \"" + path.Replace("\\", "/") + "\")");
        UnityMolMain.recordUndoPythonCommand("unloadTraj(\"" + structureName + "\")");
    }

    /// <summary>
    /// Unload a trajectory for a specific structure
    /// </summary>
    public static void unloadTraj(string structureName) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        s.unloadTrajectoryXDR();

        UnityMolMain.recordPythonCommand("unloadTraj(\"" + structureName + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Load a density map for a specific structure
    /// This function creates a DXReader instance in the UnityMolStructure
    /// </summary>
    public static void loadDXmap(string structureName, string path) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);

        try {
            s.readDX(path);
        }
        catch {
            Debug.LogError("Could not load DX map file '" + path + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("loadDXmap(\"" + structureName + "\", \"" + path.Replace("\\", "/") + "\")");
        UnityMolMain.recordUndoPythonCommand("unloadDXmap(\"" + structureName + "\")");
    }
    /// <summary>
    /// Unload the density map for the structure
    /// </summary>
    public static void unloadDXmap(string structureName) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);

        s.unloadDX();

        UnityMolMain.recordPythonCommand("unloadDXmap(\"" + structureName + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Read a json file and display fieldLines for the specified structure
    /// </summary>
    public static void readJSONFieldlines(string structureName, string path) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolSelection sel = s.currentModel.ToSelection();

        FieldLinesReader flr = new FieldLinesReader(path);

        s.currentModel.fieldLinesR = flr;

        deleteRepresentationInSelection(s.ToSelectionName(), "fl");

        repManager.AddRepresentation (s, AtomType.fieldlines, BondType.nobond, flr);

        UnityMolMain.recordPythonCommand("readJSONFieldlines(\"" + structureName + "\", \"" + path.Replace("\\", "/") + "\")");
        UnityMolMain.recordUndoPythonCommand("unloadJSONFieldlines(\"" + structureName + "\")");
    }

    /// <summary>
    /// Remove the json file for fieldlines stored in the currentModel of the specified structure
    /// </summary>
    public static void unloadJSONFieldlines(string structureName) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }
        UnityMolStructure s = sm.GetStructure(structureName);
        s.currentModel.fieldLinesR = null;

        UnityMolMain.recordPythonCommand("unloadJSONFieldlines(\"" + structureName + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }
    /// <summary>
    /// Change fieldline computation gradient threshold
    /// </summary>
    public static void setFieldlineGradientThreshold(string selName, float val) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repM = UnityMolMain.getRepresentationManager();

        float prev = 10.0f;
        if (selM.selections.ContainsKey(selName)) {
            RepType repType = APIPython.getRepType("fl");

            List<UnityMolRepresentation> existingReps = repM.representationExists(selName, repType);
            foreach (UnityMolRepresentation rep in existingReps) {
                SubRepresentation sr = rep.subReps.First();//There shouldn't be more than one
                FieldLinesRepresentation flRep = (FieldLinesRepresentation) sr.atomRep;
                prev = flRep.magThreshold;
                flRep.recompute(val);
            }
        }
        UnityMolMain.recordPythonCommand("setFieldlineGradientThreshold(\"" + selName + "\", " + val.ToString("f3", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("setFieldlineGradientThreshold(\"" + selName + "\", " + prev.ToString("f3", culture) + ")");
    }

    /// <summary>
    /// Utility function to be able to get the group of the structure
    /// This group is used to be able to move all the loaded molecules in the same group
    /// Groups can be between 0 and 9 included
    /// </summary>
    public static int getStructureGroup(string structureName) {
        int group = -1;
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return group;
        }

        if (sm.nameToStructure.ContainsKey(structureName)) {
            UnityMolStructure s = sm.GetStructure(structureName);
            group = s.groupID;
        }
        return group;
    }

    /// <summary>
    /// Utility function to be able to get the structures of the group
    /// This group is used to be able to move all the loaded molecules in the same group
    /// Groups can be between 0 and 9 included
    /// </summary>
    public static HashSet<UnityMolStructure> getStructuresOfGroup(int group) {
        HashSet<UnityMolStructure> result = new HashSet<UnityMolStructure>();

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        foreach (UnityMolStructure s in sm.loadedStructures) {
            if (s.groupID == group) {
                result.Add(s);
            }
        }
        return result;
    }

    /// <summary>
    /// Utility function to be set the group of a structure
    /// This group is used to be able to move all the loaded molecules in the same group
    /// Groups can be between 0 and 9 included
    /// </summary>
    public static void setStructureGroup(string structureName, int newGroup) {

        int prevGroup = 0;
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        if (sm.nameToStructure.ContainsKey(structureName)) {
            UnityMolStructure s = sm.GetStructure(structureName);
            prevGroup = s.groupID;
            s.groupID = newGroup;
        }
        else {
            Debug.LogWarning("No molecule loaded name '" + structureName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setStructureGroup(\"" + structureName + "\", " + newGroup + ")");
        UnityMolMain.recordUndoPythonCommand("setStructureGroup(\"" + structureName + "\", " + prevGroup + ")");

    }

    /// <summary>
    /// Delete a molecule and all its UnityMolSelection and UnityMolRepresentation
    /// </summary>
    public static void delete(string structureName) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolStructure s = sm.GetStructure(structureName);

        if (s != null) { //Remove a complete loaded molecule
            sm.Delete(s);
            Debug.LogWarning("Deleting molecule '" + structureName + "'");
        }
        else {
            Debug.LogWarning("No molecule loaded name '" + structureName + "'");
            return;
        }
        UnityMolMain.recordPythonCommand("delete(\"" + structureName + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Show as 'type' all loaded molecules
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    public static void show(string type) {

        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        RepType repType = getRepType(type);
        if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {


            foreach (UnityMolStructure s in sm.loadedStructures) {
                List<UnityMolRepresentation> existingReps = repManager.representationExists(s.ToSelectionName(), repType);

                if (existingReps == null) {
                    repManager.AddRepresentation(s, repType.atomType, repType.bondType);
                }
                else {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.Show();
                    }
                }
            }
        }
        else {
            Debug.LogError("Wrong representation type");
            return;
        }

        UnityMolMain.recordPythonCommand("show(\"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("hide(\"" + type + "\")");
    }

    /// <summary>
    /// Show all loaded molecules only as the 'type' representation
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    public static void showAs(string type) {

        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        RepType repType = getRepType(type);
        if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {
            //First hide all representations
            foreach (UnityMolStructure s in sm.loadedStructures) {
                foreach (UnityMolRepresentation r in s.representations) {
                    r.Hide();
                }
            }


            foreach (UnityMolStructure s in sm.loadedStructures) {
                List<UnityMolRepresentation> existingReps = repManager.representationExists(s.ToSelectionName(), repType);

                if (existingReps == null) {
                    repManager.AddRepresentation(s, repType.atomType, repType.bondType);
                }
                else {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.Show();
                    }
                }
            }
        }
        else {
            Debug.LogError("Wrong representation type");
            return;
        }

        UnityMolMain.recordPythonCommand("show(\"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("hide(\"" + type + "\")");
    }

    /// <summary>
    /// Create selections and default representations: all in cartoon, not protein in hyperballs
    /// Also create a selection containing "not protein and not water and not ligand and not ions"
    /// </summary>
    public static bool defaultRep(string selName) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();


        if (selM.selections.ContainsKey(selName)) {
            //Remove all previous representations
            UnityMolSelection sel = selM.selections[selName];
            selM.DeleteRepresentations(sel);

            RepType repType = getRepType("c");
            RepType repTypehb = getRepType("hb");

            repManager.AddRepresentation(sel, repType.atomType, repType.bondType);

            bool cartoonEmpty = false;
            //If the cartoon is empty => show has hyperball
            List<UnityMolRepresentation> existingReps = repManager.representationExists(sel.name, repType);
            try {
                if (existingReps.Last() != null) {
                    SubRepresentation sr = existingReps.Last().subReps.Last();
                    CartoonRepresentation rep = (CartoonRepresentation) sr.atomRep;
                    if (rep != null && rep.totalVertices == 0) {
                        repManager.AddRepresentation(sel, repTypehb.atomType, repTypehb.bondType);
                        cartoonEmpty = true;
                    }
                }
            }
            catch {
                cartoonEmpty = true;
            }

            if (sel.structures != null && sel.structures.Count > 0) {
                string sName = sel.structures[0].uniqueName;
                string notPSelName = sName + "_not_protein";
                if (selM.selections.ContainsKey(notPSelName)) {
                    selM.DeleteRepresentations(selM.selections[notPSelName]);
                }
                MDAnalysisSelection selec = new MDAnalysisSelection("not protein", sel.atoms);
                UnityMolSelection ret = selec.process();
                ret.name = notPSelName;

                if (ret.Count != 0) {
                    if (!cartoonEmpty) { //Show not protein as hb only if the cartoon was successfully shown
                        repManager.AddRepresentation(ret, repTypehb.atomType, repTypehb.bondType);
                    }

                    selM.Add(ret);
                    selM.AddSelectionKeyword(ret.name, ret.name);
                }


                //Unknown atoms = not protein/ligand/water/ions
                MDAnalysisSelection selecUnreco = new MDAnalysisSelection("not protein and not water and not ligand and not ions", sel.atoms);
                UnityMolSelection selUnreco = selecUnreco.process();
                selUnreco.name = sName + "_unrecognized_atoms";

                if (selUnreco.Count != 0) {
                    selM.Add(selUnreco);
                    selM.AddSelectionKeyword(selUnreco.name, selUnreco.name);
                }
            }
            // selM.SetCurrentSelection(sel);
            // sel.mergeRepresentations(ret);
            selM.ClearCurrentSelection();
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return false;
        }
        return true;

    }

    /// <summary>
    /// Create default representations (cartoon for protein + HB for not protein atoms)
    /// </summary>
    public static void showDefault(string selName) {

        if (!defaultRep(selName)) {
            return;
        }

        UnityMolMain.recordPythonCommand("showDefault(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("hideSelection(\"" + selName + "\", \"c\")\nhideSelection(\"" + selName + "\", \"hb\")\nhideSelection(\"" + (selName + "_not_protein") + "\", \"hb\")");
    }

    public static void waitOneFrame() {
        instance.StartCoroutine(instance.waitOf());
    }

    public IEnumerator waitOf() {
        yield return  new WaitForEndOfFrame();
    }
    /// <summary>
    /// Unhide all representations already created for a specified structure
    /// </summary>
    public static void showStructureAllRepresentations(string structureName) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        if (sm.nameToStructure.ContainsKey(structureName)) {
            UnityMolStructure s = sm.GetStructure(structureName);
            foreach (UnityMolRepresentation r in s.representations) {
                r.Show();
            }
        }
        else {
            Debug.LogWarning("No molecule loaded name '" + structureName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("showStructureAllRepresentations(\"" + structureName + "\")");
        UnityMolMain.recordUndoPythonCommand("hideStructureAllRepresentations(\"" + structureName + "\")");
    }

    /// <summary>
    /// Show the selection as 'type'
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// If the representation is already there, update it if the selection content changed and show it
    /// Surface example: showSelection("all(1kx2)", "s", True, True, True, SurfMethod.MSMS)
    /// Iso-surface example: showSelection("all(1kx2)", "dxiso", last().dxr, 0.0f)
    /// </summary>
    public static void showSelection(string selName, string type, params object[] args) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();


        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType(type);

            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);

                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        if (repType.atomType != AtomType.noatom && sel.Count != existingRep.nbAtomsInRep) {
                            existingRep.updateWithNewSelection(sel);
                        }
                        else if (repType.bondType != BondType.nobond && sel.bonds.Count != existingRep.nbBondsInRep) {
                            existingRep.updateWithNewSelection(sel);
                        }

                        existingRep.Show();
                    }
                }
                else {
                    repManager.AddRepresentation(sel, repType.atomType, repType.bondType, args);
                }
            }
            else {
                Debug.LogError("Wrong representation type");
                return;
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }


        string command = "showSelection(\"" + selName + "\", \"" + type + "\"";
        foreach (object o in args) {
            if (o is string) {
                command += ", \"" + o.ToString() + "\"";
            }
            else if (o is float) {
                command += ", " + ((float)o).ToString("f3", culture);
            }
            else if (o is DXReader) {
                command += ", (DXReader)UnityMolMain.getSelectionManager().selections[\"" + selName + "\"].structures[0].dxr";
            }
            else {
                command += ", " + o.ToString();
            }
        }
        command += ")";
        UnityMolMain.recordPythonCommand(command);

        UnityMolMain.recordUndoPythonCommand("hideSelection(\"" + selName + "\", \"" + type + "\")");

    }

    /// <summary>
    /// Hide every representations of the specified selection
    /// </summary>
    public static void hideSelection(string selName) {


        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];
            foreach (List<UnityMolRepresentation> lr in sel.representations.Values) {
                foreach (UnityMolRepresentation r in lr) {
                    r.Hide();
                }
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }
        UnityMolMain.recordPythonCommand("hideSelection(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("showSelection(\"" + selName + "\")");
    }

    /// <summary>
    /// Hide every representation of type 'type' of the specified selection
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    public static void hideSelection(string selName, string type) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                UnityMolSelection sel = selM.selections[selName];

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);

                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.Hide();
                    }
                }
            }
            else {
                Debug.LogError("Wrong representation type");
                return;
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("hideSelection(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("showSelection(\"" + selName + "\", \"" + type + "\")");
    }

    /// <summary>
    /// Delete every representations of type 'type' of the specified selection
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    public static void deleteRepresentationInSelection(string selName, string type) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {


                UnityMolSelection sel = selM.selections[selName];

                selM.DeleteRepresentation(sel, repType);
            }
            else {
                Debug.LogError("Wrong representation type");
                return;
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("deleteRepresentationInSelection(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("showSelection(\"" + selName + "\", \"" + type + "\")");
    }

    /// <summary>
    /// Delete every representations of the specified selection
    /// </summary>
    public static void deleteRepresentationsInSelection(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            UnityMolSelection sel = selM.selections[selName];

            selM.DeleteRepresentations(sel);
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("deleteRepresentationsInSelection(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Hide every representations of the specified structure
    /// </summary>
    public static void hideStructureAllRepresentations(string structureName) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        if (sm.nameToStructure.ContainsKey(structureName)) {
            UnityMolStructure s = sm.GetStructure(structureName);
            foreach (UnityMolRepresentation r in s.representations) {
                r.Hide();
            }
        }
        else {
            Debug.LogWarning("No molecule loaded name '" + structureName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("hideStructureAllRepresentations(\"" + structureName + "\")");
        UnityMolMain.recordUndoPythonCommand("showStructureAllRepresentations(\"" + structureName + "\")");
    }

    /// <summary>
    /// Utility function to test if a representation is shown for a specified structure
    /// </summary>
    public static bool areRepresentationsOn(string structureName) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return false;
        }

        if (sm.nameToStructure.ContainsKey(structureName)) {
            UnityMolStructure s = sm.GetStructure(structureName);
            foreach (UnityMolRepresentation r in s.representations) {
                if (r.isActive()) {
                    return true;
                }
            }
        }
        else {
            Debug.LogWarning("No molecule loaded name '" + structureName + "'");
        }
        return false;
    }

    /// <summary>
    /// Utility function to test if a representation of type 'type' is shown for a specified selection
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    public static bool areRepresentationsOn(string selName, string type) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {


                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        if (existingRep.isActive()) {
                            return true;
                        }
                    }
                }
            }
            else {
                Debug.LogError("Wrong representation type");
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
        }
        return false;
    }

    /// <summary>
    /// Hide all representations of type 'type'
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    public static void hide(string type) {

        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }
        RepType repType = getRepType(type);
        if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

            foreach (UnityMolStructure s in sm.loadedStructures) {
                foreach (UnityMolRepresentation rep in s.representations) {
                    if (rep.repType == repType) {
                        rep.Hide();
                    }
                }
            }
        }
        else {
            Debug.LogError("Wrong representation type");
            return;
        }

        UnityMolMain.recordPythonCommand("hide(\"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("show(\"" + type + "\")");
    }

    /// <summary>
    /// Switch between the 2 types of surface computation methods: EDTSurf and MSMS
    /// </summary>
    public static void switchSurfaceComputeMethod(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType("s");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolSurfaceManager surfM = (UnityMolSurfaceManager) sr.atomRepManager;
                        surfM.SwitchComputeMethod();
                    }
                }
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("switchSurfaceComputeMethod(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("switchSurfaceComputeMethod(\"" + selName + "\")");
    }

    /// <summary>
    /// Switch between cut surface mode and no-cut surface mode
    /// </summary>
    public static void switchCutSurface(string selName, bool isCut) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType("s");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolSurfaceManager surfM = (UnityMolSurfaceManager) sr.atomRepManager;
                        surfM.SwitchCutSurface(isCut);
                    }
                }
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("switchCutSurface(\"" + selName + "\", " + isCut + ")");
        UnityMolMain.recordUndoPythonCommand("switchCutSurface(\"" + selName + "\", " + !isCut + ")");
    }


    /// <summary>
    /// Switch all surface representation in selection to a solid surface material
    /// </summary>
    public static void setSolidSurface(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        bool wasWireframe = false;
        bool wasTransparent = false;

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType("s");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolSurfaceManager surfM = (UnityMolSurfaceManager) sr.atomRepManager;
                        if (surfM.isWireframe) {
                            wasWireframe = true;
                            surfM.SwitchWireframe();
                        }
                        else if (surfM.isTransparent) {
                            wasTransparent = true;
                            surfM.SwitchTransparent();
                        }
                    }
                }
            }
            repType = getRepType("dxiso");

            existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolSurfaceManager surfM = (UnityMolSurfaceManager) sr.atomRepManager;
                        if (surfM.isWireframe) {
                            wasWireframe = true;
                            surfM.SwitchWireframe();
                        }
                        else if (surfM.isTransparent) {
                            wasTransparent = true;
                            surfM.SwitchTransparent();
                        }
                    }
                }
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setSolidSurface(\"" + selName + "\")");

        if (wasTransparent) {
            UnityMolMain.recordUndoPythonCommand("setTransparentSurface(\"" + selName + "\")");
        }
        else if (wasWireframe) {
            UnityMolMain.recordUndoPythonCommand("setWireframeSurface(\"" + selName + "\")");
        }
        else {
            UnityMolMain.recordUndoPythonCommand("");
        }
    }

    /// <summary>
    /// Switch all surface representation in selection to a wireframe surface material when available
    /// </summary>
    public static void setWireframeSurface(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        bool wasSolid = false;
        bool wasTransparent = false;

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType("s");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolSurfaceManager surfM = (UnityMolSurfaceManager) sr.atomRepManager;

                        if (surfM.isTransparent) {
                            wasTransparent = true;
                        }
                        if (!surfM.isTransparent && !surfM.isWireframe) {
                            wasSolid = true;
                        }
                        if (!surfM.isWireframe) {
                            surfM.SwitchWireframe();
                        }
                    }
                }
            }

            repType = getRepType("dxiso");

            existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolSurfaceManager surfM = (UnityMolSurfaceManager) sr.atomRepManager;

                        if (surfM.isTransparent) {
                            wasTransparent = true;
                        }
                        if (!surfM.isTransparent && !surfM.isWireframe) {
                            wasSolid = true;
                        }
                        if (!surfM.isWireframe) {
                            surfM.SwitchWireframe();
                        }
                    }
                }
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setWireframeSurface(\"" + selName + "\")");

        if (wasTransparent) {
            UnityMolMain.recordUndoPythonCommand("setTransparentSurface(\"" + selName + "\")");
        }
        else if (wasSolid) {
            UnityMolMain.recordUndoPythonCommand("setSolidSurface(\"" + selName + "\")");
        }
        else {
            UnityMolMain.recordUndoPythonCommand("");
        }
    }

    /// <summary>
    /// Switch all surface representation in selection to a transparent surface material
    /// </summary>
    public static void setTransparentSurface(string selName, float alpha = 0.8f) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        bool wasSolid = false;
        bool wasWireframe = false;

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType("s");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolSurfaceManager surfM = (UnityMolSurfaceManager) sr.atomRepManager;


                        if (!surfM.isTransparent && !surfM.isWireframe) {
                            wasSolid = true;
                        }
                        if (surfM.isWireframe) {
                            wasWireframe = true;
                            surfM.SwitchWireframe();
                        }
                        if (!surfM.isTransparent) {
                            surfM.SwitchTransparent();
                        }
                        surfM.SetAlpha(alpha);
                    }
                }
            }

            repType = getRepType("dxiso");

            existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolSurfaceManager surfM = (UnityMolSurfaceManager) sr.atomRepManager;


                        if (!surfM.isTransparent && !surfM.isWireframe) {
                            wasSolid = true;
                        }
                        if (surfM.isWireframe) {
                            wasWireframe = true;
                            surfM.SwitchWireframe();
                        }
                        if (!surfM.isTransparent) {
                            surfM.SwitchTransparent();
                        }
                        surfM.SetAlpha(alpha);
                    }
                }
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setTransparentSurface(\"" + selName + "\", " + alpha.ToString("f1", culture) + ")");

        if (wasWireframe) {
            UnityMolMain.recordUndoPythonCommand("setWireframeSurface(\"" + selName + "\")");
        }
        else if (wasSolid) {
            UnityMolMain.recordUndoPythonCommand("setSolidSurface(\"" + selName + "\")");
        }
        else {
            UnityMolMain.recordUndoPythonCommand("");
        }
    }

    /// <summary>
    /// Recompute the DX surface with a new iso value
    /// </summary>
    public static void updateDXIso(string selName, float newVal) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();
        float prevVal = 0.0f;

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType("dxiso");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        prevVal = ((DXSurfaceRepresentation)sr.atomRep).isoValue;
                        ((DXSurfaceRepresentation)sr.atomRep).isoValue = newVal;
                        ((DXSurfaceRepresentation)sr.atomRep).recompute();
                    }
                }
            }
        }

        UnityMolMain.recordPythonCommand("updateDXIso(\"" + selName + "\", " + newVal.ToString("F3", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("updateDXIso(\"" + selName + "\", " + prevVal.ToString("F3", culture) + ")");
    }

    /// <summary>
    /// Change hyperball representation parameters in the specified selection to a preset
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    public static void setSmoothness(string selName, string type, float val) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            if (sr.atomRepManager != null) {
                                sr.atomRepManager.SetSmoothness(val);
                            }
                            if (sr.bondRepManager != null) {
                                sr.bondRepManager.SetSmoothness(val);
                            }
                        }
                    }
                }
            }
            else {
                Debug.LogError("Wrong representation type");
                return;
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setSmoothness(\"" + selName + "\", \"" + type + "\", " +
                                         val.ToString("f2", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Change hyperball representation parameters in the specified selection to a preset
    /// Metaphores can be "Smooth", "Balls&Sticks", "VdW", "Licorice"
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    public static void setMetal(string selName, string type, float val) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            if (sr.atomRepManager != null) {
                                sr.atomRepManager.SetMetal(val);
                            }
                            if (sr.bondRepManager != null) {
                                sr.bondRepManager.SetMetal(val);
                            }
                        }
                    }
                }
            }
            else {
                Debug.LogError("Wrong representation type");
                return;
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setMetal(\"" + selName + "\", \"" + type + "\", " +
                                         val.ToString("f2", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Change surface wireframe size
    /// </summary>
    public static void setSurfaceWireframe(string selName, string type, float val) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            if (sr.atomRepManager != null) {
                                ((UnityMolSurfaceManager)sr.atomRepManager).SetWireframeSize(val);
                            }
                        }
                    }
                }
            }
            else {
                Debug.LogError("Wrong representation type");
                return;
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setSurfaceWireframe(\"" + selName + "\", \"" + type + "\", " +
                                         val.ToString("f2", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Change hyperball representation parameters in all selections that contains a hb representation
    /// Metaphores can be "Smooth", "Balls&Sticks", "VdW", "Licorice"
    /// </summary>
    public static void setHyperBallMetaphore(string metaphore, bool forceAOOff = false) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        float scaleBond = 1.0f;
        float scaleAtom = 1.0f;
        float shrink = 0.1f;
        bool doAO = false;

        switch (metaphore) {
        case "Smooth":
        case "smooth":
            scaleAtom = 1.0f;
            scaleBond = 1.0f;
            shrink = 0.4f;
            break;
        case "Balls&Sticks":
        case "Ball&Stick":
        case "BallsAndSticks":
        case "Ballandstick":
        case "bas":
        case "ballsandsticks":
        case "ballandstick":
            scaleAtom = 0.5f;
            scaleBond = 0.2f;
            shrink = 0.001f;
            break;
        case "VdW":
        case "vdw":
        case "VDW":
            scaleAtom = 3.0f;
            scaleBond = 0.0f;
            shrink = 1.0f;
            if (!forceAOOff)
                doAO = true;
            break;
        case "Licorice":
        case "licorice":
            scaleAtom = 0.3f;
            scaleBond = 0.3f;
            shrink = 0.001f;
            break;
        default:
            Debug.LogError("Metaphore not recognized");
            return;
        }

        foreach (string selName in selM.selections.Keys) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType("hb");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolHStickMeshManager hsManager = (UnityMolHStickMeshManager) sr.bondRepManager;
                        UnityMolHBallMeshManager hbManager = (UnityMolHBallMeshManager) sr.atomRepManager;
                        if (hsManager != null) {
                            hsManager.SetShrink(shrink);
                            hsManager.SetSizes(sel.atoms, scaleBond);
                        }

                        hbManager.SetSizes(sel.atoms, scaleAtom);
                        if (doAO) {
                            hbManager.computeAO();
                        }
                    }
                }
            }
        }

        UnityMolMain.recordPythonCommand("setHyperBallMetaphore(\"" + metaphore + "\", " + cBoolToPy(forceAOOff) + ")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Change hyperball representation parameters in the specified selection to a preset
    /// Metaphores can be "Smooth", "Balls&Sticks", "VdW", "Licorice"
    /// </summary>
    public static void setHyperBallMetaphore(string selName, string metaphore, bool forceAOOff = false) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        float scaleBond = 1.0f;
        float scaleAtom = 1.0f;
        float shrink = 0.1f;
        bool doAO = false;

        switch (metaphore) {
        case "Smooth":
        case "smooth":
            scaleAtom = 1.0f;
            scaleBond = 1.0f;
            shrink = 0.4f;
            break;
        case "Balls&Sticks":
        case "Ball&Stick":
        case "BallsAndSticks":
        case "Ballandstick":
        case "bas":
        case "ballsandsticks":
        case "ballandstick":
            scaleAtom = 0.5f;
            scaleBond = 0.2f;
            shrink = 0.001f;
            break;
        case "VdW":
        case "vdw":
        case "VDW":
            scaleAtom = 3.0f;
            scaleBond = 0.0f;
            shrink = 1.0f;
            if (!forceAOOff)
                doAO = true;
            break;
        case "Licorice":
        case "licorice":
            scaleAtom = 0.3f;
            scaleBond = 0.3f;
            shrink = 0.001f;
            break;
        default:
            Debug.LogError("Metaphore not recognized");
            return;
        }

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType("hb");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolHStickMeshManager hsManager = (UnityMolHStickMeshManager) sr.bondRepManager;
                        UnityMolHBallMeshManager hbManager = (UnityMolHBallMeshManager) sr.atomRepManager;
                        if (hsManager != null) {
                            hsManager.SetShrink(shrink);
                            hsManager.SetSizes(sel.atoms, scaleBond);
                        }

                        hbManager.SetSizes(sel.atoms, scaleAtom);
                        if (doAO) {
                            hbManager.computeAO();
                        }
                    }
                }
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setHyperBallMetaphore(\"" + selName + "\", \"" + metaphore + "\", " + cBoolToPy(forceAOOff) + ")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Set shininess for the hyperball representations of the specified selection
    /// </summary>
    public static void setHyperBallShininess(string selName, float shin) {

        float prev = 0.0f;

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType("hb");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {

                        UnityMolHStickMeshManager hsManager = (UnityMolHStickMeshManager) sr.bondRepManager;
                        UnityMolHBallMeshManager hbManager = (UnityMolHBallMeshManager) sr.atomRepManager;

                        prev = hbManager.shininess;

                        hbManager.SetShininess(shin);
                        hsManager.SetShininess(shin);
                    }
                }
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setHyperBallShininess(\"" + selName + "\", " +
                                         shin.ToString("f2", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("setHyperBallShininess(\"" + selName + "\", " +
                                             prev.ToString("f2", culture) + ")");
    }

    /// <summary>
    /// Set the shrink factor for the hyperball representations of the specified selection
    /// </summary>
    public static void setHyperballShrink(string selName, float shrink) {

        float prev = 0.0f;

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType("hb");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {

                        UnityMolHStickMeshManager hsManager = (UnityMolHStickMeshManager) sr.bondRepManager;

                        prev = hsManager.shrink;

                        hsManager.SetShrink(shrink);
                    }
                }
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setHyperballShrink(\"" + selName + "\", " +
                                         shrink.ToString("f2", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("setHyperballShrink(\"" + selName + "\", " +
                                             prev.ToString("f2", culture) + ")");
    }


    /// <summary>
    /// Change all hyperball representation in the selection with a new texture mapped
    /// idTex of the texture is the index in UnityMolMain.atomColors.textures
    /// </summary>
    public static void setHyperballTexture(string selName, int idTex) {

        if (idTex >= UnityMolMain.atomColors.textures.Length) {
            Debug.LogError("Invalid Texture index " + idTex + " " + UnityMolMain.atomColors.textures.Length);
            return;
        }

        float prev = 0.0f;

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType("hb");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                Texture tex = null;
                if (idTex >= 0) {
                    tex = (Texture) UnityMolMain.atomColors.textures[idTex];
                }
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolHBallMeshManager hbManager = (UnityMolHBallMeshManager) sr.atomRepManager;
                        UnityMolHStickMeshManager hsManager = (UnityMolHStickMeshManager) sr.bondRepManager;
                        hbManager.SetTexture(tex);
                        hsManager.SetTexture(tex);
                    }
                }
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setHyperballTexture(\"" + selName + "\", " + idTex + ")");
        UnityMolMain.recordUndoPythonCommand("setHyperballTexture(\"" + selName + "\", 0)");
    }

/// <summary>
    /// Change all bond order representation in the selection with a new texture mapped
    /// idTex of the texture is the index in UnityMolMain.atomColors.textures
    /// </summary>
    public static void setBondOrderTexture(string selName, int idTex) {

        if (idTex >= UnityMolMain.atomColors.textures.Length) {
            Debug.LogError("Invalid Texture index " + idTex + " " + UnityMolMain.atomColors.textures.Length);
            return;
        }

        float prev = 0.0f;

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType("bondorder");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                Texture tex = null;
                if (idTex >= 0) {
                    tex = (Texture) UnityMolMain.atomColors.textures[idTex];
                }
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolAtomBondOrderManager hbManager = (UnityMolAtomBondOrderManager) sr.atomRepManager;
                        UnityMolBondBondOrderManager hsManager = (UnityMolBondBondOrderManager) sr.bondRepManager;
                        hbManager.SetTexture(tex);
                        hsManager.SetTexture(tex);
                    }
                }
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setBondOrderTexture(\"" + selName + "\", " + idTex + ")");
        UnityMolMain.recordUndoPythonCommand("setBondOrderTexture(\"" + selName + "\", 0)");
    }



    public static void clearHyperballAO(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType("hb");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolHBallMeshManager hbManager = (UnityMolHBallMeshManager) sr.atomRepManager;
                        hbManager.cleanAO();
                    }
                }
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("clearHyperballAO(\"" + selName + "\"s)");
        UnityMolMain.recordUndoPythonCommand("");
    }

    public static void clearSurfaceAO(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType("s");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolSurfaceManager sMana = (UnityMolSurfaceManager) sr.atomRepManager;
                        sMana.ClearAO();
                    }
                }
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("clearSurfaceAO(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    public static void increaseAmbientLight() {
        RenderSettings.ambientLight = RenderSettings.ambientLight * 1.3f;
    }
    public static void decreaseAmbientLight() {
        RenderSettings.ambientLight = RenderSettings.ambientLight * 0.7f;
    }

    /// <summary>
    /// Set the color of the cartoon representation of the specified selection based on the nature of secondary structure assigned
    /// ssType can be "helix", "sheet" or "coil"
    /// </summary>
    public static void setCartoonColorSS(string selName, string ssType, Color col) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType("c");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {

                        if (sr.atomRepManager != null) {
                            UnityMolCartoonManager cManager = (UnityMolCartoonManager) sr.atomRepManager;
                            if (ssType == "helix") {
                                MDAnalysisSelection selec = new MDAnalysisSelection("ss " + ssType, sel.atoms);
                                UnityMolSelection selSS = selec.process();
                                cManager.SetColor(col, selSS);
                            }
                            if (ssType == "sheet") {
                                MDAnalysisSelection selec = new MDAnalysisSelection("ss " + ssType, sel.atoms);
                                UnityMolSelection selSS = selec.process();
                                cManager.SetColor(col, selSS);
                            }
                            if (ssType == "coil") {
                                MDAnalysisSelection selec = new MDAnalysisSelection("ss " + ssType, sel.atoms);
                                UnityMolSelection selSS = selec.process();
                                cManager.SetColor(col, selSS);
                            }
                            sr.atomRep.colorationType = colorType.custom;
                        }
                    }
                }
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand(String.Format(CultureInfo.InvariantCulture, "setCartoonColorSS(\"{0}\", \"{1}\", {2})", selName, ssType, col));
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Change the size of the representation of type 'type' in the selection
    /// Mainly used for hyperball representation
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    public static void setRepSize(string selName, string type, float size) {

        float prev = 0.0f;

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            if (sr.atomRepManager != null) {
                                sr.atomRepManager.SetSizes(sel.atoms, size);
                            }
                            if (sr.bondRepManager != null) {
                                sr.bondRepManager.SetSizes(sel.atoms, size);
                            }
                        }
                    }
                }
            }
            else {
                Debug.LogError("Wrong representation type");
                return;
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setRepSize(\"" + selName + "\", \"" + type + "\", " + size.ToString("f2", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("setRepSize(\"" + selName + "\", \"" + type + "\", 1.0)");

    }


    /// <summary>
    /// Change the color of all representation of type 'type' in selection
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    public static void colorSelection(string selName, string type, Color col) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            if (sr.atomRepManager != null) {
                                sr.atomRepManager.SetColor(col, sel);
                                sr.atomRep.colorationType = colorType.full;
                            }
                            if (sr.bondRepManager != null) {
                                sr.bondRepManager.SetColor(col, sel);
                                sr.bondRep.colorationType = colorType.full;
                            }
                        }
                    }
                }
            }
            else {
                Debug.LogError("Wrong representation type");
                return;
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand(String.Format(CultureInfo.InvariantCulture, "colorSelection(\"{0}\", \"{1}\", {2})", selName, type, col));
        UnityMolMain.recordUndoPythonCommand("");

    }

    /// <summary>
    /// Change the color of all representation of type 'type' in selection
    /// colorS can be "black", "white", "yellow", "green", "red", "blue", "pink", "gray"
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    public static void colorSelection(string selName, string type, string colorS) {

        colorS = colorS.ToLower();
        Color col = strToColor(colorS);
        colorSelection(selName, type, col);

        UnityMolMain.recordPythonCommand("colorSelection(\"" + selName + "\", \"" + type + "\", \"" + colorS + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Change the color of all representation of type 'type' in selection
    /// colors is a list of colors the length of the selection named selName
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    public static void colorSelection(string selName, string type, List<Color> colors) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];
            if (sel.Count != colors.Count) {
                Debug.LogError("Length of the 'colors' parameter does not have the length of the selection");
                return;
            }

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.SetColors(sel.atoms, colors);
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            if (sr.atomRepManager != null) {
                                sr.atomRep.colorationType = colorType.custom;
                            }
                            if (sr.bondRepManager != null) {
                                sr.bondRep.colorationType = colorType.custom;
                            }
                        }
                    }
                }
            }
            else {
                Debug.LogError("Wrong representation type");
                return;
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        string command = "colorSelection(\"" + selName + "\", \"" + type + "\", [";
        for (int i = 0; i < colors.Count; i++) {
            command += "Color(" + colors[i].r.ToString("F3", culture) + ", " +
                       colors[i].g.ToString("F3", culture) + ", " +
                       colors[i].b.ToString("F3", culture) + ", " +
                       colors[i].a.ToString("F3", culture) + ")";
            if (i != colors.Count - 1) {
                command += ", ";
            }
        }
        command += "])";

        UnityMolMain.recordPythonCommand(command);
        UnityMolMain.recordUndoPythonCommand("");
    }


    /// <summary>
    /// Reset the color of all representation of type 'type' in selection to the default value
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    public static void resetColorSelection(string selName, string type) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            if (sr.atomRepManager != null) {
                                sr.atomRepManager.ResetColors();
                            }
                            if (sr.bondRepManager != null) {
                                sr.bondRepManager.ResetColors();
                            }
                        }
                    }
                }
            }
            else {
                Debug.LogError("Wrong representation type");
                return;
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("resetColorSelection(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("");

    }

    /// <summary>
    /// In the representation of type repType, color all atoms of type atomType in the selection selName with
    /// </summary>
    public static void colorAtomType(string selName, string repType, string atomType, Color col) {


        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType rt = getRepType(repType);
            if (rt.atomType != AtomType.noatom || rt.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, rt);
                if (existingReps != null) {

                    //Use MDASelection to benefit from the wildcard
                    MDAnalysisSelection selec = new MDAnalysisSelection("type " + atomType, sel.atoms);
                    UnityMolSelection selRes = selec.process();

                    // List<UnityMolAtom> atoms = new List<UnityMolAtoms>(sel.Count);
                    // foreach (UnityMolAtom a in sel.atoms) {
                    //     if (a.type == atomType) {
                    //         atoms.Add(a);
                    //     }
                    // }

                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            if (sr.atomRepManager != null) {
                                sr.atomRepManager.SetColors(col, selRes.atoms);
                            }
                            if (sr.bondRepManager != null) {
                                sr.bondRepManager.SetColors(col, selRes.atoms);
                            }
                        }
                    }
                }
            }
            else {
                Debug.LogError("Wrong representation type");
                return;
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }
        UnityMolMain.recordPythonCommand("colorAtomType(\"" + selName + "\", \"" + repType + "\", \"" + atomType + "\", "
                                         + String.Format(CultureInfo.InvariantCulture, "{0})", col));
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Use the color palette to color representations of type 'type' in the selection 'selName' by chain
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    public static void colorByChain(string selName, string type) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.ColorByChain();
                    }
                }
            }
            else {
                Debug.LogError("Wrong representation type");
                return;
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("colorByChain(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    // /// <summary>
    // /// Use the color palette to color representations of type 'type' in the selection 'selName' by model
    // /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    // /// </summary>
    // public static void colorByModel(string selName, string type) {

    //     UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
    //     UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

    //     if (selM.selections.ContainsKey(selName)) {
    //         UnityMolSelection sel = selM.selections[selName];

    //         RepType repType = getRepType(type);
    //         if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

    //             List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
    //             if (existingReps != null) {
    //                 foreach (UnityMolRepresentation existingRep in existingReps) {
    //                     existingRep.ColorByModel();
    //                 }
    //             }
    //         }
    //         else {
    //             Debug.LogError("Wrong representation type");
    //             return;
    //         }
    //     }
    //     else {
    //         Debug.LogWarning("No selection named '" + selName + "'");
    //         return;
    //     }

    //     UnityMolMain.recordPythonCommand("colorByModel(\"" + selName + "\", \"" + type + "\")");
    //     UnityMolMain.recordUndoPythonCommand("");
    // }

    /// <summary>
    /// Use the color palette to color representations of type 'type' in the selection 'selName' by residue
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    public static void colorByResidue(string selName, string type) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.ColorByResidue();
                    }
                }
            }
            else {
                Debug.LogError("Wrong representation type");
                return;
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("colorByResidue(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("");

    }

    /// <summary>
    /// Color representations of type 'type' in the selection 'selName' by atom
    /// </summary>
    public static void colorByAtom(string selName, string type) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.ColorByAtom();
                    }
                }
            }
            else {
                Debug.LogError("Wrong representation type");
                return;
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("colorByAtom(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Color representations of type 'type' in the selection 'selName' by hydrophobicity
    /// </summary>
    public static void colorByHydrophobicity(string selName, string type) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.ColorByHydro();
                    }
                }
            }
            else {
                Debug.LogError("Wrong representation type");
                return;
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("colorByHydrophobicity(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Color representations of type 'type' in the selection 'selName' by sequence (rainbow effect)
    /// </summary>
    public static void colorBySequence(string selName, string type) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.ColorBySequence();
                    }
                }
            }
            else {
                Debug.LogError("Wrong representation type");
                return;
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("colorBySequence(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Use the dx map to color by charge around atoms
    /// Only works for surface for now
    /// If normalizeDensity is set to true, the density values will be normalized
    /// if it is set to true, the default -10|10 range is used
    /// </summary>
    public static void colorByCharge(string selName, bool normalizeDensity = false, float minDens = -10.0f, float maxDens = 10.0f) {


        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType("s");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolSurfaceManager surfM = (UnityMolSurfaceManager)sr.atomRepManager;
                        surfM.ColorByCharge(normalizeDensity, minDens, maxDens);//Use default -10 | 10 range to show charge
                    }
                }
            }
            repType = getRepType("dxiso");

            existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolSurfaceManager surfM = (UnityMolSurfaceManager)sr.atomRepManager;
                        surfM.ColorByCharge(normalizeDensity, minDens, maxDens);//Use default -10 | 10 range to show charge
                    }
                }
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("colorByCharge(\"" + selName + "\", " +
                                         cBoolToPy(normalizeDensity) + ", " +
                                         minDens.ToString("F3", culture) + ", " +
                                         maxDens.ToString("F3", culture) + ")");

        UnityMolMain.recordUndoPythonCommand("");

    }

    /// <summary>
    /// Color residues by "restype": negatively charge = red, positively charged = blue, nonpolar = light yellow,
    /// polar = green, cys = orange
    /// </summary>
    public static void colorByResidueType(string selName, string type) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.ColorByResType();
                    }
                }
            }
            else {
                Debug.LogError("Wrong representation type");
                return;
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("colorByResidueType(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Color residues by "restype": negatively charge = red, positively charged = blue, neutral = white
    /// </summary>
    public static void colorByResidueCharge(string selName, string type) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.ColorByResCharge();
                    }
                }
            }
            else {
                Debug.LogError("Wrong representation type");
                return;
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("colorByResidueCharge(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Color residues by Bfactor
    /// </summary>
    public static void colorByBfactor(string selName, string type, Color startColor, Color endColor) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.ColorByBfactor(startColor, endColor);
                    }
                }
            }
            else {
                Debug.LogError("Wrong representation type");
                return;
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("colorByBfactor(\"" + selName + "\", \"" + type + "\", " +
                                         String.Format(CultureInfo.InvariantCulture, " {0}, {1})", startColor, endColor));
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Color residues by Bfactor: low to high = blue to red
    /// </summary>
    public static void colorByBfactor(string selName, string type) {
        colorByBfactor(selName, type, Color.blue, Color.red);
    }

    public static void setLineSize(string selName, float val) {


        RepType repType = getRepType("l");

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolBondLineManager lM = (UnityMolBondLineManager)sr.bondRepManager;
                        lM.SetWidth(val);
                    }
                }
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setLineSize(\"" + selName + "\", " + val.ToString("F3", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    public static void setTraceSize(string selName, float val) {


        RepType repType = getRepType("trace");

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolTubeManager tM = (UnityMolTubeManager)sr.atomRepManager;
                        tM.SetWidth(val);
                    }
                }
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setTraceSize(\"" + selName + "\", " + val.ToString("F3", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("");
    }


    /// <summary>
    /// Offsets all representations to center the structure 'structureName'
    /// Instead of moving the camera, move the loaded molecules to center them on the center of the camera
    /// </summary>
    public static void centerOnStructure(string structureName, bool lerp = false, bool recordCommand = true) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }
        UnityMolStructure s = null;

        if (sm.nameToStructure.ContainsKey(structureName)) {
            s = sm.GetStructure(structureName);
        }
        else {
            Debug.LogWarning("No molecule loaded name '" + structureName + "'");
            return;
        }

        var foundObjects = FindObjectsOfType<ManipulationManager>();
        ManipulationManager mm = getManipulationManager();

        if (mm != null) {
            mm.centerOnStructure(s, lerp);
        }

        if (recordCommand) {
            UnityMolMain.recordPythonCommand("centerOnStructure(\"" + structureName + "\", " + cBoolToPy(lerp) + ")");
            UnityMolMain.recordUndoPythonCommand("");
        }

    }

    public static ManipulationManager getManipulationManager() {
        var foundObjects = FindObjectsOfType<ManipulationManager>();
        ManipulationManager mm = null;

        if (foundObjects.Length > 0) {
            mm = foundObjects[0].GetComponent<ManipulationManager>();
        }
        else {
            mm = UnityMolMain.getRepresentationParent().AddComponent<ManipulationManager>();
        }
        return mm;
    }

    /// <summary>
    /// Offsets all representations to center the selection 'selName'
    /// </summary>
    public static void centerOnSelection(string selName, bool lerp = false) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        if (!selM.selections.ContainsKey(selName)) {
            Debug.LogError("Selection '" + selName + "' does not exist");
            return;
        }
        UnityMolSelection sel = selM.selections[selName];

        ManipulationManager mm = getManipulationManager();

        if (mm != null) {
            mm.centerOnSelection(sel, lerp);
        }

        UnityMolMain.recordPythonCommand("centerOnSelection(\"" + selName + "\", " + cBoolToPy(lerp) + ")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// CEAlign algorithm to align two proteins with "little to no sequence similarity", only uses Calpha atoms
    /// For more details: https://pymolwiki.org/index.php/Cealign
    /// </summary>
    public static void cealign(string selNameTarget, string selNameMobile) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        if (!selM.selections.ContainsKey(selNameTarget)) {
            Debug.LogError("Selection '" + selNameTarget + "' does not exist");
            return;
        }
        if (!selM.selections.ContainsKey(selNameMobile)) {
            Debug.LogError("Selection '" + selNameMobile + "' does not exist");
            return;
        }
        UnityMolSelection selTar = selM.selections[selNameTarget];
        UnityMolSelection selMob = selM.selections[selNameMobile];

        CEAlignWrapper.alignWithCEAlign(selTar, selMob);

        UnityMolMain.recordPythonCommand("cealign(\"" + selNameTarget + "\", \"" + selNameMobile + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }


    /// <summary>
    /// Create a UnityMolSelection based on MDAnalysis selection language (https://www.mdanalysis.org/docs/documentation_pages/selections.html)
    /// Returns a UnityMolSelection object, adding it to the selection manager if createSelection is true
    /// If a selection with the same name already exists and addToExisting is true, add atoms to the already existing selection
    /// Set forceCreate to true if the selection is empty but still need to generate the selection
    /// </summary>
    public static UnityMolSelection select(string selMDA, string name = "selection", bool createSelection = true, bool addToExisting = false, bool silent = false, bool setAsCurrentSelection = true, bool forceCreate = false, bool allModels = false, bool addToSelectionKeyword = true) {


        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            UnityMolMain.recordUndoPythonCommand("");
            return null;
        }

        if (addToExisting && selM.selections.ContainsKey(name)) {
            if (!selM.selections[name].isAlterable) {
                //Just print the warning from the setter
                selM.selections[name].atoms = null;//Not actually changing the selection
                return selM.selections[name];
            }
        }
        if (!addToExisting && selM.selections.ContainsKey(name)) {
            selM.Delete(name);
        }

        if (setAsCurrentSelection) {
            selM.ClearCurrentSelection();
        }

        UnityMolSelection result = new UnityMolSelection(new List<UnityMolAtom>(), newBonds: null, name, selMDA);

        List<UnityMolAtom> allAtoms = new List<UnityMolAtom>();

        if (allModels) {
            foreach (UnityMolStructure s in sm.loadedStructures) {
                foreach (UnityMolModel m in s.models) {
                    allAtoms.AddRange(m.allAtoms);
                }
            }
        }
        else {
            foreach (UnityMolStructure s in sm.loadedStructures) {
                allAtoms.AddRange(s.currentModel.allAtoms);
            }
        }

        MDAnalysisSelection selec = new MDAnalysisSelection(selMDA, allAtoms);
        result = result + selec.process();

        if (addToExisting && selM.selections.ContainsKey(name)) {
            result = selM.selections[name] + result;
            if (createSelection) {
                selM.SetCurrentSelection(result);
            }
            // Debug.LogWarning("Adding to existing selection: " + result);
            UnityMolMain.recordPythonCommand("select(\"" + selMDA + "\", \"" + name + "\", " +
                                             cBoolToPy(createSelection) + ", " + cBoolToPy(addToExisting) + ", " + cBoolToPy(silent) + ", " +
                                             cBoolToPy(setAsCurrentSelection) + ", " + cBoolToPy(forceCreate) + ", " + cBoolToPy(allModels) + ")");
            UnityMolMain.recordUndoPythonCommand("");
            return result;
        }

        //Should I record the selection
        if (forceCreate || (result.atoms.Count != 0 && createSelection)) {
            selM.SetCurrentSelection(result);
        }

        if (addToSelectionKeyword)
            selM.AddSelectionKeyword(name, name);

        UnityMolMain.recordPythonCommand("select(\"" + selMDA + "\", \"" + name + "\", " +
                                         cBoolToPy(createSelection) + ", " + cBoolToPy(addToExisting) + ", " + cBoolToPy(silent) + ", " +
                                         cBoolToPy(setAsCurrentSelection) + ", " + cBoolToPy(forceCreate) + ", " + cBoolToPy(allModels) + ", " +
                                         cBoolToPy(addToSelectionKeyword) + ")");

        UnityMolMain.recordUndoPythonCommand("deleteSelection(\"" + result.name + "\")");
        if (!silent) {
            Debug.Log(result);
        }
        return result;
    }


    /// <summary>
    /// Add a keyword to the selection language
    /// </summary>
    public static void addSelectionKeyword(string keyword, string selName) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        selM.AddSelectionKeyword(keyword, selName);
        UnityMolMain.recordPythonCommand("addSelectionKeyword(\"" + keyword + "\", \"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("removeSelectionKeyword(\"" + keyword + "\", \"" + selName + "\")");
    }

    /// <summary>
    /// Remove a keyword from the selection language
    /// </summary>
    public static void removeSelectionKeyword(string keyword, string selName) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        selM.RemoveSelectionKeyword(keyword);
        UnityMolMain.recordPythonCommand("removeSelectionKeyword(\"" + keyword + "\", \"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("addSelectionKeyword(\"" + keyword + "\", \"" + selName + "\")");
    }

    /// <summary>
    /// Set the selection as currentSelection in the UnityMolSelectionManager
    /// </summary>
    public static void setCurrentSelection(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        if (!selM.selections.ContainsKey(selName)) {
            Debug.LogError("Cannot set selection '" + selName + "' as current as it does not exist");
            return;
        }
        UnityMolSelection sel = selM.selections[selName];
        selM.SetCurrentSelection(sel);

        UnityMolMain.recordPythonCommand("setCurrentSelection(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }


    /// <summary>
    /// Look for an existing selection named 'name' and add atoms to it based on MDAnalysis selection language
    /// </summary>
    public static void addToSelection(string selMDA, string name = "selection", bool silent = false, bool allModels = false) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }
        if (!selM.selections.ContainsKey(name)) {
            Debug.LogError("Cannot modify selection '" + name + "' as it does not exist");
            return;
        }

        //Process the selection
        UnityMolSelection result = new UnityMolSelection(new List<UnityMolAtom>(), name, selMDA);

        List<UnityMolAtom> allAtoms = new List<UnityMolAtom>();

        if (allModels) {
            foreach (UnityMolStructure s in sm.loadedStructures) {
                foreach (UnityMolModel m in s.models) {
                    allAtoms.AddRange(m.allAtoms);
                }
            }
        }
        else {
            foreach (UnityMolStructure s in sm.loadedStructures) {
                allAtoms.AddRange(s.currentModel.allAtoms);
            }
        }

        MDAnalysisSelection selec = new MDAnalysisSelection(selMDA, allAtoms);
        UnityMolSelection ret = selec.process();
        result = result + ret;

        // Debug.Log("Removing "+result+" from "+selM.selections[name]);

        //Add atoms of the selection to the existing one
        UnityMolSelection sumSel = (selM.selections[name] + result);
        selM.selections[name] = sumSel;

#if !DISABLE_HIGHLIGHT
        UnityMolHighlightManager hM = UnityMolMain.getHighlightManager();
        hM.Clean();
        hM.HighlightAtoms(sumSel);
#endif

        UnityMolMain.recordPythonCommand("addToSelection(\"" + selMDA + "\", \"" + name + "\", " + cBoolToPy(silent) + ", " + cBoolToPy(allModels) + ")");
        UnityMolMain.recordUndoPythonCommand("");

        if (!silent) {
            Debug.Log(sumSel);
        }
    }

    /// <summary>
    /// Look for an existing selection named 'name' and remove atoms from it based on MDAnalysis selection language
    /// </summary>
    public static void removeFromSelection(string selMDA, string name = "selection", bool silent = false, bool allModels = false) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }
        if (!selM.selections.ContainsKey(name)) {
            Debug.LogError("Cannot modify selection '" + name + "' as it does not exist");
            return;
        }
        if (!selM.selections[name].isAlterable) {
            //Just print the warning from the setter
            selM.selections[name].atoms = null;//Not actually changing the selection
            return;
        }

        UnityMolSelection result = new UnityMolSelection(new List<UnityMolAtom>(), name, selMDA);
        List<UnityMolAtom> allAtoms = new List<UnityMolAtom>();

        if (allModels) {
            foreach (UnityMolStructure s in sm.loadedStructures) {
                foreach (UnityMolModel m in s.models) {
                    allAtoms.AddRange(m.allAtoms);
                }
            }
        }
        else {
            foreach (UnityMolStructure s in sm.loadedStructures) {
                allAtoms.AddRange(s.currentModel.allAtoms);
            }
        }

        MDAnalysisSelection selec = new MDAnalysisSelection(selMDA, allAtoms);
        UnityMolSelection ret = selec.process();
        result = result + ret;

        // Debug.Log("Removing "+result+" from "+selM.selections[name]);

        //Remove atoms of the selection to the existing one
        selM.selections[name] = (selM.selections[name] - result);

#if !DISABLE_HIGHLIGHT
        UnityMolHighlightManager hM = UnityMolMain.getHighlightManager();
        hM.Clean();
        hM.HighlightAtoms(selM.selections[name]);
#endif

        UnityMolMain.recordPythonCommand("removeFromSelection(\"" + selMDA + "\", \"" + name + "\", " + cBoolToPy(silent) + ", " + cBoolToPy(allModels) + ")");
        UnityMolMain.recordUndoPythonCommand("");

        if (!silent) {
            Debug.Log(selM.selections[name]);
        }
    }


    /// <summary>
    /// Delete selection 'selName' and all its representations
    /// </summary>
    public static void deleteSelection(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        if (!selM.selections.ContainsKey(selName)) {
            Debug.LogError("Cannot delete selection '" + selName + "' as it does not exist");
            return;
        }

        selM.Delete(selName);
        selM.RemoveSelectionKeyword(selName);

        UnityMolMain.recordPythonCommand("deleteSelection(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Duplicate selection 'selName' and without the representations
    /// </summary>
    public static string duplicateSelection(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (!selM.selections.ContainsKey(selName)) {
            Debug.LogError("Cannot delete selection '" + selName + "' as it does not exist");
            return null;
        }

        string newSelName = selM.findNewSelectionName(selName);

        UnityMolSelection sel = selM.selections[selName];
        UnityMolSelection newSel = new UnityMolSelection(sel.atoms, sel.bonds, newSelName, sel.MDASelString);
        newSel.forceGlobalSelection = sel.forceGlobalSelection;
        newSel.isAlterable = sel.isAlterable;

        selM.Add(newSel);
        selM.AddSelectionKeyword(newSelName, newSelName);

        UnityMolMain.recordPythonCommand("duplicateSelection(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("deleteSelection(\"" + newSelName + "\")");
        return newSelName;
    }

    /// <summary>
    /// Change the 'oldSelName' selection name into 'newSelName'
    /// </summary>
    public static bool renameSelection(string oldSelName, string newSelName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        newSelName = newSelName.Replace(" ", "_");

        if (selM.selections.ContainsKey(newSelName)) {
            Debug.LogError("Cannot rename the selection to " +
                           newSelName + ", a selection with the same name already exists");
            return false;
        }
        if (!selM.selections.ContainsKey(oldSelName)) {
            Debug.LogError("Cannot rename the selection to " +
                           newSelName + ", the selection named " + oldSelName + " does not exist");
            return false;
        }
        UnityMolSelection sel = selM.selections[oldSelName];
        bool saveAlte = sel.isAlterable;
        sel.isAlterable = true;

        selM.selections.Remove(oldSelName);
        sel.name = newSelName;
        selM.Add(sel);

        sel.isAlterable = saveAlte;
        Debug.Log("Renamed selection '" + oldSelName + "'' to '" + newSelName + "'");

        selM.RemoveSelectionKeyword(oldSelName);
        selM.AddSelectionKeyword(newSelName, newSelName);

        UnityMolMain.recordPythonCommand("renameSelection(\"" + oldSelName + "\", \"" + newSelName + "\")");
        UnityMolMain.recordUndoPythonCommand("renameSelection(\"" + newSelName + "\", \"" + oldSelName + "\")");

        return true;
    }

    /// <summary>
    /// Update the atoms of the selection based on a new MDAnalysis language selection
    /// The selection only applies to the structures of the selection
    /// </summary>
    public static bool updateSelectionWithMDA(string selName, string selectionString, bool forceAlteration, bool silent = false, bool recordCommand = true, bool allModels = false) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (!selM.selections.ContainsKey(selName)) {
            Debug.LogError("Cannot update the selection '" + selName + "' as it does not exist");
            return false;
        }

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return false;
        }

        try {
            UnityMolSelection sel = selM.selections[selName];

            UnityMolSelection result = new UnityMolSelection(new List<UnityMolAtom>(), "", selectionString);
            List<UnityMolAtom> allAtoms = new List<UnityMolAtom>();

            if (sel.forceGlobalSelection || sel.structures == null || sel.structures.Count == 0) {

                if (allModels) {
                    foreach (UnityMolStructure s in sm.loadedStructures) {
                        foreach (UnityMolModel m in s.models) {
                            allAtoms.AddRange(m.allAtoms);
                        }
                    }
                }
                else {
                    foreach (UnityMolStructure s in sm.loadedStructures) {
                        allAtoms.AddRange(s.currentModel.allAtoms);
                    }
                }
                MDAnalysisSelection selec = new MDAnalysisSelection(selectionString, allAtoms);
                UnityMolSelection ret = selec.process();
                result = result + ret;

            }
            else {
                if (allModels) {
                    foreach (UnityMolStructure s in sel.structures) {
                        foreach (UnityMolModel m in s.models) {
                            allAtoms.AddRange(m.allAtoms);
                        }
                    }
                }
                else {
                    foreach (UnityMolStructure s in sel.structures) {
                        allAtoms.AddRange(s.currentModel.allAtoms);
                    }
                }


                MDAnalysisSelection selec = new MDAnalysisSelection(selectionString, allAtoms);
                UnityMolSelection ret = selec.process();
                result = result + ret;

            }
            //Don't update representations if the selection did not change
            if (sel.sameAtoms(result)) {
                return true;
            }


            bool saveAlte = sel.isAlterable;
            if (forceAlteration) {
                sel.isAlterable = true;
            }

            sel.atoms = result.atoms;
            sel.bonds = result.bonds;


            sel.fromSelectionLanguage = true;
            sel.MDASelString = result.MDASelString;
            sel.fillStructures();

            selM.selections[selName] = sel;

            if (!silent) {
                Debug.LogWarning("Modified the selection '" + selName + "' now with " + sel.Count + " atoms");
            }

            // selM.SetCurrentSelection(sel);

            updateRepresentations(selName);

            if (forceAlteration) {
                sel.isAlterable = saveAlte;
            }

        }
        catch (System.Exception e) {
#if UNITY_EDITOR
            Debug.LogError("Failed to update the selection: " + e);
            return false;
#endif
        }

        if (recordCommand) {
            UnityMolMain.recordPythonCommand("updateSelectionWithMDA(\"" + selName + "\", \"" + selectionString + "\", "
                                             + cBoolToPy(forceAlteration) + ", " + cBoolToPy(silent) + ", " + cBoolToPy(allModels) + ")");
            UnityMolMain.recordUndoPythonCommand("");
        }

        return true;
    }

    public static void cleanHighlight() {
        UnityMolHighlightManager hM = UnityMolMain.getHighlightManager();

        hM.Clean();
        UnityMolMain.recordPythonCommand("cleanHighlight()");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Select atoms of all loaded molecules inside a sphere defined by a world space position and a radius in Anstrom
    /// </summary>
    public static UnityMolSelection selectInSphere(Vector3 position, float radius) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return null;
        }

        selM.ClearCurrentSelection();

        UnityMolSelection result = new UnityMolSelection(new List<UnityMolAtom>(), "selection", "insphere");

        foreach (UnityMolStructure s in sm.loadedStructures) {

            //Translate world space sphere center and radius into the local structure space
            GameObject sgo = sm.structureToGameObject[s.uniqueName];
            Vector3 sphereCenter = sgo.transform.InverseTransformPoint(position);
            float sphereRadius = radius / sgo.transform.lossyScale.x / 2;
            string selMDA = "insphere " + sphereCenter.x + " " + sphereCenter.y + " " + sphereCenter.z + " " + sphereRadius.ToString("F3");
            MDAnalysisSelection selec = new MDAnalysisSelection(selMDA, s.currentModel.allAtoms);
            UnityMolSelection ret = selec.process();
            result = result + ret;
        }

        //Should I record the selection
        selM.SetCurrentSelection(result);
        Debug.Log(result);

        UnityMolMain.recordPythonCommand(String.Format(CultureInfo.InvariantCulture, "selectInSphere(Vector3({0}, {1}, {2}), {3})",
                                         position.x, position.y, position.z, radius));

        UnityMolMain.recordUndoPythonCommand("deleteSelection(\"" + result.name + "\")");

        return result;
    }

    /// <summary>
    /// Update representations of the specified selection, called automatically after a selection content change
    /// </summary>
    public static void updateRepresentations(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (!selM.selections.ContainsKey(selName)) {
            Debug.LogError("Cannot update representations of the selection '" + selName + "' as it does not exist");
            return;
        }
        UnityMolSelection sel = selM.selections[selName];

        // if (sel.structures.Count != 1) {
        // Debug.Log("Several structures");
        // Dictionary<UnityMolStructure, UnityMolSelection> byStruc = repManager.cutSelectionByStructure(sel);

        // //Save the state of the representations
        // List<bool> displayedRep = new List<bool>();
        // foreach (List<UnityMolRepresentation> reps in sel.representations.Values) {
        //     foreach (UnityMolRepresentation r in reps) {
        //         displayedRep.Add(r.shouldEnable());
        //     }
        // }

        // selM.DeleteRepresentations(sel);


        // }
        // else {

        foreach (List<UnityMolRepresentation> reps in sel.representations.Values) {
            foreach (UnityMolRepresentation r in reps) {
                r.updateWithNewSelection(sel);
            }
        }
        // }
    }

    /// <summary>
    /// Clear the currentSelection in UnityMolSelectionManager
    /// </summary>
    public static void clearSelections() {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        selM.ClearCurrentSelection();

        UnityMolMain.recordPythonCommand("clearSelections()");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Utility function to test if a trajectory is playing for any loaded molecule
    /// </summary>
    public static bool isATrajectoryPlaying() {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        foreach (UnityMolStructure s in sm.loadedStructures) {
            if (s.trajPlayer && s.trajPlayer.play) {
                return true;
            }
        }
        return false;
    }

    public static void setUpdateSelectionTraj(string selName, bool v) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (!selM.selections.ContainsKey(selName)) {
            Debug.LogError("Selection '" + selName + "' not found");
            return;
        }
        selM.selections[selName].updateWithTraj = v;
        UnityMolMain.recordPythonCommand("setUpdateSelectionTraj(\"" + selName + "\", " + cBoolToPy(v) + ")");
        UnityMolMain.recordUndoPythonCommand("setUpdateSelectionTraj(\"" + selName + "\", " + cBoolToPy(!v) + ")");
    }

    /// <summary>
    /// Show or hide representation shadows
    /// </summary>
    public static void setShadows(string selName, string type, bool enable) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType(type);

            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);

                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.ShowShadows(enable);
                    }
                }
            }
            else {
                Debug.LogError("Wrong representation type");
                return;
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }
        UnityMolMain.recordPythonCommand("setShadows(\"" + selName + "\", \"" + type + "\", " + enable + ")");
        UnityMolMain.recordUndoPythonCommand("setShadows(\"" + selName + "\", \"" + type + "\", " + !enable + ")");
    }

    /// <summary>
    /// Enable DOF effect, only available in desktop mode
    /// </summary>
    public static void enableDOF() {
        if (UnityMolMain.inVR()) {
            Debug.LogWarning("Cannot enable DOF in VR");
            return;
        }
        try {
            MouseAutoFocus maf = Camera.main.gameObject.GetComponent<MouseAutoFocus>();
            MouseOverSelection mos = Camera.main.gameObject.GetComponent<MouseOverSelection>();
            if (maf == null) {
                maf = Camera.main.gameObject.AddComponent<MouseAutoFocus>();
            }
            maf.Init();
            maf.enableDOF();

        }
        catch {
            Debug.LogError("Couldn't enable DOF");
            return;
        }
        UnityMolMain.recordPythonCommand("enableDOF()");
        UnityMolMain.recordUndoPythonCommand("disableDOF()");
    }

    /// <summary>
    /// Disable DOF effect, only available in desktop mode
    /// </summary>
    public static void disableDOF() {

        try {
            MouseAutoFocus maf = Camera.main.gameObject.GetComponent<MouseAutoFocus>();
            MouseOverSelection mos = Camera.main.gameObject.GetComponent<MouseOverSelection>();
            if (maf == null) {
                maf = Camera.main.gameObject.AddComponent<MouseAutoFocus>();
            }
            maf.Init();
            maf.disableDOF();

        }
        catch {
            Debug.LogError("Couldn't disable DOF");
            return;
        }
        UnityMolMain.recordPythonCommand("disableDOF()");
        UnityMolMain.recordUndoPythonCommand("enableDOF()");
    }

    /// <summary>
    /// Set DOF aperture
    /// </summary>
    public static void setDOFAperture(float a) {

        float prev = 0.0f;
        try {
            MouseAutoFocus maf = Camera.main.gameObject.GetComponent<MouseAutoFocus>();

            prev = maf.getAperture();
            maf.setAperture(a);

        }
        catch {
            Debug.LogError("Couldn't set DOF aperture");
            return;
        }
        UnityMolMain.recordPythonCommand("setDOFAperture(" + a.ToString("f3") + ")");
        UnityMolMain.recordUndoPythonCommand("setDOFAperture(" + prev.ToString("f3") + ")");
    }
    /// <summary>
    /// Set DOF focal length
    /// </summary>
    public static void setDOFFocalLength(float f) {

        float prev = 0.0f;
        try {
            MouseAutoFocus maf = Camera.main.gameObject.GetComponent<MouseAutoFocus>();

            prev = maf.getFocalLength();
            maf.setFocalLength(f);

        }
        catch {
            Debug.LogError("Couldn't set DOF focal length");
            return;
        }
        UnityMolMain.recordPythonCommand("setDOFFocalLength(" + f.ToString("f3") + ")");
        UnityMolMain.recordUndoPythonCommand("setDOFFocalLength(" + prev.ToString("f3") + ")");
    }
    /// <summary>
    /// Enable outline post-process effect
    /// </summary>
    public static void enableOutline() {

        try {
            OutlineEffectUtil outlineScript = Camera.main.gameObject.GetComponent<OutlineEffectUtil>();
            if (outlineScript == null) {
                outlineScript =  Camera.main.gameObject.AddComponent<OutlineEffectUtil>();
            }

            outlineScript.enableOutline();

        }
        catch {
            Debug.LogError("Couldn't enable Outline effect");
            return;
        }
        UnityMolMain.recordPythonCommand("enableOutline()");
        UnityMolMain.recordUndoPythonCommand("disableOutline()");
    }

    /// <summary>
    /// Disable outline effect
    /// </summary>
    public static void disableOutline() {

        try {
            OutlineEffectUtil outlineScript = Camera.main.gameObject.GetComponent<OutlineEffectUtil>();
            if (outlineScript == null) {
                outlineScript =  Camera.main.gameObject.AddComponent<OutlineEffectUtil>();
            }

            outlineScript.disableOutline();

        }
        catch {
            Debug.LogError("Couldn't disable Outline effect");
            return;
        }
        UnityMolMain.recordPythonCommand("disableOutline()");
        UnityMolMain.recordUndoPythonCommand("enableOutline()");
    }



    /// <summary>
    /// Utility function to change the material of highlighted selection
    /// </summary>
    public static void changeHighlightMaterial(Material newMat) {
#if !DISABLE_HIGHLIGHT
        UnityMolHighlightManager highlm = UnityMolMain.getHighlightManager();
        highlm.changeMaterial(newMat);
#endif
    }

    /// <summary>
    /// Take a screenshot of the current viewpoint with a specific resolution
    /// </summary>
    public static void screenshot(string path, int resolutionWidth = 1280, int resolutionHeight = 720, bool transparentBG = false) {
        RecordManager.takeScreenshot(path, resolutionWidth, resolutionHeight, transparentBG);
    }

    /// <summary>
    /// Start to record a video with FFMPEG at a specific resolution and framerate
    /// </summary>
    public static void startVideo(string filePath, int resolutionWidth = 1280, int resolutionHeight = 720, int frameRate = 30) {
        RecordManager.startRecordingVideo(filePath, resolutionWidth, resolutionHeight, frameRate);
    }
    /// <summary>
    /// Stop recording
    /// </summary>
    public static void stopVideo() {
        RecordManager.stopRecordingVideo();
    }

    // --------------- Python history functions
    /// <summary>
    /// Play the opposite function of the lastly called APIPython function recorded in UnityMolMain.pythonUndoCommands
    /// </summary>
    public static void undo() {
        if (UnityMolMain.pythonUndoCommands.Count == 0) {
            return;
        }
        string lastUndoCommand = UnityMolMain.pythonUndoCommands.Last();

        if (lastUndoCommand != null) {
            Debug.Log("Undo command = " + lastUndoCommand);
            pythonConsole.ExecuteCommand(lastUndoCommand);

            UnityMolMain.pythonUndoCommands.RemoveAt(UnityMolMain.pythonUndoCommands.Count - 1);
            UnityMolMain.pythonCommands.RemoveAt(UnityMolMain.pythonCommands.Count - 1);



            //Remove the 2 last commands, the undo + the previous command

            if (lastUndoCommand != "") {
                UnityMolMain.pythonCommands.RemoveAt(UnityMolMain.pythonCommands.Count - 1);
                UnityMolMain.pythonUndoCommands.RemoveAt(UnityMolMain.pythonUndoCommands.Count - 1);

                int count = lastUndoCommand.Split('\n').Length - 1;
                for (int i = 0; i < count; i++) {
                    UnityMolMain.pythonCommands.RemoveAt(UnityMolMain.pythonCommands.Count - 1);
                    UnityMolMain.pythonUndoCommands.RemoveAt(UnityMolMain.pythonUndoCommands.Count - 1);
                }
            }
        }

    }

    /// <summary>
    /// Set the local position and rotation (euler angles) of the given structure
    /// </summary>
    public static void setStructurePositionRotation(string structureName, Vector3 pos, Vector3 rot) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolStructure s = sm.GetStructure(structureName);

        Vector3 savePos = Vector3.zero;
        Vector3 saveRot = Vector3.zero;
        if (s != null) {
            GameObject sgo = sm.structureToGameObject[s.uniqueName];
            savePos = sgo.transform.localPosition;
            saveRot = sgo.transform.localEulerAngles;

            sgo.transform.localPosition = pos;
            sgo.transform.localRotation = Quaternion.Euler(rot);
        }
        else {
            Debug.LogError("Wrong structure name");
            return;
        }

        UnityMolMain.recordPythonCommand("setStructurePositionRotation( \"" + structureName + "\", Vector3(" + pos.x.ToString("F4", culture) + ", " +
                                         pos.y.ToString("F4", culture) + ", " + pos.z.ToString("F4", culture) + "), " +
                                         "Vector3(" + rot.x.ToString("F4", culture) + ", " +
                                         rot.y.ToString("F4", culture) + ", " + rot.z.ToString("F4", culture) + "))");
        UnityMolMain.recordUndoPythonCommand("setStructurePositionRotation( \"" + structureName + "\", Vector3(" + savePos.x.ToString("F4", culture) + ", " +
                                             savePos.y.ToString("F4", culture) + ", " + savePos.z.ToString("F4", culture) + "), " +
                                             "Vector3(" + saveRot.x.ToString("F4", culture) + ", " +
                                             saveRot.y.ToString("F4", culture) + ", " + saveRot.z.ToString("F4", culture) + "))");
    }

    public static void getStructurePositionRotation(string structureName, ref Vector3 pos, ref Vector3 rot) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolStructure s = sm.GetStructure(structureName);

        if (s != null) {
            GameObject sgo = sm.structureToGameObject[s.uniqueName];
            pos = sgo.transform.localPosition;
            rot = sgo.transform.localEulerAngles;
            return;
        }
        else {
            Debug.LogError("Wrong structure name");
            return;
        }
    }

    /// <summary>
    /// Save the history of commands executed in a file
    /// </summary>
    public static void saveHistoryScript(string path) {
        string scriptContent = UnityMolMain.commandHistory();

        //Set center to false in fetch and load commands => this is handled by the loadedMolParentToString function
        string[] commands = scriptContent.Split(new [] { '\n'}, System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < commands.Length; i++) {
            string c = commands[i];
            if (c.StartsWith("fetch(") || c.StartsWith("load(")) {
                commands[i] = commands[i].Replace(", center=True", ", center=False");
                commands[i] = commands[i].Replace(", center= True", ", center= False");
            }
        }
        scriptContent = string.Join("\n", commands);

        scriptContent += loadedMolParentToString();

        File.WriteAllText(path, scriptContent);

        Debug.Log("Saved history script to '" + path + "'");
    }


    static string loadedMolParentToString() {
        ManipulationManager mm = getManipulationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        string res = "";
        foreach (UnityMolStructure s in sm.loadedStructures) {
            Vector3 spos = Vector3.zero;
            Vector3 srot = Vector3.zero;
            getStructurePositionRotation(s.uniqueName, ref spos, ref srot);
            res += "\n\nsetStructurePositionRotation(\"" + s.uniqueName + "\", Vector3(" + spos.x.ToString("F4", culture) + ", " +
                   spos.y.ToString("F4", culture) + ", " + spos.z.ToString("F4", culture) + "), " +
                   "Vector3(" + srot.x.ToString("F4", culture) + ", " +
                   srot.y.ToString("F4", culture) + ", " + srot.z.ToString("F4", culture) + "))";
        }



        //Save parent state
        Transform parentT = UnityMolMain.getRepresentationParent().transform;
        res += "\n#Save parent position\nsetMolParentTransform( Vector3(" + parentT.position.x.ToString("F4", culture) + ", " +
               parentT.position.y.ToString("F4", culture) + ", " + parentT.position.z.ToString("F4", culture) + "), Vector3(" + parentT.localScale.x.ToString("F4", culture) + ", " +
               parentT.localScale.y.ToString("F4", culture) + ", " + parentT.localScale.z.ToString("F4", culture) + "), Vector3(" + parentT.eulerAngles.x.ToString("F4", culture) + ", " +
               parentT.eulerAngles.y.ToString("F4", culture) + ", " + parentT.eulerAngles.z.ToString("F4", culture) + "), Vector3(" + mm.currentCenterPosition.x.ToString("F4", culture) + ", " +
               mm.currentCenterPosition.y.ToString("F4", culture) + ", " + mm.currentCenterPosition.z.ToString("F4", culture) + ") )\n";

        return res;
    }

    public static void setRotationCenter(Vector3 newPos) {
        ManipulationManager mm = getManipulationManager();
        if (mm != null) {
            mm.setRotationCenter(newPos);
        }
    }

    /// <summary>
    /// Load a python script of commands (possibly the output of the saveHistoryScript function)
    /// </summary>
    public static void loadHistoryScript(string path) {
        System.IO.StreamReader file = new System.IO.StreamReader(path);
        List<string> scriptCommands = new List<string>();
        string line = "";
        while ((line = file.ReadLine()) != null)
        {
            scriptCommands.Add(line);
        }

        file.Close();
        foreach (string c in scriptCommands) {
            string comm = c.Trim();
            if (comm.Length == 0 || comm.StartsWith("#")) {
                continue;
            }
            try {
                pythonConsole.ExecuteCommand(comm);
            }
            catch (System.Exception e) {
                Debug.LogError("Could not execute command '" + comm + "'");
#if UNITY_EDITOR
                Debug.LogError(e);
#endif
                return;
            }
        }
        Debug.Log("Loaded history script from '" + path + "'");

    }

    /// <summary>
    /// Set the position, scale and rotation of the parent of all loaded molecules
    /// Linear interpolation between the current state of the camera to the specified values
    /// </summary>
    public static void setMolParentTransform(Vector3 pos, Vector3 scale, Vector3 rot, Vector3 centerOfRotation) {
        instance.setPosScaleRot(pos, scale, rot, centerOfRotation);
    }

    public void setPosScaleRot(Vector3 pos, Vector3 scale, Vector3 rot, Vector3 centerOfRotation) {
        Transform parentT = UnityMolMain.getRepresentationParent().transform;
        StartCoroutine( delayedSetTransform(parentT, pos, scale, rot, centerOfRotation));
    }

    IEnumerator delayedSetTransform(Transform t, Vector3 pos, Vector3 scale, Vector3 rot, Vector3 centerOfRotation) {
        //End of frame
        yield return 0;

        t.localScale = scale;
        int steps = 200;
        for (int i = 1; i < steps / 4; i++) {
            float tt = i / (float)steps;

            t.position = Vector3.Lerp(t.position, pos, tt);
            Vector3 newRot = new Vector3(Mathf.LerpAngle(t.eulerAngles.x, rot.x, tt),
                                         Mathf.LerpAngle(t.eulerAngles.y, rot.y, tt),
                                         Mathf.LerpAngle(t.eulerAngles.z, rot.z, tt));
            t.eulerAngles = newRot;
            yield return 0;
        }
        t.position = pos;
        t.eulerAngles = rot;
        setRotationCenter(centerOfRotation);
    }

    /// <summary>
    /// Change the scale of the parent of the representations of each molecules
    /// Try to not move the center of mass
    /// </summary>
    public static void changeGeneralScale_cog(float newVal) {
        if (newVal > 0.0f && newVal != Mathf.Infinity) {
            UnityMolStructureManager sm = UnityMolMain.getStructureManager();

            if (sm.loadedStructures.Count == 0) {
                Debug.LogWarning("No molecule loaded");
                return;
            }

            Transform molPar = UnityMolMain.getRepresentationParent().transform;

            List<Vector3> savedCog = new List<Vector3>();

            foreach (Transform t in molPar) {
                UnityMolStructure s = sm.selectionNameToStructure(t.name);
                if (s != null) {
                    savedCog.Add(t.TransformPoint(s.currentModel.centerOfGravity));
                }
            }

            molPar.localScale = Vector3.one * newVal;

            int i = 0;
            foreach (Transform t in molPar) {
                UnityMolStructure s = sm.selectionNameToStructure(t.name);
                if (s != null) {
                    Vector3 newCog = t.TransformPoint(s.currentModel.centerOfGravity);
                    t.Translate(savedCog[i++] - newCog, Space.World);
                }
            }
        }
    }

    /// <summary>
    /// Change the scale of the parent of the representations of each molecules
    /// Keep relative positions of molecules, use the first loaded molecule center of gravity to compensate the translation due to scaling
    /// </summary>
    public static void changeGeneralScale(float newVal) {
        if (newVal > 0.0f && newVal != Mathf.Infinity) {
            UnityMolStructureManager sm = UnityMolMain.getStructureManager();

            if (sm.loadedStructures.Count == 0) {
                Debug.LogWarning("No molecule loaded");
                return;
            }

            Transform molPar = UnityMolMain.getRepresentationParent().transform;

            if (molPar.childCount == 0) {
                return;
            }
            Transform t = molPar.GetChild(0);

            UnityMolStructure s = sm.selectionNameToStructure(t.name);
            Vector3 save = Vector3.zero;

            if (s != null) {
                save = t.TransformPoint(s.currentModel.centerOfGravity);

                molPar.localScale = Vector3.one * newVal;

                Vector3 newP = t.TransformPoint(s.currentModel.centerOfGravity);

                molPar.Translate( save - newP, Space.World);
            }

        }
    }

    /// <summary>
    /// Use Reduce method to add hydrogens
    /// </summary>
    public static void addHydrogensReduce(string structureName) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolStructure s = sm.GetStructure(structureName);

        if (s != null) { //Remove a complete loaded molecule
            ReduceWrapper.callReduceOnStructure(s);
        }
        else {
            Debug.LogError("Wrong structure name");
            return;
        }

        UnityMolMain.recordPythonCommand("addHydrogensReduce(\"" + structureName + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Use HAAD method to add hydrogens
    /// </summary>
    public static void addHydrogensHaad(string structureName) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolStructure s = sm.GetStructure(structureName);

        if (s != null) { //Remove a complete loaded molecule
            // ReduceWrapper.callReduceOnStructure(s);
            HaadWrapper.callHaadOnStructure(s);
        }
        else {
            Debug.LogError("Wrong structure name");
            return;
        }
        UnityMolMain.recordPythonCommand("addHydrogensHaad(\"" + structureName + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Set the atoms of the selection named 'selName' to ligand
    /// </summary>
    public static void setAsLigand(string selName, bool isLig = true, bool updateAllSelections = true) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();


        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            foreach (UnityMolAtom a in sel.atoms) {
                a.isLigand = isLig;

            }
            Debug.Log("Set " + sel.Count + " atom(s) as " + (isLig ? "" : "non-") + "ligand");
        }
        else {
            Debug.LogError("No selection named " + selName);
            return;
        }

        //Record command before calling another APIPython function
        UnityMolMain.recordPythonCommand("setAsLigand(\"" + selName + "\", " + cBoolToPy(isLig) + ", " + cBoolToPy(updateAllSelections) + ")");

        //Caution
        UnityMolMain.recordUndoPythonCommand("setAsLigand(\"" + selName + "\", " + cBoolToPy(!isLig) + ", True)");

        if (updateAllSelections) {
            List<UnityMolSelection> sels = selM.selections.Values.ToList();

            foreach (UnityMolSelection sele in sels) {
                if (sele.fromSelectionLanguage) {
                    updateSelectionWithMDA(sele.name, sele.MDASelString, true, recordCommand: false);
                }
            }
        }
    }

    /// <summary>
    /// Merge UnityMolStructure structureName2 in structureName using a different chain name to avoid conflict
    /// </summary>
    public static void mergeStructure(string structureName, string structureName2, string chainName = "Z") {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolStructure toMerge = sm.GetStructure(structureName2);
        if (s != null && toMerge != null) {
            s.MergeStructure(toMerge, chainName);
            UnityMolMain.recordPythonCommand("mergeStructure(\"" + structureName + "\", \"" + structureName2 + "\", \"" + chainName + "\")");
            UnityMolMain.recordUndoPythonCommand("");
        }
    }

    /// <summary>
    /// Save current atom positions of the selection to a PDB file
    /// World atom positions are transformed to be relative to the first structure in the selection
    /// </summary>
    public static void saveToPDB(string selName, string filePath, bool writeSSinfo = false) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];
            if (sel.Count == 0) {
                Debug.LogWarning("Empty selection");
                return;
            }

            Vector3[] atomPos = new Vector3[sel.Count];

            Transform strucPar = sel.structures[0].getAtomGos()[0].transform.parent.parent;

            int id = 0;
            foreach (UnityMolAtom a in sel.atoms) {
                atomPos[id++] = strucPar.InverseTransformPoint(a.curWorldPosition);
            }

            string pdbLines = PDBReader.Write(sel, overridedPos: atomPos, writeSS: writeSSinfo);
            try {
                StreamWriter writer = new StreamWriter(filePath, false);
                writer.WriteLine(pdbLines);
                writer.Close();
                Debug.Log("Wrote PDB file: '" + Path.GetFullPath(filePath) + "'");
            }
            catch {
                Debug.LogError("Failed to write to '" + Path.GetFullPath(filePath) + "'");
            }

        }
        else {
            Debug.LogError("No selection named " + selName);
            return;
        }
    }

    /// <summary>
    /// Connect to a running simulation using the IMD protocol implemented in Artemis
    /// The running simulation is binded to a UnityMolStructure
    /// </summary>
    public static bool connectIMD(string structureName, string adress, int port) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return false;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        bool res = false;
        try {
            if (s.artemisM != null) {
                Debug.LogError("Already connected to a running simulation");
                return false;
            }
            GameObject sgo = sm.structureToGameObject[s.uniqueName];
            s.artemisM = sgo.AddComponent<ArtemisManager>();
            s.artemisM.structure = s;

            res = s.artemisM.connect(adress, port);
            if (!res) {
                s.artemisM = null;
            }
        }
        catch (System.Exception e) {
            Debug.LogError("Could not connect to the simulation on " + adress + " : " + port + "\n " + e);
            return false;
        }

        UnityMolMain.recordPythonCommand("connectIMD(\"" + structureName + "\", \"" + adress + "\", " + port + ")");
        UnityMolMain.recordUndoPythonCommand("disconnectIMD(\"" + structureName + "\")");
        return res;
    }

    /// <summary>
    /// Disconnect from the IMD simulation for the specified structure
    /// </summary>
    public static void disconnectIMD(string structureName) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        s.disconnectIMD();

        UnityMolMain.recordPythonCommand("disconnectIMD(\"" + structureName + "\")");
        UnityMolMain.recordUndoPythonCommand("");

    }


    public static string getSurfaceType(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType("s");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);

            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolSurfaceManager surfM = (UnityMolSurfaceManager)sr.atomRepManager;

                        if (!surfM.isTransparent && !surfM.isWireframe) {
                            return "Solid";
                        }
                        if (surfM.isWireframe) {
                            return "Wireframe";
                        }
                        return "Transparent";
                    }
                }
            }

            repType = getRepType("dxiso");

            existingReps = repManager.representationExists(selName, repType);

            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolSurfaceManager surfM = (UnityMolSurfaceManager)sr.atomRepManager;

                        if (!surfM.isTransparent && !surfM.isWireframe) {
                            return "Solid";
                        }
                        if (surfM.isWireframe) {
                            return "Wireframe";
                        }
                        return "Transparent";
                    }
                }
            }
        }
        return "";
    }


    public static string getHyperBallMetaphore(string selName) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType("hb");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);

            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolHStickMeshManager hsManager = (UnityMolHStickMeshManager)sr.bondRepManager;
                        if (hsManager != null) {
                            if (hsManager.shrink == 0.4f && hsManager.scaleBond == 1.0f)
                                return "Smooth";
                            if (hsManager.shrink == 0.001f && hsManager.scaleBond == 0.2f)
                                return "BallsAndSticks";
                            if (hsManager.shrink == 1.0f && hsManager.scaleBond == 0.0f)
                                return "VdW";
                            if (hsManager.shrink == 0.001f && hsManager.scaleBond == 0.3f)
                                return "Licorice";
                        }
                    }
                }
            }
        }
        return "";
    }

    public static void setCameraNearPlane(float newV) {
        float prevVal = Camera.main.nearClipPlane;
        newV = Mathf.Clamp(newV, 0.001f, 100.0f);
        Camera.main.nearClipPlane = newV;
        UnityMolMain.recordPythonCommand("setCameraNearPlane(" + newV.ToString("F4", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("setCameraNearPlane(" + prevVal.ToString("F4", culture) + ")");
    }
    public static void setCameraFarPlane(float newV) {
        float prevVal = Camera.main.farClipPlane;
        newV = Mathf.Clamp(newV, 0.1f, 5000.0f);
        Camera.main.farClipPlane = newV;
        UnityMolMain.recordPythonCommand("setCameraFarPlane(" + newV.ToString("F4", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("setCameraFarPlane(" + prevVal.ToString("F4", culture) + ")");
    }


    public static void enableDepthCueing() {
        UnityMolMain.isFogOn = true;
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        foreach (UnityMolStructure s in sm.loadedStructures) {
            foreach (UnityMolRepresentation r in s.representations) {
                foreach (SubRepresentation sr in r.subReps) {
                    if (sr.atomRepManager != null) {
                        sr.atomRepManager.EnableDepthCueing();
                    }
                    if (sr.bondRepManager != null) {
                        sr.bondRepManager.EnableDepthCueing();
                    }
                }
            }
        }

        UnityMolMain.recordPythonCommand("enableDepthCueing()");
        UnityMolMain.recordUndoPythonCommand("disableDepthCueing()");
    }

    public static void disableDepthCueing() {
        UnityMolMain.isFogOn = false;
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        foreach (UnityMolStructure s in sm.loadedStructures) {
            foreach (UnityMolRepresentation r in s.representations) {
                foreach (SubRepresentation sr in r.subReps) {
                    if (sr.atomRepManager != null) {
                        sr.atomRepManager.DisableDepthCueing();
                    }
                    if (sr.bondRepManager != null) {
                        sr.bondRepManager.DisableDepthCueing();
                    }
                }
            }
        }
        UnityMolMain.recordPythonCommand("disableDepthCueing()");
        UnityMolMain.recordUndoPythonCommand("enableDepthCueing()");
    }

    public static void setDepthCueingStart(float v) {
        float prev = UnityMolMain.fogStart;
        UnityMolMain.fogStart = v;

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        foreach (UnityMolStructure s in sm.loadedStructures) {
            foreach (UnityMolRepresentation r in s.representations) {
                foreach (SubRepresentation sr in r.subReps) {
                    if (sr.atomRepManager != null) {
                        sr.atomRepManager.SetDepthCueingStart(UnityMolMain.fogStart);
                    }
                    if (sr.bondRepManager != null) {
                        sr.bondRepManager.SetDepthCueingStart(UnityMolMain.fogStart);
                    }
                }
            }
        }
        UnityMolMain.recordPythonCommand("setDepthCueingStart(" + UnityMolMain.fogStart.ToString("F2", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("setDepthCueingStart(" + prev.ToString("F2", culture) + ")");
    }

    public static void setDepthCueingDensity(float v) {
        float prev = UnityMolMain.fogDensity;
        UnityMolMain.fogDensity = v;

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        foreach (UnityMolStructure s in sm.loadedStructures) {
            foreach (UnityMolRepresentation r in s.representations) {
                foreach (SubRepresentation sr in r.subReps) {
                    if (sr.atomRepManager != null) {
                        sr.atomRepManager.SetDepthCueingDensity(UnityMolMain.fogDensity);
                    }
                    if (sr.bondRepManager != null) {
                        sr.bondRepManager.SetDepthCueingDensity(UnityMolMain.fogDensity);
                    }
                }
            }
        }
        UnityMolMain.recordPythonCommand("setDepthCueingDensity(" + UnityMolMain.fogDensity.ToString("F2", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("setDepthCueingDensity(" + prev.ToString("F2", culture) + ")");
    }

    public static void setDepthCueingColor(Color col) {

        RenderSettings.fogColor = col;

        UnityMolMain.recordPythonCommand("setDepthCueingColor(" + String.Format(CultureInfo.InvariantCulture, "{0})", col));
        UnityMolMain.recordUndoPythonCommand("setDepthCueingColor(" + String.Format(CultureInfo.InvariantCulture, "{0})", col));
    }


    // ---------------
    /// <summary>
    /// Print the content of the current directory
    /// </summary>
    public static List<string> ls() {
        // var info = new DirectoryInfo(path);
        List<string> ret = Directory.GetFiles(path).ToList();
        foreach (string f in ret) {
            Debug.Log(f);
        }
        return ret;
    }
    /// <summary>
    /// Change the current directory
    /// </summary>
    public static void cd(string newPath) {
        path = Path.GetFullPath(newPath);
        Debug.Log("Current path: '" + newPath + "'");
    }
    /// <summary>
    /// Print the current directory
    /// </summary>
    public static void pwd() {
        Debug.Log(path);
    }

    /// <summary>
    /// Return the lastly loaded UnityMolStructure
    /// </summary>
    public static UnityMolStructure last() {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return null;
        }
        return sm.loadedStructures.Last();
    }

    /// <summary>
    /// Change the background color of the camera based on a color name, also changes the fog color
    /// </summary>
    public static void bg_color(string colorS) {
        colorS = colorS.ToLower();
        Color col = strToColor(colorS);
        Color colprev = Camera.main.backgroundColor;
        Camera.main.backgroundColor = col;
        setDepthCueingColor(Camera.main.backgroundColor);
        UnityMolMain.recordPythonCommand("bg_color(" + String.Format(CultureInfo.InvariantCulture, "{0})", col));
        UnityMolMain.recordUndoPythonCommand("bg_color(" + String.Format(CultureInfo.InvariantCulture, "{0})", colprev));
    }

    /// <summary>
    /// Change the background color of the camera, also changes the fog color
    /// </summary>
    public static void bg_color(Color col) {
        Color colprev = Camera.main.backgroundColor;
        Camera.main.backgroundColor = col;
        setDepthCueingColor(Camera.main.backgroundColor);
        UnityMolMain.recordPythonCommand("bg_color(" + String.Format(CultureInfo.InvariantCulture, "{0})", col));
        UnityMolMain.recordUndoPythonCommand("bg_color(" + String.Format(CultureInfo.InvariantCulture, "{0})", colprev));
    }

    /// <summary>
    /// Convert a color string to a standard Unity Color
    /// Values can be "black", "white", "yellow" ,"green", "red", "blue", "pink", "gray"
    /// </summary>
    static Color strToColor(string input) {
        Color res = Color.black;
        switch (input) {
        case "black":
            return Color.black;
        case "white":
            return Color.white;
        case "yellow":
            return Color.yellow;
        case "green":
            return Color.green;
        case "red":
            return Color.red;
        case "blue":
            return Color.blue;
        case "pink":
            return new Color(1.0f, 0.75f, 0.75f);
        case "gray":
            return Color.gray;
        default:
            Debug.LogWarning("Unrecognized color");
            return Color.gray;
        }
    }


    /// <summary>
    /// Switch on or off the rotation around the X axis of all loaded molecules
    /// </summary>
    public static void switchRotateAxisX() {

        ManipulationManager mm = getManipulationManager();

        if (mm == null) {
            return;
        }

        if (mm.rotateX) {
            mm.rotateX = false;
        }
        else {
            mm.rotateX = true;
        }
        UnityMolMain.recordPythonCommand("switchRotateAxisX()");
        UnityMolMain.recordUndoPythonCommand("switchRotateAxisX()");
    }
    /// <summary>
    /// Switch on or off the rotation around the Y axis of all loaded molecules
    /// </summary>
    public static void switchRotateAxisY() {

        ManipulationManager mm = getManipulationManager();

        if (mm == null) {
            return;
        }

        if (mm.rotateY) {
            mm.rotateY = false;
        }
        else {
            mm.rotateY = true;
        }
        UnityMolMain.recordPythonCommand("switchRotateAxisY()");
        UnityMolMain.recordUndoPythonCommand("switchRotateAxisY()");
    }
    /// <summary>
    /// Switch on or off the rotation around the Z axis of all loaded molecules
    /// </summary>
    public static void switchRotateAxisZ() {

        ManipulationManager mm = getManipulationManager();

        if (mm == null) {
            return;
        }

        if (mm.rotateZ) {
            mm.rotateZ = false;
        }
        else {
            mm.rotateZ = true;
        }
        UnityMolMain.recordPythonCommand("switchRotateAxisZ()");
        UnityMolMain.recordUndoPythonCommand("switchRotateAxisZ()");
    }
    /// <summary>
    /// Change the rotation speed around the X axis
    /// </summary>
    public static void changeRotationSpeedX(float val) {

        ManipulationManager mm = getManipulationManager();

        if (mm == null) {
            return;
        }
        mm.speedX = val;
    }
    /// <summary>
    /// Change the rotation speed around the Y axis
    /// </summary>
    public static void changeRotationSpeedY(float val) {

        ManipulationManager mm = getManipulationManager();

        if (mm == null) {
            return;
        }
        mm.speedY = val;
    }

    /// <summary>
    /// Change the rotation speed around the Z axis
    /// </summary>
    public static void changeRotationSpeedZ(float val) {

        ManipulationManager mm = getManipulationManager();

        if (mm == null) {
            return;
        }
        mm.speedZ = val;
    }

    /// <summary>
    /// Change the mouse scroll speed
    /// </summary>
    public static void setMouseScrollSpeed(float val) {

        if (val > 0.0f) {
            ManipulationManager mm = getManipulationManager();

            if (mm == null) {
                return;
            }
            mm.scrollSpeed = val;
        }
        else {
            Debug.LogError("Wrong speed value");
        }
    }

    /// <summary>
    /// Change the speed of mouse rotations and translations
    /// </summary>
    public static void setMouseMoveSpeed(float val) {


        if (val > 0.0f) {
            ManipulationManager mm = getManipulationManager();

            if (mm == null) {
                return;
            }
            mm.moveSpeed = val;
        }
        else {
            Debug.LogError("Wrong speed value");
        }
    }


    /// <summary>
    /// Stop rotation around all axis
    /// </summary>
    public static void stopRotations() {

        ManipulationManager mm = getManipulationManager();
        if (mm == null) {
            return;
        }

        mm.rotateX = false;
        mm.rotateY = false;
        mm.rotateZ = false;

        UnityMolMain.recordPythonCommand("stopRotations()");
        UnityMolMain.recordUndoPythonCommand("stopRotations()");
    }


    /// <summary>
    /// Transform a string of representation type to a RepType object
    /// </summary>
    public static RepType getRepType(string type) {

        type = type.ToLower();

        AtomType atype = AtomType.noatom;
        BondType btype = BondType.nobond;

        switch (type) {
        case "c":
        case "cartoon":
            atype = AtomType.cartoon;
            btype = BondType.nobond;
            break;
        case "s":
        case "surf":
        case "surface":
            atype = AtomType.surface;
            btype = BondType.nobond;
            break;
        case "dxiso":
            atype = AtomType.DXSurface;
            btype = BondType.nobond;
            break;
        case "hb":
            atype = AtomType.optihb;
            btype = BondType.optihs;
            break;
        case "bondorder":
            atype = AtomType.bondorder;
            btype = BondType.bondorder;
            break;
        case "sphere":
            atype = AtomType.sphere;
            btype = BondType.nobond;
            break;

        case "l":
        case "line":
            atype = AtomType.noatom;
            btype = BondType.line;
            break;
        case "hbond":
        case "hbonds":
            atype = AtomType.noatom;
            btype = BondType.hbond;
            break;
        case "hbondtube":
            atype = AtomType.noatom;
            btype = BondType.hbondtube;
            break;
        case "fl":
        case "fieldlines":
            atype = AtomType.fieldlines;
            btype = BondType.nobond;
            break;
        case "tube":
        case "trace":
            atype = AtomType.trace;
            btype = BondType.nobond;
            break;
        case "sugar":
        case "sugarribbons":
            atype = AtomType.sugarribbons;
            btype = BondType.nobond;
            break;
        case "ellipsoid":
            atype = AtomType.ellipsoid;
            btype = BondType.nobond;
            break;
        case "point":
        case "p":
            atype = AtomType.point;
            btype = BondType.nobond;
            break;
        default:
            Debug.LogWarning("Unrecognized representation type '" + type + "'");
            break;
        }
        RepType result;
        result.atomType = atype;
        result.bondType = btype;
        return result;
    }

    /// <summary>
    /// Transform a representation type into a string
    /// </summary>
    public static string getTypeFromRepType(RepType rept) {
        if (rept.atomType == AtomType.cartoon && rept.bondType == BondType.nobond)
            return "c";
        if (rept.atomType == AtomType.surface && rept.bondType == BondType.nobond)
            return "s";
        if (rept.atomType == AtomType.DXSurface && rept.bondType == BondType.nobond)
            return "dxiso";
        if (rept.atomType == AtomType.optihb && rept.bondType == BondType.optihs)
            return "hb";
        if (rept.atomType == AtomType.bondorder && rept.bondType == BondType.bondorder)
            return "bondorder";
        if (rept.atomType == AtomType.sphere && rept.bondType == BondType.nobond)
            return "sphere";
        if (rept.atomType == AtomType.noatom && rept.bondType == BondType.line)
            return "l";
        if (rept.atomType == AtomType.noatom && rept.bondType == BondType.hbond)
            return "hbond";
        if (rept.atomType == AtomType.noatom && rept.bondType == BondType.hbondtube)
            return "hbondtube";
        if (rept.atomType == AtomType.fieldlines && rept.bondType == BondType.nobond)
            return "fl";
        if (rept.atomType == AtomType.trace && rept.bondType == BondType.nobond)
            return "trace";
        if (rept.atomType == AtomType.sugarribbons && rept.bondType == BondType.nobond)
            return "sugarribbons";
        if (rept.atomType == AtomType.ellipsoid && rept.bondType == BondType.nobond)
            return "ellipsoid";
        if (rept.atomType == AtomType.point && rept.bondType == BondType.nobond)
            return "point";
        Debug.LogWarning("Not a predefined type");
        return "";
    }
    private static string cBoolToPy(bool val) {
        if (val) {
            return "True";
        }
        return "False";
    }

    //--------------- Annotations

    /// Measure modes : 0 = distance, 1 = angle, 2 = torsion angle
    public static void setMeasureMode(int newMode) {
        if (newMode < 0 || newMode > 2) {
            Debug.LogError("Measure mode should be between 0 and 2");
            return;
        }
        int prevVal = (int) UnityMolMain.measureMode;
        UnityMolMain.measureMode = (MeasureMode) newMode;

        UnityMolMain.recordPythonCommand("setMeasureMode(" + newMode + ")");
        UnityMolMain.recordUndoPythonCommand("setMeasureMode(" + prevVal + ")");
    }
    public static void annotateAtom(string structureName, int atomId) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (atomId >= 0 && atomId < s.currentModel.allAtoms.Count) {
            anM.Annotate(s.currentModel.allAtoms[atomId]);
        }
        else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("annotateAtom(\"" + structureName + "\", " + atomId + ")");
        UnityMolMain.recordUndoPythonCommand("removeAnnotationAtom(\"" + structureName + "\", " + atomId + ")");
    }

    public static void removeAnnotationAtom(string structureName, int atomId) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (atomId >= 0 && atomId < s.currentModel.allAtoms.Count) {
            SphereAnnotation sa = new SphereAnnotation();
            sa.atoms.Add(s.currentModel.allAtoms[atomId]);
            anM.RemoveAnnotation(sa);
        }
        else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("removeAnnotationAtom(\"" + structureName + "\", " + atomId + ")");
        UnityMolMain.recordUndoPythonCommand("annotateAtom(\"" + structureName + "\", " + atomId + ")");
    }

    public static void annotateAtomText(string structureName, int atomId, string text) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (atomId >= 0 && atomId < s.currentModel.allAtoms.Count) {
            anM.AnnotateText(s.currentModel.allAtoms[atomId], text);
        }
        else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("annotateAtomText(\"" + structureName + "\", " + atomId + ", \"" + text + "\")");
        UnityMolMain.recordUndoPythonCommand("removeAnnotationAtomText(\"" + structureName + "\", " + atomId + ", \"" + text + "\")");
    }

    public static void removeAnnotationAtomText(string structureName, int atomId, string text) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (atomId >= 0 && atomId < s.currentModel.allAtoms.Count) {
            TextAnnotation sa = new TextAnnotation();
            sa.atoms.Add(s.currentModel.allAtoms[atomId]);
            sa.content = text;
            anM.RemoveAnnotation(sa);
        }
        else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("removeAnnotationAtomText(\"" + structureName + "\", " + atomId + ", \"" + text + "\")");
        UnityMolMain.recordUndoPythonCommand("annotateAtomText(\"" + structureName + "\", " + atomId + ", \"" + text + "\")");
    }

    public static void annotateLine(string structureName, int atomId, string structureName2, int atomId2) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolStructure s2 = sm.GetStructure(structureName2);

        if (atomId >= 0 && atomId < s.currentModel.allAtoms.Count &&
                atomId2 >= 0 && atomId2 < s2.currentModel.allAtoms.Count) {
            anM.AnnotateLine(s.currentModel.allAtoms[atomId], s2.currentModel.allAtoms[atomId2]);
        }
        else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("annotateLine(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
        UnityMolMain.recordUndoPythonCommand("removeAnnotationLine(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
    }

    public static void removeAnnotationLine(string structureName, int atomId, string structureName2, int atomId2) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolStructure s2 = sm.GetStructure(structureName2);

        if (atomId >= 0 && atomId < s.currentModel.allAtoms.Count &&
                atomId2 >= 0 && atomId2 < s2.currentModel.allAtoms.Count) {
            LineAtomAnnotation la = new LineAtomAnnotation();
            la.atoms.Add(s.currentModel.allAtoms[atomId]);
            la.atoms.Add(s2.currentModel.allAtoms[atomId2]);
            anM.RemoveAnnotation(la);
        }
        else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("removeAnnotationLine(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
        UnityMolMain.recordUndoPythonCommand("annotateLine(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
    }


    public static void annotateDistance(string structureName, int atomId, string structureName2, int atomId2) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolStructure s2 = sm.GetStructure(structureName2);

        if (atomId >= 0 && atomId < s.currentModel.allAtoms.Count &&
                atomId2 >= 0 && atomId2 < s2.currentModel.allAtoms.Count) {
            anM.AnnotateDistance(s.currentModel.allAtoms[atomId], s2.currentModel.allAtoms[atomId2]);
        }
        else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("annotateDistance(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
        UnityMolMain.recordUndoPythonCommand("removeAnnotationDistance(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
    }

    public static void removeAnnotationDistance(string structureName, int atomId, string structureName2, int atomId2) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolStructure s2 = sm.GetStructure(structureName2);

        if (atomId >= 0 && atomId < s.currentModel.allAtoms.Count &&
                atomId2 >= 0 && atomId2 < s2.currentModel.allAtoms.Count) {
            DistanceAnnotation da = new DistanceAnnotation();
            da.atoms.Add(s.currentModel.allAtoms[atomId]);
            da.atoms.Add(s2.currentModel.allAtoms[atomId2]);
            anM.RemoveAnnotation(da);
        }
        else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("removeAnnotationDistance(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
        UnityMolMain.recordUndoPythonCommand("annotateDistance(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
    }

    public static void annotateAngle(string structureName, int atomId, string structureName2, int atomId2, string structureName3, int atomId3) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolStructure s2 = sm.GetStructure(structureName2);
        UnityMolStructure s3 = sm.GetStructure(structureName3);

        if (atomId >= 0 && atomId < s.currentModel.allAtoms.Count &&
                atomId2 >= 0 && atomId2 < s2.currentModel.allAtoms.Count &&
                atomId3 >= 0 && atomId3 < s3.currentModel.allAtoms.Count) {
            anM.AnnotateAngle(s.currentModel.allAtoms[atomId], s2.currentModel.allAtoms[atomId2],
                              s3.currentModel.allAtoms[atomId3]);
        }
        else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("annotateAngle(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", "
                                         + atomId2 + ", \"" + structureName3 + "\", " + atomId3 + ")");
        UnityMolMain.recordUndoPythonCommand("removeAnnotationAngle(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", "
                                             + atomId2 + ", \"" + structureName3 + "\", " + atomId3 + ")");
    }

    public static void removeAnnotationAngle(string structureName, int atomId, string structureName2, int atomId2, string structureName3, int atomId3) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolStructure s2 = sm.GetStructure(structureName2);
        UnityMolStructure s3 = sm.GetStructure(structureName3);

        if (atomId >= 0 && atomId < s.currentModel.allAtoms.Count &&
                atomId2 >= 0 && atomId2 < s2.currentModel.allAtoms.Count &&
                atomId3 >= 0 && atomId3 < s3.currentModel.allAtoms.Count) {

            AngleAnnotation aa = new AngleAnnotation();
            aa.atoms.Add(s.currentModel.allAtoms[atomId]);
            aa.atoms.Add(s2.currentModel.allAtoms[atomId2]);
            aa.atoms.Add(s3.currentModel.allAtoms[atomId3]);
            anM.RemoveAnnotation(aa);
        }
        else {
            Debug.LogError("Wrong atom id");
            return;
        }

        UnityMolMain.recordPythonCommand("removeAnnotationAngle(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", "
                                         + atomId2 + ", \"" + structureName3 + "\", " + atomId3 + ")");
        UnityMolMain.recordUndoPythonCommand("annotateAngle(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", "
                                             + atomId2 + ", \"" + structureName3 + "\", " + atomId3 + ")");
    }


    public static void annotateDihedralAngle(string structureName, int atomId, string structureName2, int atomId2, string structureName3, int atomId3, string structureName4, int atomId4) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolStructure s2 = sm.GetStructure(structureName2);
        UnityMolStructure s3 = sm.GetStructure(structureName3);
        UnityMolStructure s4 = sm.GetStructure(structureName4);

        if (atomId >= 0 && atomId < s.currentModel.allAtoms.Count &&
                atomId2 >= 0 && atomId2 < s2.currentModel.allAtoms.Count &&
                atomId3 >= 0 && atomId3 < s3.currentModel.allAtoms.Count &&
                atomId4 >= 0 && atomId4 < s4.currentModel.allAtoms.Count) {

            anM.AnnotateDihedralAngle(s.currentModel.allAtoms[atomId], s2.currentModel.allAtoms[atomId2],
                                      s3.currentModel.allAtoms[atomId3], s4.currentModel.allAtoms[atomId4]);
        }
        else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("annotateDihedralAngle(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", "
                                         + atomId2 + ", \"" + structureName3 + "\", " + atomId3 +  ", \"" + structureName4 + "\", " + atomId4 + ")");
        UnityMolMain.recordUndoPythonCommand("removeAnnotationDihedralAngle(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", "
                                             + atomId2 + ", \"" + structureName3 + "\", " + atomId3 +  ", \"" + structureName4 + "\", " + atomId4 + ")");
    }

    public static void removeAnnotationDihedralAngle(string structureName, int atomId, string structureName2, int atomId2, string structureName3, int atomId3, string structureName4, int atomId4) {


        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolStructure s2 = sm.GetStructure(structureName2);
        UnityMolStructure s3 = sm.GetStructure(structureName3);
        UnityMolStructure s4 = sm.GetStructure(structureName4);

        if (atomId >= 0 && atomId < s.currentModel.allAtoms.Count &&
                atomId2 >= 0 && atomId2 < s2.currentModel.allAtoms.Count &&
                atomId3 >= 0 && atomId3 < s3.currentModel.allAtoms.Count &&
                atomId4 >= 0 && atomId4 < s4.currentModel.allAtoms.Count) {

            TorsionAngleAnnotation ta = new TorsionAngleAnnotation();
            ta.atoms.Add(s.currentModel.allAtoms[atomId]);
            ta.atoms.Add(s2.currentModel.allAtoms[atomId2]);
            ta.atoms.Add(s3.currentModel.allAtoms[atomId3]);
            ta.atoms.Add(s4.currentModel.allAtoms[atomId4]);
            anM.RemoveAnnotation(ta);
        }
        else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("removeAnnotationDihedralAngle(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", "
                                         + atomId2 + ", \"" + structureName3 + "\", " + atomId3 +  ", \"" + structureName4 + "\", " + atomId4 + ")");
        UnityMolMain.recordUndoPythonCommand("annotateDihedralAngle(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", "
                                             + atomId2 + ", \"" + structureName3 + "\", " + atomId3 +  ", \"" + structureName4 + "\", " + atomId4 + ")");
    }


    public static void annotateRotatingArrow(string structureName, int atomId, string structureName2, int atomId2) {


        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolStructure s2 = sm.GetStructure(structureName2);

        if (atomId >= 0 && atomId < s.currentModel.allAtoms.Count &&
                atomId2 >= 0 && atomId2 < s2.currentModel.allAtoms.Count) {

            anM.AnnotateDihedralArrow(s.currentModel.allAtoms[atomId], s2.currentModel.allAtoms[atomId2]);
        }
        else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("annotateRotatingArrow(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
        UnityMolMain.recordUndoPythonCommand("removeAnnotationRotatingArrow(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
    }

    public static void removeAnnotationRotatingArrow(string structureName, int atomId, string structureName2, int atomId2) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolStructure s2 = sm.GetStructure(structureName2);

        if (atomId >= 0 && atomId < s.currentModel.allAtoms.Count &&
                atomId2 >= 0 && atomId2 < s2.currentModel.allAtoms.Count) {
            ArrowAnnotation aa = new ArrowAnnotation();
            aa.atoms.Add(s.currentModel.allAtoms[atomId]);
            aa.atoms.Add(s2.currentModel.allAtoms[atomId2]);
            anM.RemoveAnnotation(aa);
        }
        else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("removeAnnotationRotatingArrow(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
        UnityMolMain.recordUndoPythonCommand("annotateRotatingArrow(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
    }



    public static void annotateArcLine(string structureName, int atomId, string structureName2, int atomId2, string structureName3, int atomId3) {


        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolStructure s2 = sm.GetStructure(structureName2);
        UnityMolStructure s3 = sm.GetStructure(structureName3);

        if (atomId >= 0 && atomId < s.currentModel.allAtoms.Count &&
                atomId2 >= 0 && atomId2 < s2.currentModel.allAtoms.Count &&
                atomId3 >= 0 && atomId3 < s3.currentModel.allAtoms.Count) {

            anM.AnnotateCurvedLine(s.currentModel.allAtoms[atomId], s2.currentModel.allAtoms[atomId2],
                                   s3.currentModel.allAtoms[atomId3]);
        }
        else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("annotateArcLine(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", "
                                         + atomId2 + ", \"" + structureName3 + "\", " + atomId3 + ")");
        UnityMolMain.recordUndoPythonCommand("removeAnnotationArcLine(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", "
                                             + atomId2 + ", \"" + structureName3 + "\", " + atomId3 + ")");
    }

    public static void removeAnnotationArcLine(string structureName, int atomId, string structureName2, int atomId2, string structureName3, int atomId3) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolStructure s2 = sm.GetStructure(structureName2);
        UnityMolStructure s3 = sm.GetStructure(structureName3);

        if (atomId >= 0 && atomId < s.currentModel.allAtoms.Count &&
                atomId2 >= 0 && atomId2 < s2.currentModel.allAtoms.Count &&
                atomId3 >= 0 && atomId3 < s3.currentModel.allAtoms.Count) {

            ArcLineAnnotation aa = new ArcLineAnnotation();
            aa.atoms.Add(s.currentModel.allAtoms[atomId]);
            aa.atoms.Add(s2.currentModel.allAtoms[atomId2]);
            aa.atoms.Add(s3.currentModel.allAtoms[atomId3]);
            anM.RemoveAnnotation(aa);
        }
        else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("removeAnnotationArcLine(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", "
                                         + atomId2 + ", \"" + structureName3 + "\", " + atomId3 + ")");
        UnityMolMain.recordUndoPythonCommand("annotateArcLine(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", "
                                             + atomId2 + ", \"" + structureName3 + "\", " + atomId3 + ")");
    }

    public static void annotateDrawLine(string structureName, List<Vector3> line, Color col) {




        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);

        int id = -1;
        if ( s != null) {
            id = anM.AnnotateDrawing(s, line, col);
        }
        else {
            Debug.LogError("Wrong structure name");
            return;
        }

        string command = "annotateDrawLine(\"" + structureName + "\",  List[Vector3]([";
        for (int i = 0; i < line.Count; i++) {
            command += "Vector3(" + line[i].x.ToString("F3", culture) + ", " +
                       line[i].y.ToString("F3", culture) + ", " +
                       line[i].z.ToString("F3", culture) + ")";
            if (i != line.Count - 1) {
                command += ", ";
            }
        }
        command += String.Format(CultureInfo.InvariantCulture, "]), {0}, )", col);
        UnityMolMain.recordPythonCommand(command);
        UnityMolMain.recordUndoPythonCommand("removeLastDrawLine(\"" + structureName + "\", " + id + ")");
    }

    public static void removeLastDrawLine(string structureName, int id) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }
        UnityMolStructure s = sm.GetStructure(structureName);

        if ( s != null && id != -1) {
            DrawAnnotation da = new DrawAnnotation();
            da.atoms.Add(s.currentModel.allAtoms[0]);
            da.id = id;
            anM.RemoveAnnotation(da);
        }
        else {
            Debug.LogError("Wrong structure name");
            return;
        }


        UnityMolMain.recordPythonCommand("removeLastDrawLine(\"" + structureName + "\", " + id + ")");
        UnityMolMain.recordUndoPythonCommand("");

    }

    public static void clearDrawings() {
        UnityMolAnnotationManager am = UnityMolMain.getAnnotationManager();
        am.CleanDrawings();
        UnityMolMain.recordPythonCommand("clearDrawings()");
        UnityMolMain.recordUndoPythonCommand("");
    }

    public static void clearAnnotations() {
        UnityMolMain.getAnnotationManager().Clean();
        UnityMolMain.recordPythonCommand("clearAnnotations()");
        UnityMolMain.recordUndoPythonCommand("");
    }
}
}
}
