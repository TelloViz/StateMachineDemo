using System;
using StateMachine.Core;

namespace StateMachine.Models
{
    public class LookUpState : State
    {
        private readonly Action<string> _logAction;
        
        public LookUpState(Action<string> logAction)
        {
            Name = "LookUp";
            _logAction = logAction;
        }

        public override void OnEnter()
        {
            _logAction?.Invoke("Entered Look Up State");
        }

        public override void OnExit()
        {
            _logAction?.Invoke("Exited Look Up State");
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
