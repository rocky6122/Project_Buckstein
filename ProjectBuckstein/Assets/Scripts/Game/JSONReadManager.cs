////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  JSONReadManager : static class | Written by Parker Staszkiewicz                                               //
//  Manager for JsonData objects, part of LitJson package, found at https://litjson.net/                          //
//  Static dictionary holds JsonData objects with filenames as keys for access.                                   //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;

public static class JSONReadManager
{
    // Dictionary (Map) of JsonData objects
    private static Dictionary<string, JsonData> JSONdictionary = new Dictionary<string, JsonData>();

    /// <summary>
    /// Loads a .json file and converts it into a 
    /// JsonData object from the LitJson package.
    /// </summary>
    /// <param name="fileName">Filename to be opened, without the .json extension.</param>
    public static void LoadFile(string fileName)
    {
        string jsonString = File.ReadAllText(Application.dataPath + "/JSONFiles/" + fileName + ".json");
        JsonData itemData = JsonMapper.ToObject(jsonString);
        JSONdictionary.Add(fileName, itemData);
    }

    /// <summary>
    /// Returns the JsonData object at the specified key.
    /// </summary>
    /// <param name="fileName">Key for accessing the JsonData from the Dictionary.</param>
    /// <returns>The JsonData object at the specified key of the Dictionary.</returns>
    public static JsonData GetItemData(string fileName)
    {
        return JSONdictionary[fileName];
    }
}
