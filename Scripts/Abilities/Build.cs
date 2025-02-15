using Godot;
using System;

public partial class Build : Ability
{
	[Export] public PackedScene BuildingPrefab;
	public Unit Building = null;

    public override void Effect()
    {
		if(Building != null) {
			Building.ConstructionState = 0;
			GetParent<Unit>().ConstructionTarget = Building;
			Building = null;
		}
    }
}
