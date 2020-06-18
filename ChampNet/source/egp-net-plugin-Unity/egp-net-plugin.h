/*
EGP Networking: Plugin Interface/Wrapper
Dan Buckstein
October 2018

Main interface for plugin. Defines symbols to be exposed to whatever
application consumes the plugin. Targeted for Unity but should also
be accessible by custom executables (e.g. test app).
*/

///////////////////////////////////////////////////////////////////////////////
// Heavily Modified by John Imgrund, Anthony Pascone, Parker Staszkiewicz 
///////////////////////////////////////////////////////////////////////////////


#ifndef _EGP_NET_PLUGIN_H_
#define _EGP_NET_PLUGIN_H_

#include "egp-net-plugin-config.h"
#include "NetworkingStructs.h"

#ifdef __cplusplus
extern "C"
{
#endif //__cplusplus
	//START OF C COMPILER LAND
	
	//Init + Destroy
	DLL_SYMBOL
		void InitializeHostNetworking(char* userName);

	DLL_SYMBOL
		void InitializeClientNetworking(char* userName, char* serverIP);

	DLL_SYMBOL
		void StopNetworking();

	//Polling
	DLL_SYMBOL
		int ReceiveMessageType();

	DLL_SYMBOL
		int GetCurrentClientNum();

	//Specific Message Functions

	//ChatMessage Functions
	DLL_SYMBOL
		void AddChatMessageToSender(char* message);

	DLL_SYMBOL
		char* ChatMessageUserName();

	DLL_SYMBOL
		char* ChatMessageMessage();

	//UnitSelectMessage Functions
	DLL_SYMBOL
		void AddUnitSelectMessageToSender(int unitID, char selected);

	DLL_SYMBOL
		int UnitSelectMessageID();

	DLL_SYMBOL
		char UnitSelectMessageSelected();

	//UnitMoveMessage Functions
	DLL_SYMBOL
		void AddUnitMoveMessageToSender(int unitID, int xPos, int zPos);

	DLL_SYMBOL
		int UnitMoveMessageID();

	DLL_SYMBOL
		int UnitMoveMessageXPos();

	DLL_SYMBOL
		int UnitMoveMessageZPos();

	//RandomSeedMessage Functions
	DLL_SYMBOL
		void AddRandomSeedMessageToSender(int seed);

	DLL_SYMBOL
		int RandomSeedMessageSeed();


	//UnitDamageMessage Functions
	DLL_SYMBOL
		void AddUnitDamageMessageToSender(int unitID, int shooterID, int damageDealt);

	DLL_SYMBOL
		int UnitDamageMessageID();

	DLL_SYMBOL
		int UnitDamageMessageShooterID();

	DLL_SYMBOL
		int UnitDamageMessageDamage();

	//StartTurnMessage Function
	DLL_SYMBOL
		void AddStartTurnMessageToSender();

	//FinalizeTurnMessage Function
	DLL_SYMBOL
		void AddFinalizeTurnMessageToSender();

	
	//Message Popper
	DLL_SYMBOL
		void PopReceiver();




	//Testing Purposes
	DLL_SYMBOL
		int foo(int bar);

	DLL_SYMBOL
		char* tester(char* test);


	//END OF C COMPILER LAND
#ifdef __cplusplus
}
#endif //__cplusplus

#endif	// !_EGP_NET_PLUGIN_H_