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
using System.Linq;

namespace UMol {
public class UnityMolRepresentation {

	public RepType repType;

	public UnityMolSelection selection;

	public List<SubRepresentation> subReps;

	private object[] savedArgs;

	public bool isEnabled = false;

	public int nbAtomsInRep = 0;
	public int nbBondsInRep = 0;

	public UnityMolRepresentation(AtomType atomR, BondType bondR, UnityMolSelection sel, params object[] listArgs) {

		if (sel == null) {
			Debug.LogError("Cannot create a representation for null selection");
			return;
		}

		subReps = new List<SubRepresentation>();

		repType.atomType = atomR;
		repType.bondType = bondR;

		savedArgs = listArgs;
		selection = sel;

		Dictionary<UnityMolStructure, UnityMolSelection> byStruc = cutSelectionByStructure(sel);

		foreach (UnityMolStructure s in byStruc.Keys) {
			SubRepresentation sr;
			sr.subSelection = byStruc[s];

			sr.atomRep = null;
			sr.bondRep = null;
			sr.atomRepManager = null;
			sr.bondRepManager = null;
			sr.representationParent = null;
			sr.atomRepresentationTransform = null;
			sr.bondRepresentationTransform = null;


			ShowAtoms(ref sr, listArgs);
			ShowBonds(ref sr, listArgs);

			if (sr.atomRep != null) {
				sr.representationParent = sr.atomRep.representationParent;
				sr.atomRepresentationTransform = sr.atomRep.representationTransform;
				nbAtomsInRep += sr.atomRep.nbAtoms;
			}
			if (sr.bondRep != null) {
				sr.bondRepresentationTransform = sr.bondRep.representationTransform;
				if (sr.representationParent == null) {
					sr.representationParent = sr.bondRep.representationParent;
				}
				nbBondsInRep += sr.bondRep.nbBonds;
			}


			subReps.Add(sr);
		}

		isEnabled = true;

		UnityMolMain.getRepresentationManager().UpdateActiveRepresentations();

	}

	public static Dictionary<UnityMolStructure, UnityMolSelection> cutSelectionByStructure(UnityMolSelection sel) {
		if (sel.structures.Count == 1) {
			Dictionary<UnityMolStructure, UnityMolSelection> singleSToSel = new Dictionary<UnityMolStructure, UnityMolSelection>();
			singleSToSel[sel.structures[0]] = sel;
			return singleSToSel;

		}
		Dictionary<UnityMolStructure, UnityMolSelection> selByStructure = new Dictionary<UnityMolStructure, UnityMolSelection>();
		HashSet<UnityMolStructure> allStructures = new HashSet<UnityMolStructure>();

		for (int i = 0; i < sel.atoms.Count; i++) {
			allStructures.Add(sel.atoms[i].residue.chain.model.structure);
		}
		if (allStructures.Count == 1) {
			sel.fillStructures();
			selByStructure[allStructures.First()] = sel;
			return selByStructure;
		}

		for (int i = 0; i < sel.atoms.Count; i++) {
			UnityMolStructure structure = sel.atoms[i].residue.chain.model.structure;
			if (!selByStructure.ContainsKey(structure)) {
				selByStructure[structure] = new UnityMolSelection(new List<UnityMolAtom>(), sel.name + "_" + structure.ToSelectionName());
			}
			selByStructure[structure].atoms.Add(sel.atoms[i]);
		}
		foreach (UnityMolStructure s in selByStructure.Keys) {
			selByStructure[s].structures.Add(s);
		}

		return selByStructure;
	}


	public void Clean() {

		for (int i = 0; i < subReps.Count ; i++) {
			SubRepresentation sr = subReps[i];

			if (sr.atomRepManager != null) {
				sr.atomRepManager.Clean();
			}
			if (sr.bondRepManager != null) {
				sr.bondRepManager.Clean();
			}
			if (sr.atomRep != null) {
				sr.atomRep.Clean();
			}
			if (sr.bondRep != null) {
				sr.bondRep.Clean();
			}
			sr.atomRep = null;
			sr.bondRep = null;
			sr.representationParent = null;
			sr.atomRepresentationTransform = null;
			sr.bondRepresentationTransform = null;
			sr.atomRepManager = null;
			sr.bondRepManager = null;
		}
		subReps.Clear();

		selection = null;
		savedArgs = null;
		isEnabled = false;
		nbAtomsInRep = 0;
		nbBondsInRep = 0;

	}
	public bool isActive() {
		return isEnabled;
	}

