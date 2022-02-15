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
using UnityEngine.XR;
using System.Collections.Generic;

using VRTK;
using VRTK.GrabAttachMechanics;
using VRTK.SecondaryControllerGrabActions;


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


	void Init(){
		var objs = GameObject.FindObjectsOfType<UnityMolQuit>();
		if(objs.Length == 0){
			GameObject umolq = new GameObject("UMolQuit");
			umolq.AddComponent<UnityMolQuit>();
		}
		init = true;
	}
	public int AddStructure(UnityMolStructure mol) {
		if(!init){
			Init();
		}
		if (isNameUsed(mol.uniqueName)) {
			mol.uniqueName = findNewStructureName(mol.uniqueName);
		}
		int idStruct = loadedStructures.Count;
		loadedStructures.Add(mol);
		nameToStructure[mol.uniqueName] = mol;
		SetCurrentStructure(idStruct);

		Transform molParent = findStructureGO(mol);
		if (molParent != null) {
			structureToGameObject[mol.uniqueName] = molParent.gameObject;
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

	public Transform findStructureGO(UnityMolStructure s) {
		GameObject loadedMolGO = UnityMolMain.getRepresentationParent();
		string sName = s.ToSelectionName();
		Transform result = loadedMolGO.transform.Find(sName);
		if (UnityMolMain.inVR() && result == null) {

			Transform clref = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController);
			Transform crref = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController);
			if (clref != null) {
				result = clref.Find(sName);
			}
			if (result == null && crref != null) {
				result = crref.Find(sName);
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
			UnityMolMain.getPrecompRepManager().Clear(s.uniqueName);

			s.OnDestroy();
			nameToStructure.Remove(s.uniqueName);
			structureToGameObject.Remove(s.uniqueName);
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
			Debug.LogWarning("Structure to delete not found " + e);
		}
	}

	private void SetVRInteractableObject(UnityMolStructure mol, bool useLastMolPos = true) {

		Physics.autoSyncTransforms = false;

		string structName = mol.ToSelectionName();
		GameObject loadedMolGO = UnityMolMain.getRepresentationParent();
		Transform repParent = GetStructureGameObject(mol.uniqueName).transform;

		if (repParent == null) { //Should never happen because it is created in Reader.cs by the CreateColliders function
			repParent = (new GameObject(structName).transform);
			repParent.parent = loadedMolGO.transform;
			repParent.localPosition = Vector3.zero;
			repParent.localRotation = Quaternion.identity;
			repParent.localScale = Vector3.one;
		}

		if (useLastMolPos && loadedStructures.Count > 1) {
			UnityMolStructure slast = loadedStructures[loadedStructures.Count - 2];
			Transform tlast = GetStructureGameObject(slast.uniqueName).transform;
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
		if (name.StartsWith("all(") && name.EndsWith(")")) {
			string nameStructure = name.Substring(4, name.Length - 1 - 4);
			if (nameToStructure.ContainsKey(nameStructure)) {
				return nameToStructure[nameStructure];
			}
			return null;
		}

		Debug.LogError("Could not extract the name of a structure in " + name);
		return null;
	}

	public void DeleteAll() {
		List<UnityMolStructure> copyLoadedS = new List<UnityMolStructure>(loadedStructures);
		for (int i = 0; i < copyLoadedS.Count; i++) {
			Delete(copyLoadedS[i]);
		}
		copyLoadedS.Clear();
	}
}
}