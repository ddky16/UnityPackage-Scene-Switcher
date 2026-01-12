#if UNITY_2021_2_OR_NEWER
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace SceneSwitcher.Editor
{
    /// <summary>
    /// Unity 2021.2+ Overlay that adds scene switching to the Scene View toolbar.
    /// This integrates directly with Unity's modern overlay system.
    /// </summary>
    [Overlay(typeof(SceneView), "Scene Switcher", true)]
    [Icon("d_SceneAsset Icon")]
    public class SceneSwitcherOverlay : ToolbarOverlay
    {
        public SceneSwitcherOverlay() : base(
            SceneSwitcherDropdown.Id,
            SceneSwitcherOpenWindowButton.Id
        )
        { }
    }
    
    /// <summary>
    /// Toolbar dropdown for scene selection.
    /// </summary>
    [EditorToolbarElement(Id, typeof(SceneView))]
    public class SceneSwitcherDropdown : EditorToolbarDropdown
    {
        public const string Id = "SceneSwitcher/SceneDropdown";
        
        private string _currentSceneName = "Select Scene";
        
        public SceneSwitcherDropdown()
        {
            text = GetCurrentSceneName();
            tooltip = "Click to switch scenes";
            
            clicked += ShowSceneMenu;
            
            // Update when scene changes
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += (scene, mode) =>
            {
                text = GetCurrentSceneName();
            };
        }
        
        private string GetCurrentSceneName()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            return string.IsNullOrEmpty(scene.name) ? "Untitled" : scene.name;
        }
        
        private void ShowSceneMenu()
        {
            var menu = new GenericMenu();
            
            var sceneGuids = AssetDatabase.FindAssets("t:SceneAsset");
            var buildScenes = EditorBuildSettings.scenes;
            var currentPath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            
            // Add build scenes first
            menu.AddDisabledItem(new GUIContent("--- Build Scenes ---"));
            
            for (int i = 0; i < buildScenes.Length; i++)
            {
                var scenePath = buildScenes[i].path;
                var sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                var isCurrentScene = scenePath == currentPath;
                
                menu.AddItem(
                    new GUIContent($"[{i}] {sceneName}"),
                    isCurrentScene,
                    () => SceneSwitcherToolbar.LoadScene(scenePath)
                );
            }
            
            menu.AddSeparator("");
            menu.AddDisabledItem(new GUIContent("--- All Scenes ---"));
            
            // Add all other scenes
            foreach (var guid in sceneGuids)
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(guid);
                var sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                var isCurrentScene = scenePath == currentPath;
                
                // Skip if already in build scenes
                bool isInBuild = false;
                foreach (var bs in buildScenes)
                {
                    if (bs.path == scenePath)
                    {
                        isInBuild = true;
                        break;
                    }
                }
                
                if (!isInBuild)
                {
                    menu.AddItem(
                        new GUIContent(sceneName),
                        isCurrentScene,
                        () => SceneSwitcherToolbar.LoadScene(scenePath)
                    );
                }
            }
            
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Open Scene Switcher Window"), false, SceneSwitcherWindow.ShowWindow);
            
            menu.ShowAsContext();
        }
    }
    
    /// <summary>
    /// Button to open the full Scene Switcher window.
    /// </summary>
    [EditorToolbarElement(Id, typeof(SceneView))]
    public class SceneSwitcherOpenWindowButton : EditorToolbarButton
    {
        public const string Id = "SceneSwitcher/OpenWindowButton";
        
        public SceneSwitcherOpenWindowButton()
        {
            text = "";
            icon = EditorGUIUtility.IconContent("d_winbtn_win_max").image as Texture2D;
            tooltip = "Open Scene Switcher Window (Ctrl+Shift+O)";
            clicked += SceneSwitcherWindow.ShowWindow;
        }
    }
}
#endif
