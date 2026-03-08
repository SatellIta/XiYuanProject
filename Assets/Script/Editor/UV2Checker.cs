using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class UV2Checker : EditorWindow
{
    [MenuItem("Tools/检测静态物体是否缺失 UV2")]
    public static void CheckUV2()
    {
        // 获取场景中所有带渲染器的物体
        MeshFilter[] allMeshFilters = GameObject.FindObjectsOfType<MeshFilter>();
        List<GameObject> missingUV2Objects = new List<GameObject>();

        foreach (var mf in allMeshFilters)
        {
            // 检查是否被勾选为 Contribute GI (静态光照贡献)
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(mf.gameObject);
            if ((flags & StaticEditorFlags.ContributeGI) != 0)
            {
                Mesh mesh = mf.sharedMesh;
                if (mesh != null)
                {
                    // 检查第二套 UV (uv2) 是否为空
                    if (mesh.uv2 == null || mesh.uv2.Length == 0)
                    {
                        missingUV2Objects.Add(mf.gameObject);
                    }
                }
            }
        }

        // 输出结果
        if (missingUV2Objects.Count > 0)
        {
            Debug.LogWarning($"发现 {missingUV2Objects.Count} 个静态物体缺失 UV2！");
            foreach (var obj in missingUV2Objects)
            {
                Debug.Log("- " + obj.name, obj); // 点击日志可直接在场景中定位物体
            }
            // 自动选中这些“问题物体”方便批量处理
            Selection.objects = missingUV2Objects.ToArray();
        }
        else
        {
            Debug.Log("太棒了！场景中所有静态物体都拥有 UV2。");
        }
    }
}