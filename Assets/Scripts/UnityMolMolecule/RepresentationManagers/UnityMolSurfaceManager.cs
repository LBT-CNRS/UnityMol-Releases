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
using UnityEngine.Rendering;

namespace UMol {
public class UnityMolSurfaceManager : UnityMolGenericRepresentationManager {

    ///Is limited view activated
    public bool limitedView = false;
    ///Local space coordinates of the center of limited view
    public Vector3 limitedViewCenter = Vector3.zero;
    ///Radius in Angstrom of the limited view
    public float limitedViewRadius = 10.0f;

    private List<Vector2[]> meshesUV2;

    public ISurfaceRepresentation atomRep;

    private bool needUpdate = false;

    public bool isWireframe = false;
    public bool isTransparent = false;

    public float curAlpha = 0.1f;
    public float curWireSize = 0.1f;

    // private Material highlightMat;

    private bool savedShadowBeforeLimited = true;
    public bool AOOn = true;

    public List<GameObject> meshesGO;


    /// <summary>
    /// Disables the renderers for all objects managed by the instance of the manager.
    /// </summary>
    public override void DisableRenderers() {
        if (atomRep.meshesGO == null)
            return;
        for (int i = 0; i < atomRep.meshesGO.Count; i++) {
            atomRep.meshesGO[i].GetComponent<Renderer>().enabled = false;
        }
        isEnabled = false;
        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();
    }

    /// <summary>
    /// Enables the renderers for all objects managed by the instance of the manager.
    /// </summary>
    public override void EnableRenderers() {
        if (atomRep.meshesGO == null)
            return;
        if (needUpdate) {
            DoRecompute();
        }
        for (int i = 0; i < atomRep.meshesGO.Count; i++) {
            atomRep.meshesGO[i].GetComponent<Renderer>().enabled = true;
        }
        isEnabled = true;
        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();
    }

    public void DoRecompute(bool isTraj = false) {
        RaytracingMaterial savedrtMat = null;
        if (rtos != null && rtos.Count > 0) {
            savedrtMat = rtos[0].rtMat;

            for (int i = 0; i < rtos.Count; i++) {
                GameObject.Destroy(rtos[i]);
            }
            rtos.Clear();
        }
        atomRep.recompute(isTraj);
        needUpdate = false;

        if (rtos != null) {
            foreach (GameObject meshGO in atomRep.meshesGO) {
                var rto = meshGO.AddComponent<RaytracedObject>();
                if (savedrtMat != null)
                    rto.rtMat = savedrtMat;
                rtos.Add(rto);
            }
        }

    }

    public void SwitchComputeMethod() {
        if (atomRep.isStandardSurface) {
            SurfaceRepresentation sr = (SurfaceRepresentation) atomRep;
            if (sr.SurfMethod == SurfMethod.EDTSurf) {
                sr.SurfMethod = SurfMethod.MSMS;
            }
            else if (sr.SurfMethod == SurfMethod.MSMS) {
                sr.SurfMethod = SurfMethod.EDTSurf;
            }
            // else if (sr.surfMethod == SurfMethod.QUICKSES) {
            //     sr.surfMethod = SurfMethod.EDTSurf;
            // }

            DoRecompute();
        }
    }

