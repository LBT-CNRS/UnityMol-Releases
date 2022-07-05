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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using UnityEngine.XR;
using VRTK;

namespace UMol {
public class CartoonRepresentation : AtomRepresentation {

    public List<GameObject> meshesGO;
    public Dictionary<UnityMolResidue, GameObject> residueToGo;
    public List<UnityMolAtom> savedAtoms = new List<UnityMolAtom>();//Used to Save/Restore cartoon colors

    public Dictionary<UnityMolResidue, List<int>> residueToVert;

    public int totalVertices = 0;

    public Dictionary<GameObject, List<Color32>> meshColors;
    public List<Color32[]> savedColors = new List<Color32[]>();
    public Color32[] colorByRes = null;

    private List<List<UnityMolResidue>> residuesPerChain;

    private string structureName;
    private List<Segment> segments;
    private GameObject newRep;
    private Material ribbonMat;
    private bool useHET = false;
    private bool useWAT = false;
    private bool isFullMesh = false;



    public CartoonRepresentation(string structName, UnityMolSelection sel, bool useHetatm = false, bool useWat = false) {

        colorationType = colorType.defaultCartoon;
        meshesGO = new List<GameObject>();
        residueToGo = new Dictionary<UnityMolResidue, GameObject>();
        meshColors = new Dictionary<GameObject, List<Color32>>();

        ribbonMat = new Material(Shader.Find("Custom/SurfaceVertexColor"));

        structureName = structName;
        selection = sel;
        useHET = useHetatm;
        useWAT = useWat;

        GameObject loadedMolGO = UnityMolMain.getRepresentationParent();

        representationParent = loadedMolGO.transform.Find(structName);
        if (UnityMolMain.inVR() && representationParent == null) {

            Transform clref = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController);
            Transform crref = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController);
            if (clref != null) {
                representationParent = clref.Find(structName);
            }
            if (representationParent == null && crref != null) {
                representationParent = crref.Find(structName);
            }
        }
        if (representationParent == null) {
            representationParent = (new GameObject(structName).transform);
            representationParent.parent = loadedMolGO.transform;
            representationParent.localPosition = Vector3.zero;
            representationParent.localRotation = Quaternion.identity;
            representationParent.localScale = Vector3.one;
        }

        newRep = new GameObject("AtomCartoonRepresentation");
        newRep.transform.parent = representationParent;
        representationTransform = newRep.transform;


        if (!selection.sameModel()) {

            segments = cutSelectionInSegments(selection);

            displayCartoonMesh(structName, segments, newRep.transform);

        }
        else {


            UnityMolSelection biggerSel = getAllStructureSelection(selection);

            segments = cutSelectionInSegments(biggerSel);

            displayCartoonMesh(structName, segments, newRep.transform);

            if (biggerSel.Count != selection.Count) {
                // Remove mesh parts not in selection
                removeMeshParts(biggerSel);
            }
        }

        getMeshColors();

        newRep.transform.localPosition = Vector3.zero;
        newRep.transform.localRotation = Quaternion.identity;
        newRep.transform.localScale = Vector3.one;

        nbAtoms = selection.Count;

