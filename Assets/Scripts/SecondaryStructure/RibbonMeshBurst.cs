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
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


namespace UMol {

public class RibbonMeshBurst {

	public static int splineSteps = 16;
	public static int profileDetail = 8;

	public static int trajSplineSteps = 4;
	public static int trajProfileDetail = 4;

	public static float ribbonWidth = 6.5f;
	public static float ribbonHeight = 0.125f;
	public static float ribbonOffset = 1.5f;
	public static float arrowHeadWidth = 3.0f;
	public static float arrowWidth = 2.0f;
	public static float arrowHeight = 0.5f;
	public static float tubeSize = 1.0f;

	public static float limitDistCA_CA = 5.0f; //Max distance between 2 CA in Angstrom

	// cartoon input => CA & O positions / ss type / residue ids
	public static MeshData createChainMesh(int idF, UnityMolSelection sel,
	                                       List<UnityMolResidue> residues,
	                                       ref Dictionary<UnityMolResidue, List<int>> residueToVert,
	                                       float ctubeSize, bool onlyTube, bool tubeAsBfactor, bool isTraj = false) {

		tubeSize = ctubeSize;

		float start = Time.realtimeSinceStartup;

		NativeArray<float3> caPos = new NativeArray<float3>(residues.Count, Allocator.Persistent);
		NativeArray<float3> oPos = new NativeArray<float3>(residues.Count, Allocator.Persistent);
		NativeArray<int> resId = new NativeArray<int>(residues.Count, Allocator.Persistent);
		NativeArray<bool> missing = new NativeArray<bool>(residues.Count, Allocator.Persistent);
		NativeArray<UnityMolResidue.secondaryStructureType> ssType =
		    new NativeArray<UnityMolResidue.secondaryStructureType>(residues.Count, Allocator.Persistent);
		NativeArray<float> bfs = new NativeArray<float>(residues.Count, Allocator.Persistent);

		int cptNotMissing = 0;
		float minBfac = float.MaxValue;
		float maxBfac = float.MinValue;
		for (int idR = 0; idR < residues.Count; idR++) {
			UnityMolResidue r = residues[idR];
			if (!r.atoms.ContainsKey("CA") || !r.atoms.ContainsKey("O")) {
				missing[idR] = true;
				// caPos[idR] = dummy;
				// oPos[idR] = dummy;
				continue;
			}
			if (idR < residues.Count - 1 && !consecutiveResidue(residues[idR].id, residues[idR + 1].id)) {
				missing[idR] = true;
				// caPos[idR] = dummy;
				// oPos[idR] = dummy;
				// missing[idR + 1] = true;
				continue;
			}
			if (idR > 0 && !missing[idR - 1] && Vector3.Distance(r.atoms["CA"].position, caPos[idR - 1]) >= limitDistCA_CA) {
				missing[idR] = true;
				continue;
			}

			cptNotMissing++;
			missing[idR] = false;

			caPos[idR] = r.atoms["CA"].position;
			oPos[idR] = r.atoms["O"].position;
			bfs[idR] = r.atoms["CA"].bfactor;
			minBfac = math.min(minBfac, bfs[idR]);
			maxBfac = math.max(maxBfac, bfs[idR]);
			if (idF != -1) {
				int ida = sel.atomToIdInSel[r.atoms["CA"]];
				caPos[idR] = sel.extractTrajFramePositions[idF][ida];
				ida = sel.atomToIdInSel[r.atoms["O"]];
				oPos[idR] = sel.extractTrajFramePositions[idF][ida];
			}

			resId[idR] = r.id;
			ssType[idR] = r.secondaryStructure;
		}
		if (cptNotMissing < 3) {
			caPos.Dispose();
			oPos.Dispose();
			resId.Dispose();
			missing.Dispose();
			ssType.Dispose();
			bfs.Dispose();
			return null;
		}

		//Normalize bfactors between 0.5 and 2
		if (onlyTube && tubeAsBfactor && math.abs(maxBfac - minBfac) > 0.01f) {

			float div = 1.0f / (maxBfac - minBfac);
			for (int i = 0; i < bfs.Length; i++) {
				if (!missing[i]) {
					bfs[i] = 1.5f * ((bfs[i] - minBfac) * div) + 0.5f;
				}
			}
		}

		MeshData mesh = computeCartoonMesh(caPos, oPos, resId, missing, bfs, ssType, onlyTube, tubeAsBfactor, isTraj);


		// Fill residueToVert
		int pdetail = profileDetail;
		int ssteps = splineSteps;
		if (isTraj) {
			pdetail = trajProfileDetail;
			ssteps = trajSplineSteps;
		}
		int splineNPoints = ssteps + 2;
		float invSplineN = 1.0f / (float) splineNPoints;
		int nbPoints = splineNPoints * residues.Count;
		int nVert = 4 * nbPoints;
		for (int i = 0; i < residues.Count; i++) {
			UnityMolResidue r = residues[i];
			residueToVert[r] = new List<int>(4 * splineNPoints);
			int sv = (4 * splineNPoints) * i;
			for (int j = 0; j < 4 * splineNPoints; j++) {
				residueToVert[r].Add(sv + j);
			}
		}

		caPos.Dispose();
		oPos.Dispose();
		resId.Dispose();
		ssType.Dispose();
		missing.Dispose();
		bfs.Dispose();

		// #if UNITY_EDITOR
		// UnityEngine.Debug.Log("Time for burst cartoon: " + (1000.0f * (Time.realtimeSinceStartup - start)).ToString("f3") + " ms");
		// #endif

		return mesh;
	}

