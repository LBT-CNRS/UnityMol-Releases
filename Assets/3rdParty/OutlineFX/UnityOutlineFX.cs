using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.XR;

[RequireComponent(typeof(Camera))]
public class UnityOutlineFX : MonoBehaviour 
{

    #region public vars

    [Header("Outline Settings")]
	[SerializeField]
	public Color OutlineColor =  new Color(1,0,0,.05f); // alpha = fill alpha; does not effect outline alpha;

    public CameraEvent BufferDrawEvent = CameraEvent.BeforeImageEffects;

    [Header("Blur Settings")]
    [Range(0, 1)]
    public int Downsample = 1; // NOTE: downsampling will make things more efficient, as well as make the outline a bit thicker
    [Range(0.0f, 3.0f)]
    public float BlurSize = 1.0f;
    

    #endregion

    #region private field

    private CommandBuffer _commandBuffer;

    private int _outlineRTID, _blurredRTID, _temporaryRTID, _depthRTID, _idRTID;

    private List<List<Renderer>> _objectRenderers;

    private Material _outlineMaterial;		
    private Camera _camera;

	private int _RTWidth = 512;
	private int _RTHeight = 512;

    #endregion

    public void AddRenderers(List<Renderer> renderers)
    {
        _objectRenderers.Add(renderers);      
        RecreateCommandBuffer();
    }

    public void RemoveRenderers(List<Renderer> renderers)
    {
        _objectRenderers.Remove(renderers);      
        RecreateCommandBuffer();
    }

    public void ClearOutlineData()
    {
        _objectRenderers.Clear();
        RecreateCommandBuffer();
    }

    private void Awake()
	{
        _objectRenderers = new List<List<Renderer>>();

        _commandBuffer = new CommandBuffer();
        _commandBuffer.name = "UnityOutlineFX Command Buffer";

		_depthRTID = Shader.PropertyToID("_DepthRT");
        _outlineRTID = Shader.PropertyToID("_OutlineRT");
        _blurredRTID = Shader.PropertyToID("_BlurredRT");
        _temporaryRTID = Shader.PropertyToID("_TemporaryRT");
        _idRTID = Shader.PropertyToID("_idRT");
        
        _RTWidth = Screen.width;
        _RTHeight = Screen.height;

        _outlineMaterial = new Material(Shader.Find("Hidden/UnityOutline"));

        _camera = GetComponent<Camera>();
        _camera.depthTextureMode = DepthTextureMode.Depth;
        _camera.AddCommandBuffer(BufferDrawEvent, _commandBuffer);
	}

    private void RecreateCommandBuffer()
    {
        _commandBuffer.Clear();

        if (_objectRenderers.Count == 0)
            return;

        // initialization
        _commandBuffer.GetTemporaryRT(_depthRTID, _RTWidth, _RTHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

        //This is needed to avoid "Dimensions of color surface does not match dimensions of depth surface" error in VR
        // if (UMol.UnityMolMain.inVR()){
            _commandBuffer.SetRenderTarget(_depthRTID);
        // }
        // else{
            _commandBuffer.SetRenderTarget(_depthRTID);
        // }
        _commandBuffer.ClearRenderTarget(false, true, Color.clear);

        // render selected objects into a mask buffer, with different colors for visible vs occluded ones 
        float id = 0f;
		foreach (var collection in _objectRenderers)
        {
            id += 0.25f;
            _commandBuffer.SetGlobalFloat("_ObjectId", id);
    
            foreach (var render in collection)
            {
                _commandBuffer.DrawRenderer(render, _outlineMaterial, 0, 1);
                _commandBuffer.DrawRenderer(render, _outlineMaterial, 0, 0);
            }
        }
        
        // object ID edge dectection pass
        _commandBuffer.GetTemporaryRT(_idRTID, _RTWidth, _RTHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
        _commandBuffer.Blit(_depthRTID, _idRTID, _outlineMaterial, 3);

        // Blur
        int rtW = _RTWidth >> Downsample;
        int rtH = _RTHeight >> Downsample;

        _commandBuffer.GetTemporaryRT(_temporaryRTID, rtW, rtH, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
        _commandBuffer.GetTemporaryRT(_blurredRTID, rtW, rtH, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

        _commandBuffer.Blit(_idRTID,_blurredRTID);

        _commandBuffer.SetGlobalVector("_BlurDirection", new Vector2(BlurSize,0));
        _commandBuffer.Blit(_blurredRTID, _temporaryRTID, _outlineMaterial,2);
        _commandBuffer.SetGlobalVector("_BlurDirection", new Vector2(0,BlurSize));
        _commandBuffer.Blit(_temporaryRTID, _blurredRTID, _outlineMaterial, 2);


        // final overlay
        _commandBuffer.SetGlobalColor("_OutlineColor", OutlineColor);
        _commandBuffer.Blit(_blurredRTID,BuiltinRenderTextureType.CameraTarget, _outlineMaterial, 4);

        // release tempRTs
        _commandBuffer.ReleaseTemporaryRT(_blurredRTID);
		_commandBuffer.ReleaseTemporaryRT(_outlineRTID);
		_commandBuffer.ReleaseTemporaryRT(_temporaryRTID);
		_commandBuffer.ReleaseTemporaryRT(_depthRTID);

    }
}
