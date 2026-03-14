using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicGuideTrigger : MonoBehaviour
{
    public TherapyUIManager UIManager;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.LogWarning("玩家触发");
            // 确保有引用并且不为空
            if (UIManager != null)
            {
                // 调用目标脚本中的公共函数
                Debug.LogWarning("函数即将被调用");
                UIManager.AddAIMessageAsync("欢迎来到音乐房间，你可以自由演奏，抒发心情。可以按右边墙上的按钮退出");   
                Debug.LogWarning("函数调用完毕");
            }
            else
            {
                Debug.LogWarning("目标脚本未在MediationGuideTrigger中分配！");
            }
        }
    }
}
