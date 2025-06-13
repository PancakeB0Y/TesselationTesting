using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FollowPath : MonoBehaviour
{
    public float moveSpeed = 8;

    public List<GameObject> waypoints;

    string waypointTag = "Waypoint";

    Vector3 startPos;
    Transform nextWaypoint;
    void Start()
    {
        startPos = transform.position;

        waypoints = GameObject.FindGameObjectsWithTag(waypointTag).ToList();
        waypoints.Reverse();

        if (waypoints.Count > 0) {
            nextWaypoint = waypoints[0].transform;
        }
    }

    
    void Update()
    {
        if (nextWaypoint == null) {
            return;
        }

        Vector3 moveDirection = (nextWaypoint.position - transform.position).normalized;

        transform.position += moveDirection * Time.deltaTime * moveSpeed;

        float distanceToWaypoint = Vector3.Distance(transform.position, nextWaypoint.position);

        if (distanceToWaypoint <= 0.1)
        {
            UpdateNextWaypoint();
        }
    }

    void UpdateNextWaypoint()
    {
        // check if the current target waypoint is the last one
        if (nextWaypoint == waypoints[waypoints.Count - 1].transform)
        {
            nextWaypoint = null;
            return;
        }


        int waypointIndex = -1; 

        // get the index of the next waypoint
        for (int i = 0; i < waypoints.Count; i++) {
            if(waypoints[i].transform == nextWaypoint)
            {
                waypointIndex = i + 1;
                break;
            }
        }

        // update the next waypoint
        nextWaypoint = waypoints[waypointIndex].transform;
    }
}
