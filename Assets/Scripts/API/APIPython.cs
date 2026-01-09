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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;


namespace UMol {
namespace API {


/// <summary>
/// Defines all the functions available from the console.
/// <c>APIPython</c> derives from <c>MonoBehaviour</c> to access the coroutines for a few methods.
/// The rest of the methods are static because no instance is needed.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeInvocation")]
public class APIPython : MonoBehaviour {

    /// <summary>
    /// Path of data folder
    /// </summary>
    private static string path;

    /// <summary>
    /// Uniq instance of the class (Singleton).
    /// </summary>
    private static APIPython instance;

    /// <summary>
    /// Limit the size of selection string query, switch to atomid ranges when over this limit
    /// </summary>
    private const int limitSizeSelectionString = 500;

    /// <summary>
    /// Reference to the python console
    /// </summary>
    public static PythonConsole2 pythonConsole;

    /// <summary>
    /// Component for external TCP commands.
    /// </summary>
    private static TCPServerCommand extCom;

    /// <summary>
    /// Output correctly formated floats
    /// </summary>
    private static readonly CultureInfo culture = CultureInfo.InvariantCulture;

    private void Awake() {

        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        instance = this;

        path = Application.dataPath;
        PythonConsole2[] objs = FindObjectsOfType<PythonConsole2>();
        if (objs.Length == 0) {
            Debug.LogWarning("Couldn't find the python console object");
        } else {
            pythonConsole = objs[0];
        }
    }


    /// <summary>
    /// Fetch a PDB Id from RCSB server (pdb or mmcif zipped)
    /// </summary>
    /// <param name="PDBId">PDB id</param>
    /// <param name="usemmCIF">Use mmcif type?</param>
    /// <param name="readHetm">Read hetero atoms?</param>
    /// <param name="forceDSSP">Compute secondary structure through DSSP?</param>
    /// <param name="showDefaultRep">Show the default representation?</param>
    /// <param name="center">center the molecule?</param>
    /// <param name="modelsAsTraj">If several models are present in the file, treat them as a trajectory?</param>
    /// <param name="forceStructureType">Type of molecular file (-1 = auto-detect / 0 = standard / 1 = CG / 2 = OPEP / 3 = HIRERNA)</param>
    /// <param name="bioAssembly">Show the macromolecular assembly?</param>
    /// <returns>the molecule as a UnityMolStructure</returns>
    public static UnityMolStructure fetch(string PDBId, bool usemmCIF = true, bool readHetm = true, bool forceDSSP = false,
        bool showDefaultRep = true, bool center = true, bool modelsAsTraj = true, int forceStructureType = -1, bool bioAssembly = false) {

        UnityMolStructure newStruct;

        if (usemmCIF) {
            PDBxReader rx = new() {
                ModelsAsTraj = modelsAsTraj
            };

            newStruct = rx.Fetch(PDBId, readHet : readHetm, forceType : forceStructureType, bioAssembly : bioAssembly);
        } else {
            if (bioAssembly) {
                Debug.LogWarning("Biological Assembly data are available only for mmCIF");
            }
            PDBReader r = new() {
                ModelsAsTraj = modelsAsTraj
            };

            newStruct = r.Fetch(PDBId, readHet : readHetm, forceType : forceStructureType);
        }

        newStruct.readHET = readHetm;
        newStruct.modelsAsTraj = modelsAsTraj;
        newStruct.fetchedmmCIF = usemmCIF;
        newStruct.pdbID = PDBId;
        newStruct.bioAssembly = bioAssembly;

        UnityMolSelection sel = newStruct.ToSelection();

        if (forceDSSP || !newStruct.ssInfoFromFile) {
            DSSP.assignSS_DSSP(newStruct);
        } else {
            Debug.Log("Using secondary structure definition from the file");
        }

        if (showDefaultRep) {
            defaultRep(sel.name);
        }

        if (center) {
            centerOnStructure(newStruct.name, recordCommand : false);
        }

        UnityMolMain.recordPythonCommand("fetch(PDBId=\"" + PDBId + "\", usemmCIF=" + cBoolToPy(usemmCIF) + ", readHetm=" + cBoolToPy(readHetm) + ", forceDSSP=" +
                                         cBoolToPy(forceDSSP) + ", showDefaultRep=" + cBoolToPy(showDefaultRep) + ", center=" + cBoolToPy(center) +
                                         ", modelsAsTraj=" + cBoolToPy(modelsAsTraj) + ", forceStructureType=" + forceStructureType +
                                         ", bioAssembly=" + cBoolToPy(bioAssembly) + ")");

        UnityMolMain.recordUndoPythonCommand("delete(\"" + newStruct.name + "\")");

        return newStruct;
    }
    /// <summary>
    /// Fetch a remote molecular file (pdb/mmcif/gro/mol2/sdf/xyz formats) from a URL
    /// </summary>
    /// <param name="urlPath">URL of the file</param>
    /// <param name="readHetm">Read hetero atoms?</param>
    /// <param name="forceDSSP">Compute secondary structure through DSSP?</param>
    /// <param name="showDefaultRep">Show the default representation?</param>
    /// <param name="center">center the molecule?</param>
    /// <param name="modelsAsTraj">If several models are present in the file, treat them as a trajectory?</param>
    /// <param name="forceStructureType">Type of molecular file (-1 = auto-detect / 0 = standard / 1 = CG / 2 = OPEP / 3 = HIRERNA)</param>
    /// <returns>the molecule as a UnityMolStructure</returns>
    public static UnityMolStructure fetch_URL(string urlPath, bool readHetm = true, bool forceDSSP = false,
        bool showDefaultRep = true, bool center = true, bool modelsAsTraj = true, int forceStructureType = -1) {

        Uri url = new(urlPath);
        string filename = Path.GetFileName(url.AbsolutePath);

        UnityMolStructure newStruct;

        Reader r = Reader.GuessReaderFrom(filename);
        if (r != null) {
            r.ModelsAsTraj = modelsAsTraj;
            newStruct = r.Fetch_URL(urlPath,readHet: readHetm, forceType: forceStructureType);

            if (newStruct != null) {
                Debug.Log("Fetched " + filename + " with " + newStruct.models.Count + " models");
                UnityMolSelection sel = newStruct.ToSelection();

                if (forceDSSP || !newStruct.ssInfoFromFile) {
                    DSSP.assignSS_DSSP(newStruct);
                } else {
                    Debug.Log("Using secondary structure definition from the file");
                }

                if (showDefaultRep) {
                    defaultRep(sel.name);
                }

                if (center) {
                    centerOnStructure(newStruct.name, recordCommand : false);
                }
            } else {
                return null;
            }
        } else {
            Debug.LogError("Could not fetch remote file: " + urlPath);
            return null;
        }
        UnityMolMain.recordPythonCommand("Fetch_URL(URLPath=\"" + urlPath + "\", readHetm=" + cBoolToPy(readHetm) + ", forceDSSP=" +
                                         cBoolToPy(forceDSSP) + ", showDefaultRep=" + cBoolToPy(showDefaultRep) + ", center=" + cBoolToPy(center) +
                                         ", modelsAsTraj=" + cBoolToPy(modelsAsTraj) + ", forceStructureType=" + forceStructureType + ")");
        UnityMolMain.recordUndoPythonCommand("delete(\"" + newStruct.name + "\")");
        return newStruct;
    }

    /// <summary>
    /// Load a local molecular file (pdb/mmcif/gro/mol2/sdf/xyz formats)
    /// </summary>
    /// <param name="filePath">Path of the local molecular file</param>
    /// <param name="readHetm">Read hetero atoms?</param>
    /// <param name="forceDSSP">Compute secondary structure through DSSP?</param>
    /// <param name="showDefaultRep">Show the default representation?</param>
    /// <param name="center">center the molecule?</param>
    /// <param name="modelsAsTraj">If several models are present in the file, treat them as a trajectory?</param>
    /// <param name="forceStructureType">Type of molecular file (-1 = auto-detect / 0 = standard / 1 = CG / 2 = OPEP / 3 = HIRERNA)</param>
    /// <returns>the molecule as a UnityMolStructure</returns>
    public static UnityMolStructure load(string filePath, bool readHetm = true, bool forceDSSP = false,
        bool showDefaultRep = true, bool center = true, bool modelsAsTraj = true, int forceStructureType = -1) {

        string realPath = filePath;
        string customPath = Path.Combine(path, filePath);
        if (File.Exists(customPath)) {
            realPath = customPath;
        }

        Reader r = Reader.GuessReaderFrom(realPath);
        if (r == null) {
            return null;
        }

        r.ModelsAsTraj = modelsAsTraj;
        UnityMolStructure newStruct = r.Read(readHet: readHetm, forceType: forceStructureType);

        if (newStruct != null) {
            string fileName = Path.GetFileName(realPath);
            Debug.Log("Loaded " + fileName + " with " + newStruct.models.Count + " models");
            UnityMolSelection sel = newStruct.ToSelection();

            if (forceDSSP || !newStruct.ssInfoFromFile) {
                DSSP.assignSS_DSSP(newStruct);
            } else {
                Debug.Log("Using secondary structure definition from the file");
            }

            if (showDefaultRep) {
                defaultRep(sel.name);
            }

            if (center) {
                centerOnStructure(newStruct.name, recordCommand: false);
            }
        } else {
            Debug.LogError("Could not load file " + realPath);
            return null;
        }

        UnityMolMain.recordPythonCommand("load(filePath=\"" + realPath.Replace("\\", "/") + "\", readHetm=" + cBoolToPy(readHetm) + ", forceDSSP=" +
                                         cBoolToPy(forceDSSP) + ", showDefaultRep=" + cBoolToPy(showDefaultRep) + ", center=" + cBoolToPy(center) +
                                         ", modelsAsTraj=" + cBoolToPy(modelsAsTraj) + ", forceStructureType=" + forceStructureType + ")");
        UnityMolMain.recordUndoPythonCommand("delete(\"" + newStruct.name + "\")");

        return newStruct;
    }

    /// <summary>
    /// Show/Hide UnityMol console
    /// </summary>
    /// <param name="show"> If True, show the console </param>
    public static void showHideConsole(bool show) {
        PythonConsole2[] objs = FindObjectsOfType<PythonConsole2>();
        foreach (PythonConsole2 pc in objs) {
            UnityEngine.UI.Button showB = pc.showConsoleButton;
            UnityEngine.UI.Button hideB = pc.hideConsoleButton;
            if (show) {
                if (showB != null) {
                    showB.onClick.Invoke();
                }
            } else {
                if (hideB != null) {
                    hideB.onClick.Invoke();
                }
            }
        }
    }

    /// <summary>
    /// Can one execute python command through the Python console?
    /// </summary>
    /// <returns>True if one can execute a command. False otherwise</returns>
    private static bool canRunCommand() {
        return !UnityMolMain.multiUserMode || UnityMolMain.multiUserPresenter;
    }

    /// <summary>
    /// Allow to call python API commands and record them in the history from C#
    /// </summary>
    /// <param name="command">command to execute</param>
    /// <param name="force">Force the execution of the command</param>
    /// <returns>True if success. False otherwise</returns>
    public static bool ExecuteCommand(string command, bool force = false) {
        if (!force && !canRunCommand()) {
            Debug.LogWarning("You are not the current presenter");
            return false;
        }
        bool success = false;
        pythonConsole.ExecuteCommand(command, ref success);
        return success;
    }

    /// <summary>
    /// Allow to call python API commands and record them in the history from C#
    /// </summary>
    /// <param name="command">command to execute</param>
    /// <param name="success">Will be updated to True if success. False otherwise</param>
    /// <param name="force">Force the execution of the command</param>
    /// <returns>an object containing the return value of the command.</returns>
    public static object ExecuteCommandWithFeedback(string command, ref bool success, bool force = false) {
        if (!force && !canRunCommand()) {
            Debug.LogWarning("You are not the current presenter");
            return false;
        }
        object res= null;
        res = pythonConsole.ExecuteCommand(command, ref success);
        return res;
    }

    /// <summary>
    /// Load the UnityMolCommons python module defined in the file "UnityMolCommons.py" in the StreamingAssets folder.
    /// This module contains some useful functions to interact with UnityMol.
    /// </summary>
    public static void loadPythonCommonsModule() {
        bool res = false;
        Debug.Log("Load common pythons functions from UnityMolCommons.py");
        pythonConsole.ExecuteCommand("import sys", ref res);
        pythonConsole.ExecuteCommand("sys.path.append(Application.streamingAssetsPath)", ref res);
        pythonConsole.ExecuteCommand("from UnityMolCommons import *", ref res);
    }

    /// <summary>
    /// Load a local molecular file (pdb/mmcif/gro/mol2/sdf/xyz formats) from a string
    /// </summary>
    /// <param name="molecularName">Name of the molecule</param>
    /// <param name="molecularData">Molecular data</param>
    /// <param name="readHetm">Read hetero atoms?</param>
    /// <param name="forceDSSP">Compute secondary structure through DSSP?</param>
    /// <param name="showDefaultRep">Show the default representation?</param>
    /// <param name="center">center the molecule?</param>
    /// <param name="modelsAsTraj">If several models are present in the file, treat them as a trajectory?</param>
    /// <param name="forceStructureType">Type of molecular file (-1 = auto-detect / 0 = standard / 1 = CG / 2 = OPEP / 3 = HIRERNA)</param>
    /// <returns>the molecule as a UnityMolStructure</returns>
    public static UnityMolStructure loadFromString(string molecularName, string molecularData, bool readHetm = true, bool forceDSSP = false,
        bool showDefaultRep = true, bool center = true, bool modelsAsTraj = true, int forceStructureType = -1) {

        UnityMolStructure newStruct = null;

        string tempPath = Path.GetTempFileName();
        try {

            using(StreamWriter sw = new(tempPath, false)) {

                sw.WriteLine(molecularData);
                sw.Close();
            }

            Reader r = Reader.GuessReaderFrom(tempPath);

            if (r != null) {
                r.ModelsAsTraj = modelsAsTraj;

                newStruct = r.Read(readHet: readHetm, forceType: forceStructureType);

                if (newStruct != null) {
                    Debug.Log("Loaded PDB " + molecularName + " with " + newStruct.models.Count + " models");
                    UnityMolSelection sel = newStruct.ToSelection();

                    if (forceDSSP || !newStruct.ssInfoFromFile) {
                        DSSP.assignSS_DSSP(newStruct);
                    } else {
                        Debug.Log("Using secondary structure definition from the file");
                    }

                    if (showDefaultRep) {
                        defaultRep(sel.name);
                    }

                    if (center) {
                        centerOnStructure(newStruct.name, recordCommand : false);
                    }
                } else {
                    Debug.LogError("Could not load file content");
                    return null;
                }
                UnityMolMain.recordPythonCommand("loadFromString(fileName=\"" + tempPath + "\", fileContent= \"" +
                                                 molecularData + "\", readHetm=" + cBoolToPy(readHetm) + ", forceDSSP=" +
                                                 cBoolToPy(forceDSSP) + ", showDefaultRep=" + cBoolToPy(showDefaultRep) +
                                                 ", center=" + cBoolToPy(center) + ", modelsAsTraj=" + cBoolToPy(modelsAsTraj) +
                                                 ", forceStructureType=" + forceStructureType + ")");
                UnityMolMain.recordUndoPythonCommand("delete(\"" + newStruct.name + "\")");

            }
        } finally {
            if (File.Exists(tempPath)) {
                File.Delete(tempPath);
            }
        }
        return newStruct;
    }



    /// <summary>
    /// Load a Martini ITP file to parse elastic network and secondary structure for a structure
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    /// <param name="filePath">Path of the Martini ITP file</param>
    public static void loadMartiniITP(string structureName, string filePath) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        string realPath = filePath;
        string customPath = Path.Combine(path, filePath);
        if (File.Exists(customPath)) {
            realPath = customPath;
        }

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (s != null) {
            ITPReader.loadSystemITPFile(realPath, s);
        } else {
            Debug.LogError("Structure not found");
        }

        UnityMolMain.recordPythonCommand("loadMartiniITP(\"" + structureName + "\", \"" + realPath.Replace("\\", "/") + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Show bounding box lines around the structure
    /// <remarks>This box is based on the max/min coordinates of the atoms. It is not the CRYSTAL box or the simulation box</remarks>
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    public static void showBoundingBox(string structureName) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (s != null) {
            s.showBoundingBox();
        } else {
            Debug.LogError("Structure not found");
        }
        UnityMolMain.recordPythonCommand("showBoundingBox(\"" + structureName + "\")");
        UnityMolMain.recordUndoPythonCommand("hideBoundingBox(\"" + structureName + "\")");
    }

    /// <summary>
    /// Hide bounding box around the structure
    /// <remarks>This box is based on the max/min coordinates of the atoms. It is not the CRYSTAL box or the simulation box</remarks>
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    public static void hideBoundingBox(string structureName) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (s != null) {
            s.hideBoundingBox();
        } else {
            Debug.LogError("Structure not found");
        }

        UnityMolMain.recordPythonCommand("hideBoundingBox(\"" + structureName + "\")");
        UnityMolMain.recordUndoPythonCommand("showBoundingBox(\"" + structureName + "\")");
    }

    /// <summary>
    /// Set the size of the bounding box lines
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    /// <param name="size">size of the lines</param>
    public static void setBoundingBoxLineSize(string structureName, float size = 0.005f) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        float prevSize = 0.005f;
        if (s != null) {
            prevSize = s.getBoundingBoxLineSize();
            s.setBoundingBoxLineSize(size);
        } else {
            Debug.LogError("Structure not found");
        }

        UnityMolMain.recordPythonCommand("setBoundingBoxLineSize(\"" + structureName + "\"," + size + ")");
        UnityMolMain.recordUndoPythonCommand("setBoundingBoxLineSize(\"" + structureName + "\"," + prevSize + ")");
    }

