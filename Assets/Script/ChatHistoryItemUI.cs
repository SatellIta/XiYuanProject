using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 这个脚本用于挂载获取到的聊天记录信息到UI上
public class ChatHistoryItemUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI messageText;
    private string chatId;
    private Button detailButton;

    // 当对象被创建时，添加按钮监听器
    private void Awake()
    {
        detailButton = GetComponent<Button>();
        if (detailButton != null)
        {
            detailButton.onClick.AddListener(OnItemClicked);
        }
    }

    // 公共方法，用于设置UI显示的内容
    public void Setup(string name, string message)
    {
        nameText.text = name;
        messageText.text = message;
        this.chatId = message;
    }

    // 当chat被点击时，显示详细的历史记录
    void OnItemClicked()
    {
        MainMenuManager mainMenuManager = FindObjectOfType<MainMenuManager>();
        if (mainMenuManager != null)
        {
            mainMenuManager.OpenChatDetail();
            ChatHistoryGet chatHistoryGet = mainMenuManager.chatHistoryGet;
            if (chatHistoryGet != null)
            {
                chatHistoryGet.ViewChat(chatId);
            }
        }
    }

    // 当对象被销毁时，最好移除监听器，以防内存泄漏
    void OnDestroy()
    {
        if (detailButton != null) 
        {
            detailButton.onClick.RemoveListener(OnItemClicked);
        }
    }
}