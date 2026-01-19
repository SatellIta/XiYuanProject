using TMPro;
using UnityEngine;

// 这个脚本用于处理玩家与世界物体的交互
// 目前只有与门交互的部分
public class PlayerInteraction : MonoBehaviour
{
    [Header("交互设置")]
    [Tooltip("玩家可以进行交互的最大距离")]
    public float interactionDistance = 2.0f;
    [Tooltip("可交互对象的图层")]
    public LayerMask interactableLayer; // 为了优化性能，只检测特定图层的对象
    // 新增：交互提示文本UI（拖入Canvas下的Text组件）
    [Header("提示文本UI")]
    public TextMeshProUGUI interactionPromptText;
    // 新增：视野检测角度（如60度，只检测玩家正前方的对象）
    public float interactionFOV = 90f;

    // 缓存最近的可交互对象
    private IInteractable closestInteractable;

    void Update()
    {
        // 1. 持续检测最近的可交互对象
        FindClosestInteractable();

        // 2. 显示/隐藏交互提示文本
        UpdateInteractionPrompt();

        // 3. 按下E键时交互（仅当有可交互对象且可交互时）
        if (Input.GetKeyDown(KeyCode.E) && closestInteractable != null && closestInteractable.CanInteract())
        {
            closestInteractable.Interact();
        }
    }
    // 改造原有FindAndInteractWithClosestDoor为通用的“找最近可交互对象”
    void FindClosestInteractable()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, interactionDistance, interactableLayer);
        //Debug.Log($"找到 {nearbyColliders.Length} 个碰撞体");

        IInteractable closest = null;
        float minDistance = float.MaxValue;

        foreach (Collider col in nearbyColliders)
        {
            if (col.GetComponentInParent<IInteractable>() is IInteractable interactable)
            {
                Vector3 directionToInteractable = interactable.Transform.position - transform.position;
                float distance = Vector3.Distance(transform.position, interactable.Transform.position);
                float angle = Vector3.Angle(transform.forward, directionToInteractable);

                //Debug.Log($"设施: {col.name}, 距离: {distance}, 角度: {angle}, FOV限制: {interactionFOV / 2}");

                if (angle > interactionFOV / 2)
                {
                    //Debug.Log($"跳过 - 超出视野范围");
                    continue;
                }

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = interactable;
                }
            }
        }

        closestInteractable = closest;
        //Debug.Log($"最终选择的设施: {(closest != null ? closest.ToString() : "无")}");
    }

    // 新增：更新交互提示文本
    void UpdateInteractionPrompt()
    {
        if (closestInteractable != null)
        {
            // 显示提示文本
            interactionPromptText.text = closestInteractable.InteractionPrompt;
            interactionPromptText.gameObject.SetActive(true);
        }
        else
        {
            // 隐藏提示文本
            interactionPromptText.gameObject.SetActive(false);
        }
    }
    //void FindAndInteractWithClosestDoor()
    //{
    //    // 找出玩家附近所有的门
    //    Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, interactionDistance, interactableLayer);

    //    DoorInteraction closestDoor = null;
    //    float minDistance = float.MaxValue;

    //    // 遍历找到的所有碰撞体
    //    foreach (Collider col in nearbyColliders)
    //    {   
    //        // 尝试从碰撞体获取 DoorInteraction 脚本
    //        DoorInteraction door = col.GetComponentInParent<DoorInteraction>();

    //        // 如果找到了脚本
    //        if (door != null)
    //        {
    //            Debug.Log("发现门: " + door.name);
    //            // 计算与这个门的距离
    //            float distance = Vector3.Distance(transform.position, door.transform.position);

    //            // 如果这个门比之前记录的最近的门还要近
    //            if (distance < minDistance)
    //            {
    //                minDistance = distance;
    //                closestDoor = door;
    //            }
    //        }
    //    }

    //    // 如果找到了最近的门，就与之交互
    //    if (closestDoor != null)
    //    {
    //        Debug.Log("正在与最近的门交互: " + closestDoor.name);
    //        closestDoor.ToggleDoor();
    //    }
    //}
}