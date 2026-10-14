using System;
using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class SimulationLogic : MonoBehaviour {
    public SDFVolumeData volume;

    public SimulationData physicSetup;

    [Header("Symulacja")]
    public int count = 10000;
    public Vector3 gravity = new Vector3(0f, -9.81f, 0f);
    public Vector3 spawnCenter = new Vector3(0f, 15f, 0f);

    public Vector3 spawnSize = new Vector3(10f, 4f, 10f);

    [Header("Rendering")]
    public Mesh mesh;
    public Material material;             // Enable GPU Instancing

    const int MaxBatch = 1023;

    NativeArray<BallData> balls;
    NativeArray<float4x4> matrices;
    NativeArray<Matrix4x4> matricesView;
    RenderParams renderParams;
    SimulationSpace space;
    void Setup() {

        space = SimulationSpace.Make(volume);
        balls = new NativeArray<BallData>(count, Allocator.Persistent);

        matrices = new NativeArray<float4x4>(count, Allocator.Persistent);
        matricesView = matrices.Reinterpret<Matrix4x4>();

        for (int i = 0; i < count; i++) {
            balls[i] = new BallData {
                position = (float3)spawnCenter + new float3(
                    UnityEngine.Random.Range(-0.5f, 0.5f) * spawnSize.x,
                    UnityEngine.Random.Range(-0.5f, 0.5f) * spawnSize.y,
                    UnityEngine.Random.Range(-0.5f, 0.5f) * spawnSize.z),
                velocity = float3.zero
            };
        }

        if (mesh == null) {
            throw new Exception("No mesh in simulation");
        }

        renderParams = new RenderParams(material) {
            shadowCastingMode = ShadowCastingMode.Off,
            receiveShadows = false
        };


    }
    public void Simulate() {
        Clear();
        StopAllCoroutines();
        Setup();
        StartCoroutine(Calculate());
    }


    IEnumerator Calculate() {
        while (true) {
            var job = new BallSimulationJob {
                simulationSpace = space,
                balls = balls,
                matrices = matrices,
                deltaTime = Time.deltaTime,
                physicSetup = physicSetup
            };

            job.Schedule(count, 64).Complete();

            Render();
            yield return null;
        }
    }

    void Render() {
        for (int start = 0; start < count; start += MaxBatch) {
            int n = math.min(MaxBatch, count - start);
            Graphics.RenderMeshInstanced(renderParams, mesh, 0, matricesView, n, start);
        }
    }

    void Clear() {
        space.Dispose();
        if (balls.IsCreated)
            balls.Dispose();
        if (matrices.IsCreated)
            matrices.Dispose();
    }

    void OnDestroy() {
        Clear();
    }

}