	private void ShowAtoms(ref SubRepresentation sr, params object[] listArgs) {

		sr.atomRep = null;
		switch (repType.atomType) {
		case AtomType.cartoon:
			sr.atomRep = new CartoonRepresentation(sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			sr.atomRepManager = new UnityMolCartoonManager();
			sr.atomRepManager.Init(sr);
			break;
		case AtomType.surface:
			if (listArgs.Length == 4) {
				bool cutByChain = (bool)listArgs[0];
				bool AO = (bool)listArgs[1];
				bool cutSurface = (bool)listArgs[2];
				SurfMethod method = (SurfMethod) listArgs[3];
				sr.atomRep = new SurfaceRepresentation(sr.subSelection.structures[0].ToSelectionName(), sr.subSelection, cutByChain, AO, cutSurface, method);
			}
			else if (listArgs.Length == 1) {
				SurfMethod meth = (SurfMethod) listArgs[0];
				sr.atomRep = new SurfaceRepresentation(sr.subSelection.structures[0].ToSelectionName(), sr.subSelection, method: meth);
			}
			else {
				sr.atomRep = new SurfaceRepresentation(sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			}
			sr.atomRepManager = new UnityMolSurfaceManager();
			sr.atomRepManager.Init(sr);
			break;
		case AtomType.DXSurface:
			DXReader dx = (DXReader)listArgs[0];

			float iso = 0.0f;
			if (listArgs[1] is double) {
				double tmp = (double)listArgs[1];
				iso = (float) tmp;
			}
			else {
				iso = (float)listArgs[1];
			}
			sr.atomRep = new DXSurfaceRepresentation(sr.subSelection.structures[0].ToSelectionName(), sr.subSelection, dx, iso);
			sr.atomRepManager = new UnityMolSurfaceManager();//A Surface manager!
			sr.atomRepManager.Init(sr);
			break;
		case AtomType.optihb:
			sr.atomRep = new AtomRepresentationOptihb(sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			sr.atomRepManager = new UnityMolHBallMeshManager();
			sr.atomRepManager.Init(sr);
			break;
		case AtomType.bondorder:
			sr.atomRep = new AtomRepresentationBondOrder(sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			sr.atomRepManager = new UnityMolAtomBondOrderManager();
			sr.atomRepManager.Init(sr);
			break;
		case AtomType.sphere:
			sr.atomRep = new AtomRepresentationSphere(sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			sr.atomRepManager = new UnityMolSphereManager();
			sr.atomRepManager.Init(sr);
			break;
		case AtomType.fieldlines:
			FieldLinesReader flR = sr.subSelection.structures[0].currentModel.fieldLinesR;
			sr.atomRep = new FieldLinesRepresentation(sr.subSelection.structures[0].ToSelectionName(), sr.subSelection, flR);
			sr.atomRepManager = new FieldLinesRepresentationManager();
			sr.atomRepManager.Init(sr);
			break;
		case AtomType.trace:
			sr.atomRep = new AtomRepresentationTube(sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			sr.atomRepManager = new UnityMolTubeManager();
			sr.atomRepManager.Init(sr);
			break;
		case AtomType.sugarribbons:
			sr.atomRep = new AtomRepresentationSugarRibbons(sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			sr.atomRepManager = new UnityMolSugarRibbonsManager();
			sr.atomRepManager.Init(sr);
			break;
		case AtomType.ellipsoid:
			sr.atomRep = new AtomEllipsoidRepresentation(sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			sr.atomRepManager = new UnityMolEllipsoidManager();
			sr.atomRepManager.Init(sr);
			break;
		case AtomType.point:
			sr.atomRep = new AtomRepresentationPoint(sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			sr.atomRepManager = new UnityMolAtomPointManager();
			sr.atomRepManager.Init(sr);
			break;
		case AtomType.noatom:
			break;
		}

		if (sr.atomRepManager != null) {
			if (UnityMolMain.isFogOn) {
				sr.atomRepManager.EnableDepthCueing();
				sr.atomRepManager.SetDepthCueingStart(UnityMolMain.fogStart);
				sr.atomRepManager.SetDepthCueingDensity(UnityMolMain.fogDensity);
			}
		}
	}


	private void ShowBonds(ref SubRepresentation sr, params object[] listArgs) {
		sr.bondRep = null;
		switch (repType.bondType) {
		case BondType.line:
			sr.bondRep = new BondRepresentationLine(sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			sr.bondRepManager = new UnityMolBondLineManager();
			sr.bondRepManager.Init(sr);
			break;
		case BondType.optihs:
			sr.bondRep = new BondRepresentationOptihs(sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			sr.bondRepManager = new UnityMolHStickMeshManager();
			sr.bondRepManager.Init(sr);
			break;
		case BondType.bondorder:
			sr.bondRep = new BondRepresentationBondOrder(sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			sr.bondRepManager = new UnityMolBondBondOrderManager();
			sr.bondRepManager.Init(sr);
			break;
		case BondType.hbond:
			if (listArgs.Length == 1) {
				sr.bondRep = new BondRepresentationHbonds(sr.subSelection.structures[0].ToSelectionName(), sr.subSelection, (bool)listArgs[0]);
			}
			else {
				sr.bondRep = new BondRepresentationHbonds(sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			}
			sr.bondRepManager = new UnityMolHbondManager();
			sr.bondRepManager.Init(sr);
			break;
		case BondType.hbondtube:
			if (listArgs.Length == 1) {
				sr.bondRep = new BondRepresentationHbondsTube(sr.subSelection.structures[0].ToSelectionName(), sr.subSelection, (bool)listArgs[0]);
			}
			else {
				sr.bondRep = new BondRepresentationHbondsTube(sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			}
			sr.bondRepManager = new UnityMolHbondTubeManager();
			sr.bondRepManager.Init(sr);
			break;
		case BondType.nobond:
			break;
		}

		if (sr.bondRepManager != null) {
			if (UnityMolMain.isFogOn) {
				sr.bondRepManager.EnableDepthCueing();
				sr.bondRepManager.SetDepthCueingStart(UnityMolMain.fogStart);
				sr.bondRepManager.SetDepthCueingDensity(UnityMolMain.fogDensity);
			}
		}
	}


	public void updateWithNewSelection(UnityMolSelection sel) {

		// if(sel.Count == selection.Count){//MEMO: Cannot do that as the selection is already updated
		// 	updateWithModel();
		// 	return;
		// }

		nbAtomsInRep = 0;
		nbBondsInRep = 0;

		//Save representation parameters in the same order
		UnityMolRepresentationParameters savedRepParamsAtom = null;
		UnityMolRepresentationParameters savedRepParamsBond = null;
		bool firstAtomRep = true;
		bool firstBondRep = true;

		foreach (SubRepresentation sr in subReps) {
			if (sr.atomRepManager != null) {
				if (firstAtomRep) {
					savedRepParamsAtom = sr.atomRepManager.Save();
					firstAtomRep = false;
				}
				sr.atomRepManager.Clean();
			}
			if (sr.bondRepManager != null) {
				if(firstBondRep){
					savedRepParamsBond = sr.bondRepManager.Save();
					firstBondRep = false;
				}
				sr.bondRepManager.Clean();
			}
		}
		subReps.Clear();

		Dictionary<UnityMolStructure, UnityMolSelection> byStruc = cutSelectionByStructure(sel);

		foreach (UnityMolStructure s in byStruc.Keys) {
			SubRepresentation sr;
			sr.subSelection = byStruc[s];

			sr.atomRep = null;
			sr.bondRep = null;
			sr.atomRepManager = null;
			sr.bondRepManager = null;
			sr.representationParent = null;
			sr.atomRepresentationTransform = null;
			sr.bondRepresentationTransform = null;


			ShowAtoms(ref sr, savedArgs);
			ShowBonds(ref sr, savedArgs);

			if (sr.atomRep != null) {
				sr.representationParent = sr.atomRep.representationParent;
				sr.atomRepresentationTransform = sr.atomRep.representationTransform;
				nbAtomsInRep += sr.atomRep.nbAtoms;
			}
			if (sr.bondRep != null) {
				sr.bondRepresentationTransform = sr.bondRep.representationTransform;
				if (sr.representationParent == null) {
					sr.representationParent = sr.bondRep.representationParent;
				}
				nbBondsInRep += sr.bondRep.nbBonds;
			}

			subReps.Add(sr);
		}

		bool saveIsAlte = selection.isAlterable;
		selection.isAlterable = true;
		selection.atoms = sel.atoms;
		selection.bonds = sel.bonds;
		selection.isAlterable = saveIsAlte;


		if (!isEnabled) {
			Hide();
		}

		//Restore with saved parameters
		foreach (SubRepresentation sr in subReps) {

			if (sr.atomRepManager != null && savedRepParamsAtom != null) {
				sr.atomRepManager.Restore(savedRepParamsAtom);
			}
			if (sr.bondRepManager != null && savedRepParamsBond != null) {
				sr.bondRepManager.Restore(savedRepParamsBond);
			}
		}

		UnityMolMain.getRepresentationManager().UpdateActiveRepresentations();
	}



	public void updateWithTrajectory() {
		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null)
				sr.atomRepManager.updateWithTrajectory();
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null)
				sr.bondRepManager.updateWithTrajectory();
		}
	}

	public void updateWithModel() {
		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null)
				sr.atomRepManager.updateWithModel();
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null)
				sr.bondRepManager.updateWithModel();
		}
	}

	public void Show() {
		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null)
				sr.atomRepManager.EnableRenderers();
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null)
				sr.bondRepManager.EnableRenderers();
		}
		isEnabled = true;
		UnityMolMain.getRepresentationManager().UpdateActiveRepresentations();
	}

	public void Hide() {
		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null)
				sr.atomRepManager.DisableRenderers();
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null)
				sr.bondRepManager.DisableRenderers();
		}
		isEnabled = false;
		UnityMolMain.getRepresentationManager().UpdateActiveRepresentations();
	}



	public void SetColor(UnityMolAtom atom, Color col) {
		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null){
				sr.atomRepManager.SetColor(col, atom);
				sr.atomRep.colorationType = colorType.custom;
			}
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null){
				sr.bondRepManager.SetColor(col, atom);
				sr.bondRep.colorationType = colorType.custom;
			}
		}
	}

