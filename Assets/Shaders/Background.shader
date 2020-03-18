Shader "Skybox/Background"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tint ("Tint", Color) = (1.0, 1.0, 1.0, 1.0)
        _TexRatio ("Texture Aspect Ratio", Float) = 1.0
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

            sampler2D _MainTex;
            float4 _Tint;
            float4 _MainTex_ST;
            float _TexRatio;

            v2f vert(appdata v) 
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.screenPosition = ComputeScreenPos(o.position);
                return o;
            }

            fixed4 frag(v2f i) : SV_TARGET
            {
                float2 textureCoordinate = i.screenPosition.xy / i.screenPosition.w;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                if(_TexRatio < aspect)
                {
                    textureCoordinate.y = textureCoordinate.y / aspect;
                    textureCoordinate = TRANSFORM_TEX(textureCoordinate, _MainTex);

                    textureCoordinate.y = textureCoordinate.y * _TexRatio;

                    float t = 1 / _TexRatio - 1 / aspect;
                    textureCoordinate.y += t / 2;
                }
                else
                {
                    textureCoordinate.x = textureCoordinate.x * aspect;
                    textureCoordinate = TRANSFORM_TEX(textureCoordinate, _MainTex);

                    textureCoordinate.x = textureCoordinate.x / _TexRatio;

                    float t = _TexRatio - aspect;
                    textureCoordinate.x += t / 2;
                }
                

                return tex2D(_MainTex, textureCoordinate) * _Tint;
            }
            ENDCG
        }
    }
}
