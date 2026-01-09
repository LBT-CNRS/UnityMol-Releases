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
using UnityEngine.EventSystems;
using UMol;

/// <summary>
/// Component to activate the VR Keyboard during an InputField
/// </summary>
[RequireComponent(typeof(InputField))]
public class activateKeyboard : MonoBehaviour {


    /// <summary>
    /// GameObject holding the VR Keyboard.
    /// Should be initialized to the GameObject in the scene.
    /// </summary>
    public GameObject Keyboard ;

    /// <summary>
    /// InputField to populate with the keyboard
    /// </summary>
    private InputField currentInputField;

    private void OnEnable() {
        //If this Component is inside a prefab, we need to initialize 'Keyboard' dynamically.
        if (Keyboard == null) {
            Keyboard = FindObjectOfType<KeyboardUI>(true).gameObject;
        }
    }

    void Update () {
        //Always update the currentinputField to handle changes in focus (i.e. users click to another inputfield)
        currentInputField = GetComponent<InputField>();

        if (UnityMolMain.inVR() && EventSystem.current.currentSelectedGameObject == gameObject) {
            if (Keyboard != null && currentInputField.isFocused)
            {
                if (!Keyboard.activeInHierarchy) {
                    Keyboard.SetActive(true);
                }
                Keyboard.GetComponent<KeyboardUI>().inpF = currentInputField;
            }
        }
    }
}
