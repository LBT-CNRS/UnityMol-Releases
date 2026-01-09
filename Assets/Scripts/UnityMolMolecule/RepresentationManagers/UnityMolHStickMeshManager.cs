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
public class UnityMolHStickMeshManager : UnityMolGenericRepresentationManager {
    private UnityMolRepresentation rep;
    public UnityMolHBallMeshManager hbmm;
    public BondRepresentationOptihs bondRep;

    private bool[] texturesToUpdate;
    public float shininess = 0.0f;
    public float shrink = 0.4f;
    public float scaleBond = 1.0f;
    public bool largeBB = false;
    public int idTex = 0;

    AtomDuo key = new AtomDuo(null, null);


    /// <summary>
    /// Initializes this instance of the manager.
    /// </summary>
    public override void Init(SubRepresentation umolRep) {

        if (isInit) {
            return;
        }

        bondRep = (BondRepresentationOptihs) umolRep.bondRep;
        if (bondRep.meshesGO.Count != 0) {
            texturesToUpdate = new bool[bondRep.paramTextures.Length];
        }
        else {
            texturesToUpdate = new bool[0];
        }

        isInit = true;
        isEnabled = true;
        areSideChainsOn = true;
        areHydrogensOn = true;
        isBackboneOn = true;

        shrink = 0.4f;

        if (UnityMolMain.raytracingMode)
            InitRT();
    }

    public override void InitRT() {
        if (rtos == null) {
            rtos = new List<RaytracedObject>();
            recreateRTObject();
        }
    }

    public override void Clean() {

        if (rtos != null) {
            rtos.Clear();
        }

        if (bondRep.meshesGO != null) {
            for (int i = 0; i < bondRep.meshesGO.Count; i++) {
                if (bondRep.meshesGO[i] != null) {
                    GameObject.Destroy(bondRep.meshesGO[i].GetComponent<MeshFilter>().sharedMesh);
                    GameObject.Destroy(bondRep.meshesGO[i]);
                }
            }
            bondRep.meshesGO.Clear();

        }

        if (bondRep.representationTransform != null) {
            GameObject.Destroy(bondRep.representationTransform.gameObject);
        }

        if (bondRep.coordStickTexture != null) {
            bondRep.coordStickTexture.Clear();
        }
        if (bondRep.atomToDuo != null) {
            bondRep.atomToDuo.Clear();
        }
        texturesToUpdate = null;
        if (bondRep.paramTextures != null) {
            for (int i = 0; i < bondRep.paramTextures.Length; i++) {
                GameObject.Destroy(bondRep.paramTextures[i]);
            }
        }

        bondRep.paramTextures = null;
        bondRep.coordStickTexture = null;
        bondRep.atomToDuo = null;
        bondRep.meshesGO = null;
        bondRep = null;

        isInit = false;
        isEnabled = false;

    }

    public void recreateRTObject() {
        if (rtos == null)
            return;
        for (int i = 0; i < rtos.Count; i++) {
            GameObject.Destroy(rtos[i].gameObject);
        }
        rtos.Clear();
        Mesh m = ExtractHyperballMesh.computeHBMesh(bondRep.selection.atoms,
                 bondRep.selection.bonds,
                 shrink, scaleBond, scaleBond, hbmm);
        GameObject expHB = new GameObject("HBRaytracedMesh");
        expHB.transform.parent = bondRep.representationTransform;
        expHB.transform.localScale = Vector3.one;
        expHB.transform.localPosition = Vector3.zero;
        expHB.transform.localRotation = Quaternion.identity;
        MeshFilter mf = expHB.AddComponent<MeshFilter>();
        mf.mesh = m;

        RaytracedObject rto = expHB.AddComponent<RaytracedObject>();
        rtos.Add(rto);
    }

    public void ApplyTextures() {
        if (bondRep.paramTextures != null) {
            for (int i = 0; i < bondRep.paramTextures.Length; i++) {
                if (texturesToUpdate[i]) {
                    bondRep.paramTextures[i].Apply(false, false);
                }
                texturesToUpdate[i] = false;
            }
        }
        if (UnityMolMain.raytracingMode)
            recreateRTObject();
    }

    /// <summary>
    /// Disables the renderers for all objects managed by the instance of the manager.
    /// </summary>
    public override void DisableRenderers() {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;

        for (int i = 0; i < bondRep.meshesGO.Count; i++) {
            bondRep.meshesGO[i].GetComponent<Renderer>().enabled = false;
        }
        if (rtos != null) {
            foreach (var rto in rtos)
                rto.showHide(false);
        }

        isEnabled = false;
        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();
    }

