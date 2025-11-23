using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatBubbleCell : MonoBehaviour
{
    [Header("组件引用")]
    [SerializeField] private TMP_Text contentText; // 拖入子物体的 Text 组件
    [SerializeField] private Image bgImage;    // 拖入自身的 Image 组件

    // 初始化气泡数据
    public void Setup(string text)
    {
        contentText.text = text;

        // 强制刷新布局，防止文字刚出来时背景框大小不对
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
}