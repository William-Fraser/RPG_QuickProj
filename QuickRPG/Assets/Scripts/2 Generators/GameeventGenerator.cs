using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameeventGenerator : MonoBehaviour
{
    //create events
    //event's contain their own info and manage themselves within the gamemode once active,
    //Dialogue is a special event that can be chained together usually read from a file, though could be input in unity
    //
    void Start()
    {
        //init
    }

    public GameeventController[] CreateEvents(int amount)
    {
        GameEvents[] events = new GameEvents[amount];
        GameeventController[] executableEvents = new GameeventController[amount];

        // assign random events?

        return executableEvents;
    }

    public GameeventController CreateEvent(GameEvents gameEvent)
    {
        GameeventController executableEvent = new(gameEvent);
        Debug.Log($"Event Created {gameEvent.ToString()}");

        return executableEvent;
    }
    
    //dialog creation goes here
}
