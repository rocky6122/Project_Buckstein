////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  ChampNet : namespace | Written by Parker Staszkiewicz                                                         //
//  The ChampNet namespace ensures that anything defined by our plug-in will not conflict with any Unity          //
//  definitions.                                                                                                  //
//                                                                                                                //
//  CHAMPNET_MESSAGE_ID : enum | Written by John Imgrund                                                          //
//  These message IDs correspond to values within the DLL plug-in and are used for determing what message         //
//  was sent.                                                                                                     //
//                                                                                                                //
//  ChampNetManager : static class | Written by Parker Staszkiewicz and John Imgrund                              //
//  A collection of static extern functions for accessing the DLL plug-in as well as static wrapper functions     //
//  for calling the extern functions within other classes.                                                        //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace ChampNet
{
    public enum CHAMPNET_MESSAGE_ID
    { 
        ID_MESSAGE_DATA = 136,
        ID_UNIT_SELECT_DATA,
        ID_UNIT_MOVE_DATA,
        ID_SEED_DATA,
        ID_UNIT_DAMAGE_DATA,
        ID_START_TURN_DATA,
        ID_FINALIZE_TURN_DATA
    }

    public struct UnitSelectMessage 
    {
        public int unitID;
        public char selected;
    }

    public struct ChatMessage
    {
        public string userName;
        public string message;
    }

    public struct UnitMoveMessage
    {
        public int UnitID;
        public Vector2 gridPos;
    }

    public struct RandomSeedMessage
    {
        public int seed;
    }

    public struct UnitDamageMessage
    {
        public int unitID;
        public int shooterID;
        public int damageDealt;
    }

    public static class ChampNetManager
    {
        #region DLL Imports

        #region Init and Destroy
        [DllImport("ChampNet_x64")]
        static extern void InitializeHostNetworking(string userName);

        [DllImport("ChampNet_x64")]
        static extern void InitializeClientNetworking(string userName, string serverIP);

        [DllImport("ChampNet_x64")]
        static extern void StopNetworking();
        #endregion

        #region Polling
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [DllImport("ChampNet_x64")]
        static extern int ReceiveMessageType();

        [DllImport("ChampNet_x64")]
        static extern int GetCurrentClientNum();

        [DllImport("ChampNet_x64")]
        static extern void PopReceiver();
        #endregion

        #region UnitSelectMessage
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        [DllImport("ChampNet_x64")]
        static extern void AddUnitSelectMessageToSender(int unitID, char selected);

        [DllImport("ChampNet_x64")]
        static extern int UnitSelectMessageID();

        [DllImport("ChampNet_x64")]
        static extern char UnitSelectMessageSelected();
        #endregion

        #region ChatMessage
        [DllImport("ChampNet_x64")]
        static extern void AddChatMessageToSender(string message);

        [DllImport("ChampNet_x64")]
        static extern IntPtr ChatMessageUserName();

        [DllImport("ChampNet_x64")]
        static extern IntPtr ChatMessageMessage();

        #endregion

        #region UnitMoveMessage
        [DllImport("ChampNet_x64")]
        static extern void AddUnitMoveMessageToSender(int unitID, int xPos, int zPos);

        [DllImport("ChampNet_x64")]
        static extern int UnitMoveMessageID();

        [DllImport("ChampNet_x64")]
        static extern int UnitMoveMessageXPos();

        [DllImport("ChampNet_x64")]
        static extern int UnitMoveMessageZPos();
        #endregion

        #region RandomSeedMessage
        [DllImport("ChampNet_x64")]
        static extern void AddRandomSeedMessageToSender(int seed);

        [DllImport("ChampNet_x64")]
        static extern int RandomSeedMessageSeed();
        #endregion

        #region UnitDamageMessage
        [DllImport("ChampNet_x64")]
        static extern void AddUnitDamageMessageToSender(int unitID, int shooterID, int damageDealt);

        [DllImport("ChampNet_x64")]
        static extern int UnitDamageMessageID();

        [DllImport("ChampNet_x64")]
        static extern int UnitDamageMessageShooterID();

        [DllImport("ChampNet_x64")]
        static extern int UnitDamageMessageDamage();
        #endregion

        #region Start and End Turn Message
        [DllImport("ChampNet_x64")]
        static extern void AddStartTurnMessageToSender();

        [DllImport("ChampNet_x64")]
        static extern void AddFinalizeTurnMessageToSender();
        #endregion

        #endregion

        #region Wrapper Functions

        static bool networkIsActive = false;

        public static int GetMessageType()
        {
            return ReceiveMessageType();
        }

        public static void PopMessage()
        {
            PopReceiver();
        }

        public static int GetClientNum()
        {
            return GetCurrentClientNum();
        }

        public static void InitHost(string username)
        {
            InitializeHostNetworking(username);
            networkIsActive = true;
        }

        public static void InitClient(string username, string serverIP)
        {
            InitializeClientNetworking(username, serverIP);
            networkIsActive = true;
        }

        public static void StopNetworkConnection()
        {
            if (networkIsActive)
            {
                StopNetworking();
            }
        }

        public static void SendUnitSelectMessage(int UnitID, char selected)
        {
            AddUnitSelectMessageToSender(UnitID, selected);
        }

        public static UnitSelectMessage GetUnitSelectMessage()
        {
            UnitSelectMessage unitSelect;

            unitSelect.unitID = UnitSelectMessageID();

            unitSelect.selected = UnitSelectMessageSelected();

            PopReceiver();

            return unitSelect;
        }

        public static void SendChatMessage(string msg)
        {
            AddChatMessageToSender(msg);
        }

        public static ChatMessage GetChatMessage()
        {
            ChatMessage chatMsg;

            chatMsg.userName = Marshal.PtrToStringAnsi(ChatMessageUserName());
            chatMsg.message = Marshal.PtrToStringAnsi(ChatMessageMessage());

            PopReceiver();

            return chatMsg;
        }

        public static void SendSeedMessage(int seed)
        {
            AddRandomSeedMessageToSender(seed);
        }

        public static int GetSeedMessage()
        {
            int seed = RandomSeedMessageSeed();

            PopReceiver();

            return seed;
        }

        public static void SendMoveMessage(int id, int posX, int posZ)
        {
            AddUnitMoveMessageToSender(id, posX, posZ);
        }

        public static UnitMoveMessage GetMoveMessage()
        {
            UnitMoveMessage msg;

            msg.UnitID = UnitMoveMessageID();

            msg.gridPos = new Vector2(UnitMoveMessageXPos(), UnitMoveMessageZPos());

            PopReceiver();

            return msg;
        }

        public static void SendDamageMessage(int unitID, int shooterID, int damageDealt)
        {
            AddUnitDamageMessageToSender(unitID, shooterID, damageDealt);
        }

        public static UnitDamageMessage GetDamageMessage()
        {
            UnitDamageMessage message;

            message.unitID = UnitDamageMessageID();
            message.shooterID = UnitDamageMessageShooterID();
            message.damageDealt = UnitDamageMessageDamage();

            PopReceiver();

            return message;
        }

        public static void SendTurnStartMessage()
        {
            AddStartTurnMessageToSender();
        }

        public static void SendTurnOverMessage()
        {
            AddFinalizeTurnMessageToSender();
        }

        #endregion
    }
}
