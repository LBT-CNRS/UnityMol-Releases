/// @file LoadTypeGUI.cs
/// @brief This static class contains a collection of functions that define GUI windows.
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
/// $Id: LoadTypeGUI.cs 213 2013-04-06 21:13:42Z baaden $
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


namespace UI{
	using UnityEngine;
	using System.Collections;
	using Molecule.Model;
	using Molecule.View;
	using Reorient;
	using OptimalView;
	
	// Can't be static because GUIMoleculeController isn't, thought it probably ought to be.
	public class LoadTypeGUI : GUIMoleculeController{
		
		// Size values for the orthographic camera
		public static float minOrthoSize = 1f ;
		public static float maxOrthoSize = 60f ;
		public static float orthoSize = 10f; // size of the orthographic camera

		//Parameters for SugarRibbons
		public static float RibbonsThickness = 0.15f;
		public static float OxySphereSize = 1f;
		public static float OxySphereSizeCheck = 1f;
		public static float thickness_Little=1.8f;
		public static float thickness_BIG=1f;
		public static float thickness_bond_6_C1_C4=0.2f; 
		public static float thickness_6_other=0.16f; 
		public static float thickness_bond_5=0.2f;
		public static float lighter_color_factor_bond=0.35f;
		public static float lighter_color_factor_bond_check=0.35f;
		public static float lighter_color_factor_ring=0.35f;
		public static float lighter_color_factor_ring_check=0.35f;
		public static int ColorationModeRing=0;
		public static int ColorationModeBond=0;

		//definition of sugarRibons and RingBlending, to avoid create them to each frame
		public static SugarRibbons SR;
		public static RingBlending ringblending;
		
		// Colors for buttons in secondary structure menu
		// Note : LoadTypeGUI and GUIDisplay are separated randomly. Don't know why some UI features are in one or another. Here, it forces us to set this public.
		// TODO : Fusion LoadTypeGUI and GUIDisplay ? (and GUIMoleculeController ???) or at least do some cleaning in those
		public static Color[] helixButtonNew = new Color[200];
		public static Texture2D helixButton = new Texture2D(20,10,TextureFormat.ARGB32,false);
		public static Color[] sheetButtonNew = new Color[200];
		public static Texture2D sheetButton = new Texture2D(20,10,TextureFormat.ARGB32,false);
		public static Color[] coilButtonNew = new Color[200];
		public static Texture2D coilButton = new Texture2D(20,10,TextureFormat.ARGB32,false);

		public static Texture2D chainbuttonA = new Texture2D(20,10,TextureFormat.ARGB32,false);
		public static Color[] chainbuttonAnew = new Color[200];
		public static Texture2D chainbuttonB = new Texture2D(20,10,TextureFormat.ARGB32,false);
		public static Color[] chainbuttonBnew = new Color[200];
		public static Texture2D chainbuttonC = new Texture2D(20,10,TextureFormat.ARGB32,false);
		public static Color[] chainbuttonCnew = new Color[200];
		public static Texture2D chainbuttonD = new Texture2D(20,10,TextureFormat.ARGB32,false);
		public static Color[] chainbuttonDnew = new Color[200];
		public static Texture2D chainbuttonE = new Texture2D(20,10,TextureFormat.ARGB32,false);
		public static Color[] chainbuttonEnew = new Color[200];
		
		// TODO : Make C-Alpha trace and his color/texture work again
/*		// Textures for buttons in AdvOptions 
		private static Texture2D chainATex;
		private static Texture2D chainBTex;
		private static Texture2D chainCTex;
		private static Texture2D chainDTex;
		private static Texture2D chainETex;
		private static Texture2D chainFTex;
		private static Texture2D chainGTex;
		private static Texture2D chainHTex;
		
		// Colors for the chains
		private static ColorObject chainAColor = new ColorObject(Color.white);
		private static ColorObject chainBColor = new ColorObject(Color.white);
		private static ColorObject chainCColor = new ColorObject(Color.white);
		private static ColorObject chainDColor = new ColorObject(Color.white);
		private static ColorObject chainEColor = new ColorObject(Color.white);
		private static ColorObject chainFColor = new ColorObject(Color.white);
		private static ColorObject chainGColor = new ColorObject(Color.white);
		private static ColorObject chainHColor = new ColorObject(Color.white);
		
		
		// Color arrays for the button Textures
		private static Color[] chainAColors = new Color[200];
		private static Color[] chainBColors = new Color[200];
		private static Color[] chainCColors = new Color[200];
		private static Color[] chainDColors = new Color[200];
		private static Color[] chainEColors = new Color[200];
		private static Color[] chainFColors = new Color[200];
		private static Color[] chainGColors = new Color[200];
		private static Color[] chainHColors = new Color[200];
		
		// Set to true when the chainXColors arrays have been initalized
		private static bool chainColorsInit = false ;
*/
		
		
		public static string SymmetryOriginX = "34.3444";
		public static string SymmetryOriginY = "4.29016";
		public static string SymmetryOriginZ = "69.0832";
		public static string SymmetryDirectionX = "0.446105";
		public static string SymmetryDirectionY = "0.00135695";
		public static string SymmetryDirectionZ = "-0.894949";
		public static string TargetX = "16.32";
		public static string TargetY = "-1.42";
		public static string TargetZ = "-18.17";
		public static string CameraDistance = "20.0";
		
		public static bool showOriginAxe = true;
		public static bool originThere = true;
		/// <summary>
		/// Sets the title of the current window.
		/// The FlexibleSpace() function around the Label is here for centering.
		/// </summary>
		/// <param name='s'>
		/// The title you wish to set for the window. String.
		/// </param>
		public static void SetTitle(string s) {
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label(s);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		
		
		/// <summary>
		/// This is a somewhat odd but very convenient function. When called by a function that defines
		/// the contents of a window, it creates a loosely centered label from its string argument,
		/// that basically acts as a title.
		/// It also creates a little 'close' button aligned to the right, and returns false if it was
		/// pressed, true otherwise.
		/// Therefore, the function both creates a title and determines whether the window must be
		/// kept open.
		/// </summary>
		/// <returns>
		/// A boolean value set to false if the 'close' button was pressed.
		/// </returns>
		/// <param name='s'>
		/// A string, the title to set for the current window.
		/// </param>
		public static bool SetTitleExit(string s) {
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			
			bool keepOpen = true ;
			
			GUILayout.Label(s);
			GUILayout.FlexibleSpace();
			if(GUILayout.Button(new GUIContent("X", "Close window")))
				keepOpen = false;
			
			GUILayout.EndHorizontal();
			
			return keepOpen;
		}
		
		
		
		/// <summary>
		/// Defines the style of atoms.
		/// </summary>
		/// <param name='a'>
		/// Window identifier.
		/// </param>
		public static void AtomStyle (int a) {
			SetTitle("Choose Atom Style");
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button (new GUIContent ("Cube", "Use cubes to represent atoms"))) {	
				
				Debug.Log("Cube representation");
				UIData.resetDisplay = true;
				UIData.isCubeToSphere = false;
				UIData.isSphereToCube = true;
				UIData.atomtype = UIData.AtomType.cube;
				Debug.Log ("UIData.resetDisplay:" + UIData.resetDisplay);
				Debug.Log ("UIData.isCubeToSphere:" + UIData.isCubeToSphere);
				Debug.Log ("UIData.isSphereToCube:" + UIData.isSphereToCube);
				showAtomType = false;
				toggle_NA_HIDE = false; // whether molecule must be hidden (I think)
				toggle_NA_CLICK = false;
				BallUpdate.resetColors = true;
			}
			
			if (GUILayout.Button (new GUIContent ("Sphere", "Use triangulated spheres to represent atoms"))) {
				
				Debug.Log("Sphere representation");
				UIData.resetDisplay = true;
				UIData.isCubeToSphere = true;
				UIData.isSphereToCube = false;
				UIData.atomtype = UIData.AtomType.sphere;
				Debug.Log ("UIData.resetDisplay:" + UIData.resetDisplay);
				Debug.Log ("UIData.isCubeToSphere:" + UIData.isCubeToSphere);
				Debug.Log ("UIData.isSphereToCube:" + UIData.isSphereToCube);
				showAtomType = false;
				toggle_NA_HIDE = false;
				toggle_NA_CLICK = false;
				BallUpdate.resetColors = true;
			}
			GUILayout.EndHorizontal ();
			
			
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("Hyperball", "Use the HyperBalls shader to render atoms"))) {
				
				Debug.Log("HyperBall representation");
				UIData.resetDisplay = true;
				UIData.isCubeToSphere = false;
				UIData.isSphereToCube = true;
				UIData.atomtype = UIData.AtomType.hyperball;
				Debug.Log ("UIData.resetDisplay:" + UIData.resetDisplay);
				Debug.Log ("UIData.isCubeToSphere:" + UIData.isCubeToSphere);
				Debug.Log ("UIData.isSphereToCube:" + UIData.isSphereToCube);
				showAtomType = false;
				toggle_NA_HIDE = false;
				toggle_NA_CLICK = false;
				BallUpdate.resetColors = true;
			}
			
	
			if (GUILayout.Button (new GUIContent ("Particle", "Use the ParticleBall shader to represent atoms"))) {
				
				Debug.Log("Particle representation");
				UIData.resetDisplay = true;
				UIData.isSphereToCube = false;
				UIData.isCubeToSphere = false;
				UIData.atomtype = UIData.AtomType.particleball;
				Debug.Log ("UIData.resetDisplay:" + UIData.resetDisplay);
				Debug.Log ("UIData.isCubeToSphere:" + UIData.isCubeToSphere);
				Debug.Log ("UIData.isSphereToCube:" + UIData.isSphereToCube);
				UIData.resetBondDisplay = true;
				UIData.bondtype=UIData.BondType.nobond; // Probably best to do this by default. Users can still enable bonds if they wish.
				showAtomType = false;
				toggle_NA_HIDE = false;
				toggle_NA_SWITCH = false;
				toggle_NA_CLICK = false;
				GameObject shurikenParticleManagerObj = GameObject.FindGameObjectWithTag("ShurikenParticleManager");
				ShurikenParticleManager shManager = shurikenParticleManagerObj.GetComponent<ShurikenParticleManager>();
//				shManager.Init();
				shManager.EnableRenderers();
			}						
			GUILayout.EndHorizontal ();
	
			
			// Those hidden features aren't working at all
/*			if (UIData.openAllMenu) { 
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button (new GUIContent ("Raycasting", "Use raycasting to represent atoms"))) {
					UIData.resetDisplay = true;
					UIData.isCubeToSphere = false;
					UIData.isSphereToCube = true;
					UIData.atomtype = UIData.AtomType.raycasting;
					Debug.Log ("UIData.resetDisplay:" + UIData.resetDisplay);
					Debug.Log ("UIData.isCubeToSphere:" + UIData.isCubeToSphere);
					Debug.Log ("UIData.isSphereToCube:" + UIData.isSphereToCube);
					showAtomType = false;
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
					showAtomType = false;
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
					showAtomType = false;
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
					showAtomType = false;
					toggle_NA_HIDE = false;
				}						
				GUILayout.EndHorizontal ();
			}
			
			if (UIData.openAllMenu) {			
			
				GUILayout.BeginHorizontal ();
				
				GUILayout.Label ("Billboard", GUILayout.MaxWidth (50));
				
				UIData.toggleClip = GUILayout.Toggle (UIData.toggleClip, new GUIContent ("Clip", "Toggle the Clip plane"));
				UIData.togglePlane = !UIData.toggleClip;
				
				UIData.togglePlane = GUILayout.Toggle (UIData.togglePlane, new GUIContent ("Plane", "Toggle the Cut plane"));
				UIData.toggleClip = !UIData.togglePlane;
				
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
*/
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
			
