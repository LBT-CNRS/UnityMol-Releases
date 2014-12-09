using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

/// <summary>
/// VRPN manager script
/// </summary>
/// <description>This script is attached to a VRPNManager GameObject</description>
using System;
using System.Collections.Generic;
using System.Linq;
using UI;

using System.IO;

public class VRPNManager : MonoBehaviour {
	
	[ StructLayout( LayoutKind.Sequential )]
	public struct TimeVal
	{
		public UInt32 tv_sec;
		public UInt32 tv_usec;
	}
	
	/// <summary>
	/// Default server address
	/// </summary>
	/// <remarks>Set for XBOX. In the future, this should perfectly configurable.</remarks>
	public static string ServerAddress = "172.27.0.80";
	
	/// <summary>
	/// Flag for VRPNManager status
	/// </summary>
	/// <description>Set to false by default. Turned to true after VRPNServerStart is called.</description>
	private bool initialized = false;
	
	private static bool omniConnected = false;
	private static bool mouseConnected = false;
	
	/// <summary>
	/// Array of device names
	/// </summary>
	/// <description>Hard-written for now</description>
	/// <remarks>In the future, this could be fed by a server transaction if the VRPN protocol allows this.</remarks>
	private Hashtable devices = new Hashtable();
	private string[] availableDevices = {"Omni", "device0"};
	
	/// <summary>
	/// VRPNServerStart
	/// </summary>
	/// <param name="file">File: A VRPN config file to start the server with.</param>
	/// <param name="location">Location: The server address/ip.</param>
	/// <description>This function will not start any server if no config <paramref name="file">file</paramref> is provided. Instead, it will only connect to the provided <paramref name="location">location</paramref></description>
	/// <remarks>TODO: This done not provide any information about the success of the operation.</remarks>
	[DllImport ("UnityVRPN")]
	private static extern void VRPNServerStart(string file, string location);
	
	/// <summary>
	/// VRPNServerStop
	/// </summary>
	/// <description>Stop the server or close the connection with it.</description>
	[DllImport ("UnityVRPN")]
	private static extern void VRPNServerStop();
	
	/// <summary>
	/// VRPNServerLoop
	/// </summary>
	/// <description>As its name does not indicate, it handles any incoming message.</description>
	[DllImport ("UnityVRPN")]
	private static extern void VRPNServerLoop();
	
	[DllImport ("UnityVRPN")]
	private static extern bool VRPNServerConnected();
	
	List<GameObject> tools;
	
