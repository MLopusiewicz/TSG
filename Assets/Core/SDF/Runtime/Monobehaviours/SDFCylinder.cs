using UnityEngine;

public class SDFCylinder : SDFItem {
    [SerializeField] float radius = 1f;
    [SerializeField] float height = 2f;

    //TODO:
    // 1. Add support for rotation
    // 2. Add support for non-uniform scaling.

    public override float GetDistance(Vector3 position) {

        var q = position - transform.position;

        Vector2 point = new Vector2(new Vector2(q.x, q.z).magnitude, Mathf.Abs(q.y));

        Vector2 distance = point - Vector2.Scale(new Vector2(radius, height / 2), transform.lossyScale);
        return Mathf.Max(distance.x, distance.y);

    }


    private void OnDrawGizmos() {
    }

}
