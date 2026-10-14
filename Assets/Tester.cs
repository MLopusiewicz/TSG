using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Tester : MonoBehaviour {
    public SDFVolumeData volume;
    public Vector3Int snappedIndex;
    public float sampledDistance;
    public float simulationSpaceVoxelValue;

    public float textureVoxelValue;
    public float bakeValue;

    SimulationSpace space;
    SDFItem[] items;

    private void Awake() {
        items = FindObjectsByType<SDFItem>(FindObjectsSortMode.None);
        space = SimulationSpace.Make(volume);
    }

    //normally i'd put in function with [Button] attribute from Odin inspector so it's not calculated every frame. But displaying it in public floats works 
    private void Update() {
        snappedIndex = GetIndex(transform.position);
        simulationSpaceVoxelValue = space.VoxelSample(new int3(snappedIndex.x, snappedIndex.y, snappedIndex.z));
        sampledDistance = space.Sample(new float3(transform.position.x, transform.position.y, transform.position.z));
        textureVoxelValue = volume.SDFTexture.GetPixel(snappedIndex.x, snappedIndex.y, snappedIndex.z).r;

        float minDistance = float.MaxValue;

        foreach (var item in items) {
            float distance = item.GetDistance(GetVoxelPos(snappedIndex));
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
        if (!Application.isPlaying)
            return;
        if (volume == null)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one * volume.VoxelSize);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(GetVoxelPos(snappedIndex), Vector3.one * volume.VoxelSize);
    }


    public Vector3Int GetIndex(Vector3 pos) {
        Vector3 localPos = pos - volume.Bounds.min;
        return new Vector3Int(
           Mathf.Max(0, Mathf.FloorToInt(localPos.x / volume.VoxelSize)),
           Mathf.Max(0, Mathf.FloorToInt(localPos.y / volume.VoxelSize)),
           Mathf.Max(0, Mathf.FloorToInt(localPos.z / volume.VoxelSize))
       );
    }


    public Vector3 GetVoxelPos(Vector3Int index) {
        Vector3 voxelCenter = volume.Bounds.min + ((Vector3)index + Vector3.one * 0.5f) * volume.VoxelSize;
        return voxelCenter;
    }
}
