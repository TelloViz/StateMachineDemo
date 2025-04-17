using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using StateMachine.Animation;
using StateMachine.Models;
using StateMachine.Core;

namespace StateMachine.ViewModels
{
    public class StateMachineViewModel : INotifyPropertyChanged
    {
        private readonly StateMachine.Core.StateMachine _stateMachine;
        private readonly DispatcherTimer _updateTimer;
        private readonly DispatcherTimer _activityTimer;
        private readonly Random _random = new Random();
        private readonly SpriteAnimator _spriteAnimator;
        private readonly DateTime _applicationStartTime;
        
        private string _currentStateName = "None";
        private string _previousStateName = "None";
        private string _logText = string.Empty;
        private ImageSource _currentSpriteFrame;
        private DateTime _stateEntryTime;
        private string _timeInCurrentState = "0s";
        private double _transitionIndicatorOpacity = 0;
        private string _lastTransitionPathData = "";
        private bool _isSimulatingActivity = false;
        
        // State visualization properties
        private SolidColorBrush _idleStateBackground = new SolidColorBrush(Colors.White);
        private SolidColorBrush _walkingStateBackground = new SolidColorBrush(Colors.White);
        private SolidColorBrush _runningStateBackground = new SolidColorBrush(Colors.White);
        private SolidColorBrush _lookUpStateBackground = new SolidColorBrush(Colors.White);
        
