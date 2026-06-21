using UnityEngine;
using UnityEngine.InputSystem; 

public class HideoutState : IGameState
{
    public void EnterState(GameStateManager manager)
    {
        Debug.Log("Entered Hideout. Enabling UI and passive production.");
        Debug.Log("PRESS SPACEBAR TO TEST ARENA TRANSITION");
    }

    public void UpdateState(GameStateManager manager)
    {
        // Reminder: REMOVE THIS DEBUG TRIGGER ONCE UI BUTTONS ARE READY
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            manager.LoadSceneAndChangeState("02_Arena_Sandbox", new ArenaState());
        }
    }

    public void ExitState(GameStateManager manager)
    {
        Debug.Log("Leaving Hideout...");
    }
}