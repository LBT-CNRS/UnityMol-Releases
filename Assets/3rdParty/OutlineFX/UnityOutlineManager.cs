using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Xenu.Game {

	public class UnityOutlineManager : MonoBehaviour {
		// [System.Serializable]
		// public class OutlineData
		// {
		// 	public Renderer renderer;
		// }

		public UnityOutlineFX outlinePostEffect;
		// public OutlineData[] outliners;
		// int curNbRenderer = 0;

		// private void Start()
		// {
		// 	foreach (var obj in outliners)
		// 	{
		// 		outlinePostEffect.AddRenderers(new List<Renderer>() { obj.renderer });
		// 	}
		// 	curNbRenderer = outliners.Length;
		// }

		public void ClearOutlines(){
			outlinePostEffect.ClearOutlineData();
		}

		public void AddOutline(MeshRenderer mr){
			outlinePostEffect.AddRenderers(new List<Renderer>() { mr });
		}

		// void Update(){
		// 	if(curNbRenderer != outliners.Length){
		// 		outlinePostEffect.ClearOutlineData();
				
		// 		foreach (var obj in outliners)
		// 		{
		// 			outlinePostEffect.AddRenderers(new List<Renderer>() { obj.renderer });
		// 		}

		// 		curNbRenderer = outliners.Length;
		// 	}
		// }
	}

}