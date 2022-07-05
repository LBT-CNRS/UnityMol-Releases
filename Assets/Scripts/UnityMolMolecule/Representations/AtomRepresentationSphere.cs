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
using UnityEngine.XR;
using VRTK;


namespace UMol {
public class AtomRepresentationSphere : AtomRepresentation {

    public List<GameObject> meshesGO;
    public MaterialPropertyBlock properties;
    public Dictionary<UnityMolAtom, int> atomToId;



    public AtomRepresentationSphere(string structName, UnityMolSelection sel) {
        colorationType = colorType.atom;
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
        GameObject newRep = new GameObject("AtomInstantiatedSphereRepresentation");
        newRep.transform.parent = representationParent;
        representationTransform = newRep.transform;


        selection = sel;
        atomToId = new Dictionary<UnityMolAtom, int>();
        DisplaySphere(newRep.transform);
        // newRep.transform.position -= offset;
        newRep.transform.localPosition = Vector3.zero;
        newRep.transform.localRotation = Quaternion.identity;
        newRep.transform.localScale = Vector3.one;

        nbAtoms = selection.Count;
    }

    private void DisplaySphere(Transform repParent){
        meshesGO = new List<GameObject>();
        properties = new MaterialPropertyBlock();
        atomToId = new Dictionary<UnityMolAtom, int>();
        GameObject prefab = Resources.Load("Prefabs/SpherePrefab") as GameObject ;


        for (int i = 0; i < selection.Count; i++) {
            Transform t = GameObject.Instantiate(prefab).transform;
            t.name = selection.atoms[i].residue.chain.name+"_"+selection.atoms[i].residue.name+selection.atoms[i].residue.id+"_"+selection.atoms[i].name+"_"+selection.atoms[i].number;
            t.position = selection.atoms[i].position;
            t.localScale = Vector3.one * selection.atoms[i].radius*2;
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
    public override void Clean(){}
}
}