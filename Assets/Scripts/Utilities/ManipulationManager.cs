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
using UnityEngine.UI;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using System.Linq;

namespace UMol {

public class ManipulationManager : MonoBehaviour {

    public Transform currentTransform;

    public float moveSpeed = 5.0f;
    public float scrollSpeed = 5.0f;

    public float speedX = 1.0f;
    public float speedY = 1.0f;
    public float speedZ = 1.0f;

    public bool rotateX = false;
    public bool rotateY = false;
    public bool rotateZ = false;

    public Vector3 currentCenterPosition = Vector3.zero;

    public bool disableMouseInVR = true;

    private Transform loadedMolPar;
    private MouseOverSelection mouseSel;

    void Start() {
        mouseSel = GetComponent<MouseOverSelection>();
    }


    public void resetPosition() {
        currentTransform.position = Vector3.zero;
        UnityMolMain.getCustomRaycast().needsUpdatePos = true;
    }
    public void resetRotation() {
        currentTransform.rotation = Quaternion.identity;
        UnityMolMain.getCustomRaycast().needsUpdatePos = true;
    }
    public void setSelectedOject(Transform seleT) {
        currentTransform = seleT;
    }

    public void centerOnStructure(UnityMolStructure s, bool lerp) {

        if (lerp) {
            centerOnStructureLerp(s);
        }
        else {
            Transform tpar = UnityMolMain.getRepresentationParent().transform;
            Vector3 bary = s.currentModel.centerOfGravity;
            Transform molPar = tpar.Find(s.ToSelectionName());
            Vector3 worldBary = molPar.TransformPoint(bary);
            if (!UnityMolMain.inVR()) {
                tpar.Translate(-worldBary, Space.World);
                setRotationCenter(molPar.TransformPoint(bary));
            }
            else {
                tpar.Translate(-worldBary, Space.World);

                Transform head = VRTK.VRTK_DeviceFinder.HeadsetCamera();

                if (head != null) {
                    Vector3 targetPos = head.position + head.forward;
                    tpar.Translate(targetPos, Space.World);
                }
                setRotationCenter(molPar.TransformPoint(bary));
            }
            UnityMolMain.getCustomRaycast().needsUpdatePos = true;
        }
    }

    public void centerOnStructureLerp(UnityMolStructure s) {
        Transform tpar = UnityMolMain.getRepresentationParent().transform;
        Vector3 bary = s.currentModel.centerOfGravity;
        Transform molPar = tpar.Find(s.ToSelectionName());
        StartCoroutine(delayedCenterOnStructure(tpar, bary, molPar));
    }
    public void setRotationCenter(Vector3 pos) {
        currentCenterPosition = pos;
    }

    IEnumerator delayedCenterOnStructure(Transform tpar, Vector3 bary, Transform molPar) {
        //End of frame
        yield return 0;



        Vector3 worldBary = molPar.TransformPoint(bary);

        Vector3 savedPos = tpar.position;
        tpar.Translate(-worldBary, Space.World);


        if (UnityMolMain.inVR()) {
            Transform head = VRTK.VRTK_DeviceFinder.HeadsetCamera();
            if (head != null) {
                Vector3 headTarget = head.position + head.forward;
                tpar.Translate(headTarget, Space.World);
            }
        }

        Vector3 targetPos = tpar.position;
        tpar.position = savedPos;

        int steps = 200;
        for (int i = 1; i < steps / 4; i++) {
            float tt = i / (float)steps;
            tpar.position = Vector3.Lerp(tpar.position, targetPos, tt);
            yield return 0;
        }

        setRotationCenter(molPar.TransformPoint(bary));

        UnityMolMain.getCustomRaycast().needsUpdatePos = true;

    }


