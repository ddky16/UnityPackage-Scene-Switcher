# Scene Switcher for Unity

A powerful editor tool for quick scene switching in Unity. Stop digging through your project folder to find scenes - just use the dropdown!

## Features

- **Quick Scene Dropdown**: Access all scenes in your project from a searchable dropdown
- **Tabbed Interface**: View All Scenes, Build Scenes only, Favorites, or Recent scenes
- **Favorites System**: Star your frequently used scenes for quick access
- **Recent Scenes**: Automatically tracks your recently opened scenes
- **Keyboard Shortcuts**: 
  - `Ctrl+Shift+O` - Open Scene Switcher window
  - `Ctrl+1` through `Ctrl+9` - Quick load build scenes by index
  - `Ctrl+Alt+S` - Save all open scenes
  - `Ctrl+Alt+R` - Reload current scene
- **Scene View Overlay** (Unity 2021.2+): Dropdown directly in the Scene View toolbar
- **Context Menu**: Right-click for additional options like "Open Additive", "Add to Build Settings", "Ping in Project"
- **Safe Scene Switching**: Prompts to save unsaved changes before switching

## Installation

Via Unity Package Manager:
1. Open Package Manager (Window > Package Manager)
2. Click + → Add package from git URL...
3. Enter: https://github.com/ddky16/UnityPackage-Scene-Switcher.git

## Usage

### Opening the Scene Switcher Window

- **Menu**: `Tools > Scene Switcher > Open Scene Switcher`
- **Shortcut**: `Ctrl+Shift+O` (Windows) / `Cmd+Shift+O` (Mac)

### Using the Window

1. **Tabs**:
   - **All Scenes**: Shows every `.unity` file in your project
   - **Build Scenes**: Shows only scenes added to Build Settings
   - **Favorites**: Your starred scenes
   - **Recent**: Recently opened scenes

2. **Search**: Type in the search field to filter scenes by name or path

3. **Scene Entry**:
   - Click the **star (☆/★)** to add/remove from favorites
   - Click the **scene name** to load it
   - Click the **menu (⋮)** for additional options

4. **Context Menu Options**:
   - Open Scene
   - Open Additive (load alongside current scene)
   - Add to/Remove from Favorites
   - Add to/Remove from Build Settings
   - Ping in Project (highlight in Project window)
   - Show in Explorer
   - Copy Path

### Quick Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+Shift+O` | Open Scene Switcher window |
| `Ctrl+1` to `Ctrl+9` | Load build scene by index |
| `Ctrl+Alt+S` | Save all open scenes |
| `Ctrl+Alt+R` | Reload current scene |

### Scene View Overlay (Unity 2021.2+)

In Unity 2021.2 and later, you'll see a "Scene Switcher" overlay in the Scene View:

1. If not visible, click the **Overlays** menu (hamburger icon) in Scene View
2. Enable "Scene Switcher"
3. Click the dropdown to switch scenes directly from Scene View

## API Usage

You can also use the Scene Switcher programmatically:

```csharp
using SceneSwitcher.Editor;

// Load a scene by path
SceneSwitcherToolbar.LoadScene("Assets/Scenes/MainMenu.unity");

// Get all scene paths
string[] allScenes = SceneSwitcherToolbar.GetAllScenePaths();

// Get all scene display names
string[] sceneNames = SceneSwitcherToolbar.GetAllSceneNames();

// Draw the dropdown in your custom editor window
SceneSwitcherToolbar.DrawSceneDropdown(200f);

// Draw compact dropdown with refresh button
SceneSwitcherToolbar.DrawSceneDropdownCompact();
```

## Settings (Optional)

Create a settings asset for team-shared configuration:

1. Right-click in Project window
2. Select `Create > Scene Switcher > Settings`
3. Configure options like auto-save, max recent scenes, quick access scenes, etc.

## Compatibility

- **Unity Version**: 2020.3 LTS and later
- **Platforms**: All platforms (Editor only - no runtime impact)
- **Render Pipelines**: Works with Built-in, URP, and HDRP

## Troubleshooting

### Scenes not appearing in the list
- Click the **Refresh** button in the toolbar
- Ensure your scenes have the `.unity` extension
- Check that scenes are inside the `Assets` folder

### Keyboard shortcuts not working
- Check for conflicts with other packages or tools
- Shortcuts may differ on Mac (use `Cmd` instead of `Ctrl`)

### Overlay not showing (Unity 2021.2+)
- Open Scene View
- Click the Overlays menu (hamburger icon in top-left)
- Enable "Scene Switcher"

## License

MIT License - Feel free to use in personal and commercial projects.

## Contributing

Contributions welcome! Feel free to submit issues and pull requests.
