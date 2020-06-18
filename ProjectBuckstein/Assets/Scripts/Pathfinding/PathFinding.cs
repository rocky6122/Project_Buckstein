////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  PathFinding : class | Written by Parker Staszkiewicz                                                          //
//  Specific application of A*, called DnD*, which does not allow for diagonal movement around obstacle coerners; //
//  additionally, every other diagonal move costs double. This cost is determined when retracing the path,        //
//  prior to sending it to a NavAgent.                                                                            //
//                                                                                                                //
//  All pathfinding is done on a coroutine, which allows any/all pathfinders to do an amount of work each frame   //
//  until they have finished their pathfinding and are freed.                                                     //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour {

    GridManagerScript grid;
    PathRequestManager pathRequester;

    bool endededOnDiagonal;

    void Start()
    {
        grid = GridManagerScript.instance;
        pathRequester = PathRequestManager.instance;
    }

    /// <summary>
    /// Begins searching for a viable path
    /// </summary>
    /// <param name="start">Node that begins the path.</param>
    /// <param name="end">Node that ends the path.</param>
    /// <param name="diagonal">Whether the unit has already moved an odd number of diagonals.</param>
    public void StartPath(Node start, Node end, bool diagonal)
    {
        endededOnDiagonal = diagonal;
        StartCoroutine(FindPath(start, end));
    }

    /// <summary>
    /// Coroutine to find the best path according to the DnD* algorithm.
    /// </summary>
    /// <param name="start">Node which starts the path.</param>
    /// <param name="end">Node that ends the path.</param>
    private IEnumerator FindPath(Node start, Node end)
    {
        Node[] nodePath = new Node[0];

        bool pathSuccess = false;

        Node startNode = start;
        Node targetNode = end;

        // If the target is able to be pathed to...
        if(targetNode.IsWalkable())
        {
            // Prepare sets of nodes for storing information
            List<Node> openSet = new List<Node>(grid.length * grid.width); // creates open list of max size equal to max number of tiles in grid
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);

            // While there are still nodes left to process...
            while (openSet.Count > 0)
            {
                Node currentNode = openSet[0];

                openSet.RemoveAt(0);

                closedSet.Add(currentNode);

                // Check to see if the node being processed is the target
                if (currentNode == targetNode)
                {
                    pathSuccess = true;
                    break;
                }

                // Get connections from this node
                List<Node> connections = currentNode.connections;

                /// Check every connection for the best next node to check
                foreach (Node connection in connections)
                {
                    if (!connection.IsWalkable() || closedSet.Contains(connection))
                    {
                        // we skip over this connecting node
                        continue;
                    }

                    int newMoveCost = currentNode.gCost + Heuristic(currentNode, connection);

                    if (newMoveCost < connection.gCost || !openSet.Contains(connection))
                    {
                        connection.gCost = newMoveCost;
                        connection.hCost = Heuristic(connection, targetNode);
                        connection.fromNode = currentNode;

                        if (!openSet.Contains(connection))
                        {
                            openSet.Add(connection);
                        }
                    }
                }
            }
        }

        yield return null;

        if (pathSuccess)
        {
            nodePath = RetracePath(startNode, targetNode);
        }
        else
        {
            nodePath = null;
        }

        pathRequester.FreePathfinder(this, nodePath, pathSuccess, endededOnDiagonal);
    }

    /// <summary>
    /// We only have access to the target node, 
    /// we retrace the pointer path back to the start node.
    /// </summary>
    /// <param name="startNode">Node which starts the path.</param>
    /// <param name="targetNode">Node which ends the path.</param>
    /// <returns>Path for NavAgent to travel.</returns>
    private Node[] RetracePath(Node startNode, Node targetNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = targetNode;

        bool hasHadDiagonal = endededOnDiagonal;

        while (currentNode != startNode)
        {
            path.Add(currentNode);

            // This is where DnD* handles the double cost for even numbered diagonal movement.
            if (IsDiagonalTo(currentNode, currentNode.fromNode))
            {
                if (!hasHadDiagonal)
                {
                    hasHadDiagonal = true;
                }
                else
                {
                    path.Add(currentNode.fromNode);
                    hasHadDiagonal = false;
                }
            }

            currentNode = currentNode.fromNode;
        }

        endededOnDiagonal = hasHadDiagonal;

        Node[] nodePath = new Node[path.Count];

        for (int i = 0; i < path.Count; i++)
        {
            nodePath[i] = path[i];
        }

        // Reverse the path so that it starts at the start node
        Array.Reverse(nodePath);
        return nodePath;
    }

    /// <summary>
    /// Determines if two nodes are diagonal to each other.
    /// </summary>
    /// <returns>Wether the two nodes are diagonal to each other.</returns>
    private bool IsDiagonalTo(Node nodeA, Node nodeB)
    {
        bool diagonal = false;

        int nodeAX = nodeA.parentTile.xLoc;
        int nodeAZ = nodeA.parentTile.zLoc;

        int nodeBX = nodeB.parentTile.xLoc;
        int nodeBZ = nodeB.parentTile.zLoc;

        if (nodeBX == nodeAX + 1 || nodeBX == nodeAX - 1)
        {
            if (nodeBZ == nodeAZ + 1 || nodeBZ == nodeAZ - 1)
            {
                diagonal = true;
            }
        }

        return diagonal;
    }

    /// <summary>
    /// Diagonal Manhatten Heuristic.
    /// </summary>
    int Heuristic(Node nodeA, Node nodeB)
    {
        int D = 5;
        int D2 = 10;

        int dx = Mathf.Abs(nodeA.parentTile.xLoc - nodeB.parentTile.xLoc);
        int dy = Mathf.Abs(nodeA.parentTile.zLoc - nodeB.parentTile.zLoc);
        return D * (dx + dy) + (D2 - 2 * D) * Mathf.Min(dx, dy);
    }
}
