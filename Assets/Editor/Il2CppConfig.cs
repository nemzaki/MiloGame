using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class Il2CppConfig : IPreprocessBuildWithReport {
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report) {
        // Only apply for Android builds
        if (report.summary.platform == BuildTarget.Android) {
            PlayerSettings.SetAdditionalIl2CppArgs("--compiler-flags=\"-fbracket-depth=1024\"");
            UnityEngine.Debug.Log("Applied -fbracket-depth=1024 for Android build.");
        }
    }
}