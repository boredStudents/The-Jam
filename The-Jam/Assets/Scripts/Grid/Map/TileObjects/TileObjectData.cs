﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class contains all the tileobject information for a specific Tile
/// </summary>
public class TileObjectData {

    public List<BaseTileObject> Objects = new List<BaseTileObject>();

    public Tile Location;


    public TileObjectData(Tile loc, params BaseTileObject[] objects)
    {
        Location = loc;
        Objects.AddRange(objects);
    }
    /// <summary>
    /// Checks for Cover from a specific direction
    /// </summary>
    /// <param name="directionIn">The direction the OTHER <see cref="Tile"/> is checking</param>
    /// <param name="checkSource">true - This Tile checks; false - other Tile checks</param>
    /// <returns>The amount of Cover received from this an Object</returns>
    public CoverType CoverProvided(Direction directionIn,bool checkSource)
    {
        if (checkSource)
        {
            // we are checking
            return GetHighestCoverInDirection(directionIn);
        }
        else
        {
            // somebody else is checking
            return GetHighestCoverInDirection(directionIn.Invert());
        }
    }

    public bool ObstructsTile()
    {
        foreach (var item in Objects)
        {
            if (item != null)
            {
                Debug.Log("Object Debug");
                if (item.Position == Positioning.Center && item.Solidity == CoverType.Partial)
                {
                    Debug.Log("Obstructing Object found");
                    return true;
                }
            }
        }
        return false;
    }

    public CoverType GetHighestCoverInDirection(Direction dir)
    {
        int highest = 0;
        Positioning pos = (Positioning)(int)dir;
        if (Objects.Count > 0)
        {
            foreach (var item in Objects)
            {
                if (pos == item.Position || item.Position == Positioning.Center)
                {
                    if ((int)item.Solidity>highest)
                    {
                        highest = (int)item.Solidity;
                    }
                }
            }
        }
        return (CoverType)highest;
    }
    public CoverType GetHighestCoverInDirection(int dir)
    {
        return GetHighestCoverInDirection((Direction)dir);
    }

    public BaseTileObject PlaceObject(BaseTileObject obj, Positioning Connected = Positioning.Center)
    {
        if (Connected == Positioning.Center)
        {
            obj.Place(Location);
        }
        else
        {
            obj.Place(Location, Connected);
        }
        Objects.Add(obj);
        return obj;
    }

    public BaseTileObject[] GetAllObjectsInDirection(Direction dir)
    {
        List<BaseTileObject> obj = new List<BaseTileObject>();

        foreach (var item in Objects)
        {
            if (item != null)
            {
                if ((int)item.Position == (int)dir)
                {
                    obj.Add(item);
                }
            }
        }
        return obj.ToArray();
    }

}
