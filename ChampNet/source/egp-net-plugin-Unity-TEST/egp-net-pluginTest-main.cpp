/*
	EGP Networking: Plugin Test
	Dan Buckstein
	October 2018

	Developer's test application for plugin for Unity.
*/

// include the plugin interface file to take the "easy route"
#include "../egp-net-plugin-Unity/egp-net-plugin.h"
#include <iostream>
#include <thread> //testing
#include <chrono> //testing
#include <string>


// macro for name of plugin
#define PLUGIN_NAME "egp-net-plugin-Unity"


#ifdef _EGP_NET_PLUGIN_H_
// included plugin interface
// link static version of plugin
#pragma comment(lib,PLUGIN_NAME".lib")
#else
// no knowledge of functions in plugin
// the goal of this project is to "fake" what the plugin should do
// therefore, we need to load it and consume it like Unity will
#include <Windows.h>
#endif	// _EGP_NET_PLUGIN_H_


int main(int const argc, char const *const *const argv)
{
#ifndef _EGP_NET_PLUGIN_H_
	// consume library
	HMODULE library = LoadLibrary(PLUGIN_NAME".dll");
	if (library)
	{
		// load and test functions from library

#endif	// !_EGP_NET_PLUGIN_H_

		InitializeHostNetworking("John");
		//InitializeClientNetworking("Parker", "216.93.149.83"); //MUST BE EITHER EMPTY OR A SET OF NUMBERS. NO WORDS

		std::this_thread::sleep_for(std::chrono::milliseconds(10000)); //testing

		//printf("sending unitLock\n");
		//AddUnitSelectMessageToSender(2, '1');

		printf("Sending ChatMessage\n");
		AddChatMessageToSender("I am a message.");


		int testReturn = ReceiveMessageType();

		std::this_thread::sleep_for(std::chrono::milliseconds(30000)); //testing


		printf("Tester: %d", testReturn);

		StopNetworking();

		char* answer;

		answer = tester("217.69.149.93.12");

		printf("%s\n", answer);

		system("pause");

#ifndef _EGP_NET_PLUGIN_H_
		// release library
		FreeLibrary(library);
	}
#endif	// !_EGP_NET_PLUGIN_H_
}
