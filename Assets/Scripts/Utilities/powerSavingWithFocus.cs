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
using UnityEngine.Rendering;

namespace UMol {
public class powerSavingWithFocus : MonoBehaviour
{
    Camera mainCam;
    Camera uiCam;
#if !UNITY_EDITOR
    bool isPaused = false;

    void OnGUI()
    {
        if (isPaused)
            GUI.Label(new Rect(100, 100, 100, 100), "<size=20>UnityMol is paused</size>");
    }

    void Start() {
        if (mainCam == null) {
            mainCam = Camera.main;
        }
        if (mainCam != null) {
            uiCam = mainCam.gameObject.GetComponentInChildren<Camera>();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        isPaused = !hasFocus;

        if (mainCam == null) {
            mainCam = Camera.main;
            if (mainCam != null) {
                uiCam = mainCam.gameObject.GetComponentInChildren<Camera>();
            }
        }

        if (!UnityMolMain.inVR()) {
            if (isPaused) {
                Debug.Log("Losing focus");

                if (mainCam != null) {
                    mainCam.enabled = false;
                }
                if (uiCam != null) {
                    uiCam.enabled = false;
                }

                QualitySettings.vSyncCount = 0;  // VSync must be disabled
                Application.targetFrameRate = 5;
                // OnDemandRendering.renderFrameInterval = 10;
                Physics.autoSimulation = false;


            }
            else {
                Debug.Log("Gaining focus");

                if (mainCam != null) {
                    mainCam.enabled = true;
                }
                if (uiCam != null) {
                    uiCam.enabled = true;
                }

                Physics.autoSimulation = true;
                // OnDemandRendering.renderFrameInterval = 1;
                QualitySettings.vSyncCount = 1;
                Application.targetFrameRate = Screen.currentResolution.refreshRate;
            }
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        isPaused = pauseStatus;
    }
#endif
}
}