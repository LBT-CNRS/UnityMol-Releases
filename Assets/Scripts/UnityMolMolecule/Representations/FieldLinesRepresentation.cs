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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Linq;
using System.Linq;
using System.Globalization;

namespace UMol {
public class FieldLinesRepresentation: AtomRepresentation {

    public Dictionary <string, List<Vector3>> linesPositions;
    public Int2[] meshVertIds;
    public FieldLinesReader reader;
    public GameObject goFL;
    public float magThreshold = 1.8f;
    public int nbIter = 100;
    public float lineWidth = 0.05f;
    public Color startColor = Color.white;
    public Color endColor = Color.white;
    public float lengthLine = 0.2f;//Length of the line in the shader
    public float speedLine = 1.0f;

    public FieldLinesRepresentation(string structName, UnityMolSelection sel, FieldLinesReader r) {
        colorationType = colorType.full;

        selection = sel;

        GameObject loadedMolGO = UnityMolMain.getRepresentationParent();

        representationParent = loadedMolGO.transform.Find(structName);
        if (representationParent == null) {
            representationParent = (new GameObject(structName).transform);
            representationParent.parent = loadedMolGO.transform;
            representationParent.localPosition = Vector3.zero;
            representationParent.localRotation = Quaternion.identity;
            representationParent.localScale = Vector3.one;
        }

        GameObject newRep = new GameObject("FieldLinesRepresentation");
        newRep.transform.parent = representationParent;

        selection = sel;

        reader = r;

        if (reader == null) {
            if (sel.structures[0].dxr != null) {
                Debug.LogWarning("No fieldlines computed or loaded. Computing fieldlines using dx map...");

                reader = FieldLinesComputation.computeFieldlinesToFLReader(sel.structures[0].dxr, nbIter, magThreshold);

                // API.APIPython.computeFieldlines(sel.structures[0].uniqueName, 1.8f);
                // throw new Exception("No fieldlines computed or loaded");
                // return;
            }
            else {
                // Debug.LogError("No fieldlines computed or loaded");
                GameObject.Destroy(newRep);
                throw new Exception("No fieldlines computed or loaded");
                // return;
            }
        }
        linesPositions = reader.linesPositions;

        DisplayFieldlines (startColor, endColor, newRep.transform);

        newRep.transform.localPosition = Vector3.zero;
        newRep.transform.localRotation = Quaternion.identity;
        newRep.transform.localScale = Vector3.one;

        nbAtoms = sel.Count;
    }

    public void recompute(float newMag, int ite = 100) {


        if (selection.structures[0].currentModel.fieldLinesR == null) {
            if (selection.structures[0].dxr != null) {

                nbIter = ite;
                magThreshold = newMag;

                reader = FieldLinesComputation.computeFieldlinesToFLReader(selection.structures[0].dxr, nbIter, magThreshold);

            }
            else {
                Debug.LogWarning("Cannot recompute the fieldlines representation, no DX map loaded");
                return;
            }
        }
        else {
            Debug.LogWarning("Unload the fieldlines JSON file to recompute this representation");
            return;
        }

        GameObject newRep = null;
        if (goFL != null) {
            newRep = goFL.transform.parent.gameObject;
            GameObject.Destroy(goFL);
        }
        else {
            Debug.LogError("Failed to recompute the fieldlines representation");
            return;
        }


        linesPositions = reader.linesPositions;

        DisplayFieldlines (startColor, endColor, newRep.transform);

        newRep.transform.localPosition = Vector3.zero;
        newRep.transform.localRotation = Quaternion.identity;
        newRep.transform.localScale = Vector3.one;

    }

