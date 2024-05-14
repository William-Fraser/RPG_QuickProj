using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Transactions;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

public class NavMeshCharacterController : MonoBehaviour
{
    //please do not collapse headnotes they're for legacy purposes and should be seen often

    ///keep in mind
    /// accessability to control AI
    /// setting up defaults in start
    /// idle, patrol and roam are the base states, state patterns built off the base are more likely to activate


    /// ideas go here / theses are additions thought of while working on another part / if wanting improvements check this list
    /// move static defaults to start/awake!!, this will prevent overiding on awake.
    /// PatrolPoint extender in code for runtime Patrol expansion (spawns new Patrol point and adds to list).
    /// CustomEditor to hide unused behaviours traits.
    /// throw info for err checks, there are debug err calls that don't have any info, some errors are still unaccounted for.
    /// When porting features over, create a curiosity scale and and interesting rate, the curiosity scale get's multiplied by the interesting rate and
    ///     then the result can be used for timers and the sort, instead of asking for a bunch of numbers make it fit a character, different emotions like fear or 
    ///     anger? might be needed to attune the timers better, this is one way of knocking out multiple timers with one value.
    /// some timers should be in a random range
    ///     


    //set up types of characters

    public enum STATE
    {
        IDLE,
        PATROL,     // could use a better inspector section with a custom editor, the ability to add more points easily // needs method to add more patrol points 
        ROAM,
        CHASE,      // needs 'tired' bool, where losing a target will not set the character to ROAM while it has roaming trait
        COWER,      // Run away at low health // needs ability to sense strong characters and run away "if their damage is higher than their health"
        ATTACK,     // Check for available skills and use
        INTERACT,   // Interact with object either by instruction, habit or exploration
        SPAWN       // Create New Creature, duplication // needs method for co-creation (characteristics from two characters into one, this is used when information like skills and stats come into play)
    }

    #region variables

    [SerializeField] private Traits traits;// !!! these are options set in the inspector, they determine what a character can do, // moved to a subclass for folding

    [Header("Patrol")]
    public int patrol_idleTime;
    private int patrol_currentPoint;
    [SerializeField] private List<GameObject> patrol_points;

    [Header("Roam")]
    public int roam_idleTime;
    [SerializeField] private GameObject roam_target;
    [Tooltip("Max roaming Distance")] public float roam_distance = 5;

    [Header("Chase")]
    public int chase_idleTime;
    public int chase_roamTime;
    public List<GameObject> chase_targetsToChaseList;
    [SerializeField] private bool chase_checkTargetTags;
    private GameObject chase_target;
    [Tooltip("Distance that character will chase targets at")][SerializeField]private float chase_distance = 5;
    [Tooltip("Amount of time (seconds) before\nchecking for objects to chase")][SerializeField] private int chase_findFrequencyTime = 2;
    private float chase_lastTimeFound;
    private Vector3 chase_lastTargetPos;
    [SerializeField] private Collider[] chase_collisionInRange;

    [Header("Cower")]
    public float cower_percentHealthThreshold; // measured in %
    private float cower_baseForPercentThreshold = 20;
    public int cower_safeDistance = 100;

    [Header("Attack")]
    public float attack_distance;
    [SerializeField] private float attack_baseDamage = 9f; // base damage delt in attack
    private GameObject attack_target;
    [SerializeField] private int attack_cooldown;

    [Header("Interact")]
    public GameObject interact_Target;

    [Header("Spawn")]
    public float spawn_waitTime = 90; // the interval between natural spawning times
    private float spawn_spawningTime; // time that the character will naturally try to spawn
    private Vector3 spawn_spawningPoint;
    private int spawn_distance = 1; // distance that characters spawn at, an alien or plant like chaaracter could spawn at a further distance, this can be used and manipulated for other purposes as well
    private GameObject spawn_newCharacter;
    [SerializeField] private float spawn_healthPercentCheck = 10;
    [SerializeField] GameObject spawn_cloneInstance;

    [Header("Misc.")]
    //status'
    public GameObject charLastAttackedBy;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float health; // base HP for attacks to take from
    [Tooltip("starts with edited health")][SerializeField] private bool startWithEditedHealth = false;
    private bool alive;
    //states
    [SerializeField] private STATE state;
    private STATE baseState;
    private Dictionary<int, STATE> timedStates;
    //movements
    private NavMeshAgent agent;
    private float moveStopDistance = 1;
    private Vector3 moveCheck;

