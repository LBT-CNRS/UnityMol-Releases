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
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Linq;
using UnityEngine;

namespace UMol {

public class UnityMolCartoonManager : UnityMolGenericRepresentationManager {

    public CartoonRepresentation atomRep;

    private bool needUpdate = false;
    public bool isTransparent = false;
    public float curAlpha = 1.0f;

    ///Is limited view activated
    public bool limitedView = false;
    ///Local space coordinates of the center of limited view
    public Vector3 limitedViewCenter = Vector3.zero;
    ///Radius in Angstrom of the limited view
    public float limitedViewRadius = 10.0f;

    private bool savedShadowBeforeLimited = true;


    /// <summary>
    /// Disables the renderers for all objects managed by the instance of the manager.
    /// </summary>
    public override void DisableRenderers() {
        isEnabled = false;
        if (atomRep.meshesGO == null)
            return;
        for (int i = 0; i < atomRep.meshesGO.Count; i++) {
            atomRep.meshesGO[i].GetComponent<Renderer>().enabled = false;
        }
        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();
    }

    /// <summary>
    /// Enables the renderers for all objects managed by the instance of the manager.
    /// </summary>
    public override void EnableRenderers() {
        isEnabled = true;
        if (atomRep.meshesGO == null)
            return;
        if (needUpdate) {
            DoRecompute();
        }
        for (int i = 0; i < atomRep.meshesGO.Count; i++) {
            atomRep.meshesGO[i].GetComponent<Renderer>().enabled = true;
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

        atomRep = (CartoonRepresentation) umolRep.atomRep;

        if (UnityMolMain.raytracingMode)
            InitRT();

        isInit = true;
        isEnabled = true;
        areSideChainsOn = true;
        areHydrogensOn = true;
        isBackboneOn = true;
        isTransparent = false;
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
        GameObject parent = null;
        if (atomRep.meshesGO != null) {
            if (atomRep.meshesGO.Count != 0 && atomRep.meshesGO[0] != null && atomRep.meshesGO[0].transform.parent != null) {
                parent = atomRep.meshesGO[0].transform.parent.gameObject;
            }
        }
        if (rtos != null) {
            rtos.Clear();
        }

        for (int i = 0; i < atomRep.meshesGO.Count; i++) {
            if (atomRep.meshesGO[i] != null) {
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
        isEnabled = false;
        isInit = false;
        isTransparent = false;
    }

    public override void ShowShadows(bool show) {
        if (atomRep.meshesGO == null)
            return;
        for (int i = 0; i < atomRep.meshesGO.Count; i++) {
            if (show)
                atomRep.meshesGO[i].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            else
                atomRep.meshesGO[i].GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    public override void ShowHydrogens(bool show) {
        areHydrogensOn = show;
    } //Does not really make sense for cartoon

    public override void ShowSideChains(bool show) {
        areSideChainsOn = show;
    } //Does not really make sense for cartoon

    public override void ShowBackbone(bool show) {
        isBackboneOn = show;
    } //Does not really make sense for cartoon

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

    public override void SetColor(Color32 col, UnityMolAtom atom) { //Does not really make sense for atoms
        Debug.LogWarning("Cannot set the color of one atom with the cartoon representation");
    }

    public override void SetColors(Color32 col, List<UnityMolAtom> atoms) {
        SetColor(col, new UnityMolSelection(atoms, newBonds : null, "tmp"));
    }

    public override void SetColors(List<Color32> cols, List<UnityMolAtom> atoms) {
        if (cols.Count == atoms.Count) {
            HashSet<UnityMolResidue> doneRes = new HashSet<UnityMolResidue>();
            for (int i = 0; i < cols.Count; i++) {
                UnityMolResidue r = atoms[i].residue;
                //Try to find a CA atom
                if (r.atoms.ContainsKey("CA")) {
                    if (atoms[i].name == "CA") {
                        SetColor(cols[i], r, false);
                        doneRes.Add(r);
                    }
                }
                //Try to find a O atom
                else if (r.atoms.ContainsKey("O")) {
                    if (atoms[i].name == "O") {
                        SetColor(cols[i], r, false);
                        doneRes.Add(r);
                    }
                } else { //Take the first atom of the color list
                    if (!doneRes.Contains(r)) {
                        SetColor(cols[i], r, false);
                        doneRes.Add(r);
                    }
                }
            }
            ApplyColors();
        } else {
            Debug.LogError("Number of colors should be equal to the number of atoms");
            return;
        }
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

    void DoRecompute(bool isNewM = false) {
        RaytracingMaterial savedrtMat = null;
        if (rtos != null && rtos.Count > 0) {
            savedrtMat = rtos[0].rtMat;
            for (int i = 0; i < rtos.Count; i++) {
                GameObject.Destroy(rtos[i]);
            }
            rtos.Clear();
        }
        atomRep.recompute(isNewModel: isNewM);
        needUpdate = false;
        if (savedrtMat != null) {
            foreach (GameObject meshGO in atomRep.meshesGO) {
                var rto = meshGO.AddComponent<RaytracedObject>();
                rto.rtMat = savedrtMat;
                rtos.Add(rto);
            }
        }
    }

    public override void updateWithTrajectory() {

        needUpdate = true;
        bool wasEnabled = true;
        if (atomRep.meshesGO != null && atomRep.meshesGO.Count >= 1)
            wasEnabled = atomRep.meshesGO[0].GetComponent<Renderer>().enabled;

        if (wasEnabled) {
            DoRecompute();
        }
    }

    public override void updateWithModel() {
        needUpdate = true;
        bool wasEnabled = true;
        if (atomRep.meshesGO != null && atomRep.meshesGO.Count >= 1)
            wasEnabled = atomRep.meshesGO[0].GetComponent<Renderer>().enabled;

        if (wasEnabled) {
            DoRecompute(true);
        }
    }

    public void SetTubeSize(float v) {
        needUpdate = true;
        bool wasEnabled = true;
        if (atomRep.meshesGO != null && atomRep.meshesGO.Count >= 1)
            wasEnabled = atomRep.meshesGO[0].GetComponent<Renderer>().enabled;

        if (atomRep.customTubeSize != v) {
            // UnityMolMain.getPrecompRepManager().Clear(atomRep.selection.structures[0].name, "Cartoon");
            atomRep.customTubeSize = v;
        }

        if (wasEnabled) {
            DoRecompute();
        }
    }

    public void DrawAsTube(bool t = true) {
        needUpdate = true;
        bool wasEnabled = true;
        if (atomRep.meshesGO != null && atomRep.meshesGO.Count >= 1)
            wasEnabled = atomRep.meshesGO[0].GetComponent<Renderer>().enabled;

        if (atomRep.tube != t) {
            // UnityMolMain.getPrecompRepManager().Clear(atomRep.selection.structures[0].name, "Cartoon");
            atomRep.tube = t;
        }

        if (wasEnabled) {
            DoRecompute();
        }
    }

    public void DrawAsBfactorTube(bool dobf = true) {

        needUpdate = true;
        bool wasEnabled = true;
        if (atomRep.meshesGO != null && atomRep.meshesGO.Count >= 1)
            wasEnabled = atomRep.meshesGO[0].GetComponent<Renderer>().enabled;

        if (atomRep.bfactortube != dobf) {
            // UnityMolMain.getPrecompRepManager().Clear(atomRep.selection.structures[0].name, "Cartoon");
            atomRep.tube = dobf;
            atomRep.bfactortube = dobf;
        }

        if (wasEnabled) {
            DoRecompute();
        }
    }

    public override void ShowAtom(UnityMolAtom atom, bool show) {
        Debug.LogWarning("Cannot show/hide one atom with the cartoon representation");
    }

    public override void SetSize(UnityMolAtom atom, float size) {
        Debug.LogWarning("Cannot set the size of one atom with the cartoon representation");
    }

    public override void SetSizes(List<UnityMolAtom> atoms, List<float> sizes) {
        Debug.LogWarning("Cannot set the size of atoms with the cartoon representation");
    }

    public override void SetSizes(List<UnityMolAtom> atoms, float size) {
        Debug.LogWarning("Cannot set the size of atoms with the cartoon representation");
    }
    public override void ResetSize(UnityMolAtom atom) {
        Debug.LogWarning("Cannot set the size of one atom with the cartoon representation");
    }
    public override void ResetSizes() {
        Debug.LogWarning("Cannot set the size of atoms with the cartoon representation");
    }

    public override void ResetColor(UnityMolAtom atom) {
        SetColor(atom.color32, atom);
    }
    public override void ResetColors() {

        if (atomRep.savedColors.Count == atomRep.meshesGO.Count) {
            int i = 0;
            foreach (GameObject go in atomRep.meshesGO) {
                atomRep.meshColors[go] = atomRep.savedColors[i].ToList();
                i++;
            }
            ApplyColors();
        }
        atomRep.colorationType = colorType.defaultCartoon;
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
        foreach (GameObject meshGO in atomRep.meshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_Glossiness", val);
        }
    }
    public override void SetMetal(float val) {
        foreach (GameObject meshGO in atomRep.meshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_Metallic", val);
        }
    }

    public void SwitchTransparent(bool force = false) {

        if (isTransparent && !force) {
            foreach (GameObject go in atomRep.meshesGO) {
                go.GetComponent<Renderer>().sharedMaterial = atomRep.ribbonMat;
            }
            isTransparent = false;
        }
        else {
            if (atomRep.transMat == null) {
                atomRep.transMat = new Material(Shader.Find("Custom/SurfaceVertexColorTransparent"));
                // atomRep.transMat.SetTexture("_DitherPattern", Resources.Load("Images/BayerDither8x8") as Texture2D);
            }
            foreach (GameObject go in atomRep.meshesGO) {
                go.GetComponent<Renderer>().sharedMaterial = atomRep.transMat;
            }
            isTransparent = true;
        }

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
        } else {
            DisableDepthCueing();
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

    public override void UpdateLike() { }

    public override UnityMolRepresentationParameters Save() {
        UnityMolRepresentationParameters res = new UnityMolRepresentationParameters();
        res.repT.atomType = AtomType.cartoon;
        res.repT.bondType = BondType.nobond;
        res.colorationType = atomRep.colorationType;
        res.CartoonTubeSize = atomRep.customTubeSize;
        res.CartoonTransparent = isTransparent;
        res.CartoonAsTube = atomRep.tube;
        res.CartoonAsBFactorTube = atomRep.bfactortube;
        res.CartoonAlpha = curAlpha;
        res.CartoonLimitedView = limitedView;
        res.CartoonLimitedViewRadius = limitedViewRadius;
        res.CartoonLimitedViewCenter = limitedViewCenter;

        if (res.colorationType == colorType.custom) {
            //Need a light atom comparer to avoid indexing with unique atom serial here
            res.colorPerAtom = new Dictionary<UnityMolAtom, Color32>(atomRep.savedAtoms.Count, new LightAtomComparer());
            foreach (UnityMolAtom a in atomRep.savedAtoms) {
                List<int> listVertId;
                GameObject curGo = null;
                if (atomRep.residueToGo.TryGetValue(a.residue, out curGo) && atomRep.residueToVert.TryGetValue(a.residue, out listVertId)) {
                    List<Color32> colors = atomRep.meshColors[curGo];
                    res.colorPerAtom[a] = colors[listVertId[0]];
                }
            }
        } else if (res.colorationType == colorType.full) { //Get color of first atom/residue
            foreach (UnityMolAtom a in atomRep.savedAtoms) {
                List<int> listVertId;
                GameObject curGo = null;
                if (atomRep.residueToGo.TryGetValue(a.residue, out curGo) && atomRep.residueToVert.TryGetValue(a.residue, out listVertId)) {
                    List<Color32> colors = atomRep.meshColors[curGo];
                    res.fullColor = colors[0];
                    break;
                }
            }
        } else if (res.colorationType == colorType.bfactor) {
            res.bfactorStartColor = atomRep.bfactorStartCol;
            res.bfactorMidColor = atomRep.bfactorMidColor;
            res.bfactorEndColor = atomRep.bfactorEndCol;
        }
        if (atomRep.meshesGO != null && atomRep.meshesGO.Count >= 1) {
            res.smoothness = atomRep.meshesGO[0].GetComponent<Renderer>().sharedMaterials[0].GetFloat("_Glossiness");
            res.metal = atomRep.meshesGO[0].GetComponent<Renderer>().sharedMaterials[0].GetFloat("_Metallic");
            res.shadow = (atomRep.meshesGO[0].GetComponent<Renderer>().shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.On);
        }

        return res;
    }

    public override void Restore(UnityMolRepresentationParameters savedParams) {
        isTransparent = savedParams.CartoonTransparent;
        atomRep.tube = savedParams.CartoonAsTube;
        atomRep.bfactortube = savedParams.CartoonAsBFactorTube;
        curAlpha = savedParams.CartoonAlpha;

        if (savedParams.CartoonTubeSize != atomRep.customTubeSize) {
            SetTubeSize(savedParams.CartoonTubeSize);
        }

        if (atomRep.tube)
            DrawAsTube();
        if (atomRep.bfactortube)
            DrawAsBfactorTube();


        if (savedParams.repT.atomType == AtomType.cartoon && savedParams.repT.bondType == BondType.nobond) {
            if (savedParams.colorationType == colorType.full) {
                SetColor(savedParams.fullColor, atomRep.selection);
                atomRep.colorationType = colorType.full;
            } else if (savedParams.colorationType == colorType.custom) {
                foreach (UnityMolAtom a in atomRep.selection.atoms) {
                    if (savedParams.colorPerAtom.ContainsKey(a)) {
                        SetColor(savedParams.colorPerAtom[a], a.residue, false);
                    }
                }
                ApplyColors();
            } else if (savedParams.colorationType == colorType.defaultCartoon) {
                //Do nothing !
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
            } else if (savedParams.colorationType == colorType.resnum) {
                colorByResnum(atomRep.selection);
            } else if (savedParams.colorationType == colorType.resid) {
                colorByResid(atomRep.selection);
            } else if (savedParams.colorationType == colorType.rescharge) {
                colorByResCharge(atomRep.selection);
            } else if (savedParams.colorationType == colorType.bfactor) {
                colorByBfactor(atomRep.selection, savedParams.bfactorStartColor, savedParams.bfactorMidColor, savedParams.bfactorEndColor);
            }
            atomRep.colorationType = savedParams.colorationType;

            SetMetal(savedParams.metal);
            SetSmoothness(savedParams.smoothness);
            ShowShadows(savedParams.shadow);
            if (isTransparent) {
                SwitchTransparent(true);
                SetAlpha(curAlpha);
            }

            limitedViewRadius = savedParams.CartoonLimitedViewRadius;
            limitedViewCenter = savedParams.CartoonLimitedViewCenter;
            if (savedParams.CartoonLimitedView)
                activateLimitedView();

        } else {
            Debug.LogError("Could not restore representation parameters");
        }
    }
}
}
