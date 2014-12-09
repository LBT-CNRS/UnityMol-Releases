/// @file MouseCameraControl.cs
/// @brief Details to be specified
/// @author FvNano/LBT team
/// @author Marc Baaden <baaden@smplinux.de>
/// @date   2013-4
///
/// Copyright Centre National de la Recherche Scientifique (CNRS)
///
/// contributors :
/// FvNano/LBT team, 2010-13
/// Marc Baaden, 2010-13
///
/// baaden@smplinux.de
/// http://www.baaden.ibpc.fr
///
/// This software is a computer program based on the Unity3D game engine.
/// It is part of UnityMol, a general framework whose purpose is to provide
/// a prototype for developing molecular graphics and scientific
/// visualisation applications. More details about UnityMol are provided at
/// the following URL: "http://unitymol.sourceforge.net". Parts of this
/// source code are heavily inspired from the advice provided on the Unity3D
/// forums and the Internet.
///
/// This software is governed by the CeCILL-C license under French law and
/// abiding by the rules of distribution of free software. You can use,
/// modify and/or redistribute the software under the terms of the CeCILL-C
/// license as circulated by CEA, CNRS and INRIA at the following URL:
/// "http://www.cecill.info".
/// 
/// As a counterpart to the access to the source code and rights to copy, 
/// modify and redistribute granted by the license, users are provided only 
/// with a limited warranty and the software's author, the holder of the 
/// economic rights, and the successive licensors have only limited 
/// liability.
///
/// In this respect, the user's attention is drawn to the risks associated 
/// with loading, using, modifying and/or developing or reproducing the 
/// software by the user in light of its specific status of free software, 
/// that may mean that it is complicated to manipulate, and that also 
/// therefore means that it is reserved for developers and experienced 
/// professionals having in-depth computer knowledge. Users are therefore 
/// encouraged to load and test the software's suitability as regards their 
/// requirements in conditions enabling the security of their systems and/or 
/// data to be ensured and, more generally, to use and operate it in the 
/// same conditions as regards security.
///
/// The fact that you are presently reading this means that you have had 
/// knowledge of the CeCILL-C license and that you accept its terms.
///
/// $Id: MouseCameraControl.cs 385 2014-03-31 14:37:56Z tubiana $
///
/// References : 
/// If you use this code, please cite the following reference : 	
/// Z. Lv, A. Tek, F. Da Silva, C. Empereur-mot, M. Chavent and M. Baaden:
/// "Game on, Science - how video game technology may help biologists tackle
/// visualization challenges" (2013), PLoS ONE 8(3):e57990.
/// doi:10.1371/journal.pone.0057990
///
/// If you use the HyperBalls visualization metaphor, please also cite the
/// following reference : M. Chavent, A. Vanel, A. Tek, B. Levy, S. Robert,
/// B. Raffin and M. Baaden: "GPU-accelerated atom and dynamic bond visualization
/// using HyperBalls, a unified algorithm for balls, sticks and hyperboloids",
/// J. Comput. Chem., 2011, 32, 2924
///

using UnityEngine;

[AddComponentMenu("Camera-Control/Mouse")]
public class MouseCameraControl : MonoBehaviour
{
    // Mouse buttons in the same order as Unity

    public enum MouseButton { Left = 0, Right = 1, Middle = 2, None = 3 }

    [System.Serializable]
    // Handles left modifiers keys (Alt, Ctrl, Shift)
    public class Modifiers
    {
        public bool leftAlt;
        public bool leftControl;
        public bool leftShift;
        
        public Modifiers()
        {
        	this.leftControl=true;	
        }

        public bool checkModifiers()
        {
            return (!leftAlt ^ Input.GetKey(KeyCode.LeftAlt)) &&
                (!leftControl ^ Input.GetKey(KeyCode.LeftControl)) &&
                (!leftShift ^ Input.GetKey(KeyCode.LeftShift));
        }
    }

    [System.Serializable]
    // Handles common parameters for translations and rotations
    public class MouseControlConfiguration
    {

        public bool activate;
        public MouseButton mouseButton;
        public Modifiers modifiers;
        public float sensitivity;
        
        public MouseControlConfiguration()
        {
        	this.activate=true;
        }
        
        public MouseControlConfiguration(MouseButton mouseButton,Modifiers modifiers,float sensitivity)
        {
        	this.activate=true;
        	this.mouseButton=mouseButton;
        	this.modifiers=modifiers;
        	this.sensitivity=sensitivity;
        }