    public void DisplayFieldlines(Color c1, Color c2, Transform repParent, bool randomOffset = true) {

        // Material mat = new Material (Shader.Find ("Particles/Standard Unlit"));
        // Material mat = new Material (Shader.Find ("Custom/SurfaceVertexColor"));
        Material mat = new Material (Shader.Find ("Custom/FieldlineAnimation"));

        meshVertIds = new Int2[linesPositions.Count];

        Mesh nmesh = new Mesh();
        nmesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        GameObject line = new GameObject ("Fieldlines");
        goFL = line;
        MeshFilter mf = line.AddComponent<MeshFilter>();
        MeshRenderer mr = line.AddComponent<MeshRenderer>();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.transform.SetParent (repParent);
        line.transform.localPosition = Vector3.zero;
        line.transform.localScale = Vector3.one;
        line.transform.localRotation = Quaternion.identity;

        mr.material = mat;

        int totalVert = 0;
        int totalTri = 0;
        foreach (List<Vector3> traj in linesPositions.Values) {
            totalVert += traj.Count * 4;
            totalTri += (4 + traj.Count * 8) * 3;
        }

        Vector3[] vertices = new Vector3[totalVert];
        Vector3[] normals = new Vector3[totalVert];
        Vector2[] uv = new Vector2[totalVert];
        Vector2[] uv2 = new Vector2[totalVert];
        int[] triangles = new int[totalTri];
        Color32[] colors = new Color32[totalVert];


        int vOffset = 0;
        long tOffset = 0;
        int cptL = 0;
        foreach (KeyValuePair<string, List<Vector3>> sl in linesPositions) {

            List<Vector3> traj = sl.Value;

            if (traj.Count == 0 ) {
                continue;
            }

            float offset = 0.0f;
            if (randomOffset) {
                offset = UnityEngine.Random.Range(0.01f, 1.0f);
            }


            Vector3 norm = Vector3.Cross(traj[0], traj[1]);
            Vector3 s = Vector3.Cross(norm, traj[1] - traj[0]);
            s.Normalize();
            norm.Normalize();

            Int2 mId;
            mId.x = vOffset;
            mId.y = traj.Count * 4;
            meshVertIds[cptL] = mId;

            vertices[vOffset + 0] = traj[0] - (s * (lineWidth / 2)) - (norm * (lineWidth / 2));
            vertices[vOffset + 1] = traj[0] + (s * (lineWidth / 2)) - (norm * (lineWidth / 2));
            vertices[vOffset + 2] = traj[0] - (s * (lineWidth / 2)) + (norm * (lineWidth / 2));
            vertices[vOffset + 3] = traj[0] + (s * (lineWidth / 2)) + (norm * (lineWidth / 2));

            uv[vOffset + 0] = Vector2.zero;
            uv[vOffset + 1] = Vector2.zero;
            uv[vOffset + 2] = Vector2.zero;
            uv[vOffset + 3] = Vector2.zero;

            uv2[vOffset + 0] = Vector2.one * offset;
            uv2[vOffset + 1] = Vector2.one * offset;
            uv2[vOffset + 2] = Vector2.one * offset;
            uv2[vOffset + 3] = Vector2.one * offset;

            colors[vOffset + 0] = startColor;
            colors[vOffset + 1] = startColor;
            colors[vOffset + 2] = startColor;
            colors[vOffset + 3] = startColor;

            triangles[tOffset + 0] = vOffset + 1;
            triangles[tOffset + 1] = vOffset + 0;
            triangles[tOffset + 2] = vOffset + 2;

            triangles[tOffset + 3] = vOffset + 1;
            triangles[tOffset + 4] = vOffset + 2;
            triangles[tOffset + 5] = vOffset + 3;

            int idV = 4;
            int idT = 6;
            for (int p = 1; p < traj.Count - 1; p++) {
                Vector3 cur = traj[p];
                Vector3 next = traj[p + 1];

                Vector3 normal = Vector3.Cross(cur, next);
                Vector3 side = Vector3.Cross(normal, next - cur);
                side.Normalize();
                normal.Normalize();

                Vector3 botLef = cur - (side * (lineWidth / 2)) - (normal * (lineWidth / 2));
                Vector3 botRig = cur + (side * (lineWidth / 2)) - (normal * (lineWidth / 2));
                Vector3 topLef = cur - (side * (lineWidth / 2)) + (normal * (lineWidth / 2));
                Vector3 topRig = cur + (side * (lineWidth / 2)) + (normal * (lineWidth / 2));


                vertices[vOffset + idV + 0] = botLef;
                vertices[vOffset + idV + 1] = botRig;
                vertices[vOffset + idV + 2] = topLef;
                vertices[vOffset + idV + 3] = topRig;

                float l = p / (float)traj.Count;

                uv[vOffset + idV + 0] = Vector2.one * l;
                uv[vOffset + idV + 1] = Vector2.one * l;
                uv[vOffset + idV + 2] = Vector2.one * l;
                uv[vOffset + idV + 3] = Vector2.one * l;

                uv2[vOffset + idV + 0] = Vector2.one * offset;
                uv2[vOffset + idV + 1] = Vector2.one * offset;
                uv2[vOffset + idV + 2] = Vector2.one * offset;
                uv2[vOffset + idV + 3] = Vector2.one * offset;

                Color col = Color.Lerp(startColor, endColor, p / (float) traj.Count);
                // DXReader dxr = API.APIPython.last().dxr;
                // float val = dxr.getValueAtPosition(cur);
                // Color col = UnityMolSurfaceManager.dxDensityToColor(val, -5.0f, 5.0f);
                colors[vOffset + idV + 0] = col;
                colors[vOffset + idV + 1] = col;
                colors[vOffset + idV + 2] = col;
                colors[vOffset + idV + 3] = col;


                triangles[tOffset + idT + 0] = vOffset + idV - 4;
                triangles[tOffset + idT + 1] = vOffset + idV - 3;
                triangles[tOffset + idT + 2] = vOffset + idV + 1;

                triangles[tOffset + idT + 3] = vOffset + idV - 4;
                triangles[tOffset + idT + 4] = vOffset + idV + 1;
                triangles[tOffset + idT + 5] = vOffset + idV;

                triangles[tOffset + idT + 6] = vOffset + idV - 2;
                triangles[tOffset + idT + 7] = vOffset + idV - 4;
                triangles[tOffset + idT + 8] = vOffset + idV;

                triangles[tOffset + idT + 9]  = vOffset + idV - 2;
                triangles[tOffset + idT + 10] = vOffset + idV;
                triangles[tOffset + idT + 11] = vOffset + idV + 2;

                triangles[tOffset + idT + 12] = vOffset + idV - 1;
                triangles[tOffset + idT + 13] = vOffset + idV - 2;
                triangles[tOffset + idT + 14] = vOffset + idV + 2;

                triangles[tOffset + idT + 15] = vOffset + idV - 1;
                triangles[tOffset + idT + 16] = vOffset + idV + 2;
                triangles[tOffset + idT + 17] = vOffset + idV + 3;

                triangles[tOffset + idT + 18] = vOffset + idV - 3;
                triangles[tOffset + idT + 19] = vOffset + idV - 1;
                triangles[tOffset + idT + 20] = vOffset + idV + 3;

                triangles[tOffset + idT + 21] = vOffset + idV - 3;
                triangles[tOffset + idT + 22] = vOffset + idV + 3;
                triangles[tOffset + idT + 23] = vOffset + idV + 1;

                idT += 24;
                idV += 4;

            }


            int N = traj.Count;
            //Add the last 4 vertices
            norm = Vector3.Cross(traj[N - 2], traj[N - 1]);
            s = Vector3.Cross(norm, traj[N - 1] - traj[N - 2]);
            s.Normalize();
            norm.Normalize();

            vertices[vOffset + idV + 0] = traj[N - 1] - (s * (lineWidth / 2)) - (norm * (lineWidth / 2));
            vertices[vOffset + idV + 1] = traj[N - 1] + (s * (lineWidth / 2)) - (norm * (lineWidth / 2));
            vertices[vOffset + idV + 2] = traj[N - 1] - (s * (lineWidth / 2)) + (norm * (lineWidth / 2));
            vertices[vOffset + idV + 3] = traj[N - 1] + (s * (lineWidth / 2)) + (norm * (lineWidth / 2));

            uv[vOffset + idV + 0] = Vector2.one;
            uv[vOffset + idV + 1] = Vector2.one;
            uv[vOffset + idV + 2] = Vector2.one;
            uv[vOffset + idV + 3] = Vector2.one;

            uv2[vOffset + idV + 0] = Vector2.one * offset;
            uv2[vOffset + idV + 1] = Vector2.one * offset;
            uv2[vOffset + idV + 2] = Vector2.one * offset;
            uv2[vOffset + idV + 3] = Vector2.one * offset;


            colors[vOffset + idV + 0] = endColor;
            colors[vOffset + idV + 1] = endColor;
            colors[vOffset + idV + 2] = endColor;
            colors[vOffset + idV + 3] = endColor;

            triangles[tOffset + idT + 0] = vOffset + idV - 4;
            triangles[tOffset + idT + 1] = vOffset + idV - 3;
            triangles[tOffset + idT + 2] = vOffset + idV + 1;

            triangles[tOffset + idT + 3] = vOffset + idV - 4;
            triangles[tOffset + idT + 4] = vOffset + idV + 1;
            triangles[tOffset + idT + 5] = vOffset + idV;

            triangles[tOffset + idT + 6] = vOffset + idV - 2;
            triangles[tOffset + idT + 7] = vOffset + idV - 4;
            triangles[tOffset + idT + 8] = vOffset + idV;

            triangles[tOffset + idT + 9] = vOffset + idV - 2;
            triangles[tOffset + idT + 10] = vOffset + idV;
            triangles[tOffset + idT + 11] = vOffset + idV + 2;

            triangles[tOffset + idT + 12] = vOffset + idV - 1;
            triangles[tOffset + idT + 13] = vOffset + idV - 2;
            triangles[tOffset + idT + 14] = vOffset + idV + 2;

            triangles[tOffset + idT + 15] = vOffset + idV - 1;
            triangles[tOffset + idT + 16] = vOffset + idV + 2;
            triangles[tOffset + idT + 17] = vOffset + idV + 3;

            triangles[tOffset + idT + 18] = vOffset + idV - 3;
            triangles[tOffset + idT + 19] = vOffset + idV - 1;
            triangles[tOffset + idT + 20] = vOffset + idV + 3;

            triangles[tOffset + idT + 21] = vOffset + idV - 3;
            triangles[tOffset + idT + 22] = vOffset + idV + 3;
            triangles[tOffset + idT + 23] = vOffset + idV + 1;

            idT += 24;

            triangles[tOffset + idT + 0] = vOffset + idV;
            triangles[tOffset + idT + 1] = vOffset + idV + 1;
            triangles[tOffset + idT + 2] = vOffset + idV + 2;

            triangles[tOffset + idT + 3] = vOffset + idV + 1;
            triangles[tOffset + idT + 4] = vOffset + idV + 3;
            triangles[tOffset + idT + 5] = vOffset + idV + 2;

            idV += 4;
            idT += 6;

            vOffset += idV;
            tOffset += idT;
            cptL++;
        }

        nmesh.vertices = vertices;
        nmesh.normals = normals;
        nmesh.triangles = triangles;
        nmesh.colors32 = colors;
        nmesh.uv = uv;
        nmesh.uv2 = uv2;

        mf.mesh = nmesh;

    }

