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

namespace UMol {

public class UnityMolHbondManager : UnityMolGenericRepresentationManager {
    private BondRepresentationHbonds bondRep;

    /// <summary>
    /// Initializes this instance of the manager.
    /// </summary>
    public override void Init(SubRepresentation umolRep) {

        if (isInit) {
            return;
        }

        bondRep = (BondRepresentationHbonds) umolRep.bondRep;
        isInit = true;
        isEnabled = true;
        areSideChainsOn = true;
        areHydrogensOn = true;
        isBackboneOn = true;

    }

    public override void InitRT() {}

    public override void Clean() {
        GameObject parent = null;
        if (bondRep.MeshesGO != null && bondRep.MeshesGO.Count != 0) {
            parent = bondRep.MeshesGO[0].transform.parent.gameObject;
            for (int i = 0; i < bondRep.MeshesGO.Count; i++) {
                GameObject.Destroy(bondRep.MeshesGO[i].GetComponent<MeshFilter>().sharedMesh);
                GameObject.Destroy(bondRep.MeshesGO[i]);
            }
        }

        if (parent != null) {
            GameObject.Destroy(parent);
        }

        bondRep = null;
        isInit = false;
        isEnabled = false;
        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();

    }
    /// <summary>
    /// Disables the renderers for all objects managed by the instance of the manager.
    /// </summary>

    public override void DisableRenderers() {
        foreach (GameObject meshGO in bondRep.MeshesGO) {
            meshGO.GetComponent<Renderer>().enabled = false;
        }
        isEnabled = false;
        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();
    }
    /// <summary>
    /// Enables the renderers for all objects managed by the instance of the manager.
    /// </summary>
    public override void EnableRenderers() {
        foreach (GameObject meshGO in bondRep.MeshesGO) {
            meshGO.GetComponent<Renderer>().enabled = true;
        }
        isEnabled = true;
        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();

    }

    public void RemoveForAtom(UnityMolAtom atom) {
        if (bondRep.AtomToGO.ContainsKey(atom)) {
            List<GameObject> goToDelete = bondRep.AtomToGO[atom];

            foreach (GameObject go in goToDelete) {
                try {
                    GameObject.Destroy(go);
                    bondRep.MeshesGO.Remove(go);
                }
                catch {
                    //Already destroyed
                }
            }
            bondRep.AtomToVertices.Remove(atom);
            bondRep.AtomToMeshes.Remove(atom);

            bondRep.AtomToGO[atom].Clear();
            bondRep.AtomToGO.Remove(atom);
        }
    }

    public override void ShowHydrogens(bool show) {
        areHydrogensOn = show;
    }//Does not really make sense for hbonds
    public override void ShowSideChains(bool show) {
        areSideChainsOn = show;
    }//Does not really make sense for hbonds

    public override void ShowBackbone(bool show) {
        isBackboneOn = show;
    }//Does not really make sense for hbonds


    public override void SetColor(Color32 col, UnityMolSelection sele) {
        foreach (UnityMolAtom a in sele.atoms) {
            SetColor(col, a);
        }
    }

