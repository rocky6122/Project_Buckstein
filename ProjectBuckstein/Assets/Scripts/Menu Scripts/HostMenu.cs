////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  HostMenu : class | Written by John Imgrund                                                                    //
//  
//  
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using ChampNet;



public class HostMenu : MonoBehaviour
{

    public TMP_InputField userName;

    private void Start()
    {
        userName.characterLimit = 12;
    }

    /// <summary>
    ///Sets up a server and starts the game
    /// </summary>
    public void HostServer()
    {
        ChampNetManager.InitHost(userName.text);

        PersistentData.SetUserName(userName.text);
        PersistentData.SetHosting(true);

        NextScene();
    }


    private void NextScene()
    {
        if (userName.text == "")
        {
            return;
        }

        SceneManager.LoadScene(1);
    }
}