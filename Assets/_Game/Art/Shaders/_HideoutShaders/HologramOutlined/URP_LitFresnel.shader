Shader "Custom/URP_LitFresnel"
{
    Properties
    {
        [Header(Surface Maps)]
        _BaseMap ("Albedo (RGB)", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Float) = 1.0

        _MetallicGlossMap ("Metallic (R) Smoothness (A)", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5

        _OcclusionMap ("Occlusion", 2D) = "white" {}
        _OcclusionStrength ("Occlusion Strength", Range(0,1)) = 1.0

        [Header(Fresnel Outline)]
        _RimColor ("Rim Color", Color) = (0.3, 0.8, 1.0, 1.0)
        _RimPower ("Rim Power", Range(0.1, 10)) = 3.0
        _RimIntensity ("Rim Intensity", Range(0, 10)) = 2.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                float4 tangentWS   : TEXCOORD3;
                float3 viewDirWS   : TEXCOORD4;
                float4 shadowCoord : TEXCOORD5;
            };

            TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);
            TEXTURE2D(_MetallicGlossMap);    SAMPLER(sampler_MetallicGlossMap);
            TEXTURE2D(_OcclusionMap);        SAMPLER(sampler_OcclusionMap);

            float4 _BaseMap_ST;
            float4 _BaseColor;
            float _BumpScale;
            float _Metallic;
            float _Smoothness;
            float _OcclusionStrength;

            float4 _RimColor;
            float _RimPower;
            float _RimIntensity;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);

                OUT.positionHCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.normalWS = normalInputs.normalWS;
                OUT.tangentWS = float4(normalInputs.tangentWS, IN.tangentOS.w);
                OUT.viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.shadowCoord = GetShadowCoord(positionInputs);

                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // --- Sample maps ---
                half4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;

                half4 metallicGloss = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, IN.uv);
                half metallic = metallicGloss.r * _Metallic;
                half smoothness = metallicGloss.a * _Smoothness;

                half occlusion = lerp(1.0, SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, IN.uv).g, _OcclusionStrength);

                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uv), _BumpScale);
                float3 bitangentWS = cross(IN.normalWS, IN.tangentWS.xyz) * IN.tangentWS.w;
                float3x3 tangentToWorld = float3x3(IN.tangentWS.xyz, bitangentWS, IN.normalWS);
                float3 normalWS = normalize(mul(normalTS, tangentToWorld));

                float3 viewDirWS = normalize(IN.viewDirWS);

                // --- Fresnel rim term ---
                half rim = 1.0 - saturate(dot(viewDirWS, normalWS));
                half fresnel = pow(rim, _RimPower) * _RimIntensity;
                half3 rimEmission = _RimColor.rgb * fresnel;

                // --- Build SurfaceData / InputData for URP's PBR lighting ---
                SurfaceData surfaceData;
                surfaceData.albedo = baseTex.rgb;
                surfaceData.metallic = metallic;
                surfaceData.specular = half3(0,0,0);
                surfaceData.smoothness = smoothness;
                surfaceData.normalTS = normalTS;
                surfaceData.emission = rimEmission;
                surfaceData.occlusion = occlusion;
                surfaceData.alpha = baseTex.a;
                surfaceData.clearCoatMask = 0;
                surfaceData.clearCoatSmoothness = 0;

                InputData inputData;
                inputData.positionWS = IN.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewDirWS;
                inputData.shadowCoord = IN.shadowCoord;
                inputData.fogCoord = 0;
                inputData.vertexLighting = half3(0,0,0);
                inputData.bakedGI = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV = 0;
                inputData.shadowMask = half4(1,1,1,1);

                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                return color;
            }
            ENDHLSL
        }

        // Needed so the object still casts shadows correctly
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
