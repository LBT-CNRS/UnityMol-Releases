/// @file Molecule3D.cs
/// @brief Details to be specified
/// @author FvNano/LBT team
/// @author Marc Baaden <baaden@smplinux.de>
/// @date   2013-4
///
/// Copyright Centre National de la Recherche Scientifique (CNRS)
///
/// contributors :
/// FvNano/LBT team, 2010-13
/// Marc Baaden, 2010-13
///
/// baaden@smplinux.de
/// http://www.baaden.ibpc.fr
///
/// This software is a computer program based on the Unity3D game engine.
/// It is part of UnityMol, a general framework whose purpose is to provide
/// a prototype for developing molecular graphics and scientific
/// visualisation applications. More details about UnityMol are provided at
/// the following URL: "http://unitymol.sourceforge.net". Parts of this
/// source code are heavily inspired from the advice provided on the Unity3D
/// forums and the Internet.
///
/// This software is governed by the CeCILL-C license under French law and
/// abiding by the rules of distribution of free software. You can use,
/// modify and/or redistribute the software under the terms of the CeCILL-C
/// license as circulated by CEA, CNRS and INRIA at the following URL:
/// "http://www.cecill.info".
/// 
/// As a counterpart to the access to the source code and rights to copy, 
/// modify and redistribute granted by the license, users are provided only 
/// with a limited warranty and the software's author, the holder of the 
/// economic rights, and the successive licensors have only limited 
/// liability.
///
/// In this respect, the user's attention is drawn to the risks associated 
/// with loading, using, modifying and/or developing or reproducing the 
/// software by the user in light of its specific status of free software, 
/// that may mean that it is complicated to manipulate, and that also 
/// therefore means that it is reserved for developers and experienced 
/// professionals having in-depth computer knowledge. Users are therefore 
/// encouraged to load and test the software's suitability as regards their 
/// requirements in conditions enabling the security of their systems and/or 
/// data to be ensured and, more generally, to use and operate it in the 
/// same conditions as regards security.
///
/// The fact that you are presently reading this means that you have had 
/// knowledge of the CeCILL-C license and that you accept its terms.
///
/// $Id: Molecule3D.cs 672 2014-10-02 08:13:56Z tubiana $
///
/// References : 
/// If you use this code, please cite the following reference : 	
/// Z. Lv, A. Tek, F. Da Silva, C. Empereur-mot, M. Chavent and M. Baaden:
/// "Game on, Science - how video game technology may help biologists tackle
/// visualization challenges" (2013), PLoS ONE 8(3):e57990.
/// doi:10.1371/journal.pone.0057990
///
/// If you use the HyperBalls visualization metaphor, please also cite the
/// following reference : M. Chavent, A. Vanel, A. Tek, B. Levy, S. Robert,
/// B. Raffin and M. Baaden: "GPU-accelerated atom and dynamic bond visualization
/// using HyperBalls, a unified algorithm for balls, sticks and hyperboloids",
/// J. Comput. Chem., 2011, 32, 2924
///

using UnityEngine;
using System.Collections;
using System.Xml;
using System.Text;
using System;
using ParseData.ParsePDB;
using ParseData.IParsePDB;
using UI;
using Molecule.View;
//using System.Net.Sockets;
using System.Net;

using SocketConnect.UnitySocket;
using Cmd;
using DisplayControl;
using Config;
using Molecule.Model;
using System.IO;

public class Molecule3D:MonoBehaviour {

	private GameObject molecule;
	
	private bool fontInitialized = false;

	public float rotationSpeed = 100.0F;
	private GameObject[] Boxes;
	private GameObject[] boxes;
	private UnityEngine.Object cameraUser;
	private  GameObject LocCamera;
	private Light li;
	private float []defaultRanglesXZ;
	private float []defaultRanglesXY;
	public string url;
	public string myXml;
	private WWW xmlDownload;
	
	private StreamWriter fpsLog;
	private bool fpsLogToggle = false;
	private int fpsCount = 0;
	private float fpsSum = 0;

	private GameObject Target;
//	private Vector3 Deta;
//	private 	string textField="";
//	private string id="";
	public GUIDisplay gUIDisplay;
	private RequestPDB requestPDB=new RequestPDB();
//	private Boolean flag=false;
	private Boolean isControl=false;
	private IPAddress _ipAddr;
  	public 	IPEndPoint _ipep;
//  private Socket _nws = null;
	private  SocketPDB socketPDB;
//	private float rotationX=0f;
//	private float rotationY=0f;
//	private float rotationZ=0f;
//	private Vector3 axisX=new Vector3(1,0,0);
//	private Vector3 axisY=new Vector3(0,1,0);
//	private Vector3 axisZ=new Vector3(0,0,1);
//	private Vector3 newPosition=new Vector3(0,0,0);
	public float []atomsScaleList={1.72f,1.6f,1.32f,2.08f,2.6f,1.55f,1f};
	public ArrayList clubLocationalist=new ArrayList();
//	private ArrayList clubRotationList =new ArrayList();
	public float sensitivityX = 1.5F;
	public float sensitivityY = 1.5F;
	public float minimumX = -360F;
	public float maximumX = 360F;
	public float minimumY = -60F;
	public float maximumY = 60F;
	private float rotationXX = 0F;
	private float rotationYY = 0F;
//	Quaternion originalRotation;
	public string location;
    public Vector2 directoryScroll= new Vector2();
    public Vector2 fileScroll= new Vector2();

