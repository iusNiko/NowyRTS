using Godot;
using System;

public partial class StateDead : State
{
    //Properties

    //Methods
    public StateDead(StateManager stateManager) : base(stateManager) {}

    public override void OnEnter() {
        StateManager.Unit.OnDeath();
    }

    public override void OnExit() {

    }

    public override void OnUpdate(float delta) {
        
    }
}