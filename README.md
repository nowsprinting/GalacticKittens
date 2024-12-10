# **Galactic Kittens** Autoplay using Anjin

This fork is a modified version of the [GalacticKittens](https://github.com/UnityTechnologies/GalacticKittens) project,



## Diff with upstream

- Upgrade Unity to 6000.0.23f1
- Install [Multiplayer Play Mode](https://docs.unity3d.com/Packages/com.unity.multiplayer.playmode@latest) (MPPM) package v1.3.2
- Install [Anjin](https://github.com/DeNA/Anjin) package v1.8.1
- Install [Input Test Helper](https://github.com/nowsprinting/test-helper.input) package v1.0.1
- Input Manager can be DI enabled
  - Assets/Scripts/Managers/MenuManager.cs
  - Assets/Scripts/Player/PlayerShipMovement.cs
  - Assets/Scripts/Player/PlayerShipShootBullet.cs
- Implements autoplay under Assets/Autopilot folder



## MPPM settings

Open **Window > Multiplayer > Multiplayer Play Mode** window and set the following tags.

- Main Editor: Add tag `host`
- Virtual Players: Add tag `join`



## Autoplay scenario

In Anjin, the Agent is responsible for the operation for each scene.

### Menu: MenuAgent

Menu scene is two states.

1. PRESS ANY KEY TO START: Use stub Input to make the situation any key pressed, proceed to the next step.
2. Select HOST or JOIN: Depending on the MPPM tag assigned to the Virtual Player, click "HOST" or "JOIN" button.

### CharacterSelection: UGUIPlaybackAgent (built-in)

Click "READY" button.

### Gameplay: GameplayAgent

Shoots bullets while moving up and down randomly using stub Input.

### Defeat and Victory: UGUIPlaybackAgent (built-in)

Click "MENU" button.



## How to run autoplay

1. Open Assets/Autopilot/Settings/AutopilotSettings by inspector window.
2. Click "Run" button.
