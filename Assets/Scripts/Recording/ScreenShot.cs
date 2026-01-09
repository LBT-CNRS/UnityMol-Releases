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
using System.Collections;
using System.IO;
using System;
using UMol;

[RequireComponent(typeof(Camera))]
public class ScreenShot : MonoBehaviour
{
    private bool open = false;
    public string directorypath = "";
    // private int frameRate = 30;
    private int width = 1280;
    private int height = 720;

    private int idseq = 1;
    private bool transparentBG = false;
    private Camera curCam;
    public Camera uiCam;

    RenderTexture _tempTarget;

    void Start() {
        // Time.captureFramerate = frameRate;
        curCam = GetComponent<Camera>();
        updateCameraParameters();
    }

    void Update()
    {
        if (open) {
            string file = Path.Combine(directorypath, "img_" + idseq.ToString("D4") + ".png");
            if (transparentBG) {
                takeTransparentScreenshot(width, height, file);
            }
            else {
                takeScreenshot(width, height, file);
            }
            idseq++;
        }
    }

    public void startScreenshotSequence(int resWidth, int resHeight, string dirPath, bool transparent) {
        idseq = 1;
        open = true;
        transparentBG = transparent;
        directorypath = dirPath;
        width = resWidth;
        height = resHeight;

        if (!System.IO.Directory.Exists(directorypath)) {
            // Create the folder
            System.IO.Directory.CreateDirectory(directorypath);
        }
        updateCameraParameters();

    }

    public void stopScreenshotSequence() {
        open = false;
    }

    public void takeScreenshot(int resWidth, int resHeight, string path) {
        if (curCam == null) {
            curCam = GetComponent<Camera>();
        }

        var rt = RenderTexture.GetTemporary(resWidth, resHeight, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);

        byte[] bytes = null;
        if (UnityMolMain.raytracingMode) {
            RaytracerManager rtm = transform.parent.gameObject.GetComponent<RaytracerManager>();
            if (rtm != null) {
                Texture2D t = rtm._texture;
                Graphics.Blit(t, rt);

                uiCam.targetTexture = rt;
                uiCam.Render();
                uiCam.targetTexture = null;


                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
                curCam.targetTexture = null;
                RenderTexture.active = null;
                // Destroy(rt);
                RenderTexture.ReleaseTemporary(rt);
                bytes = screenShot.EncodeToPNG();
            }
        }
        else {
            updateCameraParameters();

            // RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);

            curCam.targetTexture = rt;
            curCam.Render();

            uiCam.targetTexture = rt;
            uiCam.Render();
            uiCam.targetTexture = null;

            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            curCam.targetTexture = null;
            RenderTexture.active = null;
            // Destroy(rt);
            RenderTexture.ReleaseTemporary(rt);
            bytes = screenShot.EncodeToPNG();
        }

        if (bytes != null) {
            System.IO.File.WriteAllBytes(path, bytes);
            Debug.Log(string.Format("Saved screenshot to: {0}", path));
        }
        else {
            Debug.LogError("Failed to take screenshot");
        }
    }

    public void takeTransparentScreenshot(int resWidth, int resHeight, string path) {
        if (curCam == null) {
            curCam = GetComponent<Camera>();
        }

        updateCameraParameters();
        Color saveCameraColor = curCam.backgroundColor;

        var rt = RenderTexture.GetTemporary(resWidth, resHeight, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        curCam.targetTexture = rt;

        curCam.clearFlags = CameraClearFlags.Color;
        curCam.backgroundColor = Color.black;
        curCam.Render();


        uiCam.targetTexture = rt;
        uiCam.Render();
        uiCam.targetTexture = null;


        RenderTexture.active = rt;

        Texture2D lBlackBackgroundCapture = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);
        lBlackBackgroundCapture.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0, false);

        curCam.backgroundColor = Color.white;
        curCam.Render();

        RenderTexture.active = rt;
        Texture2D lWhiteBackgroundCapture = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);
        lWhiteBackgroundCapture.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0, false);

        for (int x = 0; x < lWhiteBackgroundCapture.width; ++x) {
            for (int y = 0; y < lWhiteBackgroundCapture.height; ++y)
            {
                Color lColorWhenBlack = lBlackBackgroundCapture.GetPixel(x, y);
                Color lColorWhenWhite = lWhiteBackgroundCapture.GetPixel(x, y);
                if (lColorWhenBlack != Color.clear)
                {
                    //set real color
                    lWhiteBackgroundCapture.SetPixel(x, y,
                                                     getColor(lColorWhenBlack, lColorWhenWhite));
                }
            }
        }

        lWhiteBackgroundCapture.Apply();
        Texture2D lOut = lWhiteBackgroundCapture;
        byte[] bytes = lOut.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
        RenderTexture.ReleaseTemporary(rt);

        curCam.backgroundColor = saveCameraColor;
        curCam.targetTexture = null;
        RenderTexture.active = null;

        // // RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        // var rt = RenderTexture.GetTemporary(resWidth, resHeight, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        // curCam.targetTexture = rt;
        // Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);

        // Color saveCameraColor = curCam.backgroundColor;
        // curCam.clearFlags = CameraClearFlags.SolidColor;
        // curCam.backgroundColor = new Color(1.0f,1.0f,1.0f,0.0f);

        // curCam.Render();
        // RenderTexture.active = rt;
        // screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0, false);

        // curCam.backgroundColor = saveCameraColor;

        // curCam.targetTexture = null;
        // RenderTexture.active = null; // JC: added to avoid errors
        // // Destroy(rt);
        // RenderTexture.ReleaseTemporary(rt);
        // byte[] bytes = screenShot.EncodeToPNG();
        // System.IO.File.WriteAllBytes(path, bytes);
        Debug.Log(string.Format("Took screenshot to: {0}", path));
    }

    void updateCameraParameters() {
        if (curCam == null) {
            curCam = GetComponent<Camera>();
        }
        Camera mainCam = Camera.main;
        if (curCam != null && mainCam != null) {
            curCam.backgroundColor = mainCam.backgroundColor;
            curCam.farClipPlane = mainCam.farClipPlane;
            curCam.nearClipPlane = mainCam.nearClipPlane;
            if (!UnityMolMain.inVR()) {
                curCam.fieldOfView = mainCam.fieldOfView;
                uiCam.fieldOfView = mainCam.fieldOfView;
                curCam.orthographic = mainCam.orthographic;
                uiCam.orthographic = mainCam.orthographic;
                if (curCam.orthographic) {
                    curCam.orthographicSize = mainCam.orthographicSize;
                    uiCam.orthographicSize = mainCam.orthographicSize;
                }
            }
        }
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
    //pColorWhenBlack!=Color.clear
    static Color getColor(Color pColorWhenBlack, Color pColorWhenWhite)
    {
        float lAlpha = getAlpha(pColorWhenBlack.r, pColorWhenWhite.r);
        return new Color(
                   pColorWhenBlack.r / lAlpha,
                   pColorWhenBlack.g / lAlpha,
                   pColorWhenBlack.b / lAlpha,
                   lAlpha);
    }


    //           Color*Alpha      Color   Color+(1-Color)*(1-Alpha)=1+Color*Alpha-Alpha
    //0----------ColorWhenZero----Color---ColorWhenOne------------1
    static float getAlpha(float pColorWhenZero, float pColorWhenOne)
    {
        //pColorWhenOne-pColorWhenZero=1-Alpha
        return 1 + pColorWhenZero - pColorWhenOne;
    }
}
