////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  UnitType : enum | Written by Parker Staszkiewicz                                                               /
//  Enum for assigning stats to Units within base class.                                                          //
//                                                                                                                //
//  Unit : abstract class | Written by Anthony Pascone and Parker Staszkiewicz                                    //
//  Base class for Units in the game.  Each Unit must have a NavAgent and SpriteRenderer.                         //
//  A Unit can be selected, deselected, shot, killed; it can move, shoot, etc.                                    //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using LitJson;

public enum UnitType
{
    INVALID,
    SNIPER,
    SCOUT,
    TANK
}

[RequireComponent(typeof(NavAgent), typeof(SpriteRenderer))]
public abstract class Unit : MonoBehaviour {

    private NavAgent nav;

    private TileScript tile;

    private SpriteRenderer sr;

    protected UnitManager manager;

    public UnitType typeOfUnit;

    public Sprite standardSprite;
    public Sprite selectedSprite;
    public Sprite alternateSelectedSprite;
    public Sprite targetedSprite;

    protected bool canBeShot = false;
    private bool hasShotThisTurn = false;
    private bool isSelected = false;

    private int unitID;

    public TileScript Tile
    {
        get { return tile; }
        set { tile = value; }
    }

    // Variables for heath.  These are set in the derived classes.
    protected int maxHealth;
    private int currentHealth;

    // Defines how far a unit can move each turn
    protected int totalMoveRadius;
    protected int moveRadiusLeft;

    // Defines how far a unit can shoot
    protected int shootRadius;

    protected int damageValue;

    private string unitName;
    private string bloodType;
    private string likes;
    private string dislikes;
    private string unitTypeString;
    private int status;

    public bool endedOnDiagonal = false;

    /// <summary>
    /// Initialize the units variables
    /// </summary>
    virtual public void Initialize(UnitManager parent, int ID)
    {
        nav = GetComponent<NavAgent>();
        sr = GetComponent<SpriteRenderer>();

        unitID = ID;

        manager = parent;

        SetUnitStats();

        SetPersonalityStats();
    }

    #region GETTERS

    /// <summary>
    /// Get the moves left of the unit
    /// </summary>
    /// <returns>int moveRadiusLeft</returns>
    public int GetMovesLeft()
    {
        return moveRadiusLeft;
    }

    /// <summary>
    /// Get the shoot radius for the unit
    /// </summary>
    /// <returns>int shootRadius</returns>
    public int GetShootRadius()
    {
        return shootRadius;
    }

    /// <summary>
    /// Get the current health of the unit.
    /// </summary>
    /// <returns>int currentHealth</returns>
    public int GetHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// Returns the damage value of this Unit.
    /// </summary>
    public int GetDamage()
    {
        return damageValue;
    }

    /// <summary>
    /// Get the max health of the unit.
    /// </summary>
    /// <returns>int maxHealth</returns>
    public int GetMaxHealth()
    {
        return maxHealth;
    }

    /// <summary>
    /// Returns whether this Unit is selected.
    /// </summary>
    public bool GetSelected()
    {
        return isSelected;
    }

    /// <summary>
    /// Returns Unit Type enum.
    /// </summary>
    public UnitType GetUnitType()
    {
        return typeOfUnit;
    }

    /// <summary>
    /// Returns Unit ID.
    /// </summary>
    public int GetID()
    {
        return unitID;
    }

    /// <summary>
    /// Returns if this Unit is currently Shootable.
    /// </summary>
    public bool GetShootable()
    {
        return canBeShot;
    }

    /// <summary>
    /// Returns whether this Unit has taken a shot this turn.
    /// </summary>
    public bool HasShot()
    {
        return hasShotThisTurn;
    }

    #endregion

    /// <summary>
    /// Sets stats for Unit based on UnitType enum.
    /// </summary>
    private void SetUnitStats()
    {
        switch (typeOfUnit)
        {
            case UnitType.INVALID:
                {
                    Debug.LogError("UnitType set to Invalid");
                }
                break;
            case UnitType.SNIPER:
                {
                    maxHealth = 25;
                    totalMoveRadius = 3;
                    shootRadius = 10;
                    damageValue = 15;
                    unitTypeString = "Sniper";
                }
                break;
            case UnitType.SCOUT:
                {
                    maxHealth = 35;
                    totalMoveRadius = 7;
                    shootRadius = 6;
                    damageValue = 8;
                    unitTypeString = "Scout";
                }
                break;
            case UnitType.TANK:
                {
                    maxHealth = 50;
                    totalMoveRadius = 5;
                    shootRadius = 3;
                    damageValue = 12;
                    unitTypeString = "Tank";
                }
                break;
        }

        currentHealth = maxHealth;
        moveRadiusLeft = totalMoveRadius;
    }

