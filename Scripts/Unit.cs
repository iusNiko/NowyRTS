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
	[Export] public float _MaxLife = 100f;
	public float MaxLife {
		get {
			float life = _MaxLife;
			foreach(PropertyModifier property in PropertyModifiers) {
				if(property.Name == "MaxLife") {
					life += property.FlatModifier;
					life *= property.PercentageModifier;
				}
			}
			return life;
		}
		set {
			float lifeFactor = Life / _MaxLife;
			_MaxLife = value;
			Life = _MaxLife * lifeFactor;
		}
	}
	[Export] public float Life = 100f;
	[Export] public float _MovementSpeed = 100f;
	public float MovementSpeed {
		get {
			float speed = _MovementSpeed;
			foreach(PropertyModifier property in PropertyModifiers) {
				if(property.Name == "MovementSpeed") {
					speed += property.FlatModifier;
					speed *= property.PercentageModifier;
				}
			}
			return speed;
		}
		set {
			_MovementSpeed = value;
		}
	}
	public int NoMovementFrames = 0;
	Vector2 LastFramePos;
	public NavigationAgent2D _navigationAgent;
	public Vector2 MovementTarget;
	public Vector2 LastMovementTarget = Vector2.Zero;
	public float LastFrameMovementDistance = 0f;
	public List<float> MovementHistory = new();
	[Export] int MovementHistoryLength = 5;
	[Export] public int Team = 1;
	public Unit AttackTarget = null;
	[Export] public float _AttackDamage = 10f;
	public float AttackDamage {
		get {
			float damage = _AttackDamage;
			foreach(PropertyModifier property in PropertyModifiers) {
				if(property.Name == "AttackDamage") {
					damage += property.FlatModifier;
					damage *= property.PercentageModifier;
				}
			}
			return damage;
		}
		set {
			_AttackDamage = value;
		}
	}
	[Export] public float _ProjectileSpeed = 100f;
	public float ProjectileSpeed {
		get {
			float speed = _ProjectileSpeed;
			foreach(PropertyModifier property in PropertyModifiers) {
				if(property.Name == "ProjectileSpeed") {
					speed += property.FlatModifier;
					speed *= property.PercentageModifier;
				}
			}
			return speed;
		}
		set {
			_ProjectileSpeed = value;
		}
	}
	[Export] public float _AttackRange = 16f;
	public float AttackRange {
		get {
			float range = _AttackRange;
			foreach(PropertyModifier property in PropertyModifiers) {
				if(property.Name == "AttackRange") {
					range += property.FlatModifier;
					range *= property.PercentageModifier;
				}
			}
			return range;
		}
		set {
			_AttackRange = value;
		}
	}
	[Export] public float _AttackCooldown = 1f;
	public float AttackCooldown {
		get {
			float cooldown = _AttackCooldown;
			foreach(PropertyModifier property in PropertyModifiers) {
				if(property.Name == "AttackCooldown") {
					cooldown += property.FlatModifier;
					cooldown *= property.PercentageModifier;
				}
			}
			return cooldown;
		}
		set {
			_AttackCooldown = value;
		}
	}
	[Export] public float AttackTargetPursueRange = 150f;
	public float TimeSinceLastAttack = 0f;
	public bool PursueAttackTarget = false;
	[Export] public PackedScene ProjectilePrefab;
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
	[Export] public float _HarvestCooldown = 1f;
	public float HarvestCooldown {
		get {
			float cooldown = _HarvestCooldown;
			foreach(PropertyModifier property in PropertyModifiers) {
				if(property.Name == "HarvestCooldown") {
					cooldown += property.FlatModifier;
					cooldown *= property.PercentageModifier;
				}
			}
			return cooldown;
		}
		set {
			_HarvestCooldown = value;
		}
	}
	[Export] public float TimeSinceLastHarvest = 0f;
	[Export] public float _HarvestAmountPerCycle = 5f;
	public float HarvestAmountPerCycle {
		get {
			float amount = _HarvestAmountPerCycle;
			foreach(PropertyModifier property in PropertyModifiers) {
				if(property.Name == "HarvestAmountPerCycle") {
					amount += property.FlatModifier;
					amount *= property.PercentageModifier;
				}
			}
			return amount;
		}
		set {
			_HarvestAmountPerCycle = value;
		}
	}
	[Export] public float _MaxResourceCapacity = 15f;
	public float MaxResourceCapacity {
		get {
			float capacity = _MaxResourceCapacity;
			foreach(PropertyModifier property in PropertyModifiers) {
				if(property.Name == "MaxResourceCapacity") {
					capacity += property.FlatModifier;
					capacity *= property.PercentageModifier;
				}
			}
			return capacity;
		}
		set {
			_MaxResourceCapacity = value;
		}
	}
	public float GoldHarvested = 0f;
	public float WoodHarvested = 0f;
	public Unit DropoffTarget = null;
	public bool MovingTowardsResourceDropoff = false;
	[Export] public float ResourceDropoffRange = 24f;
	public int RandomFrameModifier = -1;
	public float Delta = 0;
	[Export] public string[] Flags = Array.Empty<string>();
	public PropertyModifier[] PropertyModifiers = Array.Empty<PropertyModifier>();

	public bool HasAllFlags(string[] flags) {
		foreach(string flag in flags) {
			if(!Flags.Contains(flag)) {
				return false;
			}
		}
		return true;
	}
	public bool IsAttackTargetValid() {
		if(IsInstanceValid(AttackTarget)) return true;
		else return false;
	}

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

		if(StateManager == null) {
			StateManager = new StateManager(this);
			StateManager.ChangeState(new StateIdle(StateManager));
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

		//Apply Augments

		foreach(Augment augment in World.Instance.Augments) {
			augment.OnUnitSpawn(this);
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
		if(Animation.Animation == "walk"|| Animation.Animation == "idle" || !Animation.IsPlaying() || Animation.Animation == "construction") {
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
		Vector2 oldTarget = _navigationAgent.TargetPosition;
		_navigationAgent.TargetPosition = GlobalPosition + safeVelocity * Delta;
		if(!_navigationAgent.IsTargetReachable()) {
			_navigationAgent.TargetPosition = GlobalPosition;
			_navigationAgent.Velocity = Vector2.Zero;
			Velocity = Vector2.Zero;
			MovementTarget = GlobalPosition;
			return;
		}
		else {
			_navigationAgent.TargetPosition = oldTarget;
		}
		Velocity = safeVelocity;
		GlobalPosition += Velocity * Delta;
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
	}

    public override void _PhysicsProcess(double delta)
    {
		Delta = (float) delta;
		TimeSinceLastAttack += (float) delta;
		TimeSinceLastHarvest += (float) delta;
		if(RandomFrameModifier == -1) {
			LateReady();
		}
		StateManager.CurrentState.OnUpdate((float) delta);

		if(Life <= 0) {
			StateManager.ChangeState(new StateDead(StateManager));
		}
    }
}