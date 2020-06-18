////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  EnemyUnitManager : class | Written by Parker Staszkiewicz and Anthony Pascone                                 //
//  Inherits from UnitManager.  Implements behaviors specific to Enemy Unit management.                           //
//  Specifically overrides SpawnUnits(), StartTurn(), and EndTurn()                                               //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class EnemyUnitManager : UnitManager {

    #region SINGLETON
    public static EnemyUnitManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion

    public GameObject[] enemyUnitPrefabs;

    // index to be used for iterating through unit turns
    int unitIndex = 0;

    /// <summary>
    /// Goes through the JSON mapFile to find any "2"'s to spawn a sniper unit at that spot in the grid and initialize that unit
    /// </summary>
    protected override void SpawnUnits()
    {
        int length = GridManagerScript.instance.length;
        int width = GridManagerScript.instance.width;

        JsonData data = JSONReadManager.GetItemData("mapFile");

        GameObject holder = new GameObject("Enemy Units");

        for (int i = 0; i < length; ++i)
        {
            for (int j = 0; j < width; ++j)
            {
                int unitID = (int)data["array"][i][j];

                int x = j;
                int z = length - 1 - i;

                GameObject obj = null;

                if (unitID == 2)
                {
                    obj = Instantiate(enemyUnitPrefabs[0], holder.transform);
                }
                else if (unitID == 3)
                {
                    obj = Instantiate(enemyUnitPrefabs[1], holder.transform);
                }
                else if (unitID == 4)
                {
                    obj = Instantiate(enemyUnitPrefabs[2], holder.transform);
                }

                if (obj != null)
                {
                    EnemyUnit unit = obj.GetComponent<EnemyUnit>();

                    unit.Initialize(this, nextUnitID);
                    ++nextUnitID;

                    TileScript tile = grid[x, z].GetComponent<TileScript>();

                    unit.Tile = tile;

                    unitList.Add(unit);

                    //Make Obstacles position equal to the grid coordinates given and add half of the size of the mesh renderer.y to not have the obstacle object halfway through the floor
                    obj.transform.position = new Vector3(tile.transform.position.x, 0.0f,
                        tile.transform.position.z);

                    tile.SetOccupy(true);
                }
            }
        }
    }

    /// <summary>
    /// Go through every unit and start the Utility AI for them
    /// </summary>
    public override void StartTurn()
    {
        ResetUnits();

        GridManagerScript.instance.ToggleTileHover(false);

        unitIndex = 0;

        //Make the camera follow the enemy units
        Camera.main.GetComponent<CameraScript>().SetCameraFollow(true);

        RunNextUnit();

    }

    /// <summary>
    /// Activates UtilityAI on each Unit until all have acted. Then Prepares to end turn.
    /// </summary>
    private void RunNextUnit()
    {
        if (unitIndex > unitList.Count - 1)
        {
            // end the turn
            ChampNet.ChampNetManager.SendTurnStartMessage();

            GameManager.instance.LocalTurnStart();

            return;
        }

        EnemyUnit pUnit = unitList[unitIndex].GetComponent<EnemyUnit>();

        SelectUnit(pUnit);

        StopCoroutine(DoUnitTurn(null)); // this line should be irrelevent. its there for safety
        StartCoroutine(DoUnitTurn(pUnit));
    }

    /// <summary>
    /// Ienumerator for handling UtilityAI with deliberate delays.
    /// </summary>
    IEnumerator DoUnitTurn(EnemyUnit pUnit)
    {
        yield return new WaitForSecondsRealtime(0.75f);

        StartCoroutine(pUnit.GetUtilityAI().DoActions());

        while (pUnit.GetUtilityAI().IsActing())
        {
            yield return null;
        }

        ++unitIndex;
        RunNextUnit();
    }

    /// <summary>
    /// Overrides abstract base. Cleans up Manager after turn.
    /// </summary>
    public override void EndTurn()
    {
        CameraScript cam = Camera.main.GetComponent<CameraScript>();

        cam.SetCameraFollow(false);

        DeselectUnit();

        EndShooting();

        GridManagerScript.instance.ToggleTileHover(true);
    }

    /// <summary>
    /// Returns the index of the Unit currently being manipulated
    /// by the manager.
    /// </summary>
    public int GetUnitIndex()
    {
        return unitIndex;
    }

    /// <summary>
    /// Returns the position of the closest player unit to a specificed enemy unit
    /// </summary>
    /// <param name="enemyUnit">The specific enemy unit that is used to find the closest player unit to</param>
    /// <returns></returns>
    public Vector3 GetClosestPlayerUnitPosition(EnemyUnit enemyUnit, out Unit closestUnit)
    {
        List <Unit> unitList = PlayerUnitManager.instance.GetUnitList();

        float shortest = Vector3.Distance(enemyUnit.transform.position, unitList[0].transform.position);
        int indexOfShortest = 0;

        for(int i = 1; i < unitList.Count; ++i)
        {
            float temp = Vector3.Distance(enemyUnit.transform.position, unitList[i].transform.position);
            if(temp < shortest)
            {
                shortest = temp;
                indexOfShortest = i;
            }
        }

        closestUnit = unitList[indexOfShortest];
        return unitList[indexOfShortest].transform.position;
    }
}
