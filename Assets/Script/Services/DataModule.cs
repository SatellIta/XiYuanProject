using System;
using System.Collections.Generic;
using Newtonsoft.Json;

// 定义了与API进行交互所需的数据模型

// --- 核心存档数据 ---
[Serializable]
public class SaveDataDTO
{
    [JsonProperty("chatId")]
    public string chatId;

    [JsonProperty("timestamp")]
    public long timestamp; // 使用 Unix 时间戳 (毫秒)

    [JsonProperty("completedTherapySession")]
    public int completedTherapySession;

    [JsonProperty("inChat")]
    public bool inChat;

    [JsonProperty("chatHistory")]
    public List<ChatMessage> chatHistory;

    [JsonProperty("therapySummary")]
    public List<string> therapySummary;

    [JsonProperty("playerState")]
    public PlayerStateDTO playerState;

    [JsonProperty("problem")]
    public string problem;

    [JsonProperty("solution")]
    public string solution;
    
    // --- 本地辅助字段 (不上传给后端) ---
    [JsonIgnore] public string LocalFileName; 

    public SaveDataDTO()
    {
        // 初始化默认值
        chatHistory = new List<ChatMessage>();
        therapySummary = new List<string>();
        playerState = new PlayerStateDTO(); // 默认全 50
        completedTherapySession = 0;
        inChat = true;
    }
}

[Serializable]
public class PlayerStateDTO
{
    public int anxiety = 50;
    public int happiness = 50;
    public int stress = 50;
    public int energy = 50;
    public int trust = 50;
    public int resilience = 50;
}

[Serializable]
public class ChatMessage
{
    public string role; // "user" 或 "assistant"
    public string content;
}

// 统一响应模型：映射后端所有可能返回的 JSON 字段
[Serializable]
public class AIResponse
{
    // 基础字段 (所有情况都会有)
    [JsonProperty("dialogue")]
    public string Dialogue { get; set; }

    // Session 1 字段 (问题识别)
    // 后端未识别时为 null，识别后为字符串
    [JsonProperty("problem")]
    public string Problem { get; set; }

    // Session 2 字段 (方案生成)
    [JsonProperty("solution_1")]
    public string Solution1 { get; set; }

    [JsonProperty("solution_2")]
    public string Solution2 { get; set; }

    // Session 3 字段 (关卡推荐)
    [JsonProperty("recommended_level")]
    public string RecommendedLevel { get; set; }

    // Session 3 字段 (是否回归询问)
    [JsonProperty("is_returning")]
    public bool IsReturning { get; set; }

    // 如果后续有更多字段，在这里添加...

    // 辅助属性：用于快速判断当前状态 (只读)
    [JsonIgnore] public bool IsProblemFound => !IsNullOrEmptyOrNone(Problem);
    [JsonIgnore] public bool AreSolutionsReady => !IsNullOrEmptyOrNone(Solution1) && !IsNullOrEmptyOrNone(Solution2);
    [JsonIgnore] public bool IsLevelRecommended => !IsNullOrEmptyOrNone(RecommendedLevel);

    private static bool IsNullOrEmptyOrNone(string str)
    {
        if (string.IsNullOrEmpty(str)) return true;
        string lower = str.Trim().ToLower();
        return lower == "null" || lower == "none" || lower == "n/a" || lower == "empty";
    }
}

// 发送给 /chats/continue 的请求体
[Serializable]
public class ChatRequest
{
    [JsonProperty("chatId")]
    public string ChatId;

    [JsonProperty("body")]
    public string UserMessage;
}

// 发送给 /sessions/confirm 的请求体
[Serializable]
public class ConfirmStageDTO
{
    [JsonProperty("chatId")]
    public string chatId;

    [JsonProperty("problem")]
    public string problem; // 可选

    [JsonProperty("solution")]
    public string solution; // 可选
}


[Serializable]
public class FinishTherapyRequest
{
    [JsonProperty("chatId")]
    public string ChatId;
    [JsonProperty("reason")]
    public string Reason; // "我感觉好多了"
}