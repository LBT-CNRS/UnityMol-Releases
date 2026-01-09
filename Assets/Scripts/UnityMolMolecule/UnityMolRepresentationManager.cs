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
public class UnityMolRepresentationManager : MonoBehaviour {

	/// <summary>
	/// All representations
	/// </summary>
	public List<UnityMolRepresentation> representations = new List<UnityMolRepresentation>();

	public delegate void RepresentationVisibility();
	public static event RepresentationVisibility OnRepresentationVisibility;

	public delegate void NewRepresentation();
	public static event NewRepresentation OnNewRepresentation;

	public delegate void DelRepresentation();
	public static event DelRepresentation OnRepresentationDeleted;

	/// <summary>
	/// Store all the activated/visible representations
	/// </summary>
	public List<UnityMolRepresentation> activeRepresentations = new List<UnityMolRepresentation>();

	/// Store all the activated/visible representations from a trajectory extraction
	public List<UnityMolRepresentation> extractedTrajRep = new List<UnityMolRepresentation>();
	/// Count atoms extracted from trajectory with an active representation, with all positions
	public int countExtractTrajActive = 0;
	/// Count atoms extracted from trajectory with an active representation, only the atom array
	public int countExtractTrajActiveOnlyAtom = 0;

	public HashSet<UnityMolAtom> atomsWithActiveRep = new HashSet<UnityMolAtom>();

	void Awake() {
		RaytracerManager.OnRTActivate += callRTInit;
	}
	void OnDestroy() {
		RaytracerManager.OnRTActivate -= callRTInit;
	}

	void callRTInit() {
		foreach (UnityMolRepresentation r in representations) {
			foreach (SubRepresentation sr in r.subReps) {
				if (sr.atomRepManager != null) {
					sr.atomRepManager.InitRT();
				}
				if (sr.bondRepManager != null) {
					sr.bondRepManager.InitRT();
				}
			}
		}
	}

	public UnityMolRepresentation AddRepresentation(UnityMolSelection selection, AtomType atomRep = AtomType.optihb,
	        BondType bondRep = BondType.optihs, params object[] args) {

		RepType repType;
		repType.atomType = atomRep;
		repType.bondType = bondRep;

		UnityMolRepresentation newRep = new UnityMolRepresentation(atomRep, bondRep, selection, args);

		if (!selection.representations.ContainsKey(repType)) {
			selection.representations[repType] = new List<UnityMolRepresentation>();
		}
		selection.representations[repType].Add(newRep);

		representations.Add(newRep);

		foreach (UnityMolStructure s in selection.structures) {
			s.representations.Add(newRep);
		}

		UpdateActiveRepresentations();

		if (OnNewRepresentation != null)
			OnNewRepresentation();

		return newRep;

	}

	public List<UnityMolRepresentation> representationExists(string nameSel, RepType repType) {

		UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

		if (!selM.selections.ContainsKey(nameSel)) {
			return null;
		}
		UnityMolSelection sel = selM.selections[nameSel];

		if (!sel.representations.ContainsKey(repType)) {
			return null;
		}
		return sel.representations[repType];
	}

	public void AddRepresentationToStructures(UnityMolSelection selection, UnityMolRepresentation newRep) {
		Dictionary<UnityMolStructure, bool> structures = new Dictionary<UnityMolStructure, bool>();
		for (int i = 0; i < selection.atoms.Count; i++) {
			UnityMolStructure structure = selection.atoms[i].residue.chain.model.structure;
			structures[structure] = true;
		}
		foreach (UnityMolStructure s in structures.Keys) {
			s.representations.Add(newRep);
		}
	}


