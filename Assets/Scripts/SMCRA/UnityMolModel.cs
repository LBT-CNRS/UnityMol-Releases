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
using System.Linq;
using System.Collections.Generic;
using Unity.Mathematics;

namespace UMol {
/// <summary>
/// Part of the SMCRA data structure, UnityMolModel stores the chains of the structure
/// as a dictionary <see cref="Dictionary{string, UnityMolChain}" />.
/// It also stores the bonds as UnityMolBonds
/// A list of <see cref="UnityMolAtom"/> of the model is provided to loop over all atoms quickly.
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
	/// Bonds of the model, contains a dictionary of <UnityMolAtom UnityMolAtom[]/>
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

	/// <summary>
	/// Array of coordinates of all atoms of the model.
	/// </summary>
	private Vector3[] allPositions;

	/// <summary>
	/// Center of gravity of the model
	/// </summary>
	public Vector3 centroid;

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
	public List<int2> customChemBonds = new List<int2>();

	/// <summary>
	/// FieldLines Json file reader to be passed to UnityMolRepresentation
	/// </summary>
	public FieldLinesReader fieldLinesR;

	private Dictionary<long, int> _atomIdToIndex;

	/// <summary>
	/// Mapping between the atom id/serial/number and its
	/// index in the global atom list allAtoms.
	/// </summary>
	public Dictionary<long, int> atomIdToIndex {
		get {
            if (_atomIdToIndex != null) {
                return _atomIdToIndex;
            }
            _atomIdToIndex = new Dictionary<long, int>(allAtoms.Count);
            for (int i = 0; i < allAtoms.Count; i++) {
                _atomIdToIndex[allAtoms[i].number] = i;
            }
            return _atomIdToIndex;
		}
	}

	/// <summary>
	/// Number of atoms in the model
	/// </summary>
	public int Count => allAtoms.Count;

    /// <summary>
	/// Construct a UnityMolModel with a dictionnary of chains and a name
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
	/// Construct a UnityMolModel with a list of chains
	/// all the chains are inserted into the _chains dictionary
	/// </summary>
	public UnityMolModel(List<UnityMolChain> listChains, string nameModel) {

		chains = new Dictionary<string, UnityMolChain>();
        foreach (UnityMolChain t in listChains)
        {
            if (!chains.TryGetValue(t.name, out UnityMolChain outChain)) {
                chains[t.name] = t;
            }
            else {
                outChain.AddResidues(t.residues);
            }
        }
		name = nameModel;
		allAtoms = new List<UnityMolAtom>();
		foreach (UnityMolChain t in listChains)
        {
            t.model = this;
        }
		foreach (UnityMolChain c in chains.Values) {
			foreach (UnityMolResidue r in c.residues) {
				r.chain = c;
			}
		}
	}

	/// <summary>
	/// Construct a UnityMolModel with one UnityMolChain and a name
	/// the chain is inserted into the chains dictionary
	/// </summary>
	public UnityMolModel(UnityMolChain newChain, string nameModel) {
		chains = new Dictionary<string, UnityMolChain> {
            [newChain.name] = newChain
        };
        name = nameModel;
		allAtoms = new List<UnityMolAtom>();
		newChain.model = this;
	}

	/// <summary>
	/// Return all UnityMolChain as a list
	/// </summary>
	public List<UnityMolChain> GetChains() {
		return chains.Values.ToList();
	}

	/// <summary>
	/// Compute the centroid of the model
	/// </summary>
	public void ComputeCentroid() {
		if (allAtoms.Count == 0) {
			centroid = Vector3.zero;
			return;
		}

		centroid = CenterOfGravBurst.computeCOG(allAtoms, ref minimumPositions, ref maximumPositions);
	}


	/// <summary>
	/// Compute the centroid of an array of coordinates
	/// </summary>
	/// <param name="positions">the array of coordiantes</param>
	/// <returns>the centroid</returns>
	public static Vector3 ComputeCentroid(Vector3[] positions) {
		Vector3 dummymin = Vector3.zero;
		Vector3 dummymax = Vector3.zero;
		return CenterOfGravBurst.computeCOG(positions, ref dummymin, ref dummymax);
	}


