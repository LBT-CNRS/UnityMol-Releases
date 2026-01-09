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

namespace UMol {

public class UnityMolSugarRibbonsManager : UnityMolGenericRepresentationManager {

    private List<GameObject> meshesGO;

    public AtomRepresentationSugarRibbons atomRep;

    private bool needUpdate = false;
    private Material highlightMat;



    /// <summary>
    /// Disables the renderers for all objects managed by the instance of the manager.
    /// </summary>
    public override void DisableRenderers() {
        isEnabled = false;
        if (meshesGO == null)
            return;
        for (int i = 0; i < meshesGO.Count; i++) {
            meshesGO[i].GetComponent<Renderer>().enabled = false;
        }
        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();
    }

    /// <summary>
    /// Enables the renderers for all objects managed by the instance of the manager.
    /// </summary>
    public override void EnableRenderers() {
        isEnabled = true;
        if (meshesGO == null)
            return;
        if (needUpdate) {
            atomRep.recompute();
            needUpdate = false;
        }
        for (int i = 0; i < meshesGO.Count; i++) {
            meshesGO[i].GetComponent<Renderer>().enabled = true;
        }
        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();
    }

    /// <summary>
    /// Initializes this instance of the manager.
    /// </summary>
    public override void Init(SubRepresentation umolRep) {

        if (isInit) {
            return;
        }

        atomRep = (AtomRepresentationSugarRibbons) umolRep.atomRep;
        meshesGO = atomRep.meshesGO;
        highlightMat = (Material) Resources.Load("Materials/HighlightMaterial");


        isInit = true;
        isEnabled = true;
        areSideChainsOn = true;
        areHydrogensOn = true;
        isBackboneOn = true;
    }

    public override void InitRT() {
    }

    public override void Clean() {
        GameObject parent = null;
        if (meshesGO.Count != 0) {
            parent = meshesGO[0].transform.parent.gameObject;
        }

        if (meshesGO != null) {
            for (int i = 0; i < meshesGO.Count; i++) {
                GameObject.Destroy(meshesGO[i].GetComponent<MeshFilter>().sharedMesh);
                GameObject.Destroy(meshesGO[i]);
            }
            meshesGO.Clear();
        }
        if (parent != null) {
            GameObject.Destroy(parent);
        }


        meshesGO = null;
        atomRep = null;
        isEnabled = false;
        isInit = false;
        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();
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
    }//Does not really make sense for sugarRibbons

    public override void ShowSideChains(bool show) {
        areSideChainsOn = show;
    }//Does not really make sense for sugarRibbons

    public override void ShowBackbone(bool show) {
        isBackboneOn = show;
    }//Does not really make sense for sugarRibbons

    public override void SetColor(Color32 col, UnityMolSelection sele) {
        SetColors(col, sele.atoms);
    }

    public override void SetColor(Color32 col, UnityMolAtom atom) {//Does not really make sense for atoms
        try {

            if (atomRep.atomToVertBB.ContainsKey(atom)) {
                Color32[] cols = atomRep.meshesGO[1].GetComponent<MeshFilter>().sharedMesh.colors32;
                foreach (int vid in atomRep.atomToVertBB[atom]) {
                    cols[vid] = col;
                }
                atomRep.meshesGO[1].GetComponent<MeshFilter>().sharedMesh.colors32 = cols;
            }
        }
        catch {
            Debug.LogError("Could not find atom " + atom + " in this representation");
        }

    }

    public override void SetColors(Color32 col, List<UnityMolAtom> atoms) {
        Color32[] cols = atomRep.meshesGO[1].GetComponent<MeshFilter>().sharedMesh.colors32;
        foreach (UnityMolAtom a in atoms) {
            if (atomRep.atomToVertBB.ContainsKey(a)) {
                foreach (int vid in atomRep.atomToVertBB[a]) {
                    cols[vid] = col;
                }
            }
        }
        atomRep.meshesGO[1].GetComponent<MeshFilter>().sharedMesh.colors32 = cols;

    }

