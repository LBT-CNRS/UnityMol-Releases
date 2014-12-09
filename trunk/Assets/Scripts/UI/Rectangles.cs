 /// @file Rectangles.cs
/// @brief This static class simply contains a collection of rectangles
/// used by the GUI. It exists for the purpose of keeping 
/// GUIMoleculeController relatively tidy.
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

using UnityEngine;
using System.Collections;

public class Rectangles : UI.GUIMoleculeController {

	private static int sWidth = (int) (Screen.width * UI.GUIDisplay.guiScale) ;
	private static int sHeight = (int) (Screen.height * UI.GUIDisplay.guiScale) ;	
	
	private static float BASE_FONT_SIZE = 20f;
	private static float BASE_SCREEN_WIDTH = 1920f;
//	private static float BASE_SCREEN_HEIGHT = 1200f; The aspect ratio is fixed to 16:10 at this time, so this is redundant, and not particularly convenient.

	public static int mainXstart = sWidth / 400;
	public static int mainYstart = sWidth / 400;
	public static int mainWidth =  (int) (sWidth * 0.8f); //695 with bfactor
	public static int mainHeight = sHeight / 20;
	
	public static int helpXstart = mainXstart + mainWidth ;
	public static int helpYstart = mainYstart ;
	public static int helpWidth = sWidth / 50 ;
	public static int helpHeight = mainHeight ;
	
	public static int exitXstart = helpXstart + helpWidth;
	public static int exitYstart = mainYstart ;
	public static int exitWidth = sWidth / 12 ;
	public static int exitHeight = mainHeight ;
		
	// Rectangle for the main menu, the top horizontal bar
	public static Rect mainRect = new Rect (mainXstart, mainYstart, mainWidth, mainHeight) ;
	// Rectangle for the little help button
	public static Rect helpRect = new Rect (helpXstart, helpYstart, helpWidth, helpHeight) ;
	// Rectangle for the little exit button
	public static Rect exitRect = new Rect (exitXstart, exitYstart, exitWidth, exitHeight);
	
	public static int fileBrowserWidth = sWidth / 3;
	public static int fileBrowserXstart = Screen.width - fileBrowserWidth; // aligned to the right
	public static int fileBrowserYstart = mainYstart + mainHeight;
	public static int fileBrowserHeight = (int) (sHeight * 0.60f);
	
	public static Rect fileBrowserRect = new Rect(fileBrowserXstart, fileBrowserYstart, fileBrowserWidth, fileBrowserHeight);
	
	public static int atomScalesWidth = sWidth / 3;
	public static int atomScalesXstart =  Screen.width - atomScalesWidth; // aligned to the right
	public static int atomScalesYstart = mainYstart;
	public static int atomScalesHeight = (int) (sHeight * 0.5f) ;	
	
	// Rectangle for the "atom scales and colors" menu
	public static Rect atomScalesRect = new Rect (atomScalesXstart, atomScalesYstart, atomScalesWidth, atomScalesHeight);
	
	// Rectangle for the Panels Menu
	public static Rect panelsMenuRect = new Rect (atomScalesXstart, Screen.height/2, atomScalesWidth, atomScalesHeight/3);

	public static int openXstart = mainXstart;
	public static int openYstart = mainYstart + mainHeight;
	public static int openWidth = sWidth / 4;
	public static int openHeight = sHeight / 2;
	
	// Rectangle for the open file menu
	public static Rect openRect = new Rect(openXstart, openYstart, openWidth, openHeight);
	
	public static int atomMenuXstart = mainXstart;
	public static int atomMenuYstart = mainYstart + mainHeight;
	public static int atomMenuWidth = (int) (sWidth * 0.14f);
	public static int atomButtonWidth = (int) (Rectangles.atomMenuWidth * 0.45f); // a little less than half the menu
	public static int atomMenuHeight = (int) (sHeight * 0.48f);
	
	// Rectangle for the main atom menu
	public static Rect atomMenuRect = new Rect (atomMenuXstart, atomMenuYstart, atomMenuWidth, atomMenuHeight) ;
	