    /// <summary>
    /// Uses JsonData object to randomly assign personality
    /// and character traits to Unit.
    /// </summary>
    private void SetPersonalityStats()
    {
        JsonData data = JSONReadManager.GetItemData("personalityFile");

        int random = Random.Range(0, (int)data["namesNum"]);

        unitName = (string)data["names"][random];

        random = Random.Range(0, (int)data["bloodNum"]);

        bloodType = (string)data["bloodType"][random];

        random = Random.Range(0, 2);

        bloodType = random == 0 ? bloodType + "-" : bloodType + "+";

        random = Random.Range(0, (int)data["likesNum"]);

        likes = (string)data["likes"][random];

        int randomDislike = random;

        while (randomDislike == random)
        {
            randomDislike = Random.Range(0, (int)data["likesNum"]);
        }

        dislikes = (string)data["likes"][randomDislike];

        status = Random.Range(0, 2);
    }

    /// <summary>
    /// Reduces the number of moves left this turn and signifies if we ended the movement on a diagonal
    /// </summary>
    /// <param name="reduction">How many moves to reduce</param>
    /// <param name="endOnDiagonal">Whether you ended your movement on the first of a pair of diagonals</param>
    public void ReduceMoves(int reduction, bool endOnDiagonal)
    {
        moveRadiusLeft -= reduction;

        endedOnDiagonal = endOnDiagonal;

        if (moveRadiusLeft > 0)
        {
            if (!PlayerUnitManager.instance.GetUnitList().Contains(this) || !PlayerUnitManager.instance.IsNetworkedUnit(this))
            {
                GridManagerScript.instance.DisplayWalkableTiles(tile, moveRadiusLeft, endedOnDiagonal);
            }
        }
    }

   /// <summary>
   /// Decrease the current health by a certain amount
   /// </summary>
   /// <param name="damage">The amount to decrease health by</param>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        //Create damage animation on the unit getting shot
        GameObject shot = Instantiate(manager.GetShotPrefab(), transform);
        shot.transform.position = transform.position;

        if (currentHealth <= 0)
        {
            currentHealth = 0;

            Die();
        }
    }

   /// <summary>
   /// Destroy the gameObject
   /// </summary>
    private void Die()
    {
        manager.Free(this);

        //Check if every unit on a team is dead
        GameManager.instance.CheckEnd();

        tile.SetOccupy(false);

        Destroy(gameObject);

        GameManager.instance.DisableText();
    }

    /// <summary>
    /// An abstract function that would select a unit
    /// </summary>
    public abstract void SelectUnit();

    /// <summary>
    /// Deselects a unit
    /// </summary>
    public void DeselectUnit()
    {
        BecomeNothing();
    }

    /// <summary>
    /// Make the unit move to a secified node
    /// </summary>
    /// <param name="targetNode">The secific node that the unit should move to</param>
    public void MoveToNode(Node targetNode)
    {
        nav.MoveToNode(targetNode);
    }

    /// <summary>
    /// Alternate function call, when a path has already been found.
    /// </summary>
    /// <param name="path">Node array which the NavAgent will use.</param>
    /// <param name="diagonal">Whether the Unit ended its last Movement on an odd number diagonal.</param>
    public void MoveToNode(Node[] path, bool diagonal)
    {
        nav.MoveToNode(path, diagonal);
    }

    /// <summary>
    /// Get the variable isMoving from the NavAgent
    /// </summary>
    /// <returns></returns>
    public bool IsMoving()
    {
        return nav.isMoving;
    }

    /// <summary>
    /// Resets the turn variables on this Unit.
    /// </summary>
    public void ResetUnit()
    {
        moveRadiusLeft = totalMoveRadius;

        endedOnDiagonal = false;

        hasShotThisTurn = false;
    }

    /// <summary>
    /// Designates this Unit as being able to be shot. Changes Sprite to reflect this.
    /// </summary>
    public void BecomeTarget()
    {
        canBeShot = true;

        sr.sprite = targetedSprite;
    }

    /// <summary>
    /// Flags Unit as selected. Changes sprite to reflect this.
    /// </summary>
    /// <param name="networked">Whether the Unit was selected by a Networked peer.</param>
    public void BecomeSelected(bool networked = false)
    {
        isSelected = true;

        sr.sprite = networked ? alternateSelectedSprite : selectedSprite;
    }

    /// <summary>
    /// Deflags the Unit.
    /// </summary>
    public void BecomeNothing()
    {
        isSelected = false;

        canBeShot = false;

        sr.sprite = standardSprite;
    }

    /// <summary>
    /// Flags this Unit as having taken a shot this turn.
    /// </summary>
    public void SetShot()
    {
        hasShotThisTurn = true;
    }
		
    /// <summary>
    /// Makes every unit always facing the camera
    /// </summary>
	public void LookAtCamera()
	{
		//Get where the unit has to look
		Vector3 lookPos = Camera.main.transform.position - transform.position;
		//This makes sure there will only be rotation around the y axis
		lookPos.y = 0;

		Quaternion rot = Quaternion.LookRotation (lookPos);
		transform.rotation = Quaternion.Slerp (transform.rotation, rot, 1);
	}

    protected virtual void OnMouseOver()
    {
        GameManager.instance.ShowText(unitTypeString, unitName, currentHealth, maxHealth, moveRadiusLeft, hasShotThisTurn, damageValue, bloodType, likes, dislikes, status);
    }

    private void OnMouseExit()
    {
        GameManager.instance.DisableText();
    }
}
