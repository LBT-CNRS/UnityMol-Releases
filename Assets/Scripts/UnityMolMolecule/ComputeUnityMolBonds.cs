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
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

namespace UMol {

public class ComputeUnityMolBonds {

	public static bool checkCovalentBondDistance = false;
	public static float fudgeFactor = 0.62f;
	public static float lowerBondDistance = 0.1f;
	public static float higherBondDistance = 3.0f;


	public static UnityMolBonds ComputeBondsGridFudge(List<UnityMolAtom> allAtoms, List<UnityMolAtom> atoms) {
		float maxVDW = UnityMolMain.atomColors.getMaxRadius(atoms);

		int N = atoms.Count;
		UnityMolBonds bonds = new UnityMolBonds();

		//-------------------- C# Burst compiled implementation

		int maxRes = 15;
		float maxDistNei = (maxVDW + maxVDW) * fudgeFactor;
		
		float cutoff = maxVDW;
		UnityMolStructure s = allAtoms[0].residue.chain.model.structure;

		// NativeArray<int> results = nsgb.getPointsInRadius(allPoints, atomPoints, maxRes, cutoff);
		// NativeArray<int> results = s.spatialSearch.SearchKNeighbors(atoms, maxRes);
		NativeArray<int> results = s.spatialSearch.SearchWithin(atoms, maxDistNei, maxRes);

		for (int i = 0; i < atoms.Count; i++) {
			UnityMolAtom atom = atoms[i];
			Vector3 atom0 = atom.position;

			float maxDist = (atom.radius + maxVDW) * fudgeFactor;

			for (int n = 0; n < maxRes; n++) {

				int id = results[i * maxRes + n];
				if (id < 0 || id >= allAtoms.Count) {
					break;
				}
				UnityMolAtom atom2 = allAtoms[id];

				if (atom2 == atom) {
					continue;
				}

				//Don't bind atoms from different residues
				if (atom.residue == atom2.residue) {
					continue;
				}

				float dist = Vector3.Distance(atom0, atom2.position);
				float accept = (atom.radius + atom2.radius) * fudgeFactor;

				if (dist > maxDist || dist < 0.01f) {
					continue;
				}

				// Specific VDW radius for hydrogen, different from the one we show
				if (atom.type == "H") {
					accept = (0.4f + atom2.radius) * fudgeFactor;
				}
				if (atom2.type == "H") {
					accept = (0.4f + atom.radius) * fudgeFactor;
				}

				if (dist > lowerBondDistance && dist <= accept) {

					if (!bonds.isBondedTo(atom, atom2) && !(atom.type == "H" && atom2.type == "H")) {
						bonds.Add(atom, atom2);
					}
				}

			}
		}

		results.Dispose();

		return bonds;
	}

	public static UnityMolBonds ComputeBondsKDtreeFudge(List<UnityMolAtom> allAtoms, List<UnityMolAtom> atoms) {
		//Adapted from MDAnalysis: https://github.com/MDAnalysis/mdanalysis/blob/36e12e98c958580d263c3e7c54f43c5565171787/package/MDAnalysis/topology/guessers.py
		float maxVDW = UnityMolMain.atomColors.getMaxRadius(atoms);

		int N = atoms.Count;
		UnityMolBonds bonds = new UnityMolBonds();
		// LightResidueComparer lrc = new LightResidueComparer();
		if (N < 100) { //Use the naive implementation

			for (int i = 0; i < N; i++) {
				UnityMolAtom atom = atoms[i];
				bool isH = atom.type == "H";
				Vector3 atom0 = atom.position;
				float maxDist = (atom.radius + maxVDW) * fudgeFactor;

				for (int j = 0; j < allAtoms.Count; j++) {
					UnityMolAtom atom2 = allAtoms[j];
					bool secIsH = atom2.type == "H";

					if (atom2 == atom) {
						continue;
					}

					//Don't bind atoms from different residues
					if (atom.residue != atom2.residue) {
						continue;
					}

					float dist = Vector3.Distance(atom0, atom2.position);
					float accept = (atom.radius + atom2.radius) * fudgeFactor;

					if (dist > maxDist || dist < 0.01f) {
						continue;
					}

					// Specific VDW radius for hydrogen, different from the one we show
					if (isH) {
						accept = (0.4f + atom2.radius) * fudgeFactor;
					}
					if (secIsH) {
						accept = (0.4f + atom.radius) * fudgeFactor;
					}

					if (dist > lowerBondDistance && dist <= accept) {

						if (!(isH && secIsH) && !bonds.isBondedTo(atom, atom2) ) {
							bonds.Add(atom, atom2);
						}
					}
				}
			}

			return bonds;
		}

		//-------------------- C# Burst compiled implementation

		UnityMolStructure s = allAtoms[0].residue.chain.model.structure;

		float maxDistNei = (maxVDW + maxVDW) * fudgeFactor;
		int kNeighbours = 15;

		//This is a bit slower
		NativeArray<int> results = s.spatialSearch.SearchWithin(atoms, maxDistNei, kNeighbours);
		// NativeArray<int> results = s.spatialSearch.SearchKNeighbors(atoms, kNeighbours);


		for (int i = 0; i < N; i++) {
			UnityMolAtom atom = atoms[i];
			bool isH = atom.type == "H";
			Vector3 atom0 = atom.position;
			string atomtype0 = atom.type;

			float maxDist = (atom.radius + maxVDW) * fudgeFactor;

			for (int n = 0; n < kNeighbours; n++) {

				int id = results[i * kNeighbours + n];
				if (id < 0 || id >= allAtoms.Count) {
					continue;
				}
				UnityMolAtom atom2 = allAtoms[id];

				if (atom2 == atom) {
					continue;
				}

				//Don't bind atoms from different residues
				if ((atom.type != "S" && atom.residue.name != "CYS") && atom.residue != atom2.residue) {
					continue;
				}

				float dist = Vector3.Distance(atom0, atom2.position);
				float accept = (atom.radius + atom2.radius) * fudgeFactor;

				if (dist > maxDist || dist < 0.01f) {
					continue;
				}

				bool secIsH = atom2.type == "H";

				// Specific VDW radius for hydrogen, different from the one we show
				if (isH) {
					accept = (0.4f + atom2.radius) * fudgeFactor;
				}
				if (secIsH) {
					accept = (0.4f + atom.radius) * fudgeFactor;
				}

				if (dist > lowerBondDistance && dist <= accept) {

					if (!(isH && secIsH) && !bonds.isBondedTo(atom, atom2) ) {
						bonds.Add(atom, atom2);
					}
				}
			}
		}

		results.Dispose();

		return bonds;
	}


