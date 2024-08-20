#ifndef ONSCALETILE
#define ONSCALETILE
StructuredBuffer<float2> scalePositions;
int numScalePositions = 0;

// sampler2D _scalePositionsTexture;
Texture2D _scalePositionsTexture;
// SamplerState SamplerState_Point_Clamp;

void isOnScaleTile_float(float2 position, out float result)
{
    result = 0.0f;

    [loop]
    for (int i = 0; i < numScalePositions; ++i)
    {
        // Calculate the texture coordinate to sample (assuming the texture is 1D)
        // float2 texCoord = float2(i / float(numScalePositions), 0.5f); // y = 0.5f for middle of the 1D texture

        // Sample the texture to get the scale position
        float2 scalePos = _scalePositionsTexture.Load(int3(i,0,0)).rg;

        if (distance(position, scalePos) <= 0.01f)
        {
            result = 1.0f;
            return;
        }
    }
}

void test_float(out float result)
{
    result = numScalePositions;
}
#endif