////////////////////////////////////////////////////////////////////////////////////
// ChampNet| Written by John Imgrund, Anthony Pascone, Parker Staszkiewicz (c) 2018
////////////////////////////////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////////////////
// Header Description:
//The objective of the networkManager header file is to contain the actual networking loop as
//well as any other functions that help deal directly with Networking. It is 
//decoupled from our NetworkingInstance singleton for clarity, and interacts with it
//via the singletons getInstance() return.
//
//
//Structs:
//	ChampNetMessageContainer: const unsigned char TIMESTAMP_ID, Time timeStamp, unsigned char networkMessageID
//
//Functions:
//	hostGame()
//	joinGame()
//	hostHandleNetworking()
//	clientHandleNetworking()
//	hostRead()
//	clientRead()
//	hostWrite()
//	clientWrite()
//
///////////////////////////////////////////////////////////////////////////////

#pragma once


#include "RakNet/RakPeerInterface.h"
#include "RakNet/MessageIdentifiers.h"
#include "RakNet/RakNetTypes.h"
#include "RakNet/BitStream.h"
#include "RakNet/GetTime.h"

//List of gameMessages to tag our games custom messages
enum GameMessages
{
	ID_CUSTOM_MESSAGE_START = ID_USER_PACKET_ENUM,

	//Custom message types that can be sent
	ID_SEND_USER_DATA,
	ID_MESSAGE_DATA,
	ID_UNIT_SELECT_DATA,
	ID_UNIT_MOVE_DATA,
	ID_SEED_DATA,
	ID_UNIT_DAMAGE_DATA,
	ID_START_TURN_DATA,
	ID_FINALIZE_TURN_DATA
};

#pragma pack(push, 1)

//Message container
///<summary>ChampNetMessageContainer is used to hold a timeStamp to track latency, and notify the reader of the incoming networkMessage. const unsigned char TIMESTAMP_ID, Time timeStamp, unsigned char networkMessageID</summary>
struct ChampNetMessageContainer
{
	const unsigned char TIMESTAMP_ID = ID_TIMESTAMP;
	RakNet::Time timeStamp;
	//Denotes the ChampNetMessage following this ChampNetMessageContainer
	unsigned char networkMessageID;
};

#pragma pack (pop)

//Starters
///<summary>Sets up a server based on host instances parameters.</summary>
void hostGame();
///<summary>Connects a server based on the client instances parameters.</summary>
void joinGame();


//Handlers
///<summary>The main networking loop for hosts.</summary>
void hostHandleNetworking();
///<summary>The main networking loop for clients.</summary>
void clientHandleNetworking();


//Readers
///<summary>Handles all incoming packets aimed at the host.</summary>
void hostRead();
///<summary>Handles all incoming packets aimed at the client.</summary>
void clientRead();


//Writers
///<summary>Handles all the outgoing packets from the host.</summary>
void hostWrite();
///<summary>Handles all the outgoing packets from the client .</summary>
void clientWrite();