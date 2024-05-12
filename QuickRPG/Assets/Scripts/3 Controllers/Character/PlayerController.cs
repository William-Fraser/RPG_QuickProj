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

    private float scrollSpeed;
    private float scrollDistanceCap;

    void Start()
    {
        //instantiate
        camPosAdd = new Vector3(3f, 4f, -3f);
        cam = gameObject.AddComponent<Camera>();
        cam.transform.position = modelObject.transform.position + camPosAdd;
        cam.transform.rotation = Quaternion.Euler(45.4646835f, 315.705444f, 0.432923496f);
        
        state = STATE.MOVING;
    }

    void Update()
    {
        UpdateCamera();

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

    // Moving, Player highlights available options.
    void CastSelectableRadius()
    {
        //within a determined radius, highlight available tiles in a color set by the player,
        float radius;
        TileController tile = currentTile.TileController;

        //determine radius with stats by deciding action regen rate(prevents player from overexerting themselves)
        radius = stats.actionRegenRate;

        Debug.Log($"found tile for selectable: {tile.name}");
        //find selected tiles in radius around current tile
        selectableTiles = tile.FindTilesInRadius(tile.transform.position, radius);

        // change colour for 
        // colour settings are set for clientside performance
        // favouriteColour;

        // ally, neutral and enemy movement is different colours only viewable from difficulty settings or spells,
        // this might be changed to character controller for expandability
    }
    void MovePlayer() 
        
        
        // maybe should be consolidated to a seperate class file that can be plugged into the controller
        // could lead to all facets being pluggable features?
    {
        //CastSelectableRadius();<<<<<<<<<<<<<<<<<<<<<<<<<<<<<***!!!!

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
                }
            }
        }
    }
}
