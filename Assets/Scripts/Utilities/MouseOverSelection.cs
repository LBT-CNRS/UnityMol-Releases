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
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

namespace UMol {

public class MouseOverSelection : MonoBehaviour {

    TextMesh textm;
    GameObject haloGo;
    public Camera mainCam;
    UnityMolSelectionManager selM;

    public bool tempDisable = false;
    public bool showSphereHovering = true;

    float lastClickTime = 0.0f;
    float lastRightClickTime = 0.0f;
    float catchTime = 0.25f;//Double click
    public bool clicked = false;

    private Vector3 worldPos = Vector3.zero;
    public int framesToWait = 5;
    // int frameCount = 0;


    public bool duringIMD = false;
    UnityMolSelection curSel = null;
    Vector3 initPos;
    Vector3 initPosA;
    Vector3 initVec;
    Transform atomT;
    GameObject arrow;

    float hoverScaleMultiplier = 1.0f;


    void Start() {
        selM = UnityMolMain.getSelectionManager();
        createHaloGo();

        mainCam = Camera.main;

        if (arrow == null) {
            arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arrow.name = "Arrow";
            arrow.GetComponent<MeshRenderer>().enabled = false;
            Destroy(arrow.GetComponent<BoxCollider>());
        }

    }

    void Update() {
        if (mainCam == null) {
            mainCam = Camera.main;
        }

        if (arrow == null) {
            arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arrow.name = "Arrow";
            arrow.GetComponent<MeshRenderer>().enabled = false;
            Destroy(arrow.GetComponent<BoxCollider>());
        }
        if (haloGo == null) {
            createHaloGo();
        }

        // //Show hover after the mouse stopped on the atom for some time
        // if (frameCount >= framesToWait) {
        //     Vector3 curPos = Input.mousePosition;
        //     //Mouse stayed at the same place
        //     if (Mathf.Abs(worldPos.x - curPos.x) < 2.0f && Mathf.Abs(worldPos.y - curPos.y) < 2.0f) {
        //         if (showSphereHovering && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
        //             doHovering();
        //         }
        //     }
        //     worldPos = curPos;
        //     frameCount = 0;
        // }
        // frameCount++;



        if (Input.GetMouseButtonUp(0)) {
            arrow.GetComponent<MeshRenderer>().enabled = false;
            if (duringIMD) {
                UnityMolStructure s = curSel.structures[0];
                //Get artemis manager
                ArtemisManager am = s.artemisM;
                if (am == null) {
                    duringIMD = false;
                    return;
                }
                am.clearForces();
            }
            duringIMD = false;
        }


        if (!tempDisable && Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() ) { //Mouse clicked
            doIMDPull();
            clearHovering();
            clicked = false;
            if (Time.time - lastClickTime < catchTime) {//Double click => center on selected structure
                duringIMD = false;
                UnityMolAtom a = getAtomPointed();
                if (a != null) {
                    UnityMolStructure s = a.residue.chain.model.structure;
                    API.APIPython.centerOnStructure(s.uniqueName, true);
                }
            }
            else { //Delay the simple click to avoid doing the selection
                clicked = true;
            }

            lastClickTime = Time.time;
        }
        if (Input.GetMouseButtonUp(0)) {
            float diffTime = Time.time - lastClickTime;
            if (clicked && diffTime > 0.2f) { //Long clicked
                clicked = false;
            }
        }

        if (Input.GetMouseButtonDown(1) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() ) { //Mouse right clicked
            if (Time.time - lastRightClickTime < catchTime) {//Double click
                doHovering();
            }
            else { //Delay the simple click to avoid doing the selection
                clearHovering();
            }

            lastRightClickTime = Time.time;
        }

        if (!duringIMD && clicked && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() && Time.time - lastClickTime > catchTime) {
            if (!Input.GetMouseButton(0)) {
                doSelection();
            }
            clicked = false;
        }

        //Add force to the IMD simulation + show cube visual feedback
        if (duringIMD && curSel != null && curSel.Count != 0) {

            UnityMolStructure s = curSel.structures[0];
            UnityMolStructureManager sm = UnityMolMain.getStructureManager();
            Transform molParent = sm.structureToGameObject[s.uniqueName].transform;

            Vector3 curPosA = molParent.InverseTransformPoint(atomT.position);

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Vector3.Distance(mainCam.transform.position, atomT.position);
            Vector3 mousePosW = mainCam.ScreenToWorldPoint(mousePos);
            Vector3 curPos = molParent.InverseTransformPoint(mousePosW);


            Vector3 between = (curPos - curPosA);
            Vector3 forceL = between;

            //Get artemis manager
            ArtemisManager am = s.artemisM;
            if (am == null) {
                return;
            }

            am.addForce(curSel.atoms[0].idInAllAtoms, new float[] {forceL.x, forceL.y, forceL.z});

            //Visual feedback
            arrow.GetComponent<MeshRenderer>().enabled = true;

            float dist = (mousePosW - atomT.position).magnitude;
            arrow.transform.localScale = new Vector3(dist / 12.0f, dist / 12.0f, dist);
            arrow.transform.position = atomT.position + (mousePosW - atomT.position) / 2.0f;
            arrow.transform.LookAt(mousePosW);

        }

    }

