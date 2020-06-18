////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  PathRequestManager : class | Written by Parker Staszkiewicz                                                   //
//  Manages any requests for pathfinding to be carried out.  Does so by using a queue of requests and a pool of   //
//  pathfinders which are either available or unavailable, as determined by which list they are on.               //
//                                                                                                                //
//  PathRequest : struct | Written by Parker Staszkiewicz                                                         //
//  Container for storing requests to be handled by the manager.  These containers store the start and end nodes, //
//  a bool of whether the unit which requested the path has used an odd number of diagonals, and a callback       //
//  function which will be handled once the path is found.                                                        //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;
using UnityEngine;
using System;

public class PathRequestManager : MonoBehaviour {

    struct PathRequest
    {
        public Node startNode;
        public Node endNode;
        public bool endedOnDiagonal;
        //Unity Action it knows that there will be a function that its parameters are a node 
        //array and a bool, but we dont know what that is yet, we will assign it later
        public Action<Node[], bool, bool> callBack;

        public PathRequest(Node start, Node end, bool diagonal, Action<Node[], bool, bool> func)
        {
            startNode = start;
            endNode = end;
            endedOnDiagonal = diagonal;
            callBack = func;
        }
    }

    struct Container
    {
        public PathRequest request;
        public PathFinding finder;

        public Container(PathRequest req, PathFinding find)
        {
            request = req;
            finder = find;
        }
    }

    #region SINGLETON

    public static PathRequestManager instance;

    public void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        InitializePathfinders();
    }

    #endregion

    private Queue<PathRequest> requestQueue = new Queue<PathRequest>();
    Queue<PathFinding> availablePathfinders;
    List<PathFinding> unavailablePathfinders;
    List<Container> containers;

    private void InitializePathfinders()
    {
        availablePathfinders = new Queue<PathFinding>();
        unavailablePathfinders = new List<PathFinding>();

        containers = new List<Container>();

        GameObject GO = new GameObject("Pathfinder");
   
        for (int i = 0; i < 25; ++i)
        {
            PathFinding finder = GO.AddComponent<PathFinding>();

            availablePathfinders.Enqueue(finder);
        }
    }

    private PathFinding GetAvailablePathfinder()
    {
        PathFinding finder = availablePathfinders.Dequeue();

        unavailablePathfinders.Add(finder);

        return finder;
    }

    public void FreePathfinder(PathFinding finder, Node[] path, bool success, bool diagonal)
    {
        if (unavailablePathfinders.Contains(finder))
        {
            unavailablePathfinders.Remove(finder);
            availablePathfinders.Enqueue(finder);

            FinishRequest(finder, path, success, diagonal);
        }
        else
        {
            Debug.LogError("Pathfinder not on closed list");
        }
    }

    public static void RequestPath(Node pathStart, Node pathEnd, bool diagonal, Action<Node[], bool, bool> callback)
    {
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, diagonal, callback);
        instance.requestQueue.Enqueue(newRequest);
        instance.ProcessNext();
    }

    private void ProcessNext()
    {
        if (requestQueue.Count > 0 && availablePathfinders.Count > 0)
        {
            PathRequest currentRequest = instance.requestQueue.Dequeue();
            PathFinding finder = GetAvailablePathfinder();

            containers.Add(new Container(currentRequest, finder));

            finder.StartPath(currentRequest.startNode, currentRequest.endNode, currentRequest.endedOnDiagonal);
        }
    }

    public void FinishRequest(PathFinding finder, Node[] path, bool success, bool diagonal)
    {
        foreach (Container item in containers)
        {
            if (item.finder == finder)
            {
                item.request.callBack(path, success, diagonal);

                containers.Remove(item);

                ProcessNext();

                break;
            }
        }
    }

    public bool IsRunning()
    {
        return unavailablePathfinders.Count > 0 || requestQueue.Count > 0;
    }
}