	// Use this for initialization
	void Start () {
		initialized = true;
		VRPNServerStart(null, ServerAddress);
//		VRPNServerLoop();
		if (VRPNServerConnected()) {
			initialized = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		VRPNServerLoop();
		initialized = VRPNServerConnected();
	}
	
	void OnDestroy () {
		VRPNServerStop();
		initialized = false;
	}
	
	public bool addDevice (string deviceName, VRPNDeviceType type) {
		Debug.Log (deviceName);
		VRPNDevice device = new VRPNDevice(deviceName, type);
		if (true || (availableDevices.Contains(deviceName) && !devices.ContainsKey(deviceName))) {
			devices.Add(deviceName, device);
			switch(type) {
				case VRPNDeviceType.Phantom:
				Debug.Log ("Instanciating VRPNPicker");
				GameObject go = (GameObject) GameObject.Instantiate(Resources.Load("VRPN/VRPNPicker", typeof(GameObject)), Vector3.zero, Quaternion.identity);
				Debug.Log ("Configuring VRPNPickerController");
				VRPNPickerController s1 = go.GetComponent<VRPNPickerController>();
					if (s1)
					{
						s1.setManager(this);
						s1.setDeviceName(deviceName);
					}
					
					Debug.Log ("After pickercontroller");
					
					VRPNButtonController s2 = go.GetComponent<VRPNButtonController>();
					if (s2)
					{
					    s2.setManager(this);
					    s2.setDeviceName(deviceName);
					}
					
					VRPNForceFeedback s3 = go.GetComponent<VRPNForceFeedback>();
					if (s3)
					{
						s3.setManager(this);
					    s3.setDeviceName(deviceName);
					    if (!s3.startDeviceInLib())
					    {
						    Debug.Log (":: VRPNManager :: Can't activate Force Feedback");
						}
					}
					
					break;
				case VRPNDeviceType.Mouse3DConnexion:
				    VRPNAnalogController s = GameObject.FindGameObjectWithTag("LoadBox").GetComponent<VRPNAnalogController>();
				    s.setManager(this);
				    break;
				default:
				    break;
			}
			return true;
		}
		return false;
	}
	
	public void renderDevicesGUI ()
	{
		if (initialized) {
			VRPNDevice dev;
			foreach (DictionaryEntry de in devices) {
				dev = (VRPNDevice) de.Value;
				
				GUILayout.Space (10);
				GUILayout.BeginHorizontal();
				GUILayout.Label(dev.getName());
				GUILayout.EndHorizontal();
				
				if (dev.getType() == VRPNDeviceType.Phantom) {
					GUILayout.BeginHorizontal();
					GUILayout.Label("Connected");
					GUILayout.EndHorizontal();
				}
				else if (dev.getType() == VRPNDeviceType.Mouse3DConnexion) {
					VRPNAnalogController script = GameObject.FindGameObjectWithTag("LoadBox").GetComponent<VRPNAnalogController>();
	
					GUILayout.BeginHorizontal();
					script.axisInverted = GUILayout.Toggle(script.axisInverted, "Invert Axis");
					GUILayout.EndHorizontal();
					
					GUILayout.BeginHorizontal();
					GUILayout.Label("X Speed");
					GUILayout.Label(script.xspeed.ToString());
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					script.xspeed = GUILayout.HorizontalSlider(script.xspeed, 0.0f, 4.0f);
					GUILayout.EndHorizontal();
					
					GUILayout.BeginHorizontal();
					GUILayout.Label("Y Speed");
					GUILayout.Label(script.yspeed.ToString());
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					script.yspeed = GUILayout.HorizontalSlider(script.yspeed, 0.0f, 4.0f);
					GUILayout.EndHorizontal();
					
					GUILayout.BeginHorizontal();
					GUILayout.Label("Zoom Speed");
					GUILayout.Label(script.zspeed.ToString());
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					script.zspeed = GUILayout.HorizontalSlider(script.zspeed, 0.0f, 4.0f);
					GUILayout.EndHorizontal();
					
					GUILayout.BeginHorizontal();
					GUILayout.Label("Rotation Speed");
					GUILayout.Label(script.rotationSpeed.ToString());
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					script.rotationSpeed = GUILayout.HorizontalSlider(script.rotationSpeed, 120.0f, 280.0f);
					GUILayout.EndHorizontal();
				}
			}
		}
	}
	
	public bool isInitialized()
	{
		return initialized;
	}
	
	public static GameObject vrpnManager;
	public static VRPNManager vrpnManagerScript;
	public static string deviceName = "Omni";
	public static VRPNDeviceType type = VRPNDeviceType.Phantom;
	public static void VRPNM (int a) {
		GUIMoleculeController.showVRPNMenu = LoadTypeGUI.SetTitleExit("VRPN Client");
		
		/*
			 * Device configuration section
			 */
		
		if (vrpnManagerScript && vrpnManagerScript.isInitialized()) {
			vrpnManagerScript.renderDevicesGUI();
		}
		
		GUILayout.Space(20);
		
		/*
			 * Server connection section
			 */
		
		if (!omniConnected && GUILayout.Button ("Omni@localhost:3884"))
		{
			Debug.Log ("Adding device");
			omniConnected = true;
			vrpnManagerScript.addDevice("Omni@localhost:3884", VRPNDeviceType.Phantom);
			Debug.Log ("Omni@localhost added");
		}
		
		if (!mouseConnected && GUILayout.Button ("device0@localhost"))
		{
			Debug.Log ("Adding device");
			mouseConnected = true;
			vrpnManagerScript.addDevice("device0@localhost", VRPNDeviceType.Mouse3DConnexion);
			Debug.Log ("device0@localhost added");
		}
		
		GUI.enabled = true;
		GUI.DragWindow();
	}
}
