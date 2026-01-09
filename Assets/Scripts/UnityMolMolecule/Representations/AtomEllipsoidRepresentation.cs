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
using UnityEngine.Rendering;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using Unity.Mathematics;


namespace UMol {
public class AtomEllipsoidRepresentation : AtomRepresentation {


    public List<GameObject> meshesGO;
    public List<int3> ellipsoidTriplet;
    public Dictionary<int, KeyValuePair<int, int> > coordAtomTexture;
    public Texture2D[] paramTextures;
    public Dictionary<UnityMolAtom, int> atomToId;
    public List<Color32> atomColors;
    public bool withShadow = true;
    private List<Material> materials;


    public AtomEllipsoidRepresentation(int idF, string structName, UnityMolSelection sel) {
        colorationType = colorType.atom;

        representationParent = UnityMolMain.getRepStructureParent(structName).transform;
        
        GameObject newRep = new GameObject("AtomEllipsoidRepresentation");
        newRep.transform.parent = representationParent;
        representationTransform = newRep.transform;

        newRep.transform.localPosition = Vector3.zero;
        newRep.transform.localRotation = Quaternion.identity;
        newRep.transform.localScale = Vector3.one;

        ellipsoidTriplet = getEllipsoidAtoms(sel);

        selection = sel;
        idFrame = idF;

        materials = new List<Material>();
        atomToId = new Dictionary<UnityMolAtom, int>();

        DisplayEllipsoids(newRep.transform, ellipsoidTriplet);

        nbAtoms = selection.atoms.Count;
    }

    private List<int3> getEllipsoidAtoms(UnityMolSelection sel) {

        // For bases G and A, ellipsoid is computed from the triplet CY-G1-G2 or CY-A1-A2
        // For U and C, we use CA-CY-C1 or CA-CY-U1

        List<int3> res = new List<int3>(sel.atoms.Count / 3);
        HashSet<UnityMolResidue> doneRes = new HashSet<UnityMolResidue>();

        foreach (UnityMolAtom a in sel.atoms) {
            if (doneRes.Contains(a.residue))
                continue;

            if ((a.residue.name == "G" || a.residue.name == "A") && a.residue.atoms.ContainsKey("CY") && a.residue.atoms.ContainsKey("CA")) {
                if (a.residue.atoms.ContainsKey("G1") && a.residue.atoms.ContainsKey("G2")) {
                    int3 t;
                    t.x = sel.atomToIdInSel[a.residue.atoms["CY"]];
                    t.y = sel.atomToIdInSel[a.residue.atoms["G1"]];
                    t.z = sel.atomToIdInSel[a.residue.atoms["G2"]];
                    // AtomTrio t = new AtomTrio(a.residue.atoms["CY"], a.residue.atoms["G1"], a.residue.atoms["G2"]);
                    doneRes.Add(a.residue);
                    res.Add(t);
                }
                else if (a.residue.atoms.ContainsKey("A1") && a.residue.atoms.ContainsKey("A2")) {
                    int3 t;
                    t.x = sel.atomToIdInSel[a.residue.atoms["CY"]];
                    t.y = sel.atomToIdInSel[a.residue.atoms["A1"]];
                    t.z = sel.atomToIdInSel[a.residue.atoms["A2"]];
                    // AtomTrio t = new AtomTrio(a.residue.atoms["CY"], a.residue.atoms["A1"], a.residue.atoms["A2"]);
                    doneRes.Add(a.residue);
                    res.Add(t);
                }
            }
            else if ((a.residue.name == "C" || a.residue.name == "U") && a.residue.atoms.ContainsKey("CY") && a.residue.atoms.ContainsKey("CA")) {
                if (a.residue.atoms.ContainsKey("C1")) {
                    int3 t;
                    t.x = sel.atomToIdInSel[a.residue.atoms["CY"]];
                    t.y = sel.atomToIdInSel[a.residue.atoms["CA"]];
                    t.z = sel.atomToIdInSel[a.residue.atoms["C1"]];
                    // AtomTrio t = new AtomTrio(a.residue.atoms["CY"], a.residue.atoms["CA"], a.residue.atoms["C1"]);
                    doneRes.Add(a.residue);
                    res.Add(t);
                }
                else if (a.residue.atoms.ContainsKey("U1")) {
                    int3 t;
                    t.x = sel.atomToIdInSel[a.residue.atoms["CY"]];
                    t.y = sel.atomToIdInSel[a.residue.atoms["CA"]];
                    t.z = sel.atomToIdInSel[a.residue.atoms["U1"]];
                    // AtomTrio t = new AtomTrio(a.residue.atoms["CY"], a.residue.atoms["CA"], a.residue.atoms["U1"]);
                    doneRes.Add(a.residue);
                    res.Add(t);
                }
            }
        }

        return res;
    }

