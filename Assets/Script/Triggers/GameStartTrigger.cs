using UnityEngine;

public class GameStartTrigger : MonoBehaviour
{
    [Header("配置")]
    [Tooltip("只有标签为这个的物体进入才会触发")]
    [SerializeField] private string targetTag = "Player";

    [Tooltip("触发后是否销毁自身（防止重复触发）")]
    [SerializeField] private bool destroyAfterTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        // 1. 检查进入的是不是玩家
        if (other.CompareTag(targetTag))
        {
            // 2. 找到游戏管理器
            var manager = FindObjectOfType<TherapyGameManager>();
            
            if (manager != null)
            {
                Debug.Log("[Trigger] 玩家进入区域，正在激活游戏系统...");
                
                // 3. 调用我们在 Manager 中写好的激活接口
                manager.ActivateGameSystem();

                // 4. 清理触发器
                if (destroyAfterTrigger)
                {
                    Destroy(gameObject); // 销毁自己，确保只触发一次
                }
            }
            else
            {
                Debug.LogError("[Trigger] 场景中找不到 TherapyGameManager！请检查是否已挂载。");
            }
        }
    }
}