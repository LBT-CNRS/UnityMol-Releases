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
using System.Collections.Generic;
using System.Linq;
using System;

namespace UMol
{
public class SheherasadeRepresentation : AtomRepresentation
{

    public List<GameObject> meshesGO;
    public Dictionary<GameObject, List<Color32>> meshColors;
    public Dictionary<UnityMolResidue, GameObject> residueToGo;
    public Dictionary<UnityMolResidue, List<int>> residueToVert;

    public Color32[] colorByRes = null;

    private string structureName;
    private List<Segment> segments;
    private GameObject newRep;
    public Material mat;
    private bool useHET = false;

    public bool bezier = true;      // Use to compute Straight or smooth Sheherasade

    public SheherasadeRepresentation(int idF, string structName, UnityMolSelection sel, bool useHetatm = false) {
        colorationType = colorType.full;

        meshesGO = new List<GameObject>();
        meshColors = new Dictionary<GameObject, List<Color32>>();
        residueToGo = new Dictionary<UnityMolResidue, GameObject>();
        mat = new Material(Shader.Find("Custom/SurfaceVertexColor"));

        structureName = structName;
        selection = sel;
        useHET = useHetatm;
        idFrame = idF;

        representationParent = UnityMolMain.getRepStructureParent(structName).transform;
        
        newRep = new GameObject("AtomSheherasadeRepresentation");
        newRep.transform.parent = representationParent;
        representationTransform = newRep.transform;

        segments = CutSelectionInSegments(selection);

        DisplaySheherasadeMesh(structName, segments, newRep.transform);

        getMeshColors();

        newRep.transform.localPosition = Vector3.zero;
        newRep.transform.localRotation = Quaternion.identity;
        newRep.transform.localScale = Vector3.one;

        nbAtoms = selection.atoms.Count;
    }

    List<Segment> CutSelectionInSegments(UnityMolSelection sele)
    {
        List<Segment> res = new List<Segment>();

        Segment curSeg = new Segment(sele.atoms[0].residue);
        res.Add(curSeg);

        for (int i = 1; i < sele.atoms.Count; i++)
        {
            UnityMolAtom atom = sele.atoms[i];
            if (!useHET && atom.isHET)
            {
                continue;
            }

            curSeg.residues.Add(atom.residue);
        }

        res[0].residues = res[0].residues.Distinct().ToList();

        return res;
    }


    public void DisplaySheherasadeMesh(string structName, List<Segment> segments, Transform repParent, float ribbonWidth = 4.5f,
                                       float bRad = 0.3f, int bRes = 10, bool useBspline = true, bool isTraj = false) {

        residueToVert = new Dictionary<UnityMolResidue, List<int>>();
        for (int seg = 0; seg < segments.Count; seg++)
        {
            int nbRes = segments[seg].residues.Count;
            if (nbRes < 2)
            {
                continue;
            }
            List<Vector2> uv_Map = new List<Vector2>();
            MeshData mesh = Sheherasade.CreateChainMesh(selection, idFrame, segments[seg].residues, uv_Map, ref residueToVert, bezier);

            if (mesh.vertices != null)
            {
                string nameSheherasadeMesh = structName + "_" + segments[seg].residues[0].chain.name + "_SheherasadeMesh";
                CreateUnityMesh(segments[seg], repParent, nameSheherasadeMesh, mesh, uv_Map);
            }

        }
    }

    void CreateUnityMesh(Segment seg, Transform parent, string name,
                         Vector3[] vertices, Vector3[] normals, int[] triangles, Color32[] colors,
                         List<Vector2> uv_Map)
    {
        GameObject go = new GameObject(name);
        MeshFilter mf = go.AddComponent<MeshFilter>();
        Mesh m = new Mesh
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        m.vertices = vertices;
        m.triangles = triangles;
        m.colors32 = colors;
        m.uv = uv_Map.ToArray();

        if (normals.Count() != 0)
            m.normals = normals;
        else
            m.RecalculateNormals();

        mf.sharedMesh = m;
        go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        go.transform.parent = parent;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;

        meshesGO.Add(go);

        foreach (UnityMolResidue r in seg.residues) {
            if (!residueToGo.ContainsKey(r))
                residueToGo.Add(r, go);
        }

    }

