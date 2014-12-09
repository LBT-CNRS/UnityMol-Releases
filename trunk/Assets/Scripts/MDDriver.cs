using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using Molecule.Model;
using UI;
using Molecule.View;
using System.Linq;
using System.Collections.Generic;

public class MDDriver : MonoBehaviour {
	
	public struct IMDEnergies
	{
		public int tstep;  //!< integer timestep index
		public float T;          //!< Temperature in degrees Kelvin
		public float Etot;       //!< Total energy, in Kcal/mol
		public float Epot;       //!< Potential energy, in Kcal/mol
		public float Evdw;       //!< Van der Waals energy, in Kcal/mol
		public float Eelec;      //!< Electrostatic energy, in Kcal/mol
		public float Ebond;      //!< Bond energy, Kcal/mol
		public float Eangle;     //!< Angle energy, Kcal/mol
		public float Edihe;      //!< Dihedral energy, Kcal/mol
		public float Eimpr;
	};
	
	//Lets make our calls from the Plugin
	[DllImport ("Unity_MDDriver")]
	private static extern void MDDriver_init(string hostname, int port);
	
	[DllImport ("Unity_MDDriver")]
	private static extern int MDDriver_start();
	
	[DllImport ("Unity_MDDriver")]
	private static extern int MDDriver_stop();

	[DllImport ("Unity_MDDriver")]
	private static extern bool MDDriver_isConnected();	

	[DllImport ("Unity_MDDriver")]
	private static extern int MDDriver_getNbParticles();

	[DllImport ("Unity_MDDriver")]
	private static extern int MDDriver_getPositions([In, Out] float[] verts, int nbParticles);
	
	[DllImport ("Unity_MDDriver")]
	private static extern void MDDriver_pause();
	
	[DllImport ("Unity_MDDriver")]
	private static extern void MDDriver_play();
	
	[DllImport ("Unity_MDDriver")]
	private static extern void MDDriver_setForces(int nbforces, int[] atomslist, float[] forceslist);
	
	[DllImport ("Unity_MDDriver")]
	private static extern void MDDriver_resetForces();
	
	[DllImport ("Unity_MDDriver")]
	private static extern void MDDriver_getEnergies(ref IMDEnergies energies);
	
	
	private static bool hb = false;
	private int nbParticles = -1;
	private float[] pos2;
	public static string host = "localhost";
	public static string portString = "3000";
	private static int port = 3000;
	
	private Molecule3D Molecule3DComp;
	private bool logFPS = false;
	
	void Start () {
		initMDDriver();
		GameObject LoadBox;
		LoadBox = GameObject.Find ("LoadBox");
		Molecule3DComp = LoadBox.GetComponent<Molecule3D> ();
		PlotManager.Instance.PlotCreate("Total Energy", -50, 400, Color.black, new Vector2(500,100));
	}
	
	public void initMDDriver(){
		if(MDDriver_isConnected())
			MDDriver_stop();
		
		MDDriver_init(host, port);
		Debug.Log("MDD start: " + MDDriver_start());
//		 GUIMoleculeController.toggle_NA_INTERACTIVE = true;
	}
	
	public void applyForces(int[] atoms, float[] forces)
	{
		MDDriver_setForces(atoms.Length, atoms, forces);
	}
	
	public void resetForces()
	{
		MDDriver_resetForces();
	}
	
	void Update() {
		IMDEnergies energies = new IMDEnergies();
		if(MDDriver_isConnected())
		{
//			if (!logFPS)
//			{
//				logFPS = true;
//				Molecule3DComp.toggleFPSLog ();
//			}
			MDDriver_getEnergies(ref energies);
//			Debug.Log (energies.Etot);
			PlotManager.Instance.PlotAdd("Total Energy", energies.Etot);
			if (Input.GetKeyDown (KeyCode.A)){
				MDDriver_pause();
			}
			
			if (Input.GetKeyDown (KeyCode.Z)){
				MDDriver_play();
			}
			
			int readParticles = MDDriver_getNbParticles();
			//Debug.Log("readParticles: " + readParticles);
			if(readParticles != nbParticles)
			{
				Debug.Log("readParticles: " + readParticles);
			 	nbParticles = readParticles;
				pos2 = new float[nbParticles*3];
			}
			else if(pos2.Length == readParticles*3)
			{
				// Store retrieved positions but let managers update positions later
				readParticles = MDDriver_getPositions(pos2, readParticles);
				List<Vector3> lst = new List<Vector3>();
				for (int i = 0; i < readParticles; i++)
				{
					lst.Add(new Vector3(pos2[i*3],pos2[i*3+1],pos2[i*3+2]));
				}
				MoleculeModel.atomsMDDriverLocationlist = lst;
			}
			
			List<GenericManager> managers = Molecule.View.DisplayMolecule.GetManagers();
			
			GenericManager atomManager = managers[0];
			atomManager.ResetMDDriverPositions();
			
			// Copied from DisplayMolecule.GetManagers
			// Does not work otherwise. Why?
			if (UIData.bondtype == UIData.BondType.line) {
				GameObject lineManagerObj = GameObject.FindGameObjectWithTag("LineManager");
				LineManager lineManager = lineManagerObj.GetComponent<LineManager>();
				lineManager.ResetPositions();
			} else if (UIData.bondtype == UIData.BondType.cube) {
				GameObject cubeBondManagerObj = GameObject.FindGameObjectWithTag("CubeBondManager");
				CubeBondManager cubeBondManager = cubeBondManagerObj.GetComponent<CubeBondManager>();
				cubeBondManager.ResetPositions();
			} else if (UIData.bondtype == UIData.BondType.hyperstick) {
				GameObject hStickManagerObj = GameObject.FindGameObjectWithTag("HStickManager");
				HStickManager hStickManager = hStickManagerObj.GetComponent<HStickManager>();
				hStickManager.ResetPositions();
			}
			
			if (MDDriver.hb)
			{
//				Debug.Log (RNAView.findHbonds().Count);
//				List<int[]> hbonds = RNAView.findHbonds();
//				Debug.Log (bonds.Count + " bonds found");
//				int sequenceLength = MoleculeModel.sequence.Count();
//				string structure = VARNA.generateStructureString(sequenceLength, hbonds);
//				VARNA.generateImage(MoleculeModel.sequence, structure, "test.png");
			}
		}
		else
		{
			Debug.Log("Not connected...");
		}

	}
	
