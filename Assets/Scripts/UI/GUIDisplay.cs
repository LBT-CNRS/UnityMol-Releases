/// @file GUIDisplay.cs
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
/// $Id: GUIDisplay.cs 247 2013-04-07 20:38:19Z baaden $
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

namespace UI
{
	
	using UnityEngine;
	using System.Collections;
	using SocketConnect.UnitySocket;
	using Config;
	using Molecule.View;
	using Molecule.Model;
	using System.Text;
	
	/** !WiP manage GUI, and provide static strings for the GUI.
	 * 
	 * $Id: GUIDisplay.cs 247 2013-04-07 20:38:19Z baaden $
	 * An important part of the User interface. Texture handling for atoms and bonds is done
	 * here.
	 * 
	 * Unity3D doc :<BR>
	 * <A HREF="http://docs.unity3d.com/Documentation/ScriptReference/GUIContent.html">GUIContent</A><BR>
	 * <A HREF="http://docs.unity3d.com/Documentation/ScriptReference/Texture2D.html">Texture2D</A><BR>
	 * <A HREF="http://docs.unity3d.com/Documentation/ScriptReference/TextureFormat.html">TextureFormat</A><BR>
	 * <A HREF="http://docs.unity3d.com/Documentation/ScriptReference/TextAnchor.html">TextAnchor</A><BR>
	 * <A HREF="http://docs.unity3d.com/Documentation/ScriptReference/GUIStyle.html">GUIStyle</A><BR>
	 * <A HREF="http://docs.unity3d.com/Documentation/ScriptReference/Debug.html">Debug</A><BR>
	 * <A HREF="http://docs.unity3d.com/Documentation/ScriptReference/GUI.html">GUI</A><BR>
	 * <A HREF="http://docs.unity3d.com/Documentation/ScriptReference/Resources.html">Resources</A><BR>
	 * <A HREF="http://docs.unity3d.com/Documentation/ScriptReference/Rect.html">Rect</A><BR>
	 * 
	 */
	public class GUIDisplay
	{
		/* TODO:
		 * GET RID OF STATIC VARIABLES !
		 * See also : Singleton design pattern.
		 */
		public 	static string pdbID="1KX2";
		public	static string pdbServer = "http://www.pdb.org/pdb/files/";
		// public 	static string proxyServer = "cache.ibpc.fr";
		// public	static string proxyPort = "8080";
		public 	static string proxyServer = "";
		public	static string proxyPort = "";
		private 	   StringBuilder proxyPortValidate;

//		private static string idField="172.27.0.141";
//		private static string PortField="843";
		public    bool   display=false;
		public bool displayChoice=false;
		private GUIContent []list;
		// public static string directorypath="/opt/Unity3D/UnityMol_SVN/";
		public static string directorypath="/opt/src/Unity/UnityMol_SVN/";
		public static string file_base_name = "";
		public static string file_extension = ".pdb";
				
		private string id="";
		public InputURL inputURL=new InputURL();
		public static bool m_max=false;
		public static bool m_texture=false;
		public static int texSet_max=30; /*!< Maximum number of full texture pages */
		public static int besttexSet_min=-5; /*!< Maximum number of condensed texture pages (negative value!) */
		//public bool only_best_textures = false; /*!< Toggle condensed set of textures vs. full set */
		public bool only_best_textures = true;
		public static int texture_set=1;
		
		public bool LoginFlag=false;
		public bool LoginAgainFlag=false;
//		private AtomAttribution oxygenAtomAttribution;
//		private AtomAttribution sulphurAtomAttribution;
//		private AtomAttribution carbonAtomAttribution;
//		private AtomAttribution nitrogenAtomAttribution;
//		private AtomAttribution phosphorusAtomAttribution;
//		private AtomAttribution hydrogenAtomAttribution;
//		private AtomAttribution unknownAtomAttribution;

		public GUIMoleculeController gUIMoleculeController= new GUIMoleculeController();
		
		public ImprovedFileBrowser m_fileBrowser;
		public string m_textPath;
		public string m_last_texture_dir = null;
		public string m_lastOpenDir = null ;
		public Texture2D ext_surf;

		private Texture2D directoryimage, fileimage;
				
		private ColorObject carbon_color 	= new ColorObject(MoleculeModel.carbonColor);
		private ColorObject nitrogen_color = new ColorObject(MoleculeModel.nitrogenColor);
		private ColorObject oxygen_color	= new ColorObject(MoleculeModel.oxygenColor);
		private ColorObject sulphur_color	= new ColorObject(MoleculeModel.sulphurColor);
		private ColorObject phosphorus_color	= new ColorObject(MoleculeModel.phosphorusColor);
		private ColorObject hydrogen_color	= new ColorObject(MoleculeModel.hydrogenColor);
		private ColorObject unknown_color	= new ColorObject(MoleculeModel.unknownColor);
		
		Color[] colcarbonNew = new Color[200];
		Texture2D colCarbon = new Texture2D(20,10,TextureFormat.ARGB32,false);
		
		Color[] colNitrogenNew = new Color[200];
		Texture2D colNitrogen = new Texture2D(20,10,TextureFormat.ARGB32,false);
		
		Color[] colOxygenNew = new Color[200];
		Texture2D colOxygen = new Texture2D(20,10,TextureFormat.ARGB32,false);
		
		Color[] colSulphurNew = new Color[200];
		Texture2D colSulphur = new Texture2D(20,10,TextureFormat.ARGB32,false);
		
		Color[] colPhosphorNew = new Color[200];
		Texture2D colPhosphor = new Texture2D(20,10,TextureFormat.ARGB32,false);
		
		Color[] colHydrogenNew = new Color[200];
		Texture2D colHydrogen = new Texture2D(20,10,TextureFormat.ARGB32,false);
		
		Color[] colUnknowNew = new Color[200];
		Texture2D colUnknow = new Texture2D(20,10,TextureFormat.ARGB32,false);

