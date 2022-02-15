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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMol.API;
using System;
using System.Linq;


namespace UMol {

public class SugarRibbons {

	public static float thick = 0.1f;
	public static float height = 0.1f;
	public static bool createPlanes = true;

	public static List<Mesh> createSugarRibbons(UnityMolSelection sel, ref Dictionary<UnityMolAtom, List<int>> atomToVertId,
	        float rthickness = 0.1f, float rheight = 0.1f, bool createPlanes = true) {

		thick = rthickness;
		height = rheight;
		UMolGraph g = new UMolGraph();
		g.init(sel);
		List<List<UnityMolAtom>> cycles = g.getAllCycles();

		Mesh newMesh = new Mesh();
		Mesh bbMesh = new Mesh();
		newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
		bbMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

		List<Vector3> vertices = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Color32> newColors = new List<Color32>();

		List<Vector3> verticesBB = new List<Vector3>();
		List<Vector3> normalsBB = new List<Vector3>();
		List<int> trianglesBB = new List<int>();
		List<Color32> newColorsBB = new List<Color32>();
		atomToVertId = new Dictionary<UnityMolAtom, List<int>>();

		for (int i = 0; i < cycles.Count; i++) {
			List<UnityMolAtom> c = cycles[i];

			Vector3 center = cog(c);
			Vector3 normal = computeMeanNormal(c, center);

			if (createPlanes) {
				constructCyclePlanes(c, center, normal, vertices, normals, triangles, newColors);
			}

			constructCycleBackbone(c, center, normal, verticesBB, normalsBB, trianglesBB, newColorsBB, atomToVertId);
		}
		for (int i = 0; i < cycles.Count - 1; i++) {
			for (int j = i + 1; j < cycles.Count; j++) {

				AtomDuo linknext = areCyclesLinked(g, cycles[i], cycles[j]);

				if (linknext != null && linknext.a1 != null && linknext.a2 != null) {

					constructLinkBetweenCycles(linknext.a1, linknext.a2, trianglesBB, atomToVertId);
				}
			}
		}


		newMesh.SetVertices(vertices);
		newMesh.SetTriangles(triangles, 0);
		newMesh.SetColors(newColors);
		newMesh.SetNormals(normals);
		// newMesh.RecalculateNormals();


		bbMesh.SetVertices(verticesBB);
		bbMesh.SetTriangles(trianglesBB, 0);
		bbMesh.SetColors(newColorsBB);
		bbMesh.SetNormals(normalsBB);
		// bbMesh.RecalculateNormals();


		List<Mesh> meshes = new List<Mesh>(2);

		meshes.Add(newMesh);
		meshes.Add(bbMesh);

		return meshes;

	}

	static void constructCyclePlanes(List<UnityMolAtom> cycleAtoms, Vector3 center, Vector3 normal,
	                                 List<Vector3> vertices, List<Vector3> normals, List<int> triangles,
	                                 List<Color32> colors) {

		if (cycleAtoms != null && cycleAtoms.Count > 2) {

			int idCenter = vertices.Count;
			vertices.Add(center);
			normals.Add(normal);
			colors.Add(Color.white);

			//Double face
			vertices.Add(center);
			normals.Add(-normal);
			colors.Add(Color.white);


			for (int i = 0; i < cycleAtoms.Count - 1; i++) {
				int id = vertices.Count;
				vertices.Add(cycleAtoms[i].position);
				vertices.Add(cycleAtoms[i + 1].position);

				normals.Add(normal);
				normals.Add(normal);
				colors.Add(Color.white);
				colors.Add(Color.white);

				triangles.Add(idCenter);
				triangles.Add(id);
				triangles.Add(id + 1);

				//Add the inverted triangle too

				vertices.Add(cycleAtoms[i].position);
				vertices.Add(cycleAtoms[i + 1].position);

				normals.Add(-normal);
				normals.Add(-normal);
				colors.Add(Color.white);
				colors.Add(Color.white);

				triangles.Add(idCenter + 1);
				triangles.Add(id + 3);
				triangles.Add(id + 2);
			}

			int idlast = vertices.Count;
			//Last triangle from last to 0
			vertices.Add(cycleAtoms[0].position);
			vertices.Add(cycleAtoms[cycleAtoms.Count - 1].position);

			normals.Add(normal);
			normals.Add(normal);
			colors.Add(Color.white);
			colors.Add(Color.white);

			triangles.Add(idCenter);
			triangles.Add(idlast);
			triangles.Add(idlast + 1);

			//Add the inverted triangle too
			vertices.Add(cycleAtoms[0].position);
			vertices.Add(cycleAtoms[cycleAtoms.Count - 1].position);

			normals.Add(-normal);
			normals.Add(-normal);
			colors.Add(Color.white);
			colors.Add(Color.white);

			triangles.Add(idCenter + 1);
			triangles.Add(idlast + 3);
			triangles.Add(idlast + 2);

		}
	}

