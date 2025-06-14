using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class FollowPath : MonoBehaviour
{
    public float moveSpeed = 8;

    [SerializeField] List<GameObject> waypoints;

    Vector3 startPos;
    Transform nextWaypoint;

    Vector3 moveDirection;

    void Start()
    {
        startPos = transform.position;

        StartCoroutine(InitializeWaypointsCoroutine());
    }
    
    IEnumerator InitializeWaypointsCoroutine()
    {
        yield return new WaitForSeconds(2);

        InitializeWaypoints();

        if (FPSCounter.instance != null) {
            FPSCounter.instance.StartTrackingFPS();
        }
    }

    void InitializeWaypoints()
    {
        // set the first waypoint and the direction towards it
        if (waypoints.Count > 0)
        {
            nextWaypoint = waypoints[0].transform;
            moveDirection = (nextWaypoint.position - transform.position).normalized;

            // rotate towards the next waypoint
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }
    
    void Update()
    {
        if (nextWaypoint == null) {
            return;
        }

        // move in the direction of the next waypoint
        transform.position += moveDirection * Time.deltaTime * moveSpeed;

        if (HasReachedWaypoint())
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

            if (FPSCounter.instance != null)
            {
                FPSCounter.instance.StopTrackingFPS();
            }

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

        // update the move direction to the new waypoint
        moveDirection = (nextWaypoint.position - transform.position).normalized;

        // rotate towards new waypoint
        transform.rotation = Quaternion.LookRotation(moveDirection);
    }

    // check if close enough to the waypoint
    bool HasReachedWaypoint()
    {
        float tolerance = 0.1f;

        float distanceToWaypoint = Vector3.Distance(transform.position, nextWaypoint.position);

        return distanceToWaypoint <= tolerance;
    }
}
