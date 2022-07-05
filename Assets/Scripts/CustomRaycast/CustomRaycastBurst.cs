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



namespace UMol {
public class CustomRaycastBurst {

	NativeArray<float3> atomPos;
	NativeArray<float> atomRad;
	NativeArray<bool> results;

	public bool DEBUG = false;
	private static List<GameObject> debugGos = new List<GameObject>();

	public bool needsFullUpdate = true;
	public bool needsUpdatePos = false;
	public bool needsUpdateRadii = false;

	public UnityMolAtom[] allAtoms;
	public Transform[] allTransforms;

	void updateAllLists() {
		UnityMolRepresentationManager repM = UnityMolMain.getRepresentationManager();

		int N = repM.atomsWithActiveRep.Count;

		if (atomPos.IsCreated) {
			atomPos.Dispose();
			atomRad.Dispose();
			results.Dispose();
		}

		atomPos = new NativeArray<float3>(N, Allocator.Persistent);
		atomRad = new NativeArray<float>(N, Allocator.Persistent);
		results = new NativeArray<bool>(N, Allocator.Persistent);

		allTransforms = new Transform[N];
		allAtoms = new UnityMolAtom[N];

		int id = 0;
		foreach (UnityMolAtom a in repM.atomsWithActiveRep) {
			UnityMolStructure s = a.residue.chain.model.structure;
			GameObject go = s.atomToGo[a];
			Transform t = go.transform;
			float scale = t.lossyScale.x;
			atomPos[id] = t.position;
			allAtoms[id] = a;
			atomRad[id] = a.radius * 0.5f * scale;
			allTransforms[id] = t;
			id++;

		}
	}

	void updatePositions() {
		for (int i = 0; i < allTransforms.Length; i++) {
			atomPos[i] = allTransforms[i].position;
		}
	}

	void updateRadii() {

		for (int i = 0; i < allAtoms.Length; i++) {
			Transform t = allTransforms[i];
			float scale = t.lossyScale.x;
			atomRad[i] = allAtoms[i].radius * 0.5f * scale;
		}
	}

	public void Clean() {
		if (atomPos.IsCreated) {
			atomPos.Dispose();
			atomRad.Dispose();
			results.Dispose();
		}
	}

	public UnityMolAtom customRaycastAtomBurst(Vector3 origin, Vector3 direction) {


		if (needsFullUpdate) {
			updateAllLists();
			needsFullUpdate = false;
			needsUpdatePos = false;
			needsUpdateRadii = false;
		}
		if (needsUpdatePos) {
			updatePositions();
			needsUpdatePos = false;
		}
		if (needsUpdateRadii) {
			updateRadii();
			needsUpdateRadii = false;
		}

		if (DEBUG) {
			CleanDebug();
			int idA = 0;
			foreach (Transform t in allTransforms) {
				GameObject test = GameObject.CreatePrimitive(PrimitiveType.Cube);
				debugGos.Add(test);
				test.transform.parent = t;
				test.transform.localPosition = Vector3.zero;
				test.transform.localScale = Vector3.one;
				idA++;
			}
		}

		var raycastJob = new RaycastJob() {
			res = results,
			pos = atomPos,
			radii = atomRad,
			ori = origin,
			dir = direction
		};

		var raycastJobHandle = raycastJob.Schedule(results.Length, 64);
		raycastJobHandle.Complete();

		float minDist = 99999.0f;
		int idMin = -1;
		for (int i = 0; i < results.Length; i++) {
			if (results[i]) {
				float sdist = squaredDist(origin, atomPos[i]);
				if (sdist < minDist) {
					minDist = sdist;
					idMin = i;
				}
			}
		}

		if (idMin >= 0 && idMin < results.Length) {
			//Make sure to get the atom from the current model
			UnityMolStructure s = allAtoms[idMin].residue.chain.model.structure;
			int idAtom = allAtoms[idMin].idInAllAtoms;
			return s.currentModel.allAtoms[idAtom];
		}

		return null;

	}

	static void	CleanDebug() {
		if (debugGos.Count != 0) {
			for (int i = 0; i < debugGos.Count; i++) {
				GameObject.Destroy(debugGos[i]);
			}
			debugGos.Clear();
		}
	}

	static float squaredDist(float3 a, float3 b) {

		float3 d = a - b;
		return math.dot(d, d);
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

	[BurstCompile]
	struct RaycastJob : IJobParallelFor
	{
		[ReadOnly] public NativeArray<float3> pos;
		[ReadOnly] public NativeArray<float> radii;
		public NativeArray<bool> res;
		[ReadOnly] public float3 ori;
		[ReadOnly] public float3 dir;

		void IJobParallelFor.Execute(int index)
		{
			float3 l = pos[index] - ori;
			float tc = math.dot(l, dir);

			if (tc < 0.0f) {
				res[index] = false;
				return;
			}

			float ll = math.dot(l, l);

			float d2 = ll - (tc * tc);

			float rad2 = radii[index] * radii[index];

			if (d2 > rad2) {
				res[index] = false;
			}
			else {
				res[index] = true;
			}
		}
	}

}
}