Shader "Hidden/Linefy/PolylineWorldspaceXY" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_ViewOffset ("_ViewOffset", Float) = 0
		_DepthOffset ("_DepthOffset", Float) = 0
		_TextureScale("Texture Scale", Float) = 1
		_WidthMultiplier("_WidthMultiplier", Float) = 2
		_Color("_Color", Color) = (1, 1, 1, 1) 
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTestCompare("ZTest", Float) = 4  
		_FadeAlphaDistanceFrom("FadeAlphaDistanceFrom", Float) = 100000
		_FadeAlphaDistanceTo("FadeAlphaDistanceTo", Float) = 100000
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
		sampler2D _MainTex;
		float _ViewOffset;
		float _WidthMultiplier;
		fixed4 _Color;

        #pragma vertex polyline10vertWorldspaceXY
        #pragma fragment frag
        #include "UnityCG.cginc"
		#include "_polylineWorldspaceXY.cginc"
 		#pragma multi_compile_instancing

        fixed4 frag (v2fpolyline i) : SV_Target { 
			 float2 _uv = float2( i.uv.x,  i.uv.y/i.uv.z  );
			if(_ProjectionParams.x > 0){
				 _uv.y = 1-_uv.y;
			}
			fixed4 col = tex2D(_MainTex, _uv) *  i.color;
			clip(col.a - 0.5);
			return col;
		}
		
		
        ENDCG
    }
}
}