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
#if False

using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using System.Linq;

//13/02/2020 : Doesn't work for now !!!

namespace UMol {
public class NeighborSearchGridBurst {


	///Can contain duplicates
	public void getAtomsWithin(NativeArray<float3> allPoints, NativeArray<float3> qPoints, NativeQueue<int> results, float cutoff) {

		initGridNeighbor(allPoints);

		searchInRadiusMerge(qPoints, results, cutoff);

		clear();
	}

// public class NeighborSearchGridBurst: MonoBehaviour {
	// void Start() {

	// 	UnityMolStructureManager sm = UnityMolMain.getStructureManager();

	// 	API.APIPython.fetch("3eam");
	// 	NativeArray<float3> posAtoms = new NativeArray<float3>(API.APIPython.last().currentModel.Count, Allocator.Persistent);
	// 	int id = 0;
	// 	foreach (UnityMolAtom a in API.APIPython.last().currentModel.allAtoms) {
	// 		posAtoms[id++] = a.position;
	// 	}

	// 	initGridNeighbor(posAtoms);

	// 	var rd = new System.Random();
	// 	// Vector3[] testQ = new Vector3[50000];
	// 	// Vector3[] testQ = new Vector3[1];
	// 	Vector3[] testQ = new Vector3[posAtoms.Length];

	// 	for (int i = 0; i < testQ.Length; i++) {
	// 		// int tmp = rd.Next(0, posAtoms.Length);
	// 		// testQ[i] = posAtoms[tmp];
	// 		testQ[i] = posAtoms[i];
	// 	}

	// 	float rad = 10.0f;
	// 	float start = Time.realtimeSinceStartup;
	// 	// int[] res = searchClosestPoint(testQ);
	// 	List<int>[] res2 = searchInRadius(testQ, rad);

	// 	// for (int i = 0; i < res2.Length; i++) {
	// 	// 	for (int j = 0; j < res2[i].Count; j++) {
	// 	// 		int ida = res2[i][j];
	// 	// 		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
	// 	// 		go.transform.parent = sm.structureToGameObject[API.APIPython.last().uniqueName].transform;
	// 	// 		go.transform.localScale = Vector3.one * 0.5f;
	// 	// 		go.transform.localPosition = posAtoms[ida];
	// 	// 	}
	// 	// 	Debug.Log("Rsuls : "+res2[i].Count);
	// 	// }
	// 	float maxdens = 0.0f;
	// 	int maxV = 0;
	// 	float volume = 4 * Mathf.PI * rad *rad;
	// 	for (int i = 0; i < res2.Length; i++) {
	// 		if(maxdens < res2[i].Count){
	// 			maxV = res2[i].Count;
	// 		}
	// 		maxdens = math.max(maxdens, res2[i].Count / volume);

	// 	}
	// 	Debug.Log("Max density = "+maxdens.ToString("f3") + " / "+maxV);


	// 	// for (int i = 0; i < res.Length; i++) {
	// 	// 	GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
	// 	// 	go.transform.parent = sm.structureToGameObject[API.APIPython.last().uniqueName].transform;
	// 	// 	go.transform.localScale = Vector3.one * 2.0f;
	// 	// 	go.transform.localPosition = posAtoms[i];
	// 	// }


	// 	Debug.Log("Time for search: " + (1000.0f * (Time.realtimeSinceStartup - start)).ToString("f3") + " ms");

	// 	posAtoms.Dispose();
	// 	clear();
	// }

	public bool isInit = false;
	private NativeArray<float3> sortedPos;
	private NativeArray<int2> cellStartEnd;
	private NativeArray<int2> hashIndex;

	public float3 oriGrid;
	public float resoGrid;
	public int3 gridDim;



