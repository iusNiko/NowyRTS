using Godot;
using System;

public partial class LifeBar : ProgressBar
{
	Unit Unit;
	public override void _Ready()
	{
		Unit = GetParent<Unit>();
		if(Unit.Team == 2) {
			Modulate = new Color(1, 0, 0);
		}
		else if(Unit.Team == 1) {
			Modulate = new Color(0, 1, 0);
		}
		else {
			Modulate = new Color(1, 1, 1);
		}
	}

	public override void _Process(double delta)
	{
		MaxValue = Unit.MaxLife;
		Value = Unit.Life;

		if(MaxValue != Value) Visible = true;
		else Visible = false;
	}
}
