float4x4 World;
float4x4 View;
float4x4 Projection;


texture particles;

sampler particleSampler = sampler_state
{
    texture = (particles);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
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
    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = float4(input.Position, 1);
    output.TexCoord = input.TexCoord;// - float2(0.5f/1280.0f, 0.5f/800.0f);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    int nbSample = 3;
    float4 finalColor = float4(0.0f, 0.0f, 0.0f, 0.0f);
    float nbNonBlackPixels = 0;
    for (int i = 0; i < nbSample; i++)
    {
        for (int j = 0; j < nbSample; j++)
        {
            finalColor += tex2D(particleSampler, input.TexCoord + float2((i - 1) / 1280.0f, (j - 1) /800.0f)).rgba;
            nbNonBlackPixels +=  tex2D(particleSampler, input.TexCoord + float2((i - 1) / 1280.0f, (j - 1)/800.0f)).a;
        }
    }    
    
    
    //return float4(finalColor.r/100.0f, finalColor.g/100.0f, (finalColor.b/100.0f)%3, finalColor.a);//float4( 0.00f, 0.00f, 1.0f * (25.0f/nbNonBlackPixels), 1.0f);
    return saturate(saturate((finalColor/3.0f)) * (nbNonBlackPixels >= 2.9f) + float4(0.30f, 0.30f, 0.95f, 0.10f) * (nbNonBlackPixels <= 0.1f));
    //return float4( 1.00f, 1.00f, 1.00f, /*1.0f */ nbNonBlackPixels/25.0f );//finalColor/10 + float4(0.0f, 0.0f, 0.0f, 1.0f);
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
