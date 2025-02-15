using Godot;
using System;

public partial class StateUnderConstruction : State
{
    //Properties

    //Methods
    public StateUnderConstruction(StateManager stateManager) : base(stateManager) {}

    public override void OnEnter() {
        StateManager.Unit.PlayAnimation("construction");
        StateManager.Unit.Animation.Modulate = new Color(1, 1, 1, 1);
        StateManager.Unit.Builder.SetSelected(false);
        World.Instance.SelectedUnits.Remove(StateManager.Unit.Builder);
        UI.Instance.ClearCommandCard();
        GD.Print("Under construction");
    }

    public override void OnExit() {
        StateManager.Unit.Builder.Visible = true;
        StateManager.Unit.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
        StateManager.Unit.ConstructionState = 2;
    }

    public override void OnUpdate(float delta) {
        Unit unit = StateManager.Unit;
        unit.TimeSinceConstructionStart += delta;
        if(unit.TimeSinceConstructionStart >= unit.ConstructionTime) {
            unit.StateManager.ChangeState(new StateIdle(unit.StateManager));
        }
    }
}