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
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using System.Linq;

namespace UMol {

public class ManipulationManager : MonoBehaviour {
    public delegate void TourModified();
    public static event TourModified OnTourModified;

    public Transform currentTransform;

    public int currentTourSel = 0;
    public List<string> tourSelections = new List<string>();

    public float moveSpeed = 1.0f;
    public float scrollSpeed = 1.0f;

    public float speedX = 1.0f;
    public float speedY = 1.0f;
    public float speedZ = 1.0f;

    public bool rotateX = false;
    public bool rotateY = false;
    public bool rotateZ = false;

    public bool isRotating {
        get {
            return rotateX | rotateY | rotateZ;
        }
    }

    public Vector3 currentCenterPosition = Vector3.zero;

    public bool disableMouseInVR = true;
    public bool ActivateMouse = true;

    private Transform loadedMolPar;
    private MouseOverSelection mouseSel;
    private Camera mainCam;
    private Transform translatingT;
    private Transform rotatingT;
    private Vector3 clickedRotationCenter;
    private Coroutine moveCoroutine;
    private Coroutine tourCoroutine;

    private bool mouseClickedOnUI = false;
    bool curCenter = false;

    public string followSelection = "";



    void Start() {
        mouseSel = GetComponent<MouseOverSelection>();
        // default mouse movement on Mac is very slow, make it faster if we detect we run on a Mac
        #if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        moveSpeed = 6.0f * moveSpeed;
        #endif
    }

    void OnEnable() {
        if (mainCam == null)
            updateMainCamRef();
        UnityMolSelectionManager.OnSelectionDeleted += checkTourSelections;
        UnityMolSelectionManager.OnSelectionModified += checkTourSelections;
    }

    void OnDisable() {
        UnityMolSelectionManager.OnSelectionDeleted -= checkTourSelections;
        UnityMolSelectionManager.OnSelectionModified -= checkTourSelections;
    }

    public void updateMainCamRef() {
        mainCam = Camera.main;
    }

    public void resetPosition() {
        stopCurrentMovements();
        if (currentTransform != null)
            currentTransform.position = Vector3.zero;
        UnityMolMain.getCustomRaycast().needsUpdatePos = true;
    }
    public void resetRotation() {
        stopCurrentMovements();
        if (currentTransform != null)
            currentTransform.rotation = Quaternion.identity;
        UnityMolMain.getCustomRaycast().needsUpdatePos = true;
    }
    public void setSelectedOject(Transform seleT) {
        currentTransform = seleT;
    }

    public void centerOnStructure(UnityMolStructure s, bool lerp) {
        UnityMolSelection sel = s.ToSelection();
        centerOnSelection(sel, lerp);
    }

    public void setRotationCenter(Vector3 pos) {
        currentCenterPosition = pos;
    }

    public void stopCurrentMovements() {
        if (moveCoroutine != null && !curCenter) {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }
    }

