using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public class VRPNAnalogController : VRPNRemote {
	
	const int MAX_ANALOG_CHANNELS = 128;
	
	[DllImport ("UnityVRPN")]
	private static extern void VRPNAnalogStart(string name, int max = 1000);
	
	[DllImport ("UnityVRPN")]
	private static extern int VRPNAnalogNumReports(string name);
	
	[DllImport ("UnityVRPN")]
	private static extern void VRPNAnalogReports(string name, [In,Out] IntPtr[] reports, ref int number, [Out] IntPtr ts, bool clear = true);
	
	[ StructLayout( LayoutKind.Sequential)]
	public struct VRPNAnalogReport
	{
		public VRPNManager.TimeVal msg_time;
		public int num_channel;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ANALOG_CHANNELS)]
		public double[] channel;
	}
	
	private static Vector3 invertionVector = new Vector3(-1, 1, -1);
	public bool previousAxisInverted = false;
	public bool axisInverted = false;
	
	private VRPNManager manager;
	
	public float rotationSpeed = 200.0f;
	
	private float distance = 50.0f;
	private float xoffset = 0.0f;
	private float yoffset = 0.0f;
	
	public float xspeed = 2.0f;
	public float yspeed = 2.0f;
	public float zspeed = 2.0f;
	
	public Transform target;
	
	Vector3 rot = Vector3.zero;
	
	float deadCenterThreshold = 0.25f;
	
	public void setManager (VRPNManager m) {
		manager = m;
	}
	
	public static void invertAxis() {
		invertionVector *= -1;
	}
	
	// Use this for initialization
	void Start () {
		if (!target) {
			GameObject go = new GameObject ("VRPN Cam Target");
			target = go.transform;
		}
		
		deviceName = "device0@localhost";
		initializeReports();
	}
	
	protected override void initializeReports()
	{
		reports = new IntPtr[maxNumberOfReports];
		VRPNAnalogReport report = new VRPNAnalogReport();
		report.num_channel = MAX_ANALOG_CHANNELS;
		report.channel = new double[MAX_ANALOG_CHANNELS];
		for(int i = 0; i < maxNumberOfReports; i++)
		{
			reports[i] = Marshal.AllocHGlobal(Marshal.SizeOf (typeof(VRPNAnalogReport)));
			// Copy the report struct to unmanaged memory (reports array)
			Marshal.StructureToPtr(report, reports[i], true);
		}
	}
	
	protected override bool startDevice () { 
		
		if (manager && manager.isInitialized() && deviceName != null)
		{
			// Register Analog Device
			VRPNAnalogStart(deviceName);
			Debug.Log ("VRPNAnalogController initialized");
			initialized = true;
			return true;
		}
		return false;
	}
	
	public void setDeviceName(string dname) {
		if (!initialized) {
			deviceName = dname;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!initialized && !startDevice()) return;
		
		GameObject lb = gameObject;
		
		if (previousAxisInverted != axisInverted) {
			invertAxis();
			previousAxisInverted = axisInverted;
		}
		
		rot = Vector3.zero;
		
		if (VRPNAnalogNumReports(deviceName) > 0) {
			int num = maxNumberOfReports;
			VRPNAnalogReports(deviceName, reports, ref num, IntPtr.Zero, true);
			VRPNAnalogReport[] reps = new VRPNAnalogReport[num];
			
			int i;
			Vector3 pos = Vector3.zero;
			for (i = 0; i < num; i++)
			{
				reps[i] = (VRPNAnalogReport)Marshal.PtrToStructure(reports[i], typeof(VRPNAnalogReport));
				if (reps[i].num_channel == 6) {
					Vector3 cpos = new Vector3((float)reps[i].channel[0], (float)reps[i].channel[2], (float)reps[i].channel[1]); // Revert y and z axis
					Vector3 crot = new Vector3(-(float)reps[i].channel[3], (float)reps[i].channel[5], (float)reps[i].channel[4]);
					
					pos += cpos;
					rot += crot;
				}
			}
			
			// Average position on the number of reports
			pos /= num;
			
			// Multiply by invertion vector
			// Invertion vector is set to the desired configuration by clicking a GUI checkbox
			pos.x *= invertionVector.x;
			pos.y *= invertionVector.y;
			pos.z *= invertionVector.z;
			
			// Average rotation on the number of reports
			rot /= num;
			rot = -rot; // Invert rotation
			
			// Dead center
			if ((pos.x > -deadCenterThreshold && pos.x < deadCenterThreshold))
				pos.x = 0;
			
			if ((pos.y > -deadCenterThreshold && pos.y < deadCenterThreshold))
				pos.y = 0;
			
			if ((pos.z > -deadCenterThreshold && pos.z < deadCenterThreshold))
				pos.z = 0;
			
			xoffset += pos.x * xspeed;
			yoffset += pos.y * yspeed;
			distance += pos.z * zspeed;
		}
	}
	
	void LateUpdate() {
		if (!initialized && !startDevice()) return;
		GameObject lb = gameObject;
		
		//		lb.transform.rotation = Quaternion.Lerp (lb.transform.rotation, rot, );
		lb.transform.Rotate(rot * rotationSpeed * Time.deltaTime, Space.Self);
		
		Quaternion targetRotation = Quaternion.Euler (rot.x, rot.y, rot.z);
		lb.transform.rotation = Quaternion.Lerp(lb.transform.rotation, lb.transform.rotation * targetRotation, Time.deltaTime);
		
		Vector3 new_target_pos = lb.transform.rotation * new Vector3(xoffset, yoffset, 0.0f);
		
		Vector3 v = new Vector3(-transform.forward.x, -transform.forward.y, -transform.forward.z) * distance + new_target_pos;
//		lb.transform.position = Vector3.Lerp (lb.transform.position, v, Time.deltaTime * 6.0f);
		lb.transform.position = v;
	}
	
	void OnDestroy() {
		// The "marshaled" memory has to be deallocated explicitly
		for(int i = 0; i < maxNumberOfReports; i++)
		{
			Marshal.FreeHGlobal(reports[i]);
		}
	}
}
