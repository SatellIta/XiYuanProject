using UnityEngine;

// 这个控制角色移动的脚本可以兼容rigidbody组件
// 确保该对象上有关联的Rigidbody组件
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

    // Start is called before the first frame update
    void Start()
    {
        // 获取并存储Rigidbody组件的引用，以便后续使用
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // "Horizontal" 对应 A/D键 或 左右方向键
        horizontalInput = Input.GetAxis("Horizontal");
        // "Vertical" 对应 W/S键 或 上下方向键
        verticalInput = Input.GetAxis("Vertical");
        // 计算目标移动向量
        targetVelocity = (transform.forward * verticalInput + transform.right * horizontalInput) * moveSpeed;
    }

    // FixedUpdate用于物理计算，它的调用频率是固定的
    void FixedUpdate()
    {
        // 创建移动向量，结合前后和左右移动s
        // Time.fixedDeltaTime确保移动是基于时间的，而不是帧率
        // 使用Rigidbody.MovePosition来移动角色，这能保证物理交互的正确性
        // 平滑过渡到目标速度
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, smoothFactor);
        rb.velocity = new Vector3(currentVelocity.x, rb.velocity.y, currentVelocity.z);
    }
}