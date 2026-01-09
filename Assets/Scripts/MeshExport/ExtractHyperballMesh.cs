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
using System.Linq;

using UMol.API;

namespace UMol {

public class ExtractHyperballMesh {

    static Vector3[] sphereVert = null;
    static Vector3[] sphereNorms = null;
    static int[] sphereTri = null;
    public static Material extHBMat = null;

    public static List<GameObject> getAllHBForStructure(UnityMolStructure s, bool addRenderer = false) {

        List<GameObject> result = new List<GameObject>();
        foreach (UnityMolRepresentation r in s.representations) {
            if (r.repType.atomType ==  AtomType.optihb &&
                    r.repType.bondType == BondType.optihs) {
                foreach (SubRepresentation sr in r.subReps) {
                    if (sr.atomRepManager != null && sr.bondRepManager != null) {
                        List<UnityMolAtom> atoms = sr.atomRep.selection.atoms;
                        UnityMolBonds bonds = sr.bondRep.selection.bonds;
                        UnityMolHBallMeshManager hbmm = (UnityMolHBallMeshManager)sr.atomRepManager;
                        UnityMolHStickMeshManager hsmm = (UnityMolHStickMeshManager)sr.bondRepManager;
                        Mesh m = null;
                        try {
                            m = computeHBMesh(atoms, bonds, hsmm.shrink, hbmm.lastScale, hsmm.scaleBond, hbmm);
                        }
                        catch {
                            Debug.LogWarning("Ignoring a subrepresentation, hyperball extraction failed");
                            continue;
                        }

                        GameObject expHB = new GameObject("HyperballExportedMesh");
                        expHB.transform.parent = sr.bondRep.representationTransform;
                        expHB.transform.localScale = Vector3.one;
                        expHB.transform.localPosition = Vector3.zero;
                        expHB.transform.localRotation = Quaternion.identity;
                        MeshFilter mf = expHB.AddComponent<MeshFilter>();

                        if (addRenderer) {
                            var mr = expHB.AddComponent<MeshRenderer>();
                            if (extHBMat == null)
                                extHBMat = new Material(Shader.Find("Diffuse"));
                            mr.material = extHBMat;
                        }

                        mf.sharedMesh = m;

                        result.Add(expHB);
                    }
                }
            }
        }
        return result;
    }


    public static Mesh computeHBMesh(List<UnityMolAtom> atoms,
                                     UnityMolBonds curB, float shrink,
                                     float scaleAtoms, float scaleBonds, UnityMolHBallMeshManager hbmm,
                                     float step = 0.2f, float circleStep = 0.1f) {

        Mesh newMesh;
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Color> colors = new List<Color>();

        List<float> preCompX = new List<float>();
        List<float> preCompY = new List<float>();

        for (float i = 0.0f; i <= 2 * Mathf.PI; i += circleStep) {
            float xx = Mathf.Cos(i);
            float yy = Mathf.Sin(i);
            preCompX.Add(xx);
            preCompY.Add(yy);
        }


        if (sphereVert == null) {
            GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Mesh sphereMesh = tmp.GetComponent<MeshFilter>().mesh;
            sphereVert = sphereMesh.vertices;
            sphereNorms = sphereMesh.normals;
            sphereTri = sphereMesh.triangles;
            GameObject.Destroy(tmp);
        }


        newMesh = new Mesh();
        newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;


        UnityMolBonds bonds = curB;
        UnityMolModel curModel = atoms[0].residue.chain.model;

        foreach (UnityMolAtom a in atoms) {
            AddSphere(a.position, scaleAtoms * a.radius / 3, hbmm.getColorAtom(a),
                      ref vertices,
                      ref normals,
                      ref triangles,
                      ref colors);
        }

        if (shrink < 0.999f) {

            int cpt = 0;
            foreach (int ida in bonds.bonds.Keys) {
                int[] bonded = bonds.bonds[ida];
                UnityMolAtom a = curModel.allAtoms[ida];
                foreach (int idb in bonded) {
                    if (idb != -1) {
                        UnityMolAtom b = curModel.allAtoms[idb];

                        ExportHBMesh(a.position, b.position,
                                     scaleAtoms * a.radius / 3,
                                     scaleAtoms * b.radius / 3,
                                     hbmm.getColorAtom(a), hbmm.getColorAtom(b),
                                     scaleBonds, shrink, step,
                                     ref vertices,
                                     ref normals,
                                     ref triangles,
                                     ref colors,
                                     preCompX,
                                     preCompY);
                        cpt++;

                    }
                }
            }
        }

        newMesh.SetVertices(vertices);
        newMesh.SetTriangles(triangles, 0);
        newMesh.SetColors(colors);
        newMesh.SetNormals(normals);

        if(shrink < 0.999f)
            newMesh.RecalculateNormals();

        return newMesh;
    }

