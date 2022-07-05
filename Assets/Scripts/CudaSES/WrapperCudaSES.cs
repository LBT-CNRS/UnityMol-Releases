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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using UMol.API;
using VRTK;

namespace UMol {
public class WrapperCudaSES  {
    // [Range(0.07f, 1.0f)]
    // public float resolutionSES = 0.5f;

    // [Range(0, 5)]
    // public int smoothingSteps = 2;

    // public string filePath = "C:/Users/oculusVR/Desktop/pdbs/1kx2.pdb";


    [DllImport("SESCuda")]
    public static extern void API_computeSES(float resoSES, Vector3 [] atomPos, float [] atomRadii, int N,
            out IntPtr out_vertices, out int NVert, out IntPtr out_triangles , out int NTri, int smooth);

    [DllImport("SESCuda")]
    public static extern IntPtr API_getVertices();

    [DllImport("SESCuda")]
    public static extern IntPtr API_getTriangles(bool invertTri);

    [DllImport("SESCuda")]
    public static extern IntPtr API_getAtomIdPerVert();

    [DllImport("SESCuda")]
    public static extern void API_freeMesh();

    public static GameObject createCUDASESSurface(Transform meshPar, string name, UnityMolSelection select, ref int[] atomIdPerVert, ref MeshData mData,
            float resolutionSES = 0.3f, int smoothingSteps = 1) {

        Vector3[] pos = new Vector3[select.Count];
        float[] radii = new float[select.Count];
        int id = 0;
        int N = select.Count;
        foreach (UnityMolAtom a in select.atoms) {
            pos[id] = a.position;
            radii[id] = a.radius;
            id++;
        }


        IntPtr outVertices = IntPtr.Zero;
        IntPtr outTriangles = IntPtr.Zero;
        IntPtr outAtomIdPerVert = IntPtr.Zero;
        float[] resultVerticesf;
        Vector3[] resultVertices;
        int[] resultTriangles;
        int[] resultAtomIdPerVert;
        int NVert = 0;
        int NTri = 0;

        float timerSES = Time.realtimeSinceStartup;

        API_computeSES(resolutionSES, pos, radii, N, out outVertices, out NVert, out outTriangles, out NTri, smoothingSteps);

        if (NVert != 0 && NTri != 0) {
            outTriangles = API_getTriangles(true);
            outVertices = API_getVertices();
            outAtomIdPerVert = API_getAtomIdPerVert();

            mData = new MeshData();
            resultVerticesf = new float[NVert * 3];
            resultVertices = new Vector3[NVert];
            resultTriangles = new int[NTri];
            resultAtomIdPerVert = new int[NVert];

            // float timerMarshal = Time.realtimeSinceStartup;

            Marshal.Copy(outVertices, resultVerticesf, 0, NVert * 3);
            Marshal.Copy(outTriangles, resultTriangles, 0, NTri);
            Marshal.Copy(outAtomIdPerVert, resultAtomIdPerVert, 0, NVert);

            atomIdPerVert = resultAtomIdPerVert;
    
    // Debug.Log("Time for Marhal: " + (1000.0f * (Time.realtimeSinceStartup - timerMarshal)).ToString("f3") + " ms");

            // float timerPostpro = Time.realtimeSinceStartup;


            for (int i = 0; i < NVert; i++) {
                resultVertices[i] = new Vector3(resultVerticesf[i * 3 + 0],
                                                resultVerticesf[i * 3 + 1],
                                                resultVerticesf[i * 3 + 2]);
            }

            mData.vertices = resultVertices;
            mData.triangles = resultTriangles;
    // Debug.Log("Time for ppost pro: " + (1000.0f * (Time.realtimeSinceStartup - timerPostpro)).ToString("f3") + " ms");

        }
        else {
            Debug.LogError("Failed to compute SES");
            return null;
        }

        float timerSES2 = Time.realtimeSinceStartup;

        // Debug.Log("Time for SES: " + (1000.0f * (timerSES2 - timerSES)).ToString("f3") + " ms");

        //Free mem
        API_freeMesh();

        // Marshal.FreeCoTaskMem(outVertices);
        // Marshal.FreeCoTaskMem(outTriangles);

        //Show Surf

        GameObject newMeshGo = new GameObject(name);
        newMeshGo.transform.parent = meshPar;
        newMeshGo.transform.localPosition = Vector3.zero;
        newMeshGo.transform.localRotation = Quaternion.identity;
        newMeshGo.transform.localScale = Vector3.one;

        Mesh newMesh = new Mesh();
        newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        newMesh.vertices = resultVertices;
        newMesh.triangles = resultTriangles;
        // newMesh.colors32 = mData.colors;

        newMesh.RecalculateNormals();


        MeshFilter mf = newMeshGo.AddComponent<MeshFilter>();
        mf.mesh = newMesh;

        MeshRenderer mr = newMeshGo.AddComponent<MeshRenderer>();

        Material mat = new Material(Shader.Find("Custom/SurfaceVertexColor"));
        mat.SetFloat("_Glossiness", 0.0f);
        mat.SetFloat("_Metallic", 0.0f);
        mat.SetFloat("_AOIntensity", 1.03f);
        mat.SetFloat("_AOPower", 8.0f);

        mr.material = mat;

        return newMeshGo;

    }


    // void Start() {
    //     //Load PDB
    //     UnityMolStructure newStruct = APIPython.load(filePath);
    //     APIPython.hide("cartoon");
    //     APIPython.show("hb");



    //     //Compute surf
    //     List<UnityMolAtom> atoms = newStruct.currentModel.allAtoms;
    //     createCUDASESSurface();

    // }
}
}