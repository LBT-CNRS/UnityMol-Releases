using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent (typeof(Camera))]
[AddComponentMenu("Image Effects/Dream Effect")]
public class DreamEffect : MonoBehaviour
{
	
		/// Blur iterations - larger number means more blur.
	public int iterations = 3;
	
	/// Blur spread for each iteration. Lower values
	/// give better looking blur, but require more iterations to
	/// get large blurs. Value is usually between 0.5 and 1.0.
	public float blurSpread = 0.6f;
	
	public float ContrastPower = 5.0f;
	public float ContrastBias = 0.3f;
	
	
	// --------------------------------------------------------
	// The blur iteration shader.
	// Basically it just takes 4 texture samples and averages them.
	// By applying it repeatedly and spreading out sample locations
	// we get a Gaussian blur approximation.
	
	private static string blurMatString =
@"Shader ""BlurConeTap"" {
	SubShader {
		Pass {
			ZTest Always Cull Off ZWrite Off Fog { Mode Off }
			SetTexture [__RenderTex] {constantColor (0,0,0,0.25) combine texture * constant alpha}
			SetTexture [__RenderTex] {constantColor (0,0,0,0.25) combine texture * constant + previous}
			SetTexture [__RenderTex] {constantColor (0,0,0,0.25) combine texture * constant + previous}
			SetTexture [__RenderTex] {constantColor (0,0,0,0.25) combine texture * constant + previous}
		}
	}
	Fallback off
}";

	static Material m_BlurMaterial = null;
	protected static Material blurMaterial {
		get {
			if (m_BlurMaterial == null) {
				m_BlurMaterial = new Material( blurMatString );
				m_BlurMaterial.hideFlags = HideFlags.HideAndDontSave;
				m_BlurMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_BlurMaterial;
		} 
	}
	
	public Shader blackChannelShader;
	Material m_BlackChannelMaterial = null;
	protected Material BlackChannelMaterial {
		get {
			if (m_BlackChannelMaterial == null) {
				m_BlackChannelMaterial = new Material( blackChannelShader );
				m_BlackChannelMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_BlackChannelMaterial;
		} 
	}
	
	protected void OnDisable() {
		if( m_BlurMaterial ) {
			DestroyImmediate( m_BlurMaterial.shader );
			DestroyImmediate( m_BlurMaterial );
		}

		if( m_BlackChannelMaterial ) {
			DestroyImmediate( m_BlackChannelMaterial, true );
		}

	}	
	
	// --------------------------------------------------------
	
	protected void Start()
	{
		// Disable if we don't support image effects
		if (!SystemInfo.supportsImageEffects) {
			enabled = false;
			return;
		}
		// Disable if the shader can't run on the users graphics card
		if (!blurMaterial.shader.isSupported) {
			enabled = false;
			return;
		}
	
	}
	
	// Performs one blur iteration.
	public void FourTapCone (RenderTexture source, RenderTexture dest, int iteration)
	{
		RenderTexture.active = dest;
		source.SetGlobalShaderProperty ("__RenderTex");
		
		float offsetX = (.5F+iteration*blurSpread) / (float)source.width;
		float offsetY = (.5F+iteration*blurSpread) / (float)source.height;
		GL.PushMatrix ();
		GL.LoadOrtho ();    
		
		for (int i = 0; i < blurMaterial.passCount; i++) {
			blurMaterial.SetPass (i);
			Render4TapQuad( dest, offsetX, offsetY );
		}
		GL.PopMatrix ();
	}
	
	// Downsamples the texture to a quarter resolution.
	private void DownSample4x (RenderTexture source, RenderTexture dest)
	{
		RenderTexture.active = dest;
		source.SetGlobalShaderProperty ("__RenderTex");
		
		float offsetX = 1.0f / (float)source.width;
		float offsetY = 1.0f / (float)source.height;
		
		GL.PushMatrix ();
		GL.LoadOrtho ();
		for (int i = 0; i < blurMaterial.passCount; i++)
		{
			blurMaterial.SetPass (i);
			Render4TapQuad( dest, offsetX, offsetY );
		}
		GL.PopMatrix ();
	}
	
	// Called by camera to apply image effect
	void OnRenderImage (RenderTexture source, RenderTexture destination) 
	{
		
		RenderTexture buffer = RenderTexture.GetTemporary(source.width/4, source.height/4, 0);
		RenderTexture buffer2 = RenderTexture.GetTemporary(source.width/4, source.height/4, 0);
		RenderTexture dreamBuffer = RenderTexture.GetTemporary( source.width, source.height, 0);
		
		BlackChannelMaterial.SetFloat( "_ContrastPower", ContrastPower );
		BlackChannelMaterial.SetFloat( "_ContrastBias", ContrastBias );
		
		ImageEffects2.BlitWithMaterial (BlackChannelMaterial , source, dreamBuffer);
		
		// Copy source to the 4x4 smaller texture.
		DownSample4x (source, buffer);
		
		// Blur the small texture
		bool oddEven = true;
		for(int i = 0; i < iterations; i++)
		{
			if( oddEven ) FourTapCone (buffer, buffer2, i);
			else FourTapCone (buffer2, buffer, i);
			oddEven = !oddEven;
		}
		if( oddEven ) ImageEffects2.Blit(buffer, destination);
		else ImageEffects2.Blit(buffer2, destination);
		
		ImageEffects2.Blit( dreamBuffer, destination, BlendMode.Multiply );
		
		RenderTexture.ReleaseTemporary(buffer);
		RenderTexture.ReleaseTemporary(buffer2);
		RenderTexture.ReleaseTemporary(dreamBuffer);
		

	}
	
	private static void Render4TapQuad( RenderTexture dest, float offsetX, float offsetY )
	{
		GL.Begin( GL.QUADS );

		// Direct3D needs interesting texel offsets!		
		Vector2 off = Vector2.zero;
		if( dest != null )
			off = dest.GetTexelOffset() * 0.75f;
		
		Set4TexCoords( off.x, off.y, offsetX, offsetY );
		GL.Vertex3( 0,0, .1f );
		
		Set4TexCoords( 1.0f + off.x, off.y, offsetX, offsetY );
		GL.Vertex3( 1,0, .1f );
		
		Set4TexCoords( 1.0f + off.x, 1.0f + off.y, offsetX, offsetY );
		GL.Vertex3( 1,1,.1f );
		
		Set4TexCoords( off.x, 1.0f + off.y, offsetX, offsetY );
		GL.Vertex3( 0,1,.1f );
		
		GL.End();
	}
	
	private static void Set4TexCoords( float x, float y, float offsetX, float offsetY )
	{
		GL.MultiTexCoord2( 0, x - offsetX, y - offsetY );
		GL.MultiTexCoord2( 1, x + offsetX, y - offsetY );
		GL.MultiTexCoord2( 2, x + offsetX, y + offsetY ); 
		GL.MultiTexCoord2( 3, x - offsetX, y + offsetY );
	}
}
