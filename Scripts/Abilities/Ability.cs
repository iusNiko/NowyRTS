using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Ability : Node
{
	[Export] public Texture2D ButtonTexture;
	[Export] public string Shortcut = "none";
	[Export] public bool HideButton = false;
	[Export] public bool EnableQuickcast = false;
	[Export] public bool IsAbilityToBeQueued = false;
	[Export] public float QueueTime = 2f;
	public float TimeInQueue = 0f;
	[Export] public float Cooldown = 0f;
	[Export] public bool IsAbilityWithTarget = false;
	[Export] public bool IsTargetUnit = false;
	public Vector2 Target = Vector2.Zero;
	public Unit UnitTarget = null;
	public float TimeSinceUsed = 0f;
	[Export] float GoldCost = 0f;
	[Export] float WoodCost = 0f;


    public override void _Ready()
    {
        if(GetParent() is Unit unit) {
			unit.Abilities = unit.Abilities.Append<Ability>(this).ToArray();
		}
    }

    public override void _Process(double delta)
    {
        TimeSinceUsed += (float) delta;

		if(IsAbilityWithTarget && Target != Vector2.Zero && !IsTargetUnit) {
			if(Execute()) {
				DeductCostsFromPlayer();
			}
			Target = Vector2.Zero;
		}
		if(IsAbilityWithTarget && IsTargetUnit && UnitTarget != null) {
			if(Execute()) {
				DeductCostsFromPlayer();
			}
			UnitTarget = null;
		}
    }

	public virtual bool CanAfford() {
		if(World.Instance.Gold < GoldCost) return false;
		else if(World.Instance.Wood < WoodCost) return false;

		return true;
	}

	public virtual void DeductCostsFromPlayer() {
		World.Instance.Gold -= GoldCost;
		World.Instance.Wood -= WoodCost;
	}

    public virtual void CreateButton() {
		Vector2I pos = UI.Instance.GetNearestEmptyButtonSpace();
		Button button = new();
		button.Texture = ButtonTexture;
		button.Ability = this;
		UI.Instance.CommandCard.AddChild(button);
		
		if(HideButton) {
			pos.X += 5000;
		}
		else {
			UI.Instance.CommandCardButtons[pos.X, pos.Y] = button;
		}
		
		
		button.Position = UI.Instance.ButtonPosToCommandCardPos(pos);
		
	}

	public virtual void CreateQueueButton(Vector2I pos) {
		Button button = new();
		button.Texture = ButtonTexture;
		button.Ability = this;
		button.IsCancelButton = true;
		UI.Instance.QueueUI.AddChild(button);
		button.Position = pos;
	}

	public virtual void Effect() {

	}

    public virtual bool Execute() {
		if(TimeSinceUsed >= Cooldown || IsAbilityToBeQueued) {
			TimeSinceUsed = 0;
			if(IsAbilityWithTarget && IsTargetUnit) {
				GD.Print("Ability " + Name + " of unit " + GetParent().Name + " executed on " + UnitTarget.Name);
			}
			else  if(IsAbilityWithTarget) {
				GD.Print("Ability " + Name + " of unit " + GetParent().Name + " executed on " + Target);
			}
			else {
				GD.Print("Ability " + Name + " of unit " + GetParent().Name + " executed");
			}

			Effect();
			return true;
		}
		else {
			return false;
		}
	}

	public virtual bool AddToQueue() {
		if(GetParent() is Unit unit) {
			if(unit.AbilityQueue.Count < unit.AbilityQueueLength) {
				unit.AbilityQueue.Enqueue(this);
				GD.Print("Ability " + Name + " of unit " + GetParent().Name + " added to queue");
				TimeInQueue = 0;
				return true;
			}
		}
		return false;
	}

	public virtual bool IsCompleted() {
		return true;
	}
}
