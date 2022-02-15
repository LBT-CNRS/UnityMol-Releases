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
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;
using System.Linq;
using UMol.API;
using UnityEngine.Rendering;

namespace UMol {


public class AOHB : MonoBehaviour {

    public enum samplesAOpreset  {
        VeryLow = 16,
        Low = 36,
        Medium = 64,
        High = 144,
        VeryHigh = 256,
        TooMuch = 1024,
        WayTooMuch = 2048
    }

    public bool debugMe = false;

    private LayerMask AOLayer;
    public samplesAOpreset samplesAO = samplesAOpreset.High;
    public int AOtexResolution = 64;//AO texture point per atom

    private Vector3[] rayDir;
    private MeshFilter[] mfs;
    private int[] saveLayer;
    private bool[] saveEnable;
    private ShadowCastingMode[] saveShadowMode;
    private Transform savedPar;
    private Vector3 savedPos;
    private Vector3 savedScale;
    private Quaternion savedRot;

    private Camera AOCam;

    public RenderTexture AORTCur;
    public RenderTexture AORTCumul;
    private Texture2D vertTex;

    public Material AOMatCompute;
    public Material depthMat;

    public RenderTexture depthRT;

    private bool renderAOTex = false;
    bool renderDepthTex = false;

    private float maxDist;
    int N = 0;
    int atombycol = 512;
    Texture2D texAtomPos;


    private Vector3 minPos;
    private Vector3 maxPos;
    private Vector3 bary;

    public void Run(UnityMolSelection sel, UnityMolHBallMeshManager hbm) {


        foreach (UnityMolStructure s in sel.structures) {
            if (s.trajectoryLoaded) {
                Debug.LogWarning("Cannot compute AO for structure with a trajectory loaded");
                return;
            }
        }

        if (sel.Count > 75000) {
            AOtexResolution = 32;
        }
        if (sel.Count > 100000) {
            AOtexResolution = 16;
        }

        float timerAO = Time.realtimeSinceStartup;

        AOLayer = 1 << LayerMask.NameToLayer("AOLayer");
        N = sel.Count;

        savedPar = transform.parent.parent;
        savedRot = transform.parent.rotation;
        savedScale = transform.parent.localScale;
        savedPos = transform.parent.position;

        try {
            InitSamplePos(sel);
            InitTextures(sel);
            CreateAOCam();
            DoAO(hbm);
        }
        catch (System.Exception e) {
            Debug.LogError("Failed to compute AO for hyperballs: " + e);
            transform.parent.parent = savedPar;
            transform.parent.rotation = savedRot;
            transform.parent.localScale = savedScale;
            transform.parent.position = savedPos;

            return;
        }


#if UNITY_EDITOR
        Debug.Log("Time for full AO: " + (1000.0f * (Time.realtimeSinceStartup - timerAO)).ToString("f3") + " ms");
#endif
    }


    void getBounds() {
        saveLayer = new int[mfs.Length];
        saveEnable = new bool[mfs.Length];
        saveShadowMode = new ShadowCastingMode[mfs.Length];


        for (int i = 0; i < mfs.Length; i++) {
            MeshRenderer mr = mfs[i].gameObject.GetComponent<MeshRenderer>();


            saveEnable[i] = mr.enabled;
            saveLayer[i] = mfs[i].gameObject.layer;
            saveShadowMode[i] = mr.shadowCastingMode;

            mr.shadowCastingMode = ShadowCastingMode.TwoSided;
            mr.enabled = true;

        }

    }