	public GUISkin mySkin;
	
	// video with Benoit
	public int polop = 0;
	public int masque = 0;
	
	private GameObject scenecontroller;
	
	private float updateInterval= 0.5f;

	private float accum= 0.0f; // FPS accumulated over the interval
	private float frames= 0; // Frames drawn over the interval
	private float timeleft; // Left time for current interval
	
	//T.T DEBUG
	public bool fileLoadingFinished=false;

//		private string FPS="";

	//To keep track of the normal type when hiding atoms or in LOD mode
    private UIData.AtomType previous_AtomType = UIData.AtomType.noatom;
    public UIData.AtomType PreviousAtomType {
    	get {
    		return previous_AtomType;
    	}
    }
	
	private UIData.BondType previous_BondType = UIData.BondType.nobond;
	public UIData.BondType PreviousBondType {
		get {
			return previous_BondType;
		}
	}

	void Awake() {
		System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");	
	}

	void Start() {		
		DebugStreamer.message = "Hello world!";
		LocCamera=GameObject.Find("Camera");
		DebugStreamer.message = "Find Camera";
		LocCamera.GetComponent<Skybox>().enabled=false;

		scenecontroller = GameObject.Find("LoadBox");
		scenecontroller.AddComponent<ReadDX>();
		
		gUIDisplay=new GUIDisplay();
		DebugStreamer.message = "new GUIDisplay()";
		//Init
		// DebugStreamer.message = "Find LoadBox";
		
//		originalRotation = transform.localRotation;
		//requestPDB.mySkin=mySkin;
		
		timeleft = updateInterval;
		
//		AtomModel.InitHiRERNA();
		AtomModel.InitAtomic();
		
		SendMessage("InitScene",requestPDB,SendMessageOptions.DontRequireReceiver);
	}

	public void Display() {
		DisplayMolecule.Display();
		DisplayMolecule.DisplayFieldLine();
//		Deta=MoleculeModel.target;
		isControl=true;

		GUIMoleculeController.InitMoleculeParameters();
		SetCenter(0);
	}

	public void HideAtoms() {
		if(UIData.atomtype != UIData.AtomType.noatom) {
			Debug.Log("Hiding all atoms now.");
			DisplayMolecule.HideAtoms();
			previous_AtomType = UIData.atomtype;
			UIData.atomtype = UIData.AtomType.noatom;
		}
	}

	public void ShowAtoms() {
		if(UIData.atomtype == UIData.AtomType.noatom) {
			UIData.atomtype = previous_AtomType;
			previous_AtomType = UIData.AtomType.noatom;
			DisplayMolecule.ShowAtoms();
		}
	}