	public static MeshData computeCartoonMesh(NativeArray<float3> caPos, NativeArray<float3> oPos,
	        NativeArray<int> resId, NativeArray<bool> missing, NativeArray<float> bfs,
	        NativeArray<UnityMolResidue.secondaryStructureType> ssType, bool onlyTube, bool tubeAsBfactor, bool isTraj) {

		int pdetail = profileDetail;
		int ssteps = splineSteps;
		if (isTraj) {
			pdetail = trajProfileDetail;
			ssteps = trajSplineSteps;
		}

		NativeArray<float4> bspline = new NativeArray<float4>(4, Allocator.TempJob);
		bspline[0] = new float4(-1.0f, 3.0f, -3.0f, 1.0f) / 6.0f;
		bspline[1] = new float4(3.0f, -6.0f, 3.0f, 0.0f) / 6.0f;
		bspline[2] = new float4(-3.0f, 0.0f, 3.0f, 0.0f) / 6.0f;
		bspline[3] = new float4(1.0f, 4.0f, 1.0f, 0.0f) / 6.0f;

		int4 curhelixCol = new int4(230, 159, 0, 255);
		int4 curhelixPi = new int4(250, 228, 66, 255);
		int4 curhelix310 = new int4(213, 94, 0, 255);
		int4 curhelixOther = new int4(204, 121, 167, 255);
		int4 curstrandCol = new int4(0, 114, 178, 255);
		int4 curcoilCol = new int4(155, 155, 155, 255);


		int nbRes = missing.Length;

		int splineNPoints = ssteps + 2;
		float invSplineN = 1.0f / (float) splineNPoints;
		int nbPoints = splineNPoints * nbRes;
		int nVert = 4 * nbPoints;

		NativeArray<float3> Gvec = new NativeArray<float3>(nbRes, Allocator.TempJob);
		NativeArray<float2> widthH = new NativeArray<float2>(nbRes, Allocator.TempJob);
		NativeArray<float3> controlPoints = new NativeArray<float3>(nbRes, Allocator.TempJob);
		NativeArray<float3> pNormals = new NativeArray<float3>(nbRes, Allocator.TempJob);


		var cartoonJob0 = new CartoonJobStep0() {
			caPos = caPos,
			oPos = oPos,
			missing = missing,
			Gvec = Gvec
		};
		var cartoonJob0Handle = cartoonJob0.Schedule();
		cartoonJob0Handle.Complete();


		var cartoonJob1 = new CartoonJobStep1() {
			caPos = caPos,
			oPos = oPos,
			missing = missing,
			pNormals = pNormals,
			ssType = ssType,
			controlPoints = controlPoints,
			widthHeight = widthH,
			rWidth = ribbonWidth,
			tSize = tubeSize,
			allAsCoil = onlyTube,
			asBfactor = tubeAsBfactor,
			bfactors = bfs,
			Gvec = Gvec
		};
		var cartoonJobHandle = cartoonJob1.Schedule(nbRes, Mathf.Min(nbRes, 8));
		cartoonJobHandle.Complete();

		pNormals[0] = pNormals[1];
		for (int i = 1; i < nbRes; i++) {
			if (missing[i]) {
				controlPoints[i] = controlPoints[i - 1];
			}
		}
		Gvec.Dispose();


		NativeArray<float3> splinePoints = new NativeArray<float3>(nbPoints, Allocator.TempJob);
		NativeArray<float3> splineNormals = new NativeArray<float3>(nbPoints, Allocator.TempJob);
		NativeArray<int4> splineCols = new NativeArray<int4>(nbPoints, Allocator.TempJob);
		NativeArray<float2> splineWidthHeigths = new NativeArray<float2>(nbPoints, Allocator.TempJob);
		NativeArray<float> coilPerPoint = new NativeArray<float>(nbPoints, Allocator.TempJob);

		var cartoonJob2 = new CartoonJobStep2() {
			splineNPoints = splineNPoints,
			invSplineN = invSplineN,
			ssType = ssType,
			bspline = bspline,
			pNormals = pNormals,
			splinePoints = splinePoints,
			splineNormals = splineNormals,
			splineCols = splineCols,
			splineWidthHeigths = splineWidthHeigths,
			controlPoints = controlPoints,
			widthHeight = widthH,
			curhelixCol = curhelixCol,
			curhelixPi = curhelixPi,
			curhelix310 = curhelix310,
			curhelixOther = curhelixOther,
			curstrandCol = curstrandCol,
			curcoilCol = curcoilCol,
			coilPerPoint = coilPerPoint
		};


		var cartoonJobHandle2 = cartoonJob2.Schedule(nbRes, Mathf.Min(nbRes, 8));
		cartoonJobHandle2.Complete();

		bspline.Dispose();
		widthH.Dispose();
		controlPoints.Dispose();
		pNormals.Dispose();


		NativeArray<float3> verts = new NativeArray<float3>(nVert, Allocator.TempJob);
		NativeArray<float3> norms = new NativeArray<float3>(nVert, Allocator.TempJob);
		NativeArray<int4> cols = new NativeArray<int4>(nVert, Allocator.TempJob);
		NativeArray<int> tris = new NativeArray<int>(nbPoints * 3 * 8, Allocator.TempJob);


		var cartoonJob3 = new CartoonJobStep3() {
			nbPoints = nbPoints,
			vertices = verts,
			normals = norms,
			colors = cols,
			triangles = tris,
			splinePoints = splinePoints,
			splineNormals = splineNormals,
			splineCols = splineCols,
			splineWidthHeigths = splineWidthHeigths,
			coilPerPoint = coilPerPoint
		};

		var cartoonJobHandle3 = cartoonJob3.Schedule(nbPoints, Mathf.Min(nbPoints, 8));
		cartoonJobHandle3.Complete();


		MeshData mesh = new MeshData();
		Vector3[] allVerts = new Vector3[verts.Length];
		Vector3[] allNorms = new Vector3[verts.Length];
		Color32[] allCols = new Color32[verts.Length];
		int[] allTris = new int[tris.Length];

		SetNativeArray(allVerts, verts);
		SetNativeArray(allNorms, norms);
		SetNativeArrayTri(allTris, tris);
		for (int i = 0; i < verts.Length; i++) {
			allCols[i] = new Color32((byte) cols[i].x, (byte) cols[i].y, (byte) cols[i].z, 255);
		}

		if (allTris.Length != 0 && allVerts.Length != 0) {
			mesh.triangles = allTris;
			mesh.vertices = allVerts;
			// mesh.normals = allNorms;//Wrong normals
			mesh.colors = allCols;
			mesh.normals = allNorms;
		}


		verts.Dispose();
		tris.Dispose();
		norms.Dispose();
		cols.Dispose();


		splinePoints.Dispose();
		splineNormals.Dispose();
		splineCols.Dispose();
		splineWidthHeigths.Dispose();
		coilPerPoint.Dispose();

		if (allTris.Length == 0 || allVerts.Length == 0) {
			return null;
		}

		return mesh;

	}

