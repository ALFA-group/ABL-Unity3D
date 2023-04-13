Shader "Hidden/Linefy/LinesTransparentPixelBillboard" {

	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TextureScale("Texture Scale", Float) = 1
		_ViewOffset ("_ViewOffset", Float) = 0
		_DepthOffset ("_DepthOffset", Float) = 0
		_WidthMultiplier("_WidthMultiplier", Float) = 2
		_Color("_Color", Color) = (1, 1, 1, 1) 
		_Feather("_Feather", Float) = 1
		_FadeAlphaDistanceFrom("FadeAlphaDistanceFrom", Float) = 100000
		_FadeAlphaDistanceTo("FadeAlphaDistanceTo", Float) = 100000
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTestCompare("ZTest", Float) = 4  
		_AutoTextureOffset("AutoTextureOffset", Float) = 0
		_PersentOfScreenHeightMode("PersentOfScreenHeightMode", Float) = 0
	}

	SubShader {
	
		Tags {
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"ForceNoShadowCasting"="True"
			"DisableBatching" = "True" 
		}

 		Cull Off
		Lighting Off
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
 		ZTest [_zTestCompare]

		Pass {
			Fog { Mode Off }
			Offset  [_DepthOffset] , [_DepthOffset] 

			CGPROGRAM
			float _ViewOffset;
			float _Feather;
			sampler2D _MainTex;
			float _WidthMultiplier;
			fixed4 _Color;

			#pragma vertex line4vertPixelBillboard
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "_linesPixelBillboard.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest

			fixed4 frag (v2fline i) : SV_Target { 
				 float2 _uv = float2( i.uv.x,  i.uv.y/i.uv.z  );
 				 fixed4 col = tex2D(_MainTex, _uv) *  i.color;
				 if(_Feather > 0.001){
				 	float w =  i.width.y/_Feather;
					float f = min(_uv.y,  1  - _uv.y)*w;
					f = min(f,1);
 				 	col.a = col.a * f;
				 }
 				 return col; 
			}
			ENDCG
		}
	}
}