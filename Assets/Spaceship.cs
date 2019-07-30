
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Spaceship : MonoBehaviourPun
{
    Rigidbody rb;
    public GameObject CameraGameObject;
    public GameObject bullet;
    public Vector3 rotationGoal;
    public float forwardThrustForce = 1.2f;
    public float backwardThrustForce = 1.2f;
    public float cameraMovementSpeed = 7f;
    public float shipRotationSpeed = 1f;
    public float maximumVelcoity = 10;
    public float timeBetweenShots = 500;
    public float health = 100;

    public bool thrustersOn = false;
    private bool first = true;
    public GameObject lockOnTarget;
    private float lastShootTime = 0;

    void Start()
    {
        rb = transform.GetComponent<Rigidbody>();
        CameraGameObject = GameObject.Find("MainCamera").gameObject;
    }
    private void FixedUpdate()
    {
        //user force to rotate the ship towards the cameras vector
        if (rotationGoal != null)
            transform.forward = Vector3.Lerp(transform.forward, rotationGoal, Time.deltaTime * shipRotationSpeed);

        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
            return;

        //CAMERA
        //put the camera behind the gameobject
        CameraGameObject.transform.position = Vector3.Lerp(CameraGameObject.transform.position, transform.position + (transform.up * 0.8f) + (-3 * CameraGameObject.transform.forward.normalized), Time.deltaTime * cameraMovementSpeed);
        rotationGoal = CameraGameObject.transform.forward;


    }
    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
            return;

        //MOVEMENT
        if (Input.GetKey(KeyCode.W))
        {
            ForwardThrust();
            thrustersOn = true;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            ReverseThrust();
            thrustersOn = false;
        }
        else
            thrustersOn = false;
        //limit velocity
        if (rb.velocity.magnitude > maximumVelcoity)
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maximumVelcoity);

        //SHOOTING
        //locking on 
        if (Input.GetKey(KeyCode.Mouse1))
        {
            RaycastHit hit;
            Ray ray = new Ray(CameraGameObject.transform.position, CameraGameObject.transform.forward);
            int layerMask = ~(1 << 9);
            if (Physics.Raycast(ray, out hit, 10000, layerMask, QueryTriggerInteraction.Collide))
            {
                if (hit.transform.gameObject.tag == "Ship")
                {
                    lockOnTarget = hit.transform.gameObject;
                }
            }
        }
        PhotonNetwork.FetchServerTimestamp();
        if (Input.GetKey(KeyCode.Mouse0) && PhotonNetwork.ServerTimestamp > lastShootTime + timeBetweenShots)
        {
            lastShootTime = PhotonNetwork.ServerTimestamp;
            PhotonView photonView = PhotonView.Get(this);
            RaycastHit hit;
            Ray ray = new Ray(CameraGameObject.transform.position, CameraGameObject.transform.forward);
            int layerMask = ~(1 << 9);
            if (Physics.Raycast(ray, out hit, 1000, layerMask, QueryTriggerInteraction.Collide))
                photonView.RPC("ShootGun", RpcTarget.All, hit.point - transform.position);
            else
                photonView.RPC("ShootGun", RpcTarget.All, transform.forward);
        }
    }
    public void ForwardThrust()
    {
        rb.AddForceAtPosition(transform.forward * forwardThrustForce, transform.position);
    }
    public void ReverseThrust()
    {
        rb.AddForceAtPosition(-1 * transform.forward * backwardThrustForce, transform.position);
    }
    [PunRPC]
    void ShootGun(Vector3 direction, PhotonMessageInfo info)
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
