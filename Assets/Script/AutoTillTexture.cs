using UnityEngine;

[ExecuteAlways]
public class AutoTiling : MonoBehaviour
{
    public enum TilingAxis { XY, XZ, YZ }

    [Header("平铺设置")]
    [Tooltip("选择使用哪个物体的轴向来计算平铺。")]
    public TilingAxis axis = TilingAxis.XZ;

    [Tooltip("基础平铺密度。数值越大，纹理重复越多。1表示1米平铺1次。")]
    public float textureDensity = 1.0f;

    // 私有变量
    private Renderer objectRenderer;
    private Material materialInstance;
    private Vector3 previousScale;

    // OnEnable 在对象激活时调用
    void OnEnable()
    {
        // 确保只在需要时获取组件和创建实例
        ApplyTiling();
    }
    
    // 在对象被禁用或销毁前调用
    void OnDisable()
    {
        // 检查 materialInstance 是否是创建的副本
        // 并且检查渲染器是否仍然使用这个实例
        if (materialInstance != null && objectRenderer != null && objectRenderer.sharedMaterial == materialInstance)
        {
            // 销毁创建的实例
            DestroyImmediate(materialInstance);
        }
    }


    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (transform.lossyScale != previousScale)
            {
                ApplyTiling();
                previousScale = transform.lossyScale;
            }
        }
#endif
    }

    void ApplyTiling()
    {
        if (objectRenderer == null)
        {
            objectRenderer = GetComponent<Renderer>();
        }
        
        if (objectRenderer == null || objectRenderer.sharedMaterial == null) return;
        
        // 如果还没有实例，或者实例丢失了（例如，撤销操作），就创建一个
        if (materialInstance == null)
        {
            materialInstance = new Material(objectRenderer.sharedMaterial);
            objectRenderer.material = materialInstance;
        }

        if (materialInstance.mainTexture == null) return;

        Vector3 worldScale = transform.lossyScale;
        Vector2 newTiling = Vector2.one;

        switch (axis)
        {
            case TilingAxis.XY:
                newTiling.x = worldScale.x;
                newTiling.y = worldScale.y;
                break;
            case TilingAxis.XZ:
                newTiling.x = worldScale.x;
                newTiling.y = worldScale.z;
                break;
            case TilingAxis.YZ:
                newTiling.x = worldScale.y;
                newTiling.y = worldScale.z;
                break;
        }

        newTiling *= textureDensity;
        
        materialInstance.mainTextureScale = newTiling;
    }
}