		// MB for centered text		
		protected GUIStyle CentredText {
        	get {
            	if (m_centredText == null) 
            	{
	                m_centredText = new GUIStyle(GUI.skin.label);
	                m_centredText.alignment = TextAnchor.MiddleCenter;
            	}
            	return m_centredText;
        	}
    	}
	    protected GUIStyle m_centredText;


		
		
		
// 		private int left=0;//0:oxygen;1:sulphur;2:carbon;3:nitrogen;4:phosphorus;5:unknown

		
		/** Make a box for atom color selecting.
		 * 
		 * <p>
		 *		<img src="imageDoc/colorBox_UnityMol.png" alt="Pannel for selecting Atoms Color in UnityMol." title="Atoms color selection Pannel."/>
		 * <p>
		 */
		public GUIDisplay()
		{
			#if !UNITY_WEBPLAYER
				m_last_texture_dir = System.IO.Directory.GetCurrentDirectory();
				m_lastOpenDir = System.IO.Directory.GetCurrentDirectory();
			#endif

			DebugStreamer.message = " -- GUIDisplay::GUIDisplay()";
//			oxygenAtomAttribution=new  AtomAttribution();
//			sulphurAtomAttribution=new  AtomAttribution();
//			carbonAtomAttribution=new  AtomAttribution();
//			nitrogenAtomAttribution=new  AtomAttribution();
//			phosphorusAtomAttribution=new  AtomAttribution();
//			hydrogenAtomAttribution=new  AtomAttribution();
//			unknownAtomAttribution=new  AtomAttribution();
			list = new GUIContent[10];
		    
			list[0] = new GUIContent("Red");
		  	list[1] = new GUIContent("Orange");
		    list[2] = new GUIContent("Yellow");
		    list[3] = new GUIContent("Green");
		    list[4] = new GUIContent("Blue");
			list[5] = new GUIContent("Cyan");
		    list[6] = new GUIContent("Purple");
		   	list[7] = new GUIContent("Black");
		   	list[8] = new GUIContent("White");
		   	list[9] = new GUIContent("Gray");

		   	DebugStreamer.message = " -- GUIDisplay::GUIDisplay() - allocs";

			for (int i =0; i <200; i++){
				// NEW PASTEL COLOR THEME
				colcarbonNew[i]=   new Color(0.282f,0.6f,0.498f,1f);
				colNitrogenNew[i]= new Color(0.443f,0.662f,0.882f,1f);
				colOxygenNew[i]=   new Color(0.827f,0.294f,0.333f,1f);
				colPhosphorNew[i]= new Color(0.960f,0.521f,0.313f,1f);
				colHydrogenNew[i]= Color.white;
				colSulphurNew[i]=  new Color(1f,0.839f,0.325f,1f);
				colUnknowNew[i]=   Color.black;
				// OLD DEFAULT :: Basic color set
//				colcarbonNew[i]=Color.green;
//				colNitrogenNew[i]=Color.blue;
//				colOxygenNew[i]=Color.red;
//				colPhosphorNew[i]=new Color(0.6f,0.3f,0.0f,1f);
//				colHydrogenNew[i]=Color.white;
//				colSulphurNew[i]=Color.yellow;
//				colUnknowNew[i]=Color.black;

			}

			DebugStreamer.message = " -- GUIDisplay::GUIDisplay() - for";
			
			colCarbon.SetPixels(colcarbonNew);
			colCarbon.Apply(true);
			colNitrogen.SetPixels(colNitrogenNew);
			colNitrogen.Apply(true);
			colOxygen.SetPixels(colOxygenNew);
			colOxygen.Apply(true);	
			colSulphur.SetPixels(colSulphurNew);
			colSulphur.Apply(true);	
			colPhosphor.SetPixels(colPhosphorNew);
			colPhosphor.Apply(true);
			colHydrogen.SetPixels(colHydrogenNew);
			colHydrogen.Apply(true);
			colUnknow.SetPixels(colUnknowNew);
			colUnknow.Apply(true);
			
			DebugStreamer.message = " -- GUIDisplay::GUIDisplay() - end";

			// AT:: temporary hack to test webplayer ------------------
			// #if UNITY_WEBPLAYER 
			// {
			// 	UIData.fetchPDBFile = false;
			// 	UIData.isOpenFile = true;
			// 	UIData.atomtype = UIData.AtomType.particleball;
			// 	UIData.bondtype = UIData.BondType.nobond;
			// 	GUIMoleculeController.menuOpen_show=false;
			// }
			// #endif

			// AT:: temporary hack to test webplayer ------------------
			
		}

