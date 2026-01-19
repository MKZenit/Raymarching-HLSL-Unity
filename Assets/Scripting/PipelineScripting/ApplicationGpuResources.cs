using System;
using UnityEngine;

public class ApplicationGpuResources : MonoBehaviour
{
    [SerializeField] CustomPipeline pipeline;
    
    public void UpdateResourceGetter(string name, Func<object> resourceGetter)
    {
        pipeline.UpdateSharedResource(name, resourceGetter);
    }

    public void UpdateApplicationResource(string name, object resource)
    {
        pipeline.UpdateSharedResource(name, resource);
    }
}