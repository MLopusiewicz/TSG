using UnityEngine;

public class TheWorld : MonoBehaviour {
    public float GetDistance(Vector3 position) {
        return 0;
    }

}


public class WorldData {
    public float[,,] data;
    public Vector3 center;
    public Vector3 size;
    public Vector3 voxel;

    Vector3Int ToVoxel(Vector3 position) {
        Vector3 localPos = position - center + size * 0.5f;
        return new Vector3Int(
            Mathf.FloorToInt(localPos.x / voxel.x),
            Mathf.FloorToInt(localPos.y / voxel.y),
            Mathf.FloorToInt(localPos.z / voxel.z)
        );
    }
    

    public float GetDistance(Vector3 position) {
        Vector3Int voxelPos = ToVoxel(position);
        if (voxelPos.x < 0 || voxelPos.x >= data.GetLength(0) ||
            voxelPos.y < 0 || voxelPos.y >= data.GetLength(1) ||
            voxelPos.z < 0 || voxelPos.z >= data.GetLength(2)) {
            return float.MaxValue;
        }
        return data[voxelPos.x, voxelPos.y, voxelPos.z];
    }
}