    ///Fit the selection in the camera field of view if distance is negative, otherwise the molecule will be placed at "distance" from the camera
    //https://i.imgur.com/xz0erbV.png from https://forum.unity.com/threads/fit-object-exactly-into-perspective-cameras-field-of-view-focus-the-object.496472/
    //Compute the "radius" of the sphere encompassing the selection to make it match with the camera FOV
    public void centerOnSelection(UnityMolSelection sel, bool lerp, float distance = -1.0f, float duration = 0.25f) {
        stopCurrentMovements();

        if (lerp && duration > 0.0f) {
            centerOnSelectionLerp(sel, distance, duration);
        }
        else {
            Transform tpar = UnityMolMain.getRepresentationParent().transform;
            Transform molPar = getSelectionParent(tpar, sel);

            float h = distance;
            if (distance <= 0.0) {//If the distance was not specified

                Vector3 wMinP = molPar.TransformPoint(sel.minPos);
                Vector3 wMaxP = molPar.TransformPoint(sel.maxPos);

                float r = Vector3.Distance(wMinP, wMaxP) / 2.0f;//Compute world space sphere radius encompassing the selection
                if (UnityMolMain.inVR()) {//Distance should be at least 0.3 in VR otherwise the molecule is too close
                    r = Mathf.Max(0.3f, r);
                }
                h = r / Mathf.Sin(Mathf.Deg2Rad * mainCam.fieldOfView / 2.0f);//Compute the distance between the camera and the center of this sphere
            }

            Vector3 bary = computeCentroidSel(sel);
            Vector3 worldBary = molPar.TransformPoint(bary);
            tpar.Translate(-worldBary, Space.World);//Move the parent to 0,0,0

            float dist = Vector3.Distance(mainCam.transform.position, molPar.TransformPoint(bary));//Get the new distance between the selection barycenter and the camera

            if (!UnityMolMain.inVR()) {
                tpar.Translate(new Vector3(0.0f, 0.0f, -dist + h), Space.World);
            }
            else {
                Transform head = mainCam.transform;
                if (head != null) {
                    Vector3 headTarget = head.position + (head.forward * h);
                    tpar.Translate(headTarget, Space.World);
                }
            }

            setRotationCenter(molPar.TransformPoint(bary));
            UnityMolMain.getCustomRaycast().needsUpdatePos = true;
        }
    }

    private void centerOnSelectionLerp(UnityMolSelection sel, float distance, float duration) {
        Transform tpar = UnityMolMain.getRepresentationParent().transform;
        Vector3 bary = computeCentroidSel(sel);
        Transform molPar = getSelectionParent(tpar, sel);
        curCenter = true;//Set this to true this frame to avoid interupting the movement

        moveCoroutine = StartCoroutine(delayedCenterOnSelection(sel, tpar, bary, molPar, distance, duration));
    }

    IEnumerator delayedCenterOnSelection(UnityMolSelection sel, Transform tpar, Vector3 bary, Transform molPar, float distance, float duration) {
        //End of frame
        yield return 0;

        if (molPar == null) {
            molPar = getSelectionParent(tpar, sel);
        }
        if (molPar == null)//Something went really wrong !
            yield break;
        Vector3 worldBary = molPar.TransformPoint(bary);

        Vector3 savedPos = tpar.position;
        tpar.Translate(-worldBary, Space.World);

        float dist = Vector3.Distance(mainCam.transform.position, molPar.TransformPoint(bary));

        float h = distance;
        if (distance <= 0.0f) {//If the distance was not specified

            Vector3 wMinP = molPar.TransformPoint(sel.minPos);
            Vector3 wMaxP = molPar.TransformPoint(sel.maxPos);

            float r = Vector3.Distance(wMinP, wMaxP) / 2.0f;//Compute world space sphere radius encompassing the selection
            if (UnityMolMain.inVR()) {//Distance should be at least 0.3 in VR otherwise the molecule is too close
                r = Mathf.Max(0.3f, r);
            }
            h = r / Mathf.Sin(Mathf.Deg2Rad * mainCam.fieldOfView / 2.0f);//Compute the distance between the camera and the center of this sphere
        }

        if (!UnityMolMain.inVR()) {
            tpar.Translate(new Vector3(0.0f, 0.0f, -dist + h), Space.World);
        }
        else {
            Transform head = mainCam.transform;
            if (head != null) {
                Vector3 headTarget = head.position + (head.forward * h);
                tpar.Translate(headTarget, Space.World);
            }
        }

        Vector3 targetPos = tpar.position;
        tpar.position = savedPos;

        float timeNow = Time.realtimeSinceStartup;

        float inversed = 1.0f / duration;
        int i = 0;
        for (float step = 0.0f; step < 1.0f; step += Time.deltaTime * inversed) {
            tpar.position = Vector3.Lerp(savedPos, targetPos, step);
            //Set the rotation center before => avoid problems when stopping coroutine
            setRotationCenter(molPar.TransformPoint(bary));

            curCenter = false;
            if (Time.realtimeSinceStartup - timeNow < 0.1f)//less than 100 ms that we started center on selection = don't interupt it
                curCenter = true;//Set this to true this frame to avoid interupting the movement
            i++;
            UnityMolMain.getCustomRaycast().needsUpdatePos = true;
            yield return 0;
        }

        UnityMolMain.getCustomRaycast().needsUpdatePos = true;
    }

