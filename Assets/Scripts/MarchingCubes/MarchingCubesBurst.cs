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

//TODO:
//Implement a parallel exclusive scan using C# job system, right now this is the slowest part (because done in serial) but should be the fastest
//Avoid copying tritable arrays to native arrays by correctly initialize them

namespace UMol {
public class MarchingCubesBurst {

	NativeArray<float> values;
	NativeArray<float3> grad;
	NativeArray<float3> curVertices;
	NativeArray<Color32> curColors;
	NativeArray<float3> curNormals;
	NativeArray<int> curTriangles;
	NativeArray<int> vertexToCellId;

	NativeArray<int> nbTriTable;
	NativeArray<int> triTable;
	NativeArray<float3> natDeltas;
	NativeArray<float3> natDir;


	Vector3[] deltas;
	float3 originGrid;
	int3 gridSize;
	int totalSize;
	bool useGradient = true;


	public MarchingCubesBurst(float[] densValues, Vector3[] gradient,
	                          int3 gSize, Vector3 ori, Vector3[] cellDims,
	                          Vector3[] cellDir) {

		totalSize = gSize.x * gSize.y * gSize.z;
		originGrid = ori;
		deltas = cellDims;

		if (totalSize != densValues.Length) {
			Debug.LogError("Something is wrong with data length " + totalSize + " != " + densValues.Length);
			return;
		}
		if (gSize.x > 512 || gSize.y > 512 || gSize.z > 512) {
			Debug.LogError("Grid too large");
			return;
		}

		values = new NativeArray<float>(totalSize, Allocator.Persistent);
		GetNativeArray(values, densValues);

		if (gradient != null) {
			grad = new NativeArray<float3>(totalSize, Allocator.Persistent);
			useGradient = true;
		} else {
			grad = new NativeArray<float3>(1, Allocator.Persistent);
			useGradient = false;
		}

		natDeltas = new NativeArray<float3>(3, Allocator.Persistent);
		natDir = new NativeArray<float3>(3, Allocator.Persistent);

		initTriTable();

		if (useGradient)
			GetNativeArray(grad, gradient);

		gridSize.x = gSize.x;
		gridSize.y = gSize.y;
		gridSize.z = gSize.z;

		natDeltas[0] = deltas[0];
		natDeltas[1] = deltas[1];
		natDeltas[2] = deltas[2];

		natDir[0] = cellDir[0];
		natDir[1] = cellDir[1];
		natDir[2] = cellDir[2];

	}

	void initTriTable() {
		nbTriTable = new NativeArray<int>(256, Allocator.Persistent);
		triTable = new NativeArray<int>(4096, Allocator.Persistent);
		int id = 0;
		for (int i = 0; i < managed_triTable.GetLength(0); i++) {
			for (int j = 0; j < managed_triTable.GetLength(1); j++) {
				triTable[id++] = managed_triTable[i, j];
			}
		}
		for (int i = 0; i < managed_nbTriTable.Length; i++) {
			nbTriTable[i] = managed_nbTriTable[i];
		}
	}

	public void computeIsoSurface(float isoValue) {

		if (curVertices.IsCreated)
			curVertices.Dispose();
		if (curNormals.IsCreated)
			curNormals.Dispose();
		if (curTriangles.IsCreated)
			curTriangles.Dispose();
		if (curColors.IsCreated)
			curColors.Dispose();
		if (vertexToCellId.IsCreated)
			vertexToCellId.Dispose();

		//CountVertexPerVoxelJob
		NativeArray<uint2> vertPerCellIn = new NativeArray<uint2>(totalSize, Allocator.TempJob);
		NativeArray<uint2> vertPerCell = new NativeArray<uint2>(totalSize, Allocator.TempJob);
		NativeArray<uint> compactedVoxel = new NativeArray<uint>(totalSize, Allocator.TempJob);


		var countVJob = new CountVertexPerVoxelJob() {
			densV = values,
			nbTriTable = nbTriTable,
			triTable = triTable,
			vertPerCell = vertPerCellIn,
			gridSize = gridSize,
			totalVoxel = totalSize,
			isoValue = isoValue
		};

		var countVJobHandle = countVJob.Schedule(totalSize, 128);
		countVJobHandle.Complete();


		//exclusivescan => compute the total number of vertices
		uint2 lastElem = vertPerCellIn[totalSize - 1];

		float timerEsc = Time.realtimeSinceStartup;

		var escanJob = new ExclusiveScanTrivialJob() {
			vertPerCell = vertPerCellIn,
			result = vertPerCell,
			totalVoxel = totalSize
		};

		var escanJobJobHandle = escanJob.Schedule();
		escanJobJobHandle.Complete();


		uint2 lastScanElem = vertPerCell[totalSize - 1];

		uint newTotalVoxels = lastElem.y + lastScanElem.y;
		uint totalVerts = lastElem.x + lastScanElem.x;

		if (totalVerts <= 0 || newTotalVoxels <= 0) {
			Debug.LogWarning("Empty iso-surface");
			vertPerCellIn.Dispose();
			vertPerCell.Dispose();
			compactedVoxel.Dispose();
			return;
		}

		curVertices = new NativeArray<float3>((int)totalVerts, Allocator.Persistent);
		curNormals = new NativeArray<float3>((int)totalVerts, Allocator.Persistent);
		curColors = new NativeArray<Color32>((int) totalVerts, Allocator.Persistent);
		vertexToCellId = new NativeArray<int>((int) totalVerts, Allocator.Persistent);
		//Double the triangles to have both faces
		curTriangles = new NativeArray<int>((int)totalVerts * 2, Allocator.Persistent);
		// NativeArray<int3> savedVoxelId = new NativeArray<int3>((int)totalVerts, Allocator.TempJob);

		//compactvoxels

		var compactJob = new CompactVoxelJob() {
			vertPerCell = vertPerCell,
			compVoxel = compactedVoxel,
			gridSize = gridSize,
			totalVoxel = totalSize,
			lastElem = lastElem.y
		};

		var compactJobHandle = compactJob.Schedule(totalSize, 128);
		compactJobHandle.Complete();


		//MC
		var MCJob = new MarchingCubesJob() {
			// voxelIds = savedVoxelId,
			vertices = curVertices,
			normals = curNormals,
			colors = curColors,
			compVoxel = compactedVoxel,
			vertPerCell = vertPerCell,
			cellIdPerVert = vertexToCellId,
			densV = values,
			gradient = grad,
			nbTriTable = nbTriTable,
			triTable = triTable,
			oriGrid = originGrid,
			dx = natDeltas,
			dirs = natDir,
			gridSize = gridSize,
			isoValue = isoValue,
			totalVerts = totalVerts,
			useGrad = useGradient
		};

		var MCJobHandle = MCJob.Schedule((int)newTotalVoxels, 128);
		MCJobHandle.Complete();

		// //Normals
		// var NormJob = new ComputeNormalsJob() {
		// 	normals = curNormals,
		// 	vertices = curVertices,
		// 	voxelIds = savedVoxelId,
		// 	colors = curColors,
		// 	densV = values,
		// 	gradient, grad,
		// 	oriGrid = originGrid,
		// 	dx = natDeltas,
		// 	dirs = natDir,
		// 	gridSize = gridSize
		// };
		// var NormJobHandle = NormJob.Schedule((int)totalVerts, 128);
		// NormJobHandle.Complete();

		for (int i = 0; i < totalVerts - 3; i += 3) {
			curTriangles[i] = i;
			curTriangles[i + 1] = i + 1;
			curTriangles[i + 2] = i + 2;
		}
		//Double the triangles to have both faces
		for (int i = (int)totalVerts; i < totalVerts * 2 - 3; i += 3) {
			curTriangles[i] = i - (int)totalVerts;
			curTriangles[i + 2] = i + 1 - (int)totalVerts; //Invert triangles here
			curTriangles[i + 1] = i + 2 - (int)totalVerts;
		}

		vertPerCellIn.Dispose();
		vertPerCell.Dispose();
		compactedVoxel.Dispose();
		// savedVoxelId.Dispose();
	}

