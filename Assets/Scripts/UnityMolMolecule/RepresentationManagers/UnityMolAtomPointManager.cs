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

public class UnityMolAtomPointManager : UnityMolGenericRepresentationManager {

	public AtomRepresentationPoint atomRep;
	private Vector3[] vertices;
    public bool largeBB = false;


	/// <summary>
	/// Initializes this instance of the manager.
	/// </summary>
	public override void Init(SubRepresentation umolRep) {

		if (isInit) {
			return;
		}
		atomRep = (AtomRepresentationPoint) umolRep.atomRep;

		isInit = true;
		isEnabled = true;
		areSideChainsOn = true;
		areHydrogensOn = true;
		isBackboneOn = true;
	}

	public override void InitRT() {
	}

	public override void Clean() {

		if (atomRep.meshGO != null) {
			GameObject.Destroy(atomRep.meshGO.GetComponent<MeshFilter>().sharedMesh);
			GameObject.Destroy(atomRep.meshGO.GetComponent<MeshRenderer>().sharedMaterial);
		}
		if (atomRep.representationTransform != null) {
			GameObject.Destroy(atomRep.representationTransform.gameObject);
		}
		if (atomRep.atomToId != null)
			atomRep.atomToId.Clear();

		vertices = null;
		atomRep = null;
		isInit = false;
		isEnabled = false;

	}

	/// <summary>
	/// Disables the renderers for all objects managed by the instance of the manager.
	/// </summary>
	public override void DisableRenderers() {
		atomRep.meshGO.GetComponent<Renderer>().enabled = false;
		isEnabled = false;
	}

	/// <summary>
	/// Enables the renderers for all objects managed by the instance of the manager.
	/// </summary>
	public override void EnableRenderers() {
		atomRep.meshGO.GetComponent<Renderer>().enabled = true;
		isEnabled = true;
	}

	public override void ShowShadows(bool show) {
	}

	/// <summary>
	/// Resets the positions of all atoms. Used when trajectory reading
	/// </summary>
	public void ResetPositions() {
		MeshFilter mf = atomRep.meshGO.GetComponent<MeshFilter>();
		if (vertices == null)
			vertices = mf.sharedMesh.vertices;

		for (int i = 0; i < atomRep.selection.atoms.Count; i++) {
			vertices[i] = atomRep.selection.atoms[i].position;
		}
		mf.sharedMesh.vertices = vertices;

	}

	/// Set a large bounding box to avoid culling
	public void SetLargeBoundingVolume() {
		if (!largeBB) {
			if (atomRep.meshGO != null) {
				Bounds b = atomRep.meshGO.GetComponent<MeshFilter>().sharedMesh.bounds;
				b.size = Vector3.one * 5000.0f;
				atomRep.meshGO.GetComponent<MeshFilter>().sharedMesh.bounds = b;
			}
		}
		largeBB = true;
	}

	public override void SetColor(Color32 col, UnityMolAtom a) {
		if (atomRep.atomToId.ContainsKey(a)) {
			int id = atomRep.atomToId[a];
			MeshFilter mf = atomRep.meshGO.GetComponent<MeshFilter>();
			Color32[] cols = mf.sharedMesh.colors32;
			cols[id] = col;
			mf.sharedMesh.colors32 = cols;
		}
	}

	public override void SetColor(Color32 col, UnityMolSelection sele) {
		SetColors(col, sele.atoms);
	}

	public override void SetColors(Color32 col, List<UnityMolAtom> atoms) {
		MeshFilter mf = atomRep.meshGO.GetComponent<MeshFilter>();
		Color32[] cols = mf.sharedMesh.colors32;
		for (int i = 0; i < atoms.Count; i++) {
			UnityMolAtom a = atoms[i];

			if (atomRep.atomToId.ContainsKey(a)) {
				int id = atomRep.atomToId[a];
				cols[id] = col;
			}
		}
		mf.sharedMesh.colors32 = cols;
	}

	public override void SetColors(List<Color32> cols, List<UnityMolAtom> atoms) {
		MeshFilter mf = atomRep.meshGO.GetComponent<MeshFilter>();
		Color32[] colors = mf.sharedMesh.colors32;
		for (int i = 0; i < atoms.Count; i++) {
			UnityMolAtom a = atoms[i];

			if (atomRep.atomToId.ContainsKey(a)) {
				int id = atomRep.atomToId[a];
				colors[id] = cols[i];
			}
		}
		mf.sharedMesh.colors32 = colors;
	}

	public override void SetDepthCueingStart(float v) {
		Material[] mats = atomRep.meshGO.GetComponent<Renderer>().sharedMaterials;
		mats[0].SetFloat("_FogStart", v);
	}

	public override void SetDepthCueingDensity(float v) {
		Material[] mats = atomRep.meshGO.GetComponent<Renderer>().sharedMaterials;
		mats[0].SetFloat("_FogDensity", v);
	}

	public override void EnableDepthCueing() {
		Material[] mats = atomRep.meshGO.GetComponent<Renderer>().sharedMaterials;
		mats[0].SetFloat("_UseFog", 1.0f);

	}

