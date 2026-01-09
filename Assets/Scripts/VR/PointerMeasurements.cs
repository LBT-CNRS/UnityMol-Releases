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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;

using UMol.API;

namespace UMol {
[RequireComponent(typeof(ViveRoleSetter))]
[RequireComponent(typeof(AudioSource))]
public class PointerMeasurements : MonoBehaviour {

    private MeasureMode prevMeasureMode = MeasureMode.distance;

    int touchedAtoms = 0;

    Transform camTransform;

    PointerHoverAtom hoverScript;

    UnityMolSelectionManager selM;
    UnityMolAnnotationManager annoM;
    CustomRaycastBurst raycaster;

    UnityMolAtom[] atomsArray = new UnityMolAtom[4];
    Vector3[] posExtrArray = new Vector3[4];

    AudioSource source;

    ViveRoleProperty curRole;

    void OnEnable() {
        curRole = GetComponent<ViveRoleSetter>().viveRole;

        if (curRole != null) {
            ViveInput.AddPressDown((HandRole)curRole.roleValue, ControllerButton.Menu, buttonPressed);
        }
    }
    void OnDisable() {
        if (curRole != null) {
            ViveInput.RemovePressDown((HandRole)curRole.roleValue, ControllerButton.Menu, buttonPressed);
        }
    }

    void Start() {

        source = GetComponent<AudioSource>();

        touchedAtoms = 0;

        selM = UnityMolMain.getSelectionManager();
        annoM = UnityMolMain.getAnnotationManager();
        raycaster = UnityMolMain.getCustomRaycast();

    }

