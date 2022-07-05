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

using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using System.Linq;

//Adapted from https://github.com/mdtraj/mdtraj/blob/master/mdtraj/geometry/src/dssp.cpp

namespace UMol {
public class DSSP {

	public static void assignSS_DSSP(UnityMolStructure s) {
		Debug.LogWarning("Using DSSP");
		kabschSander(s);
		s.ssInfoFromFile = false;
	}

	public static bool isProline(UnityMolResidue r) {
		return r.name.ToUpper().StartsWith("PRO");
	}

	public static bool containsCA_N_C_O(UnityMolResidue r) {
		if (!r.atoms.ContainsKey("C") ||
		        !r.atoms.ContainsKey("CA") ||
		        !r.atoms.ContainsKey("O") ||
		        !r.atoms.ContainsKey("N")) {
			return false;
		}

		return true;
	}

	public static List<bool> checkResToSkip(List<UnityMolResidue> residues) {
		List<bool> toSkip = new List<bool>();

		for (int i = 0; i < residues.Count; i++) {
			if (containsCA_N_C_O(residues[i])) {
				toSkip.Add(false);
			}
			else {
				toSkip.Add(true);
			}
		}

		return toSkip;
	}

	public static Dictionary<UnityMolResidue, Vector3> ksAssignHydrogens(List<bool> toSkip, List<UnityMolResidue> residues) {

		Dictionary<UnityMolResidue, Vector3> HatomsModel = new Dictionary<UnityMolResidue, Vector3>();
		try {
			HatomsModel.Add(residues[0], residues[0].atoms["N"].position);
		}
		catch {
			try {
				HatomsModel.Add(residues[0], residues[0].atoms["NT"].position);//For OPEP
			}
			catch {
				toSkip[0] = true;
			}
		}
		for (int i = 1; i < residues.Count; i++) {
			if (!toSkip[i] && !toSkip[i - 1]) {
				Vector3 prevC = residues[i - 1].atoms["C"].position;
				Vector3 prevO = residues[i - 1].atoms["O"].position;
				Vector3 R_CO = prevC - prevO;
				Vector3 N = Vector3.zero;
				if (residues[i].atoms.ContainsKey("N")) {
					N = residues[i].atoms["N"].position;
				}
				else if (residues[i].atoms.ContainsKey("NT")) { //For OPEP
					N = residues[i].atoms["NT"].position;
				}
				Vector3 H = N + R_CO.normalized;
				HatomsModel.Add(residues[i], H);
			}
		}

		return HatomsModel;
	}

