using UnityEngine;

/// <summary>
/// Adds a slight lag to camera rotation to make the third person camera a little more interesting.
/// Requires that it starts parented to something in order to follow it correctly.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    Vector2 rotation = new Vector2(0, 0);
    public float speed = 3;

    void Update()
    {
        rotation.y += Input.GetAxis("Mouse X");
        rotation.x += -Input.GetAxis("Mouse Y");
        transform.eulerAngles = (Vector2)rotation * speed;
    }
}
