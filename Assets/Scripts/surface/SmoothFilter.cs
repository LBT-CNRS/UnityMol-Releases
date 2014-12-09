using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 
/*
    MeshSmoothTest
 
	Laplacian Smooth Filter, HC-Smooth Filter
 
	MarkGX, Jan 2011
*/
public class SmoothFilter : MonoBehaviour {
	private static float NUDGE_COEF = 0.05f;
	private static float BETA = 0.85f;
	private static float ONE_MINUS_BETA = 1f - BETA;
	
	// The HCSmoothing algorithm tries to compensate for the shrinking induced by the Laplacian
	// algorithm. The extent to which it does this is governed by this variable. If it is very small,
	// e.g. in [0;2], the algorithm will produce results similar to the Laplacian filter. If it is around
	// 0.5 or higher, the compensation will be more noticeable, but the smoothing effect will not be as good.
	private static float HC_CORRECT = 0.2f;
	
	
	/*
		Standard Laplacian Smooth Filter
	*/
	/*
	public static Vector3[] laplacianFilter(Vector3[] sv, int[] t, AdjacencySets adjacencySets) {
		Vector3[] wv = new Vector3[sv.Length];
		List<Vector3> adjacentVertices = new List<Vector3>();
 
		float dx = 0.0f;
		float dy = 0.0f;
		float dz = 0.0f;
 
		for (int vi=0; vi< sv.Length; vi++) {
			// Find the sv neighboring vertices
			//adjacentVertices = MeshUtils.findAdjacentNeighbors (sv, t, sv[vi]);
			adjacentVertices.Clear();
			BuildNeighborsLists(adjacencySets, adjacentVertices, vi, sv);
 
			if (adjacentVertices.Count != 0) {
				dx = 0.0f;
				dy = 0.0f;
				dz = 0.0f;
 
				//Debug.Log("Vertex Index Length = "+vertexIndexes.Length);
				// Add the vertices and divide by the number of vertices
				for (int j=0; j<adjacentVertices.Count; j++) {
					dx += adjacentVertices[j].x;
					dy += adjacentVertices[j].y;
					dz += adjacentVertices[j].z;
				}
 
				wv[vi].x = dx / adjacentVertices.Count;
				wv[vi].y = dy / adjacentVertices.Count;
				wv[vi].z = dz / adjacentVertices.Count;
			}
		}
		return wv;
	}
	*/
	
	// Assuming "neighbors" only includes actual neighbors, not the vertex itself
	private static Vector3 TrueAverageVertex(List<Vector3> neighbors, Vector3 v) {
		/* // Not supposed to happen anyway!
		if(neighbors.Count < 1)
			return v;
		*/

		foreach(Vector3 vert in neighbors)
			v += vert;

		float denominator = ( (float)neighbors.Count + 1f);
		float factor = 1f/denominator;
		if(factor == 0f)
			Debug.Log("HERE IS THE PROBLEM!!!!!!!!!!!!!");
		v = v * factor; // awkward, but you can't divide a vector by a float
		return v;
	}
	
	private static Vector3 AverageOfNeighbors(List<Vector3> neighbors) {
		Vector3 average = Vector3.zero;
		foreach(Vector3 v in neighbors)
			average += v;
		
		float denominator = (float)neighbors.Count;
		float factor = 1f/denominator;
		if(factor == 0f)
			Debug.Log("HERE IS THE PROBLEM!!!!!!!!!!!!!");
		average *= factor;
		return average;
	}
	
	private static Vector3 NudgeVertex(List<Vector3> neighbors, Vector3 v) {
		if(neighbors.Count == 0)
			return v;
		
		Vector3 nudgeVector;
		foreach(Vector3 vert in neighbors) {
			nudgeVector = vert - v;
			v += NUDGE_COEF * nudgeVector;
		}
		return v;
	}
	
	private static void BuildNeighborsLists(AdjacencySets adjSets, List<Vector3> neighbors,
											List<int> neighborIndices, int i, Vector3[] vertices) {
		HashSet<int> aSet = adjSets.GetAdjacencySet(i);
		foreach(int index in aSet) {
			if(i != index) {
				neighbors.Add(vertices[index]);
				neighborIndices.Add(index);
			}
		}
	}
	
	
	private static bool IsInsideVertex(List<Vector3> neighbors,
										List<int> nIndices, AdjacencySets aSets) {
		int nbNeighbors;	// number of neighbors that each neighbor possesses
		HashSet<int> aSet;
		
		// First we iterate over every neighbor of the "central" vertex we consider
		for(int nIndex=0; nIndex<neighbors.Count; nIndex++) {
		
			// We then get the adjacency set containing *its* neighbors
			aSet = aSets.GetAdjacencySet(nIndices[nIndex]);
			nbNeighbors = 0;
			
			// We now iterate over this set. If any value in it is also in the nIndices list,
			// i.e. in the list of neighbors of our "central" vertex, we increment the number
			// of neighbors.
			foreach(int a in aSet) {
				if(nIndices.Contains(a))
					nbNeighbors++;
			}
			
			// If this number is less than two, this means one of the neighbors of the central
			// vertex has less than two neighbors that are also a neighbor of the central vertex.
			// In other words, the central vertex is on an edge of the mesh.
			if(nbNeighbors < 2)
				return false;
		}
		
		// If the function has never returned false, then the vertex is not on an edge.
		// It can be moved.
		return true;
	}
	
	

