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
using UnityEditor;
using UnityEngine;
using System.Collections;
using UMol;

#if UNITY_EDITOR

// Creates an instance of a primitive depending on the option selected by the user.
public class ExportMoleculeToPackage : EditorWindow {

	string assetName = "UmolExport";
	int index = 0;
	string[] options = new string[] { "Mol1" };


	[MenuItem("UnityMol/Export Molecule To Package")]
	static void Init() {
		EditorWindow window = GetWindow(typeof(ExportMoleculeToPackage));
		UnityMolStructureManager sm = UnityMolMain.getStructureManager();
		int N = sm.loadedStructures.Count;
		((ExportMoleculeToPackage)window).options = new string[N];
		for (int i = 0; i < N; i++) {
			((ExportMoleculeToPackage)window).options[i] = sm.loadedStructures[i].name;
		}

		window.Show();



	}
	void OnGUI()
	{
		GUILayout.Label ("UnityMol Molecule Exporter", EditorStyles.boldLabel);
		assetName = EditorGUILayout.TextField ("Export name", assetName);
		index = EditorGUILayout.Popup("Molecule to export", index, options);

		if (GUILayout.Button("Export"))
			ExportMolecule();
	}


	void ExportMolecule() {
		UnityMolStructureManager sm = UnityMolMain.getStructureManager();
		UnityMolStructure s = sm.GetStructure(options[index]);
		GameObject sgo = sm.structureToGameObject[s.name];

		int idMesh = 0;
		string path = "Assets/" + assetName + ".asset";
		string prefabpath = "Assets/" + assetName + ".prefab";
		string packagepath = "Assets/" + assetName + ".unitypackage";
		Texture tex = null;

		foreach (UnityMolRepresentation rep in s.representations) {
			foreach (SubRepresentation sr in rep.subReps) {
				if (sr.atomRep != null) {
					foreach (Transform t in sr.atomRepresentationTransform) {
						MeshFilter mf = t.GetComponent<MeshFilter>();
						MeshRenderer mr = t.GetComponent<MeshRenderer>();
						Mesh m = mf.sharedMesh;
						Material mat = mr.sharedMaterial;
						tex = mat.GetTexture("_MainTex");

						if (idMesh == 0) {
							AssetDatabase.CreateAsset(m, path);
						}
						else {
							try{
								AssetDatabase.AddObjectToAsset (m, path);
							}
							catch{

							}
						}
						try {
							AssetDatabase.AddObjectToAsset (mat, path);
							if (tex != null) {
								AssetDatabase.AddObjectToAsset (tex, path);
							}
						}
						catch {

						}
						idMesh++;
					}
				}
				if (sr.bondRep != null) {
					foreach (Transform t in sr.bondRepresentationTransform) {
						MeshFilter mf = t.GetComponent<MeshFilter>();
						MeshRenderer mr = t.GetComponent<MeshRenderer>();
						Mesh m = mf.sharedMesh;
						Material mat = mr.sharedMaterial;

						tex = mat.GetTexture("_MainTex");

						if (idMesh == 0) {
							AssetDatabase.CreateAsset(m, path);
						}
						else {
							AssetDatabase.AddObjectToAsset (m, path);
						}
						try {
							AssetDatabase.AddObjectToAsset (mat, path);
							if (tex != null) {
								AssetDatabase.AddObjectToAsset (tex, path);
							}
						}
						catch {

						}

						idMesh++;
					}
				}
			}
		}


		//Copy the Hyperball shared shader
        string sharedHBpath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("shared_hyperball", null)[0]);
        string sharedWireframepath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("WireframeBakedBarycentricCoordinates", null)[0]);


		AssetDatabase.SaveAssets ();

		//Remove atom parent gameobject from the prefab
		Transform atomPar = sgo.transform.Find("AtomParent").transform;
		Transform savedPar = atomPar.parent;
		atomPar.parent = null;

		PrefabUtility.SaveAsPrefabAsset(sgo, prefabpath);

		atomPar.parent = savedPar;


		string[] filesToExport = new string[3];
		filesToExport[0] = sharedHBpath;
		filesToExport[1] = prefabpath;
		filesToExport[2] = sharedWireframepath;

		AssetDatabase.ExportPackage(filesToExport, packagepath, ExportPackageOptions.IncludeDependencies | ExportPackageOptions.Recurse);
	}
}

#endif
