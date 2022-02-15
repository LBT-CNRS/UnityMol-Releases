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
using UnityEngine.Rendering;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine.XR;

using VRTK;

namespace UMol {

public class AtomRepresentationSugarRibbons : AtomRepresentation {

    public List<GameObject> meshesGO;
    public Dictionary<UnityMolAtom, List<int>> atomToVertBB;
    public bool createPlanes;
    private Material ribbonMat;
    private Material ribbonMatPlane;
    private string structureName;
    private GameObject newRep;

    public AtomRepresentationSugarRibbons(string structName, UnityMolSelection sel, bool planes = true) {

        colorationType = colorType.atom;
        meshesGO = new List<GameObject>();

        ribbonMat = new Material(Shader.Find("Custom/SurfaceVertexColor"));
        ribbonMatPlane = new Material(Shader.Find("Custom/SurfaceVertexColorTransparent"));
        ribbonMatPlane.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);

        structureName = structName;
        selection = sel;
        createPlanes = planes;

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

        newRep = new GameObject("AtomSugarRibbonsRepresentation");
        newRep.transform.parent = representationParent;
        representationTransform = newRep.transform;

        displaySugarRibbonsMesh(structName, newRep.transform);

        newRep.transform.localPosition = Vector3.zero;
        newRep.transform.localRotation = Quaternion.identity;
        newRep.transform.localScale = Vector3.one;

        nbAtoms = selection.Count;
    }

    public void displaySugarRibbonsMesh(string structName, Transform repParent, float ribbonThick = 0.2f,
                                        float ribbonHeight = 0.35f, bool isTraj = false) {

        List<Mesh> meshes = SugarRibbons.createSugarRibbons(selection, ref atomToVertBB, ribbonThick, ribbonHeight, createPlanes);

        if (meshes.Count == 2) {

            GameObject goPlane = new GameObject(structName + "_SugarRibbonsPlanesMesh");
            MeshFilter mfp = goPlane.AddComponent<MeshFilter>();

            mfp.mesh = meshes[0];
            goPlane.AddComponent<MeshRenderer>().sharedMaterial = ribbonMatPlane;
            goPlane.transform.parent = repParent;
            goPlane.transform.localRotation = Quaternion.identity;
            goPlane.transform.localPosition = Vector3.zero;
            goPlane.transform.localScale = Vector3.one;

            meshesGO.Add(goPlane);

            GameObject go = new GameObject(structName + "_SugarRibbonsMesh");
            MeshFilter mf = go.AddComponent<MeshFilter>();

            mf.mesh = meshes[1];
            go.AddComponent<MeshRenderer>().sharedMaterial = ribbonMat;
            go.transform.parent = repParent;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            meshesGO.Add(go);

        }
    }

    public void Clear() {
        foreach (GameObject go in meshesGO) {
            GameObject.Destroy(go);
        }
        meshesGO.Clear();
        atomToVertBB.Clear();
    }

    public void recompute(bool isNewModel = false) {

    }
    public override void Clean(){
        Clear();
    }
}
}