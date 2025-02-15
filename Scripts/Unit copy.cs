/*
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Unit : CharacterBody2D
{
	public bool Selected = false;
	public Panel SelectionBox;
	public Marker2D ProjectileSpawn;
	public AnimatedSprite2D Animation;
	public StateManager StateManager;
	[Export] public float MaxLife = 100f;
	[Export] public float Life = 100f;
	[Export] public float MovementSpeed = 100f;
	public int NoMovementFrames = 0;
	Vector2 LastFramePos;
	private NavigationAgent2D _navigationAgent;
	public Vector2 MovementTarget;
	public Vector2 LastMovementTarget = Vector2.Zero;
	public List<Vector2> MovementHistory = new();
	[Export] int MovementHistoryLength = 5;
	[Export] public int Team = 1;
	public Unit AttackTarget = null;
	[Export] public float AttackDamage = 10f;
	[Export] public float ProjectileSpeed = 100f;
	[Export] public float AttackRange = 16f;
	[Export] public float AttackCooldown = 1f;
	[Export] public float AttackTargetPursueRange = 150f;
	public float TimeSinceLastAttack = 0f;
	public bool PursueAttackTarget = false;
	[Export] PackedScene ProjectilePrefab;
	bool IsFacingRight = true;
	public Ability[] Abilities = Array.Empty<Ability>();
	[Export] public int AbilityQueueLength = 5;
	public Queue<Ability> AbilityQueue = new();
	public Queue<Ability> AbilityShiftQueue = new();
	public Unit ConstructionTarget = null;
	[Export] public bool IsBuilding = false;
	public int ConstructionState = 2; // -1 - Ghost, 0 - Planned, 1 - Under Construction, 2 - Fully Constructed
	public bool CanBeBuilt = true;
	public Unit Builder = null;
	[Export] public float ConstructionTime = 10f;
	[Export] public float TimeSinceConstructionStart = 0f;
	[Export] public bool IsResourceDropOff = false;
	[Export] public bool IsHarvester = false;
	public ResourceDeposit HarvestingTarget = null;
	[Export] public float HarvestingDistance = 16;
	[Export] public float HarvestCooldown = 1f;
	[Export] public float TimeSinceLastHarvest = 0f;
	[Export] public float HarvestAmountPerCycle = 5f;
	[Export] public float MaxResourceCapacity = 15f;
	public float GoldHarvested = 0f;
	public float WoodHarvested = 0f;
	public Unit DropoffTarget = null;
	public bool MovingTowardsResourceDropoff = false;
	[Export] public float ResourceDropoffRange = 24f;
	public int RandomFrameModifier = -1;
	public float Delta = 0;

	 private async void ActorSetup()
    {
        // Wait for the first physics frame so the NavigationServer can sync.
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        // Now that the navigation map is no longer empty, set the movement target.
        _navigationAgent.TargetPosition = GlobalPosition;
    }

    public override void _Ready()
    {
		_navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
		_navigationAgent.PathDesiredDistance = 4.0f;
        _navigationAgent.TargetDesiredDistance = 4.0f;
		_navigationAgent.MaxSpeed = MovementSpeed;
		if(!IsBuilding) {
			_navigationAgent.VelocityComputed += Movement;
		}
        SelectionBox = GetNode<Panel>("SelectionBox");
		ProjectileSpawn = GetNode<Marker2D>("ProjectileSpawn");
		Animation = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		SetSelected(Selected);
		MovementTarget = GlobalPosition;
		LastFramePos = GlobalPosition;

		Callable.From(ActorSetup).CallDeferred();
    }

	public virtual void LateReady() {
		if(IsBuilding) {
			Animation.Play("idle");
			Animation.Modulate = new Color(1, 1, 1, 1f);
			GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
			if(Builder != null) {
				Builder.Visible = true;
				Builder.ConstructionTarget = null;
				Builder = null;
			}
		}
		
		RandomFrameModifier = World.Instance.RNG.RandiRange(0, 5);
		World.Instance.Units = World.Instance.Units.Append(this).ToArray();

		if(Team == 0) {
			World.Instance.NeutralUnits = World.Instance.NeutralUnits.Append(this).ToArray();
		}
		else if(Team == 1) {
			World.Instance.PlayerUnits = World.Instance.PlayerUnits.Append(this).ToArray();
		}
		else if(Team > 1) {
			World.Instance.EnemyUnits = World.Instance.EnemyUnits.Append(this).ToArray();
		}
	}

	//This method pupulates command card with unit's abilities
	public virtual void CreateCommandCard() {
		foreach(Ability ability in Abilities) {
			ability.CreateButton();
		}
	}

	public virtual void SetSelected(bool value) {
		SelectionBox.Visible = value;
		Selected = value;
	}

	public virtual Unit SearchTarget() {
		Unit closestUnit = null;
		float distanceSquaredToClosestUnit = float.MaxValue;

		if(Team == 0) {
			return null;
		}
		else if(Team == 1) {
			for(int i = 0; i < World.Instance.EnemyUnits.Length; i++) {
				Unit unit = World.Instance.EnemyUnits[i];
				if(unit.Team != 0 && (GlobalPosition.DistanceSquaredTo(unit.GlobalPosition) < distanceSquaredToClosestUnit)) {
					closestUnit = unit;
					distanceSquaredToClosestUnit = GlobalPosition.DistanceSquaredTo(unit.GlobalPosition);
				}
			}
			
		}
		else if(Team > 1) {
			for(int i = 0; i < World.Instance.PlayerUnits.Length; i++) {
				Unit unit = World.Instance.PlayerUnits[i];
				if(unit.Team != 0 && (GlobalPosition.DistanceSquaredTo(unit.GlobalPosition) < distanceSquaredToClosestUnit)) {
					closestUnit = unit;
					distanceSquaredToClosestUnit = GlobalPosition.DistanceSquaredTo(unit.GlobalPosition);
				}
			}
			
		}
		
		return closestUnit;
	}

	public virtual void PlayAnimation(string name) {
		if(Animation.Animation == "walk"|| Animation.Animation == "idle" || !Animation.IsPlaying()) {
			Animation.Play(name);
		}
	}

	public virtual void OnDeath() {
		World.Instance.Units = World.Instance.Units.Except(new Unit[]{this}).ToArray();
		if(Team == 0) {
			World.Instance.NeutralUnits = World.Instance.NeutralUnits.Except(new Unit[]{this}).ToArray();
		}
		else if(Team == 1) {
			World.Instance.PlayerUnits = World.Instance.PlayerUnits.Except(new Unit[]{this}).ToArray();
		}
		else if(Team > 1) {
			World.Instance.EnemyUnits = World.Instance.EnemyUnits.Except(new Unit[]{this}).ToArray();
		}

		World.Instance.SelectedUnits.Remove(this);

		QueueFree();
	}

	public virtual void ChangefacingDirection(Vector2 movementTarget) {
		if(AttackTarget != null) {
			if(GlobalPosition.DirectionTo(AttackTarget.GlobalPosition).X >= 0) {
				Animation.FlipH = false;
			}
			else {
				Animation.FlipH = true;
			}
			return;
		}

		if(GlobalPosition.DirectionTo(movementTarget).X >= 0) {
			Animation.FlipH = false;
		}
		else {
			Animation.FlipH = true;
		}

		
	}

	public virtual void Movement(Vector2 safeVelocity) {
		Velocity = safeVelocity;
		GlobalPosition += Velocity * (float) Delta;
		//Movement - Push colliding units
		/*
		if(MovementTarget != GlobalPosition) {
			for(int i = 0; i < GetSlideCollisionCount(); i++) {
				if(GetSlideCollision(i).GetCollider() is Unit u) {
					if(u.Team == Team && !u.IsBuilding) {
						u.GlobalPosition -= GetSlideCollision(i).GetNormal() * MovementSpeed * Delta;
						u.MovementTarget -= GetSlideCollision(i).GetNormal() * MovementSpeed * Delta;
					}
				}
			}	
		}
		*/
		//-----------------------------------
		/*
	}

    public override void _PhysicsProcess(double delta)
    {
		StateManager.CurrentState.OnUpdate((float) delta);
		
		Delta = (float) delta;
		if(Animation.Animation == "attack") {
			Animation.SpeedScale = 1 / AttackCooldown;
		}
		else {
			Animation.SpeedScale = 1;
		}
		//Construction
		if(ConstructionState == -1)  {
			
			KinematicCollision2D collision = MoveAndCollide(Vector2.Zero, true);
			if(collision != null) {
				Animation.Modulate = new Color(1, 0, 0, 0.25f);
				CanBeBuilt = false;
			}
			else {
				Animation.Modulate = new Color(1, 1, 1, 0.25f);
				CanBeBuilt = true;
			}

			goto Skip;
		}
		if(ConstructionState == 0) {
			if(!CanBeBuilt) {
				QueueFree();
				return;
			}
			Animation.Modulate = new Color(1, 1, 1, 0.25f);
			if(Builder != null) {
				ConstructionState = 1;
				Builder.Visible = false;
				Builder.SetSelected(false);
				UI.Instance.ClearCommandCard();
			}
			goto Skip;
		}
		if(ConstructionState == 1) {
			Animation.Modulate = new Color(1, 1, 1, 1f);
			if(IsBuilding) Animation.Play("construction");
			if(Builder != null) {
				TimeSinceConstructionStart += (float) delta;
				if(TimeSinceConstructionStart >= ConstructionTime) {
					ConstructionState = 2;
					Builder.Visible = true;
					Builder.ConstructionTarget = null;
					Builder = null;
				}
			}
			goto Skip;
		}

		

		if(ConstructionTarget != null) {
			if(!IsInstanceValid(ConstructionTarget)) {
				ConstructionTarget = null;
				goto Skip;
			}
			if(GlobalPosition.DistanceTo(ConstructionTarget.GlobalPosition) <= 30) {
				ConstructionTarget.Builder = this;
				goto Skip;
			}
		}

		//-----------------

		/*

		if(World.Instance.AMove && Input.IsActionJustPressed("LeftClick") && Selected) {
			AttackTarget = null;
			HarvestingTarget = null;
			PursueAttackTarget = true;
			MovementTarget = GetGlobalMousePosition();
			ChangefacingDirection(MovementTarget);
			MovementHistory.Clear();
		}
		else if(Selected && Input.IsActionJustPressed("RightClick") && World.Instance.AbilityTargetedRightNow == null) {
			if(World.Instance.MouseCursor.TargetedUnit != null && World.Instance.MouseCursor.TargetedUnit.Team > 1) {
				AttackTarget = World.Instance.MouseCursor.TargetedUnit;
				HarvestingTarget = null;
				PursueAttackTarget = true;
			}
			else if(World.Instance.MouseCursor.TargetedUnit is ResourceDeposit rd && IsHarvester) {
				HarvestingTarget = rd;
				AttackTarget = null;
				PursueAttackTarget = false;
			}
			else {
				AttackTarget = null;
				HarvestingTarget = null;
				PursueAttackTarget = false;
			}

			if(ConstructionTarget != null) {
				World.Instance.Units = World.Instance.Units.Except(new Unit[]{this}).ToArray();
				ConstructionTarget.QueueFree();
				ConstructionTarget = null;
			}
			
			MovementTarget = GetGlobalMousePosition();
			ChangefacingDirection(MovementTarget);
			MovementHistory.Clear();
		}
		*/
