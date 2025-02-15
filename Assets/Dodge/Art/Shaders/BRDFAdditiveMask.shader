// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

 Shader "Magic Leap/BRDF/AdditiveMask"
 {
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Color Map/Alpha Spec", 2D) = "white" {}
		_RampLight ("RampLight", 2D) = "Black" {}
		_Multi ("Fade", Range(0.0,2.0)) = 1.0
		_LightVector ("LightVector", Vector) = (0,0,0,0)
    }

	SubShader
	{
		Tags { "Queue"="geometry-10" "RenderType" = "Transparent" }
		LOD 200
		ZWrite Off
		Lighting Off
		Cull back
		ZTest off
		Blend One One 
		Fog { Mode Off}
		CGPROGRAM
		#pragma surface surf RampLight novertexlights exclude_path:prepass noambient noforwardadd nolightmap nodirlightmap
		#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_Ramp;
		};

		sampler2D _MainTex;
		sampler2D _RampLight;
		half _Luma;
		half _Multi;
		float4 _Color;
		float4 _LightVector;

		half4 LightingRampLight (SurfaceOutput s, half3 lightDir, half3 viewDir, fixed atten)
		{
			float light = dot(s.Normal,_LightVector);
			float rim = dot(s.Normal,viewDir);
			float diff = (light*.5)+.5;
				
			float2 brdfdiff = float2(rim, diff);
			float3 BRDFLight = tex2D(_RampLight, brdfdiff.xy).rgb;
		
			half3 BRDFColor = (s.Albedo);
			
      
			half4 c;
			c.rgb =(lerp(0,BRDFColor,BRDFLight.g*_Multi)+BRDFLight.r)*_Color;
			c.a = _Color.a;
			return c;
		}

		void surf (Input IN, inout SurfaceOutput o)
		{
			half3 maintex = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = maintex;
		}
		ENDCG
	}

    Fallback "Diffuse"
}