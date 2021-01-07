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
                float2 texUv1 : TEXCOORD1;
                float2 texUv2 : TEXCOORD2;
                float2 texUv3 : TEXCOORD3;
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

            float2 calcUv(float aspectRatio, float4 screenPos)
            {
                float2 textureCoordinate = screenPos.xy / screenPos.w;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                if (aspectRatio < aspect)
                {
                    textureCoordinate.y = textureCoordinate.y / aspect;
                    textureCoordinate = TRANSFORM_TEX(textureCoordinate, _Tex1);

                    textureCoordinate.y = textureCoordinate.y * aspectRatio;

                    float t = 1 / aspectRatio - 1 / aspect;
                    textureCoordinate.y += t / 2;
                }
                else
                {
                    textureCoordinate.x = textureCoordinate.x * aspect;
                    textureCoordinate = TRANSFORM_TEX(textureCoordinate, _Tex1);

                    textureCoordinate.x = textureCoordinate.x / aspectRatio;

                    float t = aspectRatio - aspect;
                    textureCoordinate.x += t / 2;
                }

                return textureCoordinate;
            }

            v2f vert(appdata v) 
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                float4 screenPos = ComputeScreenPos(o.position);

                o.texUv1 = calcUv(_TexRatios.x, screenPos);
                o.texUv2 = calcUv(_TexRatios.y, screenPos);
                o.texUv3 = calcUv(_TexRatios.z, screenPos);

                o.screenPosition = screenPos;
                return o;
            }

            float4 mix(float4 a, float4 b, float p)
            {
                return a * (1 - p) + b * p;
            }

            fixed4 frag(v2f i) : SV_TARGET
            {
                float4 a = tex2D(_Tex1, i.texUv1);
                float4 b = tex2D(_Tex2, i.texUv2);
                float4 c = tex2D(_Tex3, i.texUv3);

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
