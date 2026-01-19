#ifndef GEOMETRY_TYPE_CGINC
#define GEOMETRY_TYPE_CGINC

uint GeometryToUint(int geometryType, int geometryId)
{
    return (uint) ((geometryType << 28) + geometryId);
}
void UintToGeometry(uint val, out int geometryType, out int geometryId)
{
    geometryType = (val >> 28);
    geometryId = (val << 4) >> 4;
}

void UintToIteration(uint val, out int iterationCount, out int iterationChange, out int iterationEnclosing)
{
    iterationCount = val >> 24; // limited to 255
    iterationChange = (val << 8) >> 20; // limited to 4095
    iterationEnclosing = (val << 20) >> 20; // limited to 4095
}

// Geometry Type enum
#define GT_Structural -1
#define GT_Skybox 0
#define GT_Sphere 1
#define GT_Box 2
#define GT_Stab 3
#define GT_Slash 4

struct Geometry
{
    int id;
    int type; // Any GT_ value
};

#endif