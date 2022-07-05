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
public class AtomEllipsoidRepresentation : AtomRepresentation {


	public List<GameObject> meshesGO;
	public List<AtomTrio> ellipsoidTriplet;
	public Dictionary<int, KeyValuePair<int, int> > coordAtomTexture;
	public Texture2D[] paramTextures;
	public Dictionary<UnityMolAtom, int> atomToId;
	public List<Color> atomColors;
	public bool withShadow = true;


	public AtomEllipsoidRepresentation(string structName, UnityMolSelection sel) {
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
		GameObject newRep = new GameObject("AtomEllipsoidRepresentation");
		newRep.transform.parent = representationParent;
		representationTransform = newRep.transform;

		newRep.transform.localPosition = Vector3.zero;
		newRep.transform.localRotation = Quaternion.identity;
		newRep.transform.localScale = Vector3.one;

		ellipsoidTriplet = getEllipsoidAtoms(sel);

		selection = sel;

		atomToId = new Dictionary<UnityMolAtom, int>();

		DisplayEllipsoids(newRep.transform, ellipsoidTriplet);

		nbAtoms = selection.Count;
	}

	private List<AtomTrio> getEllipsoidAtoms(UnityMolSelection sel) {

		// For bases G and A, ellipsoid is computed from the triplet CY-G1-G2 or CY-A1-A2
		// For U and C, we use CA-CY-C1 or CA-CY-U1

		List<AtomTrio> res = new List<AtomTrio>(sel.Count / 3);
		HashSet<UnityMolResidue> doneRes = new HashSet<UnityMolResidue>();

		foreach (UnityMolAtom a in sel.atoms) {
			if (doneRes.Contains(a.residue))
				continue;

			if ((a.residue.name == "G" || a.residue.name == "A") && a.residue.atoms.ContainsKey("CY") && a.residue.atoms.ContainsKey("CA")) {
				if (a.residue.atoms.ContainsKey("G1") && a.residue.atoms.ContainsKey("G2")) {
					AtomTrio t = new AtomTrio(a.residue.atoms["CY"], a.residue.atoms["G1"], a.residue.atoms["G2"]);
					doneRes.Add(a.residue);
					res.Add(t);
				}
				else if (a.residue.atoms.ContainsKey("A1") && a.residue.atoms.ContainsKey("A2")) {
					AtomTrio t = new AtomTrio(a.residue.atoms["CY"], a.residue.atoms["A1"], a.residue.atoms["A2"]);
					doneRes.Add(a.residue);
					res.Add(t);
				}
			}
			else if ((a.residue.name == "C" || a.residue.name == "U") && a.residue.atoms.ContainsKey("CY") && a.residue.atoms.ContainsKey("CA")) {
				if (a.residue.atoms.ContainsKey("C1")) {
					AtomTrio t = new AtomTrio(a.residue.atoms["CY"], a.residue.atoms["CA"], a.residue.atoms["C1"]);
					doneRes.Add(a.residue);
					res.Add(t);
				}
				else if (a.residue.atoms.ContainsKey("U1")) {
					AtomTrio t = new AtomTrio(a.residue.atoms["CY"], a.residue.atoms["CA"], a.residue.atoms["U1"]);
					doneRes.Add(a.residue);
					res.Add(t);
				}
			}
		}

		return res;
	}

	private void DisplayEllipsoids(Transform repParent, List<AtomTrio> ellipsoidTriplet) {

		meshesGO = new List<GameObject>();

		float brightness = 1.0f;

		for (int i = 0; i < ellipsoidTriplet.Count; i++) {


			GameObject currentGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
			currentGO.transform.name = "AtomEllipsoid";
			currentGO.transform.parent = repParent;
			GameObject.Destroy(currentGO.GetComponent<Collider>());

			Vector3 posAtom = barycenter(ellipsoidTriplet[i]);
			Vector3 target = ellipsoidTriplet[i].a3.residue.atoms["CA"].curWorldPosition;

			Vector3 wnormal = wellipsoidNormal(ellipsoidTriplet[i]);

			currentGO.transform.localScale = new Vector3(3.0f, 1.5f, 4.0f) * 1.5f;
			currentGO.transform.localPosition = posAtom;
			currentGO.transform.LookAt(target, wnormal);


			AssignMaterial(currentGO, brightness, ellipsoidTriplet[i].a3.color);

			atomToId[ellipsoidTriplet[i].a1] = i;
			atomToId[ellipsoidTriplet[i].a2] = i;
			atomToId[ellipsoidTriplet[i].a3] = i;

			meshesGO.Add(currentGO);

		}
	}


	void AssignMaterial(GameObject curGO, float brightness, Color col) {


		Material EllipMat = new Material(Shader.Find("UMol/HyperBalls GL_D3D"));


		//Set fixed parameters for the shader
		EllipMat.SetTexture("_MatCap", (Texture) Resources.Load("Images/MatCap/daphz05"));
		EllipMat.SetFloat("_Brightness", brightness);
		EllipMat.SetVector("_Color", col);
		curGO.GetComponent<Renderer>().sharedMaterial = EllipMat;

		if (withShadow) {
			curGO.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.On;
			curGO.GetComponent<Renderer>().receiveShadows = true;
		}
		else {
			curGO.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
			curGO.GetComponent<Renderer>().receiveShadows = false;
		}
	}

	public static Vector3 barycenter(AtomTrio t) {
		Vector3 res = t.a1.position + t.a2.position + t.a3.position;
		return res / 3.0f;
	}
	public static Vector3 wbarycenter(AtomTrio t) {
		Vector3 res = t.a1.curWorldPosition + t.a2.curWorldPosition + t.a3.curWorldPosition;
		return res / 3.0f;
	}
	public static Vector3 ellipsoidNormal(AtomTrio t) {
		return Vector3.Cross(t.a2.position - t.a1.position, t.a3.position - t.a1.position).normalized;
	}
	public static Vector3 wellipsoidNormal(AtomTrio t) {
		return Vector3.Cross(t.a2.curWorldPosition - t.a1.curWorldPosition, t.a3.curWorldPosition - t.a1.curWorldPosition).normalized;
	}
	public override void Clean() {}
}
}