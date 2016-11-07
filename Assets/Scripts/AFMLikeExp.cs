/// @file ClickAtom.cs
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
/// $Id: ClickAtom.cs 415 2014-04-10 13:58:47Z roudier $
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
using System.Collections;
using System.Collections.Generic;
using UI;
using Molecule.Model;
using System;
using System.IO;

public class AFMLikeExp : MonoBehaviour 
{
	public float HALO_SELECTED_ALPHA = 0.3f;
	public float HALO_UNSELECTED_ALPHA = 0.2f;

	// Those two handle selection 
	private List<GameObject> atomList = new List<GameObject>();
	private List<GameObject> haloList = new List<GameObject>();
	private List<GameObject> arrowList = new List<GameObject>();
	private List<int> atomNumberList = new List<int>();
	
	// State variable for selection after button event
	private bool selectCurrentAtom = false;
	private GameObject currentAtom;
	private GameObject currentHalo;

	// Distance threshold for ray casting
	public float distance = 12.0f;
	
	// Seemed necessary to initialize the button controller reference
	private bool listen = false;
	
	VRPNForceFeedback ffScript;
	
	DateTime lastPressedTime;
	bool applyForce = false;
	bool forceApplied = false;
	bool checkTime = false;
	
	MDDriver mddriverScript;
	
	
	/// <summary>
	/// Button event handler
	/// </summary>
	/// <param name="buttonNumber">Button number.</param>
	/// <param name="buttonState">Button state (1 for pressed, 0 for released).</param>
	void OnVRPNButtonEvent(VRPNButtonController.VRPNButtonReport report) {
		if(report.button == 0 && report.state == 1) {
			lastPressedTime = System.DateTime.Now;
			checkTime = true;
		}
		if (report.button == 0 && report.state == 0) {
			double diffSecs = System.DateTime.Now.Subtract (lastPressedTime).TotalSeconds;
			if (diffSecs < 1.0f) {
				selectCurrentAtom = true;
			}
			checkTime = false;
			applyForce = false;
		}
	}
	
	void Start() {
		Set(MoleculeModel.atoms[0] as GameObject);
		Select();
		Set(MoleculeModel.atoms[296] as GameObject);
		Select();
	}
	
