/// @file GenerateMesh.cs
/// @brief Details to be specified
/// @author FvNano/LBT team
/// @author Marc Baaden <baaden@smplinux.de>
/// @date   2013-4
///
/// Copyright Centre National de la Recherche Scientifique (CNRS)
///
/// contributors :
/// FvNano/LBT team, 2010-13
/// Marc Baaden, 2010-13
///
/// baaden@smplinux.de
/// http://www.baaden.ibpc.fr
///
/// This software is a computer program based on the Unity3D game engine.
/// It is part of UnityMol, a general framework whose purpose is to provide
/// a prototype for developing molecular graphics and scientific
/// visualisation applications. More details about UnityMol are provided at
/// the following URL: "http://unitymol.sourceforge.net". Parts of this
/// source code are heavily inspired from the advice provided on the Unity3D
/// forums and the Internet.
///
/// This software is governed by the CeCILL-C license under French law and
/// abiding by the rules of distribution of free software. You can use,
/// modify and/or redistribute the software under the terms of the CeCILL-C
/// license as circulated by CEA, CNRS and INRIA at the following URL:
/// "http://www.cecill.info".
/// 
/// As a counterpart to the access to the source code and rights to copy, 
/// modify and redistribute granted by the license, users are provided only 
/// with a limited warranty and the software's author, the holder of the 
/// economic rights, and the successive licensors have only limited 
/// liability.
///
/// In this respect, the user's attention is drawn to the risks associated 
/// with loading, using, modifying and/or developing or reproducing the 
/// software by the user in light of its specific status of free software, 
/// that may mean that it is complicated to manipulate, and that also 
/// therefore means that it is reserved for developers and experienced 
/// professionals having in-depth computer knowledge. Users are therefore 
/// encouraged to load and test the software's suitability as regards their 
/// requirements in conditions enabling the security of their systems and/or 
/// data to be ensured and, more generally, to use and operate it in the 
/// same conditions as regards security.
///
/// The fact that you are presently reading this means that you have had 
/// knowledge of the CeCILL-C license and that you accept its terms.
///
/// $Id: GenerateMesh.cs 518 2014-05-14 14:52:51Z roudier $
///
/// References : 
/// If you use this code, please cite the following reference : 	
/// Z. Lv, A. Tek, F. Da Silva, C. Empereur-mot, M. Chavent and M. Baaden:
/// "Game on, Science - how video game technology may help biologists tackle
/// visualization challenges" (2013), PLoS ONE 8(3):e57990.
/// doi:10.1371/journal.pone.0057990
///
/// If you use the HyperBalls visualization metaphor, please also cite the
/// following reference : M. Chavent, A. Vanel, A. Tek, B. Levy, S. Robert,
/// B. Raffin and M. Baaden: "GPU-accelerated atom and dynamic bond visualization
/// using HyperBalls, a unified algorithm for balls, sticks and hyperboloids",
/// J. Comput. Chem., 2011, 32, 2924
///

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using Molecule.Model;

public class GenerateMesh {

	private static Vector3 CENTER = new Vector3(0f,0f,0f);
	private static float[,,] VOXELS;
	private static Vector3 DELTA;
	private static Vector3 ORIGIN;
	private static Vector3 OFFSET_ORIGIN;
	private static Transform DADDY;
	private static string TAG;
	private static int XDIM, YDIM, ZDIM;
	private static int SLICE_WIDTH = 80;
	private static float S_FUDGE_FACTOR = 18f;
	private static Vector3 FUDGE_FACTOR = new Vector3(S_FUDGE_FACTOR, S_FUDGE_FACTOR, S_FUDGE_FACTOR);
	private static bool ELECTRO = false;
	
