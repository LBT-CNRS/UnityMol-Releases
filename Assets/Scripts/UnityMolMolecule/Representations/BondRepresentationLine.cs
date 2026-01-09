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
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;


namespace UMol {
public class BondRepresentationLine : BondRepresentation {

    public GameObject meshGO;
    public Mesh curMesh;
    public Dictionary<UnityMolAtom, List<int>> atomToMeshVertex;
    public float lineWidth = 0.15f;
    float prevlineWidth = 0.15f;
    public NativeArray<float3> vertices;
    public NativeArray<float3> normals;
    public NativeArray<float4> atomPosRad;
    public Vector2[] uvs;
    public Color32[] colors;
    public HashSet<int2> displayedBonds;
    public Material lineMat;
    public bool fast = false;//Only 4 vertices per atom when on vs a real cylinder+hemisphere when off
    private int circleSteps = 8;
    private NativeArray<float3> icoSphereVert;
    private NativeArray<float3> icoSphereNorm;
    private int[] icoSphereTri;
    private NativeArray<float> preCompX;
    private NativeArray<float> preCompY;
    public int lastLineVert = 0;

    private NativeArray<int2> nativeDisplayedBonds;
    private NativeArray<int> atomIdsInSel;

    public BondRepresentationLine(int idF, string structName, UnityMolSelection sel) {
        colorationType = colorType.atom;

        representationParent = UnityMolMain.getRepStructureParent(structName).transform;

        GameObject newRep = new GameObject("BondLineRepresentation");
        newRep.transform.parent = representationParent;
        representationTransform = newRep.transform;

        selection = sel;
        idFrame = idF;

        if (selection.Count > 10000)
            fast = true;

        DisplayLine(newRep.transform);
        // newRep.transform.position -= offset;

        newRep.transform.localPosition = Vector3.zero;
        newRep.transform.localRotation = Quaternion.identity;
        newRep.transform.localScale = Vector3.one;

        nbBonds = selection.bonds.Count;

    }

