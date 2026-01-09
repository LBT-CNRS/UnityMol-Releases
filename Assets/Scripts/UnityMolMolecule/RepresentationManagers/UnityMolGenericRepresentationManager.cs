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

namespace UMol {

public abstract class UnityMolGenericRepresentationManager {

	/// <summary>
	/// State of the renderer
	/// </summary>
	public bool isEnabled = false;

	/// <summary>
	/// State of the manager
	/// </summary>
	public bool isInit = false;

	/// <summary>
	/// Are hydrogens shown
	/// </summary>
	public bool areHydrogensOn = true;

	/// <summary>
	/// Are side chains shown
	/// </summary>
	public bool areSideChainsOn = true;

	/// <summary>
	/// Is backbone shown
	/// </summary>
	public bool isBackboneOn = true;

	/// <summary>
	/// List of RaytracedObject script
	/// </summary>
	public List<RaytracedObject> rtos;


	/// <summary>
	/// Disables the renderers for all objects managed by the instance of the manager.
	/// </summary>
	public abstract void DisableRenderers();

	/// <summary>
	/// Enables the renderers for all objects managed by the instance of the manager.
	/// </summary>
	public abstract void EnableRenderers();

	/// <summary>
	/// Initializes this instance of the manager.
	/// </summary>
	public abstract void Init(SubRepresentation rep);

	public abstract void InitRT();

	public abstract void Clean();

	public abstract void ShowShadows(bool show);

	public abstract void ShowHydrogens(bool show);

	public abstract void ShowSideChains(bool show);

	public abstract void ShowBackbone(bool show);

	public abstract void SetColor(Color32 col, UnityMolSelection sele);

	public abstract void SetColors(Color32 col, List<UnityMolAtom> atoms);

	public abstract void SetColors(List<Color32> cols, List<UnityMolAtom> atoms);

	public abstract void SetColor(Color32 col, UnityMolAtom atom);

	public abstract void SetDepthCueingStart(float v);

	public abstract void SetDepthCueingDensity(float v);

	public abstract void EnableDepthCueing();

	public abstract void DisableDepthCueing();

	public abstract void updateWithTrajectory();

	public abstract void updateWithModel();

	public abstract void ShowAtom(UnityMolAtom atom, bool show);

	public abstract void SetSize(UnityMolAtom atom, float size);

	public abstract void SetSizes(List<UnityMolAtom> atoms, List<float> sizes);

	public abstract void SetSizes(List<UnityMolAtom> atoms, float size);

	public abstract void ResetSize(UnityMolAtom atom);

	public abstract void ResetSizes();

	public abstract void ResetColor(UnityMolAtom atom);

	public abstract void ResetColors();

	public abstract void HighlightRepresentation();

	public abstract void DeHighlightRepresentation();

	// public abstract void HighlightPart(List<UnityMolAtom> atoms);

	// public abstract void DehighlightPart(List<UnityMolAtom> atoms);

	public abstract void SetSmoothness(float val);

	public abstract void SetMetal(float val);

	public abstract void UpdateLike();



	public void SetRTMaterialType(int t) {
        if(rtos == null || rtos.Count == 0){
			return;
		}
        foreach(RaytracedObject rto in rtos){
			rto.changeMaterialType(t);
        }
	}
	public int GetRTMaterialType(){
        if(rtos == null || rtos.Count == 0){
			return -1;
		}
        foreach(RaytracedObject rto in rtos){
			return rto.RTMatToType();
		}
		return -1;
	}
	public void SetRTMaterialProperty(string n, object val) {
        if(rtos == null || rtos.Count == 0)
            return;
        foreach(RaytracedObject rto in rtos){
            rto.rtMat.setRTMatProperty(n, val);
        }
	}

	public void SetRTMaterial(RaytracingMaterial material){
        foreach(RaytracedObject rto in rtos){
			rto.rtMat = material;
		}
	}

	public abstract UnityMolRepresentationParameters Save();

	public abstract void Restore(UnityMolRepresentationParameters savedParams);

	public void colorByAtom(UnityMolSelection sel) {
		List<Color32> colors = new List<Color32>(sel.atoms.Count);

		foreach (UnityMolAtom a in sel.atoms) {
			colors.Add(a.color32);
		}
		SetColors(colors, sel.atoms);
	}

	public void colorByChain(UnityMolSelection sel) {
		//Copied from UnityMolRepresentation
		//TODO: reorganize code to avoid duplicates
		Dictionary<UnityMolChain, Color32> chainCol = new Dictionary<UnityMolChain, Color32>();
		int cptColor = 0;
		foreach (UnityMolAtom a in sel.atoms) {
			if (!chainCol.ContainsKey(a.residue.chain)) {
				chainCol[a.residue.chain] = UnityMolMain.atomColors.getColorFromPalette(cptColor);
				cptColor++;
			}
			// SetColor(a, chainCol[a.residue.chain]);
		}

		foreach (UnityMolChain c in chainCol.Keys) {
			SetColor(chainCol[c], c.ToSelection());
		}
	}
	public void colorByRes(UnityMolSelection sel) {
		//Copied from UnityMolRepresentation
		//TODO: reorganize code to avoid duplicates
		List<Color32> colors = new List<Color32>(sel.atoms.Count);

		foreach (UnityMolAtom a in sel.atoms) {
			colors.Add(UnityMolMain.atomColors.getColorForResidue(a.residue));
		}
		SetColors(colors, sel.atoms);
	}

