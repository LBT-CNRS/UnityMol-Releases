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
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;
using System.IO;
using System.Linq;
using UMol.API;
using Unity.Mathematics;

namespace UMol {
public class CCP4Parser_WIP : MonoBehaviour {
    public string path = "/pdbs/1cbs.ccp4";

    public float rad = 20.0f;

    GameObject meshgo;
    Vector2[] uv2;
    Vector3[] vertices;

    void Start () {


        UnityMolStructure s = APIPython.fetch("1cbs", showDefaultRep: false);
        APIPython.showSelection(s.ToSelectionName(), "hb");
        APIPython.setHyperBallMetaphore(s.ToSelectionName(), "licorice");

        byte[] fileBytes = File.ReadAllBytes(path);

        int mode = System.Convert.ToInt32(fileBytes[3 * 4]);
        if (mode != 2) {
            Debug.LogError("Error reading the CCP4 file (only mode 2 allowed)");
            return;
        }

        int X = System.Convert.ToInt32(fileBytes[0]);
        int Y = System.Convert.ToInt32(fileBytes[1 * 4]);
        int Z = System.Convert.ToInt32(fileBytes[2 * 4]);

        int nx = System.Convert.ToInt32(fileBytes[4 * 4]);
        int ny = System.Convert.ToInt32(fileBytes[5 * 4]);
        int nz = System.Convert.ToInt32(fileBytes[6 * 4]);

        int gx = System.Convert.ToInt32(fileBytes[7 * 4]);
        int gy = System.Convert.ToInt32(fileBytes[8 * 4]);
        int gz = System.Convert.ToInt32(fileBytes[9 * 4]);



        if (X < 0 || gx < 0 || X > DXReader.MAXGRIDSIZE || Y < 0 || gy < 0 || Y > DXReader.MAXGRIDSIZE || Z < 0 || gz < 0 || Z > DXReader.MAXGRIDSIZE) {
            Debug.LogError("Error reading the CCP4 file");
            return;
        }

        float xlen = System.BitConverter.ToSingle(fileBytes, 10 * 4);
        float ylen = System.BitConverter.ToSingle(fileBytes, 11 * 4);
        float zlen = System.BitConverter.ToSingle(fileBytes, 12 * 4);

        float dx = xlen / gx;
        float dy = ylen / gy;
        float dz = zlen / gz;


        int crs2x = System.Convert.ToInt32(fileBytes[16 * 4]);
        int crs2y = System.Convert.ToInt32(fileBytes[17 * 4]);
        int crs2z = System.Convert.ToInt32(fileBytes[18 * 4]);

        int3 xyz2crs;
        xyz2crs.x = 0;
        xyz2crs.y = 0;
        xyz2crs.z = 0;
        xyz2crs[crs2x - 1] = 0;
        xyz2crs[crs2y - 1] = 1;
        xyz2crs[crs2z - 1] = 2;

        Vector3 delta = new Vector3(dx, dy, dz);


        int totalSize = X * Y * Z;
        Debug.Log("Total size = " + X + " x " + Y + " x " + Z + " = " + totalSize);


        float[] grid = new float[totalSize];
        float[] finalGrid = new float[totalSize];

        System.Buffer.BlockCopy(fileBytes, 256 * 4, grid, 0, totalSize * 4);

        int3 gridSize; gridSize.x = X; gridSize.y = Y; gridSize.z = Z;
        Debug.Log("Delta: " + delta.x + " / " + delta.y + " / " + delta.z);
        float maxVal = grid[0];
        float minVal = grid[0];
        for (int i = 0; i < grid.Length; i++) {
            maxVal = Mathf.Max(maxVal, grid[i]);
            minVal = Mathf.Min(minVal, grid[i]);
        }
        Debug.Log("Min: " + minVal.ToString("f3") + " Max: " + maxVal.ToString("f3") + " / gridleng = " + grid.Length);

        int3 extent; extent.x = 0; extent.y = 0; extent.z = 0;

        extent[xyz2crs.x] = X;
        extent[xyz2crs.y] = Y;
        extent[xyz2crs.z] = Z;

        gridSize = extent;

        int3 coord; coord.x = 0; coord.y = 0; coord.z = 0;

        // float[, ,] tmpArr = new float[X, Y, Z];

        int cpt = 0;
        for (coord.z = 0; coord.z < extent.z; coord.z++) {
            for (coord.y = 0; coord.y < extent.y; coord.y++) {
                for (coord.x = 0; coord.x < extent.x; coord.x++) {
                    int x = coord[xyz2crs.x];
                    int y = coord[xyz2crs.y];
                    int z = coord[xyz2crs.z];
                    // tmpArr[x, y, z] = grid[cpt++];
                    // finalGrid[x + y * X + z * X * Y] = grid[cpt++];
                    finalGrid[Z * Y * x + Z * y + z] = grid[cpt++];
                }
            }
        }

        // cpt = 0;
        // for (int i = 0; i < X; i++) {
        //  for (int j = 0; j < Y; j++) {
        //      for (int k = 0; k < Z; k++) {
        //          finalGrid[Z * Y * i + Z * j + k] = tmpArr[i, j, k];
        //      }
        //  }
        // }

        DXReader ccp4map = new DXReader(path);
        ccp4map.structure = s;
        ccp4map.gridSize = gridSize;
        ccp4map.densityValues = finalGrid;
        Vector3[] deltaS = new Vector3[3];
        deltaS[0] = new Vector3(delta.x, 0.0f, 0.0f);
        deltaS[1] = new Vector3(0.0f, delta.y, 0.0f);
        deltaS[2] = new Vector3(0.0f, 0.0f, delta.z);
        ccp4map.deltaS = deltaS;
        ccp4map.origin = Vector3.zero;
        ccp4map.worldOrigin = Vector3.zero;
        ccp4map.maxDensityVal = maxVal;
        ccp4map.minDensityVal = minVal;

        s.dxr = ccp4map;

        APIPython.showSelection(s.ToSelectionName(), "dxiso", s.dxr, 0.75f);
        APIPython.setWireframeSurface(s.ToSelectionName());

        UnityMolRepresentationManager repM = UnityMolMain.getRepresentationManager();
        string selName = s.ToSelectionName();
        RepType repType = APIPython.getRepType("dxiso");
        List<UnityMolRepresentation> existingReps = repM.representationExists(selName, repType);


        UnityMolRepresentation r = existingReps.Last();
        GameObject go = ((DXSurfaceRepresentation)r.subReps.Last().atomRep).meshesGO.First();

        meshgo = go;

    }




}
}
