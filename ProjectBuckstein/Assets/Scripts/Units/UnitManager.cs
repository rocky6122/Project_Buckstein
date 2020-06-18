////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  UnitManager : abstract class | Written by Parker Staszkiewicz and Anthony Pascone                             //
//  Abstract base class for a UnitManager; contains functionality for manipulating units and handling actions.    //
//  Classes PlayerUnitManager and EnemyUnitManager inherit from this class and add specific implementations.      //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChampNet;

public abstract class UnitManager : MonoBehaviour
{
    protected int nextUnitID;

    protected GameObject[,] grid;
    protected List<Unit> unitList;

    private Unit selectedUnit = null;
    private Unit networkedUnit = null;

    bool canDeselect = false;

    protected bool isShooting = false;

    private LayerMask unitLayer;

    public GameObject shootRadiusPrefab;

    private GameObject shootRadiusObject;

    public GameObject shotFeedbackPrefab;

    /// <summary>
    /// Intialize the required variables for handling Units.
    /// </summary>
    public void Initialize()
    {
        nextUnitID = 0;

        grid = GridManagerScript.instance.grid;

        unitList = new List<Unit>();

        unitLayer = LayerMask.GetMask("Unit");

        SpawnUnits();

        shootRadiusObject = Instantiate(shootRadiusPrefab, transform);
        shootRadiusObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        MakeUnitsLookAtCamera();
    }

    /// <summary>
    /// Base function, overriden by children classes.
    /// </summary>
    protected abstract void SpawnUnits();


    /// <summary>
    /// Move the selected unit to the secified node
    /// </summary>
    /// <param name="targetNode">The specific node that the unit should move to.</param>
    public void MoveUnit(Node targetNode)
    {
        if (selectedUnit != null && !selectedUnit.IsMoving())
        {
            EndShooting();

            ChampNetManager.SendMoveMessage(selectedUnit.GetID(), targetNode.parentTile.xLoc, targetNode.parentTile.zLoc);

            selectedUnit.MoveToNode(targetNode);
        }
    }

    /// <summary>
    /// Prepares a Networked Unit for navigating to a node in the grid.
    /// </summary>
    /// <param name="targetGridPos">Vector containing the x and z parameters for the location of node in the grid.</param>
    public void PrepareMoveUnit(Vector2 targetGridPos)
    {
        PathRequestManager.RequestPath(networkedUnit.Tile.thisNode, GridManagerScript.instance.grid[(int)targetGridPos.x, (int)targetGridPos.y].GetComponent<TileScript>().thisNode, networkedUnit.endedOnDiagonal, MoveNetworkUnit);
    }

    /// <summary>
    /// Callback function for a Networked Unit to begin moving
    /// </summary>
    /// <param name="path">Array of nodes which will be used by the NavAgent.</param>
    /// <param name="success">Whether or not the path returned from the Pathfinder correctly.</param>
    /// <param name="diagonal">Wether or not the path will cause the Unit to end on an odd number diagonal.</param>
    public void MoveNetworkUnit(Node[] path, bool success, bool diagonal)
    {
        if (success)
        {
            networkedUnit.MoveToNode(path, diagonal);
        }
    }

    /// <summary>
    /// Compares passed Unit to the Networked Unit stored in the Unit Manager.
    /// </summary>
    /// <param name="pUnit">Unit to compare to the Networked Unit variable.</param>
    /// <returns>Returns bool of comparison.</returns>
    public bool IsNetworkedUnit(Unit pUnit)
    {
        if (networkedUnit == null)
        {
            return false;
        }

        return pUnit == networkedUnit;
    }

    /// <summary>
    /// Base function, overriden by children classes.
    /// </summary>
    public abstract void EndTurn();

    /// <summary>
    /// Select the specified unit, which will display all the tiles that it can move to
    /// </summary>
    /// <param name="pUnit">The specific unit that should be selected</param>
    public void SelectUnit(Unit pUnit)
    {
        if ((selectedUnit == null || !selectedUnit.IsMoving()) && !pUnit.GetSelected())
        {
            if (pUnit != null && pUnit != selectedUnit)
            {
                DeselectUnit();
            }

            canDeselect = false;

            pUnit.BecomeSelected();

            UnitSelectMessage message;
            message.unitID = pUnit.GetID();
            message.selected = '1';

            ChampNetManager.SendUnitSelectMessage(message.unitID, message.selected);

            selectedUnit = pUnit;
            GridManagerScript.instance.DisplayWalkableTiles(pUnit.Tile, pUnit.GetMovesLeft(), pUnit.endedOnDiagonal);

            StartCoroutine(DeflagDeselect());
        }
    }

