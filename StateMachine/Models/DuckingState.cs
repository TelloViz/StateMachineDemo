using System;
using StateMachine.Core;

namespace StateMachine.Models
{
    public class DuckingState : State
    {
        private readonly Action<string> _logAction;
        
        public DuckingState(Action<string> logAction)
        {
            Name = "Ducking";
            _logAction = logAction;
        }

        public override void OnEnter()
        {
            _logAction?.Invoke("Entered Ducking State");
        }

        public override void OnExit()
        {
            _logAction?.Invoke("Exited Ducking State");
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
}
