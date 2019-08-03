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
    public void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
            return;
        if (this.photonView.Owner.NickName.ToLower().StartsWith("i"))
        {
            UpdateIan();
        }
        else
        {
            UpdateJohn();
            int x = 5;
        }
    }

    public void UpdateTarget()
    {
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

    }

    public void UpdateIan()
    {
        //Debug.Log("IM IAN");

        UpdateTarget();

        float shootVelocityMagnitude = 20;
        //shoot at target
        float distanceToTarget = Vector3.Distance(target.transform.position, transform.position);
        Vector3 targetVelocity = target.transform.GetComponent<Rigidbody>().velocity;// - transform.GetComponent<Rigidbody>().velocity;
        float timeToTarget = distanceToTarget / shootVelocityMagnitude;
        //timeToTarget = timeToTarget *1.15f;
        Vector3 targetPositionAfterTime = targetVelocity * timeToTarget + target.transform.position;
        float diffindistances = 100;
        while (diffindistances > .5)
        {
            distanceToTarget = Vector3.Distance(targetPositionAfterTime, transform.position);
            timeToTarget = distanceToTarget / shootVelocityMagnitude;
            Vector3 newpos = targetVelocity * timeToTarget + target.transform.position;
            diffindistances = Vector3.Distance(targetPositionAfterTime, newpos);
            targetPositionAfterTime = newpos;
        }
        //Vector3 targetPositionAfterTime = targetVelocity * timeToTarget + target.transform.position;
        //for (int i = 0; i < 10; i++)
        //{
        //    distanceToTarget = Vector3.Distance(targetPositionAfterTime, transform.position);
        //    timeToTarget = distanceToTarget / shootVelocityMagnitude;
        //    targetPositionAfterTime = targetVelocity * timeToTarget + target.transform.position;
        //}

        System.Random r = new System.Random();
        //pretend the target changes by 5% in travel time. Perhaps take distance into account. 
        float diffx = target.transform.position.x - targetPositionAfterTime.x;
        float diffy = target.transform.position.y - targetPositionAfterTime.y;
        float diffz = target.transform.position.z - targetPositionAfterTime.z;
        targetPositionAfterTime.x = target.transform.position.x - (1f - (r.Next(-5, 5) / 25f)) * diffx;
        targetPositionAfterTime.y = target.transform.position.y - (1f - (r.Next(-5, 5) / 25f)) * diffy;
        targetPositionAfterTime.z = target.transform.position.z - (1f - (r.Next(-5, 5) / 25f)) * diffz;

        Vector3 shootVector = targetPositionAfterTime - transform.position;
        Vector3 shootVelocity = shootVector.normalized * shootVelocityMagnitude;

        /*GameObject newBullet = Instantiate(bullet, transform.position, Quaternion.identity);
        newBullet.GetComponent<Bullet>().owner = this.gameObject;
        newBullet.transform.forward = shootVector;
        newBullet.GetComponent<Rigidbody>().velocity = shootVelocity + this.GetComponent<Rigidbody>().velocity;
        GameObject.Destroy(newBullet, 10);*/
        this.GetComponent<Spaceship>().ShootGun(shootVector);
    }

    public void UpdateJohn()
    {
        UpdateTarget();

        float shootVelocityMagnitude = 20;
        //shoot at target
        Vector3 ourVelocity = transform.GetComponent<Rigidbody>().velocity;
        float distanceToTarget = Vector3.Distance(target.transform.position, transform.position);
        Vector3 targetVelocity = target.transform.GetComponent<Rigidbody>().velocity;
        float timeToTarget = distanceToTarget / (shootVelocityMagnitude + ourVelocity.magnitude);

        Vector3 targetPositionAfterTime = targetVelocity * timeToTarget + target.transform.position;
        for (int i = 0; i < 10; i++)
        {
            distanceToTarget = Vector3.Distance(targetPositionAfterTime, transform.position);
            timeToTarget = distanceToTarget / (shootVelocityMagnitude + ourVelocity.magnitude);
            targetPositionAfterTime = targetVelocity * timeToTarget + target.transform.position;
        }

        Vector3 shootVector = targetPositionAfterTime - transform.position;
        Vector3 shootVelocity = shootVector.normalized * shootVelocityMagnitude;
        /* GameObject newBullet = Instantiate(bullet, transform.position, Quaternion.identity);
         newBullet.transform.forward = shootVector;
         newBullet.GetComponent<Bullet>().owner = this.gameObject;
         newBullet.GetComponent<Rigidbody>().velocity = shootVelocity + this.GetComponent<Rigidbody>().velocity;
         GameObject.Destroy(newBullet, 10);*/
        this.GetComponent<Spaceship>().ShootGun(shootVector);
    }
}
