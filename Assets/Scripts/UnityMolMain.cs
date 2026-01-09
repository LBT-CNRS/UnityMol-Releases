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
using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using UMol.ForceFields;

namespace UMol {
public class UnityMolMain {


    /// <summary>
    /// Reference to the UnityMol version
    /// Based on the git tag for a release build.
    /// Based on the git tag + commit id for the other cases.
    /// </summary>
    /// <remarks>
    /// This is the only valid place to get the correct version.
    /// Don't use the PlayerSettings GUI for setting the version.
    /// </remarks>
    public static string Version => UnityMolVersion.GetVersion();

    public static UnityMolTopologyDef topologies = new UnityMolTopologyDef();

    public static Dictionary< string, Dictionary<string, CGAtomITP>> loadedITP = ITPReader.InitITPDict();

    public static UnityMolDefaultColors atomColors = new UnityMolDefaultColors();

    private static UnityMolStructureManager structureManager = new UnityMolStructureManager();
    private static UnityMolRepresentationManager representationManager;
    private static UnityMolSelectionManager selectionManager = new UnityMolSelectionManager();

    private static PrecomputedRepresentationManager precompRepManager = new PrecomputedRepresentationManager();

#if !DISABLE_HIGHLIGHT
    private static UnityMolHighlightManager highlightManager = new UnityMolHighlightManager();
#endif

    private static ForceFieldsManager ffManager = new ForceFieldsManager();
    private static DockingManager dockingManager;

    private static UnityMolAnnotationManager annotationManager;

    private static GraphManager graphManager = new GraphManager();

    public static MeasureMode measureMode = MeasureMode.distance;

    public static ObservedList<string> pythonCommands = new ObservedList<string>();
    public static ObservedList<string> pythonUndoCommands = new ObservedList<string>();
    public static int NRestoreCommands = 50;
    public static bool writeAllCommandsToConsole = true;

    private static GameObject loadedMolGO;

    private static CustomRaycastBurst raycaster;

    public static string APBSInstallPath = "";

    public static bool isFogOn = false;
    public static float fogStart = 0.0f;
    public static float fogDensity = 0.5f;
    public static bool isDOFOn = false;



    /// Color of the ambient light.
    public static Color initAmbientColor = RenderSettings.ambientLight;
    public static float ambientLightScale = 1.0f;

    public static bool multiUserMode = false;
    public static bool multiUserVR = false;
    public static bool multiUserPresenter = false;

    public static bool IMDRunning = false;
    static lowerFrameRateIdle idleManager;

    ///Lowers framerate when nothing happens for more than 5sec (see lowerFrameRateIdle.cs)
    public static bool allowIDLE = true;

    ///External threads are launched when loading a molecule to pre-compute surfaces, 2 threads per chain (EDTSurf + MSMS)
    public static bool disableSurfaceThread = false;
    ///Files with 50k atoms or more will not trigger surface thread
    public static int surfaceThreadLimit = 50000;

    ///By default, when raytracing is activated, the denoiser will start after 8 RT frames
    private static bool _raytracingModeStarted = false;
    private static bool _raytracingMode = false;
    public static bool raytracingMode {
        get {
            return _raytracingMode;
        }
        set {
            if (inVR()) {
                Debug.LogError("Cannot activate raytracing mode in VR");
            }
            _raytracingMode = value;
            if (!_raytracingModeStarted && value) {
                Camera mc = Camera.main;
                if (mc != null && mc.gameObject.GetComponent<RaytracerManager>() == null) {
                    mc.gameObject.AddComponent<RaytracerManager>();
                    mc.gameObject.AddComponent<RaytracedObject>().type = RaytracedObject.RayTObjectType.camera;
                    Light[] lights = GameObject.FindObjectsOfType<Light>();
                    foreach (var l in lights) {
                        if (l.type == LightType.Directional) {
                            l.gameObject.AddComponent<RaytracedObject>().type = RaytracedObject.RayTObjectType.light;
                        }
                    }
                    _raytracingModeStarted = true;
                }
            }
        }
    }

    private static GameObject leftVRController;
    private static GameObject rightVRController;

    public delegate void OnNewCommand(CommandEventArgs args);
    public static event OnNewCommand onNewCommand;
    public static bool pauseCommandEvent = false;

    public delegate void OnReplacePrevCommand(CommandEventArgs args);
    public static event OnReplacePrevCommand onReplacePrevCommand;


    public static bool inVR() {
        return XRSettings.enabled && (XRSettings.loadedDeviceName != null
                                      && !XRSettings.loadedDeviceName.StartsWith("stereo"));
    }

