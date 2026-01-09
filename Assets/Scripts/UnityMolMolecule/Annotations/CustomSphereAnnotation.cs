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
public class CustomSphereAnnotation : UnityMolAnnotation {

    public float scale = 1.1f;

    public override void Create() {

        GameObject haloGo = GameObject.Instantiate((GameObject) Resources.Load("Prefabs/SphereOverAtom"));
        haloGo.transform.GetChild(0).gameObject.SetActive(false);//Disable text

        haloGo.layer = LayerMask.NameToLayer("Ignore Raycast");
        haloGo.SetActive(true);

        haloGo.transform.parent = annoParent;
        haloGo.transform.localPosition = Vector3.zero;
        haloGo.transform.rotation = Quaternion.identity;//Keep global rotation
        haloGo.transform.localScale =  Vector3.one * scale;

        go = haloGo;

    }
    public override void Update() {
    }
    public override void UnityUpdate() {
    }
    public override void Delete() {
        if (go != null && go.transform.parent != null) {
            GameObject.Destroy(go.transform.parent.gameObject);
        }
    }

    public override void Show(bool show = true) {
        if (go != null) {
            isShown = show;
            go.SetActive(show);
        }
    }

    public override SerializedAnnotation Serialize() {
        SerializedAnnotation san = new SerializedAnnotation();
        san.positions = new List<Vector3>(1);
        san.positions.Add(annoParent.position);
        san.size = scale;
        fillSerializedAtoms(san);
        return san;
    }
    public override int toAnnoType(){
        return 5;
    }
}
}