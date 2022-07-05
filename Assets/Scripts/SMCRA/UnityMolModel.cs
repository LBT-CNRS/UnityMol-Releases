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
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace UMol {
/// <summary>
/// Part of the SMCRA data structure, UnityMolModel stores the chains of the structure
/// as a dictionary <string,UnityMolChain>.
/// It also stores the bonds as UnityMolBonds
/// A list of UnityMolAtom of the model is provided to loop over all atoms quickly
/// </summary>
public class UnityMolModel {
	/// <summary>
	/// Store all the chains of the model
	/// </summary>
	public UnityMolStructure structure;

	/// <summary>
	/// Store all the chains of the model
	/// </summary>
	public Dictionary<string, UnityMolChain> chains;

	/// <summary>
	/// Name of the model
	/// </summary>
	public string name;

	/// <summary>
	/// Bonds of the model, contains a dictionary of <UnityMolAtom, UnityMolAtom[]>
	/// </summary>
	public UnityMolBonds bonds;

	/// <summary>
	/// Saved bonds of the model
	/// </summary>
	public UnityMolBonds savedBonds;

	/// <summary>
	/// Bonds parsed with BondOrderParser, records only covalent bonds
	/// </summary>
	public Dictionary<AtomDuo, bondOrderType> covBondOrders;

	/// <summary>
	/// Stores a reference to all the atoms of the model
	/// </summary>
	public List<UnityMolAtom> allAtoms;

	private Vector3[] allPositions;

	/// <summary>
	/// Center of gravity of the model
	/// </summary>
	public Vector3 centerOfGravity;

	/// <summary>
	/// Maximum position in x, y and z
	/// </summary>
	public Vector3 maximumPositions;

	/// <summary>
	/// Minimum position in x, y and z
	/// </summary>
	public Vector3 minimumPositions;

	/// <summary>
	/// Custom chemical bonds read in a PDB file
	/// </summary>
	public List<Int2> customChemBonds = new List<Int2>();

	/// <summary>
	/// FieldLines Json file reader to be passed to UnityMolRepresentation
	/// </summary>
	public FieldLinesReader fieldLinesR;

	public int Count {
		get {return allAtoms.Count;}
	}

	/// <summary>
	/// UnityMolModel constructor taking chain dictionary as arg
	/// </summary>
	public UnityMolModel(Dictionary<string, UnityMolChain> dictChains, string nameModel) {
		chains = dictChains;
		name = nameModel;
		allAtoms = new List<UnityMolAtom>();
		foreach (UnityMolChain c in dictChains.Values) {
			c.model = this;
		}
	}

	/// <summary>
	/// UnityMolModel constructor taking chain list as arg,
	/// all the chains are inserted into the _chains dictionary
	/// </summary>
	public UnityMolModel(List<UnityMolChain> listChains, string nameModel) {
		chains = new Dictionary<string, UnityMolChain>();
		UnityMolChain outChain = null;
		for (int c = 0; c < listChains.Count; c++) {
			if (!chains.TryGetValue(listChains[c].name, out outChain)) {
				chains[listChains[c].name] = listChains[c];
			}
			else {
				outChain.AddResidues(listChains[c].residues);
			}
		}
		name = nameModel;
		allAtoms = new List<UnityMolAtom>();
		for (int c = 0; c < listChains.Count; c++) {
			listChains[c].model = this;
		}
	}

	/// <summary>
	/// UnityMolModel constructor taking one chain as arg,
	/// the chain is inserted into the chains dictionary
	/// </summary>
	public UnityMolModel(UnityMolChain newChain, string nameModel) {
		chains = new Dictionary<string, UnityMolChain>();
		chains[newChain.name] = newChain;
		name = nameModel;
		allAtoms = new List<UnityMolAtom>();
		newChain.model = this;
	}


	public void ComputeCenterOfGravity() {
		if (allAtoms.Count == 0) {
			centerOfGravity = Vector3.zero;
			return;
		}

		centerOfGravity = CenterOfGravBurst.computeCOG(allAtoms, ref minimumPositions, ref maximumPositions);

	}


	public static Vector3 ComputeCenterOfGravity(Vector3[] positions) {
		Vector3 dummymin = Vector3.zero;
		Vector3 dummymax = Vector3.zero;
		return CenterOfGravBurst.computeCOG(positions, ref dummymin, ref dummymax);
	}


	/// <summary>
	/// Fills UnityMol.position using UnityMolAtom.oriposition and the centerOfGravity computed with ComputeCenterOfGravity()
	/// </summary>
	public void CenterAtoms() {
		for (int i = 0; i < allAtoms.Count; i++) {
			allAtoms[i].position = allAtoms[i].oriPosition - centerOfGravity;
		}
	}

	/// <summary>
	/// Fills idInAllAtoms field
	/// </summary>
	public void fillIdAtoms() {
		for (int i = 0; i < allAtoms.Count; i++) {
			allAtoms[i].idInAllAtoms = i;
		}
	}

	public UnityMolAtom getAtomWithID(int idAtom) {
		foreach (UnityMolAtom a in allAtoms) {
			if (a.number == idAtom) {
				return a;
			}
		}
		return null;
	}

	/// Creates a new list of atoms
	public List<UnityMolAtom> ToAtomList() {
		return allAtoms.ToList();//Copy the list
	}


	public UnityMolSelection ToSelection() {
		List<UnityMolAtom> selectedAtoms = allAtoms;
		return new UnityMolSelection(selectedAtoms, bonds, name);
	}

	public bool hasHydrogens() {
		foreach (UnityMolAtom a in allAtoms) {
			if (a.type == "H") {
				return true;
			}
		}
		return false;
	}

	public Vector3[] getAllPositions() {
		if (allPositions == null) {
			allPositions = new Vector3[allAtoms.Count];
			int id = 0;
			foreach (UnityMolAtom a in allAtoms) {
				allPositions[id++] = a.position;
			}
		}
		return allPositions;
	}
}
}