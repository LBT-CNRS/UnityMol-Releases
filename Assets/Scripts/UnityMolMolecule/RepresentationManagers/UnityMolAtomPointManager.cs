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

public class UnityMolAtomPointManager : UnityMolGenericRepresentationManager {

	public AtomRepresentationPoint atomRep;


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

	public override void Clean() {

		if (atomRep.representationTransform != null) {
			GameObject.DestroyImmediate(atomRep.representationTransform.gameObject);
		}
		atomRep.atomToId.Clear();

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
		Vector3[] verts = mf.sharedMesh.vertices;
		for (int i = 0; i < atomRep.selection.Count; i++) {
			verts[i] = atomRep.selection.atoms[i].position;
		}
		mf.sharedMesh.vertices = verts;

	}

	public override void SetColor(Color col, UnityMolAtom a) {
		if (atomRep.atomToId.ContainsKey(a)) {
			int id = atomRep.atomToId[a];
			MeshFilter mf = atomRep.meshGO.GetComponent<MeshFilter>();
			Color32[] cols = mf.sharedMesh.colors32;
			cols[id] = col;
			mf.sharedMesh.colors32 = cols;
		}
	}

	public override void SetColor(Color col, UnityMolSelection sele) {
		SetColors(col, sele.atoms);
	}

	public override void SetColors(Color col, List<UnityMolAtom> atoms) {
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

	public override void SetColors(List<Color> cols, List<UnityMolAtom> atoms) {
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
	}

	public override void SetDepthCueingDensity(float v) {
	}

	public override void EnableDepthCueing() {
	}

	public override void DisableDepthCueing() {
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
		if (atoms.Count == atomRep.selection.Count) {
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
		SetColor(atom.color, atom);
	}

	public override void ResetColors() {
		MeshFilter mf = atomRep.meshGO.GetComponent<MeshFilter>();
		Color32[] cols = mf.sharedMesh.colors32;
		for (int i = 0; i < atomRep.selection.Count; i++) {
			UnityMolAtom a = atomRep.selection.atoms[i];

			if (atomRep.atomToId.ContainsKey(a)) {
				int id = atomRep.atomToId[a];
				cols[id] = a.color;
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

	public override UnityMolRepresentationParameters Save() {
		UnityMolRepresentationParameters res = new UnityMolRepresentationParameters();

		res.repT.atomType = AtomType.point;
		res.colorationType = atomRep.colorationType;

		if (res.colorationType == colorType.custom) {
			int atomNum = 0;
			res.colorPerAtom = new Dictionary<UnityMolAtom, Color32>(atomRep.selection.Count);
			// foreach (UnityMolAtom a in atomRep.selection.atoms) {
			// 	if (coordAtomTexture.TryGetValue(atomNum, out keyValP)) {
			// 		res.colorPerAtom[a] = colors[atomNum];
			// 	}
			// }
		}
		else if (res.colorationType == colorType.full) { //Get color of first atom/residue
			int atomNum = 0;
			// foreach (UnityMolAtom a in atomRep.selection.atoms) {
			// 	if (coordAtomTexture.TryGetValue(atomNum, out keyValP)) {
			// 		res.fullColor = colors[atomNum];
			// 		break;
			// 	}
			// }
		}
		else if (res.colorationType == colorType.bfactor) {
			res.bfactorStartColor = atomRep.bfactorStartCol;
			res.bfactorEndColor = atomRep.bfactorEndCol;
		}
		// res.shadow = (meshesGO[0].GetComponent<Renderer>().shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.On);
		// res.HBScale = lastScale;

		return res;
	}
	public override void Restore(UnityMolRepresentationParameters savedParams) {

		if (savedParams.repT.atomType == AtomType.point) {
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
			// SetShininess(savedParams.smoothness);
			// ShowShadows(savedParams.shadow);
			atomRep.colorationType = savedParams.colorationType;
		}
		else {
			Debug.LogError("Could not restore representation parameteres");
		}
	}

}
}
