using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class World : Node2D
{
	public MouseCursor MouseCursor;
	[Export] public float Gold = 0f;
	[Export] public float Wood = 0f;
	public static World Instance;
	public Unit LastUnitSelected = null;
	public Ability AbilityTargetedRightNow = null;
	public Unit[] Units = Array.Empty<Unit>();
	public Unit[] NeutralUnits = Array.Empty<Unit>();
	public Unit[] PlayerUnits = Array.Empty<Unit>();
	public Unit[] EnemyUnits = Array.Empty<Unit>();
	public List<Unit> SelectedUnits = new();
	public RandomNumberGenerator RNG = new();
	public int Frame = 0;
	public bool AMove = false;
	public int LastFrameAMove = 0;
	public Augment[] Augments = Array.Empty<Augment>();

    public override void _Ready()
	{
		RNG.Randomize();

		Instance = this;

		GetNode<Camera>("Camera2D").AreaSelected += OnAreaSelected;
		MouseCursor = GetNode<MouseCursor>("MouseCursor");		

		PackedScene s = GD.Load<PackedScene>("res://Prefabs/Units/Humans/Swordsman.tscn");

		//spawn 100 enemy units and 100 player units for testing

		/*

		Vector2 ps = new Vector2(0, 50);
		
		for (int i = 0; i < 100; i++)
		{
			Unit u = s.Instantiate<Unit>();
			u.Team = 1;
			ps.X += u.GetNode<NavigationAgent2D>("NavigationAgent2D").Radius * 2 + 1;
			u.GlobalPosition = ps;
			AddChild(u);
			if(ps.X > 200) {
				ps.X = 0;
				ps.Y += u.GetNode<NavigationAgent2D>("NavigationAgent2D").Radius * 2 + 1;
			}
		}

		ps = new Vector2(300, 150);

		for (int i = 0; i < 100; i++)
		{
			Unit u = s.Instantiate<Unit>();
			u.Team = 2;
			ps.X += u.GetNode<NavigationAgent2D>("NavigationAgent2D").Radius * 2 + 1;
			u.GlobalPosition = ps;
			AddChild(u);
			if(ps.X > 600) {
				ps.X = 300;
				ps.Y += u.GetNode<NavigationAgent2D>("NavigationAgent2D").Radius * 2 + 1;
			}
		}
		Augments = Augments.Append(new QuadraDamageAugment(1)).ToArray();

		*/
	}

    public override void _PhysicsProcess(double delta)
    {
		Frame += 1;

		if(LastFrameAMove > 0) {
			LastFrameAMove--;
		}

		Unit firstUnit = null;
		int SelectedUnitsCountRoot = (int)Math.Sqrt(SelectedUnits.Count);
		int i = 0;
		Vector2 Pos = Vector2.Zero;
		float BiggestRadius = 0;

		Vector2 center = Vector2.Zero;
		float maxDistance = float.MinValue;
		float maxRadius = float.MinValue;

		foreach(Unit u in SelectedUnits) {
			center += u.GlobalPosition;
			float closestUnitDistance = float.MaxValue;
			foreach(Unit u2 in SelectedUnits) {
				float distance = u.GlobalPosition.DistanceTo(u2.GlobalPosition);
				if(distance < closestUnitDistance || u2 != u) {
					closestUnitDistance = distance;
				}
			}
			if(closestUnitDistance > maxDistance) {
				maxDistance = closestUnitDistance;
			}
			if(u._navigationAgent.Radius > maxRadius) {
				maxRadius = u._navigationAgent.Radius;
			}
		}

		center /= SelectedUnits.Count;

		foreach (Unit selectedUnit in SelectedUnits)
		{
			if(selectedUnit.IsBuilding) {
				continue;
			}
			if(firstUnit == null) {
				firstUnit = selectedUnit;
			}
			if (AMove && Input.IsActionJustPressed("LeftClick"))
			{
				selectedUnit.MovementHistory.Clear();
				if(selectedUnit == firstUnit) {
					selectedUnit.MovementTarget = GetGlobalMousePosition();
				}
				if (selectedUnit == SelectedUnits.Last())
				{
					AMove = false;
					LastFrameAMove = 12;
				}
				selectedUnit.GetNode<NavigationAgent2D>("NavigationAgent2D").AvoidanceLayers = 1;
				selectedUnit.GetNode<NavigationAgent2D>("NavigationAgent2D").AvoidanceMask = 3;
				selectedUnit.AttackTarget = null;
				selectedUnit.HarvestingTarget = null;
				selectedUnit.PursueAttackTarget = true;
				selectedUnit.MovementTarget = selectedUnit.GlobalPosition + (GetGlobalMousePosition() - firstUnit.GlobalPosition);
				selectedUnit.StateManager.ChangeState(new StateAMove(selectedUnit.StateManager));
				selectedUnit.MovementHistory.Clear();

				
			}
			else if (Input.IsActionJustPressed("RightClick") && AbilityTargetedRightNow == null)
			{
				selectedUnit.MovementHistory.Clear();
				if (MouseCursor.TargetedUnit != null && MouseCursor.TargetedUnit.Team == 1)
				{
					if(selectedUnit.GetNode<NavigationAgent2D>("NavigationAgent2D").Radius > BiggestRadius) {
						BiggestRadius = selectedUnit.GetNode<NavigationAgent2D>("NavigationAgent2D").Radius;
					}
					if(selectedUnit == firstUnit) {
						selectedUnit.MovementTarget = MouseCursor.TargetedUnit.GlobalPosition;
						Pos = MouseCursor.TargetedUnit.GlobalPosition;
					}
					if(i % SelectedUnitsCountRoot == 0) {
						Pos.Y += BiggestRadius * 2;
						Pos.X = firstUnit.MovementTarget.X;
					}
					Pos.X += 1;
					Pos.Y += 1;
					selectedUnit.MovementTarget = Pos;
					Pos.X -= 1;
					Pos.Y -= 1;
					selectedUnit.GetNode<NavigationAgent2D>("NavigationAgent2D").AvoidanceLayers = 0;
					selectedUnit.GetNode<NavigationAgent2D>("NavigationAgent2D").AvoidanceMask = 2;
					Pos.X += selectedUnit.GetNode<NavigationAgent2D>("NavigationAgent2D").Radius * 2;
					selectedUnit.StateManager.ChangeState(new StateWalking(selectedUnit.StateManager));
				}
				else if(maxDistance > maxRadius * 15) {
					if(selectedUnit.GetNode<NavigationAgent2D>("NavigationAgent2D").Radius > BiggestRadius) {
						BiggestRadius = selectedUnit.GetNode<NavigationAgent2D>("NavigationAgent2D").Radius;
					}
					if(selectedUnit == firstUnit) {
						selectedUnit.MovementTarget = GetGlobalMousePosition();
						Pos = GetGlobalMousePosition();
					}
					if(i % SelectedUnitsCountRoot == 0) {
						Pos.Y += BiggestRadius * 2;
						Pos.X = firstUnit.MovementTarget.X;
					}
					Pos.X += 1;
					Pos.Y += 1;
					selectedUnit.MovementTarget = Pos;
					Pos.X -= 1;
					Pos.Y -= 1;
					selectedUnit.GetNode<NavigationAgent2D>("NavigationAgent2D").AvoidanceLayers = 0;
					selectedUnit.GetNode<NavigationAgent2D>("NavigationAgent2D").AvoidanceMask = 2;
					Pos.X += selectedUnit.GetNode<NavigationAgent2D>("NavigationAgent2D").Radius * 2;
					selectedUnit.StateManager.ChangeState(new StateWalking(selectedUnit.StateManager));
				}
				else {
					//if(selectedUnit == firstUnit) {
					//	selectedUnit.MovementTarget = GetGlobalMousePosition();
					//}
					if (MouseCursor.TargetedUnit != null && MouseCursor.TargetedUnit.Team > 1)
					{
						selectedUnit.AttackTarget = MouseCursor.TargetedUnit;
						selectedUnit.HarvestingTarget = null;
						selectedUnit.PursueAttackTarget = true;
						selectedUnit.GetNode<NavigationAgent2D>("NavigationAgent2D").AvoidanceLayers = 1;
						selectedUnit.GetNode<NavigationAgent2D>("NavigationAgent2D").AvoidanceMask = 3;
						selectedUnit.StateManager.ChangeState(new StatePursuit(selectedUnit.StateManager));
					}
					else if (MouseCursor.TargetedUnit is ResourceDeposit rd && selectedUnit.IsHarvester)
					{
						selectedUnit.HarvestingTarget = rd;
						selectedUnit.AttackTarget = null;
						selectedUnit.PursueAttackTarget = false;
						selectedUnit.StateManager.ChangeState(new StateHarvesting(selectedUnit.StateManager));
					}
					else
					{
						selectedUnit.AttackTarget = null;
						selectedUnit.HarvestingTarget = null;
						selectedUnit.PursueAttackTarget = false;
						selectedUnit.StateManager.ChangeState(new StateWalking(selectedUnit.StateManager));
					}

					if (selectedUnit.ConstructionTarget != null && 
					 (selectedUnit.ConstructionTarget.StateManager.CurrentState is StateConstructionPlacement 
					 || selectedUnit.ConstructionTarget.StateManager.CurrentState is StateConstructionPlanned))
					{
						Units = Units.Except(new Unit[] { selectedUnit }).ToArray();
						selectedUnit.ConstructionTarget.QueueFree();
						selectedUnit.ConstructionTarget = null;
					}

					selectedUnit.MovementTarget = selectedUnit.GlobalPosition + (GetGlobalMousePosition() - center);
					selectedUnit.GetNode<NavigationAgent2D>("NavigationAgent2D").AvoidanceLayers = 1;
					selectedUnit.GetNode<NavigationAgent2D>("NavigationAgent2D").AvoidanceMask = 3;
				}
			}
			i++;
		}

		if(AbilityTargetedRightNow != null && AbilityTargetedRightNow is Build buildAbility) {
			if(buildAbility.Building == null) {
				Unit u = buildAbility.BuildingPrefab.Instantiate<Unit>();
				if(u is Unit unit) {
					AddChild(unit);
					unit.Builder = buildAbility.GetParent<Unit>();
					unit.StateManager = new StateManager(unit);
					unit.StateManager.ChangeState(new StateConstructionPlacement(unit.StateManager));
					Vector2 pos = GetGlobalMousePosition();
					unit.GlobalPosition = pos;
					unit.MovementTarget = pos;
					unit.ConstructionState = -1;
					buildAbility.Building = unit;
					unit.Builder.ConstructionTarget = unit;
				}
			}
			else {
				Vector2 pos = GetGlobalMousePosition();
				buildAbility.Building.GlobalPosition = pos;
				buildAbility.Building.MovementTarget = pos;
			}
			
		}

		if(Input.IsActionJustPressed("a")) {
			AMove = true;
		}
		
		if(Input.IsActionJustReleased("RightClick")) {
			AbilityTargetedRightNow = null;
			AMove = false;
			Gold += 50;
			Wood += 50;
		}
    }

    public void OnAreaSelected(Area2D area) {
		if(LastFrameAMove > 0) {
			return;	
		}
		if(UI.Instance.IsMouseOverCommandCard || AMove) {
			return;
		}

		if(AbilityTargetedRightNow != null) {
			if(AbilityTargetedRightNow.IsTargetUnit) {
				AbilityTargetedRightNow.UnitTarget = MouseCursor.TargetedUnit;
			}
			else {
				AbilityTargetedRightNow.Target = GetGlobalMousePosition();
			}
			AbilityTargetedRightNow = null;
			return;
		}

		LastUnitSelected = null;

		if(!Input.IsActionPressed("Shift")) {
			foreach(Unit u in Units) {
				u.SetSelected(false);
				SelectedUnits.Clear();
			}
		}

		UI.Instance.ClearCommandCard();

		int notBuildingsCount = 0;

		foreach(Unit u in area.GetOverlappingBodies()) {
			if(u.Team == 1 && u.ConstructionState == 2 && u.Visible) {
				u.SetSelected(true);
				SelectedUnits.Add(u);
				LastUnitSelected = u;
				if(!u.IsBuilding) {
					notBuildingsCount++;
				}
			}
		}

		List<Unit> UnitsToDeselect = new List<Unit>();

		if(notBuildingsCount > 0) {
			foreach(Unit u in SelectedUnits) {
				if(u.IsBuilding) {
					u.SetSelected(false);
					UnitsToDeselect.Add(u);
				}
				else {
					LastUnitSelected = u;
				}
			}
			foreach(Unit u in UnitsToDeselect) {
				SelectedUnits.Remove(u);
			}
		}
		
		
		if(LastUnitSelected != null) {
			LastUnitSelected.CreateCommandCard();
		}
	}

	public Unit[] GetUnitsInArea(Tuple<Vector2, Vector2> area) {
		Unit[] units = Array.Empty<Unit>();
		foreach(Unit unit in Units) {
			if(unit.Position.X > area.Item1.X && unit.Position.X < area.Item2.X) {
				if(unit.Position.Y > area.Item1.Y && unit.Position.Y < area.Item2.Y) {
					units = units.Append(unit).ToArray();
				}
			}
		}
		return units;
	}

	public Unit[] GetUnitsInRadius(Vector2 pos, float radius) {
		Unit[] units = Array.Empty<Unit>();
		foreach(Unit unit in Units) {
			if(unit.GlobalPosition.DistanceTo(pos) < radius) {
				units = units.Append(unit).ToArray();
			}
		}
		return units;
	}

	public Unit NearestResourceDropoffPosition(Vector2 startPos)
	{
		return Units
			.Where(unit => unit.IsResourceDropOff)
			.MinBy(unit => startPos.DistanceTo(unit.GlobalPosition));
	}
}
