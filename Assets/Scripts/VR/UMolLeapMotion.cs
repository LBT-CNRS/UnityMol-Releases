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
#if LEAP_MOTION_SUPPORT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

using Leap.Unity;
using Leap;

namespace UMol {
[RequireComponent(typeof(PinchDetector))]
public class UMolLeapMotion : MonoBehaviour
{

    [SerializeField]
    public PinchDetector pinchD1;
    [SerializeField]
    public PinchDetector pinchD2;

    GameObject selected1 = null;
    GameObject selected2 = null;

    private Transform _anchor1;
    private Transform _anchor2;

    float startDoublePinchDist;
    float scaleStart = 1.0f;

    bool firstFrameSel1;
    bool firstFrameSel2;
    bool isDoubleGrabbed = false;

    Transform loadedMols;

    float sizeScale = 0.1f;
    CustomRaycastBurst raycaster;

    bool doSel1 = false;
    bool doSel2 = false;
    bool doDesel1 = false;
    bool doDesel2 = false;

    void Start() {

        if(!XRSettings.enabled) {//Desactivate Leap motion if we are not in VR
            Camera.main.gameObject.GetComponent<LeapXRServiceProvider>().enabled = false;
            foreach(var pinchs in FindObjectsOfType<PinchDetector>())
                pinchs.enabled = false;
            this.enabled = false;
        }            

        raycaster = UnityMolMain.getCustomRaycast();
        loadedMols = UnityMolMain.getRepresentationParent().transform;

        pinchD1.OnActivate.AddListener(launchSelect1);
        pinchD1.OnDeactivate.AddListener(launchDeselect1);

        pinchD2.OnActivate.AddListener(launchSelect2);
        pinchD2.OnDeactivate.AddListener(launchDeselect2);

        GameObject pinchControl1 = new GameObject("RTS Anchor L");
        GameObject pinchControl2 = new GameObject("RTS Anchor R");
        _anchor1 = pinchControl1.transform;
        _anchor2 = pinchControl2.transform;
        // _anchor.parent = transform.parent;
        // transform.parent = _anchor;
    }

    void launchSelect1() {
        doSel1 = true;
    }
    void launchDeselect1() {
        doDesel1 = true;
    }
    void launchSelect2() {
        doSel2 = true;
    }
    void launchDeselect2() {
        doDesel2 = true;
    }


    void trySelect1() {
        Vector3 pinchP = pinchD1.Position;
        float minD = float.MaxValue;

        Vector3 palmPos = pinchD1.HandModel.GetLeapHand().PalmPosition.ToVector3();
        Vector3 palmNorm = pinchD1.HandModel.GetLeapHand().PalmNormal.ToVector3();

        UnityMolStructure s = raycaster.customRaycastStructure(palmPos, palmNorm);
        if (s == null) //If pointer didn't hit get closest molecule
            s = raycaster.customClosestStructure(pinchP);
        if (s != null) {
            selected1 = UnityMolMain.getStructureManager().structureToGameObject[s.name];
        }

        if (selected1 == null)
            return;

        firstFrameSel1 = true;
    }

    void deselect1() {
        restoreP1();

        selected1 = null;
    }

    void trySelect2() {
        Vector3 pinchP = pinchD2.Position;
        Vector3 palmPos = pinchD2.HandModel.GetLeapHand().PalmPosition.ToVector3();
        Vector3 palmNorm = pinchD2.HandModel.GetLeapHand().PalmNormal.ToVector3();

        UnityMolStructure s = raycaster.customRaycastStructure(palmPos, palmNorm);
        if (s == null) //If pointer didn't hit get closest molecule
            s = raycaster.customClosestStructure(pinchP);
        if (s != null) {
            selected2 = UnityMolMain.getStructureManager().structureToGameObject[s.name];
        }

        if (selected2 == null)
            return;

        firstFrameSel2 = true;
    }

    void deselect2() {
        restoreP2();

        selected2 = null;
    }

    void restoreP1() {
        if (selected1 == null)
            return;
        if (selected1.transform.parent == _anchor1) {
            if (_anchor1.parent != null) {
                selected1.transform.parent = _anchor1.parent;
                _anchor1.parent = null;
            }
            else
                selected1.transform.parent = null;
        }
    }

