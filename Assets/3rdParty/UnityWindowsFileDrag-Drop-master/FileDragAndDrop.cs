using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using B83.Win32;


public class FileDragAndDrop : MonoBehaviour
{

    #if UNITY_STANDALONE_WIN
    // important to keep the instance alive while the hook is active.
    UnityDragAndDropHook hook;
    void OnEnable ()
    {
        // must be created on the main thread to get the right thread id.
        hook = new UnityDragAndDropHook();
        hook.InstallHook();
        hook.OnDroppedFiles += OnFiles;

    }
    void OnDisable()
    {
        hook.UninstallHook();
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
                string lastStructureName = UMol.API.APIPython.last().uniqueName;
                if (lastStructureName != null) {
                    UMol.API.APIPython.loadTraj(lastStructureName, path);
                }
            }
            else if (path.EndsWith(".dx")) {
                string lastStructureName = UMol.API.APIPython.last().uniqueName;
                if (lastStructureName != null) {
                    UMol.API.APIPython.loadDXmap(lastStructureName, path);
                }
            }
            else if (path.EndsWith(".py")) {
                UMol.API.APIPython.loadHistoryScript(path);
            }
            else if (path.EndsWith(".itp")) {
                //WARNING Not published yet
            }
            else {
                UMol.API.APIPython.load(path);
            }
        }
    }
    #endif
}