	public static void kabschSander(UnityMolStructure s) {
		const float HBOND_ENERGY_CUTOFF = -0.5f;
		const float MINIMAL_CA_DISTANCE2 = 0.81f;

		foreach (UnityMolModel m in s.models) {

			Dictionary<UnityMolResidue, HbondKS> hbonds = new Dictionary<UnityMolResidue, HbondKS>();
			List<DSSP_Bridge.SSType> secondary = new List<DSSP_Bridge.SSType>();

			int idC = 0;
			List<UnityMolResidue> allRes = new List<UnityMolResidue>();
			List<int> idChains = new List<int>();

			foreach (UnityMolChain c in m.chains.Values) {
				// UnityMolResidue[] residues = c.residues.Values.ToArray();
				List<UnityMolResidue> residues = c.residues.Values.ToList();
				foreach (UnityMolResidue r in residues) {
					//Ignore water!
					if (!WaterSelection.waterResidues.Contains(r.name, StringComparer.OrdinalIgnoreCase)) {
						allRes.Add(r);
						idChains.Add(idC);
					}
				}
				idC++;
			}

			List<bool> toSkip = checkResToSkip(allRes);

			Dictionary<UnityMolResidue, Vector3> hatoms = ksAssignHydrogens(toSkip, allRes);
			if (hatoms == null) {
				return;
			}

			for (int i = 0; i < allRes.Count; i++) {
				//Fill secondary with Loop
				secondary.Add(DSSP_Bridge.SSType.SS_LOOP);

				if (toSkip[i]) {
					continue;
				}
				UnityMolAtom iCA = allRes[i].atoms["CA"];

				for (int j = i + 1; j < allRes.Count; j++) {
					if (toSkip[j]) {
						continue;
					}
					UnityMolAtom jCA = allRes[j].atoms["CA"];
					Vector3 r12 = (iCA.position - jCA.position) / 10.0f;

					if (Vector3.Dot(r12, r12) < MINIMAL_CA_DISTANCE2) {
						float e = ksDonorAcceptor(hatoms, allRes, i, j);
						if (e < HBOND_ENERGY_CUTOFF && !isProline(allRes[i])) {
							storeEnergies(hbonds, allRes[i], allRes[j], e);// hbond from donor=ri to acceptor=rj
						}
						if (j != i + 1) {
							float e2 = ksDonorAcceptor(hatoms, allRes, j, i);
							if (e2 < HBOND_ENERGY_CUTOFF && !isProline(allRes[j])) {
								storeEnergies(hbonds, allRes[j], allRes[i], e);// hbond from donor=rj to acceptor=ri
							}
						}
					}
				}

			}


			computeBetaSheets(allRes, idChains, toSkip, hbonds, secondary);
			computeAlphaHelices(allRes, idChains, toSkip, hbonds, secondary);


			for (int r = 0; r < allRes.Count; r++) {
				UnityMolResidue.secondaryStructureType ss = UnityMolResidue.secondaryStructureType.Coil;

				switch (secondary[r]) {
				case DSSP_Bridge.SSType.SS_ALPHAHELIX:
					ss = UnityMolResidue.secondaryStructureType.Helix;
					break;
				case DSSP_Bridge.SSType.SS_STRAND:
					ss = UnityMolResidue.secondaryStructureType.Strand;
					break;
				case DSSP_Bridge.SSType.SS_HELIX_3:
					ss = UnityMolResidue.secondaryStructureType.Helix310;
					break;
				case DSSP_Bridge.SSType.SS_HELIX_5:
					ss = UnityMolResidue.secondaryStructureType.HelixRightPi;
					break;

				}
				allRes[r].secondaryStructure = ss;

			}

		}
	}



	/// <summary>
	/// Compute the Kabsch-Sander hydrogen bond energy between two residues
	/// </summary>
	public static float ksDonorAcceptor(Dictionary<UnityMolResidue, Vector3> hatoms,
	                                    List<UnityMolResidue> residues, int idDon, int idAcc) {

		if (!hatoms.ContainsKey(residues[idDon])) {
			return float.MaxValue;
		}

		Vector4 coupling = new Vector4(-2.7888f, -2.7888f, 2.7888f, 2.7888f); // 332 (kcal*A/mol) * 0.42 * 0.2
		Vector3 NDon = residues[idDon].atoms["N"].position / 10.0f;
		Vector3 HDon = hatoms[residues[idDon]] / 10.0f;
		Vector3 CAcc = residues[idAcc].atoms["C"].position / 10.0f;
		Vector3 OAcc = residues[idAcc].atoms["O"].position / 10.0f;

		Vector3 rHO = HDon - OAcc;
		Vector3 rHC = HDon - CAcc;
		Vector3 rNC = NDon - CAcc;
		Vector3 rNO = NDon - OAcc;

		// Compute all four dot products (each of the squared distances) and pack them into a single fvec4.
		Vector4 d2_HONCHCNO = new Vector4( Vector3.Dot(rHO, rHO),
		                                   Vector3.Dot(rNC, rNC),
		                                   Vector3.Dot(rHC, rHC),
		                                   Vector3.Dot(rNO, rNO));


		Vector4 recipSqrt = new Vector4( 1.0f / Mathf.Sqrt(d2_HONCHCNO.x),
		                                 1.0f / Mathf.Sqrt(d2_HONCHCNO.y),
		                                 1.0f / Mathf.Sqrt(d2_HONCHCNO.z),
		                                 1.0f / Mathf.Sqrt(d2_HONCHCNO.w));

		float energy = Vector4.Dot(coupling, recipSqrt);
		// Debug.Log("energy = "+energy);
		return (energy < -9.9f ? -9.9f : energy);

	}