    public void changeFLSize(float newSize) {

        if (goFL != null) {
            lineWidth = newSize;
        }
        else {
            Debug.LogError("Failed to recompute the fieldlines representation");
            return;
        }

        Mesh m = goFL.GetComponent<MeshFilter>().sharedMesh;
        Vector3[] verts = m.vertices;

        int vOffset = 0;
        foreach (KeyValuePair<string, List<Vector3>> sl in linesPositions) {

            List<Vector3> traj = sl.Value;

            if (traj.Count == 0 ) {
                continue;
            }

            Vector3 norm = Vector3.Cross(traj[0], traj[1]);
            Vector3 s = Vector3.Cross(norm, traj[1] - traj[0]);
            s.Normalize();
            norm.Normalize();


            verts[vOffset + 0] = traj[0] - (s * (lineWidth / 2)) - (norm * (lineWidth / 2));
            verts[vOffset + 1] = traj[0] + (s * (lineWidth / 2)) - (norm * (lineWidth / 2));
            verts[vOffset + 2] = traj[0] - (s * (lineWidth / 2)) + (norm * (lineWidth / 2));
            verts[vOffset + 3] = traj[0] + (s * (lineWidth / 2)) + (norm * (lineWidth / 2));


            int idV = 4;
            // int idT = 6;
            for (int p = 1; p < traj.Count - 1; p++) {
                Vector3 cur = traj[p];
                Vector3 next = traj[p + 1];

                Vector3 normal = Vector3.Cross(cur, next);
                Vector3 side = Vector3.Cross(normal, next - cur);
                side.Normalize();
                normal.Normalize();

                Vector3 botLef = cur - (side * (lineWidth / 2)) - (normal * (lineWidth / 2));
                Vector3 botRig = cur + (side * (lineWidth / 2)) - (normal * (lineWidth / 2));
                Vector3 topLef = cur - (side * (lineWidth / 2)) + (normal * (lineWidth / 2));
                Vector3 topRig = cur + (side * (lineWidth / 2)) + (normal * (lineWidth / 2));


                verts[vOffset + idV + 0] = botLef;
                verts[vOffset + idV + 1] = botRig;
                verts[vOffset + idV + 2] = topLef;
                verts[vOffset + idV + 3] = topRig;

                idV += 4;

            }


            int N = traj.Count;
            //Add the last 4 vertices
            norm = Vector3.Cross(traj[N - 2], traj[N - 1]);
            s = Vector3.Cross(norm, traj[N - 1] - traj[N - 2]);
            s.Normalize();
            norm.Normalize();

            verts[vOffset + idV + 0] = traj[N - 1] - (s * (lineWidth / 2)) - (norm * (lineWidth / 2));
            verts[vOffset + idV + 1] = traj[N - 1] + (s * (lineWidth / 2)) - (norm * (lineWidth / 2));
            verts[vOffset + idV + 2] = traj[N - 1] - (s * (lineWidth / 2)) + (norm * (lineWidth / 2));
            verts[vOffset + idV + 3] = traj[N - 1] + (s * (lineWidth / 2)) + (norm * (lineWidth / 2));

            idV += 4;

            vOffset += idV;
        }

        m.vertices = verts;
    }
    public override void Clean(){}

