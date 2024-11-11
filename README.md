# **Galactic Kittens** Autoplay by Anjin

This fork is a modified version of the [GalacticKittens](https://github.com/UnityTechnologies/GalacticKittens) project,



## Diff with upstream

- Upgrade Unity to 6000.0.23f1
- Install [Multiplayer Play Mode](https://docs.unity3d.com/Packages/com.unity.multiplayer.playmode@latest) package v1.3.1
- Install [Anjin](https://github.com/DeNA/Anjin) package v1.7.0
- Install [Input Test Helper](https://github.com/nowsprinting/test-helper.input) package v1.0.1
- Input manager can be DI enabled
  - Assets/Scripts/Managers/MenuManager.cs
  - Assets/Scripts/Player/PlayerShipMovement.cs
  - Assets/Scripts/Player/PlayerShipShootBullet.cs
- Implements autoplay under Assets/Autopilot folder



## Autopilot scenario

In Anjin, the Agent is responsible for the operation for each scene.

### Menu: MenuAgent

Menu scene is two states.

1. PRESS ANY KEY TO START: Use stub Input to make the situation any key pressed, proceed to the next step.
2. Select Host or Join: Depending on the tag assigned to the Virtual Player, click "Host" or "Join" button.

### CharacterSelection: UGUIPlaybackAgent

Click "READY" button.

### Gameplay: GameplayAgent

Shoots bullets while moving up and down randomly using stub Input.

### Defeat and Victory: UGUIPlaybackAgent

Click "MENU" button.



## How to run autopilot

1. Open Assets/Autopilot/Settings/AutopilotSettings by inspector window.
2. Click "Run" button.
