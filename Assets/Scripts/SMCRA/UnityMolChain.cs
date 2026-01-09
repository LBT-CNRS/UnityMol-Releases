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
using System.Collections.Generic;
using System.Linq;

namespace UMol {
/// <summary>
/// Part of the SMCRA data structure, it stores the residues of the structure as a list of <see cref="UnityMolResidue"/>.
/// A reference to the <see cref="UnityMolModel"/> model it belongs is provided.
/// </summary>
public class UnityMolChain {

    /// <summary>
    /// Store all the residues of the chain based on their ids
    /// </summary>
    public List<UnityMolResidue> residues;

    /// <summary>
    /// Reference to the model the chain belongs to
    /// </summary>
    public UnityMolModel model;

    /// <summary>
    /// Name of the chain
    /// </summary>
    public string name;

    private int _count = -1;

    /// <summary>
    /// Construct a UnityMolChain with a list of UnityMolResidue and a name
    /// </summary>
    public UnityMolChain(List<UnityMolResidue> _residues, string _name) {
        residues = new List<UnityMolResidue>();
        AddResidues(_residues);
        name = _name;
    }

    /// <summary>
    /// Construct a UnityMolChain with one UnityMolResidue and a name
    /// </summary>
    public UnityMolChain(UnityMolResidue _residue, string _name) {
        residues = new List<UnityMolResidue> { _residue };
        name = _name;
    }

    /// <summary>
    /// Number of residues in the chain
    /// </summary>
    public int Count {
        get {
            if (_count < 0) {
                getCount();
            }
            return _count;
        }
    }

    /// <summary>
    /// Add a list of residues to the stored residues
    /// </summary>
    public void AddResidues(List<UnityMolResidue> newResidues) {
        foreach (UnityMolResidue r in newResidues) {
            residues.Add(r);
        }
        _count = -1;
    }

    /// <summary>
    /// Compute the number of residue and update the _count attribute
    /// </summary>
    private void getCount() {
        _count = 0;
        foreach (UnityMolResidue r in residues) {
            _count += r.Count;
        }
    }

    /// <summary>
    /// List of all UnityMolAtom of the chain
    /// </summary>
    public List<UnityMolAtom> AllAtoms => ToAtomList();

    /// <summary>
    /// Return a UnityMolAtom list containing all atoms of the chain
    /// </summary>
    /// <returns>the UnityMolAtom list</returns>
    public List<UnityMolAtom> ToAtomList() {
        List<UnityMolAtom> res = new();

        foreach (UnityMolResidue r in residues) {
            foreach (UnityMolAtom a in r.atoms.Values) {
                res.Add(a);
            }
        }
        return res;
    }

    /// <summary>
    /// Return a UnityMolSelection based on the atoms of the chain
    /// </summary>
    /// <param name="doBonds">Whether bonds are included in the selection</param>
    /// <returns>the new UnityMolSelection</returns>
    public UnityMolSelection ToSelection(bool doBonds = true) {
        List<UnityMolAtom> selectedAtoms = ToAtomList();
        string selectionMDA = ToSelectionMDA();

        if (doBonds) {
            return new UnityMolSelection(selectedAtoms, ToSelectionName(), selectionMDA);
        }
        return new UnityMolSelection(selectedAtoms, newBonds: null, ToSelectionName(), selectionMDA);
    }

    /// <summary>
    /// Return the MDAnalysis selection command of the chain as a string
    /// Ususally : "modelName and chain X"
    /// </summary>
    /// <returns>the string containing the selection command</returns>
    public string ToSelectionMDA() {
        return model.structure.name + " and chain " + name;
    }

    /// <summary>
    /// Generate a selection string name based on the structure, model and chain name
    /// </summary>
    /// <returns>the string containing the selection name</returns>
    public string ToSelectionName() {
        return model.structure.name + "_" + model.name + "_" + name;
    }

    //TODO: make this faster
    /// <summary>
    /// Return a UnityMolResidue based on the resid
    /// Return null if not found
    /// </summary>
    /// <param name="resid">the resid to look for</param>
    /// <returns>the UnityMolResidue. null if not found</returns>
    public UnityMolResidue GetResidueWithId(int resid) {
        return residues.FirstOrDefault(t => t.id == resid);
    }

    /// <summary>
    /// Clone a UnityMolChain by cloning residues and atoms
    /// </summary>
    public UnityMolChain Clone() {
        List<UnityMolResidue> clonedRes = new(residues.Count);

        foreach (UnityMolResidue r in residues) {
            UnityMolResidue newR = r.Clone();
            clonedRes.Add(newR);
        }

        UnityMolChain cloned = new(clonedRes, name);

        foreach (UnityMolResidue r in cloned.residues) {
            r.chain = cloned;
        }

        return cloned;
    }

}
}
