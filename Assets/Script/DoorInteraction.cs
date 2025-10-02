using UnityEngine;

// 现在玩家触发开门的逻辑部分在PlayerInteraction脚本中处理
// 这个脚本专注于门的具体交互行为
[RequireComponent(typeof(AudioSource))]
public class DoorInteraction : MonoBehaviour
{
    [Header("动画对象设置")]
    [Tooltip("请将需要旋转的子对象拖拽到这里")]
    public Animator doorAnimator; // 获取门的Animator组件

    [Header("音效设置")]
    [Tooltip("开门时播放的音效")]
    public AudioClip openSound;
    [Tooltip("关门时播放的音效")]
    public AudioClip closeSound;
    [Tooltip("门锁定时播放的音效")]
    public AudioClip lockedSound;

    // 记录门的当前状态（开或关）
    private bool isOpen = false;
    // 引用音频播放源
    private AudioSource audioSource;

    [Header("锁定设置")]
    [Tooltip("勾选此项，玩家将无法打开这扇门。")]
    public bool isLocked = false;

    void Awake() // 使用 Awake 确保在其他脚本的 Start 之前获取组件
    {
        audioSource = GetComponent<AudioSource>();
        if (doorAnimator == null)
        {
            Debug.LogError("门的Animator未在Inspector中指定！请将门扇对象拖拽到脚本的'Door Animator'字段。", this);
            enabled = false; // 禁用脚本
        }
    }
    // 开关门的核心逻辑
    public void ToggleDoor()
    {
        isOpen = !isOpen;

        // 使用我们指定的doorAnimator来控制动画
        doorAnimator.SetBool("IsOpen", isOpen);
        audioSource = GetComponent<AudioSource>(); // 获取 AudioSource 组件
        // 根据门的新状态播放对应的音效
        if (isOpen)
        {
            // 播放开门音效
            if (openSound != null)
            {
                audioSource.PlayOneShot(openSound);
            }
        }
        else
        {
            // 播放关门音效
            if (closeSound != null)
            {
                audioSource.PlayOneShot(closeSound);
            }
        }

        Debug.Log("门的状态已切换。IsOpen 现在是: " + isOpen);
    }

    // 提供一个公共方法以便其他脚本查询门的状态
    public bool IsDoorOpen()
    {
        return isOpen;
    }

    // 提供一个公共方法以便其他脚本查询门的锁定状态
    public bool IsDoorLocked()
    {
        return isLocked;
    }

    // 提供一个公共方法以便其他脚本解锁门
    public void UnlockDoor()
    {
        isLocked = false;
        Debug.Log("门 '" + name + "' 已被解锁！");
    }
}