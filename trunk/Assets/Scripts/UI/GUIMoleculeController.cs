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

namespace UI
{
	using UnityEngine;
	using System.Collections;
	using Molecule.Model;
	using Molecule.View;
	using Molecule.View.DisplayAtom;

//	using System;


	public class GUIMoleculeController
	{
                private static string umolversion = "v0.9.1 (R251)";
		//TODO: GET RID OF STATIC VARIABLES !!!!!!!!!!!!!!!!! (Singleton ?)

		//private bool toggleMouse = false;
		//private bool toggleKey = false;
		
		public static int principalxstart = 5;
		public static int principalystart = 5;
		public static int principalwidth = 455; //535 with bfactor
		public static int principalheight = 25;
		
		// Disposition of windows
		public	static int menuxstart = 5; //5
		public	static int menuystart = 10; //10
		public	static int menuwidth = 180; // 768
		public	static int menuheight = 500; //30
		
		public	static int menuAtomxstart = 5;
		public	static int menuAtomystart = principalystart + principalheight;
		public	static int menuAtomwidth = 180;
		public	static int menuAtomheight = 205;
		public	static int menuSurfacexstart = menuAtomxstart + menuAtomwidth + 5 ;
		public	static int menuSurfaceystart = principalystart + principalheight;
		public	static int menuSurfacewidth = 175;
		public	static int menuSurfaceheight = 100;
		public	static int menuFieldxstart = menuSurfacexstart + menuSurfacewidth + 5 ;
		public	static int menuFieldystart = principalystart + principalheight;
		public	static int menuFieldwidth = 200;
		public	static int menuFieldheight = 300;
		public 	static Rect MenuElectro_Rect = new Rect (menuFieldxstart, 
														 menuFieldystart, 
														 menuFieldwidth, 
														 120);
		public 	static Rect FieldLineRect = new Rect (menuFieldxstart, 
													  menuFieldystart + 120, 
													  menuFieldwidth, 
													  menuFieldheight - 60);

		public static int hyperballtypexstart = 5;
		public static int hyperballtypeystart = menuAtomystart + menuAtomheight;
		public static int hyperballtypewidth = 180;
		public static int hyperballtypeheight = 360;
		public static int atomtypexstart = 5;
		public static int atomtypeystart = menuAtomystart + menuAtomheight;
		public static int atomtypewidth = 180;
		public static int atomtypeheight = 180;
		public static int bondtypexstart = 5;
		public static int bondtypeystart = menuAtomystart + menuAtomheight;
		public static int bondtypewidth = 180;
		public static int bondtypeheight = 180;
		public static int graycolortypexstart = Screen.width - 240;
		public static int graycolortypeystart = 5;
		public static int graycolortypewidth = 238;
		public static int graycolortypeheight = 308;
		public static int texturetypexstart = principalxstart + principalwidth + 70;
//		public static int texturetypeystart = graycolortypeystart+graycolortypeheight+20;
		public static int texturetypeystart = principalystart;
		public static int texturewidth = 290;
		public static int textureheight = 208;
		public static Rect texturetype_rect = new Rect (texturetypexstart, texturetypeystart, texturewidth, textureheight);

		public static int texSet_max=30; /*!< Maximum number of full texture pages */
		public static int besttexSet_min=-5; /*!< Maximum number of condensed texture pages (negative value!) */
		//public bool only_best_textures = false; /*!< Toggle condensed set of textures vs. full set */
		public bool only_best_textures = true;
		public static int texture_set=1;

		public static int menuparamsurfacexstart = menuSurfacexstart;
		public static int menuparamsurfaceystart = menuSurfaceystart + 100;
		public static int menuparamsurfacewidth = menuSurfacewidth;
		public static int menuparamsurfaceheight = menuSurfaceheight + 50;
		public static int surfacecuttypexstart = menuAtomxstart + menuAtomwidth + 5;
		public static int surfacecuttypeystart = menuSurfaceystart + menuSurfaceheight + 150;
		public static int surfacecuttypewidth = 175;
		public static int surfacecuttypeheight = 200;
		public static int movesurfacecuttypexstart = surfacecuttypexstart;
		public static int movesurfacecuttypeystart = surfacecuttypeystart + surfacecuttypeheight;
		public static int movesurfacecuttypewidth = 175;
		public static int movesurfacecuttypeheight = 175;
		public static int metaphortypexstart = menuAtomxstart + menuAtomwidth;
		public static int metaphortypeystart = surfacecuttypeystart + surfacecuttypeheight + 5;
		public static int metaphortypewidth = 180;
		public static int metaphortypeheight = 140;
				
		// a revoir
		public static int manipulatorwidth = 230;
		public static int manipulatorheight = 175;
		public static int manipulatorxstart = Screen.width - 240;
		public static int manipulatorystart = Screen.height - manipulatorheight - 20;
		public static Rect rect_manipulator = new Rect(manipulatorxstart,manipulatorystart,manipulatorwidth,manipulatorheight);
		private static int manipulatormoveheight = manipulatorheight-50;
		private static int manipulatormoveystart = Screen.height - manipulatormoveheight - 20;
		public static Rect rect_manipulatormove = new Rect(manipulatorxstart,manipulatormoveystart,manipulatorwidth,manipulatormoveheight);
		public static Rect colorPicker_rect = new Rect(manipulatorxstart - graycolortypewidth - 5, 
												Screen.height - graycolortypeheight - 20,
												graycolortypewidth, 
												graycolortypeheight);
		public static int effecttypewidth = 180;
		public static int effecttypeheight = 245;
		public static int effecttypexstart = (int)colorPicker_rect.x - effecttypewidth - 5;
		public static int effecttypeystart = Screen.height - effecttypeheight -20;
		public static int backtypewidth = 110;
		public static int backtypeheight = 125;
		public static int backtypexstart = manipulatorxstart + 60;
		public static int backtypeystart = manipulatorystart - backtypeheight - 5;

		// bool for showing window

		public static bool menuOpen_show = true;
		public static bool menuAtom_show = false;
		public bool menuSurface_show = false;
		public bool menuBfactor_show = false;
		public bool menuField_show = false;
		public bool menuManipulator_show = false;
		public static bool SetAtomScale_show = false;
		string structtype = "All atoms";

		// Generic toggles
		// DISTRIBUTION
		public static bool toggle_HELP = false;
		public static bool toggle_VE_COPYR = true;
		public	static bool toggle_HB_SANIM = false;
		public	static	 float hb_sanim = 0.7f;
		public	static	 float hb_ssign = 1.0f;
		public	static	 bool toggle_HB_RANIM = false;
		public	static	 float hb_ranim = 0.4f;
		public	static	 float hb_rsign = 1.0f;
		public	static float shrink = 0.1f;
		public	static float rayon = 1.0f;
		public	static float linkscale = 1.0f;
		public  static float depthfactor = -1.0f;
		public	static float drag = 0.6f;
		public	static float spring = 5;
		
		
//Electric field line symbol radius
		public  static float symbolradius = 1.0f;

//CPK Licorice
		public	static	 bool transCPK_LICORICE = false;
		public	static	 float deltaShrink;
		public	static	 float deltaScale;
		public	static	 float deltaRadius;
		public	static	 float newShrink;
		public	static	 float newScale;
		public	static	 float newRadius;
		public	static	 float transDelta = 25.0f;
		public	static	 bool toggle_HB_TRANS = true;
		public 	static	 bool toggle_NA_INTERACTIVE = false;
		public 	static 	 bool toggle_NA_HIDE = false;
		public 	static	 bool toggle_NA_SWITCH = false;
		public 	static	 bool toggle_NA_HBALLSMOOTH = false;
		public 	static 	 bool LOD_INITIALIZED = false;
		public 	static	 bool toggle_NA_MEASURE = false;
		public 	static	 bool toggle_NA_CLICK = false;
		public  static   bool toggle_NA_MAXCAM = true;
		public  static   bool toggle_NA_AUTOMOVE = false;
		public  static   bool toggle_MESHCOMBINE = false;
		private GameObject scenecontroller = null;
		private Molecule3D Molecule3DComp = null;
		private Quaternion NA_SCCROT = new Quaternion (-0.1f, 0.1f, 0.0f, -1.0f);
		private Vector3 NA_SCCPOS = new Vector3 (0.4f, 1.8f, -12.0f);
		private bool toggle_VE_BLUR = false;
		private bool toggle_VE_SSAO = false;
		private bool toggle_VE2_VORTX = false;
		private bool toggle_VE2_TWIRL = false;
		private bool toggle_VE2_SEPIA = false;
		private bool toggle_VE2_NOISE = false;
		private bool toggle_VE2_GRAYS = false;
		private bool toggle_VE2_GLOW = false;
		private bool toggle_VE2_EDGE = false;
		private bool toggle_VE2_CONTR = false;
		private bool toggle_VE2_CCORR = false;
		private bool toggle_VE2_BLUR2 = false;
		private bool toggle_VE2_DREAM = false;
		private GUIContent[] listatom;
		private GUIContent[] listbond;
		private GUIStyle listStyle;
	
		// Bottom left UnityMol icon and link to helptext (currently local file, to be updated).
		private static Texture2D guicon = Resources.Load ("Artwork/guicon") as Texture2D;
		//private static string umolbase = "file:///opt/Unity3D/UnityMol";
//		private static string umolbase = "http://www.shaman.ibpc.fr";
		private static string umolbase = "http://www.baaden.ibpc.fr/umol";

		// Ramps for grayscale effect
		private string[] ve2_grays_ramps = {"grayscale ramp", "grayscale ramp inverse"};
		private int ve2_grays_rampn = 1;
		private int ve2_grays_rampc = 1;

		// Ramps for color correction effect
		private string[] ve2_ccorr_ramps = {"oceangradient", "nightgradient"};
		private int ve2_ccorr_rampn = 1;
		private int ve2_ccorr_rampc = 0;

		// bool for showing window
		public bool atomtype_show = false;
		public bool bondtype_show = false;
		public bool effecttype_show = false;
		public bool graycolor_show = false;
		public bool hyperballs_menu_show = false;
//		public bool surfacecolor_show= false;
		public bool SurfaceButton_show = false;
		public bool backgroundtype_show = false;
		public bool metaphortype_show = false;
//		public bool ParamFieldLine_show=false;
		public static bool FieldLine_show = false;
		public bool Surface_show = false;
		public static bool FieldLineColorGradient = true;
	
		
		// variable for the surface and field lines cutting
		public static float depthcut = 40f;
		public static bool surface_mobilecut = false;
		public static bool surface_staticcut = false;
		public static bool SurfaceCut_show = false;
		public static bool SurfaceMobileCut_show = false;
		public static float depthcutMin = 0;
		public static float depthcutMax = 0;

		public static bool surface_texture = false;
		public static bool external_surface_texture = false;
		public static bool surface_texture_show = false;
		public static string surface_texture_name;
		public static float cutX = 1f;
		public static float cutY = 0f;
		public static float cutZ = 0f;
		public static float adjustFieldLinecut = 40f;
		public static float generateSeuil = 0.5f;
		public static float generateSeuilDx_pos = 0f;
		public static float generateSeuilDx_neg = 0f;
		public static bool modif = false;
		public static bool pdbgen = false; // bool of density grid. true when density was calculated
		public static bool dxread = false; // true when dx read
		public static bool surface_build = false;
		public static bool FileBrowser_show = false;
		public static bool FileBrowser_show2 = false;
		public static ImprovedFileBrowser m_fileBrowser;
		public static string m_textPath;
		public static string m_last_extSurf_Path = null;
		public static Texture2D ext_surf;
		
		// MB for centered text		
		protected GUIStyle CentredText {
			get {
				if (m_centredText == null) {
					m_centredText = new GUIStyle (GUI.skin.label);
					m_centredText.alignment = TextAnchor.MiddleCenter;
				}
				return m_centredText;
			}
		}

		protected GUIStyle m_centredText;

//				pos = mul (UNITY_MATRIX_MVP, float4(IN.worldPos,0f));
//		clip (frac(-(-5+pos.z)/500) - 0.2);
		
		ReadDX readdx;// = new ReadDX ();
		
		// 
		public Texture2D aTexture;
		float colorRed = 0.0f;
		float colorGreen = 0.0f;
		float colorBlue = 0.0f;    

		
//	 Field line paramter: 
		public static float speed = 0.13333333f;
		public static float density = 3.4f;
		public static float linewidth = 0.2f;
//		public static float intensity = 0.1f;
		public static float linelength = 0.7f;
		public static ColorObject EnergyGrayColor = new ColorObject(Color.white);
		public static ColorObject SurfaceGrayColor = new ColorObject(Color.white); // color of surface
		public static ColorObject BackgroundColor = new ColorObject(Color.black);
		public static ColorPicker m_colorPicker = null;

		
		public static Material electricsymbol = (Material)Resources.Load ("Materials/electricparticle");
		private bool firstpass = true;

		//Electrostatic iso-surface parameters
		public static bool Elect_iso_positive_show = false;
		public static bool Elect_iso_negative_show = false;
		public static bool Elect_iso_positive_initialized = false;
		public static bool Elect_iso_negative_initialized = false;



		public static void CreateColorPicker(ColorObject col, string title)
		{
			if(m_colorPicker != null)
				m_colorPicker = null;
			m_colorPicker = new ColorPicker(colorPicker_rect, col, title);
		}
		
