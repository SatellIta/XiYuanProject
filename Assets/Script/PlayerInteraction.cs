using UnityEngine;

// 这个脚本用于处理玩家与世界物体的交互
// 目前只有与门交互的部分
public class PlayerInteraction : MonoBehaviour
{
    [Header("交互设置")]
    [Tooltip("玩家可以进行交互的最大距离")]
    public float interactionDistance = 3.0f;
    [Tooltip("可交互对象的图层")]
    public LayerMask interactableLayer; // 为了优化性能，只检测特定图层的对象

    void Update()
    {
        // 当玩家按下 'E' 键时
        if (Input.GetKeyDown(KeyCode.E))
        {
            FindAndInteractWithClosestDoor();
        }
    }

    void FindAndInteractWithClosestDoor()
    {
        // 找出玩家附近所有的门
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, interactionDistance, interactableLayer);

        DoorInteraction closestDoor = null;
        float minDistance = float.MaxValue;

        // 遍历找到的所有碰撞体
        foreach (Collider col in nearbyColliders)
        {   
            // 尝试从碰撞体获取 DoorInteraction 脚本
            DoorInteraction door = col.GetComponentInParent<DoorInteraction>();

            // 如果找到了脚本
            if (door != null)
            {
                Debug.Log("发现门: " + door.name);
                // 计算与这个门的距离
                float distance = Vector3.Distance(transform.position, door.transform.position);

                // 如果这个门比之前记录的最近的门还要近
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestDoor = door;
                }
            }
        }

        // 如果找到了最近的门，就与之交互
        if (closestDoor != null)
        {
            Debug.Log("正在与最近的门交互: " + closestDoor.name);
            closestDoor.ToggleDoor();
        }
    }
}