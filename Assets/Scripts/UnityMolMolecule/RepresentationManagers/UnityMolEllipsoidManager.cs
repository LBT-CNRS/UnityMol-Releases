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
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Linq;

namespace UMol {

public class UnityMolEllipsoidManager : UnityMolGenericRepresentationManager {

	public AtomEllipsoidRepresentation atomRep;
	private int nbAtoms;
	private List<GameObject> meshesGO;
	private List<Color> colors;

	public GameObject AtomMeshParent;
	public float shininess = 0.0f;
	public float lastScale = 1.0f;

	private KeyValuePair<int, int> keyValP = new KeyValuePair<int, int>();

	/// <summary>
	/// Initializes this instance of the manager.
	/// </summary>
	public override void Init(SubRepresentation umolRep) {

		if (isInit) {
			return;
		}

		atomRep = (AtomEllipsoidRepresentation) umolRep.atomRep;
		meshesGO = atomRep.meshesGO;
		nbAtoms = atomRep.meshesGO.Count;

		colors = atomRep.atomColors;

		isInit = true;
		isEnabled = true;
		areSideChainsOn = true;
		areHydrogensOn = true;
		isBackboneOn = true;
	}

	public override void Clean() {


		if (atomRep.representationTransform != null) {
			GameObject.DestroyImmediate(atomRep.representationTransform.gameObject);
		}

		for (int i = 0; i < meshesGO.Count; i++) {
			GameObject.DestroyImmediate(meshesGO[i]);
		}

		atomRep.atomToId.Clear();
		atomRep.ellipsoidTriplet.Clear();
		meshesGO.Clear();
		GameObject.DestroyImmediate(AtomMeshParent);

		atomRep.ellipsoidTriplet = null;
		atomRep.atomToId = null;
		nbAtoms = 0;
		atomRep = null;
		meshesGO = null;
		isInit = false;
		isEnabled = false;

	}

	/// <summary>
	/// Disables the renderers for all objects managed by the instance of the manager.
	/// </summary>
	public override void DisableRenderers() {
		for (int i = 0; i < meshesGO.Count; i++) {
			meshesGO[i].GetComponent<Renderer>().enabled = false;
		}
		isEnabled = false;
	}

	/// <summary>
	/// Enables the renderers for all objects managed by the instance of the manager.
	/// </summary>
	public override void EnableRenderers() {
		for (int i = 0; i < meshesGO.Count; i++) {
			meshesGO[i].GetComponent<Renderer>().enabled = true;
		}
		isEnabled = true;
	}

	public override void ShowShadows(bool show) {
		for (int i = 0; i < meshesGO.Count; i++) {
			if (show) {
				meshesGO[i].GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.On;
				meshesGO[i].GetComponent<Renderer>().receiveShadows = true;
			}
			else {
				meshesGO[i].GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
				meshesGO[i].GetComponent<Renderer>().receiveShadows = false;
			}
		}
	}

	/// <summary>
	/// Resets the positions of all atoms. Used when trajectory reading
	/// </summary>
	public void ResetPositions() {
		for (int i = 0; i < atomRep.ellipsoidTriplet.Count; i++) {
			Vector3 posAtom = AtomEllipsoidRepresentation.barycenter(atomRep.ellipsoidTriplet[i]);
			Vector3 target = atomRep.ellipsoidTriplet[i].a3.residue.atoms["CA"].curWorldPosition;

			Vector3 wnormal = AtomEllipsoidRepresentation.wellipsoidNormal(atomRep.ellipsoidTriplet[i]);

			meshesGO[i].transform.localPosition = posAtom;
			meshesGO[i].transform.LookAt(target, wnormal);

		}
	}

	public void SetTexture(Texture tex) {
		for (int i = 0; i < meshesGO.Count; i++) {
			meshesGO[i].GetComponent<Renderer>().sharedMaterial.SetTexture("_MatCap", tex);
		}
	}
	public void ResetTexture() {
		Texture tex = (Texture) Resources.Load("Images/MatCap/daphz05");
		for (int i = 0; i < meshesGO.Count; i++) {
			meshesGO[i].GetComponent<Renderer>().sharedMaterial.SetTexture("_MatCap", tex);
		}
	}

