// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Perso/Water"
{
	Properties
	{
		[Header(Shading)]
		_Color1("Color1", Color) = (1,1,1,1)
		_Color2("Color2", Color) = (1,1,1,1)
		_Smoothness("Smoothness", Range(0,1)) = 0.5
		_TranslucentGain("Translucent Gain", Range(0,1)) = 0.5

		[Header(Geometry and noise)]
		_TessellationUniform("Tessellation Uniform (blade density)", Range(1, 64)) = 1
		_Frequency("Frequency", Range(0, 30)) = 1

		[Header(Environement)]
		_NoiseStrength("Noise Strength", Range(0, 10)) = 0.01
		_Speed("Speed", Range(0, 1)) = 1
		_Shadows("Shadows", Range(0, 1)) = 0.5
	}


	CGINCLUDE
	#include "UnityCG.cginc"
	#include "Autolight.cginc"
	#include "noiseSimplex.cginc"

		float _TessellationUniform;

		struct vertexInput
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
		};
		struct vertexOutput
		{
			float4 vertex : SV_POSITION;
			float3 normal : NORMAL;
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
			float3 srcPos : TEXCOORD2;
			unityShadowCoord4 _ShadowCoord : TEXCOORD3;
		};

		vertexOutput vert(vertexInput v)
		{
			vertexOutput o;
			o.vertex = v.vertex;
			o.normal = v.normal;
			return o;
		}
		tessellationFactors patchConstantFunction(InputPatch<vertexOutput, 3> patch)
		{
			tessellationFactors f;
			f.edge[0] = _TessellationUniform;
			f.edge[1] = _TessellationUniform;
			f.edge[2] =  _TessellationUniform;
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
			v.normal = patch[0].normal * barycentricCoordinates.x + patch[1].normal * barycentricCoordinates.y + patch[2].normal * barycentricCoordinates.z;
			return v;
		}


		float4 _Color1; float4 _Color2;
		float _Smoothness;
		float _TranslucentGain;
		float _Speed;
		float _NoiseStrength;
		float _Frequency;
		float _Shadows;

		geometryOutput VertexOutput(float3 pos, float3 normal)
		{
			geometryOutput o;
			o.pos = UnityObjectToClipPos(pos);
			o.normal = UnityObjectToWorldNormal(normal);
			o._ShadowCoord = ComputeScreenPos(o.pos);
			o.uv = float4(length(UnityObjectToViewPos(pos).xyz), 0, 0, 0);
			o.worldRefl = reflect(-normalize(UnityWorldSpaceViewDir(o.pos)), o.normal);
			o.srcPos = mul(unity_ObjectToWorld, float4(pos, 1)) * _Frequency + _Time.y * _Speed;
			return o;
		}
		[maxvertexcount(3)]
		void geo(triangle vertexOutput IN[3] : SV_POSITION, inout TriangleStream<geometryOutput> output)
		{
			output.Append(VertexOutput(IN[0].vertex, normalize(IN[0].normal)));
			output.Append(VertexOutput(IN[1].vertex, normalize(IN[1].normal)));
			output.Append(VertexOutput(IN[2].vertex, normalize(IN[2].normal)));
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
				float3 seed1 = float3(i.srcPos.xz, 0) * _Frequency + _Time.y * _Speed;
				float3 seed2 = float3(i.srcPos.zx, 0) * 0.25 * _Frequency - 2 * _Time.y * _Speed;
				float ns = snoise(seed1) * _NoiseStrength;
				float4 waterColor = _Color1;

				if (ns < 0 || ns > 1) waterColor = _Color1;
				else if (ns < 0.5) waterColor = lerp(_Color1, _Color2, 2 * ns);
				else waterColor = lerp(_Color1, _Color2, 1 - 2 * (ns - 0.5));

				float3 normal = normalize(i.normal + _Shadows * float3(snoise(seed2), 0, snoise(seed2+ 1000)));
				float dotresult = dot(normal, _WorldSpaceLightPos0);
				float NdotL = saturate(saturate(dotresult) + _TranslucentGain);

				float4 col = waterColor * NdotL;

				float4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.worldRefl);
				float3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);
				col = float4(lerp(col.xyz, skyColor, _Smoothness), waterColor.w);

				col = col + _Smoothness * float4(skyColor, 1);
				

				float fogFactor = (unity_FogParams.x * i.uv.x);
				fogFactor = saturate(exp(-fogFactor * fogFactor));
				col = lerp(col, unity_FogColor, 1 - fogFactor);



				return col;
			}
			ENDCG
		}
    }
}