/*

		//Init after _Ready
		if(RandomFrameModifier == -1) {
			LateReady();
		}
		//-----------------

		//Handle Abilities
		if(AbilityShiftQueue.Count > 0) {
			if(AbilityShiftQueue.First().IsCompleted()) {
				AbilityShiftQueue.Dequeue();
				if(AbilityShiftQueue.Count > 0) {
					AbilityShiftQueue.First().Execute();
				}
			}
		}

		if(AbilityQueue.Count > 0) {
			AbilityQueue.First().TimeInQueue += (float) delta;
			if(AbilityQueue.First().TimeInQueue >= AbilityQueue.First().QueueTime) {
				AbilityQueue.Dequeue().Execute();
				if(AbilityQueue.Count > 0) {
					AbilityQueue.First().TimeInQueue = 0; // We have to make sure to reset TimeInQueue, because the same ability might be queued multiple times
				}
			}
		}

		//-----------------------------------

		//MOVEMENT

		if(NoMovementFrames > 0) {
			NoMovementFrames--;
			MovementHistory.Clear();
			goto Skip;
		}

		Movement:

		if(!IsInstanceValid(AttackTarget)) AttackTarget = null;


		//Movement - Anti-Stutter

		float expectedOffset = MovementSpeed * (float) delta * MovementHistoryLength;
		float offset = 0;

		if(MovementHistory.Count >= MovementHistoryLength) {
			Vector2 vec = Vector2.Zero;
			foreach(Vector2 v in MovementHistory) {
				vec += v - MovementHistory[0];
			}
			offset = vec.Length();
		}

		//----------------------------------

		//Movement - Main

		if(IsBuilding) goto Attack;

		if(MovementHistory.Count >= MovementHistoryLength && offset < expectedOffset) {
			PlayAnimation("idle");
			NoMovementFrames = 10;
		}

		if(MovementTarget == GlobalPosition) {
			PursueAttackTarget = true;
			_navigationAgent.AvoidanceLayers = 1;
			_navigationAgent.AvoidanceMask = 0;
		}
		Vector2 movementTarget = MovementTarget;

		//Change MovementTarget based on other priority targets

		if(PursueAttackTarget && AttackTarget != null && GlobalPosition.DistanceTo(AttackTarget.GlobalPosition) <= AttackTargetPursueRange) {
			_navigationAgent.AvoidanceLayers = 1;
			_navigationAgent.AvoidanceMask = 1;
			if(GlobalPosition.DistanceTo(AttackTarget.GlobalPosition) <= AttackRange - 2) {
				movementTarget = GlobalPosition;
			}
			else {
				movementTarget = AttackTarget.GlobalPosition;
			}
			ChangefacingDirection(movementTarget);
		}

		if(HarvestingTarget != null && IsInstanceValid(HarvestingTarget)) {
			movementTarget = HarvestingTarget.GlobalPosition;
			ChangefacingDirection(movementTarget);
			_navigationAgent.AvoidanceLayers = 2;
			_navigationAgent.AvoidanceMask = 0;
		}

		if(ConstructionTarget != null) {
			movementTarget = ConstructionTarget.GlobalPosition;
			ChangefacingDirection(movementTarget);
		}

		if(MovingTowardsResourceDropoff) {
			DropoffTarget = World.Instance.NearestResourceDropoffPosition(GlobalPosition);
			if(DropoffTarget != null) {
				movementTarget = DropoffTarget.GlobalPosition;
				ChangefacingDirection(movementTarget);
			}
		}

		_navigationAgent.TargetPosition = movementTarget;

		if(!_navigationAgent.IsNavigationFinished()) {
			Vector2 currentAgentPosition = GlobalTransform.Origin;
        	Vector2 nextPathPosition = _navigationAgent.GetNextPathPosition();

			Velocity = currentAgentPosition.DirectionTo(nextPathPosition) * MovementSpeed;

			MovementHistory.Add(GlobalPosition);
			if(MovementHistory.Count > MovementHistoryLength) {
				MovementHistory.RemoveAt(0);
			}
			PlayAnimation("walk");
		}
		else {
			PlayAnimation("idle");
			Velocity = Vector2.Zero;
			if(!PursueAttackTarget) {
				MovementTarget = GlobalPosition;
			}
		}

		//GlobalPosition += Velocity * (float) delta;
		//MoveAndSlide();
		_navigationAgent.Velocity = Velocity;

		//-----------------------------------

		//Attack
		
		Attack:


		if(ProjectilePrefab != null) {
			TimeSinceLastAttack += (float) delta;

			AttackTarget = SearchTarget();

			if(AttackTarget != null && TimeSinceLastAttack > AttackCooldown && GlobalPosition.DistanceTo(AttackTarget.GlobalPosition) <= AttackRange) {

				ChangefacingDirection(MovementTarget);

				PlayAnimation("attack");

				TimeSinceLastAttack = 0;

				var projectile = ProjectilePrefab.Instantiate();
				
				if(projectile is HomingProjectile hp) {
					hp.Target = AttackTarget;
					hp.Source = this;
					if(AttackDamage != -1) hp.Damage = AttackDamage;
					if(ProjectileSpeed != -1) hp.Speed = ProjectileSpeed;
					World.Instance.AddChild(hp);
					hp.GlobalPosition = ProjectileSpawn.GlobalPosition;
				}
			}
		}

		//-----------------------------------

		//Harvesting Resources

		if(IsHarvester) {
			TimeSinceLastHarvest += (float) delta;

			if(HarvestingTarget != null && GlobalPosition.DistanceTo(HarvestingTarget.GlobalPosition) <= HarvestingDistance && TimeSinceLastHarvest > HarvestCooldown) {
				TimeSinceLastHarvest = 0;
				if(HarvestingTarget.GoldAmount > 0) {
					if(HarvestingTarget.GoldAmount >= HarvestAmountPerCycle) {
						GoldHarvested += HarvestAmountPerCycle;
						HarvestingTarget.GoldAmount -= HarvestAmountPerCycle;
					}
					else {
						GoldHarvested += HarvestingTarget.GoldAmount;
						HarvestingTarget.GoldAmount = 0;
					}
				}
				if(HarvestingTarget.WoodAmount > 0) {
					if(HarvestingTarget.WoodAmount >= HarvestAmountPerCycle) {
						WoodHarvested += HarvestAmountPerCycle;
						HarvestingTarget.WoodAmount -= HarvestAmountPerCycle;
					}
					else {
						WoodHarvested += HarvestingTarget.WoodAmount;
						HarvestingTarget.WoodAmount = 0;
					}
				}
			}

			if(GoldHarvested >= MaxResourceCapacity || WoodHarvested >= MaxResourceCapacity) {
				MovingTowardsResourceDropoff = true;
			}
			else{
				MovingTowardsResourceDropoff = false;
			}

			if(DropoffTarget != null && GlobalPosition.DistanceTo(DropoffTarget.GlobalPosition) <= ResourceDropoffRange) {
				World.Instance.Gold += GoldHarvested;
				GoldHarvested = 0;
				World.Instance.Wood += WoodHarvested;
				WoodHarvested = 0;
				MovingTowardsResourceDropoff = false;
			}

		}

		//-----------------------------------

		//Death

		if(Life <= 0) {
			OnDeath();
			return;
		}

		Skip:
		return;
    }
}
*/
