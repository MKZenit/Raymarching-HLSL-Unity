using System;
using UnityEngine;
using RGRC = RaymarchingGpuResourcesNames;
using AKRC = ApplicationKernelResourcesNames;

public class SphereTracingLighting : CustomKernel
{
    // Properties
    public ComputeShader _raymarchingComputeShader;

    [Header("Main")]
    [SerializeField] DirectionalLight _directionalLight;
    [SerializeField] Lighting _lighting;

    override public KernelDescription GetKernelDescription()
    {
        return new(_raymarchingComputeShader, "CSLight", PerPixelDispatchSize,
           () => new (string, object)[] {
            (nameof(_directionalLight)+nameof(_directionalLight.Direction), _directionalLight.Direction.normalized),
            (nameof(_directionalLight)+nameof(_directionalLight.Diffuse)+nameof(_directionalLight.Diffuse.Color), _directionalLight.Diffuse.Color),
            (nameof(_directionalLight) + nameof(_directionalLight.Diffuse) + nameof(_directionalLight.Diffuse.Power) , _directionalLight.Diffuse.Power),
            (nameof(_directionalLight) + nameof(_directionalLight.Specular) + nameof(_directionalLight.Specular.Color) , _directionalLight.Specular.Color),
            (nameof(_directionalLight) + nameof(_directionalLight.Specular) + nameof(_directionalLight.Specular.Power) , _directionalLight.Specular.Power),

            (nameof(_lighting) + nameof(_lighting.SpecularHardness) , _lighting.SpecularHardness),
        }, useSharedProperties: () => new string[] {
            AKRC.Camera.CameraPosition,
            AKRC.Camera.CameraVectorX, AKRC.Camera.CameraVectorY, AKRC.Camera.CameraVectorZ,
            AKRC.Camera.CameraFov,

            RGRC.MainConstants.RenderTextureSize,
            RGRC.MainConstants._maxDistanceThreshold,

            RGRC.MainTextures.DepthTexSRV,
            RGRC.MainTextures.NormalTexSRV,
            RGRC.MainTextures.Result,
        });
    }

    [Serializable]
    struct DirectionalLight
    {
        [SerializeField] public Vector3 Direction;
        [SerializeField] public UnlitComponent Diffuse;
        [SerializeField] public UnlitComponent Specular;
    }

    [Serializable]
    struct Lighting
    {
        [SerializeField] public float SpecularHardness;
    }
    [Serializable]
    struct UnlitComponent
    {
        [SerializeField] public Vector3 Color;
        [SerializeField] public float Power;
    }
}
