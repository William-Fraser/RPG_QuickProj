using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum TILETYPE
{
    BASE
}

public class TileController : MonoBehaviour
{
    [Header("Tile Objects")]
    public GameObject activeTileObject;
    public GameObject basicTileObject;

    [SerializeField] private bool UseActiveTile;
    [SerializeField] private BaseTile tile;
    [SerializeField] public BaseTile Tile { get; set; }

    void Start()
    {
        if (UseActiveTile == false || activeTileObject == null) return;

        SetTile(new BaseTile(basicTileObject, this));
    }

    public TileController(TILETYPE tileType) 
    { 
        CreateTile(tileType);
    }

    public BaseTile CreateTile(TILETYPE type)
    {
        BaseTile newTile;

        switch (type)
        {
            case TILETYPE.BASE:
                Debug.Log($"loading new tile()!");
                newTile = new BaseTile(basicTileObject);
                newTile.TileController.SetTile(newTile); // the new tile will generate a new tilecontroller

                Debug.Log($"Tile made: {gameObject.name}");
                Debug.Log($"Tile connection: {newTile.TileController.gameObject.name}");
                break;
            default:
                Debug.LogWarning($"WARNING: Tile type {type} not registered in method");
                newTile = new BaseTile(basicTileObject);
                break;
        }

        return newTile;
    }

    //tile related function
    public BaseTile[] FindTilesInRadius(Vector3 centre, float radius)
    {
        RaycastHit[] hits = new RaycastHit[0];
        GameObject[] objects = new GameObject[0];
        List<BaseTile> tiles = new();

        //spherecast in a radius around centre, and grab tiles to return
        hits = Physics.SphereCastAll(centre, radius, Vector3.down, 3);

        objects = new GameObject[hits.Length];;

        for (int i = 0; i < hits.Length; i++)
        {
            objects[i] = hits[i].collider.gameObject;

            if (objects[i].tag == "Tile")
            {
                tiles.Add(objects[i].GetComponent<TileController>().Tile);
            }
        }

        return tiles.ToArray();
    }

    //complex set
    private void SetTile(BaseTile tile_)
    {
        //check if tile is in the right position otherwise throw error
        Tile = tile_;
    }
}

#region TileSet
/// <tilesNeeded>
/// Transport - traveling between platforms
/// Monster - starts with an enemy
/// Treasure - starts with some treasure
/// </summary>
public class BaseTile
{
    /// Tiles, mainly used in world generator and other scripts to create objects using this TileController script

    protected GameObject tileObject;
    protected TileController tileController;
    [SerializeField] protected Vector3 standingPos;

    public GameObject TileObject { get { return tileObject; } set { tileObject = value; } }
    public TileController TileController { get { return tileController; } }
    public Vector3 StandingPos { get { return standingPos; } private set { standingPos = value; } }

    //the basic tile, game Characters can walk on this
    public BaseTile(GameObject baseTile)
    {
        tileObject = Object.Instantiate(baseTile);
        tileController = tileObject.GetComponent<TileController>();
    }
    public BaseTile(GameObject baseTile, TileController tController)
    {
        tileObject = baseTile;
        tileController = tController;
        SetStandingPos();
    }

    public void SetPosition(Vector3 setPosition)
    {
        tileObject.transform.position = setPosition;
        SetStandingPos();
    }

    public void SetStandingPos()
    {
        //set standing point a few different ways


        //basic
        Bounds bounds = tileObject.GetComponent<MeshRenderer>().bounds;
        StandingPos = new Vector3(tileObject.transform.position.x, bounds.size.y / 2, tileObject.transform.position.z);
    }
}
# endregion
