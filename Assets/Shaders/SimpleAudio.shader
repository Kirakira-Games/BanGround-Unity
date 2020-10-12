Shader "Unlit/SimpleAudio"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Stencil ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane"}

        ZWrite Off
        Blend One One
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv1 = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = float4(abs(i.uv.x - 0.5) * 2, 0.2, 1.0 - abs(i.uv.y - 0.5) * 2,1);
                float sample1 = tex2D(_MainTex, float2(abs(i.uv.x - 0.5) + 0.01, 0)).r;

                float v = abs(i.uv.y - 0.5);

                col *= smoothstep(v, v * 8, sample1);
                col = pow(col, 0.4545);
                col *= 0.8;
                return col;
            }
            ENDCG
        }
    }
}
