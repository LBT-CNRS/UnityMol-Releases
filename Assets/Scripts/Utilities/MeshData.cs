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
using System;
using System.Runtime.InteropServices;

public class MeshData {
    public int[] triangles;
    public Vector3[] normals;
    public Vector3[] vertices;
    public Color32[] colors;
    public float[] vertBuffer;
    public int[] colBuffer;
    public int[] atomByVert;

    public int nVert = 0;
    public int nTri = 0;

    public void Scale(Vector3 scale) {
        for (int i = 0; i < nVert; i++) {
            vertices[i] = Vector3.Scale(scale, vertices[i]);
        }
    }
    public void Offset(Vector3 offset) {
        for (int i = 0; i < nVert; i++) {
            vertices[i] += offset;
        }
    }
    public void InvertX() {
        for (int i = 0; i < nVert; i++) {
            vertices[i].x = -vertices[i].x;
        }
    }
    public void InvertTri() {
        for (int i = 0; i < nTri; i++) { //Revert the triangles
            int save = triangles[i * 3];
            triangles[i * 3] = triangles[i * 3 + 1];
            triangles[i * 3 + 1] = save;
        }
    }

    public void CopyVertBufferToVert() {
        GCHandle handleVs = GCHandle.Alloc(vertices, GCHandleType.Pinned);
        IntPtr pV = handleVs.AddrOfPinnedObject();

        Marshal.Copy(vertBuffer, 0, pV, vertBuffer.Length);
    }

    public void FillWhite() {
        Color32 white = Color.white;
        for (int i = 0; i < nVert; i++) {
            colors[i] = white;
        }
    }
}

public struct LInt2 {
    public long x;
    public long y;
}