using Godot;
using System;

public partial class StateConstructionPlacement : State {
    //Properties

    //Methods
    public StateConstructionPlacement(StateManager stateManager) : base(stateManager) {}

    public override void OnEnter() {
        StateManager.Unit.PlayAnimation("idle");
        StateManager.Unit.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
        GD.Print("Construction placement");
    }

    public override void OnExit() {
        
    }

    public override void OnUpdate(float delta) {
        Unit unit = StateManager.Unit;

        unit.GlobalPosition = World.Instance.GetGlobalMousePosition();
        
        KinematicCollision2D collision = unit.MoveAndCollide(Vector2.Zero, true);
		if(collision != null) {
			unit.Animation.Modulate = new Color(1, 0, 0, 0.25f);
			unit.CanBeBuilt = false;
		}
		else {
			unit.Animation.Modulate = new Color(1, 1, 1, 0.25f);
			unit.CanBeBuilt = true;
		}

        if(Input.IsActionJustPressed("LeftClick") && unit.CanBeBuilt) {
            StateManager.ChangeState(new StateConstructionPlanned(StateManager));
            StateManager.Unit.Builder.StateManager.ChangeState(new StateGoToConstructionSite(StateManager.Unit.Builder.StateManager));
        }
    }
}