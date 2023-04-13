sampler2D _MainTex;
float4 _TextureTransform;
float _Ambient;
fixed4 _Color;
float _ViewOffset;
float _FadeAlphaDistanceFrom;
float _FadeAlphaDistanceTo;

struct appdata
{
	float4 vertex : POSITION;
	float3 norm : NORMAL;
	float2 uv : TEXCOORD0;
	fixed4 color : COLOR;
};

struct v2f
{
	float4 vertex : SV_POSITION;
	fixed4 color : COLOR;
	float2 uv : TEXCOORD0;
};



v2f vertPolygonalMesh (appdata v)
{
	v2f o;
	float3 uo0 = v.vertex;
	float3 vd = UNITY_MATRIX_IT_MV[2] ;
	float3 osvd =  ObjSpaceViewDir ( float4 (uo0,0) );
	float l0 = length(osvd);
	osvd = l0 == 0? osvd : osvd/ l0;
	float3 offset = lerp(osvd  , vd , unity_OrthoParams.w)* _ViewOffset;
	o.vertex = UnityObjectToClipPos(uo0+offset); 
    o.uv.x = v.uv.x*_TextureTransform.x + _TextureTransform.z;
    o.uv.y = v.uv.y*_TextureTransform.y + _TextureTransform.w;
	
	//TRANSFORM_TEX(v.uv, _MainTex);
	float3 worldNormal = UnityObjectToWorldNormal(v.norm );
	float attenuation = saturate(  saturate( dot( _WorldSpaceLightPos0.xyz, worldNormal ) )  + _Ambient);
	float4 acolor = float4(attenuation,attenuation,attenuation,1);
	o.color = v.color  * acolor * _Color ;
	float ndist = saturate( (l0-_FadeAlphaDistanceFrom) / (_FadeAlphaDistanceTo - _FadeAlphaDistanceFrom));
	o.color.a  =  o.color.a *  ( 1 - ndist )  ;
	return o;
}