	[BurstCompile]
	struct CartoonJobStep0 : IJob {

		public NativeArray<float3> Gvec;

		[ReadOnly] public NativeArray<float3> caPos;
		[ReadOnly] public NativeArray<float3> oPos;

		[ReadOnly] public NativeArray<bool> missing;

		void IJob.Execute() {
			float3 G = float3.zero;
			Gvec[0] = G;
			int nbRes = missing.Length;
			for (int index = 1; index < nbRes; index++) {
				int m1 = math.max(0, index - 1);
				float3 CA = caPos[index];
				float3 O = oPos[index];

				float3 CAm1 = caPos[m1];
				float3 Om1 = oPos[m1];

				if (missing[m1]) {
					CAm1 = CA;
					Om1 = O;
				}

				//Compute normals

				float3 A = CA - CAm1;
				float3 B = Om1 - CAm1;
				float3 C = math.cross(A, B);//Residue-1 peptide plane
				float3 D = math.cross(C, A);//Normal to plane and backbone

				float3 BB = (math.dot(D, Gvec[m1]) < 0.0f) ? -D : D;
				float3 E = math.normalize(Gvec[m1] + BB);
				if (math.isnan(E.x)) {
					E = float3.zero;
				}


				Gvec[index] = E;

			}
		}

	}

