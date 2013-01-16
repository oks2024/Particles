float4x4 World;
float4x4 View;
float4x4 Projection;

// TODO: add effect parameters here.

texture physicsMap;
texture image;

bool isMonochrome = true;

float particleSize = 1.0f;

sampler imageSampler = sampler_state
{
    texture = (image);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    MipFilter = NONE;
};

sampler physicsSampler = sampler_state
{
    texture = (physicsMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    MipFilter = NONE;
};

struct VertexShaderInput
{
    float4 vertexData : POSITION;
    float4 color : COLOR0;
};

struct VertexShaderOutput
{
    float4 position : POSITION0;
    float4 color : COLOR0;
    float2 texCoord : COLOR1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4x4 worldView= mul(World, View);
    float4x4 WorldViewProj=mul(worldView, Projection);

    float4 realPosition = tex2Dlod ( physicsSampler, float4(input.vertexData.x/* + 0.5f/1000.0f*/, input.vertexData.y/* + 0.5f/1000.0f*/,0,0));
    float4 velocityColor = float4(-0.25f + clamp(length(float2(realPosition.z, realPosition.w))/400.0f, 0.0f, 1.0f), 0.20f, 0.25f-clamp(length(float2(realPosition.z, realPosition.w))/400.0f, 0.0f, 1.0f), 0.0f) * (isMonochrome == false);
    realPosition = float4(realPosition.x, realPosition.y, 0.0, 1.0);

    

    output.color = input.color + velocityColor;

    if ( input.vertexData.z == 0 )
        realPosition.xy += float2(particleSize,-particleSize);
    if ( input.vertexData.z == 1 )
        realPosition.xy += float2(particleSize,particleSize);
    if ( input.vertexData.z == 2 )
        realPosition.xy += float2(-particleSize,particleSize);
    if ( input.vertexData.z == 3 )
        realPosition.xy += float2(-particleSize,-particleSize);
/*
    if ( input.vertexData.z == 0 )
        realPosition.xy = float2(300,250);
    if ( input.vertexData.z == 1 )
        realPosition.xy = float2(300,300);
    if ( input.vertexData.z == 2 )
        realPosition.xy = float2(250,300);
    if ( input.vertexData.z == 3 )
        realPosition.xy = float2(250,250);*/

    output.texCoord = float2(realPosition.x/640.0f, 1.0f - realPosition.y/800.0f);
    output.position = mul(realPosition, WorldViewProj);
    //output.position = float4(realPosition.xy, 0.0f, 1.0f);
    return output;
}

float4 PixelShaderColor(VertexShaderOutput input) : COLOR0
{
    return input.color;
}

float4 PixelShaderTexture(VertexShaderOutput input) : COLOR0
{
    float4 imageColor = tex2D(imageSampler, input.texCoord);
    return float4(imageColor.rgb + float3(0.02f, 0.02f, 0.10f), 0.30f);
}

technique colorParticles
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderColor();
    }
}

technique textureParticles
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderTexture();
    }
}
