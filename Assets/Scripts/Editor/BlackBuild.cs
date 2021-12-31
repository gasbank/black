using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;

internal static class BlackBuild {
    static List<string> ListAllScenes()
    {
        var scenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                scenes.Add(scene.path);
                Debug.Log(scene.path);
            }
        }

        return scenes;
    }

    static string[] Scenes => ListAllScenes().ToArray();

    [MenuItem("Black/Perform Android Build (Mono)")]
    [UsedImplicitly]
    public static void PerformAndroidBuildMono() {
        Environment.SetEnvironmentVariable("BLACK_DEV_BUILD", "1");
        PerformAndroidBuildInternal(false, false);
        EditorUtility.RevealInFinder("./black.apk");
    }

    [UsedImplicitly]
    public static void PerformAndroidBuild() {
        PerformAndroidBuildInternal(true, false);
    }
    
    [UsedImplicitly]
    public static void PerformAndroidPlayStoreBuild() {
        PerformAndroidBuildInternal(true, true);
    }

    static void PerformAndroidBuildInternal(bool il2cpp, bool appBundle, bool run = false) {
        AddressableAssetSettings.BuildPlayerContent();
        
        var isReleaseBuild = Environment.GetEnvironmentVariable("BLACK_DEV_BUILD") != "1";
        var skipArmV7 = Environment.GetEnvironmentVariable("BLACK_SKIP_ARMV7") == "1";
        BuildPlayerOptions options = new BuildPlayerOptions {
            scenes = Scenes, target = BuildTarget.Android, locationPathName = "./black.apk"
        };
        if (run) {
            options.options |= BuildOptions.AutoRunPlayer;
        }

        // Split APKs 옵션 켠다. 개발중에는 끄고 싶을 때도 있을 것
        if (il2cpp) {
            // 자동 빌드는 IL2CPP로 
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            // Split APK
            PlayerSettings.Android.buildApkPerCpuArchitecture = true;
            // 개발중일때는 ARM64만 빌드하자. 빠르니까...
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            if (isReleaseBuild || skipArmV7 == false) {
                PlayerSettings.Android.targetArchitectures |= AndroidArchitecture.ARMv7;
            }
        } else {
            // 개발 기기에서 바로 보고 싶을 땐 mono로 보자
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
            // Split Apk 필요 없다
            PlayerSettings.Android.buildApkPerCpuArchitecture = false;
            // 개발중일때는 ARM64만 빌드하자. 빠르니까...
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
        }

        EditorUserBuildSettings.buildAppBundle = appBundle;

        // Pro 버전일때만 되는 기능이긴 한데, 이걸 켜고 푸시한 경우도 있을테니 여기서 꺼서 안전장치로 작동하게 한다.
        PlayerSettings.SplashScreen.show = false;
        // BLACK_DEBUG 심볼을 빼서 디버그 메시지 나오지 않도록 한다.
        if (isReleaseBuild) {
            RemovingBlackDebugDefine(BuildTargetGroup.Android);
        }

        var cmdArgs = Environment.GetCommandLineArgs().ToList();
        if (ProcessAndroidKeystorePassArg(cmdArgs)) {
            ProcessBuildNumber(cmdArgs);
            var buildReport = BuildPipeline.BuildPlayer(options);
            if (buildReport.summary.result != BuildResult.Succeeded && Application.isBatchMode) {
                EditorApplication.Exit(-1);
            }
        }
    }

    static bool ProcessAndroidKeystorePassArg(List<string> cmdArgs) {
        // 이미 채워져있다면 더 할 게 없다.
        if (string.IsNullOrEmpty(PlayerSettings.Android.keystorePass) == false
            && string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass) == false) {
            return true;
        }

        var keystorePassIdx = cmdArgs.FindIndex(e => e == "-keystorePass");
        string keystorePass;
        if (keystorePassIdx >= 0) {
            keystorePass = cmdArgs[keystorePassIdx + 1];
            PlayerSettings.Android.keystorePass = keystorePass;
            PlayerSettings.Android.keyaliasPass = keystorePass;
            return true;
        } else {
            keystorePass = Environment.GetEnvironmentVariable("BLACK_KEYSTORE_PASS");
            if (string.IsNullOrEmpty(keystorePass)) {
                try {
                    keystorePass = File.ReadAllText(".black_keystore_pass").Trim();
                    PlayerSettings.Android.keystorePass = keystorePass;
                    PlayerSettings.Android.keyaliasPass = keystorePass;
                    return true;
                } catch {
                    Debug.LogError(
                        "-keystorePass argument or '.black_keystore_pass' file should be provided to build Android APK.");
                }
            } else {
                PlayerSettings.Android.keystorePass = keystorePass;
                PlayerSettings.Android.keyaliasPass = keystorePass;
                return true;
            }
        }

        return false;
    }

    public static AppMetaInfo ProcessBuildNumber(List<string> cmdArgs) {
        var buildNumberIdx = cmdArgs.FindIndex(e => e == "-buildNumber");
        var buildNumber = "<?>";
        if (buildNumberIdx >= 0) {
            buildNumber = cmdArgs[buildNumberIdx + 1];
        }

        var appMetaInfo = AppMetaInfoEditor.CreateAppMetaInfoAsset();
        appMetaInfo.buildNumber = buildNumber;
        appMetaInfo.buildStartDateTime = DateTime.Now.ToShortDateString();
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

    [UsedImplicitly]
    public static void PerformIosAdHocBuild()
    {
        PerformIosDistributionBuild(Environment.GetEnvironmentVariable("BLACK_IOS_TEAM_ID"),
            Environment.GetEnvironmentVariable("BLACK_IOS_AD_HOC_PROFILE_ID"), false);
    }

    [UsedImplicitly]
    public static void PerformIosAppStoreBuild()
    {
        PerformIosDistributionBuild(Environment.GetEnvironmentVariable("BLACK_IOS_TEAM_ID"),
            Environment.GetEnvironmentVariable("BLACK_IOS_APP_STORE_PROFILE_ID"), true);
    }

    static void PerformIosDistributionBuild(string teamId, string profileId, bool universal)
    {
        AddressableAssetSettings.BuildPlayerContent();
        
        if (Application.isBatchMode && string.IsNullOrEmpty(teamId))
        {
            Debug.LogError($"Team ID: {teamId} is empty.");
            EditorApplication.Exit(-1);
        }

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = Scenes, target = BuildTarget.iOS, locationPathName = "./build"
        };
        // Pro 버전일때만 되는 기능이긴 한데, 이걸 켜고 푸시한 경우도 있을테니 여기서 꺼서 안전장치로 작동하게 한다.
        PlayerSettings.SplashScreen.show = false;
        // BLACK_DEBUG 심볼을 빼서 디버그 메시지 나오지 않도록 한다.
        if (Environment.GetEnvironmentVariable("BLACK_DEV_BUILD") != "1")
        {
            RemovingBlackDebugDefine(BuildTargetGroup.iOS);
        }

        PlayerSettings.iOS.appleDeveloperTeamID = teamId;
        if (string.IsNullOrEmpty(profileId))
        {
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        }
        else
        {
            PlayerSettings.iOS.appleEnableAutomaticSigning = false;
            PlayerSettings.iOS.iOSManualProvisioningProfileID = profileId;
            PlayerSettings.iOS.iOSManualProvisioningProfileType = ProvisioningProfileType.Distribution;
        }

        PlayerSettings.SetArchitecture(BuildTargetGroup.iOS,
            (int) (universal ? AppleMobileArchitecture.Universal : AppleMobileArchitecture.ARM64));

        var cmdArgs = Environment.GetCommandLineArgs().ToList();
        ProcessBuildNumber(cmdArgs);
        UnbindAllTemporaryAssets();
        var buildReport = BuildPipeline.BuildPlayer(options);
        if (buildReport.summary.result != BuildResult.Succeeded && Application.isBatchMode)
        {
            EditorApplication.Exit(-1);
        }
    }

    // 에디터 환경에서 개발 중 임시로 연결했던 애셋의 연결을 끊는다.
    static void UnbindAllTemporaryAssets()
    {
        // 에디터 열어놓고 하는 빌드에서는 이 작업 하면 안된다.
        if (Application.isBatchMode == false) return;
        
//        var dialogScene = EditorSceneManager.OpenScene("Assets/Scenes/Dialog.unity");
//        
//        UnbindAllTemporaryAssetsFromScene<BackgroundImageSelector>(dialogScene);
//        UnbindAllTemporaryAssetsFromScene<StoreBackgroundImageSelector>(dialogScene);
//        UnbindAllTemporaryAssetsFromScene<EmojiImageSelector>(dialogScene);
//        UnbindAllTemporaryAssetsFromScene<PortraitImageSelector>(dialogScene);
//        UnbindAllTemporaryAssetsFromScene<ImageSpriteUnbinder>(dialogScene);
//        
//        EditorSceneManager.SaveScene(dialogScene);
//        EditorSceneManager.CloseScene(dialogScene, true);
    }

//    static void UnbindAllTemporaryAssetsFromScene<T>(Scene scene) where T : IResourceSelector
//    {
//        var selectorList = scene.GetRootGameObjects()
//            .Select(e => e.GetComponentsInChildren<T>())
//            .SelectMany(e => e);
//        foreach (var s in selectorList)
//        {
//            s.Unbind();
//        }
//    }

    static void RemovingBlackDebugDefine(BuildTargetGroup buildTargetGroup)
    {
        var scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        scriptingDefineSymbols = string.Join(";",
            scriptingDefineSymbols.Split(';')
                .Where(e => e != "BLACK_DEBUG" && e != "BLACK_ADMIN" && e != "CONDITIONAL_DEBUG"));
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, scriptingDefineSymbols);
    }
}