    private void createHaloGo() {
        haloGo = GameObject.Instantiate((GameObject) Resources.Load("Prefabs/SphereOverAtom"));
        textm = haloGo.GetComponentsInChildren<TextMesh>()[0];
        // haloGo.layer = LayerMask.NameToLayer("Ignore Raycast");
        haloGo.SetActive(false);
        textm.gameObject.GetComponent<MeshRenderer>().sortingOrder = 50;
    }

    public UnityMolAtom getAtomPointed() {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition); // Create the ray from screen to infinite

        CustomRaycastBurst raycaster = UnityMolMain.getCustomRaycast();
        UnityMolAtom a = raycaster.customRaycastAtomBurst(ray.origin, ray.direction);

        return a;
    }

    private void doHovering() {

        UnityMolAtom a = getAtomPointed();

        if (a != null) {

            if (haloGo == null) { //Somehow got destroyed
                createHaloGo();
            }
            //Format the text of the atom
            textm.text = PointerHoverAtom.formatAtomText(a);
            haloGo.SetActive(true);

            UnityMolStructureManager sm = UnityMolMain.getStructureManager();
            UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
            UnityMolStructure s = a.residue.chain.model.structure;
            Transform molPar = sm.GetStructureGameObject(s.uniqueName).transform;

            haloGo.transform.position = molPar.TransformPoint(a.position);

            haloGo.transform.rotation = Quaternion.LookRotation(haloGo.transform.position - mainCam.transform.position);
            GameObject goAtom = s.atomToGo[a];
            haloGo.transform.SetParent(goAtom.transform);

            haloGo.transform.localScale =  hoverScaleMultiplier * a.radius * Vector3.one * 1.1f;

        }
        else {
            clearHovering();
        }
    }

    void clearHovering() {
        if (haloGo != null) {
            haloGo.SetActive(false);
            haloGo.transform.parent = null;
        }
    }

    private void doIMDPull() {
        UnityMolAtom hoveredAtom = getAtomPointed();

        if (hoveredAtom != null) {

            UnityMolSelection hoveredSelection = null;

            if (hoveredAtom.residue.chain.model.structure.artemisM != null) {
                //IMD running for this structure
                UnityMolStructure s = hoveredAtom.residue.chain.model.structure;
                hoveredSelection = hoveredAtom.ToSelection();

                UnityMolStructureManager sm = UnityMolMain.getStructureManager();
                Transform molParent = sm.structureToGameObject[s.uniqueName].transform;
                atomT = s.atomToGo[hoveredAtom].transform;

                curSel = hoveredSelection;

                initPos = molParent.InverseTransformPoint(atomT.position);
                initPosA = molParent.InverseTransformPoint(atomT.position);
                initVec = initPos - initPosA;

                duringIMD = true;
                return;
            }
        }
    }

    private void doSelection() {

        UnityMolAtom hoveredAtom = getAtomPointed();

        if (hoveredAtom != null) {

            UnityMolSelection hoveredSelection = null;


            // //Do that to force creating a new selection when clicking
            // if (selM.currentSelection != null && selM.currentSelection.name.StartsWith("all(")) {
            //     API.APIPython.clearSelections();
            // }


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

            if (selM.onNewClickSelection != null) {
                selM.onNewClickSelection(new NewSelEventArgs(hoveredSelection));
            }

            int countBeforeAdding = 0;

            if (selM.currentSelection == null || !selM.currentSelection.isAlterable) {
                selM.getClickSelection();
            }

            UnityMolSelection cSel = selM.currentSelection;

            countBeforeAdding = cSel.Count;

            UnityMolSelection newSel = API.APIPython.select(hoveredSelection.MDASelString, cSel.name, true,
                                       addToExisting: true, silent: true);

            int afterAdding = newSel.Count;

            if (countBeforeAdding == afterAdding) {
                API.APIPython.removeFromSelection(hoveredSelection.MDASelString, cSel.name, silent: true);
            }
            if (cSel.Count == 1) {
                Debug.Log(cSel + " : " + cSel.atoms[0]);
            }
            else {
                Debug.Log(cSel);
            }
            // API.APIPython.updateRepresentations(cSel.name);


        }
        else {
            selM.ClearCurrentSelection();
        }
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