	void Update () {
		if (GUIMoleculeController.toggle_MDDRIVER && mddriverScript == null)
		{
			mddriverScript = GameObject.FindObjectOfType<MDDriver>();
		}
	
		if (!listen) {
			VRPNButtonController btn_ctrl = gameObject.GetComponent<VRPNButtonController>() as VRPNButtonController;
			if (btn_ctrl != null) {
				VRPNButtonController.OnButton += OnVRPNButtonEvent;
				listen = true;
			}
		}
		
		ffScript = gameObject.GetComponent<VRPNForceFeedback>() as VRPNForceFeedback;
		
		if (checkTime) {
			double diffSecs = System.DateTime.Now.Subtract (lastPressedTime).TotalSeconds;
			if (diffSecs >= 1.0f) {
				applyForce = true;
				checkTime = false;
			}
		}
		
//		Vector3 direction = -transform.forward;
		
		GameObject closestAtom = MoleculeModel.atoms[0] as GameObject;
		float minDist = Vector3.Distance(closestAtom.transform.position, transform.position);
		//Find closest atom
		for (int i = 1; i < MoleculeModel.atoms.Count; i++) {
			GameObject go = MoleculeModel.atoms[i] as GameObject;
			float dist = Vector3.Distance(go.transform.position, transform.position);
			if (dist < minDist) {
				minDist = dist;
				closestAtom = go;
			}
		}
		
		float[] forces = new float[atomNumberList.Count * 3];
		
		Vector3 diff = atomList[0].transform.position - atomList[1].transform.position;
		Vector3 currentForce = diff.normalized;
		Vector3 clampedCurrentForce = currentForce * 5.0f;
		forces[0] = clampedCurrentForce.x;
		forces[1] = clampedCurrentForce.y;
		forces[2] = clampedCurrentForce.z;
		
		forces[3] = -clampedCurrentForce.x;
		forces[4] = -clampedCurrentForce.y;
		forces[5] = -clampedCurrentForce.z;
		
		string filename = "umol_2K96_data.txt";
		StreamWriter fpsLog = new StreamWriter(filename, true);
		fpsLog.WriteLine(clampedCurrentForce.x + "\t" + clampedCurrentForce.y + "\t" + clampedCurrentForce.z + "\t" + Vector3.Distance(atomList[0].transform.position, atomList[1].transform.position));
		fpsLog.Close();
		fpsLog.Dispose();
		
		mddriverScript.applyForces(atomNumberList.ToArray(), forces);
		
		for (int i = 0; i < arrowList.Count; i++)
		{
			arrowList[i].transform.up = diff;
			arrowList[i].transform.localScale = new Vector3(1.0f, 10.0f, 1.0f);
		}
		
		if (applyForce)
		{
			if (atomList.Count > 0) {
				Vector3 barycenter = (atomList[0].transform.position + atomList[1].transform.position) / 2;
				diff = transform.position - barycenter;
				float distance = diff.magnitude;
//				Vector3 d2 = GameObject.FindGameObjectWithTag("LoadBox").transform.worldToLocalMatrix * diff;
//				
				// Interactive mode
//				for (int i = 0; i < atomList.Count; i++) {
//					if (atomList[i].rigidbody != null) {
//						atomList[i].rigidbody.AddForce(diff, ForceMode.Impulse);
//					}
//				}

				ffScript.setLinearForceForVector(-diff);
				if (GUIMoleculeController.toggle_MDDRIVER && mddriverScript != null)
				{
					forces = new float[atomNumberList.Count * 3];
					
					diff = atomList[0].transform.position - atomList[1].transform.position;
					currentForce = diff.normalized;
//					currentForce *= Mathf.Sqrt(distance);
					currentForce *= 1 / Mathf.Pow((1 + 0.5f * Mathf.Exp(-3*distance)), 2);
//					currentForce *= 1 / Mathf.Pow(1 + Mathf.Exp(-(distance - 4)), 2);
					
					//Vector3 clampedCurrentForce = currentForce * distance / 4.0f;
					clampedCurrentForce = currentForce * 10.0f;
					
					for (int i = 0; i < atomNumberList.Count; i++)
					{
						forces[i] = clampedCurrentForce.x;
						forces[i+1] = clampedCurrentForce.y;
						forces[i+2] = clampedCurrentForce.z;
					}

//					forces[0] = clampedCurrentForce.x;
//					forces[1] = clampedCurrentForce.y;
//					forces[2] = clampedCurrentForce.z;
//					
//					forces[3] = -clampedCurrentForce.x;
//					forces[4] = -clampedCurrentForce.y;
//					forces[5] = -clampedCurrentForce.z;
					
//					filename = "umol_2K96_data.txt";
//					fpsLog = new StreamWriter(filename, true);
//					fpsLog.WriteLine(clampedCurrentForce.x + "\t" + clampedCurrentForce.y + "\t" + clampedCurrentForce.z + "\t" + Vector3.Distance(atomList[0].transform.position, atomList[1].transform.position));
//					fpsLog.Close();
//					fpsLog.Dispose();
					
					if (forceApplied == false)
					{
						for (int i = 0; i < arrowList.Count; i++)
						{
							arrowList[i].transform.GetChild(0).gameObject.GetComponent<Renderer>().enabled = true;
						}
						forceApplied = true;
					}
					float arrowZScale = distance / 10.0f;
					float arrowScale = distance / 20.0f;
					for (int i = 0; i < arrowList.Count; i++)
					{
						arrowList[i].transform.up = diff;
						arrowList[i].transform.localScale = new Vector3(arrowScale, arrowZScale, arrowScale);
					}
					
					mddriverScript.applyForces(atomNumberList.ToArray(), forces);
				}
			}
		}
		else
		{
			if (forceApplied == true)
			{
				for (int i = 0; i < arrowList.Count; i++)
				{
					arrowList[i].transform.GetChild(0).gameObject.GetComponent<Renderer>().enabled = false;
					arrowList[i].transform.localScale = new Vector3(0.4f, 3.5f, 0.4f);
				}
				
				forces = new float[atomNumberList.Count * 3];
				for (int i = 0; i < atomNumberList.Count; i++)
				{
					forces[i] = 0.0f;
					forces[i+1] = 0.0f;
					forces[i+2] = 0.0f;
				}
				mddriverScript.applyForces(atomNumberList.ToArray(), forces);
				mddriverScript.resetForces();
				
				forceApplied = false;
			}
//			GameObject obj = CastRayFromPicker (distance);
			GameObject obj = closestAtom;
			
			// If there is no raycasted object or the object is not an atom, we reset the state variables and display
//			if (obj == null || (obj.GetComponent<BallUpdateHB>() == null)) {
			if (minDist > 8.0f)
			{
				Reset();
				ffScript.resetForce();
			}
			else {
				// The user pressed the button, so we perform the selection and set se selection variable to false
				if (selectCurrentAtom) {
					Select();
					selectCurrentAtom = false;
				}
				// Set currently pointed atom and associated halo vars
				Set(obj);
			}
			
			if (ffScript != null) {
				ffScript.setForceForAtomPosition(closestAtom.transform.position);
			}
//			Debug.Log ("Calling setForceForAtomPosition");
//			ffScript.setForceForAtomPosition(closestAtom.transform.position);
		}
	}
	
