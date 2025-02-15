using Godot;
using System;

public partial class StateConstructionPlanned : State
{
    //Properties

    //Methods
    public StateConstructionPlanned(StateManager stateManager) : base(stateManager) {}

    public override void OnEnter() {
        GD.Print("Construction planned");
    }

    public override void OnExit() {
        
    }

    public override void OnUpdate(float delta) {
        
    }
}