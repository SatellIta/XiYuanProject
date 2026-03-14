using System;
using UnityEngine;

[Obsolete("这个脚本已经废弃，完全可以使用NotificationTrigger来实现")]
public class BeginningTipTrigger : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private TherapyUIManager ui;


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ui.ShowNotification("请使用 WASD 键移动", 3f);
        }
    }
}