using Unity.VisualScripting;
using UnityEngine;

public class DrawRenderTexture : MonoBehaviour
{
    public RenderTexture _compute_rt;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // does not use the source image but the renderTexture of the compute shader.
        Graphics.Blit(_compute_rt, destination);
    }
}