	public static UnityMolBonds ComputeBondsMartiniITP(List<UnityMolAtom> atoms) {
		UnityMolBonds bonds = new UnityMolBonds();

		int nbBonds = 0;
		int id = 0;
		UnityMolResidue curRes = null;

		foreach (UnityMolAtom atom in atoms) {
			if (id == 0) {
				curRes = atom.residue;
			}
			Dictionary<string, CGAtomITP> res = null;
			string curCGName = atom.residue.name;

			if (curRes.name != curCGName) {
				//Try to link protein residues between each other
				if (curRes != null) {
					if (atom.residue.atoms.ContainsKey("BB") && curRes.atoms.ContainsKey("BB")) {
						UnityMolAtom prevBB = curRes.atoms["BB"];
						if (!bonds.isBondedTo(prevBB, atom.residue.atoms["BB"])) {
							bonds.Add(prevBB, atom.residue.atoms["BB"]);
							nbBonds++;
						}
					}
				}
				curRes = atom.residue;
			}

			id++;

			if (UnityMolMain.loadedITP.TryGetValue(curCGName, out res)) {
				CGAtomITP cga;

				if (res.TryGetValue(atom.name, out cga)) {

					foreach (string a in cga.bonds) {
						if (atom.residue.atoms.ContainsKey(a) && !bonds.isBondedTo(atom, atom.residue.atoms[a])) {
							bonds.Add(atom, atom.residue.atoms[a]);
							nbBonds++;
						}
					}
				}

			}
			else {

			}
		}
		Debug.Log("Found " + nbBonds + " Martini bonds");

		return bonds;
	}

