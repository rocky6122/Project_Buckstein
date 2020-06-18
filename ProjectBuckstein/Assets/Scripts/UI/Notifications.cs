////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  Notifications : class | Written by Anthony Pascone and Parker Staszkiewicz                                    //
//  Manages the notification panel when the text chat is not open in game.                                        //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using TMPro;

public class Notifications : MonoBehaviour {

    public TextMeshProUGUI number;

    int notificationsNum;

    /// <summary>
    /// Sets notification number to zero and shows the notification panel
    /// </summary>
    public void ResetNotifications()
    {
        notificationsNum = 0;
        ShowNotifications();
    }

    /// <summary>
    /// Increase notification number
    /// </summary>
    public void AddNotification()
    {
        notificationsNum++;
        ShowNotifications();
    }

    /// <summary>
    /// Change the text of the notification number to be what we have stored and show the notification panel
    /// </summary>
    public void ShowNotifications()
    {
        number.text = notificationsNum.ToString();

        gameObject.SetActive(true);
    }

    /// <summary>
    /// Turns off the notification panel
    /// </summary>
    public void TurnOff()
    {
        gameObject.SetActive(false);
    }
}