	public static int atomStyleXstart = mainXstart;
	public static int atomStyleYstart = atomMenuYstart + atomMenuHeight;
	public static int atomStyleWidth = atomMenuWidth;
	public static int atomStyleHeight = (int) (sHeight / 6);
	
	// Rectangle for the atom style selection
	public static Rect atomStyleRect = new Rect(atomStyleXstart, atomStyleYstart, atomStyleWidth, atomStyleHeight);
	
	public static int bondTypeXstart = mainXstart;
	public static int bondTypeYstart = atomMenuYstart + atomMenuHeight;
	public static int bondTypeWidth = atomMenuWidth;
	public static int bondTypeHeight = sHeight / 6;
	
	// Retangle for the bond type selection menu
	public static Rect bondTypeRect = new Rect(bondTypeXstart, bondTypeYstart, bondTypeWidth, bondTypeHeight);
	
	public static int secStructMenuXstart = atomMenuXstart + atomMenuWidth;
	public static int secStructMenuYstart = mainYstart + mainHeight;
	public static int secStructMenuWidth = (int)(sWidth * 0.23f);
	public static int secStructMenuHeight = secStructMenuHeight = (int) (sHeight * 0.8f);
	
	public static Rect secStructMenuRect = new Rect(secStructMenuXstart, secStructMenuYstart, secStructMenuWidth, secStructMenuHeight);
	
	public	static int surfaceMenuXstart = secStructMenuXstart + secStructMenuWidth ;
	public	static int surfaceMenuYstart = mainYstart + mainHeight;
	public	static int surfaceMenuWidth = sWidth / 8;
	public	static int surfaceMenuHeight = sHeight / 4;
	
	// Rectangle for the main surface menu
	public static Rect surfaceMenuRect = new Rect(surfaceMenuXstart, surfaceMenuYstart, surfaceMenuWidth, surfaceMenuHeight);
	
	public static int surfaceParametersXstart = surfaceMenuXstart;
	public static int surfaceParametersYstart = surfaceMenuYstart + surfaceMenuHeight;
	public static int surfaceParametersWidth = surfaceMenuWidth;
	public static int surfaceParametersHeight = (int)(sHeight * 0.50f);

	// Rectangle for the surface parameters menu
	public static Rect surfaceParametersRect = new Rect(surfaceParametersXstart, surfaceParametersYstart, surfaceParametersWidth, surfaceParametersHeight);

	public static int hydroMenuXstart = surfaceMenuXstart + surfaceMenuWidth;
	public static int hydroMenuYstart = surfaceMenuYstart + surfaceMenuHeight + 120;
	public static int hydroMenuWidth = surfaceMenuWidth + 30;
	public static int hydroMenuHeight = (int)(surfaceMenuHeight);

	//Rectangle for the hydrophobic scale menu
	public static Rect hydroMenuRect = new Rect (hydroMenuXstart, hydroMenuYstart, hydroMenuWidth, hydroMenuHeight);
	
	public static int surfaceCutXstart = surfaceParametersXstart;
	public static int surfaceCutYstart = surfaceParametersYstart + surfaceParametersHeight;
	public static int surfaceCutWidth = surfaceParametersWidth;
	public static int surfaceCutHeight = (int) (sHeight * 0.32f);
	
	// Rectangle for the surface static cut menu
	public static Rect surfaceCutRect = new Rect(surfaceCutXstart, surfaceCutYstart, surfaceCutWidth, surfaceCutHeight);
	
	public static int surfaceMobileCutXstart = surfaceCutXstart;
	public static int surfaceMobileCutYstart = surfaceCutYstart;
	public static int surfaceMobileCutWidth = surfaceCutWidth ;
	public static int surfaceMobileCutHeight = sHeight / 6;
	
	// Rectangle for the surface mobile cut menu
	public static Rect surfaceMobileCutRect = new Rect(surfaceMobileCutXstart, surfaceMobileCutYstart, surfaceMobileCutWidth, surfaceMobileCutHeight);
	
	public static int moveCutPlaneWidth = (int)(sWidth * 0.18f); // empty plan, so this is fairly arbitrary
	public static int moveCutPlaneXstart = Screen.width - moveCutPlaneWidth; // aligned  to the right
	public static int moveCutPlaneYstart = mainYstart;
	public static int moveCutPlaneHeight = moveCutPlaneWidth; // square window seems more convenient
	
