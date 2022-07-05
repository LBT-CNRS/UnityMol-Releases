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
using System;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour {

    public bool automove = false;

    public int zoomRate = 200;
    public float panSpeed = 2.0f;

    public float xSpeed = 400.0f;
    public float ySpeed = 400.0f;

    public float zoomDampening = 20.0f;

    // Size values for the orthographic camera
    private float minOrthoSize = 1f;
    private float maxOrthoSize = 60f;
    public float orthoSize = 10f;
    // size of the orthographic camera

    private Camera mainCamera;
    private Transform target;
    private Vector3 newposition;
    private bool cameraTranslation = false, centerChanged = false;
    private Quaternion currentRotation;
    private Quaternion desiredRotation;
    private float currentDistance;
    private float desiredDistance;

    private float xDeg = 0.0f;
    private float yDeg = 0.0f;
    private float zDeg = 0.0f;
    private float weight_cam = 0.0f;

    void Start() {

        mainCamera = GetComponent<Camera>();

        // If there is no target, create a temporary target at 'distance' from the cameras current viewpoint
        if (!target) {
            GameObject go = new GameObject("Cam Target");
            //transform.position = go.transform.position + (transform.forward * distance);
            target = go.transform;
        }

        currentDistance = Vector3.Distance(transform.position, Vector3.zero);
        desiredDistance = currentDistance;
        currentRotation = transform.rotation;
        desiredRotation = transform.rotation;
        
        xDeg = Vector3.Angle(Vector3.right, transform.right);
        yDeg = Vector3.Angle(Vector3.up, transform.up);
    }


    ///
    /// Camera logic on LateUpdate to only update after all character movement logic has been handled. 
    ///
    void LateUpdate() {

        if (Input.GetButtonUp("Spin Molecule")) {
            automove = !automove;
        }

        // Camera sliding and re-center on a atom or a group of atoms
        if (Input.GetKeyUp(KeyCode.R)) {
            cameraTranslation = true;
            centerChanged = true;
            newposition = Vector3.zero;
        }

        keyboardOperate();

        // If Control and Alt and Middle button? ZOOM!
        if (Input.GetMouseButton(1)) {
            desiredDistance -= Input.GetAxis("Mouse Y") * Time.deltaTime * zoomRate;//* Mathf.Abs(desiredDistance);
        }
        // If middle mouse and left alt are selected? ORBIT
        else if (Input.GetMouseButton(0)) { //Click on UI parts (not canvas) 

            if (Input.mousePosition.x < Screen.width * 0.85f && Input.mousePosition.y < Screen.height * 0.85f 
                && Input.mousePosition.y > Screen.height * 0.15f)
            {
                xDeg = Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                yDeg = -Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
            } else if (Input.mousePosition.x > Screen.width * 0.85f) {
                yDeg = -Input.GetAxis("Mouse Y") * xSpeed * 0.02f;
                xDeg = 0;
            } else if (Input.mousePosition.y > Screen.height * 0.85f) {
                xDeg = Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                yDeg = 0;
            } else {
                zDeg = Input.GetAxis("Mouse X") * ySpeed * 0.02f;
                yDeg = 0;                   
            }
        }
        // otherwise if middle mouse is selected, we pan by way of transforming the target in screenspace
        else if (Input.GetMouseButton(2)) {

            Vector3 v = transform.localPosition;
            v.x -= Input.GetAxis("Mouse X") * panSpeed;
            v.y -= Input.GetAxis("Mouse Y") * panSpeed;
            transform.localPosition = v;
        }   

        if (mainCamera.orthographic) { // orthographic mode we can achieve the same effet by making the camera bigger/smaller

            float tmp_size = mainCamera.orthographicSize - Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate;
            if (tmp_size <= maxOrthoSize && tmp_size >= minOrthoSize)
                mainCamera.orthographicSize = tmp_size;
        } else
            desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate;


        if (automove) {
            xDeg += Mathf.Lerp(0.0F, 100.0F, Time.deltaTime * 0.8f);
            yDeg = 0;
        }

        if (centerChanged) {
            // if we reach the position (for camera sliding) we stop!

            // otherwise we continue the translation.
            if (cameraTranslation) {

                weight_cam += Time.deltaTime * 1;
                target.localPosition = Vector3.Lerp(target.localPosition, newposition, weight_cam);

                transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, weight_cam);

                // mainCamera.transform.eulerAngles = Vector3.Lerp (mainCamera.transform.eulerAngles, Vector3.zero, weight);

                if (target.position == newposition && transform.localPosition == Vector3.zero) {

                    weight_cam = 0;
                    cameraTranslation = false;
                    centerChanged = false;
                }
            }
        }
        
        if (xDeg != 0 || yDeg != 0 || zDeg != 0) {
            //Camera rotation
            desiredRotation *= Quaternion.Euler(yDeg, xDeg, zDeg);
            currentRotation = transform.rotation;
            transform.rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);

        }
        

        //Camera movement
        currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);
        Vector3 position = target.position - (transform.rotation * Vector3.forward * currentDistance);

        transform.position = position;


        xDeg = 0;
        yDeg = 0;
        zDeg = 0;
    }

    private static float ClampAngle(float angle, float min, float max) {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }

    public void ToCenter() {
        transform.localPosition = Vector3.zero;
        target.transform.localPosition = Vector3.zero;
        transform.position = new Vector3(0, 0, transform.position.z);
        transform.rotation = Quaternion.identity;
        centerChanged = false;
        weight_cam = 0;
    }

    // control of the joypad =============================================================================================
    //                       =============================================================================================
    // private void joypadOperate ()
    // {
    //  target.rotation = transform.rotation;

    //  Vector3 v = LocCamera.transform.localPosition;

    //  /*
    //   * Left pad
    //   */
    //  v.x -= (Input.GetAxis ("Horizontal") * panSpeed * Time.deltaTime);
    //  v.y -= (Input.GetAxis ("Vertical") * panSpeed * Time.deltaTime);
        
    //  LocCamera.transform.localPosition = v;

    //  /*
    //   * Right pad
    //   */
    //  if (Input.GetAxis ("Axis3") > joypadDeadzone || (Input.GetAxis ("Axis3") < -joypadDeadzone))
    //  {
    //      xDeg = Input.GetAxis ("Axis3") * xSpeed * 0.08f;
    //  }

    //  if (Input.GetAxis ("Axis4") > joypadDeadzone || (Input.GetAxis ("Axis4") < -joypadDeadzone))
    //  {
    //      yDeg = Input.GetAxis ("Axis4") * ySpeed * 0.08f;
    //  }
        
    //  //Z rotation
    //  if (Input.GetButton("Rotate Z Right"))
    //  {
    //      zDeg = Time.deltaTime * xSpeed * 0.5f;
    //  }

    //  if (Input.GetButton("Rotate Z Left"))
    //  {
    //      zDeg = - Time.deltaTime * xSpeed * 0.5f;
    //  }

    //  //Zoom in
    //  if (Input.GetButton("Zoom In")) {
    //      desiredDistance -= Time.deltaTime * zoomRate * 0.08f;
    //      // if (UIData.switchmode)
    //          // Molecule3DComp.ToParticle ();
    //  }
    //  //zoom out
    //  if (Input.GetButton("Zoom Out")) {
    //      desiredDistance += Time.deltaTime * zoomRate * 0.08f;
    //      // if (UIData.switchmode)
    //          // Molecule3DComp.ToParticle ();
    //  }
    // }
    
    /// <summary>
    /// Manage the camera rotation with keyboard inputs.
    /// </summary>
    private void keyboardOperate() {

        /*
         * X Rotation
         */


        if (Input.GetButton("Rotate X Right")) {
            upxDeg();
        }
        
        if (Input.GetButton("Rotate X Left")) {
            downxDeg();
        }

        /*
         * Y Rotation
         */

        if (Input.GetButton("Rotate Y Up")) {
            upyDeg();
        }

        if (Input.GetButton("Rotate Y Down")) {
            downyDeg();
        }

    }
    // End of KeyboardOperate

    public void upxDeg() {
        xDeg = 1;
    }

    public void upyDeg() {
        yDeg = -1;
    }

    public void downxDeg() {
        xDeg = -1;
    }

    public void downyDeg() {
        yDeg = 1;
    }

    public void downzDeg() {
        zDeg = 1;
    }

    public void upzDeg() {
        zDeg = -1;
    }

    
}

