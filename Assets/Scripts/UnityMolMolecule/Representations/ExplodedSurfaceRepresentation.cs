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
using System.Linq;

namespace UMol {
public class ExplodedSurfaceRepresentation : ISurfaceRepresentation {

    public Vector3 symOrigin;
    public Vector3 symVector;
    public float sliceSize = 10.0f;

    public ExplodedSurfaceRepresentation(int idF, string structName, UnityMolSelection sel, Vector3 oriSym, Vector3 vecSym, float sliceVolSize = 10.0f) {

        isStandardSurface = false;

        symOrigin = oriSym;
        symVector = vecSym;
        selection = sel;
        sliceSize = sliceVolSize;

        meshesGO = new List<GameObject>();
        meshColors = new Dictionary<GameObject, Color32[]>();
        colorByAtom = new Color32[sel.atoms.Count];

        normalMat = new Material(Shader.Find("Custom/SurfaceVertexColor"));
        normalMat.enableInstancing = true;

        useAO = false;

        normalMat.SetFloat("_Glossiness", 0.0f);
        normalMat.SetFloat("_Metallic", 0.0f);
        normalMat.SetFloat("_AOIntensity", 0.0f);

        currentMat = normalMat;

        representationParent = UnityMolMain.getRepStructureParent(structName).transform;

        newRep = new GameObject("AtomExplodedSurfaceRepresentation");
        newRep.transform.parent = representationParent;
        representationTransform = newRep.transform;

        subSelections = cutSelection(selection);

        foreach (UnityMolSelection s in subSelections) {
            displayExploSurfaceMesh(s.name + "_ExploSurface", s, newRep.transform);
            // break;
        }

        getMeshColors();

        Color32 white = Color.white;
        for (int i = 0; i < selection.atoms.Count; i++) {
            colorByAtom[i] = white;
        }


        newRep.transform.localPosition = Vector3.zero;
        newRep.transform.localRotation = Quaternion.identity;
        newRep.transform.localScale = Vector3.one;

        // GameObject currentGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // currentGO.transform.parent = newRep.transform;
        // currentGO.transform.localPosition = symOrigin;
        // currentGO.transform.name = "origin";
        // currentGO.transform.localScale = Vector3.one;

        // GameObject currentGO2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // currentGO2.transform.parent = newRep.transform;
        // currentGO2.transform.localPosition = symOrigin + (symVector * 10.0f);
        // currentGO2.transform.name = "vec";
        // currentGO2.transform.localScale = Vector3.one;

        // Debug.DrawLine(sel.atoms[0].associatedGameObject.transform.parent.parent.TransformPoint(symOrigin),
        //     sel.atoms[0].associatedGameObject.transform.parent.parent.TransformPoint(symOrigin + (symVector * 100.0f)), Color.white, 50.0f);

        // Debug.DrawLine(sel.atoms[0].associatedGameObject.transform.parent.parent.TransformPoint(symOrigin),
        //     sel.atoms[0].associatedGameObject.transform.parent.parent.TransformPoint(symOrigin - (symVector * 100.0f)), Color.white, 50.0f);


        nbAtoms = selection.Count;

    }

    private void displayExploSurfaceMesh(string name, UnityMolSelection sele, Transform repParent) {

        GameObject go = constructExploShape(name, repParent, sele);

        if (go != null) {
            meshesGO.Add(go);
            foreach (UnityMolAtom a in sele.atoms) {
                if (!chainToGo.ContainsKey(a.residue.chain)) {
                    chainToGo[a.residue.chain] = go;
                }
            }
        }
    }


