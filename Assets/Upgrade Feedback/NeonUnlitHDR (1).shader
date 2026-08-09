Shader "Custom/URP/NeonUnlitHDR"
{
    // A minimal URP-compatible unlit shader for UI/text elements that need
    // to spike past 1.0 brightness so URP's Bloom post-process picks them up.
    // Works on a UI Image / RawImage material, or a plain Quad/MeshRenderer.

    Properties
    {
        // NOTE: must be named "_MainTex" so Unity's UI Image / Canvas Renderer
        // auto-assigns the Image's Source sprite texture onto this shader.
        _MainTex    ("Texture / Sprite", 2D)      = "white" {}
        [HDR] _BaseColor  ("Base Color (HDR)", Color)  = (1,1,1,1)
        [HDR] _NeonColor  ("Neon Color (HDR)", Color)  = (0,1,1,1)
        _Intensity  ("Neon Intensity", Range(0, 12))  = 1
        _Blend      ("Base <-> Neon Blend", Range(0,1)) = 0

        // Standard UI-friendly blend/stencil-safe render state
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil     ("Stencil ID", Float) = 0
        _StencilOp   ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask", Float) = 255
        _ColorMask   ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "Queue"          = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        ColorMask [_ColorMask]

        Pass
        {
            Name "NeonUnlitHDR"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4  _BaseColor;
                half4  _NeonColor;
                half   _Intensity;
                half   _Blend;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // Blend between the resting color and the neon color, then
                // multiply by intensity. When intensity > 1 the output
                // exceeds the 0-1 range, which is exactly what URP Bloom
                // uses as its threshold trigger.
                half4 col = lerp(_BaseColor, _NeonColor, _Blend);
                col.rgb *= _Intensity;

                half4 final = tex * col * IN.color;
                return final;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
