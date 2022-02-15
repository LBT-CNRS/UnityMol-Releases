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

[RequireComponent(typeof(VRTK_ControllerEvents))]
public class GrapplingHookMolecule : MonoBehaviour {

    VRTK_ControllerEvents controllerEvents;
    ControllerGrabAndScale cgs;
	public Vector2 axisDeadzone = new Vector2(0.2f, 0.2f);
	public float speedScale = 0.05f;

	private Vector2 currentAxis = Vector2.zero;
	protected bool isChanging;


	void OnEnable() {
		controllerEvents = GetComponent<VRTK_ControllerEvents>();
		cgs = GetComponent<ControllerGrabAndScale>();
		isChanging = false;

		controllerEvents.TouchpadTouchStart += TouchpadTouchStart;
		controllerEvents.TouchpadTouchEnd += TouchpadTouchEnd;
	}

	void Update(){

		if(isChanging && cgs.grabbedMolecule != null){
        	Vector2 actualAxis = controllerEvents.GetTouchpadAxis();
			currentAxis = actualAxis;

			if (OutsideDeadzone(currentAxis.y, axisDeadzone.y) || currentAxis.y == 0f){
				Vector3 localPos = cgs.grabbedMolecule.localPosition;
				
				localPos.z += currentAxis.y * speedScale;
				cgs.grabbedMolecule.localPosition = localPos;
				
				Vector3 cogPosWorld = cgs.grabbedMolecule.TransformPoint(cgs.grabbedCenterOfGravity);
				Vector3 CM = cogPosWorld - transform.position;
				if(CM.z < 0.0f){
					cgs.grabbedMolecule.Translate(-CM, Space.World);
				}
				// if(localPos.z < 0.0f){
				// 	localPos.z = 0.0f;
				// }
			}
		}
	}

	protected virtual void TouchpadTouchStart(object sender, ControllerInteractionEventArgs e)
	{
		isChanging = true;
	}


	protected virtual void TouchpadTouchEnd(object sender, ControllerInteractionEventArgs e)
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