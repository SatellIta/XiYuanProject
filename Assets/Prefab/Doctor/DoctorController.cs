using UnityEngine;

// 控制医生角色的动画
[RequireComponent(typeof(Animator))]
public class DoctorController : MonoBehaviour
{
    [Header("笔记本对象")]
    [Tooltip("医生手里的笔记本对象")]
    public GameObject notebookObject;

    // Animator组件的引用
    private Animator animator;

    // 定义动画状态的常量，避免使用魔法数字
    public const int STATE_IDLE = 0;
    public const int STATE_TAKING_NOTES = 1;
    public const int STATE_WALKING = 2;

    void Awake()
    {
        animator = GetComponent<Animator>();

        // 确保游戏开始时笔记本是隐藏的
        if (notebookObject != null)
        {
            notebookObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("笔记本对象未在DoctorController中指定！", this);
        }
    }

    void Start()
    {
        // 游戏开始时，进入空闲状态
        SetAnimationState(STATE_IDLE);
    }

    /// <summary>
    /// 统一的动画状态设置方法。
    /// </summary>
    /// <param name="stateIndex">要播放的动画状态。</param>
    public void SetAnimationState(int stateIndex)
    {
        if (animator == null) return;

        // 根据状态决定是否显示笔记本
        if (stateIndex == STATE_WALKING)
        {
            // 行走时确保笔记本是隐藏的
            if (notebookObject != null) notebookObject.SetActive(false);
        }
        // 注意：显示笔记本的操作应该由记笔记动画的事件来触发，而不是在这里立即显示

        animator.SetInteger("Animation", stateIndex);
        Debug.Log("设置动画状态为: " + stateIndex);
    }

    // 控制角色开始行走。
    public void TriggerWalking()
    {
        if (animator != null)
        {
            notebookObject.SetActive(false);
            animator.SetInteger("Animation", 2);
        }
    }

    // 触发记笔记的动画
    // 角色会先播放拿出笔记本的动画，然后进入记笔记状态
    // 拿出笔记本的动画还没做
    public void TriggerTakingNotes()
    {

        if (animator != null)
        {
            // animator.SetTrigger("StartTakingNotes");
            notebookObject.SetActive(true);
            animator.SetInteger("Animation", 1); // 1对应记笔记动画状态
        }
    }

    //以下是给动画事件调用的公共函数
    // (由动画事件调用) 在动画的特定帧显示笔记本。
    public void ShowNotebook()
    {
        if (notebookObject != null)
        {
            notebookObject.SetActive(true);
            Debug.Log("动画事件：显示笔记本");
        }
    }
    // (由动画事件调用) 在动画的特定帧隐藏笔记本。
    public void HideNotebook()
    {
        if (notebookObject != null)
        {
            notebookObject.SetActive(false);
            Debug.Log("动画事件：隐藏笔记本");
        }
    }
}