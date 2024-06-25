using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GAMEMODE
{
    ADVENTURE,
    TURNBASED
}

  //manages playmode for each player individually, depending on surroundings
  //to alter gamemode change the rules

public class GamemodeManager : MonoBehaviour
{
    public GameeventGenerator gameeventGenerator;
    public GameeventController activeGameevent;
    public GameRules gameRules;

    void Start()
    {
        gameRules = new();
        gameeventGenerator = new();
        gameeventGenerator.CreateEvent(new EventsSwitchToAdventure());

        Debug.Log($"Gamemode: {gameRules.Gamemode.ToString()}");
    }

    // Update is called once per frame
    void Update()
    {
        //Debug
        if (Input.GetMouseButtonDown(1))
        {
            GameManager.manager.worldManager.player.RemoveCurrentSelectableTiles();
            
            if (gameRules.FreeMove) 
                gameeventGenerator.CreateEvent(new EventsSwitchToTurnBased());
            else gameeventGenerator.CreateEvent(new EventsSwitchToAdventure());

            Debug.Log($"Gamemode: {gameRules.Gamemode.ToString()}");
        }

        //switch statement changing to appropriate gamemode
        switch (gameRules.Gamemode)
        { 
            case GAMEMODE.ADVENTURE:

                // when npcs are roaming they will use all their action points and
                // recharge at a rate of 1regen per sec, then wait a random amount of time below 5 seconds
                // npc's don't normally roam during other gamemodes

                break;

            case GAMEMODE.TURNBASED:

                // turn based consults player first never skipping their turn,
                // events are needed to start Turn based encounters /debug turnbased initially with manual switching 
                //NOTE : turns are determined(probably in a manager method) by the character's
                //action rate and so are certain skills like the walk skill range,
                //never walking more in a turn for them to reduce exhaustion and
                //being at peak action ability at the start of each turn



                //if running and not in any unique events or dialogues and is
                //outside battle range change gamemode to adventure
                break;
        }
    }
}

public class GameRules
{
    private bool freeMove;
    private GAMEMODE gamemode;

    public bool FreeMove { get; }
    public GAMEMODE Gamemode { get; }

    public void FreeMoveActive(bool yesOrNo)
    { 
        freeMove = yesOrNo;
    }

    public void SwitchGamemode(GAMEMODE gamemode)
    {
        this.gamemode = gamemode;
    }
}

///Depricated code
///// the ol' Gamemode switcher, when there was only two
/*   gameRules.FreeMoveActive(!gameRules.FreeMove);

   int currentgamemodenumeraldebug = (int)gameRules.Gamemode;

   if (currentgamemodenumeraldebug < 1) 
       gameRules.SwitchGamemode((GAMEMODE)currentgamemodenumeraldebug++);
   else gameRules.SwitchGamemode((GAMEMODE)currentgamemodenumeraldebug--);
*/
