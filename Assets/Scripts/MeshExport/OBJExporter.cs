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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace UMol {

//Adapted from https://wiki.unity3d.com/index.php/ExportOBJ

public class ObjExporter  {

    private static int StartIndex = 0;
    //private static int groupCount = 0;
    private static CultureInfo culture = CultureInfo.InvariantCulture;

    public static string MeshToString(MeshFilter mf, Transform t, bool withAO)
    {
        Vector3 s       = t.localScale;
        Vector3 p       = t.localPosition;
        Quaternion r    = t.localRotation;


        int numVertices = 0;
        Mesh m = mf.sharedMesh;
        if (!m)
        {
            return "####Error####";
        }
        // Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

        StringBuilder sb = new StringBuilder();

        bool hasColor = true;
        Vector3[] verts = m.vertices;
        Color[] cols = m.colors;
        if (cols == null || cols.Length == 0) {
            cols = new Color[verts.Length];
            for (int i = 0; i < verts.Length; i++)
                cols[i] = Color.white;
            hasColor = false;
        }
        bool addAO = false;
        int id = 0;
        Renderer rd = mf.GetComponent<Renderer>();
        if (hasColor && withAO && rd != null) {
            Material mat = rd.sharedMaterial;
            if (mat.HasProperty("_AOIntensity") && mat.GetFloat("_AOIntensity") != 0.0f) {
                addAO = true;
                float inten = mat.GetFloat("_AOIntensity");

                foreach (Vector3 vv in m.vertices) {
                    Vector3 v = t.TransformPoint(vv);
                    numVertices++;
                    Color c = cols[id];

                    c *= Mathf.Clamp( c.a * inten, 0.0f, 1.0f);

                    sb.AppendFormat("v {0} {1} {2} {3} {4} {5}\n",
                                            (-v.x).ToString("F6", CultureInfo.InvariantCulture), v.y.ToString("F6", CultureInfo.InvariantCulture), v.z.ToString("F6", CultureInfo.InvariantCulture),
                                            c.r.ToString("F3", CultureInfo.InvariantCulture), c.g.ToString("F3", CultureInfo.InvariantCulture), c.b.ToString("F3", CultureInfo.InvariantCulture));
                    id++;
                }
            }
        }
        if (!addAO) {
            foreach (Vector3 vv in m.vertices) {
                Vector3 v = t.TransformPoint(vv);
                numVertices++;
                Color c = cols[id];
                sb.AppendFormat("v {0} {1} {2} {3} {4} {5}\n",
                                        (-v.x).ToString("F6", CultureInfo.InvariantCulture), v.y.ToString("F6", CultureInfo.InvariantCulture), v.z.ToString("F6", CultureInfo.InvariantCulture),
                                        c.r.ToString("F3", CultureInfo.InvariantCulture), c.g.ToString("F3", CultureInfo.InvariantCulture), c.b.ToString("F3", CultureInfo.InvariantCulture));
                id++;
            }
        }

        sb.Append("\n");
        foreach (Vector3 nn in m.normals)
        {
            Vector3 v = r * nn;
            sb.AppendFormat("vn {0} {1} {2}\n",
                                    (-v.x).ToString("F6", CultureInfo.InvariantCulture), v.y.ToString("F6", CultureInfo.InvariantCulture), v.z.ToString("F6", CultureInfo.InvariantCulture));
        }
        sb.Append("\n");
        // foreach (Vector3 v in m.uv)
        // {
        //     sb.Append(string.Format(CultureInfo.InvariantCulture,"vt {0} {1}\n", v.x, v.y));
        // }
        for (int material = 0; material < m.subMeshCount; material ++)
        {
            sb.Append("\n");
            // sb.Append("usemtl ").Append(mats[material].name).Append("\n");
            // sb.Append("usemap ").Append(mats[material].name).Append("\n");

            int[] triangles = m.GetTriangles(material);
            for (int i = 0; i < triangles.Length; i += 3) {
                if (triangles[i + 1] != triangles[i] && triangles[i] != triangles[i + 2] && triangles[i + 1] != triangles[i + 2])
                    sb.AppendFormat(CultureInfo.InvariantCulture, "f {0} {1} {2}\n",
                                            triangles[i + 1] + 1 + StartIndex, triangles[i] + 1 + StartIndex, triangles[i + 2] + 1 + StartIndex);
            }
        }

        StartIndex += numVertices;
        return sb.ToString();
    }

    public static string DoExport(List<GameObject> gos, bool makeSubmeshes, bool withAO = true) {
        if (gos.Count == 0) {
            return "";
        }

        StartIndex = 0;
        //groupCount = 0;

        StringBuilder meshString = new StringBuilder();

        meshString.Append("#From UnityMol\n#-------------\n\n");

        foreach (GameObject go in gos) {
            Transform t = go.transform;

            // Vector3 originalPosition = t.position;
            // t.position = Vector3.zero;


            meshString.Append("g ").Append(t.name).Append("\n");

            meshString.Append(processTransform(t, makeSubmeshes, withAO));

            // t.position = originalPosition;
        }


        StartIndex = 0;
        //groupCount = 0;

        return meshString.ToString();
    }

    static string processTransform(Transform t, bool makeSubmeshes, bool withAO)
    {
        StringBuilder meshString = new StringBuilder();

        meshString.Append("#" + t.name
                          + "\n#-------\n");

        // if (makeSubmeshes)
        // {
        //     meshString.Append("g ").Append(t.name).Append(groupCount).Append("\n");
        //     groupCount++;
        // }

        MeshFilter mf = t.GetComponent<MeshFilter>();
        if (mf)
        {
            meshString.Append(MeshToString(mf, t, withAO));
        }

        for (int i = 0; i < t.childCount; i++)
        {
            meshString.Append(processTransform(t.GetChild(i), makeSubmeshes, withAO));
        }

        return meshString.ToString();
    }
}
}