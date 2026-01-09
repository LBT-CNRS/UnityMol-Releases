/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

        Xavier Martinez, 2017-2022
        Hubert Santuz, 2022-2026
        Marc Baaden, 2010-2026
        unitymol@gmail.com
        https://unity.mol3d.tech/

        This file is part of UnityMol, a general framework whose purpose is to provide
        a prototype for developing molecular graphics and scientific
        visualisation applications based on the Unity3D game engine.
        More details about UnityMol are provided at the following URL: https://unity.mol3d.tech/

        This program is free software: you can redistribute it and/or modify
        it under the terms of the GNU General Public License as published by
        the Free Software Foundation, either version 3 of the License, or
        (at your option) any later version.

        This program is distributed in the hope that it will be useful,
        but WITHOUT ANY WARRANTY; without even the implied warranty of
        MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
        GNU General Public License for more details.

        You should have received a copy of the GNU General Public License
        along with this program. If not, see <https://www.gnu.org/licenses/>.

        To help us with UnityMol development, we ask that you cite
        the research papers listed at https://unity.mol3d.tech/cite-us/.
    ================================================================================
*/
using UnityEngine;

/// A unility class with functions to scale Texture2D Data.
///
/// Scale is performed on the GPU using RTT, so it's blazing fast.
/// Setting up and Getting back the texture data is the bottleneck. 
/// But Scaling itself costs only 1 draw call and 1 RTT State setup!
/// WARNING: This script override the RTT Setup! (It sets a RTT!)	 
///
/// Note: This scaler does NOT support aspect ratio based scaling. You will have to do it yourself!
/// It supports Alpha, but you will have to divide by alpha in your shaders, 
/// because of premultiplied alpha effect. Or you should use blend modes.
public class TextureScaler
{

	/// <summary>
	///	Returns a scaled copy of given texture. 
	/// </summary>
	/// <param name="tex">Source texure to scale</param>
	/// <param name="width">Destination texture width</param>
	/// <param name="height">Destination texture height</param>
	/// <param name="mode">Filtering mode</param>
	public static Texture2D scaled(Texture2D src, int width, int height, FilterMode mode = FilterMode.Trilinear)
	{
		Rect texR = new Rect(0,0,width,height);
		_gpu_scale(src,width,height,mode);
		
		//Get rendered data back to a new texture
		Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, true);
		result.Reinitialize(width, height);
		result.ReadPixels(texR,0,0,true);
		return result;			
	}
	
	/// <summary>
	/// Scales the texture data of the given texture.
	/// </summary>
	/// <param name="tex">Texure to scale</param>
	/// <param name="width">New width</param>
	/// <param name="height">New height</param>
	/// <param name="mode">Filtering mode</param>
	public static void scale(Texture2D tex, int width, int height, FilterMode mode = FilterMode.Trilinear)
	{
		Rect texR = new Rect(0,0,width,height);
		_gpu_scale(tex,width,height,mode);
		
		// Update new texture
		tex.Reinitialize(width, height);
		tex.ReadPixels(texR,0,0,true);
		tex.Apply(true);	//Remove this if you hate us applying textures for you :)
	}
		
	// Internal unility that renders the source texture into the RTT - the scaling method itself.
	static void _gpu_scale(Texture2D src, int width, int height, FilterMode fmode)
	{
		//We need the source texture in VRAM because we render with it
		src.filterMode = fmode;
		src.Apply(true);	
				
		//Using RTT for best quality and performance. Thanks, Unity 5
		// RenderTexture rtt = new RenderTexture(width, height, 32);
        RenderTexture rtt = RenderTexture.GetTemporary(width, height, 32);
		
		//Set the RTT in order to render to it
		Graphics.SetRenderTarget(rtt);
		
		//Setup 2D matrix in range 0..1, so nobody needs to care about sized
		GL.LoadPixelMatrix(0,1,1,0);
		
		//Then clear & draw the texture to fill the entire RTT.
		GL.Clear(true,true,new Color(0,0,0,0));
		Graphics.DrawTexture(new Rect(0,0,1,1),src);

        RenderTexture.ReleaseTemporary(rtt);
	}
}