    public void SwitchCutSurface(bool isCut) {
        if (atomRep.isStandardSurface) {
            ((SurfaceRepresentation)atomRep).IsCutSurface = isCut;
            DoRecompute();
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
        AOOn = atomRep.useAO;
        meshesGO = atomRep.meshesGO;

        if (UnityMolMain.raytracingMode)
            InitRT();

        isInit = true;
        isEnabled = true;
        isWireframe = false;
        isTransparent = false;
        areSideChainsOn = true;
        areHydrogensOn = true;
        isBackboneOn = true;
    }

    public override void InitRT() {
        if (rtos == null) {
            rtos = new List<RaytracedObject>();

            foreach (GameObject go in atomRep.meshesGO) {
                RaytracedObject rto = go.AddComponent<RaytracedObject>();
                rtos.Add(rto);
            }
        }
    }

    public override void Clean() {

        if (rtos != null) {
            for (int i = 0; i < rtos.Count; i++) {
                GameObject.Destroy(rtos[i]);
            }
            rtos.Clear();
        }

        GameObject parent = null;
        if (atomRep.meshesGO != null) {
            if (atomRep.meshesGO.Count != 0) {
                parent = atomRep.meshesGO[0].transform.parent.gameObject;
            }

            for (int i = 0; i < atomRep.meshesGO.Count; i++) {
                GameObject.Destroy(atomRep.meshesGO[i].GetComponent<MeshFilter>().sharedMesh);
                GameObject.Destroy(atomRep.meshesGO[i]);
            }
        }
        if (parent != null) {
            GameObject.Destroy(parent);
        }

        atomRep.meshesGO.Clear();

        atomRep.meshesGO = null;
        atomRep = null;

        isWireframe = false;
        isTransparent = false;
        isInit = false;
        isEnabled = false;
    }

    public override void ShowShadows(bool show) {
        if (atomRep.meshesGO == null)
            return;
        for (int i = 0; i < atomRep.meshesGO.Count; i++) {
            if (show)
                atomRep.meshesGO[i].GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.On;
            else
                atomRep.meshesGO[i].GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
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
        int id = 0;
        foreach (GameObject go in atomRep.meshesGO) {
            go.GetComponent<MeshFilter>().sharedMesh.SetColors(atomRep.meshColors[go]);

            if (rtos != null && rtos.Count > 0) {
                rtos[id].shouldUpdateMeshColor = true;
            }
            id++;
        }
    }


    public override void SetColor(Color32 col, UnityMolSelection sele) {
        if (sele.atoms.Count == atomRep.selection.atoms.Count) {
            foreach (GameObject go in atomRep.meshesGO) {
                Color32[] colors = atomRep.meshColors[go];
                for (int i = 0; i < colors.Length; i++) {
                    colors[i] = col;
                }
            }

            for (int i = 0; i < sele.atoms.Count; i++) {
                atomRep.colorByAtom[i] = col;
            }
        }
        else {
            //Optimization for color by chain
            string selmda = sele.MDASelString;
            UnityMolChain chain = sele.atoms[0].residue.chain;
            if (selmda == chain.ToSelectionMDA() && sele.Count == chain.Count) {
                GameObject go = null;
                Color32 newCol = col;
                int subSel = 0;
                if (atomRep.chainToGo.TryGetValue(chain, out go)) {
                    subSel = atomRep.chainToIdSubSel[chain];
                    Color32[] colors = atomRep.meshColors[go];
                    for (int i = 0; i < colors.Length; i++) {
                        colors[i] = newCol;
                    }
                }
                ApplyColors();

                //Fill color by atom
                int offset = 0;
                for (int i = 0; i < subSel; i++) {
                    offset += atomRep.subSelections[i].Count;
                }

                for (int i = 0; i < atomRep.subSelections[subSel].Count; i++)
                    atomRep.colorByAtom[offset + i] = col;

                return;
            }

            //DX surface and no correspondance between atoms and vertices
            if (!atomRep.isStandardSurface && atomRep.vertToAtom.Count == 0) {
                atomRep.computeNearestVertexPerAtom(atomRep.meshesGO.Last(), atomRep.subSelections.Last());
            }
            foreach (UnityMolAtom a in sele.atoms) {
                SetColor(col, a, false);
            }
        }
        ApplyColors();
    }

    public override void SetColor(Color32 col, UnityMolAtom a) {
        //DX surface and no correspondance between atoms and vertices
        if (!atomRep.isStandardSurface && atomRep.vertToAtom.Count == 0) {
            atomRep.computeNearestVertexPerAtom(atomRep.meshesGO.Last(), atomRep.subSelections.Last());
        }

        GameObject go = null;
        if (atomRep.chainToGo.TryGetValue(a.residue.chain, out go)) {
            if (go != null) {
                int subSel = atomRep.chainToIdSubSel[a.residue.chain];
                UnityMolSelection sel = atomRep.subSelections[subSel];

                //TODO: Improve this, it can be slow
                int idA = sel.atoms.IndexOf(a);
                if (idA == -1)
                    return;

                Color32[] colors = atomRep.meshColors[go];

                Color32 newCol = col;
                int[] vtoa = atomRep.vertToAtom[subSel];

                //Loop over all the vertices and set the vertices associated with our atom
                for (int i = 0; i < vtoa.Length; i++) {
                    if (vtoa[i] == idA) {
                        colors[i] = newCol;
                    }
                }
                atomRep.colorByAtom[idA] = newCol;
            }
        }

        ApplyColors();
    }

    public void SetColor(Color32 col, UnityMolAtom a, bool applyNow) {

        //DX surface and no correspondance between atoms and vertices
        if (!atomRep.isStandardSurface && atomRep.vertToAtom.Count == 0) {
            atomRep.computeNearestVertexPerAtom(atomRep.meshesGO.Last(), atomRep.subSelections.Last());
        }

        if (applyNow) {
            SetColor(col, a);
            return;
        }
        GameObject go = null;
        if (atomRep.chainToGo.TryGetValue(a.residue.chain, out go)) {
            if (go != null) {
                int subSel = atomRep.chainToIdSubSel[a.residue.chain];
                UnityMolSelection sel = atomRep.subSelections[subSel];

                //TODO: Improve this, it can be slow
                int idA = sel.atoms.IndexOf(a);
                if (idA == -1)
                    return;

                Color32[] colors = atomRep.meshColors[go];

                Color32 newCol = col;
                int[] vtoa = atomRep.vertToAtom[subSel];

                //Loop over all the vertices and set the vertices associated with our atom
                for (int i = 0; i < vtoa.Length; i++) {
                    if (vtoa[i] == idA) {
                        colors[i] = newCol;
                    }
                }
                atomRep.colorByAtom[idA] = newCol;
            }
        }
    }
    public void ClearAO() {
        foreach (GameObject go in atomRep.meshesGO) {
            Mesh m = go.GetComponent<MeshFilter>().sharedMesh;
            m.uv2 = null;
            go.GetComponent<Renderer>().sharedMaterial.SetFloat("_AOIntensity", 0.0f);
        }
        AOOn = false;
    }

    public void DoAO() {
        if (atomRep.meshesGO.Count > 0) {
            try {
                GameObject aoGo = new GameObject("tmpAO");
                geoAO aoScript = aoGo.AddComponent<geoAO>();
                aoScript.ComputeAO(atomRep.meshesGO);
                GameObject.Destroy(aoGo);
                AOOn = true;
                foreach (GameObject go in atomRep.meshesGO) {
                    go.GetComponent<Renderer>().sharedMaterial.SetFloat("_AOIntensity", 8.0f);
                }
            }
            catch {
                Debug.LogWarning("Could not compute AO");
            }
        }
    }

    public override void SetColors(Color32 col, List<UnityMolAtom> atoms) {
        foreach (UnityMolAtom a in atoms) {
            SetColor(col, a, false);
        }
        ApplyColors();
        if (atoms.Count == atomRep.selection.Count)
            atomRep.colorationType = colorType.full;
    }

    public override void SetColors(List<Color32> cols, List<UnityMolAtom> atoms) {
        if (atoms.Count != cols.Count) {
            Debug.LogError("Lengths of color list and atom list are different");
            return;
        }
        //Optimization when we can just copy the array of colors
        if (atoms.Count == atomRep.selection.Count) {
            int offset = 0;
            for (int i = 0; i < atomRep.meshesGO.Count; i++) {
                GameObject go = atomRep.meshesGO[i];
                int[] vtoa = atomRep.vertToAtom[i];
                Color32[] mcolors = atomRep.meshColors[go];
                for (int j = 0; j < mcolors.Length; j++) {
                    int idA = vtoa[j];
                    if (idA + offset < cols.Count) {
                        Color32 newc = cols[idA + offset];
                        mcolors[j] = newc;
                        atomRep.colorByAtom[idA + offset] = newc;
                    }
                }

                offset += atomRep.subSelections[i].Count;
            }
        }
        else {
            for (int i = 0; i < atoms.Count; i++) {
                UnityMolAtom a = atoms[i];
                Color32 col = cols[i];
                SetColor(col, a, false);
            }
        }

        atomRep.colorationType = colorType.custom;

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
        //DX surface and no correspondance between atoms and vertices
        if (!atomRep.isStandardSurface && atomRep.vertToAtom.Count == 0) {
            atomRep.computeNearestVertexPerAtom(atomRep.meshesGO.Last(), atomRep.subSelections.Last());
        }

        //If only one structure in selection and a DX map was loaded for this structure
        if (atomRep.selection.structures.Count == 1 && atomRep.selection.structures[0].dxr != null) {
            DXReader dxr = atomRep.selection.structures[0].dxr;
            foreach (GameObject go in atomRep.meshesGO) {
                Mesh m = go.GetComponent<MeshFilter>().sharedMesh;
                Color32[] colors = m.colors32;
                Vector3[] verts = m.vertices;
                for (int i = 0; i < colors.Length; i++) {
                    Color32 col;

                    Vector3 v = verts[i];
                    float val = dxr.getValueAtPosition(v);
                    if (normalizeDens) {
                        col = dxDensityToColor32(val, dxr.minDensityVal, dxr.maxDensityVal);
                    }
                    else {
                        col = dxDensityToColor32(val, minDens, maxDens);
                    }

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
        if (atomRep.meshesGO == null)
            return;
        foreach (GameObject meshGO in atomRep.meshesGO) {
            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_FogStart", v);
        }
    }

    public override void SetDepthCueingDensity(float v) {
        if (atomRep.meshesGO == null)
            return;
        foreach (GameObject meshGO in atomRep.meshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_FogDensity", v);
        }
    }

    public override void EnableDepthCueing() {
        if (atomRep.meshesGO == null)
            return;
        foreach (GameObject meshGO in atomRep.meshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_UseFog", 1.0f);
        }
    }

    public override void DisableDepthCueing() {
        if (atomRep.meshesGO == null)
            return;
        foreach (GameObject meshGO in atomRep.meshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_UseFog", 0.0f);
        }
    }

    /// <summary>
    /// Surface representations with trajectory can be super slow !
    /// </summary>
    public override void updateWithTrajectory() {
        // Debug.LogWarning("Deactivating surface representation when reading trajectory = too slow");
        // DisableRenderers();

        needUpdate = true;
        bool wasEnabled = true;
        if (atomRep.meshesGO != null && atomRep.meshesGO.Count >= 1)
            wasEnabled = atomRep.meshesGO[0].GetComponent<Renderer>().enabled;

        if (wasEnabled) {
            DoRecompute(true);
        }
    }

    public override void updateWithModel() {
        needUpdate = true;
        bool wasEnabled = true;
        if (atomRep.meshesGO != null && atomRep.meshesGO.Count >= 1)
            wasEnabled = atomRep.meshesGO[0].GetComponent<Renderer>().enabled;

        if (wasEnabled) {
            DoRecompute(false);
        }
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
        if (isWireframe && !force) {
            int i = 0;
            foreach (GameObject go in atomRep.meshesGO) {
                Material[] mats = go.GetComponent<Renderer>().sharedMaterials;
                mats[0] = atomRep.normalMat;
                // if (mats.Length == 2)
                //     mats[1] = null;
                go.GetComponent<Renderer>().sharedMaterials = mats;
                if (isTransparent)
                    go.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.On;
                i++;
            }
            atomRep.currentMat = atomRep.normalMat;
            isWireframe = false;
        }
        else if (!isWireframe) {
            if (atomRep.wireMat == null)
                atomRep.wireMat = new Material(Shader.Find("UMol/WireframeBakedBarycentricCoordinates"));
            foreach (GameObject go in atomRep.meshesGO) {
                Material[] mats = go.GetComponent<Renderer>().sharedMaterials;
                //For some reason, wireframe does not accept to have a second material even when null
                // if (mats.Length == 2) {
                //     Material[] newMats = new Material[1];
                //     mats = newMats;
                // }
                mats[0] = atomRep.wireMat;

                go.GetComponent<Renderer>().sharedMaterials = mats;
            }
            atomRep.currentMat = atomRep.wireMat;
            isWireframe = true;
        }


        isTransparent = false;
        if (limitedView) {
            activateLimitedView();
        }
        else {
            disableLimitedView();
        }
        if (UnityMolMain.isFogOn) {
            EnableDepthCueing();
            SetDepthCueingStart(UnityMolMain.fogStart);
            SetDepthCueingDensity(UnityMolMain.fogDensity);
        }
        else {
            DisableDepthCueing();
        }

    }
    public void SwitchTransparent(bool force = false) {

        if (isTransparent && !force) {
            int i = 0;
            foreach (GameObject go in atomRep.meshesGO) {
                Material[] mats = go.GetComponent<Renderer>().sharedMaterials;
                mats[0] = atomRep.normalMat;
                // if (mats.Length == 2) {
                //     mats[1] = null;
                // }
                go.GetComponent<Renderer>().sharedMaterials = mats;
                go.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.On;

                i++;
            }
            isTransparent = false;
            atomRep.currentMat = atomRep.normalMat;
        }
        else {
            if (atomRep.transMat == null) {
                atomRep.transMat = new Material(Shader.Find("Custom/SurfaceVertexColorTransparent"));
                // atomRep.transMat.SetTexture("_DitherPattern", Resources.Load("Images/BayerDither8x8") as Texture2D);
            }

            foreach (GameObject go in atomRep.meshesGO) {
                Material[] mats = go.GetComponent<Renderer>().sharedMaterials;
                // if (mats.Length != 2) {
                //     Material[] newMats = new Material[2];
                //     mats = newMats;
                // }
                mats[0] = atomRep.transMat;
                // mats[1] = atomRep.transMatShadow;

                go.GetComponent<Renderer>().sharedMaterials = mats;
                go.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            }
            isTransparent = true;
            atomRep.currentMat = atomRep.transMat;
        }

        isWireframe = false;
        if (limitedView) {
            activateLimitedView();
        }
        else {
            disableLimitedView();
        }

        if (UnityMolMain.isFogOn) {
            EnableDepthCueing();
            SetDepthCueingStart(UnityMolMain.fogStart);
            SetDepthCueingDensity(UnityMolMain.fogDensity);
        }
        else {
            DisableDepthCueing();
        }

    }

    public override void HighlightRepresentation() {
        // foreach (GameObject meshGO in atomRep.meshesGO) {

        //     Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;

        //     if (mats.Length != 2) {
        //         Material[] newMats = new Material[2];
        //         newMats[0] = mats[0];
        //         newMats[1] = highlightMat;
        //         mats = newMats;
        //         meshGO.GetComponent<Renderer>().sharedMaterials = newMats;
        //     }
        // }

    }


    public override void DeHighlightRepresentation() {

        // foreach (GameObject meshGO in atomRep.meshesGO) {

        //     Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;

        //     if (mats.Length != 1) {
        //         Material[] newMats = new Material[1];
        //         newMats[0] = mats[0];
        //         meshGO.GetComponent<Renderer>().sharedMaterials = newMats;
        //     }
        // }
    }
    public override void SetSmoothness(float val) {
        if (!isWireframe) {
            foreach (GameObject meshGO in atomRep.meshesGO) {

                Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
                mats[0].SetFloat("_Glossiness", val);
            }
        }
    }
    public override void SetMetal(float val) {
        if (!isWireframe) {
            foreach (GameObject meshGO in atomRep.meshesGO) {

                Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
                mats[0].SetFloat("_Metallic", val);
            }
        }
    }

    public void SetAlpha(float alpha) {
        if (isTransparent) {
            foreach (GameObject meshGO in atomRep.meshesGO) {

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
        atomRep.wireMat.SetFloat("_Thickness", s);
        curWireSize = s;
    }

    public void activateLimitedView() {
        foreach (GameObject meshGO in atomRep.meshesGO) {
            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_LimitedView", 1.0f);
            if (mats.Length == 2 && mats[1] != null)
                mats[1].SetFloat("_LimitedView", 1.0f);
        }
        limitedView = true;
        setLimitedViewRadius(limitedViewRadius);
        setLimitedViewCenter(limitedViewCenter);
        savedShadowBeforeLimited = (atomRep.meshesGO[0].GetComponent<Renderer>().shadowCastingMode == ShadowCastingMode.On);
        ShowShadows(false);

    }
    public void disableLimitedView() {
        foreach (GameObject meshGO in atomRep.meshesGO) {
            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_LimitedView", 0.0f);
            if (mats.Length == 2 && mats[1] != null)
                mats[1].SetFloat("_LimitedView", 0.0f);
        }
        limitedView = false;
        if (!isTransparent)
            ShowShadows(savedShadowBeforeLimited);
    }
    public void setLimitedViewRadius(float newRadius) {
        foreach (GameObject meshGO in atomRep.meshesGO) {
            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            try {
                mats[0].SetFloat("_LimitedViewRadius", newRadius);
                if (mats.Length == 2 && mats[1] != null)
                    mats[1].SetFloat("_LimitedViewRadius", newRadius);
            }
            catch {
            }
        }
        limitedViewRadius = newRadius;
    }
    public void setLimitedViewCenter(Vector3 newCenter) {
        foreach (GameObject meshGO in atomRep.meshesGO) {
            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            try {
                mats[0].SetVector("_LimitedViewCenter", newCenter);
                if (mats.Length == 2 && mats[1] != null)
                    mats[1].SetVector("_LimitedViewCenter", newCenter);
            }
            catch {
            }
        }
        limitedViewCenter = newCenter;
    }

    public override void UpdateLike() {
    }

    public override UnityMolRepresentationParameters Save() {

        UnityMolRepresentationParameters res = new UnityMolRepresentationParameters();
        res.repT.atomType = AtomType.surface;
        res.repT.bondType = BondType.nobond;
        res.colorationType = atomRep.colorationType;

        res.surfLimitedView = limitedView;
        res.surfLimitedViewRadius = limitedViewRadius;
        res.surfLimitedViewCenter = limitedViewCenter;

        if (atomRep.isStandardSurface) {
            res.surfCutSurface = ((SurfaceRepresentation)atomRep).IsCutSurface;
            res.surfMethod = ((SurfaceRepresentation)atomRep).SurfMethod;
            res.surfCutByChain = ((SurfaceRepresentation)atomRep).IsCutByChain;
        }

        if (res.colorationType == colorType.custom) {
            //Need a light atom comparer to avoid indexing with unique atom serial here
            res.colorPerAtom = new Dictionary<UnityMolAtom, Color32>(atomRep.selection.atoms.Count, new LightAtomComparer());
            for (int i = 0; i < atomRep.selection.atoms.Count; i++) {
                UnityMolAtom a = atomRep.selection.atoms[i];
                res.colorPerAtom[a] = atomRep.colorByAtom[i];
            }
        }
        else if (res.colorationType == colorType.full) { //Get color of first atom/residue
            res.fullColor = atomRep.meshColors[atomRep.meshesGO[0]][0];
        }
        else if (res.colorationType == colorType.bfactor) {
            res.bfactorStartColor = atomRep.bfactorStartCol;
            res.bfactorMidColor = atomRep.bfactorMidColor;
            res.bfactorEndColor = atomRep.bfactorEndCol;
        }


        if (!isTransparent && !isWireframe) {
            res.smoothness = atomRep.meshesGO[0].GetComponent<Renderer>().sharedMaterials[0].GetFloat("_Glossiness");
            res.metal = atomRep.meshesGO[0].GetComponent<Renderer>().sharedMaterials[0].GetFloat("_Metallic");
        }
        res.shadow = (atomRep.meshesGO[0].GetComponent<Renderer>().shadowCastingMode == ShadowCastingMode.On);
        res.surfAlpha = curAlpha;
        res.surfWireframeSize = curWireSize;
        res.surfIsTransparent = isTransparent;
        res.surfIsWireframe = isWireframe;

        return res;
    }

    public override void Restore(UnityMolRepresentationParameters savedParams) {

        //TODO: improve this because it can recompute several times the surface !

        if (atomRep.isStandardSurface && !savedParams.surfCutByChain) {
            ((SurfaceRepresentation)atomRep).IsCutByChain = savedParams.surfCutByChain;
        }

        if (atomRep.isStandardSurface && savedParams.surfMethod != SurfMethod.EDTSurf) {
            SwitchComputeMethod();
        }
        if (atomRep.isStandardSurface && !savedParams.surfCutSurface) {
            SwitchCutSurface(false);
        }

        if (savedParams.repT.atomType == AtomType.surface && savedParams.repT.bondType == BondType.nobond) {
            if (savedParams.colorationType == colorType.full) {
                SetColor(savedParams.fullColor, atomRep.selection);
            }
            else if (savedParams.colorationType == colorType.custom) {
                atomRep.restoreColorsPerAtom();
            }
            else if (savedParams.colorationType == colorType.atom) {
                colorByAtom(atomRep.selection);
            } else if (savedParams.colorationType == colorType.res) {
                colorByRes(atomRep.selection);
            } else if (savedParams.colorationType == colorType.chain) {
                colorByChain(atomRep.selection);
            } else if (savedParams.colorationType == colorType.hydro) {
                colorByHydro(atomRep.selection);
            } else if (savedParams.colorationType == colorType.seq) {
                colorBySequence(atomRep.selection);
            } else if (savedParams.colorationType == colorType.charge) {
                colorByCharge(atomRep.selection);
            } else if (savedParams.colorationType == colorType.restype) {
                colorByResType(atomRep.selection);
            } else if (savedParams.colorationType == colorType.rescharge) {
                colorByResCharge(atomRep.selection);
            } else if (savedParams.colorationType == colorType.resid) {
                colorByResid(atomRep.selection);
            } else if (savedParams.colorationType == colorType.resnum) {
                colorByResnum(atomRep.selection);
            } else if (savedParams.colorationType == colorType.bfactor) {
                colorByBfactor(atomRep.selection, savedParams.bfactorStartColor, savedParams.bfactorMidColor, savedParams.bfactorEndColor);
            }

            atomRep.colorationType = savedParams.colorationType;

            if (!AOOn)
                ClearAO();
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
            if (UnityMolMain.isFogOn) {
                EnableDepthCueing();
                SetDepthCueingStart(UnityMolMain.fogStart);
                SetDepthCueingDensity(UnityMolMain.fogDensity);
            }
            else {
                DisableDepthCueing();
            }
            limitedViewRadius = savedParams.surfLimitedViewRadius;
            limitedViewCenter = savedParams.surfLimitedViewCenter;
            if (savedParams.surfLimitedView)
                activateLimitedView();

        }
        else {
            Debug.LogError("Could not restore representation parameters");
        }
    }
}
}
