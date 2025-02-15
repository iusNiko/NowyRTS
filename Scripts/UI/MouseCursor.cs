using Godot;
using System;

public partial class MouseCursor : Area2D
{
	public Unit TargetedUnit = null;

	public void UnitEntered(Node unit) {
		if(unit is Unit u) {
			TargetedUnit = u;
		}
	}

	public void UnitExited(Node unit) {
		if(unit == TargetedUnit) {
			TargetedUnit = null;
		}
	}
    public override void _Ready()
    {
        BodyEntered += UnitEntered;
		BodyExited += UnitExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition = GetGlobalMousePosition();
    }
}