	void initGridNeighbor(NativeArray<float3> atomPos) {

		if (atomPos.Length < 3) {
			Debug.LogError("Too few points to search");
			return;
		}
		float3 minValue = float3.zero;
		float3 maxValue = float3.zero;

		getMinMaxCoords(atomPos, ref minValue, ref maxValue);

		float3 originGrid = minValue;
		float gridResolutionNeighbor = (3 * 2) * 2 ;//Approx 2 atoms per cell
		float maxDist = math.max(maxValue.x - minValue.x, math.max(maxValue.y - minValue.y, maxValue.z - minValue.z));

		oriGrid = originGrid;
		resoGrid = gridResolutionNeighbor;

		if (maxDist < 0.1f) {
			Debug.LogError("Failed to init grid for neighbor search");
			return;
		}
		int gridNeighborSize = (int)math.ceil(maxDist / gridResolutionNeighbor);
		int3 gridNeighborDim = new int3(gridNeighborSize, gridNeighborSize, gridNeighborSize);
		int nbcellsNeighbor = gridNeighborDim.x * gridNeighborDim.y * gridNeighborDim.z;

		gridDim = gridNeighborDim;

		hashIndex = new NativeArray<int2>(atomPos.Length, Allocator.TempJob);
		sortedPos = new NativeArray<float3>(atomPos.Length, Allocator.Persistent);
		cellStartEnd = new NativeArray<int2>(nbcellsNeighbor, Allocator.Persistent);

		var assignHashJob = new AssignHashJob() {
			oriGrid = originGrid,
			resoGrid = gridResolutionNeighbor,
			gridDim = gridNeighborDim,
			pos = atomPos,
			hashIndex = hashIndex
		};
		var assignHashJobHandle = assignHashJob.Schedule(atomPos.Length, 128);
		assignHashJobHandle.Complete();

		hashIndex.Sort(new int2Comparer());//Compare int2 !

		var memsetCellStartJob = new MemsetCellStartJob() {
			cellStartEnd = cellStartEnd
		};
		var memsetCellStartJobHandle = memsetCellStartJob.Schedule(nbcellsNeighbor, 256);
		memsetCellStartJobHandle.Complete();



		var sortCellJob = new SortCellJob() {
			pos = atomPos,
			hashIndex = hashIndex,
			cellStartEnd = cellStartEnd,
			sortedPos = sortedPos
		};

		var sortCellJobHandle = sortCellJob.Schedule(atomPos.Length, 128);
		sortCellJobHandle.Complete();

		isInit = true;
	}

	public void clear() {
		sortedPos.Dispose();
		cellStartEnd.Dispose();
		hashIndex.Dispose();

	}
	public int[] searchClosestPoint(Vector3[] queryPoints) {

		NativeArray<float3> qPoints = new NativeArray<float3>(queryPoints.Length, Allocator.TempJob);
		NativeArray<int> results = new NativeArray<int>(queryPoints.Length, Allocator.TempJob);
		GetNativeArray(qPoints, queryPoints);




		var closestPointJob = new ClosestPointJob() {
			oriGrid = oriGrid,
			resoGrid = resoGrid,
			gridDim = gridDim,
			queryPos = qPoints,
			sortedPos = sortedPos,
			cellStartEnd = cellStartEnd,
			results = results
		};

		var closestPointJobHandle = closestPointJob.Schedule(qPoints.Length, 16);
		closestPointJobHandle.Complete();

		int[] res = new int[qPoints.Length];
		SetNativeArrayInt(res, results);

		qPoints.Dispose();
		results.Dispose();

		return res;
	}

