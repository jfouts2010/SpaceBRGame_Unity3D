
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class Spaceship : MonoBehaviourPun
{
    Rigidbody rb;
    public GameObject CameraGameObject;
    public GameObject explosionPrefab;
    public GameObject Missle; //TEMP
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
    public Dictionary<SpaceshipSystem, float> SystemHealth = new Dictionary<SpaceshipSystem, float>();
    public Dictionary<SpaceshipSystem, float> SystemHealthMax = new Dictionary<SpaceshipSystem, float>();
    public Dictionary<SpaceshipSystem, float> SystemHealthFixedHealth = new Dictionary<SpaceshipSystem, float>();
    public Dictionary<SpaceshipSystem, bool> SystemActive = new Dictionary<SpaceshipSystem, bool>();
    public float hullHealth = 0;
    public float hullHealAmount = 5;
    public bool thrustersOn = false;
    private bool first = true;
    public GameObject lockOnTarget;
    public float lastShootTime = 0;
    private GameObject ventSteam;
    private GameObject OverheatFlames;
    public float SpaceshipTick = 500;
    public float lastSpaceshipTick = 0;

    //heat
    public float heat = 0;
    public float MaxHeat = 100;
    public float idleHeatDisipationPerTick = 2;
    public bool overheat = false;
    public bool venting = false;

    //EnergyShield
    public float energyshieldMax = 100;
    public float energyshield = 100;
    public float energyshieldRechargePerTick = 2;

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
        SystemHealth.Add(SpaceshipSystem.Engine, 100);
        SystemHealth.Add(SpaceshipSystem.Weapons, 100);
        SystemHealth.Add(SpaceshipSystem.Shields, 100);
        SystemHealth.Add(SpaceshipSystem.Hull, 500);

        SystemHealthMax.Add(SpaceshipSystem.Engine, 100);
        SystemHealthMax.Add(SpaceshipSystem.Weapons, 100);
        SystemHealthMax.Add(SpaceshipSystem.Shields, 100);
        SystemHealthMax.Add(SpaceshipSystem.Hull, 500);

        SystemHealthFixedHealth.Add(SpaceshipSystem.Engine, 25);
        SystemHealthFixedHealth.Add(SpaceshipSystem.Weapons, 25);
        SystemHealthFixedHealth.Add(SpaceshipSystem.Hull, 0);
        SystemHealthFixedHealth.Add(SpaceshipSystem.Shields, 100);

        SystemActive.Add(SpaceshipSystem.Engine, true);
        SystemActive.Add(SpaceshipSystem.Weapons, true);
        SystemActive.Add(SpaceshipSystem.Hull, true);
        SystemActive.Add(SpaceshipSystem.Shields, true);
    }
    private void FixedUpdate()
    {
        hullHealth = SystemHealth[SpaceshipSystem.Hull];

        if (this.transform.localEulerAngles.x != 0 || this.transform.localEulerAngles.z != 0)
        {
            this.transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
        }
    }
    // Update is called once per frame
    void Update()
    {
        //limit velocity
        if (rb.velocity.magnitude > maximumVelcoity)
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maximumVelcoity);
        //limit turn speed
        if (rb.angularVelocity.magnitude > 1)
            rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, 1);

        //turn off engines if engine systems died
        if (!SystemActive[SpaceshipSystem.Engine])
            systemThrustMultiplier = 0;
        else
            systemThrustMultiplier = 1;

        //turn of shields if not active
        if (energyshield <= 0)
            SystemActive[SpaceshipSystem.Shields] = false;
        if (SystemActive[SpaceshipSystem.Shields] == false)
            transform.Find("EnergyShield").gameObject.SetActive(false);
        else
            transform.Find("EnergyShield").gameObject.SetActive(true);

        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
            return;
        PhotonNetwork.FetchServerTimestamp();
        //Spaceship health/heat updates
        if (PhotonNetwork.ServerTimestamp > lastSpaceshipTick + SpaceshipTick)
        {
            lastSpaceshipTick = PhotonNetwork.ServerTimestamp;

            //health
            foreach (SpaceshipSystem sys in Enum.GetValues(typeof(SpaceshipSystem)))
            {

                if (SystemHealth[sys] < SystemHealthMax[sys] && supplies >= hullHealAmount)
                {
                    SystemHealth[sys] += hullHealAmount;
                    supplies -= hullHealAmount;
                }
                if (SystemActive[sys] == false && SystemHealth[sys] >= SystemHealthFixedHealth[sys] && sys != SpaceshipSystem.Shields)
                    SystemActive[sys] = true;
            }
            //heat
            if (venting)
            {
                ventSteam.SetActive(true);
                foreach (SpaceshipSystem sys in Enum.GetValues(typeof(SpaceshipSystem)))
                    SystemActive[sys] = false;
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
                SystemActive[sys] = false;
        }
        else
            OverheatFlames.SetActive(false);


        energyshield = Mathf.Clamp(energyshield, 0, energyshieldMax);
        heat = Mathf.Clamp(heat, 0, MaxHeat);
    }
    public void ForwardThrust(float percent)
    {
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
        photonView.RPC("ShieldsSwitchRPC", RpcTarget.All);
    }
    [PunRPC]
    void ShieldsSwitchRPC(PhotonMessageInfo info)
    {
        SystemActive[SpaceshipSystem.Shields] = !SystemActive[SpaceshipSystem.Shields];
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
            if (SystemActive[(SpaceshipSystem)system])
            {
                SystemActive[(SpaceshipSystem)system] = false;
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
        SystemHealth[SpaceshipSystem.Hull] -= bulletDamage;
        if (SystemHealth[SpaceshipSystem.Hull] <= 0)
            SystemDestroyed(SpaceshipSystem.Hull);


        if (SystemHealth[(SpaceshipSystem)sys] > 0)
        {
            SystemHealth[(SpaceshipSystem)sys] -= bulletDamage;
            if (SystemHealth[(SpaceshipSystem)sys] <= 0)
                SystemDestroyed((SpaceshipSystem)sys);
        }
    }
    public void ShootAllRockets()
    {
        if (SystemActive[SpaceshipSystem.Weapons])
            photonView.RPC("ShootAllRocketsRPC", RpcTarget.All);
    }
    [PunRPC]
    void ShootAllRocketsRPC(PhotonMessageInfo info)
    {
        PhotonNetwork.FetchServerTimestamp();
        if (PhotonNetwork.ServerTimestamp > lastShootTime + timeBetweenShots)
        {
            lastShootTime = PhotonNetwork.ServerTimestamp;
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
        PhotonNetwork.FetchServerTimestamp();
        if (Time.time > lastShootTime + wep.ReloadTime)
        {
            lastShootTime = Time.time;
            if (SystemActive[SpaceshipSystem.Weapons])
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(this.SystemHealth);
            stream.SendNext(this.heat);
            stream.SendNext(this.supplies);
        }
        else
        {
            // Network player, receive data
            this.heat = (float)stream.ReceiveNext();
            this.SystemHealth = (Dictionary<SpaceshipSystem, float>)stream.ReceiveNext();
            this.supplies = (float)stream.ReceiveNext();
        }
    }
}