    #endregion

    public STATE State { get => state; }


    public void Awake() // changed to from start / for spawning / Instantiate only catches awake not start
    {
        Debug.Log($"{transform.parent.gameObject.name} is waking up");

        //data checks
        if (traits.patrolling && patrol_points.Count < 1) { Debug.LogError($"{gameObject.name} PATROLERROR: 1 or more patrol points needed"); traits.patrolling = false; }
        if (roam_distance < moveStopDistance) { Debug.LogError($"{gameObject.name} ROAMERROR: distance is shorter than stopping distance"); }
        if (!startWithEditedHealth) { health = maxHealth; }

        //setup starting base state
        if (traits.idling) { state = STATE.IDLE; baseState = STATE.IDLE; }
        if (traits.roaming) { state = STATE.ROAM; baseState = STATE.ROAM; }
        if (traits.patrolling) { state = STATE.PATROL; baseState = STATE.PATROL; }

        agent = GetComponent<NavMeshAgent>();
        patrol_currentPoint = 0;
        timedStates = new Dictionary<int, STATE>();
        roam_target.transform.parent = this.transform.parent; // set parent to self
        spawn_spawningTime = Time.time + spawn_waitTime;
        spawn_spawningPoint = this.transform.position;
        attack_target = roam_target;
        charLastAttackedBy = transform.parent.gameObject;

        SetRoamTargetToClosestNavPos();

        alive = true;
    }

    public void Update()
    {
        CheckTimedStates();
        CheckToCower();

        switch (state)
        { 
            case STATE.IDLE: // this is the default see switch doc???

                FindChaseTarget();
                CheckToSpawn();
                return;

            case STATE.PATROL:

                //init variables
                Vector3 pointPos = patrol_points[patrol_currentPoint].transform.position;
                float distanceFromPoint = Vector3.Distance(pointPos, this.gameObject.transform.position);

                MoveTo(pointPos);
                
                if (distanceFromPoint <= moveStopDistance) // change patrol points and maybe idle
                {
                    ChangePatrolPoint();
                
                    if (traits.idling)
                    {
                        Idle();
                        SetStateTimer(patrol_idleTime, STATE.PATROL);
                    }
                }

                FindChaseTarget();
                CheckToSpawn();
                return;

            case STATE.ROAM: // might lag, then increase idle time with every entity within loading range, increasing the time added too maybe :)

                FindRoamingPos();
                MoveTo(roam_target.transform.position);

                FindChaseTarget();
                CheckToSpawn();
                return;

            case STATE.CHASE:

                TargetLossCatch();
                MoveTo(chase_target.transform.position);
                TrackChaseTarget();
                CheckChaseToAttack();
                return;

            case STATE.COWER:

                Cower();

                cower_percentHealthThreshold = cower_baseForPercentThreshold; // reset threshold it will be modified again if needed / this could be taken out if not built on

                CheckToSpawn();
                CheckToEndCower();
                return;

            case STATE.ATTACK:

                Damage(attack_target);
                attack_target = roam_target; //resets target to not attackable object
                return;

            case STATE.INTERACT:

                MoveTo(interact_Target.transform.position);
                InteractWith(interact_Target);
                return;

            case STATE.SPAWN:

                MoveTo(spawn_spawningPoint);

                if (Vector3.Distance(this.transform.position, spawn_spawningPoint) <= spawn_distance )
                { SpawnCopy(); } // this will be replaced with a switch statement?, perhaps nested in a method
                return;

            default:
                Idle();
                return;
        }
    }

    private void MoveTo(Vector3 pointPos)
    {
        //blocker statement
        if (moveCheck == pointPos) return;
        
        agent.SetDestination(pointPos);
        moveCheck = pointPos;
    } // moves to target