	// Rectangle for the plane that lets you move the surface cut
	public static Rect movePlaneRect = new Rect(moveCutPlaneXstart, moveCutPlaneYstart, moveCutPlaneWidth, moveCutPlaneHeight);

	public	static int electroMenuXstart = surfaceMenuXstart + surfaceMenuWidth ;
	public	static int electroMenuYstart = mainYstart + mainHeight;
	public	static int electroMenuWidth = sWidth / 8;
	public	static int electroMenuHeight = (int) (sHeight *0.36f);
	
	// Rectangle for the main electrostatics menu
	public 	static Rect electroMenuRect = new Rect (electroMenuXstart, electroMenuYstart, electroMenuWidth, electroMenuHeight);
	
	public static int fieldLinesXstart = electroMenuXstart;
	public static int fieldLinesYstart = electroMenuYstart + electroMenuHeight;
	public static int fieldLinesWidth = electroMenuWidth;
	public static int fieldLinesHeight = (int) (sHeight *0.36f);
	
	// Rectangle for the field lines menu
	public 	static Rect fieldLinesRect = new Rect (fieldLinesXstart, fieldLinesYstart, fieldLinesWidth, fieldLinesHeight);
	
	public static int manipulatorWidth = sWidth / 6;
	public static int manipulatorHeight = (int) (sHeight * 0.30f);
	public static int manipulatorXstart = Screen.width - manipulatorWidth;
	public static int manipulatorYstart = Screen.height - manipulatorHeight;
	
	// Rectangle for the Display menu
	public static Rect manipulatorRect = new Rect(manipulatorXstart, manipulatorYstart, manipulatorWidth, manipulatorHeight);
	
	public static int manipulatorMoveWidth = manipulatorWidth;
	public static int manipulatorMoveXstart = Screen.width - manipulatorMoveWidth;
	private static int manipulatorMoveHeight = (int) (sHeight * 0.22f);
	private static int manipulatorMoveYstart = Screen.height - manipulatorMoveHeight;
	
	// Rectangle for the Molecule Manipulator menu
	public static Rect manipulatorMoveRect = new Rect(manipulatorXstart, manipulatorMoveYstart, manipulatorWidth, manipulatorMoveHeight);
	
	public static int colorPickerWidth = 238;
	public static int colorPickerXstart = Screen.width - colorPickerWidth - atomScalesWidth;
	public static int colorPickerYstart = mainYstart;
	public static int colorPickerHeight = 284;
	
	// Needs a complete overhaul, but it works (sort of).
	public static Rect colorPickerRect = new Rect(colorPickerXstart, colorPickerYstart,	colorPickerWidth, colorPickerHeight);
	
	public static int advOptXstart = electroMenuXstart + electroMenuWidth;
	public static int advOptYstart = mainYstart + mainHeight;
	public static int advOptWidth = sWidth / 4 ;
	public static int advOptHeight = (int) (sHeight *0.30f) ;
	
	// Rectangle for the advanced options menu
	public static Rect advOptionsRect = new Rect(advOptXstart, advOptYstart, advOptWidth, advOptHeight);
	
	public static int cubeLineBondTypeXstart = mainXstart ;
	public static int cubeLineBondTypeYstart = atomMenuYstart + atomMenuHeight;
	public static int cubeLineBondTypeWidth = atomMenuWidth ;
	public static int cubeLineBondTypeHeight = sHeight / 8 ;	
	
	// Sugar Menu
	public static int SugarMenuTypeXstart = atomMenuXstart+atomMenuWidth ;
	public static int SugarMenuTypeYstart = atomMenuYstart;
	public static int SugarMenuTypeWidth = surfaceMenuWidth ;
	public static int SugarMenuTypeHeight = (int) (sHeight / 4) ;
	//Now that we define the rectangle size, we create it.
	public static Rect SugarMenuRect = new Rect(SugarMenuTypeXstart, SugarMenuTypeYstart, SugarMenuTypeWidth, SugarMenuTypeHeight);