	static void constructCycleBackbone(List<UnityMolAtom> cycleAtoms, Vector3 center, Vector3 normal,
	                                   List<Vector3> vertices, List<Vector3> normals, List<int> triangles,
	                                   List<Color32> colors, Dictionary<UnityMolAtom, List<int>> atomToVertId) {

		if (cycleAtoms != null && cycleAtoms.Count > 2) {
			int idVStart = vertices.Count;

			for (int i = 0; i < cycleAtoms.Count; i++) {

				UnityMolAtom aa2;
				Vector3 a1 = cycleAtoms[i].position;
				Vector3 a2 = Vector3.zero;

				if (i == cycleAtoms.Count - 1) {
					a2 = cycleAtoms[0].position;
					aa2 = cycleAtoms[0];
				}
				else {
					a2 = cycleAtoms[i + 1].position;
					aa2 = cycleAtoms[i + 1];
				}

				Vector3 a1Toa2 = a2 - a1;
				Vector3 normalToa1a2 = Vector3.Cross(a1Toa2, normal).normalized;
				int idV = vertices.Count;
				//out-up
				vertices.Add(a1 + (thick * normalToa1a2) + normal * height);
				//out-down
				vertices.Add(a1 + (thick * normalToa1a2) + -normal * height);

				//in-up
				vertices.Add(a1 - (thick * normalToa1a2) + normal * height);
				//in-down
				vertices.Add(a1 - (thick * normalToa1a2) + -normal * height);

				//out-up
				vertices.Add(a2 + (thick * normalToa1a2) + normal * height);
				//out-down
				vertices.Add(a2 + (thick * normalToa1a2) + -normal * height);

				//in-up
				vertices.Add(a2 - (thick * normalToa1a2) + normal * height);
				//in-down
				vertices.Add(a2 - (thick * normalToa1a2) + -normal * height);


				if (!atomToVertId.ContainsKey(cycleAtoms[i])) {
					atomToVertId[cycleAtoms[i]] = new List<int>();
				}
				atomToVertId[cycleAtoms[i]].Add(idV);
				atomToVertId[cycleAtoms[i]].Add(idV + 1);
				atomToVertId[cycleAtoms[i]].Add(idV + 2);
				atomToVertId[cycleAtoms[i]].Add(idV + 3);

				if (!atomToVertId.ContainsKey(aa2)) {
					atomToVertId[aa2] = new List<int>();
				}
				atomToVertId[aa2].Add(idV + 4);
				atomToVertId[aa2].Add(idV + 5);
				atomToVertId[aa2].Add(idV + 6);
				atomToVertId[aa2].Add(idV + 7);

				normals.Add((normal + normalToa1a2) * 0.5f);
				normals.Add((-normal + normalToa1a2) * 0.5f);

				normals.Add((normal - normalToa1a2) * 0.5f);
				normals.Add((-normal - normalToa1a2) * 0.5f);

				normals.Add((normal + normalToa1a2) * 0.5f);
				normals.Add((-normal + normalToa1a2) * 0.5f);

				normals.Add((normal - normalToa1a2) * 0.5f);
				normals.Add((-normal - normalToa1a2) * 0.5f);

				colors.Add(cycleAtoms[i].color);
				colors.Add(cycleAtoms[i].color);

				colors.Add(cycleAtoms[i].color);
				colors.Add(cycleAtoms[i].color);

				colors.Add(aa2.color);
				colors.Add(aa2.color);

				colors.Add(aa2.color);
				colors.Add(aa2.color);

				triangles.Add(idV);
				triangles.Add(idV + 1);
				triangles.Add(idV + 5);

				triangles.Add(idV);
				triangles.Add(idV + 5);
				triangles.Add(idV + 4);

				triangles.Add(idV + 3);
				triangles.Add(idV + 2);
				triangles.Add(idV + 7);

				triangles.Add(idV + 7);
				triangles.Add(idV + 2);
				triangles.Add(idV + 6);

				//Up
				triangles.Add(idV);
				triangles.Add(idV + 6);
				triangles.Add(idV + 2);

				triangles.Add(idV);
				triangles.Add(idV + 4);
				triangles.Add(idV + 6);


				//Down
				triangles.Add(idV + 1);
				triangles.Add(idV + 3);
				triangles.Add(idV + 7);

				triangles.Add(idV + 1);
				triangles.Add(idV + 7);
				triangles.Add(idV + 5);


				if (i < cycleAtoms.Count - 1) {
					//Out Close the rectangle with in between points
					triangles.Add(idV + 4);
					triangles.Add(idV + 5);
					triangles.Add(idV + 9);

					triangles.Add(idV + 4);
					triangles.Add(idV + 9);
					triangles.Add(idV + 8);

					//In
					triangles.Add(idV + 7);
					triangles.Add(idV + 6);
					triangles.Add(idV + 11);

					triangles.Add(idV + 6);
					triangles.Add(idV + 10);
					triangles.Add(idV + 11);

					//Up
					triangles.Add(idV + 4);
					triangles.Add(idV + 10);
					triangles.Add(idV + 6);

					triangles.Add(idV + 4);
					triangles.Add(idV + 8);
					triangles.Add(idV + 10);

					//Down
					triangles.Add(idV + 5);
					triangles.Add(idV + 7);
					triangles.Add(idV + 11);

					triangles.Add(idV + 5);
					triangles.Add(idV + 11);
					triangles.Add(idV + 9);

				}
				else {
					triangles.Add(idV + 4);
					triangles.Add(idV + 5);
					triangles.Add(idVStart + 1);

					triangles.Add(idV + 4);
					triangles.Add(idVStart + 1);
					triangles.Add(idVStart + 0);

					//In
					triangles.Add(idV + 7);
					triangles.Add(idV + 6);
					triangles.Add(idVStart + 3);

					triangles.Add(idV + 6);
					triangles.Add(idVStart + 2);
					triangles.Add(idVStart + 3);

					//Up
					triangles.Add(idV + 4);
					triangles.Add(idVStart + 2);
					triangles.Add(idV + 6);

					triangles.Add(idV + 4);
					triangles.Add(idVStart + 0);
					triangles.Add(idVStart + 2);

					//Down
					triangles.Add(idV + 5);
					triangles.Add(idV + 7);
					triangles.Add(idVStart + 3);

					triangles.Add(idV + 5);
					triangles.Add(idVStart + 3);
					triangles.Add(idVStart + 1);

				}

			}
		}

	}

