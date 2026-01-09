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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UMol {

[RequireComponent(typeof(Button))]
public class SwitchDockingModeUI : MonoBehaviour {

    private string firstPartText = "Docking mode:\n<b><color=#007AC1>";
    public List<Button> dockingButtons = new List<Button>();
    public List<GameObject> toShowHide = new List<GameObject>();

    public Text butText;

    void Start() {
        if (butText == null) {
            butText = GetComponentsInChildren<Text>()[0];
        }
    }

    public void switchDockingModeText() {
        string newText = "";
        if(UnityMolMain.getDockingManager().isRunning){
            newText += "On</color></b>";
        }
        else{
            newText += "Off</color></b>";
        }
        butText.text = firstPartText + newText;

        foreach (Button button in dockingButtons){
            button.interactable = UnityMolMain.getDockingManager().isRunning;
        }
        foreach(GameObject go in toShowHide){
            go.SetActive(UnityMolMain.getDockingManager().isRunning);
        }
    }
}
}