	public static UnityMolBonds ComputeBondsByResidue(List<UnityMolAtom> atoms, bool ignoreTopology = false) {
		float start = Time.realtimeSinceStartup;

		if (atoms.Count > 0 && atoms[0].residue.chain.model.structure.structureType == UnityMolStructure.MolecularType.Martini) {
			return ComputeBondsMartiniITP(atoms);
		}
		UnityMolBonds bonds = new UnityMolBonds();

		if (atoms.Count < 2)
			return bonds;

		List<UnityMolAtom> bonded = new List<UnityMolAtom>(30);//Atom buffer
		int N = atoms.Count;
		UnityMolResidue curRes = null;
		List<UnityMolAtom> otherAtoms = new List<UnityMolAtom>();
		// HashSet<UnityMolAtom> inParsedConnectivity = new HashSet<UnityMolAtom>();
		StringBuilder sb = new StringBuilder();
		Dictionary<UnityMolAtom, int> bondPerAtom = null;

		if (atoms[0].residue.chain.model.structure.parsedConnectivity != null) { //Use parsed connectivity
			UnityMolModel curMod = atoms[0].residue.chain.model;
			Dictionary<long, UnityMolAtom> atomIdToAtom = new Dictionary<long, UnityMolAtom>();
			bondPerAtom = new Dictionary<UnityMolAtom, int>(curMod.Count);
			foreach (UnityMolAtom a in curMod.allAtoms) {
				atomIdToAtom[a.number] = a;
			}
			int maxBondPerAtom = 0;

			//First pass to compute max bond per atom

			foreach (int2 b in curMod.structure.parsedConnectivity) {
				if (!atomIdToAtom.ContainsKey(b.x))
					continue;

				if (!atomIdToAtom.ContainsKey(b.y))
					continue;
				UnityMolAtom a1 = atomIdToAtom[b.x];
				UnityMolAtom a2 = atomIdToAtom[b.y];

				if (!bondPerAtom.ContainsKey(a1))
					bondPerAtom[a1] = 0;
				if (!bondPerAtom.ContainsKey(a2))
					bondPerAtom[a2] = 0;
				if (a1 != a2) {
					bondPerAtom[a1]++;
					bondPerAtom[a2]++;
					maxBondPerAtom = Mathf.Max(bondPerAtom[a1], Mathf.Max(bondPerAtom[a2], maxBondPerAtom));
				}
			}
			bonds.NBBONDS = (maxBondPerAtom > bonds.NBBONDS ? maxBondPerAtom : bonds.NBBONDS);

			foreach (int2 b in curMod.structure.parsedConnectivity) {
				if (!atomIdToAtom.ContainsKey(b.x)) {
					// sb.Append("Couldn't find atom ");
					// sb.Append(b.x);
					// sb.Append(" -> ignoring this bond\n");
					// continue;
					if (sb.Length == 0)
						sb.Append("Ignoring parsed bonds, could not find atoms: ");
					sb.Append("(");
					sb.Append(b.x);
					sb.Append(" | ");
					sb.Append(b.y);
					sb.Append(") ");
					continue;
				}
				if (!atomIdToAtom.ContainsKey(b.y)) {
					// sb.Append("Couldn't find atom ");
					// sb.Append(b.y);
					// sb.Append(" -> ignoring this bond\n");
					// continue;
					if (sb.Length == 0)
						sb.Append("Ignoring parsed bonds, could not find atoms: ");
					sb.Append("(");
					sb.Append(b.x);
					sb.Append(" | ");
					sb.Append(b.y);
					sb.Append(") ");
					continue;
				}
				UnityMolAtom a1 = atomIdToAtom[b.x];
				UnityMolAtom a2 = atomIdToAtom[b.y];

				// if (!inParsedConnectivity.Contains(a1))
				// 	inParsedConnectivity.Add(a1);
				// if (!inParsedConnectivity.Contains(a2))
				// 	inParsedConnectivity.Add(a2);

				if (a1 != a2 && !bonds.isBondedTo(a1, a2)) {
					bonds.Add(a1, a2);
				}
			}
		}
		if (sb.Length != 0) {
			Debug.LogWarning(sb.ToString());
		}

		if (ignoreTopology) { //Only use neighbors to compute bonds
			for (int i = 0; i < N; i++) {
				UnityMolAtom atom = atoms[i];
				otherAtoms.Add(atom);
			}
		}
		else {
			for (int i = 0; i < N; i++) {
				UnityMolAtom atom = atoms[i];

				//If we have information in the PDB CONECT field ignore this atom
				if (atom.isHET && bondPerAtom != null && !bondPerAtom.ContainsKey(atom)) {
					// if (atom.isHET && !inParsedConnectivity.Contains(atom)) {
					otherAtoms.Add(atom);
					continue;
				}


				UnityMolMain.topologies.getBondedAtomsInResidue(atom, ref bonded);

				if (bonded.Count == 0) {
					otherAtoms.Add(atom);
					continue;
				}


				int nbAdded = 0;
				for (int a = 0; a < bonded.Count - 1; a += 2) {
					if (bonded[a] != bonded[a + 1] && !bonds.isBondedTo(bonded[a], bonded[a + 1])) {
						if (checkCovalentBondDistance && Vector3.Distance(bonded[a].position, bonded[a + 1].position) > higherBondDistance) {
							continue;
						}
						bonds.Add(bonded[a], bonded[a + 1]);
						nbAdded++;
					}
				}
				// if(nbAdded == 0){
				// 	otherAtoms.Add(atom);
				// 	continue;
				// }

				//For disulfure bonds
				if (curRes != null && atom.type == "S" && curRes.name == "CYS") {
					otherAtoms.Add(atom);
				}

				if (curRes != null && atom.residue.id != curRes.id && atom.residue.id == curRes.id + 1) { //New consecutive residue

					//For proteins !
					pairString crossResAtoms = UnityMolMain.topologies.getPreviousAtomToLink(atom);
					if (crossResAtoms.s1 == null) {
						continue;
					}
					string prevAtomName = crossResAtoms.s1.Replace("-", "");
					string nextAtomName = crossResAtoms.s2;

					if (curRes.atoms.ContainsKey(prevAtomName) && atom.residue.atoms.ContainsKey(nextAtomName)) {
						UnityMolAtom prevAtom = curRes.atoms[prevAtomName];
						UnityMolAtom newAtom = atom.residue.atoms[nextAtomName];
						if (prevAtom != newAtom) {
							float dist = Vector3.Distance(prevAtom.position, newAtom.position);
							if (dist > lowerBondDistance) {
								if (checkCovalentBondDistance && dist < higherBondDistance)
									bonds.Add(prevAtom, newAtom);
								else
									bonds.Add(prevAtom, newAtom);
							}
						}
					}
				}
				curRes = atom.residue;
			}
		}

		//Compute bonds for other atoms
		if (otherAtoms.Count != 0) {
			// UnityMolBonds otherBonds = ComputeBondsSlidingWindow(otherAtoms);
			// UnityMolBonds otherBonds = ComputeBondsKDtree(otherAtoms);
			UnityMolBonds otherBonds = ComputeBondsKDtreeFudge(atoms, otherAtoms);

			foreach (int ida in otherBonds.bonds.Keys) {
				foreach (int idb in otherBonds.bonds[ida]) {
					if (idb != -1 && ida != idb && !bonds.isBondedTo(ida, idb)) {
						bonds.Add(ida, idb, atoms[0].residue.chain.model);
					}
				}
			}
		}
#if UNITY_EDITOR
		Debug.Log("Time for bonds: " + (1000.0f * (Time.realtimeSinceStartup - start)).ToString("f3") + " ms");
#endif

		return bonds;
	}

