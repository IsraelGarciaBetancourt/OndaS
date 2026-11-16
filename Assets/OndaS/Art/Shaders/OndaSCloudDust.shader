Shader "OndaS/CloudDust"
{
    Properties
    {
        _MainTex ("Dust texture (legacy)", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        Pass
        {
            Name "ForwardDust"
            Tags { "LightMode"="UniversalForward" }
            Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD1; float2 uv : TEXCOORD0; half4 color : COLOR; };
            Varyings vert(Attributes v)
            {
                Varyings o; o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS); o.uv = v.uv; o.color = v.color; return o;
            }
            float hash21(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            float noise2(float2 p)
            {
                float2 cell = floor(p), f = frac(p); f = f*f*(3.0-2.0*f);
                return lerp(lerp(hash21(cell),hash21(cell+float2(1,0)),f.x),
                            lerp(hash21(cell+float2(0,1)),hash21(cell+1.0),f.x),f.y);
            }
            half4 frag(Varyings i) : SV_Target
            {
                // Broad, softly mottled haze: no hard silhouette or repeated cloud picture.
                float2 uv = i.uv;
                float edge = smoothstep(0.0, .32, uv.x) * smoothstep(0.0, .32, 1.0-uv.x)
                           * smoothstep(0.0, .32, uv.y) * smoothstep(0.0, .32, 1.0-uv.y);
                float2 drift = _Time.y * float2(.006, .003);
                float mottling = .54 + .32 * noise2(uv * 3.7 + drift) + .14 * noise2(uv * 8.3 - drift);
                InputData inputData = (InputData)0;
                inputData.positionWS = i.positionWS;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);
                half3 illumination = max(SampleSH(half3(0,1,0)), 0) * .45;
                Light mainLight = GetMainLight();
                illumination += mainLight.color * mainLight.distanceAttenuation * .4;
                #if defined(_ADDITIONAL_LIGHTS)
                    uint lightCount = GetAdditionalLightsCount();
                    #if USE_CLUSTER_LIGHT_LOOP
                        [loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
                        {
                            CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK
                            Light light = GetAdditionalLight(lightIndex, i.positionWS, half4(1,1,1,1));
                            illumination += light.color * light.distanceAttenuation * light.shadowAttenuation;
                        }
                    #endif
                    LIGHT_LOOP_BEGIN(lightCount)
                        Light light = GetAdditionalLight(lightIndex, i.positionWS, half4(1,1,1,1));
                        illumination += light.color * light.distanceAttenuation * light.shadowAttenuation;
                    LIGHT_LOOP_END
                #endif
                half brightness = saturate(max(illumination.r, max(illumination.g, illumination.b)) * 2.0);
                half3 dustColor = i.color.rgb * _Color.rgb * min(illumination * 2.0, half3(2,2,2));
                return half4(dustColor, i.color.a * _Color.a * edge * mottling * brightness);
            }
            ENDHLSL
        }
    }
}
