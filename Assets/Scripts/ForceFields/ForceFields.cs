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
//
// Object to hold all data concerned with force fields
// Used to assign parameters to atoms on molecule loading/parsing.
//
// Joao Rodrigues: j.p.g.l.m.rodrigues@gmail.com
//

namespace UMol {
namespace ForceFields {

    // Unity imports
    using UnityEngine;

    // C# imports
    using System;
    using System.Collections;
    using System.Collections.Generic;
    // Forcefield
    //   attributes:
    //   -- string name
    //   -- ResidueLibrary: collection of residues
    //   -- TypeLibrary: collection of atom types
    //
    //   methods:
    //     ResidueLibrary
    //       -- addResidue(name)
    //       -- addAtom(parent, type)
    //       -- containsResidue(name) -> residue object
    //     TypeLibrary
    //       -- addAtomType(charge, rmin, eps) -> to TypeLibrary
    //       -- containsAtomType(type) -> atomtype object
    //
    //   -- FFResidue object: string name, list<Atom> atoms
    //   -- Atom object: parent, name, type
    //   -- AtomType object: charge, Rmin, eps
    //

    public class ForceField
    {
        public string name;
        private Dictionary<string, FFResidue> ResidueLibrary = new Dictionary<string, FFResidue>();
        private Dictionary<string, FFAtomType> AtomTypeLibrary = new Dictionary<string, FFAtomType>();

        public ForceField(string name) {
            this.name = name;
        }
        
        // Residue Library Methods
        public void AddResidue(FFResidue aResidue) {
            
            FFResidue entry = (FFResidue)aResidue;
            
            if (this.ContainsResidue(entry)) {
                string err = "Force field already contains an entry for residue '{0}'";
                throw new System.Exception(String.Format(err, entry.name));
            }

            this.ResidueLibrary.Add(entry.name, entry);
        }

        public bool ContainsResidue(FFResidue aResidue) {
            FFResidue entry = (FFResidue)aResidue;
            return this.ResidueLibrary.ContainsKey(entry.name);
        }
        public bool ContainsResidue(string resName) {
            return this.ResidueLibrary.ContainsKey(resName);
        }

        public FFResidue GetResidue(string resName) {
            return ResidueLibrary[resName]; 
        }

        // AtomTypeLibrary methods
        public void AddAtomType(FFAtomType anAtomType) {
            
            FFAtomType entry = (FFAtomType)anAtomType;
            
            if (this.ContainsAtomType(entry)) {
                string err = "Force field already contains an entry for atom type '{0}'";
                throw new System.Exception(String.Format(err, entry.name));
            }

            this.AtomTypeLibrary.Add(entry.name, entry);
        }

        public bool ContainsAtomType(FFAtomType anAtomType) {
            FFAtomType entry = (FFAtomType)anAtomType;
            return this.AtomTypeLibrary.ContainsKey(entry.name);
        }
        public bool ContainsAtomType(string atomTypeName) {
            return this.AtomTypeLibrary.ContainsKey(atomTypeName);
        }

        public FFAtomType GetAtomType(string atomTypeName) {
            return AtomTypeLibrary[atomTypeName];
        }


        // ToString
        public override String ToString() {
            string stringFmt= "ForceField '{0}' ({1} residues, {2} atom types)";
            return String.Format(stringFmt, name, ResidueLibrary.Count, AtomTypeLibrary.Count);
        }

    }

    /// <summary>
    /// Class FFAtom
    /// Container for atom parameters in a force field.
    /// Subclass if you need to add more detailed info.
    /// </summary>
    public class FFAtom
    {
        public string parent;
        public string name;
        public string type;
        public float charge = 0.0f;


        // Constructor
        // <summary>
        // Instantiates a new FFAtom object.
        // Charge defaults to 0.0
        // </summary>
        // <param name='parent'>
        // [string] name of residue the atom belongs to
        // </param>
        // <param name='name'>
        // [string] atom name
        // </param>
        // <param name='type'>
        // [string] force field atom type
        // </param>
        // <param name='charge'>
        // [float] atom partial charge
        // </param>
        public FFAtom(string parent, string name, string type) {
            this.parent = parent;
            this.name = name;
            this.type = type;
        }

        // ToString
        public override String ToString() {
            string stringFmt= "FFAtom '{0}' -> type={1} q={2,-6:N3}";
            return String.Format(stringFmt, name, type, charge);
        }
    }

    public class FFAtomType
    {
        public string name;
        public float rmin = 0.0f;
        public float eps = 0.0f;
        public float _sigma = float.MinValue;
        public float sigma {
            //rmin = 2^(1/6) _sigma => _sigma = rmin / 1.224...
            get {if(_sigma == float.MinValue)
                    _sigma = rmin / 1.12246204831f;
                return _sigma;}
        }
        // Constructor
        // <summary>
        // Instantiates a new FFAtomType object.
        // If no rmin, or eps values given, default to 0.0
        // </summary>
        // <param name='name'>
        // [string] force field atom type
        // </param>
        // <param name='charge'>
        // [float] atom partial charge
        // </param>
        // <param name='rmin'>
        // [float] LJ Rmin value (distance at which the potential is at minimum)
        // </param>
        // <param name='eps'>
        // [float] LJ epsilon value (depth of the potential well, i.e. attractiveness)
        // </param>
        public FFAtomType(string name) {
            this.name = name;
        }

        // ToString
        public override String ToString() {
            string stringFmt= "FFAtomType '{0}' -> eps={1,-6:N3} Rmin={2,-6:N3}";
            return String.Format(stringFmt, name, eps, rmin);
        }
    }

    /// <summary>
    /// Class FFResidue
    /// Container for residue definitions in a force field.
    /// Subclass if you need to add more detailed info.
    /// </summary>
    public class FFResidue
    {
        public string name;
        public Dictionary<string, FFAtom> atoms = new Dictionary<string, FFAtom>();

        // Constructor
        // <summary>
        // Instantiates a new FFResidue object with an empty atom list.
        // </summary>
        // <param name='name'>
        // [string] name of residue
        // </param>
        public FFResidue(string name) {
            this.name = name;
        }

        // Methods
        public void AddAtom(FFAtom newAtom) {
            
            FFAtom child = (FFAtom)newAtom;
            
            if (this.ContainsAtom(child)) {
                string err = "FFResidue already contains a definition for atom '{0}'";
                throw new System.Exception(String.Format(err, child.name));
            }

            child.parent = this.name;
            this.atoms.Add(child.name, child);
        }

        // ContainsAtom methods: by FFAtom and by FFAtom.name
        public bool ContainsAtom(FFAtom anAtom) {
            FFAtom child = (FFAtom)anAtom;
            return this.atoms.ContainsKey(child.name);
        }
        public bool ContainsAtom(string atomName) {
            return this.atoms.ContainsKey(atomName);
        }

        // Get Atom
        public FFAtom GetAtom(string atomName) {
            return atoms[atomName]; 
        }

        // ToString
        public override String ToString() {
            string stringFmt= "Residue '{0}' ({1} atoms)";
            return String.Format(stringFmt, name, atoms.Count);
        }
    }
}
}