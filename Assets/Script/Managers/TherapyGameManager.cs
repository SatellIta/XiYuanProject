using UnityEngine;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // 需要场景管理

public class TherapyGameManager : MonoBehaviour
{
    [Header("配置")]
    private const string PREFS_LAST_SAVE_FILE = "Therapy_LastSaveFile";
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // 主菜单场景名

    [Header("依赖")]
    [SerializeField] private TherapyUIManager ui;

    // 运行时状态
    private SaveDataDTO currentSaveData;
    private bool isGameStarted = false; 

    // 公共接口，用于其他脚本查询游戏状态
    public bool IsGameStarted() => isGameStarted;
    public bool IsInChat() => ui.IsInChat();

    private async void Start()
    {
        // 绑定输入事件
        if (ui != null)
        {
            // 绑定单参数版本 (适配 Action<string>)
            ui.onUserSubmit = OnUserSubmitText;
            
            // 绑定结束治疗按钮
            ui.BindFinishButton(OnUserRequestFinishTherapy);
            
            // 绑定返回主菜单事件
            ui.onReturnToMenu = ReturnToMainMenu;
             // 绑定直接退出事件 (不保存)
            ui.onQuitWithoutSave = QuitWithoutSave;
        }

        if (GameLaunchConfig.IsNewGame)
        {
            Debug.Log("[GameManager] 进入新游戏模式：系统待机，等待激活信号...");
        }
        else
        {
            await LoadGameLogic(GameLaunchConfig.TargetSaveFileName);
        }
    }

    public void ActivateGameSystem()
    {
        if (isGameStarted) return;

        if (GameLaunchConfig.IsNewGame)
        {
            Debug.Log("[GameManager] 收到触发信号 -> 启动新游戏流程");
            _ = SafeRun(StartNewGameLogic());
        }
    }