    // Mesh computeMeshLine(List<Vector3> traj, float offset, ) {
    //     // Mesh m = new Mesh();

    //     //Add the first 4 vertices
    //     Vector3 norm = Vector3.Cross(traj[0], traj[1]);
    //     Vector3 s = Vector3.Cross(norm, traj[1] - traj[0]);
    //     s.Normalize();
    //     norm.Normalize();

    //     vertices[0] = traj[0] - (s * (lineWidth / 2)) - (norm * (lineWidth / 2));
    //     vertices[1] = traj[0] + (s * (lineWidth / 2)) - (norm * (lineWidth / 2));
    //     vertices[2] = traj[0] - (s * (lineWidth / 2)) + (norm * (lineWidth / 2));
    //     vertices[3] = traj[0] + (s * (lineWidth / 2)) + (norm * (lineWidth / 2));

    //     uv[0] = Vector2.zero;
    //     uv[1] = Vector2.zero;
    //     uv[2] = Vector2.zero;
    //     uv[3] = Vector2.zero;

    //     uv2[0] = Vector2.one * offset;
    //     uv2[1] = Vector2.one * offset;
    //     uv2[2] = Vector2.one * offset;
    //     uv2[3] = Vector2.one * offset;

    //     colors[0] = startColor;
    //     colors[1] = startColor;
    //     colors[2] = startColor;
    //     colors[3] = startColor;

