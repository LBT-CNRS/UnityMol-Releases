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

public class UnityMolHbondTubeManager : UnityMolGenericRepresentationManager {
    private BondRepresentationHbondsTube bondRep;
    public List<GameObject> meshesGO;

    public Dictionary<UnityMolAtom, List<GameObject>> atomToGo;


    /// <summary>
    /// Initializes this instance of the manager.
    /// </summary>
    public override void Init(SubRepresentation umolRep) {

        if (isInit) {
            return;
        }

        bondRep = (BondRepresentationHbondsTube) umolRep.bondRep;
        meshesGO = bondRep.meshesGO;
        atomToGo = bondRep.atomToGo;
        isInit = true;
        isEnabled = true;
        areSideChainsOn = true;
        areHydrogensOn = true;
        isBackboneOn = true;

    }

    public override void Clean() {
        GameObject parent = null;
        if (meshesGO != null && meshesGO.Count != 0) {
            parent = meshesGO[0].transform.parent.gameObject;
            for (int i = 0; i < meshesGO.Count; i++) {
                GameObject.Destroy(meshesGO[i]);
            }
            
            atomToGo.Clear();
        }

        if (parent != null) {
            GameObject.Destroy(parent);
        }

        bondRep.Clear();


        meshesGO = null;
        bondRep = null;
        atomToGo = null;

        isInit = false;
        isEnabled = false;
        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();

    }
    /// <summary>
    /// Disables the renderers for all objects managed by the instance of the manager.
    /// </summary>

    public override void DisableRenderers() {
        foreach (GameObject meshGO in meshesGO) {
            meshGO.GetComponent<Renderer>().enabled = false;
        }
        isEnabled = false;
        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();
    }
    /// <summary>
    /// Enables the renderers for all objects managed by the instance of the manager.
    /// </summary>
    public override void EnableRenderers() {
        foreach (GameObject meshGO in meshesGO) {
            meshGO.GetComponent<Renderer>().enabled = true;
        }
        isEnabled = true;
        // UnityMolMain.getRepresentationManager().UpdateActiveColliders();
        
    }

    public void RemoveForAtom(UnityMolAtom atom){
        if(atomToGo.ContainsKey(atom)){
            List<GameObject> goToDelete = atomToGo[atom];

            foreach(GameObject go in goToDelete){
                try{
                    GameObject.Destroy(go);
                    meshesGO.Remove(go);
                }
                catch{
                    //Already destroyed
                }
            }
            bondRep.atomToVertices.Remove(atom);
            bondRep.atomToMeshes.Remove(atom);

            atomToGo[atom].Clear();
            atomToGo.Remove(atom);
        }
    }

    public override void ShowHydrogens(bool show) {
        areHydrogensOn = show;
    }//Does not really make sense for hbonds
    public override void ShowSideChains(bool show) {
        areSideChainsOn = show;
    }//Does not really make sense for hbonds

    public override void ShowBackbone(bool show){
        isBackboneOn = show;
    }//Does not really make sense for hbonds


    public override void SetColor(Color col, UnityMolSelection sele) {
        foreach (UnityMolAtom a in sele.atoms) {
            SetColor(col, a);
        }
    }

    public override void SetColor(Color col, UnityMolAtom a) {
        try {
            List<Mesh> meshes = bondRep.atomToMeshes[a];
            // List<int> ids = bondRep.atomToVertices[a];
            // int curid = 0;
            foreach (Mesh mesh in meshes) {
                Color32[] cols = mesh.colors32;
                for(int i = 0; i < cols.Length; i++){
                    cols[i] = col;
                }
                // cols[ids[curid]] = col;
                // cols[ids[curid + 1]] = col;
                // curid += 2;
                mesh.colors32 = cols;
            }

        }
        catch {
            // Debug.LogError("Could not find atom " + a + " in this representation");
        }
    }


    public override void SetColors(Color col, List<UnityMolAtom> atoms) {
        foreach (UnityMolAtom a in atoms) {
            SetColor(col, a);
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
        if(meshesGO == null)
            return;
        foreach (GameObject meshGO in meshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_FogStart", v);
        }  
    }

    public override void SetDepthCueingDensity(float v) {
        if(meshesGO == null)
            return;
        foreach (GameObject meshGO in meshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_FogDensity", v);
        }
    }

    public override void EnableDepthCueing() {
        if(meshesGO == null)
            return;
        foreach (GameObject meshGO in meshesGO) {

            Material[] mats = meshGO.GetComponent<Renderer>().sharedMaterials;
            mats[0].SetFloat("_UseFog", 1.0f);
        }
    }

    public override void DisableDepthCueing() {
        if(meshesGO == null)
            return;
        foreach (GameObject meshGO in meshesGO) {

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
        if (meshesGO != null && meshesGO.Count >= 1)
            wasEnabled = meshesGO[0].GetComponent<Renderer>().enabled;

        if (wasEnabled) {
            bondRep.recompute();
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
        foreach (UnityMolAtom a in bondRep.selection.bonds.bondsDual.Keys) {
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
    public override void SetSmoothness(float val){
        Debug.LogWarning("Cannot change this value for the h-bond representation");
    }
    public override void SetMetal(float val){
        Debug.LogWarning("Cannot change this value for the h-bond representation");
    }

    private Color32 getAtomColor(UnityMolAtom a) {
        try {
            Mesh mesh = bondRep.atomToMeshes[a][0];
            Color32[] cols = mesh.colors32;
            return cols[0];
        }
        catch (System.Exception e){
            Debug.LogError("Couldn't get atom color "+e);
        }
        return Color.white;
    }

    public override UnityMolRepresentationParameters Save() {
        UnityMolRepresentationParameters res = new UnityMolRepresentationParameters();

        res.repT.bondType = BondType.hbondtube;
        res.colorationType = bondRep.colorationType;

        if (res.colorationType == colorType.custom) {
            res.colorPerAtom = new Dictionary<UnityMolAtom, Color32>(bondRep.selection.Count);
            foreach (UnityMolAtom a in bondRep.atomToMeshes.Keys) {
                res.colorPerAtom[a] = getAtomColor(a);
            }

        }
        else if (res.colorationType == colorType.full) { //Get color of first atom/residue
            foreach (UnityMolAtom a in bondRep.atomToMeshes.Keys) {
                res.fullColor = getAtomColor(a);
                break;
            }
        }
        else if (res.colorationType == colorType.bfactor) {
            res.bfactorStartColor = bondRep.bfactorStartCol;
            res.bfactorEndColor = bondRep.bfactorEndCol;
        }
        return res;
    }
    public override void Restore(UnityMolRepresentationParameters savedParams) {
        if (savedParams.repT.bondType == BondType.hbondtube) {
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
            bondRep.colorationType = savedParams.colorationType;
        }
        else {
            Debug.LogError("Could not restore representation parameteres");
        }
    }
}
}