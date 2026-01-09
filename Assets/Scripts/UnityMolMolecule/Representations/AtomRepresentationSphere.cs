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

namespace UMol {
public class AtomRepresentationSphere : AtomRepresentation {

    public List<GameObject> meshesGO;
    public MaterialPropertyBlock properties;
    public Dictionary<UnityMolAtom, int> atomToId;

    public Material solidMat;
    public Material transMat;

    public AtomRepresentationSphere(int idF, string structName, UnityMolSelection sel) {
        colorationType = colorType.atom;
        
        representationParent = UnityMolMain.getRepStructureParent(structName).transform;

        GameObject newRep = new GameObject("AtomInstantiatedSphereRepresentation");
        newRep.transform.parent = representationParent;
        representationTransform = newRep.transform;


        selection = sel;
        idFrame = idF;

        atomToId = new Dictionary<UnityMolAtom, int>();
        DisplaySphere(newRep.transform);
        // newRep.transform.position -= offset;
        newRep.transform.localPosition = Vector3.zero;
        newRep.transform.localRotation = Quaternion.identity;
        newRep.transform.localScale = Vector3.one;

        nbAtoms = selection.atoms.Count;
    }

    private void DisplaySphere(Transform repParent) {
        meshesGO = new List<GameObject>();
        properties = new MaterialPropertyBlock();
        atomToId = new Dictionary<UnityMolAtom, int>();
        GameObject prefab = Resources.Load("Prefabs/SpherePrefab") as GameObject ;
        solidMat = prefab.GetComponent<MeshRenderer>().sharedMaterial;

        for (int i = 0; i < selection.atoms.Count; i++) {
            Transform t = GameObject.Instantiate(prefab).transform;
            t.name = selection.atoms[i].residue.chain.name + "_" + selection.atoms[i].residue.name + selection.atoms[i].residue.id + "_" + selection.atoms[i].name + "_" + selection.atoms[i].number;
            if (idFrame != -1) {
                t.position = selection.extractTrajFramePositions[idFrame][i];
            }
            else {
                t.position = selection.atoms[i].position;
            }
            t.localScale = Vector3.one * selection.atoms[i].radius * 2;
            t.SetParent(repParent);

            properties.SetColor("_Color", selection.atoms[i].color);

            MeshRenderer r = t.GetComponent<MeshRenderer>();
            if (r) {
                r.SetPropertyBlock(properties);
            }
            atomToId[selection.atoms[i]] = i;
            meshesGO.Add(t.gameObject);
        }
    }
    public override void Clean() {}
}
}