Shader "Hidden/Sunshine/TerrainEngine/Splatmap/Standard-Base" {
	Properties {
		_MainTex ("Base (RGB) Smoothness (A)", 2D) = "white" {}
		_SpecularMetallicTex ("Specular (RGB) Metallic (A)", 2D) = "white" {}
		_Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.0

		// used in fallback on old cards
		_Color ("Main Color", Color) = (1,1,1,1)
	}

	SubShader {
		Tags {
			"RenderType" = "Opaque"
			"Queue" = "Geometry-100"
		}
		LOD 200

		CGPROGRAM
		
		// This shader uses all texture interpolators, so Sunshine must work completely in the Pixel Shader.
		#define SUNSHINE_PUREPIXEL
		#include "Assets/Sunshine/Shaders/Sunshine.cginc"
		#pragma multi_compile SUNSHINE_DISABLED SUNSHINE_FILTER_PCF_4x4 SUNSHINE_FILTER_PCF_3x3 SUNSHINE_FILTER_PCF_2x2 SUNSHINE_FILTER_HARD

		#pragma surface surf Standard
		#pragma target 3.0
		// needs more than 8 texcoords
		#pragma exclude_renderers gles
		#include "UnityPBSLighting.cginc"

		#pragma multi_compile __ _TERRAIN_OVERRIDE_SMOOTHNESS

		sampler2D _MainTex;
		sampler2D _SpecularMetallicTex;

		#ifdef _TERRAIN_OVERRIDE_SMOOTHNESS
			half _Smoothness;
		#endif

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = 1;
			#ifdef _TERRAIN_OVERRIDE_SMOOTHNESS
				o.Smoothness = _Smoothness;
			#else
				o.Smoothness = c.a;
			#endif
			o.Metallic = tex2D (_SpecularMetallicTex, IN.uv_MainTex).a;
		}

		ENDCG
	}

	FallBack "Sunshine/Legacy Shaders/Diffuse"
}
