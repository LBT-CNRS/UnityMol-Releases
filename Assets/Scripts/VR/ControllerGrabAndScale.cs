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
using System.Text;
using VRTK;

/// Grab a molecule (trigger button) or a group of molecules (grip button)
/// Scale using both controllers on the same molecule

namespace UMol {

[RequireComponent(typeof(VRTK_ControllerEvents))]
[RequireComponent(typeof(VRTK_StraightPointerRendererNoRB))]
public class ControllerGrabAndScale : MonoBehaviour {

    public ControllerGrabAndScale otherController;

    VRTK_ControllerEvents controllerEvents;
    VRTK_StraightPointerRendererNoRB pointerR;
    public Transform grabbedMolecule = null;
    public Vector3 grabbedCenterOfGravity = Vector3.zero;
    public List<Transform> grabbedGroupMolecules = new List<Transform>();
    public bool isGroupGrabbed = false;
    public int grabbedGroupId = -1;
    public bool isScaling = false;
    public bool isScalingGroup = false;
    public bool isClicking = false;
    Transform savedParent = null;
    private bool didScale = false;

    Vector3 initPosScale = Vector3.zero;
    float startMagnitude = 0.0f;
    float sizeScale = 0.1f;
    Vector3 scaleStart = Vector3.one;
    Transform loadedMols;
    LayerMask notUILayer;
    CustomRaycastBurst raycaster;
    UnityMolStructureManager sm;
    PointerMoveUI moveUIScript;

    RigidbodyConstraints savedConstraints;

    public bool grabbedUI {
        get{
            if(moveUIScript != null){
                return moveUIScript.grabbedUI;
            }
            return false;
        }
    }

    void Start() {
        sm = UnityMolMain.getStructureManager();
        raycaster = UnityMolMain.getCustomRaycast();
        notUILayer = ~ LayerMask.GetMask("UI", "Ignore Raycast");

        if (controllerEvents == null) {
            controllerEvents = GetComponent<VRTK_ControllerEvents>();
        }
        if (pointerR == null) {
            pointerR = GetComponent<VRTK_StraightPointerRendererNoRB>();
        }
        moveUIScript = GetComponent<PointerMoveUI>();


        loadedMols = UnityMolMain.getRepresentationParent().transform;

        controllerEvents.TriggerPressed += triggerClicked;
        controllerEvents.TriggerReleased += triggerReleased;
        controllerEvents.TriggerUnclicked += triggerReleased;
        controllerEvents.TriggerTouchEnd += triggerReleased;


        controllerEvents.GripPressed += gridPressed;
        controllerEvents.GripReleased += triggerReleased;
        controllerEvents.GripUnclicked += triggerReleased;
        controllerEvents.GripTouchEnd += triggerReleased;

        if (otherController == null) {
            if (transform.name == "LeftController") {
                otherController = GameObject.Find("RightController").GetComponent<ControllerGrabAndScale>();
            }
            else {
                otherController = GameObject.Find("LeftController").GetComponent<ControllerGrabAndScale>();
            }
        }
    }


    private void triggerClicked(object sender, ControllerInteractionEventArgs e) {

        if (isGroupGrabbed) {
            isScaling = false;
            isScalingGroup = false;
            isClicking = false;
            return;
        }

        if (loadedMols == null) {
            loadedMols = UnityMolMain.getRepresentationParent().transform;
        }
        if (loadedMols == null) {
            isScaling = false;
            isScalingGroup = false;
            isClicking = false;
            return;
        }

        isClicking = true;


        //Send 3 rays in the plane of the controller to select a chain
        Vector3 vecDir30 =  Quaternion.AngleAxis(30, transform.right) * pointerR.actualTracer.transform.forward;
        Vector3 vecDir60 =  Quaternion.AngleAxis(60, transform.right) * pointerR.actualTracer.transform.forward;


        UnityMolAtom a = raycaster.customRaycastAtomBurst(pointerR.actualContainer.transform.position, pointerR.actualTracer.transform.forward);

        if (a == null) {
            a = raycaster.customRaycastAtomBurst(pointerR.actualContainer.transform.position, vecDir30);
            if (a == null) {
                a = raycaster.customRaycastAtomBurst(pointerR.actualContainer.transform.position, vecDir60);
            }
        }
        if (a != null) {

            Transform atomPar = sm.structureToGameObject[a.residue.chain.model.structure.uniqueName].transform;
            grabMolecule(atomPar, e.controllerReference);
            grabbedCenterOfGravity = a.residue.chain.model.structure.currentModel.centerOfGravity;
        }
        if (otherController != null) {
            if (otherController.isClicking && //Both controllers are clicking
                !grabbedUI && !otherController.grabbedUI &&
                //None grabs anything
                 ((grabbedMolecule == null && otherController.grabbedMolecule == null) ||   
                //I grabbed but the other does not
                (grabbedMolecule != null && otherController.grabbedMolecule == null) ||
                //Other grabbed but not me
                (grabbedMolecule == null && otherController.grabbedMolecule != null) ||
                //Both grabbed the same
                (grabbedMolecule == otherController.grabbedMolecule))) {

                isScaling = true;
                isScalingGroup = false;
                initPosScale = otherController.transform.position;
                startMagnitude = sizeScale * (transform.position - initPosScale).magnitude;
                scaleStart = loadedMols.localScale;
            }
        }
    }

