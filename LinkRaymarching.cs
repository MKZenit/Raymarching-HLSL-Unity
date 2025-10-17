using UnityEngine;

public class LinkRaymarching: MonoBehaviour
{
    public RenderTexture _outputRenderTexture;

    public ComputeShader _computeShader;
    private int kernelIndex;

    [Header("Main parameters")]
    [Range(0.001f, 10)]
    public float _minDistanceThreshold;
    [Range(100, 10000)]
    public float _maxDistanceThreshold;
    [Range(1, 250)]
    public int _maxSteps;

    [Header("Test parameter")]
    [Range(0.1f,4f)]
    public float _testParameter;


    struct Sphere
    {
        public Vector3 position;
        public float distance;
    };
    struct Box
    {
        public Vector3 position;
        public Vector4 rotation;
        public Vector3 dimension;
    };

    struct Strike
    {
        public Vector3 position;
        public float width;
        public Vector3 rotation;
        public float depth;
    };

    GraphicsBuffer InputGeometryBox;
    GraphicsBuffer InputGeometrySphere;

    GraphicsBuffer InputGeometrySlash;
    GraphicsBuffer InputGeometryStab;

    void OnEnable()
    {
        _outputRenderTexture.enableRandomWrite = true;
        _outputRenderTexture.Create();

        kernelIndex = _computeShader.FindKernel("CSMain");

        Box[] Boxes = new Box[] { new() { position = new Vector3(4, 2.5f, 8.25f), rotation = new Vector4(0, 0, 0, 1), dimension = new Vector3(1, 1, 1)} };
        InputGeometryBox = new(GraphicsBuffer.Target.Structured, Boxes.Length, (1+3*3)*sizeof(float));
        InputGeometryBox.SetData(Boxes);

        Sphere[] Spheres = new Sphere[] { new() { position = new Vector3(-0.35f, -0.35f, 9.5f), distance = 0.75f }, new() { position = new Vector3(0, 0, 10), distance = 1f }, new() { position = new Vector3(0, 2, 10), distance = 1f } };
        InputGeometrySphere = new(GraphicsBuffer.Target.Structured, Spheres.Length, 4 * sizeof(float));
        InputGeometrySphere.SetData(Spheres);


        Strike[] Slashes = new Strike[] { new() { position = new Vector3(-4f, 3f, 7.5f), width = 1, rotation = new Vector3(0, 0, 0), depth = 2f },
                                          new() { position = new Vector3(0.6f, -5.5f, 7.5f), width = 2, rotation = new Vector3(0, 0, 0), depth = 1f }};
        InputGeometrySlash = new(GraphicsBuffer.Target.Structured, Slashes.Length, 8 * sizeof(float));
        InputGeometrySlash.SetData(Slashes);

        Strike[] Stabs = new Strike[] { new() { position = new Vector3(-0.5f, -6f, 9f), width = Mathf.PI/5f, rotation = new Vector3(0, 0, 0), depth = 2f },
                                        new() { position = new Vector3(3f, -1f, 7.5f), width = Mathf.PI/3.5f, rotation = new Vector3(0, 0, 0), depth = 3f },
                                        new() { position = new Vector3(-3f, -7f, 9f), width = Mathf.PI/12f, rotation = new Vector3(0, 0, 0), depth = 6f },
                                        new() { position = new Vector3(0f, 3f, 8f), width = Mathf.PI/3.55f, rotation = new Vector3(0, 0, 0), depth = 3f }};
        InputGeometryStab = new(GraphicsBuffer.Target.Structured, Stabs.Length, 8 * sizeof(float));
        InputGeometryStab.SetData(Stabs);

    }

    void FixedUpdate()
    {
        _computeShader.SetFloat("_testParameter", _testParameter);

        _computeShader.SetFloat("_minDistanceThreshold", _minDistanceThreshold);
        _computeShader.SetFloat("_maxDistanceThreshold", _maxDistanceThreshold);
        _computeShader.SetFloat("_maxSteps", _maxSteps);

        _computeShader.SetVector("texturePixelSize", new Vector4(_outputRenderTexture.width, _outputRenderTexture.height) );

        _computeShader.SetMatrix("WorldToViewMatrix", Camera.main.worldToCameraMatrix);
        _computeShader.SetMatrix("ProjectionMatrix", Camera.main.projectionMatrix);

        _computeShader.SetBuffer(kernelIndex, "InputGeometryBox", InputGeometryBox);
        _computeShader.SetBuffer(kernelIndex, "InputGeometrySphere", InputGeometrySphere);

        _computeShader.SetBuffer(kernelIndex, "InputGeometrySlash", InputGeometrySlash);
        _computeShader.SetBuffer(kernelIndex, "InputGeometryStab", InputGeometryStab);

        _computeShader.SetTexture(kernelIndex, "Result", _outputRenderTexture);
        _computeShader.Dispatch(kernelIndex, (int)(_outputRenderTexture.width / 32), (int)(_outputRenderTexture.height / 32), 1);
    }

    private void OnDisable()
    {
        InputGeometryBox.Release();
        InputGeometrySphere.Release();

        InputGeometrySlash.Release();
        InputGeometryStab.Release();
    }
}
