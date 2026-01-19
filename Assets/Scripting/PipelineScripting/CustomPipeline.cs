using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using RGRC = RaymarchingGpuResourcesNames;

/// <summary>
/// Build a pipeline and create a CommandBuffer from it
/// </summary>
[RequireComponent(typeof(InterconnectedGpuResources))]
public class CustomPipeline : MonoBehaviour
{
    [Header("Main")]
    public string pipelineName;
    public enum Framerate { 
        _8 = 8, _15 = 15, _30 = 30, _60 = 60, _120 = 120,
        _unlimited
    };
    public Framerate framerate;

    [SerializeField] CustomKernel[] kernels;

    [Header("Information Canvas")]
    public TextMeshProUGUI _totalTimeTMP;
    public TextMeshProUGUI _frameRateMsTMP;
    public TextMeshProUGUI _framePerSecondTMP;

    [Header("Unwanted")]
    private InterconnectedGpuResources _gpuResources;
    public Vector3Int PerPixelDispatchSize { get { return new Vector3Int(_gpuResources.RenderTextureSize.x, _gpuResources.RenderTextureSize.y, 1); } }

    readonly List<KernelDescription> pipeline = new();
    readonly List<ComputeShaderWatcher> shaderWatchers = new();

    readonly List<string> sharedResourcesNames = new();
    readonly Dictionary<string, object> sharedResources = new();

    readonly List<string> staticSharedResourcesNames = new();
    readonly Dictionary<string, object> staticSharedResources = new();

    CommandBuffer commandBuffer;

    private bool isInitialized = false;
    private void Start()
    {
        _gpuResources = GetComponent<InterconnectedGpuResources>();
        commandBuffer = new() { name = pipelineName };
        isInitialized = true;
        OnValidate();
    }

    public void OnValidate()
    {
        if (isInitialized)
        {
            if (framerate == Framerate._unlimited)
                Application.targetFrameRate = 0;
            else
                Application.targetFrameRate = (int)framerate;

            pipeline.Clear();
            shaderWatchers.ForEach(x => x.Dispose());
            shaderWatchers.Clear();
            
            sharedResourcesNames.Clear();
            sharedResources.Clear();
            // build pipeline per kernel
            foreach (var kernel in kernels)
            {
                if (kernel.enabled)
                {
                    if (kernel == null) continue;
                    var desc = kernel.GetKernelDescription();
                    pipeline.Add(desc);

                    if (desc.setSharedProperties == null) continue;
                    (string, object)[] setSharedProperties = desc.setSharedProperties();
                    for (int i = 0; i < setSharedProperties.Length; i++)
                    {
                        var (srName, sr) = setSharedProperties[i];
                        if (!sharedResourcesNames.Contains(srName))
                        {
                            sharedResourcesNames.Add(srName);
                            sharedResources.Add(srName, sr);
                        }
                        else
                            throw new ArgumentException("Shared resource name conflict : " + srName + " already exists in the shared resources.");
                    }
#if UNITY_EDITOR
                    ComputeShaderWatcher csw = new(desc.computeShader,
            Application.dataPath + "/../" + AssetDatabase.GetAssetPath(desc.computeShader));
                    bool hasWatcher = false;
                    foreach (var watcher in shaderWatchers)
                        hasWatcher |= watcher.Equals(csw);
                    if (!hasWatcher)
                        shaderWatchers.Add(csw);
                    else
                        csw.Dispose();
#endif
                }
            }

            // add static shared resources
            foreach (var (srName, sr) in staticSharedResources)
            {
                if (!sharedResourcesNames.Contains(srName))
                {
                    sharedResourcesNames.Add(srName);
                    sharedResources.Add(srName, sr);
                }
                else
                    print("Static shared resource name conflict : " + srName + " already exists in the shared resources.");
            }
        }
    }
    
    private void OnDestroy()
    {
        isInitialized = false;

        commandBuffer.Dispose();
        shaderWatchers.ForEach(watcher => watcher.Dispose());
        shaderWatchers.Clear();

        sharedResources.Clear();
        sharedResourcesNames.Clear();

        staticSharedResources.Clear();
        staticSharedResourcesNames.Clear();
        
        pipeline.Clear();
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        BuildCommandBuffer(src, dest);
        Graphics.ExecuteCommandBuffer(commandBuffer);
#if UNITY_EDITOR
        var ResultResource = GetResultTexture();
        if (ResultResource.IsCreated())
            Graphics.Blit(ResultResource, dest);
        else
            Graphics.Blit(src, dest);
#endif
    }

