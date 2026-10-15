using UnityEngine;

public abstract class SDFItem : MonoBehaviour {
    public abstract float GetDistance(Vector3 position);
    public abstract Bounds GetBounds();

    protected virtual void OnDrawGizmos() {
        Gizmos.color = Color.red;
        var bounds = GetBounds();
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        Gizmos.color = Color.white;
    }
}
