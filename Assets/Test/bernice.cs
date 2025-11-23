using UnityEngine;

/// <summary>
/// 控制医生角色的动画。
/// 使用Animator中的一个名为 "Animation" 的整数参数来切换动画状态。
/// </summary>
[RequireComponent(typeof(Animator))]
public class BerniceController : MonoBehaviour
{
    [Header("动画设置")]
    [Tooltip("勾选此项，角色将自动循环播放初始动画，适用于站立不动的NPC。")]
    public bool loopInitialAnimation = true;

    [Tooltip("设置角色的初始动画状态 (对应Animator中的整数值)。例如：1 代表站立动画。")]
    public int initialAnimationState = 1;

    // Animator组件的引用
    private Animator animator;

    void Awake()
    {
        // 获取并存储Animator组件的引用
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // 如果启用了循环模式，则在开始时设置初始动画状态
        if (loopInitialAnimation)
        {
            SetAnimationState(initialAnimationState);
        }
    }

    /// <summary>
    /// 公共方法，用于从其他脚本更改角色的动画状态。
    /// </summary>
    /// <param name="stateIndex">要播放的动画状态对应的整数值。</param>
    public void SetAnimationState(int stateIndex)
    {
        if (animator != null)
        {
            // 设置Animator中的 "Animation" 整数参数
            animator.SetInteger("Animation", stateIndex);
            Debug.Log("设置动画状态为: " + stateIndex);
        }
        else
        {
            Debug.LogError("在 " + gameObject.name + " 上找不到Animator组件！");
        }
    }
}