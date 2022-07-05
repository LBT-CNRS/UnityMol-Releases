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
using FFmpegOut;


public static class RecordManager {

    public static bool busy = false;

    public static Camera screenshotCam;

    public static void takeScreenshot(string filePath, int resolutionWidth = 1280, int resolutionHeight = 720, bool transparentBG = false) {
        if (busy) {
            Debug.LogError("Already recording");
            return;
        }
        if(screenshotCam == null){
            createScreenShotCamera();
        }
        try{
            ScreenShot sc = screenshotCam.GetComponent<ScreenShot>();
            if(transparentBG){
                sc.takeTransparentScreenshot(resolutionWidth, resolutionHeight, filePath);
            }
            else{
                sc.takeScreenshot(resolutionWidth, resolutionHeight, filePath);
            }
        }
        catch(System.Exception e) {
            Debug.LogError("Error recording in file '" + filePath + "' "+e);
            busy = false;
            return;
        }

    }

    public static void startScreenshotSequence(string folderPath, int resolutionWidth = 1280, int resolutionHeight = 720, bool transparentBG = false) {
        if (busy) {
            Debug.LogError("Already recording");
            return;
        }
        if(screenshotCam == null){
            createScreenShotCamera();
        }
        busy = true;
        try{
            ScreenShot sc = screenshotCam.GetComponent<ScreenShot>();
            sc.startScreenshotSequence(resolutionWidth, resolutionHeight, folderPath, transparentBG);
        }
        catch{
            Debug.LogError("Failed to start screenshot sequence for folder '"+folderPath+"'");
        }

    }
    public static void stopScreenshotSequence() {
        if (!busy) {
            Debug.LogWarning("Not recording");
            return;
        }

        ScreenShot sc = screenshotCam.GetComponent<ScreenShot>();
        sc.stopScreenshotSequence();
        busy = false;

    }

    public static void startRecordingVideo(string filePath, int resolutionWidth = 1280, int resolutionHeight = 720, int frameRate = 30) {
        if (busy) {
            Debug.LogError("Already recording");
            return;
        }
        if(screenshotCam == null){
            createScreenShotCamera();
        }
        try {
            CameraRecord recorder = screenshotCam.GetComponent<CameraRecord>();
            if (resolutionWidth > 0 && resolutionHeight > 0 && frameRate > 0) {
                recorder._width = resolutionWidth;
                recorder._height = resolutionHeight;
                recorder._frameRate = frameRate;
            }
            else {
                Debug.LogWarning("Resolution or frameRate should be > 0");
            }
            recorder.StartRecording(filePath);
        }
        catch {
            Debug.LogError("Error recording in file '" + filePath + "'");
            busy = false;
            return;
        }

        busy = true;
    }

    public static void stopRecordingVideo() {
        if (!busy) {
            Debug.LogWarning("Not recording");
            return;
        }
        try {
            CameraRecord recorder = screenshotCam.GetComponent<CameraRecord>();
            recorder.StopRecording();
            //Reset default values
            recorder._width = 1280;
            recorder._height = 720;
            recorder._frameRate = 30;
        }
        catch {
            Debug.LogError("Failed to stop recording");
        }
        busy = false;
    }
    static void createScreenShotCamera(){
        Transform camPar = Camera.main.transform;
        GameObject camGo = new GameObject("ScreenShotCamera");
        camGo.transform.parent = camPar;
        camGo.transform.localPosition = Vector3.zero;
        camGo.transform.localRotation = Quaternion.identity;

        screenshotCam = camGo.AddComponent<Camera>();
        // screenshotCam.AddComponent<Smaa.SMAA>();
        // screenshotCam.CopyFrom(Camera.main);
        screenshotCam.nearClipPlane = Camera.main.nearClipPlane;
        screenshotCam.farClipPlane = Camera.main.farClipPlane;
        screenshotCam.stereoTargetEye = StereoTargetEyeMask.None;
        screenshotCam.targetDisplay = 3;
        screenshotCam.enabled = false;

        camGo.AddComponent<ScreenShot>();
        camGo.AddComponent<CameraRecord>();


    }

}