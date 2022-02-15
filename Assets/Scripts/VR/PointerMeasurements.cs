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
using System.Text;
using System.Linq;
using VRTK;
using UMol.API;

namespace UMol {

[RequireComponent(typeof(VRTK_Pointer), typeof(VRTK_ControllerEvents))]
[RequireComponent(typeof(VRTK_StraightPointerRendererNoRB))]
public class PointerMeasurements : MonoBehaviour {

    private MeasureMode prevMeasureMode = MeasureMode.distance;

    VRTK_Pointer pointer;

    VRTK_ControllerEvents controllerEvents;
    VRTK_StraightPointerRendererNoRB pointerR;

    int touchedAtoms = 0;

    Transform camTransform;

    PointerHoverAtom hoverScript;

    UnityMolSelectionManager selM;
    UnityMolAnnotationManager annoM;
    CustomRaycastBurst raycaster;

    UnityMolAtom[] atomsArray = new UnityMolAtom[4];

    void Start() {

        if (pointer == null) {
            pointer = GetComponent<VRTK_Pointer>();
        }
        if (pointerR == null) {
            pointerR = GetComponent<VRTK_StraightPointerRendererNoRB>();
        }
        controllerEvents = GetComponent<VRTK_ControllerEvents>();

        // pointer.PointerStateValid += DetectedCollision;
        // pointer.PointerStateInvalid += buttonOut;
        controllerEvents.ButtonTwoPressed += buttonPressed;
        controllerEvents.ButtonTwoReleased += buttonReleased;
        // pointer.SelectionButtonPressed += buttonPressed;
        // pointer.SelectionButtonReleased += buttonReleased;
        // pointer.ActivationButtonReleased += buttonReleased;

        touchedAtoms = 0;

        selM = UnityMolMain.getSelectionManager();
        annoM = UnityMolMain.getAnnotationManager();
        raycaster = UnityMolMain.getCustomRaycast();


    }

