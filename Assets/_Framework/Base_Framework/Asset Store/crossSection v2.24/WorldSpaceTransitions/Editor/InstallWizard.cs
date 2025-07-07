//////////////////////////////////////////////////////
// CrossSection Install Wizard Configuration    	//
//					                                //
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

#if UNITY_EDITOR
using Configuration = WorldSpaceTransitions.CrossSection.Editor.InstallWizard.Configuration;
namespace WorldSpaceTransitions.CrossSection.Editor.InstallWizard
{
    public sealed class InstallWizard : EditorWindow
    {
        #pragma warning disable CS0414
        private static readonly string _version = "2.18";
        #pragma warning restore CS0414
        private static readonly Vector2Int _referenceResolution = new Vector2Int(2560, 1440);
        private static float _sizeScale;
        private static int _scaledWidth;
        private static int _scaledHeight;
        private static Vector2 _windowScrollPos;

        private static readonly int _rawWidth = 360;
        private static readonly int _rawHeight = 640;
        private static readonly string _title = "CrossSection Install Wizard";

        private GUIStyle _flowTextStyle { get { return new GUIStyle(EditorStyles.label) { wordWrap = true }; } }
        private static readonly int _loadTimeInFrames = 72;
        private static int _waitFramesTillReload = _loadTimeInFrames;

        private static InstallWizard _window;
        private static RenderPipeline _targetRenderPipeline = RenderPipeline.Built_in_Legacy;
        private static bool _showInstallerOnReload = true;

        [MenuItem("Window/CrossSection/Install Wizard")]
        private static void ShowWindow()
        {
            if(Screen.currentResolution.height > Screen.currentResolution.width)
                _sizeScale = (float) Screen.currentResolution.width / (float)_referenceResolution.x;
            else
                _sizeScale = (float) Screen.currentResolution.height / (float)_referenceResolution.y;

            _scaledWidth = (int)((float)_rawWidth * _sizeScale);
            _scaledHeight = (int)((float)_rawHeight * _sizeScale);
            _window = (InstallWizard) EditorWindow.GetWindow<InstallWizard>(true, _title, true);

            _window.minSize = new Vector2(_scaledWidth, _scaledHeight);
            _window.maxSize = new Vector2(_scaledWidth * 2, _scaledHeight * 2);
            _window.Show();
        }

        [InitializeOnLoadMethod]
        private static void ShowInstallerOnReload()
        {
            QueryReload();
        }

        private static void QueryReload()
        {
            _waitFramesTillReload = _loadTimeInFrames;
            EditorApplication.update += Reload;
        }

        private static void Reload()
        {
            if (_waitFramesTillReload > 0)
            {
                --_waitFramesTillReload;
            }
            else
            {
                EditorApplication.update -= Reload;
                if(Configuration.isReady && Configuration.TryGetShowInstallerOnReload())
                    ShowWindow();
            }
        }