		public GUIMoleculeController ()
		{
			#if !UNITY_WEBPLAYER
				m_last_extSurf_Path = System.IO.Directory.GetCurrentDirectory();
			#endif


			scenecontroller = GameObject.Find ("LoadBox");
			Molecule3DComp = scenecontroller.GetComponent<Molecule3D> ();

//			listatom = new GUIContent[11];
//		    listatom[0] = new GUIContent("Cube");
//		    listatom[1] = new GUIContent("Sphere");
//		    listatom[2] = new GUIContent("HyperBall");
//		    listatom[3] = new GUIContent("Raycasting");
//		    listatom[4] = new GUIContent("Common Billboard");
//		    listatom[5] = new GUIContent("RayCasting Billboard");
//		    listatom[6] = new GUIContent("HyperBall Billboard");
//		    listatom[7] = new GUIContent("RayCasting Sprite");
//		    listatom[8] = new GUIContent("Multi-Hyperball");
//		    listatom[9] = new GUIContent("CombineMesh HyperBall");		    
//		    listatom[10] = new GUIContent("ParticleBall");		    
//
//			listbond = new GUIContent[7];
//		    listbond[0] = new GUIContent("Cube");
//		    listbond[1] = new GUIContent("Line");
//		    listbond[2] = new GUIContent("HyperStick");
//		    listbond[3] = new GUIContent("Tube Stick");
//		    listbond[4] = new GUIContent("Billboard HyperStick");
//		    listbond[5] = new GUIContent("Particle Stick");
//		    listbond[6] = new GUIContent("No Stick");
		    
			//Get the ReadDX component
			readdx = scenecontroller.GetComponent<ReadDX>();
			//Debug.Log("READDX GUIMolecule: " + readdx);

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
		    
			aTexture = (Texture2D)Resources.Load ("EnergyGrayColor2");
		}

		public static void InitMoleculeParameters()
		{
			depthcutMin = -5 + Mathf.Min(new float[]{MoleculeModel.MinValue.x,
												MoleculeModel.MinValue.y,
												MoleculeModel.MinValue.z});
			depthcutMax = 5 + Mathf.Max(new float[]{MoleculeModel.MaxValue.x,
												MoleculeModel.MaxValue.y,
												MoleculeModel.MaxValue.z});
			depthcut = depthcutMax;
		}

		// void OnGUI ()
		// {
		// 	if (GUI.tooltip != "")
		// 		GUI.Label (new Rect (120, Screen.height - 35, Screen.width - 360, 20), GUI.tooltip);
		// }

		public void CameraStop ()
		{
			
			// rectangle were the mouse isn't active 
			Rect guirect = new Rect (principalxstart, principalystart, principalwidth, principalheight);
			Rect guirect2 = new Rect (menuAtomxstart, menuAtomystart, menuAtomwidth, menuAtomheight);
			Rect guirect3 = new Rect (atomtypexstart, atomtypeystart, atomtypewidth, atomtypeheight);
			Rect guirect4 = new Rect (bondtypexstart, bondtypeystart, bondtypewidth, bondtypeheight);
			Rect guirect5 = new Rect (effecttypexstart, effecttypeystart, effecttypewidth, effecttypeheight);
			Rect guirect6 = new Rect (backtypexstart, backtypeystart, backtypewidth, backtypeheight);
			// Rect guirect7 = new Rect (graycolortypexstart, graycolortypeystart, graycolortypewidth, graycolortypeheight);
			Rect guirect8 = new Rect (metaphortypexstart, metaphortypeystart, metaphortypewidth, metaphortypeheight);
			// Rect guirect9 = new Rect (menuFieldxstart, menuFieldystart, menuFieldwidth, menuFieldheight);
			Rect guirect10 = new Rect (menuSurfacexstart, menuSurfaceystart, menuSurfacewidth, menuSurfaceheight);
			Rect rect_surfacecut = new Rect (surfacecuttypexstart, surfacecuttypeystart, surfacecuttypewidth, surfacecuttypeheight);
			Rect rect_cutplanmove = new Rect (surfacecuttypexstart, surfacecuttypeystart, surfacecuttypewidth, surfacecuttypeheight + movesurfacecuttypeheight);
			Rect guirectHyperballs = new Rect (hyperballtypexstart, hyperballtypeystart, hyperballtypewidth, hyperballtypeheight);

			// right window
			Rect guirectD11 = new Rect (Screen.width - 295, 0, 290, 350);
			Rect guirectD12 = new Rect (Screen.width - 295, 0, 290, 65);
			
			Rect rect_paramsurface = new Rect (menuparamsurfacexstart, menuparamsurfaceystart, menuparamsurfacewidth, menuparamsurfaceheight);
			Rect guirectFileBrow = new Rect (400, 100, 600, 500);				
			Rect Manipulator = new Rect (manipulatorxstart, manipulatorystart, manipulatorwidth, manipulatorheight);			
			Rect moveplan = new Rect (movesurfacecuttypexstart, movesurfacecuttypeystart, movesurfacecuttypewidth, movesurfacecuttypeheight);
			// Rect colorpickerAtom = new Rect (Screen.width - 238, 350, 238, 308);
			// TO DO : un rectangle pour desactivé le mouvement de la protéine quand on regle le plan. de coupe		
			
			Rect screen = new Rect (0, 0, Screen.width, Screen.height);
			
			Vector3 mousePos = Input.mousePosition;
			mousePos.y = Screen.height - mousePos.y;
			
			// check were mouse have to be active
			if (!screen.Contains (mousePos)
				|| (guirectFileBrow.Contains (mousePos) && FileBrowser_show)
				|| (guirectFileBrow.Contains (mousePos) && FileBrowser_show2)
				|| (Manipulator.Contains (mousePos) && menuManipulator_show)
				|| guirect.Contains (mousePos) 
				|| (menuAtom_show && guirect2.Contains (mousePos))
				|| (atomtype_show && guirect3.Contains (mousePos))
				|| (bondtype_show && guirect4.Contains (mousePos))
				|| (effecttype_show && guirect5.Contains (mousePos))
				|| (backgroundtype_show && guirect6.Contains (mousePos))
				|| (m_colorPicker != null && m_colorPicker.enabled && colorPicker_rect.Contains (mousePos))
				|| (metaphortype_show && guirect8.Contains (mousePos))
				|| (menuField_show && MenuElectro_Rect.Contains (mousePos))
				|| (menuField_show && FieldLine_show && FieldLineRect.Contains(mousePos))
				|| (menuSurface_show && guirect10.Contains (mousePos))
				|| (menuBfactor_show && guirect10.Contains (mousePos))
				|| (menuSurface_show && SurfaceCut_show && rect_cutplanmove.Contains (mousePos))
				|| (menuSurface_show && SurfaceMobileCut_show && rect_surfacecut.Contains (mousePos))
				|| (surface_texture_show && texturetype_rect.Contains (mousePos))
				|| SetAtomScale_show && !UIData.hiddenUIbutFPS && guirectD11.Contains (mousePos)
				|| GUIDisplay.m_max && UIData.hiddenUIbutFPS && guirectD12.Contains (mousePos) 
				|| GUIDisplay.m_texture && texturetype_rect.Contains (mousePos)
				|| (rect_paramsurface.Contains (mousePos) && GameObject.FindGameObjectWithTag ("SurfaceManager") && (menuSurface_show || menuBfactor_show || menuField_show))
				|| (guirectHyperballs.Contains (mousePos) && hyperballs_menu_show)
				) {
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
			if (moveplan.Contains (mousePos) && SurfaceCut_show && menuSurface_show) {
				if (Input.GetMouseButton (0)) {
					cutX += Input.GetAxis ("Mouse X") * 1 * 0.02f;
					cutY -= Input.GetAxis ("Mouse Y") * 1 * 0.02f;
					cutZ -= Input.GetAxis ("Mouse X") * 1 * 0.02f;
				}
				if (cutX < -1)
					cutX = -1;
				if (cutX > 1)
					cutX = 1;
				if (cutY < -1)
					cutY = -1;
				if (cutY > 1)
					cutY = 1;
				if (cutZ < -1)
					cutZ = -1;
				if (cutZ > 1)
					cutZ = 1;
				depthcut -= Input.GetAxis ("Mouse ScrollWheel");
//				cutZ +=Input.GetAxis("Mouse X") * 1 * 0.02f;
//				cutZ -=Input.GetAxis("Mouse Y") * 1 * 0.02f;
			}
		}
		
		public void DisplayGUI ()
		{
				
			GUI.Window (1, new Rect (principalxstart, principalystart, principalwidth, principalheight), loadGUI, "");
			
			if (firstpass) {
				firstpass = false;
//				menuManipulator_show=false;
				Camera.mainCamera.backgroundColor = new Color (0.0f, 0.0f, 0.0f);

				// MB:: temporary hack to speedup texture inclusion ------------------
//				if (1 == 0) {
//					GUIDisplay.directorypath = "/opt/Unity3D/UnityMol_SVN/";
//					GUIDisplay.file_base_name = "/opt/Unity3D/UnityMol_SVN/1KX2";
//					//	GUIDisplay.directorypath = "/opt/src/Unity/UnityMol_SVN/";
//					//	GUIDisplay.file_base_name = "/opt/src/Unity/UnityMol_SVN/1KX2";
//					GUIDisplay.file_extension = "pdb";
//					UIData.isOpenFile = true;
//					UIData.atomtype = UIData.AtomType.particleball;
//					UIData.bondtype = UIData.BondType.nobond;
//					GUIMoleculeController.menuOpen_show = false;
//					GUIDisplay.m_texture = true;
//					UIData.resetDisplay = true;
//					UIData.isCubeToSphere = false;
//					UIData.isSphereToCube = true;
//					UIData.atomtype = UIData.AtomType.hyperball;
//					atomtype_show = false;
//					MoleculeModel.oxygenColor = Color.white;
//					MoleculeModel.sulphurColor = Color.white;
//					MoleculeModel.carbonColor = Color.white;
//					MoleculeModel.nitrogenColor = Color.white;
//					MoleculeModel.phosphorusColor = Color.white;
//					MoleculeModel.hydrogenColor = Color.white;
//					MoleculeModel.unknownColor = Color.white;
//					UIData.isConfirm = true;
//				}
				// MB:: temporary hack to speedup texture inclusion ------------------

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
		
		private void loadGUI (int a)
		{
			if(toggle_NA_HIDE)
			{
				Molecule3DComp.HideAtoms();
			}else{
				Molecule3DComp.ShowAtoms();
			}

			if (GUI.Button (new Rect (20, 2, 80, 20), new GUIContent ("Open", "Open the File Open dialogue"))) {
				if (menuOpen_show)
					menuOpen_show = false;
				else {
					menuOpen_show = true;
					menuAtom_show = false;
					menuSurface_show = false;
					menuBfactor_show = false;
					menuField_show = false;
					menuManipulator_show = false;
					SetAtomScale_show = false;
					GUIDisplay.m_texture = false;
//					atomtype_show=false;
//					bondtype_show=false;
//					effecttype_show=false;
					m_colorPicker = null;
					SurfaceButton_show = false;
					backgroundtype_show = false;
//					metaphortype_show=false;
//					ParamFieldLine_show=false;
					SurfaceCut_show = false;
					SurfaceMobileCut_show = false;
					surface_texture_show = false;
//					UIData.atomtype=UIData.AtomType.particleball;
				}

			}
			
			if (GUI.Button (new Rect (80 + 21, 2, 80, 20), new GUIContent ("Atoms", "Open the Atom appearance dialogue"))) {
				if (menuAtom_show) {
					menuAtom_show = false;
//					atomtype_show=false;
//					bondtype_show=false;
//					effecttype_show=false;
//					metaphortype_show=false;
					SetAtomScale_show = false;
					// GUIDisplay.m_colorpick_Atom = null;
//					UIData.atomtype=UIData.AtomType.particleball;
				} else {
					menuAtom_show = true;
					menuOpen_show = false;
				}
			}
			if (GUI.Button (new Rect (160 + 22, 2, 80, 20), new GUIContent ("Surface", "Open the Surface rendering dialogue"))) {
				if (menuSurface_show) {
					menuSurface_show = false;
//					SurfaceCut_show=false;
//					SurfaceMobileCut_show=false;
					surface_texture_show = false;
				} else {
					menuSurface_show = true;
					menuBfactor_show = false;
					menuOpen_show = false;

				}
				if (!UIData.toggleSurf) {
					UIData.toggleBfac = false;
					UIData.toggleSurf = true;
					pdbgen = false;
				}
			}

			//No bfactor option in this version
			// if (GUI.Button (new Rect (240 + 23, 2, 80, 20), new GUIContent ("Bfactor", "Open the Bfactor settings dialogue"))) 
			// {
			// 	if (menuBfactor_show) {
			// 		menuBfactor_show = false;
			// 		SurfaceCut_show = false;
			// 		SurfaceMobileCut_show = false;
			// 		m_colorpick_fieldline = null;	
			// 		surface_texture_show = false;
			// 		;
			// 		m_colorpick_Surface = null;
			// 	} else {
			// 		menuBfactor_show = true;
			// 		menuSurface_show = false;	
			// 		menuOpen_show = false;

			// 	}
			// 	if (!UIData.toggleBfac) {
			// 		UIData.toggleBfac = true;
			// 		UIData.toggleSurf = false;
			// 		pdbgen = false;
			// 	}
			// }
			// if (GUI.Button (new Rect (320 + 24, 2, 80, 20), new GUIContent ("Electrostat.", "Open the electrostatics fieldline dialogue"))) 
			if (GUI.Button (new Rect (240 + 24, 2, 80, 20), new GUIContent ("Electrostat.", "Open the electrostatics fieldline dialogue")))
			{
				if (menuField_show) {
					menuField_show = false;
					graycolor_show = false;
				} else {
					menuField_show = true;
					menuOpen_show = false;

				}
			}
			// if (GUI.Button (new Rect (400 + 25, 2, 90, 20), new GUIContent ("Display", "Open the Display dialogue"))) 
			if (GUI.Button (new Rect (320 + 25, 2, 90, 20), new GUIContent ("Display", "Open display configuration menu")))
			{
				if (menuManipulator_show)
					menuManipulator_show = false;
				else
					menuManipulator_show = true;
			}
			
			// generate the cam target.			
			if (!toggle_NA_MAXCAM && scenecontroller.GetComponent<maxCamera> ().enabled) {
				scenecontroller.GetComponent<maxCamera> ().enabled = false;
				scenecontroller.transform.rotation = NA_SCCROT;
				scenecontroller.transform.position = NA_SCCPOS;
			} else if (toggle_NA_MAXCAM && !scenecontroller.GetComponent<maxCamera> ().enabled) { 
				scenecontroller.GetComponent<maxCamera> ().enabled = true;
			}
				
				
				
			// if (!toggle_NA_AUTOMOVE && scenecontroller.GetComponent<maxCamera> ().automove) {
			// 	scenecontroller.GetComponent<maxCamera> ().automove = false;	
			// 	Molecule3DComp.toggleFPSLog ();
			// } else if (toggle_NA_AUTOMOVE && !scenecontroller.GetComponent<maxCamera> ().automove) {
			// 	scenecontroller.GetComponent<maxCamera> ().automove = true;
			// 	Molecule3DComp.toggleFPSLog ();
			// }
			// MB
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;

		}
		
		private  void loadmenuAtom (int a)
		{

			AtomStyle ();
//			BondStyle();
			RenderParameter ();
			
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
		}
		
		private void loadControl (int a)
		{
			// VisualControl ();
			loadScreenShot ();
			//ShaderControl();
			BackGroundControl ();

			if(backgroundtype_show)
				GUI.enabled = false;
			BackColor ();
			GUI.enabled = true;
			
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("Effect", "Toggle which kind of special effect to apply to the scene"))) 
			{
				if (effecttype_show)
					effecttype_show = false;
				else
					effecttype_show = true;		
			}
			GUILayout.EndHorizontal ();

			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
		}
		
		private void loadMove (int a)
		{
			maxCamera fixeCam;
			fixeCam = scenecontroller.GetComponent<maxCamera> ();
			if (GUILayout.RepeatButton (new GUIContent ("up", "move up"))) {
				fixeCam.upyDeg ();
			}
			
			GUILayout.BeginHorizontal ();
			if (GUILayout.RepeatButton (new GUIContent ("left", "move left"))) {
				fixeCam.downxDeg ();
			}
			if (GUILayout.RepeatButton (new GUIContent ("right", "move right"))) {
				fixeCam.upxDeg ();
			}
			GUILayout.EndHorizontal ();

			if (GUILayout.RepeatButton (new GUIContent ("down", "move down"))) {
				fixeCam.downyDeg ();
			}

			GUILayout.BeginHorizontal ();
			if (GUILayout.RepeatButton (new GUIContent ("rot left", "rotate left"))) {
				fixeCam.upzDeg ();
			}
			if (GUILayout.RepeatButton (new GUIContent ("rot right", "rotate right"))) {
				fixeCam.downzDeg ();
			}
			GUILayout.EndHorizontal ();
			
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
		}
		
		
		
		//Activer une mise a jour automatique quand on clique dessus
		private void StructureChoice ()
		{
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent (structtype, "Change the molecule appearance between all atom and carbon alpha trace"))) {
				if (UIData.secondarystruct) {
					UIData.secondarystruct = false;
					structtype = "All atoms";
					UIData.changeStructure = true;
				} else {
					UIData.secondarystruct = true;
					structtype = "Carbon alpha trace";
					UIData.changeStructure = true;
				}
			}
			GUILayout.EndHorizontal ();
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
		}

