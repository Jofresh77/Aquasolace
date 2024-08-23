Shader "Custom/UIBlurCircle"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Radius ("Blur Radius", Range(0, 64)) = 10
        _CircleRadius ("Circle Radius", Range(0, 1)) = 0.5
        _EdgeSoftness ("Edge Softness", Range(0, 0.5)) = 0.01
        _BlurStrength ("Blur Strength", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        GrabPass { "_BackgroundTexture" }

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade

        sampler2D _MainTex;
        sampler2D _BackgroundTexture;
        float _Radius;
        float _CircleRadius;
        float _EdgeSoftness;
        float _BlurStrength;

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            
            float2 center = float2(0.5, 0.5);
            float dist = distance(IN.uv_MainTex, center);
            
            float circle = 1 - smoothstep(_CircleRadius - _EdgeSoftness, _CircleRadius + _EdgeSoftness, dist);
            
            float4 blurredColor = float4(0,0,0,0);
            float totalWeight = 0;
            
            for (int i = -_Radius; i <= _Radius; i++)
            {
                for (int j = -_Radius; j <= _Radius; j++)
                {
                    float2 offset = float2(i, j) * 0.01;
                    float weight = 1.0 / (1 + length(offset));
                    blurredColor += tex2D(_BackgroundTexture, screenUV + offset) * weight;
                    totalWeight += weight;
                }
            }
            blurredColor /= totalWeight;
            
            float4 texColor = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = lerp(texColor.rgb, blurredColor.rgb, _BlurStrength);
            o.Alpha = texColor.a * circle;
        }
        ENDCG
    }
    FallBack "Diffuse"
}