    //     triangles[0] = 1;
    //     triangles[1] = 0;
    //     triangles[2] = 2;

    //     triangles[3] = 1;
    //     triangles[4] = 2;
    //     triangles[5] = 3;

    //     int idV = 4;
    //     int idT = 6;
    //     for (int p = 1; p < traj.Count - 1; p++) {
    //         Vector3 cur = traj[p];
    //         Vector3 next = traj[p + 1];

    //         Vector3 normal = Vector3.Cross(cur, next);
    //         Vector3 side = Vector3.Cross(normal, next - cur);
    //         side.Normalize();
    //         normal.Normalize();

    //         Vector3 botLef = cur - (side * (lineWidth / 2)) - (normal * (lineWidth / 2));
    //         Vector3 botRig = cur + (side * (lineWidth / 2)) - (normal * (lineWidth / 2));
    //         Vector3 topLef = cur - (side * (lineWidth / 2)) + (normal * (lineWidth / 2));
    //         Vector3 topRig = cur + (side * (lineWidth / 2)) + (normal * (lineWidth / 2));


    //         vertices[idV + 0] = botLef;
    //         vertices[idV + 1] = botRig;
    //         vertices[idV + 2] = topLef;
    //         vertices[idV + 3] = topRig;

    //         float l = p / (float)traj.Count;