	/// <summary>
	/// This computes the normal vectors from a MeshData object, and adds them to it.
	/// This is necessary because Unity3D can only do it for real Mesh objects,
	/// but this creates artifacts at the edges
	/// when said meshes are created by splitting a surface.
	/// </summary>
	/// <param name='mData'>
	/// The MeshData object to which normals will be added.
	/// </param>
	private static void ProperlyCalculateNormals(MeshData mData) {
		Vector3[] vertices = mData.vertices;
		int vCount = mData.vertices.Length;
		
		// this will contain the list of normals affecting each vertex
		List<List<Vector3>> normal_buffer = new List<List<Vector3>>();
		
		// temporary storage for normals, you can't directly access mesh.normals[i]
		Vector3[] myNormals = new Vector3[vCount];
		
		// initializing the buffer so we can access it by index and add stuff freely later
		for(int i=0; i<vCount; i++) {
			normal_buffer.Add(new List<Vector3>());
			myNormals[i] = new Vector3(0f,0f,0f); // make sure we start with empty normals
		}
		
		int[] triangles = mData.triangles;
		int index = triangles.Length;
		
		// For each triangle
		for(int i=0; i<index; i+=3) {
			Vector3 p1 = vertices[triangles[i+0]];
			Vector3 p2 = vertices[triangles[i+1]];
			Vector3 p3 = vertices[triangles[i+2]];
			
			Vector3 edge1 = p2 - p1;
			Vector3 edge2 = p3 - p1;
			Vector3 normal = Vector3.Cross(edge1, edge2);
			normal.Normalize();
			//Debug.Log(normal.ToString());
			
			// Storing the normal for each vertex affected. We get the correct index from the triangles array
			normal_buffer[triangles[i+0]].Add(normal);
			normal_buffer[triangles[i+1]].Add(normal);
			normal_buffer[triangles[i+2]].Add(normal);
		}
		
		// Iterating over each vertex
		for(int i=0; i<vCount; i++) {
			int normalNumber = normal_buffer[i].Count; // number of normals affecting this vertex
			//Debug.Log(normalNumber.ToString());
			
			for(int j=0; j<normalNumber; j++)
				myNormals[i] += normal_buffer[i][j];

			//if(normal_buffer[i].Count != 0) // is this necessary? apparently not
			myNormals[i].Normalize();
		}
		mData.normals = myNormals;
	}
	