    GameObject constructExploShape(string name, Transform repParent, UnityMolSelection sel) {

        if (sel.Count <= 3) {
            return null;
        }
        int arcPrecision = 15;

        Vector3[] projPointsHor = new Vector3[sel.Count];
        Vector3[] projPointsVert = new Vector3[sel.Count];

        int id = 0;
        //Compute center of gravity of the selection
        Vector3 centerOfGrav = Vector3.zero;
        foreach (UnityMolAtom a in sel.atoms) {
            centerOfGrav += a.position;
        }
        centerOfGrav /= sel.Count;


        //Find normal vector of the axis of symmetry
        Vector3 verNorm = Vector3.Normalize(centerOfGrav - symOrigin);

        Vector3 oriToCogProj = Vector3.ProjectOnPlane(centerOfGrav - symOrigin, symVector);

        Vector3 minVert = Vector3.zero;
        Vector3 maxVert = Vector3.zero;
        Vector3 minAnglePt = Vector3.zero;
        Vector3 maxAnglePt = Vector3.zero;

        float distVertMin = -9999.0f;
        float distVertMax = -9999.0f;
        float minAngle = 9999.0f;
        float maxAngle = -9999.0f;

        id = 0;
        foreach (UnityMolAtom a in sel.atoms) {

            //Horizontal projection
            // projPointsHor[id] = ProjectPointOnPlane(symVector, centerOfGrav, a.position);

            //Vertical projection => compute higher and lower points
            projPointsVert[id] = ProjectPointOnPlane(verNorm, centerOfGrav, a.position);
            float d = Vector3.Distance(projPointsVert[id], symOrigin);
            Vector3 A = projPointsVert[id] - symOrigin;

            if (Vector3.Dot(A, symVector) > 0) {

                if (d > distVertMax) {
                    distVertMax = d;
                    maxVert = projPointsVert[id] + a.radius * symVector;
                }
            }
            else {
                if (d > distVertMin) {
                    distVertMin = d;
                    minVert = projPointsVert[id] - a.radius * symVector;
                }
            }

            //Find the 2 horizontal points with the largest angle to define left and right extrem points

            // float angleOriPt_OriCog = Vector3.SignedAngle(centerOfGrav - symOrigin, a.position - symOrigin, symVector);

            Vector3 projB = Vector3.ProjectOnPlane(a.position - symOrigin, symVector);

            float angleOriPt_OriCog = Vector3.SignedAngle(oriToCogProj, projB, symVector);


            if (angleOriPt_OriCog < 0.0f) {
                if (angleOriPt_OriCog < minAngle) {
                    minAngle = angleOriPt_OriCog;
                    minAnglePt = a.position ;//+ a.radius * (a.position - centerOfGrav).normalized;
                }
            }
            else {
                if (angleOriPt_OriCog > maxAngle) {
                    maxAngle = angleOriPt_OriCog;
                    maxAnglePt = a.position ;//+ a.radius * (a.position - centerOfGrav).normalized;
                }
            }

            id++;
        }

        float size = Vector3.Distance(maxVert, minVert);

        int nbSlice = (int)Mathf.Floor(size / sliceSize);

        Vector3[] maxHorSlice = new Vector3[nbSlice];
        Vector3[] minHorSlice = new Vector3[nbSlice];
        float[] maxHorDistSlice = new float[nbSlice];
        float[] minHorDistSlice = new float[nbSlice];

        for (int i = 0; i < nbSlice; i++) {
            maxHorDistSlice[i] = -999.0f;
            minHorDistSlice[i] = 999.0f;
        }

        Vector3 minVertOnSymAxis = ProjectPointOnAxis(minVert, symVector, symOrigin);

        //Cut the selection into slices and find the farthest point per slice
        foreach (UnityMolAtom a in sel.atoms) {
            Vector3 projOnSymAxis = ProjectPointOnAxis(a.position, symVector, symOrigin);
            Vector3 projOnHorAxis = ProjectPointOnAxis(a.position, verNorm, centerOfGrav);

            //Find in what slice the atom is
            float dPmin = Vector3.Distance(minVertOnSymAxis, projOnSymAxis);
            int sliceId = Mathf.Clamp((int)Mathf.Floor(dPmin / sliceSize), 0, nbSlice - 1 );

            float d = Vector3.Distance(symOrigin, projOnHorAxis);
            if ( d > maxHorDistSlice[sliceId] ) {
                maxHorDistSlice[sliceId] = d;
                maxHorSlice[sliceId] = a.position + verNorm * a.radius;
            }
            if ( d < minHorDistSlice[sliceId]) {
                minHorDistSlice[sliceId] = d;
                minHorSlice[sliceId] = a.position - verNorm * a.radius;
            }

        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        List<Vector3> vertices = new List<Vector3>();
        // List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();
        int idV = 0;


        for (int i = 0; i < nbSlice; i++) {

            //Compute lower and higher part of the slice
            float height = i * sliceSize;
            Vector3 FarLow = ProjectPointOnPlane(symVector, minVert + (height * symVector), maxHorSlice[i]);
            Vector3 CloseLow = ProjectPointOnPlane(symVector, minVert + (height * symVector), minHorSlice[i]);


            //Compute distance between the origin and the furthest point of the slice
            Vector3 FarOnSymAxis = ProjectPointOnAxis(FarLow, symVector, symOrigin);
            Vector3 CloseOnSymAxis = ProjectPointOnAxis(CloseLow, symVector, symOrigin);

            float distFar = Vector3.Distance(FarOnSymAxis, FarLow);
            float distClose = Vector3.Distance(FarOnSymAxis, CloseLow);

            //Compute far points on the left and on the right
            Vector3 FarLowLeft = ProjectPointOnPlane(symVector, FarOnSymAxis, minAnglePt);
            FarLowLeft = FarOnSymAxis + Vector3.Normalize(FarLowLeft - FarOnSymAxis) * distFar;

            Vector3 CloseLowLeft = ProjectPointOnPlane(symVector, CloseOnSymAxis, minAnglePt);
            CloseLowLeft = CloseOnSymAxis + Vector3.Normalize(CloseLowLeft - CloseOnSymAxis) * distClose;


            Vector3 FarHighLeft = ProjectPointOnPlane(symVector, minVert + ((height +  sliceSize) * symVector), minAnglePt);
            Vector3 FarHighLeftOnSymAxis = ProjectPointOnAxis(FarHighLeft, symVector, symOrigin);
            FarHighLeft = FarHighLeftOnSymAxis + Vector3.Normalize(FarHighLeft - FarHighLeftOnSymAxis) * distFar;

            Vector3 CloseHighLeft = ProjectPointOnPlane(symVector, minVert + ((height + sliceSize) * symVector), minAnglePt);
            Vector3 CloseHighLeftOnSymAxis = ProjectPointOnAxis(CloseHighLeft, symVector, symOrigin);
            CloseHighLeft = CloseHighLeftOnSymAxis + Vector3.Normalize(CloseHighLeft - CloseHighLeftOnSymAxis) * distClose;

            Vector3 FarLowRight = ProjectPointOnPlane(symVector, FarOnSymAxis, maxAnglePt);
            FarLowRight = FarOnSymAxis + Vector3.Normalize(FarLowRight - FarOnSymAxis) * distFar;

            Vector3 CloseLowRight = ProjectPointOnPlane(symVector, CloseOnSymAxis, maxAnglePt);
            CloseLowRight = CloseOnSymAxis + Vector3.Normalize(CloseLowRight - CloseOnSymAxis) * distClose;

            float angleTotal = Vector3.SignedAngle(FarLowLeft - FarOnSymAxis, FarLowRight - FarOnSymAxis, symOrigin - FarOnSymAxis);

            float step = angleTotal / (float) arcPrecision;

            Vector3 rotatedFarPt = FarLowLeft;
            Vector3 rotatedFarPtHigh = FarHighLeft;
            Vector3 rotatedClosePt = CloseLowLeft;
            Vector3 rotatedClosePtHigh = CloseHighLeft;

            Vector3 lowToSym = Vector3.Normalize(FarOnSymAxis - FarLowLeft);


            vertices.Add(CloseLowLeft); idV++;
            // normals.Add(Vector3.Normalize(Vector3.Cross(lowToSym, -symVector)));
            vertices.Add(CloseHighLeft); idV++;
            // normals.Add(Vector3.Normalize(Vector3.Cross(symVector, lowToSym)));

            vertices.Add(FarLowLeft); idV++;
            // normals.Add(Vector3.Normalize(Vector3.Cross(-symVector, -lowToSym)));
            vertices.Add(FarHighLeft); idV++;
            // normals.Add(Vector3.Normalize(Vector3.Cross(-lowToSym, symVector)));

            //Left triangles
            triangles.Add(idV - 4);
            triangles.Add(idV - 2);
            triangles.Add(idV - 3);

            triangles.Add(idV - 3);
            triangles.Add(idV - 2);
            triangles.Add(idV - 1);


            for (int j = 0; j < arcPrecision; j++) {

                rotatedFarPt = RotatePointAroundPivot(FarOnSymAxis, symOrigin, rotatedFarPt, step);
                rotatedFarPtHigh = RotatePointAroundPivot(FarOnSymAxis, symOrigin, rotatedFarPtHigh, step);

                rotatedClosePt = RotatePointAroundPivot(FarOnSymAxis, symOrigin, rotatedClosePt, step);
                rotatedClosePtHigh = RotatePointAroundPivot(FarOnSymAxis, symOrigin, rotatedClosePtHigh, step);

                int idVStart = idV;

                lowToSym = Vector3.Normalize(FarOnSymAxis - rotatedClosePt);

                vertices.Add(rotatedClosePt); idV++;
                // normals.Add(Vector3.Normalize(Vector3.Lerp(symVector, lowToSym, 0.5f)));
                vertices.Add(rotatedClosePtHigh); idV++;
                // normals.Add(Vector3.Normalize(Vector3.Lerp(symVector, lowToSym, 0.5f)));

                vertices.Add(rotatedFarPt); idV++;
                // normals.Add(Vector3.Normalize(Vector3.Lerp(-symVector, -lowToSym, 0.5f)));
                vertices.Add(rotatedFarPtHigh); idV++;
                // normals.Add(Vector3.Normalize(Vector3.Lerp(symVector, -lowToSym, 0.5f)));


                //Lower triangles
                triangles.Add(idVStart - 4);
                triangles.Add(idVStart);
                triangles.Add(idVStart + 2);

                triangles.Add(idVStart - 2);
                triangles.Add(idVStart - 4);
                triangles.Add(idVStart + 2);


                // //Upper triangles
                triangles.Add(idVStart + 1 );
                triangles.Add(idVStart + 1 - 4);
                triangles.Add(idVStart + 1 + 2);

                triangles.Add(idVStart + 1 - 4);
                triangles.Add(idVStart + 1 - 2);
                triangles.Add(idVStart + 1 + 2);

                // //Slice far triangles
                triangles.Add(idVStart - 4);
                triangles.Add(idVStart - 3);
                triangles.Add(idVStart);

                triangles.Add(idVStart - 3);
                triangles.Add(idVStart + 1);
                triangles.Add(idVStart);

                // //Slice close triangles
                triangles.Add(idVStart - 2);
                triangles.Add(idVStart + 2);
                triangles.Add(idVStart - 1);

                triangles.Add(idVStart - 1);
                triangles.Add(idVStart + 2);
                triangles.Add(idVStart + 3);

            }

            // //Right triangles
            triangles.Add(idV - 4);
            triangles.Add(idV - 3);
            triangles.Add(idV - 1);

            triangles.Add(idV - 4);
            triangles.Add(idV - 1);
            triangles.Add(idV - 2);
        }


        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        // mesh.SetNormals(normals);
        mesh.RecalculateNormals(0);


        GameObject go = new GameObject(name);

        MeshFilter mf = go.AddComponent<MeshFilter>();

        mf.mesh = mesh;

        go.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Custom/SurfaceVertexColor"));

        go.transform.parent = repParent;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;

        return go;


        // return null;

    }


    public static Vector3 RotatePointAroundPivot(Vector3 A, Vector3 B, Vector3 C, float degrees)
    {
        Vector3 rotationCenter = A + Vector3.Project(C - A, B - A);
        Vector3 rotationAxis = (B - A).normalized;
        Vector3 relativePosition = C - rotationCenter;

        Quaternion rotatedAngle = Quaternion.AngleAxis(degrees, rotationAxis);
        Vector3 rotatedPosition = rotatedAngle * relativePosition;

        // New object position
        return rotationCenter + rotatedPosition;
    }

    public static Vector3 ProjectPointOnAxis(Vector3 point, Vector3 axis, Vector3 pointOnAxis) {
        return pointOnAxis + Vector3.Dot((point - pointOnAxis), axis) / Vector3.Dot(axis, axis) * axis;
    }


    //From http://wiki.unity3d.com/index.php?title=3d_Math_functions
    //This function returns a point which is a projection from a point to a plane.
    public static Vector3 ProjectPointOnPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 point) {

        float distance;
        Vector3 translationVector;

        //First calculate the distance from the point to the plane:
        distance = SignedDistancePlanePoint(planeNormal, planePoint, point);

        //Reverse the sign of the distance
        distance *= -1;

        //Get a translation vector
        translationVector = SetVectorLength(planeNormal, distance);

        //Translate the point to form a projection
        return point + translationVector;
    }

