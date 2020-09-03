// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Arno/Cloud_dome2"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		[Header(Translucency)]
		_Translucency("Strength", Range( 0 , 50)) = 1
		_TransNormalDistortion("Normal Distortion", Range( 0 , 1)) = 0.1
		_TransScattering("Scaterring Falloff", Range( 1 , 50)) = 2
		_TransDirect("Direct", Range( 0 , 1)) = 1
		_TransAmbient("Ambient", Range( 0 , 1)) = 0.2
		_TransShadow("Shadow", Range( 0 , 1)) = 0.9
		_Texture0("Texture 0", 2D) = "white" {}
		_Cloud_color("Cloud_color", Color) = (0,0,0,0)
		_Cloud_tiling("Cloud_tiling", Range( 0.0001 , 1)) = 0
		_Cloud_cleareness("Cloud_cleareness", Range( 0 , 1)) = 0.5162644
		_Cloud_density("Cloud_density", Range( 0 , 10)) = -10
		_Speed("Speed", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 5.0
		struct Input
		{
			float2 uv_texcoord;
		};

		struct SurfaceOutputStandardCustom
		{
			fixed3 Albedo;
			fixed3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			fixed Alpha;
			fixed3 Translucency;
		};

		uniform float4 _Cloud_color;
		uniform sampler2D _Texture0;
		uniform float _Speed;
		uniform float _Cloud_tiling;
		uniform float _Cloud_cleareness;
		uniform half _Translucency;
		uniform half _TransNormalDistortion;
		uniform half _TransScattering;
		uniform half _TransDirect;
		uniform half _TransAmbient;
		uniform half _TransShadow;
		uniform float _Cloud_density;

		inline half4 LightingStandardCustom(SurfaceOutputStandardCustom s, half3 viewDir, UnityGI gi )
		{
			#if !DIRECTIONAL
			float3 lightAtten = gi.light.color;
			#else
			float3 lightAtten = lerp( _LightColor0, gi.light.color, _TransShadow );
			#endif
			half3 lightDir = gi.light.dir + s.Normal * _TransNormalDistortion;
			half transVdotL = pow( saturate( dot( viewDir, -lightDir ) ), _TransScattering );
			half3 translucency = lightAtten * (transVdotL * _TransDirect + gi.indirect.diffuse * _TransAmbient) * s.Translucency;
			half4 c = half4( s.Albedo * translucency * _Translucency, 0 );

			SurfaceOutputStandard r;
			r.Albedo = s.Albedo;
			r.Normal = s.Normal;
			r.Emission = s.Emission;
			r.Metallic = s.Metallic;
			r.Smoothness = s.Smoothness;
			r.Occlusion = s.Occlusion;
			r.Alpha = s.Alpha;
			return LightingStandard (r, viewDir, gi) + c;
		}

		inline void LightingStandardCustom_GI(SurfaceOutputStandardCustom s, UnityGIInput data, inout UnityGI gi )
		{
			UNITY_GI(gi, s, data);
		}

		void surf( Input i , inout SurfaceOutputStandardCustom o )
		{
			float mulTime71 = _Time.y * 0.01;
			float temp_output_132_0 = ( mulTime71 * _Speed );
			float2 temp_output_17_0 = ( i.uv_texcoord * _Cloud_tiling );
			float2 temp_output_15_0 = ( (abs( temp_output_17_0+temp_output_132_0 * float2(0,1 ))) * float2( 1,1 ) );
			float2 temp_output_16_0 = ( (abs( temp_output_17_0+temp_output_132_0 * float2(1,0 ))) * float2( 2,2 ) );
			float temp_output_30_0 = ( tex2D( _Texture0, temp_output_15_0 ).r + tex2D( _Texture0, temp_output_16_0 ).r );
			float Cloud_occlusion121 = ( temp_output_30_0 + ( ( _Cloud_cleareness * 0.5 ) + 0.0 ) );
			float4 temp_output_114_0 = ( _Cloud_color * Cloud_occlusion121 );
			o.Albedo = temp_output_114_0.rgb;
			float temp_output_56_0 = 0.0;
			o.Metallic = temp_output_56_0;
			o.Smoothness = temp_output_56_0;
			float temp_output_122_0 = Cloud_occlusion121;
			o.Occlusion = temp_output_122_0;
			float temp_output_103_0 = ( _Cloud_cleareness + -1.0 );
			float temp_output_104_0 = ( _Cloud_density * -1.0 );
			float temp_output_95_0 = (( temp_output_103_0 * temp_output_104_0 ) + (temp_output_30_0 - 0.0) * (( ( temp_output_103_0 + 1.0 ) * temp_output_104_0 ) - ( temp_output_103_0 * temp_output_104_0 )) / (2.0 - 0.0));
			float clampResult108 = clamp( ( 1.0 - temp_output_95_0 ) , 0.0 , 1.0 );
			float Diffusion60 = clampResult108;
			float temp_output_61_0 = Diffusion60;
			float3 temp_cast_1 = (temp_output_61_0).xxx;
			o.Translucency = temp_cast_1;
			float clampResult42 = clamp( temp_output_95_0 , 0.0 , 1.0 );
			float Cloud_mask8 = clampResult42;
			o.Alpha = Cloud_mask8;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustom alpha:fade keepalpha fullforwardshadows exclude_path:deferred 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			# include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float3 worldPos : TEXCOORD6;
				float4 texcoords01 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.texcoords01 = float4( v.texcoord.xy, v.texcoord1.xy );
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			fixed4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord.xy = IN.texcoords01.xy;
				float3 worldPos = IN.worldPos;
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				SurfaceOutputStandardCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandardCustom, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=11010
2417;60;1176;974;2342.631;1409.288;1;True;True
Node;AmplifyShaderEditor.SimpleTimeNode;71;-1943.604,-789.8237;Float;False;1;0;FLOAT;0.01;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;18;-2091.208,-980.5297;Float;False;Property;_Cloud_tiling;Cloud_tiling;9;0;0;0.0001;1;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;131;-1935.212,-705.6287;Float;False;Property;_Speed;Speed;13;0;0;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.TexCoordVertexDataNode;137;-1994.631,-1160.288;Float;False;0;2;0;5;FLOAT2;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;132;-1761.212,-778.6287;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-1747.208,-1018.53;Float;False;2;2;0;FLOAT2;0.0;False;1;FLOAT;0,0;False;1;FLOAT2
Node;AmplifyShaderEditor.PannerNode;69;-1495.803,-905.8242;Float;False;1;0;2;0;FLOAT2;0,0;False;1;FLOAT;0.0;False;1;FLOAT2
Node;AmplifyShaderEditor.RangedFloatNode;26;-1631.707,-196.9273;Float;False;Property;_Cloud_cleareness;Cloud_cleareness;11;0;0.5162644;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.PannerNode;68;-1484.603,-1025.224;Float;False;0;1;2;0;FLOAT2;0,0;False;1;FLOAT;0.0;False;1;FLOAT2
Node;AmplifyShaderEditor.SimpleAddOpNode;103;-1182.01,-200.1269;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;-1.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-1276.208,-1017.13;Float;False;2;2;0;FLOAT2;0.0;False;1;FLOAT2;1,1;False;1;FLOAT2
Node;AmplifyShaderEditor.TexturePropertyNode;1;-1477,-707.3001;Float;True;Property;_Texture0;Texture 0;7;0;None;False;white;LockedToTexture2D;0;1;SAMPLER2D
Node;AmplifyShaderEditor.RangedFloatNode;102;-1264.709,-14.52677;Float;False;Property;_Cloud_density;Cloud_density;12;0;-10;0;10;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-1274.808,-910.7296;Float;False;2;2;0;FLOAT2;0.0;False;1;FLOAT2;2,2;False;1;FLOAT2
Node;AmplifyShaderEditor.SamplerNode;12;-1012.509,-743.5298;Float;True;Property;_TextureSample2;Texture Sample 2;1;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;2;-1011,-945.9996;Float;True;Property;_TextureSample0;Texture Sample 0;1;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;104;-962.3093,-37.627;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;-1.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;98;-1037.41,-167.7278;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;1.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;30;-626.9081,-1030.327;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0,0,0,0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;129;-1308.912,-341.2272;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.5;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;101;-819.9099,-270.3268;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;100;-822.3104,-169.9266;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.TFHCRemap;95;-657.7103,-716.4265;Float;False;5;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;2.0;False;3;FLOAT;0.0;False;4;FLOAT;1.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;128;-1162.611,-367.8272;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;126;-409.0117,-923.9272;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.OneMinusNode;117;-487.6115,-803.627;Float;False;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.ColorNode;11;-576.8091,-243.6302;Float;False;Property;_Cloud_color;Cloud_color;8;0;0,0,0,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;108;-288.2089,-798.9267;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;1;FLOAT
Node;AmplifyShaderEditor.RegisterLocalVarNode;121;-75.11207,-920.3285;Float;False;Cloud_occlusion;-1;True;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;42;-294.107,-587.7267;Float;False;3;0;FLOAT;0,0,0,0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;1;FLOAT
Node;AmplifyShaderEditor.GetLocalVarNode;122;-560,-64;Float;False;121;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;114;-357.4097,-184.0258;Float;False;2;2;0;COLOR;0.0;False;1;FLOAT;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.GetLocalVarNode;10;-330.2085,39.4704;Float;False;9;0;1;FLOAT3
Node;AmplifyShaderEditor.GetLocalVarNode;61;-611.0052,232.9773;Float;False;60;0;1;FLOAT
Node;AmplifyShaderEditor.RegisterLocalVarNode;9;-340.5876,-1258.729;Float;False;Cloud_normal;-1;True;1;0;FLOAT3;0,0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.GetLocalVarNode;7;-417.9179,325.8304;Float;False;8;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;-316.0052,182.7766;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;120;-190.0121,-121.0271;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.RangedFloatNode;56;-599.0049,72.27612;Float;False;Constant;_Float0;Float 0;5;0;0;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;67;-562.2032,443.3761;Float;False;Constant;_Float1;Float 1;5;0;0.5;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;123;-632.4117,-1308.628;Float;False;2;2;0;FLOAT3;0.0;False;1;FLOAT3;0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.TexturePropertyNode;110;-1519.908,-1385.326;Float;True;Property;_Cloud_normal_map;Cloud_normal_map;10;0;None;True;bump;Auto;0;1;SAMPLER2D
Node;AmplifyShaderEditor.SamplerNode;111;-1027.11,-1438.126;Float;True;Property;_TextureSample1;Texture Sample 1;6;0;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;125;-491.7118,-1272.227;Float;False;2;2;0;FLOAT3;0.0;False;1;FLOAT3;0.2,0.2,0.2;False;1;FLOAT3
Node;AmplifyShaderEditor.NormalizeNode;130;-602.313,-1435.529;Float;False;1;0;FLOAT3;0,0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.RegisterLocalVarNode;8;-43.89769,-615.4102;Float;False;Cloud_mask;-1;True;1;0;FLOAT;0,0,0,0;False;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;112;-1020.71,-1230.127;Float;True;Property;_TextureSample3;Texture Sample 3;6;0;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.LightColorNode;6;-583.8748,152.9253;Float;False;0;1;COLOR
Node;AmplifyShaderEditor.RegisterLocalVarNode;60;-51.40507,-750.4222;Float;False;Diffusion;-1;True;1;0;FLOAT;0,0,0;False;1;FLOAT
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;7;Float;ASEMaterialInspector;0;Standard;Arno/Cloud_dome2;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;Back;0;0;False;0;0;Transparent;0.5;True;True;0;False;Transparent;Transparent;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;False;0;4;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;False;Relative;0;;-1;0;-1;-1;0;0;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;OBJECT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;132;0;71;0
WireConnection;132;1;131;0
WireConnection;17;0;137;0
WireConnection;17;1;18;0
WireConnection;69;0;17;0
WireConnection;69;1;132;0
WireConnection;68;0;17;0
WireConnection;68;1;132;0
WireConnection;103;0;26;0
WireConnection;15;0;68;0
WireConnection;16;0;69;0
WireConnection;12;0;1;0
WireConnection;12;1;16;0
WireConnection;2;0;1;0
WireConnection;2;1;15;0
WireConnection;104;0;102;0
WireConnection;98;0;103;0
WireConnection;30;0;2;1
WireConnection;30;1;12;1
WireConnection;129;0;26;0
WireConnection;101;0;103;0
WireConnection;101;1;104;0
WireConnection;100;0;98;0
WireConnection;100;1;104;0
WireConnection;95;0;30;0
WireConnection;95;3;101;0
WireConnection;95;4;100;0
WireConnection;128;0;129;0
WireConnection;126;0;30;0
WireConnection;126;1;128;0
WireConnection;117;0;95;0
WireConnection;108;0;117;0
WireConnection;121;0;126;0
WireConnection;42;0;95;0
WireConnection;114;0;11;0
WireConnection;114;1;122;0
WireConnection;9;0;130;0
WireConnection;59;0;6;0
WireConnection;59;1;61;0
WireConnection;120;0;114;0
WireConnection;120;1;122;0
WireConnection;123;0;111;0
WireConnection;123;1;112;0
WireConnection;111;0;110;0
WireConnection;111;1;15;0
WireConnection;125;0;123;0
WireConnection;130;0;125;0
WireConnection;8;0;42;0
WireConnection;112;0;110;0
WireConnection;112;1;16;0
WireConnection;60;0;108;0
WireConnection;0;0;114;0
WireConnection;0;3;56;0
WireConnection;0;4;56;0
WireConnection;0;5;122;0
WireConnection;0;7;61;0
WireConnection;0;9;7;0
ASEEND*/
//CHKSM=5B52C330A437745C29E80A7380C06324DD208FD7