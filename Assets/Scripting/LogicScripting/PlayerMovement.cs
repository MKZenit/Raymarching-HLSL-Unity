using System;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using AKRC = ApplicationKernelResourcesNames;

[RequireComponent(typeof(ApplicationGpuResources))]
public class PlayerMovement: MonoBehaviour
{
    [SerializeField] private float directionalAcceleration;
    [SerializeField] private float maxMovementAcceleration;
    [SerializeField] private float deceleration;

    private Vector3 velocity = new (0,0,0);
    public Vector3 Velocity { get { return velocity; } }

    private Vector3 position = new (0,0,0);
    public Vector3 Position { get { return position; } }


    //private Vector3 gravityDirection = new(0,-1,0); // defines what "down" is from player perspective
    //private float gravityIntensity = 0f;
    [SerializeField] private float mouseSensitivity;

    private Quaternion rotation = new (0,0,0,1);
    public Quaternion Rotation { get { return rotation; } }


    ApplicationGpuResources gpuResources;
    private void OnEnable()
    {
        gpuResources = GetComponent<ApplicationGpuResources>();
        gpuResources.UpdateResourceGetter(AKRC.Camera.CameraPosition, () => Position );
        gpuResources.UpdateResourceGetter(AKRC.Camera.CameraVectorX, () => { return Vector3.Normalize(Rotation * Vector3.right); });
        gpuResources.UpdateResourceGetter(AKRC.Camera.CameraVectorY, () => { return Vector3.Normalize(Rotation * Vector3.up); });
        gpuResources.UpdateResourceGetter(AKRC.Camera.CameraVectorZ, () => { return Vector3.Normalize(Rotation * Vector3.forward); });
    }

    private void Start()
    {
        Quaternion qYaw = Quaternion.AngleAxis(90, Vector3.up);
        Quaternion qPitch = Quaternion.AngleAxis(-90, Vector3.right);
        rotation = qYaw * qPitch;
    }

    bool wasUnfocused = false;
    float yaw = 0, pitch = 0;
    private void Update()
    {
        Vector3 curAcceleration = new (0,0,0);
        if (Input.GetKey(KeyCode.W))
        {
            curAcceleration.z += 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            curAcceleration.x -= 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            curAcceleration.z -= 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            curAcceleration.x += 1;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            curAcceleration.y += 1;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            curAcceleration.y -= 1;
        }

        float nbComposant = (Math.Abs(curAcceleration.x) + Math.Abs(curAcceleration.y) + Math.Abs(curAcceleration.z));
        if (curAcceleration.magnitude != 0)
            curAcceleration /= nbComposant;

        float deltaMaxMovementAcceleration = (maxMovementAcceleration / nbComposant) * Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftControl))
        {
            curAcceleration /= 8f;
            deltaMaxMovementAcceleration /= 8f;
        }

        velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * deceleration);
        velocity += directionalAcceleration * Time.deltaTime * curAcceleration;

        velocity = Vector3.Min(velocity, new Vector3(deltaMaxMovementAcceleration, deltaMaxMovementAcceleration, deltaMaxMovementAcceleration) );
        velocity = Vector3.Max(velocity, -new Vector3(deltaMaxMovementAcceleration, deltaMaxMovementAcceleration, deltaMaxMovementAcceleration) );

        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;

        if(Application.isFocused & wasUnfocused)
        {
            mx = my = 0;
            wasUnfocused = false;
        }
        wasUnfocused |= !Application.isFocused;

        yaw += mx;
        pitch -= my;

        pitch = Mathf.Clamp(pitch, -89, 89);

        Quaternion qYaw = Quaternion.AngleAxis(yaw, Vector3.up);
        Quaternion qPitch = Quaternion.AngleAxis(pitch, Vector3.right);

        rotation = qYaw * qPitch;
        
        Vector3 rotatedDisplacement = qYaw * new Vector3(velocity.x, 0, velocity.z); // Orient xy movements on camera direction
        position += new Vector3(rotatedDisplacement.x, 0, rotatedDisplacement.z);
        position.y += velocity.y;

    }
}