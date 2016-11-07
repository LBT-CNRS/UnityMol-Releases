using UnityEngine;
using System.Collections;

// DllImport
using System.Runtime.InteropServices;

/// <summary>
/// VRPN picker controller script/class.
/// </summary>
/// <description>
/// Script attached to VRPNPicker GameObject instances.
/// VRPNPicker GameObject is tagged with "VRPNPicker".
///
/// This script retrieves reports from the VRPN server.
/// Reports are filled into an array by the library/plugin. This array is passed as a pointer, made available by this controller.
/// </description>
using System; // To use IntPtr
using Molecule.Model;

public class VRPNPickerController : VRPNRemote {
	
	/*
	 * Library imports
	*/
	
	/// <summary>
	/// VRPNTrackerStart wrapper API function
	/// </summary>
	/// <param name="name">Device Name: The device name. Must map the vrpn server configuration file. example: Omni</param>
	/// <param name="deriv">Derivation: Not used in the wrapper test scene, or not investigated enough. Must be 1.</param>
	/// <description>Initialize a "tracker", a kind of client/listener, in the wrapper library (client) linked the vrpn server, for a provided <paramref name="name">device</paramref></description>
	[DllImport ("UnityVRPN")]
	private static extern void VRPNTrackerStart(string name, int deriv, int max = 1000);
	
	
	/// <summary>
	/// VRPNTrackerPosReport wrapper API function
	/// </summary>
	/// <param name="name">Device name</param>
	/// <param name="rep">Rep: report struct pointer</param>
	/// <param name="ts">Ts: </param>
	/// <param name="sensor">Sensor: </param>
	/// <description>Fill the <paramref name="rep">rep structure</paramref> with data for the given <paramref name="name">device</paramref></description>
	[DllImport ("UnityVRPN")]
	private static extern void VRPNTrackerPosReport(string name, [In,Out] IntPtr rep, [Out] IntPtr ts, int sensor = 0);
	
	/// <summary>
	/// VRPNTrackerNumPosReports wrapper API function
	/// </summary>
	/// <returns>The number of reports sent by the server</returns>
	/// <param name="name">Name: Device name</param>
	/// <description>Provides the number of reports the server sent for a given <paramref name="name">device</paramref>.</description>
	[DllImport ("UnityVRPN")]
	private static extern int VRPNTrackerNumPosReports(string name);
	
	/// <summary>
	/// VRPNTrackerPosReports wrapper API function
	/// </summary>
	/// <param name="name">Name: Device name</param>
	/// <param name="repsPtr">Reps ptr: Array of pointers to reports</param>
	/// <param name="nmbr">Nmbr: Int pointer to the total number of reports</param>
	/// <description>Fill the array pointed by <paramref name="repsPtr">rep</paramref> with server reports for the given <paramref name="name">device</paramref>. Set <paramref name="nmbr">nmbr</paramref> to the number of available reports.</description>
	[DllImport ("UnityVRPN")]
	private static extern void VRPNTrackerPosReports(string name, [In,Out] IntPtr[] repsPtr, [In,Out] ref int nmbr);
	
	[DllImport ("UnityVRPN")]
	private static extern void VRPNTrackerVelReport(string name, [In,Out] IntPtr report, IntPtr ts, int sensor = 0);
	
	/*
	 * Attributes
	 */
	
	// Report data structure from UART
	// VRPN Tracker Report Structure   
	[ StructLayout( LayoutKind.Sequential, Pack=1 )] // set Pack=0 for Windows and Pack=1 for OSX
	public struct VRPNReport
	{
		public VRPNManager.TimeVal msg_time;
		public int sensor;
		[ MarshalAs( UnmanagedType.ByValArray, SizeConst=3 )]
		public double[] pos;
		[ MarshalAs( UnmanagedType.ByValArray, SizeConst=4 )]
		public double[] quat;
	}
	
	private IntPtr velReport;
	
	private Vector3 trackerPos = Vector3.zero;
	private Quaternion trackerQuat = Quaternion.identity;
	
	private Vector3 trackerVelocity = Vector3.zero;
	
	private VRPNManager manager;
	
	/// <summary>
	/// The main camera
	/// </summary>
	/// <description>Stores a reference to the main camera GameOBject</description>
	private GameObject camGO;
	
	GameObject cube;
	
