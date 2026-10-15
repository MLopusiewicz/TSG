using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;


[BurstCompile]
struct SimulationJob : IJobParallelFor {

    public NativeArray<float3> positions;
    public NativeArray<float3> velocities;

    [ReadOnly] public SimulationVolume simulationSpace;
    [ReadOnly] public SimulationData physicSetup;
    public float deltaTime;

    public void Execute(int i) {

        float3 v = velocities[i] + physicSetup.gravity * deltaTime;
        float3 position = positions[i] + v * deltaTime;

        float surfaceToSurfaceDistance = simulationSpace.Sample(position) - physicSetup.radius;


        if (surfaceToSurfaceDistance < 0f) {
            float3 surfaceNormal = simulationSpace.Gradient(position);
            position -= surfaceNormal * surfaceToSurfaceDistance;

            float vn = math.dot(v, surfaceNormal);
            vn = math.clamp(vn, float.MinValue, 0);

            float3 vNormal = vn * surfaceNormal;
            float3 vTangent = v - vNormal;


            v = -vNormal * (physicSetup.bounciness);
            v += vTangent * (1 - physicSetup.friction);
        }

        v *= (1f - physicSetup.drag);

        velocities[i] = v;
        positions[i] = position;
    }
}