    private void DisplayLine(Transform repParent) {

        int nbSticks = selection.bonds.Count;
        displayedBonds = new HashSet<int2>();
        if (nbSticks == 0)
            return;

        GameObject currentGO = null;
        // if (!recompute) {
        atomToMeshVertex = new Dictionary<UnityMolAtom, List<int>>();
        currentGO = new GameObject("BondLineMesh");
        currentGO.transform.parent = repParent;
        curMesh = new Mesh();
        curMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        // }
        // else {
        //     currentGO = meshGO;
        // }

        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();
        List<Color32> newColors = new List<Color32>();
        List<Vector2> newUV = new List<Vector2>();
        List<Vector3> newNormals = new List<Vector3>();
        HashSet<UnityMolAtom> doneAtoms = new HashSet<UnityMolAtom>();

        UnityMolModel curM = selection.atoms[0].residue.chain.model;
        if (!fast && (preCompX == null || preCompX.Length != circleSteps)) {
            if (preCompX.IsCreated) {
                preCompX.Dispose();
                preCompY.Dispose();
            }
            preCompX = new NativeArray<float>(circleSteps, Allocator.Persistent);
            preCompY = new NativeArray<float>(circleSteps, Allocator.Persistent);
        }


        if (!fast) {
            float twopi = 2 * Mathf.PI;
            float step = twopi / circleSteps;
            for (int i = 0; i < circleSteps; i++) {
                float a = i * step;
                float xx = Mathf.Cos(a);
                float yy = Mathf.Sin(a);
                preCompX[i] = xx * lineWidth * 0.49f; // 0.49 -> tweak to minimize the mismatch width between the sphere and the tube
                preCompY[i] = yy * lineWidth * 0.49f; // 0.49 -> tweak to minimize the mismatch width between the sphere and the tube
            }

            if (icoSphereVert == null || !icoSphereVert.IsCreated) {
                Mesh tmpm = IcoSphereCreator.Create(4, 1.0f);
                tmpm.RecalculateNormals(60.0f);
                if(icoSphereVert.IsCreated){
                    icoSphereVert.Dispose();
                    icoSphereNorm.Dispose();
                }
                icoSphereVert = new NativeArray<float3>(tmpm.vertexCount, Allocator.Persistent);
                icoSphereNorm = new NativeArray<float3>(tmpm.vertexCount, Allocator.Persistent);
                icoSphereTri = new int[tmpm.triangles.Length];

                Vector3[] tmpmVerts = tmpm.vertices;
                Vector3[] tmpNorms = tmpm.normals;
                for (int i = 0; i < tmpm.vertexCount; i++) {
                    icoSphereVert[i] = tmpmVerts[i];
                    icoSphereNorm[i] = tmpNorms[i];
                }

                tmpm.triangles.CopyTo(icoSphereTri, 0);
                GameObject.Destroy(tmpm);
            }
        }


        int2 k, invk;
        int idV = 0;
        int idT = 0;
        foreach (int ida in selection.bonds.bonds.Keys) {
            UnityMolAtom atom1 = curM.allAtoms[ida];
            foreach (int idb in selection.bonds.bonds[ida]) {
                if (idb != -1) {
                    k.x = ida; invk.x = idb;
                    k.y = idb; invk.y = ida;
                    if (displayedBonds.Contains(k) || displayedBonds.Contains(invk))
                        continue;

                    displayedBonds.Add(k);
                    UnityMolAtom atom2 = curM.allAtoms[idb];

                    doneAtoms.Add(atom1);
                    doneAtoms.Add(atom2);

                    Vector3 start = atom1.position;
                    Vector3 end   = atom2.position;
                    if (idFrame != -1) {
                        int iida = selection.atomToIdInSel[atom1];
                        start = selection.extractTrajFramePositions[idFrame][iida];
                        iida = selection.atomToIdInSel[atom2];
                        end = selection.extractTrajFramePositions[idFrame][iida];
                    }

                    Vector3 normal = Vector3.Cross(start, end);
                    Vector3 side = Vector3.Cross(normal, end - start);
                    side.Normalize();
                    normal.Normalize();

                    if (fast) {

                        Vector3 a = start + side * (lineWidth / 2);
                        Vector3 b = start - side * (lineWidth / 2);
                        Vector3 c = end + side * (lineWidth / 2);
                        Vector3 d = end - side * (lineWidth / 2);

                        Vector3 a1 = a + normal * (lineWidth / 2);
                        Vector3 a2 = a - normal * (lineWidth / 2);
                        Vector3 b1 = b + normal * (lineWidth / 2);
                        Vector3 b2 = b - normal * (lineWidth / 2);
                        Vector3 c1 = c + normal * (lineWidth / 2);
                        Vector3 c2 = c - normal * (lineWidth / 2);
                        Vector3 d1 = d + normal * (lineWidth / 2);
                        Vector3 d2 = d - normal * (lineWidth / 2);

                        int idva = idV;
                        // if (recompute) {
                        //     vertices[idva] = a1;
                        //     vertices[idva + 1] = a2;
                        //     vertices[idva + 2] = b1;
                        //     vertices[idva + 3] = b2;
                        //     vertices[idva + 4] = c1;
                        //     vertices[idva + 5] = c2;
                        //     vertices[idva + 6] = d1;
                        //     vertices[idva + 7] = d2;

                        //     idT += 36;

                        // }
                        // else {
                        newVertices.Add(a1);
                        newVertices.Add(a2);
                        newVertices.Add(b1);
                        newVertices.Add(b2);
                        newVertices.Add(c1);
                        newVertices.Add(c2);
                        newVertices.Add(d1);
                        newVertices.Add(d2);

                        newTriangles.Add(idva);//a1
                        newTriangles.Add(idva + 1); //a2
                        newTriangles.Add(idva + 2); //b1

                        newTriangles.Add(idva + 2); //b1
                        newTriangles.Add(idva + 1); //a2
                        newTriangles.Add(idva + 3); //b2

                        newTriangles.Add(idva);//a1
                        newTriangles.Add(idva + 4); //c1
                        newTriangles.Add(idva + 1); //a2

                        newTriangles.Add(idva + 4); //c1
                        newTriangles.Add(idva + 5); //c2
                        newTriangles.Add(idva + 1); //a2

                        newTriangles.Add(idva + 6); //d1
                        newTriangles.Add(idva + 4); //c1
                        newTriangles.Add(idva + 2); //b1

                        newTriangles.Add(idva);//a1
                        newTriangles.Add(idva + 2); //b1
                        newTriangles.Add(idva + 4); //c1


                        newTriangles.Add(idva + 2); //b1
                        newTriangles.Add(idva + 3); //b2
                        newTriangles.Add(idva + 6); //d1

                        newTriangles.Add(idva + 6); //d1
                        newTriangles.Add(idva + 3); //b2
                        newTriangles.Add(idva + 7); //d2

                        newTriangles.Add(idva + 7); //d2
                        newTriangles.Add(idva + 1); //a2
                        newTriangles.Add(idva + 5); //c2

                        newTriangles.Add(idva + 1); //a2
                        newTriangles.Add(idva + 7); //d2
                        newTriangles.Add(idva + 3); //b2

                        newTriangles.Add(idva + 4); //c1
                        newTriangles.Add(idva + 6); //d1
                        newTriangles.Add(idva + 7); //d2

                        newTriangles.Add(idva + 7); //d2
                        newTriangles.Add(idva + 5); //c2
                        newTriangles.Add(idva + 4); //c1

                        newColors.Add(atom1.color32);//0
                        newColors.Add(atom1.color32);//1
                        newColors.Add(atom1.color32);//2
                        newColors.Add(atom1.color32);//3

                        newColors.Add(atom2.color32);//4
                        newColors.Add(atom2.color32);//5
                        newColors.Add(atom2.color32);//6
                        newColors.Add(atom2.color32);//7

                        newUV.Add(Vector2.one);
                        newUV.Add(Vector2.one);
                        newUV.Add(Vector2.one);
                        newUV.Add(Vector2.one);

                        newUV.Add(Vector2.one);
                        newUV.Add(Vector2.one);
                        newUV.Add(Vector2.one);
                        newUV.Add(Vector2.one);



                        if (!atomToMeshVertex.ContainsKey(atom1)) {
                            atomToMeshVertex[atom1] = new List<int>();
                        }
                        atomToMeshVertex[atom1].Add(idva);
                        atomToMeshVertex[atom1].Add(idva + 1);
                        atomToMeshVertex[atom1].Add(idva + 2);
                        atomToMeshVertex[atom1].Add(idva + 3);

                        if (!atomToMeshVertex.ContainsKey(atom2)) {
                            atomToMeshVertex[atom2] = new List<int>();
                        }
                        atomToMeshVertex[atom2].Add(idva + 4);
                        atomToMeshVertex[atom2].Add(idva + 5);
                        atomToMeshVertex[atom2].Add(idva + 6);
                        atomToMeshVertex[atom2].Add(idva + 7);
                        // }
                        idV += 8;
                    }
                    else {
                        Vector3 mainv = (end - start).normalized;
                        Quaternion rot = Quaternion.FromToRotation(Vector3.forward, mainv);

                        int sv = idV;
                        for (int i = 0; i < circleSteps; i++) {
                            float x = preCompX[i];
                            float y = preCompY[i];
                            Vector3 p1 = new Vector3(x, y, 0.0f);

                            p1 = (rot * p1) + start;

                            newVertices.Add(p1);
                            newColors.Add(atom1.color32);
                            newUV.Add(Vector2.one);
                            newNormals.Add((p1 - start).normalized);

                            if (!atomToMeshVertex.ContainsKey(atom1)) {
                                atomToMeshVertex[atom1] = new List<int>();
                            }
                            atomToMeshVertex[atom1].Add(idV);

                            idV++;
                        }

                        for (int i = 0; i < circleSteps; i++) {
                            float x = preCompX[i];
                            float y = preCompY[i];
                            Vector3 p2 = new Vector3(x, y, 0.0f);
                            p2 = (rot * p2) + end;

                            newVertices.Add(p2);
                            newColors.Add(atom2.color32);
                            newUV.Add(Vector2.one);
                            newNormals.Add((p2 - end).normalized);

                            if (!atomToMeshVertex.ContainsKey(atom2)) {
                                atomToMeshVertex[atom2] = new List<int>();
                            }
                            atomToMeshVertex[atom2].Add(idV);

                            if (i != circleSteps - 1) {
                                newTriangles.Add(sv + i);
                                newTriangles.Add(sv + i + 1);
                                newTriangles.Add(sv + i + circleSteps);

                                newTriangles.Add(sv + i + 1);
                                newTriangles.Add(sv + i + circleSteps + 1);
                                newTriangles.Add(sv + i + circleSteps);
                            }
                            else {

                                newTriangles.Add(sv);
                                newTriangles.Add(sv + circleSteps);
                                newTriangles.Add(sv + circleSteps - 1);

                                newTriangles.Add(sv + circleSteps - 1);
                                newTriangles.Add(sv + circleSteps);
                                newTriangles.Add(sv + circleSteps + circleSteps - 1);
                            }
                            // }

                            idT += 6;
                            idV++;
                        }
                    }
                }
            }
        }

        lastLineVert = idV;
        if (!fast) {
            //Add spheres
            foreach (UnityMolAtom a in doneAtoms) {
                int startv = idV;
                Vector3 start = a.position;

                if (idFrame != -1) {
                    int iida = selection.atomToIdInSel[a];
                    start = selection.extractTrajFramePositions[idFrame][iida];
                }

                for (int v = 0; v < icoSphereVert.Length; v++)
                {

                    newVertices.Add(((Vector3)icoSphereVert[v]) * lineWidth * 0.48f + start);  // 0.48 -> tweak to minimize the mismatch width between the sphere and the tube
                    newNormals.Add(icoSphereNorm[v]);
                    newColors.Add(a.color32);
                    newUV.Add(Vector2.one);
                    atomToMeshVertex[a].Add(idV);
                    idV++;
                }
                for (int t = 0; t < icoSphereTri.Length; t++)
                {
                    newTriangles.Add(startv + icoSphereTri[t]);
                    idT++;
                }
                // }
            }
        }

        if (vertices.IsCreated)
            vertices.Dispose();
        vertices = new NativeArray<float3>(newVertices.Count, Allocator.Persistent);
        for (int i = 0; i < newVertices.Count; i++) {
            vertices[i] = newVertices[i];
        }
        colors = newColors.ToArray();
        uvs = newUV.ToArray();
        if (!fast) {
            if (normals.IsCreated)
                normals.Dispose();

            normals = new NativeArray<float3>(newNormals.Count, Allocator.Persistent);
            for (int i = 0; i < newNormals.Count; i++) {
                normals[i] = newNormals[i];
            }
        }

        curMesh.SetVertices(vertices);
        curMesh.uv2 = uvs;
        curMesh.colors32 = colors;

        curMesh.SetTriangles(newTriangles, 0);


        if (fast) {
            curMesh.RecalculateNormals();
        }
        else {
            curMesh.SetNormals(normals);
        }
        curMesh.RecalculateBounds();


        if (lineMat == null) {
            if (fast)
                lineMat = new Material(Shader.Find("Unlit/SurfaceVertexColorNotCull"));
            else
                lineMat = new Material(Shader.Find("Custom/SurfaceVertexColor"));
        }

        // if (!recompute) {
        MeshFilter mf = currentGO.AddComponent<MeshFilter>();
        mf.sharedMesh = curMesh;
        MeshRenderer mr = currentGO.AddComponent<MeshRenderer>();

        mr.sharedMaterial = lineMat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        // }
        // else {
        //     currentGO.GetComponent<MeshFilter>().sharedMesh = curMesh;
        // }

        meshGO = currentGO;

        if (atomPosRad.IsCreated)
            atomPosRad.Dispose();
        if (atomIdsInSel.IsCreated)
            atomIdsInSel.Dispose();
        if (nativeDisplayedBonds.IsCreated)
            nativeDisplayedBonds.Dispose();
        atomPosRad = new NativeArray<float4>(curM.Count, Allocator.Persistent);
        atomIdsInSel = new NativeArray<int>(doneAtoms.Count, Allocator.Persistent);
        nativeDisplayedBonds = new NativeArray<int2>(displayedBonds.Count, Allocator.Persistent);
        // }
        int id = 0;
        foreach (int2 c in displayedBonds) {
            nativeDisplayedBonds[id++] = c;
        }


        int tid = 0;
        foreach (UnityMolAtom a in doneAtoms) {
            atomIdsInSel[tid] = a.idInAllAtoms;
            tid++;
        }
    }

