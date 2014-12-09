using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AdjacencySets {
	private HashSet<int>[] vertexArray;
	private static int NBVERT = 65000;
	
	/*
	public AdjacencySets() {
		vertexArray = new HashSet<int>[NBVERT];
		for(int i=0; i<NBVERT; i++) {
			vertexArray[i] = new HashSet<int>();
		}
	}
	*/
	
	public AdjacencySets(int nb) {
		NBVERT = nb;
		vertexArray = new HashSet<int>[NBVERT];
		for(int i=0; i<NBVERT; i++) {
			vertexArray[i] = new HashSet<int>();
		}
	}
	
	public HashSet<int> GetAdjacencySet(int vIndex) {
		return vertexArray[vIndex];
	}
	
	
	public void AddAllTriangles(int[] triangles) {
		for(int i=0; i<triangles.Length; i+=3) {
			// Neighbors of vertex i
			vertexArray[triangles[i]].Add(triangles[i+1]);
			vertexArray[triangles[i]].Add(triangles[i+2]);
			
			// Neighbors of vertex i+1
			vertexArray[triangles[i+1]].Add(triangles[i]);
			vertexArray[triangles[i+1]].Add(triangles[i+2]);
			
			// Neighbors of vertex i+2
			vertexArray[triangles[i+2]].Add(triangles[i]);
			vertexArray[triangles[i+2]].Add(triangles[i+1]);
		}
	}
	
	
	
	/*
	public void AddLastNTriangles(int nbTriangles, List<int> triangles) {
		int firstIndex = triangles.Count - 3*nbTriangles;
		for(int i=firstIndex; i<triangles.Count; i+=3) {
			// Neighbors of vertex i
			vertexArray[i].Add(triangles[i+1]);
			vertexArray[i].Add(triangles[i+2]);
			
			// Neighbors of vertex i+1
			vertexArray[i+1].Add(triangles[i]);
			vertexArray[i+1].Add(triangles[i+2]);
			
			// Neighbors of vertex i+1
			vertexArray[i+2].Add(triangles[i]);
			vertexArray[i+2].Add(triangles[i+1]);
		}
	}
	*/
}
