using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;


struct BallData {
    public float3 position;
    public float3 velocity;
}

[BurstCompile]
struct BallSimulationJob : IJobParallelFor {
    public NativeArray<BallData> balls;
    [WriteOnly] public NativeArray<float4x4> matrices;

    public SimulationSpace simulationSpace;
    public SimulationData physicSetup;
    public float deltaTime;

    public void Execute(int i) {
        var ball = balls[i];

        float3 v = ball.velocity + physicSetup.gravity * deltaTime;
        float3 position = ball.position + v * deltaTime;

        float d = simulationSpace.Sample(position) - physicSetup.radius;


        if (d < 0f) {
            float3 surfaceNormal = simulationSpace.Gradient(position);
            position -= surfaceNormal * d;

            float vn = math.dot(v, surfaceNormal);
            vn = math.abs(math.clamp(vn, float.MinValue, 0));


            //float3 tangent = (v - (vn * surfaceNormal));
            //v += math.normalize(-tangent) * math.clamp(physicSetup.friction, 0f, math.length(tangent));

            v += vn * surfaceNormal * (1 + physicSetup.bounciness);
        }

        v *= (1f - physicSetup.drag);

        ball.velocity = v;
        ball.position = position;
        balls[i] = ball;
        matrices[i] = float4x4.TRS(position, quaternion.identity, new float3(physicSetup.radius * 2f));
    }
}
