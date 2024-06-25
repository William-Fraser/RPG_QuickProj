using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public enum EFFECT
{ 
    // these are effects that affect the player and when applied happen in a separate switch statement
    BASELINE,
    BLEEDING, // deals hp damage
    BRUISED, // deals stamina damage
    MANASICKNESS, // deals mana damage
    TWISTED // deals action damage

        //+6 more that reduce added stats
}

public abstract class CharController : MonoBehaviour
{
    // abstract object, base for other controllers

    protected enum STATE
    { 
        IDLE, //used in turns, the character is out of actions and is waiting for the next turn
        READY, //the character is deciding on what to do, a form of IDLE when there are available actions
        MOVING //the character is using the move function
    }

    [SerializeField] protected GameObject modelObject;
    [Space]

    protected STATE state;
    [SerializeField] protected Statistics stats;

    #region character Effects 
    // maybe move to stats? and switch stats and the like
    // to dictionaries or arrays of ints with enum quantifiers

    public bool affectWithEffects;
    private List<EFFECT> activeEffects;
    private int effectsAffected;
    
    private int bleedingDamage;
    private int bleedingTime;

    private int bruisedDamage;
    private int bruisedTime;
    
    private int mSicknessDamage;
    private int mSicknessTime;
    
    private int twistedDamage;
    private int twistedTime;
    #endregion

    private static NavMeshAgent navAgent;
    protected BaseTile currentTile;
    protected Dictionary <Vector3, BaseTile> selectableTiles;
    protected Vector3[] selectableTileKeys;
    protected GameObject focusedTarget;

    protected void Awake()
    {
        effectsAffected = 0;
        bleedingTime = 0;
        bruisedDamage = 0;
        bruisedTime = 0;
        mSicknessDamage = 0;
        mSicknessTime = 0;
        twistedDamage = 0;
        twistedTime = 0;

        /*RaycastHit hit;
        Physics.Raycast(modelObject.transform.position, Vector3.down, out hit);
        currentTile = hit.transform.gameObject.GetComponent<TileController>().Tile;*/

        //check for model postition, if not at local.zero send error.
        if (modelObject.transform.localPosition != Vector3.zero)
        
        //pass object references
        navAgent = modelObject.GetComponent<NavMeshAgent>();

        //set up stats for undefined characters, for characters that dont choose a path
        stats = new();
        stats.StarterStats();

        selectableTiles = new();
    }

    protected void Update()
    {
        stats.Update();

        if (affectWithEffects)
        { 
            affectWithEffects = false; //set true when executing turns or just all the time in free mode, with a second delay
            effectsAffected = 0;

            UpdateEffects();
        }
    }

    private void UpdateEffects()
    {
        if (effectsAffected == activeEffects.Count) return;

        switch (activeEffects[effectsAffected])
        {
            case EFFECT.BASELINE:
                //character is fine do nothing
                break;

            case EFFECT.BLEEDING:
                stats.CalculatePoolStat(stats.Health, bleedingDamage);
                bleedingTime--;
                break;

            case EFFECT.BRUISED:
                stats.CalculatePoolStat(stats.Stamina, bruisedDamage);
                bruisedTime--;
                break;

            case EFFECT.MANASICKNESS:
                stats.CalculatePoolStat(stats.Mana, mSicknessDamage); 
                mSicknessTime--;
                break;

            case EFFECT.TWISTED:
                stats.CalculatePoolStat(stats.Action, twistedDamage); 
                twistedTime--;
                break;

            default: return;
        }

        LookForAnotherActiveEffect();
    }

    private void LookForAnotherActiveEffect()
    { 
        effectsAffected++;
        UpdateEffects();
    }

    protected void Move(Vector3 pos)// Move to specified position; can initiate turn without cost of action
    {
        navAgent.SetDestination(pos);
    }
    
    void Target(GameObject target)// focus on and face object in view; costs 1 action
    {
        //set a target to pursue
        focusedTarget = target;
        
        //face target
        FaceDir(target.transform);
    }
    