        private SolidColorBrush _idleStateBorderBrush = new SolidColorBrush(Colors.Gray);
        private SolidColorBrush _walkingStateBorderBrush = new SolidColorBrush(Colors.Gray);
        private SolidColorBrush _runningStateBorderBrush = new SolidColorBrush(Colors.Gray);
        private SolidColorBrush _lookUpStateBorderBrush = new SolidColorBrush(Colors.Gray);

        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties
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
                    UpdateStateDiagram();
                }
            }
        }

        public string PreviousStateName
        {
            get => _previousStateName;
            private set
            {
                if (_previousStateName != value)
                {
                    _previousStateName = value;
                    OnPropertyChanged();
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
        
        public int StateCount => _stateMachine?.States.Count ?? 0;
        
        public string TimeInCurrentState
        {
            get => _timeInCurrentState;
            private set
            {
                _timeInCurrentState = value;
                OnPropertyChanged();
            }
        }
        
        public double TransitionIndicatorOpacity
        {
            get => _transitionIndicatorOpacity;
            private set
            {
                _transitionIndicatorOpacity = value;
                OnPropertyChanged();
            }
        }
        
        public string LastTransitionPathData
        {
            get => _lastTransitionPathData;
            private set
            {
                _lastTransitionPathData = value;
                OnPropertyChanged();
            }
        }
        
        public SolidColorBrush IdleStateBackground 
        { 
            get => _idleStateBackground; 
            set 
            {
                _idleStateBackground = value;
                OnPropertyChanged();
            }
        }
        
        public SolidColorBrush WalkingStateBackground 
        { 
            get => _walkingStateBackground; 
            set 
            {
                _walkingStateBackground = value;
                OnPropertyChanged();
            }
        }
        
        public SolidColorBrush RunningStateBackground 
        { 
            get => _runningStateBackground; 
            set 
            {
                _runningStateBackground = value;
                OnPropertyChanged();
            }
        }
        
        public SolidColorBrush LookUpStateBackground 
        { 
            get => _lookUpStateBackground; 
            set 
            {
                _lookUpStateBackground = value;
                OnPropertyChanged();
            }
        }
        
        public SolidColorBrush IdleStateBorderBrush 
        { 
            get => _idleStateBorderBrush; 
            set 
            {
                _idleStateBorderBrush = value;
                OnPropertyChanged();
            }
        }
        
        public SolidColorBrush WalkingStateBorderBrush 
        { 
            get => _walkingStateBorderBrush; 
            set 
            {
                _walkingStateBorderBrush = value;
                OnPropertyChanged();
            }
        }
        
        public SolidColorBrush RunningStateBorderBrush 
        { 
            get => _runningStateBorderBrush; 
            set 
            {
                _runningStateBorderBrush = value;
                OnPropertyChanged();
            }
        }
        
        public SolidColorBrush LookUpStateBorderBrush 
        { 
            get => _lookUpStateBorderBrush; 
            set 
            {
                _lookUpStateBorderBrush = value;
                OnPropertyChanged();
            }
        }
        
        public ObservableCollection<string> AvailableStates { get; } = new ObservableCollection<string>();

        public ICommand ChangeStateCommand { get; }
        public ICommand ClearLogCommand { get; }
        public ICommand UpdateStateCommand { get; }
        public ICommand SimulateActivityCommand { get; }
        #endregion

        public StateMachineViewModel()
        {
            _applicationStartTime = DateTime.Now;
            _stateEntryTime = DateTime.Now;
            
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
            _stateMachine = new StateMachine.Core.StateMachine();
            _stateMachine.OnStateChange = state => 
            {
                PreviousStateName = CurrentStateName;
                CurrentStateName = state?.Name ?? "None";
                _stateEntryTime = DateTime.Now;
                DisplayStateTransition(PreviousStateName, CurrentStateName);
                AddLogMessage($"State changed to: {CurrentStateName}");
            };

            // Create and add states
            var idleState = new IdleState(AddLogMessage);
            var walkingState = new WalkingState(AddLogMessage);
            var runningState = new RunningState(AddLogMessage);
            var lookUpState = new LookUpState(AddLogMessage);
            
            _stateMachine.AddStates(idleState, walkingState, runningState, lookUpState);
            
            // Initialize available states collection
            foreach (var state in _stateMachine.States)
            {
                AvailableStates.Add(state.Name);
            }

            // Create commands
            ChangeStateCommand = new RelayCommand<string>(ChangeState);
            ClearLogCommand = new RelayCommand<object>(_ => ClearLog());
            UpdateStateCommand = new RelayCommand<object>(_ => UpdateActiveState());
            SimulateActivityCommand = new RelayCommand<object>(_ => ToggleAutomaticStateChanges());
            
            // Setup update timer for updating state time display and other UI elements
            _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _updateTimer.Tick += (s, e) => 
            {
                UpdateTimeInState();
                FadeTransitionIndicator();
            };
            _updateTimer.Start();
            
            // Setup activity simulation timer (initially stopped)
            _activityTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _activityTimer.Tick += (s, e) => SimulateStateChange();
            
            // Set initial state
            if (_stateMachine.States.Count > 0)
            {
                _stateMachine.ChangeState(_stateMachine.States[0]);
                OnPropertyChanged(nameof(StateCount));
            }
        }
        
        private void ChangeState(string stateName)
        {
            if (!string.IsNullOrEmpty(stateName))
            {
                // Check for LookUp state constraints
                if (stateName == "LookUp" && CurrentStateName != "Idle")
                {
                    AddLogMessage($"Cannot transition to {stateName} from {CurrentStateName}. Only allowed from Idle state.");
                    return;
                }
                
                if (CurrentStateName == "LookUp" && stateName != "Idle")
                {
                    AddLogMessage($"Cannot transition from LookUp to {stateName}. Only allowed to go to Idle state.");
                    return;
                }
                
                _stateMachine.ChangeState(stateName);
            }
        }
        
        private void UpdateActiveState()
        {
            _stateMachine.UpdateActiveState();
            AddLogMessage($"Update() called on state: {CurrentStateName}");
        }
        
        private void AddLogMessage(string message)
        {
            var timeOffset = DateTime.Now - _applicationStartTime;
            LogText = $"[{timeOffset:hh\\:mm\\:ss\\.fff}] {message}\n{LogText}";
        }

        private void ClearLog()
        {
            LogText = string.Empty;
            AddLogMessage("Log cleared");
        }
        
        private void UpdateTimeInState()
        {
            var timeInState = DateTime.Now - _stateEntryTime;
            TimeInCurrentState = $"{timeInState.TotalSeconds:F1}s";
        }
        
        private void FadeTransitionIndicator()
        {
            if (TransitionIndicatorOpacity > 0)
            {
                TransitionIndicatorOpacity -= 0.02;
                if (TransitionIndicatorOpacity < 0) TransitionIndicatorOpacity = 0;
            }
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
                case "LookUp":
                    SpritesheetHelper.ConfigureLookUpAnimation(_spriteAnimator);
                    break;
                default:
                    // Default to idle animation
                    SpritesheetHelper.ConfigureIdleAnimation(_spriteAnimator);
                    break;
            }
            
            _spriteAnimator.Play();
        }
        
        private void UpdateStateDiagram()
        {
            // Reset all backgrounds and borders
            IdleStateBackground = new SolidColorBrush(Colors.White);
            WalkingStateBackground = new SolidColorBrush(Colors.White);
            RunningStateBackground = new SolidColorBrush(Colors.White);
            LookUpStateBackground = new SolidColorBrush(Colors.White);
            
            IdleStateBorderBrush = new SolidColorBrush(Colors.Gray);
            WalkingStateBorderBrush = new SolidColorBrush(Colors.Gray);
            RunningStateBorderBrush = new SolidColorBrush(Colors.Gray);
            LookUpStateBorderBrush = new SolidColorBrush(Colors.Gray);
            
            // Highlight current state
            switch (CurrentStateName)
            {
                case "Idle":
                    IdleStateBackground = new SolidColorBrush(Color.FromRgb(200, 230, 255));
                    IdleStateBorderBrush = new SolidColorBrush(Colors.Blue);
                    break;
                case "Walking":
                    WalkingStateBackground = new SolidColorBrush(Color.FromRgb(200, 230, 255));
                    WalkingStateBorderBrush = new SolidColorBrush(Colors.Blue);
                    break;
                case "Running":
                    RunningStateBackground = new SolidColorBrush(Color.FromRgb(200, 230, 255));
                    RunningStateBorderBrush = new SolidColorBrush(Colors.Blue);
                    break;
                case "LookUp":
                    LookUpStateBackground = new SolidColorBrush(Color.FromRgb(200, 230, 255));
                    LookUpStateBorderBrush = new SolidColorBrush(Colors.Blue);
                    break;
            }
        }
        
        private void DisplayStateTransition(string fromState, string toState)
        {
            // Show transition animation on the state diagram
            if (fromState == toState || fromState == "None") return;
            
            // Define transition paths
            string pathData = "";
            
            // Original transitions
            if (fromState == "Idle" && toState == "Walking")
                pathData = "M100,85 C105,80 108,80 110,75";
            else if (fromState == "Walking" && toState == "Running")
                pathData = "M170,80 C175,80 178,85 180,85";
            else if (fromState == "Running" && toState == "Idle")
                pathData = "M180,110 C160,115 120,115 100,110";
            else if (fromState == "Walking" && toState == "Idle")
                pathData = "M110,80 C105,85 100,85 95,90";
            else if (fromState == "Idle" && toState == "Running")
                pathData = "M100,110 C130,115 150,115 180,110";
            else if (fromState == "Running" && toState == "Walking")
                pathData = "M180,85 C175,85 170,80 165,80";
                
            // LookUp state transitions - only to/from Idle
            else if (fromState == "Idle" && toState == "LookUp")
                pathData = "M70,80 C90,50 120,30 140,30";
            else if (fromState == "LookUp" && toState == "Idle")
                pathData = "M140,30 C120,30 90,50 70,80";
            
            LastTransitionPathData = pathData;
            TransitionIndicatorOpacity = 1.0;
        }
        
        private void ToggleAutomaticStateChanges()
        {
            _isSimulatingActivity = !_isSimulatingActivity;
            
            if (_isSimulatingActivity)
            {
                AddLogMessage("Started automatic state simulation");
                _activityTimer.Start();
            }
            else
            {
                AddLogMessage("Stopped automatic state simulation");
                _activityTimer.Stop();
            }
        }
        
        private void SimulateStateChange()
        {
            string newState;
            
            // Handle state transition restrictions
            if (CurrentStateName == "LookUp")
            {
                // From LookUp, can only go to Idle
                newState = "Idle";
            }
            else if (CurrentStateName == "Idle")
            {
                // From Idle, can go to any state
                string[] idleOptions = { "Walking", "Running", "LookUp" };
                newState = idleOptions[_random.Next(idleOptions.Length)];
            }
            else
            {
                // From other states, can go anywhere except LookUp
                string[] otherOptions = { "Idle", "Walking", "Running" };
                do 
                {
                    newState = otherOptions[_random.Next(otherOptions.Length)];
                } while (newState == CurrentStateName);
            }
            
            AddLogMessage($"Automatic transition to {newState}");
            ChangeState(newState);
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
