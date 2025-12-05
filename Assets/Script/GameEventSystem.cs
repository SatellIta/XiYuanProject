// 一个用于控制全局游戏进程的系统
// 包括游戏开始、暂停、继续和结束等功能
// 可以给其他组件提供一个查询当前游戏进度的框架
using UnityEngine;

// 全局游戏事件与状态系统 (单例模式)，作为游戏关键信息的信息中心，供其他脚本查询。
public class GameEventSystem : MonoBehaviour
{
    // 创建一个静态的、全局可访问的实例
    public static GameEventSystem Instance { get; private set; }

    [Header("管理器引用")]
    [Tooltip("请将场景中挂载了 TherapyGameManager 脚本的对象拖拽到这里")]
    [SerializeField] private TherapyGameManager therapyGameManager;

    [Tooltip("请将场景中挂载了 TherapyUIManager 脚本的对象拖拽到这里")]
    [SerializeField] private TherapyUIManager therapyUIManager;

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
        if (therapyGameManager == null)
        {
            Debug.LogError("GameEventSystem 错误：未关联 TherapyGameManager！", this);
        }
        if (therapyUIManager == null)
        {
            Debug.LogError("GameEventSystem 错误：未关联 TherapyUIManager！", this);
        }
    }

    // --- 公共查询方法 ---

    // 查询游戏是否已暂停 (ESC菜单是否打开)
    public bool IsGamePaused()
    {
        // 直接访问 TherapyUIManager 的静态变量，这是最快的方式
        return TherapyUIManager.IsGamePaused;
    }

    // 查询游戏主流程是否已经开始
    public bool IsGameStarted()
    {
        if (therapyGameManager != null)
        {
            return therapyGameManager.IsGameStarted();
        }
        Debug.LogWarning("尝试查询游戏开始状态，但 TherapyGameManager 未设置。");
        return false;
    }

    // 查询当前聊天面板是否打开
    public bool IsInChat()
    {
        // 直接访问 TherapyUIManager 的静态变量
        return TherapyUIManager.IsChatActive;
    }
}