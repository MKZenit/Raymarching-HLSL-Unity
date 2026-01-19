using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class TextureHolder
{    
    private RenderTexture texture;
    public RenderTexture Texture { get { return texture;} }
    private RenderTextureDescriptor descriptor;
    public string name;

    public TextureHolder(GraphicsFormat graphicsFormat, bool enableRandomWrite, int width, int height, string name)
    {
        descriptor = new(width, height) {
            graphicsFormat = graphicsFormat,
            enableRandomWrite = enableRandomWrite,
            depthBufferBits = 0
        };
        this.texture = new RenderTexture(width, height, 0);
        this.name = name;
    }
    public TextureHolder(RenderTextureFormat renderTextureFormat, bool enableRandomWrite, bool isSRGB, int width, int height, string name)
    {
        descriptor = new(width, height, renderTextureFormat){
            sRGB = isSRGB,
            enableRandomWrite = enableRandomWrite,
            depthBufferBits = 0
        };
        this.texture = new RenderTexture(descriptor);
        this.name = name;
    }

    public void Update(int width, int height)
    {
        descriptor.width = width;
        descriptor.height = height;
        texture.Release();
        texture = new RenderTexture(descriptor);
        texture.Create();
    }

    public void Release()
    {
        texture.Release();
    }
}