    //Get the shortest distance between a point and a plane. The output is signed so it holds information
    //as to which side of the plane normal the point is.
    public static float SignedDistancePlanePoint(Vector3 planeNormal, Vector3 planePoint, Vector3 point) {

        return Vector3.Dot(planeNormal, (point - planePoint));
    }

    //create a vector of direction "vector" with length "size"
    public static Vector3 SetVectorLength(Vector3 vector, float size) {

        //normalize the vector
        Vector3 vectorNormalized = Vector3.Normalize(vector);

        //scale the vector
        return vectorNormalized *= size;
    }

    //----------------------------------------------------------------------------------
    //Code from Théodore Arnaud d'Avray

    //3EI0
    //showSelection("all_3ei0", "explo", Vector3(-26.3882, 3.87756, 52.2547), Vector3(-0.446105, 0.001356, 0.89494))
    // Vector3 sym_origin = new Vector3(-26.3882f, 3.87756f, 52.2547f);
    // Vector3 sym_vector = new Vector3(-0.446105f, 0.001356f, 0.89494f);


    //4XTO

    // Vector3 sym_origin = Vector3.zero;//new Vector3(31.31f, -5.13f, 0.17f);
    // Vector3 sym_vector = new Vector3(0.038f , -0.99f, -0.03f);

    //----------------------------------------------------------------------------------


    public override void recompute(bool isTraj = false) {
        Clear();

        foreach (UnityMolSelection sel in subSelections) {
            displayExploSurfaceMesh(sel.name, sel, newRep.transform);
        }

        getMeshColors();

        restoreColorsPerAtom();
    }

    public override void Clean() {
        Clear();
        colorByAtom = null;
        meshColors.Clear();

        if (normalMat != null)
            GameObject.Destroy(normalMat);
        normalMat = null;

        if (transMat != null)
            GameObject.Destroy(transMat);
        transMat = null;

        if (transMatShadow != null)
            GameObject.Destroy(transMatShadow);
        transMatShadow = null;

        if (wireMat != null)
            GameObject.Destroy(wireMat);
        wireMat = null;
    }

}
}
