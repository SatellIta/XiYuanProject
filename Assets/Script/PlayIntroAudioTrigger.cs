using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioTrigger : MonoBehaviour
{
    public AudioClip audioClip; // 要播放的音频剪辑
    private AudioSource audioSource; // 音频源组件
    private bool audioPlayed = false;

    void Start()
    {
        // 获取音频源组件
        audioSource = GetComponent<AudioSource>();
        // 如果音频源组件不存在，则添加一个
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // 设置音频剪辑
        audioSource.clip = audioClip;
    }

    void OnTriggerEnter(Collider other)
    {
        // 检查触发碰撞的是否是玩家（假设玩家的标签为 "Player"）
        if (other.CompareTag("Player") && !audioPlayed)
        {
            // 播放音频
            audioSource.Play();
            audioPlayed = true;
        }
    }
}
