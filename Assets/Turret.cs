using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Photon.Pun;

public class Turret : MonoBehaviourPun
{
    GameObject target;
    public GameObject bullet;

    public void Update()
    {
        if (target == null)
        {
            GameObject go = GameObject.FindWithTag("Ship");
            if (go != null)
                target = go;
        }
        else
        {
            float shootVelocityMagnitude = 20;
            //shoot at target
            float distanceToTarget = Vector3.Distance(target.transform.position, transform.position);
            Vector3 targetVelocity = target.transform.GetComponent<Rigidbody>().velocity - transform.GetComponent<Rigidbody>().velocity;
            float timeToTarget = distanceToTarget / shootVelocityMagnitude;
           
            Vector3 targetPositionAfterTime = targetVelocity * timeToTarget + target.transform.position;
            for (int i = 0; i < 10; i++)
            {
                distanceToTarget = Vector3.Distance(targetPositionAfterTime, transform.position);
                timeToTarget = distanceToTarget / shootVelocityMagnitude;
                targetPositionAfterTime = targetVelocity * timeToTarget + target.transform.position;
            }

            Vector3 shootVector = targetPositionAfterTime - transform.position;
            Vector3 shootVelocity = shootVector.normalized * shootVelocityMagnitude;
            GameObject newBullet = Instantiate(bullet, transform.position, Quaternion.identity);
            newBullet.transform.forward = shootVector;
            newBullet.GetComponent<Rigidbody>().velocity = shootVelocity;
            GameObject.Destroy(newBullet, 10);
        }
    }
}