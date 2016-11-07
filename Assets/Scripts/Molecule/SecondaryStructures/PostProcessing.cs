using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PostProcessing {
	private static Vector3 CENTER = new Vector3(0f,0f,0f);
	
	/// <summary>
	/// First attempt at making ribbons thick. Not used anymore.
	/// </summary>
	/// <param name='vertices'>
	/// Vertices.
	/// </param>
	/// <param name='normals'>
	/// Normals.
	/// </param>
	/// <param name='triangles'>
	/// Triangles.
	/// </param>
	/// <param name='colors'>
	/// Colors.
	/// </param>
	private static void DuplicateVertices(List<Vector3> vertices, List<Vector3> normals,
													List<int> triangles, List<Color32> colors) {
		int nbVertices = vertices.Count; // we need a constant value, can't iterate with <vertices.Count in the loop
		float displacement = 0.6f;
		//float displacement = 0.0f;

		
		for(int i =0; i<nbVertices; i+=16) {
			vertices.Add(vertices[i] - displacement * normals[i] );
			vertices.Add(vertices[i+1] - displacement * normals[i+1] );
			vertices.Add(vertices[i+2] - displacement * normals[i+2] );
			
			vertices.Add(vertices[i+3] - displacement * normals[i+3] );
			vertices.Add(vertices[i+4] - displacement * normals[i+4] );
			vertices.Add(vertices[i+5] - displacement * normals[i+5] );
			
			vertices.Add(vertices[i+6] - displacement * normals[i+6] );
			vertices.Add(vertices[i+7] - displacement * normals[i+7] );
			vertices.Add(vertices[i+8] - displacement * normals[i+8] );
			
			vertices.Add(vertices[i+9] - displacement * normals[i+9] );
			vertices.Add(vertices[i+10] - displacement * normals[i+10] );
			vertices.Add(vertices[i+11] - displacement * normals[i+11] );
			
			vertices.Add(vertices[i+12] - displacement * normals[i] );
			vertices.Add(vertices[i+13] - displacement * normals[i+1] );
			vertices.Add(vertices[i+14] - displacement * normals[i+6] );
			vertices.Add(vertices[i+15] - displacement * normals[i+7] );
			
			colors.Add(colors[i]);
			colors.Add(colors[i+1]);
			colors.Add(colors[i+2]);
			
			colors.Add(colors[i+2]);
			colors.Add(colors[i+4]);
			colors.Add(colors[i+5]);
			
			colors.Add(colors[i+6]);
			colors.Add(colors[i+7]);
			colors.Add(colors[i+8]);
			
			colors.Add(colors[i+9]);
			colors.Add(colors[i+10]);
			colors.Add(colors[i+11]);
			
			colors.Add(colors[i+12]);
			colors.Add(colors[i+13]);
			colors.Add(colors[i+14]);
			colors.Add(colors[i+15]);
			
			
			normals.Add(-normals[i]);
			normals.Add(-normals[i+1]);
			normals.Add(-normals[i+2]);
			
			normals.Add(-normals[i+3]);
			normals.Add(-normals[i+4]);
			normals.Add(-normals[i+5]);
			
			normals.Add(-normals[i+6]);
			normals.Add(-normals[i+7]);
			normals.Add(-normals[i+8]);
			
			normals.Add(-normals[i+9]);
			normals.Add(-normals[i+10]);
			normals.Add(-normals[i+11]);
			
			// No need to flip those normals
			normals.Add(normals[i+12]);
			normals.Add(normals[i+13]);
			normals.Add(normals[i+14]);
			normals.Add(normals[i+15]);
			
			
			triangles.Add(nbVertices+i);
			triangles.Add(nbVertices+i+1);
			triangles.Add(nbVertices+i+2);
			
			triangles.Add(nbVertices+i+3);
			triangles.Add(nbVertices+i+4);
			triangles.Add(nbVertices+i+5);
			
			triangles.Add(nbVertices+i+6);
			triangles.Add(nbVertices+i+7);
			triangles.Add(nbVertices+i+8);
			
			triangles.Add(nbVertices+i+9);
			triangles.Add(nbVertices+i+10);
			triangles.Add(nbVertices+i+11);
			
			
			triangles.Add(i+12);
			triangles.Add(i+13);
			triangles.Add(i+nbVertices+12);
			
			triangles.Add(i+nbVertices+12);
			triangles.Add(i+nbVertices+13);
			triangles.Add(i+13);
			
			triangles.Add(i+nbVertices+14);
			triangles.Add(i+15);
			triangles.Add(i+14);
			
			triangles.Add(i+nbVertices+14);
			triangles.Add(i+nbVertices+15);
			triangles.Add(i+15);
		}
		/*
		for(int i=0; i<nbVertices; i+=12) {
			triangles.Add(i);
			triangles.Add(i+1);
			triangles.Add(i+nbVertices);
			
			triangles.Add(i+1);
			triangles.Add(i+nbVertices+1);
			triangles.Add(i+nbVertices);
			
			triangles.Add(i+nbVertices+6);
			triangles.Add(i+7);
			triangles.Add(i+6);
			
			triangles.Add(i+nbVertices+6);
			triangles.Add(i+nbVertices+7);
			triangles.Add(i+7);
		}
		*/
	}
	
	private static void SubGenerateMeshes(List<Mesh> meshes, string tag, string gameobj) {
		foreach(Mesh mesh in meshes) {
			
			/*
			int nbVertices = mesh.vertices.Length;
			Color32[] colors = new Color32[nbVertices];
			for(int i=0; i<nbVertices; i++) {
				colors[i] = new Color32(0,255,0,255);
			}
			mesh.colors32 = colors;
			*/
			
			GameObject ribbObj = new GameObject(gameobj);
			ribbObj.tag = tag;
			ribbObj.AddComponent<MeshFilter>();
			ribbObj.AddComponent<MeshRenderer>();
			ribbObj.GetComponent<MeshFilter>().mesh = mesh;
			ribbObj.GetComponent<Renderer>().material = new Material(Shader.Find("Custom/Ribbons"));
			ribbObj.transform.position = CENTER;
			ribbObj.transform.localPosition = CENTER;
		}
	}
	
	private static void AddFirstFrontalFace(List<Vector3> vertices, List<Vector3> normals,
										List<int> triangles, List<Color32> colors, int[] ss) {
		int nbVert = vertices.Count;
		Vector3 back = (vertices[8] - vertices[7]).normalized;
		
		// Duplicating the correct vertices
		
		// S1P0_b => + 0
		vertices.Add(vertices[0]);
		
		// CP0_b => + 1
		vertices.Add(vertices[8]);
		
		// S1P0_t => + 2
		vertices.Add(vertices[3]);
		
		// CP0_t => + 3
		vertices.Add(vertices[11]);
		
		// S2P0_b => + 4
		vertices.Add(vertices[12]);
		
		// S2P0_t => + 5
		vertices.Add(vertices[15]);
		
		// Same backwards normal vector for all
		for(int i=0; i<6; i++) {
			normals.Add(back);
			colors.Add(colors[0]);
		}
		// Now the triangles
		// _b == bottom, _t == top
		
		// S1P0_b, CP0_b, S1P0_t
		triangles.Add(nbVert + 0);
		triangles.Add(nbVert + 1);
		triangles.Add(nbVert + 2);
		
		// CP0_b, S1P0_t, CP0_t
		triangles.Add(nbVert + 1);
		triangles.Add(nbVert + 2);
		triangles.Add(nbVert + 3);
		
		// CP0_b, CP0_t, S2P0_t
		triangles.Add(nbVert + 1);
		triangles.Add(nbVert + 3);
		triangles.Add(nbVert + 5);
		
		// CP0_b, S2P0_b, S2P0_t
		triangles.Add(nbVert + 1);
		triangles.Add(nbVert + 4);
		triangles.Add(nbVert + 5);


	}
	
	private static void AddLastFrontalFace(List<Vector3> vertices, List<Vector3> normals,
										List<int> triangles, List<Color32> colors, int[] ss) {
		int nbVert = vertices.Count;
		
		// beginning of last section of last residue
		// -6 to account for vertices added by previous function
		int start = nbVert - 32 - 6;
		
		Vector3 front = (vertices[7] - vertices[8]).normalized;
		
		// Duplicating the correct vertices
		
		// S1P1_b => + 0
		vertices.Add(vertices[start+1]);
		
		// CP1_b => + 1
		vertices.Add(vertices[start+2]);
		
		// S1P1_t => + 2
		vertices.Add(vertices[start+4]);
		
		// CP1_t => + 3
		vertices.Add(vertices[start+5]);
		
		// S2P1_b => + 4
		vertices.Add(vertices[start+13]);
		
		// S2P1_t => + 5
		vertices.Add(vertices[start+16]);
		
		// Same backwards normal vector for all
		for(int i=0; i<6; i++) {
			normals.Add(front);
			colors.Add(colors[nbVert - 1 - 6]); // -6 to account for vertices added just before
		}
		// Now the triangles
		// _b == bottom, _t == top
		
		// S1P1_b, CP1_b, S1P1_t
		triangles.Add(nbVert + 0);
		triangles.Add(nbVert + 1);
		triangles.Add(nbVert + 2);
		
		// CP1_b, S1P1_t, CP1_t
		triangles.Add(nbVert + 1);
		triangles.Add(nbVert + 2);
		triangles.Add(nbVert + 3);
		
		// CP1_b, CP1_t, S2P1_t
		triangles.Add(nbVert + 1);
		triangles.Add(nbVert + 3);
		triangles.Add(nbVert + 5);
		
		// CP1_b, S2P1_b, S2P1_t
		triangles.Add(nbVert + 1);
		triangles.Add(nbVert + 4);
		triangles.Add(nbVert + 5);

	}
	

	public static void GenerateMeshes(List<Vector3> vertices, List<Vector3> normals,
										List<int> triangles, List<Color32> colors, int[] ss,
	                                  string tag="RibbonObj", string gameobj="Ribbons") {
	//	DuplicateVertices(vertices, normals, triangles, colors);
		tag = Ribbons.ribbontag;
		AddFirstFrontalFace(vertices, normals, triangles, colors, ss);
		AddLastFrontalFace(vertices, normals, triangles, colors, ss);
		MeshData mData = new MeshData();


		mData.vertices = vertices.ToArray();
		mData.normals = normals.ToArray();
		mData.triangles = triangles.ToArray();
		mData.colors = colors.ToArray();

		/*
		for(int i=0; i<mData.vertices.Length; i++) {
			mData.vertices[i] += Molecule.Model.MoleculeModel.Offset;
		}
		*/
		
		Splitting split = new Splitting();
		List<Mesh> meshes = split.Split(mData);
		SubGenerateMeshes(meshes, tag, gameobj);		
	}
}
