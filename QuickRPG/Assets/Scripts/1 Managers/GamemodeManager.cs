using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum GAMEMODE
{
    ADVENTURE,
    TURNBASED
}

  //manages playmode for each player individually, depending on surroundings

public class GamemodeManager : MonoBehaviour
{
    public GameRules gameRules;

    private GAMEMODE gamemode;

    void Start()
    {
        gameRules = new();
        gameRules.FreeMoveActive(true);
        gamemode = GAMEMODE.ADVENTURE;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log($"Freemove: {gameRules.FreeMove}");

        //Debug
        if (Input.GetMouseButtonDown(1))
        {
            gameRules.FreeMoveActive(!gameRules.FreeMove);

            GameManager.manager.levelManager.player.RemoveCurrentSelectableTiles();

            if ((int)gamemode < 1) gamemode++;
            else gamemode--;
        }

        //switch statement changing to appropriate gamemode
        switch (gamemode)
        { 
            case GAMEMODE.ADVENTURE:

                gameRules.FreeMoveActive(true);

                AdventureModeUpdate();

                break;

            case GAMEMODE.TURNBASED:

                TurnBasedModeUpdate();

                //if running and not in any unique events or dialogues and is
                //outside battle range switch back to AdventureMode
                break;
        }
    }

    
    private void AdventureModeUpdate()
    {
        // when npcs are roaming they will use all their action points and
        // recharge at a rate of 1regen per sec, then wait a random amount of time below 5 seconds
        // npc's don't normally roam during other gamemodes

    }
    private void TurnBasedModeUpdate() // this is usually when something important is happening
    {
        // turn based consults player first never skipping their turn,
        // events are needed to start Turn based encounters /debug turnbased initially with manual switching 
        //NOTE : turns are determined(probably in a manager method) by the character's
        //action rate and so are certain skills like the walk skill range,
        //never walking more in a turn for them to reduce exhaustion and
        //being at peak action ability at the start of each turn
    }
}

public class GameRules
{
    private bool freeMove;

    public bool FreeMove { get; }

    public void FreeMoveActive(bool yesOrNo)
    { 
        freeMove = yesOrNo;
    }
}
