using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CullTriangles {
	private static Dictionary<string, bool> triDict;
	private static int[] triangles;
	private static string key;
	private static int[] indices;
	private static List<int> culledTriangles;
	
	
	public static void Cull(Mesh mesh) {
		triangles = mesh.triangles;
		triDict = new Dictionary<string, bool>();
		indices = new int[3];
		culledTriangles = new List<int>();
		
		for(int i=0; i<triangles.Length; i+=3) {
			indices[0] = triangles[i];
			indices[1] = triangles[i+1];
			indices[2] = triangles[i+2];
			System.Array.Sort(indices);
			
			key = indices[0] + "," + indices[1] + "," + indices[2];
			if(!triDict.ContainsKey(key)) {
				triDict.Add(key, true);
				culledTriangles.Add(triangles[i]);
				culledTriangles.Add(triangles[i+1]);
				culledTriangles.Add(triangles[i+2]);
			}
		}
		
		triangles = culledTriangles.ToArray();
		mesh.triangles = triangles;
		
		Debug.Log("CullTriangles::Cull() > Number of triangles after culling:");
		Debug.Log((mesh.triangles.Length / 3).ToString());
		Debug.Log((culledTriangles.Count / 3).ToString());
	}
}
