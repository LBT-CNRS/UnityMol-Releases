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
using System.Collections.Generic;
using Unity.Mathematics;

namespace UMol {
public class MarchingCubesWrapper {
    private MarchingCubesBurst mcb;

    private readonly Dictionary<int, int> cellIdToAtomId;
    private readonly UnityMolStructure curStructure;


	public MarchingCubesWrapper(float[] densityValues, Vector3[] gradient,
	                 int3 sizeGrid, Vector3 ori, Vector3[] deltas,
	                 Vector3[] cellDir, Dictionary<int, int> cellToAtom = null,
	                 UnityMolStructure s = null) {
		cellIdToAtomId = cellToAtom;
		curStructure = s;

		mcb = new MarchingCubesBurst(densityValues, gradient, sizeGrid, ori, deltas, cellDir);
	}

	public MeshData computeMC(float isoValue) {

		MeshData mData = new();

		mcb.computeIsoSurface(isoValue);

		Vector3[] newVerts = mcb.getVertices();
		Vector3[] newNorms = mcb.getNormals();
		//Invert x for vertices and normals
		if (isoValue > 0.0f) {
			for (int i = 0; i < newVerts.Length; i++) {
				// newVerts[i].x *= -1;
				newNorms[i] *= -1;
			}
		}
		int[] newTri = mcb.getTriangles();
		Color32[] newCols = mcb.getColors();

		mData.triangles = newTri;
		mData.vertices = newVerts;
		mData.colors = newCols;
		mData.normals = newNorms;
		mData.nVert = newVerts.Length;
		mData.nTri = newTri.Length;

		if (curStructure != null && cellIdToAtomId != null) {
			int[] vertexToCellId = mcb.getVertexToCellId();
			mData.atomByVert = new int[newVerts.Length];

			for (int i = 0; i < newVerts.Length; i ++) {
				int idVoxel = vertexToCellId[i];
				if (cellIdToAtomId.ContainsKey(idVoxel)) {
					int idAtom = cellIdToAtomId[idVoxel];
					mData.atomByVert[i] = idAtom;
				}
			}
		}

		return mData;
	}

	public void FreeMC() {
        if (mcb == null) {
            return;
        }

        mcb.Clean();
        mcb = null;
    }
}
}
