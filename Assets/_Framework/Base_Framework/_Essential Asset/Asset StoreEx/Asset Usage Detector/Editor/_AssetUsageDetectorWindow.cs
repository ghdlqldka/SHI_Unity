// Asset Usage Detector - by Suleyman Yasir KULA (yasirkula@gmail.com)

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;
#if UNITY_2021_2_OR_NEWER
using PrefabStage = UnityEditor.SceneManagement.PrefabStage;
using PrefabStageUtility = UnityEditor.SceneManagement.PrefabStageUtility;
#elif UNITY_2018_3_OR_NEWER
using PrefabStage = UnityEditor.Experimental.SceneManagement.PrefabStage;
using PrefabStageUtility = UnityEditor.Experimental.SceneManagement.PrefabStageUtility;
#endif

namespace AssetUsageDetectorNamespace
{
    public class _AssetUsageDetectorWindow : AssetUsageDetectorWindow
    {
        private static string LOG_FORMAT = "<color=#33A4A4><b>[_AssetUsageDetectorWindow]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Awake()");

            LoadPrefs();
        }

        protected override void Update()
        {
            // base.Update();

            if (shouldRepositionSelf)
            {
                shouldRepositionSelf = false;
                position = windowTargetPosition;
            }
        }

        protected override void OnGUI()
        {
            // Make the window scrollable
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, Utilities.GL_EXPAND_WIDTH, Utilities.GL_EXPAND_HEIGHT);

            GUILayout.BeginVertical();

            if (currentPhase == Phase.Processing)
            {
                // If we are stuck at this phase, then we have encountered an exception
                GUILayout.Label(". . . Search in progress or something went wrong (check console) . . .");

                if (GUILayout.Button("RETURN", Utilities.GL_HEIGHT_30))
                {
                    ReturnToSetupPhase();
                    GUIUtility.ExitGUI();
                }
            }
            else if (currentPhase == Phase.Setup)
            {
                DrawObjectsToSearchSection();

                GUILayout.Space(10f);

                Utilities.DrawHeader("<b>SEARCH IN</b>");

                searchInAssetsFolder = WordWrappingToggleLeft("Project window (Assets folder)", searchInAssetsFolder);

                if (searchInAssetsFolder)
                {
                    BeginIndentedGUI();
                    searchInAssetsSubsetDrawer.Draw(searchInAssetsSubset);
                    excludedAssetsDrawer.Draw(excludedAssets);
                    EndIndentedGUI();
                }

                GUILayout.Space(5f);

                dontSearchInSourceAssets = WordWrappingToggleLeft("Don't search \"SEARCHED OBJECTS\" themselves for references", dontSearchInSourceAssets);
                searchUnusedMaterialProperties = WordWrappingToggleLeft("Search unused material properties (e.g. normal map of a material that no longer uses normal mapping)", searchUnusedMaterialProperties);

                Utilities.DrawSeparatorLine();

                if (searchInAllScenes && !EditorApplication.isPlaying)
                    GUI.enabled = false;

                searchInOpenScenes = WordWrappingToggleLeft("Currently open (loaded) scene(s)", searchInOpenScenes);

                if (!EditorApplication.isPlaying)
                {
                    searchInScenesInBuild = WordWrappingToggleLeft("Scenes in Build Settings", searchInScenesInBuild);

                    if (searchInScenesInBuild)
                    {
                        BeginIndentedGUI(false);
                        searchInScenesInBuildTickedOnly = EditorGUILayout.ToggleLeft("Ticked only", searchInScenesInBuildTickedOnly, Utilities.GL_WIDTH_100);
                        searchInScenesInBuildTickedOnly = !EditorGUILayout.ToggleLeft("All", !searchInScenesInBuildTickedOnly, Utilities.GL_WIDTH_100);
                        EndIndentedGUI(false);
                    }

                    GUI.enabled = true;

                    searchInAllScenes = WordWrappingToggleLeft("All scenes in the project", searchInAllScenes);
                }

                BeginIndentedGUI();
                excludedScenesDrawer.Draw(excludedScenes);
                EndIndentedGUI();

                EditorGUI.BeginDisabledGroup(!searchInOpenScenes && !searchInScenesInBuild && !searchInAllScenes);
                searchInSceneLightingSettings = WordWrappingToggleLeft("Scene Lighting Settings (WARNING: This may change the active scene during search)", searchInSceneLightingSettings);
                EditorGUI.EndDisabledGroup();

                Utilities.DrawSeparatorLine();

                searchInProjectSettings = WordWrappingToggleLeft("Project Settings (Player Settings, Graphics Settings etc.)", searchInProjectSettings);

                GUILayout.Space(10f);

                Utilities.DrawHeader("<b>SETTINGS</b>");

#if ASSET_USAGE_ADDRESSABLES
				EditorGUI.BeginDisabledGroup( addressablesSupport );
#endif
                lazySceneSearch = WordWrappingToggleLeft("Lazy scene search: scenes are searched in detail only when they are manually refreshed (faster search)", lazySceneSearch);
#if ASSET_USAGE_ADDRESSABLES
				EditorGUI.EndDisabledGroup();
				addressablesSupport = WordWrappingToggleLeft( "Addressables support (Experimental) (WARNING: 'Lazy scene search' will be disabled) (slower search)", addressablesSupport );
#endif
                calculateUnusedObjects = WordWrappingToggleLeft("Calculate unused objects", calculateUnusedObjects);
                hideDuplicateRows = WordWrappingToggleLeft("Hide duplicate rows in search results", hideDuplicateRows);
                hideRedundantPrefabReferencesInAssets = WordWrappingToggleLeft(hideRedundantPrefabReferencesInAssetsLabel, hideRedundantPrefabReferencesInAssets);
                hideRedundantPrefabReferencesInScenes = WordWrappingToggleLeft(hideRedundantPrefabReferencesInScenesLabel, hideRedundantPrefabReferencesInScenes);
                noAssetDatabaseChanges = WordWrappingToggleLeft("I haven't modified any assets/scenes since the last search (faster search)", noAssetDatabaseChanges);
                showDetailedProgressBar = WordWrappingToggleLeft("Update search progress bar more often (cancelable search) (slower search)", showDetailedProgressBar);

                GUILayout.Space(10f);

                // Don't let the user press the GO button without any valid search location
                if (!searchInAllScenes && !searchInOpenScenes && !searchInScenesInBuild && !searchInAssetsFolder && !searchInProjectSettings)
                    GUI.enabled = false;

                if (GUILayout.Button("GO!", Utilities.GL_HEIGHT_30))
                {
                    Debug.LogFormat(LOG_FORMAT, "===================================");
                    InitiateSearch();
                    GUIUtility.ExitGUI();
                }

                GUILayout.Space(5f);
            }
            else if (currentPhase == Phase.Complete)
            {
                // Draw the results of the search
                GUI.enabled = false;

                DrawObjectsToSearchSection();

                if (drawObjectsToSearchSection)
                    GUILayout.Space(10f);

                GUI.enabled = true;

                if (GUILayout.Button("Reset Search", Utilities.GL_HEIGHT_30))
                {
                    ReturnToSetupPhase();
                    GUIUtility.ExitGUI();
                }

                if (searchResult == null)
                {
                    EditorGUILayout.HelpBox("ERROR: searchResult is null", MessageType.Error);
                    return;
                }
                else if (!searchResult.SearchCompletedSuccessfully)
                    EditorGUILayout.HelpBox("ERROR: search was interrupted, check the logs for more info", MessageType.Error);

                if (searchResult.NumberOfGroups == 0)
                {
                    GUILayout.Space(10f);
                    GUILayout.Box("No references found...", Utilities.BoxGUIStyle, Utilities.GL_EXPAND_WIDTH);
                }
                else
                {
                    noAssetDatabaseChanges = WordWrappingToggleLeft("I haven't modified any assets/scenes since the last search (faster Refresh)", noAssetDatabaseChanges);

                    EditorGUILayout.Space();

                    scrollPosition.y = searchResult.DrawOnGUI(this, scrollPosition.y, noAssetDatabaseChanges);
                }
            }

            if (Event.current.type == EventType.MouseLeaveWindow)
            {
                SearchResultTooltip.Hide();

                if (searchResult != null)
                    searchResult.CancelDelayedTreeViewTooltip();
            }

            GUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        protected override void SavePrefs()
        {
            // base.SavePrefs();

            EditorPrefs.SetInt("_" + PREFS_SEARCH_SCENES, (int)GetSceneSearchMode(false));
            EditorPrefs.SetBool("_" + PREFS_SEARCH_SCENE_LIGHTING_SETTINGS, searchInSceneLightingSettings);
            EditorPrefs.SetBool("_" + PREFS_SEARCH_ASSETS, searchInAssetsFolder);
            EditorPrefs.SetBool("_" + PREFS_DONT_SEARCH_SOURCE_ASSETS, dontSearchInSourceAssets);
            EditorPrefs.SetBool("_" + PREFS_SEARCH_PROJECT_SETTINGS, searchInProjectSettings);
            EditorPrefs.SetInt("_" + PREFS_SEARCH_DEPTH_LIMIT, searchDepthLimit);
            EditorPrefs.SetInt("_" + PREFS_SEARCH_FIELDS, (int)fieldModifiers);
            EditorPrefs.SetInt("_" + PREFS_SEARCH_PROPERTIES, (int)propertyModifiers);
            EditorPrefs.SetBool("_" + PREFS_SEARCH_NON_SERIALIZABLES, searchNonSerializableVariables);
            EditorPrefs.SetBool("_" + PREFS_SEARCH_UNUSED_MATERIAL_PROPERTIES, searchUnusedMaterialProperties);
            EditorPrefs.SetBool("_" + PREFS_LAZY_SCENE_SEARCH, lazySceneSearch);
#if ASSET_USAGE_ADDRESSABLES
			EditorPrefs.SetBool("_" + PREFS_ADDRESSABLES_SUPPORT, addressablesSupport );
#endif
            EditorPrefs.SetBool("_" + PREFS_CALCULATE_UNUSED_OBJECTS, calculateUnusedObjects);
            EditorPrefs.SetBool("_" + PREFS_HIDE_DUPLICATE_ROWS, hideDuplicateRows);
            EditorPrefs.SetBool("_" + PREFS_HIDE_REDUNDANT_PREFAB_REFERENCES_IN_ASSETS, hideRedundantPrefabReferencesInAssets);
            EditorPrefs.SetBool("_" + PREFS_HIDE_REDUNDANT_PREFAB_REFERENCES_IN_SCENES, hideRedundantPrefabReferencesInScenes);
            EditorPrefs.SetBool("_" + PREFS_SHOW_PROGRESS, showDetailedProgressBar);
        }

        protected override void LoadPrefs()
        {
            // base.LoadPrefs();

            ParseSceneSearchMode((SceneSearchMode)EditorPrefs.GetInt("_" + PREFS_SEARCH_SCENES, (int)(SceneSearchMode.OpenScenes | SceneSearchMode.ScenesInBuildSettingsTickedOnly | SceneSearchMode.AllScenes)));
            searchInSceneLightingSettings = EditorPrefs.GetBool("_" + PREFS_SEARCH_SCENE_LIGHTING_SETTINGS, true);
            searchInAssetsFolder = EditorPrefs.GetBool("_" + PREFS_SEARCH_ASSETS, true);
            dontSearchInSourceAssets = EditorPrefs.GetBool("_" + PREFS_DONT_SEARCH_SOURCE_ASSETS, true);
            searchInProjectSettings = EditorPrefs.GetBool("_" + PREFS_SEARCH_PROJECT_SETTINGS, true);
            searchDepthLimit = EditorPrefs.GetInt("_" + PREFS_SEARCH_DEPTH_LIMIT, 4);
            fieldModifiers = (BindingFlags)EditorPrefs.GetInt("_" + PREFS_SEARCH_FIELDS, (int)(BindingFlags.Public | BindingFlags.NonPublic));
            propertyModifiers = (BindingFlags)EditorPrefs.GetInt("_" + PREFS_SEARCH_PROPERTIES, (int)(BindingFlags.Public | BindingFlags.NonPublic));
            searchNonSerializableVariables = EditorPrefs.GetBool("_" + PREFS_SEARCH_NON_SERIALIZABLES, true);
            searchUnusedMaterialProperties = EditorPrefs.GetBool("_" + PREFS_SEARCH_UNUSED_MATERIAL_PROPERTIES, true);
            lazySceneSearch = EditorPrefs.GetBool("_" + PREFS_LAZY_SCENE_SEARCH, true);
#if ASSET_USAGE_ADDRESSABLES
			addressablesSupport = EditorPrefs.GetBool("_" + PREFS_ADDRESSABLES_SUPPORT, false );
#endif
            calculateUnusedObjects = EditorPrefs.GetBool("_" + PREFS_CALCULATE_UNUSED_OBJECTS, false);
            hideDuplicateRows = EditorPrefs.GetBool("_" + PREFS_HIDE_DUPLICATE_ROWS, true);
            hideRedundantPrefabReferencesInAssets = EditorPrefs.GetBool("_" + PREFS_HIDE_REDUNDANT_PREFAB_REFERENCES_IN_ASSETS, hideRedundantPrefabReferencesInAssets);
            hideRedundantPrefabReferencesInScenes = EditorPrefs.GetBool("_" + PREFS_HIDE_REDUNDANT_PREFAB_REFERENCES_IN_SCENES, hideRedundantPrefabReferencesInScenes);
            showDetailedProgressBar = EditorPrefs.GetBool("_" + PREFS_SHOW_PROGRESS, true);
        }

        protected static _AssetUsageDetectorWindow GetWindowEx(WindowFilter filter)
        {
            _AssetUsageDetectorWindow[] windows = Resources.FindObjectsOfTypeAll<_AssetUsageDetectorWindow>();
            _AssetUsageDetectorWindow window = System.Array.Find(windows, (w) => w && !w.IsLocked);
            if (!window)
                window = System.Array.Find(windows, (w) => w);

            if (window && (filter == WindowFilter.AlwaysReturnActive || (!window.IsLocked && filter == WindowFilter.ReturnActiveIfNotLocked)))
            {
                window.Show();
                window.Focus();

                return window;
            }

            Rect? windowTargetPosition = null;
            if (window)
            {
                Rect position = window.position;
                position.position += new Vector2(50f, 50f);
                windowTargetPosition = position;
            }

            window = CreateInstance<_AssetUsageDetectorWindow>();
            window.titleContent = windowTitle;
            window.minSize = windowMinSize;

            if (windowTargetPosition.HasValue)
            {
                window.shouldRepositionSelf = true;
                window.windowTargetPosition = windowTargetPosition.Value;
            }

            window.Show(true);
            window.Focus();

            return window;
        }

        // [MenuItem( "Window/Asset Usage Detector/Active Window" )]
        [MenuItem("_Asset Usage Detector/Active Window")]
        private static void OpenActiveWindow()
        {
            Debug.LogFormat(LOG_FORMAT, "OpenActiveWindow()");

            windowTitle = new GUIContent("_Asset Usage Detector");
            GetWindowEx(WindowFilter.AlwaysReturnActive);
        }

        // [MenuItem( "Window/Asset Usage Detector/New Window" )]
        [MenuItem("_Asset Usage Detector/New Window")]
        private static void OpenNewWindow()
        {
            Debug.LogFormat(LOG_FORMAT, "OpenNewWindow()");

            windowTitle = new GUIContent("_Asset Usage Detector");
            GetWindowEx(WindowFilter.AlwaysReturnNew);
        }

        // Quickly initiate search for the selected assets
        // [MenuItem( "GameObject/Search for References/This Object Only", priority = 49 )]
        [MenuItem("GameObject/_Asset Usage Detector/Search for References/This Object Only", priority = 49)]
        [MenuItem("Assets/_Asset Usage Detector/Search for References", priority = 1000)]
        private static void SearchSelectedAssetReferences(MenuCommand command)
        {
            // This happens when this button is clicked via hierarchy's right click context menu
            // and is called once for each object in the selection. We don't want that, we want
            // the function to be called only once
            if (command.context)
            {
                EditorApplication.update -= CallSearchSelectedAssetReferencesOnce;
                EditorApplication.update += CallSearchSelectedAssetReferencesOnce;
            }
            else
                ShowAndSearch(Selection.objects);
        }

        [MenuItem("GameObject/_Asset Usage Detector/Search for References/Include Children", priority = 49)]
        private static void SearchSelectedAssetReferencesWithChildren(MenuCommand command)
        {
            if (command.context)
            {
                EditorApplication.update -= CallSearchSelectedAssetReferencesWithChildrenOnce;
                EditorApplication.update += CallSearchSelectedAssetReferencesWithChildrenOnce;
            }
            else
                ShowAndSearch(Selection.objects, true);
        }

        // Show the menu item only if there is a selection in the Editor
        [MenuItem("GameObject/_Asset Usage Detector/Search for References/This Object Only", validate = true)]
        [MenuItem("GameObject/_Asset Usage Detector/Search for References/Include Children", validate = true)]
        [MenuItem("Assets/_Asset Usage Detector/Search for References", validate = true)]
        private static bool SearchSelectedAssetReferencesValidate(MenuCommand command)
        {
            return Selection.objects.Length > 0;
        }
    }
}