	public void SetColor(List<UnityMolAtom> atoms, Color col) {
		foreach (UnityMolAtom a in atoms) {
			SetColor(a, col);
		}
	}

	public void SetColors(List<UnityMolAtom> atoms, Color col) {
		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null){
				sr.atomRepManager.SetColors(col, atoms);
				sr.atomRep.colorationType = colorType.custom;
			}
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null){
				sr.bondRepManager.SetColors(col, atoms);
				sr.bondRep.colorationType = colorType.custom;
			}
		}
	}
	public void SetColors(List<UnityMolAtom> atoms, List<Color> cols) {
		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null){
				sr.atomRepManager.SetColors(cols, atoms);
				sr.atomRep.colorationType = colorType.custom;
			}
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null){
				sr.bondRepManager.SetColors(cols, atoms);
				sr.bondRep.colorationType = colorType.custom;
			}
		}
	}

	public void ColorByAtom() {
		List<Color> colors = new List<Color>(selection.Count);

		foreach (UnityMolAtom a in selection.atoms) {
			colors.Add(a.color);
		}

		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null) {
				sr.atomRepManager.SetColors(colors, selection.atoms);
				sr.atomRep.colorationType = colorType.atom;
			}
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null) {
				sr.bondRepManager.SetColors(colors, selection.atoms);
				sr.bondRep.colorationType = colorType.atom;
			}
		}
	}

	public static Color hydroToColor(float dens, float minD, float maxD) {
		if (dens < 0.0f) {
			float t = Mathf.Clamp(dens / minD, 0.0f, 1.0f);
			return Color.Lerp(Color.white, Color.red, t);
		}

		float tp = Mathf.Clamp(dens / maxD, 0.0f, 1.0f);
		return Color.Lerp(Color.white, Color.blue, tp);
	}

	public static Color bfactorToColorNorm(float bf, Color startColor, Color endColor,
	                                       float minB = 5.0f, float maxB = 100.0f) {
		float mid = (maxB - minB) * 0.5f;
		if (bf < mid) {
			endColor = Color.white;
		}
		else {
			startColor = Color.white;
		}

		float t = (bf - minB) / (maxB - minB);
		return Color.Lerp(startColor, endColor, t);
	}

	public static Color bfactorToColor(float bf, Color startColor, Color endColor,
	                                   float minB = 5.0f, float maxB = 100.0f) {
		float mid = (maxB - minB) * 0.5f;

		float t = (bf - minB) / (maxB - minB);
		return Color.Lerp(startColor, endColor, t);
	}
	public void ColorByHydro() {


		List<Color> colors = new List<Color>(selection.Count);

		foreach (UnityMolAtom a in selection.atoms) {
			float hydro = a.residue.kdHydro;
			Color col = hydroToColor(hydro, -4.5f, 4.5f);
			colors.Add(col);
		}

		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null) {
				sr.atomRepManager.SetColors(colors, selection.atoms);
				sr.atomRep.colorationType = colorType.hydro;
			}
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null) {
				sr.bondRepManager.SetColors(colors, selection.atoms);
				sr.bondRep.colorationType = colorType.hydro;
			}
		}
	}

	public void ColorByBfactor(Color startColor, Color endColor) {

		List<Color> colors = new List<Color>(selection.Count);

		float minBfac = selection.atoms[0].bfactor;
		float maxBfac = selection.atoms[0].bfactor;
		foreach (UnityMolAtom a in selection.atoms) {
			minBfac = Mathf.Min(a.bfactor, minBfac);
			maxBfac = Mathf.Max(a.bfactor, maxBfac);
		}

		foreach (UnityMolAtom a in selection.atoms) {
			// colors.Add(bfactorToColor(a.bfactor, startColor, endColor));
			colors.Add(bfactorToColorNorm(a.bfactor, startColor, endColor, minBfac, maxBfac));
		}

		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null) {
				sr.atomRepManager.SetColors(colors, selection.atoms);
				sr.atomRep.colorationType = colorType.bfactor;
				sr.atomRep.bfactorStartCol = startColor;
				sr.atomRep.bfactorEndCol = endColor;
			}
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null) {
				sr.bondRepManager.SetColors(colors, selection.atoms);
				sr.bondRep.colorationType = colorType.bfactor;
				sr.bondRep.bfactorStartCol = startColor;
				sr.bondRep.bfactorEndCol = endColor;
			}
		}
	}

	public void ColorByChain() {
		Dictionary<UnityMolChain, Color> chainCol = new Dictionary<UnityMolChain, Color>();
		int cptColor = 0;
		foreach (UnityMolAtom a in selection.atoms) {
			if (!chainCol.ContainsKey(a.residue.chain)) {
				chainCol[a.residue.chain] = UnityMolMain.atomColors.getColorFromPalette(cptColor);
				cptColor++;
			}
			// SetColor(a, chainCol[a.residue.chain]);
		}
		foreach (UnityMolChain c in chainCol.Keys) {
			foreach (SubRepresentation sr in subReps) {
				if (repType.atomType != AtomType.noatom && sr.atomRepManager != null) {
					sr.atomRepManager.SetColor(chainCol[c], c.ToSelection());
					sr.atomRep.colorationType = colorType.chain;
				}
				if (repType.bondType != BondType.nobond && sr.bondRepManager != null) {
					sr.bondRepManager.SetColor(chainCol[c], c.ToSelection());
					sr.bondRep.colorationType = colorType.chain;
				}
			}
		}
	}

	// public void ColorByModel() {
	// 	Dictionary<UnityMolModel, Color> modelCol = new Dictionary<UnityMolModel, Color>();
	// 	int cptColor = 0;
	// 	foreach (UnityMolAtom a in selection.atoms) {
	// 		if (!modelCol.ContainsKey(a.residue.chain.model)) {
	// 			modelCol[a.residue.chain.model] = UnityMolMain.atomColors.getColorFromPalette(cptColor);
	// 			cptColor++;
	// 		}
	// 	}
	// 	foreach (UnityMolModel m in modelCol.Keys) {
	// 		foreach (SubRepresentation sr in subReps) {
	// 			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null) {
	// 				sr.atomRepManager.SetColor(modelCol[m], c.ToSelection());
	// 				sr.atomRep.colorationType = colorType.model;
	// 			}
	// 			if (repType.bondType != BondType.nobond && sr.bondRepManager != null) {
	// 				sr.bondRepManager.SetColor(modelCol[m], c.ToSelection());
	// 				sr.bondRep.colorationType = colorType.model;
	// 			}
	// 		}
	// 	}
	// }

	public void ColorByResidue() {

		List<Color> colors = new List<Color>(selection.Count);

		foreach (UnityMolAtom a in selection.atoms) {
			colors.Add(UnityMolMain.atomColors.getColorForResidue(a.residue));
		}

		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null) {
				sr.atomRepManager.SetColors(colors, selection.atoms);
				sr.atomRep.colorationType = colorType.res;
			}
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null) {
				sr.bondRepManager.SetColors(colors, selection.atoms);
				sr.bondRep.colorationType = colorType.res;
			}
		}
	}

	public void ColorByResType() {


		List<Color> colors = new List<Color>(selection.Count);

		foreach (UnityMolAtom a in selection.atoms) {
			colors.Add(UnityMolMain.atomColors.getColorRestypeForResidue(a.residue));
		}

		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null) {
				sr.atomRepManager.SetColors(colors, selection.atoms);
				sr.atomRep.colorationType = colorType.restype;
			}
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null) {
				sr.bondRepManager.SetColors(colors, selection.atoms);
				sr.bondRep.colorationType = colorType.restype;
			}
		}
	}
	public void ColorByResCharge() {

		List<Color> colors = new List<Color>(selection.Count);

		foreach (UnityMolAtom a in selection.atoms) {
			colors.Add(UnityMolMain.atomColors.getColorReschargeForResidue(a.residue));
		}

		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null) {
				sr.atomRepManager.SetColors(colors, selection.atoms);
				sr.atomRep.colorationType = colorType.rescharge;
			}
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null) {
				sr.bondRepManager.SetColors(colors, selection.atoms);
				sr.bondRep.colorationType = colorType.rescharge;
			}
		}
	}


	/// Jet method to compute a rainbox effect (0 = blue to 1 = red)
	public static Color rainbowColor(float t) {
		t = Mathf.Clamp(t, 0.0f, 1.0f);
		Color c = Color.white;
		if (t < 0.25f)
			c = new Color(0.0f, 4.0f * t, 1.0f);
		else if (t < 0.5f)
			c = new Color(0.0f, 1.0f, 1.0f + 4.0f * (0.25f - t));
		else if (t < 0.75f)
			c = new Color(4.0f * (t - 0.5f), 1.0f, 0.0f);
		else
			c = new Color(1.0f, 1.0f + 4.0f * (0.75f - t), 0.0f);

		return c;
	}

	/// Rainbow effect for the structure
	public void ColorBySequence() {

		Dictionary<UnityMolResidue, int> residuesOfS = new Dictionary<UnityMolResidue, int>();
		Dictionary<string, int> nbResPerS = new Dictionary<string, int>();
		foreach (UnityMolStructure s in selection.structures) {
			if (!nbResPerS.ContainsKey(s.uniqueName)) {
				nbResPerS[s.uniqueName] = 0;
			}
		}
		foreach (UnityMolAtom a in selection.atoms) {
			if (!residuesOfS.ContainsKey(a.residue)) {
				residuesOfS[a.residue] = nbResPerS[a.residue.chain.model.structure.uniqueName];
				nbResPerS[a.residue.chain.model.structure.uniqueName]++;
			}
		}


		Dictionary<UnityMolResidue, Color> resCol =
		    new Dictionary<UnityMolResidue, Color>(new LightResidueComparer());

		List<Color> colors = new List<Color>(selection.Count);

		foreach (UnityMolAtom a in selection.atoms) {
			if (!resCol.ContainsKey(a.residue)) {
				UnityMolStructure s = a.residue.chain.model.structure;
				int nbRes = nbResPerS[s.uniqueName];
				int idResPerS = residuesOfS[a.residue];

				resCol[a.residue] = rainbowColor(idResPerS / (float) nbRes);
			}
			colors.Add(resCol[a.residue]);
		}
		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null) {
				sr.atomRepManager.SetColors(colors, selection.atoms);
				sr.atomRep.colorationType = colorType.seq;
			}
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null) {
				sr.bondRepManager.SetColors(colors, selection.atoms);
				sr.bondRep.colorationType = colorType.seq;
			}
		}
	}

	/// Rainbow effect for the whole structure
	/// Not used
	public void ColorBySequence_global() {

		Dictionary<UnityMolResidue, int> residuesOfS = new Dictionary<UnityMolResidue, int>();
		Dictionary<string, int> nbResPerS = new Dictionary<string, int>();

		//Give a number to each residue in each structures
		foreach (UnityMolStructure s in selection.structures) {
			int cpt = 0;
			foreach (UnityMolChain c in s.currentModel.chains.Values) {
				foreach (UnityMolResidue r in c.residues.Values) {
					residuesOfS[r] = cpt;
					cpt++;
				}
			}
			nbResPerS[s.uniqueName] = cpt;
		}


		Dictionary<UnityMolResidue, Color> resCol =
		    new Dictionary<UnityMolResidue, Color>(new LightResidueComparer());

		List<Color> colors = new List<Color>(selection.Count);

		foreach (UnityMolAtom a in selection.atoms) {
			if (!resCol.ContainsKey(a.residue)) {
				UnityMolStructure s = a.residue.chain.model.structure;
				int nbRes = nbResPerS[s.uniqueName];
				int idResPerS = residuesOfS[a.residue];

				resCol[a.residue] = rainbowColor(idResPerS / (float) nbRes);
			}
			colors.Add(resCol[a.residue]);
		}
		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null) {
				sr.atomRepManager.SetColors(colors, selection.atoms);
				sr.atomRep.colorationType = colorType.seq;
			}
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null) {
				sr.bondRepManager.SetColors(colors, selection.atoms);
				sr.bondRep.colorationType = colorType.seq;
			}
		}
	}

	public void ShowAtom(UnityMolAtom atom , bool show = true) {
		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null)
				sr.atomRepManager.ShowAtom(atom, show);
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null)
				sr.bondRepManager.ShowAtom(atom, show);
		}
	}

	public void SetSize(UnityMolAtom atom, float size) {
		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null)
				sr.atomRepManager.SetSize(atom, size);
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null)
				sr.bondRepManager.SetSize(atom, size);
		}
	}
	public void ShowShadows(bool show) {
		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null)
				sr.atomRepManager.ShowShadows(show);
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null)
				sr.bondRepManager.ShowShadows(show);
		}
	}
	public void SetSizes(List<UnityMolAtom> atoms, float size) {
		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null)
				sr.atomRepManager.SetSizes(atoms, size);
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null)
				sr.bondRepManager.SetSizes(atoms, size);
		}
	}
	public void SetSizes(List<UnityMolAtom> atoms, List<float> sizes) {
		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null)
				sr.atomRepManager.SetSizes(atoms, sizes);
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null)
				sr.bondRepManager.SetSizes(atoms, sizes);
		}
	}


	public void ResetSize(UnityMolAtom atom) {
		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null)
				sr.atomRepManager.ResetSize(atom);
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null)
				sr.bondRepManager.ResetSize(atom);
		}
	}

	public void ResetSizes() {
		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null)
				sr.atomRepManager.ResetSizes();
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null)
				sr.bondRepManager.ResetSizes();
		}
	}

	public void ResetColor(UnityMolAtom atom) {
		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null)
				sr.atomRepManager.ResetColor(atom);
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null)
				sr.bondRepManager.ResetColor(atom);
		}
	}

	public void ResetColor() {
		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null)
				sr.atomRepManager.ResetColors();
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null)
				sr.bondRepManager.ResetColors();
		}
	}

	public override string ToString() {
		if (selection == null) {
			return "Representation of empty selection : " + repType.atomType + ":" + repType.bondType;
		}
		return "Representation of '" + selection.name + "': " + repType.atomType + ":" + repType.bondType + " " + selection.Count + " atoms";
	}

	public static bool operator ==(UnityMolRepresentation lhs, UnityMolRepresentation rhs) {

		if (ReferenceEquals(null, lhs) && ReferenceEquals(null, rhs)) { return true;}
		if (ReferenceEquals(null, lhs) || ReferenceEquals(null, rhs)) { return false;}
		if (lhs.selection == null && rhs.selection == null) { return true;}
		if (lhs.selection == null || rhs.selection == null) { return false;}
		if (lhs.selection.name != rhs.selection.name) {return false;}
		// if (lhs.selection.Count != rhs.selection.Count) {return false;}
		if (lhs.repType.atomType != rhs.repType.atomType) {return false;}
		if (lhs.repType.bondType != rhs.repType.bondType) {return false;}
		return true;
	}
	public static bool operator !=(UnityMolRepresentation lhs, UnityMolRepresentation rhs) {
		return !(lhs == rhs);
	}

	public override bool Equals(object obj) {
		if (obj is UnityMolRepresentation) {
			return this == obj;
		}
		return false;
	}

}

public struct SubRepresentation {
	public AtomRepresentation atomRep;
	public BondRepresentation bondRep;
	public UnityMolGenericRepresentationManager atomRepManager;
	public UnityMolGenericRepresentationManager bondRepManager;
	public Transform representationParent;
	public Transform atomRepresentationTransform;
	public Transform bondRepresentationTransform;
	public UnityMolSelection subSelection;
}
}