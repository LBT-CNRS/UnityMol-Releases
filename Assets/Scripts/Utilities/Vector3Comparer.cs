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
 
class Vector3Comparer : IComparer<Vector3>
{
    public static float epsilon = 0.000000001f;
    public int Compare(Vector3 a, Vector3 b) {
        if(Mathf.Abs(a.x - b.x) < epsilon){
            if(Mathf.Abs(a.y - b.y) < epsilon){
                if(Mathf.Abs(a.z - b.z) < epsilon) return 0;
                else if(a.z < b.z) return -1;
            }
            else if(a.y < b.y) return -1;
        }
        else if(a.x < b.x) return -1;
        return 1;
        // if      (a.x <= b.x && a.y <= b.y && a.z < b.z) return -1;
        // else if (a.x <= b.x && a.y < b.y) return -1;
        // else if (a.x < b.x) return -1;
        // else return 1;
    }
}