////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  StatDisplay : class | Written by Anthony Pascone and Parker Staszkiewicz                                      //
//      
//       
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatDisplay : MonoBehaviour {

    public GameObject statDisplay;

    public TextMeshProUGUI unitClassText;
    public TextMeshProUGUI unitNameText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI damageHealthText;
    public TextMeshProUGUI movesLeftText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI bloodTypeText;
    public TextMeshProUGUI likesText;
    public TextMeshProUGUI dislikesText;

    public Image moveImage;
    public Image shootImage;
    public Image heartbeatImage;

    public Sprite[] heartbeats;

    private Color standardMoveColor;
    public Color inactiveMoveColor;

    private Color standardShootColor;
    public Color inactiveShootColor;

    public void Initialize()
    {
        standardMoveColor = moveImage.color;
        standardShootColor = shootImage.color;
    }

    public void ShowText(string unitClass, string unitName, int health, int maxHealth, int moves, bool hasShot, int damage, string bloodType, string like, string dislike, int status)
    {
        unitClassText.text = unitClass;
        unitNameText.text = unitName;
        healthText.text = "Health: " + health.ToString() + "/" + maxHealth.ToString();
        movesLeftText.text = moves.ToString();
        damageText.text = damage.ToString();
        bloodTypeText.text = "Blood Type: " + bloodType;
        likesText.text = like;
        dislikesText.text = dislike;

        heartbeatImage.sprite = (status == 0) ? heartbeats[0] : heartbeats[1];

        moveImage.color = (moves > 0) ? standardMoveColor : inactiveMoveColor;
        shootImage.color = !hasShot ? standardShootColor : inactiveShootColor;

        statDisplay.SetActive(true);
    }

    public void DisableText()
    {
        statDisplay.SetActive(false);

        damageHealthText.enabled = false;
    }

    public void ShowDamage(int damage)
    {
        damageHealthText.enabled = true;

        string damageSTR;

        if (damage < 10)
        {
            damageSTR = "0" + damage.ToString();
        }
        else
        {
            damageSTR = damage.ToString();
        }

        damageHealthText.text = "-" + damageSTR;
    }
}
