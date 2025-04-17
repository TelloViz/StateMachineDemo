# State Machine Demo with Sprite Animation

This application demonstrates a simple state machine with animated sprites.

## Spritesheet Setup

For the sprite animations to work:

1. Copy the Mario spritesheet to the application's output directory. Typically:
   `bin/Debug/net9.0-windows/mario_spritesheet.png`

2. If the spritesheet is not found, you'll be prompted with the exact location where it should be placed.

## Sprite Animation Configuration

The sprite animations are currently configured with approximate values. You may need to adjust the sprite 
frame coordinates in `SpritesheetHelper.cs` to match the exact positions in your spritesheet.

For each state (Idle, Walking, Running), the following methods define the animation frames:
- `ConfigureIdleAnimation`
- `ConfigureWalkingAnimation`
- `ConfigureRunningAnimation`

## Customizing Animations

To adjust frame coordinates, modify the `AddFrame` or `AddFrames` calls in the corresponding configuration 
methods in `SpritesheetHelper.cs`. Each frame is defined by:
- `x`: Left coordinate of the sprite frame in the spritesheet
- `y`: Top coordinate of the sprite frame in the spritesheet
- `width`: Width of the sprite frame
- `height`: Height of the sprite frame

Example:
```csharp
animator.AddFrame(14, 39, 16, 16);  // x=14, y=39, width=16, height=16
```

You may need to experiment with different coordinates to find the exact position of each sprite frame in your spritesheet.
