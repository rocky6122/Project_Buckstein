////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  NavAgent : class | Written by Parker Staszkiewicz                                                             //
//  The NavAgent handles the physical movement of an object which has a path found by a Pathfinder.               //
//  NavAgent is run on coroutines so that the game continues to run normally while the GameObject moves.          //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class NavAgent : MonoBehaviour {

    private Unit self;

    public int stoppingDistance;
    public float speed;

    public bool isMoving = false;
    private bool pathFinished = true;

    bool fromNetwork = false;
    bool endOnDiagonal = false;

    Node[] nodePath;

    int targetIndex = 0; // The current index of the NodePath the agent is moving towards

    private void Start()
    {
        // Self refers to the unit that this NavAgent will control
        self = GetComponent<Unit>();
    }

    /// <summary>
    /// Grabs the path from the targetNode and determines if the path is found.
    /// </summary>
    /// <param name="targetNode">The node that has been selected to path towards.</param>
    public void MoveToNode(Node targetNode)
    {
        fromNetwork = false;

        // Because the tiles store the paths to themselves based on the current 
        // unit selected, we can get that path here.
        Node[] path = targetNode.parentTile.GetPath();
        
        if (path != null)
        {
            PathFound(path, true);
        }
    }

    /// <summary>
    /// Takes a predetermined path and moves a networked unit.
    /// </summary>
    /// <param name="path">The array of nodes which makes the path.</param>
    /// <param name="diagonal">Whether the path ends on an odd number diagonal.</param>
    public void MoveToNode(Node[] path, bool diagonal)
    {
        fromNetwork = true;
        endOnDiagonal = diagonal;

        if (path == null)
        {
            return;
        }

        PathFound(path, true);
    }

    /// <summary>
    /// Sets the nodePath variable and calls the function to StartMoving
    /// </summary>
    /// <param name="newPath">The path that will be traveled</param>
    /// <param name="pathFound">Bool determining that the path is complete</param>
    public void PathFound(Node[] newPath, bool pathFound)
    {
        if (pathFound)
        {
            nodePath = newPath;
            StartMoving();
        }
    }

    /// <summary>
    /// Calls coroutine to move Unit towards nodes on the path
    /// </summary>
    public void StartMoving()
    {
        // In case we were already moving somewhere
        // else, we stop the coroutine
        StopCoroutine(FollowPath());

        self.Tile.Select(false);

        if (!fromNetwork)
        {
            GridManagerScript.instance.ClearDisplayedTiles();
        }
        else
        {
            PlayerUnitManager.instance.RefreshWalkableTiles();
        }

        // Now we can start it again
        pathFinished = false;
        isMoving = true;
        StartCoroutine(FollowPath());
    }

    /// <summary>
    /// Clears variables that were used for the current path. Updates the unit.
    /// </summary>
    /// <param name="endNode">The Node the path has ended at</param>
    public void FinishMoving(Node endNode)
    {
        StopCoroutine(FollowPath());
        targetIndex = 0;

        // Updates Unit 
        self.Tile = endNode.parentTile;

        bool diagonal = fromNetwork ? endOnDiagonal : endNode.diagonal;

        if (fromNetwork)
        {
            PlayerUnitManager.instance.RefreshWalkableTiles();
        }

        self.ReduceMoves(nodePath.Length, diagonal);


        isMoving = false;
        nodePath = null;
        pathFinished = true;
    }

    /// <summary>
    /// Coroutine for moving to next node in the path.
    /// </summary>
    IEnumerator FollowPath()
    {
        self.Tile.SetOccupy(false);
        Node currentWaypoint = nodePath[targetIndex];
        currentWaypoint.parentTile.SetOccupy(true);

        while (true)
        {
            // If we have reached our current destination
            if (transform.position == currentWaypoint.GetWorldPosition())
            {
                // our last waypoint is no longer being used
                currentWaypoint.parentTile.SetOccupy(false);

                // if we have indexed past the last node
                if (targetIndex >= nodePath.Length - 1 - stoppingDistance)
                {
                    // reset everything and be done moving
                    currentWaypoint.parentTile.SetOccupy(true);
                    FinishMoving(currentWaypoint);
                    yield break;
                }
                // If the next node is not being used
                else if (!nodePath[targetIndex + 1].GetOccupied())
                {
                    // our index is now the next waypoint in the array
                    targetIndex++;
                }

                // Assign the new waypoint based on current index
                currentWaypoint = nodePath[targetIndex];
                // Make that waypoint used so that no other NavAgent can use it
                currentWaypoint.parentTile.SetOccupy(true);
            }

            // move towards that spot
            if (isMoving)
            {
                transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.GetWorldPosition(), speed * Time.deltaTime);
            }
            // yield (wait) one frame before continuing this coroutine
            yield return null;

        }
    }

    /// <summary>
    /// Returns whether the NavAgent is currently moving on a path.
    /// </summary>
    /// <returns>Bool pathFinished</returns>
    public bool GetPathFinished()
    {
        return pathFinished;
    }
}
