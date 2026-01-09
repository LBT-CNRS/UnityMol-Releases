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
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Linq;

namespace UMol {

public class UnityMolHBallMeshManager : UnityMolGenericRepresentationManager {

	public AtomRepresentationOptihb atomRep;
	private int nbAtoms;
	private bool[] texturesToUpdate;

	public GameObject AtomMeshParent;
	public float shininess = 0.0f;
	public float lastScale = 1.0f;
	private bool largeBB = false;
	public bool isAO = false;//AO for VDW metaphore
	public int idTex = 0;

	private KeyValuePair<int, int> keyValP = new KeyValuePair<int, int>();

	/// <summary>
	/// Initializes this instance of the manager.
	/// </summary>
	public override void Init(SubRepresentation umolRep) {

		if (isInit) {
			return;
		}

		atomRep = (AtomRepresentationOptihb) umolRep.atomRep;
		nbAtoms = atomRep.selection.atoms.Count;

		texturesToUpdate = new bool[atomRep.paramTextures.Length];
		for (int i = 0; i < atomRep.paramTextures.Length; i++)
			texturesToUpdate[i] = false;

		isInit = true;
		isEnabled = true;
		areSideChainsOn = true;
		areHydrogensOn = true;
		isBackboneOn = true;
	}

	public override void InitRT() {
	}

	public override void Clean() {

		if (atomRep.meshesGO != null) {
			for (int i = 0; i < atomRep.meshesGO.Count; i++) {
				if (atomRep.meshesGO[i] != null) {
					GameObject.Destroy(atomRep.meshesGO[i].GetComponent<MeshFilter>().sharedMesh);
					GameObject.Destroy(atomRep.meshesGO[i]);
				}
			}
		}

		if (atomRep.representationTransform != null) {
			GameObject.Destroy(atomRep.representationTransform.gameObject);
		}


		atomRep.atomColors.Clear();
		atomRep.meshesGO.Clear();
		atomRep.coordAtomTexture.Clear();
		atomRep.atomToId.Clear();
		texturesToUpdate = null;

		for (int i = 0; i < atomRep.paramTextures.Length; i++) {
			GameObject.Destroy(atomRep.paramTextures[i]);
		}
		GameObject.Destroy(AtomMeshParent);

		nbAtoms = 0;
		atomRep.atomToId = null;
		atomRep.paramTextures = null;
		atomRep.meshesGO = null;
		atomRep.coordAtomTexture = null;
		atomRep = null;
		isInit = false;
		isEnabled = false;

		// UnityMolMain.getRepresentationManager().UpdateActiveColliders();

	}

	public void ApplyTextures() {
		for (int i = 0; i < atomRep.paramTextures.Length; i++) {
			if (texturesToUpdate[i]) {
				atomRep.paramTextures[i].Apply(false, false);
			}
			texturesToUpdate[i] = false;
		}
	}

	/// <summary>
	/// Disables the renderers for all objects managed by the instance of the manager.
	/// </summary>
	public override void DisableRenderers() {
		if (atomRep.meshesGO == null)
			return;
		for (int i = 0; i < atomRep.meshesGO.Count; i++) {
			atomRep.meshesGO[i].GetComponent<Renderer>().enabled = false;
		}
		isEnabled = false;
		// UnityMolMain.getRepresentationManager().UpdateActiveColliders();
	}

	/// <summary>
	/// Enables the renderers for all objects managed by the instance of the manager.
	/// </summary>
	public override void EnableRenderers() {
		if (atomRep.meshesGO == null)
			return;
		for (int i = 0; i < atomRep.meshesGO.Count; i++) {
			atomRep.meshesGO[i].GetComponent<Renderer>().enabled = true;
		}
		isEnabled = true;
		// UnityMolMain.getRepresentationManager().UpdateActiveColliders();
	}