	void OnGUI() {	
		GUI.skin = mySkin;
		
		if(!fontInitialized) {
			Rectangles.SetFontSize();
			fontInitialized = true;
		}
	
		if(GUIMoleculeController.m_fileBrowser != null)
			GUIMoleculeController.m_fileBrowser.OnGUI();



		if(gUIDisplay.m_fileBrowser != null) {
			GUIMoleculeController.FileBrowser_show=true;
			gUIDisplay.m_fileBrowser.OnGUI();
		} else
			GUIMoleculeController.FileBrowser_show=false;
		
		UIData.EnableUpdate=false;
		if((!UIData.hiddenUI)&&(!UIData.hiddenUIbutFPS))
			gUIDisplay.Display();
		
		if((!UIData.hiddenUI)&&(UIData.hiddenUIbutFPS)){
			GUIMoleculeController.toggle_INFOS = true;
		}
		
		if(!UIData.hiddenUI)
			if(GUIMoleculeController.showPanelsMenu)
				GUIMoleculeController.SetPanels();
		
		if(!UIData.hiddenUI)
			if (GUIMoleculeController.showResiduesMenu)
				GUIMoleculeController.SetResidues();
		
		if(!UIData.hiddenUI)
			if (GUIMoleculeController.showAtomsExtendedMenu)
				GUIMoleculeController.SetAtomsExtended();
		
		if(!UIData.hiddenUI)
			if (GUIMoleculeController.showChainsMenu)
				GUIMoleculeController.SetChains();

		if(UIData.changeStructure) {
			DisplayMolecule.ResetDisplay();
			UIData.changeStructure = false;
			UIData.isParticlesInitialized = false;
		}
		
		if(UIData.isclear) {
			DisplayMolecule.DestroyFieldLine();
			DisplayMolecule.DestroyObject();
			DisplayMolecule.DestroyRingBlending();
			DisplayMolecule.DestroySugarRibbons();
			DisplayMolecule.DestroyOxySpheres();
			DisplayMolecule.DestroyBondObject();
			DisplayMolecule.DestroySurfaces();
			DisplayMolecule.DestroyElectIso();
			DisplayMolecule.ClearMemory();
			
			// ----- Clearing all variables -----
			UIData.isCubeLoaded = false;
			UIData.isSphereLoaded = false;
			UIData.isHBallLoaded = false;
			LoadTypeGUI.buildSurfaceDone = false;
			LoadTypeGUI.surfaceTextureDone = false;
			LoadTypeGUI.toggle_RING_BLENDING = false;
			LoadTypeGUI.toggle_NA_HIDE = false;
			LoadTypeGUI.toggle_TWISTER= false;
			LoadTypeGUI.toggle_HIDE_HYDROGEN = false;
			LoadTypeGUI.toggle_OXYGEN = false;
			LoadTypeGUI.ColorationModeBond=0;
			LoadTypeGUI.ColorationModeRing=0;
			UIData.isParticlesInitialized=false;
			GUIMoleculeController.globalRadius = 1.0f;
			UIData.secondarystruct = false;
			UIData.atomtype = UIData.AtomType.noatom;
			UIData.bondtype = UIData.BondType.nobond;
			MoleculeModel.existingName.Clear();
			MoleculeModel.existingRes.Clear();
			MoleculeModel.existingChain.Clear();
//			id="";
			//T.T test debug
			Molecule.Model.MoleculeModel.atomsLocalScaleList.Clear();
			RequestPDB.isDone=false;
			
			UIData.isclear=false;
			Debug.Log("UIData.isclear");
		}
		
		if(UIData.resetDisplay&&UIData.isCubeToSphere) {
			DisplayMolecule.CubeToSphere();
			Debug.Log ("UIData :: resetDisplay && iscubetoSphere");
		}
		
		if(UIData.resetDisplay&&UIData.isSphereToCube) {
			DisplayMolecule.SphereToCube();
			Debug.Log ("UIData :: reset display && is spheretocube");
		}
		
		if(UIData.resetBondDisplay) {
			DisplayMolecule.ResetBondDisplay();
			Debug.Log ("UIData :: reset bonddisplay ");
		}
		
		if(UIData.isOpenFile) {	
			StartCoroutine(loadLoadFile());
		}
		
		if(UIData.backGroundIs)
			LocCamera.GetComponent<Skybox>().enabled=true;
		else
			LocCamera.GetComponent<Skybox>().enabled=false;

		UIData.EnableUpdate=true;
		
		if(UIData.interactive&&UIData.resetInteractive)	{
			DisplayMolecule.AddAllPhysics();
			UIData.resetInteractive=false;			
		}
		else if(!UIData.interactive && UIData.resetInteractive) {
			DisplayMolecule.DeleteAllPhysics();
			UIData.resetInteractive = false;
		}
		
		if(UIData.meshcombine) {
			DisplayMolecule.AddCombineMesh();
			UIData.resetMeshcombine=false;			
		}
		else if(!UIData.meshcombine) {
			DisplayMolecule.DeleteCombineMesh();
			UIData.resetMeshcombine=false;			
		}
		
		/*if (requestPDB.Loading) {
            	GUI.Label(new Rect(100, 15, 200, 30), "", "bj");
            	GUI.Label(new Rect(100,15, requestPDB.progress * 200, 30), "", "qj");
        }*/

//		if(GUI.tooltip != "")GUI.Label ( new Rect(180,Screen.height-35,Screen.width-360,20), GUI.tooltip);
//		if(MoleculeModel.newtooltip != "")GUI.Label ( new Rect(180,Screen.height-35,Screen.width-360,20), MoleculeModel.newtooltip);
		if(GUI.tooltip != "")GUI.Box ( new Rect(180,Screen.height-55,450,25), GUI.tooltip);
		if(MoleculeModel.newtooltip != "")GUI.Box ( new Rect(180,Screen.height-55,450,25), MoleculeModel.newtooltip);


	}
	
	//this fonction is used to synchronise the file loading and the Display
	//Otherwise Display is execute before the end of the loading.
	public IEnumerator loadLoadFile(){
			UIData.isOpenFile = false;
			yield return StartCoroutine(loadFile());
			Debug.Log ("T.T ==> BEFORE DISPLAY");
			Display();		
	}
	
	
	// loading the file in all possibilities
	public  IEnumerator loadFile() {
		#if !UNITY_WEBPLAYER
		{
//				alist=requestPDB.LoadPDBRequest(url,id);
		
		// check all format reading by unitymol PDB, XGMML and OBJ
			if(UIData.fetchPDBFile) {
				Debug.Log("pdbServer/pdbID :: "+GUIDisplay.pdbServer + GUIDisplay.pdbID);
				Debug.Log("proxyServer+proxyPort :: "+GUIDisplay.proxyServer + GUIDisplay.proxyPort);
				int proxyport = 0;
				if(GUIDisplay.proxyPort != "")
					proxyport = int.Parse(GUIDisplay.proxyPort);
				else
					proxyport = 0;

				requestPDB.FetchPDB(GUIDisplay.pdbServer, GUIDisplay.pdbID, GUIDisplay.proxyServer, proxyport);
			}
		// if we laod a pdb file launch the reading of file
			else if(GUIDisplay.file_extension=="pdb")
				requestPDB.LoadPDBRequest(GUIDisplay.file_base_name);

		// check the format of xgmml	
			else if(UI.GUIDisplay.file_extension=="xgmml") {
					yield return StartCoroutine(requestPDB.LoadXGMML("file://"+GUIDisplay.file_base_name+"."+GUIDisplay.file_extension));
					while(!RequestPDB.isDone) {
						Debug.Log(requestPDB.progress);
						yield return new WaitForEndOfFrame();
					}
					UIData.atomtype=UIData.AtomType.hyperball;
					UIData.bondtype=UIData.BondType.hyperstick;
					GUIMoleculeController.globalRadius = 0.22f;
					GUIMoleculeController.shrink = 0.0001f;
					GUIMoleculeController.linkScale = 0.70f;
					SendMessage("Display",SendMessageOptions.DontRequireReceiver);
			}
			else if(UI.GUIDisplay.file_extension=="obj") {
					requestPDB.LoadOBJRequest(GUIDisplay.file_base_name+"."+GUIDisplay.file_extension);
					MoleculeModel.surfaceFileExists=true;
					GUIMoleculeController.modif=true;
			}	
//				requestPDB.GetTypelist();
		}
		//if the application is an wep player or a mobile application
		#else
		{
			
// 			socketPDB=new SocketPDB(id);
// 			socketPDB.getAtoms();
// //				socketPDB.getTypes();
// 			clubLocationalist=socketPDB.getClubLocation();
// 			clubRotationList=socketPDB.getClubRotation();
			if(UIData.init_molecule != "")
				requestPDB.LoadPDBResource(UIData.init_molecule);
		}			
		#endif
		//Debug.Log("SDGFSDGSDGDSG");
		GUIMoleculeController.showAtomMenu = true;
		Camera.main.GetComponent<SplashScreen>().enabled = false;
		Debug.Log("T.T ==> END OF LOADING");
}

