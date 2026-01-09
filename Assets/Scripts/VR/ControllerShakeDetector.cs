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

namespace UMol {

public class ControllerShakeDetector : MonoBehaviour {

	// VRTK_VelocityEstimator veloEstim;
	// public float thresholdMagnitude = 50000.0f;
	// public float catchTime = 0.25f;
	// private float pauseTime = 0.7f;
	// private float lastEventTime = 0.0f;
	// private List<float> shakeTimes = new List<float>();
	// private List<Vector3> shakeVec = new List<Vector3>();

	// public delegate void ShakeDetected();
	// public event ShakeDetected OnShakeDetected;

	// void OnEnable() {
	// 	veloEstim = GetComponent<VRTK_VelocityEstimator>();
	// }

	// void Update() {
	// 	Vector3 accel = veloEstim.GetAccelerationEstimate();

	// 	if (accel.sqrMagnitude > thresholdMagnitude) {
	// 		float curTime = Time.time;
	// 		//Not in recovery time
	// 		if (curTime - lastEventTime > pauseTime) {

	// 			if (shakeTimes.Count > 2) {
	// 				int N = shakeTimes.Count;

	// 				//Acceleration changed several times
	// 				if (curTime - shakeTimes[N - 1] < catchTime &&
	// 				        shakeTimes[N - 1] - shakeTimes[N - 2] < catchTime &&
	// 				        shakeTimes[N - 2] - shakeTimes[N - 3] < catchTime) {

	// 					//At least one of the acceleration is opposite to the previous one
	// 					if (Vector3.Dot(accel, shakeVec[N - 1]) < 0.0f ||
	// 					        Vector3.Dot(shakeVec[N - 1], shakeVec[N - 2]) < 0.0f ||
	// 					        Vector3.Dot(shakeVec[N - 2], shakeVec[N - 3]) < 0.0f) {

	// 						shakeTimes.Clear();
	// 						shakeVec.Clear();
	// 						lastEventTime = curTime;

	// 						if (OnShakeDetected != null) {
	// 							OnShakeDetected();
	// 						}
	// 					}
	// 				}
	// 			}
	// 			shakeTimes.Add(curTime);
	// 			shakeVec.Add(accel);
	// 		}
	// 	}
	// }
}
}