	/// <summary>
	/// Store a computed hbond energy
	/// </summary>
	public static void storeEnergies(Dictionary<UnityMolResidue, HbondKS> hbonds,
	                                 UnityMolResidue r1, UnityMolResidue r2, float e) {

		if (!hbonds.ContainsKey(r1) || e < hbonds[r1].e) {
			HbondKS hb;
			hb.r2 = r2;
			hb.e = e;
			hbonds[r1] = hb;
		}
		else if (!hbonds.ContainsKey(r2) || e < hbonds[r2].e) {
			HbondKS hb;
			hb.r2 = r1;
			hb.e = e;
			hbonds[r2] = hb;
		}
	}


	/**
	 * Is there an h-bond from donor to acceptor
	 */
	static bool testBonded(UnityMolResidue donor, UnityMolResidue acceptor, Dictionary<UnityMolResidue, HbondKS> hbonds)
	{
		if (!hbonds.ContainsKey(donor)) {
			return false;
		}
		return hbonds[donor].r2 == acceptor;
	}

	/**
	 * Test whether two residues are engaged in a beta-bridge
	 */
	static DSSP_Bridge.BridgeType residueTestBridge(int i, int j,
	        List<UnityMolResidue> residues,
	        List<int> idChains,
	        Dictionary<UnityMolResidue, HbondKS> hbonds)
	{
		int N = residues.Count;
		int a = i - 1; int b = i; int c = i + 1;
		int d = j - 1; int e = j; int f = j + 1;

		if (a >= 0 && c < N && idChains[a] == idChains[c] &&
		        d >= 0 && f < N && idChains[d] == idChains[f]) {

			if ((testBonded(residues[c], residues[e], hbonds) && testBonded(residues[e], residues[a], hbonds)) ||
			        (testBonded(residues[f], residues[b], hbonds) && testBonded(residues[b], residues[d], hbonds)))
				return DSSP_Bridge.BridgeType.BRIDGE_PARALLEL;
			if ((testBonded(residues[c], residues[d], hbonds) && testBonded(residues[f], residues[a], hbonds)) ||
			        (testBonded(residues[e], residues[b], hbonds) && testBonded(residues[b], residues[e], hbonds)))
				return DSSP_Bridge.BridgeType.BRIDGE_ANTIPARALLEL;
		}

		return DSSP_Bridge.BridgeType.BRIDGE_NONE;
	}


