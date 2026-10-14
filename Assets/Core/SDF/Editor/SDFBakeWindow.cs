using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

//TODO
// 1. add support for uneven voxel sizes

public class SDFBakeWindow : EditorWindow {
    private Bounds bounds;
    private int voxelMargin = 1;
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

        bounds = EditorGUILayout.BoundsField("Bounds", bounds);
        voxelMargin = EditorGUILayout.IntField("Margin", voxelMargin);
        voxelSize = EditorGUILayout.FloatField("Voxel Size", voxelSize);
        if (EditorGUI.EndChangeCheck()) {
            SceneView.RepaintAll();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Automatic bounds", GUILayout.Height(30))) {
            this.bounds = GetVolume();
        }

        if (GUILayout.Button("Bake", GUILayout.Height(30))) {
            Bake();
        }

    }

    private void OnSceneGUI(SceneView sceneView) {
        Handles.color = Color.green;
        Handles.DrawWireCube(bounds.center, bounds.size);
    }

    private void Bake() {
        SDFBaker baker = new SDFBaker(bounds, voxelSize);
        var progress = new EditorBakeProgress("Baking SDF");
        SDFVolumeData volume = baker.Make(progress);

        if (progress.canceled) {
            Debug.Log("Bake canceled");
            return;
        }

        if (!System.IO.Directory.Exists(folder)) {
            System.IO.Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }

        AssetDatabase.CreateAsset(volume.SDFTexture, textureAssetPath);
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

    public Bounds GetVolume() {
        Bounds volume = new Bounds(Vector3.zero, Vector3.zero);

        foreach (var a in FindObjectsByType<SDFItem>(FindObjectsSortMode.None)) {
            volume.Encapsulate(a.GetBounds());
        }
        volume.Expand(voxelMargin * voxelSize);

        return volume;
    }

}

