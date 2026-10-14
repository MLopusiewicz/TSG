using Codice.CM.Common.Replication;
using log4net.Util;
using UnityEngine;

public class SDFVolumeData : ScriptableObject {
    [SerializeField] float voxelSize = 0.1f;

    [SerializeField] Bounds bounds;
    [SerializeField] Texture3D sdfTexture;

    public Bounds Bounds => bounds;
    public float VoxelSize => voxelSize;
    public Texture3D SDFTexture => sdfTexture;

    public SDFVolumeData(Bounds bounds, float voxelSize, Texture3D sdfTexture) {
        this.voxelSize = voxelSize;
        this.sdfTexture = sdfTexture;
        this.bounds = bounds;
    }

    public Vector3Int GetIndex(Vector3 pos) {
        Vector3 localPos = pos - Bounds.min;
        return new Vector3Int(
           Mathf.Max(0, Mathf.FloorToInt(localPos.x / VoxelSize)),
           Mathf.Max(0, Mathf.FloorToInt(localPos.y / VoxelSize)),
           Mathf.Max(0, Mathf.FloorToInt(localPos.z / VoxelSize))
       );
    }


    public Vector3 GetVoxelPos(Vector3Int index) {
        Vector3 voxelCenter = bounds.min + ((Vector3)index + Vector3.one * 0.5f) * voxelSize;
        return voxelCenter;
    }
}
