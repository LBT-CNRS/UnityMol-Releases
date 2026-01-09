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
using System.Collections;
using System.Globalization;
using System.Text;

using UMol.API;

using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;

namespace UMol {
[RequireComponent(typeof(ViveRoleSetter))]
[RequireComponent(typeof(AudioSource))]
public class PointerAtomSelection : MonoBehaviour {

    UnityMolSelectionManager selM;
    CustomRaycastBurst raycaster;
    private AudioSource source;
    public bool isOverUI = false;

    bool isPressed = false;
    bool longPress = false;
    UnityMolAtom hoveredAtom = null;
    float longPressLimit = 0.4f;
    float timePressed = 0.0f;
    float initDistPointerToAtom = 0.0f;
    List<UnityMolAtom> curAllAtoms = null;
    GameObject selSphere;
    float curSelRad;
    ViveRoleProperty curRole;

    void OnEnable() {

        curRole = GetComponent<ViveRoleSetter>().viveRole;

        raycaster = UnityMolMain.getCustomRaycast();

        selM = UnityMolMain.getSelectionManager();

        selSphere = GameObject.Instantiate((GameObject) Resources.Load("Prefabs/SelectionSphere"));
        
        DontDestroyOnLoad(selSphere);
        selSphere.SetActive(false);

        source = GetComponent<AudioSource>();

        if (curRole != null) {
            ViveInput.AddPressDown((HandRole)curRole.roleValue, ControllerButton.Pad, padClicked);
            ViveInput.AddPressUp((HandRole)curRole.roleValue, ControllerButton.Pad, buttonReleased);
        }

        isPressed = false;
        longPress = false;
    }


    void OnDisable() {
        if (curRole != null) {
            ViveInput.RemovePressDown((HandRole)curRole.roleValue, ControllerButton.Pad, padClicked);
            ViveInput.RemovePressUp((HandRole)curRole.roleValue, ControllerButton.Pad, buttonReleased);
        }
    }

    void padClicked() {

        Vector3 p = Vector3.zero;
        bool isExtrAtom = false;

        RigidPose cpose = VivePose.GetPose(curRole);

        UnityMolAtom a = raycaster.customRaycastAtomBurst(
                             cpose.pos,
                             cpose.forward,
                             ref p, ref isExtrAtom, false);

        if (a == null) {
            return;
        }

        if (source != null) {
            AudioSource.PlayClipAtPoint(source.clip, a.curWorldPosition);
        }

        hoveredAtom = a;
        timePressed = Time.realtimeSinceStartup;
        initDistPointerToAtom = Vector3.Distance(cpose.pos, a.curWorldPosition);
        isPressed = true;

        UnityMolSelection hoveredSelection = null;
        if (selM.selectionMode == UnityMolSelectionManager.SelectionMode.Atom) {
            hoveredSelection = hoveredAtom.ToSelection();
        }
        else if (selM.selectionMode == UnityMolSelectionManager.SelectionMode.Residue) {
            UnityMolResidue hoveredRes = hoveredAtom.residue;
            hoveredSelection = hoveredRes.ToSelection(false);
        }
        else if (selM.selectionMode == UnityMolSelectionManager.SelectionMode.Chain) {
            UnityMolChain hoveredChain = hoveredAtom.residue.chain;
            hoveredSelection = hoveredChain.ToSelection(false);
        }
        // else if(selM.selectionMode == UnityMolSelectionManager.SelectionMode.Molecule){
        // }

        // APIPython.clearSelections();

        if (selM.onNewClickSelection != null) {
            selM.onNewClickSelection(new NewSelEventArgs(hoveredSelection));
        }

        int countBeforeAdding = 0;

        if (selM.currentSelection == null || !selM.currentSelection.isAlterable) {
            selM.getClickSelection();
        }

        UnityMolSelection curSel = selM.currentSelection;

        countBeforeAdding = curSel.Count;

        UnityMolSelection newSel = API.APIPython.select(hoveredSelection.MDASelString, curSel.name, true,
                                   addToExisting: true, silent: true);

        int afterAdding = newSel.Count;

        if (countBeforeAdding == afterAdding) {
            API.APIPython.removeFromSelection(hoveredSelection.MDASelString, curSel.name, silent: true);
        }
        curSel = selM.currentSelection;
        Debug.Log(curSel);

        // API.APIPython.updateRepresentations(curSel.name);

    }
    void buttonReleased() {

        if (longPress) {
            curAllAtoms = new List<UnityMolAtom>();

            UnityMolStructureManager sm = UnityMolMain.getStructureManager();

            foreach (UnityMolStructure s in sm.loadedStructures) {
                curAllAtoms.AddRange(s.currentModel.allAtoms);
            }

            Transform loadedMol = UnityMolMain.getRepresentationParent().transform;

            //Transform the world space position into the molecular space position
            Vector3 sphereCenter = loadedMol.InverseTransformPoint(hoveredAtom.curWorldPosition);
            float radInAng = curSelRad / loadedMol.transform.lossyScale.x;

            string selMDA = "insphere " + sphereCenter.x.ToString("F5", CultureInfo.InvariantCulture) + " " +
                            sphereCenter.y.ToString("F5", CultureInfo.InvariantCulture) + " " + sphereCenter.z.ToString("F5", CultureInfo.InvariantCulture) + " " +
                            radInAng.ToString("F3", CultureInfo.InvariantCulture);

            if (selM.selectionMode == UnityMolSelectionManager.SelectionMode.Residue || selM.selectionMode == UnityMolSelectionManager.SelectionMode.Chain) {
                selMDA = "byres " + selMDA;
            }


            if (selM.currentSelection == null || !selM.currentSelection.isAlterable) {
                selM.getClickSelection();
            }

            UnityMolSelection curSel = selM.currentSelection;

            UnityMolSelection newSel = API.APIPython.select(selMDA, curSel.name, createSelection: true, setAsCurrentSelection: true,
                                       addToExisting: true, silent: true);


            if (selM.onNewClickSelection != null) {
                selM.onNewClickSelection(new NewSelEventArgs(newSel));
            }

        }


        isPressed = false;
        longPress = false;
        hoveredAtom = null;
        if (curAllAtoms != null)
            curAllAtoms.Clear();
        selSphere.SetActive(false);
    }

    void Update() {
        if (isPressed) {
            if (Time.realtimeSinceStartup - timePressed > longPressLimit) {
                updateLongPressSelection();
            }
        }
    }

    void updateLongPressSelection() {
        if (isPressed && hoveredAtom != null) {

            selSphere.SetActive(true);
            longPress = true;

            RigidPose cpose = VivePose.GetPose(curRole);


            //Compute new pointer position based on previous distance from selected atom to controller
            Vector3 newP = cpose.pos + cpose.forward * initDistPointerToAtom;
            selSphere.transform.position = hoveredAtom.curWorldPosition;

            curSelRad = Vector3.Distance(newP, hoveredAtom.curWorldPosition);
            selSphere.transform.localScale = Vector3.one * curSelRad * 2;

        }
    }

}
}