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

using UMol.API;

namespace UMol {
public class lowerFrameRateIdle : MonoBehaviour {


    public int idleTimeInSec = 5;
    public bool isIdle = false;
    float lastIdleTime;
#if !UNITY_EDITOR
    ManipulationManager mm = null;
    DockingManager dm = null;

    void Start() {
        mm = APIPython.getManipulationManager();
        dm = UnityMolMain.getDockingManager();
    }
    public bool idleCheck() {
        if(!UnityMolMain.allowIDLE)
            return false;
        if (UnityMolMain.inVR() || UnityMolMain.raytracingMode)
            return false;
        if (mm != null && mm.isRotating)
            return false;
        if (RecordManager.busy)
            return false;
        if(dm.isRunning)
            return false;
        if(UnityMolMain.IMDRunning)
            return false;
        if(APIPython.isATrajectoryPlaying())
            return false;
        #if UNITY_STANDALONE_OSX && !UNITY_2020_1_OR_NEWER
        return false;
        #endif
        return Time.time - lastIdleTime > idleTimeInSec;
    }

    void enterIDLE() {
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 5;
        Physics.autoSimulation = false;
        AudioListener.pause = true;
    }
    void exitIDLE() {
        Physics.autoSimulation = true;
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
        AudioListener.pause = false;
    }

    void Update() {

        if (Input.anyKey || Input.mouseScrollDelta != Vector2.zero) {
            if (isIdle) {
                isIdle = false;
                exitIDLE();
            }
            lastIdleTime = Time.time;
        }
        else if (idleCheck() && !isIdle) {
            isIdle = true;
            enterIDLE();
        }
    }
#endif

}
}