    /// <summary>
    /// Load an XML file containing covalent and non-covalent bonds
    /// Possible bond types are: 'covalent' or 'db_geom', 'hbond' or 'h-bond' or 'hbond_weak',
    /// 'halogen', 'ionic', 'aromatic', 'hydrophobic', 'carbonyl'
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    /// <param name="filePath">path of the XML file</param>
    /// <param name="modelId">ID of the model in the structure. -1 means current model</param>
    public static void loadBondsXML(string structureName, string filePath, int modelId = -1) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        string realPath = filePath;
        string customPath = Path.Combine(path, filePath);
        if (File.Exists(customPath)) {
            realPath = customPath;
        }

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (s != null) {
            UnityMolModel m = s.currentModel;

            if (modelId >= 0 && modelId < s.models.Count) {
                m = s.models[modelId];
            } else if (modelId != -1) {
                Debug.LogError("Wrong model");
                return;
            }

            //Stores the bonds in the model.covBondOrders
            Dictionary<bondOrderType, List<AtomDuo>> res = BondOrderParser.parseBondOrderFile(m, realPath);


            int id = 0;
            foreach (bondOrderType bot in res.Keys) {

                int maxAtomPerBond = 0;
                Dictionary<UnityMolAtom, int> bondPerA = new(m.Count);
                //First pass to compute the max number of bonds per atom
                foreach (AtomDuo d in res[bot]) {
                    bondPerA.TryAdd(d.a1, 0);
                    bondPerA.TryAdd(d.a2, 0);

                    bondPerA[d.a1]++;
                    bondPerA[d.a2]++;
                    maxAtomPerBond = Mathf.Max(bondPerA[d.a1], Mathf.Max(bondPerA[d.a2], maxAtomPerBond));
                }

                UnityMolBonds curBonds = new();
                if (maxAtomPerBond > curBonds.NBBONDS) {
                    curBonds.NBBONDS = maxAtomPerBond;
                }

                foreach (AtomDuo d in res[bot]) {
                    curBonds.Add(d.a1, d.a2);
                }

                UnityMolSelection sel = new(m.allAtoms,
                        curBonds, s.name + "_" + bot.btype + id + "_ExternalBonds") {
                    canUpdateBonds = false
                };

                UnityMolMain.getSelectionManager().Add(sel);
                showSelection(sel.name, "hbondtube", true);
                id++;
            }
        } else {
            Debug.LogError("Structure not found");
        }
        UnityMolMain.recordPythonCommand("loadBondsXML(\"" + structureName + "\", \"" + realPath.Replace("\\", "/") + "\", " + modelId + ")");
        UnityMolMain.recordUndoPythonCommand("unloadCustomBonds(\"" + structureName + "\", " + modelId + ")");
    }

    /// <summary>
    /// Override the current bonds of the model modelId of the structure 'structureName'
    /// and saves the previous one in model.savedBonds
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    /// <param name="modelId">ID of the model in the structure. -1 means current model</param>
    public static void overrideBondsWithXML(string structureName, int modelId = -1) {

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
            } else if (modelId != -1) {
                Debug.LogError("Wrong model");
                return;
            }

            Dictionary<AtomDuo, bondOrderType> xmlBonds = m.covBondOrders;
            if (xmlBonds != null) {
                m.savedBonds = m.bonds;

                UnityMolBonds newBonds = new();
                foreach (AtomDuo d in xmlBonds.Keys) {
                    newBonds.Add(d.a1, d.a2);
                }
                m.bonds = newBonds;
                s.updateRepresentations(trajectory: false);
            } else {
                Debug.LogError("No bonds parsed from a XML file in this model");
            }

        }

        UnityMolMain.recordPythonCommand("overrideBondsWithXML(\"" + structureName + "\", " + modelId + ")");
        UnityMolMain.recordUndoPythonCommand("restoreBonds(\"" + structureName + "\", " + modelId + ")");
    }

    /// <summary>
    /// Load topology information from a PSF file for a structure.
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    /// <param name="psfPath">path of the PSF file</param>
    /// <param name="modelId">ID of the model in the structure. -1 means current model</param>
    public static void loadPSFTopology(string structureName, string psfPath, int modelId = -1) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        string realPath = psfPath;
        string customPath = Path.Combine(path, psfPath);
        if (File.Exists(customPath)) {
            realPath = customPath;
        }

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (s != null) {
            UnityMolModel m = s.currentModel;

            if (modelId >= 0 && modelId < s.models.Count) {
                m = s.models[modelId];
            } else if (modelId != -1) {
                Debug.LogError("Wrong model");
                return;
            }

            UnityMolBonds newBonds = PSFReader.readTopologyFromPSF(realPath, s);
            if (newBonds != null) {
                m.savedBonds = m.bonds;
                m.bonds = newBonds;
                Debug.Log("Read " + newBonds.Count + " bonds from PSF");
                s.updateRepresentations(trajectory: false);
            } else {
                Debug.LogError("No bonds were parsed from the PSF file");
                return;
            }
        }
        UnityMolMain.recordPythonCommand("loadPSFTopology(\"" + structureName + "\", \"" + realPath.Replace("\\", "/") + "\", " + modelId + ")");
        UnityMolMain.recordUndoPythonCommand("restoreBonds(\"" + structureName + "\", " + modelId + ")");
    }

    /// <summary>
    /// Load topology information from a TOP file for a structure.
    /// specialBondString
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    /// <param name="topPath">path of the PSF file</param>
    /// <param name="modelId">ID of the model in the structure. -1 means current model</param>
    /// <param name="specialBondString">When not empty is used to create a selection containing only these special bonds,
    /// shown as hbondtube</param>
    public static void loadTOPTopology(string structureName, string topPath, int modelId = -1,
        string specialBondString = "restrain") {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        string realPath = topPath;
        string customPath = Path.Combine(path, topPath);
        if (File.Exists(customPath)) {
            realPath = customPath;
        }

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (s != null) {
            UnityMolModel m = s.currentModel;

            if (modelId >= 0 && modelId < s.models.Count) {
                m = s.models[modelId];
            } else if (modelId != -1) {
                Debug.LogError("Wrong model");
                return;
            }

            UnityMolBonds newBonds = TOPReader.readTopologyFromTOP(realPath, s, specialBondString);
            if (newBonds != null) {
                m.savedBonds = m.bonds;
                m.bonds = newBonds;
                Debug.Log("Read " + newBonds.Count + " bonds from TOP");
                s.updateRepresentations(trajectory: false);
            } else {
                Debug.LogError("No bonds were parsed from the TOP file");
                return;
            }
        }
        UnityMolMain.recordPythonCommand("loadTOPTopology(\"" + structureName + "\", \"" + realPath.Replace("\\", "/") + "\", " + modelId + ", \"" + specialBondString + "\")");
        UnityMolMain.recordUndoPythonCommand("restoreBonds(\"" + structureName + "\", " + modelId + ")");
    }

    /// <summary>
    /// Restore bonds saved in the 'model.savedBonds'
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    /// <param name="modelId">ID of the model in the structure. -1 means current model</param>
    public static void restoreBonds(string structureName, int modelId = -1) {

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
            } else if (modelId != -1) {
                Debug.LogError("Wrong model");
                return;
            }

            Dictionary<AtomDuo, bondOrderType> xmlBonds = m.covBondOrders;
            if (xmlBonds != null && m.savedBonds != null) {
                m.bonds = m.savedBonds;
                s.updateRepresentations(trajectory: false);
            } else {
                Debug.LogError("No bonds parsed from a XML file in this model");
            }

        }

        UnityMolMain.recordPythonCommand("restoreBonds(\"" + structureName + "\", " + modelId + ")");
        UnityMolMain.recordUndoPythonCommand("overrideBondsWithXML(\"" + structureName + "\", " + modelId + ")");
    }

    /// <summary>
    /// Removes the covBondOrders bonds loaded by loadBondsXML from the model 'modelId' of the structure 'structureName'
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    /// <param name="modelId">ID of the model in the structure. -1 means current model</param>
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
        } else {
            Debug.LogError("Structure not found");
        }
        UnityMolMain.recordPythonCommand("unloadCustomBonds(\"" + structureName + "\", " + modelId + ")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Delete all the loaded molecules/structures
    /// </summary>
    public static void reset() {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        List<string> toDelete = new();

        foreach (UnityMolStructure s in sm.loadedStructures) {
            toDelete.Add(s.name);
        }
        foreach (string s in toDelete) {
            UnityMolStructure stru = sm.GetStructure(s);
            if (stru != null) {
                sm.Delete(stru);
            }
        }
        ManipulationManager mm = getManipulationManager();
        if (mm != null) {
            mm.resetPosition();
            mm.resetRotation();
        }
        stopVideo();
        UnityMolMain.raytracingMode = false;
        clearTour();
        UnityMolMain.getAnnotationManager().Clean();
    }

    /// <summary>
    /// Switch between secondary structure information parsed from the file
    /// and the ones from DSSP computation.
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    /// <param name="forceDSSP">Compute Secondary structure info from DSSP?</param>
    public static void switchSSAssignmentMethod(string structureName, bool forceDSSP = false) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (s == null) {
            return;
        }

        if (forceDSSP || s.parsedSSInfo == null) {
            DSSP.assignSS_DSSP(s);
            Debug.LogWarning("Setting secondary structure assignment to DSSP");
        } else if (s.ssInfoFromFile) {
            DSSP.assignSS_DSSP(s);
            Debug.LogWarning("Setting secondary structure assignment to DSSP");
        } else {
            Reader.FillSecondaryStructure(s, s.parsedSSInfo);
            Debug.LogWarning("Setting secondary structure assignment parsed from file");
        }

        UnityMolMain.getPrecompRepManager().Clear(s.name);

        UnityMolMain.recordPythonCommand("switchSSAssignmentMethod(\"" + structureName + "\"," + cBoolToPy(forceDSSP) + ")");
        UnityMolMain.recordUndoPythonCommand("switchSSAssignmentMethod(\"" + structureName + "\")");
    }

    /// <summary>
    /// Show/Hide hydrogens in representations of the provided selection
    /// This only works for lines, hyperballs and sphere representations
    /// </summary>
    /// <param name="selName">Name of the selection concerned.</param>
    public static void showHideHydrogensInSelection(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        bool firstRep = true;
        bool shouldShow = true;

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            foreach (List<UnityMolRepresentation> reps in sel.representations.Values) {
                foreach (UnityMolRepresentation r in reps) {
                    foreach (SubRepresentation sr in r.subReps) {
                        if (sr.atomRepManager != null) {
                            if (firstRep) {
                                shouldShow = !sr.atomRepManager.areHydrogensOn;
                                firstRep = false;
                            }
                            sr.atomRepManager.ShowHydrogens(shouldShow);
                        }
                        if (sr.bondRepManager != null) {
                            if (firstRep) {
                                shouldShow = !sr.bondRepManager.areHydrogensOn;
                                firstRep = false;
                            }
                            sr.bondRepManager.ShowHydrogens(shouldShow);
                        }
                    }
                }
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }
        UnityMolMain.recordPythonCommand("showHideHydrogensInSelection(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("showHideHydrogensInSelection(\"" + selName + "\")");
    }

    /// <summary>
    /// Show/Hide side chains in representations of the current selection
    /// This only works for lines, hyperballs and sphere representations
    /// </summary>
    /// <param name="selName">Name of the selection concerned.</param>
    public static void showHideSideChainsInSelection(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

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
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }
        UnityMolMain.recordPythonCommand("showHideSideChainsInSelection(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("showHideSideChainsInSelection(\"" + selName + "\")");
    }

    /// <summary>
    /// Show/Hide backbone in representations of the current selection
    /// This only works for lines, hyperballs and sphere representations
    /// </summary>
    /// <param name="selName">Name of the selection concerned.</param>
    public static void showHideBackboneInSelection(string selName) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

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
        } else {
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
    /// <param name="structureName">Name of the structure concerned</param>
    /// <param name="modelId">ID of the model in the structure. -1 means current model</param>
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

        int lenSameCom = 10 + structureName.Length + 2;
        bool replaced = UnityMolMain.recordPythonCommand("setModel(\"" + structureName + "\", " + modelId + ")", true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("setModel(\"" + structureName + "\", " + prev + ")", replaced);
    }


    /// <summary>
    /// Load a trajectory file (XTC or TRR) for a structure
    /// It creates a XDRFileReader in the corresponding UnityMolStructure and a TrajectoryPlayer
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    /// <param name="filePath">path of the trajectory file</param>
    public static void loadTraj(string structureName, string filePath) {

        string realPath = filePath;
        string customPath = Path.Combine(path, filePath);
        if (File.Exists(customPath)) {
            realPath = customPath;
        }

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);

        try {
            s.readTrajectoryXDR(realPath);
            s.createTrajectoryPlayer();
        } catch (Exception) {
            Debug.LogError("Error while loading trajectory file '" + realPath + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("loadTraj(\"" + structureName + "\", \"" + realPath.Replace("\\", "/") + "\")");
        UnityMolMain.recordUndoPythonCommand("unloadTraj(\"" + structureName + "\")");
    }

    /// <summary>
    /// Unload a trajectory for a specific structure
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
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
    /// Create a special selection containing frames from the trajectory
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    /// <param name="selectionQuery">Selection query</param>
    /// <param name="frameStart">starting frame in the trajectory</param>
    /// <param name="frameEnd">ending frame in the trajectory</param>
    /// <param name="step">Step between frames</param>
    /// <returns>The selection name</returns>
    public static string pickTrajectoryFrames(string structureName, string selectionQuery = "all", int frameStart = 0, int frameEnd = 1, int step = 1) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return null;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (s == null) {
            Debug.LogError("Structure not found");
            return null;
        }
        if (s.xdr == null) {
            Debug.LogError("No trajectory loaded for this structure");
            return null;
        }
        if (step <= 0) {
            Debug.LogError("Wrong step");
            return null;
        }

        if (frameEnd < frameStart) {
            Debug.LogError("Ending frame should be larger than starting frame");
            return null;
        }
        if (frameStart < 0 || frameStart >= s.xdr.NumberFrames) {
            Debug.LogError("Wrong starting frame");
            return null;
        }
        if (frameEnd < 0 || frameEnd >= s.xdr.NumberFrames) {
            Debug.LogError("Wrong ending frame");
            return null;
        }

        UnityMolSelection resSel;
        try {
            MDAnalysisSelection selec = new(selectionQuery, s.currentModel.allAtoms);
            resSel = selec.process();
        } catch (Exception e) {
            Debug.LogError("Wrong selection query\n" + e);
            return null;
        }
        if (resSel.Count == 0) {
            Debug.LogError("Empty selection");
            return null;
        }

        List<Vector3[]> extractedPos = new();
        List<int> idFrames = new();
        int N = resSel.Count;

        for (int idFrame = frameStart; idFrame <= frameEnd; idFrame += step) {
            Vector3[] f = s.xdr.GetFrame(idFrame);
            Vector3[] pos = new Vector3[N];
            idFrames.Add(idFrame);

            for (int i = 0; i < N; i++) {
                pos[i] = f[resSel.atoms[i].idInAllAtoms];
            }
            extractedPos.Add(pos);
        }

        Debug.Log("Extracted " + idFrames.Count + " each of " + N + " atoms");

        string selName = s.name + "_pickedFrame_" + frameStart + "/" + frameEnd + "/" + step + "_" + N;

        UnityMolSelection pickedSel = new(resSel.atoms, newBonds : resSel.bonds, selName) {
            updateRepWithTraj = false,
            extractTrajFrame = true,
            extractTrajFrameIds = idFrames,
            extractTrajFramePositions = extractedPos
        };

        selM.Add(pickedSel);

        UnityMolMain.recordPythonCommand("pickTrajectoryFrames(\"" + structureName + "\", \"" + selectionQuery + "\", " + frameStart + ", " + frameEnd + ", " + step + ")");
        UnityMolMain.recordUndoPythonCommand("deleteSelection(\"" + pickedSel.name + "\")");

        return selName;
    }


    /// <summary>
    ///Set the current trajectory frame of the structure to a specific frame.
    /// frame has to be between 0 and the total number of frames.
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    /// <param name="frame"> ID of the frame</param>
    public static void setTrajFrame(string structureName, int frame) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (s == null) {
            Debug.LogError("Structure not found");
            return;
        }
        if (s.xdr == null) {
            Debug.LogError("No trajectory loaded for this structure");
            return;
        }
        if (frame < 0 || frame >= s.xdr.NumberFrames) {
            Debug.LogError("Wrong frame");
            return;
        }

        int prevFrame = s.xdr.CurrentFrame;

        s.trajSetFrame(frame);

        UnityMolMain.recordPythonCommand("setTrajFrame(\"" + structureName + "\", " + frame + ")");
        UnityMolMain.recordUndoPythonCommand("setTrajFrame(\"" + structureName + "\", " + prevFrame + ")");
    }


    /// <summary>
    /// Load a density map for a specific structure
    /// This function creates a DXReader instance in the UnityMolStructure
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    /// <param name="filePath">path of the density map file</param>
    public static void loadDXmap(string structureName, string filePath) {

        string realPath = filePath;
        string customPath = Path.Combine(path, filePath);
        if (File.Exists(customPath)) {
            realPath = customPath;
        }

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);

        try {
            s.readDX(realPath);
        } catch {
            Debug.LogError("Could not load DX map file '" + realPath + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("loadDXmap(\"" + structureName + "\", \"" + realPath.Replace("\\", "/") + "\")");
        UnityMolMain.recordUndoPythonCommand("unloadDXmap(\"" + structureName + "\")");
    }

    /// <summary>
    /// Show lines around the Density (DX) map
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    public static void showDXLines(string structureName) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (s != null) {
            if (s.dxr == null) {
                Debug.LogError("No DX map loaded for this structure");
                return;
            }
            s.dxr.showLines();
        } else {
            Debug.LogError("Wrong structure name");
            return;
        }
        UnityMolMain.recordPythonCommand("showDXLines(\"" + structureName + "\")");
        UnityMolMain.recordUndoPythonCommand("hideDXLines(\"" + structureName + "\")");
    }

    /// <summary>
    /// Hide lines around the Density (DX) map
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    public static void hideDXLines(string structureName) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (s != null) {
            if (s.dxr == null) {
                Debug.LogError("No DX map loaded for this structure");
                return;
            }
            s.dxr.hideLines();
        } else {
            Debug.LogError("Wrong structure name");
            return;
        }
        UnityMolMain.recordPythonCommand("hideDXLines(\"" + structureName + "\")");
        UnityMolMain.recordUndoPythonCommand("showDXLines(\"" + structureName + "\")");
    }

    /// <summary>
    /// Unload the density map of the structure
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    public static void unloadDXmap(string structureName) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (s != null) {
            s.unloadDX();
        } else {
            Debug.LogError("Wrong structure name");
            return;
        }

        UnityMolMain.recordPythonCommand("unloadDXmap(\"" + structureName + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Read a JSON file and display fieldLines for the specified structure
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    /// <param name="filePath">path of the JSON file</param>
    public static void readJSONFieldlines(string structureName, string filePath) {

        string realPath = filePath;
        string customPath = Path.Combine(path, filePath);
        if (File.Exists(customPath)) {
            realPath = customPath;
        }

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolSelection sel = s.currentModel.ToSelection();

        FieldLinesReader flr = new(realPath);

        s.currentModel.fieldLinesR = flr;

        deleteRepresentationInSelection(s.ToSelectionName(), "fl");

        if (!selM.selections.ContainsKey(s.ToSelectionName())) {
            selM.Add(sel);
        }

        repManager.AddRepresentation(sel, AtomType.fieldlines, BondType.nobond, flr);

        UnityMolMain.recordPythonCommand("readJSONFieldlines(\"" + structureName + "\", \"" + realPath.Replace("\\", "/") + "\")");
        UnityMolMain.recordUndoPythonCommand("unloadJSONFieldlines(\"" + structureName + "\")");
    }

    /// <summary>
    /// Remove the json file for fieldlines stored in the currentModel of the specified structure
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
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
    /// Change fieldlines computation gradient threshold
    /// </summary>
    /// <param name="selName">Name of the selection concerned.</param>
    /// <param name="val">New threshold value</param>
    public static void setFieldlineGradientThreshold(string selName, float val) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repM = UnityMolMain.getRepresentationManager();

        float prev = 10.0f;
        if (selM.selections.ContainsKey(selName)) {
            RepType repType = getRepType("fl");

            List<UnityMolRepresentation> existingReps = repM.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation rep in existingReps) {
                    SubRepresentation sr = rep.subReps.First(); //There shouldn't be more than one
                    FieldLinesRepresentation flRep = (FieldLinesRepresentation) sr.atomRep;
                    prev = flRep.magThreshold;
                    flRep.recompute(val);
                }
            }
        }
        int lenSameCom = 31 + selName.Length + 2;
        bool replaced = UnityMolMain.recordPythonCommand("setFieldlineGradientThreshold(\"" + selName + "\", " + val.ToString("f3", culture) + ")", true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("setFieldlineGradientThreshold(\"" + selName + "\", " + prev.ToString("f3", culture) + ")", replaced);
    }


    /// <summary>
    /// Utility function to be able to get the group of the structure
    /// This group is used to be able to move all the loaded molecules in the same group
    /// Groups can be between 0 and 9 included
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    /// <returns>The group of the structure. -1 means an issue.</returns>
    public static int getStructureGroup(string structureName) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return -1;
        }

        if (sm.nameToStructure.ContainsKey(structureName)) {
            UnityMolStructure s = sm.GetStructure(structureName);
            return s.groupID;
        }
        Debug.LogError("Wrong structure name");
        return -1;
    }

    /// <summary>
    ///  Utility function to be able to get all structures of the group
    /// This group is used to be able to move all the loaded molecules in the same group
    /// Groups can be between 0 and 9 included
    /// </summary>
    /// <param name="group">ID of the group</param>
    /// <returns>a list of UnityMolStructure belonging to the group. Can be empty.</returns>
    public static HashSet<UnityMolStructure> getStructuresOfGroup(int group) {
        HashSet<UnityMolStructure> result = new();

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        foreach (UnityMolStructure s in sm.loadedStructures) {
            if (s.groupID == group) {
                result.Add(s);
            }
        }
        return result;
    }

    /// <summary>
    /// Utility function to set the group of a structure
    /// This group is used to be able to move all the loaded molecules in the same group
    /// Groups can be between 0 and 9 included
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    /// <param name="newGroup">ID of the group</param>
    public static void setStructureGroup(string structureName, int newGroup) {

        int prevGroup;
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        if (sm.nameToStructure.ContainsKey(structureName)) {
            UnityMolStructure s = sm.GetStructure(structureName);
            prevGroup = s.groupID;
            s.groupID = newGroup;
        } else {
            Debug.LogWarning("No molecule loaded name '" + structureName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setStructureGroup(\"" + structureName + "\", " + newGroup + ")");
        UnityMolMain.recordUndoPythonCommand("setStructureGroup(\"" + structureName + "\", " + prevGroup + ")");

    }

    /// <summary>
    /// Delete a molecule based on its UnityMolStructure name.
    /// Delete also all its UnityMolSelection and UnityMolRepresentation
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    public static void delete(string structureName) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolStructure s = sm.GetStructure(structureName);

        if (s != null) { //Remove a complete loaded molecule
            sm.Delete(s);
            Debug.LogWarning("Deleting molecule '" + structureName + "'");
        } else {
            Debug.LogWarning("No molecule loaded name '" + structureName + "'");
            return;
        }
        UnityMolMain.recordPythonCommand("delete(\"" + structureName + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Show the representation type 'type' for all loaded molecules.
    /// Type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    /// <param name="type">Representation type</param>
    public static void show(string type) {

        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        RepType repType = getRepType(type);
        if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

            foreach (UnityMolStructure s in sm.loadedStructures) {
                List<UnityMolRepresentation> existingReps = repManager.representationExists(s.ToSelectionName(), repType);

                UnityMolSelection sel;
                if (!selM.selections.ContainsKey(s.ToSelectionName())) {
                    sel = s.currentModel.ToSelection();
                    selM.Add(sel);
                } else {
                    sel = selM.selections[s.ToSelectionName()];
                }

                if (existingReps == null) {
                    repManager.AddRepresentation(sel, repType.atomType, repType.bondType);
                } else {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.Show();
                    }
                }
            }
        } else {
            Debug.LogError("Wrong representation type");
            return;
        }

        UnityMolMain.recordPythonCommand("show(\"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("hide(\"" + type + "\")");
    }

    /// <summary>
    /// Show *only* the representation type 'type' for all loaded molecules.
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    /// <param name="type">Representation type</param>
    public static void showAs(string type) {

        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

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

                UnityMolSelection sel;
                if (!selM.selections.ContainsKey(s.ToSelectionName())) {
                    sel = s.currentModel.ToSelection();
                    selM.Add(sel);
                } else {
                    sel = selM.selections[s.ToSelectionName()];
                }

                List<UnityMolRepresentation> existingReps = repManager.representationExists(s.ToSelectionName(), repType);

                if (existingReps == null) {
                    repManager.AddRepresentation(sel, repType.atomType, repType.bondType);
                } else {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.Show();
                    }
                }
            }
        } else {
            Debug.LogError("Wrong representation type");
            return;
        }

        UnityMolMain.recordPythonCommand("showAs(\"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("hide(\"" + type + "\")");
    }

    /// <summary>
    ///  Restore all representations of a structure to the default representation
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
    public static void resetRep(string structureName) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        if (sm.nameToStructure.ContainsKey(structureName)) {
            UnityMolStructure s = sm.GetStructure(structureName);
            List<string> sels = new();
            foreach (UnityMolRepresentation r in s.representations) {
                if (!sels.Contains(r.selection.name)) {
                    sels.Add(r.selection.name);
                }
            }
            foreach (string sname in sels) {
                selM.Delete(sname);
                selM.RemoveSelectionKeyword(sname);
            }

            UnityMolSelection sel = s.ToSelection();
            defaultRep(sel.name);
        } else {
            Debug.LogWarning("No molecule loaded name '" + structureName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("resetRep(\"" + structureName + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Create selections and default representations: all in cartoon, not protein in hyperballs
    /// Also create a selection containing "not protein and not water and not ligand and not ions"
    /// </summary>
    /// <param name="selName">The default selection of the whole structure</param>
    /// <returns>True if success. False otherwise</returns>
    private static bool defaultRep(string selName) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            UnityMolSelection sel = selM.selections[selName];
            if (sel == null || sel.structures == null || sel.structures.Count <= 0) {
                return false;
            }
            //Remove all previous representations
            selM.DeleteRepresentations(sel);

            RepType repType = getRepType("c");
            RepType repTypehb = getRepType("hb");
            RepType repTypel = getRepType("l");
            RepType repTypep = getRepType("p");

            //Create protein or nucleic selection and show as cartoon
            string sName = sel.structures[0].name;
            string ProtOrNucSelName = sName + "_protein_or_nucleic";
            if (selM.selections.ContainsKey(ProtOrNucSelName)) {
                selM.DeleteRepresentations(selM.selections[ProtOrNucSelName]);
            }

            MDAnalysisSelection selecprotornuc = new("protein or nucleic", sel.atoms);
            UnityMolSelection retprotornuc = selecprotornuc.process();
            retprotornuc.name = ProtOrNucSelName;
            if (retprotornuc.Count != 0) {
                selM.Add(retprotornuc);
                selM.AddSelectionKeyword(retprotornuc.name, retprotornuc.name);
                repManager.AddRepresentation(retprotornuc, repType.atomType, repType.bondType);
            }

            bool cartoonEmpty = false;
            //If the cartoon is empty => show has hyperball
            List<UnityMolRepresentation> existingReps = repManager.representationExists(retprotornuc.name, repType);
            try {
                if (existingReps.Last() != null) {
                    SubRepresentation sr = existingReps.Last().subReps.Last();
                    CartoonRepresentation rep = (CartoonRepresentation) sr.atomRep;
                    if (rep == null || rep.totalVertices == 0) {
                        cartoonEmpty = true;
                    }
                }
            } catch {
                cartoonEmpty = true;
            }

            if (cartoonEmpty && retprotornuc.Count != 0) {
                repManager.AddRepresentation(retprotornuc, repTypehb.atomType, repTypehb.bondType);
            }

            MDAnalysisSelection selecwat = new MDAnalysisSelection("water", sel.atoms);
            UnityMolSelection retwat = selecwat.process();
            retwat.name = sName + "_water";
            bool containsWat = retwat.Count != 0;
            if (containsWat) {
                selM.Add(retwat);

                if (retwat.bonds == null || retwat.bonds.Count == 0) {
                    repManager.AddRepresentation(retwat, repTypep.atomType, repTypep.bondType);
                } else {
                    repManager.AddRepresentation(retwat, repTypel.atomType, repTypel.bondType);
                }

            }

            //Create not protein/nucleic selection and show as hb if the cartoon was correctly shown
            string notPSelName = sName + "_not_protein_nucleic";
            if (selM.selections.ContainsKey(notPSelName)) {
                selM.DeleteRepresentations(selM.selections[notPSelName]);
            }
            MDAnalysisSelection selec = new("not protein and not nucleic" + (containsWat ? " and not water" : ""), sel.atoms);
            UnityMolSelection ret = selec.process();
            ret.name = notPSelName;
            bool shownNotProtAsHB = false;

            if (ret.Count != 0) {
                if (!cartoonEmpty) { //Show not protein as hb only if the cartoon was successfully shown
                    repManager.AddRepresentation(ret, repTypehb.atomType, repTypehb.bondType);
                    shownNotProtAsHB = true;
                }

                selM.Add(ret);
                selM.AddSelectionKeyword(ret.name, ret.name);
            }

            //Unknown atoms = not protein/nucleic/ligand/water/ions
            MDAnalysisSelection selecUnreco = new("not protein and not nucleic and not water and not ligand and not ions", sel.atoms);
            UnityMolSelection selUnreco = selecUnreco.process();
            selUnreco.name = sName + "_unrecognized_atoms";

            if (selUnreco.Count != 0) {
                selM.Add(selUnreco);
                selM.AddSelectionKeyword(selUnreco.name, selUnreco.name);
                if (cartoonEmpty && !shownNotProtAsHB) {
                    repManager.AddRepresentation(selUnreco, repTypehb.atomType, repTypehb.bondType);
                }
            }

            //If nothing was shown as hyperball or cartoon => show as hyperball
            if (ret.Count == 0 && cartoonEmpty && retprotornuc.Count != 0) {
                repManager.AddRepresentation(retprotornuc, repTypehb.atomType, repTypehb.bondType);
            }

            // selM.SetCurrentSelection(sel);
            // sel.mergeRepresentations(ret);
            selM.ClearCurrentSelection();
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return false;
        }
        return true;

    }

    /// <summary>
    /// Create selections and default representations: all in cartoon, not protein in hyperballs
    /// Also create a selection containing "not protein and not water and not ligand and not ions"
    /// </summary>
    /// <param name="selName">The default selection of the whole structure</param>
    public static void showDefault(string selName) {

        if (!defaultRep(selName)) {
            return;
        }

        UnityMolMain.recordPythonCommand("showDefault(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("hideSelection(\"" + selName + "\", \"c\")\nhideSelection(\"" + selName + "\", \"hb\")\nhideSelection(\"" + (selName + "_not_protein") + "\", \"hb\")");
    }

    /// <summary>
    /// Show all representations already created for a specified structure
    /// </summary>
    /// <param name="structureName">Name of the structure concerned</param>
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
        } else {
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
    /// Surface example: showSelection("all_1kx2", "s", True, True, True, SurfMethod.MSMS) # arguments are cutByChain, AO, cutSurface, computeSurfaceMethod
    /// Iso-surface example: showSelection("all_1kx2", "dxiso", last().dxr, 0.0f)
    /// </summary>
    /// <param name="selName">The selection name to show</param>
    /// <param name="type">The type of the representation</param>
    /// <param name="args">The options for the representation chosen.</param>
    public static void showSelection(string selName, string type, params object[] args) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType(type);

            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                // List should be equal 1 always.
                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);

                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        if (repType.atomType != AtomType.noatom && sel.atoms.Count != existingRep.nbAtomsInRep) {
                            existingRep.updateWithNewSelection(sel);
                        } else if (repType.bondType != BondType.nobond && sel.bonds.Count != existingRep.nbBondsInRep) {
                            existingRep.updateWithNewSelection(sel);
                        }

                        existingRep.Show();
                    }
                } else {
                    repManager.AddRepresentation(sel, repType.atomType, repType.bondType, args);
                }
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        string command = "showSelection(\"" + selName + "\", \"" + type + "\"";
        foreach (object o in args) {
            if (o is string) {
                command += ", \"" + o + "\"";
            } else if (o is float) {
                command += ", " + ((float) o).ToString("f3", culture);
            } else if (o is Vector3) {
                command += ", " + cVec3ToPy((Vector3)o);
            } else if (o is SurfMethod) {
                command += ", SurfMethod." + o;
            }
            else {
                command += ", " + o;
            }
        }
        command += ")";
        UnityMolMain.recordPythonCommand(command);
        UnityMolMain.recordUndoPythonCommand("hideSelection(\"" + selName + "\", \"" + type + "\")");
    }

    /// <summary>
    /// Show all representations of the selection named 'selName'
    /// </summary>
    /// <param name="selName">The selection name concerned.</param>
    public static void showSelection(string selName) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];
            foreach (List<UnityMolRepresentation> lr in sel.representations.Values) {
                foreach (UnityMolRepresentation r in lr) {
                    r.Show();
                }
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }
        UnityMolMain.recordPythonCommand("showSelection(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("hideSelection(\"" + selName + "\")");
    }

    /// <summary>
    /// Hide all representations of the selection named 'selName'
    /// </summary>
    /// <param name="selName">The selection name concerned.</param>
    public static void hideSelection(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];
            foreach (List<UnityMolRepresentation> lr in sel.representations.Values) {
                foreach (UnityMolRepresentation r in lr) {
                    r.Hide();
                }
            }
        } else {
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
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of representation</param>
    public static void hideSelection(string selName, string type) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);

                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.Hide();
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("hideSelection(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("showSelection(\"" + selName + "\", \"" + type + "\")");
    }

    /// <summary>
    /// Delete every representation of type 'type' of the specified selection
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of representation</param>
    public static void deleteRepresentationInSelection(string selName, string type) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                UnityMolSelection sel = selM.selections[selName];

                selM.DeleteRepresentation(sel, repType);
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("deleteRepresentationInSelection(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("showSelection(\"" + selName + "\", \"" + type + "\")");
    }

    /// <summary>
    /// Delete all representations of the specified selection
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    public static void deleteRepresentationsInSelection(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];
            selM.DeleteRepresentations(sel);
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("deleteRepresentationsInSelection(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Hide all representations of the given structure.
    /// </summary>
    /// <param name="structureName">The structure name concerned</param>
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
        } else {
            Debug.LogWarning("No molecule loaded name '" + structureName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("hideStructureAllRepresentations(\"" + structureName + "\")");
        UnityMolMain.recordUndoPythonCommand("showStructureAllRepresentations(\"" + structureName + "\")");
    }

    /// <summary>
    /// Delete all representations of the given structure.
    /// </summary>
    /// <param name="structureName">The structure name concerned</param>
    public static void deleteAllSelectionsStructure(string structureName) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        if (sm.nameToStructure.ContainsKey(structureName)) {
            UnityMolStructure s = sm.GetStructure(structureName);
            List<string> selNames = new();

            foreach (UnityMolSelection sel in selM.selections.Values) {
                if (sel.structures.Contains(s)) {
                    selNames.Add(sel.name);
                }
            }

            foreach (string t in selNames)
            {
                selM.Delete(t);
                selM.RemoveSelectionKeyword(t);
            }
        } else {
            Debug.LogWarning("No molecule loaded name '" + structureName + "'");
            return;
        }
        UnityMolMain.recordPythonCommand("deleteAllSelectionsStructure(\"" + structureName + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Test whether at least one representation of a given structure is shown or not
    /// </summary>
    /// <param name="structureName">The structure name concerned</param>
    /// <returns>True if representation is shown. False otherwise</returns>
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
        } else {
            Debug.LogWarning("No molecule loaded name '" + structureName + "'");
        }
        return false;
    }

    /// <summary>
    /// Test whether a representation of type 'type' is shown for a specified selection
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of representation</param>
    /// <returns>True if representation is shown. False otherwise</returns>
    public static bool areRepresentationsOn(string selName, string type) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
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
            } else {
                Debug.LogError("Wrong representation type");
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
        }
        return false;
    }

    /// <summary>
    /// Hide all representations of type 'type'
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    /// <param name="type">The type of representation</param>
    public static void hide(string type) {

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
        } else {
            Debug.LogError("Wrong representation type");
            return;
        }

        UnityMolMain.recordPythonCommand("hide(\"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("show(\"" + type + "\")");
    }

    /// <summary>
    /// Switch between the 2 types of surface computation methods: EDTSurf and MSMS
    /// for a given selection.
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    public static void switchSurfaceComputeMethod(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

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
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("switchSurfaceComputeMethod(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("switchSurfaceComputeMethod(\"" + selName + "\")");
    }

    /// <summary>
    /// Switch between cut surface mode and no-cut surface mode
    /// for a given selection.
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="isCut">Active cut surface mode?</param>
    public static void switchCutSurface(string selName, bool isCut) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

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
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("switchCutSurface(\"" + selName + "\", " + isCut + ")");
        UnityMolMain.recordUndoPythonCommand("switchCutSurface(\"" + selName + "\", " + !isCut + ")");
    }

    /// <summary>
    /// Switch all surface representations in selection to a solid surface material
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    public static void setSolidSurface(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        bool wasWireframe = false;
        bool wasTransparent = false;

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType("s");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolSurfaceManager surfM = (UnityMolSurfaceManager) sr.atomRepManager;
                        if (surfM.isWireframe) {
                            wasWireframe = true;
                            surfM.SwitchWireframe();
                        } else if (surfM.isTransparent) {
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
                        } else if (surfM.isTransparent) {
                            wasTransparent = true;
                            surfM.SwitchTransparent();
                        }
                    }
                }
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setSolidSurface(\"" + selName + "\")");

        if (wasTransparent) {
            UnityMolMain.recordUndoPythonCommand("setTransparentSurface(\"" + selName + "\")");
        } else if (wasWireframe) {
            UnityMolMain.recordUndoPythonCommand("setWireframeSurface(\"" + selName + "\")");
        } else {
            UnityMolMain.recordUndoPythonCommand("");
        }
    }

    /// <summary>
    /// Switch all surface representations in selection to a wireframe surface material when available
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    public static void setWireframeSurface(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        bool wasSolid = false;
        bool wasTransparent = false;

        if (selM.selections.ContainsKey(selName)) {

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
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setWireframeSurface(\"" + selName + "\")");

        if (wasTransparent) {
            UnityMolMain.recordUndoPythonCommand("setTransparentSurface(\"" + selName + "\")");
        } else if (wasSolid) {
            UnityMolMain.recordUndoPythonCommand("setSolidSurface(\"" + selName + "\")");
        } else {
            UnityMolMain.recordUndoPythonCommand("");
        }
    }

    /// <summary>
    /// Switch all surface representations in selection to a transparent surface material
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="alpha">Value of the transparency.</param>
    public static void setTransparentSurface(string selName, float? alpha = null) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        bool wasSolid = false;
        bool wasWireframe = false;

        if (selM.selections.ContainsKey(selName)) {

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
                        if (alpha.HasValue) {
                            surfM.SetAlpha(alpha.Value);
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
                        if (alpha.HasValue) {
                            surfM.SetAlpha(alpha.Value);
                        }
                    }
                }
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        int lenSameCom = 22 + selName.Length + 2;
        string command = "setTransparentSurface(\"" + selName;
        if (alpha.HasValue) {
            command += "\", " + alpha.Value.ToString("f2", culture) + ")";
        }
        else {
            command += "\")";
        }
        bool replaced = UnityMolMain.recordPythonCommand(command, true, lenSameCom);

        if (wasWireframe) {
            UnityMolMain.recordUndoPythonCommand("setWireframeSurface(\"" + selName + "\")", replaced);
        } else if (wasSolid) {
            UnityMolMain.recordUndoPythonCommand("setSolidSurface(\"" + selName + "\")", replaced);
        } else {
            UnityMolMain.recordUndoPythonCommand("", replaced);
        }
    }
    /// <summary>
    /// Switch cartoon material from transparent to normal/solid for a selection.
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    public static void setSolidCartoon(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        float prevAlpha = 0.3f;

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType("c");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolCartoonManager cM = (UnityMolCartoonManager) sr.atomRepManager;

                        prevAlpha = cM.curAlpha;
                        if (cM.isTransparent) {
                            cM.SwitchTransparent();
                        }
                    }
                }
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setSolidCartoon(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("setTransparentCartoon(\"" + selName + "\", " + prevAlpha.ToString("f2", culture) + ")");
    }

    /// <summary>
    /// Set the cartoon material to transparent for a selection
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="alpha">Value of the transparency</param>
    public static void setTransparentCartoon(string selName, float alpha = 0.3f) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        bool wasSolid = false;
        float prevAlpha = 0.3f;

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType("c");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolCartoonManager cM = (UnityMolCartoonManager) sr.atomRepManager;

                        prevAlpha = cM.curAlpha;

                        if (!cM.isTransparent) {
                            wasSolid = true;
                            cM.SwitchTransparent();
                        }
                        cM.SetAlpha(alpha);
                    }
                }
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        int lenSameCom = 23 + selName.Length + 2;
        bool replaced = UnityMolMain.recordPythonCommand("setTransparentCartoon(\"" + selName + "\", " + alpha.ToString("f2", culture) + ")", true, lenSameCom);
        if (wasSolid) {
            UnityMolMain.recordUndoPythonCommand("setSolidCartoon(\"" + selName + "\")", replaced);
        } else {
            UnityMolMain.recordUndoPythonCommand("setTransparentCartoon(\"" + selName + "\", " + prevAlpha.ToString("f2", culture) + ")", replaced);
        }
    }

    /// <summary>
    /// Switch sphere material from transparent to normal/solid for a given selection
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    public static void setSolidSphere(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        float prevAlpha = 0.3f;

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType("sphere");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolSphereManager sM = (UnityMolSphereManager) sr.atomRepManager;

                        prevAlpha = sM.curAlpha;
                        if (sM.isTransparent) {
                            sM.SwitchTransparent();
                        }
                    }
                }
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setSolidSphere(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("setTransparentSphere(\"" + selName + "\", " + prevAlpha.ToString("f2", culture) + ")");
    }

    /// <summary>
    /// Set the sphere material to transparent for a selection
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="alpha">Value of the transparency</param>
    public static void setTransparentSphere(string selName, float alpha = 0.3f) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        bool wasSolid = false;
        float prevAlpha = 0.3f;

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType("sphere");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolSphereManager sM = (UnityMolSphereManager) sr.atomRepManager;

                        prevAlpha = sM.curAlpha;

                        if (!sM.isTransparent) {
                            wasSolid = true;
                            sM.SwitchTransparent();
                        }
                        sM.SetAlpha(alpha);
                    }
                }
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        int lenSameCom = 21 + selName.Length + 2;
        bool replaced = UnityMolMain.recordPythonCommand("setTransparentSphere(\"" + selName + "\", " + alpha.ToString("f2", culture) + ")", true, lenSameCom);
        if (wasSolid) {
            UnityMolMain.recordUndoPythonCommand("setSolidSphere(\"" + selName + "\")", replaced);
        } else {
            UnityMolMain.recordUndoPythonCommand("setTransparentSphere(\"" + selName + "\", " + prevAlpha.ToString("f2", culture) + ")", replaced);
        }
    }

    /// <summary>
    /// Recompute cartoon representation with new tube size
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="newVal">Tube size</param>
    public static void setTubeSizeCartoon(string selName, float newVal) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        float prevVal = 1.0f;

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType("c");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolCartoonManager cM = (UnityMolCartoonManager) sr.atomRepManager;

                        prevVal = cM.atomRep.customTubeSize;

                        cM.SetTubeSize(newVal);
                    }
                }
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        int lenSameCom = 20 + selName.Length + 2;
        bool replaced = UnityMolMain.recordPythonCommand("setTubeSizeCartoon(\"" + selName + "\", " + newVal.ToString("f2", culture) + ")", true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("setTubeSizeCartoon(\"" + selName + "\", " + prevVal.ToString("f2", culture) + ")", replaced);
    }

    /// <summary>
    /// Draw cartoon representation as tube for a selection
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="drawAsTube">Whether to draw it as a tube or not</param>
    public static void drawCartoonAsTube(string selName, bool drawAsTube = true) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType("c");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolCartoonManager cM = (UnityMolCartoonManager) sr.atomRepManager;

                        cM.DrawAsTube(drawAsTube);
                    }
                }
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("drawCartoonAsTube(\"" + selName + "\", " + cBoolToPy(drawAsTube) + ")");
        UnityMolMain.recordUndoPythonCommand("drawCartoonAsTube(\"" + selName + "\", " + cBoolToPy(!drawAsTube) + ")");
    }

    /// <summary>
    /// Draw cartoon representation as tube with Bfactor as a tube size for a selection
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="drawAsBTube">Whether to draw it as a tube or not</param>
    public static void drawCartoonAsBfactorTube(string selName, bool drawAsBTube = true) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType("c");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolCartoonManager cM = (UnityMolCartoonManager) sr.atomRepManager;

                        cM.DrawAsBfactorTube(drawAsBTube);
                    }
                }
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("drawCartoonAsBfactorTube(\"" + selName + "\", " + cBoolToPy(drawAsBTube) + ")");
        UnityMolMain.recordUndoPythonCommand("drawCartoonAsBfactorTube(\"" + selName + "\", " + cBoolToPy(!drawAsBTube) + ")");
    }

    /// <summary>
    /// Recompute the DX surface with a new iso value for a selection.
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="newVal">New iso value</param>
    public static void updateDXIso(string selName, float newVal) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();
        float prevVal = 0.0f;

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType("dxiso");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        prevVal = ((DXSurfaceRepresentation) sr.atomRep).isoValue;
                        ((DXSurfaceRepresentation) sr.atomRep).isoValue = newVal;
                        ((DXSurfaceRepresentation) sr.atomRep).recompute();
                    }
                }
            }
        }

        int lenSameCom = 13 + selName.Length + 2;
        bool replaced = UnityMolMain.recordPythonCommand("updateDXIso(\"" + selName + "\", " + newVal.ToString("F3", culture) + ")", true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("updateDXIso(\"" + selName + "\", " + prevVal.ToString("F3", culture) + ")", replaced);
    }

    /// <summary>
    /// Set the smoothness of a given representation of a given selection
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    /// <param name="val">the new smoothness value</param>
    public static void setSmoothness(string selName, string type, float val) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

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
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        int lenSameCom = 15 + selName.Length + 4 + type.Length + 2;
        bool replaced = UnityMolMain.recordPythonCommand("setSmoothness(\"" + selName + "\", \"" + type + "\", " +
                        val.ToString("f2", culture) + ")", true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("", replaced);
    }

    /// <summary>
    /// Set the metal value of a given representation of a given selection
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// Does nothing if the representation has no metal feature.
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    /// <param name="val">the new metal value</param>
    public static void setMetal(string selName, string type, float val) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

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
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        int lenSameCom = 10 + selName.Length + 4 + type.Length + 2;
        bool replaced = UnityMolMain.recordPythonCommand("setMetal(\"" + selName + "\", \"" + type + "\", " +
                        val.ToString("f2", culture) + ")", true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("", replaced);
    }

    /// <summary>
    /// Set the surface wireframe size
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    /// <param name="val">the new wireframe size</param>
    public static void setSurfaceWireframe(string selName, string type, float val) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            if (sr.atomRepManager != null) {
                                ((UnityMolSurfaceManager) sr.atomRepManager).SetWireframeSize(val);
                            }
                        }
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        int lenSameCom = 21 + selName.Length + 4 + type.Length + 2;
        bool replaced = UnityMolMain.recordPythonCommand("setSurfaceWireframe(\"" + selName + "\", \"" + type + "\", " +
                        val.ToString("f2", culture) + ")", true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("", replaced);
    }

    /// <summary>
    /// Show only a part of the representation inside a sphere.
    /// <remarks>Only works with surface or cartoon types for now</remarks>
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    public static void enableLimitedView(string selName, string type) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            if (sr.atomRepManager != null) {
                                if (repType.atomType == AtomType.surface || repType.atomType == AtomType.DXSurface) {
                                    ((UnityMolSurfaceManager) sr.atomRepManager).activateLimitedView();
                                }

                                if (repType.atomType == AtomType.cartoon) {
                                    ((UnityMolCartoonManager) sr.atomRepManager).activateLimitedView();
                                }
                            }
                        }
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }
        UnityMolMain.recordPythonCommand("enableLimitedView(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("disableLimitedView(\"" + selName + "\", \"" + type + "\")");
    }

    /// <summary>
    /// Disable the limited view which is a part of the representation inside a sphere.
    /// <remarks>Only works with surface or cartoon types for now</remarks>
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    public static void disableLimitedView(string selName, string type) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            if (sr.atomRepManager != null) {
                                if (repType.atomType == AtomType.cartoon) {
                                    ((UnityMolCartoonManager) sr.atomRepManager).disableLimitedView();
                                } else {
                                    ((UnityMolSurfaceManager) sr.atomRepManager).disableLimitedView();
                                }
                            }
                        }
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }
        UnityMolMain.recordPythonCommand("disableLimitedView(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("enableLimitedView(\"" + selName + "\", \"" + type + "\")");
    }

    /// <summary>
    /// Test if the limited view is active or not.
    /// <remarks>Only works with surface or cartoon types for now</remarks>
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    public static bool getLimitedView(string selName, string type) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            if (sr.atomRepManager != null) {
                                if (repType.atomType == AtomType.cartoon) {
                                    return ((UnityMolCartoonManager) sr.atomRepManager).limitedView;
                                }

                                return ((UnityMolSurfaceManager) sr.atomRepManager).limitedView;
                            }
                        }
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return false;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
        }
        return false;
    }

    /// <summary>
    /// Set the center of the limited view
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    /// <param name="center">The new center</param>
    public static void setLimitedViewCenter(string selName, string type, Vector3 center) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        Vector3 prevCenter = Vector3.zero;
        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            if (sr.atomRepManager != null) {
                                if (repType.atomType == AtomType.cartoon) {
                                    prevCenter = ((UnityMolCartoonManager) sr.atomRepManager).limitedViewCenter;
                                    ((UnityMolCartoonManager) sr.atomRepManager).setLimitedViewCenter(center);
                                }
                                else {
                                    prevCenter = ((UnityMolSurfaceManager) sr.atomRepManager).limitedViewCenter;
                                    ((UnityMolSurfaceManager) sr.atomRepManager).setLimitedViewCenter(center);
                                }
                            }
                        }
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }
        UnityMolMain.recordPythonCommand(string.Format(CultureInfo.InvariantCulture, "setLimitedViewCenter(\"{0}\", \"{1}\", Vector3{2})", selName, type, center));
        UnityMolMain.recordUndoPythonCommand(string.Format(CultureInfo.InvariantCulture, "setLimitedViewCenter(\"{0}\", \"{1}\", Vector3{2})", selName, type, prevCenter));
    }

    /// <summary>
    /// Retrieve the current center of the limited view
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    /// <returns>The current center</returns>
    public static Vector3 getLimitedViewCenter(string selName, string type) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            if (sr.atomRepManager != null) {
                                if (repType.atomType == AtomType.cartoon) {
                                    return ((UnityMolCartoonManager) sr.atomRepManager).limitedViewCenter;
                                }
                                return ((UnityMolSurfaceManager) sr.atomRepManager).limitedViewCenter;
                            }
                        }
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return Vector3.zero;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return Vector3.zero;
        }
        return Vector3.zero;
    }

    /// <summary>
    /// Set the radius (in Angstrom) of the limited view
    /// <remarks>Only works with surface or cartoon types for now</remarks>
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    /// <param name="radius">the new radius in Angstrom</param>
    public static void setLimitedViewRadius(string selName, string type, float radius) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        float prevRadius = 1.0f;
        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            if (sr.atomRepManager != null) {
                                if (repType.atomType == AtomType.cartoon) {
                                    prevRadius = ((UnityMolCartoonManager) sr.atomRepManager).limitedViewRadius;
                                    ((UnityMolCartoonManager) sr.atomRepManager).setLimitedViewRadius(radius);
                                }
                                else {
                                    prevRadius = ((UnityMolSurfaceManager) sr.atomRepManager).limitedViewRadius;
                                    ((UnityMolSurfaceManager) sr.atomRepManager).setLimitedViewRadius(radius);
                                }
                            }
                        }
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }
        int lenSameCom = 22 + selName.Length + 4 + type.Length + 2;
        bool replaced = UnityMolMain.recordPythonCommand("setLimitedViewRadius(\"" + selName + "\", \"" + type + "\", " + radius.ToString("f2", culture) + ")", true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("setLimitedViewRadius(\"" + selName + "\", \"" + type + "\", " + prevRadius.ToString("f2", culture) + ")", replaced);

    }

    /// <summary>
    /// Change hyperballs metaphor parameters in all selections that contains a Hyperballs representation
    /// Metaphor can be "Smooth", "Balls&Sticks", "VdW", "Licorice", "Hidden"
    /// </summary>
    /// <param name="metaphor">the new metaphor</param>
    /// <param name="forceAOOff">Disable Ambient occlusion?</param>
    /// <param name="lerp">Activate linear interpolation when changing the metaphor?</param>
    /// <param name="duration">Duration of the linear interpolation</param>
    public static void setHyperBallMetaphore(string metaphor, bool forceAOOff = true, bool lerp = false, float duration = 0.5f) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        float scaleBond, scaleAtom, shrink;
        bool doAO = false;

        switch (metaphor) {
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
            if (!forceAOOff) {
                doAO = true;
            }
            break;
        case "Licorice":
        case "licorice":
            scaleAtom = 0.3f;
            scaleBond = 0.3f;
            shrink = 0.001f;
            break;
        case "Hidden":
        case "hidden":
        case "hide":
            scaleAtom = 0.0f;
            scaleBond = 0.0f;
            shrink = 1.0f;
            break;
        default:
            Debug.LogError("Metaphor not recognized");
            return;
        }


        RepType repType = getRepType("hb");

        if (lerp) {
            Vector3 prevParams = getHyperballParams();
            Vector3 targetParams = new(scaleAtom, scaleBond, shrink);
            instance.setHyperballParam("", prevParams, targetParams, duration);
        }
        else {
            foreach (string selName in selM.selections.Keys) {
                UnityMolSelection sel = selM.selections[selName];

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
        }

        UnityMolMain.recordPythonCommand("setHyperBallMetaphore(\"" + metaphor + "\", " + cBoolToPy(forceAOOff) + ", " + cBoolToPy(lerp) + ", " + duration.ToString("f3", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Retrieve the Hyperball parameters : scaleAtom, scaleBonds & shrink
    /// </summary>
    /// <param name="selName">The selection name concerned. If "", look for the 1st Hyperball representation across all.</param>
    /// <returns>The hyperball parameters</returns>
    private static Vector3 getHyperballParams(string selName = "") {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();
        RepType repType = getRepType("hb");

        Vector3 res = new(1.0f, 1.0f, 0.1f);
        if (selName == "") {
            foreach (string seln in selM.selections.Keys) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(seln, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            UnityMolHStickMeshManager hsManager = (UnityMolHStickMeshManager) sr.bondRepManager;
                            UnityMolHBallMeshManager hbManager = (UnityMolHBallMeshManager) sr.atomRepManager;
                            if (hsManager != null) {
                                res.y = hsManager.scaleBond;
                                res.z = hsManager.shrink;
                            }
                            if (hbManager != null) {
                                res.x = hbManager.lastScale;
                            }
                            if (hsManager != null && hbManager != null) {
                                return res;
                            }
                        }
                    }
                }
            }
        }
        else {
            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolHStickMeshManager hsManager = (UnityMolHStickMeshManager) sr.bondRepManager;
                        UnityMolHBallMeshManager hbManager = (UnityMolHBallMeshManager) sr.atomRepManager;
                        if (hsManager != null) {
                            res.y = hsManager.scaleBond;
                            res.z = hsManager.shrink;
                        }
                        if (hbManager != null) {
                            res.x = hbManager.lastScale;
                        }
                        if (hsManager != null && hbManager != null) {
                            return res;
                        }
                    }
                }
            }
        }
        return res;
    }

    /// <summary>
    /// Change the hyperball parameters of the selection across a certain duration.
    /// Use coroutine.
    /// </summary>
    /// <param name="selName">the selection name concerned</param>
    /// <param name="prevScaleShrink">the previous parameters</param>
    /// <param name="scaleShrink">the new parameters</param>
    /// <param name="duration">Duration of the change</param>
    private void setHyperballParam(string selName, Vector3 prevScaleShrink, Vector3 scaleShrink, float duration) {
        StartCoroutine(delayedSetHyperballParam(selName, prevScaleShrink, scaleShrink, duration));
    }

    /// <summary>
    /// Coroutine handling the changes of hyperball parameters
    /// </summary>
    /// <param name="selName">the selection name concerned</param>
    /// <param name="prevScaleShrink">the previous parameters</param>
    /// <param name="scaleShrink">the new parameters</param>
    /// <param name="duration">Duration of the change</param>
    /// <returns>IEnumerator</returns>
    private IEnumerator delayedSetHyperballParam(string selName, Vector3 prevScaleShrink, Vector3 scaleShrink, float duration) {
        //End of frame
        yield return 0;

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();
        RepType repType = getRepType("hb");

        //Set everything to starting scales and shrink
        if (selName == "") {
            foreach (string seln in selM.selections.Keys) {
                UnityMolSelection sel = selM.selections[seln];

                List<UnityMolRepresentation> existingReps = repManager.representationExists(seln, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            UnityMolHStickMeshManager hsManager = (UnityMolHStickMeshManager) sr.bondRepManager;
                            UnityMolHBallMeshManager hbManager = (UnityMolHBallMeshManager) sr.atomRepManager;
                            if (hsManager != null) {
                                hsManager.SetShrink(prevScaleShrink.z);
                                hsManager.SetSizes(sel.atoms, prevScaleShrink.y);
                            }

                            hbManager.SetSizes(sel.atoms, prevScaleShrink.x);
                        }
                    }
                }
            }
        }

        float multi = 1.0f / duration;
        float ratio = 0.0f;
        Vector3 current = prevScaleShrink;
        while (current != scaleShrink) {
            ratio += Time.deltaTime * multi;
            current = Vector3.Lerp(prevScaleShrink, scaleShrink, ratio);

            if (selName == "") {

                foreach (string sname in selM.selections.Keys) {
                    UnityMolSelection sel = selM.selections[sname];

                    List<UnityMolRepresentation> existingReps = repManager.representationExists(sname, repType);
                    if (existingReps != null) {
                        foreach (UnityMolRepresentation existingRep in existingReps) {
                            foreach (SubRepresentation sr in existingRep.subReps) {
                                UnityMolHStickMeshManager hsManager = (UnityMolHStickMeshManager) sr.bondRepManager;
                                UnityMolHBallMeshManager hbManager = (UnityMolHBallMeshManager) sr.atomRepManager;
                                if (hsManager != null) {
                                    hsManager.SetShrink(current.z);
                                    hsManager.SetSizes(sel.atoms, current.y);
                                }
                                hbManager.SetSizes(sel.atoms, current.x);
                            }
                        }
                    }
                }
            }
            else {
                UnityMolSelection sel = selM.selections[selName];

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            UnityMolHStickMeshManager hsManager = (UnityMolHStickMeshManager) sr.bondRepManager;
                            UnityMolHBallMeshManager hbManager = (UnityMolHBallMeshManager) sr.atomRepManager;
                            if (hsManager != null) {
                                hsManager.SetShrink(current.z);
                                hsManager.SetSizes(sel.atoms, current.y);
                            }
                            hbManager.SetSizes(sel.atoms, current.x);
                        }
                    }
                }
            }

            yield return 0;
        }
    }

    /// <summary>
    /// Change hyperballs metaphor parameters for a selection that contains a Hyperballs representation
    /// Metaphor can be "Smooth", "Balls&Sticks", "VdW", "Licorice", "Hidden"
    /// </summary>
    /// <param name="selName">the selection name concerned</param>
    /// <param name="metaphor">the new metaphor</param>
    /// <param name="forceAOOff">Disable Ambient occlusion?</param>
    /// <param name="lerp">Activate linear interpolation when changing the metaphor?</param>
    /// <param name="duration">Duration of the linear interpolation</param>
    public static void setHyperBallMetaphore(string selName, string metaphor, bool forceAOOff = true, bool lerp = false, float duration = 0.5f) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        float scaleBond;
        float scaleAtom;
        float shrink;
        bool doAO = false;

        switch (metaphor) {
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
            if (!forceAOOff) {
                doAO = true;
            }

            break;
        case "Licorice":
        case "licorice":
            scaleAtom = 0.3f;
            scaleBond = 0.3f;
            shrink = 0.001f;
            break;
        case "Hidden":
        case "hidden":
        case "hide":
            scaleAtom = 0.0f;
            scaleBond = 0.0f;
            shrink = 1.0f;
            break;
        default:
            Debug.LogError("Metaphore not recognized");
            return;
        }

        if (selM.selections.ContainsKey(selName)) {
            if (lerp) {
                Vector3 prevParams = getHyperballParams(selName);
                Vector3 targetParams = new(scaleAtom, scaleBond, shrink);
                instance.setHyperballParam(selName, prevParams, targetParams, duration);
            }
            else {
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
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setHyperBallMetaphore(\"" + selName + "\", \"" + metaphor + "\", " + cBoolToPy(forceAOOff) + ", " + cBoolToPy(lerp) + ", " + duration.ToString("f3", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Set the shininess for the hyperball representation for a given selection
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="shin">the new shininess value</param>
    public static void setHyperBallShininess(string selName, float shin) {

        float prev = 0.0f;

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

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

        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        int lenSameCom = 23 + selName.Length + 2;
        bool replaced = UnityMolMain.recordPythonCommand("setHyperBallShininess(\"" + selName + "\", " +
                        shin.ToString("f3", culture) + ")", true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("setHyperBallShininess(\"" + selName + "\", " +
                                             prev.ToString("f3", culture) + ")", replaced);
    }

    /// <summary>
    /// Set the shrink factor for the hyperball representation for a given selection
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="shrink">the new shrink factor</param>
    public static void setHyperballShrink(string selName, float shrink) {

        float prev = 0.0f;

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

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
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        int lenSameCom = 20 + selName.Length + 2;
        bool replaced = UnityMolMain.recordPythonCommand("setHyperballShrink(\"" + selName + "\", " +
                        shrink.ToString("f3", culture) + ")", true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("setHyperballShrink(\"" + selName + "\", " +
                                             prev.ToString("f3", culture) + ")", replaced);
    }

    /// <summary>
    /// Change all hyperball representations in the selection with a new texture
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="idTex">Texture Index in UnityMolMain.atomColors.textures</param>
    public static void setHyperballTexture(string selName, int idTex) {

        if (idTex >= UnityMolMain.atomColors.textures.Length) {
            Debug.LogError("Invalid Texture index " + idTex + " " + UnityMolMain.atomColors.textures.Length);
            return;
        }

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType("hb");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolHBallMeshManager hbManager = (UnityMolHBallMeshManager) sr.atomRepManager;
                        UnityMolHStickMeshManager hsManager = (UnityMolHStickMeshManager) sr.bondRepManager;
                        hbManager.SetTexture(idTex);
                        hsManager.SetTexture(idTex);
                    }
                }
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setHyperballTexture(\"" + selName + "\", " + idTex + ")");
        UnityMolMain.recordUndoPythonCommand("setHyperballTexture(\"" + selName + "\", 0)");
    }

    /// <summary>
    /// Change all bond order representations in the selection with a new texture
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="idTex">Texture Index in UnityMolMain.atomColors.textures</param>
    public static void setBondOrderTexture(string selName, int idTex) {

        if (idTex >= UnityMolMain.atomColors.textures.Length) {
            Debug.LogError("Invalid Texture index " + idTex + " " + UnityMolMain.atomColors.textures.Length);
            return;
        }

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

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
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setBondOrderTexture(\"" + selName + "\", " + idTex + ")");
        UnityMolMain.recordUndoPythonCommand("setBondOrderTexture(\"" + selName + "\", 0)");
    }
    /// <summary>
    /// Remove Ambient Occlusion from hyperball representation in a given selection
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    public static void clearHyperballAO(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

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
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("clearHyperballAO(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Compute object space Ambient Occlusion for surface representations of a given selection.
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    public static void computeSurfaceAO(string selName) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            RepType repType = getRepType("s");
            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolSurfaceManager sMana = (UnityMolSurfaceManager) sr.atomRepManager;
                        sMana.DoAO();
                    }
                }
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }
        UnityMolMain.recordPythonCommand("computeSurfaceAO(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("clearSurfaceAO(\"" + selName + "\")");
    }

    /// <summary>
    /// Remove Ambient Occlusion for surface representations of a given selection.
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    public static void clearSurfaceAO(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType("s");
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            UnityMolSurfaceManager sMana = (UnityMolSurfaceManager) sr.atomRepManager;
                            sMana.ClearAO();
                        }
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("clearSurfaceAO(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("computeSurfaceAO(\"" + selName + "\")");
    }

    /// <summary>
    /// Is Ambient Occlusion for surface representations of a given selection activate or not?
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <returns>True if AO activated. False otherwise</returns>
    public static bool isSurfaceAOOn(string selName) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType("s");
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            UnityMolSurfaceManager sMana = (UnityMolSurfaceManager) sr.atomRepManager;
                            return sMana.AOOn;
                        }
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return false;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
        }
        return false;
    }

    /// <summary>
    /// Set the global ambient light intensity
    /// </summary>
    /// <param name="ambientLightValue">the new ambient light intensity</param>
    public static void setAmbientLightIntensity(float ambientLightValue) {
        float prevI = UnityMolMain.ambientLightScale;
        UnityMolMain.ambientLightScale = ambientLightValue;

        RenderSettings.ambientLight = (UnityMolMain.initAmbientColor * ambientLightValue);

        if (UnityMolMain.raytracingMode) {
            RaytracerManager.Instance.setAmbientLight(ambientLightValue * 0.2f);
        }

        const int lenSameCom = 25;
        bool replaced = UnityMolMain.recordPythonCommand(string.Format(CultureInfo.InvariantCulture, "setAmbientLightIntensity({0})", ambientLightValue), true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand(string.Format(CultureInfo.InvariantCulture, "setAmbientLightIntensity({0})", prevI), replaced);
    }

    /// <summary>
    /// Set light intensity of all directional lights found in the scene
    /// </summary>
    /// <param name="lightIntensity">the new light intensity</param>
    public static void setDirLightIntensity(float lightIntensity) {
        Light[] lights = FindObjectsOfType<Light>();
        float prevI = 1.0f;
        foreach (Light l in lights) {
            if (l.type == LightType.Directional) {
                prevI = l.intensity;
                l.intensity = lightIntensity;
            }
        }
        const int lenSameCom = 21;
        bool replaced = UnityMolMain.recordPythonCommand(string.Format(CultureInfo.InvariantCulture, "setDirLightIntensity({0})", lightIntensity), true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand(string.Format(CultureInfo.InvariantCulture, "setDirLightIntensity({0})", prevI), replaced);
    }

    /// <summary>
    /// Set light shadow strength of all directional lights found in the scene
    /// 0 is no shadow at all, 1 is full black shadow
    /// </summary>
    /// <param name="lightShadow">the new light shadow</param>
    public static void setDirLightShadow(float lightShadow) {
        Light[] lights = FindObjectsOfType<Light>();
        float prevI = 1.0f;
        foreach (Light l in lights) {
            if (l.type == LightType.Directional) {
                prevI = l.shadowStrength;
                l.shadowStrength = lightShadow;
            }
        }
        const int lenSameCom = 18;
        bool replaced = UnityMolMain.recordPythonCommand(string.Format(CultureInfo.InvariantCulture, "setDirLightShadow({0})", lightShadow), true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand(string.Format(CultureInfo.InvariantCulture, "setDirLightShadow({0})", prevI), replaced);
    }

    /// <summary>
    /// Set light direction based on eulers for all directional lights found in the scene
    /// </summary>
    /// <param name="eulers">The light direction</param>
    public static void setDirLightDirection(Vector3 eulers) {
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light l in lights) {
            if (l.type == LightType.Directional) {
                l.transform.rotation = Quaternion.Euler(eulers);
            }
        }

        const int lenSameCom = 21;
        bool replaced = UnityMolMain.recordPythonCommand(string.Format(CultureInfo.InvariantCulture, "setDirLightDirection({0})", eulers), true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("", replaced);
    }

    ///<summary>
    /// Set light color of all directional lights found in the scene
    ///</summary>
    /// <param name="c">the new light color</param>
    public static void setDirLightColor(Color c) {
        Light[] lights = FindObjectsOfType<Light>();
        Color prevCol = Color.white;
        foreach (Light l in lights) {
            if (l.type == LightType.Directional) {
                prevCol = l.color;
                l.color = c;
            }
        }
        const int lenSameCom = 17;
        bool replaced = UnityMolMain.recordPythonCommand(string.Format(CultureInfo.InvariantCulture, "setDirLightColor({0})", c), true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand(string.Format(CultureInfo.InvariantCulture, "setDirLightColor({0})", prevCol), replaced);
    }


    /// <summary>
    /// Set the color of the cartoon representation of the specified selection based on the nature of secondary structure assigned
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="ssType">Cartoon representation type: "helix", "sheet" or "coil"</param>
    /// <param name="col">the new color</param>
    public static void setCartoonColorSS(string selName, string ssType, Color col) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();
        Color32 newCol = col;

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
                                MDAnalysisSelection selec = new("ss " + ssType, sel.atoms);
                                UnityMolSelection selSS = selec.process();
                                cManager.SetColor(newCol, selSS);
                            }
                            if (ssType == "sheet") {
                                MDAnalysisSelection selec = new("ss " + ssType, sel.atoms);
                                UnityMolSelection selSS = selec.process();
                                cManager.SetColor(newCol, selSS);
                            }
                            if (ssType == "coil") {
                                MDAnalysisSelection selec = new("ss " + ssType, sel.atoms);
                                UnityMolSelection selSS = selec.process();
                                cManager.SetColor(newCol, selSS);
                            }
                            sr.atomRep.colorationType = colorType.custom;
                        }
                    }
                }
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        int lenSameCom = 19 + selName.Length + 4 + ssType.Length + 2;
        bool replaced = UnityMolMain.recordPythonCommand(string.Format(CultureInfo.InvariantCulture, "setCartoonColorSS(\"{0}\", \"{1}\", {2})", selName, ssType, newCol), true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("", replaced);
    }


    /// <summary>
    /// Change the size of the representation of type 'type' in the selection for each atom.
    /// The parameters sizes is a list new values for each atom of the selection.
    /// <remarks>Mainly used for hyperball representation</remarks>
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    /// <param name="sizes">the new size for each atom of the representation</param>
    public static void setRepSizes(string selName, string type, List<float> sizes) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            if (sel.atoms.Count != sizes.Count) {
                Debug.LogError("Length of the 'sizes' parameter does not have the length of the selection");
                return;
            }

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            if (sr.atomRepManager != null) {
                                sr.atomRepManager.SetSizes(sel.atoms, sizes);
                            }
                            if (sr.bondRepManager != null) {
                                sr.bondRepManager.SetSizes(sel.atoms, sizes);
                            }
                        }
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        int lenSameCom = 12 + selName.Length + 4 + type.Length + 2;
        string command = "setRepSizes(\"" + selName + "\", \"" + type + "\", [";

        for (int i = 0; i < sizes.Count; i++) {
            command += sizes[i].ToString(culture);
            if (i != sizes.Count - 1) {
                command += ", ";
            }
        }
        command += "])";
        bool replaced = UnityMolMain.recordPythonCommand(command, true, lenSameCom);

        UnityMolMain.recordUndoPythonCommand("setRepSizes(\"" + selName + "\", \"" + type + "\", 1.0)", replaced);
    }

    /// <summary>
    /// Change the size of the representation of type 'type' in the selection
    /// <remarks>Mainly used for hyperball representation</remarks>
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond","point"/"p"
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    /// <param name="size">the new size of the representation</param>
    public static void setRepSize(string selName, string type, float size) {


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
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        int lenSameCom = 12 + selName.Length + 4 + type.Length + 2;
        bool replaced = UnityMolMain.recordPythonCommand("setRepSize(\"" + selName + "\", \"" + type + "\", " + size.ToString("f2", culture) + ")", true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("setRepSize(\"" + selName + "\", \"" + type + "\", 1.0)", replaced);

    }

    /// <summary>
    ///  Change the color of all representation of type 'type' in the selection
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    /// <param name="col">the new color as a Color object for the representations</param>
    public static void colorSelection(string selName, string type, Color col) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();
        Color32 newCol = col;

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            if (sr.atomRepManager != null) {
                                sr.atomRepManager.SetColor(newCol, sel);
                                sr.atomRep.colorationType = colorType.full;
                            }
                            if (sr.bondRepManager != null) {
                                sr.bondRepManager.SetColor(newCol, sel);
                                sr.bondRep.colorationType = colorType.full;
                            }
                        }
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        string command = string.Format(CultureInfo.InvariantCulture, "colorSelection(\"{0}\", \"{1}\", {2})", selName, type, col);
        int lenSameCom = command.Length - (string.Format("{0}", newCol).Length + 2);
        bool replaced = UnityMolMain.recordPythonCommand(command, true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("", replaced);

    }

    /// <summary>
    ///  Change the color of all representation of type 'type' in the selection
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// colorS can be "black", "white", "yellow", "green", "red", "blue", "pink", "gray"
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    /// <param name="colorS">the new color as a string for the representations</param>
    public static void colorSelection(string selName, string type, string colorS) {

        colorS = colorS.ToLower();
        Color col = strToColor(colorS);
        colorSelection(selName, type, col);

        UnityMolMain.recordPythonCommand("colorSelection(\"" + selName + "\", \"" + type + "\", \"" + colorS + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    ///  Change the color of all representation of type 'type' in the selection
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// colors is a list of colors for each atom of the selection
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    /// <param name="colors">the new color for each atom of the representation</param>
    public static void colorSelection(string selName, string type, List<Color32> colors) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];
            if (sel.atoms.Count != colors.Count) {
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
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        string command = "colorSelection(\"" + selName + "\", \"" + type + "\", [";
        for (int i = 0; i < colors.Count; i++) {
            Color col = colors[i];
            command += "Color(" + col.r.ToString("F3", culture) + ", " +
                       col.g.ToString("F3", culture) + ", " +
                       col.b.ToString("F3", culture) + ", " +
                       col.a.ToString("F3", culture) + ")";
            if (i != colors.Count - 1) {
                command += ", ";
            }
        }
        command += "])";

        UnityMolMain.recordPythonCommand(command);
        UnityMolMain.recordUndoPythonCommand("");
    }


    /// <summary>
    /// Change the color of all representations in a selection
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="col">The new color</param>
    public static void colorSelection(string selName, Color col) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        Color32 newCol = col;

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            foreach (List<UnityMolRepresentation> reps in sel.representations.Values) {
                foreach (UnityMolRepresentation r in reps) {
                    foreach (SubRepresentation sr in r.subReps) {
                        if (sr.atomRepManager != null) {
                            sr.atomRepManager.SetColor(newCol, sel);
                            sr.atomRep.colorationType = colorType.full;
                        }
                        if (sr.bondRepManager != null) {
                            sr.bondRepManager.SetColor(newCol, sel);
                            sr.bondRep.colorationType = colorType.full;
                        }
                    }
                }
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        string command = string.Format(CultureInfo.InvariantCulture, "colorSelection(\"{0}\", {1})", selName, newCol);
        int lenSameCom = command.Length - (string.Format("{0}", col).Length + 2);
        bool replaced = UnityMolMain.recordPythonCommand(command, true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("", replaced);
    }

    /// <summary>
    /// From a global selection name,
    /// change the color of all atoms selected on the selection query "selQuery" in the representation 'type'.
    /// If 'type' is not specified, change the color for all representations concerned.
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="col">the new color</param>
    /// <param name="selQuery">the selection query</param>
    /// <param name="type">the type of representation to modify color. "" means all representations</param>
    public static void colorSelection(string selName, Color col, string selQuery, string type = "") {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();
        Color32 newCol = col;

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            MDAnalysisSelection selec = new(selQuery, sel.atoms);
            UnityMolSelection selRes = selec.process();


            if (type == "") {//No rep type specified => color all the representations of the selection
                foreach (List<UnityMolRepresentation> reps in sel.representations.Values) {
                    foreach (UnityMolRepresentation r in reps) {
                        foreach (SubRepresentation sr in r.subReps) {
                            if (sr.atomRepManager != null) {
                                sr.atomRepManager.SetColor(newCol, selRes);
                                sr.atomRep.colorationType = colorType.custom;
                            }
                            if (sr.bondRepManager != null) {
                                sr.bondRepManager.SetColor(newCol, selRes);
                                sr.bondRep.colorationType = colorType.custom;
                            }
                        }
                    }
                }
            }
            else {
                RepType repType = getRepType(type);
                if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                    List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                    if (existingReps != null) {
                        foreach (UnityMolRepresentation existingRep in existingReps) {
                            foreach (SubRepresentation sr in existingRep.subReps) {
                                if (sr.atomRepManager != null) {
                                    sr.atomRepManager.SetColor(newCol, selRes);
                                    sr.atomRep.colorationType = colorType.custom;
                                }
                                if (sr.bondRepManager != null) {
                                    sr.bondRepManager.SetColor(newCol, selRes);
                                    sr.bondRep.colorationType = colorType.custom;
                                }
                            }
                        }
                    }
                } else {
                    Debug.LogError("Wrong representation type");
                    return;
                }
            }
        }
        else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        string command = string.Format(CultureInfo.InvariantCulture, "colorSelection(\"{0}\", {1}, \"{2}\", \"{3}\")", selName, newCol, selQuery, type);
        int lenSameCom = 15 + 2 + selName.Length;
        bool replaced = UnityMolMain.recordPythonCommand(command, true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("", replaced);
    }

    /// <summary>
    /// Reset the color of all representations of type 'type' in selection to the default value
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">the type of representation to modify color. "" means all representations</param>
    public static void resetColorSelection(string selName, string type) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
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
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("resetColorSelection(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("");

    }

    /// <summary>
    /// Change the color of the atom type 'atomType' in the representation 'type' in the given selection
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    /// <param name="atomType">the type of atoms to change the color</param>
    /// <param name="col">the new color</param>
    public static void colorAtomType(string selName, string type, string atomType, Color col) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();
        Color32 newCol = col;

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            RepType rt = getRepType(type);
            if (rt.atomType != AtomType.noatom || rt.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, rt);
                if (existingReps != null) {

                    //Use MDASelection to benefit from the wildcard
                    MDAnalysisSelection selec = new MDAnalysisSelection("type " + atomType, sel.atoms);
                    UnityMolSelection selRes = selec.process();


                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            if (sr.atomRepManager != null) {
                                sr.atomRepManager.SetColors(newCol, selRes.atoms);
                            }
                            if (sr.bondRepManager != null) {
                                sr.bondRepManager.SetColors(newCol, selRes.atoms);
                            }
                        }
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }
        UnityMolMain.recordPythonCommand("colorAtomType(\"" + selName + "\", \"" + type + "\", \"" + atomType + "\", " +
                                         string.Format(CultureInfo.InvariantCulture, "{0})", newCol));
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Use the color palette to color by chain the representation of type 'type' in the selection 'selName'
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    public static void colorByChain(string selName, string type) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.ColorByChain();
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("colorByChain(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Use the color palette to color by residue the representation of type 'type' in the selection 'selName'
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    public static void colorByResidue(string selName, string type) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.ColorByResidue();
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("colorByResidue(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("");

    }

    /// <summary>
    /// Use the color palette to color by atom the representation of type 'type' in the selection 'selName'
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    public static void colorByAtom(string selName, string type) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.ColorByAtom();
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("colorByAtom(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Use the color palette to color by hydrophobicity the representation of type 'type' in the selection 'selName'
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    public static void colorByHydrophobicity(string selName, string type) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.ColorByHydro();
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("colorByHydrophobicity(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Use the color palette to color by residue ID the representation of type 'type' in the selection 'selName'
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    public static void colorByResid(string selName, string type) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.ColorByResid();
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("colorByResid(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("");

    }

    /// <summary>
    /// Use the color palette to color by residue number representation of type 'type' in the selection 'selName'
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    public static void colorByResnum(string selName, string type) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.ColorByResnum();
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("colorByResnum(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("");

    }

    /// <summary>
    /// Use the color palette to color by sequence (rainbow effect) representation of type 'type' in the selection 'selName'
    /// type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    public static void colorBySequence(string selName, string type) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.ColorBySequence();
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("colorBySequence(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Use the DX map to color by charge around atoms.
    /// <remarks>Only works for surface for now</remarks>
    /// If 'normalizeDensity' is set to true, the density values will be normalized and 'minDens' & 'maxDens' are ignored.
    /// If 'normalizeDensity' is set to false, 'minDens' & 'maxDens' are used.
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="normalizeDensity">Normalize the density?</param>
    /// <param name="minDens">manual minimum value of the density</param>
    /// <param name="maxDens">manual maximum value of the density</param>
    public static void colorByCharge(string selName, bool normalizeDensity = false, float minDens = -10.0f, float maxDens = 10.0f) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType("s");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolSurfaceManager surfM = (UnityMolSurfaceManager) sr.atomRepManager;
                        surfM.ColorByCharge(normalizeDensity, minDens, maxDens);
                    }
                }
            }
            repType = getRepType("dxiso");

            existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolSurfaceManager surfM = (UnityMolSurfaceManager) sr.atomRepManager;
                        surfM.ColorByCharge(normalizeDensity, minDens, maxDens);
                    }
                }
            }
        } else {
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
    /// Use the color palette to color by residue type the representation of type 'type' in the selection 'selName'
    /// Colors: negatively charge = red, positively charged = blue, nonpolar = light yellow, polar = green, cys = orange
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    public static void colorByResidueType(string selName, string type) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.ColorByResType();
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("colorByResidueType(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Use the color palette to color by residue charge the representation of type 'type' in the selection 'selName'
    /// Colors: negatively charge = red, positively charged = blue, neutral = white
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    public static void colorByResidueCharge(string selName, string type) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.ColorByResCharge();
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("colorByResidueCharge(\"" + selName + "\", \"" + type + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Color residues by B-factor the representation of type 'type' in the selection 'selName'
    /// Uses a linear interpolation between 'startColor', 'midColor' & 'endColor' to cover all B-factor values from low to high.
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    /// <param name="startColor"></param>
    /// <param name="midColor"></param>
    /// <param name="endColor"></param>
    public static void colorByBfactor(string selName, string type, Color startColor, Color midColor, Color endColor) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.ColorByBfactor(startColor, midColor, endColor);
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("colorByBfactor(\"" + selName + "\", \"" + type + "\", " +
                                         string.Format(CultureInfo.InvariantCulture, " {0}, {1}, {2})", startColor, midColor, endColor));
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Color residues by B-factor the representation of type 'type' in the selection 'selName'
    /// Uses linear interpolation between low values (blue), medium values (yellow) & high values (red)
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    public static void colorByBfactor(string selName, string type) {
        colorByBfactor(selName, type, Color.blue, Color.yellow, Color.red);
    }

    /// <summary>
    /// Set the size of the line representation in a given selection
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="val">The new size</param>
    public static void setLineSize(string selName, float val) {

        RepType repType = getRepType("l");

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolBondLineManager lM = (UnityMolBondLineManager) sr.bondRepManager;
                        lM.SetWidth(val);
                    }
                }
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        int lenSameCom = 13 + selName.Length + 2;
        bool replaced = UnityMolMain.recordPythonCommand("setLineSize(\"" + selName + "\", " + val.ToString("F3", culture) + ")", true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("", replaced);
    }

    /// <summary>
    /// Set the size of the trace representation in a given selection
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="val">The new size</param>
    public static void setTraceSize(string selName, float val) {

        RepType repType = getRepType("trace");

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolTubeManager tM = (UnityMolTubeManager) sr.atomRepManager;
                        tM.SetWidth(val);
                    }
                }
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        int lenSameCom = 14 + selName.Length + 2;
        bool replaced = UnityMolMain.recordPythonCommand("setTraceSize(\"" + selName + "\", " + val.ToString("F3", culture) + ")", true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("", replaced);
    }

    /// <summary>
    /// Change sheherasade computation method in a given selection
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    public static void switchSheherasadeMethod(string selName) {
        RepType repType = getRepType("sheherasade");

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolSheherasadeManager sM = (UnityMolSheherasadeManager) sr.atomRepManager;
                        sM.SetSheherasadeForm(!sM.atomRep.bezier);
                    }
                }
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("switchSheherasadeMethod(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("switchSheherasadeMethod(\"" + selName + "\")");
    }

    /// <summary>
    /// Change all sheherasade representations in the selection with a new texture
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="idTex">Texture Index in UnityMolMain.atomColors.textures</param>
    public static void setSheherasadeTexture(string selName, int idTex) {
        RepType repType = getRepType("sheherasade");

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolSheherasadeManager sM = (UnityMolSheherasadeManager) sr.atomRepManager;
                        if (idTex >= 0) {
                            sM.SetTexture(Sheherasade.arrowTexture);
                        } else {
                            sM.SetTexture(null);
                        }
                    }
                }
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }

        UnityMolMain.recordPythonCommand("setSheherasadeTexture(\"" + selName + "\", " + idTex + ")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Center the structure 'structureName' by offsets all representations
    /// Instead of moving the camera, move the loaded molecules GO to center them in the center of the camera
    /// </summary>
    /// <param name="structureName">The name of the structure concerned</param>
    /// <param name="lerp">Center with a linear interpolation?</param>
    /// <param name="recordCommand">Record this command in the history?</param>
    public static void centerOnStructure(string structureName, bool lerp = false, bool recordCommand = true) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }
        UnityMolStructure s;

        if (sm.nameToStructure.ContainsKey(structureName)) {
            s = sm.GetStructure(structureName);
        } else {
            Debug.LogWarning("No molecule loaded name '" + structureName + "'");
            return;
        }

        ManipulationManager mm = getManipulationManager();

        if (mm) {
            mm.centerOnStructure(s, lerp);
        }

        if (!recordCommand) {
            return;
        }

        UnityMolMain.recordPythonCommand("centerOnStructure(\"" + structureName + "\", " + cBoolToPy(lerp) + ")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Get the current ManipulationManager, creates one if there is none
    /// </summary>
    /// <returns>the ManipulationManager</returns>
    public static ManipulationManager getManipulationManager() {
        ManipulationManager[] foundObjects = FindObjectsOfType<ManipulationManager>();
        ManipulationManager mm;

        if (foundObjects.Length > 0) {
            mm = foundObjects[0].GetComponent<ManipulationManager>();
        } else {
            mm = UnityMolMain.getRepresentationParent().AddComponent<ManipulationManager>();
        }
        return mm;
    }

    /// <summary>
    /// Center the selections 'selName' by offsets all representations
    /// If lerp is true and duration is > 0, centering is done during 'duration' seconds
    /// Fit the selection in the camera field of view if distance is negative, otherwise the molecule will be placed at "distance" from the camera
    /// </summary>
    /// <param name="selName">The name of the structure concerned</param>
    /// <param name="lerp">Center with a linear interpolation?</param>
    /// <param name="distance">distance of the selection from the camera</param>
    /// <param name="duration">Duration of the centering with interpolation</param>
    public static void centerOnSelection(string selName, bool lerp = false, float distance = -1.0f, float duration = 0.25f) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        if (!selM.selections.ContainsKey(selName)) {
            Debug.LogError("Selection '" + selName + "' does not exist");
            return;
        }
        UnityMolSelection sel = selM.selections[selName];

        ManipulationManager mm = getManipulationManager();

        if (mm != null) {
            mm.centerOnSelection(sel, lerp, distance, duration);
        }

        UnityMolMain.recordPythonCommand("centerOnSelection(\"" + selName + "\", " + cBoolToPy(lerp) + ", " + distance.ToString("F4", culture) + ", " + duration.ToString("F4", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("");
    }


    /// <summary>
    /// Use CEAlign algorithm to align two selections (usually molecules), uses only C-alpha atoms
    /// <remarks>For more details: https://pymolwiki.org/index.php/Cealign</remarks>
    /// </summary>
    /// <param name="selNameTarget">the reference selection</param>
    /// <param name="selNameMobile">the target selection</param>
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
    /// <param name="selMDA">the selection query</param>
    /// <param name="name">the name of the selection.</param>
    /// <param name="createSelection">Add the selection to the manager?</param>
    /// <param name="addToExisting">add the atoms of this query to an existing selection if present?</param>
    /// <param name="silent">Print the new selection in the console</param>
    /// <param name="setAsCurrentSelection">Make the selection the current one</param>
    /// <param name="forceCreate">Create the selection even if it's empty</param>
    /// <param name="allModels">apply the selection to all models of structures concerned?</param>
    /// <param name="addToSelectionKeyword">add this selection as a keyword?</param>
    /// <returns>the UnityMolSelection object</returns>
    public static UnityMolSelection select(string selMDA, string name = "selection", bool createSelection = true,
        bool addToExisting = false, bool silent = false, bool setAsCurrentSelection = true, bool forceCreate = false,
        bool allModels = false, bool addToSelectionKeyword = true) {

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
                selM.selections[name].atoms = null; //Not actually changing the selection
                return selM.selections[name];
            }
        }
        if (!addToExisting && selM.selections.ContainsKey(name)) {
            selM.Delete(name);
        }

        if (setAsCurrentSelection) {
            selM.ClearCurrentSelection();
        }

        List<UnityMolAtom> allAtoms = new();

        if (allModels) {
            foreach (UnityMolStructure s in sm.loadedStructures) {
                foreach (UnityMolModel m in s.models) {
                    allAtoms.AddRange(m.allAtoms);
                }
            }
        } else {
            foreach (UnityMolStructure s in sm.loadedStructures) {
                allAtoms.AddRange(s.currentModel.allAtoms);
            }
        }

        MDAnalysisSelection selec = new(selMDA, allAtoms);
        UnityMolSelection newContent = selec.process();
        newContent.name = name;

        if (addToExisting && selM.selections.ContainsKey(name)) {
            //TODO Use selectionmanager updateSelectionsWithMDA

            List<UnityMolAtom> newListAtoms = selM.selections[name].atoms;
            newListAtoms.AddRange(newContent.atoms);
            newListAtoms = newListAtoms.Distinct().ToList();

            selM.selections[name].atoms = newListAtoms;
            if (!selM.selections[name].bondsNull) {
                selM.selections[name].fillBonds();
            }
            selM.selections[name].fillStructures();

            if (selM.selections[name].MDASelString.Length + selMDA.Length > limitSizeSelectionString) {
                selM.selections[name].MDASelString = selM.selections[name].ToSelectionCommand(true);
            } else {
                if (selM.selections[name].MDASelString == "nothing") {
                    selM.selections[name].MDASelString = selMDA;
                } else {
                    selM.selections[name].MDASelString = "(" + selM.selections[name].MDASelString + ") or (" + selMDA + ")";
                }
            }

            if (createSelection && setAsCurrentSelection) {
                selM.SetCurrentSelection(selM.selections[name]);
            }

            if (addToSelectionKeyword) {
                selM.AddSelectionKeyword(name, name);
            }

            UnityMolSelectionManager.launchSelectionModified();//Start the event

            // Debug.LogWarning("Adding to existing selection: " + result);
            UnityMolMain.recordPythonCommand("select(\"" + selMDA + "\", \"" + name + "\", " +
                                             cBoolToPy(createSelection) + ", " + cBoolToPy(true) + ", " + cBoolToPy(silent) + ", " +
                                             cBoolToPy(setAsCurrentSelection) + ", " + cBoolToPy(forceCreate) + ", " + cBoolToPy(allModels) + ", " +
                                             cBoolToPy(addToSelectionKeyword) + ")");
            UnityMolMain.recordUndoPythonCommand("deleteSelection(\"" + name + "\")");
            return selM.selections[name];
        }

        //Should I record the selection
        if (forceCreate || (newContent.atoms.Count != 0 && createSelection)) {
            if (setAsCurrentSelection) {
                selM.SetCurrentSelection(newContent);
            } else {
                selM.Add(newContent);
            }

            if (addToSelectionKeyword) {
                selM.AddSelectionKeyword(name, name);
            }
        }

        UnityMolMain.recordPythonCommand("select(\"" + selMDA + "\", \"" + name + "\", " +
                                         cBoolToPy(createSelection) + ", " + cBoolToPy(addToExisting) + ", " + cBoolToPy(silent) + ", " +
                                         cBoolToPy(setAsCurrentSelection) + ", " + cBoolToPy(forceCreate) + ", " + cBoolToPy(allModels) + ", " +
                                         cBoolToPy(addToSelectionKeyword) + ")");

        UnityMolMain.recordUndoPythonCommand("deleteSelection(\"" + newContent.name + "\")");
        if (!silent) {
            Debug.Log(newContent);
        }
        return newContent;
    }

    /// <summary>
    /// Add a keyword to the selection language for the selection 'selName'
    /// </summary>
    /// <param name="keyword">the new keyword</param>
    /// <param name="selName">the selection name concerned.</param>
    public static void addSelectionKeyword(string keyword, string selName) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        selM.AddSelectionKeyword(keyword, selName);
        UnityMolMain.recordPythonCommand("addSelectionKeyword(\"" + keyword + "\", \"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("removeSelectionKeyword(\"" + keyword + "\", \"" + selName + "\")");
    }

    /// <summary>
    /// Remove a keyword to the selection language for the selection 'selName'
    /// </summary>
    /// <param name="keyword">the keyword to remove</param>
    /// <param name="selName">the selection name concerned.</param>
    public static void removeSelectionKeyword(string keyword, string selName) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        selM.RemoveSelectionKeyword(keyword);
        UnityMolMain.recordPythonCommand("removeSelectionKeyword(\"" + keyword + "\", \"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("addSelectionKeyword(\"" + keyword + "\", \"" + selName + "\")");
    }

    /// <summary>
    /// Set the selection as the current selection in the UnityMolSelectionManager
    /// </summary>
    /// <param name="selName">the selection name concerned.</param>
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
    /// <param name="selMDA">the selection query</param>
    /// <param name="name">the name of the selection.</param>
    /// <param name="silent">Print the new selection in the console</param>
    /// <param name="allModels">apply the selection to all models of structures concerned?</param>
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

        //Get all necessary atoms to execute the selection query
        List<UnityMolAtom> allAtoms = new();

        if (allModels) {
            foreach (UnityMolStructure s in sm.loadedStructures) {
                foreach (UnityMolModel m in s.models) {
                    allAtoms.AddRange(m.allAtoms);
                }
            }
        } else {
            foreach (UnityMolStructure s in sm.loadedStructures) {
                allAtoms.AddRange(s.currentModel.allAtoms);
            }
        }

        //Process the selection
        MDAnalysisSelection selec = new(selMDA, allAtoms);
        UnityMolSelection ret = selec.process();

        List<UnityMolAtom> newAtomList = selM.selections[name].atoms;
        newAtomList.AddRange(ret.atoms);

        newAtomList = newAtomList.Distinct().ToList();

        //Add atoms of the selection to the existing one
        selM.selections[name].atoms = newAtomList;
        if (!selM.selections[name].bondsNull) {
            selM.selections[name].fillBonds();
        }
        selM.selections[name].fillStructures();

        if (selM.selections[name].MDASelString.Length + selMDA.Length > limitSizeSelectionString) {
            selM.selections[name].MDASelString = selM.selections[name].ToSelectionCommand(true);
        } else {
            if (selM.selections[name].MDASelString == "nothing") {
                selM.selections[name].MDASelString = selMDA;
            } else {
                selM.selections[name].MDASelString = "(" + selM.selections[name].MDASelString + ") or (" + selMDA + ")";
            }
        }

        UnityMolSelectionManager.launchSelectionModified();//Start the event

