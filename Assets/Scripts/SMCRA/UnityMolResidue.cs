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
/// Part of the SMCRA data structure, UnityMolResidue stores the atoms of the structure as a dictionary <string,UnityMolAtom>
/// </summary>
public class UnityMolResidue {

    /// <summary>
    /// Store the reference to the chain it belongs to
    /// </summary>
    public UnityMolChain chain;

    /// <summary>
    /// Each residue has an id
    /// </summary>
    public int id = -1;

    /// <summary>
    /// Secondary structure is encoded in an enumeration
    /// </summary>
    public enum secondaryStructureType {
        Coil = 0, Helix = 1, Strand = 12, HelixRightOmega = 2, HelixRightPi = 3, HelixRightGamma = 4, Helix310 = 5,
        HelixLeftAlpha = 6, HelixLeftOmega = 7, HelixLeftGamma = 8, Helix27 = 9, PolyProline = 10,
        Turn = 16, StrandA = 17, Bridge = 18, Bend = 19, CoilA = 20
    }
    public static Dictionary<string, string> residueName3To1 = new Dictionary<string, string>
    {
        { "ALA", "A" },
        { "ARG", "R" },
        { "ASN", "N" },
        { "ASP", "D" },
        { "ASX", "B" },
        { "CYS", "C" },
        { "GLU", "E" },
        { "GLN", "Q" },
        { "GLX", "Z" },
        { "GLY", "G" },
        { "HIS", "H" },
        { "ILE", "I" },
        { "LEU", "L" },
        { "LYS", "K" },
        { "MET", "M" },
        { "PHE", "F" },
        { "PRO", "P" },
        { "SER", "S" },
        { "THR", "T" },
        { "TRP", "W" },
        { "TYR", "Y" },
        { "VAL", "V" }
    };

    public static Dictionary<string, string> residueName1To3 = new Dictionary<string, string>
    {
        { "A", "ALA" },
        { "R", "ARG" },
        { "N", "ASN" },
        { "D", "ASP" },
        { "B", "ASX" },
        { "C", "CYS" },
        { "E", "GLU" },
        { "Q", "GLN" },
        { "Z", "GLX" },
        { "G", "GLY" },
        { "H", "HIS" },
        { "I", "ILE" },
        { "L", "LEU" },
        { "K", "LYS" },
        { "M", "MET" },
        { "F", "PHE" },
        { "P", "PRO" },
        { "S", "SER" },
        { "T", "THR" },
        { "W", "TRP" },
        { "Y", "TYR" },
        { "V", "VAL" }
    };

    public static Dictionary<string, float> kdHydroDic = new Dictionary<string, float> {
        { "ALA", 1.8f },
        { "ARG", -4.5f },
        { "ASN", -3.5f },
        { "ASP", -3.5f },
        { "ASX", -3.5f },
        { "CYS", 2.5f },
        { "GLU", -3.5f },
        { "GLN", -3.5f },
        { "GLX", -3.5f },
        { "GLY", -0.4f },
        { "HIS", -3.2f },
        { "ILE", 4.5f },
        { "LEU", 3.8f },
        { "LYS", -3.9f },
        { "MET", 1.9f },
        { "PHE", 2.8f },
        { "PRO", -1.6f },
        { "SER", -0.8f },
        { "THR", -0.7f },
        { "TRP", -0.9f },
        { "TYR", -1.3f },
        { "VAL", 4.2f }
    };

    /// <summary>
    /// Hydrophobicity of the residue (from Kyte and Doolittle, https://www.cgl.ucsf.edu/chimera/current/docs/UsersGuide/midas/hydrophob.html)
    /// </summary>
    public float kdHydro = 0.0f;

    /// <summary>
    /// Type of secondary structure
    /// </summary>
    public secondaryStructureType secondaryStructure;

    /// <summary>
    /// Store all the atoms of the residue
    /// </summary>
    public Dictionary<string, UnityMolAtom> atoms;

    /// <summary>
    /// Name of the residue
    /// </summary>
    public string name;



    public List<UnityMolAtom> allAtoms {
        get { return ToAtomList(); }
    }

    /// <summary>
    /// UnityMolResidue constructor taking a dictionary of atoms as arg
    /// </summary>
    public UnityMolResidue(int idRes, Dictionary<string, UnityMolAtom> dictAtoms, string nameResidue) {
        id = idRes;
        atoms = dictAtoms;
        name = nameResidue;
        if (kdHydroDic.ContainsKey(name)) {
            kdHydro = kdHydroDic[name];
        }
    }

