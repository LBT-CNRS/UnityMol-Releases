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
using System.Collections.Generic;

namespace UMol {
public class FieldLinesBurst {

	public static List<Vector3>[] computeFL(Vector3[] grad, Vector3[] dx, Vector3 origin,
	                                        int3 gridSize, int nbIter, float gradThreshold,
	                                        float xl, float yl, float zl,
	                                        float minLength = 10.0f, float maxLength = 50.0f) {

		NativeArray<float3> gradient = new NativeArray<float3>(grad.Length, Allocator.TempJob);
		GetNativeArray(gradient, grad);

		int3 gSize = new int3(gridSize.x, gridSize.y, gridSize.z);
		NativeList<int> seeds = getSeeds(gradient, gSize, gradThreshold);

		if (seeds.Length == 0) {
			Debug.LogWarning("No seeds");
			gradient.Dispose();
			seeds.Dispose();
			return null;
		}

		List<Vector3>[] fieldlines = computeFLBurst(gradient, nbIter, dx, seeds, gSize, minLength, maxLength, origin,
		                             xl, yl, zl);


		gradient.Dispose();
		seeds.Dispose();
		return fieldlines;
	}

	static NativeList<int> getSeeds(NativeArray<float3> grad, int3 gridSize, float gradThreshold) {

		float minGrad = (gradThreshold * 0.5f);
		float maxGrad = (gradThreshold * 1.5f);
		float minGrad2 = minGrad * minGrad;
		float maxGrad2 = maxGrad * maxGrad;

		NativeList<int> seeds = new NativeList<int>(Allocator.TempJob);
		NativeArray<bool> keep = new NativeArray<bool>(grad.Length, Allocator.TempJob);

		var seedJob = new SeedJob() {
			gradient = grad,
			minGrad2 = minGrad2,
			maxGrad2 = maxGrad2,
			keep = keep
		};

		var seedJobHandle = seedJob.Schedule(grad.Length, 64);
		seedJobHandle.Complete();

		for (int i = 0; i < grad.Length; i++) {
			if (keep[i]) {
				seeds.Add(i);
			}
		}

		keep.Dispose();

		return seeds;
	}

	static List<Vector3>[] computeFLBurst(NativeArray<float3> grad, int nbIter, Vector3[] dx, NativeList<int> seeds,
	                                      int3 gridSize, float minLength, float maxLength, float3 origin,
	                                      float xl, float yl, float zl) {

		NativeArray<float3> fl = new NativeArray<float3>((nbIter + 1) * seeds.Length, Allocator.TempJob);
		NativeArray<float3> natDX = new NativeArray<float3>(3, Allocator.TempJob);

		natDX[0] = dx[0];
		natDX[1] = dx[1];
		natDX[2] = dx[2];

		float mincelllen = math.min(math.min(xl, yl), zl);
		float delta = 0.25f * mincelllen;

		float minGradMag = 0.0001f;
		float maxGradMag = 5.0f;


		var flJob = new fieldlineJob() {
			nbIte = nbIter,
			seeds = seeds,
			gradient = grad,
			dim = gridSize,
			minGrad = minGradMag,
			maxGrad = maxGradMag,
			minLength = minLength,
			maxLength = maxLength,
			delta = delta,
			dx = natDX,
			ori = origin,
			fieldlines = fl
		};

		var flJobHandle = flJob.Schedule(seeds.Length, 64);
		flJobHandle.Complete();


		List<Vector3>[] res = new List<Vector3>[seeds.Length];
		for (int i = 0; i < seeds.Length; i++) {
			res[i] = new List<Vector3>(nbIter + 1);
			Vector3 v = fl[(nbIter + 1) * i];
			if (Mathf.Approximately(v.x, -1) &&
			        Mathf.Approximately(v.y, -1) &&
			        Mathf.Approximately(v.z, -1) ) {
				continue;
			}
			for (int j = 0; j < nbIter + 1; j++) {
				v = fl[(nbIter + 1) * i + j];
				if (Mathf.Approximately(v.x, -1) &&
				        Mathf.Approximately(v.y, -1) &&
				        Mathf.Approximately(v.z, -1) ) {
					break;
				}
				res[i].Add(v);
			}
		}


		fl.Dispose();
		natDX.Dispose();

		return res;
	}



	[BurstCompile]
	struct SeedJob : IJobParallelFor
	{
		[ReadOnly] public NativeArray<float3> gradient;
		[ReadOnly] public float minGrad2;
		[ReadOnly] public float maxGrad2;
		public NativeArray<bool> keep;