    //         uv[idV + 0] = Vector2.one * l;
    //         uv[idV + 1] = Vector2.one * l;
    //         uv[idV + 2] = Vector2.one * l;
    //         uv[idV + 3] = Vector2.one * l;

    //         uv2[idV + 0] = Vector2.one * offset;
    //         uv2[idV + 1] = Vector2.one * offset;
    //         uv2[idV + 2] = Vector2.one * offset;
    //         uv2[idV + 3] = Vector2.one * offset;

    //         Color col = Color.Lerp(startColor, endColor, p / (float) traj.Count);
    //         colors[idV + 0] = col;
    //         colors[idV + 1] = col;
    //         colors[idV + 2] = col;
    //         colors[idV + 3] = col;


    //         triangles[idT + 0] = idV - 4;
    //         triangles[idT + 1] = idV - 3;
    //         triangles[idT + 2] = idV + 1;

    //         triangles[idT + 3] = idV - 4;
    //         triangles[idT + 4] = idV + 1;
    //         triangles[idT + 5] = idV;

    //         triangles[idT + 6] = idV - 2;
    //         triangles[idT + 7] = idV - 4;
    //         triangles[idT + 8] = idV;

    //         triangles[idT + 9] = idV - 2;
    //         triangles[idT + 10] = idV;
    //         triangles[idT + 11] = idV + 2;

    //         triangles[idT + 12] = idV - 1;
    //         triangles[idT + 13] = idV - 2;
    //         triangles[idT + 14] = idV + 2;

    //         triangles[idT + 15] = idV - 1;
    //         triangles[idT + 16] = idV + 2;
    //         triangles[idT + 17] = idV + 3;

    //         triangles[idT + 18] = idV - 3;
    //         triangles[idT + 19] = idV - 1;
    //         triangles[idT + 20] = idV + 3;

    //         triangles[idT + 21] = idV - 3;
    //         triangles[idT + 22] = idV + 3;
    //         triangles[idT + 23] = idV + 1;

    //         idT += 24;

    //         idV += 4;

    //     }


    //     int N = traj.Count;
    //     //Add the last 4 vertices
    //     norm = Vector3.Cross(traj[N - 2], traj[N - 1]);
    //     s = Vector3.Cross(norm, traj[N - 1] - traj[N - 2]);
    //     s.Normalize();
    //     norm.Normalize();

    //     vertices[idV + 0] = traj[N - 1] - (s * (lineWidth / 2)) - (norm * (lineWidth / 2));
    //     vertices[idV + 1] = traj[N - 1] + (s * (lineWidth / 2)) - (norm * (lineWidth / 2));
    //     vertices[idV + 2] = traj[N - 1] - (s * (lineWidth / 2)) + (norm * (lineWidth / 2));
    //     vertices[idV + 3] = traj[N - 1] + (s * (lineWidth / 2)) + (norm * (lineWidth / 2));

