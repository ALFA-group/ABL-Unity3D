Shader "Hidden/Linefy/LinesWorldspaceBillboard" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_TextureScale("Texture Scale", Float) = 1
		_ViewOffset ("_ViewOffset", Float) = 0
		_DepthOffset ("_DepthOffset", Float) = 0
		_WidthMultiplier("_WidthMultiplier", Float) = 2
		_Color("_Color", Color) = (1, 1, 1, 1) 
		_FadeAlphaDistanceFrom("FadeAlphaDistanceFrom", Float) = 100000
		_FadeAlphaDistanceTo("FadeAlphaDistanceTo", Float) = 100000
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTestCompare("ZTest", Float) = 4  
		_AutoTextureOffset("AutoTextureOffset", Float) = 0
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

        #pragma vertex line4vertWorldspaceBillboard
        #pragma fragment frag
        #include "UnityCG.cginc"
		#include "_linesWorldspaceBillboard.cginc"
		#pragma fragmentoption ARB_precision_hint_fastest
         
        fixed4 frag (v2fline i) : SV_Target {
			float2 _uv = float2( i.uv.x,  i.uv.y/i.uv.z  );
			fixed4 col = tex2D(_MainTex,_uv) *  i.color;
			clip(col.a - 0.5);
			return col;
		}
        ENDCG
    }
}
}