    public void recompute() {

        Transform savedPar = null;
        if (meshGO != null) {
            savedPar = meshGO.transform.parent;
        }

        fillPositionRad();

        if (prevlineWidth != lineWidth) {
            if (!fast) {
                if (preCompX == null || preCompX.Length != circleSteps) {
                    if (preCompX.IsCreated) {
                        preCompX.Dispose();
                        preCompY.Dispose();
                    }
                    preCompX = new NativeArray<float>(circleSteps, Allocator.Persistent);
                    preCompY = new NativeArray<float>(circleSteps, Allocator.Persistent);
                }

                float twopi = 2 * Mathf.PI;
                float step = twopi / circleSteps;
                for (int i = 0; i < circleSteps; i++) {
                    float a = i * step;
                    float xx = Mathf.Cos(a);
                    float yy = Mathf.Sin(a);
                    preCompX[i] = xx * lineWidth * 0.49f;  // 0.49 -> tweak to minimize the mismatch width between the sphere and the tube
                    preCompY[i] = yy * lineWidth * 0.49f;  // 0.49 -> tweak to minimize the mismatch width between the sphere and the tube
                }
                prevlineWidth = lineWidth;
            }
        }

        recomputeBurst();
    }

    public override void Clean() {
        if (meshGO != null) {
            GameObject.Destroy(meshGO.GetComponent<MeshFilter>().sharedMesh);
            GameObject.Destroy(meshGO);
        }
        if (lineMat != null)
            GameObject.Destroy(lineMat);
        lineMat = null;

        if (atomToMeshVertex != null)
            atomToMeshVertex.Clear();
        atomToMeshVertex = null;
        colors = null;
        uvs = null;
        if (displayedBonds != null)
            displayedBonds.Clear();
        displayedBonds = null;
        if (nativeDisplayedBonds.IsCreated)
            nativeDisplayedBonds.Dispose();
        if (vertices.IsCreated)
            vertices.Dispose();
        if (normals.IsCreated)
            normals.Dispose();
        if (atomPosRad.IsCreated)
            atomPosRad.Dispose();
        if (icoSphereVert.IsCreated)
            icoSphereVert.Dispose();
        if (icoSphereNorm.IsCreated)
            icoSphereNorm.Dispose();
        if (atomIdsInSel.IsCreated)
            atomIdsInSel.Dispose();
        if (preCompX.IsCreated)
            preCompX.Dispose();
        if (preCompY.IsCreated)
            preCompY.Dispose();
    }

