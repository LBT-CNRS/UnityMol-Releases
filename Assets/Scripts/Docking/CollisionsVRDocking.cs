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
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;

[RequireComponent (typeof (Rigidbody))]
public class CollisionsVRDocking : MonoBehaviour {

    // public MoveChainsVRDocking refMoveCh;
    public ExplosionSpawner spawner;

    private int uilayer;
    private AudioClip audioClip;

    void OnEnable(){
        uilayer = LayerMask.NameToLayer("UI");
        audioClip = (AudioClip) Resources.Load("Sounds/bang1.wav");
    }

    void OnTriggerEnter(Collider col){
        if(col.gameObject.layer == uilayer)
            return;
        if(col.gameObject.name[0] != '<')
            return;
        // refMoveCh.isGameObjectColliding[gameObject] = true;


        //TODO: get the real position of the collision
        spawner.SpawnExplosion(col.transform, Vector3.zero);

        ViveInput.TriggerHapticPulse(HandRole.RightHand, 500);
        ViveInput.TriggerHapticPulse(HandRole.LeftHand, 500);

        if(audioClip == null){
            audioClip = (AudioClip) Resources.Load("Sounds/bang1.wav");
        }
        AudioSource.PlayClipAtPoint(audioClip, transform.position);
    }
    void OnCollisionEnter(Collision collision){
        // if(collision.collider.gameObject.layer == uilayer)
            // return;
        // if(collision.collider.gameObject.name[0] != '<')
            // return;

        spawner.SpawnExplosion(collision.collider.transform, collision.contacts[0].point);

        ViveInput.TriggerHapticPulse(HandRole.RightHand, 500);
        ViveInput.TriggerHapticPulse(HandRole.LeftHand, 500);

        if(audioClip == null){
            audioClip = Resources.Load("Sounds/bang1") as AudioClip;
        }

        AudioSource.PlayClipAtPoint(audioClip, transform.position, 0.1f);

    }
    // void OnTriggerStay(Collider col){
    //  refMoveCh.isGameObjectColliding[gameObject] = true;
    // }

    void OnDestroy(){
    }
}