    static void AddSphere(Vector3 posA1, float radA1, Color colA1,
                          ref List<Vector3> vertices,
                          ref List<Vector3> normals,
                          ref List<int> triangles,
                          ref List<Color> colors) {
        int tmp = vertices.Count;
        for (int i = 0; i < sphereVert.Length; i++) {
            vertices.Add(sphereVert[i] * radA1 * 2 + posA1);
            normals.Add(sphereNorms[i]);
            colors.Add(colA1);
        }
        for (int i = 0; i < sphereTri.Length; i++) {
            triangles.Add(tmp + sphereTri[i]);
        }
    }
    static void ExportHBMesh(Vector3 posA1, Vector3 posA2, float radA1, float radA2,
                             Color colA1, Color colA2, float scaleBonds, float shrink, float step,
                             ref List<Vector3> vertices,
                             ref List<Vector3> normals,
                             ref List<int> triangles,
                             ref List<Color> colors,
                             List<float> preCompX,
                             List<float> preCompY) {
        float rA1 = radA1;
        float rA2 =  radA2;
        float wa1 = (rA1 * rA1) / shrink;
        float wa2 = (rA2 * rA2) / shrink;

        float atomDist = Vector3.Distance(posA1, posA2);
        Vector3 tPosA1 = Vector3.zero;
        Vector3 tPosA2 = new Vector3(atomDist, 0.0f, 0.0f);

        float focusF = (tPosA1.sqrMagnitude  - tPosA2.sqrMagnitude + wa2 - wa1) / (2 * (tPosA1 - tPosA2).sqrMagnitude);

        Vector3 focus = (tPosA1 - tPosA2) * focusF;

        float R2 = wa1 - (tPosA1 - focus).sqrMagnitude;

        Vector3 c1 = tPosA1 - (shrink * (tPosA1 - focus));
        Vector3 c2 = tPosA2 - (shrink * (tPosA2 - focus));


        bool hadBreak = false;

        int N = preCompX.Count;

        Vector3 offset = posA1 + (posA1 - posA2) * focusF;
        Quaternion rotVert = Quaternion.FromToRotation(Vector3.forward, (posA2 - posA1));

        float curStep = step;
        float y = 0.0f;

        bool firstCircle = true;
        float zmin = (c1.x - focus.x);
        float zmax = (c2.x - focus.x);

        for (float z = zmin; z <= zmax; z += curStep) {

            curStep = step;
            if (Mathf.Abs(z) < 0.2f) {//Close to focus => increase precision
                curStep = step / 3.0f;
            }

            bool lastStep = (z + curStep) >= zmax;
            if (lastStep) {
                z = zmax;
            }

            //This is the radius of the circle to draw
            float x = scaleBonds * Mathf.Sqrt( -(y * y) + ( (shrink * (z * z)) / (1 - shrink) ) + (R2 * shrink));

            int curId = vertices.Count;

            if ((x <= 0.001f || float.IsNaN(x))) { //Close the empty ends of the hyperboloid sheet
                hadBreak = true;

                if (!lastStep && !firstCircle) {
                    for (int i = 0; i < N / 2; i++) {
                        triangles.Add(curId + i);
                        triangles.Add(curId + N / 2);
                        triangles.Add(curId + i + 1);

                        triangles.Add(curId + i - N);
                        triangles.Add(curId + i + 1 - N);
                        triangles.Add(curId + N / 2 - N);
                    }
                    for (int i = N / 2; i < N - 1; i++) {
                        triangles.Add(curId + i);
                        triangles.Add(curId);
                        triangles.Add(curId + i + 1);

                        triangles.Add(curId + i - N);
                        triangles.Add(curId + i + 1 - N);
                        triangles.Add(curId - N);
                    }
                }

                continue;
            }


            Color curColor = Color.Lerp(colA1, colA2, (z - zmin) / (zmax - zmin));
            Vector3 posOnAxis = new Vector3(0, 0, z);
            for (int i = 0; i < N; i++) {
                float xx = preCompX[i] * x;
                float yy = preCompY[i] * x;

                Vector3 pos = new Vector3(xx, yy, z);

                if (!firstCircle && !hadBreak) {

                    if (i < N - 1) {
                        triangles.Add(curId + i - N);
                        triangles.Add(curId + i + 1 - N);
                        triangles.Add(curId + i);

                        triangles.Add(curId + i + 1 - N);
                        triangles.Add(curId + i + 1);
                        triangles.Add(curId + i);
                    }
                    else {
                        triangles.Add(curId + i - N - (N - 1));
                        triangles.Add(curId + i);
                        triangles.Add(curId + i - N);

                        triangles.Add(curId + i - N - (N - 1));
                        triangles.Add(curId + i - (N - 1));
                        triangles.Add(curId + i);
                    }
                }

                vertices.Add( (rotVert * pos) + offset);
                colors.Add(curColor);

                //TODO: improve normal computation not exact near the atoms
                normals.Add( (rotVert * (pos - posOnAxis)).normalized);
            }

            hadBreak = false;

            firstCircle = false;
        }
    }
}
}