	[BurstCompile]
	struct CartoonJobStep1 : IJobParallelFor {

		public NativeArray<float3> pNormals;

		public NativeArray<float3> controlPoints;

		public NativeArray<float2> widthHeight;

		[ReadOnly] public NativeArray<float3> caPos;
		[ReadOnly] public NativeArray<float3> oPos;
		[ReadOnly] public NativeArray<float> bfactors;
		[ReadOnly] public NativeArray<float3> Gvec;
		[ReadOnly] public NativeArray<bool> missing;
		[ReadOnly] public NativeArray<UnityMolResidue.secondaryStructureType> ssType;
		[ReadOnly] public float rWidth;
		[ReadOnly] public float tSize;
		[ReadOnly] public bool allAsCoil;
		[ReadOnly] public bool asBfactor;


		void IJobParallelFor.Execute(int index) {
			int nbRes = missing.Length;
			float bRad = 0.15f;

			int m1 = math.max(0, index - 1);
			float3 CA = caPos[index];
			float3 O = oPos[index];

			float3 CAm1 = caPos[m1];
			float3 Om1 = oPos[m1];

			if (missing[m1]) {
				CAm1 = CA;
				Om1 = O;
			}


			UnityMolResidue.secondaryStructureType ss = ssType[index];

			if (allAsCoil) {
				controlPoints[index] = CA;
				if (asBfactor)
					widthHeight[index] = new float2(bRad * tSize * bfactors[index], bRad * tSize * bfactors[index]);
				else
					widthHeight[index] = new float2(bRad * tSize, bRad * tSize);
			} else {
				switch (ss) {
				case UnityMolResidue.secondaryStructureType.Helix:
				case UnityMolResidue.secondaryStructureType.HelixRightOmega:
				case UnityMolResidue.secondaryStructureType.Helix310:
				case UnityMolResidue.secondaryStructureType.HelixRightGamma:
				case UnityMolResidue.secondaryStructureType.HelixRightPi:
					//Use alpha carbon as control point
					controlPoints[index] = CA;
					widthHeight[index] = new float2(rWidth * bRad, bRad / 2.0f);
					break;

				case UnityMolResidue.secondaryStructureType.Strand:
					widthHeight[index] = new float2(rWidth * bRad, bRad / 2.0f);

					float3 CAp1 = CA;
					bool head = false;
					if (index + 1 >= nbRes) {
						head = true;
					} else {
						CAp1 = caPos[index + 1];
						if (missing[index + 1]) {
							CAp1 = CA;
						}
						if (ssType[index + 1] != UnityMolResidue.secondaryStructureType.Strand) {
							head = true;
						}
					}
					if (head) {
						widthHeight[index] = new float2(-rWidth * bRad * 1.75f, bRad / 2.0f);
					}

					float3 filteredPos = ((CA + CA) + CAp1 + CAm1) * 0.25f;
					controlPoints[index] = filteredPos;
					break;

				default: //Coil
					controlPoints[index] = CA;
					widthHeight[index] = new float2(bRad * tSize, bRad * tSize);
					break;
				}
			}

			float3 G = Gvec[index];
			pNormals[index] = G;

			if (missing[index] || missing[m1]) {
				// widthHeight[m1] = float2.zero;
				widthHeight[index] = float2.zero;
				pNormals[index] = float3.zero;
			}

		}

	}