	// Update is called once per frame
	void Update() {
		// requestPDB.Loading looks actually useless since progress bar is commented (see below)
		/*if (RequestPDB.isDone){
            requestPDB.Loading = false;
		}
        else{
            requestPDB.Loading = true;
		}*/
		
		if(isControl&&(!UIData.cameraStop2)) { // Seems to be true as long as the mouse is in the game window and not on the gui
			MouseOperate();
			KeyOperate();
			
			//SetCenterbySpace();
			HiddenOperate();
			OpenMenuOperate();
			OpenBoundOperate();
		}
		
		// Always false ?
		/*if (GUIMoleculeController.toggle_HB_SANIM) {
			GUIMoleculeController.shrink +=  Time.deltaTime * GUIMoleculeController.hb_sanim * GUIMoleculeController.hb_ssign;
			if (GUIMoleculeController.shrink > 0.95f )
				GUIMoleculeController.hb_ssign = -1.0f;
			if (GUIMoleculeController.shrink < 0.05f )
				GUIMoleculeController.hb_ssign = 1.0f;
		}*/
		
		// Always false ?
		/*if (GUIMoleculeController.toggle_HB_RANIM) {
			GUIMoleculeController.globalRadius +=  Time.deltaTime * GUIMoleculeController.hb_ranim * GUIMoleculeController.hb_rsign;
			if (GUIMoleculeController.globalRadius > 0.95f )
				GUIMoleculeController.hb_rsign = -1.0f;
			if (GUIMoleculeController.globalRadius < 0.05f )
				GUIMoleculeController.hb_rsign = 1.0f;
		}*/
	
		if(GUIMoleculeController.toggle_HB_TRANS) // Always true ?
			GUIMoleculeController.transDelta = 25.0f;
		else
			GUIMoleculeController.transDelta = 1.0f;
	
		if (GUIMoleculeController.transMETAPHOR) {
			GUIMoleculeController.globalRadius = transition(GUIMoleculeController.globalRadius, GUIMoleculeController.newGlobalRadius, GUIMoleculeController.deltaRadius);
			GUIMoleculeController.linkScale = transition(GUIMoleculeController.linkScale, GUIMoleculeController.newScale, GUIMoleculeController.deltaScale);
			GUIMoleculeController.shrink = transition(GUIMoleculeController.shrink, GUIMoleculeController.newShrink, GUIMoleculeController.deltaShrink);
			if(GUIMoleculeController.globalRadius == GUIMoleculeController.newGlobalRadius && GUIMoleculeController.linkScale == GUIMoleculeController.newScale && GUIMoleculeController.shrink == GUIMoleculeController.newShrink) 			
				GUIMoleculeController.transMETAPHOR = false;
		}
		
		LineUpdate.scale=GUIMoleculeController.linkScale;
		
		StickUpdate.radiusFactor = GUIMoleculeController.globalRadius;
		StickUpdate.shrink      = GUIMoleculeController.shrink;
		StickUpdate.scale 		= GUIMoleculeController.linkScale;
		BallUpdateHB.radiusFactor = GUIMoleculeController.globalRadius;
//		BallUpdateHB.depthfactor = GUIMoleculeController.depthfactor;
		HBallManager.depthFactor = GUIMoleculeController.depthfactor;
		HStickManager.depthFactor = GUIMoleculeController.depthfactor;
		BallUpdateSphere.radiusFactor = GUIMoleculeController.globalRadius;
		BallUpdateCube.radiusFactor = GUIMoleculeController.globalRadius;
		BallUpdateRC.radiusFactor = GUIMoleculeController.globalRadius;
		
		BallUpdateHB.drag = GUIMoleculeController.drag;
		BallUpdateHB.spring = GUIMoleculeController.spring;
		
		BallUpdateHB.EnergyGrayColor = GUIMoleculeController.EnergyGrayColor.color;		
		
		// TODO: This is gross. Should be fixed.
		GameObject[] FieldLines = GameObject.FindGameObjectsWithTag("FieldLineManager");
		foreach (GameObject FieldLine in FieldLines) {
			LineRenderer curLineRenderer;
        	curLineRenderer = FieldLine.GetComponent<LineRenderer>();
			curLineRenderer.material.SetFloat("_timeOff",Time.time);
			
			// for benoist video comment next line
			curLineRenderer.material.SetColor("_Color", GUIMoleculeController.EnergyGrayColor.color);
			
			if (GUIMoleculeController.fieldLineColorGradient)
				curLineRenderer.material.SetFloat("_colormode", 0f);
			else
				curLineRenderer.material.SetFloat("_colormode", 1f);

			curLineRenderer.material.SetFloat("_Speed",GUIMoleculeController.speed);
			curLineRenderer.material.SetFloat("_Density",GUIMoleculeController.density);
			curLineRenderer.material.SetFloat("_Length", GUIMoleculeController.linelength);
			curLineRenderer.SetWidth(GUIMoleculeController.linewidth,GUIMoleculeController.linewidth);
			curLineRenderer.material.SetFloat("_depthcut", (GUIMoleculeController.depthCut-maxCamera.currentDistance));
			curLineRenderer.material.SetFloat("_adjust",(GUIMoleculeController.adjustFieldLineCut));
			curLineRenderer.material.SetVector("_SurfacePos", FieldLine.transform.position);

			if (GUIMoleculeController.surfaceMobileCut)
				curLineRenderer.material.SetFloat("_cut", 2f);
			else if ( GUIMoleculeController.surfaceStaticCut){
				curLineRenderer.material.SetFloat("_cut", 1f);
				curLineRenderer.material.SetVector("_cutplane",new Vector4(GUIMoleculeController.cutX,
																			GUIMoleculeController.cutY,
																			GUIMoleculeController.cutZ,
																			GUIMoleculeController.depthCut));
			}
		}

		GameObject[] Surfaces = GameObject.FindGameObjectsWithTag("SurfaceManager");
			
		foreach (GameObject Surface in Surfaces) {
			
			if ((GUIMoleculeController.surfaceTexture || GUIMoleculeController.externalSurfaceTexture) && !GUIMoleculeController.surfaceTextureDone) {
				if(GUIMoleculeController.externalSurfaceTexture){
					if(!UIData.grayscalemode)
						Surface.GetComponent<Renderer>().material.SetTexture("_MatCap",GUIMoleculeController.extSurf);
					else{
						GameObject hbManagerObj = GameObject.FindGameObjectWithTag("HBallManager");
						HBallManager hbManager = hbManagerObj.GetComponent<HBallManager>();
						Surface.GetComponent<Renderer>().material.SetTexture("_MatCap",hbManager.ToGray(GUIMoleculeController.extSurf));
					}
					Debug.Log("File choose surface texture");
				}
				else{
					if(!UIData.grayscalemode)
						Surface.GetComponent<Renderer>().material.SetTexture("_MatCap",(Texture)Resources.Load(GUIMoleculeController.surfaceTextureName)); // do not do that every frame!
					else{
						GameObject hbManagerObj = GameObject.FindGameObjectWithTag("HBallManager");
						HBallManager hbManager = hbManagerObj.GetComponent<HBallManager>();
						Surface.GetComponent<Renderer>().material.SetTexture("_MatCap",hbManager.ToGray((Texture)Resources.Load(GUIMoleculeController.surfaceTextureName)));
					}
					Debug.Log("Quick choose surface texture");
				}
			}
			else if ((GUIMoleculeController.buildSurface || GUIMoleculeController.dxRead) && !GUIMoleculeController.buildSurfaceDone) {
				Surface.GetComponent<Renderer>().material.SetTexture("_MatCap",(Texture)Resources.Load("lit_spheres/divers/daphz1"));
				Debug.Log("Default surface texture");
			}
			
			// send all the paramter to the surface shader
			// Surface.renderer.material.SetFloat("_Shininess", GUIMoleculeController.intensity);
			// if (Input.GetKey("n")) // uncoment for benoist
			
			Surface.GetComponent<Renderer>().material.SetColor("_Color", GUIMoleculeController.SurfaceGrayColor.color);
			Surface.GetComponent<Renderer>().material.SetColor("_ColorIN", GUIMoleculeController.SurfaceInsideColor.color);
			//Surface.renderer.material.SetColor("_Color", new Color(1f,1f,1f)); // couleur blanche fixé
			Surface.GetComponent<Renderer>().material.SetFloat("_depthcut", GUIMoleculeController.depthCut);
			Surface.GetComponent<Renderer>().material.SetFloat("_cutX", GUIMoleculeController.cutX);
			Surface.GetComponent<Renderer>().material.SetFloat("_cutY", GUIMoleculeController.cutY);
			Surface.GetComponent<Renderer>().material.SetFloat("_cutZ", GUIMoleculeController.cutZ);
			Surface.GetComponent<Renderer>().material.SetVector("_SurfacePos", Surface.transform.position);
			
			if (GUIMoleculeController.surfaceMobileCut && Surface.GetComponent<Renderer>().material.shader.name == "Mat Cap Cut"){	// set the cutting mode
				if(Surface.GetComponent<Renderer>().material.GetFloat("_cut") != 2f)
					Surface.GetComponent<Renderer>().material.SetFloat("_cut", 2f);
			}
			else if (GUIMoleculeController.surfaceStaticCut && Surface.GetComponent<Renderer>().material.shader.name == "Mat Cap Cut"){
				if(Surface.GetComponent<Renderer>().material.GetFloat("_cut") != 1f)
					Surface.GetComponent<Renderer>().material.SetFloat("_cut", 1f);
			}
			else if(Surface.GetComponent<Renderer>().material.shader.name == "Mat Cap Cut"){
				if(Surface.GetComponent<Renderer>().material.GetFloat("_cut") != 0f)
					Surface.GetComponent<Renderer>().material.SetFloat("_cut", 0f);
			}
		}
		GUIMoleculeController.surfaceTextureDone = true;
		GUIMoleculeController.buildSurfaceDone = true;

		//FPS Count
		
		timeleft -= Time.deltaTime;
		accum += Time.timeScale/Time.deltaTime;
		++frames;

		// Interval ended - update GUI text and start new interval
		if( timeleft <= 0.0f ) {
		// display two fractional digits (f2 format)
			float fps = accum/frames;//(1 / Time.deltaTime);
			MoleculeModel.FPS = fps.ToString("f2");
			//Write FPS data into file
			if(fpsLogToggle) {
				fpsCount ++;
				fpsSum += fps;
				if(fpsCount > 35) {
					Debug.Log("Info :; End fps measure");
					toggleFPSLog();
					fpsCount = 0;
					fpsSum = 0;
					GameObject LoadBox=GameObject.Find("LoadBox");
					maxCamera comp = LoadBox.GetComponent<maxCamera>();
					comp.automove = false;
				}
			}
			timeleft = updateInterval;
			accum = 0.0f;
			frames = 0;
		}
		
		//SetVolumetricDensity();
		// gUIDisplay.gUIMoleculeController.GetPanelPixel();
	}
	
