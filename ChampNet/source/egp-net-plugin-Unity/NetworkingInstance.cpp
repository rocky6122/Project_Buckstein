////////////////////////////////////////////////////////////////////////////////////
// ChampNet| Written by John Imgrund, Anthony Pascone, Parker Staszkiewicz (c) 2018
////////////////////////////////////////////////////////////////////////////////////

#include "NetworkingInstance.h"
#include "networkManager.h"

void NetworkingInstance::initializeHostInstance(char* userName)
{
	//Create a pointer to the RakPeer instance
	mPeer = RakNet::RakPeerInterface::GetInstance();

	//Hosting
	mIsHost = true;

	//Assigns the userName to the networkingInstance
	strcpy(mUserName, userName);

	//Create the Client List based on max client size
	mClientData = new UserData[M_MAX_CLIENTS];

	//Lets the instance know it wants to enable threading
	mThreadAllowed = 1;

	//Start threading
	egpCreateThread(mNetworkThread, (egpThreadFunc)networkingHostThread, 0);
}

void NetworkingInstance::initializeClientInstance(char* userName, char* serverIP)
{
	//Create a pointer to the RakPeer instance
	mPeer = RakNet::RakPeerInterface::GetInstance();

	//Assigns the userName to the networkingInstance
	strcpy(mUserName, userName);

	//Assigns the IP address to the NetworkingInstance
	strcpy(mServerIP, serverIP);

	//testing
	printf("Host is: %s\n", mServerIP);

	//Lets the instance know it wants to enable threading
	mThreadAllowed = 1;

	//Start threading
	egpCreateThread(mNetworkThread, (egpThreadFunc)networkingClientThread, 0);
}

void NetworkingInstance::addToSender(ChampNetMessage* messageToSend)
{
	//Pushes NetworkMessage onto the stack to be handled in the networkManager
	mMessagesToSend.push(messageToSend);
}

ChampNetMessage* NetworkingInstance::grabFromsender()
{
	//Creates a copy of the message to grab
	ChampNetMessage* messageToSend = mMessagesToSend.front();

	//Deletes the message from the queue
	mMessagesToSend.pop();

	//Returns the NetworkMessage struct, or null if nothing was available
	return messageToSend;
}

void NetworkingInstance::addToReceiver(ChampNetMessage* messageToReceive)
{
	//pushed ChampNetMessage onto the back of the Receiver queue to be handled by the client
	mMessagesReceived.push(messageToReceive);
}

ChampNetMessage* NetworkingInstance::grabFromReciever()
{
	//Creates a copy of the message to grab
	ChampNetMessage* messageReceived = mMessagesReceived.front();

	//Deletes the message from the queue
	mMessagesReceived.pop();

	//Returns the NetworkMessage struct, or null if nothing was available
	return messageReceived;
}

void NetworkingInstance::cleanUpInstance()
{
	//testing
	printf("Preparing to die\n");

	//This should end the thread, then wait for it to finish its last loop
	mThreadAllowed = 0;
	while (mNetworkThread->running);
	memset(mNetworkThread, 0, sizeof(mNetworkThread));

	//Terminates the thread if not already terminated
	egpTerminateThread(mNetworkThread);

	//testing
	printf("Im dead\n");
	
	if (mPeer->IsActive()) //If the networking is active then kill it.
	{
		//Shutdown the peer and close connections
		mPeer->Shutdown(0);

		//Delete RakNet Peer link
		RakNet::RakPeerInterface::DestroyInstance(mPeer);
	}

	//Cleans the member variables
	cleanMembers();
}

int NetworkingInstance::networkingHostThread()
{
	//Get the Networking Instance
	NetworkingInstance& gpInstance = NetworkingInstance::getInstance();

	// 0 is continuous (always ticks)
	egpTimer threadTimer[1] = { 0 };
	egpTimerSet(threadTimer, 0.0);

	// start networking timer
	egpTimerStart(threadTimer);

	//SetUp RakNet Peer
	hostGame();

	// loop
	while (gpInstance.isThreadingAllowed())
	{
		if (egpTimerUpdate(threadTimer))
		{
			hostHandleNetworking();
		}
	}

	// stop timer
	egpTimerStop(threadTimer);

	// done
	return (threadTimer->ticks);
}

int NetworkingInstance::networkingClientThread()
{
	//Get the Networking Instance
	NetworkingInstance& gpInstance = NetworkingInstance::getInstance();

	// 0 is continuous (always ticks)
	egpTimer threadTimer[1] = { 0 };
	egpTimerSet(threadTimer, 0.0);

	// start networing timer
	egpTimerStart(threadTimer);

	//SetUp RakNet Peer
	joinGame();

	// loop
	while (gpInstance.isThreadingAllowed())
	{
		if (egpTimerUpdate(threadTimer))
		{
			clientHandleNetworking();
		}
	}

	// stop timer
	egpTimerStop(threadTimer);

	// done
	return (threadTimer->ticks);
}

void NetworkingInstance::cleanMembers()
{
	//RakNet members
	mPeer = NULL;

	//Host info members
	mCurrentClientNum = NULL;
	delete mClientData;
	mClientData = NULL;
	strcpy(mServerIP, "");

	//Generic info members
	mIsHost = false;
	strcpy(mUserName, "");

	//Empty the Sender queue
	while (!mMessagesToSend.empty())
	{
		//Grab a pointer to the front
		ChampNetMessage* messageToDelete = mMessagesToSend.front();
		
		//Take it off the queue
		mMessagesToSend.pop();

		//Delete it
		delete messageToDelete;
	}

	//Empty the Receiver queue
	while (!mMessagesReceived.empty())
	{
		//Grab a pointer to the front
		ChampNetMessage* messageToDelete = mMessagesReceived.front();

		//Take it off the queue
		mMessagesToSend.pop();

		//Delete it
		delete messageToDelete;
	}
}