		void IJobParallelFor.Execute(int index)
		{
			int idGrad = index;
			float sqrMag = math.dot(gradient[idGrad], gradient[idGrad]);
			if (sqrMag >= minGrad2 && sqrMag <= maxGrad2) {
				keep[index] = true;
			}
			else {
				keep[index] = false;
			}
		}
	}

	[BurstCompile]
	struct fieldlineJob : IJobParallelFor
	{
		[ReadOnly] public NativeArray<float3> gradient;
		[ReadOnly] public NativeList<int> seeds;
		[ReadOnly] public int nbIte;
		[ReadOnly] public int3 dim;
		[ReadOnly] public float minGrad;
		[ReadOnly] public float maxGrad;
		[ReadOnly] public float minLength;
		[ReadOnly] public float maxLength;
		[ReadOnly] public float delta;
		[ReadOnly] public NativeArray<float3> dx;
		[ReadOnly] public float3 ori;

		[NativeDisableParallelForRestriction]
		public NativeArray<float3> fieldlines;

		static int3 unflatten1DTo3D(int index, int3 dim) {
			int x = index / (dim.y * dim.z);
			int y = (index - x * dim.y * dim.z) / dim.z;
			int z = index - x * dim.y * dim.z - y * dim.z;

			return new int3(x, y, z);
		}

		// grid to space
		static float3 g2s(int3 ijk, NativeArray<float3> dx, float3 origin) {

			float3 res = origin;

			res += dx[0] * ijk.x;
			res += dx[1] * ijk.y;
			res += dx[2] * ijk.z;

			return res;
		}

		// #space to grid
		static int3 s2g(float3 pos3d, NativeArray<float3> dx, float3 origin) {
			float3 tmp = pos3d - origin;
			float4x4 m = new float4x4(
			    dx[0].x, dx[1].x, dx[2].x, 0.0f,
			    dx[0].y, dx[1].y, dx[2].y, 0.0f,
			    dx[0].z, dx[1].z, dx[2].z, 0.0f,
			    0.0f, 0.0f, 0.0f, 1.0f);

			float4x4 invm = math.inverse(m);

			int3 res = (int3)math.transform(invm, tmp);

			return res;
		}
		static bool isInBox(float3 pos, NativeArray<float3> dx, float3 origin, int3 dim) {
			int3 ijk = s2g(pos, dx, origin);
			if (ijk.x < 0 || ijk.x >= dim.x)
				return false;
			if (ijk.y < 0 || ijk.y >= dim.y)
				return false;
			if (ijk.z < 0 || ijk.z >= dim.z)
				return false;
			return true;
		}

		void IJobParallelFor.Execute(int index)
		{
			for (int i = 0; i < (nbIte + 1); i++) {
				fieldlines[(nbIte + 1) * index + i] = new float3(-1, -1, -1);
			}
			float lengthPerLine = 0.0f;
			int idStart = seeds[index];
			int3 index3 = unflatten1DTo3D(idStart, dim);
			fieldlines[(nbIte + 1) * index] = g2s(index3, dx, ori);
			for (int it = 1; it < nbIte + 1; it++) {

				float3 prevP = fieldlines[(nbIte + 1) * index + (it - 1)];
				int3 ijk = s2g(prevP, dx, ori);
				int idGrad = ((dim.z * dim.y * ijk.x) + (dim.z * ijk.y) + ijk.z);
				float3 gvalue = gradient[idGrad];
				float norm = math.length(gvalue);

				if (norm < minGrad || norm > maxGrad) {
					break;
				}

				float3 newP = prevP + (gvalue * delta / norm);

				lengthPerLine += delta;

				bool3 newPNan = math.isnan(newP);
				if (isInBox(newP, dx, ori, dim) && !newPNan.x && !newPNan.y && !newPNan.z) {
					fieldlines[(nbIte + 1) * index + it] = newP;
				}
				else {
					break;
				}
				if (lengthPerLine > maxLength) {
					break;
				}
			}
			if (lengthPerLine < minLength || lengthPerLine > maxLength) {
				fieldlines[(nbIte + 1) * index] = new float3(-1, -1, -1);
			}
		}
	}


	//From https://gist.github.com/LotteMakesStuff/c2f9b764b15f74d14c00ceb4214356b4
	static unsafe void GetNativeArray(NativeArray<float3> posNativ, Vector3[] posArray)
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

}
}