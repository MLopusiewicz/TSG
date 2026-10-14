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
}
