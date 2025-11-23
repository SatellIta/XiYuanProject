using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class TherapyUIManager : MonoBehaviour
{   
    // ★ 全局静态开关：供角色控制脚本读取
    public static bool IsChatActive { get; private set; } = false;
    public static bool IsGamePaused { get; private set; } = false;
    [Header("暂停菜单")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeBtn;
    [SerializeField] private Button returnToMenuBtn;
    [SerializeField] private Button quitWithoutSaveBtn; // ★ 新增：不保存直接退出
    
    [Header("核心容器")]
    [SerializeField] private GameObject chatPanel;      // 主聊天面板，用于控制scrollRect和输入区显隐
    [SerializeField] private ScrollRect scrollRect;      // 对应 ChatScrollView
    [SerializeField] private Transform chatContentParent; 
    
    [Header("输入区")]
    [SerializeField] private GameObject inputPanel;      // 输入区的父物体容器
    [SerializeField] private TMP_InputField inputField; 
    [SerializeField] private Button sendButton;

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
    [SerializeField] private KeyCode openChatKey = KeyCode.T;
    [SerializeField] private KeyCode closeChatKey = KeyCode.Escape;

    private GameObject currentTypingIndicator;

    // 事件回调
    // 当用户点击发送或按回车时，触发此事件，把文字传出去
    public Action<string> onUserSubmit; 
    public Action onReturnToMenu; // 返回主菜单事件
    public Action onQuitWithoutSave; // ★ 新增：不保存退出事件

    // --- 1. 基础聊天功能 ---

    public void AddUserMessage(string text)
    {
        CreateBubble(userBubblePrefab, text);
        ClearInput();
    }

    public void AddAIMessage(string text)
    {
        HideTyping(); 
        CreateBubble(aiBubblePrefab, text);
    }

    public void ShowTyping(bool show)
    {
        if (show)
        {
            if (currentTypingIndicator == null && typingIndicatorPrefab != null)
            {
                currentTypingIndicator = Instantiate(typingIndicatorPrefab, chatContentParent);
                ScrollToBottom();
            }
        }
        else
        {
            HideTyping();
        }
    }

    private void HideTyping()
    {
        if (currentTypingIndicator != null)
        {
            Destroy(currentTypingIndicator);
            currentTypingIndicator = null;
        }
    }

    private void Start()
    {
        SetChatActive(false);
        TogglePauseMenu(false);

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
                TogglePauseMenu(false); // 先关UI
                onReturnToMenu?.Invoke(); // 通知 Manager 保存并退出
            });
        }
        // ★ 新增：绑定直接退出按钮
        if (quitWithoutSaveBtn)
        {
            quitWithoutSaveBtn.onClick.RemoveAllListeners();
            quitWithoutSaveBtn.onClick.AddListener(() => {
                TogglePauseMenu(false);
                onQuitWithoutSave?.Invoke(); // 通知 Manager 直接退出
            });
        }
    }

    private void Update()
    {
        // 逻辑优先级：
        // 1. 如果暂停菜单已开 -> ESC 关闭暂停菜单 (恢复游戏)
        // 2. 如果特殊交互面板已开 (Problem/Solution/Level) -> ESC 打开暂停菜单 (覆盖显示，不关闭原面板)
        // 3. 如果只是普通聊天框已开 -> ESC 关闭聊天框 (恢复移动)
        // 4. 如果都没开 -> ESC 打开暂停菜单
        // 5. 如果都没开 -> T 打开聊天框

        if (IsGamePaused)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) TogglePauseMenu(false);
        }
        else if (IsAnySpecialPanelActive())
        {
            // 特殊状态下，ESC 呼出暂停菜单，而不是关闭面板
            if (Input.GetKeyDown(KeyCode.Escape)) TogglePauseMenu(true);
        }
        else if (IsChatActive)
        {
            // 普通聊天状态下，ESC 关闭聊天
            if (Input.GetKeyDown(closeChatKey)) SetChatActive(false);
        }
        else
        {
            // 漫游状态
            if (Input.GetKeyDown(KeyCode.Escape)) TogglePauseMenu(true);
            if (Input.GetKeyDown(openChatKey)) SetChatActive(true);
        }
    }

    private void HandleSubmit()
    {
        if (inputField == null) return;
        string text = inputField.text;
        if (string.IsNullOrWhiteSpace(text)) return;

        onUserSubmit?.Invoke(text);
        ClearInput();
        
        // 发送完后保持开启还是关闭？
        // 通常保持开启方便连续对话。如果想发完即走，这里调 SetChatActive(false);
        inputField.ActivateInputField(); 
    }

    // --- 修改生成气泡的逻辑 (修复排版和缩放) ---
    private void CreateBubble(ChatBubbleCell prefab, string text)
    {
        // 1. 生成 (作为 chatContentParent 的子物体)
        ChatBubbleCell cell = Instantiate(prefab, chatContentParent);
        
        // 2. ★ 关键修复：重置缩放和位置
        cell.transform.localScale = Vector3.one; 
        cell.transform.localPosition = Vector3.zero; // Z轴归零
        
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

    // --- 暂停菜单控制 ---
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

        // 鼠标控制
        if (show)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // 恢复游戏时，如果不在聊天状态，可能需要锁定鼠标(根据你的游戏类型)
            if (!IsChatActive)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    // 互斥面板切换器
    // 作用：只显示传入的 targetPanel，隐藏其他所有已知面板
    private void SwitchToPanel(GameObject targetPanel)
    {
        // 1. 先关掉所有
        if (chatPanel) chatPanel.SetActive(false);
        if (problemPanel) problemPanel.SetActive(false);
        if (solutionPanel) solutionPanel.SetActive(false);
        if (levelRecommendPanel) levelRecommendPanel.SetActive(false);
        if (levelSelectionPanel) levelSelectionPanel.SetActive(false);

        // 2. 再打开目标
        if (targetPanel) 
        {   
            // 打开聊天：解锁鼠标
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            targetPanel.SetActive(true);
            targetPanel.transform.SetAsLastSibling(); // 依然保持置顶习惯，防止背景遮挡
        }
    }

    public void SetChatActive(bool isActive)
    {
        IsChatActive = isActive;

        if (isActive)
        {
            SwitchToPanel(chatPanel); // 显示主聊天面板
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            if (inputField != null) inputField.ActivateInputField();
            
            // 确保子组件显示
            if (inputPanel) inputPanel.SetActive(true);
            if (scrollRect) scrollRect.gameObject.SetActive(true);
        }
        else
        {
            // 关闭所有面板
            SwitchToPanel(null); 
            if (inputField != null) inputField.DeactivateInputField();
        }
    }

    public void ShowChatUI()
    {
        SetChatActive(true);
        if (finishTherapyBtn != null)
        {
            finishTherapyBtn.gameObject.SetActive(true);
            finishTherapyBtn.transform.SetAsLastSibling();
        }
    }

    // --- Session 1: 问题确认 ---
    public void ShowProblemWindow(string problem, Action onConfirm, Action onReject)
    {
        if(problemPanel == null) return;

        // ★ 切换 UI：显示问题，隐藏聊天
        SwitchToPanel(problemPanel);
        
        // 强制解锁鼠标 (防止从非聊天状态直接跳过来)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        IsChatActive = true; // 视为交互中，暂停角色移动

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

        // ★ 切换 UI
        SwitchToPanel(solutionPanel);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        IsChatActive = true;
        
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
                
                // 2. ★ 关键修改：将预设文本填入输入框，等待用户补充
                if (inputField != null)
                {
                    inputField.text = "这些方案我都不太满意，我想再讨论一下，"; // 加逗号引导
                    inputField.ActivateInputField();
                    
                    // 启动协程将光标移到末尾 (需要等待一帧)
                    StartCoroutine(MoveCaretToEnd());
                }
                
                // 3. 调用回调 (Manager层对应传入空操作即可，因为现在不发请求了)
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

        // ★ 切换 UI
        SwitchToPanel(levelRecommendPanel);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        IsChatActive = true;

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

    // --- Session 3: 全部关卡列表 ---
    public void ShowAllLevels(Action<TherapyLevelID> onLevelSelected, Action onBack)
    {
        if(levelSelectionPanel == null) return;

        // ★ 切换 UI
        SwitchToPanel(levelSelectionPanel);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        IsChatActive = true;

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
}