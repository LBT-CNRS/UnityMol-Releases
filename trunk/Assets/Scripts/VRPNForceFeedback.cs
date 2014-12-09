using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class VRPNForceFeedback : VRPNRemote {
	[DllImport ("UnityVRPN")]
	private static extern void VRPNForceFeedbackInit(string name);
	
	[DllImport ("UnityVRPN")]
	private static extern void VRPNForceFeedbackSetForce(float force_x, float force_y, float force_z);

	private VRPNManager manager;
	
	public float gaussianMean = 1.8f;
	public float gaussianDeviation = 0.8f;
	public float magnetFeedbackScale = 0.7f;
	
	public float linearForceFeedbackFactor = 6.0f;
	
	void Start()
	{
		deviceName = "Omni@localhost:3884";
	}
	
	protected override void initializeReports()
	{
	}
	
	protected override bool startDevice()
	{
		Debug.Log (manager != null);
		Debug.Log (manager.isInitialized());
		Debug.Log (deviceName != null);
		if (manager && (true || manager.isInitialized()) && deviceName != null)
		{
			Debug.Log ("::VRPNForceFeedback:: Initializing");
			// Register Force Feedback Device
			VRPNForceFeedbackInit(deviceName);
			
			initialized = true;
			return true;
		}
		return false;
	}
	
	private float Gaussian(float distance, float mean, float deviation) {
		return Mathf.Exp(-((Mathf.Pow ((distance - mean) / deviation, 2))) / 2);
	}
	
	// Magnetic force
	public void setForceForAtomPosition(Vector3 atomPosition) 
	{
		if (!initialized) return;
		
		// Debug.Log ("setForceForAtomPosition");
		
		// Compute the distance between the atom and the picker for each axis
		Vector3 forceFactor = (atomPosition - transform.position);
		
		// Compute the absolute distance between the atom and the picker
		float distance = Mathf.Sqrt (Mathf.Pow (forceFactor.x, 2) + Mathf.Pow (forceFactor.y, 2) + Mathf.Pow (forceFactor.z, 2));
		
		// Compute a gaussian factor
		float gaussian = Gaussian(distance, gaussianMean, gaussianDeviation);
		
		VRPNPickerController pickerScript = (VRPNPickerController) gameObject.GetComponent<VRPNPickerController>();
		Vector3 trackerVelocity = new Vector3(1.0f, 1.0f, 1.0f);
		if (pickerScript != null)
		{
			trackerVelocity = pickerScript.getTrackerVelocity();
		}
		
		forceFactor /= distance;
		forceFactor *= gaussian * magnetFeedbackScale;
		
		Vector3 feedbackForce = GameObject.FindGameObjectWithTag("LoadBox").transform.worldToLocalMatrix * forceFactor;
		
		feedbackForce.x -= trackerVelocity.x;
		feedbackForce.y -= trackerVelocity.y;
		feedbackForce.z -= trackerVelocity.z;

		// Send the desired feedback to server
		// z axis is inverted
		VRPNForceFeedbackSetForce(feedbackForce.x, feedbackForce.y, -feedbackForce.z);
	}
	
	public void setLinearForceForVector(Vector3 v) {
		if (!initialized) return;
		
		Vector3 nv = GameObject.FindGameObjectWithTag("LoadBox").transform.worldToLocalMatrix * v;
		float distance = Mathf.Sqrt (Mathf.Pow (nv.x, 2) + Mathf.Pow (nv.y, 2) + Mathf.Pow (nv.z, 2));
		nv.Normalize();
//		nv *=  Mathf.Sqrt(distance);
		nv *= 1 / Mathf.Pow(1 + Mathf.Exp(-(distance - 4)), 2);
		nv *= linearForceFeedbackFactor;
//		Debug.Log (nv);
//		VRPNPickerController pickerScript = (VRPNPickerController) gameObject.GetComponent<VRPNPickerController>();
//		Vector3 trackerVelocity = new Vector3(1.0f, 1.0f, 1.0f);
//		if (pickerScript != null)
//		{
//			trackerVelocity = pickerScript.getTrackerVelocity();
//		}
//		
//		nv.x *= trackerVelocity.x;
//		nv.y *= trackerVelocity.y;
//		nv.z *= -trackerVelocity.z;
		
//		nv = Vector3.ClampMagnitude(nv, 2.0f);
		
		VRPNForceFeedbackSetForce (nv.x, nv.y, -nv.z);
	}
	
	public void resetForce()
	{
		if (!initialized) return;
		VRPNForceFeedbackSetForce(0.0f, 0.0f, 0.0f);
	}
	
	public void setDeviceName(string dname) {
		Debug.Log ("::VRPNForceFeedback:: Setting device name");
		if (!initialized) {
			deviceName = dname;
		}
	}
	
	public void setManager (VRPNManager m) {
		Debug.Log ("::VRPNForceFeedback:: Setting Manager");
		manager = m;
	}
	
	public bool startDeviceInLib() {
		return startDevice();
	}
}