    void fillPositionRad() {
        int i = 0;
        UnityMolModel curM = selection.atoms[0].residue.chain.model;

        foreach (UnityMolAtom a in curM.allAtoms) {
            Vector3 pos = a.position;

            if (idFrame != -1) {
                int iida = selection.atomToIdInSel[a];
                pos = selection.extractTrajFramePositions[idFrame][iida];
            }

            float4 tmp;
            tmp.x = pos.x;
            tmp.y = pos.y;
            tmp.z = pos.z;
            tmp.w = a.radius;
            atomPosRad[i] = tmp;
            i++;
        }
    }


    public void recomputeBurst() {

        if (fast) {
            var lineJobf = new LineJobFast() {
                bonds = nativeDisplayedBonds,
                verts = vertices,
                lw = lineWidth,
                posrad = atomPosRad,
            };
            var lineJobHandlef = lineJobf.Schedule(displayedBonds.Count, 128);
            lineJobHandlef.Complete();
        }
        else {
            var lineJob = new LineJob() {
                bonds = nativeDisplayedBonds,
                verts = vertices,
                norms = normals,
                lw = lineWidth,
                posrad = atomPosRad,
                circleSteps = circleSteps,
                preCompX = preCompX,
                preCompY = preCompY,
            };
            lastLineVert = nativeDisplayedBonds.Length * circleSteps * 2;


            var sphereJob = new SphereJob() {
                ids = atomIdsInSel,
                posrad = atomPosRad,
                verts = vertices,
                norms = normals,
                lw = lineWidth,
                lastLineVert = lastLineVert,
                icoSphereVert = icoSphereVert,
                icoSphereNorm = icoSphereNorm
            };
            var lineJobHandle = lineJob.Schedule(displayedBonds.Count, 128);
            lineJobHandle.Complete();
            var sphereJobHandle = sphereJob.Schedule(atomIdsInSel.Length, 128);
            sphereJobHandle.Complete();
        }


        curMesh.SetVertices(vertices);
        if (!fast)
            curMesh.SetNormals(normals);
        else
            curMesh.RecalculateNormals();
    }

