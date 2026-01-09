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
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

namespace UMol {

public class MouseOverSelection : MonoBehaviour {

    TextMesh textm;
    GameObject haloGo;
    GameObject trajExtraGo;
    public Camera mainCam;
    UnityMolSelectionManager selM;
    public bool disableMouseInVR = true;

    public bool imdUseSelectionMode = false;

    public bool tempDisable = false;
    public bool showSphereHovering = true;

    float lastClickTime = 0.0f;
    float lastRightClickTime = 0.0f;
    float catchTime = 0.25f;//Double click
    public bool clicked = false;

    private Vector3 worldPos = Vector3.zero;
    public int framesToWait = 10;
    // int frameCount = 0;


    public bool duringIMD = false;
    UnityMolSelection curSel = null;
    // Vector3 imdInitPos;
    // Vector3 imdInitPosA;
    // Vector3 imdInitVec;
    GameObject hoverAtomGo;
    Transform atomT;
    UnityMolAtom imdAtom;
    GameObject arrow;
    List<GameObject> imdArrowList = new List<GameObject>();

    float hoverScaleMultiplier = 1.0f;


    void Start() {
        selM = UnityMolMain.getSelectionManager();
        createHaloGo();
        trajExtraGo = new GameObject("DummyTrajExtractedGo");
        hoverAtomGo = new GameObject("HoverAtomGo");
        DontDestroyOnLoad(trajExtraGo);
        DontDestroyOnLoad(hoverAtomGo);


        mainCam = Camera.main;

        if (arrow == null) {
            arrow = Instantiate(Resources.Load("Prefabs/SpringPrefab") as GameObject);
            arrow.name = "Arrow";
            arrow.GetComponentInChildren<MeshRenderer>().enabled = false;
            DontDestroyOnLoad(arrow);
        }
    }