    private void MoveFrom(Vector3 pointPos) // moves to opposite pos from target // very bad code, toggles parent to get pos
    {
        //blocker statement
        if (moveCheck == pointPos) return;
        
        roam_target.transform.position = pointPos;              //grab position with gO
        roam_target.transform.parent = gameObject.transform;    //set parent for local
        roam_target.transform.localPosition = new Vector3(      //inverse on player's y axis
            -(roam_target.transform.localPosition.x+2),
            roam_target.transform.localPosition.y, 
            -(roam_target.transform.localPosition.z+2));          //Magic # +2 to give the target room for chasing at close range

        roam_target.transform.parent = this.transform.parent;   //set parent for movement
        agent.SetDestination(roam_target.transform.position);   //move to pos
        moveCheck = pointPos; // pass first changing (checkable) value for blocker
    }
    
    //state methods
    #region Misc., Idle & Patrol
    private void SetState(STATE setState)
    {
        if (setState == state) Debug.LogWarning($"{gameObject.name} WARNING: State was set to the same state");

        state = setState;
    }

    private void SetStateTimer(int timeTilTimeOut, STATE switchToOnTimeOut) //used in multiple methods, it's a timer that sets the state after a while, good for lots of things
    {
        int stateTimer = (int)Time.time + timeTilTimeOut;
        timedStates.Add(stateTimer, switchToOnTimeOut);
    }

    private void CheckTimedStates()
    {
        //returns to other state, see SetStateTimer();
        //blocker statements
        if (timedStates.Count == 0) return;
        if (!timedStates.ContainsKey((int)Time.time)) return;

        STATE newState = timedStates[(int)Time.time];

        if (newState == baseState) timedStates.Clear(); // stop timers while at base state, timers can clear for different events like chasing

        SetState(newState); 
        timedStates.Remove((int)Time.time);
    }

    private void SetBaseState(STATE newBaseState) // used to have characters change from following(chase) to idle to patrol
    {
        if (newBaseState == baseState) Debug.LogWarning($"{gameObject.name} WARNING: Base state was set to the same state");

        baseState = newBaseState;
    }


    //idle
    public void Idle() // access Idle from behaviour scripts
    {
        if (!traits.idling) { Debug.LogError($"{gameObject.name} is Idle when it is not an active trait"); return; }
        state = STATE.IDLE;
    }


    //patrol
    private void ChangePatrolPoint()
    {
        patrol_currentPoint++;

        if (patrol_currentPoint >= patrol_points.Count) patrol_currentPoint = 0;
    }
    
    public void Patrol() //access patrol from behaviour scripts
    {
        if (!traits.patrolling) { Debug.LogError($"{gameObject.name} is Patrolling when it is not an active trait"); return; }
        state = STATE.PATROL;
    }
    #endregion

    #region Roam
    private void SetRoamTargetToClosestNavPos()
    {
        //initiate method objects
        float roamX = UnityEngine.Random.Range(-roam_distance, roam_distance);
        float roamY = gameObject.transform.position.y;
        float roamZ = UnityEngine.Random.Range(-roam_distance, roam_distance);
        Vector3 newPos = gameObject.transform.position + new Vector3(roamX, roamY, roamZ);
        NavMeshHit hit;
        
        if (NavMesh.SamplePosition(newPos, out hit, roam_distance, 1))
            roam_target.transform.position = hit.position;

    } // finds a random position / sets it as the roam target

    private void FindRoamingPos()
    {
        float disFromTarget = Vector3.Distance(roam_target.transform.position, this.gameObject.transform.position);

        //find close target if current is outside roam distance
        if (disFromTarget > roam_distance) 
            SetRoamTargetToClosestNavPos();

        //blocker statement
        if (disFromTarget > moveStopDistance) return;

        //maybe idle for a bit at each position
        if (traits.idling)
        {
            SetStateTimer(roam_idleTime, STATE.ROAM);
            SetState(STATE.IDLE);
        }

        //after reaching destination change position
        SetRoamTargetToClosestNavPos();
    }   // checks distance / SetRoamTargetToClosestNavPos / maybe idles
    
    public void Roam() // access Roam from behaviour scripts
    {
        if (!traits.roaming) { Debug.LogError($"{gameObject.name} is Roaming when it is not an active trait"); return; }
        state = STATE.ROAM;
    }
    #endregion

