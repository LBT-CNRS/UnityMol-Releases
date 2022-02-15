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
using UnityEngine.XR;
using VRTK;

namespace UMol {
public class BondRepresentationLine : BondRepresentation {

    public GameObject meshGO;
    public Mesh curMesh;
    public Dictionary<UnityMolAtom, List<int>> atomToMeshVertex;
    public float lineWidth = 0.15f;
    public List<Vector3> vertices;
    public List<Color32> colors;

    public BondRepresentationLine(string structName, UnityMolSelection sel) {
        colorationType = colorType.atom;
        
        GameObject loadedMolGO = UnityMolMain.getRepresentationParent();

        representationParent = loadedMolGO.transform.Find(structName);
        if (UnityMolMain.inVR() && representationParent == null) {

            Transform clref = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController);
            Transform crref = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController);
            if (clref != null) {
                representationParent = clref.Find(structName);
            }
            if (representationParent == null && crref != null) {
                representationParent = crref.Find(structName);
            }
        }
        if (representationParent == null) {
            representationParent = (new GameObject(structName).transform);
            representationParent.parent = loadedMolGO.transform;
            representationParent.localPosition = Vector3.zero;
            representationParent.localRotation = Quaternion.identity;
            representationParent.localScale = Vector3.one;
        }

        GameObject newRep = new GameObject("BondLineRepresentation");
        newRep.transform.parent = representationParent;
        representationTransform = newRep.transform;

        selection = sel;
        
        DisplayLine(newRep.transform);
        // newRep.transform.position -= offset;

        newRep.transform.localPosition = Vector3.zero;
        newRep.transform.localRotation = Quaternion.identity;
        newRep.transform.localScale = Vector3.one;

        nbBonds = selection.bonds.Count;

    }

    private void DisplayLine(Transform repParent) {

        int nbSticks = selection.bonds.Count;
        if (nbSticks == 0)
            return;


        atomToMeshVertex = new Dictionary<UnityMolAtom, List<int>>();

        GameObject currentGO = new GameObject("BondLineMesh");
        currentGO.transform.parent = repParent;

        curMesh = new Mesh();
        curMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();
        List<Color32> newColors = new List<Color32>();
        List<Vector2> newUV = new List<Vector2>();
        foreach (UnityMolAtom atom1 in Dbonds.Keys) {
            for (int at = 0; at < Dbonds[atom1].Length; at++) {
                UnityMolAtom atom2 = Dbonds[atom1][at];
                if (Dbonds[atom1][at] != null) {
                    Vector3 start = atom1.position;
                    Vector3 end   = atom2.position;

                    Vector3 normal = Vector3.Cross(start, end);
                    Vector3 side = Vector3.Cross(normal, end-start);
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
                    newTriangles.Add(ida+1);//a2
                    newTriangles.Add(ida+2);//b1

                    newTriangles.Add(ida+2);//b1
                    newTriangles.Add(ida+1);//a2
                    newTriangles.Add(ida+3);//b2

                    newTriangles.Add(ida);//a1
                    newTriangles.Add(ida+4);//c1
                    newTriangles.Add(ida+1);//a2

                    newTriangles.Add(ida+4);//c1
                    newTriangles.Add(ida+5);//c2
                    newTriangles.Add(ida+1);//a2

                    newTriangles.Add(ida+6);//d1
                    newTriangles.Add(ida+4);//c1
                    newTriangles.Add(ida+2);//b1

                    newTriangles.Add(ida);//a1
                    newTriangles.Add(ida+2);//b1
                    newTriangles.Add(ida+4);//c1


                    newTriangles.Add(ida+2);//b1
                    newTriangles.Add(ida+3);//b2
                    newTriangles.Add(ida+6);//d1

                    newTriangles.Add(ida+6);//d1
                    newTriangles.Add(ida+3);//b2
                    newTriangles.Add(ida+7);//d2

                    newTriangles.Add(ida+7);//d2
                    newTriangles.Add(ida+1);//a2
                    newTriangles.Add(ida+5);//c2

                    newTriangles.Add(ida+1);//a2
                    newTriangles.Add(ida+7);//d2
                    newTriangles.Add(ida+3);//b2

                    newTriangles.Add(ida+4);//c1
                    newTriangles.Add(ida+6);//d1
                    newTriangles.Add(ida+7);//d2

                    newTriangles.Add(ida+7);//d2
                    newTriangles.Add(ida+5);//c2
                    newTriangles.Add(ida+4);//c1

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


                    if(!atomToMeshVertex.ContainsKey(atom1)){
                        atomToMeshVertex[atom1] = new List<int>();
                    }
                    atomToMeshVertex[atom1].Add(ida);
                    atomToMeshVertex[atom1].Add(ida+1);
                    atomToMeshVertex[atom1].Add(ida+2);
                    atomToMeshVertex[atom1].Add(ida+3);

                    if(!atomToMeshVertex.ContainsKey(atom2)){
                        atomToMeshVertex[atom2] = new List<int>();
                    }
                    atomToMeshVertex[atom2].Add(ida+4);
                    atomToMeshVertex[atom2].Add(ida+5);
                    atomToMeshVertex[atom2].Add(ida+6);
                    atomToMeshVertex[atom2].Add(ida+7);


                    //A quad per bond 

                    // int ida = newVertices.Count;
                    // newVertices.Add(a);
                    // newVertices.Add(b);
                    // newVertices.Add(c);
                    // newVertices.Add(d);

                    // newTriangles.Add(ida);
                    // newTriangles.Add(ida+1);//b
                    // newTriangles.Add(ida+2);//c

                    // newTriangles.Add(ida+2);
                    // newTriangles.Add(ida+1);//c
                    // newTriangles.Add(ida+3);//d

                    // newColors.Add(atom1.color);
                    // newColors.Add(atom1.color);
                    // newColors.Add(atom2.color);
                    // newColors.Add(atom2.color);

                    // if(atomToMeshVertex.ContainsKey(atom1)){
                    //     atomToMeshVertex[atom1].Add(ida);
                    //     atomToMeshVertex[atom1].Add(ida+1);
                    // }
                    // else{
                    //     atomToMeshVertex[atom1] = new List<int>();
                    //     atomToMeshVertex[atom1].Add(ida);
                    //     atomToMeshVertex[atom1].Add(ida+1);
                    // }

                    // if(atomToMeshVertex.ContainsKey(atom2)){
                    //     atomToMeshVertex[atom2].Add(ida+2);
                    //     atomToMeshVertex[atom2].Add(ida+3);
                    // }
                    // else{
                    //     atomToMeshVertex[atom2] = new List<int>();
                    //     atomToMeshVertex[atom2].Add(ida+2);
                    //     atomToMeshVertex[atom2].Add(ida+3);
                    // }
                }

            }

        }
        curMesh.SetVertices(newVertices);
        curMesh.SetTriangles(newTriangles, 0);
        curMesh.SetColors(newColors);
        curMesh.SetUVs(1, newUV);
        curMesh.RecalculateNormals();
        curMesh.RecalculateBounds();

        vertices = newVertices;
        colors = newColors;

        MeshFilter mf = currentGO.AddComponent<MeshFilter>();
        mf.mesh = curMesh;
        MeshRenderer mr = currentGO.AddComponent<MeshRenderer>();
        mr.material = new Material(Shader.Find("Unlit/SurfaceVertexColorNotCull"));
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        meshGO = currentGO;

    }
    public override void Clean(){
    }
}
}