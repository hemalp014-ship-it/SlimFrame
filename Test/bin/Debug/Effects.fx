
cbuffer cbPerObj : register(b0)
{
    Matrix Proj;
    Matrix View;
    Matrix World;
    float3 camPos;
    float pad;
};

struct Light
{
    float3 pos;
    float range;
    float3 dir;
    float cone;
    float3 att;
    int type;
    float4 ambient;
    float4 diffuse;
};

cbuffer cbPerFrame : register(b2)
{
    Light light[4];
};

cbuffer Material : register(b1)
{
    float4 Diffuse;
    float4 Ambient;
    float4 Specular;
    float4 Emissive;
    int TextureType;
    int BumpType;
    float Transparency;
    float Reflectivity;
    float Refractivity;
    float SpecularPower;
    float EmissivePower;
    float Dissolve;
};

Texture2D tex;
TextureCube texCube;
Texture2D Bump;
TextureCube BumpCube;
TextureCube SkyMap;

SamplerState ObjSamplerState;

//--------------------------------------------------------------------------------------
struct VS_INPUT
{
    float3 Pos : POSITION;
    float3 Normal : NORMAL;
    float2 TexCoord: TEXCOORD;
};

struct PS_INPUT
{
    float4 Pos : SV_POSITION;
    float3 WorldPos : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord: TEXCOORD0;
};

PS_INPUT Vertex_Shader(VS_INPUT input)
{
    PS_INPUT output = (PS_INPUT)0;

    output.Pos = float4(input.Pos, 1.0f);
    output.Pos = mul(output.Pos, World);
    output.Pos = mul(output.Pos, View);
    output.Pos = mul(output.Pos, Proj);
    
    output.WorldPos = mul(input.Pos,(float3x4) World);
    
    output.Normal = mul(input.Normal,(float3x4)  World);
    output.TexCoord = input.TexCoord;

    return output;
}

PS_INPUT Vertex_Shader_Sky(VS_INPUT input)
{
    PS_INPUT output = (PS_INPUT)0;

    //output.Pos = mul(float4(input.Pos, 1.0f), WVP)
    
    output.Pos = float4(input.Pos, 1.0f);
    output.Pos = mul(output.Pos, World);
    output.Pos = mul(output.Pos, View);
    output.Pos = mul(output.Pos, Proj).xyww;
    
    output.WorldPos = mul(input.Pos, (float3x4) World);
    output.TexCoord = input.TexCoord;
    output.Normal = mul(input.Normal,(float3x4) World);

    return output;
}

float4 Pixel_Shader(PS_INPUT input) : SV_TARGET
{
    float4 texColor;
    float4 normalColor;
    float3 normal;
    float transparency = Transparency;
        
    //pos , normal remain same
    
    normal = normalize(input.Normal);

    if (TextureType == 0)
        texColor = tex.Sample(ObjSamplerState, input.TexCoord);
    else if (TextureType == 1)
        texColor = texCube.Sample(ObjSamplerState, input.WorldPos);
    else
        texColor = Diffuse;
    
    float component = saturate(dot(normal, normalize(input.WorldPos - camPos)));
    
    //if (transparency > 0.0f)
    //{
    //    texColor = Specular * pow(component, SpecularPower); // + light.ambient * (1.0f - component);
    //    return float4(texColor.rgb, 1 - transparency);
    //}
    //else
    //{
    if (Specular.r + Specular.g + Specular.b > 0)
        texColor += Specular * pow(component, SpecularPower);
    //}
    
    float3 dir = normalize(camPos - input.WorldPos);
    
    // good reflect
    //texColor += SkyMap.Sample(ObjSamplerState, -reflect(dir, normal)) * Reflectivity;
    //// good refract
    //// Reflectivity = 0, transparency = 1
    //texColor += SkyMap.Sample(ObjSamplerState, -refract(dir, normal, 1.0f / Refractivity));// * (transparency) * (1 - Reflectivity);

    //texColor.a = 1.0f;
    //return texColor;
    
    float4 outColor = float4(0, 0, 0, 0);
    for (int i = 0; i < 4; i++)
    {
        
        //Create the vector between light position and pixels position
        float3 lightToPixelVec = light[i].pos - input.WorldPos;
        //Find the distance between the light pos and pixel pos
        float d = length(lightToPixelVec);
        lightToPixelVec /= d;
    
        float lighting;
    
        switch (light[i].type)
        {
            case 0:
        
                if (BumpType >= 0)
                {
                    if (BumpType == 0)
                        normalColor = Bump.Sample(ObjSamplerState, input.TexCoord);
                    else if (BumpType == 1)
                        normalColor = BumpCube.Sample(ObjSamplerState, input.WorldPos);
        
                    float3 tempNormal = normal;
                
                    tempNormal.x = 2 * normalColor.r - 1;
                    tempNormal.y = 2 * normalColor.g - 1;
                    tempNormal.z = 2 * normalColor.b - 1;
        
                    normal = tempNormal.z * input.Normal;
                }
                lighting = saturate(dot(-lightToPixelVec, normal));
    
                if (lighting > 0.0f)
                    lighting /= light[i].att[0] + (light[i].att[1] * d) + (light[i].att[2] * (d * d));
    
    //If pixel is too far, return pixel color with ambient light
                if (d > light[i].range)
                {
                    lighting = 0.0f;
                }
                break;
            case 1:
                break;
            case 2:
        
                if (BumpType >= 0)
                {
                    if (BumpType == 0)
                        normalColor = Bump.Sample(ObjSamplerState, input.TexCoord);
                    else if (BumpType == 1)
                        normalColor = BumpCube.Sample(ObjSamplerState, input.WorldPos);
        
                    float3 tempNormal = normal;
                
                    tempNormal.x = 2 * normalColor.r - 1;
                    tempNormal.y = 2 * normalColor.g - 1;
                    tempNormal.z = 2 * normalColor.b - 1;
        
                    normal = tempNormal.z * input.Normal;
                }
                lighting = saturate(dot(light[i].dir, normal));
			
                break;
        }
    
    //if (length(Diffuse) == 0)
    //{
    //    if (length(texColor) == 0)
    //        outColor = saturate((lighting * light.diffuse + light.ambient));
    //    else
    //        outColor = saturate(texColor * (lighting * light.diffuse + light.ambient));
    //}
    //else
    //{
        outColor += saturate(texColor * (lighting * light[i].diffuse + light[i].ambient));
    //}
    }
    
    outColor.w = saturate(1.0f - transparency);
    return outColor;
}

float4 Pixel_Shader_Sky(PS_INPUT input) : SV_TARGET
{
    if (TextureType == 0)
        return saturate(tex.Sample(ObjSamplerState, input.TexCoord) * Diffuse + Ambient);
    else
        return saturate(texCube.Sample(ObjSamplerState, input.WorldPos));
}