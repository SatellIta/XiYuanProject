using UnityEngine;
using UnityEditor;
using System.IO;

// 这是一个用于从fbx模型中的子物体批量创建预制体的脚本
public class BatchCreatePrefabsFromChildren : Editor
{
    // 顶部菜单栏中添加一个名为 "工具" -> "从子物体批量创建预制体 (处理嵌套)" 的菜单项
    [MenuItem("Tools/Batch Create Prefabs From Children (Handle Nested)")]
    private static void CreatePrefabs()
    {
        // 获取当前在Hierarchy窗口中选中的GameObject
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog("错误", "请先在层级（Hierarchy）窗口中选择一个包含目标物体的父级容器！", "确定");
            return;
        }

        // 需要处理的子物体都位于选中对象的第一个子节点下
        // 这种情况在处理FBX实例时非常常见
        Transform parentTransform = selectedObject.transform;
        if (selectedObject.transform.childCount > 0)
        {
            // 弹窗询问用户是否要处理第一个子物体的子级
            if (EditorUtility.DisplayDialog("选择目标父级",
                "检测到子对象 '" + selectedObject.transform.GetChild(0).name + "'。\n\n您是想转换 '" + selectedObject.name + "' 的直接子级，还是转换其第一个子对象 '" + selectedObject.transform.GetChild(0).name + "' 下的子级？",
                "转换第一个子对象下的子级", "转换直接子级"))
            {
                parentTransform = selectedObject.transform.GetChild(0);
            }
        }
        
        if (parentTransform.childCount == 0)
        {
             EditorUtility.DisplayDialog("提示", "目标父级 '" + parentTransform.name + "' 下没有任何子物体可以转换。", "确定");
            return;
        }

        // 弹出一个对话框，让用户选择保存预制体的文件夹
        string savePath = EditorUtility.SaveFolderPanel("选择保存预制体的文件夹", "Assets/", "");

        if (string.IsNullOrEmpty(savePath))
        {
            return;
        }

        if (savePath.StartsWith(Application.dataPath))
        {
            savePath = "Assets" + savePath.Substring(Application.dataPath.Length);
        }
        else
        {
            EditorUtility.DisplayDialog("路径错误", "请选择项目Assets文件夹内的路径！", "确定");
            return;
        }

        int createdCount = 0;
        // 遍历目标父物体的所有直接子物体
        foreach (Transform childTransform in parentTransform)
        {
            GameObject childObject = childTransform.gameObject;
            string prefabPath = Path.Combine(savePath, childObject.name + ".prefab");

            // 创建一个新的GameObject，并复制子物体状态，以断开与原预制体的连接
            GameObject objectToPrefab = Object.Instantiate(childObject);
            objectToPrefab.name = childObject.name; // 恢复原始名称

            // 检查同名预制体是否已存在
            if (AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)))
            {
                if (EditorUtility.DisplayDialog("文件已存在", "预制体 '" + childObject.name + "' 已存在。您想覆盖它吗？", "覆盖", "跳过"))
                {
                    CreateNewPrefab(objectToPrefab, prefabPath);
                    createdCount++;
                }
            }
            else
            {
                CreateNewPrefab(objectToPrefab, prefabPath);
                createdCount++;
            }
            
            // 销毁临时的实例
            Object.DestroyImmediate(objectToPrefab);
        }
        
        if (createdCount > 0)
        {
            EditorUtility.DisplayDialog("完成", createdCount + " 个子物体已成功创建为预制体！", "确定");
            AssetDatabase.Refresh();
        }
        else
        {
            EditorUtility.DisplayDialog("操作取消", "没有创建任何新的预制体。", "确定");
        }
    }

    private static void CreateNewPrefab(GameObject objToPrefab, string path)
    {
        // 创建预制体
        PrefabUtility.SaveAsPrefabAsset(objToPrefab, path, out bool success);

        if (success)
        {
            Debug.Log("成功创建预制体：" + path);
        }
        else
        {
            Debug.LogError("创建预制体失败：" + path);
        }
    }

    // 验证函数，只有在Hierarchy中选中了物体时，菜单项才可用
    [MenuItem("Tools/Batch Create Prefabs From Children (Handle Nested)", true)]
    private static bool ValidateCreatePrefabs()
    {
        return Selection.activeGameObject != null;
    }
}