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
using System;
using System.IO;

using Unity.Collections;
using Unity.Mathematics;
using KNN;
using KNN.Jobs;
using Unity.Jobs;

namespace UMol {
public abstract class ISurfaceRepresentation : AtomRepresentation {

    public bool isStandardSurface = true;

    public List<GameObject> meshesGO;
    public Dictionary<GameObject, List<Color32>> meshColors;

    public Dictionary<UnityMolAtom, GameObject> atomToGo;
    public Dictionary<UnityMolAtom, List<int>> atomToMesh;
    public Dictionary<UnityMolAtom, Color32> colorByAtom;

    public List<UnityMolSelection> subSelections;
    public GameObject newRep;


    public static List<UnityMolSelection> cutSelection(UnityMolSelection sele) {
        List<UnityMolSelection> res = new List<UnityMolSelection>();
        Dictionary<string, UnityMolSelection> segDict = new Dictionary<string, UnityMolSelection>();

        for (int i = 0; i < sele.atoms.Count; i++) {
            UnityMolAtom atom = sele.atoms[i];
            string segKey = atom.residue.chain.model.structure.uniqueName + "_" +
                            atom.residue.chain.model.name + "_" +
                            atom.residue.chain.name;

            UnityMolSelection curSeg;
            if (segDict.TryGetValue(segKey, out curSeg)) {
                curSeg.atoms.Add(atom);
            }
            else {
                curSeg = new UnityMolSelection(atom, newBonds: null, segKey);
                res.Add(curSeg);
                segDict[segKey] = curSeg;
            }
        }

        return res;
    }

    public virtual void Clear() {
        foreach (GameObject go in meshesGO) {
            GameObject.DestroyImmediate(go);
        }
        meshesGO.Clear();
        meshColors.Clear();
        atomToGo.Clear();
        atomToMesh.Clear();
    }
    public virtual int[] computeNearestVertexPerAtom(GameObject gomesh, UnityMolSelection sele, bool removeHETWater = true) {
        UnityMolSelection newSel = null;
        if (removeHETWater) {
            List<UnityMolAtom> atoms = new List<UnityMolAtom>(sele.Count);
            foreach (UnityMolAtom a in sele.atoms) {
                if (!a.isHET && !WaterSelection.waterResidues.Contains(a.residue.name, StringComparer.OrdinalIgnoreCase)) {
                    atoms.Add(a);
                }
            }
            newSel = new UnityMolSelection(atoms, newBonds: null, "tmpSel");
        }
        else {
            newSel = sele;
        }

        Vector3[] verts = gomesh.GetComponent<MeshFilter>().sharedMesh.vertices;


#if True // The fast C# jobs implementation

        if(verts.Length < 3){
            return null;
        }
        var allAtoms = new NativeArray<float3>(newSel.atoms.Count, Allocator.Persistent);
        var allVerts = new NativeArray<float3>(verts.Length, Allocator.Persistent);
        for (int i = 0; i < newSel.atoms.Count; i++) {
            allAtoms[i] = newSel.atoms[i].position;
        }
        for (int i = 0; i < verts.Length; i++) {
            allVerts[i] = verts[i];
        }

        int kNeighbours = 1;
        //Create KDTree
        var knnContainer = new KnnContainer(allAtoms, false, Allocator.TempJob);
        var queryPositions = new NativeArray<float3>(allVerts, Allocator.TempJob);
        var results = new NativeArray<int>(verts.Length * kNeighbours, Allocator.TempJob);

        var rebuildJob = new KnnRebuildJob(knnContainer);
        rebuildJob.Schedule().Complete();

        var batchQueryJob = new QueryKNearestBatchJob(knnContainer, queryPositions, results);
        batchQueryJob.ScheduleBatch(queryPositions.Length, queryPositions.Length / 32).Complete();

        int[] idPerVer = new int[verts.Length];
        for (int i = 0; i < verts.Length; i++) {
            int atomId = results[i];
            if (atomId >= 0 && atomId < newSel.atoms.Count) {
                addToDicCorres(newSel.atoms[atomId], i);
            }
            else {
                atomId = -1;
            }
            idPerVer[i] = atomId;
        }

        knnContainer.Dispose();
        queryPositions.Dispose();
        results.Dispose();
        allVerts.Dispose();
        allAtoms.Dispose();

        return idPerVer;

#else // Standard C# KDTree implementation => SLOW

        KDTree kdtree = KDTree.MakeFromUnityMolAtoms(newSel.atoms);

        int[] idPerVer = new int[verts.Length];
        for (int i = 0; i < verts.Length; i++) {
            int atomId = kdtree.FindNearest(verts[i]);
            if (atomId >= 0 && atomId < newSel.atoms.Count) {
                addToDicCorres(newSel.atoms[atomId], i);
            }
            else {
                atomId = -1;
            }
            idPerVer[i] = atomId;
        }
        return idPerVer;
#endif
    }
    
