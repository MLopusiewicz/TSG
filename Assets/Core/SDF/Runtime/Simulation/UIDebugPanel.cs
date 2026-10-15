using UnityEngine;

//Low priority: AI generated 
public class IMGUIPanel : MonoBehaviour {
    public SDFVolumeData volume;
    public SimulationLogic target;

    [Header("View")]
    public float panelWidth = 230f;
    public Vector2 margin = new Vector2(10f, 10f);
    public Color backgroundColor = new Color(0.08f, 0.09f, 0.11f, 0.85f);
    public Color accentColor = new Color(0.30f, 0.65f, 1f, 1f);
    public float referenceHeight = 1080f; 

    float smoothedDelta = 1f / 60f;

    GUIStyle boxStyle, titleStyle, labelStyle, valueStyle, buttonStyle, lineStyle;
    Texture2D bgTex, lineTex, btnNormalTex, btnHoverTex, btnActiveTex;

    void Update() {

        smoothedDelta = Mathf.Lerp(smoothedDelta, Time.unscaledDeltaTime, 0.05f);
    }

    void OnDestroy() {
        Destroy(bgTex);
        Destroy(lineTex);
        Destroy(btnNormalTex);
        Destroy(btnHoverTex);
        Destroy(btnActiveTex);
    }

    static Texture2D MakeTex(Color c) {
        var t = new Texture2D(1, 1, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }


    void InitStyles() {
        if (boxStyle != null)
            return;

        bgTex = MakeTex(backgroundColor);
        lineTex = MakeTex(new Color(1f, 1f, 1f, 0.12f));
        btnNormalTex = MakeTex(new Color(1f, 1f, 1f, 0.08f));
        btnHoverTex = MakeTex(new Color(accentColor.r, accentColor.g, accentColor.b, 0.35f));
        btnActiveTex = MakeTex(new Color(accentColor.r, accentColor.g, accentColor.b, 0.6f));

        boxStyle = new GUIStyle(GUI.skin.box) {
            padding = new RectOffset(14, 14, 12, 14),
            margin = new RectOffset(0, 0, 0, 0),
            stretchHeight = false
        };
        boxStyle.normal.background = bgTex;

        titleStyle = new GUIStyle(GUI.skin.label) {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            margin = new RectOffset(0, 0, 0, 4)
        };
        titleStyle.normal.textColor = accentColor;

        labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 13 };
        labelStyle.normal.textColor = new Color(0.8f, 0.82f, 0.86f);

        valueStyle = new GUIStyle(labelStyle) {
            alignment = TextAnchor.MiddleRight,
            fontStyle = FontStyle.Bold
        };
        valueStyle.normal.textColor = Color.white;

        buttonStyle = new GUIStyle(GUI.skin.button) {
            fontSize = 13,
            fixedHeight = 28,
            margin = new RectOffset(0, 4, 4, 0)
        };
        buttonStyle.normal.background = btnNormalTex;
        buttonStyle.hover.background = btnHoverTex;
        buttonStyle.active.background = btnActiveTex;
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.hover.textColor = Color.white;
        buttonStyle.active.textColor = Color.white;

        lineStyle = new GUIStyle {
            fixedHeight = 1,
            margin = new RectOffset(0, 0, 6, 6)
        };
        lineStyle.normal.background = lineTex;
    }

    void Row(string label, string value) {
        using (new GUILayout.HorizontalScope()) {
            GUILayout.Label(label, labelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label(value, valueStyle);
        }
    }

    void OnGUI() {
        if (target == null)
            return;
        InitStyles();


        float scale = Mathf.Max(0.75f, Screen.height / referenceHeight);
        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));


        var area = new Rect(margin.x, margin.y, panelWidth, Screen.height / scale);
        using (new GUILayout.AreaScope(area))
        using (new GUILayout.VerticalScope(boxStyle, GUILayout.ExpandHeight(false))) {
            GUILayout.Label("Simulation", titleStyle);
            GUILayout.Box(GUIContent.none, lineStyle);

            Row("Balls", target.ballCount.ToString("N0"));

            float fps = 1f / Mathf.Max(smoothedDelta, 0.0001f);
            Color prev = GUI.contentColor;
            GUI.contentColor = fps >= 60f ? new Color(0.4f, 0.9f, 0.5f)
                             : fps >= 30f ? new Color(1f, 0.85f, 0.3f)
                                          : new Color(1f, 0.4f, 0.4f);
            Row("FPS", fps.ToString("F1"));
            GUI.contentColor = prev;

            if (volume != null) {
                GUILayout.Label(volume.Bounds.ToString(), labelStyle);
            }

            GUILayout.Box(GUIContent.none, lineStyle);

            if (GUILayout.Button("Reset", buttonStyle)) {
                target.Simulate();
            }

            using (new GUILayout.HorizontalScope()) {
                if (GUILayout.Button("Reset +1K", buttonStyle)) {
                    target.ballCount += 1000;
                    target.Simulate();
                }
                if (GUILayout.Button("Reset +5K", buttonStyle)) {
                    target.ballCount += 5000;
                    target.Simulate();
                }
            }
        }

        GUI.matrix = oldMatrix;
    }
}