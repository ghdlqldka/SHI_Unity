//////////////////////////////////////////////////////
// CrossSection Install Wizard Configuration    	//
//					                                //
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

#if UNITY_EDITOR
using Configuration = WorldSpaceTransitions.CrossSection.Editor.InstallWizard.Configuration;
namespace WorldSpaceTransitions.CrossSection.Editor.InstallWizard
{
    [CreateAssetMenu(fileName = "Configuration", menuName = "CrossSection/Install Wizard/Create Configuration Asset")]
    public sealed class Configuration : ScriptableObject
    {
//#pragma warning disable CS0414

        internal static bool isReady 
        { 
            get
            { 
                if(_instance == null)
                    TryGetInstance();
                return _instance != null;
            } 
        }
        
        [SerializeField]
        private RenderPipeline _renderPipeline = RenderPipeline.Built_in_Legacy;

        [SerializeField]
        internal bool showInstallerOnReload = true;

        [SerializeField][Space]
        private Texture2D _titleImage = null;

        [SerializeField][Space]
        private string _manualUrl = "https://virtualplayground.d2.pl/wiki/CrossSection";

        [SerializeField][Space]
        private TextAsset _version = null;

        //[SerializeField][Space]
        //private Object[] _basePackagesBuiltin = null;
#if RENDERING_URP
        [SerializeField]
        private Object[] _basePackagesURP = null;
        [SerializeField]
        private float _packagesURP_version = 1.0f;
#endif
#if RENDERING_HDRP
        [SerializeField]
        [Space]
        private Object[] _basePackagesHDRP = null;
        [SerializeField]
        private float _packagesHDRP_version = 1.0f;
#endif
        [SerializeField]
        [Space]
        private Object _outlinesBuiltinPackage = null;
        [SerializeField]
        [Space]
        private Object _textmeshproPackage = null;

        /*
        [SerializeField][Space]
        private Object _examplesPackageInc = null;
        [SerializeField]
        private Object _examplesPackageBuiltin = null;
        [SerializeField]
        private Object _examplesPackageLWRP = null;
        [SerializeField]
        private Object _examplesPackageURP = null;
        [SerializeField]
        private Object _examplesPackageURP_2020_3_and_newer = null;
        [SerializeField]
        private Object _examplesPackageHDRP = null;
        [SerializeField]
        private Object _examplesPackageHDRP_2020_2_and_newer = null;
        [SerializeField]
        private Object _examplesPackageHDRP_2021_2_and_newer = null;
        [SerializeField]
        private Object _examplesPackageHDRP_2021_3_and_newer = null;
        [SerializeField]
        private Object _examplesPackageHDRP_2023_1_and_newer = null;

        [SerializeField][Space]
        private ExampleContainer[] _examples = null;
        */

        private static void LogAssetNotFoundError()
        {
            Debug.LogError("Could not find Install Wizard Configuration Asset, please try to import the package again.");
        }

        private static Configuration _instance = null;
        
        internal static Configuration TryGetInstance()
        {
            if(_instance == null)
            {
                string[] _guids = AssetDatabase.FindAssets("t:" + typeof(Configuration).Namespace + ".Configuration", null);
                if(_guids.Length > 0)
                {
                    _instance = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(_guids[0]), typeof(Configuration)) as Configuration;
                    if(_instance != null)
                        return _instance;
                    else
                    {
                        LogAssetNotFoundError();
                        return null;
                    }
                }
                else
                {
                    LogAssetNotFoundError();
                    return null;
                }
            }
            else
                return _instance;
        }

        internal static string TryGetPath()
        {
            if(isReady)
            {
                return AssetDatabase.GetAssetPath(_instance);
            }
            else
            {
                return string.Empty;
            }
        }

        internal static Texture2D TryGetTitleImage()
        {
            if(isReady)
            {
                return _instance._titleImage;
            }
            else
            {
                return null;
            }
        }

        /*
        internal static ExampleContainer[] TryGetExamples()
        {
            if(isReady)
            {
                return _instance._examples;
            }
            else
            {
                return null;
            }
        }
        */

        internal static bool TryGetShowInstallerOnReload()
        {
            if(isReady)
            {
                return _instance.showInstallerOnReload;
            }
            else
            {
                return false;
            }
        }
        internal static void TrySetShowInstallerOnReload(bool v)
        {
            if(isReady)
            {
                _instance.showInstallerOnReload = v;
                SaveInstance();
            }
        }

