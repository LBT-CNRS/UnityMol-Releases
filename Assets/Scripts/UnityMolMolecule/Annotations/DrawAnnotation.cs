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
public class DrawAnnotation : UnityMolAnnotation {

    public int id;//Unique id given by AnnotationManager
    public Color colorLine = Color.black;
    public float sizeLine = 0.005f;
    public List<Vector3> positions = new List<Vector3>();

    public override void Create() {
        if (atoms == null || atoms.Count != 1) {
            Debug.LogError("Could not create DrawAnnotation, 'atoms' list is not correctly set");
            return;
        }

        GameObject lineObject = new GameObject("drawLine");
        lineObject.transform.parent = annoParent;
        lineObject.transform.localPosition = Vector3.zero;
        lineObject.transform.localScale = Vector3.one;
        lineObject.transform.localRotation = Quaternion.identity;

        lineObject.AddComponent<MeshFilter>();
        MeshRenderer mr = lineObject.AddComponent<MeshRenderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = colorLine;
        mr.sharedMaterial = mat;

        MeshLineRenderer lr = lineObject.AddComponent<MeshLineRenderer>();
        lr.Init();
        lr.setWidth(sizeLine);

        for (int i = 0; i < positions.Count; i++) {
            lr.AddPoint(positions[i]);
        }

        go = lineObject;
    }
    public override void Update() {

    }
    public override void Delete() {
        if (go != null) {
            GameObject.Destroy(go.GetComponent<MeshRenderer>().sharedMaterial);
            GameObject.Destroy(go);
        }
        if (positions != null) {
            positions.Clear();
        }
    }
    public override void UnityUpdate() {
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
        san.id = id;
        san.size = sizeLine;
        san.positions = new List<Vector3>(positions.Count);
        san.positions.AddRange(positions);
        fillSerializedAtoms(san);
        return san;
    }
    public override int toAnnoType() {
        return 8;
    }
}
}