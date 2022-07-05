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
using System.Collections.Generic;
using System.Text;
using VRTK;

namespace UMol {

[RequireComponent(typeof(VRTK_VelocityEstimator))]
public class ControllerShakeDetector : MonoBehaviour {

	VRTK_VelocityEstimator veloEstim;
	public float thresholdMagnitude = 50000.0f;
	public float catchTime = 0.25f;
	private float pauseTime = 0.7f;
	private float lastEventTime = 0.0f;
	private List<float> shakeTimes = new List<float>();
	private List<Vector3> shakeVec = new List<Vector3>();

	public delegate void ShakeDetected();
	public event ShakeDetected OnShakeDetected;

	void OnEnable() {
		veloEstim = GetComponent<VRTK_VelocityEstimator>();
	}

	void Update() {
		Vector3 accel = veloEstim.GetAccelerationEstimate();

		if (accel.sqrMagnitude > thresholdMagnitude) {
			float curTime = Time.time;
			//Not in recovery time
			if (curTime - lastEventTime > pauseTime) {

				if (shakeTimes.Count > 2) {
					int N = shakeTimes.Count;

					//Acceleration changed several times
					if (curTime - shakeTimes[N - 1] < catchTime &&
					        shakeTimes[N - 1] - shakeTimes[N - 2] < catchTime &&
					        shakeTimes[N - 2] - shakeTimes[N - 3] < catchTime) {

						//At least one of the acceleration is opposite to the previous one
						if (Vector3.Dot(accel, shakeVec[N - 1]) < 0.0f ||
						        Vector3.Dot(shakeVec[N - 1], shakeVec[N - 2]) < 0.0f ||
						        Vector3.Dot(shakeVec[N - 2], shakeVec[N - 3]) < 0.0f) {

							shakeTimes.Clear();
							shakeVec.Clear();
							lastEventTime = curTime;

							if (OnShakeDetected != null) {
								OnShakeDetected();
							}
						}
					}
				}
				shakeTimes.Add(curTime);
				shakeVec.Add(accel);
			}
		}
	}
}
}