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


namespace UMol {
public static class FieldLinesComputation {

    // #space to grid
    static Int3 s2g(Vector3 pos3d, Vector3 dx, Vector3 origin, Int3 dim) {
        int i = Mathf.Max(0, Mathf.FloorToInt((origin.x - pos3d.x) / dx.x));
        int j = Mathf.Max(0, Mathf.FloorToInt((pos3d.y - origin.y) / dx.y));
        int k = Mathf.Max(0, Mathf.FloorToInt((pos3d.z - origin.z) / dx.z));
        i = Mathf.Min(i, dim.x - 1);
        j = Mathf.Min(j, dim.y - 1);
        k = Mathf.Min(k, dim.z - 1);
        Int3 res;
        res.x = i;
        res.y = j;
        res.z = k;
        return res;
    }

    // #grid to space
    static Vector3 g2s(Int3 ijk, Vector3 dx, Vector3 origin, Int3 dim) {
        float x = origin.x - ijk.x * dx.x;
        float y = origin.y + ijk.y * dx.y;
        float z = origin.z + ijk.z * dx.z;
        Vector3 res = new Vector3(x, y, z);
        return res;
    }
    // #to know if the trajectory go out of the grid3D
    static bool isInBox(Vector3 pos, Vector3 dx, Vector3 origin, Int3 dim) {
        if (pos.x < origin.x - dim.x * dx.x)
            return false;
        if (pos.x > origin.x)
            return false;
        if (pos.y > origin.y + dim.y * dx.y)
            return false;
        if (pos.y < origin.y)
            return false;
        if (pos.z > origin.z + dim.z * dx.z)
            return false;
        if (pos.z < origin.z)
            return false;
        return true;
    }

    static List<Int3> getSeeds(Vector3[] grad, Int3 gridSize, float gradThreshold) {

        float minGrad = (gradThreshold * 0.5f);
        float maxGrad = (gradThreshold * 1.5f);
        float minGrad2 = minGrad * minGrad;
        float maxGrad2 = maxGrad * maxGrad;

        List<Int3> ids = new List<Int3>();
        for (int i = 0; i < gridSize.x; i++) {
            for (int j = 0; j < gridSize.y; j++) {
                for (int k = 0; k < gridSize.z; k++) {
                    int idGrad = ((gridSize.z * gridSize.y * i) + (gridSize.z * j) + k);

                    if (grad[idGrad].sqrMagnitude >= minGrad2 && grad[idGrad].sqrMagnitude <= maxGrad2) {
                        Int3 id;
                        id.x = i; id.y =  j; id.z = k;
                        ids.Add(id);
                    }
                }
            }
        }
        return ids;
    }

    static bool IsNaN(Vector3 p) {
        return float.IsNaN(p.x) || float.IsNaN(p.y) || float.IsNaN(p.z);
    }

    public static List<Vector3>[] computeFL(Vector3[] grad, Vector3 dx, Vector3 origin,
                                            Int3 gridSize, int nbIter, float gradThreshold,
                                            float minLength = 10.0f, float maxLength = 50.0f) {

        return FieldLinesBurst.computeFL(grad, dx, origin, gridSize, nbIter, gradThreshold, minLength, maxLength);

    }

    public static FieldLinesReader computeFieldlinesToFLReader(DXReader r, int nbIter, float gradThreshold) {

        Vector3[] grad = r.gradient;
        
        List<Vector3>[] fl = computeFL(grad, r.delta, r.origin, r.gridSize, nbIter, gradThreshold);

        if (fl == null) {
            return null;
        }

        FieldLinesReader fakeFLR = new FieldLinesReader();
        Dictionary<string, List<Vector3>> linesPos = new Dictionary<string, List<Vector3>>();
        int id = 0;
        for (int i = 0; i < fl.Length; i++) {
            if (fl[i].Count != 0) {
                linesPos[id.ToString()] = fl[i];
                id++;
            }
        }
        fakeFLR.linesPositions = linesPos;
        return fakeFLR;
    }

}
}