		void OnLevelWasLoaded () /**< Debug function.*/
		{
		    Debug.Log ("If you don't see me, Uniteh brOke!");
		}
		/* TODO : delete.
		public void Confirm()
		{
			id=file_base_name;
			inputURL.SetURL(id);
		}
		*/
		/**
		 * @param
		 */
		public void OpenFileCallback(string path) 
		{
			m_fileBrowser = null;
			if(path == null)
				return;
			
	     	directorypath = System.IO.Path.GetDirectoryName(path);
		 	m_lastOpenDir = directorypath;
			file_base_name = directorypath + System.IO.Path.DirectorySeparatorChar +
							 System.IO.Path.GetFileNameWithoutExtension(path);
			file_extension = System.IO.Path.GetExtension(path).Substring(1);
			
			id = System.IO.Path.GetFileNameWithoutExtension(path);

			UIData.fetchPDBFile = false;
			UIData.isOpenFile=true;
			UIData.atomtype=UIData.AtomType.particleball;
			UIData.bondtype=UIData.BondType.nobond;
	    }
		/** Display a GUI pannel for selecting a PDB on a server or on a local file. 
		 * <p>
		 *		<img src="imageDoc/boxFetchPDB.png" alt="Pannel for selecting Atoms Color in UnityMol." title="Pannel for selecting a pdb file."/>
		 * <p>
		 */
		public void Display()
		{
			
			directoryimage = (Texture2D)Resources.Load("FileBrowser/dossier");
			fileimage=(Texture2D)Resources.Load("FileBrowser/fichiers");
			
			if(GUIMoleculeController.menuOpen_show)
			{
				GUILayout.BeginArea(new Rect(5,30,300,300));
				#if !UNITY_WEBPLAYER
				{
					//id != "" if a molecule is already open
					if (!UIData.hasMoleculeDisplay)
					{
						GUILayout.BeginHorizontal();
						GUILayout.Label("Proxy Server");
						GUILayout.Label("Proxy Port");
						GUILayout.EndHorizontal();
						
						GUILayout.BeginHorizontal();
						proxyServer = GUILayout.TextField(proxyServer,256);

						//Validate the proxyPort : only digits
						proxyPortValidate = new StringBuilder();
				        foreach (char c in proxyPort) 
				        {
				            if (char.IsDigit(c)) 
				            {
				                proxyPortValidate.Append(c);
	            			}
						}
						proxyPort = GUILayout.TextField(proxyPortValidate.ToString(),4);
						GUILayout.EndHorizontal();

						GUILayout.Label("please input a PDB id");
						GUILayout.BeginHorizontal();
						pdbID=GUILayout.TextField(pdbID,4);
						if(GUILayout.Button(new GUIContent("Fetch PDB","Fetch a PDB file from the PDB server")))
						{
							id = pdbID;
							UIData.fetchPDBFile = true;
							UIData.isOpenFile = true;
							GUIMoleculeController.menuOpen_show=false;
							UIData.atomtype=UIData.AtomType.particleball;
							UIData.bondtype=UIData.BondType.nobond;
						}
						GUILayout.EndHorizontal();
					}
					GUILayout.BeginHorizontal();
					if (!UIData.hasMoleculeDisplay)
					{
						if(GUILayout.Button (new GUIContent("Open File From Disk","Load a pdb file from disk")))
						{
							m_fileBrowser = new ImprovedFileBrowser(
		                		new Rect(400, 100, 600, 500),
		                		"Choose PDB/json/obj file",
		                		OpenFileCallback,
		                		m_lastOpenDir
		            		);
							//m_fileBrowser.SelectionPattern = "*.pdb|*.xgmml";
							m_fileBrowser.DirectoryImage = directoryimage; 
							m_fileBrowser.FileImage = fileimage;
							GUIMoleculeController.menuOpen_show=false;
						}
					}
					else
					{
						if(GUILayout.Button(new GUIContent("Clear","Clear the scene")))
						{
							id="";
							/*ToDo: Delete
							 * inputURL.ClearURL(id);
							 * */
							UIData.isclear=true;
							GUIMoleculeController.pdbgen = false;
							UIData.hasMoleculeDisplay = false;
						}
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.BeginHorizontal();
				if (id=="")
				{
					UIData.readHetAtom = GUILayout.Toggle (UIData.readHetAtom, "Read Hetero Atoms ?");
				}
				GUILayout.EndHorizontal();
				#endif
					
				#if UNITY_WEBPLAYER
				{
	// 				GUILayout.BeginHorizontal();
	// 				GUILayout.Label("Server Address:");
	// 				GUILayout.EndHorizontal();
	// 				GUILayout.BeginHorizontal();
	// 				idField=GUILayout.TextField(idField,15);
	// 				PortField=GUILayout.TextField(PortField,4);
	// 				FunctionConfig.id=idField;
	// 				FunctionConfig.port=PortField;
	// 				GUILayout.EndHorizontal();
	// 				if(!LoginFlag)
	// 				{
						
	// 					if(GUILayout.Button("Login"))
	// 					{
	// 						Debug.Log("Login1");
	// 						UnitySocket.SocketConnection(FunctionConfig.id,int.Parse(FunctionConfig.port));
	// 						Debug.Log("Login2");
	// 						LoginFlag=true;
	// 						Debug.Log("Login3");
	// 						Debug.Log(UIData.loginSucess);
							
	// 					}		
							
	// 				}
	// 				else 
	// 				{
	// 					GUILayout.BeginHorizontal();
	// 					if(UIData.loginSucess)
	// 					{
	// 						GUILayout.Label("Login Success!");
	// 					}
	// 					else
	// 					{	
	// //						Debug.Log(UIData.loginSucess);
	// 						GUILayout.Label("Login Error!",GUILayout.Width(80));
	// 						if(GUILayout.Button("Login Again",GUILayout.Width(80)))
	// 						{
								
	// 							UnitySocket.SocketConnection(FunctionConfig.id,int.Parse(FunctionConfig.port));
							
	// 							LoginAgainFlag=true;
								
	// 						}
	// 					}
						
	// 					GUILayout.EndHorizontal();
	// 				}

					GUILayout.BeginHorizontal();
					if(GUILayout.Button("Main Menu"))
					{
						UIData.isclear = true;
						GUIMoleculeController.pdbgen = false;
						UIData.hasMoleculeDisplay = false;
						Application.LoadLevel("MainMenu");
					}
					GUILayout.EndHorizontal();

				}
				#endif
				GUILayout.EndArea();
				
				
			}
//			gUIMoleculeController.RenderHelp();
//			gUIMoleculeController.SetOpenMenu()
			gUIMoleculeController.SetAtomMenu();
			gUIMoleculeController.SetSurfaceMenu();
			gUIMoleculeController.SetBfactorMenu();
			gUIMoleculeController.SetFieldMenu();
			gUIMoleculeController.SetManipulatorMenu();
			gUIMoleculeController.SetMnipulatormove();
			gUIMoleculeController.DisplayGUI();
			gUIMoleculeController.SetAtomType();
			gUIMoleculeController.SetBondType();
			gUIMoleculeController.SetHyperBall();
			gUIMoleculeController.SetEffectType();
			gUIMoleculeController.SetSurfaceTexture();
			gUIMoleculeController.SetSurfaceCut();
			gUIMoleculeController.SetSurtfaceMobileCut();
			gUIMoleculeController.SetBackGroundType();
			gUIMoleculeController.SetMetaphorType();
			gUIMoleculeController.CameraStop();
			gUIMoleculeController.RenderHelp();
			SetHyperballMatCapTexture();
			
			if( GUIMoleculeController.m_colorPicker != null )
			{
				changeAllColor();
			}
		
				
			// }
//			GUI.Label ( new Rect(120,Screen.height-35,Screen.width-360,20), GUI.tooltip);

		}
		/*
		 * ToDo : Delete
		public string  getURL()
		{
			
			return inputURL.GetURL();
		}
		
		public string getID()
		{
			return id;
		}
		*/
		/**
		 * 
		 * 
		 * 
		 */
		public void SetAtomScale()
		{
			
//				GUIContent c=new GUIContent("Set Atom scale");
//				GUI.Window( 1, new Rect( Screen.width-295,0, 170, 300 ), loadGUI, "");
				if(!UIData.hiddenUIbutFPS)
				{
					GUI.Window( 40, new Rect( Screen.width-295,0, 290, 350 ), loadGUI, "Colors");
				}
				else
				{
					GUI.Window( 40, new Rect( Screen.width-295,0, 290, 65 ), loadGUI, "Colors");
				}
				

//			if(display)			
//			{
//				GUI.Window( 41, new Rect( Screen.width-265,365, 250, 260 ), DisplayColors, "Colors");
//			}
		}
		
		public void SetHyperballMatCapTexture()
		{
//			GUIContent texturehyper = new GUIContent("Set Matp Cap texture");
			if (m_texture && UIData.atomtype == UIData.AtomType.hyperball)
			{
				GUI.Window(41,GUIMoleculeController.texturetype_rect, loadtexture, "");
			}
//			if(displayChoice)			
//			{
//				GUI.Window( 2, new Rect( Screen.width-295,355, 290, 200 ), DisplayColors, "");
//			}
//			
		}

		private void loadGUI(int a)
		{
			
			
			// check on http://docs.unity3d.com/Documentation/ScriptReference/Texture2D.SetPixels.html


			
			GUILayout.Box("Atoms Parameters"+"          FPS:"+MoleculeModel.FPS+"  ");
			GUILayout.BeginHorizontal();
			GUILayout.Label("Name",GUILayout.MinWidth(90));
			GUILayout.Label("Scale",GUILayout.MinWidth(80));
			GUILayout.Label("Color",GUILayout.MinWidth(50));	
			GUILayout.Label("Number",GUILayout.MinWidth(50));
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			MoleculeModel.carbonScale = gUIMoleculeController.LabelSlider (MoleculeModel.carbonScale, 50, 150.0f, "Carbon  "+(int)(MoleculeModel.carbonScale*10)/10, "Determines atom radius",true,80,90);
 
//			tex =	(Texture2D)Resources.Load("color");
			if(GUILayout.Button(colCarbon,GUILayout.MinWidth(50),GUILayout.MinHeight(20))){
				GUIMoleculeController.CreateColorPicker(carbon_color, "Carbon Color");
			}
			GUILayout.Label(MoleculeModel.carbonNumber,GUILayout.MaxWidth(150));
			GUILayout.EndHorizontal();
			
	
			GUILayout.BeginHorizontal();
			MoleculeModel.nitrogenScale = gUIMoleculeController.LabelSlider (MoleculeModel.nitrogenScale, 50, 150.0f, "Nitrogen  "+(int)(MoleculeModel.nitrogenScale*10)/10, "Determines atom radius",true,80,90);
//			Texture tex = Resources.Load("color.png");
			if(GUILayout.Button(colNitrogen,GUILayout.MinWidth(50),GUILayout.MinHeight(20))){
				GUIMoleculeController.CreateColorPicker(nitrogen_color, "Nitrogen Color");
			}
			GUILayout.Label(MoleculeModel.nitrogenNumber,GUILayout.MaxWidth(150));
			GUILayout.EndHorizontal();
			
			
			GUILayout.BeginHorizontal();
			MoleculeModel.oxygenScale = gUIMoleculeController.LabelSlider (MoleculeModel.oxygenScale, 50, 150.0f, "Oxygen  "+(int)(MoleculeModel.oxygenScale*10)/10, "Determines atom radius",true,80,90);
//			Texture tex = Resources.Load("color.png");
			if(GUILayout.Button(colOxygen,GUILayout.MinWidth(50),GUILayout.MinHeight(20))){
				GUIMoleculeController.CreateColorPicker(oxygen_color, "Oxygen Color");
			}
			GUILayout.Label(MoleculeModel.oxygenNumber,GUILayout.MaxWidth(150));
			GUILayout.EndHorizontal();
			
			
			GUILayout.BeginHorizontal();
			MoleculeModel.sulphurScale =gUIMoleculeController. LabelSlider (MoleculeModel.sulphurScale, 50, 150.0f, "Sulphur  "+(int)(MoleculeModel.sulphurScale*10)/10, "Determines atom radius",true,80,90);
//			Texture tex = Resources.Load("color.png");
			if(GUILayout.Button(colSulphur,GUILayout.MinWidth(50),GUILayout.MinHeight(20))){
				GUIMoleculeController.CreateColorPicker(sulphur_color, "Sulphur Color");
			}
			GUILayout.Label(MoleculeModel.sulphurNumber,GUILayout.MaxWidth(150));
			GUILayout.EndHorizontal();
			
			
			GUILayout.BeginHorizontal();
			MoleculeModel.phosphorusScale = gUIMoleculeController.LabelSlider (MoleculeModel.phosphorusScale, 50, 150.0f, "Phosphor  "+(int)(MoleculeModel.phosphorusScale*10)/10, "Determines atom radius",true,80,90);
//			Texture tex = Resources.Load("color.png");
			if(GUILayout.Button(colPhosphor,GUILayout.MinWidth(50),GUILayout.MinHeight(20))){
				GUIMoleculeController.CreateColorPicker(phosphorus_color, "Phosphorus Color");
			}
			GUILayout.Label(MoleculeModel.phosphorusNumber,GUILayout.MaxWidth(150));
			GUILayout.EndHorizontal();
			
			
			GUILayout.BeginHorizontal();
			MoleculeModel.hydrogenScale = gUIMoleculeController.LabelSlider (MoleculeModel.hydrogenScale, 50, 150.0f, "Hydrogen  "+(int)(MoleculeModel.hydrogenScale*10)/10, "Determines atom radius",true,80,90);
//			Texture tex = Resources.Load("color.png");
			if(GUILayout.Button(colHydrogen,GUILayout.MinWidth(50),GUILayout.MinHeight(20))){
				GUIMoleculeController.CreateColorPicker(hydrogen_color, "Hydrogen Color");
			}
			GUILayout.Label(MoleculeModel.hydrogenNumber,GUILayout.MaxWidth(150));
			GUILayout.EndHorizontal();
			
			
			GUILayout.BeginHorizontal();
			MoleculeModel.unknownScale = gUIMoleculeController.LabelSlider (MoleculeModel.unknownScale, 50, 150.0f, "Unknow  "+(int)(MoleculeModel.unknownScale*10)/10, "Determines atom radius",true,80,90);
//			Texture tex = Resources.Load("color.png");
			if(GUILayout.Button(colUnknow,GUILayout.MinWidth(50),GUILayout.MinHeight(20))){
				GUIMoleculeController.CreateColorPicker(unknown_color, "Unknown Color");
			}
						GUILayout.Label(MoleculeModel.unknownNumber,GUILayout.MaxWidth(150));

			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			if( GUILayout.Button(new GUIContent("All white", "Change all colors to white"))){
				// m_colorpick_Atom=null;
				oxygen_color.color = Color.white;
				sulphur_color.color = Color.white;
				carbon_color.color = Color.white;
				nitrogen_color.color = Color.white;
				phosphorus_color.color = Color.white;
				hydrogen_color.color = Color.white;
				unknown_color.color = Color.white;
				changeAllColor();
				UIData.isConfirm=true;
				}
			if (GUILayout.Button(new GUIContent("Goodsell", "Change all colors to David Goodsell style colors"))){
				oxygen_color.color = new Color(0.95f,0.76f,0.76f,1f);
				nitrogen_color.color = new Color(0.75f,0.76f,0.94f,1f);
				sulphur_color.color = new Color(0.85f,0.84f,0.46f,1f);
				carbon_color.color = new Color(0.76f,0.76f,0.76f,1f);
				phosphorus_color.color = new Color(0.99f,0.82f,0.59f,1f);
				
				hydrogen_color.color = new Color(0.95f,0.95f,0.95f,1f);
				
				unknown_color.color = new Color(0.55f,0.55f,0.55f,1f);

				changeAllColor();
				UIData.isConfirm=true;
				}
			if (GUILayout.Button(new GUIContent("Watercolor", "Change all colors to Watercolor palette"))){
				oxygen_color.color = new Color(0.60f,0.13f,0.11f,1f);
				nitrogen_color.color = new Color(0.19f,0.27f,0.63f,1f);
				sulphur_color.color = new Color(0.98f,0.91f,0.44f,1f);
				carbon_color.color = new Color(0.55f,0.86f,0.89f,1f);
				phosphorus_color.color = new Color(0.99f,0.69f,0.28f,1f);
				
				hydrogen_color.color = new Color(0.99f,0.98f,0.96f,1f);
				
				unknown_color.color = new Color(0.16f,0.17f,0.29f,1f);

				changeAllColor();
				UIData.isConfirm=true;
				}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("Pastel", "Change all colors to pastel"))){
				// m_colorpick_Atom=null;
				oxygen_color.color = new  Color(0.827f,0.294f,0.333f,1f);
				sulphur_color.color = new  Color(1f,0.839f,0.325f,1f);
				carbon_color.color = new  Color(0.282f,0.6f,0.498f,1f);
				nitrogen_color.color =  new  Color(0.443f,0.662f,0.882f,1f);
				phosphorus_color.color = new  Color(0.960f,0.521f,0.313f,1f);
				hydrogen_color.color = Color.white;
				unknown_color.color = Color.black;

				changeAllColor();
				UIData.isConfirm=true;
				}

			if (GUILayout.Button(new GUIContent("CPK", "A CPK-like atom color palette"))){
				oxygen_color.color = new Color(0.78f,0.0f,0.09f,1f);
				nitrogen_color.color = new Color(0.21f,0.67f,0.92f,1f);
				sulphur_color.color = new Color(0.86f,0.84f,0.04f,1f);
				carbon_color.color = new Color(0.02f,0.02f,0.03f,1f);
				phosphorus_color.color = new Color(1.0f,0.60f,0.0f,1f);
				
				hydrogen_color.color = new Color(1.0f,1.00f,0.99f,1f);
				
				unknown_color.color = new Color(0.03f,0.56f,0.26f,1f);

				changeAllColor();
				UIData.isConfirm=true;
				}

			if(GUILayout.Button(new GUIContent("Basic","Set previous default color parameters (quite intense colors)")))
			{
				// m_colorpick_Atom=null;
				MoleculeModel.carbonScale=100f;
		 		MoleculeModel.nitrogenScale=100f;
		 		MoleculeModel.oxygenScale=100f;
		 		MoleculeModel.sulphurScale=100f;
		 		MoleculeModel.phosphorusScale=100f;
		 		MoleculeModel.hydrogenScale=100f;
		 		
		 		MoleculeModel.unknownScale=100f;
				
				oxygen_color.color = Color.red;
				sulphur_color.color = Color.yellow;
				carbon_color.color = Color.green;
				nitrogen_color.color = Color.blue;
				phosphorus_color.color = new Color(0.6f,0.3f,0.0f,1f);
				hydrogen_color.color = Color.white;			
				unknown_color.color = Color.black;
		
				changeAllColor();
				UIData.isConfirm=true;
			}

			if (GUILayout.Button(new GUIContent("IUPAC?", "A IUPAC color palette (?)"))){
				oxygen_color.color = new Color(0.21f,0.67f,0.92f,1f);
				nitrogen_color.color = new Color(0.03f,0.56f,0.26f,1f);
				sulphur_color.color = new Color(0.86f,0.84f,0.04f,1f);
				carbon_color.color = new Color(1.0f,1.00f,0.99f,1f);
				phosphorus_color.color = new Color(1.0f,0.60f,0.0f,1f);
				
				hydrogen_color.color = new Color(0.02f,0.02f,0.03f,1f);
				
				unknown_color.color = new Color(0.78f,0.0f,0.09f,1f);

				changeAllColor();
				UIData.isConfirm=true;
				}
			GUILayout.EndHorizontal();


			if(GUILayout.Button(new GUIContent("Confirm","Confirm all modifications")))
			{
				
				UIData.isConfirm=true;
			}

			GUILayout.BeginHorizontal();
			GUILayout.Label("Atom count:",GUILayout.MaxWidth(80));	
			GUILayout.Label(MoleculeModel.atomsnumber.ToString(),GUILayout.MaxWidth(60));	
			
			GUILayout.Label("Bond count:",GUILayout.MaxWidth(80));	
			GUILayout.Label(MoleculeModel.bondsnumber.ToString(),GUILayout.MaxWidth(60));	
			GUILayout.EndHorizontal();

			if (Event.current.type == EventType.Repaint)
            	MoleculeModel.newtooltip = GUI.tooltip;

		}
		
