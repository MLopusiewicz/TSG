using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SDFBakeWindow : EditorWindow {
    private Vector3 size = Vector3.one * 10f;
    private Vector3 offset = Vector3.zero;

    private float voxelSize = 0.1f;

    string sceneName => SceneManager.GetActiveScene().name;
    string volumeAssetName => $"{sceneName}_SDF";
    string folder => $"Assets/Levels/{sceneName}_data/";
    string volumeAssetPath => $"{folder}/{volumeAssetName}.asset";
    string textureAssetPath => $"{folder}/{volumeAssetName}_texture.asset";


    [MenuItem("Tools/Bake Window")]
    public static void ShowWindow() {
        var window = GetWindow<SDFBakeWindow>("SDF Bake");
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
        offset = EditorGUILayout.Vector3Field("Offset", offset);

        voxelSize = EditorGUILayout.FloatField("Voxel Size", voxelSize);

        if (EditorGUI.EndChangeCheck()) {
            SceneView.RepaintAll();
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
            Mathf.Max(1, Mathf.RoundToInt(size.x / voxelSize)),
            Mathf.Max(1, Mathf.RoundToInt(size.y / voxelSize)),
            Mathf.Max(1, Mathf.RoundToInt(size.z / voxelSize))
        );

        Texture3D resultTex = new Texture3D(res.x, res.y, res.z, TextureFormat.RFloat, false);
        var items = FindObjectsByType<SDFItem>(FindObjectsSortMode.None);
        resultTex = BakeSDFTexture(items, resultTex);

        SDFVolumeData volume = new SDFVolumeData(size, offset, voxelSize, resultTex);


        if (!System.IO.Directory.Exists(folder)) {
            System.IO.Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }

        AssetDatabase.CreateAsset(resultTex, textureAssetPath);
        SaveAsset(volume);

        Debug.Log($"Baked to: {volumeAssetPath}");
    }

    void SaveAsset(SDFVolumeData volume) {
        var asset = AssetDatabase.LoadAssetAtPath<SDFVolumeData>(volumeAssetPath);
        if (asset != null) {
            volume.name = volumeAssetName;
            EditorUtility.CopySerialized(volume, asset);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        } else {
            AssetDatabase.CreateAsset(volume, volumeAssetPath);
        }
    }


    public Texture3D BakeSDFTexture(SDFItem[] items, Texture3D tex) {
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
