// Copyright (c) 2015, Felix Kate All rights reserved.
// Usage of this code is governed by a BSD-style license that can be found in the LICENSE file.

/*<Description>
For the new Unity GUI System won't work on older Unity Versions.
Short script for handling the controls of the color picker GUI element.
The user can drag the slider on the ring to change the hue and the slider in the box to set the blackness and saturation.
If used without prefab add this to an image canvas element which useses the ColorWheelMaterial.
Also needs 2 subobjects with images as slider graphics and an even trigger for clicking that references the OnClick() method of this script.
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

public class ColorWheelControl : MonoBehaviour {

	//Output Color
	public Color Selection;

	public UnityEvent newColorEvent;

	//Control values
	private float outer;
	private Vector2 inner;

	private bool dragOuter, dragInner;

	//The Components of the wheel
	private Material mat;
	private RectTransform RectTrans, SelectorOut, SelectorIn;

	private float halfSize;

	//Set up the transforms
	void Start(){
		//Get the rect transform and make x and y the same to avoid streching
		RectTrans =  GetComponent<RectTransform>();
		RectTrans.sizeDelta = new Vector2(RectTrans.sizeDelta.x, RectTrans.sizeDelta.x);

		//Find and scale the children
		SelectorOut = transform.Find ("Selector_Out").GetComponent<RectTransform>();
		SelectorIn = transform.Find ("Selector_In").GetComponent<RectTransform>();

		SelectorOut.sizeDelta =  RectTrans.sizeDelta / 20.0f;
		SelectorIn.sizeDelta =  RectTrans.sizeDelta / 20.0f;

		//Calculate the half size
		halfSize = RectTrans.sizeDelta.x / 2;

		//Set the material
		mat = GetComponent<Image>().material;

		//Set first selected value to red (0° rotation and upper right corner in the box)
		Selection = Color.red;

		//Update the material of the box to red
		updateMaterial();
	}



	//Update the selectors
	void Update(){
		//Drag selector of outer circle
		if(dragOuter){
			//Get mouse direction
			Vector2 dir = RectTrans.position - Input.mousePosition;
			dir.Normalize();

			//Calculate the radians
			outer = Mathf.Atan2 (-dir.x, -dir.y);

			//And update
			updateMaterial ();
			updateColor();

			//On mouse release also release the drag
			if(Input.GetMouseButtonUp(0))dragOuter = false;
		
		//Drag selector of inner box
		}else if(dragInner){
			//Get position inside the box
			Vector2 dir = RectTrans.position - Input.mousePosition;
			dir.x = Mathf.Clamp(dir.x, -halfSize / 2,  halfSize / 2) + halfSize / 2;
			dir.y = Mathf.Clamp(dir.y, -halfSize / 2,  halfSize / 2) + halfSize / 2;

			//Scale the value to 0 - 1;
			inner = dir / halfSize;

			updateColor();

			//On mouse release also releaste the drag
			if(Input.GetMouseButtonUp(0))dragInner = false;
		}

		//Set the selectors positions
		SelectorOut.localPosition = new Vector3(Mathf.Sin(outer) * halfSize * 0.85f, Mathf.Cos(outer) * halfSize * 0.85f, 1);
		SelectorIn.localPosition = new Vector3(halfSize * 0.5f - inner.x * halfSize, halfSize * 0.5f - inner.y * halfSize, 1);
	}



	//Update the material of the inner box to match the hue color
	void updateMaterial(){
		Color c = Color.white;

		//Calculation of rgb from degree with a modified 3 wave function
		//Check out http://en.wikipedia.org/wiki/File:HSV-RGB-comparison.svg to understand how it should look
		c.r = Mathf.Clamp(2/Mathf.PI * Mathf.Asin(Mathf.Cos(outer)) * 1.5f + 0.5f, 0, 1);
		c.g = Mathf.Clamp(2/Mathf.PI * Mathf.Asin(Mathf.Cos(2 * Mathf.PI * (1.0f/3.0f) - outer)) * 1.5f + 0.5f, 0, 1);
		c.b = Mathf.Clamp(2/Mathf.PI * Mathf.Asin(Mathf.Cos(2 * Mathf.PI * (2.0f/3.0f) - outer)) *  1.5f + 0.5f, 0, 1);

		mat.SetColor("_Color", c);
	}



	//Gets called after changes
	void updateColor(){
		Color c = Color.white;

		//Calculation of color same as above
		c.r = Mathf.Clamp(2/Mathf.PI * Mathf.Asin(Mathf.Cos(outer)) * 1.5f + 0.5f, 0, 1);
		c.g = Mathf.Clamp(2/Mathf.PI * Mathf.Asin(Mathf.Cos(2 * Mathf.PI * (1.0f/3.0f) - outer)) * 1.5f + 0.5f, 0, 1);
		c.b = Mathf.Clamp(2/Mathf.PI * Mathf.Asin(Mathf.Cos(2 * Mathf.PI * (2.0f/3.0f) - outer)) *  1.5f + 0.5f, 0, 1);

		//Add the colors of the inner box
		c = Color.Lerp(c, Color.white, inner.x);
		c = Color.Lerp(c, Color.black, inner.y);

		Selection = c;
		newColorEvent.Invoke();

	}



	//Method for setting the picker to a given color
	public void PickColor(Color c){
		//Get hsb color from the rgb values
		float max = Mathf.Max (c.r, c.g, c.b);
		float min = Mathf.Min (c.r, c.g, c.b);
		
		float hue = 0;
		float sat = (1 - min);

		if(max == min){
			sat = 0;
		}
		
		hue = Mathf.Atan2(Mathf.Sqrt(3)*(c.g-c.b), 2* c.r-c.g-c.b);

		//Set the sliders
		outer = hue;
		inner.x = 1 - sat;
		inner.y = 1 - max;

		//And update them once
		updateMaterial();
	}

	//Gets called by an event trigger at a click
	public void OnClick(){
		//Check if click was in outer circle
		if(Vector2.Distance(RectTrans.position, Input.mousePosition) <= halfSize &&
		   Vector2.Distance(RectTrans.position, Input.mousePosition) >= halfSize - halfSize / 4){
			dragOuter = true;
			return;
		//Check if click was in inner box
		}else if(Mathf.Abs(RectTrans.position.x - Input.mousePosition.x) <= halfSize / 2 &&
		         Mathf.Abs(RectTrans.position.y - Input.mousePosition.y) <= halfSize / 2){
			dragInner = true;
			return;
		}
	}

}