		private void PhysicalChoice ()  // MB - sert a quoi?
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Velocity Colors", GUILayout.MaxWidth (50));
			
			UIData.toggleGray = GUILayout.Toggle (UIData.toggleGray, "Gray");
			
			if (UIData.toggleGray) {
				UIData.toggleColor = false;
			} else {
				UIData.toggleColor = true;
			}
			
			UIData.toggleColor = GUILayout.Toggle (UIData.toggleColor, "Normal");
			
			if (UIData.toggleColor) {
				UIData.toggleGray = false;
			} else {
				UIData.toggleGray = true;
			}
			
			GUILayout.EndHorizontal ();
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
		}

		private void VisualControl ()
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Visual", GUILayout.MaxWidth (50));
			
			UIData.toggleMouse = GUILayout.Toggle (UIData.toggleMouse, "Mouse");
			
			if (UIData.toggleMouse) {
				UIData.toggleKey = false;
			} else {
				UIData.toggleKey = true;
			}
			
			UIData.toggleKey = GUILayout.Toggle (UIData.toggleKey, "Key");
			if (UIData.toggleKey) {
				UIData.toggleMouse = false;
			} else {
				UIData.toggleMouse = true;
			}
			
			GUILayout.EndHorizontal ();
			
		}

// choice of the background color
		private void BackGroundControl ()
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (new GUIContent ("BackGround", "Toggle the use of a skybox on/off"), GUILayout.MaxWidth (120));
			
			UIData.backGroundIs = GUILayout.Toggle (UIData.backGroundIs, new GUIContent ("Yes", "Toggle the use of a skybox to ON"));
			
			if (UIData.backGroundIs) {
				UIData.backGroundNo = false;
			} else {
				UIData.backGroundNo = true;
			}
			
			UIData.backGroundNo = GUILayout.Toggle (UIData.backGroundNo, new GUIContent ("No", "Toggle the use of a skybox to OFF"));
			if (UIData.backGroundNo) {
				UIData.backGroundIs = false;
			} else {
				UIData.backGroundIs = true;
			}
			GUILayout.EndHorizontal ();
			
			// MB: only show possibility to change skybox if it is set to on
			if (UIData.backGroundIs) 
			{
				backgroundtype_show = true;
			}else{
				backgroundtype_show = false;
			}
			////////////////////////
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
		}
		
		private void MetaphorControl ()
		{
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("Metaphor", "Change HyperBalls parameters to values for standard representations"))) {
				if (metaphortype_show)
					metaphortype_show = false;
				else
					metaphortype_show = true;
				
			}	
			GUILayout.EndHorizontal ();
			////////////////////////
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
		}
		
		private void BackColor ()
		{
			// Camera.mainCamera.backgroundColor = new Color(colorRed / 255.0f, colorGreen / 255.0f, colorBlue / 255.0f);
			Camera.mainCamera.backgroundColor = new Color (colorRed, colorGreen, colorBlue);
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("White", "Set background to plain white"))) {
				BackgroundColor.color = new Color(1,1,1,0);
			}
			if (GUILayout.Button (new GUIContent ("Black", "Set background color to plain black"))) {
				BackgroundColor.color = Color.black;
			}
			GUILayout.EndHorizontal ();
			// 		GUI.color = new Color(colorRed / 255.0f, 0, 0);

			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;

			//         colorGreen = GUI.HorizontalSlider(new Rect(55, 195, 95, 30), colorGreen, 0.0f, 255.0f);
			//         GUI.Label(new Rect(25, 190, 30, 30), ((int)colorGreen).ToString());
			//         GUI.Label(new Rect(10, 190, 30, 30), "G");
			//         //GUI.Label(new Rect(535, 57.5f, 15, 15), "", "bj");

			// //        GUI.color = Color.white;
			// 	    GUI.color = new Color(0, 0, colorBlue / 255.0f);

			//         colorBlue = GUI.HorizontalSlider(new Rect(55, 220, 95, 30), colorBlue, 0.0f, 255.0f);
			//         GUI.Label(new Rect(25, 215, 30, 30), ((int)colorBlue).ToString());
			//         GUI.Label(new Rect(10, 215, 30, 30), "B");
			//GUI.Label(new Rect(535, 92.5f, 15, 15), "", "bj");  
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("Background Color", "Choose the background color"))) 
			{
				if (m_colorPicker != null)
					m_colorPicker = null;
					
				m_colorPicker = new ColorPicker(colorPicker_rect,
        										BackgroundColor,
        										"Background Color");
			}
			GUILayout.EndHorizontal ();
		}
		
// choose teh style of the atom
		private void AtomStyle ()
		{

			GUILayout.BeginHorizontal ();
			StructureChoice ();
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Atom Style       Bond Style", GUILayout.MaxWidth (170));
			GUILayout.EndHorizontal ();


			GUILayout.BeginHorizontal ();
			string atomtype = "";
			UIData.AtomType atype = UIData.atomtype;
			if(atype == UIData.AtomType.noatom)
				atype = Molecule3DComp.PreviousAtomType;
			switch (atype) {
			case UIData.AtomType.cube:
				atomtype = "Cube";
				break;
			case UIData.AtomType.sphere:	
				atomtype = "Sphere";
				break;
			case UIData.AtomType.hyperball:
				atomtype = "HyperBall";
				break;
			case UIData.AtomType.raycasting:
				atomtype = "Raycasting";
				break;
			case UIData.AtomType.billboard:
				atomtype = "Common Billboard";
				break;
			case UIData.AtomType.rcbillboard:
				atomtype = "RayCasting Billboard";
				break;
			case UIData.AtomType.hbbillboard:
				atomtype = "HyperBall Billboard";
				break;
			case UIData.AtomType.rcsprite:
				atomtype = "RayCasting Sprite";
				break;
			case UIData.AtomType.multihyperball:
				atomtype = "Multi-Hyperball";
				break;
			case UIData.AtomType.combinemeshball:
				atomtype = "CombineMesh HyperBall";
				break;
			case UIData.AtomType.particleball:
				atomtype = "ParticleBall";
				break;
			case UIData.AtomType.particleballalphablend:
				atomtype = "ParticleBallAlpahBlend";
				break;
			}

			
//			if(GUILayout.Button(atomtype))
			if (GUILayout.Button (new GUIContent (atomtype, "Change the atom appearance style or rendering method"))) {	
				if (atomtype_show)
					atomtype_show = false;
				else
					atomtype_show = true;
				bondtype_show = false;
			}

//			GUILayout.EndHorizontal();
//
//		}
//		
//		private void BondStyle()
//		{
//
//			GUILayout.BeginHorizontal();
//			GUILayout.Label("Bond Style",GUILayout.MaxWidth(85));

//			GUILayout.EndHorizontal();

//			GUILayout.BeginHorizontal();
			string bondtype = "";
			switch (UIData.bondtype) {
			case UIData.BondType.cube:
				bondtype = "Cube";
				break;
			case UIData.BondType.line:
				bondtype = "Line";
				break;
			case UIData.BondType.hyperstick:
				bondtype = "HyperStick";
				break;
			case UIData.BondType.tubestick:
				bondtype = "TubeStick";
				break;
			case UIData.BondType.bbhyperstick:
				bondtype = "Billboard HyperStick";
				break;
			case UIData.BondType.particlestick:
				bondtype = "Particle Stick";
				break;
			case UIData.BondType.nobond:
				bondtype = "No Stick";
				break;
			}

			
			if (GUILayout.Button (new GUIContent (bondtype, "Change the bond appearance style or rendering method"))) {	
				if (bondtype_show)
					bondtype_show = false;
				else
					bondtype_show = true;
				atomtype_show = false;
			}

			GUILayout.EndHorizontal ();
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;

		}
			
		public float LabelSlider (float sliderValue, float sliderMinValue, float sliderMaxValue, string labelText, string toolTip, bool enable, int sliderwidth, int labelwidth=100)
		{
			GUI.enabled = enable;
			GUILayout.Label (new GUIContent (labelText, toolTip), GUILayout.MinWidth (labelwidth));
			sliderValue = GUILayout.HorizontalSlider (sliderValue, sliderMinValue, sliderMaxValue, GUILayout.Width (sliderwidth));
			//GUI.Box (Rect (10, 10, 100, 20), new GUIContent ("", "My Tooltip"), GUIStyle.none);

			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;

			return sliderValue;
		}
		
		
		
// All the function who open a new window in the GUI.  =============================================================================================		
		
