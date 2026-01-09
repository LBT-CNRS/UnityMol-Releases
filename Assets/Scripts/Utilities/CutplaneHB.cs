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

using UMol.API;

namespace UMol {
public class CutplaneHB : MonoBehaviour
{

	private Plane cutPlane;
	private Camera mainCam;

	public bool doCutPlane = false;
	public float distplane = 1f;
	public float cutPlaneZ = 10.0f;


	void Start(){
		activateCutPlane();
	}

	// Update is called once per frame
	void Update()
	{
		if (mainCam == null) {
			mainCam = Camera.main;
		}

		if(doCutPlane){
			hideAtomsCutplane();
			cutPlane.distance = cutPlaneZ;
		}


	}

	void activateCutPlane() {
		if (mainCam == null) {
			mainCam = Camera.main;
		}
		distplane = 10.0f;
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCam);
		cutPlane = planes[4];
		cutPlane.distance /= distplane;
		cutPlane.normal *= -1;
	}

	void hideAtomsCutplane() {
		UnityMolStructure s = APIPython.last();
		HashSet<UnityMolAtom> toHide = new HashSet<UnityMolAtom>();

		RepType rt = APIPython.getRepType("hb");

		for (int i = 0; i < s.representations.Count; i++) {
			UnityMolRepresentation rep = s.representations[i];
			if (rep.repType == rt) {
				foreach (SubRepresentation sr in rep.subReps) {
					((UnityMolHBallMeshManager)sr.atomRepManager).ResetVisibility();
					((UnityMolHStickMeshManager)sr.bondRepManager).ResetVisibility();
				}
			}
		}

		foreach (UnityMolAtom a in s.currentModel.allAtoms) {
			Vector3 p = a.curWorldPosition;
			if (cutPlane.GetSide(p)) {
				foreach(UnityMolAtom ar in a.residue.atoms.Values){
					toHide.Add(ar);
				}
			}
		}
		for (int i = 0; i < s.representations.Count; i++) {
			UnityMolRepresentation rep = s.representations[i];
			if (rep.repType == rt) {
				foreach (SubRepresentation sr in rep.subReps) {
					// ((UnityMolHBallMeshManager)sr.atomRepManager).ShowAtoms(toShow, true);
					((UnityMolHBallMeshManager)sr.atomRepManager).ShowAtoms(toHide, false);

					// ((UnityMolHStickMeshManager)sr.bondRepManager).ShowAtoms(toShow, true);
					((UnityMolHStickMeshManager)sr.bondRepManager).ShowAtoms(toHide, false);
				}
			}
		}
	}

}
}