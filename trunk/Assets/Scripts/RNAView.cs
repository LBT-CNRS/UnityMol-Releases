using UnityEngine;
using System.Collections;
using Molecule.Model;
using System.Collections.Generic;

public class RNAView : MonoBehaviour {
	public struct Hbparam
	{
		public float dref;
		public float alpa;
		public float alpb;
		public int s;
		
		public Hbparam(float dreference, float alphaa, float alphab, int sv)
		{
			dref = dreference;
			alpa = alphaa;
			alpb = alphab;
			s = sv;
		}
	}

	private static Hbparam[][][] hb_params;

	private static Dictionary<string, int> bases = new Dictionary<string, int>()
	{
		{"G2", 0},
		{"A2", 1},
		{"C1", 2},
		{"U1", 3}
	};
	
	public static void RNAView_init() {
		hb_params = new Hbparam[4][][];
		
		hb_params[0] = new Hbparam[4][];
		hb_params[0][0] = new Hbparam[2];
		hb_params[0][0][0] = new Hbparam(6.24f, 2.77f, 1.20f, 2);
		hb_params[0][0][1] = new Hbparam(7.33f, 2.93f, 1.27f, 1);
		
		hb_params[0][1] = new Hbparam[4];
		hb_params[0][1][0] = new Hbparam(5.07f, 2.67f, 2.95f, 2);
		hb_params[0][1][1] = new Hbparam(6.26f, 1.48f, 1.20f, 2);
		hb_params[0][1][2] = new Hbparam(7.02f, 1.93f, 2.06f, 1);
		hb_params[0][1][3] = new Hbparam(7.54f, 2.14f, 0.80f, 1);
		
		hb_params[0][2] = new Hbparam[2];
		hb_params[0][2][0] = new Hbparam(4.80f, 2.76f, 2.17f, 3);
		hb_params[0][2][1] = new Hbparam(7.40f, 1.28f, 2.89f, 1);
		
		hb_params[0][3] = new Hbparam[2];
		hb_params[0][3][0] = new Hbparam(5.67f, 2.15f, 1.72f, 2);
		hb_params[0][3][1] = new Hbparam(7.05f, 2.85f, 1.43f, 1);
		
		hb_params[1] = new Hbparam[4][];
		
		hb_params[1][0] = new Hbparam[4];
		hb_params[1][0] = hb_params[0][1];
		
		hb_params[1][1] = new Hbparam[4];
		hb_params[1][1][0] = new Hbparam(5.43f, 2.55f, 2.55f, 2);
		hb_params[1][1][1] = new Hbparam(7.33f, 1.01f, 1.73f, 1);
		hb_params[1][1][2] = new Hbparam(6.86f, 0.98f, 0.98f, 2);
		hb_params[1][1][3] = new Hbparam(7.33f, 1.73f, 1.01f, 1);
		
		hb_params[1][2] = new Hbparam[2];
		hb_params[1][2][0] = new Hbparam(5.60f, 2.40f, 1.82f, 1);
		hb_params[1][2][1] = new Hbparam(6.94f, 2.07f, 1.48f, 1);
		
		hb_params[1][3] = new Hbparam[2];
		hb_params[1][3][0] = new Hbparam(4.96f, 2.92f, 2.23f, 2);
		hb_params[1][3][1] = new Hbparam(6.43f, 0.84f, 1.95f, 2);
		
		hb_params[2] = new Hbparam[4][];
		
		hb_params[2][0] = new Hbparam[2];
		hb_params[2][0] = hb_params[0][2];
		
		hb_params[2][1] = new Hbparam[2];
		hb_params[2][1] = hb_params[1][2];
		
		hb_params[2][2] = new Hbparam[1];
		hb_params[2][2][0] = new Hbparam(4.91f, 2.22f, 2.24f, 2);
		
		hb_params[2][3] = new Hbparam[2];
		hb_params[2][3][0] = new Hbparam(5.62f, 1.75f, 2.64f, 1);
		hb_params[2][3][1] = new Hbparam(7.48f, 2.88f, 2.71f, 1);
		
		hb_params[3] = new Hbparam[4][];
		
		hb_params[3][0] = new Hbparam[2];
		hb_params[3][0] = hb_params[0][3];
		
		hb_params[3][1] = new Hbparam[2];
		hb_params[3][1] = hb_params[1][3];
		
		hb_params[3][2] = new Hbparam[2];
		hb_params[3][2] = hb_params[2][3];
		
		hb_params[3][3] = new Hbparam[2];
		hb_params[3][3][0] = new Hbparam(5.39f, 1.71f, 2.61f, 1);
		hb_params[3][3][1] = new Hbparam(7.33f, 2.61f, 2.39f, 1);
	}
	