			GUI.DragWindow();
		} // end of AtomStyle
		
		
		
		//Sugar Menu
		public static void SugarM (int a){
			showSugarChainMenu = SetTitleExit("Sugar");
			bool ssToggled = toggle_RING_BLENDING;

			/*************************************************/
			GUILayout.BeginHorizontal();
			toggle_RING_BLENDING = UIData.hasMoleculeDisplay && GUILayout.Toggle(toggle_RING_BLENDING,
				new GUIContent("Enable RingBlending", "enable RingBlending visualisation"));
			
			if(!ssToggled && toggle_RING_BLENDING) { // enabling the SugarBlending
				ringblending = new RingBlending();
				ringblending.CreateRingBlending();
			}else {
				if (ssToggled && !toggle_RING_BLENDING) { // destroying the SugarBlending
					GameObject[] blendObjs = GameObject.FindGameObjectsWithTag("RingBlending");
					foreach(GameObject blendobj in blendObjs)
						GameObject.Destroy(blendobj);
				}
			}			
			GUILayout.EndHorizontal();
			/*************************************************/

			//------- Twister
			bool twToggled = toggle_TWISTER;
			/*************************************************/
			GUILayout.BeginHorizontal();
			toggle_TWISTER = UIData.hasMoleculeDisplay && GUILayout.Toggle(toggle_TWISTER,
			                                                                  new GUIContent("Enable SugarRibbons", "Switch between all-atoms and SugarRibbons representation"));


			if(!twToggled && toggle_TWISTER) { // enabling the ribbons
				SR = new SugarRibbons(toggle_SUGAR_ONLY);
				//Twister twisters = new Twister();
				//twisters.CreateTwister();
				SR.createSugarRibs(RibbonsThickness, toggle_SUGAR_ONLY, thickness_Little, thickness_BIG, 
				                   thickness_bond_6_C1_C4, thickness_6_other, thickness_bond_5, lighter_color_factor_ring, lighter_color_factor_bond,
				                   ColorationModeRing, ColorationModeBond, BondColor, RingColor, OxySphereSize,OxySphereColor);
				toggle_NA_HIDE = !toggle_NA_HIDE;
				toggle_SHOW_HB_NOT_SUGAR = false;
				toggle_SHOW_HB_W_SR = false;
				toggle_HIDE_HYDROGEN = false;

				//Initialize bond & ring color to an "empty" color.
				BondColorcheck.color=Color.white;
				BondColor.color=Color.white;
				RingColorcheck.color = Color.white;
				RingColor.color = Color.white;
				OxySphereColorCheck.color = Color.red;

			} else if (twToggled && !toggle_TWISTER) { // destroying the ribbons
					toggle_NA_HIDE = !toggle_NA_HIDE;
					GameObject [] SugarRibbons;
					SugarRibbons = GameObject.FindGameObjectsWithTag("SugarRibbons_RING_BIG");
					foreach (GameObject SugarRibbon in SugarRibbons)
						Object.Destroy(SugarRibbon);
					SugarRibbons = GameObject.FindGameObjectsWithTag("SugarRibbons_RING_little");
					foreach (GameObject SugarRibbon in SugarRibbons)
						Object.Destroy(SugarRibbon);
					SugarRibbons = GameObject.FindGameObjectsWithTag("SugarRibbons_BOND");
					foreach (GameObject SugarRibbon in SugarRibbons)
						Object.Destroy(SugarRibbon);
				}
					
			GUILayout.EndHorizontal();
			/*************************************************/
			/*************************************************/
			GUILayout.BeginHorizontal();
			bool hydroToggled = toggle_HIDE_HYDROGEN;
			toggle_HIDE_HYDROGEN = UIData.hasMoleculeDisplay && GUILayout.Toggle(toggle_HIDE_HYDROGEN,
			                                                                     new GUIContent("Hide Hydrogens", "hide hydrogens atoms"));
			if(!hydroToggled && toggle_HIDE_HYDROGEN)
				showHydrogens(false);
			else if (hydroToggled && !toggle_HIDE_HYDROGEN)
				showHydrogens(true);
			GUILayout.EndHorizontal();
			/*************************************************/
			GUILayout.BeginHorizontal();
			GUILayout.Label(">> Hiding atoms");
			GUILayout.EndHorizontal();
			/*************************************************/

			GUILayout.BeginHorizontal();
			bool hb_w_sb_toggled = toggle_SHOW_HB_W_SR;
			toggle_SHOW_HB_W_SR = UIData.hasMoleculeDisplay && GUILayout.Toggle(toggle_SHOW_HB_W_SR,
			                                                                     new GUIContent("Sugar", "Hide sugar atoms"));

			bool hb_not_sugar_toggled = toggle_SHOW_HB_NOT_SUGAR;
			toggle_SHOW_HB_NOT_SUGAR = UIData.hasMoleculeDisplay && GUILayout.Toggle(toggle_SHOW_HB_NOT_SUGAR,
			                                                                    new GUIContent("Non Sugar", "Hide Non sugar Atoms"));




			if(!hb_w_sb_toggled && toggle_SHOW_HB_W_SR)
				show_HyperBalls_Sugar(false);
			else if (hb_w_sb_toggled && !toggle_SHOW_HB_W_SR)
				show_HyperBalls_Sugar(true);

			if(!hb_not_sugar_toggled && toggle_SHOW_HB_NOT_SUGAR)
				Hide_No_Sugar_Hiperballs(false);
			else if (hb_not_sugar_toggled && !toggle_SHOW_HB_NOT_SUGAR)
				Hide_No_Sugar_Hiperballs(true);
			
			GUILayout.EndHorizontal();
			
			/*************************************************/
			/*************************************************/
			GUILayout.BeginHorizontal();
			if(GUILayout.Button(new GUIContent("Tune Menu"))) {
				showSugarRibbonsTuneMenu = !showSugarRibbonsTuneMenu;
			}
			
			GUI.enabled = true;
			GUILayout.EndHorizontal();
			/*************************************************/

			// Bugs otherwise.
			/*
			if(!UIData.hasMoleculeDisplay) {
				showSecStructMenu = false;
				return;
			}
*/
			GUILayout.FlexibleSpace();
			GUI.DragWindow();
		}


		public static void SugarRibbonsTune(int a){
			showSugarRibbonsTuneMenu = SetTitleExit("Tune Menu");
			if (!showSugarChainMenu)
				showSugarRibbonsTuneMenu = false;

			int labelWidth = (int) (0.2f * Rectangles.SugarRibbonsTuneWidth);
			int sliderWidth = (int) (0.73f * Rectangles.SugarRibbonsTuneWidth);

			/*************************************************/
			GUILayout.BeginHorizontal();
			bool oxyToggled = toggle_OXYGEN; //to avoid to create all sphere to each frames
			toggle_OXYGEN = UIData.hasMoleculeDisplay && GUILayout.Toggle(toggle_OXYGEN,
			                                                              new GUIContent("Show Oxygens", "show ring oxygens with a red sphere"));
			
			if (toggle_TWISTER && toggle_OXYGEN && !oxyToggled) {
				SR.ShowOxySphere ();		
				oxyToggled = true;
			} else if (toggle_RING_BLENDING && toggle_OXYGEN && !oxyToggled) {
				ringblending.ShowOxySphere();
				oxyToggled = true;
			}else{
				if (oxyToggled && !toggle_OXYGEN){
					GameObject[] OxySpheres = GameObject.FindGameObjectsWithTag("OxySphere");
					foreach(GameObject OxySphere in OxySpheres)
						GameObject.Destroy(OxySphere);
				}
			}
			
			toggle_SUGAR_ONLY = UIData.hasMoleculeDisplay && GUILayout.Toggle(toggle_SUGAR_ONLY,
			                                                                  new GUIContent("Sugar Only?", "use only sugar for RingBlending and SugarRibbons"));
			
			GUILayout.EndHorizontal();
			/*************************************************/
			GUILayout.BeginHorizontal();
			if(GUILayout.Button(new GUIContent("Change Coloration"))) {
				showColorTuneMenu = !showColorTuneMenu;
			}
			
			GUI.enabled = true;
			GUILayout.EndHorizontal();
			
				/*************************************************/
			GUILayout.BeginHorizontal();
			GUILayout.Label("Oxygen Sphere Size");
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			OxySphereSize = LabelSlider(OxySphereSize, 0.01f, 2f,
			                            OxySphereSize.ToString("0.00"), "", true, sliderWidth, labelWidth, true);
			if (OxySphereSizeCheck!=OxySphereSize){
				OxySphereSizeCheck=OxySphereSize;
				SR.OXYSPHERESIZE =  OxySphereSize;
				GameObject[] OxySpheres = GameObject.FindGameObjectsWithTag("OxySphere");
				foreach(GameObject OxySphere in OxySpheres)
					OxySphere.transform.localScale = new Vector3(OxySphereSize, OxySphereSize, OxySphereSize);
			}


			GUILayout.EndHorizontal();
			/*************************************************/
			/*************************************************/
			GUILayout.BeginHorizontal();
			GUILayout.Label("Ribbons Thickness");
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			RibbonsThickness = LabelSlider(RibbonsThickness, 0.02f, 2f,
			                               RibbonsThickness.ToString("0.00"), "", true, sliderWidth, labelWidth, true);
			GUILayout.EndHorizontal();
			/*************************************************/
			/*************************************************/
			GUILayout.BeginHorizontal();
			GUILayout.Label("Inner Ring Thickness");
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			thickness_Little = LabelSlider(thickness_Little, 0f, 3f,
			                               thickness_Little.ToString("0.00"), "", true, sliderWidth, labelWidth, true);
			GUILayout.EndHorizontal();
			/*************************************************/
			/*************************************************/
			GUILayout.BeginHorizontal();
			GUILayout.Label("Outer Ring Thickness");
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			thickness_BIG = LabelSlider(thickness_BIG, 0.00f, 3f,
			                            thickness_BIG.ToString("0.00"), "", true, sliderWidth, labelWidth, true);
			GUILayout.EndHorizontal();
			/*************************************************/
			/*************************************************/
			GUILayout.BeginHorizontal();
			GUILayout.Label("Pyranose (6) : C1,C4 Bond Thickness");
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			thickness_bond_6_C1_C4 = LabelSlider(thickness_bond_6_C1_C4, 0.0f, 1f,
			                                     thickness_bond_6_C1_C4.ToString("0.00"), "", true, sliderWidth, labelWidth, true);
			GUILayout.EndHorizontal();
			/*************************************************/
			/*************************************************/
			GUILayout.BeginHorizontal();
			GUILayout.Label("Pyranose (6) : Other Bond Thickness");
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			thickness_6_other = LabelSlider(thickness_6_other, 0.01f, 0.3f,
			                                thickness_6_other.ToString("0.00"), "", true, sliderWidth, labelWidth, true);
			GUILayout.EndHorizontal();
			/*************************************************/
			/*************************************************/
			GUILayout.BeginHorizontal();
			GUILayout.Label("Furanose (5) : Bond Thickness");
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			thickness_bond_5 = LabelSlider(thickness_bond_5, 0.01f, 0.3f,
			                               thickness_bond_5.ToString("0.00"), "", true, sliderWidth, labelWidth, true);
			GUILayout.EndHorizontal();
			/*************************************************/

			/*************************************************/
			GUI.enabled = toggle_TWISTER;
			GUILayout.BeginHorizontal();
			if(GUILayout.Button(new GUIContent("Apply changes"))) {
				// Destroying the ribbons
				ResetSugarRibbons();
			}
			GUI.enabled = true;
			GUILayout.EndHorizontal();
			/*************************************************/
			/*************************************************/
			GUILayout.BeginHorizontal();
			if(GUILayout.Button(new GUIContent("Reset parameters"))) {
				// Destroying the ribbons
				ResetDefaultParametersSugarRibbons();
			}
			GUILayout.EndHorizontal();
			/*************************************************/

			GUILayout.FlexibleSpace();
			GUI.DragWindow();
		}

		//FOR SUGAR RIBBONS ONLY
		public static void ColorTuneMenu(int a){
			showColorTuneMenu = SetTitleExit("ColorTune Menu");
			if (!showSugarChainMenu)
				showColorTuneMenu = false;
			int labelWidth = (int) (0.2f * Rectangles.ColorTuneWidth);
			int sliderWidth = (int) (0.73f * Rectangles.ColorTuneWidth);

			/*************************************************/

			GUILayout.BeginHorizontal();
			GUILayout.Label("OXYGEN SPHERE");
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("Sugar", "Coloration by sugar Type"))) {
				oxyToggled = true;
				GameObject[] OxySpheres = GameObject.FindGameObjectsWithTag("OxySphere");
				foreach(GameObject OxySphere in OxySpheres)
					GameObject.Destroy(OxySphere);

				SR.ShowOxySphere(1);
				toggle_OXYGEN=true;
			}
			if (GUILayout.Button (new GUIContent ("Chain", "Coloration by Chain"))) {
				oxyToggled = true;
				GameObject[] OxySpheres = GameObject.FindGameObjectsWithTag("OxySphere");
				foreach(GameObject OxySphere in OxySpheres)
					GameObject.Destroy(OxySphere);
				
				SR.ShowOxySphere(2);
				toggle_OXYGEN=true;
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("Custom", "Choose a custom Color"))) {
				if (m_colorPicker != null)
					m_colorPicker = null;
				
				m_colorPicker = new ColorPicker(Rectangles.colorPickerRect,	OxySphereColor, null, "All", "All", "Color Picket");
			}
			GUILayout.EndHorizontal ();
			if (OxySphereColorCheck.color != OxySphereColor.color){
				OxySphereColorCheck.color = OxySphereColor.color;
				
				GameObject[] OxySpheres = GameObject.FindGameObjectsWithTag("OxySphere");
				if (OxySpheres.Length>0){
					foreach(GameObject OxySphere in OxySpheres)
						OxySphere.GetComponent<Renderer>().material.color = OxySphereColor.color;
				}else{
					SR.ShowOxySphere();
					toggle_OXYGEN=true;
				}
				
			}
			/*************************************************/
			/*************************************************/
			GUILayout.BeginHorizontal();
			GUILayout.Label("RINGS");
			GUILayout.EndHorizontal();


			GUILayout.BeginHorizontal();
			if (GUILayout.Button (new GUIContent ("Sugar", "Coloration by sugar Type"))) {
				ColorationModeRing=0;
				//ResetSugarRibbons();
				SR.updateColor("SugarRibbons_RING_BIG", ColorationModeRing);
			}

			if (GUILayout.Button (new GUIContent ("Chain", "Coloration by Chain"))) {
				ColorationModeRing=1;
				//ResetSugarRibbons();
				SR.updateColor("SugarRibbons_RING_BIG", ColorationModeRing);
			}
			GUILayout.EndHorizontal();
			/*************************************************/
			/*************************************************/
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("Custom Color", "Choose a custom Color"))) {
				if (m_colorPicker != null)
					m_colorPicker = null;
				
				m_colorPicker = new ColorPicker(Rectangles.colorPickerRect,	RingColor, null, "All", "All", "Color Picket");

			}
			GUILayout.EndHorizontal ();

			if (RingColorcheck.color != RingColor.color){
				RingColorcheck.color = RingColor.color;
				ColorationModeRing=2;

				SR.updateColor("SugarRibbons_RING_BIG",RingColor);
			}
			/*************************************************/
			/*************************************************/
			GUILayout.BeginHorizontal();
			GUILayout.Label("Lighter Color Factor");
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			lighter_color_factor_ring = LabelSlider(lighter_color_factor_ring, -1f, 1f,
			                                       lighter_color_factor_ring.ToString("0.00"), "", true, sliderWidth, labelWidth, true);
			GUILayout.EndHorizontal();

			if (lighter_color_factor_ring_check != lighter_color_factor_ring){
				lighter_color_factor_ring_check=lighter_color_factor_ring;
				SR.LIGHTER_COLOR_FACTOR_RING = lighter_color_factor_ring;

				if (ColorationModeRing==2)
					SR.updateColor("SugarRibbons_RING_BIG", RingColor);
				else
					SR.updateColor("SugarRibbons_RING_BIG", ColorationModeRing);
			}

			/*************************************************/
			/*************************************************/
			GUILayout.BeginHorizontal();
			GUILayout.Label("BONDS");
			GUILayout.EndHorizontal();

			
			/*************************************************/
			GUILayout.BeginHorizontal();
			if (GUILayout.Button (new GUIContent ("Sugar", "Coloration by sugar Type"))) {
				ColorationModeBond=0;
				SR.updateColor("SugarRibbons_BOND", ColorationModeBond); //TODO find a way to retreive vertex coloration with side sugar type.
				//ResetSugarRibbons();
			}
			
			if (GUILayout.Button (new GUIContent ("Chain", "Coloration by Chain"))) {
				ColorationModeBond=1;
				SR.updateColor("SugarRibbons_BOND", ColorationModeBond);
			}
			GUILayout.EndHorizontal ();
			/*************************************************/
			/*************************************************/
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("Custom Color", "Choose a custom Color"))) {
				if (m_colorPicker != null)
					m_colorPicker = null;
				
				m_colorPicker = new ColorPicker(Rectangles.colorPickerRect,	BondColor, null, "All", "All", "Color Picket");
			}
			GUILayout.EndHorizontal ();
			if (BondColorcheck.color != BondColor.color){
				BondColorcheck.color = BondColor.color;

				ColorationModeBond=2;
				SR.updateColor("SugarRibbons_BOND",BondColor);
			}
			/*************************************************/
			/*************************************************/
			GUILayout.BeginHorizontal();
			GUILayout.Label("Lighter Color Factor");
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			lighter_color_factor_bond = LabelSlider(lighter_color_factor_bond, -1f, 1f,
			                                        lighter_color_factor_bond.ToString("0.00"), "", true, sliderWidth, labelWidth, true);
			GUILayout.EndHorizontal();

			if (lighter_color_factor_bond_check != lighter_color_factor_bond){
				lighter_color_factor_bond_check=lighter_color_factor_bond;
				SR.LIGHTER_COLOR_FACTOR_BOND = lighter_color_factor_bond;
				if(ColorationModeBond==2)
					SR.updateColor("SugarRibbons_BOND", BondColor);
				else
					SR.updateColor("SugarRibbons_BOND", ColorationModeBond);


			}
			
			/*************************************************/
			
			GUILayout.FlexibleSpace();
			GUI.DragWindow();
		}

		public static void ResetSugarRibbons(){
			toggle_NA_HIDE = !toggle_NA_HIDE;

			GameObject[] objs; 
			objs= GameObject.FindGameObjectsWithTag("SugarRibbons_RING_BIG");
			foreach(GameObject obj in objs)
				GameObject.Destroy(obj);

			objs = GameObject.FindGameObjectsWithTag("SugarRibbons_RING_little");
			foreach(GameObject obj in objs)
				GameObject.Destroy(obj);
			objs = GameObject.FindGameObjectsWithTag("SugarRibbons_BOND");
			foreach(GameObject obj in objs)
				GameObject.Destroy(obj);
			//We flush all the previous list.
			SR.cleanup();
			// Recreating them
			SR.createSugarRibs(RibbonsThickness, toggle_SUGAR_ONLY, thickness_Little, thickness_BIG, 
			                   thickness_bond_6_C1_C4, thickness_6_other, thickness_bond_5, lighter_color_factor_ring, lighter_color_factor_bond, 
			                   ColorationModeRing, ColorationModeBond, BondColor, RingColor, OxySphereSize,OxySphereColor);
			toggle_NA_HIDE = true;


			if (toggle_OXYGEN){
				GameObject[] OxySpheres = GameObject.FindGameObjectsWithTag("OxySphere");
				foreach(GameObject OxySphere in OxySpheres)
					GameObject.Destroy(OxySphere);
				SR.ShowOxySphere ();		

			}
		}

		public static void show_HyperBalls_Sugar(bool show){
			int i=0;
			for (i=0; i<HBallManager.hballs.Length; i++){
				int atom_number =(int) HBallManager.hballs[i].GetComponent<BallUpdate>().number;
				if (MoleculeModel.sugarResname.Contains(MoleculeModel.atomsResnamelist[atom_number])){
					HBallManager.hballs[i].GetComponent<Renderer>().enabled=show;
				}
				if (i<HStickManager.sticks.Length){ 
					int atom_number1=(int)HStickManager.sticks[i].atompointer1.GetComponent<BallUpdate>().number;
					int atom_number2=(int)HStickManager.sticks[i].atompointer2.GetComponent<BallUpdate>().number;

					if ((MoleculeModel.sugarResname.Contains(MoleculeModel.atomsResnamelist[atom_number1]))||
					    (MoleculeModel.sugarResname.Contains(MoleculeModel.atomsResnamelist[atom_number2])))
						HStickManager.sticks[i].GetComponent<Renderer>().enabled=show;
				}
			}
			//If we didn't finish to check the bond list (more bond than atoms)
			//we check the end of the list.
			while(i<HStickManager.sticks.Length){
				int atom_number1=(int)HStickManager.sticks[i].atompointer1.GetComponent<BallUpdate>().number;
				int atom_number2=(int)HStickManager.sticks[i].atompointer2.GetComponent<BallUpdate>().number;
				if ((MoleculeModel.sugarResname.Contains(MoleculeModel.atomsResnamelist[atom_number1]))||
				    (MoleculeModel.sugarResname.Contains(MoleculeModel.atomsResnamelist[atom_number2])))
					HStickManager.sticks[i].GetComponent<Renderer>().enabled=show;
				i++;
			}
		}

		/*This fonction is made to hide hyperballs which are not Sugar Atoms*/
		public static void Hide_No_Sugar_Hiperballs(bool show){
			int i=0;
			for (i=0; i<HBallManager.hballs.Length; i++){
				int atom_number =(int) HBallManager.hballs[i].GetComponent<BallUpdate>().number;
				if (!MoleculeModel.sugarResname.Contains(MoleculeModel.atomsResnamelist[atom_number])){
					HBallManager.hballs[i].GetComponent<Renderer>().enabled=show;
				}
				if (i<HStickManager.sticks.Length){ 
					int atom_number1=(int)HStickManager.sticks[i].atompointer1.GetComponent<BallUpdate>().number;
					int atom_number2=(int)HStickManager.sticks[i].atompointer2.GetComponent<BallUpdate>().number;
					
					if ((!MoleculeModel.sugarResname.Contains(MoleculeModel.atomsResnamelist[atom_number1]))||
					    (!MoleculeModel.sugarResname.Contains(MoleculeModel.atomsResnamelist[atom_number2])))
						HStickManager.sticks[i].GetComponent<Renderer>().enabled=show;
				}
			}
			//If we didn't finish to check the bond list (more bond than atoms)
			//we check the end of the list.
			while(i<HStickManager.sticks.Length){
				int atom_number1=(int)HStickManager.sticks[i].atompointer1.GetComponent<BallUpdate>().number;
				int atom_number2=(int)HStickManager.sticks[i].atompointer2.GetComponent<BallUpdate>().number;
				if ((!MoleculeModel.sugarResname.Contains(MoleculeModel.atomsResnamelist[atom_number1]))||
				    (!MoleculeModel.sugarResname.Contains(MoleculeModel.atomsResnamelist[atom_number2])))
					HStickManager.sticks[i].GetComponent<Renderer>().enabled=show;
				i++;
			}
		}

		public static void showHydrogens(bool show){
			int i=0;
			for (i=0; i<HBallManager.hballs.Length; i++){
				if (HBallManager.hballs[i].tag=="H"){
					HBallManager.hballs[i].GetComponent<Renderer>().enabled=show;
				}
				// We want to check atoms and bond in one loop. 
				//so we check if we not over the size of the bond list
				if (i<HStickManager.sticks.Length){ 
					if ((HStickManager.sticks[i].atompointer2.tag == "H") ||
					    (HStickManager.sticks[i].atompointer1.tag == "H"))
						HStickManager.sticks[i].GetComponent<Renderer>().enabled=show;
				}
			}
			//If we didn't finish to check the bond list (more bond than atoms)
			//we check the end of the list.
			while(i<HStickManager.sticks.Length){
				if ((HStickManager.sticks[i].atompointer2.tag == "H") ||
				    (HStickManager.sticks[i].atompointer1.tag == "H"))
					HStickManager.sticks[i].GetComponent<Renderer>().enabled=show;
				i++;
			}
		}


		
		public static void ResetDefaultParametersSugarRibbons(){

			RibbonsThickness=0.15f;
			thickness_Little=1.8f;
			thickness_BIG=1f;
			thickness_bond_6_C1_C4=0.2f;
			thickness_6_other=0.16f;
			thickness_bond_5=0.2f;
			lighter_color_factor_ring=0.35f;
			lighter_color_factor_bond=0.35f;
			ColorationModeRing=0;
			ColorationModeBond=0;
			OxySphereSize=1f;
			OxySphereColor.color=Color.red;
			lighter_color_factor_bond=0.35f;
			lighter_color_factor_ring=0.35f;

			ResetSugarRibbons();
		}

		/// <summary>
		/// Defines the bond type selection menu window, which is called from the appearance menu.
		/// </summary>
		/// <param name='a'>
		/// Window identifier.
		/// </param>
		public static void Bond (int a)	{
			SetTitle("Bond Style");
			
//			if(UIData.atomtype==UIData.AtomType.particleball&&!UIData.openAllMenu)GUI.enabled=false;
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button (new GUIContent ("Cube", "Use Cubes to represent bonds"), GUILayout.Width(Rectangles.atomButtonWidth))) {
				UIData.resetBondDisplay = true;
				UIData.bondtype = UIData.BondType.cube;
				showBondType = false;
			}

			if (GUILayout.Button (new GUIContent ("Line", "Use the Line renderer to represent bonds"), GUILayout.Width(Rectangles.atomButtonWidth))) {
				UIData.resetBondDisplay = true;
				UIData.bondtype = UIData.BondType.line;
				showBondType = false;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button (new GUIContent ("HyperStick", "Use the HyperStick shader to represent bonds"), GUILayout.Width(Rectangles.atomButtonWidth))) {	
				UIData.resetBondDisplay = true;
				UIData.bondtype = UIData.BondType.hyperstick;
				showBondType = false;
			}

			if (GUILayout.Button (new GUIContent ("No Stick", "Do not render any bonds"), GUILayout.Width(Rectangles.atomButtonWidth))) {
				UIData.resetBondDisplay = true;
				UIData.bondtype = UIData.BondType.nobond;
				showBondType = false;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal ();
			
			// Those hidden features aren't working at all		
/*			if (UIData.openAllMenu) {	
				GUILayout.BeginHorizontal ();

				if (GUILayout.Button (new GUIContent ("Tube Stick", "Use the Tube Stick renderer to represent bonds"))) {
					UIData.resetBondDisplay = true;
					UIData.bondtype = UIData.BondType.tubestick;
					showBondType = false;
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
					showBondType = false;
				}

				GUILayout.EndHorizontal ();
			}
*/
			GUI.enabled = true;
			
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
			
			GUI.DragWindow();
		} // end of Bond

		
		/// <summary>
		/// Defines the Metaphor menu window, which is launched by the Metaphor button in the  Hperball Style window
		/// </summary>
		/// <param name='a'>
		/// Window identifier.
		/// </param>
		public static void Metaphor (int a) {
			showMetaphorType = SetTitleExit("Metaphor");
			
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("CPK", "CPK representation as balls and sticks"))) {
				newGlobalRadius = 0.2f;
				deltaRadius = (newGlobalRadius - globalRadius) / transDelta;
				newShrink = 0.0001f;
				deltaShrink = (newShrink - shrink) / transDelta;
				newScale = 0.3f;
				deltaScale = (newScale - linkScale) / transDelta;
				transMETAPHOR = true;
			}
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
			
			if (GUILayout.Button (new GUIContent ("Licorice", "Licorice representation of the molecule"))) {
				newGlobalRadius = 0.1f;
				deltaRadius = (newGlobalRadius - globalRadius) / transDelta;
				newShrink = 0.0001f;
				deltaShrink = (newShrink - shrink) / transDelta;
				newScale = 1.0f;
				deltaScale = (newScale - linkScale) / transDelta;
				transMETAPHOR = true;
			}						
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			
			if (GUILayout.Button (new GUIContent ("VdW", "van der Waals representation as spacefilling spheres"))) {
				newGlobalRadius = 1.0f;
				deltaRadius = (newGlobalRadius - globalRadius) / transDelta;
				newShrink = 0.8f;
				deltaShrink = (newShrink - shrink) / transDelta;
				newScale = 1.0f;
				deltaScale = (newScale - linkScale) / transDelta;
				transMETAPHOR = true;
			}
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			
			if (GUILayout.Button (new GUIContent ("Smooth", "Smooth HyperBalls metaphor representation"))) {
				newGlobalRadius = 0.35f;
				deltaRadius = (newGlobalRadius - globalRadius) / transDelta;
				newShrink = 0.4f;
				deltaShrink = (newShrink - shrink) / transDelta;
				newScale = 1.0f;
				deltaScale = (newScale - linkScale) / transDelta;
				transMETAPHOR = true;	
			}						
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			
			if (GUILayout.Button (new GUIContent ("SmoothLink", "SmoothLink HyperBalls representation"))) {
				newGlobalRadius = 0.4f;
				deltaRadius = (newGlobalRadius - globalRadius) / transDelta;
				newShrink = 0.5f;
				deltaShrink = (newShrink - shrink) / transDelta;
				newScale = 1.0f;
				deltaScale = (newScale - linkScale) / transDelta;
				transMETAPHOR = true;	
			}						
			GUILayout.EndHorizontal ();
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
			GUI.DragWindow();
		} // End of Metaphor
		
		
		
		/// <summary>
		/// Defines the electrostatic menu window, which is opened from the main menu window.
		/// </summary>
		/// <param name='a'>
		/// Window identifier.
		/// </param>
		public static void Electrostatics (int a) {
			#if UNITY_WEBPLAYER
			GUI.enabled = false;
			#endif
			showElectrostaticsMenu = SetTitleExit("Electrostatics");
			GUILayout.BeginHorizontal ();
				electroIsoSurfaceTransparency = GUILayout.Toggle(electroIsoSurfaceTransparency, new GUIContent("Transparency", "Toggle transparent isosurfaces."));
			GUILayout.EndHorizontal ();		
			
			GUILayout.BeginHorizontal ();
			int sliderWidth = (int) (Rectangles.electroMenuWidth * 0.60f);
			generateThresholdDx_neg = LabelSlider (generateThresholdDx_neg, -10f, 0f, 
							"T: " + Mathf.Round (generateThresholdDx_neg * 10f) / 10f, "Ramp value used for surface generation",GUI.enabled , sliderWidth, 1);
			GUILayout.EndHorizontal ();
			
			if (!MoleculeModel.dxFileExists)
				GUI.enabled = false ;
			
			
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("Load Neg.", "Read an OpenDx format electrostatic field and generate a surface"))) {
				MoleculeModel.surfaceFileExists = true;
				
				readdx.ReadFile(GUIDisplay.file_base_name+".dx",MoleculeModel.Offset);
				dxRead = true;
				
				string tag = "Elect_iso_negative";
				electroIsoNegativeInitialized = true;
				showElectroIsoNegative = true;

				GameObject[] IsoSurfaces = GameObject.FindGameObjectsWithTag(tag);
				foreach(GameObject iso in IsoSurfaces)
					Object.Destroy(iso);
				readdx.isoSurface (generateThresholdDx_neg,Color.red,tag, electroIsoSurfaceTransparency);
			}
			
			GUI.enabled = true ;
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			generateThresholdDx_pos = LabelSlider (generateThresholdDx_pos, 0f, 10f, 
							"T: " + Mathf.Round (generateThresholdDx_pos * 10f) / 10f, "Ramp value used for surface generation", GUI.enabled, sliderWidth, 1);
			GUILayout.EndHorizontal ();
			
			if (!MoleculeModel.dxFileExists)
				GUI.enabled = false ;
			
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("Load Pos.", "Read an OpenDx format electrostatic field and generate a surface"))) {
				MoleculeModel.surfaceFileExists = true;	
				readdx.ReadFile(GUIDisplay.file_base_name+".dx",MoleculeModel.Offset);
				dxRead = true;
				string tag = "Elect_iso_positive";
				showElectroIsoPositive = true;
				electroIsoPositiveInitialized = true;

				GameObject[] IsoSurfaces = GameObject.FindGameObjectsWithTag(tag);
				foreach(GameObject iso in IsoSurfaces)
					Object.Destroy(iso);
				readdx.isoSurface (generateThresholdDx_pos,Color.blue,tag, electroIsoSurfaceTransparency);
			}
			
			GUI.enabled = true ;
			GUILayout.EndHorizontal ();

			if (dxRead && electroIsoNegativeInitialized)
				GUI.enabled = true;
			else 
				GUI.enabled = false;
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button (new GUIContent ("Toggle Neg.", "Toggles negative iso-surface  from visible to hidden and vice versa"))) {
				string tag = "Elect_iso_negative";
				if (showElectroIsoNegative) {
					showSurface = false;
					showSurfaceCut = false;
					showSurfaceMobileCut = false;
					showElectroIsoNegative = false;
					GameObject[] IsoSurfaces = GameObject.FindGameObjectsWithTag(tag);
					foreach(GameObject iso in IsoSurfaces)
						iso.GetComponent<Renderer>().enabled = false;
				} else {
					showElectroIsoNegative = true;
					GameObject[] IsoSurfaces = GameObject.FindGameObjectsWithTag(tag);
					foreach(GameObject iso in IsoSurfaces)
						iso.GetComponent<Renderer>().enabled = true;
				}				
			
			}
			GUILayout.EndHorizontal ();
			
			GUI.enabled = (dxRead && electroIsoPositiveInitialized);
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button (new GUIContent ("Toggle Pos.", "Toggles positive iso-surface from visible to hidden and vice versa"))) {
				string tag = "Elect_iso_positive";
				if (showElectroIsoPositive) {
					showSurface = false;
					showSurfaceCut = false;
					showSurfaceMobileCut = false;
					showElectroIsoPositive = false;
					GameObject[] IsoSurfaces = GameObject.FindGameObjectsWithTag(tag);
					foreach(GameObject iso in IsoSurfaces)
						iso.GetComponent<Renderer>().enabled = false;
				} else {
					showElectroIsoPositive = true;
					GameObject[] IsoSurfaces = GameObject.FindGameObjectsWithTag(tag);
					foreach(GameObject iso in IsoSurfaces)
						iso.GetComponent<Renderer>().enabled = true;
				}
			}
			GUILayout.EndHorizontal();

			#if UNITY_WEBPLAYER
			if(!MoleculeModel.fieldLineFileExists)
				GUI.enabled = false;
			else
				GUI.enabled = true;
			#else
			GUI.enabled = true;
			#endif
			
			GUILayout.BeginHorizontal();
			if(GUILayout.Button(new GUIContent("Volumetric Fields", "Toggles volumetric rendering of electrostatic fields"))) {
				showVolumetricFields = !showVolumetricFields;
				readdx.ReadFile(GUIDisplay.file_base_name+".dx",MoleculeModel.Offset);
				dxRead = true;
				GameObject volumObj;
				volumObj = GameObject.FindGameObjectWithTag("Volumetric");
				VolumetricFields volumetricFields = null;
				
				if(showVolumetricFields) {
					volumObj.AddComponent<VolumetricFields>(); // adding the script
					volumetricFields = volumObj.GetComponent<VolumetricFields>();
					volumetricFields.Init();
				}
				else {
					volumetricFields = volumObj.GetComponent<VolumetricFields>();
					volumetricFields.Clear();
				}
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal ();
			if (!MoleculeModel.fieldLineFileExists)
				GUI.enabled = false ;
			if (GUILayout.Button (new GUIContent ("Field Lines", "Toggles animated field lines from visible to hidden and vice versa"))) {
				if (showFieldLines) {
					showFieldLines = false;
					m_colorPicker = null ;
					GameObject FieldLineManager = GameObject.Find ("FieldLineManager");
					FieldLineModel Line = FieldLineManager.transform.GetComponent<FieldLineModel> ();
					Line.killCurrentEffects ();
				} else {
					showFieldLines = true;
					if(GameObject.FindGameObjectsWithTag("FieldLineManager").Length == 0)
						FieldLineStyle.DisplayFieldLine ();
				}				
			}
			GUI.enabled = true ;
			GUILayout.EndHorizontal ();
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
			
			GUI.DragWindow();
		} // End of Electrostatic
		
		
		/// <summary>
		/// Creates a new texture of the appropriate size for a button, unless it already exists.
		/// Used by AdvOptions.
		/// </summary>
		/// <returns>
		/// A Texture2D.
		/// </returns>
		private static Texture2D MakeButtonTex(Texture2D tex) {
			if (tex)
				return(tex);
			else
				return (new Texture2D(20,10,TextureFormat.ARGB32,false));
		}
		
		
		
		/// <summary>
		/// Defines the advanced options menu window, which is opened from the main menu window.
		/// </summary>
		/// <param name='a'>
		/// Window identifier.
		/// </param>
		public static void AdvOptions(int a){
			showAdvMenu = SetTitleExit("Advanced Options");
			
			GUILayout.BeginHorizontal();
			GUILayout.Label (new GUIContent ("GUI Scale: " + GUIDisplay.guiScale.ToString("0.00"), "Adjusts the scale of the GUI windows"), GUILayout.MinWidth ((int)(Rectangles.advOptWidth * 0.4f)));
			GUIDisplay.guiScale = GUILayout.HorizontalSlider (GUIDisplay.guiScale, 0.3f, 1.7f, GUILayout.Width (((int)(Rectangles.advOptWidth * 0.4f))));
			
			if (GUILayout.Button(new GUIContent("OK", "Apply new GUI Scale")))
				if(GUIDisplay.guiScale != GUIDisplay.oldGuiScale) {
					GUIDisplay.oldGuiScale = GUIDisplay.guiScale;
					Rectangles.Scale();
				}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("Ortho/Persp", "Switches between orthographic and perspective camera"))){
				if(Camera.main.orthographic)
					Camera.main.orthographic = false ;
				else {
					Camera.main.orthographic = true ;
					Camera.main.orthographicSize = 20f ;
				}
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			orthoSize = LabelSlider(Camera.main.orthographicSize, minOrthoSize, maxOrthoSize, 
					"Camera Size", "This slider changes the size of the orthographic camera.", Camera.main.orthographic, 100, 20);
			GUI.enabled = true; // LabeLSlider can disable the entire GUI. I don't like that at all, but it's expected in some parts of the GUI.
			// Still needs changing, methinks. ---Alexandre
			GUILayout.EndHorizontal();
			
			Camera.main.orthographicSize = orthoSize ;
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("Best Textures: "+ queryBestTextures(), 
												"This switches between the complete set of textures and a selection of the best ones"))){
				onlyBestTextures = !onlyBestTextures;
				if(onlyBestTextures)
					texture_set = 0;
				else
					texture_set = 5;
			}
			
			if (GUILayout.Button(new GUIContent("Depth Cueing", "Depth Cueing"))) {
				if (!dxRead) {
					readdx.ReadFile(GUIDisplay.file_base_name+".dx",MoleculeModel.Offset);
					dxRead = true;
				}
				if(DepthCueing.isEnabled && !DepthCueing.reset)
					depthCueing.Revert();
				else {
					depthCueing = new DepthCueing();
					depthCueing.Darken();
				}
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("Volumetric Depth Cueing", "Volumetric Depth Cueing"))) {
				showVolumetricDepth = !showVolumetricDepth;
				
				if (!dxRead) {
					readdx.ReadFile(GUIDisplay.file_base_name+".dx",MoleculeModel.Offset);
					dxRead = true;
				}
				
				GameObject volumObj;
				volumObj = GameObject.FindGameObjectWithTag("Volumetric");
				VolumetricDepth volumetricDepth = null;
				
				if(showVolumetricDepth) {
					volumObj.AddComponent<VolumetricDepth>(); // adding the script
					volumetricDepth = volumObj.GetComponent<VolumetricDepth>();
					volumetricDepth.Init();
				}
				else {
					volumetricDepth = volumObj.GetComponent<VolumetricDepth>();
					volumetricDepth.Clear();
				}
			}
			
			if(GUILayout.Button(new GUIContent("Ambient Occlusion", "Ambient occlusion based on molecular density."))) {
				if(AmbientOcclusion.isEnabled && !AmbientOcclusion.reset)
					ambientOcclusion.Revert();
				else {
					ambientOcclusion = new AmbientOcclusion();
					ambientOcclusion.Occlude();
				}
			}
			GUILayout.EndHorizontal();

			// TODO : Make C-Alpha trace and his texture/color work again !
