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
using Unity.Mathematics;

namespace UMol {
public class UnityMolTubeManager : UnityMolGenericRepresentationManager {

	public AtomRepresentationTube atomRep;


	/// <summary>
	/// Disables the renderers for all objects managed by the instance of the manager.
	/// </summary>
	public override void DisableRenderers() {
		isEnabled = false;
		if (atomRep.meshGO == null)
			return;
		atomRep.meshGO.GetComponent<Renderer>().enabled = false;
		// UnityMolMain.getRepresentationManager().UpdateActiveColliders();
	}


/// <summary>
/// Enables the renderers for all objects managed by the instance of the manager.
/// </summary>
	public override void EnableRenderers() {
		isEnabled = true;
		if (atomRep.meshGO == null)
			return;
		atomRep.meshGO.GetComponent<Renderer>().enabled = true;
		// UnityMolMain.getRepresentationManager().UpdateActiveColliders();
	}

/// <summary>
/// Initializes this instance of the manager.
/// </summary>
	public override void Init(SubRepresentation umolRep) {

		if (isInit) {
			return;
		}

		atomRep = (AtomRepresentationTube) umolRep.atomRep;

        if (UnityMolMain.raytracingMode)
            InitRT();

		isInit = true;
		isEnabled = true;
		areSideChainsOn = true;
		areHydrogensOn = true;
		isBackboneOn = true;
	}

	public override void InitRT() {
		if (rtos == null) {
			RaytracedObject rto = atomRep.meshGO.AddComponent<RaytracedObject>();
			rtos = new List<RaytracedObject>() {rto};
		}
	}

	public override void Clean() {

		if (rtos != null) {
			for (int i = 0; i < rtos.Count; i++) {
				GameObject.Destroy(rtos[i]);
			}
			rtos.Clear();
		}

		GameObject parent = null;
		if (atomRep.meshGO != null) {
			parent = atomRep.meshGO.transform.parent.gameObject;
			GameObject.Destroy(atomRep.meshGO.GetComponent<MeshFilter>().sharedMesh);
			GameObject.Destroy(atomRep.meshGO);
		}

		if (parent != null) {
			GameObject.Destroy(parent);
		}

		atomRep = null;
		isEnabled = false;
		isInit = false;
	}

	public override void ShowShadows(bool show) {
		if (atomRep.meshGO == null)
			return;

		if (show)
			atomRep.meshGO.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
		else
			atomRep.meshGO.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

	}

	public override void ShowHydrogens(bool show) {
		areHydrogensOn = show;
	}//Does not really make sense for tube

	public override void ShowSideChains(bool show) {
		areSideChainsOn = show;
	}//Does not really make sense for tube

	public override void ShowBackbone(bool show) {
		isBackboneOn = show;
	}//Does not really make sense for tube


	public override void SetColor(Color32 col, UnityMolSelection sele) {
		SetColors(col, sele.atoms);
	}

	public override void SetColor(Color32 col, UnityMolAtom a) {
		try {

			if (atomRep.atomToMeshVertex.ContainsKey(a)) {
				foreach (int vid in atomRep.atomToMeshVertex[a]) {
					atomRep.colors[vid] = col;
				}
				atomRep.curMesh.SetColors(atomRep.colors);
			}
		}
		catch {
			// Debug.LogError("Could not find atom " + a + " in this representation");
		}
	}
	public override void SetColors(Color32 col, List<UnityMolAtom> atoms) {
		try {
			foreach (UnityMolAtom a in atoms) {
				List<int> verts = null;
				if (atomRep.atomToMeshVertex.TryGetValue(a, out verts)) {
					foreach (int vid in verts) {
						atomRep.colors[vid] = col;
					}
				}
			}
			atomRep.curMesh.SetColors(atomRep.colors);

			if (rtos != null && rtos.Count > 0) {
				rtos[0].shouldUpdateMeshColor = true;
			}
		}
		catch (System.Exception e) {
			Debug.LogError("Failed to set the color of the line representation " + e);
		}
	}

	public override void SetColors(List<Color32> cols, List<UnityMolAtom> atoms) {
		if (atoms.Count != cols.Count) {
			Debug.LogError("Lengths of color list and atom list are different");
			return;
		}
		for (int i = 0; i < atoms.Count; i++) {
			UnityMolAtom a = atoms[i];
			Color32 col = cols[i];
			SetColor(col, a);
		}
	}

	public void SetWidth(float newWidth) {
		if (newWidth <= 0.0f || newWidth > 1.0f) {
			Debug.LogWarning("Line size should be between 0.0 and 1.0");
			return;
		}
		atomRep.lineWidth = newWidth;
		//Recompute the representation
		updateWithTrajectory();
	}

