// Dissolve_URP.shader
// A dissolve effect shader for Unity's Universal Render Pipeline (URP).
// Uses a noise texture to control which pixels are "burned away" and adds
// a glowing edge at the dissolve boundary. Supports basic Lit lighting
// (main light + ambient) so it reacts to your scene lighting.
//
// Requirements: URP package installed (any recent 12.x - 17.x version).
//
// Usage:
//   1. Put this file anywhere in your Assets folder (e.g. Assets/Shaders/Dissolve_URP.shader).
//   2. Create a Material and set its Shader to "Custom/URP/Dissolve".
//   3. Assign a Base Map and a Noise Texture (any grayscale noise, e.g. Perlin noise).
//   4. Animate the "Dissolve Amount" property from 0 -> 1 over time (via script)
//      to dissolve the object, or 1 -> 0 to make it appear.
//
// Example script to drive it:
//   Renderer rend = GetComponent<Renderer>();
//   rend.material.SetFloat("_DissolveAmount", value); // value 0..1

Shader "Custom/URP/Dissolve"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0,1)) = 0

        _EdgeColor ("Edge Color", Color) = (1, 0.4, 0, 1)
        _EdgeWidth ("Edge Width", Range(0.001, 0.5)) = 0.05
        _EdgeEmissionStrength ("Edge Emission Strength", Range(0, 10)) = 3

        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5

        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "TransparentCutout"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "AlphaTest"
        }
        LOD 200
        Cull [_Cull]

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _BaseColor;
            float4 _EdgeColor;
            float _DissolveAmount;
            float _EdgeWidth;
            float _EdgeEmissionStrength;
            float _Cutoff;
        CBUFFER_END

        TEXTURE2D(_BaseMap);   SAMPLER(sampler_BaseMap);
        TEXTURE2D(_NoiseTex);  SAMPLER(sampler_NoiseTex);

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS   : NORMAL;
            float2 uv         : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionHCS : SV_POSITION;
            float2 uv           : TEXCOORD0;
            float3 normalWS     : TEXCOORD1;
            float3 positionWS   : TEXCOORD2;
        };

        Varyings DissolveVert(Attributes IN)
        {
            Varyings OUT;
            VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS.xyz);
            VertexNormalInputs normalInput = GetVertexNormalInputs(IN.normalOS);

            OUT.positionHCS = vertexInput.positionCS;
            OUT.positionWS = vertexInput.positionWS;
            OUT.normalWS = normalInput.normalWS;
            OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
            return OUT;
        }

        half4 DissolveFrag(Varyings IN) : SV_Target
        {
            half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
            half noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, IN.uv).r;

            // Discard pixels whose noise value is below the dissolve threshold.
            half diff = noise - _DissolveAmount;
            clip(diff);

            // Basic lighting: main directional light + ambient (SH).
            Light mainLight = GetMainLight();
            float3 normalWS = normalize(IN.normalWS);
            half3 lighting = mainLight.color * saturate(dot(normalWS, mainLight.direction));
            lighting += SampleSH(normalWS);

            half3 color = tex.rgb * lighting;

            // Glowing edge just above the dissolve threshold.
            half edge = 1 - smoothstep(0, _EdgeWidth, diff);
            color += _EdgeColor.rgb * edge * _EdgeEmissionStrength;

            return half4(color, tex.a);
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex DissolveVert
            #pragma fragment DissolveFrag
            ENDHLSL
        }

        // Needed so the object still casts a (dissolving) shadow correctly.
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex DissolveVert
            #pragma fragment ShadowFrag

            half4 ShadowFrag(Varyings IN) : SV_Target
            {
                half noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, IN.uv).r;
                clip(noise - _DissolveAmount);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
