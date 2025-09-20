using UnityEngine;

public class LockDoorTrigger : MonoBehaviour
{
    [Header("触发设置")]
    [Tooltip("将你想要锁定的那扇门（父对象）拖拽到这里")]
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

            // 标记为已触发，并开始执行关门和锁定的逻辑
            Debug.Log("玩家进入触发区域，开始执行关门锁定...");
            hasBeenTriggered = true;
            CloseAndLockTheDoor();
        }
    }

    // 不必等待动画完成，可以直接锁门
    private void CloseAndLockTheDoor()
    {
        // 检查门当前是否是打开的
        if (targetDoor.IsDoorOpen())
        {
            // 如果是打开的，先调用关门方法
            targetDoor.ToggleDoor();
        }

        // 无论是刚关上，还是本来就是关着的，现在都执行锁定操作
        targetDoor.isLocked = true;
        Debug.Log("门 '" + targetDoor.name + "' 已被成功锁定！");

        return;
    }
}