	public override void SetDepthCueingStart(float v) {
		if (atomRep.meshGO == null)
			return;

		Material[] mats = atomRep.meshGO.GetComponent<Renderer>().sharedMaterials;
		mats[0].SetFloat("_FogStart", v);

	}

	public override void SetDepthCueingDensity(float v) {
		if (atomRep.meshGO == null)
			return;

		Material[] mats = atomRep.meshGO.GetComponent<Renderer>().sharedMaterials;
		mats[0].SetFloat("_FogDensity", v);

	}

	public override void EnableDepthCueing() {
		if (atomRep.meshGO == null)
			return;

		Material[] mats = atomRep.meshGO.GetComponent<Renderer>().sharedMaterials;
		mats[0].SetFloat("_UseFog", 1.0f);

	}

	public override void DisableDepthCueing() {
		if (atomRep.meshGO == null)
			return;

		Material[] mats = atomRep.meshGO.GetComponent<Renderer>().sharedMaterials;
		mats[0].SetFloat("_UseFog", 0.0f);
	}

	public override void updateWithTrajectory() {
		//Do not need to clear everything, just recompute the vertex array
		float lineWidth = atomRep.lineWidth;
		int idVert = 0;
		if (atomRep.vertices == null || atomRep.vertices.Count == 0)
			return;

		foreach (int2 duo in atomRep.alphaTrace) {
			UnityMolAtom atom1 = atomRep.selection.atoms[duo.x];
			UnityMolAtom atom2 = atomRep.selection.atoms[duo.y];

			Vector3 start = atom1.position;
			Vector3 end   = atom2.position;

			if (atomRep.idFrame != -1) {
				start = atomRep.selection.extractTrajFramePositions[atomRep.idFrame][duo.x];
				end = atomRep.selection.extractTrajFramePositions[atomRep.idFrame][duo.y];
			}

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

			atomRep.vertices[ idVert + 0] = a1;
			atomRep.vertices[ idVert + 1] = a2;
			atomRep.vertices[ idVert + 2] = b1;
			atomRep.vertices[ idVert + 3] = b2;
			atomRep.vertices[ idVert + 4] = c1;
			atomRep.vertices[ idVert + 5] = c2;
			atomRep.vertices[ idVert + 6] = d1;
			atomRep.vertices[ idVert + 7] = d2;

			idVert += 8;
		}

		atomRep.curMesh.SetVertices(atomRep.vertices);

		if (rtos != null && rtos.Count > 0) {
			GameObject.Destroy(rtos[0]);
			RaytracingMaterial savedrtMat = rtos[0].rtMat;
			RaytracedObject rto = atomRep.meshGO.AddComponent<RaytracedObject>();
			savedrtMat.propertyChanged = true;
			rto.rtMat = savedrtMat;
			rtos = new List<RaytracedObject>() {rto};
		}

	}

	public override void updateWithModel() {
		bool wasEnabled = true;
		if (atomRep.meshGO != null )
			wasEnabled = atomRep.meshGO.GetComponent<Renderer>().enabled;

		atomRep.recompute();


		if (rtos != null && rtos.Count > 0) {
			GameObject.Destroy(rtos[0]);
			RaytracingMaterial savedrtMat = rtos[0].rtMat;
			RaytracedObject rto = atomRep.meshGO.AddComponent<RaytracedObject>();
			savedrtMat.propertyChanged = true;
			rto.rtMat = savedrtMat;
			rtos = new List<RaytracedObject>() {rto};
		}

		if (!wasEnabled) {
			atomRep.meshGO.GetComponent<Renderer>().enabled = false;
		}
	}

	public override void ShowAtom(UnityMolAtom atom, bool show) {
		Debug.LogWarning("Cannot show/hide one atom with the tube representation");
	}

	public override void SetSize(UnityMolAtom atom, float size) {
		Debug.LogWarning("Cannot set the size of one atom with the tube representation");
	}

	public override void SetSizes(List<UnityMolAtom> atoms, List<float> sizes) {
		Debug.LogWarning("Cannot set the size of atoms with the tube representation");
	}

	public override void SetSizes(List<UnityMolAtom> atoms, float size) {
		Debug.LogWarning("Cannot set the size of atoms with the tube representation");
	}
	public override void ResetSize(UnityMolAtom atom) {
		Debug.LogWarning("Cannot set the size of one atom with the tube representation");
	}
	public override void ResetSizes() {
		Debug.LogWarning("Cannot set the size of atoms with the tube representation");
	}

	public override void ResetColor(UnityMolAtom atom) {
		SetColor(atom.color32, atom);
	}
	public override void ResetColors() {

		if (atomRep.savedColors.Count == atomRep.colors.Count) {
			atomRep.curMesh.SetColors(atomRep.savedColors);
		}
		atomRep.colorationType = colorType.atom;
	}