    //Center on selection
    public void centerOnSelection(UnityMolSelection sel, bool lerp, float distance = -1.0f) {

        if (lerp) {
            centerOnSelectionLerp(sel, distance);
        }
        else {
            Transform tpar = UnityMolMain.getRepresentationParent().transform;
            Vector3 bary = computeCenterOfGravitySel(sel);
            Transform molPar = getSelectionParent(tpar, sel);
            Vector3 worldBary = molPar.TransformPoint(bary);
            if (!UnityMolMain.inVR()) {
                tpar.Translate(-worldBary, Space.World);
                if (distance > 0.0f) {
                    float dist = Vector3.Distance(transform.position, tpar.position);
                    tpar.Translate(new Vector3(0.0f, 0.0f, - dist + distance), Space.World);
                }

                // tpar.Translate(new Vector3(0.0f, 0.0f, -distance) , Space.World);
                setRotationCenter(molPar.TransformPoint(bary));
            }
            else {
                tpar.Translate(-worldBary, Space.World);

                Transform head = VRTK.VRTK_DeviceFinder.HeadsetCamera();

                if (head != null) {
                    Vector3 targetPos = head.position + (head.forward * distance);
                    tpar.Translate(targetPos, Space.World);
                }
                setRotationCenter(molPar.TransformPoint(bary));
                UnityMolMain.getCustomRaycast().needsUpdatePos = true;
            }

        }
    }

    public void centerOnSelectionLerp(UnityMolSelection sel, float distance) {
        Transform tpar = UnityMolMain.getRepresentationParent().transform;
        Vector3 bary = computeCenterOfGravitySel(sel);
        Transform molPar = getSelectionParent(tpar, sel);
        StartCoroutine(delayedCenterOnSelection(tpar, bary, molPar, distance));
    }

    IEnumerator delayedCenterOnSelection(Transform tpar, Vector3 bary, Transform molPar, float distance) {
        //End of frame
        yield return 0;

        Vector3 worldBary = molPar.TransformPoint(bary);

        Vector3 savedPos = tpar.position;
        tpar.Translate(-worldBary, Space.World);
        if (distance > 0.0f) {
            float dist = Vector3.Distance(transform.position, tpar.position);
            tpar.Translate(new Vector3(0.0f, 0.0f, - dist + distance), Space.World);
        }


        if (UnityMolMain.inVR()) {
            Transform head = VRTK.VRTK_DeviceFinder.HeadsetCamera();
            if (head != null) {
                Vector3 headTarget = head.position + head.forward;
                tpar.Translate(headTarget, Space.World);
            }
        }

        Vector3 targetPos = tpar.position;
        tpar.position = savedPos;

        int steps = 200;
        for (int i = 1; i < steps / 4; i++) {
            float tt = i / (float)steps;
            tpar.position = Vector3.Lerp(tpar.position, targetPos, tt);
            yield return 0;
        }

        setRotationCenter(molPar.TransformPoint(bary));
        UnityMolMain.getCustomRaycast().needsUpdatePos = true;
    }

    public void emulateLookAtPOV(UnityMolSelection sel, Vector3 pov, Vector3 targetPos, bool lerp = false) {
        Transform tpar = UnityMolMain.getRepresentationParent().transform;
        Transform molPar = getSelectionParent(tpar, sel);
        Vector3 localTarget = molPar.InverseTransformPoint(targetPos);
        Vector3 localPov = molPar.InverseTransformPoint(pov);

        GameObject go1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go1.transform.position = targetPos;
        go1.transform.localScale = Vector3.one * 0.1f;
        go1.transform.parent = molPar;

        GameObject go2 = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go2.transform.position = pov;
        go2.transform.localScale = Vector3.one * 0.1f;
        go2.transform.parent = molPar;


        if (!UnityMolMain.inVR()) {
            tpar.Translate(-targetPos, Space.World);
            float dist = Vector3.Distance(transform.position, molPar.TransformPoint(localTarget));
            float distPovTarget = Vector3.Distance(pov, targetPos);
            tpar.Translate(new Vector3(0.0f, 0.0f, - dist + distPovTarget), Space.World);

            Vector3 fromVec = transform.position - molPar.TransformPoint(localTarget);
            Vector3 toVec = molPar.TransformPoint(localPov) - molPar.TransformPoint(localTarget);
            Quaternion fromTo = Quaternion.FromToRotation(fromVec, toVec);
            // tpar.rotation = tpar.rotation * fromTo;
            float angle = 0.0f;
            Vector3 axis = Vector3.zero;
            fromTo.ToAngleAxis(out angle, out axis);

            tpar.RotateAround(molPar.TransformPoint(localTarget), axis, angle);


        }
        else {

        }

        setRotationCenter(molPar.TransformPoint(localTarget));
        // Debug.Log("New current center = " + currentCenterPosition.ToString("F4"));
        UnityMolMain.getCustomRaycast().needsUpdatePos = true;
    }


