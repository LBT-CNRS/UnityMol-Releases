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
using System.Collections;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Mathematics;
using KNN;
using KNN.Jobs;
using Unity.Jobs;

namespace UMol {

public class ComputeUnityMolBonds {
	public static float lowerBondDistance = 0.1f;
	public static float fudgeFactor = 0.62f;


	// public static UnityMolBonds ComputeBondsGridFudge(List<UnityMolAtom> allAtoms, List<UnityMolAtom> atoms) {
	// 	float maxVDW = UnityMolMain.atomColors.getMaxRadius(atoms);

	// 	int N = atoms.Count;
	// 	UnityMolBonds bonds = new UnityMolBonds();
	// 	LightResidueComparer lrc = new LightResidueComparer();

	// 	//-------------------- C# Burst compiled implementation

	// 	var allPoints = new NativeArray<float3>(allAtoms.Count, Allocator.Persistent);
	// 	var atomPoints = new NativeArray<float3>(N, Allocator.Persistent);
	// 	for (int i = 0; i < allAtoms.Count; i++) {
	// 		allPoints[i] = allAtoms[i].position;
	// 	}
	// 	for (int i = 0; i < N; i++) {
	// 		atomPoints[i] = atoms[i].position;
	// 	}

	// 	int maxRes = 15;
	// 	float cutoff = maxVDW;

	// 	//Create grid

	// 	NeighborSearchGridBurst nsgb = new NeighborSearchGridBurst();

	// 	NativeArray<int> results = nsgb.getPointsInRadius(allPoints, atomPoints, maxRes, cutoff);

	// 	for (int i = 0; i < atomPoints.Length; i++) {
	// 		UnityMolAtom atom = atoms[i];
	// 		Vector3 atom0 = atom.position;

	// 		float maxDist = (atom.radius + maxVDW) * fudgeFactor;

	// 		for (int n = 0; n < maxRes; n++) {

	// 			int id = results[i * maxRes + n];
	// 			if (id < 0 || id >= allAtoms.Count) {
	// 				break;
	// 			}
	// 			UnityMolAtom atom2 = allAtoms[id];

	// 			if (atom2 == atom) {
	// 				continue;
	// 			}

	// 			//Don't bind atoms from different residues
	// 			if (!lrc.Equals(atom.residue, atom2.residue)) {
	// 				continue;
	// 			}

	// 			float dist = Vector3.Distance(atom0, atom2.position);
	// 			float accept = (atom.radius + atom2.radius) * fudgeFactor;

	// 			if (dist > maxDist || dist < 0.01f) {
	// 				continue;
	// 			}

	// 			// Specific VDW radius for hydrogen, different from the one we show
	// 			if (atom.type == "H") {
	// 				accept = (0.4f + atom2.radius) * fudgeFactor;
	// 			}
	// 			if (atom2.type == "H") {
	// 				accept = (0.4f + atom.radius) * fudgeFactor;
	// 			}

	// 			if (dist > lowerBondDistance && dist <= accept) {

	// 				if (!bonds.isBondedTo(atom, atom2) && !(atom.type == "H" && atom2.type == "H")) {
	// 					bonds.Add(atom, atom2);
	// 				}
	// 			}

	// 		}
	// 	}

	// 	results.Dispose();
	// 	allPoints.Dispose();
	// 	atomPoints.Dispose();

	// 	return bonds;
	// }

