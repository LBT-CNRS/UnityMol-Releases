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


using SimpleFileBrowser;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.XR;
using SFB;

namespace UMol {

public class ReadSaveFilesWithBrowser : MonoBehaviour
{
    public string lastOpenedFolder = "";
    public string initPath = "";
    public string extension = "";

    void loadFileFromPath(string path, bool readHetm) {

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        path = path.Replace("file:/", "");
#endif
        if (!string.IsNullOrEmpty(path))
        {

            if (path.EndsWith(".xtc")) {
                string lastStructureName = API.APIPython.last().uniqueName;
                if (lastStructureName != null) {
                    API.APIPython.loadTraj(lastStructureName, path);
                }
            }
            else if (path.EndsWith(".dx")) {
                string lastStructureName = API.APIPython.last().uniqueName;
                if (lastStructureName != null) {
                    API.APIPython.loadDXmap(lastStructureName, path);
                }
            }
            else if (path.EndsWith(".py")) {
                API.APIPython.loadHistoryScript(path);
            }
            else if (path.EndsWith(".itp")) {
                //WARNING Not published yet
            }
            else {
                API.APIPython.load(path, readHetm);
            }
        }
    }


    public void readFiles(bool readHetm = true, bool forceDesktop = false)
    {

        string[] paths = filesToRead(initPath, extension, readHetm, forceDesktop);
        if (paths != null && paths.Length != 0) {
            if(paths[0] != "")
                initPath = Path.GetDirectoryName(paths[0]);
                
            for (int i = 0; i < paths.Length; i++)
            {
                loadFileFromPath(paths[i], readHetm);
            }
        }
    }

    public void saveState() {
        string path = stateToRead(initPath);
        if (path != null) {
            API.APIPython.saveHistoryScript(path);
        }
    }

    public void readState() {
        string[] paths = filesToRead(initPath, "py");
        if (paths != null && paths.Length > 0)
        {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            paths[0] = paths[0].Replace("file:/", "");
#endif
            API.APIPython.loadHistoryScript(paths[0]);
        }
    }

    public string stateToRead(string initPath = "") {
#if (UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_WEBGL )
        var extensions = new []
        {
            new ExtensionFilter("UnityMol State Files", "py" )
        };

        return StandaloneFileBrowser.SaveFilePanel("Save UnityMol State", initPath, "UMolState.py", extensions);

#else
        StartDialogSave();
#endif
        return null;
    }
    public string[] filesToRead(string initPath = "", string extension = "", bool readHetm = true, bool forceDesktop = false)
    {


        var extensions = new []
        {
            new ExtensionFilter("Molecule Files", "pdb", "mmcif", "cif", "gro", "mol2", "sdf", "mol", "xyz"),
            new ExtensionFilter("Trajectory Files", "xtc"),
            new ExtensionFilter("Density map Files", "dx"),
            new ExtensionFilter("State/Script Files", "py"),
            new ExtensionFilter("Martini itp Files", "itp"),
            new ExtensionFilter("All Files", "*"),
        };

        string[] paths = null;
        //Use native file browser for Windows and Mac and WebGL (https://github.com/gkngkc/UnityStandaloneFileBrowser)
#if (UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_WEBGL )
        if (!UnityMolMain.inVR() || forceDesktop) {
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
        }
        else {
            StartDialog(readHetm);
        }
#else //Use asset based file browser (https://github.com/yasirkula/UnitySimpleFileBrowser)
        //Uses a coroutine
        StartDialog(readHetm);
#endif
        return paths;
    }

    void StartDialog(bool readHetm)
    {
        // Set filters (optional)
        // It is sufficient to set the filters just once (instead of each time before showing the file browser dialog),
        // if all the dialogs will be using the same filters
        FileBrowser.SetFilters( true, new FileBrowser.Filter( "Supported", ".pdb", ".cif", ".mmcif", ".gro", ".mol2", ".xyz", ".sdf", ".mol", ".py", ".dx", ".xtc", ".itp"));

        // Set default filter that is selected when the dialog is shown (optional)
        // Returns true if the default filter is set successfully
        // In this case, set Images filter as the default filter
        FileBrowser.SetDefaultFilter( ".pdb" );

        // Set excluded file extensions (optional) (by default, .lnk and .tmp extensions are excluded)
        // Note that when you use this function, .lnk and .tmp extensions will no longer be
        // excluded unless you explicitly add them as parameters to the function
        // FileBrowser.SetExcludedExtensions( ".lnk", ".tmp", ".zip", ".rar", ".exe" );

        // // Add a new quick link to the browser (optional) (returns true if quick link is added successfully)
        // // It is sufficient to add a quick link just once
        // // Name: Users
        // // Path: C:\Users
        // // Icon: default (folder icon)
        // FileBrowser.AddQuickLink( "Users", "C:\\Users", null );

        // Show a save file dialog
        // onSuccess event: not registered (which means this dialog is pretty useless)
        // onCancel event: not registered
        // Save file/folder: file, Initial path: "C:\", Title: "Save As", submit button text: "Save"
        // FileBrowser.ShowSaveDialog( null, null, false, "C:\\", "Save As", "Save" );

        // Show a select folder dialog
        // onSuccess event: print the selected folder's path
        // onCancel event: print "Canceled"
        // Load file/folder: folder, Initial path: default (Documents), Title: "Select Folder", submit button text: "Select"
        // FileBrowser.ShowLoadDialog( (path) => { Debug.Log( "Selected: " + path ); },
        //                                () => { Debug.Log( "Canceled" ); },
        //                                true, null, "Select Folder", "Select" );

        // Coroutine example
        StartCoroutine( ShowLoadDialogCoroutine(readHetm) );
    }

    void StartDialogSave() {
        FileBrowser.SetFilters( true, new FileBrowser.Filter( "UnityMo State", ".py") );

        FileBrowser.SetDefaultFilter( ".py" );

        StartCoroutine( ShowSaveDialogCoroutine() );
    }

    IEnumerator ShowLoadDialogCoroutine(bool readHetm)
    {
        if(lastOpenedFolder == ""){
            string savedFolder = PlayerPrefs.GetString("lastOpenedFolderVR");
            if(!string.IsNullOrEmpty(savedFolder)){
                lastOpenedFolder = savedFolder;
            }
        }
        // Show a load file dialog and wait for a response from user
        // Load file/folder: file, Initial path: default (Documents), Title: "Load File", submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog( false, lastOpenedFolder, "Load File", "Load" );

        // Dialog is closed
        // Print whether a file is chosen (FileBrowser.Success)
        // and the path to the selected file (FileBrowser.Result) (null, if FileBrowser.Success is false)

        if (FileBrowser.Success)
        {
            lastOpenedFolder = Path.GetDirectoryName(FileBrowser.Result);
            PlayerPrefs.SetString("lastOpenedFolderVR", lastOpenedFolder);
            foreach (string p in FileBrowser.Results) {
                loadFileFromPath(p, readHetm);
            }
        }
        else
        {
            Debug.LogError("Could not load selected file");
        }
    }
    IEnumerator ShowSaveDialogCoroutine() {
        yield return FileBrowser.WaitForSaveDialog( false, lastOpenedFolder, "Save File", "Save" );

        if (FileBrowser.Success)
        {
            lastOpenedFolder = Path.GetDirectoryName(FileBrowser.Result);
            API.APIPython.saveHistoryScript(FileBrowser.Result);
        }
        else
        {
            Debug.LogError("Could not save to selected file");
        }
    }
}
}