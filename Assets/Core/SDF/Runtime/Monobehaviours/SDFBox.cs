using UnityEngine;

public class SDFBox : SDFItem {
    [SerializeField] Vector3 size = Vector3.one;
    [SerializeField] Vector3 offset = Vector3.zero;

    //TODO:
    // 1. Add rotation support
    public override float GetDistance(Vector3 position) {
        Vector3 boxCenter = transform.position + Vector3.Scale(offset, transform.lossyScale);
        return sdBox(position - boxCenter, Vector3.Scale(size, transform.lossyScale) * 0.5f);
    }

    //https://iquilezles.org/articles/distfunctions/
    float sdBox(Vector3 p, Vector3 b) {
        Vector3 q = new Vector3(Mathf.Abs(p.x), Mathf.Abs(p.y), Mathf.Abs(p.z)) - b;
        return Mathf.Sqrt(Mathf.Max(q.x, 0.0f) * Mathf.Max(q.x, 0.0f) + Mathf.Max(q.y, 0.0f) * Mathf.Max(q.y, 0.0f) + Mathf.Max(q.z, 0.0f) * Mathf.Max(q.z, 0.0f)) + Mathf.Min(Mathf.Max(q.x, Mathf.Max(q.y, q.z)), 0.0f);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + Vector3.Scale(offset, transform.lossyScale), Vector3.Scale(size, transform.lossyScale));
        Gizmos.color = Color.white;
    }

}