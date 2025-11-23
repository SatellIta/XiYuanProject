using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("移动参数")]
    [Tooltip("角色的前进和后退速度")]
    public float moveSpeed = 5.0f;

    [Header("平滑参数")]
    [Tooltip("移动插值系数，值越小移动越粘滞")]
    [Range(0.1f, 1.0f)]
    public float smoothFactor = 0.3f;

    // 私有变量
    private Rigidbody rb;
    private float horizontalInput;
    private float verticalInput;

    // 用于平滑移动的变量
    private Vector3 currentVelocity;
    private Vector3 targetVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // ★ 核心修复：检查聊天状态
        if (TherapyUIManager.IsChatActive) 
        {
            // 1. 归零输入值，防止残留
            horizontalInput = 0f;
            verticalInput = 0f;
            
            // 2. 归零目标速度，确保 FixedUpdate 能让角色停下来
            targetVelocity = Vector3.zero;
            
            // 3. 此时再 Return，不再读取键盘
            return; 
        }

        // 正常输入读取
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        
        targetVelocity = (transform.forward * verticalInput + transform.right * horizontalInput) * moveSpeed;
    }

    void FixedUpdate()
    {
        // 如果聊天打开，targetVelocity 已经被设为 (0,0,0)，
        // 这里的 Lerp 会让角色平滑减速直到停止。
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, smoothFactor);
        
        // 保持原有的 Y 轴速度 (重力)
        rb.velocity = new Vector3(currentVelocity.x, rb.velocity.y, currentVelocity.z);
    }
}