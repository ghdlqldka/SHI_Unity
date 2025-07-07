using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine.Rendering;
// using _Base_Framework;
using UnityEditor.Build;

namespace _Magenta_Framework
{
    // [ExecuteInEditMode]
    public class _Magenta_Framework_BuildSettings
    {
        private static string LOG_FORMAT = "<color=cyan><b>[_Magenta_Framework_BuildSettings]</b></color> {0}";

        protected static void ClearUnityConsole()
        {
            var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }

        [MenuItem("Build Settings/Magenta_Framework")]
        protected static void BuildFor_PC_Android_IOS()
        {
            ClearUnityConsole();

            Debug.LogWarningFormat(LOG_FORMAT, "BuildFor_PC_Android_IOS()" + ", ProductName : <b><color=blue>" + _Magenta_Framework_Config.productName + "</color></b>");

            PlayerSettings.companyName = _Magenta_Framework_Config.companyName;
            PlayerSettings.productName = _Magenta_Framework_Config.productName;

            // Icon
            string iconFilePath = _Magenta_Framework_Config.IconFilePath;
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
#elif UNITY_WEBGL
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
            PlayerSettings.defaultIsNativeResolution = _Magenta_Framework_Config.DefaultIsNativeResolution;
            Debug.LogFormat(LOG_FORMAT, "PlayerSettings.defaultIsNativeResolution : <b>" + PlayerSettings.defaultIsNativeResolution + "</b>");
            if (_Magenta_Framework_Config.DefaultIsNativeResolution == false)
            {
                PlayerSettings.defaultScreenWidth = _Magenta_Framework_Config.DefaultScreenWidth;
                PlayerSettings.defaultScreenHeight = _Magenta_Framework_Config.DefaultScreenHeight;
            }
            Debug.LogWarningFormat(LOG_FORMAT, "PlayerSettings.defaultScreenWidth : " + PlayerSettings.defaultScreenWidth + ", defaultScreenHeight : " + PlayerSettings.defaultScreenHeight);

#if UNITY_STANDALONE
            /*
            Debug.Assert((int)AspectRatio.AspectOthers == 0);
            Debug.Assert((int)AspectRatio.Aspect16by9 == 4);
            for (int i = 0; i <= (int)AspectRatio.Aspect16by9; i++)
            {
                PlayerSettings.SetAspectRatio((AspectRatio)i, false);
            }
            */
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
            // PlayerSettings.Android.ARCoreEnabled = false;
#elif UNITY_IOS
		    targetGroup = BuildTargetGroup.iOS;
#endif
            Debug.LogFormat(LOG_FORMAT, "<b>BuildTargetGroup : " + targetGroup + "</b>");

            // Other Settings/Redering
#if UNITY_ANDROID
            GraphicsDeviceType[] apis = new GraphicsDeviceType[] 
            {
                // GraphicsDeviceType.Vulkan, // Vulkan graphics API, which is not supported by ARCore!!!!!
                GraphicsDeviceType .OpenGLES3
            };
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, apis); // You have enabled the Vulkan graphics API, which is not supported by ARCore
#endif

            // Other Settings/Identification
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.FromBuildTargetGroup(targetGroup), _Magenta_Framework_Config.applicationIdentifier);
            Debug.LogFormat(LOG_FORMAT, "PlayerSettings.applicationIdentifier : <b>" + PlayerSettings.applicationIdentifier + "</b>");
            PlayerSettings.bundleVersion = "" + _Magenta_Framework_Config.bundleVersion;
            Debug.LogWarningFormat(LOG_FORMAT, "PlayerSettings.bundleVersion : <b>" + PlayerSettings.bundleVersion + "</b>");

#if UNITY_ANDROID
            PlayerSettings.Android.bundleVersionCode = _Magenta_Framework_Config.bundleVersionCode;
            Debug.LogFormat(LOG_FORMAT, "PlayerSettings.Android.bundleVersionCode : <b>" + PlayerSettings.Android.bundleVersionCode + "</b>");

            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24; // ARCore Required apps require a minimum SDK version of 24!!!!!
            // PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel30;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            // Other Settings/Configuration
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.FromBuildTargetGroup(targetGroup), ScriptingImplementation.IL2CPP);
            // PlayerSettings.SetIncrementalIl2CppBuild(targetGroup, true);
            // Target Architectures
            Debug.LogWarningFormat(LOG_FORMAT, "Architecture : " + PlayerSettings.GetArchitecture(NamedBuildTarget.FromBuildTargetGroup(targetGroup)));
            AndroidArchitecture architecture = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
            PlayerSettings.SetArchitecture(NamedBuildTarget.FromBuildTargetGroup(targetGroup), (int)architecture);

            PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.PreferExternal;
            PlayerSettings.Android.androidTVCompatibility = false;

