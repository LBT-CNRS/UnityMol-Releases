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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
	/// Stores the bonds of the model, empty bonds are null
	/// </summary>
	public Dictionary<UnityMolAtom, UnityMolAtom[]> bonds;

	/// <summary>
	/// Stores the bonds of the model, empty bonds are null, contains both A1+A2 and A2+A1
	/// </summary>
	public Dictionary<UnityMolAtom, UnityMolAtom[]> bondsDual;


	/// <summary>
	/// Total number of bonds stored
	/// </summary>
	public int bondsCount = 0;


	/// <summary>
	/// UnityMolBonds constructor initializing the dictionary
	/// </summary>
	public UnityMolBonds() {
		bonds = new Dictionary<UnityMolAtom, UnityMolAtom[]>();
		bondsDual = new Dictionary<UnityMolAtom, UnityMolAtom[]>();
		bondsCount = 0;
	}

	/// <summary>
	/// Returns the number of bonds stored
	/// </summary>
	public int Count {
		get { return bondsCount; }
	}

	public int Length {
		get { return bondsCount; }
	}

	public Dictionary<UnityMolAtom, UnityMolAtom[]> Dbonds {
		get { return bonds; }
	}

	/// <summary>
	/// Add an array of atoms in the dictionary
	/// </summary>
	public void Add(UnityMolAtom atom, UnityMolAtom[] bonded) {
		for (int i = 0; i < bonded.Length; i++) {
			Add(atom, bonded[i]);
		}
	}

	/// <summary>
	/// Add a UnityMolAtom in the dictionary, prints a Debug.LogError when no more space is available
	/// </summary>
	public void Add(UnityMolAtom atom, UnityMolAtom bonded) {
		UnityMolAtom[] res = null;
		if (bonds.TryGetValue(atom, out res)) {
			bool added = false;
			for (int i = 0; i < NBBONDS; i++) {
				if (res[i] != null && res[i] == bonded) {
					added = true;
					break;
				}

				if (res[i] == null) {
					res[i] = bonded;
					added = true;
					bondsCount++;
					break;
				}
			}
			if (!added) {
				Debug.LogError("More than " + NBBONDS + " bonds for atom " + atom);
			}
		}
		// else if (!isBondedTo(atom, bonded)) {
		else {
			bonds[atom] = new UnityMolAtom[NBBONDS];
			for (int i = 0; i < NBBONDS; i++)
				bonds[atom][i] = null;
			bonds[atom][0] = bonded;
			bondsCount++;

		}
		AddDual(atom, bonded);
		AddDual(bonded, atom);
	}

	private void AddDual(UnityMolAtom atom, UnityMolAtom bonded) {
		UnityMolAtom[] res = null;
		if (bondsDual.TryGetValue(atom, out res)) {
			bool added = false;
			for (int i = 0; i < NBBONDS; i++) {
				if (res[i] != null && res[i] == bonded) {
					added = true;
					break;
				}

				if (res[i] == null) {
					res[i] = bonded;
					added = true;
					break;
				}
			}
			if (!added) {
				Debug.LogError("More than " + NBBONDS + " bonds for atom " + atom);
			}
		} else {
			bondsDual[atom] = new UnityMolAtom[NBBONDS];
			for (int i = 0; i < NBBONDS; i++)
				bondsDual[atom][i] = null;
			bondsDual[atom][0] = bonded;
		}
	}

	/// <summary>
	/// Returns the number of atoms bonded to the atom A
	/// </summary>
	public int countBondedAtoms(UnityMolAtom a) {
		try {
			UnityMolAtom[] res = bondsDual[a];
			int count = 0;
			for (int i = 0; i < NBBONDS; i++) {
				if (res[i] != null) {
					count++;
				}
			}
			return count;
		}
		catch {
			// Debug.LogWarning("Did not find atom "+a+" in bonds");
			return -1;
		}
	}

	/// <summary>
	/// Check if atom a1 and atom a2 are bonded
	/// </summary>
	public bool isBondedTo(UnityMolAtom a1, UnityMolAtom a2) {
		UnityMolAtom[] res = null;
		if (bondsDual.TryGetValue(a1, out res)) {
			for (int i = 0; i < NBBONDS; i++) {
				if (res[i] != null && res[i] == a2) {
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
		if(m.covBondOrders != null){
			if (m.covBondOrders.ContainsKey(d)) {
				return m.covBondOrders[d];
			}
			else if (m.covBondOrders.ContainsKey(dinv)) {
				return m.covBondOrders[dinv];
			}
		}

		if(useProtDef){
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

	/// <summary>
	/// Return a flatten list of atom couples (not from the dual dictionary)
	/// </summary>
	public List<AtomDuo> ToList() {
		List<AtomDuo> result = new List<AtomDuo>();

		foreach (UnityMolAtom a in bonds.Keys) {
			foreach (UnityMolAtom a2 in bonds[a]) {
				if (a2 != null) {
					result.Add(new AtomDuo(a, a2));
				}
			}
		}

		return result;
	}

	/// <summary>
	/// Return a flatten list of atom couples bidirectional
	/// </summary>
	public List<AtomDuo> ToListDual() {
		List<AtomDuo> result = new List<AtomDuo>();

		foreach (UnityMolAtom a in bonds.Keys) {
			foreach (UnityMolAtom a2 in bonds[a]) {
				if (a2 != null) {
					result.Add(new AtomDuo(a, a2));
					result.Add(new AtomDuo(a2, a));
				}
			}
		}

		return result;
	}

}
}