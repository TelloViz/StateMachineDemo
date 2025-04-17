using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Vast.StateMachine;

namespace StateMachine
{
    public class StateMachineViewModel : INotifyPropertyChanged
    {
        private readonly Vast.StateMachine.StateMachine _stateMachine;
        private readonly DispatcherTimer _updateTimer;
        private string _currentStateName = "None";
        private string _logText = string.Empty;
        private readonly SpriteAnimator _spriteAnimator;
        private ImageSource _currentSpriteFrame;

        public event PropertyChangedEventHandler PropertyChanged;

        public string CurrentStateName
        {
            get => _currentStateName;
            private set
            {
                if (_currentStateName != value)
                {
                    _currentStateName = value;
                    OnPropertyChanged();
                    UpdateAnimationForState();
                }
            }
        }

        public string LogText
        {
            get => _logText;
            private set
            {
                _logText = value;
                OnPropertyChanged();
            }
        }

        public ImageSource CurrentSpriteFrame
        {
            get => _currentSpriteFrame;
            private set
            {
                _currentSpriteFrame = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> AvailableStates { get; } = new ObservableCollection<string>();

        public ICommand ChangeStateCommand { get; }
        public ICommand ClearLogCommand { get; }
        
        public StateMachineViewModel()
        {
            // Initialize sprite animator
            try
            {
                _spriteAnimator = new SpriteAnimator(SpritesheetHelper.GetSpritesheetPath());
                _spriteAnimator.OnFrameChanged += frame => CurrentSpriteFrame = frame;
            }
            catch (Exception ex)
            {
                AddLogMessage($"Failed to initialize sprite animator: {ex.Message}");
            }
            
            // Initialize state machine
            _stateMachine = new Vast.StateMachine.StateMachine();
            _stateMachine.OnStateChange = state => 
            {
                CurrentStateName = state?.Name ?? "None";
                AddLogMessage($"State changed to: {CurrentStateName}");
            };

            // Create and add states
            var idleState = new IdleState(AddLogMessage);
            var walkingState = new WalkingState(AddLogMessage);
            var runningState = new RunningState(AddLogMessage);
            
            _stateMachine.AddStates(idleState, walkingState, runningState);
            
            // Initialize available states collection
            foreach (var state in _stateMachine.States)
            {
                AvailableStates.Add(state.Name);
            }

            // Create commands
            ChangeStateCommand = new RelayCommand<string>(ChangeState);
            ClearLogCommand = new RelayCommand<object>(_ => ClearLog());
            
            // Setup update timer (simulates game loop)
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _updateTimer.Tick += (s, e) => _stateMachine.UpdateActiveState();
            _updateTimer.Start();
            
            // Set initial state
            if (_stateMachine.States.Count > 0)
            {
                _stateMachine.ChangeState(_stateMachine.States[0]);
            }
        }

        private void ChangeState(string stateName)
        {
            if (!string.IsNullOrEmpty(stateName))
            {
                _stateMachine.ChangeState(stateName);
            }
        }

        private void AddLogMessage(string message)
        {
            LogText = $"{DateTime.Now:HH:mm:ss.fff}: {message}\n{LogText}";
        }

        private void ClearLog()
        {
            LogText = string.Empty;
            AddLogMessage("Log cleared");
        }

        private void UpdateAnimationForState()
        {
            if (_spriteAnimator == null)
                return;

            _spriteAnimator.Stop();
            
            switch (CurrentStateName)
            {
                case "Idle":
                    SpritesheetHelper.ConfigureIdleAnimation(_spriteAnimator);
                    break;
                case "Walking":
                    SpritesheetHelper.ConfigureWalkingAnimation(_spriteAnimator);
                    break;
                case "Running":
                    SpritesheetHelper.ConfigureRunningAnimation(_spriteAnimator);
                    break;
                default:
                    // Default to idle animation
                    SpritesheetHelper.ConfigureIdleAnimation(_spriteAnimator);
                    break;
            }
            
            _spriteAnimator.Play();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Simple relay command implementation
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