	public override void ShowShadows(bool show) {
		if (atomRep.meshesGO == null)
			return;
		for (int i = 0; i < atomRep.meshesGO.Count; i++) {
			if (show) {
				atomRep.meshesGO[i].GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.On;
				atomRep.meshesGO[i].GetComponent<Renderer>().receiveShadows = true;
			}
			else {
				atomRep.meshesGO[i].GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
				atomRep.meshesGO[i].GetComponent<Renderer>().receiveShadows = false;
			}
		}
	}

	/// <summary>
	/// Resets the positions of all atoms. Used when trajectory reading
	/// </summary>
	public void ResetPositions() {
		Vector4 offset = new Vector4(atomRep.offsetPos.x, atomRep.offsetPos.y, atomRep.offsetPos.z, 0.0f);
		for (int i = 0; i < atomRep.selection.Count; i++) {
			if (atomRep.coordAtomTexture.TryGetValue(i, out keyValP)) {
				Vector4 atomPos = atomRep.selection.atoms[i].PositionVec4 + offset;
				atomRep.paramTextures[keyValP.Key].SetPixel(keyValP.Value, 0, atomPos);

				texturesToUpdate[keyValP.Key] = true;
			}
		}
		cleanAO();
		ApplyTextures();
	}

	/// Set a large bounding box to avoid culling
	public void SetLargeBoundingVolume() {
		if (!largeBB) {
			if (atomRep.meshesGO != null && atomRep.meshesGO.Count != 0) {
				for (int i = 0; i < atomRep.meshesGO.Count; i++) {
					Bounds b = atomRep.meshesGO[i].GetComponent<MeshFilter>().sharedMesh.bounds;
					b.size = Vector3.one * 5000.0f;
					atomRep.meshesGO[i].GetComponent<MeshFilter>().sharedMesh.bounds = b;
				}
			}
		}
		largeBB = true;
	}

	public void SetTexture(int id) {
		Texture tex = null;
		if (id >= 0 && id < UnityMolMain.atomColors.textures.Length) {
			tex = (Texture) UnityMolMain.atomColors.textures[id];
			SetTexture(tex);
			idTex = id;
		}
	}
	private void SetTexture(Texture tex) {
		for (int i = 0; i < atomRep.meshesGO.Count; i++) {
			atomRep.meshesGO[i].GetComponent<Renderer>().sharedMaterial.SetTexture("_MatCap", tex);
		}
	}

    public override void SetColor(Color32 col, UnityMolSelection sele) {
            SetColors(col, sele.atoms);
    }

	public override void SetColor(Color32 col, UnityMolAtom a) {
		int idAtom = -1;
		if (atomRep.atomToId.TryGetValue(a, out idAtom)) {
			if (idAtom != -1) {
				SetColor(col, idAtom);
			}
		}
		ApplyTextures();
	}

	public void SetColor(Color32 col, int atomNum) {
		if (atomRep.coordAtomTexture.TryGetValue(atomNum, out keyValP)) {

			atomRep.paramTextures[keyValP.Key].SetPixel(keyValP.Value, 2, col);

			texturesToUpdate[keyValP.Key] = true;
			atomRep.atomColors[atomNum] = col;
		}
		//Call ApplyTextures to apply the changes !
	}

    public override void SetColors(Color32 col, List<UnityMolAtom> atoms) {
            foreach (UnityMolAtom a in atoms) {
                int idAtom = -1;
                if (atomRep.atomToId.TryGetValue(a, out idAtom)) {
                    if (idAtom != -1) {
                        SetColor(col, idAtom);
                    }
                }
            }
            ApplyTextures();
        }

    public override void SetColors(List<Color32> cols, List<UnityMolAtom> atoms) {
            if (atoms.Count != cols.Count) {
                Debug.LogError("Lengths of color list and atom list are different");
                return;
            }
            for (int i = 0; i < atoms.Count; i++) {
                UnityMolAtom a = atoms[i];
                Color32 col = cols[i];
                if (atomRep.atomToId.TryGetValue(a, out int idAtom)) {
                    SetColor(col, idAtom);
                }
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
			cleanAO();
			ApplyTextures();
		}
	}

