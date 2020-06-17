// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Perso/Water"
{
	Properties
	{
		[Header(Shading)]
		_Color("Color", Color) = (1,1,1,1)
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_TranslucentGain("Translucent Gain", Range(0,1)) = 0.5

		[Header(Geometry)]
		_TessellationUniform("Tessellation Uniform (blade density)", Range(1, 64)) = 1

		[Header(Environement)]
		_DistortionMap("Distortion Map", 2D) = "white" {}
		_NoiseFrequency("Noise Frequency", Vector) = (0.05, 0.05, 0, 0)
		_NoiseStrength("Noise Strength", Range(-2, 2)) = 0.01
	}


	CGINCLUDE
	#include "UnityCG.cginc"
	#include "Autolight.cginc"

		float _TessellationUniform;

		struct vertexInput
		{
			float4 vertex : POSITION;
		};
		struct vertexOutput
		{
			float4 vertex : SV_POSITION;
		};
		struct tessellationFactors
		{
			float edge[3] : SV_TessFactor;
			float inside : SV_InsideTessFactor;
		};
		struct geometryOutput
		{
			float4 pos : SV_POSITION;
			float3 normal : NORMAL;
			float4 uv : TEXCOORD0;
			float3 worldRefl: TEXCOORD1;
			unityShadowCoord4 _ShadowCoord : TEXCOORD2;
		};

		vertexOutput vert(vertexInput v)
		{
			vertexOutput o;
			o.vertex = v.vertex;
			return o;
		}
		tessellationFactors patchConstantFunction(InputPatch<vertexOutput, 3> patch)
		{
			tessellationFactors f;
			f.edge[0] = _TessellationUniform;
			f.edge[1] = _TessellationUniform;
			f.edge[2] = _TessellationUniform;
			f.inside = _TessellationUniform;
			return f;
		}

		[UNITY_domain("tri")]
		[UNITY_outputcontrolpoints(3)]
		[UNITY_outputtopology("triangle_cw")]
		[UNITY_partitioning("integer")]
		[UNITY_patchconstantfunc("patchConstantFunction")]
		vertexOutput hull(InputPatch<vertexOutput, 3> patch, uint id : SV_OutputControlPointID)
		{
			return patch[id];
		}

		[UNITY_domain("tri")]
		vertexOutput domain(tessellationFactors factors, OutputPatch<vertexOutput, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
		{
			vertexOutput v;

			v.vertex = patch[0].vertex * barycentricCoordinates.x + patch[1].vertex * barycentricCoordinates.y + patch[2].vertex * barycentricCoordinates.z;
			return v;
		}


		float4 _Color;
		float _Glossiness;
		float _TranslucentGain;

		sampler2D _DistortionMap;
		float4 _DistortionMap_ST;
		float4 _NoiseFrequency;
		float _NoiseStrength;

		geometryOutput VertexOutput(float3 pos, float3 normal)
		{
			geometryOutput o;
			o.pos = UnityObjectToClipPos(pos);
			o.normal = UnityObjectToWorldNormal(normal);
			o._ShadowCoord = ComputeScreenPos(o.pos);
			o.uv = float4(length(UnityObjectToViewPos(pos).xyz), 0, 0, 0);
			o.worldRefl = reflect(-normalize(UnityWorldSpaceViewDir(o.pos)), o.normal);

			return o;
		}
		float getDisplacement(float3 wpos)
		{
			float2 uv = wpos.xz * _DistortionMap_ST.xy + _NoiseFrequency.xy * _Time.y;
			return tex2Dlod(_DistortionMap, float4(uv, 0, 0)).y * 2 - 1;

			/*float phase = _Time * 20.0;
			float offset1 = _NoiseFrequency.x * wpos.x + _NoiseFrequency.y * wpos.z;
			float offset2 = _NoiseFrequency.z * wpos.x + _NoiseFrequency.w * wpos.z;

			return sin(phase + offset1) * 0.2 + sin(phase + offset2) * 0.2;*/
		}
		geometryOutput GenerateVertexFrom(float4 pos)
		{
			float4 p = mul(unity_ObjectToWorld, pos);
			float3 displacement = float3(0, getDisplacement(p.xyz) * _NoiseStrength, 0);

			float z1 = getDisplacement(p.xyz + float3(-_NoiseFrequency.w, 0, 0));
			float z2 = getDisplacement(p.xyz + float3(_NoiseFrequency.w, 0, 0));
			float z3 = getDisplacement(p.xyz + float3( 0, 0, -_NoiseFrequency.w));
			float z4 = getDisplacement(p.xyz + float3( 0, 0, _NoiseFrequency.w));

			float3 normal = normalize(float3((z2 - z1) * _NoiseStrength, 4 * _NoiseFrequency.w, (z4 - z3) * _NoiseStrength).zyx);
			return VertexOutput(pos.xyz + displacement, normal);
		}
		[maxvertexcount(3)]
		void geo(triangle vertexOutput IN[3] : SV_POSITION, inout TriangleStream<geometryOutput> output)
		{
			output.Append(GenerateVertexFrom(IN[0].vertex));
			output.Append(GenerateVertexFrom(IN[1].vertex));
			output.Append(GenerateVertexFrom(IN[2].vertex));
		}
		ENDCG



    SubShader
    {
		Tags { "Queue" = "Transparent" "DisableBatching" = "True"}
		LOD 200

		Pass
		{
			Tags { "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			#pragma vertex vert
			#pragma hull hull
			#pragma domain domain
			#pragma geometry geo
			#pragma fragment frag
			#pragma target 4.6
			#pragma multi_compile_fwdbase

			#include "Lighting.cginc"

			float4 frag(geometryOutput i, fixed facing : VFACE) : SV_Target
			{
				float3 normal = i.normal;// normalize(facing > 0 ? i.normal : -i.normal);
				float shadow = SHADOW_ATTENUATION(i);
				float dotresult = dot(normal, _WorldSpaceLightPos0);
				dotresult *= dotresult < 0 ? -1 : 1;
				float NdotL = saturate(saturate(dotresult) + _TranslucentGain) * shadow;

				float3 ambient = ShadeSH9(float4(normal, 1));
				float4 lightIntensity = (NdotL * _LightColor0 + float4(ambient, 1)) * shadow;
				float4 col = _Color * lightIntensity;

				float4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.worldRefl);
				float3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);
				col = float4(lerp(col.xyz, skyColor, _Glossiness), _Color.w) * _LightColor0;

				float fogFactor = (unity_FogParams.x * i.uv.x);
				fogFactor = saturate(exp(-fogFactor * fogFactor));
				col = lerp(col, unity_FogColor, 1 - fogFactor);

				return col;
			}
			ENDCG
		}
    }
}
