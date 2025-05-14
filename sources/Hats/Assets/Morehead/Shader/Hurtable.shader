Shader "HHH/Hurtable"
{
	Properties
	{
		_ColorOverlay("Color Overlay", Color) = (1,0,0,0)
		_ColorOverlayAmount("Color Overlay Amount", Range( 0 , 1)) = 0
		[Header(Albedo)][SingleLineTexture]_AlbedoTexture("Albedo Texture", 2D) = "white" {}
		_AlbedoColor("Albedo Color", Color) = (1,1,1,0)
		[Header(Normal)][SingleLineTexture]_NormalTexture("Normal Texture", 2D) = "bump" {}
		_NormalStrength("Normal Strength", Range( -5 , 5)) = 0
		[Header(Metallic)][SingleLineTexture]_MetallicTexture("Metallic Texture", 2D) = "white" {}
		_Metallic("Metallic", Range( 0 , 1)) = 0
		[Header(Smoothness)][SingleLineTexture]_SmoothnessTexture("Smoothness Texture", 2D) = "white" {}
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		[Header(Tiling)]_TilingX("Tiling X", Float) = 1
		_TilingY("Tiling Y", Float) = 1
		[Header(Offset)]_OffsetX("Offset X", Float) = 1
		_OffsetY("Offset Y", Float) = 1
		[Header(Emission)][SingleLineTexture]_EmissionTexture("Emission Texture", 2D) = "white" {}
		[HDR]_EmissionColor("Emission Color", Color) = (0,0,0,1)
		[Header(Fresnel)]_FresnelAmount("Fresnel Amount", Range( 0 , 1)) = 0
		_FresnelScale("FresnelScale", Float) = 0
		_FresnelPower("FresnelPower", Float) = 0
		_FresnelBias("FresnelBias", Float) = 0
		_FresnelColor("FresnelColor", Color) = (0,0,0,0)
		_FresnelEmission("Fresnel Emission", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			INTERNAL_DATA
			float3 worldNormal;
		};

		uniform sampler2D _NormalTexture;
		uniform float _TilingX;
		uniform float _TilingY;
		uniform float _OffsetX;
		uniform float _OffsetY;
		uniform float _NormalStrength;
		uniform float _ColorOverlayAmount;
		uniform float4 _ColorOverlay;
		uniform float _FresnelBias;
		uniform float _FresnelScale;
		uniform float _FresnelPower;
		uniform float4 _FresnelColor;
		uniform float _FresnelAmount;
		uniform float4 _AlbedoColor;
		uniform sampler2D _AlbedoTexture;
		uniform float _FresnelEmission;
		uniform sampler2D _EmissionTexture;
		uniform float4 _EmissionColor;
		uniform float _Metallic;
		uniform sampler2D _MetallicTexture;
		uniform half _Smoothness;
		uniform sampler2D _SmoothnessTexture;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 appendResult45 = (float2(_TilingX , _TilingY));
			float2 appendResult46 = (float2(( _OffsetX - 1.0 ) , ( _OffsetY - 1.0 )));
			float2 uv_TexCoord43 = i.uv_texcoord * appendResult45 + appendResult46;
			float2 TilingOffset51 = uv_TexCoord43;
			float3 tex2DNode15 = UnpackScaleNormal( tex2D( _NormalTexture, TilingOffset51 ), _NormalStrength );
			o.Normal = tex2DNode15;
			float4 temp_output_20_0 = ( _ColorOverlayAmount * _ColorOverlay );
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_tangentToWorldFast = float3x3(ase_worldTangent.x,ase_worldBitangent.x,ase_worldNormal.x,ase_worldTangent.y,ase_worldBitangent.y,ase_worldNormal.y,ase_worldTangent.z,ase_worldBitangent.z,ase_worldNormal.z);
			float fresnelNdotV1 = dot( mul(ase_tangentToWorldFast,tex2DNode15), ase_worldViewDir );
			float fresnelNode1 = ( _FresnelBias + _FresnelScale * pow( 1.0 - fresnelNdotV1, _FresnelPower ) );
			float clampResult36 = clamp( fresnelNode1 , 0.0 , 1.0 );
			float4 temp_output_29_0 = ( ( clampResult36 * _FresnelColor ) * _FresnelAmount );
			o.Albedo = ( temp_output_20_0 + ( temp_output_29_0 + ( _AlbedoColor * tex2D( _AlbedoTexture, TilingOffset51 ) ) ) ).rgb;
			o.Emission = ( ( temp_output_20_0 + ( ( _FresnelEmission * temp_output_29_0 ) + float4( 0,0,0,0 ) ) ) + ( ( tex2D( _EmissionTexture, TilingOffset51 ) * _EmissionColor ) * _EmissionColor.a ) ).rgb;
			o.Metallic = ( _Metallic * tex2D( _MetallicTexture, TilingOffset51 ) ).r;
			o.Smoothness = ( _Smoothness * tex2D( _SmoothnessTexture, TilingOffset51 ) ).r;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

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
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