    #region Chase
    private void TrackChaseTarget() // could use logic to see if character is suck in same pos, this would signify a bug
    {
        //calc distance
        float distanceFromTarget = Vector3.Distance(agent.destination, gameObject.transform.position);

        //blocker statement // target is being chased
        if (distanceFromTarget <= chase_distance + 3) 
            { chase_lastTargetPos = agent.destination; return; } // M# plus 3 gives the character room to chase objects running away
        
        Debug.Log($"{transform.parent.gameObject.name} let something get away");

        //finish moving to last known target pos
        MoveTo(chase_lastTargetPos);

        distanceFromTarget = Vector3.Distance(chase_lastTargetPos, gameObject.transform.position);

        if (distanceFromTarget <= 2) { return; } // M#2 so there's room for characters to breathe

        //calc/start to look for target
        STATE trackingState = STATE.IDLE;
        if (traits.roaming) trackingState = STATE.ROAM;

        switch (trackingState)
        { 
            case STATE.IDLE: SetStateTimer(chase_idleTime, baseState); SetState(STATE.IDLE); break;
            case STATE.ROAM: SetStateTimer(chase_roamTime, baseState); SetState(STATE.ROAM); break;
        }
    }

    private void FindChaseTarget()
    {   //uses chase list and chase distance to determine if a character is apporopriate to chase
        if (!traits.chasing) return;
        if (chase_targetsToChaseList == null) return;
        if (chase_lastTimeFound > Time.time) return;
        
        //update time found with frequency to delay next check
        chase_lastTimeFound = Time.time + chase_findFrequencyTime;

        chase_collisionInRange = Physics.OverlapSphere(this.transform.position, chase_distance);
        Debug.Log($"{transform.parent.gameObject.name} chase collision found {chase_collisionInRange.Length} objects"); // please do not remove instead comment out, for performance bug finding // maybe put a range on findFrequency time

        // check for rigid bodies? and check tags
        for (int i = 0; i < chase_collisionInRange.Length; i++)
        {
            GameObject collidedObject = chase_collisionInRange[i].gameObject;

            if (collidedObject.GetComponent<Rigidbody>())
            {
                if (collidedObject != this.gameObject && !collidedObject.CompareTag(this.gameObject.tag))
                { 
                    if (chase_checkTargetTags)
                    {
                        for (int j = 0; j < chase_targetsToChaseList.Count; j++)
                        { 
                            if (collidedObject.CompareTag(chase_targetsToChaseList[j].tag))
                            { 
                                Chase(collidedObject);
                            }
                        }
                    }
                    else Chase(collidedObject);
                }
            }
        }
    }
    private void TargetLossCatch()
    {
        if (chase_target == null) SetState(baseState);
    }

    public void Chase(GameObject gameObject)
    {
        //blocker statement
        if (!traits.chasing) { Debug.LogError($"{this.gameObject.name} is Chasing when it is not an active trait"); return; }
        
        Debug.Log($"{this.gameObject.name} is chasing {gameObject.name}");

        //stop state timers to prevent target loss
        timedStates.Clear();

        //setup the chase
        chase_target = gameObject;
        state = STATE.CHASE;
    }

    #endregion 
     
    #region Cower
    private void CheckToCower()
    {
        float healthPercent = (health / maxHealth) * 100; 

        if (!traits.cowering) return;
        if (healthPercent > cower_percentHealthThreshold) return;
        // maybe set up a timer for cower to end
        Debug.Log($"HP:{healthPercent}, CHT:{cower_percentHealthThreshold}");

        Cower();
    } 

    private void CheckToEndCower()
    {
        float realHealthThreshold = (cower_percentHealthThreshold / 100) * maxHealth;
        
        if (health > realHealthThreshold) SetState(baseState);
        
        if (charLastAttackedBy != null && // check for value before checking for distance \/ if initial case is invalid it should'nt check AND case
            Vector3.Distance(charLastAttackedBy.transform.position, this.transform.position) < cower_safeDistance) return;

        SetState(baseState); // when character is a safe distance away then return to base state
    }

    public void Cower()
    {
        if (!traits.cowering) return;
        if (charLastAttackedBy == null) return; // assign in take damage, fear effects will need to cause a bit of damage, or 0 damage which might look awkward
        
        state = STATE.COWER;
        // run away from most recent attacker / the more allies or enemies the easier or harder it will be to preform
        MoveFrom(charLastAttackedBy.transform.position);
    }

    public void CowerMoveTo(Vector3 pos) // used for player movement while cowering
    {
        if (!traits.cowering) return;
        if (pos == null) return;

        MoveFrom(pos);
    }
    #endregion

