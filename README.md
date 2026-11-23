# Xuixos2026

A 2D pastry competition game made with Unity `6000.3.24f1`.

## Editing the game

- Open `Assets/Scenes/00_Intro.unity` and enter Play Mode for a full run.
  The **GameManager** root in that scene holds the point limits and ending
  thresholds. It persists between scenes and resets when a new run starts.
- Open a minigame scene directly to test its controls and presentation. For
  end-to-end scoring, start from the intro. The frying scene has an Inspector
  field for the earlier-stage score to use when testing it on its own.
- Shared UI prefabs are in `Assets/Content/Shared/UI/Prefabs`. Edit the prefab for changes across scenes, or
  edit an instance for scene-specific changes. The instruction text is on each
  scene's `InstruccionsUI` instance.
- Local HUD elements, fryer bubbles, and bowl audio are authored in their scenes.
  The quit panel, rolling result panel, and credits click areas start hidden;
  select and temporarily enable them in the Editor to preview their layout.

## Script layout

`Assets/Scripts` contains a folder matching each numbered scene in `Assets/Scenes`.
`Shared/Core` contains run state and transitions,
`Shared/UI` contains reused UI components, and `Editor` contains Unity editor tooling.

## Content layout

`Assets/Content` groups scene-specific art, audio, prefabs, and data under the
matching numbered scene folders. `Shared` holds reused artwork, audio, and UI;
`Archive` keeps source art and alternate variants with no direct scene use.
`Resources`, `StreamingAssets`, and TextMesh Pro package assets keep their
special Unity paths. The credits video is in `StreamingAssets/Videos`.