	public void toggleFPSLog() { // Debugging tool creating .txt files with FPS informations
/*		if(!fpsLogToggle) {
			fpsLogToggle = true;
			Debug.Log("Entering :: Starting fps measure to file");
		}
		else {
			fpsLogToggle = false;
			DateTime currTime = DateTime.Now;
			string filename = currTime.ToString("HH_mm_ss")+"_umol_fpsdata.txt";
			fpsLog = new StreamWriter(filename);	
			fpsLog.WriteLine(fpsCount.ToString());
			fpsLog.WriteLine( (fpsSum/fpsCount).ToString() );
			fpsLog.Close();
			fpsLog.Dispose();
		}
*/
	}	
	
	/// <summary>
	/// Make the transition between metaphors.
	/// </summary>
	/// <param name='val'>
	/// Value.
	/// </param>
	/// <param name='newVal'>
	/// New value.
	/// </param>
	/// <param name='deltaVal'>
	/// Delta value.
	/// </param>
	float transition(float val, float newVal, float deltaVal) {
		if(val <= newVal && deltaVal < 0.0f) return newVal;
		if(val >= newVal && deltaVal > 0.0f) return newVal;
		return val + deltaVal;	
	}
	
	/// <summary>
	/// Switch back to the particle mode. ('Delete' key)
	/// </summary>
	private void OpenMenuOperate() {
		if(Input.GetKeyDown(KeyCode.Delete)) {
				UIData.openAllMenu=!UIData.openAllMenu; // ???
				UIData.resetDisplay=true;
				UIData.isSphereToCube=true;
				UIData.isCubeToSphere=false;
				UIData.atomtype=UIData.AtomType.particleball;
				UIData.resetBondDisplay=true;
				UIData.bondtype=UIData.BondType.nobond;
		}
	}
	