	[BurstCompile]
	struct CartoonJobStep2 : IJobParallelFor {

		[ReadOnly] public int4 curhelixCol;
		[ReadOnly] public int4 curhelixPi;
		[ReadOnly] public int4 curhelix310;
		[ReadOnly] public int4 curhelixOther;
		[ReadOnly] public int4 curstrandCol;
		[ReadOnly] public int4 curcoilCol;

		[NativeDisableParallelForRestriction]
		public NativeArray<float3> splinePoints;
		[NativeDisableParallelForRestriction]
		public NativeArray<float3> splineNormals;
		[NativeDisableParallelForRestriction]
		public NativeArray<int4> splineCols;
		[NativeDisableParallelForRestriction]
		public NativeArray<float2> splineWidthHeigths;

		[NativeDisableParallelForRestriction]
		public NativeArray<float> coilPerPoint;

		[ReadOnly] public NativeArray<float3> pNormals;

		[ReadOnly] public NativeArray<float3> controlPoints;

		[ReadOnly] public NativeArray<float2> widthHeight;

		[ReadOnly] public NativeArray<UnityMolResidue.secondaryStructureType> ssType;
		[ReadOnly] public NativeArray<float4> bspline;
		[ReadOnly] public int splineNPoints;
		[ReadOnly] public float invSplineN;