	public override void SetColor(Color col, UnityMolSelection sele) {
		foreach (UnityMolAtom a in sele.atoms) {
			SetColor(col, a);
		}
	}

	public override void SetColor(Color col, UnityMolAtom a) {
		int idAtom = -1;
		if (atomRep.atomToId.TryGetValue(a, out idAtom)) {
			if (idAtom != -1) {
				SetColor(col, idAtom);
			}
		}
	}

	public void SetColor(Color col, int ellipNum) {
		if (ellipNum >= 0 && ellipNum < meshesGO.Count) {
			meshesGO[ellipNum].GetComponent<Renderer>().sharedMaterial.SetVector("_Color", col);
		}
	}


	public override void SetColors(Color col, List<UnityMolAtom> atoms) {
		foreach (UnityMolAtom a in atoms) {
			SetColor(col, a);
		}
	}

	public override void SetColors(List<Color> cols, List<UnityMolAtom> atoms) {
		if (atoms.Count != cols.Count) {
			Debug.LogError("Lengths of color list and atom list are different");
			return;
		}
		for (int i = 0; i < atoms.Count; i++) {
			UnityMolAtom a = atoms[i];
			Color col = cols[i];
			SetColor(col, a);
		}
	}
	public void SetScale(float newScale, UnityMolAtom a, bool now = true) {
		int idAtom = -1;
		if (atomRep.atomToId.TryGetValue(a, out idAtom)) {
			if (idAtom != -1) {
				SetScale(newScale, idAtom);
			}
		}
		else {
			// Debug.LogWarning("Atom " + a + " not found in the representation");
		}
	}

	public void SetScale(float newScale, int ellipNum) {
		if (ellipNum >= 0 && ellipNum < meshesGO.Count) {
			meshesGO[ellipNum].GetComponent<Renderer>().sharedMaterial.SetFloat("_Rayon", 0.5f * newScale);
		}
	}
	public void ResetScale() {
		for (int i = 0; i < nbAtoms; i++) {
			ResetScale(i);
		}
	}
	public void ResetScale(int ellipNum) {
		SetScale(1.0f, ellipNum);
	}

	public override void ShowHydrogens(bool show) {
		for (int i = 0; i < nbAtoms; i++) {
			if (atomRep.selection.atoms[i].type == "H") {
				if (show && !areSideChainsOn && MDAnalysisSelection.isSideChain(atomRep.selection.atoms[i], atomRep.selection.bonds)) {
				}
				else {
					ShowAtom(i, show);
				}
			}
		}
		areHydrogensOn = show;
	}

	public override void ShowSideChains(bool show) {
		for (int i = 0; i < nbAtoms; i++) {
			if (show && !areHydrogensOn && atomRep.selection.atoms[i].type == "H" ) {
			}
			else {
				if (MDAnalysisSelection.isSideChain(atomRep.selection.atoms[i], atomRep.selection.bonds)) {
					ShowAtom(i, show);
				}
			}
		}
		areSideChainsOn = show;
	}

	public override void ShowBackbone(bool show) {
		for (int i = 0; i < nbAtoms; i++) {
			if (MDAnalysisSelection.isBackBone(atomRep.selection.atoms[i], atomRep.selection.bonds)) {
				ShowAtom(i, show);
			}
		}
		isBackboneOn = show;
	}

	public void ShowAtoms(HashSet<UnityMolAtom> atoms, bool show) {
		foreach (UnityMolAtom a in atoms) {
			ShowAtom(a, show);
		}
	}

	public void ShowAtom(int ellipNum, bool show) {
		if (ellipNum >= 0 && ellipNum < meshesGO.Count) {
			meshesGO[ellipNum].GetComponent<Renderer>().enabled = show;
		}
	}

