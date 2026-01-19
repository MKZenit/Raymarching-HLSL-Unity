
using System.IO;
using UnityEngine;

public class ComputeShaderWatcher
{
 
    readonly ComputeShader computeShader;
    public ComputeShader ComputeShader { get { return computeShader; } }
    readonly string fullfilePath;
    public ComputeShaderWatcher(ComputeShader computeShader, string fullfilePath)
    {
        this.computeShader = computeShader;
        this.fullfilePath = fullfilePath;

        computeShaderFileWatcher = new(Path.GetDirectoryName(fullfilePath), Path.GetFileName(fullfilePath))
        {
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };
        computeShaderFileWatcher.Changed += OnShaderFileChanged;
    }

    bool isShaderFileWriting = false;
    readonly FileSystemWatcher computeShaderFileWatcher;
    public bool IsFileReady()
    {
        return !isShaderFileWriting;
    }
    void OnShaderFileChanged(object _, FileSystemEventArgs __)
    {
        isShaderFileWriting = true;
        System.Threading.Thread.Sleep(500); // Wait for end of file write
        UnityEditor.EditorApplication.delayCall += () =>
        {
            isShaderFileWriting = false;
        };
    }

    public void Dispose()
    {
        computeShaderFileWatcher.Dispose();
    }

    public override bool Equals(object obj)
    {
        if(obj == null || obj is not ComputeShaderWatcher csw)
            return false;
        return this.fullfilePath.Equals(csw.fullfilePath);
    }
    public override int GetHashCode()
    {
        return computeShaderFileWatcher.GetHashCode();
    }

}
