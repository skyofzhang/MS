using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
public class BuildScript { 
    [MenuItem("Build/Build Android APK")]
    public static void BuildAndroid() {
        BuildPlayerOptions opt = new BuildPlayerOptions();
        opt.scenes = new[] { "Assets/Scenes/MainScene.unity" };
        opt.locationPathName = "Build/MoShou.apk";
        opt.target = BuildTarget.Android;
        opt.options = BuildOptions.None;
        BuildPipeline.BuildPlayer(opt);
    }
}