    /// <summary>
    /// UnityMolResidue constructor taking a list of atoms as arg,
    /// the atoms of the list are inserted in the atoms dictionary
    /// </summary>
    public UnityMolResidue(int idRes, List<UnityMolAtom> listAtoms, string nameResidue) {
        id = idRes;
        atoms = new Dictionary<string, UnityMolAtom>();
        UnityMolAtom outAtom = null;
        for (int i = 0; i < listAtoms.Count; i++) {
            if (!atoms.TryGetValue(listAtoms[i].name, out outAtom)) {
                atoms[listAtoms[i].name] = listAtoms[i];
            }
            // else
            // Debug.Log("Warning: An atom with the same name already exists in this residue");

        }
        name = nameResidue;
        if (kdHydroDic.ContainsKey(name)) {
            kdHydro = kdHydroDic[name];
        }
    }

    /// <summary>
    /// UnityMolResidue constructor taking a single atom as arg, it is inserted in the atoms dictionary
    /// </summary>
    public UnityMolResidue(int idRes, UnityMolAtom newAtom, string nameRes) {
        id = idRes;
        atoms = new Dictionary<string, UnityMolAtom>();
        atoms[newAtom.name] = newAtom;
        name = nameRes;
        if (kdHydroDic.ContainsKey(name)) {
            kdHydro = kdHydroDic[name];
        }
    }

    public override string ToString() {
        return "Residue_" + id + " of chain " + chain.name;
    }

    public List<UnityMolAtom> ToAtomList() {
        List<UnityMolAtom> res = atoms.Values.ToList();
        return res;
    }

    public string getResidueName3() {
        return name;
    }

    public string getResidueName1() {
        return fromResidue3To1(name);
    }
    public string fromResidue3To1(string res3) {
        if (!residueName3To1.ContainsKey(res3)) {
            throw new System.Exception("Undefinied 3 letters residue name '" + res3 + "'");
        }
        return residueName3To1[res3];
    }
    public string fromResidue1To3(string res1) {
        if (!residueName1To3.ContainsKey(res1)) {
            throw new System.Exception("Undefinied 1 letter residue name '" + res1 + "'");
        }
        return residueName1To3[res1];
    }
    public UnityMolSelection ToSelection(bool doBonds = true) {
        List<UnityMolAtom> selectedAtoms = ToAtomList();
        string selectionMDA = chain.model.structure.uniqueName +
                              " and chain " + chain.name + " and resid " + id;

        if (doBonds) {
            return new UnityMolSelection(selectedAtoms, name, selectionMDA);
        }
        return new UnityMolSelection(selectedAtoms, newBonds: null, name, selectionMDA);
    }
    public int Length {
        get {return atoms.Count();}
    }
    public int Count {
        get {return atoms.Count();}
    }
    private int _lhash = -1;
    public int lightHashCode {
        get{
            if(_lhash == -1){
                computeLightHashCode();
            }
            return _lhash;
        }
        set{
            _lhash = value;
        }
    }

    void computeLightHashCode() {

        unchecked
        {
            const int seed = 1009;
            const int factor = 9176;
            _lhash = seed;
            _lhash = _lhash * factor + chain.model.structure.uniqueName.GetHashCode();
            _lhash = _lhash * factor + chain.name.GetHashCode();
            _lhash = _lhash * factor + id;
        }
    }

    // public override bool Equals(object obj) {
    //     UnityMolResidue r2 = obj as UnityMolResidue;
    //     UnityMolResidue r1 = this;
    //     if (r1 == null && r2 == null) { return true;}
    //     if (r1 == null || r2 == null) { return false;}
    //     return r1.lightHashCode == r2.lightHashCode;
    // }
    // public override int GetHashCode() {
    //     return lightHashCode;
    // }
}

/// Class used in dictionary of residues to make sure the model of the residue is not taken into account
public class LightResidueComparer : IEqualityComparer<UnityMolResidue> {

    public bool Equals(UnityMolResidue x, UnityMolResidue y) {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;
        if (x.id != y.id) return false;
        if (x.name != y.name) return false;
        if (x.chain.name != y.chain.name) return false;
        if (x.chain.model.structure.uniqueName != y.chain.model.structure.uniqueName) return false;

        return true;
    }

    public int GetHashCode(UnityMolResidue r) {
        return r.lightHashCode;
    }
}

}