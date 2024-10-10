using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TornadoSpin : MonoBehaviour
{
    float RotationSpeed = 360.0f;

    // Update is called once per frame
    void Update()
    {
        transform.rotation *= Quaternion.AngleAxis(RotationSpeed * Time.deltaTime, Vector3.down);
    }
}
