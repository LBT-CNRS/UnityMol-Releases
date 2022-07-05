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
using UnityEngine.Rendering;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine.XR;

using VRTK;

namespace UMol {

public class AtomRepresentationTube : AtomRepresentation {

	public Dictionary<UnityMolAtom, List<int>> atomToMeshVertex;
	public List<Int2> alphaTrace;
	public GameObject meshGO;
	public Mesh curMesh;
	public float lineWidth = 0.15f;
	public List<Vector3> vertices;
	public List<Color32> colors;
	public List<Color32> savedColors;

	public AtomRepresentationTube(string structName, UnityMolSelection sel) {

		colorationType = colorType.atom;
		GameObject loadedMolGO = UnityMolMain.getRepresentationParent();

		representationParent = loadedMolGO.transform.Find(structName);
		if (UnityMolMain.inVR() && representationParent == null) {

			Transform clref = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController);
			Transform crref = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController);
			if (clref != null) {
				representationParent = clref.Find(structName);
			}
			if (representationParent == null && crref != null) {
				representationParent = crref.Find(structName);
			}
		}
		if (representationParent == null) {
			representationParent = (new GameObject(structName).transform);
			representationParent.parent = loadedMolGO.transform;
			representationParent.localPosition = Vector3.zero;
			representationParent.localRotation = Quaternion.identity;
			representationParent.localScale = Vector3.one;
		}
		GameObject newRep = new GameObject("AtomTubeRepresentation");
		newRep.transform.parent = representationParent;
		representationTransform = newRep.transform;


		selection = sel;

		DisplayTubeMesh(newRep.transform);

		// newRep.transform.position -= offset;
		newRep.transform.localPosition = Vector3.zero;
		newRep.transform.localRotation = Quaternion.identity;
		newRep.transform.localScale = Vector3.one;

