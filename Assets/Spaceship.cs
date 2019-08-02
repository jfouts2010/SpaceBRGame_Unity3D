
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Spaceship : MonoBehaviourPun
{
    Rigidbody rb;
    public GameObject CameraGameObject;
    public GameObject bullet;
    public Vector3 shipDirectionGoal;
    public Vector3 shipTurnGoal;
    public float forwardThrustForce = 1.2f;
    public float backwardThrustForce = 1.2f;
    public float rotationForce = 1.2f;
    public float cameraMovementSpeed = 7f;
    public float shipRotationSpeed = 3f;
    public float maximumVelcoity = 10;
    public float timeBetweenShots = 500;
    public float health = 100;

    public bool thrustersOn = false;
    private bool first = true;
    public GameObject lockOnTarget;
    public float lastShootTime = 0;

    void Start()
    {
        rb = transform.GetComponent<Rigidbody>();
        CameraGameObject = GameObject.Find("MainCamera").gameObject;
    }
    private void FixedUpdate()
    {
        Vector3 localEuler = this.transform.localEulerAngles;
        if (this.transform.localEulerAngles.x > 44)
            this.transform.localEulerAngles = new Vector3(44, localEuler.y, localEuler.z);
        if (this.transform.localEulerAngles.x < -44)
            this.transform.localEulerAngles = new Vector3(-44, localEuler.y, localEuler.z);

        if (this.transform.localEulerAngles.z > 44)
            this.transform.localEulerAngles = new Vector3(localEuler.x, localEuler.y, 44);
        if (this.transform.localEulerAngles.z < -44)
            this.transform.localEulerAngles = new Vector3(localEuler.x, localEuler.y, -44);


        if (this.transform.localEulerAngles.x != 0 || this.transform.localEulerAngles.z != 0)
        {
            this.transform.localEulerAngles = new Vector3(0, localEuler.y, 0);
        }
    }
    // Update is called once per frame
    void Update()
    {
        //limit velocity
        if (rb.velocity.magnitude > maximumVelcoity)
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maximumVelcoity);
        //limit turn speed
        if(rb.angularVelocity.magnitude > 1)
            rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, 1);
    }
    public void ForwardThrust(float percent)
    {
        rb.AddForceAtPosition(transform.forward * forwardThrustForce * Mathf.Clamp(percent,0,1), transform.position);
    }
    public void ReverseThrust()
    {
        rb.AddForceAtPosition(-1 * transform.forward * backwardThrustForce, transform.position);
    }
    public void TurnThrust(float percent, bool clockwise)
    {
        rb.AddTorque(Vector3.up * percent * (clockwise ? 1 : -1));
    }
    public void ZDirectionThrust(float percent, bool up)
    {
        rb.AddForce(Vector3.up * (up ? 1 : -1));
    }
    public void ShootGun(Vector3 target)
    {
        photonView.RPC("ShootGunRPC", RpcTarget.All, target);
    }
    [PunRPC]
    void ShootGunRPC(Vector3 direction, PhotonMessageInfo info)
    {
        GameObject newBullet = GameObject.Instantiate(bullet, transform.position, Quaternion.Euler(direction));
        newBullet.GetComponent<Bullet>().owner = this.gameObject;
        newBullet.transform.forward = direction.normalized;
        newBullet.GetComponent<Rigidbody>().AddForce(direction.normalized * 1500 + rb.velocity);
        GameObject.Destroy(newBullet, 10);
    }

    #region IPunObservable implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(this.lastShootTime);
            stream.SendNext(this.health);
        }
        else
        {
            // Network player, receive data
            this.lastShootTime = (float)stream.ReceiveNext();
            this.health = (float)stream.ReceiveNext();
        }
    }

    #endregion
}