    [BurstCompile]
    struct LineJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<int2> bonds;
        [ReadOnly] public NativeArray<float4> posrad;
        [ReadOnly] public float lw;
        [ReadOnly] public int circleSteps;
        [ReadOnly] public NativeArray<float> preCompX;
        [ReadOnly] public NativeArray<float> preCompY;

        [NativeDisableParallelForRestriction]
        public NativeArray<float3> verts;
        [NativeDisableParallelForRestriction]
        public NativeArray<float3> norms;

        void IJobParallelFor.Execute(int index)
        {
            int2 ids = bonds[index];
            float4 pr1 = posrad[ids.x];
            float4 pr2 = posrad[ids.y];

            float3 start = pr1.xyz;
            float3 end = pr2.xyz;


            float3 normal = math.normalize(math.cross(start, end));
            float3 side = math.normalize(math.cross(normal, end - start));

            float3 mainv = math.normalize(end - start);
            //TODO: change that to a unity.mathematics function
            quaternion rot = Quaternion.FromToRotation(Vector3.forward, mainv);

            int sv = index * circleSteps * 2;

            for (int i = 0; i < circleSteps; i++) {
                float x = preCompX[i];
                float y = preCompY[i];
                float3 p1 = new float3(x, y, 0.0f);

                p1 = math.mul(rot, p1) + start;

                verts[sv] = p1;
                norms[sv] = math.normalize(p1 - start);

                sv++;
            }

            for (int i = 0; i < circleSteps; i++) {
                float x = preCompX[i];
                float y = preCompY[i];
                float3 p2 = new float3(x, y, 0.0f);
                p2 = math.mul(rot, p2) + end;

                verts[sv] = p2;
                norms[sv] = math.normalize(p2 - end);

                sv++;
            }
        }
    }
    [BurstCompile]
    struct LineJobFast : IJobParallelFor
    {
        [ReadOnly] public NativeArray<int2> bonds;
        [ReadOnly] public NativeArray<float4> posrad;
        [ReadOnly] public float lw;