	/// <summary>
	/// Casts a ray from picker GameObject in the same direction.
	/// </summary>
	/// <returns>The casted game object</returns>
	/// <param name="distance">Distance threshold</param>
	GameObject CastRayFromPicker (float distance)
	{
		Ray ray = new Ray(transform.position - transform.forward, transform.forward);
		RaycastHit sHit;
		
		// In case the picker is directed to an atom
		if (Physics.Raycast(ray, out sHit) && sHit.distance < distance) {
			return sHit.collider.gameObject;
		}
		return (GameObject)null;
	}
	
	/// <summary>
	/// Set state variables to the pointed atom and halo
	/// </summary>
	/// <param name="atom">The pointed atom. It is a regular GameObject supposed to be an atom in the scene.</param>
	void Set(GameObject atom) {
		// Make changes when the pointed atom differ from state variables
		if (currentAtom != atom) {
			// Reset previous state
			Reset();
			// Set state to pointed object
			currentAtom = atom;
			// Two cases arise
			// When the pointed atom is in our selection list, we simply change the halo color
			// Otherwise, we instantiate a brand new white halo
			if (isAtomSelected(atom)) {
				currentHalo = haloList[atomList.IndexOf(currentAtom)];
				Color c = Color.blue;
				c.a = HALO_SELECTED_ALPHA;
				currentHalo.GetComponent<Renderer>().material.color = c;
			}
			else {
				currentHalo = CreateHaloWithParent(currentAtom);
				Color c = Color.white;
				c.a = HALO_UNSELECTED_ALPHA;
				currentHalo.GetComponent<Renderer>().material.color = c;
			}
			
			gameObject.GetComponent<Renderer>().material.color = Color.red;
		}
	}
	
	/// <summary>
	/// Clear the state variables
	/// </summary>
	void Reset() {
		if (currentAtom != null) {
			// When the atom is selected, the halo has to stay but its color has to change
			// Otherwise, the halo is simply destroyed
			if (isAtomSelected(currentAtom)) {
				Color c = Color.yellow;
				c.a = HALO_SELECTED_ALPHA;
				currentHalo.GetComponent<Renderer>().material.color = c;
			}
			else {
				GameObject.DestroyImmediate (currentHalo);
			}
			currentAtom = null;
			currentHalo = null;
			selectCurrentAtom = false;
			
			gameObject.GetComponent<Renderer>().material.color = Color.white;
		}
	}
	