#if !DISABLE_HIGHLIGHT
        UnityMolHighlightManager hM = UnityMolMain.getHighlightManager();
        hM.HighlightAtoms(selM.selections[name]);
#endif

        UnityMolMain.recordPythonCommand("addToSelection(\"" + selMDA + "\", \"" + name + "\", " + cBoolToPy(silent) + ", " + cBoolToPy(allModels) + ")");
        UnityMolMain.recordUndoPythonCommand("");

        if (!silent) {
            Debug.Log(selM.selections[name]);
        }
    }

    /// <summary>
    /// Look for an existing selection named 'name' and remove atoms to it based on MDAnalysis selection language
    /// </summary>
    /// <param name="selMDA">the selection query</param>
    /// <param name="name">the name of the selection.</param>
    /// <param name="silent">Print the new selection in the console</param>
    /// <param name="allModels">apply the selection to all models of structures concerned?</param>
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
            selM.selections[name].atoms = null; //Not actually changing the selection
            return;
        }

        List<UnityMolAtom> allAtoms = new();

        if (allModels) {
            foreach (UnityMolStructure s in sm.loadedStructures) {
                foreach (UnityMolModel m in s.models) {
                    allAtoms.AddRange(m.allAtoms);
                }
            }
        } else {
            foreach (UnityMolStructure s in sm.loadedStructures) {
                allAtoms.AddRange(s.currentModel.allAtoms);
            }
        }

        MDAnalysisSelection selec = new(selMDA, allAtoms);
        UnityMolSelection ret = selec.process();

        //Remove atoms of the selection to the existing one
        List<UnityMolAtom> newListAtoms = selM.selections[name].atoms;
        foreach (UnityMolAtom a in ret.atoms) {
            if (newListAtoms.Contains(a)) {
                newListAtoms.Remove(a);
            }
        }

        selM.selections[name].atoms = newListAtoms;
        selM.selections[name].fillStructures();
        if (!selM.selections[name].bondsNull) {
            selM.selections[name].fillBonds();
        }

        selM.selections[name].MDASelString = selM.selections[name].ToSelectionCommand(true);

