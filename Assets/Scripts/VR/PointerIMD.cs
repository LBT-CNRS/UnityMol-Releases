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
using System.Text;
using VRTK;
using UMol.API;

namespace UMol {

[RequireComponent(typeof(VRTK_Pointer))]
[RequireComponent(typeof(VRTK_StraightPointerRendererNoRB))]
public class PointerIMD : MonoBehaviour {

    VRTK_Pointer pointer;
    VRTK_StraightPointerRendererNoRB pointerR;
    UnityMolSelectionManager selM;
    PointerHoverAtom hoveringScript;
    CustomRaycastBurst raycaster;
    UnityMolSelection curSel;
    Vector3 initPos;
    Vector3 initPosA;
    Vector3 initVec;
    VRTK_ControllerReference controllerReference;

    Transform atomT;
    GameObject arrow;

    void OnEnable() {
        raycaster = UnityMolMain.getCustomRaycast();

        if (pointer == null) {
            pointer = GetComponent<VRTK_Pointer>();
        }
        if (pointerR == null) {
            pointerR = GetComponent<VRTK_StraightPointerRendererNoRB>();
        }
        selM = UnityMolMain.getSelectionManager();
        curSel = null;

        hoveringScript = GetComponent<PointerHoverAtom>();

        if (arrow == null) {
            arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arrow.GetComponent<MeshRenderer>().enabled = false;
            Destroy(arrow.GetComponent<BoxCollider>());
        }


        pointer.SelectionButtonPressed += buttonPressed;
        pointer.ActivationButtonReleased += buttonReleased;
    }



    void buttonPressed(object sender, ControllerInteractionEventArgs e) {

        UnityMolAtom a = raycaster.customRaycastAtomBurst(pointerR.actualContainer.transform.position, pointerR.actualTracer.transform.forward);

        if (a == null) {
            curSel = null;
            return;
        }

        UnityMolAtom clickedAtom = a;

        if (clickedAtom != null) {
            UnityMolSelection imdSelection = null;
            // if (selM.selectionMode == UnityMolSelectionManager.SelectionMode.Atom) {
            imdSelection = clickedAtom.ToSelection();
            // }
            // else if (selM.selectionMode == UnityMolSelectionManager.SelectionMode.Residue) {
            //     UnityMolResidue clickedRes = clickedAtom.residue;
            //     imdSelection = clickedRes.ToSelection();
            // }
            // else if (selM.selectionMode == UnityMolSelectionManager.SelectionMode.Chain) {
            //     UnityMolChain clickedChain = clickedAtom.residue.chain;
            //     imdSelection = clickedChain.ToSelection();
            // }
            // else if(selM.selectionMode == UnityMolSelectionManager.SelectionMode.Molecule){
            // }

            if (imdSelection.structures == null || imdSelection.structures.Count != 1) {
                return;
            }
            UnityMolStructure s = imdSelection.structures[0];
            UnityMolStructureManager sm = UnityMolMain.getStructureManager();
            Transform molParent = sm.structureToGameObject[s.uniqueName].transform;
            atomT = s.atomToGo[clickedAtom].transform;

            curSel = imdSelection;
            initPos = molParent.InverseTransformPoint(transform.position);
            initPosA = molParent.InverseTransformPoint(atomT.position);
            initVec = initPos - initPosA;

            controllerReference = e.controllerReference;
        }

    }

    void Update() {
        if (curSel != null && curSel.Count != 0) {

            if (hoveringScript != null) {
                hoveringScript.pauseHovering = true;
            }

            UnityMolStructure s = curSel.structures[0];
            UnityMolStructureManager sm = UnityMolMain.getStructureManager();
            Transform molParent = sm.structureToGameObject[s.uniqueName].transform;

            Vector3 curPos = molParent.InverseTransformPoint(transform.position);
            Vector3 curPosA = molParent.InverseTransformPoint(atomT.position);

            Vector3 between = (curPos - curPosA);
            Vector3 forceL = between - initVec;

            //Get artemis manager
            ArtemisManager am = s.artemisM;
            if (am == null) {
                return;
            }

            foreach(UnityMolAtom a in curSel.atoms){
                am.addForce(a.idInAllAtoms, new float[] {forceL.x, forceL.y, forceL.z});
            }

            //Haptic feedback

            //50 is supposed to be an important force magnitude
            float vibrationStrength = Mathf.Clamp01(forceL.magnitude / 50f);
            float actualVibration = Mathf.Lerp(0.0f, 1.0f, vibrationStrength);

            VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, actualVibration);

            //Visual feedback
            arrow.GetComponent<MeshRenderer>().enabled = true;


            float dist = (transform.position - atomT.position).magnitude;
            float clampedForceMag = Mathf.Clamp(forceL.magnitude, 0.1f, 50.0f);
            float lerped = Mathf.Lerp(0.1f, 1.0f, clampedForceMag / 50.0f);

            arrow.transform.localScale = new Vector3(lerped / 10.0f, lerped / 10.0f, dist);
            // arrow.transform.localScale = new Vector3(dist/12.0f, dist/12.0f, dist);
            arrow.transform.position = atomT.position + (transform.position - atomT.position) / 2.0f;
            arrow.transform.LookAt(transform.position);

        }
        else {
            if (hoveringScript != null) {
                hoveringScript.pauseHovering = false;
            }
        }
    }

    void buttonReleased(object sender, ControllerInteractionEventArgs e) {
        curSel = null;
        arrow.transform.parent = null;
        arrow.GetComponent<MeshRenderer>().enabled = false;
        if (curSel != null && curSel.Count != 0) {

            UnityMolStructure s = curSel.structures[0];

            //Get artemis manager
            ArtemisManager am = s.artemisM;
            if (am == null) {
                return;
            }
            am.clearForces();
        }
        // gameObject.GetComponent<VRTK_UIPointer>().enabled = true;
    }
    void buttonOut(object sender, DestinationMarkerEventArgs e) {
        curSel = null;
        arrow.transform.parent = null;
        arrow.GetComponent<MeshRenderer>().enabled = false;
        // gameObject.GetComponent<VRTK_UIPointer>().enabled = true;
    }

    void OnDisable() {
        curSel = null;
        pointer.SelectionButtonPressed -= buttonPressed;
        pointer.ActivationButtonReleased -= buttonReleased;
        arrow.transform.parent = null;
        arrow.GetComponent<MeshRenderer>().enabled = false;
    }
}
}