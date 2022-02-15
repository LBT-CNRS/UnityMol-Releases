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
using UMol.API;

namespace UMol {

[RequireComponent(typeof(VRTK_Pointer))]
[RequireComponent(typeof(VRTK_StraightPointerRendererNoRB))]
public class PointerAtomSelection : MonoBehaviour {

    VRTK_Pointer pointer;
    VRTK_StraightPointerRendererNoRB pointerR;
    UnityMolSelectionManager selM;
    CustomRaycastBurst raycaster;

    void OnEnable() {

        raycaster = UnityMolMain.getCustomRaycast();

        if (pointer == null) {
            pointer = GetComponent<VRTK_Pointer>();
        }
        if (pointerR == null) {
            pointerR = GetComponent<VRTK_StraightPointerRendererNoRB>();
        }
        selM = UnityMolMain.getSelectionManager();

        // pointer.PointerStateValid += DetectedCollision;
        // pointer.PointerStateInvalid += buttonOut;
        pointer.SelectionButtonPressed += buttonPressed;
        // pointer.SelectionButtonReleased += buttonReleased;
        pointer.ActivationButtonReleased += buttonReleased;


    }

    void buttonPressed(object sender, ControllerInteractionEventArgs e) {

        UnityMolAtom a = raycaster.customRaycastAtomBurst(pointerR.actualContainer.transform.position, pointerR.actualTracer.transform.forward);

        if (a == null) {
            return;
        }

        UnityMolAtom hoveredAtom = a;

        if (hoveredAtom != null) {
            UnityMolSelection hoveredSelection = null;
            if (selM.selectionMode == UnityMolSelectionManager.SelectionMode.Atom) {
                hoveredSelection = hoveredAtom.ToSelection();
            }
            else if (selM.selectionMode == UnityMolSelectionManager.SelectionMode.Residue) {
                UnityMolResidue hoveredRes = hoveredAtom.residue;
                hoveredSelection = hoveredRes.ToSelection();
            }
            else if (selM.selectionMode == UnityMolSelectionManager.SelectionMode.Chain) {
                UnityMolChain hoveredChain = hoveredAtom.residue.chain;
                hoveredSelection = hoveredChain.ToSelection();
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
            Debug.Log(curSel);
            // API.APIPython.updateRepresentations(curSel.name);
        }

    }
    void buttonReleased(object sender, ControllerInteractionEventArgs e) {
        gameObject.GetComponent<VRTK_UIPointer>().enabled = true;
    }
    void buttonOut(object sender, DestinationMarkerEventArgs e) {
        gameObject.GetComponent<VRTK_UIPointer>().enabled = true;
    }
    void OnDisable() {
        pointer.SelectionButtonPressed -= buttonPressed;
        pointer.ActivationButtonReleased -= buttonReleased;
    }
}
}