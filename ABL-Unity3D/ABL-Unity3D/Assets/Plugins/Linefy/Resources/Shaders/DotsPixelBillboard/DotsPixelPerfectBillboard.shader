Shader "Hidden/Linefy/DotsPixelPerfectBillboard" {

	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ViewOffset ("_ViewOffset", Float) = 0
		_DepthOffset ("_DepthOffset", Float) = 0
		_WidthMultiplier("_WidthMultiplier", Float) = 2
		_Color("_Color", Color) = (1, 1, 1, 1) 
		_FadeAlphaDistanceFrom("FadeAlphaDistanceFrom", Float) = 0
		_FadeAlphaDistanceTo("FadeAlphaDistanceTo", Float) = 1000000
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTestCompare("ZTest", Float) = 4  
	}

SubShader {
	
	Tags {
        "Queue"="Geometry"
        "IgnoreProjector"="True"
        "RenderType"="Opaque"
		"ForceNoShadowCasting"="True"
		"DisableBatching" = "True" 
    }

 	 Cull Off
	Lighting Off
	ZWrite On
	ZTest [_zTestCompare]


    Pass {
        Fog { Mode Off }
		Offset  [_DepthOffset] , [_DepthOffset] 
 
        CGPROGRAM
 
		float _ViewOffset;
		sampler2D _MainTex;
		float _WidthMultiplier;
		fixed4 _Color;
        #pragma vertex dot4vertPP
        #pragma fragment frag
        #include "UnityCG.cginc"
		#include "_dotsPixelBillboard.cginc"


        fixed4 frag (v2fdot i) : SV_Target { 
			fixed4 col = tex2D(_MainTex, i.uv) *  i.color;
			clip(col.a - 0.5);
			return col;
		}
        ENDCG
    }
}
}