    void Update() {
        if (mainCam == null) {
            mainCam = Camera.main;
        }
        if (disableMouseInVR && UnityMolMain.inVR()) {
            return;
        }

        if (arrow == null) {
            arrow = Instantiate(Resources.Load("Prefabs/SpringPrefab") as GameObject);//GameObject.CreatePrimitive(PrimitiveType.Cube);
            arrow.name = "Arrow";
            arrow.GetComponentInChildren<MeshRenderer>().enabled = false;
            DontDestroyOnLoad(arrow);
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
            arrow.GetComponentInChildren<MeshRenderer>().enabled = false;

            foreach (GameObject ar in imdArrowList) {
                ar.SetActive(false);
            }

            if (duringIMD) {
                UnityMolStructure s = curSel.structures[0];

                MDDriverManager mdm = s.mddriverM;
                if (mdm == null) {
                    duringIMD = false;
                    return;
                }
            }
            duringIMD = false;
        }


        if (!tempDisable && Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() ) { //Mouse clicked
            doIMDPull();
            clearHovering();
            clicked = false;
            if (Time.time - lastClickTime < catchTime) {//Double click => center on selected structure
                duringIMD = false;
                Vector3 p = Vector3.zero;
                bool isExtrAtom = false;
                UnityMolAtom a = getAtomPointed(true, ref p, ref isExtrAtom);
                if (a != null) {
                    ManipulationManager mm = API.APIPython.getManipulationManager();
                    if (mm != null) {
                        mm.centerOnSelection(a.residue.ToSelection(), true, -1, 0.25f);
                    }

                    // UnityMolStructure s = a.residue.chain.model.structure;
                    // API.APIPython.centerOnStructure(s.name, true);
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
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() ) { //Mouse left clicked + alt
            doHovering();
            clicked = false;
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
            Transform molParent = sm.structureToGameObject[s.name].transform;

            Vector3 curPosA = molParent.InverseTransformPoint(atomT.position);

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Mathf.Abs(mainCam.transform.position.z - atomT.position.z);
            Vector3 mousePosW = mainCam.ScreenToWorldPoint(mousePos);
            Vector3 curPos = molParent.InverseTransformPoint(mousePosW);

            Vector3 between = (curPos - curPosA);
            Vector3 forceL = between;

            MDDriverManager mdm = s.mddriverM;
            if (mdm == null) {
                duringIMD = false;
                return;
            }

            Vector3 initAtomToMouse = mousePosW - atomT.position;

            //Add same force to all atoms in the selection
            foreach (UnityMolAtom a in curSel.atoms) {
                mdm.addForce(a.idInAllAtoms, new float[] {forceL.x, forceL.y, forceL.z});
            }
            showAddedForces(initAtomToMouse);

        }
    }

    void showAddedForces(Vector3 iniAtomToMouse) {
        //Pool objects, can be improved with a real pool
        if (imdArrowList.Count < curSel.Count) {
            int start = imdArrowList.Count;
            int end = curSel.Count;
            for (int i = start; i < end; i++) {
                GameObject newArr = Instantiate(arrow);
                newArr.GetComponentInChildren<MeshRenderer>().enabled = true;
                imdArrowList.Add(newArr);
            }
        }
        foreach (GameObject ar in imdArrowList) {
            ar.SetActive(false);
        }

        float magn = iniAtomToMouse.magnitude;
        UnityMolStructure s = imdAtom.residue.chain.model.structure;
        int id = 0;
        foreach (UnityMolAtom a in curSel.atoms) {
            Transform aT = UnityMolMain.getAnnotationManager().getGO(a).transform;
            imdArrowList[id].SetActive(true);

            imdArrowList[id].transform.localScale = new Vector3(magn / 2.0f, magn / 2.0f, magn);
            imdArrowList[id].transform.position = aT.position + iniAtomToMouse / 2.0f;
            imdArrowList[id].transform.LookAt(aT.position + iniAtomToMouse);

            id++;
        }

    }



    private void createHaloGo() {
        haloGo = GameObject.Instantiate((GameObject) Resources.Load("Prefabs/SphereOverAtom"));
        textm = haloGo.GetComponentsInChildren<TextMesh>()[0];
        // haloGo.layer = LayerMask.NameToLayer("Ignore Raycast");
        haloGo.SetActive(false);
        textm.gameObject.GetComponent<MeshRenderer>().sortingOrder = 50;
        DontDestroyOnLoad(haloGo);
    }

    public UnityMolAtom getAtomPointed(bool useExtractedTraj, ref Vector3 outWPos, ref bool isExtrAtom) {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition); // Create the ray from screen to infinite

        CustomRaycastBurst raycaster = UnityMolMain.getCustomRaycast();
        UnityMolAtom a = raycaster.customRaycastAtomBurst(ray.origin, ray.direction, ref outWPos, ref isExtrAtom, useExtractedTraj);

        return a;
    }

    private void doHovering() {

        Vector3 p = Vector3.zero;
        bool isExtrAtom = false;
        UnityMolAtom a = getAtomPointed(true, ref p, ref isExtrAtom);

        if (a != null) {

            if (haloGo == null) { //Somehow got destroyed
                createHaloGo();
            }
            //Format the text of the atom
            textm.text = PointerHoverAtom.formatAtomText(a);
            Debug.Log(a + ": " + a.position.x.ToString("f2") + " | " + a.position.y.ToString("f2") + " | " + a.position.z.ToString("f2"));
            haloGo.SetActive(true);

            // UnityMolStructureManager sm = UnityMolMain.getStructureManager();
            // UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
            UnityMolStructure s = a.residue.chain.model.structure;
            // Transform molPar = sm.GetStructureGameObject(s.name).transform;

            // haloGo.transform.position = molPar.TransformPoint(a.position);
            haloGo.transform.position = p;
            trajExtraGo.transform.position = p;

            haloGo.transform.rotation = Quaternion.LookRotation(haloGo.transform.position - mainCam.transform.position);

            UnityMolMain.getAnnotationManager().setGOPos(a, hoverAtomGo);

            if (!isExtrAtom)
                haloGo.transform.SetParent(hoverAtomGo.transform);
            else {
                trajExtraGo.transform.SetParent(hoverAtomGo.transform);
                trajExtraGo.transform.localScale = hoverAtomGo.transform.localScale;
                haloGo.transform.SetParent(trajExtraGo.transform);

            }

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
        Vector3 p = Vector3.zero;
        bool isExtrAtom = false;
        UnityMolAtom hoveredAtom = getAtomPointed(false, ref p, ref isExtrAtom);

        if (hoveredAtom != null) {

            UnityMolSelection hoveredSelection = null;

            if (hoveredAtom.residue.chain.model.structure.mddriverM != null) {
                //IMD running for this structure
                UnityMolStructure s = hoveredAtom.residue.chain.model.structure;
                if (imdUseSelectionMode) {
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
                }
                else {
                    hoveredSelection = hoveredAtom.ToSelection();
                }

                atomT = UnityMolMain.getAnnotationManager().getGO(hoveredAtom).transform;

                imdAtom = hoveredAtom;

                curSel = hoveredSelection;

                // imdInitPos = molParent.InverseTransformPoint(atomT.position);
                // imdInitPosA = molParent.InverseTransformPoint(atomT.position);
                // imdInitVec = imdInitPos - imdInitPosA;

                duringIMD = true;
                return;
            }
        }
    }

    private void doSelection() {

        Vector3 p = Vector3.zero;
        bool isExtrAtom = false;
        UnityMolAtom hoveredAtom = getAtomPointed(false, ref p, ref isExtrAtom);

        if (hoveredAtom != null) {

            UnityMolSelection hoveredSelection = null;


            // //Do that to force creating a new selection when clicking
            // if (selM.currentSelection != null && selM.currentSelection.name.StartsWith("all_")) {
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

            if (!Input.GetKey(KeyCode.LeftControl)) { //Left control only adds to selection
                if (countBeforeAdding == afterAdding) {
                    API.APIPython.removeFromSelection(hoveredSelection.MDASelString, cSel.name, silent: true);
                }
            }

            Debug.Log(selM.currentSelection);

            API.APIPython.updateRepresentations(selM.currentSelection.name);


        }
        else {
            if (!Input.GetKey(KeyCode.LeftControl))
                API.APIPython.clearSelections();
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