		nbAtoms = selection.Count;
	}

	void DisplayTubeMesh(Transform repParent, bool isRecompute = false) {

		if (selection.Count < 2)
			return;


		alphaTrace = new List<Int2>();
		UnityMolAtom prevCA = null;
		int idPrevA = -1;

		int id = 0;
		foreach (UnityMolAtom a in selection.atoms) {
			string toFind = "CA";
			if (MDAnalysisSelection.isNucleic(a.residue)) {
				toFind = "P";
			}

			if (a.name == toFind) {

				if (prevCA != null &&
				        prevCA.residue.chain.model.structure.uniqueName == a.residue.chain.model.structure.uniqueName && //Same structure
				        prevCA.residue.chain.name == a.residue.chain.name && //Same chain
				        diffResId(prevCA.residue.id, a.residue.id) ) { //Residue difference == 1

					Int2 d;
					d.x = idPrevA;
					d.y = id;
					alphaTrace.Add(d);
				}

				prevCA = a;
				idPrevA = id;
			}
			id++;
		}

		if (alphaTrace.Count == 0) {
			return;
		}

		atomToMeshVertex = new Dictionary<UnityMolAtom, List<int>>();

		GameObject currentGO = new GameObject("TubeMesh");
		currentGO.transform.parent = repParent;
		currentGO.transform.localPosition = Vector3.zero;
		currentGO.transform.localScale = Vector3.one;
		currentGO.transform.localRotation = Quaternion.identity;



		if (!isRecompute) {
			curMesh = new Mesh();
			curMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
		}

		List<Vector3> newVertices = new List<Vector3>();
		List<int> newTriangles = new List<int>();
		List<Color32> newColors = new List<Color32>();
		List<Vector2> newUV = new List<Vector2>();


		foreach (Int2 duo in alphaTrace) {
			UnityMolAtom atom1 = selection.atoms[duo.x];
			UnityMolAtom atom2 = selection.atoms[duo.y];

			Vector3 start = atom1.position;
			Vector3 end   = atom2.position;

			Vector3 normal = Vector3.Cross(start, end);
			Vector3 side = Vector3.Cross(normal, end - start);
			side.Normalize();
			normal.Normalize();

			Vector3 a = start + side * (lineWidth / 2);
			Vector3 b = start - side * (lineWidth / 2);
			Vector3 c = end + side * (lineWidth / 2);
			Vector3 d = end - side * (lineWidth / 2);

			Vector3 a1 = a + normal * (lineWidth / 2);
			Vector3 a2 = a - normal * (lineWidth / 2);
			Vector3 b1 = b + normal * (lineWidth / 2);
			Vector3 b2 = b - normal * (lineWidth / 2);
			Vector3 c1 = c + normal * (lineWidth / 2);
			Vector3 c2 = c - normal * (lineWidth / 2);
			Vector3 d1 = d + normal * (lineWidth / 2);
			Vector3 d2 = d - normal * (lineWidth / 2);

			int ida = newVertices.Count;
			newVertices.Add(a1);
			newVertices.Add(a2);
			newVertices.Add(b1);
			newVertices.Add(b2);
			newVertices.Add(c1);
			newVertices.Add(c2);
			newVertices.Add(d1);
			newVertices.Add(d2);


			newTriangles.Add(ida);//a1
			newTriangles.Add(ida + 1); //a2
			newTriangles.Add(ida + 2); //b1

			newTriangles.Add(ida + 2); //b1
			newTriangles.Add(ida + 1); //a2
			newTriangles.Add(ida + 3); //b2

			newTriangles.Add(ida);//a1
			newTriangles.Add(ida + 4); //c1
			newTriangles.Add(ida + 1); //a2

			newTriangles.Add(ida + 4); //c1
			newTriangles.Add(ida + 5); //c2
			newTriangles.Add(ida + 1); //a2

			newTriangles.Add(ida + 6); //d1
			newTriangles.Add(ida + 4); //c1
			newTriangles.Add(ida + 2); //b1

			newTriangles.Add(ida);//a1
			newTriangles.Add(ida + 2); //b1
			newTriangles.Add(ida + 4); //c1


			newTriangles.Add(ida + 2); //b1
			newTriangles.Add(ida + 3); //b2
			newTriangles.Add(ida + 6); //d1

			newTriangles.Add(ida + 6); //d1
			newTriangles.Add(ida + 3); //b2
			newTriangles.Add(ida + 7); //d2

			newTriangles.Add(ida + 7); //d2
			newTriangles.Add(ida + 1); //a2
			newTriangles.Add(ida + 5); //c2

			newTriangles.Add(ida + 1); //a2
			newTriangles.Add(ida + 7); //d2
			newTriangles.Add(ida + 3); //b2

			newTriangles.Add(ida + 4); //c1
			newTriangles.Add(ida + 6); //d1
			newTriangles.Add(ida + 7); //d2

			newTriangles.Add(ida + 7); //d2
			newTriangles.Add(ida + 5); //c2
			newTriangles.Add(ida + 4); //c1

			newColors.Add(atom1.color);//0
			newColors.Add(atom1.color);//1
			newColors.Add(atom1.color);//2
			newColors.Add(atom1.color);//3

			newColors.Add(atom2.color);//4
			newColors.Add(atom2.color);//5
			newColors.Add(atom2.color);//6
			newColors.Add(atom2.color);//7

			newUV.Add(Vector2.one);
			newUV.Add(Vector2.one);
			newUV.Add(Vector2.one);
			newUV.Add(Vector2.one);

			newUV.Add(Vector2.one);
			newUV.Add(Vector2.one);
			newUV.Add(Vector2.one);
			newUV.Add(Vector2.one);


			if (!atomToMeshVertex.ContainsKey(atom1)) {
				atomToMeshVertex[atom1] = new List<int>();
			}
			atomToMeshVertex[atom1].Add(ida);
			atomToMeshVertex[atom1].Add(ida + 1);
			atomToMeshVertex[atom1].Add(ida + 2);
			atomToMeshVertex[atom1].Add(ida + 3);

			if (!atomToMeshVertex.ContainsKey(atom2)) {
				atomToMeshVertex[atom2] = new List<int>();
			}
			atomToMeshVertex[atom2].Add(ida + 4);
			atomToMeshVertex[atom2].Add(ida + 5);
			atomToMeshVertex[atom2].Add(ida + 6);
			atomToMeshVertex[atom2].Add(ida + 7);
		}
		curMesh.SetVertices(newVertices);
		curMesh.SetTriangles(newTriangles, 0);

		curMesh.SetUVs(1, newUV);
		curMesh.RecalculateNormals();

		vertices = newVertices;

		if (!isRecompute) {
			colors = newColors;
			savedColors = new List<Color32>(newColors);
			curMesh.SetColors(newColors);
		}
		else {
			if (nbAtoms == selection.Count) {
				curMesh.SetColors(colors);
			}
			else {
				curMesh.SetColors(newColors);
				colors = newColors;
				savedColors = new List<Color32>(newColors);
			}

		}

		MeshFilter mf = currentGO.AddComponent<MeshFilter>();
		mf.mesh = curMesh;
		MeshRenderer mr = currentGO.AddComponent<MeshRenderer>();

		if (!isRecompute) {
			mr.material = new Material(Shader.Find("Custom/SurfaceVertexColor"));
		}

		meshGO = currentGO;

	}

	static bool diffResId(int id1, int id2) {
		int diff = 1;
		if (id1 < 0 && id2 > 0) {
			diff = 2;
		}
		if (id2 - id1 > diff) {
			return false;
		}
		return true;
	}

	public void recompute() {

		int savedCount = nbAtoms;
		Material savedMat = null;
		Transform savedPar = null;
		if (meshGO != null) {
			savedMat = meshGO.GetComponent<MeshRenderer>().sharedMaterial;
			savedPar = meshGO.transform.parent;
		}

		GameObject.DestroyImmediate(meshGO);

		DisplayTubeMesh(savedPar, true);

		meshGO.GetComponent<MeshRenderer>().material = savedMat;

		nbAtoms = selection.Count;

	}
	public override void Clean(){}

}
}