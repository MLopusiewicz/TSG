using UnityEditor;
using UnityEngine;

public class SDFBaker {
    Bounds bounds;
    float voxelSize;

    public SDFBaker(Bounds bounds, float voxelSize) {
        this.bounds = bounds;
        this.voxelSize = voxelSize;
    }

    public SDFVolumeData Make(IBakeProgress progress = null) {
        var items = GameObject.FindObjectsByType<SDFItem>(FindObjectsSortMode.None);
        return new SDFVolumeData(bounds, voxelSize, BakeSDFTexture(items, progress));

    }


    Texture3D BakeSDFTexture(SDFItem[] items, IBakeProgress progress) {

        var token = false;

        Vector3Int res = new Vector3Int(
            Mathf.Max(0, Mathf.FloorToInt(bounds.size.x / voxelSize)),
            Mathf.Max(0, Mathf.RoundToInt(bounds.size.y / voxelSize)),
            Mathf.Max(0, Mathf.RoundToInt(bounds.size.z / voxelSize))
        );

        Texture3D tex = new Texture3D(res.x, res.y, res.z, TextureFormat.RFloat, false);
        int operations = (tex.width * tex.height * tex.depth);
        var data = new float[res.x * res.y * res.z];
        int reportInterval = Mathf.Max(1, operations / 200);

        try {
            for (int x = 0; x < tex.width; x++) {
                for (int y = 0; y < tex.height; y++) {
                    for (int z = 0; z < tex.depth; z++) {
                        int index = z + y * tex.depth + x * tex.height * tex.depth;

                        if (progress != null && index % reportInterval == 0) {
                            if (progress.Report((float)index / operations, $"Progress: {index}/{operations}")) {
                                token = true;
                                return tex;
                            }
                        }

                        Vector3 voxelCenter = bounds.min + (new Vector3(x + 0.5f, y + 0.5f, z + 0.5f) * voxelSize);

                        float minDistance = float.MaxValue;

                        foreach (var item in items) {
                            float distance = item.GetDistance(voxelCenter);
                            if (distance < minDistance) {
                                minDistance = distance;
                            }
                        }

                        data[index] = minDistance;
                    }
                }
            }

            tex.SetPixelData(data, 0);
            tex.Apply(false);

            EditorUtility.ClearProgressBar();
            return tex;
        } finally {
            progress?.Done(token);
        }
    }
}