using Assets.Scripting.PipelineScripting.Utilitary;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class GeometryDebugger : MonoBehaviour
{
    [SerializeField] ScenePopulating scenePopulating;

    [Header("Parameters")]

    // Dynamically enable/disable properties depending on current geometryType
    bool GetTrue => true;
    [SerializeField][DisableIf(condition:nameof(GetTrue))] private GeometryTypeST currentGeometryTypeST;
    [OdinSerialize][DisableIf(nameof(currentGeometryTypeST), GeometryTypeST.Sphere)][DisableIf(nameof(currentGeometryTypeST), GeometryTypeST.Stab)] public float width_x;
    [OdinSerialize][EnableIf(nameof(currentGeometryTypeST), GeometryTypeST.Box)] public float height_y;
    [OdinSerialize][DisableIf(nameof(currentGeometryTypeST), GeometryTypeST.Sphere)] public float depth_z;
    [OdinSerialize][EnableIf(nameof(currentGeometryTypeST), GeometryTypeST.Sphere)][EnableIf(nameof(currentGeometryTypeST), GeometryTypeST.Stab)] public float radius;
    [OdinSerialize][DisableIf(nameof(currentGeometryTypeST), GeometryTypeST.Box)][DisableIf(nameof(currentGeometryTypeST), GeometryTypeST.Sphere)] public float angle;
    [OdinSerialize] public Vector3 position;
    [OdinSerialize] public CustomRotationField orientation; // rotation

    private void OnValidate()
    {
        if (isOne)
            SpawnOne(currentGeometryTypeST);
        else
            SpawnEvery();
    }

    bool isOne = false;
    public void SpawnOne(GeometryTypeST geometryType)
    {
        isOne = true;
        if (geometryType != GeometryTypeST.Sphere
        && geometryType != GeometryTypeST.Box
        && geometryType != GeometryTypeST.Slash
        && geometryType != GeometryTypeST.Stab)
        {
            print("Unsupported Geometry Type.");
            return;
        }

        scenePopulating.GeometryType = geometryType;
        currentGeometryTypeST = geometryType;

        Quaternion qrot = Quaternion.FromToRotation(Vector3.right, orientation.Value);
        Vector4 rotation = new(qrot.x, qrot.y, qrot.z, qrot.w);

        // respawn the geometry with modified properties
        scenePopulating.Respawn(geometryType,
            width_x, height_y, depth_z, radius, angle,
            position, rotation );
    }

    public void SpawnEvery()
    {
        isOne = false;
        scenePopulating.RespawnAll();
    }
}