	public Vector3[] getVertices() {
		Vector3[] res = new Vector3[curVertices.Length];
		SetNativeVertexArray(res, curVertices);
		return res;
	}

	public Vector3[] getNormals() {
		Vector3[] res = new Vector3[curNormals.Length];
		SetNativeVertexArray(res, curNormals);
		return res;
	}

	public int[] getTriangles() {
		int[] res = new int[curTriangles.Length];
		SetNativeTriangleArray(res, curTriangles);
		return res;
	}
	public Color32[] getColors() {
		Color32[] res = new Color32[curVertices.Length];
		SetNativeColorArray(res, curColors);
		return res;
	}
	public int[] getVertexToCellId() {
		int[] res = new int[curVertices.Length];
		SetNativeTriangleArray(res, vertexToCellId);
		return res;
	}

	public void Clean() {

		if (values.IsCreated)
			values.Dispose();
		if (grad.IsCreated)
			grad.Dispose();
		if (nbTriTable.IsCreated)
			nbTriTable.Dispose();
		if (triTable.IsCreated)
			triTable.Dispose();
		if (curVertices.IsCreated)
			curVertices.Dispose();
		if (curColors.IsCreated)
			curColors.Dispose();
		if (curNormals.IsCreated)
			curNormals.Dispose();
		if (curTriangles.IsCreated)
			curTriangles.Dispose();
		if (natDeltas.IsCreated)
			natDeltas.Dispose();
		if (natDir.IsCreated)
			natDir.Dispose();
		if (vertexToCellId.IsCreated)
			vertexToCellId.Dispose();
	}


	//From https://gist.github.com/LotteMakesStuff/c2f9b764b15f74d14c00ceb4214356b4
	unsafe void SetNativeVertexArray(Vector3[] vertexArray, NativeArray<float3> vertexBuffer)
	{
		// pin the target vertex array and get a pointer to it
		fixed (void* vertexArrayPointer = vertexArray)
		{
			// memcopy the native array over the top
			UnsafeUtility.MemCpy(vertexArrayPointer, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(vertexBuffer), vertexArray.Length * (long) UnsafeUtility.SizeOf<float3>());
		}
	}

