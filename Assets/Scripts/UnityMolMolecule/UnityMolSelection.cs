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
using System.Text;
using Unity.Mathematics;

namespace UMol {
public class UnityMolSelection {

    /// If the selection can be modified.
    public bool isAlterable = true;

    ///This is used when setting a custom UnityMolBonds selection
    public bool canUpdateBonds = true;

    ///Selection content updates each frame of the trajectory
    public bool updateContentWithTraj = false;
    ///Representations of the selection updates each frame of the trajectory
    public bool updateRepWithTraj = true;

    /// Selection created by the trajectory frame picker
    public bool extractTrajFrame = false;

    /// List of id frames from which the selection was made.
    public List<int> extractTrajFrameIds;

    /// List of Atoms positions per frame extracted
    public List<Vector3[]> extractTrajFramePositions;

    /// Mapping between the atoms and the id in the selection
    public Dictionary<UnityMolAtom, int> _atomToIdInSel;

    /// Mapping between the atom and the id in the selection
    public Dictionary<UnityMolAtom, int> atomToIdInSel {
        get {
            if (_atomToIdInSel == null || _atomToIdInSel.Count != atoms.Count) {
                _atomToIdInSel = new Dictionary<UnityMolAtom, int>();
                for (int i = 0; i < atoms.Count; i++) {
                    _atomToIdInSel[atoms[i]] = i;
                }
            }
            return _atomToIdInSel;
        }
    }

    /// List of representations associated with the selection based on the type of representations.
    public Dictionary<RepType, List<UnityMolRepresentation>> representations;

    ///Atoms of the selection
    List<UnityMolAtom> _atoms;
    ///Atoms of the selection
    public List<UnityMolAtom> atoms {
        get {return _atoms;}
        set {
            if (!isAlterable) {
                Debug.LogWarning("Selection " + name + " is not alterable");
            }
            else {
                _atoms = value;
            }
        }
    }

    /// Name of the selection
    string _name;
    /// Name of the selection
    public string name {
        get {return _name;}
        set {
            if (!isAlterable) {
                Debug.LogWarning("This selection is not alterable");
            }
            else {
                string tmp = value.Replace(" ", "_");
                _name = tmp;
            }
        }
    }

    /// Number of heavy atoms in the selection.
    public int CountHeavyAtoms() {
        int heavy = 0;
        for (int i = 0; i < atoms.Count; i++)
        {
            if (atoms[i].type != "H") {
                heavy++;
            }
        }
        if (extractTrajFrame) {
            return extractTrajFramePositions.Count * heavy;
        }
        return heavy;
    }

    /// Bonds in the selection
    UnityMolBonds _bonds;
    /// Bonds in the selection
    public UnityMolBonds bonds {
        get {
            if (_bonds == null)
                fillBonds();
            return _bonds;
        }
        set {
            if (canUpdateBonds)
                _bonds = value;
        }
    }

    /// Return true if the bonds have not been filled.
    public bool bondsNull {
        get {
            return _bonds == null;
        }
    }

    /// Structures associated with the selection
    public List<UnityMolStructure> structures;

    /// Did this selection was created from the Selection langage?
    public bool fromSelectionLanguage = false;

    /// String of the selection langage
    public string MDASelString = "";

    ///Force the selection to be global.
    public bool forceGlobalSelection =  false;

    /// Centroid of the selection
    public Vector3 centroid {
        get {
            return ManipulationManager.computeCentroidSel(this);
        }
    }

    /// Minimal Position of the selection (made by the smallest components of all atoms coordinates)
    private Vector3 _minPos = Vector3.one * float.MinValue;
    /// Minimal Position of the selection (made by the smallest components of all atoms coordinates)
    public Vector3 minPos {
        get {
            if (_minPos.x == float.MinValue)
                computeMinMaxPos();
            return _minPos;
        }
    }

    /// Maximal position of the selection (made by the largest components of all atoms coordinates)
    private Vector3 _maxPos = Vector3.one * float.MaxValue;
    /// Maximal position of the selection (made by the largest components of all atoms coordinates)
    public Vector3 maxPos {
        get {
            if (_maxPos.x == float.MaxValue)
                computeMinMaxPos();
            return _maxPos;
        }
    }

