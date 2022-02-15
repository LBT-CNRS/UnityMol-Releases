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
public class TextAnnotation : UnityMolAnnotation {

    public bool showLine = true;
    public string content;
    public Color colorText = new Color(0.0f, 0.0f, 0.5f, 1.0f);

    public float lineWidth = 0.1f;
    public float textDistToAtom = 2.0f;

    GameObject linkedLine;
    Transform annoBG;

    public override void Create() {

        if (atoms == null || atoms.Count != 1) {
            Debug.LogError("Could not create TextAnnotation, 'atoms' list is not correctly set");
            return;
        }

        GameObject textObj = GameObject.Instantiate((GameObject) Resources.Load("Prefabs/SphereOverAtom"));
        textObj.name = "TextAnnotation";
        textObj.GetComponent<MeshRenderer>().enabled = false;
        TextMesh textm = textObj.GetComponentsInChildren<TextMesh>()[0];
        textm.gameObject.GetComponent<MeshRenderer>().sortingOrder = 50;

        textm.text = content;

        textObj.transform.parent = annoParent;
        textObj.transform.localPosition = Vector3.zero;
        textObj.transform.localScale = new Vector3(1.5f, 1.5f, 0.01f);

        textm.color = colorText;
        go = textObj;

        if (showLine) {
            annoBG = go.transform.Find("Text/BG");
            annoBG.parent.localPosition = -annoBG.parent.up * textDistToAtom;

            Renderer rd = annoBG.GetComponent<Renderer>();
            Vector3 topPos = rd.bounds.center + textObj.transform.up * rd.bounds.extents.y;

            linkedLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            linkedLine.name = "textToLabelLine";
            linkedLine.layer = annoBG.gameObject.layer;
            GameObject.Destroy(linkedLine.GetComponent<BoxCollider>());
            linkedLine.transform.parent = go.transform;

            Vector3 atomPos = atoms[0].curWorldPosition;
            Vector3 vec = atomPos - topPos;
            float dist = Vector3.Distance(atomPos, topPos);
            linkedLine.transform.localScale = new Vector3(lineWidth, lineWidth, dist/go.transform.lossyScale.x);
            linkedLine.transform.position = topPos + (vec * 0.5f);
            linkedLine.transform.LookAt(topPos);

            //Creates a new material instance 
            linkedLine.GetComponent<MeshRenderer>().material.color = colorText;

        }

    }
    public override void Update() {
    }
    public override void UnityUpdate() {
        if (showLine) {

            Renderer rd = annoBG.GetComponent<Renderer>();
            Vector3 topPos = rd.bounds.center + go.transform.up * rd.bounds.extents.y;

            Vector3 atomPos = atoms[0].curWorldPosition;
            Vector3 vec = atomPos - topPos;
            float dist = Vector3.Distance(atomPos, topPos);
            linkedLine.transform.localScale = new Vector3(lineWidth, lineWidth, dist/go.transform.lossyScale.x);
            linkedLine.transform.position = topPos + (vec * 0.5f);
            linkedLine.transform.LookAt(topPos);

        }

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