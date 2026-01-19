using System;
using UnityEngine;
using RGRC = RaymarchingGpuResourcesNames;
using AKRC = ApplicationKernelResourcesNames;

public class SphereTracing : CustomKernel
{
    // Properties
    public ComputeShader _raymarchingComputeShader;

    [Header("Main")]
    [Range(0.00001f, 10)] public float _minDistanceThreshold;
    [Range(100, 10000)] public float _maxDistanceThreshold;
    [Range(1, 250)] public int _maxSteps;
    [Range(-1f, 1f)] public float _extreme_angle_threshold;

    override public KernelDescription GetKernelDescription()
    {
        return new(_raymarchingComputeShader, "CSMain", PerPixelDispatchSize,
           () => new (string, object)[] {
            (nameof(_extreme_angle_threshold), _extreme_angle_threshold),
            (nameof(_minDistanceThreshold) , _minDistanceThreshold),
        }, () => new (string, string)[] {
            (RGRC.MainTextures.DepthTex, RGRC.MainTextures.DepthTexSRV),
            (RGRC.MainTextures.GeometryTex, RGRC.MainTextures.GeometryTexSRV),
            (RGRC.MainTextures.NormalTex, RGRC.MainTextures.NormalTexSRV),
            (RGRC.MainTextures.IterationTex, RGRC.MainTextures.IterationTexSRV),
        }, setSharedProperties: () => new (string, object)[] {
            (RGRC.MainConstants._maxDistanceThreshold , _maxDistanceThreshold),
            (RGRC.MainConstants._maxSteps, _maxSteps),
        }, useSharedProperties: () => new string[]{
            AKRC.Camera.CameraPosition,
            AKRC.Camera.CameraVectorX, AKRC.Camera.CameraVectorY, AKRC.Camera.CameraVectorZ,
            AKRC.Camera.CameraFov,

            RGRC.MainTextures.DepthTex,
            RGRC.MainTextures.GeometryTex,
            RGRC.MainTextures.NormalTex,
            RGRC.MainTextures.IterationTex,

            RGRC.MainConstants.RenderTextureSize,
            "_enableIterationDebugStep",
            "_iterationDebugStep",

            AKRC.GeometryBuffers.Boxes,
            AKRC.GeometryBuffers.Spheres,
            AKRC.GeometryBuffers.Slashes,
            AKRC.GeometryBuffers.Stabs,

            AKRC.GeometryDimensions.spheres_count,
            AKRC.GeometryDimensions.boxes_count,
            AKRC.GeometryDimensions.stabs_count,
            AKRC.GeometryDimensions.slashes_count,
        });
    }

}
