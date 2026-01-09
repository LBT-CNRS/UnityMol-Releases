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
/// Wrapper to handle the CUDA SES external library
/// </summary>
public class WrapperCudaSES  {

    [DllImport("SESCuda")]
    private static extern void API_computeSES(float resoSES, Vector3 [] atomPos, float [] atomRadii, int N,
            out IntPtr out_vertices, out int NVert, out IntPtr out_triangles , out int NTri, int smooth);

    [DllImport("SESCuda")]
    private static extern IntPtr API_getVertices();

    [DllImport("SESCuda")]
    private static extern IntPtr API_getTriangles(bool invertTri);

    [DllImport("SESCuda")]
    private static extern IntPtr API_getAtomIdPerVert();

    [DllImport("SESCuda")]
    private static extern void API_freeMesh();

    /// <summary>
    /// Handle & Manage the call to the CUDA SES external function
    /// </summary>
    /// <param name="idF">Frame Id</param>
    /// <param name="select">the selection to compute the SES surface on</param>
    /// <param name="mData"> the MeshData generated from the computation</param>
    /// <param name="resolutionSES">the resolution from the SES algorithm</param>
    /// <param name="smoothingSteps">the smoothing steps from the SES algorithm</param>
    public static void createCUDASESSurface(int idF, UnityMolSelection select, ref MeshData mData,
                                            float resolutionSES = 0.3f, int smoothingSteps = 1) {

        int N = select.atoms.Count;
        Vector3[] pos = new Vector3[N];
        float[] radii = new float[N];
        int id = 0;
        foreach (UnityMolAtom a in select.atoms) {
            pos[id] = a.position;
            if (idF != -1) {
                pos[id] = select.extractTrajFramePositions[idF][id];
            }
            radii[id] = a.radius;
            id++;
        }


        IntPtr outVertices;
        IntPtr outTriangles;
        int NVert;
        int NTri;

        float timerSES = Time.realtimeSinceStartup;

        try {
            API_computeSES(resolutionSES, pos, radii, N, out outVertices, out NVert, out outTriangles, out NTri, smoothingSteps);
        } catch (DllNotFoundException) {
            Debug.LogError("SES CUDA failed: Missing external library.");
            return;
        }

        if (NVert != 0 && NTri != 0) {
            outTriangles = API_getTriangles(true);
            outVertices = API_getVertices();
            IntPtr outAtomIdPerVert = API_getAtomIdPerVert();

            if (mData == null) {
                mData = new MeshData {
                    vertBuffer = new float[NVert * 3],
                    vertices = new Vector3[NVert],
                    triangles = new int[NTri],
                    colors = new Color32[NVert],
                    normals = new Vector3[NVert],
                    atomByVert = new int[NVert]
                };
            }
            else {
                if (NVert > mData.vertices.Length) {//We need more space
                    mData.vertBuffer = new float[NVert * 3];
                    mData.vertices = new Vector3[NVert];
                    mData.colors = new Color32[NVert];
                    mData.normals = new Vector3[NVert];
                    mData.atomByVert = new int[NVert];
                    mData.triangles = new int[NTri];
                }
            }
            mData.nVert = NVert;
            mData.nTri = NTri / 3;

            Marshal.Copy(outVertices, mData.vertBuffer, 0, NVert * 3);
            Marshal.Copy(outTriangles, mData.triangles, 0, NTri);
            Marshal.Copy(outAtomIdPerVert, mData.atomByVert, 0, NVert);

            mData.FillWhite();
            mData.CopyVertBufferToVert();


        }
        else {
            Debug.LogError("Failed to compute SES");
            return;
        }

        float timerSES2 = Time.realtimeSinceStartup;


        Debug.Log("Time for SES: " + (1000.0f * (timerSES2 - timerSES)).ToString("f3") + " ms");

        //Free mem
        API_freeMesh();

    }
}
}
