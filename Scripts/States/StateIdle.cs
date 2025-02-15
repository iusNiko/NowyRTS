using Godot;
using System;
using System.Linq;

public partial class StateIdle : State
{
    //Properties

    //Methods
    public StateIdle(StateManager stateManager) : base(stateManager) {}

    public override void OnEnter() {
        StateManager.Unit.Animation.Play("idle");
        
    }

    public override void OnExit() {
        
    }

    public override void OnUpdate(float delta) {
        Unit unit = StateManager.Unit;
        Unit at = null;

        unit.GetNode<NavigationAgent2D>("NavigationAgent2D").AvoidanceLayers = 1;
		unit.GetNode<NavigationAgent2D>("NavigationAgent2D").AvoidanceMask = 3;

        if(World.Instance.Frame % (unit.RandomFrameModifier + 1) == 0 && !unit.IsBuilding) {
            at = unit.SearchTarget();
        }
        else {
            at = null;
        }

        if(at != null && StateManager.Unit.GlobalPosition.DistanceTo(at.GlobalPosition) < StateManager.Unit.AttackTargetPursueRange) {
            unit.AttackTarget = at;
            StateManager.ChangeState(new StatePursuit(StateManager));
        }

        if(unit.AbilityQueue.Count > 0) {
			unit.AbilityQueue.First().TimeInQueue += delta;
			if(unit.AbilityQueue.First().TimeInQueue >= unit.AbilityQueue.First().QueueTime) {
				unit.AbilityQueue.Dequeue().Execute();
				if(unit.AbilityQueue.Count > 0) {
					unit.AbilityQueue.First().TimeInQueue = 0; // We have to make sure to reset TimeInQueue, because the same ability might be queued multiple times
				}
			}
		}
    }
}