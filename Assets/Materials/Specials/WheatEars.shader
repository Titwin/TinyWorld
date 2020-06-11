Shader "Perso/WheatEars"
{
	Properties
	{
		[Header(Shading)]
		_YoungColor("Youngalbedo color", Color) = (1,1,1,1)
		_FinalColor("Final albedo color", Color) = (1,1,1,1)
		_TranslucentGain("Translucent Gain", Range(0,1)) = 0.5

		[Header(Geometry)]
		_h1("h1", Range(0,1)) = 0.2
		_w2("w2", Range(0.002,0.1)) = 0.02
		_h2("h2", Range(0.002,0.5)) = 0.1
		_lambda2("lambda2", Range(0.002,0.2)) = 0.2
		_lambda3("lambda3", Range(0.002,1)) = 0
		_growth("Growth", Range(0,1)) = 0
			
		[Header(Geometry deforming)]
		_BendRotationRandom("Bend Rotation Random", Range(0, 1)) = 0.2
		_EarBendRotationRandom("EarBend Rotation Random", Range(0, 1)) = 0.2
		_TessellationUniform("Tessellation Uniform (blade density)", Range(1, 10)) = 1
		_BladeForward("Blade Forward Amount", Float) = 0.38
		_BladeCurve("Blade Curvature Amount", Range(1, 4)) = 2

		[Header(Environement)]
		_WindDistortionMap("Wind Distortion Map", 2D) = "white" {}
		_WindFrequency("Wind Frequency", Vector) = (0.05, 0.05, 0, 0)
		_WindStrength("Wind Strength", Range(-0.5, 0.5)) = 0.01
		_PathTrajectoriesSize("Number of trajectories", Int) = 1
		_PathWidth("Path width", Range(0.001, 5.0)) = 1.0
		_SmashedAngle("Smashed blade angle", Range(-3.14, 3.14)) = 0.7
		_SmashedTest("_SmashedTest", Range(0,1)) = 0
	}

		CGINCLUDE
#include "UnityCG.cginc"
#include "Autolight.cginc"

			struct geometryOutput
		{
			float4 pos : SV_POSITION;
			float2 normal : NORMAL;
			unityShadowCoord4 _ShadowCoord : TEXCOORD1;
		};

		// parameters
		float4 _YoungColor;
		float4 _FinalColor;
		float _TranslucentGain;

		float _h1;
		float _h2; float _w2; float _lambda2; float _lambda3;
		float _growth;
		float _BladeForward;
		float _BladeCurve;

		float _BendRotationRandom; float _EarBendRotationRandom;

		sampler2D _WindDistortionMap;
		float4 _WindDistortionMap_ST;
		float2 _WindFrequency;
		float _WindStrength;
		float _SmashedAngle;

		float _SmashedTest;

		#include "GrassTessellation.cginc"


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

		float3 earPos(float3 from, float w, float h, int segment, float lambda, float ratio)
		{
			return saturate(w + lambda) * (1 - ratio * segment) * from + (1 - ratio * segment) * segment * float3(0, h, 0);
		}
		float4 getColor()
		{
			return lerp(_YoungColor, _FinalColor, saturate(7.14 *(_growth - 0.86)));
		}
		float getH1()
		{
			if (_growth > 0.71) return _h1;
			else if (_growth > 0.42) return _h1 * lerp(0.37, 1, saturate((_growth - 0.42) / (0.71 - 0.42)));
			else return _h1 * lerp(0, 0.37, _growth / 0.42);
		}

		float getW2()
		{
			if (_growth > 0.86) return _w2;
			else if (_growth > 0.71) return _w2 * lerp(0.6, 1, saturate((_growth - 0.71) / (0.86 - 0.71)));
			else if (_growth > 0.54) return _w2 * lerp(0.1, 0.6, saturate((_growth - 0.54) / (0.71 - 0.54)));
			else return 0.1 * _w2;
		}
		float getH2()
		{
			if (_growth > 0.86) return _h2;
			else if (_growth > 0.71) return _h2 * lerp(0.6, 1, saturate((_growth - 0.71) / (0.86 - 0.71)));
			else if (_growth > 0.54) return _h2 * lerp(0.1, 0.6, saturate((_growth - 0.54) / (0.71 - 0.54)));
			else return _h2 * 0.1;
		}
		float getLambda2()
		{
			if (_growth > 0.86) return _lambda2;
			else if (_growth > 0.71) return _lambda2 * lerp(0, 1, saturate((_growth - 0.71) / (0.86 - 0.71)));
			else return 0;
		}


		geometryOutput VertexOutput(float3 position, float3 normal)
		{
			geometryOutput o;
			o.pos = UnityObjectToClipPos(position);
			o.normal = normalize(UnityObjectToWorldNormal(normal)).xy;
			o._ShadowCoord = ComputeScreenPos(o.pos);

			#if UNITY_PASS_SHADOWCASTER
				// Applying the bias prevents artifacts from appearing on the surface.
				o.pos = UnityApplyLinearShadowBias(o.pos);
			#endif

			return o;
		}
		geometryOutput GenerateVertex(float3 origin, float3 localPosition, float3 localNormal, float3x3 transformMatrix)
		{
			float3 position = origin + mul(transformMatrix, localPosition.xzy);
			return VertexOutput(position, localNormal);
		}

		[maxvertexcount(93)]
		void geo(triangle vertexOutput IN[3] : SV_POSITION, inout TriangleStream<geometryOutput> output) //OutputStream.RestartStrip();
		{
			float3 pos = 0.333 * (IN[0].vertex + IN[1].vertex + IN[2].vertex);
			//if (_growth > 0.71)
			{
				// local base
				float3 vNormal = IN[0].normal;
				float4 vTangent = IN[0].tangent;
				float3 vBinormal = cross(vNormal, vTangent) * vTangent.w;
				float3x3 tangentToLocal = float3x3(vTangent.x, vBinormal.x, vNormal.x, vTangent.y, vBinormal.y, vNormal.y, vTangent.z, vBinormal.z, vNormal.z);
				float3x3 facingRotationMatrix = AngleAxis3x3(rand(pos) * UNITY_TWO_PI, float3(0, 0, 1));

				// wind
				float windFactor = lerp(_WindStrength, 0.01, IN[0].uv.z);
				float2 uv = pos.xz * _WindDistortionMap_ST.xy + _WindDistortionMap_ST.zw + _WindFrequency * _Time.y;
				float2 windSample = (tex2Dlod(_WindDistortionMap, float4(uv, 0, 0)).yz * 2 - 1) * windFactor;
				float3 wind = normalize(float3(windSample.x, 0, windSample.y));
				wind = mul(unity_WorldToObject, wind);
				float3x3 windRotation = AngleAxis3x3(lerp((_WindStrength > 0 ? 1 : -1) *UNITY_PI * windSample, 0, _SmashedTest), wind.xzy);

				// matrices
				float3x3 transformationMatrixFacing = mul(tangentToLocal, facingRotationMatrix);
				float bendAngle = lerp(rand(pos.zzx) * _BendRotationRandom * UNITY_PI * 0.5, _SmashedAngle, IN[0].uv.z);
				float3x3 earBendAngle = rand(pos.zzx) * _EarBendRotationRandom * UNITY_PI * 0.5;
				float3x3 bendRotationMatrix = AngleAxis3x3(bendAngle, float3(-1, 0, 0));
				float3x3 earBendRotationMatrix = AngleAxis3x3(earBendAngle, float3(0, 1, 0));
				float3x3 transformationMatrix = mul(mul(mul(tangentToLocal, bendRotationMatrix), facingRotationMatrix), windRotation);

				// geometry parameters
				float scale = 1 + 0.3 * rand(pos.yxz);
				float h1 = scale * getH1();
				float w2 = scale * getW2();
				float h2 = scale * getH2();
				float lambda2 = scale * getLambda2();
				float lambda3 = _lambda3;
				float u = 1 - _lambda2 * lambda2;

				float3 n0 = float3(0, -lambda2, -u);
				float3 n1 = float3(-u, -lambda2, 0);
				float3 n2 = float3(0, -lambda2, -u);
				float3 n3 = float3(-u, -lambda2, 0);

				float3 v0 = float3(-1, 0, -1);
				float3 v1 = float3(1, 0, -1);
				float3 v2 = float3(1, 0, 1);
				float3 v3 = float3(-1, 0, 1);
				float3 n;

				float3x3 bottomTransformationMatrix = transformationMatrixFacing;
				float3x3 topTransformationMatrix = transformationMatrix;
				for (int i = 0; i < 3; i++)
				{
					bottomTransformationMatrix = topTransformationMatrix;
					topTransformationMatrix = mul(topTransformationMatrix, bendRotationMatrix);
				}

				float3 origin = pos + mul(bottomTransformationMatrix, float3(0, 0, 3 * h1 - 0.01));
				bottomTransformationMatrix = mul(bottomTransformationMatrix, earBendRotationMatrix);
				topTransformationMatrix = mul(mul(topTransformationMatrix, earBendRotationMatrix), earBendRotationMatrix);

				n = mul(bottomTransformationMatrix, float3(0, 0, -1));
				output.Append(GenerateVertex(origin, saturate(w2 - 2 * lambda2) * v3, n, bottomTransformationMatrix));
				output.Append(GenerateVertex(origin, saturate(w2 - 2 * lambda2) * v2, n, bottomTransformationMatrix));
				output.Append(GenerateVertex(origin, saturate(w2 - 2 * lambda2) * v0, n, bottomTransformationMatrix));
				output.Append(GenerateVertex(origin, saturate(w2 - 2 * lambda2) * v1, n, bottomTransformationMatrix));
				output.RestartStrip();

				for (int i = 0; i < 4; i++)
				{
					float lambda0 = i == 0 ? -2 * lambda2 : 0;
					n = mul(bottomTransformationMatrix, n0);
					output.Append(GenerateVertex(origin, earPos(v0, w2, h2, i, lambda0, lambda3), n, bottomTransformationMatrix));
					output.Append(GenerateVertex(origin, earPos(v0, w2, h2, i + 1, lambda2, lambda3), n, topTransformationMatrix));
					output.Append(GenerateVertex(origin, earPos(v1, w2, h2, i, lambda0, lambda3), n, bottomTransformationMatrix));
					output.Append(GenerateVertex(origin, earPos(v1, w2, h2, i + 1, lambda2, lambda3), n, topTransformationMatrix));
					output.RestartStrip();

					n = mul(bottomTransformationMatrix, n1);
					output.Append(GenerateVertex(origin, earPos(v1, w2, h2, i, lambda0, lambda3), n, bottomTransformationMatrix));
					output.Append(GenerateVertex(origin, earPos(v1, w2, h2, i + 1, lambda2, lambda3), n, topTransformationMatrix));
					output.Append(GenerateVertex(origin, earPos(v2, w2, h2, i, lambda0, lambda3), n, bottomTransformationMatrix));
					output.Append(GenerateVertex(origin, earPos(v2, w2, h2, i + 1, lambda2, lambda3), n, topTransformationMatrix));
					output.RestartStrip();

					n = mul(bottomTransformationMatrix, n2);
					output.Append(GenerateVertex(origin, earPos(v2, w2, h2, i, lambda0, lambda3), n, bottomTransformationMatrix));
					output.Append(GenerateVertex(origin, earPos(v2, w2, h2, i + 1, lambda2, lambda3), n, topTransformationMatrix));
					output.Append(GenerateVertex(origin, earPos(v3, w2, h2, i, lambda0, lambda3), n, bottomTransformationMatrix));
					output.Append(GenerateVertex(origin, earPos(v3, w2, h2, i + 1, lambda2, lambda3), n, topTransformationMatrix));
					output.RestartStrip();

					n = mul(bottomTransformationMatrix, n3);
					output.Append(GenerateVertex(origin, earPos(v3, w2, h2, i, lambda0, lambda3), n, bottomTransformationMatrix));
					output.Append(GenerateVertex(origin, earPos(v3, w2, h2, i + 1, lambda2, lambda3), n, topTransformationMatrix));
					output.Append(GenerateVertex(origin, earPos(v0, w2, h2, i, lambda0, lambda3), n, bottomTransformationMatrix));
					output.Append(GenerateVertex(origin, earPos(v0, w2, h2, i + 1, lambda2, lambda3), n, topTransformationMatrix));
					output.RestartStrip();

					n = mul(bottomTransformationMatrix, float3(0, 0, 1));
					output.Append(GenerateVertex(origin, earPos(v3, w2, h2, i + 1, lambda2, lambda3), n, topTransformationMatrix));
					output.Append(GenerateVertex(origin, earPos(v2, w2, h2, i + 1, lambda2, lambda3), n, topTransformationMatrix));
					output.Append(GenerateVertex(origin, earPos(v0, w2, h2, i + 1, lambda2, lambda3), n, topTransformationMatrix));
					output.Append(GenerateVertex(origin, earPos(v1, w2, h2, i + 1, lambda2, lambda3), n, topTransformationMatrix));
					output.RestartStrip();

					bottomTransformationMatrix = topTransformationMatrix;
					topTransformationMatrix = mul(topTransformationMatrix, earBendRotationMatrix);
				}

				origin += mul(bottomTransformationMatrix, float3(0, 0, (1 - lambda3 * 4) * 4 * h2));
				float3 up = 0.2 * float3(0, h2, 0);
				v0 = 0.4 * w2 * v0;
				v1 = 0.4 * w2 * v1;
				v2 = 0.4 * w2 * v2;
				v3 = 0.4 * w2 * v3;
				n0 = float3(0, 0.4 * w2, up.y);
				n1 = float3(0.4 * w2, 0, up.y);
				n2 = float3(0, -0.4 * w2, up.y);
				n3 = float3(-0.4 * w2, 0, up.y);

				n = mul(bottomTransformationMatrix, n0);
				output.Append(GenerateVertex(origin, v0, n, bottomTransformationMatrix));
				output.Append(GenerateVertex(origin, up, n, bottomTransformationMatrix));
				output.Append(GenerateVertex(origin, v1, n, bottomTransformationMatrix));
				output.RestartStrip();

				n = mul(bottomTransformationMatrix, n1);
				output.Append(GenerateVertex(origin, v1, n, bottomTransformationMatrix));
				output.Append(GenerateVertex(origin, up, n, bottomTransformationMatrix));
				output.Append(GenerateVertex(origin, v2, n, bottomTransformationMatrix));
				output.RestartStrip();

				n = mul(bottomTransformationMatrix, n2);
				output.Append(GenerateVertex(origin, v2, n, bottomTransformationMatrix));
				output.Append(GenerateVertex(origin, up, n, bottomTransformationMatrix));
				output.Append(GenerateVertex(origin, v3, n, bottomTransformationMatrix));
				output.RestartStrip();

				n = mul(bottomTransformationMatrix, n3);
				output.Append(GenerateVertex(origin, v3, n, bottomTransformationMatrix));
				output.Append(GenerateVertex(origin, up, n, bottomTransformationMatrix));
				output.Append(GenerateVertex(origin, v0, n, bottomTransformationMatrix));
				output.RestartStrip();
			}
		}
		ENDCG



		SubShader
		{
			Tags {"PreviewType" = "Plane"} // "DisableBatching" = "True" 
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
				#pragma hull hull
				#pragma domain domain
				#pragma geometry geo
				#pragma fragment frag
				#pragma target 4.6
				#pragma multi_compile_fwdbase //multi_compile_fog

				#include "Lighting.cginc"

				float4 frag(geometryOutput i, fixed facing : VFACE) : SV_Target
				{
					float3 normal = float3(i.normal.x, i.normal.y, sqrt(1.01 - i.normal.x * i.normal.x - i.normal.y * i.normal.y));
					float shadow = SHADOW_ATTENUATION(i);
					float NdotL = saturate(saturate(dot(normal, _WorldSpaceLightPos0)) + _TranslucentGain) * shadow;

					float3 ambient = ShadeSH9(float4(normal, 1));
					float4 lightIntensity = NdotL * _LightColor0 + float4(ambient, 1);
					float4 col = getColor() * lightIntensity;

					float fogFactor = (unity_FogParams.x * i.pos.w);
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