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
using System.Collections.Generic;

namespace UMol {

public class UnityMolStructureManager {

    public Dictionary<string, GameObject> structureToGameObject = new Dictionary<string, GameObject>();
    public Dictionary<string, UnityMolStructure> nameToStructure = new Dictionary<string, UnityMolStructure>();
    public List<UnityMolStructure> loadedStructures = new List<UnityMolStructure>();
    public int currentStructureId = -1;
    bool init = false;

    public delegate void MoleculeLoaded();
    public static event MoleculeLoaded OnMoleculeLoaded;
    public delegate void MoleculeDeleted();
    public static event MoleculeDeleted OnMoleculeDeleted;

    public delegate void IMDConnected(StructureEventArgs args);
    public static event IMDConnected OnIMDConnected;

    public delegate void IMDDisconnected(StructureEventArgs args);
    public static event IMDDisconnected OnIMDDisconnected;

    void Init() {
        var objs = GameObject.FindObjectsOfType<UnityMolQuit>();
        if (objs.Length == 0) {
            GameObject umolq = new GameObject("UMolQuit");
            umolq.AddComponent<UnityMolQuit>();
        }
        init = true;
    }
    public int AddStructure(UnityMolStructure mol) {
        if (!init) {
            Init();
        }
        if (isNameUsed(mol.name)) {
            mol.name = findNewStructureName(mol.name);
        }
        int idStruct = loadedStructures.Count;
        loadedStructures.Add(mol);
        nameToStructure[mol.name] = mol;
        SetCurrentStructure(idStruct);

        Transform molParent = findStructureGO(mol);
        if (molParent != null) {
            structureToGameObject[mol.name] = molParent.gameObject;
        }
        else {
            Debug.LogError("Something went wrong when adding the structure");
        }

        // if (XRSettings.enabled) {
        SetVRInteractableObject(mol);
        // }

        if (OnMoleculeLoaded != null) {
            OnMoleculeLoaded();
        }

        UnityMolMain.getCustomRaycast().needsFullUpdate = true;
        return idStruct;
    }

    private Transform findStructureGO(UnityMolStructure s) {
        GameObject loadedMolGO = UnityMolMain.getRepresentationParent();
        string sName = s.ToSelectionName();
        Transform result = loadedMolGO.transform.Find(sName);
        if (UnityMolMain.inVR() && result == null) {

            GameObject clref = GameObject.Find("LeftHand");//VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController);
            GameObject crref = GameObject.Find("RightHand");//VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController);
            if (clref != null) {
                result = clref.transform.Find(sName);
            }
            if (result == null && crref != null) {
                result = crref.transform.Find(sName);
            }
        }
        return result;
    }

    public bool isNameUsed(string name) {
        return nameToStructure.ContainsKey(name);
    }
    public string findNewStructureName(string name) {
        int toAdd = 2;
        string result = name + "_" + toAdd.ToString();
        while (isNameUsed(result)) {
            toAdd++;
            result = name + "_" + toAdd.ToString();
        }
        return result;
    }

    public UnityMolStructure GetCurrentStructure() {
        if (loadedStructures.Count > 0)
            return loadedStructures[currentStructureId];

        throw new System.Exception("Wrong current molecule id " + currentStructureId +
                                   " but " + loadedStructures.Count + " structures loaded");

    }

    public void SetCurrentStructure(int idMol) {
        if (idMol >= 0 && idMol < loadedStructures.Count)
            currentStructureId = idMol;
        else
            throw new System.Exception("Wrong molecule id " + idMol +
                                       " but " + loadedStructures.Count + " structures loaded");

    }

    public UnityMolStructure GetStructure(int idMol) {
        if (idMol >= 0 && idMol < loadedStructures.Count) {
            return loadedStructures[idMol];
        }
        throw new System.Exception("Wrong molecule id " + idMol +
                                   " but " + loadedStructures.Count + " structures loaded");
    }
    public UnityMolStructure GetStructure(string name) {
        UnityMolStructure result;
        if (nameToStructure.TryGetValue(name, out result)) {
            return result;
        }
        throw new System.Exception("No structure named '" + name + "'' in structures loaded");
    }
    public GameObject GetStructureGameObject(string name) {
        GameObject result;
        if (structureToGameObject.TryGetValue(name, out result)) {
            return result;
        }

        throw new System.Exception("No structure named '" + name + "'' in structures loaded");
    }

