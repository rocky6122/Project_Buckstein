/*
	EGP Networking: Plugin Configuration
	Dan Buckstein
	October 2018

	Utility for configuring library symbol exports and imports.
*/

///////////////////////////////////////////////////////////////////////////////
// Lightly Modified by John Imgrund, Anthony Pascone, Parker Staszkiewicz
///////////////////////////////////////////////////////////////////////////////

#ifndef _EGP_NET_PLUGIN_CONFIG_H_
#define _EGP_NET_PLUGIN_CONFIG_H_


#ifdef EGP_NET_DLLEXPORT
// compiler logic for DLL-producing project
// e.g. the plugin itself
#define DLL_SYMBOL __declspec(dllexport) // tmp linker flag, forces lib to exist

#else	// !EGP_NET_DLLEXPORT
#ifdef EGP_NET_DLLIMPORT
// compiler logic for DLL-consuming project
// e.g. Unity game; test app
#define DLL_SYMBOL __declspec(dllimport) // tmp linker flag, forces lib to exist

#else	// !EGP_NET_DLLIMPORT
// compiler logic for DLL-unrelated project
// e.g. static code project

#define DLL_SYMBOL // tmp linker flag, forces lib to exist

#endif	// EGP_NET_DLLIMPORT
#endif	// EGP_NET_DLLEXPORT


#endif	// !_EGP_NET_PLUGIN_CONFIG_H_