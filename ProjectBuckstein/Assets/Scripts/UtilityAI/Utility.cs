////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  Utility : class | Written by Anthony Pascone and Parker Staszkiewicz                                          //
//  
//
//  UtilityAction: struct | Written by Anthony Pascone      
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility {


    //Actions are what the AI can do
    struct UtilityAction
    {
        //All the things you look at to decide which action to take
        float score;

        public void AddToScore(float num)
        {
            score += num;
        }

        public float GetScore()
        {
            return score;
        }

        public void SetScoreToZero()
        {
            score = 0;
        }
    }

    private EnemyUnit thisUnit;         //The EnemyUnit component for this unit
    private bool isActing;
    private List<Unit> shootables;
    private Unit closestUnit;

    UtilityAction moveCloserToEnemy;    //This is scored from 0-100. 0 being within range, and 100 being mega far away
    UtilityAction fleeFromEnemy;        //this is scored by 100 - (0-100), so the farther away they are the lower the score
                                        //Also is scored from 0-100 based on how hurt you are. 0 being fully healed and 100 being dead
    UtilityAction stayWhereYouAre;      //When within the movement action, this gives 50 to stay where you are

    Vector3 closestUnitPos;     //The closest player unit
    float distToClosestUnit;    //Distance between this unit and the closest player unit

    public void SetUnit(EnemyUnit unit)
    {
        thisUnit = unit;
    }

    public bool IsActing()
    {
        return isActing;
    }

    /// <summary>
    /// Figures out which action to do first. It will shoot first if an enemy is in its radius at the start of the turn, but move first if not
    /// </summary>
    public IEnumerator DoActions()
    {
        isActing = true;
        bool stayedPut = false;

        //Get the player unit that is closest to this unity
        closestUnitPos = EnemyUnitManager.instance.GetClosestPlayerUnitPosition(thisUnit, out closestUnit);
        //Get the distance between the this unit and the closest player unity
        distToClosestUnit = Vector3.Distance(thisUnit.transform.position, closestUnitPos);

        // WAIT UNTIL THE PATHFINDERS ARE DONE
        while (PathRequestManager.instance.IsRunning())
        {
            yield return null;
        }

        MoveAction();

        while (thisUnit.IsMoving())
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.25f);

        ShootAction();

        yield return new WaitForSeconds(0.25f);

        //Move somewhere (or stay put) then shoot then move again if possible//
        //while (thisUnit.GetMovesLeft() > 0 && !thisUnit.HasShot())
        {
            // WAIT UNTIL THE PATHFINDERS ARE DONE
            if (thisUnit.GetMovesLeft() > 0)
            {
                while (PathRequestManager.instance.IsRunning())
                {
                    yield return null;
                }

                stayedPut = MoveAction();

                while (thisUnit.IsMoving())
                {
                    yield return null;
                }

                yield return new WaitForSeconds(0.25f);

                ShootAction();

                yield return new WaitForSeconds(0.25f);
            }

            if(!thisUnit.HasShot())
            {
                ShootAction();

                yield return new WaitForSeconds(0.25f);
            }

        }

        isActing = false;
    }


    void ShootAction()
    {
        if (shootables != null && !thisUnit.HasShot())
        {
            UtilityAction[] targetActions = new UtilityAction[shootables.Count];

            for (int i = 0; i < targetActions.Length; ++i)
            {
                targetActions[i] = GetShootScore(i);
            }

            int index = GetHighestActionIndex(targetActions);

            EnemyUnitManager.instance.Shoot(shootables[index]);
        }

        EnemyUnitManager.instance.EndShooting();
    }

    /// <summary>
    /// Gets the scores for this action and decides whether to move forward, backward, or stay put. Based on that it will find a node to move to and move to it.
    /// </summary>
    private bool MoveAction()
    {
        bool stayedPut = false;

        if(thisUnit.GetMovesLeft() > 0) //Check if you still have moves left
        {
            Node moveToNode;
            bool shouldMove;

            //Get the player unit that is closest to this unity
            closestUnitPos = EnemyUnitManager.instance.GetClosestPlayerUnitPosition(thisUnit, out closestUnit);
            //Get the distance between the this unit and the closest player unity
            distToClosestUnit = Vector3.Distance(thisUnit.transform.position, closestUnitPos);

            shootables = EnemyUnitManager.instance.PrepareToShoot();

            //Get all the new scores needed to find out where to move
            GetMovesScores();
            
            //Check what action has the highest score and do that one
            if(moveCloserToEnemy.GetScore() > fleeFromEnemy.GetScore() && moveCloserToEnemy.GetScore() > stayWhereYouAre.GetScore())
            {
                //Move closer to the enemy
                moveToNode = MoveCloser();
                shouldMove = true;
            }
            else if (fleeFromEnemy.GetScore() > moveCloserToEnemy.GetScore() && fleeFromEnemy.GetScore() > stayWhereYouAre.GetScore())
            {
                //Move farther from enemy
                moveToNode = MoveFarther();
                shouldMove = true;
            }
            else
            {
                //Stay where you are
                shouldMove = false;
                moveToNode = null;

                if (thisUnit.HasShot())
                {
                    stayedPut = true;
                }
            }

            //Move to the node that was selected
            if (shouldMove)
            {
                EnemyUnitManager.instance.MoveUnit(moveToNode);
            }

            EnemyUnitManager.instance.EndShooting();

            if (shootables != null)
            {
                shootables.Clear();
            }

            shootables = EnemyUnitManager.instance.PrepareToShoot();
        }

        return stayedPut;
    }

    /// <summary>
    /// Finds the most optimal spot to move to. If it can move a spot where the player 
    /// unit is at the top of its shoot radius, then it will. If not it will just move as far as it can
    /// </summary>
    /// <returns></returns>
    Node MoveCloser()
    {
        TileScript tile = null;

        if (!thisUnit.HasShot())
        {
            // FIND A TILE WITH LINE OF SIGHT TO AN ENEMY
            List<TileScript> tilesUnitCanSee = GridManagerScript.instance.GetTilesUnitCanSee(closestUnit, thisUnit.GetShootRadius() + (thisUnit.GetShootRadius() / 2));

            //if there are tiles the enemy can see (should literally always be true)
            if (tilesUnitCanSee != null)
            {
                // Determine how many of the tiles the enemy can see are within walking range of our movement
                List<TileScript> overlappingTiles = GridManagerScript.instance.GetWalkableTilesInRange(tilesUnitCanSee);

                //foreach (TileScript thisTile in overlappingTiles)
                //{
                //    thisTile.sr.color = Color.blue;
                //}

                if (overlappingTiles != null)
                {
                    tile = GridManagerScript.instance.FindClosestTile(thisUnit.transform.position, overlappingTiles);
                }

            }
        }

        // NORMAL MOVE CLOSER

        if (tile == null)
        {
            //Get the vector between the two units
            Vector3 diff = closestUnitPos - thisUnit.transform.position;
            //Make it a unit vector
            diff.Normalize();
            //Mutiply it by the length of your shoot range to get the vector to the end of your shoot range in the direction you want
            diff *= thisUnit.GetShootRadius();
            //Get the vector from this unit  to the tile that will make the player unit at the tip of your range
            Vector3 rangeToUnit = closestUnitPos - thisUnit.transform.position - diff;
            //RangetoUnit is currently using the unit as 0,0 so we have to make the grid's 0,0 the actual starting point
            rangeToUnit += thisUnit.transform.position;

            //Find the closest tile to that point within your walkable zone
            tile = GridManagerScript.instance.FindClosestTile(rangeToUnit);
        }

        return tile.GetNode();
    }

    /// <summary>
    /// Moves the unit as far as it can away from the closest player unit
    /// </summary>
    /// <returns></returns>
    Node MoveFarther()
    {
        //Get the vector between the two units
        Vector3 diff = closestUnitPos - thisUnit.transform.position;
        //Negate it to get the farthest direction from the player unit
        diff *= -1f;
        //Normalize and multiply by the amount of movement you have left to go as far as possible
        diff.Normalize();
        diff *= thisUnit.GetMovesLeft();

        diff += thisUnit.transform.position;

        TileScript tile = GridManagerScript.instance.FindClosestTile(diff);

        return tile.GetNode();
    }

    /// <summary>
    /// Based on some math, the scores for each action is calculated
    /// </summary>
    private void GetMovesScores()
    {
        //Reset all the scores
        moveCloserToEnemy.SetScoreToZero();
        stayWhereYouAre.SetScoreToZero();
        fleeFromEnemy.SetScoreToZero();

        //More health = lower score. Lower health = higher score
        fleeFromEnemy.AddToScore(70f - 70f * (thisUnit.GetHealth() / thisUnit.GetMaxHealth()));

        if (thisUnit.HasShot())
        {
            fleeFromEnemy.AddToScore(100f);
        }

        //The closer the enemy, the higher the score
        //fleeFromEnemy.AddToScore(100f - distToClosestUnit);

        //Get the vector between the two units
        Vector3 diff = closestUnitPos - thisUnit.transform.position;
        //Make it a unit vector
        diff.Normalize();
        //Mutiply it by the length of your shoot range to get the vector to the end of your shoot range in the direction you want
        diff *= thisUnit.GetShootRadius();
        //Get the distance between that shoot range vector and the position of the unit
        float rangeToEnemyDist = Vector3.Distance(diff, closestUnitPos);
        //The closer the enemy is to the tip of your range, the higher the score
        moveCloserToEnemy.AddToScore(120f - rangeToEnemyDist);

        //Check if an enemy is within range currently
        if (!thisUnit.HasShot() && shootables != null)
        {
            stayWhereYouAre.AddToScore(200f); 
        }
    }

    private UtilityAction GetShootScore(int index)
    {
        UtilityAction action = new UtilityAction();

        action.SetScoreToZero();

        // If thisUnit can kill a target, choose that target
        if (shootables[index].GetHealth() <= thisUnit.GetDamage())
        {
            action.AddToScore(200f);
        }
        else
        {
            action.AddToScore(100f - (100f * (shootables[index].GetHealth() / shootables[index].GetMaxHealth())));
        }

        // Snipers are dangerous, so choose that target
        if (shootables[index].GetUnitType() == UnitType.SNIPER)
        {
            action.AddToScore(50f);
        }
        // Otherwise, Tanks are also dangerous, so choose that target
        else if (shootables[index].GetUnitType() == UnitType.TANK)
        {
            action.AddToScore(25f);
        }
        // Lastly, Scouts are dangerous, so choose that target
        else if (shootables[index].GetUnitType() == UnitType.SCOUT)
        {
            action.AddToScore(10f);
        }

        if (thisUnit.GetUnitType() == UnitType.SNIPER)
        {
            Vector3 distance = shootables[index].transform.position - thisUnit.transform.position;

            if (distance.magnitude <= 4)
            {
                action.AddToScore(70f);
            }
            else if (distance.magnitude >= 8)
            {
                action.AddToScore(50f);
            }
        }

        return action;
    }

    private int GetHighestActionIndex(UtilityAction[] actions)
    {
        int index = -1;
        int highestScore = 0;

        for (int i = 0; i < actions.Length; ++i)
        {
            if (actions[i].GetScore() > highestScore)
            {
                index = i;
            }
        }

        return index;
    }
}
