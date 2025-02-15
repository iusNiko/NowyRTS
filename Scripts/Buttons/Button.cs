using Godot;
using System;

public partial class Button : TextureRect
{
	public Ability Ability;
	public bool IsCancelButton = false;

	public virtual void ButtonUsed(bool ShortcutUsed) {
		if(ShortcutUsed && Ability.EnableQuickcast) {
			if(!Ability.CanAfford()) {
				return;
			}
			if(IsCancelButton) {
				return;
			}

			if(Ability.IsAbilityToBeQueued) {
				if(Ability.AddToQueue()) {
					Ability.DeductCostsFromPlayer();
				}
			}
			else if(Ability.IsAbilityWithTarget) {
				Ability.Target = World.Instance.GetGlobalMousePosition();
				if(Ability.IsTargetUnit) {
					if(World.Instance.MouseCursor.TargetedUnit != null) {
						Ability.UnitTarget = World.Instance.MouseCursor.TargetedUnit;
					}
					else {
						return;
					}
				}
			}
			else {
				if(Ability.Execute()) {
					Ability.DeductCostsFromPlayer();
				}
			}
		}
		else {
			if(!Ability.CanAfford()) {
				return;
			}
			if(IsCancelButton) {
				return;
			}

			if(Ability.IsAbilityToBeQueued) {
				if(Ability.AddToQueue()) {
					Ability.DeductCostsFromPlayer();
				}
			}
			else if(Ability.IsAbilityWithTarget) {
				World.Instance.AbilityTargetedRightNow = Ability;
			}
			else {
				if(Ability.Execute()) {
					Ability.DeductCostsFromPlayer();
				}
			}
		}
		
	}
    public override void _GuiInput(InputEvent @event)
    {
		if(@event is InputEventMouseButton) {
			if(Input.IsActionJustReleased("LeftClick")) {
				ButtonUsed(false);
			}
		}
    }

    public override void _PhysicsProcess(double delta) {
		if(Input.IsActionJustPressed(Ability.Shortcut)) {
			ButtonUsed(true);
		}
    }
}