    public override void SetColor(Color32 col, UnityMolAtom a) {
        try {
            List<Mesh> meshes = bondRep.AtomToMeshes[a];
            List<int> ids = bondRep.AtomToVertices[a];
            int curid = 0;
            foreach (Mesh mesh in meshes) {
                Color32[] cols = mesh.colors32;
                cols[ids[curid]] = col;
                cols[ids[curid + 1]] = col;
                curid += 2;
                mesh.colors32 = cols;
            }

        }
        catch {
            // Debug.LogError("Could not find atom " + a + " in this representation");
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
        if (bondRep.MeshesGO == null)
            return;
        foreach (GameObject meshGO in bondRep.MeshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_FogStart", v);
        }
    }

    public override void SetDepthCueingDensity(float v) {
        if (bondRep.MeshesGO == null)
            return;
        foreach (GameObject meshGO in bondRep.MeshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_FogDensity", v);
        }
    }

    public override void EnableDepthCueing() {
        if (bondRep.MeshesGO == null)
            return;
        foreach (GameObject meshGO in bondRep.MeshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_UseFog", 1.0f);
        }
    }

    public override void DisableDepthCueing() {
        if (bondRep.MeshesGO == null)
            return;
        foreach (GameObject meshGO in bondRep.MeshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_UseFog", 0.0f);
        }
    }

    /// <summary>
    /// No shadows for hbonds
    /// </summary>
    public override void ShowShadows(bool show) {}

    /// <summary>
    /// Recomputes the hbonds
    /// </summary>
    public override void updateWithTrajectory() {
        bool wasEnabled = true;
        if (bondRep.MeshesGO != null && bondRep.MeshesGO.Count >= 1)
            wasEnabled = bondRep.MeshesGO[0].GetComponent<Renderer>().enabled;

        if (wasEnabled) {
            bondRep.Recompute(false);
        }
    }

    public override void updateWithModel() {
        updateWithTrajectory();
    }

    public override void ShowAtom(UnityMolAtom atom, bool show) {
        //TODO: Implement this function
    }

    /// <summary>
    /// Size does not matter for line representation
    /// </summary>
    public override void SetSize(UnityMolAtom atom, float size) {
        Debug.LogWarning("Cannot change the size atoms with the hbond representation");
    }

    public override void SetSizes(List<UnityMolAtom> atoms, List<float> sizes) {
        Debug.LogWarning("Cannot change the size atoms with the line representation");
    }

    public override void SetSizes(List<UnityMolAtom> atoms, float size) {
        Debug.LogWarning("Cannot change the size atoms with the line representation");
    }

    public override void ResetSize(UnityMolAtom atom) {
        Debug.LogWarning("Cannot change the size atoms with the hbond representation");
    }
    public override void ResetSizes() {
        Debug.LogWarning("Cannot change the size atoms with the hbond representation");
    }

    public override void ResetColor(UnityMolAtom atom) {
        SetColor(Color.white, atom);
    }
    public override void ResetColors() {
        UnityMolModel m = bondRep.selection.atoms[0].residue.chain.model;

        foreach(int ida in bondRep.selection.bonds.bonds.Keys) {
            UnityMolAtom a = m.allAtoms[ida];
            SetColor(Color.white, a);
        }
        bondRep.colorationType = colorType.full;
    }

    public override void HighlightRepresentation() {
        Debug.LogWarning("Hbonds cannot be highlighted");
    }


    public override void DeHighlightRepresentation() {
        Debug.LogWarning("Hbonds cannot be highlighted");
    }
    public override void SetSmoothness(float val) {
        Debug.LogWarning("Cannot change this value for the h-bond representation");
    }
    public override void SetMetal(float val) {
        Debug.LogWarning("Cannot change this value for the h-bond representation");
    }
    private Color32 getAtomColor(UnityMolAtom a) {
        try {
            Mesh mesh = bondRep.AtomToMeshes[a][0];
            List<int> ids = bondRep.AtomToVertices[a];
            Color32[] cols = mesh.colors32;
            return cols[ids[0]];
        }
        catch (System.Exception e){
            Debug.LogError("Couldn't get atom color "+e);
        }
        return Color.white;
    }

    public override void UpdateLike(){
    }

    public override UnityMolRepresentationParameters Save() {
        UnityMolRepresentationParameters res = new UnityMolRepresentationParameters();

        res.repT.bondType = BondType.hbond;
        res.colorationType = bondRep.colorationType;

        if (res.colorationType == colorType.custom) {
            res.colorPerAtom = new Dictionary<UnityMolAtom, Color32>(bondRep.selection.atoms.Count);
            foreach (UnityMolAtom a in bondRep.AtomToMeshes.Keys) {
                res.colorPerAtom[a] = getAtomColor(a);
            }

        }
        else if (res.colorationType == colorType.full) { //Get color of first atom/residue
            foreach (UnityMolAtom a in bondRep.AtomToMeshes.Keys) {
                res.fullColor = getAtomColor(a);
                break;
            }
        }
        else if (res.colorationType == colorType.bfactor) {
            res.bfactorStartColor = bondRep.bfactorStartCol;
            res.bfactorMidColor = bondRep.bfactorMidColor;
            res.bfactorEndColor = bondRep.bfactorEndCol;
        }
        return res;
    }
    public override void Restore(UnityMolRepresentationParameters savedParams) {
        if (savedParams.repT.bondType == BondType.hbond) {
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
        }
        else {
            Debug.LogError("Could not restore representation parameters");
        }
    }
}
}