	public static void computeBetaSheets(List<UnityMolResidue> allRes,
	                                     List<int> idChains,
	                                     List<bool> toSkip,
	                                     Dictionary<UnityMolResidue, HbondKS> hbonds,
	                                     List<DSSP_Bridge.SSType> secondary) {

		List<DSSP_Bridge> bridges = new List<DSSP_Bridge>();

		for (int i = 1; i < allRes.Count - 4; i++) {
			for (int j = i + 3; j < allRes.Count; j++) {
				DSSP_Bridge.BridgeType type = residueTestBridge(j, i, allRes, idChains, hbonds);

				if (type == DSSP_Bridge.BridgeType.BRIDGE_NONE || toSkip[i] || toSkip[j]) {
					continue;
				}

				bool found = false;
				foreach (DSSP_Bridge b in bridges) {
					if (type != b.type || i != b.i.Last() + 1) {
						continue;
					}
					if (type == DSSP_Bridge.BridgeType.BRIDGE_PARALLEL && b.j.Last() + 1 == j) {
						b.i.AddLast(i);
						b.j.AddLast(j);
						found = true;
						break;
					}

					if (type == DSSP_Bridge.BridgeType.BRIDGE_ANTIPARALLEL && b.j.First() - 1 == j) {
						b.i.AddLast(i);
						b.j.AddFirst(j);
						found = true;
						break;
					}
				}
				if (!found) {
					DSSP_Bridge nb = new DSSP_Bridge(type, idChains[i], idChains[j], i, j);
					bridges.Add(nb);
				}
			}
		}

		bridges.Sort((x, y) => x.compare(y));

		for (int i = 0; i < bridges.Count; i++) {
			for (int j = i + 1; j < bridges.Count; j++) {
				int ibi = bridges[i].i.First();
				int iei = bridges[i].i.Last();
				int jbi = bridges[i].j.First();
				int jei = bridges[i].j.Last();
				int ibj = bridges[j].i.First();
				int iej = bridges[j].i.Last();
				int jbj = bridges[j].j.First();
				int jej = bridges[j].j.Last();

				if ((bridges[i].type != bridges[j].type) ||
				        idChains[Mathf.Min(ibi, ibj)] != idChains[Mathf.Max(iei, iej)] ||
				        idChains[Mathf.Min(jbi, jbj)] != idChains[Mathf.Max(jei, jej)] ||
				        ibj - iei >= 6 || (iei >= ibj && ibi <= iej)) {
					continue;
				}

				bool bulge = false;
				if (bridges[i].type == DSSP_Bridge.BridgeType.BRIDGE_PARALLEL)
					bulge = (jbj > jbi) && ((jbj - jei < 6 && ibj - iei < 3) || (jbj - jei < 3));
				else
					bulge = (jbj < jbi) && ((jbi - jej < 6 && ibj - iei < 3) || (jbi - jej < 3));

				if (bulge) {
					foreach (int val in bridges[j].i) {
						bridges[i].i.AddLast(val);
					}

					if (bridges[i].type == DSSP_Bridge.BridgeType.BRIDGE_PARALLEL) {
						foreach (int val in bridges[j].j) {
							bridges[i].j.AddLast(val);
						}
					}
					else {
						var litem = bridges[j].j.Last;
						while (litem != null) {
							bridges[i].j.AddFirst(litem.Value);

							litem = litem.Previous;
						}
					}
					bridges.RemoveAt(j);
					--j;
				}
			}
		}

		for (int i = 0; i < bridges.Count; i++) {
			DSSP_Bridge b = bridges[i];

			DSSP_Bridge.SSType ss = DSSP_Bridge.SSType.SS_BETABRIDGE;

			if (b.i.Count > 1) {
				ss = DSSP_Bridge.SSType.SS_STRAND;
			}

			foreach (int bi in b.i) {
				if (secondary[bi] != DSSP_Bridge.SSType.SS_STRAND) {
					secondary[bi] = ss;
				}
			}

			foreach (int bj in b.j) {
				if (secondary[bj] != DSSP_Bridge.SSType.SS_STRAND) {
					secondary[bj] = ss;
				}
			}
		}
	}


