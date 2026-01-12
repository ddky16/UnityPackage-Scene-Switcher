# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-01-12

### Added
- Initial release
- Scene Switcher EditorWindow with tabbed interface (All Scenes, Build Scenes, Favorites, Recent)
- Searchable scene list with real-time filtering
- Favorites system with star toggle
- Recent scenes tracking (last 10 scenes)
- Context menu with additional options:
  - Open Scene
  - Open Additive
  - Add to/Remove from Favorites
  - Add to/Remove from Build Settings
  - Ping in Project
  - Show in Explorer
  - Copy Path
- Keyboard shortcuts:
  - Ctrl+Shift+O to open Scene Switcher
  - Ctrl+1 through Ctrl+9 for quick build scene loading
  - Ctrl+Alt+S to save all scenes
  - Ctrl+Alt+R to reload current scene
- Scene View Overlay for Unity 2021.2+ with dropdown in toolbar
- SceneSwitcherToolbar API for custom editor integrations
- SceneSwitcherSettings ScriptableObject for team-shared configuration
- Safe scene switching with unsaved changes prompt
- Assembly definition for proper compilation isolation
- Full Unity Package Manager support