	private static Mesh GenerateTextureCoordinates(Mesh mesh) {
		Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];
        int i = 0;
        while (i < uvs.Length) {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
            i++;
        }
        mesh.uv = uvs;
		return mesh;
	}
	
	private static Mesh CalculateMeshTangents(Mesh mesh) {
		//speed up math by copying the mesh arrays
		int[] triangles = mesh.triangles;
		Vector3[] vertices = mesh.vertices;
		Vector2[] uv = mesh.uv;
		Vector3[] normals = mesh.normals;
		
		//variable definitions
		int triangleCount = triangles.Length;
		int vertexCount = vertices.Length;
		
		Vector3[] tan1 = new Vector3[vertexCount];
		Vector3[] tan2 = new Vector3[vertexCount];
		
		Vector4[] tangents = new Vector4[vertexCount];
		
		for (long a = 0; a < triangleCount; a += 3) {
			long i1 = triangles[a + 0];
			long i2 = triangles[a + 1];
			long i3 = triangles[a + 2];
			
			Vector3 v1 = vertices[i1];
			Vector3 v2 = vertices[i2];
			Vector3 v3 = vertices[i3];
			
			Vector2 w1 = uv[i1];
			Vector2 w2 = uv[i2];
			Vector2 w3 = uv[i3];
			
			float x1 = v2.x - v1.x;
			float x2 = v3.x - v1.x;
			float y1 = v2.y - v1.y;
			float y2 = v3.y - v1.y;
			float z1 = v2.z - v1.z;
			float z2 = v3.z - v1.z;
			
			float s1 = w2.x - w1.x;
			float s2 = w3.x - w1.x;
			float t1 = w2.y - w1.y;
			float t2 = w3.y - w1.y;
			
			float r = 1.0f / (s1 * t2 - s2 * t1);
			
			Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
			Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
			
			tan1[i1] += sdir;
			tan1[i2] += sdir;
			tan1[i3] += sdir;
			
			tan2[i1] += tdir;
			tan2[i2] += tdir;
			tan2[i3] += tdir;
		}
		
		for (long a = 0; a < vertexCount; ++a) {
			Vector3 n = normals[a];
			Vector3 t = tan1[a];
			
			//Vector3 tmp = (t - n * Vector3.Dot(n, t)).normalized;
			//tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);
			Vector3.OrthoNormalize(ref n, ref t);
			tangents[a].x = t.x;
			tangents[a].y = t.y;
			tangents[a].z = t.z;
			tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
		}
		
		mesh.tangents = tangents;
		mesh.uv = uv;
		return mesh;
	}
	
	private static void FlipTriangles(MeshData mData) {
		int[] triangles = new int[mData.triangles.Length];
		
		for(int i=0; i<mData.triangles.Length; i+=3) {
			triangles[i]	=	mData.triangles[i+2];
			triangles[i+1]	=	mData.triangles[i+1];
			triangles[i+2]	=	mData.triangles[i];
		}
		
		mData.triangles = triangles;
	}
	
	private static void FlipNormals(MeshData mData) {
		Vector3[] normals = mData.normals;
		for(int i=0; i<normals.Length; i++)
			normals[i] = -normals[i];
		
		mData.normals = normals;
	}
	
	//private static Mesh OffsetVertices(Vector3 delta, Vector3 origin, Mesh mesh){
	private static void OffsetVertices(MeshData mData){
		Vector3[] vertices = mData.vertices;
		
		Vector3 invDelta = new Vector3(1f/DELTA.x, 1f/DELTA.y, 1f/DELTA.z);	
		for(int i=0; i<vertices.Length; i++) {
			if(ELECTRO)
				vertices[i] = Vector3.Scale(DELTA, vertices[i]) + OFFSET_ORIGIN;
			else
				vertices[i] = Vector3.Scale(invDelta, (vertices[i] - FUDGE_FACTOR)) + OFFSET_ORIGIN;
		}
		mData.vertices = vertices;
	}	
	
	/*
	private static void CreateSurfaceObject(	int xStart, int xEnd,
												int yStart, int yEnd,
												int zStart, int zEnd) {
		MeshData mData = MarchingCubes.CreateMesh(VOXELS, xStart, xEnd, yStart, yEnd, zStart, zEnd);
		
		// Culling empty meshes
		if(mData.vertices.Length == 0)
			return;
		
		AdjacencySets adjacencySets = new AdjacencySets(mData.triangles.Length);
		adjacencySets.AddAllTriangles(mData.triangles);
		SmoothFilter.AdjSetsSmoother(mData, adjacencySets);
		
		GameObject surface = new GameObject("SurfaceOBJ");
		surface.tag = TAG;
		surface.transform.parent = DADDY;
		surface.transform.localPosition = CENTER;
		
		OffsetVertices(mData);
		Mesh mesh = new Mesh();
		mesh.vertices = mData.vertices;
		mesh.triangles = mData.triangles;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		surface.AddComponent<MeshFilter>();
		surface.AddComponent<MeshRenderer>();
		surface.GetComponent<MeshFilter>().mesh = mesh;
		surface.renderer.material = new Material(Shader.Find("Diffuse"));
		// What about MoleculeModel.vertices?
	}
	*/
	
	private static void CreateSurfaceObjects(List<Mesh> meshes) {
		foreach(Mesh mesh in meshes) {
			GameObject surface = new GameObject("SurfaceOBJ");
			surface.tag = TAG;
			surface.transform.parent = DADDY;
			surface.transform.localPosition = CENTER;
			//mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			surface.AddComponent<MeshFilter>();
			surface.AddComponent<MeshRenderer>();
			surface.GetComponent<MeshFilter>().mesh = mesh;
			//surface.renderer.material = new Material(Shader.Find("Diffuse"));
			
			if(ELECTRO)
				surface.GetComponent<Renderer>().material = new Material(Shader.Find("Diffuse"));
			/*else if(UI.UIData.toggleBfac){
				surface.renderer.material = new Material(Shader.Find("Transparent/OIT_BLUE"));} */
			else {
				surface.GetComponent<Renderer>().material = new Material(Shader.Find("Mat Cap Cut"));
				surface.GetComponent<Renderer>().material.SetTexture("_MatCap",(Texture)Resources.Load("lit_spheres/divers/daphz1"));
			}
			Debug.Log(surface.GetComponent<Renderer>().material.shader.name);
		}
	}
	
	private static void SetDims() {
		XDIM = VOXELS.GetLength(0);
		YDIM = VOXELS.GetLength(1);
		ZDIM = VOXELS.GetLength(2);
	}
	
	private static void DebugDims() {
		Debug.Log(SLICE_WIDTH.ToString());
		Debug.Log("GenerateMesh::CreateSurfaceObjects > Voxel grid dimensions (x,y,z):");
		Debug.Log(XDIM.ToString());
		Debug.Log(YDIM.ToString());
		Debug.Log(ZDIM.ToString());
	}
	
	private static void InitGenMesh(float[,,] vox, float thresh, Vector3 d,	Vector3 o, string t="SurfaceManager") {
		VOXELS = vox;
		DELTA = d;
		ORIGIN = o;
		OFFSET_ORIGIN = ORIGIN;
		TAG = t;
		DADDY = GameObject.Find("SurfaceManager").transform;
		MarchingCubes.SetTarget(thresh);
		MarchingCubes.SetModeToCubes();
		// We could use MarchingCubes.SetWindingOrder here instead of flipping the triangles.
		
	}
	
	/// <summary>
	/// Creates the surface objects.
	/// </summary>
	/// <param name='voxels'>
	/// Voxels, i.e. the scalar field used to compute the surface.
	/// </param>
	/// <param name='threshold'>
	/// The threshold on which the isosurface is based.
	/// </param>
	/// <param name='delta'>
	/// Delta parameter from the grid, basically the size of each cell.
	/// </param>
	/// <param name='origin'>
	/// Origin of the grid.
	/// </param>
	/// <param name='colors'>
	/// Colors. Kept from previous implementation, but doesn't do anything here. I'm only
	/// keeping it because I'm not sure what it was used for. --- Alexandre
	/// </param>
	/// <param name='tag'>
	/// Tag for the objects to be created.
	/// </param>
	/// <param name='electro'>
	/// True if this is an electrostatic field isosurface.
	/// </param>
	public static void CreateSurfaceObjects(float[,,] voxels, float threshold, Vector3 delta, Vector3 origin,
												Color[] colors, string tag="SurfaceManager", bool electro = false) {
		ELECTRO = electro;
		Debug.Log(ELECTRO.ToString());
		if(ELECTRO) {
			ReadDX readDX = UI.GUIMoleculeController.readdx;
			origin = readDX.GetOrigin();
			delta  = readDX.GetDelta();
		}
		
		InitGenMesh(voxels, threshold, delta, origin, tag);
		SetDims();
		
		float bMCTime = Time.realtimeSinceStartup;
		MeshData mData = MarchingCubes.CreateMesh(VOXELS, 0, XDIM, 0, YDIM, 0, ZDIM);
		Debug.Log("Entire surface contains " + mData.vertices.Length.ToString() + " vertices.");
		float elapsed = 10f * (Time.realtimeSinceStartup - bMCTime);
		Debug.Log("GenerateMesh::MarchingCubes time: " + elapsed.ToString());
		OffsetVertices(mData);
		
		float bSmooth = Time.realtimeSinceStartup;
		AdjacencySets adjacencySets = new AdjacencySets(mData.triangles.Length);
		adjacencySets.AddAllTriangles(mData.triangles);
		SmoothFilter.AdjSetsSmoother(mData, adjacencySets);
		elapsed = Time.realtimeSinceStartup - bSmooth;
		Debug.Log("Smoothing time: " + elapsed.ToString());
		
		ProperlyCalculateNormals(mData);
		
		// Necessary for electrostatic fields isosurfaces
		Debug.Log(threshold.ToString());
		if(threshold < 0)
			FlipTriangles(mData);
		
		Splitting splitting = new Splitting();
		List<Mesh> meshes = splitting.Split(mData);
		CreateSurfaceObjects(meshes);
	}
	
	
	public void GM (Vector3[] Mvertices, int[] Mtriangles, Vector3 center, int surfaceNb, Color[] Colors, string tag="SurfaceManager") {
		Debug.Log("Entering :: GM: "+tag);		
		GameObject GOSurface = new GameObject("SurfaceOBJ");
		GOSurface.tag = tag;
		GOSurface.transform.parent = GameObject.Find("SurfaceManager").transform;
	    Mesh mesh = new Mesh();
		GOSurface.AddComponent<MeshFilter>();
		GOSurface.AddComponent<MeshRenderer>();
		GOSurface.GetComponent<MeshFilter>().mesh = mesh;
		//Material SurfaceShader = Resources.Load("SurfaceShader", typeof(Material)) as Material;
		//GOSurface.renderer.material = SurfaceShader;
	
		
		GOSurface.GetComponent<Renderer>().material = new Material(Shader.Find("Mat Cap Cut"));
		GOSurface.GetComponent<Renderer>().material.SetTexture("_MatCap",(Texture)Resources.Load("lit_spheres/divers/daphz1"));
		
		
		//GOSurface.renderer.material = new Material(Shader.Find("Diffuse"));
	
		//GOSurface.renderer.material = new Material(Shader.Find("Vertex Colored"));
		//GOSurface.renderer.material.SetTexture("_MatCap",(Texture)Resources.Load("graypic/bruckner"));
		GOSurface.transform.localPosition = center;
		mesh.Clear();
	
		// //Unity has a left-handed coordinates system while Molecular obj are right-handed
		// //So we have to negate the x axis and change the winding order
		// for(int i=0; i < Mvertices.Length; i++)
		// {
		// 	Mvertices[i].x = -Mvertices[i].x;
		// }
		
		// for(int tri=0; tri<Mtriangles.Length; tri=tri+3)
	 //    {
	 //        int tmp = Mtriangles[tri];
	 //        Mtriangles[tri] = Mtriangles[tri+2];
	 //        Mtriangles[tri+2] = tmp;
	 //    }
		mesh.vertices = Mvertices;
		MoleculeModel.vertices= Mvertices;
		Debug.Log("Exiting :: vertices filled");
		mesh.triangles = Mtriangles;
		mesh.colors = Colors;
		mesh.RecalculateBounds();
//		mesh.normals = Mnormals;
		mesh.RecalculateNormals();
		//mesh = ProperlyCalculateNormals(mesh);
		//mesh = GenerateTextureCoordinates(mesh);
		//mesh = CalculateMeshTangents(mesh);
//			mesh.Optimize();

	}
	
	

}