    void CreateUnityMesh(Segment seg, Transform parent, string name,
                         MeshData meshD, List<Vector2> uv_Map)
    {
        CreateUnityMesh(seg, parent, name, meshD.vertices,
                        meshD.normals, meshD.triangles, meshD.colors, uv_Map);
    }

    private void getMeshColors() {
        meshColors.Clear();
        foreach (GameObject go in meshesGO) {
            Mesh m = go.GetComponent<MeshFilter>().sharedMesh;
            Color32[] tmpArrayColor = m.colors32;
            if (tmpArrayColor == null || tmpArrayColor.Length == 0) {
                tmpArrayColor = new Color32[m.vertexCount];
            }
            meshColors[go] = tmpArrayColor.ToList();
        }
    }

    private void getMeshColorsPerResidue() {
        //Warning: This function expects that getMeshColors() was called before!
        if (colorByRes == null) {
            colorByRes = new Color32[residueToGo.Keys.Count];
        }

        int idR = 0;
        foreach (UnityMolResidue r in residueToGo.Keys) {
            GameObject curGo = null;
            Color32 col = Color.white;
            List<int> listVertId;

            if (residueToGo.TryGetValue(r, out curGo)) {
                List<Color32> colors = meshColors[curGo];
                if (residueToVert.TryGetValue(r, out listVertId)) {
                    if (listVertId.Count != 0 && listVertId[0] >= 0 && listVertId[0] < colors.Count) {
                        col = colors[listVertId[0]];
                    }
                }
            }

            colorByRes[idR++] = col;
        }
    }
    private void restoreColorsPerResidue() {
        if (colorByRes != null && residueToGo != null) {

            getMeshColors();
            int idR = 0;

            foreach (UnityMolResidue r in residueToGo.Keys) {
                List<int> listVertId;
                GameObject curGo = null;
                Color32 col = colorByRes[idR++];

                if (residueToGo.TryGetValue(r, out curGo)) {
                    List<Color32> colors = meshColors[curGo];

                    if (residueToVert.TryGetValue(r, out listVertId)) {
                        foreach (int c in listVertId) {
                            if (c >= 0 && c < colors.Count) {
                                colors[c] = col;
                            }
                        }
                    }
                }
            }
            foreach (GameObject go in meshesGO) {
                go.GetComponent<MeshFilter>().sharedMesh.SetColors(meshColors[go]);
            }
        }
    }

    public void recompute(bool isNewModel = false)
    {
        List<Material> savedMat = new List<Material>();

        foreach (GameObject m in meshesGO) {
            savedMat.Add(m.GetComponent<MeshRenderer>().sharedMaterial);
        }

        getMeshColorsPerResidue();

        Clean();

        segments = CutSelectionInSegments(selection);
        if (isNewModel)
            DisplaySheherasadeMesh(structureName, segments, newRep.transform, isTraj: false);
        else
            DisplaySheherasadeMesh(structureName, segments, newRep.transform, isTraj: true);


        if (meshesGO.Count > 0 && meshesGO.Count == savedMat.Count) {
            int i = 0;
            foreach (GameObject m in meshesGO) {
                m.GetComponent<MeshRenderer>().sharedMaterial = savedMat[i++];
            }
        }

        restoreColorsPerResidue();
    }
    public override void Clean() {
        if (meshesGO != null) {
            for (int i = 0; i < meshesGO.Count; i++) {
                GameObject.Destroy(meshesGO[i].GetComponent<MeshFilter>().sharedMesh);
                GameObject.Destroy(meshesGO[i].GetComponent<MeshRenderer>().sharedMaterial);
                GameObject.Destroy(meshesGO[i]);
            }
            meshesGO.Clear();
        }
        if (residueToGo != null)
            residueToGo.Clear();
        if (residueToVert != null)
            residueToVert.Clear();

    }

}
}
