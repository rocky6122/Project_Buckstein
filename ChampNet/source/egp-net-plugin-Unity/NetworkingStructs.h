////////////////////////////////////////////////////////////////////////////////////
// ChampNet| Written by John Imgrund, Anthony Pascone, Parker Staszkiewicz (c) 2018
////////////////////////////////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////////////////
// Header Description:
//The objective of the NetworkingStructs header file is to store all structs to
//be sent via the networking System. This keeps them decoupled from the 
//networkManager and the NetworkingInstance for clarity, as well as usability.
//
//This file will change based on the needs of the game being used by the plugin
//as the structs sent are almost always specific to the game at hand.
//
//All structs inherit from generic struct 'ChampNetMessage', which contains 
//a unsigned char networkMessageID. This ID, denotes what kind of message the
//struct is and allows for it to be static_cast into its propper form when 
//pulled from the polymorphic queues of ChampNetMessages.
//
//Assuming all ChampNetMessage children will eventually be sent over the network
//all of the structs are pragma packed to eliminate excess data.
//
//
//Structs:
//	ChampNetMessage: unsigned char networkMessageID
//	UserData: char userName[31]
//	ChatMessage: char userName[31], char message[512]
//	UnitSelectMessage: int unitID, char selected
//	UnitMoveMessage: int unitID, int xPos, int zPos
//	randomSeedMessage: int seed
//	UnitDamageMessage: int unitID, int damageDealt
//	StartTurn
//	FinalizeTurn
//
///////////////////////////////////////////////////////////////////////////////


#ifndef _NETWORKING_STRUCTS_H_
#define _NETWORKING_STRUCTS_H_

//Messages to send over the network via a NetworkMessageContainer
#pragma pack(push, 1)

//Generic Struct that messages derive from to be stored in queue. DO NOT ACTUALLY USE THIS DIRECTLY
///<summary>A generic struct with a unsigned char networkMessageID to be inherited by other structs to be sent via the network. This allows them to all be stored in a queue of structs.</summary>
typedef struct ChampNetMessage
{
	unsigned char networkMessageID;
} NetworkMessage;


//ChampNetMessage inherited structs

///<summary>UserData is used to send a clients data to the Host for later reference. Each UserData includes: char userName[31].</summary>
struct UserData : ChampNetMessage
{
	char userName[31];
};

///<summary>ChatMessage is used to send messages between clients. Each ChatMessage includes: char userName[31], char message[512] .</summary>
struct ChatMessage : ChampNetMessage
{
	char userName[31];
	char message[512];
};

///<summary>UnitSelectMessage lock/unlocks the unit at specific unitID based on the bool stored in the message. Each UnitSelectMessage includes: int unitID,  char selected.</summary>
struct UnitSelectMessage : ChampNetMessage
{
	int unitID;
	char selected;
};

///<summary>UnitMoveMessage moves the unit with UnitID to the new (X,Z) position. Each UnitMoveMessage includes: int unitID, int xPos, int zPos.</summary>
struct UnitMoveMessage : ChampNetMessage
{
	int unitID;
	int xPos;
	int zPos;
};

///<summary>PrepareToShootMessage designates a unit that is ready to shoot. Each PrepareToShootMessage includes: int unitID.</summary>
struct RandomSeedMessage : ChampNetMessage
{
	int seed;
};

///<summary>UnitDamageMessage deals damage to unit at unitID based on the damage value inside. Each UnitDamageMessage includes: int unitID, int damageDealt.</summary>
struct UnitDamageMessage : ChampNetMessage
{
	int unitID;
	int shooterID;
	int damageDealt;
};

///<summary>StartTurn indicates to the client that the other player is ready to begin the next turn. StartTurn includes nothing, its just a ping message.</summary>
struct StartTurnMessage : ChampNetMessage {};

///<summary>FinalizeTurn indicates to the client that the other player is ready to end their turn. FinalizeTurn includes nothing, its just a ping message.</summary>
struct FinalizeTurnMessage : ChampNetMessage {};

#pragma pack (pop)


#endif 