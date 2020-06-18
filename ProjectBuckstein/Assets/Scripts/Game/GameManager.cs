////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  GameManager : class | Written by Parker Staszkiewicz, Anthony Pascone, and John Imgrund                       //
//        
//       
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using ChampNet;

[RequireComponent(typeof(StatDisplay))]
public class GameManager : MonoBehaviour {

    #region SINGLETON
    public static GameManager instance;

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

    private GridManagerScript gridInstance;
    private PlayerUnitManager playerUnitManager;
    private EnemyUnitManager enemyUnitManager;

    private int randomSeed;

    private StatDisplay display;

    public ChatSystem chat;
    public Notifications notifications;

    private bool showingChat;

    private bool localTurnOver;
    private bool networkedTurnOver;
    public GameObject TeammateEndedNotification;

    private bool localStartTurn;
    private bool networkedStartTurn;

    private void Start()
    {
        JSONReadManager.LoadFile("mapFile");
        JSONReadManager.LoadFile("personalityFile");

        gridInstance = GridManagerScript.instance;
        playerUnitManager = PlayerUnitManager.instance;
        enemyUnitManager = EnemyUnitManager.instance;

        gridInstance.Initialize();

        display = GetComponent<StatDisplay>();
        display.Initialize();
        DisableText();

        chat.InitializeChatBox();
        chat.ShowAndHide(false);
        showingChat = false;

        AddMessageToChat("HOST", "Left click controls Units.");
        AddMessageToChat("HOST", "Right click deselects Units.");
        AddMessageToChat("HOST", "Press E to Prepare to Shoot.");
        AddMessageToChat("HOST", "Left click to shoot enemies.");
        AddMessageToChat("HOST", "WASD moves camera.");
        AddMessageToChat("HOST", "Middle Mouse rotates camera.");

        localTurnOver = false;
        networkedTurnOver = false;

        localStartTurn = false;
        networkedStartTurn = false;

        if (PersistentData.GetHosting())
        {
            randomSeed = (int)System.DateTime.Now.Ticks;

            ChampNetManager.SendSeedMessage(randomSeed);

            InitUnitManagers(randomSeed);
        }
    }

    private void Update()
    {
        HandleNetworking();

        if (BothTurnsOver())
        {
            TurnManager.instance.TurnOver();

            localTurnOver = false;
            networkedTurnOver = false;
            TeammateEndedNotification.SetActive( false );
        }
        else if (BothTurnStart())
        {
            TurnManager.instance.TurnOver();

            localStartTurn = false;
            networkedStartTurn = false;
        }

        if (Input.GetKeyDown(KeyCode.T) && !chat.IsTyping())
        {
            if (!showingChat)
            {
                chat.ShowAndHide(true);
                notifications.TurnOff();
                showingChat = true;
            }
            else
            {
                chat.ShowAndHide(false);
                notifications.ResetNotifications();
                showingChat = false;
            }
        }
    }

    private void InitUnitManagers(int seed)
    {
        Random.InitState(seed);

        playerUnitManager.Initialize();
        enemyUnitManager.Initialize();
    }

    public bool IsChatting()
    {
        return chat.IsTyping();
    }

    private void OnApplicationQuit()
    {
        ChampNetManager.StopNetworkConnection();
    }