	public void DeleteRepresentationFromStructures(UnityMolRepresentation rep) {
		//Get all the structures having some atoms in this representation
		HashSet<UnityMolStructure> structures = new HashSet<UnityMolStructure>();
		for (int i = 0; i < rep.selection.atoms.Count; i++) {
			UnityMolStructure structure = rep.selection.atoms[i].residue.chain.model.structure;
			structures.Add(structure);
		}

		foreach (UnityMolStructure s in structures) {
			s.representations.RemoveAll(r => r.selection.name == rep.selection.name && r.repType.atomType == rep.repType.atomType && r.repType.bondType == rep.repType.bondType);
		}

		// UpdateActiveRepresentations();
	}
	public void UpdateActiveRepresentations() {
		if (OnRepresentationVisibility != null) {
			OnRepresentationVisibility();
		}

		activeRepresentations.Clear();
		extractedTrajRep.Clear();
		countExtractTrajActive = 0;
		countExtractTrajActiveOnlyAtom = 0;

		foreach (UnityMolRepresentation rep in representations) {
			if (rep.isActive() && !rep.selection.extractTrajFrame) {
				activeRepresentations.Add(rep);
			}
			else if (rep.isActive() && rep.selection.extractTrajFrame) {
				extractedTrajRep.Add(rep);
				countExtractTrajActive += rep.selection.Count;
				countExtractTrajActiveOnlyAtom += rep.selection.atoms.Count;
			}
		}

		UpdateActiveColliders();
	}



	//Should be called by UnityMolSelectionManager
	public void Delete(UnityMolRepresentation r, bool delNow = true) {

		if (!representations.Contains(r)) {
			Debug.LogWarning("Representation to delete does not exist " + r);
			return;
		}
		if (delNow)
			representations.Remove(r);
		DeleteRepresentationFromStructures(r);
		r.Clean();
		UpdateActiveRepresentations();

		if (OnRepresentationDeleted != null)
			OnRepresentationDeleted();

	}
	public void Clean() {

		for (int i = 0; i < representations.Count; i++) {
			Delete(representations[i], false);
		}

		representations.Clear();
		UpdateActiveRepresentations();
	}


	/// <summary>
	/// Keep track of atoms with an active representations
	/// For cartoon representations, we only add backbone atoms (ignoring H atoms)
	/// </summary>
	public void UpdateActiveColliders() {
		atomsWithActiveRep.Clear();
		foreach (UnityMolRepresentation r in representations) {
			if (r.isEnabled) {
				if (r.repType.atomType == AtomType.cartoon) {
					foreach (UnityMolAtom a in r.selection.atoms) {
						//TODO integrate this with isBackBone
						if (a.residue.chain.model.structure.structureType ==
						        UnityMolStructure.MolecularType.Martini) {

							if (a.name.StartsWith("BB")) {
								atomsWithActiveRep.Add(a);
							}
						}

						//Ignoring H atoms by setting the "bonds" argument to null
						if (MDAnalysisSelection.isBackBone(a, null)) {
							atomsWithActiveRep.Add(a);
						}
					}
				}
				else if (r.repType.atomType == AtomType.trace) {

					foreach (UnityMolAtom a in r.selection.atoms) {
						string toFind = "CA";
						if (MDAnalysisSelection.isNucleic(a.residue)) {
							toFind = "P";
						}
						if (a.name == toFind) {
							atomsWithActiveRep.Add(a);
						}
					}
				}
				else {
					foreach (UnityMolAtom a in r.selection.atoms) {
						atomsWithActiveRep.Add(a);
					}
				}
			}
		}

		UnityMolMain.getCustomRaycast().needsFullUpdate = true;

	}

	void Update() {

		foreach (UnityMolRepresentation r in activeRepresentations) {
			foreach (SubRepresentation sr in r.subReps) {
				if (sr.atomRepManager != null) {
					sr.atomRepManager.UpdateLike();
				}
				if (sr.bondRepManager != null) {
					sr.bondRepManager.UpdateLike();
				}
			}
		}
	}

}

// public class RepComparer : IEqualityComparer<UnityMolRepresentation>
// {
//     public bool Equals(UnityMolRepresentation r1, UnityMolRepresentation r2)
//     {
//         if (r1 == null && r2 == null) { return true;}
//         if (r1 == null || r2 == null) { return false;}
//         if (r1.selection.name != r2.selection.name) {return false;}
//         // if (r1.selection.Count != r2.selection.Count) {return false;}
//         if (r1.repType.atomType != r2.repType.atomType) {return false;}
//         if (r1.repType.bondType != r2.repType.bondType) {return false;}
//         return true;
//     }
//     public int GetHashCode(UnityMolRepresentation r)
//     {
//     	if(r.selection == null){
//     		return 0;
//     	}
//         string code = r.selection.name+":"+r.repType.atomType+":"+r.repType.bondType;
//         // string code = r.selection.name+":"+r.selection.Count+":"+r.repType.atomType+":"+r.repType.bondType;
//         return code.GetHashCode();
//     }
// }
}