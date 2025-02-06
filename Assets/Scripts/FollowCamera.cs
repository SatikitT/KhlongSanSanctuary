using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;
    public float zoomSpeed = 2f; // Speed of zooming
    public float minZoom = 3f;   // Minimum zoom limit
    public float maxZoom = 10f;  // Maximum zoom limit

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        // Follow the player
        transform.position = new Vector3(offset.x + player.position.x, offset.y + player.position.y, offset.z);

        // Handle zooming with scroll wheel
        float scrollInput = Input.GetAxis("Mouse ScrollWheel"); // Get scroll input
        if (scrollInput != 0)
        {
            if (cam.orthographic) // 2D Camera
            {
                cam.orthographicSize -= scrollInput * zoomSpeed;
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
            }
            else // 3D Camera
            {
                cam.fieldOfView -= scrollInput * zoomSpeed * 10f;
                cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minZoom * 10f, maxZoom * 10f);
            }
        }
    }
}
