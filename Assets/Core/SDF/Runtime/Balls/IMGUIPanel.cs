using System;
using UnityEngine;

public class IMGUIPanel : MonoBehaviour {
    public int ballCount = 10;

    public SDFVolumeData volume;
    public SimulationLogic target;
    string fps = "";
    private void Update() {
        fps = (1f / Time.deltaTime).ToString("F1");
    }
    private void OnGUI() {

        GUI.Box(new Rect(0, 0, 100, 90), "");
        GUILayout.Label($"Ball Count: {ballCount}");

        GUILayout.Label($"FPS: {fps}");
        GUILayout.Label($"SDF: {volume.Size}");

        using (new GUILayout.HorizontalScope()) {
            if (GUILayout.Button("Reset")) {
                target.Simulate();
            }
        }


    }


}