        internal static RenderPipeline TryGetRenderPipeline()
        {
            if(isReady)
            {
                return _instance._renderPipeline;
            }
            else
            {
                return RenderPipeline.Built_in_Legacy;
            }
        }
        internal static void TrySetRenderPipeline(RenderPipeline v)
        {
            if(isReady)
            {
                _instance._renderPipeline = v;

                SaveInstance();
            }
        }

        internal static void SaveInstance()
        {
            if(isReady)
            {
                EditorUtility.SetDirty(_instance);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        internal static void ImportPackages(RenderPipeline renderPipeline)
        {
            if(isReady)
            {
#pragma warning disable 0168
                string asspath, asspath2, newpath, spath;
#pragma warning restore 0168
                //bool moved = false;
                switch (renderPipeline)
                {
                    case RenderPipeline.Built_in_Legacy:
                        //foreach(var package in _instance._basePackagesBuiltin) AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(package), false);
                    break;
#if RENDERING_URP
                    case RenderPipeline.Universal:
                        //move folders
                        if (_instance._basePackagesURP[0] != null)
                        {
                            asspath = AssetDatabase.GetAssetPath(_instance._basePackagesURP[0]);
                            asspath = Path.GetDirectoryName(asspath);                     
                            newpath = Path.Combine(asspath, "crossSection (URP)");
                            spath = Path.Combine(newpath, "URP");
                            if(!AssetDatabase.IsValidFolder(spath)) AssetDatabase.CreateFolder(newpath, "URP");
                            newpath = Path.Combine(spath, "Editor");
                            asspath2 = Path.Combine(asspath, "crossSection (URP)", "Editor");
                            AssetDatabase.MoveAsset(asspath2, newpath);
                            newpath = Path.Combine(spath, "EdgeEffect");
                            asspath2 = Path.Combine(asspath, "crossSection (URP)", "EdgeEffect");
                            AssetDatabase.MoveAsset(asspath2, newpath);
                            newpath = Path.Combine(spath, "shaders");
                            asspath2 = Path.Combine(asspath, "crossSection (URP)", "shaders");
                            AssetDatabase.MoveAsset(asspath2, newpath);
                            //
                            AssetDatabase.Refresh();
                        }
                        else
                        {
                            Debug.LogWarning("Missing Configuration BasePackagesURP[0]");
                        }

                        foreach (var package in _instance._basePackagesURP) 
                        {
                            if (package == null)
                            {
                                Debug.LogWarning("Missing Configuration Package URP");
                                continue;
                            }
                            string packageName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(package));
                            if (packageName.Contains("URP_"))
                            {
                                string ver = packageName.Replace("URP_", "");
#if UNITY_2023_2_OR_NEWER
                            if (ver == "2023_2_and_newer") {_instance._packagesURP_version = 3.2f;} else {continue;}
#elif UNITY_2023_1_OR_NEWER
                            if (ver == "2023_1_and_newer") {_instance._packagesURP_version = 3.1f;} else {continue;}
#elif UNITY_2022_2_OR_NEWER
                            if (ver == "2022_2_and_newer") {_instance._packagesURP_version = 2.2f;} else {continue;}
#elif UNITY_2022_1_OR_NEWER
                            if (ver == "2022_1_and_newer") { _instance._packagesURP_version = 2.1f; } else { continue; }
#else
                            _instance._packagesURP_version = 1.2f;
#endif
                            }
                            AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(package), false); 
                        }

                        break;
#endif

#if RENDERING_HDRP
                    case RenderPipeline.High_Definition:
                        //move folders
                        if (_instance._basePackagesHDRP[0] != null)
                        {
                            asspath = AssetDatabase.GetAssetPath(_instance._basePackagesHDRP[0]);
                            asspath = Path.GetDirectoryName(asspath);
                            newpath = Path.Combine(asspath, "crossSection (HDRP)");
                            spath = Path.Combine(newpath, "HDRP");
                            if (!AssetDatabase.IsValidFolder(spath)) AssetDatabase.CreateFolder(newpath, "HDRP");
                            newpath = Path.Combine(spath, "Editor");
                            asspath2 = Path.Combine(asspath, "crossSection (HDRP)", "Editor");
                            AssetDatabase.MoveAsset(asspath2, newpath);
                            newpath = Path.Combine(spath, "EdgeEffect");
                            asspath2 = Path.Combine(asspath, "crossSection (URP)", "EdgeEffect");
                            AssetDatabase.MoveAsset(asspath2, newpath);
                            newpath = Path.Combine(spath, "shaders");
                            asspath2 = Path.Combine(asspath, "crossSection (HDRP)", "shaders");
                            AssetDatabase.MoveAsset(asspath2, newpath);
                            AssetDatabase.Refresh();
                        }
                        else
                        {
                            Debug.LogWarning("Missing Configuration BasePackagesHDRP[0]");
                        }
                        foreach (var package in _instance._basePackagesHDRP)
                        {
                            if (package == null)
                                                        {
                                Debug.LogWarning("Missing Configuration Package HDRP");
                                continue;
                            }
                            string packageName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(package));
                            if (packageName.Contains("HDRP_"))
                            {
                                string ver = packageName.Replace("HDRP_", "");
//#if UNITY_2023_2_OR_NEWER
//                            if (ver == "2023_2_and_newer") {_instance._packagesHDRP_version = 3.2f;} else {continue;}
#if UNITY_2023_1_OR_NEWER
                            if (ver == "2023_1_and_newer") {_instance._packagesHDRP_version = 3.1f;} else {continue;}
#elif UNITY_2022_2_OR_NEWER
                            if (ver == "2022_2_and_newer") {_instance._packagesHDRP_version = 2.2f;} else {continue;}
#elif UNITY_2022_1_OR_NEWER
                            if (ver == "2022_1_and_newer") { _instance._packagesHDRP_version = 2.1f; } else { continue; }
#else
                            _instance._packagesHDRP_version = 1.2f;
#endif
                            }
                            AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(package), false); 
                        }

                    break;
#endif
                                default:
                    //All cases should be handled
                    break;
                }
                TrySetShowInstallerOnReload(false);
            }
        }

        internal static void ImportOutlinesForBuiltinPackage()
        {
            if (isReady)
            {
                AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._outlinesBuiltinPackage), false);
            }
        }
        internal static void ImportTextMeshProPackage()
        {
            if (isReady)
            {
                AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._textmeshproPackage), false);
                Debug.Log(Path.GetFileName(AssetDatabase.GetAssetPath(_instance._textmeshproPackage)));
            }
        }

        /*
                internal static void ImportExamples(RenderPipeline renderPipeline)
                {
                    if(isReady)
                    {
                        AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageInc), false);
                        switch(renderPipeline)
                        {
                            case RenderPipeline.Built_in_Legacy:
                                AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageBuiltin), false);
                            break;
                            case RenderPipeline.Built_in_PostProcessingStack:
                                AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageLWRP), false);
                            break;
                            //case RenderPipeline.Lightweight:
                            //    AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageLWRP), false);
                            //break;
                            case RenderPipeline.Universal:
#if UNITY_2020_3_OR_NEWER
                                    AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageURP_2020_3_and_newer), false);
#else
                                    AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageURP), false);
#endif
                            break;
                            case RenderPipeline.High_Definition:
                                AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesSkyHDRP), false);
#if UNITY_2023_1_OR_NEWER
                                    AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageHDRP_2023_1_and_newer), false);
#elif UNITY_2021_3_OR_NEWER
                                    AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageHDRP_2021_3_and_newer), false);
#elif UNITY_2021_2_OR_NEWER
                                    AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageHDRP_2021_2_and_newer), false);
#elif UNITY_2020_2_OR_NEWER
                                    AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageHDRP_2020_2_and_newer), false);
#else
                                    AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(_instance._examplesPackageHDRP), false);
#endif
                            break;
                            default:
                            //All cases should be handled
                            break;
                        }
                    }
                }


        */

        internal static void OpenDocumentation()
        {
            Application.OpenURL(_instance._manualUrl);
            //AssetDatabase.OpenAsset(_instance._readMe);
        }

        internal static string TryGetVersion()
        {
            string v = "2.17";
            if (_instance._version)
            {
                string vText = _instance._version.text;
                string[] vLines = vText.Split("\n");
                v = vLines[0];
            }
            return v;
        }

        
#pragma warning restore CS0414
    }
}
#endif