using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using Cysharp.Threading.Tasks;

public class TherapyUIManager : MonoBehaviour
{   
    private static TherapyUIManager Instance;

    // ★ 全局静态开关：供角色控制脚本读取
    public static bool IsGamePaused { get; private set; } = false;
    public static bool IsInLevel { get; private set;} = false;
    // 动态计算是否有面板处于激活状态
    public static bool IsChatActive => Instance != null &&
        (Instance.isInputOpen || Instance.isHistoryOpen || Instance.IsAnySpecialPanelActive());
    [Header("暂停菜单")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeBtn;
    [SerializeField] private Button returnToMenuBtn;
    [SerializeField] private Button quitWithoutSaveBtn; // ★ 新增：不保存直接退出     

    [Header("对话记录")]
    [SerializeField] private GameObject historyPanel;   // 对话历史面板
    [SerializeField] private ScrollRect scrollRect;      // 之前的 ChatScrollView, 现在是历史菜单的子组件
    [SerializeField] private Transform chatContentParent; 

    
    [Header("输入区")]
    [SerializeField] private GameObject inputPanel;      // 输入区的父物体容器
    [SerializeField] private TMP_InputField inputField; 
    [SerializeField] private Button sendButton;

    [Header("字幕系统")]
    [SerializeField] private GameObject subtitlePanel;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private float typeSpeed = 0.03f;     // 打字速度
    [SerializeField] private float subtitleStayTime = 3f; // 字幕停留时间

    [Header("系统提示")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TMP_Text notificationText;

    [Header("资源预制体")]
    [SerializeField] private ChatBubbleCell userBubblePrefab; 
    [SerializeField] private ChatBubbleCell aiBubblePrefab;   
    [SerializeField] private GameObject typingIndicatorPrefab; 

    [Header("特殊流程面板")]
    [SerializeField] private GameObject problemPanel;
    [SerializeField] private TMP_Text problemContentText; 
    [SerializeField] private Button problemConfirmBtn;
    [SerializeField] private Button problemRejectBtn;

    [SerializeField] private GameObject solutionPanel;
    [SerializeField] private Button solutionABtn;
    [SerializeField] private TMP_Text solutionAText;   
    [SerializeField] private Button solutionBBtn;
    [SerializeField] private TMP_Text solutionBText;   
    [SerializeField] private Button solutionContinueBtn; 

    [Header("Session 3: 关卡交互")]
    [SerializeField] private GameObject levelRecommendPanel;
    [SerializeField] private Transform recommendCardContainer; 
    [SerializeField] private Button acceptLevelBtn;
    [SerializeField] private Button showAllLevelsBtn;
    [SerializeField] private Button backToRealWorldBtn;
    [SerializeField] private LevelItemCell levelCardPrefab;

    [SerializeField] private GameObject levelSelectionPanel;
    [SerializeField] private Transform levelGridContent;     
    [SerializeField] private Button closeSelectionBtn; 
    
    [Header("全局控制")]
    [SerializeField] private Button finishTherapyBtn;

    // 聊天界面按键配置
    [SerializeField] private KeyCode openInputKey = KeyCode.T;
    [SerializeField] private KeyCode closeChatKey = KeyCode.Escape;
    [SerializeField] private KeyCode openHistoryKey = KeyCode.H;

    // 事件回调
    public Action<string> onUserSubmit; // 用户提交文本事件
    public Action onReturnToMenu; // 返回主菜单事件
    public Action onQuitWithoutSave; // 不保存退出事件

    // 内部状态
    private GameObject currentTypingIndicator;
    private Coroutine subtitleCoroutine;      // 字幕协程
    private Coroutine notificationCoroutine;  // 提示信息协程
    private bool isSubtitlePlaying = false;
    private bool isHistoryOpen = false;
    private bool isInputOpen = false;
    private bool canOpenInput = true;

    // 公共查询接口
    public bool IsInChat() => inputPanel.activeSelf;

    private void Awake()
    {
        Instance = this;
    }

    // 消息添加，现在既要添加字幕，也要保存历史
    // createBubble是在历史聊天记录面板中创建对话框

    public void AddUserMessage(string text)
    {
        CreateBubble(userBubblePrefab, text);
        // ClearInput();
    }

    public void AddAIMessage(string text)
    {
        HideTyping(); 
        CreateBubble(aiBubblePrefab, text);

        // 触发滚动字幕
        if (subtitlePanel != null && subtitleText != null)
        {
            if (subtitleCoroutine != null) StopCoroutine(subtitleCoroutine);
            subtitleCoroutine = StartCoroutine(PlaySubtitle(text));
        }
    }

    public async UniTask AddAIMessageAsync(string text) 
    { 
        HideTyping(); 
        CreateBubble(aiBubblePrefab, text); 

        if (subtitlePanel != null && subtitleText != null)
        {
            if (subtitleCoroutine != null) 
            {
                StopCoroutine(subtitleCoroutine);
            }
            
            // 开启协程并标记状态
            isSubtitlePlaying = true;
            subtitleCoroutine = StartCoroutine(PlaySubtitle(text));
            
            // 直到播放完毕才停止协程和重置状态
            await UniTask.WaitUntil(() => !isSubtitlePlaying);
        }
    }

    public void ShowTyping(bool show)
    {
        if (show)
        {
            // 历史记录显示输入状态
            if (currentTypingIndicator == null && typingIndicatorPrefab != null)
            {
                currentTypingIndicator = Instantiate(typingIndicatorPrefab, chatContentParent);
                ScrollToBottom();
            }

            // 在字幕区域显示思考中
            if (subtitlePanel != null && subtitleText != null)
            {
                if (subtitleCoroutine != null) StopCoroutine(subtitleCoroutine);
                subtitlePanel.SetActive(true);
                subtitleText.text = "[AI医生正在分析思考中...]";
            }
        }
        else
        {
            HideTyping();
        }
    }

    // 打字效果

    private void HideTyping()
    {
        if (currentTypingIndicator != null)
        {
            Destroy(currentTypingIndicator);
            currentTypingIndicator = null;
        }
    }

    // 打字机字幕系统
    private IEnumerator PlaySubtitle(string text)
    {
        subtitlePanel.SetActive(true);
        subtitleText.text = "";

        // 逐字打印
        foreach (char c in text)
        {
            subtitleText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        // 停留一段时间后隐藏
        yield return new WaitForSeconds(subtitleStayTime);
        subtitlePanel.SetActive(false);
        isSubtitlePlaying = false;
    }

    private void Start()
    {
        // 初始化隐藏所有 UI
        ToggleHistory(false);
        ToggleInput(false);
        TogglePauseMenu(false);
        HideNotification();
        if (subtitlePanel) subtitlePanel.SetActive(false);

        if (sendButton != null)
        {
            sendButton.onClick.RemoveAllListeners();
            sendButton.onClick.AddListener(HandleSubmit);
        }

        if (inputField != null)
        {
            inputField.onSubmit.AddListener((text) => HandleSubmit());
        }

        // 绑定暂停菜单按钮
        if (resumeBtn)
        {
            resumeBtn.onClick.RemoveAllListeners();
            resumeBtn.onClick.AddListener(() => TogglePauseMenu(false));
        }
        if (returnToMenuBtn)
        {
            returnToMenuBtn.onClick.RemoveAllListeners();
            returnToMenuBtn.onClick.AddListener(() => {
                TogglePauseMenu(false); 
                onReturnToMenu?.Invoke(); 
            });
        }
        if (quitWithoutSaveBtn)
        {
            quitWithoutSaveBtn.onClick.RemoveAllListeners();
            quitWithoutSaveBtn.onClick.AddListener(() => {
                TogglePauseMenu(false);
                onQuitWithoutSave?.Invoke(); 
            });
        }
    }

    private void Update()
    {
        // 1. 最高优先级拦截 - 暂停菜单控制
        if (IsGamePaused)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) TogglePauseMenu(false);
            return; 
        }

        // 2. 关卡模式拦截 - 屏蔽所有交互快捷键
        if (IsInLevel)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) TogglePauseMenu(true);
            return; 
        }

