Shader "Skybox/FancyScrollBackground"
{
    Properties
    {
        _Tex1 ("Texture1", 2D) = "white" {}
        _Tex2 ("Texture2", 2D) = "white" {}
        _Tex3 ("Texture3", 2D) = "white" {}
        _Tint ("Tint", Color) = (1.0, 1.0, 1.0, 1.0)
        _TexRatios ("Texture Aspect Ratios", Vector) = (1.0, 1.0, 1.0)
        _Progress ("Progress", Float) = 0.5
    }
    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 position : SV_POSITION;
                float4 screenPosition : TEXCOORD0;
            };

            sampler2D _Tex1;
            sampler2D _Tex2;
            sampler2D _Tex3;
            float4 _Tint;
            float4 _Tex1_ST;
            float4 _Tex2_ST;
            float4 _Tex3_ST;
            float3 _TexRatios;
            float _Progress;

            v2f vert(appdata v) 
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.screenPosition = ComputeScreenPos(o.position);
                return o;
            }

            float4 mix(float4 a, float4 b, float p)
            {
                return a * (1 - p) + b * p;
            }

            float4 SampleTexture1(v2f i)
            {
                float2 textureCoordinate = i.screenPosition.xy / i.screenPosition.w;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                if (_TexRatios.x < aspect)
                {
                    textureCoordinate.y = textureCoordinate.y / aspect;
                    textureCoordinate = TRANSFORM_TEX(textureCoordinate, _Tex1);

                    textureCoordinate.y = textureCoordinate.y * _TexRatios.x;

                    float t = 1 / _TexRatios.x - 1 / aspect;
                    textureCoordinate.y += t / 2;
                }
                else
                {
                    textureCoordinate.x = textureCoordinate.x * aspect;
                    textureCoordinate = TRANSFORM_TEX(textureCoordinate, _Tex1);

                    textureCoordinate.x = textureCoordinate.x / _TexRatios.x;

                    float t = _TexRatios.x - aspect;
                    textureCoordinate.x += t / 2;
                }

                return tex2D(_Tex1, textureCoordinate);
            }

            float4 SampleTexture2(v2f i)
            {
                float2 textureCoordinate = i.screenPosition.xy / i.screenPosition.w;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                if (_TexRatios.y < aspect)
                {
                    textureCoordinate.y = textureCoordinate.y / aspect;
                    textureCoordinate = TRANSFORM_TEX(textureCoordinate, _Tex1);

                    textureCoordinate.y = textureCoordinate.y * _TexRatios.y;

                    float t = 1 / _TexRatios.y - 1 / aspect;
                    textureCoordinate.y += t / 2;
                }
                else
                {
                    textureCoordinate.x = textureCoordinate.x * aspect;
                    textureCoordinate = TRANSFORM_TEX(textureCoordinate, _Tex2);

                    textureCoordinate.x = textureCoordinate.x / _TexRatios.y;

                    float t = _TexRatios.y - aspect;
                    textureCoordinate.x += t / 2;
                }

                return tex2D(_Tex2, textureCoordinate);
            }

            float4 SampleTexture3(v2f i)
            {
                float2 textureCoordinate = i.screenPosition.xy / i.screenPosition.w;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                if (_TexRatios.z < aspect)
                {
                    textureCoordinate.y = textureCoordinate.y / aspect;
                    textureCoordinate = TRANSFORM_TEX(textureCoordinate, _Tex3);

                    textureCoordinate.y = textureCoordinate.y * _TexRatios.z;

                    float t = 1 / _TexRatios.z - 1 / aspect;
                    textureCoordinate.y += t / 2;
                }
                else
                {
                    textureCoordinate.x = textureCoordinate.x * aspect;
                    textureCoordinate = TRANSFORM_TEX(textureCoordinate, _Tex1);

                    textureCoordinate.x = textureCoordinate.x / _TexRatios.z;

                    float t = _TexRatios.z - aspect;
                    textureCoordinate.x += t / 2;
                }

                return tex2D(_Tex3, textureCoordinate);
            }

            fixed4 frag(v2f i) : SV_TARGET
            {
                float4 a = SampleTexture1(i);
                float4 b = SampleTexture2(i);
                float4 c = SampleTexture3(i);

                float y = (i.screenPosition.y / i.screenPosition.w) / 2 + 0.25;

                float4 col = mix(a,
                    mix(
                        b,
                        c,
                        step(y, _Progress - 0.25)
                    ),
                    step(y, _Progress + 0.25)
                );

                return col * _Tint;
            }
            ENDCG
        }
    }
}
