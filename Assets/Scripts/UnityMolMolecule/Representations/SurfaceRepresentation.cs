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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR;
using VRTK;

namespace UMol {
public class SurfaceRepresentation : ISurfaceRepresentation {

    public SurfMethod surfMethod;
    public bool useAO;
    public bool isCutSurface;
    public bool isCutByChain;
    public float probeRadius = 1.4f;

    public SurfaceRepresentation(string structName, UnityMolSelection sel, bool cutByChain = true,
                                 bool AO = true, bool cutSurface = true, SurfMethod method = SurfMethod.EDTSurf, float probeRad = 1.4f) {

        colorationType = colorType.full;
        meshesGO = new List<GameObject>();
        atomToGo = new Dictionary<UnityMolAtom, GameObject>();
        meshColors = new Dictionary<GameObject, List<Color32>>();
        atomToMesh = new Dictionary<UnityMolAtom, List<int>>();
        colorByAtom = new Dictionary<UnityMolAtom, Color32>();

        selection = sel;
        surfMethod = method;
        useAO = AO;
        isCutByChain = cutByChain;
        isCutSurface = cutSurface;
        probeRadius = probeRadius;

#if (!UNITY_EDITOR_WIN) && (!UNITY_STANDALONE_WIN)
        if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore) {
            //Desactivate AO for non windows bc of a bug in the AO implementation
            useAO = false;
        }
#endif

        GameObject loadedMolGO = UnityMolMain.getRepresentationParent();

