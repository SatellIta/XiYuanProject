using UnityEngine;

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

    [Header("交互距离设置")]
    [Tooltip("玩家需要离门多近才能进行交互？")]
    public float interactionDistance = 3.0f;
    // 引用玩家对象
    private Transform playerTransform;

    void Start()
    {
        // 检查doorAnimator是否在Inspector中被赋值
        if (doorAnimator == null)
        {
            Debug.LogError("门的Animator未在Inspector中指定！请将门扇对象拖拽到脚本的'Door Animator'字段。", this);
            return; // 如果没有指定，则禁用脚本以防出错
        }

        // 自动寻找场景中标签为 "Player" 的对象
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("场景中找不到标签为 'Player' 的对象！请为你的玩家角色设置Tag。", this);
        }
    }

    void Update()
    {
        if (playerTransform == null || doorAnimator == null) return;

        // 计算玩家与门（父对象）的距离
        float distance = Vector3.Distance(playerTransform.position, transform.position);

        if (Input.GetKeyDown(KeyCode.E) && distance <= interactionDistance)
        {
            if (isLocked)
            {
                // 播放锁定音效
                if (lockedSound != null)
                {
                    audioSource.PlayOneShot(lockedSound);
                }
                Debug.Log("这扇门被锁住了！");
                return;
            }

            ToggleDoor();
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