        // 3. 特殊交互面板打开时 (问题/方案/选关)
        if (IsAnySpecialPanelActive())
        {
            if (Input.GetKeyDown(KeyCode.Escape)) TogglePauseMenu(true);
            return;
        }

        // 以下是正常的探索状态 (可以开历史、开输入、开菜单)
        
        // 处理 ESC 关闭逻辑
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isInputOpen) ToggleInput(false);
            else if (isHistoryOpen) ToggleHistory(false);
            else TogglePauseMenu(true);
            return;
        }

        // 处理 H 键 (历史记录)
        if (Input.GetKeyDown(openHistoryKey) && !isInputOpen)
        {
            ToggleHistory(!isHistoryOpen);
        }

        // 处理 T 键 (小型输入框)
        if (Input.GetKeyDown(openInputKey) && !isHistoryOpen && !isInputOpen && canOpenInput)
        {
            ToggleInput(!isInputOpen);
        }

        // Viewport 自动修复
        if (scrollRect != null && scrollRect.gameObject.activeInHierarchy)
        {
            RectTransform viewport = scrollRect.viewport;
            if (viewport != null && viewport.rect.height < 1f) 
            {
                viewport.anchorMin = Vector2.zero;
                viewport.anchorMax = Vector2.one;
                viewport.sizeDelta = Vector2.zero;
                viewport.anchoredPosition = Vector2.zero;
            }
        }
    }

    private void HandleSubmit()
    {
        if (inputField == null) return;
        string text = inputField.text;
        if (string.IsNullOrWhiteSpace(text)) return;

        onUserSubmit?.Invoke(text);
        ClearInput();
        
        // 发送完后关闭面板
        ToggleInput(false);
    }

    // --- 修改生成气泡的逻辑 (修复排版和缩放) ---
    private void CreateBubble(ChatBubbleCell prefab, string text)
    {
        // 1. 生成 (作为 chatContentParent 的子物体)
        ChatBubbleCell cell = Instantiate(prefab, chatContentParent);
        
        // 2. 重置缩放和位置
        cell.transform.localScale = Vector3.one; 
        
        // 3. 确保物体激活
        cell.gameObject.SetActive(true);
        
        // 4. 填充数据
        cell.Setup(text);

        // 5. 强制刷新布局 (LayoutRebuilder)
        if(chatContentParent != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatContentParent.GetComponent<RectTransform>());
            
        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases(); 
        StartCoroutine(ScrollCoroutine());
    }

    private IEnumerator ScrollCoroutine()
    {
        yield return null; 
        if(scrollRect) scrollRect.verticalNormalizedPosition = 0f; 
    }

    // --- 交互控制 ---
    public void SetInputState(bool interactable)
    {
        canOpenInput = interactable;
        if(inputField) inputField.interactable = interactable;
        if(sendButton) sendButton.interactable = interactable;
        
        if(interactable && inputField) inputField.ActivateInputField();
    }

    private void ClearInput()
    {
        if(inputField) inputField.text = "";
    }

    // 辅助方法，用于判断是否有特殊面板处于激活状态
    // 这个方法用于处理esc菜单逻辑
    private bool IsAnySpecialPanelActive()
    {
        return (problemPanel != null && problemPanel.activeSelf) ||
               (solutionPanel != null && solutionPanel.activeSelf) ||
               (levelRecommendPanel != null && levelRecommendPanel.activeSelf) ||
               (levelSelectionPanel != null && levelSelectionPanel.activeSelf);
    }

    // 辅助方法，统一的状态更新与鼠标状态更新
    private void UpdateStateAndCursor()
    {
        bool needsCursor = IsGamePaused || isHistoryOpen || isInputOpen || IsAnySpecialPanelActive();
        if (needsCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // 各种菜单控制
    // 暂停菜单
    public void TogglePauseMenu(bool show)
    {
        IsGamePaused = show;
        
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(show);
            if (show) pauseMenuPanel.transform.SetAsLastSibling();
        }

        // 暂停/恢复 游戏时间
        Time.timeScale = show ? 0f : 1f;
        UpdateStateAndCursor();
    }

    // 对话记录菜单
    public void ToggleHistory(bool show)
    {
        isHistoryOpen = show;
        if (historyPanel != null)
        {
            historyPanel.SetActive(show);
            if (show) historyPanel.transform.SetAsLastSibling();
        }
        UpdateStateAndCursor();
    }

    // 输入菜单
    public void ToggleInput(bool show)
    {
        isInputOpen = show;
        if (inputPanel != null)
        {
            inputPanel.SetActive(show);
            if (show && inputField != null) inputField.ActivateInputField();
            else if (!show && inputField != null) inputField.DeactivateInputField();
        }
        UpdateStateAndCursor();
    }

    // 互斥面板切换器
    // 作用：只显示传入的 targetPanel，隐藏其他所有已知面板
    private void SwitchToPanel(GameObject targetPanel)
    {
        // 1. 先关掉所有
        if (problemPanel) problemPanel.SetActive(false);
        if (solutionPanel) solutionPanel.SetActive(false);
        if (subtitlePanel) subtitlePanel.SetActive(false);
        if (levelRecommendPanel) levelRecommendPanel.SetActive(false);
        if (levelSelectionPanel) levelSelectionPanel.SetActive(false);

        // 2. 再打开目标
        if (targetPanel) 
        {   
            targetPanel.SetActive(true);
            targetPanel.transform.SetAsLastSibling(); // 依然保持置顶习惯，防止背景遮挡
        }
        UpdateStateAndCursor();
    }

    public void ShowChatUI()
    {
        // 之前是显示聊天面板，现在重构后，这个函数变成切回正常游戏状态的函数
        // 也就是关闭所有特殊面板
        SwitchToPanel(null);
        UpdateStateAndCursor();
    }

    // --- Session 1: 问题确认 ---
    public void ShowProblemWindow(string problem, Action onConfirm, Action onReject)
    {
        if(problemPanel == null) return;

        
        // 切换 UI：显示问题，隐藏聊天
        SwitchToPanel(problemPanel);
        
        UpdateStateAndCursor();

        if(problemContentText != null) problemContentText.text = problem; 

        if(problemConfirmBtn)
        {
            problemConfirmBtn.onClick.RemoveAllListeners();
            problemConfirmBtn.onClick.AddListener(() => {
                // 点击后：切回聊天界面，再执行回调
                ShowChatUI(); 
                AddUserMessage("是的，这就是我的问题。"); 
                onConfirm?.Invoke();
            });
        }

        if(problemRejectBtn)
        {
            problemRejectBtn.onClick.RemoveAllListeners();
            problemRejectBtn.onClick.AddListener(() => {
                // 点击后：切回聊天界面
                ShowChatUI();
                AddUserMessage("不，这描述得不对...");
                onReject?.Invoke();
            });
        }
    }

    // --- Session 2: 方案选择 ---
    public void ShowSolutionWindow(string solA, string solB, Action<string> onSelected, Action onContinue)
    {
        if(solutionPanel == null) return;

        // 切换 UI
        SwitchToPanel(solutionPanel);
        UpdateStateAndCursor();
        
        if(solutionAText) solutionAText.text = solA; 
        if(solutionBText) solutionBText.text = solB; 

        if(solutionABtn)
        {
            solutionABtn.onClick.RemoveAllListeners();
            solutionABtn.onClick.AddListener(() => {
                ShowChatUI();
                AddUserMessage($"我觉得方案一不错：{solA}");
                onSelected?.Invoke(solA);
            });
        }

        if(solutionBBtn)
        {
            solutionBBtn.onClick.RemoveAllListeners();
            solutionBBtn.onClick.AddListener(() => {
                ShowChatUI();
                AddUserMessage($"我更喜欢方案二：{solB}");
                onSelected?.Invoke(solB);
            });
        }

        if(solutionContinueBtn)
        {
            solutionContinueBtn.onClick.RemoveAllListeners();
            solutionContinueBtn.onClick.AddListener(() => {
                // 1. 切回聊天界面
                ShowChatUI();
                
                // 2. 关键修改：将预设文本填入输入框，等待用户补充
                ToggleInput(true); // 打开输入框, 这次重构新增
                if (inputField != null)
                {
                    inputField.text = "这些方案我都不太满意，我想再讨论一下，"; // 加逗号引导
                    inputField.ActivateInputField();
                    
                    // 启动协程将光标移到末尾
                    StartCoroutine(MoveCaretToEnd());
                }
                
                // 3. 调用回调
                onContinue?.Invoke();
            });
        }
    }

    // 辅助协程：确保输入框激活后光标在文字末尾
    private IEnumerator MoveCaretToEnd()
    {
        yield return null; // 等待下一帧 UI 刷新
        if(inputField != null)
        {
            inputField.caretPosition = inputField.text.Length;
        }
    }

    // --- Session 3: 推荐关卡 ---
    public void ShowLevelRecommendation(LevelData level, Action onAccept, Action onShowAll, Action onRealWorld)
    {
        if(levelRecommendPanel == null) return;

        // 切换 UI
        SwitchToPanel(levelRecommendPanel);
        UpdateStateAndCursor();

        foreach (Transform child in recommendCardContainer) Destroy(child.gameObject);

        if(levelCardPrefab && recommendCardContainer)
        {
            LevelItemCell card = Instantiate(levelCardPrefab, recommendCardContainer);
            card.Setup(level, (id) => AcceptLevelLogic(level, onAccept));
        }

        if(acceptLevelBtn)
        {
            acceptLevelBtn.onClick.RemoveAllListeners();
            acceptLevelBtn.onClick.AddListener(() => AcceptLevelLogic(level, onAccept));
        }

        if(showAllLevelsBtn)
        {
            showAllLevelsBtn.onClick.RemoveAllListeners();
            showAllLevelsBtn.onClick.AddListener(() => onShowAll?.Invoke()); // 注意：ShowAllLevels 内部会切面板
        }

        if(backToRealWorldBtn)
        {
            backToRealWorldBtn.onClick.RemoveAllListeners();
            backToRealWorldBtn.onClick.AddListener(() => {
                ShowChatUI();
                AddUserMessage("我想先在现实生活中尝试一下这个方案。");
                onRealWorld?.Invoke();
            });
        }
    }

    private void AcceptLevelLogic(LevelData level, Action onAccept)
    {
        ShowChatUI();
        AddUserMessage($"好的，我试一试{level.title}");
        onAccept?.Invoke();
    }

    // --- Session 3: 设置是否处于关卡状态 ---
    public void SetInLevel(bool inLevel)
    {
        IsInLevel = inLevel;
        if (inLevel)
        {
            // 进入关卡时，自动关闭聊天和特殊面板，确保界面干净
            SwitchToPanel(null);
            ToggleHistory(false);
            ToggleInput(false);
            if (subtitlePanel) subtitlePanel.SetActive(false);
        }
        UpdateStateAndCursor();
    }

    // --- Session 3: 全部关卡列表 ---
    public void ShowAllLevels(Action<TherapyLevelID> onLevelSelected, Action onBack)
    {
        if(levelSelectionPanel == null) return;

        // ★ 切换 UI
        SwitchToPanel(levelSelectionPanel);
        UpdateStateAndCursor();

        foreach (Transform child in levelGridContent) Destroy(child.gameObject);

        foreach (var levelData in GameConfig.AllLevels)
        {
            LevelItemCell cell = Instantiate(levelCardPrefab, levelGridContent);
            cell.Setup(levelData, (selectedId) => {
                ShowChatUI(); // 选中后回聊天
                AddUserMessage($"我选择了：{levelData.title}");
                onLevelSelected?.Invoke(selectedId);
            });
        }

        if(closeSelectionBtn)
        {
            closeSelectionBtn.onClick.RemoveAllListeners();
            closeSelectionBtn.onClick.AddListener(() => {
                // 这里比较特殊，点返回可能想回推荐界面，也可能想回聊天
                // 由 onBack 回调决定，但 UI 这里暂时先隐藏自己
                // 既然 onBack 通常会调 ShowLevelRecommendation，那里会负责切面板
                onBack?.Invoke(); 
            });
        }
    }

    public void BindFinishButton(Action onFinish)
    {
        if(finishTherapyBtn)
        {
            finishTherapyBtn.onClick.RemoveAllListeners();
            finishTherapyBtn.onClick.AddListener(() => onFinish?.Invoke());
        }
    }

    // 系统提示系统
    // 输入文本和显示时长，自动在界面上显示一定时长的提示
    public void ShowNotification(string message, float duration)
    {
        if (notificationPanel == null || notificationText == null)
        {
            Debug.LogWarning("[TherapyUIManager] Notification UI 未分配，无法显示: " + message);
            return;
        }

        // 如果当前已有通知在显示，打断它，显示新的
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
        }
        
        notificationCoroutine = StartCoroutine(DisplayNotificationCoroutine(message, duration));
    }

    // 立即隐藏当前提示，用于内部方法
    private void HideNotification()
    {
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
        notificationCoroutine = null;
    }

    private IEnumerator DisplayNotificationCoroutine(string message, float duration)
    {
        notificationText.text = message;
        notificationPanel.SetActive(true);
        notificationPanel.transform.SetAsLastSibling(); // 保证显示在最前面
        
        yield return new WaitForSeconds(duration);
        
        notificationPanel.SetActive(false);
        notificationCoroutine = null;
    }

}