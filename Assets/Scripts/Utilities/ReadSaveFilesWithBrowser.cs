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
using System.Collections;
using UnityEngine;
using System.IO;
using System.Linq;
using SFB;
using SimpleFileBrowser;

namespace UMol {

/// <summary>
/// Class to handle OS File Browser
/// Uses the StandaloneFileBrowser plugin
/// Supports Windows, Linux & Mac
/// For VR, uses SimpleFileBrowser to open a dialog box inside the VR view.
/// </summary>
public class ReadSaveFilesWithBrowser : MonoBehaviour {

    /// <summary>
    /// Last opened folder. Save in the Player preferences.
    /// </summary>
    public string LastOpenedFolder = "";

    /// <summary>
    /// Default path when opening a FileBrowser dialog box.
    /// Empty mean current folder
    /// </summary>
    public string InitPath = "";

    /// <summary>
    /// Extension of the file to open/save
    /// </summary>
    public string Extension = "";

    /// <summary>
    /// Extensions that required a loaded molecule first
    /// </summary>
    private static readonly string[] extensionsRelyingOnMolecule = { ".xtc", ".dx", ".itp", ".psf", ".top" };


    /// <summary>
    /// Open a FileBrowser dialog box to open one or several file(s)
    /// </summary>
    /// <param name="initPath">path when opening the dialog box.</param>
    /// <param name="extension">extension to search for</param>
    /// <param name="forceBrowserVR">force the opening of a dialog box in VR.</param>
    /// <returns>An array of paths</returns>
    private string[] filesToRead(string initPath = "", string extension = "", bool forceBrowserVR = false)
    {
        ExtensionFilter[] extensions = {
            new("Molecule Files", "pdb", "ent", "mmcif", "cif", "gro", "mol2", "sdf", "mol", "xyz"),
            new("Trajectory Files", "xtc"),
            new("Density map Files", "dx"),
            new("State/Script Files", "py"),
            new("Martini itp Files", "itp"),
            new("PSF Files", "psf"),
            new("TOP Files", "top"),
            new("All Files", "*")
        };

        string[] paths = null;

        if (!UnityMolMain.inVR() || forceBrowserVR) {
            if (extension == "")
            {
                paths = StandaloneFileBrowser.OpenFilePanel("Open File", initPath, extensions, true);
            }
            else if (extension == "*")
            {
                paths = StandaloneFileBrowser.OpenFilePanel("Open File", initPath, "", true);
            }
            else
            {
                paths = StandaloneFileBrowser.OpenFilePanel("Open File", initPath, extension, true);
            }
        } else {
            // Use SimpleFileBrowser extension to open a dialog box
            // Used in VR to show the box within the VR scene.
            startGeneralLoadDialogBox(true);

        }
        return paths;
    }

    /// <summary>
    /// Call the correct API function to load a file depending on its extension.
    /// </summary>
    /// <param name="path">Path of the file to open</param>
    /// <param name="readHetm">For molecular file, is the hetero atoms read?</param>
    private static void loadFileFromPath(string path, bool readHetm) {

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        path = path.Replace("file:/", "").Replace("%20", " ");
#endif
        if (string.IsNullOrEmpty(path)) {
            return;
        }

        if (path.EndsWith(".py")) {
            API.APIPython.loadHistoryScript(path);
            return;
        }

        //Check if there is a loaded molecule for files which requires a loaded molecule
        if (extensionsRelyingOnMolecule.Any(path.EndsWith)) {
            UnityMolStructure lastMolecule = API.APIPython.last();
            if (lastMolecule == null) {
                return;
            }

            if (path.EndsWith(".xtc")) {
                string lastStructureName = lastMolecule.name;
                if (lastStructureName != null) {
                    API.APIPython.loadTraj(lastStructureName, path);
                }
            } else if (path.EndsWith(".dx")) {
                string lastStructureName = lastMolecule.name;
                if (lastStructureName != null) {
                    API.APIPython.loadDXmap(lastStructureName, path);
                }
            } else if (path.EndsWith(".itp")) {
                string lastStructureName = lastMolecule.name;
                if (lastStructureName != null) {
                    API.APIPython.loadMartiniITP(lastStructureName, path);
                }
            } else if (path.EndsWith(".psf")) {
                string lastStructureName = lastMolecule.name;
                if (lastStructureName != null) {
                    API.APIPython.loadPSFTopology(lastStructureName, path);
                }
            } else if (path.EndsWith(".top")) {
                string lastStructureName = lastMolecule.name;
                if (lastStructureName != null) {
                    API.APIPython.loadTOPTopology(lastStructureName, path);
                }
            }
        } else { // Try to load a molecular file
            API.APIPython.load(path, readHetm);
        }
    }


