using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using UnityEngine;

// controls elements specific to this game session i.e:
// player menus(often changing), npcs and some interactive behaviours

public class WorldManager : MonoBehaviour
{
    //handle character spawning in Generator and utilize generated map
    //create these things if the world is in Gameplay mode,
    //Generated Maps could be saved

    private WorldGenerator worldGenerator;

    public GameObject StartingCharacter; //base character others are loaded over

    public PlayerController player;


    public void Awake()
    {
        // create a new root object for the world to generate in after generation the
        // world data should be saved so it can be loaded

        // when generating the world be sure to update the tile data to include 
        // player spawning tiles based on how many are starting, for new players
        // joining there will be options for the host to choose how they spawn

        // create player character in the worldobject and assign them to the player
        // field, this should get saved automatically and get loaded into the
        // player character whenever they're reloaded
    }

    public void SetPlayerObject(PlayerController player)
    {
        player.transform.parent = transform;
        this.player = player;
    }

}