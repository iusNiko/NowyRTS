using Godot;
using System;

public partial class HomingProjectile : AnimatedSprite2D
{
	[Export] public float Damage = 0;
	[Export] public float Speed = 100f;
	public Unit Target;
	public Unit Source;

    public override void _Process(double delta)
    {
		if(!IsInstanceValid(Target)) {
			QueueFree();
			return;
		}
		if(GlobalPosition.DistanceTo(Target.GlobalPosition) < (GlobalPosition.DirectionTo(Target.GlobalPosition) * Speed * (float) delta).Length()) {
			Target.Life -= Damage;
			Target.MovementTarget = Source.GlobalPosition;
			Target.StateManager.ChangeState(new StateAMove(Target.StateManager));
			foreach(Unit u in World.Instance.GetUnitsInRadius(Target.GlobalPosition, 50)) {
				if(u.Team == Target.Team) {
					u.MovementTarget = Source.GlobalPosition;
					u.StateManager.ChangeState(new StateAMove(u.StateManager));
				}
			}
			QueueFree();
			return;
		}
		else {
			Position += GlobalPosition.DirectionTo(Target.GlobalPosition) * Speed * (float) delta;
			LookAt(Target.GlobalPosition);
		}
    }
}
