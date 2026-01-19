using System;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(CustomPipeline))]
public abstract class CustomKernel : MonoBehaviour
{
    private CustomPipeline customPipeline;
    private void OnEnable()
    {
        customPipeline = GetComponent<CustomPipeline>();
        Enable();
    }
    protected void OnValidate()
    {
#pragma warning disable UNT0008 // Null propagation on Unity objects
        customPipeline?.OnValidate();
#pragma warning restore UNT0008 // Null propagation on Unity objects
    }

    protected Vector3Int PerPixelDispatchSize { get { return customPipeline.PerPixelDispatchSize; } }
    protected object GetApplicationResource(string resourceName)
    {
        return customPipeline.GetApplicationResource(resourceName);
    }

    virtual protected void Enable() { }
    abstract public KernelDescription GetKernelDescription();
}

public class KernelDescription
{
    public ComputeShader computeShader;
    public string kernel;
    public Func<(string, object)[]> ressources;
    public Func<(string, object)[]> setSharedProperties;
    public Func<string[]> useSharedProperties;
    public Func<(string, string)[]> texturesTransition;
    public Vector3Int totalDimension;

    public KernelDescription(ComputeShader computeShader, string kernel, Vector3Int totalDimension,
        Func<(string, object)[]> ressources, Func<(string, string)[]> texturesTransition = null,
        Func<(string, object)[]> setSharedProperties = null, Func<string[]> useSharedProperties = null
    ) {
        this.computeShader = computeShader;
        this.kernel = kernel;
        this.ressources = ressources;
        this.setSharedProperties = setSharedProperties;
        this.useSharedProperties = useSharedProperties;
        this.texturesTransition = texturesTransition;
        this.totalDimension = totalDimension;
    }
}