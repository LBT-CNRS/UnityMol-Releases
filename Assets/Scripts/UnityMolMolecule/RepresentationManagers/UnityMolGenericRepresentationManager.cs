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

	public abstract void Clean();

	public abstract void ShowShadows(bool show);

	public abstract void ShowHydrogens(bool show);

	public abstract void ShowSideChains(bool show);

	public abstract void ShowBackbone(bool show);

	public abstract void SetColor(Color col, UnityMolSelection sele);

	public abstract void SetColors(Color col, List<UnityMolAtom> atoms);

	public abstract void SetColors(List<Color> cols, List<UnityMolAtom> atoms);

	public abstract void SetColor(Color col, UnityMolAtom atom);

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

	public abstract void SetSmoothness(float val);

	public abstract void SetMetal(float val);

	public abstract UnityMolRepresentationParameters Save();

	public abstract void Restore(UnityMolRepresentationParameters savedParams);

	public void colorByChain(UnityMolSelection sel) {
		//Copied from UnityMolRepresentation
		//TODO: reorganize code to avoid duplicates
		Dictionary<UnityMolChain, Color> chainCol = new Dictionary<UnityMolChain, Color>();
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
		List<Color> colors = new List<Color>(sel.Count);

		foreach (UnityMolAtom a in sel.atoms) {
			colors.Add(UnityMolMain.atomColors.getColorForResidue(a.residue));
		}
		SetColors(colors, sel.atoms);
	}

	public void colorByHydro(UnityMolSelection sel) {
		//Copied from UnityMolRepresentation
		//TODO: reorganize code to avoid duplicates
		List<Color> colors = new List<Color>(sel.Count);

		foreach (UnityMolAtom a in sel.atoms) {
			float hydro = a.residue.kdHydro;
			Color col = UnityMolRepresentation.hydroToColor(hydro, -4.5f, 4.5f);
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
			if (!nbResPerS.ContainsKey(s.uniqueName)) {
				nbResPerS[s.uniqueName] = 0;
			}
		}
		foreach (UnityMolAtom a in sel.atoms) {
			if (!residuesOfS.ContainsKey(a.residue)) {
				residuesOfS[a.residue] = nbResPerS[a.residue.chain.model.structure.uniqueName];
				nbResPerS[a.residue.chain.model.structure.uniqueName]++;
			}
		}


		Dictionary<UnityMolResidue, Color> resCol =
		    new Dictionary<UnityMolResidue, Color>(new LightResidueComparer());

		List<Color> colors = new List<Color>(sel.Count);

		foreach (UnityMolAtom a in sel.atoms) {
			if (!resCol.ContainsKey(a.residue)) {
				UnityMolStructure s = a.residue.chain.model.structure;
				int nbRes = nbResPerS[s.uniqueName];
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
		List<Color> colors = new List<Color>(sel.Count);

		foreach (UnityMolAtom a in sel.atoms) {
			colors.Add(UnityMolMain.atomColors.getColorReschargeForResidue(a.residue));
		}
		SetColors(colors, sel.atoms);
	}
	public void colorByResType(UnityMolSelection sel) {
		//Copied from UnityMolRepresentation
		//TODO: reorganize code to avoid duplicates
		List<Color> colors = new List<Color>(sel.Count);

		foreach (UnityMolAtom a in sel.atoms) {
			colors.Add(UnityMolMain.atomColors.getColorRestypeForResidue(a.residue));
		}
		SetColors(colors, sel.atoms);
	}
	public void colorByResCharge(UnityMolSelection sel) {
		//Copied from UnityMolRepresentation
		//TODO: reorganize code to avoid duplicates
		List<Color> colors = new List<Color>(sel.Count);

		foreach (UnityMolAtom a in sel.atoms) {
			colors.Add(UnityMolMain.atomColors.getColorReschargeForResidue(a.residue));
		}
		SetColors(colors, sel.atoms);
	}
	public void colorByBfactor(UnityMolSelection sel, Color startCol, Color endCol) {
		//Copied from UnityMolRepresentation
		//TODO: reorganize code to avoid duplicates
		List<Color> colors = new List<Color>(sel.Count);


		float minBfac = sel.atoms[0].bfactor;
		float maxBfac = sel.atoms[0].bfactor;
		foreach (UnityMolAtom a in sel.atoms) {
			minBfac = Mathf.Min(a.bfactor, minBfac);
			maxBfac = Mathf.Max(a.bfactor, maxBfac);
		}

		foreach (UnityMolAtom a in sel.atoms) {
			colors.Add(UnityMolRepresentation.bfactorToColorNorm(a.bfactor, startCol, endCol, minBfac, maxBfac));
		}
		SetColors(colors, sel.atoms);
	}



}
}