    public void emulateLookAtPOV(UnityMolSelection sel, Vector3 pov, Vector3 targetPos, bool lerp = false) {
        stopCurrentMovements();
        Transform tpar = UnityMolMain.getRepresentationParent().transform;
        Transform molPar = getSelectionParent(tpar, sel);
        Vector3 localTarget = molPar.InverseTransformPoint(targetPos);
        Vector3 localPov = molPar.InverseTransformPoint(pov);

        GameObject go1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go1.transform.position = targetPos;
        go1.transform.localScale = Vector3.one * 0.1f;
        go1.transform.parent = molPar;

        GameObject go2 = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go2.transform.position = pov;
        go2.transform.localScale = Vector3.one * 0.1f;
        go2.transform.parent = molPar;


        if (!UnityMolMain.inVR()) {
            tpar.Translate(-targetPos, Space.World);
            float dist = Vector3.Distance(transform.position, molPar.TransformPoint(localTarget));
            float distPovTarget = Vector3.Distance(pov, targetPos);
            tpar.Translate(new Vector3(0.0f, 0.0f, - dist + distPovTarget), Space.World);

            Vector3 fromVec = transform.position - molPar.TransformPoint(localTarget);
            Vector3 toVec = molPar.TransformPoint(localPov) - molPar.TransformPoint(localTarget);
            Quaternion fromTo = Quaternion.FromToRotation(fromVec, toVec);
            // tpar.rotation = tpar.rotation * fromTo;
            float angle = 0.0f;
            Vector3 axis = Vector3.zero;
            fromTo.ToAngleAxis(out angle, out axis);

            tpar.RotateAround(molPar.TransformPoint(localTarget), axis, angle);

        }

        setRotationCenter(molPar.TransformPoint(localTarget));
        UnityMolMain.getCustomRaycast().needsUpdatePos = true;
    }


    public static Vector3 computeCentroidSel(UnityMolSelection sel) {
        Vector3 pos = Vector3.zero;
        foreach (UnityMolAtom a in sel.atoms) {
            pos += a.position;
        }
        pos /= sel.atoms.Count;
        return pos;
    }

    Transform getSelectionParent(Transform allParent, UnityMolSelection sel) {
        if (sel.structures.Count == 1) {
            return allParent.Find(sel.structures[0].ToSelectionName());
        }
        else if (sel.structures.Count > 1) {
            return allParent;
        }
        return null;
    }

    public UnityMolAtom getAtomPointed() {
        if (mainCam == null)
            updateMainCamRef();
        if (mainCam == null)
            return null;
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition); // Create the ray from screen to infinite
        Vector3 dummy = Vector3.zero;
        bool dummy2 = false;
        CustomRaycastBurst raycaster = UnityMolMain.getCustomRaycast();
        UnityMolAtom a = raycaster.customRaycastAtomBurst(ray.origin, ray.direction, ref dummy, ref dummy2, true);

        return a;
    }

    public void RotateAroundCentroid(float angle, int axis) {

        Vector3 vecAxis = loadedMolPar.right;
        if (axis == 1) //Y
            vecAxis = loadedMolPar.up;
        else if (axis == 2) //Z
            vecAxis = loadedMolPar.forward;

        foreach (UnityMolStructure s in UnityMolMain.getStructureManager().loadedStructures) {
            Transform t = UnityMolMain.getStructureManager().structureToGameObject[s.name].transform;
            t.RotateAround(t.TransformPoint(s.currentModel.centroid), vecAxis, angle);
        }
        UnityMolMain.getCustomRaycast().needsUpdatePos = true;
    }

    public void ZoomInOut(float dist) {
        if (mainCam == null)
            updateMainCamRef();
        if (mainCam == null)
            return;

        currentTransform.position += (mainCam.transform.forward * dist);
        currentCenterPosition += (mainCam.transform.forward * dist);
        UnityMolMain.getCustomRaycast().needsUpdatePos = true;
    }