	void OnDisable () {
		GUIMoleculeController.toggle_MDDRIVER = false;
		Debug.Log("Exiting sim Disable");
		Debug.Log("Stop: " + MDDriver_stop());
	}
	
	public static void MDDriverM(int a) {
		GUIMoleculeController.showMDDriverMenu = LoadTypeGUI.SetTitleExit("MDDriver Client");
		
		GameObject go = GameObject.FindGameObjectWithTag("MDDriver");
		
		if(go != null  && MDDriver_isConnected()) {
		
			GUILayout.BeginHorizontal();
			GUILayout.Label("Connected");
			GUILayout.EndHorizontal();
				
//			GUILayout.BeginHorizontal();
//			if (GUILayout.Button (new GUIContent("Disconnect", "Disconnect from simulation server"))) {
//				Debug.Log("Stop: " + MDDriver_stop());
//				GUIMoleculeController.toggle_MDDRIVER = false;
//			}
//			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button (new GUIContent("Hydrogen bonds", "Render or no hydrogen bonds"))) 
			{
				if (MDDriver.hb == false)
				{
					PlotManager.Instance.PlotCreate ("NHBonds", -1, 40, Color.black, new Vector2(500, 500));
				}
				GameObject bb = GameObject.Instantiate(Resources.Load ("VARNABillboard")) as GameObject;
				GameObject camera = GameObject.Find ("Camera");
				bb.transform.parent = camera.transform;
				MDDriver.hb = !MDDriver.hb;
			}
			GUILayout.EndHorizontal();
		}
		else
		{
			if (go != null)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Not connected");
				GUILayout.EndHorizontal();
			}
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Server");
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			host = GUILayout.TextField(host, 30);
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Port");
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			portString = GUILayout.TextField(portString, 30);
			GUILayout.EndHorizontal();
			
			int temp = 0;
			if (int.TryParse(portString, out temp))
    		{
        		port = Mathf.Clamp(temp, 0, 99999);
    		}
    		else if (portString == "")
			{
				port = 3000;
			}
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button (new GUIContent("Connect", "Connect to simulation server")))
			{
				instantiateClient();
			}
			GUILayout.EndHorizontal();
		}
		
		GUILayout.Space(20);
		
		if(UIData.atomtype == UIData.AtomType.hyperball) {
			GUILayout.BeginHorizontal();
			GameObject hbManagerObj = GameObject.FindGameObjectWithTag("HBallManager");
			HBallManager hbManager = hbManagerObj.GetComponent<HBallManager>();
			if (hbManager.ellipsoidViewEnabled()) 
			{
				if (GUILayout.Button (new GUIContent("CG RNA", "Switch to RNA Coarse-Grained representation")) && UIData.atomtype == UIData.AtomType.hyperball) {
					hbManager.SwitchRendering();
				}
			}
			else {
				if (GUILayout.Button (new GUIContent("Ellipsoid RNA", "Switch to RNA ellipsoid representation")) && UIData.atomtype == UIData.AtomType.hyperball) {
					hbManager.SwitchRendering();
				}
			}
			GUILayout.EndHorizontal();
		}
		
//		if (GUILayout.Button (new GUIContent("Benchmark", "")))
//		{
//			GameObject LoadBox;
//			LoadBox = GameObject.Find ("LoadBox");
//			Molecule3D Molecule3DComp = LoadBox.GetComponent<Molecule3D> ();
//			Molecule3DComp.toggleFPSLog ();
//		}
		
		GUI.enabled = true;
		GUI.DragWindow();
		
	}
	
	public static void instantiateClient() { 
		GameObject go = GameObject.FindGameObjectWithTag("MDDriver");
		if (go != null) {
			MDDriver_stop();
			GameObject.Destroy(go);
			GUIMoleculeController.toggle_MDDRIVER = false;
		}
		GameObject.Instantiate(Resources.Load("MDDriver/MDDriver"));
		GUIMoleculeController.toggle_MDDRIVER = true;
	}
}