/*			//if((structType != "C-alpha trace"))
				//GUI.enabled = false ;
			
			// Creating textures to put on the buttons. Not created if they already exist.
			chainATex = MakeButtonTex(chainATex);
			chainBTex = MakeButtonTex(chainBTex);
			chainCTex = MakeButtonTex(chainCTex);
			chainDTex = MakeButtonTex(chainDTex);
			chainETex = MakeButtonTex(chainETex);
			chainFTex = MakeButtonTex(chainFTex);
			chainGTex = MakeButtonTex(chainGTex);
			chainHTex = MakeButtonTex(chainHTex);
			
			// Refreshing the color of each chain if needed
			if( (AtomModel.IsAlive()) && (!chainColorsInit) ) {
				chainAColor.color = AtomModel.GetChainColor("chainA");
				chainBColor.color = AtomModel.GetChainColor("chainB");
				chainCColor.color = AtomModel.GetChainColor("chainC");
				chainDColor.color = AtomModel.GetChainColor("chainD");
				chainEColor.color = AtomModel.GetChainColor("chainE");
				chainFColor.color = AtomModel.GetChainColor("chainF");
				chainGColor.color = AtomModel.GetChainColor("chainG");
				chainHColor.color = AtomModel.GetChainColor("chainH");
				
				chainColorsInit = true;
			}
			
			// Getting the current colors of each chain
			for(int i=0; i<200; i++) {
				if(!AtomModel.IsAlive()) {
					chainAColors[i] = Color.white;
					chainBColors[i] = Color.white;
					chainCColors[i] = Color.white;
					chainDColors[i] = Color.white;
					chainEColors[i] = Color.white;
					chainFColors[i] = Color.white;
					chainGColors[i] = Color.white;
					chainHColors[i] = Color.white;
				}
				else {
					chainAColors[i] = chainAColor.color;
					chainBColors[i] = chainBColor.color;
					chainCColors[i] = chainCColor.color;
					chainDColors[i] = chainDColor.color;
					chainEColors[i] = chainEColor.color;
					chainFColors[i] = chainFColor.color;
					chainGColors[i] = chainGColor.color;
					chainHColors[i] = chainHColor.color;
				}
			}
			
			
			// Actually setting the colors to the textures
			chainATex.SetPixels(chainAColors);
			chainATex.Apply(true);
			
			chainBTex.SetPixels(chainBColors);
			chainBTex.Apply(true);
			
			chainCTex.SetPixels(chainCColors);
			chainCTex.Apply(true);
			
			chainDTex.SetPixels(chainDColors);
			chainDTex.Apply(true);
			
			chainETex.SetPixels(chainEColors);
			chainETex.Apply(true);
			
			chainFTex.SetPixels(chainFColors);
			chainFTex.Apply(true);
			
			chainGTex.SetPixels(chainGColors);
			chainGTex.Apply(true);
			
			chainHTex.SetPixels(chainHColors);
			chainHTex.Apply(true);
			
			// New Layout line
			GUILayout.BeginHorizontal();
			GUILayout.Label("Carbon alpha trace chains colors");
			GUILayout.EndHorizontal();			
			
			// New Layout line
			GUILayout.BeginHorizontal();
			GUILayout.Label("Chain A color");
			if(GUILayout.Button(chainATex,GUILayout.MinWidth(50),GUILayout.MinHeight(20)))
				CreateColorPicker(chainAColor, "Chain A Color", "chainA");
			GUILayout.EndHorizontal();
			
			// New Layout line
			GUILayout.BeginHorizontal();
			GUILayout.Label("Chain B color");
			if(GUILayout.Button(chainBTex,GUILayout.MinWidth(50),GUILayout.MinHeight(20)))
				CreateColorPicker(chainBColor, "Chain B color", "chainB");
			GUILayout.EndHorizontal();
			
			// New Layout line
			GUILayout.BeginHorizontal();
			GUILayout.Label("Chain C color");
			if(GUILayout.Button(chainCTex,GUILayout.MinWidth(50),GUILayout.MinHeight(20)))
				CreateColorPicker(chainCColor, "Chain C color", "chainC");
			GUILayout.EndHorizontal();
			
			// New Layout line
			GUILayout.BeginHorizontal();
			GUILayout.Label("Chain D color");
			if(GUILayout.Button(chainDTex,GUILayout.MinWidth(50),GUILayout.MinHeight(20)))
				CreateColorPicker(chainDColor, "Chain D color", "chainD");
			GUILayout.EndHorizontal();
			
			// New Layout line
			GUILayout.BeginHorizontal();
			GUILayout.Label("Chain E color");
			if(GUILayout.Button(chainETex,GUILayout.MinWidth(50),GUILayout.MinHeight(20)))
				CreateColorPicker(chainEColor, "Chain E color", "chainE");
			GUILayout.EndHorizontal();
			
			// New Layout line
			GUILayout.BeginHorizontal();
			GUILayout.Label("Chain F color");
			if(GUILayout.Button(chainFTex,GUILayout.MinWidth(50),GUILayout.MinHeight(20)))
				CreateColorPicker(chainFColor, "Chain F color", "chainF");
			GUILayout.EndHorizontal();
			
			// New Layout line
			GUILayout.BeginHorizontal();
			GUILayout.Label("Chain G color");
			if(GUILayout.Button(chainGTex,GUILayout.MinWidth(50),GUILayout.MinHeight(20)))
				CreateColorPicker(chainGColor, "Chain G color", "chainG");
			GUILayout.EndHorizontal();
			
			// New Layout line
			GUILayout.BeginHorizontal();
			GUILayout.Label("Chain H color");
			if(GUILayout.Button(chainHTex,GUILayout.MinWidth(50),GUILayout.MinHeight(20)))
				CreateColorPicker(chainHColor, "Chain H color", "chainH");
			GUILayout.EndHorizontal();		
			
			// New Layout line
			GUILayout.BeginHorizontal();
			if(GUILayout.Button(new GUIContent("Apply", "Apply changes"))) {
				AtomModel.ChangeChainColor("chainA", chainAColor.color);
				AtomModel.ChangeChainColor("chainB", chainBColor.color);
				AtomModel.ChangeChainColor("chainC", chainCColor.color);
				AtomModel.ChangeChainColor("chainD", chainDColor.color);
				AtomModel.ChangeChainColor("chainE", chainEColor.color);
				AtomModel.ChangeChainColor("chainF", chainFColor.color);
				AtomModel.ChangeChainColor("chainG", chainGColor.color);
				AtomModel.ChangeChainColor("chainH", chainHColor.color);
				
				UIData.resetDisplay = true;
				//BallUpdate.resetColors = true;
			}
			GUILayout.EndHorizontal();
*/
			
			GUI.enabled = true;
			GUI.DragWindow();
		} // End of AdvOptions

		public static void GuidedOptions(int a){
			//Debug.Log ("GUIDED ACTIVE");
			showGuidedMenu = SetTitleExit("Guided Navigation");
			
//			if(!SymmetryLoaded)
//			{
			if(!UIData.guided)
			{
				// Enter the origin of symmetry as an array of 3 floats
				GUILayout.BeginHorizontal();
				GUILayout.Label (new GUIContent ("Symmetry origin: ", "Enter here the origin 3D coordinates of the symmetry axis"), GUILayout.MinWidth ((int)(Rectangles.advOptWidth * 0.4f)));
				//try{
					SymmetryOriginX = GUILayout.TextField (SymmetryOriginX, 8);
					SymmetryOriginY = GUILayout.TextField (SymmetryOriginY, 8);
					SymmetryOriginZ = GUILayout.TextField (SymmetryOriginZ, 8);
				//} catch {}
				GUILayout.EndHorizontal ();
				// Enter the direction of symmetry as an array of 3 floats
				GUILayout.BeginHorizontal();
				GUILayout.Label (new GUIContent ("Symmetry direction: ", "Enter here the direction of the symmetry axis"), GUILayout.MinWidth ((int)(Rectangles.advOptWidth * 0.4f)));
				//try{
					SymmetryDirectionX = GUILayout.TextField (SymmetryDirectionX, 8);
					SymmetryDirectionY = GUILayout.TextField (SymmetryDirectionY, 8);
					SymmetryDirectionZ = GUILayout.TextField (SymmetryDirectionZ, 8);
	         	//} catch{}
				GUILayout.EndHorizontal ();
	
				// Submit the symmetry information to proceed to the atoms coordinates changes (only one time)
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button (new GUIContent ("Send", "submit the symmetrical information."))) {
					float OriginX = float.Parse(SymmetryOriginX);
					float OriginY = float.Parse(SymmetryOriginY);
					float OriginZ = float.Parse(SymmetryOriginZ);
					float DirectionX = float.Parse(SymmetryDirectionX);
					float DirectionY = float.Parse(SymmetryDirectionY);
					float DirectionZ = float.Parse(SymmetryDirectionZ);
					Reorient.LoadSymmetry (OriginX, OriginY, OriginZ, DirectionX, DirectionY, DirectionZ);
				}
				GUILayout.EndHorizontal ();
			}
			
			if(UIData.guided)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label (new GUIContent ("Target: ", "Enter here the 3D coordinates of the atom target"), GUILayout.MinWidth ((int)(Rectangles.advOptWidth * 0.4f)));
				//try{
				TargetX = GUILayout.TextField (TargetX, 8);
				TargetY = GUILayout.TextField (TargetY, 8);
				TargetZ = GUILayout.TextField (TargetZ, 8);
				//} catch {}
				GUILayout.EndHorizontal ();
				
				GUILayout.BeginHorizontal();
				GUILayout.Label (new GUIContent ("Camera distance: ", "Enter here the desired distance between the target and the camera"), GUILayout.MinWidth ((int)(Rectangles.advOptWidth * 0.4f)));
				//try{
				CameraDistance = GUILayout.TextField (CameraDistance, 8);
				//} catch {}
				GUILayout.EndHorizontal ();
				
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button (new GUIContent ("Send", "compute the best point-of-view for the target coordinates."))) {
					float[] TargetCoords = new float[3];
					TargetCoords[0] = float.Parse(TargetX);
					TargetCoords[1] = float.Parse(TargetY);
					TargetCoords[2] = float.Parse(TargetZ);
					float Distance = float.Parse(CameraDistance);
					OptimalView.GetOptimalPosition(TargetCoords, Distance);
				}
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				showOriginAxe = UIData.hasMoleculeDisplay && GUILayout.Toggle(showOriginAxe,
				new GUIContent("Show symmetry axe and origin point", "Hide/Show symmetry axe and origin point"));
				if(showOriginAxe && !originThere){
					Reorient.CreateAxeAndOrigin();
					originThere = true;
				}else if(!showOriginAxe && originThere){
					GameObject[] Objs = GameObject.FindGameObjectsWithTag("Origin");
					foreach(GameObject obj in Objs)
						GameObject.Destroy (obj);
					originThere = false;
				}
				GUILayout.EndHorizontal ();
			}

			GUI.enabled = true;
			GUI.DragWindow();
		}

		/// <summary>
		/// Defines the Field Lines window, which is opened from the Electrostatic window.
		/// Opening this window should only be possible when a JSON file containing field lines has been loaded.
		/// </summary>
		/// <param name='a'>
		/// A.
		/// </param>
		public static void FieldLines (int a) {
			
			if (GUILayout.Button (new GUIContent ("Energy/Field Color", "Choose color to represent potential energy or field lines"))) 
				CreateColorPicker(EnergyGrayColor, "Field Lines Color", null);

			if (GUILayout.Button (new GUIContent ("Color Gradient", "Display field lines with a color gradient")))
				fieldLineColorGradient = true;
			
			int sliderWidth = (int) (Rectangles.fieldLinesWidth * 0.8f);
			
			speed = LabelSlider (speed, 0.001f, 1.0f, 
				"Speed  " + speed, "Determines field lines animation speed", true, sliderWidth, 1, true);
			density = LabelSlider (density, 1.0f, 8.0f, 
				"Density  " + density, "Determines field lines density", true, sliderWidth, 1, true);
			linewidth = LabelSlider (linewidth, 0.01f, 5.0f, 
				"Width  " + linewidth, "Determines field lines width", true, sliderWidth, 1, true);
			linelength = LabelSlider (linelength, 0.8f, 0.1f, 
				"Length " + (1 - linelength), "Determines field lines length", true, sliderWidth, 1, true);
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
			
			GUI.DragWindow();
		} // End of FieldLines
		
		

		/// <summary>
		/// Defines the Surface menu window, which is opened from the main menu window.
		/// </summary>
		/// <param name='a'>
		/// Window identifier.
		/// </param>
		public static void Surface (int a) {
			showSurfaceMenu = SetTitleExit("Surface");
			
			GUILayout.BeginHorizontal ();
			generateThreshold = LabelSlider (generateThreshold, 0.002f, 4f, "T:" + Mathf.Round (generateThreshold * 10f) / 10f,
									"Determines ramp value for surface generation", true, (int) (0.5 * Rectangles.surfaceMenuWidth), 1);
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
			GUI.enabled = UIData.hasMoleculeDisplay; // trying to generate a surface without a molecule would generate an error
			if (GUILayout.Button (new GUIContent ("Generate", "Generate a new surface mesh"))) {

				if(UIData.toggleBfac || showSurface){
					UIData.toggleBfac = false;
					pdbGen = false;
					showSurface = false;
					GameObject[] Existpdbden = GameObject.FindGameObjectsWithTag("pdb2den");
					GameObject[] ExistSurf = GameObject.FindGameObjectsWithTag ("SurfaceManager");
					foreach(GameObject s in ExistSurf)
						GameObject.Destroy(s);
					foreach(GameObject s in Existpdbden)
						GameObject.Destroy(s);					
				}

				if (!pdbGen) {	
					MoleculeModel.surfaceFileExists = true;
					
					GameObject pdb2den = new GameObject("pdb2den OBJ");
					pdb2den.tag = "pdb2den";
					pdb2den.AddComponent<PDBtoDEN>();
					PDBtoDEN generatedensity = pdb2den.GetComponent<PDBtoDEN>();
					
					generatedensity.TranPDBtoDEN ();
					pdbGen = true;
					buildSurface = true;
				}

				if(!showSurface) {
					PDBtoDEN.ProSurface (generateThreshold);
					showSurface = true;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal ();
			GUI.enabled = UIData.hasMoleculeDisplay; // trying to generate a surface without a molecule would generate an error
			if (GUILayout.Button (new GUIContent ("BFactor", "Generate a new surface mesh using b-factors"))) {
				if(showSurface){
					showSurface = false;
					pdbGen = false;
					GameObject[] ExistSurf = GameObject.FindGameObjectsWithTag ("SurfaceManager");
					GameObject[] Existpdbden = GameObject.FindGameObjectsWithTag ("pdb2den");
					foreach(GameObject s in ExistSurf) 
						GameObject.Destroy(s);	
					foreach(GameObject s in Existpdbden)
						GameObject.Destroy(s);	
				}
				
				if (!pdbGen) {	
					MoleculeModel.surfaceFileExists = true;
					UIData.toggleBfac = true;
					
					GameObject pdb2den = new GameObject("pdb2den OBJ");
					pdb2den.tag = "pdb2den";
					pdb2den.AddComponent<PDBtoDEN>();
					PDBtoDEN generatedensity = pdb2den.GetComponent<PDBtoDEN>();
					generatedensity.TranPDBtoDEN ();
					pdbGen = true;
					buildSurface = true;
				}
				if(!showSurface) {
					PDBtoDEN.ProSurface (generateThreshold);
					showSurface = true;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent("Volumetric", "Display volumetric density"))) {
				showVolumetricDensity = !showVolumetricDensity;
				
				GameObject volumObj;
				volumObj = GameObject.FindGameObjectWithTag("Volumetric");
				VolumetricDensity volumetric = null;
				
				if (showVolumetricDensity) {
					volumObj.AddComponent<VolumetricDensity>(); // adding the script
					volumetric = volumObj.GetComponent<VolumetricDensity>();
					volumetric.Init();
				}
				else {
					volumetric = volumObj.GetComponent<VolumetricDensity>();
					volumetric.Clear();
				}
			}
			GUI.enabled = true;
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal();
			GUI.enabled = (buildSurface || MoleculeModel.surfaceFileExists);			
			if (GUILayout.Button (new GUIContent ("Toggle surface", "Toggles surface from visible to hidden and vice versa"))) {
				if (showSurface) {
					showSurface = false;
					showSurfaceCut = false;
					showSurfaceMobileCut = false;
					buildSurfaceDone = false;
					surfaceTextureDone = false;
					GameObject[] SurfaceManager = GameObject.FindGameObjectsWithTag ("SurfaceManager");
					foreach (GameObject Surface in SurfaceManager) {
//						Surface.SetActiveRecursively (false);
//						Surface.SetActive (false);
						Surface.GetComponent<Renderer>().enabled = false;
					}
				} else {
					showSurface = true;
					GameObject[] SurfaceManager = GameObject.FindGameObjectsWithTag ("SurfaceManager");
					foreach (GameObject Surface in SurfaceManager) {
//						Surface.SetActiveRecursively (false);
						Surface.GetComponent<Renderer>().enabled = true;
					}
				}
			}	
			GUILayout.EndHorizontal();


			GUILayout.BeginHorizontal();
			GUI.enabled = true; // otherwise the window might not be draggabl

			MoleculeModel.useHetatmForSurface = UIData.hasMoleculeDisplay && GUILayout.Toggle(MoleculeModel.useHetatmForSurface,
			                                                              new GUIContent("HetAtoms", "Use Hetero atoms for surface calculation"));
			MoleculeModel.useSugarForSurface = UIData.hasMoleculeDisplay && GUILayout.Toggle(MoleculeModel.useSugarForSurface,
			                                                                                  new GUIContent("Sugars", "Use sugar for surface calculation"));
			GUILayout.EndHorizontal();

			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
			
			GUI.enabled = true; // otherwise the window might not be draggable
			GUI.DragWindow();
		} // End of Surface
		

		/// <summary>
		/// Defines the Surface Parameters menu window. The latter is automatically opened when a surface
		/// is generated, in the Surface window.
		/// </summary>
		/// <param name='a'>
		/// Window identifier.
		/// </param>
		public static void SurfaceParams (int a) {
			SetTitle("Parameters");

			
			if (GUILayout.Button (new GUIContent ("Color", "Choose the color of the surface")))
				CreateColorPicker(SurfaceGrayColor,"Surface Color", null);
			
			if (GUILayout.Button (new GUIContent ("Inside color", "Choose the color of the inside of the surface")))
				CreateColorPicker(SurfaceInsideColor, "Surface inside color", null);
	
			if(toggle_NA_HIDE)
				GUI.enabled = false;
			if (GUILayout.Button (new GUIContent ("Use atom color", "Enable/Disable the colors of the atoms on the surface (\"Hide\" must be off)"))) {
				GameObject surfaceManagerObj = GameObject.FindGameObjectWithTag("NewSurfaceManager");
				SurfaceManager surfaceManager = surfaceManagerObj.GetComponent<SurfaceManager>();
				GameObject[] surfaceObjs = GameObject.FindGameObjectsWithTag("SurfaceManager");
//				Debug.Log(surfaceObjs.Length);
				foreach(GameObject surfaceObj in surfaceObjs) {
					if(surfaceObj.GetComponent<Renderer>().material.shader == Shader.Find("Vertex Colored")){
						surfaceObj.GetComponent<Renderer>().material = new Material(Shader.Find("Mat Cap Cut"));
						surfaceObj.GetComponent<Renderer>().material.SetTexture("_MatCap", (Texture)Resources.Load ("lit_spheres/divers/daphz1"));
					}
					else{
//						surfaceObj.renderer.material = new Material(Shader.Find("Vertex Colored"));
						surfaceManager.InitTree();
						surfaceManager.ColorVertices();
						// Init and ColorVertice already change the shader of all the SurfaceOBJ. So if you continue the foreach
						// you'll find "VertexColored" for the next shaders and it will replace them by MatCapCut
						break; // So you must break...
					}
				}
			}
			GUI.enabled = true;

			if (GUILayout.Button (new GUIContent ("Use chain color", "Enable/Disable the colors of the chain on the surface (\"Hide\" must be off)"))) {
				UIData.surfColChain = !UIData.surfColChain;

				if(UIData.surfColChain){
					GameObject surfaceManagerObj = GameObject.FindGameObjectWithTag("NewSurfaceManager");
					SurfaceManager surfaceManager = surfaceManagerObj.GetComponent<SurfaceManager>();
					GameObject[] surfaceObjs = GameObject.FindGameObjectsWithTag("SurfaceManager");
					//				Debug.Log(surfaceObjs.Length);
					foreach(GameObject surfaceObj in surfaceObjs) {
						if(surfaceObj.GetComponent<Renderer>().material.shader == Shader.Find("Vertex Colored")){
							surfaceObj.GetComponent<Renderer>().material = new Material(Shader.Find("Mat Cap Cut"));
							surfaceObj.GetComponent<Renderer>().material.SetTexture("_MatCap", (Texture)Resources.Load ("lit_spheres/divers/daphz1"));
						}
						else{
							//surfaceObj.renderer.material = new Material(Shader.Find("Vertex Colored"));
							surfaceManager.InitTree();
							surfaceManager.ColorVertices();
							// Init and ColorVertice already change the shader of all the SurfaceOBJ. So if you continue the foreach
							// you'll find "VertexColored" for the next shaders and it will replace them by MatCapCut
							break; // So you must break...
						}
					}
					UIData.surfColChain = false;
				}
			}

			if (GUILayout.Button (new GUIContent ("Hydrophobic scale", "Open Hydrophobic scales Menu"))) {
				UI.GUIMoleculeController.showHydroMenu = !UI.GUIMoleculeController.showHydroMenu;
			}

			if (GUILayout.Button (new GUIContent ("Use properties color", "Show amino acids properties (red/acid ; blue/basic ; green/polar ; white/apolar) (\"Hide\" must be off)"))) {
				UIData.surfColPChim = !UIData.surfColPChim;
				
				if(UIData.surfColPChim){
					GameObject surfaceManagerObj = GameObject.FindGameObjectWithTag("NewSurfaceManager");
					SurfaceManager surfaceManager = surfaceManagerObj.GetComponent<SurfaceManager>();
					GameObject[] surfaceObjs = GameObject.FindGameObjectsWithTag("SurfaceManager");
					//				Debug.Log(surfaceObjs.Length);
					foreach(GameObject surfaceObj in surfaceObjs) {
						if(surfaceObj.GetComponent<Renderer>().material.shader == Shader.Find("Vertex Colored")){
							surfaceObj.GetComponent<Renderer>().material = new Material(Shader.Find("Mat Cap Cut"));
							surfaceObj.GetComponent<Renderer>().material.SetTexture("_MatCap", (Texture)Resources.Load ("lit_spheres/divers/daphz1"));
						}
						else{
							//surfaceObj.renderer.material = new Material(Shader.Find("Vertex Colored"));
							surfaceManager.InitTree();
							surfaceManager.ColorVertices();
							// Init and ColorVertice already change the shader of all the SurfaceOBJ. So if you continue the foreach
							// you'll find "VertexColored" for the next shaders and it will replace them by MatCapCut
							break; // So you must break...
						}
					}
					UIData.surfColPChim = false;
				}
			}

			if (GUILayout.Button (new GUIContent ("Use BFactor color", "Show B-Factor values (\"Hide\" must be off)"))) {
				UIData.surfColBF = !UIData.surfColBF;
				Debug.Log ("ColBF " + UIData.surfColBF);
				
				if(UIData.surfColBF){
					GameObject surfaceManagerObj = GameObject.FindGameObjectWithTag("NewSurfaceManager");
					SurfaceManager surfaceManager = surfaceManagerObj.GetComponent<SurfaceManager>();
					GameObject[] surfaceObjs = GameObject.FindGameObjectsWithTag("SurfaceManager");
					//				Debug.Log(surfaceObjs.Length);
					foreach(GameObject surfaceObj in surfaceObjs) {
						if(surfaceObj.GetComponent<Renderer>().material.shader == Shader.Find("Vertex Colored")){
							surfaceObj.GetComponent<Renderer>().material = new Material(Shader.Find("Mat Cap Cut"));
							surfaceObj.GetComponent<Renderer>().material.SetTexture("_MatCap", (Texture)Resources.Load ("lit_spheres/divers/daphz1"));
						}
						else{
							//surfaceObj.renderer.material = new Material(Shader.Find("Vertex Colored"));
							surfaceManager.InitTree();
							surfaceManager.ColorVertices();
							// Init and ColorVertice already change the shader of all the SurfaceOBJ. So if you continue the foreach
							// you'll find "VertexColored" for the next shaders and it will replace them by MatCapCut
							break; // So you must break...
						}
					}
					UIData.surfColBF = false;
				}


			}

				
			if (GUILayout.Button (new GUIContent ("Texture", "Choose the texture of the surface"))) 
				showSurfaceTexture = !showSurfaceTexture;
			
			if (GUILayout.Button (new GUIContent ("Static cut", "Activate a static cut plane on the surface"))) {
				if (surfaceStaticCut) {
					surfaceStaticCut = false;
					showSurfaceCut = false;
				} else {
					surfaceMobileCut = false;
					showSurfaceMobileCut = false;
					surfaceStaticCut = true;
					showSurfaceCut = true;
				}
			}
			
			if (GUILayout.Button (new GUIContent ("Mobile cut", "Activate a mobile cut plane on the surface"))) {
				if (surfaceMobileCut) {
					surfaceMobileCut = false;
					showSurfaceMobileCut = false;
				} else {
					surfaceStaticCut = false;
					showSurfaceCut = false;
					surfaceMobileCut = true;
					showSurfaceMobileCut = true;
				}
			}
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Brightness: " + SurfaceManager.brightness.ToString("0.00"));
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			int sliderWidth = (int) (Rectangles.surfaceMenuWidth * 0.9f);
			SurfaceManager.brightness = LabelSlider(SurfaceManager.brightness, 0.33f, 2.0f, "",
										"Adjust the brightness of the surface", true, sliderWidth, 0, false);
			if(GUI.changed)
				SurfaceManager.resetBrightness = true;			
			GUILayout.EndHorizontal();
			
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Color weight: " + SurfaceManager.colorWeight.ToString("0.00"));
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			SurfaceManager.colorWeight = LabelSlider(SurfaceManager.colorWeight, 0f, 1f, "",
										"Adjust the weight of the vertex colors of the surface", true, sliderWidth, 0, false);
			if(GUI.changed)
				SurfaceManager.resetColorWeight = true;			
			GUILayout.EndHorizontal();
			
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
			
			GUI.DragWindow();
		} // End of Surface Params

		public static void HydroMenu (int a) {
			SetTitle("Hydrophobic scales");

			if (GUILayout.Button (new GUIContent ("Kyte & Doolittle", "Surface coloring by using Kyte and Doolittle hydrophobic scale"))) {
				UIData.surfColHydroKD = !UIData.surfColHydroKD;
				
				if(UIData.surfColHydroKD){
					GameObject surfaceManagerObj = GameObject.FindGameObjectWithTag("NewSurfaceManager");
					SurfaceManager surfaceManager = surfaceManagerObj.GetComponent<SurfaceManager>();
					GameObject[] surfaceObjs = GameObject.FindGameObjectsWithTag("SurfaceManager");
					foreach(GameObject surfaceObj in surfaceObjs) {
						if(surfaceObj.GetComponent<Renderer>().material.shader == Shader.Find("Vertex Colored")){
							surfaceObj.GetComponent<Renderer>().material = new Material(Shader.Find("Mat Cap Cut"));
							surfaceObj.GetComponent<Renderer>().material.SetTexture("_MatCap", (Texture)Resources.Load ("lit_spheres/divers/daphz1"));
						}
						else{
							//surfaceObj.renderer.material = new Material(Shader.Find("Vertex Colored"));
							surfaceManager.InitTree();
							surfaceManager.ColorVertices();
							// Init and ColorVertice already change the shader of all the SurfaceOBJ. So if you continue the foreach
							// you'll find "VertexColored" for the next shaders and it will replace them by MatCapCut
							break; // So you must break...
						}
					}
					UIData.surfColHydroKD = false;
				}
			}

			if (GUILayout.Button (new GUIContent ("Engleman & al.", "Surface coloring by using Engleman & al hydrophobic scale"))) {
				UIData.surfColHydroEng = !UIData.surfColHydroEng;
				
				if(UIData.surfColHydroEng){
					GameObject surfaceManagerObj = GameObject.FindGameObjectWithTag("NewSurfaceManager");
					SurfaceManager surfaceManager = surfaceManagerObj.GetComponent<SurfaceManager>();
					GameObject[] surfaceObjs = GameObject.FindGameObjectsWithTag("SurfaceManager");
					foreach(GameObject surfaceObj in surfaceObjs) {
						if(surfaceObj.GetComponent<Renderer>().material.shader == Shader.Find("Vertex Colored")){
							surfaceObj.GetComponent<Renderer>().material = new Material(Shader.Find("Mat Cap Cut"));
							surfaceObj.GetComponent<Renderer>().material.SetTexture("_MatCap", (Texture)Resources.Load ("lit_spheres/divers/daphz1"));
						}
						else{
							//surfaceObj.renderer.material = new Material(Shader.Find("Vertex Colored"));
							surfaceManager.InitTree();
							surfaceManager.ColorVertices();
							// Init and ColorVertice already change the shader of all the SurfaceOBJ. So if you continue the foreach
							// you'll find "VertexColored" for the next shaders and it will replace them by MatCapCut
							break; // So you must break...
						}
					}
					UIData.surfColHydroEng = false;
				}
			}

			if (GUILayout.Button (new GUIContent ("Eisenberg", "Surface coloring by using Eisenberg scale"))) {
				UIData.surfColHydroEis = !UIData.surfColHydroEis;
				
				if(UIData.surfColHydroEis){
					GameObject surfaceManagerObj = GameObject.FindGameObjectWithTag("NewSurfaceManager");
					SurfaceManager surfaceManager = surfaceManagerObj.GetComponent<SurfaceManager>();
					GameObject[] surfaceObjs = GameObject.FindGameObjectsWithTag("SurfaceManager");
					foreach(GameObject surfaceObj in surfaceObjs) {
						if(surfaceObj.GetComponent<Renderer>().material.shader == Shader.Find("Vertex Colored")){
							surfaceObj.GetComponent<Renderer>().material = new Material(Shader.Find("Mat Cap Cut"));
							surfaceObj.GetComponent<Renderer>().material.SetTexture("_MatCap", (Texture)Resources.Load ("lit_spheres/divers/daphz1"));
						}
						else{
							//surfaceObj.renderer.material = new Material(Shader.Find("Vertex Colored"));
							surfaceManager.InitTree();
							surfaceManager.ColorVertices();
							// Init and ColorVertice already change the shader of all the SurfaceOBJ. So if you continue the foreach
							// you'll find "VertexColored" for the next shaders and it will replace them by MatCapCut
							break; // So you must break...
						}
					}
					UIData.surfColHydroEis = false;
				}
			}

				if (GUILayout.Button (new GUIContent ("White Octanol", "Surface coloring by using White Octanol scale"))) {
					UIData.surfColHydroWO = !UIData.surfColHydroWO;
					
					if(UIData.surfColHydroWO){
						GameObject surfaceManagerObj = GameObject.FindGameObjectWithTag("NewSurfaceManager");
						SurfaceManager surfaceManager = surfaceManagerObj.GetComponent<SurfaceManager>();
						GameObject[] surfaceObjs = GameObject.FindGameObjectsWithTag("SurfaceManager");
						foreach(GameObject surfaceObj in surfaceObjs) {
							if(surfaceObj.GetComponent<Renderer>().material.shader == Shader.Find("Vertex Colored")){
								surfaceObj.GetComponent<Renderer>().material = new Material(Shader.Find("Mat Cap Cut"));
								surfaceObj.GetComponent<Renderer>().material.SetTexture("_MatCap", (Texture)Resources.Load ("lit_spheres/divers/daphz1"));
							}
							else{
								//surfaceObj.renderer.material = new Material(Shader.Find("Vertex Colored"));
								surfaceManager.InitTree();
								surfaceManager.ColorVertices();
								// Init and ColorVertice already change the shader of all the SurfaceOBJ. So if you continue the foreach
								// you'll find "VertexColored" for the next shaders and it will replace them by MatCapCut
								break; // So you must break...
							}
						}
						UIData.surfColHydroWO = false;
					}
			}

		} // End of HydroMenu
		
		

		/// <summary>
		/// Defines the Background selection window.
		/// It is automatically opened when the background is toggled, from the Display menu.
		/// </summary>
		/// <param name='a'>
		/// Window identifier.
		/// </param>
		public static void Background (int a) {
			showBackgroundType = SetTitleExit("Background");
			GameObject LocCamera = GameObject.Find ("Camera");

			GUILayout.BeginHorizontal ();

			if (GUILayout.Button (new GUIContent ("1", "Lerpz background")))
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/skyBoxLerpzMaterial");		

			if (GUILayout.Button (new GUIContent ("2", "HotDesert background")))
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/skyBoxHotDesert");

			if (GUILayout.Button (new GUIContent ("3", "Molecule background")))
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/skyBoxmolecularMaterial");
			
			if (GUILayout.Button (new GUIContent ("4", "Snow background")))
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/skyBoxSnow");
						
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("5", "DawnDusk Skybox")))
							LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/DawnDusk Skybox");				

			if (GUILayout.Button (new GUIContent ("6", "Eerie Skybox")))
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/Eerie Skybox");				

			if (GUILayout.Button (new GUIContent ("7", "MoonShine Skybox")))
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/MoonShine Skybox");				

			if (GUILayout.Button (new GUIContent ("8", "Overcast1 Skybox")))
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/Overcast1 Skybox");				

			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			
			if (GUILayout.Button (new GUIContent ("9", "Overcast2 Skybox")))
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/Overcast2 Skybox");

			if (GUILayout.Button (new GUIContent ("10", "StarryNight Skybox")))
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/StarryNight Skybox");				

			if (GUILayout.Button (new GUIContent ("11", "Sunny1 Skybox")))
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/Sunny1 Skybox");				

			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			
			if (GUILayout.Button (new GUIContent ("12", "Sunny2 Skybox")))
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/Sunny2 Skybox");				

			if (GUILayout.Button (new GUIContent ("13", "Sunny3 Skybox")))
				LocCamera.GetComponent<Skybox> ().material = (Material)Resources.Load ("skybox/Sunny3 Skybox");				

			GUILayout.EndHorizontal ();

			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
			
			GUI.DragWindow();
		} // End of Background
		
		

		/// <summary>
		/// Defines the static surface cut window, which is opened from the Surface parameters menu.
		/// </summary>
		/// <param name='a'>
		/// Window identifier.
		/// </param>
		public static void SurfaceCut (int a) {
			showSurfaceCut = SetTitleExit("Cut Parameters");
			surfaceStaticCut = showSurfaceCut; // To disable the cut along with the window.
			int sliderWidth = (int) (Rectangles.surfaceCutWidth * 0.80f);
			
			depthCut = LabelSlider (depthCut, depthCutMin, depthCutMax, "Depth " + depthCut.ToString("0.00"), 
									"Determines cut plane depth position", true, sliderWidth, 1, true); 
			cutX = LabelSlider (cutX, -1f, 1f, " X: " + cutX.ToString("0.00"),
									"Determines cut plane X position", true, sliderWidth, 1, true); 
			cutY = LabelSlider (cutY, -1f, 1f, " Y: " + cutY.ToString("0.00"),
									"Determines cut plane Y position", true, sliderWidth, 1, true); 
			cutZ = LabelSlider (cutZ, -1f, 1f, " Z: " + cutZ.ToString("0.00"),
									"Determines cut plane Z position", true, sliderWidth, 1, true);
			
			GUI.enabled = true;
			GUI.DragWindow();
		}
		
		
		
		/// <summary>
		/// Defines the mobile surface cut window, which is opened from the Surface parameters menu.
		/// </summary>
		/// <param name='a'>
		/// Window identifier.
		/// </param>
		public static void SurfaceMobileCut (int a) {
			int sliderWidth = (int) (Rectangles.surfaceCutWidth * 0.45f);
			SetTitle("Surface Mobile Cut");
			depthCut = LabelSlider (depthCut, -40f, 40f, "Cutting depth " + depthCut,
									"Determines mobile cut plane depth position", true, sliderWidth, 1); 
			adjustFieldLineCut = LabelSlider (adjustFieldLineCut, -100f, 100f, " FL cut :" + adjustFieldLineCut,
									"Determines field line cut position", true, sliderWidth, 1); 
			GUI.DragWindow();
		}
		
		
		
		/// <summary>
		/// Defines the Effect selection window for SSAO, DOF, etc. Opened from the Display menu.
		/// </summary>
		/// <param name='a'>
		/// A.
		/// </param>
		public static void Effects (int a) {
			showEffectType = SetTitleExit("Visual Effects");
			
			GUILayout.BeginHorizontal ();
			toggle_VE_SSAO = GUILayout.Toggle (toggle_VE_SSAO, new GUIContent ("SSAO", "Toggle screen space ambient occlusion effect"));
			if (!toggle_VE_SSAO) { 
				if (Camera.main.GetComponent<SSAOEffect> ().enabled) 
					Camera.main.GetComponent<SSAOEffect> ().enabled = false;
			}
			else { 
				Camera.main.GetComponent<SSAOEffect>().enabled = true;
				Camera.main.GetComponent<SSAOEffect>().m_Radius = 4f;
				Camera.main.GetComponent<SSAOEffect>().m_OcclusionIntensity = 1f;
			}
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
			// Make a toggle for BLUR ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			toggle_VE_BLUR = GUILayout.Toggle (toggle_VE_BLUR, new GUIContent ("BLUR", "Toggle motion blur effect"));
			if (!toggle_VE_BLUR && Camera.main.GetComponent<MotionBlur> ().enabled)
				Camera.main.GetComponent<MotionBlur> ().enabled = false;
			else if (toggle_VE_BLUR && !Camera.main.GetComponent<MotionBlur> ().enabled) { 
				Camera.main.GetComponent<MotionBlur> ().shader = Shader.Find ("Hidden/MotionBlur");
				Camera.main.GetComponent<MotionBlur> ().enabled = true;
			}
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
			// Make a toggle for NOISE :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			toggle_VE2_NOISE = GUILayout.Toggle (toggle_VE2_NOISE, new GUIContent ("NOISE", "Toggle noise effect"));
			if (!toggle_VE2_NOISE && Camera.main.GetComponent<NoiseEffect> ().enabled)
				Camera.main.GetComponent<NoiseEffect> ().enabled = false;
			else if (toggle_VE2_NOISE && !Camera.main.GetComponent<NoiseEffect> ().enabled) { 
				GameObject.FindWithTag ("MainCamera").AddComponent <NoiseEffect>();
				Camera.main.GetComponent<NoiseEffect> ().enabled = true;
				Camera.main.GetComponent<NoiseEffect> ().shaderRGB = Shader.Find ("Hidden/Noise Shader RGB");
				Camera.main.GetComponent<NoiseEffect> ().shaderYUV = Shader.Find ("Hidden/Noise Shader YUV");
				Camera.main.GetComponent<NoiseEffect> ().grainTexture = Resources.Load ("NoiseEffectGrain") as Texture2D;
				Camera.main.GetComponent<NoiseEffect> ().scratchTexture = Resources.Load ("NoiseEffectScratch") as Texture2D;
			}
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
			// Make a toggle for BLUR2 :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			toggle_VE2_BLUR2 = GUILayout.Toggle (toggle_VE2_BLUR2, new GUIContent ("BLUR2", "Toggle overall blur effect"));
			if (!toggle_VE2_BLUR2 && Camera.main.GetComponent<BlurEffect> ().enabled)
				Camera.main.GetComponent<BlurEffect> ().enabled = false;
			else if (toggle_VE2_BLUR2 && !Camera.main.GetComponent<BlurEffect> ().enabled) { 
				Camera.main.GetComponent<BlurEffect> ().blurShader = Shader.Find ("Hidden/BlurEffectConeTap");
				Camera.main.GetComponent<BlurEffect> ().enabled = true;
			}
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal();
			// Make a toggle for DOF :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			toggle_VE_DOF = GUILayout.Toggle (toggle_VE_DOF, new GUIContent ("DOF", "Toggle depth of field effect."));
			if (!toggle_VE_DOF && Camera.main.GetComponent<DepthOfFieldScatter>().enabled) {
				Camera.main.GetComponent<DepthOfFieldScatter>().enabled = false;
				Camera.main.GetComponent<SelectAtomFocus>().enabled = false;
			} else if (toggle_VE_DOF && !Camera.main.GetComponent<DepthOfFieldScatter>().enabled) { 
				GameObject.FindWithTag ("MainCamera").AddComponent <DepthOfFieldScatter>();
				Camera.main.GetComponent<DepthOfFieldScatter>().dofHdrShader = Shader.Find("Hidden/Dof/DepthOfFieldHdr");
				Camera.main.GetComponent<DepthOfFieldScatter>().focalSize = 0;
				Camera.main.GetComponent<DepthOfFieldScatter>().enabled = true ;
				Camera.main.GetComponent<DepthOfFieldScatter>().aperture = 25f;
				Debug.Log(Camera.main.GetComponent<DepthOfFieldScatter>().aperture);
				Camera.main.GetComponent<SelectAtomFocus>().enabled = true;
			}
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
			// Make a toggle for Crease :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			toggle_VE_CREASE = GUILayout.Toggle (toggle_VE_CREASE, new GUIContent ("CREASE", "Toggle crease effect"));
			if (!toggle_VE_CREASE && Camera.main.GetComponent<Crease>().enabled)
				Camera.main.GetComponent<Crease> ().enabled = false;
			else if (toggle_VE_CREASE && !Camera.main.GetComponent<Crease> ().enabled) { 
				GameObject.FindWithTag ("MainCamera").AddComponent <Crease>();
				Camera.main.GetComponent<Crease> ().creaseApplyShader = Shader.Find("Hidden/CreaseApply");
				Camera.main.GetComponent<Crease> ().intensity = 20;
				Camera.main.GetComponent<Crease> ().enabled = true ;
			}
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			// Make a toggle for EDGE :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			toggle_VE2_EDGE = GUILayout.Toggle (toggle_VE2_EDGE, new GUIContent ("EDGE", "Toggle edge detection effect"));
			if (!toggle_VE2_EDGE && Camera.main.GetComponent<EdgeDetectEffect> ().enabled)
				Camera.main.GetComponent<EdgeDetectEffect> ().enabled = false;
			else if (toggle_VE2_EDGE && !Camera.main.GetComponent<EdgeDetectEffect> ().enabled) { 
				GameObject.FindWithTag ("MainCamera").AddComponent <EdgeDetectEffect>();
				Camera.main.GetComponent<EdgeDetectEffect> ().shader = Shader.Find ("Hidden/Edge Detect X");
				Camera.main.GetComponent<EdgeDetectEffect> ().enabled = true;
			}
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
			// Make a toggle for VORTX :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			toggle_VE2_VORTX = GUILayout.Toggle (toggle_VE2_VORTX, new GUIContent ("VORTX", "Toggle vortex deformation effect"));
			if (!toggle_VE2_VORTX && Camera.main.GetComponent<VortexEffect> ().enabled)
				Camera.main.GetComponent<VortexEffect> ().enabled = false;
			else if (toggle_VE2_VORTX && !Camera.main.GetComponent<VortexEffect> ().enabled) { 
				GameObject.FindWithTag ("MainCamera").AddComponent <VortexEffect>();
				Camera.main.GetComponent<VortexEffect> ().shader = Shader.Find ("Hidden/Twist Effect");
				Camera.main.GetComponent<VortexEffect> ().enabled = true;
			}
			GUILayout.EndHorizontal ();

			// Make a combined toggle+button for GRAYS ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			GUILayout.BeginHorizontal ();
			toggle_VE2_GRAYS = GUILayout.Toggle (toggle_VE2_GRAYS, new GUIContent ("GRAYS", "Toggle grayscale color effect"));
			if (!toggle_VE2_GRAYS && Camera.main.GetComponent<GrayscaleEffect> ().enabled)
				Camera.main.GetComponent<GrayscaleEffect> ().enabled = false;
			else if (toggle_VE2_GRAYS && !Camera.main.GetComponent<GrayscaleEffect> ().enabled) { 
				GameObject.FindWithTag ("MainCamera").AddComponent <GrayscaleEffect>();
				Camera.main.GetComponent<GrayscaleEffect> ().enabled = true;
				Camera.main.GetComponent<GrayscaleEffect> ().shader = Shader.Find ("Hidden/Grayscale Effect");
				Camera.main.GetComponent<GrayscaleEffect> ().textureRamp = Resources.Load (ve2_grays_ramps [ve2_grays_rampc]) as Texture2D;
			}
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("Ramp " + ve2_grays_rampc, "Choose among several grayscale ramps"))) {
				ve2_grays_rampc += 1;
				if (ve2_grays_rampc > ve2_grays_rampn)
					ve2_grays_rampc = 0;

				if (toggle_VE2_GRAYS)
					Camera.main.GetComponent<GrayscaleEffect> ().textureRamp = Resources.Load (ve2_grays_ramps [ve2_grays_rampc]) as Texture2D;
			}
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
			// Make a toggle for TWIRL :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			toggle_VE2_TWIRL = GUILayout.Toggle (toggle_VE2_TWIRL, new GUIContent ("TWIRL", "Toggle twirl deformation effect"));
			if (!toggle_VE2_TWIRL && Camera.main.GetComponent<TwirlEffect> ().enabled)
				Camera.main.GetComponent<TwirlEffect> ().enabled = false;
			else if (toggle_VE2_TWIRL && !Camera.main.GetComponent<TwirlEffect> ().enabled) { 
				GameObject.FindWithTag ("MainCamera").AddComponent <TwirlEffect>();
				Camera.main.GetComponent<TwirlEffect> ().shader = Shader.Find ("Hidden/Twirt Effect Shader");
				Camera.main.GetComponent<TwirlEffect> ().enabled = true;
			}
			GUILayout.EndHorizontal ();
			
			
