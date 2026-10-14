using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Tester : MonoBehaviour {
    public SDFVolumeData SDFVolumeData;
    public Vector3Int snappedIndex;
    SimulationSpace space;
    public float sampledDistance;
    public float simulationSpaceVoxelValue;

    public float textureVoxelValue;
    public float bakeValue;
    SDFItem[] items;
    private void Awake() {
        items = FindObjectsByType<SDFItem>(FindObjectsSortMode.None);
        space = SimulationSpace.Make(SDFVolumeData);
    }

    private void Update() {

        snappedIndex = SDFVolumeData.GetIndex(transform.position);
        simulationSpaceVoxelValue = space.VoxelSample(new int3(snappedIndex.x, snappedIndex.y, snappedIndex.z));
        sampledDistance = space.Sample(new float3(transform.position.x, transform.position.y, transform.position.z));
        textureVoxelValue = SDFVolumeData.SDFTexture.GetPixel(snappedIndex.x, snappedIndex.y, snappedIndex.z).r;

        float minDistance = float.MaxValue;

        foreach (var item in items) {
            float distance = item.GetDistance(SDFVolumeData.GetVoxelPos(snappedIndex));
            if (distance < minDistance) {
                minDistance = distance;
            }
        }

        bakeValue = minDistance;


    }

    private void OnDestroy() {
        space.Dispose();
    }

    private void OnDrawGizmos() {

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one * SDFVolumeData.VoxelSize);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(SDFVolumeData.GetVoxelPos(snappedIndex), Vector3.one * SDFVolumeData.VoxelSize);
    }
}
