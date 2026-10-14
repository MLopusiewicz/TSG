using System;
using Unity.Collections;
using Unity.Mathematics;

public struct SimulationSpace : IDisposable {
    [ReadOnly] public NativeArray<float> data;
    public int3 dimensions;
    public float3 boundsMin, boundsMax, boundsSize;
    float voxelSize;

    public static SimulationSpace Make(SDFVolumeData data) {
        var space = new SimulationSpace() {
            dimensions = new int3(data.SDFTexture.width, data.SDFTexture.height, data.SDFTexture.depth),
            boundsMin = data.Bounds.min,
            boundsMax = data.Bounds.max,
            boundsSize = data.Bounds.size,
            voxelSize = data.VoxelSize
        };
        space.data = new NativeArray<float>(space.dimensions.x * space.dimensions.y * space.dimensions.z, Allocator.Persistent);

        space.data.CopyFrom(data.SDFTexture.GetPixelData<float>(0));

        return space;

    }

    public float Sample(float3 worldPos) {

        //based on https://en.wikipedia.org/wiki/Trilinear_interpolation
        //proxy box with corners in voxel centers and center in looked up voxel
        //center of -1,-1, -1 voxel from current

        float3 clampedWorldPos = math.clamp(worldPos, boundsMin, boundsMax);
        float distanceToSimulationSpace = math.length(worldPos - clampedWorldPos);
        float3 simSpacePos = clampedWorldPos - boundsMin;


        int3 boxBoundVoxel = (int3)math.round((simSpacePos) / voxelSize) - new int3(1, 1, 1);
        float3 boxBoundMin = (float3)(boxBoundVoxel) * voxelSize;
        float3 posInBoxNormalized = (simSpacePos - boxBoundMin) / (2 * voxelSize);


        //sample corners
        float c000 = VoxelSample(boxBoundVoxel + new int3(0, 0, 0));
        float c100 = VoxelSample(boxBoundVoxel + new int3(1, 0, 0));
        float c010 = VoxelSample(boxBoundVoxel + new int3(0, 1, 0));
        float c110 = VoxelSample(boxBoundVoxel + new int3(1, 1, 0));
        float c001 = VoxelSample(boxBoundVoxel + new int3(0, 0, 1));
        float c101 = VoxelSample(boxBoundVoxel + new int3(1, 0, 1));
        float c011 = VoxelSample(boxBoundVoxel + new int3(0, 1, 1));
        float c111 = VoxelSample(boxBoundVoxel + new int3(1, 1, 1));

        //approx. x by lerping to yz plane
        float c00 = math.lerp(c000, c100, posInBoxNormalized.x);
        float c10 = math.lerp(c010, c110, posInBoxNormalized.x);
        float c01 = math.lerp(c001, c101, posInBoxNormalized.x);
        float c11 = math.lerp(c011, c111, posInBoxNormalized.x);

        //approx. y by lerping to z line
        float c0 = math.lerp(c00, c10, posInBoxNormalized.y);
        float c1 = math.lerp(c01, c11, posInBoxNormalized.y);

        return math.lerp(c0, c1, posInBoxNormalized.z) + distanceToSimulationSpace;
    }


    public float3 Gradient(float3 p) {
        float e = voxelSize;
        float3 g = new float3(
            Sample(p + new float3(e, 0, 0)) - Sample(p - new float3(e, 0, 0)),
            Sample(p + new float3(0, e, 0)) - Sample(p - new float3(0, e, 0)),
            Sample(p + new float3(0, 0, e)) - Sample(p - new float3(0, 0, e)));
        return math.normalizesafe(g, new float3(0, 1, 0));
    }

    public float VoxelSample(int3 indexVector) {
        indexVector = math.clamp(indexVector, int3.zero, dimensions - 1);
        return data[indexVector.x + indexVector.y * dimensions.x + indexVector.z * dimensions.x * dimensions.y];
    }

    public void Dispose() {
        if (data.IsCreated)
            data.Dispose();
    }
}