	public List<int> searchInRadius(NativeArray<float3> atomPos, NativeArray<float3> qPoints, float rad) {
		float start = Time.realtimeSinceStartup;		
		initGridNeighbor(atomPos);
		Debug.Log("Time for init: " + (1000.0f * (Time.realtimeSinceStartup - start)).ToString("f3") + " ms");

		start = Time.realtimeSinceStartup;

        int maxRes = (int)((Mathf.PI * 1.333f * rad * rad * rad) * 0.35f);//Assuming 0.35 atom per Angstrom^3
		NativeArray<int> results = new NativeArray<int>(qPoints.Length * maxRes, Allocator.TempJob);

		var radiusJob = new InRadiusArrayJob() {
			oriGrid = oriGrid,
			resoGrid = resoGrid,
			gridDim = gridDim,
			queryPos = qPoints,
			sortedPos = sortedPos,
			cellStartEnd = cellStartEnd,
			radius = rad,
			radrad = rad * rad,
			maxRes = maxRes,
			results = results,
			hashIndex = hashIndex
		};
		var radiusJobHandle = radiusJob.Schedule(qPoints.Length, 64);
		radiusJobHandle.Complete();

		Debug.Log("Time for grid search: " + (1000.0f * (Time.realtimeSinceStartup - start)).ToString("f3") + " ms");


		List<int> tmpHash = new List<int>(atomPos.Length);

		int tmpcout = 0;
		for (int i = 0; i < results.Length; i++) {
			if (results[i] >= 0) {
				tmpHash.Add(results[i]);
			}
		}
		clear();
		results.Dispose();

		return tmpHash;

	}

	public HashSet<int> searchInRadiusstream(NativeArray<float3> atomPos, NativeArray<float3> qPoints, float rad) {
		initGridNeighbor(atomPos);

        int maxRes = (int)((Mathf.PI * 1.333f * rad * rad * rad) * 0.35f);//Assuming 0.35 atom per Angstrom^3
		// NativeArray<float3> qPoints = new NativeArray<float3>(queryPoints.Length, Allocator.TempJob);
		// NativeMultiHashMap<int, int> results = new NativeMultiHashMap<int, int>(queryPoints.Length * maxRes, Allocator.TempJob);
		NativeStream results = new NativeStream(qPoints.Length, Allocator.TempJob);

		// GetNativeArray(qPoints, queryPoints);


		var radiusJob = new InRadiusStreamJob() {
			oriGrid = oriGrid,
			resoGrid = resoGrid,
			gridDim = gridDim,
			queryPos = qPoints,
			sortedPos = sortedPos,
			cellStartEnd = cellStartEnd,
			radius = rad,
			radrad = rad * rad,
			// results = results.ToConcurrent(),
			results = results.AsWriter(),
			hashIndex = hashIndex
		};

		var radiusJobHandle = radiusJob.Schedule(qPoints.Length, 16);
		JobHandle.ScheduleBatchedJobs();
		radiusJobHandle.Complete();

		HashSet<int> tmpHash = new HashSet<int>();

		var reader = results.AsReader();
		// List<int>[] output = new List<int>[queryPoints.Length];
		for (int i = 0; i < qPoints.Length; i++) {

			int count = reader.BeginForEachIndex(i);

			for (int j = 0; j < count; j++) {
				int item = reader.Read<int>();
				tmpHash.Add(item);
			}
			reader.EndForEachIndex();
			// if (results.TryGetFirstValue(i, out int item, out NativeMultiHashMapIterator<int> it)) {
			// 	do {
			// 		tmpHash.Add(item);
			// 	} while (results.TryGetNextValue(out item, ref it));
			// }

			// output[i] = tmpHash.ToList();
		}


		// qPoints.Dispose();
		results.Dispose();
		clear();

		return tmpHash;

	}


	public void searchInRadiusMerge(NativeArray<float3> qPoints, NativeQueue<int> results, float rad) {

        int maxRes = (int)((Mathf.PI * 1.333f * rad * rad * rad) * 0.35f);//Assuming 0.35 atom per Angstrom^3

		int3 cell = (int3)((qPoints[0] - oriGrid) / resoGrid);

		int neighcellhash = (gridDim.y * gridDim.z * cell.x) + (gridDim.z * cell.y) + cell.z;

		int idStart = cellStartEnd[neighcellhash].x;
		int idStop = cellStartEnd[neighcellhash].y;


		var radiusJob = new InRadiusMergeJob() {
			oriGrid = oriGrid,
			resoGrid = resoGrid,
			gridDim = gridDim,
			queryPos = qPoints,
			sortedPos = sortedPos,
			cellStartEnd = cellStartEnd,
			radius = rad,
			radrad = rad * rad,
			results = results.ToConcurrent(),
			hashIndex = hashIndex
		};

		var radiusJobHandle = radiusJob.Schedule(qPoints.Length, 16);
		JobHandle.ScheduleBatchedJobs();
		radiusJobHandle.Complete();

	}

