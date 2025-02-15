// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

  Shader "Magic Leap/Unlit/MeshVertexTriPlanar" {
    Properties {
      
      //3d projection
      _MainTex ("Texture", 2D) = "Black" {}
      
      //3d texture placment
      _Scale ("Scale", Range(0,10))= 1
      _Offset ("Offset", Range(-1,1))= 0
      
      //Sprite sheet controls
      _TexWidth ("Sheet Width", float) = 0.0
      _CellAmount ("Cell Amount", float) = 0.0
      _Speed ("Speed", Range(0.01, 32)) = 12
      
      //Falloff Ramp
      _Ramp ("Texture", 2D) = "Black" {}
      _RampScale ("RampScale", Range(.001,1))= 1
      _Offsetx ("OffsetX", Range(-10,10))= 0
      _Offsetz ("OffsetZ", Range(-10,10))= 0
      _RampColor ("RampColor", Color) = (1,1,1)
      
      //Dot scale offset and brightness
      _Multi ("Multiplier", Range(0.0,2)) = 0.0
      _Brightness ("Brightness", Range(0.0,5)) = 0.0
      
      //Dot colors (alpha currently white)
      _Color1 ("RedColor", Color) = (1,0,0)
      _Color2 ("GreenColor", Color) = (0,1,0)
      _Color3 ("BlueColor", Color) = (0,0,1)
      _Color4 ("AlphaColor", Color) = (0,0,1)


      
    }
    SubShader {
      Tags { "Queue"="geometry" "RenderType" = "Opaque" }
      LOD 200
      Lighting Off
      Cull Back
      Fog { Mode Off}
      CGPROGRAM
     #pragma surface surf Additive vertex:vert halfasview novertexlights exclude_path:prepass noambient noforwardadd nolightmap nodirlightmap

      half4 LightingAdditive (SurfaceOutput s, half3 lightDir, half3 viewDir) {
		half3 h = normalize (lightDir + viewDir);


          half4 c;
          c.rgb = s.Albedo;
          c.a = s.Alpha;
          return c;
      }
      struct Input {
          float2 uv_MainTex;
          float2 uv_Ramp;
          float3 worldNormal;
          float3 worldPos;
          
          float4 vertColor;
      
      };
      
      sampler2D _MainTex;
      sampler2D _Ramp;
      float _TexWidth;
      float _CellAmount;
      float _Speed;
      float _Multi;
      float _Brightness;
      float _Scale;
      float _Offset;
      float _RampScale;
      float _Offsetx;
      float _Offsetz;
      fixed3 _Color1;
      fixed3 _Color2;
      fixed3 _Color3;
      fixed3 _Color4;
      fixed3 _RampColor;

	  void vert(inout appdata_full v, out Input o)
	  {
	      UNITY_INITIALIZE_OUTPUT(Input,o);
	  	  o.vertColor = v.color;
	  } 


      
      void surf (Input IN, inout SurfaceOutput o) {

          float2 spriteUV = float2(0,0);
          float cellPixelWidth = _TexWidth/ _CellAmount;
          float cellUVPercentage = cellPixelWidth/_TexWidth;
          
          float timeVal = fmod(_Time.y * _Speed, _CellAmount);
          timeVal = ceil(timeVal);
          
          float xValue = spriteUV.x;
          xValue += cellUVPercentage * timeVal * _CellAmount;
          xValue *= cellUVPercentage;
          
          spriteUV = float2 (xValue, spriteUV.y);
          
          //Triplanar Projection
          float3 projNormal = saturate(pow(IN.worldNormal * 1.4, 4));
          
          float4 x = tex2D(_MainTex, frac(IN.worldPos.zy * _Scale+spriteUV)) * abs(IN.worldNormal.x);
          
          float4 y = tex2D(_MainTex, frac(IN.worldPos.zx * _Scale+spriteUV)) * abs(IN.worldNormal.y);
          
          float4 z = tex2D(_MainTex, frac(IN.worldPos.xy * _Scale+spriteUV)) * abs(IN.worldNormal.z);
          
          float4 projcomp =lerp(z,x,projNormal.x);
          float4 projcomp2 = lerp(0,y,projNormal.y);
		  half4 maintex = projcomp2;
	      half3 rampy = tex2D (_Ramp, frac(float2(IN.worldPos.z+_Offsetz,IN.worldPos.x+_Offsetx) * _RampScale))*_RampColor ;
	      half ramp = clamp(IN.vertColor.r*_Multi+.15,0,1);
	      half green = clamp((maintex.g-ramp)/(ramp),0,1);
	      half red = clamp((maintex.r-ramp)/(ramp),0,1);
	      half blue = clamp((maintex.b-ramp)/(ramp),0,1);
      	  half mask =clamp((maintex.a-ramp)/(ramp),0,1);;
      	  half luma = clamp((green+blue+red+mask)*_Brightness,0,1);
      	  half3 colred = red*_Color1;
      	  half3 colgreen = green*_Color2;
      	  half3 colblue = blue*_Color3;
      	  half3 colmask = mask*_Color4;
      	  
      	  half3 colorcomp = colred+ colgreen+ colblue+colmask;

          o.Emission = luma*colorcomp+rampy;
          
          
      }
      ENDCG
    } 
    Fallback "Diffuse"
  }