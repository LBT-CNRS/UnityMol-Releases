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
/// $Id: Molecule3D.cs 247 2013-04-07 20:38:19Z baaden $
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
using System.Net.Sockets;
using System.Net;

using SocketConnect.UnitySocket;
using Cmd;
using DisplayControl;
using Config;
using Molecule.Model;
using System.IO;
//using BehaveLibrary;
public class Molecule3D:MonoBehaviour
{

	// Use this for initialization

	private GameObject molecule;

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


//	private float speed=0.0001f;
//	private float rangle0=0f;
//	private float rangle1=0f;
//	private Vector3 vr=new Vector3(0f,0f,0f);
	private GameObject Target;
	private Vector3 Deta;
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
	private Vector3 axisX=new Vector3(1,0,0);
	private Vector3 axisY=new Vector3(0,1,0);
	private Vector3 axisZ=new Vector3(0,0,1);
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

//		private string FPS="";

	//To keep track of the normal type when hiding atoms or in LOD mode
    private UIData.AtomType previous_AtomType = UIData.AtomType.noatom;
    public UIData.AtomType PreviousAtomType
    {
    	get
    	{
    		return previous_AtomType;
    	}
    }

	void Awake()
	{
		System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");	
	}

	void Start()
	{		
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
		// AtomModel.InitHiRERNA();
		AtomModel.InitAtomic();
		SendMessage("InitScene",requestPDB,SendMessageOptions.DontRequireReceiver);
	}

	public void Display()
	{
//		Debug.Log("Display display");
		DisplayMolecule.Display();
		DisplayMolecule.DisplayFieldLine();
		Deta=MoleculeModel.target;
		isControl=true;

		GUIMoleculeController.InitMoleculeParameters();
		
		SetCenter(0);
	}

	public void HideAtoms()
	{
		if(UIData.atomtype != UIData.AtomType.noatom)
		{
			previous_AtomType = UIData.atomtype;
			UIData.atomtype = UIData.AtomType.noatom;
			DisplayMolecule.HideAtoms();
		}
	}

	public void ShowAtoms()
	{
		if(UIData.atomtype == UIData.AtomType.noatom)
		{
			UIData.atomtype = previous_AtomType;
			previous_AtomType = UIData.AtomType.noatom;
		}
		DisplayMolecule.ShowAtoms();
	}
	
