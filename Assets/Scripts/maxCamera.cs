/// @file maxCamera.cs
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
/// $Id: maxCamera.cs 634 2014-08-01 09:14:04Z trellet $
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

// $Id: maxCamera.cs 634 2014-08-01 09:14:04Z trellet $
// Filename: maxCamera.cs
//
// original: http://www.unifycommunity.com/wiki/index.php?title=MouseOrbitZoom
//
// --01-18-2010 - create temporary target, if none supplied at start
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UI;
using Molecule.Model;
using System;

//[AddComponentMenu("Camera-Control/3dsMax Camera Style")]
public class maxCamera : MonoBehaviour
{
	public static bool cameraStop = false;
	public bool automove = false;
	public Transform target;
	public Vector3 targetOffset;
	public float distance = 20.0f;
	public float maxDistance = 1000;
	public float minDistance = -1000f;
	public float xSpeed = 400.0f;
	public float ySpeed = 400.0f;
	public int yMinLimit = -80;
	public int yMaxLimit = 80;
	public int zoomRate = 200;
	public float panSpeed = 2f;
	public float zoomDampening = 20.0f ;
	public float rotationFactor = 50.0f ;
	public float strength = 0.5f; // for camera translation
	public int reslim =192; // for second step spreading (currently only available for GLIC)
	
	public Vector3 originalCamPosition;
	public bool centerChanged = false; // for camera rotation;

	public Vector3 newposition;
	public Vector3 cameraLocalPositionSave;

	public bool cameraTranslation = false;
	public float weight = 0.0f;
	public float joypadDeadzone = 0.01f;
	private float xDeg = 0.0f;
	private float yDeg = 0.0f;
	private float zDeg = 0.0f;
	public static float currentDistance;
	public static float desiredDistance;
	private Quaternion currentRotation;
	private Quaternion desiredRotation;
	private Quaternion rotation;
	private Vector3 position;
	private bool guiControl = false;
	private GameObject LoadBox;
	public static GameObject LocCamera;
	private Molecule3D Molecule3DComp;

	// +------------------------------+
	// | Guided Navigation Variables  |
	// +------------------------------+

	//Variables for constrained navigation
	public float nav_speed = 1.0f;
	public bool navigationUp = false;
	public bool navigationDown = false;
	public bool guidedzoom = false;
	public bool checkangle = false;
	//public bool checkangle2 = false;
	public bool isneg = false;
	public Vector3 move;
	public bool panoramic = false;
	public bool reset_panoramic;
	public bool isabove = false;
	public bool isunder = false;
	public bool dirup = false;
	public bool dirdown = false;

	//Variables for "jump" navigation
	public float rotation_done = 0.0f;
	public bool monomer_jump = false;
	public bool next_right = false;
	public bool next_left = false;

	//Variables for Spreading/Narrowing
	private BallUpdateHB[] hballs;
	private BallUpdateSphere[] sballs;
	private BallUpdate[] pballs;
	private Vector3 spreadA, spreadA1, spreadA2;
	private Vector3 spreadB, spreadB1, spreadB2;
	private Vector3 spreadC, spreadC1, spreadC2;
	private Vector3 spreadD, spreadD1, spreadD2;
	private Vector3 spreadE, spreadE1, spreadE2;
	private double comp_spread = 0;
	private AtomTree atomtree;
	private string rep = "";
	private float[] closeatom;
	private Vector3 poscloseatom;
	private float distclose;
	private bool near_spread = true;
	private bool onlyTMD = false;

	// Variables for best point of view transition //
	// PyMol like transition
	public GameObject ghost_target;
	public GameObject ghost_target_real;
	private bool ghost_target_instantiate = false;
	public static Vector3 optim_target;
	public static Vector3 optim_cam_position;
	public bool cam_pos_optimized = false;
	public float angle = 0;
	private Vector3 velocity = Vector3.zero;
	public bool RemotePhase=true;
	public bool HeightPhase=true;
	public bool RotatePhase=true;
	// Straight transition
	public bool ApproachPhase=true;
	public bool smooth = false;
	public bool FocusPhase = true;
	public float transitionDuration = 2.5f;
	// Elliptic transition
//	public Vector3 center = new Vector3(0, 1, 0);
//	public float radiusA;
//	public float radiusB = MoleculeModel.;

	
	//Nouvelle variable

	void Awake ()
	{
		Init ();
	}

	void OnEnable ()
	{
		Init ();
	}
	
	public void Init ()
	{
		LoadBox = GameObject.Find ("LoadBox");
		LocCamera = GameObject.Find ("Camera");

		Molecule3DComp = LoadBox.GetComponent<Molecule3D> ();
		//If there is no target, create a temporary target at 'distance' from the cameras current viewpoint
		if (!target) {
			GameObject go = new GameObject ("Cam Target");
			//transform.position = go.transform.position + (transform.forward * distance);
			target = go.transform;
		}
        
		// Debug.Log("Cam distance : " + Vector3.Distance(transform.position, target.position));
		distance = Vector3.Distance (transform.position, target.position);
		currentDistance = distance;
		desiredDistance = distance;
                
		//be sure to grab the current rotations as starting points.
		position = transform.position;
		rotation = transform.rotation;
		// position = Vector3.zero;
		// rotation = Quaternion.identity;
		currentRotation = transform.rotation;
		desiredRotation = transform.rotation;
        
		xDeg = Vector3.Angle (Vector3.right, transform.right);
		yDeg = Vector3.Angle (Vector3.up, transform.up);
	}
    
	public void upxDeg ()
	{
		xDeg = 1;
		guiControl = true;
	}

	public void upyDeg ()
	{
		yDeg = -1;
		guiControl = true;
	}

	public void downxDeg ()
	{
		xDeg = -1;
		guiControl = true;
	}

	public void downyDeg ()
	{
		yDeg = 1;
		guiControl = true;
	}
	
	public void downzDeg ()
	{
		zDeg = 1;
		guiControl = true;
	}

	public void upzDeg ()
	{
		zDeg = -1;
		guiControl = true;
	}

