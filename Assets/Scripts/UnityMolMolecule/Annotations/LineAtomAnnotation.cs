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
        curLine.SetColors(colorLine, colorLine);
        curLine.SetWidth(sizeLine, sizeLine);
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

            curLine.SetColors(colorLine, colorLine);
            curLine.SetWidth(sizeLine, sizeLine);

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
            go.SetActive(show);
        }
    }

}
}