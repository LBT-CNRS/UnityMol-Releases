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

using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;

namespace UMol {
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(ViveRoleSetter))]

public class GeometricalSelection : MonoBehaviour {

	public enum GeometrySelectionMode
	{
		Sphere,
		Cube
	}

	public GeometrySelectionMode mode = GeometrySelectionMode.Sphere;

	public Transform secondController;

	// private float startDistance = -1.0f;

	// void Update(){

	// 	if(secondController == null){
	// 		return;
	// 	}
	// 	if((ViveInput.GetPress(HandRole.RightHand, activationButton)
	// 		&& ViveInput.GetPressDown(HandRole.LeftHand, activationButton)) ||
	// 		(ViveInput.GetPressDown(HandRole.RightHand, activationButton)
	// 		&& ViveInput.GetPress(HandRole.LeftHand, activationButton))
	// 		){

	// 		startDistance = Vector3.Distance(transform.position, secondController.position);
	// 		return;
	// 	}
	// 	if(ViveInput.GetPressUp(HandRole.RightHand, activationButton) ||
	// 		ViveInput.GetPressUp(HandRole.LeftHand, activationButton)) {

	// 			startDistance = -1.0f;
	// 			API.APIPython.selectInSphere(transform.position, transform.lossyScale.x);
	// 		}

	// 	if(ViveInput.GetPress(HandRole.RightHand, activationButton) &&
	// 		ViveInput.GetPress(HandRole.LeftHand, activationButton)){

	// 		if(startDistance == -1.0f){
	// 			return;
	// 		}

	// 		float dist = Vector3.Distance(transform.position, secondController.position);
	// 		float diff = (dist - startDistance)/10.0f;
	// 		transform.localScale += Vector3.one * diff;
	// 		transform.localScale = Mathf.Max(0.01f, transform.localScale.x) * Vector3.one;
	// 		transform.localScale = Mathf.Min(1.1f, transform.localScale.x) * Vector3.one;
	// 	}
	// }

}
}