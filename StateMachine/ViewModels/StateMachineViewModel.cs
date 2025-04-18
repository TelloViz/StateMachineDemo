using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using StateMachine.Animation;
using StateMachine.Models;
using StateMachine.Core;
using System.Collections.Generic;

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
        private bool _isSimulatingActivity = false;
        
        // State visualization properties
        private SolidColorBrush _idleStateBackground = new SolidColorBrush(Colors.White);
        private SolidColorBrush _walkingStateBackground = new SolidColorBrush(Colors.White);
        private SolidColorBrush _runningStateBackground = new SolidColorBrush(Colors.White);
        private SolidColorBrush _lookUpStateBackground = new SolidColorBrush(Colors.White);
        private SolidColorBrush _duckingStateBackground = new SolidColorBrush(Colors.White);
        
        private SolidColorBrush _idleStateBorderBrush = new SolidColorBrush(Colors.Gray);
        private SolidColorBrush _walkingStateBorderBrush = new SolidColorBrush(Colors.Gray);
        private SolidColorBrush _runningStateBorderBrush = new SolidColorBrush(Colors.Gray);
        private SolidColorBrush _lookUpStateBorderBrush = new SolidColorBrush(Colors.Gray);
        private SolidColorBrush _duckingStateBorderBrush = new SolidColorBrush(Colors.Gray);

        private Dictionary<string, Path> _transitionPaths = new Dictionary<string, Path>();
        private Dictionary<string, Path> _arrowheadPaths = new Dictionary<string, Path>();
        private Path _lastHighlightedPath;
        private Path _lastHighlightedArrowhead;
        private SolidColorBrush _defaultPathColor = new SolidColorBrush(Colors.Gray);
        private SolidColorBrush _highlightPathColor = new SolidColorBrush(Colors.Red);

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
        
        public SolidColorBrush DuckingStateBackground 
        { 
            get => _duckingStateBackground; 
            set 
            {
                _duckingStateBackground = value;
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
        
        public SolidColorBrush DuckingStateBorderBrush 
        { 
            get => _duckingStateBorderBrush; 
            set 
            {
                _duckingStateBorderBrush = value;
                OnPropertyChanged();
            }
        }
        
        public ObservableCollection<string> AvailableStates { get; } = new ObservableCollection<string>();

        public ICommand ChangeStateCommand { get; }
        public ICommand ClearLogCommand { get; }
        public ICommand UpdateStateCommand { get; }
        public ICommand SimulateActivityCommand { get; }
        public ICommand SwitchToMarioCommand { get; }
        public ICommand SwitchToLuigiCommand { get; }
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
            var duckingState = new DuckingState(AddLogMessage);
            
            _stateMachine.AddStates(idleState, walkingState, runningState, lookUpState, duckingState);
            
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
            SwitchToMarioCommand = new RelayCommand<object>(_ => SwitchCharacter(SpritesheetHelper.Character.Mario));
            SwitchToLuigiCommand = new RelayCommand<object>(_ => SwitchCharacter(SpritesheetHelper.Character.Luigi));
            
            // Setup update timer for updating state time display and other UI elements
            _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _updateTimer.Tick += (s, e) => 
            {
                UpdateTimeInState();
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
        
        public void RegisterTransitionPaths(
            Path idleToWalking, Path idleToWalking_Forward, Path idleToWalking_Backward,
            Path walkingToRunning, Path walkingToRunning_Forward, Path walkingToRunning_Backward,
            Path idleToRunning, Path idleToRunning_Forward, Path idleToRunning_Backward,
            Path idleToLookUp, Path idleToLookUp_Forward, Path idleToLookUp_Backward,
            Path idleToDucking, Path idleToDucking_Forward, Path idleToDucking_Backward)
        {
            _transitionPaths["IdleToWalking"] = idleToWalking;
            _arrowheadPaths["IdleToWalking_Forward"] = idleToWalking_Forward;
            _arrowheadPaths["IdleToWalking_Backward"] = idleToWalking_Backward;
            
            _transitionPaths["WalkingToRunning"] = walkingToRunning;
            _arrowheadPaths["WalkingToRunning_Forward"] = walkingToRunning_Forward;
            _arrowheadPaths["WalkingToRunning_Backward"] = walkingToRunning_Backward;
            
            _transitionPaths["IdleToRunning"] = idleToRunning;
            _arrowheadPaths["IdleToRunning_Forward"] = idleToRunning_Forward;
            _arrowheadPaths["IdleToRunning_Backward"] = idleToRunning_Backward;
            
            _transitionPaths["IdleToLookUp"] = idleToLookUp;
            _arrowheadPaths["IdleToLookUp_Forward"] = idleToLookUp_Forward;
            _arrowheadPaths["IdleToLookUp_Backward"] = idleToLookUp_Backward;
            
            _transitionPaths["IdleToDucking"] = idleToDucking;
            _arrowheadPaths["IdleToDucking_Forward"] = idleToDucking_Forward;
            _arrowheadPaths["IdleToDucking_Backward"] = idleToDucking_Backward;
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
                
                // Check for Ducking state constraints
                if (stateName == "Ducking" && CurrentStateName != "Idle")
                {
                    AddLogMessage($"Cannot transition to {stateName} from {CurrentStateName}. Only allowed from Idle state.");
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
                case "Ducking":
                    SpritesheetHelper.ConfigureDuckingAnimation(_spriteAnimator);
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
            DuckingStateBackground = new SolidColorBrush(Colors.White);
            
            IdleStateBorderBrush = new SolidColorBrush(Colors.Gray);
            WalkingStateBorderBrush = new SolidColorBrush(Colors.Gray);
            RunningStateBorderBrush = new SolidColorBrush(Colors.Gray);
            LookUpStateBorderBrush = new SolidColorBrush(Colors.Gray);
            DuckingStateBorderBrush = new SolidColorBrush(Colors.Gray);
            
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
                case "Ducking":
                    DuckingStateBackground = new SolidColorBrush(Color.FromRgb(200, 230, 255));
                    DuckingStateBorderBrush = new SolidColorBrush(Colors.Blue);
                    break;
            }
        }
        
        private void DisplayStateTransition(string fromState, string toState)
        {
            // Show transition animation by highlighting the appropriate edge and arrowhead
            if (fromState == toState || fromState == "None") return;
            
            // Reset any previous highlight before setting a new one
            ResetTransitionHighlight();
            
            Path pathToHighlight = null;
            Path arrowheadToHighlight = null;
            
            // Determine which path/arrowhead to highlight based on direction
            if (fromState == "Idle" && toState == "Walking")
            {
                pathToHighlight = _transitionPaths["IdleToWalking"];
                arrowheadToHighlight = _arrowheadPaths["IdleToWalking_Forward"];
            }
            else if (fromState == "Walking" && toState == "Idle")
            {
                pathToHighlight = _transitionPaths["IdleToWalking"];
                arrowheadToHighlight = _arrowheadPaths["IdleToWalking_Backward"];
            }
            else if (fromState == "Walking" && toState == "Running")
            {
                pathToHighlight = _transitionPaths["WalkingToRunning"];
                arrowheadToHighlight = _arrowheadPaths["WalkingToRunning_Forward"];
            }
            else if (fromState == "Running" && toState == "Walking")
            {
                pathToHighlight = _transitionPaths["WalkingToRunning"];
                arrowheadToHighlight = _arrowheadPaths["WalkingToRunning_Backward"];
            }
            else if (fromState == "Idle" && toState == "Running")
            {
                pathToHighlight = _transitionPaths["IdleToRunning"];
                arrowheadToHighlight = _arrowheadPaths["IdleToRunning_Forward"];
            }
            else if (fromState == "Running" && toState == "Idle")
            {
                pathToHighlight = _transitionPaths["IdleToRunning"];
                arrowheadToHighlight = _arrowheadPaths["IdleToRunning_Backward"];
            }
            else if (fromState == "Idle" && toState == "LookUp")
            {
                pathToHighlight = _transitionPaths["IdleToLookUp"];
                arrowheadToHighlight = _arrowheadPaths["IdleToLookUp_Forward"];
            }
            else if (fromState == "LookUp" && toState == "Idle")
            {
                pathToHighlight = _transitionPaths["IdleToLookUp"];
                arrowheadToHighlight = _arrowheadPaths["IdleToLookUp_Backward"];
            }
            else if (fromState == "Idle" && toState == "Ducking")
            {
                pathToHighlight = _transitionPaths["IdleToDucking"];
                arrowheadToHighlight = _arrowheadPaths["IdleToDucking_Forward"];
            }
            else if (fromState == "Ducking" && toState == "Idle")
            {
                pathToHighlight = _transitionPaths["IdleToDucking"];
                arrowheadToHighlight = _arrowheadPaths["IdleToDucking_Backward"];
            }
            
            // Highlight the path and arrowhead if found
            if (pathToHighlight != null)
            {
                pathToHighlight.Stroke = _highlightPathColor;
                _lastHighlightedPath = pathToHighlight;
            }
            
            if (arrowheadToHighlight != null)
            {
                arrowheadToHighlight.Fill = _highlightPathColor;
                _lastHighlightedArrowhead = arrowheadToHighlight;
            }
        }
        
        private void ResetTransitionHighlight()
        {
            if (_lastHighlightedPath != null)
            {
                _lastHighlightedPath.Stroke = _defaultPathColor;
                _lastHighlightedPath = null;
            }
            
            if (_lastHighlightedArrowhead != null)
            {
                _lastHighlightedArrowhead.Fill = _defaultPathColor;
                _lastHighlightedArrowhead = null;
            }
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
                string[] idleOptions = { "Walking", "Running", "LookUp", "Ducking" };
                newState = idleOptions[_random.Next(idleOptions.Length)];
            }
            else if (CurrentStateName == "Ducking")
            {
                // From Ducking, can only go to Idle
                newState = "Idle";
            }
            else
            {
                // From other states, can go anywhere except LookUp and Ducking
                string[] otherOptions = { "Idle", "Walking", "Running" };
                do 
                {
                    newState = otherOptions[_random.Next(otherOptions.Length)];
                } while (newState == CurrentStateName);
            }
            
            AddLogMessage($"Automatic transition to {newState}");
            ChangeState(newState);
        }

        private void SwitchCharacter(SpritesheetHelper.Character character)
        {
            // Update the character in SpritesheetHelper
            SpritesheetHelper.SetCharacter(character);
            
            // Reinitialize the sprite animator with the new spritesheet
            try
            {
                _spriteAnimator.Stop();
                _spriteAnimator.ChangeSpritesheet(SpritesheetHelper.GetSpritesheetPath());
                
                // Update animation for current state
                UpdateAnimationForState();
                
                AddLogMessage($"Switched to {character} sprite");
            }
            catch (Exception ex)
            {
                AddLogMessage($"Failed to switch sprite: {ex.Message}");
            }
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
