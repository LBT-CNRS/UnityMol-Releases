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
using System.Runtime.InteropServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace UMol
{
public class RaytracedObject : MonoBehaviour
{


    [DllImport("UnityPathTracer")]
    static extern bool UnityPathTracerIsInit(IntPtr RTObj);

    [DllImport("UnityPathTracer")]
    static extern void RT_SetBackgroundColor(IntPtr RTObj, Vector3 color);

    [DllImport("UnityPathTracer")]
    static extern void RT_SetCameraTransform(IntPtr RTObj, float[] matrix);

    [DllImport("UnityPathTracer")]
    static extern void RT_SetCameraAperture(IntPtr RTObj, float aper);

    [DllImport("UnityPathTracer")]
    static extern void RT_SetCameraFocalDistance(IntPtr RTObj, float fdist);

    [DllImport("UnityPathTracer")]
    static extern void RT_SetCameraFOV(IntPtr RTObj, float fov);

    [DllImport("UnityPathTracer")]
    static extern int RT_AddLight(IntPtr RTObj, int id, Vector3 dir, Vector3 pos);

    [DllImport("UnityPathTracer")]
    static extern void RT_SetLightPosition(IntPtr RTObj, int id, Vector3 pos);

    [DllImport("UnityPathTracer")]
    static extern void RT_SetLightDirection(IntPtr RTObj, int id, Vector3 dir);

    [DllImport("UnityPathTracer")]
    static extern void RT_SetLightColor(IntPtr RTObj, int id, float r, float g, float b, float a);

    [DllImport("UnityPathTracer")]
    static extern void RT_SetLightIntensity(IntPtr RTObj, int id, float intensity);

    [DllImport("UnityPathTracer")]
    static extern void RT_ShowHideLight(IntPtr RTObj, int id, bool show);

    [DllImport("UnityPathTracer")]
    static extern int RT_AddMesh(IntPtr RTObj, int vertCount,
                                 int triCount,
                                 bool hasNormals,
                                 bool hasUV,
                                 Vector3[] vertices,
                                 int[] triangles,
                                 Vector3[] normals,
                                 float[] uvs);

    [DllImport("UnityPathTracer")]
    static extern void RT_SetMeshTransform(IntPtr RTObj, int id, float[] matrix);

    [DllImport("UnityPathTracer")]
    static extern void RT_ChangeMaterialType(IntPtr RTObj, int id, string type);


    [DllImport("UnityPathTracer")]
    static extern void RT_SetMeshVertexColor(IntPtr RTObj, int id, float[] colors, int count);

    [DllImport("UnityPathTracer")]
    static extern void RT_SetMeshMaterialPropertyFloat(IntPtr RTObj, int id, string prop, float v);

    [DllImport("UnityPathTracer")]
    static extern void RT_SetMeshMaterialPropertyFloat3(IntPtr RTObj, int id, string prop, Vector3 v);

    [DllImport("UnityPathTracer")]
    static extern void RT_SetMeshMaterialPropertyBool(IntPtr RTObj, int id, string prop, bool v);


    [DllImport("UnityPathTracer")]
    static extern void RT_ShowHideMesh(IntPtr RTObj, int id, bool show);

    [DllImport("UnityPathTracer")]
    static extern int RT_AddSpheres(IntPtr URT, int count, Vector3[] pos, float[] radii, Vector3[] colors);

    [DllImport("UnityPathTracer")]
    static extern void RT_SetSpheresTransform(IntPtr URT, int id, float[] matrix);

    [DllImport("UnityPathTracer")]
    static extern int RT_AddTexture(IntPtr URT, Color[] data, int width, int height);

    [DllImport("UnityPathTracer")]
    static extern void RT_SetMeshAlbedoMap(IntPtr URT, int id, int idTexture);//Use idTexture = -1 to disable albedo map

    [DllImport("UnityPathTracer")]
    static extern void RT_SetMeshNormalMap(IntPtr URT, int id, int idTexture);//Use idTexture = -1 to disable normal map

    public enum RayTObjectType
    {
        camera = 0,
        light = 1,
        // sphere = 2,
        mesh = 3,
        spheres = 4
    }

    public RayTObjectType type = RayTObjectType.mesh;
    private RaytracingMaterial _rtMat = new RaytracingPrincipledMaterial();
    public RaytracingMaterial rtMat {
        get { return _rtMat;}
        set {
            _rtMat = value;
            shouldUpdateMatType = true;//Don't call the function directly but wait for rtobject to start
        }
    }

    public delegate void OnNewRTMaterial(NewRTMatEventArgs args);
    public static OnNewRTMaterial onNewRTMaterial;

    public int idObject = -1;

    private int currentLightType = 0;

    [HideInInspector]
    public Color color = Color.white;
    private Color prevCol = Color.black;
    private float prevInten = 1.0f;
    public bool hasStarted = false;
    public bool shouldUpdateMeshColor = false;
    public bool shouldUpdateMatType = false;
    private bool wasShown = true;

    Camera cam;
    MeshRenderer mr;
    Light lightComponent;
    void doStart()
    {

        Color col = color;
        switch (type)
        {
        case RayTObjectType.camera:
            cam = gameObject.GetComponent<Camera>();
            if (cam != null)
            {
                col = cam.backgroundColor;
                prevCol = col;
                RT_SetBackgroundColor(RaytracerManager.Instance.URT, new Vector3(col.r, col.g, col.b));


                Matrix4x4 mat = cam.projectionMatrix;

                float fov = getFOV(mat);

                RT_SetCameraFOV(RaytracerManager.Instance.URT, fov);
            }
            else
            {
                prevCol = Color.black;
                RT_SetBackgroundColor(RaytracerManager.Instance.URT, Vector3.zero);
            }

            break;
        case RayTObjectType.light:

            float intensity = 1.0f;
            int lightType = 0;
            Light lightComponent = GetComponent<Light>();
            if (lightComponent != null)
            {
                col = lightComponent.color;
                prevCol = col;
                intensity = lightComponent.intensity;
                prevInten = intensity;
                lightType = getLightType(lightComponent);//Only directional and point light for now
                currentLightType = lightType;

                Vector3 dir = transform.forward;
                dir.x *= -1;
                Vector3 pos = transform.position;

                idObject = RT_AddLight(RaytracerManager.Instance.URT, lightType, dir, pos);
                RT_SetLightColor(RaytracerManager.Instance.URT, idObject, col.r, col.g, col.b, 1.0f);
                RT_SetLightIntensity(RaytracerManager.Instance.URT, idObject, intensity * 2.0f);
            }

            break;
        case RayTObjectType.mesh:

            MeshFilter mf = gameObject.GetComponent<MeshFilter>();
            if (gameObject.GetComponent<MeshRenderer>() != null)
                wasShown = gameObject.GetComponent<MeshRenderer>().enabled;
            else
                wasShown = true;

            Mesh mesh = mf.sharedMesh;

            if (!mesh.isReadable)
            {
                Debug.LogError("Mesh is not readable, please fix the import settings");
                this.enabled = false;
                return;
            }
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            Color[] colors = mesh.colors;

            int[] tris = mesh.triangles;
            float[] cols = new float[4 * vertices.Length];
            float[] uvs = new float[2 * vertices.Length];

            if (mesh.vertexCount == 0 || tris.Length / 3 == 0) {
                break;
            }

            Vector3 invX = new Vector3(-1, 1, 1);
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = Vector3.Scale(vertices[i], invX);
            }


            if (colors == null || colors.Length != vertices.Length)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    cols[i * 4 + 0] = 1.0f;
                    cols[i * 4 + 1] = 1.0f;
                    cols[i * 4 + 2] = 1.0f;
                    cols[i * 4 + 3] = 1.0f;
                }
            }
            else
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    cols[i * 4 + 0] = colors[i].r;
                    cols[i * 4 + 1] = colors[i].g;
                    cols[i * 4 + 2] = colors[i].b;
                    cols[i * 4 + 3] = 1.0f;
                }
            }
            if (normals == null || normals.Length != vertices.Length)
            {
                Debug.LogWarning("No normals for " + gameObject.name);
            }
            else
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    normals[i] = Vector3.Scale(normals[i], invX);
                }
            }

            if (mesh.uv == null || mesh.uv.Length == 0)
            {

                for (int i = 0; i < vertices.Length; i++)
                {
                    uvs[i * 2 + 0] = i / (float)vertices.Length;
                    uvs[i * 2 + 1] = i / (float)vertices.Length;
                }
            }
            else
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    uvs[i * 2 + 0] = mesh.uv[i].x;
                    uvs[i * 2 + 1] = mesh.uv[i].y;
                }
            }


            idObject = RT_AddMesh(RaytracerManager.Instance.URT,
                                  vertices.Length, tris.Length / 3,
                                  normals != null, true, vertices,
                                  tris, normals, uvs);
            RT_SetMeshVertexColor(RaytracerManager.Instance.URT, idObject, cols, vertices.Length);


            // int w = 0;
            // int h = 0;
            // Color[] data = getTextureData("Images/texturesOSPRay/Sand_normal", ref w, ref h);
            // Color[] data2 = getTextureData("Images/texturesOSPRay/Sand_basecolor", ref w, ref h);

            // int idTex = RT_AddTexture(RaytracerManager.Instance.URT, data, w, h);
            // int idTex2 = RT_AddTexture(RaytracerManager.Instance.URT, data2, w, h);

            // // RaytracerManager.Instance.setEnvMap(idTex);
            // RT_SetMeshNormalMap(RaytracerManager.Instance.URT, idObject, idTex);
            // RT_SetMeshAlbedoMap(RaytracerManager.Instance.URT, idObject, idTex2);

            if (!wasShown)
                RT_ShowHideMesh(RaytracerManager.Instance.URT, idObject, false);

            break;

        case RayTObjectType.spheres:
            List<Vector3> positions = new List<Vector3>();
            List<float> radii = new List<float>();
            List<Vector3> scolors = new List<Vector3>();
            foreach (Transform t in transform)
            {
                Vector3 p = t.position;
                p.x *= -1;
                positions.Add(p);
                radii.Add(t.lossyScale.x * 0.5f);
                scolors.Add(Vector3.one);
            }
            idObject = RT_AddSpheres(RaytracerManager.Instance.URT, positions.Count, positions.ToArray(), radii.ToArray(), scolors.ToArray());
            break;

        }
        transform.hasChanged = true;
        hasStarted = true;
    }

    void Start() {

        // FindObjectOfType(RaytracedObject).rtMat = ReadOSPRayMaterialJson.readRTMatJson("/Users/martinez/Downloads/OSPRayMatJson/silver_metal.json");
        // ReadOSPRayMaterialJson.readRTMatJson("/Users/martinez/Downloads/OSPRayMatJson/carpaint_1.json");

    }
    void Update()
    {

        if (!UnityMolMain.raytracingMode)
            return;

        if (RaytracerManager.Instance == null || RaytracerManager.Instance.URT == IntPtr.Zero || !UnityPathTracerIsInit(RaytracerManager.Instance.URT))
        {   //Wait for URT to be initialized
            return;
        }
        if (!RaytracerManager.Instance.allOK)
            return;
        if (!hasStarted)
        {
            doStart();
        }
        if (type != RayTObjectType.camera && idObject < 0)
            return;

        if (gameObject.activeInHierarchy) {
            if (mr == null)
                mr = GetComponent<MeshRenderer>();
            if (lightComponent == null)
                lightComponent = GetComponent<Light>();

            switch (type)
            {
            case RayTObjectType.camera:
                if (cam != null) {
                    if (prevCol != cam.backgroundColor) {
                        prevCol = cam.backgroundColor;
                        RT_SetBackgroundColor(RaytracerManager.Instance.URT, new Vector3(prevCol.r, prevCol.g, prevCol.b));
                    }
                }
                break;
            case RayTObjectType.light:
                if (lightComponent != null) {
                    if (wasShown != lightComponent.enabled) {
                        RT_ShowHideLight(RaytracerManager.Instance.URT, idObject, lightComponent.enabled);
                        wasShown = lightComponent.enabled;
                    }
                    if (prevCol != lightComponent.color) {
                        prevCol = lightComponent.color;
                        RT_SetLightColor(RaytracerManager.Instance.URT, idObject, prevCol.r, prevCol.g, prevCol.b, 1.0f);
                    }
                    if (prevInten != lightComponent.intensity) {
                        prevInten = lightComponent.intensity;
                        RT_SetLightIntensity(RaytracerManager.Instance.URT, idObject, prevInten * 2.0f);
                    }
                }
                break;

            // case RayTObjectType.sphere:
            //     if (mr != null) {
            //         ShowHideSphere(idObject, mr.enabled);
            //     }
            //     break;

            case RayTObjectType.mesh:
                if (mr != null) {
                    if (wasShown != mr.enabled)
                    {
                        RT_ShowHideMesh(RaytracerManager.Instance.URT, idObject, mr.enabled);
                        if (mr.enabled)
                            transform.hasChanged = true;
                    }
                    wasShown = mr.enabled;
                }
                if (shouldUpdateMatType) {
                    updateMaterialType();
                    shouldUpdateMatType = false;
                }
                if (shouldUpdateMeshColor) {
                    updateMeshColors();
                    shouldUpdateMeshColor = false;
                }
                if (rtMat.propertyChanged) {
                    updateMaterial();
                }
                break;
            }
        }

        if (transform.hasChanged) {
            Matrix4x4 m = transform.localToWorldMatrix;

            Color col = color;
            MeshRenderer mr = GetComponent<MeshRenderer>();
            //Light lightComponent = GetComponent<Light>();

            switch (type)
            {
            case RayTObjectType.camera:
                RT_SetCameraTransform(RaytracerManager.Instance.URT, matrixToFloatArray(m));
                break;
            case RayTObjectType.light:

                // float intensity = 3.0f;
                // int lightType = 0;//Only directional light for now
                // if (lightComponent != null) {
                //     col = lightComponent.color;
                //     intensity = lightComponent.intensity;
                //     lightType = getLightType(lightComponent);
                // }
                Vector3 dir = transform.forward;
                dir.x *= -1;
                Vector3 pos = transform.position;

                RT_SetLightPosition(RaytracerManager.Instance.URT, idObject, pos);
                RT_SetLightDirection(RaytracerManager.Instance.URT, idObject, dir);
                // RT_SetLightColor(RaytracerManager.Instance.URT, idObject, col.r, col.g, col.b, 1.0f);
                // RT_SetLightIntensity(RaytracerManager.Instance.URT, idObject, intensity);
                // if (lightType != currentLightType) {
                //     RT_SetLightType(RaytracerManager.Instance.URT, idObject, lightType);
                //     currentLightType = lightType;
                // }
                break;
            // case RayTObjectType.sphere:
            //     if (mr != null) {
            //         col = mr.sharedMaterial.color;
            //     }

            //     SetSpherePositionRad(idObject, -transform.position.x, transform.position.y, transform.position.z, transform.localScale.x / 2);
            //     SetSphereColor(idObject, col.r, col.g, col.b);
            //     break;
            case RayTObjectType.mesh:

                RT_SetMeshTransform(RaytracerManager.Instance.URT, idObject, matrixToFloatArray(m));

                // if (mr != null) {
                //     if (mr.sharedMaterial.HasProperty("_Color")) {
                //         col = mr.sharedMaterial.color;
                //     }
                //     if (mr.sharedMaterial.HasProperty("_Glossiness")) {
                //         roughness = mr.sharedMaterial.GetFloat("_Glossiness");
                //         roughness = convertRoughness(roughness);
                //     }
                // }
                // //RT_SetMeshMaterial(RaytracerManager.Instance.URT, ....);

                break;
            case RayTObjectType.spheres:
                RT_SetSpheresTransform(RaytracerManager.Instance.URT, idObject, matrixToFloatArray(m));
                break;
            }
            transform.hasChanged = false;
        }
    }

    //For hyperball
    public void showHide(bool s) {
        switch (type) {
        case RayTObjectType.camera:
            break;
        case RayTObjectType.light:
            RT_ShowHideLight(RaytracerManager.Instance.URT, idObject, s);
            break;
        case RayTObjectType.mesh:
            RT_ShowHideMesh(RaytracerManager.Instance.URT, idObject, s);
            break;
        }
        wasShown = s;
    }

    public void updateMeshColors()
    {
        if (type == RayTObjectType.mesh)
        {
            MeshFilter mf = gameObject.GetComponent<MeshFilter>();
            Mesh mesh = mf.sharedMesh;

            Color[] colors = mesh.colors;
            float[] cols = new float[4 * colors.Length];

            for (int i = 0; i < colors.Length; i++)
            {
                cols[i * 4 + 0] = colors[i].r;
                cols[i * 4 + 1] = colors[i].g;
                cols[i * 4 + 2] = colors[i].b;
                cols[i * 4 + 3] = 1.0f;
            }
            RT_SetMeshVertexColor(RaytracerManager.Instance.URT, idObject, cols, colors.Length);
        }
    }

    public void changeMaterialType(int matType)
    {
        if (type != RayTObjectType.mesh)
            return;
        if (matType == 0)
        {
            rtMat = new RaytracingPrincipledMaterial();
        }
        else if (matType == 1)
        {
            rtMat = new RaytracingCarPaintMaterial();
        }
        else if (matType == 2)
        {
            rtMat = new RaytracingMetalMaterial();
        }
        else if (matType == 3)
        {
            rtMat = new RaytracingAlloyMaterial();
        }
        else if (matType == 4)
        {
            rtMat = new RaytracingGlassMaterial();
        }
        else if (matType == 5)
        {
            rtMat = new RaytracingThinGlassMaterial();
        }
        else if (matType == 6)
        {
            rtMat = new RaytracingMetallicPaintMaterial();
        }
        else if (matType == 7)
        {
            rtMat = new RaytracingLuminousMaterial();
        }
        else {
            Debug.LogError("Unknown type");
            return;
        }
        if (onNewRTMaterial != null) {
            onNewRTMaterial(new NewRTMatEventArgs(rtMat));
        }
    }
    private void updateMaterialType()
    {
        if (type != RayTObjectType.mesh)
            return;

        if (rtMat.GetType() == typeof(RaytracingPrincipledMaterial))
        {
            RT_ChangeMaterialType(RaytracerManager.Instance.URT, idObject, "principled");
        }
        else if (rtMat.GetType() == typeof(RaytracingCarPaintMaterial))
        {
            RT_ChangeMaterialType(RaytracerManager.Instance.URT, idObject, "carPaint");
        }
        else if (rtMat.GetType() == typeof(RaytracingMetalMaterial))
        {
            RT_ChangeMaterialType(RaytracerManager.Instance.URT, idObject, "metal");
        }
        else if (rtMat.GetType() == typeof(RaytracingAlloyMaterial))
        {
            RT_ChangeMaterialType(RaytracerManager.Instance.URT, idObject, "alloy");
        }
        else if (rtMat.GetType() == typeof(RaytracingGlassMaterial))
        {
            RT_ChangeMaterialType(RaytracerManager.Instance.URT, idObject, "glass");
        }
        else if (rtMat.GetType() == typeof(RaytracingThinGlassMaterial))
        {
            RT_ChangeMaterialType(RaytracerManager.Instance.URT, idObject, "thinGlass");
        }
        else if (rtMat.GetType() == typeof(RaytracingMetallicPaintMaterial))
        {
            RT_ChangeMaterialType(RaytracerManager.Instance.URT, idObject, "metallicPaint");
        }
        else if (rtMat.GetType() == typeof(RaytracingLuminousMaterial))
        {
            RT_ChangeMaterialType(RaytracerManager.Instance.URT, idObject, "luminous");
        }
        else {
            Debug.LogError("Unknown type");
            return;
        }
        updateMaterial();
        if (onNewRTMaterial != null) {
            onNewRTMaterial(new NewRTMatEventArgs(rtMat));
        }
    }

    void OnDisable()
    {
        if (!hasStarted)
            return;

        if (RaytracerManager.Instance == null || !RaytracerManager.Instance.allOK)
            return;

        // if (UnityMolMain.raytracingMode) {
        switch (type) {

        case RayTObjectType.camera:
            break;

        case RayTObjectType.light:
            RT_ShowHideLight(RaytracerManager.Instance.URT, idObject, false);
            break;

        // case RayTObjectType.sphere:
        //     ShowHideSphere(idObject, gameObject.activeInHierarchy);
        //     break;

        case RayTObjectType.mesh:
            RT_ShowHideMesh(RaytracerManager.Instance.URT, idObject, false);
            break;
        }
        // }
    }

    void OnEnable()
    {
        if (!UnityMolMain.raytracingMode)
            return;
        if (RaytracerManager.Instance == null || !RaytracerManager.Instance.allOK) {
            Invoke("OnEnable", 0.5f);
            return;
        }
        if (!hasStarted) {
            Invoke("OnEnable", 0.5f);
            return;
        }

        if (UnityMolMain.raytracingMode && RaytracerManager.Instance != null && RaytracerManager.Instance.allOK)
        {
            switch (type)
            {

            case RayTObjectType.camera:
                break;

            case RayTObjectType.light:
                RT_ShowHideLight(RaytracerManager.Instance.URT, idObject, gameObject.activeInHierarchy);
                break;

            // case RayTObjectType.sphere:
            //     ShowHideSphere(idObject, gameObject.activeInHierarchy);
            //     break;

            case RayTObjectType.mesh:
                RT_ShowHideMesh(RaytracerManager.Instance.URT, idObject, gameObject.activeInHierarchy);
                break;
            }
        }
    }

    private int getLightType(Light l)
    {
        if (l.type == LightType.Directional) {
            return 0;
        }
        if (l.type == LightType.Point) {
            return 1;
        }
        Debug.LogWarning("Directional and point light only for now");

        return 0;//Directional by default
    }

    void updateMaterial()
    {
        //Loop over all the material variables to update them
        Type t = rtMat.GetType();
        var propertyValues = t.GetProperties();
        foreach (var p in propertyValues)
        {
            if (p.PropertyType == typeof(Vector3))
            {   //Vector3
                RT_SetMeshMaterialPropertyFloat3(RaytracerManager.Instance.URT, idObject, p.Name, (Vector3)p.GetValue(rtMat));
            }
            else if (p.PropertyType == typeof(float))
            {   //float
                RT_SetMeshMaterialPropertyFloat(RaytracerManager.Instance.URT, idObject, p.Name, (float)p.GetValue(rtMat));
            }
            else if (p.PropertyType == typeof(bool) && p.Name != "propertyChanged")
            {   //bool
                RT_SetMeshMaterialPropertyBool(RaytracerManager.Instance.URT, idObject, p.Name, (bool)p.GetValue(rtMat));
            }
        }

        rtMat.propertyChanged = false;
    }

    public static float[] matrixToFloatArray(Matrix4x4 m)
    {
        float[] res = new float[16];
        res[0] = m.m00; res[1] = m.m01; res[2] = m.m02; res[3] = m.m03;
        res[4] = m.m10; res[5] = m.m11; res[6] = m.m12; res[7] = m.m13;
        res[8] = m.m20; res[9] = m.m21; res[10] = m.m22; res[11] = m.m23;
        res[12] = m.m30; res[13] = m.m31; res[14] = m.m32; res[15] = m.m33;

        return res;
    }

    public void setAperture(float a)
    {
        RT_SetCameraAperture(RaytracerManager.Instance.URT, a);
    }

    public void setFDist(float fd)
    {
        RT_SetCameraFocalDistance(RaytracerManager.Instance.URT, fd);
    }

    Color[] getTextureData(string path, ref int w, ref int h) {
        Texture2D envMap = Resources.Load(path) as Texture2D;
        RenderTexture tempRt = RenderTexture.GetTemporary(envMap.width,
                               envMap.height,
                               0,
                               RenderTextureFormat.ARGBHalf);
        Material getTexMat = new Material(Shader.Find("Custom/TargetShader"));
        RenderTexture.active = tempRt;
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, envMap.width, envMap.height, 0);

        getTexMat.SetTexture("target", envMap);
        Graphics.DrawTexture(
            new Rect(0, 0, envMap.width, envMap.height),
            envMap, getTexMat);
        GL.PopMatrix();
        Texture2D renvmap = new Texture2D(envMap.width,
                                          envMap.height,
                                          TextureFormat.RGBAHalf, false);
        renvmap.ReadPixels(new Rect(0, 0, envMap.width,
                                    envMap.height), 0, 0, false);
        renvmap.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(tempRt);
        GameObject.Destroy(getTexMat);

        w = renvmap.width;
        h = renvmap.height;
        return renvmap.GetPixels();
    }

    public float getFOV(Matrix4x4 mat) {
        float a = mat[0];
        float b = mat[5];
        float c = mat[10];
        float d = mat[14];

        float aspect_ratio = b / a;

        float k = (c - 1.0f) / (c + 1.0f);
        float clip_min = (d * (1.0f - k)) / (2.0f * k);
        float clip_max = k * clip_min;

        float RAD2DEG = 180.0f / 3.14159265358979323846f;
        return RAD2DEG * (2.0f * (float)Mathf.Atan(1.0f / b));
    }
    public void setfov(float v) {
        RT_SetCameraFOV(RaytracerManager.Instance.URT, v);
    }

    public int RTMatToType() {
        if (rtMat.GetType() == typeof(RaytracingPrincipledMaterial))
        {
            return 0;
        }
        else if (rtMat.GetType() == typeof(RaytracingCarPaintMaterial))
        {
            return 1;
        }
        else if (rtMat.GetType() == typeof(RaytracingMetalMaterial))
        {
            return 2;
        }
        else if (rtMat.GetType() == typeof(RaytracingAlloyMaterial))
        {
            return 3;
        }
        else if (rtMat.GetType() == typeof(RaytracingGlassMaterial))
        {
            return 4;
        }
        else if (rtMat.GetType() == typeof(RaytracingThinGlassMaterial))
        {
            return 5;
        }
        else if (rtMat.GetType() == typeof(RaytracingMetallicPaintMaterial))
        {
            return 6;
        }
        else if (rtMat.GetType() == typeof(RaytracingLuminousMaterial))
        {
            return 7;
        }
        return -1;
    }

}
}
