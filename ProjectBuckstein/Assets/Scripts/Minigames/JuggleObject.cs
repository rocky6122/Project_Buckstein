////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  JuggleObject : class | Written by Parker Staszkiewicz                                                         //
//  Component to sit on a GameObject with a RigidBody2D and Collider2D;                                           //
//  OnMouseDown event adds force to the object based on where it was clicked.                                     //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class JuggleObject : MonoBehaviour
{
    private Rigidbody2D rgbd;

    public int force = 250;

    private void Start()
    {
        rgbd = GetComponent<Rigidbody2D>();
    }

    private void OnMouseDown()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector2 differenceVector;

        rgbd.velocity = new Vector2(rgbd.velocity.x, 0);

        // Compensates for whether it was clicked below or above the midpoint of the object
        if (mousePos.y < transform.position.y)
        {
            differenceVector = (Vector2)transform.position - mousePos;
        }
        else
        {
            differenceVector = mousePos - (Vector2)transform.position;

            differenceVector = new Vector2(-differenceVector.x, differenceVector.y);
        }

        differenceVector.Normalize();
        rgbd.AddForce(differenceVector * force);
    }
}
