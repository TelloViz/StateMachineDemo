using System;
using StateMachine.Core;

namespace StateMachine.Models
{
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
}
