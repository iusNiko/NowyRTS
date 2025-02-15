using Godot;
using System;

public partial class StateManager
{
    //Properties

    public Unit Unit;
    public State CurrentState;

    //Methods

    public StateManager(Unit unit) {
        Unit = unit;
    }
    
    public void ChangeState(State newState) {
        if(CurrentState != null) {
            CurrentState.OnExit();
        }
        CurrentState = newState;
        CurrentState.OnEnter();
    }
}

