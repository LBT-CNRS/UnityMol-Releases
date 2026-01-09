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

using UMol.API;

using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;

namespace UMol {
[RequireComponent(typeof(ViveRoleSetter))]
public class PointerIMD : MonoBehaviour {

    public bool imdUseSelectionMode = false;

    UnityMolSelectionManager selM;
    PointerHoverAtom hoveringScript;
    CustomRaycastBurst raycaster;
    UnityMolSelection curSel;

    Vector3 initPos;
    Vector3 initPosA;
    Vector3 initVec;

    List<GameObject> imdArrowList = new List<GameObject>();

    Transform atomT;
    GameObject arrow;

    ViveRoleProperty curRole;


    void OnEnable() {
        curRole = GetComponent<ViveRoleSetter>().viveRole;

        raycaster = UnityMolMain.getCustomRaycast();

        selM = UnityMolMain.getSelectionManager();
        curSel = null;

        hoveringScript = GetComponent<PointerHoverAtom>();

        if (arrow == null) {
            arrow = Instantiate(Resources.Load("Prefabs/SpringPrefab") as GameObject);
            arrow.GetComponentInChildren<MeshRenderer>().enabled = false;
            DontDestroyOnLoad(arrow);
        }

        if (curRole != null) {
            ViveInput.AddPressDown((HandRole)curRole.roleValue, ControllerButton.Pad, buttonPressed);
            ViveInput.AddPressUp((HandRole)curRole.roleValue, ControllerButton.Pad, buttonReleased);
        }
    }

    void OnDisable() {
        if (curRole != null) {
            ViveInput.RemovePressDown((HandRole)curRole.roleValue, ControllerButton.Pad, buttonPressed);
            ViveInput.RemovePressUp((HandRole)curRole.roleValue, ControllerButton.PadTouch, buttonReleased);
        }

        curSel = null;

        if (arrow != null) {
            arrow.transform.parent = null;
            arrow.GetComponentInChildren<MeshRenderer>().enabled = false;
        }
    }


    void buttonPressed() {

        RigidPose cpose = VivePose.GetPose(curRole);


        Vector3 p = Vector3.zero;
        bool isExtrAtom = false;
        UnityMolAtom a = raycaster.customRaycastAtomBurst(
                             cpose.pos,
                             cpose.forward,
                             ref p, ref isExtrAtom, false);

        if (a == null) {
            curSel = null;
            return;
        }

        UnityMolAtom clickedAtom = a;

        if (clickedAtom != null) {
            UnityMolSelection imdSelection = null;

            if (imdUseSelectionMode) {
                if (selM.selectionMode == UnityMolSelectionManager.SelectionMode.Atom) {
                    imdSelection = clickedAtom.ToSelection();
                }
                else if (selM.selectionMode == UnityMolSelectionManager.SelectionMode.Residue) {
                    UnityMolResidue hoveredRes = clickedAtom.residue;
                    imdSelection = hoveredRes.ToSelection(false);
                }
                else if (selM.selectionMode == UnityMolSelectionManager.SelectionMode.Chain) {
                    UnityMolChain hoveredChain = clickedAtom.residue.chain;
                    imdSelection = hoveredChain.ToSelection(false);
                }
            }
            else {
                imdSelection = clickedAtom.ToSelection();
            }


            if (imdSelection.structures == null || imdSelection.structures.Count != 1) {
                return;
            }
            UnityMolStructure s = imdSelection.structures[0];
            UnityMolStructureManager sm = UnityMolMain.getStructureManager();
            Transform molParent = sm.structureToGameObject[s.name].transform;
            atomT = UnityMolMain.getAnnotationManager().getGO(clickedAtom).transform;

            curSel = imdSelection;

            initPos = molParent.InverseTransformPoint(transform.position);
            initPosA = molParent.InverseTransformPoint(atomT.position);
            initVec = initPos - initPosA;
        }

    }