	void getMinMaxCoords(NativeArray<float3> atomPos, ref float3 minV, ref float3 maxV) {
		NativeArray<float3> tmpmin = new NativeArray<float3>(1, Allocator.TempJob);
		NativeArray<float3> tmpmax = new NativeArray<float3>(1, Allocator.TempJob);
		var mmJob = new getminmaxJob() {
			minVal = tmpmin,
			maxVal = tmpmax,
			pos = atomPos
		};
		var mmJobHandle = mmJob.Schedule();
		mmJobHandle.Complete();
		minV = tmpmin[0];
		maxV = tmpmax[0];
		tmpmin.Dispose();
		tmpmax.Dispose();
	}


	[BurstCompile]
	struct getminmaxJob : IJob {
		public NativeArray<float3> minVal;
		public NativeArray<float3> maxVal;
		[ReadOnly] public NativeArray<float3> pos;

		void IJob.Execute() {
			minVal[0] = pos[0];
			maxVal[0] = pos[0];
			float x, y, z;
			for (int i = 1; i < pos.Length; i++) {
				x = math.min(minVal[0].x, pos[i].x);
				y = math.min(minVal[0].y, pos[i].y);
				z = math.min(minVal[0].z, pos[i].z);
				minVal[0] = new float3(x, y, z);
				x = math.max(maxVal[0].x, pos[i].x);
				y = math.max(maxVal[0].y, pos[i].y);
				z = math.max(maxVal[0].z, pos[i].z);
				maxVal[0] = new float3(x, y, z);
			}
		}
	}


	[BurstCompile]
	struct AssignHashJob : IJobParallelFor {
		[ReadOnly] public float3 oriGrid;
		[ReadOnly] public float resoGrid;
		[ReadOnly] public int3 gridDim;
		[ReadOnly] public NativeArray<float3> pos;
		public NativeArray<int2> hashIndex;

		void IJobParallelFor.Execute(int index)
		{
			float3 p = pos[index];

			int3 cell = spaceToGrid(p, oriGrid, resoGrid);
			int hash = flatten3DTo1D(cell, gridDim);

			int2 v;
			v.x = hash;
			v.y = index;
			hashIndex[index] = v;
		}
		int3 spaceToGrid(float3 pos3D, float3 originGrid, float dx) {
			return (int3)((pos3D - originGrid) / dx);
		}
		int flatten3DTo1D(int3 id3d, int3 gridDim) {
			return (gridDim.y * gridDim.z * id3d.x) + (gridDim.z * id3d.y) + id3d.z;
		}
	}

	[BurstCompile]
	struct MemsetCellStartJob : IJobParallelFor {
		public NativeArray<int2> cellStartEnd;

		void IJobParallelFor.Execute(int index)
		{
			int2 v;
			v.x = int.MaxValue - 1;
			v.y = int.MaxValue - 1;
			cellStartEnd[index] = v ;
		}
	}

	[BurstCompile]
	struct SortCellJob : IJobParallelFor {

		[ReadOnly] public NativeArray<float3> pos;
		[ReadOnly] public NativeArray<int2> hashIndex;

		[NativeDisableParallelForRestriction]
		public NativeArray<int2> cellStartEnd;

		public NativeArray<float3> sortedPos;


		void IJobParallelFor.Execute(int index)
		{
			int hash = hashIndex[index].x;
			int id = hashIndex[index].y;
			int2 newV;

			int hashm1;
			if (index != 0)
				hashm1 = hashIndex[index - 1].x;
			else
				hashm1 = hash;


			if (index == 0 || hash != hashm1) {
				newV.x = index;
				newV.y = cellStartEnd[hash].y;

				cellStartEnd[hash] = newV; // set start

				if (index > 0) {
					newV.x = cellStartEnd[hashm1].x;
					newV.y = index;
					cellStartEnd[hashm1] = newV; // set end
				}
			}

			if (index == pos.Length - 1) {
				newV.x = cellStartEnd[hash].x;
				newV.y = index + 1;

				cellStartEnd[hash] = newV; // set end
			}

			// Reorder atoms according to sorted indices

			sortedPos[index] = pos[id];
		}
	}