    void buttonPressed(object sender, ControllerInteractionEventArgs e) {

        if(prevMeasureMode != UnityMolMain.measureMode){
            prevMeasureMode = UnityMolMain.measureMode;
            resetTouchedAtoms();
        }


        UnityMolAtom a = raycaster.customRaycastAtomBurst(pointerR.actualContainer.transform.position, pointerR.actualTracer.transform.forward);

        if (a != null) {

            UnityMolStructureManager sm = UnityMolMain.getStructureManager();
            UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

            Transform atomPar = sm.structureToGameObject[a.residue.chain.model.structure.uniqueName].transform;

            string textAtom = a.ToString();

            Transform currentT = a.residue.chain.model.structure.atomToGo[a].transform;

            if (touchedAtoms == 4) {
                touchedAtoms = 0;
            }

            atomsArray[touchedAtoms] = a;

            if (atomsArray[touchedAtoms] == null || currentT == null) {
                Debug.LogError("Problem measuring atoms");
                resetTouchedAtoms();
                return;
            }

            if (touchedAtoms > 0 && atomsArray[touchedAtoms - 1] == atomsArray[touchedAtoms]) {
                //Touched the same atom = Stop measurements
                resetTouchedAtoms();
                return;
            }
            //Touched an atom from another molecule
            if (touchedAtoms >= 1) {
                bool sameStruc = true;
                string sName = atomsArray[0].residue.chain.model.structure.uniqueName;
                for (int i = 1; i <= touchedAtoms; i++) {
                    if (sName != atomsArray[i].residue.chain.model.structure.uniqueName) {
                        sameStruc = false;
                        break;
                    }
                }

                if (!sameStruc) {
                    Debug.LogWarning("No inter-molecule measurements allowed");
                    resetTouchedAtoms();
                    return;
                }
            }

            string s1Name = null;
            string s2Name = null;
            string s3Name = null;
            string s4Name = null;

            if (touchedAtoms == 0) {
                s1Name = atomsArray[0].residue.chain.model.structure.uniqueName;
                APIPython.annotateAtom(s1Name, atomsArray[0].idInAllAtoms);
                touchedAtoms++;
                return;
            }

            switch (UnityMolMain.measureMode) {
            case MeasureMode.distance:
                if (touchedAtoms == 1) {
                    s1Name = atomsArray[0].residue.chain.model.structure.uniqueName;
                    s2Name = atomsArray[1].residue.chain.model.structure.uniqueName;

                    APIPython.annotateLine(s1Name, atomsArray[0].idInAllAtoms,
                                           s2Name, atomsArray[1].idInAllAtoms);

                    APIPython.annotateDistance(s1Name, atomsArray[0].idInAllAtoms,
                                               s2Name, atomsArray[1].idInAllAtoms);

                    APIPython.annotateAtom(s2Name, atomsArray[1].idInAllAtoms);
                    resetTouchedAtoms();
                }
                break;
            case MeasureMode.angle:
                if (touchedAtoms == 1) {
                    s1Name = atomsArray[0].residue.chain.model.structure.uniqueName;
                    s2Name = atomsArray[1].residue.chain.model.structure.uniqueName;

                    APIPython.annotateLine(s1Name, atomsArray[0].idInAllAtoms,
                                           s2Name, atomsArray[1].idInAllAtoms);

                    APIPython.annotateAtom(s2Name, atomsArray[1].idInAllAtoms);
                    touchedAtoms++;
                }
                if (touchedAtoms == 2) {
                    s1Name = atomsArray[0].residue.chain.model.structure.uniqueName;
                    s2Name = atomsArray[1].residue.chain.model.structure.uniqueName;
                    s3Name = atomsArray[2].residue.chain.model.structure.uniqueName;

                    APIPython.annotateAtom(s3Name, atomsArray[2].idInAllAtoms);

                    APIPython.annotateLine(s2Name, atomsArray[1].idInAllAtoms,
                                           s3Name, atomsArray[2].idInAllAtoms);

                    APIPython.annotateAngle(s1Name, atomsArray[0].idInAllAtoms,
                                            s2Name, atomsArray[1].idInAllAtoms,
                                            s3Name, atomsArray[2].idInAllAtoms);

                    APIPython.annotateArcLine(s1Name, atomsArray[0].idInAllAtoms,
                                              s2Name, atomsArray[1].idInAllAtoms,
                                              s3Name, atomsArray[2].idInAllAtoms);

                    resetTouchedAtoms();
                }
                break;
            case MeasureMode.torsAngle:

                if (touchedAtoms == 1) {
                    s1Name = atomsArray[0].residue.chain.model.structure.uniqueName;
                    s2Name = atomsArray[1].residue.chain.model.structure.uniqueName;

                    APIPython.annotateLine(s1Name, atomsArray[0].idInAllAtoms,
                                           s2Name, atomsArray[1].idInAllAtoms);

                    APIPython.annotateAtom(s2Name, atomsArray[1].idInAllAtoms);
                    touchedAtoms++;
                }
                if (touchedAtoms == 2) {
                    s1Name = atomsArray[0].residue.chain.model.structure.uniqueName;
                    s2Name = atomsArray[1].residue.chain.model.structure.uniqueName;
                    s3Name = atomsArray[2].residue.chain.model.structure.uniqueName;

                    APIPython.annotateLine(s2Name, atomsArray[1].idInAllAtoms,
                                           s3Name, atomsArray[2].idInAllAtoms);

                    APIPython.annotateAtom(s3Name, atomsArray[2].idInAllAtoms);
                    touchedAtoms++;
                }
                if (touchedAtoms == 3) {
                    s1Name = atomsArray[0].residue.chain.model.structure.uniqueName;
                    s2Name = atomsArray[1].residue.chain.model.structure.uniqueName;
                    s3Name = atomsArray[2].residue.chain.model.structure.uniqueName;
                    s4Name = atomsArray[3].residue.chain.model.structure.uniqueName;

                    APIPython.annotateAtom(s4Name, atomsArray[3].idInAllAtoms);

                    APIPython.annotateLine(s3Name, atomsArray[2].idInAllAtoms,
                                           s4Name, atomsArray[3].idInAllAtoms);
                    APIPython.annotateDihedralAngle(s1Name, atomsArray[0].idInAllAtoms,
                                                    s2Name, atomsArray[1].idInAllAtoms,
                                                    s3Name, atomsArray[2].idInAllAtoms,
                                                    s4Name, atomsArray[3].idInAllAtoms);

                    APIPython.annotateRotatingArrow(s2Name, atomsArray[1].idInAllAtoms,
                                                    s3Name, atomsArray[2].idInAllAtoms);
                    resetTouchedAtoms();
                }
                break;
            }
        }

    }

    void buttonOut(object sender, DestinationMarkerEventArgs e) {
        gameObject.GetComponent<VRTK_UIPointer>().enabled = true;
    }

    void buttonReleased(object sender, ControllerInteractionEventArgs e) {
        gameObject.GetComponent<VRTK_UIPointer>().enabled = true;
    }
    public void resetTouchedAtoms() {
        touchedAtoms = 0;
        gameObject.GetComponent<VRTK_UIPointer>().enabled = true;
    }
}

public enum MeasureMode {
    distance = 0,
    angle = 1,
    torsAngle = 2
}

}