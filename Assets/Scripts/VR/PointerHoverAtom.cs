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
using System.Text;

using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;

namespace UMol {
[RequireComponent(typeof(ViveRoleSetter))]
[RequireComponent(typeof(PointerAtomSelection))]
public class PointerHoverAtom : MonoBehaviour {

    GameObject trajExtraGo;
    GameObject haloGo;
    TextMesh textm;
    Transform camTransform;

    bool pressed = false;
    CustomRaycastBurst raycaster;

    float hoverScaleMultiplier = 1.0f;

    public bool pauseHovering = false;
    UnityMolAtom lastPointedAtom = null;

    ViveRoleProperty curRole;

    PointerAtomSelection pas;
    GameObject goAtom;


    void OnEnable() {
        curRole = GetComponent<ViveRoleSetter>().viveRole;

        pas = GetComponent<PointerAtomSelection>();
        if (curRole != null) {
            ViveInput.AddPressDown((HandRole)curRole.roleValue, ControllerButton.PadTouch, buttonPressed);
            ViveInput.AddPressUp((HandRole)curRole.roleValue, ControllerButton.PadTouch, buttonReleased);
        }
    }
    void OnDisable() {
        if (curRole != null) {
            ViveInput.RemovePressDown((HandRole)curRole.roleValue, ControllerButton.PadTouch, buttonPressed);
            ViveInput.RemovePressUp((HandRole)curRole.roleValue, ControllerButton.PadTouch, buttonReleased);
        }
    }

    void Start() {
        raycaster = UnityMolMain.getCustomRaycast();

        haloGo = GameObject.Instantiate((GameObject) Resources.Load("Prefabs/SphereOverAtom"));
        textm = haloGo.GetComponentsInChildren<TextMesh>()[0];

        // haloGo.layer = LayerMask.NameToLayer("Ignore Raycast");
        haloGo.SetActive(false);
        textm.gameObject.GetComponent<MeshRenderer>().sortingOrder = 50;

        trajExtraGo = new GameObject("DummyTrajExtractedGo");
        goAtom = new GameObject("HoverAtomGo");
        DontDestroyOnLoad(trajExtraGo);
        DontDestroyOnLoad(haloGo);
        DontDestroyOnLoad(goAtom);
    }

    void Update() {
        if (pauseHovering) {
            disableHovering();
            return;
        }
        if (pressed && !pas.isOverUI) {
            showHover();
        }
    }
    public static string formatAtomText(UnityMolAtom a) {
        string nameS = a.residue.chain.model.structure.FormatName(25);


        string textAtom = "<size=30>" + nameS + " </size>\n";

        textAtom += "<color=white>" + a.residue.chain.name + "</color> | ";
        textAtom += "<color=white>" + a.residue.name + a.residue.id + "</color> | ";
        textAtom += "<b>" + a.name + "</b>";
        // textAtom = ReplaceFirstOccurrance(textAtom, "<", "");
        // textAtom = ReplaceFirstOccurrance(textAtom, "|", "\n");
        // textAtom = ReplaceFirstOccurrance(textAtom, ">", "");


        return textAtom;
    }
    void showHover() {


        RigidPose cpose = VivePose.GetPose(curRole);

        Vector3 p = Vector3.zero;
        bool isExtrAtom = false;
        UnityMolAtom a = raycaster.customRaycastAtomBurst(
                             cpose.pos,
                             cpose.forward,
                             ref p, ref isExtrAtom, true);
        if (a != null) {
            if (haloGo == null) {
                haloGo = GameObject.Instantiate((GameObject) Resources.Load("Prefabs/SphereOverAtom"));
                textm = haloGo.GetComponentsInChildren<TextMesh>()[0];
                haloGo.layer = LayerMask.NameToLayer("Ignore Raycast");
                haloGo.SetActive(false);
                DontDestroyOnLoad(haloGo);
            }

            UnityMolStructure s = a.residue.chain.model.structure;


            textm.text = formatAtomText(a);
            haloGo.SetActive(true);

            haloGo.transform.position = p;
            trajExtraGo.transform.position = p;

            if (camTransform == null) {
                camTransform = Camera.main.transform;
            }

            haloGo.transform.rotation = Quaternion.LookRotation(haloGo.transform.position - camTransform.position);

            UnityMolMain.getAnnotationManager().setGOPos(a, goAtom);

            if (!isExtrAtom)
                haloGo.transform.SetParent(goAtom.transform);
            else {
                trajExtraGo.transform.SetParent(goAtom.transform.parent);
                trajExtraGo.transform.localScale = goAtom.transform.localScale;
                haloGo.transform.SetParent(trajExtraGo.transform);
            }

            haloGo.transform.localScale =  hoverScaleMultiplier * a.radius * Vector3.one * 1.1f;

            if (lastPointedAtom == null || lastPointedAtom != a) {
                ViveInput.TriggerHapticPulseEx(curRole.roleType, curRole.roleValue, 500);
            }
            lastPointedAtom = a;
        }

    }
    void disableHovering() {
        if (haloGo != null) {
            haloGo.SetActive(false);
            haloGo.transform.parent = null;
        }
    }

    void buttonReleased() {
        disableHovering();
        pressed = false;
        lastPointedAtom = null;
    }
    void buttonPressed() {
        pressed = true;
    }
    public static string ReplaceFirstOccurrance(string original, string oldValue, string newValue)
    {
        if (string.IsNullOrEmpty(original))
            return "";
        if (string.IsNullOrEmpty(oldValue))
            return original;
        int loc = original.IndexOf(oldValue);
        return original.Remove(loc, oldValue.Length).Insert(loc, newValue);
    }
}
}