	/// <summary>
	/// Center all atoms coordinates of the model based on the centroid
	/// Fills UnityMol.position using UnityMolAtom.oriposition and the centroid computed with ComputeCentroid()
	/// </summary>
	public void CenterAtoms() {
        foreach (UnityMolAtom t in allAtoms)
        {
            t.position = t.oriPosition - centroid;
        }
    }

	/// <summary>
	/// Fills idInAllAtoms field
	/// </summary>
	public void fillIdAtoms() {
		for (int i = 0; i < allAtoms.Count; i++) {
			allAtoms[i].idInAllAtoms = i;
		}
		_atomIdToIndex = null;//Force update of atomId to index
	}

	/// <summary>
	/// Return a UnityMolAtom based on its id
	/// </summary>
	/// <param name="idAtom"> the id of the atom</param>
	/// <returns>the UnityMolAtom. null if not found</returns>
	public UnityMolAtom getAtomWithID(int idAtom) {
        if (atomIdToIndex.TryGetValue(idAtom, out int res)) {
			return allAtoms[res];
		}
		return null;
	}

	/// <summary>
	/// Return a copy of the allAtoms list
	/// </summary>
	/// <returns>the copy</returns>
	public List<UnityMolAtom> ToAtomList() {
		return allAtoms.ToList();//Copy the list
	}

	/// <summary>
	/// Return a UnityMolSelection of all atoms of the model
	/// </summary>
	/// <returns>the UnityMolSelection</returns>
	public UnityMolSelection ToSelection() {
		List<UnityMolAtom> selectedAtoms = allAtoms;
		return new UnityMolSelection(selectedAtoms, bonds, ToSelectionName());
	}

    /// <summary>
    /// Generate a selection string name based on the structure and model name
    /// </summary>
    /// <returns>the string containing the selection name</returns>
	public string ToSelectionName() {
		return structure.name + "_" + name;
	}

	/// <summary>
	/// Check wether the model has hydrogens
	/// Return true if at least one atom is an hydrogen
	/// </summary>
	/// <returns>true if model has hydrogens. False Otherwise</returns>
	public bool HasHydrogens() {
        return allAtoms.Any(a => a.type == "H");
    }



	/// <summary>
	/// Clone a UnityMolModel by cloning chains, residues and atoms
	/// if newName is not set, the name will be nameModel_cloned
	/// </summary>
	/// <param name="newName">new name of the model</param>
	/// <returns>the cloned model</returns>
	public UnityMolModel Clone(string newName = "") {
		List<UnityMolChain> clonedChains = new(chains.Count);
		List<UnityMolAtom> newAllAtoms = new(Count);
		foreach (UnityMolChain c in chains.Values) {
			UnityMolChain newC = c.Clone();
			clonedChains.Add(newC);
			newAllAtoms.AddRange(newC.AllAtoms);
		}

		if (string.IsNullOrEmpty(newName)) {
			newName = name + "_cloned";
		}

		UnityMolModel cloned = new(clonedChains, newName);

		if (bonds != null) {
			cloned.bonds = new UnityMolBonds {
                bonds = new Dictionary<int, int[]>(bonds.bonds),
                bondsCount = bonds.bondsCount
            };
        }


		if (savedBonds != null) {
			cloned.savedBonds = new UnityMolBonds {
                bonds = new Dictionary<int, int[]>(bonds.bonds),
                bondsCount = bonds.bondsCount
            };
        }
		if (covBondOrders != null) {
			covBondOrders = new Dictionary<AtomDuo, bondOrderType>(covBondOrders);
		}

		cloned.allAtoms = newAllAtoms;

		foreach (UnityMolChain c in cloned.chains.Values) {
			c.model = cloned;
		}

		return cloned;
	}

	/// <summary>
	/// Renumber all UnityMolAtoms number starting from the value startingNumber (0 by default).
	/// </summary>
	/// <param name="startingNumber">the value to start renumbering</param>
	public void UpdateAtomNumber(long startingNumber = 0)
	{
		for (int i = 0; i < allAtoms.Count; i++)
		{
			allAtoms[i].number = i + startingNumber;
		}
	}
}
}