	static void constructLinkBetweenCycles(UnityMolAtom a1, UnityMolAtom a2,
	                                       List<int> triangles,
	                                       Dictionary<UnityMolAtom, List<int>> atomToVertId) {


		List<int> vertA1 = atomToVertId[a1];
		List<int> vertA2 = atomToVertId[a2];


		triangles.Add(vertA1[0]);
		triangles.Add(vertA1[1]);
		triangles.Add(vertA2[0]);

		triangles.Add(vertA1[1]);
		triangles.Add(vertA2[1]);
		triangles.Add(vertA2[0]);


		triangles.Add(vertA1[4]);
		triangles.Add(vertA2[4]);
		triangles.Add(vertA1[5]);

		triangles.Add(vertA2[4]);
		triangles.Add(vertA2[5]);
		triangles.Add(vertA1[5]);


		triangles.Add(vertA1[4]);
		triangles.Add(vertA1[0]);
		triangles.Add(vertA2[0]);

		triangles.Add(vertA2[0]);
		triangles.Add(vertA2[4]);
		triangles.Add(vertA1[4]);


		triangles.Add(vertA1[1]);
		triangles.Add(vertA1[5]);
		triangles.Add(vertA2[1]);

		triangles.Add(vertA1[5]);
		triangles.Add(vertA2[5]);
		triangles.Add(vertA2[1]);

	}

