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

namespace UMol {
public class DXSurfaceRepresentation : ISurfaceRepresentation {

    private DXReader dxR;
    public float isoValue = 0.0f;
    public MarchingCubesWrapper mcWrapper;

    public DXSurfaceRepresentation(string structName, UnityMolSelection sel, DXReader dx, float iso) {
        colorationType = colorType.full;
        useAO = false;
        if (dx == null) {
            throw new System.Exception("No DX map loaded");
        }

        isStandardSurface = false;

        dxR = dx;
        isoValue = iso;
        selection = sel;

        meshesGO = new List<GameObject>();
        meshColors = new Dictionary<GameObject, Color32[]>();
        colorByAtom = new Color32[sel.atoms.Count];
        chainToGo = new Dictionary<UnityMolChain, GameObject>();

        representationParent = UnityMolMain.getRepStructureParent(structName).transform;

        newRep = new GameObject("AtomDXSurfaceRepresentation");
        newRep.transform.parent = representationParent;
        representationTransform = newRep.transform;

        subSelections = new List<UnityMolSelection>(1) {selection};

        vertToAtom = new List<int[]>(subSelections.Count);

        mcWrapper = new MarchingCubesWrapper(dxR.densityValues, dxR.gradient,
                       dxR.gridSize, dxR.origin, dxR.deltaS, dxR.cellDir,
                       dxR.cellIdToAtomId, selection.structures[0]);

        foreach (UnityMolSelection s in subSelections) {
            displayDXSurfaceMesh(s.name + "_DXSurface", s, newRep.transform);
        }

        getMeshColors();

        Color32 white = Color.white;
        for (int i = 0; i < selection.atoms.Count; i++) {
            colorByAtom[i] = white;
        }

        newRep.transform.localPosition = Vector3.zero;
        newRep.transform.localRotation = Quaternion.identity;
        newRep.transform.localScale = Vector3.one;

        nbAtoms = selection.atoms.Count;

    }

    private void displayDXSurfaceMesh(string name, UnityMolSelection selection, Transform repParent) {

        GameObject go = createDXSurface(name, repParent);
        if (go != null) {
            meshesGO.Add(go);

            foreach (UnityMolAtom a in selection.atoms) {
                if (!chainToGo.ContainsKey(a.residue.chain)) {
                    chainToGo[a.residue.chain] = go;
                }
            }
        }
    }

    GameObject createDXSurface(string name, Transform repParent) {

        MeshData mdata = mcWrapper.computeMC(isoValue);

        Mesh newMesh = new() {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
            vertices = mdata.vertices,
            triangles = mdata.triangles,
            normals = mdata.normals,
            colors32 = mdata.colors
        };

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject.Destroy(go.GetComponent<BoxCollider>());
        go.GetComponent<MeshFilter>().sharedMesh = newMesh;
        go.transform.SetParent(repParent);

        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        go.name = name;
        if (normalMat == null) {
            normalMat = new Material(Shader.Find("Custom/SurfaceVertexColor"));
            normalMat.SetFloat("_Glossiness", 0.0f);
            normalMat.SetFloat("_Metallic", 0.0f);
            normalMat.SetFloat("_AOIntensity", 0.0f);
            currentMat = normalMat;
        }
        go.GetComponent<MeshRenderer>().sharedMaterial = currentMat;

        vertToAtom.Add(mdata.atomByVert);
        return go;

    }


    public override void recompute(bool isTraj = false) {

        Clear();

        vertToAtom.Clear();

        foreach (UnityMolSelection sel in subSelections) {
            displayDXSurfaceMesh(sel.name, sel, newRep.transform);
        }

        getMeshColors();

        restoreColorsPerAtom();
    }

    public override void Clean() {

        vertToAtom.Clear();
        Clear();
        colorByAtom = null;
        meshColors.Clear();

        if (mcWrapper != null) {
            mcWrapper.FreeMC();
        }
        mcWrapper = null;
        if (normalMat != null)
            GameObject.Destroy(normalMat);
        if (transMat != null)
            GameObject.Destroy(transMat);
        if (wireMat != null)
            GameObject.Destroy(wireMat);
        if (transMatShadow != null)
            GameObject.Destroy(transMatShadow);
    }
}
}
