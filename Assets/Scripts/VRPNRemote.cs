using UnityEngine;
using System.Collections;
using System; // For IntPtr

public abstract class VRPNRemote : MonoBehaviour {

	protected bool initialized = false;
	
	/// <summary>
	/// The name of the device.
	/// </summary>
	/// <description>This field is set to use "Omni".</description>
	/// <remarks>In the future, it should be set by VRPNManager.</remarks>
	protected string deviceName;
	
	/// <summary>
	/// Array of report
	/// </summary>
	/// <description>Marshaled array filled by the wrapper.</description>
	protected IntPtr[] reports;
	
	/// <summary>
	/// The max number of reports.
	/// </summary>
	/// <description>We accept a maximum number of reports from the server.</description>
	protected int maxNumberOfReports = 20;
	
	abstract protected void initializeReports();
	
	abstract protected bool startDevice();
}
