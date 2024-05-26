using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : CharController
{
    // controls a character in place of NPCController
    // interacts with the environment using input from the player
    // mostly controls using UI and in game elements

    private Camera cam;
    private Vector3 camPosAdd;

    private Ray ray;
    private RaycastHit hit;

    private Color favouriteColour;
    private bool radiusCast;

    private float scrollSpeed;
    private float scrollDistanceCap;

    void Start()
    {
        //instantiate
        cam = gameObject.AddComponent<Camera>();
        camPosAdd = new Vector3(3f, 4f, -3f);
        cam.transform.position = modelObject.transform.position + camPosAdd;
        cam.transform.rotation = Quaternion.Euler(45.4646835f, 315.705444f, 0.432923496f);
        favouriteColour = new Color(178, 172, 136);
        radiusCast = false;
        
        //testing
        state = STATE.MOVING;
    }

    void Update()
    {
        //current regular updates are 3 per player
        UpdateCamera();
        UpdateCurrentTile();

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
    }

    #region Regular Updates
    //for the reggies yo
    void UpdateCamera()
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

    void UpdateCurrentTile()
    {
        if (currentTile != null) return;

        Physics.Raycast(modelObject.transform.position, Vector3.down, out hit, 1000f);
        currentTile = hit.transform.gameObject.GetComponent<TileController>().Tile;
    }
    #endregion

    // Moving, Player highlights available options.
    void CastSelectableRadius()
    {


        if (radiusCast) return;
        radiusCast = true;

        //within a determined radius, highlight available tiles in a color set by the player,
        float radius;
        TileController tile = currentTile.TileController;
        BaseTile[] foundTiles;

        //determine radius with stats by deciding action regen rate(prevents player from overexerting themselves)
        radius = stats.actionRegenRate;
        foundTiles = tile.FindTilesInRadius(tile.transform.position, radius);

        Debug.Log($"found tile for selectable: {tile.name}");
        //find selected tiles in radius around current tile
        selectableTiles = new BaseTile[foundTiles.Length-1];
        selectableTiles = foundTiles;

        // colour settings are set for clientside performance
        for (int i = 0; i < selectableTiles.Length; i++)
            GameManager.manager.levelManager.SetObjectColor(
                selectableTiles[i].TileObject.gameObject, favouriteColour);

        // ally, neutral and enemy movement is different colours only viewable from difficulty settings or spells,
        // this might be changed to character controller for expandability
    }
    void OnDrawGizmos()
    {
       // Gizmos.DrawWireSphere(currentTile.TileController.transform.position, stats.actionRegenRate);
    }

    void RemoveCurrentSelectableTiles() // Selectable Radius
    {   // after moving set selectable tiles to old colour array
        radiusCast = false;

        for (int i = 0; i < selectableTiles.Length; i++)
            GameManager.manager.levelManager.SetObjectColor(
                selectableTiles[i].TileObject.gameObject, Color.white);
    }
    void MovePlayer() 
        
        
        // maybe should be consolidated to a seperate class file that can be plugged into the controller
        // could lead to all facets being pluggable features?
    {
        CastSelectableRadius();

        if (Input.GetMouseButtonDown(0)) 
        {
            Vector3 finPOS;
            TileController tileController;
            
            ray = cam.ScreenPointToRay(Input.mousePosition);
            // ignore characters in movemode
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.TryGetComponent(out tileController))
                {
                    finPOS = tileController.Tile.StandingPos;
                    currentTile = tileController.Tile;
                      
                    Move(finPOS);
                    RemoveCurrentSelectableTiles();
                }
            }
        }
    }
}