    void buttonPressed() {

        if (prevMeasureMode != UnityMolMain.measureMode) {
            prevMeasureMode = UnityMolMain.measureMode;
            resetTouchedAtoms();
        }

        RigidPose cpose = VivePose.GetPose(curRole);

        Vector3 p = Vector3.zero;
        bool isExtrAtom = false;
        UnityMolAtom a = raycaster.customRaycastAtomBurst(
                             cpose.pos,
                             cpose.forward,
                             ref p, ref isExtrAtom, false);

        if (a != null) {

            UnityMolStructureManager sm = UnityMolMain.getStructureManager();
            UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

            string textAtom = a.ToString();

            if (touchedAtoms == 4) {
                touchedAtoms = 0;
            }

            switch (UnityMolMain.measureMode) {
            case MeasureMode.distance:
                if (touchedAtoms >= 2)
                    touchedAtoms = 0;
                break;
            case MeasureMode.angle:
                if (touchedAtoms >= 3)
                    touchedAtoms = 0;
                break;
            case MeasureMode.torsAngle:
                if (touchedAtoms >= 4)
                    touchedAtoms = 0;
                break;
            }

            atomsArray[touchedAtoms] = a;
            posExtrArray[touchedAtoms] = p;

            if (source != null)
                AudioSource.PlayClipAtPoint(source.clip, p);

            if (atomsArray[touchedAtoms] == null) {
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
                string sName = atomsArray[0].residue.chain.model.structure.name;
                for (int i = 1; i <= touchedAtoms; i++) {
                    if (sName != atomsArray[i].residue.chain.model.structure.name) {
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
                s1Name = atomsArray[0].residue.chain.model.structure.name;

                if (!isExtrAtom) {
                    APIPython.annotateAtom(s1Name, (int)atomsArray[0].number);
                }
                else {
                    Vector3 losS = atomsArray[0].residue.chain.model.structure.annotationParent.transform.lossyScale;
                    APIPython.annotateSphere(posExtrArray[0], losS.x);
                }
                touchedAtoms++;
                return;
            }

            switch (UnityMolMain.measureMode) {
            case MeasureMode.distance:
                if (touchedAtoms == 1) {
                    s1Name = atomsArray[0].residue.chain.model.structure.name;
                    s2Name = atomsArray[1].residue.chain.model.structure.name;

                    if (!isExtrAtom) {

                        APIPython.annotateLine(s1Name, (int)atomsArray[0].number,
                                               s2Name, (int)atomsArray[1].number);

                        APIPython.annotateDistance(s1Name, (int)atomsArray[0].number,
                                                   s2Name, (int)atomsArray[1].number);

                        APIPython.annotateAtom(s2Name, (int)atomsArray[1].number);
                    }
                    else {
                        Transform sPar = atomsArray[0].residue.chain.model.structure.annotationParent.transform.parent;
                        Vector3 a1pos = sPar.InverseTransformPoint(posExtrArray[0]);
                        Vector3 a2pos = sPar.InverseTransformPoint(posExtrArray[1]);

                        float dist = Vector3.Distance(a1pos, a2pos);

                        string distText = dist.ToString("F1") + "\u212B";
                        float sizeLine = 0.005f;//TODO: probably not the correct value => compute that
                        APIPython.annotateWorldLine(posExtrArray[0], posExtrArray[1],
                                                    sizeLine, new Color(0.0f, 0.0f, 0.5f, 1.0f));

                        float scaleText = 1.0f;//TODO: probably not the correct value => compute that
                        APIPython.annotateWorldText((posExtrArray[0] + posExtrArray[1]) * 0.5f,
                                                    scaleText, distText, new Color(0.0f, 0.0f, 0.5f, 1.0f));


                        Vector3 losS = atomsArray[1].residue.chain.model.structure.annotationParent.transform.lossyScale;
                        APIPython.annotateSphere(posExtrArray[1], losS.x);
                    }
                    resetTouchedAtoms();
                }
                break;
            case MeasureMode.angle:
                if (touchedAtoms == 1) {
                    s1Name = atomsArray[0].residue.chain.model.structure.name;
                    s2Name = atomsArray[1].residue.chain.model.structure.name;

                    APIPython.annotateLine(s1Name, (int)atomsArray[0].number,
                                           s2Name, (int)atomsArray[1].number);

                    APIPython.annotateAtom(s2Name, (int)atomsArray[1].number);
                    touchedAtoms++;
                    break;
                }
                if (touchedAtoms == 2) {
                    s1Name = atomsArray[0].residue.chain.model.structure.name;
                    s2Name = atomsArray[1].residue.chain.model.structure.name;
                    s3Name = atomsArray[2].residue.chain.model.structure.name;

                    APIPython.annotateAtom(s3Name, (int)atomsArray[2].number);

                    APIPython.annotateLine(s2Name, (int)atomsArray[1].number,
                                           s3Name, (int)atomsArray[2].number);

                    APIPython.annotateAngle(s1Name, (int)atomsArray[0].number,
                                            s2Name, (int)atomsArray[1].number,
                                            s3Name, (int)atomsArray[2].number);

                    APIPython.annotateArcLine(s1Name, (int)atomsArray[0].number,
                                              s2Name, (int)atomsArray[1].number,
                                              s3Name, (int)atomsArray[2].number);

                    resetTouchedAtoms();
                }
                break;
            case MeasureMode.torsAngle:

                if (touchedAtoms == 1) {
                    s1Name = atomsArray[0].residue.chain.model.structure.name;
                    s2Name = atomsArray[1].residue.chain.model.structure.name;

                    APIPython.annotateLine(s1Name, (int)atomsArray[0].number,
                                           s2Name, (int)atomsArray[1].number);

                    APIPython.annotateAtom(s2Name, (int)atomsArray[1].number);
                    touchedAtoms++;
                    break;
                }
                if (touchedAtoms == 2) {
                    s1Name = atomsArray[0].residue.chain.model.structure.name;
                    s2Name = atomsArray[1].residue.chain.model.structure.name;
                    s3Name = atomsArray[2].residue.chain.model.structure.name;

                    APIPython.annotateLine(s2Name, (int)atomsArray[1].number,
                                           s3Name, (int)atomsArray[2].number);

                    APIPython.annotateAtom(s3Name, (int)atomsArray[2].number);
                    touchedAtoms++;
                    break;
                }
                if (touchedAtoms == 3) {
                    s1Name = atomsArray[0].residue.chain.model.structure.name;
                    s2Name = atomsArray[1].residue.chain.model.structure.name;
                    s3Name = atomsArray[2].residue.chain.model.structure.name;
                    s4Name = atomsArray[3].residue.chain.model.structure.name;

                    APIPython.annotateAtom(s4Name, (int)atomsArray[3].number);

                    APIPython.annotateLine(s3Name, (int)atomsArray[2].number,
                                           s4Name, (int)atomsArray[3].number);
                    APIPython.annotateDihedralAngle(s1Name, (int)atomsArray[0].number,
                                                    s2Name, (int)atomsArray[1].number,
                                                    s3Name, (int)atomsArray[2].number,
                                                    s4Name, (int)atomsArray[3].number);

                    APIPython.annotateRotatingArrow(s2Name, (int)atomsArray[1].number,
                                                    s3Name, (int)atomsArray[2].number);
                    resetTouchedAtoms();
                }
                break;
            }
        }

    }

    public void resetTouchedAtoms() {
        touchedAtoms = 0;
    }
}

public enum MeasureMode {
    distance = 0,
    angle = 1,
    torsAngle = 2
}

}