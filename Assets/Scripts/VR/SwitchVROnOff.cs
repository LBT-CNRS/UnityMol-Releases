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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class SwitchVROnOff : MonoBehaviour {

    public GameObject room;
    public GameObject floor;
    public GameObject VRMenu;
    public GameObject NotifMenu;

    public bool reactiveVR = false;
    public bool deactiveVR = false;

    string prevLoadedDevice = "";

    void Start() {
        if (room == null) {
            room = GameObject.Find("RoomVR");
        }
        if (floor == null) {
            floor = GameObject.Find("Floor");
        }
        if (VRMenu == null) {
            VRMenu = GameObject.Find("CanvasMainUIVR");
        }
        if (NotifMenu == null) {
            NotifMenu = GameObject.Find("CanvasNotif");
        }
    }

    //From https://stackoverflow.com/questions/36702228/enable-disable-vr-from-code
    IEnumerator LoadDevice(string newDevice, bool enable)
    {
        XRSettings.LoadDeviceByName(newDevice);
        yield return null;
        XRSettings.enabled = enable;
        if (newDevice == "") {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
        }
        else {
            Camera.main.clearFlags = CameraClearFlags.Skybox;
        }
    }

    public void DisableVR()
    {
        if (floor != null)
            floor.SetActive(false);
        if (room != null)
            room.SetActive(false);
        if (VRMenu != null)
            VRMenu.SetActive(false);
        if (NotifMenu != null)
            NotifMenu.SetActive(false);

        Transform clref = VRTK.VRTK_DeviceFinder.DeviceTransform(VRTK.VRTK_DeviceFinder.Devices.LeftController);
        Transform crref = VRTK.VRTK_DeviceFinder.DeviceTransform(VRTK.VRTK_DeviceFinder.Devices.RightController);

        if (clref != null) {
            clref.parent.gameObject.SetActive(false);
        }
        if (crref != null) {
            crref.parent.gameObject.SetActive(false);
        }
        prevLoadedDevice = XRSettings.loadedDeviceName;
        StartCoroutine(LoadDevice("", false));
    }
    public void ActivateVR() {
        if (XRSettings.supportedDevices.Length != 0) {
            string deviceToLoad = "";
            if (!string.IsNullOrEmpty(prevLoadedDevice)) {
                deviceToLoad = prevLoadedDevice;
            }
            else {
                deviceToLoad = XRSettings.supportedDevices[0];
                if (deviceToLoad == "None" && XRSettings.supportedDevices.Length > 1) {
                    deviceToLoad = XRSettings.supportedDevices[1];
                }
            }
#if UNITY_EDITOR
            Debug.Log("Loading VR device: " + deviceToLoad);
#endif
            if (floor != null)
                floor.SetActive(true);
            if (room != null)
                room.SetActive(true);
            if (VRMenu != null)
                VRMenu.SetActive(true);
            if (NotifMenu != null)
                NotifMenu.SetActive(true);

            Transform clref = VRTK.VRTK_DeviceFinder.DeviceTransform(VRTK.VRTK_DeviceFinder.Devices.LeftController);
            Transform crref = VRTK.VRTK_DeviceFinder.DeviceTransform(VRTK.VRTK_DeviceFinder.Devices.RightController);

            if (clref != null) {
                clref.parent.gameObject.SetActive(true);
            }
            if (crref != null) {
                crref.parent.gameObject.SetActive(true);
            }

            StartCoroutine(LoadDevice(deviceToLoad, true));
        }
#if UNITY_EDITOR
        else {
            Debug.LogError("No VR device to load");
        }
#endif
    }

    public void switchVR() {
        if (XRSettings.enabled) {
            DisableVR();
        }
        else {
            ActivateVR();
        }
    }

    void Update() {
        if (reactiveVR) {
            ActivateVR();
            reactiveVR = false;
        }
        if (deactiveVR) {
            DisableVR();
            deactiveVR = false;
        }
    }
}