    public void Delete(UnityMolStructure s) {
        try {
            UnityMolMain.getDockingManager().stopDockingMode();
            UnityMolMain.getPrecompRepManager().Clear(s.name);

            UnityMolMain.getAnnotationManager().RemoveAnnotations(s);

            s.OnDestroy();
            nameToStructure.Remove(s.name);
            structureToGameObject.Remove(s.name);
            loadedStructures.Remove(s);

            //Remove the selections and the representations that contains the structure
            UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
            selM.Delete(s);


            //Destroy the parent of colliders
            GameObject loadedMolGO = UnityMolMain.getRepresentationParent();
            Transform repParent = loadedMolGO.transform.Find(s.ToSelectionName());
            if (repParent != null) {
                GameObject.DestroyImmediate(repParent.gameObject);
            }

            if (OnMoleculeDeleted != null) {
                OnMoleculeDeleted();
            }
            UnityMolMain.getCustomRaycast().needsFullUpdate = true;
            //Destroy Unity materials not currently used
            Resources.UnloadUnusedAssets();
        }
        catch (System.Exception e) {
            Debug.LogWarning("Structure to delete failed " + e);
        }

        if (loadedStructures.Count == 0) {
            ManipulationManager mm = API.APIPython.getManipulationManager();
            if (mm != null) {
                mm.resetPosition();
                mm.resetRotation();
            }
        }
    }

    private void SetVRInteractableObject(UnityMolStructure mol, bool useLastMolPos = true) {

        Physics.autoSyncTransforms = false;

        string structName = mol.ToSelectionName();
        GameObject loadedMolGO = UnityMolMain.getRepresentationParent();
        Transform repParent = GetStructureGameObject(mol.name).transform;

        if (repParent == null) { //Should never happen because it is created in Reader.cs by the CreateColliders function
            repParent = (new GameObject(structName).transform);
            repParent.parent = loadedMolGO.transform;
            repParent.localPosition = Vector3.zero;
            repParent.localRotation = Quaternion.identity;
            repParent.localScale = Vector3.one;
        }

        if (useLastMolPos && loadedStructures.Count > 1) {
            UnityMolStructure slast = loadedStructures[loadedStructures.Count - 2];
            Transform tlast = GetStructureGameObject(slast.name).transform;
            repParent.localPosition = tlast.localPosition;
            repParent.localScale = tlast.localScale;
            repParent.localRotation = tlast.localRotation;
            return;
        }

        //Scale the molecule so that it fits inside a N x N x N cube
        float N = 2.0f;
        float maxDist = Vector3.Distance(mol.currentModel.minimumPositions, mol.currentModel.maximumPositions);

        API.APIPython.changeGeneralScale(N / maxDist);
        // loadedMolGO.transform.localScale = Vector3.one * N / maxDist;


    }


    public UnityMolStructure selectionNameToStructure(string name) {
        if (name.StartsWith("all_")) {
            string nameStructure = name.Substring(4, name.Length - 4);
            if (nameToStructure.ContainsKey(nameStructure)) {
                return nameToStructure[nameStructure];
            }
            return null;
        }
        if (!name.StartsWith("World"))
            Debug.LogError("Could not extract the name of a structure in " + name);
        return null;
    }

    public void DeleteAll() {
        List<string> toDelete = new List<string>();

        foreach (UnityMolStructure s in loadedStructures) {
            toDelete.Add(s.name);
        }

        foreach (string s in toDelete) {
            UnityMolStructure stru = GetStructure(s);
            if (stru != null) {
                Delete(stru);
            }
        }
        loadedStructures.Clear();
        structureToGameObject.Clear();
        nameToStructure.Clear();
    }

    public static void callIMDConnectionEvent(UnityMolStructure s) {
        if (OnIMDConnected != null)
            OnIMDConnected(new StructureEventArgs(s));
    }
    public static void callIMDDisconnectionEvent(UnityMolStructure s) {
        if (OnIMDDisconnected != null)
            OnIMDDisconnected(new StructureEventArgs(s));
    }
}
}