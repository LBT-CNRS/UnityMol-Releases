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
/// $Id: GUIDisplay.cs 647 2014-08-06 12:20:04Z tubiana $
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

namespace UI {
	using UnityEngine;
	using System.Collections;
	using System.IO;
	using System.Collections.Generic;
	using SocketConnect.UnitySocket;
	using Config;
	using Molecule.View;
	using Molecule.Model;
	using System.Text;
	
	/** !WiP manage GUI, and provide static strings for the GUI.
	 * 
	 * $Id: GUIDisplay.cs 647 2014-08-06 12:20:04Z tubiana $
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
	public class GUIDisplay	{
		public static float guiScale = 1.0f;
		public static float oldGuiScale = guiScale;
		/* TODO:
		 * GET RID OF STATIC VARIABLES ?
		 * See also : Singleton design pattern.
		 */
		public 	static string pdbID="1KX2";
		public	static string pdbServer = "http://www.pdb.org/pdb/files/";
		public 	static string proxyServer = ""; //OLD proxy : cache.ibpc.fr
		public	static string proxyPort = ""; //OLD port : 8080
		private 	   StringBuilder proxyPortValidate;

//		private static string idField="172.27.0.141";
//		private static string PortField="843";
		public    bool   display=false;
		public bool displayChoice=false;
//		private GUIContent []list;
		public static string directorypath="/opt/src/Unity/UnityMol_SVN/";
		//		public static string directorypath="/opt/Unity3D/UnityMol_SVN/";
		public static string file_base_name = "";
		public static string file_extension = ".pdb";
				
		private string id="";
		public InputURL inputURL=new InputURL();
		public static bool m_max=false;
		public static bool m_texture=false; // texture menu is displayed if true
//		public static int texSet_max=30; /*!< Maximum number of full texture pages */ // Actually unused
//		public static int besttexSet_min=-5; /*!< Maximum number of condensed texture pages (negative value!) */
		public static int texture_set=0;
		
		public bool LoginFlag=false;
		public bool LoginAgainFlag=false;

		public GUIMoleculeController gUIMoleculeController= new GUIMoleculeController();
		
		public ImprovedFileBrowser m_fileBrowser;
		public string m_textPath;
		public string m_last_texture_dir = null;
		public string m_lastOpenDir = null ;
		public Texture2D ext_surf;

		private Texture2D directoryimage, fileimage;
		
		public static float oldScale = 100f;
		public static float newScale = 100f;
		
/*		private static Dictionary<string, bool> applyToAtoms = new Dictionary<string, bool>(){
			{"H", false},
			{"C", false},
			{"N", false},
			{"O", false},
			{"P", false},
			{"S", false},
			{"X", false},
		};
*/
		
		public static bool quickSelection = true;
		//public static bool extendedSelection = false;
		
		public static List<string> applyToAtoms = new List<string>();
		private static string applyToRes = "All";
		private static string applyToChain = "All";
		
		public struct RendererInfos {
			public string atom;
			public string residue;
			public string chain;
			public Color color;
			public string texture;
		}
		private static Dictionary<string, List<RendererInfos>> panelDict = new Dictionary<string, List<RendererInfos>>();
		
		// Those are public because "SurfaceTexture" in LoadTypeGUI use them for the Surface Textures Menu.
		// BTW, I don't really get how things are distributed between GUIDisplay, LoadTypeGUI or GUIMoleculeController... [Erwan]
		public static List<string> textureMenuTitles = new List<string>();
		public static List<string[]> textureMenuList = new List<string[]>();
		
		// public to be used in PaperChain script
		public static Dictionary<string, Color> colorByResiduesDict = new Dictionary<string, Color>();
		public static Dictionary<string, Color> ChainColorDict = new Dictionary<string, Color>();
				
		static Color[] colorButtonNew = new Color[200];
		static Texture2D colorButton = new Texture2D(20,10,TextureFormat.ARGB32,false);
		
		static ColorObject buttonColor = new ColorObject(Color.red);