        savedAtoms.Clear();
        foreach (UnityMolAtom a in selection.atoms) {
            savedAtoms.Add(a);
        }
    }

    UnityMolSelection getAllStructureSelection(UnityMolSelection oriSel) {
        if (oriSel.structures.Count == 1) {
            return oriSel.structures[0].ToSelection();
        }
        List<UnityMolAtom> atoms = new List<UnityMolAtom>();

        foreach (UnityMolStructure s in oriSel.structures) {
            atoms.AddRange(s.currentModel.allAtoms);
        }

        UnityMolSelection res = new UnityMolSelection(atoms, null, "allStructCartoonSel", "");
        return res;
    }

    List<Segment> cutSelectionInSegments(UnityMolSelection sele) {
        List<Segment> res = new List<Segment>();
        Dictionary<string, Segment> segDict = new Dictionary<string, Segment>();

        for (int i = 0; i < sele.atoms.Count; i++) {
            UnityMolAtom atom = sele.atoms[i];
            if (!useHET && atom.isHET) {
                continue;
            }
            if (!useWAT && WaterSelection.waterResidues.Contains(atom.residue.name, StringComparer.OrdinalIgnoreCase)) {
                continue;
            }
            string segKey = atom.residue.chain.model.structure.name + "_" +
                            atom.residue.chain.model.name + "_" +
                            atom.residue.chain.name;
            Segment curSeg;
            if (segDict.TryGetValue(segKey, out curSeg)) {
                curSeg.residues.Add(atom.residue);
            }
            else {
                curSeg = new Segment(atom.residue);
                res.Add(curSeg);
                segDict[segKey] = curSeg;
            }
        }
        for (int seg = 0; seg < res.Count; seg++) {
            res[seg].residues = res[seg].residues.Distinct().ToList();
        }

        return res;
    }

    private void removeMeshParts(UnityMolSelection bigSel) {

        if (isFullMesh) {
            return;
        }
        Dictionary<GameObject, HashSet<int>> vertIdToDel = new Dictionary<GameObject, HashSet<int>>();
        HashSet<UnityMolResidue> oriRes = new HashSet<UnityMolResidue>();

        foreach (UnityMolAtom a in selection.atoms) {
            oriRes.Add(a.residue);
        }
        HashSet<UnityMolResidue> residuesToDel = new HashSet<UnityMolResidue>();

        //Get all the residues to delete
        foreach (UnityMolAtom a in bigSel.atoms) {
            if (!useWAT && WaterSelection.waterResidues.Contains(a.residue.name, StringComparer.OrdinalIgnoreCase)) {
                continue;
            }
            if (!useHET && a.isHET) {
                continue;
            }
            if (!oriRes.Contains(a.residue)) {
                residuesToDel.Add(a.residue);
            }
        }
        if (residuesToDel.Count == 0) {
            isFullMesh = true;
            return;
        }


        int totalToDel = 0;
        //For each residue to delete, record the vertices to hide
        foreach (UnityMolResidue r in residuesToDel) {
            List<int> listVertId;
            GameObject curGo = null;

            if (residueToGo.TryGetValue(r, out curGo) && residueToVert.TryGetValue(r, out listVertId)) {
                if (!vertIdToDel.ContainsKey(curGo)) {
                    vertIdToDel[curGo] = new HashSet<int>();
                }
                foreach (int c in listVertId) {
                    vertIdToDel[curGo].Add(c);
                    totalToDel++;
                }
            }
        }

        if (totalToDel == 0) {
            isFullMesh = true;
            return;
        }


        //Hide the triangles that contains at least one of the vertex to hide
        foreach (GameObject go in vertIdToDel.Keys) {

            Mesh m = go.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] vertices = m.vertices;
            int[] triangles = m.triangles;
            List<int> newTri = new List<int>(triangles.Length);

            for (int t = 0; t < triangles.Length - 3; t += 3) {
                if (vertIdToDel[go].Contains(triangles[t])) {
                    continue;
                }
                if (vertIdToDel[go].Contains(triangles[t + 1])) {
                    continue;
                }
                if (vertIdToDel[go].Contains(triangles[t + 2])) {
                    continue;
                }
                newTri.Add(triangles[t]);
                newTri.Add(triangles[t + 1]);
                newTri.Add(triangles[t + 2]);
            }

            m.triangles = newTri.ToArray();
        }

    }



    public void displayCartoonMesh(string structName, List<Segment> segments, Transform repParent, float ribbonWidth = 4.5f,
                                   float bRad = 0.3f, int bRes = 10, bool useBspline = true, bool isTraj = false)
    {


        // float start = Time.realtimeSinceStartup;
        savedColors = new List<Color32[]>();
        residueToVert = new Dictionary<UnityMolResidue, List<int>>();

        for (int seg = 0; seg < segments.Count; seg++)
        {

            Dictionary<UnityMolResidue, List<int>> segResidueToVert = new Dictionary<UnityMolResidue, List<int>>();
            int nbRes = segments[seg].residues.Count;
            if (nbRes < 2)
            {
                continue;
            }

            bool isProt = isSegmentProt(segments[seg].residues);
            bool isNucleic = isSegmentNucleic(segments[seg].residues);

            MeshData mesh = null;
            string nameCartoonMesh = null;
            UnityMolResidue r = segments[seg].residues[0];
            string keyPrecomputedRep = r.chain.model.structure.uniqueName + "_" + r.chain.model.name + "_" + r.chain.name + "_Cartoon";

            if (UnityMolMain.getPrecompRepManager().ContainsRep(keyPrecomputedRep)) {

                mesh = UnityMolMain.getPrecompRepManager().precomputedRep[keyPrecomputedRep];
                Dictionary<UnityMolResidue, List<int>> tmpResVert = UnityMolMain.getPrecompRepManager().precomputedCartoonAsso[keyPrecomputedRep];
                foreach (UnityMolResidue res in tmpResVert.Keys) {
                    segResidueToVert[res] = tmpResVert[res];
                }
            }
            else {
                if (isNucleic) {

                    mesh = RibbonMeshDNA.createChainMesh(segments[seg].residues, ref segResidueToVert, isTraj);

                }
                else {//Try with standard cartoon

                    mesh = RibbonMesh.createChainMesh(segments[seg].residues, ref segResidueToVert, isTraj);
                    // MeshData mesh = CartoonMesh.createChainMesh(segments[seg].residues, ref residueToMesh, isTraj);

                }

                if (mesh != null && mesh.vertices != null) {
                    UnityMolMain.getPrecompRepManager().precomputedRep[keyPrecomputedRep] = mesh;
                    UnityMolMain.getPrecompRepManager().precomputedCartoonAsso[keyPrecomputedRep] = segResidueToVert;
                }
            }
            if (mesh != null && mesh.vertices != null && mesh.vertices.Length > 3 && !float.IsNaN(mesh.vertices[0].x)) {
                nameCartoonMesh = structName + "_" + segments[seg].residues[0].chain.name + "_CartoonMesh";
                createUnityMesh(segments[seg], repParent, nameCartoonMesh, mesh, ribbonMat);
                totalVertices += mesh.vertices.Length;
            }

            foreach (UnityMolResidue res in segResidueToVert.Keys) {
                residueToVert[res] = segResidueToVert[res];
            }

        }
        // Debug.Log("Time for cartoon: " + (1000.0f * (Time.realtimeSinceStartup - start)).ToString("f3") + " ms");
    }

    bool isSegmentProt(List<UnityMolResidue> residues) {
        int cptProt = 0;
        foreach (UnityMolResidue r in residues) {
            if (MDAnalysisSelection.isProtein(r)) {
                cptProt++;
            }
            if (cptProt >= residues.Count / 3) {
                return true;
            }
        }
        return false;
    }

    bool isSegmentNucleic(List<UnityMolResidue> residues) {
        int cptNucl = 0;
        foreach (UnityMolResidue r in residues) {
            if (MDAnalysisSelection.isNucleic(r)) {
                cptNucl++;
            }
            if (cptNucl >= Mathf.Min(10, residues.Count)) {
                return true;
            }
        }
        return false;
    }


    void createUnityMesh(Segment seg, Transform parent, string name,
                         Vector3[] vertices, Vector3[] normals, int[] triangles, Color32[] colors, Material ribbonMat = null)
    {

        //Resources.Load("Materials/standardColorSpecular") as Material;
        if (ribbonMat == null)
            ribbonMat = new Material(Shader.Find("Custom/SurfaceVertexColor"));
        Mesh m = new Mesh();
        m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        m.vertices = vertices;

        m.triangles = triangles;
        m.colors32 = colors;

        savedColors.Add(colors);

        if (normals != null)
            m.normals = normals;


        GameObject go = new GameObject(name);
        MeshFilter mf = go.AddComponent<MeshFilter>();
        if (normals == null) {
            m.RecalculateNormals();
            //High quality smoothing normals
            // m.RecalculateNormals(60);
        }

        mf.mesh = m;
        go.AddComponent<MeshRenderer>().sharedMaterial = ribbonMat;
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

    void createUnityMesh(Segment seg, Transform parent, string name, MeshData meshD, Material ribbonMat = null)
    {
        createUnityMesh(seg, parent, name, meshD.vertices, meshD.normals, meshD.triangles, meshD.colors, ribbonMat);
    }

    private void getMeshColors() {
        meshColors.Clear();
        foreach (GameObject go in meshesGO) {
            Mesh m = go.GetComponent<MeshFilter>().sharedMesh;
            Color32[] tmpArrayColor = m.colors32;
            if (tmpArrayColor == null || tmpArrayColor.Length == 0) {
                tmpArrayColor = new Color32[m.vertices.Length];
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

    public void Clear() {
        foreach (GameObject go in meshesGO) {
            GameObject.Destroy(go);
        }
        meshesGO.Clear();
        residueToGo.Clear();
        residueToVert.Clear();
    }

    public void recompute(bool isNewModel = false) {

        List<Material> savedMat = new List<Material>();

        foreach (GameObject m in meshesGO) {
            savedMat.Add(m.GetComponent<MeshRenderer>().sharedMaterial);
        }

        getMeshColorsPerResidue();

        Clear();

        if (isNewModel) {
            segments = cutSelectionInSegments(selection);
            displayCartoonMesh(structureName, segments, newRep.transform, isTraj: false);
        }
        else { //If trajectory then make sure we have a faster way to compute the cartoon.
            // Right now we just lower the cartoon quality settings
            displayCartoonMesh(structureName, segments, newRep.transform, isTraj: true);
        }

        if (meshesGO.Count > 0 && meshesGO.Count == savedMat.Count) {
            int i = 0;
            foreach (GameObject m in meshesGO) {
                m.GetComponent<MeshRenderer>().sharedMaterial = savedMat[i++];
            }
        }

        UnityMolSelection biggerSel = getAllStructureSelection(selection);
        if (biggerSel.Count != selection.Count) {
            //Remove mesh parts not in selection
            removeMeshParts(biggerSel);
        }

        restoreColorsPerResidue();
    }
    public override void Clean() {
        Clear();
    }

}
public class Segment {
    public List<UnityMolResidue> residues;


    public Segment(UnityMolResidue residue) {
        residues = new List<UnityMolResidue>();
        residues.Add(residue);
    }

}
}