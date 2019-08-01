using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class AIShip : MonoBehaviourPun
{
    Rigidbody rb;
    GameObject[] Players;
    GameObject lastClosestTarget;
    public GameObject targetedPlayer;
    public float targetOrbitRange = 50;

    private float retargeting = 0.0f;
    public float retargetPeriod = 10f;
    private Spaceship ss;
    public Vector3 movementGoalsRelative;
    // Start is called before the first frame update
    void Start()
    {
        rb = this.transform.GetComponent<Rigidbody>();
        Players = GameObject.FindGameObjectsWithTag("Ship");
        ss = this.transform.GetComponent<Spaceship>();
    }

    // Update is called once per frame
    void Update()
    {
        //look for the closest player and target them, only checking every 10 seconds and only if they are still the closest target
        if (Time.time > retargeting)
        {
            retargeting += retargetPeriod;
            Players = GameObject.FindGameObjectsWithTag("Ship");
            float distanceToTarget = 10000;

            GameObject closestTarget = null;
            if (targetedPlayer != null)
            {
                Vector3.Distance(targetedPlayer.transform.position, this.transform.position);
                closestTarget = targetedPlayer;
            }
            foreach (GameObject ship in Players)
            {
                if (ship.GetComponent<AIShip>() != null)
                    continue;
                float distance = Vector3.Distance(ship.transform.position, this.transform.position);
                if (distance < distanceToTarget)
                {
                    closestTarget = ship;
                }
            }
            if (lastClosestTarget == closestTarget && lastClosestTarget != targetedPlayer)
                targetedPlayer = closestTarget;
            lastClosestTarget = closestTarget;
        }

        //find out where they should move to
        if (targetedPlayer != null && (movementGoalsRelative == null || movementGoalsRelative == Vector3.zero || Vector3.Distance(this.transform.position, movementGoalsRelative + targetedPlayer.transform.position) < 10))
        {
            FindNextMovementGoal();
        }

        if (movementGoalsRelative != null && targetedPlayer != null)
        {
            Debug.DrawLine(targetedPlayer.transform.position, movementGoalsRelative + targetedPlayer.transform.position, Color.green, .1f);

            //rotate towards x,z of location
            Vector3 movementGoal = movementGoalsRelative + targetedPlayer.transform.position;
            Vector3 movementGoalXZ = new Vector3(movementGoal.x, 0, movementGoal.z);
            Vector3 forwardVectorXZ = new Vector3(transform.forward.x, 0, transform.forward.z);
            Vector3 transformXZ = new Vector3(transform.position.x, 0, transform.position.z);
            float movementGoalAngle = Vector3.Angle(forwardVectorXZ, movementGoalXZ - transformXZ);
            //if direction is positive, its to the left, if negative, to the right
            float direction = Vector3.Cross(transform.forward, movementGoal - transform.position).y;
            if (movementGoalAngle > 5)
            {
                float perfectPower = Mathf.Clamp(Mathf.Log(movementGoalAngle + .4f) / 2f, .1f, 1);
                if (direction > 0)
                {
                    if(movementGoalAngle < 30 && rb.angularVelocity.y > .2f)
                        ss.TurnThrust(1, false);
                    ss.TurnThrust(perfectPower, true);
                }
                else
                {
                    if (movementGoalAngle < 30 && rb.angularVelocity.y < -.2f)
                        ss.TurnThrust(1, true);
                    ss.TurnThrust(perfectPower, false);
                }
            }
            //accelerate only when facing right direction

            //turned enough to thrust
            if (movementGoalAngle < 10)
            {
                float velocityVectorAngle = Vector3.Angle(rb.velocity.normalized, movementGoal - transform.position);
                if (Vector3.Distance(this.transform.position, movementGoalsRelative + targetedPlayer.transform.position) > 100)
                {
                    ss.ForwardThrust(1f);
                }
                //dont bother turning on if its already going towards vector and going fast enough
                else if (velocityVectorAngle > 30 || rb.velocity.magnitude < 2)
                {
                    ss.ForwardThrust(.7f);
                    Debug.DrawLine(this.transform.position, this.transform.position + this.transform.forward * 5, Color.red, .1f);
                }

            }
        }
    }
    public void FindNextMovementGoal()
    {
        Vector3 toTargetVector = (targetedPlayer.transform.position - this.transform.position);
        Vector3 res = Vector3.Cross(toTargetVector, Vector3.up);
        Vector3 targetPosition = res.normalized * targetOrbitRange;
        Debug.DrawLine(this.transform.position, targetedPlayer.transform.position, Color.red, .1f);
        Debug.DrawLine(targetedPlayer.transform.position, targetPosition, Color.blue, .1f);
        movementGoalsRelative = targetPosition - targetedPlayer.transform.position;
    }
}
