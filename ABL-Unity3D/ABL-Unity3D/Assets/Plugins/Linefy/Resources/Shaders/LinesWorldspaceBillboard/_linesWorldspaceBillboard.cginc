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



v2fline line4vertWorldspaceBillboard (appdataL v) {
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
	
	float3 uovp0 = UnityObjectToViewPos(uo0);
	float3 uovp1 = UnityObjectToViewPos(uo1);
	
   	float3 osvd0 =  ObjSpaceViewDir ( float4 (uo0,0) );
	float l0 = length(osvd0);
		
	if(abs( _ViewOffset)>0.0001){	
		float3 vd = UNITY_MATRIX_IT_MV[2] ;
		float3 osvd1 =  ObjSpaceViewDir ( float4 (uo1,0) );
		float l1 = length(osvd1);
		osvd0 = l0 == 0? osvd0 : osvd0/ l0;
		osvd1 = l1 == 0? osvd1 : osvd1/ l1;
		float3 offset0 = lerp(osvd0  , vd , unity_OrthoParams.w)* _ViewOffset;
		float3 offset1 = lerp(osvd1  , vd , unity_OrthoParams.w)* _ViewOffset;
		uo0.xyz += offset0;
		uo1.xyz += offset1;
	}
	
	if(uovp0.z>0){
		uo0 = lerp (uo0, uo1, -uovp0.z/ (-uovp0.z+uovp1.z)+0.001);
	}
	
	if(uovp1.z>0){
		uo1 = lerp (uo1, uo0, -uovp1.z/ (-uovp1.z+uovp0.z)+0.001);
	}
 
	float wmagnitude =  length(uo0-uo1);
	float4 mp0 =  UnityObjectToClipPos( uo0 );
	float4 mp1 =  UnityObjectToClipPos( uo1 );
	 
	 
 	float2 p0 = clipToPixel( mp0 );
 	float2 p1 = clipToPixel( mp1 );
	float2 dir01 =  (p1 - p0)  ;
 
	float dir01magnitude = length(dir01);
	dir01 = dir01/dir01magnitude;
	float2 dir01ortho = float2(dir01.y, -dir01.x);
	
	//float uvx =  _TextureOffset + v.uv.x *_TextureScale ;
 	o.color  = v.color * _Color ;
 
	fixed uvsign = 2;
	float w = _WidthMultiplier *  v.uv.y * 0.5   ;
  
 
	float3 camUp = UNITY_MATRIX_IT_MV[1] ;
	float4 mp0up =  UnityObjectToClipPos( uo0 + camUp * w);
    float2 p0up = clipToPixel( mp0up );
	w = p0.y-p0up.y;
	if(_ProjectionParams.x > 0){
		uvsign = -2;
	} 
	
	float uvw =  w *  mp0.w;
 	float ndist = saturate( (l0-_FadeAlphaDistanceFrom) / (_FadeAlphaDistanceTo - _FadeAlphaDistanceFrom));
	o.color.a  =  o.color.a *  ( 1 - ndist )  ;
	 
	
	if(id == 0){
		float uvx = _TextureOffset + v.uv.x *_TextureScale ;
 		p0 -= dir01ortho*w;
		o.uv = float3(uvx,0,uvw);
		o.width = float2(0,w*uvsign );
		o.pos = pixelToClip(p0, mp0.z, mp0.w );
		return o;
 	}  
	
	if(id== 1){
		float uvx =  _TextureOffset + v.uv.x *_TextureScale ;
 		p0 += dir01ortho*w;
		o.uv = float3(uvx,uvw,uvw);
		o.width = float2(0,w*uvsign );
		o.pos = pixelToClip(p0, mp0.z, mp0.w );
		return o;
 	} 
	
	if(id == 2){
		float uvx =  _TextureOffset + (v.uv.x +  length(uo0-uo1) * _AutoTextureOffset) *_TextureScale ;
		//uvx = _TextureOffset + wmagnitude * _TextureScale;	
		p0 -= dir01ortho*w;
		o.uv = float3(uvx,uvw,uvw);
		o.width = float2(dir01magnitude,w*uvsign );
		o.pos = pixelToClip(p0, mp0.z, mp0.w );
		return o;
	}
	float uvx =  _TextureOffset + (v.uv.x +  length(uo0-uo1) * _AutoTextureOffset) *_TextureScale ;
	//uvx = _TextureOffset +wmagnitude * _TextureScale;
 	p0 += dir01ortho*w;
	o.uv = float3(uvx,0,uvw);
	o.width = float2(dir01magnitude,w*uvsign );
	o.pos = pixelToClip(p0, mp0.z, mp0.w );
	return o;
} 