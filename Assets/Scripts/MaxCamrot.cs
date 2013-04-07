/// @file MaxCamrot.cs
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
/// $Id: MaxCamrot.cs 213 2013-04-06 21:13:42Z baaden $
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

// // $Id: MaxCamrot.cs 213 2013-04-06 21:13:42Z baaden $
// // Filename: maxCamera.cs
// //
// // original: http://www.unifycommunity.com/wiki/index.php?title=MouseOrbitZoom
// //
// // --01-18-2010 - create temporary target, if none supplied at start

// using UnityEngine;
// using System.Collections;
// using UI;

// //[AddComponentMenu("Camera-Control/3dsMax Camera Style")]
// public class MaxCamrot : MonoBehaviour
// {
// 	public static bool cameraStop = false;
// 	public bool automove   = true;
	
//     public Transform target;
//     public Vector3 targetOffset;
//     public float distance = 50.0f;
//     public float maxDistance = 1000;
//     public float minDistance = 0f;
//     public float xSpeed = 400.0f;
//     public float ySpeed = 400.0f;
//     public int yMinLimit = -80;
//     public int yMaxLimit = 80;
//     public int zoomRate = 100;
//     public float panSpeed = 0.3f;
//     public float zoomDampening = 5.0f;
	
//     private float zDeg = 0.0f;
//     public static float currentDistance;
//     private float desiredDistance;
//     private float dir = 0f;
// 	private float duration= 0.2f; // time in seconds
// 	private float angle = 12f; // angle to turn left or right
// 	private float limit = 0.3f; // set the sensitivity
// 	private float speed = 100f; // isn't used in this code
	
// 	private Quaternion currentRotation;
//     private Quaternion desiredRotation;
//     private Quaternion rotation;
// 	private Quaternion currentRotationZone;
//     private Quaternion desiredRotationZone;
//     private Quaternion rotationZone;
// 	private Quaternion test;
//     private Vector3 position;

//     void Start() { Init(); }
//     void OnEnable() { Init(); }

//     public void Init()
//     {
 
//         //be sure to grab the current rotations as starting points.
//         position = transform.position;
//         rotation = transform.rotation;
//         currentRotation = transform.rotation;
//         desiredRotation = transform.rotation;
        
//         zDeg = Vector3.Angle(Vector3.zero, transform.up);
//     }


//     /*
//      * Camera logic on LateUpdate to only update after all character movement logic has been handled. 
//      */
//     void LateUpdate()
//     {
// 	    	if(cameraStop == false){
// 				if (Input.GetMouseButton(0))
// 		        {
// 		        	if(UIData.switchmode)ToParticle();
						
// 	// implmemet of a rotation mod when the mouse is in the right of the scene.
// 	// TODO : Probleme when we rotate the protéine, the comand rotate too.
// 					if (Input.mousePosition.x>Screen.width-100)
// 						{
// 						zDeg -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
// 						}
// 					zDeg = ClampAngle(zDeg, -360,360);
		 
// 					desiredRotation = Quaternion.Euler(0, 0, zDeg);
// 		            currentRotation = transform.rotation;
		            
// 		            rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
// 		            transform.rotation = rotation;
						
				
				
// 			}
// 	}
// 	}
// // clamp the value of the angle between -360 and 360, in an angle  361 = 1  
//     private static float ClampAngle(float angle, float min, float max)
//     {
//         if (angle < -360)
//             angle += 360;
//         if (angle > 360)
//             angle -= 360;
//         return Mathf.Clamp(angle, min, max);
//     }
    
	
	
// // switch mode : change visualisation to hyperball
//     public void ToHyperBall() {
// 		GameObject LoadBox=GameObject.Find("LoadBox");
// 			Molecule3D Molecule3DComp = LoadBox.GetComponent<Molecule3D>();
// 		Molecule3DComp.displayMolecule.ToHyperBall();

// 	}
	
	
	
// // switch mode : change visualisation to particle	
// 	public void ToParticle() {
// 		GameObject LoadBox=GameObject.Find("LoadBox");
// 			Molecule3D Molecule3DComp = LoadBox.GetComponent<Molecule3D>();
// 		Molecule3DComp.displayMolecule.ToParticle();

// 	}
	
	
// }
