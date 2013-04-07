/// @file MoleculeModel.cs
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
/// $Id: MoleculeModel.cs 213 2013-04-06 21:13:42Z baaden $
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

namespace Molecule.Model
{	
	using UnityEngine;
	using System.Collections;
	using Config;

	public class MoleculeModel
	{
		public static ArrayList atomsLocationlist=new ArrayList();//Atoms coordinates
		public static ArrayList CatomsLocationlist=new ArrayList();//CA atoms coordinates

		public static ArrayList atomsTypelist=new ArrayList();	//Type of each atom
		public static ArrayList atomsNamelist=new ArrayList(); //Name of each atom
		public static ArrayList atomsResnamelist=new ArrayList(); //Residue name of each atom
		
		public static ArrayList bondList=new ArrayList();//The list of the bond by position and rotation.
		public static ArrayList bondEPList=new ArrayList();//The list of the bond by the position of the two atoms.
		public static ArrayList CSidList=new ArrayList();
		
		public static ArrayList CSSGDList=new ArrayList();
		public static ArrayList CSRadiusList=new ArrayList();
		public static ArrayList CSColorList=new ArrayList();
		public static ArrayList CSLabelList=new ArrayList();
		
		public static ArrayList FieldLineList= null;
		public static ArrayList FieldLineDist= null;// Field lines distance arrays
		
		public static ArrayList CaSplineList=new ArrayList();
		
		public static ArrayList bondCAList=new ArrayList();
		
		public static ArrayList CaSplineTypeList=new ArrayList();
		public static ArrayList CaSplineChainList=new ArrayList();
		public static ArrayList BFactorList=new ArrayList();

		public static Vector3 target=new Vector3(0f,0f,0f);//
		public static Vector3 cameraLocation=new Vector3(0f,0f,0f);//
		
		public static Vector3 Offset=new Vector3(0f,0f,0f);
		public static Vector3 Center=new Vector3(0f,0f,0f);
		public static Vector3 MinValue= new Vector3(0f,0f,0f);
		public static Vector3 MaxValue= new Vector3(0f,0f,0f);

		// OLD BASIC COLOR SCHEME
//		public static Color oxygenColor=Color.red;
//		public static Color carbonColor=Color.green;	
//		public static Color nitrogenColor=Color.blue;
//		public static Color hydrogenColor=Color.white;					
//		public static Color sulphurColor=Color.yellow;
//		//public static string lodineColor="Purple";
//		//public static string chlorineColor="Green";		
//		public static Color phosphorusColor=new Color(0.6f,0.3f,0.0f,1f);
//		public static Color unknownColor=Color.black;
		// NEW PASTEL COLOR THEME
		public static Color oxygenColor=new Color(0.827f,0.294f,0.333f,1f);
		public static Color carbonColor=new Color(0.282f,0.6f,0.498f,1f);	
		public static Color nitrogenColor=new Color(0.443f,0.662f,0.882f,1f);
		public static Color hydrogenColor=Color.white;					
		public static Color sulphurColor=new Color(1f,0.839f,0.325f,1f);
		public static Color phosphorusColor=new Color(0.960f,0.521f,0.313f,1f);
		public static Color unknownColor=Color.black;
		
		
		public static string oxygenNumber="0";
		public static string carbonNumber="0";
		public static string nitrogenNumber="0";	
		public static string hydrogenNumber="0";	
		public static string sulphurNumber="0";
		//public static string lodineNumber="0";
		//public static string chlorineNumber="0";		
		public static string phosphorusNumber="0";
		public static string unknownNumber="0";
		
		// public static GameObject [] boxes;
		
		public static GameObject[] Oes;		
		public static GameObject[] Ces;
		public static GameObject[] Nes;
		public static GameObject[] Hes;		
		public static GameObject[] Ses;
		//public static GameObject[] Les;
		//public static GameObject[] Cles;
		public static GameObject[] Pes;		
		public static GameObject[] NOes;
		
		public static GameObject[] clubs;
		
		
		//public static float []atomsScaleList={1.72f,1.6f,1.32f,2.08f,2.6f,1f};//c\n\o\s\p\n
		
		public static Vector3 vo=new Vector3(0.66f,0.66f,0.66f);		
		public static Vector3 vc=new Vector3(0.86f,0.86f,0.86f);
		public static Vector3 vn=new Vector3(0.80f,0.80f,0.80f);
		public static Vector3 vh=new Vector3(0.78f,0.78f,0.78f);		
		public static Vector3 vs=new Vector3(1.04f,1.04f,1.04f);
		//public static Vector3 vl=new Vector3(1.95f,1.95f,1.95f);
		//public static Vector3 vcl=new Vector3(0.91f,0.91f,0.91f);		
		public static Vector3 vp=new Vector3(1.30f,1.30f,1.30f);
		public static Vector3 vno=new Vector3(1f,1f,1f);
		
		public static	float oxygenScale=100f;
		public static 	float carbonScale=100f;
		public static 	float nitrogenScale=100f;
		public static 	float hydrogenScale=100f;		
		public static	float sulphurScale=100f;
		//public static	string lodineScale="100";
		//public static	string chlorineScale="100";
		public static	float phosphorusScale=100f;
		public static 	float unknownScale=100f;
		
		public static long atomsnumber=0;
		public static long bondsnumber=0;
		
		public static string FPS="";
		

		public static ArrayList atoms = new ArrayList();
		
		public static Particle[] p;
		
		public static Particle[] fieldlinep;

		public static string newtooltip;
		
		public static bool FieldLineFileExist=false;

		public static bool SurfaceFileExist=false;
		
		public static Vector3[] vertices;
		
		public MoleculeModel()
		{

		}
	}
}