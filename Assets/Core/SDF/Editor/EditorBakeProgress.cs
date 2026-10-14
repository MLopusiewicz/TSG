using UnityEditor;


public interface IBakeProgress {
    bool Report(float progress, string message);
    void Done(bool canceled);
}

public class EditorBakeProgress : IBakeProgress {
    readonly string title;
    public bool canceled;
    public EditorBakeProgress(string title = "SDF Bake") {
        this.title = title;
    }

    public bool Report(float progress, string message) =>
        EditorUtility.DisplayCancelableProgressBar(title, message, progress);

    public void Done(bool canceled) {
        this.canceled = canceled;
        EditorUtility.ClearProgressBar();
    }
}
