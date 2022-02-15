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
using Unity.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;

//TODO:
//Compute normals !
//Implement a parallel exclusive scan using C# job system, right now this is the slowest part (because done in serial) but should be the fastest
//Avoid copying tritable arrays to native arrays by correctly initialize them

namespace UMol {
public class MarchingCubesBurst {

	NativeArray<float> values;
	NativeArray<float3> curVertices;
	/// Not filled for now !
	NativeArray<float3> curNormals;
	NativeArray<int> curTriangles;
	NativeArray<int> nbTriTable;
	NativeArray<int> triTable;

	float dx;
	float3 originGrid;
	int3 gridSize;
	int totalSize;

	public MarchingCubesBurst(float[] densValues, Int3 gSize, Vector3 ori, float cellDim) {
		totalSize = gSize.x * gSize.y * gSize.z;
		originGrid = ori;
		dx = cellDim;
		if (totalSize != densValues.Length) {
			Debug.LogError("Something is wrong with data length " + totalSize + " != " + densValues.Length);
			return;
		}
		if (gSize.x > 512 || gSize.y > 512 || gSize.z > 512) {
			Debug.LogError("Really that big ?");
			return;
		}
		values = new NativeArray<float>(totalSize, Allocator.Persistent);

		initTriTable();
		GetNativeArray(values, densValues);

		gridSize.x = gSize.x;
		gridSize.y = gSize.y;
		gridSize.z = gSize.z;
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

		if (totalVerts <= 0) {
			Debug.LogWarning("Empty iso-surface");
			vertPerCellIn.Dispose();
			vertPerCell.Dispose();
			compactedVoxel.Dispose();
			return;
		}

		curVertices = new NativeArray<float3>((int)totalVerts, Allocator.Persistent);
		curNormals = new NativeArray<float3>((int)totalVerts, Allocator.Persistent);
		//Double the triangles to have both faces
		curTriangles = new NativeArray<int>((int)totalVerts * 2, Allocator.Persistent);

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
			vertices = curVertices,
			normals = curNormals,
			compVoxel = compactedVoxel,
			vertPerCell = vertPerCell,
			densV = values,
			nbTriTable = nbTriTable,
			triTable = triTable,
			oriGrid = originGrid,
			dx = dx,
			gridSize = gridSize,
			isoValue = isoValue,
			totalVerts = totalVerts
		};
		var MCJobHandle = MCJob.Schedule((int)newTotalVoxels, 128);
		MCJobHandle.Complete();

		//Normals
		var NormJob = new ComputeNormalsJob() {
			normals = curNormals,
			vertices = curVertices,
			densV = values,
			oriGrid = originGrid,
			dx = dx,
			gridSize = gridSize
		};
		var NormJobHandle = NormJob.Schedule((int)totalVerts, 128);
		NormJobHandle.Complete();
		
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

