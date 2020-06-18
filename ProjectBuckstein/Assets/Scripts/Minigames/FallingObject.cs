////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  FallingObject : class | Written by Parker Staszkiewicz                                                        //
//  Component to sit on a GameObject with a RigidBody2D, SpriteRenderer, and Collider2D;                          //
//  Handles collision with "Floor" and "Unit" as part of Waiting Minigame.                                        //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(Collider2D))]
public class FallingObject : MonoBehaviour {

    public void Init(float scale)
    {
        Rigidbody2D rgbd = GetComponent<Rigidbody2D>();

        // Gravity scale changes based on size of object (i.e. how close it is to the screen)
        rgbd.gravityScale = scale * 1.5f;

        // Alpha of color changes based on size of object
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color startColor = sr.color;
        scale.Map(0.3f, 0.8f, 0.35f, 1.0f);
        startColor.a = scale;
        sr.color = startColor;
    }

    private void OnMouseDown()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Unit"))
        {
            SceneLoader.AddScore();
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Floor"))
        {
            Destroy(gameObject);
        }
    }
}
