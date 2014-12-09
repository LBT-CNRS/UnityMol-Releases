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
/// $Id: MoleculeModel.cs 660 2014-08-26 13:46:34Z sebastien $
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

namespace Molecule.Model {	
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using Config;

	public class MoleculeModel : MonoBehaviour {
		public static string sequence = "";
			
		/// <summary>
		/// The coordinates of each atom. List of float[3].
		/// </summary>
		public static List<float[]> atomsLocationlist = new List<float[]>();
		
		/// <summary>
		/// The coordinates of each atom, simulated through MDDriver. List of float[3].
		/// </summary>
		public static List<Vector3> atomsMDDriverLocationlist;
		
		/// <summary>
		/// The coordinates of each Carbon alpha. List of float[3].
		/// </summary>
		public static List<float[]> CatomsLocationlist = new List<float[]>();

		/// <summary>
		/// Backup of the coordinates of each Carbon alpha. List of float[3].
		/// </summary>
		public static List<float[]> backupCatomsLocationlist = new List<float[]>();
		
		/// <summary>
		/// The coordinates of each Carbon alpha in the CA-Spline. List of float[3].
		/// </summary>
		public static List<float[]> CaSplineList = new List<float[]>();
		
		/// <summary>
		/// The type of each atom. List of AtomModel.
		/// </summary>
		public static List<AtomModel> atomsTypelist = new List<AtomModel>();
		
		/// <summary>
		/// The name of each atom. E.g.: O, N, C, H1, H2, etc. List of strings.
		/// </summary>
		public static List<string> atomsNamelist = new List<string>();


		/// <summary>
		/// The number of each atoms (in the PDB file)
		/// </summary>
		public static List<int> atomsNumberList = new List<int>();

		public static List<string> atomsSugarNamelist = new List<string>();
		public static List<string> atomsSugarResnamelist = new List<string>();
		public static List<string> sugarResname = new List<string> {"ABE","ACE","ALT","API","ARA","DHA","FRU","FUC","GAL",
			"GLC","GUL","IDO","DKN","KDO","MAN","NEG","RHA","RIB","SIA","TAG","TAL","XYL",
			"GLA","FUL","GLB","NAG","NDG","BMA","MMA","A2G","AAL","BGC"};
		public static List<float[]> atomsSugarLocationlist = new List<float[]>();
		public static List<string> resSugarChainList = new List<string>();
		public static List<int[]> bondEPSugarList = new List<int[]>(); // Not sure of what EP means btw
		public static List<AtomModel> atomsSugarTypelist = new List<AtomModel>();
		public static List<int> sortedResIndexByListSugar = new List<int>();

		public static List<int[]> BondListFromPDB = new List<int[]>();

		public static List<string> atomHetTypeList = new List<string>();


		/// <summary>
		/// List of the names existing in the molecule.
		/// </summary>
		public static List<string> existingName = new List<string>();
		
		/// <summary>
		/// The name of the residue to which each atom belongs. E.g.: ALA, LEU, ASP, etc. List of strings.
		/// </summary>
		public static List<string> atomsResnamelist = new List<string>();
		
		/// <summary>
		/// List of the residues existing in the molecule.
		/// </summary>
		public static List<string> existingRes = new List<string>();
			
		/// <summary>
		/// The residue identifiers. One per atom.
		/// </summary>
		public static List<int> residueIds = new List<int>();
		
		/// <summary>
		/// The residues. Keys: residue IDs. Values: list of atoms IDs.
		/// </summary>
		public static Dictionary<int, ArrayList> residues = new Dictionary<int, ArrayList>();
		
		/// <summary>
		/// The chain of each atom.
		/// </summary>
		public static List<string> atomsChainList = new List<string>();

		/// <summary>
		/// The chain of each residue (only work if residues are numbered by chain).
		/// </summary>
		public static List<string> resChainList = new List<string>();

		/// <summary>
		/// The chain of each residue.
		/// </summary>
		public static List<string> resChainList2 = new List<string>();

		/// <summary>
		/// First residue number in pdb.
		/// </summary>
		public static int firstresnb = new int();

		/// <summary>
		/// List of the chains existing in the molecule.
		/// </summary>
		public static List<string> existingChain = new List<string>();
		
