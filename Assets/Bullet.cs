using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Bullet : MonoBehaviourPun
{
    public Photon.Realtime.Player owner;
    public int bulletDamage = 10;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        if(other.transform.root.tag == "Ship")
        {
            //check to make sure this isnt you
            PhotonView v = other.transform.root.gameObject.GetComponent<PhotonView>();
            if (v != null)
            {
                if (v.Owner == owner)
                    return; //ignore if its yourself
            }
            else
                return; //if it doesnt have a photon view you can probably ignore it
            Destroy(this.gameObject);
            Spaceship ss = other.transform.root.gameObject.GetComponent<Spaceship>();
            if(ss != null)
            {
                ss.SystemHealth[SpaceshipSystem.Hull] -= bulletDamage;
                if (ss.SystemHealth[SpaceshipSystem.Hull] <= 0)
                    ss.SystemDestroyed(SpaceshipSystem.Hull);
              
                SpaceshipSystem sys = other.GetComponent<SystemTag>().system;
                if (ss.SystemHealth[sys] > 0)
                {
                    ss.SystemHealth[sys] -= bulletDamage;
                    if (ss.SystemHealth[sys] <= 0)
                        ss.SystemDestroyed(sys);
                }
            }
        }
    }
}
