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
/// $Id: maxCamera.cs 213 2013-04-06 21:13:42Z baaden $
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

// $Id: maxCamera.cs 213 2013-04-06 21:13:42Z baaden $
// Filename: maxCamera.cs
//
// original: http://www.unifycommunity.com/wiki/index.php?title=MouseOrbitZoom
//
// --01-18-2010 - create temporary target, if none supplied at start

using UnityEngine;
using System.Collections;
using UI;
using Molecule.Model;

//[AddComponentMenu("Camera-Control/3dsMax Camera Style")]
public class maxCamera : MonoBehaviour
{
	public static bool cameraStop = false;
	public bool automove   = false;
	
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
    private GameObject LocCamera;
    private Molecule3D Molecule3DComp;


	//Nouvelle variable

    void Awake() { Init(); }
    void OnEnable() { Init(); }
	
    public void Init()
    {
        LoadBox = GameObject.Find("LoadBox");
        LocCamera = GameObject.Find("Camera");

        Molecule3DComp = LoadBox.GetComponent<Molecule3D>();
        //If there is no target, create a temporary target at 'distance' from the cameras current viewpoint
        if (!target)
        {
            GameObject go = new GameObject("Cam Target");
            //transform.position = go.transform.position + (transform.forward * distance);
            target = go.transform;
        }
        
        // Debug.Log("Cam distance : " + Vector3.Distance(transform.position, target.position));
        distance = Vector3.Distance(transform.position, target.position);
        currentDistance = distance;
        desiredDistance = distance;
                
        //be sure to grab the current rotations as starting points.
        position = transform.position;
        rotation = transform.rotation;
        // position = Vector3.zero;
        // rotation = Quaternion.identity;
        currentRotation = transform.rotation;
        desiredRotation = transform.rotation;
        
        xDeg = Vector3.Angle(Vector3.right, transform.right );
        yDeg = Vector3.Angle(Vector3.up, transform.up );
    }
    
	public void upxDeg(){
		xDeg = 1;
        guiControl = true;
	}

	public void upyDeg(){
		yDeg =-1;
        guiControl = true;
	}
	public void downxDeg(){
		xDeg = -1;
        guiControl = true;
	}

	public void downyDeg(){
		yDeg =1;
        guiControl = true;
	}
	
	public void downzDeg(){
		zDeg =1;
        guiControl = true;
	}

	public void upzDeg(){
		zDeg =-1;
        guiControl = true;
	}

    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Space))
    	{
            Debug.Log("Space push");
    		if(automove)
    		{
    			automove = false;
    		}
    		else
    		{
    			automove = true;
    		}
    		Molecule3DComp.toggleFPSLog();
    	}	
    }

    /*
     * Camera logic on LateUpdate to only update after all character movement logic has been handled. 
     */
    void LateUpdate()
    {
    	if(cameraStop == false)
        {
            keyboardOperate();
            if(!guiControl)
                joypadOperate();

            // If Control and Alt and Middle button? ZOOM!
            if (Input.GetMouseButton(1) )//&& Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl))
            {
            	if(UIData.switchmode) Molecule3DComp.ToParticle();
                desiredDistance -= Input.GetAxis("Mouse Y") * Time.deltaTime * zoomRate;//* Mathf.Abs(desiredDistance);
            }
            // If middle mouse and left alt are selected? ORBIT
            else if (Input.GetMouseButton(0) && !guiControl)// && Input.GetKey(KeyCode.LeftAlt))
            {
            	if(UIData.switchmode) Molecule3DComp.ToParticle();
    			if (Input.mousePosition.x<Screen.width*0.85f && Input.mousePosition.y<Screen.height*0.85f && Input.mousePosition.y>Screen.height*0.15f)
    				{	
                	xDeg = Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                	yDeg = -Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
    				}
    			else if (Input.mousePosition.x>Screen.width*0.85f ) {
    					yDeg = -Input.GetAxis("Mouse Y") * xSpeed * 0.02f;
    					xDeg = 0;
    			}else if( Input.mousePosition.y>Screen.height*0.85f ){
                		xDeg = Input.GetAxis("Mouse X") * xSpeed * 0.02f;
    					yDeg = 0;
    			}else{
    					zDeg = Input.GetAxis("Mouse X") * ySpeed * 0.02f;
    					yDeg = 0;  					
    				}

            }
            // otherwise if middle mouse is selected, we pan by way of transforming the target in screenspace
            else if (Input.GetMouseButton(2))
            {
            	if(UIData.switchmode)Molecule3DComp.ToParticle();
                Vector3 v = LocCamera.transform.localPosition;
                v.x -= Input.GetAxis("Mouse X") * panSpeed;
                v.y -= Input.GetAxis("Mouse Y") * panSpeed;
                LocCamera.transform.localPosition = v;
            }
            else
            {
                if(UIData.switchmode)
                {
                    Molecule3DComp.ToNotParticle();
                }
            }

            // affect the desired Zoom distance if we roll the scrollwheel
            desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate;// * Mathf.Abs(desiredDistance);       
    	}

        if(automove == true)
        {
            if(UIData.switchmode)Molecule3DComp.ToParticle();
            xDeg += Mathf.Lerp(0.0F, 100.0F, Time.deltaTime*0.8f);
            yDeg = 0;
        }


        //Camera rotation
        desiredRotation *= Quaternion.Euler(yDeg, xDeg, zDeg);
        currentRotation = transform.rotation;
        rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
        transform.rotation = rotation;

        //Camera movement
        currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);
        position = target.position - (rotation * Vector3.forward * currentDistance + targetOffset);
        transform.position = position;

        xDeg = 0;
        yDeg = 0;
        zDeg = 0;
        guiControl = false; 
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
    
	public void ToCenter()
	{
        LocCamera.transform.localPosition = Vector3.zero;
        target.transform.localPosition = Vector3.zero;
        transform.position = new Vector3(0,0,MoleculeModel.cameraLocation.z);
        transform.rotation = Quaternion.identity;
        Init();
	}

