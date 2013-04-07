/// @file Script3DCameras.js
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
/// $Id: Script3DCameras.js 213 2013-04-06 21:13:42Z baaden $
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

// Unity 120Hz DLP Link Stereoscopic 3D for DLP Projectors 
// Version: 0.1 (April 8, 2011)
// 
// Credits:
// Project started (V0.1) 03/2011 by Mark Hessburg (VIC20 @ Unity Forums)
//
//
// /////////////////////////////////////////////////////////////////////////////
// License: 
// You are allowed to use this script in any (commercial) Unity3D Game/Project. 
// The script is free, you are NOT allowed to sell this script except as part of a Game. 
// (E.g. you are not allowed to sell it on the Unity Asset Store or in similar ways like as a package for Unity Developers)
// You are not allowed to change the status of the "free usage" license of this script in future versions
// Upload a newer version to this thread if you improved the script but keep this Text/License and edit the Description, Credits and "To do" as necessary.
// http://forum.unity3d.com/threads/83595-Stereoscopic-3D-(DLP-Link-120Hz-active-Shutter-Glasses)
// /////////////////////////////////////////////////////////////////////////////
//
// Description:
// Works with any "3D Ready" DLP-Projector and DLP Link Glasses in the usual 3D settings. (3D turned on, Monitor set to 120Hz)
// The DLP Link Glasses get their synch from an invisible 120Hz flash of the Projector.
// This script changes the Camera pictures at a frequency of 120Hz (resulting in a 60Hz stereoscopic picture) 
// (As usual with DLP Link technology press the key to toggle L&R Pictures to display them on the correct eye if the stereoscopic 3D looks wrong.)
//
// You can change the stereo separation and convergence of the camera setup. Use convergence for fine adjustment of the stereoscopic picture when you changed the separation, 
// and/or use convergence to change the 3D depth of Objects (you can move closer objects further into the room this way, like if they are in front of the screen, 
// or you can change the 3D effect to let the screen look like watching through a window into another room - looking at the mouse pointer while changing convergence helps)
//
// IMPORTANT:
// set FixedTimeStep to 0.008333334 !!! (At the moment the 120Hz synch works with FixedUpdate, this will affect physics too)
// Set player settings to synch to VBL at 120Hz
// 
// /////////////////////////////////////////////////////////////////////////////
// TO DO:
// V0.1:
// Currently it works with the free and pro version of Unity as long as the Game runs above 119.9999 frames per second.
// I've made some experiments with render to texture and Unity Pro, this still works at 120fps, I saw no loss of performance. I have not added this part of the script to avoid confusion. (I've changed the script to work with the render texture independet from the aspect ratio, ask if you need it for further testing)
// It is possible to use RenderTextures as a buffer for the Left & Right Pictures. So (theoretically) you could use this buffer to display and switch L&R pictures at 120Hz while waiting for the next frame - the problem is you can't force a Camera to render at a certain frequency because it depends on the time the updates/rendering of all Cameras needs to finish.
// Someone needs to find a solution to display these RenderTextures with an OpenGL PlugIn at a fixed refresh rate of 120Hz even when the game runs below 120fps. I just don't know if it is possible to render to the display with OpenGL while Unity is running (no Unity Camera needs to render to the screen anyway when using render to texture)
//
// So basically the best would be if a future version of unity could provide a special camera for textures only that always runs at a fixed speed and also a secondary (independend to the physics timestep) Fixed Update.
//
//
//
//
//var HUD : GUIText;
//var HUDobject: GameObject;
//var FPScounter : GameObject;
//var KeyInputDelayTimer : float; // Keyboard input delay... quick&dirty
//
//
//var TargetFPS : int=120; // Used if you switch between 60Hz & 120Hz
//
//var cam1 : Camera; // Stereo Camera 1
//var cam2 : Camera; // Stereo Camera 2
//var camMONO : Camera;  // the 2D Mode Camera
//
//var cam1object : GameObject;
//var cam2object : GameObject;
//var camMONOobject : GameObject; 
//var cam1Transform : Transform;
//var cam2Transform : Transform;
//
//var CamLorR : int; // decides about which of the Cameras will be active 
//var ToggleLR : int; // To switch sides of L&R pictures by changing CamLorR
//
//var Separation : float; // Distances between the cameras
//var Convergence : float = 0.0; // Use convergence to move close objects in/out of the screen
//
//var originalProjection1 : Matrix4x4; // needed for convergence
//var p1 : Matrix4x4 ;
//var originalProjection2 : Matrix4x4;
//var p2 : Matrix4x4 ;
//
//var FOV : float = 60.0;
//
//var CamDistance : float = 0.0;  // Not important for Stereo 3D
//var is3dOn : int = 0; // turns stereoscopi mode on/off (switches between cam1/cam2 or CamMONO)
//
//
//
//
// Setup to some nice Stereoscopic settings
//
//function Start()
//{
//	Application.targetFrameRate = 120;
//	Time.captureFramerate = 120; // actually I have no idea if this one is necessary ;-)
//
//	cam1object.active =true;
//	cam2object.active =true;
//	camMONOobject.active =false;
//	HUDobject.active=true;
//	FPScounter.active=true;
//	is3dOn=0;
//	
//	FOV=50.0;
//	cam1.ResetProjectionMatrix();
//	cam2.ResetProjectionMatrix();
//	cam1.fieldOfView = FOV;
//	cam2.fieldOfView = FOV;
//	
//	camMONO.fieldOfView = FOV;
//	originalProjection1 = cam1.projectionMatrix;
//	originalProjection2 = cam2.projectionMatrix;
//	
//	Separation = 0.02169;
//	cam2Transform.localPosition.x=Separation;
//	cam1Transform.localPosition.x=0.0-Separation;
//
//	Convergence=0.011; 	
//	p1  = originalProjection1;
//    p1.m02 = Convergence;
//    cam1.projectionMatrix = p1;
//
//    p2  = originalProjection2;
//    p2.m02 = Convergence*-1;
//    cam2.projectionMatrix = p2;
//
//}
//
//
//
//
//
//function Update() 
//{
//
// Toggle HUD
//	
//if (Input.GetKey (KeyCode.H) && KeyInputDelayTimer+0.1<Time.time)
//{
//		KeyInputDelayTimer=Time.time;	
//		if(HUDobject.active==true)
//		{
//			HUDobject.active=false;
//			FPScounter.active=false;
//		}		
//		else
//		{
//			HUDobject.active=true;
//			FPScounter.active=true;
//		}	
//}
//
//
// Turn Stereoscopic 3D On/Off 
//	
//if (Input.GetKey (KeyCode.Backspace) && KeyInputDelayTimer+0.1 <Time.time && is3dOn ==0)
//{
//	KeyInputDelayTimer=Time.time;	
//	is3dOn =1;
//	cam1object.active =false;
//	cam2object.active =false;
//	camMONOobject.active =true;
//}
//
//if (Input.GetKey (KeyCode.Backspace) && KeyInputDelayTimer+0.1<Time.time && is3dOn ==1)
//{
//	KeyInputDelayTimer=Time.time;	
//	is3dOn =0;
//	cam1object.active =true;
//	cam2object.active =true;
//	camMONOobject.active =false;	
//}
//
//
// Switch between 120Hz DLP and 60Hz CRT Glasses (no idea if 60Hz really works)
//	
//if (Input.GetKey (KeyCode.F15) && KeyInputDelayTimer+0.1<Time.time)	
//{
//	KeyInputDelayTimer=Time.time;
//	Application.targetFrameRate = 120;	
//	Time.captureFramerate = 120;	
//	TargetFPS=120;
//}
//
//if (Input.GetKey (KeyCode.F14) && KeyInputDelayTimer+0.1<Time.time)	
//{
//	KeyInputDelayTimer=Time.time;
//	Application.targetFrameRate = 60;	
//	Time.captureFramerate = 60;	
//	TargetFPS=60;
//}
//
//
//
//
//
//if(is3dOn==0) // check for more input keys if 3D Mode is on.
//{
//
// Key to Toggle Eyes
//		
//if (Input.GetKey (KeyCode.F1) && KeyInputDelayTimer+0.1<Time.time)
//{
//	ToggleLR=ToggleLR=1;	
//	KeyInputDelayTimer=Time.time;
//}
//
//
// Change Separation
//
//if (Input.GetKey (KeyCode.F2) && KeyInputDelayTimer+0.02<Time.time)
//{
//	KeyInputDelayTimer=Time.time;
//	Separation=Separation-0.0001;
//	if(Separation<0.0) Separation=0.0;
//	cam2Transform.localPosition.x=Separation;
//	cam1Transform.localPosition.x=0.0-Separation;
//}
//
//if (Input.GetKey (KeyCode.F3) && KeyInputDelayTimer+0.02<Time.time)
//{
//	KeyInputDelayTimer=Time.time;
//	Separation=Separation+0.0001;
//	if(Separation>1.0) Separation=1.0;
//	cam2Transform.localPosition.x=Separation;
//	cam1Transform.localPosition.x=0.0-Separation;
//}
//
//
// Change Convergence
//
//if (Input.GetKey (KeyCode.F4) && KeyInputDelayTimer+0.02<Time.time)
//{
//	KeyInputDelayTimer=Time.time;
//	Convergence=Convergence+0.0001;
//    p1  = originalProjection1;
//    p1.m02 = Convergence;
//    cam1.projectionMatrix = p1;
//    p2  = originalProjection2;
//    p2.m02 = Convergence*-1;
//    cam2.projectionMatrix = p2;
//}
//
//if (Input.GetKey (KeyCode.F5) && KeyInputDelayTimer+0.02<Time.time)
//{
//	KeyInputDelayTimer=Time.time;
//	Convergence=Convergence-0.0001;
//    p1  = originalProjection1;
//    p1.m02 = Convergence;
//    cam1.projectionMatrix = p1;
//    p2  = originalProjection2;
//    p2.m02 = Convergence*-1;
//    cam2.projectionMatrix = p2;
//}
//
//
// Change Field of View
//
//if (Input.GetKey (KeyCode.F6) && KeyInputDelayTimer+0.02<Time.time)
//{
//	KeyInputDelayTimer=Time.time;	
//	FOV=FOV-0.1;
//	cam1.ResetProjectionMatrix();
//	cam2.ResetProjectionMatrix();
//	cam1.fieldOfView = FOV;
//	cam2.fieldOfView = FOV;
//	camMONO.fieldOfView = FOV;
//	originalProjection1 = cam1.projectionMatrix;
//	originalProjection2 = cam2.projectionMatrix;
// 	p1  = originalProjection1;
//    p1.m02 = Convergence;
//    cam1.projectionMatrix = p1;
//    p2  = originalProjection2;
//    p2.m02 = Convergence*-1;
//    cam2.projectionMatrix = p2;  
//}
//
//if (Input.GetKey (KeyCode.F7) && KeyInputDelayTimer+0.02<Time.time)
//{
//	KeyInputDelayTimer=Time.time;	
//	FOV=FOV+0.1;
//	cam1.ResetProjectionMatrix();
//	cam2.ResetProjectionMatrix();
//	cam1.fieldOfView = FOV;
//	cam2.fieldOfView = FOV;
//	camMONO.fieldOfView = FOV;
//	originalProjection1 = cam1.projectionMatrix;
//	originalProjection2 = cam2.projectionMatrix;
// 	p1  = originalProjection1;
//    p1.m02 = Convergence;
//    cam1.projectionMatrix = p1;
//    p2  = originalProjection2;
//    p2.m02 = Convergence*-1;
//    cam2.projectionMatrix = p2;
//}
//
//
// Change distance of the Camera to the Character
//
//if (Input.GetKey (KeyCode.F8) && KeyInputDelayTimer+0.02<Time.time)
//{
//	KeyInputDelayTimer=Time.time;
//	CamDistance =CamDistance -0.01;
//  	transform.localPosition.z=CamDistance;
//   	if (CamDistance<-2.0)
//   	{
//   		CamDistance=-2.0;
//   	}	
//}
//
//if (Input.GetKey (KeyCode.F9) && KeyInputDelayTimer+0.02<Time.time)
//{
//	KeyInputDelayTimer=Time.time;
//	CamDistance =CamDistance +0.02;
//  	transform.localPosition.z=CamDistance;
//  	 if (CamDistance>2.0)
//   	{
//   		CamDistance=2.0;
//  	}	
//}
//
//
//}
//
//
// Display Info on the HUD 
//
//if(is3dOn==0)
//{
//	HUD.text = "Separation: "+Separation+"\n"+"Convergence: "+(Convergence*-1.0)+"\n"+"FOV: "+FOV+"\n"+"Target Hz: "+TargetFPS+"\n"+"3D: On"+"\n\n\nKeys for Visual Options:\nBackspace: Stereo 3D On/Off\nF1: Toggle L&R\nF2/F3: Separation\nF4/F5: Convergence\nF6/F7: Field of View\nF14: 60Hz / F15: 120Hz (DLP-Link)\nH: Toggle HUD\n";
//}
//else
//	{
//		HUD.text = "Separation: Off"+"\n"+"Convergence: Off"+"\n"+"FOV: Off"+"\n"+"Target Hz: "+TargetFPS+"\n"+"3D: Off"+"\n\n\nKeys for Visual Options:\nBackspace: Stereo 3D On/Off\nF1: Toggle L&R\nF2/F3: Separation\nF4/F5: Convergence\nF6/F7: Field of View:\nF14: 60Hz / F15: 120Hz (DLP-Link)\nH: Toggle HUD";
//	}
//
//}
//
//
//
//
//
//function FixedUpdate () {    // Used to switch camera pictures at 120Hz (when Synch to VBL is turned on and FixedTimestep is set to 0.008333334)
//
//
//if(ToggleLR==1) // Toggle Eyes
//{
//	ToggleLR=0;
//	if(CamLorR==1)
//	{
//		CamLorR=0;
//	}
//	else
//	{
//		CamLorR=1;
//	}
//}
//
// Needed if we switch back from 60Hz to 120Hz Mode
//
//if(CamLorR==2 && TargetFPS==120)
//{
//	CamLorR=0;	
//}
//
//if(CamLorR==3 && TargetFPS==120)
//{
//	CamLorR=1;	
//}
//
// Switch Cameras @120Hz
//
//if(CamLorR==0 && TargetFPS==120)
//{
//	cam1.Render();	
//	cam2.enabled = false;
//	cam1.enabled = true;
//	CamLorR=1;
//}
//else
//{
//	if(CamLorR==1 && TargetFPS==120)	
//	{
//		cam2.Render();	
//		cam1.enabled = false;
//		cam2.enabled = true;		
//		CamLorR=0;
//	}
//}
//
//
//
// 60Hz Mode - only for old CRT Shutterglases - just ignore, I don't know if this will work at all
// Switch Cameras @60Hz
//
//if(CamLorR==0 && TargetFPS==60)
//{
//	cam1.Render();	
//	cam2.enabled = false;
//	cam1.enabled = true;
//	CamLorR=1;
//}
//else
//{
//	if(CamLorR==1 && TargetFPS==60)	
//	{
//		cam1.Render();	
//		cam2.enabled = false;	
//		cam1.enabled = true;		
//		CamLorR=2;
//	}
//	else
//	{
//		if(CamLorR==2 && TargetFPS==60)	
//		{
//			cam2.Render();	
//			cam1.enabled = false;
//			cam2.enabled = true;		
//			CamLorR=3;
//		}
//		else
//		{
//			if(CamLorR==3 && TargetFPS==60)	
//			{
//				cam2.Render();	
//				cam1.enabled = false;
//				cam2.enabled = true;		
//				CamLorR=0;
//			}	
//		}	
//	}
//}
//
//}