    float DelayedFrameRate = 0f;
    float DelayedFPS = 0f;
    public void BuildCommandBuffer(RenderTexture src, RenderTexture dest)
    {
        commandBuffer.Clear();

#if UNITY_EDITOR
        float BeginRaymarchingTime = Time.time;
        _totalTimeTMP.SetText("Total time (seconds) : " + MathF.Round(Time.time, 1).ToString());
        _frameRateMsTMP.SetText("Frame rate with callback (ms) : " + MathF.Round(DelayedFrameRate, 1).ToString());
        DelayedFPS = 0.5f*DelayedFPS + 0.5f*Time.deltaTime;
        _framePerSecondTMP.SetText("FPS : " + MathF.Round(1 / DelayedFPS, 1) + " ; ms : " + MathF.Round(DelayedFPS*1000, 1) );

        if (AreFilesReady())
        {
#endif
            var ResultResource = GetResultTexture();
            commandBuffer.Blit(ResultResource, dest);

            foreach (var kernelDesc in pipeline)
            {
                commandBuffer.BeginSample(kernelDesc.computeShader.name + "_" + kernelDesc.kernel);

                int kernelIndex = kernelDesc.computeShader.FindKernel(kernelDesc.kernel);

                if (kernelDesc.useSharedProperties != null)
                {
                    string[] useSharedProperties = kernelDesc.useSharedProperties();
                    // link shared ressources of other kernels
                    for (int i = 0; i < useSharedProperties.Length; i++)
                        SetWithSharedResource(kernelDesc.computeShader, kernelIndex, useSharedProperties[i]);
                }

                if (kernelDesc.setSharedProperties != null)
                {
                    (string, object)[] setSharedProperties = kernelDesc.setSharedProperties();
                    // link owned shared ressources
                    for (int i = 0; i < setSharedProperties.Length; i++)
                        SetWithSharedResource(kernelDesc.computeShader, kernelIndex, setSharedProperties[i].Item1);
                }

                if(kernelDesc.ressources != null)
                {
                    // link ressources
                    foreach (var (resourceName,resource) in kernelDesc.ressources())
                        SetResource(kernelDesc.computeShader, kernelIndex, resource, resourceName);
                }

                // dispatch
                try{
                    kernelDesc.computeShader.GetKernelThreadGroupSizes(kernelIndex, out uint threadGroupSizeX, out uint threadGroupSizeY, out uint threadGroupSizeZ);
                    commandBuffer.DispatchCompute(kernelDesc.computeShader, kernelIndex,
                        (int)(kernelDesc.totalDimension.x / threadGroupSizeX), (int)(kernelDesc.totalDimension.y / threadGroupSizeY), (int)(kernelDesc.totalDimension.z / threadGroupSizeZ));
                }
                catch(IndexOutOfRangeException e){
                    Debug.LogError("Dispatch error, a kernel is certainly invalid : "+ e.Message);
                }

                if (kernelDesc.texturesTransition != null)
                {
                    // textures transition
                    (string, string)[] texturesTransitionNames = kernelDesc.texturesTransition();
                    (RenderTexture, RenderTexture)[] texturesTransition = new (RenderTexture, RenderTexture)[(texturesTransitionNames != null) ? texturesTransitionNames.Length : 0];
                    for (int i = 0; i < texturesTransitionNames?.Length; i++)
                    {
                        (string texName, string texSRVName) = texturesTransitionNames[i];
                        texturesTransition[i] = (
                            sharedResources[texName] as RenderTexture,
                            sharedResources[texSRVName] as RenderTexture
                        );
                    }
                    if (texturesTransition != null)
                        foreach (var (tex, texSRV) in texturesTransition)
                            commandBuffer.CopyTexture(tex, texSRV);
                }

                commandBuffer.EndSample(kernelDesc.computeShader.name + "_" + kernelDesc.kernel);
            }
#if UNITY_EDITOR
            commandBuffer.RequestAsyncReadback(ResultResource, (request) =>
            {
                if (request.hasError) Debug.Log("GPU readback failed!");
                else DelayedFrameRate = DelayedFrameRate * 0.99f + ((Time.time - BeginRaymarchingTime) * 1000) * 0.01f;
            });
        }
        else {
            commandBuffer.Blit(src, dest);
        }
#endif
    }

