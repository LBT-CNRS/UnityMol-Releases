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

/// <summary>
/// Class handling the Surface Representation
/// Contains the method for the computation and the meshData
/// </summary>
public class SurfaceRepresentation: ISurfaceRepresentation {

    /// <summary>
    /// Enum for the algorithm of surface calculation
    /// </summary>
    public SurfMethod SurfMethod { get; set;}

    /// <summary>
    /// Wether ligands or hetero atoms are excluded from the surface
    /// </summary>
    public bool IsCutSurface { get; set; }

    /// <summary>
    /// Wether the surface is cut for each chain of a selection
    /// </summary>
    public bool IsCutByChain { get; set; }

    /// <summary>
    /// Radius of the probe used the surface calculation (in A)
    /// </summary>
    private readonly float _probeRadius;

    /// <summary>
    /// List of computed surfaces (to avoid allocations in trajectory)
    /// </summary>
    private List<MeshData> _allMeshData;

    /// <summary>
    /// Constructor to generate a surface
    /// Generate a GameObject 'AtomSurfaceRepresentation' which hold the information
    /// </summary>
    /// <param name="idF">Frame Id in case of trajectory</param>
    /// <param name="structName">the name of the UnityMolStructure</param>
    /// <param name="sel">the UnityMolSelection to </param>
    /// <param name="cutByChain">cut the surface for each chain if true</param>
    /// <param name="AO">activate ambient occlusion</param>
    /// <param name="cutSurface">exclude heteros atoms for the surface if true</param>
    /// <param name="method">Algorithm used for the surface generation</param>
    /// <param name="probeRad">radius of the probe of the algorithm (in A)</param>
    public SurfaceRepresentation(int idF, string structName, UnityMolSelection sel, bool cutByChain = true,
                                 bool AO = true, bool cutSurface = true, SurfMethod method = SurfMethod.EDTSurf, float probeRad = 1.4f) {

        colorationType = colorType.full;
        meshesGO = new List<GameObject>();
        meshColors = new Dictionary<GameObject, Color32[]>();
        chainToGo = new Dictionary<UnityMolChain, GameObject>();
        chainToIdSubSel = new Dictionary<UnityMolChain, int>();

        colorByAtom = new Color32[sel.atoms.Count];
        normalMat = new Material(Shader.Find("Custom/SurfaceVertexColor")) {
            enableInstancing = true
        };


        selection = sel;
        SurfMethod = method;
        useAO = AO;
        IsCutByChain = cutByChain;
        IsCutSurface = cutSurface;
        _probeRadius = probeRad;
        idFrame = idF;

#if (!UNITY_EDITOR_WIN) && (!UNITY_STANDALONE_WIN)
        if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore) {
            //Desactivate AO for non windows bc of a bug in the AO implementation
            useAO = false;
        }
#endif

        //Disable AO for quicksurf by default
        if (SurfMethod == SurfMethod.QUICKSURF) {
            useAO = false;
        }

        normalMat.SetFloat("_Glossiness", 0.0f);
        normalMat.SetFloat("_Metallic", 0.0f);
        if (useAO) {
            normalMat.SetFloat("_AOIntensity", 8.0f);
        }
        else {
            normalMat.SetFloat("_AOIntensity", 0.0f);
        }

        currentMat = normalMat;


        representationParent = UnityMolMain.getRepStructureParent(structName).transform;

        newRep = new GameObject("AtomSurfaceRepresentation");
        newRep.transform.parent = representationParent;
        representationTransform = newRep.transform;

        //TODO: implement this!
        if (sel.extractTrajFrame && IsCutSurface) {
            Debug.LogWarning("Cutting surfaces for frames extracted from a trajectory is not supported for now");
            IsCutSurface = false;
        }

        if (IsCutByChain) {
            subSelections = cutSelection(selection);
        }
        else {
            subSelections = new List<UnityMolSelection>() {selection};
        }

        _allMeshData = new List<MeshData>(subSelections.Count);
        vertToAtom = new List<int[]>(subSelections.Count);


        //Check if we try to compute the surface for ligand or hetero atoms
        if (!sel.extractTrajFrame && IsCutSurface) {

            if (checkHeteroOrLigand(selection)) {
                IsCutSurface = false;
                Debug.LogWarning("Forcing cut surface off, the selection contains only ligand or hetero atoms");
            }
            if (!selection.sameModel()) {
                IsCutSurface = false;
                Debug.LogWarning("Forcing cut surface off, the selection contains atoms from different models");
            }
        }