// control of the joypad =============================================================================================      
//                       =============================================================================================      
    private void joypadOperate()
    {
        target.rotation = transform.rotation;

        Vector3 v = LocCamera.transform.localPosition;
        if(Input.GetAxis("Axis5")>joypadDeadzone || (Input.GetAxis("Axis5")<joypadDeadzone))
            v.x -= Input.GetAxis("Axis5") * panSpeed;
        if(Input.GetAxis("Axis6")>joypadDeadzone || (Input.GetAxis("Axis6")<joypadDeadzone))
            v.y -= Input.GetAxis("Axis6") * panSpeed;
        if(Input.GetAxis("AxisX")>joypadDeadzone || (Input.GetAxis("AxisX")<joypadDeadzone))
            v.x -= Input.GetAxis("AxisX") * panSpeed * 3.0f;
        if(Input.GetAxis("AxisY")>joypadDeadzone || (Input.GetAxis("AxisY")<joypadDeadzone))
            v.y -= Input.GetAxis("AxisY") * panSpeed * 3.0f;
        LocCamera.transform.localPosition = v;

        if(Input.GetAxis("Axis3")>joypadDeadzone || (Input.GetAxis("Axis3")<joypadDeadzone))
            xDeg = Input.GetAxis("Axis3") * xSpeed * 0.08f;

        if(Input.GetAxis("Axis4")>joypadDeadzone || (Input.GetAxis("Axis4")<joypadDeadzone))
            yDeg = Input.GetAxis("Axis4") * ySpeed * 0.08f;
        
        //Z rotation
        if(Input.GetKey("joystick button 7"))
        {
            zDeg = Time.deltaTime * xSpeed * 0.5f;
        }

        if(Input.GetKey("joystick button 5"))
        {
            zDeg = - Time.deltaTime * xSpeed * 0.5f;
        }
        

        //Zoom out
        if(Input.GetKey("joystick button 4"))
        {
            desiredDistance -= Time.deltaTime * zoomRate * 0.08f; 
            if(UIData.switchmode)Molecule3DComp.ToParticle();
        }
        //zoom in
        if(Input.GetKey("joystick button 6"))
        {
            desiredDistance += Time.deltaTime * zoomRate * 0.08f;
            if(UIData.switchmode)Molecule3DComp.ToParticle();
        }

    }

    private void keyboardOperate()
    {
        //rotation
        if(Input.GetKey(KeyCode.RightArrow))
        {
            upxDeg();
        }
        
        //rotation
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            downxDeg();
        }

        //rotation
        if(Input.GetKey(KeyCode.UpArrow))
        {
            upyDeg();
        }
        
        //rotation
        if(Input.GetKey(KeyCode.DownArrow))
        {
            downyDeg();
        }
    }

}
