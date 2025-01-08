using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;

    void Update()
    {
        transform.position = new Vector3(offset.x + player.position.x, offset.y + player.position.y, offset.z);
    }
}
