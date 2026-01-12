using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneSwitcher.Editor
{
    /// <summary>
    /// Provides a dropdown in the Unity toolbar for quick scene switching.
    /// This gives immediate access to scenes without opening a separate window.
    /// </summary>
    [InitializeOnLoad]
    public static class SceneSwitcherToolbar
    {
        private static List<SceneInfo> _cachedScenes = new();
        private static string[] _sceneNames = Array.Empty<string>();
        private static int _currentSceneIndex = -1;
        private static double _lastRefreshTime;
        private const double REFRESH_INTERVAL = 5.0; // Refresh every 5 seconds max
        
        private struct SceneInfo
        {
            public string Name;
            public string Path;
            public bool IsInBuild;
            public int BuildIndex;
        }
        
        static SceneSwitcherToolbar()
        {
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            
            RefreshSceneCache();
        }
        
        private static void OnEditorUpdate()
        {
            // Periodic refresh
            if (EditorApplication.timeSinceStartup - _lastRefreshTime > REFRESH_INTERVAL)
            {
                RefreshSceneCache();
            }
        }
        
        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            RefreshSceneCache();
        }
        
        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            UpdateCurrentSceneIndex();
        }
        
        private static void RefreshSceneCache()
        {
            _lastRefreshTime = EditorApplication.timeSinceStartup;
            _cachedScenes.Clear();
            
            var sceneGuids = AssetDatabase.FindAssets("t:SceneAsset");
            var buildScenes = EditorBuildSettings.scenes;
            
            foreach (var guid in sceneGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;
                
                var sceneInfo = new SceneInfo
                {
                    Name = System.IO.Path.GetFileNameWithoutExtension(path),
                    Path = path,
                    IsInBuild = false,
                    BuildIndex = -1
                };
                
                // Check if in build settings
                for (int i = 0; i < buildScenes.Length; i++)
                {
                    if (buildScenes[i].path == path)
                    {
                        sceneInfo.IsInBuild = true;
                        sceneInfo.BuildIndex = i;
                        break;
                    }
                }
                
                _cachedScenes.Add(sceneInfo);
            }
            
            // Sort: Build scenes first (by index), then alphabetically
            _cachedScenes = _cachedScenes
                .OrderByDescending(s => s.IsInBuild)
                .ThenBy(s => s.IsInBuild ? s.BuildIndex : int.MaxValue)
                .ThenBy(s => s.Name)
                .ToList();
            
            // Create display names
            _sceneNames = _cachedScenes
                .Select(s => s.IsInBuild ? $"[{s.BuildIndex}] {s.Name}" : s.Name)
                .ToArray();
            
            UpdateCurrentSceneIndex();
        }
        
        private static void UpdateCurrentSceneIndex()
        {
            var currentPath = SceneManager.GetActiveScene().path;
            _currentSceneIndex = _cachedScenes.FindIndex(s => s.Path == currentPath);
        }
        
        /// <summary>
        /// Draw the scene dropdown. Call this from your custom toolbar or editor window.
        /// </summary>
        public static void DrawSceneDropdown(float width = 200f)
        {
            if (_sceneNames.Length == 0)
            {
                RefreshSceneCache();
            }
            
            EditorGUI.BeginChangeCheck();
            
            var newIndex = EditorGUILayout.Popup(
                _currentSceneIndex, 
                _sceneNames, 
                EditorStyles.toolbarPopup,
                GUILayout.Width(width)
            );
            
            if (EditorGUI.EndChangeCheck() && newIndex != _currentSceneIndex && newIndex >= 0)
            {
                LoadSceneAtIndex(newIndex);
            }
        }
        
        /// <summary>
        /// Draw a compact scene dropdown with additional controls.
        /// </summary>
        public static void DrawSceneDropdownCompact()
        {
            EditorGUILayout.BeginHorizontal();
            
            // Dropdown
            DrawSceneDropdown(180f);
            
            // Quick action buttons
            if (GUILayout.Button(EditorGUIUtility.IconContent("Refresh"), 
                EditorStyles.toolbarButton, GUILayout.Width(24)))
            {
                RefreshSceneCache();
            }
            
            if (GUILayout.Button(EditorGUIUtility.IconContent("_Popup"), 
                EditorStyles.toolbarButton, GUILayout.Width(24)))
            {
                SceneSwitcherWindow.ShowWindow();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private static void LoadSceneAtIndex(int index)
        {
            if (index < 0 || index >= _cachedScenes.Count) return;
            
            var scene = _cachedScenes[index];
            
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
            
            EditorSceneManager.OpenScene(scene.Path);
            _currentSceneIndex = index;
        }
        
        /// <summary>
        /// Get all scene paths (useful for custom implementations).
        /// </summary>
        public static string[] GetAllScenePaths()
        {
            return _cachedScenes.Select(s => s.Path).ToArray();
        }
        
        /// <summary>
        /// Get all scene names (useful for custom implementations).
        /// </summary>
        public static string[] GetAllSceneNames()
        {
            return _sceneNames;
        }
        
        /// <summary>
        /// Load a scene by path.
        /// </summary>
        public static void LoadScene(string path, bool saveCurrentIfDirty = true)
        {
            if (saveCurrentIfDirty && SceneManager.GetActiveScene().isDirty)
            {
                if (EditorUtility.DisplayDialog(
                    "Scene Has Been Modified",
                    "Do you want to save the current scene before switching?",
                    "Save", "Don't Save"))
                {
                    EditorSceneManager.SaveOpenScenes();
                }
            }
            
            EditorSceneManager.OpenScene(path);
            UpdateCurrentSceneIndex();
        }
    }
}