    /// <summary>
    /// Deselects and Reselects unit in order to update
    /// with the result of a Networked Unit movement.
    /// </summary>
    public void RefreshWalkableTiles()
    {
        Unit pUnit = selectedUnit;

        if (pUnit != null)
        {
            // Stores whether we were shooting, because it will be reset
            // when we Deselect.
            bool wasShooting = isShooting;

            DeselectUnit();

            SelectUnit(pUnit);

            // If we were shooting, prepare to shoot again.
            if (wasShooting)
            {
                PrepareToShoot();
            }
        }
    }

    /// <summary>
    /// Alternate selection for a unit being "locked" by another player.
    /// </summary>
    /// <param name="ID">ID to find the unit which was selected.</param>
    public void AlternateSelectUnit(int ID)
    {
        Unit pUnit = FindUnit(ID);

        if (pUnit == selectedUnit)
        {
            // Other player also selected same unit
            DeselectUnit();
            return;
        }
        else
        {
            networkedUnit = pUnit;

            pUnit.BecomeSelected(true);
            return;
        }
    }

    /// <summary>
    /// Searches the Unit List for a unit with the ID.
    /// </summary>
    /// <param name="UnitID">ID of Unit to find.</param>
    /// <returns>The unit with the passed ID.</returns>
    public Unit FindUnit(int UnitID)
    {
        foreach (Unit u in unitList)
        {
            if (u.GetID() == UnitID)
            {
                return u;
            }
        }

        return null;
    }

    /// <summary>
    /// Deselect the selected unit, which will stop displaying it's walkable tiles.
    /// </summary>
    public void DeselectUnit()
    {
        if (canDeselect && selectedUnit && !selectedUnit.IsMoving())
        {
            UnitSelectMessage message;
            message.unitID = selectedUnit.GetID();
            message.selected = '0';

            ChampNetManager.SendUnitSelectMessage(message.unitID, message.selected);

            selectedUnit.DeselectUnit();
            selectedUnit = null;
            GridManagerScript.instance.ClearDisplayedTiles();

            EndShooting();
        }
    }

    /// <summary>
    /// Function for handling a Unit Selection from a networked message
    /// </summary>
    /// <param name="unitID">Id of the unit to update.</param>
    public void AlternateDeselect(int unitID)
    {
        Unit pUnit = FindUnit(unitID);

        if (pUnit != null)
        {
            pUnit.BecomeNothing();

            networkedUnit = null;

            return;
        }

    }

    /// <summary>
    /// Goes through every unit in the list and makes them face the camera
    /// </summary>
	public void MakeUnitsLookAtCamera()
	{
        if (unitList == null)
        {
            return;
        }

        foreach (Unit u in unitList)
        {
            u.LookAtCamera();
        }
	}

    /// <summary>
    /// Removes unit from Unit List so that it can be destroyed
    /// safely.
    /// </summary>
    /// <param name="pUnit">Unit to remove from Unit List.</param>
    public void Free(Unit pUnit)
    {
        unitList.Remove(pUnit);
    }

    /// <summary>
    /// Gets the list of units.
    /// </summary>
    /// <returns>List of units called unitList</returns>
    public List<Unit> GetUnitList()
    {
        return unitList;
    }

    /// <summary>
    /// Resets variables on Unit at the start of a new turn.
    /// </summary>
    public void ResetUnits()
    {
        for (int i = 0; i < unitList.Count; ++i)
        {
            unitList[i].ResetUnit();
        }
    }

    /// <summary>
    /// Abstract function for starting a turn
    /// </summary>
    public abstract void StartTurn();

    /// <summary>
    /// Forces the client to wait until all PathRequests
    /// have been handled to deselect a Unit.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DeflagDeselect()
    {
        while(PathRequestManager.instance.IsRunning())
        {
            yield return null;
        }

        canDeselect = true;
    }