        float timerSES = Time.realtimeSinceStartup;

        int idSubSel = 0;
        foreach (UnityMolSelection s in subSelections) {
            int[] atomIdPerVert = null;
            bool success = displaySurfaceMesh(s.name, s, newRep.transform, ref atomIdPerVert, idSubSel, false);
            if (success) {
                if (atomIdPerVert == null) {//Shouldn't happen as all surf methods generate atomIdPerVert array
                    atomIdPerVert = computeNearestVertexPerAtom(meshesGO.Last(), s);
                    vertToAtom.Add(atomIdPerVert);
                }
            }
            idSubSel++;
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
        int id = 0;
        foreach (UnityMolAtom a in selection.atoms) {
            colorByAtom[id++] = white;
        }

        newRep.transform.localPosition = Vector3.zero;
        newRep.transform.localRotation = Quaternion.identity;
        newRep.transform.localScale = Vector3.one;

        nbAtoms = selection.atoms.Count;

    }

    /// <summary>
    /// Launch the surface calculation for the UnityMolSelection 'selection'
    /// and update the mesh object
    /// </summary>
    /// <param name="name">name of the selection</param>
    /// <param name="selection">the UnityMolSelection</param>
    /// <param name="repParent">the Transform object of the parent of the selection</param>
    /// <param name="atomIdPerVert">the reference to an array of atomId per vextex</param>
    /// <param name="idSubSel">the id of the sub selection</param>
    /// <param name="isTraj">true if part of a trajectory</param>
    /// <returns></returns>
    private bool displaySurfaceMesh(string name, UnityMolSelection selection,
                                    Transform repParent, ref int[] atomIdPerVert,
                                    int idSubSel, bool isTraj) {

        if (selection.atoms.Count == 0) {
            return false;
        }

        UnityMolSelection sel = selection;
        MeshData meshd = null;

        if (isTraj && idSubSel < _allMeshData.Count) {
            meshd = _allMeshData[idSubSel];
        }


        if (!sel.extractTrajFrame) {

            if (IsCutSurface && IsCutByChain) {
                sel = selection.atoms[0].residue.chain.ToSelection(false);
                sel.isAlterable = true;
            }
            else if (IsCutSurface && !IsCutByChain) {
                sel = selection.atoms[0].residue.chain.model.structure.ToSelection();
                sel.isAlterable = true;
            }
        }

        string keyPrecomputedRep = sel.atoms[0].residue.chain.model.structure.name + "_" + sel.atoms[0].residue.chain.name + "_" + SurfMethod.ToString();

        if (sel.atoms.Count <= 10) {
            Debug.LogWarning("Forcing MSMS surface for small selections");
            MSMSWrapper.createMSMSSurface(idFrame, sel, ref meshd, probeRad: _probeRadius);
            atomIdPerVert = meshd.atomByVert;

            if (!sel.extractTrajFrame && meshd != null && meshd.vertices != null) { //Save the surface as precomputed
                UnityMolMain.getPrecompRepManager().precomputedRep[keyPrecomputedRep] = meshd;
                UnityMolMain.getPrecompRepManager().precomputedSurfAsso[keyPrecomputedRep] = atomIdPerVert;
            }
        }
        else {

            bool alreadyComputed = UnityMolMain.getPrecompRepManager().ContainsRep(keyPrecomputedRep);
            if (sel.extractTrajFrame) {
                alreadyComputed = false;
            }

            if (IsCutSurface && IsCutByChain) { //Use precomputed surface

                if (alreadyComputed) { //Already pre-computed => use the saved mesh
                    meshd = UnityMolMain.getPrecompRepManager().precomputedRep[keyPrecomputedRep];
                    //Restore association between atoms and vertices
                    atomIdPerVert = UnityMolMain.getPrecompRepManager().precomputedSurfAsso[keyPrecomputedRep];
                }
                else { //Not precomputed => compute it
                    if (SurfMethod == SurfMethod.MSMS) {
                        MSMSWrapper.createMSMSSurface(idFrame, sel, ref meshd, probeRad: _probeRadius);
                    }
                    else if (SurfMethod == SurfMethod.EDTSurf) {
                        EDTSurfWrapper.createEDTSurface(idFrame, name, sel, ref meshd);
                    }
                    else if (SurfMethod == SurfMethod.QUICKSES) {
                        WrapperCudaSES.createCUDASESSurface(idFrame, sel, ref meshd);
                    }

                    else if (SurfMethod == SurfMethod.QUICKSURF) {
                        WrapperCudaQuickSurf.createSurface(idFrame, sel, ref meshd);
                    }

                    if (meshd != null && meshd.vertices != null) { //Save the surface as precomputed
                        atomIdPerVert = meshd.atomByVert;
                        UnityMolMain.getPrecompRepManager().precomputedRep[keyPrecomputedRep] = meshd;
                        UnityMolMain.getPrecompRepManager().precomputedSurfAsso[keyPrecomputedRep] = atomIdPerVert;
                    }
                }
            }

            else { //Need to compute the surface because isCutSurface = false or isCutByChain = false
                if (SurfMethod == SurfMethod.MSMS) {
                    MSMSWrapper.createMSMSSurface(idFrame, sel, ref meshd, probeRad: _probeRadius);
                }
                else if (SurfMethod == SurfMethod.EDTSurf) {
                    EDTSurfWrapper.createEDTSurface(idFrame, name, sel, ref meshd);
                }
                else if (SurfMethod == SurfMethod.QUICKSES) {
                    WrapperCudaSES.createCUDASESSurface(idFrame, sel, ref meshd);
                }
                else if (SurfMethod == SurfMethod.QUICKSURF) {
                    WrapperCudaQuickSurf.createSurface(idFrame, sel, ref meshd);
                }
                if (meshd != null) {
                    atomIdPerVert = meshd.atomByVert;
                }

            }
        }

        if (meshd != null) {

            if (isTraj) {
                if (idSubSel >= _allMeshData.Count) {
                    _allMeshData.Add(meshd);
                }
                if (idSubSel >= meshesGO.Count) {//Need to create the gameobject
                    GameObject go = createUnityMesh(meshd, repParent, name);
                    meshesGO.Add(go);
                    chainToGo[selection.atoms[0].residue.chain] = go;
                    chainToIdSubSel[selection.atoms[0].residue.chain] = idSubSel;
                }
                GameObject curGo = meshesGO[idSubSel];
                Mesh m = curGo.GetComponent<MeshFilter>().sharedMesh;

                m.Clear();
                m.SetVertices(meshd.vertices, 0, meshd.nVert);
                m.SetIndices(meshd.triangles, 0, meshd.nTri * 3, MeshTopology.Triangles, 0, false, 0);
                m.SetColors(meshd.colors, 0, meshd.nVert);

                if (meshd.normals == null || meshd.normals[0] == Vector3.zero) {
                    if (UnityMolMain.raytracingMode) {
                        m.RecalculateNormals(60.0f);
                    }
                    else {
                        m.RecalculateNormals();
                    }
                }
                else {
                    m.SetNormals(meshd.normals, 0, meshd.nVert);
                }
            }
            else {
                GameObject go = createUnityMesh(meshd, repParent, name);
                meshesGO.Add(go);
                chainToGo[selection.atoms[0].residue.chain] = go;
                chainToIdSubSel[selection.atoms[0].residue.chain] = idSubSel;
            }

            if (atomIdPerVert != null) {
                vertToAtom.Add(atomIdPerVert);
            }


            if (IsCutSurface) { //Remove triangles for atoms not in selection
                if (sel.atoms.Count == selection.atoms.Count) {
                    return true;
                }


                HashSet<int> vertIdToDel = new HashSet<int>();
                HashSet<UnityMolAtom> selectionhs = new HashSet<UnityMolAtom>(selection.atoms);

                for (int i = 0; i < meshd.nVert; i++) {
                    int aId = atomIdPerVert[i];
                    if (aId >= sel.atoms.Count || aId < 0 || !selectionhs.Contains(sel.atoms[aId])) {
                        vertIdToDel.Add(i);
                    }
                }

                // Skip if no triangle to remove
                if (vertIdToDel.Count() != 0) {

                    //Update the mesh => recreate a triangle array
                    Mesh m = meshesGO[idSubSel].GetComponent<MeshFilter>().sharedMesh;
                    int[] triangles = m.triangles;
                    List<int> newTri = new List<int>(triangles.Length);
                    for (int t = 0; t <= triangles.Length - 3; t += 3) {
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

                    m.SetTriangles(newTri, 0);
                    if (newTri.Count == 0) {
                        Debug.LogWarning("The surface might not show because every triangles have been hidden");
                    }
                }
            }

            return true;
        }
        return false;
    }

    public override void recompute(bool isTraj = false) {

        // getMeshColorsPerAtom();

        Clear();

        vertToAtom.Clear();

        if (IsCutByChain) {
            subSelections = cutSelection(selection);
        }
        else {
            subSelections = new List<UnityMolSelection>() {selection};
        }

        int idSubSel = 0;
        foreach (UnityMolSelection sel in subSelections) {
            int[] atomIdPerVert = null;
            bool success = displaySurfaceMesh(sel.name, sel, newRep.transform, ref atomIdPerVert, idSubSel, isTraj);

            if (success) {
                if (atomIdPerVert == null) {//Shouldn't happen as all surf methods generate atomIdPerVert array
                    atomIdPerVert = computeNearestVertexPerAtom(meshesGO.Last(), sel);
                }
            }
            idSubSel++;
        }

        if (meshesGO.Count > 0) {
            foreach (GameObject m in meshesGO) {
                m.GetComponent<MeshRenderer>().sharedMaterial = currentMat;
                if (isTraj) {
                    m.GetComponent<Renderer>().sharedMaterial.SetFloat("_AOIntensity", 0.0f);
                }
            }
        }

        if (meshesGO.Count > 0 && useAO && !isTraj) {
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

    /// <summary>
    /// Create a new GameObject holding one surface representation
    /// </summary>
    /// <param name="mData">the MeshData holding the mesh</param>
    /// <param name="meshPar">the parent mesh to attach the new GameObject</param>
    /// <param name="name">the name of the GameObject to create</param>
    /// <returns></returns>
    private GameObject createUnityMesh(MeshData mData, Transform meshPar, string name) {

        GameObject newMeshGo = new GameObject(name);
        newMeshGo.transform.parent = meshPar;
        newMeshGo.transform.localPosition = Vector3.zero;
        newMeshGo.transform.localRotation = Quaternion.identity;
        newMeshGo.transform.localScale = Vector3.one;

        Mesh newMesh = new Mesh {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        newMesh.SetVertices(mData.vertices, 0, mData.nVert);
        newMesh.SetColors(mData.colors, 0, mData.nVert);
        newMesh.SetIndices(mData.triangles, 0, mData.nTri * 3, MeshTopology.Triangles, 0, false, 0);


        if (mData.normals == null || mData.normals[0] == Vector3.zero) {
            if (UnityMolMain.raytracingMode) {
                    newMesh.RecalculateNormals(60.0f);
            }
            else {
                newMesh.RecalculateNormals();
            }
        }
        else {
            newMesh.SetNormals(mData.normals, 0, mData.nVert);
        }

        MeshFilter mf = newMeshGo.AddComponent<MeshFilter>();
        mf.sharedMesh = newMesh;

        MeshRenderer mr = newMeshGo.AddComponent<MeshRenderer>();

        if (normalMat == null) {
            normalMat = new Material(Shader.Find("Custom/SurfaceVertexColor")) {
                enableInstancing = true
            };
        }
        normalMat.SetFloat("_Glossiness", 0.0f);
        normalMat.SetFloat("_Metallic", 0.0f);
        if (useAO) {
            normalMat.SetFloat("_AOIntensity", 8.0f);
        }
        else {
            normalMat.SetFloat("_AOIntensity", 0.0f);
        }
        mr.sharedMaterial = normalMat;
        currentMat = normalMat;

        return newMeshGo;
    }

    /// <summary>
    /// Check if there is ligands or heteros atoms in the selection 'sel'.
    /// Return true if at least one of those atoms is find.
    /// </summary>
    /// <param name="sel">the UnityMolSelection</param>
    /// <returns>True if found ligands or hetero atoms. False otherwise</returns>
    private bool checkHeteroOrLigand(UnityMolSelection sel) {

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

    /// <summary>
    /// Delete and destroy all data (included GameObjects) linked to this surface representation
    /// </summary>
    public override void Clean() {
        Clear();

        colorByAtom = null;

        meshColors.Clear();

        if (normalMat != null) {
            GameObject.Destroy(normalMat);
        }
        normalMat = null;

        if (transMat != null) {
            GameObject.Destroy(transMat);
        }
        transMat = null;

        if (transMatShadow != null) {
            GameObject.Destroy(transMatShadow);
        }
        transMatShadow = null;

        if (wireMat != null) {
            GameObject.Destroy(wireMat);
        }
        wireMat = null;
    }
}
}
