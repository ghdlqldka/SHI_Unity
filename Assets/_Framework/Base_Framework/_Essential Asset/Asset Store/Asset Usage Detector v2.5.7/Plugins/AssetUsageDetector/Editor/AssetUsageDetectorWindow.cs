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
	public enum Phase { Setup, Processing, Complete };

	public class AssetUsageDetectorWindow : EditorWindow, IHasCustomMenu
	{
		public enum WindowFilter { AlwaysReturnActive, ReturnActiveIfNotLocked, AlwaysReturnNew };
		
		protected const string PREFS_SEARCH_SCENES = "AUD_SceneSearch";
		protected const string PREFS_SEARCH_SCENE_LIGHTING_SETTINGS = "AUD_LightingSettingsSearch";
		protected const string PREFS_SEARCH_ASSETS = "AUD_AssetsSearch";
		protected const string PREFS_SEARCH_PROJECT_SETTINGS = "AUD_ProjectSettingsSearch";
		protected const string PREFS_DONT_SEARCH_SOURCE_ASSETS = "AUD_AssetsExcludeSrc";
		protected const string PREFS_SEARCH_DEPTH_LIMIT = "AUD_Depth";
		protected const string PREFS_SEARCH_FIELDS = "AUD_Fields";
		protected const string PREFS_SEARCH_PROPERTIES = "AUD_Properties";
		protected const string PREFS_SEARCH_NON_SERIALIZABLES = "AUD_NonSerializables";
		protected const string PREFS_SEARCH_UNUSED_MATERIAL_PROPERTIES = "AUD_SearchUnusedMaterialProps";
		protected const string PREFS_LAZY_SCENE_SEARCH = "AUD_LazySceneSearch";
#if ASSET_USAGE_ADDRESSABLES
        protected const string PREFS_ADDRESSABLES_SUPPORT = "AUD_AddressablesSupport";
#endif
		protected const string PREFS_CALCULATE_UNUSED_OBJECTS = "AUD_FindUnusedObjs";
		protected const string PREFS_HIDE_DUPLICATE_ROWS = "AUD_HideDuplicates";
		protected const string PREFS_HIDE_REDUNDANT_PREFAB_REFERENCES_IN_ASSETS = "AUD_HideRedundantPRefsInAssets";
		protected const string PREFS_HIDE_REDUNDANT_PREFAB_REFERENCES_IN_SCENES = "AUD_HideRedundantPRefsInScenes";
		protected const string PREFS_SHOW_PROGRESS = "AUD_Progress";

        protected static /*readonly*/ GUIContent windowTitle = new GUIContent( "Asset Usage Detector" );
        protected static readonly Vector2 windowMinSize = new Vector2( 325f, 220f );

        protected static readonly GUILayoutOption GL_WIDTH_12 = GUILayout.Width( 12f );

        protected static readonly GUIContent sharedGUIContent = new GUIContent();

        protected readonly GUIContent hideRedundantPrefabReferencesInAssetsLabel = new GUIContent( "Hide redundant prefab references in Assets", "Hides redundant/non-overridden references in prefab variants and nested prefabs. " +
			"This will help focus on only the references that actually matter. For example:\n\n" +
			"- Material CloudMat is assigned to prefab Cloud and its variant CloudBig. Since changing Cloud's material will also affect CloudBig, search results won't show CloudBig's reference" );
        protected readonly GUIContent hideRedundantPrefabReferencesInScenesLabel = new GUIContent( "Hide redundant prefab references in Scenes", "Hides redundant/non-overridden references in prefab instances. " +
			"This will help focus on only the references that actually matter. For example:\n\n" +
			"- Prefab Healthbar is nested inside prefab Player. An instance of Player exists in the current scene and its Healthbar isn't modified. Since modifying Healthbar in Player prefab will also affect " +
			"the instance in the scene, search results won't show the instance in the scene while searching for Healthbar's references" );

        protected GUIStyle lockButtonStyle;

        protected readonly AssetUsageDetector core = new AssetUsageDetector();
        protected SearchResult searchResult; // Overall search results

        // This isn't readonly so that it can be serialized
        protected List<ObjectToSearch> objectsToSearch = new List<ObjectToSearch>() { new ObjectToSearch( null ) };

		[SerializeField] // Since titleContent persists between Editor sessions, so should the IsLocked property because otherwise, "[L]" in title becomes confusing when the EditorWindow isn't actually locked
		private bool m_isLocked;
        protected bool IsLocked
		{
			get { return m_isLocked; }
			set
			{
				if( m_isLocked != value )
				{
					m_isLocked = value;
					titleContent = value ? new GUIContent( "[L] " + windowTitle.text, EditorGUIUtility.IconContent( "InspectorLock" ).image ) : windowTitle;
				}
			}
		}

        protected Phase currentPhase = Phase.Setup;

		protected bool searchInOpenScenes = true; // Scenes currently open in Hierarchy view
		protected bool searchInScenesInBuild = true; // Scenes in build
		protected bool searchInScenesInBuildTickedOnly = true; // Scenes in build (ticked only or not)
		protected bool searchInAllScenes = true; // All scenes (including scenes that are not in build)
		protected bool searchInSceneLightingSettings = true; // Window-Rendering-Lighting settings
		protected bool searchInAssetsFolder = true; // Assets in Project window
		protected bool dontSearchInSourceAssets = true; // objectsToSearch won't be searched for internal references
		protected bool searchInProjectSettings = true; // Player Settings, Graphics Settings etc.

		protected List<Object> searchInAssetsSubset = new List<Object>() { null }; // If not empty, only these assets are searched for references
		protected List<Object> excludedAssets = new List<Object>() { null }; // These assets won't be searched for references
		protected List<Object> excludedScenes = new List<Object>() { null }; // These scenes won't be searched for references

        protected int searchDepthLimit = 4; // Depth limit for recursively searching variables of objects

        protected bool lazySceneSearch = true;
#if ASSET_USAGE_ADDRESSABLES
        protected bool addressablesSupport = false;
#endif
		protected bool searchNonSerializableVariables = true;
		protected bool searchUnusedMaterialProperties = true;
		protected bool calculateUnusedObjects = false;
		protected bool hideDuplicateRows = true;
		protected bool hideRedundantPrefabReferencesInAssets = false;
		protected bool hideRedundantPrefabReferencesInScenes = false;
		protected bool noAssetDatabaseChanges = false;
		protected bool showDetailedProgressBar = true;

        protected BindingFlags fieldModifiers, propertyModifiers;

        protected SearchRefactoring searchRefactoring = null; // Its value can be assigned via ShowAndSearch

		protected readonly ObjectToSearchListDrawer objectsToSearchDrawer = new ObjectToSearchListDrawer();
		protected readonly ObjectListDrawer searchInAssetsSubsetDrawer = new ObjectListDrawer( "Search following asset(s) only:", false );
		protected readonly ObjectListDrawer excludedAssetsDrawer = new ObjectListDrawer( "Don't search following asset(s):", false );
		protected readonly ObjectListDrawer excludedScenesDrawer = new ObjectListDrawer( "Don't search in following scene(s):", false );

        protected bool drawObjectsToSearchSection = true;

        protected Vector2 scrollPosition = Vector2.zero;

        protected bool shouldRepositionSelf;
        protected Rect windowTargetPosition;

		void IHasCustomMenu.AddItemsToMenu( GenericMenu contextMenu )
		{
			contextMenu.AddItem( new GUIContent( "Lock" ), IsLocked, () => IsLocked = !IsLocked );
			contextMenu.AddSeparator( "" );
			contextMenu.AddItem( new GUIContent( "Settings" ), false, () => SettingsService.OpenProjectSettings( "Project/yasirkula/Asset Usage Detector" ) );

			if( currentPhase == Phase.Setup )
			{
				contextMenu.AddSeparator( "" );
				contextMenu.AddItem( new GUIContent( "Refresh Sub-Assets of Searched Objects" ), false, () =>
				{
					for( int i = objectsToSearch.Count - 1; i >= 0; i-- )
						objectsToSearch[i].RefreshSubAssets();
				} );
			}
			else if( currentPhase == Phase.Complete )
			{
				if( searchResult != null && searchResult.NumberOfGroups > 0 )
				{
					contextMenu.AddSeparator( "" );
					contextMenu.AddItem( new GUIContent( "Collapse All" ), false, searchResult.CollapseAllSearchResultGroups );
				}
			}
		}

		// Shows lock button at the top-right corner
		// Credit: http://leahayes.co.uk/2013/04/30/adding-the-little-padlock-button-to-your-editorwindow.html
		private void ShowButton( Rect position )
		{
			if( lockButtonStyle == null )
				lockButtonStyle = "IN LockButton";

			IsLocked = GUI.Toggle( position, IsLocked, GUIContent.none, lockButtonStyle );
		}

		private static AssetUsageDetectorWindow GetWindow( WindowFilter filter )
		{
			AssetUsageDetectorWindow[] windows = Resources.FindObjectsOfTypeAll<AssetUsageDetectorWindow>();
			AssetUsageDetectorWindow window = System.Array.Find( windows, ( w ) => w && !w.IsLocked );
			if( !window )
				window = System.Array.Find( windows, ( w ) => w );

			if( window && ( filter == WindowFilter.AlwaysReturnActive || ( !window.IsLocked && filter == WindowFilter.ReturnActiveIfNotLocked ) ) )
			{
				window.Show();
				window.Focus();

				return window;
			}

			Rect? windowTargetPosition = null;
			if( window )
			{
				Rect position = window.position;
				position.position += new Vector2( 50f, 50f );
				windowTargetPosition = position;
			}

			window = CreateInstance<AssetUsageDetectorWindow>();
			window.titleContent = windowTitle;
			window.minSize = windowMinSize;

			if( windowTargetPosition.HasValue )
			{
				window.shouldRepositionSelf = true;
				window.windowTargetPosition = windowTargetPosition.Value;
			}

			window.Show( true );
			window.Focus();

			return window;
		}

		[MenuItem( "Window/Asset Usage Detector/Active Window" )]
		private static void OpenActiveWindow()
		{
			GetWindow( WindowFilter.AlwaysReturnActive );
		}

		[MenuItem( "Window/Asset Usage Detector/New Window" )]
		private static void OpenNewWindow()
		{
			GetWindow( WindowFilter.AlwaysReturnNew );
		}

		// Quickly initiate search for the selected assets
		[MenuItem( "GameObject/Search for References/This Object Only", priority = 49 )]
		[MenuItem( "Assets/Search for References", priority = 1000 )]
		private static void SearchSelectedAssetReferences( MenuCommand command )
		{
			// This happens when this button is clicked via hierarchy's right click context menu
			// and is called once for each object in the selection. We don't want that, we want
			// the function to be called only once
			if( command.context )
			{
				EditorApplication.update -= CallSearchSelectedAssetReferencesOnce;
				EditorApplication.update += CallSearchSelectedAssetReferencesOnce;
			}
			else
				ShowAndSearch( Selection.objects );
		}

		[MenuItem( "GameObject/Search for References/Include Children", priority = 49 )]
		private static void SearchSelectedAssetReferencesWithChildren( MenuCommand command )
		{
			if( command.context )
			{
				EditorApplication.update -= CallSearchSelectedAssetReferencesWithChildrenOnce;
				EditorApplication.update += CallSearchSelectedAssetReferencesWithChildrenOnce;
			}
			else
				ShowAndSearch( Selection.objects, true );
		}

		// Show the menu item only if there is a selection in the Editor
		[MenuItem( "GameObject/Search for References/This Object Only", validate = true )]
		[MenuItem( "GameObject/Search for References/Include Children", validate = true )]
		[MenuItem( "Assets/Search for References", validate = true )]
		private static bool SearchSelectedAssetReferencesValidate( MenuCommand command )
		{
			return Selection.objects.Length > 0;
		}

		// Quickly show the AssetUsageDetector window and initiate a search
		public static void ShowAndSearch( IEnumerable<Object> searchObjects, bool? shouldSearchChildren = null )
		{
			GetWindow( WindowFilter.ReturnActiveIfNotLocked ).ShowAndSearchInternal( searchObjects, null, shouldSearchChildren );
		}

		// Quickly show the AssetUsageDetector window and initiate a search
		public static void ShowAndSearch( AssetUsageDetector.Parameters searchParameters, bool? shouldSearchChildren = null )
		{
			if( searchParameters == null )
			{
				Debug.LogError( "searchParameters can't be null!" );
				return;
			}

			GetWindow( WindowFilter.ReturnActiveIfNotLocked ).ShowAndSearchInternal( searchParameters.objectsToSearch, searchParameters, shouldSearchChildren );
		}

        protected static void CallSearchSelectedAssetReferencesOnce()
		{
			EditorApplication.update -= CallSearchSelectedAssetReferencesOnce;
			SearchSelectedAssetReferences( new MenuCommand( null ) );
		}

        protected static void CallSearchSelectedAssetReferencesWithChildrenOnce()
		{
			EditorApplication.update -= CallSearchSelectedAssetReferencesWithChildrenOnce;
			SearchSelectedAssetReferencesWithChildren( new MenuCommand( null ) );
		}

		private void ShowAndSearchInternal( IEnumerable<Object> searchObjects, AssetUsageDetector.Parameters searchParameters, bool? shouldSearchChildren )
		{
			if( !ReturnToSetupPhase() )
			{
				Debug.LogError( "Need to reset the previous search first!" );
				return;
			}

			objectsToSearch.Clear();
			if( searchObjects != null )
			{
				foreach( Object obj in searchObjects )
					objectsToSearch.Add( new ObjectToSearch( obj, shouldSearchChildren ) );
			}

			if( searchParameters != null )
			{
				ParseSceneSearchMode( searchParameters.searchInScenes );
				searchInSceneLightingSettings = searchParameters.searchInSceneLightingSettings;
				searchInAssetsFolder = searchParameters.searchInAssetsFolder;
				dontSearchInSourceAssets = searchParameters.dontSearchInSourceAssets;
				searchInProjectSettings = searchParameters.searchInProjectSettings;
				searchDepthLimit = searchParameters.searchDepthLimit;
				fieldModifiers = searchParameters.fieldModifiers;
				propertyModifiers = searchParameters.propertyModifiers;
				searchNonSerializableVariables = searchParameters.searchNonSerializableVariables;
				searchUnusedMaterialProperties = searchParameters.searchUnusedMaterialProperties;
				searchRefactoring = searchParameters.searchRefactoring;
				lazySceneSearch = searchParameters.lazySceneSearch;
#if ASSET_USAGE_ADDRESSABLES
				addressablesSupport = searchParameters.addressablesSupport;
#endif
				calculateUnusedObjects = searchParameters.calculateUnusedObjects;
				hideDuplicateRows = searchParameters.hideDuplicateRows;
				hideRedundantPrefabReferencesInAssets = searchParameters.hideRedundantPrefabReferencesInAssets;
				hideRedundantPrefabReferencesInScenes = searchParameters.hideRedundantPrefabReferencesInScenes;
				noAssetDatabaseChanges = searchParameters.noAssetDatabaseChanges;
				showDetailedProgressBar = searchParameters.showDetailedProgressBar;

				searchInAssetsSubset.Clear();
				if( searchParameters.searchInAssetsSubset != null )
				{
					foreach( Object obj in searchParameters.searchInAssetsSubset )
						searchInAssetsSubset.Add( obj );
				}

				excludedAssets.Clear();
				if( searchParameters.excludedAssetsFromSearch != null )
				{
					foreach( Object obj in searchParameters.excludedAssetsFromSearch )
						excludedAssets.Add( obj );
				}

				excludedScenes.Clear();
				if( searchParameters.excludedScenesFromSearch != null )
				{
					foreach( Object obj in searchParameters.excludedScenesFromSearch )
						excludedScenes.Add( obj );
				}
			}

			InitiateSearch();
			Repaint();
		}

		protected virtual void Awake()
		{
			LoadPrefs();
		}

        protected virtual void OnEnable()
		{
			if( currentPhase == Phase.Complete && AssetUsageDetectorSettings.ShowCustomTooltip )
				wantsMouseMove = wantsMouseEnterLeaveWindow = true; // These values aren't preserved during domain reload on Unity 2020.3.0f1

			PrefabStage.prefabStageClosing += ReplacePrefabStageObjectsWithAssets;
		}

        protected virtual void OnDisable()
		{
			PrefabStage.prefabStageClosing -= ReplacePrefabStageObjectsWithAssets;
			SearchResultTooltip.Hide();
		}

		private void OnDestroy()
		{
			if( core != null )
				core.SaveCache();

			SavePrefs();

			if( searchResult != null && currentPhase == Phase.Complete )
				searchResult.RestoreInitialSceneSetup();
		}

		protected virtual void SavePrefs()
		{
			EditorPrefs.SetInt( PREFS_SEARCH_SCENES, (int) GetSceneSearchMode( false ) );
			EditorPrefs.SetBool( PREFS_SEARCH_SCENE_LIGHTING_SETTINGS, searchInSceneLightingSettings );
			EditorPrefs.SetBool( PREFS_SEARCH_ASSETS, searchInAssetsFolder );
			EditorPrefs.SetBool( PREFS_DONT_SEARCH_SOURCE_ASSETS, dontSearchInSourceAssets );
			EditorPrefs.SetBool( PREFS_SEARCH_PROJECT_SETTINGS, searchInProjectSettings );
			EditorPrefs.SetInt( PREFS_SEARCH_DEPTH_LIMIT, searchDepthLimit );
			EditorPrefs.SetInt( PREFS_SEARCH_FIELDS, (int) fieldModifiers );
			EditorPrefs.SetInt( PREFS_SEARCH_PROPERTIES, (int) propertyModifiers );
			EditorPrefs.SetBool( PREFS_SEARCH_NON_SERIALIZABLES, searchNonSerializableVariables );
			EditorPrefs.SetBool( PREFS_SEARCH_UNUSED_MATERIAL_PROPERTIES, searchUnusedMaterialProperties );
			EditorPrefs.SetBool( PREFS_LAZY_SCENE_SEARCH, lazySceneSearch );
#if ASSET_USAGE_ADDRESSABLES
			EditorPrefs.SetBool( PREFS_ADDRESSABLES_SUPPORT, addressablesSupport );
#endif
			EditorPrefs.SetBool( PREFS_CALCULATE_UNUSED_OBJECTS, calculateUnusedObjects );
			EditorPrefs.SetBool( PREFS_HIDE_DUPLICATE_ROWS, hideDuplicateRows );
			EditorPrefs.SetBool( PREFS_HIDE_REDUNDANT_PREFAB_REFERENCES_IN_ASSETS, hideRedundantPrefabReferencesInAssets );
			EditorPrefs.SetBool( PREFS_HIDE_REDUNDANT_PREFAB_REFERENCES_IN_SCENES, hideRedundantPrefabReferencesInScenes );
			EditorPrefs.SetBool( PREFS_SHOW_PROGRESS, showDetailedProgressBar );
		}

		protected virtual void LoadPrefs()
		{
			ParseSceneSearchMode( (SceneSearchMode) EditorPrefs.GetInt( PREFS_SEARCH_SCENES, (int) ( SceneSearchMode.OpenScenes | SceneSearchMode.ScenesInBuildSettingsTickedOnly | SceneSearchMode.AllScenes ) ) );
			searchInSceneLightingSettings = EditorPrefs.GetBool( PREFS_SEARCH_SCENE_LIGHTING_SETTINGS, true );
			searchInAssetsFolder = EditorPrefs.GetBool( PREFS_SEARCH_ASSETS, true );
			dontSearchInSourceAssets = EditorPrefs.GetBool( PREFS_DONT_SEARCH_SOURCE_ASSETS, true );
			searchInProjectSettings = EditorPrefs.GetBool( PREFS_SEARCH_PROJECT_SETTINGS, true );
			searchDepthLimit = EditorPrefs.GetInt( PREFS_SEARCH_DEPTH_LIMIT, 4 );
			fieldModifiers = (BindingFlags) EditorPrefs.GetInt( PREFS_SEARCH_FIELDS, (int) ( BindingFlags.Public | BindingFlags.NonPublic ) );
			propertyModifiers = (BindingFlags) EditorPrefs.GetInt( PREFS_SEARCH_PROPERTIES, (int) ( BindingFlags.Public | BindingFlags.NonPublic ) );
			searchNonSerializableVariables = EditorPrefs.GetBool( PREFS_SEARCH_NON_SERIALIZABLES, true );
			searchUnusedMaterialProperties = EditorPrefs.GetBool( PREFS_SEARCH_UNUSED_MATERIAL_PROPERTIES, true );
			lazySceneSearch = EditorPrefs.GetBool( PREFS_LAZY_SCENE_SEARCH, true );
#if ASSET_USAGE_ADDRESSABLES
			addressablesSupport = EditorPrefs.GetBool( PREFS_ADDRESSABLES_SUPPORT, false );
#endif
			calculateUnusedObjects = EditorPrefs.GetBool( PREFS_CALCULATE_UNUSED_OBJECTS, false );
			hideDuplicateRows = EditorPrefs.GetBool( PREFS_HIDE_DUPLICATE_ROWS, true );
			hideRedundantPrefabReferencesInAssets = EditorPrefs.GetBool( PREFS_HIDE_REDUNDANT_PREFAB_REFERENCES_IN_ASSETS, hideRedundantPrefabReferencesInAssets );
			hideRedundantPrefabReferencesInScenes = EditorPrefs.GetBool( PREFS_HIDE_REDUNDANT_PREFAB_REFERENCES_IN_SCENES, hideRedundantPrefabReferencesInScenes );
			showDetailedProgressBar = EditorPrefs.GetBool( PREFS_SHOW_PROGRESS, true );
		}

        protected SceneSearchMode GetSceneSearchMode( bool hideOptionsInPlayMode )
		{
			SceneSearchMode sceneSearchMode = SceneSearchMode.None;
			if( searchInOpenScenes )
				sceneSearchMode |= SceneSearchMode.OpenScenes;
			if( !hideOptionsInPlayMode || !EditorApplication.isPlaying )
			{
				if( searchInScenesInBuild )
					sceneSearchMode |= searchInScenesInBuildTickedOnly ? SceneSearchMode.ScenesInBuildSettingsTickedOnly : SceneSearchMode.ScenesInBuildSettingsAll;
				if( searchInAllScenes )
					sceneSearchMode |= SceneSearchMode.AllScenes;
			}

			return sceneSearchMode;
		}

        protected void ParseSceneSearchMode( SceneSearchMode sceneSearchMode )
		{
			searchInOpenScenes = ( sceneSearchMode & SceneSearchMode.OpenScenes ) == SceneSearchMode.OpenScenes;
			searchInScenesInBuild = ( sceneSearchMode & SceneSearchMode.ScenesInBuildSettingsAll ) == SceneSearchMode.ScenesInBuildSettingsAll || ( sceneSearchMode & SceneSearchMode.ScenesInBuildSettingsTickedOnly ) == SceneSearchMode.ScenesInBuildSettingsTickedOnly;
			searchInScenesInBuildTickedOnly = ( sceneSearchMode & SceneSearchMode.ScenesInBuildSettingsAll ) != SceneSearchMode.ScenesInBuildSettingsAll;
			searchInAllScenes = ( sceneSearchMode & SceneSearchMode.AllScenes ) == SceneSearchMode.AllScenes;
		}

        protected virtual void Update()
		{
			if( shouldRepositionSelf )
			{
				shouldRepositionSelf = false;
				position = windowTargetPosition;
			}
		}

        protected virtual void OnGUI()
		{
			// Make the window scrollable
			scrollPosition = EditorGUILayout.BeginScrollView( scrollPosition, Utilities.GL_EXPAND_WIDTH, Utilities.GL_EXPAND_HEIGHT );

			GUILayout.BeginVertical();

			if( currentPhase == Phase.Processing )
			{
				// If we are stuck at this phase, then we have encountered an exception
				GUILayout.Label( ". . . Search in progress or something went wrong (check console) . . ." );

				if( GUILayout.Button( "RETURN", Utilities.GL_HEIGHT_30 ) )
				{
					ReturnToSetupPhase();
					GUIUtility.ExitGUI();
				}
			}
			else if( currentPhase == Phase.Setup )
			{
				DrawObjectsToSearchSection();

				GUILayout.Space( 10f );

				Utilities.DrawHeader( "<b>SEARCH IN</b>" );

				searchInAssetsFolder = WordWrappingToggleLeft( "Project window (Assets folder)", searchInAssetsFolder );

				if( searchInAssetsFolder )
				{
					BeginIndentedGUI();
					searchInAssetsSubsetDrawer.Draw( searchInAssetsSubset );
					excludedAssetsDrawer.Draw( excludedAssets );
					EndIndentedGUI();
				}

				GUILayout.Space( 5f );

				dontSearchInSourceAssets = WordWrappingToggleLeft( "Don't search \"SEARCHED OBJECTS\" themselves for references", dontSearchInSourceAssets );
				searchUnusedMaterialProperties = WordWrappingToggleLeft( "Search unused material properties (e.g. normal map of a material that no longer uses normal mapping)", searchUnusedMaterialProperties );

				Utilities.DrawSeparatorLine();

				if( searchInAllScenes && !EditorApplication.isPlaying )
					GUI.enabled = false;

				searchInOpenScenes = WordWrappingToggleLeft( "Currently open (loaded) scene(s)", searchInOpenScenes );

				if( !EditorApplication.isPlaying )
				{
					searchInScenesInBuild = WordWrappingToggleLeft( "Scenes in Build Settings", searchInScenesInBuild );

					if( searchInScenesInBuild )
					{
						BeginIndentedGUI( false );
						searchInScenesInBuildTickedOnly = EditorGUILayout.ToggleLeft( "Ticked only", searchInScenesInBuildTickedOnly, Utilities.GL_WIDTH_100 );
						searchInScenesInBuildTickedOnly = !EditorGUILayout.ToggleLeft( "All", !searchInScenesInBuildTickedOnly, Utilities.GL_WIDTH_100 );
						EndIndentedGUI( false );
					}

					GUI.enabled = true;

					searchInAllScenes = WordWrappingToggleLeft( "All scenes in the project", searchInAllScenes );
				}

				BeginIndentedGUI();
				excludedScenesDrawer.Draw( excludedScenes );
				EndIndentedGUI();

				EditorGUI.BeginDisabledGroup( !searchInOpenScenes && !searchInScenesInBuild && !searchInAllScenes );
				searchInSceneLightingSettings = WordWrappingToggleLeft( "Scene Lighting Settings (WARNING: This may change the active scene during search)", searchInSceneLightingSettings );
				EditorGUI.EndDisabledGroup();

				Utilities.DrawSeparatorLine();

				searchInProjectSettings = WordWrappingToggleLeft( "Project Settings (Player Settings, Graphics Settings etc.)", searchInProjectSettings );

				GUILayout.Space( 10f );

				Utilities.DrawHeader( "<b>SETTINGS</b>" );

#if ASSET_USAGE_ADDRESSABLES
				EditorGUI.BeginDisabledGroup( addressablesSupport );
#endif
				lazySceneSearch = WordWrappingToggleLeft( "Lazy scene search: scenes are searched in detail only when they are manually refreshed (faster search)", lazySceneSearch );
#if ASSET_USAGE_ADDRESSABLES
				EditorGUI.EndDisabledGroup();
				addressablesSupport = WordWrappingToggleLeft( "Addressables support (Experimental) (WARNING: 'Lazy scene search' will be disabled) (slower search)", addressablesSupport );
#endif
				calculateUnusedObjects = WordWrappingToggleLeft( "Calculate unused objects", calculateUnusedObjects );
				hideDuplicateRows = WordWrappingToggleLeft( "Hide duplicate rows in search results", hideDuplicateRows );
				hideRedundantPrefabReferencesInAssets = WordWrappingToggleLeft( hideRedundantPrefabReferencesInAssetsLabel, hideRedundantPrefabReferencesInAssets );
				hideRedundantPrefabReferencesInScenes = WordWrappingToggleLeft( hideRedundantPrefabReferencesInScenesLabel, hideRedundantPrefabReferencesInScenes );
				noAssetDatabaseChanges = WordWrappingToggleLeft( "I haven't modified any assets/scenes since the last search (faster search)", noAssetDatabaseChanges );
				showDetailedProgressBar = WordWrappingToggleLeft( "Update search progress bar more often (cancelable search) (slower search)", showDetailedProgressBar );

				GUILayout.Space( 10f );

				// Don't let the user press the GO button without any valid search location
				if( !searchInAllScenes && !searchInOpenScenes && !searchInScenesInBuild && !searchInAssetsFolder && !searchInProjectSettings )
					GUI.enabled = false;

				if( GUILayout.Button( "GO!", Utilities.GL_HEIGHT_30 ) )
				{
					InitiateSearch();
					GUIUtility.ExitGUI();
				}

				GUILayout.Space( 5f );
			}
			else if( currentPhase == Phase.Complete )
			{
				// Draw the results of the search
				GUI.enabled = false;

				DrawObjectsToSearchSection();

				if( drawObjectsToSearchSection )
					GUILayout.Space( 10f );

				GUI.enabled = true;

				if( GUILayout.Button( "Reset Search", Utilities.GL_HEIGHT_30 ) )
				{
					ReturnToSetupPhase();
					GUIUtility.ExitGUI();
				}

				if( searchResult == null )
				{
					EditorGUILayout.HelpBox( "ERROR: searchResult is null", MessageType.Error );
					return;
				}
				else if( !searchResult.SearchCompletedSuccessfully )
					EditorGUILayout.HelpBox( "ERROR: search was interrupted, check the logs for more info", MessageType.Error );

				if( searchResult.NumberOfGroups == 0 )
				{
					GUILayout.Space( 10f );
					GUILayout.Box( "No references found...", Utilities.BoxGUIStyle, Utilities.GL_EXPAND_WIDTH );
				}
				else
				{
					noAssetDatabaseChanges = WordWrappingToggleLeft( "I haven't modified any assets/scenes since the last search (faster Refresh)", noAssetDatabaseChanges );

					EditorGUILayout.Space();

					scrollPosition.y = searchResult.DrawOnGUI( this, scrollPosition.y, noAssetDatabaseChanges );
				}
			}

			if( Event.current.type == EventType.MouseLeaveWindow )
			{
				SearchResultTooltip.Hide();

				if( searchResult != null )
					searchResult.CancelDelayedTreeViewTooltip();
			}

			GUILayout.EndVertical();

			EditorGUILayout.EndScrollView();
		}

        protected void DrawObjectsToSearchSection()
		{
			Utilities.DrawHeader( "<b>SEARCHED OBJECTS</b>" );

			Rect searchedObjectsHeaderRect = GUILayoutUtility.GetLastRect();
			searchedObjectsHeaderRect.x += 5f;
			searchedObjectsHeaderRect.yMin += ( searchedObjectsHeaderRect.height - EditorGUIUtility.singleLineHeight ) * 0.5f;
			searchedObjectsHeaderRect.height = EditorGUIUtility.singleLineHeight;

			drawObjectsToSearchSection = EditorGUI.Foldout( searchedObjectsHeaderRect, drawObjectsToSearchSection, GUIContent.none, true );

			if( drawObjectsToSearchSection )
				objectsToSearchDrawer.Draw( objectsToSearch );
		}

        protected void BeginIndentedGUI( bool isVertical = true )
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space( 35f );

			if( isVertical )
				GUILayout.BeginVertical();
		}

        protected void EndIndentedGUI( bool isVertical = true )
		{
			if( isVertical )
				GUILayout.EndVertical();

			GUILayout.EndHorizontal();
		}

		public static bool WordWrappingToggleLeft( string label, bool value )
		{
			sharedGUIContent.text = label;
			sharedGUIContent.tooltip = null;
			return WordWrappingToggleLeft( sharedGUIContent, value );
		}

		public static bool WordWrappingToggleLeft( GUIContent label, bool value )
		{
			GUILayout.BeginHorizontal();
			bool result = EditorGUILayout.ToggleLeft( GUIContent.none, value, GL_WIDTH_12 );
			if( GUILayout.Button( label, EditorStyles.wordWrappedLabel ) )
			{
				GUI.FocusControl( null );
				result = !value;
			}
			GUILayout.EndHorizontal();

			return result;
		}

        protected void InitiateSearch()
		{
			currentPhase = Phase.Processing;

			SavePrefs();
			ReplacePrefabStageObjectsWithAssets( PrefabStageUtility.GetCurrentPrefabStage() );

			// Start searching
			searchResult = core.Run( new AssetUsageDetector.Parameters()
			{
				objectsToSearch = !objectsToSearch.IsEmpty() ? new ObjectToSearchEnumerator( objectsToSearch ).ToArray() : null,
				searchInScenes = GetSceneSearchMode( true ),
				searchInSceneLightingSettings = searchInSceneLightingSettings,
				searchInAssetsFolder = searchInAssetsFolder,
				searchInAssetsSubset = !searchInAssetsSubset.IsEmpty() ? searchInAssetsSubset.ToArray() : null,
				excludedAssetsFromSearch = !excludedAssets.IsEmpty() ? excludedAssets.ToArray() : null,
				dontSearchInSourceAssets = dontSearchInSourceAssets,
				excludedScenesFromSearch = !excludedScenes.IsEmpty() ? excludedScenes.ToArray() : null,
				searchInProjectSettings = searchInProjectSettings,
				//fieldModifiers = fieldModifiers,
				//propertyModifiers = propertyModifiers,
				//searchDepthLimit = searchDepthLimit,
				//searchNonSerializableVariables = searchNonSerializableVariables,
				searchUnusedMaterialProperties = searchUnusedMaterialProperties,
				searchRefactoring = searchRefactoring,
#if ASSET_USAGE_ADDRESSABLES
				lazySceneSearch = lazySceneSearch && !addressablesSupport,
				addressablesSupport = addressablesSupport,
#else
				lazySceneSearch = lazySceneSearch,
#endif
				calculateUnusedObjects = calculateUnusedObjects,
				hideDuplicateRows = hideDuplicateRows,
				hideRedundantPrefabReferencesInAssets = hideRedundantPrefabReferencesInAssets,
				hideRedundantPrefabReferencesInScenes = hideRedundantPrefabReferencesInScenes && searchInAssetsFolder,
				noAssetDatabaseChanges = noAssetDatabaseChanges,
				showDetailedProgressBar = showDetailedProgressBar
			} );

			currentPhase = Phase.Complete;

			// We really don't want SearchRefactoring to affect next searches unless the search is initiated via ShowAndSearch again
			searchRefactoring = null;

			if( AssetUsageDetectorSettings.ShowCustomTooltip )
				wantsMouseMove = wantsMouseEnterLeaveWindow = true;
		}

		// Try replacing searched objects who are part of currently open prefab stage with their corresponding prefab assets
		public void ReplacePrefabStageObjectsWithAssets( PrefabStage prefabStage )
		{
			if( prefabStage == null || !prefabStage.stageHandle.IsValid() )
				return;

			GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>( prefabStage.assetPath );
			if( prefabAsset == null || prefabAsset.Equals( null ) )
				return;

			for( int i = 0; i < objectsToSearch.Count; i++ )
			{
				Object obj = objectsToSearch[i].obj;
				if( obj != null && !obj.Equals( null ) && obj is GameObject && prefabStage.IsPartOfPrefabContents( (GameObject) obj ) )
				{
					GameObject prefabStageObjectSource = ( (GameObject) obj ).FollowSymmetricHierarchy( prefabStage.prefabContentsRoot, prefabAsset );
					if( prefabStageObjectSource != null )
						objectsToSearch[i].obj = prefabStageObjectSource;

					List<ObjectToSearch.SubAsset> subAssets = objectsToSearch[i].subAssets;
					for( int j = 0; j < subAssets.Count; j++ )
					{
						obj = subAssets[j].subAsset;
						if( obj != null && !obj.Equals( null ) && obj is GameObject && prefabStage.IsPartOfPrefabContents( (GameObject) obj ) )
						{
							prefabStageObjectSource = ( (GameObject) obj ).FollowSymmetricHierarchy( prefabStage.prefabContentsRoot, prefabAsset );
							if( prefabStageObjectSource != null )
								subAssets[j].subAsset = prefabStageObjectSource;
						}
					}
				}
			}
		}

        protected bool ReturnToSetupPhase()
		{
			if( searchResult != null && !EditorApplication.isPlaying && !searchResult.RestoreInitialSceneSetup() )
				return false;

			searchResult = null;
			currentPhase = Phase.Setup;
			wantsMouseMove = wantsMouseEnterLeaveWindow = false;

			SearchResultTooltip.Hide();

			return true;
		}

		internal void OnSettingsChanged( bool highlightedSearchTextColorChanged = false, bool tooltipDescriptionsColorChanged = false )
		{
			if( searchResult == null )
				return;

			wantsMouseMove = wantsMouseEnterLeaveWindow = AssetUsageDetectorSettings.ShowCustomTooltip;

			for( int i = searchResult.NumberOfGroups - 1; i >= 0; i-- )
			{
				if( searchResult[i].treeView != null )
				{
					searchResult[i].treeView.rowHeight = EditorGUIUtility.singleLineHeight + AssetUsageDetectorSettings.ExtraRowHeight;
					searchResult[i].treeView.OnSettingsChanged( highlightedSearchTextColorChanged, tooltipDescriptionsColorChanged );
				}
			}
		}
	}
}