	public void Clean() {

		if (values.IsCreated)
			values.Dispose();
		if (nbTriTable.IsCreated)
			nbTriTable.Dispose();
		if (triTable.IsCreated)
			triTable.Dispose();
		if (curVertices.IsCreated)
			curVertices.Dispose();
		if (curNormals.IsCreated)
			curNormals.Dispose();
		if (curTriangles.IsCreated)
			curTriangles.Dispose();
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
		[NativeDisableParallelForRestriction]
		public NativeArray<float3> normals;
		[ReadOnly] public NativeArray<uint> compVoxel;
		[ReadOnly] public NativeArray<uint2> vertPerCell;
		[ReadOnly] public NativeArray<float> densV;
		[ReadOnly] public NativeArray<int> nbTriTable;
		[ReadOnly] public NativeArray<int> triTable;
		[ReadOnly] public float3 oriGrid;
		[ReadOnly] public float dx;
		[ReadOnly] public int3 gridSize;
		[ReadOnly] public float isoValue;
		[ReadOnly] public float totalVerts;

		void IJobParallelFor.Execute(int index) {
			int voxel = (int)compVoxel[index];
			int3 ijk = to3D(voxel, gridSize);
			float3 zer = float3.zero;
			float3 p = gridPosition(ijk, oriGrid, dx);
			float3 offs = new float3(dx, 0, 0);
			float3 v0 = p;
			float3 v1 = p + offs;
			offs.x = dx; offs.y = dx; offs.z = 0.0f;
			float3 v2 = p + offs;
			offs.x = 0.0f; offs.y = dx; offs.z = 0.0f;
			float3 v3 = p + offs;
			offs.x = 0.0f; offs.y = 0.0f; offs.z = dx;
			float3 v4 = p + offs;
			offs.x = dx; offs.y = 0.0f; offs.z = dx;
			float3 v5 = p + offs;
			offs.x = dx; offs.y = dx; offs.z = dx;
			float3 v6 = p + offs;
			offs.x = 0.0f; offs.y = dx; offs.z = dx;
			float3 v7 = p + offs;


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

			int numVerts = nbTriTable[cubeIndex];

			for (int i = 0; i < numVerts; i += 3) {

				int id = (int)vertPerCell[(int)voxel].x + i;
				if (id >= totalVerts - 3)
					return;
				int edge = triTable[i + cubeIndex * 16]; // ==> triTable[cubeIndex][i]

				//Avoid using an array by doing a lot of if...
				//TODO: improve that part
				if (edge == 0) vertices[id] = verts0;
				else if (edge == 1 ) vertices[id] = verts1 ;
				else if (edge == 2 ) vertices[id] = verts2 ;
				else if (edge == 3 ) vertices[id] = verts3 ;
				else if (edge == 4 ) vertices[id] = verts4 ;
				else if (edge == 5 ) vertices[id] = verts5 ;
				else if (edge == 6 ) vertices[id] = verts6 ;
				else if (edge == 7 ) vertices[id] = verts7 ;
				else if (edge == 8 ) vertices[id] = verts8 ;
				else if (edge == 9 ) vertices[id] = verts9 ;
				else if (edge == 10) vertices[id] = verts10;
				else if (edge == 11) vertices[id] = verts11;

				edge = triTable[(i + 1) + cubeIndex * 16];
				if (edge == 0) vertices[id + 1] = verts0;
				else if (edge == 1 ) vertices[id + 1] = verts1 ;
				else if (edge == 2 ) vertices[id + 1] = verts2 ;
				else if (edge == 3 ) vertices[id + 1] = verts3 ;
				else if (edge == 4 ) vertices[id + 1] = verts4 ;
				else if (edge == 5 ) vertices[id + 1] = verts5 ;
				else if (edge == 6 ) vertices[id + 1] = verts6 ;
				else if (edge == 7 ) vertices[id + 1] = verts7 ;
				else if (edge == 8 ) vertices[id + 1] = verts8 ;
				else if (edge == 9 ) vertices[id + 1] = verts9 ;
				else if (edge == 10) vertices[id + 1] = verts10;
				else if (edge == 11) vertices[id + 1] = verts11;

				edge = triTable[(i + 2) + cubeIndex * 16];
				if (edge == 0) vertices[id + 2] = verts0;
				else if (edge == 1 ) vertices[id + 2] = verts1 ;
				else if (edge == 2 ) vertices[id + 2] = verts2 ;
				else if (edge == 3 ) vertices[id + 2] = verts3 ;
				else if (edge == 4 ) vertices[id + 2] = verts4 ;
				else if (edge == 5 ) vertices[id + 2] = verts5 ;
				else if (edge == 6 ) vertices[id + 2] = verts6 ;
				else if (edge == 7 ) vertices[id + 2] = verts7 ;
				else if (edge == 8 ) vertices[id + 2] = verts8 ;
				else if (edge == 9 ) vertices[id + 2] = verts9 ;
				else if (edge == 10) vertices[id + 2] = verts10;
				else if (edge == 11) vertices[id + 2] = verts11;
			}
		}
		float3 gridPosition(int3 cellPos, float3 originGrid, float dx) {
			float3 cp = new float3(cellPos.x, cellPos.y, cellPos.z);
			return (originGrid + (cp * dx) );
		}

		float3 vertexInterp(float iso, float3 p0, float3 p1, float f0, float f1) {
			float t = (iso - f0) / (f1 - f0);
			return math.lerp(p0, p1, t);
		}
	}
	[BurstCompile]
	struct ComputeNormalsJob : IJobParallelFor
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<float3> normals;

		[ReadOnly] public NativeArray<float3> vertices;
		[ReadOnly] public NativeArray<float> densV;
		[ReadOnly] public float3 oriGrid;
		[ReadOnly] public float dx;
		[ReadOnly] public int3 gridSize;

		void IJobParallelFor.Execute(int index) {

			float3 v = vertices[index];
			int3 ijk = (int3)((v - oriGrid) / dx);

			int id = to1D(ijk, gridSize);
			float field0 = densV[id];
			float field1 = densV[id];
			float field2 = densV[id];
			float field3 = densV[id];
			float field4 = densV[id];
			float field5 = densV[id];

			if (ijk.x < gridSize.x - 1)
				field0 = densV[to1D(ijk + new int3(1, 0, 0), gridSize)];
			if (ijk.x > 0)
				field1 = densV[to1D(ijk - new int3(1, 0, 0), gridSize)];
			if (ijk.y < gridSize.y - 1)
				field2 = densV[to1D(ijk + new int3(0, 1, 0), gridSize)];
			if (ijk.y > 0)
				field3 = densV[to1D(ijk - new int3(0, 1, 0), gridSize)];
			if (ijk.z < gridSize.z - 1)
				field4 = densV[to1D(ijk + new int3(0, 0, 1), gridSize)];
			if (ijk.z > 0)
				field5 = densV[to1D(ijk - new int3(0, 0, 1), gridSize)];

			float3 n;
			n.x = field1 - field0;
			n.y = field3 - field2;
			n.z = field5 - field4;

			normals[index] = n;

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

