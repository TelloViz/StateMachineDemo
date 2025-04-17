using System;
using StateMachine.Core;

namespace StateMachine.Models
{
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