		private void changeAllColor(){
			MoleculeModel.carbonColor = carbon_color.color;		
			MoleculeModel.nitrogenColor = nitrogen_color.color;
			MoleculeModel.oxygenColor = oxygen_color.color;
			MoleculeModel.sulphurColor = sulphur_color.color;
			MoleculeModel.phosphorusColor = phosphorus_color.color;
			MoleculeModel.hydrogenColor = hydrogen_color.color;
			MoleculeModel.unknownColor = unknown_color.color;

			for (int i =0; i <200; i++){
				colcarbonNew[i]   =MoleculeModel.carbonColor;
				colNitrogenNew[i] =MoleculeModel.nitrogenColor;
				colOxygenNew[i]   =MoleculeModel.oxygenColor;
				colPhosphorNew[i] =MoleculeModel.phosphorusColor;
				colHydrogenNew[i] =MoleculeModel.hydrogenColor;
				colSulphurNew[i]  =MoleculeModel.sulphurColor;
				colUnknowNew[i]   =MoleculeModel.unknownColor;

			}
			
			colCarbon.SetPixels(colcarbonNew);
			colCarbon.Apply(true);
			colNitrogen.SetPixels(colNitrogenNew);
			colNitrogen.Apply(true);
			colOxygen.SetPixels(colOxygenNew);
			colOxygen.Apply(true);	
			colSulphur.SetPixels(colSulphurNew);
			colSulphur.Apply(true);	
			colPhosphor.SetPixels(colPhosphorNew);
			colPhosphor.Apply(true);
			colHydrogen.SetPixels(colHydrogenNew);
			colHydrogen.Apply(true);
			colUnknow.SetPixels(colUnknowNew);
			colUnknow.Apply(true);
					
		}
		

