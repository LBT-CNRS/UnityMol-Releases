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
using UnityEngine.Rendering;
using System.Linq;

namespace UMol {

public class UnityMolBondLineManager : UnityMolGenericRepresentationManager {
    private UnityMolRepresentation rep;
    private BondRepresentationLine bondRep;
    public GameObject meshGO;
    public Mesh curMesh;
    private List<Vector3> verticesList;
    private List<Color32> colorsList;
    public Dictionary<UnityMolAtom, List<int>> atomToMeshVertex;
    private List<AtomDuo> flattenBonds;

    // private Material highlightMat;

    /// <summary>
    /// Initializes this instance of the manager.
    /// </summary>
    public override void Init(SubRepresentation umolRep) {

        if (isInit) {
            return;
        }

        bondRep = (BondRepresentationLine) umolRep.bondRep;
        meshGO = bondRep.meshGO;
        curMesh = bondRep.curMesh;
        atomToMeshVertex = bondRep.atomToMeshVertex;
        // highlightMat = (Material) Resources.Load("Materials/HighlightMaterial");

        flattenBonds = bondRep.selection.bonds.ToList();

        verticesList = bondRep.vertices;
        colorsList = bondRep.colors;

        isInit = true;
        isEnabled = true;
        areSideChainsOn = true;
        areHydrogensOn = true;
        isBackboneOn = true;
    }

    public override void Clean() {
        if (meshGO != null && meshGO.transform.parent != null) {
            GameObject.Destroy(meshGO.transform.parent.gameObject);
        }

        if (flattenBonds != null) {
            flattenBonds.Clear();
        }
        if (atomToMeshVertex != null) {
            atomToMeshVertex.Clear();
        }
        if (verticesList != null) {
            verticesList.Clear();
        }
        if (colorsList != null) {
            colorsList.Clear();
        }

        isEnabled = false;
        curMesh = null;
        bondRep = null;
        flattenBonds = null;


        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();
    }
    /// <summary>
    /// Disables the renderers for all objects managed by the instance of the manager.
    /// </summary>

    public override void DisableRenderers() {
        if (meshGO != null) {
            meshGO.GetComponent<Renderer>().enabled = false;
        }
        isEnabled = false;
        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();
    }
    /// <summary>
    /// Enables the renderers for all objects managed by the instance of the manager.
    /// </summary>
    public override void EnableRenderers() {
        if (meshGO != null) {
            meshGO.GetComponent<Renderer>().enabled = true;
        }
        isEnabled = true;
        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();
    }

    public override void ShowHydrogens(bool show) {

        HashSet<UnityMolAtom> toHide = new HashSet<UnityMolAtom>();

        foreach (AtomDuo duo in flattenBonds) {
            UnityMolAtom atom1 = duo.a1;
            UnityMolAtom atom2 = duo.a2;

            if (atom1.type == "H") {
                if (show && !areSideChainsOn && MDAnalysisSelection.isSideChain(atom1, bondRep.selection.bonds)) {
                }
                else {
                    toHide.Add(atom1);
                }
            }

            if (atom2.type == "H") {
                if (show && !areSideChainsOn && MDAnalysisSelection.isSideChain(atom2, bondRep.selection.bonds)) {
                }
                else {
                    toHide.Add(atom2);
                }
            }
        }
        ShowAtoms(toHide, show);
        areHydrogensOn = show;
    }

