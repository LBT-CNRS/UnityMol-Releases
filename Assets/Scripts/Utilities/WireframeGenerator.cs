/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

        Xavier Martinez, 2017-2022
        Hubert Santuz, 2022-2026
        Marc Baaden, 2010-2026
        unitymol@gmail.com
        https://unity.mol3d.tech/

        This file is part of UnityMol, a general framework whose purpose is to provide
        a prototype for developing molecular graphics and scientific
        visualisation applications based on the Unity3D game engine.
        More details about UnityMol are provided at the following URL: https://unity.mol3d.tech/

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

        To help us with UnityMol development, we ask that you cite
        the research papers listed at https://unity.mol3d.tech/cite-us/.
    ================================================================================
*/

using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;

namespace UMol {
public class WireframeGenerator {


	public Mesh toWireframe(Mesh inMesh, ref int[] vToA, float wireSize = 0.01f) {

		//Simpler version:
		Mesh outMesh = new Mesh();
		outMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
		Vector3[] vverts = inMesh.vertices;
        int[] ttris = inMesh.triangles;

        int N = inMesh.vertexCount;
        int Ntri = ttris.Length;
        int[] newTris = new int[Ntri * 2];

        for (int i = 0; i < Ntri / 3 - 3; i += 3) {
            newTris[i * 6] = ttris[i * 3];
            newTris[i * 6 + 1] = ttris[i * 3 + 1];
            newTris[i * 6 + 2] = ttris[i * 3 + 1];
            newTris[i * 6 + 3] = ttris[i * 3 + 2];
            newTris[i * 6 + 4] = ttris[i * 3 + 2];
            newTris[i * 6 + 5] = ttris[i * 3];
        }

        outMesh.SetVertices(vverts);
        outMesh.SetIndices(newTris, MeshTopology.Lines, 0);

        return outMesh;

        //Complicated method

		// Vector3[] verts = inMesh.vertices;
		// Vector3[] norms = inMesh.normals;
		// int[] tris = inMesh.triangles;
		// Color32[] cols = inMesh.colors32;

		// NativeArray<float3> InVertices = new NativeArray<float3>(verts.Length, Allocator.TempJob);
		// NativeArray<float3> InNormals = new NativeArray<float3>(verts.Length, Allocator.TempJob);
		// NativeArray<Color32> InColors = new NativeArray<Color32>(verts.Length, Allocator.TempJob);
		// NativeArray<int> InTriangles = new NativeArray<int>(tris.Length, Allocator.TempJob);
		// NativeArray<int> InVToA = new NativeArray<int>(verts.Length, Allocator.TempJob);


		// int totalVerts = tris.Length * 4;//Each vertex of the triangles generate 2 vertices
		// int totalTris = tris.Length * 2 * 3 * 2;//4 more triangles per triangle

		// NativeArray<float3> OutVertices = new NativeArray<float3>(totalVerts, Allocator.TempJob);
		// NativeArray<float3> OutNormals = new NativeArray<float3>(totalVerts, Allocator.TempJob);
		// NativeArray<Color32> OutColors = new NativeArray<Color32>(totalVerts, Allocator.TempJob);
		// NativeArray<int> OutTriangles = new NativeArray<int>(totalTris, Allocator.TempJob);
		// NativeArray<int> OutVToA = new NativeArray<int>(totalVerts, Allocator.TempJob);


		// GetNativeArray(InVertices, verts);
		// GetNativeArray(InNormals, norms);
		// GetNativeArray(InTriangles, tris);
		// GetNativeArray(InColors, cols);
		// if (vToA != null)
		// 	GetNativeArray(InVToA, vToA);


		// var wireJob = new computeWireframe() {
		// 	outVerts = OutVertices,
		// 	outNorms = OutNormals,
		// 	outTris = OutTriangles,
		// 	outCols = OutColors,
		// 	outVToA = OutVToA,
		// 	verts = InVertices,
		// 	norms = InNormals,
		// 	v2a = InVToA,
		// 	tris = InTriangles,
		// 	cols = InColors,
		// 	size = wireSize,
		// 	useV2A = (vToA != null)
		// };

		// var wireJobHandle = wireJob.Schedule(tris.Length / 3, 64);

		// Mesh newMesh = new Mesh();
		// newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

		// verts = new Vector3[OutVertices.Length];
		// norms = new Vector3[OutNormals.Length];
		// tris = new int[OutTriangles.Length];
		// cols = new Color32[OutColors.Length];

		// wireJobHandle.Complete();


		// SetNativeArray(verts, OutVertices);
		// SetNativeArray(tris, OutTriangles);
		// SetNativeArray(norms, OutNormals);
		// SetNativeArray(cols, OutColors);
		// if (vToA != null){
		// 	vToA = new int[OutVToA.Length];
		// 	SetNativeArray(vToA, OutVToA);
		// }

		// newMesh.vertices = verts;
		// newMesh.triangles = tris;
		// newMesh.normals = norms;
		// newMesh.colors32 = cols;

		// //Used for hiding vertices
		// Vector2[] uvs = new Vector2[verts.Length];
		// for (int i = 0; i < uvs.Length; i++) {
		// 	uvs[i] = Vector2.one;
		// }
		// newMesh.uv2 = uvs;

		// InVertices.Dispose();
		// InNormals.Dispose();
		// InColors.Dispose();
		// InTriangles.Dispose();
		// InVToA.Dispose();

		// OutVertices.Dispose();
		// OutNormals.Dispose();
		// OutColors.Dispose();
		// OutTriangles.Dispose();
		// OutVToA.Dispose();

		// return newMesh;
	}


