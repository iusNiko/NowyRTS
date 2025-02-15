using Godot;
using System;

public partial class Train : Ability
{
	[Export] public PackedScene UnitPrefab;

    public override void Effect()
    {
		if(UnitPrefab != null) {
			var u = UnitPrefab.Instantiate();
			if(u is Unit unit) {
				World.Instance.AddChild(unit);
				Vector2 pos = GetParent<Unit>().GlobalPosition;
				pos.Y += 16;
				unit.GlobalPosition = pos;
				unit.MovementTarget = pos;
				
			}
		}
    }
}