    void Update() {
        if (curSel != null && curSel.Count != 0) {

            if (hoveringScript != null) {
                hoveringScript.pauseHovering = true;
            }

            UnityMolStructure s = curSel.structures[0];
            UnityMolStructureManager sm = UnityMolMain.getStructureManager();
            Transform molParent = sm.structureToGameObject[s.name].transform;

            Vector3 curPos = molParent.InverseTransformPoint(transform.position);
            Vector3 curPosA = molParent.InverseTransformPoint(atomT.position);

            Vector3 between = (curPos - curPosA);
            Vector3 forceL = between - initVec;

            MDDriverManager mdm = s.mddriverM;
            if (mdm == null) {
                return;
            }

            foreach (UnityMolAtom a in curSel.atoms) {
                // am.addForce(a.idInAllAtoms, new float[] {forceL.x, forceL.y, forceL.z});
                mdm.addForce(a.idInAllAtoms, new float[] {forceL.x, forceL.y, forceL.z});
            }

            //Haptic feedback

            //50 is supposed to be an important force magnitude
            float vibrationStrength = Mathf.Clamp01(forceL.magnitude / 50f);
            float actualVibration = Mathf.Lerp(0.0f, 1.0f, vibrationStrength);

            ViveInput.TriggerHapticPulseEx(curRole.roleType, curRole.roleValue, (ushort)(actualVibration * 3999));//3999 is the max value

            //Visual feedback
            arrow.GetComponentInChildren<MeshRenderer>().enabled = true;

            Vector3 initAtomToCtrl = transform.position - atomT.position;

            showAddedForces(initAtomToCtrl);


            // float dist = (transform.position - atomT.position).magnitude;
            // float clampedForceMag = Mathf.Clamp(forceL.magnitude, 0.1f, 50.0f);
            // float lerped = Mathf.Lerp(0.1f, 1.0f, clampedForceMag / 50.0f);

            // arrow.transform.localScale = new Vector3(lerped / 10.0f, lerped / 10.0f, dist);
            // // arrow.transform.localScale = new Vector3(dist/12.0f, dist/12.0f, dist);
            // arrow.transform.position = atomT.position + (transform.position - atomT.position) / 2.0f;
            // arrow.transform.LookAt(transform.position);

        }
        else {
            if (hoveringScript != null) {
                hoveringScript.pauseHovering = false;
            }
        }
    }


    void showAddedForces(Vector3 iniAtomToCtrl) {
        //Pool objects, can be improved with a real pool
        if (imdArrowList.Count < curSel.Count) {
            int start = imdArrowList.Count;
            int end = curSel.Count;
            for (int i = start; i < end; i++) {
                GameObject newArr = Instantiate(arrow);
                newArr.GetComponentInChildren<MeshRenderer>().enabled = true;
                imdArrowList.Add(newArr);
            }
        }
        foreach (GameObject ar in imdArrowList) {
            ar.SetActive(false);
        }

        float magn = iniAtomToCtrl.magnitude;
        UnityMolStructure s = curSel.structures[0];
        int id = 0;
        foreach (UnityMolAtom a in curSel.atoms) {
            Transform aT = UnityMolMain.getAnnotationManager().getGO(a).transform;
            imdArrowList[id].SetActive(true);

            imdArrowList[id].transform.localScale = new Vector3(magn / 2.0f, magn / 2.0f, magn);
            imdArrowList[id].transform.position = aT.position + iniAtomToCtrl / 2.0f;
            imdArrowList[id].transform.LookAt(aT.position + iniAtomToCtrl);

            id++;
        }

    }

    void buttonReleased() {
        arrow.transform.parent = null;
        arrow.GetComponentInChildren<MeshRenderer>().enabled = false;
        foreach (GameObject ar in imdArrowList) {
            ar.SetActive(false);
        }

        curSel = null;

    }
    void buttonOut() {
        curSel = null;
        arrow.transform.parent = null;
        arrow.GetComponentInChildren<MeshRenderer>().enabled = false;
    }
}
}