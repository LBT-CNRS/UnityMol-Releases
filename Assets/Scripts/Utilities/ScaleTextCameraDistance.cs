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

public class ScaleTextCameraDistance : MonoBehaviour {
    public float distanceFactor = 100.0f;
    public float minSize = 0.1f;
    public float maxSize = 2.0f;

    Camera mainCam;
    Transform savedParent;

    TextMesh tm;
    Renderer rd;
    Transform bg;

    void Awake () {
        savedParent = transform.parent;
        transform.parent = null;
        mainCam = Camera.main;
        transform.parent = savedParent;

        tm = GetComponent<TextMesh>();
        if (tm != null) {
            rd = GetComponent<Renderer>();
            bg = transform.GetChild(0);
        }
    }

    void Start() {
        if (tm == null) {
            tm = GetComponent<TextMesh>();
        }
        if (mainCam == null) {
            mainCam = Camera.main;
        }

        scaleCamDist();
        scaleBackground();
    }

    void Update () {
        if (mainCam == null) {
            mainCam = Camera.main;
        }

        scaleCamDist();

        // scaleBackground();


    }

    void scaleCamDist()
    {
        if (mainCam == null) {
            return;
        }
        float dist = Vector3.Distance(mainCam.transform.position, transform.position);
        float scaling = dist / distanceFactor;
        Vector3 newScale = Vector3.one * scaling;

        transform.localScale = Vector3.one;

        float clampedScale = Mathf.Clamp(scaling, minSize * transform.lossyScale.x, maxSize * transform.lossyScale.x);

        transform.localScale = Vector3.one * clampedScale / transform.lossyScale.x;

        // savedParent = transform.parent;
        // transform.parent = null;
        // transform.localScale = newScale;
        // transform.parent = savedParent;
    }

    void scaleBackground() {
        if (rd == null) {
            if (tm != null) {
                rd = GetComponent<Renderer>();
                bg = transform.GetChild(0);
            }
        }

        if (rd != null) {

            Quaternion tmpRot = rd.gameObject.transform.rotation;
            rd.gameObject.transform.rotation = Quaternion.identity;

            float sx = 0.0f;
            float sy = 0.0f;

            sy = rd.bounds.size.y;
            sx = Mathf.Max(sy * 3.33f, rd.bounds.size.x);
            sy = Mathf.Clamp(sy, sx / 3.33f, rd.bounds.size.y);

            Vector3 nsc = new Vector3(sx, sy, 0.0f) * 1.5f;

            float zscale = bg.localScale.z;
            bg.localScale = new Vector3(nsc.x / transform.lossyScale.x, nsc.y / transform.lossyScale.x, zscale);

            rd.gameObject.transform.rotation = tmpRot;

        }
    }

}
