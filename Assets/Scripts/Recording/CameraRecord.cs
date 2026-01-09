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

namespace FFmpegOut
{
[RequireComponent(typeof(Camera))]
[AddComponentMenu("FFmpegOut/Camera Record")]
public class CameraRecord : MonoBehaviour
{
    #region Editable properties

    [SerializeField] bool _setResolution = true;
    [SerializeField] public int _width = 1280;
    [SerializeField] public int _height = 720;
    [SerializeField] public int _frameRate = 30;
    [SerializeField] bool _allowSlowDown = true;
    [SerializeField] FFmpegPipe.Preset _preset;
    [SerializeField] string filePath;
    public bool _pause = false;
    public Camera uiCam;


    #endregion

    #region Private members

    [SerializeField, HideInInspector] Shader _shader;
    Material _material;

    FFmpegPipe _pipe;
    float _elapsed;

    RenderTexture _tempTarget;
    GameObject _tempBlitter;

    static int _activePipeCount;

    RenderTexture sourceUI;
    UMol.RaytracerManager rtm;

    #endregion

    #region MonoBehavior functions


    void OnEnable()
    {
        if (!FFmpegConfig.CheckAvailable)
        {
            Debug.LogError(
                "ffmpeg.exe is missing. " +
                "Please refer to the installation instruction. " +
                "https://github.com/keijiro/FFmpegOut"
            );
            enabled = false;
            return;
        }
        if (rtm == null)
            rtm = transform.parent.gameObject.GetComponent<UMol.RaytracerManager>();
    }

    void OnDisable()
    {
        if (_pipe != null) ClosePipe();
    }

    void OnDestroy()
    {
        if (_pipe != null) ClosePipe();
    }

    void Start()
    {
        if (_shader == null) {
            _shader = Shader.Find("Hidden/FFmpegOut/CameraCapture");
        }
        _material = new Material(_shader);
    }


    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_pause)
            return;

        if (_pipe != null) {

            var tempRT = RenderTexture.GetTemporary(source.width, source.height);

            if (UMol.UnityMolMain.raytracingMode) {
                if (rtm == null)
                    rtm = transform.parent.gameObject.GetComponent<UMol.RaytracerManager>();
                Texture2D t = rtm._texture;

                Graphics.Blit(t, sourceUI);

            }
            else {

                Graphics.Blit(source, sourceUI);
                // Graphics.Blit(source, tempRT, _material, 0);
            }

            uiCam.targetTexture = sourceUI;
            uiCam.Render();
            uiCam.targetTexture = null;

            // Graphics.Blit(source, tempRT, _material, 1);
            Graphics.Blit(sourceUI, tempRT, _material, 0);


            var tempTex = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            tempTex.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);
            tempTex.Apply();

            _pipe.Write(tempTex.GetRawTextureData());

            Destroy(tempTex);
            RenderTexture.ReleaseTemporary(tempRT);

            Graphics.Blit(sourceUI, destination);

        }
        else
            Graphics.Blit(source, destination);
    }


    #endregion

    #region Private methods

    void OpenPipe()
    {
        if (_pipe != null) return;
        _pause = false;

        var camera = GetComponent<Camera>();
        var width = _width;
        var height = _height;

        // Apply the screen resolution settings.
        if (_setResolution)
        {
            _tempTarget = RenderTexture.GetTemporary(width, height, 24);
            sourceUI = RenderTexture.GetTemporary(width, height, 24);
            camera.targetTexture = _tempTarget;
            uiCam.targetTexture = sourceUI;
            _tempBlitter = Blitter.CreateGameObject(camera);
        }
        else
        {
            width = camera.pixelWidth;
            height = camera.pixelHeight;
        }

        if (filePath == null || filePath == "") {
            filePath = name;
        }
        // Open an output stream.
        _pipe = new FFmpegPipe(filePath, width, height, _frameRate, _preset);
        _activePipeCount++;

        // Change the application frame rate on the first pipe.
        if (_activePipeCount == 1)
        {
            if (_allowSlowDown)
                Time.captureFramerate = _frameRate;
            else
                Application.targetFrameRate = _frameRate;
        }

        camera.enabled = true;
        Debug.Log("Capture started (" + _pipe.Filename + ")");
    }

    void ClosePipe()
    {
        var camera = GetComponent<Camera>();

        // Destroy the blitter object.
        if (_tempBlitter != null)
        {
            Destroy(_tempBlitter);
            _tempBlitter = null;
        }

        // Release the temporary render target.
        if (_tempTarget != null && _tempTarget == camera.targetTexture)
        {
            camera.targetTexture = null;
            RenderTexture.ReleaseTemporary(_tempTarget);
            _tempTarget = null;
        }
        if (sourceUI != null) {
            uiCam.targetTexture = null;
            RenderTexture.ReleaseTemporary(sourceUI);
            sourceUI = null;
        }

        // Close the output stream.
        if (_pipe != null)
        {
            Debug.Log("Capture ended (" + _pipe.Filename + ")");

            _pipe.Close();
            _activePipeCount--;

            if (!string.IsNullOrEmpty(_pipe.Error))
            {
                Debug.LogWarning(
                    "ffmpeg returned with a warning or an error message. " +
                    "See the following lines for details:\n" + _pipe.Error
                );
            }

            _pipe = null;

            // Reset the application frame rate on the last pipe.
            if (_activePipeCount == 0)
            {
                if (_allowSlowDown)
                    Time.captureFramerate = 0;
                else
                    Application.targetFrameRate = -1;
            }
        }
        camera.enabled = false;
    }
    public void StartRecording(string path, bool pausing = false) {
        if (_pipe != null) {
            Debug.LogWarning("Already recording");
            return;
        }
        filePath = path;
        _pause = pausing;
        OpenPipe();
    }

    public void PauseRecording() {
        _pause = true;
    }

    public void UnpauseRecording() {
        _pause = false;
    }

    public void StopRecording() {
        ClosePipe();
    }

    public void rec() {
        if (_pipe == null)
            StartRecording(Application.dataPath);
        else
            StopRecording();
    }

    #endregion
}
}
