using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using UnityEngine;

// controls elements specific to this game session i.e:
// player's character menus(often changing), npcs and some interactive behaviours

public class WorldManager : MonoBehaviour
{
    //handle character spawning in Generator and utilize generated map
    //create these things if the world is in Gameplay mode,
    //Generated Maps could be saved

    private WorldGenerator worldGenerator;
    private GameeventGenerator gameeventGenerator;
    private GameWorld world;

    public PlayerController player;
    public GameObject StartingCharacter; //base character others are loaded over


    public void Awake()
    {
        worldGenerator = new WorldGenerator();
        gameeventGenerator = new GameeventGenerator();
        world = new GameWorld();

        world


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

public class GameWorld : MonoBehaviour
{ 

}