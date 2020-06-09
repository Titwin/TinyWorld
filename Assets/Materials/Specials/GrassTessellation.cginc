// Tessellation programs based on this article by Catlike Coding:
// https://catlikecoding.com/unity/tutorials/advanced-rendering/tessellation/

struct vertexInput
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float2 uv : TEXCOORD0;
};

struct vertexOutput
{
	float4 vertex : SV_POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float4 uv : TEXCOORD0;
};

struct TessellationFactors 
{
	float edge[3] : SV_TessFactor;
	float inside : SV_InsideTessFactor;
};

vertexOutput vert(vertexInput v)
{
	vertexOutput o;
	o.vertex = v.vertex;
	o.normal = v.normal;
	o.tangent = v.tangent;
	o.uv = float4(v.uv, 0, 0);
	return o;
}

#define TRAJECTORY_SAMPLES 64


float _TessellationUniform;
int _PathTrajectoriesSize;
float4 _PathTrajectories[32* TRAJECTORY_SAMPLES];
float _PathWidth;

float2 computePathParameters(vertexOutput v)
{
	float3 vertex = mul(unity_ObjectToWorld, v.vertex);
	float2 best = float2(1, 0);
	
	for (int i = 0; i < _PathTrajectoriesSize; i++)
	{
		float3 p1 = _PathTrajectories[TRAJECTORY_SAMPLES * i + TRAJECTORY_SAMPLES - 1].xyz;
		float bend1 = _PathTrajectories[TRAJECTORY_SAMPLES * i + TRAJECTORY_SAMPLES - 1].w;
		for (int j = TRAJECTORY_SAMPLES - 2; j >= 0; j--)
		{
			float3 p2 = _PathTrajectories[TRAJECTORY_SAMPLES * i + j].xyz;
			float bend2 = _PathTrajectories[TRAJECTORY_SAMPLES * i + j].w;
			float d = length(p2 - p1);

			if (d > 0)
			{
				float lambda = saturate(dot(vertex - p1, p2 - p1) / (d*d));
				float3 closest = lerp(p1, p2, lambda);
				float d2 = length(vertex - closest) / _PathWidth;
				float bend = saturate(1 + lerp(bend1, bend2, lambda));

				if (d2 < 1 && bend >= best.y)
				{
					best = float2(d2, bend);
				}
			}

			p1 = p2;
			bend1 = bend2;
		}
	}
	return best;
}
vertexOutput tessVert(vertexOutput v)
{
	vertexOutput o;
	o.vertex = v.vertex;
	o.normal = v.normal;
	o.tangent = v.tangent;
	o.uv = float4(v.uv.x, v.uv.y, computePathParameters(v).y, 0);
	return o;
}
TessellationFactors patchConstantFunction (InputPatch<vertexOutput, 3> patch)
{
	TessellationFactors f;
	f.edge[0] =  _TessellationUniform;
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
vertexOutput hull (InputPatch<vertexOutput, 3> patch, uint id : SV_OutputControlPointID)
{
	return patch[id];
}

[UNITY_domain("tri")]
vertexOutput domain(TessellationFactors factors, OutputPatch<vertexOutput, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
{
	vertexOutput v;

	#define MY_DOMAIN_PROGRAM_INTERPOLATE(fieldName) v.fieldName = \
		patch[0].fieldName * barycentricCoordinates.x + \
		patch[1].fieldName * barycentricCoordinates.y + \
		patch[2].fieldName * barycentricCoordinates.z;

	MY_DOMAIN_PROGRAM_INTERPOLATE(vertex)
	MY_DOMAIN_PROGRAM_INTERPOLATE(normal)
	MY_DOMAIN_PROGRAM_INTERPOLATE(tangent)
	MY_DOMAIN_PROGRAM_INTERPOLATE(uv)

	return tessVert(v);
}
