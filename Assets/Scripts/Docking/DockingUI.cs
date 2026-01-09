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
using System.Globalization;

using UMol.Docking;

namespace UMol {
public class DockingUI : MonoBehaviour {
    public Text elecText;
    public Text vdwText;
    public Text totalText;

    public Canvas mainCanvas;
    public bool disableWhenIDLE = false;

    private float bigValue = 9999.9f;

    private float reasonableValue = 500.0f;


    private Color badColor = Color.red;
    private Color goodColor = Color.green;

    bool stateDockingMode;
    public DockingManager dockingManager;

    void Start() {
        stateDockingMode = false;
        dockingManager = UnityMolMain.getDockingManager();
    }
    void Update() {

        stateDockingMode = dockingManager.isRunning;

        if (disableWhenIDLE)
            mainCanvas.enabled = stateDockingMode;

        if (stateDockingMode) {
            float elecScaled = dockingManager.calcNBEnergy.nbEnergies.elec * dockingManager.ElecUIScaling;
            float vdwScaled = dockingManager.calcNBEnergy.nbEnergies.vdw * dockingManager.VDWUIScaling;

            elecText.text = TruncateEnergyValue(elecScaled);
            elecText.color = GetEnergyTextColor(elecScaled);

            vdwText.text = TruncateEnergyValue(vdwScaled);
            vdwText.color = GetEnergyTextColor(vdwScaled);

            float total = elecScaled + vdwScaled;
            totalText.text = TruncateEnergyValue(total);
            totalText.color = GetEnergyTextColor(total);
        }
    }
    string TruncateEnergyValue(float energy) {
        string res = "";
        if (energy > bigValue) {
            res = bigValue.ToString("F2", CultureInfo.InvariantCulture);
        }
        else if (energy < -bigValue) {
            res = (-bigValue).ToString("F2", CultureInfo.InvariantCulture);
        }
        else {
            res = energy.ToString("F2", CultureInfo.InvariantCulture);
        }
        return res;
    }

    Color GetEnergyTextColor(float energy) {

        Color textColor = Color.white;

        if (energy >= 0.0f) {
            float scaledEnergy = energy / reasonableValue;
            textColor = Color.Lerp(Color.white, badColor, scaledEnergy);
        }
        else {
            float scaledEnergy = -energy / reasonableValue;
            textColor = Color.Lerp(Color.white, goodColor, scaledEnergy);
        }

        return textColor;
    }
}
}