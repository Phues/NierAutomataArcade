using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearSphereEnemy : Enemy
{
    [SerializeField] private Transform[] waypoints;
    private int wayPointIndex=0;

    public override void Move()
    {
        
        if (wayPointIndex<= waypoints.Length - 1)
        {
            //Debug.Log(wayPointIndex);
            Vector3 targetPosition = waypoints[wayPointIndex].position;
            // Calculate the direction to the target position
            Vector3 direction = targetPosition - transform.position;
            // Calculate the movement for the frame based on the direction and speed
            Vector3 movement = direction.normalized * (speed * Time.deltaTime);

            // Move the sphere towards the current waypoint
            transform.Translate(movement);

            // Check if the sphere has reached the current waypoint
            if (Vector3.Distance(transform.position, waypoints[wayPointIndex].position) < 0.1f)
            {
                // Move to the next waypoint
                wayPointIndex++;
            }
        }
        else
        {
            wayPointIndex = 0;
        }
    }
}
