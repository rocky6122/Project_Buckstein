////////////////////////////////////////////////////////////////////////////////////
// ChampNet| Written by John Imgrund, Anthony Pascone, Parker Staszkiewicz (c) 2018
////////////////////////////////////////////////////////////////////////////////////

#include "networkManager.h"
#include "NetworkingInstance.h"

//Starters

void hostGame()
{
	//YOU ARE A SERVER

	//Get the Networking Instance
	NetworkingInstance& gpInstance = NetworkingInstance::getInstance();

	//The socket descriptor allocates the socket to be used while the game is running
	RakNet::SocketDescriptor selectedSocket(gpInstance.getServerPort(), 0);
	gpInstance.getPeer()->Startup(gpInstance.getMaxClients(), &selectedSocket, 1);

	//Let the server accept incoming connections from the clients
	printf("Starting the game.\n");
	gpInstance.getPeer()->SetMaximumIncomingConnections(gpInstance.getMaxClients());
}

void joinGame()
{
	//YOU ARE A CLIENT

	// temporary buffer to use for the IP address
	const unsigned int bufferSz = 32;
	char ipAddress[bufferSz];

	//Get the Networking Instance
	NetworkingInstance& gpInstance = NetworkingInstance::getInstance();

	//Grabs a free socket to use
	RakNet::SocketDescriptor selectedSocket;
	gpInstance.getPeer()->Startup(1, &selectedSocket, 1);

	//Copies in the IP address to connect too
	strcpy(ipAddress, gpInstance.getServerIP());

	//If no IP address was entered, assume its a local server
	if (ipAddress[0] == '\n' || ipAddress[0] == NULL)
	{
		strcpy(ipAddress, "127.0.0.1");
	}

	//Connect to server
	printf("Joining the game.\n");
	printf("IP Address is: %s\n", ipAddress);
	gpInstance.getPeer()->Connect(ipAddress, gpInstance.getServerPort(), 0, 0);
}


//Handlers

void hostHandleNetworking()
{
	//Runs to read for IDs related hosting a game
	hostRead();
	//Runs to write host messages to send to clients relating to a game
	hostWrite();
}

void clientHandleNetworking()
{
	//Runs to read for IDs related to being a client in a game
	clientRead();
	//Runs to write client messages to send to the host relating to a game
	clientWrite();
}


//Readers

