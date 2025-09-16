using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBreathLight : MonoBehaviour
{
    [SerializeField] Light targetLight;
    [Range(0.2f, 1f)] public float speed = 0.7f;
    [Range(0.1f, 0.4f)] public float amount = 0.2f;

    private float baseIntensity;

    void Start() => baseIntensity = targetLight.intensity;

    void Update()
    {
        float flicker = Mathf.Sin(Time.time * speed) * amount;
        targetLight.intensity = baseIntensity + flicker;
    }
}
