/// @file AtomModel.cs
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
/// $Id: AtomModel.cs 548 2014-06-11 09:51:34Z sebastien $
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
	using System.Collections.Specialized;
	
	public class AtomModel {
		protected string 	_type;		//Atomic Element
		protected float		_radius;	//Radius
		protected float		_scale;		//Scale
		protected Color 	_baseColor;	//Color
		protected string	_baseTexture;//Texture

		public string type {
			get {	return _type;	}
		}

		public float radius {
			get {	return _radius;	}
			set {   _radius = value;	}
		}

		public float scale {
			get {	return _scale;	}
			set {   _scale = value;	}
		}

		public Color baseColor {
			get {	return _baseColor;	}
			set {   _baseColor = value;	}
		}

		public string baseTexture {
			get {	return _baseTexture;	}
			set {   _baseTexture = value;	}
		}

		AtomModel(string type, float radius, float scale, Color col, string texture) {
			_type = type;
			_radius = radius;
			_scale = scale;
			_baseColor = col;
			_baseTexture = texture;
		}

		private static OrderedDictionary s_typeToModel = null;
		
		
		/// <summary>
		/// Changes the color of the chain.
		/// </summary>
		/// <param name='chain'>
		/// Chain to be changed, string.
		/// </param>
		/// <param name='c'>
		/// Color to be applied, type Color.
		/// </param>
		public static void ChangeChainColor(string chain, Color c) {
			((AtomModel)s_typeToModel[chain])._baseColor = c;
		}
		
		
		/// <summary>
		/// Gets the color of the chain.
		/// </summary>
		/// <returns>
		/// The chain color.
		/// </returns>
		/// <param name='chain'>
		/// Chain.
		/// </param>
		public static Color GetChainColor(string chain) {
				return ( ((AtomModel)s_typeToModel[chain])._baseColor ) ;
		}
		
		/// <summary>
		/// Changes the color of the atom type.
		/// </summary>
		/// <param name='atomType'>
		/// Atom type.
		/// </param>
		/// <param name='c'>
		/// New color of the atom type.
		/// </param>
		public static void ChangeAtomColor(string atomType, Color c) {
			((AtomModel)s_typeToModel[atomType])._baseColor = c;	
		}
		
		/// <summary>
		/// Gets the color of the atom type.
		/// </summary>
		/// <returns>
		/// The atom color.
		/// </returns>
		/// <param name='atomType'>
		/// Atom type.
		/// </param>
		public static Color GetAtomColor(string atomType) {
			return ((AtomModel)s_typeToModel[atomType])._baseColor;	
		}
		
		/// <summary>
		/// Determines whether this instance is alive.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this instance is alive; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsAlive() {
			return(s_typeToModel != null);
		}
		
		/// <summary>
		/// Inits the atomic and chains scales.
		/// </summary>
		public static void InitAtomic() {
			s_typeToModel = new OrderedDictionary();

			//Atomic
/*
			// OLD BASIC COLOR SCHEME
			s_typeToModel.Add("O", new AtomModel("O", 1.52f, 100, Color.red, 					null));
			s_typeToModel.Add("C", new AtomModel("C", 1.70f, 100, Color.green, 					null));
			s_typeToModel.Add("N", new AtomModel("N", 1.55f, 100, Color.blue, 					null));
			s_typeToModel.Add("H", new AtomModel("H", 1.20f, 100, Color.white, 					null));
			s_typeToModel.Add("S", new AtomModel("S", 2.27f, 100, Color.yellow,					null));
			s_typeToModel.Add("P", new AtomModel("P", 1.80f, 100, new Color(0.55f,0.44f,0.01f),	null)); //tan
			s_typeToModel.Add("X", new AtomModel("X", 1.40f, 100, Color.black, 					null));	
*/
			// NEW PASTEL COLOR THEME
			s_typeToModel.Add("O", new AtomModel("O", 1.52f, 100, new Color(0.827f,0.294f,0.333f,1f), null));
			s_typeToModel.Add("C", new AtomModel("C", 1.70f, 100, new Color(0.282f,0.6f,0.498f,1f)  , null));
			s_typeToModel.Add("N", new AtomModel("N", 1.55f, 100, new Color(0.443f,0.662f,0.882f,1f), null));
			s_typeToModel.Add("H", new AtomModel("H", 1.20f, 100, Color.white                       , null));
			s_typeToModel.Add("S", new AtomModel("S", 2.27f, 100, new Color(1f,0.839f,0.325f,1f)    , null));
			s_typeToModel.Add("P", new AtomModel("P", 1.80f, 100, new Color(0.960f,0.521f,0.313f,1f), null));
			s_typeToModel.Add("X", new AtomModel("X", 1.40f, 100, Color.black, 					null));	
			
			//Chains
			s_typeToModel.Add("chainA", new AtomModel("C", 1.70f, 100, Color.red,	null));
			s_typeToModel.Add("chainB", new AtomModel("C", 1.70f, 100, Color.green,	null));
			s_typeToModel.Add("chainC", new AtomModel("C", 1.70f, 100, Color.blue,	null));
			s_typeToModel.Add("chainD", new AtomModel("C", 1.70f, 100, Color.yellow,	null)); 	//orange
			s_typeToModel.Add("chainE", new AtomModel("C", 1.70f, 100, Color.gray,	null));
			s_typeToModel.Add("chainF", new AtomModel("C", 1.70f, 100, new Color(1.0f,0.5f,0.0f),	null)); //tan
			s_typeToModel.Add("chainG", new AtomModel("C", 1.70f, 100, new Color(0.5f,0.2f,1.0f),	null)); 	//silver
			s_typeToModel.Add("chainH", new AtomModel("C", 1.70f, 100, Color.white,	null));

			//Bfactor
			s_typeToModel.Add("BF1", new AtomModel("C", 1.00f, 100, new Color(0.0f, 0.0f, 1.0f),	null));
			s_typeToModel.Add("BF2", new AtomModel("C", 1.30f, 100, new Color(0.1f, 0.0f, 0.9f),	null));
			s_typeToModel.Add("BF3", new AtomModel("C", 1.60f, 100, new Color(0.2f, 0.0f, 0.8f),	null));
			s_typeToModel.Add("BF4", new AtomModel("C", 1.90f, 100, new Color(0.3f, 0.0f, 0.7f),	null));
			s_typeToModel.Add("BF5", new AtomModel("C", 2.20f, 100, new Color(0.4f, 0.0f, 0.6f),	null));
			s_typeToModel.Add("BF6", new AtomModel("C", 2.50f, 100, new Color(0.5f, 0.0f, 0.5f), 	null));
			s_typeToModel.Add("BF7", new AtomModel("C", 2.80f, 100, new Color(0.6f, 0.0f, 0.4f),	null));
			s_typeToModel.Add("BF8", new AtomModel("C", 3.10f, 100, new Color(0.7f, 0.0f, 0.3f),	null));
			s_typeToModel.Add("BF9", new AtomModel("C", 3.40f, 100, new Color(0.8f, 0.0f, 0.2f),	null));
			s_typeToModel.Add("BF10", new AtomModel("C", 3.70f, 100, new Color(0.9f, 0.0f, 0.1f),	null));

		}

		public static void InitHiRERNA() {
			s_typeToModel = new OrderedDictionary();

			//Atomic
//			s_typeToModel.Add("O", new AtomModel("O", 3.5f, 100, Color.red, 					null));
//			s_typeToModel.Add("C", new AtomModel("C", 3.5f, 100, Color.green, 					null));
//			s_typeToModel.Add("P", new AtomModel("P", 5.0f, 100, new Color(0.55f,0.44f,0.01f),	null)); //tan
//			s_typeToModel.Add("G", new AtomModel("G", 3.5f, 100, Color.cyan, 					null));
//			s_typeToModel.Add("A", new AtomModel("A", 3.5f, 100, Color.cyan, 					null));	
//			s_typeToModel.Add("U", new AtomModel("U", 3.5f, 100, Color.cyan, 					null));	
//			s_typeToModel.Add("C1", new AtomModel("C1", 3.5f, 100, Color.cyan, 					null));
//			s_typeToModel.Add("X", new AtomModel("X", 1.40f, 100, Color.black, 					null));
			
			s_typeToModel.Add("P", new AtomModel("P", 3.5f, 100, MoleculeModel.phosphorusColor.color, null));
			s_typeToModel.Add("O5*", new AtomModel("O", 3.5f, 100, MoleculeModel.oxygenColor.color, null));
			s_typeToModel.Add("C5*", new AtomModel("C", 3.5f, 100, MoleculeModel.carbonColor.color, null));
			s_typeToModel.Add("CA", new AtomModel("C", 3.5f, 100, MoleculeModel.carbonColor.color, null));
			s_typeToModel.Add("CY", new AtomModel("C", 3.5f, 100, MoleculeModel.carbonColor.color, null));
			
			s_typeToModel.Add("G1", new AtomModel("X", 3.5f, 100, Color.green, null));
			s_typeToModel.Add("G2", new AtomModel("X", 3.5f, 100, Color.green, null));
			
			s_typeToModel.Add("C1", new AtomModel("X", 3.5f, 100, Color.yellow, null));
			
			s_typeToModel.Add("A1", new AtomModel("X", 3.5f, 100, Color.red, null));
			s_typeToModel.Add("A2", new AtomModel("X", 3.5f, 100, Color.red, null));
			
			s_typeToModel.Add("U1", new AtomModel("X", 3.5f, 100, new Color(0.7f, 0.7f, 0.98f), null));
		}

		public static AtomModel GetModel(string type) {
			if(s_typeToModel == null)
				return null;
			return (AtomModel)s_typeToModel[type];
		}
		
	}
}