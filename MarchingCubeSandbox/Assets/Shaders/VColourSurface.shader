﻿Shader "Custom/VColourSurface"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }

			CGPROGRAM
			#pragma surface surf Standard vertex:vert fullforwardshadows 
			#pragma target 4.6

			struct Input
			{
				float2 uv_MainTex : SV_POSITION;
				float3 uv_Color : COLOR;
			};

			void vert(inout appdata_full v, out Input o)
			{
				UNITY_INITIALIZE_OUTPUT(Input,o);
			}

			sampler2D _MainTex;

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
				o.Albedo *= IN.uv_Color;
			}
			ENDCG
		}
			FallBack "Diffuse"
}