	//SugarRibbonsTuneRect Menu
	public static int SugarRibbonsTuneXstart = SugarMenuTypeXstart ;
	public static int SugarRibbonsTuneYstart = SugarMenuTypeYstart + SugarMenuTypeHeight;
	public static int SugarRibbonsTuneWidth = surfaceMenuWidth+75 ;
	public static int SugarRibbonsTuneHeight = (int) (sHeight / 1.45) ;
	//Now that we define the rectangle size, we create it.
	public static Rect SugarRibbonsTuneRect = new Rect(SugarRibbonsTuneXstart, SugarRibbonsTuneYstart, SugarRibbonsTuneWidth, SugarRibbonsTuneHeight);


	//ColorChanges for SugarRibbons Menu
	public static int ColorTuneXstart = SugarMenuTypeXstart + SugarRibbonsTuneWidth  ;
	public static int ColorTuneYstart = SugarRibbonsTuneYstart ;
	public static int ColorTuneWidth = surfaceMenuWidth;
	public static int ColorTuneHeight = (int) (sHeight / 2) ;
	//Now that we define the rectangle size, we create it.
	public static Rect ColorTuneRect = new Rect(ColorTuneXstart, ColorTuneYstart, ColorTuneWidth, ColorTuneHeight);

	// GuidedNavigation Menu
	public static int GuidedNavXstart = surfaceMenuXstart ;
	public static int GuidedNavYstart = surfaceMenuYstart + surfaceMenuHeight;
	public static int GuidedNavWidth = surfaceMenuWidth ;
	public static int GuidedNavHeight = sHeight / 6 ;
//	//Now that we define the rectangle size, we create it.
	public static Rect GuidedNavRect = new Rect(GuidedNavXstart, GuidedNavYstart, GuidedNavWidth, GuidedNavHeight);
	
	// Rectangle for the (cube/line) bond width selection menu
	public	static Rect cubeLineBondRect = new Rect(cubeLineBondTypeXstart,	cubeLineBondTypeYstart, cubeLineBondTypeWidth, cubeLineBondTypeHeight) ;
	
	public static int hyperballXstart = mainXstart;
	public static int hyperballYstart = atomMenuYstart + atomMenuHeight;
	public static int hyperballWidth = atomMenuWidth;
	public static int hyperballHeight = (int) (sHeight * 0.45f);
	
	// Rectangle for the Hyperball Style menu
	public static Rect hyperballRect = new Rect(hyperballXstart, hyperballYstart, hyperballWidth, hyperballHeight);

	public static int textureXstart = exitXstart + exitWidth;
	public static int textureYstart = mainYstart + mainHeight;
	public static int textureWidth = (int) (sWidth * 0.35f) ;
	public static int textureHeight = (int) (sHeight * 0.55f);
	
	// Rectangle for the texture selection menu (for the MatCap shader)
	public static Rect textureRect = new Rect (textureXstart, textureYstart, textureWidth, textureHeight);
	
	public static int metaphorXstart = atomMenuXstart + atomMenuWidth;
	public static int metaphorYstart = atomMenuYstart + atomMenuHeight + 40;
	//public static int metaphorYstart = surfaceCutYstart + surfaceCutHeight;
	public static int metaphorWidth = sWidth / 8;
	public static int metaphorHeight = (int) (sHeight * 0.26f);
	
	// Rectangle for the normal metaphor menu, opened from the Hyperball Style menu.
	public static Rect metaphorRect = new Rect(metaphorXstart, metaphorYstart, metaphorWidth, metaphorHeight);

	public static int effectTypeWidth = sWidth / 9;
	public static int effectTypeHeight = (int) (sHeight * 0.48f);
	public static int effectTypeXstart = Screen.width - effectTypeWidth;
	public static int effectTypeYstart = manipulatorYstart - effectTypeHeight;
	
	// Rectangle for the visual effect selection menu, opened from the Display menu.
	public static Rect effectTypeRect = new Rect(effectTypeXstart, effectTypeYstart, effectTypeWidth, effectTypeHeight);

	public static int backTypeWidth = sWidth / 6;
	public static int backTypeHeight = sHeight / 4;
	public static int backTypeXstart = manipulatorXstart + ( (manipulatorWidth - backTypeWidth) / 2); // Centred on the Display menu.
	public static int backTypeYstart = manipulatorYstart - backTypeHeight; // Aligned to the top of the Display menu.
	
