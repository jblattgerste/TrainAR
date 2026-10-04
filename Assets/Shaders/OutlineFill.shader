// QuickOutline, created by Chris Nolet. Copyright (c) 2018 Chris Nolet.
// URP implementation retains the material interface and view-space extrusion.
Shader "Custom/Outline Fill"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0
        _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineWidth("Outline Width", Range(0, 10)) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent+110"
            "RenderType" = "Transparent"
            "DisableBatching" = "True"
        }

        Pass
        {
            Name "Fill"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Off
            ZTest [_ZTest]
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask RGB
            Stencil
            {
                Ref 1
                ReadMask 1
                WriteMask 0
                Comp NotEqual
                Pass Keep
            }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                float _OutlineWidth;
                float _ZTest;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float3 smoothNormalOS : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 normalOS = any(input.smoothNormalOS) ? input.smoothNormalOS : input.normalOS;
                float3 positionVS = TransformWorldToView(TransformObjectToWorld(input.positionOS.xyz));
                // TransformObjectToWorldNormal uses the inverse transpose for non-uniform scales.
                float3 normalVS = normalize(TransformWorldToViewDir(TransformObjectToWorldNormal(normalOS)));
                positionVS += normalVS * -positionVS.z * _OutlineWidth / 1000.0;
                output.positionCS = mul(UNITY_MATRIX_P, float4(positionVS, 1));
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}
