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
using System.Runtime.InteropServices;
using System;

namespace UMol {

/// <summary>
/// Wrapper to handle the CUDA QuickSurf external library
/// </summary>
public static class WrapperCudaQuickSurf  {
    [DllImport("QuickSurf")]
    private static extern void API_computeQS(Vector3 [] atomPos, float [] atomRadii, int N,
                                            out int NVert, out int NTri, float rscale, float gspace, float glim, float iso);

    [DllImport("QuickSurf")]
    private static extern IntPtr API_getVertices();

    [DllImport("QuickSurf")]
    private static extern IntPtr API_getNormals();

    [DllImport("QuickSurf")]
    private static extern IntPtr API_getTriangles(bool invertTri);

    [DllImport("QuickSurf")]
    private static extern IntPtr API_getAtomIdPerVert();

    [DllImport("QuickSurf")]
    private static extern void API_freeMesh();


    [ThreadStatic] //Ensure thread safe
    private static Vector3[] posBuffer;
    [ThreadStatic] //Ensure thread safe
    private static float[] radBuffer;

    /// <summary>
    /// Handle & Manage the call to the CUDA QuickSurf external function
    /// </summary>
    /// <param name="idF">Frame Id</param>
    /// <param name="select">the selection to compute the SES surface on</param>
    /// <param name="mData"> the MeshData generated from the computation</param>
    /// <param name="rscale">the r-scale from the QuickSurf algorithm</param>
    /// <param name="gspace">the g-space from the QuickSurf algorithm</param>
    /// <param name="glim">the g-lim from the QuickSurf algorithm</param>
    /// <param name="iso">the iso from the QuickSurf algorithm</param>
    public static void createSurface(int idF, UnityMolSelection select, ref MeshData mData,
                                     float rscale = 1.0f, float gspace = 1.0f, float glim = 1.5f, float iso = 0.5f) {

        int N = select.atoms.Count;

        if (posBuffer == null || N > posBuffer.Length) {
            posBuffer = new Vector3[N];
            radBuffer = new float[N];
        }


        int id = 0;
        foreach (UnityMolAtom a in select.atoms) {
            posBuffer[id] = a.position;
            if (idF != -1) {
                posBuffer[id] = select.extractTrajFramePositions[idF][id];
            }
            radBuffer[id] = a.radius;
            id++;
        }

        IntPtr outVertices = IntPtr.Zero;
        IntPtr outTriangles = IntPtr.Zero;
        IntPtr outNormals = IntPtr.Zero;
        IntPtr outAtomIdPerVert = IntPtr.Zero;
        int NVert;
        int NTri;

        try {
            API_computeQS(posBuffer, radBuffer, N, out NVert, out NTri, rscale, gspace, glim, iso);
        } catch (DllNotFoundException) {
            Debug.LogError("CUDA QuickSurf failed: Missing external library.");
            return;
        }

        if (NVert != 0 && NTri != 0) {
            outTriangles = API_getTriangles(false);
            outVertices = API_getVertices();
            outNormals = API_getNormals();
            outAtomIdPerVert = API_getAtomIdPerVert();

            if (mData == null) {
                mData = new MeshData {
                    vertices = new Vector3[NVert],
                    triangles = new int[NTri],
                    colors = new Color32[NVert],
                    normals = new Vector3[NVert],
                    atomByVert = new int[NVert],
                    vertBuffer = new float[NVert * 3],
                    nVert = NVert,
                    nTri = NTri / 3
                };
            }
            else {
                if (NVert > mData.vertices.Length) {//We need more space
                    mData.vertices = new Vector3[NVert];
                    mData.colors = new Color32[NVert];
                    mData.normals = new Vector3[NVert];
                    mData.atomByVert = new int[NVert];
                    mData.vertBuffer = new float[NVert * 3];
                    mData.nVert = NVert;
                }

                if (NTri > mData.triangles.Length) {//We need more space
                    mData.triangles = new int[NTri];
                }

                mData.nTri = NTri / 3;
                mData.nVert = NVert;
            }

            Marshal.Copy(outVertices, mData.vertBuffer, 0, NVert * 3);
            Marshal.Copy(outTriangles, mData.triangles, 0, NTri);
            Marshal.Copy(outAtomIdPerVert, mData.atomByVert, 0, NVert);


            mData.FillWhite();
            mData.CopyVertBufferToVert();

            Marshal.Copy(outNormals, mData.vertBuffer, 0, NVert * 3);

            for (int i = 0; i < NVert; i++) {
                mData.normals[i] = new Vector3(mData.vertBuffer[i * 3 + 0],
                                               mData.vertBuffer[i * 3 + 1],
                                               mData.vertBuffer[i * 3 + 2]);
            }

        }
        else {
            Debug.LogError("Failed to compute QuickSurf");
            return;
        }

        //Free mem
        API_freeMesh();

    }
}
}
