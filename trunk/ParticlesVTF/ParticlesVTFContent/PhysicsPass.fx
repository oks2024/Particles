float width = 1.0f;
float height = 800.0f/1280.0f;
float currentTime = 0;

float2 mousePosition = float2(640, 400);
//float2 gravity = float2(0.0f, 0.0f);//float2(0.0f, -100.0f);
float topSpeed = 400.0f;
float attractionPower = 10.0f;
float isAttracting = 1.0f;
float textureSize = 1024.0f;
float4 GravityFlow;

bool randomBounce = false;
texture physicsMap;
texture temporaryMap;
texture randomMap;
texture backgroundMap;
texture flowMap;

float elapsedTime;

sampler physicsSampler = sampler_state
{
    texture = (physicsMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    MipFilter = NONE;
};

sampler temporarySampler = sampler_state
{
    texture = (temporaryMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    MipFilter = NONE;
};

sampler randomSampler = sampler_state
{
    texture = (randomMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    MipFilter = NONE;
};

sampler backgroundSampler = sampler_state
{
    texture = (backgroundMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    MipFilter = NONE;
};

sampler flowMapSampler = sampler_state
{
    texture = (flowMap);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = POINT;
    MinFilter = POINT;
    MipFilter = NONE;
};

/*
struct VertexShaderInput
{
    float3 Position : POSITION0;
    float4 PhysicsValues : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 PhysicsValues : COLOR0;
};*/

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

struct PixelShaderOutput
{
    float4 Color : COLOR0;
};



VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = float4(input.Position, 1);
    output.TexCoord = input.TexCoord;// + 0.5f/textureSize;

    return output;
}

//////// RESET POSITIONS AND VELOCITY /////////////////

float2 generateNewPosition(float2 uv)
{
        float4 rand = tex2D(randomSampler, uv);
        return float2(rand.x*width,rand.y*height);
}

float4 ResetPhysicsPS(VertexShaderOutput input) : COLOR
{
    
    return float4(generateNewPosition(input.TexCoord), 0.0, 0.0);
}

////////////// COPY TO TEMPORARY RENDER TARGET ///////////////

float4 CopyTexturePS(VertexShaderOutput input) : COLOR
{
    return tex2D(temporarySampler, input.TexCoord);
}

///////////// UPDATE FUNCTIONS /////////////

float4 UpdatePositionsPS(VertexShaderOutput input): COLOR
{
    float2 gravity = GravityFlow.xy;
    float4 physicsValues = tex2D(physicsSampler, input.TexCoord);
    float2 position = physicsValues.xy;
    
    float2 velocity = physicsValues.zw;
    
    float4 rand = tex2D(randomSampler, input.TexCoord);
    
    position = position + elapsedTime * velocity;
    
    float4 backgroundColor = tex2D(backgroundSampler, float2(position.x/width, 1.0f - position.y/height));

    velocity += gravity * elapsedTime;
    
    float2 flow = tex2D(flowMapSampler, float2(position.x/width, position.y/height)).rg;
    flow.rg = (flow.rg - 0.5f) * 2.0f; 
    flow.rg *= GravityFlow.z;
    velocity += flow;

    float2 attractionVector = mousePosition - position;
    //if ( length(attractionVector) <100.0f)
    //velocity += isAttracting * (normalize(mousePosition - position) * attractionPower) * ( length(attractionVector) <400.0f);
    velocity += isAttracting * (normalize(mousePosition - position) * lerp(0.0f, 2.0f, (800.0f - length(attractionVector))/40)* ( length(attractionVector) <800.0f));	

    //float2 mousePosition2 = float2(1280.0f - mousePosition.x, mousePosition.y);
    //float2 attractionVector2 = mousePosition2 - position;
    //velocity += isAttracting * (normalize(mousePosition2 - position) * attractionPower) * ( length(attractionVector2) <400.0f);
    //velocity += isAttracting * (normalize(mousePosition2 - position) * lerp(0.0f, 2.0f, (800.0f - length(attractionVector2))/80)* ( length(attractionVector2) <800.0f));

    float bounceAttenuation = 10.0f;

    if ((backgroundColor.a != 0.0f) && (backgroundColor.r <= 0.5f))
    {
        velocity = -velocity * 0.7f;
    }

    if (position.x < 0.0f)
    {	
        velocity = float2(abs(velocity.x) * (0.9f+rand.z * (randomBounce == true)), velocity.y);
        //position.x = 1.0f;
    }

    if (position.x > width)
    {	
        velocity = float2(-abs(velocity.x) * (0.9f+rand.z * (randomBounce == true)), velocity.y);
        //position.x = width - 1.0f;
    }

    if (position.y < 0.0f)
    {
        velocity = float2(velocity.x, abs(velocity.y) * (0.5f+rand.z * (randomBounce == true))) + float2(0.0f, 20.0f + 20.0f * cos(4.0f * 3.14f * ((position.x + currentTime)/width))) ;//(position.x) * currentTime/position.x));
        //position.x += position.y;
    }

    if (position.y > height)
    {
        velocity = float2(velocity.x, -abs(velocity.y)* (0.8f+rand.z * (randomBounce == true)));
        //position.y = height - 1.0f;
    }

    
    if ( length(velocity) > topSpeed)
        velocity = normalize(velocity) * topSpeed;

    velocity = normalize(velocity) * length(velocity) * GravityFlow.w;
    return float4(position, velocity);
}

technique ResetPositions
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 ResetPhysicsPS();
    }
}

technique CopyTexture
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 CopyTexturePS();
    }
}

technique UpdatePhysics
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 UpdatePositionsPS();
    }
}