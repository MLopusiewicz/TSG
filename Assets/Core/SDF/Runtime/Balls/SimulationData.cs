using System;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct SimulationData {

    public float3 gravity;

    [Range(0f, 1f)]
    public float bounciness;

    [Range(0f, 1f)]
    public float drag;

    [Range(0f, 1f)]
    public float radius;

}
