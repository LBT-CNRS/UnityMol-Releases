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

namespace UMol {
[RequireComponent(typeof(ViveRoleSetter))]
public class GrapplingHookMolecule : MonoBehaviour {


    ControllerGrabAndScale cgs;
    public Vector2 axisDeadzone = new Vector2(0.2f, 0.2f);
    public float speedScale = 0.05f;

    private Vector2 currentAxis = Vector2.zero;
    protected bool isChanging;

    ViveRoleProperty curRole;


    void OnEnable() {

        curRole = GetComponent<ViveRoleSetter>().viveRole;

        cgs = GetComponent<ControllerGrabAndScale>();
        isChanging = false;

        if (curRole != null) {
            ViveInput.AddPressDown((HandRole)curRole.roleValue, ControllerButton.PadTouch, TouchpadTouchStart);
            ViveInput.AddPressUp((HandRole)curRole.roleValue, ControllerButton.PadTouch, TouchpadTouchEnd);
        }
    }

    void Update() {

        if (isChanging && cgs.grabbedMolecule != null) {
            Vector2 actualAxis = ViveInput.GetPadAxis(curRole);
            currentAxis = actualAxis;

            if (OutsideDeadzone(currentAxis.y, axisDeadzone.y) || currentAxis.y == 0f) {
                Vector3 localPos = cgs.grabbedMolecule.localPosition;

                localPos.z += currentAxis.y * speedScale;
                cgs.grabbedMolecule.localPosition = localPos;

                Vector3 cogPosWorld = cgs.grabbedMolecule.TransformPoint(cgs.grabbedCentroid);
                Vector3 CM = cogPosWorld - transform.position;
                if (CM.z < 0.0f) {
                    cgs.grabbedMolecule.Translate(-CM, Space.World);
                }
                // if(localPos.z < 0.0f){
                //  localPos.z = 0.0f;
                // }
            }
        }
    }

    protected virtual void TouchpadTouchStart()
    {
        isChanging = true;
    }


    protected virtual void TouchpadTouchEnd()
    {
        currentAxis = Vector2.zero;
        isChanging = false;
    }

    protected virtual bool OutsideDeadzone(float axisValue, float deadzoneThreshold)
    {
        return (axisValue > deadzoneThreshold || axisValue < -deadzoneThreshold);
    }
}
}