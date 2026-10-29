Shader "OndaS/RetroComposite"
{
    Properties
    {
        [PerRendererData] _MainTex ("World", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Levels ("Color levels per channel", Range(4,64)) = 32
        _Dither ("Dither strength", Range(0,1)) = 0.08
        _Quantization ("Color reduction strength", Range(0,1)) = 0.3
        _Desaturation ("Desaturation", Range(0,1)) = 0.12
        _Vignette ("Vignette", Range(0,1)) = 0.08
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest Always
        Blend One Zero
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            float _Levels, _Dither, _Quantization, _Desaturation, _Vignette;
            v2f vert(appdata v) { v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color; return o; }
            fixed4 frag(v2f i) : SV_Target
            {
                float2 pixel = floor(i.uv * _MainTex_TexelSize.zw);
                float2 p = fmod(pixel, 4.0);
                // Ordered 4x4 Bayer matrix, evaluated at the internal pixel resolution.
                float d = 0;
                if (p.y < 1) d = p.x < 1 ? 0 : (p.x < 2 ? 8 : (p.x < 3 ? 2 : 10));
                else if (p.y < 2) d = p.x < 1 ? 12 : (p.x < 2 ? 4 : (p.x < 3 ? 14 : 6));
                else if (p.y < 3) d = p.x < 1 ? 3 : (p.x < 2 ? 11 : (p.x < 3 ? 1 : 9));
                else d = p.x < 1 ? 15 : (p.x < 2 ? 7 : (p.x < 3 ? 13 : 5));
                float3 c = tex2D(_MainTex, i.uv).rgb * _Color.rgb * i.color.rgb;
                float lum = dot(c, float3(0.299, 0.587, 0.114));
                c = lerp(c, lum.xxx, _Desaturation);
                float2 v = i.uv * 2.0 - 1.0;
                c *= 1.0 - saturate(dot(v,v) * 0.5) * _Vignette;
                float3 quantized = floor(saturate(c) * (_Levels - 1.0) + 0.5 + ((d + 0.5) / 16.0 - 0.5) * _Dither) / (_Levels - 1.0);
                c = lerp(c, quantized, _Quantization);
                return fixed4(c, 1);
            }
            ENDCG
        }
    }
}