    /// <summary>
    /// Using a Unit's shooting range, determines which enemies
    /// are within range and within vision to be shot. Flags those 
    /// Units.
    /// </summary>
    /// <returns>List of Units which have been flagged as able to be shot.</returns>
    public List<Unit> PrepareToShoot()
    {
        if (selectedUnit != null && !selectedUnit.HasShot() && !selectedUnit.IsMoving() && !isShooting)
        {
            List<Unit> shootableTargets = new List<Unit>();

            //Tried to show the shoot radius, will get back to this later
            shootRadiusObject.transform.localScale = new Vector3(selectedUnit.GetShootRadius() * 2 + 0.5f, selectedUnit.GetShootRadius() * 2 + 0.5f, 1);

            shootRadiusObject.transform.position = selectedUnit.transform.position;

            shootRadiusObject.SetActive(true);

            // Changes the layer of the selected unit to ensure that the raycast does not hit the selected unit
            selectedUnit.gameObject.layer = Physics.IgnoreRaycastLayer;

            isShooting = true;

            Collider[] hits = Physics.OverlapSphere(selectedUnit.transform.position, selectedUnit.GetShootRadius(), unitLayer);

            Vector3 unitOffset = new Vector3(0.0f, 0.2f, 0.0f);

            foreach (Collider col in hits)
            {
                Unit pUnit = col.GetComponent<Unit>();

                // if our list does not have the unit, then it is in the enemy list
                if (!unitList.Contains(pUnit))
                {
                    Vector3 direction = col.transform.position - selectedUnit.transform.position;
                    direction.Normalize();

                    RaycastHit hit;

                    Physics.Raycast(selectedUnit.transform.position + unitOffset, direction, out hit, selectedUnit.GetShootRadius());

                    if (hit.collider != null && hit.collider.gameObject.CompareTag("Unit") && hit.collider == col)
                    {
                        pUnit.BecomeTarget();
                        shootableTargets.Add(pUnit);
                    }
                }
            }

            // Reverts the layer of the selected unit so it can be hit by raycasts again
            selectedUnit.gameObject.layer = 8;

            if (shootableTargets.Count < 1)
            {
                return null;
            }
            else
            {
                return shootableTargets;
            }

        }

        return null;
    }

    /// <summary>
    /// Deals damage to the Unit based on the Selected Unit's damage value.
    /// </summary>
    /// <param name="pUnit">Unit to take damage.</param>
    public void Shoot(Unit pUnit)
    {
        if (isShooting && pUnit.GetShootable())
        {
            pUnit.TakeDamage(selectedUnit.GetDamage());

            selectedUnit.SetShot();

            ChampNetManager.SendDamageMessage(pUnit.GetID(), selectedUnit.GetID(), selectedUnit.GetDamage());
        }

        EndShooting();
    }

    /// <summary>
    /// Deals damage to the unit with corresponding ID.
    /// </summary>
    /// <param name="unitID">ID of unit to take damage.</param>
    /// <param name="damage">Amount of damage to be dealt.</param>
    public void NetworkShoot(int unitID, int damage)
    {
        Unit pUnit = FindUnit(unitID);

        if (pUnit != null)
        {
            pUnit.TakeDamage(damage);
        }    
    }

    /// <summary>
    /// Cleans up Units and resets shooting status of UnitManager.
    /// </summary>
    public void EndShooting()
    {
        if (isShooting)
        {
            isShooting = false;

            List<Unit> units = TurnManager.instance.GetOtherManager().GetUnitList(); // COULD POSSIBLY STORE WHICH UNITS WERE TARGETS, SO AS NOT TO RESET ALL

            foreach (Unit u in units)
            {
                u.BecomeNothing(); // can no longer be shot, is not targeted.
            }

            shootRadiusObject.SetActive(false);

            GameManager.instance.DisableText(); // POSSIBLY IRRELEVENT
        }
    }

    /// <summary>
    /// Get the current Selected Unit.
    /// </summary>
    /// <returns>Selected Unit, if not null.</returns>
    public Unit GetSelectedUnit()
    {
        if (selectedUnit != null)
        {
            return selectedUnit;
        }
        else
        {
            return null;
        }
    }

    public GameObject GetShotPrefab()
    {
        return shotFeedbackPrefab;
    }

    public bool UnitIsMoving()
    {
        if (selectedUnit == null)
        {
            return false;
        }

        return selectedUnit.IsMoving();
    }
}