		void IJobParallelFor.Execute(int index) {
			int nbRes = ssType.Length;
			int im1 = math.max(0, index - 1);
			int im2 = math.max(0, index - 2);
			int ip1 = math.min(nbRes - 1, index + 1);

			float4x4 qMat = float4x4.identity;

			int4 curCol = getResidueSSColor(ssType[index]);
			int4 colm1 = getResidueSSColor(ssType[im1]);

			float4 curColf = new float4(curCol);
			float4 colm1f = new float4(colm1);


			float a = 0.0f;
			float b = 0.0f;
			float c = 0.0f;
			for (int i = 0; i < 4; i++) {
				a = bspline[i].x * controlPoints[im2].x;
				b = bspline[i].x * controlPoints[im2].y;
				c = bspline[i].x * controlPoints[im2].z;

				a += bspline[i].y * controlPoints[im1].x;
				b += bspline[i].y * controlPoints[im1].y;
				c += bspline[i].y * controlPoints[im1].z;

				a += bspline[i].z * controlPoints[index].x;
				b += bspline[i].z * controlPoints[index].y;
				c += bspline[i].z * controlPoints[index].z;

				a += bspline[i].w * controlPoints[ip1].x;
				b += bspline[i].w * controlPoints[ip1].y;
				c += bspline[i].w * controlPoints[ip1].z;

				qMat[i][0] = a;
				qMat[i][1] = b;
				qMat[i][2] = c;

			}

			for (int j = 0; j < splineNPoints; j++) {
				float v = j * invSplineN;
				float vv = 1.0f - v;
				float4 tmp = v * (v * (v * qMat[0] + qMat[1]) + qMat[2]) + qMat[3];
				splinePoints[index * splineNPoints + j] = tmp.xyz;

				//Compute normal for each point
				splineNormals[index * splineNPoints + j] = math.normalize(vv * pNormals[im1] + v * pNormals[index]);
				//Get the color of the spline point
				splineCols[index * splineNPoints + j] = (int4) math.lerp(colm1f, curColf, v);

				if (ssType[index] != UnityMolResidue.secondaryStructureType.Coil) {
					coilPerPoint[index * splineNPoints + j] = 0.0f;
				}
				else if ( ssType[index] != ssType[im1]) {//End of coil
					coilPerPoint[index * splineNPoints + j] = v;
				}
				else if ( ssType[index] != ssType[ip1]) {//Start of coil
					coilPerPoint[index * splineNPoints + j] = vv;
				}
				else {//Only coil
					coilPerPoint[index * splineNPoints + j] = 1.0f;
				}
			}


			//Compute width and heigth for each point
			float wm1 = widthHeight[im1].x;
			float w = widthHeight[index].x;
			float wp1 = widthHeight[ip1].x;
			float hm1 = widthHeight[im1].y;
			float h = widthHeight[index].y;

			if (w >= 0 && wm1 >= 0) {
				for (int j = 0; j < splineNPoints; j++) {
					float v = j * invSplineN;
					float vv = 1.0f - v;
					splineWidthHeigths[index * splineNPoints + j] = new float2(vv * wm1 + v * w, vv * hm1 + v * h);
				}
			} else {
				if (wp1 < 0) {
					wp1 = -wp1;
				}
				if (w < 0) {
					w = -w;

					for (int j = 0; j < splineNPoints / 2; j++) {
						float v = j * invSplineN;
						splineWidthHeigths[index * splineNPoints + j] = new float2(wm1, (1.0f - v) * hm1 + v * h);
					}
					for (int j = splineNPoints / 2; j < splineNPoints; j++) {
						float v = j * invSplineN;
						float nv = (j - splineNPoints / 2) * invSplineN;
						splineWidthHeigths[index * splineNPoints + j] = new float2((1.0f - nv) * w + nv * wp1, (1.0f - v) * hm1 + v * h);
					}
				} else {
					wm1 = -wm1;

					for (int j = 0; j < splineNPoints / 2; j++) {

						float v = j * invSplineN;
						float nv = (j + (splineNPoints - splineNPoints / 2)) * invSplineN;
						splineWidthHeigths[index * splineNPoints + j] = new float2((1.0f - nv) * wm1 + nv * w, (1.0f - v) * hm1 + v * h);
					}
					for (int j = splineNPoints / 2; j < splineNPoints; j++) {
						float v = j * invSplineN;
						splineWidthHeigths[index * splineNPoints + j] = new float2(w, (1.0f - v) * hm1 + v * h);
					}
				}
			}
		}
		int4 getResidueSSColor(UnityMolResidue.secondaryStructureType t) {
			switch (t) {
			case UnityMolResidue.secondaryStructureType.HelixRightOmega:
			case UnityMolResidue.secondaryStructureType.HelixRightGamma:
			case UnityMolResidue.secondaryStructureType.HelixLeftAlpha:
			case UnityMolResidue.secondaryStructureType.HelixLeftOmega:
			case UnityMolResidue.secondaryStructureType.HelixLeftGamma:
			case UnityMolResidue.secondaryStructureType.PolyProline:
			case UnityMolResidue.secondaryStructureType.Helix27:
				return curhelixOther;
			case UnityMolResidue.secondaryStructureType.HelixRightPi:
				return curhelixPi;
			case UnityMolResidue.secondaryStructureType.Helix310:
				return curhelix310;
			case UnityMolResidue.secondaryStructureType.Helix:
				return curhelixCol;
			case UnityMolResidue.secondaryStructureType.Strand:
				return curstrandCol;
			default:
				return curcoilCol;

			}
		}

	}

	[BurstCompile]
	struct CartoonJobStep3 : IJobParallelFor {
		[NativeDisableParallelForRestriction]
		public NativeArray<float3> vertices;
		[NativeDisableParallelForRestriction]
		public NativeArray<int4> colors;
		[NativeDisableParallelForRestriction]
		public NativeArray<float3> normals;
		[NativeDisableParallelForRestriction]
		public NativeArray<int> triangles;

		[ReadOnly] public NativeArray<float3> splinePoints;
		[ReadOnly] public NativeArray<float3> splineNormals;
		[ReadOnly] public NativeArray<int4> splineCols;
		[ReadOnly] public NativeArray<float2> splineWidthHeigths;

		[ReadOnly] public NativeArray<float> coilPerPoint;


		[ReadOnly] public int nbPoints;


