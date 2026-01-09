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
        anim.t1 = UnityMolMain.getAnnotationManager().getGO(atoms[0]).transform;
        anim.t2 = UnityMolMain.getAnnotationManager().getGO(atoms[1]).transform;

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
            isShown = show;
            go.SetActive(show);
        }
    }

    public override SerializedAnnotation Serialize() {
        SerializedAnnotation san = new SerializedAnnotation();
        san.color = colorLine;
        fillSerializedAtoms(san);
        return san;
    }
    public override int toAnnoType(){
        return 3;
    }
}
}