	/// <summary>
	/// Select or deselect the pointed atom
	/// The pointed atom is retrieved from state variables
	/// </summary>
	void Select() {
		if (currentAtom != null) {
			// Two cases arise
			// The atom is already selected. In this case, we remove it and its associated halo from lists and set the halo color to white
			// The atom is not selected. Thus, we add it and its halo to our lists. The halo color is set to blue to denote we are pointing to an already selected atom.
			if (isAtomSelected(currentAtom)) {
				Color c = Color.white;
				c.a = HALO_UNSELECTED_ALPHA;
				currentHalo.GetComponent<Renderer>().material.color = c;
				atomList.Remove (currentAtom);
				haloList.Remove (currentHalo);
				atomNumberList.Remove ((int)currentAtom.GetComponent<BallUpdate>().number);
				GameObject arrow = currentAtom.transform.GetChild(1).gameObject;
				Debug.Log ("Removing Arrow gameObject");
				Debug.Log (arrow);
				arrowList.Remove (arrow);
				GameObject.DestroyImmediate(arrow);
			}
			else {
				atomList.Add(currentAtom);
				atomNumberList.Add((int)currentAtom.GetComponent<BallUpdate>().number);
				haloList.Add(currentHalo);
				
				GameObject arrowParent = new GameObject("Arrow");
				
				GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
				arrow.transform.localScale = new Vector3(0.4f, 3.5f, 0.4f);
				arrow.transform.parent = arrowParent.transform;
				arrow.transform.Translate(arrow.transform.up * 2.0f);
				arrow.GetComponent<Renderer>().enabled = false;
				
				arrowParent.transform.position = currentAtom.transform.position;				
				arrowParent.transform.parent = currentAtom.transform;
				arrowList.Add(arrowParent);
				
				Color c = Color.blue;
				c.a = HALO_SELECTED_ALPHA;
				currentHalo.GetComponent<Renderer>().material.color = c;
			}
		}
	}
	
	/// <summary>
	/// Is the atom in our selection list?
	/// </summary>
	/// <returns><c>true</c>, if atom is selected, <c>false</c> otherwise.</returns>
	/// <param name="atom">Atom we want to check</param>
	bool isAtomSelected(GameObject atom) {
		return (atomList.IndexOf(atom) >= 0);
	}
	
	/// <summary>
	/// Instanciate an halo sphere with the atom as parent.
	/// </summary>
	/// <returns>The halo GameObject</returns>
	/// <param name="atom">Atom we want to highlight</param>
	GameObject CreateHaloWithParent(GameObject atom)
	{		
		// Instantiate an halo to surround the provided atom and scale with the atom radius
		GameObject newHalo;
		Vector3 pos = atom.transform.position;
		float rad = atom.GetComponent<BallUpdate>().GetRealRadius();
		newHalo = Instantiate(Resources.Load("transparentsphere"), pos, new Quaternion(0f,0f,0f,0f)) as GameObject;
		Destroy(newHalo.GetComponent<SphereCollider>());
		newHalo.transform.localScale = new Vector3(rad+1,rad+1,rad+1);
		newHalo.transform.parent = atom.transform;
		return newHalo;
	}
	
	/// <summary>
	/// Clears the selection and associated game objects.
	/// </summary>
	public void ClearSelection()
	{
		foreach(GameObject halo in haloList)
			Destroy(halo);
		foreach(GameObject arrow in haloList)
			Destroy(arrow);
		atomList.Clear();
		haloList.Clear();
		arrowList.Clear();
	}
	
	void onDisable()
	{
		ClearSelection();
		mddriverScript.resetForces();
		//resetForces();
	}
	
	void OnDestroy()
	{
		// resetForces();
		
//		float[] force = new float[3] { 0.0f, 0.0f, 0.0f };
//		VRPNForceFeedbackSetForce(deviceName, force);
		
		// Remove halo if it exists
//		GameObject.Destroy (halo);
	}
}