	private static float Ehbond(float dij, float alpa, float alpb, Hbparam hbp)
	{
//		Debug.Log ("Entering Ehbond");
		
		alpa = Mathf.Cos (alpa - hbp.alpa);
		alpb = Mathf.Cos (alpb - hbp.alpb);
		
//		Debug.Log("alpa: " + alpa);
//		Debug.Log("alpb: " + alpb);
		
		float r = (dij - hbp.dref) / MoleculeModel.scale_RNA[12];
//		Debug.Log ("SCALE_RNA[12]" + MoleculeModel.scale_RNA[12]);
		float Vr = -hbp.s * Mathf.Exp(-r * r);
		float Vangl = Mathf.Pow(alpa * alpb, MoleculeModel.scale_RNA[11]);
		
//		Debug.Log("r: " + r);
//		Debug.Log("Vr: " + Vr);
//		Debug.Log("Vangl: " + Vangl);
//		
//		Debug.Log ("Exiting Ehbond");
		return MoleculeModel.scale_RNA[9] * Vr * Vangl;
	}
	
	private static float angle(Vector3 a, Vector3 b, Vector3 c) {
		Vector3 u = a - b;
		Vector3 v = c - b;
		
		float w = Vector3.Dot(u, v);
		w /= (u.magnitude * v.magnitude);
		return Mathf.Acos(w);
	}
	
	private static float PointToPlaneDistance(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 x) {
		Vector3 n = Vector3.Cross (p2-p1, p3-p1);
		n.Normalize();
		return Vector3.Dot(n, x-p2);
	}
	
	private static float NewPlane(Vector3 a, Vector3 b, Vector3 c, Vector3 d) {
		float ppDistance = PointToPlaneDistance(a, b, c, d);
		return MoleculeModel.scale_RNA[14] * Mathf.Exp(-Mathf.Pow(ppDistance / MoleculeModel.scale_RNA[13], 2)) / 6.0f;
	}
	
	private static float ENewPlane(int atomId1, int atomId2) {
		float Enp1 = 0;
		float Enp2 = 0;
		
		for (int i = 0; i < 3; i++)
		{
			Enp1 += NewPlane(
				MoleculeModel.atomsMDDriverLocationlist[atomId1-2],
				MoleculeModel.atomsMDDriverLocationlist[atomId1-1],
				MoleculeModel.atomsMDDriverLocationlist[atomId1],
				MoleculeModel.atomsMDDriverLocationlist[atomId2-i]
				);
				
			Enp2 += NewPlane(
				MoleculeModel.atomsMDDriverLocationlist[atomId2-2],
				MoleculeModel.atomsMDDriverLocationlist[atomId2-1],
				MoleculeModel.atomsMDDriverLocationlist[atomId2],
				MoleculeModel.atomsMDDriverLocationlist[atomId1-i]
				);
		}
//		Debug.Log ("Enp: " + Enp1 + " " + Enp2);
		return Enp1 * Enp2;
	}
	
	private static float computeEnergyForBasePair(int residueId1, int residueId2) {
//		Debug.Log ("---- " + residueId1 + " " + residueId2);
		int i = MoleculeModel.baseIdx[residueId1-1];
		int j = MoleculeModel.baseIdx[residueId2-1];
		
		Vector3 posi = MoleculeModel.atomsMDDriverLocationlist[i];
		Vector3 posj = MoleculeModel.atomsMDDriverLocationlist[j];
		
		float dij = Vector3.Distance(posi, posj);
//		Debug.Log ("Distance " + residueId1 + " " + residueId2 + " " + dij);
		if (dij > 15)
		{
			return 0.0f;
		}
		
		float Enp = ENewPlane(i, j);
		
		float ca = angle(
			MoleculeModel.atomsMDDriverLocationlist[i-1],
			MoleculeModel.atomsMDDriverLocationlist[i],
			MoleculeModel.atomsMDDriverLocationlist[j]
			);
		float cb = angle(
			MoleculeModel.atomsMDDriverLocationlist[i],
			MoleculeModel.atomsMDDriverLocationlist[j],
			MoleculeModel.atomsMDDriverLocationlist[j-1]
			);
		
//		Debug.Log ("Enp " + Enp + " ca " + ca + " cb " + cb);
			
		float Ehb = 0.0f;
		Hbparam param;
		
		string resA = MoleculeModel.atomsNamelist[i];
		string resB = MoleculeModel.atomsNamelist[j];
		int resAVal = bases[resA];
		int resBVal = bases[resB];
//		Debug.Log ("----------");
//		Debug.Log (hb_params[0]);
//		Debug.Log (hb_params.Length);
//		Debug.Log (resAVal);
//		Debug.Log (resBVal);
		Hbparam[] parameterList = hb_params[resAVal][resBVal];
		
		for(int k = 0; k < parameterList.Length; k++)
		{
			param = parameterList[k];
			Ehb += Ehbond(dij, ca, cb, param);
		}
		
//		Debug.Log ("Ehb " + Ehb);
//		Debug.Break();
			
		return Ehb * Enp;
	}
	
