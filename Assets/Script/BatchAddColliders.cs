using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// 这是一个用于批量为选中的预制体添加碰撞体的脚本
// 支持添加Box Collider（自动适配大小）和Mesh Collider两种类型
public class BatchAddColliders : Editor
{
    // --- 菜单项1：添加并自动适配Box Collider ---
    [MenuItem("Tools/Batch Add Colliders/Add Auto-Sized BoxCollider")]
    private static void AddBoxColliders()
    {
        AddColliderToSelectedPrefabs<BoxCollider>();
    }

    // --- 菜单项2：添加Mesh Collider ---
    [MenuItem("Tools/Batch Add Colliders/Add MeshCollider")]
    private static void AddMeshColliders()
    {
        AddColliderToSelectedPrefabs<MeshCollider>();
    }

    // 泛型方法，处理添加不同类型的碰撞体
    private static void AddColliderToSelectedPrefabs<T>() where T : Collider
    {
        // 获取在Project窗口中选中的所有GameObject（即Prefabs）
        GameObject[] selectedPrefabs = Selection.GetFiltered<GameObject>(SelectionMode.Assets);

        if (selectedPrefabs.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "请在项目（Project）窗口中至少选择一个Prefab！", "确定");
            return;
        }

        int prefabsModified = 0;
        foreach (GameObject prefab in selectedPrefabs)
        {
            // 找到Prefab中所有包含MeshFilter的子物体
            MeshFilter[] meshFilters = prefab.GetComponentsInChildren<MeshFilter>();

            if (meshFilters.Length == 0)
            {
                Debug.LogWarning("Prefab '" + prefab.name + "' 中未找到任何MeshFilter，已跳过。");
                continue;
            }

            bool addedColliderThisPrefab = false;
            foreach (MeshFilter meshFilter in meshFilters)
            {
                GameObject targetObject = meshFilter.gameObject;

                // 检查是否已经存在任何类型的碰撞体，如果存在则跳过，避免重复添加
                if (targetObject.GetComponent<Collider>() != null)
                {
                    continue;
                }

                // 添加指定类型的Collider
                T newCollider = targetObject.AddComponent<T>();
                addedColliderThisPrefab = true;

                // 特殊处理：如果添加的是BoxCollider，则自动计算并设置大小
                if (newCollider is BoxCollider boxCollider)
                {
                    // 使用共享网格的边界来计算尺寸
                    Bounds bounds = meshFilter.sharedMesh.bounds;
                    boxCollider.center = bounds.center;
                    boxCollider.size = bounds.size;
                }

                // 特殊处理：如果添加的是MeshCollider，可以设置其为凸形（如果需要它动）
                // if (newCollider is MeshCollider meshCollider)
                // {
                //     meshCollider.convex = true; // 如果需要物体是动态的，取消这行注释
                // }
            }

            if (addedColliderThisPrefab)
            {
                prefabsModified++;
                EditorUtility.SetDirty(prefab); // 标记Prefab已修改
                Debug.Log("成功为Prefab '" + prefab.name + "' 添加了碰撞体。");
            }
        }

        if (prefabsModified > 0)
        {
            AssetDatabase.SaveAssets(); // 保存所有修改
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("完成", "已成功为 " + prefabsModified + " 个Prefab添加了碰撞体！", "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("提示", "选中的Prefab均已存在碰撞体或不含模型，未执行任何操作。", "确定");
        }
    }
}