        [NativeDisableParallelForRestriction]
        public NativeArray<float3> verts;

        void IJobParallelFor.Execute(int index)
        {
            int2 ids = bonds[index];
            float4 pr1 = posrad[ids.x];
            float4 pr2 = posrad[ids.y];

            float3 start = pr1.xyz;
            float3 end = pr2.xyz;


            float3 normal = math.normalize(math.cross(start, end));
            float3 side = math.normalize(math.cross(normal, end - start));

            float3 mainv = math.normalize(end - start);

            int sv = index * 8;
            float halflw = lw / 2;

            float3 a = start + side * halflw;
            float3 b = start - side * halflw;
            float3 c = end + side * halflw;
            float3 d = end - side * halflw;

            float3 a1 = a + normal * halflw;
            float3 a2 = a - normal * halflw;
            float3 b1 = b + normal * halflw;
            float3 b2 = b - normal * halflw;
            float3 c1 = c + normal * halflw;
            float3 c2 = c - normal * halflw;
            float3 d1 = d + normal * halflw;
            float3 d2 = d - normal * halflw;

            verts[sv] = a1;
            verts[sv + 1] = a2;
            verts[sv + 2] = b1;
            verts[sv + 3] = b2;
            verts[sv + 4] = c1;
            verts[sv + 5] = c2;
            verts[sv + 6] = d1;
            verts[sv + 7] = d2;

        }
    }

    [BurstCompile]
    struct SphereJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<int> ids;
        [ReadOnly] public NativeArray<float4> posrad;
        [ReadOnly] public float lw;
        [ReadOnly] public int lastLineVert;

        [ReadOnly] public NativeArray<float3> icoSphereVert;
        [ReadOnly] public NativeArray<float3> icoSphereNorm;


        [NativeDisableParallelForRestriction]
        public NativeArray<float3> verts;
        [NativeDisableParallelForRestriction]
        public NativeArray<float3> norms;

        void IJobParallelFor.Execute(int index) {
            if (ids[index] == -1)
                return;
            float4 pr = posrad[ids[index]];
            int idV = lastLineVert + index * icoSphereVert.Length;

            for (int v = 0; v < icoSphereVert.Length; v++) {

                verts[idV] = icoSphereVert[v] * lw * 0.48f + pr.xyz;  // 0.48 -> tweak to minimize the mismatch width between the sphere and the tube
                norms[idV] = icoSphereNorm[v];

                idV++;
            }
        }
    }
}
}
