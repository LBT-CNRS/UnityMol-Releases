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
using System;
using System.Text;
using System.IO;
using System.Linq;

namespace UMol {

public class DXReader {

    public static int MAXGRIDSIZE = 512;
    public float[] densityValues;
    public Int3 gridSize;
    public Vector3 delta;
    public Vector3 origin;
    public Vector3 worldOrigin;

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

        maxDensityVal = -9999.0f;
        minDensityVal = 9999.0f;


        using(sr) {
            string line;
            while ((line = sr.ReadLine()) != null) {
                if (line.Length > 0 && line[0] != '#') {
                    break;
                }
            }

            //Parse grid dimensions
            string[] tokens = line.Split(new [] { ' '}, System.StringSplitOptions.RemoveEmptyEntries);
            int X = int.Parse(tokens[5]);
            int Y = int.Parse(tokens[6]);
            int Z = int.Parse(tokens[7]);

            if (X <= 0 || Y <= 0 || Z <= 0 || X > MAXGRIDSIZE || Y > MAXGRIDSIZE || Z > MAXGRIDSIZE) {
                Debug.LogError("Could not correctly parse the grid size");
                return;
            }

            totalN = X * Y * Z;
            gridSize.x = X;
            gridSize.y = Y;
            gridSize.z = Z;

            densityValues = new float[totalN];

            int cptDelta = 0;
            delta = Vector3.zero;
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
                    if (cptDelta == 0) {
                        delta.x = float.Parse(odvals[1], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else if (cptDelta == 1) {
                        delta.y = float.Parse(odvals[2], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else if (cptDelta == 2) {
                        delta.z = float.Parse(odvals[3], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    cptDelta++;
                }
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

        Debug.Log("Loaded DX map for " + s.uniqueName);
        Debug.Log("Grid size = " + gridSize.x + " x " + gridSize.y + " x " + gridSize.z);
        Debug.Log("Delta = " + delta);
        worldOrigin = new Vector3(origin.x, origin.y, origin.z);

        // #if UNITY_EDITOR
        //     Debug.Log("Origin = "+worldOrigin+" delta = "+delta);
        //     Transform par = UnityMolMain.getStructureManager().structureToGameObject[s.uniqueName].transform;
        //     GameObject go1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //     go1.transform.parent = par;
        //     go1.transform.localScale = Vector3.one;
        //     go1.transform.localPosition = worldOrigin;
        //     GameObject go2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //     go2.transform.parent = par;
        //     go2.transform.localScale = Vector3.one;
        //     Vector3 pos2 = worldOrigin;
        //     pos2.x -= gridSize.x * delta.x;
        //     go2.transform.localPosition = pos2;
        //     GameObject go3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //     go3.transform.parent = par;
        //     go3.transform.localScale = Vector3.one;
        //     Vector3 pos3 = worldOrigin;
        //     pos3.y += gridSize.y * delta.y;
        //     go3.transform.localPosition = pos3;
        //     GameObject go4 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //     go4.transform.parent = par;
        //     go4.transform.localScale = Vector3.one;
        //     Vector3 pos4 = worldOrigin;
        //     pos4.z += gridSize.z * delta.z;
        //     go4.transform.localPosition = pos4;
        //     GameObject go5 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //     go5.transform.parent = par;
        //     go5.transform.localScale = Vector3.one;
        //     Vector3 pos5 = worldOrigin;
        //     pos5.x -= gridSize.x * delta.x;
        //     pos5.y += gridSize.y * delta.y;
        //     go5.transform.localPosition = pos5;
        //     GameObject go6 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //     go6.transform.parent = par;
        //     go6.transform.localScale = Vector3.one;
        //     Vector3 pos6 = worldOrigin;
        //     pos6.x -= gridSize.x * delta.x;
        //     pos6.z += gridSize.z * delta.z;
        //     go6.transform.localPosition = pos6;
        //     GameObject go7 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //     go7.transform.parent = par;
        //     go7.transform.localScale = Vector3.one;
        //     Vector3 pos7 = worldOrigin;
        //     pos7.y += gridSize.y * delta.y;
        //     pos7.z += gridSize.z * delta.z;
        //     go7.transform.localPosition = pos7;
        //     GameObject go8 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //     go8.transform.parent = par;
        //     go8.transform.localScale = Vector3.one;
        //     Vector3 pos8 = worldOrigin;
        //     pos8.x -= gridSize.x * delta.x;
        //     pos8.y += gridSize.y * delta.y;
        //     pos8.z += gridSize.z * delta.z;
        //     go8.transform.localPosition = pos8;
        // #endif


        // origin.x = origin.x - s.currentModel.centerOfGravity.x;
        // origin.y = origin.y - s.currentModel.centerOfGravity.y;
        // origin.z = origin.z - s.currentModel.centerOfGravity.z;
    }

    public void showAsIsoSurface(float isoValue = 0.5f) {

        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();
        repManager.AddRepresentation(structure.ToSelection(), AtomType.DXSurface, BondType.nobond, this, isoValue);
        if (isoValue < 0.0f) {
            structure.representations.Last().SetColors(structure.ToSelection().atoms, Color.red);
        }
        else if (isoValue > 0.0f) {
            structure.representations.Last().SetColors(structure.ToSelection().atoms, Color.blue);
        }
        else {
            structure.representations.Last().SetColors(structure.ToSelection().atoms, Color.white);
        }
    }

    private int World2Grid(Vector3 pos) {
        //X is inverted in Unity
        int x = Mathf.Max(0, (int)((worldOrigin.x - pos.x) / delta.x));
        // int x = Mathf.Max(0, (int)((pos.x - worldOrigin.x) / delta.x));
        int y = Mathf.Max(0, (int)((pos.y - worldOrigin.y) / delta.y));
        int z = Mathf.Max(0, (int)((pos.z - worldOrigin.z) / delta.z));

        return (gridSize.z * gridSize.y * x) + (gridSize.z * y) + z;
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
    public Vector3[] computeGradient() {
        float xsinv = 1.0f;
        float ysinv = 1.0f;
        float zsinv = 1.0f;

        if (gridSize.x > 1)
            xsinv = 1.0f / (delta.x * 2);
        if (gridSize.y > 1)
            ysinv = 1.0f / (delta.y * 2);
        if (gridSize.z > 1)
            zsinv = 1.0f / (delta.z * 2);

        float xs = -0.5f * xsinv;
        float ys = 0.5f * ysinv;
        float zs = 0.5f * zsinv;

        // float val;
        // float valprec;
        // float valnext;

        // Vector3[, ,] gradient = new Vector3[gridSize.x, gridSize.y, gridSize.z];
        Vector3[] gradient = new Vector3[gridSize.x*gridSize.y*gridSize.z];

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
}
}