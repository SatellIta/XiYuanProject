using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LevelItemCell : MonoBehaviour
{
    [Header("UI 引用")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private Button actionBtn;

    // 初始化数据和点击事件
    public void Setup(LevelData data, Action<TherapyLevelID> onClick)
    {
        titleText.text = data.title;
        descText.text = data.description;

        actionBtn.onClick.RemoveAllListeners();
        actionBtn.onClick.AddListener(() => {
            onClick?.Invoke(data.id);
        });
    }
}