		void IJobParallelFor.Execute(int index) {

			int idT = index * 24;
			int idV = index * 4;
			//Compute forward vector & up vector
			float3 frontV = float3.zero;
			if (index < nbPoints - 1) {
				frontV = splinePoints[index] - splinePoints[index + 1];
			} else if (index > 0) {
				frontV = splinePoints[index - 1] - splinePoints[index];
			}

			float3 normalV = splineNormals[index];
			float3 upV = math.normalize(math.cross(normalV, frontV));

			float3 v1 = (splinePoints[index] + upV * splineWidthHeigths[index].y) + normalV * splineWidthHeigths[index].x;
			float3 v2 = (splinePoints[index] + upV * splineWidthHeigths[index].y) - normalV * splineWidthHeigths[index].x;

			float3 v3 = (splinePoints[index] - upV * splineWidthHeigths[index].y) + normalV * splineWidthHeigths[index].x;
			float3 v4 = (splinePoints[index] - upV * splineWidthHeigths[index].y) - normalV * splineWidthHeigths[index].x;

			vertices[idV] = v1;
			vertices[idV + 1] = v2;
			vertices[idV + 2] = v3;
			vertices[idV + 3] = v4;

			colors[idV] = splineCols[index];
			colors[idV + 1] = splineCols[index];
			colors[idV + 2] = splineCols[index];
			colors[idV + 3] = splineCols[index];

			normals[idV] = upV + (normalV * coilPerPoint[index]);
			normals[idV + 1] = upV - (normalV * coilPerPoint[index]);
			normals[idV + 2] = -upV + (normalV * coilPerPoint[index]);
			normals[idV + 3] = -upV - (normalV * coilPerPoint[index]);

			idV += 4;

			if (index != nbPoints - 1) {

				//Top
				triangles[idT++] = index * 4;
				triangles[idT++] = index * 4 + 1;
				triangles[idT++] = (index + 1) * 4;
				//Top
				triangles[idT++] = index * 4 + 1;
				triangles[idT++] = (index + 1) * 4 + 1;
				triangles[idT++] = (index + 1) * 4;

				//Bottom
				triangles[idT++] = index * 4 + 2; //v3
				triangles[idT++] = (index + 1) * 4 + 2; //v3'
				triangles[idT++] = index * 4 + 3; //v4
				//Bottom
				triangles[idT++] = index * 4 + 3; //v4
				triangles[idT++] = (index + 1) * 4 + 2; //v3'
				triangles[idT++] = (index + 1) * 4 + 3; //v4'

				// //Left
				triangles[idT++] = index * 4 + 2; //v3
				triangles[idT++] = index * 4; //v1
				triangles[idT++] = (index + 1) * 4 + 2; //v3'
				//Left
				triangles[idT++] = index * 4; //v1
				triangles[idT++] = (index + 1) * 4; //v1'
				triangles[idT++] = (index + 1) * 4 + 2; //v3'

				//Right
				triangles[idT++] = index * 4 + 1; //v2
				triangles[idT++] = index * 4 + 3; //v4
				triangles[idT++] = (index + 1) * 4 + 1; //v2'
				//Right
				triangles[idT++] = index * 4 + 3; //v4
				triangles[idT++] = (index + 1) * 4 + 3; //v4'
				triangles[idT++] = (index + 1) * 4 + 1; //v2'

			}
		}

	}


	unsafe static void SetNativeArray(Vector3[] posArray, NativeArray<float3> posNativ) {
		// pin the target array and get a pointer to it
		fixed (void * posArrayPointer = posArray) {
			// memcopy the native array over the top
			UnsafeUtility.MemCpy(posArrayPointer, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(posNativ), posArray.Length * (long) UnsafeUtility.SizeOf<float3>());
		}
	}
	unsafe static void SetNativeArrayTri(int[] arr, NativeArray<int> nativ) {
		// pin the target array and get a pointer to it
		fixed (void * arrPointer = arr) {
			// memcopy the native array over the top
			UnsafeUtility.MemCpy(arrPointer, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(nativ), arr.Length * (long) UnsafeUtility.SizeOf<int>());
		}
	}

	static bool consecutiveResidue(int id1, int id2) {
		int diff = 1;
		if (id1 < 0 && id2 > 0) {
			diff = 2;
		}
		if (id2 - id1 > diff) {
			return false;
		}
		return true;
	}

}
}