	[BurstCompile]
	struct ClosestPointJob : IJobParallelFor {
		[ReadOnly] public float3 oriGrid;
		[ReadOnly] public float resoGrid;
		[ReadOnly] public int3 gridDim;
		[ReadOnly] public NativeArray<float3> queryPos;
		[ReadOnly] public NativeArray<int2> cellStartEnd;
		[ReadOnly] public NativeArray<float3> sortedPos;
		public NativeArray<int> results;

		void IJobParallelFor.Execute(int index) {
			results[index] = -1;
			float3 p = queryPos[index];

			int3 cell = spaceToGrid(p, oriGrid, resoGrid);

			float minD = 9999.0f;
			int3 curGridId;
			int minRes = -1;

			for (int x = -1; x <= 1; x++) {
				curGridId.x = cell.x + x;
				if (curGridId.x >= 0 && curGridId.x < gridDim.x) {
					for (int y = -1; y <= 1; y++) {
						curGridId.y = cell.y + y;
						if (curGridId.y >= 0 && curGridId.y < gridDim.y) {
							for (int z = -1; z <= 1; z++) {
								curGridId.z = cell.z + z;
								if (curGridId.z >= 0 && curGridId.z < gridDim.z) {

									int neighcellhash = flatten3DTo1D(curGridId, gridDim);
									int idStart = cellStartEnd[neighcellhash].x;
									int idStop = cellStartEnd[neighcellhash].y;

									if (idStart < int.MaxValue - 1) {
										for (int id = idStart; id < idStop; id++) {

											float3 posA = sortedPos[id];
											float d = sqr_distance(posA, p);
											if (d < minD) {
												minRes = id;
												minD = d;
											}
										}
									}
								}
							}
						}
					}
				}
			}
			if (minRes != -1) {
				results[index] = minRes;
			}
			else {
				//Compute all the distances ! = SLOW
				for (int id = 0; id < sortedPos.Length; id++) {

					float3 posA = sortedPos[id];
					float d = sqr_distance(p, posA);
					if (d < minD) {
						minRes = id;
						minD = d;
					}
				}
				results[index] = minRes;
			}
		}
		int3 spaceToGrid(float3 pos3D, float3 originGrid, float dx) {
			return (int3)((pos3D - originGrid) / dx);
		}
		int flatten3DTo1D(int3 id3d, int3 gridDim) {
			return (gridDim.y * gridDim.z * id3d.x) + (gridDim.z * id3d.y) + id3d.z;
		}

		float sqr_distance(float3 p1, float3 p2) {
			float x = (p1.x - p2.x) * (p1.x - p2.x);
			float y = (p1.y - p2.y) * (p1.y - p2.y);
			float z = (p1.z - p2.z) * (p1.z - p2.z);

			return x + y + z;
		}
	}

	[BurstCompile]
	struct InRadiusStreamJob : IJobParallelFor {
		[ReadOnly] public float3 oriGrid;
		[ReadOnly] public float resoGrid;
		[ReadOnly] public int3 gridDim;
		[ReadOnly] public NativeArray<float3> queryPos;
		[ReadOnly] public NativeArray<int2> cellStartEnd;
		[ReadOnly] public NativeArray<float3> sortedPos;
		[ReadOnly] public float radius;
		[ReadOnly] public float radrad;
		[ReadOnly] public NativeArray<int2> hashIndex;


		// public NativeMultiHashMap<int, int>.Concurrent results;
		public NativeStream.Writer results;

