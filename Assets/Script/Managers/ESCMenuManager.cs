using UnityEngine;
using UnityEngine.SceneManagement;

public class ESCMenuManager : MonoBehaviour
{
    // 公开变量，用于在 Unity 编辑器中关联菜单面板
    public GameObject pauseMenuPanel;
    // 导入聊天面板，以便在暂停时禁用它
    public GameObject chatPanel;

    // 用于追踪游戏当前是否已暂停
    private bool isPaused = false;

    // 静态变量，方便其他脚本访问菜单状态
    public static bool isGamePaused = false;

    void Update()
    {
        // 检测玩家是否按下了 "Escape" 键
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                // 如果当前已暂停，则恢复游戏
                Resume();
            }
            else
            {
                // 如果当前未暂停，则暂停游戏
                Pause();
            }
        }
    }

    // 恢复游戏的方法
    public void Resume()
    {
        pauseMenuPanel.SetActive(false); // 隐藏菜单
        chatPanel.SetActive(true);   // 显示聊天面板
        Time.timeScale = 1f;             // 将游戏时间恢复为正常速度
        isPaused = false;
        isGamePaused = false;
        Cursor.lockState = CursorLockMode.Locked; // 重新锁定鼠标
        Cursor.visible = false;                   // 隐藏鼠标
    }

    // 暂停游戏的方法
    void Pause()
    {
        pauseMenuPanel.SetActive(true);  // 显示菜单
        chatPanel.SetActive(false);  // 隐藏聊天面板
        Time.timeScale = 0f;             // 将游戏时间停止
        isPaused = true;
        isGamePaused = true;
        Cursor.lockState = CursorLockMode.None; // 解锁鼠标
        Cursor.visible = true;                  // 显示鼠标
    }

    // 加载主菜单的方法（示例）
    public void LoadMenu()
    {
        Time.timeScale = 1f; // 确保在加载新场景前恢复时间
        chatPanel.SetActive(true);   // 显示聊天面板
        isGamePaused = false;
        isPaused = false;
        SceneManager.LoadScene("MainMenu");
        Debug.Log("返回主菜单...");
    }

    // 退出游戏的方法
    public void QuitGame()
    {
        Debug.Log("正在退出游戏...");
        Application.Quit();
    }
}
