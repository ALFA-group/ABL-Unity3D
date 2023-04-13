float _TextureScale;
float _TextureOffset;
float _FadeAlphaDistanceFrom;
float _FadeAlphaDistanceTo;
float _PersentOfScreenHeightMode;

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

struct v2fpolyline {
    float4 pos : SV_POSITION;
	float3 uv : TEXCOORD0;
	float2 width : TEXCOORD1;
    fixed4 color : COLOR;
	// stereo & instancing
	UNITY_VERTEX_INPUT_INSTANCE_ID 
	UNITY_VERTEX_OUTPUT_STEREO 
};

struct appdata {
    float3 vertex : POSITION;
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
	fixed4 color : COLOR;
	float3  uv : TEXCOORD0;
	float3  uv1 : TEXCOORD1;
	float3  uv2 : TEXCOORD2;
	fixed4  uv3 : TEXCOORD3;
	// stereo & instancing
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

v2fpolyline polyline10vertPixelBillboard (appdata v ) {
    v2fpolyline o;
 
 	// stereo & instancing
    UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_OUTPUT(v2fpolyline, o);  
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);  
 
	int _id = v.uv1.x;
	float rsign = sign(_id); 
 
 	o.pos = 0;
	o.uv = float3(0,0,0);
	o.width = float2(0,0);
	o.color = fixed4(0,0,0,0);
	
	if(v.uv1.y == 2){
		return o;
	}
  
	int end = v.uv1.y == 1? 1: 0 ;  
  
	float3 uo0 =  v.tangent   ;
	float3 uo1 =  v.vertex  ;
	float3 uo2 =  v.normal ;
 	float3 osvd1 =  ObjSpaceViewDir ( float4 (uo1,0) );
	float l1 = length(osvd1);
	
	if(abs(_ViewOffset)>0.0001){	
		float3 vd = UNITY_MATRIX_IT_MV[2] ;
		float3 osvd0 =  ObjSpaceViewDir ( float4 (uo0,0) );
		float3 osvd2 =  ObjSpaceViewDir ( float4 (uo2,0) );
		float l0 = length(osvd0);
		float l2 = length(osvd2);
		osvd0 = l0 == 0? osvd0 : osvd0/ l0;
		osvd2 = l2 == 0? osvd2 : osvd2/ l2;
		float3 offset0 = lerp(osvd0  , vd , unity_OrthoParams.w)* _ViewOffset;
		osvd1 = l1 == 0? osvd1 : osvd1/ l1;
		float3 offset1 = lerp(osvd1  , vd , unity_OrthoParams.w)* _ViewOffset;
		uo1.xyz += offset1;
		float3 offset2 = lerp(osvd2  , vd , unity_OrthoParams.w)* _ViewOffset;
		uo0.xyz += offset0;
		uo2.xyz += offset2;
 	 }

	float4 mp0 =  UnityObjectToClipPos( uo0 );	
	float4 mp1 =  UnityObjectToClipPos( uo1 );
	float4 mp2 =  UnityObjectToClipPos( uo2 );	

	float2 p0 = clipToPixel( mp0 );
	float2 p1 = clipToPixel( mp1 );
	float2 p2 = clipToPixel( mp2 );
	
	float2 inputw =  lerp(  _WidthMultiplier * v.uv2*0.5,  _ScreenParams.y * _WidthMultiplier* v.uv2 * 0.005,  _PersentOfScreenHeightMode);
	float w = inputw.x  ;
 
 	float  rsignlv = (rsign+1)/2;
	inputw = lerp (inputw, float2(inputw.y, inputw.x), rsignlv );
 
	float uvx = v.uv.x * _TextureScale + _TextureOffset;
	float uvxn = v.uv.y * _TextureScale + _TextureOffset;
	
	float uvwAt = inputw.x * mp1.w;
 	float uvwBt = inputw.y * mp1.w;
 
	o.width = float2(rsignlv, w);
	o.color  = v.color * _Color ;

	float ndist = saturate( (l1-_FadeAlphaDistanceFrom) / (_FadeAlphaDistanceTo - _FadeAlphaDistanceFrom));
	o.color.a  =  o.color.a *  ( 1 - ndist )  ;
 	
 	if(_id ==  -1){
		//r = p1;
		o.pos = mp1;
 		o.uv = float3(uvx, uvwAt/2, uvwAt);
		return o;
	}
	
	if(_id ==  1){
		//r = p1;
		o.pos = mp1;
 		o.uv = float3(uvx, uvwBt/2, uvwBt);
 		return o;
	}
 
 
	float2 _tdir =  (p1 - p0) ;
	float2 tdir =  _tdir ;
	float tdirmagnitude = length(tdir);
 	
	if( tdirmagnitude < 0.001){
		end = 1;
	} else {
		tdir = tdir/tdirmagnitude;
	}
	float2 tdirortho = float2(tdir.y, -tdir.x);

	float2 _ndir =  (p2 - p1) ;
	float2 ndir =  _ndir ;
	float ndirmagnitude = length(ndir);
	if( ndirmagnitude < 0.001){
		end = 1;
	} else {
		ndir = ndir/ndirmagnitude;
	}

	float2 ndirortho = float2(ndir.y, -ndir.x);
	float2 midDir = normalize(tdirortho + ndirortho);
	int left =  sign( dot(tdir, midDir) );
		
	if(dot( tdirortho, ndirortho)<-0.999){
 		end = 1;
	}
 
 	float2 r = p1; 
	
