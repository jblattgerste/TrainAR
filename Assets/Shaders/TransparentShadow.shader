Shader "Custom/TransparentShadow"
{
    Properties
    {
        _ShadowIntensity("Shadow Intensity", Range(0, 1)) = 0.6
    }

    SubShader
    {
        // Keep the original plane's ordering and depth so virtual objects occlude correctly.
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "AlphaTest" "RenderType" = "Transparent" }

        Pass
        {
            Name "Shadow Catcher"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask RGB

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half _ShadowIntensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                    float4 shadowCoord = float4(GetNormalizedScreenSpaceUV(input.positionCS), 0, 1);
                #else
                    float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                #endif
                Light mainLight = GetMainLight(shadowCoord);
                half shadow = (1 - mainLight.shadowAttenuation) * (1 - GetMainLightShadowFade(input.positionWS));
                return half4(0, 0, 0, saturate(shadow) * _ShadowIntensity);
            }
            ENDHLSL
        }
    }
}