	[BurstCompile]
	struct computeWireframe : IJobParallelFor
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<float3> outVerts;
		[NativeDisableParallelForRestriction]
		public NativeArray<float3> outNorms;
		[NativeDisableParallelForRestriction]
		public NativeArray<int> outTris;
		[NativeDisableParallelForRestriction]
		public NativeArray<Color32> outCols;
		[NativeDisableParallelForRestriction]
		public NativeArray<int> outVToA;

		[ReadOnly] public NativeArray<float3> verts;
		[ReadOnly] public NativeArray<float3> norms;
		[ReadOnly] public NativeArray<int> tris;
		[ReadOnly] public NativeArray<int> v2a;
		[ReadOnly] public NativeArray<Color32> cols;
		[ReadOnly] public float size;
		[ReadOnly] public bool useV2A;

		void IJobParallelFor.Execute(int index)
		{

			int t1 = tris[index * 3 + 0];
			int t2 = tris[index * 3 + 1];
			int t3 = tris[index * 3 + 2];

			if (t1 == t2 || t2 == t3 || t3 == t1) {
				return;
			}

			float3 v1 = verts[t1];
			float3 v2 = verts[t2];
			float3 v3 = verts[t3];

			float3 v1v2 = v2 - v1;
			float3 v2v3 = v3 - v2;
			float3 v3v1 = v1 - v3;

			// float3 sidev1 = math.normalize(math.cross(v1v2, Vector3.up));
			// float3 sidev2 = math.normalize(math.cross(v2v3, Vector3.up));
			// float3 sidev3 = math.normalize(math.cross(v3v1, Vector3.up));
			float3 sidev1 = math.normalize(math.cross(v1v2, norms[t1]));
			float3 sidev2 = math.normalize(math.cross(v2v3, norms[t2]));
			float3 sidev3 = math.normalize(math.cross(v3v1, norms[t3]));

			int newId = index * 3 * 4;
			int newIdT = index * 3 * 6 * 2;

			outVerts[newId + 0] = v1 + sidev1 * size;
			outVerts[newId + 1] = v1 - sidev1 * size;
			outVerts[newId + 2] = v2 + sidev1 * size;
			outVerts[newId + 3] = v2 - sidev1 * size;

			outVerts[newId + 4] = v2 + sidev2 * size;
			outVerts[newId + 5] = v2 - sidev2 * size;
			outVerts[newId + 6] = v3 + sidev2 * size;
			outVerts[newId + 7] = v3 - sidev2 * size;

			outVerts[newId + 8] = v3 + sidev3 * size;
			outVerts[newId + 9] = v3 - sidev3 * size;
			outVerts[newId + 10] = v1 + sidev3 * size;
			outVerts[newId + 11] = v1 - sidev3 * size;

			outNorms[newId + 0] = norms[t1]; outCols[newId + 0] = cols[t1];
			outNorms[newId + 1] = norms[t1]; outCols[newId + 1] = cols[t1];
			outNorms[newId + 2] = norms[t2]; outCols[newId + 2] = cols[t2];
			outNorms[newId + 3] = norms[t2]; outCols[newId + 3] = cols[t2];

			outNorms[newId + 4] = norms[t2]; outCols[newId + 4] = cols[t2];
			outNorms[newId + 5] = norms[t2]; outCols[newId + 5] = cols[t2];
			outNorms[newId + 6] = norms[t3]; outCols[newId + 6] = cols[t3];
			outNorms[newId + 7] = norms[t3]; outCols[newId + 7] = cols[t3];

			outNorms[newId + 8] = norms[t3]; outCols[newId + 8] = cols[t3];
			outNorms[newId + 9] = norms[t3]; outCols[newId + 9] = cols[t3];
			outNorms[newId + 10] = norms[t1]; outCols[newId + 10] = cols[t1];
			outNorms[newId + 11] = norms[t1]; outCols[newId + 11] = cols[t1];

			outTris[newIdT + 0] = newId; outTris[newIdT + 18] = newId + 1;
			outTris[newIdT + 1] = newId + 1; outTris[newIdT + 19] = newId + 0;
			outTris[newIdT + 2] = newId + 2; outTris[newIdT + 20] = newId + 2;

			outTris[newIdT + 3] = newId + 1; outTris[newIdT + 21] = newId + 3;
			outTris[newIdT + 4] = newId + 3; outTris[newIdT + 22] = newId + 1;
			outTris[newIdT + 5] = newId + 2; outTris[newIdT + 23] = newId + 2;

			outTris[newIdT + 6] = newId + 4; outTris[newIdT + 24] = newId + 5;
			outTris[newIdT + 7] = newId + 5; outTris[newIdT + 25] = newId + 4;
			outTris[newIdT + 8] = newId + 6; outTris[newIdT + 26] = newId + 6;

			outTris[newIdT + 9] = newId + 5; outTris[newIdT + 27] = newId + 7;
			outTris[newIdT + 10] = newId + 7; outTris[newIdT + 28] = newId + 5;
			outTris[newIdT + 11] = newId + 6; outTris[newIdT + 29] = newId + 6;

			outTris[newIdT + 12] = newId + 8; outTris[newIdT + 30] = newId + 9;
			outTris[newIdT + 13] = newId + 9; outTris[newIdT + 31] = newId + 8;
			outTris[newIdT + 14] = newId + 10; outTris[newIdT + 32] = newId + 10;

			outTris[newIdT + 15] = newId + 9; outTris[newIdT + 33] = newId + 11;
			outTris[newIdT + 16] = newId + 11; outTris[newIdT + 34] = newId + 9;
			outTris[newIdT + 17] = newId + 10; outTris[newIdT + 35] = newId + 10;

			if (useV2A) {
				outVToA[newId + 0] = v2a[t1];
				outVToA[newId + 1] = v2a[t1];
				outVToA[newId + 2] = v2a[t2];
				outVToA[newId + 3] = v2a[t2];
				outVToA[newId + 4] = v2a[t2];
				outVToA[newId + 5] = v2a[t2];
				outVToA[newId + 6] = v2a[t3];
				outVToA[newId + 7] = v2a[t3];
				outVToA[newId + 8] = v2a[t3];
				outVToA[newId + 9] = v2a[t3];
				outVToA[newId + 10] = v2a[t1];
				outVToA[newId + 11] = v2a[t1];
			}

		}

	}