	// Use this for initialization
	void Start () {
		// Setup the reference to the main camera
		camGO = GameObject.FindGameObjectWithTag("LoadBox");
		deviceName = "Omni@localhost:3884";
		initializeReports();
		
		/*
		 * Initialize haptic workspace cube
		 */
		xCoef = baseScale;
		yCoef = baseScale;
		zCoef = baseScale * 2;
		 
		cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		Destroy(cube.GetComponent<BoxCollider>());
		
		Shader s = Shader.Find("Transparent/Diffuse");
		cube.GetComponent<Renderer>().material.shader = s;
		Color c = Color.white;
		c.a = 0.1f;
		cube.GetComponent<Renderer>().material.color = c;
		
		computeWorkspaceCube();
	}
	
	protected override void initializeReports ()
	{
		// Allocate and initialize memory for reports
		// The process involves Marshaling, in order to exchange data between Unity and the C++ VRPN wrapper
		reports = new IntPtr[maxNumberOfReports];
		VRPNReport report = new VRPNReport();
		report.sensor = 0;
		report.pos = new double[3];
		report.quat = new double[4];
		report.quat[3] = 1.0f;
		for(int i = 0; i < maxNumberOfReports; i++)
		{
			reports[i] = Marshal.AllocHGlobal(Marshal.SizeOf (typeof(VRPNReport)));
			// Copy the report struct to unmanaged memory (reports array)
			Marshal.StructureToPtr(report, reports[i], true);
		}
		velReport = Marshal.AllocHGlobal(Marshal.SizeOf (typeof(VRPNReport)));
		Marshal.StructureToPtr (report, velReport, true);
	}
	
	// Copied from UART VRPN wrapper
	// It is worth noting that VRPNTrackerStart must be called every frame
	protected override bool startDevice () { 	
		if (manager && manager.isInitialized() && deviceName != null)
		{
			// Register Tracker Device
			Debug.Log ("VRPNTrackerStart");
			VRPNTrackerStart(deviceName, 3, maxNumberOfReports);
			
			initialized = true;
			return true;
		}
		return false;
	}
	
	// Multiplicative factor for picker movement in local space
	static private float baseScale = 60.0f;
	public float xCoef;
	public float yCoef;
	public float zCoef;
	
//	private float clutchingOffset = 0;
	private Vector3 clutchingOffset = new Vector3(0.0f, 0.0f, 0.0f);
	private Vector3 currentClutchingPos = new Vector3(0.0f, 0.0f, 0.0f); 
	
	private float xMin = 0;
	private float xMax = 0;
	private float yMin = 0;
	private float yMax = 0;
	private float zMin = 0;
	private float zMax = 0;
	
	private bool listen = false;
	private bool moveWorkspace = false;
	
	private float currentScale = baseScale;
	private float currentScaleFactor = 0.0f;
	
