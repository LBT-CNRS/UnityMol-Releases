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
public class ArcLineAnnotation : UnityMolAnnotation {

    public Color colorLine = new Color(0.0f, 0.0f, 0.5f, 1.0f);
    public float sizeLine = 0.0025f;

    Vector3 p1;
    Vector3 p2;
    Vector3 p3;

    public override void Create() {

        if (atoms == null || atoms.Count != 3) {
            Debug.LogError("Could not create ArcLineAnnotation, 'atoms' list is not correctly set");
            return;
        }

        p1 = atoms[0].position;
        p2 = atoms[1].position;
        p3 = atoms[2].position;

        GameObject lineObject = new GameObject("angleLine");
        lineObject.transform.parent = annoParent;
        lineObject.transform.localRotation = Quaternion.identity;
        lineObject.transform.localPosition = Vector3.zero;
        lineObject.transform.localScale = Vector3.one;


        LineRenderer curLine = lineObject.AddComponent<LineRenderer>();
        ArcLine arc = lineObject.AddComponent<ArcLine>();
        curLine.useWorldSpace = false;

        Shader lineShader = Shader.Find("Particles/Alpha Blended");
        if (lineShader == null)
            lineShader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");

        curLine.material = new Material (lineShader);
        curLine.SetColors(colorLine, colorLine);
        curLine.alignment = LineAlignment.View;             // have line always face viewer

        curLine.SetWidth(sizeLine, sizeLine);

        Vector3 posA1 = annoParent.parent.TransformPoint(p1);
        Vector3 posA2 = annoParent.parent.TransformPoint(p2);
        Vector3 posA3 = annoParent.parent.TransformPoint(p3);

        arc.A = posA1;
        arc.B = posA2;
        arc.C = posA3;

        arc.UpdatePointLine();

        go = lineObject;

    }
    public override void Update() {
        if (p1 != atoms[0].position || p2 != atoms[1].position || p3 != atoms[2].position) {

            p1 = atoms[0].position;
            p2 = atoms[1].position;
            p3 = atoms[2].position;

            LineRenderer curLine = go.GetComponent<LineRenderer>();
            ArcLine arc = go.GetComponent<ArcLine>();


            curLine.SetColors(colorLine, colorLine);

            curLine.SetWidth(sizeLine, sizeLine);

            Vector3 posA1 = annoParent.parent.TransformPoint(p1);
            Vector3 posA2 = annoParent.parent.TransformPoint(p2);
            Vector3 posA3 = annoParent.parent.TransformPoint(p3);

            arc.A = posA1;
            arc.B = posA2;
            arc.C = posA3;

            arc.UpdatePointLine();

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