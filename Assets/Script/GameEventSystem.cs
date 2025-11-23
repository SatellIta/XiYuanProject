// 一个用于控制全局游戏进程的系统
// 包括游戏开始、暂停、继续和结束等功能
// 可以给其他组件提供一个查询当前游戏进度的框架
using UnityEngine;

public class GameEventSystem : MonoBehaviour
{
    // 创建一个静态的、全局可访问的实例
    public static GameEventSystem Instance { get; private set; }

    [Header("管理器引用")]
    [Tooltip("请将场景中挂载了 AIChatManager 脚本的对象拖拽到这里")]
    [SerializeField] private AIChatManager chatManager;

    [Tooltip("请将场景中挂载了 SubmitScript 脚本的对象拖拽到这里")]
    [SerializeField] private SubmitScript submitScript;

    void Awake()
    {
        // 实现单例模式，确保场景中只有一个GameEventSystem实例
        if (Instance != null && Instance != this)
        {
            // 如果已存在实例，则销毁当前这个重复的
            Destroy(gameObject);
        }
        else
        {
            // 否则，将当前实例设为全局实例
            Instance = this;
            // （可选）如果你的游戏有多个场景切换，取消下面这行注释
            // DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        // 在游戏开始时检查引用是否已正确设置，提供清晰的错误提示
        if (chatManager == null)
        {
            Debug.LogError("GameEventSystem 错误：未关联 AIChatManager！", this);
        }
        if (submitScript == null)
        {
            Debug.LogError("GameEventSystem 错误：未关联 SubmitScript！", this);
        }
    }

    // --- 公共查询方法 ---

    // 查询AI对话管理器是否已经被触发
    public bool IsChatManagerTriggered()
    {
        if (chatManager != null)
        {
            return chatManager.IsTrigger();
        }
        Debug.LogWarning("尝试查询AIChatManager触发状态，但 AIChatManager 未设置。");
        return false;
    }

    // 查询当前是否处于对话流程中。
    public bool IsChatting()
    {
        if (chatManager != null)
        {
            return chatManager.IsChat();
        }
        Debug.LogWarning("尝试查询对话状态，但 AIChatManager 未设置。");
        return false;
    }

    // 查询玩家是否已经完成了量表填写。
    public bool IsScaleFinished()
    {
        if (chatManager != null)
        {
            return chatManager.IsScaleFinished();
        }
        Debug.LogWarning("尝试查询量表完成状态，但 AIChatManager 未设置。");
        return false;
    }

    // 获取量表总分。
    public int GetTotalScaleScore()
    {
        if (submitScript != null)
        {
            return submitScript.TotalScore();
        }
        Debug.LogWarning("尝试获取量表分数，但 SubmitScript 未设置。");
        return -1;
    }

    // 获取根据分数生成的简版症状描述。
    public string GetSymptomLevel()
    {
        if (submitScript != null)
        {
            return submitScript.Syphotom();
        }
        Debug.LogWarning("尝试获取症状描述，但 SubmitScript 未设置。");
        return "暂无";
    }
}