using System;
using UnityEngine;

public class IMGUIPanel : MonoBehaviour {


    public SDFVolumeData volume;
    public SimulationLogic target;
    string fps = "";
    private void Update() {
        fps = (1f / Time.deltaTime).ToString("F1");
    }
    private void OnGUI() {

        GUI.Box(new Rect(0, 0, 200, 200), "");
        GUILayout.Label($"Balls: {target.ballCount}");

        GUILayout.Label($"FPS: {fps}");
        GUILayout.Label($"SDF volume: {volume.Bounds}");

        using (new GUILayout.HorizontalScope()) {
            if (GUILayout.Button("Reset")) {
                target.Simulate();
            }
        }
        using (new GUILayout.HorizontalScope()) {
            if (GUILayout.Button("Reset +1K")) {
                target.ballCount += 1000;
                target.Simulate();
            }
        }
        using (new GUILayout.HorizontalScope()) {
            if (GUILayout.Button("Reset +5K")) {
                target.ballCount += 5000;
                target.Simulate();
            }
        }


    }


}
