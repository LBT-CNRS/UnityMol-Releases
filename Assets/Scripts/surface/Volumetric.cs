/// @file Volumetric.cs;
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
/// $Id: Volumetric.cs 227 2013-04-25 15:21:09Z baaden $
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
using System.Collections.Generic;
using Molecule.Model;

public abstract class Volumetric : MonoBehaviour {
	public static float particleScale = 2.5f;
	protected float ALPHA_THRESHOLD = 0.05f;
	
	protected bool raise = false;
	protected float ALPHA_FACTOR = 7f;
	
	protected float[,,] density;
	
	protected ParticleSystem.Particle[] points;
	protected ParticleSystem pSystem;
	protected GameObject parentObj;
	protected List<ParticleSystem.Particle> dynPoints = new List<ParticleSystem.Particle>();
	public int pNumber;
	protected static Vector3 delta;
	protected static Vector3 origin = MoleculeModel.MinValue;
	
	/// <summary>
	/// Initializes this instance. To be called whenever you wish to create a particle system for volumetric rendering.
	/// </summary>
	public abstract void Init();
	
	/// <summary>
	/// Creates the points for this particle system.
	/// More precisely, it fills the dynamic list called dynPoints with particles of the right color, transparency and position.
	/// This list is raw, i.e. it contains one particle for each point of the grid. Therefore, it is typically very large.
	/// </summary>
	public abstract void CreatePoints();
	
	/// <summary>
	/// Clears this instance. It empties the list of points, resets the number of particles, and disables the renderer.
	/// To be called when you want to destroy the particle system.
	/// </summary>	
	public void Clear()
	{
		points = null;
		pNumber = 0;
		GetComponent<Renderer>().enabled = false;
		dynPoints.Clear();
		Destroy(this);
	}
	
	/// <summary>
	/// Removes from the dynamic particle list all particles whose
	/// alpha component is lower than ALPHA_THRESHOLD.
	/// Currently not used, since dynamic lists are culled during their creation.
	/// </summary>
	protected void AlphaCulling(float cull) {
		//dynPoints.RemoveAll(item => item.color.a < cull);
		dynPoints.RemoveAll(item => item.color == Color.blue);
	}
	
	/// <summary>
	/// Builds the static particle array from the dynamic list.
	/// </summary>
	protected void BuildParticleArray()
	{
		pNumber = dynPoints.Count;
		points = new ParticleSystem.Particle[pNumber];
		for(int i=0; i<pNumber; i++)
			points[i] = dynPoints[i];
	}
	
	/// <summary>
	/// Sets the particle system.
	/// This creates the dynamic particle list, builds the static particle array,
	/// "sets" it to the particle system, and enables the renderer.
	/// </summary>
	protected void SetParticleSystem()
	{
		// We get the parent object and a reference to its particle system, so we can control it.
		parentObj = GameObject.FindGameObjectWithTag("Volumetric");
		pSystem = parentObj.GetComponent<ParticleSystem>();
		
		CreatePoints();
		
		//AlphaCulling(0.10f); // you could further cull here.
		BuildParticleArray();

		pSystem.SetParticles(points, pNumber);
		GetComponent<Renderer>().enabled = true;
	}
}
