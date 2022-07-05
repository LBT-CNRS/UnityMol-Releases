/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

        Joseph Laurenti, 2019-2020
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
using System.Text;
using VRTK;
using UMol.API;

namespace UMol
{

[RequireComponent(typeof(VRTK_ControllerEvents))]
[RequireComponent(typeof(VRTK_StraightPointerRendererNoRB))]

public class MenuCommands : MonoBehaviour
{
    VRTK_ControllerEvents controllerEvents;

    public bool Button2Click = false;
    public bool TriggerClick = false;
    public bool gripClick = false;
    public bool AddLight = false;

    public GameObject LightBulb;
    public GameObject Menu;
    public GameObject Hand;
    public Camera m_Camera;

    // Start is called before the first frame update
    void Start()
    {
        controllerEvents = GetComponent<VRTK_ControllerEvents>();

        controllerEvents.ButtonTwoPressed += buttonPressed;
        controllerEvents.ButtonTwoReleased += buttonReleased;
        controllerEvents.TriggerPressed += triggerClicked;
        controllerEvents.TriggerReleased += triggerReleased;
        controllerEvents.GripPressed += gripPressed;
        controllerEvents.GripReleased += gripReleased;
    }

    // Update is called once per frame
    void Update()
    {
        if (Button2Click && TriggerClick)
        {
            Menu.transform.position = Hand.transform.position;
            Menu.transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.forward, m_Camera.transform.rotation * Vector3.up);

            Button2Click = false;
            TriggerClick = false;
        }

        if (Button2Click && gripClick)
        {
            takeScreenshot();

            Button2Click = false;
            gripClick = false;
        }

        if (TriggerClick && gripClick)
        {
            AddLight = true;
            Vector3 handposition = Hand.transform.position;

            if (AddLight)
            {
                Instantiate(LightBulb, handposition, Quaternion.identity);
                gripClick = false;
                TriggerClick = false;
                AddLight = false;
            }
        }
    }

    void buttonPressed(object sender, ControllerInteractionEventArgs e)
    {
        Button2Click = true;
    }

    void buttonReleased(object sender, ControllerInteractionEventArgs e)
    {
        Button2Click = false;
    }

    void triggerClicked(object sender, ControllerInteractionEventArgs e)
    {
        TriggerClick = true;
    }

    void triggerReleased(object sender, ControllerInteractionEventArgs e)
    {
        TriggerClick = false;
    }

    void gripPressed(object sender, ControllerInteractionEventArgs e)
    {
        gripClick = true;
    }

    void gripReleased(object sender, ControllerInteractionEventArgs e)
    {
        gripClick = false;
    }

    public void keepMenuOnHand (bool followingHand)
    {

    }
    public void takeScreenshot() {
        string directorypath = APIPython.path;
        if (!System.IO.Directory.Exists(directorypath))
        {
            System.IO.Directory.CreateDirectory(directorypath);
        }
        int idI = 0;
        string filePath = directorypath + "UmolImg_" + idI.ToString() + ".png";
        while (System.IO.File.Exists(filePath))
        {
            idI++;
            filePath = directorypath + "UmolImg_" + idI.ToString() + ".png";
        }
        APIPython.screenshot(filePath, 1920, 1080);
    }

}
}
