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
using System.Text;

namespace UMol {
public class UnityMolSelection {

	public bool isAlterable = true;
	public bool canUpdateBonds = true;

	///Selection content updates each frame of the trajectory
	public bool updateWithTraj = false;
	public Dictionary<RepType, List<UnityMolRepresentation>> representations;
	List<UnityMolAtom> _atoms;
	public List<UnityMolAtom> atoms {
		get {return _atoms;}
		set {
			if (!isAlterable) {
				Debug.LogWarning("This selection is not alterable");
			}
			else {
				_atoms = value;
			}
		}
	}

	string _name;
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

	UnityMolBonds _bonds;
	public UnityMolBonds bonds {
		get {
			if (_bonds == null)
				fillBonds();
			return _bonds;
		}
		set {
			if(canUpdateBonds)
				_bonds = value;
		}
	}
	public bool bondsNull {
		get {
			return _bonds == null;
		}
	}

	public List<UnityMolStructure> structures;

	public bool fromSelectionLanguage = false;
	public string MDASelString = "";

	public bool forceGlobalSelection =  false;

	public Vector3 centerOfGravity {
		get {
			return ManipulationManager.computeCenterOfGravitySel(this);
		}
	}

	public int Count {
		get {
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


	private void fillBonds() {
		UnityMolBonds recoveredBonds = new UnityMolBonds();
		HashSet<UnityMolAtom> atomsHS = new HashSet<UnityMolAtom>();
		foreach (UnityMolAtom a in atoms) {
			atomsHS.Add(a);
		}

		List<UnityMolAtom> outBonded = new List<UnityMolAtom>();
		foreach (UnityMolAtom a in atoms) {
			if (a.residue.chain.model.bonds.Dbonds.ContainsKey(a)) {
				outBonded.Clear();
				UnityMolAtom[] bonded = a.residue.chain.model.bonds.Dbonds[a];

				foreach (UnityMolAtom a2 in bonded) {
					if (a2 != null) {
						if (atomsHS.Contains(a2)) {
							outBonded.Add(a2);
						}
					}
				}
				recoveredBonds.Add(a, outBonded.ToArray());
			}
		}

		_bonds = recoveredBonds;
	}

	public override string ToString() {
		if (_bonds != null) {
			return "Selection of " + atoms.Count + " atoms / " + _bonds.Count + " bonds, named '" + name + "'";
		}
		return "Selection of " + atoms.Count + " atoms, named '" + name + "'";

	}

	public bool isGlobalSelection() {
		if (structures == null) {
			fillStructures();
		}
		return structures.Count > 1;
	}

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

	public void mergeRepresentations(UnityMolSelection sel2) {
		foreach (RepType rept in sel2.representations.Keys) {
			if (!representations.ContainsKey(rept)) {
				representations[rept] = new List<UnityMolRepresentation>();
			}
			representations[rept].AddRange(sel2.representations[rept]);
		}
	}

	/// Warning : Not merging representations of the 2 selections
	public static UnityMolSelection operator +(UnityMolSelection sel1, UnityMolSelection sel2) {
		if (!sel1.isAlterable) {
			Debug.LogWarning("This selection is not alterable");
			return sel1;
		}
		UnityMolSelection result = sel1;
		result.atoms.AddRange(sel2.atoms);
		result.atoms = result.atoms.Distinct().ToList();
		result._bonds = null;
		result.fillStructures();

		// //Merging representations
		// foreach(RepType rep in sel2.representations.Keys){
		// 	if(!sel1.representations.ContainsKey(rep)){
		// 		sel1.representations[rep] = sel2.representations[rep];
		// 	}
		// 	else{
		// 		sel1.representations[rep].AddRange(sel2.representations[rep]);
		// 	}
		// }
		return result;
	}

	//Warning : Not modifying representations of the 2 selections
	public static UnityMolSelection operator -(UnityMolSelection sel1, UnityMolSelection sel2) {

		if (!sel1.isAlterable) {
			Debug.LogWarning("This selection is not alterable");
			return sel1;
		}

		UnityMolSelection result = new UnityMolSelection(new List<UnityMolAtom>(), sel1.name);
		HashSet<UnityMolAtom> atomsHS = new HashSet<UnityMolAtom>();

		foreach (UnityMolAtom a in sel2.atoms) {
			atomsHS.Add(a);
		}


		foreach (UnityMolAtom a in sel1.atoms) {
			if (!atomsHS.Contains(a)) {
				result.atoms.Add(a);
			}
		}


		result.representations = sel1.representations;
		// foreach(RepType rep in sel2.representations.Keys){
		// 	if(sel1.representations.ContainsKey(rep)){
		// 		List<UnityMolRepresentation> toRemove = new List<UnityMolRepresentation>();
		// 		foreach(UnityMolRepresentation r in sel1.representations[rep]){
		// 			for(int i=0;i<sel2.representations[rep].Count;i++){
		// 				if(r == sel2.representations[rep][i]){
		// 					toRemove.Add(sel2.representations[rep][i]);
		// 				}
		// 			}
		// 		}
		// 		foreach(UnityMolRepresentation r in toRemove){
		// 			sel2.representations[rep].Remove(r);
		// 		}
		// 	}
		// }
		result.fillStructures();

		return result;
	}

	public string ToSelectionCommand() {

		if (fromSelectionLanguage) {
			return MDASelString;
		}

		StringBuilder sb = new StringBuilder();

		foreach (UnityMolStructure s in structures) {
			bool fatom = true;
			foreach (UnityMolAtom a in atoms) {
				if (a.residue.chain.model.structure == s) {
					if (!fatom) {
						sb.Append(" or ");
					}
					sb.Append("atomid ");
					sb.Append(a.number.ToString());
					sb.Append(" and ");
					sb.Append(s.uniqueName);
					fatom = false;
				}
			}
			sb.Append(" ");
		}

		// string prevStrucName = "";

		// foreach(UnityMolAtom a in atoms){
		// 	int idAtom = a.residue.chain.model.structure.currentModel.allAtoms.IndexOf(a);
		// 	if(idAtom == -1){
		// 		Debug.LogWarning("Couldn't find the atom in the model");
		// 		continue;
		// 	}

		// 	string structureName = a.residue.chain.model.structure.name;
		// 	if(prevStrucName != structureName){
		// 		sb.Append("_s = UnityMolMain.getStructureManager().nameToStructure[\"");
		// 		sb.Append(structureName);
		// 		sb.Append("\"]\n");
		// 	}
		// 	sb.Append("_selectedAtoms.Add(_s.currentModel.allAtoms["+idAtom+"])\n");
		// 	prevStrucName = structureName;
		// }
		// sb.Append("_selection = UnityMolSelection(_selectedAtoms,\""+name+"\")");


		return sb.ToString();
	}

	public bool sameAtoms(UnityMolSelection sel2) {

		if (sel2.Count != this.Count)
			return false;

		for (int i = 0; i < sel2.Count; i++) {
			if (atoms[i] != sel2.atoms[i]) {
				return false;
			}
		}
		return true;
	}

	public bool sameModel() {
		for (int i = 0 ; i < Count - 1; i++) {
			if (atoms[i].residue.chain.model.name != atoms[i + 1].residue.chain.model.name) {
				return false;
			}
		}
		return true;
	}
}
}