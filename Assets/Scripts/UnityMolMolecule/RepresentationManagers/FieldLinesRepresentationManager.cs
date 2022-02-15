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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UMol {
public class FieldLinesRepresentationManager: UnityMolGenericRepresentationManager {

	public FieldLinesRepresentation atomRep;


	/// <summary>
	/// Initializes this instance of the manager.
	/// </summary>
	public override void Init(SubRepresentation rep) {
		if (isInit) {
			return;
		}

		atomRep = (FieldLinesRepresentation) rep.atomRep;
		isInit = true;
		isEnabled = true;
		areSideChainsOn = true;
		areHydrogensOn = true;
		isBackboneOn = true;
	}

	public override void Clean() {
		if (atomRep.goFL != null) {
			GameObject.Destroy(atomRep.goFL.transform.parent.gameObject);
		}
		// for (int i = 0; i < atomRep.allLines.Count; i++) {
		// GameObject.Destroy(atomRep.allLines[i]);
		// }
		// atomRep.allLines.Clear();
		// atomRep.allLines = null;

		isInit = false;
		isEnabled = false;
		areSideChainsOn = false;
		areHydrogensOn = false;
		isBackboneOn = false;
	}

	/// <summary>
	/// Disables the renderers for all objects managed by the instance of the manager.
	/// </summary>
	public override void DisableRenderers() {
		// foreach (GameObject l in atomRep.allLines) {
		// l.SetActive(false);
		// }
		if (atomRep.goFL != null) {
			atomRep.goFL.SetActive(false);
		}
		isEnabled = false;
	}

	/// <summary>
	/// Enables the renderers for all objects managed by the instance of the manager.
	/// </summary>
	public override void EnableRenderers() {
		// foreach (GameObject l in atomRep.allLines) {
		// l.SetActive (true);
		// }
		if (atomRep.goFL != null) {
			atomRep.goFL.SetActive(true);
		}
		isEnabled = true;

	}

	public void SetStartColor(Color c1) {

		if (atomRep.goFL != null) {
			Color32 c1_32 = c1;
			Color32 c2_32 = atomRep.endColor;
			atomRep.startColor = c1;

			Mesh m = atomRep.goFL.GetComponent<MeshFilter>().sharedMesh;
			Color32[] cols = m.colors32;
			foreach (Int2 mid in atomRep.meshVertIds) {
				int start = mid.x;
				int len = mid.y;
				int cpt = 0;
				for (int i = start; i < start + len; i++) {
					cols[i] = Color32.Lerp(c1_32, c2_32, cpt / (float) len);
					cpt++;
				}

			}

			m.colors32 = cols;
		}
	}

	public void SetEndColor(Color c2) {

		if (atomRep.goFL != null) {
			Color32 c1_32 = atomRep.startColor;
			Color32 c2_32 = c2;
			atomRep.endColor = c2;

			Mesh m = atomRep.goFL.GetComponent<MeshFilter>().sharedMesh;
			Color32[] cols = m.colors32;
			foreach (Int2 mid in atomRep.meshVertIds) {
				int start = mid.x;
				int len = mid.y;
				int cpt = 0;
				for (int i = start; i < start + len; i++) {
					cols[i] = Color32.Lerp(c1_32, c2_32, cpt / (float) len);
					cpt++;
				}

			}

			m.colors32 = cols;
		}
	}
	public void SetStartEndColors(Color c1, Color c2) {
		Color32 c1_32 = c1;
		Color32 c2_32 = c2;

		atomRep.startColor = c1;
		atomRep.endColor = c2;

		if (atomRep.goFL != null) {
			Mesh m = atomRep.goFL.GetComponent<MeshFilter>().sharedMesh;
			Color32[] cols = m.colors32;
			foreach (Int2 mid in atomRep.meshVertIds) {
				int start = mid.x;
				int len = mid.y;
				int cpt = 0;
				for (int i = start; i < start + len; i++) {
					cols[i] = Color32.Lerp(c1_32, c2_32, cpt / (float) len);
					cpt++;
				}

			}

			m.colors32 = cols;
		}

	}

	public void SetSizes(float width) {
		if (atomRep.goFL != null) {
			atomRep.changeFLSize(width);
		}
	}
	public void SetLengthLine(float newL) {
		if (atomRep.goFL != null) {
			atomRep.lengthLine = newL;
			Material mat = atomRep.goFL.GetComponent<MeshRenderer>().sharedMaterial;
			mat.SetFloat("size", newL);
		}
	}
	public void SetSpeedLine(float newS) {
		if (atomRep.goFL != null) {
			atomRep.speedLine = newS;
			Material mat = atomRep.goFL.GetComponent<MeshRenderer>().sharedMaterial;
			mat.SetFloat("speed", newS);
		}
	}

	public void recomputeWithMagnitudeValue(float newMag) {

	}

	public override void ResetSizes() {
		SetSizes(0.05f);
	}

	public override void ResetColors() {
		if (atomRep.goFL != null) {
			Mesh m = atomRep.goFL.GetComponent<MeshFilter>().sharedMesh;
			Color32 white = new Color32(255, 255, 255, 255);
			Color32[] cols = m.colors32;
			for (int i = 0; i < cols.Length; i++) {
				cols[i] = white;
			}
			m.colors32 = cols;
		}
	}



	public override void ShowShadows(bool show) {
		Debug.LogWarning("Cannot change this parameter for the FieldLines representation");
	}

	public override void ShowHydrogens(bool show) {
		Debug.LogWarning("Cannot change this parameter for the FieldLines representation");
	}

	public override void SetColor(Color col, UnityMolSelection sele) {
		SetStartEndColors(col, col);
	}

	public override void SetColors(Color col, List<UnityMolAtom> atoms) {
		SetStartEndColors(col, col);
	}

	public override void SetColors(List<Color> cols, List<UnityMolAtom> atoms) {
		Debug.LogWarning("Cannot change this parameter for the FieldLines representation");
	}

	public override void SetColor(Color col, UnityMolAtom atom) {
		Debug.LogWarning("Cannot change this parameter for the FieldLines representation");
	}

	public override void SetDepthCueingStart(float v) {
		if (atomRep.goFL != null) {
			Material mat = atomRep.goFL.GetComponent<MeshRenderer>().sharedMaterial;
			mat.SetFloat("_FogStart", v);
		}
	}

	public override void SetDepthCueingDensity(float v) {
		if (atomRep.goFL != null) {
			Material mat = atomRep.goFL.GetComponent<MeshRenderer>().sharedMaterial;
			mat.SetFloat("_FogDensity", v);
		}
	}

	public override void EnableDepthCueing() {
		if (atomRep.goFL != null) {
			Material mat = atomRep.goFL.GetComponent<MeshRenderer>().sharedMaterial;
			mat.SetFloat("_UseFog", 1.0f);
		}

	}

	public override void DisableDepthCueing() {
		if (atomRep.goFL != null) {
			Material mat = atomRep.goFL.GetComponent<MeshRenderer>().sharedMaterial;
			mat.SetFloat("_UseFog", 0.0f);
		}
	}


	public override void updateWithTrajectory() {
		Debug.LogWarning("Cannot change this parameter for the FieldLines representation");
	}

	public override void updateWithModel() {
		Debug.LogWarning("Cannot change this parameter for the FieldLines representation");
	}

	public override void ShowAtom(UnityMolAtom atom, bool show) {
		Debug.LogWarning("Cannot change this parameter for the FieldLines representation");
	}

	public override void SetSize(UnityMolAtom atom, float size) {
		Debug.LogWarning("Cannot change this parameter for the FieldLines representation");
	}

	public override void SetSizes(List<UnityMolAtom> atoms, List<float> sizes) {
		Debug.LogWarning("Cannot change this parameter for the FieldLines representation");
	}

	public override void SetSizes(List<UnityMolAtom> atoms, float size) {
		SetSizes(size);
	}

	public override void ResetSize(UnityMolAtom atom) {
		Debug.LogWarning("Cannot change this parameter for the FieldLines representation");
	}

	public override void ResetColor(UnityMolAtom atom) {
		Debug.LogWarning("Cannot change this parameter for the FieldLines representation");
	}

	public override void ShowSideChains(bool show) {
		Debug.LogWarning("Cannot change this parameter for the FieldLines representation");
	}

	public override void ShowBackbone(bool show) {
		Debug.LogWarning("Cannot change this parameter for the FieldLines representation");
	}

	public override void HighlightRepresentation() {
	}

	public override void DeHighlightRepresentation() {
	}
	public override void SetSmoothness(float val) {
		Debug.LogWarning("Cannot change this parameter for the FieldLines representation");
	}

	public override void SetMetal(float val) {
		Debug.LogWarning("Cannot change this parameter for the FieldLines representation");
	}

	public override UnityMolRepresentationParameters Save(){
		UnityMolRepresentationParameters res = new UnityMolRepresentationParameters();
		return res;
	}
    public override void Restore(UnityMolRepresentationParameters savedParams){
        
    }
}
}