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
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR;
using System.Runtime.InteropServices;

using System;
using System.Text;

using Xenu.Game;

namespace UMol {

#if !DISABLE_HIGHLIGHT
public class UnityMolHighlightManager {


    // [DllImport("ComputeHighlightMesh")]
    // public static extern void computeHighlightMesh(Vector3[] modelVertices, int nbVert, int[] modelTri, int nbTri,
    //         float rFac, Vector3[] positions, float[] radii, int N, Vector3[] outVert, int[] outTri);

    /// <summary>
    /// Dictionary of GameObject holding the highlight meshes for each loaded molecule
    /// </summary>
    public Dictionary<UnityMolStructure, GameObject> highlightDict = new Dictionary<UnityMolStructure, GameObject>();

    // private bool firstTry = true;
    private bool useCPP;

    private UnityMolSelectionManager selM;

    private Mesh sphereMesh;
    private Vector3[] sphereVertices;
    private int[] sphereTriangles;
    private Vector3[] sphereNormals;

    public float highlightScaleFactor = 1.0f;

    public Material highlightMat;
    Camera mainCam;

    private UnityMolSelection saveSel;

    UnityOutlineManager[] outlineManagers;


    void getSphereVertices(bool cube = false) {
        GameObject tmpSphere = null;
        if (cube) {
            tmpSphere = GameObject.CreatePrimitive (PrimitiveType.Cube);
            // sphereMesh = tmpSphere.GetComponent<MeshFilter>().sharedMesh;
            sphereMesh = IcoSphereCreator.Create(1, 1.0f);

        }
        else {
            sphereMesh = IcoSphereCreator.Create(2, 1.0f);
        }
        sphereTriangles = sphereMesh.triangles;
        sphereVertices = sphereMesh.vertices;
        sphereNormals = sphereMesh.normals;

        mainCam = Camera.main;

        if (cube) {
#if UNITY_EDITOR
            GameObject.DestroyImmediate(tmpSphere);
#else
            GameObject.Destroy(tmpSphere);
#endif
        }
    }

    public void changeMaterial(Material newMat) {
        highlightMat = newMat;
        foreach (GameObject go in highlightDict.Values) {
            if (go != null) {
                go.GetComponent<MeshRenderer>().sharedMaterial = newMat;
            }
        }
    }

    public void HighlightAtoms(UnityMolSelection selection) {
        HighlightAtomsMerge(selection);
    }

