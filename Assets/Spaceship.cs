
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class Spaceship : MonoBehaviourPun, IPunObservable
{
    [HideInInspector]
    Rigidbody rb;
    [HideInInspector]
    public GameObject CameraGameObject;
    [HideInInspector]
    public GameObject explosionPrefab;
    [HideInInspector]
    public GameObject Missle; //TEMP
    [HideInInspector]
    public List<GameObject> Turrets = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> Systems = new List<GameObject>();
    public float forwardThrustForce = 1.2f;
    public float backwardThrustForce = 1.2f;
    private float rotationForce = 1.2f;
    public float cameraMovementSpeed = 7f;
    private float shipRotationSpeed = 3f;
    private float defaultMaximumVelcoity = 2.5f;
    public float currentMaximumVelocity;
    public bool boosting = false;

    private float forwardThrustMultiplyer = 1;
    private float systemThrustMultiplier = 1;
    public Dictionary<int, float> SystemHealth = new Dictionary<int, float>();
    public Dictionary<int, float> SystemHealthMax = new Dictionary<int, float>();
    public Dictionary<int, float> SystemHealthFixedHealth = new Dictionary<int, float>();
    public Dictionary<int, bool> SystemActive = new Dictionary<int, bool>();
    public float hullHealth = 0;
    private float hullHealAmount = 5;
    private bool first = true;
    public GameObject lockOnTarget;
    public float lastShootTime = 0;
    private GameObject ventSteam;
    private GameObject OverheatFlames;
    public float SpaceshipTick = .5f;
    public float lastSpaceshipTick = 0;
    public float hitMarkerStartTime;
    //heat
    public float heat = 0;
    public float MaxHeat = 100;
    private float idleHeatDisipationPerTick = 2;
    public bool overheat = false;
    public bool venting = false;

    //system active for sending on photon
    public bool engineActive = true;
    public bool weaponsActive = true;
    public bool shieldsActive = true;

    //EnergyShield
    public float energyshieldMax = 100;
    public float energyshield = 100;
    private float energyshieldRechargePerTick = 2;

    //supplies
    public float supplies = 100;

    //weapons to switch
    public List<WeaponName> weapons = new List<WeaponName>();
    public int currentWeapon;

    public void Start()
    {
        ventSteam = transform.Find("VentSteam").gameObject;
        ventSteam.SetActive(false);
        OverheatFlames = transform.Find("OverheatFlames").gameObject;
        OverheatFlames.SetActive(false);
        rb = transform.GetComponent<Rigidbody>();
        CameraGameObject = GameObject.Find("MainCamera").gameObject;
        //get all the turrets for this ship
        GameObject goTurrets = transform.Find("Turrets").gameObject;
        for (int i = 0; i < goTurrets.transform.childCount; i++)
        {
            Turrets.Add(goTurrets.transform.GetChild(i).gameObject);
            WeaponName name = goTurrets.transform.GetChild(i).GetComponent<Weapons>().name;
            if (!weapons.Contains(name))
                weapons.Add(name);
        }
        currentWeapon = 0;
        GameObject goSystems = transform.Find("Systems").gameObject;
        for (int i = 0; i < goSystems.transform.childCount; i++)
        {
            Systems.Add(goSystems.transform.GetChild(i).gameObject);
        }

        //set system health
        SystemHealth.Add((int)SpaceshipSystem.Engine, 100);
        SystemHealth.Add((int)SpaceshipSystem.Weapons, 100);
        SystemHealth.Add((int)SpaceshipSystem.Shields, 100);
        SystemHealth.Add((int)SpaceshipSystem.Hull, 500);

        SystemHealthMax.Add((int)SpaceshipSystem.Engine, 100);
        SystemHealthMax.Add((int)SpaceshipSystem.Weapons, 100);
        SystemHealthMax.Add((int)SpaceshipSystem.Shields, 100);
        SystemHealthMax.Add((int)SpaceshipSystem.Hull, 500);

        SystemHealthFixedHealth.Add((int)SpaceshipSystem.Engine, 25);
        SystemHealthFixedHealth.Add((int)SpaceshipSystem.Weapons, 25);
        SystemHealthFixedHealth.Add((int)SpaceshipSystem.Hull, 0);
        SystemHealthFixedHealth.Add((int)SpaceshipSystem.Shields, 100);

        SystemActive.Add((int)SpaceshipSystem.Engine, true);
        SystemActive.Add((int)SpaceshipSystem.Weapons, true);
        SystemActive.Add((int)SpaceshipSystem.Hull, true);
        SystemActive.Add((int)SpaceshipSystem.Shields, true);
    }
    private void FixedUpdate()
    {
        hullHealth = SystemHealth[(int)SpaceshipSystem.Hull];

        if (this.transform.localEulerAngles.x != 0 || this.transform.localEulerAngles.z != 0)
        {
            this.transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
        }
    }
    // Update is called once per frame
    void Update()
    {
        currentMaximumVelocity = (boosting ? 2 : 1) * defaultMaximumVelcoity;

        //limit turn speed
        if (rb.angularVelocity.magnitude > 1)
            rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, 1);

        //turn off engines if engine systems died
        if (!SystemActive[(int)SpaceshipSystem.Engine])
            systemThrustMultiplier = 0;
        else
            systemThrustMultiplier = 1;

        //turn of shields if not active
        if (SystemActive[(int)SpaceshipSystem.Shields] == false)
            transform.Find("EnergyShield").gameObject.SetActive(false);
        else
            transform.Find("EnergyShield").gameObject.SetActive(true);

        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
            return;
        if (energyshield <= 0)
            SystemActive[(int)SpaceshipSystem.Shields] = false;
        //Spaceship health/heat updates
        if (Time.time > lastSpaceshipTick + SpaceshipTick)
        {
            lastSpaceshipTick = Time.time;

            //health
            foreach (SpaceshipSystem sys in Enum.GetValues(typeof(SpaceshipSystem)))
            {

                if (SystemHealth[(int)sys] < SystemHealthMax[(int)sys] && supplies >= hullHealAmount)
                {
                    SystemHealth[(int)sys] += hullHealAmount;
                    supplies -= hullHealAmount;
                }
                if (SystemActive[(int)sys] == false && SystemHealth[(int)sys] >= SystemHealthFixedHealth[(int)sys] && sys != SpaceshipSystem.Shields)
                    SystemActive[(int)sys] = true;
            }
            //heat
            if (venting)
            {
                ventSteam.SetActive(true);
                foreach (SpaceshipSystem sys in Enum.GetValues(typeof(SpaceshipSystem)))
                    SystemActive[(int)sys] = false;
                heat -= idleHeatDisipationPerTick * 3f;
                if (heat < 0)
                    venting = false;
            }
            else
            {
                ventSteam.SetActive(false);
                if (heat > 0)
                    heat -= idleHeatDisipationPerTick;
            }
            if (boosting)
                heat += 4;

            //energyshield
            if (energyshield < energyshieldMax)
                energyshield += energyshieldRechargePerTick;

        }
        if (heat >= MaxHeat)
            overheat = true;
        if (heat < MaxHeat / 2 && overheat)
            overheat = false;

        if (overheat)
        {
            OverheatFlames.gameObject.SetActive(true);
            foreach (SpaceshipSystem sys in Enum.GetValues(typeof(SpaceshipSystem)))
                SystemActive[(int)sys] = false;
        }
        else
            OverheatFlames.SetActive(false);


        energyshield = Mathf.Clamp(energyshield, 0, energyshieldMax);
        heat = Mathf.Clamp(heat, 0, MaxHeat);
    }
    public void ForwardThrust(float percent)
    {
        //limit velocity
        if (rb.velocity.magnitude < currentMaximumVelocity)
            rb.AddForceAtPosition(transform.forward * forwardThrustForce * Mathf.Clamp(percent, 0, 1) * systemThrustMultiplier, transform.position);
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
    public void ShieldsSwitch()
    {
        //SystemActive[SpaceshipSystem.Shields] = !SystemActive[SpaceshipSystem.Shields];
        //shieldsActive = !shieldsActive;
        photonView.RPC("ShieldsSwitchRPC", RpcTarget.All);
    }
    [PunRPC]
    void ShieldsSwitchRPC(PhotonMessageInfo info)
    {
        SystemActive[(int)SpaceshipSystem.Shields] = !SystemActive[(int)SpaceshipSystem.Shields];
    }
    public void SystemDestroyed(SpaceshipSystem system)
    {
        photonView.RPC("SystemDestroyedRPC", RpcTarget.All, (int)system);
    }

    [PunRPC]
    void SystemDestroyedRPC(int system, PhotonMessageInfo info)
    {
        if ((SpaceshipSystem)system == SpaceshipSystem.Hull)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 3);
            Destroy(this.gameObject);
        }
        else
        {
            if (SystemActive[(int)system])
            {
                SystemActive[(int)system] = false;
                foreach (GameObject go in Systems)
                {
                    if (go.GetComponent<SystemTag>().system == (SpaceshipSystem)system)
                    {
                        GameObject explosion = Instantiate(explosionPrefab, go.transform.position, Quaternion.identity);
                        Destroy(explosion, 3);
                        break;
                    }
                }
            }
        }
    }
    public void TakeSystemDamageRPC(SpaceshipSystem sys, float bulletDamage)
    {
        photonView.RPC("TakeSystemDamage", RpcTarget.All, (int)sys, bulletDamage);
    }
    [PunRPC]
    void TakeSystemDamage(int sys, float bulletDamage, PhotonMessageInfo info)
    {
        SystemHealth[(int)SpaceshipSystem.Hull] -= bulletDamage;
        if (SystemHealth[(int)SpaceshipSystem.Hull] <= 0)
            SystemDestroyed(SpaceshipSystem.Hull);


        if (SystemHealth[(int)sys] > 0)
        {
            SystemHealth[(int)sys] -= bulletDamage;
            if (SystemHealth[(int)sys] <= 0)
                SystemDestroyed((SpaceshipSystem)sys);
        }
    }
    public void ShootAllRockets()
    {
        if (SystemActive[(int)SpaceshipSystem.Weapons])
            photonView.RPC("ShootAllRocketsRPC", RpcTarget.All);
    }
    [PunRPC]
    void ShootAllRocketsRPC(PhotonMessageInfo info)
    {
        if (PhotonNetwork.ServerTimestamp > lastShootTime + 1)
        {
            lastShootTime = Time.time;
            GameObject newMissle = Instantiate(Missle, transform.position, Quaternion.identity);
            newMissle.transform.forward = this.transform.up;
            newMissle.GetComponent<Rigidbody>().velocity = (this.transform.up * 20);
            newMissle.GetComponent<Missle>().Target = lockOnTarget;
        }
    }
    public void ShootAllTurrets(Vector3 target, WeaponName name)
    {
        GameObject go = Resources.Load(name.ToString()) as GameObject;
        Weapons wep = go.GetComponent<Weapons>();
        if (Time.time > lastShootTime + wep.ReloadTime)
        {
            lastShootTime = Time.time;
            if (SystemActive[(int)SpaceshipSystem.Weapons])
                photonView.RPC("ShootAllTurretsRPC", RpcTarget.All, target, (int)name);
        }
    }
    [PunRPC]
    void ShootAllTurretsRPC(Vector3 target, int weapon, PhotonMessageInfo info)
    {
        foreach (GameObject turret in Turrets)
        {
            Weapons wep = turret.GetComponent<Weapons>();
            if (wep.name != (WeaponName)weapon)
                continue;
            Vector3 turretTarget = target - turret.transform.position;
            GameObject newBullet = Instantiate(wep.BulletPrefab, turret.transform.position, Quaternion.identity);
            newBullet.GetComponent<Bullet>().ownerGameObject = this.gameObject;
            newBullet.GetComponent<Bullet>().owner = GetComponent<PhotonView>().Owner;
            Vector3 bulletTraj = turretTarget.normalized + new Vector3(UnityEngine.Random.Range(-wep.inAccuracy, wep.inAccuracy), UnityEngine.Random.Range(-wep.inAccuracy, wep.inAccuracy), UnityEngine.Random.Range(-wep.inAccuracy, wep.inAccuracy));
            newBullet.transform.forward = bulletTraj.normalized;
            newBullet.GetComponent<Rigidbody>().velocity = (bulletTraj.normalized * wep.BulletVelocity);// + rb.velocity);
            newBullet.GetComponent<Bullet>().bulletDamage = wep.bulletDamage;
            GameObject.Destroy(newBullet, 10);

            heat += wep.heatPerShot;
        }
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //update photon Bools
            /*  engineActive = SystemActive[SpaceshipSystem.Engine];
              weaponsActive = SystemActive[SpaceshipSystem.Weapons];
              shieldsActive = SystemActive[SpaceshipSystem.Shields];*/

            // We own this player: send the others our data
            stream.SendNext(this.SystemHealth);
            stream.SendNext(this.SystemActive);
            stream.SendNext(this.heat);
            stream.SendNext(this.supplies);
            stream.SendNext(this.boosting);
            //stream.SendNext(this.shieldsActive);
            //stream.SendNext(this.hitMarkerStartTime);
        }
        else
        {
            // Network player, receive data
            this.SystemHealth = (Dictionary<int, float>)stream.ReceiveNext();
            this.SystemActive = (Dictionary<int, bool>)stream.ReceiveNext();
            this.heat = (float)stream.ReceiveNext();
            //this.shieldsActive = (bool)stream.ReceiveNext();
            this.supplies = (float)stream.ReceiveNext();
            this.boosting = (bool)stream.ReceiveNext();
            //this.hitMarkerStartTime = (float)stream.ReceiveNext();

            //update photon Bools
            /*  SystemActive[SpaceshipSystem.Engine] = engineActive;
              SystemActive[SpaceshipSystem.Weapons] = weaponsActive;
              SystemActive[SpaceshipSystem.Shields] = shieldsActive;*/


        }
    }
}