    public void UpdateSharedResource(string resourceName, object resource)
    {
        if (!staticSharedResourcesNames.Contains(resourceName))
        {
            staticSharedResourcesNames.Add(resourceName);
            staticSharedResources.Add(resourceName, resource);
        }
        else
        {
            staticSharedResources[resourceName] = resource;
        }

        if (!sharedResourcesNames.Contains(resourceName))
        {
            sharedResourcesNames.Add(resourceName);
            sharedResources.Add(resourceName, resource);
        }
        else
        {
            sharedResources[resourceName] = resource;
        }
    }

    public object GetApplicationResource(string resourceName)
    {
        if (staticSharedResourcesNames.Contains(resourceName))
            return staticSharedResources[resourceName];
        if(sharedResourcesNames.Contains(resourceName))
            return sharedResources[resourceName];
        throw new NotImplementedException(resourceName + " shared resource could not be found.");
    }

    private void SetWithSharedResource(ComputeShader computeShader, int kernelIndex, string resourceName)
    {
        // Note : resourcesName shall have same shared name and GPU variable name.
        if (sharedResourcesNames.Contains(resourceName))
        {
            SetResource(computeShader, kernelIndex, sharedResources[resourceName], resourceName);
        }
        else
            throw new NotImplementedException(computeShader.name + "." + resourceName + " shared resource could not be linked.");
    }

    private void SetResource(ComputeShader computeShader, int kernelIndex, object resource, string resourceName)
    {
        if (resource is Func<object> resourceGetter)
            SetResource(computeShader, kernelIndex, resourceGetter.Invoke(), resourceName);
        else if (resource is bool b)
            commandBuffer.SetComputeIntParam(computeShader, resourceName, (b) ? 1 : 0);
        else if (resource is float || resource is double)
            commandBuffer.SetComputeFloatParam(computeShader, resourceName, (float)resource);
        else if (resource is int integer)
            commandBuffer.SetComputeIntParam(computeShader, resourceName, integer);
        else if (resource is uint uinteger)
            commandBuffer.SetComputeIntParam(computeShader, resourceName, (int)uinteger);
        else if (resource is Matrix4x4 matrix)
            commandBuffer.SetComputeMatrixParam(computeShader, resourceName, matrix);
        else if (resource is Vector2 vector2)
            commandBuffer.SetComputeVectorParam(computeShader, resourceName, vector2);
        else if (resource is Vector3 vector3)
            commandBuffer.SetComputeVectorParam(computeShader, resourceName, vector3);
        else if (resource is Vector4 vector4)
            commandBuffer.SetComputeVectorParam(computeShader, resourceName, vector4);
        else if (resource is Vector2Int vector2int)
            commandBuffer.SetComputeVectorParam(computeShader, resourceName, new Vector2(vector2int.x, vector2int.y));
        else if (resource is Vector3Int vector3int)
            commandBuffer.SetComputeVectorParam(computeShader, resourceName, new Vector3(vector3int.x, vector3int.y, vector3int.z));
        else if (resource is BufferHolder bufferHolder)
            commandBuffer.SetComputeBufferParam(computeShader, kernelIndex, resourceName, bufferHolder.Buffer);
        else if (resource is RenderTexture texture)
            commandBuffer.SetComputeTextureParam(computeShader, kernelIndex, resourceName, texture);
        else throw new NotImplementedException(computeShader.name + ".kernel(" + kernelIndex.ToString() + ")." + resourceName);
    }

    public bool AreFilesReady()
    {
        bool filesReady = true;
        foreach(var watcher in shaderWatchers)
        {
            filesReady &= watcher.IsFileReady();
        }
        return filesReady;
    }
    private RenderTexture GetResultTexture()
    {
        var ResultResource = sharedResources[RGRC.MainTextures.Result] as RenderTexture;
        if (ResultResource == null)
            throw new NullReferenceException("Result RenderTexture is null.");
        else
            return ResultResource;
    }
}