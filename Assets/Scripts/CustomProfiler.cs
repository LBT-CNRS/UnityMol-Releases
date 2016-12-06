using UnityEngine;
using System.Collections;

public class CustomProfiler : MonoBehaviour {
	public bool loadProfilerData = false;
	public bool recordProfilerdata = false;
	// Use this for initialization
	void Start () {
		if(loadProfilerData)
			Profiler.AddFramesFromFile(Application.dataPath + "/profilerLog.txt");
	}
	
	// Update is called once per frame
	void Update () {
		if (recordProfilerdata) {
			// write FPS to "profilerLog.txt"
			Profiler.logFile = Application.persistentDataPath + "/profilerLog.txt";   
			//string text = Application.persistentDataPath;
			//Debug.Log (text);
			// write Profiler Data to "profilerLog.txt.data"                                                                                        
			Profiler.enableBinaryLog = true;                                                 
			Profiler.enabled = true;
		}
	}
}
