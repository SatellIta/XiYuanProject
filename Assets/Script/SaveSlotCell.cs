using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SaveSlotCell : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private Button loadButton;

    // 初始化方法
    public void Setup(SaveDataDTO data, Action<string> onLoadClicked)
    {
        // 1. 显示时间 (假设 timestamp 是毫秒)
        DateTime date = DateTimeOffset.FromUnixTimeMilliseconds(data.timestamp).LocalDateTime;
        timeText.text = date.ToString("yyyy/MM/dd HH:mm");

        // 2. 显示摘要
        string summary = $"进度：第{data.completedTherapySession + 1}阶段" + 
             (string.IsNullOrEmpty(data.problem) ? "" : $"\n诊断：{data.problem}");
            
        infoText.text = summary;

        // 3. 绑定点击事件
        loadButton.onClick.RemoveAllListeners();
        loadButton.onClick.AddListener(() => {
            onLoadClicked?.Invoke(data.LocalFileName);
        });
    }
}