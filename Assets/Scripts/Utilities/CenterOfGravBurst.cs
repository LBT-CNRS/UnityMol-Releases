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
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;

using System.Collections.Generic;

namespace UMol {
public class CenterOfGravBurst {

	public static Vector3 computeCOG(List<UnityMolAtom> atoms, ref Vector3 mmin, ref Vector3 mmax) {
		if (atoms.Count <= 0) {
			return Vector3.zero;
		}
		NativeArray<float3> atomPos = new NativeArray<float3>(atoms.Count, Allocator.TempJob);

		for (int i = 0; i < atoms.Count; i++) {
			atomPos[i] = atoms[i].position;
		}

		NativeArray<float3> curRes = new NativeArray<float3>(1, Allocator.TempJob);
		NativeArray<float3> curMin = new NativeArray<float3>(1, Allocator.TempJob);
		NativeArray<float3> curMax = new NativeArray<float3>(1, Allocator.TempJob);

		var cogJob = new COGJob() {
			pos = atomPos,
			res = curRes,
			cmin = curMin,
			cmax = curMax
		};

		var cogJobHandle = cogJob.Schedule();

		cogJobHandle.Complete();
		atomPos.Dispose();

		mmin = cogJob.cmin[0];
		mmax = cogJob.cmax[0];

		curMin.Dispose();
		curMax.Dispose();

		Vector3 result = cogJob.res[0];
		curRes.Dispose();

		return result;
	}

	public static Vector3 computeCOG(Vector3[] pos, ref Vector3 mmin, ref Vector3 mmax) {
		if (pos.Length <= 0) {
			return Vector3.zero;
		}
		NativeArray<float3> atomPos = new NativeArray<float3>(pos.Length, Allocator.TempJob);

		NativeArray<float3> curRes = new NativeArray<float3>(1, Allocator.TempJob);
		NativeArray<float3> curMin = new NativeArray<float3>(1, Allocator.TempJob);
		NativeArray<float3> curMax = new NativeArray<float3>(1, Allocator.TempJob);


		GetNativeArray(atomPos, pos);

		var cogJob = new COGJob() {
			pos = atomPos,
			res = curRes,
			cmin = curMin,
			cmax = curMax
		};

		var cogJobHandle = cogJob.Schedule();
		cogJobHandle.Complete();

		atomPos.Dispose();

		mmin = cogJob.cmin[0];
		mmax = cogJob.cmax[0];

		curMin.Dispose();
		curMax.Dispose();

		Vector3 result = cogJob.res[0];
		curRes.Dispose();

		return result;
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

	[BurstCompile]
	struct COGJob : IJob
	{
		public NativeArray<float3> res;
		public NativeArray<float3> cmin;
		public NativeArray<float3> cmax;
		[ReadOnly] public NativeArray<float3> pos;


		public void Execute()
		{
			res[0] = float3.zero;
			cmin[0] = pos[0];
			cmax[0] = pos[0];
			for (int i = 0; i < pos.Length; i++) {
				res[0] += pos[i];
				cmin[0] = math.min(cmin[0], pos[i]);
				cmax[0] = math.max(cmax[0], pos[i]);
			}
			res[0] /= pos.Length;
		}
	}

}
}