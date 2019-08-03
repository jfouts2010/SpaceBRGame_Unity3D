using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Bullet : MonoBehaviour
{
    public Photon.Realtime.Player owner;
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
        if(other.tag == "Ship")
        {
            //check to make sure this isnt you
            PhotonView v = other.gameObject.GetComponent<PhotonView>();
            if (v != null)
            {
                if (v.Owner == owner)
                    return; //ignore if its yourself
            }
            else
                return; //if it doesnt have a photon view you can probably ignore it
            Destroy(this.gameObject);
            Spaceship ss = other.GetComponent<Spaceship>();
            if(ss != null)
            {
                ss.health -= 10;
            }
        }
    }
}
