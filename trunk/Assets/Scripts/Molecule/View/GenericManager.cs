using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class GenericManager : MonoBehaviour {

	/// <summary>
	/// Disables the renderers for all objects managed by the instance of the manager.
	/// </summary>
	public abstract void DisableRenderers();
	
	/// <summary>
	/// Enables the renderers for all objects managed by the instance of the manager.
	/// </summary>
	public abstract void EnableRenderers();
	
	/// <summary>
	/// Initializes this instance of the manager.
	/// </summary>
	public abstract void Init();
	
	public abstract void DestroyAll();
	
	public abstract void SetColor(Color col, List<string> atoms, string residue = "All", string chain = "All");
	public abstract void SetColor(Color col, int atomNum);
	
	public abstract void SetRadii(List<string> atoms, string residue = "All", string chain = "All");
	public abstract void SetRadii(int atomNum);
	
	public abstract GameObject GetBall(int id);
	
	public abstract void ToggleDistanceCueing(bool enabling);
	
	public abstract void ResetPositions();
	public abstract void ResetMDDriverPositions();
}