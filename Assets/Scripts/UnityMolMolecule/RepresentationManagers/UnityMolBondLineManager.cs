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
using UnityEngine.Rendering;
using System.Linq;
using Unity.Mathematics;

namespace UMol {

public class UnityMolBondLineManager : UnityMolGenericRepresentationManager {
    private UnityMolRepresentation rep;
    private BondRepresentationLine bondRep;

    // private Material highlightMat;

    /// <summary>
    /// Initializes this instance of the manager.
    /// </summary>
    public override void Init(SubRepresentation umolRep) {

        if (isInit) {
            return;
        }

        bondRep = (BondRepresentationLine) umolRep.bondRep;

        if (UnityMolMain.raytracingMode)
            InitRT();

        isInit = true;
        isEnabled = true;
        areSideChainsOn = true;
        areHydrogensOn = true;
        isBackboneOn = true;
    }

    public override void InitRT() {
        if (bondRep.meshGO != null && rtos == null) {
            RaytracedObject rto  = bondRep.meshGO.AddComponent<RaytracedObject>();
            rtos = new List<RaytracedObject>() {rto};
        }
    }

    public override void Clean() {

        if (rtos != null) {
            rtos.Clear();
        }

        if (bondRep.meshGO != null) {
            GameObject.Destroy(bondRep.meshGO.GetComponent<MeshFilter>().sharedMesh);
            if (bondRep.meshGO.transform.parent != null)
                GameObject.Destroy(bondRep.meshGO.transform.parent.gameObject);
        }

        if (bondRep.atomToMeshVertex != null) {
            bondRep.atomToMeshVertex.Clear();
        }


        isEnabled = false;
        bondRep.curMesh = null;
        bondRep = null;
        isInit = false;
        areSideChainsOn = true;
        areHydrogensOn = true;
        isBackboneOn = true;

        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();
    }
    /// <summary>
    /// Disables the renderers for all objects managed by the instance of the manager.
    /// </summary>

    public override void DisableRenderers() {
        if (bondRep.meshGO != null) {
            bondRep.meshGO.GetComponent<Renderer>().enabled = false;
        }
        isEnabled = false;
        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();
    }
    /// <summary>
    /// Enables the renderers for all objects managed by the instance of the manager.
    /// </summary>
    public override void EnableRenderers() {
        if (bondRep.meshGO != null) {
            bondRep.meshGO.GetComponent<Renderer>().enabled = true;
        }
        isEnabled = true;
        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();
    }

