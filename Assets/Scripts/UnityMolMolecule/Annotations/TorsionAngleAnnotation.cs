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
public class TorsionAngleAnnotation : UnityMolAnnotation {

    public Color colorText = new Color(0.0f, 0.0f, 0.5f, 1.0f);
    Vector3 p1;
    Vector3 p2;
    Vector3 p3;
    Vector3 p4;

    public override void Create() {

        if (atoms == null || atoms.Count != 4) {
            Debug.LogError("Could not create TorsionAngleAnnotation, 'atoms' list is not correctly set");
            return;
        }

        p1 = atoms[0].position;
        p2 = atoms[1].position;
        p3 = atoms[2].position;
        p4 = atoms[3].position;

        float dihe = UnityMolAnnotationManager.dihedral(atoms[0].position, atoms[1].position, atoms[2].position, atoms[3].position);

        Vector3 posA2 = annoParent.parent.TransformPoint(atoms[1].position);
        Vector3 posA3 = annoParent.parent.TransformPoint(atoms[2].position);

        Vector3 mid = (posA2 + posA3) * 0.5f;

        string text = dihe.ToString("F1") + "°";

        GameObject textObj = GameObject.Instantiate((GameObject) Resources.Load("Prefabs/SphereOverAtom"));
        textObj.name = "AngleDih";
        textObj.GetComponent<MeshRenderer>().enabled = false;
        TextMesh textm = textObj.GetComponentsInChildren<TextMesh>()[0];
        textm.gameObject.GetComponent<MeshRenderer>().sortingOrder = 50;

        textm.text = text;

        textObj.transform.parent = annoParent;
        textObj.transform.position = mid;
        Vector3 tmpPos = textObj.transform.localPosition;
        tmpPos.y += 1.0f;
        textObj.transform.localPosition =  tmpPos;
        textObj.transform.localScale = Vector3.one * 1.5f;

        textm.color = colorText;

        Debug.Log("Dihedral angle between " + atoms[0] + " & " + atoms[1] + " & " + atoms[2] + " & " + atoms[3] + " : " + text);
        go = textObj;

    }
    public override void Update() {
        if (p1 != atoms[0].position || p2 != atoms[1].position || p3 != atoms[2].position || p4 != atoms[3].position) {

            p1 = atoms[0].position;
            p2 = atoms[1].position;
            p3 = atoms[2].position;
            p4 = atoms[3].position;


            float dihe = UnityMolAnnotationManager.dihedral(p1, p2, p3, p4);

            Vector3 posA2 = annoParent.parent.TransformPoint(p2);
            Vector3 posA3 = annoParent.parent.TransformPoint(p3);

            Vector3 mid = (posA2 + posA3) * 0.5f;

            string text = dihe.ToString("F1") + "°";

            TextMesh textm = go.GetComponentsInChildren<TextMesh>()[0];
            textm.gameObject.GetComponent<MeshRenderer>().sortingOrder = 50;

            textm.text = text;

            go.transform.position = mid;
            Vector3 tmpPos = go.transform.localPosition;
            tmpPos.y += 1.0f;
            go.transform.localPosition =  tmpPos;

            textm.color = colorText;

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
        san.color = colorText;
        fillSerializedAtoms(san);
        return san;
    }
    public override int toAnnoType(){
        return 13;
    }
}
}