 Shader "Hidden/Linefy/DefaultPolygonalMeshTransparent"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Ambient("Ambient", Float) = 1
		_DepthOffset ("_DepthOffset", Float) = 0
		_Color("_Color", Color) = (1, 1, 1, 1) 
		_FadeAlphaDistanceFrom("FadeAlphaDistanceFrom", Float) = 100000
		_FadeAlphaDistanceTo("FadeAlphaDistanceTo", Float) = 100000
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTestCompare("ZTest", Float) = 4  
		[Enum(UnityEngine.Rendering.CullMode)] _Culling ("Culling", Float) = 0
		_TextureTransform("TexTransform", Vector) = (1,1,0,0) 
	}

	SubShader
	{

		Tags {
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"ForceNoShadowCasting"="True"
			"DisableBatching" = "True" 
		}

		Cull [_Culling]
		Lighting Off
		Blend SrcAlpha OneMinusSrcAlpha
 
		ZWrite Off
		ZTest [_zTestCompare]

		Pass
		{

			Offset  [_DepthOffset] , [_DepthOffset] 
			CGPROGRAM
			#pragma vertex vertPolygonalMesh
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "_polygonalMesh.cginc"
			#pragma fragmentoption ARB_precision_hint_fastest
			
 
			
			fixed4 frag (v2f i) : SV_Target
			{
 				fixed4 col = tex2D(_MainTex, i.uv) * i.color;
				 
				return col;
			}
			ENDCG
		}
	}
}