    public override void ShowSideChains(bool show) {

        float lineWidth = bondRep.lineWidth;

        int idVert = 0;

        foreach (AtomDuo duo in flattenBonds) {
            UnityMolAtom atom1 = duo.a1;
            UnityMolAtom atom2 = duo.a2;
            if (MDAnalysisSelection.isSideChain(atom1, bondRep.selection.bonds) || MDAnalysisSelection.isSideChain(atom2, bondRep.selection.bonds)) {
                if (show && !areHydrogensOn && (atom1.type == "H" || atom2.type == "H")) {
                }
                else {
                    Vector3 start = atom1.position;
                    Vector3 end   = atom2.position;

                    if (!show) {
                        verticesList[ idVert + 0] = start;
                        verticesList[ idVert + 1] = start;
                        verticesList[ idVert + 2] = start;
                        verticesList[ idVert + 3] = start;
                        verticesList[ idVert + 4] = start;
                        verticesList[ idVert + 5] = start;
                        verticesList[ idVert + 6] = start;
                        verticesList[ idVert + 7] = start;
                    }
                    else {

                        Vector3 normal = Vector3.Cross(start, end);
                        Vector3 side = Vector3.Cross(normal, end - start);
                        side.Normalize();
                        normal.Normalize();

                        Vector3 a = start + side * (lineWidth / 2);
                        Vector3 b = start - side * (lineWidth / 2);
                        Vector3 c = end + side * (lineWidth / 2);
                        Vector3 d = end - side * (lineWidth / 2);

                        Vector3 a1 = a + normal * (lineWidth / 2);
                        Vector3 a2 = a - normal * (lineWidth / 2);
                        Vector3 b1 = b + normal * (lineWidth / 2);
                        Vector3 b2 = b - normal * (lineWidth / 2);
                        Vector3 c1 = c + normal * (lineWidth / 2);
                        Vector3 c2 = c - normal * (lineWidth / 2);
                        Vector3 d1 = d + normal * (lineWidth / 2);
                        Vector3 d2 = d - normal * (lineWidth / 2);

                        verticesList[ idVert + 0] = a1;
                        verticesList[ idVert + 1] = a2;
                        verticesList[ idVert + 2] = b1;
                        verticesList[ idVert + 3] = b2;
                        verticesList[ idVert + 4] = c1;
                        verticesList[ idVert + 5] = c2;
                        verticesList[ idVert + 6] = d1;
                        verticesList[ idVert + 7] = d2;
                    }
                }
            }

            idVert += 8;
        }
        curMesh.SetVertices(verticesList);
        areSideChainsOn = show;
    }

    public override void ShowBackbone(bool show) {
        HashSet<UnityMolAtom> toHide = new HashSet<UnityMolAtom>();

        foreach (AtomDuo duo in flattenBonds) {
            UnityMolAtom atom1 = duo.a1;
            UnityMolAtom atom2 = duo.a2;

            if (MDAnalysisSelection.isBackBone(atom1, bondRep.selection.bonds)) {
                // ShowAtom(atom1, show);//Slow
                toHide.Add(atom1);
            }
            if (MDAnalysisSelection.isBackBone(atom2, bondRep.selection.bonds)) {
                // ShowAtom(atom2, show);//Slow
                toHide.Add(atom2);
            }
        }
        ShowAtoms(toHide, show);
        isBackboneOn = show;
    }


    public override void SetColor(Color col, UnityMolSelection sele) {
        SetColors(col, sele.atoms);
    }

    public override void SetColor(Color col, UnityMolAtom a) {
        try {
            Color32 newCol = col;

            foreach (int vid in atomToMeshVertex[a]) {
                colorsList[vid] = newCol;
            }
            curMesh.SetColors(colorsList);
        }
        catch {
            Debug.LogError("Could not find atom " + a + " in this representation");
        }
    }
    public override void SetColors(Color col, List<UnityMolAtom> atoms) {
        try {
            Color32 newCol = col;
            foreach (UnityMolAtom a in atoms) {
                List<int> verts = null;
                if (atomToMeshVertex.TryGetValue(a, out verts)) {
                    foreach (int vid in verts) {
                        colorsList[vid] = newCol;
                    }
                }
            }
            curMesh.SetColors(colorsList);
        }
        catch (System.Exception e) {
            Debug.LogError("Failed to set the color of the line representation " + e);
        }
    }

    public override void SetColors(List<Color> cols, List<UnityMolAtom> atoms) {
        if (atoms.Count != cols.Count) {
            Debug.LogError("Lengths of color list and atom list are different");
            return;
        }
        for (int i = 0; i < atoms.Count; i++) {
            UnityMolAtom a = atoms[i];
            Color col = cols[i];
            SetColor(col, a);
        }
    }

    public override void SetDepthCueingStart(float v) {
        if (meshGO == null)
            return;

        Material mat = meshGO.GetComponent<Renderer>().sharedMaterial;
        mat.SetFloat("_FogStart", v);

    }

    public override void SetDepthCueingDensity(float v) {
        if (meshGO == null)
            return;

        Material mat = meshGO.GetComponent<Renderer>().sharedMaterial;
        mat.SetFloat("_FogDensity", v);
    }

    public override void EnableDepthCueing() {
        if (meshGO == null)
            return;
        Material mat = meshGO.GetComponent<Renderer>().sharedMaterial;
        mat.SetFloat("_UseFog", 1.0f);
    }

