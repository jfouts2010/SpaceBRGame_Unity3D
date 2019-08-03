using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerControlls : MonoBehaviourPun
{
    public GameObject CameraGameObject;
    public Spaceship ss;
    public List<GameObject> Turrets = new List<GameObject>();
    public float cameraPositionY = .8f;
    public float cameraPositionZ = -3f;
    // Start is called before the first frame update
    void Start()
    {
        CameraGameObject = GameObject.Find("MainCamera").gameObject;
        ss = this.GetComponent<Spaceship>();

    }
    private void FixedUpdate()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
            return;

        //CAMERA
        //put the camera behind the gameobject
        CameraGameObject.transform.position = Vector3.Lerp(CameraGameObject.transform.position, transform.position + (transform.up * cameraPositionY) + (cameraPositionZ * CameraGameObject.transform.forward.normalized), Time.deltaTime * ss.cameraMovementSpeed);
    }
    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
            return;

        //MOVEMENT
        if (Input.GetKey(KeyCode.W))
        {
            ss.ForwardThrust(1f);
            ss.thrustersOn = true;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            ss.ReverseThrust();
            ss.thrustersOn = false;
        }
        else
            ss.thrustersOn = false;
        if (Input.GetKey(KeyCode.A))
        {
            ss.TurnThrust(1, false);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            ss.TurnThrust(1, true);
        }
        //turn z axis
        if (Input.GetKey(KeyCode.LeftShift))
        {
            ss.ZDirectionThrust(.5f, false);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            ss.ZDirectionThrust(.5f, true);
        }
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
                    ss.lockOnTarget = hit.transform.gameObject;
                }
            }
        }
        PhotonNetwork.FetchServerTimestamp();
        if (Input.GetKey(KeyCode.Mouse0) && PhotonNetwork.ServerTimestamp > ss.lastShootTime + ss.timeBetweenShots)
        {
            ss.lastShootTime = PhotonNetwork.ServerTimestamp;
            PhotonView photonView = PhotonView.Get(this);
            //where to shoot
            Vector3 cameraForwardVector = CameraGameObject.transform.forward;
            float distanceToTarget = 50; // default if no target
            if (ss.lockOnTarget != null)
                distanceToTarget = Vector3.Distance(CameraGameObject.transform.position, ss.lockOnTarget.transform.position);
            Vector3 shootTarget = cameraForwardVector.normalized * distanceToTarget + CameraGameObject.transform.position;
            ss.ShootAllTurrets(shootTarget);
           /* RaycastHit hit;
            Ray ray = new Ray(CameraGameObject.transform.position, CameraGameObject.transform.forward);
            int layerMask = ~(1 << 9);
            if (Physics.Raycast(ray, out hit, 1000, layerMask, QueryTriggerInteraction.Collide))
                ss.ShootAllTurrets(hit.point - transform.position);
            else
                ss.ShootAllTurrets(transform.forward);*/
        }
    }
}
