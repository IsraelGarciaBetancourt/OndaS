Shader "OndaS/ItemLit"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _BaseColor ("Color", Color) = (1,1,1,1)
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        [Header(Item Highlight)]
        _HighlightColor ("Highlight Color", Color) = (0.88,0.91,0.82,1)
        _HighlightIntensity ("Highlight Intensity", Range(0,2)) = 0.65
        _HighlightDistance ("Highlight Distance (m)", Float) = 5
        _HighlightFade ("Distance Fade (m)", Float) = 1.5
        _HighlightPulseSpeed ("Pulse Speed", Range(0,4)) = 1.5
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Cull [_Cull]
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _HighlightColor;
            float _HighlightIntensity;
            float _HighlightDistance;
            float _HighlightFade;
            float _HighlightPulseSpeed;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; float2 uv:TEXCOORD2; half4 color:COLOR; };
            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs p=GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS=p.positionCS; output.positionWS=p.positionWS;
                output.normalWS=TransformObjectToWorldNormal(input.normalOS);
                output.uv=TRANSFORM_TEX(input.uv,_BaseMap); output.color=input.color;
                return output;
            }
            half4 frag(Varyings input):SV_Target
            {
                half3 normal=normalize(input.normalWS);
                Light main=GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half3 illumination=SampleSH(normal)+main.color*saturate(dot(normal,main.direction))*main.shadowAttenuation;
                uint count=GetAdditionalLightsCount();
                for(uint i=0u;i<count;i++)
                {
                    Light l=GetAdditionalLight(i,input.positionWS);
                    illumination+=l.color*saturate(dot(normal,l.direction))*l.distanceAttenuation;
                }
                half4 surface=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv)*_BaseColor*input.color;
                float3 toCamera=GetCameraPositionWS()-input.positionWS;
                float cameraDistance=length(toCamera);
                float range=max(_HighlightDistance,0.001);
                float fade=clamp(_HighlightFade,0.001,range);
                half proximity=(1-smoothstep(range-fade,range,cameraDistance))*step(0.001,_HighlightDistance);
                half rim=pow(1-saturate(abs(dot(normal,toCamera/max(cameraDistance,0.001)))),2.5);
                half pulse=0.9+0.1*sin(_Time.y*_HighlightPulseSpeed);
                half strength=proximity*_HighlightIntensity*pulse;
                // A neutral edge and a little texture-preserving fill help locate
                // nearby items even in darkness. The normal depth test remains active.
                half3 highlight=_HighlightColor.rgb*rim*0.7+surface.rgb*0.12;
                return half4(surface.rgb*illumination+highlight*strength,1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}
