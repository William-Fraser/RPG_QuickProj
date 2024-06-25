using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : CharController
{
    // controls a character in place of NPCController
    // interacts with the environment using input from the player
    // mostly controls using UI and in game elements
    [Header("Player Values")]
    private Camera cam;
    private Vector3 camPosAdd;
    [SerializeField] private float scrollSpeed;
    [SerializeField] private float scrollDistanceCap;

    private Ray ray;
    private RaycastHit hit;

    private bool turnStarted;
    private bool radiusCast;
    [SerializeField] private Color favouriteColour;

    private void Start()
    {
        //instantiate
        cam = gameObject.AddComponent<Camera>();
        cam.gameObject.tag = "MainCamera";
        camPosAdd = new Vector3(3f, 4f, -3f);
        cam.transform.position = modelObject.transform.position + camPosAdd;
        cam.transform.rotation = Quaternion.Euler(45.4646835f, 315.705444f, 0.432923496f);
        radiusCast = false;
        
        //testing
        state = STATE.MOVING;
    }

    new private void Update() // max possible updates 5 : 6 on turn start 
        // recalculated 2 times so far
    {
        //debug

        //look
        /*ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject )
                if ( currentTile.TileObject.gameObject)
            FaceDir(hit.transform);
        }*/


        base.Update();

        //current regular updates are 4 per player
        UpdateCamera();
        UpdateCurrentTile();
        UpdateOnTurnStart();

        //switch updates are at max 1
        switch (state)
        {
            case STATE.IDLE:

                break;

            case STATE.READY:

                break;

            case STATE.MOVING:

                MovePlayer();
                break;
        }
    } // 2 change on recalculation to maintainconsistency

    #region Regular Updates
    //for the reggies yo
    private void UpdateCamera()
    {
        //controls for camera, including movement corrections, scrolling, other techniques and limits of all tech 
        if (cam.transform.position != (modelObject.transform.position + camPosAdd))
            cam.transform.position = (modelObject.transform.position + camPosAdd);

        if (Input.mouseScrollDelta.y != 0)
        {
            cam.gameObject.transform.localPosition -= new Vector3(Input.mouseScrollDelta.y * 0.1f, Input.mouseScrollDelta.y * 0.1f, -Input.mouseScrollDelta.y * 0.1f);
            camPosAdd = cam.gameObject.transform.localToWorldMatrix.GetPosition() - modelObject.transform.position;
        }
    }

    private void UpdateCurrentTile()
    {
        if (currentTile != null) return;

        Physics.Raycast(modelObject.transform.position, Vector3.down, out hit, 1000f);
        if (hit.transform != null)
            currentTile = hit.transform.gameObject.GetComponent<TileController>().Tile;
    }

    private void UpdateOnTurnStart()
    {
        if (turnStarted) return;
        turnStarted = true;
        //Updates character for things that happen at the start of a turn
        //starts by checking levelmanager if it is their turn then affect stats appropriately

    }
    #endregion

    // Moving, Player highlights available options.
    private void CastSelectableRadius()
    {
        if (radiusCast) return;
        if (currentTile == null) return;

        radiusCast = true;

        //within a determined radius, highlight available tiles in a color set by the player,
        float radius;
        TileController tile = currentTile.TileController;
        BaseTile[] foundTiles;

        //determine radius with stats by deciding action regen rate(prevents player from overexerting themselves)
        radius = stats.ActionRegen;
        foundTiles = tile.FindTilesInRadius(tile.transform.position, radius);

        //find selected tiles in radius around current tile
        selectableTiles = new Dictionary<Vector3, BaseTile>();
        selectableTileKeys = new Vector3[foundTiles.Length];

        for (int i = 0; i < foundTiles.Length; i++)
        {
            selectableTiles.Add(foundTiles[i].TileController.gameObject.transform.position, foundTiles[i]);
            selectableTileKeys[i] = foundTiles[i].TileController.gameObject.transform.position;

            GameManager.manager.levelManager.SetObjectColor(
                foundTiles[i].TileObject.gameObject, favouriteColour);
        }

        Vector3 heightXZ = modelObject.transform.position;
        Vector3 height = new Vector3(heightXZ.x, modelObject.GetComponent<Collider>().bounds.extents.y, heightXZ.z);
        GameManager.manager.levelManager.CreatePopUp("OKAY", height, favouriteColour);

        // colour settings are set for clientside performance

        // ally, neutral and enemy movemen
        // t is different colours only viewable from difficulty settings or spells,
        // this might be changed to character controller for expandability
    }

    private void MovePlayer() 
        // maybe should be consolidated to a seperate class file that can be plugged into the controller
        // could lead to all facets being pluggable features?
    {
        ///AdventureMovement
        if (GameManager.manager.gamemodeManager.gameRules.FreeMove)
        { 
            if (Input.GetMouseButton(0))
            {
                ray = cam.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    TileController tileController;

                    Move(hit.transform.position);
                    RemoveCurrentSelectableTiles();

                    if (hit.transform.TryGetComponent(out tileController))
                        currentTile = tileController.Tile;
                }
            }
        }
        ///TurnBasedMovement
        else // TurnBased
        {
            if (currentTile == null) return;

            CastSelectableRadius(); // <reason for big mess

            if (Input.GetMouseButtonDown(0) && Vector3.Distance(modelObject.transform.position, currentTile.StandingPos) < 1) 
            {
                ray = cam.ScreenPointToRay(Input.mousePosition);
                
                if (Physics.Raycast(ray, out hit))
                {
                    TileController tileController;

                    if (hit.transform.TryGetComponent(out tileController))
                    {
                        if (selectableTiles.ContainsKey(tileController.transform.position))
                        { 
                            Vector3 finPOS;
                            
                            finPOS = tileController.Tile.StandingPos;

                            Move(finPOS);
                            RemoveCurrentSelectableTiles();

                            currentTile = tileController.Tile;
                        }
                    }
                }
            }
        }
    }

    public void RemoveCurrentSelectableTiles() // Selectable Radius
    {   // after moving set selectable tiles to old colour array
        radiusCast = false;
        if (selectableTiles.Count == 0) return;

        for (int i = 0; i < selectableTiles.Count; i++)
            GameManager.manager.levelManager.SetObjectColor(
                selectableTiles[selectableTileKeys[i]].TileObject.gameObject, Color.white);
    }
}