	public override void ShowAtom(UnityMolAtom a, bool show) {
		int id = 0;
		if (atomRep.atomToId.TryGetValue(a, out id)) {
			ShowAtom(id, show);
		}
	}

	public void ResetVisibility() {

		for (int i = 0; i < nbAtoms; i++) {
			ShowAtom(i, true);
		}
	}

	public void SetShininess(float val) {
		//Clamp and invert shininess
		shininess = val;
		float valShine = (shininess < 0.0001f ? 0.0f : 1.0f / shininess);

		for (int i = 0; i < meshesGO.Count; i++) {
			meshesGO[i].GetComponent<Renderer>().sharedMaterial.SetFloat("_Shininess", valShine);
		}
	}

	public void ResetShininess() {
		SetShininess(0.0f);
	}

	public override void updateWithTrajectory() {
		ResetPositions();
	}

	public override void updateWithModel() {
		ResetPositions();
	}

	public override void SetSize(UnityMolAtom atom, float size) {
		SetScale(size, atom);
	}

	public override void SetSizes(List<UnityMolAtom> atoms, List<float> sizes) {
		int i = 0;
		foreach (UnityMolAtom a in atoms) {
			SetScale(sizes[i], a, false);
			i++;
		}
	}

	public override void SetSizes(List<UnityMolAtom> atoms, float size) {
		foreach (UnityMolAtom a in atoms) {
			SetScale(size, a, false);
		}
	}


	public override void ResetSize(UnityMolAtom atom) {
		SetScale(1.0f, atom);
	}

	public override void ResetSizes() {
		foreach (UnityMolAtom a in atomRep.selection.atoms) {
			SetScale(1.0f, a, false);
		}
	}

	public override void ResetColor(UnityMolAtom atom) {
		int id = 0;
		if (atomRep.atomToId.TryGetValue(atom, out id)) {
			SetColor(atomRep.ellipsoidTriplet[id].a3.color, atom);
		}
	}

	public override void ResetColors() {
		foreach (UnityMolAtom a in atomRep.selection.atoms) {
			ResetColor(a);
		}
	}

	public override void SetDepthCueingStart(float v) {
		if (meshesGO == null)
			return;
		foreach (GameObject meshGO in meshesGO) {

			Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
			mats[0].SetFloat("_FogStart", v);
		}
	}

	public override void SetDepthCueingDensity(float v) {
		if (meshesGO == null)
			return;
		foreach (GameObject meshGO in meshesGO) {

			Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
			mats[0].SetFloat("_FogDensity", v);
		}
	}

	public override void EnableDepthCueing() {
		if (meshesGO == null)
			return;
		foreach (GameObject meshGO in meshesGO) {

			Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
			mats[0].SetFloat("_UseFog", 1.0f);
		}
	}

	public override void DisableDepthCueing() {
		if (meshesGO == null)
			return;
		foreach (GameObject meshGO in meshesGO) {

			Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
			mats[0].SetFloat("_UseFog", 0.0f);
		}
	}

	public void highlightAtom(UnityMolAtom a) {

	}

	public void removeHighlightAtom(UnityMolAtom a) {
	}


	public override void HighlightRepresentation() {
		foreach (UnityMolAtom atom in atomRep.selection.atoms) {
			highlightAtom(atom);
		}
	}


	public override void DeHighlightRepresentation() {
		foreach (UnityMolAtom atom in atomRep.selection.atoms) {
			removeHighlightAtom(atom);
		}
	}

	public override void SetSmoothness(float val) {
		Debug.LogWarning("Cannot change this value for the hyperball representation");
	}
	public override void SetMetal(float val) {
		Debug.LogWarning("Cannot change this value for the hyperball representation");
	}
	public override UnityMolRepresentationParameters Save(){
		UnityMolRepresentationParameters res = new UnityMolRepresentationParameters();
		return res;
	}
    public override void Restore(UnityMolRepresentationParameters savedParams){
        
    }
}
}