    public static GameObject getRepresentationParent() {
        if (loadedMolGO == null) {
            loadedMolGO = GameObject.Find("LoadedMolecules");
        }
        if (loadedMolGO == null) {
            loadedMolGO = new GameObject("LoadedMolecules");
        }
        return loadedMolGO;
    }

    public static GameObject getRepStructureParent(string structName) {
        GameObject loadedMolGO = getRepresentationParent();
        Transform representationParent = loadedMolGO.transform.Find(structName);
        if (inVR() && representationParent == null) {

            GameObject clref = getLeftController();
            GameObject crref = getRightController();
            if (clref != null) {
                representationParent = clref.transform.Find(structName);
            }
            if (representationParent == null && crref != null) {
                representationParent = crref.transform.Find(structName);
            }
        }
        if (representationParent == null) {
            representationParent = (new GameObject(structName).transform);
            representationParent.parent = loadedMolGO.transform;
            representationParent.localPosition = Vector3.zero;
            representationParent.localRotation = Quaternion.identity;
            representationParent.localScale = Vector3.one;
        }
        return representationParent.gameObject;
    }

    public static CustomRaycastBurst getCustomRaycast() {
        if (raycaster == null) {
            raycaster = new CustomRaycastBurst();
        }
        return raycaster;
    }

    public static UnityMolStructureManager getStructureManager() {
        return structureManager;
    }

    public static UnityMolRepresentationManager getRepresentationManager() {
        if (representationManager == null) {
            GameObject repManGo = new GameObject("RepresentationManager");
            repManGo.transform.hideFlags = HideFlags.HideInInspector;
            representationManager = repManGo.AddComponent<UnityMolRepresentationManager>();
            try {Object.DontDestroyOnLoad(repManGo);}
            catch {
#if !UNITY_EDITOR
                Debug.LogError("Failed to move objects to a safe scene! This is bad !");
#endif
            }
        }
        return representationManager;
    }

    public static UnityMolSelectionManager getSelectionManager() {
        return selectionManager;
    }

    public static PrecomputedRepresentationManager getPrecompRepManager() {
        return precompRepManager;
    }

#if !DISABLE_HIGHLIGHT
    public static UnityMolHighlightManager getHighlightManager() {
        return highlightManager;
    }
#endif

    public static ForceFieldsManager getForceFieldsManager()
    {
        return UnityMolMain.ffManager;
    }

    public static DockingManager getDockingManager() {
        if (dockingManager == null) {
            GameObject dockingManaGo = new GameObject("DockingManager");
            dockingManager = dockingManaGo.AddComponent<DockingManager>();
            try {Object.DontDestroyOnLoad(dockingManager);}
            catch {
#if !UNITY_EDITOR
                Debug.LogError("Failed to move objects to a safe scene! This is bad !");
#endif
            }
        }
        return dockingManager;
    }

    public static UnityMolAnnotationManager getAnnotationManager() {
        if (annotationManager == null) {
            GameObject annoManaGo = new GameObject("AnnotationManager");
            annotationManager = annoManaGo.AddComponent<UnityMolAnnotationManager>();
            try {Object.DontDestroyOnLoad(annoManaGo);}
            catch {
#if !UNITY_EDITOR
                Debug.LogError("Failed to move objects to a safe scene! This is bad !");
#endif
            }
        }
        return annotationManager;
    }

    public static GraphManager getGraphManager() {
        return graphManager;
    }

    public static GameObject getLeftController() {
        if (leftVRController != null)
            return leftVRController;
        leftVRController = GameObject.Find("LeftHand");
        return leftVRController;
    }

    public static GameObject getRightController() {
        if (rightVRController != null)
            return rightVRController;
        rightVRController = GameObject.Find("RightHand");
        return rightVRController;
    }

    // public static lowerFrameRateIdle getIDLEManager() {
    //  if(idleManager == null){
    //      lowerFrameRateIdle[] foundObjects = GameObject.FindObjectsOfType<lowerFrameRateIdle>();
    //      if(foundObjects.Length == 0) {
    //          GameObject idleGo = new GameObject("IDLEModeManager");
    //          idleManager = idleGo.AddComponent<lowerFrameRateIdle>();
    //      }
    //      else {
    //          idleManager = foundObjects[0];
    //      }
    //  }
    //  return idleManager;
    // }

