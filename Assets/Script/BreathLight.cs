using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreathLight : MonoBehaviour
{
    [SerializeField] Light targetLight;
    [Range(0.1f, 3f)] public float breathSpeed = 0.2f;  // 呼吸频率
    [Range(0.02f, 0.2f)] public float breathAmount = 0.15f; // 亮度变化幅度

    private float baseIntensity;

    void Start()
    {
        if (targetLight == null) targetLight = GetComponent<Light>();
        baseIntensity = targetLight.intensity;
    }

    void Update()
    {
        float flicker = Mathf.Sin(Time.time * breathSpeed) * breathAmount;
        targetLight.intensity = baseIntensity + flicker;
    }
}
