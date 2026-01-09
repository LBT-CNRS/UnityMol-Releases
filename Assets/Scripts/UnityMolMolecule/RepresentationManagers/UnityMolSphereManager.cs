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

public class UnityMolSphereManager : UnityMolGenericRepresentationManager {

    private Dictionary<UnityMolAtom, int> atomToId;


    public bool isTransparent = false;
    public float curAlpha = 1.0f;
    private int nbAtoms;

    private List<GameObject> meshesGO;
    public AtomRepresentationSphere atomRep;

    private Material highlightMat;


    public override void Init(SubRepresentation umolRep) {

        if (isInit) {
            return;
        }

        atomRep = (AtomRepresentationSphere) umolRep.atomRep;
        meshesGO = atomRep.meshesGO;
        nbAtoms = atomRep.selection.atoms.Count;
        atomToId = atomRep.atomToId;
        highlightMat = (Material) Resources.Load("Materials/HighlightMaterial");

        isInit = true;
        isEnabled = true;
        areSideChainsOn = true;
        areHydrogensOn = true;
        isBackboneOn = true;
        isTransparent = false;
    }

    public override void InitRT() {
    }

    public override void Clean() {
        GameObject parent = null;
        if (meshesGO.Count != 0) {
            parent = meshesGO[0].transform.parent.gameObject;
        }

        for (int i = 0; i < meshesGO.Count; i++) {
#if UNITY_EDITOR
            GameObject.DestroyImmediate(meshesGO[i]);
            #else
            GameObject.Destroy(meshesGO[i]);
#endif
        }
        if (parent != null) {
#if UNITY_EDITOR
            GameObject.DestroyImmediate(parent);
#else
            GameObject.Destroy(parent);
#endif
        }

        nbAtoms = 0;
        meshesGO.Clear();
        atomToId.Clear();

        atomRep = null;
        meshesGO = null;
        atomToId = null;

        isInit = false;
        isEnabled = false;
        isTransparent = false;

        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();

    }

    /// <summary>
    /// Disables the renderers for all objects managed by the instance of the manager.
    /// </summary>
    public override void DisableRenderers() {
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
        for (int i = 0; i < meshesGO.Count; i++) {
            meshesGO[i].GetComponent<Renderer>().enabled = true;
        }
        isEnabled = true;
        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();

    }

    public override void ShowShadows(bool show) {
        for (int i = 0; i < meshesGO.Count; i++) {
            if (show) {
                meshesGO[i].GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.On;
                meshesGO[i].GetComponent<Renderer>().receiveShadows = true;
            }
            else {
                meshesGO[i].GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
                meshesGO[i].GetComponent<Renderer>().receiveShadows = false;
            }
        }
    }

    public override void ShowHydrogens(bool show) {
        for (int i = 0; i < nbAtoms; i++) {
            if (atomRep.selection.atoms[i].type == "H") {
                ShowAtom(atomRep.selection.atoms[i], show);
            }
        }
        areHydrogensOn = show;
    }
    public override void ShowSideChains(bool show) {
        for (int i = 0; i < nbAtoms; i++) {
            if (MDAnalysisSelection.isSideChain(atomRep.selection.atoms[i], atomRep.selection.bonds)) {
                ShowAtom(atomRep.selection.atoms[i], show);
            }
        }
        areSideChainsOn = show;
    }
    public override void ShowBackbone(bool show) {
        for (int i = 0; i < nbAtoms; i++) {
            if (MDAnalysisSelection.isBackBone(atomRep.selection.atoms[i], atomRep.selection.bonds)) {
                ShowAtom(atomRep.selection.atoms[i], show);
            }
        }
        isBackboneOn = show;
    }


    public void SwitchTransparent(bool force = false) {

        if (isTransparent && !force) {
            foreach (GameObject go in meshesGO) {
                go.GetComponent<Renderer>().sharedMaterial = atomRep.solidMat;
            }
            isTransparent = false;
        }
        else {
            if (atomRep.transMat == null) {
                atomRep.transMat = new Material(Shader.Find("Custom/SurfaceVertexColorTransparent"));
                atomRep.transMat.SetTexture("_DitherPattern", Resources.Load("Images/BayerDither8x8") as Texture2D);
            }
            foreach (GameObject go in meshesGO) {
                go.GetComponent<Renderer>().sharedMaterial = atomRep.transMat;
            }
            isTransparent = true;
        }

        if (UnityMolMain.isFogOn) {
            EnableDepthCueing();
            SetDepthCueingStart(UnityMolMain.fogStart);
            SetDepthCueingDensity(UnityMolMain.fogDensity);
        } else {
            DisableDepthCueing();
        }

    }
    public void SetAlpha(float alpha) {
        if (isTransparent) {
            MaterialPropertyBlock props = new MaterialPropertyBlock();
            foreach (GameObject meshGO in meshesGO) {
                Renderer r = meshGO.GetComponent<Renderer>();
                r.GetPropertyBlock(props);
                Color col = props.GetColor("_Color");
                col.a = alpha;
                props.Clear();
                props.SetColor("_Color", col);
                r.SetPropertyBlock(props);
            }
            curAlpha = alpha;
        }
    }

    public override void SetColor(Color32 col, UnityMolSelection sele) {
        foreach (UnityMolAtom a in sele.atoms) {
            SetColor(col, a);
        }
    }

    public void SetColor(Color col, int atomNum) {
        MaterialPropertyBlock props = new MaterialPropertyBlock();
        Renderer r = meshesGO[atomNum].GetComponent<Renderer>();
        r.GetPropertyBlock(props);
        Color oldcol = props.GetColor("_Color");
        col.a = oldcol.a;//keep alpha
        props.Clear();
        props.SetColor("_Color", col);
        r.SetPropertyBlock(props);
    }