void hostRead()
{
	//Get the NetworkingInstance
	NetworkingInstance& gpInstance = NetworkingInstance::getInstance();

	//Create temp packet to handle incoming
	RakNet::Packet *packet;

	for (
		packet = gpInstance.getPeer()->Receive();
		packet;
		gpInstance.getPeer()->DeallocatePacket(packet), packet = gpInstance.getPeer()->Receive()
		)
	{
		switch (packet->data[0])
		{
		case ID_NEW_INCOMING_CONNECTION:
		{
			printf("A new client is connecting.\n");
		}
		break; //End of ID_NEW_INCOMING_CONNECTION

		case ID_DISCONNECTION_NOTIFICATION:
		{
			printf("Server Disconnected.\n");
		}
		break; //End of ID_DISCONNECTION_NOTIFICATION

		case ID_CONNECTION_LOST:
		{
			printf("Server lost connection lost.\n");
		}
		break; //End of ID_CONNECTION_LOST

		case ID_TIMESTAMP:
		{
			printf("TimeStamp received\n");

			//Create incoming BitStream to read
			RakNet::BitStream incomingStream(packet->data, packet->length, false);

			//Ignore the MessageID
			incomingStream.IgnoreBytes(sizeof(RakNet::MessageID));

			//Create a temporary incomingMessage on the stack. Will auto delete when we leave the instance
			ChampNetMessageContainer incomingMessage[1];

			//Reads the message in from the incomingStream
			incomingStream.Read(incomingMessage);

			//Grab the timeStamp out of the ChampNetMessageContainer
			RakNet::Time sentTime = incomingMessage->timeStamp;
		
			//Grab the MessageID to be switched
			unsigned char networkMessageID = incomingMessage->networkMessageID;

			switch (networkMessageID + 1) //Switch between all user created structs sent through the network via a NetworkMessageContainer
			{
	
			case ID_SEND_USER_DATA:
			{
				printf("UserData received\n");

				if (gpInstance.getCurrentClientNum() == 0) //Host (Sets host data into the array on first run through)
				{
					//Creates a new userData to be filled with Host info
					UserData hostData;

					//Filling with the various host info fields
					gpInstance.getUserName(hostData.userName); //char[] userName

					//Place the userdata into the list of client data then update the total clientNum
					UserData* tempClientData = gpInstance.getClientData();
					
					//Place the hostData onto the ClientData
					tempClientData[gpInstance.getCurrentClientNum()] = hostData;
					gpInstance.addToClientNum(1);
				}

				//Reads the bitstream in as the correct type of NetworkMessage
				UserData userData[1];
				incomingStream.Read(userData);

				//Place the userData onto the list
				gpInstance.getClientData()[gpInstance.getCurrentClientNum()] = *userData;
				gpInstance.addToClientNum(1);

				printf("%s has joined the game.\n", userData->userName);
			}
			break; //End of ID_SEND_USER_DATA

			case ID_MESSAGE_DATA:
			{
				printf("ChatMessage Received.\n");

				//Reads the bitstream in as the correct type of NetworkMessage
				ChatMessage* message = new ChatMessage();
				incomingStream.Read(*message); //Must be *message to pass by reference

				//Add the networkMessageID back in case it was lost in transit
				message->networkMessageID = ID_MESSAGE_DATA;

				printf("UserName: %s\n", message->userName);
				printf("Message: %s\n", message->message);

				//Push it back onto the MessageReceiver queue to be handled later in Unity
				gpInstance.addToReceiver(message);
			}
			break; //End of ID_MESSAGE_DATA

			case ID_UNIT_SELECT_DATA:
			{
				printf("UnitSelectMessage Received.\n");

				//Reads the bitstream in as the correct type of NetworkMessage
				UnitSelectMessage* message = new UnitSelectMessage();
				incomingStream.Read(*message); //Must be *message to pass by reference

				//Add the networkMessageID back in case it was lost in transit
				message->networkMessageID = ID_UNIT_SELECT_DATA;

				printf("UnitID: %i\n", message->unitID);
				printf("Selected: %c\n", message->selected);

				//Push it back onto the MessageReceiver queue to be handled later in Unity
				gpInstance.addToReceiver(message);
			}
			break;

			case ID_UNIT_MOVE_DATA:
			{
				printf("UnitMoveMessage Received.\n");
				
				//Reads the bitstream in as the corret type of NetworkMessage
				UnitMoveMessage* message = new UnitMoveMessage();
				incomingStream.Read(*message);

				//Add the NetworkMessageID back in case it was lost in transit
				message->networkMessageID = ID_UNIT_MOVE_DATA;

				printf("UnitID: %i\n", message->unitID);
				printf("xPos: %i\n", message->xPos);
				printf("zPos, %i\n", message->zPos);

				//Push it back onto the MessageReceiver queue to be handled later in Unity
				gpInstance.addToReceiver(message);
			}
			break;

			case ID_SEED_DATA:
			{
				printf("RandomSeedMessage Received.\n");

				//Reads the bitstream in as the corret type of NetworkMessage
				RandomSeedMessage* message = new RandomSeedMessage();
				incomingStream.Read(*message);

				//Add the NetworkMessageID back in case it was lost in transit
				message->networkMessageID = ID_SEED_DATA;

				printf("Seed: %i\n", message->seed);

				//Push it back onto the MessageReceiver queue to be handled later in Unity
				gpInstance.addToReceiver(message);
			}
			break;

			case ID_UNIT_DAMAGE_DATA:
			{
				printf("UnitDamageMessage Received.\n");

				//Reads the bitstream in as the corret type of NetworkMessage
				UnitDamageMessage* message = new UnitDamageMessage();
				incomingStream.Read(*message);

				//Add the NetworkMessageID back in case it was lost in transit
				message->networkMessageID = ID_UNIT_DAMAGE_DATA;

				printf("UnitID: %i\n", message->unitID);
				printf("damageDealt: %i\n", message->damageDealt);

				//Push it back onto the MessageReceiver queue to be handled later in Unity
				gpInstance.addToReceiver(message);
			}
			break;

			case ID_START_TURN_DATA:
			{
				printf("StartTurnMessage Received.\n");

				//Reads the bitstream in as the corret type of NetworkMessage
				StartTurnMessage* message = new StartTurnMessage();
				incomingStream.Read(*message);

				//Add the NetworkMessageID back in case it was lost in transit
				message->networkMessageID = ID_START_TURN_DATA;

				//Push it back onto the MessageReceiver queue to be handled later in Unity
				gpInstance.addToReceiver(message);
			}
			break;

			case ID_FINALIZE_TURN_DATA:
			{
				printf("FinalizeTurnMessage Received.\n");

				//Reads the bitstream in as the corret type of NetworkMessage
				FinalizeTurnMessage* message = new FinalizeTurnMessage();
				incomingStream.Read(*message);

				//Add the NetworkMessageID back in case it was lost in transit
				message->networkMessageID = ID_FINALIZE_TURN_DATA;

				//Push it back onto the MessageReceiver queue to be handled later in Unity
				gpInstance.addToReceiver(message);
			}
			break;

			default:
				printf("Message with identifier %i has arrived.\n", networkMessageID);
				break;
			}
		}
		break; //End of ID_TIMESTAMP
		
		default:
			printf("Message with identifier %i has arrived.\n", packet->data[0]);
			break;
		}
	}
}

