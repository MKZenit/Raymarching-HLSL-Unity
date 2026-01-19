using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using UnityEngine;
using AKRC = ApplicationKernelResourcesNames;
using RGRC = RaymarchingGpuResourcesNames;

public class SphereTracingDebug : CustomKernel
{
    // Properties
    public ComputeShader _raymarchingComputeShader;

    [Title("Main")]
    [EnumToggleButtons][OdinSerialize] public DebugMode _debugMode;

    [OdinSerialize] public GeometryDebugger _geometryDebugger;
    [InfoBox("GeometryType dictates what geometry appears in case there is only one geometry")]
    [OnValueChanged(nameof(OnChangeSceneType))][OdinSerialize][EnumToggleButtons] public ScenePopulationType _sceneType;

    private bool IsIteration => _debugMode == DebugMode.Iteration;
    [Title("Main Iteration")][ShowIfGroup("Iteration", Condition = nameof(IsIteration))][EnumToggleButtons] public IterationRenderingMode _iterationRenderingMode;

    [Title("Iteration Functionnalities")][ShowIfGroup("Iteration")][OdinSerialize] public bool _toggleDebugIteration;
    [ShowIfGroup("Iteration/_debugIteration", Condition = nameof(_toggleDebugIteration))][OdinSerialize] public DebugIteration _debugIteration;

    private bool IsGeometry => _debugMode == DebugMode.Geometry;
    [Title("Main Geometry")][ShowIfGroup("Geometry", Condition = nameof(IsGeometry))][EnumToggleButtons] public GeometryRenderingMode _geometryRenderingMode;
    [OnValueChanged(nameof(OnChangeGeometryType))]
    [Title("Geometry Target")][ShowIfGroup("Geometry")][EnumToggleButtons] public GeometryTypeST _geometryType;
    
    [ShowIfGroup("Geometry")][MinValue(0)] public int geometryId;

    [Title("Geometry Functionnalities")][ShowIfGroup("Geometry")] public bool _toggleDebugPlane;
    [ShowIfGroup("Geometry/_debugPlane", Condition = nameof(_toggleDebugPlane))] public DebugPlane _debugPlane;

    [ShowIfGroup("Geometry")] public bool _toggleDebugZone;
    [ShowIfGroup("Geometry/_debugZone", Condition = nameof(_toggleDebugZone))] public DebugZone _debugZone;

    [Title("Scene Functionalities")]

    override protected void Enable()
    {
        OnChangeSceneType(_sceneType);
    }

    override public KernelDescription GetKernelDescription()
    {
        return new(_raymarchingComputeShader, "CSDebug", PerPixelDispatchSize,
           () => new (string, object)[] {
            ("_renderingMode", (_debugMode==DebugMode.Geometry? (int)_geometryRenderingMode : (int)_iterationRenderingMode)), // _depthMode is an int here while being a boolean in the shader
            ("_debugGeometry", ToVector2Int(
                GetApplicationResource(AKRC.GeometryDimensions.spheres_count),
                GetApplicationResource(AKRC.GeometryDimensions.boxes_count),
                GetApplicationResource(AKRC.GeometryDimensions.stabs_count),
                GetApplicationResource(AKRC.GeometryDimensions.slashes_count)
            )),
            ("_debugFunctionalitiesFlags" , DebugFlagsToInt()),

            (nameof(_debugPlane) + nameof(_debugPlane.Position), _debugPlane.Position),
            (nameof(_debugPlane) + nameof(_debugPlane.Normal) , _debugPlane.Normal),
            (nameof(_debugZone) + nameof(_debugZone.Position) , _debugZone.Position),
            (nameof(_debugZone) + nameof(_debugZone.Radius) , _debugZone.Radius),
            (nameof(_debugZone) + nameof(_debugZone.Opacity) , _debugZone.Opacity),

            (nameof(_debugIteration._maxIterationChange), _debugIteration._maxIterationChange),
            (nameof(_debugIteration._maxIterationEnclosing), _debugIteration._maxIterationEnclosing),
        }, setSharedProperties: () => new (string, object)[]{
            ("_enableIterationDebugStep", (int)_debugIteration._analysisType==1),
            ("_iterationDebugStep", _debugIteration._step)
        }, useSharedProperties: () => new string[] {
            AKRC.Camera.CameraPosition,
            AKRC.Camera.CameraVectorX, AKRC.Camera.CameraVectorY, AKRC.Camera.CameraVectorZ,
            AKRC.Camera.CameraFov,

            RGRC.MainConstants.RenderTextureSize,
            RGRC.MainConstants._maxDistanceThreshold,
            RGRC.MainConstants._maxSteps,

            RGRC.MainTextures.Result,
            RGRC.MainTextures.DepthTexSRV,
            RGRC.MainTextures.GeometryTexSRV,
            RGRC.MainTextures.NormalTexSRV,
            RGRC.MainTextures.IterationTexSRV,

            AKRC.GeometryBuffers.Boxes,
            AKRC.GeometryBuffers.Spheres,
            AKRC.GeometryBuffers.Slashes,
            AKRC.GeometryBuffers.Stabs,

            AKRC.GeometryDimensions.spheres_count,
            AKRC.GeometryDimensions.boxes_count,
            AKRC.GeometryDimensions.stabs_count,
            AKRC.GeometryDimensions.slashes_count
        });
    }

