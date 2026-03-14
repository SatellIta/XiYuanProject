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
                if (!hasPlayed) 
                {
                    UIManager.ShowNotification("欢迎来到冥想房间,请你跟着舒缓的音乐，关注呼吸，平静心灵吧。你可以按前方墙上的按钮退出", 5f);
                }
                else
                {
                    UIManager.ShowNotification("欢迎回来，继续享受冥想吧！", 3f);
                }
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

    // 离开房间停止播放音乐
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (meditationAudioSource != null && meditationAudioSource.isPlaying)
            {
                UIManager.ShowNotification("你可以随时回来继续冥想！", 3f);
                meditationAudioSource.Pause();
            }
        }
    }

    private void PlayMeditationMusic()
    {
        if (meditationAudioSource != null)
        {
            if (!hasPlayed)
            {
                meditationAudioSource.Play();
            }
            else 
            {
                meditationAudioSource.UnPause();
            }
        }
        else 
        {
            Debug.LogWarning("未设置音乐文件或AudioSource组件");
        }
    }
}
