////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  ShotFeedbaclScript : class | Written by Anthony Pascone                                                       //
//  Script for the shot feedback animation. Once the animation is finished, it will call the DestroyThis function //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

public class ShotFeedbackScript : MonoBehaviour {

	public void DestroyThis()
    {
        Destroy(gameObject);
    }
}
