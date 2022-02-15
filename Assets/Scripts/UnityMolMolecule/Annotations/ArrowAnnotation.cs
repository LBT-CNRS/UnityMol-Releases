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
using System.Collections.Generic;
using System.Linq;

namespace UMol {
public class ArrowAnnotation : UnityMolAnnotation {

    public Color colorLine = new Color(0.0f, 0.0f, 0.5f, 1.0f);

    Vector3 p1;
    Vector3 p2;

    public override void Create() {

        if (atoms == null || atoms.Count != 2) {
            Debug.LogError("Could not create ArrowAnnotation, 'atoms' list is not correctly set");
            return;
        }

        p1 = atoms[0].position;
        p2 = atoms[1].position;

        GameObject arrowObject = GameObject.Instantiate((GameObject) Resources.Load("Prefabs/ArrowCircleSimple"));
        arrowObject.transform.parent = annoParent;

        Vector3 posA2 = annoParent.parent.TransformPoint(p1);
        Vector3 posA3 = annoParent.parent.TransformPoint(p2);

        Vector3 mid = (posA2 + posA3) * 0.5f;

        arrowObject.transform.position = mid;
        arrowObject.transform.rotation = Quaternion.FromToRotation(arrowObject.transform.up, (posA3 - posA2).normalized);

        arrowObject.transform.localScale = Vector3.one * 0.01f;

        AnimateTorsionAngle anim = arrowObject.AddComponent<AnimateTorsionAngle>();
        anim.t1 = atoms[0].correspondingGo.transform;
        anim.t2 = atoms[1].correspondingGo.transform;

        arrowObject.GetComponentsInChildren<MeshRenderer>()[0].material.color = colorLine;

        // addToDic(a4, arrowObject);
        go = arrowObject;
    }
    public override void Update() {
        if (p1 != atoms[0].position || p2 != atoms[1].position) {

            p1 = atoms[0].position;
            p2 = atoms[1].position;

            Vector3 posA2 = annoParent.parent.TransformPoint(p1);
            Vector3 posA3 = annoParent.parent.TransformPoint(p2);

            Vector3 mid = (posA2 + posA3) * 0.5f;

            go.transform.position = mid;
            go.transform.rotation = Quaternion.FromToRotation(go.transform.up, (posA3 - posA2).normalized);

            go.GetComponentsInChildren<MeshRenderer>()[0].material.color = colorLine;

        }
    }
    public override void UnityUpdate() {
    }
    public override void Delete() {
        if (go != null) {
            GameObject.Destroy(go);
        }
    }

    public override void Show(bool show = true) {
        if (go != null) {
            go.SetActive(show);
        }
    }
}
}