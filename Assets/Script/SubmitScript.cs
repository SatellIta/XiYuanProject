using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubmitScript : MonoBehaviour
{
    // ① 在 Inspector 把 8 个 ToggleGroup 拖进来
    [SerializeField] private ToggleGroup[] toggleGroups;

    // ② 拖量表面板（PsychScalePanel）
    [SerializeField] private GameObject scalePanel;

    // ③ 拖 AIChatManager（挂有 ShowScaleResult 的脚本）
    [SerializeField] private AIChatManager chatMgr;

    // ④ 拖自己的按钮（可选，方便清事件）
    [SerializeField] private Button submitBtn;

    public MouseLook mouselook;

    // 存储变量，方便外部调用
    private int totalScore = -1;
    private string syphotom = "暂无";

    public int TotalScore()
    {
        return totalScore;
    }

    public string Syphotom()
    {
        return syphotom;
    }

    // 提交按钮被点击时调用
    public void OnSubmitScale()
    {
        // 逐组取分
        int total = 0;
        for (int i = 0; i < toggleGroups.Length; i++)
        {
            Toggle selected = null;
            foreach (var toggle in toggleGroups[i].GetComponentsInChildren<Toggle>())
            {
                if (toggle.isOn)
                {
                    selected = toggle;
                    break;
                }
            }

            if (selected != null)
                total += selected.transform.GetSiblingIndex();
        }
        totalScore = total;

        // 简版解读
        string level = total switch
        {
            <= 8 => "不错",
            <= 16 => "一般，可关注",
            <= 24 => "较严重，建议寻求专业支持",
            _ => "很严重，强烈建议联系心理专业人士"
        };
        syphotom = level;

        // 关闭量表 + 清事件 + 回到对话
        if (submitBtn != null) submitBtn.onClick.RemoveAllListeners();
        // when done with UI
        StartCoroutine(chatMgr.ShowScaleResult(total, level));
        scalePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        mouselook.uiActive = false;
    }
}