    public static bool recordPythonCommand(string command, bool replacePrevSame = false, int firstNSame = -1) {

        if (!multiUserMode && replacePrevSame && firstNSame > 0 && pythonCommands.Count > 0) {
            string lastCo = pythonCommands[pythonCommands.Count - 1];
            if (lastCo.Length > firstNSame) {
                string lastCoStart = lastCo.Substring(0, firstNSame);

                if (command.StartsWith(lastCoStart)) {

                    //Remove last saved command & replace it
                    pythonCommands.RemoveAt(pythonCommands.Count - 1);
                    pythonCommands.Add(command);
                    if (onReplacePrevCommand != null) {
                        onReplacePrevCommand(new CommandEventArgs(command));
                    }
                    return true;
                }
            }
        }

        if (onNewCommand != null && !pauseCommandEvent) {
            onNewCommand(new CommandEventArgs(command));
        }

        pythonCommands.Add(command);
        return false;
    }
    public static void recordUndoPythonCommand(string command, bool replacedPrevSame = false) {
        if (replacedPrevSame && pythonUndoCommands.Count > 0) {
            pythonUndoCommands.RemoveAt(pythonUndoCommands.Count - 1);
        }
        pythonUndoCommands.Add(command);
    }

    public static string commandHistory() {
        StringBuilder sb = new StringBuilder();
        foreach (string s in pythonCommands) {
            sb.Append(s);
            sb.Append("\n");
        }
        return sb.ToString();
    }

    ///Restore session by deserializing a Json file
    public static void JSONToSession(string jsonSession)
    {
        SerializedRoot root = JsonUtility.FromJson<SerializedRoot>(jsonSession);
        root.restoreStructures();
        root.restoreSelections();
        root.restoreRepresentations();
        root.restoreAnnotations();
    }

    ///Fills classes to use JsonUtility and outputs a JSON string
    public static string sessionToJSON() {
        UnityMolSelectionManager selM = getSelectionManager();
        UnityMolRepresentationManager repM = getRepresentationManager();
        UnityMolStructureManager sM = UnityMolMain.getStructureManager();
        UnityMolAnnotationManager aM = UnityMolMain.getAnnotationManager();

        SerializedRoot root = new SerializedRoot();
        root.UMolSession = new SerializedUMolSession();
        root.UMolSession.structures = new List<SerializedStructure>(sM.loadedStructures.Count);
        root.UMolSession.selections = new List<SerializedSelection>();
        root.UMolSession.representations = new List<SerializedRepresentation>();
        root.UMolSession.annotations = new List<SerializedAnnotation>();


        //Structures
        foreach (var structure in sM.loadedStructures) {
            SerializedStructure sstruc = structure.Serialize();
            if (sstruc != null)
                root.UMolSession.structures.Add(sstruc);
        }

        //Selections
        foreach (UnityMolSelection sel in selM.selections.Values) {
            SerializedSelection ssel = sel.Serialize();
            if (ssel != null)
                root.UMolSession.selections.Add(ssel);
        }

        //Representations
        foreach (UnityMolRepresentation r in repM.representations) {
            if (r.subReps.Count != 0) {
                SubRepresentation sr = r.subReps[0];
                //Special case for hyperballs
                if (sr.atomRepManager != null && sr.bondRepManager != null &&
                        r.repType.atomType == AtomType.optihb && r.repType.bondType == BondType.optihs) {
                    SerializedRepresentation repa = sr.atomRepManager.Save().Serialize(r.selection);
                    SerializedRepresentation repb = sr.bondRepManager.Save().Serialize(r.selection);
                    repa.FuseHB(repb);
                    root.UMolSession.representations.Add(repa);
                }
                else {
                    if (sr.atomRepManager != null) {
                        SerializedRepresentation repa = sr.atomRepManager.Save().Serialize(r.selection);
                        root.UMolSession.representations.Add(repa);
                    }
                    if (sr.bondRepManager != null) {
                        SerializedRepresentation repb = sr.bondRepManager.Save().Serialize(r.selection);
                        root.UMolSession.representations.Add(repb);
                    }
                }
            }
        }

        //Annotations
        foreach (UnityMolAnnotation an in aM.allAnnotations) {
            SerializedAnnotation serAnn = an.Serialize();
            root.UMolSession.annotations.Add(serAnn);
        }


        return JsonUtility.ToJson(root, true);
    }
}

public class CommandEventArgs : System.EventArgs {
    public CommandEventArgs(string com) {
        this.command = com;
    }
    public string command {get; private set;}
}

public class NewSelEventArgs : System.EventArgs {
    public NewSelEventArgs(UnityMolSelection s) {
        this.sel = s;
    }
    public UnityMolSelection sel {get; private set;}
}

public class AnnoEventArgs : System.EventArgs {
    public AnnoEventArgs(UnityMolAnnotation a) {
        this.anno = a;
    }
    public UnityMolAnnotation anno {get; private set;}
}

public class StructureEventArgs : System.EventArgs {
    public StructureEventArgs(UnityMolStructure s) {
        this.structure = s;
    }
    public UnityMolStructure structure {get; private set;}
}

}