    public static Vector3 computeCenterOfGravitySel(UnityMolSelection sel) {
        Vector3 pos = Vector3.zero;
        foreach (UnityMolAtom a in sel.atoms) {
            pos += a.position;
        }
        pos /= sel.atoms.Count;
        return pos;
    }
    Transform getSelectionParent(Transform allParent, UnityMolSelection sel) {
        if (sel.structures.Count == 1) {
            return allParent.Find(sel.structures[0].ToSelectionName());
        }
        else if (sel.structures.Count > 1) {
            return allParent;
        }
        return null;
    }


    void Update ()
    {

        if (loadedMolPar == null) {
            loadedMolPar = UnityMolMain.getRepresentationParent().transform;
        }
        GameObject curUIInput = EventSystem.current.currentSelectedGameObject;

        if (rotateX) {
            foreach (UnityMolStructure s in UnityMolMain.getStructureManager().loadedStructures) {
                Transform t = UnityMolMain.getStructureManager().structureToGameObject[s.uniqueName].transform;
                t.RotateAround(t.TransformPoint(s.currentModel.centerOfGravity), loadedMolPar.right, speedX);
            }
            UnityMolMain.getCustomRaycast().needsUpdatePos = true;
        }
        if (rotateY) {
            foreach (UnityMolStructure s in UnityMolMain.getStructureManager().loadedStructures) {
                Transform t = UnityMolMain.getStructureManager().structureToGameObject[s.uniqueName].transform;
                t.RotateAround(t.TransformPoint(s.currentModel.centerOfGravity), loadedMolPar.up, speedY);
            }
            UnityMolMain.getCustomRaycast().needsUpdatePos = true;
        }
        if (rotateZ) {
            foreach (UnityMolStructure s in UnityMolMain.getStructureManager().loadedStructures) {
                Transform t = UnityMolMain.getStructureManager().structureToGameObject[s.uniqueName].transform;
                t.RotateAround(t.TransformPoint(s.currentModel.centerOfGravity), loadedMolPar.forward, speedZ);
            }
            UnityMolMain.getCustomRaycast().needsUpdatePos = true;
        }
        if(curUIInput == null && Input.GetButton("RotationXLeft")){
            foreach (UnityMolStructure s in UnityMolMain.getStructureManager().loadedStructures) {
                Transform t = UnityMolMain.getStructureManager().structureToGameObject[s.uniqueName].transform;
                t.RotateAround(t.TransformPoint(s.currentModel.centerOfGravity), loadedMolPar.right, speedX);
            }
            UnityMolMain.getCustomRaycast().needsUpdatePos = true;
        }
        if(curUIInput == null && Input.GetButton("RotationXRight")){
            foreach (UnityMolStructure s in UnityMolMain.getStructureManager().loadedStructures) {
                Transform t = UnityMolMain.getStructureManager().structureToGameObject[s.uniqueName].transform;
                t.RotateAround(t.TransformPoint(s.currentModel.centerOfGravity), -loadedMolPar.right, speedX);
            }
            UnityMolMain.getCustomRaycast().needsUpdatePos = true;
        }
        if(curUIInput == null && Input.GetButton("RotationYLeft")){
            foreach (UnityMolStructure s in UnityMolMain.getStructureManager().loadedStructures) {
                Transform t = UnityMolMain.getStructureManager().structureToGameObject[s.uniqueName].transform;
                t.RotateAround(t.TransformPoint(s.currentModel.centerOfGravity), loadedMolPar.up, speedY);
            }
            UnityMolMain.getCustomRaycast().needsUpdatePos = true;
        }
        if(curUIInput == null && Input.GetButton("RotationYRight")){
            foreach (UnityMolStructure s in UnityMolMain.getStructureManager().loadedStructures) {
                Transform t = UnityMolMain.getStructureManager().structureToGameObject[s.uniqueName].transform;
                t.RotateAround(t.TransformPoint(s.currentModel.centerOfGravity), -loadedMolPar.up, speedY);
            }
            UnityMolMain.getCustomRaycast().needsUpdatePos = true;
        }
        if(curUIInput == null && Input.GetButton("RotationZLeft")){
            foreach (UnityMolStructure s in UnityMolMain.getStructureManager().loadedStructures) {
                Transform t = UnityMolMain.getStructureManager().structureToGameObject[s.uniqueName].transform;
                t.RotateAround(t.TransformPoint(s.currentModel.centerOfGravity), loadedMolPar.forward, speedZ);
            }
            UnityMolMain.getCustomRaycast().needsUpdatePos = true;
        }
        if(curUIInput == null && Input.GetButton("RotationZRight")){
            foreach (UnityMolStructure s in UnityMolMain.getStructureManager().loadedStructures) {
                Transform t = UnityMolMain.getStructureManager().structureToGameObject[s.uniqueName].transform;
                t.RotateAround(t.TransformPoint(s.currentModel.centerOfGravity), -loadedMolPar.forward, speedZ);
            }
            UnityMolMain.getCustomRaycast().needsUpdatePos = true;
        }

        if(curUIInput == null && Input.GetButton("ZoomIn")){
            Vector3 pos = currentTransform.position;
            float val = moveSpeed * 0.1f;
            pos.z += val;
            currentTransform.position = pos;
            currentCenterPosition.z += val;
            UnityMolMain.getCustomRaycast().needsUpdatePos = true;
        }
        if(curUIInput == null && Input.GetButton("ZoomOut")){
            Vector3 pos = currentTransform.position;
            float val = moveSpeed * 0.1f;
            pos.z -= val;
            currentTransform.position = pos;
            currentCenterPosition.z -= val;
            UnityMolMain.getCustomRaycast().needsUpdatePos = true;
        }

        if (disableMouseInVR && UnityMolMain.inVR()) {
            return;
        }
        if (currentTransform == null)
            setSelectedOject(UnityMolMain.getRepresentationParent().transform);

        if (currentTransform == null)
            return;

        if (Input.GetMouseButton(0) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) {
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
                if (mouseSel == null || !mouseSel.duringIMD) {
                    currentTransform.RotateAround(currentCenterPosition, Vector3.up, -Input.GetAxis("Mouse X")*moveSpeed);
                    currentTransform.RotateAround(currentCenterPosition, Vector3.right, Input.GetAxis("Mouse Y")*moveSpeed);
                    UnityMolMain.getCustomRaycast().needsUpdatePos = true;
                }
            }
        }
        if (Input.GetMouseButton(1) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) {
            Vector3 pos = currentTransform.position;
            float val = -Input.GetAxis("Mouse Y") * moveSpeed * 0.5f;
            pos.z += val;
            currentTransform.position = pos;
            currentCenterPosition.z += val;
            UnityMolMain.getCustomRaycast().needsUpdatePos = true;
        }
        if (Input.GetMouseButton(2) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) {
            currentTransform.Translate(Vector3.up * Input.GetAxis("Mouse Y")*moveSpeed * 0.05f, Space.World);
            currentTransform.Translate(Vector3.right * Input.GetAxis("Mouse X")*moveSpeed * 0.05f, Space.World);
            currentCenterPosition.x += Input.GetAxis("Mouse X") * moveSpeed * 0.05f;
            currentCenterPosition.y += Input.GetAxis("Mouse Y") * moveSpeed * 0.05f;
            UnityMolMain.getCustomRaycast().needsUpdatePos = true;
        }

        float scroll = -Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
        if (scroll != 0.0f && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
            Vector3 pos = currentTransform.position;
            pos.z += scroll * moveSpeed;
            currentTransform.position = pos;
            currentCenterPosition.z += scroll * moveSpeed;
            UnityMolMain.getCustomRaycast().needsUpdatePos = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.R)) {
            resetPosition();
            resetRotation();
        }

    }
}
}