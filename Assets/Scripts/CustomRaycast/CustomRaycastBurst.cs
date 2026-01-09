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

	NativeArray<float3> atomPosTrajExtract;
	NativeArray<float> atomRadTrajExtract;
	NativeArray<bool> resultsTrajExtract;
	public UnityMolAtom[] allAtomsExtract;

	public bool needsFullUpdate = true;
	public bool needsUpdatePos = false;
	public bool needsUpdateRadii = false;

	public UnityMolAtom[] allAtoms;
	UnityMolRepresentationManager repM;

	void updateAllLists() {
		if (repM == null)
			repM = UnityMolMain.getRepresentationManager();

		int N = repM.atomsWithActiveRep.Count;

		if (atomPos.IsCreated) {
			atomPos.Dispose();
			atomRad.Dispose();
			results.Dispose();
		}

		atomPos = new NativeArray<float3>(N, Allocator.Persistent);
		atomRad = new NativeArray<float>(N, Allocator.Persistent);
		results = new NativeArray<bool>(N, Allocator.Persistent);

		allAtoms = new UnityMolAtom[N];

		int id = 0;
		foreach (UnityMolAtom a in repM.atomsWithActiveRep) {
			UnityMolStructure s = a.residue.chain.model.structure;
			float scale = s.annotationParent.transform.lossyScale.x;
			atomPos[id] = a.curWorldPosition;
			allAtoms[id] = a;
			atomRad[id] = a.radius * 0.5f * scale;
			id++;
		}

		int Nextract = repM.countExtractTrajActive;

		if (Nextract != 0) {

			if (atomPosTrajExtract.IsCreated) {
				atomPosTrajExtract.Dispose();
				atomRadTrajExtract.Dispose();
				resultsTrajExtract.Dispose();
			}

			atomPosTrajExtract = new NativeArray<float3>(Nextract, Allocator.Persistent);
			atomRadTrajExtract = new NativeArray<float>(Nextract, Allocator.Persistent);
			resultsTrajExtract = new NativeArray<bool>(Nextract, Allocator.Persistent);

			allAtomsExtract = new UnityMolAtom[Nextract];

			int idExtr = 0;


			foreach (UnityMolRepresentation r in repM.extractedTrajRep) {
				for (int i = 0; i < r.selection.extractTrajFramePositions.Count; i++) {
					for (int j = 0; j < r.selection.atoms.Count; j++) {
						UnityMolAtom a = r.selection.atoms[j];
						UnityMolStructure s = a.residue.chain.model.structure;
						float scale = s.annotationParent.transform.lossyScale.x;

						if (idExtr < Nextract) {
							Vector3 pos = r.selection.extractTrajFramePositions[i][j];
							atomPosTrajExtract[idExtr] = s.annotationParent.transform.TransformPoint(pos);
							atomRadTrajExtract[idExtr] = a.radius * 0.5f * scale;
							allAtomsExtract[idExtr] = a;
							idExtr++;
						}
						else {//Should not happen
							break;
						}
					}
				}
			}
		}
	}

	void updatePositions() {
		if (repM == null)
			repM = UnityMolMain.getRepresentationManager();

		for (int i = 0; i < allAtoms.Length; i++) {
			atomPos[i] = allAtoms[i].curWorldPosition;
		}

		if (atomPosTrajExtract.IsCreated && atomPosTrajExtract.Length != 0) {

			int idExtr = 0;
			int Nextract = atomPosTrajExtract.Length;
			foreach (UnityMolRepresentation r in repM.extractedTrajRep) {
				for (int i = 0; i < r.selection.extractTrajFramePositions.Count; i++) {
					for (int j = 0; j < r.selection.atoms.Count; j++) {
						UnityMolAtom a = r.selection.atoms[j];
						UnityMolStructure s = a.residue.chain.model.structure;

						if (idExtr < Nextract) {
							Vector3 pos = r.selection.extractTrajFramePositions[i][j];
							atomPosTrajExtract[idExtr] = s.annotationParent.transform.TransformPoint(pos);
							idExtr++;
						}
					}
				}
			}
		}
	}

	void updateRadii() {
		if (repM == null)
			repM = UnityMolMain.getRepresentationManager();

		for (int i = 0; i < allAtoms.Length; i++) {
			float scale = allAtoms[i].residue.chain.model.structure.annotationParent.transform.lossyScale.x;
			atomRad[i] = allAtoms[i].radius * 0.5f * scale;
		}

		if (atomPosTrajExtract.IsCreated && atomPosTrajExtract.Length != 0) {

			int Nextract = atomRadTrajExtract.Length;
			for (int i = 0; i < Nextract; i++) {
				UnityMolAtom a = allAtomsExtract[i];
				UnityMolStructure s = a.residue.chain.model.structure;
				float scale = s.annotationParent.transform.lossyScale.x;
				atomRadTrajExtract[i] = a.radius * 0.5f * scale;
			}
		}
	}

	public void Clean() {
		if (atomPos.IsCreated) {
			atomPos.Dispose();
			atomRad.Dispose();
			results.Dispose();
		}
		if (atomPosTrajExtract.IsCreated) {
			atomPosTrajExtract.Dispose();
			atomRadTrajExtract.Dispose();
			resultsTrajExtract.Dispose();
		}
	}

	public UnityMolAtom customRaycastAtomBurst(Vector3 origin, Vector3 direction,
	        ref Vector3 worldPos, ref bool isExtractedAtom, bool useExtractedTraj = true) {

		isExtractedAtom = false;

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

		float minDistEx = 99999.0f;
		int idMinEx = -1;

		if (useExtractedTraj) {
			if (atomPosTrajExtract.IsCreated && atomPosTrajExtract.Length != 0) {

				var raycastJobEx = new RaycastJob() {
					res = resultsTrajExtract,
					pos = atomPosTrajExtract,
					radii = atomRadTrajExtract,
					ori = origin,
					dir = direction
				};

				var raycastJobHandleEx = raycastJobEx.Schedule(resultsTrajExtract.Length, 64);
				raycastJobHandleEx.Complete();


				for (int i = 0; i < resultsTrajExtract.Length; i++) {
					if (resultsTrajExtract[i]) {
						float sdist = squaredDist(origin, atomPosTrajExtract[i]);
						if (sdist < minDistEx) {
							minDistEx = sdist;
							idMinEx = i;
						}
					}
				}
			}
		}


		if (idMinEx < 0) {//No extracted trajectory frame atoms
			if (idMin >= 0 && idMin < results.Length) {
				//Make sure to get the atom from the current model
				UnityMolStructure s = allAtoms[idMin].residue.chain.model.structure;
				int idAtom = allAtoms[idMin].idInAllAtoms;
				worldPos = atomPos[idMin];
				return s.currentModel.allAtoms[idAtom];
			}
		}
		else {//Choose closest between normal atom and atom from extracted trajectory frames
			if (idMin >= 0 && idMin < results.Length && idMinEx < resultsTrajExtract.Length) {
				if (minDistEx < minDist) { //Traj frame atom in front
					UnityMolStructure s = allAtomsExtract[idMinEx].residue.chain.model.structure;
					int idAtom = allAtomsExtract[idMinEx].idInAllAtoms;
					worldPos = atomPosTrajExtract[idMinEx];
					isExtractedAtom = true;
					return s.currentModel.allAtoms[idAtom];
				}
				else {
					UnityMolStructure s = allAtoms[idMin].residue.chain.model.structure;
					int idAtom = allAtoms[idMin].idInAllAtoms;
					worldPos = atomPos[idMin];
					return s.currentModel.allAtoms[idAtom];
				}
			}
			else if (idMin < 0 && idMinEx < resultsTrajExtract.Length) { //No normal atoms found but extracted trajectory frame atom found
				UnityMolStructure s = allAtomsExtract[idMinEx].residue.chain.model.structure;
				int idAtom = allAtomsExtract[idMinEx].idInAllAtoms;
				worldPos = atomPosTrajExtract[idMinEx];
				isExtractedAtom = true;
				return s.currentModel.allAtoms[idAtom];
			}
		}

		return null;

	}


	///Use structure bounding box instead of active atoms
	public UnityMolStructure customRaycastStructure(Vector3 origin, Vector3 direction) {

		UnityMolStructureManager sm = UnityMolMain.getStructureManager();
		Transform molPar = UnityMolMain.getRepresentationParent().transform;

		float bestDist = float.MaxValue;
		UnityMolStructure intersectedStructure = null;
		Ray r = new Ray(origin, direction);

		foreach (UnityMolStructure s in sm.loadedStructures) {
			Bounds b = new Bounds();
			Vector3 wpMax = s.annotationParent.parent.TransformPoint(s.currentModel.maximumPositions);
			Vector3 wpMin = s.annotationParent.parent.TransformPoint(s.currentModel.minimumPositions);
			b.Encapsulate(wpMax);
			b.Encapsulate(wpMin);
			b.center = s.annotationParent.parent.TransformPoint(s.currentModel.centroid);
			float dist = 0.0f;
			if (b.IntersectRay(r, out dist)) {
				if (dist < bestDist) {
					bestDist = dist;
					intersectedStructure = s;
				}
			}
		}

		return intersectedStructure;

	}

	///Use structure bounding box to find the closest structure to a world space position
	public UnityMolStructure customClosestStructure(Vector3 wpos) {
		UnityMolStructureManager sm = UnityMolMain.getStructureManager();
		Transform molPar = UnityMolMain.getRepresentationParent().transform;
		float bestDist = float.MaxValue;
		UnityMolStructure found = null;
		foreach (UnityMolStructure s in sm.loadedStructures) {
			Bounds b = new Bounds();
			Vector3 wpMax = sm.structureToGameObject[s.name].transform.TransformPoint(s.currentModel.maximumPositions);
			Vector3 wpMin = sm.structureToGameObject[s.name].transform.TransformPoint(s.currentModel.minimumPositions);
			b.Encapsulate(wpMax);
			b.Encapsulate(wpMin);
			b.center = sm.structureToGameObject[s.name].transform.TransformPoint(s.currentModel.centroid);

			Vector3 p = b.ClosestPoint(wpos);
			float d = Vector3.Distance(p, wpos);
			if (d < bestDist) {
				bestDist = d;
				found = s;
			}
		}

		return found;
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