////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  MainMenu : class | Written by Anthony Pascone and John Imgrund                                                //
//  Used in both the MenuScene and EndScene. Has functions for button in the scenes to use and will show the      //
//  correct message during the end scene.                                                                         //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public GameObject won;
    public GameObject lost;

    /// <summary>
    /// If it is the end scene, it will set the correct text to true and the other to false
    /// depending on whether the players won or lost
    /// </summary>
    private void Start()
    {
        if (PersistentData.GetIsEndScene())
        {
            if (PersistentData.GetPlayerHasWon())
            {
                won.SetActive(true);
                lost.SetActive(false);
            }
            else
            {
                won.SetActive(false);
                lost.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Quits the game.
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Loads the Main Menu Scene
    /// </summary>
    public void GoToMainMenue()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
