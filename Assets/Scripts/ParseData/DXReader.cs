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
using System;
using System.Text;
using System.IO;
using System.Linq;

using Unity.Mathematics;

namespace UMol {

public class DXReader {

    public static int MAXGRIDSIZE = 512;
    public float[] densityValues;
    public int3 gridSize;
    public bool diffCellDim = false;
    public Vector3[] deltaS;
    public Vector3 origin;
    public Vector3 worldOrigin;
    public List<GameObject> linesGO;

    public Vector3[] cellDir;
    public Vector3[] cellAxis;
    public float xl;
    public float yl;
    public float zl;


    public float maxDensityVal;
    public float minDensityVal;

    Vector3[] _grad;

    public Vector3[] gradient {
        get {
            if (_grad == null)
                _grad = computeGradient();
            return _grad;
        }
    }
    public Dictionary<int, int> _cellIdToAtomId;
    public Dictionary<int, int> cellIdToAtomId {
        get {
            if (_cellIdToAtomId == null) {
                getCellContainingAtoms();
            }
            return _cellIdToAtomId;
        }
    }



    public string dxFilePath;

    public UnityMolStructure structure;

    public DXReader(string filePath) {
        dxFilePath = filePath;
    }

    public void readDxFile(UnityMolStructure s) {
        structure = s;
        StreamReader sr = new StreamReader(dxFilePath);
        densityValues = null;
        int idGrid = 0;
        int totalN = 0;
        int X;
        int Y;
        int Z;
        maxDensityVal = float.MinValue;
        minDensityVal = float.MaxValue;
        deltaS = new Vector3[3];
        cellDir = new Vector3[3];
        cellAxis = new Vector3[3];
        linesGO = new List<GameObject>(12);

        using(sr) {
            string line;
            while ((line = sr.ReadLine()) != null) {
                if (line.Length > 0 && line[0] != '#') {
                    break;
                }
            }

            if (line.StartsWith("object 1 class gridpositions counts")) {


                //Parse grid dimensions
                string[] tokens = line.Split(new [] { ' '}, System.StringSplitOptions.RemoveEmptyEntries);
                X = int.Parse(tokens[5]);
                Y = int.Parse(tokens[6]);
                Z = int.Parse(tokens[7]);

                if (X <= 0 || Y <= 0 || Z <= 0 || X > MAXGRIDSIZE || Y > MAXGRIDSIZE || Z > MAXGRIDSIZE) {
                    Debug.LogError("Could not correctly parse the grid size");
                    return;
                }

                totalN = X * Y * Z;
                gridSize.x = X;
                gridSize.y = Y;
                gridSize.z = Z;

                densityValues = new float[totalN];
            }
            else {
                Debug.LogError("Couldn't read the grid dimensions");
                return;
            }



            int cptDelta = 0;
            while ((line = sr.ReadLine()) != null) {
                if (!line.StartsWith("origin") && !line.StartsWith("delta") && !line.StartsWith("object")) {
                    break;
                }
                string[] odvals = line.Split(new [] { ' '}, System.StringSplitOptions.RemoveEmptyEntries);
                if (line.StartsWith("origin")) {
                    // origin = new Vector3(float.Parse(odvals[1]), float.Parse(odvals[2]), float.Parse(odvals[3]));
                    origin = new Vector3(-float.Parse(odvals[1], System.Globalization.CultureInfo.InvariantCulture),
                                         float.Parse(odvals[2], System.Globalization.CultureInfo.InvariantCulture),
                                         float.Parse(odvals[3], System.Globalization.CultureInfo.InvariantCulture));
                }
                if (line.StartsWith("delta")) {

                    deltaS[cptDelta].x = float.Parse(odvals[1], System.Globalization.CultureInfo.InvariantCulture);
                    deltaS[cptDelta].y = float.Parse(odvals[2], System.Globalization.CultureInfo.InvariantCulture);
                    deltaS[cptDelta].z = float.Parse(odvals[3], System.Globalization.CultureInfo.InvariantCulture);


                    cptDelta++;
                }
            }
            for (int i = 0; i < 3; i++) {
                deltaS[i].x *= -1;
            }

            //Check if this is a map with different cell dimensions
            if (deltaS[0].y != 0.0f || deltaS[0].z != 0.0f ||
                    deltaS[1].x != 0.0f || deltaS[1].z != 0.0f ||
                    deltaS[2].x != 0.0f || deltaS[2].y != 0.0f) {
                diffCellDim = true;
            }

            bool stop = false;
            int x, y, z;
            x = 0; y = 0; z = 0;
            do {

                string[] fvals = line.Split(new [] { ' ', '\t'}, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (string st in fvals) {
                    if (idGrid == totalN) {
                        stop = true;
                        break;
                    }
                    // densityValues[idGrid++] = float.Parse(st);
                    // densityValues[x + y * X + z * X * Y] = float.Parse(st);//VMD

                    float val = float.Parse(st, System.Globalization.CultureInfo.InvariantCulture);
                    densityValues[Z * Y * x + Z * y + z] = val;

                    maxDensityVal = Mathf.Max(val, maxDensityVal);
                    minDensityVal = Mathf.Min(val, minDensityVal);


                    idGrid++;
                    z++;
                    if (z >= Z) {
                        z = 0;
                        y++;
                        if (y >= Y) {
                            y = 0;
                            x++;
                        }
                    }
                }
                if (stop) {
                    break;
                }
            }
            while ((line = sr.ReadLine()) != null);
        }
        s.dxr = this;

        worldOrigin = new Vector3(origin.x, origin.y, origin.z);

        Debug.Log("Loaded DX map for " + s.name);
        Debug.Log("Grid size = " + gridSize.x + " x " + gridSize.y + " x " + gridSize.z);
        Debug.Log("Delta = " + deltaS[0].ToString("f3") + " / " + deltaS[1].ToString("f3") + " / " + deltaS[2].ToString("f3"));
        Debug.Log("Origin = " + worldOrigin.ToString("f4"));


        for (int i = 0; i < 3; i++) {
            cellAxis[0][i] = deltaS[0][i] * (X - 1);
            cellAxis[1][i] = deltaS[1][i] * (Y - 1);
            cellAxis[2][i] = deltaS[2][i] * (Z - 1);
        }

        // computeCellDir();

        cellDir[0] = deltaS[0];
        cellDir[1] = deltaS[1];
        cellDir[2] = deltaS[2];

        xl = Mathf.Sqrt(Vector3.Dot(cellAxis[0], cellAxis[0])) * (1.0f / (Mathf.Max(1, gridSize.x - 1)));
        yl = Mathf.Sqrt(Vector3.Dot(cellAxis[1], cellAxis[1])) * (1.0f / (Mathf.Max(1, gridSize.y - 1)));
        zl = Mathf.Sqrt(Vector3.Dot(cellAxis[2], cellAxis[2])) * (1.0f / (Mathf.Max(1, gridSize.z - 1)));


        Transform par = UnityMolMain.getStructureManager().structureToGameObject[s.name].transform;
        GameObject go1 = new GameObject("DXGrid");
        go1.transform.parent = par;
        go1.transform.localScale = Vector3.one * 2;
        go1.transform.localPosition = worldOrigin;
        GameObject go2 = new GameObject("DXGrid");
        go2.transform.parent = par;
        go2.transform.localScale = Vector3.one;
        Vector3 pos2 = worldOrigin;
        // pos2.x -= gridSize.x * delta.x;
        pos2 += deltaS[0] * gridSize.x;
        // pos2 += -cellAxis[0];
        go2.transform.localPosition = pos2;
        GameObject go3 = new GameObject("DXGrid");
        go3.transform.parent = par;
        go3.transform.localScale = Vector3.one;
        Vector3 pos3 = worldOrigin;
        // pos3.y += gridSize.y * delta.y;
        pos3 += deltaS[1] * gridSize.y;
        go3.transform.localPosition = pos3;
        GameObject go4 = new GameObject("DXGrid");
        go4.transform.parent = par;
        go4.transform.localScale = Vector3.one;
        Vector3 pos4 = worldOrigin;
        // pos4.z += gridSize.z * delta.z;
        pos4 += deltaS[2] * gridSize.z;
        go4.transform.localPosition = pos4;
        GameObject go5 = new GameObject("DXGrid");
        go5.transform.parent = par;
        go5.transform.localScale = Vector3.one;
        Vector3 pos5 = worldOrigin;
        // pos5.x -= gridSize.x * delta.x;
        // pos5.y += gridSize.y * delta.y;
        pos5 += deltaS[0] * gridSize.x;
        pos5 += deltaS[1] * gridSize.y;
        go5.transform.localPosition = pos5;
        GameObject go6 = new GameObject("DXGrid");
        go6.transform.parent = par;
        go6.transform.localScale = Vector3.one;
        Vector3 pos6 = worldOrigin;
        // pos6.x -= gridSize.x * delta.x;
        // pos6.z += gridSize.z * delta.z;
        pos6 += deltaS[0] * gridSize.x;
        pos6 += deltaS[2] * gridSize.z;
        go6.transform.localPosition = pos6;
        GameObject go7 = new GameObject("DXGrid");
        go7.transform.parent = par;
        go7.transform.localScale = Vector3.one;
        Vector3 pos7 = worldOrigin;
        // pos7.y += gridSize.y * delta.y;
        // pos7.z += gridSize.z * delta.z;
        pos7 += deltaS[1] * gridSize.y;
        pos7 += deltaS[2] * gridSize.z;
        go7.transform.localPosition = pos7;
        GameObject go8 = new GameObject("DXGrid");
        go8.transform.parent = par;
        go8.transform.localScale = Vector3.one;
        Vector3 pos8 = worldOrigin;
        // pos8.x -= gridSize.x * delta.x;
        // pos8.y += gridSize.y * delta.y;
        // pos8.z += gridSize.z * delta.z;
        pos8 += deltaS[0] * gridSize.x;
        pos8 += deltaS[1] * gridSize.y;
        pos8 += deltaS[2] * gridSize.z;
        go8.transform.localPosition = pos8;

        DrawLine(go1.transform.localPosition, go2.transform.localPosition, Color.red, par);
        DrawLine(go1.transform.localPosition, go3.transform.localPosition, Color.green, par);
        DrawLine(go1.transform.localPosition, go4.transform.localPosition, Color.blue, par);
        DrawLine(go2.transform.localPosition, go5.transform.localPosition, Color.white, par);
        DrawLine(go3.transform.localPosition, go5.transform.localPosition, Color.white, par);
        DrawLine(go4.transform.localPosition, go6.transform.localPosition, Color.white, par);
        DrawLine(go2.transform.localPosition, go6.transform.localPosition, Color.white, par);
        DrawLine(go4.transform.localPosition, go7.transform.localPosition, Color.white, par);
        DrawLine(go3.transform.localPosition, go7.transform.localPosition, Color.white, par);
        DrawLine(go8.transform.localPosition, go7.transform.localPosition, Color.white, par);
        DrawLine(go6.transform.localPosition, go8.transform.localPosition, Color.white, par);
        DrawLine(go8.transform.localPosition, go5.transform.localPosition, Color.white, par);

        // origin.x = origin.x - s.currentModel.centroid.x;
        // origin.y = origin.y - s.currentModel.centroid.y;
        // origin.z = origin.z - s.currentModel.centroid.z;
    }



    private int World2Grid(Vector3 pos) {

        float3 tmp = pos - worldOrigin;
        float4x4 m = new float4x4(

            deltaS[0].x, deltaS[1].x, deltaS[2].x, 0.0f,
            deltaS[0].y, deltaS[1].y, deltaS[2].y, 0.0f,
            deltaS[0].z, deltaS[1].z, deltaS[2].z, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f);

        float4x4 invm = math.inverse(m);

        int3 res = (int3)math.transform(invm, tmp);

        return (gridSize.z * gridSize.y * res.x) + (gridSize.z * res.y) + res.z;
    }

    public float getValueAtPosition(Vector3 pos) {
        int world2Grid = World2Grid(pos);
        world2Grid = Mathf.Min(Mathf.Max(0, world2Grid), densityValues.Length - 1);
        return densityValues[world2Grid];
    }


    float getVal(int i, int j, int k) {
        int id = ((gridSize.z * gridSize.y * i) + (gridSize.z * j) + k);
        // int id = ((gridSize.x * gridSize.y * k) + (gridSize.x * j) + i);
        return densityValues[id];
    }
    static int3 unflatten1DTo3D(int index, int3 dim) {
        int x = index / (dim.y * dim.z);
        int y = (index - x * dim.y * dim.z) / dim.z;
        int z = index - x * dim.y * dim.z - y * dim.z;

        return new int3(x, y, z);
    }

    public Vector3[] computeGradient() {

        float xs = -0.5f * xl;
        float ys = 0.5f * yl;
        float zs = 0.5f * zl;

        // float val;
        // float valprec;
        // float valnext;

        // Vector3[, ,] gradient = new Vector3[gridSize.x, gridSize.y, gridSize.z];
        Vector3[] gradient = new Vector3[gridSize.x * gridSize.y * gridSize.z];

        for (int i = 0; i < gridSize.x; i++) {
            int xm = Mathf.Clamp(i - 1, 0, gridSize.x - 1);
            int xp = Mathf.Clamp(i + 1, 0, gridSize.x - 1);
            for (int j = 0; j < gridSize.y; j++) {
                int ym = Mathf.Clamp(j - 1, 0, gridSize.y - 1);
                int yp = Mathf.Clamp(j + 1, 0, gridSize.y - 1);
                for (int k = 0; k < gridSize.z; k++) {

                    Vector3 gradVal = Vector3.zero;
                    int zm = Mathf.Clamp(k - 1, 0, gridSize.z - 1);
                    int zp = Mathf.Clamp(k + 1, 0, gridSize.z - 1);
                    // val = getVal(i, j, k);

                    float xprev = getVal(xm, j, k);
                    float xnext = getVal(xp, j, k);

                    // gradient[i, j, k].x = ((val - valprec) + (valnext - val)) / delta.x;
                    gradVal.x = (xnext - xprev) * xs;

                    float yprev = getVal(i, ym, k);
                    float ynext = getVal(i, yp, k);

                    // gradient[i, j, k].y = ((val - valprec) + (valnext - val)) / delta.y;
                    gradVal.y = (ynext - yprev) * ys;

                    float zprev = getVal(i, j, zm);
                    float znext = getVal(i, j, zp);

                    // gradient[i, j, k].z = ((val - valprec) + (valnext - val)) / delta.z;
                    gradVal.z = (znext - zprev) * zs;

                    // gradient[i, j, k] = (gradient[i, j, k] * scale);
                    int id = ((gridSize.z * gridSize.y * i) + (gridSize.z * j) + k);
                    gradient[id] = gradVal;
                }
            }
        }

        return gradient;

    }

    ///Can be called to update the association between grid cells and atoms
    public void getCellContainingAtoms() {
        _cellIdToAtomId = new Dictionary<int, int>();
        int3 gSize = new int3(gridSize.x, gridSize.y, gridSize.z);
        float maxDelta = math.max(deltaS[0].x, math.max(deltaS[1].y, deltaS[2].z));

        Transform par = UnityMolMain.getStructureManager().structureToGameObject[structure.name].transform;

        foreach (UnityMolAtom a in structure.currentModel.allAtoms) {

            int idcell = World2Grid(a.position);
            int3 ijk = unflatten1DTo3D(idcell, gSize);

            //Double the radius here to clearly see atom colors on iso-surfaces
            int range = (int)Mathf.Ceil(2 * a.radius * maxDelta);
            for (int i = math.max(0, ijk.x - range); i < math.min(gSize.x, ijk.x + range); i++) {
                for (int j = math.max(0, ijk.y - range); j < math.min(gSize.y, ijk.y + range); j++) {
                    for (int k = math.max(0, ijk.z - range); k < math.min(gSize.z, ijk.z + range); k++) {
                        int newId = ((gridSize.z * gridSize.y * i) + (gridSize.z * j) + k);
                        cellIdToAtomId[newId] = a.idInAllAtoms;
                    }
                }
            }
        }
    }

    void DrawLine(Vector3 localstart, Vector3 localend, Color color, Transform par, float duration = 0.0f)
    {
        GameObject myLine = new GameObject("LineDX");
        myLine.transform.parent = par;
        myLine.transform.localScale = Vector3.one;
        myLine.transform.localRotation = Quaternion.identity;
        myLine.transform.localPosition = Vector3.zero;

        LineRenderer lr = myLine.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.positionCount = 2;

        Shader lineShader = Shader.Find("Particles/Alpha Blended");
        if (lineShader == null)
            lineShader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");

        lr.sharedMaterial = new Material(lineShader);
        lr.startColor = lr.endColor = color;
        lr.startWidth = lr.endWidth = 0.01f;
        lr.alignment = LineAlignment.View;

        lr.SetPosition(0, localstart);
        lr.SetPosition(1, localend);
        linesGO.Add(myLine);
        if (duration > 0.0f) {
            GameObject.Destroy(myLine, duration);
            GameObject.Destroy(lr.sharedMaterial, duration);
        }
    }

    public void hideLines() {
        if (linesGO != null) {
            foreach (GameObject go in linesGO) {
                go.GetComponent<LineRenderer>().enabled = false;
            }
        }
    }
    public void showLines() {
        if (linesGO != null) {
            foreach (GameObject go in linesGO) {
                go.GetComponent<LineRenderer>().enabled = true;
            }
        }
    }
    public void destroyLines() {
        if (linesGO != null) {
            try {
                for (int i = 0; i < linesGO.Count; i++) {
                    MeshRenderer mr = linesGO[i].GetComponent<MeshRenderer>();
                    if (mr)
                        GameObject.Destroy(mr.sharedMaterial);
                    GameObject.Destroy(linesGO[i]);
                }
                linesGO.Clear();
                linesGO = null;
            }
            catch (System.Exception e) {
#if UNITY_EDITOR
                throw new System.Exception("" + e);
#endif
            }
        }
    }
}
}
