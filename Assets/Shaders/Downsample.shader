Shader "Other/Downsample"
{
    Properties
    {
        _MainTex("Backbuffer", 2D) = "white" {}
    }

    CGINCLUDE
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        float4 _MainTex_TexelSize;

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

        v2f vert(appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = ComputeScreenPos(o.vertex);
            return o;
        }

        fixed4 blur(sampler2D image, float2 uv, float2 direction) 
        {
            fixed4 color = fixed4(0,0,0,0);
            float2 off1 = 1.3846153846 * direction;
            float2 off2 = 3.2307692308 * direction;
            color += tex2D(image, uv) * 0.2270270270;
            color += tex2D(image, uv + (off1 * _MainTex_TexelSize)) * 0.3162162162;
            color += tex2D(image, uv - (off1 * _MainTex_TexelSize)) * 0.3162162162;
            color += tex2D(image, uv + (off2 * _MainTex_TexelSize)) * 0.0702702703;
            color += tex2D(image, uv - (off2 * _MainTex_TexelSize)) * 0.0702702703;
            return color;
        }
    ENDCG

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

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = blur(_MainTex, i.uv, float2(1,0));
                return col;
            }
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = blur(_MainTex, i.uv, float2(0,1));
                return col;
            }
            ENDCG
        }
    }
}
