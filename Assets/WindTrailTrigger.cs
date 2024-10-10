using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindTrailTrigger : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        PlayerRig PlayerRigRef = collision.collider.gameObject.GetComponent<PlayerRig>();

        if (PlayerRigRef != null)
        {
            //If Sails Up, Lower Max Speed, otherwise do nothing.
        }
    }
}