    void InitSamplePos(UnityMolSelection sel) {

        transform.parent.parent = null;
        transform.parent.localScale = Vector3.one;
        transform.parent.position = Vector3.zero;
        transform.parent.rotation = Quaternion.identity;

        mfs = transform.parent.GetComponentsInChildren<MeshFilter>();

        getBounds();

        texAtomPos = new Texture2D(atombycol, (int)Mathf.Ceil(N / (float)atombycol), TextureFormat.RGBAFloat, false);
        texAtomPos.filterMode = FilterMode.Point;
        texAtomPos.wrapMode = TextureWrapMode.Clamp;
        texAtomPos.anisoLevel = 0;

        Transform loadedMol = UnityMolMain.getRepresentationParent().transform;

        // Vector3 transPos = transform.parent.TransformPoint(sel.atoms[0].correspondingGo.transform.parent.InverseTransformPoint(sel.atoms[0].curWorldPosition));

        // minPos = transPos;
        // maxPos = transPos;

        minPos = transform.parent.TransformPoint(sel.atoms[0].position);
        maxPos = transform.parent.TransformPoint(sel.atoms[0].position);


        bary = Vector3.zero;

        Vector4 tmpos = Vector3.zero;
        for (int i = 0; i < N; i++) {
            UnityMolAtom a = sel.atoms[i];
            UnityMolStructure s = a.residue.chain.model.structure;
            Vector3 k = transform.parent.TransformPoint(a.position);

            tmpos.x = k.x;
            tmpos.y = k.y;
            tmpos.z = k.z;
            tmpos.w = sel.atoms[i].radius;

            texAtomPos.SetPixel(i % atombycol, i / atombycol, tmpos);

            bary += k;
            minPos.x = Mathf.Min(minPos.x, k.x);
            minPos.y = Mathf.Min(minPos.y, k.y);
            minPos.z = Mathf.Min(minPos.z, k.z);
            maxPos.x = Mathf.Max(maxPos.x, k.x);
            maxPos.y = Mathf.Max(maxPos.y, k.y);
            maxPos.z = Mathf.Max(maxPos.z, k.z);

            // GameObject testT = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // testT.transform.parent = transform.parent;
            // testT.transform.localScale = Vector3.one * 1.0f;
            // testT.transform.position = k;

        }
        texAtomPos.Apply(false, false);

        bary /= (float)N;
        float maxSize = Mathf.Max(Vector3.Distance(bary, maxPos), Vector3.Distance(bary, minPos));

        rayDir = new Vector3[(int)samplesAO];
        float golden_angle = Mathf.PI * (3 - Mathf.Sqrt(5));
        float start =  1 - 1.0f / (int)samplesAO;
        float end = 1.0f / (int)samplesAO - 1;

        for (int i = 0; i < (int)samplesAO; i++) {
            float theta = golden_angle * i;
            float z = start + i * (end - start) / (int)samplesAO;
            float radius = Mathf.Sqrt(1 - z * z);
            rayDir[i].x = radius * Mathf.Cos(theta);
            rayDir[i].y = radius * Mathf.Sin(theta);
            rayDir[i].z = z;
            rayDir[i] = bary + rayDir[i] * maxSize;

            // GameObject test = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // test.transform.parent = transform.parent;
            // test.transform.localScale = Vector3.one * 1.0f;
            // test.transform.position = rayDir[i];

            // Debug.DrawLine(bary, rayDir[i], Color.yellow, 1000.0f);
        }
        maxDist = maxSize;


    }
    void changeAspectRatio() {
        float targetaspect = 1.0f;

        // determine the game window's current aspect ratio
        float windowaspect = (float)Screen.width / (float)Screen.height;

        // current viewport height should be scaled by this amount
        float scaleheight = windowaspect / targetaspect;


        // if scaled height is less than current height, add letterbox
        if (scaleheight < 1.0f)
        {
            Rect rect = AOCam.rect;

            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;

            AOCam.rect = rect;
        }
        else // add pillarbox
        {
            float scalewidth = 1.0f / scaleheight;

            Rect rect = AOCam.rect;

            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;

            AOCam.rect = rect;
        }

    }

    void CreateAOCam() {

        AOCam = gameObject.AddComponent<Camera>();
        if (AOCam == null)
            AOCam = gameObject.GetComponent<Camera>();


        AOCam.enabled = false;

        AOCam.orthographic = true;
        AOCam.cullingMask = AOLayer;
        AOCam.clearFlags = CameraClearFlags.Depth;
        AOCam.nearClipPlane = 0.1f;
        AOCam.allowHDR = false;
        AOCam.allowMSAA = false;
        AOCam.allowDynamicResolution = false;
        AOCam.stereoTargetEye = StereoTargetEyeMask.None;

        AOCam.depthTextureMode = DepthTextureMode.Depth;

        AOCam.orthographicSize = maxDist ;
        AOCam.farClipPlane = maxDist * 2;
        // AOCam.aspect = 1f;
        // changeAspectRatio();
    }


