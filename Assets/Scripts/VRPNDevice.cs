using UnityEngine;
using System.Collections;

public enum VRPNDeviceType { Phantom, Mouse3DConnexion };

public class VRPNDevice {
	private string name;
	private VRPNDeviceType type;
		
	public bool pick = false;
	
	public VRPNDevice(string name, VRPNDeviceType type)
	{
		this.name = name;
		this.type = type;
	}
	
	public string getName()
	{
		return name;
	}
	
	public VRPNDeviceType getType()
	{
		return type;
	}
}
