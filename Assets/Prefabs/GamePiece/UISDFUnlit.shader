// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/UISDF"
{
	Properties
	{
		 _MainTex ("Sprite Texture", 2D) = "white" {}
		_Main2Tex ("Sprite Next Texture", 2D) = "white" {}
		 _TexLerp ("Sprite Lerp", Range(0, 1)) = 0
		
		[HDR]_Color ("Tint", Color) = (1,1,1,1)
		[HDR] _OutlineColor ("Outline", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_SDFThreshold("SDF Threshold", Range(0, 1)) = .5
		_Smoothness("SDF Smooth", Range(0, 2)) = .01
		_OutlineThickness("Outline Thickness", Range(0, 1)) = .5
		_NoisePower("Noise Power", Range(0, 1)) = .4
		
		_OffsetPower("Offset Power", Range(0, 1)) = 0
		
		_StencilComp		("Stencil Comparison", Float) = 8
		_Stencil			("Stencil ID", Float) = 0
		_StencilOp			("Stencil Operation", Float) = 0
		_StencilWriteMask	("Stencil Write Mask", Float) = 255
		_StencilReadMask	("Stencil Read Mask", Float) = 255
		
		MySrcMode ("SrcMode", Float) = 0
		MyDstMode ("DstMode", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

			
//		Stencil
//		{
//			Ref [_Stencil]
//			Comp [_StencilComp]
//			Pass [_StencilOp]
//			ReadMask [_StencilReadMask]
//			WriteMask [_StencilWriteMask]
//		}
//		
		Cull Off
		Lighting Off
		ZWrite Off
//		Blend One OneMinusSrcAlpha
		Blend [MySrcMode] [MyDstMode]

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
			};
			
			fixed4 _Color;
			float _SDFThreshold;
			float _Smoothness;
			float _OutlineThickness;
			fixed4 _OutlineColor;
			float _NoisePower;
			float _TexLerp;
			float _OffsetPower;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				//OUT.color = IN.color * _Color;
				OUT.color = IN.color;
				OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex);
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			sampler2D _Main2Tex;
			float _AlphaSplitEnabled;


			float rand(float2 uv) {
				float n = frac(sin(uv.x * 993. + uv.y * 321) * 5432.);
				return n;
			}

			float smoothNoise(float2 uv) {

				float2 gv = frac(uv);
				float2 id = floor(uv);

				gv = gv * gv * (3 - 2 * gv);

				float bl = rand(id);
				float br = rand(id + float2(1, 0));
				float b = lerp(bl, br, gv.x);

				float tl = rand(id + float2(0, 1));
				float tr = rand(id + float2(1, 1));
				float t = lerp(tl, tr, gv.x);

				float f = lerp(b, t, gv.y);

				return f;
			}

			float worldWind(float2 worldUv, float2 vels) {
				float t = _Time.z;

				float2 nOffset = .1 * float2(cos(t) * vels.x, -t * vels.y);

				float n = smoothNoise(nOffset + worldUv.xy * 1);
				n += smoothNoise(nOffset + worldUv.xy * 2) * .5;
				n += smoothNoise(nOffset + worldUv.xy * 4) * .25;
				n += smoothNoise(nOffset + worldUv.xy * 8) * .125;
				n *= .5;

				n = (n * 2) - 1;
				//n *= .002;
				return (.5 + .5 * n);
			}

			fixed4 SampleSpriteTexture (sampler2D sample, float2 uv)
			{
				uv.x = clamp(0, 1, uv.x);
				fixed4 color = tex2D (sample, uv);

#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;
#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{


				float offsetx = .2 * (worldWind(IN.worldPos.xy * .01, float2(1, 1)) - .5);
				float offsety = .2 * (worldWind(IN.worldPos.xy * .05 + float2(325.123, 3928.2), float2(1, .5)) - .5);

				float2 offset = _OffsetPower * float2(offsetx, offsety);
				
				fixed4 c1 = SampleSpriteTexture (_MainTex, IN.texcoord + offset);// * IN.color;
				fixed4 c2 = SampleSpriteTexture (_Main2Tex, IN.texcoord + offset);// * IN.color;

				fixed4 c = lerp(c1, c2, _TexLerp);
				float d = c.a;

				
				c.rgb = _Color.rgb * IN.color.rgb;
				//c.a *= _Color.a * IN.color.a;
				// c.rgb = 1;

				// float n = 1 * _NoisePower * worldWind(IN.worldPos.xy * .01, float2(11, 14));// + .8 * _NoisePower * worldWind(IN.worldPos.xy * .1, float2(2, 2));
				float n = _NoisePower * worldWind(IN.worldPos.xy * .8, float2(2, 2)) + .8 * _NoisePower * worldWind(IN.worldPos.xy * .1, float2(2, 2));
				//c.a += n;
				d += n;
				float m = smoothstep(_SDFThreshold, saturate(_SDFThreshold + _Smoothness), d);
				// c.a = max(c.a, n);
				//c.a *= (n + 1);
				
				float outline = smoothstep(saturate(_SDFThreshold + _Smoothness), saturate(_SDFThreshold + _Smoothness + _OutlineThickness), d);
				//c.rgb *= m;
				float3 x = lerp(c.rgb, _OutlineColor, 1 - sqrt(min(1.0, (outline ))));

				c.rgb = x;
				
				//c.rgb = _OutlineColor * (1-outline) + (outline * c.rgb);
				//c.rgb = c.a - outline;
				// c.a -= n;
				//if (c.a < 0){ c.a = 0;}
				c.a *= IN.color.a * _Color.a * m;
				c.rgb *= c.a;
				//c.rgb *= m;
				//c.a = 
				//c = saturate(c);
				return c;
			}
		ENDCG
		}
	}
}