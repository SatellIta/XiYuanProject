using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))] // 【关键修正】强制要求此组件
public class AutoTiling : MonoBehaviour
{
    public enum TilingAxis { XZ_Floor, XY_Wall, YZ_Wall }

    [Header("平铺设置")]
    [Tooltip("选择平铺的平面类型。")]
    public TilingAxis axis = TilingAxis.XZ_Floor;

    [Tooltip("基础平铺密度。1表示1米平铺1次。")]
    public float textureDensity = 1.0f;

    void OnValidate()
    {
        #if UNITY_EDITOR
        EditorApplication.delayCall += ApplyTiling;
        #endif
    }

    void Update()
    {
        #if UNITY_EDITOR
        if (!Application.isPlaying && transform.hasChanged)
        {
            ApplyTiling();
            transform.hasChanged = false;
        }
        #endif
    }

    private void ApplyTiling()
    {
        if (this == null) return;

        var objectRenderer = GetComponent<Renderer>();
        var meshFilter = GetComponent<MeshFilter>();

        // --- 更稳健的检查 ---
        if (objectRenderer == null || meshFilter == null)
        {
            // 由于 RequireComponent 的存在，这几乎不可能发生，但作为保险
            return;
        }
        if (meshFilter.sharedMesh == null)
        {
            // MeshFilter 中还没有指定网格
            return;
        }
        if (objectRenderer.sharedMaterial == null)
        {
            // Renderer 中还没有指定材质，给出友好提示
            Debug.LogWarning($"[AutoTiling] 请为对象 '{this.gameObject.name}' 的 Mesh Renderer 指定一个材质，以激活自动平铺功能。", this);
            return;
        }
        
        // 如果所有检查都通过，才继续执行核心逻辑
        #if UNITY_EDITOR
        Material materialToModify = GetOrCreateMaterialAssetForObject(objectRenderer);
        #else
        Material materialToModify = objectRenderer.material;
        #endif

        if (materialToModify == null || materialToModify.mainTexture == null) return;

        // --- 应用平铺计算 ---
        Vector3 meshSize = meshFilter.sharedMesh.bounds.size;
        Vector3 worldScale = transform.lossyScale;
        Vector3 realWorldDimensions = new Vector3(meshSize.x * worldScale.x, meshSize.y * worldScale.y, meshSize.z * worldScale.z);
        Vector2 newTiling = Vector2.one;
        switch (axis) {
            case TilingAxis.XZ_Floor: newTiling.x = realWorldDimensions.x; newTiling.y = realWorldDimensions.z; break;
            case TilingAxis.XY_Wall: newTiling.x = realWorldDimensions.x; newTiling.y = realWorldDimensions.y; break;
            case TilingAxis.YZ_Wall: newTiling.x = realWorldDimensions.z; newTiling.y = realWorldDimensions.y; break;
        }
        newTiling *= textureDensity;

        if (materialToModify.mainTextureScale != newTiling)
        {
            materialToModify.mainTextureScale = newTiling;
            #if UNITY_EDITOR
            if(AssetDatabase.Contains(materialToModify))
            {
                EditorUtility.SetDirty(materialToModify);
            }
            #endif
        }
    }

#if UNITY_EDITOR
    private Material GetOrCreateMaterialAssetForObject(Renderer objectRenderer)
    {
        string currentMatPath = AssetDatabase.GetAssetPath(objectRenderer.sharedMaterial);
        if (!string.IsNullOrEmpty(currentMatPath) && currentMatPath.Contains($"_{this.gameObject.name}_AutoTiled.mat"))
        {
            return objectRenderer.sharedMaterial;
        }

        string prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(this.gameObject);
        if (string.IsNullOrEmpty(prefabAssetPath))
        {
            if (objectRenderer.sharedMaterial.name.EndsWith("(Instance)")) return objectRenderer.sharedMaterial;
            return objectRenderer.material;
        }

        string prefabName = Path.GetFileNameWithoutExtension(prefabAssetPath);
        string directory = Path.GetDirectoryName(prefabAssetPath);
        string originalMatName = objectRenderer.sharedMaterial.name;
        string newMatName = $"{prefabName}_{this.gameObject.name}_{originalMatName}_AutoTiled.mat";
        foreach (char c in Path.GetInvalidFileNameChars()) { newMatName = newMatName.Replace(c, '_'); }
        string newMatPath = Path.Combine(directory, newMatName);

        Material managedMaterial = AssetDatabase.LoadAssetAtPath<Material>(newMatPath);

        if (managedMaterial == null)
        {
            managedMaterial = new Material(objectRenderer.sharedMaterial);
            managedMaterial.name = Path.GetFileNameWithoutExtension(newMatName);
            AssetDatabase.CreateAsset(managedMaterial, newMatPath);
        }

        if (objectRenderer.sharedMaterial != managedMaterial)
        {
            objectRenderer.sharedMaterial = managedMaterial;
            EditorUtility.SetDirty(this.gameObject);
        }
        return managedMaterial;
    }
#endif
}