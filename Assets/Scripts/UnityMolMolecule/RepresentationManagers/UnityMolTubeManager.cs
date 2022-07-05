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
		atomRep.meshGO = atomRep.meshGO;

		isInit = true;
		isEnabled = true;
		areSideChainsOn = true;
		areHydrogensOn = true;
		isBackboneOn = true;
	}

	public override void Clean() {
		GameObject parent = null;
		if (atomRep.meshGO != null) {
			parent = atomRep.meshGO.transform.parent.gameObject;
		}

		GameObject.DestroyImmediate(atomRep.meshGO);

		if (parent != null) {
			GameObject.DestroyImmediate(parent);
		}

		atomRep.meshGO = null;
		atomRep = null;
		isEnabled = false;
		isInit = false;
		// UnityMolMain.getRepresentationManager().UpdateActiveColliders();
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


	public override void SetColor(Color col, UnityMolSelection sele) {
		SetColors(col, sele.atoms);
	}

	public override void SetColor(Color col, UnityMolAtom a) {
		try {
			Color32 newCol = col;

			if (atomRep.atomToMeshVertex.ContainsKey(a)) {
				foreach (int vid in atomRep.atomToMeshVertex[a]) {
					atomRep.colors[vid] = newCol;
				}
				atomRep.curMesh.SetColors(atomRep.colors);
			}
		}
		catch {
			Debug.LogError("Could not find atom " + a + " in this representation");
		}
	}
	public override void SetColors(Color col, List<UnityMolAtom> atoms) {
		try {
			Color32 newCol = col;
			foreach (UnityMolAtom a in atoms) {
				List<int> verts = null;
				if (atomRep.atomToMeshVertex.TryGetValue(a, out verts)) {
					foreach (int vid in verts) {
						atomRep.colors[vid] = newCol;
					}
				}
			}
			atomRep.curMesh.SetColors(atomRep.colors);
		}
		catch (System.Exception e) {
			Debug.LogError("Failed to set the color of the line representation " + e);
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

	public void SetWidth(float newWidth) {
		if (newWidth <= 0.0f || newWidth > 1.0f) {
			Debug.LogWarning("Line size should be between 0.0 and 1.0");
			return;
		}
		atomRep.lineWidth = newWidth;
		//Recompute the line representation
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

		foreach (Int2 duo in atomRep.alphaTrace) {
			UnityMolAtom atom1 = atomRep.selection.atoms[duo.x];
			UnityMolAtom atom2 = atomRep.selection.atoms[duo.y];

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
	}

	public override void updateWithModel() {
		bool wasEnabled = true;
		if (atomRep.meshGO != null )
			wasEnabled = atomRep.meshGO.GetComponent<Renderer>().enabled;

		atomRep.recompute();

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
		SetColor(atom.color, atom);
	}
	public override void ResetColors() {

		if (atomRep.savedColors.Count == atomRep.colors.Count) {
			atomRep.curMesh.SetColors(atomRep.savedColors);
		}
	}

	public override void HighlightRepresentation() {

	}

	public override void DeHighlightRepresentation() {

	}
	public override void SetSmoothness(float val) {

		Material[] mats = atomRep.meshGO.GetComponent<Renderer>().sharedMaterials;
		mats[0].SetFloat("_Glossiness", val);
	}
	public override void SetMetal(float val) {

		Material[] mats = atomRep.meshGO.GetComponent<Renderer>().sharedMaterials;
		mats[0].SetFloat("_Metallic", val);
	}
	public override UnityMolRepresentationParameters Save(){
		UnityMolRepresentationParameters res = new UnityMolRepresentationParameters();
		return res;
	}
    public override void Restore(UnityMolRepresentationParameters savedParams){
        
    }
}
}