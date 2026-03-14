using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;

public class EasterEggRoomTrigger : MonoBehaviour
{
    [System.Serializable]
    public struct SecretDialogue
    {
        [Tooltip("在这句话触发前，需要玩家在房间里死等多少秒")]
        public float delayBeforePlay;
        
        [TextArea(2, 4)]
        [Tooltip("旁白说的话")]
        public string dialogueText;
    }

    [Header("工具间独白序列")]
    public List<SecretDialogue> dialogues;
    
    [Header("玩家中途离开时的嘲讽 (留空则不触发)")]
    [TextArea(2, 3)]
    public string exitDialogue = "[系统] 玩家终于离开了这个毫无意义的角落，系统的 CPU 松了一口气。";

    [Header("设置")]
    [Tooltip("玩家的标签，确保触发器只对玩家响应")]

    [Header("动画控制（控制神秘立方）")]
    public Animator cubeAnimator;
    public string animeBoolPara = "isInRoom";

    // 供动画脚本查看玩家是否还在房间内
    public static bool IsInRoom = false;

    public string playerTag = "Player";

    // 异步取消令牌源，用于在玩家离开触发器时中止独白序列
    private CancellationTokenSource cts;
    private bool hasFinishedAll = false;

    private void Awake()
    {
        // 确保挂载此脚本的物体的碰撞箱是 Trigger 模式
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            IsInRoom = true;
            Debug.Log("[彩蛋] 玩家进入了秘密角落，开始计时...");

            if (cubeAnimator != null && !string.IsNullOrEmpty(animeBoolPara))
            {
                cubeAnimator.SetBool(animeBoolPara, true);
            }
            
            // 如果玩家反复横跳，先取消之前的序列
            CancelSequence();
            
            // 创建一个新的取消令牌
            cts = new CancellationTokenSource();
            hasFinishedAll = false;
            
            // 启动异步独白序列
            StartMonologueSequence(cts.Token).Forget();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("[彩蛋] 玩家离开了秘密角落。");

            if (cubeAnimator != null && !string.IsNullOrEmpty(animeBoolPara))
            {
                cubeAnimator.SetBool(animeBoolPara, false);
            }
            
            // 玩家离开，立刻发送取消信号，中止里面的 Delay 倒计时
            CancelSequence();
            
            // 如果玩家没听完所有独白就跑了，触发嘲讽
            if (!hasFinishedAll && !string.IsNullOrEmpty(exitDialogue))
            {
                // 使用 Fire-and-Forget 模式播放离场话语
                TherapyUIManager.Instance.AddAIMessageAsync(exitDialogue).Forget();
            }
            IsInRoom = false;
        }
    }

    private void CancelSequence()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }
    }

    private async UniTaskVoid StartMonologueSequence(CancellationToken token)
    {
        try
        {
            foreach (var line in dialogues)
            {
                // 挂起等待指定的时间。
                // 传入了 token，如果在这期间玩家离开了触发器，这行代码会直接抛出 OperationCanceledException
                await UniTask.Delay(System.TimeSpan.FromSeconds(line.delayBeforePlay), cancellationToken: token);
                
                // 时间到了，调用 UI 播放字幕。
                await TherapyUIManager.Instance.AddAIMessageAsync(line.dialogueText);
            }
            
            hasFinishedAll = true;
            
            // 全说完后的神秘提示
            await TherapyUIManager.Instance.AddAIMessageAsync("[系统] 你真的在这个角落浪费了人生中宝贵的几分钟。");
        }
        catch (OperationCanceledException)
        {
            // 玩家中途离开了触发器，导致 Delay 被强行取消，代码会跳到这里
            Debug.Log("[彩蛋] 独白序列已被玩家的离开打断。");
        }
    }
}