        representationParent = loadedMolGO.transform.Find(structName);
        if (UnityMolMain.inVR() && representationParent == null) {

            Transform clref = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController);
            Transform crref = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController);
            if (clref != null) {
                representationParent = clref.Find(structName);
            }
            if (representationParent == null && crref != null) {
                representationParent = crref.Find(structName);
            }
        }
        if (representationParent == null) {
            representationParent = (new GameObject(structName).transform);
            representationParent.parent = loadedMolGO.transform;
            representationParent.localPosition = Vector3.zero;
            representationParent.localRotation = Quaternion.identity;
            representationParent.localScale = Vector3.one;
        }
        newRep = new GameObject("AtomSurfaceRepresentation");
        newRep.transform.parent = representationParent;
        representationTransform = newRep.transform;

        if (isCutByChain) {
            subSelections = cutSelection(selection);
        }
        else {
            subSelections = new List<UnityMolSelection>() {selection};
        }


        //Check if we try to compute the surface for ligand or hetero atoms
        if (isCutSurface) {

            if (checkHeteroOrLigand(selection)) {
                isCutSurface = false;
                Debug.LogWarning("Forcing cut surface off, the selection contains only ligand or hetero atoms");
            }
            if (!selection.sameModel()) {
                isCutSurface = false;
                Debug.LogWarning("Forcing cut surface off, the selection contains atoms from different models");
            }
        }

        float timerSES = Time.realtimeSinceStartup;

        foreach (UnityMolSelection s in subSelections) {
            int[] atomIdPerVert = null;
            bool success = displaySurfaceMesh(s.name, s, newRep.transform, ref atomIdPerVert);
            if (success) {
                if (atomIdPerVert == null) {//Shouldn't happen as all surf methods generate atomIdPerVert array
                    computeNearestVertexPerAtom(meshesGO.Last(), s);
                }
            }
        }

        #if UNITY_EDITOR
        Debug.Log("Total time : " + (1000.0f * (Time.realtimeSinceStartup - timerSES)).ToString("f3") + " ms");
        #endif


        if (meshesGO.Count > 0 && useAO) {
            try {
                GameObject aoGo = new GameObject("tmpAO");
                geoAO aoScript = aoGo.AddComponent<geoAO>();
                aoScript.ComputeAO(meshesGO);
                GameObject.Destroy(aoGo);
            }
            catch {
                Debug.LogWarning("Could not compute AO");
            }
        }

        getMeshColors();

        Color32 white = Color.white;
        foreach (UnityMolAtom a in selection.atoms) {
            colorByAtom[a] = white;
        }

        newRep.transform.localPosition = Vector3.zero;
        newRep.transform.localRotation = Quaternion.identity;
        newRep.transform.localScale = Vector3.one;

        nbAtoms = selection.Count;

    }

    private bool displaySurfaceMesh(string name, UnityMolSelection selection, Transform repParent, ref int[] atomIdPerVert) {

        if (selection.Count <= 0) {
            return false;
        }
        GameObject go = null;
        UnityMolSelection sel = selection;
        MeshData meshd = null;
        if (isCutSurface && isCutByChain) {
            sel = selection.atoms[0].residue.chain.ToSelection(false);
            sel.isAlterable = true;
        }
        else if (isCutSurface && !isCutByChain) {
            sel = selection.atoms[0].residue.chain.model.structure.ToSelection();
            sel.isAlterable = true;
        }

        string keyPrecomputedRep = sel.atoms[0].residue.chain.model.structure.uniqueName + "_" + sel.atoms[0].residue.chain.name + "_" + surfMethod.ToString();

        if (sel.Count <= 10) {
            Debug.LogWarning("Forcing MSMS surface for small selections");
            go = MSMSWrapper.createMSMSSurface(repParent, name, sel, ref meshd, ref atomIdPerVert, probeRad: probeRadius);

            if (meshd != null && meshd.vertices != null) { //Save the surface as precomputed
                UnityMolMain.getPrecompRepManager().precomputedRep[keyPrecomputedRep] = meshd;
                UnityMolMain.getPrecompRepManager().precomputedSurfAsso[keyPrecomputedRep] = atomIdPerVert;
            }
        }
        else {

            bool alreadyComputed = UnityMolMain.getPrecompRepManager().ContainsRep(keyPrecomputedRep);

            if (isCutSurface && isCutByChain) { //Use precomputed surface

                if (alreadyComputed) { //Already pre-computed => use the saved mesh
                    meshd = UnityMolMain.getPrecompRepManager().precomputedRep[keyPrecomputedRep];
                    go = createUnityMesh(meshd, repParent, name);
                    //Restore association between atoms and vertices
                    atomIdPerVert = UnityMolMain.getPrecompRepManager().precomputedSurfAsso[keyPrecomputedRep];

                    if (atomIdPerVert != null) {
                        for (int i = 0; i < atomIdPerVert.Length; i++) {
                            int atomId = atomIdPerVert[i];
                            if (atomId >= 0 && atomId < sel.Count) {
                                addToDicCorres(sel.atoms[atomId], i);
                            }
                        }
                    }
                }
                else { //Not precomputed => compute it
                    if (surfMethod == SurfMethod.MSMS) {
                        go = MSMSWrapper.createMSMSSurface(repParent, name, sel, ref meshd, ref atomIdPerVert, probeRad: probeRadius);

                        for (int i = 0; i < atomIdPerVert.Length; i++) {
                            int atomId = atomIdPerVert[i];
                            if (atomId >= 0 && atomId < sel.Count) {
                                addToDicCorres(sel.atoms[atomId], i);
                            }
                        }

                    }
                    else if (surfMethod == SurfMethod.EDTSurf) {
                        go = EDTSurfWrapper.createEDTSurface(repParent, name, sel, ref atomIdPerVert, ref meshd);

                        for (int i = 0; i < atomIdPerVert.Length; i++) {
                            int atomId = atomIdPerVert[i];
                            if (atomId >= 0 && atomId < sel.Count) {
                                addToDicCorres(sel.atoms[atomId], i);
                            }
                        }
                    }
                    else if (surfMethod == SurfMethod.QUICKSES) {
                        go = WrapperCudaSES.createCUDASESSurface(repParent, name, sel, ref atomIdPerVert, ref meshd);

                        for (int i = 0; i < atomIdPerVert.Length; i++) {
                            int atomId = atomIdPerVert[i];
                            if (atomId >= 0 && atomId < sel.Count) {
                                addToDicCorres(sel.atoms[atomId], i);
                            }
                        }
                    }

                    if (meshd != null && meshd.vertices != null) { //Save the surface as precomputed
                        UnityMolMain.getPrecompRepManager().precomputedRep[keyPrecomputedRep] = meshd;
                        UnityMolMain.getPrecompRepManager().precomputedSurfAsso[keyPrecomputedRep] = atomIdPerVert;
                    }
                }
            }

            else { //Need to compute the surface because isCutSurface = false or isCutByChain = false
                if (surfMethod == SurfMethod.MSMS) {
                    go = MSMSWrapper.createMSMSSurface(repParent, name, sel, ref meshd, ref atomIdPerVert, probeRad: probeRadius);

                    for (int i = 0; i < atomIdPerVert.Length; i++) {
                        int atomId = atomIdPerVert[i];
                        if (atomId >= 0 && atomId < sel.Count) {
                            addToDicCorres(sel.atoms[atomId], i);
                        }
                    }
                }
                else if (surfMethod == SurfMethod.EDTSurf) {
                    go = EDTSurfWrapper.createEDTSurface(repParent, name, sel, ref atomIdPerVert, ref meshd);
                    if(atomIdPerVert != null){
                        for (int i = 0; i < atomIdPerVert.Length; i++) {
                            int atomId = atomIdPerVert[i];
                            if (atomId >= 0 && atomId < sel.Count) {
                                addToDicCorres(sel.atoms[atomId], i);
                            }
                        }
                    }
                }
                else if (surfMethod == SurfMethod.QUICKSES) {
                    go = WrapperCudaSES.createCUDASESSurface(repParent, name, sel, ref atomIdPerVert, ref meshd);

                    for (int i = 0; i < atomIdPerVert.Length; i++) {
                        int atomId = atomIdPerVert[i];
                        if (atomId >= 0 && atomId < sel.Count) {
                            addToDicCorres(sel.atoms[atomId], i);
                        }
                    }
                }
            }
        }

        if (go != null) {
            meshesGO.Add(go);
            foreach (UnityMolAtom a in selection.atoms) {
                atomToGo.Add(a, go);
            }

            if (isCutSurface) { //Remove triangles for atoms not in selection
                if (sel.Count == selection.Count) {
                    return true;
                }


                HashSet<int> vertIdToDel = new HashSet<int>();
                HashSet<UnityMolAtom> selectionhs = new HashSet<UnityMolAtom>(selection.atoms);

                foreach (UnityMolAtom a in sel.atoms) {
                    if (!selectionhs.Contains(a)) {
                        if (atomToMesh.ContainsKey(a)) {
                            List<int> vertToDel = atomToMesh[a];
                            foreach (int v in vertToDel) {
                                vertIdToDel.Add(v);
                            }
                        }
                    }
                }


                //Update the mesh => recreate a triangle array
                Mesh m = go.GetComponent<MeshFilter>().sharedMesh;
                Vector3[] vertices = m.vertices;
                int[] triangles = m.triangles;
                List<int> newTri = new List<int>(triangles.Length);
                for (int t = 0; t < triangles.Length - 3; t += 3) {
                    if (vertIdToDel.Contains(triangles[t])) {
                        continue;
                    }
                    if (vertIdToDel.Contains(triangles[t + 1])) {
                        continue;
                    }
                    if (vertIdToDel.Contains(triangles[t + 2])) {
                        continue;
                    }
                    newTri.Add(triangles[t]);
                    newTri.Add(triangles[t + 1]);
                    newTri.Add(triangles[t + 2]);
                }

                m.triangles = newTri.ToArray();
                if (newTri.Count == 0) {
                    Debug.LogWarning("The surface might not show because every triangles have been hidden");
                }

            }

            return true;
        }
        return false;
    }

    public override void recompute() {

        List<Material> savedMat = new List<Material>();
        foreach (GameObject m in meshesGO) {
            savedMat.Add(m.GetComponent<MeshRenderer>().sharedMaterial);
        }

        // getMeshColorsPerAtom();

        Clear();

        if (isCutByChain) {
            subSelections = cutSelection(selection);
        }
        else {
            subSelections = new List<UnityMolSelection>() {selection};
        }

        foreach (UnityMolSelection sel in subSelections) {
            int[] atomIdPerVert = null;
            bool success = displaySurfaceMesh(sel.name, sel, newRep.transform, ref atomIdPerVert);

            if (success) {
                if (atomIdPerVert == null) {//Shouldn't happen as all surf methods generate atomIdPerVert array
                    computeNearestVertexPerAtom(meshesGO.Last(), sel);
                }
            }
        }

        if (meshesGO.Count > 0 && meshesGO.Count == savedMat.Count) {
            int i = 0;
            foreach (GameObject m in meshesGO) {
                m.GetComponent<MeshRenderer>().sharedMaterial = savedMat[i++];
            }
        }

        if (meshesGO.Count > 0 && useAO) {
            try {
                GameObject aoGo = new GameObject("tmpAO");
                geoAO aoScript = aoGo.AddComponent<geoAO>();
                aoScript.ComputeAO(meshesGO);
                GameObject.Destroy(aoGo);
            }
            catch {
                Debug.LogWarning("Could not compute AO");
            }
        }
        getMeshColors();

        restoreColorsPerAtom();
    }

    GameObject createUnityMesh(MeshData mData, Transform meshPar, string name) {

        GameObject newMeshGo = new GameObject(name);
        newMeshGo.transform.parent = meshPar;
        newMeshGo.transform.localPosition = Vector3.zero;
        newMeshGo.transform.localRotation = Quaternion.identity;
        newMeshGo.transform.localScale = Vector3.one;

        Mesh newMesh = new Mesh();
        newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        newMesh.vertices = mData.vertices;
        newMesh.triangles = mData.triangles;
        newMesh.colors32 = mData.colors;

        newMesh.RecalculateNormals();

        MeshFilter mf = newMeshGo.AddComponent<MeshFilter>();
        mf.mesh = newMesh;

        MeshRenderer mr = newMeshGo.AddComponent<MeshRenderer>();

        Material mat = new Material(Shader.Find("Custom/SurfaceVertexColor"));
        mat.SetFloat("_Glossiness", 0.0f);
        mat.SetFloat("_Metallic", 0.0f);
        if (useAO) {
            mat.SetFloat("_AOIntensity", 1.03f);
            mat.SetFloat("_AOPower", 8.0f);
        }
        else {
            mat.SetFloat("_AOIntensity", 0.0f);
            mat.SetFloat("_AOPower", 0.0f);
        }
        mr.material = mat;

        return newMeshGo;
    }

    bool checkHeteroOrLigand(UnityMolSelection sel) {

        MDAnalysisSelection selec = new MDAnalysisSelection("ligand or ions", sel.atoms);
        UnityMolSelection ret = selec.process();
        HashSet<UnityMolAtom> ligAtoms = new HashSet<UnityMolAtom>(ret.atoms);

        foreach (UnityMolAtom a in sel.atoms) {
            if (!a.isHET && !ligAtoms.Contains(a)) {
                return false;
            }
        }
        return true;
    }
    public override void Clean() {}
}
}
