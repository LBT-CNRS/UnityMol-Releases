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
using System.Text;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;

/// Grab a molecule (trigger button) or a group of molecules (grip button)
/// Scale using both controllers

namespace UMol {
[RequireComponent(typeof(ViveRoleSetter))]
public class ControllerGrabAndScale : MonoBehaviour {

    public ControllerGrabAndScale otherController;

    public Transform grabbedMolecule = null;
    public Vector3 grabbedCentroid = Vector3.zero;
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
    PointerMoveUI[] moveUIScripts;

    RigidbodyConstraints savedConstraints;

    public ViveRoleProperty curRole;

    private float menustartPressedTime;
    private float minLongPress = 0.45f;//in s

    public bool grabbedUI {
        get {
            if (moveUIScripts != null && moveUIScripts.Length != 0) {
                foreach (var muis in moveUIScripts) {
                    if (muis.grabbedUI)
                        return true;
                }
            }
            return false;
        }
    }



    void Start() {
        sm = UnityMolMain.getStructureManager();
        raycaster = UnityMolMain.getCustomRaycast();
        notUILayer = ~ LayerMask.GetMask("UI", "Ignore Raycast");

        moveUIScripts = GameObject.FindObjectsOfType<PointerMoveUI>();

        loadedMols = UnityMolMain.getRepresentationParent().transform;

        if (otherController == null) {
            searchOtherController();
        }
    }
    void searchOtherController() {
// var objs = FindObjectOfType<ControllerGrabAndScale>();
        if (curRole.roleValue == (int)HandRole.LeftHand) {

            otherController = GameObject.Find("RightHand").GetComponent<ControllerGrabAndScale>();
        }

        else {
            otherController = GameObject.Find("LeftHand").GetComponent<ControllerGrabAndScale>();
        }
    }


    void OnEnable() {

        curRole = GetComponent<ViveRoleSetter>().viveRole;
        // curRole = ViveRoleProperty.New(GetComponent<ViveRoleSetter>().viveRole);

        if (curRole != null) {
            ViveInput.AddPressDown((HandRole)curRole.roleValue, ControllerButton.Trigger, triggerClicked);
            ViveInput.AddPressUp((HandRole)curRole.roleValue, ControllerButton.Trigger, triggerReleased);

            ViveInput.AddPressDown((HandRole)curRole.roleValue, ControllerButton.Grip, gridPressed);
            ViveInput.AddPressUp((HandRole)curRole.roleValue, ControllerButton.Grip, triggerReleased);


            ViveInput.AddPressDown((HandRole)curRole.roleValue, ControllerButton.Menu, menuPressed);
            ViveInput.AddPressUp((HandRole)curRole.roleValue, ControllerButton.Menu, menuReleased);
        }
    }

    void OnDisable() {

        if (curRole != null) {

            ViveInput.RemovePressDown((HandRole)curRole.roleValue, ControllerButton.Trigger, triggerClicked);
            ViveInput.RemovePressUp((HandRole)curRole.roleValue, ControllerButton.Trigger, triggerReleased);

            ViveInput.RemovePressDown((HandRole)curRole.roleValue, ControllerButton.Grip, gridPressed);
            ViveInput.RemovePressUp((HandRole)curRole.roleValue, ControllerButton.Grip, triggerReleased);

            ViveInput.RemovePressDown((HandRole)curRole.roleValue, ControllerButton.Menu, menuPressed);
            ViveInput.RemovePressUp((HandRole)curRole.roleValue, ControllerButton.Menu, menuReleased);
        }
    }

