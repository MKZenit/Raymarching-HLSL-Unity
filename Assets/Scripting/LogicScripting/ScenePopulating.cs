using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using AKRC = ApplicationKernelResourcesNames;

[RequireComponent(typeof(ApplicationGpuResources))]
public class ScenePopulating : MonoBehaviour
{
    [SerializeField]
    public enum SceneType
    {
        Box, Sphere, Slash, Stab,
        RandomDistribution
    }

    bool GetTrue => true;
    [Header("General")]
    [DisableIf(nameof(GetTrue))] public SceneType _sceneType;
    public float size_multiplier;

    [Header("Random populating")]
    [SerializeField] Int2 SpheresDescription;
    [SerializeField] Int2 BoxesDescription;
    [SerializeField] Int2 StabsDescription;
    [SerializeField] Int2 SlashesDescription;

    [Serializable]
    struct Int2
    {
        [SerializeField][MinValue(0)] public int count;
        [SerializeField][MinValue(0)] public int seed;
    }

    public GeometryTypeST GeometryType { set { switch (value) {
                case GeometryTypeST.Box: _sceneType = SceneType.Box; break;
                case GeometryTypeST.Sphere: _sceneType = SceneType.Sphere; break;
                case GeometryTypeST.Stab: _sceneType = SceneType.Stab; break;
                case GeometryTypeST.Slash: _sceneType = SceneType.Slash; break;
                default: throw new NotImplementedException("GeometryType not yet implemented");
            }
            OnValidate();
        } }

    private BufferHolder _spheres;
    public BufferHolder Spheres { get { return _spheres; } }

    private BufferHolder _boxes;
    public BufferHolder Boxes { get { return _boxes; } }

    private BufferHolder _slashes;
    public BufferHolder Slashes { get { return _slashes; } }

    private BufferHolder _stabs;
    public BufferHolder Stabs { get { return _stabs; } }

    private bool isInitialized = false;
    ApplicationGpuResources appGpuResources;
    private void Awake()
    {
        _spheres = new(GeometryOverall.SphereStride);
        _boxes = new(GeometryOverall.BoxStride);
        _slashes = new(GeometryOverall.SlashStride);
        _stabs = new(GeometryOverall.StabStride);

        appGpuResources = GetComponent<ApplicationGpuResources>();
        appGpuResources.UpdateResourceGetter(AKRC.GeometryBuffers.Boxes, () => _boxes);
        appGpuResources.UpdateResourceGetter(AKRC.GeometryBuffers.Spheres, () => _spheres);
        appGpuResources.UpdateResourceGetter(AKRC.GeometryBuffers.Slashes, () => _slashes);
        appGpuResources.UpdateResourceGetter(AKRC.GeometryBuffers.Stabs, () => _stabs);

        isInitialized = true;
        OnValidate();
    }

    void OnValidate()
    {
        if(!isInitialized) return;
        
        _boxes.Update(PopulateBoxes());
        _spheres.Update(PopulateSpheres());
        _slashes.Update(PopulateSlashes());
        _stabs.Update(PopulateStabs());

        appGpuResources.UpdateApplicationResource(AKRC.GeometryDimensions.boxes_count, _boxes.Buffer.count);
        appGpuResources.UpdateApplicationResource(AKRC.GeometryDimensions.spheres_count, _spheres.Buffer.count);
        appGpuResources.UpdateApplicationResource(AKRC.GeometryDimensions.slashes_count, _slashes.Buffer.count);
        appGpuResources.UpdateApplicationResource(AKRC.GeometryDimensions.stabs_count, _stabs.Buffer.count);
    }

    private void OnDestroy()
    {
        _boxes.Release();
        _spheres.Release();
        _slashes.Release();
        _stabs.Release();
    }

