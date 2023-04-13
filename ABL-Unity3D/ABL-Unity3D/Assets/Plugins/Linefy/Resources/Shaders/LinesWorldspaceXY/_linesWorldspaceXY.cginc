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
 
 struct v2fline {
    float4 pos : SV_POSITION;
	float3 uv : TEXCOORD0;
	float2 width : TEXCOORD1;
    fixed4 color : COLOR;
		// stereo & instancing
	UNITY_VERTEX_INPUT_INSTANCE_ID 
	UNITY_VERTEX_OUTPUT_STEREO 
};
 
struct appdataL {
    float3 vertex : POSITION;
    float3 normal : NORMAL;
	fixed4 color : COLOR;
	float3  uv : TEXCOORD0;
	float3  uv1 : TEXCOORD1;
		// stereo & instancing
	UNITY_VERTEX_INPUT_INSTANCE_ID
};


 
v2fline line4vertWorldspaceXY (appdataL v) {
    v2fline o;
	
	// stereo & instancing
    UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_OUTPUT(v2fline, o);  
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); 
	
	int tid = v.uv1.y;
	int id = v.uv1.x;
	if(tid == 1){
		o.pos = float4(0,0,0,0);
		o.uv = float3(0,0,0);
		o.width = float2(0,0);
		o.color = fixed4(0,0,0,0);
		return o;
	}
	
	float3 uo0 = v.vertex;
 	float3 uo1 = v.normal;
	
   	float3 osvd0 =  ObjSpaceViewDir ( float4 (uo0,0) );
	float l0 = length(osvd0);
	
	/// todo _ViewOffset
 
  	float2 p0 = uo0.xy;
 	float2 p1 = uo1.xy;
	float2 dir01 =  (p1 - p0)  ;
 
	float dir01magnitude = length(dir01);
	dir01 = dir01/dir01magnitude;
	float2 dir01ortho = float2(dir01.y, -dir01.x);
	

 	o.color  = v.color * _Color ;
 
	fixed uvsign = 2;
	float w = _WidthMultiplier *  v.uv.y * 0.5   ;

	float uvw =  w;  
	//mp0.w;
 	float ndist = saturate( (l0-_FadeAlphaDistanceFrom) / (_FadeAlphaDistanceTo - _FadeAlphaDistanceFrom));
	o.color.a  =  o.color.a *  ( 1 - ndist )  ;
	
	if(id == 0){
		float uvx =  _TextureOffset + v.uv.x *_TextureScale ;
 		p0 -= dir01ortho*w;
		o.uv = float3(uvx,0,uvw);
		o.width = float2(0,w*uvsign );
		o.pos = UnityObjectToClipPos( float3(p0, uo0.z) );
		//pixelToClip(p0, mp0.z, mp0.w );
		return o;
 	}  
	
	if(id== 1){
		float uvx =  _TextureOffset + v.uv.x *_TextureScale ;
 		p0 += dir01ortho*w;
		o.uv = float3(uvx,uvw,uvw);
		o.width = float2(0,w*uvsign );
 		o.pos = UnityObjectToClipPos( float3(p0, uo0.z) );
		return o;
 	} 
	
	if(id == 2){
		float uvx =  _TextureOffset + (v.uv.x +  length(uo0-uo1) * _AutoTextureOffset) *_TextureScale ;
		//uvx = _TextureOffset + wmagnitude * _TextureScale;	
		p0 -= dir01ortho*w;
		o.uv = float3(uvx,uvw,uvw);
		o.width = float2(dir01magnitude,w*uvsign );
		//o.pos = pixelToClip(p0, mp0.z, mp0.w );
		o.pos = UnityObjectToClipPos( float3(p0, uo0.z) );
		return o;
	}
	
	//uvx = _TextureOffset +wmagnitude * _TextureScale;
	float uvx =  _TextureOffset + (v.uv.x +  length(uo0-uo1) * _AutoTextureOffset) *_TextureScale ;
 	p0 += dir01ortho*w;
	o.uv = float3(uvx,0,uvw);
	o.width = float2(dir01magnitude,w*uvsign );
	o.pos = UnityObjectToClipPos( float3(p0, uo0.z) );
	//o.pos = pixelToClip(p0, mp0.z, mp0.w );
	return o;
}   