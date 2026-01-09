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


		if (selection.extractTrajFrame) {
			bool changedSSInfo = false;
			List<UnityMolResidue.secondaryStructureType> savedSSInfo = null;

			if (selection.structures[0].updateSSWithTraj) {
				//Save current SS info
				savedSSInfo = selection.structures[0].ssInfoToList();
			}
			for (int id = 0; id < selection.extractTrajFrameIds.Count; id++) {
				if (selection.structures[0].updateSSWithTraj) { //Needs to update the secondary structure
					DSSP.assignSS_DSSP(selection.structures[0], false, id, selection);
					changedSSInfo = true;
				}
				SubRepresentation sr;
				sr.subSelection = sel;

				sr.atomRep = null;
				sr.bondRep = null;
				sr.atomRepManager = null;
				sr.bondRepManager = null;
				sr.representationParent = null;
				sr.atomRepresentationTransform = null;
				sr.bondRepresentationTransform = null;
				sr.idTrajExtract = id;

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
			if (changedSSInfo) {
				//Restore ss info
				int idR = 0;
				foreach (UnityMolChain c in selection.structures[0].currentModel.chains.Values) {
					foreach (UnityMolResidue r in c.residues) {
						r.secondaryStructure = savedSSInfo[idR];
						idR++;
					}
				}
			}
		}
		else {
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
				sr.idTrajExtract = -1;


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
			sr.atomRep = new CartoonRepresentation(sr.idTrajExtract, sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			sr.atomRepManager = new UnityMolCartoonManager();
			sr.atomRepManager.Init(sr);
			break;
		case AtomType.surface:
			if (listArgs.Length == 4) {
				bool cutByChain = (bool)listArgs[0];
				bool AO = (bool)listArgs[1];
				bool cutSurface = (bool)listArgs[2];
				SurfMethod method = (SurfMethod) listArgs[3];
				sr.atomRep = new SurfaceRepresentation(sr.idTrajExtract, sr.subSelection.structures[0].ToSelectionName(), sr.subSelection, cutByChain, AO, cutSurface, method);
			}
			else if (listArgs.Length == 1) {
				SurfMethod meth = (SurfMethod) listArgs[0];
				sr.atomRep = new SurfaceRepresentation(sr.idTrajExtract, sr.subSelection.structures[0].ToSelectionName(), sr.subSelection, method: meth);
			}
			else {
				sr.atomRep = new SurfaceRepresentation(sr.idTrajExtract, sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			}
			sr.atomRepManager = new UnityMolSurfaceManager();
			sr.atomRepManager.Init(sr);
			break;
		case AtomType.DXSurface:
			UnityMolStructureManager sm = UnityMolMain.getStructureManager();
			UnityMolStructure s = sm.GetStructure((string)listArgs[0]);
			if (s == null) {
				Debug.LogError("Structure not found");
				return;
			}
			if (s.dxr == null) {
				Debug.LogError("No map loaded for this structure");
				return;
			}
			DXReader dx = s.dxr;

			float iso = 10.0f;
			if (listArgs.Length > 1) {
				if (listArgs[1] is double) {
					double tmp = (double)listArgs[1];
					iso = (float) tmp;
				}
				else if (listArgs[1] is int) {
					int tmp = (int)listArgs[1];
					iso = (float) tmp;
				}
				else {
					iso = (float)listArgs[1];
				}
			}
			sr.atomRep = new DXSurfaceRepresentation(sr.subSelection.structures[0].ToSelectionName(), sr.subSelection, dx, iso);
			sr.atomRepManager = new UnityMolSurfaceManager();//A Surface manager!
			sr.atomRepManager.Init(sr);
			break;
		case AtomType.optihb:
			sr.atomRep = new AtomRepresentationOptihb(sr.idTrajExtract, sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			sr.atomRepManager = new UnityMolHBallMeshManager();
			sr.atomRepManager.Init(sr);
			break;
		case AtomType.bondorder:
			sr.atomRep = new AtomRepresentationBondOrder(sr.idTrajExtract, sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			sr.atomRepManager = new UnityMolAtomBondOrderManager();
			sr.atomRepManager.Init(sr);
			break;
		case AtomType.sphere:
			sr.atomRep = new AtomRepresentationSphere(sr.idTrajExtract, sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
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
			sr.atomRep = new AtomRepresentationTube(sr.idTrajExtract, sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			sr.atomRepManager = new UnityMolTubeManager();
			sr.atomRepManager.Init(sr);
			break;
		case AtomType.sugarribbons:
			sr.atomRep = new AtomRepresentationSugarRibbons(sr.idTrajExtract, sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			sr.atomRepManager = new UnityMolSugarRibbonsManager();
			sr.atomRepManager.Init(sr);
			break;
		case AtomType.sheherasade:
			sr.atomRep = new SheherasadeRepresentation(sr.idTrajExtract, sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			sr.atomRepManager = new UnityMolSheherasadeManager();
			sr.atomRepManager.Init(sr);
			break;
		case AtomType.ellipsoid:
			sr.atomRep = new AtomEllipsoidRepresentation(sr.idTrajExtract, sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			sr.atomRepManager = new UnityMolEllipsoidManager();
			sr.atomRepManager.Init(sr);
			break;
		case AtomType.point:
			sr.atomRep = new AtomRepresentationPoint(sr.idTrajExtract, sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			sr.atomRepManager = new UnityMolAtomPointManager();
			sr.atomRepManager.Init(sr);
			break;
		case AtomType.explosurf:
			Vector3 symO = (Vector3)listArgs[0];
			Vector3 symV = (Vector3)listArgs[1];
			float sliceSize = 10.0f;
			if (listArgs.Length > 2) {
				if (listArgs[2] is double) {
					double tmp = (double)listArgs[2];
					sliceSize = (float) tmp;
				}
				else if (listArgs[2] is int) {
					int tmp = (int)listArgs[2];
					sliceSize = (float) tmp;
				}
				else {
					sliceSize = (float)listArgs[2];
				}
			}

			sr.atomRep = new ExplodedSurfaceRepresentation(sr.idTrajExtract, sr.subSelection.structures[0].ToSelectionName(), sr.subSelection, symO, symV, sliceSize);
			sr.atomRepManager = new UnityMolSurfaceManager();//A Surface manager!
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
			sr.bondRep = new BondRepresentationLine(sr.idTrajExtract, sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			sr.bondRepManager = new UnityMolBondLineManager();
			sr.bondRepManager.Init(sr);
			break;
		case BondType.optihs:
			sr.bondRep = new BondRepresentationOptihs(sr.idTrajExtract, sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			sr.bondRepManager = new UnityMolHStickMeshManager();
			((UnityMolHStickMeshManager)sr.bondRepManager).hbmm = (UnityMolHBallMeshManager)sr.atomRepManager;
			sr.bondRepManager.Init(sr);
			break;
		case BondType.bondorder:
			sr.bondRep = new BondRepresentationBondOrder(sr.idTrajExtract, sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			sr.bondRepManager = new UnityMolBondBondOrderManager();
			sr.bondRepManager.Init(sr);
			break;
		case BondType.hbond:
			if (listArgs.Length == 1) {
				sr.bondRep = new BondRepresentationHbonds(sr.idTrajExtract, sr.subSelection.structures[0].ToSelectionName(), sr.subSelection, (bool)listArgs[0]);
			}
			else {
				sr.bondRep = new BondRepresentationHbonds(sr.idTrajExtract, sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
			}
			sr.bondRepManager = new UnityMolHbondManager();
			sr.bondRepManager.Init(sr);
			break;
		case BondType.hbondtube:
			if (listArgs.Length == 1) {
				sr.bondRep = new BondRepresentationHbondsTube(sr.idTrajExtract, sr.subSelection.structures[0].ToSelectionName(), sr.subSelection, (bool)listArgs[0]);
			}
			else {
				sr.bondRep = new BondRepresentationHbondsTube(sr.idTrajExtract, sr.subSelection.structures[0].ToSelectionName(), sr.subSelection);
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

		if (selection.extractTrajFrame) {
			//TODO implement this correctly
			Debug.LogError("Cannot update a special selection with new content. Please recreate a selection");
			return;
		}

		//MEMO: Cannot do that as the selection is already updated
		// if(sel.Count == selection.atoms.Count){
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
				sr.atomRep.Clean();
			}
			if (sr.bondRepManager != null) {
				if (firstBondRep) {
					savedRepParamsBond = sr.bondRepManager.Save();
					firstBondRep = false;
				}
				sr.bondRepManager.Clean();
				sr.bondRep.Clean();

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
			sr.idTrajExtract = -1;

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
		if (!selection.updateRepWithTraj)
			return;
		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null)
				sr.atomRepManager.updateWithTrajectory();
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null)
				sr.bondRepManager.updateWithTrajectory();
		}
	}

	public void updateWithModel() {
		if (selection.extractTrajFrame)
			return;
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



	public void SetColor(UnityMolAtom atom, Color32 col) {
		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null) {
				sr.atomRepManager.SetColor(col, atom);
				sr.atomRep.colorationType = colorType.custom;
			}
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null) {
				sr.bondRepManager.SetColor(col, atom);
				sr.bondRep.colorationType = colorType.custom;
			}
		}
	}

	public void SetColor(List<UnityMolAtom> atoms, Color32 col) {
		foreach (UnityMolAtom a in atoms) {
			SetColor(a, col);
		}
	}

	public void SetColors(List<UnityMolAtom> atoms, Color32 col) {
		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null) {
				sr.atomRepManager.SetColors(col, atoms);
				sr.atomRep.colorationType = colorType.custom;
			}
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null) {
				sr.bondRepManager.SetColors(col, atoms);
				sr.bondRep.colorationType = colorType.custom;
			}
		}
	}
	public void SetColors(List<UnityMolAtom> atoms, List<Color32> cols) {
		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null) {
				sr.atomRepManager.SetColors(cols, atoms);
				sr.atomRep.colorationType = colorType.custom;
			}
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null) {
				sr.bondRepManager.SetColors(cols, atoms);
				sr.bondRep.colorationType = colorType.custom;
			}
		}
	}

	public void ColorByAtom() {
		List<Color32> colors = new List<Color32>(selection.atoms.Count);

		foreach (UnityMolAtom a in selection.atoms) {
			colors.Add(a.color32);
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

	public static Color32 hydroToColor(float dens, float minD, float maxD) {
		if (dens < 0.0f) {
			float t = Mathf.Clamp(dens / minD, 0.0f, 1.0f);
			return Color32.Lerp((Color32)Color.white, (Color32)Color.red, t);
		}

		float tp = Mathf.Clamp(dens / maxD, 0.0f, 1.0f);
		return Color32.Lerp((Color32)Color.white, (Color32)Color.blue, tp);
	}

	public static Color32 bfactorToColorNorm(float bf, Color32 startColor, Color32 midCol, Color32 endColor,
	        float minB = 5.0f, float maxB = 100.0f) {

		float div = (Mathf.Abs(maxB - minB) > 0.001f ? 1.0f / (maxB - minB) : 1.0f);
		float normalizedBf = (bf - minB) * div;//bf between 0 and 1

		if (normalizedBf < 0.5f) {
			endColor = midCol;
		}
		else {
			startColor = midCol;
		}

		return Color.Lerp(startColor, endColor, normalizedBf);
	}

	public static Color32 bfactorToColor(float bf, Color32 startColor, Color32 endColor,
	                                   float minB = 5.0f, float maxB = 100.0f) {

		float div = (Mathf.Abs(maxB - minB) > 0.001f ? 1.0f / (maxB - minB) : 1.0f);
		float normalizedBf = (bf - minB) * div;//bf between 0 and 1

		return Color32.Lerp(startColor, endColor, normalizedBf);
	}

	public void ColorByHydro() {


		List<Color32> colors = new List<Color32>(selection.atoms.Count);

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

	public void ColorByBfactor(Color startColor, Color midCol, Color endColor) {

		Color32 startColor32 = (Color32) startColor;
		Color32 midCol32 = (Color32) midCol;
		Color32 endColor32 = (Color32) endColor;

		List<Color32> colors = new List<Color32>(selection.atoms.Count);

		bool containsCA = false;
		foreach (UnityMolAtom a in selection.atoms) {
			if (a.name == "CA" && a.type == "C") {
				containsCA = true;
			}
		}

		float minBfac = selection.atoms[0].bfactor;
		float maxBfac = selection.atoms[0].bfactor;
		foreach (UnityMolAtom a in selection.atoms) {
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
			foreach (UnityMolAtom a in selection.atoms) {
				colors.Add(bfactorToColorNorm(a.bfactor, startColor32, midCol32, endColor32, minBfac, maxBfac));
			}
		}
		else {
			foreach (UnityMolAtom a in selection.atoms) {
				if (a.residue.atoms.ContainsKey("CA")) {
					float bf = a.residue.atoms["CA"].bfactor;
					colors.Add(bfactorToColorNorm(bf, startColor32, midCol32, endColor32, minBfac, maxBfac));
				}
				else
					colors.Add(bfactorToColorNorm(a.bfactor, startColor32, midCol32, endColor32, minBfac, maxBfac));
			}
		}


		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null) {
				sr.atomRepManager.SetColors(colors, selection.atoms);
				sr.atomRep.colorationType = colorType.bfactor;
				sr.atomRep.bfactorStartCol = startColor;
				sr.atomRep.bfactorMidColor = midCol;
				sr.atomRep.bfactorEndCol = endColor;
			}
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null) {
				sr.bondRepManager.SetColors(colors, selection.atoms);
				sr.bondRep.colorationType = colorType.bfactor;
				sr.bondRep.bfactorStartCol = startColor;
				sr.bondRep.bfactorMidColor = midCol;
				sr.bondRep.bfactorEndCol = endColor;
			}
		}
	}

	public void ColorByChain() {
		Dictionary<UnityMolChain, Color32> chainCol = new Dictionary<UnityMolChain, Color32>();
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
				UnityMolSelection sel = c.ToSelection(false);
				if (repType.atomType != AtomType.noatom && sr.atomRepManager != null) {
					sr.atomRepManager.SetColor(chainCol[c], sel);
					sr.atomRep.colorationType = colorType.chain;
				}
				if (repType.bondType != BondType.nobond && sr.bondRepManager != null) {
					sr.bondRepManager.SetColor(chainCol[c], sel);
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

		List<Color32> colors = new List<Color32>(selection.atoms.Count);

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


		List<Color32> colors = new List<Color32>(selection.atoms.Count);

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

		List<Color32> colors = new List<Color32>(selection.atoms.Count);

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

	public void ColorByResid() {


		List<Color32> colors = new List<Color32>(selection.atoms.Count);

		foreach (UnityMolAtom a in selection.atoms) {
			colors.Add(UnityMolMain.atomColors.getColorFromPalette(a.residue.id));
		}

		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null) {
				sr.atomRepManager.SetColors(colors, selection.atoms);
				sr.atomRep.colorationType = colorType.resid;
			}
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null) {
				sr.bondRepManager.SetColors(colors, selection.atoms);
				sr.bondRep.colorationType = colorType.resid;
			}
		}
	}

	public void ColorByResnum() {
		List<Color32> colors = new List<Color32>(selection.atoms.Count);

		foreach (UnityMolAtom a in selection.atoms) {
			colors.Add(UnityMolMain.atomColors.getColorFromPalette(a.residue.resnum));
		}

		foreach (SubRepresentation sr in subReps) {
			if (repType.atomType != AtomType.noatom && sr.atomRepManager != null) {
				sr.atomRepManager.SetColors(colors, selection.atoms);
				sr.atomRep.colorationType = colorType.resnum;
			}
			if (repType.bondType != BondType.nobond && sr.bondRepManager != null) {
				sr.bondRepManager.SetColors(colors, selection.atoms);
				sr.bondRep.colorationType = colorType.resnum;
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
			if (!nbResPerS.ContainsKey(s.name)) {
				nbResPerS[s.name] = 0;
			}
		}
		foreach (UnityMolAtom a in selection.atoms) {
			if (!residuesOfS.ContainsKey(a.residue)) {
				residuesOfS[a.residue] = nbResPerS[a.residue.chain.model.structure.name];
				nbResPerS[a.residue.chain.model.structure.name]++;
			}
		}


		Dictionary<UnityMolResidue, Color32> resCol =
		    new Dictionary<UnityMolResidue, Color32>(new LightResidueComparer());

		List<Color32> colors = new List<Color32>(selection.atoms.Count);

		foreach (UnityMolAtom a in selection.atoms) {
			if (!resCol.ContainsKey(a.residue)) {
				UnityMolStructure s = a.residue.chain.model.structure;
				int nbRes = nbResPerS[s.name];
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
				foreach (UnityMolResidue r in c.residues) {
					residuesOfS[r] = cpt;
					cpt++;
				}
			}
			nbResPerS[s.name] = cpt;
		}


		Dictionary<UnityMolResidue, Color32> resCol =
		    new Dictionary<UnityMolResidue, Color32>(new LightResidueComparer());

		List<Color32> colors = new List<Color32>(selection.atoms.Count);

		foreach (UnityMolAtom a in selection.atoms) {
			if (!resCol.ContainsKey(a.residue)) {
				UnityMolStructure s = a.residue.chain.model.structure;
				int nbRes = nbResPerS[s.name];
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
		// if (lhs.selection.Count != rhs.selection.atoms.Count) {return false;}
		if (lhs.repType.atomType != rhs.repType.atomType) {return false;}
		if (lhs.repType.bondType != rhs.repType.bondType) {return false;}
		return true;
	}
	public static bool operator !=(UnityMolRepresentation lhs, UnityMolRepresentation rhs) {
		return !(lhs == rhs);
	}

	public override bool Equals(object obj) {
		if (obj is UnityMolRepresentation) {
			return this == (UnityMolRepresentation)obj;
		}
		return false;
	}

    public override int GetHashCode()
    {
    	if(selection == null){
    		return 0;
    	}
        string code = selection.name + ":" + repType.atomType + ":" + repType.bondType;
        return code.GetHashCode();
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
	public int idTrajExtract;//Used by special extract selections
}
}