	public static UnityMolBonds ComputeNewBondsForAtoms(UnityMolBonds curBonds, List<UnityMolAtom> newAtoms, List<UnityMolAtom> allAtoms) {
		UnityMolBonds result = curBonds;

		int N = newAtoms.Count;
		UnityMolResidue curRes = null;
		List<UnityMolAtom> bonded = new List<UnityMolAtom>(30);//Atom buffer
		List<UnityMolAtom> otherAtoms = new List<UnityMolAtom>();

		for (int i = 0; i < N; i++) {
			UnityMolAtom atom = newAtoms[i];
			curRes = atom.residue;

			UnityMolMain.topologies.getBondedAtomsInResidue(atom, ref bonded);

			if (bonded.Count == 0) {
				otherAtoms.Add(atom);
				continue;
			}

			int nbAdded = 0;
			for (int a = 0; a < bonded.Count - 1; a += 2) {//Bonds for atoms in residue
				if (bonded[a] != bonded[a + 1] && !result.isBondedTo(bonded[a], bonded[a + 1])) {
					result.Add(bonded[a], bonded[a + 1]);
					nbAdded++;
				}
			}

			//For disulfure bonds
			if (curRes != null && atom.type == "S" && curRes.name == "CYS") {
				otherAtoms.Add(atom);
			}

			if (curRes != null && atom.residue.id != curRes.id && atom.residue.id == curRes.id + 1) { //New consecutive residue

				//For proteins !
				pairString crossResAtoms = UnityMolMain.topologies.getPreviousAtomToLink(atom);
				if (crossResAtoms.s1 == null) {
					continue;
				}
				string prevAtomName = crossResAtoms.s1.Replace("-", "");
				string nextAtomName = crossResAtoms.s2;

				if (curRes.atoms.ContainsKey(prevAtomName) && atom.residue.atoms.ContainsKey(nextAtomName)) {
					UnityMolAtom prevAtom = curRes.atoms[prevAtomName];
					UnityMolAtom newAtom = atom.residue.atoms[nextAtomName];
					if (prevAtom != newAtom) {
						float dist = Vector3.Distance(prevAtom.position, newAtom.position);
						if (dist > lowerBondDistance) {
							result.Add(prevAtom, newAtom);
						}
					}
				}
			}
			curRes = atom.residue;
		}

		//Compute bonds for other atoms = not linked to a known residue or S-S bonds
		if (otherAtoms.Count != 0) {
			UnityMolBonds otherBonds = ComputeBondsKDtreeFudge(allAtoms, otherAtoms);

			foreach (int ida in otherBonds.bonds.Keys) {
				foreach (int idb in otherBonds.bonds[ida]) {
					if (idb != -1 && ida != idb && !result.isBondedTo(ida, idb)) {
						result.Add(ida, idb, allAtoms[0].residue.chain.model);
					}
				}
			}
		}



		return result;
	}
}
}