    /// Number of atoms in the selection.
    public int Count {
        get {
            if (extractTrajFrame) {
                return extractTrajFramePositions.Count * atoms.Count;
            }
            return atoms.Count;
        }
    }

    public UnityMolSelection(List<UnityMolAtom> newAtoms, UnityMolBonds newBonds, string nameSelection, string MDASele = "") {
        atoms = newAtoms;
        _bonds = newBonds;
        name = nameSelection;
        representations = new Dictionary<RepType, List<UnityMolRepresentation>>();
        fillStructures();

        if (MDASele == "") {
            fromSelectionLanguage = false;
        }
        else {
            MDASelString = MDASele;
            fromSelectionLanguage = true;
        }

    }
    public UnityMolSelection(List<UnityMolAtom> newAtoms, string nameSelection, string MDASele = "") {
        atoms = newAtoms;
        _bonds = null;

        name = nameSelection;
        representations = new Dictionary<RepType, List<UnityMolRepresentation>>();
        fillStructures();

        if (MDASele == "") {
            fromSelectionLanguage = false;
        }
        else {
            MDASelString = MDASele;
            fromSelectionLanguage = true;
        }

    }
    public UnityMolSelection(UnityMolAtom atom, string nameSelection) {
        atoms = new List<UnityMolAtom>();
        atoms.Add(atom);
        _bonds = null;
        name = nameSelection;
        representations = new Dictionary<RepType, List<UnityMolRepresentation>>();
        fillStructures();
    }

    public UnityMolSelection(UnityMolAtom atom, UnityMolBonds newBonds, string nameSelection) {
        atoms = new List<UnityMolAtom>();
        atoms.Add(atom);
        _bonds = newBonds;
        name = nameSelection;
        representations = new Dictionary<RepType, List<UnityMolRepresentation>>();
        fillStructures();
    }


    /// Retrieve bonds from the atoms in the selection. Based on the model of the atoms.
    public void fillBonds() {
        UnityMolBonds recoveredBonds = new UnityMolBonds();
        HashSet<int> atomsHS = new HashSet<int>();
        foreach (UnityMolAtom a in atoms) {
            atomsHS.Add(a.idInAllAtoms);
        }

        if (atoms.Count > 0)
            recoveredBonds.NBBONDS = atoms[0].residue.chain.model.bonds.NBBONDS;

        int[] bonded = null;
        foreach (UnityMolAtom a in atoms) {
            int idA = a.idInAllAtoms;
            UnityMolModel curM = a.residue.chain.model;
            if (curM.bonds.bonds.TryGetValue(idA, out bonded)) {
                foreach (int ia2 in bonded) {
                    if (ia2 != -1 && atomsHS.Contains(ia2) && ia2 != idA) {
                        recoveredBonds.Add(idA, ia2, curM);
                    }
                }
            }
        }

        _bonds = recoveredBonds;
    }

    /// Returns a formatted string for this selection.
    public override string ToString() {
        if (_bonds != null) {
            if (fromSelectionLanguage) {
                return "Selection of " + atoms.Count + " atoms / " + _bonds.Count + " bonds, named '" + name + "' (" + MDASelString + ")";
            }
            return "Selection of " + atoms.Count + " atoms / " + _bonds.Count + " bonds, named '" + name + "'";
        }
        if (fromSelectionLanguage) {
            return "Selection of " + atoms.Count + " atoms, named '" + name + "' (" + MDASelString + ")";
        }
        return "Selection of " + atoms.Count + " atoms, named '" + name + "'";

    }

    /// Does the selection involve several structures?
    public bool isGlobalSelection() {
        if (structures == null) {
            fillStructures();
        }
        return structures.Count > 1;
    }

    /// Retrieve structures from the atoms in the selection. Based on the model of the atoms.
    public void fillStructures() {
        if (atoms == null) {
            return;
        }
        HashSet<UnityMolStructure> tmpS = new HashSet<UnityMolStructure>();
        foreach (UnityMolAtom a in atoms) {
            tmpS.Add(a.residue.chain.model.structure);
        }
        structures = tmpS.ToList();
    }


    /// Output a string containting the content of the selection using the selection language.

