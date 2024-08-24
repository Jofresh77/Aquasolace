Shader "Custom/MaskedBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0, 20)) = 5
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade

        sampler2D _MainTex;
        sampler2D _MaskTex;
        float _BlurSize;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_MaskTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            float4 c = tex2D(_MainTex, IN.uv_MainTex);
            float4 mask = tex2D(_MaskTex, IN.uv_MaskTex);
            
            // Apply blur based on mask
            if (mask.a > 0)
            {
                float4 blurredColor = float4(0,0,0,0);
                float totalWeight = 0;
                for (int x = -5; x <= 5; x++)
                {
                    for (int y = -5; y <= 5; y++)
                    {
                        float2 offset = float2(x, y) * _BlurSize / 1000.0;
                        float weight = 1.0 / (1 + x*x + y*y);
                        blurredColor += tex2D(_MainTex, IN.uv_MainTex + offset) * weight;
                        totalWeight += weight;
                    }
                }
                blurredColor /= totalWeight;
                c = lerp(c, blurredColor, mask.a);
            }

            o.Albedo = c.rgb;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}