    void InitTextures(UnityMolSelection sel) {

        AOMatCompute = new Material(Shader.Find("UMol/AOComputeHB"));
        // depthMat = new Material(Shader.Find("Custom/RenderDepth"));

        int sqrtupcount = Mathf.CeilToInt(Mathf.Sqrt(N));
        int sizeRT = sqrtupcount * AOtexResolution;

        AORTCur = new RenderTexture(sizeRT, sizeRT, 0, RenderTextureFormat.ARGBHalf);
        AORTCumul = new RenderTexture(sizeRT, sizeRT, 0, RenderTextureFormat.ARGBHalf);
        AORTCur.anisoLevel = 0;
        AORTCumul.anisoLevel = 0;
        AORTCur.filterMode = FilterMode.Point;
        AORTCumul.filterMode = FilterMode.Point;

        AOMatCompute.SetTexture("_AtomPos", texAtomPos);

        AOMatCompute.SetInt("_Sampling", (int)samplesAO - 1);
        AOMatCompute.SetTexture("_AOTex", AORTCumul);
        AOMatCompute.SetInt("_NBAtoms", N);
        AOMatCompute.SetInt("_AORes", AOtexResolution);
        AOMatCompute.SetInt("_IDSample", 0);
        AOMatCompute.SetInt("_AtomsByCol", atombycol);
        AOMatCompute.SetVector("_ScreenSize", new Vector4(AORTCur.width, AORTCur.height, 0f, 0f));


        // depthRT = new RenderTexture(sizeRT, sizeRT, 0, RenderTextureFormat.ARGBFloat);


    }

    void DoAO(UnityMolHBallMeshManager hbm) {
        for (int i = 0; i < mfs.Length; i++) {
            mfs[i].gameObject.layer = LayerMask.NameToLayer("AOLayer");
        }
        for (int i = 0; i < (int)samplesAO; i++) {
            AOCam.transform.position = rayDir[i];
            AOCam.transform.LookAt(bary);

            AOMatCompute.SetInt("_IDSample", i);

            renderAOTex = true;
            AOCam.Render();

        }


        List<Vector2> atlasinfo = new List<Vector2>(N);
        int idx = 0;
        int idy = 0;
        for (int i = 0; i < N; i++) {
            Vector2 idatom = Vector2.zero;
            idatom.x = (i * AOtexResolution) % AORTCur.width;
            idatom.y = AOtexResolution * ((i * AOtexResolution) / AORTCur.height);
            atlasinfo.Add(idatom);
        }

        hbm.updateAOInfo(atlasinfo);

        AORTCur.filterMode = FilterMode.Bilinear;

        for (int i = 0; i < mfs.Length; i++) {
            mfs[i].gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_AOTex", AORTCur);
            mfs[i].gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_AOStrength", 1.0f);
            mfs[i].gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_AOTexwidth", AORTCur.width);
            mfs[i].gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_AOTexheight", AORTCur.height);
            mfs[i].gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetInt("_AORes", AOtexResolution);

        }

        //Restore
        for (int i = 0; i < mfs.Length; i++) {
            mfs[i].gameObject.layer = saveLayer[i];
            mfs[i].gameObject.GetComponent<Renderer>().shadowCastingMode = saveShadowMode[i];
            mfs[i].gameObject.GetComponent<Renderer>().enabled = saveEnable[i];
            transform.parent.parent = savedPar;
            transform.parent.rotation = savedRot;
            transform.parent.localScale = savedScale;
            transform.parent.position = savedPos;
        }

    }

    void OnRenderImage (RenderTexture source, RenderTexture destination) {
        if (renderAOTex) {


            Matrix4x4 V = AOCam.worldToCameraMatrix;
            Matrix4x4 P = AOCam.projectionMatrix;

            bool d3d = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;
            if (d3d) {
                // Invert Y for rendering to a render texture
                for (int a = 0; a < 4; a++) {
                    P[1, a] = -P[1, a];
                }
                // Scale and bias from OpenGL -> D3D depth range
                for (int a = 0; a < 4; a++) {
                    P[2, a] = P[2, a] * 0.5f + P[3, a] * 0.5f;
                }
            }

            AOMatCompute.SetMatrix("_CustomVP", (P * V));


            var matrix = AOCam.cameraToWorldMatrix;
            AOMatCompute.SetMatrix("_InverseView", matrix);
            Graphics.Blit(source, AORTCur, AOMatCompute);
            AOCam.targetTexture = null;
            Graphics.Blit(AORTCur, AORTCumul);
            renderAOTex = false;
        }

        if (renderDepthTex) {
            Graphics.Blit(null, depthRT, depthMat);
            AOCam.targetTexture = null;
            renderDepthTex = false;
        }
    }

    void Update() {
        if (debugMe) {
            AOMatCompute.SetInt("_IDSample", 0);
            AOMatCompute.SetInt("_Sampling",  1);


            AOCam.transform.LookAt(bary);
            renderAOTex = true;
            renderDepthTex = true;
            AOCam.Render();
        }
    }
}
}