/*
			// Make a combined toggle+button for CCORR ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			GUILayout.BeginHorizontal ();
			toggle_VE2_CCORR = GUILayout.Toggle (toggle_VE2_CCORR, new GUIContent ("CCORR", "Toggle color correction effect"));
			if (!toggle_VE2_CCORR && Camera.main.GetComponent<ColorCorrectionEffect> ().enabled)
				Camera.main.GetComponent<ColorCorrectionEffect> ().enabled = false;
			else if (toggle_VE2_CCORR && !Camera.main.GetComponent<ColorCorrectionEffect> ().enabled) { 
				GameObject.FindWithTag ("MainCamera").AddComponent ("ColorCorrectionEffect");
				Camera.main.GetComponent<ColorCorrectionEffect> ().enabled = true;
				Camera.main.GetComponent<ColorCorrectionEffect> ().shader = Shader.Find ("Hidden/Grayscale Effect");
				Camera.main.GetComponent<ColorCorrectionEffect> ().textureRamp = Resources.Load (ve2_ccorr_ramps [ve2_ccorr_rampc]) as Texture2D;
			}
			if (GUILayout.Button (new GUIContent ("Ramp " + ve2_ccorr_rampc, "Choose among several color correction ramps"))) {
				ve2_ccorr_rampc += 1;
				if (ve2_ccorr_rampc > ve2_ccorr_rampn)
					ve2_ccorr_rampc = 0;

				if (toggle_VE2_CCORR)
					Camera.main.GetComponent<ColorCorrectionEffect> ().textureRamp = Resources.Load (ve2_ccorr_ramps [ve2_ccorr_rampc]) as Texture2D;
			}
			GUILayout.EndHorizontal ();
*/
			
			GUILayout.BeginHorizontal ();
			// Make a toggle for SEPIA :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			toggle_VE2_SEPIA = GUILayout.Toggle (toggle_VE2_SEPIA, new GUIContent ("SEPIA", "Toggle sepia tone color effect"));
			if (!toggle_VE2_SEPIA && Camera.main.GetComponent<SepiaToneEffect> ().enabled)
				Camera.main.GetComponent<SepiaToneEffect> ().enabled = false;
			else if (toggle_VE2_SEPIA && !Camera.main.GetComponent<SepiaToneEffect> ().enabled) { 
				GameObject.FindWithTag ("MainCamera").AddComponent <SepiaToneEffect>();
				Camera.main.GetComponent<SepiaToneEffect> ().shader = Shader.Find ("Hidden/Sepiatone Effect");
				Camera.main.GetComponent<SepiaToneEffect> ().enabled = true;
			}
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
			// Make a toggle for GLOW :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
			toggle_VE2_GLOW = GUILayout.Toggle (toggle_VE2_GLOW, new GUIContent ("GLOW", "Toggle glow effect"));
			if (!toggle_VE2_GLOW && Camera.main.GetComponent<GlowEffect> ().enabled)
				Camera.main.GetComponent<GlowEffect> ().enabled = false;
			else if (toggle_VE2_GLOW && !Camera.main.GetComponent<GlowEffect> ().enabled) { 
				GameObject.FindWithTag ("MainCamera").AddComponent <GlowEffect>();
				Camera.main.GetComponent<GlowEffect> ().compositeShader = Shader.Find ("Hidden/GlowCompose");
				Camera.main.GetComponent<GlowEffect> ().blurShader = Shader.Find ("Hidden/GlowConeTap");
				Camera.main.GetComponent<GlowEffect> ().downsampleShader = Shader.Find ("Hidden/Glow Downsample");
//				Camera.main.GetComponent<GlowEffect>().blurspread = 0.7f;
//				Camera.main.GetComponent<GlowEffect>().bluriterations = 3f;
				Camera.main.GetComponent<GlowEffect> ().glowIntensity = 0.3f;
				Camera.main.GetComponent<GlowEffect> ().enabled = true;
			}
			GUILayout.EndHorizontal ();

			showBackgroundType = false;
			showGrayColor = false;
