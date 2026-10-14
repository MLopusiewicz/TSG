using System;
using Unity.Collections;
using Unity.Mathematics;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;

public struct SimulationSpace : IDisposable {
    [ReadOnly] public NativeArray<float> data;
    public int3 dimensions;
    public float3 boundsMin, boundsMax, boundsSize;
    float voxelSize;

    public static SimulationSpace Make(SDFVolumeData data) {
        var space = new SimulationSpace() {
            dimensions = new int3(data.SDFTexture.width, data.SDFTexture.height, data.SDFTexture.depth),
            boundsMin = -data.Size / 2 + data.Offset,
            boundsMax = data.Size / 2 + data.Offset,
            boundsSize = data.Size,
            voxelSize = data.VoxelSize
        };
        space.data = new NativeArray<float>(space.dimensions.x * space.dimensions.y * space.dimensions.z, Allocator.Persistent);

        space.data.CopyFrom(data.SDFTexture.GetPixelData<float>(0));

        return space;

    }

    public float Sample(float3 worldPos) {

        float3 clampedWorldPos = math.clamp(worldPos, boundsMin, boundsMax); 
        float distanceToSimulationSpace = math.length(worldPos - clampedWorldPos);

        int3 voxelIndex = (int3)math.floor((clampedWorldPos - boundsMin) / voxelSize);


        float3 voxelPos = (float3)voxelIndex * voxelSize; //corner
        float3 simSpacePos = clampedWorldPos - boundsMin;
        float3 voxelNormalizedSpace = (simSpacePos - voxelPos) / voxelSize;


        //https://en.wikipedia.org/wiki/Trilinear_interpolation
        //sample corners
        float c000 = VoxelSample(voxelIndex + new int3(0, 0, 0));
        float c100 = VoxelSample(voxelIndex + new int3(1, 0, 0));
        float c010 = VoxelSample(voxelIndex + new int3(0, 1, 0));
        float c110 = VoxelSample(voxelIndex + new int3(1, 1, 0));
        float c001 = VoxelSample(voxelIndex + new int3(0, 0, 1));
        float c101 = VoxelSample(voxelIndex + new int3(1, 0, 1));
        float c011 = VoxelSample(voxelIndex + new int3(0, 1, 1));
        float c111 = VoxelSample(voxelIndex + new int3(1, 1, 1));

        //lerp to yz plane
        float c00 = math.lerp(c000, c100, voxelNormalizedSpace.x);
        float c10 = math.lerp(c010, c110, voxelNormalizedSpace.x);
        float c01 = math.lerp(c001, c101, voxelNormalizedSpace.x);
        float c11 = math.lerp(c011, c111, voxelNormalizedSpace.x);

        //lerp to z line
        float c0 = math.lerp(c00, c10, voxelNormalizedSpace.y);
        float c1 = math.lerp(c01, c11, voxelNormalizedSpace.y);

        return math.lerp(c0, c1, voxelNormalizedSpace.z) + distanceToSimulationSpace;
    }

    public float3 Gradient(float3 p) {
        float e = voxelSize;
        float3 g = new float3(
            Sample(p + new float3(e, 0, 0)) - Sample(p - new float3(e, 0, 0)),
            Sample(p + new float3(0, e, 0)) - Sample(p - new float3(0, e, 0)),
            Sample(p + new float3(0, 0, e)) - Sample(p - new float3(0, 0, e)));
        return math.normalizesafe(g, new float3(0, 1, 0));
    }

    float VoxelSample(int3 indexVector) {
        indexVector = math.clamp(indexVector, int3.zero, dimensions - 1);
        return data[indexVector.x + indexVector.y * dimensions.x + indexVector.z * dimensions.x * dimensions.y];
    }

    public void Dispose() {
        if (data.IsCreated)
            data.Dispose();
    }
}