    public virtual Dictionary<UnityMolAtom, List<int>> computeNearestVertexPerAtomCustom(GameObject gomesh, UnityMolSelection sele) {
        KDTree kdtree = KDTree.MakeFromUnityMolAtoms(sele.atoms);
        Dictionary<UnityMolAtom, List<int>> res = new Dictionary<UnityMolAtom, List<int>>();

        Vector3[] verts = gomesh.GetComponent<MeshFilter>().sharedMesh.vertices;
        for (int i = 0; i < verts.Length; i++) {
            int atomId = kdtree.FindNearest(verts[i]);
            if (atomId >= 0 && atomId < sele.atoms.Count) {
                addToDicCorres(sele.atoms[atomId], i, res);
            }
        }
        return res;
    }
    public virtual void addToDicCorres(UnityMolAtom a, int i) {
        List<int> res = null;
        if (atomToMesh.TryGetValue(a, out res)) {
            res.Add(i);
        }
        else {
            res = new List<int>();
            res.Add(i);
            atomToMesh.Add(a, res);
        }
    }
    public static void addToDicCorres(UnityMolAtom a, int i, Dictionary<UnityMolAtom, List<int>> customDic) {
        List<int> res = null;
        if (customDic.TryGetValue(a, out res)) {
            res.Add(i);
        }
        else {
            res = new List<int>();
            res.Add(i);
            customDic.Add(a, res);
        }
    }

    public void getMeshColors() {
        meshColors.Clear();

        foreach (GameObject go in meshesGO) {
            Mesh m = go.GetComponent<MeshFilter>().sharedMesh;
            Color32[] tmpArrayColor = m.colors32;
            if (tmpArrayColor == null || tmpArrayColor.Length == 0) {
                tmpArrayColor = new Color32[m.vertices.Length];
            }
            meshColors[go] = tmpArrayColor.ToList();
        }
    }
    // public void getMeshColorsPerAtom() {
    //     //Warning: This function expects that getMeshColors() was called before!
    //     if (colorByAtom == null) {
    //         colorByAtom = new Color32[selection.Count];
    //     }


    //     int idA = 0;
    //     foreach (UnityMolAtom a in selection.atoms) {

    //         Color32 col = Color.white;
    //         List<int> listVertId;

    //         if (atomToMesh.TryGetValue(a, out listVertId)) {
    //             GameObject curGo = atomToGo[a];
    //             List<Color32> colors = meshColors[curGo];

    //             if (listVertId.Count != 0 && listVertId[0] >= 0 && listVertId[0] < colors.Count) {
    //                 col = colors[listVertId[0]];
    //             }
    //         }
    //         colorByAtom[idA++] = col;
    //     }
    // }

    public void restoreColorsPerAtom() {
        if (colorByAtom != null && atomToMesh != null && atomToGo != null) {

            for (int idA = 0; idA < selection.Count; idA++) {
                UnityMolAtom a = selection.atoms[idA];
                Color32 col = colorByAtom[a];

                List<int> listVertId;
                // Color32 col = colorByAtom[idA];

                if (atomToMesh.TryGetValue(a, out listVertId)) {
                    GameObject curGo = atomToGo[a];

                    List<Color32> colors = meshColors[curGo];


                    foreach (int c in listVertId) {
                        if (c >= 0 && c < colors.Count) {
                            Color32 tmp = colors[c];
                            col.a = tmp.a;
                            colors[c] = col;
                        }
                    }
                }
            }
            foreach (GameObject go in meshesGO) {
                go.GetComponent<MeshFilter>().sharedMesh.SetColors(meshColors[go]);
            }
        }
    }


    public abstract void recompute();
}
}