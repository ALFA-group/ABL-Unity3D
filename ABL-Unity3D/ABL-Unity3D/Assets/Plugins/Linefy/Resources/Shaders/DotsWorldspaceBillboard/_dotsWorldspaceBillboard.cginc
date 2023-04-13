float _TextureScale;
float _TextureOffset;
float _FadeAlphaDistanceFrom;
float _FadeAlphaDistanceTo;
float _AutoTextureOffset;

float2 clipToPixel(float4 a){
	float2 pa = a/ a.w;
	pa.x =   (pa.x+1)*0.5*_ScreenParams.x  ;
	pa.y =  (pa.y+1)*0.5* _ScreenParams.y  ;
	return pa;
 }
 
   
float4 pixelToClip (float2 p, float z, float w){
	float x = (p.x/_ScreenParams.x * 2 -1) * w;
	float y = (p.y/_ScreenParams.y * 2 -1) * w;
	return float4( x,y,z,w );
}
 
struct appdataL {
    float3 vertex : POSITION;
    float3 normal : NORMAL;
	fixed4 color : COLOR;
	float3  uv : TEXCOORD0;
	float3  uv1 : TEXCOORD1;
	// stereo & instancing
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
 
struct v2fdot {
    float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
    fixed4 color : COLOR;
	// stereo & instancing
	UNITY_VERTEX_INPUT_INSTANCE_ID 
	UNITY_VERTEX_OUTPUT_STEREO 
};
 
v2fdot dot4vert (appdataL v) {
    v2fdot o;
	// stereo & instancing
    UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_OUTPUT(v2fdot, o);  
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);  
 
	float3 uo0 = v.vertex + v.uv1;
	float3 normal = v.normal;
	
	if(normal.z <= 0){
		o.pos = float4(0,0,0,0);
		o.color = fixed4(0,0,0,0);
		o.uv = fixed2(0,0); 
		return o;
	}
	
	float3 osvd0 =  ObjSpaceViewDir ( float4 (uo0,0) );
	float l0 = length(osvd0);
		
	if(abs( _ViewOffset)>0.0001){	
		float3 vd = UNITY_MATRIX_IT_MV[2] ;
		osvd0 = l0 == 0? osvd0 : osvd0/ l0;
 		float3 offset0 = lerp(osvd0  , vd , unity_OrthoParams.w)* _ViewOffset;
 		uo0.xyz += offset0;
 	}

 	float w = _WidthMultiplier;
	float4 mp0 =  UnityObjectToClipPos( uo0);
	float2 p0 = clipToPixel( mp0 );
	float2 uv = v.uv; 

	if(_ProjectionParams.x > 0){
	    normal.x = -normal.x; 
 	} 
 	float3 camUp = UNITY_MATRIX_IT_MV[1] ;
	float4 mp0up =  UnityObjectToClipPos( uo0 + camUp * w);
	float2 p0up = clipToPixel( mp0up );
	float wn = p0.y - p0up.y;
	//float scale = wn/w;
	w = wn ;
	p0+= normal.xy * normal.z * w;
 
	
 
	o.uv = uv; 
	o.color  = v.color * _Color;
	float ndist = saturate( (l0-_FadeAlphaDistanceFrom) / (_FadeAlphaDistanceTo - _FadeAlphaDistanceFrom));
	o.color.a  =  o.color.a *  ( 1 - ndist )  ;
	o.pos = pixelToClip(p0, mp0.z, mp0.w);
	return o;
}