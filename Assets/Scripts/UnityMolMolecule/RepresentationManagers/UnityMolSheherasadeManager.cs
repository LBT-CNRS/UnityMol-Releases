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
public class UnityMolSheherasadeManager : UnityMolGenericRepresentationManager {

    private List<GameObject> meshesGO;

    public SheherasadeRepresentation atomRep;

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
    public override void Init(SubRepresentation umolRep)
    {
        if (isInit)
            return;

        atomRep = (SheherasadeRepresentation)umolRep.atomRep;
        meshesGO = atomRep.meshesGO;
        highlightMat = (Material)Resources.Load("Materials/HighlightMaterial");


        isInit = true;
        isEnabled = true;
        areSideChainsOn = true;
        areHydrogensOn = true;
        isBackboneOn = true;
    }

    public override void InitRT() {}

    public override void Clean()
    {
        GameObject parent = null;
        if (meshesGO != null && meshesGO.Count != 0) {
            parent = meshesGO[0].transform.parent.gameObject;
        }

        if (meshesGO != null) {
            for (int i = 0; i < meshesGO.Count; i++) {
                GameObject.Destroy(meshesGO[i].GetComponent<MeshFilter>().sharedMesh);
                GameObject.Destroy(meshesGO[i].GetComponent<MeshRenderer>().sharedMaterial);
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

    public override void updateWithTrajectory()
    {

        needUpdate = true;
        bool wasEnabled = true;
        if (meshesGO != null && meshesGO.Count >= 1)
            wasEnabled = meshesGO[0].GetComponent<Renderer>().enabled;

        if (wasEnabled) {
            atomRep.recompute();
            needUpdate = false;
        }
    }

    public override void updateWithModel()
    {
        needUpdate = true;
        bool wasEnabled = true;
        if (meshesGO != null && meshesGO.Count >= 1)
            wasEnabled = meshesGO[0].GetComponent<Renderer>().enabled;

        if (wasEnabled) {
            atomRep.recompute(isNewModel: true);
            needUpdate = false;
        }
    }

    public override void HighlightRepresentation()
    {
    }

    public override void DeHighlightRepresentation()
    {
    }



    public void SetTexture(Texture tex) {
        for (int i = 0; i < meshesGO.Count; i++) {
            meshesGO[i].GetComponent<Renderer>().sharedMaterials[0].SetTexture("_MainTex", tex);
        }
    }

    public override void ResetColors() {

        for (int i = 0; i < meshesGO.Count; i++) {
            Color32[] cols = meshesGO[i].GetComponent<MeshFilter>().sharedMesh.colors32;
            for (int c = 0; c < cols.Length; c++) {
                cols[c] = Sheherasade.sheetColor;
            }
            meshesGO[i].GetComponent<MeshFilter>().sharedMesh.colors32 = cols;
        }
    }

    public override void SetSmoothness(float val)
    {
        for (int i = 0; i < meshesGO.Count; i++) {
            Material[] mats = meshesGO[i].GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_Glossiness", val);
        }
    }

    public override void SetMetal(float val)
    {
        for (int i = 0; i < meshesGO.Count; i++) {
            Material[] mats = meshesGO[i].GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_Metallic", val);
        }
    }

    public void SetSheherasadeForm(bool bezier = true)
    {
        atomRep.bezier = bezier;
        atomRep.recompute();
    }

    public override void SetColor(Color32 col, UnityMolSelection sele) {
        List<UnityMolResidue> residues = new List<UnityMolResidue>();

        foreach (UnityMolAtom a in sele.atoms) {
            residues.Add(a.residue);
        }
        var residuesIE = residues.Distinct();
        foreach (UnityMolResidue r in residuesIE) {
            SetColor(col, r, false);
        }
        ApplyColors();
    }

    public void SetColor(Color32 col, UnityMolResidue r, bool applyNow) {
        List<int> listVertId;
        GameObject curGo = null;

        if (atomRep.residueToGo.TryGetValue(r, out curGo) && atomRep.residueToVert.TryGetValue(r, out listVertId)) {

            List<Color32> colors = atomRep.meshColors[curGo];

            foreach (int c in listVertId) {
                if (c >= 0 && c < colors.Count) {
                    colors[c] = col;
                }
            }

            if (applyNow) {
                ApplyColors();
            }
        }
    }

    public void ApplyColors() {
        foreach (GameObject go in meshesGO) {
            go.GetComponent<MeshFilter>().sharedMesh.SetColors(atomRep.meshColors[go]);
        }
    }

    public override void SetColor(Color32 col, UnityMolAtom atom)
    {   //Does not really make sense for atoms
        Debug.LogWarning("Cannot set the color of one atom with the Sheherasade representation");
    }

    public override void SetColors(Color32 col, List<UnityMolAtom> atoms)
    {
        SetColor(col, new UnityMolSelection(atoms, newBonds: null, "tmp"));
    }

    public override void SetColors(List<Color32> cols, List<UnityMolAtom> atoms)
    {
        Debug.LogWarning("Cannot set the color of one atom with the Sheherasade representation");
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

    public override void ShowAtom(UnityMolAtom atom, bool show)
    {
        Debug.LogWarning("Cannot show/hide one atom with the Sheherasade representation");
    }

    public override void SetSize(UnityMolAtom atom, float size)
    {
        Debug.LogWarning("Cannot set the size of one atom with the Sheherasade representation");
    }

    public override void SetSizes(List<UnityMolAtom> atoms, List<float> sizes)
    {
        Debug.LogWarning("Cannot set the size of atoms with the Sheherasade representation");
    }

    public override void SetSizes(List<UnityMolAtom> atoms, float size)
    {
        Debug.LogWarning("Cannot set the size of atoms with the Sheherasade representation");
    }

    public override void ResetSize(UnityMolAtom atom)
    {
        Debug.LogWarning("Cannot set the size of one atom with the Sheherasade representation");
    }

    public override void ResetSizes()
    {
        Debug.LogWarning("Cannot set the size of atoms with the Sheherasade representation");
    }

    public override void ShowShadows(bool show)
    {
        if (meshesGO == null)
            return;
        for (int i = 0; i < meshesGO.Count; i++) {
            if (show)
                meshesGO[i].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            else
                meshesGO[i].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    public override void ShowHydrogens(bool show)
    {
        areHydrogensOn = show;
    }//Does not really make sense for sheherasade

    public override void ShowSideChains(bool show)
    {
        areSideChainsOn = show;
    }//Does not really make sense for sheherasade

    public override void ShowBackbone(bool show)
    {
        isBackboneOn = show;
    }//Does not really make sense for sheherasade

    public override void ResetColor(UnityMolAtom atom)
    {
        Debug.LogWarning("Cannot set the color of one atom with the Sheherasade representation");
    }
    public override void UpdateLike() {
    }

    public override UnityMolRepresentationParameters Save() {
        UnityMolRepresentationParameters res = new UnityMolRepresentationParameters();

        res.repT.atomType = AtomType.sheherasade;
        res.colorationType = atomRep.colorationType;

        if (res.colorationType == colorType.custom) {
            res.colorPerAtom = new Dictionary<UnityMolAtom, Color32>(atomRep.selection.atoms.Count);

        }
        else if (res.colorationType == colorType.full) { //Get color of first atom/residue
            Color32[] colors = meshesGO[0].GetComponent<MeshFilter>().sharedMesh.colors32;
            res.fullColor = colors[0];
        }
        else if (res.colorationType == colorType.bfactor) {
            res.bfactorStartColor = atomRep.bfactorStartCol;
            res.bfactorMidColor = atomRep.bfactorMidColor;
            res.bfactorEndColor = atomRep.bfactorEndCol;
        }
        // res.shadow = (meshesGO[0].GetComponent<Renderer>().shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.On);

        return res;
    }
    public override void Restore(UnityMolRepresentationParameters savedParams) {

        if (savedParams.repT.atomType == AtomType.sheherasade) {
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

            atomRep.colorationType = savedParams.colorationType;
        }
        else {
            Debug.LogError("Could not restore representation parameters");
        }
    }
}
}
