/*
EGP Networking: Plugin Interface/Wrapper
Dan Buckstein
October 2018

Main implementation of Unity plugin wrapper.
*/

///////////////////////////////////////////////////////////////////////////////
// Heavily Modified by John Imgrund, Anthony Pascone, Parker Staszkiewicz
///////////////////////////////////////////////////////////////////////////////

#include "egp-net-plugin.h"
#include "NetworkingInstance.h"

NetworkingInstance& gpInstance = NetworkingInstance::getInstance();

///<summary>Intializes the Networking Instance for the host running a game and starts the thread it will run on.</summary>
void InitializeHostNetworking(char* userName)
{
	//Credit: Dan Buckstein Animal3D Console Allocation
	HANDLE handle = GetConsoleWindow();
	if (!handle)
	{
		int status = AllocConsole();
		freopen("CONOUT$", "w", stdout);
	}

	//testing
	printf("UserName: %s\n", userName);
	gpInstance.initializeHostInstance(userName);
}

///<summary>Intializes the Networking Instance for the client and starts the thread it will run on.</summary>
void InitializeClientNetworking(char* userName, char* serverIP)
{
	//Credit: Dan Buckstein Animal3D Console Allocation
	HANDLE handle = GetConsoleWindow();
	if (!handle)
	{
		int status = AllocConsole();
		freopen("CONOUT$", "w", stdout);
	}

	//testing
	printf("UserName: %s\n", userName);
	printf("ServerIP: %s\n", serverIP);
	gpInstance.initializeClientInstance(userName, serverIP);
}

///<summary>Ends the Networking instance and closes the thread it ran on.</summary>
void StopNetworking()
{
	gpInstance.cleanUpInstance();
}

///<summary>Polls the Receiver queue to see if it has a message to be handled. If it does, it returns the messageType that needs to be handled, else -1.</summary>
int ReceiveMessageType()
{
	int messageType;

	//Makes sure the queue contained a value to avoid error checking a null queue front
	if (gpInstance.getReceiverQueue().size() > 0)
	{
		ChampNetMessage* messageToCheck = gpInstance.getReceiverQueue().front();

		//Sets messageType to correct messageID
		messageType = messageToCheck->networkMessageID;
	}
	else //No messages to be handled
	{
		messageType = -1;
	}

	return messageType;
}

///<summary>Returns the current number of clients connected to the host.</summary>
int GetCurrentClientNum()
{
	return gpInstance.getCurrentClientNum();
}

///<summary>Takes in the values needed for a ChatMessage and constructs it plugin side.</summary>
void AddChatMessageToSender(char * message)
{
	//Create new ChatMessage
	ChatMessage* newMessage = new ChatMessage();

	//Assign Values
	gpInstance.getUserName(newMessage->userName);
	strcpy(newMessage->message, message);

	newMessage->networkMessageID = 136; //ID_MESSAGE_DATA

										//Pushes to sender to be handled later
	gpInstance.addToSender(newMessage);
}

///<summary>Receives the char* userName from a ChatMessage on front of the queue, use ReceiveMessageType() to check.</summary>
char * ChatMessageUserName()
{
	//Create a pointer to the message then static_cast it as a ChatMessage
	ChampNetMessage* messageToCheck = gpInstance.getReceiverQueue().front();
	ChatMessage* chat = static_cast<ChatMessage*>(messageToCheck);

	return chat->userName;
}

///<summary>Receives the char* message from a ChatMessage on front of the queue, use ReceiveMessageType() to check.</summary>
char * ChatMessageMessage()
{
	//Create a pointer to the message then static_cast it as a ChatMessage
	ChampNetMessage* messageToCheck = gpInstance.getReceiverQueue().front();
	ChatMessage* chat = static_cast<ChatMessage*>(messageToCheck);

	return chat->message;
}

///<summary>Takes in values needed for a UnitSelectMessage and constructs it plugin side.</summary>
void AddUnitSelectMessageToSender(int unitID, char selected)
{
	//Creates new UnitSelectMessage
	UnitSelectMessage* newMessage = new UnitSelectMessage();

	//Assign Values
	newMessage->unitID = unitID;
	newMessage->selected = selected;

	newMessage->networkMessageID = 137; //ID_UNIT_SELECT_DATA

	//Pushes to sender to be handled later
	gpInstance.addToSender(newMessage);
}

///<summary>Receives the int UnitID from a UnitSelectMessage on front of queue, use ReceiveMessageType() to check.</summary>
int UnitSelectMessageID()
{
	//Create a pointer to the message then static_cast is as a UnitSelectMessage
	ChampNetMessage* messageToCheck = gpInstance.getReceiverQueue().front();
	UnitSelectMessage* unitSelect = static_cast<UnitSelectMessage*>(messageToCheck);

	return unitSelect->unitID;
}

///<summary>Receives the selected char from a UnitSelectMessage on front of queue, use ReceiveMessageType() to check.</summary>
char UnitSelectMessageSelected()
{
	//Create a pointer to the message then static_cast it as a UnitSelectMessage
	ChampNetMessage* messageToCheck = gpInstance.getReceiverQueue().front();
	UnitSelectMessage* unitSelect = static_cast<UnitSelectMessage*>(messageToCheck);

	return unitSelect->selected;
}

///<summary>Takes in values needed for a UnitMoveMessage and constructs it plugin side.</summary>
void AddUnitMoveMessageToSender(int unitID, int xPos, int zPos)
{
	//Creates new UnitMoveMessage
	UnitMoveMessage* newMessage = new UnitMoveMessage();

	//Assign Values
	newMessage->unitID = unitID;
	newMessage->xPos = xPos;
	newMessage->zPos = zPos;

	newMessage->networkMessageID = 138; //ID_UNIT_MOVE_DATA

	//pushes to sender to be handled later
	gpInstance.addToSender(newMessage);
}