    private void triggerClicked() {

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

        if (curRole == null)
            curRole = ViveRoleProperty.New(GetComponent<ViveRoleSetter>().viveRole);

        RigidPose cpose = VivePose.GetPose(curRole);

        //Send 3 rays in the plane of the controller to select a chain
        Vector3 vecDir30 =  Quaternion.AngleAxis(30, transform.right) * cpose.forward;
        Vector3 vecDir60 =  Quaternion.AngleAxis(60, transform.right) * cpose.forward;


        Vector3 p = Vector3.zero;
        bool isExtrAtom = false;
        UnityMolAtom a = raycaster.customRaycastAtomBurst(
                             cpose.pos,
                             cpose.forward,
                             ref p, ref isExtrAtom, true);

        if (a == null) {
            a = raycaster.customRaycastAtomBurst(cpose.pos, vecDir30, ref p, ref isExtrAtom, true);
            if (a == null) {
                a = raycaster.customRaycastAtomBurst(cpose.pos, vecDir60, ref p, ref isExtrAtom, true);
            }
        }
        if (a != null) {

            Transform atomPar = sm.structureToGameObject[a.residue.chain.model.structure.name].transform;
            grabMolecule(atomPar);
            grabbedCentroid = a.residue.chain.model.structure.currentModel.centroid;
        }


        // UnityMolStructure s = raycaster.customRaycastStructure(cpose.pos, cpose.forward);
        // if (s == null) {
        //     s = raycaster.customRaycastStructure(cpose.pos, vecDir30);
        //     if (s == null) {
        //         s = raycaster.customRaycastStructure(cpose.pos, vecDir60);
        //     }
        // }

        // if (s != null && !grabbedUI) {
        //     Transform atomPar = sm.structureToGameObject[s.name].transform;
        //     grabMolecule(atomPar);
        //     grabbedCentroid = s.currentModel.centroid;
        // }

        if (otherController == null) {
            searchOtherController();
        }
        if (otherController != null) {
            if (otherController.isClicking && //Both controllers are clicking
                    !grabbedUI &&
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

    private void gridPressed() {

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

        RigidPose cpose = VivePose.GetPose(curRole);

        //Send 3 rays in the plane of the controller to select a chain
        Vector3 vecDir30 =  Quaternion.AngleAxis(30, transform.right) * cpose.forward;
        Vector3 vecDir60 =  Quaternion.AngleAxis(60, transform.right) * cpose.forward;

        Vector3 p = Vector3.zero;
        bool isExtrAtom = false;
        UnityMolAtom a = raycaster.customRaycastAtomBurst(cpose.pos,
                         cpose.forward,
                         ref p, ref isExtrAtom, true);

        if (a == null) {
            a = raycaster.customRaycastAtomBurst(cpose.pos,
                                                 vecDir60,
                                                 ref p, ref isExtrAtom, true);
            if (a == null) {
                a = raycaster.customRaycastAtomBurst(cpose.pos,
                                                     vecDir60,
                                                     ref p, ref isExtrAtom, true);
            }
        }

        if (a != null) {

            Transform atomPar = sm.structureToGameObject[a.residue.chain.model.structure.name].transform;
            grabGroupMolecule(atomPar);
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

    private void triggerReleased() {
        ungrabMolecule();
    }

    private void grabMolecule(Transform toGrab) {

        if (UnityMolMain.getAnnotationManager().drawMode) {
            return;
        }

        grabbedMolecule = toGrab;
        savedParent = grabbedMolecule.parent;
        grabbedMolecule.parent = transform;

        DockingManager dm = UnityMolMain.getDockingManager();
        if (dm.isRunning) {
            Rigidbody rb = toGrab.gameObject.GetComponent<Rigidbody>();
            if (rb != null) {
                savedConstraints = rb.constraints;
                rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
            }
        }

        ViveInput.TriggerHapticPulseEx(curRole.roleType, curRole.roleValue, 500);
    }


    public void ungrabMolecule() {

        DockingManager dm = UnityMolMain.getDockingManager();

        if (grabbedMolecule != null && savedParent != null) {
            grabbedMolecule.parent = savedParent;

            if (dm.isRunning) {
                Rigidbody rb = grabbedMolecule.gameObject.GetComponent<Rigidbody>();
                if (rb != null) {
                    if (savedConstraints != 0) {
                        rb.constraints = savedConstraints;
                    }
                    else {
                        rb.constraints = RigidbodyConstraints.None;
                    }
                }
            }
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

                if (dm.isRunning) {
                    Rigidbody rb = t.gameObject.GetComponent<Rigidbody>();
                    if (rb != null) {
                        rb.constraints = RigidbodyConstraints.None;
                    }
                }
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


    private void grabGroupMolecule(Transform toGrab) {

        DockingManager dm = UnityMolMain.getDockingManager();

        UnityMolStructure structure = sm.selectionNameToStructure(toGrab.name);
        grabbedCentroid = Vector3.zero;

        if (structure != null) {
            HashSet<UnityMolStructure> strucOfGroup = API.APIPython.getStructuresOfGroup(structure.groupID);
            foreach (UnityMolStructure s in strucOfGroup) {
                Transform structureParent = sm.GetStructureGameObject(s.name).transform;

                if (dm.isRunning) {
                    Rigidbody rb = structureParent.gameObject.GetComponent<Rigidbody>();
                    if (rb != null) {
                        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
                    }
                }

                grabbedGroupMolecules.Add(structureParent);
                structureParent.parent = transform;
                grabbedCentroid += s.currentModel.centroid;
            }
            grabbedCentroid /= Mathf.Max(1, strucOfGroup.Count);
            isGroupGrabbed = true;
            grabbedGroupId = structure.groupID;

            ViveInput.TriggerHapticPulseEx(curRole.roleType, curRole.roleValue, 500);
        }

    }

    void menuPressed() {
        menustartPressedTime = Time.realtimeSinceStartup;
    }

    void menuReleased() {
        float diffTime = Time.realtimeSinceStartup - menustartPressedTime;

        if (diffTime > minLongPress) {
            //Detected long press
            StartCoroutine(bringMenuCloser());
        }
    }
    public IEnumerator bringMenuCloser() {
        GameObject mainUIGo = GameObject.Find("CanvasMainUIVR");
        Transform head = Camera.main.transform;

        if (mainUIGo != null && head != null) {
            Vector3 targetPos = head.position + head.forward;
            Vector3 targetRot = head.rotation.eulerAngles;
            int steps = 400;
            for (int i = 1; i < steps / 4; i++) {
                float tt = i / (float)steps;
                mainUIGo.transform.position = Vector3.Lerp(mainUIGo.transform.position, targetPos, tt);

                Vector3 newRot = new Vector3(Mathf.LerpAngle(mainUIGo.transform.eulerAngles.x, targetRot.x, tt),
                                             Mathf.LerpAngle(mainUIGo.transform.eulerAngles.y, targetRot.y, tt),
                                             Mathf.LerpAngle(mainUIGo.transform.eulerAngles.z, targetRot.z, tt));
                mainUIGo.transform.eulerAngles = newRot;
                yield return 0;
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
