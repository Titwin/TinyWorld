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
		_NormalFactor("Reflection factor", Range(0, 10)) = 0.01
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

			/*#define MY_DOMAIN_PROGRAM_INTERPOLATE(fieldName) v.fieldName = \
				patch[0].fieldName * barycentricCoordinates.x + \
				patch[1].fieldName * barycentricCoordinates.y + \
				patch[2].fieldName * barycentricCoordinates.z;

			MY_DOMAIN_PROGRAM_INTERPOLATE(vertex)*/

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
		float _NormalFactor;

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
		geometryOutput GenerateVertexFrom(float3 pos)
		{
			float4 p = mul(unity_ObjectToWorld, float4(pos, 1));
			float2 uv = p.xz * _DistortionMap_ST.xy + _DistortionMap_ST.zw + _NoiseFrequency.xy * _Time.y;
			float2 distortion = tex2Dlod(_DistortionMap, float4(uv, 0, 0)).yz * 2 - 1;
			float3 displacement = float3(0, distortion.y * _NoiseStrength, 0);
			uv = p.xz * _NormalFactor *_DistortionMap_ST.xy + _DistortionMap_ST.zw +  _NoiseFrequency.zw * _Time.y;
			distortion = tex2Dlod(_DistortionMap, float4(uv, 0, 0)).yz * 2 - 1;
			float3 normal = normalize(float3(distortion.x, 1, distortion.y));
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
				float3 normal = normalize(facing > 0 ? i.normal : -i.normal);
				float shadow = SHADOW_ATTENUATION(i);
				float dotresult = dot(normal, _WorldSpaceLightPos0);
				dotresult *= dotresult < 0 ? -1 : 1;
				float NdotL = saturate(saturate(dotresult) + _TranslucentGain) * shadow;

				float3 ambient = ShadeSH9(float4(normal, 1));
				float4 lightIntensity = NdotL * _LightColor0 + float4(ambient, 1);
				float4 col = _Color * shadow;

				float4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.worldRefl);
				float3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR) * _LightColor0;
				col = float4(lerp(col.xyz, skyColor, _Glossiness), col.w);

				float fogFactor = (unity_FogParams.x * i.uv.x);
				fogFactor = saturate(exp(-fogFactor * fogFactor));
				col = lerp(col, unity_FogColor, 1 - fogFactor);

				return col;
			}
			ENDCG
		}
    }
}
