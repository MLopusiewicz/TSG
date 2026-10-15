using System;
using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class SimulationLogic : MonoBehaviour {

    [Header("Settings")]
    public SDFVolumeData volume;
    public SimulationData physicSetup;

    [Header("Spawn")]
    public int ballCount = 10000;
    public Bounds spawnBounds;


    [Header("Rendering")]
    public Mesh mesh;
    public Material material;



    GraphicsBuffer positionsBuffer;
    RenderParams renderParams;
    SimulationVolume space;
    NativeArray<float3> positions;
    NativeArray<float3> velocities;
    JobHandle simulationHandle;

    void Setup() {

        space = SimulationVolume.Make(volume);
        positions = new NativeArray<float3>(ballCount, Allocator.Persistent);
        velocities = new NativeArray<float3>(ballCount, Allocator.Persistent);

        for (int i = 0; i < ballCount; i++) {
            positions[i] = (float3)spawnBounds.center + new float3(
                UnityEngine.Random.Range(-0.5f, 0.5f) * spawnBounds.size.x,
                UnityEngine.Random.Range(-0.5f, 0.5f) * spawnBounds.size.y,
                UnityEngine.Random.Range(-0.5f, 0.5f) * spawnBounds.size.z);
            velocities[i] = float3.zero;
        }


        if (mesh == null) {
            throw new Exception("No mesh in simulation");
        }

        positionsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, ballCount, sizeof(float) * 3);

        var mpb = new MaterialPropertyBlock();
        mpb.SetBuffer("_Positions", positionsBuffer);
        mpb.SetFloat("_Radius", physicSetup.radius * 2f);

        renderParams = new RenderParams(material) {
            matProps = mpb,
            worldBounds = new Bounds(Vector3.zero, new Vector3(100, 100, 100)),
            shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
            receiveShadows = true
        };
    }

    public void Simulate() {
        Clear();
        StopAllCoroutines();
        Setup();
        StartCoroutine(SimulationStep());
        StartCoroutine(Render());
    }


    IEnumerator SimulationStep() {
        while (true) {
            var job = new SimulationJob {
                simulationSpace = space,
                positions = positions,
                velocities = velocities,
                deltaTime = Time.fixedDeltaTime,
                physicSetup = physicSetup
            };

            simulationHandle = job.Schedule(ballCount, 64);
            simulationHandle.Complete();
            yield return new WaitForFixedUpdate();

        }
    }

    IEnumerator Render() {
        while (true) {
            positionsBuffer.SetData(positions);
            Graphics.RenderMeshPrimitives(renderParams, mesh, 0, ballCount);
            yield return null;
        }
    }

    void Clear() {
        simulationHandle.Complete();
        space.Dispose();

        if (positionsBuffer != null)
            positionsBuffer.Dispose();
        if (positions.IsCreated)
            positions.Dispose();
        if (velocities.IsCreated)
            velocities.Dispose();
    }


    void OnDestroy() {
        Clear();
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(spawnBounds.center, spawnBounds.size);
    }
}


