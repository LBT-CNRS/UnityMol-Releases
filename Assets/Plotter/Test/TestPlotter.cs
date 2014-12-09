using UnityEngine;
using System.Collections;

public class TestPlotter : MonoBehaviour {

	// Use this for initialization
	void Start () {

        //  Create a new graph named "MouseX", with a range of 0 to 2000, colour green at position 100,100
        PlotManager.Instance.PlotCreate("MouseX", 0, 2000, Color.green, new Vector2(100,100));

        // Create a new child "MouseY" graph.  Colour is red and parent is "MouseX"
        PlotManager.Instance.PlotCreate("MouseY", Color.red, "MouseX");
	}
	
	// Update is called once per frame
	void Update () {

        // Add data to graphs
        PlotManager.Instance.PlotAdd("MouseX", Input.mousePosition.x);
        PlotManager.Instance.PlotAdd("MouseY", Input.mousePosition.y);
	}


}
