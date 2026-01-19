public static class ApplicationKernelResourcesNames
{
    public static class Camera
    {
        public const string CameraPosition = nameof(CameraPosition);
        public const string CameraVectorX = nameof(CameraVectorX);
        public const string CameraVectorY = nameof(CameraVectorY);
        public const string CameraVectorZ = nameof(CameraVectorZ);
        public const string CameraFov = nameof(CameraFov);

        public const string ModelViewMatrix = nameof(ModelViewMatrix);
        public const string ViewModelMatrix = nameof(ViewModelMatrix);
    }

    public static class GeometryDimensions
    {
        public const string spheres_count = nameof(spheres_count);
        public const string boxes_count = nameof(boxes_count);
        public const string stabs_count = nameof(stabs_count);
        public const string slashes_count = nameof(slashes_count);
    }

    public static class GeometryBuffers
    {
        public const string Boxes = nameof(Boxes);
        public const string Spheres = nameof(Spheres);
        public const string Stabs = nameof(Stabs);
        public const string Slashes = nameof(Slashes);
    }
}