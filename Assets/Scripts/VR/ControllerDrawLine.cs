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
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRTK;
using UMol.API;


namespace UMol {

[RequireComponent(typeof(VRTK_ControllerEvents))]
public class ControllerDrawLine : MonoBehaviour {

    public GameObject penGo;

    VRTK_ControllerEvents controllerEvents;
    Transform loadedMols;
    GameObject curGo;
    UnityMolStructure curS;
    MeshLineRenderer lr;
    UnityMolSelectionManager selM;
    UnityMolStructureManager sm;

    void Start() {
        selM = UnityMolMain.getSelectionManager();
        sm = UnityMolMain.getStructureManager();

        if (controllerEvents == null) {
            controllerEvents = GetComponent<VRTK_ControllerEvents>();
        }

        loadedMols = UnityMolMain.getRepresentationParent().transform;

        controllerEvents.TriggerPressed += triggerClicked;
        controllerEvents.TriggerReleased += triggerReleased;
        controllerEvents.TriggerUnclicked += triggerReleased;
        controllerEvents.TriggerTouchEnd += triggerReleased;

    }

    private void triggerClicked(object sender, ControllerInteractionEventArgs e) {
        if (sm.loadedStructures.Count == 0) {
            return;
        }
        if(!UnityMolMain.getAnnotationManager().drawMode){
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
        curGo = new GameObject(s.uniqueName + "_DrawLine");
        curGo.transform.parent = sm.findStructureGO(s).transform;
        curGo.transform.localPosition = Vector3.zero;

        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.black;
        curGo.AddComponent<MeshRenderer>().material = mat;


        lr = curGo.AddComponent<MeshLineRenderer>();

        // curGo.AddComponent<MeshFilter>().mesh = new Mesh();
        // curGo.AddComponent<MeshRenderer>();
        // curGo.AddComponent<MeshCollider>();
        // UnityMolMain.getAnnotationManager().drawings.Add(curGo);


    }

    private void triggerReleased(object sender, ControllerInteractionEventArgs e) {
        if(UnityMolMain.getAnnotationManager().drawMode && curGo != null && lr != null){
            APIPython.annotateDrawLine(curS.uniqueName, lr.positions, Color.blue);
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
            if(penGo != null){
                penGo.SetActive(false);
            }
            return;
        }

        if (curGo != null && lr != null) {
            if(penGo != null){
                penGo.SetActive(true);
            }
            //Fill the line
            lr.AddPoint(penGo.transform.position);
        }
    }


}
}