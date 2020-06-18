////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  InteractablesHander : class | Written by Parker Staszkiewicz                                                  //
//  Manager for Interactable objects in the LobbyScene Waiting Minigame;                                          //
//  Spawns FallingObjects on a timer which decreases over time.                                                   //
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

public class InteractablesHandler : MonoBehaviour
{
    // Prefab of a fallingInteractable to be spawned
    public GameObject fallingObjectPrefab;

    // Initial timer values
    float timer = 2.0f;
    float timeBetweenSpawn = 1.5f;

    private void Update()
    {
        // Add time of last frame to timer
        timer += Time.smoothDeltaTime;

        // If enough time has passed, spawn new object
        // and reset timer values
        if (timer >= timeBetweenSpawn)
        {
            timer = 0;

            if (timeBetweenSpawn > 0.25f)
            {
                timeBetweenSpawn -= 0.25f;
            }

            SpawnFallingObject();
        }
    }

    /// <summary>
    /// Spawns a FallingObject gameObject within 
    /// bounds of -8f and 8f on X-Axis and at 8f on Y-Axis
    /// </summary>
    private void SpawnFallingObject()
    {
        float randomX = Random.Range(-8.0f, 8.0f);

        GameObject GO = Instantiate(fallingObjectPrefab, new Vector3(randomX, 8.0f, 0.0f), Quaternion.identity, transform);

        float scale = Random.Range(0.3f, 0.8f);

        GO.transform.localScale = new Vector3(scale, scale, 1);

        GO.GetComponent<FallingObject>().Init(scale);
    }
}
