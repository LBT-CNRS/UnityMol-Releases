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
public class BondRepresentationHbonds : BondRepresentation {

    public List<GameObject> meshesGO;
    public Mesh curMesh;
    public Dictionary<UnityMolAtom, List<GameObject>> atomToGo;
    public Dictionary<UnityMolAtom, List<Mesh>> atomToMeshes;
    public Dictionary<UnityMolAtom, List<int>> atomToVertices;

    public UnityMolBonds hbonds;
    private GameObject newRep;
    private Material hbondMat;

    public bool isCustomHbonds = false;

    /// If customHbonds is true, use the bonds from the selection,
    /// else run hbond detection algorithm
    public BondRepresentationHbonds(string structName, UnityMolSelection sel, bool customHbonds = false) {

        colorationType = colorType.full;
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



        newRep = new GameObject("BondHbondRepresentation");
        newRep.transform.parent = representationParent;
        representationTransform = newRep.transform;


        hbondMat = Resources.Load("Materials/hbondsTransparentUnlit") as Material;

        AnimateHbonds anim = newRep.AddComponent<AnimateHbonds>();
        anim.hbondMat = hbondMat;


        selection = sel;

        if (customHbonds == false) {
            hbonds = HbondDetection.DetectHydrogenBonds(sel);
        }
        else {
            hbonds = sel.bonds;
            isCustomHbonds = true;
        }

        DisplayHBonds(newRep.transform);

        newRep.transform.localPosition = Vector3.zero;
        newRep.transform.localRotation = Quaternion.identity;
        newRep.transform.localScale = Vector3.one;

        //Don't do that to avoid updating the representation every time showSelection is called
        // nbBonds = hbonds.Count;
        nbBonds = sel.bonds.Count;
    }

    public void DisplayHBonds(Transform repParent) {

        int nbSticks = hbonds.Length;


        meshesGO = new List<GameObject>();
        atomToMeshes = new Dictionary<UnityMolAtom, List<Mesh>>();
        atomToVertices = new Dictionary<UnityMolAtom, List<int>>();
        atomToGo = new Dictionary<UnityMolAtom, List<GameObject>>();


        if (nbSticks == 0)
            return;


        int countBond = 0;

        var keys = hbonds.Dbonds.Keys;

        foreach (UnityMolAtom atom1 in keys) {
            for (int at = 0; at < hbonds.Dbonds[atom1].Length; at++) {

                UnityMolAtom atom2 = hbonds.Dbonds[atom1][at];
                if (hbonds.Dbonds[atom1][at] != null) {

                    // GameObject currentGO = GameObject.Instantiate((GameObject) Resources.Load("Prefabs/HbondPrefab"));
                    // currentGO.name = "BondHBond_" + countBond + "_" + atom1.number + "/" + atom2.number;
                    GameObject currentGO = new GameObject("BondHBond_" + "BondHBond_" + countBond + "_" + atom1.number + "/" + atom2.number);
                    currentGO.transform.parent = repParent;
                    currentGO.transform.localRotation = Quaternion.identity;
                    currentGO.transform.localPosition = Vector3.zero;
                    currentGO.transform.localScale = Vector3.one;

                    Mesh curMesh = createQuadMesh(atom1, atom2);

                    if (!atomToGo.ContainsKey(atom1)) {
                        atomToGo[atom1] = new List<GameObject>();
                    }

                    if (!atomToGo.ContainsKey(atom2)) {
                        atomToGo[atom2] = new List<GameObject>();
                    }

                    atomToGo[atom1].Add(currentGO);
                    atomToGo[atom2].Add(currentGO);



                    MeshFilter mf = currentGO.AddComponent<MeshFilter>();
                    mf.mesh = curMesh;
                    MeshRenderer mr = currentGO.AddComponent<MeshRenderer>();
                    mr.material = hbondMat;


                    meshesGO.Add(currentGO);
                    countBond++;
                }
            }
        }
    }



    Mesh createQuadMesh(UnityMolAtom atom1, UnityMolAtom atom2, float lineWidth = 0.5f) {

        Vector3[] newVertices = new Vector3[4];
        Vector2[] newUV = new Vector2[4];
        Color32[] newColors = new Color32[4];
        int[] newTriangles = new int[6];

        Vector3 start = atom1.position;
        Vector3 end   = atom2.position;

        Vector3 normal = Vector3.Cross(start, end);
        Vector3 side = Vector3.Cross(normal, end - start);
        side.Normalize();

        Vector3 a = start + side * (lineWidth / 2);
        Vector3 b = start - side * (lineWidth / 2);
        Vector3 c = end + side * (lineWidth / 2);
        Vector3 d = end - side * (lineWidth / 2);


        //A quad per bond

        int ida = 0;
        newVertices[0] = a;
        newVertices[1] = b;
        newVertices[2] = c;
        newVertices[3] = d;

        newTriangles[0] = 0;
        newTriangles[1] = 1; //b
        newTriangles[2] = 2; //c

        newTriangles[3] = 2;
        newTriangles[4] = 1; //c
        newTriangles[5] = 3; //d

        newUV[0] = Vector2.zero;
        newUV[1] = new Vector2(0, 1);
        newUV[2] = new Vector2(1, 0);
        newUV[3] = Vector2.one;


        newColors[0] = Color.white;
        newColors[1] = Color.white;
        newColors[2] = Color.white;
        newColors[3] = Color.white;



        Mesh curMesh = new Mesh();

        curMesh.vertices = newVertices;
        curMesh.triangles = newTriangles;
        curMesh.colors32 = newColors;
        curMesh.uv = newUV;
        curMesh.RecalculateNormals();


        if (atomToMeshes.ContainsKey(atom1)) {
            atomToMeshes[atom1].Add(curMesh);
            atomToVertices[atom1].Add(ida);
            atomToVertices[atom1].Add(ida + 1);

        }
        else {
            atomToMeshes[atom1] = new List<Mesh>();
            atomToVertices[atom1] = new List<int>();
            atomToMeshes[atom1].Add(curMesh);
            atomToVertices[atom1].Add(ida);
            atomToVertices[atom1].Add(ida + 1);

        }

        if (atomToMeshes.ContainsKey(atom2)) {
            atomToMeshes[atom2].Add(curMesh);
            atomToVertices[atom2].Add(ida + 2);
            atomToVertices[atom2].Add(ida + 3);

        }
        else {
            atomToMeshes[atom2] = new List<Mesh>();
            atomToVertices[atom2] = new List<int>();
            atomToMeshes[atom2].Add(curMesh);
            atomToVertices[atom2].Add(ida + 2);
            atomToVertices[atom2].Add(ida + 3);

        }
        return curMesh;
    }




    public void Clear() {
        foreach (GameObject go in meshesGO) {
            GameObject.Destroy(go);
        }
        meshesGO.Clear();
        atomToGo.Clear();
        atomToMeshes.Clear();
        atomToVertices.Clear();
    }
    public override void Clean() {
        Clear();
    }

    public void recompute() {
        Clear();
        if (!isCustomHbonds)
            hbonds = HbondDetection.DetectHydrogenBonds(selection);
        else
            hbonds = selection.bonds;

        DisplayHBonds(newRep.transform);
    }
    public void recomputeLight() {
        Clear();
        DisplayHBonds(newRep.transform);
    }
}
}