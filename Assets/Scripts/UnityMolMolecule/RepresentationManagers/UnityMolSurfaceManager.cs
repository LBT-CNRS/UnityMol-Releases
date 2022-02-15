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

namespace UMol {
public class UnityMolSurfaceManager : UnityMolGenericRepresentationManager {

    private List<GameObject> meshesGO;

    public ISurfaceRepresentation atomRep;

    private bool needUpdate = false;

    public bool isWireframe = false;
    public bool isTransparent = false;

    public float curAlpha = 1.0f;
    public float curWireSize = 0.1f;

    private List<Material> initMat;
    private List<Material> wireframeMat;
    private List<Material> transparentMat;

    private Material highlightMat;


    /// <summary>
    /// Disables the renderers for all objects managed by the instance of the manager.
    /// </summary>
    public override void DisableRenderers() {
        if (meshesGO == null)
            return;
        for (int i = 0; i < meshesGO.Count; i++) {
            meshesGO[i].GetComponent<Renderer>().enabled = false;
        }
        isEnabled = false;
        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();
    }

    /// <summary>
    /// Enables the renderers for all objects managed by the instance of the manager.
    /// </summary>
    public override void EnableRenderers() {
        if (meshesGO == null)
            return;
        if (needUpdate) {
            atomRep.recompute();
            needUpdate = false;
        }
        for (int i = 0; i < meshesGO.Count; i++) {
            meshesGO[i].GetComponent<Renderer>().enabled = true;
        }
        isEnabled = true;
        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();
    }

    public void SwitchComputeMethod() {
        if (atomRep.isStandardSurface) {
            SurfaceRepresentation sr = (SurfaceRepresentation) atomRep;
            if (sr.surfMethod == SurfMethod.EDTSurf) {
                sr.surfMethod = SurfMethod.MSMS;
            }
            else if (sr.surfMethod == SurfMethod.MSMS) {
                sr.surfMethod = SurfMethod.EDTSurf;
            }
            // else if (sr.surfMethod == SurfMethod.QUICKSES) {
            //     sr.surfMethod = SurfMethod.EDTSurf;
            // }
            atomRep.recompute();
        }
    }

    public void SwitchCutSurface(bool isCut) {
        if (atomRep.isStandardSurface) {
            ((SurfaceRepresentation)atomRep).isCutSurface = isCut;
            atomRep.recompute();
        }
    }

    /// <summary>
    /// Initializes this instance of the manager.
    /// </summary>
    public override void Init(SubRepresentation umolRep) {


        if (isInit) {
            return;
        }

        atomRep = (ISurfaceRepresentation) umolRep.atomRep;
        meshesGO = atomRep.meshesGO;
        initMat = new List<Material>();
        foreach (GameObject go in meshesGO) {
            initMat.Add(go.GetComponent<Renderer>().sharedMaterial);
        }
        wireframeMat = new List<Material>();
        transparentMat = new List<Material>();

        highlightMat = (Material) Resources.Load("Materials/HighlightMaterial");


        isInit = true;
        isEnabled = true;
        isWireframe = false;
        isTransparent = false;
        areSideChainsOn = true;
        areHydrogensOn = true;
        isBackboneOn = true;
    }

    public override void Clean() {
        GameObject parent = null;
        if (meshesGO.Count != 0) {
            parent = meshesGO[0].transform.parent.gameObject;
        }

        for (int i = 0; i < meshesGO.Count; i++) {
            GameObject.Destroy(meshesGO[i]);
        }
        if (parent != null) {
            GameObject.Destroy(parent);
        }

        wireframeMat.Clear();
        initMat.Clear();
        transparentMat.Clear();
        meshesGO.Clear();

        atomRep = null;
        wireframeMat = null;
        initMat = null;
        transparentMat = null;
        meshesGO = null;

        isWireframe = false;
        isTransparent = false;
        isInit = false;
        isEnabled = false;
    }