	static AtomDuo areCyclesLinked(UMolGraph g, List<UnityMolAtom> c1, List<UnityMolAtom> c2, int maxDistLink = 3) {
		AtomDuo res = null;

		//One atom of c1 is part of the same connected component as one atom from the cycle c2
		int segc1id = g.getSegmentId(c1[0]);
		int segc2id = g.getSegmentId(c2[0]);

		if (segc1id == segc2id) {
			//Get one path from one atom of the cycle c1 to one atom of the cycle c2
			List<UnityMolAtom> path = g.getPath(segc1id, c1[0], c2[0]);
			UnityMolAtom lastc2 = null;
			UnityMolAtom firstc1 = null;
			int d = 0;
			foreach (UnityMolAtom a in path) {
				if (c2.Contains(a)) {
					lastc2 = a;
				}
				else if (c1.Contains(a)) {
					firstc1 = a;
					break;
				}
				else {
					d++;
				}
			}
			if (d > maxDistLink)
				return res;
			res = new AtomDuo(firstc1, lastc2);
		}
		return res;
	}


	static Vector3 cog(List<UnityMolAtom> cycleAtoms) {
		Vector3 p = Vector3.zero;

		foreach (UnityMolAtom a in cycleAtoms) {
			p += a.position;
		}
		return p / Mathf.Max(1, cycleAtoms.Count);
	}

	static Vector3 computeMeanNormal(List<UnityMolAtom> cycleAtoms, Vector3 c) {

		Vector3 n = Vector3.zero;
		for (int i = 0; i < cycleAtoms.Count - 1; i++) {
			Vector3 cToa1 = cycleAtoms[i].position - c;
			Vector3 cToa2 = cycleAtoms[i + 1].position - c;
			n += Vector3.Cross(cToa1, cToa2);

			if (i == cycleAtoms.Count - 1) {
				cToa1 = cycleAtoms[i].position - c;
				cToa2 = cycleAtoms[0].position - c;
				n += Vector3.Cross(cToa1, cToa2);
			}
		}

		return (n / Mathf.Max(1, cycleAtoms.Count)).normalized;
	}



	public class UMolGraph {

		public List<List<UnityMolAtom>> segments = null;
		int nbCycles = 0;
		Dictionary<UnityMolAtom, GNode> dicNode;
		UnityMolSelection selection;
		List<UnityMolAtom> curList = new List<UnityMolAtom>();


		public void init(UnityMolSelection sel) {
			selection = sel;
		}

		void findAllCycles(UnityMolAtom u, UnityMolAtom p) {

			// Already completely visited
			if (dicNode.ContainsKey(u) && dicNode[u].nodeCol == 2) {
				return;
			}

			// seen vertex, but was not completely visited -> cycle detected.
			// backtrack based on parents to find the complete cycle.
			if (dicNode.ContainsKey(u) && dicNode[u].nodeCol == 1) {
				nbCycles++;
				UnityMolAtom cur = p;
				dicNode[cur].nodeMark = nbCycles;

				// backtrack the vertex which are
				// in the current cycle thats found
				while (cur != u) {
					cur = dicNode[cur].nodePar;
					dicNode[cur].nodeMark = nbCycles;
				}
				return;
			}

			if (!dicNode.ContainsKey(u)) {
				dicNode[u] = new GNode();
			}
			dicNode[u].nodePar = p;
			// partially visited.
			dicNode[u].nodeCol = 1;

			// simple dfs on graph
			if (selection.bonds.bondsDual.ContainsKey(u)) {
				foreach (UnityMolAtom v in selection.bonds.bondsDual[u]) {
					if (v != null) {
						if (v == dicNode[u].nodePar) {
							continue;
						}
						findAllCycles(v, u);
					}
				}
			}

			dicNode[u].nodeCol = 2;
		}

