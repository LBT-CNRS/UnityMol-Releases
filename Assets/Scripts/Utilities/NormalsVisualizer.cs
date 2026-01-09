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

[RequireComponent(typeof(MeshFilter))]
public class NormalsVisualizer : MonoBehaviour {

    private Mesh mesh;
    private Mesh debugMesh;
    private GameObject curGo;

    void Start() {
        curGo = new GameObject("DebugNormals");
        curGo.transform.SetParent(transform);
        MeshRenderer mr = curGo.AddComponent<MeshRenderer>();
        mr.sharedMaterial = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));
        debugMesh = new Mesh();
        debugMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        curGo.AddComponent<MeshFilter>().sharedMesh = debugMesh;

        MeshFilter mf = gameObject.GetComponent<MeshFilter>();
        if (mf != null) {
            mesh = mf.sharedMesh;
            createNormalLines(mesh);
        }
    }

    void createNormalLines(Mesh m) {
        if (debugMesh == null) {
            debugMesh = new Mesh();
            debugMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }
        
        Vector3[] normals = m.normals;
        Vector3[] verts = m.vertices;

        int N = m.vertexCount;
        Vector3[] newVerts = new Vector3[N * 2];
        Color[] newcols = new Color[N * 2];
        int[] newTris = new int[N * 2];

        for (int i = 0; i < m.vertexCount; i++) {

            newVerts[i * 2] = transform.TransformPoint(verts[i]);
            newVerts[i * 2 + 1] = transform.TransformPoint(verts[i]) + transform.TransformVector(normals[i]);

            newTris[i * 2] = i * 2;
            newTris[i * 2 + 1] = i * 2 + 1;

            Color col = Color.white;
            if (i % 3 == 1) {
                col = Color.yellow;
            }
            if (i % 3 == 2)
                col = Color.blue;

            newcols[i * 2] = col;
            newcols[i * 2 + 1] = col;

        }
        debugMesh.SetVertices(newVerts);
        debugMesh.SetColors(newcols);
        debugMesh.SetIndices(newTris, MeshTopology.Lines, 0);
    }
    void OnDestroy() {
        if (debugMesh != null) {
            GameObject.Destroy(curGo.GetComponent<MeshRenderer>().sharedMaterial);
            GameObject.Destroy(debugMesh);
        }
    }
}