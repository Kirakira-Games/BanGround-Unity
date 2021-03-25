Shader "Custom/DualKawaseBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Radius ("Blur Radius", Float) = 1.5

    }

    CGINCLUDE
        #pragma vertex vert
        #pragma fragment frag

        #include "UnityCG.cginc"

        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        sampler2D _MainTex;
        float4 _MainTex_TexelSize;
        float4 _MainTex_ST;
        float _Radius;
    ENDCG

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {   
            CGPROGRAM
                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float4 uv01: TEXCOORD1;
                    float4 uv23: TEXCOORD2;
                };

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                    _MainTex_TexelSize *= 0.5;

                    float2 uv = o.uv;

                    o.uv01.xy = uv - _MainTex_TexelSize * float2(1 + _Radius, 1 + _Radius);
                    o.uv01.zw = uv + _MainTex_TexelSize * float2(1 + _Radius, 1 + _Radius);
                    o.uv23.xy = uv - float2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * float2(1 + _Radius, 1 + _Radius);
                    o.uv23.zw = uv + float2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * float2(1 + _Radius, 1 + _Radius);

                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    fixed4 sum = tex2D(_MainTex, i.uv) * 4;
                    sum += tex2D(_MainTex, i.uv01.xy);
                    sum += tex2D(_MainTex, i.uv01.zw);
                    sum += tex2D(_MainTex, i.uv23.xy);
                    sum += tex2D(_MainTex, i.uv23.zw);

                    return sum * 0.125;
                }
            ENDCG
        }

        Pass
        {
            CGPROGRAM
                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float4 uv01: TEXCOORD1;
                    float4 uv23: TEXCOORD2;
                    float4 uv45: TEXCOORD3;
                    float4 uv67: TEXCOORD4;
                };

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                    _MainTex_TexelSize *= 0.5;
                    _Radius = float2(1 + _Radius, 1 + _Radius);

                    float2 uv = o.uv;

                    o.uv01.xy = uv + float2(-_MainTex_TexelSize.x * 2, 0) * _Radius;
                    o.uv01.zw = uv + float2(-_MainTex_TexelSize.x, _MainTex_TexelSize.y) * _Radius;
                    o.uv23.xy = uv + float2(0, _MainTex_TexelSize.y * 2) * _Radius;
                    o.uv23.zw = uv + _MainTex_TexelSize * _Radius;
                    o.uv45.xy = uv + float2(_MainTex_TexelSize.x * 2, 0) * _Radius;
                    o.uv45.zw = uv + float2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * _Radius;
                    o.uv67.xy = uv + float2(0, -_MainTex_TexelSize.y * 2) * _Radius;
                    o.uv67.zw = uv - _MainTex_TexelSize * _Radius;

                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 sum = 0;
		            sum += tex2D(_MainTex, i.uv01.xy);
		            sum += tex2D(_MainTex, i.uv01.zw) * 2;
		            sum += tex2D(_MainTex, i.uv23.xy);
		            sum += tex2D(_MainTex, i.uv23.zw) * 2;
		            sum += tex2D(_MainTex, i.uv45.xy);
		            sum += tex2D(_MainTex, i.uv45.zw) * 2;
		            sum += tex2D(_MainTex, i.uv67.xy);
		            sum += tex2D(_MainTex, i.uv67.zw) * 2;
		            
		            return sum * 0.0833;
                }
            ENDCG
        }
    }
}
