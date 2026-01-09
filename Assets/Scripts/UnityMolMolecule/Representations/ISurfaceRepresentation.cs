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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

using BurstGridSearch;


namespace UMol {
public abstract class ISurfaceRepresentation : AtomRepresentation {

    public bool isStandardSurface = true;

    public List<GameObject> meshesGO;
    public Dictionary<GameObject, Color32[]> meshColors;

    public Dictionary<UnityMolChain, GameObject> chainToGo;
    public Dictionary<UnityMolChain, int> chainToIdSubSel;
    public Color32[] colorByAtom;
    public List<int[]> vertToAtom;//Each vertex has an atom id associated, one array for each subselection

    public List<UnityMolSelection> subSelections;
    public GameObject newRep;
    public bool useAO = false;
    public Material normalMat;
    public Material transMat;
    public Material transMatShadow;//Usefull for outline
    public Material wireMat;
    public Material currentMat;



    public static List<UnityMolSelection> cutSelection(UnityMolSelection sele) {
        List<UnityMolSelection> res = new List<UnityMolSelection>();
        Dictionary<string, UnityMolSelection> segDict = new Dictionary<string, UnityMolSelection>();

        for (int i = 0; i < sele.atoms.Count; i++) {
            UnityMolAtom atom = sele.atoms[i];
            string segKey = atom.residue.chain.model.structure.name + "_" +
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
        if (sele.extractTrajFrame) {
            //Copy extracted positions

            foreach (UnityMolSelection sel in segDict.Values) {
                List<Vector3[]> partExtratedPos = new List<Vector3[]>();

                for (int i = 0; i < sele.extractTrajFrameIds.Count; i++) {
                    Vector3[] tmp = new Vector3[sel.atoms.Count];
                    int ida = 0;
                    foreach (UnityMolAtom a in sel.atoms) {
                        tmp[ida++] = sele.extractTrajFramePositions[i][sele.atomToIdInSel[a]];
                    }
                    partExtratedPos.Add(tmp);
                }

                sel.updateRepWithTraj = false;
                sel.extractTrajFrame = true;
                sel.extractTrajFrameIds =  sele.extractTrajFrameIds;
                sel.extractTrajFramePositions = partExtratedPos;
            }

        }

        return res;
    }

    public virtual void Clear() {//Warning: it does not destroy the materials and colorByAtom/meshColors are not cleared
        if (meshesGO != null) {
            foreach (GameObject go in meshesGO) {
                GameObject.Destroy(go.GetComponent<MeshFilter>().sharedMesh);
                GameObject.Destroy(go);
            }
            meshesGO.Clear();
        }
        if (chainToGo != null)
            chainToGo.Clear();
        if (chainToIdSubSel != null)
            chainToIdSubSel.Clear();
        // meshColors.Clear();
        //colorByAtom.Clear();
    }
    public virtual int[] computeNearestVertexPerAtom(GameObject gomesh, UnityMolSelection sele, bool removeHETWater = true) {
        UnityMolSelection newSel = null;

        if (sele.Count == 0) {
            return null;
        }

        if (removeHETWater) {
            List<UnityMolAtom> atoms = new List<UnityMolAtom>(sele.atoms.Count);
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

        if (verts.Length < 3 ) {
            return null;
        }

        NativeArray<int> result = sele.structures[0].spatialSearch.SearchClosestPoint(verts);

        int[] idPerVer = new int[verts.Length];
        for (int i = 0; i < verts.Length; i++) {
            int atomId = result[i];
            if (atomId >= 0 && atomId < newSel.atoms.Count) {
                idPerVer[i] = atomId;
            }
        }
        result.Dispose();
        return idPerVer;
    }

    public void getMeshColors() {
        meshColors.Clear();
        Color32 white = Color.white;

        foreach (GameObject go in meshesGO) {
            Mesh m = go.GetComponent<MeshFilter>().sharedMesh;
            Color32[] tmpArrayColor = m.colors32;
            if (tmpArrayColor == null || tmpArrayColor.Length == 0) {
                tmpArrayColor = new Color32[m.vertexCount];
                for (int i = 0; i < m.vertexCount; i++) {
                    tmpArrayColor[i] = white;
                }
            }
            meshColors[go] = tmpArrayColor;
        }
    }
    // public void getMeshColorsPerAtom() {
    //     //Warning: This function expects that getMeshColors() was called before!
    //     if (colorByAtom == null) {
    //         colorByAtom = new Color32[selection.atoms.Count];
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
        if (colorByAtom != null && chainToGo != null) {
            int offset = 0;
            for (int i = 0; i < meshesGO.Count; i++) {
                Color32[] cols = meshColors[meshesGO[i]];
                int[] vtoa = vertToAtom[i];
                for (int v = 0; v < cols.Length; v++) {
                    int idA = vtoa[v];

                    if (idA >= 0 && idA + offset < colorByAtom.Length) {
                        cols[v] = colorByAtom[idA + offset];
                    }
                }
                offset += subSelections[i].Count;
            }
            foreach (GameObject go in meshesGO) {
                go.GetComponent<MeshFilter>().sharedMesh.SetColors(meshColors[go]);
            }
        }
    }

    public abstract void recompute(bool isTraj = false);
}
}