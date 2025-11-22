using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AIChatManager : MonoBehaviour
{   
    private bool wasEscPressed = false;
    private bool waitingForReply = false;
    private bool hasBeenTriggered = false;
    public MouseLook mouselook;

    private readonly string[] welcomePool = {
    "你好，欢迎来到心理疗愈空间。我是一位 AI 陪伴者。在开始之前，你愿意花一点时间填写一份心理量表吗？这样我能更好地了解你。按 Y 表示“愿意”，按 N 表示“跳过”。",
    "很高兴见到你！这里是安全、无评判的疗愈角落。如果想让我更了解你，可以填写一份量表。按 Y 开始，按 N 跳过。",
    "你来了，真好。我是 AI 陪伴者，会陪你一起探索内心。愿意花一点点时间填个量表吗？按 Y 愿意，按 N 跳过。",
    "欢迎！这里只有温暖与倾听。想让我更懂你，可以填一份量表；若不想，直接按 N 跳过即可。按 Y 开始。"
};

    [Header("Scale")]
    [SerializeField] private GameObject scalePanel;   // 拖量表面板
    [SerializeField] private TextMeshProUGUI[] scaleTexts; // 拖 8 个题干 TMP
    [SerializeField] private Button submitBtn;        // 拖 SubmitBtn
    [SerializeField] private SubmitScript scaleSubmit;

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

    private bool isChat = false;
    private bool scaleFinished = false;

    // 公共方法，用于查询是否已经触发该manager
    public bool IsTrigger()
    {
        return hasBeenTriggered;
    }

    // 公共方法，用于其他方法查询当前对话情况
    public bool IsChat()
    {
        return isChat;
    }

    // 公共方法，用于其他方法查询玩家是否完成量表
    public bool IsScaleFinished()
    {
        return scaleFinished;
    }

    // ① 被触发器调用：新建对话 + 拿 AI 第一句
    public void StartNewChat()
    {
        // 只激活一次
        if (hasBeenTriggered) return;
        hasBeenTriggered = true;

        if (isChat == false)
        {
            isChat = true;
        }

        chatId = DateTime.UtcNow.ToString("yyyy-MM-dd-HH:mm"); ;
        string welcome = welcomePool[UnityEngine.Random.Range(0, welcomePool.Length)];
        
        StartCoroutine(TypeSubtitle(welcome, () => ShowHint("本游戏不可替代医学治疗")));
    }

    private void ShowHint(string hint)
    {
        waitingForReply = true;
        hintText.SetActive(true);
        hintText.GetComponent<TextMeshProUGUI>().text = hint;
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
            StartCoroutine(TypeSubtitle(aiSentence, () => ShowHint("按F开始回复")));
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

        hintText.SetActive(false);
        subtitle.text = "对方正在回复……";

        inputField.gameObject.SetActive(false);
        string playerText = inputField.text;
        inputField.caretPosition = 0;
        inputField.selectionAnchorPosition = 0;
        inputField.selectionFocusPosition = 0;
        inputField.text = "";                // 清空
        inputField.interactable = false;     // 等待期间禁用

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        mouselook.uiActive = false;

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
            StartCoroutine(TypeSubtitle(aiReply, () => ShowHint("按F开始回复")));
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
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        mouselook.uiActive = true;
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

        if (waitingForReply && Input.GetKeyDown(KeyCode.Y))
        {
            waitingForReply = false;
            hintText.SetActive(false);     // 隐藏提示
            StartScale();                   // 打开量表
            return;
        }
        if (waitingForReply && Input.GetKeyDown(KeyCode.N))
        {
            waitingForReply = false;
            hintText.SetActive(false);
            StartCoroutine(SkipToNormalChat()); 

        }
        if (waitingForReply && Input.GetKeyDown(KeyCode.F))
        {
            EnableInput();
        }
    }
    private void StartScale()
    {

        // when showing the form
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        mouselook.uiActive = true;

        scalePanel.SetActive(true);

        Canvas.ForceUpdateCanvases();
        var scrollRect = scalePanel.GetComponentInChildren<ScrollRect>();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;

        submitBtn.onClick.RemoveAllListeners();          // 防重复注册
        submitBtn.onClick.AddListener(scaleSubmit.OnSubmitScale);    //点击即提交
        scaleFinished = true;
    }
    public IEnumerator ShowScaleResult(int total, string level)
    {
        // 1️⃣ Compose the player's first message based on scale
        string resultSummary = $"谢谢完成！你的总得分是 {total} 分（满分 32），整体状态{level}。";
        string prompt = $"{resultSummary} 接下来我们开始聊聊吧。";

        // Step 1️⃣ Type the summary text
        yield return StartCoroutine(TypeSubtitle(prompt, () =>
        {
            Debug.Log("Typing finished → now starting PostNewChatWithPrompt");
            StartCoroutine(PostNewChatWithPrompt(total)); // 🚀 call it asynchronously
        }));
    }


    private IEnumerator SkipToNormalChat()
    {
        string skipReply = "好的，我们直接开始聊天吧！你可以随时分享你的感受或想法，我会在这里陪伴你。";

        // Step 1️⃣: Type the message
        yield return StartCoroutine(TypeSubtitle(skipReply, null));

        // Step 2️⃣: Start new chat session with no initial message
        yield return StartCoroutine(PostNewChat());
    }
    private IEnumerator PostNewChatWithPrompt(int total)
    {
        Debug.Log("post new chat in");

        yield return StartCoroutine(SetSystemPrompt(total));

        string url = $"{server}/chats/new";
        string systemPromptName = "therapy_prompt";

        string json = $"{{\"chatId\":\"{chatId}\",\"systemPromptName\":\"{systemPromptName}\"}}";

        using UnityWebRequest req = new UnityWebRequest(url, "POST");
        byte[] raw = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(raw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string aiReply = req.downloadHandler.text.Trim('"');
            StartCoroutine(TypeSubtitle(aiReply, () => ShowHint("按F开始回复")));
        }
        else
        {
            Debug.LogError("新建对话失败: " + req.error);
        }
    }
    private string GetPromptByScore(int total)
    {
        Debug.Log("get prompt in");

        if (total <= 8)
        {
            return "你是一位专业的心理支持助手。用户的心理量表分数显示状态不错。请以温暖、支持性的语气回应用户，重点在于维持用户的良好状态，提供积极的肯定和建设性的建议。帮助用户巩固现有的心理资源，鼓励他们继续保持健康的生活方式和思维习惯。注意请以对话的形式与用户交流，不要使用括号内的表示动作和神态的文字。";
        }
        else if (total <= 16)
        {
            return "你是一位专业的心理支持助手。用户的心理量表分数显示状态一般，需要关注。请以关怀、理解的态度回应用户，耐心倾听他们的困扰，提供情感支持和实用的应对建议。帮助用户识别压力源，教授简单的压力管理技巧，鼓励他们建立健康的生活习惯。注意请以对话的形式与用户交流，不要使用括号内的表示动作和神态的文字。";
        }
        else if (total <= 24)
        {
            return "你是一位专业的心理支持助手。用户的心理量表分数显示状态较严重，需要专业支持。请以温和但明确的方式回应用户，表达深切关怀的同时，建议寻求专业心理帮助。提供情感支持，帮助用户理解他们的感受是正常的，强调寻求帮助是勇敢的行为。可以建议一些即时可用的应对策略，但始终强调专业支持的重要性。注意请以对话的形式与用户交流，不要使用括号内的表示动作和神态的文字。";
        }
        else
        {
            return "你是一位专业的心理支持助手。用户的心理量表分数显示状态很严重，急需专业干预。请以极其关怀和紧急的态度回应用户，强烈建议立即联系心理专业人士。提供危机干预的基本支持，帮助用户稳定情绪，但明确表示AI无法替代专业治疗。建议具体的求助途径（心理热线、医院心理科等），并鼓励用户不要独自承受。注意请以对话的形式与用户交流，不要使用括号内的表示动作和神态的文字。";
        }
    }

    // 设置系统提示词到服务端
    private IEnumerator SetSystemPrompt(int total)
    {
        Debug.Log("set prompt in");

        string promptContent = GetPromptByScore(total);
        //string promptContent = "test";
        string systemPromptName = "therapy_prompt"; // 固定的提示词名称

        string url = $"{server}/prompts/set";
        string json = $"{{\"systemPromptName\":\"{systemPromptName}\",\"prompt\":\"{promptContent}\"}}";
        Debug.Log("json:" + json);

        using UnityWebRequest req = new UnityWebRequest(url, "POST");
        byte[] raw = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(raw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("系统提示词设置成功");
        }
        else
        {
            Debug.LogError("设置系统提示词失败: " + req.error);
        }
    }

}

