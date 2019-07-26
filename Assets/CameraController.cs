using UnityEngine;
using Photon.Pun;
/// <summary>
/// Adds a slight lag to camera rotation to make the third person camera a little more interesting.
/// Requires that it starts parented to something in order to follow it correctly.
/// </summary>
public class CameraController : MonoBehaviourPun
{
    GameObject cameraGameobject;
    Vector2 rotation = new Vector2(0, 0);
    public float speed = 3;

    public void Start()
    {
        cameraGameobject = GameObject.Find("MainCamera").gameObject;
    }
    void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }
        rotation.y += Input.GetAxis("Mouse X");
        rotation.x += -Input.GetAxis("Mouse Y");
        cameraGameobject.transform.eulerAngles = (Vector2)rotation * speed;
    }
}
