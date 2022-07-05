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

public class UnityMolAtomBondOrderManager : UnityMolGenericRepresentationManager {

	public AtomRepresentationBondOrder atomRep;
	private int nbAtoms;
	private List<GameObject> meshesGO;
	private Dictionary<int, KeyValuePair<int, int> > coordAtomTexture;
	private Dictionary<UnityMolAtom, int> atomToCoord;
	private bool[] texturesToUpdate;
	private Texture2D[] texturesAtoms;
	private List<Color> colors;

	public GameObject AtomMeshParent;
	public float shininess = 0.0f;
	public float lastScale = 1.0f;
	private bool largeBB = false;

	private KeyValuePair<int, int> keyValP = new KeyValuePair<int, int>();

	/// <summary>
	/// Initializes this instance of the manager.
	/// </summary>
	public override void Init(SubRepresentation umolRep) {

		if (isInit) {
			return;
		}

		atomRep = (AtomRepresentationBondOrder) umolRep.atomRep;
		meshesGO = atomRep.meshesGO;
		nbAtoms = atomRep.selection.atoms.Count;
		texturesAtoms = atomRep.paramTextures;
		coordAtomTexture = atomRep.coordAtomTexture;
		atomToCoord = atomRep.atomToId;

		texturesToUpdate = new bool[texturesAtoms.Length];
		for (int i = 0; i < texturesAtoms.Length; i++)
			texturesToUpdate[i] = false;

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

		colors.Clear();
		meshesGO.Clear();
		coordAtomTexture.Clear();
		atomToCoord.Clear();
		texturesToUpdate = null;
		for (int i = 0; i < texturesAtoms.Length; i++) {
			GameObject.DestroyImmediate(texturesAtoms[i]);
		}
		GameObject.DestroyImmediate(AtomMeshParent);

		nbAtoms = 0;
		atomRep = null;
		atomToCoord = null;
		texturesAtoms = null;
		meshesGO = null;
		coordAtomTexture = null;
		isInit = false;
		isEnabled = false;

		// UnityMolMain.getRepresentationManager().UpdateActiveColliders();

	}

	public void ApplyTextures() {
		for (int i = 0; i < texturesAtoms.Length; i++) {
			if (texturesToUpdate[i]) {
				texturesAtoms[i].Apply(false, false);
			}
			texturesToUpdate[i] = false;
		}
	}

	/// <summary>
	/// Disables the renderers for all objects managed by the instance of the manager.
	/// </summary>
	public override void DisableRenderers() {
		for (int i = 0; i < meshesGO.Count; i++) {
			meshesGO[i].GetComponent<Renderer>().enabled = false;
		}
		isEnabled = false;
		// UnityMolMain.getRepresentationManager().UpdateActiveColliders();
	}

	/// <summary>
	/// Enables the renderers for all objects managed by the instance of the manager.
	/// </summary>
	public override void EnableRenderers() {
		for (int i = 0; i < meshesGO.Count; i++) {
			meshesGO[i].GetComponent<Renderer>().enabled = true;
		}
		isEnabled = true;
		// UnityMolMain.getRepresentationManager().UpdateActiveColliders();
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
		for (int i = 0; i < nbAtoms; i++) {
			if (coordAtomTexture.TryGetValue(i, out keyValP)) {
				Vector4 atomPos = atomRep.selection.atoms[i].PositionVec4;
				texturesAtoms[keyValP.Key].SetPixel(keyValP.Value, 0, atomPos);

				texturesToUpdate[keyValP.Key] = true;
			}
		}
		ApplyTextures();
	}

	/// Set a large bounding box to avoid culling
	public void SetLargeBoundingVolume() {
		if (!largeBB) {
			if (meshesGO != null && meshesGO.Count != 0) {
				for (int i = 0; i < meshesGO.Count; i++) {
					Bounds b = meshesGO[i].GetComponent<MeshFilter>().mesh.bounds;
					b.size = Vector3.one * 5000.0f;
					meshesGO[i].GetComponent<MeshFilter>().mesh.bounds = b;
				}
			}
		}
		largeBB = true;
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
		ApplyTextures();
	}