    public override void DisableDepthCueing() {
        if (meshGO == null)
            return;
        Material mat = meshGO.GetComponent<Renderer>().sharedMaterial;
        mat.SetFloat("_UseFog", 0.0f);
    }

/// <summary>
/// No shadows for bond lines
/// </summary>
    public override void ShowShadows(bool show) {}

/// <summary>
/// Recomputes the bond representation
/// </summary>
    public override void updateWithTrajectory() {
        if (curMesh == null) {
            return;
        }
        //Do not need to clear everything, just recompute the vertex array
        float lineWidth = bondRep.lineWidth;
        int idVert = 0;

        UnityMolModel curM = null;

        foreach (AtomDuo duo in flattenBonds) {

            UnityMolStructure s1 = duo.a1.residue.chain.model.structure;
            UnityMolStructure s2 = duo.a2.residue.chain.model.structure;

            UnityMolModel m1 = curM;
            UnityMolModel m2 = curM;

            if (curM == null || curM.structure != s1 || curM.structure != s2) {
                m1 = s1.currentModel;
                m2 = s2.currentModel;
                curM = m1;
            }

            UnityMolAtom atom1 = m1.allAtoms[duo.a1.idInAllAtoms];
            UnityMolAtom atom2 = m2.allAtoms[duo.a2.idInAllAtoms];

            Vector3 start = atom1.position;
            Vector3 end   = atom2.position;

            Vector3 normal = Vector3.Cross(start, end);
            Vector3 side = Vector3.Cross(normal, end - start);
            side.Normalize();
            normal.Normalize();

            Vector3 a = start + side * (lineWidth / 2);
            Vector3 b = start - side * (lineWidth / 2);
            Vector3 c = end + side * (lineWidth / 2);
            Vector3 d = end - side * (lineWidth / 2);

            Vector3 a1 = a + normal * (lineWidth / 2);
            Vector3 a2 = a - normal * (lineWidth / 2);
            Vector3 b1 = b + normal * (lineWidth / 2);
            Vector3 b2 = b - normal * (lineWidth / 2);
            Vector3 c1 = c + normal * (lineWidth / 2);
            Vector3 c2 = c - normal * (lineWidth / 2);
            Vector3 d1 = d + normal * (lineWidth / 2);
            Vector3 d2 = d - normal * (lineWidth / 2);

            verticesList[ idVert + 0] = a1;
            verticesList[ idVert + 1] = a2;
            verticesList[ idVert + 2] = b1;
            verticesList[ idVert + 3] = b2;
            verticesList[ idVert + 4] = c1;
            verticesList[ idVert + 5] = c2;
            verticesList[ idVert + 6] = d1;
            verticesList[ idVert + 7] = d2;

            idVert += 8;
        }

        curMesh.SetVertices(verticesList);
        curMesh.RecalculateBounds();
    }

    public override void updateWithModel() {
        updateWithTrajectory();
    }

    public void ShowAtoms(HashSet<UnityMolAtom> atoms, bool show) {
        if (curMesh == null) {
            return;
        }
        float lineWidth = bondRep.lineWidth;

        int idVert = 0;

        foreach (AtomDuo duo in flattenBonds) {
            UnityMolAtom atom1 = duo.a1;
            UnityMolAtom atom2 = duo.a2;
            if (atoms.Contains(atom1) || atoms.Contains(atom2)) {
                Vector3 start = atom1.position;
                Vector3 end   = atom2.position;

                Vector3 normal = Vector3.Cross(start, end);
                Vector3 side = Vector3.Cross(normal, end - start);
                side.Normalize();
                normal.Normalize();

                Vector3 a = start + side * (lineWidth / 2);
                Vector3 b = start - side * (lineWidth / 2);
                Vector3 c = end + side * (lineWidth / 2);
                Vector3 d = end - side * (lineWidth / 2);

                Vector3 a1 = a + normal * (lineWidth / 2);
                Vector3 a2 = a - normal * (lineWidth / 2);
                Vector3 b1 = b + normal * (lineWidth / 2);
                Vector3 b2 = b - normal * (lineWidth / 2);
                Vector3 c1 = c + normal * (lineWidth / 2);
                Vector3 c2 = c - normal * (lineWidth / 2);
                Vector3 d1 = d + normal * (lineWidth / 2);
                Vector3 d2 = d - normal * (lineWidth / 2);

                if (!show) {
                    verticesList[ idVert + 0] = start;
                    verticesList[ idVert + 1] = start;
                    verticesList[ idVert + 2] = start;
                    verticesList[ idVert + 3] = start;
                    verticesList[ idVert + 4] = start;
                    verticesList[ idVert + 5] = start;
                    verticesList[ idVert + 6] = start;
                    verticesList[ idVert + 7] = start;
                }
                else {
                    verticesList[ idVert + 0] = a1;
                    verticesList[ idVert + 1] = a2;
                    verticesList[ idVert + 2] = b1;
                    verticesList[ idVert + 3] = b2;
                    verticesList[ idVert + 4] = c1;
                    verticesList[ idVert + 5] = c2;
                    verticesList[ idVert + 6] = d1;
                    verticesList[ idVert + 7] = d2;
                }
            }
            idVert += 8;
        }
        curMesh.SetVertices(verticesList);
    }

