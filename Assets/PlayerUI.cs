using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    Rigidbody rb;
    Spaceship ss;
    GameObject speedTextGameObject;
    GameObject healthTextGameObject;
    private void Start()
    {
        rb = this.transform.parent.GetComponent<Rigidbody>();
        ss = this.transform.parent.GetComponent<Spaceship>();
        speedTextGameObject = transform.Find("Speed").gameObject;
        healthTextGameObject = transform.Find("Health").gameObject;
    }
    // Update is called once per frame
    void Update()
    {
        speedTextGameObject.GetComponent<TextMeshProUGUI>().text = rb.velocity.magnitude.ToString("0.##");
        healthTextGameObject.GetComponent<TextMeshProUGUI>().text = ss.health.ToString();
    }
}
