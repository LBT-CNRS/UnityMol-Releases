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
using Unity.Mathematics;


namespace UMol {

/// <summary>
/// Handles the Hbonds representation
/// Create a GameObject for each Hbond with a Mesh associated to it.
/// </summary>
public class BondRepresentationHbonds : BondRepresentation {

    /// <summary>
    /// List of GameObjects containing the meshes of the Hbonds
    /// </summary>
    public List<GameObject> MeshesGO { get; private set; }

    /// <summary>
    /// Dictionary of UnityMolAtoms involved in at least one Hbond and their associated GameObjects
    /// </summary>
    public Dictionary<UnityMolAtom, List<GameObject>> AtomToGO { get; private set; }

    /// <summary>
    /// Dictionary of UnityMolAtoms involved in at least one Hbond and their associated Meshs.
    /// </summary>
    public Dictionary<UnityMolAtom, List<Mesh>> AtomToMeshes { get; private set; }

    /// <summary>
    /// Dictionary of UnityMolAtoms involved in at least one Hbond and their associated Vertices
    /// </summary>
    public Dictionary<UnityMolAtom, List<int>> AtomToVertices { get; private set; }

    /// <summary>
    /// Hydrogen bonds
    /// </summary>
    public UnityMolBonds Hbonds { get; set; }

    /// <summary>
    /// If the Hbonds are custom
    /// </summary>
    private bool isCustomHbonds { get; }

    /// <summary>
    /// GameObject parent holding all Hbonds GameObjects
    /// </summary>
    private readonly GameObject mainHbondsGO;

    /// <summary>
    /// Material for the Hbond
    /// </summary>
    private Material hbondMat;

    /// If customHbonds is true, use the bonds from the selection,
    /// else run hbond detection algorithm
    public BondRepresentationHbonds(int idF, string structName, UnityMolSelection sel, bool customHbonds = false) {

        colorationType = colorType.full;

        representationParent = UnityMolMain.getRepStructureParent(structName).transform;


        mainHbondsGO = new GameObject("BondHbondRepresentation");
        mainHbondsGO.transform.parent = representationParent;
        representationTransform = mainHbondsGO.transform;

        hbondMat = Resources.Load("Materials/hbondsTransparentUnlit") as Material;

        AnimateHbonds anim = mainHbondsGO.AddComponent<AnimateHbonds>();
        anim.hbondMat = hbondMat;

        selection = sel;
        idFrame = idF;
        isCustomHbonds = customHbonds;

        if (isCustomHbonds) {
            Hbonds = sel.bonds;
        }
        else {
            Hbonds = HbondDetection.DetectHydrogenBonds(sel, idFrame, selection.atomToIdInSel);
        }


        displayHBonds();

        mainHbondsGO.transform.localPosition = Vector3.zero;
        mainHbondsGO.transform.localRotation = Quaternion.identity;
        mainHbondsGO.transform.localScale = Vector3.one;

        //Don't do that to avoid updating the representation every time showSelection is called
        // nbBonds = hbonds.Count;
        nbBonds = sel.bonds.Count;
    }

    /// <summary>
    /// Create GameObjects and meshes for displaying the Hbonds
    /// </summary>
    private void displayHBonds() {

        int nbSticks = Hbonds.Length;


        MeshesGO = new List<GameObject>();
        AtomToMeshes = new Dictionary<UnityMolAtom, List<Mesh>>();
        AtomToVertices = new Dictionary<UnityMolAtom, List<int>>();
        AtomToGO = new Dictionary<UnityMolAtom, List<GameObject>>();

        if (nbSticks == 0) {
            return;
        }

        int countBond = 0;

        UnityMolModel curM = selection.atoms[0].residue.chain.model;

        HashSet<int2> doneBonds = new HashSet<int2>();

        foreach (int ida in Hbonds.bonds.Keys) {
            UnityMolAtom atom1 = curM.allAtoms[ida];
            foreach (int idb in Hbonds.bonds[ida]) {
                if (idb == -1) {
                    continue;
                }
                int2 k, invk;
                k.x = ida; invk.x = idb;
                k.y = idb; invk.y = ida;
                if (doneBonds.Contains(k) || doneBonds.Contains(invk)) {
                    continue;
                }

                doneBonds.Add(k);
                UnityMolAtom atom2 = curM.allAtoms[idb];

                GameObject currentGO = new GameObject("BondHBond_" + "BondHBond_" + countBond + "_" + atom1.number + "/" + atom2.number);
                currentGO.transform.parent = mainHbondsGO.transform;
                currentGO.transform.localRotation = Quaternion.identity;
                currentGO.transform.localPosition = Vector3.zero;
                currentGO.transform.localScale = Vector3.one;

                Mesh curMesh = createQuadMesh(atom1, atom2);

                if (!AtomToGO.ContainsKey(atom1)) {
                    AtomToGO[atom1] = new List<GameObject>();
                }

                if (!AtomToGO.ContainsKey(atom2)) {
                    AtomToGO[atom2] = new List<GameObject>();
                }

                AtomToGO[atom1].Add(currentGO);
                AtomToGO[atom2].Add(currentGO);



                MeshFilter mf = currentGO.AddComponent<MeshFilter>();
                mf.sharedMesh = curMesh;
                MeshRenderer mr = currentGO.AddComponent<MeshRenderer>();
                mr.sharedMaterial = hbondMat;


                MeshesGO.Add(currentGO);
                countBond++;
            }
        }
    }

