Shader "Hidden/Linefy/DotsTransparentWorldspaceXY" {

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
		sampler2D _MainTex;
		float _WidthMultiplier;
		fixed4 _Color;

 		#pragma shader_feature  WORLDSPACE_WIDTH
        #pragma vertex dot4vert
        #pragma fragment frag
        #include "UnityCG.cginc"
		#include "_dotsWorldspaceXY.cginc"
   
        fixed4 frag (v2fdot i) : SV_Target { 
			fixed4 col = tex2D(_MainTex, i.uv) *  i.color;
			return col ;
		}
        ENDCG
    }


}
}