//			ParamshowFieldLine=false;
			showSurfaceButton = false;
			

//			GUI.Label ( new Rect(120,Screen.height-35,Screen.width-360,20), GUI.tooltip);
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
			
			GUI.DragWindow();
		} // End of Effects
		
		
		
		/// <summary>
		/// Defines the cube/line bond window, that lets users define how wide they want their bonds.
		/// Opened from the Atom appearance menu.
		/// </summary>
		/// <param name='a'>
		/// Window identifier.
		/// </param>
		public static void CubeLineBond (int a){
			SetTitle("Bond Width");

			bondWidth = LabelSlider(bondWidth, 0.00001f, 0.5f, "Width: " + bondWidth.ToString("0.00"), 
					"Determines width of bonds for Cubes and Lines", true, (int)(0.90 * Rectangles.cubeLineBondTypeWidth), 1, true);
			BondCubeUpdate.width = bondWidth;
			LineUpdate.width = bondWidth;
			GUI.enabled = true;
			GUI.DragWindow();
		}
		
		/// <summary>
		/// Returns true if the mouse cursor is lower than the Window title.
		/// This function assumes that the cursor is contained in the window,
		/// it is not to be called in any other case.
		/// </summary>
		/// <returns>
		/// A boolean.
		/// </returns>
		/// <param name='rect'>
		/// This is the rectangle representing the window.
		/// </param>
		/// <param name='mpos'>
		/// This is the *flipped* mouse position. I don't know why, but Unity returns a Vector3 for this, not a Vector2.
		/// And to make things even more fun, the mouse starts at 0,0 in the bottom left corner and increases to
		/// maxWidth,maxHeight in the top right corner, while rectangles start at 0,0 in the top left corner,
		/// and increase to maxW,maxH in the bottom right corner.
		/// So it's a Vector3, and the y axis must be flipped before mpos is fed to this function.
		/// </param>
		private static bool InDeadZone(Rect rect, Vector3 mpos)
		{
			GUIStyle currentStyle = GUI.skin.label;
			GUIContent strContent = new GUIContent("str"); // Creating a GUIContent of type string. Probably not the cleanest way.
			float deadZone = currentStyle.CalcSize(strContent).y; // Getting its height in pixels.
			deadZone *= 3.0f ; // Making it a bit bigger. After all, the title label doesn't start right at the top of the window.
			
			cutPlaneIsDraggable = (mpos.y - rect.yMin < deadZone); // if true, we're in the dead zone
			return cutPlaneIsDraggable;
		}
		
		/// <summary>
		/// Defines the move cut plane window, that lets the user change the cut plane of the surface by clicking or scrolling.
		/// Opened from the Surface menu.
		/// </summary>
		/// <param name='a'>
		/// Window identifier.
		/// </param>
		public static void MoveCutPlane (int a) {
			SetTitle("Move cut plane");
			if(cutPlaneIsDraggable)
				GUI.DragWindow();
			
			Vector3 mousePos = Input.mousePosition;
			mousePos.y = Screen.height - mousePos.y;
			if (Rectangles.movePlaneRect.Contains(mousePos) && showSurfaceCut && showSurfaceMenu 
				&& !InDeadZone(Rectangles.movePlaneRect, mousePos) && GUIUtility.hotControl == 0) 
			{
				if (Input.GetMouseButton (0)) {
					GUIMoleculeController.cutX += Input.GetAxis ("Mouse X") * 1 * 0.02f;
					GUIMoleculeController.cutY -= Input.GetAxis ("Mouse Y") * 1 * 0.02f;
					GUIMoleculeController.cutZ -= Input.GetAxis ("Mouse X") * 1 * 0.02f;
				}
				if (GUIMoleculeController.cutX < -1)
					GUIMoleculeController.cutX = -1;
				if (GUIMoleculeController.cutX > 1)
					GUIMoleculeController.cutX = 1;
				if (GUIMoleculeController.cutY < -1)
					GUIMoleculeController.cutY = -1;
				if (GUIMoleculeController.cutY > 1)
					GUIMoleculeController.cutY = 1;
				if (GUIMoleculeController.cutZ < -1)
					GUIMoleculeController.cutZ = -1;
				if (GUIMoleculeController.cutZ > 1)
					GUIMoleculeController.cutZ = 1;
				GUIMoleculeController.depthCut -= Input.GetAxis ("Mouse ScrollWheel");
//				cutZ +=Input.GetAxis("Mouse X") * 1 * 0.02f;
//				cutZ -=Input.GetAxis("Mouse Y") * 1 * 0.02f;
			}
		}
		
		
		/// <summary>
		/// Just triggers the metaphor menu. Part of the Hyperball Style window.
		/// </summary>
		private static void MetaphorControl () {
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("Metaphor", "Change HyperBalls parameters to values for standard representations")))
				showMetaphorType = !showMetaphorType;
			GUILayout.EndHorizontal ();
			
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
		}
		
		
		/// <summary>
		/// This function defines the GUI components that let users choose the colors used in interactive mode.
		/// When in interactive mode, toggling 'Gray' will turn the molecule gray, and the higher the velocity
		/// of an atom/bond, the darker it will be.
		/// </summary>
		private static void PhysicalChoice () {
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Velocity Colors:");
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
			
			UIData.toggleGray = GUILayout.Toggle (UIData.toggleGray, "Gray");
			if (UIData.toggleGray)
				UIData.toggleColor = false;
			else
				UIData.toggleColor = true;
			
			UIData.toggleColor = GUILayout.Toggle (UIData.toggleColor, "Normal");
			if (UIData.toggleColor)
				UIData.toggleGray = false;
			else
				UIData.toggleGray = true;
			
			GUILayout.EndHorizontal ();
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
		}
		
		
		/// <summary>
		/// Defines the Hyperball Style window. opened from the Atom Appearance menu.
		/// </summary>
		/// <param name='a'>
		/// Window identifier.
		/// </param>
		public static void HyperballStyle (int a) {
			int sliderWidth = (int)(0.40 * Rectangles.hyperballWidth);
			SetTitle("Hyperball Style");
			shrink = LabelSlider (shrink, 0.00001f, 0.99f, "Shrink " + shrink.ToString("0.00"),
				"Determines shrink factor parameter for HyperBalls", true, sliderWidth, 1);
			
//			toggle_HB_RANIM = GUILayout.Toggle (toggle_HB_RANIM, new GUIContent ("HB_RANIM", "Animate radius parameter"));
			linkScale = LabelSlider (linkScale, 0.00001f, 1.0f, "Scale " + linkScale.ToString("0.00"),
				"Determines scale parameter", true, sliderWidth, 1);
			
			depthfactor = LabelSlider (depthfactor, -3.0f, 3.0f, "DFactor " + depthfactor.ToString("0.00"),
				"Determines depth factor for network visualization", MoleculeModel.networkLoaded, sliderWidth, 1);
			GUI.enabled = true;
			
			MetaphorControl ();
			
			if(UIData.atomtype != UIData.AtomType.hyperball){
				GUI.enabled = false;
				toggle_NA_INTERACTIVE = false;
			}
			toggle_NA_INTERACTIVE = GUILayout.Toggle (toggle_NA_INTERACTIVE, new GUIContent ("Interactive mode", "Toggle interactive mode, the physics engine will be used"));
			GUI.enabled = true;
			drag = LabelSlider (drag, 0.00001f, 5f, "Drag " + drag.ToString("0.00"), "Determines PhysX engine drag value", 
									UIData.interactive, sliderWidth, 1);
			spring = LabelSlider (spring, 0.00001f, 20, "Spring " + spring.ToString("0.00"), "Determines PhysX engine spring constant",
									UIData.interactive, sliderWidth, 1);
			PhysicalChoice();
//			toggle_NA_INTERACTIVE=false;
			GUI.enabled = true;	
			
			GUILayout.BeginHorizontal();
			toggle_NA_MEASURE = GUILayout.Toggle (toggle_NA_MEASURE, new GUIContent ("Measure dist.", "Toggle mouse clicking to measure the distance between two atoms"));
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			toggle_DISTANCE_CUEING = GUILayout.Toggle (toggle_DISTANCE_CUEING, new GUIContent ("Dist. cueing", "Toggle distance cueing, which darkens distant objects"));
			GUILayout.EndHorizontal();
			
			if(toggle_DISTANCE_CUEING) {
				if(!distanceCueingEnabled)
					DisplayMolecule.ToggleDistanceCueing(true);
					distanceCueingEnabled = true;
				}
			else
				if(distanceCueingEnabled) {
					DisplayMolecule.ToggleDistanceCueing(false);
					distanceCueingEnabled = false;
				}


			//Alex :
			//TODO : LOD mode buggy with other representation than Hyperball

			GUI.enabled = true;
			if (!toggle_NA_MEASURE && Camera.main.GetComponent<MeasureDistance> ())
				Camera.main.GetComponent<MeasureDistance> ().enabled = false;
			else if (toggle_NA_MEASURE && Camera.main.GetComponent<MeasureDistance> ())
				Camera.main.GetComponent<MeasureDistance> ().enabled = true;
//			GUILayout.EndHorizontal();
			


			//////////////////////modify///////////////////////
			
			if (toggle_NA_INTERACTIVE && UIData.atomtype == UIData.AtomType.hyperball) {
				UIData.interactive = true;
				UIData.resetInteractive = true;
			} else {
				UIData.toggleGray = false;
				UIData.toggleColor = true;
				UIData.interactive = false;
				//UIData.resetInteractive = true;
				if (!MoleculeModel.fieldLineFileExists)
					showGrayColor = false;
				if (!MoleculeModel.surfaceFileExists)
					showSurfaceButton = false;
			}
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
			GUI.DragWindow();
		} // End of HyperballStyle
		
		
		
		
		/// <summary>
		/// Defines the main menu of the GUI.
		/// Controls the main menus.
		/// </summary>
		/// <param name='a'>
		/// Window identifier.
		/// </param>
		public static void MainFun (int a) {
			if(toggle_NA_HIDE)
				Molecule3DComp.HideAtoms();
			else
				Molecule3DComp.ShowAtoms();
			
//			GUILayout.BeginVertical();
			
			GUILayout.BeginHorizontal();
//			if (GUI.Button (new Rect(300,5,40,10),new GUIContent ("Open", "Open the File Open dialogue"))) {
			if (GUILayout.Button (new GUIContent ("File", "Open the File Open dialogue"))) {
				if (showOpenMenu)
					showOpenMenu = false;
				else {
					showOpenMenu = true;
					showAtomMenu = false;
					showSurfaceMenu = false;
					showBfactorMenu = false;
					showElectrostaticsMenu = false;
					fieldLineColorGradient = false;
					showManipulatorMenu = false;
					showSetAtomScales = false;
					showPanelsMenu = false;
					GUIDisplay.m_texture = false;
					m_colorPicker = null;
					showSurfaceButton = false;
					showBackgroundType = false;
					showSurfaceCut = false;
					showSurfaceMobileCut = false;
					showSurfaceTexture = false;
					showAtomsExtendedMenu = false;
					showResiduesMenu = false;
					showChainsMenu = false;
					GUIMoleculeController.toggle_NA_SWITCH = false;
					showSugarChainMenu=false; //T TEST
					showVRPNMenu = false;
					showMDDriverMenu = false;
				}

			}
			
			if(!UIData.hasMoleculeDisplay)
				GUI.enabled = false;
			if (GUILayout.Button (new GUIContent ("Atoms", "Open the Atom appearance dialogue"))) {
				if (showAtomMenu) { // already open, we close it
					showAtomMenu = false;
					GUIDisplay.m_texture = false ; // this is pointless when the atom menu is closed
					showSetAtomScales = false;
				} else {
					showAtomMenu = true;
					showOpenMenu = false;
				}
			}
			
			if(GUILayout.Button(new GUIContent("Sec. Structures", "Open the secondary structures dialogue")))
				showSecStructMenu = !showSecStructMenu;
			
			if (GUILayout.Button (new GUIContent ("Surface", "Open the Surface rendering dialogue"))) {
				if (showSurfaceMenu) {
					showSurfaceMenu = false;
//					showSurfaceCut=false;
//					showSurfaceMobileCut=false;
					showSurfaceTexture = false;
				} else {
					showSurfaceMenu = true;
					showBfactorMenu = false;
					showOpenMenu = false;

				}
				if (!UIData.toggleSurf) {
					UIData.toggleBfac = false;
					UIData.toggleSurf = true;
					pdbGen = false;
				}
			}
			//No bfactor option in this version
/*			if (GUI.Button (new Rect (240 + 23, 2, 80, 20), new GUIContent ("Bfactor", "Open the Bfactor settings dialogue"))) 
			{
				if (showBfactorMenu) {
					showBfactorMenu = false;
			 		showSurfaceCut = false;
			 		showSurfaceMobileCut = false;
			 		m_colorpick_fieldline = null;	
			 		showSurfaceTexture = false;
			 		;
			 		m_colorpick_Surface = null;
				} else {
			 		showBfactorMenu = true;
			 		showSurfaceMenu = false;	
			 		showOpenMenu = false;

				}
			 	if (!UIData.toggleBfac) {
			 		UIData.toggleBfac = true;
			 		UIData.toggleSurf = false;
			 		pdbGen = false;
			 	}
			}
*/
			if(!MoleculeModel.dxFileExists)
				GUI.enabled = false;
			if (GUILayout.Button (new GUIContent ("Electrostat.", "Open the electrostatics field lines dialogue"))) {
				if (showElectrostaticsMenu) {
					showElectrostaticsMenu = false;
					showGrayColor = false;
					m_colorPicker = null ;
				} else {
					showElectrostaticsMenu = true;
					showOpenMenu = false;

				}
			}
			GUI.enabled = true ;
			
			if(!UIData.hasMoleculeDisplay)
				GUI.enabled = false;

			if (GUILayout.Button (new GUIContent ("Display", "Open display configuration menu")))
				showManipulatorMenu = !showManipulatorMenu;
			
			if (GUILayout.Button(new GUIContent("Advanced", "Opens the advanced options menu")))
				showAdvMenu = !showAdvMenu ;

			if (GUILayout.Button (new GUIContent ("Guided Nav.", "Opens the guided navigation menu")))
				showGuidedMenu = !showGuidedMenu ;
			
			if (GUILayout.Button(new GUIContent("Sugar", "Opens the Sugar visualisation menu"))) //T TEST
				showSugarChainMenu = !showSugarChainMenu ;
			
			if (GUILayout.Button (new GUIContent("VRPN", "Configure and run a VRPN client"))) {
				showVRPNMenu = !showVRPNMenu;
				if(VRPNManager.vrpnManager == null) {
					Debug.Log ("Creating VRPN Manager");
					VRPNManager.vrpnManager = (GameObject) GameObject.Instantiate(Resources.Load("VRPN/VRPNManager", typeof(GameObject)), Vector3.zero, Quaternion.identity);
					VRPNManager.vrpnManagerScript = VRPNManager.vrpnManager.GetComponent<VRPNManager>();
				}
			}
			
			if (GUILayout.Button (new GUIContent("MDDriver", "Configure and run a molecular dynamics simulation"))) {
				showMDDriverMenu = !showMDDriverMenu;
			}
			
			if (GUILayout.Button (new GUIContent("Reset", "Resets the molecule to its original position"))) {
				maxCamera fixeCam;
				fixeCam = scenecontroller.GetComponent<maxCamera> ();
				fixeCam.ToCenter();
				if(UIData.atomtype == UIData.AtomType.hyperball){
					GameObject hbManagerObj = GameObject.FindGameObjectWithTag("HBallManager");
					HBallManager hbManager = hbManagerObj.GetComponent<HBallManager>();
					hbManager.ResetPositions();
				}
			}
			
			
			GUILayout.EndHorizontal();
			GUI.enabled = true;
			
			//GUILayout.EndVertical();
			
			// generate the cam target.			
			if (!toggle_NA_MAXCAM && scenecontroller.GetComponent<maxCamera> ().enabled) {
				scenecontroller.GetComponent<maxCamera> ().enabled = false;
				scenecontroller.transform.rotation = NA_SCCROT;
				scenecontroller.transform.position = NA_SCCPOS;
			} else if (toggle_NA_MAXCAM && !scenecontroller.GetComponent<maxCamera> ().enabled)
				scenecontroller.GetComponent<maxCamera> ().enabled = true;				
				
			if (!toggle_NA_AUTOMOVE && scenecontroller.GetComponent<maxCamera> ().automove) {
				scenecontroller.GetComponent<maxCamera> ().automove = false;	
			 	Molecule3DComp.toggleFPSLog ();
			} else if (toggle_NA_AUTOMOVE && !scenecontroller.GetComponent<maxCamera> ().automove) {
			 	scenecontroller.GetComponent<maxCamera> ().automove = true;
			 	Molecule3DComp.toggleFPSLog ();
			}
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;

		} // End of MainFun
		
		/// <summary>
		/// Defines the menu for handling secondary structures (with ribbons)
		/// </summary>
		/// <param name='a'>
		/// Window identifier
		/// </param>
		public static void SecStructMenu(int a) {
			showSecStructMenu = SetTitleExit("Secondary Structures");
			bool ssToggled = toggle_SEC_STRUCT;

			GUILayout.BeginHorizontal();
			GUILayout.Box("Secondary structures");
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			toggle_SEC_STRUCT = UIData.hasMoleculeDisplay && GUILayout.Toggle(toggle_SEC_STRUCT,
				new GUIContent("Enable Secondary structures", "Switch between all-atoms and secondary structures representation"));
			if(!ssToggled && toggle_SEC_STRUCT) { // enabling the ribbons
				Ribbons ribbons = new Ribbons();
				ribbons.CreateRibbons();
				toggle_NA_HIDE = !toggle_NA_HIDE;
			} else {
				if (ssToggled && !toggle_SEC_STRUCT) { // destroying the ribbons
					toggle_NA_HIDE = !toggle_NA_HIDE;
					GameObject[] Objs = Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];;
					foreach(GameObject ribObj in Objs){
						if(ribObj.name == "Ribbons")
						//foreach(GameObject ribObj in ribbonObjs)
							GameObject.Destroy(ribObj);
					}
				}
			}			
			GUILayout.EndHorizontal();

			// Bugs otherwise.
			if(!UIData.hasMoleculeDisplay) {
				showSecStructMenu = false;
				return;
			}
			
			int labelWidth = (int) (0.45f * Rectangles.secStructMenuWidth);
			int sliderWidth = (int) (0.50f * Rectangles.secStructMenuWidth);
			
			GUILayout.BeginHorizontal();
			Ribbons.ribbonWidth[0] = LabelSlider(Ribbons.ribbonWidth[0], 0.375f, 3.0f,
				"Helix Width: " + Ribbons.ribbonWidth[0].ToString("0.00"), "", true, sliderWidth, labelWidth, true);
			GUILayout.EndHorizontal();

		

			GUILayout.BeginHorizontal();
			Ribbons.ribbonWidth[1] = LabelSlider(Ribbons.ribbonWidth[1], 0.425f, 3.4f,
				"Sheet Width: " + Ribbons.ribbonWidth[1].ToString("0.00"), "", true, sliderWidth, labelWidth, true);
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			Ribbons.ribbonWidth[2] = LabelSlider(Ribbons.ribbonWidth[2], 0.075f, 0.6f,
				"Coil Width: " + Ribbons.ribbonWidth[2].ToString("0.00"), "", true, sliderWidth, labelWidth, true);
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			Ribbons.THICKNESS = LabelSlider(Ribbons.THICKNESS, 0.075f, 0.6f,
				"Thickness: " + Ribbons.THICKNESS.ToString("0.00"), "", true, sliderWidth, labelWidth, true);
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			Ribbons.HELIX_DIAM = LabelSlider(Ribbons.HELIX_DIAM, 0.45f, 3.6f,
				"Helix diameter: " + Ribbons.HELIX_DIAM.ToString("0.00"), "", true, sliderWidth, labelWidth, true);
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			Ribbons.ARROW_WIDTH = LabelSlider(Ribbons.ARROW_WIDTH, 0f, 3.6f,
				"Arrow width: " + Ribbons.ARROW_WIDTH.ToString("0.00"), "", true, sliderWidth, labelWidth, true);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal ();
			//UIData.ssColStruct = UIData.hasMoleculeDisplay && GUILayout.Toggle(UIData.ssColStruct,
			//new GUIContent("Color by Structure", "Color cartoon representation by structure"));
			if(GUILayout.Button(new GUIContent("Color by ss")))
				UIData.ssColStruct = !UIData.ssColStruct;
			GUILayout.EndHorizontal();

			if(UIData.ssColStruct){
			GUILayout.BeginHorizontal();
			GUILayout.Label("Helix Color :");
			GUILayout.FlexibleSpace();
			if(GUILayout.Button(helixButton,GUILayout.MinWidth(100),GUILayout.MinHeight(20))){
				CreateColorPicker(Ribbons.HELIX_COLOR, "Helix color", null);
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Sheet Color :");
			GUILayout.FlexibleSpace();
			if(GUILayout.Button(sheetButton,GUILayout.MinWidth(100),GUILayout.MinHeight(20))){
				CreateColorPicker(Ribbons.STRAND_COLOR, "Sheet color", null);
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Coil Color :");
			GUILayout.FlexibleSpace();
			if(GUILayout.Button(coilButton,GUILayout.MinWidth(100),GUILayout.MinHeight(20))){
				CreateColorPicker(Ribbons.COIL_COLOR, "Coil color", null);
			}
			GUILayout.EndHorizontal();
			}

			//C.R Test color by chains
			GUILayout.BeginHorizontal ();
			//UIData.ssColChain = UIData.hasMoleculeDisplay && GUILayout.Toggle(UIData.ssColChain,
			//new GUIContent("Color by Chain", "Color cartoon representation by chain"));
			if(GUILayout.Button(new GUIContent("Color by chain")))
				UIData.ssColChain = !UIData.ssColChain;
			GUILayout.EndHorizontal();

			if (UIData.ssColChain) {

				if(UIData.isGLIC){
					GUILayout.BeginHorizontal();
					UIData.ssDivCol = UIData.hasMoleculeDisplay && GUILayout.Toggle(UIData.ssDivCol,
					new GUIContent("Show domains", "Color by domains"));
					GUILayout.EndHorizontal();}

				GUILayout.BeginHorizontal();
				GUILayout.Label("Chain A :");
				if(GUILayout.Button(chainbuttonA,GUILayout.MinWidth(100),GUILayout.MinHeight(20))){
					CreateColorPicker(Ribbons.ChainColorA, "Chain A color", null);
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("Chain B :");
				if(GUILayout.Button(chainbuttonB,GUILayout.MinWidth(100),GUILayout.MinHeight(20))){
					CreateColorPicker(Ribbons.ChainColorB, "Chain B color", null);
				}
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal();
				GUILayout.Label("Chain C :");
				if(GUILayout.Button(chainbuttonC,GUILayout.MinWidth(100),GUILayout.MinHeight(20))){
					CreateColorPicker(Ribbons.ChainColorC, "Chain C color", null);
				}
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal();
				GUILayout.Label("Chain D :");
				if(GUILayout.Button(chainbuttonD,GUILayout.MinWidth(100),GUILayout.MinHeight(20))){
					CreateColorPicker(Ribbons.ChainColorD, "Chain D color", null);
				}
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal();
				GUILayout.Label("Chain E :");
				if(GUILayout.Button(chainbuttonE,GUILayout.MinWidth(100),GUILayout.MinHeight(20))){
					CreateColorPicker(Ribbons.ChainColorE, "Chain E color", null);
				}
				GUILayout.EndHorizontal();
			}
			
			GUILayout.BeginHorizontal();
			GUI.enabled = toggle_SEC_STRUCT;
			if(GUILayout.Button(new GUIContent("Apply changes"))) {
				// Destroying the ribbons
				toggle_NA_HIDE = !toggle_NA_HIDE;
				GameObject[] Objs = Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];;
				foreach(GameObject ribObj in Objs){
					if(ribObj.name == "Ribbons")
						//foreach(GameObject ribObj in ribbonObjs)
						GameObject.Destroy(ribObj);
				}
				// Recreating them
				Ribbons ribbons = new Ribbons();
				ribbons.CreateRibbons();
				toggle_NA_HIDE = !toggle_NA_HIDE;
			}
			GUI.enabled = true;
			GUILayout.EndHorizontal();
			
			// ---------- C-ALPHA TRACE ----------
			GUILayout.BeginHorizontal();
			GUILayout.Box("C-alpha trace");
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			ChooseStructure();
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal();
			ChooseSmoothness();
			GUILayout.EndHorizontal ();	

			//C.R

			GUILayout.BeginHorizontal();
			GUILayout.Box("Bfactor Representation");
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			ChooseStructure_BF();
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal();
			ChooseSmoothness_BF();
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal();
			MinMaxChoice ();
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal();
			SetHighBFSlider ();
			GUILayout.EndHorizontal ();
			
			GUI.DragWindow();
		}
		
		
		/// <summary>
		/// Helper function to fill a texture choice menu with up to 15 boxes.
		/// </summary>
		/// <param name='texDir'>
		/// Texture directory.
		/// </param>
		/// <param name='texList'>
		/// List of textures.
		/// </param>
		/// <param name='texDescr'>
		/// Texture description.
		/// </param>
		private static void textureMenu (string texDir, string[] texList, string texDescr) {
//			GUI.Label (new Rect (0, 0, 290, 20), "Surface Texture - " + texDescr, CentredText);
			showSurfaceTexture = SetTitleExit("Surface Texture - " + texDescr); //, CentredText);
			
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			UIData.grayscalemode = GUILayout.Toggle (UIData.grayscalemode, new GUIContent ("Grayscale", "Use grayscale version of the texture"));
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("<<", "Go to previous series of textures"))) { // cycle through texture sets 
				texture_set--; 
					
				if(onlyBestTextures){
					if(texture_set < 0)
						texture_set = 4; // First 5 pages are best textures (0-4)
				}
				else{
					if(texture_set < 5)
						texture_set = GUIDisplay.textureMenuList.Count - 1;
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

				if (onlyBestTextures) {
					if(texture_set>4) // First 5 pages are best textures (0-4)
						texture_set = 0;
				}
				else{
					if (texture_set > GUIDisplay.textureMenuList.Count - 1)
						texture_set = 5;
				}
			}			
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();			

			// check whether texList has more than 15 entries and raise an error!!
			int i = 0;
//			GUILayout.EndHorizontal(); GUILayout.Box(texDescr); GUILayout.BeginHorizontal();
			foreach (string texFil in texList) {
				i++; 
				if (i > 5) {
					GUILayout.EndHorizontal (); 
					GUILayout.BeginHorizontal (); 
					i = 1;
				}
//				if(GUILayout.Button((Texture2D)Resources.Load(texDir+texFil),GUILayout.Width(50),GUILayout.Height(50))) 
				int buttonWidth = (int) (Rectangles.textureWidth *0.18f);
				int buttonHeight = (int) (Rectangles.textureHeight / 4f);
				if (GUILayout.Button (new GUIContent ((Texture2D)Resources.Load (texDir + texFil), texFil), GUILayout.Width (buttonWidth), GUILayout.Height (buttonHeight))) { 
					if(texFil != "None") {
						surfaceTexture = true;
						externalSurfaceTexture = false;
						surfaceTextureDone = false;
						surfaceTextureName = texDir + texFil;
					}
					else {
						surfaceTexture = true;
						externalSurfaceTexture = false;
						buildSurfaceDone = false;
						surfaceTextureDone = false;
						surfaceTextureName = "lit_spheres/divers/daphz1";
					}
				}	
			}
			GUILayout.EndHorizontal ();
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
		}
		
		
		/// <summary>
		/// Defines the texture selection window.
		/// Negative values of texture_set are used to represent the "best" sets.
		/// </summary>
		/// <param name='a'>
		/// Window identifier.
		/// </param>
		public static void SurfaceTexture (int a) {
			
			textureMenu ("lit_spheres/", GUIDisplay.textureMenuList[texture_set], GUIDisplay.textureMenuTitles[texture_set]);
			
			GUI.DragWindow();
		}	// End of SurfaceTexture				
		
		
		/// <summary>
		/// Defines the rendering parameters, in the atom appearance menu.
		/// </summary>
		private static void RenderingParameters () {
//			toggle_HB_SANIM = GUILayout.Toggle (toggle_HB_SANIM, new GUIContent ("HB_SANIM", "Animate shrink parameter"));
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Color/Texture/Scale"); // Make a little more place between the representation selection and the rendering parameters
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace(); // so as to center the buttons
			if (GUILayout.Button (new GUIContent ("Renderer", "Choose the color, texture and scale of each atom"), GUILayout.Width(Rectangles.atomButtonWidth))){
				showSetAtomScales = !showSetAtomScales;
				showAtomsExtendedMenu = false;
				showResiduesMenu = false;
				showChainsMenu = false;
				GUIDisplay.applyToAtoms.Add("All");
			}
			
			if (GUILayout.Button (new GUIContent ("Panels", "Open colors and textures panels menu"), GUILayout.Width(Rectangles.atomButtonWidth))) {
				showPanelsMenu = !showPanelsMenu;
			}
			
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal();
			GUI.enabled = true;
			toggle_NA_HIDE = GUILayout.Toggle (toggle_NA_HIDE, new GUIContent ("Hide", "Hide/Display atoms")); // && !carbon_alpha?
			GUILayout.EndHorizontal();
			
			globalRadius = LabelSlider (globalRadius, 0.00001f, 2.0f, "Radius " + globalRadius.ToString("0.00"), "Determines radius value", true, (int)(0.90 * Rectangles.atomMenuWidth), 1, true);
			
			
			if (toggle_NA_HBALLSMOOTH) {
				m_colorPicker = null;
				UIData.resetDisplay = true;
				UIData.isCubeToSphere = false;
				UIData.isSphereToCube = true;
				
				UIData.atomtype = UIData.AtomType.hyperball;
				Debug.Log ("UIData.resetDisplay :: " + UIData.resetDisplay);
				Debug.Log ("UIData.isCubeToSphere :: " + UIData.isCubeToSphere);
				Debug.Log ("UIData.isSphereToCube :: " + UIData.isSphereToCube);
				showAtomType = false;
				
				BallUpdate.resetColors = true;
				BallUpdate.resetRadii = true;
				UIData.resetBondDisplay = true;
				UIData.bondtype = UIData.BondType.hyperstick;
				showBondType = false;
				
				globalRadius = 0.4f;
				shrink = 0.5f;
				linkScale = 1.0f;
				
				toggle_NA_HBALLSMOOTH = false;
				UIData.hballsmoothmode = false;
			}
			

			GUILayout.BeginHorizontal();
			if(toggle_NA_HIDE || UIData.atomtype == UIData.AtomType.particleball)
				GUI.enabled = false;
			toggle_NA_SWITCH = GUILayout.Toggle (toggle_NA_SWITCH, new GUIContent 
				("LOD mode", "Toggle LOD.  When this is enabled and the molecule is moving, hyperboloids are replaced by particle balls for smoother framerates."));
			UIData.switchmode = toggle_NA_SWITCH;
			GUI.enabled = true;
			if(toggle_NA_HIDE)
				GUI.enabled = false;
			toggle_NA_AUTOMOVE = GUILayout.Toggle (toggle_NA_AUTOMOVE, new GUIContent ("Automove", "Camera auto rotation"));
			GUI.enabled = true;
			GUILayout.EndHorizontal();
			
			GUI.enabled = true;
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;

		}
		
		private static string structTypeButtonLabel(string st) {
			if(st == "All atoms")
				return("C-alpha trace");
			else
				return("All atoms");
		}

		private static string structTypeButtonLabel_BF(string st) {
			if(st == "All atoms")
				return("B Factor");
			else
				return("All atoms");
		}		
		
		/// <summary>
		/// Induces switch between all-atom and carbon alpha trace representations, as necessary.
		/// </summary>
		private static void ChooseStructure () {
			GUILayout.BeginHorizontal ();
			
			GUI.enabled = (MoleculeModel.CatomsLocationlist.Count > 2);
			if (GUILayout.Button (new GUIContent (structTypeButtonLabel(structType), "Switch to " + structTypeButtonLabel(structType)))) {

				if (UIData.secondarystruct) {
					UIData.secondarystruct = false;
					structType = "All atoms";
					UIData.changeStructure = true;
					globalRadius = 0.40f;
					shrink = 0.50f;
				} else {
					UIData.secondarystruct = true;
					structType = "C-alpha trace";
					if (UIData.toggle_bf){
						AlphaChainSmoother.ReSpline ();
						DisplayMolecule.InitManagers();
						UIData.toggle_bf = false;
					}
					UIData.changeStructure = true;
					globalRadius = 0.25f;
					shrink = 0.0001f;
				}
				DisplayMolecule.DestroyAtomsAndBonds();
				UIData.resetDisplay = true ;
				SetAtomStyle();
			}

			GUI.enabled = true;
			GUILayout.EndHorizontal ();
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
		}

		/// <summary>
		/// Induces switch between all-atom and Bfactor color/radius representation
		/// </summary>
		private static void ChooseStructure_BF () {
			GUILayout.BeginHorizontal ();
			
			GUI.enabled = (MoleculeModel.CatomsLocationlist.Count > 2);
			if (GUILayout.Button (new GUIContent (structTypeButtonLabel_BF(structType), "Switch to " + structTypeButtonLabel_BF(structType)))) {
				if (UIData.secondarystruct) {
					UIData.secondarystruct = false;
					structType = "All atoms";
					UIData.changeStructure = true;
					globalRadius = 0.40f;
					shrink = 0.50f;
				} else {
					UIData.secondarystruct = true;
					structType = "B Factor";
				//	DisplayMolecule.DestroyAtomsAndBonds();
					UIData.toggle_bf = true;
					BFactorRep.CreateBFRep();
					DisplayMolecule.InitManagers();
					UIData.changeStructure = true;
					globalRadius = 0.15f;
					shrink = 0.0001f;

				//	UIData.resetDisplay = true;
				}
				
				//if (Event.current.type == EventType.Repaint)
					//MoleculeModel.newtooltip = GUI.tooltip;
				DisplayMolecule.DestroyAtomsAndBonds();
				UIData.resetDisplay = true ;
				SetAtomStyle();
			}
			GUI.enabled = true;
			GUILayout.EndHorizontal ();
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
		}

		/// <summary>
		/// Chooses the smoothness of the carbon alpha trace(s).
		/// </summary>
		private static void ChooseSmoothness () {
			bool isChain = (structType == "C-alpha trace");
			int labelWidth = (int) (0.35f * Rectangles.secStructMenuWidth);
			int sliderWidth = (int) (0.55f * Rectangles.secStructMenuWidth);
			int newSmooth;
			newSmooth = (int) LabelSlider(GenInterpolationPoint.smoothnessFactor, 1f, 15f,
					"Smoothness", "Smoothness of the carbon alpha chain spline", isChain, sliderWidth, labelWidth, true);
			GUI.enabled = true;
			
			if(newSmooth != GenInterpolationPoint.smoothnessFactor) {
				GenInterpolationPoint.smoothnessFactor = newSmooth;
				DisplayMolecule.DestroyAtomsAndBonds();
				AlphaChainSmoother.ReSpline();
				DisplayMolecule.InitManagers();
				UIData.changeStructure = true;
				UIData.resetDisplay = true;
			}
			
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
		} // End of ChooseSmoothness

		/// <summary>
		/// Chooses the smoothness of the BFactor color/radius Representation.
		/// </summary>
		private static void ChooseSmoothness_BF () {
			bool isChain = (structType == "B Factor");
			int labelWidth = (int) (0.35f * Rectangles.secStructMenuWidth);
			int sliderWidth = (int) (0.55f * Rectangles.secStructMenuWidth);
			int newSmooth;
			newSmooth = (int) LabelSlider(GenInterpolationPoint_BF.smoothnessFactor, 1f, 15f,
			                              "Smoothness", "Smoothness of the Bfactor color/radius Representation", isChain, sliderWidth, labelWidth, true);
			GUI.enabled = true;
			
			if(newSmooth != GenInterpolationPoint_BF.smoothnessFactor) {
				GenInterpolationPoint_BF.smoothnessFactor = newSmooth;
				DisplayMolecule.DestroyAtomsAndBonds();
				BFactorRep.CreateBFRep();
				DisplayMolecule.InitManagers();
				UIData.changeStructure = true;
				UIData.resetDisplay = true;
			}
			
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
		} // End of ChooseSmoothness

		/// <summary>
		/// Display slider setting the radius value for the highest Bfactor
		/// </summary>
		private static void SetHighBFSlider(){

			bool isChain = (structType == "B Factor");
			int labelWidth = (int) (0.35f * Rectangles.secStructMenuWidth);
			int sliderWidth = (int) (0.55f * Rectangles.secStructMenuWidth);

			GUI.enabled = true;

			highBFradius = LabelSlider(highBFradius, 1.0f, 2.0f, "High value radius", "Set highest Bfactor radius value", isChain, sliderWidth, labelWidth, true);
			
			if (GUI.changed) {
				BallUpdate.resetRadii = true;
			}
		}

		/// <summary>
		/// Chooses min and max values to use for BFactor Representation
		/// </summary>
		public static void MinMaxChoice(){

			bool isChain = (structType == "B Factor");
			int textWidth = (int) (0.18f * Rectangles.secStructMenuWidth);
			int buttonWidth = (int) (0.47 * Rectangles.secStructMenuWidth);
			GUI.enabled = isChain;

			GUILayout.Label ("Choose scale");
			GUILayout.Label ("Min");
			//BFactorRep.minval = GUILayout.TextField (BFactorRep.minValue.ToString(), 8, GUILayout.Width (labelWidth));
			BFactorRep.minval = GUILayout.TextField (BFactorRep.minval, 6, GUILayout.Width (textWidth));
			GUILayout.Label ("Max");
			BFactorRep.maxval = GUILayout.TextField (BFactorRep.maxval, 6, GUILayout.Width (textWidth));

			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();

			if (GUILayout.Button (new GUIContent ("Rescaling", "Rescale Bfactor values"), GUILayout.Width (buttonWidth))) {
				UIData.isRescale = true;
				DisplayMolecule.DestroyAtomsAndBonds();
				BFactorRep.CreateBFRep();
				DisplayMolecule.InitManagers ();
				UIData.changeStructure = true;
				UIData.resetDisplay = true;

			}

			if (GUILayout.Button (new GUIContent ("Reset", "Reset to original Bfactor values"), GUILayout.Width (buttonWidth))){
				UIData.isRescale = false;
				DisplayMolecule.DestroyAtomsAndBonds();
				BFactorRep.CreateBFRep();
				DisplayMolecule.InitManagers ();
				UIData.changeStructure = true;
				UIData.resetDisplay = true;
			}
		}