    /// <summary>
    /// Enables the renderers for all objects managed by the instance of the manager.
    /// </summary>
    public override void EnableRenderers() {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;
        for (int i = 0; i < bondRep.meshesGO.Count; i++) {
            bondRep.meshesGO[i].GetComponent<Renderer>().enabled = true;
        }
        if (rtos != null) {
            foreach (var rto in rtos)
                rto.showHide(true);
        }
        isEnabled = true;
        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();
    }

    /// <summary>
    /// Resets the positions of all atoms
    /// </summary>
    public void ResetPositions() {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;
        Vector4 atom1Pos = Vector4.zero;
        Vector4 atom2Pos = Vector4.zero;
        Vector4 offset = new Vector4(bondRep.offsetPos.x, bondRep.offsetPos.y, bondRep.offsetPos.z, 0.0f);
        int3 infoTex;

        UnityMolModel curM = null;

        foreach (AtomDuo d in bondRep.coordStickTexture.Keys) {
            UnityMolStructure s1 = d.a1.residue.chain.model.structure;
            UnityMolStructure s2 = d.a2.residue.chain.model.structure;

            UnityMolModel m1 = curM;
            UnityMolModel m2 = curM;

            if (curM == null || curM.structure != s1 || curM.structure != s2) {
                m1 = s1.currentModel;
                m2 = s2.currentModel;
                curM = m1;
            }

            UnityMolAtom atom1 = m1.allAtoms[d.a1.idInAllAtoms];
            UnityMolAtom atom2 = m2.allAtoms[d.a2.idInAllAtoms];

            atom1Pos.x = atom1.position.x;
            atom1Pos.y = atom1.position.y;
            atom1Pos.z = atom1.position.z;

            atom2Pos.x = atom2.position.x;
            atom2Pos.y = atom2.position.y;
            atom2Pos.z = atom2.position.z;

            atom1Pos += offset;
            atom2Pos += offset;

            infoTex = bondRep.coordStickTexture[d];

            bondRep.paramTextures[infoTex.x].SetPixel(infoTex.y, 4, atom1Pos);
            bondRep.paramTextures[infoTex.x].SetPixel(infoTex.y, 5, atom2Pos);
            texturesToUpdate[infoTex.x] = true;

        }

        ApplyTextures();

    }

    /// Set a large bounding box to avoid culling
    public void SetLargeBoundingVolume() {
        if (!largeBB) {
            if (bondRep.meshesGO != null && bondRep.meshesGO.Count != 0) {
                for (int i = 0; i < bondRep.meshesGO.Count; i++) {
                    Bounds b = bondRep.meshesGO[i].GetComponent<MeshFilter>().sharedMesh.bounds;
                    b.size = Vector3.one * 5000.0f;
                    bondRep.meshesGO[i].GetComponent<MeshFilter>().sharedMesh.bounds = b;
                }
            }
        }
        largeBB = true;
    }

    public void SetTexture(int id) {
        Texture tex = null;
        if (id >= 0 && id < UnityMolMain.atomColors.textures.Length) {
            tex = (Texture) UnityMolMain.atomColors.textures[id];
            SetTexture(tex);
            idTex = id;
        }
    }
    private void SetTexture(Texture tex) {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;
        for (int i = 0; i < bondRep.meshesGO.Count; i++) {
            bondRep.meshesGO[i].GetComponent<Renderer>().sharedMaterial.SetTexture("_MatCap", tex);
        }
    }
    public void ResetTexture() {
        Texture tex = (Texture) Resources.Load("Images/MatCap/daphz05");
        SetTexture(tex);
    }

    public void ShowAtoms(HashSet<UnityMolAtom> atoms, bool show) {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;
        foreach (AtomDuo duo in bondRep.coordStickTexture.Keys) {
            UnityMolAtom atom1 = duo.a1;
            UnityMolAtom atom2 = duo.a2;
            int3 infoTex = bondRep.coordStickTexture[duo];

            if (atoms.Contains(atom1) || atoms.Contains(atom2)) {
                if (show)
                    bondRep.paramTextures[infoTex.x].SetPixel(infoTex.y, 10, Vector4.one);
                else
                    bondRep.paramTextures[infoTex.x].SetPixel(infoTex.y, 10, Vector4.zero);

                texturesToUpdate[infoTex.x] = true;
            }
        }
        ApplyTextures();
    }