	public override void SetColor(Color col, UnityMolAtom a) {
		int idAtom = -1;
		if (atomRep.atomToId.TryGetValue(a, out idAtom)) {
			if (idAtom != -1) {
				SetColor(col, idAtom);
			}
		}
		ApplyTextures();
	}

	public void SetColor(Color col, int atomNum) {
		if (coordAtomTexture.TryGetValue(atomNum, out keyValP)) {

			texturesAtoms[keyValP.Key].SetPixel(keyValP.Value, 2, col);

			texturesToUpdate[keyValP.Key] = true;
			colors[atomNum] = col;
		}
		//Call ApplyTextures to apply the changes !
	}


	public override void SetColors(Color col, List<UnityMolAtom> atoms) {
		foreach (UnityMolAtom a in atoms) {
			SetColor(col, a);
		}
		ApplyTextures();
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
		ApplyTextures();
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
		if (now) {
			ApplyTextures();
		}
	}

	public void SetScale(float newScale, int idAtom) {
		if (coordAtomTexture.TryGetValue(idAtom, out keyValP)) {
			texturesAtoms[keyValP.Key].SetPixel(keyValP.Value, 8, Vector4.one * newScale);
			texturesToUpdate[keyValP.Key] = true;
			lastScale = newScale;
		}
		//Call ApplyTextures to apply the changes !
	}
	public void ResetScale() {
		for (int i = 0; i < nbAtoms; i++) {
			ResetScale(i);
		}
		ApplyTextures();
	}
	public void ResetScale(int idAtom) {
		SetScale(1.0f, idAtom);
		//Call ApplyTextures to apply the changes !
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
		ApplyTextures();
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
		ApplyTextures();
		areSideChainsOn = show;
	}

	public override void ShowBackbone(bool show) {
		for (int i = 0; i < nbAtoms; i++) {
			if (MDAnalysisSelection.isBackBone(atomRep.selection.atoms[i], atomRep.selection.bonds)) {
				ShowAtom(i, show);
			}
		}
		ApplyTextures();
		isBackboneOn = show;
	}

	public void ShowAtoms(HashSet<UnityMolAtom> atoms, bool show) {
		foreach (UnityMolAtom a in atoms) {
			ShowAtom(a, show);
		}
		ApplyTextures();
	}

	public void ShowAtom(int idAtom, bool show) {
		if (coordAtomTexture.TryGetValue(idAtom, out keyValP)) {
			if (show) {
				texturesAtoms[keyValP.Key].SetPixel(keyValP.Value, 7, Vector4.one);
			} else {
				texturesAtoms[keyValP.Key].SetPixel(keyValP.Value, 7, Vector4.zero);
			}
			texturesToUpdate[keyValP.Key] = true;
		}
		//Call ApplyTextures to apply the changes !
	}
	public override void ShowAtom(UnityMolAtom a, bool show) {
		int idInCoord = 0;
		if (atomToCoord.TryGetValue(a, out idInCoord)) {
			ShowAtom(idInCoord, show);
		}
		ApplyTextures();
	}

	public void ResetVisibility() {

		for (int i = 0; i < nbAtoms; i++) {
			if (coordAtomTexture.TryGetValue(i, out keyValP)) {
				texturesAtoms[keyValP.Key].SetPixel(keyValP.Value, 7, Vector4.one);
				texturesToUpdate[keyValP.Key] = true;
			}
		}
		ApplyTextures();
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
		SetLargeBoundingVolume();
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
		ApplyTextures();
	}

	public override void SetSizes(List<UnityMolAtom> atoms, float size) {
		foreach (UnityMolAtom a in atoms) {
			SetScale(size, a, false);
		}
		ApplyTextures();
	}


	public override void ResetSize(UnityMolAtom atom) {
		SetScale(1.0f, atom);
	}

	public override void ResetSizes() {
		foreach (UnityMolAtom a in atomRep.selection.atoms) {
			SetScale(1.0f, a, false);
		}
		ApplyTextures();
	}

	public override void ResetColor(UnityMolAtom atom) {
		SetColor(atom.color, atom);
	}

