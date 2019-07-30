using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AIShip : MonoBehaviourPun
{
    GameObject[] Players;
    GameObject lastClosestTarget;
    GameObject targetedPlayer;
    // Start is called before the first frame update
    void Start()
    {
        Players = GameObject.FindGameObjectsWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        //look for the closest player and target them, only checking every 10 seconds and only if they are still the closest target
        if (Time.time % 10 == 0)
        {
            float distanceToTarget = Vector3.Distance(targetedPlayer.transform.position, this.transform.position);
            GameObject closestTarget = targetedPlayer;
            foreach (GameObject ship in Players)
            {
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

        //try to orbit closest player

    }
}