		void IJobParallelFor.Execute(int index) {

			results.BeginForEachIndex(index);

			float3 p = queryPos[index];
			int3 cell = spaceToGrid(p, oriGrid, resoGrid);

			int3 curGridId;
			int range = math.max(1, (int)(radius / resoGrid));

			for (int x = -range; x <= range; x++) {
				curGridId.x = cell.x + x;
				if (curGridId.x >= 0 && curGridId.x < gridDim.x) {
					for (int y = -range; y <= range; y++) {
						curGridId.y = cell.y + y;
						if (curGridId.y >= 0 && curGridId.y < gridDim.y) {
							for (int z = -range; z <= range; z++) {
								curGridId.z = cell.z + z;
								if (curGridId.z >= 0 && curGridId.z < gridDim.z) {

									int neighcellhash = flatten3DTo1D(curGridId, gridDim);
									int idStart = cellStartEnd[neighcellhash].x;
									int idStop = cellStartEnd[neighcellhash].y;

									if (idStart < int.MaxValue - 1) {//Not empty cell
										for (int id = idStart; id < idStop; id++) {

											if (id > 0 && id < sortedPos.Length) {
												float3 posA = sortedPos[id];
												float d = sqr_distance(posA, p);
												// Debug.Log(curGridId + " / "+id+" / "+math.sqrt(d));
												if (d <= radrad) {
													results.Write(hashIndex[id].y);
													// results.Add(index, hashIndex[id].y);
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			results.EndForEachIndex();

		}
		int3 spaceToGrid(float3 pos3D, float3 originGrid, float dx) {
			return (int3)((pos3D - originGrid) / dx);
		}
		int flatten3DTo1D(int3 id3d, int3 gridDim) {
			return (gridDim.y * gridDim.z * id3d.x) + (gridDim.z * id3d.y) + id3d.z;
		}
		float sqr_distance(float3 p1, float3 p2) {
			float x = (p1.x - p2.x) * (p1.x - p2.x);
			float y = (p1.y - p2.y) * (p1.y - p2.y);
			float z = (p1.z - p2.z) * (p1.z - p2.z);

			return x + y + z;
		}
	}

	[BurstCompile]
	struct InRadiusArrayJob : IJobParallelFor {
		[ReadOnly] public float3 oriGrid;
		[ReadOnly] public float resoGrid;
		[ReadOnly] public int3 gridDim;
		[ReadOnly] public NativeArray<float3> queryPos;
		[ReadOnly] public NativeArray<int2> cellStartEnd;
		[ReadOnly] public NativeArray<float3> sortedPos;
		[ReadOnly] public float radius;
		[ReadOnly] public float radrad;
		[ReadOnly] public NativeArray<int2> hashIndex;
		[ReadOnly] public int maxRes;

		[NativeDisableParallelForRestriction]
		public NativeArray<int> results;

		void IJobParallelFor.Execute(int index) {

			int offset = index * maxRes;
			for (int i = 0; i < maxRes; i++) {
				results[offset + i] = -1;
			}

			float3 p = queryPos[index];
			int3 cell = spaceToGrid(p, oriGrid, resoGrid);

			int3 curGridId;
			int range = math.max(1, (int)(radius / resoGrid));

			int nbRes = 0;
			for (int x = -range; x <= range; x++) {
				curGridId.x = cell.x + x;
				if (curGridId.x >= 0 && curGridId.x < gridDim.x) {
					for (int y = -range; y <= range; y++) {
						curGridId.y = cell.y + y;
						if (curGridId.y >= 0 && curGridId.y < gridDim.y) {
							for (int z = -range; z <= range; z++) {
								curGridId.z = cell.z + z;
								if (curGridId.z >= 0 && curGridId.z < gridDim.z) {

									int neighcellhash = flatten3DTo1D(curGridId, gridDim);
									int idStart = cellStartEnd[neighcellhash].x;
									int idStop = cellStartEnd[neighcellhash].y;

									if (idStart < int.MaxValue - 1) {//Not empty cell
										for (int id = idStart; id < idStop; id++) {

											if (id > 0 && id < sortedPos.Length) {
												float3 posA = sortedPos[id];
												float d = sqr_distance(posA, p);
												// Debug.Log(curGridId + " / "+id+" / "+math.sqrt(d));
												if (d <= radrad) {
													results[offset + nbRes] = hashIndex[id].y;
													nbRes++;
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
		int3 spaceToGrid(float3 pos3D, float3 originGrid, float dx) {
			return (int3)((pos3D - originGrid) / dx);
		}
		int flatten3DTo1D(int3 id3d, int3 gridDim) {
			return (gridDim.y * gridDim.z * id3d.x) + (gridDim.z * id3d.y) + id3d.z;
		}
		float sqr_distance(float3 p1, float3 p2) {
			float x = (p1.x - p2.x) * (p1.x - p2.x);
			float y = (p1.y - p2.y) * (p1.y - p2.y);
			float z = (p1.z - p2.z) * (p1.z - p2.z);

			return x + y + z;
		}
	}

	[BurstCompile]
	struct InRadiusMergeJob : IJobParallelFor {
		[ReadOnly] public float3 oriGrid;
		[ReadOnly] public float resoGrid;
		[ReadOnly] public int3 gridDim;
		[ReadOnly] public NativeArray<float3> queryPos;
		[ReadOnly] public NativeArray<int2> cellStartEnd;
		[ReadOnly] public NativeArray<float3> sortedPos;
		[ReadOnly] public float radius;
		[ReadOnly] public float radrad;
		[ReadOnly] public NativeArray<int2> hashIndex;


		public NativeQueue<int>.Concurrent results;

		void IJobParallelFor.Execute(int index) {

			float3 p = queryPos[index];
			int3 cell = spaceToGrid(p, oriGrid, resoGrid);

			int3 curGridId;
			int range = math.max(1, (int)(radius / resoGrid));

			for (int x = -range; x <= range; x++) {
				curGridId.x = cell.x + x;
				if (curGridId.x >= 0 && curGridId.x < gridDim.x) {
					for (int y = -range; y <= range; y++) {
						curGridId.y = cell.y + y;
						if (curGridId.y >= 0 && curGridId.y < gridDim.y) {
							for (int z = -range; z <= range; z++) {
								curGridId.z = cell.z + z;
								if (curGridId.z >= 0 && curGridId.z < gridDim.z) {

									int neighcellhash = flatten3DTo1D(curGridId, gridDim);
									int idStart = cellStartEnd[neighcellhash].x;
									int idStop = cellStartEnd[neighcellhash].y;

									if (idStart < int.MaxValue - 1) {//Not empty cell
										for (int id = idStart; id < idStop; id++) {

											float3 posA = sortedPos[id];
											float d = sqr_distance(posA, p);
											// Debug.Log(curGridId + " / "+id+" / "+math.sqrt(d));
											if (d <= radrad) {
												results.Enqueue(hashIndex[id].y);
											}
										}
									}
								}
							}
						}
					}
				}
			}

		}
		int3 spaceToGrid(float3 pos3D, float3 originGrid, float dx) {
			return (int3)((pos3D - originGrid) / dx);
		}
		int flatten3DTo1D(int3 id3d, int3 gridDim) {
			return (gridDim.y * gridDim.z * id3d.x) + (gridDim.z * id3d.y) + id3d.z;
		}
		float sqr_distance(float3 p1, float3 p2) {
			float x = (p1.x - p2.x) * (p1.x - p2.x);
			float y = (p1.y - p2.y) * (p1.y - p2.y);
			float z = (p1.z - p2.z) * (p1.z - p2.z);

			return x + y + z;
		}
	}




	public struct int2Comparer : IComparer<int2> {
		public int Compare(int2 lhs, int2 rhs) {
			return lhs.x.CompareTo(rhs.x);
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
	unsafe static void SetNativeArrayInt(int[] arr, NativeArray<int> nativ)
	{
		// pin the target array and get a pointer to it
		fixed (void* arrPointer = arr)
		{
			// memcopy the native array over the top
			UnsafeUtility.MemCpy(arrPointer, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(nativ), arr.Length * (long) UnsafeUtility.SizeOf<int>());
		}
	}
}
}
#endif
