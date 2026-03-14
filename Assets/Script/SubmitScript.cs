using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class SubmitScript : MonoBehaviour
{
    // 在 Inspector 把 8 个 ToggleGroup 拖进来
    [SerializeField] private ToggleGroup[] toggleGroups;

    // 拖量表面板（PsychScalePanel）
    [SerializeField] private GameObject scalePanel;

    // 拖自己的按钮（可选，方便清事件）
    [SerializeField] private Button submitBtn;

    public MouseLook mouselook;

    // 存储变量
    private int totalScore = -1;
    private string syphotom = "暂无";

    // 异步任务源：用于挂起 GameManager，直到玩家点击提交
    private UniTaskCompletionSource<(int score, string level)> submitTcs;

    public int TotalScore() => totalScore;
    public string Syphotom() => syphotom;

    // 供 UIManager 调用的异步等待方法
    public UniTask<(int score, string level)> WaitForSubmitAsync()
    {
        submitTcs = new UniTaskCompletionSource<(int, string)>();
        return submitTcs.Task;
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
            <= 8 => "良好",
            <= 16 => "一般",
            <= 24 => "较严重",
            _ => "很严重"
        };
        syphotom = level;

        // 清事件
        if (submitBtn != null) submitBtn.onClick.RemoveAllListeners();
        
        // （原有的 UI 关闭和鼠标隐藏逻辑，现在交由 UIManager 统一接管，这里不再写死）
        if (mouselook != null) mouselook.uiActive = false;

        // 核心：通知等待中的代码（GameManager），玩家提交了！把结果传过去
        submitTcs?.TrySetResult((totalScore, level));
    }
}