void clientRead()
{
	NetworkingInstance& gpInstance = NetworkingInstance::getInstance();

	RakNet::Packet *packet;

	for (
		packet = gpInstance.getPeer()->Receive();
		packet;
		gpInstance.getPeer()->DeallocatePacket(packet), packet = gpInstance.getPeer()->Receive()
		)
	{
		switch (packet->data[0])
		{
		case ID_NO_FREE_INCOMING_CONNECTIONS:
		{
			printf("The server is full.\n");
		}
		break; //End of ID_NO_FREE_INCOMING_CONNECTIONS

		case ID_DISCONNECTION_NOTIFICATION:
		{
			printf("Client Disconnected.\n");
		}
		break; //End of ID_DISCONNECTION_NOTIFICATION

		case ID_CONNECTION_LOST:
		{
			printf("Client lost connection lost.\n");
		}
		break; //End of ID_CONNECTION_LOST

		case ID_CONNECTION_REQUEST_ACCEPTED:
		{
			printf("Client: Connection has been accepted by the server.\n");
			//Gathers user data to send to the server for reference.
			UserData* data = new UserData();
			gpInstance.getUserName(data->userName); //char[] userName

			//Set the NetworkMessageID
			data->networkMessageID = ID_SEND_USER_DATA;

			//Place the info back into the sender to be sent by the Write() ASAP
			gpInstance.addToSender(data);
		}
		break; //End of ID_CONNECTION_REQUEST_ACCEPTED

		case ID_TIMESTAMP:
		{
			printf("TimeStamp received\n");

			//Create incoming BitStream to read
			RakNet::BitStream incomingStream(packet->data, packet->length, false);

			//Ignore the MessageID
			incomingStream.IgnoreBytes(sizeof(RakNet::MessageID));

			//Create a temporary incomingMessage on the stack. Will auto delete when we leave the instance
			ChampNetMessageContainer incomingMessage[1];

			//Reads the message in from the incomingStream
			incomingStream.Read(incomingMessage);

			//Grab the timeStamp out of the ChampNetMessageContainer
			RakNet::Time sentTime = incomingMessage->timeStamp;

			//Grab the MessageID to be switched
			unsigned char networkMessageID = incomingMessage->networkMessageID;

			switch (networkMessageID) //Switch between all user created structs sent through the network via a NetworkMessageContainer
			{
			case ID_MESSAGE_DATA:
			{
				printf("ChatMessage Received.\n");
				//Reads the bitstream in as the correct type of NetworkMessage
				ChatMessage* message = new ChatMessage(); 
				incomingStream.Read(*message); //Must be *message to pass by reference

				//Add the networkMessageID back in case it was lost in transit
				message->networkMessageID = ID_MESSAGE_DATA;

				printf("UserName: %s\n", message->userName);
				printf("Message: %s\n", message->message);

				//Push it back onto the MessageReceiver queue to be handled later in Unity
				gpInstance.addToReceiver(message);
			}
			break; //End of ID_MESSAGE_DATA

			case ID_UNIT_SELECT_DATA:
			{
				printf("ID Unit Select Message\n");

				//Reads the bitstream in as the correct type of NetworkMessage
				UnitSelectMessage* message = new UnitSelectMessage();
				incomingStream.Read(*message); //Must be *message to pass by reference

				//Add the networkMessageID back in case it was lost in transit
				message->networkMessageID = ID_UNIT_SELECT_DATA;

				printf("UnitID: %i\n", message->unitID);
				printf("Selected: %c\n", message->selected);

				//Push it back onto the MessageReceiver queue to be handled later in Unity
				gpInstance.addToReceiver(message);
			}
			break;

			case ID_UNIT_MOVE_DATA:
			{
				printf("UnitMoveMessage Received.\n");

				//Reads the bitstream in as the corret type of NetworkMessage
				UnitMoveMessage* message = new UnitMoveMessage();
				incomingStream.Read(*message);

				//Add the NetworkMessageID back in case it was lost in transit
				message->networkMessageID = ID_UNIT_MOVE_DATA;

				printf("UnitID: %i\n", message->unitID);
				printf("xPos: %i\n", message->xPos);
				printf("zPos, %i\n", message->zPos);

				//Push it back onto the MessageReceiver queue to be handled later in Unity
				gpInstance.addToReceiver(message);
			}
			break;

			case ID_SEED_DATA:
			{
				printf("RandomSeedMessage Received.\n");

				//Reads the bitstream in as the corret type of NetworkMessage
				RandomSeedMessage* message = new RandomSeedMessage();
				incomingStream.Read(*message);

				//Add the NetworkMessageID back in case it was lost in transit
				message->networkMessageID = ID_SEED_DATA;

				printf("Seed: %i\n", message->seed);

				//Push it back onto the MessageReceiver queue to be handled later in Unity
				gpInstance.addToReceiver(message);
			}
			break;

			case ID_UNIT_DAMAGE_DATA:
			{
				printf("UnitDamageMessage Received.\n");

				//Reads the bitstream in as the corret type of NetworkMessage
				UnitDamageMessage* message = new UnitDamageMessage();
				incomingStream.Read(*message);

				//Add the NetworkMessageID back in case it was lost in transit
				message->networkMessageID = ID_UNIT_DAMAGE_DATA;

				printf("UnitID: %i\n", message->unitID);
				printf("ShooterID: %i\n", message->shooterID);
				printf("damageDealt: %i\n", message->damageDealt);

				//Push it back onto the MessageReceiver queue to be handled later in Unity
				gpInstance.addToReceiver(message);
			}
			break;

			case ID_START_TURN_DATA:
			{
				printf("StartTurnMessage Received.\n");

				//Reads the bitstream in as the corret type of NetworkMessage
				StartTurnMessage* message = new StartTurnMessage();
				incomingStream.Read(*message);

				//Add the NetworkMessageID back in case it was lost in transit
				message->networkMessageID = ID_START_TURN_DATA;

				//Push it back onto the MessageReceiver queue to be handled later in Unity
				gpInstance.addToReceiver(message);
			}
			break;

			case ID_FINALIZE_TURN_DATA:
			{
				printf("FinalizeTurnMessage Received.\n");

				//Reads the bitstream in as the corret type of NetworkMessage
				FinalizeTurnMessage* message = new FinalizeTurnMessage();
				incomingStream.Read(*message);

				//Add the NetworkMessageID back in case it was lost in transit
				message->networkMessageID = ID_FINALIZE_TURN_DATA;

				//Push it back onto the MessageReceiver queue to be handled later in Unity
				gpInstance.addToReceiver(message);
			}
			break;

			default:
				printf("Message with identifier %i has arrived.\n", networkMessageID);
				break;
			}
		}
		break; //End of ID_TIMESTAMP

		default:
			printf("Message with identifier %i has arrived.\n", packet->data[0]);
			break;
		}
	}
}