//	public void SetOpenMenu()
//		{
//			if(atomtype_show)
//			{
////				GUIContent c=new GUIContent("Set Atom Type","Set the atom type");
//				GUI.Window( 2, new Rect(atomtypexstart,atomtypeystart,atomtypewidth,atomtypeheight), loadTypeGUIAtom, "");
////				Debug.Log(Screen.width-195);
//			}
//		}
				
		
		public void SetAtomMenu ()
		{
			if (menuAtom_show) {
//				GUIContent c=new GUIContent("Set Atom Type","Set the atom type");
				GUI.Window (10, new Rect (menuAtomxstart, menuAtomystart, menuAtomwidth, menuAtomheight), loadmenuAtom, "Atom appearance");
//				Debug.Log(Screen.width-195);
			}
		}

		public void SetSurfaceMenu ()
		{
			if (menuSurface_show) { 
//			Debug.Log("test 42000");
//			GUIContent c=new GUIContent("Set Surface Params");
				
				GUI.Window (20, new Rect (menuSurfacexstart, menuSurfaceystart, menuSurfacewidth, menuSurfaceheight), loadTypeGUISurface, "Surface");
//			Debug.Log(Screen.width-195);
				
				if (GameObject.FindGameObjectWithTag ("SurfaceManager")) {
					GUI.Window (25, new Rect (menuparamsurfacexstart, menuparamsurfaceystart, menuparamsurfacewidth, menuparamsurfaceheight), loadTypeGUISurfaceParams, "Surface");	
				}
			}
		}
		
		public void SetBfactorMenu ()
		{
			if (menuBfactor_show) {
//				GUIContent c=new GUIContent("Set Atom Type","Set the atom type");
				GUI.Window (20, new Rect (menuSurfacexstart, menuSurfaceystart, menuSurfacewidth, menuSurfaceheight), loadTypeGUISurface, "B Factor");
//				Debug.Log(Screen.width-195);
				if (GameObject.FindGameObjectWithTag ("SurfaceManager")) {
					GUI.Window (25, new Rect (menuparamsurfacexstart, menuparamsurfaceystart, menuparamsurfacewidth, menuparamsurfaceheight), loadTypeGUISurfaceParams, "Surface");	
				}
			}
		}

		public void SetFieldMenu ()
		{
			if (menuField_show) {
				//			Debug.Log("test 42000");
//				GUIContent c = new GUIContent ("Set Field Lines Parameters");
					
				GUI.Window (30, MenuElectro_Rect, loadTypeGUIField, "Electrostatic 2");
				//			Debug.Log(Screen.width-195);
				// if (GameObject.FindGameObjectWithTag ("SurfaceManager")) {
				// 	GUI.Window (25, new Rect (menuSurfacexstart, menuSurfaceystart + 100, menuSurfacewidth, menuSurfaceheight + 50), loadTypeGUISurfaceParams, "Surface");	
				// }
			}

			if (FieldLine_show && menuField_show) {
				GUI.Window (31, FieldLineRect, loadTypeGUIFieldVisible, "");
			}
			
		}
		
		public void SetManipulatorMenu ()
		{
			if (menuManipulator_show) {
//				GUIContent c=new GUIContent("Set Atom Type","Set the atom type");
//				GUI.Window( 2, new Rect(atomtypexstart,atomtypeystart,atomtypewidth,atomtypeheight), loadmenuManipulator, "");
//				Debug.Log(Screen.width-195);
//				GUIContent d = new GUIContent ("Manipulator", "Manipulator submenu");
				GUIContent d = new GUIContent ("Display");

				GUI.Window (42, rect_manipulator, loadControl, d);
			}
		}
		
		public void SetMnipulatormove ()
		{
			if (!menuManipulator_show) {
				GUIContent d = new GUIContent ("Movement", "");
				GUI.Window (42, rect_manipulatormove, loadMove, d);
			}
		}
		
		public void SetAtomType ()
		{
			if (atomtype_show && menuAtom_show) {
//				GUIContent c = new GUIContent ("Set Atom Type", "Set the atom type");
				GUI.Window (11, new Rect (atomtypexstart, atomtypeystart, atomtypewidth, atomtypeheight), loadTypeGUIAtom, "Choose Atom Style");
//				Debug.Log(Screen.width-195);
			}
		}
		
		public void SetBondType ()
		{
			if (bondtype_show && menuAtom_show) {
//				GUIContent c = new GUIContent ("Set Bond Type", "Set the bond type");
				GUI.Window (11, new Rect (bondtypexstart, bondtypeystart, bondtypewidth, bondtypeheight), loadTypeGUIBond, "Bond Style");
//				Debug.Log(Screen.width-195);
			}
		}
	
		public void SetHyperBall ()
		{
			if ((UIData.atomtype == UIData.AtomType.hyperball || UIData.bondtype == UIData.BondType.hyperstick) && (!bondtype_show && !atomtype_show) && menuAtom_show) {
				GUI.Window (12, new Rect (hyperballtypexstart, hyperballtypeystart, hyperballtypewidth, hyperballtypeheight), loadTypeGUIhyperball, "Hyperball Style");
				hyperballs_menu_show = true;
			} else
				hyperballs_menu_show = false;
		}
		
		public void SetEffectType ()
		{
			if (effecttype_show) {
//				GUIContent c = new GUIContent ("Set Effect Type", "Set Effect Type");
				GUI.Window (25, new Rect (effecttypexstart, effecttypeystart, effecttypewidth, effecttypeheight), loadTypeGUIEffect, "Effect");
//				Debug.Log(Screen.width-195);
			}
		}
	
		public void SetSurfaceTexture ()
		{
			if (surface_texture_show && (menuSurface_show || menuBfactor_show)) {	
				GUIDisplay.m_texture = false;		
//				GUI.Window(41, new Rect(texturetypexstart,texturetypeystart,texturewidth,textureheight),loadSurfaceTexture, "Surface texture parameters");
				GUI.Window (41, new Rect (texturetypexstart, texturetypeystart, texturewidth, textureheight), loadSurfaceTexture, "");
			}
		}

		public void SetSurtfaceMobileCut ()
		{
			if (SurfaceMobileCut_show && (menuSurface_show || menuBfactor_show)) {
				GUI.Window (21, new Rect (surfacecuttypexstart, surfacecuttypeystart, surfacecuttypewidth, surfacecuttypeheight), loadSurfaceMobileCut, "Surface cut poarameters");
			}
		}
		
		public void SetSurfaceCut ()
		{
			if (SurfaceCut_show && (menuSurface_show || menuBfactor_show)) {
				GUI.Window (21, new Rect (surfacecuttypexstart, surfacecuttypeystart, surfacecuttypewidth, surfacecuttypeheight), loadSurfaceCut, "Surface cut parameters");
				GUI.Window (22, new Rect (movesurfacecuttypexstart, movesurfacecuttypeystart, movesurfacecuttypewidth, movesurfacecuttypeheight), loadTypeGUIsurfaceMove, "move cut plane");

			}
		}
	
		public void SetBackGroundType ()
		{
			
			if (backgroundtype_show) {
//				GUIContent c = new GUIContent ("Set BackGround Type");
				
				GUI.Window (43, new Rect (backtypexstart, backtypeystart, backtypewidth, backtypeheight), loadTypeGUIBack, "Background");
//				Debug.Log(Screen.width-195);
			}
		}
		
		public void SetMetaphorType ()
		{
			
			if (metaphortype_show && menuAtom_show) {
//				GUIContent c = new GUIContent ("Set Metaphor Type");
				
				GUI.Window (22, new Rect (metaphortypexstart, metaphortypeystart, metaphortypewidth, metaphortypeheight), loadTypeGUIMetaphor, "Metaphor");
				//				Debug.Log(Screen.width-195);
			}
		}
		
		
		
// end of windows function =============================================================================================		

		public  void MinLoadTypeGUIAtom (int b)
		{
			
		}

		
