using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameState {
    public abstract GameState Transform(GameStateMachine GSM);    
}

class StartState : GameState
{
    public override GameState Transform(GameStateMachine GSM)
    {
        if (Input.GetKeyDown(KeyCode.I))
            return new InsertState();

        throw new System.NotImplementedException();
    }
        
}

class InsertState : GameState
{
    public override GameState Transform(GameStateMachine GSM)
    {
        throw new System.NotImplementedException();
    }
}

class DropState : GameState
{
    public override GameState Transform(GameStateMachine GSM)
    {
        throw new System.NotImplementedException();
    }
}
public class GameStateMachine
{
    
}