	public override void DisableDepthCueing() {
		Material[] mats = atomRep.meshGO.GetComponent<Renderer>().sharedMaterials;
		mats[0].SetFloat("_UseFog", 0.0f);
	}

	public override void ShowHydrogens(bool show) {
		Debug.LogWarning("Cannot show/hide parts of the point representation");
	}

	public override void ShowSideChains(bool show) {
		Debug.LogWarning("Cannot show/hide parts of the point representation");
	}

	public override void ShowBackbone(bool show) {
		Debug.LogWarning("Cannot show/hide parts of the point representation");
	}

	public override void ShowAtom(UnityMolAtom a, bool show) {
		Debug.LogWarning("Cannot show/hide parts of the point representation");
	}

	public override void updateWithTrajectory() {
		ResetPositions();
        SetLargeBoundingVolume();
	}

	public override void updateWithModel() {
		ResetPositions();
	}

	public override void SetSize(UnityMolAtom atom, float size) {
		Debug.LogWarning("Cannot change this value for the point representation");
	}

	public override void SetSizes(List<UnityMolAtom> atoms, List<float> sizes) {
		Debug.LogWarning("Cannot change this value for the point representation");
	}

	public override void SetSizes(List<UnityMolAtom> atoms, float size) {
		if (atoms.Count == atomRep.selection.atoms.Count || atomRep.selection.extractTrajFrame) {
			atomRep.meshGO.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_PointSize", size);
		}
		else {
			Debug.LogWarning("Cannot change this value for the point representation");
		}
	}

	public override void ResetSize(UnityMolAtom atom) {
		Debug.LogWarning("Cannot change this value for the point representation");
	}

	public override void ResetSizes() {

		atomRep.meshGO.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_PointSize", 0.01f);
	}

	public override void ResetColor(UnityMolAtom atom) {
		SetColor(atom.color32, atom);
	}

	public override void ResetColors() {
		MeshFilter mf = atomRep.meshGO.GetComponent<MeshFilter>();
		Color32[] cols = mf.sharedMesh.colors32;
		for (int i = 0; i < atomRep.selection.atoms.Count; i++) {
			UnityMolAtom a = atomRep.selection.atoms[i];

			if (atomRep.atomToId.ContainsKey(a)) {
				int id = atomRep.atomToId[a];
				cols[id] = a.color32;
			}
		}
		mf.sharedMesh.colors32 = cols;
	}

	public override void HighlightRepresentation() {
	}

	public override void DeHighlightRepresentation() {
	}

	public override void SetSmoothness(float val) {
		Debug.LogWarning("Cannot change this value for the point representation");
	}

	public override void SetMetal(float val) {
		Debug.LogWarning("Cannot change this value for the point representation");
	}
	public override void UpdateLike() {
	}

	public override UnityMolRepresentationParameters Save() {
		UnityMolRepresentationParameters res = new UnityMolRepresentationParameters();
		MeshFilter mf = atomRep.meshGO.GetComponent<MeshFilter>();

		res.repT.atomType = AtomType.point;
		res.colorationType = atomRep.colorationType;

		if (res.colorationType == colorType.custom) {
			int atomNum = 0;
			res.colorPerAtom = new Dictionary<UnityMolAtom, Color32>(atomRep.selection.atoms.Count);
			Color32[] colors = mf.sharedMesh.colors32;
			foreach (UnityMolAtom a in atomRep.selection.atoms) {
				res.colorPerAtom[a] = colors[atomNum++];
			}
		}
		else if (res.colorationType == colorType.full) { //Get color of first atom/residue
			int atomNum = 0;
			Color32[] colors = mf.sharedMesh.colors32;
			foreach (UnityMolAtom a in atomRep.selection.atoms) {
				res.fullColor = colors[atomNum];
				break;

			}
		}
		else if (res.colorationType == colorType.bfactor) {
			res.bfactorStartColor = atomRep.bfactorStartCol;
			res.bfactorMidColor = atomRep.bfactorMidColor;
			res.bfactorEndColor = atomRep.bfactorEndCol;
		}
		// res.shadow = (meshesGO[0].GetComponent<Renderer>().shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.On);
		// res.HBScale = lastScale;
		res.PointSize = atomRep.meshGO.GetComponent<MeshRenderer>().sharedMaterial.GetFloat("_PointSize");

		return res;
	}
	public override void Restore(UnityMolRepresentationParameters savedParams) {

		if (savedParams.repT.atomType == AtomType.point) {
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
			else if (savedParams.colorationType == colorType.rescharge) {
				colorByResCharge(atomRep.selection);
			}
			else if (savedParams.colorationType == colorType.resnum) {
				colorByResnum(atomRep.selection);
			}
			else if (savedParams.colorationType == colorType.resid) {
				colorByResid(atomRep.selection);
			}
			else if (savedParams.colorationType == colorType.bfactor) {
				colorByBfactor(atomRep.selection, savedParams.bfactorStartColor, savedParams.bfactorMidColor, savedParams.bfactorEndColor);
			}

			SetSizes(atomRep.selection.atoms, savedParams.PointSize);

			atomRep.colorationType = savedParams.colorationType;
		}
		else {
			Debug.LogError("Could not restore representation parameters");
		}
	}

}
}