/*	
		private static string hideOrShowAtoms() {
			if (showAtoms)
				return("Hide atoms");
			else
				return("Show atoms");
		}
		
		
		/// <summary>
		/// Toggles the atoms (by modifying their radius, which is less than ideal).
		/// </summary>
		private static void ToggleAtoms() {
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent(hideOrShowAtoms(), "This button enables/disables atoms"))) {
				showAtoms = !showAtoms;
				if(showAtoms)
					globalRadius = prevRadius ;
				else {
					prevRadius = globalRadius ;
					globalRadius = 0f;
				}
			}
			GUILayout.EndHorizontal();
		} // End of ToggleAtoms
*/
		
		
		/// <summary>
		/// Sets the atom style. Calls a few sub-functions that define GUI elements for structure, smoothness, etc.
		/// </summary>
		private static void SetAtomStyle () {			
			
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Atom Style", GUILayout.Width(Rectangles.atomButtonWidth));
			GUILayout.Label("Bond Style", GUILayout.Width(Rectangles.atomButtonWidth));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace();
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

			string displayAtomType;
			if (atomtype == "ParticleBall")
				displayAtomType = "Particle";
			else
				displayAtomType = atomtype;
			if (GUILayout.Button (new GUIContent (displayAtomType, "Change the atom appearance style or rendering method"), GUILayout.Width(Rectangles.atomButtonWidth))) {
				showAtomType = !showAtomType;
				showBondType = false;
				m_colorPicker = null;
			}


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

			
			if (GUILayout.Button (new GUIContent (bondtype, "Change the bond appearance style or rendering method"), GUILayout.Width(Rectangles.atomButtonWidth))) {	
				showBondType = !showBondType;
				showAtomType = false;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal();
			if(toggle_NA_HIDE)
				GUI.enabled = false;
			if(GUILayout.Button(new GUIContent("Smooth HyperBalls", "Set a parameter combo for HyperBalls and Sticks with SmoothLinks once"))) {
				toggle_NA_HBALLSMOOTH = !toggle_NA_HBALLSMOOTH;
				UIData.hballsmoothmode = toggle_NA_HBALLSMOOTH;
			}
			GUILayout.EndHorizontal();
			
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
		} // End of SetAtomStyle
			
		
		/// <summary>
		/// Defines the Atom menu.
		/// </summary>
		/// <param name='a'>
		/// Window identifier.
		/// </param>
		public static void AtomMenu (int a) {
			showAtomMenu = SetTitleExit("Atom appearance");
			SetAtomStyle ();
			RenderingParameters ();
			
			GUILayout.BeginHorizontal();
			toggle_NA_CLICK = GUILayout.Toggle (toggle_NA_CLICK, new GUIContent ("Atom selection", "Toggles mouse clicking to select/deselect atoms (left click/right click)"));
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			toggle_NA_CAMLOCK = GUILayout.Toggle (toggle_NA_CAMLOCK, new GUIContent ("Lock camera", "Enable/Disable camera movements"));
			GUILayout.EndHorizontal();
			
			if (!toggle_NA_CLICK && Camera.main.GetComponent<ClickAtom> ())
				Camera.main.GetComponent<ClickAtom> ().enabled = false;
			else if (toggle_NA_CLICK && Camera.main.GetComponent<ClickAtom> ())
				Camera.main.GetComponent<ClickAtom> ().enabled = true;
			
			
			int sliderWidth = (int) (Rectangles.atomMenuWidth * 0.5f);
			int labelWidth = (int) (Rectangles.atomButtonWidth * 0.4f);
			
			GUILayout.BeginHorizontal();
			HBallManager.brightness = LabelSlider(HBallManager.brightness, 0.33f, 2.0f, "Brightness: " + HBallManager.brightness.ToString("0.00"), 
									"Adjusts the brightness of atoms and bonds represented with the MatCap shader",
									(UIData.atomtype == UIData.AtomType.hyperball), sliderWidth, labelWidth, false);
			if(GUI.changed)
				HBallManager.resetBrightness = true;
			GUILayout.EndHorizontal();
			
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
			GUI.DragWindow();
		} // End of AtomMenu		
		
		
		/// <summary>
		/// Loads the GUI components for taking screenshots in the display window.
		/// </summary>
		private static void LoadScreenShot () {
			GUILayout.BeginHorizontal();
			if (GUILayout.Button (new GUIContent ("ScreenShot", "Capture the screen and save image to the original file path"))) {
				GameObject LocCamera = GameObject.Find ("Camera");
				ScreenShot comp = LocCamera.GetComponent<ScreenShot> ();
				comp.open = true;
			}

			GUILayout.EndHorizontal();
			//////////modify///////////////////////
						
			if (GUILayout.Button (new GUIContent ("ScreenShot Sequence", "Capture the screen sequentially and save images to the original file path"))) {
				GameObject LocCamera = GameObject.Find ("Camera");
				ScreenShot comp = LocCamera.GetComponent<ScreenShot> ();
				comp.sequence = !comp.sequence;
				comp.open = !comp.sequence;				
			}	
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
		} // End of LoadScreenShot
		

		
		/// <summary>
		/// Defines the GUI components that allow for background color control in the display menu.
		/// </summary>
		private static void BackGroundControl () {
			GUILayout.BeginHorizontal ();
			
			GUILayout.Label (new GUIContent ("BackGround", "Toggle the use of a skybox on/off"), GUILayout.MaxWidth (120));
			
			UIData.backGroundIs = GUILayout.Toggle (UIData.backGroundIs, new GUIContent ("Yes", "Toggle the use of a skybox to ON"));
			UIData.backGroundNo = !UIData.backGroundIs;

			UIData.backGroundNo = GUILayout.Toggle (UIData.backGroundNo, new GUIContent ("No", "Toggle the use of a skybox to OFF"));
			UIData.backGroundIs = !UIData.backGroundNo;
			
			GUILayout.EndHorizontal ();
			
			// MB: only show possibility to change skybox if it is set to on
			showBackgroundType = UIData.backGroundIs ;

			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
		}
		

		/// <summary>
		/// Defines the GUI components for setting the BackGround color. Part of the Display window.
		/// </summary>
		private static void BackColor () {
			Camera.main.backgroundColor = new Color (colorRed, colorGreen, colorBlue);
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("White", "Set background to plain white")))
				BackgroundColor.color = new Color(1,1,1,0);
			
			if(GUILayout.Button(new GUIContent ("Grey", "Set background color to grey")))
				BackgroundColor.color = Color.gray;

			if (GUILayout.Button (new GUIContent ("Black", "Set background color to plain black")))
				BackgroundColor.color = Color.black;
			GUILayout.EndHorizontal ();

			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("Background Color", "Choose the background color"))) {
				if (m_colorPicker != null)
					m_colorPicker = null;
					
				m_colorPicker = new ColorPicker(Rectangles.colorPickerRect,	BackgroundColor, null, "All", "All", "Background Color");
			}
			GUILayout.EndHorizontal ();
		}
		
		
		
		/// <summary>
		/// Defines the Display window, opened from the maim menu.
		/// </summary>
		/// <param name='a'>
		/// A.
		/// </param>
		public static void Display (int a) {
			showManipulatorMenu = SetTitleExit("Display");
			// VisualControl ();
			LoadScreenShot ();
			//ShaderControl();
			BackGroundControl ();

			if(showBackgroundType)
				GUI.enabled = false;
			BackColor ();
			GUI.enabled = true;
			
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("Effects", "Toggle what kind of special effect to apply to the scene"))) 
				showEffectType = !showEffectType;
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (new GUIContent ("Infos", "Show/Hide the FPS, atom count and bond count"))) 
				toggle_INFOS = !toggle_INFOS;
			GUILayout.EndHorizontal ();

			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
			GUI.DragWindow();
		} // End of Display
		
		
		/// <summary>
		/// Defines the manipulator window, labaled "Movement" in the program.
		/// Opened by default when a molecule is loaded.
		/// </summary>
		/// <param name='a'>
		/// Window identifier.
		/// </param>
		public static void Manipulator (int a) {
			SetTitle("Molecule Manipulator");
			maxCamera fixeCam;
			fixeCam = scenecontroller.GetComponent<maxCamera> ();
			if (GUILayout.RepeatButton (new GUIContent ("Up", "Move Up")))
				fixeCam.upyDeg ();
			
			GUILayout.BeginHorizontal ();
			if (GUILayout.RepeatButton (new GUIContent ("Left", "Move Left")))
				fixeCam.downxDeg ();

			if (GUILayout.RepeatButton (new GUIContent ("Right", "Move Right")))
				fixeCam.upxDeg ();
			
			GUILayout.EndHorizontal ();

			if (GUILayout.RepeatButton (new GUIContent ("Down", "Move Down")))
				fixeCam.downyDeg ();

			GUILayout.BeginHorizontal ();
			if (GUILayout.RepeatButton (new GUIContent ("Rot Left", "Rotate Left")))
				fixeCam.upzDeg ();

			if (GUILayout.RepeatButton (new GUIContent ("Rot Right", "Rotate Right")))
				fixeCam.downzDeg ();

			GUILayout.EndHorizontal ();
			
			if (Event.current.type == EventType.Repaint)
				MoleculeModel.newtooltip = GUI.tooltip;
			GUI.DragWindow();
		} // End of Manipulator	
	}
}
