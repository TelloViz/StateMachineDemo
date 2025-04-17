using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace StateMachine.Animation
{
    public class SpriteAnimator
    {
        private BitmapImage _spritesheet;
        private readonly List<Int32Rect> _frames = new List<Int32Rect>();
        private readonly DispatcherTimer _animationTimer;
        private int _currentFrameIndex = 0;
        private bool _isPlaying = false;

        public event Action<ImageSource> OnFrameChanged;
        
        public int FrameCount => _frames.Count;
        public ImageSource CurrentFrame { get; private set; }
        public TimeSpan FrameDelay { get; set; } = TimeSpan.FromMilliseconds(100);
        
        public SpriteAnimator(string spritesheetPath)
        {
            _spritesheet = new BitmapImage(new Uri(spritesheetPath, UriKind.RelativeOrAbsolute));
            _animationTimer = new DispatcherTimer
            {
                Interval = FrameDelay
            };
            _animationTimer.Tick += AnimationTick;
        }

        public void AddFrame(int x, int y, int width, int height)
        {
            _frames.Add(new Int32Rect(x, y, width, height));
            
            // If this is the first frame, set it as current
            if (_frames.Count == 1)
            {
                UpdateCurrentFrame();
            }
        }

        public void AddFrames(IEnumerable<(int x, int y, int width, int height)> frameRects)
        {
            foreach (var rect in frameRects)
            {
                AddFrame(rect.x, rect.y, rect.width, rect.height);
            }
        }

        public void ClearFrames()
        {
            Stop();
            _frames.Clear();
            _currentFrameIndex = 0;
        }

        public void Play()
        {
            if (_frames.Count == 0)
                return;
                
            _isPlaying = true;
            _animationTimer.Interval = FrameDelay;
            _animationTimer.Start();
        }

        public void Stop()
        {
            _isPlaying = false;
            _animationTimer.Stop();
        }

        public void Reset()
        {
            _currentFrameIndex = 0;
            UpdateCurrentFrame();
        }

        public void ChangeSpritesheet(string spritesheetPath)
        {
            // Stop any current animation
            Stop();
            
            // Load the new spritesheet
            _spritesheet = new BitmapImage(new Uri(spritesheetPath, UriKind.RelativeOrAbsolute));
            
            // Reset the animation
            _currentFrameIndex = 0;
            
            // Update the current frame with the new spritesheet
            if (_frames.Count > 0)
            {
                UpdateCurrentFrame();
            }
        }

        private void AnimationTick(object sender, EventArgs e)
        {
            if (_frames.Count == 0)
                return;
                
            _currentFrameIndex = (_currentFrameIndex + 1) % _frames.Count;
            UpdateCurrentFrame();
        }

        private void UpdateCurrentFrame()
        {
            if (_frames.Count == 0)
                return;
                
            var rect = _frames[_currentFrameIndex];
            var croppedBitmap = new CroppedBitmap(_spritesheet, rect);
            CurrentFrame = croppedBitmap;
            OnFrameChanged?.Invoke(CurrentFrame);
        }
    }
}
