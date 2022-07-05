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


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

namespace UMol{
public class MarchingCubesWrapper {

	public enum MCType
	{
		CS = 0,
		CPP = 1,
		CUDA = 2,
		CJOB = 3
	};

	IntPtr cppMCInstance;
	IntPtr cudaMCInstance;
	MarchingCubesBurst mcb;

	[DllImport("CUDAMarchingCubes")]
	public static extern IntPtr cuda_getMCObj(float[] gridVal, int sizeX, int sizeY, int sizeZ);
	[DllImport("CUDAMarchingCubes")]
	public static extern void cuda_ComputeMesh(IntPtr instance, float isoValue, out int vertnumber, out int facenumber);
	[DllImport("CUDAMarchingCubes")]
	public static extern IntPtr cuda_getVertices(IntPtr instance);
	[DllImport("CUDAMarchingCubes")]
	public static extern IntPtr cuda_getTriangles(IntPtr instance);
	[DllImport("CUDAMarchingCubes")]
	public static extern void cuda_Destroy(IntPtr instance);
	[DllImport("CUDAMarchingCubes")]
	public static extern void cuda_freeMeshData(IntPtr instance);

	[DllImport("CPPMarchingCubes")]
	public static extern IntPtr getMCObj(float[] gridVal, int sizeX, int sizeY, int sizeZ);
	[DllImport("CPPMarchingCubes")]
	public static extern void ComputeMesh(IntPtr instance, float isoValue, out int vertnumber, out int facenumber);
	[DllImport("CPPMarchingCubes")]
	public static extern IntPtr getVertices(IntPtr instance);
	[DllImport("CPPMarchingCubes")]
	public static extern IntPtr getTriangles(IntPtr instance);
	[DllImport("CPPMarchingCubes")]
	public static extern void Destroy(IntPtr instance);
	[DllImport("CPPMarchingCubes")]
	public static extern void freeMeshData(IntPtr instance);

	float[] densVal;
	Int3 gridSize;
	//Default is C# job implementation
	public MCType mcMode = MCType.CJOB;

	public void Init(float[] densityValues, Int3 sizeGrid, Vector3 ori, float dx) {
		densVal = densityValues;
		gridSize = sizeGrid;

		if (mcMode == MCType.CUDA) {
			if (CudaAvailable.canRunCuda()) {
				try {
					cudaMCInstance = cuda_getMCObj(densVal, gridSize.x, gridSize.y, gridSize.z);
					Debug.Log("Using CUDA Marching Cubes implementation");
				}
				catch (System.Exception e) {
					// CPP version is somehow slower (memory copy probably slow) => use the CJOB version
					mcMode = MCType.CJOB;
#if UNITY_EDITOR
					Debug.LogError(e);
#endif
				}
			}
			else {
				mcMode = MCType.CJOB;
				Debug.Log("Using C# Marching Cubes implementation");
			}
		}
		if (mcMode == MCType.CJOB) {
			Vector3 oriXInv = ori;
			oriXInv.x *= -1;
			mcb = new MarchingCubesBurst(densVal, gridSize, oriXInv, dx);
		}
		else{
			mcMode = MCType.CS;
		}
	}

	public MeshData computeMC(float isoValue) {
		MeshData mData = new MeshData();
		int vertNumber = 0;
		int faceNumber = 0;

		IntPtr IntArrayPtrVertices = IntPtr.Zero;
		IntPtr IntArrayPtrTriangles = IntPtr.Zero;

		if (mcMode == MCType.CUDA) {
			cuda_ComputeMesh(cudaMCInstance, isoValue, out vertNumber, out faceNumber);

			IntArrayPtrVertices = cuda_getVertices(cudaMCInstance);
			IntArrayPtrTriangles = cuda_getTriangles(cudaMCInstance);
		}
		else if (mcMode == MCType.CPP) {
			ComputeMesh(cppMCInstance, isoValue, out vertNumber, out faceNumber);

			IntArrayPtrVertices = getVertices(cppMCInstance);
			IntArrayPtrTriangles = getTriangles(cppMCInstance);

		}
		if (mcMode == MCType.CPP || mcMode == MCType.CUDA) {
			float[] vertices = new float[vertNumber * 3];
			int[] triangles = new int[faceNumber * 3];

			Marshal.Copy(IntArrayPtrVertices, vertices, 0, 3 * vertNumber);
			Marshal.Copy(IntArrayPtrTriangles, triangles, 0, 3 * faceNumber);

			// Marshal.FreeCoTaskMem(IntArrayPtrVertices);
			// Marshal.FreeCoTaskMem(IntArrayPtrTriangles);

			// freeMeshData();

			Vector3[] allVertices = new Vector3[vertNumber];
			Vector3[] normals = new Vector3[vertNumber];
			for (long i = 0; i < vertNumber; i++) {
				Vector3 v = new Vector3(-vertices[i * 3], vertices[i * 3 + 1], vertices[i * 3 + 2]);
				allVertices[i] = v;
				normals[i] = Vector3.zero;
			}

			mData.triangles = triangles;
			mData.vertices = allVertices;
			mData.colors = new Color32[vertNumber];
			mData.normals = normals;
		}
		if (mcMode == MCType.CS) {

			// //CS version
			// Marching marching = new MarchingCubes();
			// marching.Surface = isoValue;

			//       List<Vector3> verts = new List<Vector3>();
			//       List<int> indices = new List<int>();

			// marching.Generate(densVal, gridSize.x, gridSize.y, gridSize.z, verts, indices);

			// MeshData mData = new MeshData();
			// mData.triangles = indices.ToArray();
			// mData.vertices = verts.ToArray();
			// mData.colors = new Color32[verts.Count];
			// mData.normals = new Vector3[indices.Count];

			// //CS version 2
			MarchingCubesSimple marching = new MarchingCubesSimple();

			List<Vector3> verts = new List<Vector3>();
			List<int> indices = new List<int>();

			// marching.Generate(densVal, gridSize.x, gridSize.y, gridSize.z, verts, indices);
			marching.marchingCubes(densVal, gridSize, ref indices, ref verts, isoValue);

			mData.triangles = indices.ToArray();
			mData.vertices = verts.ToArray();
			mData.colors = new Color32[verts.Count];
			mData.normals = new Vector3[verts.Count];
		}
		else if (mcMode == MCType.CJOB) {
			mcb.computeIsoSurface(isoValue);

			Vector3[] newVerts = mcb.getVertices();
			Vector3[] newNorms = mcb.getNormals();
			//Invert x for vertices and normals
			for (int i = 0; i < newVerts.Length; i++) {
				newVerts[i].x *= -1;
				newNorms[i].x *= -1;
			}
			int[] newTri = mcb.getTriangles();

			mData.triangles = newTri;
			mData.vertices = newVerts;
			mData.colors = new Color32[newVerts.Length];
			mData.normals = newNorms;

		}

		for (int i = 0; i < mData.colors.Length; i++) {
			mData.colors[i] = Color.white;
		}
		return mData;
	}
	public void FreeMC() {
		if (cppMCInstance != IntPtr.Zero) {
			Destroy(cppMCInstance);
		}
		if (cudaMCInstance != IntPtr.Zero) {
			Destroy(cudaMCInstance);
		}
		if (mcb != null) {
			mcb.Clean();
			mcb = null;
		}
	}
}
}