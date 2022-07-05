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
public class UnityMolRepresentationManager {


	public List<UnityMolRepresentation> representations = new List<UnityMolRepresentation>();

	public delegate void RepresentationVisibility();
	public static event RepresentationVisibility OnRepresentationVisibility;

	/// <summary>
	/// Store all the activated/visible representations
	/// </summary>
	public List<UnityMolRepresentation> activeRepresentations = new List<UnityMolRepresentation>();

	public HashSet<UnityMolAtom> atomsWithActiveRep = new HashSet<UnityMolAtom>();

	public void AddRepresentation(UnityMolSelection selection, AtomType atomRep = AtomType.optihb,
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

		// Dictionary<UnityMolStructure, UnityMolSelection> selByStructure = cutSelectionByStructure(selection);

		// //Create a representation for each structure of the selection
		// foreach (UnityMolStructure s in selByStructure.Keys) {
		// 	UnityMolSelection sel = selByStructure[s];

		// 	UnityMolRepresentation newRep = new UnityMolRepresentation(atomRep, bondRep, sel, s.ToSelectionName(), args);

		// 	if (!sel.representations.ContainsKey(repType)) {
		// 		sel.representations[repType] = new List<UnityMolRepresentation>();
		// 	}
		// 	if (!selection.representations.ContainsKey(repType)) {
		// 		selection.representations[repType] = new List<UnityMolRepresentation>();
		// 	}

		// 	selection.representations[repType].Add(newRep);
		// 	if(sel != selection){
		// 		sel.representations[repType].Add(newRep);
		// 	}

		// 	representations.Add(newRep);
		// 	s.representations.Add(newRep);
		// }

		UpdateActiveRepresentations();

	}


	//Only add atoms from the current model
	public void AddRepresentation(UnityMolStructure structure, AtomType atomRep, BondType bondRep, params object[] args) {


		UnityMolSelection selection = null;
		string selectionName = structure.ToSelectionName();
		UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

		if (selM.selections.ContainsKey(selectionName)) {
			selection = selM.selections[selectionName];
		}
		else {
			selection = structure.ToSelection();
			selection.fromSelectionLanguage = true;
			selection.MDASelString = structure.uniqueName;
		}

		AddRepresentation(selection, atomRep, bondRep, args);
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

		foreach (UnityMolRepresentation rep in representations) {
			if (rep.isActive()) {
				activeRepresentations.Add(rep);
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