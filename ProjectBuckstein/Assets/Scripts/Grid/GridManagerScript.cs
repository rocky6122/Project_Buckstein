////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  GridManagerScript: class | Written by Anthony Pascone and Parker Staszkiewicz                                 //
//        
//        
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class GridManagerScript : MonoBehaviour
{

    public static GridManagerScript instance = null;

    public GameObject[,] grid;
    public GameObject obstaclePrefab;
    private List<TileScript> walkableTiles;
    public GameObject tilePrefab;
    public GameObject floorPrefab;

    private LayerMask tileLayer;

    private JsonData data;

    public int length;
    public int width;

    private void Awake()
    {
        //Make sure only one instance of GridManager is around
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(instance);
        }
    }

    public void Initialize()
    {
        data = JSONReadManager.GetItemData("mapFile");

        walkableTiles = new List<TileScript>();

        CreateGrid();
        CreateObstacles();

        FillConnections();

        ClearDisplayedTiles();

        tileLayer = LayerMask.GetMask("Tile");
    }


    private void CreateGrid()
    {
        width = (int)data["width"];
        length = (int)data["length"];

        grid = new GameObject[width, length];  // length is z; width is x

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                //Spawn The Grid
                grid[x, z] = Instantiate(tilePrefab, gameObject.transform);
                //Set Tiles x and y

                //Make grid shape using the sprite renderer of the tile sprite to seperate each out accordingly
                grid[x, z].transform.position = new Vector3(x * tilePrefab.GetComponent<SpriteRenderer>().bounds.size.x, 0f,
                    z * tilePrefab.GetComponent<SpriteRenderer>().bounds.size.x);

                grid[x, z].GetComponent<TileScript>().Initializer(grid[x, z].transform.position, x, z);

                //This creates an empty gameobject in the middle of the grid for the camera to look at
                if (x == width / 2 && z == length / 2)
                {
                    GameObject mid = new GameObject("GridMid");
                    mid.transform.position = grid[x, z].transform.position;
                }
            }
        }

        GameObject floor = Instantiate(floorPrefab);
        floor.GetComponent<Floor>().Reshape(width, length);
        floor.transform.position = new Vector3(grid[width / 2, length / 2].transform.position.x, -0.51f, grid[width / 2, length / 2].transform.position.z);
    }

    private void FillConnections()
    {
        foreach (GameObject tileObject in grid)
        {
            Node n = tileObject.GetComponent<TileScript>().thisNode;

            n.connections = GetConnections(n);
        }
    }

    public void DisplayWalkableTiles(TileScript tile, int movesLeft, bool endedOnDiagonal)
    {
        Queue<TileScript> tileQueue = new Queue<TileScript>();

        ClearWalkableTiles();

        // number of total squares to check
        int iterations = (movesLeft * 3) * (movesLeft * 3);
        // vector values for direction of movement
        int di = 1;
        int dj = 0;
        // length of current segment
        int segmentLength = 1;

        // SET UP
        int i = tile.xLoc;
        int j = tile.zLoc;
        int segmentPassed = 0;

        for (int k = 0; k < iterations - 1; ++k)
        {
            // Make a step, adding direction vector to position vector
            i += di;
            j += dj;
            ++segmentPassed;

            if (i >= 0 && i < width && j >= 0 && j < length)
            {
                TileScript newTile = grid[i, j].GetComponent<TileScript>();

                if (!newTile.thisNode.GetOccupied() && !newTile.thisNode.GetObstacle())
                {
                    //newTile.RenderOnScreen(true);
                    tileQueue.Enqueue(newTile);
                }
            }

            if (segmentPassed == segmentLength)
            {
                // done with current segment
                segmentPassed = 0;

                // "rotate" direction vector
                int buffer = di;
                di = -dj;
                dj = buffer;

                // increase segment length if necessary
                if (dj == 0)
                {
                    ++segmentLength;
                }
            }
        }


        // Now path finds to all available tiles to ensure they can be reached within the limit
        while (tileQueue.Count > 0)
        {
            TileScript tileToCheck = tileQueue.Dequeue();
            tileToCheck.PrepareForPathing(movesLeft);
            PathRequestManager.RequestPath(tile.thisNode, tileToCheck.thisNode, endedOnDiagonal, tileToCheck.StorePath);
        }
    }

    public void AddTileToWalkable(TileScript tile)
    {
        walkableTiles.Add(tile);
    }

    public void ClearWalkableTiles()
    {
        walkableTiles.Clear();
    }

    public List<TileScript> GetWalkableTiles()
    {
        return walkableTiles;
    }

    public void ClearDisplayedTiles()
    {
        foreach (GameObject tile in grid)
        {
            tile.GetComponent<TileScript>().RenderOnScreen(false);
        }
    }

    private void CreateObstacles()
    {
        GameObject holder = new GameObject("Obstacle Holder");

        for (int i = 0; i < length; ++i)
        {
            for (int j = 0; j < width; ++j)
            {
                int isObstacle = (int)data["array"][i][j];

                if (isObstacle == 1)
                {
                    int x = j;
                    int z = length - 1 - i;

                    GameObject obj = Instantiate(obstaclePrefab, holder.transform);

                    //Make Obstacles position equal to the grid coordinates given and add half of the size of the mesh renderer.y to not have the obstacle object halfway through the floor
                    obj.transform.position = new Vector3(grid[x, z].transform.position.x, 0 + (obj.GetComponent<MeshRenderer>().bounds.size.y / 2),
                        grid[x, z].transform.position.z);

                    grid[x, z].GetComponent<TileScript>().SetObstacle(true);
                }
            }
        }
    }

    public List<Node> GetConnections(Node node)
    {
        List<Node> neighbors = new List<Node>();

        bool hasObstacleEast = false;
        bool hasObstacleSouth = false;
        bool hasObstacleWest = false;
        bool hasObstacleNorth = false;

        // Check Obstacles
        int pingObstacleX;
        int pingObstacleZ;

        // Check East Obstacle
        pingObstacleX = node.parentTile.xLoc + 1;
        pingObstacleZ = node.parentTile.zLoc + 0;

        if (pingObstacleX >= 0 && pingObstacleX < width && pingObstacleZ >= 0 && pingObstacleZ < length)
        {
            if (grid[pingObstacleX, pingObstacleZ].GetComponent<TileScript>().IsObstacle())
            {
                hasObstacleEast = true;
            }
        }

        // Check South Obstacle
        pingObstacleX = node.parentTile.xLoc + 0;
        pingObstacleZ = node.parentTile.zLoc - 1;

        if (pingObstacleX >= 0 && pingObstacleX < width && pingObstacleZ >= 0 && pingObstacleZ < length)
        {
            if (grid[pingObstacleX, pingObstacleZ].GetComponent<TileScript>().IsObstacle())
            {
                hasObstacleSouth = true;
            }
        }

        // Check West Obstacle
        pingObstacleX = node.parentTile.xLoc - 1;
        pingObstacleZ = node.parentTile.zLoc + 0;

        if (pingObstacleX >= 0 && pingObstacleX < width && pingObstacleZ >= 0 && pingObstacleZ < length)
        {
            if (grid[pingObstacleX, pingObstacleZ].GetComponent<TileScript>().IsObstacle())
            {
                hasObstacleWest = true;
            }
        }

        // Check North Obstacle
        pingObstacleX = node.parentTile.xLoc + 0;
        pingObstacleZ = node.parentTile.zLoc + 1;

        if (pingObstacleX >= 0 && pingObstacleX < width && pingObstacleZ >= 0 && pingObstacleZ < length)
        {
            if (grid[pingObstacleX, pingObstacleZ].GetComponent<TileScript>().IsObstacle())
            {
                hasObstacleNorth = true;
            }
        }

        // Check East Tile
        int checkX = node.parentTile.xLoc + 1;
        int checkZ = node.parentTile.zLoc + 0;

        if (checkX >= 0 && checkX < width && checkZ >= 0 && checkZ < length)
        {
            neighbors.Add(grid[checkX, checkZ].GetComponent<TileScript>().thisNode);
        }

        // Check South East Tile
        if (!hasObstacleEast && !hasObstacleSouth)
        {
            checkX = node.parentTile.xLoc + 1;
            checkZ = node.parentTile.zLoc - 1;

            if (checkX >= 0 && checkX < width && checkZ >= 0 && checkZ < length)
            {
                neighbors.Add(grid[checkX, checkZ].GetComponent<TileScript>().thisNode);
            }
        }

        // Check South Tile
        checkX = node.parentTile.xLoc + 0;
        checkZ = node.parentTile.zLoc - 1;

        if (checkX >= 0 && checkX < width && checkZ >= 0 && checkZ < length)
        {
            neighbors.Add(grid[checkX, checkZ].GetComponent<TileScript>().thisNode);
        }

        // Check South West Tile
        if (!hasObstacleWest && !hasObstacleSouth)
        {
            checkX = node.parentTile.xLoc - 1;
            checkZ = node.parentTile.zLoc - 1;

            if (checkX >= 0 && checkX < width && checkZ >= 0 && checkZ < length)
            {
                neighbors.Add(grid[checkX, checkZ].GetComponent<TileScript>().thisNode);
            }
        }

        // Check West Tile
        checkX = node.parentTile.xLoc - 1;
        checkZ = node.parentTile.zLoc + 0;

        if (checkX >= 0 && checkX < width && checkZ >= 0 && checkZ < length)
        {
            neighbors.Add(grid[checkX, checkZ].GetComponent<TileScript>().thisNode);
        }

        // Check North West Tile
        if (!hasObstacleWest && !hasObstacleNorth)
        {
            checkX = node.parentTile.xLoc - 1;
            checkZ = node.parentTile.zLoc + 1;

            if (checkX >= 0 && checkX < width && checkZ >= 0 && checkZ < length)
            {
                neighbors.Add(grid[checkX, checkZ].GetComponent<TileScript>().thisNode);
            }
        }

        // Check North Tile
        checkX = node.parentTile.xLoc + 0;
        checkZ = node.parentTile.zLoc + 1;

        if (checkX >= 0 && checkX < width && checkZ >= 0 && checkZ < length)
        {
            neighbors.Add(grid[checkX, checkZ].GetComponent<TileScript>().thisNode);
        }

        // Check North East Tile
        if (!hasObstacleNorth && !hasObstacleEast)
        {
            checkX = node.parentTile.xLoc + 1;
            checkZ = node.parentTile.zLoc + 1;

            if (checkX >= 0 && checkX < width && checkZ >= 0 && checkZ < length)
            {
                neighbors.Add(grid[checkX, checkZ].GetComponent<TileScript>().thisNode);
            }
        }

        return neighbors;
    }

    public TileScript FindClosestTile(Vector3 pos)
    {
        if (!(walkableTiles.Count > 0))
        {
            Debug.LogError("Walkable Tiles empty");
            return null;
        }

        float shortestDist = Vector3.Distance(walkableTiles[0].transform.position, pos);
        TileScript tile = walkableTiles[0];

        for (int i = 1; i < walkableTiles.Count; ++i)
        {
            float temp = Vector3.Distance(walkableTiles[i].transform.position, pos);

            if (temp < shortestDist)
            {
                shortestDist = temp;
                tile = walkableTiles[i];
            }
        }

        return tile;
    }

    public TileScript FindClosestTile(Vector3 pos, List<TileScript> possibleTiles)
    {
        TileScript tile = possibleTiles[0];
        float distance = Vector3.Distance(pos, possibleTiles[0].transform.position);

        for (int i = 1; i < possibleTiles.Count; ++i)
        {
            float newDistance = Vector3.Distance(pos, possibleTiles[i].transform.position);

            if (newDistance < distance)
            {
                distance = newDistance;
                tile = possibleTiles[i];
            }
        }

        return tile;
    }

    public List<TileScript> GetTilesUnitCanSee(Unit pUnit, int shootRange)
    {
        List<TileScript> tilesSeen = new List<TileScript>();

        pUnit.gameObject.layer = Physics.IgnoreRaycastLayer;


        Collider[] hits = Physics.OverlapSphere(pUnit.transform.position, shootRange, tileLayer);

        Vector3 unitOffset = new Vector3(0.0f, 0.5f, 0.0f);

        foreach (Collider col in hits)
        {
            Vector3 direction = col.transform.position - (pUnit.transform.position + unitOffset);

            RaycastHit hit;

            Physics.Raycast(pUnit.transform.position + unitOffset, direction, out hit);

            if (hit.collider != null && hit.collider.gameObject.CompareTag("Tile"))
            {
                tilesSeen.Add(hit.collider.gameObject.GetComponent<TileScript>());
            }
        }

        pUnit.gameObject.layer = 8;

        if (tilesSeen.Count > 0)
        {
            return tilesSeen;
        }
        else
        {
            return null;
        }
    }

    public List<TileScript> GetWalkableTilesInRange(List<TileScript> seenTiles)
    {
        List<TileScript> tilesInRange = new List<TileScript>();

        foreach (TileScript tile in seenTiles)
        {
            if (walkableTiles.Contains(tile))
            {
                tilesInRange.Add(tile);
            }
        }


        if (tilesInRange.Count < 1)
        {
            return null;
        }

        return tilesInRange;
    }

    public void ToggleTileHover(bool toggle)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < length; j++)
            {
                grid[i, j].GetComponent<TileScript>().ToggleHover(toggle);
            }
        }
    }
}