	/// <summary>
	/// Switch back to particle mode. ('-' or '=' keys)
	/// </summary>
	private void OpenBoundOperate() {
		if(Input.GetKeyDown(KeyCode.Minus)||Input.GetKeyDown(KeyCode.Equals)) {

			/*
				Debug.Log("Press Equal key.");
				UIData.openBound=!UIData.openBound; // ???
				UIData.resetDisplay=true;
				UIData.isSphereToCube=true;
				UIData.isCubeToSphere=false;
				UIData.atomtype=UIData.AtomType.particleball;
				UIData.resetBondDisplay=true;
				UIData.bondtype=UIData.BondType.nobond;
			*/
		}
	}
	
	/// <summary>
	/// Hides the GUI and enables a sort of "full-screen" mode, as in GUI-less, not as opposed to windowed.
	/// Helps quite a bit with performance, or at least with CPU load.
	/// </summary>
	private void HiddenOperate() {
		if(Input.GetKeyDown(KeyCode.Backspace))	{
			if(!UIData.hiddenUI) { //&& !UIData.hiddenUIbutFPS && !UIData.hiddenCamera) {
					UIData.hiddenUI=true;
					Debug.Log("Hide all the UI.");
			} 
			// I really don't know why we'd want to disable the camera.
/*			else if(UIData.hiddenUI && !UIData.hiddenUIbutFPS && !UIData.hiddenCamera) {
					UIData.hiddenCamera=true;
					LocCamera.GetComponent<Camera>().enabled=false;
					Debug.Log("Hide all the UI and Camera.");
			}
*/
			// Doesn't seem to work
/*			else if(UIData.hiddenUI && !UIData.hiddenUIbutFPS) { //&& UIData.hiddenCamera) {
					//UIData.hiddenCamera=false;
					//LocCamera.GetComponent<Camera>().enabled=true;
					UIData.hiddenUI=false;
					UIData.hiddenUIbutFPS=true;
					Debug.Log("Hide all the UI except FPS."); 
			}
*/
			else if(UIData.hiddenUI) { //!UIData.hiddenUI && UIData.hiddenUIbutFPS && !UIData.hiddenCamera) {
					UIData.hiddenUI=false;
					//UIData.hiddenUIbutFPS=false;		
					Debug.Log("Show all the UI and Camera.");
			}	
		}
	}