    #region Tour
    public void checkTourSelections() {
        if (tourSelections.Count != 0) {
            if (tourCoroutine != null)
                StopCoroutine(tourCoroutine);
            var selM = UnityMolMain.getSelectionManager();
            List<string> newTourSel = new List<string>();
            foreach (string s in tourSelections) {
                if (selM.selections.ContainsKey(s)) {
                    newTourSel.Add(s);
                }
            }
            if (newTourSel.Count != tourSelections.Count) {
                tourSelections = newTourSel;
                if (OnTourModified != null)
                    OnTourModified();
            }
        }
    }

    public void tourPrevious(float duration = 0.75f) {
        currentTourSel--;
        if (currentTourSel >= tourSelections.Count || currentTourSel < 0) {
            currentTourSel = tourSelections.Count - 1;
        }
        var selM = UnityMolMain.getSelectionManager();
        if (tourSelections.Count != 0 && selM.selections.ContainsKey(tourSelections[currentTourSel])) {
            centerOnSelection(selM.selections[tourSelections[currentTourSel]], true, -1, duration);
        }
    }

    public void tourNext(float duration = 0.75f) {
        currentTourSel++;
        if (currentTourSel >= tourSelections.Count) {
            currentTourSel = 0;
        }
        var selM = UnityMolMain.getSelectionManager();
        if (tourSelections.Count != 0 && selM.selections.ContainsKey(tourSelections[currentTourSel])) {
            centerOnSelection(selM.selections[tourSelections[currentTourSel]], true, -1, duration);
        }
    }

    public void addTour(UnityMolSelection sel) {
        tourSelections.Add(sel.name);
        if (OnTourModified != null)
            OnTourModified();
    }
    public void clearTour() {
        if (tourCoroutine != null)
            StopCoroutine(tourCoroutine);
        tourSelections.Clear();
        resetTour();
        if (OnTourModified != null)
            OnTourModified();
    }
    ///Remove the last occurence of the selection in tourSelections
    public void removeFromTour(UnityMolSelection sel) {
        for (int i = tourSelections.Count; i >= 0; i--) {
            if (tourSelections[i] == sel.name) {
                tourSelections.RemoveAt(i);
                resetTour();
                if (OnTourModified != null)
                    OnTourModified();
                break;
            }
        }
        if (tourCoroutine != null)
            StopCoroutine(tourCoroutine);
    }
    public void removeFromTour(int id) {
        if (id >= 0 && id < tourSelections.Count) {
            tourSelections.RemoveAt(id);
            if (OnTourModified != null)
                OnTourModified();
            if (tourCoroutine != null)
                StopCoroutine(tourCoroutine);
        }
    }
    public void resetTour() {
        currentTourSel = 0;
        if (tourCoroutine != null)
            StopCoroutine(tourCoroutine);
    }
    public void startTour(float stopTime = 2.0f, float durationPerSel = 0.75f) {
        tourCoroutine = StartCoroutine(doTour(stopTime, durationPerSel));
    }

    IEnumerator doTour(float stopTime, float durationTransi) {
        if (tourSelections.Count == 0)
            yield break;
        if (currentTourSel == tourSelections.Count - 1)
            resetTour();
        var selM = UnityMolMain.getSelectionManager();
        for (int i = currentTourSel; i < tourSelections.Count; i++) {
            if (selM.selections.ContainsKey(tourSelections[i])) {
                yield return new WaitForSeconds(stopTime);
                centerOnSelection(selM.selections[tourSelections[i]], true, -1, durationTransi);
                currentTourSel = i;
                yield return new WaitForSeconds(durationTransi);
            }
        }
    }
    #endregion

    #region depthcueing

    float depthcueInitP;
    float depthcueInitV;
    public bool depthcueUpdate = true;

    public void initFollowDepthCueing() {
        if (loadedMolPar == null)
            loadedMolPar = UnityMolMain.getRepresentationParent().transform;

        depthcueInitP = loadedMolPar.position.z;
        depthcueInitV = UnityMolMain.fogStart;
    }

