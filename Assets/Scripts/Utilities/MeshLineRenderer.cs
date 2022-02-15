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

/*
class Point {
    public Vector3 p;
    public Point next;
}
*/




[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class MeshLineRenderer : MonoBehaviour {

    private Mesh ml;

    private Vector3 s;
    
    public List<Vector3> positions = new List<Vector3>();

    private float lineSize = .005f;

    private bool firstQuad = true;

    void Start() {
        ml = GetComponent<MeshFilter>().mesh;
    }
    public void Init(){
        ml = GetComponent<MeshFilter>().mesh;
    }

    public void setWidth(float width) {
        lineSize = width;
    }

    public void AddPoint(Vector3 point) {
        if (s != Vector3.zero) {
            positions.Add(point);
            AddLine(ml, MakeQuad(s, point, lineSize, firstQuad));
            firstQuad = false;
        }

        s = point;
    }

    Vector3[] MakeQuad(Vector3 s, Vector3 e, float w, bool all) {
        w = w / 2;

        Vector3[] q;
        if (all) {
            q = new Vector3[4];
        } else {
            q = new Vector3[2];
        }

        Vector3 n = Vector3.Cross(s, e);
        Vector3 l = Vector3.Cross(n, e - s);
        l.Normalize();

        if (all) {
            q[0] = transform.InverseTransformPoint(s + l * w);
            q[1] = transform.InverseTransformPoint(s + l * -w);
            q[2] = transform.InverseTransformPoint(e + l * w);
            q[3] = transform.InverseTransformPoint(e + l * -w);
        } else {
            q[0] = transform.InverseTransformPoint(s + l * w);
            q[1] = transform.InverseTransformPoint(s + l * -w);
        }
        return q;
    }

    void AddLine(Mesh m, Vector3[] quad) {
        int vl = m.vertices.Length;

        Vector3[] vs = m.vertices;
        vs = resizeVertices(vs, 2 * quad.Length);

        for (int i = 0; i < 2 * quad.Length; i += 2) {
            vs[vl + i] = quad[i / 2];
            vs[vl + i + 1] = quad[i / 2];
        }

        Vector2[] uvs = m.uv;
        uvs = resizeUVs(uvs, 2 * quad.Length);

        if (quad.Length == 4) {
            uvs[vl] = Vector2.zero;
            uvs[vl + 1] = Vector2.zero;
            uvs[vl + 2] = Vector2.right;
            uvs[vl + 3] = Vector2.right;
            uvs[vl + 4] = Vector2.up;
            uvs[vl + 5] = Vector2.up;
            uvs[vl + 6] = Vector2.one;
            uvs[vl + 7] = Vector2.one;
        } else {
            if (vl % 8 == 0) {
                uvs[vl] = Vector2.zero;
                uvs[vl + 1] = Vector2.zero;
                uvs[vl + 2] = Vector2.right;
                uvs[vl + 3] = Vector2.right;

            } else {
                uvs[vl] = Vector2.up;
                uvs[vl + 1] = Vector2.up;
                uvs[vl + 2] = Vector2.one;
                uvs[vl + 3] = Vector2.one;
            }
        }

        int tl = m.triangles.Length;

        int[] ts = m.triangles;
        ts = resizeTriangles(ts, 12); // Joe was 12

        if (quad.Length == 2) {
            vl -= 4;
        }

        // front-facing quad
        ts[tl] = vl;
        ts[tl + 1] = vl + 2;
        ts[tl + 2] = vl + 4;

        ts[tl + 3] = vl + 2;
        ts[tl + 4] = vl + 6;
        ts[tl + 5] = vl + 4;

        // back-facing quad
        ts[tl + 6] = vl + 5;
        ts[tl + 7] = vl + 3;
        ts[tl + 8] = vl + 1;

        ts[tl + 9] = vl + 5;
        ts[tl + 10] = vl + 7;
        ts[tl + 11] = vl + 3;

        m.vertices = vs;
        m.uv = uvs;
        m.triangles = ts;
        m.RecalculateBounds();
        m.RecalculateNormals();
    }

    public Vector3[] getVertices(){
         return ml.vertices;
    }

    Vector3[] resizeVertices(Vector3[] ovs, int ns) {
        Vector3[] nvs = new Vector3[ovs.Length + ns];
        for (int i = 0; i < ovs.Length; i++) {
            nvs[i] = ovs[i];
        }

        return nvs;
    }

    Vector2[] resizeUVs(Vector2[] uvs, int ns) {
        Vector2[] nvs = new Vector2[uvs.Length + ns];
        for (int i = 0; i < uvs.Length; i++) {
            nvs[i] = uvs[i];
        }

        return nvs;
    }

    int[] resizeTriangles(int[] ovs, int ns) {
        int[] nvs = new int[ovs.Length + ns];
        for (int i = 0; i < ovs.Length; i++) {
            nvs[i] = ovs[i];
        }

        return nvs;
    }
}