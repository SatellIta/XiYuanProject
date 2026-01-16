// 先别用，重构项目后doctor行为脚本会重新编写

/*
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(DoctorController))]
[RequireComponent(typeof(NavMeshAgent))]
public class DoctorBehavior : MonoBehaviour
{
    [Header("全局事件查询")]
    [Tooltip("请将场景中挂载了 GameEventSystem 脚本的对象拖拽到这里")]
    public GameEventSystem gameEventSystem;

    [Header("行为目标")]
    [Tooltip("医生需要移动到的目标位置")]
    public Transform destination;
    [Tooltip("医生最终需要面向的目标，通常是玩家角色")]
    public Transform playerTarget; // 新增：对玩家的引用

    [Header("行为参数")]
    [Tooltip("医生转身的速度")]
    public float rotationSpeed = 5f; // 新增：转身速度

    // 组件引用
    private NavMeshAgent agent;
    private DoctorController doctorController;

    // 状态标记，增加一个转身状态
    private enum DoctorState { Idle, MovingToDestination, MovingToStart, TurningToPlayer, AtDestination }
    private DoctorState currentState = DoctorState.Idle;

    // 状态参数，记录医生是不是已经移动过一次了
    private bool hasMoved = false;

    // 存储初始位置
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        doctorController = GetComponent<DoctorController>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    // 移除Start()方法，不再自动开始移动

    // 公共方法：命令医生移动到目标位置。
    public void GoToDestination()
    {
        if (destination == null)
        {
            Debug.LogError("目标位置(destination)未设置！", this);
            return;
        }
        if (currentState != DoctorState.Idle) return;
        agent.SetDestination(destination.position);
        doctorController.SetAnimationState(DoctorController.STATE_WALKING);
        currentState = DoctorState.MovingToDestination;
    }

    // 公共方法：命令医生返回初始位置。
    public void ReturnToStartPosition()
    {
        if (currentState == DoctorState.Idle && Vector3.Distance(transform.position, initialPosition) < 0.1f) return;
        agent.SetDestination(initialPosition);
        doctorController.SetAnimationState(DoctorController.STATE_WALKING);
        currentState = DoctorState.MovingToStart;
    }


    void Update()
    {   
        if (gameEventSystem.IsChatManagerTriggered() == true && hasMoved == false)
        {
            hasMoved = true;
            GoToDestination();
        }

        if (currentState == DoctorState.MovingToDestination || currentState == DoctorState.MovingToStart)
        {
            // 检查是否已到达目的地
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    if (currentState == DoctorState.MovingToDestination) OnDestinationReached();
                    else if (currentState == DoctorState.MovingToStart) OnStartPosReached();
                }
            }
        }
        else if (currentState == DoctorState.TurningToPlayer)
        {
            // 如果处于转身状态，则平滑地转向玩家
            TurnTowardsPlayer();
        }
    }

    private void OnDestinationReached()
    {
        Debug.Log("医生已到达目的地，准备转身。");
        doctorController.SetAnimationState(DoctorController.STATE_IDLE); // 先切换到站立动画
        currentState = DoctorState.TurningToPlayer; // 进入转身状态
    }

    private void TurnTowardsPlayer()
    {
        if (playerTarget == null)
        {
            Debug.LogWarning("PlayerTarget 未设置，无法转向。直接开始记笔记。");
            StartTakingNotes(); // 如果没有目标，直接跳到下一步
            return;
        }

        // 计算从医生到玩家的方向向量，忽略Y轴差异
        Vector3 direction = playerTarget.position - transform.position;
        direction.y = 0; // 保持医生直立

        // 创建目标旋转
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // 使用Slerp或RotateTowards平滑地插值旋转
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // 检查是否已经基本对准玩家
        if (Quaternion.Angle(transform.rotation, targetRotation) < 1.0f)
        {
            StartTakingNotes(); // 转身完成后，开始记笔记
        }
    }

    private void StartTakingNotes()
    {
        Debug.Log("医生转身完毕，开始记笔记。");
        currentState = DoctorState.AtDestination;
        doctorController.TriggerTakingNotes(); // 调用记笔记的方法
    }

    private void OnStartPosReached()
    {
        Debug.Log("医生已返回初始位置。");
        currentState = DoctorState.Idle;
        doctorController.SetAnimationState(DoctorController.STATE_IDLE);
        transform.rotation = initialRotation; // 恢复初始朝向
    }
}
*/