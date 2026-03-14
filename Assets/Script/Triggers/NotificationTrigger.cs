using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationTrigger : MonoBehaviour
{
    [Header("提示文本")]
    [TextArea(2, 4)]
    [SerializeField] string warningText = "这是一个提示文本触发器，进入后会显示这段文本。";

    [Header("提示持续时间")]
    [SerializeField] float warningDuration = 3f;

    [Header("是否只能触发一次")]
    [SerializeField] bool triggerOnce = true;

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && (!triggerOnce || !hasTriggered))
        {
            TherapyUIManager.Instance.ShowNotification(warningText, warningDuration);
            hasTriggered = true;
        }
    }
}