    void restoreP2() {
        if (selected2 == null)
            return;
        if (selected2.transform.parent == _anchor2) {
            if (_anchor2.parent != null) {
                selected2.transform.parent = _anchor2.parent;
                _anchor2.parent = null;
            }
            else
                selected2.transform.parent = null;
        }
    }
    void Update() {
        if (loadedMols == null)
            loadedMols = UnityMolMain.getRepresentationParent().transform;


        if (doSel1) {
            trySelect1();
            doSel1 = false;
        }

        if (doSel2) {
            trySelect2();
            doSel2 = false;
        }

        if (doDesel1) {
            deselect1();
            doDesel1 = false;
        }

        if (doDesel2) {
            deselect2();
            doDesel2 = false;
        }

        if (selected1 != null && selected2 != null && selected1 == selected2) { //Grabbed the same object => scale
            doubleAnchor();
            firstFrameSel1 = false;
            firstFrameSel2 = false;
            return;
        }
        if (isDoubleGrabbed) {//Was scaling => restore grabbing hand
            deselect1();
            if (pinchD1.IsPinching) {
                trySelect1();
            }
            else if (pinchD2.IsPinching) {
                trySelect2();
            }
            isDoubleGrabbed = false;
        }
        if (selected1 != null) {
            singleAnchor(pinchD1, selected1, firstFrameSel1, true);
            firstFrameSel1 = false;
        }
        if (selected2 != null) {
            singleAnchor(pinchD2, selected2, firstFrameSel2, false);
            firstFrameSel2 = false;
        }

        // if (selected1 == null && selected2 == null) {
        //  _anchor1.localScale = Vector3.one;//Reset scale
        //  _anchor2.localScale = Vector3.one;//Reset scale
        // }


    }

    void doubleAnchor() {

        Quaternion pp = Quaternion.Lerp(pinchD1.Rotation, pinchD2.Rotation, 0.5f);
        Vector3 u = pp * Vector3.up;

        if (firstFrameSel1 || firstFrameSel2) {
            // startDoublePinchDist = Vector3.Distance(pinchD1.Position, pinchD2.Position);
            startDoublePinchDist = sizeScale * (pinchD1.Position - pinchD2.Position).magnitude;
            scaleStart = loadedMols.localScale.x;
            if (firstFrameSel1)
                restoreP2();
            else
                restoreP1();

            // _anchor1.localScale = Vector3.one;
            // _anchor2.localScale = Vector3.one;

            _anchor1.position = (pinchD1.Position + pinchD2.Position) / 2.0f;
            _anchor1.LookAt(pinchD1.Position, u);

            _anchor1.parent = selected1.transform.parent;
            selected1.transform.SetParent(_anchor1);

        }

        float newScale = sizeScale * (pinchD1.Position - pinchD2.Position).magnitude;//Vector3.Distance(pinchD1.Position, pinchD2.Position);
        float diffDist = newScale - startDoublePinchDist;
        float scaleVal = Mathf.Max(0.005f, Mathf.Min(0.5f, scaleStart + diffDist));

        _anchor1.position = (pinchD1.Position + pinchD2.Position) / 2.0f;
        _anchor1.LookAt(pinchD1.Position, u);

        selected1.transform.SetParent(loadedMols);
        _anchor1.parent = null;

        API.APIPython.changeGeneralScale_cog(scaleVal);

        _anchor1.parent = loadedMols;
        selected1.transform.SetParent(_anchor1);

        isDoubleGrabbed = true;

        raycaster.needsUpdatePos = true;
        raycaster.needsUpdateRadii = true;

    }

    void singleAnchor(PinchDetector pinch, GameObject sel, bool firstF, bool isLeft) {

        if (firstF) {
            if (isLeft) {
                _anchor1.position = pinch.Position;
                _anchor1.rotation = pinch.Rotation;
                // _anchor1.localScale = Vector3.one;

                _anchor1.parent = sel.transform.parent;
                sel.transform.SetParent(_anchor1);
            }
            else {
                _anchor2.position = pinch.Position;
                _anchor2.rotation = pinch.Rotation;
                // _anchor2.localScale = Vector3.one;

                _anchor2.parent = sel.transform.parent;
                sel.transform.SetParent(_anchor2);
            }
        }

        if (isLeft) {
            _anchor1.position = pinch.Position;
            _anchor1.rotation = pinch.Rotation;
        }
        else {
            _anchor2.position = pinch.Position;
            _anchor2.rotation = pinch.Rotation;
        }

        raycaster.needsUpdatePos = true;
    }
}
}
#endif