	/// <summary>
	/// For keyboard control.
	/// </summary>
	private void KeyOperate() {	
		Vector3 v=new Vector3();
		v=LocCamera.transform.localPosition;
		
		//Molecule right
		if(Input.GetKey(KeyCode.D))	{
			v.x-=0.5f;
			LocCamera.transform.localPosition=v;
			if(UIData.switchmode)
				ToParticle();
		}
		//Molecule up
		if(Input.GetKey(KeyCode.W)) {
			v.y-=0.5f;
			LocCamera.transform.localPosition=v;
			if(UIData.switchmode)
				ToParticle();
		}
		//Molecule down
		if(Input.GetKey(KeyCode.S)) {
			v.y+=0.5f;
			LocCamera.transform.localPosition=v;
			if(UIData.switchmode)
				ToParticle();
			//print("LocCamera.transform.localPosition.y"+v.y);
		}
		//Molecule left
		if(Input.GetKey(KeyCode.A)) {
			v.x+=0.5f;
			LocCamera.transform.localPosition=v;
			if(UIData.switchmode)
				ToParticle();
			//print("LocCamera.transform.localPosition.x"+v.x);
		}
		//Zoom in
		if(Input.GetKey(KeyCode.N)) {
			v.z+=0.5f;
			LocCamera.transform.localPosition=v;
			if(UIData.switchmode)
				ToParticle();
			//print("LocCamera.transform.localPosition.x"+v.x);
		}
		//Zoom out
		if(Input.GetKey(KeyCode.B)) {
			v.z-=0.5f;
			LocCamera.transform.localPosition=v;
			if(UIData.switchmode)
				ToParticle();
			//print("LocCamera.transform.localPosition.x"+v.x);
		}
		
/*		//Moved to maxCamera
		//Down rotation
		if(Input.GetKey(KeyCode.Q)) {
			//LocCamera.transform.RotateAround(Deta,axisX,0.6f);	
			//DMatrix.RotationMatrix(axisX,axisY,axisZ,0.6f);
			if(UIData.switchmode)
				ToParticle();
		}
		//Up rotation
		if(Input.GetKey(KeyCode.E)) {
			//LocCamera.transform.RotateAround(Deta,axisX,-0.6f);		
			//DMatrix.RotationMatrix(axisX, axisY, axisZ,-0.6f);
			if(UIData.switchmode)
				ToParticle();
		}
		//Right rotation
		if(Input.GetKey(KeyCode.Z)) {
			//LocCamera.transform.RotateAround(Deta,axisZ,0.6f);
			//DMatrix.RotationMatrix(axisZ,axisY, axisX,0.6f);
			if(UIData.switchmode)
				ToParticle();
		}
		//Left rotation
		if(Input.GetKey(KeyCode.X)) {
			//LocCamera.transform.RotateAround(Deta,axisZ,-0.6f);
			//DMatrix.RotationMatrix(axisZ, axisY,axisX,-0.6f);
			if(UIData.switchmode)
				ToParticle();
		}
*/
		if(Input.GetKeyUp(KeyCode.D))
			if(UIData.switchmode)
				ToNotParticle();

		if(Input.GetKeyUp(KeyCode.W))
			if(UIData.switchmode)
				ToNotParticle();

		if(Input.GetKeyUp(KeyCode.S))
			if(UIData.switchmode)
				ToNotParticle();
		
		if(Input.GetKeyUp(KeyCode.A))
			if(UIData.switchmode)
				ToNotParticle();
		
		if(Input.GetKeyUp(KeyCode.N))
			if(UIData.switchmode)
				ToNotParticle();

		if(Input.GetKeyUp(KeyCode.B))
			if(UIData.switchmode)
				ToNotParticle();
/*		
		if(Input.GetKeyUp(KeyCode.Q))
			if(UIData.switchmode)
				ToNotParticle();
		
		if(Input.GetKeyUp(KeyCode.E))
			if(UIData.switchmode)
				ToNotParticle();
		
		if(Input.GetKeyUp(KeyCode.Z))
			if(UIData.switchmode)
				ToNotParticle();
		
		if(Input.GetKeyUp(KeyCode.X))
			if(UIData.switchmode)
				ToNotParticle();
*/		
		if(Input.GetKeyUp(KeyCode.RightArrow))
			if(UIData.switchmode)
				ToNotParticle();
		
		if(Input.GetKeyUp(KeyCode.LeftArrow))
			if(UIData.switchmode)
				ToNotParticle();

        if(Input.GetKey("joystick button 3")) {
            UIData.resetDisplay=true;
            UIData.isCubeToSphere=false;
            UIData.isSphereToCube=true;
            UIData.atomtype=UIData.AtomType.cube;
        }
        
        if(Input.GetKey("joystick button 2")) {
            UIData.resetDisplay=true;
            UIData.isSphereToCube=false;
            UIData.isCubeToSphere=true;
            UIData.atomtype=UIData.AtomType.sphere;
        }
        
        if(Input.GetKey("joystick button 0")) {
            UIData.resetDisplay=true;
            UIData.isCubeToSphere=false;
            UIData.isSphereToCube=true;
            UIData.atomtype=UIData.AtomType.hyperball;
        }
        
        if(Input.GetKey("joystick button 1")) {
            UIData.resetDisplay=true;
            UIData.isSphereToCube=true;
            UIData.isCubeToSphere=false;
            UIData.atomtype=UIData.AtomType.particleball;
            UIData.resetBondDisplay=true;
            UIData.bondtype=UIData.BondType.nobond;
        }
		
		// Takes a screenshot of the scene
		if(Input.GetKeyDown(KeyCode.P)) {
			ScreenShot comp = LocCamera.GetComponent<ScreenShot> ();
			comp.open = true;
		}
		
		Vector3 vv=new Vector3();
		vv=LocCamera.transform.localPosition;		
		
		if(!GUIMoleculeController.toggle_NA_MAXCAM)
			vv.z+=Input.GetAxis("Mouse ScrollWheel")*5;

		LocCamera.transform.localPosition=vv;		
	}
	
