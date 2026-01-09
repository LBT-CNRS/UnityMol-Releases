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
using System.Collections.Generic;
using UnityEngine;

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using Autodesk.Fbx;
#endif

namespace UMol {
public class FBXExporter {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

    public static void WriteMesh(List<GameObject> gos, string filePath, bool withAO = true) {

        List<MeshFilter> allMf = new();
        foreach (GameObject go in gos) {
            MeshFilter[] mfs = go.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter mf in mfs) {
                allMf.Add(mf);
            }
        }

        using(FbxManager fbxManager = FbxManager.Create ())
        {
            // configure IO settings.
            fbxManager.SetIOSettings (FbxIOSettings.Create (fbxManager, Globals.IOSROOT));
            // Export the scene
            using (FbxExporter exporter = FbxExporter.Create (fbxManager, "myExporter"))
            {
                // Initialize the exporter.
                bool status = exporter.Initialize (filePath, -1, fbxManager.GetIOSettings ());
                // Create a new scene to export
                FbxScene scene = FbxScene.Create (fbxManager, "UMolScene");

                // Get scene root node
                FbxNode rootNode = scene.GetRootNode();


                foreach (MeshFilter mf in allMf)
                {
                    // Create base node
                    FbxNode meshNode = FbxNode.Create(scene, "curMesh" + mf.gameObject.name);

                    Mesh unitymesh = mf.sharedMesh;
                    int[] triangles = unitymesh.triangles;
                    Vector3[] vertices = unitymesh.vertices;
                    Vector3[] normals = unitymesh.normals;
                    Color[] colors = unitymesh.colors;
                    int triangleCount = triangles.Length;
                    int normalsCount = (normals == null ? 0 : normals.Length);
                    int colorCount = (colors == null ? 0 : colors.Length);


                    FbxMesh mesh = FbxMesh.Create(scene, "Mesh_" + mf.gameObject.name);
                    meshNode.SetNodeAttribute(mesh);
                    rootNode.AddChild(meshNode);

                    FbxLayer fbxLayer = mesh.GetLayer (0 /* default layer */);
                    if (fbxLayer == null)
                    {
                        mesh.CreateLayer ();
                        fbxLayer = mesh.GetLayer (0 /* default layer */);
                    }

                    // Add vertices to the mesh list
                    mesh.InitControlPoints(unitymesh.vertexCount);
                    for (int i = 0; i < unitymesh.vertexCount; i++)
                    {
                        mesh.SetControlPointAt(new FbxVector4(vertices[i].x * -1, vertices[i].y, vertices[i].z), i);
                    }

                    // Add triangles
                    for (int i = 0; i + 2 < triangleCount; i = i + 3)
                    {
                        mesh.BeginPolygon();
                        mesh.AddPolygon(triangles[i + 0]);
                        mesh.AddPolygon(triangles[i + 2]);
                        mesh.AddPolygon(triangles[i + 1]);
                        mesh.EndPolygon();
                    }

                    // Add normals
                    if (normalsCount != 0)
                    {
                        FbxLayerElementNormal meshNormals = FbxLayerElementNormal.Create (mesh, "Normals");

                        meshNormals.SetMappingMode (FbxLayerElement.EMappingMode.eByControlPoint);
                        meshNormals.SetReferenceMode (FbxLayerElement.EReferenceMode.eDirect);

                        FbxLayerElementArray fbxElementArray = meshNormals.GetDirectArray ();
                        for (int i = 0; i < normalsCount; i++)
                        {
                            fbxElementArray.Add(new FbxVector4(normals[i].x * -1, normals[i].y, normals[i].z, 0.0f));
                        }
                        fbxLayer.SetNormals(meshNormals);
                    }


                    // Add vertex colors
                    if (colorCount != 0)
                    {
                        FbxLayerElementVertexColor meshColors = FbxLayerElementVertexColor.Create(mesh, "VertexColors");

                        meshColors.SetMappingMode (FbxLayerElement.EMappingMode.eByControlPoint);
                        meshColors.SetReferenceMode (FbxLayerElement.EReferenceMode.eDirect);

                        FbxLayerElementArray fbxElementArraycol = meshColors.GetDirectArray ();

                        bool addAO = false;
                        Renderer rd = mf.GetComponent<Renderer>();
                        if (withAO && rd != null) {
                            Material mat = rd.sharedMaterial;
                            if (mat.HasProperty("_AOIntensity") && mat.GetFloat("_AOIntensity") != 0.0f) {
                                addAO = true;
                                float inten = mat.GetFloat("_AOIntensity");

                                Color colme = Color.yellow;
                                for (int i = 0; i < vertices.Length; i++) {
                                    Color c = colors[i];
                                    c *= Mathf.Clamp( c.a * inten, 0.0f, 1.0f);
                                    fbxElementArraycol.Add(new FbxColor(c.r, c.g, c.b));
                                    if (i == 10)
                                        colme = c;
                                }
                                Debug.Log("Adding AO to colors ! " + colme);
                            }
                        }
                        if (!addAO) {
                            for (int i = 0; i < vertices.Length; i++) {
                                fbxElementArraycol.Add(new FbxColor(colors[i].r, colors[i].g, colors[i].b));
                            }
                        }
                        fbxLayer.SetVertexColors(meshColors);

                    }
                }


                exporter.Export(scene);
            }
        }
    }

#endif

}
}
