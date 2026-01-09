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
/// Class to handle an averaging of a trajectory over X frames.
/// Use Burst option to compute the average.
/// </summary>
public class TrajectoryMean {

    /// <summary>
    /// Structure to handle the averaging of coordinates
    /// </summary>
    [BurstCompile]
    private struct MeanPositions : IJobParallelFor
    {
        public NativeArray<float3> MeanPos;
        [ReadOnly] public NativeArray<float3> Pos;
        [ReadOnly] public int Window;
        [ReadOnly] public int Natoms;

        void IJobParallelFor.Execute(int index)
        {
            MeanPos[index] = 0.0f;
            for (int i = 0; i < Window; i++) {
                MeanPos[index] += Pos[i * Natoms + index];
            }
            MeanPos[index] /= Window;
        }
    }


    /// <summary>
    /// Number of atoms in the trajectory
    /// </summary>
    private int nbAtoms;

    /// <summary>
    /// Size of the window to average the trajectory, i.e. the number of frames
    /// </summary>
    private int windowSize;

    /// <summary>
    /// Input coordinates of the atoms
    /// </summary>
    private NativeArray<float3> positionsInput;

    /// <summary>
    /// Coordinates of the atoms after averaging.
    /// </summary>
     private NativeArray<float3> result;

    /// <summary>
    /// Initialize the object
    /// <remarks>Can not use a classic Constructor due to the NativeArray and their disposal.</remarks>
    /// </summary>
    /// <param name="positions">array of atom positions</param>
    /// <param name="nAtoms">number of atoms</param>
    /// <param name="window">number of frames to average</param>
    public void Init(Vector3[] positions, int NAtoms, int window) {
        nbAtoms = NAtoms;

        if (positionsInput == null || positionsInput.Length == 0 || window != windowSize) {
            if (positionsInput.IsCreated) {
                positionsInput.Dispose();
            }
            positionsInput = new NativeArray<float3>(window * nbAtoms, Allocator.Persistent);
        }
        windowSize = window;
        if (result == null || result.Length != nbAtoms) {
            if (result.IsCreated) {
                result.Dispose();
            }
            result = new NativeArray<float3>(nbAtoms, Allocator.Persistent);
        }

        getNativeArray(positionsInput, positions);
    }



    /// <summary>
    /// Clear the arrays.
    /// </summary>
    public void Clear() {
        if (positionsInput.IsCreated) {
            positionsInput.Dispose();
        }
        if (result.IsCreated) {
            result.Dispose();
        }
    }


    /// <summary>
    /// Compute the averaging over the number of frames
    /// </summary>
    /// <param name="outPos">Array of averaged positions</param>
    public void Process(Vector3[] outPos) {

        if (outPos.Length == nbAtoms && windowSize > 0) {

            MeanPositions meanJob = new() {
                MeanPos = result,
                Pos = positionsInput,
                Natoms = nbAtoms,
                Window = windowSize
            };

            JobHandle meanJobHandle = meanJob.Schedule(nbAtoms, 256);
            meanJobHandle.Complete();
            setNativeArray(outPos, result);
        }
        else {
            Debug.LogError("Wrong sizes.");
        }

    }

    /// <summary>
    /// Fast way to copy a c# array into a nativearray
    /// From https://gist.github.com/LotteMakesStuff/c2f9b764b15f74d14c00ceb4214356b4
    /// </summary>
    /// <param name="posNativ"></param>
    /// <param name="posArray"></param>
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

    /// <summary>
    /// Fast way to copy a nativearray into a c# array
    /// From https://gist.github.com/LotteMakesStuff/c2f9b764b15f74d14c00ceb4214356b4
    /// </summary>
    /// <param name="posArray"></param>
    /// <param name="posNativ"></param>
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