//Writers

void hostWrite()
{
	NetworkingInstance& gpInstance = NetworkingInstance::getInstance();
	RakNet::BitStream outgoingStream;

	while (!gpInstance.getSenderQueue().empty())
	{
		//peep the front
		ChampNetMessage* messageToSend = gpInstance.getSenderQueue().front();

		//Pop the front off the queue
		gpInstance.getSenderQueue().pop();

		//Create the message container
		ChampNetMessageContainer messageContainer[1];

		//Get the time
		messageContainer->timeStamp = RakNet::GetTime();
		
		//Set the MessageID in the container
		messageContainer->networkMessageID = messageToSend->networkMessageID;

		//Write the openingTimestamp ID in
		outgoingStream.Write(messageContainer->TIMESTAMP_ID);

		//Write the message container in
		outgoingStream.Write(messageContainer);

		//CANT TELL WHETHER OR NOT U HAVE TO CAST THE MESSAGE TO ITS CORRECT TYPE BEFORE U SEND TI
		switch (messageContainer->networkMessageID)
		{
		case ID_SEND_USER_DATA:
		{
			printf("Sending User Data!\n");
			UserData* finalMessageToSend = static_cast<UserData*>(messageToSend);
			outgoingStream.Write(*finalMessageToSend); //Dereference it for sending
		}
		break;
		case ID_MESSAGE_DATA:
		{
			printf("Sending Chat Message!\n");
			ChatMessage* finalMessageToSend = static_cast<ChatMessage*>(messageToSend);
			outgoingStream.Write(*finalMessageToSend); //Dereference it for sending
		}
		break;
		case ID_UNIT_SELECT_DATA:
		{
			printf("Sending Unit Select!\n");
			UnitSelectMessage* finalMessageToSend = static_cast<UnitSelectMessage*>(messageToSend);
			outgoingStream.Write(*finalMessageToSend); //Dereference it for sending
		}
		break;
		case ID_UNIT_MOVE_DATA:
		{
			printf("Sending Unit Move!\n");
			UnitMoveMessage* finalMessageToSend = static_cast<UnitMoveMessage*>(messageToSend);
			outgoingStream.Write(*finalMessageToSend); //Derefernce if for sending
		}
		break;
		case ID_SEED_DATA:
		{
			printf("Sending seed!\n");
			RandomSeedMessage* finalMessageToSend = static_cast<RandomSeedMessage*>(messageToSend);
			outgoingStream.Write(*finalMessageToSend); //Dereference it for sending
		}
		break;
		case ID_UNIT_DAMAGE_DATA:
		{
			printf("Sending Unit Damage!\n");
			UnitDamageMessage* finalMessageToSend = static_cast<UnitDamageMessage*>(messageToSend);
			outgoingStream.Write(*finalMessageToSend); //Dereference it for sending
		}
		break;
		case ID_START_TURN_DATA:
		{
			printf("Sending Start Turn!\n");
			StartTurnMessage* finalMessageToSend = static_cast<StartTurnMessage*>(messageToSend);
			outgoingStream.Write(*finalMessageToSend); //Dereference it for sending
		}
		break;
		case ID_FINALIZE_TURN_DATA:
		{
			printf("Sending Finalize Turn!\n");
			FinalizeTurnMessage* finalMessageToSend = static_cast<FinalizeTurnMessage*>(messageToSend);
			outgoingStream.Write(*finalMessageToSend); //Dereference it for sending
		}
		break;

		default:
			printf("Error! This GameMessage doesn't exist.");
		}

		//Send the message
		gpInstance.getPeer()->Send(&outgoingStream, HIGH_PRIORITY, RELIABLE_ORDERED, 0, gpInstance.getPeer()->GetMyGUID(), true);

		delete messageToSend;
	}
}