    public void HighlightAtomsMerge(UnityMolSelection selection) {

        if (sphereVertices == null) {
            getSphereVertices();
        }

        Clean();

        Dictionary<UnityMolStructure, List<UnityMolAtom>> atomsPerStruc = new Dictionary<UnityMolStructure, List<UnityMolAtom>>();

        if (mainCam == null) {
            mainCam = Camera.main;
        }
        if (outlineManagers == null) {
            outlineManagers = GameObject.FindObjectsOfType<UnityOutlineManager>();
        }

        if (outlineManagers != null) {
            foreach (var om in outlineManagers) {
                om.ClearOutlines();
            }
        }

        if (mainCam.GetComponent<UnityOutlineManager>() != null)
            mainCam.GetComponent<UnityOutlineManager>().ClearOutlines();
        else {
            var fx = mainCam.gameObject.AddComponent<UnityOutlineFX>();
            mainCam.gameObject.AddComponent<UnityOutlineManager>().outlinePostEffect = fx;
        }

        //For Wall sized display Stereo mode
        if (mainCam.transform.parent != null &&  mainCam.transform.parent.name == "Cyclop") {
            GameObject camL = mainCam.transform.parent.Find("CameraUMolXL").gameObject;
            if (camL != null) {
                camL.GetComponent<UnityOutlineManager>().ClearOutlines();
            }
        }

        float rhigh = 1.05f / highlightScaleFactor;


        bool isGlobalSel = false;
        if (selection.forceGlobalSelection || selection.structures.Count > 1) {
            isGlobalSel = true;
        }

        if (isGlobalSel) {
            //Fill lists of atoms per structure
            foreach (UnityMolStructure s in selection.structures) {
                atomsPerStruc[s] = new List<UnityMolAtom>();
                foreach (UnityMolAtom a in selection.atoms) {
                    if (a.residue.chain.model.structure == s) {
                        atomsPerStruc[s].Add(a);
                    }
                }
            }
        }

        foreach (UnityMolStructure s in selection.structures) {
            List<UnityMolAtom> atoms = null;

            if (isGlobalSel) {
                atoms = atomsPerStruc[s];
            }
            else {
                atoms = selection.atoms;
            }

            CombineInstance[] combine = new CombineInstance[atoms.Count];
            int id = 0;

            Transform par = UnityMolMain.getStructureManager().GetStructureGameObject(s.name).transform;
            GameObject tmp = new GameObject("HighlightAtoms");

            tmp.transform.localPosition = Vector3.zero;
            tmp.transform.localScale = Vector3.one;
            tmp.transform.localRotation = Quaternion.identity;

            foreach (UnityMolAtom a in atoms) {
                tmp.transform.parent = par;
                tmp.transform.localPosition = a.position;
                tmp.transform.localScale = Vector3.one * a.radius * rhigh;

                combine[id].mesh = sphereMesh;
                combine[id++].transform = tmp.transform.localToWorldMatrix;
            }

            MeshFilter mf = tmp.AddComponent<MeshFilter>();
            Mesh currentMesh = new Mesh();
            currentMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            currentMesh.CombineMeshes(combine);
            mf.sharedMesh = currentMesh;

            if (highlightMat == null) {
                highlightMat = (Material)Resources.Load("Materials/invisible");
            }

            MeshRenderer mr = tmp.AddComponent<MeshRenderer>();
            mr.material = highlightMat;
            tmp.transform.parent = null;
            tmp.transform.localPosition = Vector3.zero;
            tmp.transform.localScale = Vector3.one;
            tmp.transform.localRotation = Quaternion.identity;
            tmp.transform.parent = par;


            if (outlineManagers != null) {
                foreach (var om in outlineManagers) {
                    om.AddOutline(mr);
                }
            }

            if (mainCam.GetComponent<UnityOutlineManager>() != null)
                mainCam.GetComponent<UnityOutlineManager>().AddOutline(mr);
            else {
                var fx = mainCam.gameObject.AddComponent<UnityOutlineFX>();
                mainCam.gameObject.AddComponent<UnityOutlineManager>().outlinePostEffect = fx;
            }

            //For Wall sized display Stereo mode
            if (mainCam.transform.parent != null &&  mainCam.transform.parent.name == "Cyclop") {
                GameObject camL = mainCam.transform.parent.Find("CameraUMolXL").gameObject;
                if (camL != null) {
                    if (mainCam.GetComponent<UnityOutlineManager>() != null)
                        camL.GetComponent<UnityOutlineManager>().AddOutline(mr);
                    else {
                        var fx = mainCam.gameObject.AddComponent<UnityOutlineFX>();
                        mainCam.gameObject.AddComponent<UnityOutlineManager>().outlinePostEffect = fx;
                    }
                }
            }

            highlightDict[s] = tmp;
        }

        saveSel = selection;
    }


    // public void show(bool sh = true) {
    //     foreach (UnityMolStructure s in highlightDict.Keys) {
    //         GameObject go = highlightDict[s];
    //         go.GetComponent<MeshRenderer>().enabled = sh;
    //     }

    public void Clean() {
        foreach (GameObject go in highlightDict.Values) {
#if UNITY_EDITOR
            GameObject.DestroyImmediate(go.GetComponent<MeshFilter>().sharedMesh);
            GameObject.DestroyImmediate(go);
#else
            GameObject.Destroy(go.GetComponent<MeshFilter>().sharedMesh);
            GameObject.Destroy(go);
#endif
        }
        highlightDict.Clear();
    }
}

#endif
}