using Godot;
using System;

    public partial class State
    {
        //Properties

        public StateManager StateManager;

        //Methods
        public State(StateManager stateManager) {
            StateManager = stateManager;
        }
        public virtual void OnEnter() {}
        public virtual void OnExit() {}
        public virtual void OnUpdate(float delta) {}
    }