	public static void computeAlphaHelices(List<UnityMolResidue> allRes,
	                                       List<int> idChains,
	                                       List<bool> toSkip,
	                                       Dictionary<UnityMolResidue, HbondKS> hbonds,
	                                       List<DSSP_Bridge.SSType> secondary) {


		List< List<DSSP_Bridge.helixFlag> > helixFlags = new List< List<DSSP_Bridge.helixFlag>>();

		Dictionary<int, List<int>> chains = new Dictionary<int, List<int>>();
		for (int i = 0; i < allRes.Count; i++) {
			if (!chains.ContainsKey(idChains[i])) {
				chains[idChains[i]] = new List<int>();
			}
			chains[idChains[i]].Add(i);

			List<DSSP_Bridge.helixFlag> hf = new List<DSSP_Bridge.helixFlag>();
			for (int a = 0; a < 6; a++) {
				hf.Add(DSSP_Bridge.helixFlag.HELIX_NONE);
			}
			helixFlags.Add(hf);

		}

		foreach (List<int> residues in chains.Values) {
			for (int stride = 3; stride <= 5; stride++) {
				for (int ii = 0; ii < residues.Count; ii++) {
					int i = residues[ii];

					if ((i + stride) < allRes.Count && testBonded(allRes[i + stride], allRes[i], hbonds) && (idChains[i] == idChains[i + stride])) {

						helixFlags[i + stride][stride] = DSSP_Bridge.helixFlag.HELIX_END;
						for (int j = i + 1; j < i + stride; j++) {
							if (helixFlags[j][stride] == DSSP_Bridge.helixFlag.HELIX_NONE)
								helixFlags[j][stride] = DSSP_Bridge.helixFlag.HELIX_MIDDLE;
						}

						if (helixFlags[i][stride] == DSSP_Bridge.helixFlag.HELIX_END)
							helixFlags[i][stride] = DSSP_Bridge.helixFlag.HELIX_START_AND_END;
						else
							helixFlags[i][stride] = DSSP_Bridge.helixFlag.HELIX_START;
					}
				}
			}
		}

		for (int i = 1; i < allRes.Count - 4; i++) {
			if ((helixFlags[i][4] == DSSP_Bridge.helixFlag.HELIX_START || helixFlags[i][4] == DSSP_Bridge.helixFlag.HELIX_START_AND_END) &&
			        (helixFlags[i - 1][4] == DSSP_Bridge.helixFlag.HELIX_START || helixFlags[i - 1][4] == DSSP_Bridge.helixFlag.HELIX_START_AND_END)) {

				for (int j = i; j <= i + 3; j++)
					secondary[j] = DSSP_Bridge.SSType.SS_ALPHAHELIX;
			}
		}

		for (int i = 1; i < allRes.Count - 3; i++) {
			if ((helixFlags[i][3] == DSSP_Bridge.helixFlag.HELIX_START || helixFlags[i][3] == DSSP_Bridge.helixFlag.HELIX_START_AND_END) &&
			        (helixFlags[i - 1][3] == DSSP_Bridge.helixFlag.HELIX_START || helixFlags[i - 1][3] == DSSP_Bridge.helixFlag.HELIX_START_AND_END)) {

				bool empty = true;
				for (int j = i; empty && j <= i + 2; ++j) {
					empty = (secondary[j] == DSSP_Bridge.SSType.SS_LOOP || secondary[j] == DSSP_Bridge.SSType.SS_HELIX_3);
				}
				if (empty) {
					for (int j = i; j <= i + 2; ++j) {
						secondary[j] = DSSP_Bridge.SSType.SS_HELIX_3;
					}
				}
			}
		}

		for (int i = 1; i < allRes.Count - 5; i++) {
			if ((helixFlags[i][5] == DSSP_Bridge.helixFlag.HELIX_START || helixFlags[i][5] == DSSP_Bridge.helixFlag.HELIX_START_AND_END) &&
			        (helixFlags[i - 1][5] == DSSP_Bridge.helixFlag.HELIX_START || helixFlags[i - 1][5] == DSSP_Bridge.helixFlag.HELIX_START_AND_END)) {

				bool empty = true;
				for (int j = i; empty && j <= i + 4; ++j) {
					empty = (secondary[j] == DSSP_Bridge.SSType.SS_LOOP || secondary[j] == DSSP_Bridge.SSType.SS_HELIX_5 || secondary[j] == DSSP_Bridge.SSType.SS_ALPHAHELIX);
				}
				if (empty) {
					for (int j = i; j <= i + 4; ++j) {
						secondary[j] = DSSP_Bridge.SSType.SS_HELIX_5;
					}
				}
			}
		}

		List<bool> isBend = computeBends(allRes, idChains, toSkip);

		for (int i = 1; i < allRes.Count - 1; i++) {
			if (secondary[i] == DSSP_Bridge.SSType.SS_LOOP && !toSkip[i]) {
				bool isTurn = false;
				for (int stride = 3; stride <= 5 && !isTurn; ++stride)
					for (int k = 1; k < stride && !isTurn; ++k)
						isTurn = (i >= k) && (helixFlags[i - k][stride] == DSSP_Bridge.helixFlag.HELIX_START ||
						                      helixFlags[i - k][stride] == DSSP_Bridge.helixFlag.HELIX_START_AND_END);

				if (isTurn)
					secondary[i] = DSSP_Bridge.SSType.SS_TURN;
				else if (isBend[i])
					secondary[i] = DSSP_Bridge.SSType.SS_BEND;
			}
		}

	}


