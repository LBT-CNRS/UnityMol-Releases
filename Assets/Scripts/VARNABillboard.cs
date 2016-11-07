using UnityEngine;
using System.Collections;
using Molecule.Model;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;
using System.Threading;

public class VARNABillboard : MonoBehaviour {

	private GameObject m_Camera;
	private bool done = true;
	
	private Process varnaProcess;
	private Thread oThread;
	
	private string outputDirectory = Application.dataPath + "/Resources";

	// Use this for initialization
	void Start () {
		m_Camera = GameObject.Find ("Camera");
		
		gameObject.transform.localPosition = new Vector3(2.0f,-1.0f,3.0f);
		gameObject.transform.localRotation = Quaternion.identity;
//		gameObject.transform.LookAt(-m_Camera.transform.position);
	}
	
	private void varnaProcess_Exited(object sender, System.EventArgs e)
	{
		
		done = true;
//		Console.WriteLine("Exit time:    {0}\r\n" +
//		                  "Exit code:    {1}\r\n", varnaProcess.ExitTime, varnaProcess.ExitCode);
	}
	
	// Update is called once per frame
	void Update () {
//		UnityEngine.Debug.Log ("--------");
//		UnityEngine.Debug.Log (done);
//		UnityEngine.Debug.Log (loading);
//		UnityEngine.Debug.Log (loaded);
		if (done)
		{
			done = false;
			
			StartCoroutine("loadFile");
			
			oThread = new Thread(new ThreadStart(generateFileInThread));
			oThread.Start();
		}
	}
	
	void generateFileInThread() {
		List<int[]> hbonds = RNAView.findHbonds();
		PlotManager.Instance.PlotAdd("NHBonds", hbonds.Count);
		int sequenceLength = MoleculeModel.sequence.Count();
		string structure = VARNA.generateStructureString(sequenceLength, hbonds);
		
		varnaProcess = VARNA.generateImage(MoleculeModel.sequence, structure, outputDirectory, "test.png");
		varnaProcess.Exited += new EventHandler(varnaProcess_Exited);
		varnaProcess.Start();
	}
	
	IEnumerator loadFile() {
		ResourceRequest request = Resources.LoadAsync("test");
		yield return request;
		gameObject.GetComponent<Renderer>().material.mainTexture = request.asset as Texture2D;
		UnityEngine.Debug.Log ("::::: Loaded");
	}
}
