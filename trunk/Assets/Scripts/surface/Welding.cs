using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Welding {
	
	
	
	private static bool isIn(List<Vector3> vList, Vector3 vertex, float threshold) {
		foreach(Vector3 v in vList) {
			if(Vector3.Distance(v, vertex) <= threshold)
				return true;
		}
		return false;
	}
	
	
	
	
	public static MeshData Weld(int[] triangles, Vector3[] vertices, Vector3 b0, Vector3 b1) {
		Vector3[] verts = vertices;
		VertexTree vertexTree = new VertexTree(b0, b1);
		
		// Build new vertex buffer and remove "duplicate" verticies
		// that are within the given threshold.
		List<Vector3> newVerts = new List<Vector3>();
		
		int vIndex = 0;
		foreach (Vector3 vert in verts) {
			// This adds the vertex to the tree, returns true if it was already there
			if (!vertexTree.FindOrAddVertex(vert, vIndex)) {
				newVerts.Add(vert);
				vIndex++;
			}
		}
		// Rebuild triangles using new verticies
		int[] tris = triangles;
		for (int i = 0; i < tris.Length; ++i) {
			// We need to find the index of the new vertex closest to verts[tris[i]]
			tris[i] = vertexTree.GetIndex(verts[tris[i]]);
		}
		
		int max = -1;
		foreach(int index in tris)
			if (index > max)
				max = index;
		
		MeshData result = new MeshData();
		Debug.Log("GenerateMesh::AutoWeld > Number of new vertices in list, and max triangle index in new triangle array.");
		Debug.Log(newVerts.Count.ToString());
		Debug.Log(max.ToString());
		//Debug.Log(vertices.Length.ToString());
		
		result.triangles = tris; // BEFORE VERTICES!!!
		result.vertices = newVerts.ToArray();
		//mesh.uv = newUVs.ToArray();
		return result;
	}

}
