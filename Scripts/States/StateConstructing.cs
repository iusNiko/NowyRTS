using Godot;
using System;

public partial class StateConstructing : State
{
    //Properties

    //Methods
    public StateConstructing(StateManager stateManager) : base(stateManager) {}

    public override void OnEnter() {
        StateManager.Unit.Visible = false;
        StateManager.Unit.SetSelected(false);
    }

    public override void OnExit() {
        StateManager.Unit.Visible = true;
    }

    public override void OnUpdate(float delta) {
        
    }
}