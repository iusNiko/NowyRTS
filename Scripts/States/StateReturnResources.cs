using Godot;
using System;

public partial class StateReturnResources : State {
    public StateReturnResources(StateManager stateManager) : base(stateManager) {}

    public Unit ResourceDropOff;

    public override void OnEnter() {
        ResourceDropOff = World.Instance.NearestResourceDropoffPosition(StateManager.Unit.GlobalPosition);
        StateManager.Unit._navigationAgent.AvoidanceLayers = 0;
		StateManager.Unit._navigationAgent.AvoidanceMask = 0;
        StateManager.Unit.PlayAnimation("walk");
    }

    public override void OnExit() {
        
    }

    public override void OnUpdate(float delta) {
        Unit unit = StateManager.Unit;

        unit._navigationAgent.TargetPosition = ResourceDropOff.GlobalPosition;
        if(!unit._navigationAgent.IsNavigationFinished()) {
			Vector2 currentAgentPosition = unit.GlobalTransform.Origin;
        	Vector2 nextPathPosition = unit._navigationAgent.GetNextPathPosition();

			unit.Velocity = currentAgentPosition.DirectionTo(nextPathPosition) * unit.MovementSpeed;
            unit.ChangefacingDirection(nextPathPosition);
		}
		else {
			unit.Velocity = Vector2.Zero;
			unit.MovementTarget = unit.GlobalPosition;
		}

        unit._navigationAgent.Velocity = unit.Velocity;
        unit.ChangefacingDirection(unit.MovementTarget);

        if(unit.GlobalPosition.DistanceTo(ResourceDropOff.GlobalPosition) <= unit.ResourceDropoffRange) {
            World.Instance.Gold += unit.GoldHarvested;
            unit.GoldHarvested = 0;
            World.Instance.Wood += unit.WoodHarvested;
            unit.WoodHarvested = 0;
            StateManager.ChangeState(new StateHarvesting(StateManager));
        }
    }
}