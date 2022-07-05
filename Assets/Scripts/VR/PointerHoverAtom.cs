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
using System.Text;
using VRTK;

namespace UMol {

[RequireComponent(typeof(VRTK_Pointer))]
[RequireComponent(typeof(VRTK_StraightPointerRendererNoRB))]
public class PointerHoverAtom : MonoBehaviour {

    VRTK_Pointer pointer;
    VRTK_StraightPointerRendererNoRB pointerR;
    GameObject haloGo;
    TextMesh textm;
    Transform camTransform;

    bool pressed = false;
    CustomRaycastBurst raycaster;

    float hoverScaleMultiplier = 1.0f;

    public bool pauseHovering = false;


    void Start() {
        raycaster = UnityMolMain.getCustomRaycast();

        if (pointer == null) {
            pointer = GetComponent<VRTK_Pointer>();
        }
        if (pointerR == null) {
            pointerR = GetComponent<VRTK_StraightPointerRendererNoRB>();
        }
        // pointer.PointerStateValid += DetectedCollision;
        pointer.PointerStateInvalid += OutCollision;
        pointer.ActivationButtonReleased += buttonReleased;
        pointer.ActivationButtonPressed += buttonPressed;

        haloGo = GameObject.Instantiate((GameObject) Resources.Load("Prefabs/SphereOverAtom"));
        textm = haloGo.GetComponentsInChildren<TextMesh>()[0];

        // haloGo.layer = LayerMask.NameToLayer("Ignore Raycast");
        haloGo.SetActive(false);
        textm.gameObject.GetComponent<MeshRenderer>().sortingOrder = 50;

    }

    // void DetectedCollision(object sender, DestinationMarkerEventArgs e) {
    //     RaycastHit raycastHit =  e.raycastHit;

    //     showHover(raycastHit);

    // }
    void Update() {
        if(pauseHovering){
            disableHovering();
            return;
        }
        if (pressed) {
            showHover();
            // showHover(pointer.pointerRenderer.GetDestinationHit());
        }
    }
    public static string formatAtomText(UnityMolAtom a) {
        string nameS = a.residue.chain.model.structure.formatName(25);


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

        UnityMolAtom a = raycaster.customRaycastAtomBurst(pointerR.actualContainer.transform.position, pointerR.actualContainer.transform.forward);
        if (a != null) {
            if (haloGo == null) {
                haloGo = GameObject.Instantiate((GameObject) Resources.Load("Prefabs/SphereOverAtom"));
                textm = haloGo.GetComponentsInChildren<TextMesh>()[0];
                haloGo.layer = LayerMask.NameToLayer("Ignore Raycast");
                haloGo.SetActive(false);
            }
            UnityMolStructureManager sm = UnityMolMain.getStructureManager();
            Transform atomPar = sm.structureToGameObject[a.residue.chain.model.structure.uniqueName].transform;


            textm.text = formatAtomText(a);
            haloGo.SetActive(true);
            haloGo.transform.position = atomPar.TransformPoint(a.position);

            if (camTransform == null) {
                camTransform = Camera.main.transform;
            }

            haloGo.transform.rotation = Quaternion.LookRotation(haloGo.transform.position - camTransform.position);
            haloGo.transform.parent = a.residue.chain.model.structure.atomToGo[a].transform;

            haloGo.transform.localScale =  hoverScaleMultiplier * a.radius * Vector3.one * 1.1f;


            //Limit the length of the pointer renderer when hitting something
            float dist = Vector3.Distance(transform.position, haloGo.transform.position);
            pointerR.maximumLength = dist;
            pointerR.SetValidColor();

        }

    }
    void disableHovering() {
        if (haloGo != null) {
            haloGo.SetActive(false);
            haloGo.transform.parent = null;
        }
        pointerR.maximumLength = 100;
    }

    void OutCollision(object sender, DestinationMarkerEventArgs e) {
        disableHovering();
    }
    void buttonReleased(object sender, ControllerInteractionEventArgs e) {
        disableHovering();
        pressed = false;
    }
    void buttonPressed(object sender, ControllerInteractionEventArgs e) {
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
