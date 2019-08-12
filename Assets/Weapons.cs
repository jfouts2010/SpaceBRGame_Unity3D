using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Weapons : MonoBehaviourPun
{
    public WeaponName name;
    public float heatPerShot = 10;
    public float bulletDamage = 10;
    public WeaponSize Size;
    public float ReloadTime;
    public float BulletVelocity = 20;
    public GameObject WeaponPrefab;
    public GameObject BulletPrefab;
    public float inAccuracy;

    private void Start()
    {
        Instantiate(WeaponPrefab, transform);
    }
}
public enum WeaponSize
{
    Small,
    Medium,
    Large
}
public enum WeaponName
{
    IonBlaster,
    PointDefense
}


