////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  Node : class | Written by Anthony Pascone                                                                     //
//  
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using UnityEngine;

public class Node {

    private bool isOccupied = false;
    private bool isObstacle = false;

    public int gridXLoc;
    public int gridZLoc;
    public Vector3 worldPos;
    public TileScript parentTile;

    public Node fromNode;

    public List<Node> connections;

    public bool diagonal = false;

    public int gCost;   // The cost to the next node
    public int hCost;   // The cost to the target node

    public Node(TileScript parentT, Vector3 pos, int gridX, int gridZ)
    {
        parentTile = parentT;
        worldPos = pos;
        gridXLoc = gridX;
        gridZLoc = gridZ;

        connections = new List<Node>();
    }

    public Vector3 GetWorldPosition()
    {
        return parentTile.transform.position;
    }

    public int GetFCost()
    {
        return gCost + hCost;
    }

    public bool GetOccupied()
    {
        return isOccupied;
    }

    public bool GetObstacle()
    {
        return isObstacle;
    }

    public void SetOccupied(bool occcupied)
    {
        isOccupied = occcupied;
    }

    public void SetObstacle(bool obstacle)
    {
        isObstacle = obstacle;
    }

    public bool IsWalkable()
    {
        return !isObstacle && !isOccupied;
    }
}