		/// <summary>
		/// The color of each atom.
		/// </summary>
		public static List<Color> atomsColorList = new List<Color>();

		public static List<float> atomsLocalScaleList = new List<float>();

		/// <summary>
		/// Terminal residue number of each subunits. 
		/// </summary>
		public static List<int> splits = new List<int>();
		
		/// <summary>
		/// Not used anymore.
		/// The bonds between atoms. Each element of this list is an int[2] where:
		/// int[0] is the index of the first atom,
		/// int[1] is the second one.
		/// </summary>
		public static List<int[]> bondList = new List<int[]>();

		/// <summary>
		/// The bonds between atoms. Each element of this list is an int[2] where:
		/// int[0] is the index of the first atom,
		/// int[1] is the second one.
		/// </summary>
		public static List<int[]> bondEPList = new List<int[]>(); // Not sure of what EP means btw

		/// <summary>
		/// Dictionary of every bounded atoms (which atoms whith which atoms).
		/// </summary>
		public static Dictionary<int, List<int>> bondEPDict= new Dictionary<int, List<int>>();

		public static List<int[]> CSidList = new List<int[]>(); // List of IDs for networks, or something like that.
		public static List<string[]> CSSGDList = new List<string[]>(); // Dunno
		public static List<float[]> CSRadiusList = new List<float[]>(); // Mystery too.
		public static List<string[]> CSColorList = new List<string[]>(); // List of colors, I guess. Probably for networks.
		public static List<string[]> CSLabelList = new List<string[]>(); // And of labels.
		
		public static List<List<Vector3>> FieldLineList= null;
		//public static ArrayList FieldLineDist= null;// Field lines distance arrays // Apparently not used anywhere
		
		//public static ArrayList CaSplineList=new ArrayList();

		/// <summary>
		/// The bonds between carbon alpha in the CA-Spline. Each element of this list is an int[2] where:
		/// int[0] is the index of the first CA,
		/// int[1] is the second one.
		/// </summary>
		public static List<int[]> bondCAList=new List<int[]>();

		/// <summary>
		/// Type of each carbon alpha in the CA-Spline. List of AtomModel.
		/// </summary>
		public static List<AtomModel> CaSplineTypeList = new List<AtomModel>();

		/// <summary>
		/// The chain of each carbon alpha in the CA-Spline.
		/// </summary>
		public static List<string> CaSplineChainList = new List<string>();

		/// <summary>
		/// Sometimes inside pdbs lists are not sorted, and residues mixed
		/// So I had to create this list to sort residues index by chain ID.
		/// </summary>
		public static List<int> sortedResIndexByList = new List<int>();

		/// <summary>
		/// Backup CaSplineChainList (chain of each carbon alpha in the CA-Spline).
		/// Used in ReSpline and BfactorRep
		/// </summary>
		public static List<string> backupCaSplineChainList = new List<string>();

		/// <summary>
		/// Bfactor of each atom.
		/// </summary>
		public static List<float> BFactorList = new List<float>();
		
		/// <summary>
		/// The atoms per ellipsoids per residue for HiRERNA rendering
		/// Key is the residue id
		/// Value is an array of atom ids (3 in any case) parameterizing the ellipsoid
		/// </summary>
		public static Dictionary<int, int[]> atomsForEllipsoidsPerResidue = new Dictionary<int, int[]>();
		
		public static Dictionary<int, int> atomsForEllipsoidsOrientationPerResidue = new Dictionary<int, int>();
		
		/// <summary>
		/// The index (in tables) of the base extremity.
		/// </summary>
		public static List<int> baseIdx = new List<int>();
		
		/// <summary>
		/// RNA Scale parameters
		/// </summary>
		public static List<float> scale_RNA = new List<float>();
		
		/// <summary>
		/// Contiguous (really?) list of ellipsoids
		/// </summary>
		public static List<GameObject> ellipsoids = new List<GameObject>();
		
		/// <summary>
		/// The ellipsoids per residue.
		/// </summary>
		public static Dictionary<int, GameObject> ellipsoidsPerResidue = new Dictionary<int, GameObject>();
		
		public static List<GameObject> bondsForReplacedAtoms = new List<GameObject>();