    public override void SetColors(List<Color32> cols, List<UnityMolAtom> atoms) {
        if (atoms.Count != cols.Count) {
            Debug.LogError("Lengths of color list and atom list are different");
            return;
        }
        for (int i = 0; i < atoms.Count; i++) {
            UnityMolAtom a = atoms[i];
            Color32 col = cols[i];
            SetColor(col, a);
        }
    }


    public override void SetDepthCueingStart(float v) {
        if (meshesGO != null && meshesGO.Count >= 1) {
            Material[] mats = meshesGO[1].GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_FogStart", v);
        }
    }

    public override void SetDepthCueingDensity(float v) {
        if (meshesGO != null && meshesGO.Count >= 1) {
            Material[] mats = meshesGO[1].GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_FogDensity", v);
        }
    }

    public override void EnableDepthCueing() {
        if (meshesGO != null && meshesGO.Count >= 1) {

            Material[] mats = meshesGO[1].GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_UseFog", 1.0f);
        }
    }

    public override void DisableDepthCueing() {
        if (meshesGO != null && meshesGO.Count >= 1) {
            Material[] mats = meshesGO[1].GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_UseFog", 0.0f);
        }
    }

    public override void updateWithTrajectory() {

        needUpdate = true;
        bool wasEnabled = true;
        if (meshesGO != null && meshesGO.Count >= 1)
            wasEnabled = meshesGO[1].GetComponent<Renderer>().enabled;

        if (wasEnabled) {
            atomRep.recompute();
            needUpdate = false;
        }
    }

    public override void updateWithModel() {
        needUpdate = true;
        bool wasEnabled = true;
        if (meshesGO != null && meshesGO.Count >= 1)
            wasEnabled = meshesGO[1].GetComponent<Renderer>().enabled;

        if (wasEnabled) {
            atomRep.recompute(isNewModel: true);
            needUpdate = false;
        }
    }

    public override void ShowAtom(UnityMolAtom atom, bool show) {
        Debug.LogWarning("Cannot show/hide one atom with the sugarRibbons representation");
    }

    public override void SetSize(UnityMolAtom atom, float size) {
        Debug.LogWarning("Cannot set the size of one atom with the sugarRibbons representation");
    }

    public override void SetSizes(List<UnityMolAtom> atoms, List<float> sizes) {
        Debug.LogWarning("Cannot set the size of atoms with the sugarRibbons representation");
    }

    public override void SetSizes(List<UnityMolAtom> atoms, float size) {
        Debug.LogWarning("Cannot set the size of atoms with the sugarRibbons representation");
    }
    public override void ResetSize(UnityMolAtom atom) {
        Debug.LogWarning("Cannot set the size of one atom with the sugarRibbons representation");
    }
    public override void ResetSizes() {
        Debug.LogWarning("Cannot set the size of atoms with the sugarRibbons representation");
    }

    public override void ResetColor(UnityMolAtom atom) {
        SetColor(atom.color32, atom);
    }
    public override void ResetColors() {

        foreach (UnityMolAtom atom in atomRep.selection.atoms) {
            ResetColor(atom);
        }
        // Mesh m = atomRep.meshesGO[1].GetComponent<MeshFilter>().sharedMesh;
        // Color32[] cols = m.colors32;
        // for (int i = 0; i < cols.Length; i++) {
        //     cols[i] = selection.atoms[i].color;
        // }
        // m.colors32 = cols;

    }

    public override void HighlightRepresentation() {
    }


    public override void DeHighlightRepresentation() {
    }

    public override void SetSmoothness(float val) {

        Material[] mats = meshesGO[1].GetComponent<Renderer>().sharedMaterials;
        mats[0].SetFloat("_Glossiness", val);
    }
    public override void SetMetal(float val) {

        Material[] mats = meshesGO[1].GetComponent<Renderer>().sharedMaterials;
        mats[0].SetFloat("_Metallic", val);
    }
    public override void UpdateLike() {
    }
    public override UnityMolRepresentationParameters Save() {
        UnityMolRepresentationParameters res = new UnityMolRepresentationParameters();
        return res;
    }
    public override void Restore(UnityMolRepresentationParameters savedParams) {

    }
}
}