    public override void ShowHydrogens(bool show) {
        if (bondRep.curMesh == null)
            return;

        HashSet<UnityMolAtom> toHide = new HashSet<UnityMolAtom>();

        UnityMolModel curM = bondRep.selection.atoms[0].residue.chain.model;

        foreach (int2 duo in bondRep.displayedBonds) {

            UnityMolAtom atom1 = curM.allAtoms[duo.x];
            UnityMolAtom atom2 = curM.allAtoms[duo.y];

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
        if (bondRep.curMesh == null)
            return;

        HashSet<UnityMolAtom> toHide = new HashSet<UnityMolAtom>();

        UnityMolModel curM = bondRep.selection.atoms[0].residue.chain.model;

        foreach (int2 duo in bondRep.displayedBonds) {

            UnityMolAtom atom1 = curM.allAtoms[duo.x];
            UnityMolAtom atom2 = curM.allAtoms[duo.y];

            if (MDAnalysisSelection.isSideChain(atom1, bondRep.selection.bonds)) {
                if (atom1.type == "H") {
                    if (show && areHydrogensOn)
                        toHide.Add(atom1);
                }
                else
                    toHide.Add(atom1);
            }
            if (MDAnalysisSelection.isSideChain(atom2, bondRep.selection.bonds)) {
                if (atom2.type == "H") {
                    if (show && areHydrogensOn)
                        toHide.Add(atom2);
                }
                else
                    toHide.Add(atom2);
            }
        }

        ShowAtoms(toHide, show);
        areSideChainsOn = show;

        RaytracedObject rto = bondRep.meshGO.GetComponent<RaytracedObject>();
        RaytracingMaterial savedrtMat = rto.rtMat;
        GameObject.Destroy(rto);
        RaytracedObject nrto = bondRep.meshGO.AddComponent<RaytracedObject>();
        nrto.rtMat = savedrtMat;
        rtos = new List<RaytracedObject>() {nrto};
    }

    public override void ShowBackbone(bool show) {
        if (bondRep.curMesh == null)
            return;
        HashSet<UnityMolAtom> toHide = new HashSet<UnityMolAtom>();

        UnityMolModel m = bondRep.selection.atoms[0].residue.chain.model;
        foreach (int2 duo in bondRep.displayedBonds) {

            UnityMolAtom atom1 = m.allAtoms[duo.x];
            UnityMolAtom atom2 = m.allAtoms[duo.y];

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


    public override void SetColor(Color32 col, UnityMolSelection sele) {
        SetColors(col, sele.atoms);
    }

    public override void SetColor(Color32 col, UnityMolAtom a) {
        if (bondRep.curMesh == null)
            return;
        try {
            Color32 newCol = col;

            foreach (int vid in bondRep.atomToMeshVertex[a]) {
                bondRep.colors[vid] = newCol;
            }

            if (rtos != null && rtos.Count > 0) {
                rtos[0].shouldUpdateMeshColor = true;
            }
            bondRep.curMesh.SetColors(bondRep.colors);
        }
        catch {
            Debug.LogError("Could not find atom " + a + " in this representation");
        }
    }
    public override void SetColors(Color32 col, List<UnityMolAtom> atoms) {
        if (bondRep.curMesh == null)
            return;
        try {
            Color32 newCol = col;
            foreach (UnityMolAtom a in atoms) {
                List<int> verts = null;
                if (bondRep.atomToMeshVertex.TryGetValue(a, out verts)) {
                    foreach (int vid in verts) {
                        bondRep.colors[vid] = newCol;
                    }
                }
            }

            if (rtos != null && rtos.Count > 0) {
                rtos[0].shouldUpdateMeshColor = true;
            }

            bondRep.curMesh.SetColors(bondRep.colors);
        }
        catch (System.Exception e) {
            Debug.LogError("Failed to set the color of the line representation " + e);
        }
    }

    public override void SetColors(List<Color32> cols, List<UnityMolAtom> atoms) {
        if (bondRep.curMesh == null)
            return;
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
        if (bondRep.meshGO == null)
            return;

        Material mat = bondRep.meshGO.GetComponent<Renderer>().sharedMaterial;
        mat.SetFloat("_FogStart", v);

    }

    public override void SetDepthCueingDensity(float v) {
        if (bondRep.meshGO == null)
            return;

        Material mat = bondRep.meshGO.GetComponent<Renderer>().sharedMaterial;
        mat.SetFloat("_FogDensity", v);
    }

    public override void EnableDepthCueing() {
        if (bondRep.meshGO == null)
            return;
        Material mat = bondRep.meshGO.GetComponent<Renderer>().sharedMaterial;
        mat.SetFloat("_UseFog", 1.0f);
    }

    public override void DisableDepthCueing() {
        if (bondRep.meshGO == null)
            return;
        Material mat = bondRep.meshGO.GetComponent<Renderer>().sharedMaterial;
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
        if (bondRep.curMesh == null) {
            return;
        }
        //Do not need to clear everything, just recompute the vertex array

        bondRep.recompute();

        if (rtos != null && rtos.Count > 0) {
            RaytracingMaterial savedrtMat = rtos[0].rtMat;
            GameObject.Destroy(rtos[0]);
            RaytracedObject rto = bondRep.meshGO.AddComponent<RaytracedObject>();
            rto.rtMat = savedrtMat;
            rtos = new List<RaytracedObject>() {rto};
        }
    }

    public override void updateWithModel() {
        updateWithTrajectory();
    }

    public void ShowAtoms(HashSet<UnityMolAtom> atoms, bool show) {
        if (bondRep.curMesh == null) {
            return;
        }

        foreach (UnityMolAtom a in atoms) {
            ShowAtom(a, show);
        }

        //TODO fix for raytracing
        // if (rtos != null && rtos.Count > 0) {
        //     RaytracingMaterial savedrtMat = rtos[0].rtMat;
        //     GameObject.Destroy(rtos[0]);
        //     RaytracedObject rto = bondRep.meshGO.AddComponent<RaytracedObject>();
        //     rto.rtMat = savedrtMat;
        //     rtos = new List<RaytracedObject>(){rto};
        // }

        // bondRep.curMesh.SetVertices(bondRep.vertices);

        bondRep.curMesh.uv2 = bondRep.uvs;

    }

    public void SetWidth(float newWidth) {
        if (newWidth <= 0.0f || newWidth > 1.0f) {
            Debug.LogWarning("Line size should be between 0.0 and 1.0");
            return;
        }
        bondRep.lineWidth = newWidth;
        //Recompute the line representation
        bondRep.recompute();

        if (rtos != null && rtos.Count > 0) {
            RaytracingMaterial savedrtMat = rtos[0].rtMat;
            GameObject.Destroy(rtos[0]);
            RaytracedObject rto = bondRep.meshGO.AddComponent<RaytracedObject>();
            rto.rtMat = savedrtMat;
            rtos = new List<RaytracedObject>() {rto};
        }
    }

    ///Call bondRep.curMesh.uv2 = bondRep.uv2; to actually show/hide atom
    public override void ShowAtom(UnityMolAtom atom, bool show) {
        if (bondRep.curMesh == null) {
            return;
        }

        Vector2 v = Vector2.one;
        if (!show)
            v = Vector2.one * 2;

        List<int> ids = bondRep.atomToMeshVertex[atom];
        for (int i = 0; i < ids.Count; i++) {
            bondRep.uvs[ids[i]] = v;
        }

        //TODO fix for raytracing
        // if (rtos != null && rtos.Count > 0) {
        //     RaytracingMaterial savedrtMat = rtos[0].rtMat;
        //     GameObject.Destroy(rtos[0]);
        //     RaytracedObject rto = bondRep.meshGO.AddComponent<RaytracedObject>();
        //     rto.rtMat = savedrtMat;
        //     rtos = new List<RaytracedObject>(){rto};
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
        SetColor(atom.color32, atom);
    }
    public override void ResetColors() {
        if (bondRep.selection.Count == 0)
            return;

        UnityMolModel m = bondRep.selection.atoms[0].residue.chain.model;

        foreach (int ida in bondRep.selection.bonds.bonds.Keys) {
            UnityMolAtom a = m.allAtoms[ida];
            SetColor(a.color32, a);
        }
        bondRep.colorationType = colorType.atom;
    }

    public override void HighlightRepresentation() {
        // Material[] mats = bondRep.meshGO.GetComponent<Renderer>().sharedMaterials;

        // if (mats.Length != 2) {
        //     Material[] newMats = new Material[2];
        //     newMats[0] = mats[0];
        //     newMats[1] = highlightMat;
        //     mats = newMats;
        //     bondRep.meshGO.GetComponent<Renderer>().sharedMaterials = newMats;
        // }

    }


    public override void DeHighlightRepresentation() {
        // Material[] mats = bondRep.meshGO.GetComponent<Renderer>().sharedMaterials;

        // if (mats.Length != 1) {
        //     Material[] newMats = new Material[1];
        //     newMats[0] = mats[0];
        //     bondRep.meshGO.GetComponent<Renderer>().sharedMaterials = newMats;
        // }
    }
    public override void SetSmoothness(float val) {
        Debug.LogWarning("Cannot change this value for the line representation");
    }
    public override void SetMetal(float val) {
        Debug.LogWarning("Cannot change this value for the line representation");
    }

    public override void UpdateLike() {
    }

    public override UnityMolRepresentationParameters Save() {
        UnityMolRepresentationParameters res = new UnityMolRepresentationParameters();

        res.repT.bondType = BondType.line;
        res.colorationType = bondRep.colorationType;

        res.sideChainsOn = areSideChainsOn;
        res.hydrogensOn = areHydrogensOn;
        res.backboneOn = isBackboneOn;

        if (res.colorationType == colorType.custom) {
            res.colorPerAtom = new Dictionary<UnityMolAtom, Color32>(bondRep.atomToMeshVertex.Keys.Count);
            foreach (UnityMolAtom a in bondRep.atomToMeshVertex.Keys) {
                res.colorPerAtom[a] = bondRep.colors[bondRep.atomToMeshVertex[a][0]];
            }

        }
        else if (res.colorationType == colorType.full) { //Get color of first atom/residue
            foreach (UnityMolAtom a in bondRep.atomToMeshVertex.Keys) {
                res.fullColor = bondRep.colors[bondRep.atomToMeshVertex[a][0]];
                break;
            }
        }
        else if (res.colorationType == colorType.bfactor) {
            res.bfactorStartColor = bondRep.bfactorStartCol;
            res.bfactorMidColor = bondRep.bfactorMidColor;
            res.bfactorEndColor = bondRep.bfactorEndCol;
        }
        res.LineWidth = bondRep.lineWidth;
        return res;
    }
    public override void Restore(UnityMolRepresentationParameters savedParams) {
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
            else if (savedParams.colorationType == colorType.resnum) {
                colorByResnum(bondRep.selection);
            }
            else if (savedParams.colorationType == colorType.resid) {
                colorByResid(bondRep.selection);
            }
            else if (savedParams.colorationType == colorType.bfactor) {
                colorByBfactor(bondRep.selection, savedParams.bfactorStartColor, savedParams.bfactorMidColor, savedParams.bfactorEndColor);
            }
            bondRep.colorationType = savedParams.colorationType;

            SetWidth(savedParams.LineWidth);

            if (savedParams.sideChainsOn != areSideChainsOn)
                ShowSideChains(savedParams.sideChainsOn);
            if (savedParams.hydrogensOn != areHydrogensOn)
                ShowHydrogens(savedParams.hydrogensOn);
            if (savedParams.backboneOn != isBackboneOn)
                ShowBackbone(savedParams.backboneOn);
        }
        else {
            Debug.LogError("Could not restore representation parameters");
        }

    }
}
}
