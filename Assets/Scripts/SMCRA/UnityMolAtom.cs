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
using System.Text;
using System.Collections.Generic;

namespace UMol {
/// <summary>
/// Part of the SMCRA data structure, it stores atom information of the structure.
/// </summary>
public class UnityMolAtom {

	/// <summary>
	/// Global atom serial counter. Do not change this.
	/// Each time a new UnityMolAtom is created, this will increase
	/// </summary>
	public static int GlobAtomSerial;

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
	public Color32 	 color32;

	/// <summary>
	/// Representation texture
	/// </summary>
	public string	 texture;

	/// <summary>
	/// Index of this atom in the model.allAtoms list
	/// </summary>
	public int idInAllAtoms = -1;

	public int index {
		get { return idInAllAtoms;}
	}

	/// <summary>
	/// Global serial number of this atom
	/// </summary>
	public readonly int serial;

	/// <summary>
	/// Position accessor as a Vector4
	/// </summary>
	public Vector4 PositionVec4 => new(position.x, position.y, position.z, 0f);

    public Vector3 curWorldPosition {
		get {
			Vector3 globalPos = residue.chain.model.structure.annotationParent.transform.TransformPoint(position);
			return globalPos;
		}
	}

	private int lhash = -1;
	public int LightHashCode {
		get {
			if (lhash == -1) {
				computeLightHashCode();
			}
			return lhash;
		}
    }

	/// <summary>
	/// UnityMolAtom constructor taking all atom information as arg, calls setAtomRepresentation()
	/// </summary>
	public UnityMolAtom(string _name, string _type, Vector3 _pos, float _bfact, long _number, bool _isHET = false) {
		name = _name;
		type = _type;
		isHET = _isHET;
		position = _pos;
		oriPosition = _pos;

		bfactor = _bfact;
		number = _number;
		serial = GlobAtomSerial++;
		setAtomRepresentation();
	}

	public void SetResidue(UnityMolResidue r) {
		residue = r;
	}


	/// <summary>
	/// Set default representation
	/// </summary>
	private void setAtomRepresentation() {

		UnityMolMain.atomColors.getColorAtom(type, out color32, out radius);
		color = color32;//Convert from Color32 to Color
		scale = 100;
		texture = null;

	}

	/// <summary>
	/// Set representation regarding the model (OPEP, HiRE-RNA, Martini or all-atom)
	/// </summary>
	public void SetAtomRepresentationModel(string prefix) {

		UnityMolMain.atomColors.getColorAtom(prefix + name, out color32, out radius);
		color = color32;//Convert from Color32 to Color
		scale = 100;
		texture = null;
	}


	/// <summary>
	/// Atom string representation
	/// </summary>
	public override string ToString() {
		StringBuilder e = new();
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
		UnityMolAtom cloned = new(name, type, oriPosition, bfactor, number, isHET) {
            position = position
        };

        cloned.SetResidue(residue);
		cloned.isLigand = isLigand;
		cloned.radius = radius;
		cloned.scale = scale;
		cloned.color = color;
		cloned.color32 = color32;
		cloned.texture = texture;
		cloned.idInAllAtoms = idInAllAtoms;

		return cloned;
	}

	public UnityMolSelection ToSelection() {
		List<UnityMolAtom> selectedAtoms = new() { this };
        string selectionMDA = residue.chain.model.structure.name +
                              " and chain " + residue.chain.name + " and resid " +
                              residue.id + " and name " + name + " and atomid " + number;

		return new UnityMolSelection(selectedAtoms, newBonds: null, "Atom_" + name + number, selectionMDA);
	}

	public string ToSelectionMDA() {
		return residue.chain.model.structure.name +
		       " and chain " + residue.chain.name + " and resid " +
		       residue.id + " and name " + name + " and atomid " + number;
	}

	public string ToSelectionName() {
		return residue.chain.model.structure.name + "_" + residue.chain.model.name + "_" +
		       residue.chain.name + "_" + residue.name + "_" + residue.id + "_" + name + "_" + number;
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
		if (obj is UnityMolAtom a2) {
            if (ReferenceEquals(null, this) && ReferenceEquals(null, a2)) { return true;}
			if (ReferenceEquals(null, this) || ReferenceEquals(null, a2)) { return false;}
			return serial == a2.serial;
		}
		return false;
	}

	public override int GetHashCode() {
		return serial;
	}

    private void computeLightHashCode() {

		unchecked
		{
			const int seed = 1009;
			const int factor = 9176;
			lhash = seed;
			lhash = lhash * factor + residue.LightHashCode;
			lhash = lhash * factor + name.GetHashCode();
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
		if (a1.residue.chain.model.structure.name != a2.residue.chain.model.structure.name) {return false;}
		return true;
	}
	public int GetHashCode(UnityMolAtom a) {
		return a.LightHashCode;
	}
}

}