	public void SetScale(float newScale, int idAtom) {
		if (atomRep.coordAtomTexture.TryGetValue(idAtom, out keyValP)) {
			atomRep.paramTextures[keyValP.Key].SetPixel(keyValP.Value, 8, Vector4.one * newScale);
			texturesToUpdate[keyValP.Key] = true;
			lastScale = newScale;
		}
		//Call ApplyTextures to apply the changes !
	}
	public void ResetScale() {
		for (int i = 0; i < nbAtoms; i++) {
			ResetScale(i);
		}
		cleanAO();
		ApplyTextures();
	}
	public void ResetScale(int idAtom) {
		SetScale(1.0f, idAtom);
		//Call ApplyTextures to apply the changes !
	}

	public override void SetDepthCueingStart(float v) {
		if (atomRep.meshesGO == null)
			return;
		foreach (GameObject meshGO in atomRep.meshesGO) {

			Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
			mats[0].SetFloat("_FogStart", v);
		}
	}

	public override void SetDepthCueingDensity(float v) {
		if (atomRep.meshesGO == null)
			return;
		foreach (GameObject meshGO in atomRep.meshesGO) {

			Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
			mats[0].SetFloat("_FogDensity", v);
		}
	}

	public override void EnableDepthCueing() {
		if (atomRep.meshesGO == null)
			return;
		foreach (GameObject meshGO in atomRep.meshesGO) {

			Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
			mats[0].SetFloat("_UseFog", 1.0f);
		}
	}