	/**
	 * Identify bends in the chain, where the kappa angle (virtual bond angle from
	 * c-alpha i-2, to i, to i+2) is greater than 70 degrees
	 * dssp-2.2.0/structure.cpp:1729
	 */
	static List<bool> computeBends(List<UnityMolResidue> allRes,
	                               List<int> idChains,
	                               List<bool> toSkip) {

		List<bool> result = new List<bool>(allRes.Count);
		for (int i = 0; i < allRes.Count; i++) {
			result.Add(false);
		}

		for (int i = 2; i < allRes.Count - 2; i++) {
			if (idChains[i - 2] == idChains[i + 2] && !toSkip[i - 2] && !toSkip[i] && !toSkip[i + 2]) {
				Vector3 prevCA = allRes[i - 2].atoms["CA"].position;
				Vector3 curCA = allRes[i].atoms["CA"].position;
				Vector3 nextCA = allRes[i + 2].atoms["CA"].position;

				Vector3 uPrime = prevCA - curCA;
				Vector3 vPrime = curCA - nextCA;
				float cosAngle = Vector3.Dot(uPrime, vPrime) / Mathf.Sqrt( Vector3.Dot(uPrime, uPrime) * Vector3.Dot(vPrime, vPrime));
				float kappa = Mathf.Acos( Mathf.Clamp(cosAngle, -1.0f, 1.0f));
				result[i] = kappa > (70.0f * (Mathf.PI / 180.0f));
			}
		}
		return result;
	}


	public struct HbondKS {
		public UnityMolResidue r2;
		public float e;
	}




}

/* This struct tracks information about beta bridges and sheets */
public class DSSP_Bridge {

	public BridgeType type;
	public int chainI, chainJ;
	public LinkedList<int> i = new LinkedList<int>();
	public LinkedList<int> j = new LinkedList<int>();



	public DSSP_Bridge(BridgeType t, int cI, int cJ, int firstI, int firstJ) {
		type = t;
		chainI = cI;
		chainJ = cJ;
		i.AddLast(firstI);
		j.AddLast(firstJ);
	}


	public bool isSup(DSSP_Bridge b) {
		return chainI < b.chainI || (chainI == b.chainI && i.First() < b.i.First());
	}

	public int compare(DSSP_Bridge b) {
		if (isSup(b)) {
			return -1;
		}
		if (chainI == b.chainI && i.First() == b.i.First()) {
			return 0;
		}
		return 1;
	}


	public enum helixFlag {
		HELIX_NONE,
		HELIX_START,
		HELIX_END,
		HELIX_START_AND_END,
		HELIX_MIDDLE
	};
	public enum BridgeType {
		BRIDGE_NONE,
		BRIDGE_PARALLEL,
		BRIDGE_ANTIPARALLEL
	};

	public enum SSType {
		SS_LOOP,
		SS_ALPHAHELIX,
		SS_BETABRIDGE,
		SS_STRAND,
		SS_HELIX_3,
		SS_HELIX_5, SS_TURN,
		SS_BEND
	};

}

}