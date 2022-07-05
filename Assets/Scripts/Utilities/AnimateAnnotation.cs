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
using System.Collections;
using System.Collections.Generic;

namespace UMol {

public class AnimateAnnotation : MonoBehaviour {

    public enum AnimationMode {
        line = 0,
        distance = 1,
        angle = 2,
        torsion = 3,
    }

    public bool updateRotation = false;

    public AnimationMode mode = AnimationMode.line;

    public Transform atomT;
    public LineRenderer distanceLine;
    // public CurvedLine angleLine;
    public ArcLine angleLine;
    public GameObject arrowsTorsion;


    public UnityMolAtom a1;
    public UnityMolAtom a2;
    public UnityMolAtom a3;
    public UnityMolAtom a4;

    private Camera mainCam;

    void Start() {
        mainCam = Camera.main;
    }
    void Update() {
        if (updateRotation) {
            if (mainCam != null) {
                transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position);
            }
        }

        if (!shouldUpdate()) {
            return;
        }

        if (mode == AnimationMode.line){
            updateDistanceLine();
        }
        else if (mode == AnimationMode.distance) {
            updateDistanceText();
        }
        else if (mode == AnimationMode.angle) {
            updateAngleText();
            updateAngleLine();
        }
        else if (mode == AnimationMode.torsion) {
            updateTorsionText();
            updateTorsionArrows();
        }
    }

    bool shouldUpdate() {
        if (atomT != null && a1 != null && a2 != null) {
            UnityMolStructure s = a1.residue.chain.model.structure;
            return s.trajectoryLoaded;
        }
        return false;
    }

    void updateDistanceText() {
        if(GetComponent<TextMesh>() != null){
            float dist = Vector3.Distance(a1.position, a2.position);

            Vector3 tPos1 = atomT.parent.TransformPoint(a1.position);
            Vector3 tPos2 = atomT.parent.TransformPoint(a2.position);

            string text = dist.ToString("F1") + "\u212B";
            transform.position = (tPos1 + tPos2) * 0.5f;

            TextMesh textm = GetComponent<TextMesh>();
            textm.text = text;
        }
    }

    void updateDistanceLine() {
        if (distanceLine != null) {
            Vector3 transformedPosition1 = atomT.parent.TransformPoint(a1.position);
            Vector3 transformedPosition2 = atomT.parent.TransformPoint(a2.position);

            distanceLine.SetPosition(0, atomT.InverseTransformPoint(transformedPosition1));
            distanceLine.SetPosition(1, atomT.InverseTransformPoint(transformedPosition2));
        }
    }

    void updateAngleText() {
        if (a3 == null) {
            return;
        }
        float angle = Vector3.Angle(a1.position - a2.position, a3.position - a2.position);

        Vector3 posA1 = atomT.parent.TransformPoint(a1.position);
        Vector3 posA2 = atomT.parent.TransformPoint(a2.position);
        Vector3 posA3 = atomT.parent.TransformPoint(a3.position);
        Vector3 mid = (posA1 + posA3) * 0.5f;

        Vector3 pos = posA2 + (mid - posA2) * 0.35f;

        string text = angle.ToString("F1") + "°";

        transform.position = pos;
        TextMesh textm = transform.GetChild(0).gameObject.GetComponent<TextMesh>();
        textm.text = text;

    }
    void updateAngleLine() {
        if (angleLine != null) {

            Vector3 posA1 = atomT.parent.TransformPoint(a1.position);
            Vector3 posA2 = atomT.parent.TransformPoint(a2.position);
            Vector3 posA3 = atomT.parent.TransformPoint(a3.position);

            // Vector3 mid = (posA1 + posA3) * 0.5f;

            // Vector3[] positions = new Vector3[3];
            // positions[0] = atomT.InverseTransformPoint(posA2 + (posA1 - posA2) * 0.25f);
            // positions[1] = atomT.InverseTransformPoint(posA2 + (mid - posA2) * 0.35f);
            // positions[2] = atomT.InverseTransformPoint(posA2 + (posA3 - posA2) * 0.25f);

            // angleLine.linePositions = positions;

            angleLine.A = posA1;
            angleLine.B = posA2;
            angleLine.C = posA3;

            angleLine.UpdatePointLine();
        }

    }

    void updateTorsionText() {
        if (a4 == null) {
            return;
        }

        float dihe = UnityMolAnnotationManager.dihedral(a1.position, a2.position, a3.position, a4.position);

        Vector3 posA2 = atomT.parent.TransformPoint(a2.position);
        Vector3 posA3 = atomT.parent.TransformPoint(a3.position);

        Vector3 mid = (posA2 + posA3) * 0.5f;

        string text = dihe.ToString("F1") + "°";


        transform.position = mid;
        Vector3 tmpPos = transform.localPosition;
        tmpPos.y += 1.0f;
        transform.localPosition =  tmpPos;

        TextMesh textm = transform.GetChild(0).gameObject.GetComponent<TextMesh>();
        textm.text = text;
    }
    void updateTorsionArrows() {
        //Rotation of the arrows is done in AnimateTorsionAngle
        if (arrowsTorsion != null) {
            Vector3 posA2 = atomT.parent.TransformPoint(a2.position);
            Vector3 posA3 = atomT.parent.TransformPoint(a3.position);
            Vector3 mid = (posA2 + posA3) * 0.5f;

            arrowsTorsion.transform.position = mid;

        }
    }
}
}