	public override void DisableDepthCueing() {
		if (atomRep.meshesGO == null)
			return;
		foreach (GameObject meshGO in atomRep.meshesGO) {

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
		cleanAO();
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
		cleanAO();
		areSideChainsOn = show;
	}

	public override void ShowBackbone(bool show) {
		for (int i = 0; i < nbAtoms; i++) {
			if (MDAnalysisSelection.isBackBone(atomRep.selection.atoms[i], atomRep.selection.bonds)) {
				ShowAtom(i, show);
			}
		}
		ApplyTextures();
		cleanAO();
		isBackboneOn = show;
	}

	public void ShowAtoms(HashSet<UnityMolAtom> atoms, bool show) {
		foreach (UnityMolAtom a in atoms) {
			ShowAtom(a, show);
		}
		cleanAO();
		ApplyTextures();
	}

	public void ShowAtom(int idAtom, bool show) {
		if (atomRep.coordAtomTexture.TryGetValue(idAtom, out keyValP)) {
			if (show) {
				atomRep.paramTextures[keyValP.Key].SetPixel(keyValP.Value, 7, Vector4.one);
			} else {
				atomRep.paramTextures[keyValP.Key].SetPixel(keyValP.Value, 7, Vector4.zero);
			}
			texturesToUpdate[keyValP.Key] = true;
		}
		//Call ApplyTextures to apply the changes !
	}
	public override void ShowAtom(UnityMolAtom a, bool show) {
		int idInCoord = 0;
		if (atomRep.atomToId.TryGetValue(a, out idInCoord)) {
			ShowAtom(idInCoord, show);
		}
		cleanAO();
		ApplyTextures();
	}

	public void ResetVisibility() {

		for (int i = 0; i < nbAtoms; i++) {
			if (atomRep.coordAtomTexture.TryGetValue(i, out keyValP)) {
				atomRep.paramTextures[keyValP.Key].SetPixel(keyValP.Value, 7, Vector4.one);
				texturesToUpdate[keyValP.Key] = true;
			}
		}
		cleanAO();
		ApplyTextures();
	}

	public void SetShininess(float val) {
		//Clamp and invert shininess
		shininess = val;
		float valShine = (shininess < 0.0001f ? 0.0f : 1.0f / shininess);

		for (int i = 0; i < atomRep.meshesGO.Count; i++) {
			atomRep.meshesGO[i].GetComponent<Renderer>().sharedMaterial.SetFloat("_Shininess", valShine);
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
		cleanAO();
	}

	public override void SetSizes(List<UnityMolAtom> atoms, List<float> sizes) {
		int i = 0;
		foreach (UnityMolAtom a in atoms) {
			SetScale(sizes[i], a, false);
			i++;
		}
		cleanAO();
		ApplyTextures();
	}

	public override void SetSizes(List<UnityMolAtom> atoms, float size) {
		foreach (UnityMolAtom a in atoms) {
			SetScale(size, a, false);
		}
		cleanAO();
		ApplyTextures();
	}


	public override void ResetSize(UnityMolAtom atom) {
		cleanAO();
		SetScale(1.0f, atom);
	}

	public override void ResetSizes() {
		cleanAO();
		foreach (UnityMolAtom a in atomRep.selection.atoms) {
			SetScale(1.0f, a, false);
		}
		ApplyTextures();
	}

	public override void ResetColor(UnityMolAtom atom) {
		SetColor(atom.color32, atom);
	}

	public override void ResetColors() {
		foreach (UnityMolAtom a in atomRep.selection.atoms) {
			SetColor(a.color32, a);
		}
		atomRep.colorationType = colorType.atom;
	}

	public Color32 getColorAtom(UnityMolAtom a) {
		int atomNum = -1;
		if (atomRep.atomToId.TryGetValue(a, out atomNum)) {
			if (atomNum != -1 && atomRep.coordAtomTexture.TryGetValue(atomNum, out keyValP)) {
				return atomRep.atomColors[atomNum];
			}
		}
		//Didn't find it
		return a.color32;
	}


	public void highlightAtom(UnityMolAtom a) {
		int idAtom = -1;
		if (atomRep.atomToId.TryGetValue(a, out idAtom)) {
			if (idAtom != -1) {
				if (atomRep.coordAtomTexture.TryGetValue(idAtom, out keyValP)) {

					atomRep.paramTextures[keyValP.Key].SetPixel(keyValP.Value, 9, Vector4.one);

					texturesToUpdate[keyValP.Key] = true;
				}
			}
		}
		//Call ApplyTextures to apply the changes !
	}

	public void removeHighlightAtom(UnityMolAtom a) {
		int idAtom = -1;
		if (atomRep.atomToId.TryGetValue(a, out idAtom)) {
			if (idAtom != -1) {
				if (atomRep.coordAtomTexture.TryGetValue(idAtom, out keyValP)) {

					atomRep.paramTextures[keyValP.Key].SetPixel(keyValP.Value, 9, Vector4.zero);

					texturesToUpdate[keyValP.Key] = true;
				}
			}
		}
		//Call ApplyTextures to apply the changes !
	}


	public override void HighlightRepresentation() {
		foreach (UnityMolAtom atom in atomRep.selection.atoms) {
			highlightAtom(atom);
		}
		ApplyTextures();
	}


	public override void DeHighlightRepresentation() {
		foreach (UnityMolAtom atom in atomRep.selection.atoms) {
			removeHighlightAtom(atom);
		}
		ApplyTextures();
	}

	public override void SetSmoothness(float val) {
		Debug.LogWarning("Cannot change this value for the hyperball representation");
	}
	public override void SetMetal(float val) {
		Debug.LogWarning("Cannot change this value for the hyperball representation");
	}

	public void updateAOInfo(List<Vector2> atlasinfo) {
		if (atlasinfo.Count != nbAtoms) {
			Debug.LogError("Failed to update AO atlas info");
			return;
		}
		Vector4 tmp = Vector4.zero;
		for (int i = 0; i < atlasinfo.Count; i++) {
			if (atomRep.coordAtomTexture.TryGetValue(i, out keyValP)) {
				tmp.x = atlasinfo[i].x;
				tmp.y = atlasinfo[i].y;
				atomRep.paramTextures[keyValP.Key].SetPixel(keyValP.Value, 10, tmp);

				texturesToUpdate[keyValP.Key] = true;
			}
		}

		ApplyTextures();
	}


	public void computeAO() {
		if (UnityMolMain.raytracingMode)
			return;
		GameObject tmp = new GameObject("TMPAO");
		tmp.transform.parent = atomRep.representationTransform;
		tmp.transform.localScale = Vector3.one;
		tmp.transform.localPosition = Vector3.zero;
		tmp.transform.localRotation = Quaternion.identity;
		AOHB ao = tmp.gameObject.AddComponent<AOHB>();
		ao.Run(atomRep.selection, this);
		isAO = true;

		GameObject.Destroy(ao);
		GameObject.Destroy(tmp);
	}
	public void cleanAO() {
		isAO = false;
		for (int i = 0; i < atomRep.meshesGO.Count; i++) {
			atomRep.meshesGO[i].GetComponent<Renderer>().sharedMaterial.SetFloat("_AOStrength", 0.0f);
		}
	}
	public override void UpdateLike() {
	}
	public override UnityMolRepresentationParameters Save() {
		UnityMolRepresentationParameters res = new UnityMolRepresentationParameters();

		res.repT.atomType = AtomType.optihb;
		res.colorationType = atomRep.colorationType;
		res.HBIdTex = idTex;

		if (atomRep.meshesGO == null || atomRep.meshesGO.Count == 0)
			return res;

		if (res.colorationType == colorType.custom) {
			int atomNum = 0;
			res.colorPerAtom = new Dictionary<UnityMolAtom, Color32>(atomRep.selection.atoms.Count);
			foreach (UnityMolAtom a in atomRep.selection.atoms) {
				if (atomRep.coordAtomTexture.TryGetValue(atomNum, out keyValP)) {
					res.colorPerAtom[a] = atomRep.atomColors[atomNum];
				}
			}
		}
		else if (res.colorationType == colorType.full) { //Get color of first atom/residue
			int atomNum = 0;
			foreach (UnityMolAtom a in atomRep.selection.atoms) {
				if (atomRep.coordAtomTexture.TryGetValue(atomNum, out keyValP)) {
					res.fullColor = atomRep.atomColors[atomNum];
					break;
				}
			}
		}
		else if (res.colorationType == colorType.bfactor) {
			res.bfactorStartColor = atomRep.bfactorStartCol;
			res.bfactorMidColor = atomRep.bfactorMidColor;
			res.bfactorEndColor = atomRep.bfactorEndCol;
		}
		res.smoothness = shininess;
		res.shadow = (atomRep.meshesGO[0].GetComponent<Renderer>().shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.On);
		res.HBScale = lastScale;
		res.HB_AO = isAO;

		return res;
	}
	public override void Restore(UnityMolRepresentationParameters savedParams) {

		if (savedParams.repT.atomType == AtomType.optihb) {
			if (savedParams.colorationType == colorType.full) {
				SetColor(savedParams.fullColor, atomRep.selection);
			}
			else if (savedParams.colorationType == colorType.custom) {
				List<Color32> colors = new List<Color32>(atomRep.selection.atoms.Count);
				List<UnityMolAtom> restoredAtoms = new List<UnityMolAtom>(atomRep.selection.atoms.Count);
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
			else if (savedParams.colorationType == colorType.resid) {
				colorByResid(atomRep.selection);
			}
			else if (savedParams.colorationType == colorType.resnum) {
				colorByResnum(atomRep.selection);
			}
			else if (savedParams.colorationType == colorType.rescharge) {
				colorByResCharge(atomRep.selection);
			}
			else if (savedParams.colorationType == colorType.bfactor) {
				colorByBfactor(atomRep.selection, savedParams.bfactorStartColor, savedParams.bfactorMidColor, savedParams.bfactorEndColor);
			}

			if (idTex != savedParams.HBIdTex)
				SetTexture(savedParams.HBIdTex);

			SetSizes(atomRep.selection.atoms, savedParams.HBScale);
			if (savedParams.HBScale == 3.0f && savedParams.HB_AO) {
				computeAO();
			}

			SetShininess(savedParams.smoothness);
			ShowShadows(savedParams.shadow);
			atomRep.colorationType = savedParams.colorationType;
		}
		else {
			Debug.LogError("Could not restore representation parameters");
		}

	}
}
}
