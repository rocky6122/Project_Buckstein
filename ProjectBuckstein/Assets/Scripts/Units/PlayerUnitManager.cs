////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  PlayerUnitManager : class | Written by Parker Staszkiewicz and Anthony Pascone                                //
//  Inherits from UnitManager.  Implements behaviors specific to Player Unit management.                          //
//  Specifically overrides SpawnUnits(), StartTurn(), and EndTurn().                                              //
//  Additionally, handles specific user inputs related to Player Units, such as selecting and deselecting         //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using LitJson;

public class PlayerUnitManager : UnitManager {

    #region SINGLETON
    public static PlayerUnitManager instance;

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

    public GameObject[] playerUnitPrefabs;

    private void Update()
    {
        if (Input.GetMouseButton(1))
        {
            DeselectUnit();

            EndShooting();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            PrepareToShoot();
        }
    }

    /// <summary>
    /// Goes through the JSON mapFile to find any "3"'s to spawn a sniper unit at that spot in the grid and initialize that unit
    /// </summary>
    protected override void SpawnUnits()
    {
        int length = GridManagerScript.instance.length;
        int width = GridManagerScript.instance.width;

        JsonData data = JSONReadManager.GetItemData("mapFile");

        GameObject holder = new GameObject("Player Units");

        for (int i = 0; i < length; ++i)
        {
            for (int j = 0; j < width; ++j)
            {
                int unitID = (int)data["array"][i][j];

                GameObject obj = null;
                int x = j;
                int z = length - 1 - i;

                if (unitID == 5)
                { 
                    obj = Instantiate(playerUnitPrefabs[0], holder.transform);
                }
                else if (unitID == 6)
                {
                    obj = Instantiate(playerUnitPrefabs[1], holder.transform);
                }
                else if (unitID == 7)
                {
                    obj = Instantiate(playerUnitPrefabs[2], holder.transform);
                }

                if (obj != null)
                {
                    PlayerUnit unit = obj.GetComponent<PlayerUnit>();

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

    public override void EndTurn()
    {
        DeselectUnit();
    }

    public bool CheckIfAUnitIsMoving()
    {
        //If any unit in the list is moving, return true
        for(int i = 0; i < unitList.Count; ++i)
        {
            if(unitList[i].IsMoving())
            {
                return true;
            }
        }
        return false;
    }

    public override void StartTurn()
    {
        ResetUnits();
    }
}