    /// When force is true, even if the selection was created with a MDASelection (fromSelectionLanguage = True), it loops over all atoms to create a selection command
    public string ToSelectionCommand(bool force = false) {

        if (!force && fromSelectionLanguage) {
            return MDASelString;
        }
        if (Count == 0) {
            return "nothing";
        }

        StringBuilder sb = new StringBuilder();
        int idS = 0;
        foreach (UnityMolStructure s in structures) {
            bool fatom = true;
            List<int2> atomRanges = new List<int2>();
            int lastId = -1;
            int2 curAtomRange;
            curAtomRange.x = -1;
            curAtomRange.y = -1000000;

            foreach (UnityMolAtom a in atoms) {
                if (a.residue.chain.model.structure == s) {
                    if (fatom) {
                        curAtomRange.x = (int)a.number;
                        fatom = false;
                    }
                    else if (lastId + 1 != a.number) {//New range
                        curAtomRange.y = lastId;
                        atomRanges.Add(curAtomRange);
                        curAtomRange.x = (int)a.number;
                        curAtomRange.y = -1000000;
                    }
                    lastId = (int)a.number;
                }
            }
            if (curAtomRange.y == -1000000) {//Add
                curAtomRange.y = lastId;
                atomRanges.Add(curAtomRange);
            }
            if (atomRanges.Count != 0) {
                sb.Append(s.name);
                sb.Append(" and (");
            }

            foreach (int2 r in atomRanges) {
                sb.Append("atomid ");
                sb.Append(r.x.ToString());
                sb.Append(":");
                sb.Append(r.y.ToString());
                sb.Append(" or ");
            }

            idS++;

        }

        sb.Length -= 4;//Remove the last " or "
        sb.Append(") ");


        return sb.ToString();
    }

    /// Return true if the given selection has the same atoms of this selection
    public bool sameAtoms(UnityMolSelection sel2) {

        if (sel2.atoms.Count != this.atoms.Count)
            return false;

        for (int i = 0; i < sel2.atoms.Count; i++) {
            if (atoms[i] != sel2.atoms[i]) {
                return false;
            }
        }
        return true;
    }

    /// Return true if the atoms of the selection belong to the same model and the same structure
    public bool sameModel() {
        if (extractTrajFrame)
            return false;
        if (structures.Count > 1)
            return false;
        for (int i = 0 ; i < atoms.Count - 1; i++) {
            if (atoms[i].residue.chain.model.name != atoms[i + 1].residue.chain.model.name) {
                return false;
            }
        }
        return true;
    }

    /// Compute the minimal and maximal positions of the selection.

    /// Based on taking the smallest and largest components of the atoms coordinates.
    private void computeMinMaxPos() {
        if (Count == 0) {
            _minPos = Vector3.zero;
            _maxPos = Vector3.zero;
            return;
        }

        _minPos = atoms[0].position - Vector3.one * atoms[0].radius;
        _maxPos = atoms[0].position + Vector3.one * atoms[0].radius;
        for (int i = 1; i < atoms.Count; i++) {
            _minPos = Vector3.Min(_minPos, atoms[i].position - Vector3.one * atoms[i].radius);
            _maxPos = Vector3.Max(_maxPos, atoms[i].position + Vector3.one * atoms[i].radius);
        }

    }

    private void resetMaxMinPos() {
        _minPos = Vector3.one * float.MinValue;
        _maxPos = Vector3.one * float.MaxValue;
    }

    /// Serialize the selection object in order to be saved.
    public SerializedSelection Serialize() {
        SerializedSelection ssel = new SerializedSelection();
        ssel.name = name;
        ssel.query = ToSelectionCommand();
        ssel.count = Count;
        ssel.structureNames = new List<string>(structures.Count);
        if (structures.Count == 1)
            ssel.structureIds = new List<int>(1);
        else
            ssel.structureIds = new List<int>(Count);

        ssel.atomIds = new List<int>(Count);

        Dictionary<UnityMolStructure, int> structureToId = new Dictionary<UnityMolStructure, int>();
        int id = 0;
        foreach (UnityMolStructure s in structures) {
            structureToId[s] = id;
            ssel.structureNames.Add(s.name);
            id++;
        }
        if (structures.Count == 1)
            ssel.structureIds.Add(0);

        for (int i = 0; i < Count; i++) {
            if (structures.Count != 1)
                ssel.structureIds.Add(structureToId[atoms[i].residue.chain.model.structure]);
            ssel.atomIds.Add(atoms[i].idInAllAtoms);
        }
        return ssel;
    }
}
}