    public enum ScenePopulationType
    {
        Single, Random_Distribution
    }

    void OnChangeSceneType(ScenePopulationType type)
    {
        switch (type)
        {
            case ScenePopulationType.Single:
                _geometryDebugger.SpawnOne(_geometryType);
                break;
            case ScenePopulationType.Random_Distribution:
                _geometryDebugger.SpawnEvery();
                break;
            default:
                throw new Exception("SceneType not yet handled.");
        }
    }
    void OnChangeGeometryType(GeometryTypeST geometryTypeST)
    {
        OnChangeSceneType(_sceneType);
    }

    public GeometryTypeST GeometryType { set {
        switch (value) {
            case GeometryTypeST.Sphere: _geometryType = GeometryTypeST.Sphere; break;
            case GeometryTypeST.Box: _geometryType = GeometryTypeST.Box; break;
            case GeometryTypeST.Slash: _geometryType = GeometryTypeST.Slash; break;
            case GeometryTypeST.Stab: _geometryType = GeometryTypeST.Stab; break;
            default: throw new NotImplementedException("GeometryType not yet implemented");
        }
        OnValidate();
    } }

    public Vector2Int ToVector2Int(object SpheresCount, object BoxesCount, object StabsCount, object SlashesCount)
    {
        return new Vector2Int((int)_geometryType, ClampedGeometryId((int)SpheresCount, (int)BoxesCount, (int)StabsCount, (int)SlashesCount));
    }

    private int ClampedGeometryId(int SpheresCount, int BoxesCount, int StabsCount, int SlashesCount)
    {
        return _geometryType switch
        {
            GeometryTypeST.Structural => geometryId,
            GeometryTypeST.Skybox => geometryId,
            GeometryTypeST.Sphere => (int)MathF.Min(SpheresCount - 1, geometryId),
            GeometryTypeST.Box => (int)MathF.Min(BoxesCount - 1, geometryId),
            GeometryTypeST.Stab => (int)MathF.Min(StabsCount - 1, geometryId),
            GeometryTypeST.Slash => (int)MathF.Min(SlashesCount - 1, geometryId),
            _ => throw new NotImplementedException("Unimplemented geometry type")
        };
    }

    public int DebugFlagsToInt()
    {
        return (int)(
          ((_debugMode == DebugMode.Geometry ? 1 : 0) << (int)DF_FlagPosition.DF_GeometryHighlight)
        | (((_debugMode == DebugMode.Geometry && _toggleDebugPlane) ? 1 : 0) << (int)DF_FlagPosition.DF_Plane)
        | (((_debugMode == DebugMode.Geometry && _toggleDebugZone) ? 1 : 0) << (int)DF_FlagPosition.DF_Zone)
        );
    }

    public enum GeometryRenderingMode
    {
        Result = 0,

        Depth = 1,
        Geometry = 2,
        Normal = 3,
    }
    public enum IterationRenderingMode
    {
        IterationCount = 4,
        IterationChange = 5,
        IterationEnclosing = 6,
    }

    enum DF_FlagPosition
    {
        DF_GeometryHighlight = 0,
        DF_Plane = 1,
        DF_Zone = 2
    }

    [Serializable]
    public struct DebugPlane
    {
        [OdinSerialize] public Vector3 Position;
        [OdinSerialize] public Vector3 Normal;
    }

    [Serializable]
    public struct DebugZone
    {
        [OdinSerialize] public Vector2 Position;
        [OdinSerialize][Range(0, 1)] public float Radius;
        [OdinSerialize][Range(0, 1)] public float Opacity;
    }

    [Serializable]
    public struct DebugIteration
    {
        [OdinSerialize][EnumToggleButtons] public AnalysisType _analysisType;
        [OdinSerialize][ShowIf("_analysisType", AnalysisType.Step)][Range(0, 64)] public uint _step;
        [OdinSerialize] public uint _maxIterationChange;
        [OdinSerialize] public uint _maxIterationEnclosing;

        public enum AnalysisType
        {
            Frame = 0, Step = 1
        }
    }

    public enum DebugMode
    {
        Geometry, Scene, Iteration
    }
}