// function who tell what happens when a window is open =============================================================================================
		private  void loadTypeGUIAtom (int a)
		{
			GUILayout.BeginHorizontal ();
			
			if (GUILayout.Button (new GUIContent ("Cube", "Use cubes to represent atoms"))) {	

				UIData.resetDisplay = true;
				UIData.isCubeToSphere = false;
				UIData.isSphereToCube = true;
				UIData.atomtype = UIData.AtomType.cube;
				Debug.Log ("UIData.resetDisplay:" + UIData.resetDisplay);
				Debug.Log ("UIData.isCubeToSphere:" + UIData.isCubeToSphere);
				Debug.Log ("UIData.isSphereToCube:" + UIData.isSphereToCube);
				atomtype_show = false;
				toggle_NA_HIDE = false;
			}
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			
			if (GUILayout.Button (new GUIContent ("Sphere", "Use triangulated spheres to represent atoms"))) {
				UIData.resetDisplay = true;
				UIData.isCubeToSphere = true;
				UIData.isSphereToCube = false;
				UIData.atomtype = UIData.AtomType.sphere;
				Debug.Log ("UIData.resetDisplay:" + UIData.resetDisplay);
				Debug.Log ("UIData.isCubeToSphere:" + UIData.isCubeToSphere);
				Debug.Log ("UIData.isSphereToCube:" + UIData.isSphereToCube);
				atomtype_show = false;
				toggle_NA_HIDE = false;
			}
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
			
			if (GUILayout.Button (new GUIContent ("Hyperball", "Use the HyperBalls shader to render atoms"))) {
				UIData.resetDisplay = true;
				UIData.isCubeToSphere = false;
				UIData.isSphereToCube = true;
				UIData.atomtype = UIData.AtomType.hyperball;
				Debug.Log ("UIData.resetDisplay:" + UIData.resetDisplay);
				Debug.Log ("UIData.isCubeToSphere:" + UIData.isCubeToSphere);
				Debug.Log ("UIData.isSphereToCube:" + UIData.isSphereToCube);
				atomtype_show = false;
				toggle_NA_HIDE = false;
			}
			GUILayout.EndHorizontal ();


			if (UIData.openAllMenu) {

				GUILayout.BeginHorizontal ();

				if (GUILayout.Button (new GUIContent ("Raycasting", "Use raycasting to represent atoms"))) {
					UIData.resetDisplay = true;
					UIData.isCubeToSphere = false;
					UIData.isSphereToCube = true;
					UIData.atomtype = UIData.AtomType.raycasting;
					Debug.Log ("UIData.resetDisplay:" + UIData.resetDisplay);
					Debug.Log ("UIData.isCubeToSphere:" + UIData.isCubeToSphere);
					Debug.Log ("UIData.isSphereToCube:" + UIData.isSphereToCube);
					atomtype_show = false;
					toggle_NA_HIDE = false;
				}
				GUILayout.EndHorizontal ();
			
			
			}
			
			if (UIData.openAllMenu) {
				GUILayout.BeginHorizontal ();

				if (GUILayout.Button (new GUIContent ("RayCasting Sprite", "Use RayCasting Sprites to represent atoms"))) {
					UIData.resetDisplay = true;
					UIData.isSphereToCube = true;
					UIData.isCubeToSphere = false;
					UIData.atomtype = UIData.AtomType.rcsprite;
					Debug.Log ("UIData.resetDisplay:" + UIData.resetDisplay);
					Debug.Log ("UIData.isCubeToSphere:" + UIData.isCubeToSphere);
					Debug.Log ("UIData.isSphereToCube:" + UIData.isSphereToCube);
					atomtype_show = false;
					toggle_NA_HIDE = false;
				}
				GUILayout.EndHorizontal ();
				
				GUILayout.BeginHorizontal ();

				if (GUILayout.Button (new GUIContent ("Multi-Hyperball", "Use Multi-Hyperballs rendering to represent atoms"))) {
					UIData.resetDisplay = true;
					UIData.isSphereToCube = true;
					UIData.isCubeToSphere = false;
					UIData.atomtype = UIData.AtomType.multihyperball;
					Debug.Log ("UIData.resetDisplay:" + UIData.resetDisplay);
					Debug.Log ("UIData.isCubeToSphere:" + UIData.isCubeToSphere);
					Debug.Log ("UIData.isSphereToCube:" + UIData.isSphereToCube);
					atomtype_show = false;
					toggle_NA_HIDE = false;
				}				
				GUILayout.EndHorizontal ();
				
				GUILayout.BeginHorizontal ();

				if (GUILayout.Button (new GUIContent ("CombineMesh HyperBall", "Use the CombineMesh HyperBall shader to represent atoms"))) {
					UIData.resetDisplay = true;
					UIData.isSphereToCube = true;
					UIData.isCubeToSphere = false;
					UIData.atomtype = UIData.AtomType.combinemeshball;
					Debug.Log ("UIData.resetDisplay:" + UIData.resetDisplay);
					Debug.Log ("UIData.isCubeToSphere:" + UIData.isCubeToSphere);
					Debug.Log ("UIData.isSphereToCube:" + UIData.isSphereToCube);
					atomtype_show = false;
					toggle_NA_HIDE = false;
				}						
				GUILayout.EndHorizontal ();
			}

			GUILayout.BeginHorizontal ();

			if (GUILayout.Button (new GUIContent ("ParticleBall", "Use the ParticleBall shader to represent atoms"))) {
				UIData.resetDisplay = true;
				UIData.isSphereToCube = false;
				UIData.isCubeToSphere = false;
				UIData.atomtype = UIData.AtomType.particleball;
				Debug.Log ("UIData.resetDisplay:" + UIData.resetDisplay);
				Debug.Log ("UIData.isCubeToSphere:" + UIData.isCubeToSphere);
				Debug.Log ("UIData.isSphereToCube:" + UIData.isSphereToCube);
				UIData.resetBondDisplay = true;
				//UIData.bondtype=UIData.BondType.nobond;
				atomtype_show = false;
				toggle_NA_HIDE = false;
			}						

			GUILayout.EndHorizontal ();
			
			// GUILayout.BeginHorizontal();


			// if(GUILayout.Button(new GUIContent("ParticleBallAlphaBlend","Use the ParticleBallAlphaBlend to represent the atoms.")))
			// {
			// 	UIData.resetDisplay=true;
			// 	UIData.isSphereToCube=true;
			// 	UIData.isCubeToSphere=false;
			// 	UIData.atomtype=UIData.AtomType.particleballalphablend;
			// 	Debug.Log("UIData.resetDisplay:"+UIData.resetDisplay);
			// 	Debug.Log("UIData.isCubeToSphere:"+UIData.isCubeToSphere);
			// 	Debug.Log("UIData.isSphereToCube:"+UIData.isSphereToCube);
			// 	UIData.resetBondDisplay=true;
			// 	UIData.bondtype=UIData.BondType.nobond;
			// 	atomtype_show=false;
			// }						

			// GUILayout.EndHorizontal();

			if (UIData.openAllMenu) {			
			
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Billboard", GUILayout.MaxWidth (50));
				
				UIData.toggleClip = GUILayout.Toggle (UIData.toggleClip, new GUIContent ("Clip", "Toggle the Clip plane"));
				
				if (UIData.toggleClip) {
					UIData.togglePlane = false;
				} else {
					UIData.togglePlane = true;
				}
				
				UIData.togglePlane = GUILayout.Toggle (UIData.togglePlane, new GUIContent ("Plane", "Toggle the Cut plane"));
				if (UIData.togglePlane) {
					UIData.toggleClip = false;
				} else {
					UIData.toggleClip = true;
				}
				
				GUILayout.EndHorizontal ();


				toggle_MESHCOMBINE = GUILayout.Toggle (toggle_MESHCOMBINE, new GUIContent ("MESHCOMBINE", "Toggle the mesh combination"));
		
				if (!toggle_MESHCOMBINE) {
					UIData.meshcombine = false;
					UIData.resetMeshcombine = true;
				
				} else if (toggle_MESHCOMBINE) { 
					UIData.meshcombine = true;
					UIData.resetMeshcombine = true;
				}
			}
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
		}
		
		private  void loadTypeGUIBond (int a)
		{
					
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("", GUILayout.MaxWidth (85));
			GUILayout.EndHorizontal ();
			//if(UIData.atomtype==UIData.AtomType.particleball&&!UIData.openAllMenu)GUI.enabled=false;
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("Cube", "Use Cubes to represent bonds"))) {
				UIData.resetBondDisplay = true;
				UIData.bondtype = UIData.BondType.cube;
				bondtype_show = false;
			}
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();

			if (GUILayout.Button (new GUIContent ("Line", "Use the Line renderer to represent bonds"))) {
				UIData.resetBondDisplay = true;
				UIData.bondtype = UIData.BondType.line;
				bondtype_show = false;
			}
			
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();

			if (GUILayout.Button (new GUIContent ("HyperStick", "Use the HyperStick shader to represent bonds"))) {	
				UIData.resetBondDisplay = true;
				UIData.bondtype = UIData.BondType.hyperstick;
				bondtype_show = false;
				
			}
			GUILayout.EndHorizontal ();
			
			if (UIData.openAllMenu) {			
				GUILayout.BeginHorizontal ();

				if (GUILayout.Button (new GUIContent ("Tube Stick", "Use the Tube Stick renderer to represent bonds"))) {
					UIData.resetBondDisplay = true;
					UIData.bondtype = UIData.BondType.tubestick;
					bondtype_show = false;
				}

				GUILayout.EndHorizontal ();
			
				GUILayout.BeginHorizontal ();

				if (GUILayout.Button (new GUIContent ("Billboard HyperStick", "Use the Billboard HyperStick shader to represent bonds"))) {
					UIData.resetBondDisplay = true;
					UIData.bondtype = UIData.BondType.bbhyperstick;
				}

				GUILayout.EndHorizontal ();
			
				GUILayout.BeginHorizontal ();

				if (GUILayout.Button (new GUIContent ("Particle Stick", "Use the Particle Stick shader to represent bonds"))) {
					UIData.resetBondDisplay = true;
					UIData.bondtype = UIData.BondType.particlestick;
					bondtype_show = false;
				}

				GUILayout.EndHorizontal ();
			}
			GUI.enabled = true;
			
			GUILayout.BeginHorizontal ();

			if (GUILayout.Button (new GUIContent ("No Stick", "Do not render any bonds"))) {
				UIData.resetBondDisplay = true;
				UIData.bondtype = UIData.BondType.nobond;
				bondtype_show = false;
			}
			

			GUILayout.EndHorizontal ();
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
			
		}
		
		private  void loadTypeGUIMetaphor (int a)
		{
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("CPK", "CPK representation as balls and sticks"))) {
				newRadius = 0.2f;
				deltaRadius = (newRadius - rayon) / transDelta;
				newShrink = 0.0001f;
				deltaShrink = (newShrink - shrink) / transDelta;
				newScale = 0.3f;
				deltaScale = (newScale - linkscale) / transDelta;
				transCPK_LICORICE = true;

			
			}
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
			
			if (GUILayout.Button (new GUIContent ("Licorice", "Licorice representation of the molecule"))) {
				newRadius = 0.1f;
				deltaRadius = (newRadius - rayon) / transDelta;
				newShrink = 0.0001f;
				deltaShrink = (newShrink - shrink) / transDelta;
				newScale = 1.0f;
				deltaScale = (newScale - linkscale) / transDelta;
				transCPK_LICORICE = true;	

				
			}						
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			
			if (GUILayout.Button (new GUIContent ("VdW", "van der Waals representation as spacefilling spheres"))) {
				newRadius = 1.0f;
				deltaRadius = (newRadius - rayon) / transDelta;
				newShrink = 0.8f;
				deltaShrink = (newShrink - shrink) / transDelta;
				newScale = 1.0f;
				deltaScale = (newScale - linkscale) / transDelta;
				transCPK_LICORICE = true;	

			
			}						
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			
			if (GUILayout.Button (new GUIContent ("Smooth", "Smooth HyperBalls metaphor representation"))) {
				newRadius = 0.35f;
				deltaRadius = (newRadius - rayon) / transDelta;
				newShrink = 0.4f;
				deltaShrink = (newShrink - shrink) / transDelta;
				newScale = 1.0f;
				deltaScale = (newScale - linkscale) / transDelta;
				transCPK_LICORICE = true;	

			
			}						
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			
			if (GUILayout.Button (new GUIContent ("SmoothLink", "SmoothLink HyperBalls representation"))) {
				newRadius = 0.4f;
				deltaRadius = (newRadius - rayon) / transDelta;
				newShrink = 0.5f;
				deltaShrink = (newShrink - shrink) / transDelta;
				newScale = 1.0f;
				deltaScale = (newScale - linkscale) / transDelta;
				transCPK_LICORICE = true;	

			
			}						
			GUILayout.EndHorizontal ();
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
		
		}
		
		private  void loadTypeGUIField (int a)
		{

			//			GUILayout.BeginHorizontal();
			//			GUILayout.Label("Parameter",GUILayout.MaxWidth(100));
			//			GUILayout.EndHorizontal();
			#if UNITY_WEBPLAYER
			GUI.enabled = false;
			#endif
			GUILayout.BeginHorizontal ();
			generateSeuilDx_neg = LabelSlider (generateSeuilDx_neg, -10f, 0f, "S: " + Mathf.Round (generateSeuilDx_neg * 10f) / 10f, "Ramp value used for surface generation",GUI.enabled , 50, 50);
			if (GUILayout.Button (new GUIContent ("Load Neg.", "Read an OpenDx format electrostatic field and generate a surface"))) {
				MoleculeModel.SurfaceFileExist = true;
				if (!dxread) {	
					
					readdx.ReadFile(GUIDisplay.file_base_name+".dx",MoleculeModel.Offset);
					dxread = true;
				}
				
				string tag = "Elect_iso_negative";
				Elect_iso_negative_initialized = true;
				Elect_iso_negative_show = true;

				GameObject[] IsoSurfaces = GameObject.FindGameObjectsWithTag(tag);
				foreach(GameObject iso in IsoSurfaces)
					Object.Destroy(iso);
				readdx.isoSurface (generateSeuilDx_neg,Color.red,tag);

			}
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			generateSeuilDx_pos = LabelSlider (generateSeuilDx_pos, 0f, 10f, "S: " + Mathf.Round (generateSeuilDx_pos * 10f) / 10f, "Ramp value used for surface generation",GUI.enabled , 50, 50);
			
			if (GUILayout.Button (new GUIContent ("Load Pos.", "Read an OpenDx format electrostatic field and generate a surface"))) {
				MoleculeModel.SurfaceFileExist = true;
				if (!dxread) {	
					
					readdx.ReadFile(GUIDisplay.file_base_name+".dx",MoleculeModel.Offset);
					dxread = true;
				}

				string tag = "Elect_iso_positive";
				Elect_iso_positive_show = true;
				Elect_iso_positive_initialized = true;

				GameObject[] IsoSurfaces = GameObject.FindGameObjectsWithTag(tag);
				foreach(GameObject iso in IsoSurfaces)
					Object.Destroy(iso);
				readdx.isoSurface (generateSeuilDx_pos,Color.blue,tag);

			}
			GUILayout.EndHorizontal ();


			if (dxread && Elect_iso_negative_initialized)
				GUI.enabled = true;
			else 
				GUI.enabled = false;
			GUILayout.BeginHorizontal();
			if (GUILayout.Button (new GUIContent ("Toggle Neg.", "Toggles negative iso-surface  from visible to hidden and vice versa"))) {
				string tag = "Elect_iso_negative";
				if (Elect_iso_negative_show) 
				{
					Surface_show = false;
					SurfaceCut_show = false;
					SurfaceMobileCut_show = false;
					Elect_iso_negative_show = false;
					GameObject[] IsoSurfaces = GameObject.FindGameObjectsWithTag(tag);
					foreach(GameObject iso in IsoSurfaces)
						iso.renderer.enabled = false;
				} else {
					Elect_iso_negative_show = true;
					GameObject[] IsoSurfaces = GameObject.FindGameObjectsWithTag(tag);
					foreach(GameObject iso in IsoSurfaces)
						iso.renderer.enabled = true;
					//rayon = 0.001f;
					// OBJ obj = new OBJ();
					// obj.Load();
					// MoleculeModel.SurfaceFileExist=true;
				}				
			
			}
			if (dxread && Elect_iso_positive_initialized)
				GUI.enabled = true;
			else 
				GUI.enabled = false;
			if (GUILayout.Button (new GUIContent ("Toggle Pos.", "Toggles positive iso-surface from visible to hidden and vice versa"))) {
				string tag = "Elect_iso_positive";
				if (Elect_iso_positive_show) 
				{
					Surface_show = false;
					SurfaceCut_show = false;
					SurfaceMobileCut_show = false;
					Elect_iso_positive_show = false;
					GameObject[] IsoSurfaces = GameObject.FindGameObjectsWithTag(tag);
					foreach(GameObject iso in IsoSurfaces)
						iso.renderer.enabled = false;
				} else {
					Elect_iso_positive_show = true;
					GameObject[] IsoSurfaces = GameObject.FindGameObjectsWithTag(tag);
					foreach(GameObject iso in IsoSurfaces)
						iso.renderer.enabled = true;
					//rayon = 0.001f;
					// OBJ obj = new OBJ();
					// obj.Load();
					// MoleculeModel.SurfaceFileExist=true;
				}				
			
			}
			GUILayout.EndHorizontal();

			#if UNITY_WEBPLAYER
			if(!MoleculeModel.FieldLineFileExist)
				GUI.enabled = false;
			else
				GUI.enabled = true;
			#else
			GUI.enabled = true;
			#endif

			GUILayout.BeginHorizontal ();
			
			// rajouter un if si présence de field line ou non//////
			// add an if if we have a json file or not /////////
			
			if (GUILayout.Button (new GUIContent ("Field Line", "Toggles animated field lines from visible to hidden and vice versa"))) {

				if (FieldLine_show) {
					FieldLine_show = false;
					GameObject FieldLineManager = GameObject.Find ("FieldLineManager");
					FieldLineModel Line = FieldLineManager.transform.GetComponent<FieldLineModel> ();
					Line.killCurrentEffects ();
				} else {
					FieldLine_show = true;
					if(GameObject.FindGameObjectsWithTag("FieldLineManager").Length == 0)
						FieldLineStyle.DisplayFieldLine ();
				}				
			
			}

			GUILayout.EndHorizontal ();
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;

		}
	
		private void loadTypeGUIFieldVisible (int a)
		{
			
			if (GUILayout.Button (new GUIContent ("Energy/Field Color", "Choose color to represent potential energy or field line"))) 
			{
				CreateColorPicker(EnergyGrayColor, "Field Lines Color");
				FieldLineColorGradient = false;
			}
			if (GUILayout.Button (new GUIContent ("Color Gradient", "Display field lines with a color gradient")))
			{
				// m_colorpick_fieldline = null;
				FieldLineColorGradient = true;
			}
			//			GUI.enabled=true;
		
			speed = LabelSlider (speed, 0.001f, 1.0f, "Speed  " + speed, "Determines field line animation speed", true, 150);
			density = LabelSlider (density, 1.0f, 8.0f, "Density  " + density, "Determines field line density", true, 150);
			linewidth = LabelSlider (linewidth, 0.01f, 5.0f, "Width  " + linewidth, "Determines field line width", true, 150);
			linelength = LabelSlider (linelength, 0.8f, 0.1f, "Length " + (1 - linelength), "Determines field line length", true, 150);
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;

			//			ParamFieldLine_show=false;	
		}
	
		private  void loadTypeGUISurface (int a)
		{

			//			GUILayout.BeginHorizontal();
			//			GUILayout.Label("Parameter",GUILayout.MaxWidth(100));
			//			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal ();

			if (GUILayout.Button (new GUIContent ("Generate", "Generate a new surface mesh"))) {
				if (!pdbgen) {	
					MoleculeModel.SurfaceFileExist = true;
					PDBtoDEN generatedensity = new PDBtoDEN ();
					generatedensity.TranPDBtoDEN ();
					//					Debug.Log("time : PDBtoDEN "+ (DateTime.Now-temp));	
					pdbgen = true;
					surface_build = true;
				}
				PDBtoDEN.ProSurface (generateSeuil);		
			}

			
			
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			generateSeuil = LabelSlider (generateSeuil, 0.002f, 2f, "S:" + Mathf.Round (generateSeuil * 10f) / 10f, "Determines ramp value for surface generation", true, 100, 50);
			
			GUILayout.EndHorizontal ();
			if (surface_build || MoleculeModel.SurfaceFileExist)
				GUI.enabled = true;
			else 
				GUI.enabled = false;
			if (GUILayout.Button (new GUIContent ("Toggle surface", "Toggles surface from visible to hidden and vice versa"))) {

				if (Surface_show) {
					Surface_show = false;
					SurfaceCut_show = false;
					SurfaceMobileCut_show = false;
					GameObject[] SurfaceManager = GameObject.FindGameObjectsWithTag ("SurfaceManager");
					foreach (GameObject Surface in SurfaceManager) {
//						Surface.SetActiveRecursively (false);
						Surface.SetActive (false);
					}
					

				} else {
					Surface_show = true;
					GameObject SurfaceManager = GameObject.Find ("SurfaceManager");
//					SurfaceManager.SetActiveRecursively (true);
					SurfaceManager.SetActive (true);
				}				
				
			}	
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;

		}

		private void loadTypeGUISurfaceParams (int a)
		{
			
			
			if (GUILayout.Button (new GUIContent ("Surface Color", "Choose the color of the surface"))) {
				CreateColorPicker(SurfaceGrayColor,"Surface Color");
			}
				
			if (GUILayout.Button (new GUIContent ("Surface Texture", "Choose the texture of the surface"))) 
			{
				if (surface_texture_show) 
				{
					surface_texture_show = false;
					//surface_texture = false;
					//external_surface_texture = false;
				} else {	
					surface_texture_show = true;
					//SurfaceCut_show = false;
					//SurfaceMobileCut_show = false;
					//SetAtomScale_show = false;
					// GUIDisplay.m_colorpick_Atom = null;
				}
			}

				
			if (GUILayout.Button (new GUIContent ("Surface static cut", "Activate a static cut plane on the surface"))) {
				if (surface_staticcut) {
					surface_staticcut = false;
					SurfaceCut_show = false;
					SurfaceMobileCut_show = false;
				} else {
					surface_staticcut = true;
					surface_mobilecut = false;
					SurfaceCut_show = true;
					SurfaceMobileCut_show = false;
				}
			}
			
			if (GUILayout.Button (new GUIContent ("Surface mobile cut", "Activate a mobile cut plane on the surface"))) {
				if (surface_mobilecut) {
					surface_mobilecut = false;
					SurfaceMobileCut_show = false;
				} else {
					SurfaceMobileCut_show = true;
					surface_mobilecut = true;
					surface_staticcut = false;
					SurfaceCut_show = false;

				}
			}
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;

		}
		
		private void loadTypeGUIsurfaceMove (int a)
		{
			
		}

		private  void loadTypeGUIBack (int a)
		{
			GameObject LocCamera = GameObject.Find ("Camera");

			// GUILayout.BeginHorizontal ();
			// GUILayout.Label ("BackGround Style", GUILayout.MaxWidth (85));
			// GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();

			if (GUILayout.Button (new GUIContent ("1", "Lerpz background"))) {
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/skyBoxLerpzMaterial");		
			}
//			GUILayout.EndHorizontal ();			
//			GUILayout.BeginHorizontal ();

			if (GUILayout.Button (new GUIContent ("2", "HotDesert background"))) {
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/skyBoxHotDesert");
			}						
//			GUILayout.EndHorizontal ();
			//GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("3", "Molecule background"))) {
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/skyBoxmolecularMaterial");
			}						