		// MB for centered text		
		protected GUIStyle CentredText {
        	get {
            	if (m_centredText == null) {
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
		
		/// <summary>
		/// Make a box for atom color selecting.
		/// </summary>
		public GUIDisplay() {
			if (Screen.width > 900)
				guiScale = 0.9f;
			if (Screen.width > 1200)
				guiScale = 0.8f;
			if (Screen.width > 1500)
				guiScale = 0.76f;
			if (Screen.width > 1900)
				guiScale = 0.74f;
			if (Screen.width > 2500)
				guiScale = 0.66f;
			
			#if !UNITY_WEBPLAYER
				m_last_texture_dir = System.IO.Directory.GetCurrentDirectory();
				m_lastOpenDir = System.IO.Directory.GetCurrentDirectory();
			#endif

			DebugStreamer.message = " -- GUIDisplay::GUIDisplay()";

		   	DebugStreamer.message = " -- GUIDisplay::GUIDisplay() - allocs";

			for (int i =0; i <200; i++){
				colorButtonNew[i]= Color.red;
			}

			DebugStreamer.message = " -- GUIDisplay::GUIDisplay() - for";
			
			ChangeAllColors();
			
			DebugStreamer.message = " -- GUIDisplay::GUIDisplay() - end";

			// AT:: temporary hack to test webplayer ------------------
			// #if UNITY_WEBPLAYER 
			// {
			// 	UIData.fetchPDBFile = false;
			// 	UIData.isOpenFile = true;
			// 	UIData.atomtype = UIData.AtomType.particleball;
			// 	UIData.bondtype = UIData.BondType.nobond;
			// 	GUIMoleculeController.showOpenMenu=false;
			// }
			// #endif

			// AT:: temporary hack to test webplayer ------------------	
		}
		
		/// <summary>
		/// Debug function
		/// </summary>
		void OnLevelWasLoaded () {
		    Debug.Log ("If you don't see me, Uniteh brOke!");
		}
		
		public void OpenFileCallback(string path) {
			m_fileBrowser = null;
			if(path == null)
				return;
				
			if (UIData.loadHireRNA == false) {
				UIData.ffType = UIData.FFType.atomic;
				AtomModel.InitAtomic();
			}
			else
			{
				UIData.ffType = UIData.FFType.HiRERNA;
				AtomModel.InitHiRERNA();
			}
			
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
		
		/// <summary>
		/// Display a GUI pannel for selecting a PDB on a server or on a local file. 
		/// </summary>
		public void Display() {
			if(!UIData.isRenderDictInit)
				InitRenderDict();
			if(!UIData.isTexturesMenuListInit)
				InitTextureMenuList();

			directoryimage = (Texture2D)Resources.Load("FileBrowser/dossier");
			fileimage=(Texture2D)Resources.Load("FileBrowser/fichiers");
			
			if(GUIMoleculeController.showOpenMenu) {
				GUILayout.BeginArea(Rectangles.openRect);
				#if !UNITY_WEBPLAYER
				{
					//id != "" if a molecule is already open
						
					GUILayout.BeginHorizontal();
					if (!UIData.hasMoleculeDisplay) {
						if(GUILayout.Button (new GUIContent("Open File From Disk", "Load a PDB file from disk"))) {
							m_fileBrowser = new ImprovedFileBrowser(Rectangles.fileBrowserRect, "", OpenFileCallback, m_lastOpenDir);
//							m_fileBrowser.SelectionPattern = "*.pdb|*.xgmml";
							m_fileBrowser.DirectoryImage = directoryimage; 
							m_fileBrowser.FileImage = fileimage;
							GUIMoleculeController.showOpenMenu = false;
							GUIMoleculeController.showSecStructMenu = false;
						}
					}
					else {
						if(GUILayout.Button(new GUIContent("Clear","Clear the scene"))) {
							id="";
							UIData.isclear = true;
							GUIMoleculeController.pdbGen = false;
							UIData.hasMoleculeDisplay = false;
							GUIMoleculeController.toggle_NA_INTERACTIVE = false; // causes all sorts of headaches otherwise
							Camera.main.GetComponent<ClickAtom>().ClearSelection(); // so the halos don't stay after clearing
							// Delete all the Parents to start again on a clear ground
							GameObject.Destroy(Molecule.View.DisplayAtom.AtomCubeStyle.AtomCubeParent);
							GameObject.Destroy(Molecule.View.DisplayAtom.AtomSphereStyle.AtomCubeParent);

							DepthCueing.reset = true;
							AmbientOcclusion.reset = true;
							
							UIData.loadHireRNA = false;
						}
					}
					GUILayout.EndHorizontal();
					
					if (!UIData.hasMoleculeDisplay) {
						int menuWidth = Rectangles.openWidth;
						float pServerWidth = menuWidth * 0.65f;
						float pPortWidth = menuWidth * 0.30f;
					
						GUILayout.BeginHorizontal();
						GUILayout.Label("Proxy Server", GUILayout.Width(pServerWidth));
						GUILayout.Label("Proxy Port", GUILayout.Width(pPortWidth));
						GUILayout.EndHorizontal();
						
						GUILayout.BeginHorizontal();
						proxyServer = GUILayout.TextField(proxyServer, 256, GUILayout.Width(pServerWidth));
//						proxyServer = MyTextField.FixedTextField(proxyServer, 256, 150f) ;
						// Validate the proxyPort : only digits
						proxyPortValidate = new StringBuilder();
						
				        foreach (char c in proxyPort) 
				            if (char.IsDigit(c)) 
				                proxyPortValidate.Append(c);
						
						proxyPort = GUILayout.TextField(proxyPortValidate.ToString(),4);
						GUILayout.EndHorizontal();


						GUILayout.Label("Please input a PDB ID");
						GUILayout.BeginHorizontal();
						pdbID = GUILayout.TextField(pdbID, 4, GUILayout.Width(pPortWidth));
//						pdbID=GUILayout.TextField(pdbID,4);
						if(GUILayout.Button(new GUIContent("Fetch PDB", "Fetch a PDB file from the PDB server"))) {
							id = pdbID;
							UIData.fetchPDBFile = true;
							UIData.isOpenFile = true;
							GUIMoleculeController.showOpenMenu=false;
							UIData.atomtype=UIData.AtomType.particleball;
							UIData.bondtype=UIData.BondType.nobond;
						}
						GUILayout.EndHorizontal();	
					}
					
				}
				if (id==""){
				GUILayout.BeginHorizontal();

					UIData.readHetAtom = GUILayout.Toggle (UIData.readHetAtom, "Read Hetero Atoms?");

					UIData.readWater = GUILayout.Toggle (UIData.readWater, "Read Water?");
				
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
					GUILayout.Label("Connectivity :");
					UIData.connectivity_calc = GUILayout.Toggle (UIData.connectivity_calc, "Calculed?");
					UIData.connectivity_PDB = GUILayout.Toggle (UIData.connectivity_PDB, "from PDB?");
				

				GUILayout.EndHorizontal();
				}
				#endif
					
				#if UNITY_WEBPLAYER
/*				{
	 				GUILayout.BeginHorizontal();
	 				GUILayout.Label("Server Address:");
	 				GUILayout.EndHorizontal();
	 				GUILayout.BeginHorizontal();
	 				idField=GUILayout.TextField(idField,15);
	 				PortField=GUILayout.TextField(PortField,4);
	 				FunctionConfig.id=idField;
	 				FunctionConfig.port=PortField;
	 				GUILayout.EndHorizontal();
	 				if(!LoginFlag)
	 				{
						
	 					if(GUILayout.Button("Login"))
	 					{
	 						Debug.Log("Login1");
	 						UnitySocket.SocketConnection(FunctionConfig.id,int.Parse(FunctionConfig.port));
	 						Debug.Log("Login2");
	 						LoginFlag=true;
	 						Debug.Log("Login3");
	 						Debug.Log(UIData.loginSucess);
							
	 					}		
							
	 				}
	 				else 
	 				{
	 					GUILayout.BeginHorizontal();
	 					if(UIData.loginSucess)
	 					{
	 						GUILayout.Label("Login Success!");
	 					}
	 					else
	 					{	
							Debug.Log(UIData.loginSucess);
	 						GUILayout.Label("Login Error!",GUILayout.Width(80));
	 						if(GUILayout.Button("Login Again",GUILayout.Width(80)))
	 						{
								
	 							UnitySocket.SocketConnection(FunctionConfig.id,int.Parse(FunctionConfig.port));
							
	 							LoginAgainFlag=true;
								
	 						}
	 					}
				
	 					GUILayout.EndHorizontal();
	 				}

					GUILayout.BeginHorizontal();
					if(GUILayout.Button("Main Menu"))
					{
						UIData.isclear = true;
						GUIMoleculeController.pdbGen = false;
						UIData.hasMoleculeDisplay = false;
						Application.LoadLevel("MainMenu");
					}
					GUILayout.EndHorizontal();

				}
*/
				#endif
				GUILayout.EndArea();
				
				
			}
			gUIMoleculeController.SetAtomMenu();
			gUIMoleculeController.SetSecStructMenu();
			gUIMoleculeController.SetSurfaceMenu();
			gUIMoleculeController.SetBfactorMenu();
			gUIMoleculeController.SetFieldMenu();
			gUIMoleculeController.SetManipulatorMenu();
			gUIMoleculeController.SetMnipulatormove();
			gUIMoleculeController.DisplayGUI();
			gUIMoleculeController.SetAtomType();
			gUIMoleculeController.SetBondType();
			gUIMoleculeController.SetCubeLineBond();
			gUIMoleculeController.SetHyperBall();
			gUIMoleculeController.SetEffectType();
			gUIMoleculeController.SetSurfaceTexture();
			gUIMoleculeController.SetSurfaceCut();
			gUIMoleculeController.SetSurtfaceMobileCut();
			gUIMoleculeController.SetBackGroundType();
			gUIMoleculeController.SetMetaphorType();
			gUIMoleculeController.SetAdvMenu();
			gUIMoleculeController.SetGuidedMenu();
			gUIMoleculeController.CameraStop();
			gUIMoleculeController.RenderHelp();
			gUIMoleculeController.setSugarMenu(); //
			gUIMoleculeController.setSugarRibbonsTuneMenu();
			gUIMoleculeController.setColorTuneMenu();
			gUIMoleculeController.SetVRPNMenu();
			gUIMoleculeController.SetMDDriverMenu();
			gUIMoleculeController.SetHydroMenu ();

//			SetHyperballMatCapTexture();
			SetAtomScales();
			
			if( GUIMoleculeController.m_colorPicker != null ){
				ChangeAllColors();
				//ColorPickerCaller ();
			}
			
			if(applyToAtoms == null)
				applyToAtoms.Add("All");
			
			if(applyToAtoms.Count > 1 && applyToAtoms.Contains("All"))
				applyToAtoms.Remove("All");


//			GUI.Label ( new Rect(120,Screen.height-35,Screen.width-360,20), GUI.tooltip);



		}
		
/*		public void SetHyperballMatCapTexture()	{
//			GUIContent texturehyper = new GUIContent("Set Mat Cap texture");
			if (m_texture && UIData.atomtype == UIData.AtomType.hyperball)
				Rectangles.textureRect = GUI.Window(41,Rectangles.textureRect, loadtexture, "");
			
//			if(displayChoice){
//				GUI.Window( 2, new Rect( Screen.width-295,355, 290, 200 ), DisplayColors, "");
//			}

		}
*/
		
		/// <summary>
		/// Sets the atom scales, color and texture menu.
		/// </summary>
		public void SetAtomScales()	{
			if (GUIMoleculeController.showSetAtomScales)
				Rectangles.atomScalesRect = GUILayout.Window(41,Rectangles.atomScalesRect, LoadMenu, "");
		}
		
		/// <summary>
		/// Load the atom scale, color and texture menu with the current texture set.
		/// </summary>
		/// <param name='a'>
		/// ?
		/// </param>
		public void LoadMenu(int a){

			AtomScales ("lit_spheres/", textureMenuList[texture_set], textureMenuTitles[texture_set]);
			
			GUI.DragWindow();
		}
		
/*		/// <summary>
		/// Convert the booleans of applyToAtoms in a List of string. (Necessary for the SetColor and SetTexture functions)
		/// </summary>
		/// <returns>
		/// The atoms of applyToAtoms as a List.
		/// </returns>
		/// <param name='dic'>
		/// Send applyToAtoms here (Dictionary of booleans).
		/// </param>
		private static List<string> ToList(Dictionary<string, bool> dic){
			List<string> sendingAtoms = new List<string>();
			foreach(string key in dic.Keys)
				if(dic[key])
					sendingAtoms.Add(key);
			
			return sendingAtoms;
		}
*/
		
		/// <summary>
		/// Convertir a string to a List of String (Necessary for the SetColor and SetTexture functions)
		/// </summary>
		/// <returns>
		/// The atom as a List
		/// </returns>
		/// <param name='atom'>
		/// Atom.
		/// </param>
		private static List<string> ToList(string atom){
			List<string> sendingAtoms = new List<string>();
			sendingAtoms.Add(atom);
			
			return sendingAtoms;
		}
		
		/// <summary>
		/// Create a ColorPicker with the currents options.
		/// </summary>
		private static void ColorPickerCaller(){
			GUIMoleculeController.CreateColorPicker(buttonColor, "Select a color", applyToAtoms, applyToRes, applyToChain);
		}
		
		/// <summary>
		/// Contents of the atom color/texture/scale menu.
		/// </summary>
		/// <param name='texDir'>
		/// Textures directory.
		/// </param>
		/// <param name='texList'>
		/// Textures list.
		/// </param>
		/// <param name='texDescr'>
		/// Textures description.
		/// </param>
		private void AtomScales(string texDir,string[] texList, string texDescr){ // TODO: Move this ?
			GUIMoleculeController.showSetAtomScales = LoadTypeGUI.SetTitleExit("Atom Colors, Textures and Scales");
			// check on http://docs.unity3d.com/Documentation/ScriptReference/Texture2D.SetPixels.html
			
			int fifth = Rectangles.atomScalesWidth / 5;
			int sliderWidth = (int)(fifth*1.65f);
			int nameWidth = (int)(fifth*1.2f);
			
			
			// ---------- ATOMS CHOICE ----------
			GUILayout.BeginHorizontal();
			GUILayout.Box("Apply changes to:");
			
			if(GUIMoleculeController.toggle_NA_CLICK){
				GUI.color = Color.yellow;
				GUILayout.Label("Atom Selection Mode: All changes applied to selected atoms");
				GUI.color = Color.white;
			}
			else{
				if(GUILayout.Button(new GUIContent("Reset", "Reset all selections"), GUILayout.Width(Rectangles.textureWidth / 8))){
					applyToAtoms.Clear();
					applyToAtoms.Add("All");
					applyToRes = "All";
					applyToChain = "All";
					GUIMoleculeController.m_colorPicker = null;
				}
			}
			
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if(quickSelection)
				GUI.color = new Color(0.5f, 1.0f, 0.5f);
			if(GUIMoleculeController.toggle_NA_CLICK)
				GUI.enabled = false;
			if( GUILayout.Button (new GUIContent ("Quick atoms selection", "Click-and-run mode for atom selection"), GUILayout.Width(Rectangles.textureWidth / 2))){
				quickSelection = true;
				//extendedSelection = false;
				GUIMoleculeController.m_colorPicker = null;
				GUIMoleculeController.showAtomsExtendedMenu = false;
				applyToAtoms.Clear();
				applyToAtoms.Add("All");
			}
			GUI.color = Color.white;
			GUILayout.FlexibleSpace();
			if(!quickSelection)
				GUI.enabled = false;
			if(GUILayout.Button(new GUIContent("All", "Select all atoms"), GUILayout.Width(Rectangles.textureWidth / 6))){
				applyToAtoms.Clear();
				applyToAtoms.Add("All");
				if(GUIMoleculeController.m_colorPicker != null)
					ColorPickerCaller ();
			}

			if(GUILayout.Button(new GUIContent("None", "Unselect all atoms"), GUILayout.Width(Rectangles.textureWidth / 6))){
				applyToAtoms.Clear();
				applyToAtoms.Add("None");
				GUIMoleculeController.m_colorPicker = null;
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			if(applyToAtoms.Contains("H") || applyToAtoms.Contains("All"))
				GUI.color = Color.green;
			if(GUILayout.Button(new GUIContent ("H("+MoleculeModel.hydrogenNumber+")", "Modify hydrogen"))){
				if(applyToAtoms.Contains("H"))
					applyToAtoms.Remove("H");
				else
					applyToAtoms.Add("H");
				if(GUIMoleculeController.m_colorPicker != null)
					ColorPickerCaller ();
			}
			GUI.color = Color.white;
			GUILayout.FlexibleSpace();
			if(applyToAtoms.Contains("C") || applyToAtoms.Contains("All"))
				GUI.color = Color.green;
			if(GUILayout.Button(new GUIContent ("C("+MoleculeModel.carbonNumber+")", "Modify carbon"))){
				if(applyToAtoms.Contains("C"))
					applyToAtoms.Remove("C");
				else
					applyToAtoms.Add("C");
				if(GUIMoleculeController.m_colorPicker != null)
					ColorPickerCaller ();
			}
			GUI.color = Color.white;
			GUILayout.FlexibleSpace();
			if(applyToAtoms.Contains("N") || applyToAtoms.Contains("All"))
				GUI.color = Color.green;
			if(GUILayout.Button(new GUIContent ("N("+MoleculeModel.nitrogenNumber+")", "Modify nitrogen"))){
				if(applyToAtoms.Contains("N"))
					applyToAtoms.Remove("N");
				else
					applyToAtoms.Add("N");
				if(GUIMoleculeController.m_colorPicker != null)
					ColorPickerCaller ();
			}
			GUI.color = Color.white;
			GUILayout.FlexibleSpace();
			if(applyToAtoms.Contains("O") || applyToAtoms.Contains("All"))
				GUI.color = Color.green;
			if(GUILayout.Button(new GUIContent ("O("+MoleculeModel.oxygenNumber+")", "Modify oxygen"))){
				if(applyToAtoms.Contains("O"))
					applyToAtoms.Remove("O");
				else
					applyToAtoms.Add("O");
				if(GUIMoleculeController.m_colorPicker != null)
					ColorPickerCaller ();
			}
			GUI.color = Color.white;
			GUILayout.FlexibleSpace();
			if(applyToAtoms.Contains("P") || applyToAtoms.Contains("All"))
				GUI.color = Color.green;
			if(GUILayout.Button( new GUIContent ("P("+MoleculeModel.phosphorusNumber+")", "Modify phosphorus"))){
				if(applyToAtoms.Contains("P"))
					applyToAtoms.Remove("P");
				else
					applyToAtoms.Add("P");
				if(GUIMoleculeController.m_colorPicker != null)
					ColorPickerCaller ();
			}
			GUI.color = Color.white;
			GUILayout.FlexibleSpace();
			if(applyToAtoms.Contains("S") || applyToAtoms.Contains("All"))
				GUI.color = Color.green;
			if(GUILayout.Button(new GUIContent ("S("+MoleculeModel.sulphurNumber+")", "Modify sulphur"))){
				if(applyToAtoms.Contains("S"))
					applyToAtoms.Remove("S");
				else
					applyToAtoms.Add("S");
				if(GUIMoleculeController.m_colorPicker != null)
					ColorPickerCaller ();
			}
			GUI.color = Color.white;
			GUILayout.FlexibleSpace();
			if(applyToAtoms.Contains("X") || applyToAtoms.Contains("All"))
				GUI.color = Color.green;
			if(GUILayout.Button(new GUIContent ("X("+MoleculeModel.unknownNumber+")", "Modify unknown"))){
				if(applyToAtoms.Contains("X"))
					applyToAtoms.Remove("X");
				else
					applyToAtoms.Add("X");
				if(GUIMoleculeController.m_colorPicker != null)
					ColorPickerCaller ();
			}
			GUI.color = Color.white;
			GUI.enabled = true;
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			if(!quickSelection)
				GUI.color = new Color(0.5f, 1.0f, 0.5f);
			if(GUIMoleculeController.toggle_NA_CLICK)//applyToAtoms["Select"])
				GUI.enabled = false;
			if( GUILayout.Button (new GUIContent ("Extended atoms selection", "In-depth atom selection"), GUILayout.Width(Rectangles.textureWidth / 2))){
				quickSelection = false;
				//extendedSelection = true;
				GUIMoleculeController.m_colorPicker = null;
				GUIMoleculeController.showAtomsExtendedMenu = !GUIMoleculeController.showAtomsExtendedMenu;
				applyToAtoms.Clear();
				applyToAtoms.Add("All");
			}
			GUI.color = Color.white;
//			GUILayout.FlexibleSpace ();
//			GUILayout.EndHorizontal();
			
//			GUILayout.BeginHorizontal();
/*			GUILayout.Label(" : ");
			if(!extendedSelection)
				GUI.enabled = false;
			if(GUILayout.Button(new GUIContent("Extended atoms", "Change atom"), GUILayout.Width(Rectangles.textureWidth / 8)))
				GUIMoleculeController.showAtomsExtendedMenu = !GUIMoleculeController.showAtomsExtendedMenu;
*/
			GUILayout.FlexibleSpace();
			
			GUI.enabled = true;
			GUILayout.EndHorizontal();
			
			
			// ---------- RESIDUES, CHAIN, GRAYSCALE, SELECTION ----------
			GUILayout.BeginHorizontal();
			GUILayout.Label("Residue :");
			if(GUIMoleculeController.toggle_NA_CLICK
				|| !UIData.hasResidues) {
				GUI.enabled = false;
				applyToRes = "All";
			}

			if(GUILayout.Button(new GUIContent(applyToRes, "Change residue"), GUILayout.Width(Rectangles.textureWidth / 8)))
				GUIMoleculeController.showResiduesMenu = !GUIMoleculeController.showResiduesMenu;

			GUI.enabled = true;
			
			GUILayout.FlexibleSpace();
			
			GUILayout.Label(" Chain :");
			if(GUIMoleculeController.toggle_NA_CLICK
				|| !UIData.hasChains) {
				GUI.enabled = false;
				applyToChain = "All";
			}
			if(GUILayout.Button(new GUIContent(applyToChain, "Change chain"), GUILayout.Width(Rectangles.textureWidth / 8))){
				GUIMoleculeController.showChainsMenu = !GUIMoleculeController.showChainsMenu;
			}
			GUI.enabled = true;
			
			GUILayout.FlexibleSpace();
			
			
			GUILayout.EndHorizontal();
			
			// ---------- SCALE, COLOR ----------
			GUILayout.BeginHorizontal();
			GUILayout.Box("Color and Scale :");
			if (GUILayout.Button (new GUIContent ("Panels", "Open colors and textures panels menu"), GUILayout.Width(Rectangles.textureWidth / 4))) {
				GUIMoleculeController.showPanelsMenu = !GUIMoleculeController.showPanelsMenu;
			}
			if(GUILayout.Button(new GUIContent("Reset scales", "Reset all scales to 100"), GUILayout.Width(Rectangles.textureWidth / 4))){
				newScale = 100f;
				GenericManager manager = DisplayMolecule.GetManagers()[0];
				manager.SetRadii(ToList("All"), "All", "All");
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		
			GUILayout.BeginHorizontal();
			//GUILayout.FlexibleSpace();
			if(GUILayout.Button(colorButton,GUILayout.MinWidth(100),GUILayout.MinHeight(20))){
				ColorPickerCaller();
			}
			GUILayout.FlexibleSpace();
			
			newScale = GUIMoleculeController.LabelSlider(newScale, 50, 150.0f,
														"Scale: "+(int)(newScale*10)/10,
														"Determines atom radius",true,sliderWidth,(int)(nameWidth*0.75));
			if(newScale != oldScale){
				GenericManager manager = DisplayMolecule.GetManagers()[0];
				if(!GUIMoleculeController.toggle_NA_CLICK){
					manager.SetRadii(applyToAtoms, applyToRes, applyToChain);
					oldScale = newScale;
				}
				else{
					foreach(GameObject obj in Camera.main.GetComponent<ClickAtom>().objList)
						manager.SetRadii((int)obj.GetComponent<BallUpdate>().number);	
						
					oldScale = newScale;
				}
			}

			//GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			// ---------- TEXTURE ----------
			GUILayout.BeginHorizontal();
			GUILayout.Box("Texture: " + texDescr);
			GUILayout.FlexibleSpace();
			if(UIData.atomtype != UIData.AtomType.hyperball)
				GUI.enabled = false;
			
			UIData.grayscalemode = GUILayout.Toggle (UIData.grayscalemode, new GUIContent ("Grayscale", "Use grayscale version of the texture"));
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			if(GUILayout.Button(new GUIContent("<<","Go to previous series of textures"))){ // Cycle through texture sets
				texture_set--; 
				if(GUIMoleculeController.onlyBestTextures){
					if(texture_set < 0)
						texture_set = 4; // First 5 pages are best textures (0-4)
				}
				else{
					if(texture_set < 5)
						texture_set = textureMenuList.Count - 1;
				}
			}			

			if(GUILayout.Button(new GUIContent("Open","Open custom texture image from disk"))) {	
				m_fileBrowser = new ImprovedFileBrowser(Rectangles.fileBrowserRect,
														"Choose Image File",
														FileSelectedCallback,
														m_last_texture_dir);
			}
			
			if(GUILayout.Button(new GUIContent(">>","Go to next series of textures"))) { // Cycle through texture sets
				texture_set++; 
				if (GUIMoleculeController.onlyBestTextures) {
					if(texture_set>4) // First 5 pages are best textures (0-4)
						texture_set = 0;
				}
				else{
					if (texture_set > textureMenuList.Count - 1)
						texture_set = 5;
				}
			}			
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();			

			// Check whether texList has more than 15 entries and raise an error !
			int i=0;
			int buttonWidth = (int) (Rectangles.atomScalesWidth * 0.18);
			int buttonHeight = (int) ( Rectangles.atomScalesHeight / 7f);
			
			
			foreach(string texFil in texList) {
				i++; if(i>5) {GUILayout.EndHorizontal(); GUILayout.BeginHorizontal(); i=1;}
//				if(GUILayout.Button((Texture)Resources.Load(texDir+texFil),GUILayout.Width(50),GUILayout.Height(50))) { 
				if(GUILayout.Button(new GUIContent((Texture)Resources.Load(texDir+texFil),texFil),GUILayout.Width(buttonWidth),GUILayout.Height(buttonHeight))) {
					GameObject hbManagerObj = GameObject.FindGameObjectWithTag("HBallManager");
					HBallManager hbManager = hbManagerObj.GetComponent<HBallManager>();
					Texture texture;
					if(texFil == "None")
						texture = (Texture)Resources.Load("lit_spheres/divers/daphz05");
					else
						texture = (Texture)Resources.Load(texDir+texFil);
					
					if(UIData.grayscalemode)
						texture = hbManager.ToGray(texture);
					
					if(!GUIMoleculeController.toggle_NA_CLICK){
						hbManager.SetTexture(texture, applyToAtoms, applyToRes, applyToChain);
					}
					else {
						foreach(GameObject obj in Camera.main.GetComponent<ClickAtom>().objList)
							hbManager.SetTexture(texture, (int)obj.GetComponent<BallUpdate>().number);
					}
				}
			}
			GUILayout.EndHorizontal();
			
			GUI.enabled = true;
			
			if (Event.current.type == EventType.Repaint)
            	MoleculeModel.newtooltip = GUI.tooltip;
			
		}
		
		/// <summary>
		/// Updates the colors of the ColorPicker boxes.
		/// </summary>
		public static void ChangeAllColors(){

			for (int i =0; i <200; i++){
				colorButtonNew[i]   =buttonColor.color;
				LoadTypeGUI.helixButtonNew[i] =Ribbons.HELIX_COLOR.color;
				LoadTypeGUI.sheetButtonNew[i] =Ribbons.STRAND_COLOR.color;
				LoadTypeGUI.coilButtonNew[i] =Ribbons.COIL_COLOR.color;
				LoadTypeGUI.chainbuttonAnew[i] = Ribbons.ChainColorA.color;
				LoadTypeGUI.chainbuttonBnew[i] = Ribbons.ChainColorB.color;
				LoadTypeGUI.chainbuttonCnew[i] = Ribbons.ChainColorC.color;
				LoadTypeGUI.chainbuttonDnew[i] = Ribbons.ChainColorD.color;
				LoadTypeGUI.chainbuttonEnew[i] = Ribbons.ChainColorE.color;
			}
			
			colorButton.SetPixels(colorButtonNew);
			colorButton.Apply(true);
			LoadTypeGUI.helixButton.SetPixels(LoadTypeGUI.helixButtonNew);
			LoadTypeGUI.helixButton.Apply(true);
			LoadTypeGUI.sheetButton.SetPixels(LoadTypeGUI.sheetButtonNew);
			LoadTypeGUI.sheetButton.Apply(true);
			LoadTypeGUI.coilButton.SetPixels(LoadTypeGUI.coilButtonNew);
			LoadTypeGUI.coilButton.Apply(true);
			LoadTypeGUI.chainbuttonA.SetPixels (LoadTypeGUI.chainbuttonAnew);
			LoadTypeGUI.chainbuttonA.Apply (true);
			LoadTypeGUI.chainbuttonB.SetPixels (LoadTypeGUI.chainbuttonBnew);
			LoadTypeGUI.chainbuttonB.Apply (true);
			LoadTypeGUI.chainbuttonC.SetPixels (LoadTypeGUI.chainbuttonCnew);
			LoadTypeGUI.chainbuttonC.Apply (true);
			LoadTypeGUI.chainbuttonD.SetPixels (LoadTypeGUI.chainbuttonDnew);
			LoadTypeGUI.chainbuttonD.Apply (true);
			LoadTypeGUI.chainbuttonE.SetPixels (LoadTypeGUI.chainbuttonEnew);
			LoadTypeGUI.chainbuttonE.Apply (true);
		} // end of AtomScales

		
		public void FileSelectedCallback(string path) {
	        m_fileBrowser = null;
			if (path!=null) {
				Debug.Log("Trying to load a texture for the Hyperballs");
		        m_textPath = path;
		        m_last_texture_dir = System.IO.Path.GetDirectoryName(path);
				WWW www = new WWW("file://" + m_textPath);
				GameObject hbManagerObj = GameObject.FindGameObjectWithTag("HBallManager");
				HBallManager hbManager = hbManagerObj.GetComponent<HBallManager>();
				Texture2D grayTexture = new Texture2D(www.texture.width, www.texture.height);
				if(!GUIMoleculeController.toggle_NA_CLICK){
					if(!UIData.grayscalemode){
						hbManager.SetTexture(www.texture, applyToAtoms, applyToRes, applyToChain);
					}
					else{
						grayTexture = hbManager.ToGray(www.texture);
						hbManager.SetTexture(grayTexture, applyToAtoms, applyToRes, applyToChain);
					}
				}
				else {
					if(!UIData.grayscalemode){
						foreach(GameObject obj in Camera.main.GetComponent<ClickAtom>().objList)
							hbManager.SetTexture(www.texture, (int)obj.GetComponent<BallUpdate>().number);
					}
					else{
						grayTexture = hbManager.ToGray(www.texture);
						foreach(GameObject obj in Camera.main.GetComponent<ClickAtom>().objList)
							hbManager.SetTexture(grayTexture, (int)obj.GetComponent<BallUpdate>().number);
					}
				}

			}
			GUIMoleculeController.FileBrowser_show2=false;
//			Debug.Log("Mis a false");
		}
		
		
		/// <summary>
		/// Open the Panel selection Menu to apply premade color and texture panel.
		/// </summary>
		/// <param name='a'>
		/// No idea...
		/// </param>
		public static void PanelsMenu(int a) {
			GUIMoleculeController.showPanelsMenu = LoadTypeGUI.SetTitleExit("Panels Menu");
			
			//---------- To edit/add Color and Texture palettes, please read Assets/Resources/HowToColorPanel.txt ----------
			// NB : the name "ColorPanel.txt" and everything related to it (like panelDict, RenderDict or whatever) doesn't seem really great,
			// but it would be a pain to change them now. [Erwan]
			GUILayout.BeginHorizontal();
			GUILayout.Label("Color Panels");
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			if( GUILayout.Button(new GUIContent("All white", "Change all colors to white"))){
				SetColorPanel("All_White");
			}
			if (GUILayout.Button(new GUIContent("Goodsell", "Change all colors to David Goodsell style colors"))){
				//extendedSelection = false;
				quickSelection = true;
				SetColorPanel("Goodsell");
				quickSelection = false;
				//extendedSelection = true;
				SetColorPanel("Goodsell");
				//O(0.95,0.76,0.76,1);N(0.75,0.76,0.94,1);S(0.85,0.84,0.46,1);C(0.76,0.76,0.76,1);P(0.99,0.82,0.59,1);H(0.95,0.95,0.95,1);X(0.55,0.55,0.55,1)
				// Now with precise coloring of charged O and N !
			}
			if (GUILayout.Button(new GUIContent("Watercolor", "Change all colors to Watercolor palette"))){
				quickSelection = true;
				SetColorPanel("Watercolor");
				//O(0.60,0.13,0.11,1);N(0.19,0.27,0.63,1);S(0.98,0.91,0.44,1);C(0.55,0.86,0.89,1);P(0.99,0.69,0.28,1);H(0.99,0.98,0.96,1);X(0.16,0.17,0.29,1)
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("Pastel", "Change all colors to pastel"))){
				quickSelection = true;
				SetColorPanel("Pastel");
				//O(0.827,0.294,0.333,1);S(1,0.839,0.325,1);C(0.282,0.6,0.498,1);N(0.443,0.662,0.882,1);P(0.960,0.521,0.313,1);P(white);X(black)
			}
			if (GUILayout.Button(new GUIContent("CPK", "A CPK-like atom color palette"))){
				quickSelection = true;
				SetColorPanel("CPK");
				//O(0.78,0.0,0.09,1);N(0.21,0.67,0.92,1);S(0.86,0.84,0.04,1);C(0.02,0.02,0.03,1);P(1.0,0.60,0.0,1);H(1.0,1.00,0.99,1);X(0.03,0.56,0.26,1)
			}
			if(GUILayout.Button(new GUIContent("Basic","Set previous default color parameters (quite intense colors)"))){
				quickSelection = true;
				SetColorPanel("Basic");
				//O(red);S(yellow);C(green);N(blue);P(0.6,0.3,0.0,1);H(white);X(black)
			}
			if (GUILayout.Button(new GUIContent("IUPAC?", "A IUPAC color palette (?)"))){
				quickSelection = true;
				SetColorPanel("IUPAC");
				//O(0.21,0.67,0.92,1);N(0.03,0.56,0.26,1);S(0.86,0.84,0.04,1);C(1.0,1.00,0.99,1);P(1.0,0.60,0.0,1);H(0.02,0.02,0.03,1);X(0.78,0.0,0.09,1)
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if(!UIData.hasChains)
				GUI.enabled = false;
			if (GUILayout.Button(new GUIContent("Chains", "Quick chains coloring"))){
				quickSelection = true;
				SetColorPanel("Chains");
			}
			GUI.enabled = true;
			//T.T. Add coloring for sugar representation
			if (GUILayout.Button(new GUIContent("Sugar", "Quick sugar type coloring"))){
				quickSelection = true;
				SetColorPanel("Sugar");
			}
			if(GUILayout.Button(new GUIContent("ADN/ARN", "Base coloring: A[red] T/U[blue] C[yellow] G[green]"))){
				quickSelection = true;
				SetColorPanel("ADN-ARN");
			}
			if(GUILayout.Button(new GUIContent("HiRE-RNA", "Base coloring: A[red] T/U[blue] C[yellow] G[green]"))){
				quickSelection = true;
				SetColorPanel("HiRERNA");
			}
			GUILayout.EndHorizontal();
		
			GUILayout.BeginHorizontal();
			GUILayout.Label("Texture Panels");
			GUILayout.EndHorizontal();
			
			if(UIData.atomtype != UIData.AtomType.hyperball)
				GUI.enabled = false;
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("Default", "Default color/texture palette"))){
				SetTexturePanel("All_White");
			}
			if (GUILayout.Button(new GUIContent("CPK", "A CPK-like atom textures palette (great with CPK metaphor)"))){
				quickSelection = true;
				SetTexturePanel("CPK");
				SetColorPanel("All_White");
			}
			if(!UIData.hasResidues)
				GUI.enabled = false;
			if (GUILayout.Button(new GUIContent("Acid-Basic", "A texturing for acid (ASP/GLU) and basic (ARG/LYS) residues."))){
				quickSelection = true;
				SetTexturePanel("Acid-Base_Res");
				SetColorPanel("Acid-Base_Res");
			}
			GUI.enabled = true;
			GUILayout.EndHorizontal();

			
			if (Event.current.type == EventType.Repaint)
            	MoleculeModel.newtooltip = GUI.tooltip;
			
			GUI.DragWindow();
		}
		
		/// <summary>
		/// Open the atoms extended selection Menu to apply changes to a specific atom.
		/// </summary>
		/// <param name='a'>
		/// No idea...
		/// </param>
		public static void AtomsExtendedMenu(int a) {
			GUIMoleculeController.showAtomsExtendedMenu = LoadTypeGUI.SetTitleExit("Choose an atom");
			
			int buttonWidth = (int)(Rectangles.residuesMenuWidth / 4.8);
			int count = 0;
			
			GUILayout.BeginHorizontal();
			
			if(applyToAtoms.Contains("All"))
					GUI.color = Color.green;
			if(GUILayout.Button(new GUIContent("All", "All atoms"))){
				applyToAtoms.Clear();
				applyToAtoms.Add("All");
				//GUIMoleculeController.showAtomsExtendedMenu = false;
			}
			GUI.color = Color.white;
			GUILayout.EndHorizontal();
			
			
			while(count < MoleculeModel.existingName.Count){
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				
				for(int i = 0; i < 4; i++){
					if(count < MoleculeModel.existingName.Count){
						if(applyToAtoms.Contains(MoleculeModel.existingName[count]))
							GUI.color = Color.green;
						if(GUILayout.Button(new GUIContent(MoleculeModel.existingName[count], ""), GUILayout.Width(buttonWidth))){
							if(!applyToAtoms.Contains(MoleculeModel.existingName[count]))
								applyToAtoms.Add(MoleculeModel.existingName[count]);
							else
								applyToAtoms.Remove(MoleculeModel.existingName[count]);
							if(GUIMoleculeController.m_colorPicker != null)
								ColorPickerCaller ();
//							GUIMoleculeController.showAtomsExtendedMenu = false;
						}
						GUI.color = Color.white;
					}
					count++;
				}
				
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
			
			if (Event.current.type == EventType.Repaint)
            	MoleculeModel.newtooltip = GUI.tooltip;
			
			GUI.DragWindow();
		}
		
		/// <summary>
		/// Open the Residue selection Menu to apply changes to a specific residue.
		/// </summary>
		/// <param name='a'>
		/// No idea...
		/// </param>
		public static void ResiduesMenu(int a) {
			GUIMoleculeController.showResiduesMenu = LoadTypeGUI.SetTitleExit("Choose a residue");
			
			int buttonWidth = (int)(Rectangles.residuesMenuWidth / 4.8);
			int count = 0;
			
			GUILayout.BeginHorizontal();
			if(applyToRes == "All")
				GUI.color = Color.green;
			if(GUILayout.Button(new GUIContent("All", "All residues"))){
				applyToRes = "All";
				//GUIMoleculeController.showResiduesMenu = false;
			}
			GUI.color = Color.white;
			GUILayout.EndHorizontal();
			
			
			while(count < MoleculeModel.existingRes.Count){
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				
				for(int i = 0; i < 4; i++){
					if(count < MoleculeModel.existingRes.Count){
						if(applyToRes == MoleculeModel.existingRes[count])
							GUI.color = Color.green;
						if(GUILayout.Button(new GUIContent(MoleculeModel.existingRes[count], ""), GUILayout.Width(buttonWidth))){
							applyToRes = MoleculeModel.existingRes[count];
							if(GUIMoleculeController.m_colorPicker != null)
								ColorPickerCaller ();
//							GUIMoleculeController.showResiduesMenu = false;
						}
						GUI.color = Color.white;
					}
					count++;
				}
				
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
			
			if (Event.current.type == EventType.Repaint)
            	MoleculeModel.newtooltip = GUI.tooltip;
			
			GUI.DragWindow();
		}
		
		/// <summary>
		/// Open the Chain selection Menu to apply changes to a specific chain.
		/// </summary>
		/// <param name='a'>
		/// No idea...
		/// </param>
		public static void ChainsMenu(int a) {
			GUIMoleculeController.showChainsMenu = LoadTypeGUI.SetTitleExit("Choose a chain");
			
			int buttonWidth = (int)(Rectangles.residuesMenuWidth / 4.8);
			int count = 0;
			
			GUILayout.BeginHorizontal();
			if(applyToChain == "All")
				GUI.color = Color.green;
			if(GUILayout.Button(new GUIContent("All", "All chains"))){
				applyToChain = "All";
				//GUIMoleculeController.showChainsMenu = false;
			}
			GUI.color = Color.white;
			GUILayout.EndHorizontal();
			
			while(count < MoleculeModel.existingChain.Count){
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				
				for(int i = 0; i < 4; i++){
					if(count < MoleculeModel.existingChain.Count){
						if(applyToChain == MoleculeModel.existingChain[count])
							GUI.color = Color.green;
						if(GUILayout.Button(new GUIContent(MoleculeModel.existingChain[count], ""), GUILayout.Width(buttonWidth))){
							applyToChain = MoleculeModel.existingChain[count];
							if(GUIMoleculeController.m_colorPicker != null)
								ColorPickerCaller ();
//							GUIMoleculeController.showChainsMenu = false;
						}
						GUI.color = Color.white;
					}
					count++;
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
			
			if (Event.current.type == EventType.Repaint)
            	MoleculeModel.newtooltip = GUI.tooltip;
			
			GUI.DragWindow();
		}
			
			
		/// <summary>
		/// Inits the color/texture panel dictionary.
		/// </summary>
		public static void InitRenderDict() {
			TextAsset cPanel = (TextAsset)Resources.Load("ColorPanel");
			StringReader sr = new StringReader(cPanel.text);
			
			string s;
			string panelName = "";
			string[] splittedLine;
			List<RendererInfos> infosFinal = new List<RendererInfos>();
			bool firstLoop = true;
			float R, G, B, A;
			
			while((s=sr.ReadLine())!=null) {
				// The last line of the file MUST be a single "$" or the last panel won't be added.
				if(s.StartsWith("$")) {
					if(!firstLoop) { 
						panelDict.Add(panelName, new List<RendererInfos>(infosFinal));
						infosFinal.Clear();
						
						
					}
					panelName = s.Substring(1, s.Length - 1);
					firstLoop = false;
				}
				else {
					RendererInfos infos = new RendererInfos();
					splittedLine = s.Split(';');
					infos.atom = splittedLine[0].Trim();
					infos.residue = splittedLine[1].Trim();
					infos.chain = splittedLine[2].Trim();
					float.TryParse(splittedLine[3].Trim(), out R);
					float.TryParse(splittedLine[4].Trim(), out G);
					float.TryParse(splittedLine[5].Trim(), out B);
					float.TryParse(splittedLine[6].Trim(), out A);
					infos.color = new Color(R, G, B, A);
					infos.texture = splittedLine[7].Trim();
					if((panelName=="PaperChain")&&(!colorByResiduesDict.ContainsKey(infos.residue))){
						colorByResiduesDict[infos.residue]=infos.color;
					}else if ((panelName=="Chains") && (!ChainColorDict.ContainsKey(infos.chain))){
						ChainColorDict[infos.chain] = infos.color;
					}


					
					infosFinal.Add(infos);
				}
			}
			sr.Close();
			UIData.isRenderDictInit = true;
			Debug.Log("ColorPanelDictionary Initialized");
		}
		
		/// <summary>
		/// Inits the Textures Menu list.
		/// </summary>
		public static void InitTextureMenuList() {
			TextAsset tList = (TextAsset)Resources.Load("TexturesMenu");
			StringReader sr = new StringReader(tList.text);
			
			string s;
			string[] splittedLine;
			
			while((s=sr.ReadLine())!=null) {
				if(s.StartsWith("$"))
					textureMenuTitles.Add(s.Substring(1, s.Length - 1));
				else {
					splittedLine = s.Split(';');
					for(int i=0; i<splittedLine.Length; i++)
						splittedLine[i].Trim();
					textureMenuList.Add(splittedLine);
				}
			}
			sr.Close();
			UIData.isTexturesMenuListInit = true;
			Debug.Log("Textures Menu List Initialized");
		}
		
		/// <summary>
		/// Change the color of an atom type.
		/// </summary>
		/// <param name='atomChar'>
		/// Character of the atom. String.
		/// </param>
		/// <param name='col'>
		/// Color to apply. Color.
		/// </param>
		public static void SetAtomColor(string atomChar, Color col) {
			switch(atomChar) {
				case "H" :
					MoleculeModel.hydrogenColor.color = col;
					break;
				case "C" :
					MoleculeModel.carbonColor.color = col;
					break;
				case "N" :
					MoleculeModel.nitrogenColor.color = col;
					break;
				case "O" :
					MoleculeModel.oxygenColor.color = col;
					break;
				case "P" :
					MoleculeModel.phosphorusColor.color = col;
					break;
				case "S" :
					MoleculeModel.sulphurColor.color = col;
					break;
				case "X" :
				default :
					MoleculeModel.unknownColor.color = col;
					break;
			}
		}
		
		
		/// <summary>
		/// Change the color of all atoms according to a color panel.
		/// </summary>
		/// <param name='panelName'>
		/// Name of the color panel. String.
		/// </param>
		public static void SetColorPanel(string panelName) {
			if(panelDict.ContainsKey(panelName)) {
				Debug.Log ("Setting color panel : " + panelName);
				GenericManager manager = Molecule.View.DisplayMolecule.GetManagers()[0];
				foreach(RendererInfos panel in panelDict[panelName]){
					manager.SetColor(panel.color, ToList(panel.atom), panel.residue, panel.chain);
				}
			}
			else
				Debug.Log("!!! Invalid Panel Key : " + panelName);
		}
		
		/// <summary>
		/// Change the texture and color of all atoms according to a texture panel.
		/// </summary>
		/// <param name='panelName'>
		/// Name of the texture panel. String
		/// </param>
		public static void SetTexturePanel(string panelName) {
			if(panelDict.ContainsKey(panelName) && UIData.atomtype == UIData.AtomType.hyperball) {
				Debug.Log ("Setting texture panel : " + panelName);
				GameObject hbManagerObj = GameObject.FindGameObjectWithTag("HBallManager");
				HBallManager hbManager = hbManagerObj.GetComponent<HBallManager>();
				foreach(RendererInfos panel in panelDict[panelName])
					hbManager.SetTexture(panel.texture, ToList(panel.atom), panel.residue, panel.chain);
			}
			else
				Debug.Log("!!! Invalid Panel Key (or not hyperball) : " + panelName);
		}
		

	}

}
