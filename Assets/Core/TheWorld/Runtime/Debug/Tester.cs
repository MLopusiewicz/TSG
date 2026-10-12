using UnityEditor;
using UnityEngine;
[ExecuteInEditMode]
public class Tester : MonoBehaviour
{

    public SDFItem item;
    public float distance;

    public void Update() {
        distance = item.GetDistance(transform.position);
    }

}
