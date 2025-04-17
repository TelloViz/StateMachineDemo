using System;
using Vast.StateMachine;

namespace StateMachine.Models
{
    public class IdleState : State
    {
        private readonly Action<string> _logAction;
        
        public IdleState(Action<string> logAction)
        {
            Name = "Idle";
            _logAction = logAction;
        }

        public override void OnEnter()
        {
            _logAction?.Invoke("Entered Idle State");
        }

        public override void OnExit()
        {
            _logAction?.Invoke("Exited Idle State");
        }

        public override void Update()
        {
            // No continuous updates in this example
        }

        public override void FixedUpdate()
        {
            // No physics updates in this example
        }
    }

    public class WalkingState : State
    {
        private readonly Action<string> _logAction;
        
        public WalkingState(Action<string> logAction)
        {
            Name = "Walking";
            _logAction = logAction;
        }

        public override void OnEnter()
        {
            _logAction?.Invoke("Entered Walking State");
        }

        public override void OnExit()
        {
            _logAction?.Invoke("Exited Walking State");
        }

        public override void Update()
        {
            // Could update position in a game context
        }

        public override void FixedUpdate()
        {
            // No physics updates in this example
        }
    }

    public class RunningState : State
    {
        private readonly Action<string> _logAction;
        
        public RunningState(Action<string> logAction)
        {
            Name = "Running";
            _logAction = logAction;
        }

        public override void OnEnter()
        {
            _logAction?.Invoke("Entered Running State");
        }

        public override void OnExit()
        {
            _logAction?.Invoke("Exited Running State");
        }

        public override void Update()
        {
            // Could update position at faster rate in a game context
        }

        public override void FixedUpdate()
        {
            // No physics updates in this example
        }
    }
}