	void OnGUI()
	{	
		
		GUI.skin = mySkin;
		if(GUIMoleculeController.m_fileBrowser != null)
		{
			GUIMoleculeController.m_fileBrowser.OnGUI();
		}
		
		if(gUIDisplay.m_fileBrowser != null)
		{
			GUIMoleculeController.FileBrowser_show=true;
			gUIDisplay.m_fileBrowser.OnGUI();
		}else
			GUIMoleculeController.FileBrowser_show=false;
		
		UIData.EnableUpdate=false;
		if((!UIData.hiddenUI)&&(!UIData.hiddenUIbutFPS))
		{
			gUIDisplay.Display();
		}
		
		if(!UIData.hiddenUI)
		{ 
			if (GUIMoleculeController.SetAtomScale_show){
				gUIDisplay.SetAtomScale();
				

			}
		}

		if(UIData.isConfirm && UIData.hasMoleculeDisplay)
		{
			// if(UIData.atomtype == UIData.AtomType.particleball)
			// {	
			// 	DisplayMolecule.ResetDisplay();
			// }
			// else
			{
				DisplayMolecule.AmendDisplay();
			}
			UIData.isConfirm=false;
		}

		if(UIData.changeStructure)
		{
			DisplayMolecule.ResetDisplay();
			UIData.changeStructure = false;
		}
		
		if(UIData.isclear)
		{
			DisplayMolecule.DestroyFieldLine();
			DisplayMolecule.DestroyObject();
			DisplayMolecule.DestroyParticles();
			DisplayMolecule.DestroyBondObject();
			DisplayMolecule.DestroySurfaces();
			DisplayMolecule.DestroyElectIso();
			DisplayMolecule.ClearMemory();

//			id="";
			UIData.isclear=false;
			UIData.isParticlesInitialized=false;
			GUIMoleculeController.rayon = 1.0f;
			UIData.secondarystruct = false;
			Debug.Log("UIData.isclear");
		}
		if(UIData.resetDisplay&&UIData.isCubeToSphere)
		{
			DisplayMolecule.CubeToSphere();
			Debug.Log ("UIData :: resetDisplay && iscubetoSphere");
		}
		
		if(UIData.resetDisplay&&UIData.isSphereToCube)
		{
			DisplayMolecule.SphereToCube();
			Debug.Log ("UIData :: reset display && is spheretocube");
		}
		
		if(UIData.resetBondDisplay)
		{
			DisplayMolecule.ResetBondDisplay();
			Debug.Log ("UIData :: reset bonddisplay");
		}
		
		
		if(UIData.isOpenFile)
		{
			// if(boxes!=null)
			// {
			// 	foreach(GameObject box in boxes)
			// 	{
			// 		Destroy(box);
			// 	}
			// }
			
			UIData.isOpenFile = false;
			
			StartCoroutine(loadFile());	
			Display();		
		}
		
		if(UIData.backGroundIs)
		{
			LocCamera.GetComponent<Skybox>().enabled=true;
			

		}
		
		else
		{
			LocCamera.GetComponent<Skybox>().enabled=false;
		}
		UIData.EnableUpdate=true;
		
		if(UIData.interactive&&UIData.resetInteractive)
		{
			DisplayMolecule.AddAllPhysics();
			UIData.resetInteractive=false;			
		}
		else if(!UIData.interactive&&UIData.resetInteractive)
		{
			DisplayMolecule.DeleteAllPhysics();
			UIData.resetInteractive=false;			
		}
		
		if(UIData.meshcombine)
		{
			DisplayMolecule.AddCombineMesh();
			UIData.resetMeshcombine=false;			
		}
		else if(!UIData.meshcombine)
		{
			DisplayMolecule.DeleteCombineMesh();
			UIData.resetMeshcombine=false;			
		}
		
		if (requestPDB.Loading)
        {

            	//GUI.Label(new Rect(100, 15, 200, 30), "", "bj");
            	//GUI.Label(new Rect(100,15, requestPDB.progress * 200, 30), "", "qj");
        }

//		if(GUI.tooltip != "")GUI.Label ( new Rect(180,Screen.height-35,Screen.width-360,20), GUI.tooltip);
//		if(MoleculeModel.newtooltip != "")GUI.Label ( new Rect(180,Screen.height-35,Screen.width-360,20), MoleculeModel.newtooltip);
		if(GUI.tooltip != "")GUI.Box ( new Rect(180,Screen.height-55,450,25), GUI.tooltip);
		if(MoleculeModel.newtooltip != "")GUI.Box ( new Rect(180,Screen.height-55,450,25), MoleculeModel.newtooltip);
		
	}
	
	
	// loading the file in all possibilities
	public  IEnumerator loadFile()
	{
		#if !UNITY_WEBPLAYER
		{


//				alist=requestPDB.LoadPDBRequest(url,id);
		
		// check all format reading by unitymol PDB, XGMML and OBJ
			if(UIData.fetchPDBFile)
			{
				Debug.Log("pdbServer/pdbID :: "+GUIDisplay.pdbServer + GUIDisplay.pdbID);
				Debug.Log("proxyServer+proxyPort :: "+GUIDisplay.proxyServer + GUIDisplay.proxyPort);
				int proxyport = 0;
				if(GUIDisplay.proxyPort != "")
				{
					proxyport = int.Parse(GUIDisplay.proxyPort);
				}else{
					proxyport = 0;
				}

				requestPDB.FetchPDB(GUIDisplay.pdbServer, 
										  GUIDisplay.pdbID,
										  GUIDisplay.proxyServer,
										  proxyport);
			}
		// if we laod a pdb file launch the reading of file
			else if(GUIDisplay.file_extension=="pdb")
			{
					requestPDB.LoadPDBRequest(GUIDisplay.file_base_name);
			}

		// check the format of xgmml	
			else if(UI.GUIDisplay.file_extension=="xgmml")
			{
			
					StartCoroutine(requestPDB.LoadXGMML("file://"+GUIDisplay.file_base_name+"."+GUIDisplay.file_extension));
					while(!requestPDB.isDone)
					{
						Debug.Log(requestPDB.progress);
						yield return new WaitForEndOfFrame();
					}
					UIData.atomtype=UIData.AtomType.hyperball;
					UIData.bondtype=UIData.BondType.hyperstick;
					GUIMoleculeController.rayon = 0.7f;
					GUIMoleculeController.shrink = 0.0001f;
					GUIMoleculeController.linkscale = 0.3f;
					SendMessage("Display",SendMessageOptions.DontRequireReceiver);
			}
			else if(UI.GUIDisplay.file_extension=="obj")
			{
					requestPDB.LoadOBJRequest(GUIDisplay.file_base_name+"."+GUIDisplay.file_extension);
					MoleculeModel.SurfaceFileExist=true;
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
			{
				requestPDB.LoadPDBResource(UIData.init_molecule);
			}
		}			
		#endif
		//Debug.Log("SDGFSDGSDGDSG");
		GUIMoleculeController.menuAtom_show = true;
		Camera.main.GetComponent<SplashScreen>().enabled = false;
		yield return null;

	}
	

	
	
	// Update is called once per frame
	void Update()
	{
	//	DebugStreamer.message = ("Current time is: " + Time.time);
		
		if (requestPDB.isDone){
            requestPDB.Loading = false;
        }
        else{
            requestPDB.Loading = true;
        }


		if(isControl&&(!UIData.cameraStop2))
		{

			MouseOperate();
			KeyOperate();
			
			SetCenterbySpace();
			HiddenOperate();
			OpenMenuOperate();
			OpenBoundOperate();
		}
		if (GUIMoleculeController.toggle_HB_SANIM)
		{
			GUIMoleculeController.shrink +=  Time.deltaTime * GUIMoleculeController.hb_sanim * GUIMoleculeController.hb_ssign;
			if (GUIMoleculeController.shrink > 0.95f ) { GUIMoleculeController.hb_ssign = -1.0f; }
			if (GUIMoleculeController.shrink < 0.05f ) { GUIMoleculeController.hb_ssign = 1.0f; }
		}
		if (GUIMoleculeController.toggle_HB_RANIM) 
		{
			GUIMoleculeController.rayon +=  Time.deltaTime * GUIMoleculeController.hb_ranim * GUIMoleculeController.hb_rsign;
			if (GUIMoleculeController.rayon > 0.95f ) { GUIMoleculeController.hb_rsign = -1.0f; }
			if (GUIMoleculeController.rayon < 0.05f ) { GUIMoleculeController.hb_rsign = 1.0f; }
		}
	
		if(GUIMoleculeController.toggle_HB_TRANS) 
		{
			GUIMoleculeController.transDelta = 25.0f;	
		}
		else
		{
			GUIMoleculeController.transDelta = 1.0f;	
		}
	
		if (GUIMoleculeController.transCPK_LICORICE)
		{
			GUIMoleculeController.rayon = transition(GUIMoleculeController.rayon, GUIMoleculeController.newRadius, GUIMoleculeController.deltaRadius);
			GUIMoleculeController.linkscale = transition(GUIMoleculeController.linkscale, GUIMoleculeController.newScale, GUIMoleculeController.deltaScale);
			GUIMoleculeController.shrink = transition(GUIMoleculeController.shrink, GUIMoleculeController.newShrink, GUIMoleculeController.deltaShrink);
			if(GUIMoleculeController.rayon == GUIMoleculeController.newRadius && GUIMoleculeController.linkscale == GUIMoleculeController.newScale && GUIMoleculeController.shrink == GUIMoleculeController.newShrink) 			
			GUIMoleculeController.transCPK_LICORICE = false;
		}
		
		
		LineUpdate.scale=GUIMoleculeController.linkscale;
		
		StickUpdate.radiusFactor = GUIMoleculeController.rayon;
		StickUpdate.shrink      = GUIMoleculeController.shrink;
		StickUpdate.scale 		= GUIMoleculeController.linkscale;
		BallUpdateHB.radiusFactor = GUIMoleculeController.rayon;
		BallUpdateHB.depthfactor = GUIMoleculeController.depthfactor;
		BallUpdateSphere.radiusFactor = GUIMoleculeController.rayon;
		BallUpdateCube.radiusFactor = GUIMoleculeController.rayon;
		BallUpdateRC.radiusFactor = GUIMoleculeController.rayon;
		
		
		
		
		BallUpdateHB.drag = GUIMoleculeController.drag;
		BallUpdateHB.spring = GUIMoleculeController.spring;
		
		BallUpdateHB.EnergyGrayColor = GUIMoleculeController.EnergyGrayColor.color;
		
		//Hiding the particles if not in particle mode.
		if(UIData.atomtype == UIData.AtomType.particleball)
			ParticleEffect.radiusFactor = GUIMoleculeController.rayon;
		else
			ParticleEffect.radiusFactor = 0.000001f;

		// if ( GUIMoleculeController.surface_staticcut)
		// {
		// 	BallUpdateHB.cut = 1f;
		// 	BallUpdateHB.cutplane = new Vector4(GUIMoleculeController.cutX,
		// 									GUIMoleculeController.cutY,
		// 									GUIMoleculeController.cutZ,
		// 									GUIMoleculeController.depthcut);
		// }



//		particleeffect.radiuschange=true;
		
		// movement of fieldline =============================================================================================		
		// Send all the paramter to the field line shader
		
		
		// GameObject scenecontroller= GameObject.Find("LoadBox");
		
		GameObject[] FieldLines = GameObject.FindGameObjectsWithTag("FieldLineManager");
		foreach (GameObject FieldLine in FieldLines) {
			LineRenderer curLineRenderer;
        	curLineRenderer = FieldLine.GetComponent<LineRenderer>();
			curLineRenderer.material.SetFloat("_timeOff",Time.time);
			
			// for benoist video comment next line
			curLineRenderer.material.SetColor("_Color", GUIMoleculeController.EnergyGrayColor.color);
			
			if (GUIMoleculeController.FieldLineColorGradient)
				curLineRenderer.material.SetFloat("_colormode", 0f);
			else
				curLineRenderer.material.SetFloat("_colormode", 1f);

			curLineRenderer.material.SetFloat("_Speed",GUIMoleculeController.speed);
			curLineRenderer.material.SetFloat("_Density",GUIMoleculeController.density);
			curLineRenderer.material.SetFloat("_Length", GUIMoleculeController.linelength);
			curLineRenderer.SetWidth(GUIMoleculeController.linewidth,GUIMoleculeController.linewidth);
			curLineRenderer.material.SetFloat("_depthcut", (GUIMoleculeController.depthcut-maxCamera.currentDistance));
			curLineRenderer.material.SetFloat("_adjust",(GUIMoleculeController.adjustFieldLinecut));
			curLineRenderer.material.SetVector("_SurfacePos", FieldLine.transform.position);

			
			if (GUIMoleculeController.surface_mobilecut)
				curLineRenderer.material.SetFloat("_cut", 2f);
			else if ( GUIMoleculeController.surface_staticcut){
				curLineRenderer.material.SetFloat("_cut", 1f);
				curLineRenderer.material.SetVector("_cutplane",new Vector4(GUIMoleculeController.cutX,
																			GUIMoleculeController.cutY,
																			GUIMoleculeController.cutZ,
																			GUIMoleculeController.depthcut));
			}
		}
//		LineRenderer curLineRenderertemp;
		
		
//Video with Benoit =============================================================================================		
		
		// if (Input.GetKey("w") & polop ==0 ){
		// 	polop=1;
		// 	for (int i=296; i< 305;i++){
		// 		FieldLines[i].SetActiveRecursively(false);
		// 	}
		// 	for (int i=321; i< 460;i++){
		// 		FieldLines[i].SetActiveRecursively(false);
		// 	}
		// 	for (int i=481; i<510 ;i++){
		// 		FieldLines[i].SetActiveRecursively(false);
		// 	}
		// 	for (int i=550; i< 757;i++){
		// 		FieldLines[i].SetActiveRecursively(false);
		// 	}
		// 	for (int i=762; i< 818;i++){
		// 		FieldLines[i].SetActiveRecursively(false);
		// 	}
		// 	for (int i=823; i< 839;i++){
		// 		FieldLines[i].SetActiveRecursively(false);
		// 	}
		// 	for (int i=843; i< 895;i++){
		// 		FieldLines[i].SetActiveRecursively(false);
		// 	}
		// 	for (int i=896; i< 1000;i++){
		// 		FieldLines[i].SetActiveRecursively(false);
		// 	}
		// 	for (int i=1100; i< 1340;i++){
		// 		FieldLines[i].SetActiveRecursively(false);
		// 	}
		// 	for (int i=1376; i< 1431;i++){
		// 		FieldLines[i].SetActiveRecursively(false);
		// 	}
		// 	for (int i=1934; i< 2001;i++){
		// 		FieldLines[i].SetActiveRecursively(false);
		// 	}
			
		// 	FieldLines[239].SetActiveRecursively(true);
		// 	FieldLines[275].SetActiveRecursively(true);
		// 	FieldLines[316].SetActiveRecursively(true);
		// 	FieldLines[384].SetActiveRecursively(true);
		// 	FieldLines[386].SetActiveRecursively(true);
		// 	FieldLines[405].SetActiveRecursively(true);
		// 	FieldLines[411].SetActiveRecursively(true);
		// 	FieldLines[426].SetActiveRecursively(true);
		// 	FieldLines[426].SetActiveRecursively(true);
		// 	FieldLines[433].SetActiveRecursively(true);
		// 	FieldLines[657].SetActiveRecursively(true);
		// 	FieldLines[910].SetActiveRecursively(true);
		// 	FieldLines[1045].SetActiveRecursively(true);
		// 	FieldLines[1096].SetActiveRecursively(true);	
		// 	FieldLines[1194].SetActiveRecursively(true);
		// 	FieldLines[1306].SetActiveRecursively(true);
		// 	FieldLines[1336].SetActiveRecursively(true);
		// 	FieldLines[1339].SetActiveRecursively(true);
		// 	FieldLines[1414].SetActiveRecursively(true);
		// 	FieldLines[1440].SetActiveRecursively(true);
		// 	FieldLines[1444].SetActiveRecursively(true);
		// }
		
		// if (Input.GetKey("x")){
		// 		curLineRenderertemp =FieldLines[161].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[239].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[275].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[316].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[384].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[386].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[405].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[411].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[426].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[433].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[657].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[1045].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[1096].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[1194].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[1306].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[1336].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[1339].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[1341].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[1414].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[1440].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[1444].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[1534].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[1535].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[1538].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[1609].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[1684].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[1685].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[1763].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[1830].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);curLineRenderertemp =FieldLines[1874].GetComponent<LineRenderer>();
		// 		curLineRenderertemp.material.SetColor("_Color",Color.blue);
		// }
		
		GameObject[] Surfaces = GameObject.FindGameObjectsWithTag("SurfaceManager");
		
		
// // color and all for benoist =============================================================================================		
// // can be reuse in unitymol classic, butwe have to choose all the color and workd with less than 6 surfaces
// 		if (MoleculeModel.SurfaceFileExist){
		
			// if (Input.GetKey("c") ){
			// 	Color purple = Color.black;
			// 	Color orange = Color.black;
			// 	Color grey = Color.black;
			// 	Color cyan = Color.white;	
			// 	Color yellow = Color.white;
				
				
			// 	purple.r=251f/255f; purple.b=255f/255f;
			// 	orange.r=255f/255f;orange.g=193f/255f;orange.b=12f/255f;
			// 	grey.r=148f/255f; grey.g= 148f/255f; grey.b=148f/255f;	
			// 	cyan.r=108f/255f;
			// 	yellow.b=66f/255f;
			// 	Surfaces[0].renderer.material.SetColor("_Color", cyan);
			// 	Surfaces[1].renderer.material.SetColor("_Color", yellow);
			// 	Surfaces[2].renderer.material.SetColor("_Color", grey);
			// 	Surfaces[3].renderer.material.SetColor("_Color", purple);
			// 	Surfaces[4].renderer.material.SetColor("_Color", orange);
				
			// }
			
			// // remove 2 structure of the protein (for benoist)
			// if (Input.GetKey("v") & masque == 0){
			// 	Surfaces[0].SetActiveRecursively(false);
			// 	Surfaces[1].SetActiveRecursively(false);
			// 	masque =1;
			// }
			
// =============================================================================================				
//			if (Input.GetKey("n")){  // stop la mise a jour des surface afin de changer la couleur que de certaine surface
			
			foreach (GameObject Surface in Surfaces) 
			{ 	
				
				if (GUIMoleculeController.SurfaceGrayColor.color != Color.white){
					Surface.renderer.material.SetFloat("_colormode", 1f);
				}else
					Surface.renderer.material.SetFloat("_colormode", 0f);
				
				
				if (GUIMoleculeController.surface_texture){
						Surface.renderer.material.shader =	Shader.Find("Mat Cap Cut");
						Surface.renderer.material.SetTexture("_MatCap",(Texture)Resources.Load(GUIMoleculeController.surface_texture_name));
				}
				else if(GUIMoleculeController.external_surface_texture){
					
					Surface.renderer.material.shader =	Shader.Find("Mat Cap Cut");
					Surface.renderer.material.SetTexture("_MatCap",GUIMoleculeController.ext_surf);
				}else if(GUIMoleculeController.surface_build || GUIMoleculeController.dxread){
					Surface.renderer.material.shader =	Shader.Find("Mat Cap Cut");
					Surface.renderer.material.SetTexture("_MatCap",(Texture)Resources.Load("lit_spheres/divers/daphz1"));
				
				}else{
					Surface.renderer.material.shader= Shader.Find("Bumped Specular cut");
					Surface.renderer.material.SetColor("_SpecColor", Color.black);
	
				}
				// send all the paramter to the surface shader
					// Surface.renderer.material.SetFloat("_Shininess", GUIMoleculeController.intensity);
					// if (Input.GetKey("n")) // uncoment for benoist
						Surface.renderer.material.SetColor("_Color", GUIMoleculeController.SurfaceGrayColor.color); 
//					Surface.renderer.material.SetColor("_Color", new Color(1f,1f,1f)); // couleur blanche fixé
					Surface.renderer.material.SetFloat("_depthcut", GUIMoleculeController.depthcut);
					Surface.renderer.material.SetFloat("_cutX", GUIMoleculeController.cutX);
					Surface.renderer.material.SetFloat("_cutY", GUIMoleculeController.cutY);
					Surface.renderer.material.SetFloat("_cutZ", GUIMoleculeController.cutZ);
					Surface.renderer.material.SetVector("_SurfacePos", Surface.transform.position);
					if (GUIMoleculeController.surface_mobilecut)	// set the cutting mode
						Surface.renderer.material.SetFloat("_cut", 2f);
					else if ( GUIMoleculeController.surface_staticcut) 
						Surface.renderer.material.SetFloat("_cut", 1f);
					else 
						Surface.renderer.material.SetFloat("_cut", 0f);
					
					 
//				} // "N" block
			
			}
		// }
		
// update the sphere clone =============================================================================================				
		// GameObject sphereclone=GameObject.Find("transparentsphere(Clone)");
		// if(sphereclone)
		// {
		// 	float localscale=3.0f*GUIMoleculeController.rayon;
		// 	if(localscale<1)localscale=1;
		// 	sphereclone.transform.localScale=new Vector3(localscale,localscale,localscale);
		// }
		

		//FPS Count
		
		timeleft -= Time.deltaTime;
		accum += Time.timeScale/Time.deltaTime;
		++frames;

		// Interval ended - update GUI text and start new interval
		if( timeleft <= 0.0f )
		{
		// display two fractional digits (f2 format)
			float fps = accum/frames;//(1 / Time.deltaTime);
			MoleculeModel.FPS = fps.ToString("f2");
			//Write FPS data into file
			if(fpsLogToggle)
			{
				fpsCount ++;
				fpsSum += fps;
				if(fpsCount > 35)
				{
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
			
		// gUIDisplay.gUIMoleculeController.GetPanelPixel();
	}
	
	
	public void toggleFPSLog()
	{
		if(!fpsLogToggle)
		{
			fpsLogToggle = true;
			Debug.Log("Entering :: Starting fps measure to file");
		}
		else
		{
			fpsLogToggle = false;
			DateTime currTime = DateTime.Now;
			string filename = currTime.ToString("HH_mm_ss")+"_umol_fpsdata.txt";
			fpsLog = new StreamWriter(filename);	
			fpsLog.WriteLine(fpsCount.ToString());
			fpsLog.WriteLine( (fpsSum/fpsCount).ToString() );
			fpsLog.Close();
			fpsLog.Dispose();
		}
	}	
	
	float transition(float val, float newVal, float deltaVal) 
	{
		if(val <= newVal && deltaVal < 0.0f) return newVal;
		if(val >= newVal && deltaVal > 0.0f) return newVal;
		return val + deltaVal;	
	}

	private void OpenMenuOperate()
	{
		if(Input.GetKeyDown(KeyCode.Delete))
		{

				UIData.openAllMenu=!UIData.openAllMenu;
				UIData.resetDisplay=true;
				UIData.isSphereToCube=true;
				UIData.isCubeToSphere=false;
				UIData.atomtype=UIData.AtomType.particleball;
				UIData.resetBondDisplay=true;
				UIData.bondtype=UIData.BondType.nobond;


		}
	}

	private void OpenBoundOperate()
	{
		if(Input.GetKeyDown(KeyCode.Minus)||Input.GetKeyDown(KeyCode.Equals))
		{

				Debug.Log("Press Equal key.");
				UIData.openBound=!UIData.openBound;
				UIData.resetDisplay=true;
				UIData.isSphereToCube=true;
				UIData.isCubeToSphere=false;
				UIData.atomtype=UIData.AtomType.particleball;
				UIData.resetBondDisplay=true;
				UIData.bondtype=UIData.BondType.nobond;


		}
	}
	
	private void HiddenOperate()
	{
		if(Input.GetKeyDown(KeyCode.Backspace))
		{
//			Debug.Log("KeyCode.Backspace press.");
			if(UIData.hiddenUI==false&&UIData.hiddenUIbutFPS==false&&UIData.hiddenCamera==false)
			{
					UIData.hiddenUI=true;
					Debug.Log("Hide all the UI.");
			}
			else if(UIData.hiddenUI==true&&UIData.hiddenUIbutFPS==false&&UIData.hiddenCamera==false)
			{
					UIData.hiddenCamera=true;
					LocCamera.GetComponent<Camera>().enabled=false;
					Debug.Log("Hide all the UI and Camera.");
			}
			else if(UIData.hiddenUI==true&&UIData.hiddenUIbutFPS==false&&UIData.hiddenCamera==true)
			{
					UIData.hiddenCamera=false;
					LocCamera.GetComponent<Camera>().enabled=true;
					UIData.hiddenUI=false;
					UIData.hiddenUIbutFPS=true;
					Debug.Log("Hide all the UI unless FPS.");
			}
			else if(UIData.hiddenUI==false&&UIData.hiddenUIbutFPS==true&&UIData.hiddenCamera==false)
			{
					
					UIData.hiddenUI=false;
					UIData.hiddenUIbutFPS=false;		
					Debug.Log("Show all the UI and Camera.");
			}	


	
		}
	}
	
	

	
	
// control with the keyboard =============================================================================================		
// I don't check if the key which are used fir benoist video are here or not.
	private void KeyOperate()
		
	{	
//		originalRotation = transform.localRotation;
		//print(LocCamera.transform.position.x);
		Vector3 v=new Vector3();
		v=LocCamera.transform.localPosition;
		
		//right move
		if(Input.GetKey(KeyCode.D))
		{

			v.x-=0.5f;
			LocCamera.transform.localPosition=v;
			if(UIData.switchmode)ToParticle();
		}
		//clockwise
		if(Input.GetKey(KeyCode.W))
		{
			
			v.y-=0.5f;
			LocCamera.transform.localPosition=v;
			if(UIData.switchmode)ToParticle();
		}
		//down move
		if(Input.GetKey(KeyCode.S))
		{
			v.y+=0.5f;
			LocCamera.transform.localPosition=v;
			if(UIData.switchmode)ToParticle();
			//print("LocCamera.transform.localPosition.y"+v.y);
		}
		//down rotation
		if(Input.GetKey(KeyCode.A))
		{
			v.x+=0.5f;
			LocCamera.transform.localPosition=v;
			if(UIData.switchmode)ToParticle();
			//print("LocCamera.transform.localPosition.x"+v.x);
		}
		//zoom out
		if(Input.GetKey(KeyCode.N))
		{
			v.z+=0.5f;
			LocCamera.transform.localPosition=v;
			if(UIData.switchmode)ToParticle();
			//print("LocCamera.transform.localPosition.x"+v.x);
		}
		//zoom in
		if(Input.GetKey(KeyCode.B))
		{
			v.z-=0.5f;
			LocCamera.transform.localPosition=v;
			if(UIData.switchmode)ToParticle();
			//print("LocCamera.transform.localPosition.x"+v.x);
		}
		
		//left move
		if(Input.GetKey(KeyCode.Q))
		{
			transform.RotateAround(Deta,axisX,0.6f);	
			DMatrix.RotationMatrix(axisX,axisY,axisZ,0.6f);
			if(UIData.switchmode)ToParticle();
		}
		//up rotation
		if(Input.GetKey(KeyCode.E))
		{
			
			transform.RotateAround(Deta,axisX,-0.6f);		
			DMatrix.RotationMatrix(axisX, axisY, axisZ,-0.6f);
			if(UIData.switchmode)ToParticle();
		}
		//up move
		if(Input.GetKey(KeyCode.Z))
		{
			
			transform.RotateAround(Deta,axisZ,0.6f);
			DMatrix.RotationMatrix(axisZ,axisY, axisX,0.6f);
			if(UIData.switchmode)ToParticle();
			
		}
		//left rotation
		if(Input.GetKey(KeyCode.X))
		{
			
			transform.RotateAround(Deta,axisZ,-0.6f);
			DMatrix.RotationMatrix(axisZ, axisY,axisX,-0.6f);
			if(UIData.switchmode)ToParticle();
			
		}

		if(Input.GetKeyUp(KeyCode.D))
		{

			if(UIData.switchmode)ToNotParticle();
		}

		if(Input.GetKeyUp(KeyCode.W))
		{
			
			if(UIData.switchmode)ToNotParticle();

		}

		if(Input.GetKeyUp(KeyCode.S))
		{
			if(UIData.switchmode)ToNotParticle();

		}
		
		if(Input.GetKeyUp(KeyCode.A))
		{
			if(UIData.switchmode)ToNotParticle();

		}
		
		if(Input.GetKeyUp(KeyCode.N))
		{
			if(UIData.switchmode)ToNotParticle();

		}
		if(Input.GetKeyUp(KeyCode.B))
		{
			if(UIData.switchmode)ToNotParticle();

		}
		
		if(Input.GetKeyUp(KeyCode.Q))
		{
			if(UIData.switchmode)ToNotParticle();

		}
		
		if(Input.GetKeyUp(KeyCode.E))
		{
			if(UIData.switchmode)ToNotParticle();

		}
		
		if(Input.GetKeyUp(KeyCode.Z))
		{
			
			if(UIData.switchmode)ToNotParticle();

			
		}
		
		if(Input.GetKeyUp(KeyCode.X))
		{
			
			if(UIData.switchmode)ToNotParticle();

			
		}
		
		if(Input.GetKeyUp(KeyCode.RightArrow))
		{
			if(UIData.switchmode)ToNotParticle();

		}
		
		
		if(Input.GetKeyUp(KeyCode.LeftArrow))
		{
			if(UIData.switchmode)ToNotParticle();

		}

        if(Input.GetKey("joystick button 3"))
        {
            UIData.resetDisplay=true;
            UIData.isCubeToSphere=false;
            UIData.isSphereToCube=true;
            UIData.atomtype=UIData.AtomType.cube;


        }
        
        if(Input.GetKey("joystick button 2"))
        {
            UIData.resetDisplay=true;
            UIData.isSphereToCube=false;
            UIData.isCubeToSphere=true;
            UIData.atomtype=UIData.AtomType.sphere;

        }
        
        if(Input.GetKey("joystick button 0"))
        {
            UIData.resetDisplay=true;
            UIData.isCubeToSphere=false;
            UIData.isSphereToCube=true;
            UIData.atomtype=UIData.AtomType.hyperball;


        }
        
        if(Input.GetKey("joystick button 1"))
        {
            UIData.resetDisplay=true;
            UIData.isSphereToCube=true;
            UIData.isCubeToSphere=false;
            UIData.atomtype=UIData.AtomType.particleball;
            UIData.resetBondDisplay=true;
            UIData.bondtype=UIData.BondType.nobond;
        }
		
		Vector3 vv=new Vector3();
		vv=LocCamera.transform.localPosition;		
		
		if(!GUIMoleculeController.toggle_NA_MAXCAM)
		{
			vv.z+=Input.GetAxis("Mouse ScrollWheel")*5;
//			if(UIData.switchmode)ToParticle();
		
		}
		LocCamera.transform.localPosition=vv;

	}
	
// function who set the center of the scene R for Reste and T on an atom
	private void SetCenterbySpace()
	{
		if(Input.GetKeyUp(KeyCode.R))
		{
			Debug.Log("Press the R key");
			SetCenter(0);
			
		}
		if(Input.GetKeyUp(KeyCode.T))
		{
			Debug.Log("Press the T key");
			SetCenter(1);
			
		}
	}

// controlled when maxcam is desactivate with the mouse =============================================================================================		
	private void MouseOperate()
	{
			Vector3 v=new Vector3();
			v=LocCamera.transform.localPosition;

		
			if(!GUIMoleculeController.toggle_NA_MAXCAM)
			{
//				if (Input.GetMouseButton(1) )
//				{
//					v=LocCamera.transform.localPosition;
//				}

				if (Input.GetMouseButton(0))
				{
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
//					transform.localRotation = originalRotation * xQuaternion * yQuaternion;
					if(UIData.switchmode)ToNotParticle();
				}
//				if(Input.GetMouseButtonUp(0))
//				{
//					if(UIData.switchmode)ToNotParticle();
//				}
				if(Input.GetMouseButtonUp(1))
				{
//					v=LocCamera.transform.localPosition;
					if(UIData.switchmode)ToNotParticle();
				}
				v.z+=Input.GetAxis("Mouse ScrollWheel")*5;
				LocCamera.transform.localPosition=v;
				Debug.Log ("get mouse: " +v.z);
				
				
			}	
//			if(UIData.switchmode)ToParticle();
		

		
	}
	
	
// affect the cam target to modifiate his position
	private void SetCenter( int mode)
	{
		GameObject CamTarget = GameObject.Find("Cam Target");
	//			CamTarget.transform.rotation =  new Quaternion(0f, 0f, 0f, 0f);
	
	
	
		// chose the main function 0 to restart position or 1 to center around an atom
		if (mode ==	0){
			Debug.Log("Entering :: SetCenter for cam target to" + MoleculeModel.cameraLocation.z);
			//Camera.main.transform.localPosition = new Vector3(0f,0f,Camera.main.transform.localPosition.z);
			// CamTarget.transform.localPosition = new Vector3(0f,0f,0f);
			// change the postition of the LoadBox
			// GameObject scenecontroller= GameObject.Find("LoadBox");
			if(scenecontroller.GetComponent<maxCamera>().enabled)
			{
				// scenecontroller.transform.rotation =  new Quaternion(0f, 0f, 0f, 0f);
				// scenecontroller.transform.localPosition = new Vector3(0f,0f,-25f);
				maxCamera comp = scenecontroller.GetComponent<maxCamera>();
				comp.ToCenter();
	
			}
		}else if (mode ==1){
			Debug.Log("target : " +MoleculeModel.target);
			CamTarget.transform.rotation = transform.rotation;
			CamTarget.transform.position = MoleculeModel.target;
		}

	}
	

// change the representation of the protein in hyperball. almost for the switch mod
	public void ToNotParticle() {
		if(UIData.atomtype != UIData.AtomType.particleball && UIData.atomtype != previous_AtomType)
			previous_AtomType = UIData.atomtype;
		DisplayMolecule.ToNotParticle(previous_AtomType);
		// Debug.Log("ToNotParticle()");

	}
	
	// change the representation of the protein in particle
	// also call in switch mod
	public void ToParticle() {
		if(UIData.atomtype != UIData.AtomType.particleball)
			previous_AtomType = UIData.atomtype;
		DisplayMolecule.ToParticle();
		// Debug.Log("ToParticle()");
	}

}

