Shader "Unlit/CustomParticle"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Param ("(ColumnSize), (X, [MaxColumn] - Y))", Vector) = (1.0, 1.0, 0.0, 0.0)
        _Scale ("Scale", Vector) = (1.0, 1.0, 0.0, 0.0)
    }
    SubShader
    {
        Tags { "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" "Queue" = "Overlay" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM
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

            CBUFFER_START(UnityPerMaterial)
                sampler2D _MainTex;
                float4 _MainTex_ST;
                float4 _Param;
                float3 _Scale;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;

                // Billboard
                o.vertex = UnityViewToClipPos(UnityObjectToViewPos(float4(0, 0, 0, 0)) + v.vertex.xyz * _Scale);
                // Texture shift
                o.uv = _Param.xy * (TRANSFORM_TEX(v.uv, _MainTex) + _Param.zw);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDHLSL
        }
    }
}