//			GUILayout.EndHorizontal ();
//			GUILayout.BeginHorizontal ();
			
			if (GUILayout.Button (new GUIContent ("4", "Snow background"))) {
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/skyBoxSnow");
			}						
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("5", "DawnDusk Skybox"))) {
							LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/DawnDusk Skybox");				
			}						
			if (GUILayout.Button (new GUIContent ("6", "Eerie Skybox"))) {
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/Eerie Skybox");				
			}						
			if (GUILayout.Button (new GUIContent ("7", "MoonShine Skybox"))) {
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/MoonShine Skybox");				
			}						
			if (GUILayout.Button (new GUIContent ("8", "Overcast1 Skybox"))) {
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/Overcast1 Skybox");				
			}						
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			
			if (GUILayout.Button (new GUIContent ("9", "Overcast2 Skybox"))) {
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/Overcast2 Skybox");
			}
			if (GUILayout.Button (new GUIContent ("10", "StarryNight Skybox"))) {
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/StarryNight Skybox");				
			}						
			if (GUILayout.Button (new GUIContent ("11", "Sunny1 Skybox"))) {
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/Sunny1 Skybox");				
			}						
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("12", "Sunny2 Skybox"))) {
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/Sunny2 Skybox");				
			}						
			if (GUILayout.Button (new GUIContent ("13", "Sunny3 Skybox"))) {
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/Sunny3 Skybox");				
			}
			GUILayout.EndHorizontal ();

			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
		}

		private void loadScreenShot ()
		{

			if (GUILayout.Button (new GUIContent ("ScreenShot", "Capture the screen and save image to the original file path"))) {
				GameObject LocCamera = GameObject.Find ("Camera");
				ScreenShot comp = LocCamera.GetComponent<ScreenShot> ();
				comp.open = true;
							
							
			}	
			//////////modify///////////////////////
						
			if (GUILayout.Button (new GUIContent ("ScreenShot Sequence", "Capture the screen sequentially and save images to the original file path"))) {
				GameObject LocCamera = GameObject.Find ("Camera");
				ScreenShot comp = LocCamera.GetComponent<ScreenShot> ();
				if (comp.sequence) {
					comp.sequence = false;
					comp.open = false;
				} else {
					comp.sequence = true;
					comp.open = true;
				}
				
			}	
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;

		}

		public static void FileSelectedCallback (string path)
		{
			#if !UNITY_WEBPLAYER
				m_fileBrowser = null;
				if (path != null) {
					m_textPath = path;
					m_last_extSurf_Path = System.IO.Path.GetDirectoryName (path);
					external_surface_texture = true;
				}
				//			FileBrowser_show = false;
				surface_texture = false;
				GUIMoleculeController.FileBrowser_show2 = false;
				WWW www = new WWW ("file://" + GUIMoleculeController.m_textPath);
				ext_surf = www.texture;
				Debug.Log (m_textPath);
			#endif
		}
		
		// HELPER FUNCTION TO FILL A TEXTURE CHOICE MENU WITH UP TO 15 BOXES
		private void textureMenu (string texDir, string[] texList, string texDescr)
		{
			GUI.Label (new Rect (0, 0, 290, 20), "Surface Texture - " + texDescr, CentredText);
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("<<", "Go to previous series of textures"))) { // cycle through texture sets 
				texture_set--;
				//Skip fun textures for the article version
				//if(texture_set == 4)
				//	texture_set = 3;

				if (only_best_textures) {
					if(texture_set>0) texture_set = -texture_set;
					if (texture_set < besttexSet_min) 
						texture_set = -1; 
				} else {
				if (texture_set < 1) {
					texture_set = texSet_max;
				}
			}
			}			

			//			if(GUILayout.Button(new GUIContent("Confirm","Confirm all the modification of the molecule.")))
			if (GUILayout.Button (new GUIContent ("Open", "Open custom texture image from disk"))) {	
				FileBrowser_show2 = true;
				m_fileBrowser = new ImprovedFileBrowser (
	                new Rect (400, 100, 600, 500),
	                "Choose Image File",
	                FileSelectedCallback,
	                m_last_extSurf_Path
	            );
			}
			
			if (GUILayout.Button (new GUIContent (">>", "Go to next series of textures"))) { // cycle through texture sets 
				texture_set++;
				//Skip fun textures for the article version
				//if(texture_set == 4)
				//	texture_set = 5; 
					
				if (only_best_textures) {
				if(texture_set>0) texture_set = -texture_set;

				if (texture_set > -1) 
					texture_set = besttexSet_min; 
				} else {

				if (texture_set > texSet_max) {
					texture_set = 1;
				} 
				}
			}			
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();			

			// check whether texList has more than 15 entries and raise an error!!
			int i = 0;
			//			GUILayout.EndHorizontal(); GUILayout.Box(texDescr); GUILayout.BeginHorizontal();
			foreach (string texFil in texList) 
			{
				i++; 
				if (i > 5) 
				{
					GUILayout.EndHorizontal (); 
					GUILayout.BeginHorizontal (); 
					i = 1;
				}
//				if(GUILayout.Button((Texture2D)Resources.Load(texDir+texFil),GUILayout.Width(50),GUILayout.Height(50))) 
				if (GUILayout.Button (new GUIContent ((Texture2D)Resources.Load (texDir + texFil), texFil), GUILayout.Width (50), GUILayout.Height (50))) 
				{ 
					if(texFil != "None")
					{
						surface_texture = true;
						external_surface_texture = false;
						surface_texture_name = texDir + texFil;
					}
					else
					{
						surface_texture = false;
						external_surface_texture = false;
					}
				}	
			}
			GUILayout.EndHorizontal ();
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;

		}
		
		private void loadSurfaceTexture (int a)
		{
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
//		private void loadSurfaceTexture(int a)
//		{
//			GUILayout.BeginHorizontal();
//			Texture2D greentext;
//			greentext = (Texture2D)Resources.Load("graypic/green_glass_860");
//			if(GUILayout.Button(greentext,GUILayout.Width(50),GUILayout.Height(50)))
//			{
//				surface_texture =true;
//				surface_texture_name ="graypic/green_glass_860";
//			}	
//			Texture2D toon;
//			toon = (Texture2D)Resources.Load("graypic/toon");
//			if(GUILayout.Button(toon,GUILayout.Width(50),GUILayout.Height(50)))
//			{
//				surface_texture =true;
//				surface_texture_name="graypic/toon";
//			}
//			
//			Texture2D blood;
//			blood = (Texture2D)Resources.Load("graypic/blood");
//			if(GUILayout.Button(blood,GUILayout.Width(50),GUILayout.Height(50)))
//			{
//				surface_texture =true;
//				surface_texture_name="graypic/blood";
//			}
//			
//			Texture2D blue;
//			blue = (Texture2D)Resources.Load("graypic/blue");
//			if(GUILayout.Button(blue,GUILayout.Width(50),GUILayout.Height(50)))
//			{
//				surface_texture =true;
//				surface_texture_name="graypic/blue";
//			}
//			
//			Texture2D draw3;
//			draw3 = (Texture2D)Resources.Load("graypic/draw3");
//			if(GUILayout.Button(draw3,GUILayout.Width(50),GUILayout.Height(50)))
//			{
//				surface_texture =true;
//				surface_texture_name="graypic/draw3";
//			}	
//			GUILayout.EndHorizontal();	
//		}
		
		
		private void loadSurfaceCut (int a)
		{
			depthcut = LabelSlider (depthcut, depthcutMin, depthcutMax, "depth cutting " + depthcut, "Determines cut plane depth position", true, 150); 
			cutX = LabelSlider (cutX, -1f, 1f, " X: " + cutX, "Determines cut plane X position", true, 150);
			cutY = LabelSlider (cutY, -1f, 1f, " Y: " + cutY, "Determines cut plane Y position", true, 150);
			cutZ = LabelSlider (cutZ, -1f, 1f, " Z: " + cutZ, "Determines cut plane Z position", true, 150);
			

		}
		
		private void loadSurfaceMobileCut (int a)
		{
			depthcut = LabelSlider (depthcut, -40f, 40f, "depth cutting " + depthcut, "Determines mobile cut plane depth position", true, 150);
			adjustFieldLinecut = LabelSlider (adjustFieldLinecut, -100f, 100f, " FL cut :" + adjustFieldLinecut, "Determines field line cut position", true, 150);
		}
		
		private  void loadTypeGUIEffect (int a)
		{
			GUILayout.BeginHorizontal ();
			// Make a toggle for SSAO ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			if (UIData.atomtype == UIData.AtomType.hyperball)
				GUI.enabled = false;
			toggle_VE_SSAO = GUILayout.Toggle (toggle_VE_SSAO, new GUIContent ("SSAO", "Toggle screen space ambient occlusion effect"));
			if (!toggle_VE_SSAO) 
			{
				if (Camera.main.GetComponent<SSAOEffect> ().enabled) 
				{ 
					Camera.main.GetComponent<SSAOEffect> ().enabled = false;
				} 
			}
			else 
			{ 
				Camera.main.GetComponent<SSAOEffect> ().enabled = true; 
			}
			GUI.enabled = true;
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
			// Make a toggle for BLUR ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			toggle_VE_BLUR = GUILayout.Toggle (toggle_VE_BLUR, new GUIContent ("BLUR", "Toggle motion blur effect"));
			if (!toggle_VE_BLUR && Camera.main.GetComponent<MotionBlur> ()) {
				Object.Destroy (Camera.main.GetComponent<MotionBlur> ());
			} else if (toggle_VE_BLUR && !Camera.main.GetComponent<MotionBlur> ()) { 
				GameObject.FindWithTag ("MainCamera").AddComponent ("MotionBlur");
				Camera.main.GetComponent<MotionBlur> ().shader = Shader.Find ("Hidden/MotionBlur");
				Camera.main.GetComponent<MotionBlur> ().enabled = true;
			}
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			// Make a toggle for NOISE :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			toggle_VE2_NOISE = GUILayout.Toggle (toggle_VE2_NOISE, new GUIContent ("NOISE", "Toggle noise effect"));
			if (!toggle_VE2_NOISE && Camera.main.GetComponent<NoiseEffect> ()) {
				Object.Destroy (Camera.main.GetComponent<NoiseEffect> ());
			} else if (toggle_VE2_NOISE && !Camera.main.GetComponent<NoiseEffect> ()) { 
				GameObject.FindWithTag ("MainCamera").AddComponent ("NoiseEffect");
				Camera.main.GetComponent<NoiseEffect> ().enabled = true;
				Camera.main.GetComponent<NoiseEffect> ().shaderRGB = Shader.Find ("Hidden/Noise Shader RGB");
				Camera.main.GetComponent<NoiseEffect> ().shaderYUV = Shader.Find ("Hidden/Noise Shader YUV");
				Camera.main.GetComponent<NoiseEffect> ().grainTexture = Resources.Load ("NoiseEffectGrain") as Texture2D;
				Camera.main.GetComponent<NoiseEffect> ().scratchTexture = Resources.Load ("NoiseEffectScratch") as Texture2D;
			}
			// Make a toggle for BLUR2 :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			toggle_VE2_BLUR2 = GUILayout.Toggle (toggle_VE2_BLUR2, new GUIContent ("BLUR2", "Toggle overall blur effect"));
			if (!toggle_VE2_BLUR2 && Camera.main.GetComponent<BlurEffect> ()) {
				Object.Destroy (Camera.main.GetComponent<BlurEffect> ());
			} else if (toggle_VE2_BLUR2 && !Camera.main.GetComponent<BlurEffect> ()) { 
				GameObject.FindWithTag ("MainCamera").AddComponent ("BlurEffect");
				Camera.main.GetComponent<BlurEffect> ().blurShader = Shader.Find ("Hidden/BlurEffectConeTap");
				Camera.main.GetComponent<BlurEffect> ().enabled = true;
			}
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			// Make a toggle for EDGE :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			toggle_VE2_EDGE = GUILayout.Toggle (toggle_VE2_EDGE, new GUIContent ("EDGE", "Toggle edge detection effect"));
			if (!toggle_VE2_EDGE && Camera.main.GetComponent<EdgeDetectEffect> ()) {
				Object.Destroy (Camera.main.GetComponent<EdgeDetectEffect> ());
			} else if (toggle_VE2_EDGE && !Camera.main.GetComponent<EdgeDetectEffect> ()) { 
				GameObject.FindWithTag ("MainCamera").AddComponent ("EdgeDetectEffect");
				Camera.main.GetComponent<EdgeDetectEffect> ().shader = Shader.Find ("Hidden/Edge Detect X");
				Camera.main.GetComponent<EdgeDetectEffect> ().enabled = true;
			}
			// Make a toggle for CONTR :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			toggle_VE2_CONTR = GUILayout.Toggle (toggle_VE2_CONTR, new GUIContent ("CONTR", "Toggle contrast stretch effect"));
			if (!toggle_VE2_CONTR && Camera.main.GetComponent<ContrastStretchEffect> ()) {
				Object.Destroy (Camera.main.GetComponent<ContrastStretchEffect> ());
			} else if (toggle_VE2_CONTR && !Camera.main.GetComponent<ContrastStretchEffect> ()) { 
				GameObject.FindWithTag ("MainCamera").AddComponent ("ContrastStretchEffect");
				Camera.main.GetComponent<ContrastStretchEffect> ().shaderLum = Shader.Find ("Hidden/Contrast Stretch Luminance");
				Camera.main.GetComponent<ContrastStretchEffect> ().shaderReduce = Shader.Find ("Hidden/Contrast Stretch Reduction");
				Camera.main.GetComponent<ContrastStretchEffect> ().shaderAdapt = Shader.Find ("Hidden/Contrast Stretch Adaptation");
				Camera.main.GetComponent<ContrastStretchEffect> ().shaderApply = Shader.Find ("Hidden/Contrast Stretch Apply");
				Camera.main.GetComponent<ContrastStretchEffect> ().enabled = true;
			}
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			// Make a toggle for VORTX :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			toggle_VE2_VORTX = GUILayout.Toggle (toggle_VE2_VORTX, new GUIContent ("VORTX", "Toggle vortex deformation effect"));
			if (!toggle_VE2_VORTX && Camera.main.GetComponent<VortexEffect> ()) {
				Object.Destroy (Camera.main.GetComponent<VortexEffect> ());
			} else if (toggle_VE2_VORTX && !Camera.main.GetComponent<VortexEffect> ()) { 
				GameObject.FindWithTag ("MainCamera").AddComponent ("VortexEffect");
				Camera.main.GetComponent<VortexEffect> ().shader = Shader.Find ("Hidden/Twist Effect");
				Camera.main.GetComponent<VortexEffect> ().enabled = true;
			}
			// Make a toggle for TWIRL :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			toggle_VE2_TWIRL = GUILayout.Toggle (toggle_VE2_TWIRL, new GUIContent ("TWIRL", "Toggle twirl deformation effect"));
			if (!toggle_VE2_TWIRL && Camera.main.GetComponent<TwirlEffect> ()) {
				Object.Destroy (Camera.main.GetComponent<TwirlEffect> ());
			} else if (toggle_VE2_TWIRL && !Camera.main.GetComponent<TwirlEffect> ()) { 
				GameObject.FindWithTag ("MainCamera").AddComponent ("TwirlEffect");
				Camera.main.GetComponent<TwirlEffect> ().shader = Shader.Find ("Hidden/Twirt Effect Shader");
				Camera.main.GetComponent<TwirlEffect> ().enabled = true;
			}
			GUILayout.EndHorizontal ();

			// Make a combined toggle+button for GRAYS ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			GUILayout.BeginHorizontal ();
			toggle_VE2_GRAYS = GUILayout.Toggle (toggle_VE2_GRAYS, new GUIContent ("GRAYS", "Toggle grayscale color effect"));
			if (!toggle_VE2_GRAYS && Camera.main.GetComponent<GrayscaleEffect> ()) {
				Object.Destroy (Camera.main.GetComponent<GrayscaleEffect> ());
			} else if (toggle_VE2_GRAYS && !Camera.main.GetComponent<GrayscaleEffect> ()) { 
				GameObject.FindWithTag ("MainCamera").AddComponent ("GrayscaleEffect");
				Camera.main.GetComponent<GrayscaleEffect> ().enabled = true;
				Camera.main.GetComponent<GrayscaleEffect> ().shader = Shader.Find ("Hidden/Grayscale Effect");
				Camera.main.GetComponent<GrayscaleEffect> ().textureRamp = Resources.Load (ve2_grays_ramps [ve2_grays_rampc]) as Texture2D;
			}
			if (GUILayout.Button (new GUIContent ("Ramp " + ve2_grays_rampc, "Choose among several grayscale ramps"))) {
				ve2_grays_rampc += 1;
				if (ve2_grays_rampc > ve2_grays_rampn) {
					ve2_grays_rampc = 0;
				}
				if (toggle_VE2_GRAYS) {
					Camera.main.GetComponent<GrayscaleEffect> ().textureRamp = Resources.Load (ve2_grays_ramps [ve2_grays_rampc]) as Texture2D;
				}
			}
			GUILayout.EndHorizontal ();

			// Make a combined toggle+button for CCORR ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			GUILayout.BeginHorizontal ();
			toggle_VE2_CCORR = GUILayout.Toggle (toggle_VE2_CCORR, new GUIContent ("CCORR", "Toggle color correction effect"));
			if (!toggle_VE2_CCORR && Camera.main.GetComponent<ColorCorrectionEffect> ()) {
				Object.Destroy (Camera.main.GetComponent<ColorCorrectionEffect> ());
			} else if (toggle_VE2_CCORR && !Camera.main.GetComponent<ColorCorrectionEffect> ()) { 
				GameObject.FindWithTag ("MainCamera").AddComponent ("ColorCorrectionEffect");
				Camera.main.GetComponent<ColorCorrectionEffect> ().enabled = true;
				Camera.main.GetComponent<ColorCorrectionEffect> ().shader = Shader.Find ("Hidden/Grayscale Effect");
				Camera.main.GetComponent<ColorCorrectionEffect> ().textureRamp = Resources.Load (ve2_ccorr_ramps [ve2_ccorr_rampc]) as Texture2D;
			}
			if (GUILayout.Button (new GUIContent ("Ramp " + ve2_ccorr_rampc, "Choose among several color correction ramps"))) {
				ve2_ccorr_rampc += 1;
				if (ve2_ccorr_rampc > ve2_ccorr_rampn) {
					ve2_ccorr_rampc = 0;
				}
				if (toggle_VE2_CCORR) {
					Camera.main.GetComponent<ColorCorrectionEffect> ().textureRamp = Resources.Load (ve2_ccorr_ramps [ve2_ccorr_rampc]) as Texture2D;
				}
			}
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			// Make a toggle for SEPIA :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			toggle_VE2_SEPIA = GUILayout.Toggle (toggle_VE2_SEPIA, new GUIContent ("SEPIA", "Toggle sepia tone color effect"));
			if (!toggle_VE2_SEPIA && Camera.main.GetComponent<SepiaToneEffect> ()) {
				Object.Destroy (Camera.main.GetComponent<SepiaToneEffect> ());
			} else if (toggle_VE2_SEPIA && !Camera.main.GetComponent<SepiaToneEffect> ()) { 
				GameObject.FindWithTag ("MainCamera").AddComponent ("SepiaToneEffect");
				Camera.main.GetComponent<SepiaToneEffect> ().shader = Shader.Find ("Hidden/Sepiatone Effect");
				Camera.main.GetComponent<SepiaToneEffect> ().enabled = true;
			}
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			// Make a toggle for GLOW :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			toggle_VE2_GLOW = GUILayout.Toggle (toggle_VE2_GLOW, new GUIContent ("GLOW", "Toggle glow effect"));
			if (!toggle_VE2_GLOW && Camera.main.GetComponent<GlowEffect> ()) {
				Object.Destroy (Camera.main.GetComponent<GlowEffect> ());
			} else if (toggle_VE2_GLOW && !Camera.main.GetComponent<GlowEffect> ()) { 
				GameObject.FindWithTag ("MainCamera").AddComponent ("GlowEffect");
				Camera.main.GetComponent<GlowEffect> ().compositeShader = Shader.Find ("Hidden/GlowCompose");
				Camera.main.GetComponent<GlowEffect> ().blurShader = Shader.Find ("Hidden/GlowConeTap");
				Camera.main.GetComponent<GlowEffect> ().downsampleShader = Shader.Find ("Hidden/Glow Downsample");
				//			Camera.main.GetComponent<GlowEffect>().blurspread = 0.7f;
				//			Camera.main.GetComponent<GlowEffect>().bluriterations = 3f;
				Camera.main.GetComponent<GlowEffect> ().glowIntensity = 0.3f;
				Camera.main.GetComponent<GlowEffect> ().enabled = true;
			}
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			// Make a toggle for DREAM :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			toggle_VE2_DREAM = GUILayout.Toggle (toggle_VE2_DREAM, new GUIContent ("DREAM", "Toggle a dream-like effect"));
			
			
//			if (!toggle_VE2_DREAM && Camera.main.GetComponent<DreamEffect> ()) {
//				Object.Destroy (Camera.main.GetComponent<DreamEffect> ());
//			} else if (toggle_VE2_DREAM && !Camera.main.GetComponent<DreamEffect> ()) { 
//				GameObject.FindWithTag ("MainCamera").AddComponent ("DreamEffect");
//				Camera.main.GetComponent<DreamEffect> ().blackChannelShader = Shader.Find ("Hidden/DreamEffect BlackChannel");
//				Camera.main.GetComponent<DreamEffect> ().enabled = true;
//			}
			GUILayout.EndHorizontal ();
			backgroundtype_show = false;
			graycolor_show = false;
			//			ParamFieldLine_show=false;
			SurfaceButton_show = false;
			

			//			GUI.Label ( new Rect(120,Screen.height-35,Screen.width-360,20), GUI.tooltip);
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
		}
			

		
// help window at the start of program =============================================================================================		
		public void RenderHelp ()
		{
			if (toggle_HELP) {
				if (!toggle_VE_COPYR) {
					toggle_VE_COPYR = true;
				}
				GUI.Box (new Rect (Screen.width / 4, Screen.height / 4 + 30, Screen.width / 2, Screen.height / 2 - 60), "UNITY MOL QUICK HELP");
				GUILayout.BeginArea (new Rect (5 + Screen.width / 4, 25 + Screen.height / 4 + 30, 0.5f * Screen.width - 10, 0.5f * Screen.height - 25));
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
				// MB: these keys don't seem to work anuy more?? check:
				//						GUILayout.Label("Use the cursor keys to navigate the scene: W/X for rotation D/E/Q/S/Z for pan, and B/N for zoom if click the key choice. You can also use the Mouse button to navigate if click the mouse choice. Alternatively,if you have a joypad you can rotation and zoom and pan by the button of the joypad, and also the radius of the atoms");
				GUILayout.Label ("You can press the delete key to activate some hidden features on the left-hand atoms menu (bugs included!).");
				// MB: the equal key does not seem to work?		
				//GUILayout.Label("If you press the equal key, the octree dividing blocks for the particle system are shown.");
				GUILayout.Label ("\nUnity Mol is an open source project by Marc Baaden and the LBT team. version "+umolversion+" (c) 2009-13.");
				GUILayout.EndArea ();
				//			Debug.Log("Hlep information window");
				if (GUI.Button (new Rect (Screen.width / 4, 0.75f * Screen.height - 20 - 30, Screen.width / 2, 20), "Close unity mol help window")) { 
					toggle_HELP = false; 
				}
			}

			// Print copyright and icon if activated :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			if (toggle_VE_COPYR) {
//				GUI.Box (new Rect (Screen.width - 700, Screen.height - 25, 450, 25), "(c)opyright 2009-13 by LBT team/M. Baaden, <baaden@smplinux.de>");
				GUI.Box (new Rect (180, Screen.height - 25, 500, 25), umolversion+" (c)opyright 2009-13 by LBT team/M. Baaden, <baaden@smplinux.de>");
				// real logo size: 87 x 263 // 175 x 58
//							if (GUI.Button ( new Rect(0,Screen.height-58, 175, 58), new GUIContent(guicon,"Open the Unity Mol manual in a web browser"), GUI.skin.GetStyle("label"))) 
				if (GUI.Button (new Rect (0, Screen.height - 58, 175, 58), new GUIContent (guicon, "Open the Unity Mol manual in a web browser"))) {
//								Application.OpenURL (umolbase+"/Assets/_Documentation/Manual.html");
//					Application.OpenURL (umolbase + "/umolfigs2.png");
					Application.OpenURL (umolbase + "/page2/index.html");
				}
			}
			#if !UNITY_WEBPLAYER 
			{
				if (GUI.Button (new Rect (principalxstart + principalwidth + 25, principalystart, 40, 20), new GUIContent ("EXIT", "APPLICATION: Exit the Unity Mol program!"))) {
					Application.Quit ();
				}
			}
			#endif
			if (GUI.Button (new Rect (principalxstart + principalwidth + 5, principalystart, 20, 20), new GUIContent ("?", "APPLICATION: Open quick help for the Unity Mol program!"))) {
				if (toggle_HELP) {
					toggle_HELP = false;
				} else {
					toggle_HELP = true;
				}
				;

			}

//		GUI.Label ( new Rect(120,Screen.height-35,Screen.width-360,20), GUI.tooltip);
		}

		private void RenderParameter ()
		{
			//		toggle_HB_SANIM = GUILayout.Toggle (toggle_HB_SANIM, new GUIContent ("HB_SANIM", "Animate shrink parameter"));
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("Color", "Choose the color for each atom type")))
			if (!SetAtomScale_show) {
				SetAtomScale_show = true;
				// m_colorpick_Surface = null;
				// m_colorpick_fieldline = null;
				// surface_texture_show = false;
			} else 
				SetAtomScale_show = false;
			
			if (UIData.atomtype == UIData.AtomType.hyperball)
				GUI.enabled = true;
			else 
				GUI.enabled = false;
				
			if (GUILayout.Button (new GUIContent ("Texture", "Change the texture used in the HyperBalls matcap shader"))) {
				if (GUIDisplay.m_texture == false) {
					GUIDisplay.m_texture = true;
					surface_texture_show = false;
					// GUIDisplay.m_colorpick_Atom = null;
						
				} else
					GUIDisplay.m_texture = false;
			}
			GUILayout.EndHorizontal ();
			
			bool enable = true;
			
			rayon = LabelSlider (rayon, 0.00001f, 2.0f, "Radius  " + rayon, "Determines radius value", enable, 150);
			//			rayon = LabelSlider (rayon, 0.00001f, 2.0f, new GUIContent("Radius  "+rayon,"Choose the atom radius"),enable,150);
				
			//		if(!UIData.openAllMenu)
			//		{
			//			if(UIData.atomtype==UIData.AtomType.particleball)enable=false;
			//		}
			//		
			//		shrink = LabelSlider (shrink, 0.00001f,  0.99f, "Shrink  "+shrink,enable);
			//
			//
			////		toggle_HB_RANIM = GUILayout.Toggle (toggle_HB_RANIM, new GUIContent ("HB_RANIM", "Animate radius parameter"));
			//
			//		linkscale = LabelSlider(linkscale, 0.00001f,  1.0f, "Scale "+linkscale,enable);
			//		
			//		depthfactor = LabelSlider(depthfactor, -2.0f,  2.0f, "Depthfactor "+depthfactor,enable);
			//	
			//		MetaphorControl();
			//
			//		GUI.enabled=true;
			//	
			////		toggle_HB_TRANS = GUILayout.Toggle (toggle_HB_TRANS, new GUIContent ("HB_TRANS", "Animate CPK/Licorice"));
			//		//Make a toggle for interactive mode
			//		if(UIData.atomtype!=UIData.AtomType.hyperball)GUI.enabled=false;
			//		toggle_NA_INTERACTIVE = GUILayout.Toggle (toggle_NA_INTERACTIVE, new GUIContent ("Interactive mode", "Toggle interactive mode, the physical engine will be open."));
			//		//toggle_NA_INTERACTIVE=false;
			//		GUI.enabled=true;
			//		toggle_NA_SWITCH = GUILayout.Toggle (toggle_NA_SWITCH, new GUIContent ("Switch mode", "Toggle switch mode, which is swith representation method between hyperball and particleball due to the static or moving."));
			//		
			//		toggle_NA_CLICK = GUILayout.Toggle (toggle_NA_CLICK, new GUIContent ("Click Mode", "Toggle Click Mode, and then click the atom, the information will be shown."));
			//
			//		toggle_NA_MEASURE = GUILayout.Toggle (toggle_NA_MEASURE, new GUIContent ("Measure distance", "Toggle Mesaure Distance between two atoms."));
			

			// 		if(!toggle_NA_MEASURE && Camera.main.GetComponent<MeasureDistance>()) 
			// 		{
			// //			Object.Destroy (Camera.main.GetComponent<MeasureDistance>());
			// 			Camera.main.GetComponent<MeasureDistance>().Clear();
			// 			Camera.main.GetComponent<MeasureDistance>().enabled = false;
			// //			GameObject line=GameObject.Find("Line");
			// ////      		Destroy(line);
			// //			line.active = false;

			// 		} 
			// 		else if (toggle_NA_MEASURE && Camera.main.GetComponent<MeasureDistance>()) 
			// 		{ 
			// 			//GameObject.FindWithTag("MainCamera").AddComponent ("MeasureDistance");
			// 			Camera.main.GetComponent<MeasureDistance>().enabled = true;
			// 		}
			// 		//GUILayout.EndHorizontal();
			
			// 		if(!toggle_NA_CLICK && Camera.main.GetComponent<ClickAtom>()) 
			// 		{
			// //			Object.Destroy (Camera.main.GetComponent<MeasureDistance>());

			// 			Camera.main.GetComponent<ClickAtom>().Clear();
			// 			Camera.main.GetComponent<ClickAtom>().enabled = false;
			// //			GameObject line=GameObject.Find("Line");
			// ////      		Destroy(line);
			// //			line.active = false;

			// 		} 
			// 		else if (toggle_NA_CLICK && Camera.main.GetComponent<ClickAtom>()) 
			// 		{ 
			// 			//GameObject.FindWithTag("MainCamera").AddComponent ("MeasureDistance");
			// 			Camera.main.GetComponent<ClickAtom>().enabled = true;
			// 			Debug.Log("RenderParameter");
			// 		}

			
			// 		if(toggle_NA_SWITCH)
			// 		{
			// 			UIData.switchmode=true;
			// 		}
			// 		else
			// 		{
			// 			UIData.switchmode=false;
			// 		}
			
			// 		if(toggle_NA_INTERACTIVE)
			// 		{
			// 			UIData.interactive=true;
			// 			UIData.resetInteractive=true;
			// 		}
			// 		else
			// 		{
			// 			UIData.toggleGray=false;
			// 			UIData.toggleColor=true;
			// 			UIData.interactive=false;
			// 			UIData.resetInteractive=true;
			// 			if(!MoleculeModel.FieldLineFileExist)graycolor_show=false;
			// 			if(!MoleculeModel.SurfaceFileExist) SurfaceButton_show= false;
			// 		}
			

			//////////////////////modify///////////////////////
			// toggle_NA_MAXCAM = GUILayout.Toggle (toggle_NA_MAXCAM, new GUIContent ("MAXCAM", "Toggle 3DS max-like scene navigation"));

			GUILayout.BeginHorizontal();
			if(toggle_NA_HIDE)
				GUI.enabled = false;
			toggle_NA_HBALLSMOOTH = GUILayout.Toggle (toggle_NA_HBALLSMOOTH, new GUIContent ("HballSmoothL", "Set a parameter combo for HyperBalls and Sticks with SmoothLinks once"));
			UIData.hballsmoothmode = toggle_NA_HBALLSMOOTH;
			GUI.enabled = true;
			toggle_NA_HIDE = GUILayout.Toggle (toggle_NA_HIDE, new GUIContent ("Hide", "Hide/Display atoms"));
			GUILayout.EndHorizontal();
			
			if (toggle_NA_HBALLSMOOTH) {
				UIData.resetDisplay = true;
				UIData.isCubeToSphere = false;
				UIData.isSphereToCube = true;
				UIData.atomtype = UIData.AtomType.hyperball;
				Debug.Log ("UIData.resetDisplay :: " + UIData.resetDisplay);
				Debug.Log ("UIData.isCubeToSphere :: " + UIData.isCubeToSphere);
				Debug.Log ("UIData.isSphereToCube :: " + UIData.isSphereToCube);
				atomtype_show = false;
				
				UIData.resetBondDisplay = true;
				UIData.bondtype = UIData.BondType.hyperstick;
				bondtype_show = false;
				
				rayon = 0.4f;
				shrink = 0.5f;
				linkscale = 1.0f;
				
				toggle_NA_HBALLSMOOTH = false;
				UIData.hballsmoothmode = false;
			}

			// LOD Mode
			// if(UIData.atomtype != UIData.AtomType.hyperball)
			// {
			// 	toggle_NA_SWITCH = false;
			// 	GUI.enabled = false;
			// }
			GUILayout.BeginHorizontal();
			if(toggle_NA_HIDE || UIData.atomtype == UIData.AtomType.particleball)
				GUI.enabled = false;
			toggle_NA_SWITCH = GUILayout.Toggle (toggle_NA_SWITCH, new GUIContent ("LOD mode", "Toggle LOD mode between hyperball and particles"));
			UIData.switchmode = toggle_NA_SWITCH;
			GUI.enabled = true;
			if(toggle_NA_HIDE)
				GUI.enabled = false;
			toggle_NA_AUTOMOVE = GUILayout.Toggle (toggle_NA_AUTOMOVE, new GUIContent ("Automove", "Camera auto rotation"));
			GUI.enabled = true;
			GUILayout.EndHorizontal();
			
			
			//////////////////////modify///////////////////////
			