	public static List<int[]> findHbonds()
	{
		List<int[]> bonds = new List<int[]>();
		float threshold = 0.1f;
		foreach(KeyValuePair<int, ArrayList> entry1 in MoleculeModel.residues)
		{
			foreach(KeyValuePair<int, ArrayList> entry2 in MoleculeModel.residues)
			{
				int atom1;
				int atom2;
				atom1 = (int)entry1.Value[entry1.Value.Count - 1];
				atom2 = (int)entry2.Value[entry2.Value.Count - 1];
				
				string base1;
				string base2;
				base1 = MoleculeModel.atomsNamelist[atom1];
				base2 = MoleculeModel.atomsNamelist[atom2];
				
				float energy;
				if (bases[base1] > bases[base2])
				{
					energy = computeEnergyForBasePair(entry2.Key, entry1.Key);
				}
				else
				{
					energy = computeEnergyForBasePair(entry1.Key, entry2.Key);
				}
				
//				Debug.Log(energy);
				if (energy < -threshold)
				{
					bonds.Add(new int[]{entry1.Key, entry2.Key});
				}
			}
		}
		return bonds;
	}
	
	
	
	
	
	
	
	
	
//	public static compute
//	public static Vector3 computeBarycenter(List<GraphVertex> atoms)
//	{
//		Vector3 b = new Vector3(0.0f, 0.0f, 0.0f);
//		for (int i = 0; i < atoms.Count; i++) {
//			float[] atomPosition =  MoleculeModel.atomsLocationlist[atoms[i].id];
//			b.x += atomPosition[0];
//			b.y += atomPosition[1];
//			b.z += atomPosition[2];
//		}
//		b /= atoms.Count;
//		return b;
//	}
//	
//	public static List<List<GraphVertex>> getBases()
//	{
//		List<List<GraphVertex>> bases = new List<List<GraphVertex>>();
//		List<GraphVertex> graph = GraphGenerator.GenerateGraph(true);
//		
//		/*
//		 * Find nitrogeneous base for each nucleotide
//		 */
//		int l = MoleculeModel.residues.Count;
//		foreach (KeyValuePair<int, ArrayList> entry in MoleculeModel.residues)
//		{
//			List<GraphVertex> currentBase = new List<GraphVertex>();
//			List<GraphVertex> queue = new List<GraphVertex>();
//			
//			for (int i = 0; i < entry.Value.Count; i++)
//			{
//				int obj = (int)entry.Value[i];
//				
//				if (    (MoleculeModel.atomsTypelist[obj].type == "N")
//				    && (graph[obj].neighbor.Count == 3)
//				    && (graph[obj].neighbor[0].type == 'C')
//				    && (graph[obj].neighbor[1].type == 'C')
//				    && (graph[obj].neighbor[2].type == 'C'))
//				{
//					graph[obj].flag = true;
//					currentBase.Add(graph[obj]);
//					queue.Add (graph[obj].neighbor[0]);
//					queue.Add (graph[obj].neighbor[1]);
//					queue.Add (graph[obj].neighbor[2]);
//					
//					break;
//				}
//			}
//			
//			if (queue.Count == 3) {
//				Debug.Log ("#############");
//				GraphVertex seed = currentBase[0];
//				for (int i = 0; i < queue.Count; i++) {
//					bool keep = false;
//					List<GraphVertex> neighbors = queue[i].neighbor;
//					Debug.Log ("-------" + queue[i].id);
//					// Check neighborhood
//					for (int j = 0; j < neighbors.Count; j++) {
//						GraphVertex n = neighbors[j];
//						if (n.flag == false && n.type == 'N') {
//							Debug.Log ("Keeping set flag to true");
//							keep = true;
//							break;
//						}
//					}
//					
//					if (keep) {
//						Debug.Log ("Keeping");
//						seed = graph[queue[i].id];
//						seed.flag = true;
//						currentBase.Add (queue[i]);
//						break;
//					}
//				}
//				
//				queue.Clear();
//				queue.Add (seed);
//				
//				Debug.Log (currentBase.Count);
//				Debug.Log (seed.flag);
//				
//				for (int i = 0; i < seed.neighbor.Count; i++) {
//					Debug.Log (seed.neighbor[i].flag);
//				}
//				Debug.Log("%%%%%%%%%");
//				while (queue.Count > 0) {
//					GraphVertex current = queue[0];
//					queue.RemoveAt (0);
//					List<GraphVertex> neighbors = current.neighbor;
//					GraphVertex currentNeighbor;
//					for (int i = 0; i < neighbors.Count; i++) {
//						currentNeighbor = neighbors[i];
//						Debug.Log (currentNeighbor.flag);
//						if (currentNeighbor.flag == false && entry.Value.Contains(currentNeighbor.id)) {
//							Debug.Log ("Queuing");
//							currentNeighbor.flag = true;
//							queue.Add (currentNeighbor);
//							currentBase.Add (currentNeighbor);
//						}
//					}
//				}
//			}
//			bases.Add(currentBase);
//		}
//		return bases;
//	}	
	
}
