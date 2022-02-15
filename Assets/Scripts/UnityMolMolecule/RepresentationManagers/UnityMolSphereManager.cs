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
using System.Linq;
using UnityEngine.Rendering;

namespace UMol {

public class UnityMolSphereManager : UnityMolGenericRepresentationManager {

    private Dictionary<UnityMolAtom, int> atomToId;

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
        nbAtoms = atomRep.selection.Count;
        atomToId = atomRep.atomToId;
        highlightMat = (Material) Resources.Load("Materials/HighlightMaterial");

        isInit = true;
        isEnabled = true;
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

        nbAtoms = 0;
        meshesGO.Clear();
        atomToId.Clear();

        atomRep = null;
        meshesGO = null;
        atomToId = null;

        isInit = false;
        isEnabled = false;
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

    public override void SetColor(Color col, UnityMolSelection sele) {
        foreach (UnityMolAtom a in sele.atoms) {
            SetColor(col, a);
        }
    }

    public void SetColor(Color col, int atomNum) {
        MaterialPropertyBlock props = new MaterialPropertyBlock();
        Renderer r = meshesGO[atomNum].GetComponent<Renderer>();
        r.GetPropertyBlock(props);
        props.Clear();
        props.SetColor("_Color", col);
        r.SetPropertyBlock(props);
    }

    public override void SetColor(Color col, UnityMolAtom atom) {
        int idInCoord = 0;
        if (atomToId.TryGetValue(atom, out idInCoord)) {
            SetColor(col, idInCoord);
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
        SetColor(atom.color, atom);
    }

    public override void ResetColors() {
        foreach (UnityMolAtom a in atomRep.selection.atoms) {
            SetColor(a.color, a);
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
    public override UnityMolRepresentationParameters Save(){
        UnityMolRepresentationParameters res = new UnityMolRepresentationParameters();
        return res;
    }
    public override void Restore(UnityMolRepresentationParameters savedParams){
        
    }
}
}