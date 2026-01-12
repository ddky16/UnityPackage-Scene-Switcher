using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneSwitcher.Editor
{
    /// <summary>
    /// Provides keyboard shortcuts for quick scene switching.
    /// Ctrl+1 through Ctrl+9 loads scenes from Build Settings.
    /// Ctrl+Shift+O opens the Scene Switcher window.
    /// </summary>
    [InitializeOnLoad]
    public static class SceneSwitcherShortcuts
    {
        static SceneSwitcherShortcuts()
        {
            // The main shortcut (Ctrl+Shift+O) is already defined in SceneSwitcherWindow
            // This class adds additional shortcuts for quick scene loading
        }
        
        // Quick load build scene shortcuts (Ctrl+1 through Ctrl+9)
        
        [MenuItem("Tools/Scene Switcher/Load Build Scene 1 %1", priority = 200)]
        private static void LoadBuildScene1() => LoadBuildSceneAtIndex(0);
        
        [MenuItem("Tools/Scene Switcher/Load Build Scene 2 %2", priority = 201)]
        private static void LoadBuildScene2() => LoadBuildSceneAtIndex(1);
        
        [MenuItem("Tools/Scene Switcher/Load Build Scene 3 %3", priority = 202)]
        private static void LoadBuildScene3() => LoadBuildSceneAtIndex(2);
        
        [MenuItem("Tools/Scene Switcher/Load Build Scene 4 %4", priority = 203)]
        private static void LoadBuildScene4() => LoadBuildSceneAtIndex(3);
        
        [MenuItem("Tools/Scene Switcher/Load Build Scene 5 %5", priority = 204)]
        private static void LoadBuildScene5() => LoadBuildSceneAtIndex(4);
        
        [MenuItem("Tools/Scene Switcher/Load Build Scene 6 %6", priority = 205)]
        private static void LoadBuildScene6() => LoadBuildSceneAtIndex(5);
        
        [MenuItem("Tools/Scene Switcher/Load Build Scene 7 %7", priority = 206)]
        private static void LoadBuildScene7() => LoadBuildSceneAtIndex(6);
        
        [MenuItem("Tools/Scene Switcher/Load Build Scene 8 %8", priority = 207)]
        private static void LoadBuildScene8() => LoadBuildSceneAtIndex(7);
        
        [MenuItem("Tools/Scene Switcher/Load Build Scene 9 %9", priority = 208)]
        private static void LoadBuildScene9() => LoadBuildSceneAtIndex(8);
        
        // Validation methods to enable/disable menu items
        
        [MenuItem("Tools/Scene Switcher/Load Build Scene 1 %1", true)]
        private static bool ValidateBuildScene1() => ValidateBuildSceneAtIndex(0);
        
        [MenuItem("Tools/Scene Switcher/Load Build Scene 2 %2", true)]
        private static bool ValidateBuildScene2() => ValidateBuildSceneAtIndex(1);
        
        [MenuItem("Tools/Scene Switcher/Load Build Scene 3 %3", true)]
        private static bool ValidateBuildScene3() => ValidateBuildSceneAtIndex(2);
        
        [MenuItem("Tools/Scene Switcher/Load Build Scene 4 %4", true)]
        private static bool ValidateBuildScene4() => ValidateBuildSceneAtIndex(3);
        
        [MenuItem("Tools/Scene Switcher/Load Build Scene 5 %5", true)]
        private static bool ValidateBuildScene5() => ValidateBuildSceneAtIndex(4);
        
        [MenuItem("Tools/Scene Switcher/Load Build Scene 6 %6", true)]
        private static bool ValidateBuildScene6() => ValidateBuildSceneAtIndex(5);
        
        [MenuItem("Tools/Scene Switcher/Load Build Scene 7 %7", true)]
        private static bool ValidateBuildScene7() => ValidateBuildSceneAtIndex(6);
        
        [MenuItem("Tools/Scene Switcher/Load Build Scene 8 %8", true)]
        private static bool ValidateBuildScene8() => ValidateBuildSceneAtIndex(7);
        
        [MenuItem("Tools/Scene Switcher/Load Build Scene 9 %9", true)]
        private static bool ValidateBuildScene9() => ValidateBuildSceneAtIndex(8);
        
        private static bool ValidateBuildSceneAtIndex(int index)
        {
            var scenes = EditorBuildSettings.scenes;
            return index < scenes.Length && scenes[index].enabled;
        }
        
        private static void LoadBuildSceneAtIndex(int index)
        {
            var scenes = EditorBuildSettings.scenes;
            
            if (index >= scenes.Length)
            {
                Debug.LogWarning($"[Scene Switcher] No scene at build index {index}");
                return;
            }
            
            var scenePath = scenes[index].path;
            
            if (!scenes[index].enabled)
            {
                Debug.LogWarning($"[Scene Switcher] Scene at index {index} is disabled in build settings");
                return;
            }
            
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
            
            EditorSceneManager.OpenScene(scenePath);
            
            var sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"[Scene Switcher] Loaded scene: {sceneName} (Build Index: {index})");
        }
        
        // Additional utility shortcuts
        
        [MenuItem("Tools/Scene Switcher/Save Current Scene %&s", priority = 300)]
        private static void SaveCurrentScene()
        {
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Scene Switcher] Saved all open scenes");
        }
        
        [MenuItem("Tools/Scene Switcher/Reload Current Scene %&r", priority = 301)]
        private static void ReloadCurrentScene()
        {
            var currentPath = SceneManager.GetActiveScene().path;
            
            if (string.IsNullOrEmpty(currentPath))
            {
                Debug.LogWarning("[Scene Switcher] Cannot reload untitled scene");
                return;
            }
            
            if (SceneManager.GetActiveScene().isDirty)
            {
                if (!EditorUtility.DisplayDialog(
                    "Reload Scene",
                    "You have unsaved changes. Are you sure you want to reload?",
                    "Reload",
                    "Cancel"))
                {
                    return;
                }
            }
            
            EditorSceneManager.OpenScene(currentPath);
            Debug.Log("[Scene Switcher] Reloaded current scene");
        }
    }
}
