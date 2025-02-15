using Godot;
using System;

public partial class StateAttacking : State
{
    //Properties

    //Methods
    public StateAttacking(StateManager stateManager) : base(stateManager) {}

    public override void OnEnter() {
        
    }

    public override void OnExit() {
        
    }

    public override void OnUpdate(float delta) {
        Unit unit = StateManager.Unit;

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
            else {
                unit._navigationAgent.AvoidancePriority = 0.1f;
                unit.StateManager.ChangeState(new StateIdle(StateManager));
            }
		}
        else {
            unit.StateManager.ChangeState(new StateIdle(StateManager));
        }
    }
}