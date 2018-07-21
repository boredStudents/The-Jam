﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Map  {

    const int RegionHeigh = 3;
    const int RegionWidth = 5;
    const int RegionLength = 5;
    
    public static Tile[,,] map;
    
    static Map()
    {
        map = new Tile[5, 5, 4];
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                map[x, y, 0] = new Tile(new SimpleCords(x, y, 0)) { Passable = true,Empty=false};
            }
        }

        map[0, 2, 0] = new Tile(new SimpleCords(0, 2, 0)) { Passable = true };
        map[0, 2, 1] = new Tile(new SimpleCords(0, 2, 1)) { Passable = true };
        map[0, 2, 2] = new Tile(new SimpleCords(0, 2, 2)) { Passable = true };
        map[0, 2, 3] = new Tile(new SimpleCords(0, 2, 3)) { Passable = true };

        map[1, 2, 0] = new Tile(new SimpleCords(1, 2, 0)) { Passable = true };
        map[1, 2, 1] = new Tile(new SimpleCords(1, 2, 1)) { Passable = true };
        map[1, 2, 1].TileObjects.PlaceObject(ObjectBox.GetObjectByName("CoverPartial"));

        map[1, 3, 0].TileObjects.PlaceObject(ObjectBox.GetObjectByName("WallHigh"),map[2,3,0]);
        map[1, 4, 0].TileObjects.PlaceObject(ObjectBox.GetObjectByName("WallHigh"), map[2, 4, 0]);

        map[4, 4, 0].TileObjects.PlaceObject(ObjectBox.GetObjectByName("WallLow"), map[4, 3, 0]);


    }

    public static void Spawn()
    {
        foreach (Tile item in map)
        {
            if (item != null)
            {
                item.SpawnObject();
            }
        }

        UpdateExistancePassability();
        UpdateCover();
        UpdateTraversals();
        UpdateObjects();

        foreach (Tile item in map)
        {
            if (item!=null)
            {
                Debug.Log("gonna do the lines now");
                item.Traversals.SpawnDebugLines();
            }
        }

    }

    public static void UpdateCover()
    {
        foreach (Tile item in map)
        {
            if (item!=null)
            {
                foreach (var side in item.Cover.GetAsDictionary())
                {
                    item.Cover[side.Key] = CheckCover(item, side.Key);
                }
                Debug.Log(item.Cover.CoverValue());
            }
        }
    }
    /// <summary>
    /// Updates the traversals
    /// </summary>
    public static void UpdateTraversals()
    {
        // currently you can only go up 1 layer and move straight

        foreach (var item in map)
        {
            if (item!= null && item.UnObstructed)
            {
                foreach (var side in item.Traversals.GetAsDictionary())
                {
                    Tile neighbour = GetTraversabalTile(item, side.Key);
                    if (neighbour != null && neighbour.UnObstructed)
                    {
                        int difference = neighbour.position.h - item.position.h;
                        Debug.Log("Difference: " + difference);
                        if (difference == 0)
                        {
                            item.Traversals[side.Key] = new Traversal(TraversalType.Walking, item, neighbour);
                        }
                        else
                        {
                            if (difference == 1)
                            {
                                item.Traversals[side.Key] = new Traversal(TraversalType.ClimbUp,item,neighbour);
                            }
                            else
                            {
                                if (difference == -1)
                                {
                                    item.Traversals[side.Key] = new Traversal(TraversalType.ClimbDown, item, neighbour);
                                }
                                else
                                {
                                    if (difference < -1)
                                    {
                                        item.Traversals[side.Key] = new Traversal(TraversalType.DropDown, item, neighbour);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("no neighbour");
                    }
                }             
            }
        }
    }

    public static void UpdateExistancePassability()
    {
        foreach (Tile item in map)
        {
            if (item != null)
            {
                item.UnObstructed = item.Passable;

                if (item.UnObstructed)
                {
                    item.UnObstructed = !item.TileObjects.ObstructsTile();
                    if (item.UnObstructed)
                    {
                        if (GetTileWithCords(item.position.GetAbove()) != null || GetTileWithCords(item.position.GetAbove(2) )!= null)
                        {
                            item.UnObstructed = false;
                            Debug.Log("Found obstructed Tile " + item.position);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Updates Object color
    /// </summary>
    public static void UpdateObjects()
    {
        foreach (Tile item in map)
        {
            if (item != null)
            {

                if (item.Cover.CoverValue() > 0)
                {
                    if (item.Cover.CoverValue()>1)
                    {
                        Debug.Log("There should be a colorchange");
                        item.myObject.GetComponent<MeshRenderer>().material.color = Color.blue;
                    }
                    else
                    {
                        Debug.Log("There should be a colorchange");
                        item.myObject.GetComponent<MeshRenderer>().material.color = Color.red;
                    }
                    
                }
                else
                {
                    Renderer mr = item.myObject.GetComponent<Renderer>();
                    mr.material.color = Color.white;
                }


            }
        }
    }

    public static Tile GetTileWithCords(SimpleCords cords)
    {
        try
        {
            return map[cords.x, cords.y, cords.h];
        }
        catch (System.IndexOutOfRangeException)
        {
            return null;
        }       
    }

    private static CoverType CheckCover(Tile origin, Direction dir)
    {
        CoverType type = CoverType.None;
        if (origin.UnObstructed)
        {
            type = origin.TileObjects.GetHighestCoverInDirection(dir);

            SimpleCords location = new SimpleCords(origin.position);
            switch (dir)
            {
                case Direction.North:
                    location.Offset(0, 1, 0);
                    break;
                case Direction.East:
                    location.Offset(1, 0, 0);
                    break;
                case Direction.South:
                    location.Offset(0, -1, 0);
                    break;
                case Direction.West:
                    location.Offset(-1, 0, 0);
                    break;
                default:
                    break;
            }
            // first check if we get cover from an an other tile
            Tile workTile = GetTileWithCords(location.Offset(0, 0, 1));
            if (workTile!=null && !workTile.Empty)
            {
                type.AddUp(CoverType.Partial);
            }
            workTile = GetTileWithCords(location.Offset(0, 0, 1));
            if (workTile != null && !workTile.Empty)
            {
                return type;
            }
            // check for cover from the tile next to us
            workTile = GetTileWithCords(location.Offset(0, 0, 1));
            if (workTile != null && !workTile.Empty)
            {
                return workTile.TileObjects.GetHighestCoverInDirection(dir + 2 % 4);
            }
            
        }
        return type;
    }

    public static Tile GetTraversabalTile(Tile tile, Direction dir)
    {
        SimpleCords OtherCord = tile.position.GetInDirection(dir, 1).GetAbove();
        
        for (int h = OtherCord.h; h >= 0 ; h--)
        {

            Tile tmp = GetTileWithCords(new SimpleCords(OtherCord.x,OtherCord.y,h));
            if (tmp != null && tmp.Empty == false)
            {
                if (tmp.Passable == true)
                {
                    return tmp;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                Debug.LogError("We didnt find a tile");
            }
        }
        return null;
    }

}
