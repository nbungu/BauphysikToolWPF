#version 430 core

in vec4 fColor;       // from layer vertex
in vec2 vTexCoord;    // from layer vertex
in vec2 vDashParams;
in vec2 vFragPos;
in float vLineDistance;

uniform sampler2D texture0;
uniform int useHatchPattern;
uniform float hatchScale;

out vec4 outputColor;

void main()
{   
    vec4 baseColor = fColor;

    // Check if we're rendering a dashed line
    if (vDashParams.x > 0.0)
    {
        float dashLength = vDashParams.x;
        float gapLength = vDashParams.y;
        float total = dashLength + gapLength;
        float dashPos = mod(vLineDistance, total);
        if (dashPos > dashLength)
        {
            discard; // inside the gap
        }
    }
    
    if (useHatchPattern == 1) {
        vec2 tiledCoord = vTexCoord * hatchScale;
        vec4 hatch = texture(texture0, tiledCoord);
        // overlay blend
        outputColor = mix(baseColor, hatch, hatch.a);
    } else {
        outputColor = baseColor;
    }
}