	public override void HighlightRepresentation() {
	}

	public override void DeHighlightRepresentation() {
	}
	public void HighlightPart(List<UnityMolAtom> atoms) {
		Vector2[] uvs2 = atomRep.curMesh.uv2;
		if (uvs2 == null || uvs2.Length == 0) {
			uvs2 = new Vector2[atomRep.curMesh.vertexCount];
			for (int i = 0; i < uvs2.Length; i++)
				uvs2[i] = -Vector2.one;
		}

		foreach (UnityMolAtom a in atoms) {
			if (atomRep.atomToMeshVertex.ContainsKey(a)) {
				foreach (int vid in atomRep.atomToMeshVertex[a]) {
					uvs2[vid] = Vector2.one;
				}
			}
		}
		atomRep.curMesh.uv2 = uvs2;
	}

	public void DehighlightPart(List<UnityMolAtom> atoms) {
		Vector2[] uvs2 = atomRep.curMesh.uv2;

		if (uvs2 == null || uvs2.Length == 0) {
			uvs2 = new Vector2[atomRep.curMesh.vertexCount];
			for (int i = 0; i < uvs2.Length; i++)
				uvs2[i] = -Vector2.one;
		}
		foreach (UnityMolAtom a in atoms) {
			if (atomRep.atomToMeshVertex.ContainsKey(a)) {
				foreach (int vid in atomRep.atomToMeshVertex[a]) {
					uvs2[vid] = -Vector2.one;
				}
			}
		}
		atomRep.curMesh.uv2 = uvs2;
	}


	public override void SetSmoothness(float val) {

		Material[] mats = atomRep.meshGO.GetComponent<Renderer>().sharedMaterials;
		mats[0].SetFloat("_Glossiness", val);
	}
	public override void SetMetal(float val) {

		Material[] mats = atomRep.meshGO.GetComponent<Renderer>().sharedMaterials;
		mats[0].SetFloat("_Metallic", val);
	}
	public override void UpdateLike() {
	}

	public override UnityMolRepresentationParameters Save() {
		UnityMolRepresentationParameters res = new UnityMolRepresentationParameters();
		MeshFilter mf = atomRep.meshGO.GetComponent<MeshFilter>();

		res.repT.atomType = AtomType.trace;
		res.colorationType = atomRep.colorationType;

		if (res.colorationType == colorType.custom) {
			int atomNum = 0;
			res.colorPerAtom = new Dictionary<UnityMolAtom, Color32>(atomRep.selection.atoms.Count);
			foreach (UnityMolAtom a in atomRep.selection.atoms) {
				if (atomRep.atomToMeshVertex.ContainsKey(a)) {
					int idVert = atomRep.atomToMeshVertex[a][0];
					res.colorPerAtom[a] = atomRep.colors[idVert];
				}
				atomNum++;
			}
		}
		else if (res.colorationType == colorType.full) { //Get color of first atom/residue
			res.fullColor = atomRep.colors[0];

		}
		else if (res.colorationType == colorType.bfactor) {
			res.bfactorStartColor = atomRep.bfactorStartCol;
			res.bfactorMidColor = atomRep.bfactorMidColor;
			res.bfactorEndColor = atomRep.bfactorEndCol;
		}

		if (atomRep.meshGO != null ) {
			res.smoothness = atomRep.meshGO.GetComponent<Renderer>().sharedMaterials[0].GetFloat("_Glossiness");
			res.metal = atomRep.meshGO.GetComponent<Renderer>().sharedMaterials[0].GetFloat("_Metallic");
			res.shadow = (atomRep.meshGO.GetComponent<Renderer>().shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.On);
		}

		res.TubeWidth = atomRep.lineWidth;


		return res;
	}
	public override void Restore(UnityMolRepresentationParameters savedParams) {

		if (savedParams.repT.atomType == AtomType.trace) {
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
			else if (savedParams.colorationType == colorType.atom) {
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
			else if (savedParams.colorationType == colorType.resid) {
				colorByResid(atomRep.selection);
			}
			else if (savedParams.colorationType == colorType.resnum) {
				colorByResnum(atomRep.selection);
			}
			else if (savedParams.colorationType == colorType.bfactor) {
				colorByBfactor(atomRep.selection, savedParams.bfactorStartColor, savedParams.bfactorMidColor, savedParams.bfactorEndColor);
			}



			SetMetal(savedParams.metal);
			SetSmoothness(savedParams.smoothness);
			ShowShadows(savedParams.shadow);
			SetWidth(savedParams.TubeWidth);

			atomRep.colorationType = savedParams.colorationType;
		}
		else {
			Debug.LogError("Could not restore representation parameters");
		}
	}

}
}