		public static List<Dictionary<string, Vector3>> residueDictionaries;
		public static List<Dictionary<string, Vector3>> residueDictionariesSugar;

		/// <summary>
		/// List of informations about each helix (extract from the pdb)
		/// float[0] is the first residue of each helix
		/// float[1] is the last residue of each helix
		/// float[2] the length of each helix
		/// float[3] the class of each helix
		/// </summary>
		public static List<float[]> ssHelixList    = new List<float[]> ();

		/// <summary>
		/// First and last residue of each strand (extract from the pdb)
		/// float[0] is the first residue of each strand
		/// float[1] is the last residue of each strand
		/// </summary>
		public static List<float[]> ssStrandList   = new List<float[]> ();

		/// <summary>
		/// The helix chain list (extract from the pdb).
		/// </summary>
		public static List<string> helixChainList  = new List<string>() ;

		/// <summary>
		/// The strand chain list (extract from the pdb).
		/// </summary>
		public static List<string> strandChainList = new List<string> ();
		
		public static Vector3 target=new Vector3(0f,0f,0f);//
		public static Vector3 cameraLocation=new Vector3(10f,10f,10f);//
		
		/// <summary>
		/// The offset for the molecule. The original barycenter of the molecule + this = (0,0,0). Vector3.
		/// Also used for density grids.
		/// </summary>
		public static Vector3 Offset=new Vector3(0f,0f,0f);
		
		/// <summary>
		/// The barycenter of the molecule. Vector3.
		/// </summary>
		public static Vector3 Center=new Vector3(0f,0f,0f);
		
		/// <summary>
		/// The "smallest" corner of the bounding box that encloses the molecule.
		/// </summary>
		public static Vector3 MinValue= new Vector3(0f,0f,0f);
		
		/// <summary>
		/// The "biggest" corner of the bounding box that encloses the molecule.
		/// </summary>
		public static Vector3 MaxValue= new Vector3(0f,0f,0f);
		
		// NEW PASTEL COLOR THEME
		public static ColorObject oxygenColor = new ColorObject(new Color(0.827f,0.294f,0.333f,1f));
		public static ColorObject carbonColor = new ColorObject(new Color(0.282f,0.6f,0.498f,1f));	
		public static ColorObject nitrogenColor = new ColorObject(new Color(0.443f,0.662f,0.882f,1f));
		public static ColorObject hydrogenColor = new ColorObject(Color.white);					
		public static ColorObject sulphurColor = new ColorObject(new Color(1f,0.839f,0.325f,1f));
		public static ColorObject phosphorusColor = new ColorObject(new Color(0.960f,0.521f,0.313f,1f));
		public static ColorObject unknownColor = new ColorObject(Color.black);
//		public static ColorObject selectionColor = new ColorObject(Color.red);
//		public static ColorObject residueColor = new ColorObject(Color.white);
		
		public static Color GetAtomColor(string atomType) {
			switch(atomType) {
			case "O": return oxygenColor.color;
			case "C": return carbonColor.color;
			case "N": return nitrogenColor.color;
			case "H": return hydrogenColor.color;
			case "S": return sulphurColor.color;
			case "P": return phosphorusColor.color;	
			default: return unknownColor.color;
			}
		}
	
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
/*
		public static GameObject[] Oes;		
		public static GameObject[] Ces;
		public static GameObject[] Nes;
		public static GameObject[] Hes;		
		public static GameObject[] Ses;
		//public static GameObject[] Les;
		//public static GameObject[] Cles;
		public static GameObject[] Pes;		
		public static GameObject[] NOes;
*/
		
		public static Dictionary<string, GameObject[]> atomsByChar = new Dictionary<string, GameObject[]>();
		public static ArrayList atoms = new ArrayList();
		
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
		
		public static Particle[] p;
		
		public static Particle[] fieldlinep;

		public static string newtooltip;
		
		public static bool fieldLineFileExists=false;
		
		public static bool dxFileExists = false ; // true if a DX file was found

		public static bool surfaceFileExists=false;

		public static bool useHetatmForSurface = false;

		public static bool useSugarForSurface = false;


		public static bool networkLoaded = false; // set to true when a network is present
		
		public static Vector3[] vertices;
		
		public MoleculeModel() {}
	}
}