	// Rectangle for the background selection window (skybox).
	public static Rect backTypeRect = new Rect(backTypeXstart, backTypeYstart, backTypeWidth, backTypeHeight);
	
	public static int webHelpXstart = 0;
	public static int webHelpHeight = (int) (Screen.height * 0.05f);
	public static int webHelpYstart = Screen.height - webHelpHeight;
	public static int webHelpWidth = sWidth / 10;
	
	// Rectangle for the button that opens documentation in a web browser (help).
	public static Rect webHelpRect = new Rect(webHelpXstart, webHelpYstart, webHelpWidth, webHelpHeight);
	
	public static int fpsInfosXstart = 0;
	public static int fpsInfosHeight = (int) (Screen.height * 0.05f);
	public static int fpsInfosYstart = (int) (Screen.height - webHelpHeight - fpsInfosHeight);
	public static int fpsInfosWidth = sWidth / 10;
	
	// Rectangle for label containing fps and atom/bond count infos.
	public static Rect fpsInfosRect = new Rect(fpsInfosXstart, fpsInfosYstart, fpsInfosWidth, fpsInfosHeight);
	
	public static int residuesMenuWidth = surfaceMenuWidth + 50;
	public static int residuesMenuXstart = Screen.width - atomScalesWidth - residuesMenuWidth;
	public static int residuesMenuHeight = atomScalesHeight / 4; // Totally arbitrary values
	public static int residuesMenuYstart = atomScalesHeight / 2;
	
	// Rectangle for the menu displaying the different residues
	public static Rect residuesMenuRect = new Rect(residuesMenuXstart, residuesMenuYstart, residuesMenuWidth, residuesMenuHeight);
	
	// Rectangle for the menu displaying the different atoms
	public static Rect atomsExtendedMenuRect = new Rect(residuesMenuXstart, residuesMenuYstart, residuesMenuWidth, residuesMenuHeight);
	
	// Rectangle for the menu displaying the different chains
	public static Rect chainsMenuRect = new Rect(residuesMenuXstart, residuesMenuYstart, residuesMenuWidth, residuesMenuHeight);
	
	// Rectangle and parameters for the VRPN client menu
	public static int vrpnMenuXstart = surfaceMenuXstart / 2;
	public static int vrpnMenuYstart =  surfaceMenuYstart + surfaceMenuHeight;
	public static int vrpnMenuWidth = surfaceMenuWidth;
	public static int vrpnMenuHeight = sHeight / 2;
	public static Rect vrpnMenuRect = new Rect(vrpnMenuXstart, vrpnMenuYstart, vrpnMenuWidth, vrpnMenuHeight);
	
	// Rectangle and parameters for the MDDriver client menu
	public static int mddriverMenuXstart = surfaceMenuXstart / 2;
	public static int mddriverMenuYstart =  surfaceMenuYstart + surfaceMenuHeight;
	public static int mddriverMenuWidth = surfaceMenuWidth;
	public static int mddriverMenuHeight = sHeight / 2;
	public static Rect mddriverMenuRect = new Rect(mddriverMenuXstart, mddriverMenuYstart, mddriverMenuWidth, mddriverMenuHeight);
	
