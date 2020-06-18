////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  ChatSystem : class | Written by Parker Staszkiewicz, Anthony Pascone, and John Imgrund                        //
//        
//       
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using ChampNet;
using TMPro;

public class ChatSystem : MonoBehaviour {

    public RectTransform contentWindow;
    private int rectHeight;

    public TMP_InputField inputField;

    public GameObject chatBoxPrefab;

    private const int xPos = 2;
    private int nextYPos;
    private int verticalSeparation;
    private int endPos;

    private int numMessages;

    public void InitializeChatBox()
    {
        rectHeight = 90;

        nextYPos = -2;
        verticalSeparation = 12;

        endPos = 0;

        numMessages = 0;

        inputField.characterLimit = 40;
    }

    public bool IsActive()
    {
        return gameObject.activeInHierarchy;
    }

    public bool IsTyping()
    {
        return inputField.isFocused;
    }

    public void ShowAndHide(bool show)
    {
        gameObject.SetActive(show);

        if (show)
        {
            ActivateInput();
        }
    }

    public void ActivateInput()
    {
        inputField.ActivateInputField();
    }

    public void DeactivateInput()
    {
        inputField.DeactivateInputField();
    }

    public void SendTypedMessage()
    {
        if (!(inputField.text == ""))
        {
            AddMessage(PersistentData.GetUserName(), inputField.text);

            ChampNetManager.SendChatMessage(inputField.text);

            inputField.text = "";
        }

        ActivateInput();
    }

    public void AddMessage(string username, string message)
    {
        numMessages++;

        GameObject messageObject = Instantiate(chatBoxPrefab, contentWindow);

        RectTransform messageTransform = messageObject.GetComponent<RectTransform>();

        messageTransform.anchoredPosition = new Vector3(xPos, nextYPos, 0);

        TextMeshProUGUI gui = messageObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        if (username == "HOST")
        {
            gui.color = Color.green;
        }

        gui.text = "[" + username + "]: " + message;

        nextYPos -= verticalSeparation;

        if (numMessages > 7)
        {
            rectHeight += 12;

            contentWindow.sizeDelta = new Vector2(0, rectHeight);


            float difference = Mathf.Abs(endPos - contentWindow.anchoredPosition.y);
            endPos += 12;

            if (difference <= 10)
            {
                contentWindow.anchoredPosition = new Vector3(0, endPos);
            }
        }
    }
}
