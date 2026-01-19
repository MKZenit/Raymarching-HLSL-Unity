using System;
using UnityEngine;

public class BufferHolder
{
    private GraphicsBuffer buffer;
    public GraphicsBuffer Buffer { get { return buffer; } }
    readonly private int stride;

    public BufferHolder(int stride)
    {
        this.buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1, stride);
        this.stride = buffer.stride;
    }

    public void Update(Array array)
    {
        buffer.Release();
        buffer = new(GraphicsBuffer.Target.Structured, (int)MathF.Max(array.Length, 1), stride); // Deallocate the buffer pointer, but does only set it locally
        buffer.SetData(array);
    }

    public void Release()
    {
        buffer.Release();
    }
}
