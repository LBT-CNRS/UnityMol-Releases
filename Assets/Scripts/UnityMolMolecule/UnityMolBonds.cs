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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace UMol {
/// <summary>
/// Stores the bonds of the model as a dictionary of <UnityMolAtom, UnityMolAtom[]>
/// </summary>
public class UnityMolBonds {

	/// <summary>
	/// Number of bonds per atom, this should change if a Martini model is parsed
	/// </summary>
	public int NBBONDS = 27;

	/// <summary>
	/// Store the bonds as a dictionary of atom id associated with an array of atom ids
	/// Both A1-A2 and A2-A1 are stored
	/// (NBBONDS preallocated atom ids with -1)
	/// </summary>
	public Dictionary<int, int[]> bonds;


	/// <summary>
	/// Total number of bonds stored
	/// </summary>
	public int bondsCount = 0;

	/// <summary>
	/// UnityMolBonds constructor initializing the dictionary
	/// </summary>
	public UnityMolBonds() {
		bonds = new Dictionary<int, int[]>();
		bondsCount = 0;
	}

	/// <summary>
	/// Returns the number of bonds stored
	/// </summary>
	public int Count {
		get {
			if (bondsCount == -1) reCount();
			return bondsCount;
		}
	}

	public int Length {
		get {
			return Count;
		}
	}

	/// <summary>
	/// Add a UnityMolAtom in the bonds dictionary, prints a Debug.LogError when no more space is available
	/// </summary>
	public void Add(UnityMolAtom atom, UnityMolAtom bonded) {
		int[] res = null;
		int idA = atom.idInAllAtoms;
		int idB = bonded.idInAllAtoms;

		if (idA == -1 || idB == -1) {
			Debug.LogWarning("Wrong atom id, this is bad");
			return;
		}
		if (idA == idB) {
			Debug.LogError("Cannot bind atom to itself");
			return;
		}

		if (bonds.TryGetValue(idA, out res)) {
			bool added = false;
			for (int i = 0; i < NBBONDS; i++) {
				if (res[i] == idB) {//Already there
					added = true;
					break;
				}

				if (res[i] == -1) {//Not there
					res[i] = idB;
					added = true;
					bondsCount++;
					break;
				}
			}
			if (!added) {
				Debug.LogError("More than " + NBBONDS + " bonds for atom " + atom);
				throw new System.Exception("AddBondError");
			}
		}
		else {
			bonds[idA] = new int[NBBONDS];
			bonds[idA][0] = idB;
			for (int i = 1; i < NBBONDS; i++)
				bonds[idA][i] = -1;
			bondsCount++;
		}

		//Add the reverse
		if (bonds.TryGetValue(idB, out res)) {
			bool added = false;
			for (int i = 0; i < NBBONDS; i++) {
				if (res[i] == idA) {//Already there
					added = true;
					break;
				}

				if (res[i] == -1) {//Not there
					res[i] = idA;
					added = true;
					break;
				}
			}
			if (!added) {
				Debug.LogError("More than " + NBBONDS + " bonds for atom " + atom);
				throw new System.Exception("AddBondError");
			}
		}
		else {
			bonds[idB] = new int[NBBONDS];
			bonds[idB][0] = idA;
			for (int i = 1; i < NBBONDS; i++)
				bonds[idB][i] = -1;
		}
	}
	/// <summary>
	/// Add a couple of atom ids in the dictionary, prints a Debug.LogError when no more space is available
	/// </summary>
	public void Add(int idA, int idB, UnityMolModel curM) {
		UnityMolAtom a = curM.allAtoms[idA];
		UnityMolAtom b = curM.allAtoms[idB];
		Add(a, b);
	}

	/// <summary>
	/// Remove all bonds containing the given atom
	/// </summary>
	public void Remove(int idA, UnityMolModel curM) {
		if (idA == -1) {
			Debug.LogWarning("Wrong atom id, this is bad");
			return;
		}
		int[] res = null;
		if (bonds.TryGetValue(idA, out res)) {

			//Loop over all bonds to look for the atom and rewrite the array if needed
			foreach (int idB in res) {
				if (idB != -1) {
					bool contains = false;
					for (int i = 0; i < NBBONDS; i++) {
						if (bonds[idB][i] == idA) {
							bonds[idB][i] = -1;
							contains = true;
						}
					}
					if (contains) {//-1 should be at the end of the array
						Array.Reverse(bonds[idB]);
					}
				}
			}

			bonds.Remove(idA);
		}
		bondsCount = -1;
	}

	/// <summary>
	/// Remove all bonds containing the given atom
	/// </summary>
	public void Remove(UnityMolAtom a) {
		Remove(a.idInAllAtoms, a.residue.chain.model);
	}


	/// <summary>
	/// Returns the number of atoms bonded to the atom A
	/// </summary>
	public int countBondedAtoms(UnityMolAtom a) {
		int[] res = null;
		if (bonds.TryGetValue(a.idInAllAtoms, out res)) {
			int count = 0;
			for (int i = 0; i < NBBONDS; i++) {
				if (res[i] != -1) {
					count++;
				}
			}
			return count;
		}

		// Debug.LogWarning("Did not find atom "+a+" in bonds");
		return -1;
	}

	///Recount the number of bonded atoms
	private void reCount() {
		int count = 0;
		foreach (int idA in bonds.Keys) {
			foreach (int idB in bonds[idA]) {
				if (idB != -1) {
					count++;
				}
			}
		}
		bondsCount = count / 2;
	}

	/// <summary>
	/// Returns the number of atoms bonded to the atom with id
	/// </summary>
	public int countBondedAtoms(int ida) {
		int[] res = null;
		if (bonds.TryGetValue(ida, out res)) {
			int count = 0;
			for (int i = 0; i < NBBONDS; i++) {
				if (res[i] != -1) {
					count++;
				}
			}
			return count;
		}

		// Debug.LogWarning("Did not find atom "+a+" in bonds");
		return -1;
	}

	/// <summary>
	/// Check if atom a1 and atom a2 are bonded
	/// </summary>
	public bool isBondedTo(UnityMolAtom a1, UnityMolAtom a2) {
		int a1i = a1.idInAllAtoms;
		int a2i = a2.idInAllAtoms;
		return isBondedTo(a1i, a2i);
	}

	/// <summary>
	/// Check if atom id a1 and atom id a2 are bonded
	/// </summary>
	public bool isBondedTo(int a1, int a2) {
		int[] res = null;
		if (bonds.TryGetValue(a1, out res)) {
			for (int i = 0; i < NBBONDS; i++) {
				if (res[i] == a2) {
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Get bond order, check current model for parsed bond orders
	/// If atoms are not bonded returns -1
	/// </summary>
	public bondOrderType getBondOrder(UnityMolAtom a1, UnityMolAtom a2, bool useProtDef = false) {
		bondOrderType res;
		res.order = -1.0f;
		res.btype = bondType.covalent;
		if (!isBondedTo(a1, a2)) {
			return res;
		}
		AtomDuo d = new AtomDuo(a1, a2);
		AtomDuo dinv = new AtomDuo(a2, a1);
		UnityMolModel m = a1.residue.chain.model;
		if (m.covBondOrders != null) {
			if (m.covBondOrders.ContainsKey(d)) {
				return m.covBondOrders[d];
			}
			else if (m.covBondOrders.ContainsKey(dinv)) {
				return m.covBondOrders[dinv];
			}
		}

		if (useProtDef) {
			return getBondOrderProt(a1, a2);
		}
		return res;

	}

	/// <summary>
	/// Get bond order (for protein residues only)
	/// If not protein residues returns 1
	/// If atoms are not bonded returns -1
	/// </summary>
	public bondOrderType getBondOrderProt(UnityMolAtom a1, UnityMolAtom a2) {
		bondOrderType res;
		res.order = -1.0f;
		res.btype = bondType.covalent;

		if (!isBondedTo(a1, a2)) {
			return res;
		}
		List<float> orders;
		List<UnityMolAtom> bonded = UnityMolMain.topologies.getBondedAtomsInResidue(a1, out orders);
		int i = 0;
		foreach (UnityMolAtom a in bonded) {
			if (a == a2) {
				res.order = orders[i];
				return res;
			}
			i++;
		}
		return res;
	}

	public List<UnityMolAtom> getAtomList(UnityMolModel m) {
		List<UnityMolAtom> res = new List<UnityMolAtom>(bonds.Count);
		foreach (int idA in bonds.Keys) {
			res.Add(m.allAtoms[idA]);
		}
		return res;
	}

	///Convert all atoms ids with new id, used when deleting atoms from the structure or split by chains
	public void convertIds(Dictionary<int, int> oldIdToNew, UnityMolModel m){
		Dictionary<int, int[]> savedbonds = bonds;
		bonds = new Dictionary<int, int[]>(savedbonds.Count);
		bondsCount = 0;
		foreach(int id in savedbonds.Keys){
			int newId = oldIdToNew[id];
			foreach(int id2 in savedbonds[id]){
				if(id2 != -1){
					int newId2 = oldIdToNew[id2];
					Add(newId, newId2, m);
				}
			}
		}
	}
}
}