Shader "Other/Downsample"
{
    Properties
    {
        _MainTex("Backbuffer", 2D) = "white" {}
    }

    SubShader
    {
        Tags {"PreviewType" = "Plane"}
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = ComputeScreenPos(o.vertex);
                return o;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            static const int iBlurSamples = 4;

            // Makes the blur smoother
            #define SMOOTH

#ifdef SMOOTH
            #define SAMPLEFUNC tex2Davg
            half4 tex2Davg(sampler2D sp, float2 uv)
            {
                half4 col = tex2D(sp, uv);
                col += tex2D(sp, uv + (_MainTex_TexelSize.xy * float2(1, 1)));
                col += tex2D(sp, uv + (_MainTex_TexelSize.xy * float2(-1, -1)));
                col += tex2D(sp, uv + (_MainTex_TexelSize.xy * float2(-1, 1)));
                col += tex2D(sp, uv + (_MainTex_TexelSize.xy * float2(1, -1)));

                return col / 5;
            }
#else
            #define SAMPLEFUNC tex2D
#endif

            half4 blur(sampler2D sp, float2 uv, float scale) 
            {
                float2 ps = _MainTex_TexelSize * scale;

                static const float kernel[9] = {
                    0.0269955, 0.0647588, 0.120985, 0.176033, 0.199471, 0.176033, 0.120985, 0.0647588, 0.0269955
                };
                static const float accum = 1.02352;

                float gaussian_weight = 0.0;
                half3 col = 0.0;

                [unroll]
                for (int x = -iBlurSamples; x <= iBlurSamples; ++x) {
                    [unroll]
                    for (int y = -iBlurSamples; y <= iBlurSamples; ++y) {
                        gaussian_weight = kernel[x + iBlurSamples] * kernel[y + iBlurSamples];
                        col += SAMPLEFUNC(sp, uv + ps * float2(x, y)).rgb * gaussian_weight;
                    }
                }

                return fixed4(col * accum, 1);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = blur(_MainTex, i.uv, 1); //tex2D(_CameraOpaqueTexture, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
