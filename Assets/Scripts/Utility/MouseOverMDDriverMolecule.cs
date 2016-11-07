using UnityEngine;
using System.Collections;

public class MouseOverMDDriverMolecule : MonoBehaviour {
	public static bool stopCamera = false;
	private GameObject arrowParent;
	
	// Use this for initialization
	
	
	// Does something as a response 
	// to the user clicking on the object 
	void OnMouseDrag () {

		if (arrowParent == null)
		{
			arrowParent = new GameObject("Arrow");

			GameObject arrow;
			arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
			arrow.transform.parent = arrowParent.transform;
			arrow.transform.localScale = new Vector3(0.4f, 3.5f, 0.4f);
			arrow.transform.Translate(arrow.transform.up * 2.0f);
			arrow.GetComponent<Renderer>().enabled = true;

			arrowParent.transform.position = transform.position;				
			arrowParent.transform.parent = transform;
		}

		MDDriver mddriverScript = GameObject.FindObjectOfType<MDDriver>();
		BallUpdate script = gameObject.GetComponent<BallUpdate>();
		
		if(Input.GetMouseButton(0)){
			maxCamera.cameraStop = true;
			stopCamera = true;
			
			Vector3 p = Input.mousePosition;
			p.z = Camera.main.WorldToScreenPoint(transform.position).z;
			
			Vector3 worldCoords = Camera.main.ScreenToWorldPoint(p);
			Vector3 force = worldCoords - transform.position;
			mddriverScript.applyForces(new int[] {(int)script.number}, new float[] {force.x, force.y, force.z});
//			transform.position = Camera.main.ScreenToWorldPoint(p);

			float distance = Vector3.Distance(worldCoords, transform.position);
			float arrowZScale = distance / 8.0f;
			float arrowScale = distance / 12.0f;
			arrowParent.transform.up = force;
			arrowParent.transform.localScale = new Vector3(arrowScale, arrowZScale, arrowScale);
		}	
	}
	
	void OnMouseUp() {
		BallUpdate script = gameObject.GetComponent<BallUpdate>();
		
		MDDriver mddriverScript = GameObject.FindObjectOfType<MDDriver>();
		mddriverScript.applyForces(new int[] {(int)script.number}, new float[] {0.0f, 0.0f, 0.0f});
		
		maxCamera.cameraStop = false;
		stopCamera = false;

		if (arrowParent != null)
		{
			GameObject.DestroyImmediate(arrowParent);
			arrowParent = null;
		}
	}
}
