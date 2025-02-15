using Godot;
using System;

public partial class StateAMove : State
{
    //Properties

    //Methods
    public StateAMove(StateManager stateManager) : base(stateManager) {}

    public override void OnEnter() {
        
        StateManager.Unit.PlayAnimation("walk");
    }

    public override void OnExit() {
        
    }

    public override void OnUpdate(float delta) {
        Unit unit = StateManager.Unit;

        unit._navigationAgent.Velocity = Vector2.Zero;

        if(unit.AttackTarget == null || !unit.IsAttackTargetValid() || unit.GlobalPosition.DistanceTo(unit.AttackTarget.GlobalPosition) > unit.AttackRange) {
            unit.AttackTarget = unit.SearchTarget();
        }

        if(unit.AttackTarget == null || (unit.GlobalPosition.DistanceTo(unit.AttackTarget.GlobalPosition) > unit.AttackTargetPursueRange && unit.GlobalPosition == unit.MovementTarget)) {
            StateManager.ChangeState(new StateIdle(StateManager));
            return;
        }

        if(unit.AttackTarget.GlobalPosition.DistanceTo(unit.GlobalPosition) <= unit.AttackRange) {
            if(unit.ProjectilePrefab != null) {
                if(unit.AttackTarget != null && unit.TimeSinceLastAttack > unit.AttackCooldown && unit.IsAttackTargetValid() && unit.GlobalPosition.DistanceTo(unit.AttackTarget.GlobalPosition) <= unit.AttackRange) {
                    unit._navigationAgent.AvoidancePriority = 1;

                    unit.ChangefacingDirection(unit.AttackTarget.GlobalPosition);

                    unit.PlayAnimation("attack");

                    unit.TimeSinceLastAttack = 0;

                    var projectile = unit.ProjectilePrefab.Instantiate();
                    
                    if(projectile is HomingProjectile hp) {
                        hp.Target = unit.AttackTarget;
                        hp.Source = unit;
                        if(unit.AttackDamage != -1) hp.Damage = unit.AttackDamage;
                        if(unit.ProjectileSpeed != -1) hp.Speed = unit.ProjectileSpeed;
                        World.Instance.AddChild(hp);
                        hp.GlobalPosition = unit.ProjectileSpawn.GlobalPosition;
                    }
                }
            }
            return;
        }
        unit._navigationAgent.AvoidancePriority = 0.1f;

        unit._navigationAgent.AvoidancePriority = 0.1f;

        if(unit.GlobalPosition.DistanceTo(unit.AttackTarget.GlobalPosition) <= unit.AttackTargetPursueRange) {
            unit._navigationAgent.TargetPosition = unit.AttackTarget.GlobalPosition;
        }
        else {
            unit._navigationAgent.TargetPosition = unit.MovementTarget;
        }

        if(!unit._navigationAgent.IsNavigationFinished()) {
			Vector2 currentAgentPosition = unit.GlobalTransform.Origin;
        	Vector2 nextPathPosition = unit._navigationAgent.GetNextPathPosition();

			unit.Velocity = currentAgentPosition.DirectionTo(nextPathPosition) * unit.MovementSpeed;

		}
		else {
			unit.Velocity = Vector2.Zero;
			unit.MovementTarget = unit.GlobalPosition;
		}

        unit._navigationAgent.Velocity = unit.Velocity;
        unit.ChangefacingDirection(unit.MovementTarget);
    }
}