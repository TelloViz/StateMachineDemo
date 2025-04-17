using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace StateMachine.Animation
{
    public static class SpritesheetHelper
    {
        // Character selection options
        public enum Character
        {
            Mario,
            Luigi
        }
        
        private static Character _currentCharacter = Character.Mario;
        
        public static Character CurrentCharacter => _currentCharacter;
        
        public static void SetCharacter(Character character)
        {
            _currentCharacter = character;
        }
        
        // Path to the spritesheet (relative to executable)
        public static string GetSpritesheetPath()
        {
            return _currentCharacter == Character.Mario 
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Mario3.png")
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "LuigiSmall.png");
        }

        // Define frames for idle animation (standing)
        public static void ConfigureIdleAnimation(SpriteAnimator animator)
        {
            animator.ClearFrames();
            
            // Using a single frame for idle (assuming it's near the walking frames)
            animator.AddFrames(new List<(int x, int y, int width, int height)>
            {
                (0, 0, 48, 48)
            });
            
            animator.FrameDelay = TimeSpan.FromMilliseconds(500);
        }

        public static void ConfigureLookUpAnimation(SpriteAnimator animator)
        {
            animator.ClearFrames();
            
            animator.AddFrames(new List<(int x, int y, int width, int height)>
            {
                (52, 0, 48, 48),
                (52, 0, 48, 48),
                (52, 0, 48, 48),
                (52, 0, 48, 48)
            });
            
            animator.FrameDelay = TimeSpan.FromMilliseconds(500);
        }

        public static void ConfigureDuckingAnimation(SpriteAnimator animator)
        {
            animator.ClearFrames();
            
            animator.AddFrames(new List<(int x, int y, int width, int height)>
            {
                (104, 0, 48, 48),
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
                
                (156, 0, 48, 48),  // Frame 2
                (208, 0, 48, 48),   // Frame 3
                (260, 0, 48, 48)  // Frame 1
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
                (52*6, 0, 48, 48),  // Frame 1
                (52*7, 0, 48, 48),  // Frame 2
                (52*8, 0, 48, 48)   // Frame 3
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