    /// <summary>
    /// Read all files from the folder `InitPath`.
    /// </summary>
    /// <param name="readHetm">For molecular file, is the hetero atoms read?</param>
    /// <param name="forceBrowserVR">force the opening of a dialog box in VR.</param>
    public void ReadFiles(bool readHetm = true, bool forceBrowserVR = false)
    {

        string[] paths = filesToRead(InitPath, Extension, forceBrowserVR);
        if (paths != null && paths.Length != 0) {
            if (paths[0] != "") {
                InitPath = Path.GetDirectoryName(paths[0]);
            }

            foreach (string t in paths)
            {
                loadFileFromPath(t, readHetm);
            }
        }
    }

    /// <summary>
    /// Save a state file (.py) in the folder `InitPath`.
    /// </summary>
    public void SaveState() {
        if (UnityMolMain.inVR()) {
            startStateSaveDialog();
        }
        else {
            ExtensionFilter[] extensions = {
                new("UnityMol State Files", "py")
            };
            string path = StandaloneFileBrowser.SaveFilePanel("Save UnityMol State", InitPath, "UMolState.py", extensions);
            if (path != null && !string.IsNullOrEmpty(path)) {
                API.APIPython.saveHistoryScript(path);
            }
        }
    }


    /// <summary>
    /// Read/Open a state file (.py) from the folder `InitPath`.
    /// <param name="forceBrowserVR">force the opening of a dialog box in VR.</param>
    /// </summary>
    public void ReadState(bool forceBrowserVR = false) {
        string[] paths = filesToRead(InitPath, "py", forceBrowserVR);
        if (paths != null && paths.Length > 0)
        {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            paths[0] = paths[0].Replace("file:/", "");
#endif
            if (!string.IsNullOrEmpty(paths[0])) {
                API.APIPython.loadHistoryScript(paths[0]);
            }
        }
    }

    /// <summary>
    /// Save a serialized state file (.json) in the folder `InitPath`.
    /// </summary>
    /// <param name="content">Content of the serialized state</param>
    public void SaveJson(string content){
        ExtensionFilter[] extensions = {
            new("Json file", "json" )
        };

        string path = StandaloneFileBrowser.SaveFilePanel("Save Json file", InitPath, "UMol.json", extensions);
        if (path != null  && !string.IsNullOrEmpty(path)) {
            File.WriteAllText(path, content);
            Debug.Log("Wrote to : " + path);
        }
    }


#region SimpleFileBrowser

    private void startGeneralLoadDialogBox(bool readHetm)
    {
        // Set filters (optional)
        FileBrowser.SetFilters( true, new FileBrowser.Filter( "Supported", ".pdb", ".cif", ".mmcif", ".gro",
                                ".mol2", ".xyz", ".sdf", ".mol", ".py", ".dx", ".xtc", ".itp", ".psf", ".top"));
        // Set default filter that is selected when the dialog is shown (optional)
        FileBrowser.SetDefaultFilter( ".pdb" );

        StartCoroutine( showGeneralLoadDialogCoroutine(readHetm) );
    }

    private IEnumerator showGeneralLoadDialogCoroutine(bool readHetm)
    {
        if (LastOpenedFolder == "") {
            string savedFolder = PlayerPrefs.GetString("lastOpenedFolderVR");
            if (!string.IsNullOrEmpty(savedFolder)) {
                LastOpenedFolder = savedFolder;
            }
        }
        // Show a load file dialog and wait for a response from user
        // Load file/folder: file, Initial path: default (Documents), Title: "Load File", submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog( false, LastOpenedFolder, "Load File", "Load" );

        // Dialog is closed
        // Print whether a file is chosen (FileBrowser.Success)
        // and the path to the selected file (FileBrowser.Result) (null, if FileBrowser.Success is false)
        if (FileBrowser.Success)
        {
            LastOpenedFolder = Path.GetDirectoryName(FileBrowser.Result);
            PlayerPrefs.SetString("lastOpenedFolderVR", LastOpenedFolder);
            foreach (string p in FileBrowser.Results) {
                loadFileFromPath(p, readHetm);
            }
        }
    }


    private void startStateSaveDialog() {
        FileBrowser.SetFilters( true, new FileBrowser.Filter("Save UnityMol State", ".py"));

        FileBrowser.SetDefaultFilter( ".py" );

        StartCoroutine( showStateSaveDialogCoroutine() );
    }

    private IEnumerator showStateSaveDialogCoroutine() {
        yield return FileBrowser.WaitForSaveDialog(false, LastOpenedFolder, "Save State File");

        //Make sure a file name was entered (if not the Result variable is the directory)
        if (FileBrowser.Success && !Directory.Exists(FileBrowser.Result))
        {
            LastOpenedFolder = Path.GetDirectoryName(FileBrowser.Result);
            API.APIPython.saveHistoryScript(FileBrowser.Result);
        }
        else
        {
            Debug.LogWarningFormat("Could not save to selected file");
        }
    }
#endregion

}
}