	void Update ()
	{
		if (Input.GetKeyUp (KeyCode.Space)) {
			Debug.Log ("Space push");
			automove = !automove;
			GUIMoleculeController.toggle_NA_AUTOMOVE = !GUIMoleculeController.toggle_NA_AUTOMOVE;
			Molecule3DComp.toggleFPSLog ();
		}


		//Camera sliding and re-center on a atom or a group or atom
		if (Input.GetKeyUp (KeyCode.R)) {
			cameraTranslation = true;
			centerChanged = true;
			newposition = Vector3.zero;
		}

		if (Input.GetKeyUp (KeyCode.C)) {

				if (UI.GUIMoleculeController.toggle_NA_CLICK) {

					/*if just one atom is selected, we center the camera (CAM TARGEt!!! THE CAMERA FOLLOW CAM TARGET)
					on this atom*/
					if (Camera.main.GetComponent<ClickAtom> ().objList.Count == 1) {
						
						int atomnumber = (int)Camera.main.GetComponent<ClickAtom> ().objList [0].GetComponent<BallUpdate> ().number;
						newposition = new Vector3 (MoleculeModel.atomsLocationlist [atomnumber] [0],
						                          MoleculeModel.atomsLocationlist [atomnumber] [1],
						                          MoleculeModel.atomsLocationlist [atomnumber] [2]);

						cameraTranslation = true;
						centerChanged = true;
						/* if multiple atom is selected, we calcul the barycenter*/
					} else if (Camera.main.GetComponent<ClickAtom> ().objList.Count > 1) {

						//barycenter of all atom selection calculation
						Vector3 barycenter = new Vector3 ();
						float xTot = 0.0f, yTot = 0.0f, zTot = 0.0f;
						for (int i=0; i<Camera.main.GetComponent<ClickAtom>().objList.Count; i++) {
							int atomnumber = (int)Camera.main.GetComponent<ClickAtom> ().objList [i].GetComponent<BallUpdate> ().number;
							xTot = xTot + MoleculeModel.atomsLocationlist [atomnumber] [0];
							yTot = yTot + MoleculeModel.atomsLocationlist [atomnumber] [1];
							zTot = zTot + MoleculeModel.atomsLocationlist [atomnumber] [2];
						}
						barycenter.x = xTot / Camera.main.GetComponent<ClickAtom> ().objList.Count;
						barycenter.y = yTot / Camera.main.GetComponent<ClickAtom> ().objList.Count;
						barycenter.z = zTot / Camera.main.GetComponent<ClickAtom> ().objList.Count;
					
						newposition = barycenter;

						cameraTranslation = true;
						centerChanged = true;
					}
				}
		}

		if(UIData.guided){

			//Automatic constrained navigation up
			if (Input.GetKeyUp (KeyCode.U)) {
				navigationDown = false;
				navigationUp = !navigationUp;

			}

			//Automatic constrained navigation down
			if (Input.GetKeyUp (KeyCode.J)) {
				navigationUp = false;
				navigationDown = !navigationDown;
			}

			//Enter panoramic mode
			if (Input.GetKeyUp (KeyCode.I)) {
				panoramic = !panoramic;
				Debug.Log ("panoramic " + panoramic);

				if(!panoramic)
					reset_panoramic = true;
			}

			//Enter jump mode
			if (Input.GetKeyUp (KeyCode.M)) {
				monomer_jump = !monomer_jump;
				Debug.Log("monomer jump: " + monomer_jump);
			}
		
			if(Input.GetKeyUp (KeyCode.L))
			   near_spread = !near_spread;

			if(near_spread)
				NearSpreading();
		}
	}
	
	/// <summary>
	/// Only available in "guided navigation" mode and specific to pentameric channels.
	/// Constrained path for camera moving around the protein structure.
	/// </summary>
	void goUpConstrained()
	{
		Vector3 v = transform.position;
		Vector3 r = transform.eulerAngles;

		dirup = true;
		if (dirdown) {
			checkangle = false;
			//dirdown = false;
		}

		if (float.Parse(MoleculeModel.FPS) > 30)
			nav_speed = SpeedNavigation();

		if(Vector3.Distance(transform.position, new Vector3(0, transform.position.y, 0)) < 2.0 && angle < 180){
			//Vector3 target = new Vector3(-LocCamera.transform.position.x,LocCamera.transform.position.y,-LocCamera.transform.position.z );
			//LocCamera.transform.RotateAround(new Vector3(0, LocCamera.transform.position.y, 0), new Vector3(0,1,0), 6);
			transform.RotateAround(new Vector3(0, transform.position.y, 0), new Vector3(0,1,0), nav_speed*6);
			angle += nav_speed*6;
			//Debug.Log("Angle: "+angle);
		}

		else if(Vector3.Distance(transform.position, new Vector3(0, transform.position.y, 0)) > 2.0 && angle >= 180)
		{
			angle = 0;
		}

		else if ((LocCamera.transform.position.y >= MoleculeModel.MaxValue.y)) {

			UIData.up_part = false;
			UIData.down_part = true;
			isabove = true;

			transform.LookAt(new Vector3(0, MoleculeModel.MaxValue.y, 0));
			Vector3 tempv = transform.position - new Vector3 (0, MoleculeModel.MaxValue.y, 0);

			if(!checkangle){
				if((r.y < 90 || r.y > 270) && (!dirdown))
					isneg = true;
				else
					isneg = false;
				checkangle = true;
			}

			if(isneg)
				move = new Vector3(0, -tempv.z, 0);
			else
				move = new Vector3(0, tempv.z, 0);

			move.Normalize ();
			transform.Translate(move);
			dirdown = false;

			target.position = new Vector3(0, MoleculeModel.MaxValue.y, 0);

		}

		else if ((LocCamera.transform.position.y <= MoleculeModel.MinValue.y)){

			UIData.up_part = true;
			UIData.down_part = false;
			isunder = true;

			transform.LookAt(new Vector3(0, MoleculeModel.MinValue.y, 0));
			Vector3 tempv = transform.position - new Vector3 (0, MoleculeModel.MinValue.y, 0) ;

			if(!checkangle){
				if((r.y < 90 || r.y > 270) && (!dirdown))
					isneg = true;
				else
					isneg = false;
				checkangle = true;
			}
			
			if(isneg)
				move = new Vector3(0, tempv.z, 0);
			else
				move = new Vector3(0, -tempv.z, 0);
			
			move.Normalize ();
			transform.Translate(move);
			dirdown = false;

			target.position = new Vector3(0, MoleculeModel.MinValue.y, 0);
			
		} else {
			if(UIData.up_part){
				checkangle = false;
				isabove = false;
				isunder = false;
				transform.eulerAngles = new Vector3 (0, transform.eulerAngles.y, 0);
				v.y += nav_speed;
				transform.position = v;
				target.position = new Vector3(0, v.y, 0);
			}else{
				checkangle = false;
				isabove = false;
				isunder = false;
				transform.eulerAngles = new Vector3 (0, transform.eulerAngles.y, 0);
				v.y -= nav_speed;
				transform.position = v;
				target.position = new Vector3(0, v.y, 0);
			}
		}
	}

	/// <summary>
	/// Only available in "guided navigation" and specific to pentameric channels.
	/// Constrained path for camera moving around the protein structure (second way).
	/// </summary>
	void goDownConstrained()
	{

		Vector3 v = transform.position;
		Vector3 r = transform.eulerAngles;

		dirdown = true;
		if (dirup) {
			checkangle = false;
			//dirup = false;
		}

		if (float.Parse(MoleculeModel.FPS) > 30)
			nav_speed = SpeedNavigation();

		if(Vector3.Distance(transform.position, new Vector3(0, transform.position.y, 0)) < 2.0 && angle < 180){
			//Vector3 target = new Vector3(-LocCamera.transform.position.x,LocCamera.transform.position.y,-LocCamera.transform.position.z );
			transform.RotateAround(new Vector3(0, transform.position.y, 0), new Vector3(0,1,0), 6);
			angle += 6;
			//Debug.Log("Angle: "+angle);
		}

		else if(Vector3.Distance(transform.position, new Vector3(0, transform.position.y, 0)) > 2.0 && angle >= 180)
		{
			angle = 0;
		}

		else if ((LocCamera.transform.position.y >= MoleculeModel.MaxValue.y)) {

			UIData.down_part = false;
			UIData.up_part = true;
			isabove = true;

			transform.LookAt(new Vector3(0, MoleculeModel.MaxValue.y, 0));
			Vector3 tempv = transform.position - new Vector3 (0, MoleculeModel.MaxValue.y, 0) ;

			if(!checkangle){
				if((r.y < 90 || r.y > 270) && (!dirup))
					isneg = true;
				else
					isneg = false;
				checkangle = true;
			}
			
			if(isneg)
				move = new Vector3(0, -tempv.z, 0);
			else
				move = new Vector3(0, tempv.z, 0);
			
			move.Normalize ();
			transform.Translate(move);
			dirup = false;

			target.position = new Vector3(0, MoleculeModel.MaxValue.y, 0);
		}

		else if ((LocCamera.transform.position.y <= MoleculeModel.MinValue.y)){

			UIData.down_part = true;
			UIData.up_part = false;
			isunder = true;

			transform.LookAt(new Vector3(0, MoleculeModel.MinValue.y, 0));
			Vector3 tempv = transform.position - new Vector3 (0, MoleculeModel.MinValue.y, 0) ;

			if(!checkangle){
				if((r.y < 90 || r.y > 270) && (!dirup))
					isneg = true;
				else
					isneg = false;
				checkangle = true;
			}
			
			if(isneg)
				move = new Vector3(0, tempv.z, 0);
			else
				move = new Vector3(0, -tempv.z, 0);
			
			move.Normalize ();
			transform.Translate(move);
			dirup = false;

			target.position = new Vector3(0, MoleculeModel.MinValue.y, 0);

		} else {
			if(UIData.down_part){
				checkangle = false;
				isabove = false;
				isunder = false;
				transform.eulerAngles = new Vector3 (0, LocCamera.transform.eulerAngles.y, 0);
				v.y += nav_speed;
				transform.position = v;
				target.position = new Vector3(0, v.y, 0);
			}else{
				checkangle = false;
				isabove = false;
				isunder = false;
				transform.eulerAngles = new Vector3 (0, LocCamera.transform.eulerAngles.y, 0);
				v.y -= nav_speed;
				transform.position = v;
				target.position = new Vector3(0, v.y, 0);
			}
		}
	}