    /// <summary>
    /// Create a quadrangle mesh for one Hbond between atom1 and atom2
    /// </summary>
    /// <param name="atom1">1st atom of the Hbond</param>
    /// <param name="atom2">2nd atom of the Hbond</param>
    /// <param name="lineWidth">Width of line of the quadrangle</param>
    /// <returns>the quadrangle mesh</returns>
    private Mesh createQuadMesh(UnityMolAtom atom1, UnityMolAtom atom2, float lineWidth = 0.5f) {

        Vector3[] newVertices = new Vector3[4];
        Vector2[] newUV = new Vector2[4];
        Color32[] newColors = new Color32[4];
        int[] newTriangles = new int[6];

        Vector3 start = atom1.position;
        Vector3 end   = atom2.position;
        if (idFrame != -1) {
            int iida = selection.atomToIdInSel[atom1];
            start = selection.extractTrajFramePositions[idFrame][iida];
            iida = selection.atomToIdInSel[atom2];
            end = selection.extractTrajFramePositions[idFrame][iida];
        }


        Vector3 normal = Vector3.Cross(start, end);
        Vector3 side = Vector3.Cross(normal, end - start);
        side.Normalize();

        Vector3 a = start + side * (lineWidth / 2);
        Vector3 b = start - side * (lineWidth / 2);
        Vector3 c = end + side * (lineWidth / 2);
        Vector3 d = end - side * (lineWidth / 2);


        //A quad per bond

        int ida = 0;
        newVertices[0] = a;
        newVertices[1] = b;
        newVertices[2] = c;
        newVertices[3] = d;

        newTriangles[0] = 0;
        newTriangles[1] = 1; //b
        newTriangles[2] = 2; //c

        newTriangles[3] = 2;
        newTriangles[4] = 1; //c
        newTriangles[5] = 3; //d

        newUV[0] = Vector2.zero;
        newUV[1] = new Vector2(0, 1);
        newUV[2] = new Vector2(1, 0);
        newUV[3] = Vector2.one;


        newColors[0] = Color.white;
        newColors[1] = Color.white;
        newColors[2] = Color.white;
        newColors[3] = Color.white;



        Mesh curMesh = new Mesh {
            vertices = newVertices,
            triangles = newTriangles,
            colors32 = newColors,
            uv = newUV
        };

        curMesh.RecalculateNormals();


        if (AtomToMeshes.ContainsKey(atom1)) {
            AtomToMeshes[atom1].Add(curMesh);
            AtomToVertices[atom1].Add(ida);
            AtomToVertices[atom1].Add(ida + 1);

        }
        else {
            AtomToMeshes[atom1] = new List<Mesh>();
            AtomToVertices[atom1] = new List<int>();
            AtomToMeshes[atom1].Add(curMesh);
            AtomToVertices[atom1].Add(ida);
            AtomToVertices[atom1].Add(ida + 1);

        }

        if (AtomToMeshes.ContainsKey(atom2)) {
            AtomToMeshes[atom2].Add(curMesh);
            AtomToVertices[atom2].Add(ida + 2);
            AtomToVertices[atom2].Add(ida + 3);

        }
        else {
            AtomToMeshes[atom2] = new List<Mesh>();
            AtomToVertices[atom2] = new List<int>();
            AtomToMeshes[atom2].Add(curMesh);
            AtomToVertices[atom2].Add(ida + 2);
            AtomToVertices[atom2].Add(ida + 3);

        }
        return curMesh;
    }

    /// <summary>
    /// Destroy all GameObjets of the hbonds and clear the various dictionaries.
    /// </summary>
    /// <param name="destroyHbondMaterial">destroy also the Material if true</param>
    private void clean(bool destroyHbondMaterial = true) {
        if (MeshesGO != null) {//Already destroyed by the manager
            foreach (GameObject go in MeshesGO) {
                GameObject.Destroy(go.GetComponent<MeshFilter>().sharedMesh);
                GameObject.Destroy(go);
            }
            MeshesGO.Clear();
        }

        if (destroyHbondMaterial) {
            hbondMat = null;
        }
        AtomToGO.Clear();
        AtomToMeshes.Clear();
        AtomToVertices.Clear();
    }

    /// <summary>
    /// Override method which just call the private clean method
    /// </summary>
    public override void Clean() {
        clean();
    }

    /// <summary>
    /// Recompute GameObjects linked to the Hbonds.
    /// If lightRecompute is false, also recompute the detection of the Hbonds if IsCustomHbonds is false.
    /// </summary>
    /// <param name="lightRecompute">If true, don't recompute the detection of the hbonds</param>
    public void Recompute(bool lightRecompute) {
        //Clean without destroying material
        clean(false);

        if (!lightRecompute) {
            if (isCustomHbonds) {
                Hbonds = selection.bonds;
            } else {
                Hbonds = HbondDetection.DetectHydrogenBonds(selection, idFrame, selection.atomToIdInSel);
            }
        }

        displayHBonds();
    }
}
}
