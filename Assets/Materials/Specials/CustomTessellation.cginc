// Tessellation programs based on this article by Catlike Coding:
// https://catlikecoding.com/unity/tutorials/advanced-rendering/tessellation/

struct vertexInput
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
};

struct vertexOutput
{
	float4 vertex : SV_POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float2 path : TEXCOORD0;
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
	o.path = float3(0, 0, 0);
	return o;
}



float _TessellationUniform;
float3 _PathTrajectories[32*16];
float _PathWidth;



vertexOutput tessVert(vertexOutput v)
{
	vertexOutput o;

	float3 vertex = mul(unity_ObjectToWorld, v.vertex);
	float pathDistance = 0;
	float pathLength = 0;
	
	for (int i = 0; i < 32; i++)
		for (int j=0; j < 16; j++)
		{
			/*float currentLength = 0.0;
			float3 startPoint = 2*(tex2Dlod(_PathTrajectory, float4(step.x * i, step.y * j, 0, 0)).xyz * 2 - 1);

			for (int j = 1; j < 8 && pathLength == 0; j++)
			{
				float3 endPoint = 10 * (tex2Dlod(_PathTrajectory, float4(step.x * j, step.y * i, 0, 0)).xyz * 2 - 1);
				float d = distance(startPoint, endPoint);
			
				if (d > 0)
				{
					float3 p = mul(unity_ObjectToWorld, v.vertex);
					float lambda = saturate(dot(p - endPoint, startPoint - endPoint) / d);
					float closest = endPoint + lambda * (startPoint - endPoint);
					float distanceToSegment = length(p - closest);

					if (distanceToSegment < pathDistance)
					{
						//pathDistance = distanceToSegment;
						//pathLength = lambda * d;



						pathLength = 1;
					}

					distanceToSegment = length(p - startPoint);
					pathDistance = min(pathDistance, distanceToSegment - (int)distanceToSegment);
				}

				startPoint = endPoint;
				currentLength += d;
			}*/

			//float3 p =  mul(unity_ObjectToWorld, v.vertex);
			float3 p = _PathTrajectories[16 * i + j];
			if (length(p - vertex) < _PathWidth)
			{
				pathDistance = 1;
			}

			//o.vertex = float4(startPoint, 1);

		}

	//o.vertex = float4(2 * (tex2Dlod(_PathTrajectory, float4(step.x * 2, step.y * 2, 0, 0)).xyz * 2 - 1), 1);

	//vertexOutput o;
	o.vertex = v.vertex;
	o.normal = v.normal;
	o.tangent = v.tangent;
	o.path = float2(pathDistance, 0);
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
	MY_DOMAIN_PROGRAM_INTERPOLATE(path)

	return tessVert(v);
}