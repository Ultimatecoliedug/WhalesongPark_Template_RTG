using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindTrailMaster : MonoBehaviour
{
    [SerializeField]
    float WindPercent;

    [SerializeField]
    float ResetTimeMax = 4.0f;

    [SerializeField]
    float ResetTimeMin = 2.0f;

    bool OnReset = false;

    [SerializeField]
    public float WindSpeed = 1.0f;

    float ResetTime = 2.0f;

    void SetResetTime()
    {
        ResetTime = Random.Range(ResetTimeMin, ResetTimeMax);
    }

    // Update is called once per frame
    void Update()
    {
        WindPercent += WindSpeed * Time.deltaTime;

        if (OnReset)
        {
            if (WindPercent > ResetTime)
            {
                WindPercent = 0.0f;
                OnReset = false;
                SetResetTime();
            }
        }
        else if (WindPercent > 1.5f)
        {
            Shader.SetGlobalFloat("_WindPercent", 0.0f);
            OnReset = true;
        }
        else
        {
            Shader.SetGlobalFloat("_WindPercent", WindPercent);
        }

    }
}
