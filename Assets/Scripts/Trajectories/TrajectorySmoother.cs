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

namespace UMol {
public class TrajectorySmoother {


	NativeArray<float3> positionsT1;
	NativeArray<float3> positionsT2;
	NativeArray<float3> result;

	public void init(Vector3[] post1, Vector3[] post2) {
		if (positionsT1 == null || positionsT1.Length == 0) {
			positionsT1 = new NativeArray<float3>(post1.Length, Allocator.Persistent);
			positionsT2 = new NativeArray<float3>(post2.Length, Allocator.Persistent);
			result = new NativeArray<float3>(post1.Length, Allocator.Persistent);
		}

		if (post1.Length == post2.Length && post1.Length == positionsT1.Length) {
			// positionsT1.CopyFrom(post1);
			// positionsT2.CopyFrom(post2);
			GetNativeArray(positionsT1, post1);
			GetNativeArray(positionsT2, post2);
		}
	}

	public void clear() {
		if(positionsT1.IsCreated){
			positionsT1.Dispose();
			positionsT2.Dispose();
		}
		if(result.IsCreated){
			result.Dispose();
		}
	}

	public void process(Vector3[] outPos, float t) {
		if(positionsT1.Length == positionsT2.Length && outPos.Length == positionsT1.Length){
			var smoothJob = new SmoothedPositions() {
				interpolatedPositions = result,
				posT1 = positionsT1,
				posT2 = positionsT2,
				step = t
			};

			var smoothJobHandle = smoothJob.Schedule(positionsT1.Length, 64);
			smoothJobHandle.Complete();

			SetNativeArray(outPos, result);
		}
		else{
			Debug.LogError("Wrong sizes, did you call init ?");
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
	unsafe void SetNativeArray(Vector3[] posArray, NativeArray<float3> posNativ)
	{
		// pin the target array and get a pointer to it
		fixed (void* posArrayPointer = posArray)
		{
			// memcopy the native array over the top
			UnsafeUtility.MemCpy(posArrayPointer, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(posNativ), posArray.Length * (long) UnsafeUtility.SizeOf<float3>());
		}
	}

	[BurstCompile]
	struct SmoothedPositions : IJobParallelFor
	{
		public NativeArray<float3> interpolatedPositions;
		[ReadOnly] public NativeArray<float3> posT1;
		[ReadOnly] public NativeArray<float3> posT2;
		[ReadOnly] public float step;

		void IJobParallelFor.Execute(int index)
		{
			interpolatedPositions[index] = math.lerp(posT1[index], posT2[index], step);
		}
	}

}
}