	void OnVRPNButtonEvent(VRPNButtonController.VRPNButtonReport report) {
		if(report.button == 1 && report.state == 1) {
			moveWorkspace = true;
			
			currentClutchingPos.x = trackerPos.x;
			currentClutchingPos.y = trackerPos.y;
			currentClutchingPos.z = trackerPos.z;
			
			currentScaleFactor = trackerQuat.z;
			currentScale = xCoef;
			return;
		}
		moveWorkspace = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (!initialized && !startDevice()) return;
		
		if (!listen) {
			VRPNButtonController btn_ctrl = gameObject.GetComponent<VRPNButtonController>() as VRPNButtonController;
			if (btn_ctrl != null) {
				VRPNButtonController.OnButton += OnVRPNButtonEvent;
				listen = true;
			}
		}
		
		int num = maxNumberOfReports;
		
		// Get the first report for now
		VRPNReport rep;
		if (VRPNTrackerNumPosReports(deviceName) > 0) {
			// Retrieve tracker position report
			VRPNTrackerPosReports(deviceName, reports, ref num);
			rep = (VRPNReport)Marshal.PtrToStructure(reports[0], typeof(VRPNReport));
			
			// Store position in a Vector3
//			trackerPos.x = (float)rep.pos[0];
//			trackerPos.y = (float)rep.pos[1];
//			trackerPos.z = (float)rep.pos[2];
//			
//			if (trackerPos.x < minX) {
//				minX = trackerPos.x;
//			}
//			if (trackerPos.x > maxX) {
//				maxX = trackerPos.x;
//			}
//			if (trackerPos.y < minY) {
//				minY = trackerPos.y;
//			}
//			if (trackerPos.y > maxY) {
//				maxY = trackerPos.y;
//			}
//			if (trackerPos.z < minZ) {
//				minZ = trackerPos.z;
//			}
//			if (trackerPos.z > maxZ) {
//				maxZ = trackerPos.z;
//			}
//			
//			Debug.Log("X min: " + minX + " max: " + maxX);
//			Debug.Log("Y min: " + minY + " max: " + maxY);
//			Debug.Log("Z min: " + minZ + " max: " + maxZ);
			
			// Store position in a Vector3
			trackerPos.x = (float)rep.pos[0] * xCoef;
			trackerPos.y = (float)rep.pos[1] * yCoef;
			trackerPos.z = -(float)rep.pos[2] * zCoef;
			
			// Store quaternion components in a Quaternion
			trackerQuat.x = -(float)rep.quat[0];
			trackerQuat.y = -(float)rep.quat[1];
			trackerQuat.z = (float)rep.quat[2];
			trackerQuat.w = (float)rep.quat[3];
		}
		// Retrieve and store tracker velocity
		VRPNTrackerVelReport(deviceName, velReport, IntPtr.Zero, 0);
		rep = (VRPNReport)Marshal.PtrToStructure(velReport, typeof(VRPNReport));
		trackerVelocity.x = (float)rep.pos[0];
		trackerVelocity.y = (float)rep.pos[1];
		trackerVelocity.z = -(float)rep.pos[2];
		
		// Retrieve the camera rotation, stored as a Quaternion
		Quaternion camRotation = camGO.transform.rotation;
		// Retrieve the camera position and forward 3D vectors
		Vector3 camPosition = camGO.transform.position;
		Vector3 camForward = camGO.transform.forward;
		
		// Clutching
		if (moveWorkspace) {
			clutchingOffset.x = currentClutchingPos.x - trackerPos.x;
			clutchingOffset.y = currentClutchingPos.y - trackerPos.y;
			clutchingOffset.z = currentClutchingPos.z - trackerPos.z;
			
//			Debug.Log ("CurrentScaleF: " + currentScaleFactor);
//			Debug.Log ("TrackerZ: " + trackerQuat.z);
			float c = currentScale * (1 - (currentScaleFactor - trackerQuat.z));
			xCoef = c;
			yCoef = c;
			zCoef = c * 2;
			computeWorkspaceCube();
		}
		
		Vector3 cubePosOffset = new Vector3(-clutchingOffset.x, -clutchingOffset.y, -clutchingOffset.z);
		
//		cube.transform.position = camPosition + camForward * 20 - (camRotation * cubePosOffset);
		cube.transform.position = Vector3.Lerp (cube.transform.position, camPosition + camForward * 20 - (camRotation * cubePosOffset), Time.deltaTime * 8.0f);
		cube.transform.rotation = camRotation;
		
		trackerPos += clutchingOffset;
		
		// Set the picker orientation to a camera and pen orientation composition
		transform.rotation = camRotation * trackerQuat;
		// Keep the picker in front of the camera at a distance
		// Set the picker position to cam position + in front of it + rotate tracker positions by cam rotation
		
		transform.position = Vector3.Lerp (transform.position, camPosition + camForward * 20 + camRotation * trackerPos, Time.deltaTime * 8.0f);
//		transform.position = camPosition + camForward * 20 + camRotation * trackerPos;
	}
	
	private void computeWorkspaceCube() {
		// Calibrated for Phantom Omni through VRPN based on captured values
		xMin = -0.2199153f * xCoef;
		xMax = 0.2178341f * xCoef;
		yMin = -0.09427144f * yCoef;
		yMax = 0.2086051f * yCoef;
		zMin = -0.09475995f * zCoef;
		zMax = 0.117646f * zCoef;
		
		cube.transform.localScale = new Vector3((xMax - xMin), (yMax - yMin), (zMax - zMin));
		Vector3 camPosition = camGO.transform.position;
		Vector3 camForward = camGO.transform.forward;
		Vector3 pos = new Vector3(((xMax + xMin) / 2), ((yMax + yMin) / 2), ((zMax + zMin) / 2));
//		cube.transform.position = camPosition + camForward * 20 + pos;
	}
	
	public Vector3 getTrackerVelocity() {
		return trackerVelocity;
	}
	
	public Quaternion getTrackerRotation () {
		return transform.rotation;
	}
	
	public Transform getTransform () {
		return transform;
	}
	
	public void setManager (VRPNManager m) {
		manager = m;
	}
	
	public void setDeviceName(string dname) {
		if (!initialized) {
			deviceName = dname;
		}
	}
	
	void OnDestroy () {
		// The "marshaled" memory has to be deallocated explicitly
		for(int i = 0; i < maxNumberOfReports; i++)
		{
			Marshal.FreeHGlobal(reports[i]);
		}
		Marshal.FreeHGlobal(velReport);
	}	
}