	//From https://gist.github.com/LotteMakesStuff/c2f9b764b15f74d14c00ceb4214356b4
	unsafe void GetNativeArray(NativeArray<float3> posNativ, Vector3[] posArray)
	{

		// pin the buffer in place...
		fixed (void* bufferPointer = posArray)
		{
			// ...and use memcpy to copy the Vector3[] into a NativeArray<floar3> without casting. whould be fast!
			UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(posNativ),
			                     bufferPointer, posArray.Length * (long) UnsafeUtility.SizeOf<float3>());
		}
		// we only have to fix the .net array in place, the NativeArray is allocated in the C++ side of the engine and
		// wont move arround unexpectedly. We have a pointer to it not a reference! thats basically what fixed does,
		// we create a scope where its 'safe' to get a pointer and directly manipulate the array
	}
	unsafe void GetNativeArray(NativeArray<int> posNativ, int[] posArray)
	{

		// pin the buffer in place...
		fixed (void* bufferPointer = posArray)
		{
			// ...and use memcpy to copy the Vector3[] into a NativeArray<floar3> without casting. whould be fast!
			UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(posNativ),
			                     bufferPointer, posArray.Length * (long) UnsafeUtility.SizeOf<int>());
		}
		// we only have to fix the .net array in place, the NativeArray is allocated in the C++ side of the engine and
		// wont move arround unexpectedly. We have a pointer to it not a reference! thats basically what fixed does,
		// we create a scope where its 'safe' to get a pointer and directly manipulate the array
	}
	unsafe void GetNativeArray(NativeArray<Color32> posNativ, Color32[] posArray)
	{

		// pin the buffer in place...
		fixed (void* bufferPointer = posArray)
		{
			// ...and use memcpy to copy the Vector3[] into a NativeArray<floar3> without casting. whould be fast!
			UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(posNativ),
			                     bufferPointer, posArray.Length * (long) UnsafeUtility.SizeOf<Color32>());
		}
		// we only have to fix the .net array in place, the NativeArray is allocated in the C++ side of the engine and
		// wont move arround unexpectedly. We have a pointer to it not a reference! thats basically what fixed does,
		// we create a scope where its 'safe' to get a pointer and directly manipulate the array
	}
	unsafe void SetNativeArray(Vector3[] posArray, NativeArray<float3> posNativ)
	{
		// pin the target array and get a pointer to it
		fixed (void* posArrayPointer = posArray)
		{
			// memcopy the native array over the top
			UnsafeUtility.MemCpy(posArrayPointer, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(posNativ), posArray.Length * (long) UnsafeUtility.SizeOf<float3>());
		}
	}

	unsafe void SetNativeArray(int[] posArray, NativeArray<int> posNativ)
	{
		// pin the target array and get a pointer to it
		fixed (void* posArrayPointer = posArray)
		{
			// memcopy the native array over the top
			UnsafeUtility.MemCpy(posArrayPointer, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(posNativ), posArray.Length * (long) UnsafeUtility.SizeOf<int>());
		}
	}

	unsafe void SetNativeArray(Color32[] posArray, NativeArray<Color32> posNativ)
	{
		// pin the target array and get a pointer to it
		fixed (void* posArrayPointer = posArray)
		{
			// memcopy the native array over the top
			UnsafeUtility.MemCpy(posArrayPointer, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(posNativ), posArray.Length * (long) UnsafeUtility.SizeOf<Color32>());
		}
	}


}
}