    public void SetWidth(float newWidth) {
        if (newWidth <= 0.0f || newWidth > 1.0f) {
            Debug.LogWarning("Line size should be between 0.0 and 1.0");
            return;
        }
        bondRep.lineWidth = newWidth;
        //Recompute the line representation
        updateWithTrajectory();
    }


    public override void ShowAtom(UnityMolAtom atom, bool show) {
        if (curMesh == null) {
            return;
        }
        float lineWidth = bondRep.lineWidth;
        int idVert = 0;

        foreach (AtomDuo duo in flattenBonds) {
            UnityMolAtom atom1 = duo.a1;
            UnityMolAtom atom2 = duo.a2;
            if (atom1 == atom || atom2 == atom) {
                Vector3 start = atom.position;
                Vector3 end   = atom2.position;

                Vector3 normal = Vector3.Cross(start, end);
                Vector3 side = Vector3.Cross(normal, end - start);
                side.Normalize();
                normal.Normalize();

                Vector3 a = start + side * (lineWidth / 2);
                Vector3 b = start - side * (lineWidth / 2);
                Vector3 c = end + side * (lineWidth / 2);
                Vector3 d = end - side * (lineWidth / 2);

                Vector3 a1 = a + normal * (lineWidth / 2);
                Vector3 a2 = a - normal * (lineWidth / 2);
                Vector3 b1 = b + normal * (lineWidth / 2);
                Vector3 b2 = b - normal * (lineWidth / 2);
                Vector3 c1 = c + normal * (lineWidth / 2);
                Vector3 c2 = c - normal * (lineWidth / 2);
                Vector3 d1 = d + normal * (lineWidth / 2);
                Vector3 d2 = d - normal * (lineWidth / 2);

                if (!show) {
                    verticesList[ idVert + 0] = start;
                    verticesList[ idVert + 1] = start;
                    verticesList[ idVert + 2] = start;
                    verticesList[ idVert + 3] = start;
                    verticesList[ idVert + 4] = start;
                    verticesList[ idVert + 5] = start;
                    verticesList[ idVert + 6] = start;
                    verticesList[ idVert + 7] = start;
                }
                else {
                    verticesList[ idVert + 0] = a1;
                    verticesList[ idVert + 1] = a2;
                    verticesList[ idVert + 2] = b1;
                    verticesList[ idVert + 3] = b2;
                    verticesList[ idVert + 4] = c1;
                    verticesList[ idVert + 5] = c2;
                    verticesList[ idVert + 6] = d1;
                    verticesList[ idVert + 7] = d2;
                }
            }
            idVert += 8;
        }
        curMesh.SetVertices(verticesList);



        // try {//Test with discard in shader but was not concluding
        //     List<int> vertIds = atomToMeshVertex[atom];
        //     Vector2[] allUVs = curMesh.uv2;
        //     Vector2 newuv = Vector2.one;
        //     if (!show) {
        //         newuv = Vector2.zero;
        //     }
        //     for (int i = 0; i < vertIds.Count; i++) {
        //         allUVs[vertIds[i]] = newuv;
        //     }
        //     curMesh.uv2 = allUVs;
        // }
        // catch {
        //     Debug.LogWarning("Could not hide atom " + atom);
        // }
    }

/// <summary>
/// Size does not matter for line representation
/// </summary>
    public override void SetSize(UnityMolAtom atom, float size) {
        Debug.LogWarning("Cannot change the size atoms with the line representation");
    }

    public override void SetSizes(List<UnityMolAtom> atoms, List<float> sizes) {
        Debug.LogWarning("Cannot change the size atoms with the line representation");
    }

