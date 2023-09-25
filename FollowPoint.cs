using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPoint : MonoBehaviour
{
    [SerializeField] private GameObject[] waypoints;
    [SerializeField] private SpriteRenderer sprite;
    private int currentWaypointIndex = 0;


    [SerializeField] private float speed = 2.2f;

   

    void Update()
    {
       

        if (Vector2.Distance(waypoints[currentWaypointIndex].transform.position, transform.position) < .1f)
        {
            currentWaypointIndex++;
            if(sprite != null) sprite.flipX = !sprite.flipX;
            speed = 2.2f;
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;
                speed = 1f; 
            }
        }
        transform.position = Vector2.MoveTowards(transform.position, waypoints[currentWaypointIndex].transform.position, Time.deltaTime * speed);
    }
}
