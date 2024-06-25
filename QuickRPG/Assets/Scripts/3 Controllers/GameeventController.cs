using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class GameeventController : MonoBehaviour
{
    //event controller hosts an event when activated then removes itself from gameplay
    private GameEvents GEvent;

    public GameeventController()
    { 
        // // blank controller used by characters to call active events
    }

    public GameeventController(GameEvents gameEvent)
    {   // // code utilized controller made for specific events that control the game
        GEvent = gameEvent;
    }

    void Update()
    {
        if (GEvent.endEvent) { Debug.Log($"Event Ended {GEvent.ToString()}"); Destroy(gameObject); }
    }
}

#region GEvents 
/// Event Rules
// events are non-specific, not made for any specific player or character
// works as a force within the world or as a governing rule of the game
// events always start with their contrusctor, creating controller to call events
// mode switch will end after switching,
// dialogue will end after dialogue is displayed,
// menus, picking up items, etc.
// every event will play and end after activating.
public class GameEvents
{
    protected GameeventController GECon;
    // handle event specific stuff
    public bool inactive;
    public bool endEvent;

    public GameEvents()
    { 
        inactive = true; 
    }

    public GameEvents(GameeventController gameeventController)
    {
        GECon = gameeventController;
        inactive = false;
    }

    virtual public void ActEvent() { if (inactive) return; }
}
/// action calls, happens to game server, labeled with 'Events'
public class EventsSwitchToTurnBased : GameEvents
{
    public EventsSwitchToTurnBased()
    {
        GameRules daRules = GameManager.manager.gamemodeManager.gameRules;

        daRules.FreeMoveActive(false);
        daRules.SwitchGamemode(GAMEMODE.TURNBASED);

        endEvent = true;
    }
}

public class EventsSwitchToAdventure : GameEvents
{
    public EventsSwitchToAdventure()
    {
        GameRules daRules = GameManager.manager.gamemodeManager.gameRules;

        daRules.FreeMoveActive(true);
        daRules.SwitchGamemode(GAMEMODE.ADVENTURE);

        endEvent = true;
    }
}

/// element, non-labeled happens just to players involved mostly clientside with some server interaction
//something that is waiting to happen can also be used as a call,
//character gains something or spawns or explodes keeping things simple like interaction or spawnning or use skill on
public class TurnTileDifferentColour : GameEvents 
{
    public void ActEvent(BaseTile tile)
    {
        base.ActEvent();

        GameManager.manager.levelManager.SetObjectColor(tile.TileObject, Color.yellow);
    }
}
#endregion