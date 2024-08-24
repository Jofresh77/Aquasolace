Shader "Custom/UIBlurCircle"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BlurAmount ("Blur Amount", Range(0, 20)) = 5
        _CircleRadius ("Circle Radius", Range(0, 1)) = 0.5
        _EdgeSoftness ("Edge Softness", Range(0, 0.5)) = 0.01
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _BlurAmount;
            float _CircleRadius;
            float _EdgeSoftness;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float4 color = tex2D(_MainTex, IN.texcoord);
                
                // Simple blur effect
                float2 texelSize = 1.0 / _ScreenParams.xy;
                for(int i = -2; i <= 2; i++)
                {
                    for(int j = -2; j <= 2; j++)
                    {
                        color += tex2D(_MainTex, IN.texcoord + float2(i, j) * texelSize * _BlurAmount);
                    }
                }
                color /= 25.0;
                
                // Apply circular mask
                float2 center = float2(0.5, 0.5);
                float dist = distance(IN.texcoord, center);
                float circle = 1 - smoothstep(_CircleRadius - _EdgeSoftness, _CircleRadius + _EdgeSoftness, dist);
                
                color.a *= circle;
                
                return color * IN.color;
            }
            ENDCG
        }
    }
}