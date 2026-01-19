using System;
using UnityEngine;
using AKRC = ApplicationKernelResourcesNames;

[RequireComponent(typeof(CustomPipeline))]
public class RaymarchingCamera : MonoBehaviour
{
    [Range(1f,179f)][SerializeField] private float Fov;

    CustomPipeline pipeline;
    private void OnEnable()
    {
        pipeline = GetComponent<CustomPipeline>();
        pipeline.UpdateSharedResource(AKRC.Camera.CameraFov, new Func<object>(()=>Fov));
    }
}
