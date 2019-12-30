/*
	UGUI Blur UIImage Using Kernel3x3
	Default kernel is Gaussian 3x3 kernel, you can set blurKernel in c# by SetFloatArray

	Paraments:
	_BlurDistance:  sample distance of blur
*/

Shader "UIKit/UIImage/UIImage_Static_Blur" {
	Properties {
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_ApplyChannel("Apply Color Channel", Color) = (1,1,1,1)
		_BlurDistance("Blur Distance", Range(0.001, 0.2)) = 0.01

		[HideInInspector] _StencilComp("Stencil Comparison", Float) = 8
		[HideInInspector] _Stencil("Stencil ID", Float) = 0
		[HideInInspector] _StencilOp("Stencil Operation", Float) = 0
		[HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
		[HideInInspector] _StencilReadMask("Stencil Read Mask", Float) = 255

		[HideInInspector] _ColorMask("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
	}

	SubShader {
		Tags {
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Stencil{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask[_ColorMask]

		Pass {
			Name "UIImage_Blur"

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_ALPHACLIP

			struct appdata_t {
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform sampler2D _MainTex;
			uniform fixed4 _TextureSampleAdd;
			uniform float4 _ClipRect;
			uniform float _BlurDistance;
			uniform float4 _ApplyChannel;

			uniform float blurKernel[9];
			static float GaussianKernel[9] = {
				0.0947416f, 0.118318f, 0.0947416f,
				0.118318f, 0.147761, 0.118318f,
				0.0947416f, 0.118318f, 0.0947416f
			};

			v2f vert(appdata_t IN) {
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = IN.texcoord;

				OUT.color = IN.color;
				return OUT;
			}

			float4 frag(v2f IN) : SV_Target {
				// sample texture an blur
				float4 col1 = tex2D(_MainTex, float2(IN.texcoord.x - _BlurDistance, IN.texcoord.y + _BlurDistance));
				float4 col2 = tex2D(_MainTex, float2(IN.texcoord.x, IN.texcoord.y + _BlurDistance));
				float4 col3 = tex2D(_MainTex, float2(IN.texcoord.x + _BlurDistance, IN.texcoord.y + _BlurDistance)) ;
				float4 col4 = tex2D(_MainTex, float2(IN.texcoord.x - _BlurDistance, IN.texcoord.y));
				float4 col5 = tex2D(_MainTex, IN.texcoord);
				float4 col6 = tex2D(_MainTex, float2(IN.texcoord.x + _BlurDistance, IN.texcoord.y));
				float4 col7 = tex2D(_MainTex, float2(IN.texcoord.x - _BlurDistance, IN.texcoord.y - _BlurDistance));
				float4 col8 = tex2D(_MainTex, float2(IN.texcoord.x, IN.texcoord.y - _BlurDistance));
				float4 col9 = tex2D(_MainTex, float2(IN.texcoord.x + _BlurDistance, IN.texcoord.y - _BlurDistance));

				float4 finColor;
				if (blurKernel[4] != 0) {
					finColor = lerp(col5, (col1* blurKernel[0] + col2 * blurKernel[1] + col3 * blurKernel[2] +
						col4* blurKernel[3] + col5 * blurKernel[4] + col6 * blurKernel[5] +
						col7* blurKernel[6] + col8 * blurKernel[7] + col9* blurKernel[8]), _ApplyChannel) * IN.color;
				}
				else {
					finColor = lerp(col5, (col1* GaussianKernel[0] + col2 * GaussianKernel[1] + col3 * GaussianKernel[2] +
						col4* GaussianKernel[3] + col5 * GaussianKernel[4] + col6 * GaussianKernel[5] +
						col7* GaussianKernel[6] + col8 * GaussianKernel[7] + col9* GaussianKernel[8]), _ApplyChannel) * IN.color;
				}

				return finColor;
			}
			ENDCG
		}
	}
}