//MB			bool enable2=true;
			
			//		if(!(UIData.interactive&&(UIData.atomtype==UIData.AtomType.hyperball)))enable2=false;
			//
			//		drag = LabelSlider (drag, 0.00001f,  5f, "Drag  "+drag,enable2);
			//		
			//		spring = LabelSlider (spring, 0.00001f,  20, "Spring  "+spring,enable2);
			

			/////////////////////////////////////////////////////////
			//		GUI.enabled=true;
			//		
			//		if(!(UIData.interactive||MoleculeModel.FieldLineFileExist))GUI.enabled=false;
				
			// pas util ici 		// pas util ici 		// pas util ici 		// pas util ici 		// pas util ici 		// pas util ici 		// pas util ici 		// pas util ici 		// pas util ici 
			// pas util ici 		// pas util ici 		// pas util ici 		// pas util ici 		// pas util ici 		// pas util ici 		// pas util ici 		// pas util ici 		// pas util ici 		// pas 
				
				
			//		if(GUILayout.Button(new GUIContent ("Field Lines","Field Lines Parameters")))	// boutton show the filedline
			//		{
			//			if(ParamFieldLine_show){
			//					ParamFieldLine_show=false;
			//					graycolor_show=false;
			//				}
			//			else {
			//				ParamFieldLine_show=true;
			//				SurfaceButton_show=false;
			//				m_colorpick_Surface= false;
			//				}	
			//		}	
			//		GUI.enabled=true;
			//		if(!(UIData.interactive||MoleculeModel.SurfaceFileExist))GUI.enabled=false; // active just when have a obj file or not
			//
			//		if(GUILayout.Button(new GUIContent ("Surface","Surface Parameters")))	// boutton show the filedline
			//		{
			//			
			//			if(SurfaceButton_show){
			//					SurfaceButton_show=false;
			//					m_colorpick_Surface= false;
			//					SurfaceCut_show=false;
			//					SurfaceMobileCut_show =false;
			//					surface_texture_show = false;
			//
			//				}
			//			else {SurfaceButton_show=true;
			//					ParamFieldLine_show=false;
			//					graycolor_show=false;
			//					
			//				}
			//		}	
			GUI.enabled = true;
			////////////////////////
			//				GUI.Label ( new Rect(120,Screen.height-35,Screen.width-360,20), GUI.tooltip);
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;

		}

		private void loadTypeGUIhyperball (int a)
		{

			shrink = LabelSlider (shrink, 0.00001f, 0.99f, "Shrink  " + shrink, "Determines shrink factor parameter for HyperBalls", true, 150);


			//		toggle_HB_RANIM = GUILayout.Toggle (toggle_HB_RANIM, new GUIContent ("HB_RANIM", "Animate radius parameter"));

			linkscale = LabelSlider (linkscale, 0.00001f, 1.0f, "Scale " + linkscale, "Determines scale parameter", true, 150);
			
			depthfactor = LabelSlider (depthfactor, -2.0f, 2.0f, "Depthfactor " + depthfactor, "Determines depth factor for network visualization", true, 150);
		
			MetaphorControl ();

			GUI.enabled = true;
		
			//		toggle_HB_TRANS = GUILayout.Toggle (toggle_HB_TRANS, new GUIContent ("HB_TRANS", "Animate CPK/Licorice"));
			//Make a toggle for interactive mode
			//if(UIData.atomtype!=UIData.AtomType.hyperball)GUI.enabled=false;
			toggle_NA_INTERACTIVE = GUILayout.Toggle (toggle_NA_INTERACTIVE, new GUIContent ("Interactive mode", "Toggle interactive mode, the physics engine will be used"));
			drag = LabelSlider (drag, 0.00001f, 5f, "Drag  " + drag, "Determines PhysX engine drag value", UIData.interactive, 150);
			spring = LabelSlider (spring, 0.00001f, 20, "Spring  " + spring, "Determines PhysX engine spring constant", UIData.interactive, 150);
			PhysicalChoice();
			//toggle_NA_INTERACTIVE=false;
			GUI.enabled = true;	
			toggle_NA_CLICK = GUILayout.Toggle (toggle_NA_CLICK, new GUIContent ("Atom info", "Toggles mouse clicking on an atom to show related information"));

			toggle_NA_MEASURE = GUILayout.Toggle (toggle_NA_MEASURE, new GUIContent ("Measure distance", "Toggle mouse clicking to measure the distance between two atoms"));

			//Alex :
			//TODO : LOD mode buggy with other representation than Hyperball

			GUI.enabled = true;
			if (!toggle_NA_MEASURE && Camera.main.GetComponent<MeasureDistance> ()) {
				// Camera.main.GetComponent<MeasureDistance>().Clear();
				Camera.main.GetComponent<MeasureDistance> ().enabled = false;
			} else if (toggle_NA_MEASURE && Camera.main.GetComponent<MeasureDistance> ()) { 
				//GameObject.FindWithTag("MainCamera").AddComponent ("MeasureDistance");
				Camera.main.GetComponent<MeasureDistance> ().enabled = true;
			}
			//GUILayout.EndHorizontal();
			
			if (!toggle_NA_CLICK && Camera.main.GetComponent<ClickAtom> ()) {
				//			Object.Destroy (Camera.main.GetComponent<MeasureDistance>());

				//Camera.main.GetComponent<ClickAtom>().Clear();
				Camera.main.GetComponent<ClickAtom> ().enabled = false;
				//			GameObject line=GameObject.Find("Line");
				////      		Destroy(line);
				//			line.active = false;

			} else if (toggle_NA_CLICK && Camera.main.GetComponent<ClickAtom> ()) { 
				//GameObject.FindWithTag("MainCamera").AddComponent ("MeasureDistance");
				Camera.main.GetComponent<ClickAtom> ().enabled = true;
			}

			

			

			
			//////////////////////modify///////////////////////

			
			if (toggle_NA_INTERACTIVE) {
				UIData.interactive = true;
				UIData.resetInteractive = true;
			} else {
				UIData.toggleGray = false;
				UIData.toggleColor = true;
				UIData.interactive = false;
				UIData.resetInteractive = true;
				if (!MoleculeModel.FieldLineFileExist)
					graycolor_show = false;
				if (!MoleculeModel.SurfaceFileExist)
					SurfaceButton_show = false;
			}
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;

		}
	}
}