	private float SpeedNavigation()
	{
			float tot = Math.Abs (MoleculeModel.MinValue.y) + Math.Abs (MoleculeModel.MaxValue.y);
			return (tot / 120);
	}
	
	IEnumerator Transition()
	{
		float t = 0.0f;
		Vector3 startingPos = transform.position;
		while (t < 1.0f)
		{
			t += Time.deltaTime * (Time.timeScale/transitionDuration);
			
			transform.position = Vector3.Lerp(startingPos, optim_target, t);
			yield return 0;
		}
	}

	/*
     * Camera logic on LateUpdate to only update after all character movement logic has been handled. 
     */
	void LateUpdate ()
	{
		if (cameraStop == false) {
			keyboardOperate ();
			if (!guiControl)
				joypadOperate ();

			// Check if guided navigation is on, if yes, no mouse interactions
			if (!UIData.guided) {
						// If Control and Alt and Middle button? ZOOM!
				if (Input.GetMouseButton (1)) {
					if (UIData.switchmode)
						Molecule3DComp.ToParticle ();
						desiredDistance -= Input.GetAxis ("Mouse Y") * Time.deltaTime * zoomRate;//* Mathf.Abs(desiredDistance);
					}
	            // If middle mouse and left alt are selected? ORBIT
	            else if (Input.GetMouseButton (0) && !guiControl) {
					if (UIData.switchmode)
						Molecule3DComp.ToParticle ();
					if (Input.mousePosition.x < Screen.width * 0.85f && Input.mousePosition.y < Screen.height * 0.85f && Input.mousePosition.y > Screen.height * 0.15f) {	
						xDeg = Input.GetAxis ("Mouse X") * xSpeed * 0.02f;
						yDeg = -Input.GetAxis ("Mouse Y") * ySpeed * 0.02f;
					} else if (Input.mousePosition.x > Screen.width * 0.85f) {
						yDeg = -Input.GetAxis ("Mouse Y") * xSpeed * 0.02f;
						xDeg = 0;
					} else if (Input.mousePosition.y > Screen.height * 0.85f) {
						xDeg = Input.GetAxis ("Mouse X") * xSpeed * 0.02f;
						yDeg = 0;
					} else {
						zDeg = Input.GetAxis ("Mouse X") * ySpeed * 0.02f;
						yDeg = 0;  					
					}


				}
	            // otherwise if middle mouse is selected, we pan by way of transforming the target in screenspace
	            else if (Input.GetMouseButton (2)) {
					if (UIData.switchmode)
						Molecule3DComp.ToParticle ();
						Vector3 v = LocCamera.transform.localPosition;
						v.x -= Input.GetAxis ("Mouse X") * panSpeed;
						v.y -= Input.GetAxis ("Mouse Y") * panSpeed;
						LocCamera.transform.localPosition = v;
					} else {
						if (UIData.switchmode)
						Molecule3DComp.ToNotParticle ();
					}
				} //End if guided

				// get the desired Zoom distance if we roll the scrollwheel
				if (!UIData.hiddenCamera)
				if (!Camera.main.orthographic){ // only if the camera is in perspective mode
					desiredDistance -= Input.GetAxis ("Mouse ScrollWheel") * Time.deltaTime * zoomRate;// * Mathf.Abs(desiredDistance);
					if(UIData.guided && (desiredDistance != currentDistance)){
						//currentDistance = Vector3.Distance (transform.position, target.position);
						guidedzoom = true;
				}
				}else { // otherwise (orthographic mode) we can achieve the same effet by making the camera bigger/smaller
					float tmp_size = Camera.main.orthographicSize - Input.GetAxis ("Mouse ScrollWheel") * Time.deltaTime * zoomRate;
					if (tmp_size <= LoadTypeGUI.maxOrthoSize && tmp_size >= LoadTypeGUI.minOrthoSize)
						Camera.main.orthographicSize = tmp_size;
				}

				if (automove == true) {
					if (UIData.switchmode)
						Molecule3DComp.ToParticle ();
					xDeg += Mathf.Lerp (0.0F, 100.0F, Time.deltaTime * 0.8f);
					yDeg = 0;
				}
	
				if (centerChanged) {
					//if we reach the position (for camera sliding) we stop!
	
					//otherwise we continue the translation.
					if (cameraTranslation) {
	
						weight += Time.deltaTime * 1;
						target.localPosition = Vector3.Lerp (target.localPosition, newposition, weight);
	
						LocCamera.transform.localPosition = Vector3.Lerp (LocCamera.transform.localPosition, Vector3.zero, weight);

						//LocCamera.transform.eulerAngles = Vector3.Lerp (LocCamera.transform.eulerAngles, Vector3.zero, weight);
	
					if (cameraTranslation && target.position == newposition && LocCamera.transform.localPosition== Vector3.zero) {
	
							weight = 0;
							cameraTranslation = false;
							centerChanged = false;
						}
					}
				}
				//Camera rotation
				desiredRotation *= Quaternion.Euler (yDeg, xDeg, zDeg);
				currentRotation = transform.rotation;
				rotation = Quaternion.Lerp (currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
				if(!UIData.guided || reset_panoramic){
					transform.rotation = rotation;
				}

				//Camera movement
				currentDistance = Mathf.Lerp (currentDistance, desiredDistance, Time.deltaTime * zoomDampening);
				position = target.position - (rotation * Vector3.forward * currentDistance + targetOffset);
				if(!UIData.guided || reset_panoramic){
					transform.position = position;
					reset_panoramic = false;
				}

				// Temporary
				if(guidedzoom){
					//Dive into the channel if zoom above or under the structure
					if((Vector3.Distance(transform.position , target.position) < 100) && isabove){
						near_spread = false;
						if(transform.position.y <= target.position.y+20)
							target.position = new Vector3(0, target.position.y-1, 0);
						transform.LookAt(target.position);
						transform.Translate(new Vector3(0, 0, Input.GetAxis ("Mouse ScrollWheel") * Time.deltaTime * zoomRate));
					}
					else if (Vector3.Distance (transform.position, target.position) < 100 && isunder){
						near_spread = false;
						if(transform.position.y >= target.position.y-20)
							target.position = new Vector3(0, target.position.y+1, 0);
						transform.LookAt (target.position);
						transform.Translate (new Vector3(0, 0, Input.GetAxis ("Mouse ScrollWheel") * Time.deltaTime * zoomRate));
					}
					else
						transform.Translate(new Vector3(0, 0, Input.GetAxis ("Mouse ScrollWheel") * Time.deltaTime * zoomRate));
				guidedzoom = false;
				}
	
				xDeg = 0;
				yDeg = 0;
				zDeg = 0;
				guiControl = false; 
			}
		//}

		if (navigationUp) 
				goUpConstrained();	

		if(navigationDown)
				goDownConstrained();

		if(panoramic){
			Vector3 v = transform.position;
			weight += Time.deltaTime * 1;
			transform.position = Vector3.Lerp (v, new Vector3(0, v.y, 0), weight);
		}
		
		if(UIData.optim_view)
		{
			if(!ghost_target_instantiate){
				Debug.LogWarning("CREATE TARGET OBJECT");
				ghost_target_real = (GameObject) GameObject.Instantiate(ghost_target, optim_target, Quaternion.identity);
				ghost_target_real.name = "Target";
				ghost_target_real.GetComponent<Renderer>().material.color = Color.magenta;
				ghost_target_real.transform.localScale = new Vector3(4.0f, 4.0f, 4.0f);
				ghost_target_real.SetActive(true);
				ghost_target_instantiate = true;
			}
			if(LocCamera.transform.position != optim_cam_position)
			{
				Debug.Log("optim: " + optim_cam_position);
				Debug.Log("camera: " + LocCamera.transform.position);
				float distance = Vector3.Distance(LocCamera.transform.position, new Vector3(0,0,0));
//				Debug.Log("initial camera location z ++: " + (-MoleculeModel.cameraLocation.z) * 1.05);
//				Debug.Log("initial camera location z --: " + (-MoleculeModel.cameraLocation.z) * 0.95);
//				Debug.Log ("height comparison: "+ !Mathf.Approximately(LocCamera.transform.position.y, optim_cam_position.y) + " smooth: " + smooth);
				
				
				if(RemotePhase){
					LocCamera.transform.position = Vector3.SmoothDamp(LocCamera.transform.position, optim_cam_position, ref velocity, 5.0f);
					LocCamera.transform.LookAt(optim_target);
				}
				
				
//				if(distance < (- MoleculeModel.cameraLocation.z * 0.95) && RemotePhase){
//					Debug.Log ("Remote Phase zoom out");
////					float distCovered = (Time.time - UIData.start_time) * 1.0F;
////					float fracJourney = distCovered / 4.0F;
////					LocCamera.transform.position = Vector3.Lerp(UIData.optim_view_start_point, new Vector3(LocCamera.transform.position.x, LocCamera.transform.position.y, MoleculeModel.cameraLocation.z), fracJourney);
////					LocCamera.transform.LookAt(new Vector3(0,LocCamera.transform.position.y,0));
//					LocCamera.transform.position = Vector3.SmoothDamp(LocCamera.transform.position, new Vector3(LocCamera.transform.position.x, LocCamera.transform.position.y, MoleculeModel.cameraLocation.z), ref velocity, 0.5f);
//					//LocCamera.transform.Translate((LocCamera.transform.position - new Vector3(0,LocCamera.transform.position.y, 0)) * Time.deltaTime * 0.5f, Space.World);
//				}
//				else if(distance > (-MoleculeModel.cameraLocation.z * 1.05) && RemotePhase){
//					Debug.Log ("Remote Phase zoom in");
//					float distCovered = (Time.time - UIData.start_time) * 1.0F;
//					float fracJourney = distCovered / 2.0F;
//					LocCamera.transform.position = Vector3.Lerp(UIData.optim_view_start_point, new Vector3(LocCamera.transform.position.x, LocCamera.transform.position.y, MoleculeModel.cameraLocation.z), fracJourney);
//					//LocCamera.transform.Translate((new Vector3(0,LocCamera.transform.position.y, 0) - LocCamera.transform.position) * Time.deltaTime * 0.5f, Space.World);
//				}
////				else if(smooth){
////					RemotePhase = false;
////					HeightPhase = false;
////					RotatePhase = false;
////					ApproachPhase = false;
////					if(FocusPhase && Quaternion.Angle(LocCamera.transform.rotation, Quaternion.LookRotation(optim_target - LocCamera.transform.position)) < 1.0F){
////						Quaternion lookat = Quaternion.LookRotation(optim_target - LocCamera.transform.position);
////						Debug.Log("Rotation : "+LocCamera.transform.rotation+" "+lookat);
////						LocCamera.transform.rotation = Quaternion.Slerp(LocCamera.transform.rotation, lookat, Time.deltaTime);
////					}
////					else if (Vector3.Distance(LocCamera.transform.position, optim_cam_position) < 0.5){
////						FocusPhase = false;
////						Debug.Log("MoveTowards");
////						float step = 1.0F * Time.deltaTime;
////						LocCamera.transform.position = Vector3.SmoothDamp(LocCamera.transform.position, optim_cam_position, ref velocity, 2.0f);
////						LocCamera.transform.LookAt(optim_target);
////					}
////					//float step = 1.0F * Time.deltaTime;
////				}
//				else if(!Mathf.Approximately(LocCamera.transform.position.y, optim_cam_position.y) && HeightPhase){
//					RemotePhase = false;
//					Debug.Log ("Height Phase");
//					LocCamera.transform.position = Vector3.SmoothDamp(LocCamera.transform.position, new Vector3(LocCamera.transform.position.x, optim_cam_position.y, LocCamera.transform.position.z), ref velocity, 0.3f);
//					UIData.start_time = Time.time;
//					UIData.optim_view_start_point = LocCamera.transform.position - new Vector3(0, LocCamera.transform.position.y, 0);
//				}
//				else if(Vector3.Angle( (LocCamera.transform.position - new Vector3(0, LocCamera.transform.position.y, 0)), (optim_cam_position - new Vector3(0, LocCamera.transform.position.y, 0))) > 2 && RotatePhase){
//					HeightPhase = false;
//					Debug.Log ("Rotate Phase");
//					Debug.Log (Vector3.Angle( (LocCamera.transform.position - new Vector3(0, LocCamera.transform.position.y, 0)), (optim_cam_position - new Vector3(0, LocCamera.transform.position.y, 0))));
////					Vector3 center = (UIData.optim_view_start_point + (optim_cam_position - new Vector3(0, LocCamera.transform.position.y, 0))) * 0.5F;
////					center -= new Vector3(0, 1, 0);
////					Vector3 riseRelCenter = UIData.optim_view_start_point - center;
////					Vector3 setRelCenter = (optim_cam_position - new Vector3(0, LocCamera.transform.position.y, 0)) - center;
////					float fracComplete = (Time.time - UIData.start_time) / 2.0F;
////					LocCamera.transform.position = Vector3.Slerp(riseRelCenter, setRelCenter, fracComplete);
////					LocCamera.transform.position += center;
//					float angle = Vector3.Angle( (LocCamera.transform.position - new Vector3(0, LocCamera.transform.position.y, 0)), (optim_cam_position - new Vector3(0, LocCamera.transform.position.y, 0)));
//					LocCamera.transform.RotateAround(Vector3.zero, Vector3.up, angle * Time.deltaTime);
//				}
//				else if(LocCamera.transform.position != optim_cam_position && ApproachPhase){
//					Debug.Log("Approach phase");
//					RotatePhase = false;
//					LocCamera.transform.position = Vector3.SmoothDamp(LocCamera.transform.position, optim_cam_position, ref velocity, 0.5f);
//				}
////				Debug.Log ("Distance: "+distance);
//				//LocCamera.transform.position = Vector3.SmoothDamp(LocCamera.transform.position, optim_cam_position, ref velocity, 0.3f);	
			}
			else{
				Debug.Log ("LookAt phase");
				ApproachPhase = false;
				Quaternion lookat = Quaternion.LookRotation(optim_target - LocCamera.transform.position);
				LocCamera.transform.rotation = Quaternion.Slerp(LocCamera.transform.rotation, lookat, Time.deltaTime);
			}
//			LocCamera.transform.position = optim_cam_position;
//			GameObject ghost_target = new GameObject();
//			ghost_target.name = "TARGET";
//			ghost_target.transform.position = optim_target;
//			ghost_target.SetActive(true);
//			LocCamera.transform.LookAt(ghost_target.transform);
		}
		if(UIData.guided){		
			if (next_right && rotation_done < 360/MoleculeModel.existingChain.Count){
				LocCamera.transform.RotateAround(Vector3.zero, Vector3.up, - (360/MoleculeModel.existingChain.Count) * Time.deltaTime);
				rotation_done += (360/MoleculeModel.existingChain.Count) * Time.deltaTime;
			}
			else if (next_left && rotation_done < 360/MoleculeModel.existingChain.Count){
				LocCamera.transform.RotateAround(Vector3.zero, Vector3.up, (360/MoleculeModel.existingChain.Count) * Time.deltaTime);
				rotation_done += (360/MoleculeModel.existingChain.Count) * Time.deltaTime;;
			}
			else if (rotation_done > 360/MoleculeModel.existingChain.Count){
				next_right = false;
				next_left = false;
				rotation_done = 0.0f;
			}
			/*if (next_left && rotation_done < 360/MoleculeModel.existingChain.Count){
				LocCamera.transform.RotateAround(Vector3.zero, Vector3.up, (360/MoleculeModel.existingChain.Count) * Time.deltaTime);
				rotation_done += (360/MoleculeModel.existingChain.Count) * Time.deltaTime;
				Debug.Log("rotation_done " + rotation_done + " next_left " + next_left);
			}
			else if (rotation_done > 360/MoleculeModel.existingChain.Count){
				Debug.Log("STOP left " + next_left);
				next_right = false;
				rotation_done = 0.0f;
			}*/
		}
	}
	
	private static float ClampAngle (float angle, float min, float max)
	{
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;
		return Mathf.Clamp (angle, min, max);
	}
    
	public void ToCenter ()
	{
		LocCamera.transform.localPosition = Vector3.zero;
		target.transform.localPosition = Vector3.zero;
		transform.position = new Vector3 (0, 0, MoleculeModel.cameraLocation.z);
		LocCamera.transform.rotation = new Quaternion (0, 0, 0, 0);
		transform.rotation = Quaternion.identity;
		centerChanged = false;
		weight = 0;
		Init ();
	}

// control of the joypad =============================================================================================      
//                       =============================================================================================      
	private void joypadOperate ()
	{
		target.rotation = transform.rotation;

		Vector3 v = LocCamera.transform.localPosition;
		if (Input.GetAxis ("Axis5") > joypadDeadzone || (Input.GetAxis ("Axis5") < joypadDeadzone))
			v.x -= Input.GetAxis ("Axis5") * panSpeed;
		if (Input.GetAxis ("Axis6") > joypadDeadzone || (Input.GetAxis ("Axis6") < joypadDeadzone))
			v.y -= Input.GetAxis ("Axis6") * panSpeed;
		if (Input.GetAxis ("AxisX") > joypadDeadzone || (Input.GetAxis ("AxisX") < joypadDeadzone))
			v.x -= Input.GetAxis ("AxisX") * panSpeed * 3.0f;
		if (Input.GetAxis ("AxisY") > joypadDeadzone || (Input.GetAxis ("AxisY") < joypadDeadzone))
			v.y -= Input.GetAxis ("AxisY") * panSpeed * 3.0f;
		LocCamera.transform.localPosition = v;

		if (Input.GetAxis ("Axis3") > joypadDeadzone || (Input.GetAxis ("Axis3") < joypadDeadzone))
			xDeg = Input.GetAxis ("Axis3") * xSpeed * 0.08f;

		if (Input.GetAxis ("Axis4") > joypadDeadzone || (Input.GetAxis ("Axis4") < joypadDeadzone))
			yDeg = Input.GetAxis ("Axis4") * ySpeed * 0.08f;
        
		//Z rotation
		if (Input.GetKey ("joystick button 7"))
			zDeg = Time.deltaTime * xSpeed * 0.5f;

		if (Input.GetKey ("joystick button 5"))
			zDeg = - Time.deltaTime * xSpeed * 0.5f;

		//Zoom out
		if (Input.GetKey ("joystick button 4")) {
			desiredDistance -= Time.deltaTime * zoomRate * 0.08f; 
			if (UIData.switchmode)
				Molecule3DComp.ToParticle ();
		}
		//zoom in
		if (Input.GetKey ("joystick button 6")) {
			desiredDistance += Time.deltaTime * zoomRate * 0.08f;
			if (UIData.switchmode)
				Molecule3DComp.ToParticle ();
		}
	}
	
	/// <summary>
	/// Manage the camera rotation with keyboard inputs.
	/// </summary>
	private void keyboardOperate ()
	{
		//rotation
		if (Input.GetKey (KeyCode.RightArrow) || Input.GetKey (KeyCode.Z)){
			if(monomer_jump && UIData.guided){
//				int nb_chains = MoleculeModel.existingChain.Count;
//				Dictionary<string, Vector3> chains_COM = new Dictionary<string, Vector3>();
//				for(int c = 0; i<nb_chains; i++){
//					chains_COM.Add (MoleculeModel.existingChain[c], Vector3.zero);
//				}
//				for(int i = 0; i<MoleculeModel.atomsChainList.Count; i++){
//					chains_COM.Add (MoleculeModel.atomsChainList[i], MoleculeModel.atomsLocationlist[i]);
//					if(chains_COM[MoleculeModel.atomsChainList[i]] == Vector3.zero)
//						chains_COM[MoleculeModel.atomsChainList[i]] += MoleculeModel.atomsLocationlist[i];
//					else
//						chains_COM[MoleculeModel.atomsChainList[i]] = (chains_COM[MoleculeModel.atomsChainList[i]] + MoleculeModel.atomsLocationlist[i]) / 2;
//				}
				next_right = true;
			}

			else if(UIData.guided){
				transform.RotateAround( new Vector3(0, transform.position.y, 0), new Vector3(0, -1, 0), 1.0f);
				checkangle = false;
			}

			else
				upxDeg ();
		}
        
		//rotation
		if (Input.GetKey (KeyCode.LeftArrow) || Input.GetKey (KeyCode.X)) {
			if(monomer_jump && UIData.guided){
				next_left = true;
			}

			else if(UIData.guided){
				transform.RotateAround( new Vector3(0, transform.position.y, 0), new Vector3(0, 1, 0), 1.0f);
				checkangle = false;
			}

			else
				downxDeg ();
		}

		//rotation
		if (Input.GetKey (KeyCode.UpArrow) || Input.GetKey (KeyCode.E)){
			if(UIData.guided){
				if(panoramic){
					Vector3 v = transform.position;
					transform.eulerAngles = new Vector3 (0, transform.eulerAngles.y, 0);
					v.y += 0.5f;
					transform.position = v;
				}
				else{
					goUpConstrained();}
			}
			else
				upyDeg ();
		}

		//rotation
		if (Input.GetKey (KeyCode.DownArrow) || Input.GetKey (KeyCode.Q)){
			if(UIData.guided){
				if(panoramic){
					Vector3 v = transform.position;
					transform.eulerAngles = new Vector3 (0, transform.eulerAngles.y, 0);
					v.y -= 0.5f;
					transform.position = v;
				}
				else{
					goDownConstrained();}
			}
			 
			else
				downyDeg ();
		}

		///////////////////   SPREADING   ////////////////////

		if (Input.GetKey (KeyCode.KeypadPlus) || Input.GetKey (KeyCode.B)) 
			Spreading();

		if (Input.GetKey (KeyCode.KeypadMinus) || Input.GetKey(KeyCode.V)) 
			Narrowing();

		if (Input.GetKey (KeyCode.KeypadMultiply) || Input.GetKey(KeyCode.T)) 
			ResetChainsPos();

	} // End of KeyboardOperate
	
	/// <summary>
	/// Compute center of mass.
	/// </summary>
	private Vector3 get_com(string chain)
	{
		float x = 0, y = 0, z = 0;
		float nb_atoms = 0;
		int first_atom = 0;

		if (onlyTMD)
			first_atom = reslim;

		if (UIData.secondarystruct) {
			for (int i=0; i < MoleculeModel.CaSplineList.Count; i++) {
				if(MoleculeModel.CaSplineChainList[i]== chain){
					x += MoleculeModel.CaSplineList[i][0];
					y += MoleculeModel.CaSplineList[i][1];
					z += MoleculeModel.CaSplineList[i][2];
					nb_atoms++;
				}
			}
			return new Vector3(x/nb_atoms, y/nb_atoms, z/nb_atoms);
		}

		else{
		for (int i=first_atom; i < MoleculeModel.atomsLocationlist.Count; i++) {
			if(MoleculeModel.atomsChainList[i]== chain){
				x += MoleculeModel.atomsLocationlist[i][0];
				y += MoleculeModel.atomsLocationlist[i][1];
				z += MoleculeModel.atomsLocationlist[i][2];
				nb_atoms++;
			}
		}
		return new Vector3(x/nb_atoms, y/nb_atoms, z/nb_atoms);
		}
	} // End get_com

	/// <summary>
	/// Compute spreading vectors (first step).
	/// </summary>
	private void computeSpreadingVector()
	{
		Vector3 sym_center;
		if(MoleculeModel.MinValue.y < 0)
			sym_center = new Vector3(0, (MoleculeModel.MaxValue.y + MoleculeModel.MinValue.y), 0);
		else
			sym_center = new Vector3(0, (MoleculeModel.MaxValue.y - MoleculeModel.MinValue.y), 0);
		
		Vector3 temp = get_com("A");
		spreadA = temp - sym_center;
		spreadA.Normalize();
		
		temp = get_com("B");
		spreadB = temp - sym_center;
		spreadB.Normalize();
		
		temp = get_com("C");
		spreadC = temp - sym_center;
		spreadC.Normalize();
		
		temp = get_com("D");
		spreadD = temp - sym_center;
		spreadD.Normalize();
		
		temp = get_com("E");
		spreadE = temp - sym_center;
		spreadE.Normalize();
	}

	/// <summary>
	/// Compute spreading vectors (second step).
	/// </summary>
	private void computeSpreadPart(out Vector3 Spread1, out Vector3 Spread2, string chain){

		float x1 = 0, y1 = 0, z1 = 0;
		float x2 = 0, y2 = 0, z2 = 0;
		float nb_atoms1 = 0;
		float nb_atoms2 = 0;

		Vector3 sym_center;
		if(MoleculeModel.MinValue.y < 0)
			sym_center = new Vector3(0, (MoleculeModel.MaxValue.y + MoleculeModel.MinValue.y), 0);
		else
			sym_center = new Vector3(0, (MoleculeModel.MaxValue.y - MoleculeModel.MinValue.y), 0);

		for (int i=0; i <  MoleculeModel.atomsLocationlist.Count; i++) {
			if(MoleculeModel.atomsChainList[i]== chain){
				if(MoleculeModel.residueIds[i] < reslim){
					x1 += MoleculeModel.atomsLocationlist[i][0];
					y1 += MoleculeModel.atomsLocationlist[i][1];
					z1 += MoleculeModel.atomsLocationlist[i][2];
					nb_atoms1++;
				}else{
					x2 += MoleculeModel.atomsLocationlist[i][0];
					y2 += MoleculeModel.atomsLocationlist[i][1];
					z2 += MoleculeModel.atomsLocationlist[i][2];
					nb_atoms2++;
				}
			}
		}
		Vector3 temp1 = new Vector3 (x1 / nb_atoms1, y1 / nb_atoms1, z1 / nb_atoms1);
		Spread1 = temp1 - sym_center;
		Spread1.Normalize ();

		Vector3 temp2 = new Vector3 (x2 / nb_atoms2, y2 / nb_atoms2, z2 / nb_atoms2);
		Spread2 = temp2 - sym_center;
		Spread2.Normalize ();

	} //End ComputeSpreadPart

	/// <summary>
	/// Computes spreading vectors for helices (third step spreading)
	/// </summary>
	/// <param name="Spreadhelix">Spreadhelix.</param>
	/// <param name="first_atom">First_atom.</param>
	/// <param name="last_atom">Last_atom.</param>
	/// <param name="chain">Chain.</param>
	private void computeSpreadHelix(out Vector3 Spreadhelix, int first_atom, int last_atom, string chain){
		float x = 0, y = 0, z = 0;
		float nb_atoms = 0;

		for (int i=first_atom; i < last_atom; i++) {
			x += MoleculeModel.atomsLocationlist[i][0];
			y += MoleculeModel.atomsLocationlist[i][1];
			z += MoleculeModel.atomsLocationlist[i][2];
			nb_atoms++;
		}

		Vector3 temp = new Vector3 (x / nb_atoms, y / nb_atoms, z / nb_atoms);
		onlyTMD = true;
		Vector3 center = get_com (chain);
		onlyTMD = false;
		Spreadhelix = temp - center;
		Spreadhelix.Normalize ();

	}

	/// <summary>
	/// Spreading chains (limit: 5 chains)
	/// </summary>
	private void Spreading(){
		computeSpreadingVector();
		comp_spread+=0.1f;
		comp_spread = Math.Round (comp_spread, 1);
		
		if(UIData.isGLIC){
			//Second step spreading
			computeSpreadPart(out spreadA1, out spreadA2, "A");
			computeSpreadPart(out spreadB1, out spreadB2, "B");
			computeSpreadPart(out spreadC1, out spreadC2, "C");
			computeSpreadPart(out spreadD1, out spreadD2, "D");
			computeSpreadPart(out spreadE1, out spreadE2, "E");
		}
		
		if(GUIMoleculeController.toggle_SEC_STRUCT){
			
			GameObject[] ribA = GameObject.FindGameObjectsWithTag("RibbonObjA");
			GameObject[] ribB = GameObject.FindGameObjectsWithTag("RibbonObjB");
			GameObject[] ribC = GameObject.FindGameObjectsWithTag("RibbonObjC");
			GameObject[] ribD = GameObject.FindGameObjectsWithTag("RibbonObjD");
			GameObject[] ribE = GameObject.FindGameObjectsWithTag("RibbonObjE");
			
			if(comp_spread > 2 && UIData.isGLIC){
				
				if(UIData.guided){
					ribA[0].transform.Translate(spreadE1);
					ribA[1].transform.Translate (spreadE2);
					ribB[0].transform.Translate(spreadD1);
					ribB[1].transform.Translate (spreadD2);
					ribC[0].transform.Translate(spreadC1);
					ribC[1].transform.Translate (spreadC2);
					ribD[0].transform.Translate(spreadB1);
					ribD[1].transform.Translate (spreadB2);
					ribE[0].transform.Translate(spreadA1);
					ribE[1].transform.Translate (spreadA2);
				}else{
					ribA[0].transform.Translate(spreadA1);
					ribA[1].transform.Translate (spreadA2);
					ribB[0].transform.Translate(spreadB1);
					ribB[1].transform.Translate (spreadB2);
					ribC[0].transform.Translate(spreadC1);
					ribC[1].transform.Translate (spreadC2);
					ribD[0].transform.Translate(spreadD1);
					ribD[1].transform.Translate (spreadD2);
					ribE[0].transform.Translate(spreadE1);
					ribE[1].transform.Translate (spreadE2);
				}
			}else{
				foreach( GameObject rib in ribA)
					// Dunno why but the vectors are reversed when guided navigation is activated
					if(UIData.guided)
						rib.transform.Translate(spreadE);
					else
						rib.transform.Translate(spreadA);
				
				foreach( GameObject rib in ribB)
					if(UIData.guided)
						rib.transform.Translate(spreadD);
					else
						rib.transform.Translate(spreadB);
				
				foreach( GameObject rib in ribC)
					rib.transform.Translate(spreadC);
				
				foreach( GameObject rib in ribD)
					if(UIData.guided)
						rib.transform.Translate(spreadB);
					else
						rib.transform.Translate(spreadD);
				
				foreach( GameObject rib in ribE)
					if(UIData.guided)
						rib.transform.Translate(spreadA);
					else
						rib.transform.Translate(spreadE);
			}
		}
		
		if(UIData.atomtype == UIData.AtomType.hyperball){
			hballs = GameObject.FindObjectsOfType(typeof(BallUpdateHB)) as BallUpdateHB[];
			Molecule.View.DisplayMolecule.DestroyBondObject();
			for (int i=0; i<hballs.Length; i++) {
				
				if(UIData.secondarystruct){
					if(MoleculeModel.CaSplineChainList[(int)hballs[i].number] == "A")
						hballs[i].transform.Translate(spreadA);
					if(MoleculeModel.CaSplineChainList[(int)hballs[i].number] == "B")
						hballs[i].transform.Translate(spreadB);
					if(MoleculeModel.CaSplineChainList[(int)hballs[i].number] == "C")
						hballs[i].transform.Translate(spreadC);
					if(MoleculeModel.CaSplineChainList[(int)hballs[i].number] == "D")
						hballs[i].transform.Translate(spreadD);
					if(MoleculeModel.CaSplineChainList[(int)hballs[i].number] == "E")
						hballs[i].transform.Translate(spreadE);
				}else{ 
					if(MoleculeModel.atomsChainList[(int)hballs[i].number] == "A")
						hballs[i].transform.Translate(spreadA);
					if(MoleculeModel.atomsChainList[(int)hballs[i].number] == "B")
						hballs[i].transform.Translate(spreadB);
					if(MoleculeModel.atomsChainList[(int)hballs[i].number] == "C")
						hballs[i].transform.Translate(spreadC);
					if(MoleculeModel.atomsChainList[(int)hballs[i].number] == "D")
						hballs[i].transform.Translate(spreadD);
					if(MoleculeModel.atomsChainList[(int)hballs[i].number] == "E")
						hballs[i].transform.Translate(spreadE);
				}
				UIData.resetBondDisplay = true;
			}
			UIData.resetDisplay = true;
		}
		
		if(UIData.atomtype == UIData.AtomType.sphere){
			sballs = GameObject.FindObjectsOfType(typeof(BallUpdateSphere)) as BallUpdateSphere[];
			Molecule.View.DisplayMolecule.DestroyBondObject();
			if(comp_spread<2){
				for (int i=0; i<sballs.Length; i++) {
					if(MoleculeModel.atomsChainList[(int)sballs[i].number] == "A")
						sballs[i].transform.Translate(spreadA);
					if(MoleculeModel.atomsChainList[(int)sballs[i].number] == "B")
						sballs[i].transform.Translate(spreadB);
					if(MoleculeModel.atomsChainList[(int)sballs[i].number] == "C")
						sballs[i].transform.Translate(spreadC);
					if(MoleculeModel.atomsChainList[(int)sballs[i].number] == "D")
						sballs[i].transform.Translate(spreadD);
					if(MoleculeModel.atomsChainList[(int)sballs[i].number] == "E")
						sballs[i].transform.Translate(spreadE);
				}
			}else{
				for (int i=0; i<sballs.Length; i++) {
					if(MoleculeModel.atomsChainList[(int)sballs[i].number] == "A"){
						if(MoleculeModel.residueIds[(int)sballs[i].number]<reslim)
							sballs[i].transform.Translate(spreadA1);
						else
							sballs[i].transform.Translate(spreadA2);
					}
					if(MoleculeModel.atomsChainList[(int)sballs[i].number] == "B"){
						if(MoleculeModel.residueIds[(int)sballs[i].number]<reslim)
							sballs[i].transform.Translate(spreadB1);
						else
							sballs[i].transform.Translate(spreadB2);
					}
					if(MoleculeModel.atomsChainList[(int)sballs[i].number] == "C"){
						if(MoleculeModel.residueIds[(int)sballs[i].number]<reslim)
							sballs[i].transform.Translate(spreadC1);
						else
							sballs[i].transform.Translate(spreadC2);
					}
					if(MoleculeModel.atomsChainList[(int)sballs[i].number] == "D"){
						if(MoleculeModel.residueIds[(int)sballs[i].number]<reslim)
							sballs[i].transform.Translate(spreadD1);
						else
							sballs[i].transform.Translate(spreadD2);
					}
					if(MoleculeModel.atomsChainList[(int)sballs[i].number] == "E"){
						if(MoleculeModel.residueIds[(int)sballs[i].number]<reslim)
							sballs[i].transform.Translate(spreadE1);
						else
							sballs[i].transform.Translate(spreadE2);
					}
				}
			}
		} // End if Sphere
	}// End Spreading

	/// <summary>
	/// Narrowing chains.
	/// </summary>
	private void Narrowing(){
		computeSpreadingVector();
		comp_spread-=0.1f;
		comp_spread = Math.Round ((double)comp_spread, 1);
		
		if(comp_spread <= 0){
			comp_spread = 0;
		}else{
			
			if(GUIMoleculeController.toggle_SEC_STRUCT){
				
				GameObject[] ribA = GameObject.FindGameObjectsWithTag("RibbonObjA");
				GameObject[] ribB = GameObject.FindGameObjectsWithTag("RibbonObjB");
				GameObject[] ribC = GameObject.FindGameObjectsWithTag("RibbonObjC");
				GameObject[] ribD = GameObject.FindGameObjectsWithTag("RibbonObjD");
				GameObject[] ribE = GameObject.FindGameObjectsWithTag("RibbonObjE");
				
				if(comp_spread > 2 && UIData.isGLIC){
					
					if(UIData.guided){
						ribA[0].transform.Translate(-spreadE1);
						ribA[1].transform.Translate (-spreadE2);
						ribB[0].transform.Translate(-spreadD1);
						ribB[1].transform.Translate (-spreadD2);
						ribC[0].transform.Translate(-spreadC1);
						ribC[1].transform.Translate (-spreadC2);
						ribD[0].transform.Translate(-spreadB1);
						ribD[1].transform.Translate (-spreadB2);
						ribE[0].transform.Translate(-spreadA1);
						ribE[1].transform.Translate (-spreadA2);
					}else{
						ribA[0].transform.Translate(-spreadA1);
						ribA[1].transform.Translate (-spreadA2);
						ribB[0].transform.Translate(-spreadB1);
						ribB[1].transform.Translate (-spreadB2);
						ribC[0].transform.Translate(-spreadC1);
						ribC[1].transform.Translate (-spreadC2);
						ribD[0].transform.Translate(-spreadD1);
						ribD[1].transform.Translate (-spreadD2);
						ribE[0].transform.Translate(-spreadE1);
						ribE[1].transform.Translate (-spreadE2);
					}
				}else{
					foreach( GameObject rib in ribA)
						if(UIData.guided)
							rib.transform.Translate(-spreadE);
						else
							rib.transform.Translate(-spreadA);
					
					foreach( GameObject rib in ribB)
						if(UIData.guided)
							rib.transform.Translate(-spreadD);
						else
							rib.transform.Translate(-spreadB);
					
					foreach( GameObject rib in ribC)
						rib.transform.Translate(-spreadC);
					
					foreach( GameObject rib in ribD)
						if(UIData.guided)
							rib.transform.Translate(-spreadB);
						else
							rib.transform.Translate(-spreadD);
					
					foreach( GameObject rib in ribE)
						if(UIData.guided)
							rib.transform.Translate(-spreadA);
						else
							rib.transform.Translate(-spreadE);
				}
			}
			
			if(UIData.atomtype == UIData.AtomType.hyperball){
				hballs = GameObject.FindObjectsOfType(typeof(BallUpdateHB)) as BallUpdateHB[]; 
				Molecule.View.DisplayMolecule.DestroyBondObject();
				for (int i=0; i<hballs.Length; i++) {
					
					if(comp_spread <= 0){
						comp_spread = 0;
						break;
					}else{ 
						if(UIData.secondarystruct){
							if(MoleculeModel.CaSplineChainList[(int)hballs[i].number] == "A")
								hballs[i].transform.Translate(-spreadA);
							if(MoleculeModel.CaSplineChainList[(int)hballs[i].number] == "B")
								hballs[i].transform.Translate(-spreadB);
							if(MoleculeModel.CaSplineChainList[(int)hballs[i].number] == "C")
								hballs[i].transform.Translate(-spreadC);
							if(MoleculeModel.CaSplineChainList[(int)hballs[i].number] == "D")
								hballs[i].transform.Translate(-spreadD);
							if(MoleculeModel.CaSplineChainList[(int)hballs[i].number] == "E")
								hballs[i].transform.Translate(-spreadE);
						}else{ 
							if(MoleculeModel.atomsChainList[(int)hballs[i].number] == "A")
								hballs[i].transform.Translate(-spreadA);
							if(MoleculeModel.atomsChainList[(int)hballs[i].number] == "B")
								hballs[i].transform.Translate(-spreadB);
							if(MoleculeModel.atomsChainList[(int)hballs[i].number] == "C")
								hballs[i].transform.Translate(-spreadC);
							if(MoleculeModel.atomsChainList[(int)hballs[i].number] == "D")
								hballs[i].transform.Translate(-spreadD);
							if(MoleculeModel.atomsChainList[(int)hballs[i].number] == "E")
								hballs[i].transform.Translate(-spreadE);
						}
						UIData.resetBondDisplay = true;
					}
					UIData.resetDisplay = true;
				}
			} // End if Hyperballs
			
			if(UIData.atomtype == UIData.AtomType.sphere){
				sballs = GameObject.FindObjectsOfType(typeof(BallUpdateSphere)) as BallUpdateSphere[];
				for (int i=0; i<sballs.Length; i++) {
					
					if(comp_spread <= 0){
						comp_spread = 0;
						break;
					}else{
						
						if(MoleculeModel.atomsChainList[(int)sballs[i].number] == "A")
							sballs[i].transform.Translate(-spreadA);
						if(MoleculeModel.atomsChainList[(int)sballs[i].number] == "B")
							sballs[i].transform.Translate(-spreadB);
						if(MoleculeModel.atomsChainList[(int)sballs[i].number] == "C")
							sballs[i].transform.Translate(-spreadC);
						if(MoleculeModel.atomsChainList[(int)sballs[i].number] == "D")
							sballs[i].transform.Translate(-spreadD);
						if(MoleculeModel.atomsChainList[(int)sballs[i].number] == "E")
							sballs[i].transform.Translate(-spreadE);
					}
				}
			} // End if Sphere
		}
	} // End Narrowing

	/// <summary>
	/// Resets the chains position.
	/// </summary>
	private void ResetChainsPos(){
		Vector3 vectemp;
		comp_spread = 0;
		if(UIData.atomtype == UIData.AtomType.hyperball){
			hballs = GameObject.FindObjectsOfType(typeof(BallUpdateHB)) as BallUpdateHB[];
			for (int i=0; i<hballs.Length; i++) {
				if(UIData.secondarystruct){
					//comp_spread = 0;
					if(hballs[i].transform.position.x == MoleculeModel.CaSplineList[(int)hballs[i].number][0] && 
					   hballs[i].transform.position.y == MoleculeModel.CaSplineList[(int)hballs[i].number][1] && 
					   hballs[i].transform.position.z == MoleculeModel.CaSplineList[(int)hballs[i].number][2])
						break;
					else{
						
						vectemp = new Vector3();
						vectemp.x = MoleculeModel.CaSplineList[(int)hballs[i].number][0];
						vectemp.y = MoleculeModel.CaSplineList[(int)hballs[i].number][1];
						vectemp.z = MoleculeModel.CaSplineList[(int)hballs[i].number][2];
						
						hballs[i].transform.position = vectemp;
					}
					
				}
				else{
					if(hballs[i].transform.position.x == MoleculeModel.atomsLocationlist[(int)hballs[i].number][0] && 
					   hballs[i].transform.position.y == MoleculeModel.atomsLocationlist[(int)hballs[i].number][1] && 
					   hballs[i].transform.position.z == MoleculeModel.atomsLocationlist[(int)hballs[i].number][2])
						break;
					else{
						
						vectemp = new Vector3();
						vectemp.x = MoleculeModel.atomsLocationlist[(int)hballs[i].number][0];
						vectemp.y = MoleculeModel.atomsLocationlist[(int)hballs[i].number][1];
						vectemp.z = MoleculeModel.atomsLocationlist[(int)hballs[i].number][2];
						
						hballs[i].transform.position = vectemp;
					}
				}
			}
		} //End if hyperballs
		
		if(UIData.atomtype == UIData.AtomType.sphere){
			sballs = GameObject.FindObjectsOfType(typeof(BallUpdateSphere)) as BallUpdateSphere[];
			for (int i=0; i<sballs.Length; i++) {
				
				if(sballs[i].transform.position.x == MoleculeModel.atomsLocationlist[(int)sballs[i].number][0] && 
				   sballs[i].transform.position.y == MoleculeModel.atomsLocationlist[(int)sballs[i].number][1] && 
				   sballs[i].transform.position.z == MoleculeModel.atomsLocationlist[(int)sballs[i].number][2])
					break;
				else{
					vectemp = new Vector3();
					vectemp.x = MoleculeModel.atomsLocationlist[(int)sballs[i].number][0];
					vectemp.y = MoleculeModel.atomsLocationlist[(int)sballs[i].number][1];
					vectemp.z = MoleculeModel.atomsLocationlist[(int)sballs[i].number][2];
					
					sballs[i].transform.position = vectemp;
				}			
			}
		} //End if Sphere
		
		if(GUIMoleculeController.toggle_SEC_STRUCT){
			GameObject[] Objs = FindObjectsOfType(typeof(GameObject)) as GameObject[];
			foreach(GameObject ribObj in Objs){
				if(ribObj.name == "Ribbons")
					GameObject.Destroy(ribObj);
			}
			Ribbons ribbons = new Ribbons();
			ribbons.CreateRibbons();
		}
		
		UIData.resetBondDisplay = true;
	}// End ResetChainsPos

	/// <summary>
	/// Slightly spreads the chains when the camera is near the protein structure.
	/// </summary>
	private void NearSpreading(){
		UIData.spread_tree = true;
		if(rep == ""){
			atomtree = AtomTree.Build();
		}
		rep = atomtree.GetClosestAtomType(transform.position);
		closeatom = MoleculeModel.atomsLocationlist[int.Parse(rep)];
		poscloseatom = new Vector3(closeatom[0], closeatom[1], closeatom[2]);
		distclose = Vector3.Distance (transform.position, poscloseatom); // Distance Camera / Closest atom;
		if((distclose < 15) && comp_spread < 0.3)
			Spreading ();
		else if ((distclose > 15) && comp_spread != 0)
			ResetChainsPos ();
		UIData.spread_tree = false;
	}
	
}
