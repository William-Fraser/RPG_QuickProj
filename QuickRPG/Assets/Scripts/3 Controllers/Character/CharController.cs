using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public abstract class CharController : MonoBehaviour
{
    // abstract object base for output controllers

    protected enum STATE
    { 
        IDLE, //used in turns, the character is out of actions and is waiting for the next turn
        READY, //the character is deciding on what to do, a form of IDLE when there are available actions
        MOVING //the character is using the move function
    }

    //NOTE : turns are determined(probably in a manager method) by the character's action rate and so are certain skills like the walk skill range,
    //never walking more in a turn for them to reduce exhaustion and being at peak action ability at the start of each turn

    [SerializeField] protected GameObject characterObject;
    [SerializeField] protected GameObject modelObject;
    [Space]

    [SerializeField] protected Statistics stats;

    protected STATE state;
    private static NavMeshAgent navAgent;
    protected BaseTile currentTile;
    protected BaseTile[] selectableTiles;

    void Awake()
    {
        RaycastHit hit;
        Physics.Raycast(modelObject.transform.position, Vector3.down, out hit);

        currentTile = hit.transform.gameObject.GetComponent<TileController>().Tile;
        //check for model postition, if not at local.zero send error.
        if (modelObject.transform.localPosition != Vector3.zero)
            Debug.LogError($"{characterObject.transform.name} desynced model");
        
        //pass object references
        navAgent = modelObject.GetComponent<NavMeshAgent>();

        //set up stats for undefined characters, for characters that dont choose a path
    }

    // Moving within specified range
    protected void Move(Vector3 pos)// Move to specified position; can initiate turn without cost of action
    {
        navAgent.SetDestination(pos);
    }
    void FaceDir(Vector3 dir)// face direction at the end of move; costs 1 action
    {
        
    }
    void Target(Vector3 target)// focus on and face object in view; costs 1 action
    { 
        // maybe use lookat
    }
    // NPCs often follow players
    void MoveNPC()
    {
        Vector3 pos;
        pos = Vector3.zero;

        //calculate movement pos

        Move(pos);
    }

    // *Acting : handled by a manager taking turns recalculated between each turn
    // types are UI for players, AI for NPC's, UI and AI for allies(decisions made by AI influenced by UI),
    // e.g. Summoned creatures can have player controllers, characters with no influence have NPC control and team mates have Ally
    
    // Attacking, giving and recieving, *AI decides who to attack based on wis and int,
    // low stat range; attacks closest characters,
    // med stat range; attacks closest if too close, lowest health or furtherest if possible,
    // high stat range; attacks take into account if other players are nearby and their potential hazard as well as their own hazzard.
    
    // Healing *AI decides based on wis and int, low stat range; healing immediately, med stat range healing when reaching appropriate health
    // high stat range; taking into account target's hazzards, prioritizes their own protection in cases of multiple target's.
    
    // Interacting, with interactable objects, outcomes often based on vit, str, dex or cha and rarely on int and wis AI decisions based on wis and int
}

[System.Serializable]
public struct Statistics
{
    public int level;
    [Space]

    // pool stats / these are added and removed with the max and regen being determined by added stats
    // with the minimum being 0 and each pool
    // regenerating at the end of a turn.
    public int health; // * needed for life, if running out it's game over, unless theres something that will recover the character
    public int healthMax;
    public int healthRegenRate;
    public int stamina; // * complete most physical actions, represented by how tired the character is
    public int staminaMax;
    public int staminaRegenRate;
    public int mana; // * complete most mental actions, represented in an aura around the player, only visible with certain abilities, using tools mana can be stored and measured
    public int manaMax;
    public int manaRegenRate;
    public int action; // * move, attack, cast spells and do almost anything, characters with high action rate are able to do more in a turn, however using all actions at once can 
    public int actionMax;
    public int actionRegenRate;
    [Space]

    // added stats / these stats are where skill points are spent getting 5 for every level up to a certain level then getting less gradually over time
    // and read from when performing certain actions,
    // also affect pool stats in various ways.
    public int vitality; // * health, stamina and action(slightly 1 every 3)
    public int strength; // * attack damage, health and various acts
    public int dexterity; // * movement distance, how fast characters can act, various acts and action(slightly 1 every 3)
    public int chance; // * chance of learning skills, chance of finding more gold, chance of finding rarer items, esentially luck
    public int intellect; // * mana(slightly 1 every 3), magic damage and various acts
    public int wisdom; // * mana(slightly 1 every 3), chance of learning skills and various acts
}