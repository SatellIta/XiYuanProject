using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MediationGuideTrigger : MonoBehaviour
{
    public TherapyUIManager UIManager;
    private AudioSource meditationAudioSource;
    private bool hasPlayed = false;

    private void Start()
    {
        meditationAudioSource = GetComponent<AudioSource>();
        meditationAudioSource.loop = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 检查进入的对象是否是玩家（通过Tag或Layer筛选更佳）
        if (other.CompareTag("Player"))
        {
            Debug.LogWarning("玩家触发");
            // 确保有引用并且不为空
            if (UIManager != null)
            {
                // 调用目标脚本中的公共函数
                Debug.LogWarning("函数即将被调用");
                UIManager.AddAIMessageAsync("欢迎来到冥想房间,请你跟着舒缓的音乐，关注呼吸，平静心灵吧。你可以按前方墙上的按钮退出");
                PlayMeditationMusic();
                hasPlayed = true;
                Debug.LogWarning("函数调用完毕");
            }
            else
            {
                Debug.LogWarning("目标脚本未在MediationGuideTrigger中分配！");
            }
        }
    }

    private void PlayMeditationMusic()
    {
        if (meditationAudioSource != null && (!hasPlayed))
        {
            meditationAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("未设置音乐文件或AudioSource组件");
        }
    }
}
