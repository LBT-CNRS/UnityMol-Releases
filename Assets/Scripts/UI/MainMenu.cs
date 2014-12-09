/// @file MainMenu.cs
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
/// $Id: MainMenu.cs 378 2013-09-10 17:18:27Z kouyoumdjian $
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

/** @class MainMenu
	 * This class manages the GUI of the splash screen.
	 * <BR>
	 * Unity3D Doc :<BR>
	 * <A HREF="http://docs.unity3d.com/Documentation/ScriptReference/MonoBehaviour.html">MonoBehaviour</A>
	 */
public class MainMenu : MonoBehaviour {
	
	private Rect rect_mainmenu;/**< 2D Rectangle who contain GUI of the splash screen. Unity3D Doc : <A HREF="http://docs.unity3d.com/Documentation/ScriptReference/Rect.html">Rect</A>  */ 	
		
	/** PDB number. 
		 * This number refers to the number of the Protein, in the Protein Data Base system.<BR>
		 * External links :<BR>
		 * <A HREF="http://pdbbeta.rcsb.org/pdb/home/home.do">PDB website</A><BR>
		 * <A HREF="https://en.wikipedia.org/wiki/Protein_Data_Bank">Wikipedia</A>
		 */
	public static string pdbID;

	// Use this for initialization
	/**
		 * Generic Unity3D method :
		 * This method instantiate the background color object : Camera.mainCamera.backgroundColor and 
		 * the area which the GUI would be placed, and set the variable : pdbID.
		 * <BR>
		 * Unity3D Doc :<BR>
		 * <A HREF="http://docs.unity3d.com/Documentation/ScriptReference/MonoBehaviour.Start.html">Start</A><BR>
		 * <A HREF="http://docs.unity3d.com/Documentation/ScriptReference/Screen.html">Screen</A><BR>
		 * <A HREF="http://docs.unity3d.com/Documentation/ScriptReference/Color.html">Color</A>
		 */
	void Start () {
		Camera.main.backgroundColor = new Color (0.0f, 0.0f, 0.0f);
		rect_mainmenu = new Rect(Screen.width/2 - 150, Screen.height/2 - 50, 300,200);
		pdbID = "";
	}
	/**
		 * Generic Unity3D method : This private method manages and define the GUI, and place every part of them.
		 * <BR>
		 * Unity3D Doc :<BR>
		 * <A HREF="http://docs.unity3d.com/Documentation/ScriptReference/MonoBehaviour.OnGUI.html">OnGUI</A><BR>
		 * <A HREF="http://docs.unity3d.com/Documentation/ScriptReference/Application.html">Application</A>
		 */
	void OnGUI() {
		GUILayout.BeginArea(rect_mainmenu);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("Welcome in the UnityMol Web Demo");
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("Please choose a Scene");
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		if(GUILayout.Button("1KX2"))
			Application.LoadLevel("1KX2");

		if(GUILayout.Button("Fieldlines"))
			Application.LoadLevel("Fieldlines");

		if(GUILayout.Button("Proteins Network"))
			Application.LoadLevel("Network");

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("Or fetch a PDB file from www.pdb.org");
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		pdbID = GUILayout.TextField(pdbID,4,GUILayout.Width(50));
		if(GUILayout.Button("Fetch"))
			Application.LoadLevel("FromPDB");

		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		#if !UNITY_WEBPLAYER
		if(GUILayout.Button("UnityMol Full"))
			Application.LoadLevel("Molecule");

		#endif
		GUILayout.EndArea();
	}
}
