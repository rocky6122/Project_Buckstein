////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  SceneLoader : class | Written by Parker Staszkiewicz                                                          //
//  Container for UI elements in the LobbyScene; additionally, handles incoming peer connection.                  //
//  Keeps track of Minigame score as well as handles transitioning to the next scene.                             //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEngine.SceneManagement;
using ChampNet;
using TMPro;

public class SceneLoader : MonoBehaviour {

    public TextMeshProUGUI scoreText;
    private static int score;

    public TextMeshProUGUI tipText;
    public TextMeshProUGUI tipNameText;
    public TextMeshProUGUI tipGameText;
    private float timer;
    private float waitTime;

    private LitJson.JsonData data;
    private int index;
    private int indexMax;

    private void Start()
    {
        // Loads data from json file
        JSONReadManager.LoadFile("tipsFile");
        data = JSONReadManager.GetItemData("tipsFile");

        // Initializes variables
        score = 0;
        timer = 0f;
        waitTime = 10.5f;

        // Creates first tip on screen
        CreateTip();
    }

    void Update ()
    {
        // Updates score each frame
        scoreText.text = score.ToString();

        // Handles timer for events
        timer += Time.deltaTime;

        if (timer >= waitTime)
        {
            timer = 0f;

            ChangeTip();
        }

        int clientNum = ChampNetManager.GetClientNum();

        if (clientNum == 2)
        {
            GoToGame();
        }
    }

    /// <summary>
    /// Generates a tip from the JsonData object
    /// and displays it to the screen.
    /// </summary>
    private void CreateTip()
    {
        indexMax = (int)data["numTips"];

        index = Random.Range(0, indexMax);

        indexMax--;

        tipText.text = (string)data["tips"][index];
        tipNameText.text = (string)data["tipNames"][index];
        tipGameText.text = (string)data["tipGames"][index];
    }

    /// <summary>
    /// Iterates through the objects within the JsonData object,
    /// changing the tip displayed to the screen.
    /// </summary>
    private void ChangeTip()
    {
        index++;

        if (index > indexMax)
        {
            index = 0;
        }

        tipText.text = (string)data["tips"][index];
        tipNameText.text = (string)data["tipNames"][index];
        tipGameText.text = (string)data["tipGames"][index];
    }

    /// <summary>
    /// Loads the GameScene
    /// </summary>
    private void GoToGame()
    {
        SceneManager.LoadScene(2);
    }

    /// <summary>
    /// Adds a single value to the score variable.
    /// </summary>
    public static void AddScore()
    {
        score++;
    }
}
