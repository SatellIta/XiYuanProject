using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class AIChatManager : MonoBehaviour
{   
    private bool wasEscPressed = false;
    private bool waitingForReply = false;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI subtitle;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject hintText;

    [Header("Server")]
    private readonly string server = "http://1.117.207.132";

    [Header("Config")]
    [SerializeField] private float typingSpeed = 0.04f; // 字幕逐字间隔

    private string chatId;
    private bool isTyping = false;

    // ① 被触发器调用：新建对话 + 拿 AI 第一句
    public void StartNewChat()
    {
        chatId = System.Guid.NewGuid().ToString();
        StartCoroutine(PostNewChat());
    }

    private void ShowFHint()
    {
        waitingForReply = true;
        hintText.SetActive(true);
        hintText.GetComponent<TextMeshProUGUI>().text = "按 F 开始回复";
    }

    private IEnumerator PostNewChat()
    {
        string url = $"{server}/chats/new";
        string json = $"{{\"chatId\":\"{chatId}\"}}";

        using UnityWebRequest req = new UnityWebRequest(url, "POST");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
        {
            string aiSentence = req.downloadHandler.text.Trim('"');
            StartCoroutine(TypeSubtitle(aiSentence, () => ShowFHint()));
        }
        else
        {
            Debug.LogError("新建对话失败: " + req.error);
        }
    }

    // ② 玩家按回车 → 继续对话
    public void OnPlayerSubmit()
    {
        if (isTyping || string.IsNullOrWhiteSpace(inputField.text)) return;

        inputField.gameObject.SetActive(false);
        string playerText = inputField.text;
        inputField.caretPosition = 0;
        inputField.selectionAnchorPosition = 0;
        inputField.selectionFocusPosition = 0;
        inputField.text = "";                // 清空
        inputField.interactable = false;     // 等待期间禁用

        StartCoroutine(PostContinue(playerText));
    }

    private IEnumerator PostContinue(string body)
    {
        string url = $"{server}/chats/continue";
        string json = $"{{\"body\":\"{body}\",\"chatId\":\"{chatId}\"}}";

        using UnityWebRequest req = new UnityWebRequest(url, "POST");
        byte[] raw = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(raw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
        {
            string aiReply = req.downloadHandler.text.Trim('"');
            StartCoroutine(TypeSubtitle(aiReply, () => ShowFHint()));
        }
        else
        {
            Debug.LogError("继续对话失败: " + req.error);
        }
    }

    // ③ 逐字显示字幕
    private IEnumerator TypeSubtitle(string fullText, System.Action onDone)
    {
        isTyping = true;
        subtitle.text = "";
        foreach (char c in fullText)
        {
            subtitle.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
        onDone?.Invoke();
    }

    private void EnableInput()
    {
        inputField.gameObject.SetActive(true);
        inputField.interactable = true;
        inputField.text = "";          // 再保险清空一次
        inputField.ForceLabelUpdate(); // 强制刷新占位符
        inputField.ActivateInputField(); // 自动聚焦
    }

    public void Update()
    {
        // 检测 ESC 被按下
        if (Input.GetKeyDown(KeyCode.Escape))
            wasEscPressed = true;

        // 如果 ESC 后第一次点鼠标左键，且输入框已显示，就强制聚焦
        if (wasEscPressed && Input.GetMouseButtonDown(0) && inputField.gameObject.activeSelf)
        {
            wasEscPressed = false;          // 只执行一次
            inputField.ActivateInputField(); // 立即聚焦并弹出键盘
        }

        if (waitingForReply && Input.GetKeyDown(KeyCode.F))
        {
            waitingForReply = false;
            hintText.SetActive(false);     // 隐藏提示
            EnableInput();                 // 打开输入框 + 聚焦
        }

    }
}
