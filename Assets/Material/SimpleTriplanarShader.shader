Shader "Custom/SimpleTriplanarShader_WithColor"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1,1,1,1) // 1. 添加颜色属性 (默认是白色)
        _Tiling ("Tiling", Float) = 1.0
        _Blend ("Blend Sharpness", Range(1, 10)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color; // 2. 声明颜色变量
        float _Tiling;
        float _Blend;

        // Input 结构体现在必须包含 INTERNAL_DATA, 否则会在某些 Unity 版本中报错
        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // 将世界法线转换为权重 (绝对值确保方向不影响)
            float3 blendWeights = abs(IN.worldNormal);
            // 提高权重差异，使混合更清晰
            blendWeights = pow(blendWeights, _Blend);
            // 归一化权重，确保总和为1
            blendWeights = blendWeights / (blendWeights.x + blendWeights.y + blendWeights.z + 0.00001); // 增加一个极小值避免除以零

            // 根据世界坐标和Tiling计算三个方向的UV
            float2 uvX = IN.worldPos.yz * _Tiling;
            float2 uvY = IN.worldPos.xz * _Tiling;
            float2 uvZ = IN.worldPos.xy * _Tiling;

            // 从三个方向采样纹理
            fixed4 colX = tex2D(_MainTex, uvX);
            fixed4 colY = tex2D(_MainTex, uvY);
            fixed4 colZ = tex2D(_MainTex, uvZ);

            // 根据权重混合三个方向的颜色
            fixed4 finalColor = colX * blendWeights.x + colY * blendWeights.y + colZ * blendWeights.z;

            // 3. 将颜色与贴图混合 (相乘)
            o.Albedo = finalColor.rgb * _Color.rgb;
            
            // 为了安全，将其他PBR参数设置为固定的默认值
            o.Metallic = 0.0;
            o.Smoothness = 0.3;
            o.Occlusion = 1.0;
            
            o.Alpha = finalColor.a * _Color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}