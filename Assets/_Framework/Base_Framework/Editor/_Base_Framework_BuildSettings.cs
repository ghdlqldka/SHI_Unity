using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Build;

namespace _Base_Framework
{
    // [ExecuteInEditMode]
    public class _Base_Framework_BuildSettings
    {
        private static string LOG_FORMAT = "<color=cyan><b>[_Base_Framework_BuildSettings]</b></color> {0}";

        protected static void ClearUnityConsole()
        {
            var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }

        [MenuItem("Build Settings/Base_Framework")]
        protected static void BuildFor_PC_Android_IOS()
        {
            ClearUnityConsole();

            Debug.LogWarningFormat(LOG_FORMAT, "BuildFor_PC_Android_IOS()" + ", ProductName : <b><color=red>" + _Base_Framework_Config.productName + "</color></b>");

            PlayerSettings.companyName = _Base_Framework_Config.companyName;
            PlayerSettings.productName = _Base_Framework_Config.productName;

            // Icon
            string iconFilePath = _Base_Framework_Config.IconFilePath;
            Texture2D texture = AssetDatabase.LoadMainAssetAtPath(iconFilePath) as Texture2D;
            Debug.AssertFormat(texture != null, "There is no texture in iconFilePath(<b>" + iconFilePath + "</b>)");
            Texture2D[] icons = new Texture2D[1] { texture };
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, icons, IconKind.Any);
            Debug.LogWarningFormat(LOG_FORMAT, "iconFilePath : " + iconFilePath);

            SetResolutionAndPresentation();

            SetOtherSettings();

            // _Debug.Log("+Delete all of child-folder in \"Assts/Resources/Prefabs(Models)\"");
            // EditorUtilities.DeleteAllModelPrefabsInResourcesFolder(); // delete all of Child-FOLDER in "Assets/Resources/Prefabs(Models)"
            // _Debug.Log("-Delete all of child-folder in \"Assts/Resources/Prefabs(Models)\"");

            AddScenesInBuild();

            AssetDatabase.Refresh();

            // EditorUtilities.CopyModelPrefabsToResourcesFolder("Assets/.Resources/Prefabs(Models)/BaseApp/", "Assets/Resources/Prefabs(Models)/BaseApp");

#if UNITY_STANDALONE
            QualitySettings.SetQualityLevel(5); // Ultra
#elif UNITY_ANDROID
            QualitySettings.SetQualityLevel(2); // Medium
#elif UNITY_IOS
		QualitySettings.SetQualityLevel(2); // Medium
#endif
            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.allowDebugging = false;

            /*
            QualitySettings.SetQualityLevel(5, true);
            if (QualitySettings.GetQualityLevel() != 5) {
            _Debug.LogError("Edit->Project settings->Quality is not FANTASTIC");
            }
            */
        }

        private static void SetResolutionAndPresentation()
        {
            // Resolution and Presentation
            PlayerSettings.defaultIsNativeResolution = _Base_Framework_Config.DefaultIsNativeResolution;
            Debug.LogFormat(LOG_FORMAT, "PlayerSettings.defaultIsNativeResolution : <b>" + PlayerSettings.defaultIsNativeResolution + "</b>");
            if (_Base_Framework_Config.DefaultIsNativeResolution == false)
            {
                PlayerSettings.defaultScreenWidth = _Base_Framework_Config.DefaultScreenWidth;
                PlayerSettings.defaultScreenHeight = _Base_Framework_Config.DefaultScreenHeight;
            }
            Debug.LogWarningFormat(LOG_FORMAT, "PlayerSettings.defaultScreenWidth : <color=yellow>" + PlayerSettings.defaultScreenWidth + "</color>, defaultScreenHeight : <color=yellow>" + PlayerSettings.defaultScreenHeight + "</color>");

#if UNITY_STANDALONE
            /*
            for (int i = 0; i < _Base_Framework_Config.SupportedAspectRatios.Length; i++)
            {
                PlayerSettings.SetAspectRatio(_Base_Framework_Config.SupportedAspectRatios[i], true);
            }

            PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Disabled;
            */
            PlayerSettings.resizableWindow = true;
            PlayerSettings.forceSingleInstance = true;
#elif UNITY_ANDROID || UNITY_IOS
            // PlayerSettings.Android.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

            /*
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            */
#endif

            PlayerSettings.SplashScreen.showUnityLogo = false;
        }

