using UnityEngine;
using System.Collections;
using UI;

public class DirLightManager : MonoBehaviour {
	private GameObject obj;

	// Use this for initialization
	void Start () {
		obj = gameObject;
	}
	
	
	void Update () {
		if(	(UIData.atomtype == UIData.AtomType.particleball) || ((UIData.atomtype == UIData.AtomType.hyperball) || (UIData.atomtype == UIData.AtomType.noatom) 
			&& (UIData.bondtype == UIData.BondType.nobond) || (UIData.bondtype == UIData.BondType.line) || (UIData.bondtype == UIData.BondType.hyperstick)) )
		{
			obj.light.shadows = LightShadows.None;
		}
		else
		{
			obj.light.shadows = LightShadows.Soft;			
		}
	}
}
