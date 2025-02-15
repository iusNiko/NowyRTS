using Godot;
using System;

public partial class StateWalking : State
{
    //Properties

    //Methods
    public StateWalking(StateManager stateManager) : base(stateManager) {}

    public override void OnEnter() {
        StateManager.Unit.Animation.Play("walk");
    }

    public override void OnExit() {
    }

    public override void OnUpdate(float delta) {
        Unit unit = StateManager.Unit;

        unit._navigationAgent.TargetPosition = unit.MovementTarget;
        if(!unit._navigationAgent.IsNavigationFinished()) {
			Vector2 currentAgentPosition = unit.GlobalTransform.Origin;
        	Vector2 nextPathPosition = unit._navigationAgent.GetNextPathPosition();

			unit.Velocity = currentAgentPosition.DirectionTo(nextPathPosition) * unit.MovementSpeed;

		}
		else {
			unit.Velocity = Vector2.Zero;
			unit.MovementTarget = unit.GlobalPosition;
            StateManager.ChangeState(new StateIdle(StateManager));
		}

        unit._navigationAgent.Velocity = unit.Velocity;
        unit.ChangefacingDirection(unit.MovementTarget);
    }
}