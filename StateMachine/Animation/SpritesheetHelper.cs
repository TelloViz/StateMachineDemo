using System;
using System.Collections.Generic;
using System.IO;

namespace StateMachine.Animation
{
    public static class SpritesheetHelper
    {
        // Path to the spritesheet (relative to executable)
        public static string GetSpritesheetPath()
        {
            // Assuming the spritesheet is in the same directory as the executable
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mario2.png");
        }

        // Define frames for idle animation (standing)
        public static void ConfigureIdleAnimation(SpriteAnimator animator)
        {
            animator.ClearFrames();
            
            // Using a single frame for idle (assuming it's near the walking frames)
            animator.AddFrames(new List<(int x, int y, int width, int height)>
            {
                (8, 33, 48, 48),
                (8, 33, 48, 48),
                (8, 33, 48, 48),
                (8, 33, 48, 48),
                (8, 33, 48, 48),
                (8, 33, 48, 48),
                (8, 33, 48, 48),
                (8, 33, 48, 48),
                (8, 33, 48, 48),
                (8, 33, 48, 48),
                (8, 33, 48, 48),
                (8, 33, 48, 48),
                (8, 33, 48, 48),
                (8, 33, 48, 48),
                (8, 33, 48, 48),
                (8, 33, 48, 48),
                (8, 33, 48, 48),
                (8, 33, 48, 48),
                (8, 33, 48, 48),
                (8+52, 33, 48, 48)
            });
            
            animator.FrameDelay = TimeSpan.FromMilliseconds(500);
        }

        // Define frames for walking animation using the exact coordinates provided
        public static void ConfigureWalkingAnimation(SpriteAnimator animator)
        {
            animator.ClearFrames();
            
            // User-provided coordinates for walking frames (48x48 pixels each)
            animator.AddFrames(new List<(int x, int y, int width, int height)>
            {
                (164, 33, 48, 48),  // Frame 1
                (216, 33, 48, 48),  // Frame 2
                (268, 33, 48, 48)   // Frame 3
            });
            
            animator.FrameDelay = TimeSpan.FromMilliseconds(150);
        }

        // Define frames for running animation (faster version of walking for now)
        public static void ConfigureRunningAnimation(SpriteAnimator animator)
        {
            animator.ClearFrames();
            
            // Using the same frames as walking but with faster animation
            animator.AddFrames(new List<(int x, int y, int width, int height)>
            {
                (268+52, 33, 48, 48),  // Frame 1
                (268+52+52, 33, 48, 48),  // Frame 2
                (268+52+52+52, 33, 48, 48)   // Frame 3
            });
            
            // Running is faster than walking
            animator.FrameDelay = TimeSpan.FromMilliseconds(80);
        }

        // Helper method to save the spritesheet to the executable directory
        public static void SaveSpritesheet(byte[] spritesheetData)
        {
            string path = GetSpritesheetPath();
            
            // Only save if the file doesn't exist
            if (!File.Exists(path))
            {
                try
                {
                    File.WriteAllBytes(path, spritesheetData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to save spritesheet: {ex.Message}");
                }
            }
        }
    }
}
