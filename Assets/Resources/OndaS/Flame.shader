Shader "OndaS/Flame"
{
    Properties { _Color ("Tint", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            half4 _Color;
            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }
            half4 frag(Varyings i) : SV_Target
            {
                float y = saturate(i.uv.y);
                float time = _Time.y;
                float sway = (sin(y * 11.0 - time * 4.0) + 0.45 * sin(y * 26.0 + time * 6.0)) * 0.06 * y;
                float x = abs(i.uv.x - 0.5 + sway);
                float turbulence = sin(y * 24.0 - time * 7.0 + sin(i.uv.x * 15.0 + time * 3.0)) * 0.07;
                float width = max(0.0, 0.46 * pow(1.0 - y, 0.7) + turbulence * y);
                float edge = 1.0 - smoothstep(width - 0.08, width + 0.02, x);
                float baseFade = smoothstep(0.0, 0.08, y);
                float inner = (1.0 - smoothstep(width * 0.12, width * 0.78 + 0.01, x)) * (1.0 - smoothstep(0.2, 0.85, y));
                half3 color = lerp(half3(1.0, 0.12, 0.005), half3(1.0, 0.82, 0.20), inner);
                return half4(color * _Color.rgb, edge * baseFade * i.color.a * _Color.a);
            }
            ENDHLSL
        }
    }
}
