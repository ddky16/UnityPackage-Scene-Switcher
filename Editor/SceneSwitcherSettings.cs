using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SceneSwitcher.Editor
{
    /// <summary>
    /// ScriptableObject for storing Scene Switcher settings and favorites.
    /// This allows settings to be version-controlled and shared across team.
    /// </summary>
    [CreateAssetMenu(fileName = "SceneSwitcherSettings", menuName = "Scene Switcher/Settings")]
    public class SceneSwitcherSettings : ScriptableObject
    {
        [Header("Favorites")]
        [Tooltip("List of favorite scene paths that appear in the Favorites tab")]
        public List<string> FavoriteScenePaths = new();
        
        [Header("Display Settings")]
        [Tooltip("Show build index next to scene names")]
        public bool ShowBuildIndex = true;
        
        [Tooltip("Show scene path in tooltip")]
        public bool ShowPathInTooltip = true;
        
        [Tooltip("Group scenes by folder")]
        public bool GroupByFolder = false;
        
        [Header("Behavior")]
        [Tooltip("Automatically save current scene before switching")]
        public bool AutoSaveOnSwitch = false;
        
        [Tooltip("Maximum number of recent scenes to remember")]
        [Range(5, 20)]
        public int MaxRecentScenes = 10;
        
        [Header("Quick Access Scenes")]
        [Tooltip("Scenes that appear at the top of the dropdown for quick access")]
        public List<SceneAsset> QuickAccessScenes = new();
        
        [Header("Keyboard Shortcuts")]
        [Tooltip("Enable Ctrl+1 through Ctrl+9 to quickly load first 9 build scenes")]
        public bool EnableNumberShortcuts = true;
        
        private static SceneSwitcherSettings _instance;
        
        /// <summary>
        /// Get or create the settings instance.
        /// </summary>
        public static SceneSwitcherSettings GetOrCreateSettings()
        {
            if (_instance != null) return _instance;
            
            // Try to find existing settings
            var guids = UnityEditor.AssetDatabase.FindAssets("t:SceneSwitcherSettings");
            if (guids.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<SceneSwitcherSettings>(path);
            }
            
            return _instance;
        }
        
        /// <summary>
        /// Add a scene to favorites.
        /// </summary>
        public void AddFavorite(string scenePath)
        {
            if (!FavoriteScenePaths.Contains(scenePath))
            {
                FavoriteScenePaths.Add(scenePath);
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
        
        /// <summary>
        /// Remove a scene from favorites.
        /// </summary>
        public void RemoveFavorite(string scenePath)
        {
            if (FavoriteScenePaths.Remove(scenePath))
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
        
        /// <summary>
        /// Check if a scene is in favorites.
        /// </summary>
        public bool IsFavorite(string scenePath)
        {
            return FavoriteScenePaths.Contains(scenePath);
        }
    }
}
