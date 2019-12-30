/*
	Blur UIImage using two pass Gaussian Blur (7 x 7)
	You'd better not to set blurKernel in c#

	Paraments:
	_BlurDistance:  sample distance of blur
*/

Shader "UIKit/UIImage/UIImage_Static_Blur_Advanced" {
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

		Stencil {
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
			Name "UIImage_Static_Blur_Advanced_X"

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
			uniform float blurKernel[7];
			static float GaussianKernel[7] = {
				0.03125f, 0.109375f, 0.21875f, 0.28125f, 0.21875f, 0.109375f, 0.03125f
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
				float4 col1 = tex2D(_MainTex, float2(IN.texcoord.x - 3.0f * _BlurDistance, IN.texcoord.y));
				float4 col2 = tex2D(_MainTex, float2(IN.texcoord.x - 2.0f * _BlurDistance, IN.texcoord.y));
				float4 col3 = tex2D(_MainTex, float2(IN.texcoord.x - 1.0f * _BlurDistance, IN.texcoord.y));
				float4 col4 = tex2D(_MainTex, IN.texcoord);
				float4 col5 = tex2D(_MainTex, float2(IN.texcoord.x + 1.0f * _BlurDistance, IN.texcoord.y));
				float4 col6 = tex2D(_MainTex, float2(IN.texcoord.x + 2.0f * _BlurDistance, IN.texcoord.y));
				float4 col7 = tex2D(_MainTex, float2(IN.texcoord.x + 3.0f * _BlurDistance, IN.texcoord.y));

				float4 finColor;
				if (blurKernel[4] != 0) {
					finColor = (col1* blurKernel[0] + col2 * blurKernel[1] + col3 * blurKernel[2] +
						col4* blurKernel[3] + col5 * blurKernel[4] + col6 * blurKernel[5] +
						col7* blurKernel[6]) * IN.color;
				}
				else {
					finColor = (col1* GaussianKernel[0] + col2 * GaussianKernel[1] + col3 * GaussianKernel[2] +
						col4* GaussianKernel[3] + col5 * GaussianKernel[4] + col6 * GaussianKernel[5] +
						col7* GaussianKernel[6]) * IN.color;
				}

				return finColor;
			}
			ENDCG
		}

		GrabPass
		{
			"_BlurTexture"
		}

		Pass{
			Name "UIImage_Static_Blur_Advanced_Y"

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
				float4 grabPosition : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform sampler2D _MainTex;
			uniform fixed4 _TextureSampleAdd;
			uniform sampler2D _BlurTexture;
			uniform float4 _ClipRect;
			uniform float _BlurDistance;
			uniform float4 _ApplyChannel;

			uniform float blurKernel[7];
			static float GaussianKernel[7] = {
				0.03125f, 0.109375f, 0.21875f, 0.28125f, 0.21875f, 0.109375f, 0.03125f
			};

			v2f vert(appdata_t IN) {
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = IN.texcoord;
				OUT.grabPosition = ComputeGrabScreenPos(OUT.vertex);

				OUT.color = IN.color;
				return OUT;
			}

			float4 frag(v2f IN) : SV_Target{
				// sample texture an blur
				float4 col1 = tex2Dproj(_BlurTexture, float4(IN.grabPosition.x, IN.grabPosition.y - 3 * _BlurDistance, IN.grabPosition.z, IN.grabPosition.w));
				float4 col2 = tex2Dproj(_BlurTexture, float4(IN.grabPosition.x, IN.grabPosition.y - 2 * _BlurDistance, IN.grabPosition.z, IN.grabPosition.w));
				float4 col3 = tex2Dproj(_BlurTexture, float4(IN.grabPosition.x, IN.grabPosition.y - 1 * _BlurDistance, IN.grabPosition.z, IN.grabPosition.w));
				float4 col4 = tex2Dproj(_BlurTexture, IN.grabPosition);
				float4 col5 = tex2Dproj(_BlurTexture, float4(IN.grabPosition.x, IN.grabPosition.y + 1 * _BlurDistance, IN.grabPosition.z, IN.grabPosition.w));
				float4 col6 = tex2Dproj(_BlurTexture, float4(IN.grabPosition.x, IN.grabPosition.y + 2 * _BlurDistance, IN.grabPosition.z, IN.grabPosition.w));
				float4 col7 = tex2Dproj(_BlurTexture, float4(IN.grabPosition.x, IN.grabPosition.y + 3 * _BlurDistance, IN.grabPosition.z, IN.grabPosition.w));
				float4 colMain = tex2D(_MainTex, IN.texcoord);

				float4 finColor;
				if (blurKernel[4] != 0) {
					finColor = lerp(colMain, (col1* blurKernel[0] + col2 * blurKernel[1] + col3 * blurKernel[2] +
						col4* blurKernel[3] + col5 * blurKernel[4] + col6 * blurKernel[5] +
						col7* blurKernel[6]), _ApplyChannel) * IN.color;
				}
				else {
					finColor = lerp(colMain, (col1* GaussianKernel[0] + col2 * GaussianKernel[1] + col3 * GaussianKernel[2] +
						col4* GaussianKernel[3] + col5 * GaussianKernel[4] + col6 * GaussianKernel[5] +
						col7* GaussianKernel[6]), _ApplyChannel) * IN.color;
				}

				return finColor;
			}
			ENDCG
		}
	}
}