    // --- 异常捕获包装器 ---
    private async Task SafeRun(Task task)
    {
        try
        {
            await task;
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameManager] 严重错误: {e.Message}\n{e.StackTrace}");
            ui.ShowNotification($"[系统错误] 游戏发生内部异常: {e.Message}", 3f);
        }
    }

    // --- 辅助：获取东八区时间 ---
    private DateTime GetChinaTime()
    {
        return DateTime.UtcNow.AddHours(8);
    }

    // --- 辅助：生成格式化时间字符串 ---
    private string GetFormattedTimeString()
    {
        return GetChinaTime().ToString("yyyy-MM-dd-HH-mm-ss");
    }

    // --- 逻辑 A: 新游戏 ---
    private async Task StartNewGameLogic()
    {
        isGameStarted = true;

        // 1. 生成 ID
        string timeStr = GetFormattedTimeString();
        string deviceName = SystemInfo.deviceName; 
        deviceName = System.Text.RegularExpressions.Regex.Replace(deviceName, "[^a-zA-Z0-9_\\-]", "");
        
        string newChatId = $"{deviceName}-{timeStr}";
        string fileName = timeStr;

        Debug.Log($"[StartNewGame] 创建新会话 ID: {newChatId}");

        // 2. 初始化本地数据
        currentSaveData = new SaveDataDTO
        {
            chatId = newChatId,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            completedTherapySession = 0,
            inChat = true
        };

        // 3. 先保存到本地 (Checkpoint)
        await SaveCheckpoint(fileName, syncToCloud: false);
        PlayerPrefs.SetString(PREFS_LAST_SAVE_FILE, fileName);

        // 4. 向云端注册
        if (APIService.Instance != null)
        {
            Debug.Log("[StartNewGame] 正在向云端注册新存档 (/saves/new)...");
            try 
            {
                var newPayload = new { chatId = newChatId };
                var result = await APIService.Instance.PostAsync<string>("/saves/new", newPayload);
                
                if (result != null) 
                {
                    Debug.Log("[StartNewGame] 云端注册成功。");
                    Debug.Log("[StartNewGame] 正在同步初始状态到云端...");
                    await SyncCloudSave();
                }
                else
                {
                    Debug.LogError("[StartNewGame] /saves/new 返回空。");
                    ui.ShowNotification("[系统] 云端建档失败，将以离线模式运行。", 3f);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[StartNewGame] 云端注册异常: {e.Message}");
                ui.ShowNotification("[系统] 无法连接云端，进入离线模式。", 3f);
            }
        }
        else
        {
            Debug.LogError("[StartNewGame] APIService 未初始化！");
        }

        // 5. 请求开场白
        Debug.Log("[StartNewGame] 正在请求 AI 开场白 (/chats/load)...");
        await InitChatSession();
    }

    // --- 逻辑 B: 加载游戏 ---
    private async Task LoadGameLogic(string fileName)
    {
        isGameStarted = true;
        Debug.Log($"[LoadGame] 加载存档: {fileName}");
        
        currentSaveData = LocalSaveSystem.LoadFromDisk(fileName);

        if (currentSaveData == null)
        {
            ui.ShowNotification("[系统] 存档文件读取失败，请检查本地文件。", 3f);
            return;
        }
        
        PlayerPrefs.SetString(PREFS_LAST_SAVE_FILE, fileName);

        bool syncSuccess = await SyncCloudSave();
        if (!syncSuccess)
        {
            Debug.LogWarning("[LoadGame] 云端同步失败");
            ui.ShowNotification("[系统] 云端同步失败，将以离线模式运行。", 3f);
        }

        await InitChatSession();
    }

    // --- 核心: 统一保存点 ---
    private async Task SaveCheckpoint(string fileName = null, bool syncToCloud = false)
    {
        if (currentSaveData == null) return;

        currentSaveData.timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        string targetFileName = fileName ?? currentSaveData.LocalFileName;
        if (string.IsNullOrEmpty(targetFileName))
        {
            targetFileName = GetFormattedTimeString();
            currentSaveData.LocalFileName = targetFileName;
        }

        LocalSaveSystem.SaveToDisk(currentSaveData, targetFileName);
        Debug.Log($"[Checkpoint] 本地存档已更新: {targetFileName}");

        if (syncToCloud)
        {
            Debug.Log($"[Checkpoint] 正在同步到云端...");
            await SyncCloudSave();
        }
    }

    private async Task<bool> SyncCloudSave()
    {
        if (APIService.Instance == null) return false;

        try 
        {
            var result = await APIService.Instance.PostAsync<string>("/saves/sync", currentSaveData);
            return result != null;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SyncCloudSave] API 请求异常: {e.Message}");
            return false;
        }
    }

    // --- 辅助: 初始化对话 ---
    private async Task InitChatSession()
    {
        if (currentSaveData == null || APIService.Instance == null) return;

        // 在收到回复后再显示 ChatUI
        // ui.ShowChatUI(); 

        var payload = new { chatId = currentSaveData.chatId };
        
        try 
        {
            ui.ShowTyping(true);  // 先提示用户ai医生正在思考了
            var response = await APIService.Instance.PostAsync<AIResponse>("/chats/load", payload);
            
            if (response != null)
            {
                Debug.Log($"[InitChatSession] 收到回复: {response.Dialogue}");
                RecordMessage("assistant", response.Dialogue); 
                ui.ShowChatUI(); // 收到回复后显示界面
                ui.ShowNotification("请按'T'键输入消息，按'H'键可以查看对话记录", 3f);
                await ProcessAIResponse(response);
            }
            else
            {
                ui.ShowNotification("[系统] 服务器未响应。", 3f);
                ui.ShowChatUI(); // 即使出错也显示界面，让玩家能看到报错
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[InitChatSession] 请求出错: {e.Message}");
            ui.ShowNotification("[系统] 连接失败，请检查网络。", 3f);
            ui.ShowChatUI();
        }
    }

    // --- 核心对话循环 ---
    
    // 重载版本：用于 UI 事件绑定 (Action<string>)
    public void OnUserSubmitText(string text) => OnUserSubmitText(text, true);

    // 完整版本：增加 showUiBubble 参数
    public async void OnUserSubmitText(string text, bool showUiBubble = true)
    {
        if (string.IsNullOrEmpty(text)) return;

        // 1. UI 显示 (仅当需要时才显示)
        if (showUiBubble)
        {
            ui.AddUserMessage(text); 
        }
        
        ui.SetInputState(false);
        ui.ShowTyping(true);

        // 2. 记录历史
        RecordMessage("user", text);

        Debug.Log($"[Chat] 发送给 AI (UI={showUiBubble}): {text}");

        try 
        {
            var response = await APIService.Instance.PostAsync<AIResponse>("/chats/continue", 
                new ChatRequest { ChatId = currentSaveData.chatId, UserMessage = text });
            
            ui.ShowTyping(false);

            if(response != null) 
            {
                RecordMessage("assistant", response.Dialogue);
                await ProcessAIResponse(response);
            }
            else 
            {
                Debug.LogError("[Chat] 发送消息失败");
                ui.ShowNotification("[系统] 网络连接超时，请重试。", 3f);
                ui.SetInputState(true);
            }
        }
        catch (Exception e)
        {
            ui.ShowTyping(false);
            Debug.LogError($"[Chat] 异常: {e.Message}");
            ui.ShowNotification("[系统] 发送失败。", 3f);
            ui.SetInputState(true);
        }
    }

    private void RecordMessage(string role, string content)
    {
        if (string.IsNullOrEmpty(content)) return;
        var msg = new ChatMessage { role = role, content = content };
        currentSaveData.chatHistory.Add(msg);
    }

    // --- 处理 AI 响应 ---
    private async Task ProcessAIResponse(AIResponse response)
    {
        string dialogue = string.IsNullOrEmpty(response.Dialogue) ? "..." : response.Dialogue;
        
        Debug.Log("1");
        
        await ui.AddAIMessageAsync(dialogue);
        Debug.Log("2");
        // [Session 1]
        if (response.IsProblemFound) 
        {
            ui.ShowProblemWindow(response.Problem, 
                onConfirm: () => ConfirmStageLogic(problem: response.Problem),
                
                // 拒绝时，静默发送请求 (UI 已显示伪造气泡)
                onReject: () => OnUserSubmitText("不，这描述得不对，请重新分析。", showUiBubble: false)
            );
            return;
        }

        // [Session 2]
        if (response.AreSolutionsReady)
        {
            ui.ShowSolutionWindow(response.Solution1, response.Solution2,
                onSelected: (choice) => ConfirmStageLogic(solution: choice),
                
                // 这里不会静默发送请求，而是聊天框内默认填充 "这些方案我都不太满意，我想再讨论一下。"
                onContinue: () =>
                {
                    ui.SetInputState(true);
                }
            );
            return;
        }

        // [Session 3]
        if (response.IsLevelRecommended)
        {
            TherapyLevelID levelId = GameConfig.ParseLevelID(response.RecommendedLevel);
            LevelData levelData = GameConfig.AllLevels.Find(l => l.id == levelId);

            if (levelData != null)
            {
                ui.SetInputState(false);
                ui.ShowLevelRecommendation(levelData,
                    onAccept: () => EnterLevel(levelData.id),
                    onShowAll: () => OpenLevelSelector(levelData),
                    onRealWorld: () => ExitToRealWorld()
                );
            }
            else
            {
                Debug.LogWarning($"[GameManager] AI 推荐了未知关卡: {response.RecommendedLevel}");
                ui.SetInputState(true);
            }
            return;
        }
        
        ui.SetInputState(true);
    }

    // --- 阶段确认逻辑 ---
    private async void ConfirmStageLogic(string problem = null, string solution = null)
    {
        ui.ShowTyping(true);

        var payload = new ConfirmStageDTO
        { 
            chatId = currentSaveData.chatId, 
            problem = problem, 
            solution = solution 
        };

        var result = await APIService.Instance.PostAsync<SimpleSuccessResponse>("/chats/confirm-stage", payload);
        
        // 收到确认结果后，暂停 Loading，准备下一步
        // ui.ShowTyping(false); // 暂时不关，因为紧接着要 TriggerNextStage

        if (result != null && result.success)
        {
            if(problem != null) currentSaveData.problem = problem;
            if(solution != null) currentSaveData.solution = solution;
            currentSaveData.completedTherapySession++; 

            await SaveCheckpoint(syncToCloud: true);
            
            // ★ 新增：保存成功后，立即触发下一阶段的对话
            await TriggerNextStage();
            
            ui.SetInputState(true); 
        }
        else
        {
            ui.ShowTyping(false); // 如果失败了手动关一下
            ui.ShowNotification("[系统] 状态同步失败。", 3f);
            ui.SetInputState(true);
        }
    }

    // ★ 新增方法：发送空包触发下一阶段
    private async Task TriggerNextStage()
    {
        Debug.Log("[GameManager] 正在触发下一阶段对话...");
        ui.ShowTyping(true);

        try 
        {
            // 发送一个空格 " " 作为 body，告诉后端我们要继续
            var response = await APIService.Instance.PostAsync<AIResponse>("/chats/continue", 
                new ChatRequest { ChatId = currentSaveData.chatId, UserMessage = " " });
            
            ui.ShowTyping(false);

            if(response != null) 
            {
                RecordMessage("assistant", response.Dialogue);
                await ProcessAIResponse(response);
            }
        }
        catch (Exception e)
        {
            ui.ShowTyping(false);
            Debug.LogError($"[TriggerNextStage] 异常: {e.Message}");
        }
    }
    // --- 退出逻辑 ---
    private async void ExitToRealWorld()
    {
        ui.ShowNotification("[系统] 正在保存进度，准备退出...", 3f);
        await SaveCheckpoint(syncToCloud: true);
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // 保存并返回主菜单逻辑
    private async void ReturnToMainMenu()
    {
        Debug.Log("[GameManager] 正在保存并返回主菜单...");
        
        // 1. 保存
        await SaveCheckpoint(syncToCloud: true);
        
        // 2. 恢复时间流速 (防止暂停菜单把时间停了)
        Time.timeScale = 1f;

        // 3. 加载主菜单
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // 不保存直接退出到桌面
    private void QuitWithoutSave()
    {
        Debug.Log("[GameManager] 放弃进度，直接退出应用程序...");
        
        // 恢复时间流速 (虽然要退出了，但保持好习惯)
        Time.timeScale = 1f;
        
        // 退出应用
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OpenLevelSelector(LevelData originalRecommend)
    {
        ui.ShowAllLevels(
            onLevelSelected: (selectedId) => EnterLevel(selectedId),
            onBack: () => {
                ui.ShowLevelRecommendation(originalRecommend, 
                    () => EnterLevel(originalRecommend.id),
                    () => OpenLevelSelector(originalRecommend),
                    () => ExitToRealWorld()
                );
            }
        );
    }

    private async void EnterLevel(TherapyLevelID levelId)
    {
        Debug.Log($"[GameManager] 进入关卡: {levelId}");
        await SaveCheckpoint(syncToCloud: true); 
        ui.SetInLevel(true);
        Debug.Log($"[系统] 正在加载 '{levelId}' 模块...");

        bool levelResult = await RunLevel(levelId);

        ui.SetInLevel(false);
        ui.ShowChatUI();

        if (levelResult)
        {
            Debug.Log($"[GameManager] 关卡 '{levelId}' 完成，通知后端...");
        }
        else
        {
            Debug.Log($"[GameManager] 关卡 '{levelId}' 未完成，返回聊天界面。");
            ui.AddAIMessage($"你已经退出了 {levelId} 关卡。");
        }
    }

    // 调用 TherapyLevelManager 来运行关卡，并等待结果
    private async Task<bool> RunLevel(TherapyLevelID levelId)
    {
        Debug.Log($"正在等待玩家前往 '{levelId}' 关卡场景并完成挑战...");

        await Task.Delay(10000); // 模拟等待玩家完成关卡

        // return await TherapyLevelManager.Instance.RunLevel(levelId);
        return true;
    }

    private async void OnUserRequestFinishTherapy()
    {
        ui.AddUserMessage("我认为我已经治疗得差不多了，我想结束疗程。");
        ui.SetInputState(false);
        ui.ShowTyping(true);
        
        RecordMessage("user", "请求结束治疗");

        // ★ 修正：使用 correct DTO property names (chatId, body)
        var response = await APIService.Instance.PostAsync<AIResponse>("/chats/continue", 
             new ChatRequest { ChatId = currentSaveData.chatId, UserMessage = "我想结束治疗" });
        
        ui.ShowTyping(false);
        
        if (response != null)
        {
            RecordMessage("assistant", response.Dialogue);
            await ProcessAIResponse(response);
            await SaveCheckpoint(syncToCloud: true);
        }
        
        ui.SetInputState(true);
    }
}

// 简单的 DTO 用于确认响应
public class SimpleSuccessResponse { public bool success; public string message; }