    public override void ShowHydrogens(bool show) {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;
        HashSet<UnityMolAtom> toHide = new HashSet<UnityMolAtom>();

        foreach (AtomDuo duo in bondRep.coordStickTexture.Keys) {
            UnityMolAtom atom1 = duo.a1;
            UnityMolAtom atom2 = duo.a2;

            if (atom1.type == "H") {
                if (show && !areSideChainsOn &&
                        MDAnalysisSelection.isSideChain(atom1, bondRep.selection.bonds)) {}
                else {
                    toHide.Add(atom1);
                }
            }
            if (atom2.type == "H") {
                if (show && !areSideChainsOn &&
                        MDAnalysisSelection.isSideChain(atom2, bondRep.selection.bonds)) {}
                else {
                    toHide.Add(atom2);
                }
            }

        }
        ShowAtoms(toHide, show);
        areHydrogensOn = show;

    }

    //TODO refactor this working mess
    public void ShowHide(AtomDuo d, bool show, bool now = true) {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;
        bool found = false;
        AtomDuo invd = new AtomDuo(d.a2, d.a1);
        if (bondRep.atomToDuo.ContainsKey(d.a1)) {
            foreach (AtomDuo d2 in bondRep.atomToDuo[d.a1]) {
                int3 infoTex;
                if (d.Equals(d2) || invd.Equals(d2)) {
                    if (invd.Equals(d2)) {
                        infoTex = bondRep.coordStickTexture[invd];
                    }
                    else {
                        infoTex = bondRep.coordStickTexture[d];
                    }

                    if (show) {
                        bondRep.paramTextures[infoTex.x].SetPixel(infoTex.y, 10, Vector4.one);
                    }
                    else {
                        bondRep.paramTextures[infoTex.x].SetPixel(infoTex.y, 10, Vector4.zero);
                    }

                    texturesToUpdate[infoTex.x] = true;
                    found = true;
                    break;
                }

            }
        }

        if (!found && bondRep.atomToDuo.ContainsKey(d.a2)) {
            foreach (AtomDuo d2 in bondRep.atomToDuo[d.a2]) {
                if (d2 == d || invd == d2) {
                    int3 infoTex;
                    if (invd.Equals(d2)) {
                        infoTex = bondRep.coordStickTexture[invd];
                    }
                    else {
                        infoTex = bondRep.coordStickTexture[d2];
                    }
                    if (show) {
                        bondRep.paramTextures[infoTex.x].SetPixel(infoTex.y, 10, Vector4.one);
                    }
                    else {
                        bondRep.paramTextures[infoTex.x].SetPixel(infoTex.y, 10, Vector4.zero);
                    }

                    texturesToUpdate[infoTex.x] = true;
                    found = true;
                    break;
                }
            }
        }
        if (!found) {
            // Debug.Log("Not found !");
        }
        if (found && now) {
            ApplyTextures();
        }
    }

    public override void ShowSideChains(bool show) {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;

        foreach (AtomDuo duo in bondRep.coordStickTexture.Keys) {
            UnityMolAtom atom1 = duo.a1;
            UnityMolAtom atom2 = duo.a2;
            int3 infoTex = bondRep.coordStickTexture[duo];
            if (MDAnalysisSelection.isSideChain(atom1, bondRep.selection.bonds) || MDAnalysisSelection.isSideChain(atom2, bondRep.selection.bonds)) {
                if (show && !areHydrogensOn && (atom1.type == "H" || atom2.type == "H")) {
                }
                else {
                    if (show) {
                        bondRep.paramTextures[infoTex.x].SetPixel(infoTex.y, 10, Vector4.one);

                    }
                    else {
                        bondRep.paramTextures[infoTex.x].SetPixel(infoTex.y, 10, Vector4.zero);
                    }

                    texturesToUpdate[infoTex.x] = true;
                }
            }
        }
        areSideChainsOn = show;
        ApplyTextures();
    }

    public override void ShowBackbone(bool show) {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;
        HashSet<UnityMolAtom> toHide = new HashSet<UnityMolAtom>();

        foreach (AtomDuo duo in bondRep.coordStickTexture.Keys) {
            UnityMolAtom atom1 = duo.a1;
            UnityMolAtom atom2 = duo.a2;

            if (MDAnalysisSelection.isBackBone(atom1, bondRep.selection.bonds)) {
                toHide.Add(atom1);
            }
            if (MDAnalysisSelection.isBackBone(atom2, bondRep.selection.bonds)) {
                toHide.Add(atom2);
            }
        }
        ShowAtoms(toHide, show);
        isBackboneOn = show;

    }

