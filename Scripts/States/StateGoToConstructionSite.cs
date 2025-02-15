using Godot;
using System;

public partial class StateGoToConstructionSite : State
{
    //Properties

    //Methods
    public StateGoToConstructionSite(StateManager stateManager) : base(stateManager) {}

    public override void OnEnter() {
        StateManager.Unit.PlayAnimation("walk");
    }

    public override void OnExit() {
    }

    public override void OnUpdate(float delta) {
        Unit unit = StateManager.Unit;

        if(unit.ConstructionTarget == null) {
            StateManager.ChangeState(new StateIdle(StateManager));
            return;
        }

        unit._navigationAgent.TargetPosition = unit.ConstructionTarget.GlobalPosition;
        if(!unit._navigationAgent.IsNavigationFinished()) {
			Vector2 currentAgentPosition = unit.GlobalTransform.Origin;
        	Vector2 nextPathPosition = unit._navigationAgent.GetNextPathPosition();

			unit.Velocity = currentAgentPosition.DirectionTo(nextPathPosition) * unit.MovementSpeed;

		}

        unit._navigationAgent.Velocity = unit.Velocity;
        unit.ChangefacingDirection(unit.MovementTarget);

        if(unit.GlobalPosition.DistanceTo(unit.ConstructionTarget.GlobalPosition) <= unit.ConstructionTarget._navigationAgent.Radius * 2 + 2) {
            unit._navigationAgent.Velocity = Vector2.Zero;
            unit.StateManager.ChangeState(new StateConstructing(StateManager));
            unit.ConstructionTarget.StateManager.ChangeState(new StateUnderConstruction(unit.ConstructionTarget.StateManager));
        }
    }
}