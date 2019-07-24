
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spaceship : MonoBehaviour
{
    Rigidbody rb;
    GameObject CameraGameObject;
    public GameObject bullet;
    // Start is called before the first frame update
    void Start()
    {
        rb = transform.GetComponent<Rigidbody>();
        CameraGameObject = transform.Find("MainCamera").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            rb.AddForceAtPosition(transform.forward * 1.2f, transform.position);
        }
        if (Input.GetKey(KeyCode.S))
        {
            rb.AddForceAtPosition(-1 * transform.forward * 1.2f, transform.position);
        }
       

        //put the camera behind the gameobject
        CameraGameObject.transform.position = Vector3.Lerp(CameraGameObject.transform.position, transform.position + (transform.up * 0.8f) + (-3 * CameraGameObject.transform.forward.normalized), Time.deltaTime * 7);
        //user force to rotate the ship towards the cameras vector
        transform.forward = Vector3.Lerp(transform.forward, CameraGameObject.transform.forward, Time.deltaTime);
        //shoot a bullet
        if (Input.GetKey(KeyCode.Space))
        {
            GameObject newBullet = GameObject.Instantiate(bullet, transform.position, Quaternion.identity);
            newBullet.GetComponent<Rigidbody>().AddForce(transform.forward*500);
            GameObject.Destroy(newBullet, 10);
        }
    }
}
