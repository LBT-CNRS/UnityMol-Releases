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
using System.Text;
using System.Collections.Generic;
using System;

namespace UMol {
/// <summary>
/// Part of the SMCRA data structure, UnityMolAtom stores atom informations of the structure
/// </summary>
public class UnityMolAtom {

	/// <summary>
	/// Global atom serial counter. Do not change this.
	/// Each time a new UnityMolAtom is created, this will increase
	/// </summary>
	public static int globAtomSerial;

	/// <summary>
	/// Store the reference to the residue it belongs to
	/// </summary>
	public UnityMolResidue residue;

	/// <summary>
	/// Name of the atom
	/// </summary>
	public string 	 name;


	/// <summary>
	/// Position of the atom, this is modified when reading a trajectory
	/// </summary>
	public Vector3    position;

	/// <summary>
	/// Position of the atom without offset, from the input file
	/// </summary>
	public Vector3    oriPosition;

	/// <summary>
	/// Bfactor of the atom
	/// </summary>
	public float 	 bfactor;

	/// <summary>
	/// Atom type
	/// </summary>
	public string 	 type;

	/// <summary>
	/// Is this atom a hetero atom
	/// </summary>
	public bool       isHET;

	/// <summary>
	/// Is this atom a ligand atom
	/// </summary>
	public bool       isLigand;

	/// <summary>
	/// Atom number
	/// </summary>
	public long 		 number;

	/// <summary>
	/// Representation default radius of the atom (Van der Walls)
	/// </summary>
	public float		 radius;

	/// <summary>
	/// Representation scale
	/// </summary>
	public float		 scale;

	/// <summary>
	/// Representation color
	/// </summary>
	public Color 	 color;

	/// <summary>
	/// Representation texture
	/// </summary>
	public string	 texture;

	/// <summary>
	/// Index of this atom in the model.allAtoms list
	/// </summary>
	public int idInAllAtoms = -1;

	/// <summary>
	/// Global serial number of this atom
	/// </summary>
	public int serial;

	/// <summary>
	/// Position accessor as a Vector4
	/// </summary>
	public Vector4 PositionVec4 {
		get { return new Vector4(position.x, position.y, position.z, 0f); }
	}

	public Vector3 curWorldPosition {
		get {
			Vector3 globalPos = residue.chain.model.structure.getAtomGos()[idInAllAtoms].transform.position;
			return globalPos;
		}
	}

	public GameObject correspondingGo {
		get {
			return residue.chain.model.structure.getAtomGos()[idInAllAtoms];
		}
	}

	private int _lhash = -1;
	public int lightHashCode {
		get {
			if (_lhash == -1) {
				computeLightHashCode();
			}
			return _lhash;
		}
		set {
			_lhash = value;
		}
	}

	/// <summary>
	/// UnityMolAtom constructor taking all atom informations as arg, calls setAtomRepresentation()
	/// </summary>
	public UnityMolAtom(string _name, string _type, Vector3 _pos, float _bfact, long _number, bool _isHET = false) {
		name = _name;
		type = _type;
		isHET = _isHET;
		position = _pos;
		oriPosition = _pos;

		bfactor = _bfact;
		number = _number;
		serial = globAtomSerial++;
		SetAtomRepresentation();
	}

	public void SetResidue(UnityMolResidue r) {
		residue = r;
	}


	/// <summary>
	/// Set default representation
	/// </summary>
	private void SetAtomRepresentation() {

		UnityMolMain.atomColors.getColorAtom(type, out color, out radius);
		scale = 100;
		texture = null;

	}

	/// <summary>
	/// Set representation regarding the model (OPEP, HiRE-RNA, Martini or all-atom)
	/// </summary>
	public void SetAtomRepresentationModel(string prefix) {

		UnityMolMain.atomColors.getColorAtom(prefix + name, out color, out radius);
		scale = 100;
		texture = null;
	}


	/// <summary>
	/// Atom string representation
	/// </summary>
	public override string ToString() {
		StringBuilder e = new StringBuilder();
		e.Append("<");
		if (residue != null) {
			e.Append(residue.chain.model.structure.name);
			e.Append(" | ");
			e.Append(residue.chain.name);
			e.Append(" | ");
			e.Append(residue.name);
			e.Append("_");
			e.Append(residue.id);
			e.Append(" | ");
		}
		e.Append(name);
		e.Append(">");
		return e.ToString();
	}


	/// <summary>
	/// Clone a UnityMolAtom
	/// Keep modified elements like position / color ...
	/// Serial will be different !
	/// </summary>
	public UnityMolAtom Clone() {
		UnityMolAtom cloned = new UnityMolAtom(name, type, oriPosition, bfactor, number, isHET);

		cloned.position = position;
		cloned.SetResidue(residue);
		cloned.isLigand = isLigand;
		cloned.radius = radius;
		cloned.scale = scale;
		cloned.color = color;
		cloned.texture = texture;
		cloned.idInAllAtoms = idInAllAtoms;

		return cloned;
	}

	public UnityMolSelection ToSelection() {
		List<UnityMolAtom> selectedAtoms = new List<UnityMolAtom>();
		selectedAtoms.Add(this);
		string selectionMDA = residue.chain.model.structure.uniqueName +
		                      " and chain " + residue.chain.name + " and resid " +
		                      residue.id + " and name " + name + " and atomid " + number;

		return new UnityMolSelection(selectedAtoms, newBonds: null, "Atom_" + name + number, selectionMDA);
	}

	public static bool operator ==(UnityMolAtom lhs, UnityMolAtom rhs) {

		if (ReferenceEquals(null, lhs) && ReferenceEquals(null, rhs)) { return true;}
		if (ReferenceEquals(null, lhs) || ReferenceEquals(null, rhs)) { return false;}
		return lhs.serial == rhs.serial;
	}
	public static bool operator !=(UnityMolAtom lhs, UnityMolAtom rhs) {
		if (ReferenceEquals(null, lhs) && ReferenceEquals(null, rhs)) { return false;}
		if (ReferenceEquals(null, lhs) || ReferenceEquals(null, rhs)) { return true;}
		return lhs.serial != rhs.serial;
	}
	public override bool Equals(object obj) {
		if (obj is UnityMolAtom) {
			UnityMolAtom a2 = obj as UnityMolAtom;
			if (ReferenceEquals(null, this) && ReferenceEquals(null, a2)) { return true;}
			if (ReferenceEquals(null, this) || ReferenceEquals(null, a2)) { return false;}
			return serial == a2.serial;
		}
		return false;
	}

	public override int GetHashCode() {
		return serial;
	}

	void computeLightHashCode() {

		unchecked
		{
			const int seed = 1009;
			const int factor = 9176;
			_lhash = seed;
			_lhash = _lhash * factor + residue.lightHashCode;
			_lhash = _lhash * factor + name.GetHashCode();
		}
	}
}

public class LightAtomComparer : IEqualityComparer<UnityMolAtom>
{
	public bool Equals(UnityMolAtom a1, UnityMolAtom a2) {
		if (a1 == null && a2 == null) {return true;}
		if (a1 == null | a2 == null) {return false;}
		if (a1.name != a2.name) {return false;}
		if (a1.type != a2.type) {return false;}
		if (a1.residue.name != a2.residue.name) {return false;}
		if (a1.residue.id != a2.residue.id) {return false;}
		if (a1.residue.chain.name != a2.residue.chain.name) {return false;}
		if (a1.residue.chain.model.structure.uniqueName != a2.residue.chain.model.structure.uniqueName) {return false;}
		return true;
	}
	public int GetHashCode(UnityMolAtom a) {
		return a.lightHashCode;
	}
}

}