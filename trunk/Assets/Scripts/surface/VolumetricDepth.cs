using UnityEngine;
using System.Collections;
using UI;
using Molecule.Model;

public class VolumetricDepth : Volumetric {
//	private float maxDepth = - float.MaxValue;
	private float minDepth = float.MaxValue;
	private float depthAmplitude;
	
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
			//if(f > maxDepth)
			//	maxDepth = f;
			if(f < minDepth)
				minDepth = f;
		}
		
		// DepthAmplitude = largest absolute value
		//if ( (-minDepth) > maxDepth)
		//	depthAmplitude = -minDepth;
		//else
		//	depthAmplitude = maxDepth;
		
		depthAmplitude = Mathf.Abs(minDepth);
		
		Debug.Log("Amplitude:");
		Debug.Log(depthAmplitude.ToString());
		
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
					
					float depth = density[x,y,z];
					Color c = Color.red; // if we see a red particle, something is wrong, they should be discared.
					if(depth < 0)
						c = Color.black;
					c.a = 2.5f * Mathf.Abs((depth / depthAmplitude)); // Somewhat arbitrary, but guarantees 0 <= c.a <= 1.
					if(raise) {
						c.a = c.a * ALPHA_FACTOR;
						if (c.a > 1f)
							c.a = 1f;
					}
					
					// Here, we cull.
					if ( (c.a > ALPHA_THRESHOLD) && (depth <0) ) {
						ParticleSystem.Particle particle = new ParticleSystem.Particle();
						Vector3 p = new Vector3(0f,0f,0f);
						p = origin + Vector3.Scale(indices, delta);
						particle.position = p;
						particle.color = c;
						//Debug.Log(particle.color);
						//pSystem.renderer.material.color = c;
						//particle.size = particleScale;
						particle.size = delta.x * 2.8f;
						dynPoints.Add(particle);	
					}
				}
			}
		}
	} // End of CreatePoints
}
