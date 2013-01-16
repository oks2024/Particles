float4x4 World;
float4x4 View;
float4x4 Projection;


texture depthMap;

sampler depthSampler = sampler_state
{
	texture = (depthMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	MipFilter = NONE;
};
// TODO: add effect parameters here.

struct VertexShaderInput
{
    float3 Position : POSITION0;

	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = float4(input.Position, 1);
	output.TexCoord = input.TexCoord - float2(0.5f/1280.0f, 0.5f/800.0f);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 finalColor = float4(0.0f, 0.0f, 0.0f, 1.0f);

	float4 depth = tex2D(depthSampler, input.TexCoord);
	/*if ( depth[13] == 1.0f)
	{
		finalColor = float4(0.0f, 0.0f, 0.0f, 1.0f);
	}
	else
	{
		finalColor = float4(1.0f, 1.0f, 1.0f, 1.0f);
	}*/
    return finalColor;
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
