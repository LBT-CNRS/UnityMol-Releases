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
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;
using System.Text;

using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;

using UMol.API;

namespace UMol {
[RequireComponent(typeof(ViveRoleSetter))]
public class ControllerDrawLine : MonoBehaviour {

    public GameObject penGo;
    public GameObject penTip;

    Transform loadedMols;
    GameObject curGo;
    UnityMolStructure curS;
    MeshLineRenderer lr;
    UnityMolSelectionManager selM;
    UnityMolStructureManager sm;
    List<Material> materials = new List<Material>();

    ViveRoleProperty curRole;

    void Start() {
        selM = UnityMolMain.getSelectionManager();
        sm = UnityMolMain.getStructureManager();

        loadedMols = UnityMolMain.getRepresentationParent().transform;
    }

    void OnEnable() {
        curRole = GetComponent<ViveRoleSetter>().viveRole;
        if (curRole != null) {
            ViveInput.AddPressDown((HandRole)curRole.roleValue, ControllerButton.Trigger, triggerClicked);
            ViveInput.AddPressUp((HandRole)curRole.roleValue, ControllerButton.Trigger, triggerReleased);
        }
    }

    void OnDisable() {
        if (curRole != null) {
            ViveInput.RemovePressDown((HandRole)curRole.roleValue, ControllerButton.Trigger, triggerClicked);
            ViveInput.RemovePressUp((HandRole)curRole.roleValue, ControllerButton.Trigger, triggerReleased);
        }
    }


    private void triggerClicked() {
        if (sm.loadedStructures.Count == 0) {
            return;
        }
        if (!UnityMolMain.getAnnotationManager().drawMode) {
            return;
        }

        UnityMolSelection sel = selM.currentSelection;
        UnityMolStructure s = null;
        try {
            s = sel.structures[0];
        }
        catch {
        }

        if (s == null) {
            s = APIPython.last();
        }

        curS = s;
        curGo = new GameObject(s.name + "_DrawLine");
        curGo.transform.parent = sm.GetStructureGameObject(s.name).transform;
        curGo.transform.localPosition = Vector3.zero;

        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.black;
        curGo.AddComponent<MeshRenderer>().sharedMaterial = mat;
        materials.Add(mat);


        lr = curGo.AddComponent<MeshLineRenderer>();

    }

    private void triggerReleased() {
        if (UnityMolMain.getAnnotationManager().drawMode && curGo != null && lr != null) {
            //Redraw the same line, and destroy the current one, this is useful for multi-user sessions !
            APIPython.annotateDrawLine(curS.name, lr.positions, Color.blue);
            GameObject.Destroy(curGo);
        }

        curGo = null;
        curS = null;
        lr = null;
    }

    void Update() {

        if (!UnityMolMain.getAnnotationManager().drawMode) {
            curGo = null;
            curS = null;
            lr = null;
            if (penGo != null) {
                penGo.SetActive(false);
            }
            return;
        }

        if (curGo != null && lr != null) {

            if (penGo != null)
                penGo.SetActive(true);

            if (penTip != null) {
                //Fill the line
                lr.AddPoint(penTip.transform.position);
            }
            else {
                if (penGo != null) {
                    //Fill the line
                    lr.AddPoint(penGo.transform.position);
                }
                else {
                    lr.AddPoint(transform.position);
                }
            }
        }
    }
    void OnDestroy() {
        if (curGo != null)
            GameObject.Destroy(curGo.GetComponent<MeshRenderer>().sharedMaterial);
        foreach (Material m in materials)
            Destroy(m);
    }


}
}