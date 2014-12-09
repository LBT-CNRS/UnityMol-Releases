/// @file ParticleEffect.cs
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
/// $Id: ParticleEffect.cs 329 2013-08-06 13:47:40Z erwan $
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


public class ParticleEffect : MonoBehaviour 
{ 
    public Transform particleEffect;
    
    public Transform effectObject;
	
	public static float radiusFactor = 1.0f;
	
	private  float radiusFactorold = 1.0f;

    public  int atomcount;
//	ArrayList atomLocationalist;
	public Particle[] p ;
	
	public Particle[] pn ;
	
	public ParticleEmitter emitter;
	
//	public bool radiuschange=false;
	
    public void Start() {
    }


    public void SpawnEffect () {
        // Instantiate the effect prefab.
        effectObject = Instantiate(particleEffect, this.transform.position, this.transform.rotation) as Transform;
        
        // Parent the new effect to this script's transform.  
        effectObject.parent = this.gameObject.transform;
        
        // Get the particle emitter from the new effect object.
        emitter = effectObject.GetComponent<ParticleEmitter>();
        
        // Make sure autodestruct is on so that dead particles systems get destroyed.
        ParticleAnimator animator = emitter.transform.GetComponent<ParticleAnimator>();
        if (animator != null)
            animator.autodestruct = true;
        
        // Generate the particles.
        emitter.Emit(atomcount);
        pn=new Particle[p.Length];
        for(int i=0;i<p.Length;i++)
        {
        		pn[i].size=p[i].size*radiusFactor*2;
        		pn[i].position=p[i].position;
				pn[i].color=p[i].color;
				pn[i].energy=p[i].energy;
        }
// 		pn=p;
        emitter.particles = pn;
		UI.UIData.isParticlesInitialized = true;
    }
    
    void Update() {
	if ((p != null) && !UI.UIData.isParticlesInitialized && (UI.UIData.atomtype == UI.UIData.AtomType.particleball) )
		{
			SpawnEffect();
		}
		
        // Spin the entire particle effect.
//      this.transform.Rotate(this.transform.up * Time.deltaTime * (-turnSpeed), Space.World);        
        if(radiusFactorold!=radiusFactor)
        {	
        	if(emitter)
        	{
        		for(int i=0;i<p.Length;i++)
        			pn[i].size=p[i].size*radiusFactor*2;
        		emitter.particles = pn;
       		}
        	radiusFactorold=radiusFactor;	
        }
    }
    
    

    
    
    // Kill all current spawns of the effect.
    public void killCurrentEffects() {
        
        // Loop thru the particle emitter children of this object.  
		// Each one is a particle effect system we want to destroy.
        ParticleEmitter[] emitters = this.transform.GetComponentsInChildren<ParticleEmitter>();
        foreach (ParticleEmitter emitter in emitters) 
        {
            Debug.Log("resetEffect killing: " + emitter.name);
            // Make sure autodestruct is on.
            ParticleAnimator animator = emitter.transform.GetComponent<ParticleAnimator>();
            if (animator != null)
                animator.autodestruct = true;
            // Now loop thru the particles and set their energies to a small number.  The effect will
            // subsequently autodestruct.  I originally tried setting the energy to zero, but in that
            // case they did *not* autodestruct.
            // I originally tried simply doing a Destroy on the emitter, but got threatening runtime messages.
            Particle[] p  = emitter.particles;
            for (int i=0; i < p.Length;  i++) 
            {
                p[i].energy = 0.1f;
            }
            emitter.particles = p;
            emitter.ClearParticles();
        }
       this.gameObject.transform.DetachChildren();
//       GameObject Particleclone=GameObject.Find("particle(Clone)");
//       GameObject Particleclone=GameObject.Find("particlein1(Clone)");

//       Destroy(Particleclone);   
		GameObject[] Particleclone;
		Particleclone = GameObject.FindGameObjectsWithTag("particlein1");
		for(int j=0;j<Particleclone.Length;j++)
		{
			Destroy(Particleclone[j]);
		}
		GameObject[] Particlemanager;
		Particlemanager = GameObject.FindGameObjectsWithTag("ParticleManager");
		for(int k=0;k<Particlemanager.Length;k++)
		{
			Destroy(Particlemanager[k]);
		}
   
    }
}