    public void RespawnAll()
    {
        _sceneType = SceneType.RandomDistribution;
        OnValidate();
    }
    public void Respawn(GeometryTypeST geometryType,
            float width_x, float height_y, float depth_z, float radius, float angle,
            Vector3 position, Vector4 rotation){
        if (!isInitialized) return;

        switch (geometryType)
        {
            case GeometryTypeST.Sphere:
                _boxes.Update(new Box[1]);
                _slashes.Update(new Slash[1]);
                _stabs.Update(new Stab[1]);
                _spheres.Update(new Sphere[1] {
                    new (){ positionRadius = new(position.x,position.y,position.z,radius), rotation = rotation}
                }); break;
            case GeometryTypeST.Box:
                _slashes.Update(new Slash[1]);
                _stabs.Update(new Stab[1]);
                _spheres.Update(new Sphere[1]);
                _boxes.Update(new Box[1] {
                    new (){ position = position, rotation = rotation, dimension = new(width_x, height_y, depth_z)}
                }); break;
            case GeometryTypeST.Slash:
                _stabs.Update(new Stab[1]);
                _spheres.Update(new Sphere[1]);
                _boxes.Update(new Box[1]);
                _slashes.Update(new Slash[1] {
                    new (){ position = position, rotation = rotation, dimensions = new(width_x*0.5f,Mathf.Tan(angle),depth_z) }
                    // precompute angle as height, so that height length is coherent depending on that angle
                }); break;
            case GeometryTypeST.Stab:
                _slashes.Update(new Slash[1]);
                _spheres.Update(new Sphere[1]);
                _boxes.Update(new Box[1]);
                _stabs.Update(new Stab[1] {
                    new (){ position = position, rotation = rotation, tdr = new(Mathf.Tan(angle), depth_z, radius)}
                }); break;
            default:
                throw new NotImplementedException("GeometryType not yet implemented");
        }
        appGpuResources.UpdateApplicationResource(AKRC.GeometryDimensions.boxes_count, _boxes.Buffer.count);
        appGpuResources.UpdateApplicationResource(AKRC.GeometryDimensions.spheres_count, _spheres.Buffer.count);
        appGpuResources.UpdateApplicationResource(AKRC.GeometryDimensions.slashes_count, _slashes.Buffer.count);
        appGpuResources.UpdateApplicationResource(AKRC.GeometryDimensions.stabs_count, _stabs.Buffer.count);
    }


    private Vector3 RandomVector3(Vector3 min, Vector3 max)
    {
        return new(UnityEngine.Random.Range(min.x, max.x),
                   UnityEngine.Random.Range(min.y, max.y),
                   UnityEngine.Random.Range(min.z, max.z));
    }
    private Vector4 RandomRotation(Vector3 min, Vector3 max)
    {
        Vector3 rdmVec3 = RandomVector3(min, max);
        Quaternion rdmRot = Quaternion.Normalize(Quaternion.Euler(rdmVec3));
        return new Vector4(rdmRot.x, rdmRot.y, rdmRot.z, rdmRot.w);
    }

    public Box[] PopulateBoxes()
    {
        switch (_sceneType) {
            case SceneType.RandomDistribution:
                UnityEngine.Random.InitState(BoxesDescription.seed);
                Box[] boxes = new Box[(int)MathF.Max(0, BoxesDescription.count)];
                float sbc = Mathf.Sqrt(BoxesDescription.count);
                for(int i=0; i< BoxesDescription.count; i++)
                {
                    Vector3 p = RandomVector3( new (-5f, -5f, 4.5f), new (5f, 5f,15f) );
                    Vector3 d = RandomVector3( new(0.5f / sbc + 0.1f, 0.5f / sbc + 0.1f, 0.5f / sbc + 0.1f),
                                               new (2 / sbc + 0.1f, 2 / sbc + 0.1f, 2 / sbc + 0.1f) );
                    Vector4 r = RandomRotation(new(-180, -180, -180), new(180, 180, 180));

                    boxes[i] = new() { position = p, dimension = d * size_multiplier, rotation = r };
                }
                return boxes;
            case SceneType.Box:
                return new Box[1] { new() { position = new Vector3(0, 0, 5f), dimension = new Vector3(1.25f, 1.25f, 1.25f) * size_multiplier, rotation = new(0, 0, 0, 1) } };
            default:
                return new Box[1];
        }
    }