    protected void FaceDir(Transform transform)// face direction at the end of move; costs 1 action
    {
        //equal look at, add animation code for characters head
        GameObject equal = new();

        equal.transform.position = new Vector3(transform.position.x, modelObject.transform.position.y, transform.position.z);

        modelObject.transform.LookAt(equal.transform, Vector3.up);
    }

    public void AddEffect(EFFECT type, int damage, int time)
    {
        activeEffects.Add(type);

        switch (type)
        {
            case EFFECT.BASELINE:
                //character is returned to normals
                effectsAffected = 0;
                bleedingTime = 0;
                bruisedDamage = 0;
                bruisedTime = 0;
                mSicknessDamage = 0;
                mSicknessTime = 0;
                twistedDamage = 0;
                twistedTime = 0;
                return;

            case EFFECT.BLEEDING:
                bleedingDamage += damage;
                bleedingTime += time;
                return;

            case EFFECT.BRUISED:
                bruisedDamage += damage;
                bruisedTime += time;
                return;

            case EFFECT.MANASICKNESS:
                mSicknessDamage += damage;
                mSicknessTime += time;
                return;

            case EFFECT.TWISTED:
                twistedDamage += damage;
                twistedTime += time;
                return;

            default: return;
        }
    }

    public void RemoveEffect(EFFECT type)
    {
        switch (type)
        {
            case EFFECT.BLEEDING:
                effectsAffected = 0;
                bleedingTime = 0;
                return;

            case EFFECT.BRUISED:
                bruisedDamage = 0;
                bruisedTime = 0;
                return;

            case EFFECT.MANASICKNESS:
                mSicknessDamage = 0;
                mSicknessTime = 0;
                return;

            case EFFECT.TWISTED:
                twistedDamage = 0;
                twistedTime = 0;
                return;

            default: return;
        }
    }

    // Remaining Methods
    // *Acting : handled by a manager taking turns recalculated between each turn
    // types are UI for players, AI for NPC's, UI and AI for allies(decisions made by AI influenced by UI),
    // e.g. Summoned creatures can have player controllers, characters with no influence have NPC control and team mates have Ally

    // Attacking, giving and recieving, *AI decides who to attack based on wis and int,
    // low stat range; attacks closest characters,
    // med stat range; attacks closest if too close, lowest health or furtherest if possible,
    // high stat range; attacks take into account if other players are nearby and their potential hazard as well as their own hazzard.
    // method dealing with effects applying them and utilizing them properly, a sub-class with
    // a timer built into turns and then counting down as seconds in free mode
    // to add Effects use AddEffects();

    // Healing *AI decides based on wis and int, low stat range; healing immediately, med stat range healing when reaching appropriate health
    // high stat range; taking into account target's hazzards, prioritizes their own protection in cases of multiple target's.

    // Interacting, with interactable objects, outcomes often based on vit, str, dex or cha and rarely on int and wis AI decisions based on wis and int
}

[System.Serializable]
public struct Statistics
{
    public int level;
    [Space(20)]

    // pool stats / these are added and removed with the max and regen being determined by added stats
    // with the minimum being 0 and each pool
    // regenerating at the end of a turn.
    #region poolstats
    [SerializeField] private int health; // * needed for life, if running out it's game over, unless theres something that will recover the character
    [SerializeField] private int healthMax;
    [SerializeField] private int healthRegen;
    [SerializeField] private int healthDecay;
    [Space(20)]
    [SerializeField] private int stamina; // * complete most physical actions, represented by how tired the character is
    [SerializeField] private int staminaMax;
    [SerializeField] private int staminaRegen;
    [SerializeField] private int staminaDecay;
    [Space(20)]
    [SerializeField] private int mana; // * complete most mental actions, represented in an aura around the player, only visible with certain abilities, using tools mana can be stored and measured
    [SerializeField] private int manaMax;
    [SerializeField] private int manaRegen;
    [SerializeField] private int manaDecay;
    [Space(20)]
    [SerializeField] private int action; // * move, attack, cast spells and do almost anything, characters with high action rate are able to do more in a turn, however using all actions at once can lead to unexpected outcomes
    [SerializeField] private int actionMax;
    [SerializeField] private int actionRegen;
    [SerializeField] private int actionDecay;
    public int Health { get; }
    public int Stamina { get; }
    public int Mana { get; }
    public int Action { get; }
    public int ActionRegen { get; }

