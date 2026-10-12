using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BakeWindow : EditorWindow {
    private Vector3 size = Vector3.one * 10f;
    private Vector3 offset = Vector3.zero;
    private Vector3 resolution = new Vector3(32, 32, 32);

    [MenuItem("Tools/Bake Window")]
    public static void ShowWindow() {
        var window = GetWindow<BakeWindow>("Bake");
        window.minSize = new Vector2(300, 120);
    }

    private void OnEnable() {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable() {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.RepaintAll();
    }

    private void OnGUI() {
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();


        size = EditorGUILayout.Vector3Field("Size", size);
        EditorGUILayout.LabelField("voxel size: {}");

        resolution = EditorGUILayout.Vector3Field("Resolution", resolution);
        offset = EditorGUILayout.Vector3Field("Offset", offset);
        if (EditorGUI.EndChangeCheck()) {
            SceneView.RepaintAll(); // odśwież box po zmianie wartości
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Bake", GUILayout.Height(30))) {
            Bake();
        }
    }

    private void OnSceneGUI(SceneView sceneView) {
        Handles.color = Color.green;
        Handles.DrawWireCube(offset, size);
    }

    private void Bake() {
        Vector3Int res = new Vector3Int(
            Mathf.Max(1, Mathf.RoundToInt(resolution.x)),
            Mathf.Max(1, Mathf.RoundToInt(resolution.y)),
            Mathf.Max(1, Mathf.RoundToInt(resolution.z))
        );

        Debug.Log($"Bake! Size: {size}, Resolution: {res}");

        Texture3D resultTex = new Texture3D(res.x, res.y, res.z, TextureFormat.RFloat, false);
        var items = FindObjectsByType<SDFItem>(FindObjectsSortMode.None);
        resultTex = Make(items, resultTex);
        var sceneName = SceneManager.GetActiveScene().name;


        string folder = $"Assets/Levels/{sceneName}_data/";
        if (!System.IO.Directory.Exists(folder)) {
            System.IO.Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }
        AssetDatabase.CreateAsset(resultTex, $"{folder}/{sceneName}_SDF.asset");
    }

    public Texture3D Make(SDFItem[] items, Texture3D tex) {
        for (int x = 0; x < tex.width; x++) {
            for (int y = 0; y < tex.height; y++) {
                for (int z = 0; z < tex.depth; z++) {
                    Vector3 pos = new Vector3(
                        (float)x / (tex.width - 1) * size.x + offset.x - size.x * 0.5f,
                        (float)y / (tex.height - 1) * size.y + offset.y - size.y * 0.5f,
                        (float)z / (tex.depth - 1) * size.z + offset.z - size.z * 0.5f
                    );
                    float minDistance = float.MaxValue;
                    foreach (var item in items) {
                        float distance = item.GetDistance(pos);
                        if (distance < minDistance) {
                            minDistance = distance;
                        }
                    }
                    Color color = new Color(minDistance, 0, 0, 1);
                    tex.SetPixel(x, y, z, color);
                }
            }
        }
        return tex;
    }
}