        private void OnGUI()
        {
            if(Configuration.isReady)
            {
                _windowScrollPos = EditorGUILayout.BeginScrollView(_windowScrollPos);
                Texture2D titleImage = Configuration.TryGetTitleImage();
                if(titleImage)
                {
                    float titleScaledWidth = EditorGUIUtility.currentViewWidth - EditorGUIUtility.standardVerticalSpacing * 4;
                    float titleScaledHeight = titleScaledWidth * ((float)titleImage.height / (float)titleImage.width);
                    Rect titleRect = EditorGUILayout.GetControlRect();
                    titleRect.width = titleScaledWidth;
                    titleRect.height = titleScaledHeight;
                    GUI.DrawTexture(titleRect, titleImage, ScaleMode.ScaleToFit);
                    GUILayout.Label("", GUILayout.Height(titleScaledHeight - 20));
                    Divider();
                }
                var versionStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
                EditorGUILayout.LabelField("version "+ Configuration.TryGetVersion(), versionStyle);
                EditorGUILayout.LabelField("1. Select your Render Pipeline", UnityEditor.EditorStyles.boldLabel);
                _targetRenderPipeline = Configuration.TryGetRenderPipeline();

                EditorGUI.BeginChangeCheck();
                _targetRenderPipeline = (RenderPipeline) EditorGUILayout.EnumPopup("Render Pipeline", _targetRenderPipeline);
                if(EditorGUI.EndChangeCheck())
                    Configuration.TrySetRenderPipeline(_targetRenderPipeline);
                int step = 2;
                if (_targetRenderPipeline != RenderPipeline.Built_in_Legacy)
                {
                    VerticalSpace();
                    Divider();
                    VerticalSpace();
                    EditorGUILayout.LabelField("2. Import Package", UnityEditor.EditorStyles.boldLabel);
                    if (GUILayout.Button("Import / Update Package"))
                    {
                        EditorUtility.DisplayProgressBar("CrossSection Install Wizard", "Importing Package", 0.5f);
                        Configuration.ImportPackages(_targetRenderPipeline);
                        EditorUtility.ClearProgressBar();
                    }
                    step++;
                }
                switch(_targetRenderPipeline)
                {
                    case RenderPipeline.Built_in_Legacy:
                    EditorGUILayout.LabelField(@"Go to example scenes: 
Assets\WorldSpaceTransitions\crossSection (Built In)\scenes", _flowTextStyle);
                    break;
#if RENDERING_URP 
                    case RenderPipeline.Universal:
                    EditorGUILayout.LabelField(@"Go to example scenes: 
Assets\WorldSpaceTransitions\crossSection (URP)\scenes", _flowTextStyle);
                    break;
#endif
#if RENDERING_HDRP
                    case RenderPipeline.High_Definition:
                    EditorGUILayout.LabelField(@"Go to example scenes: 
Assets\WorldSpaceTransitions\crossSection (HDRP)\scenes", _flowTextStyle);
                    break;
#endif
                }
                VerticalSpace();
                Divider();
                VerticalSpace();
                {
                    /*
                    EditorGUILayout.LabelField("3. Import Examples (optional)", UnityEditor.EditorStyles.boldLabel);
                    switch(_targetRenderPipeline)
                    {
                        case RenderPipeline.Built_in_Legacy:
                        //case RenderPipeline.Built_in_PostProcessingStack:
                        EditorGUILayout.LabelField("Example Scenes are based on Linear Color Space. Make sure to change from Gamma to Linear Color Space.", _flowTextStyle);
                        break;
                        //case RenderPipeline.Lightweight:
                        //break;
                        case RenderPipeline.Universal:
                        EditorGUILayout.LabelField("Example Scenes xxxx.", _flowTextStyle);
                        break;
                        case RenderPipeline.High_Definition:
                        break;
                    }
                    EditorGUILayout.LabelField("Old Input Manager is used for the examples.", _flowTextStyle);
                    /*
                    if(GUILayout.Button("Import Examples"))
                    {
                        EditorUtility.DisplayProgressBar("CrossSection Install Wizard", "Importing Examples", 0.5f);
                        Configuration.ImportExamples(_targetRenderPipeline);
                        EditorUtility.ClearProgressBar();
                    }
                    VerticalSpace();
                    Divider();
                    ExampleContainer[] examples = Configuration.TryGetExamples();
                    if(examples.Length > 0 && examples[0].scene != null)
                    {
                        VerticalSpace();
                        EditorGUILayout.LabelField("Example Scenes:");
                        EditorGUILayout.BeginHorizontal();
                        for(int i = 0; i < examples.Length; i++)
                        {
                            if(examples[i].scene != null)
                                examples[i].DrawEditorButton();
                        }
                        EditorGUILayout.EndHorizontal();
                        VerticalSpace();
                        Divider();
                    }
                    */

                    EditorGUILayout.LabelField(step.ToString()+". Import optional assets", UnityEditor.EditorStyles.boldLabel);
                    GUI.skin.button.wordWrap = true;
                    step++;
#if POSTPROCESSING
                    if (GUILayout.Button("Import / Update Selective Outline Effect for the built-in render pipeline"))
                    {
                        EditorUtility.DisplayProgressBar("CrossSection Install Wizard", "Importing Selective Outline Effect", 0.5f);
                        Configuration.ImportOutlinesForBuiltinPackage();
                        EditorUtility.ClearProgressBar();
                    }

#else
                    if(_targetRenderPipeline==RenderPipeline.Built_in_Legacy)
                    {
                    EditorGUILayout.LabelField("CrossSection Selective Outline Effect package can get available after the instalation of postprocessing package", _flowTextStyle);
                    }
#endif
//#if TEXTMESHPRO
                        if (GUILayout.Button("Import / Update TextMeshPro shaders for the CrossSection"))
                    {
                        EditorUtility.DisplayProgressBar("CrossSection Install Wizard", "Importing TextMeshPro shaders", 0.5f);
                        Configuration.ImportTextMeshProPackage();
                        EditorUtility.ClearProgressBar();
                    }
//#else
//                    EditorGUILayout.LabelField("CrossSection TextMeshPro shaders can get available after the instalation of TextMeshPro package", _flowTextStyle);
//#endif

                }
                VerticalSpace();
                EditorGUILayout.LabelField(step.ToString() + ". View documentation (Recommended)", UnityEditor.EditorStyles.boldLabel);
                if(GUILayout.Button("View documentation"))
                {
                    Configuration.OpenDocumentation();
                }

                VerticalSpace();
                Divider();
                VerticalSpace();

                _showInstallerOnReload = Configuration.TryGetShowInstallerOnReload();
                EditorGUI.BeginChangeCheck();
                _showInstallerOnReload = EditorGUILayout.Toggle("Show Installer On Reload", _showInstallerOnReload);
                if(EditorGUI.EndChangeCheck())
                    Configuration.TrySetShowInstallerOnReload(_showInstallerOnReload);

                EditorGUILayout.EndScrollView();
                GUI.FocusControl(null);
            }
            else
            {
                Repaint();
            }
        }
        private void OnProjectChange()
        {

        }

        private static void VerticalSpace()
        {
            GUILayoutUtility.GetRect(1f, EditorGUIUtility.standardVerticalSpacing);
        }

        private static void Divider()
        {
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(2) });
        }
    }
}
#endif