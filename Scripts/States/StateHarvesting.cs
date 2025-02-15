using Godot;
using System;

public partial class StateHarvesting : State {
    public StateHarvesting(StateManager stateManager) : base(stateManager) {}

    public override void OnEnter() {
        StateManager.Unit._navigationAgent.AvoidanceLayers = 0;
		StateManager.Unit._navigationAgent.AvoidanceMask = 0;
    }

    public override void OnExit() {
        
    }

    public override void OnUpdate(float delta) {
        Unit unit = StateManager.Unit;
        if(unit.HarvestingTarget != null) {
            if(unit.GlobalPosition.DistanceTo(unit.HarvestingTarget.GlobalPosition) > unit.HarvestingDistance) {
                unit._navigationAgent.TargetPosition = unit.HarvestingTarget.GlobalPosition;
                if(!unit._navigationAgent.IsNavigationFinished()) {
                    StateManager.Unit.Animation.Play("walk");
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
            }
            else {
                unit._navigationAgent.Velocity = Vector2.Zero;
                if(unit.TimeSinceLastHarvest >= unit.HarvestCooldown) {
                    unit.Animation.Play("attack");
                    unit.TimeSinceLastHarvest = 0;
                    if(unit.HarvestingTarget.GoldAmount > 0) {
                        if(unit.HarvestingTarget.GoldAmount >= unit.HarvestAmountPerCycle) {
                            unit.GoldHarvested += unit.HarvestAmountPerCycle;
                            unit.HarvestingTarget.GoldAmount -= unit.HarvestAmountPerCycle;
                        }
                        else {
                            unit.GoldHarvested += unit.HarvestingTarget.GoldAmount;
                            unit.HarvestingTarget.GoldAmount = 0;
                        }
                    }
                    if(unit.HarvestingTarget.WoodAmount > 0) {
                        if(unit.HarvestingTarget.WoodAmount >= unit.HarvestAmountPerCycle) {
                            unit.WoodHarvested += unit.HarvestAmountPerCycle;
                            unit.HarvestingTarget.WoodAmount -= unit.HarvestAmountPerCycle;
                        }
                        else {
                            unit.WoodHarvested += unit.HarvestingTarget.WoodAmount;
                            unit.HarvestingTarget.WoodAmount = 0;
                        }
                    }
                }
            }
        }
        if(unit.GoldHarvested >= unit.MaxResourceCapacity || unit.WoodHarvested >= unit.MaxResourceCapacity) {
            unit.StateManager.ChangeState(new StateReturnResources(unit.StateManager));
            return;
        }
        if(unit.HarvestingTarget == null || (unit.HarvestingTarget.GoldAmount <= 0 && unit.HarvestingTarget.WoodAmount <= 0)) {
            foreach (Unit u in World.Instance.GetUnitsInRadius(unit.GlobalPosition, 100)) {
                if(u is ResourceDeposit rd) {
                    if(rd.GoldAmount > 0 || rd.WoodAmount > 0) {
                        unit.HarvestingTarget = rd;
                        return;
                    }
                }
            }
            unit._navigationAgent.Velocity = Vector2.Zero;
            unit.StateManager.ChangeState(new StateIdle(unit.StateManager));
        }
    }
}