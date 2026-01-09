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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace UMol {

[RequireComponent(typeof(PostProcessVolume))]
[RequireComponent(typeof(MouseOverSelection))]
public class MouseAutoFocus : MonoBehaviour {

    PostProcessVolume postpV;
    MouseOverSelection mos;

    [SerializeField]
    private DepthOfField DOF;

    private UnityMolAtom curA = null;

    public void Init() {
        if (GetComponent<PostProcessVolume>() != null) {
            postpV = GetComponent<PostProcessVolume>();
            mos = GetComponent<MouseOverSelection>();
            postpV.profile.TryGetSettings(out DOF);
        }
    }
    void Start() {
        Init();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() ) { //Mouse clicked
            curA = null;
            if (postpV != null && mos != null) {

                Vector3 p = Vector3.zero;
                bool isExtrAtom = false;
                UnityMolAtom a = mos.getAtomPointed(true, ref p, ref isExtrAtom);

                if (a != null) {
                    curA = a;
                    if (UnityMolMain.isDOFOn)
                        API.APIPython.setDOFFocusDistance(Vector3.Distance(GetComponent<Camera>().transform.position, p));
                }
            }
        }
        if (curA != null && UnityMolMain.isDOFOn) {
            API.APIPython.setDOFFocusDistance(Vector3.Distance(GetComponent<Camera>().transform.position, curA.curWorldPosition));
        }
    }
    public void disableDOF() {
        curA = null;
        if (postpV != null) {
            DOF.enabled.value = false;
            UnityMolMain.isDOFOn = false;
        }
        if (mos != null) {
            mos.tempDisable = false;
        }
    }
    public void enableDOF() {
        curA = null;
        if (postpV != null) {
            DOF.enabled.value = true;
            UnityMolMain.isDOFOn = true;
        }
        if (mos != null) {
            mos.tempDisable = true;
        }
    }

    public float getFocusDistance() {
        if (postpV == null) {
            Init();
        }
        if (postpV != null) {
            return DOF.focusDistance.value;
        }
        return -1.0f;
    }
    public void setFocusDistance(float v) {
        if (postpV == null) {
            Init();
        }
        if (postpV != null) {
            FloatParameter newFocusDistance = new FloatParameter { value = v };
            DOF.focusDistance.value = newFocusDistance;
        }
    }

    public void setAperture(float v) {
        if (postpV == null) {
            Init();
        }

        if (postpV != null) {
            FloatParameter newA = new FloatParameter { value = v};
            DOF.aperture.value = newA;
        }

    }
    public float getAperture() {
        if (postpV == null) {
            Init();
        }
        if (postpV != null) {
            return DOF.aperture;
        }
        return -1.0f;
    }
    public void setFocalLength(float v) {
        if (postpV == null) {
            Init();
        }

        if (postpV != null) {
            FloatParameter newF = new FloatParameter { value = v};
            DOF.focalLength.value = newF;
        }

    }
    public float getFocalLength() {
        if (postpV == null) {
            Init();
        }
        if (postpV != null) {
            return DOF.focalLength;
        }
        return -1.0f;
    }
}
}