    //     uv[idV + 0] = Vector2.one;
    //     uv[idV + 1] = Vector2.one;
    //     uv[idV + 2] = Vector2.one;
    //     uv[idV + 3] = Vector2.one;

    //     uv2[idV + 0] = Vector2.one * offset;
    //     uv2[idV + 1] = Vector2.one * offset;
    //     uv2[idV + 2] = Vector2.one * offset;
    //     uv2[idV + 3] = Vector2.one * offset;

    //     colors[idV + 0] = endColor;
    //     colors[idV + 1] = endColor;
    //     colors[idV + 2] = endColor;
    //     colors[idV + 3] = endColor;

    //     triangles[idT + 0] = idV - 4;
    //     triangles[idT + 1] = idV - 3;
    //     triangles[idT + 2] = idV + 1;

    //     triangles[idT + 3] = idV - 4;
    //     triangles[idT + 4] = idV + 1;
    //     triangles[idT + 5] = idV;

    //     triangles[idT + 6] = idV - 2;
    //     triangles[idT + 7] = idV - 4;
    //     triangles[idT + 8] = idV;

    //     triangles[idT + 9] = idV - 2;
    //     triangles[idT + 10] = idV;
    //     triangles[idT + 11] = idV + 2;

    //     triangles[idT + 12] = idV - 1;
    //     triangles[idT + 13] = idV - 2;
    //     triangles[idT + 14] = idV + 2;

    //     triangles[idT + 15] = idV - 1;
    //     triangles[idT + 16] = idV + 2;
    //     triangles[idT + 17] = idV + 3;

    //     triangles[idT + 18] = idV - 3;
    //     triangles[idT + 19] = idV - 1;
    //     triangles[idT + 20] = idV + 3;

    //     triangles[idT + 21] = idV - 3;
    //     triangles[idT + 22] = idV + 3;
    //     triangles[idT + 23] = idV + 1;


    //     idT += 24;

    //     triangles[idT + 0] = idV;
    //     triangles[idT + 1] = idV + 1;
    //     triangles[idT + 2] = idV + 2;

    //     triangles[idT + 3] = idV + 1;
    //     triangles[idT + 4] = idV + 3;
    //     triangles[idT + 5] = idV + 2;


    //     m.vertices = vertices;
    //     m.normals = normals;
    //     m.triangles = triangles;
    //     m.colors32 = colors;
    //     m.uv = uv;
    //     m.uv2 = uv2;

    //     return m;
    // }

    //Implementation using line renderer = slow & scale is an issue
    // public void DisplayFieldlinesStatic(Color c1, Color c2, Transform repParent){

    //  Material mat = new Material (Shader.Find ("Particles/Standard Unlit"));

    //  float alpha = 1.0f;
    //  Gradient gradient = new Gradient ();
    //  gradient.SetKeys (
    //      new GradientColorKey[] { new GradientColorKey (c1, 0.0f), new GradientColorKey (c2, 1.0f) },
    //      new GradientAlphaKey[] { new GradientAlphaKey (alpha, 1.0f), new GradientAlphaKey (alpha, 1.0f) }
    //  );

    //  foreach (KeyValuePair<string, List<Vector3>> sl in linesPositions) {
    //      GameObject lines = new GameObject ("Fieldline" + sl.Key);
    //      lines.AddComponent<LineRenderer> ();
    //      lines.transform.SetParent (repParent);
    //      LineRenderer lineRenderer = lines.GetComponent<LineRenderer> ();
    //      lineRenderer.useWorldSpace = false;
    //      lineRenderer.material = mat;
    //      lineRenderer.widthMultiplier = 0.2f;
    //      lineRenderer.positionCount = sl.Value.Count;

    //      lineRenderer.colorGradient = gradient;
    //      lineRenderer.SetPositions (sl.Value.ToArray());
    //      allLinesRenderer.Add (lineRenderer);
    //  }
    // }
}
}