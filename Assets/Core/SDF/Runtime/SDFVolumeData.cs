using UnityEngine;

public class SDFVolumeData : ScriptableObject
{
    [SerializeField] Vector3 size = Vector3.one * 10f;
    [SerializeField] Vector3 offset = Vector3.zero;
    [SerializeField] float voxelSize = 0.1f;

    [SerializeField] Texture3D sdfTexture;

    public Vector3 Size => size;
    public Vector3 Offset => offset;
    public float VoxelSize => voxelSize;
    public Texture3D SDFTexture => sdfTexture;

    public SDFVolumeData(Vector3 size, Vector3 offset, float voxelSize, Texture3D sdfTexture) {
        this.size = size;
        this.offset = offset;
        this.voxelSize = voxelSize;
        this.sdfTexture = sdfTexture;
    }

}
