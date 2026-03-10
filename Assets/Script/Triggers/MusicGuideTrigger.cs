using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicGuideTrigger : MonoBehaviour
{
    public AIChatManager aiChatManager;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.LogWarning("玩家触发");
            // 确保有引用并且不为空
            if (aiChatManager != null)
            {
                // 调用目标脚本中的公共函数
                Debug.LogWarning("函数即将被调用");
                aiChatManager.TypeSubtitle("欢迎来到音乐房间，你可以自由演奏，抒发心情", null);   
                Debug.LogWarning("函数调用完毕");
            }
            else
            {
                Debug.LogWarning("目标脚本未在MediationGuideTrigger中分配！");
            }
        }
    }
}