    private void gridPressed(object sender, ControllerInteractionEventArgs e) {

        // if (isClicking) { //Already being grabbed by the trigger button
        // return;
        // }

        // if (otherController != null && otherController.isClicking
        //         && otherController.isGroupGrabbed) {
        //     otherController.isScaling = false;
        //     return;
        // }

        //Grab a group instead of just a molecule
        isClicking = true;

        if (loadedMols == null) {
            loadedMols = UnityMolMain.getRepresentationParent().transform;
        }

        //Send 3 rays in the plane of the controller to select a chain
        Vector3 vecDir30 =  Quaternion.AngleAxis(30, transform.right) * pointerR.actualTracer.transform.forward;
        Vector3 vecDir60 =  Quaternion.AngleAxis(60, transform.right) * pointerR.actualTracer.transform.forward;

        UnityMolAtom a = raycaster.customRaycastAtomBurst(pointerR.actualContainer.transform.position, pointerR.actualTracer.transform.forward);

        if (a == null) {
            a = raycaster.customRaycastAtomBurst(pointerR.actualContainer.transform.position, vecDir60);
            if (a == null) {
                a = raycaster.customRaycastAtomBurst(pointerR.actualContainer.transform.position, vecDir60);
            }
        }

        if (a != null) {

            Transform atomPar = sm.structureToGameObject[a.residue.chain.model.structure.uniqueName].transform;
            grabGroupMolecule(atomPar, e.controllerReference);
        }

        if (otherController != null) {
            if (otherController.isGroupGrabbed) {
                isScalingGroup = true;
                initPosScale = otherController.transform.position;
                startMagnitude = sizeScale * (transform.position - initPosScale).magnitude;
                scaleStart = loadedMols.localScale;
            }
        }
    }

    private void triggerReleased(object sender, ControllerInteractionEventArgs e) {

        ungrabMolecule();
    }

    private void grabMolecule(Transform toGrab, VRTK_ControllerReference controllerReference = null) {

        if (UnityMolMain.getAnnotationManager().drawMode) {
            return;
        }

        grabbedMolecule = toGrab;
        savedParent = grabbedMolecule.parent;
        grabbedMolecule.parent = transform;

        if (controllerReference != null) {
            VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f);
        }

    }


    public void ungrabMolecule() {


        if (grabbedMolecule != null && savedParent != null) {
            grabbedMolecule.parent = savedParent;

            //To release the molecule, make sure that the other controller is still grabbing the object
            if (otherController) {
                if (otherController.grabbedMolecule == null) {
                    grabbedMolecule.parent = loadedMols;
                }
            }
        }
        grabbedMolecule = null;
        savedParent = null;

        if (grabbedGroupMolecules.Count != 0) {
            foreach (Transform t in grabbedGroupMolecules) {
                t.parent = loadedMols;

            }
            grabbedGroupMolecules.Clear();
        }
        isGroupGrabbed = false;

        isClicking = false;
        isScaling = false;
        isScalingGroup = false;

        if (otherController != null) {
            otherController.isScaling = false;
            otherController.isScalingGroup = false;
        }
        raycaster.needsUpdatePos = true;

        if (didScale) {
            raycaster.needsUpdateRadii = true;
        }
        didScale = false;
    }


    private void grabGroupMolecule(Transform toGrab, VRTK_ControllerReference controllerReference = null) {


        UnityMolStructure structure = sm.selectionNameToStructure(toGrab.name);
        grabbedCenterOfGravity = Vector3.zero;

        if (structure != null) {
            HashSet<UnityMolStructure> strucOfGroup = API.APIPython.getStructuresOfGroup(structure.groupID);
            foreach (UnityMolStructure s in strucOfGroup) {
                Transform structureParent = sm.GetStructureGameObject(s.uniqueName).transform;

                grabbedGroupMolecules.Add(structureParent);
                structureParent.parent = transform;
                grabbedCenterOfGravity += s.currentModel.centerOfGravity;
            }
            grabbedCenterOfGravity /= Mathf.Max(1, strucOfGroup.Count);
            isGroupGrabbed = true;
            grabbedGroupId = structure.groupID;

            if (controllerReference != null) {
                VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, 1.0f);
            }
        }

    }

    void Update() {

        if (isScaling && !isGroupGrabbed) {
            //Only one controller is actually scaling => manage other controller grabbed molecule

            if (otherController.isGroupGrabbed) {
                return;
            }
            didScale = true;

            //Compute the current distance difference between controllers
            float magNow = sizeScale * (transform.position - otherController.transform.position).magnitude;
            float diff  = magNow - startMagnitude;
            float scaleVal = Mathf.Max(0.005f, Mathf.Min(0.5f, scaleStart.x + diff));

            //Get all molecules under the LoadedMolecules parent
            List<Transform> savedParents = new List<Transform>(sm.structureToGameObject.Count);
            foreach (GameObject gos in sm.structureToGameObject.Values) {
                savedParents.Add(gos.transform.parent);
                gos.transform.parent = loadedMols;
            }

            API.APIPython.changeGeneralScale(scaleVal);

            int id = 0;
            //Restore parents
            foreach (GameObject gos in sm.structureToGameObject.Values) {
                gos.transform.parent = savedParents[id++];
            }
        }
        if (isScalingGroup && !isScaling) {
            didScale = true;

            //Compute the current distance difference between controllers
            float magNow = sizeScale * (transform.position - otherController.transform.position).magnitude;
            float diff  = magNow - startMagnitude;
            float scaleVal = Mathf.Max(0.005f, Mathf.Min(0.5f, scaleStart.x + diff));

            List<Transform> savedPar = new List<Transform>(grabbedGroupMolecules.Count);

            foreach (GameObject gos in sm.structureToGameObject.Values) {
                savedPar.Add(gos.transform.parent);
                gos.transform.parent = loadedMols;
            }

            API.APIPython.changeGeneralScale(scaleVal);

            int i = 0;
            foreach (GameObject gos in sm.structureToGameObject.Values) {
                gos.transform.parent = savedPar[i++];
            }

        }
    }
}
}