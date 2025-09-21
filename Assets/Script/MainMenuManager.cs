using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ChatHistoryGet; // 引用ChatHistoryGet脚本


public class MainMenuManager : MonoBehaviour
{   
    [Header("脚本调用")]
    public ChatHistoryGet chatHistoryGet;
    [Header("面板调用")]
    public GameObject settingsPanel; // 设置菜单面板
    public GameObject chatHistoryPanel; // 聊天记录面板
    public GameObject chatDetailPanel; // 详细聊天记录面板

    // “开始游戏”按钮
    public void StartGame()
    {
        SceneManager.LoadScene("MainGame");
    }


    // 打开设置菜单
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    // 关闭设置菜单
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    // 打开聊天记录面板
    public void OpenChatHistory()
    {
        if (chatHistoryPanel != null)
        {
            chatHistoryPanel.SetActive(true);
            settingsPanel.SetActive(false); // 关闭设置菜单
            chatHistoryGet.ViewChatList();
        }
    }

    // 关闭聊天记录面板
    public void CloseChatHistory()
    {
        if (chatHistoryPanel != null)
        {
            chatHistoryPanel.SetActive(false);
            settingsPanel.SetActive(true); // 打开设置菜单
            chatHistoryGet.ClearChatList();
        }
    }

    // 创建显示详细聊天记录的窗口
    public void OpenChatDetail()
    {
        if (chatDetailPanel != null)
        {
            chatDetailPanel.SetActive(true);
        }
    }

    // 关闭显示详细聊天记录的窗口，同时清除内容
    public void CloseChatDetail()
    {
        if (chatDetailPanel != null)
        {
            chatDetailPanel.SetActive(false);
            chatHistoryGet.ClearChatDetail();
        }
    }

    // “退出游戏”按钮
    public void QuitGame()
    {
        // 打印一条日志，以便在Unity编辑器中看到按钮被点击了
        Debug.Log("游戏已退出！");

        // 这行代码只在编译后的游戏中生效，在编辑器模式下无效
        Application.Quit();
    }
}