		public List<List<UnityMolAtom>> getAllCycles() {

			//Search connected components to make sure we find all the cycles even in seperated chains
			if (segments == null) {
				segments = getConnectedCompo();
			}

			dicNode = new Dictionary<UnityMolAtom, GNode>();


			foreach (List<UnityMolAtom> ats in segments) {
				AtomDuo first = getFirstBond(ats);
				if (first != null) {
					findAllCycles(first.a1, first.a2);
				}
			}

			List<List<UnityMolAtom>> resCycles = new List<List<UnityMolAtom>>();
			Dictionary<int, int> markToListId = new Dictionary<int, int>();

			foreach (UnityMolAtom a in dicNode.Keys) {
				int mark = dicNode[a].nodeMark;
				if (mark > 0) {
					if (markToListId.ContainsKey(mark)) {
						resCycles[markToListId[mark]].Add(a);
					}
					else {
						int id = resCycles.Count;
						markToListId[mark] = id;
						List<UnityMolAtom> newC = new List<UnityMolAtom>();
						newC.Add(a);
						resCycles.Add(newC);
					}
				}
			}

			List<List<UnityMolAtom>> resCyclesfilter = new List<List<UnityMolAtom>>();
			foreach (List<UnityMolAtom> c in resCycles) {
				if (c.Count >= 3) {
					resCyclesfilter.Add(c);
				}
			}


			return resCyclesfilter;
		}
		public int getSegmentId(UnityMolAtom a) {
			int id = 0;
			foreach (List<UnityMolAtom> s in segments) {
				if (s.Contains(a)) {
					return id;
				}
				id++;
			}
			return -1;
		}

		AtomDuo getFirstBond(List<UnityMolAtom> atoms) {
			if (atoms.Count < 3) {
				return null;
			}

			foreach (UnityMolAtom firstA in atoms) {
				if (selection.bonds.bondsDual.ContainsKey(firstA)) {
					foreach (UnityMolAtom a in selection.bonds.bondsDual[firstA]) {
						if (a != null && selection.bonds.bondsDual.ContainsKey(a)) {
							foreach (UnityMolAtom b in selection.bonds.bondsDual[a]) {
								if (b != null) {
									int count = selection.bonds.countBondedAtoms(a);
									if (count > 1) {
										return new AtomDuo(a, b);
									}
								}
							}
						}
					}
				}
			}
			return null;
		}

		List<List<UnityMolAtom>> getConnectedCompo() {
			List<List<UnityMolAtom>> res = new List<List<UnityMolAtom>>();
			HashSet<UnityMolAtom> visited = new HashSet<UnityMolAtom>();

			foreach (UnityMolAtom a in selection.atoms) {

				if (!visited.Contains(a)) {
					DFSUtil(a, visited);
					if (curList.Count > 1) {
						res.Add(new List<UnityMolAtom>(curList));
					}
					curList.Clear();
				}
			}

			return res;
		}

		void DFSUtil(UnityMolAtom v, HashSet<UnityMolAtom> visited) {
			visited.Add(v);
			curList.Add(v);

			if (selection.bonds.bondsDual.ContainsKey(v)) {
				foreach (UnityMolAtom x in selection.bonds.bondsDual[v]) {
					if (x != null && !visited.Contains(x)) {
						DFSUtil(x, visited);
					}
				}
			}
		}

		// Assumes that start and stop are in the segment segId
		public List<UnityMolAtom> getPath(int segId, UnityMolAtom start, UnityMolAtom stop) {
			HashSet<UnityMolAtom> visited = new HashSet<UnityMolAtom>();

			if (segId < 0 || segId >= segments.Count) {
				return null;
			}

			List<UnityMolAtom> res = new List<UnityMolAtom>();
			List<UnityMolAtom> q = new List<UnityMolAtom>();
			Dictionary<UnityMolAtom, UnityMolAtom> pred = new Dictionary<UnityMolAtom, UnityMolAtom>();
			visited.Add(start);
			q.Add(start);

			bool found = false;
			while (q.Count != 0) {
				if (found) {
					break;
				}
				UnityMolAtom a = q.First();
				q.RemoveAt(0);
				if (selection.bonds.bondsDual.ContainsKey(a)) {
					foreach (UnityMolAtom b in selection.bonds.bondsDual[a]) {
						if (b != null && !visited.Contains(b)) {
							visited.Add(b);
							pred[b] = a;
							q.Add(b);

							if (b == stop) {
								found = true;
								break;
							}

						}
					}
				}
			}

			if (found) {
				res.Add(stop);
				UnityMolAtom a = stop;
				while (a != start) {
					res.Add(pred[a]);
					a = pred[a];
				}
				return res;
			}

			return null;
		}

		public class GNode {
			public int nodeCol = -1;
			public int nodeMark = -1;
			public UnityMolAtom nodePar = null;
		}

	}
}
}