		public void FileSelectedCallback(string path) {
	        m_fileBrowser = null;
			if (path!=null)
			{
		        m_textPath = path;
		        m_last_texture_dir = System.IO.Path.GetDirectoryName(path);
				WWW www = new WWW("file://"+m_textPath);
				BallUpdateHB.SetTexture(www.texture);
			}
			GUIMoleculeController.FileBrowser_show2=false;
			// Debug.Log("Mis a false");
		}
		
		// HELPER FUNCTION TO FILL A TEXTURE CHOICE MENU WITH UP TO 15 BOXES
		private void textureMenu(string texDir,string[] texList, string texDescr){
			GUI.Label(new Rect(0,0,290,20),"Atoms Texture - " + texDescr,CentredText);
			GUILayout.BeginHorizontal();
			if(GUILayout.Button(new GUIContent("<<","Go to previous series of textures"))) // cycle through texture sets
			{ 
				texture_set--; 
				//Skip fun textures for the article version
//				if(texture_set == 4)
//					texture_set = 3;
				
				if (only_best_textures) {
					if(texture_set>0) texture_set = -texture_set;
					if (texture_set < besttexSet_min) 
						texture_set = -1; 
				} else {
				if (texture_set < 1) 
					texture_set = texSet_max; 
				}
			}			

//			if(GUILayout.Button(new GUIContent("Confirm","Confirm all the modification of the molecule.")))
			if(GUILayout.Button(new GUIContent("Open","Open custom texture image from disk")))
			{	
				m_fileBrowser = new ImprovedFileBrowser(
                    new Rect(400, 100, 600, 500),
                    "Choose Image File",
                    FileSelectedCallback,
                    m_last_texture_dir
                );
				//m_texture=false;
			}
			
			if(GUILayout.Button(new GUIContent(">>","Go to next series of textures"))) // cycle through texture sets
			{ 
				texture_set++; 
				//Skip fun textures for the article version
//				if(texture_set == 4)
//					texture_set = 5;

				if (only_best_textures) {
				if(texture_set>0) texture_set = -texture_set;

				if (texture_set > -1) 
					texture_set = besttexSet_min; 
				} else {
				if (texture_set > texSet_max) 
					texture_set = 1; 
				}
			}			
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();			

			// check whether texList has more than 15 entries and raise an error!!
			int i=0;
//			GUILayout.EndHorizontal(); GUILayout.Box(texDescr); GUILayout.BeginHorizontal();
			foreach(string texFil in texList) {
				i++; if(i>5) {GUILayout.EndHorizontal(); GUILayout.BeginHorizontal(); i=1;}
//				if(GUILayout.Button((Texture)Resources.Load(texDir+texFil),GUILayout.Width(50),GUILayout.Height(50))) { 
				if(GUILayout.Button(new GUIContent((Texture)Resources.Load(texDir+texFil),texFil),GUILayout.Width(50),GUILayout.Height(50))) 
				{ 
					if(texFil == "None")
						BallUpdateHB.SetTexture("lit_spheres/divers/daphz05");
					else
						BallUpdateHB.SetTexture(texDir+texFil); 
				}
				
			}
			GUILayout.EndHorizontal();
//			GUILayout.Label(texDescr);
			if (Event.current.type == EventType.Repaint)
            	MoleculeModel.newtooltip = GUI.tooltip;

		}
		
		
		
//		private void loadtexture2(int a){ // this is an old version ??
//			
//			GUILayout.BeginHorizontal();
//			if(GUILayout.Button(new GUIContent("Confirm","Confirm all modifications")))
//			{
//				
//				m_texture=false;
//			}			
//			GUILayout.EndHorizontal();
//			GUILayout.BeginHorizontal();
//			Texture2D none;
//			none = (Texture2D)Resources.Load("");
//			if(GUILayout.Button(none,GUILayout.Width(50),GUILayout.Height(50)))
//			{
//				
//				BallUpdateHB.texture="";
//			}	
//			Texture2D daphz05;
//			daphz05 = (Texture2D)Resources.Load("lit_spheres/divers/daphz05");
//			if(GUILayout.Button(daphz05,GUILayout.Width(50),GUILayout.Height(50)))
//			{
//				
//				BallUpdateHB.texture="lit_spheres/divers/daphz05";
//			}
//			Texture2D daphz1;
//			daphz1 = (Texture2D)Resources.Load("lit_spheres/divers/daphz1");
//			if(GUILayout.Button(daphz1,GUILayout.Width(50),GUILayout.Height(50)))
//			{
//				
//				BallUpdateHB.texture="lit_spheres/divers/daphz1";
//			}	
//			Texture2D hayward_spheregray;
//			hayward_spheregray = (Texture2D)Resources.Load("hayward_spheregray");
//			if(GUILayout.Button(hayward_spheregray,GUILayout.Width(50),GUILayout.Height(50)))
//			{
//				
//				BallUpdateHB.texture="hayward_spheregray";
//				
//			}
//			GUILayout.EndHorizontal();
//			GUILayout.BeginHorizontal();
//			Texture2D greentext;
//			greentext = (Texture2D)Resources.Load("lit_spheres/objets/green_glass_860");
//			if(GUILayout.Button(greentext,GUILayout.Width(50),GUILayout.Height(50)))
//			{
//				
//				BallUpdateHB.texture="lit_spheres/objets/green_glass_860";
//			}	
//			Texture2D toon;
//			toon = (Texture2D)Resources.Load("lit_spheres/dessin/toon");
//			if(GUILayout.Button(toon,GUILayout.Width(50),GUILayout.Height(50)))
//			{
//				
//				BallUpdateHB.texture="lit_spheres/dessin/toon";
//			}
//			
//			Texture2D blood;
//			blood = (Texture2D)Resources.Load("lit_spheres/objets/blood");
//			if(GUILayout.Button(blood,GUILayout.Width(50),GUILayout.Height(50)))
//			{
//				
//				BallUpdateHB.texture="lit_spheres/objets/blood";
//			}
//			
//			Texture2D blue;
//			blue = (Texture2D)Resources.Load("lit_spheres/dessin/blue");
//			if(GUILayout.Button(blue,GUILayout.Width(50),GUILayout.Height(50)))
//			{
//				
//				BallUpdateHB.texture="lit_spheres/dessin/blue";
//			}
//			
//			Texture2D draw3;
//			draw3 = (Texture2D)Resources.Load("lit_spheres/dessin/draw3");
//			if(GUILayout.Button(draw3,GUILayout.Width(50),GUILayout.Height(50)))
//			{
//				
//				BallUpdateHB.texture="lit_spheres/dessin/draw3";
//			}	
//			GUILayout.EndHorizontal();
//		}
	