#if !DISABLE_HIGHLIGHT
        UnityMolHighlightManager hM = UnityMolMain.getHighlightManager();
        hM.HighlightAtoms(selM.selections[name]);
#endif

        UnityMolSelectionManager.launchSelectionModified();//Start the event

        UnityMolMain.recordPythonCommand("removeFromSelection(\"" + selMDA + "\", \"" + name + "\", " + cBoolToPy(silent) + ", " + cBoolToPy(allModels) + ")");
        UnityMolMain.recordUndoPythonCommand("");

        if (!silent) {
            Debug.Log(selM.selections[name]);
        }
    }

    /// <summary>
    /// Delete selection 'selName' and all its representations
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
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
    /// Duplicate selection 'selName' without the representations
    /// The duplicated selection  will have the same name of the original one + a suffix number
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <returns>The new name of the duplicated selection</returns>
    public static string duplicateSelection(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (!selM.selections.ContainsKey(selName)) {
            Debug.LogError("Cannot delete selection '" + selName + "' as it does not exist");
            return "";
        }

        string newSelName = selM.findNewSelectionName(selName);

        UnityMolSelection sel = selM.selections[selName];
        UnityMolSelection newSel = new(sel.atoms, sel.bonds, newSelName, sel.MDASelString)
        {
            forceGlobalSelection = sel.forceGlobalSelection,
            isAlterable = true
        };

        selM.Add(newSel);
        selM.AddSelectionKeyword(newSelName, newSelName);

        UnityMolMain.recordPythonCommand("duplicateSelection(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("deleteSelection(\"" + newSelName + "\")");
        return newSelName;
    }

    /// <summary>
    /// Change the selection named 'oldSelName' to 'newSelName'
    /// </summary>
    /// <param name="oldSelName">the current name of the selection</param>
    /// <param name="newSelName">the new name of the selection</param>
    /// <returns>True if success. False otherwise</returns>
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
    /// <param name="selName">The selection name concerned</param>
    /// <param name="selectionString">the selection query</param>
    /// <param name="forceAlteration">make the selection alterable</param>
    /// <param name="silent">don't print log information.</param>
    /// <param name="recordCommand">record this command to the history?</param>
    /// <param name="allModels">apply the modification to all models of the selection?</param>
    /// <returns>True if success. False otherwise</returns>
    public static bool updateSelectionWithMDA(string selName, string selectionString, bool forceAlteration,
        bool silent = false, bool recordCommand = true, bool allModels = false) {

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

            UnityMolSelection result;
            List<UnityMolAtom> allAtoms = new();

            if (sel.forceGlobalSelection || sel.structures == null || sel.structures.Count == 0) {

                if (allModels) {
                    foreach (UnityMolStructure s in sm.loadedStructures) {
                        foreach (UnityMolModel m in s.models) {
                            allAtoms.AddRange(m.allAtoms);
                        }
                    }
                } else {
                    foreach (UnityMolStructure s in sm.loadedStructures) {
                        allAtoms.AddRange(s.currentModel.allAtoms);
                    }
                }
                MDAnalysisSelection selec = new(selectionString, allAtoms);
                result = selec.process();
            } else {
                if (allModels) {
                    foreach (UnityMolStructure s in sel.structures) {
                        foreach (UnityMolModel m in s.models) {
                            allAtoms.AddRange(m.allAtoms);
                        }
                    }
                } else {
                    foreach (UnityMolStructure s in sel.structures) {
                        allAtoms.AddRange(s.currentModel.allAtoms);
                    }
                }

                MDAnalysisSelection selec = new(selectionString, allAtoms);
                result = selec.process();

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
            if (!sel.bondsNull) {
                sel.bonds = result.bonds;
            }

            sel.fromSelectionLanguage = true;
            sel.MDASelString = result.MDASelString;
            sel.fillStructures();

            UnityMolSelectionManager.launchSelectionModified();//Start the event

            selM.selections[selName] = sel;

            if (!silent) {
                Debug.LogWarning("Modified the selection '" + selName + "' now with " + sel.Count + " atoms");
            }


#if !DISABLE_HIGHLIGHT
            if (selM.currentSelection != null && selM.currentSelection.name == selName) {
                UnityMolHighlightManager hM = UnityMolMain.getHighlightManager();
                hM.HighlightAtoms(selM.selections[selName]);
            }
#endif

            updateRepresentations(selName);

            if (forceAlteration) {
                sel.isAlterable = saveAlte;
            }

        } catch (Exception e) {
            Debug.LogError("Failed to update the selection: " + e);
            return false;
        }

        if (recordCommand) {
            UnityMolMain.recordPythonCommand("updateSelectionWithMDA(\"" + selName + "\", \"" + selectionString + "\", " +
                                             cBoolToPy(forceAlteration) + ", " + cBoolToPy(silent) + ", " + cBoolToPy(allModels) + ")");
            UnityMolMain.recordUndoPythonCommand("");
        }

        return true;
    }

    /// <summary>
    /// Directly clear the highlight manager, this does not unselect the current selection
    /// </summary>
    public static void cleanHighlight() {
        UnityMolHighlightManager hM = UnityMolMain.getHighlightManager();

        hM.Clean();
        UnityMolMain.recordPythonCommand("cleanHighlight()");
        UnityMolMain.recordUndoPythonCommand("");
    }


    /// <summary>
    /// Select atoms of all loaded molecules inside a sphere defined by a molecular space position and a radius in Angstrom
    /// and create a new selection from it.
    /// </summary>
    /// <param name="position">center of the sphere</param>
    /// <param name="radius">radius of the sphere</param>
    /// <returns>the new UnityMolSelection object</returns>
    public static UnityMolSelection selectInSphere(Vector3 position, float radius) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return null;
        }

        string selName = "InSphere";

        selM.ClearCurrentSelection();

        Vector3 sphereCenter = position;

        List<UnityMolAtom> allAtoms = new();

        foreach (UnityMolStructure s in sm.loadedStructures) {
            allAtoms.AddRange(s.currentModel.allAtoms);
        }

        string selMDA = "insphere " + sphereCenter.x.ToString("F5", culture) + " " +
                        sphereCenter.y.ToString("F5", culture) + " " + sphereCenter.z.ToString("F5", culture) + " " +
                        radius.ToString("F3", culture);

        MDAnalysisSelection selec = new(selMDA, allAtoms);
        UnityMolSelection result = selec.process();
        result.name = selName;

        if (selM.selections.ContainsKey(selName)) {
            selM.selections[selName].atoms = result.atoms;
            if (!selM.selections[selName].bondsNull) {
                selM.selections[selName].fillBonds();
            }
            selM.selections[selName].fillStructures();
            updateRepresentations(selName);
        }

        selM.SetCurrentSelection(result);
        Debug.Log(result);

        UnityMolMain.recordPythonCommand(string.Format(culture, "selectInSphere(Vector3({0}, {1}, {2}), {3})",
                                         position.x, position.y, position.z, radius));

        UnityMolMain.recordUndoPythonCommand("deleteSelection(\"" + result.name + "\")");

        return result;
    }

    /// <summary>
    /// Select atoms of all loaded molecules inside a parallelepiped defined by a molecular space position and 3 axis
    /// and create a new selection from it.
    /// </summary>
    /// <param name="lowerLeft">position of the lower left point of the parallelepiped</param>
    /// <param name="xaxis">X Axis of the parallelepiped </param>
    /// <param name="yaxis">Y Axis of the parallelepiped </param>
    /// <param name="zaxis">Z Axis of the parallelepiped </param>
    /// <returns>the new UnityMolSelection object</returns>
    public static UnityMolSelection selectInRectangle(Vector3 lowerLeft, Vector3 xaxis, Vector3 yaxis, Vector3 zaxis) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return null;
        }

        string selName = "InRectangle";

        selM.ClearCurrentSelection();

        List<UnityMolAtom> allAtoms = new();

        foreach (UnityMolStructure s in sm.loadedStructures) {
            allAtoms.AddRange(s.currentModel.allAtoms);
        }

        string selMDA = "inrect " + lowerLeft.x.ToString("F5", culture) + " " +
                        lowerLeft.y.ToString("F5", culture) + " " + lowerLeft.z.ToString("F5", culture) + " " +
                        xaxis.x.ToString("F3", culture) + " " + xaxis.y.ToString("F3", culture) + " " +
                        xaxis.z.ToString("F3", culture) + " " + yaxis.x.ToString("F3", culture) + " " + yaxis.y.ToString("F3", culture) + " " +
                        yaxis.z.ToString("F3", culture) + " " + zaxis.x.ToString("F3", culture) + " " + zaxis.y.ToString("F3", culture) + " " +
                        zaxis.z.ToString("F3", culture);

        MDAnalysisSelection selec = new(selMDA, allAtoms);
        UnityMolSelection result = selec.process();
        result.name = selName;

        if (selM.selections.ContainsKey(selName)) {
            selM.selections[selName].atoms = result.atoms;
            if (!selM.selections[selName].bondsNull) {
                selM.selections[selName].fillBonds();
            }
            selM.selections[selName].fillStructures();
            updateRepresentations(selName);
        }

        selM.SetCurrentSelection(result);
        Debug.Log(result);

        UnityMolMain.recordPythonCommand(string.Format(culture, "selectInRectangle(Vector3({0}, {1}, {2}), Vector3({3}, {4}, {5}), Vector3({6}, {7}, {8}))",
                                         lowerLeft.x, lowerLeft.y, lowerLeft.z,
                                         xaxis.x, xaxis.y, xaxis.z,
                                         yaxis.x, yaxis.y, yaxis.z,
                                         zaxis.x, zaxis.y, zaxis.z));

        UnityMolMain.recordUndoPythonCommand("deleteSelection(\"" + result.name + "\")");

        return result;
    }

    /// <summary>
    /// Update all representations of the specified selection, called automatically after a selection content change
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    public static void updateRepresentations(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (!selM.selections.ContainsKey(selName)) {
            Debug.LogError("Cannot update representations of the selection '" + selName + "' as it does not exist");
            return;
        }
        UnityMolSelection sel = selM.selections[selName];

        foreach (List<UnityMolRepresentation> reps in sel.representations.Values) {
            foreach (UnityMolRepresentation r in reps) {
                r.updateWithNewSelection(sel);
            }
        }
    }

    /// <summary>
    /// Clear the current selection in UnityMolSelectionManager
    /// </summary>
    public static void clearSelections() {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        selM.ClearCurrentSelection();

        UnityMolMain.recordPythonCommand("clearSelections()");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Test if a trajectory is playing for any loaded molecule
    /// </summary>
    /// <returns>True if a trajectory is playing. False otherwise</returns>
    public static bool isATrajectoryPlaying() {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        foreach (UnityMolStructure s in sm.loadedStructures) {
            if (s.trajPlayer && s.trajPlayer.play) {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Activate or deactivate updating the content of the selection 'selName' during a trajectory
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="v">If True, activate the update. If false, deactivate the update</param>
    public static void setUpdateSelectionTraj(string selName, bool v) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (!selM.selections.ContainsKey(selName)) {
            Debug.LogError("Selection '" + selName + "' not found");
            return;
        }
        selM.selections[selName].updateContentWithTraj = v;
        UnityMolMain.recordPythonCommand("setUpdateSelectionTraj(\"" + selName + "\", " + cBoolToPy(v) + ")");
        UnityMolMain.recordUndoPythonCommand("setUpdateSelectionTraj(\"" + selName + "\", " + cBoolToPy(!v) + ")");
    }

    /// <summary>
    /// Show or hide representation shadows for a given selection and a given representation.
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of the representation</param>
    /// <param name="enable">if True, show the shadows. Hide if false</param>
    public static void setShadows(string selName, string type, bool enable) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType(type);

            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {

                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);

                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        existingRep.ShowShadows(enable);
                    }
                }
            } else {
                Debug.LogError("Wrong representation type");
                return;
            }
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }
        UnityMolMain.recordPythonCommand("setShadows(\"" + selName + "\", \"" + type + "\", " + enable + ")");
        UnityMolMain.recordUndoPythonCommand("setShadows(\"" + selName + "\", \"" + type + "\", " + !enable + ")");
    }

    /// <summary>
    /// Utility function to change the material of highlighted selection
    /// </summary>
    /// <param name="newMat">the new material as a Material object.</param>
    public static void changeHighlightMaterial(Material newMat) {
#if !DISABLE_HIGHLIGHT
        UnityMolHighlightManager highlm = UnityMolMain.getHighlightManager();
        highlm.changeMaterial(newMat);
#endif
    }

    /// <summary>
    /// Take a screenshot of the current viewpoint with a specific resolution
    /// </summary>
    /// <param name="filePath">Path to save the screenshot</param>
    /// <param name="resolutionWidth">Width of the image</param>
    /// <param name="resolutionHeight">Height of the image</param>
    /// <param name="transparentBG">Set the background to transparent?</param>
    public static void screenshot(string filePath, int resolutionWidth = 1280, int resolutionHeight = 720, bool transparentBG = false) {
        RecordManager.takeScreenshot(filePath, resolutionWidth, resolutionHeight, transparentBG);
    }

    /// <summary>
    /// Start to record a video with FFMPEG at a specific resolution and framerate
    /// </summary>
    /// <param name="filePath">Path to save the video</param>
    /// <param name="resolutionWidth">Width of the video</param>
    /// <param name="resolutionHeight">Height of the video</param>
    /// <param name="frameRate">framerate of the video</param>
    /// <param name="pauseAtStart"></param>
    public static void startVideo(string filePath, int resolutionWidth = 1280, int resolutionHeight = 720, int frameRate = 30, bool pauseAtStart = false) {
        RecordManager.startRecordingVideo(filePath, resolutionWidth, resolutionHeight, frameRate, pauseAtStart);
    }
    /// <summary>
    /// Stop recording
    /// </summary>
    public static void stopVideo() {
        RecordManager.stopRecordingVideo();
    }

    /// <summary>
    /// Pause recording
    /// </summary>
    public static void pauseVideo() {
        RecordManager.pauseRecordingVideo();
    }
    /// <summary>
    /// Unpause recording
    /// </summary>
    public static void unpauseVideo() {
        RecordManager.unpauseRecordingVideo();
    }

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
            bool success = false;
            pythonConsole.ExecuteCommand(lastUndoCommand, ref success);

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
    /// <param name="structureName">the structure name concerned</param>
    /// <param name="pos">the new position</param>
    /// <param name="rot">the new rotation as euler angles.</param>
    public static void setStructurePositionRotation(string structureName, Vector3 pos, Vector3 rot) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolStructure s = sm.GetStructure(structureName);

        Vector3 savePos, saveRot;
        if (s != null) {
            GameObject sgo = sm.structureToGameObject[s.name];
            savePos = sgo.transform.localPosition;
            saveRot = sgo.transform.localEulerAngles;

            sgo.transform.localPosition = pos;
            sgo.transform.localRotation = Quaternion.Euler(rot);
        } else {
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

    /// <summary>
    /// Get the current position and rotation of the given structure
    /// </summary>
    /// <param name="structureName">the structure name concerned</param>
    /// <param name="pos">Reference of the position</param>
    /// <param name="rot">Reference of the rotation</param>
    public static void getStructurePositionRotation(string structureName, ref Vector3 pos, ref Vector3 rot) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolStructure s = sm.GetStructure(structureName);

        if (s != null) {
            GameObject sgo = sm.structureToGameObject[s.name];
            pos = sgo.transform.localPosition;
            rot = sgo.transform.localEulerAngles;
        } else {
            Debug.LogError("Wrong structure name");
        }
    }

    /// <summary>
    /// Get the current position and rotation of the given structure as a string
    /// </summary>
    /// <param name="structureName">the structure name concerned</param>
    /// <returns>the position & rotation as a string</returns>
    public static string getStructurePositionRotation(string structureName) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolStructure s = sm.GetStructure(structureName);

        if (s == null) {
            Debug.LogError("Wrong structure name");
            return "";
        }

        GameObject sgo = sm.structureToGameObject[s.name];
            Vector3 pos = sgo.transform.localPosition;
            Vector3 rot = sgo.transform.localEulerAngles;
            return "Vector3(" + pos.x.ToString("F4", culture) + ", " +
                   pos.y.ToString("F4", culture) + ", " + pos.z.ToString("F4", culture) + "), " +
                   "Vector3(" + rot.x.ToString("F4", culture) + ", " +
                   rot.y.ToString("F4", culture) + ", " + rot.z.ToString("F4", culture) + "))";
    }

    /// <summary>
    /// Save the history of commands executed in a file
    /// </summary>
    /// <param name="filepath">path to write the file</param>
    public static void saveScript(string filepath) {
        saveHistoryScript(filepath);
    }

    /// <summary>
    /// Save the history of commands executed in a file
    /// </summary>
    /// <param name="filepath">path to write the file</param>
    public static void saveHistoryScript(string filepath) {
        string scriptContent = UnityMolMain.commandHistory();

        //Set center to false in fetch and load commands => this is handled by the loadedMolParentToString function
        string[] commands = scriptContent.Split(new [] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < commands.Length; i++) {
            string c = commands[i];
            if (c.StartsWith("fetch(") || c.StartsWith("load(")) {
                commands[i] = commands[i].Replace(", center=True", ", center=False");
                commands[i] = commands[i].Replace(", center= True", ", center= False");
            }
        }
        scriptContent = string.Join("\n", commands);

        scriptContent += loadedMolParentToString();

        File.WriteAllText(filepath, scriptContent);

        Debug.Log("Saved history script to '" + filepath + "'");
    }

    /// <summary>
    /// Save the current positions of the loaded structures in a single PDB file
    /// If no filepath provided, used a predefined filename
    /// </summary>
    /// <param name="filepath">path to write the PDB file</param>
    public static void saveDockingState(string filepath = null) {

        DockingManager dm = UnityMolMain.getDockingManager();
        if (filepath == null) {
            filepath = dm.saveDockingState();
        } else {
            dm.saveDockingState(filepath);
        }

        UnityMolMain.recordPythonCommand($"saveDockingState(\"{filepath.Replace("\\", " / ")}\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Save as a string position & rotation of all loaded structures
    /// and all the Transform information of their parent (i.e. LoadedMolecules GameObject)
    /// </summary>
    /// <param name="addToHistory">add this command to the history?</param>
    /// <returns>the string</returns>
    public static string loadedMolParentToString(bool addToHistory = false) {
        ManipulationManager mm = getManipulationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        string res = "";
        foreach (UnityMolStructure s in sm.loadedStructures) {
            Vector3 spos = Vector3.zero;
            Vector3 srot = Vector3.zero;
            getStructurePositionRotation(s.name, ref spos, ref srot);
            res += "\n\nsetStructurePositionRotation(\"" + s.name + "\", Vector3(" + spos.x.ToString("F4", culture) + ", " +
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


        if (addToHistory) {
            UnityMolMain.recordPythonCommand(res);
            UnityMolMain.recordUndoPythonCommand("");
        }

        return res;
    }

    /// <summary>
    /// Write a serialized session file to a JSON file
    /// </summary>
    /// <param name="filePath">path to write the JSON file</param>
    public static void writeSessionToFile(string filePath) {
        string sessionJson = UnityMolMain.sessionToJSON();
        using(StreamWriter sw = new(filePath, false)) {
          sw.Write(sessionJson);
          sw.Close();
        }
    }

    /// <summary>
    /// Read a JSON file and restore the session.
    /// Remove all molecules loaded previously first.
    /// </summary>
    /// <param name="filePath">path to read the JSON file</param>
    public static void readSessionFromFile(string filePath)
    {
        //Delete loaded molecules
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        List<string> toDelete = new();

        foreach (UnityMolStructure s in sm.loadedStructures) {
          toDelete.Add(s.name);
        }
        foreach (string s in toDelete) {
          UnityMolStructure stru = sm.GetStructure(s);
          if (stru != null) {
              sm.Delete(stru);
          }
        }

        //Read JSON file
        string realPath = filePath;
        string customPath = Path.Combine(path, filePath);
        if (File.Exists(customPath)) {
          realPath = customPath;
        }

        string sessionJson = File.ReadAllText(realPath, Encoding.UTF8);
        //Restore session
        UnityMolMain.JSONToSession(sessionJson);
    }

    /// <summary>
    /// Load a history file of commands (.py file)
    /// </summary>
    /// <param name="filePath">path to read the history file</param>
    public static void loadHistoryScript(string filePath) {

        string realPath = filePath;
        string customPath = Path.Combine(path, filePath);
        if (File.Exists(customPath)) {
            realPath = customPath;
        }

        instance.StartCoroutine(pythonConsole.ExecuteScript(realPath));
    }

    /// <summary>
    /// Load a history file of commands (.py file)
    /// </summary>
    /// <param name="filePath">path to read the history file</param>
    public static void loadScript(string filePath) {
        loadHistoryScript(filePath);
    }

    /// <summary>
    /// Save as a string all the Transform information of the LoadedMolecules GameObject
    /// </summary>
    /// <returns>the string</returns>
    public static string getMolParentTransform() {
        Transform parentT = UnityMolMain.getRepresentationParent().transform;
        return "Vector3(" + parentT.position.x.ToString("F4", culture) + ", " +
               parentT.position.y.ToString("F4", culture) + ", " + parentT.position.z.ToString("F4", culture) + "), " +
               "Vector3(" + parentT.rotation.eulerAngles.x.ToString("F4", culture) + ", " +
               parentT.rotation.eulerAngles.y.ToString("F4", culture) + ", " + parentT.rotation.eulerAngles.z.ToString("F4", culture) + "))";
    }


    /// <summary>
    /// Set the position, scale and rotation of the parent of all loaded molecules (LoadedMolecules GameObject)
    /// Linear interpolation between the current state of the camera to the specified values
    /// </summary>
    /// <param name="pos">the new position</param>
    /// <param name="scale">the new scale</param>
    /// <param name="rot">the new rotation</param>
    /// <param name="centerOfRotation">the new center of rotation</param>
    /// <param name="lerp">Use a linear interpolation between the current of the new values?</param>
    /// <param name="duration">Duration of the linear interpolation</param>
    public static void setMolParentTransform(Vector3 pos, Vector3 scale, Vector3 rot, Vector3 centerOfRotation,
        bool lerp = true, float duration = 1.0f) {
        instance.setPosScaleRot(pos, scale, rot, centerOfRotation, lerp, duration);
    }

    /// <summary>
    /// Set the position, scale and rotation of the parent of all loaded molecules (LoadedMolecules GameObject)
    /// Linear interpolation between the current state of the camera to the specified values
    /// </summary>
    /// <param name="pos">the new position</param>
    /// <param name="scale">the new scale</param>
    /// <param name="rot">the new rotation</param>
    /// <param name="centerOfRotation">the new center of rotation</param>
    /// <param name="lerp">Use a linear interpolation between the current of the new values?</param>
    /// <param name="duration">Duration of the linear interpolation</param>
    private void setPosScaleRot(Vector3 pos, Vector3 scale, Vector3 rot, Vector3 centerOfRotation, bool lerp, float duration) {
        Transform parentT = UnityMolMain.getRepresentationParent().transform;
        StartCoroutine(delayedSetTransform(parentT, pos, scale, rot, centerOfRotation, lerp, duration));
    }

    /// <summary>
    /// Coroutine to modify the transform component of the LoadedMolecules GameObject.
    /// </summary>
    /// <param name="t">the current transform component</param>
    /// <param name="endpos">the new position</param>
    /// <param name="scale">the new scale</param>
    /// <param name="rot">the new rotation</param>
    /// <param name="centerOfRotation">the new center of rotation</param>
    /// <param name="lerp">Use a linear interpolation between the current of the new values?</param>
    /// <param name="duration">Duration of the linear interpolation</param>
    /// <returns></returns>
    private IEnumerator delayedSetTransform(Transform t, Vector3 endpos, Vector3 scale, Vector3 rot,
        Vector3 centerOfRotation, bool lerp, float duration = 1.0f) {
        //End of frame
        yield return 0;

        t.localScale = scale;
        Quaternion targetRot = Quaternion.Euler(rot);
        Quaternion fromRot = t.rotation;
        Vector3 startpos = t.position;

        float multi = 1.0f / duration;
        float ratio = 0.0f;
        if (lerp) {
            while (t.position != endpos) {
                ratio += Time.deltaTime * multi;
                t.position = Vector3.Lerp(startpos, endpos, ratio);
                t.rotation = Quaternion.Lerp(fromRot, targetRot, ratio);
                yield return 0;
            }
        }

        t.position = endpos;
        t.rotation = targetRot;

        var mm = getManipulationManager();
        mm.setRotationCenter(centerOfRotation);
    }

    /// <summary>
    /// Change the scale of the parent of the representations of each molecules
    /// Try to not move the center of mass
    /// </summary>
    /// <param name="newVal">the new value of the scale</param>
    public static void changeGeneralScale_cog(float newVal) {
        if (!(newVal > 0.0f) || float.IsPositiveInfinity(newVal)) {
            return;
        }

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        Transform molPar = UnityMolMain.getRepresentationParent().transform;

        List<Vector3> savedCog = new();

        foreach (Transform t in molPar) {
            UnityMolStructure s = sm.selectionNameToStructure(t.name);
            if (s != null) {
                savedCog.Add(t.TransformPoint(s.currentModel.centroid));
            }
        }

        molPar.localScale = Vector3.one * newVal;

        int i = 0;
        foreach (Transform t in molPar) {
            UnityMolStructure s = sm.selectionNameToStructure(t.name);
            if (s != null) {
                Vector3 newCog = t.TransformPoint(s.currentModel.centroid);
                t.Translate(savedCog[i++] - newCog, Space.World);
            }
        }
    }

    /// <summary>
    /// Change the scale of the parent of the representations of each molecules
    /// Keep relative positions of molecules, use the first loaded molecule center of gravity to compensate the translation due to scaling
    /// </summary>
    /// <param name="newVal">the new value of the scale</param>
    public static void changeGeneralScale(float newVal) {
        if (!(newVal > 0.0f) || float.IsPositiveInfinity(newVal)) {
            return;
        }

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        Transform molPar = UnityMolMain.getRepresentationParent().transform;

        if (molPar.childCount == 0) {
            return;
        }
        UnityMolStructure s = null;
        Transform t = null;
        foreach (Transform tr in molPar) {
            s = sm.selectionNameToStructure(tr.name);
            if (s != null) {
                t = tr;
                break;
            }
        }

        if (s == null) {
            return;
        }

        Vector3 save = t.TransformPoint(s.currentModel.centroid);
        molPar.localScale = Vector3.one * newVal;
        Vector3 newP = t.TransformPoint(s.currentModel.centroid);
        molPar.Translate(save - newP, Space.World);

        //Adapt the size of the point representation with scaling
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        foreach (KeyValuePair<string, UnityMolSelection> selection in selM.selections) {
            setRepSize(selection.Key, "p",  AtomRepresentationPoint.ScalingFactor * newVal);
        }
    }

    /// <summary>
    /// Use Reduce method to add hydrogens to a given structure
    /// </summary>
    /// <param name="structureName">The name of the structure concerned</param>
    public static void addHydrogensReduce(string structureName) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolStructure s = sm.GetStructure(structureName);

        if (s == null) {
            Debug.LogError("Wrong structure name");
            return;
        }

        if (s.trajectoryLoaded) { //Prevents hydrogen addition via Reduce when a trajectory is loaded for the structure.
            Debug.LogWarning("Reduce cannot be used to build hydrogens when a trajectory is loaded.");
            return;
        }

        ReduceWrapper.callReduceOnStructure(s);
        s.updateRepresentations(trajectory: false);

        UnityMolMain.recordPythonCommand("addHydrogensReduce(\"" + structureName + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Use HAAD method to add hydrogens
    /// </summary>
    /// <param name="structureName">The name of the structure concerned</param>
    public static void addHydrogensHaad(string structureName) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolStructure s = sm.GetStructure(structureName);

        if (s == null) {
            Debug.LogError("Wrong structure name");
            return;
        }

        if (s.trajectoryLoaded) { //Prevents hydrogen addition via Haad when a trajectory is loaded for the structure.
           Debug.LogWarning("Haad cannot be used to build hydrogens when a trajectory is loaded.");
           return;
        }

        HaadWrapper.callHaadOnStructure(s);
        s.updateRepresentations(trajectory: false);

        UnityMolMain.recordPythonCommand("addHydrogensHaad(\"" + structureName + "\")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Set the atoms of the selection named 'selName' to ligand
    /// </summary>
    /// <param name="selName">The name of the selection concerned</param>
    /// <param name="updateAllSelections">Update all selections?</param>
    public static void setAsLigand(string selName, bool updateAllSelections = true) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        const bool isLig = true;

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];

            foreach (UnityMolAtom a in sel.atoms) {
                a.isLigand = isLig;

            }
            Debug.Log("Set " + sel.atoms.Count + " atom(s) as ligand");
        } else {
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
                    updateSelectionWithMDA(sele.name, sele.MDASelString, true, recordCommand : false);
                }
            }
        }
    }

    /// <summary>
    /// Merge 2 UnityMolStructures using a different chain name to avoid conflict
    /// Keep the name of the first UnityMolStructure
    /// </summary>
    /// <param name="structureName">The name of the 1st structure concerned</param>
    /// <param name="structureName2">The name of the 2nd structure</param>
    /// <param name="chainName">the name of the chain for the 2nd structure merged into the first one</param>
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
    /// <param name="selName">The name of the selection concerned</param>
    /// <param name="fullPath">path to write the PDB file</param>
    /// <param name="writeSSinfo">Save the secondary structure information in the file?</param>
    public static void saveToPDB(string selName, string fullPath, bool writeSSinfo = false) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (selM.selections.ContainsKey(selName)) {
            UnityMolSelection sel = selM.selections[selName];
            if (sel.Count == 0) {
                Debug.LogWarning("Empty selection");
                return;
            }

            Vector3[] atomPos = new Vector3[sel.atoms.Count];

            Transform strucPar = UnityMolMain.getStructureManager().GetStructureGameObject(
                                     sel.structures[0].name).transform;

            int id = 0;
            foreach (UnityMolAtom a in sel.atoms) {
                atomPos[id++] = strucPar.InverseTransformPoint(a.curWorldPosition);
            }

            string pdbLines = PDBReader.Write(sel, overridedPos : atomPos, writeSS : writeSSinfo);
            if (string.IsNullOrEmpty(pdbLines)) {
                return;
            }
            try {
                StreamWriter writer = new(fullPath, false);
                writer.WriteLine(pdbLines);
                writer.Close();
                Debug.Log("Wrote PDB file: '" + Path.GetFullPath(fullPath) + "'");
            } catch {
                Debug.LogError("Failed to write to '" + Path.GetFullPath(fullPath) + "'");
            }

        } else {
            Debug.LogError("No selection named " + selName);
        }
    }

    /// <summary>
    /// Connect to a running simulation using the IMD protocol implemented in MDDriver
    /// The running simulation is bound to a UnityMolStructure
    /// </summary>
    /// <param name="structureName">The name of the 1st structure concerned</param>
    /// <param name="adress">the IP address of the running simulation</param>
    /// <param name="port">the port of the running simulation</param>
    /// <returns>True if the connexion is successful. False otherwise</returns>
    public static bool connectIMD(string structureName, string adress, int port) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return false;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        bool res;
        try {

            if (s.mddriverM != null) {
                Debug.LogError("Already connected to a running simulation");
                return false;
            }
            res = s.connectIMD(adress, port);

        } catch (Exception e) {
            Debug.LogError("Could not connect to the simulation on " + adress + " : " + port + "\n " + e);
            s.disconnectIMD();
            return false;
        }

        UnityMolMain.recordPythonCommand("connectIMD(\"" + structureName + "\", \"" + adress + "\", " + port + ")");
        UnityMolMain.recordUndoPythonCommand("disconnectIMD(\"" + structureName + "\")");
        return res;
    }

    /// <summary>
    /// Disconnect from the IMD simulation for the specified structure
    /// </summary>
    /// <param name="structureName">The name of the 1st structure concerned</param>
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

    /// <summary>
    /// Get the current type of surface of given selection as a string
    /// Possible output are : "Solid", "Wireframe", "Transparent" or "".
    /// </summary>
    /// <param name="selName">The name of the selection concerned</param>
    /// <returns>the type of the surface. Empty if no surface representation is available for the selection.</returns>
    public static string getSurfaceType(string selName) {

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType("s");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);

            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolSurfaceManager surfM = (UnityMolSurfaceManager) sr.atomRepManager;

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
                        UnityMolSurfaceManager surfM = (UnityMolSurfaceManager) sr.atomRepManager;

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

    /// <summary>
    /// Get the hyperball metaphor of given selection as a string
    /// Possible output are : "Smooth", "BallsAndSticks", "VdW", "Licorice", "Hidden" or "".
    /// </summary>
    /// <param name="selName">The name of the selection concerned</param>
    /// <returns>the metaphor of the HyperBall. Empty if no HB representation is available for the selection.</returns>
    public static string getHyperBallMetaphore(string selName) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {

            RepType repType = getRepType("hb");

            List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);

            if (existingReps != null) {
                foreach (UnityMolRepresentation existingRep in existingReps) {
                    foreach (SubRepresentation sr in existingRep.subReps) {
                        UnityMolHStickMeshManager hsManager = (UnityMolHStickMeshManager) sr.bondRepManager;
                        if (hsManager == null) {
                            continue;
                        }

                        if (Mathf.Approximately(hsManager.shrink, 0.4f) && Mathf.Approximately(hsManager.scaleBond, 1.0f)) {
                            return "Smooth";
                        }

                        if (Mathf.Approximately(hsManager.shrink, 0.001f) && Mathf.Approximately(hsManager.scaleBond, 0.2f)) {
                            return "BallsAndSticks";
                        }

                        if (Mathf.Approximately(hsManager.shrink, 1.0f) && hsManager.scaleBond == 0.0f) {
                            return "VdW";
                        }

                        if (Mathf.Approximately(hsManager.shrink, 0.001f) && Mathf.Approximately(hsManager.scaleBond, 0.3f)) {
                            return "Licorice";
                        }

                        if (Mathf.Approximately(hsManager.shrink, 1.0f) && hsManager.scaleBond == 0.0f) {
                            return "Hidden";
                        }
                    }
                }
            }
        }
        return "";
    }

    /// <summary>
    /// Activate the orthographic mode of the camera
    /// </summary>
    public static void setCameraOrtho() {
        const bool ortho = true;
        if (UnityMolMain.inVR()) {
            Debug.LogWarning("Cannot activate orthographic camera in VR");
            return;
        }
        if (Camera.main != null && Camera.main.orthographic != ortho) {
            Camera.main.orthographic = ortho;
        }

        UnityMolMain.recordPythonCommand("setCameraOrtho(" + cBoolToPy(ortho) + ")");
        UnityMolMain.recordUndoPythonCommand("setCameraOrtho(" + cBoolToPy(!ortho) + ")");
    }

    /// <summary>
    /// Set the size of the orthographic mode of the camera
    /// </summary>
    /// <param name="orthoSize">the new size</param>
    public static void setCameraOrthoSize(float orthoSize) {
        if (UnityMolMain.inVR()) {
            Debug.LogWarning("Cannot change the orthographic size of the camera in VR");
            return;
        }

        orthoSize = Mathf.Max(0.001f, orthoSize);
        float prevVal = 5.0f;

        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.orthographic) {
            prevVal = mainCam.orthographicSize;
            mainCam.orthographicSize = orthoSize;
        }

        const int lenSameCom = 19;
        bool replaced = UnityMolMain.recordPythonCommand("setCameraOrthoSize(" + orthoSize.ToString("F4") + ")", true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("setCameraOrthoSize(" + prevVal.ToString("F4") + ")", replaced);
    }

    /// <summary>
    /// Set camera near plane
    /// <remarks>This has an impact on shadow map quality</remarks>
    /// </summary>
    /// <param name="newV">the new value</param>
    public static void setCameraNearPlane(float newV) {
        if (Camera.main == null) {
            return;
        }

        float prevVal = Camera.main.nearClipPlane;
        newV = Mathf.Clamp(newV, 0.001f, 100.0f);
        Camera.main.nearClipPlane = newV;

        const int lenSameCom = 19;
        bool replaced = UnityMolMain.recordPythonCommand("setCameraNearPlane(" + newV.ToString("F4", culture) + ")", true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("setCameraNearPlane(" + prevVal.ToString("F4", culture) + ")", replaced);
    }

    /// <summary>
    /// Set camera far plane
    /// <remarks>This has an impact on shadow map quality</remarks>
    /// </summary>
    /// <param name="newV">the new value</param>
    public static void setCameraFarPlane(float newV) {
        if (Camera.main == null) {
            return;
        }

        float prevVal = Camera.main.farClipPlane;
        newV = Mathf.Clamp(newV, 0.1f, 5000.0f);
        Camera.main.farClipPlane = newV;
        int lenSameCom = 18;
        bool replaced = UnityMolMain.recordPythonCommand("setCameraFarPlane(" + newV.ToString("F4", culture) + ")", true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("setCameraFarPlane(" + prevVal.ToString("F4", culture) + ")", replaced);
    }

    /// <summary>
    /// Enable depth cueing effect
    /// </summary>
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

    /// <summary>
    /// Disable depth cueing effect
    /// </summary>
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

    /// <summary>
    /// Set depth cueing starting position in world space
    /// </summary>
    /// <param name="v">the starting position</param>
    public static void setDepthCueingStart(float v) {
        float prev = UnityMolMain.fogStart;
        UnityMolMain.fogStart = v;
        getManipulationManager().initFollowDepthCueing();

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
        const int lenSameCom = 20;
        bool replaced = UnityMolMain.recordPythonCommand("setDepthCueingStart(" + UnityMolMain.fogStart.ToString("F2", culture) + ")", true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("setDepthCueingStart(" + prev.ToString("F2", culture) + ")", replaced);
    }

    /// <summary>
    /// Set depth cueing density
    /// </summary>
    /// <param name="v">the new density</param>
    public static void setDepthCueingDensity(float v) {
        float prev = UnityMolMain.fogDensity;
        UnityMolMain.fogDensity = v;
        getManipulationManager().initFollowDepthCueing();

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
        int lenSameCom = 22;
        bool replaced = UnityMolMain.recordPythonCommand("setDepthCueingDensity(" + UnityMolMain.fogDensity.ToString("F2", culture) + ")", true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("setDepthCueingDensity(" + prev.ToString("F2", culture) + ")", replaced);
    }

    /// <summary>
    /// Set depth cueing color
    /// </summary>
    /// <param name="col">the new color</param>
    public static void setDepthCueingColor(Color col) {

        RenderSettings.fogColor = col;

        UnityMolMain.recordPythonCommand("setDepthCueingColor(" + string.Format(CultureInfo.InvariantCulture, "{0})", col));
        UnityMolMain.recordUndoPythonCommand("setDepthCueingColor(" + string.Format(CultureInfo.InvariantCulture, "{0})", col));
    }


    /// <summary>
    /// Enable/Disable depth cueing update when zooming in or out
    /// </summary>
    /// <param name="v">Enable or disable</param>
    public static void setDepthCueingFollow(bool v) {
        getManipulationManager().depthcueUpdate = v;
        getManipulationManager().initFollowDepthCueing();

        UnityMolMain.recordPythonCommand("setDepthCueingFollow(" + cBoolToPy(v) + ")");
        UnityMolMain.recordUndoPythonCommand("setDepthCueingFollow(" + cBoolToPy(v) + ")");

    }

    /// <summary>
    /// Enable DOF (Depth of field) effect
    /// <remarks>Not available in VR</remarks>
    /// </summary>
    public static void enableDOF() {
        if (UnityMolMain.inVR()) {
            Debug.LogWarning("Cannot enable DOF in VR");
            return;
        }
        if (UnityMolMain.raytracingMode) {
            if (Camera.main != null) {
                RaytracedObject rto = Camera.main.gameObject.GetComponent<RaytracedObject>();
                rto.setAperture(1.0f);
            }
        }
        try {
            if (Camera.main != null) {
                MouseAutoFocus maf = Camera.main.gameObject.GetComponent<MouseAutoFocus>();
                if (maf == null) {
                    maf = Camera.main.gameObject.AddComponent<MouseAutoFocus>();
                }
                maf.Init();
                maf.enableDOF();
            }
        } catch {
            Debug.LogError("Couldn't enable DOF");
            return;
        }
        UnityMolMain.recordPythonCommand("enableDOF()");
        UnityMolMain.recordUndoPythonCommand("disableDOF()");
    }

    /// <summary>
    /// Disable DOF (Depth of field) effect
    /// <remarks>Not available in VR</remarks>
    /// </summary>
    public static void disableDOF() {
        if (UnityMolMain.raytracingMode) {
            if (Camera.main != null) {
                RaytracedObject rto = Camera.main.gameObject.GetComponent<RaytracedObject>();
                rto.setAperture(-1.0f);
            }
        }
        try {
            if (Camera.main != null) {
                MouseAutoFocus maf = Camera.main.gameObject.GetComponent<MouseAutoFocus>();
                if (maf == null) {
                    maf = Camera.main.gameObject.AddComponent<MouseAutoFocus>();
                }
                maf.Init();
                maf.disableDOF();
            }
        } catch {
            Debug.LogError("Couldn't disable DOF");
            return;
        }
        UnityMolMain.recordPythonCommand("disableDOF()");
        UnityMolMain.recordUndoPythonCommand("enableDOF()");
    }

    /// <summary>
    /// Set DOF focus distance
    /// this is used by the MouseAutoFocus script
    /// </summary>
    /// <param name="v">the new distance</param>
    public static void setDOFFocusDistance(float v) {
        float prev = 0.0f;
        try {
            if (Camera.main != null) {
                MouseAutoFocus maf = Camera.main.gameObject.GetComponent<MouseAutoFocus>();

                prev = maf.getFocusDistance();
                maf.setFocusDistance(v);
            }
        } catch {
            Debug.LogError("Couldn't set DOF focus distance");
            return;
        }
        const int lenSameCom = 20;
        bool replaced = UnityMolMain.recordPythonCommand("setDOFFocusDistance(" + v.ToString("F3", culture) + ")", true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("setDOFFocusDistance(" + prev.ToString("F3", culture) + ")", replaced);
    }

    /// <summary>
    /// Set DOF aperture
    /// </summary>
    /// <param name="a">the new aperture</param>
    public static void setDOFAperture(float a) {
        if (UnityMolMain.raytracingMode) {
            if (Camera.main != null) {
                RaytracedObject rto = Camera.main.gameObject.GetComponent<RaytracedObject>();
                rto.setAperture(a * 0.005f);
            }
        }
        float prev = 0.0f;
        try {
            if (Camera.main != null) {
                MouseAutoFocus maf = Camera.main.gameObject.GetComponent<MouseAutoFocus>();

                prev = maf.getAperture();
                maf.setAperture(a);
            }
        } catch {
            Debug.LogError("Couldn't set DOF aperture");
            return;
        }
        const int lenSameCom = 15;
        bool replaced = UnityMolMain.recordPythonCommand("setDOFAperture(" + a.ToString("F3", culture) + ")", true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("setDOFAperture(" + prev.ToString("F3", culture) + ")", replaced);
    }

    /// <summary>
    /// Set DOF focal length
    /// </summary>
    /// <param name="f">the new length</param>
    public static void setDOFFocalLength(float f) {
        if (UnityMolMain.raytracingMode) {
            if (Camera.main != null) {
                RaytracedObject rto = Camera.main.gameObject.GetComponent<RaytracedObject>();
                rto.setFDist(f * 0.05f);
            }
        }
        float prev = 0.0f;
        try {
            if (Camera.main != null) {
                MouseAutoFocus maf = Camera.main.gameObject.GetComponent<MouseAutoFocus>();

                prev = maf.getFocalLength();
                maf.setFocalLength(f);
            }
        } catch {
            Debug.LogError("Couldn't set DOF focal length");
            return;
        }
        const int lenSameCom = 18;
        bool replaced = UnityMolMain.recordPythonCommand("setDOFFocalLength(" + f.ToString("F3", culture) + ")", true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("setDOFFocalLength(" + prev.ToString("F3", culture) + ")", replaced);
    }

    /// <summary>
    /// Enable outline post-process effect
    /// </summary>
    public static void enableOutline() {

        try {
            OutlineEffectUtil outlineScript = Camera.main!.gameObject.GetComponent<OutlineEffectUtil>();
            if (outlineScript == null) {
                outlineScript = Camera.main.gameObject.AddComponent<OutlineEffectUtil>();
            }

            outlineScript.enableOutline();

        } catch {
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
            OutlineEffectUtil outlineScript = Camera.main!.gameObject.GetComponent<OutlineEffectUtil>();
            if (outlineScript == null) {
                outlineScript = Camera.main.gameObject.AddComponent<OutlineEffectUtil>();
            }

            outlineScript.disableOutline();

        } catch {
            Debug.LogError("Couldn't disable Outline effect");
            return;
        }
        UnityMolMain.recordPythonCommand("disableOutline()");
        UnityMolMain.recordUndoPythonCommand("enableOutline()");
    }

    /// <summary>
    /// Set the thickness of the outline effect
    /// </summary>
    /// <param name="v">the new thickness</param>
    public static void setOutlineThickness(float v) {

        float prev;
        try {
            OutlineEffectUtil outlineScript = Camera.main!.gameObject.GetComponent<OutlineEffectUtil>();
            if (outlineScript == null) {
                outlineScript = Camera.main.gameObject.AddComponent<OutlineEffectUtil>();
            }

            prev = outlineScript.getThickness();
            outlineScript.setThickness(v);

        } catch {
            Debug.LogError("Couldn't enable Outline effect");
            return;
        }

        const int lenSameCom = 20;
        bool replaced = UnityMolMain.recordPythonCommand("setOutlineThickness(" + v.ToString("F3", culture) + ")", true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("setOutlineThickness(" + prev.ToString("F3", culture) + ")", replaced);
    }

    /// <summary>
    /// Set the color of the outline effect
    /// </summary>
    /// <param name="col">the new color</param>
    public static void setOutlineColor(Color col) {

        Color prev;
        try {
            OutlineEffectUtil outlineScript = Camera.main!.gameObject.GetComponent<OutlineEffectUtil>();
            if (outlineScript == null) {
                outlineScript = Camera.main.gameObject.AddComponent<OutlineEffectUtil>();
            }

            prev = outlineScript.getColor();
            outlineScript.setColor(col);

        } catch {
            Debug.LogError("Couldn't enable Outline effect");
            return;
        }

        const int lenSameCom = 16;
        bool replaced = UnityMolMain.recordPythonCommand("setOutlineColor(" + string.Format(CultureInfo.InvariantCulture, "{0})", col), true, lenSameCom);
        UnityMolMain.recordUndoPythonCommand("setOutlineColor(" + string.Format(CultureInfo.InvariantCulture, "{0})", prev), replaced);
    }


    /// <summary>
    /// Print the content of the current directory, outputs only the files
    /// </summary>
    public static List<string> ls() {

        List<string> ret = Directory.GetFiles(path).ToList();
        foreach (string f in Directory.GetDirectories(path)) {
            Debug.Log("<b>" + f + "</b>");
        }

        foreach (string f in ret) {
            Debug.Log(f);
        }
        return ret;
    }

    /// <summary>
    /// Change the current directory to a new path
    /// </summary>
    /// <param name="newPath">the new path</param>
    public static void cd(string newPath) {
        newPath = newPath.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

        if (Directory.Exists(Path.GetFullPath(newPath))) {
            path = Path.GetFullPath(newPath);
            Debug.Log("Current path: '" + newPath + "'");
        } else {
            Debug.LogError("Incorrect path " + Path.GetFullPath(newPath));
        }

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
    /// <param name="colorS">the new color as a string</param>
    public static void bg_color(string colorS) {
        colorS = colorS.ToLower();
        Color col = strToColor(colorS);
        if (Camera.main == null) {
            return;
        }

        Color colprev = Camera.main.backgroundColor;
        Camera.main.backgroundColor = col;
        RenderSettings.fogColor = col;
        UnityMolMain.recordPythonCommand("bg_color(" + string.Format(CultureInfo.InvariantCulture, "{0})", col));
        UnityMolMain.recordUndoPythonCommand("bg_color(" + string.Format(CultureInfo.InvariantCulture, "{0})", colprev));
    }

    /// <summary>
    /// Change the background color of the camera based on a color name, also changes the fog color
    /// </summary>
    /// <param name="col">the new color as a Color object</param>
    public static void bg_color(Color col) {
        if (Camera.main == null) {
            return;
        }

        Color colprev = Camera.main.backgroundColor;
        Camera.main.backgroundColor = col;
        RenderSettings.fogColor = col;
        UnityMolMain.recordPythonCommand("bg_color(" + string.Format(CultureInfo.InvariantCulture, "{0})", col));
        UnityMolMain.recordUndoPythonCommand("bg_color(" + string.Format(CultureInfo.InvariantCulture, "{0})", colprev));
    }

    /// <summary>
    /// Convert a color string to a standard Unity Color
    /// Values can be "black", "white", "yellow" ,"green", "red", "blue", "pink", "gray"
    /// </summary>
    /// <param name="input">the color as a string</param>
    /// <returns>the color as Unity Color</returns>
    private static Color strToColor(string input) {
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
        } else {
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
        } else {
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
        } else {
            mm.rotateZ = true;
        }
        UnityMolMain.recordPythonCommand("switchRotateAxisZ()");
        UnityMolMain.recordUndoPythonCommand("switchRotateAxisZ()");
    }

    /// <summary>
    /// Change the rotation speed around the X axis
    /// </summary>
    /// <param name="val">the new speed</param>
    public static void changeRotationSpeedX(float val) {

        ManipulationManager mm = getManipulationManager();

        if (mm == null) {
            return;
        }
        float prevVal = mm.speedX;
        mm.speedX = val;

        UnityMolMain.recordPythonCommand("changeRotationSpeedX(" + val.ToString("F3", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("changeRotationSpeedX(" + prevVal.ToString("F3", culture) + ")");
    }
    /// <summary>
    /// Change the rotation speed around the Y axis
    /// </summary>
    /// <param name="val">the new speed</param>
    public static void changeRotationSpeedY(float val) {

        ManipulationManager mm = getManipulationManager();

        if (mm == null) {
            return;
        }
        float prevVal = mm.speedY;
        mm.speedY = val;

        UnityMolMain.recordPythonCommand("changeRotationSpeedY(" + val.ToString("F3", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("changeRotationSpeedY(" + prevVal.ToString("F3", culture) + ")");
    }

    /// <summary>
    /// Change the rotation speed around the Z axis
    /// </summary>
    /// <param name="val">the new speed</param>
    public static void changeRotationSpeedZ(float val) {

        ManipulationManager mm = getManipulationManager();

        if (mm == null) {
            return;
        }
        float prevVal = mm.speedZ;
        mm.speedZ = val;

        UnityMolMain.recordPythonCommand("changeRotationSpeedZ(" + val.ToString("F3", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("changeRotationSpeedZ(" + prevVal.ToString("F3", culture) + ")");
    }

    /// <summary>
    /// Change the mouse scroll speed
    /// </summary>
    /// <param name="val">the new speed</param>
    public static void setMouseScrollSpeed(float val) {

        float prevVal = 0.0f;
        if (val > 0.0f) {
            ManipulationManager mm = getManipulationManager();

            if (mm == null) {
                return;
            }
            prevVal = mm.scrollSpeed;
            mm.scrollSpeed = val;
        } else {
            Debug.LogError("Wrong speed value");
        }
        UnityMolMain.recordPythonCommand("setMouseScrollSpeed(" + val.ToString("F3", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("setMouseScrollSpeed(" + prevVal.ToString("F3", culture) + ")");
    }

    /// <summary>
    /// Change the speed of mouse rotations and translations
    /// </summary>
    /// <param name="val">the new speed</param>
    public static void setMouseMoveSpeed(float val) {

        float prevVal;
        if (val > 0.0f) {
            ManipulationManager mm = getManipulationManager();

            if (mm == null) {
                return;
            }
            prevVal = mm.moveSpeed;
            mm.moveSpeed = val;
        } else {
            Debug.LogError("Wrong speed value");
            return;
        }
        UnityMolMain.recordPythonCommand("setMouseMoveSpeed(" + val.ToString("F3", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("setMouseMoveSpeed(" + prevVal.ToString("F3", culture) + ")");
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
    /// Turn docking mode on and off
    /// </summary>
    public static void switchDockingMode() {
        DockingManager dm = UnityMolMain.getDockingManager();

        if (dm.isRunning) {
            dm.stopDockingMode();
        } else {
            dm.startDockingMode();
        }
        UnityMolMain.recordPythonCommand("switchDockingMode()");
        UnityMolMain.recordUndoPythonCommand("switchDockingMode()");
    }

    /// <summary>
    /// Set Raytracing material type for a given representation of a given selection
    /// Possible values of 'matType' are : 0 = Principled / 1 = carPaint / 2 = metal / 3 = alloy /
    /// 4 = glass / 5 = thinGlass / 6 = metallicPaint / 7 = luminous
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of representation</param>
    /// <param name="matType">the type of material</param>
    public static void setRTMaterialType(string selName, string type, int matType) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {
                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            if (sr.atomRepManager != null) {
                                sr.atomRepManager.SetRTMaterialType(matType);
                            }
                            if (sr.bondRepManager != null) {
                                sr.bondRepManager.SetRTMaterialType(matType);
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

        UnityMolMain.recordPythonCommand("setRTMaterialType(\"" + selName + "\", \"" + type + "\", " + matType + ")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Set Raytracing material property for a given representation of a given selection
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of representation</param>
    /// <param name="propName">the name of the property</param>
    /// <param name="val">the value of the property</param>
    public static void setRTMaterialProperty(string selName, string type, string propName, object val) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {
                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null) {
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            if (sr.atomRepManager != null) {
                                sr.atomRepManager.SetRTMaterialProperty(propName, val);
                            }
                            if (sr.bondRepManager != null) {
                                sr.bondRepManager.SetRTMaterialProperty(propName, val);
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

        string command = "setRTMaterialProperty(\"" + selName + "\", \"" + type + "\", \"" + propName + "\"";
        if (val is string) {
            command += ", \"" + val + "\")";
        } else if (val is float || val is double) {
            command += ", float(" + ((float) val).ToString("F3", culture) + "))";
        } else if (val is Vector3) {
            Vector3 v = (Vector3) val;
            command += ", " + cVec3ToPy(v) + ")";
        } else if (val is bool) {
            bool v = (bool) val;
            command += ", " + cBoolToPy(v) + ")";
        }
        else {
            command += ", " + val + ")";
        }

        UnityMolMain.recordPythonCommand(command);
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Read a Raytracing material(s) from a json file (VTK material files) and store it in the RT material bank
    /// </summary>
    /// <param name="filePath">path of the JSON file</param>
    public static void loadRTMaterialsJSONFile(string filePath) {
        string realPath = filePath;
        string customPath = Path.Combine(path, filePath);
        if (File.Exists(customPath)) {
            realPath = customPath;
        }

        ReadOSPRayMaterialJson.readRTMatJson(realPath);

        UnityMolMain.recordPythonCommand("loadRTMaterialsJSONFile(" + cStringToPy(filePath) + ")");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Set a Raytracing material for a representation of a given selection.
    /// Name of the material is taken from the RT material bank
    /// </summary>
    /// <param name="selName">The selection name concerned</param>
    /// <param name="type">The type of representation</param>
    /// <param name="matName">the name of the material</param>
    public static void setRTMaterial(string selName, string type, string matName) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        if (selM.selections.ContainsKey(selName)) {
            RepType repType = getRepType(type);
            if (repType.atomType != AtomType.noatom || repType.bondType != BondType.nobond) {
                List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, repType);
                if (existingReps != null && RaytracingMaterial.materialsBank.ContainsKey(matName)) {
                    RaytracingMaterial curMat = RaytracingMaterial.materialsBank[matName];
                    foreach (UnityMolRepresentation existingRep in existingReps) {
                        foreach (SubRepresentation sr in existingRep.subReps) {
                            if (sr.atomRepManager != null) {
                                sr.atomRepManager.SetRTMaterial(curMat);
                            }
                            if (sr.bondRepManager != null) {
                                sr.bondRepManager.SetRTMaterial(curMat);
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

        UnityMolMain.recordPythonCommand("setRTMaterial(" + cStringToPy(selName) + ", " + cStringToPy(type) + ", " + cStringToPy(matName) + ")");
        UnityMolMain.recordUndoPythonCommand("");
    }


    /// <summary>
    /// Enable or disable the RayTracing denoiser
    /// </summary>
    /// <param name="turnOn">enable if true, disable if false</param>
    public static void switchRTDenoiser(bool turnOn) {
        if (UnityMolMain.raytracingMode) {
            if (turnOn) {
                RaytracerManager.Instance.forceDenoiserOff(false);
            } else {
                RaytracerManager.Instance.forceDenoiserOff(true);
            }
        }
        UnityMolMain.recordPythonCommand("switchRTDenoiser(" + cBoolToPy(turnOn) + ")");
        UnityMolMain.recordUndoPythonCommand("switchRTDenoiser(" + cBoolToPy(!turnOn) + ")");
    }


    /// <summary>
    /// Return a RepType object from the name of the type of representation encoded in a string
    /// If no match is found, a Reptype object with no representation for atom and bond will be return
    /// </summary>
    /// <list type="bullet">
    /// <item> Values accepted for "Cartoon": "c", "cartoon"</item>
    /// <item> Values accepted for "Surface": "s", "surf", "surface"</item>
    /// <item> Values accepted for "Density map": "dxiso"</item>
    /// <item> Values accepted for "HyperBall": "hb", "hyperball", "hyperballs"</item>
    /// <item> Values accepted for "BondOrder": "bondorder"</item>
    /// <item> Values accepted for "Sphere": "sphere", "spheres"</item>
    /// <item> Values accepted for "lines": "l", "line", "lines"</item>
    /// <item> Values accepted for "H-bond": "hbond", "hbonds"</item>
    /// <item> Values accepted for "Tube H-bond": "hbondtube", "hbondtubes"</item>
    /// <item> Values accepted for "Fieldlines": "fl", "fieldline", "fieldlines"</item>
    /// <item> Values accepted for "Tube": "tube", "trace"</item>
    /// <item> Values accepted for "Sugar": "sugar", "sugarribbons"</item>
    /// <item> Values accepted for "Sheherasade": "sheherasade"</item>
    /// <item> Values accepted for "ellipsoid": "ellipsoid"</item>
    /// <item> Values accepted for "point": "p", "point", "points"</item>
    /// <item> Values accepted for "Exploded surface": "explo", "exploded","explodedsurface", "explosurface", "explosurf"</item>
    /// </list>
    /// <param name="type">the name of the type of representation</param>
    /// <returns>a RepType object corresponding</returns>
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
        case "hyperball":
        case "hyperballs":
            atype = AtomType.optihb;
            btype = BondType.optihs;
            break;
        case "bondorder":
            atype = AtomType.bondorder;
            btype = BondType.bondorder;
            break;
        case "sphere":
        case "spheres":
            atype = AtomType.sphere;
            btype = BondType.nobond;
            break;

        case "l":
        case "line":
        case "lines":
            atype = AtomType.noatom;
            btype = BondType.line;
            break;
        case "hbond":
        case "hbonds":
            atype = AtomType.noatom;
            btype = BondType.hbond;
            break;
        case "hbondtube":
        case "hbondtubes":
            atype = AtomType.noatom;
            btype = BondType.hbondtube;
            break;
        case "fl":
        case "fieldlines":
        case "fieldline":
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
        case "sheherasade":
            atype = AtomType.sheherasade;
            btype = BondType.nobond;
            break;
        case "ellipsoid":
            atype = AtomType.ellipsoid;
            btype = BondType.nobond;
            break;
        case "p":
        case "point":
        case "points":
            atype = AtomType.point;
            btype = BondType.nobond;
            break;
        case "explo":
        case "exploded":
        case "explodedsurface":
        case "explosurface":
        case "explosurf":
            atype = AtomType.explosurf;
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
    /// From a RepType object, return the type of representation as a string
    /// If no match is found, return an empty string
    /// </summary>
    /// <param name="rept">the repType object</param>
    /// <returns>the corresponding string</returns>
    public static string getTypeFromRepType(RepType rept) {
        if (rept.atomType == AtomType.cartoon && rept.bondType == BondType.nobond) {
            return "cartoon";
        }

        if (rept.atomType == AtomType.surface && rept.bondType == BondType.nobond) {
            return "surface";
        }

        if (rept.atomType == AtomType.DXSurface && rept.bondType == BondType.nobond) {
            return "dxiso";
        }

        if (rept.atomType == AtomType.optihb && rept.bondType == BondType.optihs) {
            return "hyperball";
        }

        if (rept.atomType == AtomType.bondorder && rept.bondType == BondType.bondorder) {
            return "bondorder";
        }

        if (rept.atomType == AtomType.sphere && rept.bondType == BondType.nobond) {
            return "sphere";
        }

        if (rept.atomType == AtomType.noatom && rept.bondType == BondType.line) {
            return "line";
        }

        if (rept.atomType == AtomType.noatom && rept.bondType == BondType.hbond) {
            return "hbond";
        }

        if (rept.atomType == AtomType.noatom && rept.bondType == BondType.hbondtube) {
            return "hbondtube";
        }

        if (rept.atomType == AtomType.fieldlines && rept.bondType == BondType.nobond) {
            return "fieldlines";
        }

        if (rept.atomType == AtomType.trace && rept.bondType == BondType.nobond) {
            return "trace";
        }

        if (rept.atomType == AtomType.sugarribbons && rept.bondType == BondType.nobond) {
            return "sugarribbons";
        }

        if (rept.atomType == AtomType.sheherasade && rept.bondType == BondType.nobond) {
            return "sheherasade";
        }

        if (rept.atomType == AtomType.ellipsoid && rept.bondType == BondType.nobond) {
            return "ellipsoid";
        }

        if (rept.atomType == AtomType.point && rept.bondType == BondType.nobond) {
            return "point";
        }

        if (rept.atomType == AtomType.explosurf && rept.bondType == BondType.nobond) {
            return "explo";
        }

        Debug.LogWarning("Not a predefined type");
        return "";
    }

    /// <summary>
    /// Return a boolean string depending on the boolean 'val'
    /// </summary>
    /// <param name="val">the boolean value</param>
    /// <returns>"True" if true. "False" if false</returns>
    public static string cBoolToPy(bool val) {
        if (val) {
            return "True";
        }
        return "False";
    }

    /// <summary>
    /// Return a Vector 3 string from the Vector3 value 'val'
    /// </summary>
    /// <param name="val">the value</param>
    /// <returns>the string representation</returns>
    private static string cVec3ToPy(Vector3 val) {
        return "Vector3(" + val.x.ToString("F3", culture) + ", " +
               val.y.ToString("F3", culture) + ", " +
               val.z.ToString("F3", culture) + ")";
    }

    /// <summary>
    /// Return a string from the string value 's'
    /// </summary>
    /// <param name="s">the string</param>
    /// <returns>the string with quote</returns>
    private static string cStringToPy(string s) {
        return "\"" + s + "\"";
    }

    /// <summary>
    /// Activate the TCP server command.
    /// Allow to receive external commands from the TCP socket.
    /// </summary>
    public static void activateExternalCommands() {

        if (extCom != null) {
            DestroyImmediate(extCom);
        }
        extCom = instance.gameObject.AddComponent<TCPServerCommand>();

        UnityMolMain.recordPythonCommand("activateExternalCommands()");
        UnityMolMain.recordUndoPythonCommand("disableExternalCommands()");
    }

    /// <summary>
    /// Log some information for a TCP server command.
    /// Public method to add messages to captured logs from external routines
    /// <param name="message">the string to be added to the log/param>
    /// </summary>
    public static void addExternalCommandLogMessage(string message) {
        if (extCom != null) {
            extCom.AddLogMessage(message);
        }
    }

    /// <summary>
    /// Disable the TCP server command.
    /// </summary>
    public static void disableExternalCommands() {
        if (extCom != null) {
            DestroyImmediate(extCom);
        }
        extCom = null;
        UnityMolMain.recordPythonCommand("disableExternalCommands()");
        UnityMolMain.recordUndoPythonCommand("activateExternalCommands()");
    }

    /// <summary>
    /// Return as a string the list of selections name
    /// </summary>
    /// <example>[sel1, sel2, sel3]</example>
    /// <returns>the string</returns>
    public static string getSelectionListString() {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        StringBuilder sb = new();
        sb.Append("[");
        int N = selM.selections.Count;
        int id = 0;
        foreach (UnityMolSelection sel in selM.selections.Values) {
            sb.Append(sel.name);
            if (id != N - 1) {
                sb.Append(", ");
            }

            id++;
        }
        sb.Append("]");
        return sb.ToString();
    }

    /// <summary>
    /// Return as a string the list of structures name
    /// </summary>
    /// <example>[struct1, struct2, struct3]</example>
    /// <returns>the string</returns>
    public static string getStructureListString() {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        StringBuilder sb = new();
        sb.Append("[");
        int N = sm.loadedStructures.Count;
        int id = 0;
        foreach (UnityMolStructure s in sm.loadedStructures) {
            sb.Append(s.name);
            if (id != N - 1) {
                sb.Append(", ");
            }

            id++;
        }
        sb.Append("]");
        return sb.ToString();
    }


    /// <summary>
    /// Stop the Tour feature
    /// </summary>
    public static void clearTour() {
        getManipulationManager().clearTour();
    }

    /// <summary>
    /// Add a selection to the Tour.
    /// </summary>
    /// <param name="selName">the selection name concerned</param>
    public static void addSelectionToTour(string selName) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (selM.selections.ContainsKey(selName)) {
            ManipulationManager mm = getManipulationManager();
            mm.addTour(selM.selections[selName]);
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }
        UnityMolMain.recordPythonCommand("addSelectionToTour(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("removeSelectionFromTour(\"" + selName + "\")");
    }

    /// <summary>
    /// Remove a selection to the Tour.
    /// </summary>
    /// <param name="selName">the selection name concerned</param>
    public static void removeSelectionFromTour(string selName) {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (selM.selections.ContainsKey(selName)) {
            ManipulationManager mm = getManipulationManager();
            mm.removeFromTour(selM.selections[selName]);
        } else {
            Debug.LogWarning("No selection named '" + selName + "'");
            return;
        }
        UnityMolMain.recordPythonCommand("removeSelectionFromTour(\"" + selName + "\")");
        UnityMolMain.recordUndoPythonCommand("addSelectionToTour(\"" + selName + "\")");
    }

    //--------------- Annotations


    /// <summary>
    /// Set the measure mode for creating annotations
    /// Measure modes : 0 = distance, 1 = angle, 2 = torsion angle
    /// </summary>
    /// <param name="newMode">the measure mode</param>
    public static void setMeasureMode(int newMode) {
        if (newMode is < 0 or > 2) {
            Debug.LogError("Measure mode should be between 0 and 2");
            return;
        }
        int prevVal = (int) UnityMolMain.measureMode;
        UnityMolMain.measureMode = (MeasureMode) newMode;

        UnityMolMain.recordPythonCommand("setMeasureMode(" + newMode + ")");
        UnityMolMain.recordUndoPythonCommand("setMeasureMode(" + prevVal + ")");
    }

    /// <summary>
    /// Annotate an atom by creating a surrounding sphere around it
    /// </summary>
    /// <param name="structureName">the structure which belongs the atom</param>
    /// <param name="atomId">the atom ID</param>
    public static void annotateAtom(string structureName, int atomId) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (s == null) {
            Debug.LogError("Wrong structure");
            return;
        }
        UnityMolAtom a = s.currentModel.getAtomWithID(atomId);
        if (a != null) {
            anM.Annotate(a);
        } else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("annotateAtom(\"" + structureName + "\", " + atomId + ")");
        UnityMolMain.recordUndoPythonCommand("removeAnnotationAtom(\"" + structureName + "\", " + atomId + ")");
    }

    /// <summary>
    /// Remove the annotation of an atom
    /// </summary>
    /// <param name="structureName">the structure which belongs the atom</param>
    /// <param name="atomId">the atom ID</param>
    public static void removeAnnotationAtom(string structureName, int atomId) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (s == null) {
            Debug.LogError("Wrong structure");
            return;
        }
        UnityMolAtom a = s.currentModel.getAtomWithID(atomId);

        if (a != null) {
            SphereAnnotation sa = new();
            sa.atoms.Add(a);
            anM.RemoveAnnotation(sa);
        } else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("removeAnnotationAtom(\"" + structureName + "\", " + atomId + ")");
        UnityMolMain.recordUndoPythonCommand("annotateAtom(\"" + structureName + "\", " + atomId + ")");
    }

    /// <summary>
    /// Create an annotation sphere
    /// </summary>
    /// <param name="worldP">the center position of the sphere (World reference)</param>
    /// <param name="scale">the scale of the sphere</param>
    public static void annotateSphere(Vector3 worldP, float scale = 1.0f) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();

        GameObject tmpSpherePar = new("WorldSphereAnnotation_" + worldP.x.ToString("F3", culture) + "_" +
                                      worldP.y.ToString("F3", culture) + "_" + worldP.z.ToString("F3", culture))
        {
            transform = {
                position = worldP
            }
        };

        anM.AnnotateSphere(tmpSpherePar.transform, scale);

        UnityMolMain.recordPythonCommand("annotateSphere(" + cVec3ToPy(worldP) + ", " + scale.ToString("F3", culture) + ")");

        UnityMolMain.recordUndoPythonCommand("removeAnnotationSphere(" + cVec3ToPy(worldP) + ", " + scale.ToString("F3", culture) + ")");
    }

    /// <summary>
    /// Remove an annotation sphere of the given parameters
    /// </summary>
    /// <param name="worldP">the center position of the sphere (World reference)</param>
    /// <param name="scale">the scale of the sphere</param>
    public static void removeAnnotationSphere(Vector3 worldP, float scale = 1.0f) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();

        SphereAnnotation sa = new();
        GameObject tmpSpherePar = new("WorldSphereAnnotation_" + worldP.x.ToString("F3", culture) + "_" +
                                      worldP.y.ToString("F3", culture) + "_" + worldP.z.ToString("F3", culture))
        {
            transform = {
                parent = UnityMolMain.getRepresentationParent().transform
            }
        };

        sa.annoParent = tmpSpherePar.transform;
        anM.RemoveAnnotation(sa);

        Destroy(tmpSpherePar);

        UnityMolMain.recordPythonCommand("removeAnnotationSphere(" + cVec3ToPy(worldP) + ", " + scale.ToString("F3", culture) + ")");
        UnityMolMain.recordUndoPythonCommand("annotateSphere(" + cVec3ToPy(worldP) + ", " + scale.ToString("F3", culture) + ")");
    }

    /// <summary>
    /// Annotate an atom by with a text
    /// </summary>
    /// <param name="structureName">the structure which belongs the atom</param>
    /// <param name="atomId">the atom ID</param>
    /// <param name="text">The text of the annotation</param>
    /// <param name="textCol">the color of the text</param>
    /// <param name="showLine">display lines around the annotation?</param>
    public static void annotateAtomText(string structureName, int atomId, string text, Color textCol, bool showLine = false) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (s == null) {
            Debug.LogError("Wrong structure");
            return;
        }

        UnityMolAtom a = s.currentModel.getAtomWithID(atomId);
        if (a != null) {
            anM.AnnotateText(a, text, textCol, showLine);
        } else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("annotateAtomText(\"" + structureName + "\", " + atomId + ", \"" + text + "\", " + cBoolToPy(showLine) + ")");
        UnityMolMain.recordUndoPythonCommand("removeAnnotationAtomText(\"" + structureName + "\", " + atomId + ", \"" + text + "\")");
    }

    /// <summary>
    /// Remove the text annotation of an atom
    /// </summary>
    /// <param name="structureName">the structure which belongs the atom</param>
    /// <param name="atomId">the atom ID</param>
    /// <param name="text">The text of the annotation</param>
    public static void removeAnnotationAtomText(string structureName, int atomId, string text) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (s == null) {
            Debug.LogError("Wrong structure");
            return;
        }
        UnityMolAtom a = s.currentModel.getAtomWithID(atomId);
        if (a != null) {
            TextAnnotation sa = new();
            sa.atoms.Add(a);
            sa.content = text;
            anM.RemoveAnnotation(sa);
        } else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("removeAnnotationAtomText(\"" + structureName + "\", " + atomId + ", \"" + text + "\")");
        UnityMolMain.recordUndoPythonCommand("annotateAtomText(\"" + structureName + "\", " + atomId + ", \"" + text + "\")");
    }

    /// <summary>
    /// Create a global text annotation in the scene
    /// </summary>
    /// <param name="worldP">the center position of the annotation (World reference)</param>
    /// <param name="scale">the scale of the annotation</param>
    /// <param name="text">the text of the annotation</param>
    /// <param name="textCol">the color of the text</param>
    public static void annotateWorldText(Vector3 worldP, float scale, string text, Color textCol) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();

        GameObject tmpTextPar = new("WorldTextAnnotation_" + worldP.x.ToString("F3", culture) + "_" +
                                    worldP.y.ToString("F3", culture) + "_" + worldP.z.ToString("F3", culture))
            {
                transform = {
                    parent = UnityMolMain.getRepresentationParent().transform.parent,
                    localPosition = worldP,
                    localRotation = Quaternion.identity,
                    localScale = Vector3.one
                }
            };

        anM.AnnotateWorldText(tmpTextPar.transform, scale, text, textCol);

        UnityMolMain.recordPythonCommand("annotateWorldText(" + cVec3ToPy(worldP) + ", " + scale.ToString("F3", culture) + ", \"" + text + "\", " +
                                         string.Format(culture, "{0})", textCol));

        UnityMolMain.recordUndoPythonCommand("removeAnnotationWorldText(" + cVec3ToPy(worldP) + ", " + scale.ToString("F3", culture) + ", \"" + text + "\", " +
                                             string.Format(culture, "{0})", textCol));
    }

    /// <summary>
    /// Remove a global text annotation of the scene
    /// </summary>
    /// <param name="worldP">the center position of the annotation (World reference)</param>
    /// <param name="scale">the scale of the annotation</param>
    /// <param name="text">the text of the annotation</param>
    /// <param name="textCol">the color of the text</param>
    public static void removeAnnotationWorldText(Vector3 worldP, float scale, string text, Color textCol) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();

        CustomTextAnnotation ta = new();

        GameObject tmpTextPar = new("WorldTextAnnotation_" + worldP.x.ToString("F3", culture) + "_" +
                                    worldP.y.ToString("F3", culture) + "_" + worldP.z.ToString("F3", culture));

        ta.annoParent = tmpTextPar.transform;

        ta.content = text;
        anM.RemoveAnnotation(ta);

        Destroy(tmpTextPar);

        UnityMolMain.recordPythonCommand("removeAnnotationWorldText(" + cVec3ToPy(worldP) + ", " + scale.ToString("F3", culture) + ", \"" + text + "\", " +
                                         string.Format(CultureInfo.InvariantCulture, "{0})", textCol));

        UnityMolMain.recordUndoPythonCommand("annotateWorldText(" + cVec3ToPy(worldP) + ", " + scale.ToString("F3", culture) + ", \"" + text + "\", " +
                                             string.Format(CultureInfo.InvariantCulture, "{0})", textCol));
    }

    /// <summary>
    /// Add a 2D annotation text over everything
    /// The screenP defines the position based on the percentage from bottom/left to top/right of the screen
    /// with 0/0 means bottom/left and 1/1 means top/right
    /// </summary>
    /// <param name="screenP">the position of the annotation</param>
    /// <param name="scale">the scale of the annotation</param>
    /// <param name="text">the text of the annotation</param>
    /// <param name="textCol">the color of the text</param>
    public static void annotate2DText(Vector2 screenP, float scale, string text, Color textCol) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();


        anM.Annotate2DText(text, scale, textCol, screenP);

        UnityMolMain.recordPythonCommand("annotate2DText(Vector2(" + screenP.x.ToString("F3", culture) + ", " +
                                         screenP.y.ToString("F3", culture) + "), " + scale.ToString("F3", culture) + ", \"" + text + "\", " +
                                         string.Format(CultureInfo.InvariantCulture, "{0})", textCol));

        UnityMolMain.recordUndoPythonCommand("removeAnnotation2DText(Vector2(" + screenP.x.ToString("F3", culture) + ", " +
                                             screenP.y.ToString("F3", culture) + "), " + scale.ToString("F3", culture) + ", \"" + text + "\", " +
                                             string.Format(CultureInfo.InvariantCulture, "{0})", textCol));
    }

    /// <summary>
    /// Remove a 2D annotation text over everything
    /// The screenP defines the position based on the percentage from bottom/left to top/right of the screen
    /// with 0/0 means bottom/left and 1/1 means top/right
    /// </summary>
    /// <param name="screenP">the position of the annotation</param>
    /// <param name="scale">the scale of the annotation</param>
    /// <param name="text">the text of the annotation</param>
    /// <param name="textCol">the color of the text</param>
    public static void removeAnnotation2DText(Vector2 screenP, float scale, string text, Color textCol) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();

        Annotate2D ta = new() {
            content = text,
            posPercent = screenP
        };

        anM.RemoveAnnotation(ta);

        UnityMolMain.recordPythonCommand("removeAnnotation2DText(Vector2(" + screenP.x.ToString("F3", culture) + ", " +
                                         screenP.y.ToString("F3", culture) + "), " + scale.ToString("F3", culture) + ", \"" + text + "\", " +
                                         string.Format(CultureInfo.InvariantCulture, "{0})", textCol));
        UnityMolMain.recordUndoPythonCommand("annotateWorldText(Vector2(" + screenP.x.ToString("F3", culture) + ", " +
                                             screenP.y.ToString("F3", culture) + "), " + scale.ToString("F3", culture) + ", \"" + text + "\", " +
                                             string.Format(CultureInfo.InvariantCulture, "{0})", textCol));
    }

    /// <summary>
    /// Create an annotation line between 2 atoms
    /// </summary>
    /// <param name="structureName">the structure which belongs the first atom</param>
    /// <param name="atomId">the first atom ID</param>
    /// <param name="structureName2">the structure which belongs the second atom</param>
    /// <param name="atomId2">the second atom ID</param>
    public static void annotateLine(string structureName, int atomId, string structureName2, int atomId2) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolStructure s2 = sm.GetStructure(structureName2);
        if (s == null || s2 == null) {
            Debug.LogError("Wrong structure");
            return;
        }
        UnityMolAtom a = s.currentModel.getAtomWithID(atomId);
        UnityMolAtom a2 = s2.currentModel.getAtomWithID(atomId2);
        if (a != null && a2 != null) {
            anM.AnnotateLine(a, a2);
        } else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("annotateLine(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
        UnityMolMain.recordUndoPythonCommand("removeAnnotationLine(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
    }

    /// <summary>
    /// Remove an annotation line between 2 atoms
    /// </summary>
    /// <param name="structureName">the structure which belongs the first atom</param>
    /// <param name="atomId">the first atom ID</param>
    /// <param name="structureName2">the structure which belongs the second atom</param>
    /// <param name="atomId2">the second atom ID</param>
    public static void removeAnnotationLine(string structureName, int atomId, string structureName2, int atomId2) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolStructure s2 = sm.GetStructure(structureName2);
        if (s == null || s2 == null) {
            Debug.LogError("Wrong structure");
            return;
        }

        UnityMolAtom a = s.currentModel.getAtomWithID(atomId);
        UnityMolAtom a2 = s2.currentModel.getAtomWithID(atomId2);
        if (a != null && a2 != null) {
            LineAtomAnnotation la = new();
            la.atoms.Add(a);
            la.atoms.Add(a2);
            anM.RemoveAnnotation(la);
        } else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("removeAnnotationLine(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
        UnityMolMain.recordUndoPythonCommand("annotateLine(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
    }

    /// <summary>
    /// Add a global annotation line between 2 positions.
    /// </summary>
    /// <param name="p1">the starting position</param>
    /// <param name="p2">the ending position</param>
    /// <param name="sizeLine">the size of the line</param>
    /// <param name="lineCol">the color of the line</param>
    public static void annotateWorldLine(Vector3 p1, Vector3 p2, float sizeLine, Color lineCol) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();

        anM.AnnotateWorldLine(p1, p2, UnityMolMain.getRepresentationParent().transform.parent, sizeLine, lineCol);

        UnityMolMain.recordPythonCommand("annotateWorldLine(" +  cVec3ToPy(p1) + ", " +
                                         cVec3ToPy(p2) + ", " +
                                         sizeLine.ToString("F3", culture) +
                                         string.Format(CultureInfo.InvariantCulture, ", {0})", lineCol));

        UnityMolMain.recordPythonCommand("removeWorldAnnotationLine(" + cVec3ToPy(p1) + ", " + cVec3ToPy(p2) + ", " +
                                         sizeLine.ToString("F3", culture) +
                                         string.Format(CultureInfo.InvariantCulture, ", {0})", lineCol));

    }

    /// <summary>
    /// Remove a global annotation line between 2 positions.
    /// </summary>
    /// <param name="p1">the starting position</param>
    /// <param name="p2">the ending position</param>
    /// <param name="sizeLine">the size of the line</param>
    /// <param name="lineCol">the color of the line</param>
    public static void removeWorldAnnotationLine(Vector3 p1, Vector3 p2, float sizeLine, Color lineCol) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();

        CustomLineAnnotation la = new() {
            start = p1,
            end = p2
        };
        anM.RemoveAnnotation(la);
        UnityMolMain.recordPythonCommand("removeWorldAnnotationLine(" + cVec3ToPy(p1) + ", " + cVec3ToPy(p2) + ", " +
                                         sizeLine.ToString("F3", culture) +
                                         string.Format(CultureInfo.InvariantCulture, ", {0})", lineCol));

        UnityMolMain.recordPythonCommand("annotateWorldLine(" + cVec3ToPy(p1) + ", " + cVec3ToPy(p2) + ", " +
                                         sizeLine.ToString("F3", culture) +
                                         string.Format(CultureInfo.InvariantCulture, ", {0})", lineCol));
    }

    /// <summary>
    /// Create an annotation of type "Distance" between 2 atoms : draw a line between the same and add a text for the distance
    /// </summary>
    /// <param name="structureName">the structure which belongs the first atom</param>
    /// <param name="atomId">the first atom ID</param>
    /// <param name="structureName2">the structure which belongs the second atom</param>
    /// <param name="atomId2">the second atom ID</param>
    public static void annotateDistance(string structureName, int atomId, string structureName2, int atomId2) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolStructure s2 = sm.GetStructure(structureName2);
        if (s == null || s2 == null) {
            Debug.LogError("Wrong structure");
            return;
        }

        UnityMolAtom a = s.currentModel.getAtomWithID(atomId);
        UnityMolAtom a2 = s2.currentModel.getAtomWithID(atomId2);
        if (a != null && a2 != null) {
            anM.AnnotateDistance(a, a2);
        } else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("annotateDistance(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
        UnityMolMain.recordUndoPythonCommand("removeAnnotationDistance(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
    }

    /// <summary>
    /// Remove an annotation of type "Distance" between 2 atoms
    /// </summary>
    /// <param name="structureName">the structure which belongs the first atom</param>
    /// <param name="atomId">the first atom ID</param>
    /// <param name="structureName2">the structure which belongs the second atom</param>
    /// <param name="atomId2">the second atom ID</param>
    public static void removeAnnotationDistance(string structureName, int atomId, string structureName2, int atomId2) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolStructure s2 = sm.GetStructure(structureName2);
        if (s == null || s2 == null) {
            Debug.LogError("Wrong structure");
            return;
        }

        UnityMolAtom a = s.currentModel.getAtomWithID(atomId);
        UnityMolAtom a2 = s2.currentModel.getAtomWithID(atomId2);
        if (a != null && a2 != null) {
            DistanceAnnotation da = new();
            da.atoms.Add(a);
            da.atoms.Add(a2);
            anM.RemoveAnnotation(da);
        } else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("removeAnnotationDistance(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
        UnityMolMain.recordUndoPythonCommand("annotateDistance(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
    }

    /// <summary>
    /// Create an annotation of type "Angle" between 3 atoms.
    /// It adds a surrounding sphere around atoms and add a text for the angle value.
    /// </summary>
    /// <param name="structureName">the structure which belongs the first atom</param>
    /// <param name="atomId">the first atom ID</param>
    /// <param name="structureName2">the structure which belongs the second atom</param>
    /// <param name="atomId2">the second atom ID</param>
    /// <param name="structureName3">the structure which belongs the third atom</param>
    /// <param name="atomId3">the third atom ID</param>
    public static void annotateAngle(string structureName, int atomId, string structureName2, int atomId2,
        string structureName3, int atomId3) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolStructure s2 = sm.GetStructure(structureName2);
        UnityMolStructure s3 = sm.GetStructure(structureName3);

        if (s == null || s2 == null || s3 == null) {
            Debug.LogError("Wrong structure");
            return;
        }

        UnityMolAtom a = s.currentModel.getAtomWithID(atomId);
        UnityMolAtom a2 = s2.currentModel.getAtomWithID(atomId2);
        UnityMolAtom a3 = s3.currentModel.getAtomWithID(atomId3);
        if (a != null && a2 != null && a3 != null) {
            anM.AnnotateAngle(a, a2, a3);
        } else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("annotateAngle(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " +
                                         atomId2 + ", \"" + structureName3 + "\", " + atomId3 + ")");
        UnityMolMain.recordUndoPythonCommand("removeAnnotationAngle(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " +
                                             atomId2 + ", \"" + structureName3 + "\", " + atomId3 + ")");
    }

    /// <summary>
    /// Remove an annotation of type "Angle" between 3 atoms.
    /// </summary>
    /// <param name="structureName">the structure which belongs the first atom</param>
    /// <param name="atomId">the first atom ID</param>
    /// <param name="structureName2">the structure which belongs the second atom</param>
    /// <param name="atomId2">the second atom ID</param>
    /// <param name="structureName3">the structure which belongs the third atom</param>
    /// <param name="atomId3">the third atom ID</param>
    public static void removeAnnotationAngle(string structureName, int atomId, string structureName2, int atomId2,
        string structureName3, int atomId3) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolStructure s2 = sm.GetStructure(structureName2);
        UnityMolStructure s3 = sm.GetStructure(structureName3);

        if (s == null || s2 == null || s3 == null) {
            Debug.LogError("Wrong structure");
            return;
        }

        UnityMolAtom a = s.currentModel.getAtomWithID(atomId);
        UnityMolAtom a2 = s2.currentModel.getAtomWithID(atomId2);
        UnityMolAtom a3 = s3.currentModel.getAtomWithID(atomId3);
        if (a != null && a2 != null && a3 != null) {

            AngleAnnotation aa = new();
            aa.atoms.Add(a);
            aa.atoms.Add(a2);
            aa.atoms.Add(a3);
            anM.RemoveAnnotation(aa);
        } else {
            Debug.LogError("Wrong atom id");
            return;
        }

        UnityMolMain.recordPythonCommand("removeAnnotationAngle(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " +
                                         atomId2 + ", \"" + structureName3 + "\", " + atomId3 + ")");
        UnityMolMain.recordUndoPythonCommand("annotateAngle(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " +
                                             atomId2 + ", \"" + structureName3 + "\", " + atomId3 + ")");
    }

    /// <summary>
    /// Create an annotation of type "Dihedral" between 4 atoms.
    /// It adds a surrounding sphere around atoms and add a text for the angle value.
    /// </summary>
    /// <param name="structureName">the structure which belongs the first atom</param>
    /// <param name="atomId">the first atom ID</param>
    /// <param name="structureName2">the structure which belongs the second atom</param>
    /// <param name="atomId2">the second atom ID</param>
    /// <param name="structureName3">the structure which belongs the third atom</param>
    /// <param name="atomId3">the third atom ID</param>
    /// <param name="structureName4">the structure which belongs the fourth atom</param>
    /// <param name="atomId4">the fourth atom ID</param>
    public static void annotateDihedralAngle(string structureName, int atomId, string structureName2, int atomId2,
        string structureName3, int atomId3, string structureName4, int atomId4) {

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

        if (s == null || s2 == null || s3 == null || s4 == null) {
            Debug.LogError("Wrong structure");
            return;
        }

        UnityMolAtom a = s.currentModel.getAtomWithID(atomId);
        UnityMolAtom a2 = s2.currentModel.getAtomWithID(atomId2);
        UnityMolAtom a3 = s3.currentModel.getAtomWithID(atomId3);
        UnityMolAtom a4 = s4.currentModel.getAtomWithID(atomId4);
        if (a != null && a2 != null && a3 != null && a4 != null) {
            anM.AnnotateDihedralAngle(a, a2, a3, a4);
        } else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("annotateDihedralAngle(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " +
                                         atomId2 + ", \"" + structureName3 + "\", " + atomId3 + ", \"" + structureName4 + "\", " + atomId4 + ")");
        UnityMolMain.recordUndoPythonCommand("removeAnnotationDihedralAngle(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " +
                                             atomId2 + ", \"" + structureName3 + "\", " + atomId3 + ", \"" + structureName4 + "\", " + atomId4 + ")");
    }

    /// <summary>
    /// Remove an annotation of type "Dihedral" between 4 atoms.
    /// </summary>
    /// <param name="structureName">the structure which belongs the first atom</param>
    /// <param name="atomId">the first atom ID</param>
    /// <param name="structureName2">the structure which belongs the second atom</param>
    /// <param name="atomId2">the second atom ID</param>
    /// <param name="structureName3">the structure which belongs the third atom</param>
    /// <param name="atomId3">the third atom ID</param>
    /// <param name="structureName4">the structure which belongs the fourth atom</param>
    /// <param name="atomId4">the fourth atom ID</param>
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

        if (s == null || s2 == null || s3 == null || s4 == null) {
            Debug.LogError("Wrong structure");
            return;
        }

        UnityMolAtom a = s.currentModel.getAtomWithID(atomId);
        UnityMolAtom a2 = s2.currentModel.getAtomWithID(atomId2);
        UnityMolAtom a3 = s3.currentModel.getAtomWithID(atomId3);
        UnityMolAtom a4 = s4.currentModel.getAtomWithID(atomId4);
        if (a != null && a2 != null && a3 != null && a4 != null) {

            TorsionAngleAnnotation ta = new();
            ta.atoms.Add(a);
            ta.atoms.Add(a2);
            ta.atoms.Add(a3);
            ta.atoms.Add(a4);
            anM.RemoveAnnotation(ta);
        } else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("removeAnnotationDihedralAngle(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " +
                                         atomId2 + ", \"" + structureName3 + "\", " + atomId3 + ", \"" + structureName4 + "\", " + atomId4 + ")");
        UnityMolMain.recordUndoPythonCommand("annotateDihedralAngle(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " +
                                             atomId2 + ", \"" + structureName3 + "\", " + atomId3 + ", \"" + structureName4 + "\", " + atomId4 + ")");
    }

    /// <summary>
    /// Create an annotation of type "arrow" between 2 atoms
    /// </summary>
    /// <param name="structureName">the structure which belongs the first atom</param>
    /// <param name="atomId">the first atom ID</param>
    /// <param name="structureName2">the structure which belongs the second atom</param>
    /// <param name="atomId2">the second atom ID</param>
    public static void annotateRotatingArrow(string structureName, int atomId, string structureName2, int atomId2) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolStructure s2 = sm.GetStructure(structureName2);
        if (s == null || s2 == null) {
            Debug.LogError("Wrong structure");
            return;
        }

        UnityMolAtom a = s.currentModel.getAtomWithID(atomId);
        UnityMolAtom a2 = s2.currentModel.getAtomWithID(atomId2);
        if (a != null && a2 != null) {
            anM.AnnotateDihedralArrow(a, a2);
        } else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("annotateRotatingArrow(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
        UnityMolMain.recordUndoPythonCommand("removeAnnotationRotatingArrow(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
    }

    /// <summary>
    /// Remove an annotation of type "arrow" between 2 atoms
    /// </summary>
    /// <param name="structureName">the structure which belongs the first atom</param>
    /// <param name="atomId">the first atom ID</param>
    /// <param name="structureName2">the structure which belongs the second atom</param>
    /// <param name="atomId2">the second atom ID</param>
    public static void removeAnnotationRotatingArrow(string structureName, int atomId, string structureName2, int atomId2) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        UnityMolStructure s2 = sm.GetStructure(structureName2);
        if (s == null || s2 == null) {
            Debug.LogError("Wrong structure");
            return;
        }

        UnityMolAtom a = s.currentModel.getAtomWithID(atomId);
        UnityMolAtom a2 = s2.currentModel.getAtomWithID(atomId2);
        if (a != null && a2 != null) {
            ArrowAnnotation aa = new();
            aa.atoms.Add(a);
            aa.atoms.Add(a2);
            anM.RemoveAnnotation(aa);
        } else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("removeAnnotationRotatingArrow(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
        UnityMolMain.recordUndoPythonCommand("annotateRotatingArrow(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " + atomId2 + ")");
    }

    /// <summary>
    /// Create an annotation of type "ArcLine" between 3 atoms.
    /// </summary>
    /// <param name="structureName">the structure which belongs the first atom</param>
    /// <param name="atomId">the first atom ID</param>
    /// <param name="structureName2">the structure which belongs the second atom</param>
    /// <param name="atomId2">the second atom ID</param>
    /// <param name="structureName3">the structure which belongs the third atom</param>
    /// <param name="atomId3">the third atom ID</param>
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
        if (s == null || s2 == null || s3 == null) {
            Debug.LogError("Wrong structure");
            return;
        }

        UnityMolAtom a = s.currentModel.getAtomWithID(atomId);
        UnityMolAtom a2 = s2.currentModel.getAtomWithID(atomId2);
        UnityMolAtom a3 = s3.currentModel.getAtomWithID(atomId3);
        if (a != null && a2 != null && a3 != null) {
            anM.AnnotateCurvedLine(a, a2, a3);
        } else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("annotateArcLine(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " +
                                         atomId2 + ", \"" + structureName3 + "\", " + atomId3 + ")");
        UnityMolMain.recordUndoPythonCommand("removeAnnotationArcLine(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " +
                                             atomId2 + ", \"" + structureName3 + "\", " + atomId3 + ")");
    }

    /// <summary>
    /// Remove an annotation of type "ArcLine" between 3 atoms.
    /// </summary>
    /// <param name="structureName">the structure which belongs the first atom</param>
    /// <param name="atomId">the first atom ID</param>
    /// <param name="structureName2">the structure which belongs the second atom</param>
    /// <param name="atomId2">the second atom ID</param>
    /// <param name="structureName3">the structure which belongs the third atom</param>
    /// <param name="atomId3">the third atom ID</param>
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
        if (s == null || s2 == null || s3 == null) {
            Debug.LogError("Wrong structure");
            return;
        }

        UnityMolAtom a = s.currentModel.getAtomWithID(atomId);
        UnityMolAtom a2 = s2.currentModel.getAtomWithID(atomId2);
        UnityMolAtom a3 = s3.currentModel.getAtomWithID(atomId3);
        if (a != null && a2 != null && a3 != null) {

            ArcLineAnnotation aa = new();
            aa.atoms.Add(a);
            aa.atoms.Add(a2);
            aa.atoms.Add(a3);
            anM.RemoveAnnotation(aa);
        } else {
            Debug.LogError("Wrong atom id");
            return;
        }
        UnityMolMain.recordPythonCommand("removeAnnotationArcLine(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " +
                                         atomId2 + ", \"" + structureName3 + "\", " + atomId3 + ")");
        UnityMolMain.recordUndoPythonCommand("annotateArcLine(\"" + structureName + "\", " + atomId + ", \"" + structureName2 + "\", " +
                                             atomId2 + ", \"" + structureName3 + "\", " + atomId3 + ")");
    }

    /// <summary>
    /// Add an annotation of type "DrawLine" linked to a structure.
    /// </summary>
    /// <param name="structureName">the name of the structure concerned</param>
    /// <param name="line">the list of positions of the line</param>
    /// <param name="col">the color of the line</param>
    public static void annotateDrawLine(string structureName, List<Vector3> line, Color col) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }

        UnityMolStructure s = sm.GetStructure(structureName);
        if (s == null) {
            Debug.LogError("Wrong structure");
            return;
        }

        int id = anM.AnnotateDrawing(s, line, col);

        string command = "annotateDrawLine(\"" + structureName + "\",  List[Vector3]([";
        for (int i = 0; i < line.Count; i++) {
            command += cVec3ToPy(line[i]);
            if (i != line.Count - 1) {
                command += ", ";
            }
        }
        command += string.Format(CultureInfo.InvariantCulture, "]), {0}, )", col);
        UnityMolMain.recordPythonCommand(command);
        UnityMolMain.recordUndoPythonCommand("removeLastDrawLine(\"" + structureName + "\", " + id + ")");
    }

    /// <summary>
    /// Remove an annotation of type "DrawLine" linked to a structure.
    /// </summary>
    /// <param name="structureName">the name of the structure concerned</param>
    /// <param name="id">the id of the annotation</param>
    public static void removeLastDrawLine(string structureName, int id) {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }
        UnityMolStructure s = sm.GetStructure(structureName);
        if (s == null) {
            Debug.LogError("Wrong structure");
            return;
        }

        if (id != -1) {
            DrawAnnotation da = new();
            da.atoms.Add(s.currentModel.allAtoms[0]);
            da.id = id;
            anM.RemoveAnnotation(da);
        } else {
            Debug.LogError("Wrong structure name");
            return;
        }

        UnityMolMain.recordPythonCommand("removeLastDrawLine(\"" + structureName + "\", " + id + ")");
        UnityMolMain.recordUndoPythonCommand("");

    }

    /// <summary>
    /// Play a sonar sound at a world position
    /// </summary>
    /// <param name="wpos">the position</param>
    public static void playSoundAtPosition(Vector3 wpos) {
        GameObject go = Instantiate(Resources.Load("Prefabs/AudioSourceSonar")) as GameObject;
        if (go != null) {
            go.transform.position = wpos;

            UnityMolMain.recordPythonCommand("playSoundAtPosition(" + cVec3ToPy(wpos) + ")");
            UnityMolMain.recordUndoPythonCommand("");
        }
    }

    /// <summary>
    /// Remove all drawing annotations
    /// </summary>
    public static void clearDrawings() {
        UnityMolAnnotationManager am = UnityMolMain.getAnnotationManager();
        am.CleanDrawings();
        UnityMolMain.recordPythonCommand("clearDrawings()");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Remove all annotations + Drawings
    /// </summary>
    public static void clearAnnotations() {
        UnityMolMain.getAnnotationManager().Clean();
        UnityMolMain.recordPythonCommand("clearAnnotations()");
        UnityMolMain.recordUndoPythonCommand("");
    }

    /// <summary>
    /// Export the given structure to an OBJ file containing several meshes
    /// BondOrder/Point/Hbonds are ignored
    /// </summary>
    /// <param name="structureName">the structure name concerned</param>
    /// <param name="fullPath">the path to write the .obj file</param>
    /// <param name="withAO">whether Ambient Occlusion is exported</param>
    public static void exportRepsToOBJFile(string structureName, string fullPath, bool withAO = true) {

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }
        UnityMolStructure s = sm.GetStructure(structureName);

        if (s != null) {
            List<GameObject> repGameObjects = new();
            foreach (UnityMolRepresentation r in s.representations) {
                if (r.repType.atomType == AtomType.bondorder ||
                        r.repType.atomType == AtomType.fieldlines ||
                        r.repType.atomType == AtomType.ellipsoid ||
                        r.repType.atomType == AtomType.point ||
                        r.repType.bondType == BondType.hbond) {
                    Debug.LogWarning("Ignoring point/hbond/bondorder/fl/ellipsoid representation");
                    continue;
                }
                if (r.repType.atomType == AtomType.optihb ||
                        r.repType.bondType == BondType.optihs) {
                    continue;//Don't export the cubes
                }

                foreach (SubRepresentation sr in r.subReps) {
                    if (sr.atomRepManager != null) {
                        if (sr.atomRep.representationTransform != null) {
                            repGameObjects.Add(sr.atomRep.representationTransform.gameObject);
                            break;
                        }
                    }
                    if (sr.bondRepManager != null) {
                        if (sr.bondRep.representationTransform != null) {
                            repGameObjects.Add(sr.bondRep.representationTransform.gameObject);
                        }
                    }
                }
            }

            //Export all hyperballs to mesh
            List<GameObject> hbMeshesGo = ExtractHyperballMesh.getAllHBForStructure(s);
            repGameObjects.AddRange(hbMeshesGo);

            string objString = ObjExporter.DoExport(repGameObjects, true, withAO);
            try {
                using(StreamWriter sw = new(fullPath, false)) {
                    sw.Write(objString);
                    sw.Close();
                }
                Debug.Log("Exported " + repGameObjects.Count + " representations to " + fullPath);
            } catch (Exception e) {
                Debug.LogError("Failed to write to " + fullPath);
#if UNITY_EDITOR
                Debug.LogError(e);
#endif
                for (int i = 0; i < hbMeshesGo.Count; i++) {
                    Destroy(hbMeshesGo[i].GetComponent<MeshFilter>().sharedMesh);
                    Destroy(hbMeshesGo[i]);
                }
                return;
            }
            for (int i = 0; i < hbMeshesGo.Count; i++) {
                Destroy(hbMeshesGo[i].GetComponent<MeshFilter>().sharedMesh);
                Destroy(hbMeshesGo[i]);
            }
        } else {
            Debug.LogError("Structure not found");
        }
    }

    /// <summary>
    /// Export the given structure to an FBX file containing several meshes
    /// BondOrder/Point/Hbonds/Fieldlines are ignored
    /// </summary>
    /// <remarks>Only available on Windows or Mac</remarks>
    /// <param name="structureName">the structure name concerned</param>
    /// <param name="fullPath">the path to write the .fbx file</param>
    /// <param name="withAO">whether Ambient Occlusion is exported</param>
    public static void exportRepsToFBXFile(string structureName, string fullPath, bool withAO = true) {
#if !UNITY_EDITOR_WIN && !UNITY_EDITOR_OSX && !UNITY_STANDALONE_OSX && !UNITY_STANDALONE_WIN
        Debug.LogError("FBX export is only available on Windows/MacOS");
        return;
#else
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }
        UnityMolStructure s = sm.GetStructure(structureName);

        if (s != null) {
            List<GameObject> repGameObjects = new();
            foreach (UnityMolRepresentation r in s.representations) {
                if (r.repType.atomType == AtomType.bondorder ||
                        r.repType.atomType == AtomType.point ||
                        r.repType.atomType == AtomType.fieldlines ||
                        r.repType.atomType == AtomType.ellipsoid ||
                        r.repType.bondType == BondType.hbond) {
                    Debug.LogWarning("Ignoring point/hbond/bondorder/fl/ellipsoid representation");
                    continue;
                }
                if (r.repType.atomType == AtomType.optihb ||
                        r.repType.bondType == BondType.optihs) {
                    continue;//Don't export the cubes
                }

                foreach (SubRepresentation sr in r.subReps) {
                    if (sr.atomRepManager != null) {
                        if (sr.atomRep.representationTransform != null) {
                            repGameObjects.Add(sr.atomRep.representationTransform.gameObject);
                            break;
                        }
                    }
                    if (sr.bondRepManager != null) {
                        if (sr.bondRep.representationTransform != null) {
                            repGameObjects.Add(sr.bondRep.representationTransform.gameObject);
                        }
                    }
                }
            }

            //Export all hyperballs to mesh
            List<GameObject> hbMeshesGo = ExtractHyperballMesh.getAllHBForStructure(s);
            repGameObjects.AddRange(hbMeshesGo);

            try {
                FBXExporter.WriteMesh(repGameObjects, fullPath, withAO);
                Debug.Log("Exported " + repGameObjects.Count + " representations to " + fullPath);
            } catch {
                Debug.LogError("Failed to write to " + fullPath);
                for (int i = 0; i < hbMeshesGo.Count; i++) {
                    Destroy(hbMeshesGo[i].GetComponent<MeshFilter>().sharedMesh);
                    Destroy(hbMeshesGo[i]);
                }
                return;
            }

            for (int i = 0; i < hbMeshesGo.Count; i++) {
                Destroy(hbMeshesGo[i].GetComponent<MeshFilter>().sharedMesh);
                Destroy(hbMeshesGo[i]);
            }
        } else {
            Debug.LogError("Structure not found");
        }
#endif
    }

    /// <summary>
    /// Return the version of UnityMol
    /// </summary>
    public static void getVersion() {
        Debug.Log("UnityMol Version: " + UnityMolMain.Version);

    }

        /// <summary>
    /// Connect the user to a multiplayer session with a roomName and a playerName
    /// Use Photon Voice & PUN library
    /// </summary>
    /// <param name="roomName">The name of the 1st structure concerned</param>
    /// <param name="playerName">the IP address of the running simulation</param>
    /// <returns>True if the connexion is successful. False otherwise</returns>
    public static bool connectMultiplayer(string roomName, string playerName) {

        // Retrieve the multiplayer component to join a session
        JoinRoom[] jrComponents = FindObjectsOfType<JoinRoom>();
        if (jrComponents.Length == 0) {
            return false;
        }
        // Should always have one only component across the scene
        JoinRoom multiplayerComponent = jrComponents[0];

        if (multiplayerComponent.IsConnected()) {
            Debug.Log("Already connected to a multiplayer session");
            return false;
        }

        try {
            multiplayerComponent.Connect(roomName, playerName);


        } catch (Exception e) {
            Debug.LogError("Could not connect to a multiplayer session with a room named " + roomName + "\n " + "(" + e + ")");
            return false;
        }

        UnityMolMain.recordPythonCommand("connectMultiplayer(\"" + roomName + "\", \"" + playerName + "\")");
        UnityMolMain.recordUndoPythonCommand("disconnectMultiplayer()");
        return true;
    }

    /// <summary>
    /// Disconnect from a multiplayer session
    /// </summary>
    public static void disconnectMultiplayer() {

        // Retrieve the multiplayer component to join a session
        JoinRoom[] jrComponents = FindObjectsOfType<JoinRoom>();
        if (jrComponents.Length == 0) {
            return;
        }
        // Should always have one only component across the scene
        JoinRoom multiplayerComponent = jrComponents[0];

        if (!multiplayerComponent.IsConnected()) {
            Debug.Log("Not connected to a multiplayer session");
            return;
        }

        multiplayerComponent.Disconnect();

        UnityMolMain.recordPythonCommand("disconnectMultiplayer()");
        UnityMolMain.recordUndoPythonCommand("");

    }

}
}
}
