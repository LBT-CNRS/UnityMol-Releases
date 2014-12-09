/// @file UIData.cs
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
/// $Id: UIData.cs 647 2014-08-06 12:20:04Z tubiana $
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
	
	/** !WiP Includes FLAGS of GUI.
	 * Like types of atoms and bounding representation.
	 * Var with //TODO !EXPLANATION! need extra explanation.<BR>
	 * Unity3D Doc :<BR>
	 * <A HREF="http://docs.unity3d.com/Documentation/ScriptReference/Color.html">Color</A>
	 */
	public class UIData {
//		public static bool standalone=true;
		
		//Initial molecule to load from resources
		public static string init_molecule = "";

		#if UNITY_EDITOR
//			public static string server_url = "http://172.27.0.170/";
			public static string server_url = "http://www.shaman.ibpc.fr/umolweb/";
		#else
			public static string server_url = "";
		#endif
//			public static string server_url = "http://localhost:8888/";
		
		
		public static bool fetchPDBFile = false;
		public static bool isConfirm = false;
		public static bool changeStructure = false;
		public static bool hasMoleculeDisplay = false;
		public static bool hasResidues = false;
		public static bool hasChains = false;
		public static bool isclear=false;
		public static bool isOpenFile=false;
		public static bool isParticlesInitialized = false;
		
		public static bool isCubeLoaded = false;
		public static bool isSphereLoaded = false;
		public static bool isHBallLoaded = false;
		
		public static bool isRenderDictInit = false;
		public static bool isTexturesMenuListInit = false;
		
		public static bool readHetAtom = true;
		public static bool readWater = false;
		public static bool connectivity_calc = true;
		public static bool connectivity_PDB = false;
		public static bool resetDisplay=false;
		public static bool isCubeToSphere=false;
		public static bool isSphereToCube=true;
		
		public static bool resetBondDisplay=false;
		
		public static bool toggleMouse = true;
		public static bool toggleKey = false;
			
		public static bool toggleClip =true;
		public static bool togglePlane=false;
		
		public static bool toggleGray =false;
		public static bool toggleColor=true;
		
		public static bool backGroundIs = false;
		public static bool backGroundNo = true;
		
		public static bool cameraStop = false;
		
		public static bool cameraStop2 = false;

		public static bool loginSucess=false;
		
		public static AtomType atomtype=AtomType.particleball;

		public static BondType bondtype=BondType.nobond;
		
		public static bool EnableUpdate=true;
		
		public static bool interactive=false;
		
		public static bool resetInteractive=false;
		
		public static bool meshcombine=false;
		
		public static bool resetMeshcombine=false;
		
		public static bool fileBrowser;
		
		public static bool switchmode=false;
		
		public static bool hballsmoothmode=false;
		
		public static bool grayscalemode = false;
		
		public static bool hiddenUI=false;
		
		public static bool hiddenUIbutFPS=false;
		
		public static bool hiddenCamera=false;

		public static bool up_part=true;
		public static bool down_part=false;
		
		public static bool openAllMenu=false;
		
		public static bool openBound=false;
		
		public static bool secondarystruct=false;
		public static bool toggle_bf = false;
		public static bool isRescale = false;
		public static bool toggle_SS = false;
		public static bool ssColChain = false;
		public static bool ssColStruct = false;
		public static bool ssDivCol = false;
		public static bool surfColChain = false;
		public static bool surfColHydroKD = false;
		public static bool surfColHydroEng = false;
		public static bool surfColHydroWO = false;
		public static bool surfColHydroEis = false;
		public static bool surfColHydroHW = false;
		public static bool surfColPChim = false;
		public static bool surfColBF = false;
		public static bool isGLIC = false;
		public static bool spread_tree = false;

		public static bool firststruct=true;
		
		public static bool toggleSurf=true;
		public static bool toggleBfac=false;

		// Guided navigation mode
		public static bool guided=false;
		// Optimal view mode
		public static bool optim_view=false;
		public static Vector3 optim_view_start_point;
		public static float start_time;
		
		public enum AtomType {
			cube=0,
			sphere=1,	
			hyperball=2,
			raycasting=3,
			billboard=4,
			rcbillboard=5,
			hbbillboard=6,
			rcsprite=7,
			multihyperball=8,
			combinemeshball=9,
			particleball=10,
			particleballalphablend=11,
			noatom = 12
		}
		
		public enum BondType {
			cube=0,
			line=1,
			hyperstick=2,
			tubestick=3,
			bbhyperstick=4,
			particlestick=5,
			nobond=6	
		}

		public enum FFType {
			atomic = 0,
			HiRERNA = 1
		}

		public static bool loadHireRNA = false;
		public static FFType ffType = FFType.atomic;
	}

}
