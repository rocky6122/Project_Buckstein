////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  Floor : class | Written by Anthony Pascone                                                                    //
//  
//  
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour {

	public void Reshape(float width, float length)
    {
        transform.localScale = new Vector3(width, transform.localScale.y, length);
    }
}
