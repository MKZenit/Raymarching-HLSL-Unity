using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using RGRC = RaymarchingGpuResourcesNames;

/// <summary>
/// Stores all per-pixel GPU resources
/// </summary>
[RequireComponent(typeof(CustomPipeline))]
public class InterconnectedGpuResources: MonoBehaviour
{
    [Header("Resources informations")]

    [SerializeField] private Vector2Int _renderTextureSize;
    public Vector2Int RenderTextureSize { get { return _renderTextureSize; } }

    [Header("Test")]
    [Range(-1f, 1)] public float _testParameter;

    [Header("Unwanted")]
    DualTextureHolder _resultDualTexHolder;

    DualTextureHolder _depthDualTexHolder;
    DualTextureHolder _geometryDualTexHolder;
    DualTextureHolder _normalDualTexHolder;
    DualTextureHolder _iterationDualTexHolder;

    CustomPipeline customPipeline;

    // Contains both DualTextureHolder and TextureHolder types.
    List<object> texturesList;

    bool isInitialized = false;
    private void Awake()
    {
        _resultDualTexHolder = new DualTextureHolder(RenderTextureFormat.ARGB32, true, RenderTextureSize.x, RenderTextureSize.y,
            RGRC.MainTextures.Result, RGRC.MainTextures.ResultSRV);

        _depthDualTexHolder = new DualTextureHolder(GraphicsFormat.R32_SFloat, RenderTextureSize.x, RenderTextureSize.y,
            RGRC.MainTextures.DepthTex, RGRC.MainTextures.DepthTexSRV);
        _geometryDualTexHolder = new DualTextureHolder(GraphicsFormat.R32_UInt, RenderTextureSize.x, RenderTextureSize.y,
            RGRC.MainTextures.GeometryTex, RGRC.MainTextures.GeometryTexSRV);
        _normalDualTexHolder = new DualTextureHolder(GraphicsFormat.R16G16B16A16_SFloat, RenderTextureSize.x, RenderTextureSize.y,
            RGRC.MainTextures.NormalTex, RGRC.MainTextures.NormalTexSRV);
        _iterationDualTexHolder = new DualTextureHolder(GraphicsFormat.R32_UInt, RenderTextureSize.x, RenderTextureSize.y,
            RGRC.MainTextures.IterationTex, RGRC.MainTextures.IterationTexSRV);

        texturesList = new List<object>() {
            _resultDualTexHolder,

            _depthDualTexHolder,
            _geometryDualTexHolder,
            _normalDualTexHolder,
            _iterationDualTexHolder
        };

        customPipeline = GetComponent<CustomPipeline>();
        isInitialized = true;
        OnValidate();
    }

    private void OnValidate()
    {
        if (isInitialized)
        {
            customPipeline.UpdateSharedResource(RGRC.MainConstants.RenderTextureSize, _renderTextureSize);
            customPipeline.UpdateSharedResource(RGRC.MainConstants._testParameter, _testParameter);

            foreach (var item in texturesList)
            {
                if (item is TextureHolder tex)
                {
                    tex.Update(_renderTextureSize.x, _renderTextureSize.y);

                    customPipeline.UpdateSharedResource(tex.name, tex.Texture);
                }
                else if (item is DualTextureHolder dualTex)
                {
                    dualTex.Update(_renderTextureSize.x, _renderTextureSize.y);
                    
                    customPipeline.UpdateSharedResource(dualTex.texture.name, dualTex.texture.Texture);
                    customPipeline.UpdateSharedResource(dualTex.textureSRV.name, dualTex.textureSRV.Texture);
                }
                else throw new System.Exception("Not yet implemented.");
            }

            customPipeline.OnValidate();
        }
    }

    private void OnDestroy()
    {
        foreach (var item in texturesList)
        {
            if (item is TextureHolder tex)
                tex.Release();
            else if (item is DualTextureHolder dualTex)
                dualTex.Release();
            else throw new System.Exception("Not yet implemented.");
        }
    }
}

public class DualTextureHolder
{
    public TextureHolder texture;
    public TextureHolder textureSRV;

    public DualTextureHolder(RenderTextureFormat renderTextureFormat, bool isSRGB, int x, int y,
        string nameTexture, string nameTextureSRV)
    {
        texture = new(renderTextureFormat, true, isSRGB, x, y, nameTexture);
        textureSRV = new(renderTextureFormat, false, isSRGB, x, y, nameTextureSRV);
    }
    public DualTextureHolder(GraphicsFormat graphicsFormat, int x, int y,
        string nameTexture, string nameTextureSRV)
    {
        texture = new(graphicsFormat, true, x, y, nameTexture);
        textureSRV = new(graphicsFormat, false, x, y, nameTextureSRV);
    }

    public void Update(int width, int height)
    {
        texture.Update(width, height);
        textureSRV.Update(width, height);
    }
    public void Release()
    {
        texture.Release();
        textureSRV.Release();
    }
}