		/**
		 * Provide a dialog for texture selection.
		 * There are two modes depending on the only_best_textures boolean. If it is true, only
		 * texture sets with negative numbers are displayed. They are supposed to be a condensed
		 * selection of the most useful textures. If only_best_textures is false, the very
		 * extensive list of all catalogued textures is available.
		 **/
		public void loadtexture(int a){
//			string[] texList;
			
			if(only_best_textures) { if(texture_set>0) texture_set=-1; }; // make sure values are negative for best textures

			// ######################################################################### TOP TEXTURES *************************************
			if (texture_set == -1) {
					string[] texList = { "None",
					"draw3","article-new-ehow-images-a02-00-ep-draw-crosshatching-hatching-800x800","05-how-shading-crosshatch-3b",
					"400px-Draw-a-Sphere-Step-7","draw-shade-spheres-1.3-800X800", "sphere_1_mth", "images-7", "toon",
					"georgetownatelier_sphere_posterized", "sphere-_tutorial_6", "sphere-1p", "sphere1_1","sphere1copy","tumblr_lsyq66Y15D1r0o2eb"};
				textureMenu ("lit_spheres/dessin/", texList, "Draw Style #1");
			}
	
			if (texture_set == -5) {
					string[] texList = { 
					"how_to_draw_a_basic_sphere_by_unknowntone-d4yfqee", "E-C-S (386)","Sketch 10 - sphere1","Sketch 9 - sphere","zbVZE","0-2",
					"CharcoalSphere","74027","8289_Making_a_metallic_sphere_", "crayon1","b11","chiaroscuro-valuescale","draw-shade-spheres-1.4-800X800",
					"400px-Draw-a-Sphere-Step-8","sph_draw_MC"};
				textureMenu ("lit_spheres/dessin/", texList, "Draw Style #2");
			}

			if (texture_set == -4) {
				string[] texList = {
					 "psurftex/2-photos-sphere 2","psurftex/2-photos-sphere","psurftex/1396091264285309","psurftex/basic_shapes_monochrome_oil_painting-(0)",
					 "psurftex/starter sphere240 2","psurftex/silvery2","objets/MatcapQuartz","artext/white_01","artext/shopblack2","artext/shine_blue",
					 "objets/moon2","objets/planet2","objets/bowling1","metal/bronze1","metal/bille-en-acier-359590"
				};
				textureMenu ("lit_spheres/", texList, "Photo Style");
			}
			
			if (texture_set == -3) {
				string[] texList = {
					"dessin/hayward_sphere","dessin/basic_shapes_monochrome_oil_painting-(81)","dessin/sphere-2j","dessin/hqdefault",
					"artistic/caspern","artistic/images","divers/draw2","wood/wood1","glass/glass","artext/or","artext/greenglass","artext/water1",
					"artext/orpur","artext/5_04","artext/banane"};
				textureMenu ("lit_spheres/", texList, "Mixed Styles #1");
			}
			
			if (texture_set == -2) {
				string[] texList = {
					"artext/perle","artext/1stwindow","objets/green_glass_860","artext/kirsch","artext/expo","artext/natural_01",
					"organic/bone","organic/skin","wood/sph_orange_MC","divers/binary-sphere-logo"
				};
				textureMenu ("lit_spheres/", texList, "Mixed Styles #2");
			}
			
				
			// ######################################################################### DESSIN
			if (texture_set == 1) {
					string[] texList = { "None","crayon1","toon","draw3","melon1", "74027", "287457d8aba0f677ac18a913b9f0f441", "article-new-ehow-images-a02-00-ep-draw-crosshatching-hatching-800x800",
				"bruckner","gray","0-2","05-how-shading-crosshatch-3b","400px-Draw-a-Sphere-Step-7","8289_Making_a_metallic_sphere_","b11"};
				textureMenu ("lit_spheres/dessin/", texList, "Drawn 1");
			}
							
			// ######################################################################### DESSIN #2
			if (texture_set == 2) {
				string[] texList = { "chiaroscuro-valuescale","CharcoalSphere","cracked_sphere","draw-shade-spheres-1.3-800X800","how_to_draw_a_basic_sphere_by_unknowntone-d4yfqee",
				"E-C-S (386)","escher_3_spheres_1 2","escher_3_spheres_1 3","droppedImage","draw-shade-spheres-1.4-800X800",
				"ex2_2_carter","eye2","fig14-charcoal-sphere","georgetownatelier_sphere_posterized","GlhTq"};
				textureMenu ("lit_spheres/dessin/", texList, "Drawn 2");
			}
							
			// ######################################################################### DESSIN #3
			if (texture_set == 3) {
				string[] texList = { "GRD3_LS03_IMG01","GRD3_LS03_IMG21","images-7","IMG_1339_2","IMG_1345_2",
					"mqdefault","nancy_sphere","pointillism 2","pointillism 3","rZ0eX",
					"Scan23c","Scan23d","shaded-sphere-1","Sketch 10 - sphere1","Sketch 9 - sphere"
				};
				textureMenu ("lit_spheres/dessin/", texList, "Drawn 3");
			}
			
			// ######################################################################### DESSIN #4
			if (texture_set == 4) {
				string[] texList = { "sphere_1_mth","sphere-_tutorial_6","sphere-1j","sphere-1p","sphere-2p",
					"sphere-drawingj1","sphere-exercise-3","sphere1_1","sphere1copy","sphere2-1",
					"sphere2p","SphereAndShadow","spheregif","spheregif2","spherewhatches"
				};
				textureMenu ("lit_spheres/dessin/", texList, "Drawn 4");
			}

			// ######################################################################### DESSIN #5
			if (texture_set == 5) {
				string[] texList = { "starter sphere240 3","starter sphere240","tumblr_lsyq66Y15D1r0o2eb","tYIlA","zbVZE",
					"400px-Draw-a-Sphere-Step-2","400px-Draw-a-Sphere-Step-8","sph_draw_MC",
				};
				textureMenu ("lit_spheres/dessin/", texList, "Drawn 5");
			}

			// ######################################################################### PHOTOS TEXTURES
			if (texture_set == 6) {
				string[] texList = { "2-photos-sphere 2","2-photos-sphere","1396091264285309","basic_shapes_monochrome_oil_painting-(0)","shading-sphere-photo-005",
					"sphere-drawing-reference","starter sphere240 2","thumbnail-140x140-ehow-images-a08-49-it-draw-planets-800x800","silvery2"
				};
				textureMenu ("lit_spheres/psurftex/", texList, "Surface Textures");
			}
							
			// ######################################################################### DESSIN COLORE
			if (texture_set == 7) {
				string[] texList = { "inks","blue","toon2","hayward_sphere","basic_shapes_monochrome_oil_painting-(81)",
					"exercise2","hand_with_reflecting_sphere","hqdefault","source","sphere-2j",
					"spherej","sphere2copykw6","sphere05"};
				textureMenu ("lit_spheres/dessin/", texList, "Drawn 6");
			}
							
			// ######################################################################### OBJECTS
			if (texture_set == 25) {
				string[] texList = { "green_glass_860","blood","petanque1", "moon1", "moon2",
					"earth1", "earth2", "uk1", "planet1","planet2",
					"bowling1","tablet1","bille-en-plastique-359520","bille","blu_green_litsphere_by_jujikabane"
				};
				textureMenu ("lit_spheres/objets/", texList, "Objects #1");
			}

			// ######################################################################### ARTEXT
			if (texture_set == 26) {
				string[] texList = { "belight","blackNblue","kirsch","chocolat","defaultball",
					"disco","disco2","expo","vert","l+s1",
					"l+s2","lightgray","natural_01","natural_02","neon" };
				textureMenu ("lit_spheres/artext/", texList, "Art Text v2 #1");
			}

			// ######################################################################### FUN
			if (texture_set == 27) {
				string[] texList = { "pig1","pumpkin1","skull1", "skull2", "skull3",
					"explode1","china1", "china2","euro1","heart1",
					"head1","head2","ballon" };
				textureMenu ("lit_spheres/fun/", texList, "Fun #1");
			}

			// ######################################################################### BIO
			if (texture_set == 28) {
				string[] texList = { "fluo1","fluo2","fluo3","fluo4","fluo5",
					"fluo6", "fluo7", "fluo8", "fluo9", "fluo10",
					"em1", "em2", "em3" };
				textureMenu ("lit_spheres/bio/", texList, "Bio #1");
			}

			// ######################################################################### METAL
			if (texture_set == 29) {
				string[] texList = { "silvery1", "silvery2", "puzzle1", "black1","chrome1",
					"bronze1","bille-en-acier-359590","steel","gold","metal",
					"silver10"
				};
				textureMenu ("lit_spheres/metal/", texList, "Metals #1");
			}

			// ######################################################################### ORGANIC
			if (texture_set == 30) {
				string[] texList = { "wool1", "nobs1","nobs2","spike1","spike2",
					"splinter1", "leaves1","anemone","blood2","bone",
				"skin","Skin","vessel"
				};
				textureMenu ("lit_spheres/organic/", texList, "Organic #1");
			}

			// ######################################################################### ARTISTIC
			if (texture_set == 8) {
				string[] texList = { "caspern","carspen2","Gloss_Black","images","tableau-moderne-bille"
				};
				textureMenu ("lit_spheres/artistic/", texList, "Artistic #1");
			}

			// ######################################################################### DIVERS
			if (texture_set == 9) {
				string[] texList = { "choco","daphz1","daphz2","daphz3","draw2",
			"email","eye3","eye3gray","eye4","eyes2",
				"eyes2gray", "eyes5", "gooch", "hexa","LitSphere_Example1"
				};
				textureMenu ("lit_spheres/divers/", texList, "Various #1");
			}

			// ######################################################################### WOOD
			if (texture_set == 10) {
				string[] texList = { "wood1","sph_orange_MC"
				};
				textureMenu ("lit_spheres/wood/", texList, "Woods #1");
			}

			// ######################################################################### OBJECTS 2
			if (texture_set == 11) {
				string[] texList = { "bluew","mat","matcap","MatcapQuartz","stone"
				};
				textureMenu ("lit_spheres/objets/", texList, "Objects #2");
			}

			// ######################################################################### GLASS 1
			if (texture_set == 12) {
				string[] texList = { "glass","glass2","glassbw"
				};
				textureMenu ("lit_spheres/glass/", texList, "Glass #1");
			}

			// ######################################################################### DIVERS 2
			if (texture_set == 13) {
				string[] texList = { "matcap_2","material37","noise","truc","vertical1",
					"hyperballparticle4","hyperballparticlegray","inks2","round_alpha","round",
					"round1","round2","roundlight"
				};
				textureMenu ("lit_spheres/divers/", texList, "Various #2");
			}

			// ######################################################################### ARTEXT
			if (texture_set == 14) {
				string[] texList = { "pasha1","piggybank","rougebrillant","red_1","red_simple",
				"redtowhite","rgestampage","rouille","shad1","shad2",
					"shad3","shadow1","shadow2","estampage","simple10"
				};
				textureMenu ("lit_spheres/artext/", texList, "Art Text v2 #2");
			}

			if (texture_set == 15) {
				string[] texList = { "simple2","simple4","simple6","simple7","simple8",
					"simple9","simple_01","simple_02","simple_03","simple_blue",
					"simple_contrast","simple_doberman","simple_goldcopy","simple_gold","simple_grey"
				};
				textureMenu ("lit_spheres/artext/", texList, "Art Text v2 #3");
			}

			if (texture_set == 16) {
				string[] texList = { "transp_01","transp_02","transp_03","transp_04","white_01",
					"white_02","white_03","x","aqua","army",
					"balloon1","balloon2","balloon3","balloon4","noir"
				}; textureMenu ("lit_spheres/artext/", texList, "Art Text v2 #4"); }

			if (texture_set == 17) {
				string[] texList = { "bluebuz","blue1","blue2","blue3","blue4",
					"bronze","chocolatlait","concave","sombre","oeuf",
					"or","gris","verrebuz","greenglass","vertvelours"
				}; textureMenu ("lit_spheres/artext/", texList, "Art Text v2 #5"); }

			if (texture_set == 18) {
				string[] texList = { "hameleon","encre","lightgray","peinture","paint1",
					"paint2","paint3","rosebuz","rose","violet",
					"purple1","rougebuz","red1","red2","removableeye"
				}; textureMenu ("lit_spheres/artext/", texList, "Art Text v2 #6"); }

			if (texture_set == 19) {
				string[] texList = { "water1","water2","water3","rayonx","aquarelle",
					"asperge","banane","noirmetal","bronze2","fruits",
					"glossy","lumiereor","bijou","lemon","orclair"
				}; textureMenu ("lit_spheres/artext/", texList, "Art Text v2 #7"); }

			if (texture_set == 20) {
				string[] texList = { "mango","lune","neon4","verreorange","orpur",
					"raye","transparent","water","jaune","1_01",
					"1_02","1_03","1_04","2_01","2_02"
				}; textureMenu ("lit_spheres/artext/", texList, "Art Text v2 #8"); }

			if (texture_set == 21) {
				string[] texList = { "2_03","2_05","4","5_01","5_02",
					"5_03","5_04","6_01","6_02","6_03",
					"6_04","7","8_01","8_02","art_01"
				}; textureMenu ("lit_spheres/artext/", texList, "Art Text v2 #9"); }

			if (texture_set == 22) {
				string[] texList = { "art_02","art_03","art_04","art_06","art_07",
					"art_10","art_11","art_red","ceramique","keramic_01",
					"perle","rainbow_01","rainbow_02","rainbow_03","rainbow_04"
				}; textureMenu ("lit_spheres/artext/", texList, "Art Text v2 #10"); }

			if (texture_set == 23) {
				string[] texList = { "shine_blue","artball1","artball10","artball11","artball2",
					"artball3","artball4","artball5","artball6","artball7",
					"artball8","artball9","shopblack1","shopblack1invert","shopblack2"
				}; textureMenu ("lit_spheres/artext/", texList, "Art Text v2 #11"); }

			if (texture_set == 24) {
				string[] texList = { "shopblack3","shopblack4","shopgreen1","shopgreen2","shoppink1",
					"shoppink2","shoppink3","shopred1","shopred2","shopred3",
					"shopred4","1stwindow","3","7_12","glass1"
				}; textureMenu ("lit_spheres/artext/", texList, "Art Text v2 #12"); }
// ######################################################################### END
			
		}
		
	}

}