	public static void AdjSetsSmoother(MeshData mData, AdjacencySets adjacencySets) {
		Vector3[] vertices = mData.vertices;
		Vector3[] ov = mData.vertices; // original vertices
		List<Vector3> neighbors = new List<Vector3>();
		List<int> neighborIndices = new List<int>();
		for(int i=0; i<vertices.Length; i++) {
			neighbors.Clear();					// must exist but be empty for each new vertex
			neighborIndices.Clear();
			BuildNeighborsLists(adjacencySets, neighbors, neighborIndices, i, ov);
			vertices[i] = TrueAverageVertex(neighbors, ov[i]);
		}
		mData.vertices = vertices;
	}
	
	// Not really necessary in UnityMol's current state.
	// Could be useful to keep around for imported meshes.
	public static void ConditionalAdjSetsSmoother(MeshData mData, AdjacencySets adjacencySets) {
		Vector3[] vertices = mData.vertices;
		Vector3[] ov = mData.vertices; // original vertices
		List<Vector3> neighbors = new List<Vector3>();
		List<int> neighborIndices = new List<int>();
		for(int i=0; i<vertices.Length; i++) {
			neighbors.Clear();					// must exist but be empty for each new vertex
			neighborIndices.Clear();
			BuildNeighborsLists(adjacencySets, neighbors, neighborIndices, i, ov);
			
			// True if the vertex is not on the edge of the mesh.
			if(IsInsideVertex(neighbors, neighborIndices, adjacencySets))
				vertices[i] = TrueAverageVertex(neighbors, ov[i]);
		}
		mData.vertices = vertices;
	}
	
	
	
	public static void AdjSetsHCSmoother(Mesh mesh, AdjacencySets adjacencySets) {
		Vector3[] ov = mesh.vertices; // original vertices
		Vector3[] wv = mesh.vertices; // will contain the Laplacian smoothing
		Vector3[] bv = new Vector3[mesh.vertices.Length];
		bool[] canBeMoved = new bool[mesh.vertices.Length];
		
		List<Vector3> neighbors = new List<Vector3>();
		List<int> neighborIndices = new List<int>();
		for(int i=0; i<wv.Length; i++) {
			neighbors.Clear();					// must exist but be empty for each new vertex
			neighborIndices.Clear();
			BuildNeighborsLists(adjacencySets, neighbors, neighborIndices, i, ov);
			
			// True if the vertex is not on the edge of the mesh.
			canBeMoved[i] = IsInsideVertex(neighbors, neighborIndices, adjacencySets);
			if(canBeMoved[i])
				wv[i] = TrueAverageVertex(neighbors, ov[i]);
			
			// Compute the differences
			bv[i] = wv[i] - ov[i];
		}
		
		// We let the first loop complete because bv must be filled for the next step
		Vector3 average;
		Vector3 move;
		for(int i=0; i<bv.Length; i++) {
			// We can't reuse the same neighbor list, because we get it from bv this time
			neighbors.Clear();
			neighborIndices.Clear();
			BuildNeighborsLists(adjacencySets, neighbors, neighborIndices, i, bv);
			average = AverageOfNeighbors(neighbors);
			
			// Final computation
			if(canBeMoved[i]) {
				move = BETA*bv[i] + ONE_MINUS_BETA*average;
				wv[i] += HC_CORRECT*move;
			}
		}
		mesh.vertices = wv;
	}
		
	
	/* // Doesn't really work
	public static void OcSmoother(Mesh mesh, VertexTree vertexTree) {
		Vector3[] vertices = mesh.vertices;
		List<Vector3> neighbors = new List<Vector3>();
		for(int i=0; i<vertices.Length; i++) {
			neighbors.Clear();					// must exist but be empty for each new vertex
			neighbors = vertexTree.GetNeighbors(neighbors, null, vertices[i]);
			//vertices[i] = AverageVertex(neighbors, vertices[i]);
			vertices[i] = NudgeVertex(neighbors, vertices[i]);
		}
		mesh.vertices = vertices;
	}
	*/
	

}