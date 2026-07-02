// Holographic card shader for URP (Unity 6)
Shader "Custom/HolographicCard"
{
    Properties
    {
        _MainTex         ("Card Texture (RGBA)", 2D)         = "white" {}
        _HoloIntensity   ("Holo Intensity",   Range(0, 3))   = 1.2
        _HoloSpeed       ("Holo Speed",       Range(0, 5))   = 0.8
        _RainbowScale    ("Rainbow Scale",    Range(0.1,10)) = 4.0
        _GlitterIntensity("Glitter Intensity",Range(0, 3))   = 1.0
        _GlitterScale    ("Glitter Scale",    Range(1, 200)) = 80.0
        _FresnelPower    ("Fresnel Power",    Range(0.1, 8)) = 2.5
        _ScanlineStrength("Scanline Strength",Range(0, 0.2)) = 0.05
        _AlphaCutoff     ("Alpha Cutoff",     Range(0, 1))   = 0.05
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "Queue"          = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float3 viewDirWS  : TEXCOORD2;
                float  fogFactor  : TEXCOORD3;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float  _HoloIntensity;
                float  _HoloSpeed;
                float  _RainbowScale;
                float  _GlitterIntensity;
                float  _GlitterScale;
                float  _FresnelPower;
                float  _ScanlineStrength;
                float  _AlphaCutoff;
            CBUFFER_END

            float3 HC_HsvToRgb(float3 hsv)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(hsv.xxx + K.xyz) * 6.0 - K.www);
                return hsv.z * lerp(K.xxx, saturate(p - K.xxx), hsv.y);
            }

            float HC_Hash21(float2 p)
            {
                p = frac(p * float2(234.34, 435.345));
                p += dot(p, p + 34.23);
                return frac(p.x * p.y);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs    = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   normalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);

                OUT.positionCS = posInputs.positionCS;
                OUT.uv         = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.normalWS   = normalInputs.normalWS;
                OUT.viewDirWS  = GetWorldSpaceViewDir(posInputs.positionWS);
                OUT.fogFactor  = ComputeFogFactor(posInputs.positionCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 base = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                clip(base.a - _AlphaCutoff);

                float3 N     = normalize(IN.normalWS);
                float3 V     = normalize(IN.viewDirWS);
                float  NdotV = saturate(dot(N, V));

                float hue = frac(NdotV * _RainbowScale
                                 + IN.uv.y * 0.4
                                 + _Time.y * _HoloSpeed * 0.08);
                float3 rainbow = HC_HsvToRgb(float3(hue, 0.85, 1.0));

                float fresnel = pow(1.0 - NdotV, _FresnelPower);

                float2 gUV    = IN.uv * _GlitterScale;
                float  gHash  = HC_Hash21(floor(gUV));
                float  gAnim  = saturate(sin(_Time.y * (gHash * 12.0 + 2.0)) * 0.5 + 0.5);
                float  glitter = step(0.96, gHash) * gAnim * _GlitterIntensity;

                float scanline = 1.0 - _ScanlineStrength
                                 * (sin(IN.uv.y * 80.0 + _Time.y * _HoloSpeed * 2.0) * 0.5 + 0.5);

                float  holoMask  = (fresnel * 0.6 + 0.4) * base.a;
                float3 holoLayer = rainbow * _HoloIntensity * holoMask;
                float3 finalRGB  = (base.rgb + holoLayer + glitter) * scanline;

                half4 result = half4(finalRGB, base.a);
                result.rgb   = MixFog(result.rgb, IN.fogFactor);
                return result;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
