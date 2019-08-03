using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Weapons : MonoBehaviourPun
{
    public string name;
    public WeaponSize Size;
    public float ReloadTime;
    public float BulletVelocity = 20;
    public GameObject WeaponPrefab;
    public GameObject BulletPrefab;

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


