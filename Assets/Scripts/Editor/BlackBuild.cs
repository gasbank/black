using UnityEngine;
using UnityEditor;
using System.Linq;

class BlackBuild {
    static string[] Scenes {
        get {
            return new string[] {
                "Assets/Scenes/Museum.unity",
                "Assets/Scenes/Stage Selection.unity",
                "Assets/Scenes/Main.unity",
            };
        }
    }

    static void PerformAndroidBuild() {
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = Scenes;
        options.target = BuildTarget.Android;
        options.locationPathName = "./black.apk";
        // 개발 관련 심볼을 빼서 디버그 메시지 나오지 않도록 한다.
        if (System.Environment.GetEnvironmentVariable("BLACK_DEV_BUILD") != "1") {
            RemovingSushiDebugDefine(BuildTargetGroup.Android);
        }
        var cmdArgs = System.Environment.GetCommandLineArgs().ToList();
        if (ProcessAndroidKeystorePassArg(options, cmdArgs)) {
            ProcessBuildNumber(cmdArgs);
            BuildPipeline.BuildPlayer(options);
        }
    }

    private static bool ProcessAndroidKeystorePassArg(BuildPlayerOptions options, System.Collections.Generic.List<string> cmdArgs) {
        var keystorePassIdx = cmdArgs.FindIndex(e => e == "-keystorePass");
        var keystorePass = "";
        if (keystorePassIdx >= 0) {
            keystorePass = cmdArgs[keystorePassIdx + 1];
            PlayerSettings.Android.keystorePass = keystorePass;
            PlayerSettings.Android.keyaliasPass = keystorePass;
            return true;
        } else {
            Debug.LogError("-keystorePass argument should be provided to build Android APK.");
        }
        return false;
    }

    public static AppMetaInfo ProcessBuildNumber(System.Collections.Generic.List<string> cmdArgs) {
        var buildNumberIdx = cmdArgs.FindIndex(e => e == "-buildNumber");
        var buildNumber = "<?>";
        if (buildNumberIdx >= 0) {
            buildNumber = cmdArgs[buildNumberIdx + 1];
        }
        var appMetaInfo = AppMetaInfoEditor.CreateAppMetaInfoAsset();
        appMetaInfo.buildNumber = buildNumber;
        appMetaInfo.buildStartDateTime = System.DateTime.Now.ToShortDateString();
#if UNITY_ANDROID
        appMetaInfo.androidBundleVersionCode = PlayerSettings.Android.bundleVersionCode;
#else
        appMetaInfo.androidBundleVersionCode = -1;
#endif
#if UNITY_IOS
        appMetaInfo.iosBuildNumber = PlayerSettings.iOS.buildNumber;
#else
        appMetaInfo.iosBuildNumber = "INVALID";
#endif
        EditorUtility.SetDirty(appMetaInfo);
        AssetDatabase.SaveAssets();
        return appMetaInfo;
    }

    static void PerformIosBuild() {
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = Scenes;
        options.target = BuildTarget.iOS;
        options.locationPathName = "./build";
        // black_DEBUG 심볼을 빼서 디버그 메시지 나오지 않도록 한다.
        if (System.Environment.GetEnvironmentVariable("BLACK_DEV_BUILD") != "1") {
            RemovingSushiDebugDefine(BuildTargetGroup.iOS);
        }
        PlayerSettings.iOS.appleDeveloperTeamID = "TG9MHV97AH";
        var cmdArgs = System.Environment.GetCommandLineArgs().ToList();
        ProcessBuildNumber(cmdArgs);
        BuildPipeline.BuildPlayer(options);
    }

    private static void RemovingSushiDebugDefine(BuildTargetGroup buildTargetGroup) {
        var scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        scriptingDefineSymbols = string.Join(";", scriptingDefineSymbols.Split(';').Where(e => e != "BLACK_DEBUG" && e != "BLACK_ADMIN"));
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, scriptingDefineSymbols);
    }
}
