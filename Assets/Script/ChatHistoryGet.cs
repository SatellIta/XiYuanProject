using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System; // 如果你要直接控制UI文本

// 这里定义了ChatHistorySettings所需的数据结构
[System.Serializable]
public class PostData
{
    public string userId;
}

[System.Serializable]
public class Chat
{
    public string role;
    public string content;
}

[System.Serializable]
public class ResponseList
{
    public string[] items;
}

[System.Serializable]
public class ResponseChats
{
    public Chat[] items;
}

public class ChatHistoryGet : MonoBehaviour
{
    // 在Inspector中设置
    public GameObject listItemPrefab;
    public Transform listContentParent;
    public Transform detailContentParent;
    public GameObject loadingIndicator;

    private readonly string serverUrl = "http://1.117.207.132/chats/";

    // 这个公共方法给按钮调用
    public void ViewChatList()
    {   
        Console.WriteLine("ViewChatList called");
        StartCoroutine(GetChatList());
    }

    // 用于解析服务器返回的聊天记录列表
    public void ViewChat(string chatId)
    {   
        StartCoroutine(GetChat(chatId));
    }

    public void ClearChatList()
    {
        // 清理旧的列表项
        foreach (Transform child in listContentParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void ClearChatDetail()
    {
        // 清理旧的列表项
        foreach (Transform child in detailContentParent)
        {
            Destroy(child.gameObject);
        }
    }

    IEnumerator GetChatList()
    {
        Console.WriteLine("GetChatList called");
        loadingIndicator.SetActive(true);

        // 使用这个构造函数来创建包含JSON body的POST请求
        UnityWebRequest request = new UnityWebRequest(serverUrl + "history", "GET");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // 发送请求并等待回应
        yield return request.SendWebRequest();

        loadingIndicator.SetActive(false);

        // 处理请求结果
        switch (request.result)
        {
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.DataProcessingError:
                Debug.LogError("Error: " + request.error);
                break;
            case UnityWebRequest.Result.ProtocolError:
                Debug.LogError("HTTP Error: " + request.error);
                break;
            case UnityWebRequest.Result.Success:
                Debug.Log("Received: " + request.downloadHandler.text);
                // 成功，解析并显示数据
                ProcessAndDisplayChatList(request.downloadHandler.text);
                break;
        }
    }

    // 获取单个聊天记录
    IEnumerator GetChat(string chatId)
    {
        loadingIndicator.SetActive(true);
        Debug.Log("Fetching chat with ID: " + chatId);
        UnityWebRequest request = UnityWebRequest.Get(serverUrl + "history/" + chatId);
        yield return request.SendWebRequest();

        loadingIndicator.SetActive(false);

        // 处理请求结果
        switch (request.result)
        {
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.DataProcessingError:
                Debug.LogError("Error: " + request.error);
                break;
            case UnityWebRequest.Result.ProtocolError:
                Debug.LogError("HTTP Error: " + request.error);
                break;
            case UnityWebRequest.Result.Success:
                Debug.Log("Received: " + request.downloadHandler.text);
                // 成功，解析并显示数据
                ProcessAndDisplayChat(request.downloadHandler.text);
                break;
        }
    }

    void ProcessAndDisplayChatList(string jsonResponse)
    {
        // 清理旧的列表项
        foreach (Transform child in listContentParent)
        {
            Destroy(child.gameObject);
        }

        // 解析JSON
        // 这里返回的JSON格式直接是一个字符串数组, 因此需要包装成json对象
        string wrappedJson = "{\"items\":" + jsonResponse + "}";
        ResponseList responseData = JsonUtility.FromJson<ResponseList>(wrappedJson);

        if (responseData == null || responseData.items == null)
        {
            Debug.LogError("Failed to parse JSON response.");
            return;
        }

        // 动态创建列表项
        foreach (string item in responseData.items)
        {
            GameObject newItem = Instantiate(listItemPrefab, listContentParent, false);

            // ChatHistoryItemUI上有脚本
            // 用来填充文本
            ChatHistoryItemUI itemUI = newItem.GetComponent<ChatHistoryItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup("Chat ID", item);
            }
        }
    }

    void ProcessAndDisplayChat(string jsonResponse)
    {
        // 清理旧的列表项
        foreach (Transform child in detailContentParent)
        {
            Destroy(child.gameObject);
        }

        // 解析JSON
        string wrappedJson = "{\"items\":" + jsonResponse + "}";
        ResponseChats responseData = JsonUtility.FromJson<ResponseChats>(wrappedJson);

        if (responseData == null || responseData.items == null)
        {
            Debug.LogError("Failed to parse JSON response.");
            return;
        }

        // 动态创建列表项
        foreach (Chat chat in responseData.items)
        {
            GameObject newItem = Instantiate(listItemPrefab, detailContentParent, false);  // false可以取消unity对坐标的相对位置计算，直接让刷出来的ui根据父物体锚点布局

            // ChatHistoryItemUI上有脚本
            // 用来填充文本
            ChatHistoryItemUI itemUI = newItem.GetComponent<ChatHistoryItemUI>();
            if (itemUI != null)
            {   
                Debug.Log("Setting up chat item UI for role: " + chat.role);
                Debug.Log("Chat content: " + chat.content);
                itemUI.Setup(chat.role, chat.content);
            }
        }
    }
}