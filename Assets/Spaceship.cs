
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Spaceship : MonoBehaviourPun
{
    Rigidbody rb;
    public GameObject CameraGameObject;
    public GameObject bullet;

    public List<GameObject> Turrets = new List<GameObject>();
    public List<GameObject> Systems = new List<GameObject>();
    public float forwardThrustForce = 1.2f;
    public float backwardThrustForce = 1.2f;
    public float rotationForce = 1.2f;
    public float cameraMovementSpeed = 7f;
    public float shipRotationSpeed = 3f;
    public float maximumVelcoity = 10;
    public float timeBetweenShots = 500;
    private float forwardThrustMultiplyer = 1;
    private float systemThrustMultiplier = 1;
    public Dictionary<SpaceshipSystem, int> SystemHealth = new Dictionary<SpaceshipSystem, int>();
    public float hullHealth = 0;
    public bool thrustersOn = false;
    private bool first = true;
    public GameObject lockOnTarget;
    public float lastShootTime = 0;

    void Start()
    {
        rb = transform.GetComponent<Rigidbody>();
        CameraGameObject = GameObject.Find("MainCamera").gameObject;
        //get all the turrets for this ship
        GameObject goTurrets = transform.Find("Turrets").gameObject;
        for (int i = 0; i < goTurrets.transform.childCount; i++)
        {
            Turrets.Add(goTurrets.transform.GetChild(i).gameObject);
        }
        GameObject goSystems = transform.Find("Systems").gameObject;
        for (int i = 0; i < goSystems.transform.childCount; i++)
        {
            Turrets.Add(goSystems.transform.GetChild(i).gameObject);
        }
        //set system health
        SystemHealth.Add(SpaceshipSystem.Engine, 100);
        SystemHealth.Add(SpaceshipSystem.Weapons, 100);
        SystemHealth.Add(SpaceshipSystem.Hull, 500);
    }
    private void FixedUpdate()
    {
        hullHealth = SystemHealth[SpaceshipSystem.Hull];
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

        //turn off engines if engine systems died
        if (SystemHealth[SpaceshipSystem.Engine] <= 0)
            systemThrustMultiplier = 0;
        else
            systemThrustMultiplier = 1;
    }
    public void ForwardThrust(float percent)
    {
        rb.AddForceAtPosition(transform.forward * forwardThrustForce * Mathf.Clamp(percent,0,1) * systemThrustMultiplier, transform.position);
    }
    public void ReverseThrust()
    {
        rb.AddForceAtPosition(-1 * transform.forward * backwardThrustForce * systemThrustMultiplier, transform.position);
    }
    public void TurnThrust(float percent, bool clockwise)
    {
        rb.AddTorque(Vector3.up * percent * (clockwise ? 1 : -1) * systemThrustMultiplier);
    }
    public void ZDirectionThrust(float percent, bool up)
    {
        rb.AddForce(Vector3.up * (up ? 1 : -1) * systemThrustMultiplier);
    }
    public void SystemDestroyed(SpaceshipSystem system)
    {

    }
    public void ShootAllTurrets(Vector3 target)
    {
        foreach(GameObject turret in Turrets)
        {
            Vector3 turretTarget = target - turret.transform.position;
            Weapons wep = turret.GetComponent<Weapons>();
            GameObject newBullet = PhotonNetwork.Instantiate(wep.BulletPrefab.name, turret.transform.position, Quaternion.identity, 0);
            newBullet.GetComponent<Bullet>().owner = GetComponent<PhotonView>().Owner;
            newBullet.transform.forward = turretTarget.normalized;
            newBullet.GetComponent<Rigidbody>().velocity = (turretTarget.normalized * wep.BulletVelocity);// + rb.velocity);
            GameObject.Destroy(newBullet, 10);
        }
    }
    public void ShootGun(Vector3 target)
    {
        photonView.RPC("ShootGunRPC", RpcTarget.All, target);
    }
    [PunRPC]
    void ShootGunRPC(Vector3 direction, PhotonMessageInfo info)
    {
        GameObject newBullet = GameObject.Instantiate(bullet, transform.position, Quaternion.Euler(direction));
        newBullet.GetComponent<Bullet>().owner = GetComponent<PhotonView>().Owner;
        newBullet.transform.forward = direction.normalized;
        newBullet.GetComponent<Rigidbody>().velocity = (direction.normalized * 20);// + rb.velocity);
        GameObject.Destroy(newBullet, 10);
    }

    #region IPunObservable implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(this.lastShootTime);
            stream.SendNext(this.SystemHealth);
        }
        else
        {
            // Network player, receive data
            this.lastShootTime = (float)stream.ReceiveNext();
            this.SystemHealth = (Dictionary<SpaceshipSystem, int>)stream.ReceiveNext();
        }
    }

    #endregion
}
