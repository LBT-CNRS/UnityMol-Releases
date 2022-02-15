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