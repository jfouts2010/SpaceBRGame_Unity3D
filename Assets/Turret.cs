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
    public int tickcounter = 0;
    public void Update2()
    {
        

    }

    public void Update()
    {
        if (this.photonView.Owner.NickName.ToLower().StartsWith("i"))
        {
            Debug.Log("IM IAN");
            if (tickcounter++ % 5 == 0 || target == null)
            {
                float mindist = float.MaxValue;
                GameObject[] gos = GameObject.FindGameObjectsWithTag("Ship");
                foreach (GameObject go in gos)
                {
                    if (go != null && this.gameObject != go.gameObject)
                    {
                        float tempdist = Vector3.Distance(go.transform.position, transform.position);
                        if (tempdist < mindist)
                        {
                            mindist = tempdist;
                            target = go;
                        }
                    }
                }
            }

            if (tickcounter % 5 != 0)
            {
                if (tickcounter > 1000000)
                    tickcounter = 0;
                return;
            }

            float shootVelocityMagnitude = 20;
            //shoot at target
            float distanceToTarget = Vector3.Distance(target.transform.position, transform.position);
            Vector3 targetVelocity = target.transform.GetComponent<Rigidbody>().velocity - transform.GetComponent<Rigidbody>().velocity;
            float timeToTarget = distanceToTarget / shootVelocityMagnitude;

            //Vector3 targetPositionAfterTime = targetVelocity * timeToTarget + target.transform.position;
            //float diffindistances = 100;
            //while (diffindistances > .5)
            //{
            //    distanceToTarget = Vector3.Distance(targetPositionAfterTime, transform.position);
            //    timeToTarget = distanceToTarget / shootVelocityMagnitude;
            //    Vector3 newpos = targetVelocity * timeToTarget + target.transform.position;
            //    diffindistances = Vector3.Distance(targetPositionAfterTime, newpos);
            //    targetPositionAfterTime = newpos;
            //}
            Vector3 targetPositionAfterTime = targetVelocity * timeToTarget + target.transform.position;
            for (int i = 0; i < 10; i++)
            {
                distanceToTarget = Vector3.Distance(targetPositionAfterTime, transform.position);
                timeToTarget = distanceToTarget / shootVelocityMagnitude;
                targetPositionAfterTime = targetVelocity * timeToTarget + target.transform.position;
            }

            System.Random r = new System.Random();
            //pretend the target changes by 5% in travel time. Perhaps take distance into account. 
            float diffx = target.transform.position.x - targetPositionAfterTime.x;
            float diffy = target.transform.position.y - targetPositionAfterTime.y;
            float diffz = target.transform.position.z - targetPositionAfterTime.z;
            targetPositionAfterTime.x = target.transform.position.x - (1f - (r.Next(-5, 5) / 20f)) * diffx;
            targetPositionAfterTime.y = target.transform.position.y - (1f - (r.Next(-5, 5) / 20f)) * diffy;
            targetPositionAfterTime.z = target.transform.position.z - (1f - (r.Next(-5, 5) / 20f)) * diffz;

            Vector3 shootVector = targetPositionAfterTime - transform.position;
            Vector3 shootVelocity = shootVector.normalized * shootVelocityMagnitude;

            GameObject newBullet = Instantiate(bullet, transform.position, Quaternion.identity);
            newBullet.transform.forward = shootVector;
            newBullet.GetComponent<Rigidbody>().velocity = shootVelocity;
            GameObject.Destroy(newBullet, 10);
        }
        else
        {
            if (tickcounter++ % 5 == 0 || target == null)
            {
                float mindist = float.MaxValue;
                GameObject[] gos = GameObject.FindGameObjectsWithTag("Ship");
                foreach (GameObject go in gos)
                {
                    if (go != null)
                    {
                        float tempdist = Vector3.Distance(go.transform.position, transform.position);
                        if (tempdist < mindist)
                        {
                            mindist = tempdist;
                            target = go;
                        }
                    }
                }
            }
            if (tickcounter % 5 != 0)
            {
                if (tickcounter > 1000000)
                    tickcounter = 0;
                return;
            }

            float shootVelocityMagnitude = 20;
            //shoot at target
            float distanceToTarget = Vector3.Distance(target.transform.position, transform.position);
            Console.WriteLine(target.transform.position.x + " " + target.transform.position.x + " " + target.transform.position.z);
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