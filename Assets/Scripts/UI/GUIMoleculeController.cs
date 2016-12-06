/// @file GUIMoleculeController.cs
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
/// $Id: GUIMoleculeController.cs 213 2013-04-06 21:13:42Z baaden $
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
	using System.Collections.Generic;
	using Molecule.Model;
	using Molecule.View;
	using Molecule.View.DisplayAtom;

//	using System;


	public class GUIMoleculeController {

		private static string umolversion = "v0.9.3 (R676)";
		
//		public static int texSet_max=30; /*!< Maximum number of full texture pages */
//		public static int besttexSet_min=-5; /*!< Maximum number of condensed texture pages (negative value!) */
		public static bool onlyBestTextures = true; /*!< Toggle condensed set of textures vs. full set */
		public static int texture_set=0;
	
		//bool for default visualisation with Hyperballs
		public static bool HYPERBALLSDEFAULT = true;


		// bool for showing windows
		public static bool showOpenMenu = true;
		public static bool showAtomMenu = false;
		public static bool showSecStructMenu = false;
		public static bool showSurfaceMenu = false;
		public static bool showHydroMenu = false;
		public static bool showBfactorMenu = false;
		public static bool showElectrostaticsMenu = false;
		public static bool showManipulatorMenu = false; // Careful! This boolean and all associated behavior are backwards!
		public static bool showSetAtomScales = false;
		public static string structType = "All atoms";
		public static bool showAdvMenu = false;
		public static bool showGuidedMenu = false;
		public static bool showResiduesMenu = false;
		public static bool showAtomsExtendedMenu = false;
		public static bool showChainsMenu = false;
		public static bool showPanelsMenu = false;
		public static bool showSugarChainMenu = false; // T.T Sugar menu
		public static bool showSugarRibbonsTuneMenu = false;
		public static bool showColorTuneMenu = false;
		public static bool showVRPNMenu = false;
		public static bool showMDDriverMenu = false;
			
			
		// Generic toggles
		// DISTRIBUTION
		public static bool toggle_HELP = false;
		public static bool toggle_INFOS = true;
		public static bool toggle_VE_COPYR = true;
		public static bool toggle_HB_SANIM = false;
		public static float hb_sanim = 0.7f;
		public static float hb_ssign = 1.0f;
		public static bool toggle_HB_RANIM = false;
		public static float hb_ranim = 0.4f;
		public static float hb_rsign = 1.0f;
		public static float shrink = 0.1f;
		public static float globalRadius = 1.0f;
		public static float linkScale = 1.0f;
		public static float depthfactor = -1.0f;
		public static float drag = 0.6f;
		public static float spring = 5;
		public static float bondWidth = 0.15f ; // default bond width for cube/line
		public static float highBFradius = 1.0f;
		
		
		//Electric field line symbol radius
		public  static float symbolradius = 1.0f;

		//CPK Licorice
		public	static	 bool transMETAPHOR = false;
		public	static	 float deltaShrink;
		public	static	 float deltaScale;
		public	static	 float deltaRadius;
		public	static	 float newShrink;
		public	static	 float newScale;
		public	static	 float newGlobalRadius = 1.0f;
		public	static	 float transDelta = 25.0f;
		
		public	static	 bool toggle_HB_TRANS = true;
		public 	static	 bool toggle_NA_INTERACTIVE = false;
		public  static   bool toggle_MDDRIVER = false;
		public 	static 	 bool toggle_NA_HIDE = false;
		public 	static	 bool toggle_NA_SWITCH = false;
		public 	static	 bool toggle_NA_HBALLSMOOTH = false;
		public 	static 	 bool LOD_INITIALIZED = false;
		public 	static	 bool toggle_NA_MEASURE = false;
		public 	static	 bool toggle_NA_CLICK = false;
		public  static   bool toggle_NA_MAXCAM = true;
		public  static   bool toggle_NA_AUTOMOVE = false;
		public	static	 bool toggle_NA_CAMLOCK = false;
		public  static   bool toggle_MESHCOMBINE = false;
		protected static GameObject scenecontroller = null;
		protected static Molecule3D Molecule3DComp = null;
		protected static Quaternion NA_SCCROT = new Quaternion (-0.1f, 0.1f, 0.0f, -1.0f);
		protected static Vector3 NA_SCCPOS = new Vector3 (0.4f, 1.8f, -12.0f);
		protected static bool toggle_VE_BLUR = false;
		protected static bool toggle_VE_SSAO = false;
		protected static bool toggle_VE_DOF = false;
		protected static bool toggle_VE_CREASE = false;
		protected static bool toggle_VE2_VORTX = false;
		protected static bool toggle_VE2_TWIRL = false;
		protected static bool toggle_VE2_SEPIA = false;
		protected static bool toggle_VE2_NOISE = false;
		protected static bool toggle_VE2_GRAYS = false;
		protected static bool toggle_VE2_GLOW = false;
		protected static bool toggle_VE2_EDGE = false;
		protected static bool toggle_VE2_CONTR = false;
		protected static bool toggle_VE2_CCORR = false;
		protected static bool toggle_VE2_BLUR2 = false;
		protected static bool toggle_VE2_DREAM = false;
		
		protected static bool toggle_DISTANCE_CUEING = false;
		public static bool toggle_SEC_STRUCT = false;
		public static bool toggle_RING_BLENDING=false; //T.T RingBlending visu - public for remove visu when CLEAR (reset to false)
		public static bool toggle_SUGAR_ONLY=true; 
		public static bool toggle_TWISTER=false;
		public static bool toggle_HIDE_HYDROGEN=false;
		public static bool toggle_SHOW_HB_W_SR=false;
		public static bool toggle_SHOW_HB_NOT_SUGAR=false;
		public static bool oxyToggled = false;
		public static bool toggle_OXYGEN=false;
		protected static bool distanceCueingEnabled = false;
		protected static bool cutPlaneIsDraggable = true;
		
		private GUIContent[] listatom;
		private GUIContent[] listbond;
		private GUIStyle listStyle;
	
		// Bottom left UnityMol icon and link to helptext (currently local file, to be updated).
		private static Texture2D guicon = Resources.Load ("Artwork/guicon") as Texture2D;
//		private static string umolbase = "file:///opt/Unity3D/UnityMol";
//		private static string umolbase = "http://www.shaman.ibpc.fr";
		private static string umolbase = "http://www.baaden.ibpc.fr/umol";

		// Ramps for grayscale effect
		protected static string[] ve2_grays_ramps = {"grayscale ramp", "grayscale ramp inverse"};
		protected static int ve2_grays_rampn = 1;
		protected static int ve2_grays_rampc = 1;

		// Ramps for color correction effect
		protected static string[] ve2_ccorr_ramps = {"oceangradient", "nightgradient"};
		protected static int ve2_ccorr_rampn = 1;
		protected static int ve2_ccorr_rampc = 0;

		// bool for showing windows
		public static bool showAtomType = false;
		public static bool showBondType = false;
		public static bool showEffectType = false;
		public static bool showGrayColor = false;
		public static bool showHyperballsMenu = false;
		public static bool showCubeLineBondMenu = false ;
//		public bool surfacecolor_show= false;
		public static bool showSurfaceButton = false;
		public static bool showBackgroundType = false;
		public static bool showMetaphorType = false;
//		public bool ParamshowFieldLine=false;
		public static bool showFieldLines = false;
		public static bool showSurface = false;
		public static bool fieldLineColorGradient = true;
		public static bool showVolumetricDensity = false;
		public static bool showVolumetricFields = false; // volumetric electrostatic fields
		public static bool showVolumetricDepth = false;
		public static bool electroIsoSurfaceTransparency = false;
	
		
		// variable for the surface and field lines cutting
		public static float depthCut = 40f;
		public static bool surfaceMobileCut = false;
		public static bool surfaceStaticCut = false;
		public static bool showSurfaceCut = false;
		public static bool showSurfaceMobileCut 	= false;
		public static float depthCutMin = 0;
		public static float depthCutMax = 0;

		public static bool surfaceTexture = false;
		public static bool externalSurfaceTexture = false;
		public static bool showSurfaceTexture = false;
		public static string surfaceTextureName;
		public static bool surfaceTextureDone = false;
		public static float cutX = 1f;
		public static float cutY = 0f;
		public static float cutZ = 0f;
		public static float adjustFieldLineCut = 40f;
		public static float generateThreshold = 0.5f;
		public static float generateThresholdDx_pos = 0f;
		public static float generateThresholdDx_neg = 0f;
		public static bool modif = false;
		public static bool pdbGen = false; // bool of density grid. true when density was calculated
		public static bool dxRead = false; // true when dx read
		public static bool buildSurface = false;
		public static bool buildSurfaceDone = false;
		public static bool FileBrowser_show = false;
		public static bool FileBrowser_show2 = false;
		public static ImprovedFileBrowser m_fileBrowser;
		public static string m_textPath;
		public static string m_last_extSurf_Path = null;
		public static Texture2D extSurf;
		
		protected static bool showAtoms = true ; // whether atoms are displayed
		protected static float prevRadius; // previous radius, needed to save the radius when hiding atoms
		protected static DepthCueing depthCueing;
		protected static AmbientOcclusion ambientOcclusion;
		
		private static GUIContent emptyContent = new GUIContent() ; // We create our own titles, we don't want to use Unity's, because they force window top padding.
		
		
		// MB for centered text		
		protected static GUIStyle CentredText {
			get {
				if (m_centredText == null) {
					m_centredText = new GUIStyle (GUI.skin.label);
					m_centredText.alignment = TextAnchor.MiddleCenter;
				}
				return m_centredText;
			}
		}

		protected static GUIStyle m_centredText;

//		pos = mul (UNITY_MATRIX_MVP, float4(IN.worldPos,0f));
//		clip (frac(-(-5+pos.z)/500) - 0.2);
		
		public static ReadDX readdx;// = new ReadDX ();
		
		// 
		public Texture2D aTexture;
		protected static float colorRed = 0.0f;
		protected static float colorGreen = 0.0f;
		protected static float colorBlue = 0.0f;    

		
		// Field line paramter: 
		public static float speed = 0.13333333f;
		public static float density = 3.4f;
		public static float linewidth = 0.2f;
//		public static float intensity = 0.1f;
		public static float linelength = 0.7f;
		public static ColorObject EnergyGrayColor = new ColorObject(Color.white);
		public static ColorObject SurfaceGrayColor = new ColorObject(Color.white); // color of surface
		public static ColorObject SurfaceInsideColor = new ColorObject(Color.gray); // color of the inside of the surface	
		public static ColorObject BackgroundColor = new ColorObject(Color.black);
		public static ColorObject BondColor  = new ColorObject(Color.black);
		public static ColorObject BondColorcheck  = new ColorObject(Color.black);
		public static ColorObject RingColor  = new ColorObject(Color.black);
		public static ColorObject OxySphereColor  = new ColorObject(Color.red);
		public static ColorObject OxySphereColorCheck  = new ColorObject(Color.red);
		public static ColorObject RingColorcheck  = new ColorObject(Color.black);
		public static ColorPicker m_colorPicker = null;

		
		public static Material electricsymbol = (Material)Resources.Load ("Materials/electricparticle");
		private bool firstpass = true;

		//Electrostatic iso-surface parameters
		public static bool showElectroIsoPositive = false;
		public static bool showElectroIsoNegative = false;
		public static bool electroIsoPositiveInitialized = false;
		public static bool electroIsoNegativeInitialized = false;



		public static void CreateColorPicker(ColorObject col, string title, List<string> atomTarget, string residueTarget = "All", string chainTarget = "All")
		{
			if(m_colorPicker != null)
				m_colorPicker = null;
			m_colorPicker = new ColorPicker(Rectangles.colorPickerRect, col, atomTarget, residueTarget, chainTarget, title);
		}
		
		public GUIMoleculeController ()
		{
			#if !UNITY_WEBPLAYER
				m_last_extSurf_Path = System.IO.Directory.GetCurrentDirectory();
			#endif


			scenecontroller = GameObject.Find ("LoadBox");
			Molecule3DComp = scenecontroller.GetComponent<Molecule3D> ();
/*
			listatom = new GUIContent[11];
		    listatom[0] = new GUIContent("Cube");
		    listatom[1] = new GUIContent("Sphere");
		    listatom[2] = new GUIContent("HyperBall");
		    listatom[3] = new GUIContent("Raycasting");
		    listatom[4] = new GUIContent("Common Billboard");
		    listatom[5] = new GUIContent("RayCasting Billboard");
		    listatom[6] = new GUIContent("HyperBall Billboard");
		    listatom[7] = new GUIContent("RayCasting Sprite");
		    listatom[8] = new GUIContent("Multi-Hyperball");
		    listatom[9] = new GUIContent("CombineMesh HyperBall");		    
		    listatom[10] = new GUIContent("ParticleBall");		    

			listbond = new GUIContent[7];
		    listbond[0] = new GUIContent("Cube");
		    listbond[1] = new GUIContent("Line");
		    listbond[2] = new GUIContent("HyperStick");
		    listbond[3] = new GUIContent("Tube Stick");
		    listbond[4] = new GUIContent("Billboard HyperStick");
		    listbond[5] = new GUIContent("Particle Stick");
		    listbond[6] = new GUIContent("No Stick");
*/
		    
			//Get the ReadDX component
			readdx = scenecontroller.GetComponent<ReadDX>();
//			Debug.Log("READDX GUIMolecule: " + readdx);

			// Make a GUIStyle that has a solid white hover/onHover background to indicate highlighted items
			listStyle = new GUIStyle ();
			listStyle.normal.textColor = Color.white;
			Texture2D tex = new Texture2D (2, 2);
			Color[] colors = new Color[4];
			for (int i=0; i<4; i++) {
				colors [i] = Color.white;
			}

			tex.SetPixels (colors);
			tex.Apply ();
			listStyle.hover.background = tex;
			listStyle.onHover.background = tex;
			listStyle.padding.left = listStyle.padding.right = listStyle.padding.top = listStyle.padding.bottom = 4;
		    
			aTexture = (Texture2D)Resources.Load ("EnergyGrayColorAlpha");
		}

		public static void InitMoleculeParameters()
		{
			depthCutMin = -5 + Mathf.Min(new float[]{MoleculeModel.MinValue.x,
												MoleculeModel.MinValue.y,
												MoleculeModel.MinValue.z});
			depthCutMax = 5 + Mathf.Max(new float[]{MoleculeModel.MaxValue.x,
												MoleculeModel.MaxValue.y,
												MoleculeModel.MaxValue.z});
			depthCut = depthCutMax;
		}
		
		
		/// <summary>
		/// Prevents the camera from moving along with the mouse if the cursor is over any active menu or button.
		/// Currently a bit dirty, since existing rectangles are recreated for the pruposes of this function.
		/// </summary>
		public void CameraStop ()
		{
/*
			// rectangle were the mouse isn't active 
			Rect guirect = Rectangles.mainRect;
			Rect guirect2 = Rectangles.atomMenuRect;
			Rect guirect3 = Rectangles.atomStyleRect;
			Rect guirect4 = Rectangles.bondTypeRect;
			Rect guirect5 = Rectangles.effectTypeRect;
			Rect guirect6 = Rectangles.backTypeRect;
			Rect guirect8 = Rectangles.metaphorRect;
			Rect guirect10 = Rectangles.surfaceMenuRect;
			Rect guirectHyperballs = Rectangles.hyperballRect;

			// right window
			Rect guirectD11 = Rectangles.atomScalesRect;
			Rect guirectD12 = new Rect (Screen.width - 295, 0, 290, 65);
			
			Rect rect_paramsurface = Rectangles.surfaceParametersRect;
			Rect guirectFileBrow = new Rect (400, 100, 600, 500);				
			Rect Manipulator = Rectangles.manipulatorRect ;	

			// Rect colorpickerAtom = new Rect (Screen.width - 238, 350, 238, 308);
*/	
			
			Rect screen = new Rect (0, 0, Screen.width, Screen.height);
			
			Vector3 mousePos = Input.mousePosition;
			mousePos.y = Screen.height - mousePos.y;
			
			// [Erwan - 28/08/13] I didn't delete this since HotControl isn't as good as checking rectangle by rectangle,
			// still, it's probably lighter than recreating all those rectangles and making all those tests
/*			if (!screen.Contains (mousePos)
				|| (Rectangles.helpRect.Contains(mousePos))
				|| (Rectangles.exitRect.Contains(mousePos))
				|| (guirectFileBrow.Contains (mousePos) && FileBrowser_show)
				|| (guirectFileBrow.Contains (mousePos) && FileBrowser_show2)
				|| (Manipulator.Contains (mousePos) && showManipulatorMenu)
				|| (Rectangles.manipulatorMoveRect.Contains(mousePos) && !showManipulatorMenu)
				|| guirect.Contains (mousePos) 
				|| (showAtomMenu && guirect2.Contains (mousePos))
				|| (showAtomType && guirect3.Contains (mousePos))
				|| (showBondType && guirect4.Contains (mousePos))
				|| (showEffectType && guirect5.Contains (mousePos))
				|| (showBackgroundType && guirect6.Contains (mousePos))
				|| (m_colorPicker != null && m_colorPicker.enabled && Rectangles.colorPickerRect.Contains (mousePos))
				|| (showMetaphorType && guirect8.Contains (mousePos))
				|| (showElectrostaticsMenu && Rectangles.electroMenuRect.Contains (mousePos))
				|| (showElectrostaticsMenu && showFieldLines && Rectangles.fieldLinesRect.Contains(mousePos))
				|| (showSurfaceMenu && guirect10.Contains (mousePos))
				|| (showBfactorMenu && guirect10.Contains (mousePos))
				|| (showSurfaceMenu && showSurfaceCut && Rectangles.movePlaneRect.Contains (mousePos))
				|| (showSurfaceMenu && showSurfaceCut && Rectangles.surfaceCutRect.Contains (mousePos))
				|| (showSurfaceMenu && showSurfaceMobileCut && Rectangles.surfaceMobileCutRect.Contains(mousePos))
				|| (showSurfaceTexture && Rectangles.textureRect.Contains (mousePos))
				|| showSetAtomScales && !UIData.hiddenUIbutFPS && guirectD11.Contains (mousePos)
				|| GUIDisplay.m_max && UIData.hiddenUIbutFPS && guirectD12.Contains (mousePos) 
				|| GUIDisplay.m_texture && Rectangles.textureRect.Contains (mousePos)
				|| (rect_paramsurface.Contains (mousePos) && GameObject.FindGameObjectWithTag ("SurfaceManager") && (showSurfaceMenu || showBfactorMenu || showElectrostaticsMenu))
				|| (guirectHyperballs.Contains (mousePos) && showHyperballsMenu)
				|| (showCubeLineBondMenu && Rectangles.cubeLineBondRect.Contains(mousePos))
				|| (showAdvMenu && Rectangles.advOptionsRect.Contains(mousePos))
				|| (MouseOverMolecule.stopCamera)
				) 
*/
			// Check where mouse has to be active
			// I think GUIUtility.hotControl is enough to check wether the mouse is on a GUI component or on the molecule. [Erwan]
			// 		After some tries, it didn't work with some rectangles like ColorPicker or CutPlane...
			// 		The mousewheel is still enabled on normal GUIs (not on colorpicker, cutplane...) -> Hotcontrol only watch the mouse click.
			//		Also, small problem when low FPS (I think ?) causing HotControl to activate with a small delay : when you click on menus, the molecule will often move a little before HotControl freeze camera
			if(
				!screen.Contains (mousePos) // Avoid scrolling outside the screen
				|| GUIUtility.hotControl != 0 // Mouse on a GUI
				|| (m_colorPicker != null && m_colorPicker.enabled && Rectangles.colorPickerRect.Contains (mousePos)) // Special GUIs
				|| (showSurfaceMenu && showSurfaceCut && Rectangles.movePlaneRect.Contains (mousePos))
				|| (showBackgroundType && Rectangles.backTypeRect.Contains(mousePos))
//				|| (FileBrowser_show2 && Rectangles.fileBrowserRect.Contains(mousePos)) // Can't manage to disable scrolling on filebrowser ??
				|| (MouseOverMolecule.stopCamera) // Interactive mode
				|| toggle_NA_CAMLOCK
			  )
			{
				maxCamera.cameraStop = true;  // stop macCamera
				UIData.cameraStop2 = true;
//				Debug.Log("maxCamera.cameraStop = true");
			} else {
				if (UIData.cameraStop) {
					maxCamera.cameraStop = true;
					UIData.cameraStop2 = true;
				} else {
					maxCamera.cameraStop = false;
					UIData.cameraStop2 = false;
				}
			}
			
		}
		
		public void DisplayGUI () {
			int padding = GUI.skin.window.padding.top ;
			GUI.skin.window.padding.top = 5;
			GUI.Window (1, Rectangles.mainRect, LoadTypeGUI.MainFun, "");
			GUI.skin.window.padding.top = padding;
//			GUI.Window (1, Rectangles.mainRect, loadGUI, "");			
			if (firstpass) {
				firstpass = false;
			//	Camera.mainCamera.backgroundColor = new Color (0.0f, 0.0f, 0.0f);
				//Camera.main.backgroundColor = new Color (0.0f, 0.0f, 0.0f);

			}
			
			//Display color pickers
			if (m_colorPicker != null) {
				m_colorPicker.OnGUI ();
			}
			//Update background color
			colorRed = BackgroundColor.color.r;
			colorGreen = BackgroundColor.color.g;
			colorBlue = BackgroundColor.color.b;
			//TODO: colorpick Atom
			//TODO: colorpick Background
		}



		private static void VisualControl (){
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Visual", GUILayout.MaxWidth (50));
			
			UIData.toggleMouse = GUILayout.Toggle (UIData.toggleMouse, "Mouse");
			UIData.toggleKey = !UIData.toggleMouse;
			
			UIData.toggleKey = GUILayout.Toggle (UIData.toggleKey, "Key");
			UIData.toggleMouse = !UIData.toggleKey;
			GUILayout.EndHorizontal ();
		}
			
		public static float LabelSlider (float sliderValue, float sliderMinValue, float sliderMaxValue, string labelText, 
			string toolTip, bool enable, int sliderwidth, int labelwidth=100, bool newLine = false) {
			GUI.enabled = enable;
			
			GUILayout.BeginHorizontal();
			GUILayout.Label (new GUIContent (labelText, toolTip), GUILayout.MinWidth (labelwidth));
			if(newLine) {
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
			}

			sliderValue = GUILayout.HorizontalSlider (sliderValue, sliderMinValue, sliderMaxValue, GUILayout.Width (sliderwidth));
			
			GUILayout.EndHorizontal();

			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;

			return sliderValue;
		}
		
		
		
// All the functions that open a new window in the GUI.  =============================================================================================
		
		public void SetAdvMenu () {
			if(showAdvMenu)		
				Rectangles.advOptionsRect = GUILayout.Window(142, Rectangles.advOptionsRect, LoadTypeGUI.AdvOptions, emptyContent);
		}

		public void SetGuidedMenu () {
			if(showGuidedMenu)		
				Rectangles.GuidedNavRect = GUILayout.Window(143, Rectangles.GuidedNavRect, LoadTypeGUI.GuidedOptions, emptyContent);
		}
		
		public void SetAtomMenu () {
			if (showAtomMenu)
			    Rectangles.atomMenuRect = GUI.Window (10, Rectangles.atomMenuRect, LoadTypeGUI.AtomMenu, emptyContent);
		}
		
		public void SetSecStructMenu () {
			if(showSecStructMenu)
				Rectangles.secStructMenuRect = GUI.Window(587, Rectangles.secStructMenuRect, LoadTypeGUI.SecStructMenu, emptyContent);			
		}
		
		//public static void SetAtomScales() {
		//	Rectangles.atomScalesRect = GUI.Window( 40, Rectangles.atomScalesRect, GUIDisplay.AtomScales, emptyContent);
		//}
		
		public static void SetPanels () {
			Rectangles.panelsMenuRect = GUILayout.Window (44, Rectangles.panelsMenuRect, GUIDisplay.PanelsMenu, emptyContent);
		}
		
		public static void SetAtomsExtended () {
			//Rectangles.atomsExtendedMenuRect = GUI.Window (45, Rectangles.atomsExtendedMenuRect, GUIDisplay.AtomsExtendedMenu, emptyContent);
			Rectangles.atomsExtendedMenuRect = GUILayout.Window(45, Rectangles.atomsExtendedMenuRect, GUIDisplay.AtomsExtendedMenu, emptyContent);
		}
		
		public static void SetResidues () {
			Rectangles.residuesMenuRect = GUILayout.Window (46, Rectangles.residuesMenuRect, GUIDisplay.ResiduesMenu, emptyContent);
		}
		
		public static void SetChains () {
			Rectangles.chainsMenuRect = GUILayout.Window (47, Rectangles.chainsMenuRect, GUIDisplay.ChainsMenu, emptyContent);
		}
		
		public void SetAtomType () {
			if (showAtomType && showAtomMenu) {
//				GUIContent c = new GUIContent ("Set Atom Type", "Set the atom type");
				Rectangles.atomStyleRect = GUI.Window (11, Rectangles.atomStyleRect, LoadTypeGUI.AtomStyle, emptyContent);
//				Debug.Log(Screen.width-195);
			}
		}
		
		public void SetBfactorMenu () {
			if (showBfactorMenu) {
//				GUIContent c=new GUIContent("Set Atom Type","Set the atom type");
				Rectangles.surfaceMenuRect = GUI.Window (20, Rectangles.surfaceMenuRect, LoadTypeGUI.Surface, emptyContent);
//				Debug.Log(Screen.width-195);
				if (GameObject.FindGameObjectWithTag ("SurfaceManager"))
					Rectangles.surfaceParametersRect = GUI.Window (25, Rectangles.surfaceParametersRect, LoadTypeGUI.SurfaceParams, emptyContent);	
			}
		}
		
		
		public void SetBondType () {
			if (showBondType && showAtomMenu)
				Rectangles.bondTypeRect = GUI.Window (11, Rectangles.bondTypeRect, LoadTypeGUI.Bond, emptyContent);
		}
		
		// T.T Sugar Menu
		public void setSugarMenu(){
			if (showSugarChainMenu)
				Rectangles.SugarMenuRect = GUI.Window(454654, Rectangles.SugarMenuRect, LoadTypeGUI.SugarM, emptyContent);
		}


		public void setSugarRibbonsTuneMenu(){
			if (showSugarRibbonsTuneMenu)
				Rectangles.SugarRibbonsTuneRect = GUI.Window(4554, Rectangles.SugarRibbonsTuneRect, LoadTypeGUI.SugarRibbonsTune, emptyContent);
		}


		public void setColorTuneMenu(){
			if (showColorTuneMenu)
				Rectangles.ColorTuneRect = GUI.Window(45154, Rectangles.ColorTuneRect, LoadTypeGUI.ColorTuneMenu, emptyContent);
		}
		public void SetCubeLineBond() {
			if	(((UIData.bondtype == UIData.BondType.cube) || (UIData.bondtype == UIData.BondType.line))
					&& !showBondType && !showAtomType && showAtomMenu) {
				Rectangles.cubeLineBondRect = GUI.Window (13, Rectangles.cubeLineBondRect, LoadTypeGUI.CubeLineBond, emptyContent) ;
				showCubeLineBondMenu = true ;
			} else
				showCubeLineBondMenu = false ;
		}
		
		
		public void SetEffectType () {
			if (showEffectType)
				Rectangles.effectTypeRect = GUI.Window (125, Rectangles.effectTypeRect, LoadTypeGUI.Effects, emptyContent);
		}
		
		public void SetFieldMenu () {
			if (showElectrostaticsMenu)
				Rectangles.electroMenuRect = GUI.Window (30, Rectangles.electroMenuRect, LoadTypeGUI.Electrostatics, emptyContent);

			if (showFieldLines && showElectrostaticsMenu)
				Rectangles.fieldLinesRect = GUI.Window (31, Rectangles.fieldLinesRect, LoadTypeGUI.FieldLines, emptyContent);
		}

		public void SetSurfaceMenu () {
			if (showSurfaceMenu) { 
//			Debug.Log("test 42000");
//			GUIContent c=new GUIContent("Set Surface Params");
				
			    Rectangles.surfaceMenuRect = GUI.Window (20, Rectangles.surfaceMenuRect, LoadTypeGUI.Surface, emptyContent);
//			Debug.Log(Screen.width-195);
				
				if (GameObject.FindGameObjectWithTag ("SurfaceManager")) {
					Rectangles.surfaceParametersRect = GUI.Window (25, Rectangles.surfaceParametersRect, LoadTypeGUI.SurfaceParams, emptyContent);	
				}
			}
		}

		public void SetHydroMenu(){
			if (showHydroMenu && showSurfaceMenu )
				Rectangles.hydroMenuRect = GUI.Window (2001, Rectangles.hydroMenuRect, LoadTypeGUI.HydroMenu, emptyContent);
		}
		
		
		public void SetManipulatorMenu () {
			if (showManipulatorMenu)
				Rectangles.manipulatorRect = GUI.Window (42, Rectangles.manipulatorRect, LoadTypeGUI.Display, emptyContent);
		}
		
		public void SetMnipulatormove () {
			if (!showManipulatorMenu)
				Rectangles.manipulatorMoveRect = GUI.Window (42, Rectangles.manipulatorMoveRect, LoadTypeGUI.Manipulator, emptyContent);
		}
		
		public void SetHyperBall ()	{
			if ((/*UIData.atomtype == UIData.AtomType.hyperball || */UIData.bondtype == UIData.BondType.hyperstick) && (!showBondType && !showAtomType) && showAtomMenu) {
				Rectangles.hyperballRect = GUI.Window (12, Rectangles.hyperballRect, LoadTypeGUI.HyperballStyle, emptyContent);
				showHyperballsMenu = true;
			} else
				showHyperballsMenu = false;
		}
		
		public void SetSurfaceTexture () {
			if (showSurfaceTexture && (showSurfaceMenu || showBfactorMenu)) {	
				GUIDisplay.m_texture = false;		
//				GUI.Window(41, new Rect(texturetypexstart,texturetypeystart,texturewidth,textureheight),loadSurfaceTexture, "Surface texture parameters");
				Rectangles.textureRect = GUI.Window (41, Rectangles.textureRect, LoadTypeGUI.SurfaceTexture, emptyContent);
			}
		}

		public void SetSurtfaceMobileCut () {
			if (showSurfaceMobileCut && (showSurfaceMenu || showBfactorMenu))
			    Rectangles.surfaceMobileCutRect = GUI.Window (21, Rectangles.surfaceMobileCutRect, LoadTypeGUI.SurfaceMobileCut, emptyContent);
		}
		
		public void SetSurfaceCut () {
			if (showSurfaceCut && (showSurfaceMenu || showBfactorMenu)) {
				Rectangles.surfaceCutRect = GUI.Window (21, Rectangles.surfaceCutRect, LoadTypeGUI.SurfaceCut, emptyContent);
				Rectangles.movePlaneRect = GUI.Window (22, Rectangles.movePlaneRect, LoadTypeGUI.MoveCutPlane, emptyContent);
			}
		}
	
		public void SetBackGroundType () {
			if (showBackgroundType) 
				Rectangles.backTypeRect = GUI.Window (43, Rectangles.backTypeRect, LoadTypeGUI.Background, emptyContent);
		}
		
		public void SetMetaphorType () {
			if (showMetaphorType && showAtomMenu)
				Rectangles.metaphorRect = GUI.Window (23, Rectangles.metaphorRect, LoadTypeGUI.Metaphor, emptyContent);
		}
		
		public void SetVRPNMenu () {
			if (showVRPNMenu)
				Rectangles.vrpnMenuRect = GUI.Window (419, Rectangles.vrpnMenuRect, VRPNManager.VRPNM, emptyContent);
		}
		
		public void SetMDDriverMenu () {
			if (showMDDriverMenu)
				Rectangles.mddriverMenuRect = GUI.Window (420, Rectangles.mddriverMenuRect, MDDriver.MDDriverM, emptyContent);
		}
			
		
// end of windows functions =============================================================================================		

		public  void MinLoadTypeGUIAtom (int b) {}

		protected static string queryBestTextures(){
			if (onlyBestTextures)
				return("On");
			else
				return("Off");
		}
		
		public static void FileSelectedCallback (string path) {
			#if !UNITY_WEBPLAYER
				m_fileBrowser = null;
				if (path != null) {
					m_textPath = path;
					m_last_extSurf_Path = System.IO.Path.GetDirectoryName (path);
					externalSurfaceTexture = true;
					surfaceTextureDone = false;
				}
//				FileBrowser_show = false;
				surfaceTexture = false;
				GUIMoleculeController.FileBrowser_show2 = false;
				WWW www = new WWW ("file://" + GUIMoleculeController.m_textPath);
				extSurf = www.texture;
				Debug.Log (m_textPath);
			#endif
		}
		



		
// help window at the start of program =============================================================================================		
		public void RenderHelp () {
			if (toggle_HELP) {
				if (!toggle_VE_COPYR)
					toggle_VE_COPYR = true;

				GUI.Box (new Rect (Screen.width / 4, Screen.height / 4 + 30, Screen.width / 2, Screen.height / 2 - 60), "UNITY MOL QUICK HELP");
				GUILayout.BeginArea (new Rect (5 + Screen.width / 4, 25 + Screen.height / 4 + 30, 0.5f * Screen.width, 0.5f * Screen.height - 25));
				GUILayout.Label ("Welcome to UnityMol. Most program functions will show a tooltip in the bottom left corner to provide some assistance.\n");
				GUILayout.Label ("The main menu functions are accessible from the buttons on the top left part of the screen." +
							"GUI elements will be arranged along the screen borders in order to leave space in the middle for the molecule view." +
							"The same button that opens a menu can usually also be re-used to close it again.\n");
				GUILayout.Label ("From the Open menu you can import PDB files, obj meshes or networks in cytoscape format." +
							"Then modify the visual aspect with the options in the Atoms menu. Generate (or read) surfaces from the Surface menu." +
							"The Electrostatics menu provides functionality to control the aspect of animated field lines." +
							"Some general functionality is assembled in the Display menu.\n");
				GUILayout.Label ("UnityMol's functionality is described in detail in the corresponding publication (PLoS One 2013, 8(3):e57990)." +
							"A tutorial image summarizing essential functions is also provided." +
							"In the future, clicking on the bottom left UnityMol icon will open the online help (keep posted)." +
							"Further information can be obtained from the UnityMol SourceForge website and mailing list.\n");
				GUILayout.Label ("Keyboard Controls :\n"+
					"Move the molecule with the arrow Keys or Q/E/Z/X. Rotate it with W/A/S/D. Modify zoom with B/N.\n"+
					"Focus an atom by selecting it and pressing T, reset camera and focus with R. Hide GUI with Backspace.\n");
				// MB: these keys don't seem to work anymore?? check:
//				GUILayout.Label("Use the cursor keys to navigate the scene: W/X for rotation D/E/Q/S/Z for pan,
//					and B/N for zoom if click the key choice. You can also use the Mouse button to navigate if click the mouse choice.
//					Alternatively,if you have a joypad you can rotation and zoom and pan by the button of the joypad, and also the radius of the atoms");
//				GUILayout.Label ("You can press the delete key to activate some hidden features on the left-hand atoms menu (bugs included!).");
				// MB: the equal key does not seem to work?		
//				GUILayout.Label("If you press the equal key, the octree dividing blocks for the particle system are shown.");
				GUILayout.Label ("\nUnity Mol is an open source project by Marc Baaden and the LBT team. version "+umolversion+" (c) 2009-13.");
				GUILayout.EndArea ();
//				Debug.Log("Help information window");
				if (GUI.Button (new Rect (Screen.width / 4, 0.75f * Screen.height - 20 - 30, Screen.width / 2, 20), "Close unity mol help window"))
					toggle_HELP = false; 
			}
			
			// Print FPS and atom/bond count if activated ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			if(toggle_INFOS) {
				GUI.Label(Rectangles.fpsInfosRect, new GUIContent(" FPS : " + MoleculeModel.FPS 
																+ "\n Atom count : " + MoleculeModel.atomsnumber.ToString()
																+ "\n Bond count : " + MoleculeModel.bondsnumber.ToString()));
			}
			
			// Print copyright and icon if activated :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			if (toggle_VE_COPYR) {
//				GUI.Box (new Rect (Screen.width - 700, Screen.height - 25, 450, 25), "(c)opyright 2009-13 by LBT team/M. Baaden, <baaden@smplinux.de>");
				GUI.Box (new Rect (Rectangles.webHelpWidth, Screen.height - 25, Screen.width, 25), umolversion+" (c)opyright 2009-14 by LBT team/M. Baaden, <baaden@smplinux.de>; SweetUnityMol by Tubiana, Imberty, Perez & MB");
				// Real logo size: 87 x 263 // 175 x 58
//				if (GUI.Button ( new Rect(0,Screen.height-58, 175, 58), new GUIContent(guicon,"Open the Unity Mol manual in a web browser"), GUI.skin.GetStyle("label"))) 
				if (GUI.Button (Rectangles.webHelpRect, new GUIContent (guicon, "Open the Unity Mol manual in a web browser"))) {
//					Application.OpenURL (umolbase+"/Assets/_Documentation/Manual.html");
//					Application.OpenURL (umolbase + "/umolfigs2.png");
					Application.OpenURL (umolbase + "/page2/index.html");
				}
			}
			#if !UNITY_WEBPLAYER 
			{
//				if (GUI.Button (new Rect (principalxstart + principalwidth + 25, principalystart, 40, 20), new GUIContent ("EXIT", "Exit the Unity Mol program"))) {
				if (GUI.Button (Rectangles.exitRect, new GUIContent ("EXIT", "Exit the Unity Mol program")))
					Application.Quit ();
			}
			#endif
			if (GUI.Button (Rectangles.helpRect, new GUIContent ("?", "Open quick help for the Unity Mol program"))) {
				toggle_HELP = !toggle_HELP;
			}
		} // End of RenderHelp
	}
}
