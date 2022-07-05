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


 
using System;
using System.Collections.Generic;
using UnityEngine;
 
public static class NormalSolver
{
    /// <summary>
    ///     Recalculate the normals of a mesh based on an angle threshold. This takes
    ///     into account distinct vertices that have the same position.
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="angle">
    ///     The smoothing angle. Note that triangles that already share
    ///     the same vertex will be smooth regardless of the angle! 
    /// </param>
    public static void RecalculateNormals(this Mesh mesh, float angle) {
        var cosineThreshold = Mathf.Cos(angle * Mathf.Deg2Rad);
 
        var vertices = mesh.vertices;
        var normals = new Vector3[vertices.Length];
 
        // Holds the normal of each triangle in each sub mesh.
        var triNormals = new Vector3[mesh.subMeshCount][];
 
        var dictionary = new Dictionary<VertexKey, List<VertexEntry>>(vertices.Length);
 
        for (var subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; ++subMeshIndex) {
            
            var triangles = mesh.GetTriangles(subMeshIndex);
 
            triNormals[subMeshIndex] = new Vector3[triangles.Length / 3];
 
            for (var i = 0; i < triangles.Length; i += 3) {
                int i1 = triangles[i];
                int i2 = triangles[i + 1];
                int i3 = triangles[i + 2];
 
                // Calculate the normal of the triangle
                Vector3 p1 = vertices[i2] - vertices[i1];
                Vector3 p2 = vertices[i3] - vertices[i1];
                Vector3 normal = Vector3.Cross(p1, p2).normalized;
                int triIndex = i / 3;
                triNormals[subMeshIndex][triIndex] = normal;
 
                List<VertexEntry> entry;
                VertexKey key;
 
                if (!dictionary.TryGetValue(key = new VertexKey(vertices[i1]), out entry)) {
                    entry = new List<VertexEntry>(4);
                    dictionary.Add(key, entry);
                }
                entry.Add(new VertexEntry(subMeshIndex, triIndex, i1));
 
                if (!dictionary.TryGetValue(key = new VertexKey(vertices[i2]), out entry)) {
                    entry = new List<VertexEntry>();
                    dictionary.Add(key, entry);
                }
                entry.Add(new VertexEntry(subMeshIndex, triIndex, i2));
 
                if (!dictionary.TryGetValue(key = new VertexKey(vertices[i3]), out entry)) {
                    entry = new List<VertexEntry>();
                    dictionary.Add(key, entry);
                }
                entry.Add(new VertexEntry(subMeshIndex, triIndex, i3));
            }
        }
 
        // Each entry in the dictionary represents a unique vertex position.
 
        foreach (var vertList in dictionary.Values) {
            for (var i = 0; i < vertList.Count; ++i) {
 
                var sum = new Vector3();
                var lhsEntry = vertList[i];
 
                for (var j = 0; j < vertList.Count; ++j) {
                    var rhsEntry = vertList[j];
 
                    if (lhsEntry.VertexIndex == rhsEntry.VertexIndex) {
                        sum += triNormals[rhsEntry.MeshIndex][rhsEntry.TriangleIndex];
                    } else {
                        // The dot product is the cosine of the angle between the two triangles.
                        // A larger cosine means a smaller angle.
                        var dot = Vector3.Dot(
                            triNormals[lhsEntry.MeshIndex][lhsEntry.TriangleIndex],
                            triNormals[rhsEntry.MeshIndex][rhsEntry.TriangleIndex]);
                        if (dot >= cosineThreshold) {
                            sum += triNormals[rhsEntry.MeshIndex][rhsEntry.TriangleIndex];
                        }
                    }
                }
 
                normals[lhsEntry.VertexIndex] = sum.normalized;
            }
        }
 
        mesh.normals = normals;
    }
 
    private struct VertexKey
    {
        private readonly long _x;
        private readonly long _y;
        private readonly long _z;
 
        // Change this if you require a different precision.
        private const int Tolerance = 100000;
 
        // Magic FNV values. Do not change these.
        private const long FNV32Init = 0x811c9dc5;
        private const long FNV32Prime = 0x01000193;
 
        public VertexKey(Vector3 position) {
            _x = (long)(Mathf.Round(position.x * Tolerance));
            _y = (long)(Mathf.Round(position.y * Tolerance));
            _z = (long)(Mathf.Round(position.z * Tolerance));
        }
 
        public override bool Equals(object obj) {
            var key = (VertexKey)obj;
            return _x == key._x && _y == key._y && _z == key._z;
        }
 
        public override int GetHashCode() {
            long rv = FNV32Init;
            rv ^= _x;
            rv *= FNV32Prime;
            rv ^= _y;
            rv *= FNV32Prime;
            rv ^= _z;
            rv *= FNV32Prime;
 
            return rv.GetHashCode();
        }
    }
 
    private struct VertexEntry {
        public int MeshIndex;
        public int TriangleIndex;
        public int VertexIndex;
 
        public VertexEntry(int meshIndex, int triIndex, int vertIndex) {
            MeshIndex = meshIndex;
            TriangleIndex = triIndex;
            VertexIndex = vertIndex;
        }
    }
}