    private void DisplayEllipsoids(Transform repParent, List<int3> ellipsoidTriplet) {

        meshesGO = new List<GameObject>();

        float brightness = 1.0f;

        Transform sParent = UnityMolMain.getStructureManager().GetStructureGameObject(
                                selection.structures[0].name).transform;

        for (int i = 0; i < ellipsoidTriplet.Count; i++) {


            GameObject currentGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            currentGO.transform.name = "AtomEllipsoid";
            currentGO.transform.parent = repParent;
            GameObject.Destroy(currentGO.GetComponent<Collider>());

            Vector3 posAtom = barycenter(selection, ellipsoidTriplet[i], idFrame);

            Vector3 target = selection.atoms[ellipsoidTriplet[i].z].residue.atoms["CA"].curWorldPosition;
            if (idFrame != -1) {
                int idAtom = selection.atomToIdInSel[selection.atoms[ellipsoidTriplet[i].z].residue.atoms["CA"]];
                target = sParent.TransformPoint(selection.extractTrajFramePositions[idFrame][idAtom]);
            }

            Vector3 wnormal = wellipsoidNormal(selection, ellipsoidTriplet[i], idFrame, sParent);

            currentGO.transform.localScale = new Vector3(3.0f, 1.5f, 4.0f) * 1.5f;
            currentGO.transform.localPosition = posAtom;
            currentGO.transform.LookAt(target, wnormal);

            AssignMaterial(currentGO, brightness, selection.atoms[ellipsoidTriplet[i].z].color);

            atomToId[selection.atoms[ellipsoidTriplet[i].x]] = i;
            atomToId[selection.atoms[ellipsoidTriplet[i].y]] = i;
            atomToId[selection.atoms[ellipsoidTriplet[i].z]] = i;

            meshesGO.Add(currentGO);

        }
    }


    void AssignMaterial(GameObject curGO, float brightness, Color col) {


        Material EllipMat = new Material(Shader.Find("UMol/HyperBalls GL_D3D"));
        materials.Add(EllipMat);

        //Set fixed parameters for the shader
        EllipMat.SetTexture("_MatCap", (Texture) Resources.Load("Images/MatCap/daphz05"));
        EllipMat.SetFloat("_Brightness", brightness);
        EllipMat.SetVector("_Color", col);
        curGO.GetComponent<Renderer>().sharedMaterial = EllipMat;

        if (withShadow) {
            curGO.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.On;
            curGO.GetComponent<Renderer>().receiveShadows = true;
        }
        else {
            curGO.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            curGO.GetComponent<Renderer>().receiveShadows = false;
        }
    }

    public static Vector3 barycenter(UnityMolSelection sel, int3 t, int idF) {
        if (idF == -1) {
            Vector3 res = sel.atoms[t.x].position + sel.atoms[t.y].position + sel.atoms[t.z].position;
            return res / 3.0f;
        }
        else {
            Vector3 res = sel.extractTrajFramePositions[idF][t.x] +
                          sel.extractTrajFramePositions[idF][t.y] +
                          sel.extractTrajFramePositions[idF][t.z];
            return res / 3.0f;
        }
    }


    public static Vector3 wellipsoidNormal(UnityMolSelection sel, int3 t, int idF, Transform sParent) {
        if (idF == -1) {
            return Vector3.Cross(sel.atoms[t.y].curWorldPosition - sel.atoms[t.x].curWorldPosition,
                                 sel.atoms[t.z].curWorldPosition - sel.atoms[t.x].curWorldPosition).normalized;
        }
        else {

            Vector3 wPosA1 = sParent.TransformPoint(sel.extractTrajFramePositions[idF][t.x]);
            Vector3 wPosA2 = sParent.TransformPoint(sel.extractTrajFramePositions[idF][t.y]);
            Vector3 wPosA3 = sParent.TransformPoint(sel.extractTrajFramePositions[idF][t.z]);

            return Vector3.Cross(wPosA2 - wPosA1, wPosA3 - wPosA1).normalized;
        }
    }
    public override void Clean() {
        if (materials != null) {
            for (int i = 0; i < materials.Count; i++) {
                GameObject.Destroy(materials[i]);
            }
            materials.Clear();
            materials = null;
        }

        if (meshesGO != null) {
            for (int i = 0; i < meshesGO.Count; i++) {
                GameObject.Destroy(meshesGO[i].GetComponent<MeshFilter>().sharedMesh);
                GameObject.Destroy(meshesGO[i]);
            }
            meshesGO.Clear();
        }
        meshesGO = null;

        if (ellipsoidTriplet != null)
            ellipsoidTriplet.Clear();
        ellipsoidTriplet = null;

        if (coordAtomTexture != null)
            coordAtomTexture.Clear();
        coordAtomTexture = null;

        if (paramTextures != null) {
            for (int i = 0; i < paramTextures.Length; i++) {
                GameObject.Destroy(paramTextures[i]);
            }
        }
        paramTextures = null;

        if (atomToId != null)
            atomToId.Clear();
        atomToId = null;

        if (atomColors != null)
            atomColors.Clear();
        atomColors = null;


    }
}
}