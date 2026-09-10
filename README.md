# ItzGame's TimeChanger

A BepInEx plugin for Gorilla Tag that lets you control the time of day with a full-featured in-game menu.

## Features

### Time Control
Choose from 4 presets — Morning, Day, Noon, and Night — to change the time of day in any lobby. Your selection persists across sessions.

### Weather
Toggle rain on and off from the menu. Your choice is saved and reapplied automatically.

### Themes
Pick from 9 built-in color themes to customize the look of the menu:
Ultraviolet, Crimson, Toxic, ItzGame, Magma, Blaze, Frostbite, Sakura, and Snow.

### Playtime Notch
A sleek pill-shaped notch at the top of the screen shows your session playtime. It slides in and out with a smooth animation when toggled. You can switch between two modes:
- **Playtime** — Displays your current session time (HH:MM:SS)
- **FPS** — Displays your live frames per second

The notch has its own scale slider so you can size it independently from the menu.

### WASD Fly
Fly around the map using your keyboard:
| Key | Action |
|-----|--------|
| W / A / S / D | Move forward / left / back / right |
| Space | Fly up |
| Ctrl | Fly down |
| Shift | 2x speed |
| Alt | Half speed |
| Arrow Keys | Turn camera left/right |
| Right Mouse Hold | Free look |

When no keys are pressed, you stay in place (stationary mode). Fly speed is adjustable from 1 to 60.

### Discord Rich Presence
Shows your current status on your Discord profile:
- Queue name and map you're playing
- Party size and player count
- A "Join Our Discord" button that links to https://discord.gg/itzgame

### Customizable Menu
- **Menu Scale** — Resize the entire menu (0.5x to 2x)
- **Menu Rounding** — Adjust corner rounding (0 to 20)
- **Notch Scale** — Independently resize the playtime/FPS notch (0.5x to 2x)

### Sound Effects
- `destiny.ogg` plays when opening or closing the menu
- `bark.ogg` plays when pressing buttons

Sounds are embedded in the DLL — no extra files needed.

## Controls
Press **F1** to open or close the menu.

## Installation
1. Install [BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html) for Gorilla Tag
2. Download `TimeChangerMod.dll` from the releases page
3. Place it in `Gorilla Tag/BepInEx/plugins/`
4. Launch the game

## Requirements
- BepInEx 5.x
- Gorilla Tag (Steam)

## Links
- [Discord](https://discord.gg/itzgame)
https://discord.com/channels/1536853966741180476/1536855668600864789/1547593206408020018
