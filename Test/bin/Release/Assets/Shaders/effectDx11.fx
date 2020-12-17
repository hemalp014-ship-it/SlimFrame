Texture2D g_textOverlay;

SamplerState g_samLinear
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

// ------------------------------------------------------
// A shader that accepts Position and Texture
// Used as a text overlay
// ------------------------------------------------------

struct TextVS_IN
{
	float4 pos : POSITION;
	float2 tex : TEXCOORD0;
};

struct TextPS_IN
{
	float4 pos : SV_POSITION;
	float2 tex : TEXCOORD0;
};

TextPS_IN TextVS(TextVS_IN input)
{
	TextPS_IN output = (TextPS_IN)0;
	output.pos = input.pos;
	output.tex = input.tex;
	return output;
}

float4 TextPS(TextPS_IN input) : SV_Target
{
	float4 color = g_textOverlay.Sample(g_samLinear, input.tex);
	return color;
}

// ------------------------------------------------------
// Techniques
// ------------------------------------------------------

technique11 Text
{
	pass P0
	{
		SetGeometryShader(0);
		SetVertexShader(CompileShader(vs_4_0, TextVS()));
		SetPixelShader(CompileShader(ps_4_0, TextPS()));
	}
}