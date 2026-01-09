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
using Unity.Mathematics;


namespace UMol {
public class BondRepresentationHbondsTube : BondRepresentation {

    public List<GameObject> meshesGO;
    public Mesh curMesh;
    public Dictionary<UnityMolAtom, List<GameObject>> atomToGo;
    public Dictionary<UnityMolAtom, List<Mesh>> atomToMeshes;
    public Dictionary<UnityMolAtom, List<int>> atomToVertices;

    public UnityMolBonds hbonds;
    private GameObject newRep;
    private Material hbondMat;

    public bool isCustomHbonds = false;

    public float tubeHeight = 0.2f;
    public float spaceBetweenTubes = 0.55f;
    public float tubeRadius = 0.1f;

    /// If customHbonds is true, use the bonds from the selection,
    /// else run hbond detection algorithm
    public BondRepresentationHbondsTube(int idF, string structName, UnityMolSelection sel, bool customHbonds = false) {
        colorationType = colorType.full;

        representationParent = UnityMolMain.getRepStructureParent(structName).transform;

        newRep = new GameObject("BondHbondRepresentationTube");
        newRep.transform.parent = representationParent;
        representationTransform = newRep.transform;


        if (hbondMat == null)
            hbondMat = new Material(Shader.Find("Custom/SurfaceVertexColor"));

        // AnimateHbonds anim = newRep.AddComponent<AnimateHbonds>();
        // anim.hbondMat = hbondMat;


        selection = sel;
        idFrame = idF;



        if (customHbonds == false) {
            hbonds = HbondDetection.DetectHydrogenBonds(sel, idFrame, selection.atomToIdInSel);
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

        Mesh tubeMesh = ProceduralTube.createTube(tubeHeight, tubeRadius);
        Vector3[] tubeMeshV = tubeMesh.vertices;
        Vector3[] tubeMeshN = tubeMesh.normals;
        int[] tubeMeshT = tubeMesh.triangles;
        Color32 white = new Color32(255, 255, 255, 255);


        UnityMolModel curM = selection.atoms[0].residue.chain.model;

        HashSet<int2> doneBonds = new HashSet<int2>();
        int2 k, invk;
        foreach (int ida in hbonds.bonds.Keys) {
            UnityMolAtom atom1 = curM.allAtoms[ida];
            foreach (int idb in hbonds.bonds[ida]) {
                if (idb != -1) {
                    k.x = ida; invk.x = idb;
                    k.y = idb; invk.y = ida;
                    if (doneBonds.Contains(k) || doneBonds.Contains(invk))
                        continue;

                    doneBonds.Add(k);
                    UnityMolAtom atom2 = curM.allAtoms[idb];

                    // GameObject currentGO = GameObject.Instantiate((GameObject) Resources.Load("Prefabs/HbondPrefab"));
                    // currentGO.name = "BondHBond_" + countBond + "_" + atom1.number + "/" + atom2.number;
                    GameObject currentGO = new GameObject("BondHBond_" + "BondHBond_" + countBond + "_" + atom1.number + "/" + atom2.number);
                    currentGO.transform.parent = repParent;
                    currentGO.transform.localRotation = Quaternion.identity;
                    currentGO.transform.localPosition = Vector3.zero;
                    currentGO.transform.localScale = Vector3.one;

                    if (!atomToVertices.ContainsKey(atom1)) {
                        atomToVertices[atom1] = new List<int>();
                    }
                    if (!atomToVertices.ContainsKey(atom2)) {
                        atomToVertices[atom2]  = new List<int>();
                    }

                    Vector3 a1pos = atom1.position;
                    Vector3 a2pos = atom2.position;

                    if (idFrame != -1) {
                        int iida = selection.atomToIdInSel[atom1];
                        a1pos = selection.extractTrajFramePositions[idFrame][iida];
                        iida = selection.atomToIdInSel[atom2];
                        a2pos = selection.extractTrajFramePositions[idFrame][iida];
                    }

                    float d = Vector3.Distance(a1pos, a2pos);

                    float i = 0.0f;

                    List<Vector3> vertices = new List<Vector3>();
                    List<Vector3> normals = new List<Vector3>();
                    List<int> triangles = new List<int>();
                    List<Color32> colors = new List<Color32>();

                    Vector3 dirLink = (a2pos - a1pos).normalized;
                    //Compute rotation to orient each tube
                    Quaternion rot = Quaternion.FromToRotation(Vector3.up, dirLink);
                    Vector3 curPos = a1pos;

                    int cptTube = 0;
                    while (i < d - tubeHeight) {
                        int startV = vertices.Count;

                        for (int v = 0; v < tubeMeshV.Length; v++) {
                            atomToVertices[atom1].Add(vertices.Count);
                            atomToVertices[atom2].Add(vertices.Count);
                            vertices.Add(curPos + (rot * tubeMeshV[v]));
                            normals.Add(rot * tubeMeshN[v]);
                            colors.Add(white);
                        }
                        for (int t = 0; t < tubeMeshT.Length; t++) {
                            triangles.Add(tubeMeshT[t] + startV);
                        }
                        curPos += dirLink * (tubeHeight + spaceBetweenTubes);
                        i += tubeHeight + spaceBetweenTubes;
                        cptTube++;
                    }

                    float needed = d / (tubeHeight + spaceBetweenTubes);

                    Mesh curMesh = new Mesh();
                    curMesh.SetVertices(vertices);
                    curMesh.SetNormals(normals);
                    curMesh.SetTriangles(triangles, 0);
                    curMesh.SetColors(colors);

                    if (!atomToGo.ContainsKey(atom1)) {
                        atomToGo[atom1] = new List<GameObject>();
                    }

                    if (!atomToGo.ContainsKey(atom2)) {
                        atomToGo[atom2] = new List<GameObject>();
                    }

                    atomToGo[atom1].Add(currentGO);
                    atomToGo[atom2].Add(currentGO);


                    if (!atomToMeshes.ContainsKey(atom1)) {
                        atomToMeshes[atom1] = new List<Mesh>();
                    }
                    if (!atomToMeshes.ContainsKey(atom2)) {
                        atomToMeshes[atom2] = new List<Mesh>();
                    }

                    atomToMeshes[atom1].Add(curMesh);
                    atomToMeshes[atom2].Add(curMesh);


                    MeshFilter mf = currentGO.AddComponent<MeshFilter>();
                    mf.sharedMesh = curMesh;
                    MeshRenderer mr = currentGO.AddComponent<MeshRenderer>();
                    mr.sharedMaterial = hbondMat;


                    meshesGO.Add(currentGO);
                    countBond++;
                }
            }
        }
    }

    public override void Clean() {
        if (meshesGO != null) {//Already destroyed by the manager
            foreach (GameObject go in meshesGO) {
                GameObject.Destroy(go.GetComponent<MeshFilter>().sharedMesh);
                GameObject.Destroy(go);
            }
            meshesGO.Clear();
        }

        GameObject.Destroy(hbondMat);
        hbondMat = null;
        meshesGO = null;

        atomToGo.Clear();
        atomToMeshes.Clear();
        atomToVertices.Clear();
    }

    public void recompute() {
        //Clean
        if (meshesGO != null) {
            foreach (GameObject go in meshesGO) {
                GameObject.Destroy(go.GetComponent<MeshFilter>().sharedMesh);
                GameObject.Destroy(go);
            }
            meshesGO.Clear();
        }

        atomToGo.Clear();
        atomToMeshes.Clear();
        atomToVertices.Clear();

        if (!isCustomHbonds)
            hbonds = HbondDetection.DetectHydrogenBonds(selection, idFrame, selection.atomToIdInSel);
        else
            hbonds = selection.bonds;

        DisplayHBonds(newRep.transform);
    }
    public void recomputeLight() {
        if (meshesGO != null) {
            foreach (GameObject go in meshesGO) {
                GameObject.Destroy(go.GetComponent<MeshFilter>().sharedMesh);
                GameObject.Destroy(go);
            }
            meshesGO.Clear();
        }
        atomToGo.Clear();
        atomToMeshes.Clear();
        atomToVertices.Clear();

        DisplayHBonds(newRep.transform);
    }
}
}