        private static void SetOtherSettings()
        {
            BuildTargetGroup targetGroup = BuildTargetGroup.Standalone;

#if UNITY_ANDROID
            targetGroup = BuildTargetGroup.Android;
            PlayerSettings.Android.ARCoreEnabled = false;
#elif UNITY_IOS
		    targetGroup = BuildTargetGroup.iOS;
#endif
            Debug.LogFormat(LOG_FORMAT, "BuildTargetGroup : <b><color=yellow>" + targetGroup + "</color></b>");

            // Other Settings
            // Other Settings/Identification
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.FromBuildTargetGroup(targetGroup), _Base_Framework_Config.applicationIdentifier);
            Debug.LogFormat(LOG_FORMAT, "PlayerSettings.applicationIdentifier : <b><color=yellow>" + PlayerSettings.applicationIdentifier + "</color></b>");
            PlayerSettings.bundleVersion = "" + _Base_Framework_Config.bundleVersion;
            Debug.LogWarningFormat(LOG_FORMAT, "PlayerSettings.bundleVersion : <b><color=yellow>" + PlayerSettings.bundleVersion + "</color></b>");
#if UNITY_ANDROID

            PlayerSettings.Android.bundleVersionCode = _Base_Framework_Config.bundleVersionCode;
            Debug.LogFormat(LOG_FORMAT, "PlayerSettings.Android.bundleVersionCode : <b>" + PlayerSettings.Android.bundleVersionCode + "</b>");

            PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.PreferExternal;
            PlayerSettings.Android.androidTVCompatibility = false;
#elif UNITY_IOS
        PlayerSettings.iOS.locationUsageDescription = "We use your location to apply the most appropriate language to you";
#endif

            // Other Settings/Configuration
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.FromBuildTargetGroup(targetGroup), ScriptingImplementation.Mono2x);
#pragma warning disable 0618
            PlayerSettings.SetIncrementalIl2CppBuild(targetGroup, true);
#pragma warning restore 0618

            string scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(targetGroup));
            Debug.LogWarningFormat(LOG_FORMAT, "Scripting Define Symbols : <color=yellow>" + scriptingDefineSymbols + "</color>");
            /*
            if (scriptingDefineSymbols.Contains("_DEBUG_LOG") == false)
            {
                scriptingDefineSymbols = "_DEBUG_LOG;" + scriptingDefineSymbols;
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(targetGroup), scriptingDefineSymbols);
            }
            */

            if (scriptingDefineSymbols.Contains("__USING_MUI__") == false)
            {
                scriptingDefineSymbols = "__USING_MUI__;" + scriptingDefineSymbols;
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(targetGroup), scriptingDefineSymbols);
            }

            if (scriptingDefineSymbols.Contains("ENABLE_INPUT_SYSTEM") == false)
            {
                scriptingDefineSymbols = "ENABLE_INPUT_SYSTEM;" + scriptingDefineSymbols;
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(targetGroup), scriptingDefineSymbols);
            }

            if (scriptingDefineSymbols.Contains("ENABLE_UNITY_GAME_SERVICES_REMOTE_CONFIG_SUPPORT") == false)
            {
                scriptingDefineSymbols = "ENABLE_UNITY_GAME_SERVICES_REMOTE_CONFIG_SUPPORT;" + scriptingDefineSymbols;
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(targetGroup), scriptingDefineSymbols);
            }
        }

        private static void AddScenesInBuild()
        {
            List<string> scenePathList = new List<string>();

            scenePathList.Add(_Base_Framework_Config.RootPath + "/Scenes/Test.unity");

            Debug.LogFormat(LOG_FORMAT, "scenePathList.Count : <b><color=yellow>" + scenePathList.Count + "</color></b>");
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[scenePathList.Count];
            for (int i = 0; i < scenePathList.Count; i++)
            {
                scenes[i] = new EditorBuildSettingsScene(scenePathList[i], true)
                {
                    enabled = true
                };
            }

            if (scenes == null || EditorBuildSettings.scenes == null)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
            }
            EditorBuildSettings.scenes = scenes;

        }
    }
}