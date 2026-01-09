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
using UnityEngine.Rendering;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using Unity.Mathematics;


namespace UMol {

public class AtomRepresentationTube : AtomRepresentation {

    public Dictionary<UnityMolAtom, List<int>> atomToMeshVertex;
    public List<int2> alphaTrace;
    public GameObject meshGO;
    public Mesh curMesh;
    public float lineWidth = 0.15f;
    public List<Vector3> vertices;
    public List<Color32> colors;
    public List<Color32> savedColors;
    public Material tubeMat;

    public AtomRepresentationTube(int idF, string structName, UnityMolSelection sel) {

        colorationType = colorType.atom;
        
        representationParent = UnityMolMain.getRepStructureParent(structName).transform;

        GameObject newRep = new GameObject("AtomTubeRepresentation");
        newRep.transform.parent = representationParent;
        representationTransform = newRep.transform;


        selection = sel;
        idFrame = idF;

        DisplayTubeMesh(newRep.transform);

        // newRep.transform.position -= offset;
        newRep.transform.localPosition = Vector3.zero;
        newRep.transform.localRotation = Quaternion.identity;
        newRep.transform.localScale = Vector3.one;

        nbAtoms = selection.atoms.Count;
    }

    void DisplayTubeMesh(Transform repParent, bool isRecompute = false) {

        if (selection.atoms.Count < 2)
            return;


        alphaTrace = new List<int2>();
        UnityMolAtom prevCA = null;
        int idPrevA = -1;

        int id = 0;
        foreach (UnityMolAtom a in selection.atoms) {
            string toFind = "CA";
            if (MDAnalysisSelection.isNucleic(a.residue)) {
                toFind = "P";
            }
            if (a.residue.chain.model.structure.structureType ==
                    UnityMolStructure.MolecularType.Martini) {
                toFind = "BB";
                if (a.name != toFind) { //Try with BB1 too
                    toFind = "BB1";
                }
            }
            if (a.name == toFind) {

                if (prevCA != null &&
                        prevCA.residue.chain.model.structure.name == a.residue.chain.model.structure.name && //Same structure
                        prevCA.residue.chain.name == a.residue.chain.name && //Same chain
                        diffResId(prevCA.residue.id, a.residue.id) ) { //Residue difference == 1

                    int2 d;
                    d.x = idPrevA;
                    d.y = id;
                    alphaTrace.Add(d);
                }

                prevCA = a;
                idPrevA = id;
            }
            id++;
        }

        if (alphaTrace.Count == 0) {
            return;
        }

        atomToMeshVertex = new Dictionary<UnityMolAtom, List<int>>();

        GameObject currentGO = new GameObject("TubeMesh");
        currentGO.transform.parent = repParent;
        currentGO.transform.localPosition = Vector3.zero;
        currentGO.transform.localScale = Vector3.one;
        currentGO.transform.localRotation = Quaternion.identity;

        if (!isRecompute) {
            curMesh = new Mesh();
            curMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();
        List<Color32> newColors = new List<Color32>();
        List<Vector2> newUV = new List<Vector2>();


        foreach (int2 duo in alphaTrace) {
            UnityMolAtom atom1 = selection.atoms[duo.x];
            UnityMolAtom atom2 = selection.atoms[duo.y];

            Vector3 start = atom1.position;
            Vector3 end   = atom2.position;

            if (idFrame != -1) {
                start = selection.extractTrajFramePositions[idFrame][duo.x];
                end = selection.extractTrajFramePositions[idFrame][duo.y];
            }


            Vector3 normal = Vector3.Cross(start, end);
            Vector3 side = Vector3.Cross(normal, end - start);
            side.Normalize();
            normal.Normalize();

            Vector3 a = start + side * (lineWidth / 2);
            Vector3 b = start - side * (lineWidth / 2);
            Vector3 c = end + side * (lineWidth / 2);
            Vector3 d = end - side * (lineWidth / 2);

            Vector3 a1 = a + normal * (lineWidth / 2);
            Vector3 a2 = a - normal * (lineWidth / 2);
            Vector3 b1 = b + normal * (lineWidth / 2);
            Vector3 b2 = b - normal * (lineWidth / 2);
            Vector3 c1 = c + normal * (lineWidth / 2);
            Vector3 c2 = c - normal * (lineWidth / 2);
            Vector3 d1 = d + normal * (lineWidth / 2);
            Vector3 d2 = d - normal * (lineWidth / 2);

            int ida = newVertices.Count;
            newVertices.Add(a1);
            newVertices.Add(a2);
            newVertices.Add(b1);
            newVertices.Add(b2);
            newVertices.Add(c1);
            newVertices.Add(c2);
            newVertices.Add(d1);
            newVertices.Add(d2);


            newTriangles.Add(ida);//a1
            newTriangles.Add(ida + 1); //a2
            newTriangles.Add(ida + 2); //b1

            newTriangles.Add(ida + 2); //b1
            newTriangles.Add(ida + 1); //a2
            newTriangles.Add(ida + 3); //b2

            newTriangles.Add(ida);//a1
            newTriangles.Add(ida + 4); //c1
            newTriangles.Add(ida + 1); //a2

            newTriangles.Add(ida + 4); //c1
            newTriangles.Add(ida + 5); //c2
            newTriangles.Add(ida + 1); //a2

            newTriangles.Add(ida + 6); //d1
            newTriangles.Add(ida + 4); //c1
            newTriangles.Add(ida + 2); //b1

            newTriangles.Add(ida);//a1
            newTriangles.Add(ida + 2); //b1
            newTriangles.Add(ida + 4); //c1


            newTriangles.Add(ida + 2); //b1
            newTriangles.Add(ida + 3); //b2
            newTriangles.Add(ida + 6); //d1

            newTriangles.Add(ida + 6); //d1
            newTriangles.Add(ida + 3); //b2
            newTriangles.Add(ida + 7); //d2

            newTriangles.Add(ida + 7); //d2
            newTriangles.Add(ida + 1); //a2
            newTriangles.Add(ida + 5); //c2

            newTriangles.Add(ida + 1); //a2
            newTriangles.Add(ida + 7); //d2
            newTriangles.Add(ida + 3); //b2

            newTriangles.Add(ida + 4); //c1
            newTriangles.Add(ida + 6); //d1
            newTriangles.Add(ida + 7); //d2

            newTriangles.Add(ida + 7); //d2
            newTriangles.Add(ida + 5); //c2
            newTriangles.Add(ida + 4); //c1

            newColors.Add(atom1.color);//0
            newColors.Add(atom1.color);//1
            newColors.Add(atom1.color);//2
            newColors.Add(atom1.color);//3

            newColors.Add(atom2.color);//4
            newColors.Add(atom2.color);//5
            newColors.Add(atom2.color);//6
            newColors.Add(atom2.color);//7

            newUV.Add(Vector2.one);
            newUV.Add(Vector2.one);
            newUV.Add(Vector2.one);
            newUV.Add(Vector2.one);

            newUV.Add(Vector2.one);
            newUV.Add(Vector2.one);
            newUV.Add(Vector2.one);
            newUV.Add(Vector2.one);


            if (!atomToMeshVertex.ContainsKey(atom1)) {
                atomToMeshVertex[atom1] = new List<int>();
            }
            atomToMeshVertex[atom1].Add(ida);
            atomToMeshVertex[atom1].Add(ida + 1);
            atomToMeshVertex[atom1].Add(ida + 2);
            atomToMeshVertex[atom1].Add(ida + 3);

            if (!atomToMeshVertex.ContainsKey(atom2)) {
                atomToMeshVertex[atom2] = new List<int>();
            }
            atomToMeshVertex[atom2].Add(ida + 4);
            atomToMeshVertex[atom2].Add(ida + 5);
            atomToMeshVertex[atom2].Add(ida + 6);
            atomToMeshVertex[atom2].Add(ida + 7);
        }
        curMesh.SetVertices(newVertices);
        curMesh.SetTriangles(newTriangles, 0);

        curMesh.SetUVs(1, newUV);
        curMesh.RecalculateNormals();

        vertices = newVertices;

        if (!isRecompute) {
            colors = newColors;
            savedColors = new List<Color32>(newColors);
            curMesh.SetColors(newColors);
        }
        else {
            if (nbAtoms == selection.atoms.Count) {
                curMesh.SetColors(colors);
            }
            else {
                curMesh.SetColors(newColors);
                colors = newColors;
                savedColors = new List<Color32>(newColors);
            }

        }

        MeshFilter mf = currentGO.AddComponent<MeshFilter>();
        mf.sharedMesh = curMesh;
        MeshRenderer mr = currentGO.AddComponent<MeshRenderer>();

        if (tubeMat == null) {
            tubeMat = new Material(Shader.Find("Custom/SurfaceVertexColor"));
        }

        mr.sharedMaterial = tubeMat;

        meshGO = currentGO;

    }

    static bool diffResId(int id1, int id2) {
        int diff = 1;
        if (id1 < 0 && id2 > 0) {
            diff = 2;
        }
        if (id2 - id1 > diff) {
            return false;
        }
        return true;
    }

    public void recompute() {

        int savedCount = nbAtoms;
        Transform savedPar = null;
        if (meshGO != null) {
            savedPar = meshGO.transform.parent;
        }

        GameObject.Destroy(meshGO);

        DisplayTubeMesh(savedPar, true);

        nbAtoms = selection.atoms.Count;

    }
    public override void Clean() {
        if (tubeMat != null) {
            GameObject.Destroy(tubeMat);
            tubeMat = null;
        }
        meshGO = null;
        if (atomToMeshVertex != null)
            atomToMeshVertex.Clear();
        atomToMeshVertex = null;
        if (alphaTrace != null)
            alphaTrace.Clear();
        alphaTrace = null;
        if (vertices != null)
            vertices.Clear();
        vertices = null;
        if (colors != null)
            colors.Clear();
        colors = null;
        if (savedColors != null)
            savedColors.Clear();
        savedColors = null;
    }

}
}