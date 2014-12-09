using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public class VRPNButtonController : VRPNRemote {
	public delegate void ButtonAction(VRPNButtonReport report);
	public static event ButtonAction OnButton;

	/// <summary>
	/// VRPNButtonStart wrapper API function
	/// </summary>
	/// <param name="name">Name: Device name</param>
	/// <param name="max">Max: The max number of retrieved reports</param>
	/// <description>Initialize a button "tracker" in the wrapper library for a provided <paramref name="name">device</paramref></description>
	[DllImport ("UnityVRPN")]
	private static extern void VRPNButtonStart(string name, int max);
	
	/// <summary>
	/// VRPNButtonReports wrapper API function
	/// </summary>
	/// <param name="name">Name: Device name</param>
	/// <param name="reportsPtr">Reports ptr: Array of pointers to reports</param>
	/// <param name="cnt">Count: Number of available reports</param>
	/// <param name="ts">Ts: TimeVal</param>
	/// <param name="btn_num">Btn num: the number of the button we want to track</param>
	/// <param name="clearReport">If set to <c>true</c> clear useless reports.</param>
	/// <description>Retrieve reports by filling the <paramref name="reportsPtr">array</paramref>. The trick with this wrapper function is not to use the ts parameter by setting it to a null pointer.</description>
	[DllImport ("UnityVRPN")]
	private static extern void VRPNButtonReports(string name, [In,Out] IntPtr[] reportsPtr, [In,Out] ref int cnt, IntPtr ts, int btn_num, bool clearReport);
	
	/// <summary>
	/// VRPNButtonNumReports
	/// </summary>
	/// <returns>The number of available reports.</returns>
	/// <param name="name">Name: Device name</param>
	[DllImport ("UnityVRPN")]
	private static extern int VRPNButtonNumReports(string name);
	
	
	// Button report data structure from UART
	[ StructLayout( LayoutKind.Sequential, Pack=1 )]
	public struct VRPNButtonReport
	{
		public VRPNManager.TimeVal msg_time;
		public int button;
		public int state;
	}
	
	private VRPNManager manager;
	
	public void setManager (VRPNManager m) {
		manager = m;
	}
	
	public void setDeviceName(string dname) {
		if (!initialized) {
			deviceName = dname;
		}
	}
	
	// Use this for initialization
	void Start () {
		deviceName = "Omni@localhost:3884";
		initializeReports();
	}
	
	protected override void initializeReports()
	{
		reports = new IntPtr[maxNumberOfReports];
		VRPNButtonReport bReport = new VRPNButtonReport();
		bReport.button = 0;
		bReport.state = 0;
		for(int i = 0; i < maxNumberOfReports; i++)
		{
			reports[i] = Marshal.AllocHGlobal(Marshal.SizeOf (typeof(VRPNButtonReport)));
			// Copy the report struct to unmanaged memory (reports array)
			Marshal.StructureToPtr(bReport, reports[i], true);
		}
	}
	
	// Copied from UART VRPN wrapper
	// It is worth noting that VRPNTrackerStart must be called every frame
	protected override bool startDevice () {
		if (manager && manager.isInitialized() && deviceName != null)
		{
			// Register Tracker Device
			VRPNButtonStart(deviceName, maxNumberOfReports);
			
			initialized = true;
			return true;
		}
		return false;
	}
	
	// Update is called once per frame
	void Update () {
		if (!initialized && !startDevice()) return;
		
		int num = maxNumberOfReports;
		
		// Get button reports if any
		if (VRPNButtonNumReports(deviceName) > 0) {
			VRPNButtonReports(deviceName, reports, ref num, IntPtr.Zero, -1, true);
			for (int i = 0; i < num; i++) {
				VRPNButtonReport brep = (VRPNButtonReport)Marshal.PtrToStructure(reports[i], typeof(VRPNButtonReport));
				if (OnButton != null)
				{
					OnButton(brep);
				}
			}
		}
	}
	
	void OnDestroy() {
		for(int i = 0; i < maxNumberOfReports; i++)
		{
			Marshal.FreeHGlobal(reports[i]);
		}
	}
}
