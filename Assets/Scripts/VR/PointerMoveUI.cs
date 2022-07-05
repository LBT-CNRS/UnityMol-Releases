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

namespace UMol {
[RequireComponent(typeof(VRTK_ControllerEvents))]
[RequireComponent(typeof(ControllerGrabAndScale))]
[RequireComponent(typeof(VRTK_UIPointer))]
public class PointerMoveUI : MonoBehaviour {

    VRTK_UIPointer UIpointer;
    VRTK_Pointer pointer;
    VRTK_ControllerEvents controllerEvents;
    ControllerGrabAndScale grabber;

    public bool grabbedUI = false;
    Transform UIT;
    GameObject grabbedGo;
    Transform savedParent;

    GameObject menuToGrab = null;

    float startPressedTime = 0.0f;
    float minLongPress = 0.45f;//in s

    void Start() {

        if (UIpointer == null) {
            UIpointer = GetComponent<VRTK_UIPointer>();
            pointer = GetComponent<VRTK_Pointer>();
        }
        if (controllerEvents == null) {
            controllerEvents = GetComponent<VRTK_ControllerEvents>();
        }

        if (grabber == null) {
            grabber = GetComponent<ControllerGrabAndScale>();
        }

        UIpointer.UIPointerElementClick += UIPointerElementClick;
        pointer.DestinationMarkerEnter += pointerCollided;
        pointer.DestinationMarkerExit += pointerStopCollided;

        controllerEvents.TriggerPressed += triggerClicked;
        controllerEvents.TriggerReleased += triggerReleased;
        controllerEvents.TriggerUnclicked += triggerReleased;
        controllerEvents.TriggerTouchEnd += triggerReleased;

        controllerEvents.ButtonTwoPressed += button2Pressed;
        controllerEvents.ButtonTwoReleased += button2Released;
    }

    private void pointerCollided(object sender, DestinationMarkerEventArgs e) {
        if (e.raycastHit.collider.transform.name.StartsWith("Canvas")) {
            Transform t = e.raycastHit.collider.transform;
            Transform displ = t.Find("Displacement");

            if (displ != null) {
                menuToGrab = displ.gameObject;
            }
        }
    }

    private void pointerStopCollided(object send, DestinationMarkerEventArgs e) {
        menuToGrab = null;
    }

    private void triggerClicked(object sender, ControllerInteractionEventArgs e) {
        if (menuToGrab != null && grabber.grabbedMolecule == null) {//Not grabbed a molecule
            if(menuToGrab.transform.parent.parent == null) {//Not already grabbed by another controller
                grabMenu(menuToGrab);
            }
        }
    }
    private void triggerReleased(object sender, ControllerInteractionEventArgs e) {
        if (grabbedUI) {
            releaseMenu();
        }
    }


    private void UIPointerElementClick(object sender, UIPointerEventArgs e)
    {
        if (grabbedUI) { //Release the menu
            releaseMenu();
            return;
        }
        else {
            if (e.currentTarget.name == "Displacement") {
                grabMenu(e.currentTarget);
            }
        }
    }

    private void grabMenu(GameObject toGrab) {
        grabbedGo = toGrab;
        savedParent = grabbedGo.transform.parent.parent;
        grabbedGo.transform.parent.SetParent(transform, true);
        grabbedUI = true;
        VRTK_ControllerHaptics.TriggerHapticPulse(VRTK_ControllerReference.GetControllerReference(controllerEvents.gameObject), 1.0f);
    }

    private void releaseMenu() {
        Vector3 savedP = grabbedGo.transform.parent.position;
        Quaternion savedR = grabbedGo.transform.parent.rotation;

        grabbedGo.transform.parent.SetParent(savedParent, true);
        grabbedUI = false;
        grabbedGo.transform.parent.position = savedP;
        grabbedGo.transform.parent.rotation = savedR;
        VRTK_ControllerHaptics.TriggerHapticPulse(VRTK_ControllerReference.GetControllerReference(controllerEvents.gameObject), 0.5f);
    }

    void button2Pressed(object sender, ControllerInteractionEventArgs e) {
        startPressedTime = Time.realtimeSinceStartup;
    }
    void button2Released(object sender, ControllerInteractionEventArgs e) {
        float diffTime = Time.realtimeSinceStartup - startPressedTime;

        if (diffTime > minLongPress) {
            //Detected long press
            StartCoroutine(bringMenuCloser());
        }
    }

    public IEnumerator bringMenuCloser() {
        GameObject mainUIGo = GameObject.Find("CanvasMainUIVR");
        Transform head = VRTK.VRTK_DeviceFinder.HeadsetCamera();

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


}
}