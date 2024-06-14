using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class WorldGenerator : MonoBehaviour
{
    public enum PLATFORMSHAPE
    {
        SQUARE,
        RECTANGLE,
        TRIANGLE,
        DIAMOND,
        AMOUNTOFSHAPES // [KEEP AS LAST ITEM] not a shape
    }

    //if needed, to remove left over pieces in platform generation make a list, add non used pieces to it and delete them all at the end of generation
    #region Map
    [SerializeField] private TileController controller;

    private int numberOfPlatforms;// this number is to count the number of platforms created in the generating process
    private List<Vector3> platformPlacements;
    private int platformPlacementLevel;
    private List<BaseTile> map;

    public int amountOfPlatforms;
    public int minSizeOfPlatforms;
    public int maxSizeOfPlatforms;
    public int amountOfBridges;
    public int minSizeOfBridges;
    public int maxSizeOfBridges;
    #endregion

    #region Characters

    #endregion

    void Start()
    {
        map = new List<BaseTile>();
        platformPlacements = new List<Vector3>();

        GenerateMap();
    }

    public void GenerateMap()
    {
        // add the amount of platforms to the map list
        for (int i = 0; i < amountOfPlatforms; i++)
        {
            BaseTile[] newPlatform;
            Vector3 platformPlacement;

            numberOfPlatforms++;
            platformPlacementLevel = 0;

            newPlatform = CreatePlatformTiles();
            platformPlacement = FindPlatformPlacement(newPlatform);

            ShapePlatform(newPlatform, platformPlacement);

            for (int j = 0; j < newPlatform.Length; j++)
                map.Add(newPlatform[j]);
        }
    }

    private BaseTile[] CreatePlatformTiles()
    {
        int size;
        BaseTile[] newPlatform;
        BaseTile newTile;
        GameObject tileGroup;

        size = UnityEngine.Random.Range(minSizeOfPlatforms, maxSizeOfPlatforms);
        newPlatform = new BaseTile[size];
        tileGroup = new GameObject($"Platform {numberOfPlatforms}");

        for (int i = 0; i < size; i++)
        {
            //add method to select tile type
            newTile = controller.CreateTile(TILETYPE.BASE);
            Debug.Log($"new tile made: {newTile.TileObject.name}");
            newTile.TileObject.name = newTile.TileObject.name + $"{i}";
            newTile.TileObject.transform.parent = tileGroup.transform;
            newPlatform[i] = newTile;
        }

        return newPlatform;
    }

    private Vector3 FindPlatformPlacement(BaseTile[] platform, int avoidDirection = -1)
    {
        Vector3 foundPlacement;
        int separationDistance;
        int chooseDirection;

        separationDistance = platform.Length / 2;
        platformPlacementLevel++;

        // grab position of last tile made, or start from zero
        if (map.Count != 0) { foundPlacement = map[map.Count - (int)Mathf.Sqrt(platform.Length)].TileObject.transform.position; }// will grab a tile around half way through
        else { foundPlacement = Vector3.zero; }

        //consolidate these
        if (avoidDirection == -1) chooseDirection = UnityEngine.Random.Range(0, 3);
        else chooseDirection = avoidDirection;
        // make sure direction is viable
        if (avoidDirection != -1)
        {
            if (avoidDirection == chooseDirection && avoidDirection != 3) { chooseDirection++; }
            else { chooseDirection = 0; }
        }

        // find a placement
        if (chooseDirection == 0) { foundPlacement = foundPlacement + (Vector3.forward * separationDistance); }
        else if (chooseDirection == 1) { foundPlacement = foundPlacement + (Vector3.right * separationDistance); }
        else if (chooseDirection == 2) { foundPlacement = foundPlacement + (Vector3.back * separationDistance); }
        else if (chooseDirection == 3) { foundPlacement = foundPlacement + (Vector3.left * separationDistance); }

        // check if placement is close to another placement, if so find a new placement
        if (platformPlacements.Count != 0 && platformPlacementLevel <= 3) // dont check on first placement or if wrapping back around
        {
            Debug.Log("checking for intersecting platforms");
            int intersectingDistance = (int)Mathf.Sqrt(platform.Length) -1;
            bool foundPlacementIntersects = false;

            //debug
            Vector3 intersectPlatform = Vector3.zero;

            for (int i = 0; i < platformPlacements.Count; i++)
            {
                float distanceBetweenPlacements = Vector3.Distance(foundPlacement, platformPlacements[i]);
                if (distanceBetweenPlacements > intersectingDistance) { Debug.Log("intersect found"); intersectPlatform = platformPlacements[i]; foundPlacementIntersects = true; break; }
            }

            if (foundPlacementIntersects) { Debug.Log($"found intersect at {intersectPlatform}"); foundPlacement = FindPlatformPlacement(platform, chooseDirection); }
        }
        else if (platformPlacementLevel > 3) { Debug.Log("end the platforms here"); }

        //instead of doing all this, let platforms combine and remove similar tiles, with a list of important tiles that take priority

        platformPlacements.Add(foundPlacement);

        return foundPlacement;
    }

    //all shapes except square could use work and variants, new shapes are welcome
    private void ShapePlatform(BaseTile[] platform, Vector3 startPos)
    {
        PLATFORMSHAPE shape;
        int platformSqrt;
        Vector3 tileSize;
        int setTileNum;

        // choose shape of platform
        shape = (PLATFORMSHAPE)UnityEngine.Random.Range(0, (int)PLATFORMSHAPE.AMOUNTOFSHAPES);
       
        // shape room
        switch (shape)
        { 
            case PLATFORMSHAPE.SQUARE:
                // find square root for list size and create rows of it
                platformSqrt = (int)Mathf.Sqrt(platform.Length);
                tileSize = platform[0].TileObject.GetComponent<Renderer>().bounds.size;
                setTileNum = 0;

                for (int z = 0; z <= platformSqrt; z++)
                {
                    for (int x = 0; x <= platformSqrt; x++)
                    {
                        if (setTileNum >= platform.Length) 
                        {
                            for (int i = 1; i <= x; i++)//M#: loop starts at 1 because tileSetNum is added to before it breaks; see containing int x loop
                                platform[setTileNum - i].TileObject.SetActive(false);

                            break;
                        }

                        float xPos = startPos.x + (x * tileSize.x);
                        float zPos = startPos.z + (z * tileSize.z);

                        platform[setTileNum].SetPosition(new Vector3(xPos, startPos.y, zPos));

                        setTileNum++;
                    }
                }

                return;
            case PLATFORMSHAPE.RECTANGLE:
                // do square equa. then divide by 2 and get a rectangle
                int platformSqrtHalf;
                int platformSqrtDbl;
                int rx; // special x value for rectangles
                int extra; // extra spaces calculated if theres more tiles than handled in shaping
                int rDirection; // the direction of the rectangle

                platformSqrt = (int)Mathf.Sqrt(platform.Length);
                tileSize = platform[0].TileObject.GetComponent<Renderer>().bounds.size;
                platformSqrtHalf = platformSqrt / 2;
                platformSqrtDbl = platformSqrt * 2;
                setTileNum = 0;
                rx = 0;
                extra = 0;
                rDirection = UnityEngine.Random.Range(-1, 2);

                if (rDirection >= 1)
                {
                    for (int z = 0; z <= platformSqrtDbl; z++)
                    {
                        for (rx = 0; rx <= platformSqrtHalf; rx++)
                        {
                            if (z >= platformSqrtDbl && rx >= platformSqrtHalf) { extra = platform.Length - setTileNum; setTileNum = platform.Length; }
                            if (setTileNum >= platform.Length)
                            {
                                for (int i = 1; i < rx + 1 + extra; i++)//M#: loop starts at 1 because tileSetNum is added to before it breaks; see containing int x loop/ add 1 to the loop end to account for the missing loop count at 0
                                { platform[setTileNum - i].TileObject.SetActive(false); }

                                break;
                            }

                            float xPos = startPos.x + (rx * tileSize.x);
                            float zPos = startPos.z + (z * tileSize.z);

                            platform[setTileNum].SetPosition(new Vector3(xPos, startPos.y, zPos));

                            setTileNum++;
                        }
                    }
                }
                else if (rDirection <= 0)
                {
                    for (int z = 0; z <= platformSqrtHalf; z++)
                    {
                        for (rx = 0; rx <= platformSqrtDbl; rx++)
                        {
                            if (z >= platformSqrtHalf && rx >= platformSqrtDbl) { extra = platform.Length - setTileNum; setTileNum = platform.Length; }
                            if (setTileNum >= platform.Length)
                            {
                                for (int i = 1; i < rx + 1 + extra; i++)//M#: loop starts at 1 because tileSetNum is added to before it breaks; see containing int x loop/ add 1 to the loop end to account for the missing loop count at 0
                                { platform[setTileNum - i].TileObject.SetActive(false); }

                                break;
                            }

                            float xPos = startPos.x + (rx * tileSize.x);
                            float zPos = startPos.z + (z * tileSize.z);

                            platform[setTileNum].SetPosition(new Vector3(xPos, startPos.y, zPos));

                            setTileNum++;
                        }
                    }
                }

                return;
            case PLATFORMSHAPE.TRIANGLE:
                // do rectangle equa. then from one side or the other build out decreasing by 2 each time
                int triangleBase;
                int shapeTriangle;
                
                triangleBase = (int)Mathf.Sqrt(platform.Length)+1;
                tileSize = platform[0].TileObject.GetComponent<Renderer>().bounds.size;
                setTileNum = 0;
                shapeTriangle = triangleBase;

                for (int z = 0; z <= triangleBase; z++)
                {
                    for (int x = 0; x <= shapeTriangle; x++)
                    {
                        float xPos = startPos.x + (x * tileSize.x);
                        float zPos = startPos.z + (z * tileSize.z);

                        platform[setTileNum].SetPosition(new Vector3(xPos, startPos.y, zPos));

                        if (shapeTriangle <= 0)
                        {
                            int extraTiles = platform.Length - setTileNum;
                            for (int i = 1; i < extraTiles; i++)//M#: loop starts at 1 because tileSetNum is added to before it breaks; see containing int x loop
                            {
                                platform[setTileNum + i].TileObject.SetActive(false);
                            }

                            break;
                        }

                        setTileNum++;
                    }
                    shapeTriangle--;
                }

                return;
            case PLATFORMSHAPE.DIAMOND:
                // offset square pattern
                platformSqrt = (int)Mathf.Sqrt(platform.Length);
                tileSize = platform[0].TileObject.GetComponent<Renderer>().bounds.size;
                setTileNum = 0;


                for (int z = 0; z <= platformSqrt; z++)
                {
                    for (int x = 0; x <= platformSqrt; x++)
                    {
                        if (setTileNum >= platform.Length)
                        {
                            for (int i = 1; i <= x; i++)//M#: loop starts at 1 because tileSetNum is added to before it breaks; see containing int x loop
                                platform[setTileNum - i].TileObject.SetActive(false);

                            break;
                        }

                        float xPos = startPos.x + (x * tileSize.x);
                        float zPos = startPos.z + (z * tileSize.z);

                        platform[setTileNum].SetPosition(new Vector3(xPos, startPos.y, zPos+x));

                        setTileNum++;
                    }
                }

                return;
            default:
                Debug.LogWarning($"Platform Shape {shape} not registered in method");
                return;
        }

        // create a method to delete overlapping tiles with important one's taking priority?
    }

}