	if(_id ==  -2){
		if(end == 1){
			r =  p1+ndirortho *  w    ;
		} else {
			if(left<0){
				float midDirInternalLength  = 1.0/dot(midDir, tdirortho) * w;
 				float maxMDI =  sqrt( pow(    ndirmagnitude/2, 2)  + pow( w, 2));
				if(maxMDI<midDirInternalLength){
					r = lerp(p1, p2, 0.5) + ndirortho *  w ;
				} else {
 					r = p1 + midDir * midDirInternalLength ;
				}

				float lv = dot(  -_ndir, (p1 - r)) /  pow(ndirmagnitude , 2);
 
				fixed4 ca = v.uv3 * _Color ;
				ca.a =  o.color.a  ;
				o.color = lerp(o.color, ca, lv);
				 
				uvx = lerp( uvx, uvxn,  lv);

			} else {
				r = p1+ndirortho *  w;
			}
 		}
 
		o.pos = pixelToClip(r, mp1.z, mp1.w );
		o.uv = float3(uvx, uvwAt , uvwAt);
		return o;
	}
	if(_id ==  2){
		if(end == 1){
			r =  p1+tdirortho *  w    ;
		} else {
			if(left<0){
				float midDirInternalLength  = 1.0/dot(midDir, tdirortho) * w;
				float maxMDI =  sqrt( pow( tdirmagnitude/2, 2)  + pow( w, 2));
  				if(maxMDI<midDirInternalLength){
					r = lerp(p0, p1, 0.5) + tdirortho *  w ;
				} else {
 					r = p1 + midDir * midDirInternalLength ;
				}
                float lv = dot( _tdir, (p1 - r)) / pow(tdirmagnitude , 2);
				uvx = lerp( uvx, uvxn,   lv);
 				
				fixed4 ca = v.uv3 * _Color ;
				ca.a =  o.color.a  ;
				o.color = lerp(o.color, ca, lv);
 
			} else {
				r = p1+tdirortho *  w;
			}
		}
 
		o.pos = pixelToClip(r, mp1.z, mp1.w);
		o.uv = float3(uvx, uvwBt , uvwBt);
 		return o;
	}
  
	if(_id ==  -3){
		if(end == 1){
			r =  p1-ndir * w    ;
		} else {
			if(left>0){
				r = p1 + midDir * w;
			}  
  		}
 
 		o.pos = pixelToClip(r, mp1.z, mp1.w);
		o.uv = float3(uvx, uvwAt , uvwAt);
 		return o;
	}
	
	
	if(_id ==  3){
		if(end == 1){
			r =  p1+tdir *  w    ;
		} else {
			if(left>0){
				r = p1+midDir * w;
			} 			 
 		}
 
		o.pos = pixelToClip(r, mp1.z, mp1.w);
		o.uv = float3(uvx, uvwBt , uvwBt);
		return o;
	}
 
	if(_id ==  -4){
		if(end == 1){
			r = p1 - ndirortho * w;
		} else {
			if(left<0){
				r = p1 - ndirortho * w;
			} else {
				float midDirInternalLength  = 1.0/dot(midDir, tdirortho) * w;
  				float maxMDI =  sqrt( pow( ndirmagnitude/2, 2)  + pow( w, 2));
				if(maxMDI<midDirInternalLength){
					r = lerp(p1, p2, 0.5) - ndirortho *  w ;
				} else {
 					r = p1 - midDir * midDirInternalLength ;
				}
 
				float lv = dot( -_ndir, (p1 - r)) / pow(ndirmagnitude , 2);
				uvx = lerp( uvx, uvxn,   lv);
				
				fixed4 ca = v.uv3 * _Color ;
				ca.a =  o.color.a;
				o.color = lerp(o.color, ca, lv);
 
			}
		}
 
		o.pos = pixelToClip(r, mp1.z, mp1.w);
		o.uv = float3(uvx, 0 , uvwAt);
		return o;
	}
	if(_id ==  4){
		if(end == 1){
			r = p1-tdirortho *w;
		} else {
			if(left<0){
				r =  p1-tdirortho *w;
			} else {
				float midDirInternalLength  = 1.0/dot(midDir, tdirortho) * w;
   				float maxMDI =  sqrt( pow( tdirmagnitude/2, 2)  + pow( w, 2));
				if(maxMDI<midDirInternalLength){
					r = lerp(p0, p1, 0.5) - tdirortho *  w ;
				} else {
 					r = p1 - midDir * midDirInternalLength ;
				}
 
                float lv = dot(  _tdir, (p1 - r)) / pow(tdirmagnitude , 2);
				uvx = lerp(  uvx,  uvxn, lv)   ;
				fixed4 ca = v.uv3 * _Color ;
				ca.a =  o.color.a;
				o.color  = lerp(o.color, ca, lv);
 
 			}
		}
 
 		o.pos = pixelToClip(r, mp1.z, mp1.w);
		o.uv = float3(uvx, 0 , uvwBt);
		return o;
	}
	  
	if(_id ==  -5){
		if(end == 1){
			r = p1-ndir * w;
		} else {
			if(left<0){
				r =  p1-midDir * w;
			}  
		}
 
 		o.pos = pixelToClip(r, mp1.z, mp1.w);
		o.uv = float3(uvx, 0 , uvwAt);
 		return o;
	}
	if(_id ==  5){
		if(end == 1){
			r = p1+tdir * w;
		} else {
			if(left<0){
				r =  p1-midDir *w;
			}  
		}
 
		o.pos = pixelToClip(r, mp1.z, mp1.w);
		o.uv = float3(uvx, 0 , uvwBt);
		return o;
	}
 
	o.pos = pixelToClip(p1, mp1.z, mp1.w);
    return o;
}