    private void HandleNetworking()
    {
        int messageType = ChampNetManager.GetMessageType();

        if (messageType != -1)
        {
            switch (messageType)
            {
                case (int)CHAMPNET_MESSAGE_ID.ID_UNIT_SELECT_DATA:
                    {
                        UnitSelectMessage message = ChampNetManager.GetUnitSelectMessage();

                        if (TurnManager.instance.GetCurrentManager() == playerUnitManager)
                        {
                            if (message.selected == '1')
                            {
                                playerUnitManager.AlternateSelectUnit(message.unitID);
                            }
                            else if (message.selected == '0')
                            {
                                playerUnitManager.AlternateDeselect(message.unitID);

                            }
                            else
                            {
                                Debug.LogError("INVALID MESSAGE");
                            }
                        }
                        // Handle an enemy being selected
                        else
                        {

                        }
                    }
                    break;

                case (int)CHAMPNET_MESSAGE_ID.ID_MESSAGE_DATA:
                    {
                        ChatMessage message = ChampNetManager.GetChatMessage();

                        AddMessageToChat(message.userName, message.message);
                    }
                    break;

                case (int)CHAMPNET_MESSAGE_ID.ID_SEED_DATA:
                    {
                        InitUnitManagers(ChampNetManager.GetSeedMessage());
                    }
                    break;

                case (int)CHAMPNET_MESSAGE_ID.ID_UNIT_MOVE_DATA:
                    {
                        UnitMoveMessage message = ChampNetManager.GetMoveMessage();

                        if (TurnManager.instance.GetCurrentManager() != playerUnitManager)
                        {
                            return;
                        }

                        playerUnitManager.PrepareMoveUnit(message.gridPos);
                    }
                    break;

                case (int)CHAMPNET_MESSAGE_ID.ID_UNIT_DAMAGE_DATA:
                    {
                        UnitDamageMessage message = ChampNetManager.GetDamageMessage();

                        if (TurnManager.instance.GetCurrentManager() != playerUnitManager)
                        {
                            return;
                        }

                        enemyUnitManager.NetworkShoot(message.unitID, message.damageDealt);

                        playerUnitManager.FindUnit(message.shooterID).SetShot();
                    }
                    break;

                case (int)CHAMPNET_MESSAGE_ID.ID_FINALIZE_TURN_DATA:
                    {
                        ChampNetManager.PopMessage();

                        NetworkTurnOver();
                    }
                    break;

                case (int)CHAMPNET_MESSAGE_ID.ID_START_TURN_DATA:
                    {
                        ChampNetManager.PopMessage();

                        NetworkTurnStart();
                    }
                    break;
            }
        }
    }

    public void CheckEnd()
    {
        if(playerUnitManager.GetUnitList().Count == 0)
        {
            //Players lose
            //Stop Networking
            ChampNetManager.StopNetworkConnection();
            PersistentData.SetPlayerHasWon(true);
            PersistentData.SetIsEndScene(true);
            SceneManager.LoadScene("EndScene");

        }
        else if (enemyUnitManager.GetUnitList().Count == 0)
        {
            //Enemies lose
            //Stop networking
            ChampNetManager.StopNetworkConnection();
            PersistentData.SetPlayerHasWon(false);
            PersistentData.SetIsEndScene(true);
            SceneManager.LoadScene("EndScene");
        }
    }

    public void ShowText(string unitClass, string unitName, int health, int maxHealth, int moves, bool hasShot, int damage, string bloodType, string like, string dislike, int status)
    {
        display.ShowText(unitClass, unitName, health, maxHealth, moves, hasShot, damage, bloodType, like, dislike, status);
    }

    public void ShowDamageText()
    {
        if (TurnManager.instance.GetCurrentManager() == PlayerUnitManager.instance)
        {
            Unit pUnit = PlayerUnitManager.instance.GetSelectedUnit();

            if (pUnit != null)
            {
                display.ShowDamage(pUnit.GetDamage());   
            }
        }
    }

    public void DisableText()
    {
        display.DisableText();
    }

    private void AddMessageToChat(string username, string message)
    {
        chat.AddMessage(username, message);

        if (!chat.IsActive())
        {
            notifications.AddNotification();
        }
    }

    private void ShowChat(bool show)
    {
        if (show)
        {
            notifications.ResetNotifications();
            notifications.TurnOff();
        }

        chat.ShowAndHide(show);
    }

    private bool BothTurnsOver()
    {
        return localTurnOver && networkedTurnOver;
    }

    private bool BothTurnStart()
    {
        return localStartTurn && networkedStartTurn;
    }

    public void LocalTurnOver()
    {
        localTurnOver = true;
    }

    private void NetworkTurnOver()
    {
        networkedTurnOver = true;
        TeammateEndedNotification.SetActive(true);
    }

    public void LocalTurnStart()
    {
        localStartTurn = true;
    }

    private void NetworkTurnStart()
    {
        networkedStartTurn = true;
    }
}
