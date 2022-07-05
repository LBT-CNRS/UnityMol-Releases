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

            Vector3 nsc = new Vector3(sx, sy, 0.001f) * 1.5f;

            bg.localScale = nsc / transform.lossyScale.x;

            rd.gameObject.transform.rotation = tmpRot;

        }
    }

}
