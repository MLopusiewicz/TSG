using Unity.Collections;
using UnityEngine;

public class SDFVolumeData : ScriptableObject {

    //These would be normally "readonly" in inspector (Odin inspector) so you wouldn't be able to mess with the values and read the values in inspector,
    //for the sake of this example only SerializeField and HideInInspector.
    [SerializeField, HideInInspector] float voxelSize = 0.1f;
    [SerializeField, HideInInspector] Bounds bounds;
    [SerializeField, HideInInspector] Texture3D sdfTexture;

    public Bounds Bounds => bounds;
    public float VoxelSize => voxelSize;
    public Texture3D SDFTexture => sdfTexture;

    public SDFVolumeData(Bounds bounds, float voxelSize, Texture3D sdfTexture) {
        this.voxelSize = voxelSize;
        this.sdfTexture = sdfTexture;
        this.bounds = bounds;
    }

    private void OnValidate() {
        if (sdfTexture != null) {
            if (sdfTexture.format != TextureFormat.RFloat) {
                Debug.LogWarning("SDF Texture format MUST be RFloat.");
            }
        }
    }
}
