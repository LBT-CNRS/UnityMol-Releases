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

/// <summary>
/// Part of the SMCRA data structure, it stores the atoms of the structure
/// as a dictionary of <see cref="Dictionary{string, UnityMolAtom}" />
/// </summary>
public class UnityMolResidue {

    /// <summary>
    /// Store the reference to the chain it belongs to
    /// </summary>
    public UnityMolChain chain;

    /// <summary>
    /// Each residue has an id (resid)
    /// </summary>
    public int id;

    /// <summary>
    /// Residue number in the file (from 0 to N)
    /// </summary>
    public int resnum;

    /// <summary>
    /// Secondary structure is encoded in an enumeration
    /// </summary>
    public enum secondaryStructureType {
        Coil = 0, Helix = 1, Strand = 12, HelixRightOmega = 2, HelixRightPi = 3, HelixRightGamma = 4, Helix310 = 5,
        HelixLeftAlpha = 6, HelixLeftOmega = 7, HelixLeftGamma = 8, Helix27 = 9, PolyProline = 10,
        Turn = 16, StrandA = 17, Bridge = 18, Bend = 19, CoilA = 20
    }
    public static Dictionary<string, string> residueName3To1 = new() {
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

    public static Dictionary<string, string> residueName1To3 = new() {
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

    public static Dictionary<string, float> kdHydroDic = new() {
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
    public float kdHydro;

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

    public static Dictionary<string, string[][]> _rotLib;

    public static Dictionary<string, string[][]> rotLib {
        get {
            if (_rotLib == null) {
                _rotLib = FillRotamerLib();
            }
            return _rotLib;
        }
    }

    private UnityMolBonds resBonds;

    /// <summary>
    /// Global residue serial counter. Used for fast dictionnary access
    /// Each time a new UnityMolResidue is created, this will increase
    /// </summary>
    static int globResSerial;

    private readonly int serial;

    /// <summary>
    /// UnityMolResidue constructor taking a dictionary of atoms as arg
    /// </summary>
    public UnityMolResidue(int rnum, int idRes, Dictionary<string, UnityMolAtom> dictAtoms, string nameResidue) {
        resnum = rnum;
        id = idRes;
        atoms = dictAtoms;
        name = nameResidue;
        if (kdHydroDic.ContainsKey(name)) {
            kdHydro = kdHydroDic[name];
        }
        serial = globResSerial++;
    }

    /// <summary>
    /// UnityMolResidue constructor taking a list of atoms as arg,
    /// the atoms of the list are inserted in the atoms dictionary
    /// </summary>
    public UnityMolResidue(int rnum, int idRes, List<UnityMolAtom> listAtoms, string nameResidue) {
        resnum = rnum;
        id = idRes;
        atoms = new Dictionary<string, UnityMolAtom>();
        foreach (UnityMolAtom t in listAtoms)
        {
            if (!atoms.TryGetValue(t.name, out UnityMolAtom _)) {
                atoms[t.name] = t;
            }
        }
        name = nameResidue;
        if (kdHydroDic.TryGetValue(name, out float hydro)) {
            kdHydro = hydro;
        }
        serial = globResSerial++;
    }

    /// <summary>
    /// UnityMolResidue constructor taking a single atom as arg, it is inserted in the atoms dictionary
    /// </summary>
    public UnityMolResidue(int rnum, int idRes, UnityMolAtom newAtom, string nameRes) {
        resnum = rnum;
        id = idRes;
        atoms = new Dictionary<string, UnityMolAtom>();
        atoms[newAtom.name] = newAtom;
        name = nameRes;
        if (kdHydroDic.ContainsKey(name)) {
            kdHydro = kdHydroDic[name];
        }
        serial = globResSerial++;
    }

    public override string ToString() {
        return "Residue_" + name + "_" + id + "_" + chain.name;
    }

    public List<UnityMolAtom> ToAtomList() {
        List<UnityMolAtom> res = atoms.Values.ToList();
        return res;
    }

    public string GetResidueName3() {
        return name;
    }

    public string GetResidueName1() {
        return FromResidue3To1(name);
    }
    public string FromResidue3To1(string res3) {
        if (!residueName3To1.ContainsKey(res3)) {
            return res3;
        }
        return residueName3To1[res3];
    }
    public string FromResidue1To3(string res1) {
        if (!residueName1To3.ContainsKey(res1)) {
            throw new System.Exception("Undefinied 1 letter residue name '" + res1 + "'");
        }
        return residueName1To3[res1];
    }
    public UnityMolSelection ToSelection(bool doBonds = true) {
        List<UnityMolAtom> selectedAtoms = ToAtomList();
        string selectionMDA = ToSelectionMDA();

        if (doBonds) {
            return new UnityMolSelection(selectedAtoms, ToSelectionName(), selectionMDA);
        }
        return new UnityMolSelection(selectedAtoms, newBonds: null, ToSelectionName(), selectionMDA);
    }
    public string ToSelectionMDA() {
        return chain.model.structure.name +
               " and chain " + chain.name + " and resid " + id;
    }

    public string ToSelectionName() {
        return chain.model.structure.name + "_" + chain.model.name + "_" + chain.name + "_" + name + "_" + id;
    }

    public int Length {
        get {return atoms.Count();}
    }
    public int Count {
        get {return atoms.Count();}
    }
    private int lhash = -1;
    public int LightHashCode {
        get {
            if (lhash == -1) {
                computeLightHashCode();
            }
            return lhash;
        }
        set => lhash = value;
    }

    private void computeLightHashCode() {

        unchecked
        {
            const int seed = 1009;
            const int factor = 9176;
            lhash = seed;
            lhash = lhash * factor + chain.model.structure.name.GetHashCode();
            lhash = lhash * factor + chain.name.GetHashCode();
            lhash = lhash * factor + id;
        }
    }



    public override bool Equals(object obj) {
        UnityMolResidue r2 = obj as UnityMolResidue;
        UnityMolResidue r1 = this;
        if (r2 == null) { return false;}
        return r1.serial == r2.serial;
    }
    public override int GetHashCode() {
        return serial;
    }
    UnityMolAtom getChiAtom(int chiId, int atomId) {
        var curRotLib = rotLib;
        int cID = chiId - 1;
        if (curRotLib.ContainsKey(name) &&
                cID < curRotLib[name].Length &&
                atomId < curRotLib[name][cID].Length) {
            string[] chi1Names = curRotLib[name][cID];
            if (atomId < chi1Names.Length && atoms.ContainsKey(chi1Names[atomId])) {
                return atoms[chi1Names[atomId]];
            }
            //Alt VAL chi1

            if (atomId < chi1Names.Length && atomId == 3
                                          && cID == 0 && name == "VAL" && atoms.ContainsKey("CG2")) {
                return atoms["CG2"];
            }
            //Alt ASP chi2

            if (atomId < chi1Names.Length && atomId == 3
                                          && cID == 1 && name == "ASP" && atoms.ContainsKey("OD2")) {
                return atoms["OD2"];
            }
            //Alt LEU/PHE/TYR chi2

            if (atomId < chi1Names.Length && atomId == 3
                                          && cID == 1 && (name == "LEU" || name == "PHE" || name == "TYR") &&
                                          atoms.ContainsKey("CD2")) {
                return atoms["CD2"];
            }

        }
        return null;
    }


    ///Return chi torsion angle
    ///chiID ranges from 1 to 4
    public float GetChi(int chiId) {
        UnityMolAtom a1 = getChiAtom(chiId, 0);
        UnityMolAtom a2 = getChiAtom(chiId, 1);
        UnityMolAtom a3 = getChiAtom(chiId, 2);
        UnityMolAtom a4 = getChiAtom(chiId, 3);
        Vector3 p1 = (a1 != null ? a1.position : new Vector3(float.MinValue, 0.0f, 0.0f));
        Vector3 p2 = (a2 != null ? a2.position : new Vector3(float.MinValue, 0.0f, 0.0f));
        Vector3 p3 = (a3 != null ? a3.position : new Vector3(float.MinValue, 0.0f, 0.0f));
        Vector3 p4 = (a4 != null ? a4.position : new Vector3(float.MinValue, 0.0f, 0.0f));

        bool allFound = p1.x != float.MinValue && p2.x != float.MinValue &&
                        p3.x != float.MinValue && p4.x != float.MinValue;
        float dihe = 0.0f;

        if (allFound) {
            dihe = UnityMolAnnotationManager.dihedral(p1, p2, p3, p4);
        }

        return dihe;
    }

    static bool simpleSideChain(UnityMolAtom a) {

        if (a.type != "CA" && !a.isHET) {
            return !BackboneSelection.knownBases.Contains(a.name);
        }

        return false;
    }

    HashSet<UnityMolAtom> getAtomsToRotate(int chiId) {
        HashSet<UnityMolAtom> res = new();

        //Remove atoms
        UnityMolAtom a2 = getChiAtom(chiId, 1);
        UnityMolAtom a3 = getChiAtom(chiId, 2);

        Queue<UnityMolAtom> toLoop = new();
        toLoop.Enqueue(a3);
        while (toLoop.Any()) {
            UnityMolAtom curA = toLoop.Dequeue();
            res.Add(curA);
            int[] bonded;
            if (resBonds.bonds.TryGetValue(curA.idInAllAtoms, out bonded)) {
                foreach (int ida in bonded) {
                    if (ida != -1 && ida != curA.idInAllAtoms) {
                        UnityMolAtom a = curA.residue.chain.model.allAtoms[ida];
                        if (!res.Contains(a)) {
                            toLoop.Enqueue(a);
                            res.Add(a);
                        }
                    }
                }
            }
        }
        res.Remove(a3);

        return res;
    }

    public bool SetChi(int chiId, float newV) {

        if (resBonds == null) {
            UnityMolSelection tmpSel = ToSelection();
            resBonds = tmpSel.bonds;
        }
        UnityMolAtom a1 = getChiAtom(chiId, 0);
        UnityMolAtom a2 = getChiAtom(chiId, 1);
        UnityMolAtom a3 = getChiAtom(chiId, 2);
        UnityMolAtom a4 = getChiAtom(chiId, 3);
        Vector3 p1 = (a1 != null ? a1.position : new Vector3(float.MinValue, 0.0f, 0.0f));
        Vector3 p2 = (a2 != null ? a2.position : new Vector3(float.MinValue, 0.0f, 0.0f));
        Vector3 p3 = (a3 != null ? a3.position : new Vector3(float.MinValue, 0.0f, 0.0f));
        Vector3 p4 = (a4 != null ? a4.position : new Vector3(float.MinValue, 0.0f, 0.0f));

        bool allFound = p1.x != float.MinValue && p2.x != float.MinValue &&
                        p3.x != float.MinValue && p4.x != float.MinValue;

        if (allFound) {
            float dihe = UnityMolAnnotationManager.dihedral(p1, p2, p3, p4);

            //Rotate around p2-p3 axis
            Vector3 rotAxis = (a3.curWorldPosition - a2.curWorldPosition).normalized;

            Vector3 from = a3.curWorldPosition;

            HashSet<UnityMolAtom> toRotate = getAtomsToRotate(chiId);
            float angle = dihe - newV;

            foreach (UnityMolAtom a in toRotate) {
                Vector3 newP = RotatePointAroundPivot(a.position, a3.position, rotAxis, angle);
                a.position = newP;
            }

            chain.model.structure.updateRepresentations(trajectory: false);


            return true;
        }
        return false;
    }
    public static Vector3 RotatePointAroundPivot(Vector3 pos, Vector3 center, Vector3 axis, float angle) {

        Quaternion q = Quaternion.AngleAxis(angle, axis);
        Vector3 dif = pos - center;
        dif = q * dif;
        return center + dif;
    }

    public static Dictionary<string, string[][]> FillRotamerLib() {
        Dictionary<string, string[][]> newRotLib = new();

        newRotLib["ARG"] = new string[5][];
        newRotLib["ASN"] = new string[2][];
        newRotLib["ASP"] = new string[2][];
        newRotLib["CYS"] = new string[1][];
        newRotLib["GLN"] = new string[3][];
        newRotLib["GLU"] = new string[3][];
        newRotLib["HIS"] = new string[2][];
        newRotLib["ILE"] = new string[2][];
        newRotLib["LEU"] = new string[2][];
        newRotLib["LYS"] = new string[4][];
        newRotLib["MET"] = new string[3][];
        newRotLib["PHE"] = new string[2][];
        newRotLib["PRO"] = new string[2][];
        newRotLib["SER"] = new string[1][];
        newRotLib["THR"] = new string[1][];
        newRotLib["TRP"] = new string[2][];
        newRotLib["TYR"] = new string[2][];
        newRotLib["VAL"] = new string[1][];

        foreach (string k in newRotLib.Keys) {
            for (int i = 0; i < newRotLib[k].Length; i++) {
                newRotLib[k][i] = new string[4];
            }

            //CHI1
            newRotLib[k][0][0] = "N";
            newRotLib[k][0][1] = "CA";
            newRotLib[k][0][2] = "CB";
            newRotLib[k][0][3] = "CG";
            if (k == "CYS") {
                newRotLib[k][0][3] = "SG";
            }

            if (k is "ILE" or "VAL") {
                newRotLib[k][0][3] = "CG1";
            }

            if (k == "SER") {
                newRotLib[k][0][3] = "OG";
            }

            if (k == "THR") {
                newRotLib[k][0][3] = "OG1";
            }

            //CHI2
            if (newRotLib[k].Length > 1) {
                newRotLib[k][1][0] = "CA";
                newRotLib[k][1][1] = "CB";
                newRotLib[k][1][2] = "CG";
                newRotLib[k][1][3] = "CD";
                if (k == "ILE") {
                    newRotLib[k][1][2] = "CG1";
                }

                if (k is "ASN" or "ASP") {
                    newRotLib[k][1][3] = "OD1";
                }

                if (k == "HIS") {
                    newRotLib[k][1][3] = "ND1";
                }

                if (k is "ILE" or "LEU" or "PHE" or "TRP" or "TYR") {
                    newRotLib[k][1][3] = "CD1";
                }

                if (k == "MET") {
                    newRotLib[k][1][3] = "SD";
                }
            }
            //CHI3
            if (newRotLib[k].Length > 2) {
                newRotLib[k][2][0] = "CB";
                newRotLib[k][2][1] = "CG";
                newRotLib[k][2][2] = "CD";
                newRotLib[k][2][3] = "CE";

                if (k == "MET") {
                    newRotLib[k][2][2] = "SD";
                }

                if (k == "ARG") {
                    newRotLib[k][2][3] = "NE";
                }

                if (k == "GLN" || k == "GLU") {
                    newRotLib[k][2][3] = "OE1";
                }
            }
            //CHI4
            if (newRotLib[k].Length > 3) {
                newRotLib[k][3][0] = "CG";
                newRotLib[k][3][1] = "CD";
                newRotLib[k][3][2] = "NE";
                newRotLib[k][3][3] = "CZ";

                if (k == "LYS") {
                    newRotLib[k][3][2] = "CE";
                    newRotLib[k][3][3] = "NZ";
                }
            }

            //CHI5
            if (k == "ARG") {
                newRotLib[k][4][0] = "CD";
                newRotLib[k][4][1] = "NE";
                newRotLib[k][4][2] = "CZ";
                newRotLib[k][4][3] = "NH1";
            }
        }

        return newRotLib;
    }

    /// <summary>
    /// Clone a UnityMolResidue by cloning atoms
    /// </summary>
    public UnityMolResidue Clone() {
        List<UnityMolAtom> clonedAtoms = new(Count);

        foreach (UnityMolAtom a in atoms.Values) {
            var newA = a.Clone();
            clonedAtoms.Add(newA);
        }

        UnityMolResidue cloned = new(resnum, id, clonedAtoms, name) {
            kdHydro = kdHydro,
            secondaryStructure = secondaryStructure
        };

        foreach (UnityMolAtom a in cloned.atoms.Values) {
            a.residue = cloned;
        }

        return cloned;
    }

}

/// Class used in dictionary of residues to make sure the model of the residue is not taken into account
public class LightResidueComparer : IEqualityComparer<UnityMolResidue> {

    public bool Equals(UnityMolResidue x, UnityMolResidue y) {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;
        if (x.id != y.id) return false;
        if (x.name != y.name) return false;
        if (x.chain.name != y.chain.name) return false;
        if (x.chain.model.structure.name != y.chain.model.structure.name) return false;

        return true;
    }

    public int GetHashCode(UnityMolResidue r) {
        return r.LightHashCode;
    }
}

}
