Shader "Skybox/FancyScrollBackground"
{
    Properties
    {
        _MainTex ("Dummy", 2D) = "white" {}
        _Tex1 ("Texture1", 2D) = "white" {}
        _Tex2 ("Texture2", 2D) = "white" {}
        _Tex3 ("Texture3", 2D) = "white" {}
        _Glitch ("Glitch Noise", 2D) = "black" {}
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
            sampler2D _Glitch;
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

            fixed4 frag(v2f i) : SV_TARGET
            {
                fixed4 glitch = tex2D(_Glitch, i.texUv2);

                fixed4 a = fixed4(0,0,0,1);
                fixed4 b = fixed4(0,0,0,1);
                fixed4 c = fixed4(0,0,0,1);

                fixed y = (i.screenPosition.y / i.screenPosition.w) / 2 + 0.25;

                fixed p = (_Progress - 0.5) * 2;
                fixed ap = max(0, -p);
                fixed cp = max(0, p);
                fixed bp = 1 - ap - cp;

                a.r = tex2D(_Tex1, i.texUv1 + float2(glitch.x - 0.5, 0) * (1 - ap)).r;
                a.g = tex2D(_Tex1, i.texUv1 + float2(glitch.y - 0.5, 0) * (1 - ap)).g;
                a.b = tex2D(_Tex1, i.texUv1 + float2(glitch.z - 0.5, 0) * (1 - ap)).b;

                b.r = tex2D(_Tex2, i.texUv2 + float2(glitch.x - 0.5, 0) * (1 - bp)).r;
                b.g = tex2D(_Tex2, i.texUv2 + float2(glitch.y - 0.5, 0) * (1 - bp)).g;
                b.b = tex2D(_Tex2, i.texUv2 + float2(glitch.z - 0.5, 0) * (1 - bp)).b;

                c.r = tex2D(_Tex3, i.texUv3 + float2(glitch.x - 0.5, 0) * (1 - cp)).r;
                c.g = tex2D(_Tex3, i.texUv3 + float2(glitch.y - 0.5, 0) * (1 - cp)).g;
                c.b = tex2D(_Tex3, i.texUv3 + float2(glitch.z - 0.5, 0) * (1 - cp)).b;

                fixed4 col = ap * a + bp * b + cp * c;

                return col * _Tint;
            }
            ENDCG
        }
    }
}
