using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missle : Projectile
{
    public GameObject Target;
    float timeStart = 0;
    public float PrepTime = 2;
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        timeStart = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > timeStart + PrepTime)
        {
            transform.LookAt(Target.transform);
            if (Vector3.Angle(transform.forward, Target.transform.position - transform.position) < 5)
                rb.AddForce(transform.forward * 30);
        }
    }
}
