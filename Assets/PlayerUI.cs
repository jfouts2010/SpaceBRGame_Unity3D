using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    Rigidbody rb;
    Spaceship ss;
    Camera mainCamera;
    GameObject speedTextGameObject;
    GameObject healthTextGameObject;
    GameObject targetting;
    GameObject hitMarker;
    GameObject supplies;
    GameObject energyShield;
    GameObject heat;

    float hitMarkerDuration = .3f;
    private void Start()
    {
        heat = transform.Find("Heat").gameObject;
        energyShield = transform.Find("EnergyShield").gameObject;
        supplies = transform.Find("Supplies").gameObject;
        hitMarker = transform.Find("HitMarker").gameObject;
        mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        rb = this.transform.parent.GetComponent<Rigidbody>();
        ss = this.transform.parent.GetComponent<Spaceship>();
        speedTextGameObject = transform.Find("Speed").gameObject;
        healthTextGameObject = transform.Find("Health").gameObject;
        targetting = transform.Find("Target").gameObject;
    }
    // Update is called once per frame
    private void FixedUpdate()
    {
        if (Time.time < ss.hitMarkerStartTime + hitMarkerDuration)
            hitMarker.SetActive(true);
        else
            hitMarker.SetActive(false);

        float minUISize = 100;
        speedTextGameObject.GetComponent<TextMeshProUGUI>().text = rb.velocity.magnitude.ToString("0.##");
        supplies.GetComponent<TextMeshProUGUI>().text = ss.supplies.ToString();
        //ENERGY SHIELD
        energyShield.transform.Find("Image").GetComponent<Image>().fillAmount = ss.energyshield / ss.energyshieldMax;
        energyShield.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = (ss.energyshield / ss.energyshieldMax * 100).ToString();
        //HEAT
        heat.transform.Find("Image").GetComponent<Image>().fillAmount = ss.heat / ss.MaxHeat;
        heat.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = (ss.heat / ss.MaxHeat * 100).ToString();

        string systemsHealth = "";
        foreach(var sss in ss.SystemHealth)
        {
            systemsHealth += sss.Key + ": " + sss.Value + "\r\n";
        }
        healthTextGameObject.GetComponent<TextMeshProUGUI>().text = systemsHealth;//ss.SystemHealth[SpaceshipSystem.Hull].ToString();
        if (ss.lockOnTarget != null)
        {
            Vector3 vectorPosition = mainCamera.WorldToScreenPoint(ss.lockOnTarget.transform.Find("SpaceShipMesh").transform.position);
            float z = vectorPosition.z;
            Rect r = GUIRectWithObject(ss.lockOnTarget);
            //limit ui
            //if its behind the player, move it to closest side
            if (vectorPosition.x >= Screen.width)
            {
                vectorPosition.x = Screen.width;
                r.width = minUISize;
                r.height = minUISize;
            }
            if (vectorPosition.x <= 0)
            {
                vectorPosition.x = 0;
                r.width = minUISize;
                r.height = minUISize;
            }
            if (vectorPosition.y >= Screen.height)
            {
                vectorPosition.y = Screen.height;
                r.width = minUISize;
                r.height = minUISize;
            }
            if (vectorPosition.y <= 0)
            {
                vectorPosition.y = 0;
                r.width = minUISize;
                r.height = minUISize;
            }
            r.width = Mathf.Max(minUISize, r.width);
            r.height = Mathf.Max(minUISize, r.height);
            if (vectorPosition.z < 0)
            {
                if (vectorPosition.y >= vectorPosition.x)
                {
                    if (vectorPosition.x > Screen.width / 2)
                    {
                        vectorPosition = new Vector3(0, vectorPosition.y, vectorPosition.z);
                    }
                    else
                    {
                        vectorPosition = new Vector3(Screen.width, vectorPosition.y, vectorPosition.z);
                    }
                }
                if (vectorPosition.x >= vectorPosition.y)
                {
                    if (vectorPosition.y > Screen.height / 2)
                    {
                        vectorPosition = new Vector3(vectorPosition.x, 0, vectorPosition.z);
                    }
                    else
                    {
                        vectorPosition = new Vector3(vectorPosition.x, Screen.height, vectorPosition.z);
                    }
                }
            }
            targetting.SetActive(true);
            targetting.transform.position = vectorPosition;
            targetting.GetComponent<RawImage>().rectTransform.sizeDelta = new Vector3(r.width, r.width);
        }
        else
        {
            targetting.SetActive(false);
        }
    }

    public Rect GUIRectWithObject(GameObject go)
    {
        Vector3 cen = go.transform.Find("SpaceShipMesh").GetComponent<Renderer>().bounds.center;
        Vector3 ext = go.transform.Find("SpaceShipMesh").GetComponent<Renderer>().bounds.extents;
        Vector2[] extentPoints = new Vector2[8]
         {
               WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
               WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
               WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
               WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),
               WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
               WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z)),
               WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
               WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z))
         };
        Vector2 min = extentPoints[0];
        Vector2 max = extentPoints[0];
        foreach (Vector2 v in extentPoints)
        {
            min = Vector2.Min(min, v);
            max = Vector2.Max(max, v);
        }
        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }

    public Vector2 WorldToGUIPoint(Vector3 world)
    {
        Vector2 screenPoint = mainCamera.WorldToScreenPoint(world);
        screenPoint.y = (float)Screen.height - screenPoint.y;
        return screenPoint;
    }
}
