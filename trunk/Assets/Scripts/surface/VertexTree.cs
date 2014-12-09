using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VertexTree {
	private Vector3 bound0, bound1, split;

	private List<Vector3> vertices;
	private List<int> indices;
	private VertexTree[] children;
	private bool isLeaf = true;
	
	// Holes appear with larger thresholds. I'm not really sure why.
	private static float THRESHOLD = 0.01f;
	private static float SQ_THRESHOLD = THRESHOLD * THRESHOLD;
	private static int MAX_VERTICES = 16;
	//private static int MIN_NEIGHBORS = 3;
	//private static float NEIGHBOR_THRESHOLD = 2.0f;
	
	public VertexTree(Vector3 b0, Vector3 b1) {
		split = 0.5f * (b0 + b1); // Middle of the cube
		bound0 = b0;
		bound1 = b1;
		vertices = new List<Vector3>();
		indices = new List<int>();
		children = new VertexTree[8];
	}
	
	private void AddVertexToLeaf(Vector3 v, int i) {
		vertices.Add(v);
		indices.Add(i);
		
		// If the addition brings the cube to the limit of vertices
		if(vertices.Count >= MAX_VERTICES)
			Subdivide();
	}
	
	private Vector3 GetOffset(Vector3 b0, Vector3 b1, int i) {
		Vector3 result;
		switch(i) {
		case 0:
			result = Vector3.zero;
			break;
		case 1:
			result = new Vector3(b1.x,	0,		0);
			break;
		case 2:
			result = new Vector3(0,		b1.y,	0);
			break;
		case 3:
			result = new Vector3(b1.x,	b1.y,	0);
			break;
		case 4:
			result = new Vector3(0,		0,		b1.z);
			break;
		case 5:
			result = new Vector3(b1.x,	0,		b1.z);
			break;
		case 6:
			result = new Vector3(0,		b1.y,	b1.z);
			break;
		case 7:
			result = b1;
			break;
		default:
			Debug.Log("VertexTree::Offset() > Something is very, very wrong here.");
			result = Vector3.zero;
			break;
		}
		return 0.5f * result;
	}
	
	private int GetChildIndex(Vector3 v) {
		int childIndex = 0;
		
		if(v.x > split.x)
			childIndex |= 1;
		if(v.y > split.y)
			childIndex |= 2;
		if(v.z > split.z)
			childIndex |= 4;
		
		return childIndex;
	}
	
	private void Subdivide() {
		Vector3 oppositeBound = bound1 - bound0;
		Vector3 offset;
		
		// Creating the sub-cubes.
		for(int i=0; i<8; i++) {
			offset = GetOffset(bound0, oppositeBound, i);
			children[i] = new VertexTree(bound0+offset, split+offset);
		}
		
		// Sending the vertices (and corresponding indices) to the correct sub-cubes.
		int childIndex;
		for(int i=0; i<MAX_VERTICES; i++) {
			childIndex = GetChildIndex(vertices[i]);			
			children[childIndex].AddVertexToLeaf(vertices[i], indices[i]);
		}
		
		// Now this cube is not a leaf anymore, but a node.
		// It must be cleared, and no more vertices should be added to it.
		vertices.Clear();
		indices.Clear();
		
		isLeaf = false;
	}
	
	public int AddVertex(Vector3 v, int ind) {
		if(isLeaf) {
			float distance;
			for(int i=0; i<vertices.Count; i++) {
				distance = Vector3.SqrMagnitude(v - vertices[i]);
				if(distance <= SQ_THRESHOLD) // A similar vertex was found, no need to add it.
					return indices[i];
			}
			// If this is a leaf and the function has not returned yet,
			// no similar vertex was found, and we must add it.
			AddVertexToLeaf(v, ind);
			
			// And finally return its index.
			return ind;
		}
		
		// If the function has not returned yet, then this is a node, not a leaf.
		// So we just find the correct sub-cube for the recursive call.
		int childIndex = GetChildIndex(v);
		
		return children[childIndex].AddVertex(v, ind);
	}
	
	
	public bool FindOrAddVertex(Vector3 v, int ind) {
		if(isLeaf) {
			float distance;
			for(int i=0; i<vertices.Count; i++) {
				distance = Vector3.SqrMagnitude(v - vertices[i]);
				if(distance <= SQ_THRESHOLD) // A similar vertex was found, no need to add it.
					return true;
			}
			// If this is a leaf and the function has not returned yet,
			// no similar vertex was found, and we must add it.
			AddVertexToLeaf(v, ind);
			
			// And finally return false, because no similar vertex was found.
			return false;
		}
		
		// If the function has not returned yet, then this is a node, not a leaf.
		// So we just find the correct sub-cube for the recursive call.
		int childIndex = GetChildIndex(v);
		
		return children[childIndex].FindOrAddVertex(v, ind);
	}
	
	public int GetIndex(Vector3 v) {
		if(isLeaf) {
			float distance;
			for(int i=0; i<vertices.Count; i++) {
				distance = Vector3.SqrMagnitude(v - vertices[i]);
				if(distance <= SQ_THRESHOLD) // A similar vertex was found, no need to add it.
					return indices[i];
			}
			// If this is a leaf and the function has not returned yet,
			// there is a problem.
			Debug.Log("VertexTree::GetIndex() > Error! No vertex found when rebuilding triangles.");
			return -1; // should crash
		}
		
		// If the function has not returned yet, then this is a node, not a leaf.
		// So we just find the correct sub-cube for the recursive call.
		int childIndex = GetChildIndex(v);
		
		return children[childIndex].GetIndex(v);
	}
	
	
	
	/*
	// Was written for an alternative smoothing method. Not used. 
	private void AddNeighborsInNode(List<Vector3> neighbors, Vector3 v) {
		foreach(VertexTree vTree in children)
			vTree.AddVectorsInLeaf(neighbors, v);
	}
	
	private void AddVectorsInLeaf(List<Vector3> neighbors, Vector3 v) {
		foreach(Vector3 vert in vertices)
			if( (v != vert) && (Vector3.Distance(v, vert) < NEIGHBOR_THRESHOLD) )
				neighbors.Add(vert);
	}
	
	
	public List<Vector3> GetNeighbors(List<Vector3> neighbors, VertexTree daddy, Vector3 v) { 
		if(isLeaf) {
			if(vertices.Count < MIN_NEIGHBORS + 1)		// Not enough vertices in this leaf
				daddy.AddNeighborsInNode(neighbors, v);
			else											// Enough vertices in this leaf
				AddVectorsInLeaf(neighbors, v);
				
			return neighbors;
		} else {
			int childIndex = GetChildIndex(v);
			return children[childIndex].GetNeighbors(neighbors, this, v);
		}
	}
	*/
	
}
