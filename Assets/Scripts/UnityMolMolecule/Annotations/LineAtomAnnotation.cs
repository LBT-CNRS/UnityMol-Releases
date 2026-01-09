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
public class LineAtomAnnotation : UnityMolAnnotation {

    public float sizeLine = 0.005f;
    public Color colorLine = new Color(0.0f, 0.0f, 0.5f, 1.0f);

    Vector3 p1;
    Vector3 p2;

    public override void Create() {
        if (atoms == null || atoms.Count != 2) {
            Debug.LogError("Could not create LineAtomAnnotation, 'atoms' list is not correctly set");
            return;
        }

        p1 = atoms[0].position;
        p2 = atoms[1].position;

        GameObject lineObject = new GameObject("distanceLine");
        lineObject.transform.parent = annoParent;
        lineObject.transform.localPosition = Vector3.zero;
        lineObject.transform.localScale = Vector3.one;
        lineObject.transform.localRotation = Quaternion.identity;

        LineRenderer curLine = lineObject.AddComponent<LineRenderer>();
        curLine.useWorldSpace = false;
        curLine.positionCount = 2;          // initialize to one line segment

        Shader lineShader = Shader.Find("Particles/Alpha Blended");
        if (lineShader == null)
            lineShader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");

        curLine.material = new Material (lineShader);
        curLine.startColor = curLine.endColor = colorLine;
        curLine.startWidth = curLine.endWidth = sizeLine;
        curLine.alignment = LineAlignment.View;             // have line always face viewer

        Vector3 transformedPosition1 = annoParent.parent.TransformPoint(atoms[0].position);
        Vector3 transformedPosition2 = annoParent.parent.TransformPoint(atoms[1].position);

        curLine.SetPosition(0, annoParent.InverseTransformPoint(transformedPosition1));
        curLine.SetPosition(1, annoParent.InverseTransformPoint(transformedPosition2));

        go = lineObject;
    }
    public override void Update() {
        if (p1 != atoms[0].position || p2 != atoms[1].position) {

            p1 = atoms[0].position;
            p2 = atoms[1].position;

            LineRenderer curLine = go.GetComponent<LineRenderer>();
;
            curLine.startColor = curLine.endColor = colorLine;
            curLine.startWidth = curLine.endWidth = sizeLine;

            Vector3 transformedPosition1 = annoParent.parent.TransformPoint(p1);
            Vector3 transformedPosition2 = annoParent.parent.TransformPoint(p2);

            curLine.SetPosition(0, annoParent.InverseTransformPoint(transformedPosition1));
            curLine.SetPosition(1, annoParent.InverseTransformPoint(transformedPosition2));
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
        san.size = sizeLine;
        fillSerializedAtoms(san);
        return san;
    }
    public override int toAnnoType() {
        return 9;
    }
}
}
