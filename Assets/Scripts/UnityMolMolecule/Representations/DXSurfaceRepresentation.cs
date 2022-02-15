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
using System.Linq;
using UnityEngine.XR;
using VRTK;

namespace UMol {
public class DXSurfaceRepresentation : ISurfaceRepresentation {

    private DXReader dxR;
    public float isoValue = 0.0f;
    public MarchingCubesWrapper mcWrapper;

    public DXSurfaceRepresentation(string structName, UnityMolSelection sel, DXReader dx, float iso) {
        colorationType = colorType.full;
        if (dx == null) {
            throw new System.Exception("No DX map loaded");
        }
        mcWrapper = new MarchingCubesWrapper();

        isStandardSurface = false;

        dxR = dx;
        isoValue = iso;
        selection = sel;

        meshesGO = new List<GameObject>();
        meshColors = new Dictionary<GameObject, List<Color32>>();
        colorByAtom = new Dictionary<UnityMolAtom, Color32>();
        atomToGo = new Dictionary<UnityMolAtom, GameObject>();
        atomToMesh = new Dictionary<UnityMolAtom, List<int>>();

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

        newRep = new GameObject("AtomDXSurfaceRepresentation");
        newRep.transform.parent = representationParent;
        representationTransform = newRep.transform;

        subSelections = cutSelection(selection);

        mcWrapper.Init(dxR.densityValues, dxR.gridSize, dxR.origin, dxR.delta.x);

        foreach (UnityMolSelection s in subSelections) {
            displayDXSurfaceMesh(s.name + "_DXSurface", s, newRep.transform);
            if (meshesGO.Count > 0) {
                computeNearestVertexPerAtom(meshesGO.Last(), s);
            }
        }

        getMeshColors();

        Color32 white = Color.white;
        foreach (UnityMolAtom a in selection.atoms) {
            colorByAtom[a] = white;
        }

        newRep.transform.localPosition = Vector3.zero;
        newRep.transform.localRotation = Quaternion.identity;
        newRep.transform.localScale = Vector3.one;

        nbAtoms = selection.Count;

    }

    private void displayDXSurfaceMesh(string name, UnityMolSelection selection, Transform repParent) {

        GameObject go = createDXSurface(name, repParent);
        if (go != null) {
            meshesGO.Add(go);
            foreach (UnityMolAtom a in selection.atoms) {
                atomToGo.Add(a, go);
            }
        }
    }

    GameObject createDXSurface(string name, Transform repParent) {

        // MeshData mdata = MarchingCubesWrapper.callMarchingCubes(dxR.densityValues, dxR.gridSize, isoValue);
        MeshData mdata = mcWrapper.computeMC(isoValue);
        // MeshData mdata = null;

        if (mdata != null) {

            if (mcWrapper.mcMode != MarchingCubesWrapper.MCType.CJOB) {
                mdata.Scale(dxR.delta);
                mdata.InvertX();
                mdata.Offset(dxR.origin);
            }

            Mesh newMesh = new Mesh();
            newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            newMesh.vertices = mdata.vertices;
            newMesh.triangles = mdata.triangles;
            newMesh.normals = mdata.normals;
            newMesh.colors32 = mdata.colors;

            // newMesh.RecalculateNormals();

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject.Destroy(go.GetComponent<BoxCollider>());
            go.GetComponent<MeshFilter>().mesh = newMesh;
            go.transform.SetParent(repParent);

            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            go.name = name;
            Material mat = new Material(Shader.Find("Custom/SurfaceVertexColor"));
            mat.SetFloat("_Glossiness", 0.0f);
            mat.SetFloat("_Metallic", 0.0f);
            mat.SetFloat("_AOIntensity", 0.0f);
            mat.SetFloat("_AOPower", 0.0f);
            go.GetComponent<MeshRenderer>().material = mat;

            return go;
        }
        else {
            return null;
        }
    }


    public override void recompute() {


        List<Material> savedMat = new List<Material>();
        foreach (GameObject m in meshesGO) {
            savedMat.Add(m.GetComponent<MeshRenderer>().sharedMaterial);
        }

        Clear();

        foreach (UnityMolSelection sel in subSelections) {
            displayDXSurfaceMesh(sel.name, sel, newRep.transform);
            if (meshesGO.Count > 0) {
                computeNearestVertexPerAtom(meshesGO.Last(), sel);
            }
        }

        if (meshesGO.Count > 0 && meshesGO.Count == savedMat.Count) {
            int i = 0;
            foreach (GameObject m in meshesGO) {
                m.GetComponent<MeshRenderer>().sharedMaterial = savedMat[i++];
            }
        }

        getMeshColors();
    }

    public override void Clean() {

        Clear();
        if (mcWrapper != null) {
            mcWrapper.FreeMC();
        }
        mcWrapper = null;

    }
}
}
