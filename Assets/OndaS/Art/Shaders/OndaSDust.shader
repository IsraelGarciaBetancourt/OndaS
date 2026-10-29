Shader "OndaS/Dust"
{
    Properties { _Color ("Color", Color) = (0.75,0.72,0.62,0.3) }
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
            Varyings vert(Attributes v) { Varyings o; o.positionCS=TransformObjectToHClip(v.positionOS.xyz); o.uv=v.uv; o.color=v.color; return o; }
            half4 frag(Varyings i) : SV_Target { half a=saturate((0.5-distance(i.uv, float2(0.5,0.5)))*5); return half4(i.color.rgb*_Color.rgb, i.color.a*_Color.a*a); }
            ENDHLSL
        }
    }
}
