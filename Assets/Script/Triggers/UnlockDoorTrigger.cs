using UnityEngine;

public class UnlockDoorTrigger : MonoBehaviour
{
    [Header("触发设置")]
    [Tooltip("将你想要解锁并打开的那扇门（父对象）拖拽到这里")]
    public DoorInteraction targetDoor;

    [Tooltip("这个触发器是否只能使用一次？")]
    public bool triggerOnce = true;

    // 内部变量，用于跟踪触发器是否已被激活
    private bool hasBeenTriggered = false;

    // 当有其他碰撞体进入这个触发区域时，这个函数会自动被调用
    private void OnTriggerEnter(Collider other)
    {
        // 检查进入的是否是玩家，并且目标门已经被指定
        if (other.CompareTag("Player") && targetDoor != null)
        {
            // 检查这个触发器是否是一次性的，并且是否已经被触发过
            if (triggerOnce && hasBeenTriggered)
            {
                return; // 如果是，则不执行任何操作
            }

            // 标记为已触发，并开始执行解锁和开门的逻辑
            Debug.Log("玩家进入触发区域，开始执行解锁开门...");
            hasBeenTriggered = true;
            
            // 解锁门
            targetDoor.UnlockDoor();

            // 检查门当前是否是关闭的
            if (!targetDoor.IsDoorOpen())
            {
                // 如果是关闭的，则调用开门方法
                targetDoor.ToggleDoor();
                Debug.Log("门 '" + targetDoor.name + "' 已被自动打开！");
            }
            else
            {
                // 如果门本来就是开着的，就不用再操作了
                Debug.Log("门 '" + targetDoor.name + "' 已经是打开状态。");
            }
        }
    }
}