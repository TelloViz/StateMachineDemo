using System;
using StateMachine.Core;

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
}