        public bool isActivated()
        {
            return activate && Input.GetMouseButton((int)mouseButton) && modifiers.checkModifiers();
        }
    }

    [System.Serializable]
    // Handles scroll parameters
    public class MouseScrollConfiguration
    {

        public bool activate;
        public Modifiers modifiers;
        public float sensitivity;
        
        public MouseScrollConfiguration(float sensitivity)
        {
        	this.activate=true;
        	this.sensitivity=sensitivity;
        }

        public bool isActivated()
        {
            return activate && modifiers.checkModifiers();
        }
    }
    // Yaw default configuration
    public MouseControlConfiguration yaw = new MouseControlConfiguration (  MouseButton.Right,null, 10F );
    

    // Pitch default configuration
    public MouseControlConfiguration pitch = new MouseControlConfiguration (  MouseButton.Right,  new Modifiers(), 10F );

    // Roll default configuration
    public MouseControlConfiguration roll = new MouseControlConfiguration();

    // Vertical translation default configuration
    public MouseControlConfiguration verticalTranslation = new MouseControlConfiguration ( MouseButton.Middle, null, 2F );

    // Horizontal translation default configuration
    public MouseControlConfiguration horizontalTranslation = new MouseControlConfiguration ( MouseButton.Middle, null, 2F );

    // Depth (forward/backward) translation default configuration
    public MouseControlConfiguration depthTranslation = new MouseControlConfiguration (  MouseButton.Left, null, 2F );

    // Scroll default configuration
    public MouseScrollConfiguration scroll = new MouseScrollConfiguration ( 2F );
    
///////////////////////////////////////////////////////////////////////////////////////////////////////    
//
//    // Yaw default configuration
//    public MouseControlConfiguration yaw = new MouseControlConfiguration { mouseButton = MouseButton.Right, sensitivity = 10F };
//    
//
//    // Pitch default configuration
//    public MouseControlConfiguration pitch = new MouseControlConfiguration ( mouseButton = MouseButton.Right, modifiers = new Modifiers( leftControl = true ), sensitivity = 10F );
//
//    // Roll default configuration
//    public MouseControlConfiguration roll = new MouseControlConfiguration();
//
//    // Vertical translation default configuration
//    public MouseControlConfiguration verticalTranslation = new MouseControlConfiguration ( mouseButton = MouseButton.Middle, sensitivity = 2F );
//
//    // Horizontal translation default configuration
//    public MouseControlConfiguration horizontalTranslation = new MouseControlConfiguration ( mouseButton = MouseButton.Middle, sensitivity = 2F );
//
//    // Depth (forward/backward) translation default configuration
//    public MouseControlConfiguration depthTranslation = new MouseControlConfiguration ( mouseButton = MouseButton.Left, sensitivity = 2F );
//
//    // Scroll default configuration
//    public MouseScrollConfiguration scroll = new MouseScrollConfiguration ( sensitivity = 2F );

    // Default unity names for mouse axes
    public string mouseHorizontalAxisName = "Mouse X";
    public string mouseVerticalAxisName = "Mouse Y";
    public string scrollAxisName = "Mouse ScrollWheel";

    void LateUpdate ()
    {
        if (yaw.isActivated())
        {
            float rotationX = Input.GetAxis(mouseHorizontalAxisName) * yaw.sensitivity;
            transform.Rotate(0, rotationX, 0);
        }
        if (pitch.isActivated())
        {
            float rotationY = Input.GetAxis(mouseVerticalAxisName) * pitch.sensitivity;
            transform.Rotate(-rotationY, 0, 0);
        }
        if (roll.isActivated())
        {
            float rotationZ = Input.GetAxis(mouseHorizontalAxisName) * roll.sensitivity;
            transform.Rotate(0, 0, rotationZ);
        }

        if (verticalTranslation.isActivated())
        {
            float translateY = Input.GetAxis(mouseVerticalAxisName) * verticalTranslation.sensitivity;
            transform.Translate(0, translateY, 0);
        }

        if (horizontalTranslation.isActivated())
        {
            float translateX = Input.GetAxis(mouseHorizontalAxisName) * horizontalTranslation.sensitivity;
            transform.Translate(translateX, 0, 0);
        }

        if (depthTranslation.isActivated())
        {
            float translateZ = Input.GetAxis(mouseVerticalAxisName) * depthTranslation.sensitivity;
            transform.Translate(0, 0, translateZ);
        }

        if (scroll.isActivated())
        {
            float translateZ = Input.GetAxis(scrollAxisName) * scroll.sensitivity;

            transform.Translate(0, 0, translateZ);
        }
    }

}