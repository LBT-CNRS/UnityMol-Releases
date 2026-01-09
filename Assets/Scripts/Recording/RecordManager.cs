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
using FFmpegOut;
using UnityEngine.Rendering.PostProcessing;

namespace UMol {
public static class RecordManager {

    public static bool busy = false;

    public static Camera screenshotCam;
    public static Camera uiCam;

    public static void takeScreenshot(string filePath, int resolutionWidth = 1280, int resolutionHeight = 720, bool transparentBG = false) {
        if (busy) {
            Debug.LogError("Already recording");
            return;
        }
        if (screenshotCam == null) {
            createScreenShotCamera();
        }
        try {
            ScreenShot sc = screenshotCam.GetComponent<ScreenShot>();
            if (transparentBG) {
                sc.takeTransparentScreenshot(resolutionWidth, resolutionHeight, filePath);
            }
            else {
                sc.takeScreenshot(resolutionWidth, resolutionHeight, filePath);
            }
        }
        catch (System.Exception e) {
            Debug.LogError("Error recording in file '" + filePath + "' " + e);
            busy = false;
            return;
        }

    }

    public static void startScreenshotSequence(string folderPath, int resolutionWidth = 1280, int resolutionHeight = 720, bool transparentBG = false) {
        if (busy) {
            Debug.LogError("Already recording");
            return;
        }
        if (screenshotCam == null) {
            createScreenShotCamera();
        }
        busy = true;
        try {
            ScreenShot sc = screenshotCam.GetComponent<ScreenShot>();
            sc.startScreenshotSequence(resolutionWidth, resolutionHeight, folderPath, transparentBG);
        }
        catch {
            Debug.LogError("Failed to start screenshot sequence for folder '" + folderPath + "'");
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

    public static void startRecordingVideo(string filePath, int resolutionWidth = 1280, int resolutionHeight = 720, int frameRate = 30, bool pause = false) {
        if (busy) {
            Debug.LogError("Already recording");
            return;
        }
        if (screenshotCam == null) {
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

            setCanvasCamera();

            recorder.StartRecording(filePath, pause);
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

            restoreCanvasCamera();

        }
        catch {
            Debug.LogError("Failed to stop recording");
        }
        busy = false;
    }

    public static void pauseRecordingVideo() {
        if (!busy) {
            Debug.LogWarning("Not recording");
            return;
        }
        CameraRecord recorder = screenshotCam.GetComponent<CameraRecord>();
        recorder.PauseRecording();
    }

    public static void unpauseRecordingVideo() {
        if (!busy) {
            Debug.LogWarning("Not recording");
            return;
        }
        CameraRecord recorder = screenshotCam.GetComponent<CameraRecord>();
        recorder.UnpauseRecording();
    }

    private static void setCanvasCamera() {
        //Set the 2D canvas camera to the recording camera
        var allA = UnityMolMain.getAnnotationManager().allAnnotations;
        foreach (var a in allA) {
            if (a.GetType().ToString() == "UMol.Annotate2D") {
                Canvas c = (a as Annotate2D).screenspaceCan;
                c.worldCamera = screenshotCam;
                break;
            }
        }
    }

    private static void restoreCanvasCamera() {

        //Set the 2D canvas camera to the main camera
        var allA = UnityMolMain.getAnnotationManager().allAnnotations;
        foreach (var a in allA) {
            if (a.GetType().ToString() == "UMol.Annotate2D") {
                Canvas c = (a as Annotate2D).screenspaceCan;
                c.worldCamera = Camera.main;
                break;
            }
        }
    }

    static void createScreenShotCamera() {

        Transform camPar = Camera.main.transform;
        GameObject camGo = new GameObject("ScreenShotCamera");
        camGo.transform.parent = camPar;
        camGo.transform.localPosition = Vector3.zero;
        camGo.transform.localRotation = Quaternion.identity;



        screenshotCam = camGo.AddComponent<Camera>();
        // screenshotCam.CopyFrom(Camera.main);
        screenshotCam.nearClipPlane = Camera.main.nearClipPlane;
        screenshotCam.farClipPlane = Camera.main.farClipPlane;
        screenshotCam.stereoTargetEye = StereoTargetEyeMask.None;
        // screenshotCam.targetDisplay = 3;
        screenshotCam.enabled = false;
        screenshotCam.cullingMask = ~(1 << LayerMask.NameToLayer("3DUI"));



        if (!UnityMolMain.inVR()) {
            var ppl = camGo.AddComponent<PostProcessLayer>();
            var mainppl = camPar.gameObject.GetComponent<PostProcessLayer>();
            // var mainppvol = camPar.gameObject.GetComponent<PostProcessVolume>();
            var outleu = camPar.gameObject.GetComponent<OutlineEffectUtil>();
            ppl.Init(outleu.postProcessResources);

            ppl.volumeLayer = mainppl.volumeLayer;
            ppl.volumeTrigger = camGo.transform;
        }

        if (uiCam == null) {
            GameObject uicamGo = new GameObject("RecordUICamera");
            uicamGo.transform.parent = camGo.transform;
            uicamGo.transform.localPosition = Vector3.zero;
            uicamGo.transform.localRotation = Quaternion.identity;

            uiCam = uicamGo.AddComponent<Camera>();
            uiCam.nearClipPlane = screenshotCam.nearClipPlane;
            uiCam.farClipPlane = screenshotCam.farClipPlane;
            uiCam.stereoTargetEye = StereoTargetEyeMask.None;
            // uiCam.targetDisplay = 3;
            uiCam.enabled = false;
            uiCam.clearFlags = CameraClearFlags.Depth;
            uiCam.cullingMask = 1 << LayerMask.NameToLayer("3DUI");
            uiCam.depth = -2;

        }
        camGo.AddComponent<ScreenShot>().uiCam = uiCam;
        camGo.AddComponent<CameraRecord>().uiCam = uiCam;
    }
}
}
