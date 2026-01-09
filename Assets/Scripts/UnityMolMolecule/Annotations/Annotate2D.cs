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
using System.Collections.Generic;
using System.Linq;

namespace UMol {
public class Annotate2D : UnityMolAnnotation {

    /// posPercent defines the position based on the percentage from bottom/left to top/right of the screen
    /// 0/0 means bottom/left and 1/1 means top/right
    public Vector2 posPercent = Vector2.zero;//Bottom right
    public float scale = 1.0f;
    public string content;
    public Color colorText = new Color(0.0f, 0.0f, 0.5f, 1.0f);
    public Canvas screenspaceCan = null;

    public override void Create() {
        GameObject gocan = GameObject.Find("CanvasScreenspace");
        if (gocan == null) {
            gocan = new GameObject("CanvasScreenspace");
            gocan.layer =  LayerMask.NameToLayer("Default");
            screenspaceCan = gocan.AddComponent<Canvas>();
            // screenspaceCan.renderMode = RenderMode.ScreenSpaceOverlay;
            screenspaceCan.renderMode = RenderMode.ScreenSpaceCamera;
            screenspaceCan.worldCamera = Camera.main;
        }
        else {
            screenspaceCan = gocan.GetComponent<Canvas>();
        }

        go = new GameObject("Text2D");
        go.transform.SetParent(gocan.transform);
        go.transform.localPosition = Vector3.zero;
        Text t = go.AddComponent<Text>();
        t.text = content;
        Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        t.font = ArialFont;
        t.material = ArialFont.material;
        t.color = colorText;

        t.fontSize = 140;


        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;

        posPercent.x = Mathf.Clamp(posPercent.x, 0.0f, 1.0f);
        posPercent.y = Mathf.Clamp(posPercent.y, 0.0f, 1.0f);

        (go.transform as RectTransform).anchorMin = posPercent;
        (go.transform as RectTransform).anchorMax = posPercent;

        (go.transform as RectTransform).pivot = Vector2.one * 0.5f;

        go.transform.localScale = Vector3.one * 0.1f * scale;
        (go.transform as RectTransform).anchoredPosition = Vector3.zero;

    }

    public override void Update() {
    }

    public override void UnityUpdate() {
    }

    public override void Delete() {
        if (go != null)
            GameObject.Destroy(go);
    }

    public override void Show(bool show = true) {
        if (go != null) {
            isShown = show;
            go.SetActive(show);
        }
    }

    public override SerializedAnnotation Serialize() {
        SerializedAnnotation san = new SerializedAnnotation();
        san.color = colorText;
        san.posPercent = posPercent;
        san.size = scale;
        san.content = content;
        san.color = colorText;
        fillSerializedAtoms(san);
        return san;
    }

    public override int toAnnoType(){
        return 1;
    }
}
}