    public override void ShowShadows(bool show) {
        if (meshesGO == null)
            return;
        for (int i = 0; i < meshesGO.Count; i++) {
            if (show)
                meshesGO[i].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            else
                meshesGO[i].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    public override void ShowHydrogens(bool show) {
        areHydrogensOn = show;
    }//Does not really make sense for surface

    public override void ShowSideChains(bool show) {
        areSideChainsOn = show;
    }//Does not really make sense for surface

    public override void ShowBackbone(bool show) {
        isBackboneOn = show;
    }//Does not really make sense for surface


    public void ApplyColors() {
        foreach (GameObject go in meshesGO) {
            go.GetComponent<MeshFilter>().sharedMesh.SetColors(atomRep.meshColors[go]);
        }
    }


    public override void SetColor(Color col, UnityMolSelection sele) {
        if (sele.Count == atomRep.selection.Count) {
            foreach (GameObject go in atomRep.meshesGO) {
                List<Color32> colors = atomRep.meshColors[go];
                for (int i = 0; i < colors.Count; i++) {
                    Color32 c = col;
                    c.a = colors[i].a;
                    colors[i] = c;
                }
            }
        }
        else {
            foreach (UnityMolAtom a in sele.atoms) {
                SetColor(col, a, false);
            }
        }
        ApplyColors();
    }

    public override void SetColor(Color col, UnityMolAtom a) {
        GameObject go = null;
        if (atomRep.atomToGo.TryGetValue(a, out go)) {
            if (go != null) {

                List<int> res = null;
                List<Color32> colors = atomRep.meshColors[go];

                if (atomRep.atomToMesh.TryGetValue(a, out res)) {
                    Color32 newCol = (Color32) col;
                    atomRep.colorByAtom[a] = newCol;
                    for (int i = 0; i < res.Count; i++) {
                        if (res[i] >= 0 && res[i] < colors.Count) {
                            Color32 tmpCol = colors[res[i]];
                            newCol.a = tmpCol.a;
                            colors[res[i]] = newCol;
                        }
                        else {
                            Debug.Log("Wrong color index: " + res[i] + " > " + colors.Count);
                        }
                    }
                }
                else {
                    // Debug.LogWarning("No mesh vertex for this atom");
                }
            }
            else {
                // Debug.LogError("No mesh vertex for this atom");
            }
        }

        ApplyColors();
    }

    public void SetColor(Color col, UnityMolAtom a, bool applyNow) {
        if (applyNow) {
            SetColor(col, a);
            return;
        }
        GameObject go = null;
        if (atomRep.atomToGo.TryGetValue(a, out go)) {
            if (go != null) {
                List<int> res = null;
                List<Color32> colors = atomRep.meshColors[go];

                if (atomRep.atomToMesh.TryGetValue(a, out res)) {
                    Color32 newCol = col;
                    atomRep.colorByAtom[a] = newCol;
                    for (int i = 0; i < res.Count; i++) {
                        if (res[i] >= 0 && res[i] < colors.Count) {
                            Color32 tmpCol = colors[res[i]];
                            newCol.a = tmpCol.a;
                            colors[res[i]] = newCol;
                        }
                        else {
                            Debug.Log("Wrong color index: " + res[i] + " > " + colors.Count);
                        }
                    }
                }
                else {
                    // Debug.LogWarning("No mesh vertex for this atom");
                }
            }
            else {
                // Debug.LogError("No mesh vertex for this atom");
            }
        }
    }
    public void ClearAO() {
        foreach (GameObject go in meshesGO) {
            Mesh m = go.GetComponent<MeshFilter>().sharedMesh;
            Color32[] colors = m.colors32;
            for (int i = 0; i < colors.Length; i++) {
                Color32 col = colors[i];
                col.a = 255;
                colors[i] = col;
            }
            m.colors32 = colors;
        }
    }

    public override void SetColors(Color col, List<UnityMolAtom> atoms) {
        foreach (UnityMolAtom a in atoms) {
            SetColor(col, a, false);
        }
        ApplyColors();
    }

    public override void SetColors(List<Color> cols, List<UnityMolAtom> atoms) {
        if (atoms.Count != cols.Count) {
            Debug.LogError("Lengths of color list and atom list are different");
            return;
        }
        for (int i = 0; i < atoms.Count; i++) {
            UnityMolAtom a = atoms[i];
            Color col = cols[i];
            SetColor(col, a, false);
        }
        ApplyColors();
    }

    public static Color dxDensityToColor(float dens, float minD, float maxD) {
        if (dens < 0.0f) {
            float t = Mathf.Clamp(dens / minD, 0.0f, 1.0f);
            return Color.Lerp(Color.white, Color.red, t);
        }

        float tp = Mathf.Clamp(dens / maxD, 0.0f, 1.0f);
        return Color.Lerp(Color.white, Color.blue, tp);
    }

    public static Color32 dxDensityToColor32(float dens, float minD, float maxD) {
        Color32 white = new Color32(255, 255, 255, 255);
        Color32 red = new Color32(255, 0, 0, 255);
        Color32 blue = new Color32(0, 0, 255, 255);

        if (dens < 0.0f) {
            float t = Mathf.Clamp(dens / minD, 0.0f, 1.0f);
            return Color32.Lerp(white, red, t);
        }

        float tp = Mathf.Clamp(dens / maxD, 0.0f, 1.0f);
        return Color32.Lerp(white, blue, tp);
    }

    // Use atom positions to fetch the dx value
    // public void ColorByCharge(bool normalizeDens, float minDens = -10.0f, float maxDens = 10.0f) {

    //     //If only one structure in selection and a DX map was loaded for this structure
    //     if (atomRep.selection.structures.Count == 1 && atomRep.selection.structures[0].dxr != null) {
    //         DXReader dxr = atomRep.selection.structures[0].dxr;
    //         List<Color> colors = new List<Color>(atomRep.selection.atoms.Count);
    //         int cpt = 0;
    //         foreach (UnityMolAtom a in atomRep.selection.atoms) {
    //             float val = dxr.getValueAtPosition(a.position);
    //             if (normalizeDens) {
    //                 colors.Add(dxDensityToColor32(val, dxr.minDensityVal, dxr.maxDensityVal));
    //             }
    //             else {
    //                 colors.Add(dxDensityToColor32(val, minDens, maxDens));
    //             }
    //             cpt++;

    //         }
    //         SetColors(colors, atomRep.selection.atoms);
    //         return;
    //     }
    //     else if (atomRep.selection.structures.Count > 1) {
    //         Debug.LogError("The selection contains more than one structure");
    //         return;
    //     }
    //     Debug.LogError("No density map loaded for this structure");

    // }

    // Fetch the dx value using the vertex positions
    public void ColorByCharge(bool normalizeDens, float minDens, float maxDens) {
        //If only one structure in selection and a DX map was loaded for this structure
        if (atomRep.selection.structures.Count == 1 && atomRep.selection.structures[0].dxr != null) {
            DXReader dxr = atomRep.selection.structures[0].dxr;
            foreach (GameObject go in meshesGO) {
                Mesh m = go.GetComponent<MeshFilter>().sharedMesh;
                Color32[] colors = m.colors32;
                Vector3[] verts = m.vertices;
                for (int i = 0; i < colors.Length; i++) {
                    Color32 col = colors[i];
                    byte alpha = col.a;

                    Vector3 v = verts[i];
                    float val = dxr.getValueAtPosition(v);
                    if (normalizeDens) {
                        col = dxDensityToColor32(val, dxr.minDensityVal, dxr.maxDensityVal);
                    }
                    else {
                        col = dxDensityToColor32(val, minDens, maxDens);
                    }

                    col.a = alpha;
                    colors[i] = col;
                }
                m.colors32 = colors;
            }
            return;
        }
        else if (atomRep.selection.structures.Count > 1) {
            Debug.LogError("The selection contains more than one structure");
            return;
        }
        Debug.LogError("No density map loaded for this structure");
    }

    public override void SetDepthCueingStart(float v) {
        if (meshesGO == null)
            return;
        foreach (GameObject meshGO in meshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_FogStart", v);
        }
    }

    public override void SetDepthCueingDensity(float v) {
        if (meshesGO == null)
            return;
        foreach (GameObject meshGO in meshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_FogDensity", v);
        }
    }

    public override void EnableDepthCueing() {
        if (meshesGO == null)
            return;
        foreach (GameObject meshGO in meshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_UseFog", 1.0f);
        }
    }

    public override void DisableDepthCueing() {
        if (meshesGO == null)
            return;
        foreach (GameObject meshGO in meshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_UseFog", 0.0f);
        }
    }

    /// <summary>
    /// As it is SUPER slow, do not update surface representations with trajectory for now
    /// </summary>
    public override void updateWithTrajectory() {
        // Debug.LogWarning("Deactivating surface representation when reading trajectory = too slow");
        // DisableRenderers();

        needUpdate = true;
        bool wasEnabled = true;
        if (meshesGO != null && meshesGO.Count >= 1)
            wasEnabled = meshesGO[0].GetComponent<Renderer>().enabled;

        if (wasEnabled) {
            atomRep.recompute();
            needUpdate = false;
        }
    }

    public override void updateWithModel() {
        updateWithTrajectory();
    }


    //TODO !
    public override void ShowAtom(UnityMolAtom atom, bool show) {
        Debug.LogWarning("Cannot show/hide one atom with the surface representation");
    }

    public override void SetSize(UnityMolAtom atom, float size) {
        Debug.LogWarning("Cannot set the size of one atom with the surface representation");
    }
    public override void ResetSize(UnityMolAtom atom) {
        Debug.LogWarning("Cannot set size of one atom with the surface representation");
    }

    public override void ResetSizes() {
        Debug.LogWarning("Cannot set size of atoms with the surface representation");
    }

    public override void SetSizes(List<UnityMolAtom> atoms, List<float> sizes) {
        Debug.LogWarning("Cannot set size of atoms with the surface representation");
    }

    public override void SetSizes(List<UnityMolAtom> atoms, float size) {
        Debug.LogWarning("Cannot set size of atoms with the surface representation");
    }

    public override void ResetColor(UnityMolAtom atom) {
        SetColor(Color.white, atom);
    }

    public override void ResetColors() {
        foreach (UnityMolAtom a in atomRep.selection.atoms) {
            SetColor(Color.white, a, false);
        }
        ApplyColors();
        atomRep.colorationType = colorType.full;
    }

    public void SwitchWireframe(bool force = false) {
        if (SystemInfo.graphicsShaderLevel >= 35) {
            if (isWireframe && !force) {
                int i = 0;
                foreach (GameObject go in meshesGO) {
                    go.GetComponent<Renderer>().sharedMaterial = initMat[i];
                    i++;
                }
            }
            else if (!isWireframe) {
                if (wireframeMat.Count == 0) {
                    Material wmat = (Material) Resources.Load("Materials/SurfaceWireframeVertColor");
                    // Material wmat = new Material(Shader.Find("UCLA Game Lab/Wireframe Double Sided"));
                    foreach (GameObject go in meshesGO) {
                        go.GetComponent<Renderer>().sharedMaterial = wmat;
                        wireframeMat.Add(wmat);
                    }
                }
                else {
                    int i = 0;
                    foreach (GameObject go in meshesGO) {
                        go.GetComponent<Renderer>().sharedMaterial = wireframeMat[i];
                        i++;
                    }
                }
            }
            isWireframe = !isWireframe;
            isTransparent = false;
        }
        else {
            Debug.LogWarning("Wireframe not available for your GPU");
        }
    }
    public void SwitchTransparent(bool force = false) {

        if (isTransparent && !force) {
            int i = 0;
            foreach (GameObject go in meshesGO) {
                go.GetComponent<Renderer>().sharedMaterial = initMat[i];
                i++;
            }
            isTransparent = false;
        }
        else {
            if (transparentMat.Count == 0) {
                // Material wmat = (Material) Resources.Load("Materials/standardColorTransparent");
                Material wmat = new Material(Shader.Find("Custom/SurfaceVertexColorTransparent"));
                foreach (GameObject go in meshesGO) {
                    go.GetComponent<Renderer>().sharedMaterial = wmat;
                    transparentMat.Add(wmat);
                }
            }
            else {
                int i = 0;
                foreach (GameObject go in meshesGO) {
                    go.GetComponent<Renderer>().sharedMaterial = transparentMat[i];
                    i++;
                }
            }
            isTransparent = true;
        }
        isWireframe = false;
    }

    public override void HighlightRepresentation() {
        foreach (GameObject meshGO in meshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;

            if (mats.Length != 2) {
                Material[] newMats = new Material[2];
                newMats[0] = mats[0];
                newMats[1] = highlightMat;
                mats = newMats;
                meshGO.GetComponent<Renderer>().sharedMaterials = newMats;
            }
        }

    }


    public override void DeHighlightRepresentation() {

        foreach (GameObject meshGO in meshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;

            if (mats.Length != 1) {
                Material[] newMats = new Material[1];
                newMats[0] = mats[0];
                meshGO.GetComponent<Renderer>().sharedMaterials = newMats;
            }
        }
    }
    public override void SetSmoothness(float val) {
        if (!isWireframe) {
            foreach (GameObject meshGO in meshesGO) {

                Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
                mats[0].SetFloat("_Glossiness", val);
            }
        }
    }
    public override void SetMetal(float val) {
        if (!isWireframe) {
            foreach (GameObject meshGO in meshesGO) {

                Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
                mats[0].SetFloat("_Metallic", val);
            }
        }
    }

    public void SetAlpha(float alpha) {
        if (isTransparent) {
            foreach (GameObject meshGO in meshesGO) {

                Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
                Color col = mats[0].color;
                col.a = alpha;
                mats[0].color = col;
            }
            curAlpha = alpha;
        }
    }

    public void SetWireframeSize(float s) {
        SwitchWireframe(true);
        foreach (Material mat in wireframeMat) {
            mat.SetFloat("_Thickness", s);
        }
        curWireSize = s;
    }
    public override UnityMolRepresentationParameters Save() {
        UnityMolRepresentationParameters res = new UnityMolRepresentationParameters();
        res.repT.atomType = AtomType.surface;
        res.repT.bondType = BondType.nobond;
        res.colorationType = atomRep.colorationType;

        if (res.colorationType == colorType.custom) {
            res.colorPerAtom = new Dictionary<UnityMolAtom, Color32>(atomRep.selection.Count);
            foreach (UnityMolAtom a in atomRep.colorByAtom.Keys) {
                res.colorPerAtom[a] = atomRep.colorByAtom[a];
            }
        }
        else if (res.colorationType == colorType.full) { //Get color of first atom/residue
            res.fullColor = atomRep.meshColors[atomRep.meshesGO[0]][0];
        }
        else if (res.colorationType == colorType.bfactor) {
            res.bfactorStartColor = atomRep.bfactorStartCol;
            res.bfactorEndColor = atomRep.bfactorEndCol;
        }

        if (!isTransparent && !isWireframe) {
            res.smoothness = meshesGO[0].GetComponent<Renderer>().sharedMaterials[0].GetFloat("_Glossiness");
            res.metal = meshesGO[0].GetComponent<Renderer>().sharedMaterials[0].GetFloat("_Metallic");
        }
        res.shadow = (meshesGO[0].GetComponent<Renderer>().shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.On);
        res.surfAlpha = curAlpha;
        res.surfWireframeSize = curWireSize;
        res.surfIsTransparent = isTransparent;
        res.surfIsWireframe = isWireframe;

        return res;
    }

    public override void Restore(UnityMolRepresentationParameters savedParams) {

        if (savedParams.repT.atomType == AtomType.surface && savedParams.repT.bondType == BondType.nobond) {
            if (savedParams.colorationType == colorType.full) {
                SetColor(savedParams.fullColor, atomRep.selection);
            }
            else if (savedParams.colorationType == colorType.custom) {
                foreach (UnityMolAtom a in atomRep.selection.atoms) {
                    if (savedParams.colorPerAtom.ContainsKey(a)) {
                        SetColor(savedParams.colorPerAtom[a], a, false);
                    }
                }
                ApplyColors();
            }
            else if (savedParams.colorationType == colorType.res) {
                colorByRes(atomRep.selection);
            }
            else if (savedParams.colorationType == colorType.chain) {
                colorByChain(atomRep.selection);
            }
            else if (savedParams.colorationType == colorType.hydro) {
                colorByHydro(atomRep.selection);
            }
            else if (savedParams.colorationType == colorType.seq) {
                colorBySequence(atomRep.selection);
            }
            else if (savedParams.colorationType == colorType.charge) {
                colorByCharge(atomRep.selection);
            }
            else if (savedParams.colorationType == colorType.restype) {
                colorByResType(atomRep.selection);
            }
            else if (savedParams.colorationType == colorType.rescharge) {
                colorByResCharge(atomRep.selection);
            }
            else if (savedParams.colorationType == colorType.bfactor) {
                colorByBfactor(atomRep.selection, savedParams.bfactorStartColor, savedParams.bfactorEndColor);
            }

            ShowShadows(savedParams.shadow);

            if (savedParams.surfIsWireframe) {
                SwitchWireframe(true);
                SetWireframeSize(savedParams.surfWireframeSize);
            }
            else if (savedParams.surfIsTransparent) {
                SwitchTransparent(true);
                SetAlpha(savedParams.surfAlpha);
            }
            else {
                SetMetal(savedParams.metal);
                SetSmoothness(savedParams.smoothness);
            }
            atomRep.colorationType = savedParams.colorationType;
        }
        else {
            Debug.LogError("Could not restore representation parameteres");
        }
    }
}
}