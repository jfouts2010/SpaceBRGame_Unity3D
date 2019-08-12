using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviourPun
{
    public Photon.Realtime.Player owner;
    public GameObject ownerGameObject;
    public float bulletDamage = 10;
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

        if (other.transform.root.tag == "Ship")
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
            if (!PhotonNetwork.IsMasterClient)
                return;
            Spaceship ss = other.transform.root.gameObject.GetComponent<Spaceship>();
            //if there is enough energy to absorb the shot, absorb it, else ignore it
            if (other.tag == "EnergyShield")
            {
                Destroy(this.gameObject);
                ss.energyshield -= bulletDamage;
                ownerGameObject.transform.Find("PlayerUI(Clone)").GetComponent<PlayerUI>().hitMarkerStartTime = Time.time;
            }
            else if(other.tag != "EnergyShield")
            {
                Destroy(this.gameObject);
                ownerGameObject.transform.Find("PlayerUI(Clone)").GetComponent<PlayerUI>().hitMarkerStartTime = Time.time;
                if (ss != null)
                {

                    SpaceshipSystem sys = other.GetComponent<SystemTag>().system;
                    ss.TakeSystemDamageRPC(sys, bulletDamage);
                }
            }
        }
    }
}