    #endregion

    void Update ()
    {

        if (mainCam == null)
            updateMainCamRef();

        bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (loadedMolPar == null) {
            loadedMolPar = UnityMolMain.getRepresentationParent().transform;
        }
        if (EventSystem.current == null) {
            gameObject.AddComponent<EventSystem>();
        }

        GameObject curUIInput = EventSystem.current.currentSelectedGameObject;

        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
            mouseClickedOnUI = true;
        }
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonDown(2))
            mouseClickedOnUI = false;

        //Depth cueing update
        if (depthcueUpdate && UnityMolMain.isFogOn) {
            float delta = loadedMolPar.position.z - depthcueInitP;
            float dc = depthcueInitV + (delta / UnityMolMain.fogDensity);
            if (dc != UnityMolMain.fogStart) {
                UMol.API.APIPython.setDepthCueingStart(dc);
            }
        }

        if (rotateX) {
            RotateAroundCentroid(speedX, 0);
        }
        if (rotateY) {
            RotateAroundCentroid(speedY, 1);
        }
        if (rotateZ) {
            RotateAroundCentroid(speedZ, 2);
        }
        if (curUIInput == null && Input.GetButton("RotationXLeft")) {
            RotateAroundCentroid(speedX, 0);
        }
        if (curUIInput == null && Input.GetButton("RotationXRight")) {
            RotateAroundCentroid(-speedX, 0);
        }
        if (curUIInput == null && Input.GetButton("RotationYLeft")) {
            RotateAroundCentroid(speedY, 1);
        }
        if (curUIInput == null && Input.GetButton("RotationYRight")) {
            RotateAroundCentroid(-speedY, 1);
        }
        if (curUIInput == null && Input.GetButton("RotationZLeft")) {
            RotateAroundCentroid(speedZ, 2);
        }
        if (curUIInput == null && Input.GetButton("RotationZRight")) {
            RotateAroundCentroid(-speedZ, 2);
        }

        if (disableMouseInVR && UnityMolMain.inVR()) {
            return;
        }

        if (!ActivateMouse) {
            return;
        }
        if (currentTransform == null)
            setSelectedOject(UnityMolMain.getRepresentationParent().transform);

        if (currentTransform == null)
            return;

        if (curUIInput == null && Input.GetButton("ZoomIn")) {
            stopCurrentMovements();
            ZoomInOut(moveSpeed * 0.1f);
        }
        if (curUIInput == null && Input.GetButton("ZoomOut")) {
            stopCurrentMovements();
            ZoomInOut(-moveSpeed * 0.1f);
        }

        var selM = UnityMolMain.getSelectionManager();
        if (!UnityMolMain.inVR() && !string.IsNullOrEmpty(followSelection) && selM.selections.ContainsKey(followSelection)) {
            centerOnSelection(selM.selections[followSelection], false);
        }


        //Translating only one molecule
        if (translatingT != null) {
            if (Input.GetMouseButtonUp(0) || Input.GetButtonUp("TranslateClicked")) {
                translatingT = null;
            }
            else {
                stopCurrentMovements();
                translatingT.Translate(transform.up * Input.GetAxis("Mouse Y")*moveSpeed * 0.05f, Space.World);
                translatingT.Translate(transform.right * Input.GetAxis("Mouse X")*moveSpeed * 0.05f, Space.World);
                //update currentCenterPosition ?

                UnityMolMain.getCustomRaycast().needsUpdatePos = true;
                return;
            }
        }
        //Clicked with translate
        else if (curUIInput == null && Input.GetButton("TranslateClicked") &&
                 Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
            UnityMolAtom a = getAtomPointed();
            if (a != null) {
                GameObject sgo = UnityMolMain.getStructureManager().GetStructureGameObject(a.residue.chain.model.structure.name);
                translatingT = sgo.transform;
            }
        }

        //Rotating only one molecule
        if (rotatingT != null) {
            if (Input.GetMouseButtonUp(0) || Input.GetButtonUp("RotateClicked")) {
                rotatingT = null;
            }
            else {
                stopCurrentMovements();
                rotatingT.RotateAround(clickedRotationCenter, transform.up, -Input.GetAxis("Mouse X")*moveSpeed);
                rotatingT.RotateAround(clickedRotationCenter, transform.right, Input.GetAxis("Mouse Y")*moveSpeed);
                UnityMolMain.getCustomRaycast().needsUpdatePos = true;
                return;
            }
        }
        //Clicked with rotate
        else if (curUIInput == null && Input.GetButton("RotateClicked") &&
                 Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
            UnityMolAtom a = getAtomPointed();
            if (a != null) {
                UnityMolStructure s = a.residue.chain.model.structure;
                GameObject sgo = UnityMolMain.getStructureManager().GetStructureGameObject(s.name);
                rotatingT = sgo.transform;

                //Get rotation center of the molecule:
                Transform tpar = UnityMolMain.getRepresentationParent().transform;
                Vector3 worldBary = rotatingT.TransformPoint(s.currentModel.centroid);
                clickedRotationCenter = worldBary;
            }
        }

        //Rotation
        if (!shiftPressed && !mouseClickedOnUI && Input.GetMouseButton(0) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) {
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
                if (mouseSel == null || !mouseSel.duringIMD) {
                    stopCurrentMovements();
                    // currentTransform.RotateAround(currentCenterPosition, transform.up, -Input.GetAxis("Mouse X")*moveSpeed);
                    // currentTransform.RotateAround(currentCenterPosition, transform.right, Input.GetAxis("Mouse Y")*moveSpeed);
                    currentTransform.RotateAround(currentCenterPosition, mainCam.transform.up, -Input.GetAxis("Mouse X")*moveSpeed);
                    currentTransform.RotateAround(currentCenterPosition, mainCam.transform.right, Input.GetAxis("Mouse Y")*moveSpeed);
                    UnityMolMain.getCustomRaycast().needsUpdatePos = true;
                }
            }
        }

        //Depth
        if (!mouseClickedOnUI && Input.GetMouseButton(1) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) {
            stopCurrentMovements();
            if (mainCam.orthographic) {
                float prev = mainCam.orthographicSize;
                API.APIPython.setCameraOrthoSize(prev + -Input.GetAxis("Mouse Y"));
            }
            else
                ZoomInOut(-Input.GetAxis("Mouse Y") * moveSpeed * 0.5f);
        }
        //Translation
        if (!mouseClickedOnUI && Input.GetMouseButton(2) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) {
            stopCurrentMovements();

            Vector3 before = currentTransform.position;
            currentTransform.Translate(mainCam.transform.up * Input.GetAxis("Mouse Y")*moveSpeed * 0.05f, Space.World);
            currentTransform.Translate(mainCam.transform.right * Input.GetAxis("Mouse X")*moveSpeed * 0.05f, Space.World);

            // currentTransform.Translate(transform.up * Input.GetAxis("Mouse Y")*moveSpeed * 0.05f, Space.World);
            // currentTransform.Translate(transform.right * Input.GetAxis("Mouse X")*moveSpeed * 0.05f, Space.World);
            currentCenterPosition += currentTransform.position - before;

            UnityMolMain.getCustomRaycast().needsUpdatePos = true;
        }
        //Depth with scroll
        float scroll = -Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
        if (scroll != 0.0f && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
            stopCurrentMovements();
            if (mainCam.orthographic) {
                float prev = mainCam.orthographicSize;
                API.APIPython.setCameraOrthoSize(prev + scroll);
            }
            else
                ZoomInOut(scroll * moveSpeed);
        }

        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.R)) {
            resetPosition();
            resetRotation();
            setRotationCenter(Vector3.zero);
        }

        if (curUIInput == null && Input.GetKey(KeyCode.F) && UnityMolMain.getSelectionManager().currentSelection != null) {
            centerOnSelection(UnityMolMain.getSelectionManager().currentSelection, true);
        }
    }
}
}