#elif UNITY_IOS
            PlayerSettings.iOS.buildNumber = _AR_Framework_Config.buildNumber;

            PlayerSettings.iOS.appleDeveloperTeamID = _AR_Framework_Config.appleDeveloperTeamID;
            PlayerSettings.iOS.appleEnableAutomaticSigning = _AR_Framework_Config.appleEnableAutomaticSigning;

            // Other Settings/Configuration
            PlayerSettings.SetScriptingBackend(targetGroup, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetIncrementalIl2CppBuild(targetGroup, true);

            PlayerSettings.iOS.cameraUsageDescription = "We use your camera for Augmented Reality";
            PlayerSettings.iOS.locationUsageDescription = "We use your location to apply the most appropriate language to you";

            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            // PlayerSettings.iOS.targetOSVersionString = "11.0";

            // PlayerSettings.iOS.
            PlayerSettings.iOS.hideHomeButton = _AR_Framework_Config.hideHomeButton;
#endif

            string scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(targetGroup));
            Debug.LogWarningFormat(LOG_FORMAT, "Scripting Define Symbols : " + scriptingDefineSymbols);
            /*
            if (scriptingDefineSymbols.Contains("_DEBUG_LOG") == false)
            {
                scriptingDefineSymbols = "_DEBUG_LOG;" + scriptingDefineSymbols;
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(targetGroup), scriptingDefineSymbols);
            }
            */

            // 
            if (scriptingDefineSymbols.Contains("RENDERING_URP") == false)
            {
                scriptingDefineSymbols = "RENDERING_URP;" + scriptingDefineSymbols;
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(targetGroup), scriptingDefineSymbols);
            }

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

            /*
            if (scriptingDefineSymbols.Contains("ENABLE_UNITY_GAME_SERVICES_REMOTE_CONFIG_SUPPORT") == false)
            {
                scriptingDefineSymbols = "ENABLE_UNITY_GAME_SERVICES_REMOTE_CONFIG_SUPPORT;" + scriptingDefineSymbols;
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(targetGroup), scriptingDefineSymbols);
            }
            */
            _Base_Framework.AddDefineSymbols.Remove("ENABLE_UNITY_GAME_SERVICES_REMOTE_CONFIG_SUPPORT");

#if false // AR
            // AR Foundation Remote
            _Base_Framework.AddDefineSymbols.Add("AR_FOUNDATION_REMOTE_INSTALLED");
            _Base_Framework.AddDefineSymbols.Add("ARFOUNDATION_4_0_OR_NEWER");
            _Base_Framework.AddDefineSymbols.Add("ARFOUNDATION_4_0_2_OR_NEWER");
            _Base_Framework.AddDefineSymbols.Add("ARFOUNDATION_4_1_OR_NEWER");
            _Base_Framework.AddDefineSymbols.Add("ENABLE_AR_FOUNDATION_REMOTE_LOCATION_SERVICES");

            _Base_Framework.AddDefineSymbols.Add("ENABLE_ARGPS_LOCATION");
#endif

#if UNITY_ANDROID
            _Base_Framework.AddDefineSymbols.Add("ANDROID_JNI_INSTALLED");

            _Base_Framework.AddDefineSymbols.Remove("UNITY_XR_ARKIT_LOADER_ENABLED");
#elif UNITY_IOS
            // UNITY_XR_ARKIT_LOADER_ENABLED
            _Base_Framework.AddDefineSymbols.Add("UNITY_XR_ARKIT_LOADER_ENABLED");

            _Base_Framework.AddDefineSymbols.Remove("ANDROID_JNI_INSTALLED");
#else
            _Base_Framework.AddDefineSymbols.Remove("ANDROID_JNI_INSTALLED");
            _Base_Framework.AddDefineSymbols.Remove("UNITY_XR_ARKIT_LOADER_ENABLED");
#endif

            _Base_Framework.AddDefineSymbols.Remove("UNITY_XR_ARKIT_FACE_TRACKING_ENABLED");

#if UNITY_WEBGL
            _Base_Framework.AddDefineSymbols.Remove("ROS2");
            _Base_Framework.AddDefineSymbols.Remove("ROS_TCP_CONNECTOR_V070");
#else
            _Base_Framework.AddDefineSymbols.Add("ROS2");
            _Base_Framework.AddDefineSymbols.Add("ROS_TCP_CONNECTOR_V070");
#endif

            PlayerSettings.allowUnsafeCode = true;
        }

#if false
        static class AddDefineSymbols
        {
            public static void Add(string define)
            {
                var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
                var allDefines = new HashSet<string>(definesString.Split(';'));

                if (allDefines.Contains(define))
                {
                    return;
                }

                allDefines.Add(define);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget),
                    string.Join(";", allDefines));
            }

            public static void Remove(string define)
            {
                string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
                var allDefines = new HashSet<string>(definesString.Split(';'));
                allDefines.Remove(define);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget),
                    string.Join(";", allDefines));
            }
        }
#endif

        private static void AddScenesInBuild()
        {
            List<string> scenePathList = new List<string>();

            scenePathList.Add(_Magenta_Framework_Config.RootPath + "/Scenes/Test.unity");

            Debug.LogFormat(LOG_FORMAT, "scenePathList.Count : <b>" + scenePathList.Count + "</b>");
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
                Debug.LogError("");
            }
            EditorBuildSettings.scenes = scenes;

        }
    }
}