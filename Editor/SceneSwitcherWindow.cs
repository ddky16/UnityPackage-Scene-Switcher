using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneSwitcher.Editor
{
    /// <summary>
    /// Main editor window for quick scene switching.
    /// Provides a searchable dropdown to load any scene in the project.
    /// </summary>
    public class SceneSwitcherWindow : EditorWindow
    {
        #region Fields
        
        private Vector2 _scrollPosition;
        private string _searchFilter = "";
        private List<SceneData> _allScenes = new();
        private List<SceneData> _filteredScenes = new();
        private List<SceneData> _favoriteScenes = new();
        private List<SceneData> _recentScenes = new();
        
        private int _selectedTab = 0;
        private readonly string[] _tabNames = { "All Scenes", "Build Scenes", "Favorites", "Recent" };
        
        private GUIStyle _sceneButtonStyle;
        private GUIStyle _favoriteButtonStyle;
        private GUIStyle _activeSceneStyle;
        private GUIStyle _searchFieldStyle;
        
        private bool _stylesInitialized = false;
        
        private const int MAX_RECENT_SCENES = 10;
        private const string FAVORITES_PREF_KEY = "SceneSwitcher_Favorites";
        private const string RECENT_PREF_KEY = "SceneSwitcher_Recent";
        
        #endregion

        #region Data Structures
        
        [Serializable]
        private class SceneData
        {
            public string Name;
            public string Path;
            public bool IsInBuildSettings;
            public int BuildIndex;
            
            public SceneData(string path)
            {
                Path = path;
                Name = System.IO.Path.GetFileNameWithoutExtension(path);
                UpdateBuildInfo();
            }
            
            public void UpdateBuildInfo()
            {
                IsInBuildSettings = false;
                BuildIndex = -1;
                
                var buildScenes = EditorBuildSettings.scenes;
                for (int i = 0; i < buildScenes.Length; i++)
                {
                    if (buildScenes[i].path == Path)
                    {
                        IsInBuildSettings = true;
                        BuildIndex = i;
                        break;
                    }
                }
            }
        }
        
        [Serializable]
        private class ScenePathList
        {
            public List<string> Paths = new();
        }
        
        #endregion

        #region Menu Items
        
        [MenuItem("Tools/Scene Switcher/Open Scene Switcher %#o", priority = 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<SceneSwitcherWindow>("Scene Switcher");
            window.minSize = new Vector2(300, 400);
            window.Show();
        }
        
        [MenuItem("Tools/Scene Switcher/Refresh Scene List", priority = 101)]
        public static void RefreshSceneList()
        {
            var window = GetWindow<SceneSwitcherWindow>();
            window.RefreshScenes();
        }
        
        #endregion

        #region Unity Callbacks
        
        private void OnEnable()
        {
            RefreshScenes();
            LoadFavorites();
            LoadRecent();
            
            EditorBuildSettings.sceneListChanged += OnBuildSettingsChanged;
        }
        
        private void OnDisable()
        {
            EditorBuildSettings.sceneListChanged -= OnBuildSettingsChanged;
        }
        
        private void OnFocus()
        {
            RefreshScenes();
        }
        
        private void OnBuildSettingsChanged()
        {
            foreach (var scene in _allScenes)
            {
                scene.UpdateBuildInfo();
            }
            Repaint();
        }
        
        #endregion

        #region GUI
        
        private void InitializeStyles()
        {
            if (_stylesInitialized) return;
            
            _sceneButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
            {
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = 24,
                padding = new RectOffset(8, 8, 4, 4),
                margin = new RectOffset(2, 2, 1, 1)
            };
            
            _favoriteButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedWidth = 20,
                fixedHeight = 20,
                padding = new RectOffset(0, 0, 0, 0)
            };
            
            _activeSceneStyle = new GUIStyle(_sceneButtonStyle)
            {
                fontStyle = FontStyle.Bold
            };
            
            _searchFieldStyle = new GUIStyle(EditorStyles.toolbarSearchField)
            {
                fixedHeight = 20,
                margin = new RectOffset(4, 4, 4, 4)
            };
            
            _stylesInitialized = true;
        }
        
        private void OnGUI()
        {
            InitializeStyles();
            
            DrawToolbar();
            DrawTabs();
            DrawSearchField();
            DrawSceneList();
            DrawFooter();
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                RefreshScenes();
            }
            
            GUILayout.FlexibleSpace();
            
            // Current scene indicator
            var currentScene = SceneManager.GetActiveScene();
            GUILayout.Label($"Current: {currentScene.name}", EditorStyles.toolbarButton);
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawTabs()
        {
            EditorGUILayout.BeginHorizontal();
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames, GUILayout.Height(25));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(2);
        }
        
        private void DrawSearchField()
        {
            EditorGUILayout.BeginHorizontal();
            
            GUI.SetNextControlName("SearchField");
            var newFilter = EditorGUILayout.TextField(_searchFilter, EditorStyles.toolbarSearchField);
            
            if (newFilter != _searchFilter)
            {
                _searchFilter = newFilter;
                FilterScenes();
            }
            
            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton") ?? EditorStyles.miniButton, GUILayout.Width(18)))
            {
                _searchFilter = "";
                FilterScenes();
                GUI.FocusControl(null);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(4);
        }
        
        private void DrawSceneList()
        {
            var scenesToShow = GetScenesForCurrentTab();
            
            if (scenesToShow.Count == 0)
            {
                EditorGUILayout.HelpBox(GetEmptyMessage(), MessageType.Info);
                return;
            }
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            var currentScenePath = SceneManager.GetActiveScene().path;
            
            foreach (var scene in scenesToShow)
            {
                DrawSceneEntry(scene, scene.Path == currentScenePath);
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawSceneEntry(SceneData scene, bool isCurrentScene)
        {
            var bgColor = GUI.backgroundColor;
            
            if (isCurrentScene)
            {
                GUI.backgroundColor = new Color(0.3f, 0.6f, 0.3f, 1f);
            }
            
            EditorGUILayout.BeginHorizontal();
            
            // Favorite toggle
            bool isFavorite = _favoriteScenes.Any(f => f.Path == scene.Path);
            var favoriteIcon = isFavorite ? "\u2605" : "\u2606"; // ★ or ☆
            
            if (GUILayout.Button(favoriteIcon, _favoriteButtonStyle))
            {
                ToggleFavorite(scene);
            }
            
            // Scene button
            var buttonLabel = scene.Name;
            if (scene.IsInBuildSettings)
            {
                buttonLabel = $"[{scene.BuildIndex}] {scene.Name}";
            }
            
            var style = isCurrentScene ? _activeSceneStyle : _sceneButtonStyle;
            
            if (GUILayout.Button(buttonLabel, style))
            {
                LoadScene(scene);
            }
            
            // Context menu button
            if (GUILayout.Button("\u22EE", EditorStyles.miniButton, GUILayout.Width(20))) // ⋮
            {
                ShowSceneContextMenu(scene);
            }
            
            EditorGUILayout.EndHorizontal();
            
            GUI.backgroundColor = bgColor;
        }
        
        private void DrawFooter()
        {
            EditorGUILayout.Space(4);
            
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            GUILayout.Label($"Total: {_allScenes.Count} scenes", EditorStyles.miniLabel);
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Build Settings", EditorStyles.toolbarButton, GUILayout.Width(90)))
            {
                EditorWindow.GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        #endregion

        #region Scene Management
        
        private void RefreshScenes()
        {
            _allScenes.Clear();
            
            // Find all scene assets in the project
            var sceneGuids = AssetDatabase.FindAssets("t:SceneAsset");
            
            foreach (var guid in sceneGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path))
                {
                    _allScenes.Add(new SceneData(path));
                }
            }
            
            // Sort by name
            _allScenes = _allScenes.OrderBy(s => s.Name).ToList();
            
            FilterScenes();
            Repaint();
        }
        
        private void FilterScenes()
        {
            if (string.IsNullOrWhiteSpace(_searchFilter))
            {
                _filteredScenes = new List<SceneData>(_allScenes);
            }
            else
            {
                var filter = _searchFilter.ToLowerInvariant();
                _filteredScenes = _allScenes
                    .Where(s => s.Name.ToLowerInvariant().Contains(filter) || 
                                s.Path.ToLowerInvariant().Contains(filter))
                    .ToList();
            }
        }
        
        private List<SceneData> GetScenesForCurrentTab()
        {
            var baseList = _selectedTab switch
            {
                0 => _filteredScenes,                                           // All Scenes
                1 => _filteredScenes.Where(s => s.IsInBuildSettings).ToList(),  // Build Scenes
                2 => _favoriteScenes,                                            // Favorites
                3 => _recentScenes,                                              // Recent
                _ => _filteredScenes
            };
            
            // Apply search filter to favorites and recent too
            if (!string.IsNullOrWhiteSpace(_searchFilter) && (_selectedTab == 2 || _selectedTab == 3))
            {
                var filter = _searchFilter.ToLowerInvariant();
                return baseList
                    .Where(s => s.Name.ToLowerInvariant().Contains(filter) || 
                                s.Path.ToLowerInvariant().Contains(filter))
                    .ToList();
            }
            
            return baseList;
        }
        
        private string GetEmptyMessage()
        {
            return _selectedTab switch
            {
                0 => string.IsNullOrWhiteSpace(_searchFilter) 
                    ? "No scenes found in project." 
                    : $"No scenes matching '{_searchFilter}'",
                1 => "No scenes added to Build Settings.",
                2 => "No favorite scenes. Click the star icon to add favorites.",
                3 => "No recent scenes.",
                _ => "No scenes to display."
            };
        }
        
        private void LoadScene(SceneData scene)
        {
            if (string.IsNullOrEmpty(scene.Path)) return;
            
            // Check for unsaved changes
            if (SceneManager.GetActiveScene().isDirty)
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    "Scene Has Been Modified",
                    "Do you want to save the current scene before switching?",
                    "Save",
                    "Don't Save",
                    "Cancel"
                );
                
                switch (choice)
                {
                    case 0: // Save
                        EditorSceneManager.SaveOpenScenes();
                        break;
                    case 1: // Don't Save
                        break;
                    case 2: // Cancel
                        return;
                }
            }
            
            // Load the scene
            EditorSceneManager.OpenScene(scene.Path);
            
            // Add to recent
            AddToRecent(scene);
        }
        
        private void ShowSceneContextMenu(SceneData scene)
        {
            var menu = new GenericMenu();
            
            menu.AddItem(new GUIContent("Open Scene"), false, () => LoadScene(scene));
            menu.AddItem(new GUIContent("Open Additive"), false, () => 
                EditorSceneManager.OpenScene(scene.Path, OpenSceneMode.Additive));
            
            menu.AddSeparator("");
            
            bool isFavorite = _favoriteScenes.Any(f => f.Path == scene.Path);
            menu.AddItem(new GUIContent(isFavorite ? "Remove from Favorites" : "Add to Favorites"), 
                isFavorite, () => ToggleFavorite(scene));
            
            menu.AddSeparator("");
            
            if (!scene.IsInBuildSettings)
            {
                menu.AddItem(new GUIContent("Add to Build Settings"), false, () => AddToBuildSettings(scene));
            }
            else
            {
                menu.AddItem(new GUIContent("Remove from Build Settings"), false, () => RemoveFromBuildSettings(scene));
            }
            
            menu.AddSeparator("");
            
            menu.AddItem(new GUIContent("Ping in Project"), false, () =>
            {
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.Path);
                EditorGUIUtility.PingObject(sceneAsset);
            });
            
            menu.AddItem(new GUIContent("Show in Explorer"), false, () =>
            {
                EditorUtility.RevealInFinder(scene.Path);
            });
            
            menu.AddItem(new GUIContent("Copy Path"), false, () =>
            {
                EditorGUIUtility.systemCopyBuffer = scene.Path;
            });
            
            menu.ShowAsContext();
        }
        
        #endregion

        #region Build Settings
        
        private void AddToBuildSettings(SceneData scene)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.Add(new EditorBuildSettingsScene(scene.Path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            scene.UpdateBuildInfo();
            Repaint();
        }
        
        private void RemoveFromBuildSettings(SceneData scene)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.RemoveAll(s => s.path == scene.Path);
            EditorBuildSettings.scenes = scenes.ToArray();
            scene.UpdateBuildInfo();
            
            // Update all scenes' build indices
            foreach (var s in _allScenes)
            {
                s.UpdateBuildInfo();
            }
            
            Repaint();
        }
        
        #endregion

        #region Favorites & Recent
        
        private void ToggleFavorite(SceneData scene)
        {
            var existing = _favoriteScenes.FirstOrDefault(f => f.Path == scene.Path);
            if (existing != null)
            {
                _favoriteScenes.Remove(existing);
            }
            else
            {
                _favoriteScenes.Add(scene);
            }
            
            SaveFavorites();
            Repaint();
        }
        
        private void SaveFavorites()
        {
            var pathList = new ScenePathList { Paths = _favoriteScenes.Select(s => s.Path).ToList() };
            EditorPrefs.SetString(FAVORITES_PREF_KEY, JsonUtility.ToJson(pathList));
        }
        
        private void LoadFavorites()
        {
            _favoriteScenes.Clear();
            
            var json = EditorPrefs.GetString(FAVORITES_PREF_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var pathList = JsonUtility.FromJson<ScenePathList>(json);
                    foreach (var path in pathList.Paths)
                    {
                        if (File.Exists(path))
                        {
                            _favoriteScenes.Add(new SceneData(path));
                        }
                    }
                }
                catch
                {
                    // Ignore corrupted prefs
                }
            }
        }
        
        private void AddToRecent(SceneData scene)
        {
            // Remove if already in recent
            _recentScenes.RemoveAll(s => s.Path == scene.Path);
            
            // Add at the beginning
            _recentScenes.Insert(0, scene);
            
            // Trim to max
            while (_recentScenes.Count > MAX_RECENT_SCENES)
            {
                _recentScenes.RemoveAt(_recentScenes.Count - 1);
            }
            
            SaveRecent();
        }
        
        private void SaveRecent()
        {
            var pathList = new ScenePathList { Paths = _recentScenes.Select(s => s.Path).ToList() };
            EditorPrefs.SetString(RECENT_PREF_KEY, JsonUtility.ToJson(pathList));
        }
        
        private void LoadRecent()
        {
            _recentScenes.Clear();
            
            var json = EditorPrefs.GetString(RECENT_PREF_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var pathList = JsonUtility.FromJson<ScenePathList>(json);
                    foreach (var path in pathList.Paths)
                    {
                        if (File.Exists(path))
                        {
                            _recentScenes.Add(new SceneData(path));
                        }
                    }
                }
                catch
                {
                    // Ignore corrupted prefs
                }
            }
        }
        
        #endregion
    }
}
