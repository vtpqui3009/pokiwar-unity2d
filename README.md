# Pokiwar - Unity 2D Multiplayer Game

A Pokémon-style evolution game where players start as small creatures and evolve by collecting food and defeating foes.

## Features

- **Real-time Multiplayer** - Unity Netcode for GameObjects
- **Evolution System** - 4000+ unique sprite evolutions
- **Combat** - Level-based attack mechanics
- **Responsive Controls** - WASD/Arrows + Mobile touch
- **UI** - Level bar, minimap, leaderboard

## Tech Stack

- **Engine**: Unity 2022.3+ LTS (2D URP)
- **Language**: C#
- **Multiplayer**: Unity Netcode for GameObjects
- **Input**: Unity Input System

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/              # Game manager, player controller, data
│   ├── Evolution/         # Evolution system, food spawning
│   ├── Combat/            # Health, combat mechanics
│   ├── Multiplayer/       # Network components
│   ├── UI/                # UI scripts (level bar, minimap, leaderboard)
│   └── Editor/            # Editor utilities
├── Sprites/
│   └── Pets/              # Imported pet sprites
├── Prefabs/               # Player, Food prefabs
└── Scenes/                # Game scenes
```

## Setup Instructions

### 1. Create Unity Project

1. Open Unity Hub
2. Create New Project → **2D (URP)**
3. Name: `Pokiwar-Unity2D`
4. Copy this folder contents into the project

### 2. Import Sprites

1. Open Unity
2. Go to `Window → Pokiwar Sprite Importer`
3. Click **Import All** to import breath-images and large-images
4. Click **Create Sprite Database** to map sprites to levels

### 3. Create Prefabs

1. Go to `Assets → Create → Pokiwar → Create Player Prefab`
2. Go to `Assets → Create → Pokiwar → Create Food Prefab`

### 4. Scene Setup

1. Create new scene: `Assets/Scenes/Game.unity`
2. Add **GameManager** (empty GameObject with `GameManager.cs`)
3. Add **FoodSpawner** (empty GameObject with `FoodSpawner.cs`)
4. Add **CombatManager** (empty GameObject with `CombatManager.cs`)
5. Add **NetworkManager** (Unity Netcode GameObject)
6. Add UI Canvas with:
   - LevelBarUI
   - LeaderboardUI
   - MinimapUI
   - MobileControls (for mobile builds)

### 5. Configure GameManager

In Inspector:
- Map Width: 100
- Map Height: 100
- Max Players: 20
- Player Prefab: Assign `Player.prefab`

### 6. Configure NetworkManager

1. Add `Unity.Netcode.NetworkManager` component
2. Add `Pokiwar.Multiplayer.NetworkManager` script
3. Assign Player Prefab
4. Set Transport to Unity Transport

## How to Play

### Desktop
- **WASD / Arrow Keys**: Move
- **Objective**: Collect food → Level up → Defeat smaller players

### Mobile
- **On-screen Joystick**: Move
- **Boost Button**: Speed boost

### Combat Rules
- Players **5+ levels higher** can defeat you
- You can defeat players **5+ levels lower**
- Defeating grants **25 XP**
- Death respawns you at half level

## Building

### Desktop (Windows/Mac/Linux)
1. `File → Build Settings`
2. Add `Game.unity` scene
3. Select platform (PC, Mac, Linux)
4. Click **Build**

### WebGL (Browser)
1. `File → Build Settings → WebGL`
2. Switch Platform
3. Build

### Mobile
1. Install required modules (iOS/Android)
2. `File → Build Settings → iOS/Android`
3. Configure mobile settings
4. Build

## Script Overview

| Script | Purpose |
|--------|---------|
| `GameManager.cs` | Singleton game state, map bounds |
| `PlayerController.cs` | WASD/Arrow movement, physics |
| `EvolutionManager.cs` | Level up, sprite changes, XP |
| `HealthController.cs` | HP, death, respawn |
| `PlayerNetwork.cs` | Multiplayer sync |
| `FoodSpawner.cs` | Spawns food across map |

## Multiplayer Setup

### Host a Game
1. Run game
2. Click **Start Host**
3. Share IP with friends

### Join a Game
1. Run game
2. Click **Start Client**
3. Enter host IP

## Troubleshooting

### Sprites not showing
- Run `Pokiwar Sprite Importer` window
- Ensure sprite assets are in `Assets/Sprites/Pets/`

### Multiplayer not connecting
- Check firewall settings
- Verify NetworkManager transport settings
- Ensure both players use same Unity version

### Players not moving
- Check PlayerController has Rigidbody2D
- Verify input system package installed
- Check console for errors

## Asset Credits

Pet sprites sourced from original Pokiwar Collect n Evolve game.

## License

This is a fan project for educational purposes.

---

**Status**: Core implementation complete. Ready for Unity project setup and testing.
