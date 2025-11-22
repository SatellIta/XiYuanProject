using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
    [Header("场景配置")]
    [SerializeField] private string gameSceneName = "MainGame"; 

    [Header("UI 面板引用")]
    [SerializeField] private GameObject mainPanel;       
    [SerializeField] private GameObject settingsPanel;   
    [SerializeField] private GameObject loadGamePanel;   

    [Header("主界面按钮")]
    [SerializeField] private Button newGameBtn;
    [SerializeField] private Button loadGameBtn;
    [SerializeField] private Button settingsBtn;
    [SerializeField] private Button exitBtn;

    [Header("子面板按钮")]
    [SerializeField] private Button loadPanelBackBtn;
    // 假设你的设置面板里有一个关闭按钮，或者你希望点击背景关闭
    [SerializeField] private Button settingsPanelCloseBtn; 

    [Header("存档列表配置")]
    [SerializeField] private Transform saveSlotContainer; 
    [SerializeField] private SaveSlotCell saveSlotPrefab; 
    [SerializeField] private GameObject noSavesText;      

    private void Start()
    {
        // --- 1. 绑定事件 ---
        
        // 主界面
        if(newGameBtn) newGameBtn.onClick.AddListener(OnClickNewGame);
        if(loadGameBtn) loadGameBtn.onClick.AddListener(OnClickOpenLoadPanel);
        if(settingsBtn) settingsBtn.onClick.AddListener(OnClickSettings);
        if(exitBtn) exitBtn.onClick.AddListener(OnClickExit);

        // 子面板
        if(loadPanelBackBtn) loadPanelBackBtn.onClick.AddListener(OnClickBackFromLoad);
        if(settingsPanelCloseBtn) settingsPanelCloseBtn.onClick.AddListener(OnClickCloseSettings);

        // --- 2. 初始化状态 ---
        ShowPanel(mainPanel);
    }

    // --- 核心功能逻辑 ---

    private void OnClickNewGame()
    {
        GameLaunchConfig.TargetSaveFileName = ""; 
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnClickOpenLoadPanel()
    {
        ShowPanel(loadGamePanel);
        RefreshSaveList();
    }

    private void OnClickSettings()
    {
        ShowPanel(settingsPanel);
    }

    private void OnClickExit()
    {
        Debug.Log("游戏已退出！");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnClickBackFromLoad()
    {
        ShowPanel(mainPanel);
    }

    private void OnClickCloseSettings()
    {
        ShowPanel(mainPanel);
    }

    // --- 内部逻辑 ---

    private void ShowPanel(GameObject panelToShow)
    {
        if(mainPanel) mainPanel.SetActive(false);
        if(settingsPanel) settingsPanel.SetActive(false);
        if(loadGamePanel) loadGamePanel.SetActive(false);

        if(panelToShow) panelToShow.SetActive(true);
    }

    private void RefreshSaveList()
    {
        // 1. 清空旧列表
        foreach (Transform child in saveSlotContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. 获取本地存档
        List<SaveDataDTO> saves = LocalSaveSystem.GetAllSaves();

        // 3. 处理空存档情况
        if (saves.Count == 0)
        {
            if(noSavesText) noSavesText.SetActive(true);
            return;
        }
        
        if(noSavesText) noSavesText.SetActive(false);

        // 4. 生成列表
        foreach (var saveData in saves)
        {
            SaveSlotCell cell = Instantiate(saveSlotPrefab, saveSlotContainer);
            
            cell.Setup(saveData, (fileName) => {
                LoadGameScene(fileName);
            });
        }
    }

    private void LoadGameScene(string fileName)
    {
        GameLaunchConfig.TargetSaveFileName = fileName;
        SceneManager.LoadScene(gameSceneName);
    }
}