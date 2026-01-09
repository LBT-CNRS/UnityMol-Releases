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
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using BenTools.Mathematics;


namespace UMol
{
public class OptimalView : MonoBehaviour {

    public int idAtom = 0;
    public float distPOV = 10.0f;
    int prevIdAtom = -1;


    /// <summary>
    /// Get the position with the largest view cone on a specific target and with respect to an input radius
    /// </summary>
    public static Vector3 GetOptimalPosition (UnityMolSelection sel, Vector3 worldTarget, float radius) {
        Transform loadedMol = UnityMolMain.getRepresentationParent().transform;

        Vector3[] points = new Vector3[sel.atoms.Count];
        int id = 0;
        foreach (UnityMolAtom a in sel.atoms) {
            // points[id++] = loadedMol.InverseTransformPoint(a.curWorldPosition);
            points[id++] = a.curWorldPosition;
        }


        KDTree tmpKDTree = KDTree.MakeFromPoints(points);

        List<Vector3> molWorldCoords = new List<Vector3>(sel.atoms.Count);

        List<Vector3> neighborCoords = new List<Vector3>();
        // List<Vector2> neighbor_theta_phi = new List<Vector2>();
        List<Vector> neighbor_theta_phi = new List<Vector>();
        List<Vector> neighbor_theta_phi_plus10 = new List<Vector>();
        List<Vector> neighbor_theta_phi_plus20 = new List<Vector>();
        float minTheta = 1000.0f;
        float maxTheta = 0.0f;
        float minPhi = 1000.0f;
        float maxPhi = 0.0f;
        int added = 0;

        Vector3 localPos = loadedMol.InverseTransformPoint(worldTarget);//a1.curWorldPosition
        // int[] ids = tmpKDTree.FindNearestsRadius(localPos , radius + 20);

        float transformedRadiusPlus20 = (radius + 20) * loadedMol.localScale.x;
        float transformedRadius = radius * loadedMol.localScale.x;
        float transformedRadiusPlus10 = (radius + 10) * loadedMol.localScale.x;
        int[] ids = tmpKDTree.FindNearestsRadius(worldTarget , transformedRadiusPlus20);

        // Get all the atoms inside the sphere centered on the target and of the input radius, translate them into polar coordinates
        for (int i = 0; i < ids.Length; i++) { //For all neighboors
            UnityMolAtom atom = sel.atoms[ids[i]];

            float dist = Vector3.Distance(localPos, points[ids[i]]);

            if ( dist > 2.0f ) {
                float theta = (float) Mathf.Acos(points[ids[i]].z / dist);
                float phi = (float) Mathf.Atan2(points[ids[i]].y, points[ids[i]].x);

                // Vector2 theta_phi = new Vector2(theta, phi);
                Vector theta_phi = new Vector(2);
                theta_phi[0] = theta;
                theta_phi[1] = phi;

                neighbor_theta_phi_plus20.Add(theta_phi);

                added++;

                if (theta + (float) Mathf.PI < 1.5f * Mathf.PI && phi + (float) Mathf.PI * 2.0f < 2.0f * Mathf.PI)
                {
                    // theta_phi.x = theta + (float) Mathf.PI;
                    // theta_phi.y = phi + (float) Mathf.PI * 2.0f;
                    theta_phi[0] = theta + (float) Mathf.PI;;
                    theta_phi[1] = phi + (float) Mathf.PI * 2.0f;;

                    neighbor_theta_phi_plus20.Add(theta_phi);
                    added += 1;
                }
                if (theta + (float) Mathf.PI < 1.5f * Mathf.PI)
                {
                    theta_phi[0] = theta + (float) Mathf.PI;
                    theta_phi[1] = phi;
                    // theta_phi.x = theta + (float) Mathf.PI;
                    // theta_phi.y = phi;

                    neighbor_theta_phi_plus20.Add(theta_phi);


                    if (theta + (float) Mathf.PI > maxTheta)
                        maxTheta = theta + (float) Mathf.PI;
                    added += 1;
                }
                if ( phi + (float) Mathf.PI * 2.0f < 2.0f * Mathf.PI)
                {
                    // theta_phi.x = theta;
                    // theta_phi.y = phi + (float) Mathf.PI * 2.0f;
                    theta_phi[0] = theta;
                    theta_phi[1] = phi + (float) Mathf.PI * 2.0f;

                    neighbor_theta_phi_plus20.Add(theta_phi);
                    if (phi + (float) Mathf.PI * 2.0f > maxPhi)
                        maxPhi = phi + (float) Mathf.PI * 2.0f;
                    added += 1;
                }
                if (theta < minTheta)
                    minTheta = theta;
                if (phi < minPhi)
                    minPhi = phi;
                if (dist < transformedRadiusPlus10)
                {
                    for (int j = 1; j <= added; j++) {
                        Vector tp = new Vector(2);
                        tp[0] = neighbor_theta_phi_plus20[neighbor_theta_phi_plus20.Count - j][0];
                        tp[1] = neighbor_theta_phi_plus20[neighbor_theta_phi_plus20.Count - j][1];
                        neighbor_theta_phi_plus10.Add(tp);
                    }
                }
                if (dist < transformedRadius)
                {
                    for (int j = 1; j <= added; j++) {
                        Vector tp = new Vector(2);
                        tp[0] = neighbor_theta_phi_plus20[neighbor_theta_phi_plus20.Count - j][0];
                        tp[1] = neighbor_theta_phi_plus20[neighbor_theta_phi_plus20.Count - j][1];
                        neighbor_theta_phi.Add(tp);
                    }
                }
                added = 0;
            }
        }
        Debug.Log("Nb of neighbors: " + neighbor_theta_phi_plus20.Count);
        // Debug.Log("Min/max theta/phi: " + minTheta + " " + maxTheta + " " + minPhi + " " + maxPhi);

        //sort coordinates of all atoms by x
        // points.OrderBy(x => x.x);

        VoronoiGraph result = Fortune.ComputeVoronoiGraph(neighbor_theta_phi);
        if(result.Vertizes.Count < 10){
            result = Fortune.ComputeVoronoiGraph(neighbor_theta_phi_plus10);
        }
        if(result.Vertizes.Count < 10){
            result = Fortune.ComputeVoronoiGraph(neighbor_theta_phi_plus20);
        }
        if(result.Vertizes.Count < 10){
            Debug.LogError("Couldn't find a good POV for this atom");
            return Vector3.zero;
        }
        BenTools.Data.HashSet temp = new BenTools.Data.HashSet();

        foreach (Vector vert in result.Vertizes) {
            if (vert[0] > minTheta && vert[0] < maxTheta && vert[1] < maxPhi && vert[1] > minPhi) {
                temp.Add(vert);
            }
        }

        result.Vertizes = temp;

        float maxDist = 0.0f;
        float[] best_pos = new float[2];

        float distance = 0.0f;
        Dictionary<float, float[]> vertices = new Dictionary<float, float[]>();

        // Find the largest distance between each vertex and the closest point to each of them
        //// 1st METHOD (faster, use the edges that contain point information)
        foreach (VoronoiEdge edge in result.Edges)
        {
            //min_dist = 1000.0;

            if (edge.VVertexA[0] > 0 && edge.VVertexA[0] < Mathf.PI && edge.VVertexA[1] < Mathf.PI && edge.VVertexA[1] > -Mathf.PI)
            {
                distance = Vector2.Distance(
                               new Vector2(edge.VVertexA[0], edge.VVertexA[1]),
                               new Vector2(edge.LeftData[0], edge.LeftData[1])
                           );
                float[] t = new float[2];
                t[0] = (float) edge.VVertexA[0];
                t[1] = (float) edge.VVertexA[1];
                vertices[distance] = t;
                if (distance > maxDist)
                {
                    maxDist = distance;
                    best_pos[0] = (float) edge.VVertexA[0];
                    best_pos[1] = (float) edge.VVertexA[1];
                }

            }
            if (edge.VVertexB[0] > 0 && edge.VVertexB[0] < Mathf.PI && edge.VVertexB[1] < Mathf.PI && edge.VVertexB[1] > -Mathf.PI)
            {
                distance = Vector2.Distance(
                               new Vector2(edge.VVertexB[0], edge.VVertexB[1]),
                               new Vector2(edge.LeftData[0], edge.LeftData[1])
                           );

                float[] t = new float[2];
                t[0] = (float) edge.VVertexB[0];
                t[1] = (float) edge.VVertexB[1];
                vertices[distance] = t;
                if (distance > maxDist)
                {
                    maxDist = distance;
                    best_pos[0] = (float) edge.VVertexB[0];
                    best_pos[1] = (float) edge.VVertexB[1];
                }
            }
        }

        var list = vertices.Keys.ToList();
        list.Sort();
        Vector3 cartesian = Vector3.zero;

        if(list.Count > 0){
            int nb = list.Count - 1;

            cartesian.x = (transformedRadius * (float) Math.Sin(vertices[list[nb]][0]) * (float) Math.Cos(vertices[list[nb]][1])) + worldTarget.x;
            cartesian.y = (transformedRadius * (float) Math.Sin(vertices[list[nb]][0]) * (float) Math.Sin(vertices[list[nb]][1])) + worldTarget.y;
            cartesian.z = (transformedRadius * (float) Math.Cos(vertices[list[nb]][0])) + worldTarget.z;
            Debug.Log("BEST POV"+ loadedMol.InverseTransformPoint(cartesian).ToString("F4"));
            return cartesian;
        }
        else{
            Debug.LogError("Not enough point...");
        }

        return Vector3.zero;


    }

    UnityMolStructure s;
    void Start() {
        s = API.APIPython.fetch("1kx2");
    //     GetOptimalPosition (API.APIPython.last().ToSelection(), API.APIPython.last().currentModel.allAtoms[0].curWorldPosition, 5.0f);
    }

    void Update(){
        if(s != null){
            if(idAtom != prevIdAtom){
                UnityMolAtom a = API.APIPython.last().currentModel.allAtoms[idAtom];
                Vector3 bpov = GetOptimalPosition (API.APIPython.last().ToSelection(), a.curWorldPosition, distPOV);
                prevIdAtom = idAtom;


                ManipulationManager mm = API.APIPython.getManipulationManager();
                // mm.centerOnSelection(a.ToSelection(), true, 1.0f);
                mm.emulateLookAtPOV(a.ToSelection(), bpov, a.curWorldPosition);
            }
        }
    }

}
}