	/// <summary>
	/// Sets the size of the font. Or rather of the fontS, but it sets the same size for every font.
	/// </summary>
	public static void SetFontSize() {
		GUISkin mySkin = GUI.skin; // getting the current skin
		
		// Computing the font size as a function of the screen width (currently proportional to its height)
		// It's probably best to exclude this from the influence of guiScale.
		int fontSize =  (int) ( ((Screen.width / BASE_SCREEN_WIDTH) * BASE_FONT_SIZE) * UI.GUIDisplay.guiScale);
		
		// You'd expect that changing the global font size would conveniently
		// change all the font sizes of the sub-components of the GUI.
		// You'd expect that, but you'd be wrong.
		// Either that or something eludes me. --- Alexandre
		// If some text doesn't scale, its category may need to be added here.
		mySkin.box.fontSize					= fontSize;
		mySkin.button.fontSize				= fontSize;
		mySkin.horizontalSlider.fontSize	= fontSize;
		mySkin.textArea.fontSize			= fontSize;
		mySkin.textField.fontSize			= fontSize;
		mySkin.toggle.fontSize				= fontSize;
		mySkin.window.fontSize				= fontSize;
		mySkin.label.fontSize				= fontSize;
		
		// This is for the file browser, which has its own GUIStyle.
		GUIStyle subStyle = mySkin.customStyles[0] ;
		if (subStyle != null)
			subStyle.fontSize = fontSize;
		
		GUI.skin = mySkin; // putting the skin back
	}
	
	
	/// <summary>
	/// Scales all rectangles according to the new guiScale value input by the user.
	/// X/Y start values, however, are not modified.
	/// </summary>
	public static void Scale() {
		SetFontSize();
		sWidth = (int) (Screen.width * UI.GUIDisplay.guiScale) ;
		sHeight = (int) (Screen.height * UI.GUIDisplay.guiScale) ;
		
		mainWidth =  (int) (sWidth * 0.5f);
		mainHeight = sHeight / 20;
		
		helpXstart = mainXstart + mainWidth ;
		helpYstart = mainYstart ;
		helpWidth = sWidth / 50 ;
		helpHeight = mainHeight ;

		exitWidth = sWidth / 12 ;
		exitHeight = mainHeight ;
		exitXstart = helpXstart + helpWidth;
		exitYstart = mainYstart ;
			

		mainRect = new Rect (mainXstart, mainYstart, mainWidth, mainHeight) ;
		helpRect = new Rect (helpXstart, helpYstart, helpWidth, helpHeight) ;
		exitRect = new Rect (exitXstart, exitYstart, exitWidth, exitHeight);
		
		fileBrowserWidth = sWidth / 3;
		fileBrowserHeight = (int) (sHeight * 0.60f);
		
		fileBrowserRect = new Rect(fileBrowserRect.x, fileBrowserRect.y, fileBrowserWidth, fileBrowserHeight);
		
		atomScalesWidth = sWidth / 3;
		atomScalesHeight = (int) (sHeight * 0.73f) ;	
		

		atomScalesRect = new Rect (atomScalesXstart, atomScalesYstart, atomScalesWidth, atomScalesHeight);
	
		openWidth = sWidth / 4;
		openHeight = sHeight / 2;
		
		openRect = new Rect(openRect.x, openRect.y, openWidth, openHeight);
		
		atomMenuWidth = (int) (sWidth * 0.14f);
		atomButtonWidth = (int) (Rectangles.atomMenuWidth * 0.45f);
		atomMenuHeight = (int) (sHeight * 0.44f);
		
		atomMenuRect = new Rect (atomMenuRect.x, atomMenuRect.y, atomMenuWidth, atomMenuHeight) ;
		
		atomStyleWidth = atomMenuWidth;
		atomStyleHeight = (int) (sHeight / 6);
		
		atomStyleRect = new Rect(atomStyleRect.x, atomStyleRect.y, atomStyleWidth, atomStyleHeight);
		
		secStructMenuWidth = (int)(sWidth * 0.23f);
		secStructMenuHeight = (int) (sHeight * 0.6f);
	
		secStructMenuRect = new Rect(secStructMenuRect.x, secStructMenuRect.y, secStructMenuWidth, secStructMenuHeight);
		
		surfaceMenuWidth = sWidth / 8;
		surfaceMenuHeight = sHeight / 5;
		
		surfaceMenuRect = new Rect(surfaceMenuRect.x, surfaceMenuRect.y, surfaceMenuWidth, surfaceMenuHeight);
		
		surfaceParametersWidth = surfaceMenuWidth;
		surfaceParametersHeight = (int)(sHeight * 0.40f);
		
		surfaceParametersRect = new Rect(surfaceParametersRect.x, surfaceParametersRect.y, surfaceParametersWidth, surfaceParametersHeight);
		
		surfaceCutWidth = surfaceParametersWidth;
		surfaceCutHeight = (int) (sHeight * 0.32f);
		
		surfaceCutRect = new Rect(surfaceCutRect.x, surfaceCutRect.y, surfaceCutWidth, surfaceCutHeight);
		
		surfaceMobileCutWidth = surfaceCutWidth ;
		surfaceMobileCutHeight = sHeight / 6;
		
		surfaceMobileCutRect = new Rect(surfaceMobileCutRect.x, surfaceMobileCutRect.y, surfaceMobileCutWidth, surfaceMobileCutHeight);
		
		moveCutPlaneWidth = (int)(sWidth * 0.18f); // empty plan, so this is fairly arbitrary
		moveCutPlaneHeight = moveCutPlaneWidth; // square window seems more convenient
		
		movePlaneRect = new Rect(movePlaneRect.x, movePlaneRect.y, moveCutPlaneWidth, moveCutPlaneHeight);
	
		electroMenuWidth = sWidth / 8;
		electroMenuHeight = (int) (sHeight *0.36f);
		
		electroMenuRect = new Rect (electroMenuRect.x, electroMenuRect.y, electroMenuWidth, electroMenuHeight);
		
		fieldLinesWidth = electroMenuWidth;
		fieldLinesHeight = (int) (sHeight * 0.36f);
		
		fieldLinesRect = new Rect (fieldLinesRect.x, fieldLinesRect.y, fieldLinesWidth, fieldLinesHeight);
		
		manipulatorWidth = sWidth / 6;
		manipulatorHeight = (int) (sHeight * 0.30f);
		
		manipulatorRect = new Rect(manipulatorRect.x, manipulatorRect.y, manipulatorWidth, manipulatorHeight);
		
		manipulatorMoveWidth = manipulatorWidth;
		manipulatorMoveHeight = (int) (sHeight * 0.22f);

		manipulatorMoveRect = new Rect(manipulatorMoveRect.x, manipulatorMoveRect.y, manipulatorWidth, manipulatorMoveHeight);
		
		colorPickerWidth = 238; //sWidth / 4;
		colorPickerHeight = 308;//(int) (sHeight * 0.50);
		
		colorPickerRect = new Rect(colorPickerRect.x, colorPickerRect.y, colorPickerWidth, colorPickerHeight);
		
		advOptWidth = sWidth / 4 ;
		advOptHeight = (int) (sHeight * 0.30f) ;
		
		advOptionsRect = new Rect(advOptionsRect.x, advOptionsRect.y, advOptWidth, advOptHeight);
		
		cubeLineBondTypeWidth = atomMenuWidth ;
		cubeLineBondTypeHeight = sHeight / 8 ;
		
		cubeLineBondRect = new Rect(cubeLineBondRect.x,	cubeLineBondRect.y, cubeLineBondTypeWidth, cubeLineBondTypeHeight) ;
		
		hyperballWidth = atomMenuWidth;
		hyperballHeight = (int) (sHeight * 0.40f);
		
		hyperballRect = new Rect(hyperballRect.x, hyperballRect.y, hyperballWidth, hyperballHeight);
		
		bondTypeWidth = atomMenuWidth;
		bondTypeHeight = sHeight / 6;
		
		bondTypeRect = new Rect(bondTypeXstart, bondTypeYstart, bondTypeWidth, bondTypeHeight);
	
		textureWidth = sWidth / 3;
		textureHeight = (int) (sHeight * 0.40f);
		
		textureRect = new Rect (textureRect.x, textureRect.y, textureWidth, textureHeight);
		
		metaphorWidth = sWidth / 8;
		metaphorHeight = (int) (sHeight * 0.26f);
		
		metaphorRect = new Rect(metaphorRect.x, metaphorRect.y, metaphorWidth, metaphorHeight);
	
		effectTypeWidth = sWidth / 5;
		effectTypeHeight = (int) (sHeight * 0.38f);
		
		effectTypeRect = new Rect(effectTypeRect.x, effectTypeRect.y, effectTypeWidth, effectTypeHeight);
	
		backTypeWidth = sWidth / 6;
		backTypeHeight = sHeight / 4;
		
		backTypeRect = new Rect(backTypeRect.x, backTypeRect.y, backTypeWidth, backTypeHeight);
	} // End of Scale
	
	
	
}