	public void colorByHydro(UnityMolSelection sel) {
		//Copied from UnityMolRepresentation
		//TODO: reorganize code to avoid duplicates
		List<Color32> colors = new List<Color32>(sel.atoms.Count);

		foreach (UnityMolAtom a in sel.atoms) {
			float hydro = a.residue.kdHydro;
			Color32 col = UnityMolRepresentation.hydroToColor(hydro, -4.5f, 4.5f);
			colors.Add(col);
		}
		SetColors(colors, sel.atoms);
	}
	public void colorBySequence(UnityMolSelection sel) {
		//Copied from UnityMolRepresentation
		//TODO: reorganize code to avoid duplicates
		Dictionary<UnityMolResidue, int> residuesOfS = new Dictionary<UnityMolResidue, int>();
		Dictionary<string, int> nbResPerS = new Dictionary<string, int>();
		foreach (UnityMolStructure s in sel.structures) {
			if (!nbResPerS.ContainsKey(s.name)) {
				nbResPerS[s.name] = 0;
			}
		}
		foreach (UnityMolAtom a in sel.atoms) {
			if (!residuesOfS.ContainsKey(a.residue)) {
				residuesOfS[a.residue] = nbResPerS[a.residue.chain.model.structure.name];
				nbResPerS[a.residue.chain.model.structure.name]++;
			}
		}


		Dictionary<UnityMolResidue, Color32> resCol =
		    new Dictionary<UnityMolResidue, Color32>(new LightResidueComparer());

		List<Color32> colors = new List<Color32>(sel.atoms.Count);

		foreach (UnityMolAtom a in sel.atoms) {
			if (!resCol.ContainsKey(a.residue)) {
				UnityMolStructure s = a.residue.chain.model.structure;
				int nbRes = nbResPerS[s.name];
				int idResPerS = residuesOfS[a.residue];

				resCol[a.residue] = UnityMolRepresentation.rainbowColor(idResPerS / (float) nbRes);
			}
			colors.Add(resCol[a.residue]);
		}
		SetColors(colors, sel.atoms);
	}

	public void colorByCharge(UnityMolSelection sel) {
		//Copied from UnityMolRepresentation
		//TODO: reorganize code to avoid duplicates
		List<Color32> colors = new List<Color32>(sel.atoms.Count);

		foreach (UnityMolAtom a in sel.atoms) {
			colors.Add(UnityMolMain.atomColors.getColorReschargeForResidue(a.residue));
		}
		SetColors(colors, sel.atoms);
	}
	public void colorByResType(UnityMolSelection sel) {
		//Copied from UnityMolRepresentation
		//TODO: reorganize code to avoid duplicates
		List<Color32> colors = new List<Color32>(sel.atoms.Count);

		foreach (UnityMolAtom a in sel.atoms) {
			colors.Add(UnityMolMain.atomColors.getColorRestypeForResidue(a.residue));
		}
		SetColors(colors, sel.atoms);
	}
	public void colorByResCharge(UnityMolSelection sel) {
		//Copied from UnityMolRepresentation
		//TODO: reorganize code to avoid duplicates
		List<Color32> colors = new List<Color32>(sel.atoms.Count);

		foreach (UnityMolAtom a in sel.atoms) {
			colors.Add(UnityMolMain.atomColors.getColorReschargeForResidue(a.residue));
		}
		SetColors(colors, sel.atoms);
	}


	public void colorByResid(UnityMolSelection sel) {
		//Copied from UnityMolRepresentation
		//TODO: reorganize code to avoid duplicates

		List<Color32> colors = new List<Color32>(sel.atoms.Count);

		foreach (UnityMolAtom a in sel.atoms) {
			colors.Add(UnityMolMain.atomColors.getColorFromPalette(a.residue.id));
		}
		SetColors(colors, sel.atoms);
	}

	public void colorByResnum(UnityMolSelection sel) {
		//Copied from UnityMolRepresentation
		//TODO: reorganize code to avoid duplicates
		List<Color32> colors = new List<Color32>(sel.atoms.Count);

		foreach (UnityMolAtom a in sel.atoms) {
			colors.Add(UnityMolMain.atomColors.getColorFromPalette(a.residue.resnum));
		}

		SetColors(colors, sel.atoms);
	}

	public void colorByBfactor(UnityMolSelection sel, Color startCol, Color midCol, Color endCol) {
		//Copied from UnityMolRepresentation
		//TODO: reorganize code to avoid duplicates
		List<Color32> colors = new List<Color32>(sel.atoms.Count);

		bool containsCA = false;
		foreach (UnityMolAtom a in sel.atoms) {
			if (a.name == "CA" && a.type == "C") {
				containsCA = true;
			}
		}

		float minBfac = sel.atoms[0].bfactor;
		float maxBfac = sel.atoms[0].bfactor;
		foreach (UnityMolAtom a in sel.atoms) {
			if (containsCA) { //Uses only CA for proteins
				if (a.type == "C" && a.name == "CA") {
					minBfac = Mathf.Min(a.bfactor, minBfac);
					maxBfac = Mathf.Max(a.bfactor, maxBfac);
				}
			}
			else {
				minBfac = Mathf.Min(a.bfactor, minBfac);
				maxBfac = Mathf.Max(a.bfactor, maxBfac);
			}
		}

		if (!containsCA) {
			foreach (UnityMolAtom a in sel.atoms) {
				colors.Add(UnityMolRepresentation.bfactorToColorNorm(a.bfactor, startCol, midCol, endCol, minBfac, maxBfac));
			}
		}
		else {
			foreach (UnityMolAtom a in sel.atoms) {
				if (a.residue.atoms.ContainsKey("CA")) {
					float bf = a.residue.atoms["CA"].bfactor;
					colors.Add(UnityMolRepresentation.bfactorToColorNorm(bf, startCol, midCol, endCol, minBfac, maxBfac));
				}
				else
					colors.Add(UnityMolRepresentation.bfactorToColorNorm(a.bfactor, startCol, midCol, endCol, minBfac, maxBfac));
			}
		}
		SetColors(colors, sel.atoms);
	}

}
}