	//From https://gist.github.com/LotteMakesStuff/c2f9b764b15f74d14c00ceb4214356b4
	unsafe void SetNativeTriangleArray(int[] triArray, NativeArray<int> triBuffer)
	{
		// pin the target vertex array and get a pointer to it
		fixed (void* triArrayPointer = triArray)
		{
			// memcopy the native array over the top
			UnsafeUtility.MemCpy(triArrayPointer, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(triBuffer), triArray.Length * (long) UnsafeUtility.SizeOf<int>());
		}
	}
	//From https://gist.github.com/LotteMakesStuff/c2f9b764b15f74d14c00ceb4214356b4
	unsafe void SetNativeColorArray(Color32[] colArray, NativeArray<Color32> colBuffer)
	{
		// pin the target vertex array and get a pointer to it
		fixed (void* colArrayPointer = colArray)
		{
			// memcopy the native array over the top
			UnsafeUtility.MemCpy(colArrayPointer, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(colBuffer), colArray.Length * (long) UnsafeUtility.SizeOf<Color32>());
		}
	}
	//From https://gist.github.com/LotteMakesStuff/c2f9b764b15f74d14c00ceb4214356b4
	unsafe void GetNativeArray(NativeArray<float> vNativ, float[] vArray)
	{

		// pin the buffer in place...
		fixed (void* bufferPointer = vArray)
		{
			// ...and use memcpy to copy the Vector3[] into a NativeArray<floar3> without casting. whould be fast!
			UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(vNativ),
			                     bufferPointer, vArray.Length * (long) UnsafeUtility.SizeOf<float>());
		}
		// we only have to fix the .net array in place, the NativeArray is allocated in the C++ side of the engine and
		// wont move arround unexpectedly. We have a pointer to it not a reference! thats basically what fixed does,
		// we create a scope where its 'safe' to get a pointer and directly manipulate the array
	}
	unsafe void GetNativeArray(NativeArray<float3> vNativ, Vector3[] vArray)
	{

		// pin the buffer in place...
		fixed (void* bufferPointer = vArray)
		{
			// ...and use memcpy to copy the Vector3[] into a NativeArray<floar3> without casting. whould be fast!
			UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(vNativ),
			                     bufferPointer, vArray.Length * (long) UnsafeUtility.SizeOf<float3>());
		}
		// we only have to fix the .net array in place, the NativeArray is allocated in the C++ side of the engine and
		// wont move arround unexpectedly. We have a pointer to it not a reference! thats basically what fixed does,
		// we create a scope where its 'safe' to get a pointer and directly manipulate the array
	}
	static int3 to3D(int id, int3 gridDim) {
		int3 res;
		res.x = id / (gridDim.y * gridDim.z); //Note the integer division . This is x
		res.y = (id - res.x * gridDim.y * gridDim.z) / gridDim.z; //This is y
		res.z = id - res.x * gridDim.y * gridDim.z - res.y * gridDim.z; //This is z
		return res;
	}


	static int to1D(int3 ids, int3 dim) {
		return (dim.y * dim.z * ids.x) + (dim.z * ids.y) + ids.z;
	}
	static int btoi(bool v) {
		if (v)
			return 1;
		return 0;
	}


	[BurstCompile]
	struct CountVertexPerVoxelJob : IJobParallelFor
	{
		[ReadOnly] public NativeArray<float> densV;
		[ReadOnly] public NativeArray<int> nbTriTable;
		[ReadOnly] public NativeArray<int> triTable;
		public NativeArray<uint2> vertPerCell;
		[ReadOnly] public int3 gridSize;
		[ReadOnly] public int totalVoxel;
		public float isoValue;

		void IJobParallelFor.Execute(int index) {
			uint2 Nverts;
			Nverts.x = 0;
			Nverts.y = 0;
			int3 ijk = to3D(index, gridSize);

			if (ijk.x > (gridSize.x - 2) || ijk.y > (gridSize.y - 2) || ijk.z > (gridSize.z - 2)) {
				vertPerCell[index] = Nverts;
				return;
			}

			float voxel0 = densV[to1D(ijk, gridSize)];
			float voxel1 = densV[to1D(ijk + new int3(1, 0, 0), gridSize)];
			float voxel2 = densV[to1D(ijk + new int3(1, 1, 0), gridSize)];
			float voxel3 = densV[to1D(ijk + new int3(0, 1, 0), gridSize)];
			float voxel4 = densV[to1D(ijk + new int3(0, 0, 1), gridSize)];
			float voxel5 = densV[to1D(ijk + new int3(1, 0, 1), gridSize)];
			float voxel6 = densV[to1D(ijk + new int3(1, 1, 1), gridSize)];
			float voxel7 = densV[to1D(ijk + new int3(0, 1, 1), gridSize)];

			int cubeIndex =   btoi(voxel0 < isoValue);
			cubeIndex += (btoi(voxel1 < isoValue)) * 2;
			cubeIndex += (btoi(voxel2 < isoValue)) * 4;
			cubeIndex += (btoi(voxel3 < isoValue)) * 8;
			cubeIndex += (btoi(voxel4 < isoValue)) * 16;
			cubeIndex += (btoi(voxel5 < isoValue)) * 32;
			cubeIndex += (btoi(voxel6 < isoValue)) * 64;
			cubeIndex += (btoi(voxel7 < isoValue)) * 128;

			Nverts.x = (uint)nbTriTable[cubeIndex];
			Nverts.y =  (uint)btoi(nbTriTable[cubeIndex] > 0);
			vertPerCell[index] = Nverts;
		}

	}

	[BurstCompile]
	struct ExclusiveScanTrivialJob : IJob
	{
		public NativeArray<uint2> vertPerCell;
		public NativeArray<uint2> result;
		[ReadOnly] public int totalVoxel;

		void IJob.Execute() {
			result[0] = new uint2(0, 0);
			for (int i = 1; i < totalVoxel; i++) {
				result[i] = vertPerCell[i - 1] + result[i - 1];
			}
			result[0] = new uint2(0, 0);
		}
	}


	[BurstCompile]
	struct CompactVoxelJob : IJobParallelFor
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<uint> compVoxel;
		[ReadOnly] public NativeArray<uint2> vertPerCell;
		[ReadOnly] public int3 gridSize;
		[ReadOnly] public int totalVoxel;
		[ReadOnly] public uint lastElem;

		void IJobParallelFor.Execute(int index) {

			if (  (index < totalVoxel - 1) ? vertPerCell[index].y < vertPerCell[index + 1].y : lastElem > 0) {
				compVoxel[ (int)vertPerCell[index].y ] = (uint)index;
			}
		}
	}


	[BurstCompile]
	struct MarchingCubesJob : IJobParallelFor
	{

		[NativeDisableParallelForRestriction]
		public NativeArray<float3> vertices;
		// [NativeDisableParallelForRestriction]
		// public NativeArray<int3> voxelIds;
		[NativeDisableParallelForRestriction]
		public NativeArray<float3> normals;

		[NativeDisableParallelForRestriction]
		public NativeArray<Color32> colors;

		[NativeDisableParallelForRestriction]
		public NativeArray<int> cellIdPerVert;

		[ReadOnly] public NativeArray<uint> compVoxel;
		[ReadOnly] public NativeArray<uint2> vertPerCell;
		[ReadOnly] public NativeArray<float> densV;
		[ReadOnly] public NativeArray<float3> gradient;
		[ReadOnly] public NativeArray<int> nbTriTable;
		[ReadOnly] public NativeArray<int> triTable;
		[ReadOnly] public float3 oriGrid;
		[ReadOnly] public NativeArray<float3> dx;
		[ReadOnly] public NativeArray<float3> dirs;
		[ReadOnly] public int3 gridSize;
		[ReadOnly] public float isoValue;
		[ReadOnly] public float totalVerts;
		[ReadOnly] public bool useGrad;

		void IJobParallelFor.Execute(int index) {
			int voxel = (int)compVoxel[index];
			int3 ijk = to3D(voxel, gridSize);
			float3 zer = float3.zero;
			float3 p = gridToSpace(ijk);
			float3 offs = dx[0];
			float3 v0 = p;
			float3 v1 = p + offs;
			offs += dx[1];
			// offs.x = dx; offs.y = dx; offs.z = 0.0f;
			float3 v2 = p + offs;
			offs = dx[1];
			// offs.x = 0.0f; offs.y = dx; offs.z = 0.0f;
			float3 v3 = p + offs;
			offs = dx[2];
			// offs.x = 0.0f; offs.y = 0.0f; offs.z = dx;
			float3 v4 = p + offs;
			offs = dx[0] + dx[2];
			// offs.x = dx; offs.y = 0.0f; offs.z = dx;
			float3 v5 = p + offs;
			offs = dx[0] + dx[1] + dx[2];
			// offs.x = dx; offs.y = dx; offs.z = dx;
			float3 v6 = p + offs;
			offs = dx[1] + dx[2];
			// offs.x = 0.0f; offs.y = dx; offs.z = dx;
			float3 v7 = p + offs;

			float voxel0 = densV[to1D(ijk, gridSize)];
			float voxel1 = densV[to1D(ijk + new int3(1, 0, 0), gridSize)];
			float voxel2 = densV[to1D(ijk + new int3(1, 1, 0), gridSize)];
			float voxel3 = densV[to1D(ijk + new int3(0, 1, 0), gridSize)];
			float voxel4 = densV[to1D(ijk + new int3(0, 0, 1), gridSize)];
			float voxel5 = densV[to1D(ijk + new int3(1, 0, 1), gridSize)];
			float voxel6 = densV[to1D(ijk + new int3(1, 1, 1), gridSize)];
			float voxel7 = densV[to1D(ijk + new int3(0, 1, 1), gridSize)];

			float3 grad0 = 0.0f;
			float3 grad1 = 0.0f;
			float3 grad2 = 0.0f;
			float3 grad3 = 0.0f;
			float3 grad4 = 0.0f;
			float3 grad5 = 0.0f;
			float3 grad6 = 0.0f;
			float3 grad7 = 0.0f;

			if (useGrad) {
				grad0 = gradient[to1D(ijk, gridSize)];
				grad1 = gradient[to1D(ijk + new int3(1, 0, 0), gridSize)];
				grad2 = gradient[to1D(ijk + new int3(1, 1, 0), gridSize)];
				grad3 = gradient[to1D(ijk + new int3(0, 1, 0), gridSize)];
				grad4 = gradient[to1D(ijk + new int3(0, 0, 1), gridSize)];
				grad5 = gradient[to1D(ijk + new int3(1, 0, 1), gridSize)];
				grad6 = gradient[to1D(ijk + new int3(1, 1, 1), gridSize)];
				grad7 = gradient[to1D(ijk + new int3(0, 1, 1), gridSize)];
			}


			int cubeIndex =   btoi(voxel0 < isoValue);
			cubeIndex += (btoi(voxel1 < isoValue)) * 2;
			cubeIndex += (btoi(voxel2 < isoValue)) * 4;
			cubeIndex += (btoi(voxel3 < isoValue)) * 8;
			cubeIndex += (btoi(voxel4 < isoValue)) * 16;
			cubeIndex += (btoi(voxel5 < isoValue)) * 32;
			cubeIndex += (btoi(voxel6 < isoValue)) * 64;
			cubeIndex += (btoi(voxel7 < isoValue)) * 128;

			float3 verts0 = vertexInterp(isoValue, v0, v1, voxel0, voxel1);
			float3 verts1 = vertexInterp(isoValue, v1, v2, voxel1, voxel2);
			float3 verts2 = vertexInterp(isoValue, v2, v3, voxel2, voxel3);
			float3 verts3 = vertexInterp(isoValue, v3, v0, voxel3, voxel0);
			float3 verts4 = vertexInterp(isoValue, v4, v5, voxel4, voxel5);
			float3 verts5 = vertexInterp(isoValue, v5, v6, voxel5, voxel6);
			float3 verts6 = vertexInterp(isoValue, v6, v7, voxel6, voxel7);
			float3 verts7 = vertexInterp(isoValue, v7, v4, voxel7, voxel4);
			float3 verts8 = vertexInterp(isoValue, v0, v4, voxel0, voxel4);
			float3 verts9 = vertexInterp(isoValue, v1, v5, voxel1, voxel5);
			float3 verts10 = vertexInterp(isoValue, v2, v6, voxel2, voxel6);
			float3 verts11 = vertexInterp(isoValue, v3, v7, voxel3, voxel7);

			float3 norm0 = normalInterp(isoValue, voxel0, voxel1, grad0, grad1);
			float3 norm1 = normalInterp(isoValue, voxel1, voxel2, grad1, grad2);
			float3 norm2 = normalInterp(isoValue, voxel2, voxel3, grad2, grad3);
			float3 norm3 = normalInterp(isoValue, voxel3, voxel0, grad3, grad0);
			float3 norm4 = normalInterp(isoValue, voxel4, voxel5, grad4, grad5);
			float3 norm5 = normalInterp(isoValue, voxel5, voxel6, grad5, grad6);
			float3 norm6 = normalInterp(isoValue, voxel6, voxel7, grad6, grad7);
			float3 norm7 = normalInterp(isoValue, voxel7, voxel4, grad7, grad4);
			float3 norm8 = normalInterp(isoValue, voxel0, voxel4, grad0, grad4);
			float3 norm9 = normalInterp(isoValue, voxel1, voxel5, grad1, grad5);
			float3 norm10 = normalInterp(isoValue, voxel2, voxel6, grad2, grad6);
			float3 norm11 = normalInterp(isoValue, voxel3, voxel7, grad3, grad7);

			int numVerts = nbTriTable[cubeIndex];

			for (int i = 0; i < numVerts; i += 3) {

				int id = (int)vertPerCell[(int)voxel].x + i;
				if (id >= totalVerts - 3)
					return;
				int edge = triTable[i + cubeIndex * 16]; // ==> triTable[cubeIndex][i]

				//Avoid using an array by doing a lot of if...
				//TODO: improve that part
				if (edge == 0) {vertices[id] = verts0; normals[id] = norm0; cellIdPerVert[id] = voxel;}
				else if (edge == 1 ) {vertices[id] = verts1 ; normals[id] = norm1 ; cellIdPerVert[id] = voxel;}
				else if (edge == 2 ) {vertices[id] = verts2 ; normals[id] = norm2 ; cellIdPerVert[id] = voxel;}
				else if (edge == 3 ) {vertices[id] = verts3 ; normals[id] = norm3 ; cellIdPerVert[id] = voxel;}
				else if (edge == 4 ) {vertices[id] = verts4 ; normals[id] = norm4 ; cellIdPerVert[id] = voxel;}
				else if (edge == 5 ) {vertices[id] = verts5 ; normals[id] = norm5 ; cellIdPerVert[id] = voxel;}
				else if (edge == 6 ) {vertices[id] = verts6 ; normals[id] = norm6 ; cellIdPerVert[id] = voxel;}
				else if (edge == 7 ) {vertices[id] = verts7 ; normals[id] = norm7 ; cellIdPerVert[id] = voxel;}
				else if (edge == 8 ) {vertices[id] = verts8 ; normals[id] = norm8 ; cellIdPerVert[id] = voxel;}
				else if (edge == 9 ) {vertices[id] = verts9 ; normals[id] = norm9 ; cellIdPerVert[id] = voxel;}
				else if (edge == 10) {vertices[id] = verts10; normals[id] = norm10; cellIdPerVert[id] = voxel;}
				else if (edge == 11) {vertices[id] = verts11; normals[id] = norm11; cellIdPerVert[id] = voxel;}

				edge = triTable[(i + 1) + cubeIndex * 16];
				if (edge == 0) {vertices[id + 1] = verts0; normals[id + 1] = norm0; cellIdPerVert[id + 1] = voxel;}
				else if (edge == 1 ) {vertices[id + 1] = verts1 ; normals[id + 1] = norm1 ; cellIdPerVert[id + 1] = voxel;}
				else if (edge == 2 ) {vertices[id + 1] = verts2 ; normals[id + 1] = norm2 ; cellIdPerVert[id + 1] = voxel;}
				else if (edge == 3 ) {vertices[id + 1] = verts3 ; normals[id + 1] = norm3 ; cellIdPerVert[id + 1] = voxel;}
				else if (edge == 4 ) {vertices[id + 1] = verts4 ; normals[id + 1] = norm4 ; cellIdPerVert[id + 1] = voxel;}
				else if (edge == 5 ) {vertices[id + 1] = verts5 ; normals[id + 1] = norm5 ; cellIdPerVert[id + 1] = voxel;}
				else if (edge == 6 ) {vertices[id + 1] = verts6 ; normals[id + 1] = norm6 ; cellIdPerVert[id + 1] = voxel;}
				else if (edge == 7 ) {vertices[id + 1] = verts7 ; normals[id + 1] = norm7 ; cellIdPerVert[id + 1] = voxel;}
				else if (edge == 8 ) {vertices[id + 1] = verts8 ; normals[id + 1] = norm8 ; cellIdPerVert[id + 1] = voxel;}
				else if (edge == 9 ) {vertices[id + 1] = verts9 ; normals[id + 1] = norm9 ; cellIdPerVert[id + 1] = voxel;}
				else if (edge == 10) {vertices[id + 1] = verts10; normals[id + 1] = norm10; cellIdPerVert[id + 1] = voxel;}
				else if (edge == 11) {vertices[id + 1] = verts11; normals[id + 1] = norm11; cellIdPerVert[id + 1] = voxel;}

				edge = triTable[(i + 2) + cubeIndex * 16];
				if (edge == 0) {vertices[id + 2] = verts0; normals[id + 2] = norm0; cellIdPerVert[id + 2] = voxel;}
				else if (edge == 1 ) {vertices[id + 2] = verts1 ; normals[id + 2] = norm1 ; cellIdPerVert[id + 2] = voxel;}
				else if (edge == 2 ) {vertices[id + 2] = verts2 ; normals[id + 2] = norm2 ; cellIdPerVert[id + 2] = voxel;}
				else if (edge == 3 ) {vertices[id + 2] = verts3 ; normals[id + 2] = norm3 ; cellIdPerVert[id + 2] = voxel;}
				else if (edge == 4 ) {vertices[id + 2] = verts4 ; normals[id + 2] = norm4 ; cellIdPerVert[id + 2] = voxel;}
				else if (edge == 5 ) {vertices[id + 2] = verts5 ; normals[id + 2] = norm5 ; cellIdPerVert[id + 2] = voxel;}
				else if (edge == 6 ) {vertices[id + 2] = verts6 ; normals[id + 2] = norm6 ; cellIdPerVert[id + 2] = voxel;}
				else if (edge == 7 ) {vertices[id + 2] = verts7 ; normals[id + 2] = norm7 ; cellIdPerVert[id + 2] = voxel;}
				else if (edge == 8 ) {vertices[id + 2] = verts8 ; normals[id + 2] = norm8 ; cellIdPerVert[id + 2] = voxel;}
				else if (edge == 9 ) {vertices[id + 2] = verts9 ; normals[id + 2] = norm9 ; cellIdPerVert[id + 2] = voxel;}
				else if (edge == 10) {vertices[id + 2] = verts10; normals[id + 2] = norm10; cellIdPerVert[id + 2] = voxel;}
				else if (edge == 11) {vertices[id + 2] = verts11; normals[id + 2] = norm11; cellIdPerVert[id + 2] = voxel;}

				colors[id + 0] = new Color32(255, 255, 255, 255);
				colors[id + 1] = new Color32(255, 255, 255, 255);
				colors[id + 2] = new Color32(255, 255, 255, 255);
			}
		}


		float3 vertexInterp(float iso, float3 p0, float3 p1, float f0, float f1) {
			float t = (iso - f0) / (f1 - f0);
			return math.lerp(p0, p1, t) /** new float3(-1, 1, 1)*/;
		}
		float3 normalInterp(float iso, float f0, float f1, float3 g0, float3 g1) {

			float isodiff = iso - f0;
			float diff = f1 - f0;
			float mu = 0.5f;

			if (math.abs(diff) > 0.0f)
				mu = isodiff / diff;

			float3 norm = new float3( g0.x + mu * (g1.x - g0.x),
			                          g0.y + mu * (g1.y - g0.y),
			                          g0.z + mu * (g1.z - g0.z));

			norm = scaleNorm(norm);
			return norm;
		}

		float3 scaleNorm(float3 n) {
			float3 sn = n;
			sn.x = -n.x * dirs[0].x + n.y * dirs[0].y + n.z * dirs[0].z;
			sn.y = -n.x * dirs[1].x + n.y * dirs[1].y + n.z * dirs[1].z;
			sn.z = -n.x * dirs[2].x + n.y * dirs[2].y + n.z * dirs[2].z;
			return math.normalize(sn);
		}
		float3 gridToSpace(int3 cellPos) {

			float3 cp = oriGrid;
			cp += cellPos.x * dirs[0];
			cp += cellPos.y * dirs[1];
			cp += cellPos.z * dirs[2];

			return cp;
		}

	}

	public static int[,] managed_triTable = new int[,] {
		{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 1, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 8, 3, 9, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 8, 3, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 9, 2, 10, 0, 2, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 2, 8, 3, 2, 10, 8, 10, 9, 8, -1, -1, -1, -1, -1, -1, -1 },
		{ 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 11, 2, 8, 11, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 9, 0, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 11, 2, 1, 9, 11, 9, 8, 11, -1, -1, -1, -1, -1, -1, -1 },
		{ 3, 10, 1, 11, 10, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 10, 1, 0, 8, 10, 8, 11, 10, -1, -1, -1, -1, -1, -1, -1 },
		{ 3, 9, 0, 3, 11, 9, 11, 10, 9, -1, -1, -1, -1, -1, -1, -1 },
		{ 9, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 4, 3, 0, 7, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 1, 9, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 4, 1, 9, 4, 7, 1, 7, 3, 1, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 2, 10, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 3, 4, 7, 3, 0, 4, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1 },
		{ 9, 2, 10, 9, 0, 2, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1 },
		{ 2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, -1, -1, -1, -1 },
		{ 8, 4, 7, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 11, 4, 7, 11, 2, 4, 2, 0, 4, -1, -1, -1, -1, -1, -1, -1 },
		{ 9, 0, 1, 8, 4, 7, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1 },
		{ 4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, -1, -1, -1, -1 },
		{ 3, 10, 1, 3, 11, 10, 7, 8, 4, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, -1, -1, -1, -1 },
		{ 4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, -1, -1, -1, -1 },
		{ 4, 7, 11, 4, 11, 9, 9, 11, 10, -1, -1, -1, -1, -1, -1, -1 },
		{ 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 9, 5, 4, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 5, 4, 1, 5, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 8, 5, 4, 8, 3, 5, 3, 1, 5, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 2, 10, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 3, 0, 8, 1, 2, 10, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1 },
		{ 5, 2, 10, 5, 4, 2, 4, 0, 2, -1, -1, -1, -1, -1, -1, -1 },
		{ 2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, -1, -1, -1, -1 },
		{ 9, 5, 4, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 11, 2, 0, 8, 11, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 5, 4, 0, 1, 5, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1 },
		{ 2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, -1, -1, -1, -1 },
		{ 10, 3, 11, 10, 1, 3, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1 },
		{ 4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, -1, -1, -1, -1 },
		{ 5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, -1, -1, -1, -1 },
		{ 5, 4, 8, 5, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1 },
		{ 9, 7, 8, 5, 7, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 9, 3, 0, 9, 5, 3, 5, 7, 3, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 7, 8, 0, 1, 7, 1, 5, 7, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 9, 7, 8, 9, 5, 7, 10, 1, 2, -1, -1, -1, -1, -1, -1, -1 },
		{ 10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, -1, -1, -1, -1 },
		{ 8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, -1, -1, -1, -1 },
		{ 2, 10, 5, 2, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1 },
		{ 7, 9, 5, 7, 8, 9, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1 },
		{ 9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, -1, -1, -1, -1 },
		{ 2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, -1, -1, -1, -1 },
		{ 11, 2, 1, 11, 1, 7, 7, 1, 5, -1, -1, -1, -1, -1, -1, -1 },
		{ 9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, -1, -1, -1, -1 },
		{ 5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, -1 },
		{ 11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, -1 },
		{ 11, 10, 5, 7, 11, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 8, 3, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 9, 0, 1, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 8, 3, 1, 9, 8, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 6, 5, 2, 6, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 6, 5, 1, 2, 6, 3, 0, 8, -1, -1, -1, -1, -1, -1, -1 },
		{ 9, 6, 5, 9, 0, 6, 0, 2, 6, -1, -1, -1, -1, -1, -1, -1 },
		{ 5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, -1, -1, -1, -1 },
		{ 2, 3, 11, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 11, 0, 8, 11, 2, 0, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 1, 9, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1 },
		{ 5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, -1, -1, -1, -1 },
		{ 6, 3, 11, 6, 5, 3, 5, 1, 3, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, -1, -1, -1, -1 },
		{ 3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9, -1, -1, -1, -1 },
		{ 6, 5, 9, 6, 9, 11, 11, 9, 8, -1, -1, -1, -1, -1, -1, -1 },
		{ 5, 10, 6, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 4, 3, 0, 4, 7, 3, 6, 5, 10, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 9, 0, 5, 10, 6, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1 },
		{ 10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4, -1, -1, -1, -1 },
		{ 6, 1, 2, 6, 5, 1, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7, -1, -1, -1, -1 },
		{ 8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6, -1, -1, -1, -1 },
		{ 7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9, -1 },
		{ 3, 11, 2, 7, 8, 4, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1 },
		{ 5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11, -1, -1, -1, -1 },
		{ 0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1 },
		{ 9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6, -1 },
		{ 8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6, -1, -1, -1, -1 },
		{ 5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11, -1 },
		{ 0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7, -1 },
		{ 6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9, -1, -1, -1, -1 },
		{ 10, 4, 9, 6, 4, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 4, 10, 6, 4, 9, 10, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1 },
		{ 10, 0, 1, 10, 6, 0, 6, 4, 0, -1, -1, -1, -1, -1, -1, -1 },
		{ 8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10, -1, -1, -1, -1 },
		{ 1, 4, 9, 1, 2, 4, 2, 6, 4, -1, -1, -1, -1, -1, -1, -1 },
		{ 3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4, -1, -1, -1, -1 },
		{ 0, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 8, 3, 2, 8, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1 },
		{ 10, 4, 9, 10, 6, 4, 11, 2, 3, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6, -1, -1, -1, -1 },
		{ 3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10, -1, -1, -1, -1 },
		{ 6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1, -1 },
		{ 9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3, -1, -1, -1, -1 },
		{ 8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1, -1 },
		{ 3, 11, 6, 3, 6, 0, 0, 6, 4, -1, -1, -1, -1, -1, -1, -1 },
		{ 6, 4, 8, 11, 6, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 7, 10, 6, 7, 8, 10, 8, 9, 10, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10, -1, -1, -1, -1 },
		{ 10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0, -1, -1, -1, -1 },
		{ 10, 6, 7, 10, 7, 1, 1, 7, 3, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7, -1, -1, -1, -1 },
		{ 2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9, -1 },
		{ 7, 8, 0, 7, 0, 6, 6, 0, 2, -1, -1, -1, -1, -1, -1, -1 },
		{ 7, 3, 2, 6, 7, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7, -1, -1, -1, -1 },
		{ 2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7, -1 },
		{ 1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11, -1 },
		{ 11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1, -1, -1, -1, -1 },
		{ 8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6, -1 },
		{ 0, 9, 1, 11, 6, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0, -1, -1, -1, -1 },
		{ 7, 11, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 3, 0, 8, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 1, 9, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 8, 1, 9, 8, 3, 1, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1 },
		{ 10, 1, 2, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 2, 10, 3, 0, 8, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1 },
		{ 2, 9, 0, 2, 10, 9, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1 },
		{ 6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8, -1, -1, -1, -1 },
		{ 7, 2, 3, 6, 2, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 7, 0, 8, 7, 6, 0, 6, 2, 0, -1, -1, -1, -1, -1, -1, -1 },
		{ 2, 7, 6, 2, 3, 7, 0, 1, 9, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6, -1, -1, -1, -1 },
		{ 10, 7, 6, 10, 1, 7, 1, 3, 7, -1, -1, -1, -1, -1, -1, -1 },
		{ 10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8, -1, -1, -1, -1 },
		{ 0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7, -1, -1, -1, -1 },
		{ 7, 6, 10, 7, 10, 8, 8, 10, 9, -1, -1, -1, -1, -1, -1, -1 },
		{ 6, 8, 4, 11, 8, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 3, 6, 11, 3, 0, 6, 0, 4, 6, -1, -1, -1, -1, -1, -1, -1 },
		{ 8, 6, 11, 8, 4, 6, 9, 0, 1, -1, -1, -1, -1, -1, -1, -1 },
		{ 9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6, -1, -1, -1, -1 },
		{ 6, 8, 4, 6, 11, 8, 2, 10, 1, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6, -1, -1, -1, -1 },
		{ 4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9, -1, -1, -1, -1 },
		{ 10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3, -1 },
		{ 8, 2, 3, 8, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8, -1, -1, -1, -1 },
		{ 1, 9, 4, 1, 4, 2, 2, 4, 6, -1, -1, -1, -1, -1, -1, -1 },
		{ 8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1, -1, -1, -1, -1 },
		{ 10, 1, 0, 10, 0, 6, 6, 0, 4, -1, -1, -1, -1, -1, -1, -1 },
		{ 4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3, -1 },
		{ 10, 9, 4, 6, 10, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 4, 9, 5, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 8, 3, 4, 9, 5, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1 },
		{ 5, 0, 1, 5, 4, 0, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1 },
		{ 11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5, -1, -1, -1, -1 },
		{ 9, 5, 4, 10, 1, 2, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1 },
		{ 6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5, -1, -1, -1, -1 },
		{ 7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2, -1, -1, -1, -1 },
		{ 3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6, -1 },
		{ 7, 2, 3, 7, 6, 2, 5, 4, 9, -1, -1, -1, -1, -1, -1, -1 },
		{ 9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7, -1, -1, -1, -1 },
		{ 3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0, -1, -1, -1, -1 },
		{ 6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8, -1 },
		{ 9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7, -1, -1, -1, -1 },
		{ 1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4, -1 },
		{ 4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10, -1 },
		{ 7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10, -1, -1, -1, -1 },
		{ 6, 9, 5, 6, 11, 9, 11, 8, 9, -1, -1, -1, -1, -1, -1, -1 },
		{ 3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5, -1, -1, -1, -1 },
		{ 0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11, -1, -1, -1, -1 },
		{ 6, 11, 3, 6, 3, 5, 5, 3, 1, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6, -1, -1, -1, -1 },
		{ 0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10, -1 },
		{ 11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5, -1 },
		{ 6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3, -1, -1, -1, -1 },
		{ 5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2, -1, -1, -1, -1 },
		{ 9, 5, 6, 9, 6, 0, 0, 6, 2, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8, -1 },
		{ 1, 5, 6, 2, 1, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6, -1 },
		{ 10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0, -1, -1, -1, -1 },
		{ 0, 3, 8, 5, 6, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 10, 5, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 11, 5, 10, 7, 5, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 11, 5, 10, 11, 7, 5, 8, 3, 0, -1, -1, -1, -1, -1, -1, -1 },
		{ 5, 11, 7, 5, 10, 11, 1, 9, 0, -1, -1, -1, -1, -1, -1, -1 },
		{ 10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1, -1, -1, -1, -1 },
		{ 11, 1, 2, 11, 7, 1, 7, 5, 1, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11, -1, -1, -1, -1 },
		{ 9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7, -1, -1, -1, -1 },
		{ 7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2, -1 },
		{ 2, 5, 10, 2, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1 },
		{ 8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5, -1, -1, -1, -1 },
		{ 9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2, -1, -1, -1, -1 },
		{ 9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2, -1 },
		{ 1, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 8, 7, 0, 7, 1, 1, 7, 5, -1, -1, -1, -1, -1, -1, -1 },
		{ 9, 0, 3, 9, 3, 5, 5, 3, 7, -1, -1, -1, -1, -1, -1, -1 },
		{ 9, 8, 7, 5, 9, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 5, 8, 4, 5, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1 },
		{ 5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0, -1, -1, -1, -1 },
		{ 0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5, -1, -1, -1, -1 },
		{ 10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4, -1 },
		{ 2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8, -1, -1, -1, -1 },
		{ 0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11, -1 },
		{ 0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5, -1 },
		{ 9, 4, 5, 2, 11, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4, -1, -1, -1, -1 },
		{ 5, 10, 2, 5, 2, 4, 4, 2, 0, -1, -1, -1, -1, -1, -1, -1 },
		{ 3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9, -1 },
		{ 5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2, -1, -1, -1, -1 },
		{ 8, 4, 5, 8, 5, 3, 3, 5, 1, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 4, 5, 1, 0, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5, -1, -1, -1, -1 },
		{ 9, 4, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 4, 11, 7, 4, 9, 11, 9, 10, 11, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11, -1, -1, -1, -1 },
		{ 1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11, -1, -1, -1, -1 },
		{ 3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4, -1 },
		{ 4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2, -1, -1, -1, -1 },
		{ 9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3, -1 },
		{ 11, 7, 4, 11, 4, 2, 2, 4, 0, -1, -1, -1, -1, -1, -1, -1 },
		{ 11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4, -1, -1, -1, -1 },
		{ 2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9, -1, -1, -1, -1 },
		{ 9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7, -1 },
		{ 3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10, -1 },
		{ 1, 10, 2, 8, 7, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 4, 9, 1, 4, 1, 7, 7, 1, 3, -1, -1, -1, -1, -1, -1, -1 },
		{ 4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1, -1, -1, -1, -1 },
		{ 4, 0, 3, 7, 4, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 4, 8, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 9, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 3, 0, 9, 3, 9, 11, 11, 9, 10, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 1, 10, 0, 10, 8, 8, 10, 11, -1, -1, -1, -1, -1, -1, -1 },
		{ 3, 1, 10, 11, 3, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 2, 11, 1, 11, 9, 9, 11, 8, -1, -1, -1, -1, -1, -1, -1 },
		{ 3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9, -1, -1, -1, -1 },
		{ 0, 2, 11, 8, 0, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 3, 2, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 2, 3, 8, 2, 8, 10, 10, 8, 9, -1, -1, -1, -1, -1, -1, -1 },
		{ 9, 10, 2, 0, 9, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8, -1, -1, -1, -1 },
		{ 1, 10, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 1, 3, 8, 9, 1, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 9, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ 0, 3, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
		{ -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }
	};

	public static int[] managed_nbTriTable = new int[] {
		0, 3, 3, 6, 3, 6, 6, 9, 3, 6, 6, 9, 6, 9, 9, 6, 3,
		6, 6, 9, 6, 9, 9, 12, 6, 9, 9, 12, 9, 12, 12, 9,
		3, 6, 6, 9, 6, 9, 9, 12, 6, 9, 9, 12, 9, 12, 12,
		9, 6, 9, 9, 6, 9, 12, 12, 9, 9, 12, 12, 9, 12,
		15, 15, 6, 3, 6, 6, 9, 6, 9, 9, 12, 6, 9, 9, 12,
		9, 12, 12, 9, 6, 9, 9, 12, 9, 12, 12, 15, 9, 12,
		12, 15, 12, 15, 15, 12, 6, 9, 9, 12, 9, 12, 6,
		9, 9, 12, 12, 15, 12, 15, 9, 6, 9, 12, 12, 9, 12,
		15, 9, 6, 12, 15, 15, 12, 15, 6, 12, 3, 3, 6, 6,
		9, 6, 9, 9, 12, 6, 9, 9, 12, 9, 12, 12, 9, 6, 9,
		9, 12, 9, 12, 12, 15, 9, 6, 12, 9, 12, 9, 15, 6,
		6, 9, 9, 12, 9, 12, 12, 15, 9, 12, 12, 15, 12,
		15, 15, 12, 9, 12, 12, 9, 12, 15, 15, 12, 12,
		9, 15, 6, 15, 12, 6, 3, 6, 9, 9, 12, 9, 12, 12,
		15, 9, 12, 12, 15, 6, 9, 9, 6, 9, 12, 12, 15,
		12, 15, 15, 6, 12, 9, 15, 12, 9, 6, 12, 3, 9,
		12, 12, 15, 12, 15, 9, 12, 12, 15, 15, 6, 9,
		12, 6, 3, 6, 9, 9, 6, 9, 12, 6, 3, 9, 6, 12, 3,
		6, 3, 3, 0
	};

}
}
