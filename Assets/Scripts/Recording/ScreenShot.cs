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
        updateCameraParameters();

        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        curCam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        curCam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        curCam.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
        Debug.Log(string.Format("Saved screenshot to: {0}", path));
    }
    public void takeTransparentScreenshot(int resWidth, int resHeight, string path) {
        if (curCam == null) {
            curCam = GetComponent<Camera>();
        }
        updateCameraParameters();

        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        curCam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);

        Color saveCameraColor = curCam.backgroundColor;
        curCam.backgroundColor = new Color(saveCameraColor.r, saveCameraColor.g, saveCameraColor.b, 0f);

        curCam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);

        curCam.backgroundColor = saveCameraColor;

        curCam.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
        Debug.Log(string.Format("Took screenshot to: {0}", path));
    }

    void updateCameraParameters() {
        if (curCam == null) {
            curCam = GetComponent<Camera>();
        }
        Camera mainCam = Camera.main;
        if(curCam != null && mainCam != null){
            curCam.backgroundColor = mainCam.backgroundColor;
            curCam.farClipPlane = mainCam.farClipPlane;
            curCam.nearClipPlane = mainCam.nearClipPlane;
            if(!UnityMolMain.inVR()){
                curCam.fieldOfView = mainCam.fieldOfView;
                curCam.orthographic = mainCam.orthographic;
                if(curCam.orthographic){
                    curCam.orthographicSize = mainCam.orthographicSize;
                }
            }
        }
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}