void clientWrite()
{
	NetworkingInstance& gpInstance = NetworkingInstance::getInstance();

	while (!gpInstance.getSenderQueue().empty())
	{
		RakNet::BitStream outgoingStream;

		//Get a pointer to the front of the queue
		ChampNetMessage* messageToSend = gpInstance.getSenderQueue().front();

		//Pop the front of the queue off
		gpInstance.getSenderQueue().pop();

		//Create NetworkMessageContainer to send
		ChampNetMessageContainer messageContainer[1];

		//Add timestamp
		messageContainer->timeStamp = RakNet::GetTime();

		//Add messageID of networkMessage to be sent after the messageContainer
		messageContainer->networkMessageID = messageToSend->networkMessageID;

		//Write the ID to the bitstream
		outgoingStream.Write(messageContainer->TIMESTAMP_ID);

		//Write the networkMessage to the bitstream
		outgoingStream.Write(messageContainer);

		//Cast NetworkMessage before you cast it
		switch (messageContainer->networkMessageID)
		{
		case ID_SEND_USER_DATA:
		{
			printf("Sending User Data!\n");
			UserData* finalMessageToSend = static_cast<UserData*>(messageToSend);
			outgoingStream.Write(*finalMessageToSend);
		}
		break;
		case ID_MESSAGE_DATA:
		{
			printf("Sending ChatMessage!\n");
			ChatMessage* finalMessageToSend = static_cast<ChatMessage*>(messageToSend);
			outgoingStream.Write(*finalMessageToSend);
		}
		break;
		case ID_UNIT_SELECT_DATA:
		{
			printf("Sending UnitSelectMessage!\n");
			UnitSelectMessage* finalMessageToSend = static_cast<UnitSelectMessage*>(messageToSend);
			outgoingStream.Write(*finalMessageToSend);
		}
		break;
		case ID_UNIT_MOVE_DATA:
		{
			printf("Sending Unit Move!\n");
			UnitMoveMessage* finalMessageToSend = static_cast<UnitMoveMessage*>(messageToSend);
			outgoingStream.Write(*finalMessageToSend); //Derefernce if for sending
		}
		break;
		case ID_SEED_DATA:
		{
			printf("Sending seed!\n");
			RandomSeedMessage* finalMessageToSend = static_cast<RandomSeedMessage*>(messageToSend);
			outgoingStream.Write(*finalMessageToSend); //Dereference it for sending
		}
		break;
		case ID_UNIT_DAMAGE_DATA:
		{
			printf("Sending Unit Damage!\n");
			UnitDamageMessage* finalMessageToSend = static_cast<UnitDamageMessage*>(messageToSend);
			outgoingStream.Write(*finalMessageToSend); //Dereference it for sending
		}
		break;
		case ID_START_TURN_DATA:
		{
			printf("Sending Start Turn!\n");
			StartTurnMessage* finalMessageToSend = static_cast<StartTurnMessage*>(messageToSend);
			outgoingStream.Write(*finalMessageToSend); //Dereference it for sending
		}
		break;
		case ID_FINALIZE_TURN_DATA:
		{
			printf("Sending Finalize Turn!\n");
			FinalizeTurnMessage* finalMessageToSend = static_cast<FinalizeTurnMessage*>(messageToSend);
			outgoingStream.Write(*finalMessageToSend); //Dereference it for sending
		}
		break;

		default:
			printf("Error!");
		}

		//Send the message
		gpInstance.getPeer()->Send(&outgoingStream, HIGH_PRIORITY, RELIABLE_ORDERED, 0, gpInstance.getPeer()->GetMyGUID(), true);

		delete messageToSend;
	}
}