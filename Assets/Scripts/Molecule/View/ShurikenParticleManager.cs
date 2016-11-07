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
/// $Id: NewParticleMananger.cs
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
using System.Collections.Generic;
using Molecule.Model;
using UI;

public class ShurikenParticleManager : GenericManager {
	public ParticleSystem pSystem;
	private Particle[] sourceParticles;
	private static Vector3 LITTLE_OFFSET = new Vector3(0f, 0f, -1f); // not sure why, but this is needed
	private ParticleSystem.Particle[] particles; // not exactly sure what the difference between Particle and ParticleSystem.Particle is
	public static int pNumber;
	//public bool test;
	public static float radiusFactor;	
	
	/// <summary>
	/// Initializes this instance of the ParticleSystem Manager.
	/// </summary>
	public override void Init () {
		Debug.Log("Shuriken Initializing");
		sourceParticles = MoleculeModel.p;
		pNumber = sourceParticles.Length;
		Debug.Log("Found: " + pNumber + " particles");
		particles = new ParticleSystem.Particle[pNumber];
		
		for(int i=0; i<pNumber; i++) {
			particles[i].position = sourceParticles[i].position + LITTLE_OFFSET;
			particles[i].color = sourceParticles[i].color;
			particles[i].size = sourceParticles[i].size * 0.5f * BallUpdate.radiusFactor;
		}
		
		//We draw the molecule HERE!
		pSystem.SetParticles(particles, pNumber);
		
		//CheckParticles();
		pSystem.GetComponent<Renderer>().enabled = true;
		enabled = true;
		Debug.Log("Shuriken Initialized");
	}
	
	public override void DestroyAll() {
		
	}
	
	public override void SetColor(Color col, List<string> atoms, string residue = "All", string chain = "All"){
		
	}
	public override void SetColor(Color col, int atomNum){
		
	}
	public override void SetRadii(List<string> atoms, string residue = "All", string chain = "All"){}
	public override void SetRadii(int atomNum){}
	
	public override GameObject GetBall(int id){
		return null;
	}
	
	public override void ToggleDistanceCueing(bool enabling) {
		
	}
	
	/// <summary>
	/// Used for debugging. Prints the position and color of each particle.
	/// </summary>
	private void CheckParticles() {
		ParticleSystem.Particle[] currentParticles = new ParticleSystem.Particle[pNumber];
		pSystem.GetParticles(currentParticles);
		foreach(ParticleSystem.Particle p in currentParticles) {
			Debug.Log(p.position.ToString());
			Debug.Log(p.color.ToString());
		}
		Debug.Log(pSystem.GetComponent<Renderer>().enabled.ToString());
	}
	
/*	private void ForDebugging() {
		radiusFactor = BallUpdate.radiusFactor;
		test = UIData.isParticlesInitialized;
		//Debug.Log(UIData.atomtype.ToString());
	}
*/
	
	/// <summary>
	/// Resets the radii of the particle balls as needed when it is changed through the GUI.
	/// </summary>
	private void ResetRadii() {
		ParticleSystem.Particle[] currentParticles = new ParticleSystem.Particle[pNumber];
		pSystem.GetParticles(currentParticles);
		for(int i=0; i<pNumber; i++)
			currentParticles[i].size = currentParticles[i].size * BallUpdate.radiusFactor / BallUpdate.oldRadiusFactor;
		
		pSystem.SetParticles(currentParticles, pNumber);
		Debug.Log("Resetting Particles radii");
		BallUpdate.oldRadiusFactor = BallUpdate.radiusFactor;
	}
	
	private void ResetColors() {
		if(UIData.atomtype == UIData.AtomType.particleball) {
			// Can't manage to change the color of the good atom. Particles and atomsColorList aren't organized the same way
//			for(int i=0; i<pNumber; i++)
//				currentParticles[i].color = MoleculeModel.atomsColorList[i];
			
			
			BallUpdate.resetColors = false;
			BallUpdate.bondsReadyToBeReset = true;
		}
	}
	
	/// <summary>
	/// Enables this script and the renderer of its particle system.
	/// </summary>
	public override void EnableRenderers() {
		pSystem.GetComponent<Renderer>().enabled = true;
		enabled = true;
	}
	
	/// <summary>
	/// Disables this script and the renderer of its particle system.
	/// </summary>
	public override void DisableRenderers() {
		pSystem.GetComponent<Renderer>().enabled = false;
		enabled = false;
	}
	
	
	void Update () {
		//ForDebugging();
		if((UIData.atomtype != UIData.AtomType.particleball) || (!UIData.hasMoleculeDisplay) ){
			DisableRenderers();
			return;
		}
		if(BallUpdate.radiusFactor != BallUpdate.oldRadiusFactor)
			ResetRadii();
		
		//if(BallUpdate.resetColors)
		//	ResetColors();
	}
	
	public override void ResetPositions(){
		for (int j=0; j<particles.Length; j++){
			particles[j].position = new Vector3(Molecule.Model.MoleculeModel.atomsLocationlist[j][0], 
			                                               Molecule.Model.MoleculeModel.atomsLocationlist[j][1],
			                                               Molecule.Model.MoleculeModel.atomsLocationlist[j][2]);
		}
		pSystem.SetParticles(particles, pNumber);
	}
	
	public override void ResetMDDriverPositions(){
//		for (int j=0; j<particles.Length; j++){
//			particles[j].position = new Vector3(Molecule.Model.MoleculeModel.atomsMDDriverLocationlist[(j*3)], 
//			                                          Molecule.Model.MoleculeModel.atomsMDDriverLocationlist[(j*3)+1],
//			                                          Molecule.Model.MoleculeModel.atomsMDDriverLocationlist[(j*3)+2]);
//		}
//		pSystem.SetParticles(particles, pNumber);
	}
}
