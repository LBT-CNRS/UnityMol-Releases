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

/// <summary>
/// Class to handle the smoothing of a trajectory
/// Use Burst option to compute the smoothing.
/// </summary>
public class TrajectorySmoother {

    /// <summary>
    /// Burst structure for  smoothing
    /// </summary>
    [BurstCompile]
    private struct SmoothedPositions : IJobParallelFor
    {
        public NativeArray<float3> InterpolatedPositions;
        [ReadOnly] public NativeArray<float3> PosT1;
        [ReadOnly] public NativeArray<float3> PosT2;
        [ReadOnly] public float Step;

        void IJobParallelFor.Execute(int index)
        {
            InterpolatedPositions[index] = math.lerp(PosT1[index], PosT2[index], Step);
        }
    }

    /// <summary>
    /// Burst structure for cubic smoothing
    /// </summary>
    [BurstCompile]
    private struct SmoothedPositionsCubic : IJobParallelFor
    {
        public NativeArray<float3> InterpolatedPositions;
        [ReadOnly] public NativeArray<float3> PosT1;//n-1
        [ReadOnly] public NativeArray<float3> PosT2;//n
        [ReadOnly] public NativeArray<float3> PosT3;//n+1
        [ReadOnly] public NativeArray<float3> PosT4;//n+2
        [ReadOnly] public float Step;

        void IJobParallelFor.Execute(int index)
        {
            float3 p0 = PosT1[index];
            float3 p1 = PosT2[index];
            float3 p2 = PosT3[index];
            float3 p3 = PosT4[index];

            float3 v0 = (p2 - p0) * 0.5f;
            float3 v1 = (p3 - p1) * 0.5f;

            float s2 = Step * Step;
            float s3 = Step * s2;

            InterpolatedPositions[index] = (2.0f * p1 - 2.0f * p2 + v0 + v1) * s3 + (-3.0f * p1 + 3.0f * p2 - 2.0f * v0 - v1) * s2 + v0 * Step + p1;
        }
    }


    //List of arrays needed for the smoothing
	private NativeArray<float3> positionsTm1;
    private NativeArray<float3> positionsT1;
    private NativeArray<float3> positionsT2;
    private NativeArray<float3> positionsT3;

    /// <summary>
    /// Coordinates of the atoms after smoothing.
    /// </summary>
    private NativeArray<float3> result;

    /// <summary>
    /// Cubic interpolation or not?
    /// </summary>
    private bool cubic;

    /// <summary>
    /// Initialize the object
    /// <remarks>Can not use a classic Constructor due to the NativeArray and their disposal.</remarks>
    /// </summary>
    /// <param name="postm1"></param>
    /// <param name="post1"></param>
    /// <param name="post2"></param>
    /// <param name="post3"></param>
	public void Init(Vector3[] postm1, Vector3[] post1, Vector3[] post2, Vector3[] post3) {
		cubic = false;
		if (postm1 != null && post3 != null) {
			cubic = true;
		}

		if (positionsT1 == null || positionsT1.Length == 0) {
			positionsT1 = new NativeArray<float3>(post1.Length, Allocator.Persistent);
			positionsT2 = new NativeArray<float3>(post2.Length, Allocator.Persistent);
			result = new NativeArray<float3>(post1.Length, Allocator.Persistent);
		}
		if (cubic) {
			if (positionsTm1 == null || positionsTm1.Length == 0) {
				positionsTm1 = new NativeArray<float3>(postm1.Length, Allocator.Persistent);
				positionsT3 = new NativeArray<float3>(post3.Length, Allocator.Persistent);
			}
		}


		if (post1.Length == post2.Length && post1.Length == positionsT1.Length) {
			getNativeArray(positionsT1, post1);
			getNativeArray(positionsT2, post2);
		}
		if (cubic) {
			getNativeArray(positionsTm1, postm1);
			getNativeArray(positionsT3, post3);
		}
	}

    /// <summary>
    /// Clear the arrays.
    /// </summary>
	public void Clear() {
		if (positionsT1.IsCreated) {
			positionsT1.Dispose();
			positionsT2.Dispose();
		}
		if (positionsTm1.IsCreated) {
			positionsTm1.Dispose();
			positionsT3.Dispose();
		}
		if (result.IsCreated) {
			result.Dispose();
		}
	}

    /// <summary>
    /// Compute the smoothing over the number of frames
    /// </summary>
    /// <param name="outPos">Array of smoothed positions</param>
    /// <param name="t">Array of smoothed positions</param>
	public void Process(Vector3[] outPos, float t) {

		if (positionsT1.Length == positionsT2.Length && outPos.Length == positionsT1.Length) {
			if (cubic) {
				SmoothedPositionsCubic smoothJob = new() {
					InterpolatedPositions = result,
					PosT1 = positionsTm1,
					PosT2 = positionsT1,
					PosT3 = positionsT2,
					PosT4 = positionsT3,
					Step = t
				};

				JobHandle smoothJobHandle = smoothJob.Schedule(positionsT1.Length, 64);
				smoothJobHandle.Complete();
			}
			else {
				SmoothedPositions smoothJob = new() {
					InterpolatedPositions = result,
					PosT1 = positionsT1,
					PosT2 = positionsT2,
					Step = t
				};

				JobHandle smoothJobHandle = smoothJob.Schedule(positionsT1.Length, 64);
				smoothJobHandle.Complete();
			}

			setNativeArray(outPos, result);
		}
		else {
			Debug.LogError("Wrong sizes, did you call init ?");
		}

	}

	//From https://gist.github.com/LotteMakesStuff/c2f9b764b15f74d14c00ceb4214356b4
    private static unsafe void getNativeArray(NativeArray<float3> posNativ, Vector3[] posArray)
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
    private static unsafe void setNativeArray(Vector3[] posArray, NativeArray<float3> posNativ)
	{
		// pin the target array and get a pointer to it
		fixed (void* posArrayPointer = posArray)
		{
			// memcopy the native array over the top
			UnsafeUtility.MemCpy(posArrayPointer, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(posNativ), posArray.Length * (long) UnsafeUtility.SizeOf<float3>());
		}
	}

}
}
