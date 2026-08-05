Shader "Custom/URP/BrokenTVScreen"
{
    Properties
    {
        [Header(Base)]
        _BaseMap ("Screen Texture", 2D) = "white" {}
        _BaseColor ("Tint", Color) = (1,1,1,1)

        [Header(Static Noise)]
        _NoiseIntensity ("Noise Intensity", Range(0,1)) = 0.15
        _NoiseSpeed ("Noise Speed", Float) = 20.0

        [Header(Scanlines)]
        _ScanlineCount ("Scanline Count", Float) = 300
        _ScanlineIntensity ("Scanline Intensity", Range(0,1)) = 0.3
        _ScanlineSpeed ("Scanline Scroll Speed", Float) = 0.5

        [Header(Glitch)]
        _GlitchAmount ("Horizontal Glitch Amount", Range(0,1)) = 0.3
        _GlitchSpeed ("Glitch Speed", Float) = 5.0
        _GlitchBlockCount ("Glitch Block Count", Float) = 12

        [Header(Chromatic Aberration)]
        _RGBSplit ("RGB Split Amount", Range(0,0.05)) = 0.006

        [Header(Flicker)]
        _FlickerIntensity ("Flicker Intensity", Range(0,1)) = 0.2
        _FlickerSpeed ("Flicker Speed", Float) = 8.0

        [Header(Vignette And Curvature)]
        _VignetteIntensity ("Vignette Intensity", Range(0,2)) = 0.8
        _Curvature ("Screen Curvature", Range(0,0.5)) = 0.1
        _VignetteColor ("Neon Vignette Color", Color) = (1,0,1,1)

        [Header(Roll)]
        _RollSpeed ("Vertical Roll Speed", Float) = 0.0
        _RollBandHeight ("Roll Band Height", Range(0,1)) = 0.15

        [Header(Cyberpunk Grading)]
        _ShadowColor ("Shadow Tint (Cyan)", Color) = (0.0, 0.9, 1.0, 1)
        _HighlightColor ("Highlight Tint (Magenta)", Color) = (1.0, 0.0, 0.8, 1)
        _GradingStrength ("Grading Strength", Range(0,1)) = 0.6
        _NeonSaturation ("Saturation Boost", Range(0,3)) = 1.6

        [Header(Neon Glow)]
        _GlowColor ("Glow Color", Color) = (0.4, 1.0, 1.0, 1)
        _GlowIntensity ("Glow Intensity", Range(0,2)) = 0.5
        _GlowThreshold ("Glow Threshold", Range(0,1)) = 0.6

        [Header(Digital Pixelation)]
        _PixelateAmount ("Pixelation (0 = off)", Range(0,1)) = 0.0
        _PixelGridX ("Pixel Grid X", Float) = 160
        _PixelGridY ("Pixel Grid Y", Float) = 90

        [Header(Scan Grid Overlay)]
        _GridColor ("Grid Line Color", Color) = (0.2, 1.0, 1.0, 1)
        _GridIntensity ("Grid Intensity", Range(0,1)) = 0.08
        _GridSize ("Grid Cell Size", Float) = 40
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Overlay" }
        LOD 100

        Pass
        {
            Name "BrokenTV"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _NoiseIntensity;
                float _NoiseSpeed;
                float _ScanlineCount;
                float _ScanlineIntensity;
                float _ScanlineSpeed;
                float _GlitchAmount;
                float _GlitchSpeed;
                float _GlitchBlockCount;
                float _RGBSplit;
                float _FlickerIntensity;
                float _FlickerSpeed;
                float _VignetteIntensity;
                float _Curvature;
                float _RollSpeed;
                float _RollBandHeight;
                float4 _VignetteColor;
                float4 _ShadowColor;
                float4 _HighlightColor;
                float _GradingStrength;
                float _NeonSaturation;
                float4 _GlowColor;
                float _GlowIntensity;
                float _GlowThreshold;
                float _PixelateAmount;
                float _PixelGridX;
                float _PixelGridY;
                float4 _GridColor;
                float _GridIntensity;
                float _GridSize;
            CBUFFER_END

            // Simple hash-based pseudo random
            float rand(float2 co)
            {
                return frac(sin(dot(co, float2(12.9898,78.233))) * 43758.5453);
            }

            float2 barrelDistort(float2 uv, float amount)
            {
                float2 cc = uv - 0.5;
                float dist = dot(cc, cc);
                return uv + cc * dist * amount;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float t = _Time.y;

                // --- Screen curvature (barrel distortion) ---
                uv = barrelDistort(uv, _Curvature);

                // Out of bounds after distortion -> black
                if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
                {
                    return half4(0,0,0,1);
                }

                // --- Vertical roll (like a losing-sync TV) ---
                uv.y = frac(uv.y + t * _RollSpeed);

                // --- Digital pixelation (chunky retro-digital blocks) ---
                if (_PixelateAmount > 0.001)
                {
                    float2 grid = float2(_PixelGridX, _PixelGridY);
                    float2 pixUV = floor(uv * grid) / grid;
                    uv = lerp(uv, pixUV, _PixelateAmount);
                }

                // --- Horizontal block glitch ---
                float blockY = floor(uv.y * _GlitchBlockCount);
                float glitchSeed = floor(t * _GlitchSpeed);
                float glitchRand = rand(float2(blockY, glitchSeed));
                float glitchOffset = (glitchRand - 0.5) * _GlitchAmount * step(0.85, glitchRand);
                uv.x = frac(uv.x + glitchOffset);

                // --- RGB split / chromatic aberration ---
                float2 dirToCenter = normalize(uv - 0.5 + 1e-5);
                float rOff = _RGBSplit;
                half r = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv + dirToCenter * rOff).r;
                half g = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).g;
                half b = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv - dirToCenter * rOff).b;
                half4 col = half4(r, g, b, 1) * _BaseColor;

                // --- Scanlines ---
                float scan = sin((uv.y * _ScanlineCount) - t * _ScanlineSpeed * 10.0);
                scan = smoothstep(-1.0, 1.0, scan);
                col.rgb *= lerp(1.0, scan, _ScanlineIntensity);

                // --- Static noise ---
                float2 noiseUV = uv * float2(640, 360) + t * _NoiseSpeed;
                float n = rand(floor(noiseUV));
                col.rgb = lerp(col.rgb, n.xxx, _NoiseIntensity * (0.5 + 0.5 * rand(float2(t,1.0))));

                // --- Random flicker ---
                float flicker = 1.0 - _FlickerIntensity * (0.5 + 0.5 * sin(t * _FlickerSpeed) * rand(float2(floor(t*_FlickerSpeed),0)));
                col.rgb *= flicker;

                // --- Occasional broken sync band (bright/dark horizontal tear) ---
                float bandCenter = frac(t * 0.07);
                float bandDist = abs(uv.y - bandCenter);
                float band = smoothstep(_RollBandHeight, 0.0, bandDist);
                col.rgb += band * 0.4;

                // --- Cyberpunk duotone grading (cyan shadows / magenta highlights) ---
                float luma = dot(col.rgb, float3(0.299, 0.587, 0.114));
                half3 graded = lerp(_ShadowColor.rgb, _HighlightColor.rgb, luma);
                col.rgb = lerp(col.rgb, col.rgb * graded * 1.4, _GradingStrength);

                // --- Extra saturation punch ---
                half satLuma = dot(col.rgb, float3(0.299, 0.587, 0.114));
                col.rgb = lerp(satLuma.xxx, col.rgb, _NeonSaturation);

                // --- Neon bloom-ish glow on bright areas ---
                half brightness = max(col.r, max(col.g, col.b));
                half glowMask = smoothstep(_GlowThreshold, 1.0, brightness);
                col.rgb += _GlowColor.rgb * glowMask * _GlowIntensity;

                // --- Cyber grid overlay (faint neon graph lines) ---
                float2 gridUV = frac(uv * _GridSize);
                float gridLine = step(0.97, gridUV.x) + step(0.97, gridUV.y);
                col.rgb += _GridColor.rgb * saturate(gridLine) * _GridIntensity;

                // --- Vignette tinted with neon color instead of plain black ---
                float2 vc = uv - 0.5;
                float vig = 1.0 - dot(vc, vc) * _VignetteIntensity * 2.0;
                vig = saturate(vig);
                col.rgb = lerp(_VignetteColor.rgb * 0.6, col.rgb, vig);

                return col;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
