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

[RequireComponent( typeof(LineRenderer) )]
public class ArcLine : MonoBehaviour {

    [Range(1, 50)]
    public int arcSize = 20;
    public float distanceFromB = 2.0f;

    public Vector3 A;
    public Vector3 B;
    public Vector3 C;



    public void UpdatePointLine()
    {

        LineRenderer line = GetComponent<LineRenderer>();

        //get smoothed values
        Vector3[] smoothedPoints = new Vector3[arcSize];

        Vector3 BA = Vector3.Normalize(A - B) * transform.lossyScale.x;
        Vector3 BC = Vector3.Normalize(C - B) * transform.lossyScale.x;

        Vector3 pt = B + BA * distanceFromB;

        Vector3 pivotVec = Vector3.Normalize(Vector3.Cross(BA, BC)) * transform.lossyScale.x;

        float totalAngle = Vector3.SignedAngle(BA, BC, pivotVec);
        float step = totalAngle / (float)arcSize;

        for (int i = 0; i < arcSize; i++) {
            pt = RotatePointAroundPivot(B, B + pivotVec, pt, step);
            smoothedPoints[i] = transform.InverseTransformPoint(pt);
        }

        //set line settings
        line.positionCount = smoothedPoints.Length;
        line.SetPositions( smoothedPoints );
    }


    public static Vector3 RotatePointAroundPivot(Vector3 vecOri, Vector3 vecEnd, Vector3 point, float degrees)
    {
        Vector3 rotationCenter = vecOri + Vector3.Project(point - vecOri, vecEnd - vecOri);
        Vector3 rotationAxis = (vecEnd - vecOri).normalized;
        Vector3 relativePosition = point - rotationCenter;

        Quaternion rotatedAngle = Quaternion.AngleAxis(degrees, rotationAxis);
        Vector3 rotatedPosition = rotatedAngle * relativePosition;

        // New object position
        return rotationCenter + rotatedPosition;
    }


}