	/// <summary>
	/// Sets the center of the scene on :
	/// The original center ('R' key)
	/// The targeted atom ('T' key)
	/// </summary>
	/// 
	/* replaced by R and C in maxCamera.
	private void SetCenterbySpace() {
		if(Input.GetKeyUp(KeyCode.R)) {
			Debug.Log("Press the R key");
			SetCenter(0);
		}
		replace by touch C in maxCamera
	  if(Input.GetKeyUp(KeyCode.T)) {
			Debug.Log("Press the T key");
			SetCenter(1);
		}
	}*/

// controlled when maxcam is desactivate with the mouse =============================================================================================		
	/// <summary>
	/// Camera controls with mouse inputs.
	/// </summary>
	private void MouseOperate() {
		Vector3 v=new Vector3();
		v=LocCamera.transform.localPosition;
	
		if(!GUIMoleculeController.toggle_NA_MAXCAM) {
//			if (Input.GetMouseButton(1) )
//				v=LocCamera.transform.localPosition;

			if (Input.GetMouseButton(0)) {
				if(UIData.switchmode)ToParticle();
				rotationXX += Input.GetAxis("Mouse X") * sensitivityX;
				rotationYY += Input.GetAxis("Mouse Y") * sensitivityY;
				print("Mouse X"+Input.GetAxis("Mouse X"));
				print("Mouse Y"+Input.GetAxis("Mouse Y"));
				print("rotationXX"+rotationXX);
				print("rotationYY"+rotationYY);
		
				Quaternion xQuaternion = Quaternion.AngleAxis (rotationXX, Vector3.up);
				Quaternion yQuaternion = Quaternion.AngleAxis (rotationYY, Vector3.left);
				transform.localRotation =  xQuaternion * yQuaternion;
//				transform.localRotation = originalRotation * xQuaternion * yQuaternion;
				if(UIData.switchmode)ToNotParticle();
			}
			
			if(Input.GetMouseButtonUp(1))
				if(UIData.switchmode)ToNotParticle();
			
			v.z+=Input.GetAxis("Mouse ScrollWheel")*5;
			LocCamera.transform.localPosition=v;
			Debug.Log ("get mouse: " +v.z);				
		}
	}

	/// <summary>
	/// Sets the center of the scene on the original center or on an atom.
	/// </summary>
	/// <param name='mode'>
	/// Setting mode (0 for original center, 1 for atom center). Int.
	/// </param>
	private void SetCenter( int mode) {
		GameObject CamTarget = GameObject.Find("Cam Target");
	
		// choose the main function 0 to restart position or 1 to center around an atom
		if (mode ==	0) {
			Debug.Log("Entering :: SetCenter for cam target to" + MoleculeModel.cameraLocation.z);
			if(scenecontroller.GetComponent<maxCamera>().enabled) {
				maxCamera comp = scenecontroller.GetComponent<maxCamera>();
				comp.ToCenter();
			}
			if(UIData.atomtype == UIData.AtomType.hyperball){
				GameObject hbManagerObj = GameObject.FindGameObjectWithTag("HBallManager");
				HBallManager hbManager = hbManagerObj.GetComponent<HBallManager>();
				hbManager.ResetPositions();
			}
		} else if (mode ==1) {
			Debug.Log("target : " +MoleculeModel.target);
			CamTarget.transform.rotation = transform.rotation;
			CamTarget.transform.position = MoleculeModel.target;
		}
	}
	
	/// <summary>
	/// Sets the volumetric density.
	/// </summary>
	public void SetVolumetricDensity () {
		if( (GUIMoleculeController.showVolumetricDensity || GUIMoleculeController.showVolumetricFields) && !UIData.hasMoleculeDisplay) {
			GameObject volumObj;
			volumObj = GameObject.FindGameObjectWithTag("Volumetric");
			Volumetric volumetric;
			volumetric = volumObj.GetComponent<VolumetricDensity>();
			if (volumetric)
				volumetric.Clear();
			volumetric = volumObj.GetComponent<VolumetricFields>();
			if (volumetric)
				volumetric.Clear();
			GUIMoleculeController.showVolumetricDensity = false;
			GUIMoleculeController.showVolumetricFields = false;
		}
	}
	
	
	/// <summary>
	/// Switch the protein representation to Hyperball. Used in switch mode (LOD).
	/// </summary>
	public void ToNotParticle() {
		if(UIData.atomtype != UIData.AtomType.particleball && UIData.atomtype != previous_AtomType) {
			previous_AtomType = UIData.atomtype;
			previous_BondType = UIData.bondtype;
		}
		DisplayMolecule.ToNotParticle(previous_AtomType, previous_BondType);
	}
	
	/// <summary>
	/// Switch the protein representation to Particle. Used in switch mode (LOD).
	/// </summary>
	public void ToParticle() {
		if(UIData.atomtype != UIData.AtomType.particleball) {
			previous_AtomType = UIData.atomtype;
			previous_BondType = UIData.bondtype;
		}
		DisplayMolecule.ToParticle();
		// Debug.Log("ToParticle()");
	}
}