    public Sphere[] PopulateSpheres()
    {
        switch (_sceneType)
        {
            case SceneType.RandomDistribution:
                UnityEngine.Random.InitState(SpheresDescription.seed);
                Sphere[] spheres = new Sphere[(int)MathF.Max(0,SpheresDescription.count)];
                float ssc = Mathf.Sqrt(SpheresDescription.count);
                for (int i = 0; i < SpheresDescription.count; i++)
                {
                    Vector3 p = RandomVector3(new(-5f, -5f, 4.5f), new(5f, 5f, 15f));
                    float radius = UnityEngine.Random.Range(0.5f / ssc + 0.1f, 3 / ssc + 0.1f);
                    Vector3 r = RandomRotation(new(-180, -180, -180), new(180, 180, 180));

                    spheres[i] = new() { positionRadius = new(p.x, p.y, p.z, radius * size_multiplier), rotation = r };
                }
                return spheres;
            case SceneType.Sphere:
                return new Sphere[1] { new() { positionRadius = new(0, 0, 5, 1*size_multiplier), rotation = new(0, 0, 0, 1) } };
            default:
                return new Sphere[1];
        }
    }

    public Slash[] PopulateSlashes()
    {
        switch (_sceneType)
        {
            case SceneType.RandomDistribution:
                UnityEngine.Random.InitState(SlashesDescription.seed);
                Slash[] slashes = new Slash[(int)MathF.Max(0, SlashesDescription.count)];
                float ssc = Mathf.Sqrt(SlashesDescription.count);
                for (int i = 0; i < SlashesDescription.count; i++)
                {
                    Vector3 p = RandomVector3(new(-5f, -5f, 4.5f), new(5f, 5f, 15f));
                    float w = UnityEngine.Random.Range(0.5f / ssc + 0.1f, 3 / ssc + 0.1f);
                    float h = UnityEngine.Random.Range(0.15f, 1f);
                    float d = UnityEngine.Random.Range(0.5f / ssc + 0.1f, 3 / ssc + 0.1f);
                    Vector4 r = RandomRotation(new(-180, -180, -180), new(180, 180, 180));

                    slashes[i] = new() { position = p, dimensions = new(w * size_multiplier * 0.5f, MathF.Tan(h), d * size_multiplier), rotation = r };
                }
                return slashes;
            case SceneType.Slash:
                return new Slash[1] { new() { position = new Vector3(0, 0, 5), dimensions = new(0.5f,MathF.Tan((float)Math.PI/4), 1f), rotation = new(0, 0, 0, 1) } };
            default:
                return new Slash[1];
        }
    }
    

    public Stab[] PopulateStabs()
    {
        switch (_sceneType)
        {
            case SceneType.RandomDistribution:
                UnityEngine.Random.InitState(StabsDescription.seed);
                Stab[] stabs = new Stab[(int)MathF.Max(0, StabsDescription.count)];
                float ssc = Mathf.Sqrt(StabsDescription.count);
                float sssc = Mathf.Sqrt(ssc);
                for (int i = 0; i < StabsDescription.count; i++)
                {
                    Vector3 p = RandomVector3(new(-5f, -5f, 4.5f), new(5f, 5f, 15f));
                    float w = UnityEngine.Random.Range(0.5f / sssc + 0.1f, 3 / sssc + 0.1f);
                    float d = UnityEngine.Random.Range(0.5f / ssc + 0.1f, 3 / ssc + 0.1f);
                    float radius = UnityEngine.Random.Range(0.5f / ssc + 0.1f, 3 / ssc + 0.1f);
                    Vector4 r = RandomRotation(new(-180, -180, -180), new(180, 180, 180));

                    stabs[i] = new() { position = p, tdr = new(MathF.Tan(w), d * size_multiplier, radius * size_multiplier * 0.5f), rotation = r };
                }
                return stabs;
            case SceneType.Stab:
                return new Stab[1] { new() { position = new Vector3(0, 0, 5), tdr = new(MathF.Tan((float)Math.PI/2), 1f), rotation = new(0, 0, 0, 1) } };
            default:
                return new Stab[1];
        }
    }

}
