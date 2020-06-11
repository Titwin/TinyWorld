Shader "Perso/WindEffect"
{
	Properties
	{
		[Header(Environement)]
		_ColorAtlas("Albedo atlas", 2D) = "white" {}
		_WindDistortionMap("Wind Distortion Map", 2D) = "white" {}
		_WindFrequency("Wind Frequency", Vector) = (0.05, 0.05, 0, 0)
		_WindStrength("Wind Strength", Range(-0.2, 0.2)) = 0.01
	}

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "Autolight.cginc"

	struct vertexInput
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float2 uv : TEXCOORD0;
	};
	struct vertexOutput
	{
		float4 vertex : SV_POSITION;
		float3 normal : NORMAL;
		float4 uv : TEXCOORD0;
	};
	struct geometryOutput
	{
		float4 pos : SV_POSITION;
		float4 uv : TEXCOORD0;
		float3 normal : NORMAL;
		unityShadowCoord4 _ShadowCoord : TEXCOORD1;
	};

	sampler2D _ColorAtlas;
	sampler2D _WindDistortionMap;
	float4 _WindDistortionMap_ST;
	float2 _WindFrequency;
	float _WindStrength;


	// Simple noise function, sourced from http://answers.unity.com/answers/624136/view.html
	// Extended discussion on this function can be found at the following link:
	// https://forum.unity.com/threads/am-i-over-complicating-this-random-function.454887/#post-2949326
	// Returns a number in the 0...1 range.
	float rand(float3 co)
	{
		return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
	}
	// Construct a rotation matrix that rotates around the provided axis, sourced from:
		// https://gist.github.com/keijiro/ee439d5e7388f3aafc5296005c8c3f33
	float3x3 AngleAxis3x3(float angle, float3 axis)
	{
		float c, s;
		sincos(angle, s, c);

		float t = 1 - c;
		float x = axis.x;
		float y = axis.y;
		float z = axis.z;

		return float3x3(
			t * x * x + c, t * x * y - s * z, t * x * z + s * y,
			t * x * y + s * z, t * y * y + c, t * y * z - s * x,
			t * x * z - s * y, t * y * z + s * x, t * z * z + c
			);
	}


	vertexOutput vert(vertexInput v)
	{
		vertexOutput o;
		o.vertex = v.vertex;
		o.normal = v.normal;
		o.uv = float4(v.uv, 0, 0);
		return o;
	}
	geometryOutput VertexOutput(float3 pos, float2 uv, float3 normal)
	{
		geometryOutput o;
		o.pos = UnityObjectToClipPos(pos);
		o.uv = float4(uv.x, uv.y, length(UnityObjectToViewPos(pos).xyz) , 0);
		o.normal = UnityObjectToWorldNormal(normal);
		o._ShadowCoord = ComputeScreenPos(o.pos);

		#if UNITY_PASS_SHADOWCASTER
		// Applying the bias prevents artifacts from appearing on the surface.
				o.pos = UnityApplyLinearShadowBias(o.pos);
		#endif
		return o;
	}
	geometryOutput GenerateVertex(float3 vertexPosition, float3 normal, float2 uv)
	{
		// wind
		float3 pos = mul(unity_ObjectToWorld, float4(vertexPosition, 1)).xyz;
		float2 winduv = pos.xz * _WindDistortionMap_ST.xy + _WindDistortionMap_ST.zw + _WindFrequency * _Time.y;
		float windFactor = min(_WindStrength * pos.y, 0.2);
		float2 windSample = (tex2Dlod(_WindDistortionMap, float4(winduv, 0, 0)).yz * 2 - 1) * windFactor;
		float3 wind = normalize(float3(windSample.x, 0, windSample.y));
		wind = mul(unity_WorldToObject, wind);
		float3x3 windRotation = AngleAxis3x3((_WindStrength > 0 ? 1 : -1) * UNITY_PI * windSample, wind.xzy);

		return VertexOutput(mul(windRotation, vertexPosition), uv, mul(windRotation, normal));
	}

	[maxvertexcount(3)]
	void geo(triangle vertexOutput IN[3] : SV_POSITION, inout TriangleStream<geometryOutput> output)
	{
		output.Append(GenerateVertex(IN[0].vertex.xyz, IN[0].normal, IN[0].uv));
		output.Append(GenerateVertex(IN[1].vertex.xyz, IN[1].normal, IN[1].uv));
		output.Append(GenerateVertex(IN[2].vertex.xyz, IN[2].normal, IN[2].uv));
	}
	ENDCG

	SubShader
	{
		LOD 250

		Pass
		{
			Tags
			{
				"RenderType" = "Opaque"
				"LightMode" = "ForwardBase"
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geo
			#pragma fragment frag
			#pragma target 4.6
			#pragma multi_compile_fwdbase

			#include "Lighting.cginc"

			float4 frag(geometryOutput i, fixed facing : VFACE) : SV_Target
			{
				float shadow = SHADOW_ATTENUATION(i);
				float NdotL = saturate(saturate(dot(i.normal, _WorldSpaceLightPos0))) * shadow;

				float3 ambient = ShadeSH9(float4(i.normal, 1));
				float4 lightIntensity = NdotL * _LightColor0 + float4(ambient, 1);
				//float4 col = lerp(_TopColor1, _TopColor2, i.uv.w);
				float4 col = tex2D(_ColorAtlas, i.uv.xy);
				col = col * lightIntensity;

				float fogFactor = (unity_FogParams.x * i.uv.z);
				fogFactor = saturate(exp(-fogFactor * fogFactor));
				col = lerp(col, unity_FogColor, 1 - fogFactor);

				return col;
			}
			ENDCG
		}


		Pass
		{
			Tags
			{
				"LightMode" = "ShadowCaster"
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geo
			#pragma fragment frag
			#pragma target 4.6
			#pragma multi_compile_shadowcaster

			float4 frag(geometryOutput i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}

			ENDCG
		}
	}
}