    [Space(20)]
    #endregion

    // added stats / these stats are where skill points are spent getting 5 for every level up to a certain level then getting less gradually over time
    // and read from when performing certain actions,
    // also affect pool stats in various ways.
    public int vitality; // * health, stamina and action(slightly 1 every 3)
    public int strength; // * attack damage, health and various acts
    public int dexterity; // * movement distance, how fast characters can act, various acts and action(slightly 1 every 3)
    public int chance; // * chance of learning skills, chance of finding more gold, chance of finding rarer items, esentially luck
    public int intellect; // * mana(slightly 1 every 3), magic damage and various acts
    public int wisdom; // * mana(slightly 1 every 3), chance of learning skills and various acts
    [Space(20)]

    private bool alive;
    public bool Alive { get; }

    public void Update() // (10, 5, 7, 4, 3, 9) // try to form the values with these starter values
    {
        UpdateZeroValues();

        if(health != healthMax || stamina != staminaMax ||
            mana != manaMax || action != actionMax)
        RegenOrDecay(health, healthMax, healthRegen, healthDecay);
        RegenOrDecay(stamina, staminaMax, staminaRegen, staminaDecay);
        RegenOrDecay(mana, manaMax, manaRegen, manaDecay);
        RegenOrDecay(action, actionMax, actionRegen, actionDecay);

        if (health == 0 && alive)
        { Kill(); return; }

        if (alive) return;
        alive = true;

        health = healthMax;
        stamina = staminaMax;
        mana = manaMax;
        action = actionMax;
    }

    private void Refresh()
    {
        healthMax = (vitality + strength) * wisdom; //150
        healthRegen = strength * chance; //20
        healthDecay = strength * chance; //* for now same as regen, gained when overexerting value past max, like overhealing

        staminaMax = (vitality + dexterity) * strength; //75
        staminaRegen = strength * intellect; // 15
        staminaDecay = strength * intellect; //*

        manaMax = (intellect + wisdom) * chance; //50 
        manaRegen = intellect * wisdom;// 30
        manaDecay = intellect * wisdom; //*

        actionMax = 5; // flat action rate
        actionRegen = 2;
        actionDecay = 1; //*

        UpdateZeroValues();
    }


    private void UpdateZeroValues()
    {
        if (health < 0) health = 0;
        if (stamina < 0) stamina = 0;
        if (mana < 0) mana = 0;
        if (action < 0) action = 0;
    }

    private void RegenOrDecay(int value, int max, int regen, int decay)
    {
        if (value < max) // regen
        {
            value += regen;
            if (value > max) value = max;
        }
        else if (value > max) //decay
        {
            value -= decay;
        }
    }

    private int CalculateAmount(int amount, bool positive)
    {
        if (!positive)
        {
            amount = 0 - amount;
        }
        else { }

        return amount;
    }

    //take and receve stats, (damage and heal)
    public void CalculatePoolStat(int value, int amount, bool positive = false)
    {
        if (value == health) health += CalculateAmount(amount, positive);
        else if (value == stamina) stamina += CalculateAmount(amount, positive);
        else if (value == mana) mana += CalculateAmount(amount, positive);
        else if (value == action) action += CalculateAmount(amount, positive);
    }

    public void AddToPoolStats(int vita, int stre, int dext, int chan, int inte, int wisd)
    {
        vitality += vita;
        strength += stre;
        dexterity += dext;
        chance += chan;
        intellect += inte;
        wisdom += wisd;

        Refresh();
    }

    public void StarterStats()
    {
        vitality = 5;
        strength = 5;
        dexterity = 5;
        chance = 5;
        intellect = 5;
        wisdom = 5;

        AddToPoolStats(5, 0, 2, -1, -2, 4);

        alive = false;
    }

    public void Kill()
    {
        alive = false;
    }
}