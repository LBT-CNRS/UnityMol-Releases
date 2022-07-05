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
/// <summary>
/// Part of the SMCRA data structure, UnityMolChain stores the residues of the structure as a list of UnityMolResidue
/// A reference to the model it belongs is provided
/// </summary>
public class UnityMolChain {

	/// <summary>
	/// Store all the residues of the chain based on their ids
	/// </summary>
	public Dictionary<int, UnityMolResidue> residues;

	/// <summary>
	/// Reference to the model the chain belongs to
	/// </summary>
	public UnityMolModel model;

	/// <summary>
	/// Name of the chain
	/// </summary>
	public string name;


	/// <summary>
	/// UnityMolChain constructor taking a list of residues as arg
	/// </summary>
	public UnityMolChain(List<UnityMolResidue> _residues, string _name) {
		residues = new Dictionary<int, UnityMolResidue>();
		AddResidues(_residues);
		name = _name;
	}

	/// <summary>
	/// UnityMolChain constructor taking a residue as arg
	/// </summary>
	public UnityMolChain(UnityMolResidue _residue, string _name) {
		residues = new Dictionary<int, UnityMolResidue>();
		residues[_residue.id] = _residue;
		name = _name;
	}

	/// <summary>
	/// Add a list of residues to the stored residues
	/// </summary>
	public void AddResidues(List<UnityMolResidue> newResidues) {
		foreach (UnityMolResidue r in newResidues) {
			residues[r.id] = r;
		}
	}

	/// <summary>
	/// Add a dictionary of residues to the stored residues
	/// </summary>
	public void AddResidues(Dictionary<int, UnityMolResidue> newResidues) {
		foreach (UnityMolResidue r in newResidues.Values) {
			residues[r.id] = r;
		}
	}

	public List<UnityMolAtom> allAtoms {
		get { return ToAtomList(); }
	}

	public List<UnityMolAtom> ToAtomList() {
		List<UnityMolAtom> res = new List<UnityMolAtom>();

		foreach (UnityMolResidue r in residues.Values) {
			// res.AddRange(r.allAtoms);
			foreach (UnityMolAtom a in r.atoms.Values) {
				res.Add(a);
			}
		}
		return res;
	}

	public UnityMolSelection ToSelection(bool doBonds = true) {
		List<UnityMolAtom> selectedAtoms = ToAtomList();
		string selectionMDA = model.structure.uniqueName + " and chain " + name;

		if (doBonds) {
			return new UnityMolSelection(selectedAtoms, name, selectionMDA);
		}
		return new UnityMolSelection(selectedAtoms, newBonds: null, name, selectionMDA);
	}
}
}