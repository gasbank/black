using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;
using JetBrains.Annotations;
#if ADDRESSABLES
using UnityEditor.AddressableAssets.Settings;
#endif
using UnityEditor.Build.Reporting;
using UnityEditor.CrashReporting;

internal static class BlackBuild
{
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
    public static void PerformAndroidBuildMono()
    {
        Environment.SetEnvironmentVariable("DEV_BUILD", "1");
        var locationPathName = "black-mono.apk";
        if (PerformAndroidBuildInternal(false, false, false, locationPathName))
        {
            EditorUtility.RevealInFinder(Path.Combine(Environment.CurrentDirectory, locationPathName));
        }
    }

    [UsedImplicitly]
    public static void PerformAndroidBuild()
    {
        var appBundle = Environment.GetEnvironmentVariable("ANDROID_APP_BUNDLE") == "1";
        PerformAndroidBuildInternal(true, appBundle, false, appBundle ? "build/Black.aab" : "build/Black.apk");
    }

    static bool PerformAndroidBuildInternal(bool il2cpp, bool appBundle, bool run, string locationPathName)
    {
#if ADDRESSABLES
        AddressableAssetSettings.BuildPlayerContent();
#endif
        var isReleaseBuild = Environment.GetEnvironmentVariable("DEV_BUILD") != "1";
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = Scenes,
            target = BuildTarget.Android,
            locationPathName = locationPathName,
        };

        if (run)
        {
            options.options |= BuildOptions.AutoRunPlayer;
        }

        // Split APKs 옵션 켠다. 개발중에는 끄고 싶을 때도 있을 것
        if (il2cpp)
        {
            // 자동 빌드는 IL2CPP로 
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        }
        else
        {
            // 개발 기기에서 바로 보고 싶을 땐 mono로 보자
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
        }

        EditorUserBuildSettings.buildAppBundle = appBundle;

        // Pro 버전일때만 되는 기능이긴 한데, 이걸 켜고 푸시한 경우도 있을테니 여기서 꺼서 안전장치로 작동하게 한다.
        PlayerSettings.SplashScreen.show = false;
        // DEV_BUILD 심볼을 빼서 디버그 메시지 나오지 않도록 한다.
        if (isReleaseBuild)
        {
            RemovingBlackDebugDefine(BuildTargetGroup.Android);
        }

        var cmdArgs = Environment.GetCommandLineArgs().ToList();
        if (ProcessAndroidKeystorePassArg(cmdArgs))
        {
            ProcessBuildNumber(cmdArgs);
            var buildReport = BuildPipeline.BuildPlayer(options);
            if (buildReport.summary.result != BuildResult.Succeeded)
            {
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(-1);
                }

                return false;
            }

            // 빌드 성공!
            return true;
        }

        // 키가 없어서 실패!
        return false;
    }

    static bool ProcessAndroidKeystorePassArg(List<string> cmdArgs)
    {
        // 이미 채워져있다면 더 할 게 없다.
        if (string.IsNullOrEmpty(PlayerSettings.Android.keystorePass) == false
            && string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass) == false)
        {
            return true;
        }

        var keystorePassIdx = cmdArgs.FindIndex(e => e == "-keystorePass");
        string keystorePass;
        if (keystorePassIdx >= 0)
        {
            keystorePass = cmdArgs[keystorePassIdx + 1];
            PlayerSettings.Android.keystorePass = keystorePass;
            PlayerSettings.Android.keyaliasPass = keystorePass;
            return true;
        }
        else
        {
            keystorePass = Environment.GetEnvironmentVariable("KEYSTORE_PASS");
            if (string.IsNullOrEmpty(keystorePass))
            {
                try
                {
                    keystorePass = File.ReadAllText(".keystore_pass").Trim();
                    PlayerSettings.Android.keystorePass = keystorePass;
                    PlayerSettings.Android.keyaliasPass = keystorePass;
                    return true;
                }
                catch
                {
                    Debug.LogError(
                        "-keystorePass argument or '.keystore_pass' file should be provided to build Android APK.");
                }
            }
            else
            {
                PlayerSettings.Android.keystorePass = keystorePass;
                PlayerSettings.Android.keyaliasPass = keystorePass;
                return true;
            }
        }

        return false;
    }

    public static AppMetaInfo ProcessBuildNumber(List<string> cmdArgs)
    {
        var buildNumberIdx = cmdArgs.FindIndex(e => e == "-buildNumber");
        var buildNumber = "<?>";
        if (buildNumberIdx >= 0)
        {
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
    public static void PerformIosBuild()
    {
        PerformIosDistributionBuild(Environment.GetEnvironmentVariable("IOS_PROFILE_GUID"));
    }

    static void PerformIosDistributionBuild(string profileId)
    {
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = Scenes, target = BuildTarget.iOS, locationPathName = "./build"
        };
        // Pro 버전일때만 되는 기능이긴 한데, 이걸 켜고 푸시한 경우도 있을테니 여기서 꺼서 안전장치로 작동하게 한다.
        PlayerSettings.SplashScreen.show = false;
        // 디버그 관련 심볼을 빼서 디버그 메시지 나오지 않도록 한다.
        if (Environment.GetEnvironmentVariable("DEV_BUILD") != "1")
        {
            RemovingBlackDebugDefine(BuildTargetGroup.iOS);

            // Unity Cloud Diagnostics 활성화한다.
            // 실서비스 빌드는 리포트 켜서 활용한다.
            CrashReportingSettings.enabled = true;
        }
        else
        {
            // 개발중 버전 크래시는 리포트 굳이 받을 필요 없다.
            CrashReportingSettings.enabled = false;
        }

        Debug.Log($"Setting CrashReportingSettings.enabled to '{CrashReportingSettings.enabled}'");

        PlayerSettings.iOS.appleDeveloperTeamID = Environment.GetEnvironmentVariable("IOS_TEAM_ID");
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

        // 자동 빌드니까 당연히 Device SDK 사용해야겠지?
        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;

        var cmdArgs = Environment.GetCommandLineArgs().ToList();
        ProcessBuildNumber(cmdArgs);
        var buildReport = BuildPipeline.BuildPlayer(options);
        if (buildReport.summary.result != BuildResult.Succeeded && Application.isBatchMode)
        {
            EditorApplication.Exit(-1);
        }
    }

    static void RemovingBlackDebugDefine(BuildTargetGroup buildTargetGroup)
    {
        var scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        scriptingDefineSymbols = string.Join(";",
            scriptingDefineSymbols.Split(';')
                .Where(e => e != "DEV_BUILD" && e != "CONDITIONAL_DEBUG"));
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, scriptingDefineSymbols);
    }
}