    public override void SetColor(Color32 col, UnityMolAtom atom) {
        int idInCoord = 0;
        if (atomToId.TryGetValue(atom, out idInCoord)) {
            SetColor(col, idInCoord);
        }
    }

    public override void SetColors(Color32 col, List<UnityMolAtom> atoms) {
        foreach (UnityMolAtom a in atoms) {
            SetColor(col, a);
        }
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

    public override void updateWithTrajectory() {
        for (int i = 0; i < nbAtoms; i++) {
            meshesGO[i].transform.localPosition = atomRep.selection.atoms[i].position;
        }
    }
    public override void updateWithModel() {
        updateWithTrajectory();
    }

    public override void ShowAtom(UnityMolAtom atom, bool show) {
        int idInCoord = 0;
        if (atomToId.TryGetValue(atom, out idInCoord)) {
            meshesGO[idInCoord].GetComponent<Renderer>().enabled = show;
        }
    }

    public override void SetSize(UnityMolAtom atom, float size) {
        int idInCoord = 0;
        if (atomToId.TryGetValue(atom, out idInCoord)) {
            meshesGO[idInCoord].transform.localScale = Vector3.one * 2 * size;
        }
    }

    public override void SetSizes(List<UnityMolAtom> atoms, List<float> sizes) {
        int i = 0;
        foreach (UnityMolAtom a in atoms) {
            SetSize(a, sizes[i]);
            i++;
        }
    }

    public override void SetSizes(List<UnityMolAtom> atoms, float size) {
        foreach (UnityMolAtom a in atoms) {
            SetSize(a, size);
        }
    }


    public override void ResetSize(UnityMolAtom atom) {
        SetSize(atom, 1.0f);
    }

    public override void ResetSizes() {
        foreach (UnityMolAtom a in atomRep.selection.atoms) {
            ResetSize(a);
        }
    }

    public override void ResetColor(UnityMolAtom atom) {
        SetColor(atom.color32, atom);
    }

    public override void ResetColors() {
        foreach (UnityMolAtom a in atomRep.selection.atoms) {
            SetColor(a.color32, a);
        }
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
        foreach (GameObject meshGO in meshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_Glossiness", val);
        }
    }
    public override void SetMetal(float val) {
        foreach (GameObject meshGO in meshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_Metallic", val);
        }
    }
    public override void UpdateLike() {
    }
    public override UnityMolRepresentationParameters Save() {
        UnityMolRepresentationParameters res = new UnityMolRepresentationParameters();

        res.repT.atomType = AtomType.sphere;
        res.colorationType = atomRep.colorationType;

        if (res.colorationType == colorType.custom) {
            Debug.Log("Custom");
            int atomNum = 0;
            res.colorPerAtom = new Dictionary<UnityMolAtom, Color32>(atomRep.selection.atoms.Count);
            foreach (UnityMolAtom a in atomRep.selection.atoms) {
                Renderer r = meshesGO[atomNum].GetComponent<Renderer>();
                Color col = r.sharedMaterial.color;
                res.colorPerAtom[a] = col;
                atomNum++;
            }
        }
        else if (res.colorationType == colorType.full) { //Get color of first atom/residue
            Renderer r = meshesGO[0].GetComponent<Renderer>();
            MaterialPropertyBlock props = new MaterialPropertyBlock();
            r.GetPropertyBlock(props);
            Color col = props.GetColor("_Color");
            res.fullColor = col;
        }
        else if (res.colorationType == colorType.bfactor) {
            res.bfactorStartColor = atomRep.bfactorStartCol;
            res.bfactorMidColor = atomRep.bfactorMidColor;
            res.bfactorEndColor = atomRep.bfactorEndCol;
        }
        // res.shadow = (meshesGO[0].GetComponent<Renderer>().shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.On);
        // res.HBScale = lastScale;

        return res;
    }
    public override void Restore(UnityMolRepresentationParameters savedParams) {

        if (savedParams.repT.atomType == AtomType.sphere) {
            if (savedParams.colorationType == colorType.full) {
                SetColor(savedParams.fullColor, atomRep.selection);
            }
            else if (savedParams.colorationType == colorType.custom) {
                List<Color32> colors = new List<Color32>(atomRep.selection.atoms.Count);
                List<UnityMolAtom> restoredAtoms = new List<UnityMolAtom>(atomRep.selection.atoms.Count);
                foreach (UnityMolAtom a in atomRep.selection.atoms) {
                    if (savedParams.colorPerAtom.ContainsKey(a)) {
                        colors.Add(savedParams.colorPerAtom[a]);
                        restoredAtoms.Add(a);
                    }
                }
                SetColors(colors, restoredAtoms);
            }
            else if (savedParams.colorationType == colorType.defaultCartoon) {
                //Do nothing !
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
            else if (savedParams.colorationType == colorType.resid) {
                colorByResid(atomRep.selection);
            }
            else if (savedParams.colorationType == colorType.resnum) {
                colorByResnum(atomRep.selection);
            }
            else if (savedParams.colorationType == colorType.bfactor) {
                colorByBfactor(atomRep.selection, savedParams.bfactorStartColor, savedParams.bfactorMidColor, savedParams.bfactorEndColor);
            }

            // SetSizes(atomRep.selection.atoms, savedParams.SphereSize);
            atomRep.colorationType = savedParams.colorationType;
        }
        else {
            Debug.LogError("Could not restore representation parameters");
        }
    }
}
}