	public override void ResetColors() {
		foreach (UnityMolAtom a in atomRep.selection.atoms) {
			SetColor(a.color, a);
		}
	}


	public override void HighlightRepresentation() {
	}


	public override void DeHighlightRepresentation() {
	}

	public override void SetSmoothness(float val) {
		Debug.LogWarning("Cannot change this value for the hyperball representation");
	}
	public override void SetMetal(float val) {
		Debug.LogWarning("Cannot change this value for the hyperball representation");
	}
	public override UnityMolRepresentationParameters Save() {
		UnityMolRepresentationParameters res = new UnityMolRepresentationParameters();

		res.repT.atomType = AtomType.bondorder;
		res.colorationType = atomRep.colorationType;

		if (res.colorationType == colorType.custom) {
			int atomNum = 0;
			res.colorPerAtom = new Dictionary<UnityMolAtom, Color32>(atomRep.selection.Count);
			foreach (UnityMolAtom a in atomRep.selection.atoms) {
				if (coordAtomTexture.TryGetValue(atomNum, out keyValP)) {
					res.colorPerAtom[a] = colors[atomNum];
				}
			}
		}
		else if (res.colorationType == colorType.full) { //Get color of first atom/residue
			int atomNum = 0;
			foreach (UnityMolAtom a in atomRep.selection.atoms) {
				if (coordAtomTexture.TryGetValue(atomNum, out keyValP)) {
					res.fullColor = colors[atomNum];
					break;
				}
			}
		}
		else if (res.colorationType == colorType.bfactor) {
			res.bfactorStartColor = atomRep.bfactorStartCol;
			res.bfactorEndColor = atomRep.bfactorEndCol;
		}
		res.smoothness = shininess;
		res.shadow = (meshesGO[0].GetComponent<Renderer>().shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.On);
		// res.HBScale = lastScale;

		return res;
	}
	public override void Restore(UnityMolRepresentationParameters savedParams) {

		if (savedParams.repT.atomType == AtomType.bondorder) {
			if (savedParams.colorationType == colorType.full) {
				SetColor(savedParams.fullColor, atomRep.selection);
			}
			else if (savedParams.colorationType == colorType.custom) {
				List<Color> colors = new List<Color>(atomRep.selection.Count);
				List<UnityMolAtom> restoredAtoms = new List<UnityMolAtom>(atomRep.selection.Count);
				foreach (UnityMolAtom a in atomRep.selection.atoms) {
					if (savedParams.colorPerAtom.ContainsKey(a)) {
						colors.Add(savedParams.colorPerAtom[a]);
						restoredAtoms.Add(a);
					}
				}
				SetColors(colors, restoredAtoms);
			}
			else if (savedParams.colorationType == colorType.defaultCartoon) {
				//Do nothing !
			}
			else if (savedParams.colorationType == colorType.res) {
				colorByRes(atomRep.selection);
			}
			else if (savedParams.colorationType == colorType.chain) {
				colorByChain(atomRep.selection);
			}
			else if (savedParams.colorationType == colorType.hydro) {
				colorByHydro(atomRep.selection);
			}
			else if (savedParams.colorationType == colorType.seq) {
				colorBySequence(atomRep.selection);
			}
			else if (savedParams.colorationType == colorType.charge) {
				colorByCharge(atomRep.selection);
			}
			else if (savedParams.colorationType == colorType.restype) {
				colorByResType(atomRep.selection);
			}
			else if (savedParams.colorationType == colorType.rescharge) {
				colorByResCharge(atomRep.selection);
			}
			else if (savedParams.colorationType == colorType.bfactor) {
				colorByBfactor(atomRep.selection, savedParams.bfactorStartColor, savedParams.bfactorEndColor);
			}

			// SetSizes(atomRep.selection.atoms, savedParams.HBScale);

			SetShininess(savedParams.smoothness);
			ShowShadows(savedParams.shadow);
			atomRep.colorationType = savedParams.colorationType;
		}
		else {
			Debug.LogError("Could not restore representation parameteres");
		}

	}

}
}