    #region Attack
    private void Damage(GameObject target)
    {
        if (!traits.attacking) return;

        NavMeshCharacterController otherCharControl;
        // if char controller is avaiable then make it take damage
        if (target.TryGetComponent<NavMeshCharacterController>(out otherCharControl))
        { 
            otherCharControl.TakeDamage(attack_baseDamage); //just base damage for now
            otherCharControl.charLastAttackedBy = gameObject;
        }
    }

    private void CheckChaseToAttack()
    {
        if (!traits.attacking) return;

        float distanceFromTarget = Vector3.Distance(chase_target.transform.position, gameObject.transform.position);

        if (distanceFromTarget <= attack_distance)
        { 
            SetState(STATE.ATTACK);
            SetStateTimer(attack_cooldown, baseState);
            attack_target = chase_target;
        }
    }

    private void CheckHealthToDie()
    {
        if (health < 0)
            alive = false;

        if (!alive) //basic death
            Destroy(transform.parent.gameObject);
    }

    public void Attack(GameObject target) //readies attack, to deal instant damage see Damage() ^^
    {
        attack_target = target;
        SetState(STATE.ATTACK);
    }

    public void TakeDamage(float damage)
    {
        //subtract health
        health = health - damage;

        CheckHealthToDie();
    }
    #endregion

    #region Interact
    private void FindInteractTargetNear(Vector3 Pos) //set state
    { 
        //find interactable object within an area
    }

    public void InteractWith(GameObject gameObject)
    { 
        //if gameobject is a interactable && is within range / interact with it somehow
    }
    #endregion

    #region Spawn
    public void SpawnCopy() // creates and identical copy of this object, used for basic char dupe mechanics
    {
        SetState(baseState);

        //for now spawn behind current character, enhance spawning placement by using NavMesh's Obstacle Avoidance
        Transform characterGroup = this.gameObject.transform.parent.transform.parent.transform;
        GameObject copyParent;

        health = health + 10; // bad fix to constant spawn when low health, fix to bool or something better maybe require timer and create a focus on low health?, maybe multiple methods

        copyParent = Instantiate(spawn_cloneInstance, characterGroup); // gets the gameobject of the parent
        copyParent.SetActive(true);
        spawn_newCharacter = copyParent.GetComponentInChildren<NavMeshAgent>().gameObject;
        spawn_newCharacter.transform.localPosition = spawn_newCharacter.transform.localPosition + Vector3.back; // maybe multiply distance by size of original        
    }
    // spawn minion // spawning something that isn't directly related to the character in code
    // spawn offspring // this entails another character is blending their traits with this character to create a new one

    private void CheckToSpawn() // checks to spawn a creature, activated from base states (see above) // set state
    {
        // checks against spawn rules and activates preferred spawn method, multiple methods could be possible at the same time, might need some sort of menu to set up
        // for now spawns at low health or after a set, preferably long amount of time

        float calculatedHealthPercent = (health / maxHealth) * 100;

        // blockers/checks
        if (!traits.spawning) return;
        if (Time.time < spawn_spawningTime) return;
        if (calculatedHealthPercent < spawn_healthPercentCheck || Time.time > spawn_spawningTime)
        {
            //reset timer
            spawn_spawningTime = spawn_spawningTime + spawn_waitTime;

            SetState(STATE.SPAWN);

            Debug.LogError($"H:{calculatedHealthPercent}, C:{spawn_healthPercentCheck}, T:{Time.time}, S:{spawn_spawningTime}");
        }
    }
    #endregion
}

[Serializable]
public class Traits // currently using as a group to fold in the inspector, when using a custom editor use EditorGUILayout.Foldout
{
    [Tooltip("*If Used in tandem with other traits. \nIdle's at Patrol points and during \nChase (if target is lost). ")] public bool idling;
    [Tooltip("*If Used in tandem with other traits. \nPatrol is a Priority State, you can \n use 1 point for a charater to return to a position ")] public bool patrolling;
    [Tooltip("*If Used in tandem with other traits. \nRoam's during Chase instead of Idle \nto 'search' for target before 'losing interest'. ")] public bool roaming;
    public bool chasing;
    public bool cowering;
    public bool attacking;
    public bool interacting;
    public bool spawning;
}
