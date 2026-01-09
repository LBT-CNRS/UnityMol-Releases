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
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Text;

using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;

using HTC.UnityPlugin.Pointer3D;

namespace UMol {
public class PointerMoveUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public Pointer3DRaycaster raycasterLeft;
    public Pointer3DRaycaster raycasterRight;
    public bool grabbedUI = false;
    Transform UIT;
    GameObject grabbedGo;
    Transform savedParent;

    bool isPointerEntered = false;
    bool left = false;

    GameObject prevOverL = null;
    GameObject prevOverR = null;

    ControllerGrabAndScale cgsL = null;
    ControllerGrabAndScale cgsR = null;

    public bool moveParent = false;

    void Awake() {
        try {
            cgsL = GameObject.Find("LeftHand").GetComponent<ControllerGrabAndScale>();
            cgsR = GameObject.Find("RightHand").GetComponent<ControllerGrabAndScale>();
        }
        catch {}
    }

    void Update()
    {
        if (grabbedUI) {
            if (left && ViveInput.GetPressUpEx(HandRole.LeftHand, ControllerButton.Trigger)) {
                releaseMenu();
            }
            if (!left && ViveInput.GetPressUpEx(HandRole.RightHand, ControllerButton.Trigger)) {
                releaseMenu();
            }
        }
        else {
            if (ViveInput.GetPressDownEx(HandRole.RightHand, ControllerButton.Trigger) && isPointerEntered)
            {
                left = false;
                if (moveParent)
                    grabMenu(gameObject.transform.parent.gameObject);
                else
                    grabMenu(gameObject);
            }
            if (!grabbedUI && ViveInput.GetPressDownEx(HandRole.LeftHand, ControllerButton.Trigger) && isPointerEntered)
            {
                left = true;
                if (moveParent)
                    grabMenu(gameObject.transform.parent.gameObject);
                else
                    grabMenu(gameObject);
            }
        }
        if (!isPointerEntered) {
            prevOverL = null;
            prevOverR = null;
        }
        else if (raycasterLeft != null && !grabbedUI) {
            var resultL = raycasterLeft.FirstRaycastResult();
            if (resultL.isValid) {
                int lid = resultL.gameObject.GetInstanceID();
                int prevlid = (prevOverL != null ? prevOverL.GetInstanceID() : 0);

                if (prevlid != lid &&
                        (resultL.gameObject.TryGetComponent(out Button b) ||
                         resultL.gameObject.TryGetComponent(out InputField ipf) ||
                         resultL.gameObject.TryGetComponent(out Toggle t)))  {


                    ViveInput.TriggerHapticPulse(HandRole.LeftHand, 1000);
                    prevOverL = resultL.gameObject;
                }
                else if (prevlid != lid)
                    prevOverL = null;
            }
            else
                prevOverL = null;
        }
        if (raycasterRight != null && !grabbedUI && isPointerEntered) {
            var resultR = raycasterRight.FirstRaycastResult();
            if (resultR.isValid) {
                int rid = resultR.gameObject.GetInstanceID();
                int prevrid = (prevOverR != null ? prevOverR.GetInstanceID() : 0);
                if ( prevrid != rid &&
                        (resultR.gameObject.TryGetComponent(out Button b) ||
                         resultR.gameObject.TryGetComponent(out InputField ipf) ||
                         resultR.gameObject.TryGetComponent(out Toggle t))) {


                    ViveInput.TriggerHapticPulse(HandRole.RightHand, 1000);
                    prevOverR = resultR.gameObject;
                }
                else if (prevrid != rid)
                    prevOverR = null;
            }
            else {
                prevOverR = null;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        isPointerEntered = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        isPointerEntered = false;
    }

    private void grabMenu(GameObject toGrab) {
        //Don't grab UI when we already grabbed a molecule
        if (left && cgsL.grabbedMolecule != null)
            return;
        if (!left && cgsR.grabbedMolecule != null)
            return;

        grabbedGo = toGrab;
        savedParent = grabbedGo.transform.parent;
        if (left)
            grabbedGo.transform.SetParent(UnityMolMain.getLeftController().transform, true);
        else
            grabbedGo.transform.SetParent(UnityMolMain.getRightController().transform, true);

        grabbedUI = true;
        // ViveInput.TriggerHapticPulseEx(curRole.roleType, curRole.roleValue, 100);
    }

    private void releaseMenu() {
        Vector3 savedP = grabbedGo.transform.position;
        Quaternion savedR = grabbedGo.transform.rotation;

        grabbedGo.transform.SetParent(savedParent, false);
        grabbedUI = false;
        grabbedGo.transform.position = savedP;
        grabbedGo.transform.rotation = savedR;

        // ViveInput.TriggerHapticPulseEx(curRole.roleType, curRole.roleValue, 100);
    }
}
}