	public static UnityMolBonds ComputeBondsKDtreeFudge(List<UnityMolAtom> allAtoms, List<UnityMolAtom> atoms) {
		//Adapted from MDAnalysis: https://github.com/MDAnalysis/mdanalysis/blob/36e12e98c958580d263c3e7c54f43c5565171787/package/MDAnalysis/topology/guessers.py
		float maxVDW = UnityMolMain.atomColors.getMaxRadius(atoms);

		int N = atoms.Count;
		UnityMolBonds bonds = new UnityMolBonds();
		LightResidueComparer lrc = new LightResidueComparer();

		if (atoms.Count < 3) { //Use the naive implementation

			for (int i = 0; i < atoms.Count; i++) {
				UnityMolAtom atom = atoms[i];
				Vector3 atom0 = atom.position;
				float maxDist = (atom.radius + maxVDW) * fudgeFactor;

				for (int j = 0; j < allAtoms.Count; j++) {
					UnityMolAtom atom2 = allAtoms[j];

					if (atom2 == atom) {
						continue;
					}

					//Don't bind atoms from different residues
					if (!lrc.Equals(atom.residue, atom2.residue)) {
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

			return bonds;
		}

		//-------------------- C# Burst compiled implementation

		var allPoints = new NativeArray<float3>(allAtoms.Count, Allocator.Persistent);
		var atomPoints = new NativeArray<float3>(N, Allocator.Persistent);
		for (int i = 0; i < allAtoms.Count; i++) {
			allPoints[i] = allAtoms[i].position;
		}
		for (int i = 0; i < N; i++) {
			atomPoints[i] = atoms[i].position;
		}

		int kNeighbours = 15;

		//Create KDTree
		var knnContainer = new KnnContainer(allPoints, true, Allocator.TempJob);
		var queryPositions = new NativeArray<float3>(atomPoints, Allocator.TempJob);
		var results = new NativeArray<int>(N * kNeighbours, Allocator.TempJob);

		var batchQueryJob = new QueryKNearestBatchJob(knnContainer, queryPositions, results);
		var handle = batchQueryJob.ScheduleBatch(queryPositions.Length, Mathf.Max(16, queryPositions.Length / 32));
		handle.Complete();

		for (int i = 0; i < N; i++) {
			UnityMolAtom atom = atoms[i];
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
				if (!lrc.Equals(atom.residue, atom2.residue)) {
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

		knnContainer.Dispose();
		queryPositions.Dispose();
		results.Dispose();
		allPoints.Dispose();
		atomPoints.Dispose();

		return bonds;
	}

	public static UnityMolBonds ComputeBondsByResidue(List<UnityMolAtom> atoms) {
		float start = Time.realtimeSinceStartup;

		UnityMolBonds bonds = new UnityMolBonds();

		int N = atoms.Count;
		UnityMolResidue curRes = null;
		List<UnityMolAtom> otherAtoms = new List<UnityMolAtom>();
		HashSet<UnityMolAtom> inParsedConnectivity = new HashSet<UnityMolAtom>();

		if (atoms[0].residue.chain.model.structure.parsedConnectivity != null) { //Use parsed connectivity
			UnityMolModel curMod = atoms[0].residue.chain.model;
			Dictionary<long, UnityMolAtom> atomIdToAtom = new Dictionary<long, UnityMolAtom>();
			foreach (UnityMolAtom a in curMod.allAtoms) {
				atomIdToAtom[a.number] = a;
			}
			foreach (Int2 b in atoms[0].residue.chain.model.structure.parsedConnectivity) {
				if (!atomIdToAtom.ContainsKey(b.x)) {
					Debug.LogWarning("Couldn't find atom " + b.x + " -> ignoring this bond");
					continue;
				}
				if (!atomIdToAtom.ContainsKey(b.y)) {
					Debug.LogWarning("Couldn't find atom " + b.y + " -> ignoring this bond");
					continue;
				}
				UnityMolAtom a1 = atomIdToAtom[b.x];
				UnityMolAtom a2 = atomIdToAtom[b.y];

				if (!inParsedConnectivity.Contains(a1))
					inParsedConnectivity.Add(a1);
				if (!inParsedConnectivity.Contains(a2))
					inParsedConnectivity.Add(a2);

				if (!bonds.isBondedTo(a1, a2)) {
					bonds.Add(a1, a2);
				}
			}
		}

		for (int i = 0; i < N; i++) {
			UnityMolAtom atom = atoms[i];

			//If we have information in the PDB CONECT field ignore this atom
			if (atom.isHET && !inParsedConnectivity.Contains(atom)) {
				otherAtoms.Add(atom);
				continue;
			}


			List<UnityMolAtom> bonded = UnityMolMain.topologies.getBondedAtomsInResidue(atom);

			if (bonded.Count == 0) {
				otherAtoms.Add(atom);
				continue;
			}


			int nbAdded = 0;
			for (int a = 0; a < bonded.Count - 1; a += 2) {
				if (!bonds.isBondedTo(bonded[a], bonded[a + 1])) {
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

					float dist = Vector3.Distance(prevAtom.position, newAtom.position);
					if (dist > lowerBondDistance) {
						bonds.Add(prevAtom, newAtom);
					}
				}

			}
			curRes = atom.residue;
		}

		//Compute bonds for other atoms
		if (otherAtoms.Count != 0) {
			// UnityMolBonds otherBonds = ComputeBondsSlidingWindow(otherAtoms);
			// UnityMolBonds otherBonds = ComputeBondsKDtree(otherAtoms);
			UnityMolBonds otherBonds = ComputeBondsKDtreeFudge(atoms, otherAtoms);


			foreach (UnityMolAtom a in otherBonds.bonds.Keys) {
				foreach (UnityMolAtom b in otherBonds.bonds[a]) {
					if (b != null && !bonds.isBondedTo(a, b)) {
						bonds.Add(a, b);
					}
				}
			}
		}
#if UNITY_EDITOR
		Debug.Log("Time for bonds: " + (1000.0f * (Time.realtimeSinceStartup - start)).ToString("f3") + " ms");
#endif

		return bonds;
	}
}
}