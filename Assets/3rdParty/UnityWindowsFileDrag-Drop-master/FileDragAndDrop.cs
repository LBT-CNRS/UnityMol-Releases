using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using B83.Win32;


public class FileDragAndDrop : MonoBehaviour
{
#if UNITY_STANDALONE_WIN
	
    // important to keep the instance alive while the hook is active.
    void OnEnable ()
    {
        // must be created on the main thread to get the right thread id.
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnFiles;
    }
    void OnDisable()
    {
        UnityDragAndDropHook.UninstallHook();
    }
    void OnFiles(List<string> aFiles, POINT aPos)
    {
        foreach (string fileP in aFiles) {
            loadGeneric(fileP);
        }
        // do something with the dropped file names. aPos will contain the
        // mouse position within the window where the files has been dropped.
        // Debug.Log("Dropped "+aFiles.Count+" files at: " + aPos + "\n"+
        // aFiles.Aggregate((a, b) => a + "\n" + b));
    }
    void loadGeneric(string path) {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        path = path.Replace("file:/", "");
#endif
        if (!string.IsNullOrEmpty(path))
        {

            if (path.EndsWith(".xtc")) {
                string lastStructureName = UMol.API.APIPython.last().name;
                if (lastStructureName != null) {
                    UMol.API.APIPython.loadTraj(lastStructureName, path);
                }
            }
            else if (path.EndsWith(".dx")) {
                string lastStructureName = UMol.API.APIPython.last().name;
                if (lastStructureName != null) {
                    UMol.API.APIPython.loadDXmap(lastStructureName, path);
                }
            }
            else if (path.EndsWith(".py")) {
                UMol.API.APIPython.loadHistoryScript(path);
            }
            else if (path.EndsWith(".itp")) {
                string lastStructureName = UMol.API.APIPython.last().name;
                if (lastStructureName != null)
                    UMol.API.APIPython.loadMartiniITP(lastStructureName, path);
            }
            else if (path.EndsWith(".psf")) {
                string lastStructureName = UMol.API.APIPython.last().name;
                if (lastStructureName != null)
                    UMol.API.APIPython.loadPSFTopology(lastStructureName, path);
            }
            else if (path.EndsWith(".top")) {
                string lastStructureName = UMol.API.APIPython.last().name;
                if (lastStructureName != null)
                    UMol.API.APIPython.loadTOPTopology(lastStructureName, path);
            }
            else {
                UMol.API.APIPython.load(path);
            }
        }
    }
#endif
}