    public override void SetColor(Color32 col, UnityMolSelection sele) {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;
        foreach (UnityMolAtom a in sele.atoms) {
            SetColor(col, a, false);
        }
        ApplyTextures();
    }

    public override void SetColor(Color32 col, UnityMolAtom a) {
        SetColor(col, a, true);
    }

    public void SetColor(Color32 col, UnityMolAtom a, bool now = true) {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0 || !bondRep.atomToDuo.ContainsKey(a))
            return;


        // For all atoms linked
        foreach (AtomDuo ad in bondRep.atomToDuo[a]) {
            if (ad.a1 == a) { //Use a2
                SetColorForAtom(col, a, ad.a2);
            }
            else { //use a1
                SetColorForAtom(col, a, ad.a1);
            }
        }
        if (now) {
            ApplyTextures();
        }
    }
    public override void SetColors(Color32 col, List<UnityMolAtom> atoms) {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;
        foreach (UnityMolAtom a in atoms) {
            SetColor(col, a, false);
        }
        ApplyTextures();
    }

    public override void SetColors(List<Color32> cols, List<UnityMolAtom> atoms) {
        if (atoms.Count != cols.Count) {
            Debug.LogError("Lengths of color list and atom list are different");
            return;
        }
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;
        for (int i = 0; i < atoms.Count; i++) {
            UnityMolAtom a = atoms[i];
            Color32 col = cols[i];
            SetColor(col, a, false);
        }
        ApplyTextures();
    }

    public void SetColorForAtom(Color32 col, UnityMolAtom a1, UnityMolAtom a2) {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;
        key.a1 = a1;
        key.a2 = a2;
        bondRep.colorPerAtom[a1] = col;
        int3 infoTex;
        if (bondRep.coordStickTexture.TryGetValue(key, out infoTex)) {
            bondRep.paramTextures[infoTex.x].SetPixel(infoTex.y, 2, col);
            texturesToUpdate[infoTex.x] = true;

        }
        else {
            key.a1 = a2;
            key.a2 = a1;
            if (bondRep.coordStickTexture.TryGetValue(key, out infoTex) ) {
                bondRep.paramTextures[infoTex.x].SetPixel(infoTex.y, 3, col);
                texturesToUpdate[infoTex.x] = true;
            }
        }

    }

    public override void SetDepthCueingStart(float v) {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;

        foreach (GameObject meshGO in bondRep.meshesGO) {
            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_FogStart", v);
        }
    }

    public override void SetDepthCueingDensity(float v) {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;

        foreach (GameObject meshGO in bondRep.meshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_FogDensity", v);
        }
    }

    public override void EnableDepthCueing() {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;
        foreach (GameObject meshGO in bondRep.meshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_UseFog", 1.0f);
        }
    }

    public override void DisableDepthCueing() {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;
        foreach (GameObject meshGO in bondRep.meshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_UseFog", 0.0f);
        }

    }

    public override void ShowShadows(bool show) {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;
        for (int i = 0; i < bondRep.meshesGO.Count; i++) {
            if (show) {
                bondRep.meshesGO[i].GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.On;
                bondRep.meshesGO[i].GetComponent<Renderer>().receiveShadows = true;
            }
            else {
                bondRep.meshesGO[i].GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
                bondRep.meshesGO[i].GetComponent<Renderer>().receiveShadows = false;
            }
        }
    }

    public void SetShininess(float val) {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;
        //Clamp and invert shininess
        shininess = val;
        float valShine = (shininess < 0.0001f ? 0.0f : 1.0f / shininess);

        for (int i = 0; i < bondRep.meshesGO.Count; i++) {
            bondRep.meshesGO[i].GetComponent<Renderer>().sharedMaterial.SetFloat("_Shininess", valShine);
        }

    }

    public void ResetShininess() {
        SetShininess(0.0f);
    }

    public void SetShrink(float newShrink) {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;
        if (newShrink > 0.0f && newShrink <= 1.0f) {
            for (int i = 0; i < bondRep.meshesGO.Count; i++) {
                bondRep.meshesGO[i].GetComponent<Renderer>().sharedMaterial.SetFloat("_Shrink", newShrink);
            }

            shrink = newShrink;
            if (UnityMolMain.raytracingMode)
                recreateRTObject();
        }
    }

    public void ShowBondForAtom(bool show, UnityMolAtom atom) {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0 || !bondRep.atomToDuo.ContainsKey(atom))
            return;

        // For all atoms linked
        foreach (AtomDuo ad in bondRep.atomToDuo[atom]) {
            if (ad.a1 == atom) { //Use a2
                showBond(atom, ad.a2, show);
            }
            else { //use a1
                showBond(atom, ad.a1, show);
            }
        }
    }

    public void SetScaleForAtom(float size, UnityMolAtom atom, bool now = true) {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0 || !bondRep.atomToDuo.ContainsKey(atom))
            return;

        // For all atoms linked
        foreach (AtomDuo ad in bondRep.atomToDuo[atom]) {
            if (ad.a1 == atom) { //Use a2
                setScaleBond(atom, ad.a2, size);
            }
            else { //use a1
                setScaleBond(atom, ad.a1, size);
            }
        }

        if (now) {
            ApplyTextures();
        }
    }

    public void showBond(UnityMolAtom atom1, UnityMolAtom atom2, bool show) {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;

        key.a1 = atom1;
        key.a2 = atom2;
        int3 infoTex;
        if (bondRep.coordStickTexture.TryGetValue(key, out infoTex)) {
            if (show) {
                bondRep.paramTextures[infoTex.x].SetPixel(infoTex.y, 10, Vector4.one);
            }
            else {
                bondRep.paramTextures[infoTex.x].SetPixel(infoTex.y, 10, Vector4.zero);
            }
            texturesToUpdate[infoTex.x] = true;
        }
    }

    public void setScaleBond(UnityMolAtom a1, UnityMolAtom a2, float size) {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;

        key.a1 = a1;
        key.a2 = a2;
        int3 infoTex;
        if (bondRep.coordStickTexture.TryGetValue(key, out infoTex)) {
            Vector4 newSize = bondRep.paramTextures[infoTex.x].GetPixel(infoTex.y, 11);
            newSize.x = size;
            bondRep.paramTextures[infoTex.x].SetPixel(infoTex.y, 11, newSize);
            texturesToUpdate[infoTex.x] = true;
        }
        else {
            key.a1 = a2;
            key.a2 = a1;
            if (bondRep.coordStickTexture.TryGetValue(key, out infoTex) ) {
                Vector4 newSize = bondRep.paramTextures[infoTex.x].GetPixel(infoTex.y, 11);
                newSize.y = size;
                bondRep.paramTextures[infoTex.x].SetPixel(infoTex.y, 11, newSize);
                texturesToUpdate[infoTex.x] = true;
            }
        }
    }

    public void ResetVisibility() {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;
        foreach (AtomDuo duo in bondRep.coordStickTexture.Keys) {
            UnityMolAtom atom1 = duo.a1;
            UnityMolAtom atom2 = duo.a2;
            int3 infoTex = bondRep.coordStickTexture[duo];
            bondRep.paramTextures[infoTex.x].SetPixel(infoTex.y, 10, Vector4.one);
            texturesToUpdate[infoTex.x] = true;

        }
        ApplyTextures();
    }

    public override void updateWithTrajectory() {
        ResetPositions();
        SetLargeBoundingVolume();
    }

    public override void updateWithModel() {
        ResetPositions();
    }

    public override void ShowAtom(UnityMolAtom atom, bool show) {
        ShowBondForAtom(show, atom);
    }

    public override void SetSize(UnityMolAtom atom, float size) {
        SetScaleForAtom(size, atom);
    }

    public override void SetSizes(List<UnityMolAtom> atoms, List<float> sizes) {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;
        int i = 0;
        foreach (UnityMolAtom a in atoms) {
            SetScaleForAtom(sizes[i], a, false);
            i++;
        }
        ApplyTextures();
    }

    public override void SetSizes(List<UnityMolAtom> atoms, float size) {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;
        scaleBond = size;

        foreach (UnityMolAtom a in atoms) {
            SetScaleForAtom(size, a, false);
        }

        ApplyTextures();
    }

    public override void ResetSize(UnityMolAtom atom) {
        SetSize(atom, 1.0f);
    }

    public override void ResetSizes() {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;
        UnityMolModel m = bondRep.selection.atoms[0].residue.chain.model;

        foreach (int ida in bondRep.selection.bonds.bonds.Keys) {
            UnityMolAtom a = m.allAtoms[ida];
            SetScaleForAtom(1.0f, a, false);
        }
        ApplyTextures();
        scaleBond = 1.0f;
    }

    public override void ResetColor(UnityMolAtom atom) {
        SetColor(atom.color32, atom);
    }

    public override void ResetColors() {
        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return;
        UnityMolModel m = bondRep.selection.atoms[0].residue.chain.model;

        foreach (int ida in bondRep.selection.bonds.bonds.Keys) {
            UnityMolAtom a = m.allAtoms[ida];
            SetColor(a.color32, a, false);
        }
        ApplyTextures();
        bondRep.colorationType = colorType.atom;
    }

    public void highlightForAtom(UnityMolAtom a1, UnityMolAtom a2) {
    }
    public void removeHighlightForAtom(UnityMolAtom a1, UnityMolAtom a2) {
    }

    public void highlightAtom(UnityMolAtom a) {

    }
    public void removeHighlightAtom(UnityMolAtom a) {
    }

    public override void HighlightRepresentation() {
    }

    public override void DeHighlightRepresentation() {
    }
    public override void UpdateLike() {
        if (UnityMolMain.raytracingMode && rtos != null && rtos.Count == 0) {
            recreateRTObject();
        }
    }
    public override void SetSmoothness(float val) {
        Debug.LogWarning("Cannot change this value for the hyperstick representation");
    }
    public override void SetMetal(float val) {
        Debug.LogWarning("Cannot change this value for the hyperstick representation");
    }
    public override UnityMolRepresentationParameters Save() {

        UnityMolRepresentationParameters res = new UnityMolRepresentationParameters();

        res.repT.bondType = BondType.optihs;
        res.colorationType = bondRep.colorationType;
        res.HSIdTex = idTex;

        if (bondRep.meshesGO == null || bondRep.meshesGO.Count == 0)
            return res;

        if (res.colorationType == colorType.custom) {
            res.colorPerAtom = new Dictionary<UnityMolAtom, Color32>(bondRep.selection.Count);
            try {
                foreach (UnityMolAtom a in bondRep.selection.atoms) {
                    res.colorPerAtom[a] = bondRep.colorPerAtom[a];
                }
            }
            catch {
                foreach (UnityMolAtom a in bondRep.selection.atoms) {
                    res.colorPerAtom[a] = a.color32;
                }
            }

        }
        else if (res.colorationType == colorType.full) { //Get color of first atom/residue
            foreach (UnityMolAtom a in bondRep.colorPerAtom.Keys) {
                res.fullColor = bondRep.colorPerAtom[a];
                break;
            }
        }
        else if (res.colorationType == colorType.bfactor) {
            res.bfactorStartColor = bondRep.bfactorStartCol;
            res.bfactorMidColor = bondRep.bfactorMidColor;
            res.bfactorEndColor = bondRep.bfactorEndCol;
        }
        res.smoothness = shininess;
        res.HSShrink = shrink;

        if (bondRep.meshesGO != null && bondRep.meshesGO.Count > 0)
            res.shadow = (bondRep.meshesGO[0].GetComponent<Renderer>().shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.On);
        else
            res.shadow = true;
        res.HSScale = scaleBond;


        return res;
    }

    public override void Restore(UnityMolRepresentationParameters savedParams) {
        if (savedParams.repT.bondType == BondType.optihs) {
            if (savedParams.colorationType == colorType.full) {
                SetColor(savedParams.fullColor, bondRep.selection);
            }
            else if (savedParams.colorationType == colorType.custom) {
                foreach (UnityMolAtom a in bondRep.selection.atoms) {
                    if (savedParams.colorPerAtom.ContainsKey(a)) {
                        SetColor(savedParams.colorPerAtom[a], a);
                    }
                }
                ApplyTextures();
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
            else if (savedParams.colorationType == colorType.resid) {
                colorByResid(bondRep.selection);
            }
            else if (savedParams.colorationType == colorType.resnum) {
                colorByResnum(bondRep.selection);
            }
            else if (savedParams.colorationType == colorType.bfactor) {
                colorByBfactor(bondRep.selection, savedParams.bfactorStartColor, savedParams.bfactorMidColor, savedParams.bfactorEndColor);
            }
            bondRep.colorationType = savedParams.colorationType;

            if (idTex != savedParams.HSIdTex)
                SetTexture(savedParams.HSIdTex);

            SetSizes(bondRep.selection.atoms, savedParams.HSScale);
            SetShrink(savedParams.HSShrink);
            SetShininess(savedParams.smoothness);
            ShowShadows(savedParams.shadow);
        }
        else {
            Debug.LogError("Could not restore representation parameters");
        }
    }
}
}
