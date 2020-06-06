Shader "Perso/Grass"
{
	Properties
	{
		[Header(Shading)]
		_TopColor("Top Color", Color) = (1,1,1,1)
		_BottomColor("Bottom Color", Color) = (1,1,1,1)
		_TranslucentGain("Translucent Gain", Range(0,1)) = 0.5

		[Header(Geometry)]
		_BladeComplexRadius("Blade LOD radius", Float) = 15
		_BladeWidth("Blade width", Float) = 0.2
		_BladeHeight("Blade height", Float) = 1.0
		_BendRotationRandom("Bend Rotation Random", Range(0, 1)) = 0.2
		_TessellationUniform("Tessellation Uniform (blade density)", Range(1, 64)) = 1
		_BladeForward("Blade Forward Amount", Float) = 0.38
		_BladeCurve("Blade Curvature Amount", Range(1, 4)) = 2

		[Header(Environement)]
		_WindDistortionMap("Wind Distortion Map", 2D) = "white" {}
		_WindFrequency("Wind Frequency", Vector) = (0.05, 0.05, 0, 0)
		_WindStrength("Wind Strength", Range(-0.5, 0.5)) = 0.01
		_PathTrajectory("Path trajectory", 2D) = "black" {}
		_PathWidth("Path width", Range(0.001, 5.0)) = 1.0
	}

		CGINCLUDE
		#include "UnityCG.cginc"
		#include "Autolight.cginc"

		#define BLADE_SEGMENTS 3

		struct geometryOutput
		{
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
			float3 normal : NORMAL;
			float2 fog : TEXCOORD2;
			float2 path : TEXCOORD3;
			unityShadowCoord4 _ShadowCoord : TEXCOORD1;
		};

		// parameters
		float4 _TopColor;
		float4 _BottomColor;
		float _TranslucentGain;

		int _ComplexBlade;
		float _BladeWidth;
		float _BladeHeight;
		float _BladeForward;
		float _BladeCurve;

		float _BendRotationRandom;
		float _BladeComplexRadius;

		sampler2D _WindDistortionMap;
		float4 _WindDistortionMap_ST;
		float2 _WindFrequency;
		float _WindStrength;

		#include "CustomTessellation.cginc"


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


		geometryOutput VertexOutput(float3 pos, float2 uv, float3 normal, float4 option)
		{
			geometryOutput o;
			o.pos = UnityObjectToClipPos(pos);
			o.uv = uv;
			o.normal = UnityObjectToWorldNormal(normal);
			o._ShadowCoord = ComputeScreenPos(o.pos);
			o.fog = option.xy;
			o.path = option.zw;

			#if UNITY_PASS_SHADOWCASTER
			// Applying the bias prevents artifacts from appearing on the surface.
				o.pos = UnityApplyLinearShadowBias(o.pos);
			#endif

			//UNITY_TRANSFER_FOG(o, o.pos);
			return o;
		}
		geometryOutput GenerateGrassVertex(float3 vertexPosition, float width, float height, float forward, float2 uv, float3x3 transformMatrix, float4 option)
		{
			float3 tangentPoint = float3(width, forward, height);
			float3 tangentNormal = normalize(float3(0, -1, forward));
			float3 localNormal = mul(transformMatrix, tangentNormal);

			float3 localPosition = vertexPosition + mul(transformMatrix, tangentPoint);
			return VertexOutput(localPosition, uv, localNormal, option);
		}
		[maxvertexcount(BLADE_SEGMENTS * 2 + 1)]
		void geo(triangle vertexOutput IN[3] : SV_POSITION, inout TriangleStream<geometryOutput> output)
		{
			// local base
			float3 pos = 0.333 * (IN[0].vertex + IN[1].vertex + IN[2].vertex);
			float3 vNormal = IN[0].normal;
			float4 vTangent = IN[0].tangent;
			float3 vBinormal = cross(vNormal, vTangent) * vTangent.w;
			float3x3 tangentToLocal = float3x3(vTangent.x, vBinormal.x, vNormal.x, vTangent.y, vBinormal.y, vNormal.y, vTangent.z, vBinormal.z, vNormal.z);
			float3x3 facingRotationMatrix = AngleAxis3x3(rand(pos) * UNITY_TWO_PI, float3(0, 0, 1));

			// wind
			float2 uv = pos.xz * _WindDistortionMap_ST.xy + _WindDistortionMap_ST.zw + _WindFrequency * _Time.y;
			float2 windSample = (tex2Dlod(_WindDistortionMap, float4(uv, 0, 0)).xy * 2 - 1) * _WindStrength;
			float3 wind = normalize(float3(windSample.x, 0, windSample.y));
			wind = mul(unity_WorldToObject, wind);
			float3x3 windRotation = AngleAxis3x3((_WindStrength > 0 ? 1 : -1) *UNITY_PI * windSample, wind.xzy);

			// end
			float3x3 transformationMatrixFacing = mul(tangentToLocal, facingRotationMatrix);
			float3x3 bendRotationMatrix = AngleAxis3x3(rand(pos.zzx) * _BendRotationRandom * UNITY_PI * 0.5, float3(-1, 0, 0));
			float3x3 transformationMatrix = mul(mul(mul(tangentToLocal, windRotation), facingRotationMatrix), bendRotationMatrix);
			float forward = rand(pos.yyz) * _BladeForward;
			float width = _BladeWidth;
			float height = _BladeHeight;
			float bladeDistanceFromCamera = length(UnityObjectToViewPos(pos).xyz);
			float4 option = float4(bladeDistanceFromCamera, 0, IN[0].path);
			
			// one grass string
			geometryOutput o;
			/*if (abs(length(pos.xz)) < 0.08)
			{
				float2 axis = wind.xz;
				float2 naxis = float2(-axis.y, axis.x);

				output.Append(GenerateGrassVertex(pos, -axis.x, 1, -axis.y, float2(0, 0), tangentToLocal));
				output.Append(GenerateGrassVertex(pos, axis.x + 0.5*naxis.x, 1, axis.y + 0.5*naxis.y, float2(0, 0), tangentToLocal));
				output.Append(GenerateGrassVertex(pos, axis.x - 0.5*naxis.x, 1, axis.y - 0.5*naxis.y, float2(0, 0), tangentToLocal));
			}
			else */if (bladeDistanceFromCamera < _BladeComplexRadius)
			{
				for (int i = 0; i < BLADE_SEGMENTS; i++)
				{
					float t = i / (float)BLADE_SEGMENTS;
					float segmentHeight = height * t;
					float segmentWidth = width * (1 - t);
					float segmentForward = pow(t, _BladeCurve) * forward;

					float3x3 transformMatrix = i == 0 ? transformationMatrixFacing : transformationMatrix;

					output.Append(GenerateGrassVertex(pos, segmentWidth, segmentHeight, segmentForward, float2(0, t), transformMatrix, option));
					output.Append(GenerateGrassVertex(pos, -segmentWidth, segmentHeight, segmentForward, float2(1, t), transformMatrix, option));
				}
				output.Append(GenerateGrassVertex(pos, 0, height, forward, float2(0.5, 1), transformationMatrix, option));
			}
			else
			{
				output.Append(GenerateGrassVertex(pos, width, 0, 0, float2(0, 0), transformationMatrixFacing, option));
				output.Append(GenerateGrassVertex(pos, -width, 0, 0, float2(1, 0), transformationMatrixFacing, option));
				output.Append(GenerateGrassVertex(pos, 0, height, forward, float2(0.5, 1), transformationMatrix, option));
			}

		}
			ENDCG

		SubShader
		{
			Cull Off
			Tags { "DisableBatching" = "True" "PreviewType" = "Plane"}
			LOD 100

			Pass
			{
				Tags
				{
					"RenderType" = "Opaque"
					"LightMode" = "ForwardBase"
				}

				CGPROGRAM
				#pragma vertex vert
				#pragma hull hull
				#pragma domain domain
				#pragma geometry geo
				#pragma fragment frag
				#pragma target 4.6
				#pragma multi_compile_fwdbase //multi_compile_fog

				#include "Lighting.cginc"

				float4 frag(geometryOutput i, fixed facing : VFACE) : SV_Target
				{
					float3 normal = facing > 0 ? i.normal : -i.normal;
					float shadow = SHADOW_ATTENUATION(i);
					float NdotL = saturate(saturate(dot(normal, _WorldSpaceLightPos0)) + _TranslucentGain) * shadow;

					float3 ambient = ShadeSH9(float4(normal, 1));
					float4 lightIntensity = NdotL * _LightColor0 + float4(ambient, 1);
					float4 col = lerp(_BottomColor, _TopColor * lightIntensity, i.uv.y);

					float fogFactor = (unity_FogParams.x * i.fog.x);
					fogFactor = saturate(exp(-fogFactor * fogFactor));
					col = lerp(col, unity_FogColor, 1 - fogFactor);

					col = float4(i.path, 0, 1);

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
				#pragma hull hull
				#pragma domain domain
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