using UnityEngine;
using UnityEngine.EventSystems;

// 这个脚本用于调试鼠标点击事件，帮助开发者确认点击到了哪个 UI 元素

public class ClickDebugger : MonoBehaviour
{
    void Update()
    {
        // 当你点击鼠标左键时
        if (Input.GetMouseButtonDown(0))
        {
            // 检查鼠标底下是否有 UI 物体
            if (EventSystem.current.IsPointerOverGameObject())
            {
                // 打印出鼠标当前点到了谁
                GameObject clickedObj = EventSystem.current.currentSelectedGameObject;
                
                // 如果 currentSelectedGameObject 为空，我们可以用射线检测找当前 hover 的物体
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                };
                
                var results = new System.Collections.Generic.List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);
                
                if (results.Count > 0)
                {
                    Debug.Log($"[ClickDebug] 你点到了 UI: {results[0].gameObject.name} (父物体: {results[0].gameObject.transform.parent.name})");
                }
            }
            else
            {
                Debug.Log("[ClickDebug] 你点到了 3D 场景，没有点到 UI。");
            }
        }
    }
}