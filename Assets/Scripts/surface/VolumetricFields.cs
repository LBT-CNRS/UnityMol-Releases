/// @file VolumetricFields.cs;
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
/// $Id: VolumetricFields.cs 227 2013-04-25 15:21:09Z baaden $
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
using UI;
using Molecule.Model;

public class VolumetricFields : Volumetric {
	private float maxCharge = - float.MaxValue;
	private float minCharge = float.MaxValue;
	private float chargeAmplitude;
	
	private ReadDX readDx;

	public override void Init() {
		// We get the ReadDX object and from it, the values we need.
		readDx = GUIMoleculeController.readdx;
		density = readDx._grid;
		delta = readDx.GetDelta();
		origin = readDx.GetOrigin();
		
		// This is needed to correctly place the particles.
		origin.x = -origin.x;
		
		// Getting the bounds and amplitude of the electrostatics field.
		foreach(float f in density) {
			if(f > maxCharge)
				maxCharge = f;
			if(f < minCharge)
				minCharge = f;
		}
		
		// ChargeAmplitude = largest absolute value
		if ( (-minCharge) > maxCharge)
			chargeAmplitude = -minCharge;
		else
			chargeAmplitude = maxCharge;
		
		Debug.Log("Amplitude:");
		Debug.Log(chargeAmplitude.ToString());
		
		// We get the parent object and a reference to its particle system, so we can control it.
		parentObj = GameObject.FindGameObjectWithTag("Volumetric");	
		pSystem = parentObj.GetComponent<ParticleSystem>();
		
		// Creating the dynamic particle list, building the static particle array,
		// setting it to the particle system, and enabling the renderer.
		SetParticleSystem();
	} // End of Init
	
	public override void CreatePoints() {
		// Getting the size of the density grid.
		int dim0 = density.GetLength(0);
		int dim1 = density.GetLength(1);
		int dim2 = density.GetLength(2);

		Vector3 indices = Vector3.zero;
		
		for(int x=0; x<dim0; x++) {
			for(int y=0; y<dim1; y++) {
				for(int z=0; z<dim2; z++) {
					indices.x = -x;
					indices.y = y;
					indices.z = z;
					
					float charge = density[x,y,z];
					Color c;
					if(charge > 0)
						c = Color.blue;
					else
						c = Color.red;
					c.a = Mathf.Abs((charge / chargeAmplitude)); // Somewhat arbitrary, but guarantees 0 <= c.a <= 1.
					if(raise) {
						c.a = c.a * ALPHA_FACTOR;
						if (c.a > 1f)
							c.a = 1f;
					}
					
					// Here, we cull.
					float size = 0.8f * particleScale / delta.x;
					if (c.a > ALPHA_THRESHOLD) {
						ParticleSystem.Particle particle = new ParticleSystem.Particle();
						Vector3 p = new Vector3(0f,0f,0f);
						p = origin + Vector3.Scale(indices, delta);
						particle.position = p;
						particle.color = c;
						//Debug.Log(particle.color);
						//pSystem.renderer.material.color = c;
						//particle.size = particleScale;
						particle.size = size;
						dynPoints.Add(particle);	
					}
				}
			}
		}
	} // End of CreatePoints
	
}