    public override void SetSizes(List<UnityMolAtom> atoms, float size) {
        Debug.LogWarning("Cannot change the size atoms with the line representation");
    }

    public override void ResetSize(UnityMolAtom atom) {
        Debug.LogWarning("Cannot change the size atoms with the line representation");
    }
    public override void ResetSizes() {
        Debug.LogWarning("Cannot change the size atoms with the line representation");
    }

    public override void ResetColor(UnityMolAtom atom) {
        SetColor(atom.color, atom);
    }
    public override void ResetColors() {
        foreach (UnityMolAtom a in bondRep.selection.bonds.bondsDual.Keys) {
            SetColor(a.color, a);
        }
        bondRep.colorationType = colorType.atom;
    }

    public override void HighlightRepresentation() {
        // Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;

        // if (mats.Length != 2) {
        //     Material[] newMats = new Material[2];
        //     newMats[0] = mats[0];
        //     newMats[1] = highlightMat;
        //     mats = newMats;
        //     meshGO.GetComponent<Renderer>().sharedMaterials = newMats;
        // }

    }


    public override void DeHighlightRepresentation() {
        // Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;

        // if (mats.Length != 1) {
        //     Material[] newMats = new Material[1];
        //     newMats[0] = mats[0];
        //     meshGO.GetComponent<Renderer>().sharedMaterials = newMats;
        // }
    }
    public override void SetSmoothness(float val) {
        Debug.LogWarning("Cannot change this value for the line representation");
    }
    public override void SetMetal(float val) {
        Debug.LogWarning("Cannot change this value for the line representation");
    }
    public override UnityMolRepresentationParameters Save(){
        UnityMolRepresentationParameters res = new UnityMolRepresentationParameters();

        res.repT.bondType = BondType.line;
        res.colorationType = bondRep.colorationType;

        if (res.colorationType == colorType.custom) {
            int atomNum = 0;
            res.colorPerAtom = new Dictionary<UnityMolAtom, Color32>(bondRep.selection.Count);
            foreach(UnityMolAtom a in bondRep.selection.atoms){
                res.colorPerAtom[a] = colorsList[atomToMeshVertex[a][0]];
            }

        }
        else if (res.colorationType == colorType.full) { //Get color of first atom/residue
            int atomNum = 0;
            foreach(UnityMolAtom a in bondRep.selection.atoms){
                res.fullColor = colorsList[atomToMeshVertex[a][0]];
                break;
            }
        }
        else if (res.colorationType == colorType.bfactor) {
            res.bfactorStartColor = bondRep.bfactorStartCol;
            res.bfactorEndColor = bondRep.bfactorEndCol;
        }
        res.LineWidth = bondRep.lineWidth;
        return res;
    }
    public override void Restore(UnityMolRepresentationParameters savedParams){
        if (savedParams.repT.bondType == BondType.line) {
            if (savedParams.colorationType == colorType.full) {
                SetColor(savedParams.fullColor, bondRep.selection);
            }
            else if (savedParams.colorationType == colorType.custom) {
                foreach (UnityMolAtom a in bondRep.selection.atoms) {
                    if (savedParams.colorPerAtom.ContainsKey(a)) {
                        SetColor(savedParams.colorPerAtom[a], a);
                    }
                }
            }
            else if (savedParams.colorationType == colorType.res) {
                colorByRes(bondRep.selection);
            }
            else if (savedParams.colorationType == colorType.chain) {
                colorByChain(bondRep.selection);
            }
            else if (savedParams.colorationType == colorType.hydro) {
                colorByHydro(bondRep.selection);
            }
            else if (savedParams.colorationType == colorType.seq) {
                colorBySequence(bondRep.selection);
            }
            else if (savedParams.colorationType == colorType.charge) {
                colorByCharge(bondRep.selection);
            }
            else if (savedParams.colorationType == colorType.restype) {
                colorByResType(bondRep.selection);
            }
            else if (savedParams.colorationType == colorType.rescharge) {
                colorByResCharge(bondRep.selection);
            }
            else if (savedParams.colorationType == colorType.bfactor) {
                colorByBfactor(bondRep.selection, savedParams.bfactorStartColor, savedParams.bfactorEndColor);
            }
            SetWidth(savedParams.LineWidth);
            bondRep.colorationType = savedParams.colorationType;
        }
        else {
            Debug.LogError("Could not restore representation parameteres");
        }

    }
}
}