////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  TurnManager : class | Written by Anthony Pascone and Parker Staszkiewicz                                      //
//  Handles the logic of turn order and resetting and ending turns.                                               //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour {

    #region SINGLETON
    public static TurnManager instance;
    public GameObject endTurnButton;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    #endregion

    int turn = 0;   //Tracks whose turn it is. 0 = player, 1= AI

    UnitManager currentManager;

    public Image vignette;
    public Color playerColor;
    public Color enemyColor;

    public GameObject blocker;

    private void Start()
    {
        currentManager = PlayerUnitManager.instance;
    }

    /// <summary>
    /// Prepares the local side for ending your turn and sends the other player that you ended your turn
    /// </summary>
    public void PrepareTurnOver()
    {
        if (!currentManager.UnitIsMoving())
        {
            blocker.SetActive(true);

            GameManager.instance.LocalTurnOver();

            ChampNet.ChampNetManager.SendTurnOverMessage();
        }
    }

    /// <summary>
    /// Ends the turn of the current active side. 
    /// </summary>
    public void TurnOver()
    {
        //Make sure everything is done before you can end your turn
        if (!PathRequestManager.instance.IsRunning() && !PlayerUnitManager.instance.CheckIfAUnitIsMoving())
        {
            // End the turn of the current Manager; cleans up all units.
            currentManager.EndTurn();

            if (turn == 0)
            {
                vignette.color = enemyColor;

                endTurnButton.SetActive(false);

                currentManager = EnemyUnitManager.instance;

                turn = 1;
            }
            else
            {
                vignette.color = playerColor;

                endTurnButton.SetActive(true);

                currentManager = PlayerUnitManager.instance;

                blocker.SetActive(false);

                turn = 0;
            }

            // Begin the turn, resetting values of units to have full movement again
            currentManager.StartTurn();
        }
    }

    /// <summary>
    /// Get the unit manager for the team whose turn it is
    /// </summary>
    /// <returns></returns>
    public UnitManager GetCurrentManager()
    {
        return currentManager;
    }

    /// <summary>
    /// Get the unit manager for the team whose turn it currently is not
    /// </summary>
    /// <returns></returns>
    public UnitManager GetOtherManager()
    {
        if (turn == 0)
        {
            return EnemyUnitManager.instance;
        }
        else
        {
            return PlayerUnitManager.instance;
        }
    }
}
