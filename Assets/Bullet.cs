using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject owner;
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
        if(other.tag == "Ship" && other.gameObject != owner)
        {
            Destroy(this.gameObject);
            Spaceship ss = other.GetComponent<Spaceship>();
            if(ss != null)
            {
                ss.health -= 10;
            }
        }
    }
}