///<summary>Receives the unitID int from a UnitMoveMessage on front of queue, use ReceiveMessageType() to check.</summary>
int UnitMoveMessageID()
{
	//Create a pointer to the message then static_cast it as a UnitMoveMessage
	ChampNetMessage* messageToCheck = gpInstance.getReceiverQueue().front();
	UnitMoveMessage* unitMove = static_cast<UnitMoveMessage*>(messageToCheck);

	return unitMove->unitID;
}

///<summary>Receives the xPos int from a UnitMoveMessage on front of queue, use ReceiveMessageType() to check.</summary>
int UnitMoveMessageXPos()
{
	//Create a pointer to the message then static_cast it as a UnitMoveMessage
	ChampNetMessage* messageToCheck = gpInstance.getReceiverQueue().front();
	UnitMoveMessage* unitMove = static_cast<UnitMoveMessage*>(messageToCheck);

	return unitMove->xPos;
}

///<summary>Receives the zPos int from a UnitMoveMessage on front of queue, use ReceiveMessageType() to check.</summary>
int UnitMoveMessageZPos()
{
	//Create a pointer to the message then static_cast it as a UnitMoveMessage
	ChampNetMessage* messageToCheck = gpInstance.getReceiverQueue().front();
	UnitMoveMessage* unitMove = static_cast<UnitMoveMessage*>(messageToCheck);
	
	return unitMove->zPos;
}

///<summary>Takes in the values needed for a RandomSeedMessage and constructs it plugin side.</summary>
void AddRandomSeedMessageToSender(int seed)
{
	//Creates new RandomSeedMessage
	RandomSeedMessage* newMessage = new RandomSeedMessage();

	//Assign Values
	newMessage->seed = seed;

	newMessage->networkMessageID = 139; //ID_SEED_DATA

	//pushes to sender to be handled later
	gpInstance.addToSender(newMessage);
}

///<summary>Receives the seed int from a RandomSeedMessage on front of queue, use ReceiveMessageType() to check.</summary>
int RandomSeedMessageSeed()
{
	//Create a pointer to the message then static_cast it as a RandomSeedMessage
	ChampNetMessage* messageToCheck = gpInstance.getReceiverQueue().front();
	RandomSeedMessage* randomSeed = static_cast<RandomSeedMessage*>(messageToCheck);

	return randomSeed->seed;
}

///<summary>Takes in the values needed for a UnitDamageMessage and constructs it plugin side.</summary>
void AddUnitDamageMessageToSender(int unitID, int ShooterID, int damageDealt)
{
	//Creates new UnitDamageMessage
	UnitDamageMessage* newMessage = new UnitDamageMessage();

	//Assign Values
	newMessage->unitID = unitID;
	newMessage->shooterID = ShooterID;
	newMessage->damageDealt = damageDealt;

	newMessage->networkMessageID = 140; //ID_UNIT_DAMAGE_DATA

	//pushes to sender to be handled later
	gpInstance.addToSender(newMessage);
}

///<summary>Receives the unitID int from a UnitDamageMessage on front of queue, use ReceiveMessageType() to check.</summary>
int UnitDamageMessageID()
{
	//Create a pointer to the message then static_cast it as a UnitDamageMessage
	ChampNetMessage* messageToCheck = gpInstance.getReceiverQueue().front();
	UnitDamageMessage* unitDamage = static_cast<UnitDamageMessage*>(messageToCheck);


	return unitDamage->unitID;
}

int UnitDamageMessageShooterID()
{
	//Create a pointer to the message then static_cast it as a UnitDamageMessage
	ChampNetMessage* messageToCheck = gpInstance.getReceiverQueue().front();
	UnitDamageMessage* unitDamage = static_cast<UnitDamageMessage*>(messageToCheck);

	
	return unitDamage->shooterID;
}

///<summary>Receives the damageDealt int from a UnitDamageMessage on front of queue, use ReceiveMessageType() to check.</summary>
int UnitDamageMessageDamage()
{
	//Create a pointer to the message then static_cast it as a UnitDamageMessage
	ChampNetMessage* messageToCheck = gpInstance.getReceiverQueue().front();
	UnitDamageMessage* unitDamage = static_cast<UnitDamageMessage*>(messageToCheck);

	return unitDamage->damageDealt;
}

///<summary>Takes in the values needed for a StartTurnMessage and constructs it plugin side.</summary>
void AddStartTurnMessageToSender()
{
	//Creates new UnitDamageMessage
	StartTurnMessage* newMessage = new StartTurnMessage();

	newMessage->networkMessageID = 141; //ID_START_TURN_DATA

	//pushes to sender to be handled later
	gpInstance.addToSender(newMessage);
}

///<summary>Takes in the values needed for a FinalizzeTurnMessage and constructs it plugin side.</summary>
void AddFinalizeTurnMessageToSender()
{
	//Creates new UnitDamageMessage
	FinalizeTurnMessage* newMessage = new FinalizeTurnMessage();

	newMessage->networkMessageID = 142; //ID_FINALIZE_TURN_DATA

	//pushes to sender to be handled later
	gpInstance.addToSender(newMessage);
}


///<summary>Pops the front of the receiver queue and deletes it.</summary>
void PopReceiver()
{
	ChampNetMessage* messageToCheck = gpInstance.getReceiverQueue().front();

	gpInstance.getReceiverQueue().pop();

	delete messageToCheck;
}

// dummy function for testing
int foo(int bar)
{
	return (bar + 1);
}

//testing
char* tester(char* test)
{
	return test;
}
