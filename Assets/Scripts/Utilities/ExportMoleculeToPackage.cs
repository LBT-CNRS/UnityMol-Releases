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
			((ExportMoleculeToPackage)window).options[i] = sm.loadedStructures[i].uniqueName;
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
		GameObject sgo = sm.structureToGameObject[s.uniqueName];

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
        string sharedWireframepath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("UCLA GameLab Wireframe Shaders", null)[0]);
        string sharedWireframe2path = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("UCLA GameLab Wireframe Functions", null)[0]);


		AssetDatabase.SaveAssets ();

		//Remove colliders from the prefab
		Transform colliders = sgo.transform.Find("Colliders").transform;
		Transform savedPar = colliders.parent;
		colliders.parent = null;

		Object prefab = PrefabUtility.CreatePrefab(prefabpath, sgo);
		PrefabUtility.ReplacePrefab(sgo, prefab, ReplacePrefabOptions.ConnectToPrefab);

		colliders.parent = savedPar;


		string[] filesToExport = new string[4];
		filesToExport[0] = sharedHBpath;
		filesToExport[1] = prefabpath;
		filesToExport[2] = sharedWireframepath;
		filesToExport[3] = sharedWireframe2path;

		AssetDatabase.ExportPackage(filesToExport, packagepath, ExportPackageOptions.IncludeDependencies | ExportPackageOptions.Recurse);
	}
}

#endif